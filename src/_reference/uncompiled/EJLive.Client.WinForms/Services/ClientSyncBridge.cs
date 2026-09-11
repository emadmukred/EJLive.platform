using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Client.WinForms.Services
{
    /// <summary>
        /// Bridges the Client WinForms UI with the core sync/journal/network services.
        /// Provides a clean facade for ClientMainForm to interact with the backend.
        /// </summary>
        public sealed class ClientSyncBridge : IDisposable
        {
            private readonly AppConfig _config;
            private readonly JournalOutbox _outbox;
            private readonly FileWatcherEngine _fileWatcher;
            private readonly JournalSyncService _syncService;
            private readonly AlertManager _alertManager;
            private NetworkEngine? _networkEngine;
            private bool _isConnected;
    
            public event Action<string>? OnLog;
            public event Action<bool>? OnConnectionChanged;
            public event Action<LiveSyncProgress>? OnSyncProgress;
            public event Action<AlertPayload>? OnAlert;
    
            public bool IsConnected => _isConnected;
            public int OutboxCount => _outbox.Count;
            public IReadOnlyCollection<JournalOutboxItem> OutboxSnapshot => _outbox.Snapshot;
            public string SessionId { get; private set; } = string.Empty;
            public long TotalBytesSent { get; private set; }
            public long TotalBytesReceived { get; private set; }
    
            public ClientSyncBridge(AppConfig config)
            {
                _config = config ?? throw new ArgumentNullException(nameof(config));
    
                _outbox = new JournalOutbox(config.SourcePath, config.BackupPath);
                _fileWatcher = new FileWatcherEngine();
                _syncService = new JournalSyncService();
                _alertManager = AlertManager.Instance;
    
                _syncService.Outbox = _outbox;
                _syncService.ProgressChanged += (_, progress) => OnSyncProgress?.Invoke(progress);
    
                _fileWatcher.OnFileChanged += OnFileChangedHandler;
                _alertManager.OnAlert += (_, alert) => OnAlert?.Invoke(alert);
            }
    
            /// <summary>
            /// Connects to the configured server.
            /// </summary>
            public async Task<bool> ConnectAsync(string serverIp, int serverPort)
            {
                try
                {
                    Log($"Connecting to {serverIp}:{serverPort}...");
    
                    _networkEngine = new NetworkEngine(
                        serverIp,
                        serverPort,
                        _config.ATM_ID,
                        _config.ATM_Type,
                        _config.NetworkType ?? "LAN",
                        _outbox);
    
                    _networkEngine.OnConnectionChanged += (_, connected) =>
                    {
                        _isConnected = connected;
                        OnConnectionChanged?.Invoke(connected);
                        Log(connected ? "Connected to server" : "Disconnected from server");
                    };
    
                    _networkEngine.OnSessionEstablished += (_, sessionId) =>
                    {
                        SessionId = sessionId;
                        Log($"Session established: {sessionId}");
                    };
    
                    _networkEngine.OnHeartbeatAck += (_, ack) =>
                    {
                        Log($"Heartbeat ACK: pendingCommands={ack.CommandsPendingCount}");
                    };
    
                    _networkEngine.OnJournalAcknowledged += (_, ack) =>
                    {
                        TotalBytesReceived += ack.Length;
                        Log($"Journal ACK received: {ack.Length} bytes");
                    };
    
                    _networkEngine.OnLog += (_, msg) => Log(msg);
    
                    await Task.Run(() => _networkEngine.Connect());
                    return _isConnected;
                }
                catch (Exception ex)
                {
                    Log($"Connection failed: {ex.Message}");
                    return false;
                }
            }
    
            /// <summary>
            /// Disconnects from the server.
            /// </summary>
            public void Disconnect()
            {
                try
                {
                    _networkEngine?.Disconnect();
                    _isConnected = false;
                    Log("Disconnected from server");
                }
                catch (Exception ex)
                {
                    Log($"Disconnect error: {ex.Message}");
                }
            }
    
            /// <summary>
            /// Starts watching journal source paths for changes.
            /// </summary>
            public void StartWatching()
            {
                var sourcePath = _config.SourcePath;
                if (!string.IsNullOrWhiteSpace(sourcePath) && Directory.Exists(sourcePath))
                {
                    _fileWatcher.AddWatch(sourcePath, "*.LOG");
                    _fileWatcher.AddWatch(sourcePath, "*.TXT");
                    _fileWatcher.AddWatch(sourcePath, "*.DAT");
                    _fileWatcher.AddWatch(sourcePath, "*.EJ");
                    _fileWatcher.AddWatch(sourcePath, "*.JRN");
                    Log($"Watching source path: {sourcePath}");
                }
    
                _fileWatcher.Start();
                _syncService.StartSync();
                Log("File watcher and sync service started");
            }
    
            /// <summary>
            /// Stops watching and sync.
            /// </summary>
            public void StopWatching()
            {
                _syncService.StopSync();
                _fileWatcher.Stop();
                Log("File watcher and sync service stopped");
            }
    
            /// <summary>
            /// Forces immediate journal synchronization.
            /// </summary>
            public void ForceSync()
            {
                try
                {
                    var sourcePath = _config.SourcePath;
                    if (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath))
                    {
                        Log("Source path not available for sync");
                        return;
                    }
    
                    var files = Directory.GetFiles(sourcePath, "*.LOG")
                        .Concat(Directory.GetFiles(sourcePath, "*.TXT"))
                        .Concat(Directory.GetFiles(sourcePath, "*.DAT"))
                        .Concat(Directory.GetFiles(sourcePath, "*.EJ"));
    
                    int count = 0;
                    foreach (var file in files)
                    {
                        try
                        {
                            var data = File.ReadAllBytes(file);
                            var checksum = ComputeChecksum(data);
                            _outbox.Enqueue(_config.ATM_ID, Path.GetFileName(file), data, 0, checksum);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            Log($"Error queuing {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
    
                    Log($"Force sync: {count} files queued");
                }
                catch (Exception ex)
                {
                    Log($"Force sync error: {ex.Message}");
                }
            }
    
            /// <summary>
            /// Sends a heartbeat to the server.
            /// </summary>
            public void SendHeartbeat()
            {
                if (_networkEngine?.IsConnected != true)
                    return;
    
                try
                {
                    _networkEngine.SendHeartbeat(new HeartbeatPayload(
                        AtmId: _config.ATM_ID,
                        SessionId: SessionId,
                        TimestampUtc: DateTime.UtcNow,
                        OutboxCount: _outbox.Count,
                        FileWatcherHealthy: _fileWatcher.IsRunning,
                        CpuPercent: 0,
                        MemoryMb: 0,
                        DiskFreeMb: 0,
                        LastJournalOffset: 0,
                        LastError: null,
                        AgentState: _isConnected ? "Running" : "Disconnected",
                        WatcherState: _fileWatcher.IsRunning ? "active" : "inactive"));
                }
                catch (Exception ex)
                {
                    Log($"Heartbeat error: {ex.Message}");
                }
            }
    
            /// <summary>
            /// Returns a diagnostic summary.
            /// </summary>
            public Dictionary<string, string> GetDiagnostics()
            {
                return new Dictionary<string, string>
                {
                    ["ATM_ID"] = _config.ATM_ID,
                    ["ATM_Type"] = _config.ATM_Type,
                    ["Server"] = $"{_config.ServerIP}:{_config.ServerPort}",
                    ["Connected"] = _isConnected.ToString(),
                    ["Session"] = string.IsNullOrWhiteSpace(SessionId) ? "(none)" : SessionId,
                    ["OutboxItems"] = _outbox.Count.ToString(),
                    ["FileWatcher"] = _fileWatcher.IsRunning ? "Running" : "Stopped",
                    ["SourcePath"] = _config.SourcePath,
                    ["BackupPath"] = _config.BackupPath,
                    ["BytesSent"] = TotalBytesSent.ToString("N0"),
                    ["BytesReceived"] = TotalBytesReceived.ToString("N0")
                };
            }
    
            private void OnFileChangedHandler(string path)
            {
                try
                {
                    if (!File.Exists(path))
                        return;
    
                    var data = File.ReadAllBytes(path);
                    var checksum = ComputeChecksum(data);
                    _outbox.Enqueue(_config.ATM_ID, Path.GetFileName(path), data, 0, checksum);
                    Log($"File detected and queued: {Path.GetFileName(path)} ({data.Length} bytes)");
                }
                catch (Exception ex)
                {
                    Log($"File change error for {path}: {ex.Message}");
                }
            }
    
            private static string ComputeChecksum(byte[] data)
            {
                using var md5 = System.Security.Cryptography.MD5.Create();
                var hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
    
            private void Log(string message) => OnLog?.Invoke($"[SyncBridge] {message}");
    
            public void Dispose()
            {
                StopWatching();
                Disconnect();
                _fileWatcher.Dispose();
                _networkEngine?.Dispose();
            }
        }
    public partial class ClientSyncBridge : IDisposable
        {
            private readonly AppConfig _config;
            private readonly JournalOutbox _outbox;
            private readonly FileWatcherEngine _fileWatcher;
            private readonly JournalSyncService _syncService;
            private readonly AlertManager _alertManager;
            private NetworkEngine? _networkEngine;
            private bool _isConnected;
            public bool IsConnected => _isConnected;
            public int OutboxCount => _outbox.Count;
            public IReadOnlyCollection<JournalOutboxItem> OutboxSnapshot => _outbox.Snapshot;
            public string SessionId { get; private set; } = string.Empty;
            public long TotalBytesSent { get; private set; }
            public long TotalBytesReceived { get; private set; }
            public ClientSyncBridge(AppConfig config)
            {
                _config = config ?? throw new ArgumentNullException(nameof(config));
                _outbox = new JournalOutbox(config.SourcePath, config.BackupPath);
                _fileWatcher = new FileWatcherEngine();
                _syncService = new JournalSyncService();
                _alertManager = AlertManager.Instance;
                _syncService.Outbox = _outbox;
                _syncService.ProgressChanged += (_, progress) => OnSyncProgress?.Invoke(progress);
                _fileWatcher.OnFileChanged += OnFileChangedHandler;
                _alertManager.OnAlert += (_, alert) => OnAlert?.Invoke(alert);
            }
            public async Task<bool> ConnectAsync(string serverIp, int serverPort)
            {
                try
                {
                    Log($"Connecting to {serverIp}:{serverPort}...");
                    _networkEngine = new NetworkEngine(
                        serverIp,
                        serverPort,
                        _config.ATM_ID,
                        _config.ATM_Type,
                        _config.NetworkType ?? "LAN",
                        _outbox);
                    _networkEngine.OnConnectionChanged += (_, connected) =>
                    {
                        _isConnected = connected;
                        OnConnectionChanged?.Invoke(connected);
                        Log(connected ? "Connected to server" : "Disconnected from server");
                    };
                    _networkEngine.OnSessionEstablished += (_, sessionId) =>
                    {
                        SessionId = sessionId;
                        Log($"Session established: {sessionId}");
                    };
                    _networkEngine.OnHeartbeatAck += (_, ack) =>
                    {
                        Log($"Heartbeat ACK: pendingCommands={ack.CommandsPendingCount}");
                    };
                    _networkEngine.OnJournalAcknowledged += (_, ack) =>
                    {
                        TotalBytesReceived += ack.Length;
                        Log($"Journal ACK received: {ack.Length} bytes");
                    };
                    _networkEngine.OnLog += (_, msg) => Log(msg);
                    await Task.Run(() => _networkEngine.Connect());
                    return _isConnected;
                }
                catch (Exception ex)
                {
                    Log($"Connection failed: {ex.Message}");
                    return false;
                }
            }
            public void Disconnect()
            {
                try
                {
                    _networkEngine?.Disconnect();
                    _isConnected = false;
                    Log("Disconnected from server");
                }
                catch (Exception ex)
                {
                    Log($"Disconnect error: {ex.Message}");
                }
            }
            public void StartWatching()
            {
                var sourcePath = _config.SourcePath;
                if (!string.IsNullOrWhiteSpace(sourcePath) && Directory.Exists(sourcePath))
                {
                    _fileWatcher.AddWatch(sourcePath, "*.LOG");
                    _fileWatcher.AddWatch(sourcePath, "*.TXT");
                    _fileWatcher.AddWatch(sourcePath, "*.DAT");
                    _fileWatcher.AddWatch(sourcePath, "*.EJ");
                    _fileWatcher.AddWatch(sourcePath, "*.JRN");
                    Log($"Watching source path: {sourcePath}");
                }
                _fileWatcher.Start();
                _syncService.StartSync();
                Log("File watcher and sync service started");
            }
            public void StopWatching()
            {
                _syncService.StopSync();
                _fileWatcher.Stop();
                Log("File watcher and sync service stopped");
            }
            public void ForceSync()
            {
                try
                {
                    var sourcePath = _config.SourcePath;
                    if (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath))
                    {
                        Log("Source path not available for sync");
                        return;
                    }
                    var files = Directory.GetFiles(sourcePath, "*.LOG")
                        .Concat(Directory.GetFiles(sourcePath, "*.TXT"))
                        .Concat(Directory.GetFiles(sourcePath, "*.DAT"))
                        .Concat(Directory.GetFiles(sourcePath, "*.EJ"));
                    int count = 0;
                    foreach (var file in files)
                    {
                        try
                        {
                            var data = File.ReadAllBytes(file);
                            var checksum = ComputeChecksum(data);
                            _outbox.Enqueue(_config.ATM_ID, Path.GetFileName(file), data, 0, checksum);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            Log($"Error queuing {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                    Log($"Force sync: {count} files queued");
                }
                catch (Exception ex)
                {
                    Log($"Force sync error: {ex.Message}");
                }
            }
            public void SendHeartbeat()
            {
                if (_networkEngine?.IsConnected != true)
                    return;
                try
                {
                    _networkEngine.SendHeartbeat(new HeartbeatPayload(
                        AtmId: _config.ATM_ID,
                        SessionId: SessionId,
                        TimestampUtc: DateTime.UtcNow,
                        OutboxCount: _outbox.Count,
                        FileWatcherHealthy: _fileWatcher.IsRunning,
                        CpuPercent: 0,
                        MemoryMb: 0,
                        DiskFreeMb: 0,
                        LastJournalOffset: 0,
                        LastError: null,
                        AgentState: _isConnected ? "Running" : "Disconnected",
                        WatcherState: _fileWatcher.IsRunning ? "active" : "inactive"));
                }
                catch (Exception ex)
                {
                    Log($"Heartbeat error: {ex.Message}");
                }
            }
            public Dictionary<string, string> GetDiagnostics()
            {
                return new Dictionary<string, string>
                {
                    ["ATM_ID"] = _config.ATM_ID,
                    ["ATM_Type"] = _config.ATM_Type,
                    ["Server"] = $"{_config.ServerIP}:{_config.ServerPort}",
                    ["Connected"] = _isConnected.ToString(),
                    ["Session"] = string.IsNullOrWhiteSpace(SessionId) ? "(none)" : SessionId,
                    ["OutboxItems"] = _outbox.Count.ToString(),
                    ["FileWatcher"] = _fileWatcher.IsRunning ? "Running" : "Stopped",
                    ["SourcePath"] = _config.SourcePath,
                    ["BackupPath"] = _config.BackupPath,
                    ["BytesSent"] = TotalBytesSent.ToString("N0"),
                    ["BytesReceived"] = TotalBytesReceived.ToString("N0")
                };
            }
            private void OnFileChangedHandler(string path)
            {
                try
                {
                    if (!File.Exists(path))
                        return;
                    var data = File.ReadAllBytes(path);
                    var checksum = ComputeChecksum(data);
                    _outbox.Enqueue(_config.ATM_ID, Path.GetFileName(path), data, 0, checksum);
                    Log($"File detected and queued: {Path.GetFileName(path)} ({data.Length} bytes)");
                }
                catch (Exception ex)
                {
                    Log($"File change error for {path}: {ex.Message}");
                }
            }
            private static string ComputeChecksum(byte[] data)
            {
                using var md5 = System.Security.Cryptography.MD5.Create();
                var hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            private void Log(string message) => OnLog?.Invoke($"[SyncBridge] {message}");
            public void Dispose()
            {
                StopWatching();
                Disconnect();
                _fileWatcher.Dispose();
                _networkEngine?.Dispose();
            }
            public event Action<string>? OnLog;
            public event Action<bool>? OnConnectionChanged;
            public event Action<LiveSyncProgress>? OnSyncProgress;
            public event Action<AlertPayload>? OnAlert;
        }
    public partial public public sealed class ClientSyncBridge : IDisposable
        {
            private readonly AppConfig _config;
            private readonly JournalOutbox _outbox;
            private readonly FileWatcherEngine _fileWatcher;
            private readonly JournalSyncService _syncService;
            private readonly AlertManager _alertManager;
            private bool _isConnected;
            public bool IsConnected => _isConnected;
            public int OutboxCount => _outbox.Count;
            public IReadOnlyCollection<JournalOutboxItem> OutboxSnapshot => _outbox.Snapshot;
            public ClientSyncBridge(AppConfig config)
            {
            public string SessionId { get; private set; }
            public long TotalBytesSent { get; private set; }
            public long TotalBytesReceived { get; private set; }
            public async Task<bool> ConnectAsync(string serverIp, int serverPort)
            {
            public void Disconnect()
            {
            public void StartWatching()
            {
            public void StopWatching()
            {
            public void ForceSync()
            {
            public void SendHeartbeat()
            {
            public Dictionary<string, string> GetDiagnostics()
            {
            private void OnFileChangedHandler(string path)
            {
            private static string ComputeChecksum(byte[] data)
            {
            private void Log(string message) =>
            public void Dispose()
            {
        }
    
    }
    public partial public sealed class ClientSyncBridge : IDisposable
        {
            private readonly AppConfig _config;
            private readonly JournalOutbox _outbox;
            private readonly FileWatcherEngine _fileWatcher;
            private readonly JournalSyncService _syncService;
            private readonly AlertManager _alertManager;
            private bool _isConnected;
            public bool IsConnected => _isConnected;
            public int OutboxCount => _outbox.Count;
            public IReadOnlyCollection<JournalOutboxItem> OutboxSnapshot => _outbox.Snapshot;
            public ClientSyncBridge(AppConfig config)
            {
            public string SessionId { get; private set; }
            public long TotalBytesSent { get; private set; }
            public long TotalBytesReceived { get; private set; }
            public async Task<bool> ConnectAsync(string serverIp, int serverPort)
            {
            public void Disconnect()
            {
            public void StartWatching()
            {
            public void StopWatching()
            {
            public void ForceSync()
            {
            public void SendHeartbeat()
            {
            public Dictionary<string, string> GetDiagnostics()
            {
            private void OnFileChangedHandler(string path)
            {
            private static string ComputeChecksum(byte[] data)
            {
            private void Log(string message) =>
            public void Dispose()
            {
        }
    
    }

    // Class: ClientSyncBridge (from 1 sources)
        public sealed partial class ClientSyncBridge : IDisposable
        {
            // --- Constants & Fields ---
                    private readonly AppConfig _config;
    
                    private readonly JournalOutbox _outbox;
    
                    private readonly FileWatcherEngine _fileWatcher;
    
                    private readonly JournalSyncService _syncService;
    
                    private readonly AlertManager _alertManager;
    
                    private NetworkEngine? _networkEngine;
    
                    private bool _isConnected;
    
    
            // --- Properties ---
                    public bool IsConnected => _isConnected;
    
                    public int OutboxCount => _outbox.Count;
    
                    public IReadOnlyCollection<JournalOutboxItem> OutboxSnapshot => _outbox.Snapshot;
    
                    public string SessionId { get; private set; } = string.Empty;
    
                    public long TotalBytesSent { get; private set; }
    
                    public long TotalBytesReceived { get; private set; }
    
    
            // --- Constructors ---
                    public ClientSyncBridge(AppConfig config)
                    {
                        _config = config ?? throw new ArgumentNullException(nameof(config));
    
                        _outbox = new JournalOutbox(config.SourcePath, config.BackupPath);
                        _fileWatcher = new FileWatcherEngine();
                        _syncService = new JournalSyncService();
                        _alertManager = AlertManager.Instance;
    
                        _syncService.Outbox = _outbox;
                        _syncService.ProgressChanged += (_, progress) => OnSyncProgress?.Invoke(progress);
    
                        _fileWatcher.OnFileChanged += OnFileChangedHandler;
                        _alertManager.OnAlert += (_, alert) => OnAlert?.Invoke(alert);
                    }
    
    
            // --- Methods ---
                    public async Task<bool> ConnectAsync(string serverIp, int serverPort)
                    {
                        try
                        {
                            Log($"Connecting to {serverIp}:{serverPort}...");
    
                            _networkEngine = new NetworkEngine(
                                serverIp,
                                serverPort,
                                _config.ATM_ID,
                                _config.ATM_Type,
                                _config.NetworkType ?? "LAN",
                                _outbox);
    
                            _networkEngine.OnConnectionChanged += (_, connected) =>
                            {
                                _isConnected = connected;
                                OnConnectionChanged?.Invoke(connected);
                                Log(connected ? "Connected to server" : "Disconnected from server");
                            };
    
                            _networkEngine.OnSessionEstablished += (_, sessionId) =>
                            {
                                SessionId = sessionId;
                                Log($"Session established: {sessionId}");
                            };
    
                            _networkEngine.OnHeartbeatAck += (_, ack) =>
                            {
                                Log($"Heartbeat ACK: pendingCommands={ack.CommandsPendingCount}");
                            };
    
                            _networkEngine.OnJournalAcknowledged += (_, ack) =>
                            {
                                TotalBytesReceived += ack.Length;
                                Log($"Journal ACK received: {ack.Length} bytes");
                            };
    
                            _networkEngine.OnLog += (_, msg) => Log(msg);
    
                            await Task.Run(() => _networkEngine.Connect());
                            return _isConnected;
                        }
                        catch (Exception ex)
                        {
                            Log($"Connection failed: {ex.Message}");
                            return false;
                        }
                    }
    
                    public void Disconnect()
                    {
                        try
                        {
                            _networkEngine?.Disconnect();
                            _isConnected = false;
                            Log("Disconnected from server");
                        }
                        catch (Exception ex)
                        {
                            Log($"Disconnect error: {ex.Message}");
                        }
                    }
    
                    public void StartWatching()
                    {
                        var sourcePath = _config.SourcePath;
                        if (!string.IsNullOrWhiteSpace(sourcePath) && Directory.Exists(sourcePath))
                        {
                            _fileWatcher.AddWatch(sourcePath, "*.LOG");
                            _fileWatcher.AddWatch(sourcePath, "*.TXT");
                            _fileWatcher.AddWatch(sourcePath, "*.DAT");
                            _fileWatcher.AddWatch(sourcePath, "*.EJ");
                            _fileWatcher.AddWatch(sourcePath, "*.JRN");
                            Log($"Watching source path: {sourcePath}");
                        }
    
                        _fileWatcher.Start();
                        _syncService.StartSync();
                        Log("File watcher and sync service started");
                    }
    
                    public void StopWatching()
                    {
                        _syncService.StopSync();
                        _fileWatcher.Stop();
                        Log("File watcher and sync service stopped");
                    }
    
                    public void ForceSync()
                    {
                        try
                        {
                            var sourcePath = _config.SourcePath;
                            if (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath))
                            {
                                Log("Source path not available for sync");
                                return;
                            }
    
                            var files = Directory.GetFiles(sourcePath, "*.LOG")
                                .Concat(Directory.GetFiles(sourcePath, "*.TXT"))
                                .Concat(Directory.GetFiles(sourcePath, "*.DAT"))
                                .Concat(Directory.GetFiles(sourcePath, "*.EJ"));
    
                            int count = 0;
                            foreach (var file in files)
                            {
                                try
                                {
                                    var data = File.ReadAllBytes(file);
                                    var checksum = ComputeChecksum(data);
                                    _outbox.Enqueue(_config.ATM_ID, Path.GetFileName(file), data, 0, checksum);
                                    count++;
                                }
                                catch (Exception ex)
                                {
                                    Log($"Error queuing {Path.GetFileName(file)}: {ex.Message}");
                                }
                            }
    
                            Log($"Force sync: {count} files queued");
                        }
                        catch (Exception ex)
                        {
                            Log($"Force sync error: {ex.Message}");
                        }
                    }
    
                    public void SendHeartbeat()
                    {
                        if (_networkEngine?.IsConnected != true)
                            return;
    
                        try
                        {
                            _networkEngine.SendHeartbeat(new HeartbeatPayload(
                                AtmId: _config.ATM_ID,
                                SessionId: SessionId,
                                TimestampUtc: DateTime.UtcNow,
                                OutboxCount: _outbox.Count,
                                FileWatcherHealthy: _fileWatcher.IsRunning,
                                CpuPercent: 0,
                                MemoryMb: 0,
                                DiskFreeMb: 0,
                                LastJournalOffset: 0,
                                LastError: null,
                                AgentState: _isConnected ? "Running" : "Disconnected",
                                WatcherState: _fileWatcher.IsRunning ? "active" : "inactive"));
                        }
                        catch (Exception ex)
                        {
                            Log($"Heartbeat error: {ex.Message}");
                        }
                    }
    
                    public Dictionary<string, string> GetDiagnostics()
                    {
                        return new Dictionary<string, string>
                        {
                            ["ATM_ID"] = _config.ATM_ID,
                            ["ATM_Type"] = _config.ATM_Type,
                            ["Server"] = $"{_config.ServerIP}:{_config.ServerPort}",
                            ["Connected"] = _isConnected.ToString(),
                            ["Session"] = string.IsNullOrWhiteSpace(SessionId) ? "(none)" : SessionId,
                            ["OutboxItems"] = _outbox.Count.ToString(),
                            ["FileWatcher"] = _fileWatcher.IsRunning ? "Running" : "Stopped",
                            ["SourcePath"] = _config.SourcePath,
                            ["BackupPath"] = _config.BackupPath,
                            ["BytesSent"] = TotalBytesSent.ToString("N0"),
                            ["BytesReceived"] = TotalBytesReceived.ToString("N0")
                        };
                    }
    
                    private void OnFileChangedHandler(string path)
                    {
                        try
                        {
                            if (!File.Exists(path))
                                return;
    
                            var data = File.ReadAllBytes(path);
                            var checksum = ComputeChecksum(data);
                            _outbox.Enqueue(_config.ATM_ID, Path.GetFileName(path), data, 0, checksum);
                            Log($"File detected and queued: {Path.GetFileName(path)} ({data.Length} bytes)");
                        }
                        catch (Exception ex)
                        {
                            Log($"File change error for {path}: {ex.Message}");
                        }
                    }
    
                    private static string ComputeChecksum(byte[] data)
                    {
                        using var md5 = System.Security.Cryptography.MD5.Create();
                        var hash = md5.ComputeHash(data);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
    
                    private void Log(string message) => OnLog?.Invoke($"[SyncBridge] {message}");
    
                    public void Dispose()
                    {
                        StopWatching();
                        Disconnect();
                        _fileWatcher.Dispose();
                        _networkEngine?.Dispose();
                    }
    
    
            // --- Events ---
                    public event Action<string>? OnLog;
    
                    public event Action<bool>? OnConnectionChanged;
    
                    public event Action<LiveSyncProgress>? OnSyncProgress;
    
                    public event Action<AlertPayload>? OnAlert;
    
    
        }
}

using var md5 = System.Security.Cryptography.MD5.Create();
