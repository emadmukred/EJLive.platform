using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Client.Service.Compatibility;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using AppConfig = EJLive.Core.Models.AppConfig;

namespace EJLive.Client.Service;

/// <summary>
/// Production-safe headless controller for the EJLive ATM client agent.
/// It preserves old runtime behavior through adapters while adding stronger
/// service orchestration, heartbeat, reconnect backoff, safe live-file reads,
/// and strict blocking of sensitive commands inside the headless service path.
/// </summary>
public sealed class AgentHeadlessController : IAgentController
{
    private readonly AppConfig _config;
    private readonly JournalOutboxAdapter _outbox;
    private readonly SimpleJournalFileWatcher _fileWatcher;
    private readonly ConcurrentQueue<object> _pendingTelemetry = new();
    private readonly object _stateLock = new();
    private readonly BackoffPolicy _reconnectBackoff = new();
    private readonly SystemBackupService _systemBackupService;

    private NetworkEngine? _network;
    private TimeSyncScheduler? _timeSyncScheduler;
    private LogBackupScheduler? _logBackupScheduler;
    private System.Threading.Timer? _heartbeatTimer;
    private System.Threading.Timer? _reconnectTimer;
    private CancellationTokenSource _cts = new();

    private AgentControllerState _state = AgentControllerState.Stopped;
    private DateTime _lastHeartbeatUtc = DateTime.MinValue;
    private DateTime _lastJournalSyncUtc = DateTime.MinValue;
    private string? _lastError;
    private string? _sessionId;
    private int _heartbeatBusy;
    private int _reconnectBusy;
    private int _systemBackupRunning;
    private long _lastSystemBackupUtcTicks;

    public event Action<string>? OnLog;
    public event Action<AgentStatus>? OnStatusUpdate;

    public string AtmId => FirstNonBlank(_config.ATM_ID, Environment.MachineName, "UNKNOWN");

    public AgentHeadlessController(
        AppConfig? config = null,
        RuntimeAgentConfigOverrides? overrides = null,
        RuntimeAgentConfigResolver? resolver = null,
        bool preferEnvironment = true,
        SystemBackupService? systemBackupService = null)
    {
        resolver ??= new RuntimeAgentConfigResolver();
        _config = ResolveConfiguration(
            config ?? AppConfig.Load(),
            overrides,
            resolver,
            preferEnvironment);

        var rawOutbox = CreateJournalOutbox(_config);
        _outbox = new JournalOutboxAdapter(rawOutbox);

        _fileWatcher = new SimpleJournalFileWatcher();
        _fileWatcher.FileChanged += path => ProcessChangedJournalFile(path);

        _systemBackupService = systemBackupService ?? new SystemBackupService();
        _systemBackupService.OnLog += HandleSystemBackupLog;
    }

    public void StartAll()
    {
        lock (_stateLock)
        {
            if (_state != AgentControllerState.Stopped && _state != AgentControllerState.Failed)
                return;

            _state = AgentControllerState.Starting;
        }

        EmitStatus();
        Log($"[AgentHeadless] Starting ATM={AtmId} Type={FirstNonBlank(_config.ATM_Type, "Unknown")}");

        try
        {
            _cts = new CancellationTokenSource();

            InitializeNetwork();
            InitializeFileWatcher();
            InitializeSchedulers();
            StartHeartbeat();
            StartReconnectTimer();

            lock (_stateLock)
            {
                _state = AgentControllerState.Running;
            }

            Log("[AgentHeadless] All subsystems started.");
            EmitStatus();
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;

            lock (_stateLock)
            {
                _state = AgentControllerState.Failed;
            }

            Log($"[AgentHeadless] Start failed: {ex}");
            EmitStatus();
            throw;
        }
    }

    public void StopAll()
    {
        lock (_stateLock)
        {
            if (_state == AgentControllerState.Stopped)
                return;

            _state = AgentControllerState.Paused;
        }

        Log("[AgentHeadless] Stopping subsystems.");

        try { _cts.Cancel(); } catch { }

        _heartbeatTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _reconnectTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        StopSchedulers();
        try { _fileWatcher.Stop(); } catch (Exception ex) { Log($"[FileWatcher] Stop warning: {ex.Message}"); }
        try { _network?.Disconnect(); } catch (Exception ex) { Log($"[Network] Disconnect warning: {ex.Message}"); }

        _heartbeatTimer?.Dispose();
        _reconnectTimer?.Dispose();
        _heartbeatTimer = null;
        _reconnectTimer = null;

        lock (_stateLock)
        {
            _state = AgentControllerState.Stopped;
        }

        Log("[AgentHeadless] Stopped.");
        EmitStatus();
    }

    public AgentStatus GetStatus()
    {
        var network = _network;

        lock (_stateLock)
        {
            return new AgentStatus(
                State: _state,
                Connected: network?.IsConnected ?? false,
                HandshakeComplete: !string.IsNullOrWhiteSpace(_sessionId),
                PendingOutboxItems: _outbox.PendingCount,
                TotalBytesSent: ReadNetworkCounter(network, "TotalBytesSent", 0),
                TotalBytesReceived: ReadNetworkCounter(network, "TotalBytesReceived", 0),
                LastHeartbeatUtc: _lastHeartbeatUtc == DateTime.MinValue ? null : _lastHeartbeatUtc,
                LastJournalSyncUtc: _lastJournalSyncUtc == DateTime.MinValue ? null : _lastJournalSyncUtc,
                SessionId: _sessionId,
                LastError: _lastError);
        }
    }

    public void ForceJournalSync()
    {
        Log("[AgentHeadless] ForceJournalSync requested.");

        try
        {
            _outbox.EnqueuePendingForImmediateDispatch();
            _outbox.EnqueueForceSyncMarker(AtmId);
            _lastJournalSyncUtc = DateTime.UtcNow;
            EmitStatus();
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            Log($"[AgentHeadless] ForceJournalSync failed: {ex.Message}");
            EmitStatus();
        }
    }

    public void ForceLogBackup()
    {
        Log("[AgentHeadless] ForceLogBackup requested.");
        RequestSystemBackup("manual request");
        _logBackupScheduler?.RunNow();

        Task.Run(() =>
        {
            try
            {
                var root = FirstNonBlank(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Client"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive"));

                var archiveDir = Path.Combine(root, "Archive", DateTime.UtcNow.ToString("yyyy-MM"));
                Directory.CreateDirectory(archiveDir);

                foreach (var source in CandidateLogFiles(root))
                {
                    if (!File.Exists(source))
                        continue;

                    var destination = Path.Combine(
                        archiveDir,
                        $"{Path.GetFileNameWithoutExtension(source)}-{DateTime.UtcNow:yyyyMMdd-HHmmss}{Path.GetExtension(source)}");

                    File.Copy(source, destination, overwrite: true);
                    Log($"[AgentHeadless] Log archived: {destination}");
                }
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Log($"[AgentHeadless] ForceLogBackup failed: {ex.Message}");
            }
        });
    }

    public void Dispose()
    {
        StopAll();
        _systemBackupService.OnLog -= HandleSystemBackupLog;
        _fileWatcher.Dispose();
        _outbox.Dispose();
        _cts.Dispose();
    }

    private static AppConfig ResolveConfiguration(
        AppConfig source,
        RuntimeAgentConfigOverrides? overrides,
        RuntimeAgentConfigResolver resolver,
        bool preferEnvironment)
    {
        source.ApplyDefaults();
        overrides ??= new RuntimeAgentConfigOverrides();

        if (!resolver.TryResolve(
                source,
                overrides,
                out var runtimeConfig,
                out var reason,
                preferEnvironment))
        {
            throw new InvalidOperationException("Invalid headless agent configuration: " + reason);
        }

        resolver.ApplyTo(source, runtimeConfig);
        source.ApplyDefaults();
        return source;
    }

    private static object CreateJournalOutbox(AppConfig config)
    {
        var outboxType = typeof(JournalOutbox);
        var defaultOutboxPath = FirstNonBlank(
            AppConstants.DefaultClientOutboxPath,
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Outbox"));

        foreach (var ctor in outboxType.GetConstructors())
        {
            var parameters = ctor.GetParameters();

            try
            {
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    return ctor.Invoke(new object[] { defaultOutboxPath });

                if (parameters.Length == 2 &&
                    parameters[0].ParameterType == typeof(string) &&
                    parameters[1].ParameterType == typeof(string))
                {
                    var source = FirstNonBlank(config.SourcePath, AppConstants.GetDefaultSourcePath(config.ATM_Type), defaultOutboxPath);
                    var backup = FirstNonBlank(config.BackupPath, Path.Combine(defaultOutboxPath, "backup"));
                    return ctor.Invoke(new object[] { source, backup });
                }
            }
            catch
            {
                // Try next constructor.
            }
        }

        // Final attempt: parameterless constructor if available.
        var empty = outboxType.GetConstructor(Type.EmptyTypes);
        if (empty != null)
            return empty.Invoke(null);

        throw new InvalidOperationException("Unable to construct JournalOutbox using known compatible constructors.");
    }

    private void InitializeNetwork()
    {
        var transport = NetworkTransportOptions.FromEnvironment();
        _network = new NetworkEngine(
            FirstNonBlank(_config.ServerIP, "127.0.0.1"),
            _config.ServerPort,
            AtmId,
            FirstNonBlank(_config.ATM_Type, "Unknown"),
            FirstNonBlank(_config.NetworkType, "LAN"),
            (JournalOutbox)_outbox.Inner,
            transport);

        _network.OnConnectionChanged += (_, connected) =>
        {
            Log($"[Network] Connection changed: connected={connected}");

            if (connected)
            {
                _lastError = null;
                _reconnectBackoff.Reset();
            }
            else
            {
                _sessionId = null;
            }

            EmitStatus();
        };

        _network.OnSessionEstablished += (_, session) =>
        {
            _sessionId = session;
            Log($"[Network] Session established: {session}");
            EmitStatus();
        };

        _network.OnMessageReceived += (_, message) => HandleServerMessage(message);

        _network.OnJournalAcknowledged += (_, ack) =>
        {
            _lastJournalSyncUtc = DateTime.UtcNow;
            Log($"[Network] Journal ACK: {ack}");
            EmitStatus();
        };

        TryConnectNetwork();
    }

    private void InitializeFileWatcher()
    {
        foreach (var root in GetConfiguredWatchRoots().Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                continue;

            _fileWatcher.AddWatchRoot(root, "*.LOG");
            _fileWatcher.AddWatchRoot(root, "*.TXT");
            _fileWatcher.AddWatchRoot(root, "*.DAT");

            Log($"[FileWatcher] Watching {root}");
        }

        _fileWatcher.Start();
    }

    private void StartHeartbeat()
    {
        var interval = TimeSpan.FromSeconds(
            _config.HeartbeatIntervalSec > 0
                ? _config.HeartbeatIntervalSec
                : AppConstants.HeartbeatIntervalSec);

        _heartbeatTimer = new System.Threading.Timer(
            _ => SendHeartbeatSafely(),
            null,
            TimeSpan.FromSeconds(10),
            interval);
    }

    private void StartReconnectTimer()
    {
        _reconnectTimer = new System.Threading.Timer(
            _ => AttemptReconnectIfNeeded(),
            null,
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(15));
    }

    private void InitializeSchedulers()
    {
        StopSchedulers();

        if (_config.SyncTimeEnabled)
        {
            _timeSyncScheduler = new TimeSyncScheduler(_network, AtmId);
            _timeSyncScheduler.OnLog += message => Log("[TimeSync] " + message);
            _timeSyncScheduler.Start();
        }

        if (_config.AutoBackup &&
            !string.IsNullOrWhiteSpace(_config.SourcePath) &&
            !string.IsNullOrWhiteSpace(_config.BackupPath))
        {
            _logBackupScheduler = new LogBackupScheduler(
                _network,
                AtmId,
                _config.SourcePath,
                _config.BackupPath,
                _config.ATM_Type);
            _logBackupScheduler.OnLog += message => Log("[Backup] " + message);
            _logBackupScheduler.OnBackupCreated += _ => RequestSystemBackup("scheduled journal archive");
            _logBackupScheduler.Start();
        }
    }

    private void RequestSystemBackup(string reason)
    {
        var lastCompletedTicks = Interlocked.Read(ref _lastSystemBackupUtcTicks);
        if (lastCompletedTicks > 0 &&
            DateTime.UtcNow - new DateTime(lastCompletedTicks, DateTimeKind.Utc) < TimeSpan.FromMinutes(5))
        {
            Log($"[SystemBackup] Skipped duplicate {reason}; a full backup completed recently.");
            return;
        }

        if (Interlocked.Exchange(ref _systemBackupRunning, 1) == 1)
        {
            Log($"[SystemBackup] Coalesced {reason}; a full backup is already running.");
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var result = await _systemBackupService
                    .CreateBackupAsync(cancellationToken: _cts.Token)
                    .ConfigureAwait(false);
                if (result.Success)
                {
                    Interlocked.Exchange(ref _lastSystemBackupUtcTicks, DateTime.UtcNow.Ticks);
                    Log(
                        $"[SystemBackup] {reason} completed: {result.BackupName}; " +
                        $"artifacts={result.ArtifactCount}; bytes={result.SourceBytes}.");
                }
                else
                {
                    _lastError = result.Message;
                    Log($"[SystemBackup] {reason} failed: {result.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                Log($"[SystemBackup] {reason} cancelled during service shutdown.");
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Log($"[SystemBackup] {reason} failed: {ex.Message}");
            }
            finally
            {
                Interlocked.Exchange(ref _systemBackupRunning, 0);
            }
        });
    }

    private void HandleSystemBackupLog(string message) => Log("[SystemBackup] " + message);

    private void StopSchedulers()
    {
        try { _timeSyncScheduler?.Dispose(); }
        catch (Exception ex) { Log($"[TimeSync] Stop warning: {ex.Message}"); }

        try { _logBackupScheduler?.Dispose(); }
        catch (Exception ex) { Log($"[Backup] Stop warning: {ex.Message}"); }

        _timeSyncScheduler = null;
        _logBackupScheduler = null;
    }

    private void TryConnectNetwork()
    {
        try
        {
            _network?.Connect();
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            Log($"[Network] Connect failed: {ex.Message}");
        }
    }

    private void ProcessChangedJournalFile(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return;

            var data = SafeLiveFileReader.ReadAllBytesShared(filePath);
            if (data.Length == 0)
                return;

            var checksum = SafeLiveFileReader.Sha256Hex(data);
            var fileName = Path.GetFileName(filePath);

            _outbox.EnqueueFile(AtmId, fileName, data, checksum);
            _lastJournalSyncUtc = DateTime.UtcNow;

            Log($"[FileWatcher] Enqueued {fileName} ({data.Length} bytes, sha256={checksum[..Math.Min(12, checksum.Length)]}...)");
            EmitStatus();
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            Log($"[FileWatcher] Error processing {filePath}: {ex.Message}");
            EmitStatus();
        }
    }

    private void HandleServerMessage(EJMessage message)
    {
        try
        {
            if (message.Text.StartsWith("TIME_SYNC_RESP|", StringComparison.OrdinalIgnoreCase))
            {
                _timeSyncScheduler?.HandleResponse(message.Text);
                return;
            }

            if (message.Type == CommunicationProtocol.MsgType.Command &&
                RemoteCommandEnvelope.TryParse(message.Text, out var command))
            {
                Log($"[Command] Received {command.CommandType} ({command.CommandId})");
                var result = ExecuteHeadlessCommand(command);
                _network?.SendMessage(
                    CommunicationProtocol.BuildCommandResult(command.CommandId, result.Success, result.Message));
                return;
            }

            if (message.Type == CommunicationProtocol.MsgType.Broadcast &&
                message.Text.StartsWith("PULSE", StringComparison.OrdinalIgnoreCase))
            {
                _lastHeartbeatUtc = DateTime.UtcNow;
                EmitStatus();
            }
        }
        catch (Exception ex)
        {
            _lastError = ex.Message;
            Log($"[MessageHandler] Error: {ex.Message}");
            EmitStatus();
        }
    }

    private CommandResult ExecuteHeadlessCommand(RemoteCommandEnvelope command)
    {
        var type = command.CommandType.ToUpperInvariant();

        // Sensitive actions are intentionally blocked here.
        // They must be executed only through SafeRemoteCommandExecutor after:
        // signed command + RBAC + policy + maintenance window + audit + rollback.
        if (type == AppConstants.CMD_RESTART.ToUpperInvariant() ||
            type == AppConstants.CMD_SHUTDOWN.ToUpperInvariant() ||
            type == AppConstants.CMD_CHANGE_PASSWORD.ToUpperInvariant() ||
            type == AppConstants.CMD_SCREENSHOT.ToUpperInvariant() ||
            type == AppConstants.CMD_REMOTE_SESSION_START.ToUpperInvariant() ||
            type == AppConstants.CMD_WINDOWS_REMOTE_START.ToUpperInvariant())
        {
            return new CommandResult(
                false,
                $"{command.CommandType} is blocked in AgentHeadlessController. Use SafeRemoteCommandExecutor policy path.");
        }

        if (type == AppConstants.CMD_SYNC_TIME.ToUpperInvariant())
            return new CommandResult(
                false,
                "Time synchronization is not available in the headless service baseline.");

        if (type == AppConstants.CMD_GET_STATS.ToUpperInvariant())
            return new CommandResult(true, JsonSerializer.Serialize(GetStatus()));

        if (type == AppConstants.CMD_PING.ToUpperInvariant())
            return new CommandResult(true, "pong");

        if (type == AppConstants.CMD_FORCE_SYNC.ToUpperInvariant())
            return ExecuteSafe(() =>
            {
                ForceJournalSync();
                return new CommandResult(true, "Force journal sync requested.");
            });

        return new CommandResult(false, $"Unknown or unsupported headless command type: {command.CommandType}");
    }

    private static CommandResult ExecuteSafe(Func<CommandResult> action)
    {
        try { return action(); }
        catch (Exception ex) { return new CommandResult(false, ex.Message); }
    }

    private void SendHeartbeatSafely()
    {
        if (Interlocked.Exchange(ref _heartbeatBusy, 1) == 1)
            return;

        try
        {
            SendHeartbeat();
        }
        finally
        {
            Interlocked.Exchange(ref _heartbeatBusy, 0);
        }
    }

    private void SendHeartbeat()
    {
        if (_network?.IsConnected != true)
            return;

        var status = GetStatus();
        var payload = JsonSerializer.Serialize(new
        {
            message = "heartbeat",
            atmId = AtmId,
            state = status.State.ToString(),
            connected = status.Connected,
            handshakeComplete = status.HandshakeComplete,
            pendingOutboxItems = status.PendingOutboxItems,
            sessionId = status.SessionId,
            totalBytesSent = status.TotalBytesSent,
            totalBytesReceived = status.TotalBytesReceived,
            timestampUtc = DateTime.UtcNow
        });

        _network.SendMessage(
            CommunicationProtocol.BuildFrame(
                CommunicationProtocol.MsgType.Broadcast,
                "PULSE_JSON|" + payload));

        _lastHeartbeatUtc = DateTime.UtcNow;
        EmitStatus();
    }

    private void AttemptReconnectIfNeeded()
    {
        if (_network?.IsConnected == true)
            return;

        if (Interlocked.Exchange(ref _reconnectBusy, 1) == 1)
            return;

        Task.Run(async () =>
        {
            try
            {
                var delay = _reconnectBackoff.NextDelay();
                Log($"[Network] Reconnect scheduled after {delay.TotalSeconds:N1}s.");
                await Task.Delay(delay, _cts.Token).ConfigureAwait(false);

                if (_cts.IsCancellationRequested || _network?.IsConnected == true)
                    return;

                Log("[Network] Attempting reconnect.");
                try { _network?.Disconnect(); } catch { }
                TryConnectNetwork();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                Log($"[Network] Reconnect failed: {ex.Message}");
                EmitStatus();
            }
            finally
            {
                Interlocked.Exchange(ref _reconnectBusy, 0);
            }
        }, _cts.Token);
    }

    private void EmitStatus()
    {
        try { OnStatusUpdate?.Invoke(GetStatus()); }
        catch { /* Observers must not crash the agent. */ }
    }

    private void Log(string message)
    {
        var formatted = $"[{DateTime.UtcNow:O}] {message}";
        try { OnLog?.Invoke(formatted); }
        catch { /* Observers must not crash the agent. */ }
    }

    private static long ReadNetworkCounter(NetworkEngine? network, string propertyName, long fallback)
    {
        if (network == null)
            return fallback;

        try
        {
            var property = network.GetType().GetProperty(propertyName);
            if (property?.GetValue(network) is long longValue)
                return longValue;

            if (property?.GetValue(network) is int intValue)
                return intValue;
        }
        catch
        {
        }

        return fallback;
    }

    private static string FirstNonBlank(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return string.Empty;
    }

    private static IEnumerable<string> CandidateLogFiles(string root)
    {
        yield return Path.Combine(root, "ejlive-client.log");
        yield return Path.Combine(root, "logs", "ejlive-client.log");
        yield return Path.Combine(root, "Agent", "ejlive-client.log");
    }

    private IEnumerable<string> GetConfiguredWatchRoots()
    {
        var configured = FirstNonBlank(_config.SourcePath, AppConstants.GetDefaultSourcePath(_config.ATM_Type), Environment.CurrentDirectory);
        foreach (var root in configured.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            yield return root;
    }

    private sealed record CommandResult(bool Success, string Message);

    /// <summary>
    /// Minimal internal watcher to avoid depending on shifting FileWatcherEngine APIs.
    /// It preserves the project goal: safely detect live journal changes in service mode.
    /// </summary>
    private sealed class SimpleJournalFileWatcher : IDisposable
    {
        private readonly List<FileSystemWatcher> _watchers = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastSeen = new(StringComparer.OrdinalIgnoreCase);

        public event Action<string>? FileChanged;

        public void AddWatchRoot(string root, string filter)
        {
            var watcher = new FileSystemWatcher(root, filter)
            {
                IncludeSubdirectories = false,
                EnableRaisingEvents = false,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
            };

            watcher.Created += (_, e) => EmitDebounced(e.FullPath);
            watcher.Changed += (_, e) => EmitDebounced(e.FullPath);
            watcher.Renamed += (_, e) => EmitDebounced(e.FullPath);

            _watchers.Add(watcher);
        }

        public void Start()
        {
            foreach (var watcher in _watchers)
                watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            foreach (var watcher in _watchers)
                watcher.EnableRaisingEvents = false;
        }

        private void EmitDebounced(string path)
        {
            var now = DateTime.UtcNow;
            var last = _lastSeen.GetOrAdd(path, DateTime.MinValue);

            if (now - last < TimeSpan.FromSeconds(1))
                return;

            _lastSeen[path] = now;
            FileChanged?.Invoke(path);
        }

        public void Dispose()
        {
            Stop();
            foreach (var watcher in _watchers)
                watcher.Dispose();

            _watchers.Clear();
        }
    }
}
