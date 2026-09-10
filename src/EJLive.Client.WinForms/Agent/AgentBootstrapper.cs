using EJLive.Client.WinForms.Services;
using EJLive.Client.WinForms.Supabase;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using AppConfig = EJLive.Shared.AppConfig;
using AppConstants = EJLive.Core.AppConstants;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace EJLive.Client.WinForms.Agent;

/// <summary>
/// Unified background agent orchestration for startup-safe, low-risk operation.
/// </summary>
public sealed class AgentBootstrapper : IDisposable
{
    private enum ReconnectCircuitState
    {
        Closed,
        Open,
        HalfOpen
    }

    private readonly AppConfig _config;
    private readonly JournalOutbox _outbox;
    private readonly RuntimeAgentConfigResolver _runtimeConfigResolver = new();
    private readonly WindowsPolicyEnforcer _policyEnforcer;
    private readonly FileWatcherEngine _fileWatcher = new();
    private readonly FileDeliveryConfirmationTracker _deliveryTracker = new();
    private readonly ConcurrentQueue<TelemetryEnvelope> _pendingTelemetry = new();
    private readonly List<WatchRoot> _watchRoots = new();
    private readonly ConcurrentQueue<QueuedCommand> _dispatchQueue = new();
    private readonly XfsLogAnalysisService _xfsAnalyzer = new();
    private readonly object _reconnectCircuitLock = new();

    private NetworkManager? _network;
    private JournalProcessor? _journalProcessor;
    private RemoteCommandHandler? _remoteCommands;
    private ScreenshotScheduler? _screenshot;
    private BootNotifier? _bootNotifier;
    private NetworkMonitor? _networkMonitor;
    private TimeSyncScheduler? _timeSync;
    private LogBackupScheduler? _logBackup;
    private SupabaseSync? _supabase;
    private System.Threading.Timer? _heartbeatTimer;
    private System.Threading.Timer? _reconnectTimer;
    private System.Threading.Timer? _windowsBaselineTimer;
    private DateTime _nextReconnectAttemptUtc = DateTime.MinValue;
    private DateTime _reconnectCircuitOpenUntilUtc = DateTime.MinValue;
    private DateTime _lastReconnectCircuitOpenLogUtc = DateTime.MinValue;
    private DateTime _lastCompanionProbeUtc = DateTime.MinValue;
    private bool _lastCompanionReachable;
    private string _lastCompanionProbeDetail = "n/a";
    private CashTelemetryService? _cashTelemetry;
    private CashTelemetrySnapshot? _lastCashSnapshot;
    private DateTime _lastCashTelemetryPublishUtc = DateTime.MinValue;
    private int _reconnectInProgress;
    private int _reconnectAttempts;
    private int _reconnectCircuitFailures;
    private int _reconnectCircuitOpenCycles;
    private int _handshakeMissCount;
    private int _dispatchInProgress;
    private long _dispatchSequence;
    private CancellationTokenSource? _dispatchCts;
    private Task? _dispatchLoop;
    private volatile bool _running;
    private ReconnectCircuitState _reconnectCircuitState = ReconnectCircuitState.Closed;

    private const int DispatchQueueCapacity = 400;
    private static readonly TimeSpan DispatchRetryBaseDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan DispatchQueueTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ReconnectCircuitBaseWindow = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan ReconnectCircuitMaxWindow = TimeSpan.FromMinutes(3);

    public event Action<string>? OnLog;
    public event Action<(bool connected, bool handshake, int pending)>? OnStatusUpdate;

    public AgentBootstrapper()
    {
        _config = AgentConfigurationXmlService.LoadAppConfig(AppConfig.Load());
        _config.ApplyDefaults();
        _outbox = new JournalOutbox();
        _policyEnforcer = new WindowsPolicyEnforcer(() => _config);

        AppLogger.Instance.Initialize(AppConstants.DefaultLogPath, "agent");
        AppLogger.Instance.OnLog += (_, entry) => Log(entry.FormattedForUI);
        DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath);
        AuditLogger.Initialize(Path.Combine(AppConstants.DefaultLogPath, "audit"));
    }

    public string AtmId => _config.ATM_ID;

    public void StartAll()
    {
        if (_running)
            return;

        _running = true;
        if (!TryApplyRuntimeNetworkOverrides(out var runtimeConfigReason))
        {
            _running = false;
            throw new InvalidOperationException("Agent runtime configuration is invalid: " + runtimeConfigReason);
        }

        try
        {
            var stateRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "Client",
                "State");
            Directory.CreateDirectory(stateRoot);
            _cashTelemetry = new CashTelemetryService(stateRoot);
        }
        catch (Exception ex)
        {
            Log("Cash telemetry service init warning: " + ex.Message);
        }

        Log($"Agent starting for {_config.ATM_ID} ({_config.ATM_Type})");
        ApplyProcessResourceBudget();
        InitializeSupabase();
        ApplyAdministrativeDefaults();

        _network = new NetworkManager(
            _config.ServerIP,
            _config.ServerPort,
            _config.ATM_ID,
            _config.ATM_Type,
            _config.NetworkType,
            _outbox,
            _config);
        _network.OnConnectionChanged += (_, connected) => OnConnectionChanged(connected);
        _network.OnMessageReceived += (_, message) => HandleNetworkMessage(message);
        _network.OnJournalAcknowledged += (_, ack) => HandleJournalAcknowledgement(ack);
        _network.OnLog += (_, message) => Log(message);

        _journalProcessor = new JournalProcessor(_network, _outbox);
        _journalProcessor.OnItemDispatched += (_, item) =>
        {
            _deliveryTracker.TryMarkSent(item.FileName, out var _);
            TrackOperationalEvent("file_sent", $"{item.FileName}|bytes={item.Data.Length};state=awaiting_ack");
            Log($"Outbox dispatched (awaiting ack): {item.FileName}");
        };
        _journalProcessor.OnItemCompleted += (_, item) =>
        {
            TrackOperationalEvent("file_confirmed", $"{item.FileName}|detail={item.LastAckDetail}");
            Log($"Outbox confirmed by ack: {item.FileName}");
        };
        _journalProcessor.OnItemFailed += (_, item) =>
        {
            TrackOperationalEvent("file_retry", $"{item.FileName}|retry={item.RetryCount}|detail={item.LastAckDetail}", "warning");
            Log($"Outbox retry: {item.FileName} ({item.RetryCount}) [{item.LastAckDetail}]");
        };
        _journalProcessor.Start();

        _remoteCommands = new RemoteCommandHandler(
            _config.ATM_ID,
            _network,
            forceSyncAction: () => _journalProcessor.ForceSendNow());
        _remoteCommands.OnLogMessage += Log;
        _remoteCommands.OnCommandExecuted += (_, command) =>
        {
            var status = command.Status.ToString();
            var commandType = string.IsNullOrWhiteSpace(command.CommandType) ? "UNKNOWN" : command.CommandType;
            TrackOperationalEvent("dispatcher_command_result", $"type={commandType};status={status}");
        };

        _dispatchCts = new CancellationTokenSource();
        _dispatchLoop = Task.Run(() => RunDispatchLoopAsync(_dispatchCts.Token), _dispatchCts.Token);

        _fileWatcher.FileChanged += (_, filePath) => HandleFileChanged(filePath);
        InitializeWatchRoots();
        _fileWatcher.Start(_watchRoots.Select(root => root.Path).ToArray());

        _bootNotifier = new BootNotifier(_network, _config.ATM_ID);
        _bootNotifier.SendBootNotification();
        TrackOperationalEvent("boot_notify", "Boot notification queued by agent startup.");

        _networkMonitor = new NetworkMonitor(_network, _config.ServerIP, _config.ATM_ID, OnNetworkMonitorEvent);
        _networkMonitor.OnLog += message => Log("[NetworkMonitor] " + message);
        _networkMonitor.Start();

        _timeSync = new TimeSyncScheduler(_network, _config.ATM_ID, _config.ServerIP);
        _timeSync.OnLog += message => Log("[TimeSync] " + message);
        _timeSync.Start();

        _logBackup = new LogBackupScheduler(_network, _config.ATM_ID, _config.SourcePath, _config.BackupPath, _config.ATM_Type);
        _logBackup.OnLog += message => Log("[Backup] " + message);
        _logBackup.OnBackupCreated += QueueBackupArtifact;
        _logBackup.Start();

        _screenshot = new ScreenshotScheduler(_network, _config.ATM_ID, intervalMin: 5, AppConstants.DefaultLogPath);
        _screenshot.OnLog += message => Log("[Screenshot] " + message);
        _screenshot.OnScreenshotCaptured += result =>
        {
            _supabase?.LogScreenshot(result.LocalPath, result.SizeBytes);
            QueueSecureScreenshotArtifact(result);
        };
        _screenshot.Start();

        var heartbeatIntervalSec = Math.Clamp(_config.HeartbeatIntervalSec, 5, 300);
        var reconnectIntervalSec = Math.Clamp(_config.ReconnectIntervalSec, 5, 300);
        _heartbeatTimer = new System.Threading.Timer(
            _ => PushHeartbeat(),
            null,
            TimeSpan.FromSeconds(Math.Min(heartbeatIntervalSec, 10)),
            TimeSpan.FromSeconds(heartbeatIntervalSec));
        _reconnectTimer = new System.Threading.Timer(
            _ => EnsureConnected(),
            null,
            TimeSpan.FromSeconds(Math.Min(reconnectIntervalSec, 15)),
            TimeSpan.FromSeconds(reconnectIntervalSec));

        if (_config.AutoPrepareWindowsRuntime)
        {
            var baselineIntervalMin = Math.Clamp(_config.WindowsBaselineRepairIntervalMin, 5, 720);
            _windowsBaselineTimer = new System.Threading.Timer(
                _ => RefreshWindowsBaseline(),
                null,
                TimeSpan.FromMinutes(Math.Min(baselineIntervalMin, 2)),
                TimeSpan.FromMinutes(baselineIntervalMin));
        }

        _ = Task.Run(EnsureConnected);
        PushHeartbeat();
        Log("Agent started.");
    }

    public void StopAll()
    {
        _running = false;
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
        _reconnectTimer?.Dispose();
        _reconnectTimer = null;
        _windowsBaselineTimer?.Dispose();
        _windowsBaselineTimer = null;

        _screenshot?.Stop();
        _timeSync?.Stop();
        _logBackup?.Stop();
        _networkMonitor?.Stop();
        _journalProcessor?.Stop();
        try { _dispatchCts?.Cancel(); } catch { }
        try { _dispatchLoop?.Wait(TimeSpan.FromSeconds(2)); } catch { }
        _dispatchLoop = null;
        _dispatchCts?.Dispose();
        _dispatchCts = null;
        _fileWatcher.Stop();
        _network?.Disconnect();
        _network?.Dispose();
        _supabase?.Stop();

        _screenshot = null;
        _timeSync = null;
        _logBackup = null;
        _networkMonitor = null;
        _journalProcessor = null;
        _network = null;
        _remoteCommands = null;
        _supabase = null;
        _cashTelemetry = null;
        _lastCashSnapshot = null;
        _lastCashTelemetryPublishUtc = DateTime.MinValue;
        _watchRoots.Clear();
        while (_dispatchQueue.TryDequeue(out _)) { }

        Log("Agent stopped.");
    }

    public void TakeScreenshotNow() => _screenshot?.TakeNow();
    public void BackupNow() => _logBackup?.RunNow();
    public void SyncNow() => _journalProcessor?.ForceSendNow();

    public (bool connected, bool handshake, int pending) GetStatus()
    {
        var connected = _network?.IsConnected == true;
        var handshake = connected && !string.IsNullOrWhiteSpace(_network?.SessionId);
        return (connected, handshake, _outbox.Count);
    }

    private bool TryApplyRuntimeNetworkOverrides(out string reason)
    {
        reason = string.Empty;
        if (!_runtimeConfigResolver.TryResolve(_config, out var runtimeConfig, out reason))
            return false;

        _runtimeConfigResolver.ApplyTo(_config, runtimeConfig);
        Log($"Runtime endpoint resolved: {runtimeConfig.ConnectionDisplay} ({runtimeConfig.AtmId}).");
        return true;
    }

    private void OnConnectionChanged(bool connected)
    {
        if (connected)
        {
            _reconnectAttempts = 0;
            _nextReconnectAttemptUtc = DateTime.MinValue;
            _handshakeMissCount = 0;
            _bootNotifier?.ResendIfPending();
            TrackOperationalEvent("network_connected", $"Connected to {_config.ServerIP}:{_config.ServerPort}");
            _supabase?.RegisterAgent(_config.ATM_Type, _config.ServerIP, _config.ServerPort);
            FlushTelemetryQueue();
            _journalProcessor?.ForceSendNow();
        }
        else
        {
            if (_nextReconnectAttemptUtc == DateTime.MinValue)
                _nextReconnectAttemptUtc = DateTime.UtcNow;
            TrackOperationalEvent("network_disconnected", $"Disconnected from {_config.ServerIP}:{_config.ServerPort}", "warning");
        }

        var status = GetStatus();
        OnStatusUpdate?.Invoke(status);
        Log(connected ? "Server connected." : "Server disconnected.");
    }

    private void HandleNetworkMessage(EJMessage message)
    {
        if (message.Type == CommunicationProtocol.MsgType.Command)
        {
            var bytes = message.Payload?.Length ?? 0;
            TrackOperationalEvent("listener_command_received", $"bytes={bytes}");
            EnqueueDispatchCommand(message);
            return;
        }

        if (message.Type == CommunicationProtocol.MsgType.Broadcast &&
            (message.Text?.StartsWith("TIME_SYNC_RESP", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            _timeSync?.HandleResponse(message.Text);
        }
    }

    private void EnqueueDispatchCommand(EJMessage message)
    {
        if (_remoteCommands == null)
            return;

        var descriptor = ResolveCommandDescriptor(message.Text);
        if (_dispatchQueue.Count >= DispatchQueueCapacity)
        {
            TrackOperationalEvent(
                "dispatcher_queue_full",
                $"commandId={descriptor.CommandId};commandType={descriptor.CommandType}",
                "warning");
            if (!string.IsNullOrWhiteSpace(descriptor.CommandId))
            {
                _network?.SendCommandResult(
                    descriptor.CommandId,
                    false,
                    $"Dispatcher queue full (capacity={DispatchQueueCapacity}).");
            }

            return;
        }

        var queued = new QueuedCommand(
            Sequence: Interlocked.Increment(ref _dispatchSequence),
            Message: message,
            EnqueuedAtUtc: DateTime.UtcNow,
            Attempt: 0,
            NextAttemptUtc: DateTime.UtcNow,
            CommandId: descriptor.CommandId,
            CommandType: descriptor.CommandType);
        _dispatchQueue.Enqueue(queued);
    }

    private async Task RunDispatchLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!_running || _remoteCommands == null || !_dispatchQueue.TryDequeue(out var queued))
                {
                    await Task.Delay(120, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                var now = DateTime.UtcNow;
                if (queued.NextAttemptUtc > now)
                {
                    _dispatchQueue.Enqueue(queued);
                    await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if ((now - queued.EnqueuedAtUtc) > DispatchQueueTimeout)
                {
                    var timeoutMessage = $"Dispatcher timeout before execution ({DispatchQueueTimeout.TotalSeconds:F0}s).";
                    if (!string.IsNullOrWhiteSpace(queued.CommandId))
                        _network?.SendCommandResult(queued.CommandId, false, timeoutMessage);
                    TrackOperationalEvent(
                        "dispatcher_queue_timeout",
                        $"commandId={queued.CommandId};commandType={queued.CommandType}",
                        "warning");
                    continue;
                }

                if (Interlocked.Exchange(ref _dispatchInProgress, 1) == 1)
                {
                    _dispatchQueue.Enqueue(queued);
                    await Task.Delay(80, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                try
                {
                    _remoteCommands.HandleCommand(queued.Message);
                }
                catch (Exception ex)
                {
                    var retry = queued.Attempt + 1;
                    if (retry <= 3)
                    {
                        var delay = TimeSpan.FromMilliseconds(DispatchRetryBaseDelay.TotalMilliseconds * retry);
                        _dispatchQueue.Enqueue(queued with
                        {
                            Attempt = retry,
                            NextAttemptUtc = DateTime.UtcNow.Add(delay)
                        });
                        TrackOperationalEvent(
                            "dispatcher_retry",
                            $"commandId={queued.CommandId};attempt={retry};error={ex.Message}",
                            "warning");
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(queued.CommandId))
                            _network?.SendCommandResult(queued.CommandId, false, "Dispatcher failed after retries: " + ex.Message);
                        TrackOperationalEvent(
                            "dispatcher_failed",
                            $"commandId={queued.CommandId};error={ex.Message}",
                            "error");
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _dispatchInProgress, 0);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log("Dispatcher loop error: " + ex.Message);
            }
        }
    }

    private (string CommandId, string CommandType) ResolveCommandDescriptor(string payload)
    {
        if (RemoteCommandEnvelope.TryParse(payload ?? string.Empty, out var envelope))
        {
            return (
                string.IsNullOrWhiteSpace(envelope.CommandId) ? Guid.NewGuid().ToString("N") : envelope.CommandId,
                string.IsNullOrWhiteSpace(envelope.CommandType) ? "UNKNOWN" : envelope.CommandType);
        }

        var parts = (payload ?? string.Empty).Split('|', StringSplitOptions.None);
        if (parts.Length >= 3)
        {
            var commandType = string.IsNullOrWhiteSpace(parts[1]) ? parts[0] : parts[1];
            var commandId = string.IsNullOrWhiteSpace(parts[2]) ? Guid.NewGuid().ToString("N") : parts[2];
            return (commandId, string.IsNullOrWhiteSpace(commandType) ? "UNKNOWN" : commandType);
        }

        return (Guid.NewGuid().ToString("N"), "UNKNOWN");
    }

    private void InitializeWatchRoots()
    {
        _watchRoots.Clear();

        foreach (var candidate in SplitRoots(_config.SourcePath))
            TryAddWatchRoot(candidate, _config.ATM_Type, "configured");

        TryAddWatchRoot(AppConstants.GetDefaultSourcePath(_config.ATM_Type), _config.ATM_Type, "default");
        TryAddWatchRoot(AppConstants.NCR_JournalPath, AppConstants.ATM_TYPE_NCR, "vendor-default", mustExist: true);
        TryAddWatchRoot(AppConstants.GRG_JournalPath, AppConstants.ATM_TYPE_GRG, "vendor-default", mustExist: true);
        TryAddWatchRoot(AppConstants.WN_JournalPath, AppConstants.ATM_TYPE_WN, "vendor-default", mustExist: true);

        if (_watchRoots.Count == 0)
            TryAddWatchRoot(AppConstants.DefaultClientOutboxPath, _config.ATM_Type, "fallback");

        Log("Watcher roots: " + string.Join(", ", _watchRoots.Select(root => $"{root.ATM_Type}:{root.Path}")));
    }

    private bool TryResolveWatchRoot(string filePath, out WatchRoot root)
    {
        var full = Path.GetFullPath(filePath);
        var matched = _watchRoots
            .Where(entry => full.StartsWith(entry.Path, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => entry.Path.Length)
            .FirstOrDefault();
        if (matched == null)
        {
            root = new WatchRoot(string.Empty, string.Empty, string.Empty);
            return false;
        }

        root = matched;
        return true;
    }

    private static bool IsWatchFileAllowed(string filePath, string atmType)
    {
        var fileName = Path.GetFileName(filePath);
        var normalized = AppConstants.NormalizeATMType(atmType);
        return normalized switch
        {
            var n when n == AppConstants.ATM_TYPE_NCR =>
                fileName.Equals(AppConstants.NCR_EJData, StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals(AppConstants.NCR_EJRcpy, StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals(AppConstants.NCR_EJDataLob, StringComparison.OrdinalIgnoreCase),
            var g when g == AppConstants.ATM_TYPE_GRG =>
                WildcardMatch(fileName, AppConstants.GRG_FilePattern) || WildcardMatch(fileName, AppConstants.GRG_TracePattern),
            var w when w == AppConstants.ATM_TYPE_WN =>
                WildcardMatch(fileName, AppConstants.WN_EJPattern) || WildcardMatch(fileName, AppConstants.WN_LogPattern),
            _ => true
        };
    }

    private void TryAddWatchRoot(string path, string atmType, string source, bool mustExist = false)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            var full = Path.GetFullPath(path.Trim());
            if (mustExist && !Directory.Exists(full))
                return;

            if (_watchRoots.Any(existing => string.Equals(existing.Path, full, StringComparison.OrdinalIgnoreCase)))
                return;

            _watchRoots.Add(new WatchRoot(full, AppConstants.NormalizeATMType(atmType), source));
        }
        catch
        {
        }
    }

    private static IEnumerable<string> SplitRoots(string value)
    {
        return (value ?? string.Empty)
            .Split(new[] { ';', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static bool WildcardMatch(string fileName, string pattern)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(pattern))
            return false;

        var regex = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*", StringComparison.Ordinal)
            .Replace("\\?", ".", StringComparison.Ordinal) + "$";
        return Regex.IsMatch(fileName, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private void HandleFileChanged(string filePath)
    {
        if (!_running || string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return;

        if (!TryResolveWatchRoot(filePath, out var watchRoot) || !IsWatchFileAllowed(filePath, watchRoot.ATM_Type))
            return;

        try
        {
            var payload = SecurityHelper.ReadFileSafe(filePath);
            if (payload == null || payload.Length == 0)
                return;

            var fileName = Path.GetFileName(filePath);
            var checksum = SecurityHelper.SHA256Hash(payload);
            _outbox.Enqueue(_config.ATM_ID, fileName, payload, 0, checksum);
            _deliveryTracker.RegisterQueued(fileName, checksum, payload.LongLength);
            _supabase?.LogFileTransfer(filePath, payload.LongLength);
            Log($"Journal queued: {fileName} ({payload.Length} bytes) root={watchRoot.ATM_Type}");
            EmitPredictiveSignals(fileName, payload, watchRoot.ATM_Type);
        }
        catch (Exception ex)
        {
            Log("File watcher queue error: " + ex.Message);
        }
    }

    private void PushHeartbeat()
    {
        if (!_running)
            return;

        var status = GetStatus();
        OnStatusUpdate?.Invoke(status);
        _supabase?.PushHeartbeat(status.connected, status.pending);
        if (status.connected && !status.handshake)
        {
            _handshakeMissCount++;
            TrackOperationalEvent(
                "handshake_missing",
                $"Connected without session handshake (miss={_handshakeMissCount}).",
                "warning");
            Log($"Handshake missing while connected (miss={_handshakeMissCount}).");

            if (_handshakeMissCount >= 3 && _network != null)
            {
                _handshakeMissCount = 0;
                Log("Recycling connection due to repeated handshake miss.");
                _network.Disconnect();
                _ = Task.Run(EnsureConnected);
            }
        }
        else if (status.handshake)
        {
            _handshakeMissCount = 0;
        }

        if (status.connected && _network != null)
        {
            FlushTelemetryQueue();
            ProbeSessionCompanionHealth();
            PublishCashTelemetry();
            var pulse = $"PULSE|{_config.ATM_ID}|{DateTime.UtcNow:O}|pending={status.pending}";
            _network.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, pulse));
            var pulseJson = BuildPulseJsonPayload(status.connected, status.handshake, status.pending);
            _network.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, "PULSE_JSON|" + pulseJson));
        }
    }

    private void EnsureConnected()
    {
        if (!_running || _network == null)
            return;
        if (_network.IsConnected)
            return;
        if (Interlocked.Exchange(ref _reconnectInProgress, 1) == 1)
            return;
        if (DateTime.UtcNow < _nextReconnectAttemptUtc)
            return;
        if (!TryEnterReconnectAttemptWindow(out var waitFor))
        {
            var now = DateTime.UtcNow;
            if ((now - _lastReconnectCircuitOpenLogUtc) > TimeSpan.FromSeconds(30))
            {
                _lastReconnectCircuitOpenLogUtc = now;
                var waitMs = Math.Max(0, (int)waitFor.TotalMilliseconds);
                TrackOperationalEvent("reconnect_circuit_open", $"waitMs={waitMs};state=open", "warning");
                Log($"Reconnect circuit open; next probe in {Math.Max(1, waitFor.TotalSeconds):F0}s.");
            }

            return;
        }

        try
        {
            _reconnectAttempts++;
            Log($"Reconnect attempt {_reconnectAttempts} to {_config.ServerIP}:{_config.ServerPort}.");
            var connected = _network.Connect();
            if (!connected)
            {
                Log("Reconnect attempt did not establish a session.");
                RegisterReconnectFailure("connect-returned-false");
                ScheduleReconnectBackoff();
            }
            else
            {
                _reconnectAttempts = 0;
                _nextReconnectAttemptUtc = DateTime.MinValue;
                ResetReconnectCircuit();
            }
        }
        catch (Exception ex)
        {
            Log("Reconnect error: " + ex.Message);
            RegisterReconnectFailure(ex.GetType().Name);
            ScheduleReconnectBackoff();
        }
        finally
        {
            Interlocked.Exchange(ref _reconnectInProgress, 0);
        }
    }

    private void QueueBackupArtifact(LogBackupScheduler.BackupArtifact artifact)
    {
        if (string.IsNullOrWhiteSpace(artifact.ArchivePath) || !File.Exists(artifact.ArchivePath))
            return;

        try
        {
            var bytes = SecurityHelper.ReadFileSafe(artifact.ArchivePath);
            if (bytes == null || bytes.Length == 0)
                return;

            var fileName = Path.GetFileName(artifact.ArchivePath);
            var checksum = SecurityHelper.SHA256Hash(bytes);
            _outbox.Enqueue(_config.ATM_ID, fileName, bytes, 0, checksum);
            _deliveryTracker.RegisterQueued(fileName, checksum, bytes.LongLength);
            _supabase?.LogFileTransfer(artifact.ArchivePath, bytes.LongLength);
            _journalProcessor?.ForceSendNow();
            Log($"Backup queued for delivery: {fileName} ({bytes.Length} bytes)");
        }
        catch (Exception ex)
        {
            Log("Backup queue error: " + ex.Message);
        }
    }

    private void QueueSecureScreenshotArtifact(ScreenshotScheduler.ScreenshotCaptureResult capture)
    {
        if (!_running || _journalProcessor == null)
            return;

        try
        {
            var bytes = SecurityHelper.ReadFileSafe(capture.LocalPath);
            if (bytes == null || bytes.Length == 0)
                return;

            var payload = bytes;
            var transformTags = new List<string>();
            if (_config.EnableCompression)
            {
                payload = SecurityHelper.Compress(payload);
                transformTags.Add("deflate");
            }

            if (_config.EnableEncryption)
            {
                payload = SecurityHelper.Encrypt(payload);
                transformTags.Add("aes");
            }

            var tag = transformTags.Count == 0 ? "raw" : string.Join("-", transformTags);
            var fileName = $"SCR_{_config.ATM_ID}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{tag}.bin";
            var checksum = SecurityHelper.SHA256Hash(payload);
            _outbox.Enqueue(_config.ATM_ID, fileName, payload, 0, checksum);
            _deliveryTracker.RegisterQueued(fileName, checksum, payload.LongLength);
            _supabase?.LogFileTransfer(capture.LocalPath, payload.LongLength);
            _journalProcessor.ForceSendNow();
        }
        catch (Exception ex)
        {
            Log("Screenshot queue error: " + ex.Message);
        }
    }

    private void InitializeSupabase()
    {
        var enabled = _config.EnableSupabaseSync ||
                      !string.IsNullOrWhiteSpace(_config.SupabaseUrl) ||
                      !string.IsNullOrWhiteSpace(_config.SupabaseServiceKey);
        if (!enabled)
            return;

        var url = ResolveConfigValue(_config.SupabaseUrl, "EJLIVE_SUPABASE_URL");
        var key = ResolveConfigValue(_config.SupabaseServiceKey, "EJLIVE_SUPABASE_SERVICE_KEY");

        _supabase = new SupabaseSync(url, key, _config.ATM_ID, _config.ATM_Type);
        _supabase.OnLog += message => Log(message);
        _supabase.Start();
        _supabase.RegisterAgent(_config.ATM_Type, _config.ServerIP, _config.ServerPort);
    }

    private void ApplyAdministrativeDefaults()
    {
        try
        {
            var configChanged = false;
            if (!_config.AllowLocalWindowsPasswordChange)
            {
                _config.AllowLocalWindowsPasswordChange = true;
                configChanged = true;
            }

            if (!_config.RequireEncryptedWindowsPasswordPayload)
            {
                _config.RequireEncryptedWindowsPasswordPayload = true;
                configChanged = true;
            }

            if (!_config.EnforceLowPriorityMode)
            {
                _config.EnforceLowPriorityMode = true;
                configChanged = true;
            }

            if (!_config.AutoEnableRemoteAccess)
            {
                _config.AutoEnableRemoteAccess = true;
                configChanged = true;
            }

            if (!_config.AutoPrepareWindowsRuntime)
            {
                _config.AutoPrepareWindowsRuntime = true;
                configChanged = true;
            }

            if (!_config.EnableWinRmBootstrap)
            {
                _config.EnableWinRmBootstrap = true;
                configChanged = true;
            }

            if (!_config.EnableRemoteRegistryBootstrap)
            {
                _config.EnableRemoteRegistryBootstrap = true;
                configChanged = true;
            }

            if (!_config.EnforceScopedFirewallRule)
            {
                _config.EnforceScopedFirewallRule = true;
                configChanged = true;
            }

            if (!_config.ConfigureDefenderExclusions)
            {
                _config.ConfigureDefenderExclusions = true;
                configChanged = true;
            }

            if (_config.ScopedFirewallPort <= 0)
            {
                _config.ScopedFirewallPort = _config.ServerPort > 0
                    ? _config.ServerPort
                    : AppConstants.DefaultPort;
                configChanged = true;
            }

            if (string.IsNullOrWhiteSpace(_config.ScopedFirewallRemoteAddresses) &&
                !string.IsNullOrWhiteSpace(_config.ServerIP))
            {
                _config.ScopedFirewallRemoteAddresses = _config.ServerIP.Trim();
                configChanged = true;
            }

            if (_config.WindowsBaselineRepairIntervalMin != 5)
            {
                _config.WindowsBaselineRepairIntervalMin = 5;
                configChanged = true;
            }

            if (string.IsNullOrWhiteSpace(_config.AllowedPasswordAccounts))
            {
                _config.AllowedPasswordAccounts = "Administrator";
                configChanged = true;
            }

            var currentUser = (Environment.UserName ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(currentUser))
            {
                var accounts = new HashSet<string>(
                    (_config.AllowedPasswordAccounts ?? string.Empty)
                        .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(item => item.Trim()),
                    StringComparer.OrdinalIgnoreCase);
                accounts.Add("Helpdesk");
                if (!accounts.Contains(currentUser))
                {
                    accounts.Add(currentUser);
                    _config.AllowedPasswordAccounts = string.Join(",", accounts.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
                    configChanged = true;
                }
                else if (!(_config.AllowedPasswordAccounts ?? string.Empty).Contains("Helpdesk", StringComparison.OrdinalIgnoreCase))
                {
                    _config.AllowedPasswordAccounts = string.Join(",", accounts.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
                    configChanged = true;
                }
            }

            if (configChanged)
            {
                _config.Save();
                AgentConfigurationXmlService.SaveAppConfig(_config);
            }

            if (!_config.AutoEnableRemoteAccess)
                return;

            EnsureRemoteBaselineReadinessAtStartup();
        }
        catch (Exception ex)
        {
            TrackOperationalEvent("remote_access_baseline_error", ex.Message, "error");
            Log("Auto admin remote baseline error: " + ex.Message);
        }
    }

    private void EnsureRemoteBaselineReadinessAtStartup()
    {
        const int startupAttempts = 4;
        const int retryDelayMs = 2000;
        WindowsPolicyEnforcementResult? lastResult = null;

        for (var attempt = 1; attempt <= startupAttempts; attempt++)
        {
            var remoteAccess = _policyEnforcer.EnforceBaseline();
            lastResult = remoteAccess;

            if (remoteAccess.Success)
            {
                var detail = $"attempt={attempt}; {_config.ServerIP}:{_config.ScopedFirewallPort}; {remoteAccess.Message}";
                Log("Auto admin remote baseline prepared.");
                TrackOperationalEvent("remote_access_baseline_ok", detail);
                return;
            }

            var level = remoteAccess.RequiresAdministrator ? "warning" : "error";
            var summary = BuildPolicyFailureSummary(remoteAccess);
            var message = $"attempt={attempt}/{startupAttempts}; {remoteAccess.Message}; {summary}";
            TrackOperationalEvent("remote_access_baseline_retry", message, level);
            Log("Auto admin remote baseline retry: " + message);

            if (attempt < startupAttempts)
                Thread.Sleep(retryDelayMs);
        }

        if (lastResult != null)
        {
            var finalLevel = lastResult.RequiresAdministrator ? "warning" : "error";
            var finalSummary = BuildPolicyFailureSummary(lastResult);
            TrackOperationalEvent("remote_access_baseline_failed", lastResult.Message + "; " + finalSummary, finalLevel);
            Log("Auto admin remote baseline final warning: " + lastResult.Message + "; " + finalSummary);
        }
    }

    private static string BuildPolicyFailureSummary(WindowsPolicyEnforcementResult result)
    {
        if (result.WhyFailedDetails.Count == 0)
            return string.IsNullOrWhiteSpace(result.WhyFailed)
                ? "why-failed=none"
                : result.WhyFailed;

        var top = result.WhyFailedDetails
            .Take(3)
            .Select(detail => $"{detail.Key}:{detail.FailureCode}")
            .ToArray();
        return "why-failed=" + string.Join("|", top);
    }

    private void ApplyProcessResourceBudget()
    {
        try
        {
            using var process = Process.GetCurrentProcess();

            if (_config.EnforceLowPriorityMode &&
                process.PriorityClass != ProcessPriorityClass.BelowNormal &&
                process.PriorityClass != ProcessPriorityClass.Idle)
            {
                process.PriorityClass = ProcessPriorityClass.BelowNormal;
                Log("Process priority baseline applied: BelowNormal.");
            }

            if (_config.PinToLastProcessorCore)
            {
                var maxBit = Math.Max(0, (IntPtr.Size * 8) - 2);
                var targetCore = Math.Clamp(Environment.ProcessorCount - 1, 0, maxBit);
                var targetMask = (IntPtr)(1L << targetCore);
                if (process.ProcessorAffinity != targetMask)
                {
                    process.ProcessorAffinity = targetMask;
                    Log($"Processor affinity baseline applied: core={targetCore}.");
                }
            }
        }
        catch (Exception ex)
        {
            Log("Process resource baseline note: " + ex.Message);
        }
    }

    private void RefreshWindowsBaseline()
    {
        if (!_running || !_config.AutoPrepareWindowsRuntime || !_config.AutoEnableRemoteAccess)
            return;

        try
        {
            var remoteAccess = _policyEnforcer.EnforceBaseline();
            if (remoteAccess.Success)
            {
                TrackOperationalEvent("remote_access_baseline_refresh_ok", remoteAccess.Message);
                return;
            }

            var severity = remoteAccess.RequiresAdministrator ? "warning" : "error";
            TrackOperationalEvent("remote_access_baseline_refresh_failed", remoteAccess.Message, severity);
        }
        catch (Exception ex)
        {
            TrackOperationalEvent("remote_access_baseline_refresh_error", ex.Message, "error");
            Log("Auto admin remote baseline refresh error: " + ex.Message);
        }
    }

    private static string ResolveConfigValue(string configuredValue, string environmentKey)
    {
        if (!string.IsNullOrWhiteSpace(configuredValue))
            return configuredValue.Trim();

        return (Environment.GetEnvironmentVariable(environmentKey) ?? string.Empty).Trim();
    }

    private void HandleJournalAcknowledgement(string ack)
    {
        Log("Journal delivery confirmed: " + ack);
        if (_journalProcessor != null &&
            _journalProcessor.ApplyAcknowledgement(ack, out var outboxItem, out var ackSuccess, out var ackDetail))
        {
            if (outboxItem != null)
            {
                var eventType = ackSuccess ? "journal_ack_ok" : "journal_ack_fail";
                var severity = ackSuccess ? "info" : "warning";
                TrackOperationalEvent(eventType, $"{outboxItem.FileName}|{ackDetail}", severity);
                if (!ackSuccess)
                    _journalProcessor.ForceSendNow();
            }
        }

        if (_deliveryTracker.TryApplyAcknowledgement(ack, out var receipt))
        {
            var success = receipt.Status == FileDeliveryStatus.Confirmed;
            _supabase?.LogDeliveryConfirmation(receipt.FileName, success, receipt.Detail);
            return;
        }

        TrackOperationalEvent("journal_ack_unparsed", ack, "warning");
    }

    private void OnNetworkMonitorEvent(string eventType, string detail)
    {
        var severity = eventType.Contains("disconnect", StringComparison.OrdinalIgnoreCase)
            ? "warning"
            : "info";
        TrackOperationalEvent(eventType, detail, severity);
    }

    private void TrackOperationalEvent(string eventType, string detail, string severity = "info")
    {
        var normalizedEventType = string.IsNullOrWhiteSpace(eventType) ? "event" : eventType.Trim();
        var normalizedSeverity = string.IsNullOrWhiteSpace(severity) ? "info" : severity.Trim().ToLowerInvariant();
        var normalizedDetail = detail ?? string.Empty;
        _supabase?.LogEvent(normalizedEventType, normalizedDetail, normalizedSeverity);
        QueueTelemetry(normalizedEventType, normalizedDetail, normalizedSeverity);
    }

    private void QueueTelemetry(string eventType, string detail, string severity)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return;

        var envelope = new TelemetryEnvelope(
            eventType.Trim(),
            (detail ?? string.Empty).Trim(),
            string.IsNullOrWhiteSpace(severity) ? "info" : severity.Trim().ToLowerInvariant(),
            DateTime.UtcNow);
        _pendingTelemetry.Enqueue(envelope);

        while (_pendingTelemetry.Count > 2000 && _pendingTelemetry.TryDequeue(out _))
        {
        }

        FlushTelemetryQueue();
    }

    private void FlushTelemetryQueue()
    {
        if (!_running || _network?.IsConnected != true)
            return;

        var sent = 0;
        while (sent < 40 && _pendingTelemetry.TryPeek(out var envelope))
        {
            if (!TrySendTelemetry(envelope))
                break;

            _pendingTelemetry.TryDequeue(out _);
            sent++;
        }
    }

    private bool TrySendTelemetry(TelemetryEnvelope envelope)
    {
        if (_network?.IsConnected != true)
            return false;

        try
        {
            var atmId = string.IsNullOrWhiteSpace(_config.ATM_ID) ? "UNKNOWN" : _config.ATM_ID.Trim();
            var safeDetail = envelope.Detail.Length <= 3000
                ? envelope.Detail
                : envelope.Detail[..3000];
            var payload =
                $"TELEMETRY|ATM={NormalizeTelemetryToken(atmId)};" +
                $"Type={NormalizeTelemetryToken(envelope.EventType)};" +
                $"Severity={NormalizeTelemetryToken(envelope.Severity)};" +
                $"Utc={envelope.ReportedAtUtc:O};" +
                $"DetailB64={Convert.ToBase64String(Encoding.UTF8.GetBytes(safeDetail))}";
            _network.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, payload));
            return true;
        }
        catch (Exception ex)
        {
            Log("Telemetry transport warning: " + ex.Message);
            return false;
        }
    }

    private void ScheduleReconnectBackoff()
    {
        var retryPolicy = RetryPolicy.ForNetwork(_config.NetworkType);
        var attempt = Math.Max(1, _reconnectAttempts);
        var policyDelayMs = retryPolicy.ComputeDelay(attempt);
        var configuredFloorMs = Math.Clamp(_config.ReconnectIntervalSec, 5, 300) * 1000;
        var baseDelayMs = Math.Max(policyDelayMs, configuredFloorMs);
        var deterministicJitterMs = ComputeDeterministicReconnectJitterMs(attempt, maxJitterMs: 4000);
        var randomJitterMs = Random.Shared.Next(150, 2200);
        var delayMs = Math.Min(600_000, checked(baseDelayMs + deterministicJitterMs + randomJitterMs));
        _nextReconnectAttemptUtc = DateTime.UtcNow.AddMilliseconds(delayMs);
        TrackOperationalEvent(
            "reconnect_backoff",
            $"attempt={attempt};delayMs={delayMs};baseMs={baseDelayMs};jitterMs={deterministicJitterMs + randomJitterMs};network={retryPolicy.Name}",
            "warning");
        Log($"Reconnect backoff scheduled: {delayMs}ms (attempt={attempt}, policy={retryPolicy.Name}, jitter={deterministicJitterMs + randomJitterMs}ms).");
    }

    private bool TryEnterReconnectAttemptWindow(out TimeSpan waitFor)
    {
        lock (_reconnectCircuitLock)
        {
            var now = DateTime.UtcNow;
            if (_reconnectCircuitState == ReconnectCircuitState.Open)
            {
                if (_reconnectCircuitOpenUntilUtc > now)
                {
                    waitFor = _reconnectCircuitOpenUntilUtc - now;
                    return false;
                }

                _reconnectCircuitState = ReconnectCircuitState.HalfOpen;
                waitFor = TimeSpan.Zero;
                return true;
            }

            waitFor = TimeSpan.Zero;
            return true;
        }
    }

    private void RegisterReconnectFailure(string reason)
    {
        lock (_reconnectCircuitLock)
        {
            _reconnectCircuitFailures++;
            if (_reconnectCircuitFailures < 5)
                return;

            _reconnectCircuitState = ReconnectCircuitState.Open;
            _reconnectCircuitOpenCycles++;
            var exponent = Math.Min(6, _reconnectCircuitOpenCycles - 1);
            var scaledOpenMs = ReconnectCircuitBaseWindow.TotalMilliseconds * Math.Pow(2, exponent);
            var deterministicJitterMs = ComputeDeterministicReconnectJitterMs(_reconnectCircuitOpenCycles + _reconnectCircuitFailures, maxJitterMs: 6000);
            var randomJitterMs = Random.Shared.Next(250, 3200);
            var openMs = Math.Min(
                ReconnectCircuitMaxWindow.TotalMilliseconds,
                scaledOpenMs + deterministicJitterMs + randomJitterMs);
            _reconnectCircuitOpenUntilUtc = DateTime.UtcNow.AddMilliseconds(openMs);
            TrackOperationalEvent(
                "reconnect_circuit_trip",
                $"reason={reason};failures={_reconnectCircuitFailures};openMs={(int)openMs};cycles={_reconnectCircuitOpenCycles}",
                "warning");
            Log($"Reconnect circuit opened after {_reconnectCircuitFailures} failures (reason={reason}, openMs={(int)openMs}).");
        }
    }

    private void ResetReconnectCircuit()
    {
        lock (_reconnectCircuitLock)
        {
            _reconnectCircuitFailures = 0;
            _reconnectCircuitOpenCycles = 0;
            _reconnectCircuitOpenUntilUtc = DateTime.MinValue;
            _reconnectCircuitState = ReconnectCircuitState.Closed;
        }
    }

    private int ComputeDeterministicReconnectJitterMs(int attempt, int maxJitterMs)
    {
        var cap = Math.Max(250, maxJitterMs);
        var token = $"{_config.ATM_ID}|{Environment.MachineName}|{attempt}|{_config.NetworkType}";
        var hash = 17;
        unchecked
        {
            foreach (var ch in token)
                hash = (hash * 31) + ch;
        }

        if (hash == int.MinValue)
            hash = int.MaxValue;

        return Math.Abs(hash) % cap;
    }

    private string BuildPulseJsonPayload(bool connected, bool handshake, int pending)
    {
        var cash = _lastCashSnapshot;
        var payload = new
        {
            terminalId = _config.ATM_ID,
            timestampUtc = DateTime.UtcNow,
            serviceState = connected ? "Connected" : "Disconnected",
            handshake,
            pendingOutbox = pending,
            networkType = _config.NetworkType,
            sessionCompanionReachable = _lastCompanionReachable,
            sessionCompanionDetail = _lastCompanionProbeDetail,
            cash = cash is null
                ? null
                : new
                {
                    cass1 = cash.Cass1,
                    cass2 = cash.Cass2,
                    cass3 = cash.Cass3,
                    cass4 = cash.Cass4,
                    remaining = cash.Remaining,
                    loaded = cash.Loaded,
                    depositIn = cash.DepositIn,
                    dispenseOut = cash.DispenseOut,
                    reject = cash.Reject,
                    retract = cash.Retract,
                    updatedAtUtc = cash.UpdatedAtUtc
                }
        };

        return JsonSerializer.Serialize(payload);
    }

    private void PublishCashTelemetry()
    {
        if (_cashTelemetry is null || _network?.IsConnected != true)
            return;

        var now = DateTime.UtcNow;
        if ((now - _lastCashTelemetryPublishUtc) < TimeSpan.FromSeconds(90))
            return;

        try
        {
            var snapshot = _cashTelemetry.GetSnapshot(_config.SourcePath);
            _lastCashSnapshot = snapshot;

            var severity =
                snapshot.Remaining <= 0
                    ? "error"
                    : snapshot.Remaining <= 3000 ||
                      snapshot.Cass1 <= 0 ||
                      snapshot.Cass2 <= 0 ||
                      snapshot.Cass3 <= 0 ||
                      snapshot.Cass4 <= 0
                        ? "warning"
                        : "info";

            var detail =
                $"cass1={snapshot.Cass1};cass2={snapshot.Cass2};cass3={snapshot.Cass3};cass4={snapshot.Cass4};" +
                $"remaining={snapshot.Remaining};loaded={snapshot.Loaded};depositIn={snapshot.DepositIn};" +
                $"dispenseOut={snapshot.DispenseOut};reject={snapshot.Reject};retract={snapshot.Retract};" +
                $"updatedAtUtc={snapshot.UpdatedAtUtc:O}";

            TrackOperationalEvent("cash_status", detail, severity);
            _lastCashTelemetryPublishUtc = now;
        }
        catch (Exception ex)
        {
            Log("Cash telemetry publish warning: " + ex.Message);
        }
    }

    private void ProbeSessionCompanionHealth()
    {
        if ((DateTime.UtcNow - _lastCompanionProbeUtc) < TimeSpan.FromMinutes(2))
            return;

        _lastCompanionProbeUtc = DateTime.UtcNow;
        var shouldProbe = SessionCompanionIpcClient.IsSessionZeroLikely();
        if (!shouldProbe)
        {
            _lastCompanionReachable = true;
            _lastCompanionProbeDetail = "interactive-session";
            return;
        }

        _lastCompanionReachable = SessionCompanionIpcClient.TryPing(out var detail, timeoutMs: 1200);
        _lastCompanionProbeDetail = string.IsNullOrWhiteSpace(detail) ? "none" : detail;
        if (!_lastCompanionReachable)
        {
            TrackOperationalEvent(
                "session_companion_unreachable",
                "Session companion probe failed in Session0 context.",
                "warning");
        }
    }

    private static string NormalizeTelemetryToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "-";

        return value
            .Replace(";", ",", StringComparison.Ordinal)
            .Replace("=", ":", StringComparison.Ordinal)
            .Replace("|", "/", StringComparison.Ordinal)
            .Trim();
    }

    private void EmitPredictiveSignals(string fileName, byte[] payload, string vendor)
    {
        if (payload == null || payload.Length == 0)
            return;

        var previewLength = Math.Min(payload.Length, 256 * 1024);
        var text = Encoding.UTF8.GetString(payload, 0, previewLength);
        var findings = _xfsAnalyzer.AnalyzeOperationalFindings(text, vendor)
            .Take(5)
            .ToArray();
        if (findings.Length == 0)
            return;

        foreach (var finding in findings)
        {
            TrackOperationalEvent(
                "xfs_predictive_signal",
                $"{fileName}|vendor={finding.Vendor}|category={finding.Category}|code={finding.Code}|action={finding.RecommendedAction}",
                MapXfsSeverity(finding.Severity));
        }
    }

    private static string MapXfsSeverity(XfsSignalSeverity severity)
    {
        return severity switch
        {
            XfsSignalSeverity.Critical => "error",
            XfsSignalSeverity.Warning => "warning",
            _ => "info"
        };
    }

    private void Log(string message)
    {
        OnLog?.Invoke(message);
    }

    public void Dispose()
    {
        StopAll();
    }

    private readonly record struct TelemetryEnvelope(
        string EventType,
        string Detail,
        string Severity,
        DateTime ReportedAtUtc);

    private sealed record WatchRoot(
        string Path,
        string ATM_Type,
        string Source);

    private readonly record struct QueuedCommand(
        long Sequence,
        EJMessage Message,
        DateTime EnqueuedAtUtc,
        int Attempt,
        DateTime NextAttemptUtc,
        string CommandId,
        string CommandType);
}
