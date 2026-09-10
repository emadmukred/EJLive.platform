using System.ComponentModel;
using EJLive.Core.Enums;
using EJLive.Core.Models;
using EJLive.Core.Network;
using EJLive.Core.Parsers;
using EJLive.Core.Services;
using EJLive.Core.Utils;

namespace EJLive.Client.Services
{
    public partial public class ClientServiceController : IDisposable
        {
            private readonly ATMConfig _config;
            public bool IsConnected => _networkEngine?.IsConnected ?? false;
            public DateTime LastHeartbeat => _networkEngine?.LastHeartbeatReceived ?? DateTime.MinValue;
            public ATMConfig Config => _config;
            public ClientServiceController(ATMConfig config)
            {
            public void Start()
            {
            public void Stop()
            {
            private async Task ConnectAsync(CancellationToken cancellationToken)
            {
            public async Task SendCommandAsync(CommandType command, string payload = "")
            {
            public async Task RequestScreenshotAsync()
            {
            public async Task RequestTimeSyncAsync()
            {
            public async Task SendJournalFileAsync(string filePath)
            {
            private void OnConnected(object? sender, EventArgs e)
            {
            private void OnDisconnected(object? sender, EventArgs e)
            {
            private void OnConnectionError(object? sender, string e)
            {
            private void OnMessageReceived(object? sender, NetworkMessage e)
            {
            private async Task MonitorLoopAsync(CancellationToken cancellationToken)
            {
            private JournalFileType DetectFileType(string filePath)
            {
            public void Dispose()
            {
        }
    
    }
    public class ClientServiceController : IDisposable
    {
        private readonly ATMConfig _config;
        private NetworkEngine? _networkEngine;
        private JournalProcessor? _journalProcessor;
        private RemoteCommandHandler? _commandHandler;
        private CancellationTokenSource? _cts;
        private Task? _monitorTask;
    
        public bool IsConnected => _networkEngine?.IsConnected ?? false;
        public DateTime LastHeartbeat => _networkEngine?.LastHeartbeatReceived ?? DateTime.MinValue;
        public ATMConfig Config => _config;
    
        public event EventHandler<bool>? ConnectionStateChanged;
        public event EventHandler<string>? StatusMessage;
        public event EventHandler<JournalEntry>? JournalEntryReceived;
        public event EventHandler<NetworkMessage>? CommandReceived;
        public event EventHandler<string>? ErrorOccurred;
    
        public ClientServiceController(ATMConfig config)
        {
            _config = config;
        }
    
        public void Start()
        {
            _cts = new CancellationTokenSource();
    
            // Initialize network engine
            _networkEngine = new NetworkEngine(
                _config.ServerIP,
                _config.ServerPort,
                _config.DeviceId,
                _config.HeartbeatIntervalSeconds);
    
            _networkEngine.Connected += OnConnected;
            _networkEngine.Disconnected += OnDisconnected;
            _networkEngine.ConnectionError += OnConnectionError;
            _networkEngine.MessageReceived += OnMessageReceived;
    
            // Initialize journal processor
            if (Directory.Exists(_config.JournalSourcePath))
            {
                _journalProcessor = new JournalProcessor(_config);
                _journalProcessor.JournalEntryParsed += (s, e) => JournalEntryReceived?.Invoke(this, e);
                _journalProcessor.FileProcessed += (s, e) =>
                    StatusMessage?.Invoke(this, $"Journal processed: {e}");
                _journalProcessor.ProcessingError += (s, e) =>
                    ErrorOccurred?.Invoke(this, $"Journal error: {e.Message}");
                _journalProcessor.Start();
            }
    
            // Initialize command handler
            _commandHandler = new RemoteCommandHandler();
    
            // Start connection
            _ = ConnectAsync(_cts.Token);
    
            // Start monitoring task
            _monitorTask = Task.Run(() => MonitorLoopAsync(_cts.Token));
    
            StatusMessage?.Invoke(this, "Services started");
            Logger.Info("Client services started", "ClientServiceController");
        }
    
        public void Stop()
        {
            _cts?.Cancel();
            _journalProcessor?.Stop();
            _networkEngine?.DisconnectAsync().Wait(TimeSpan.FromSeconds(5));
            _monitorTask?.Wait(TimeSpan.FromSeconds(5));
    
            StatusMessage?.Invoke(this, "Services stopped");
            Logger.Info("Client services stopped", "ClientServiceController");
        }
    
        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _networkEngine!.ConnectAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Error($"Connection failed: {ex.Message}", "ClientServiceController", ex);
                ErrorOccurred?.Invoke(this, ex.Message);
            }
        }
    
        public async Task SendCommandAsync(CommandType command, string payload = "")
        {
            if (_networkEngine == null || !_networkEngine.IsConnected) return;
    
            var message = new NetworkMessage
            {
                Command = command,
                DeviceId = _config.DeviceId,
                Timestamp = DateTime.UtcNow,
                Payload = payload
            };
    
            await _networkEngine.SendMessageAsync(message);
        }
    
        public async Task RequestScreenshotAsync()
        {
            await SendCommandAsync(CommandType.CMD_SCREENSHOT,
                System.Text.Json.JsonSerializer.Serialize(new ScreenshotPayload()));
        }
    
        public async Task RequestTimeSyncAsync()
        {
            await SendCommandAsync(CommandType.CMD_TIMESYNC);
        }
    
        public async Task SendJournalFileAsync(string filePath)
        {
            if (_networkEngine == null || !_networkEngine.IsConnected) return;
    
            var fileInfo = new FileInfo(filePath);
            var payload = new FileTransferPayload
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                FileSize = fileInfo.Length,
                FileType = DetectFileType(filePath)
            };
    
            var message = new NetworkMessage
            {
                Command = CommandType.FileStart,
                DeviceId = _config.DeviceId,
                FileName = payload.FileName,
                Payload = System.Text.Json.JsonSerializer.Serialize(payload)
            };
    
            await _networkEngine.SendMessageAsync(message);
        }
    
        private void OnConnected(object? sender, EventArgs e)
        {
            ConnectionStateChanged?.Invoke(this, true);
            StatusMessage?.Invoke(this, $"Connected to server {_config.ServerIP}:{_config.ServerPort}");
            Logger.Info("Connected to server", "ClientServiceController");
        }
    
        private void OnDisconnected(object? sender, EventArgs e)
        {
            ConnectionStateChanged?.Invoke(this, false);
            StatusMessage?.Invoke(this, "Disconnected from server");
            Logger.Warning("Disconnected from server", "ClientServiceController");
        }
    
        private void OnConnectionError(object? sender, string e)
        {
            ConnectionStateChanged?.Invoke(this, false);
            ErrorOccurred?.Invoke(this, e);
        }
    
        private void OnMessageReceived(object? sender, NetworkMessage e)
        {
            CommandReceived?.Invoke(this, e);
    
            // Handle remote commands
            if (e.Command == CommandType.CMD_RESTART ||
                e.Command == CommandType.CMD_SCREENSHOT ||
                e.Command == CommandType.CMD_TIMESYNC ||
                e.Command == CommandType.CMD_CHGPWD ||
                e.Command == CommandType.CMD_GHOSTREMOTE ||
                e.Command == CommandType.CMD_GETLOG ||
                e.Command == CommandType.CMD_UPDATECONFIG ||
                e.Command == CommandType.CMD_PING)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var response = await _commandHandler!.HandleCommandAsync(e);
                        if (_networkEngine?.IsConnected == true)
                        {
                            await _networkEngine.SendMessageAsync(response);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Command handling error: {ex.Message}", "ClientServiceController", ex);
                    }
                });
            }
        }
    
        private async Task MonitorLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Check connection health
                    if (_networkEngine != null && !_networkEngine.IsConnected)
                    {
                        StatusMessage?.Invoke(this, "Reconnecting...");
                        await ConnectAsync(cancellationToken);
                    }
    
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Monitor error: {ex.Message}", "ClientServiceController", ex);
                }
            }
        }
    
        private JournalFileType DetectFileType(string filePath)
        {
            var name = Path.GetFileName(filePath).ToUpperInvariant();
            if (name.Contains("EJDATA.LOG")) return JournalFileType.EJDATA_LOG;
            if (name.Contains("EJRCPY.LOG")) return JournalFileType.EJRCPY_LOG;
            if (name.Contains("EJDATA.LOB")) return JournalFileType.EJDATA_LOB;
            if (name.Contains("TRACE")) return JournalFileType.TRACE;
            if (name.Contains("EJ")) return JournalFileType.EJ;
            return JournalFileType.EJ;
        }
    
        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
            _journalProcessor?.Dispose();
            _networkEngine?.Dispose();
        }
    }
    public partial class ClientServiceController : IDisposable
        {
            private readonly ATMConfig _config;
    
    
            private NetworkEngine? _networkEngine;
    
    
            private JournalProcessor? _journalProcessor;
    
    
            private RemoteCommandHandler? _commandHandler;
    
    
            private CancellationTokenSource? _cts;
    
    
            private Task? _monitorTask;
    
    
            public bool IsConnected => _networkEngine?.IsConnected ?? false;
    
    
            public DateTime LastHeartbeat => _networkEngine?.LastHeartbeatReceived ?? DateTime.MinValue;
    
    
            public ATMConfig Config => _config;
    
    
            public ClientServiceController(ATMConfig config)
            _config = config;
    
    
            public event EventHandler<bool>? ConnectionStateChanged;
    
    
            public event EventHandler<string>? StatusMessage;
    
    
            public event EventHandler<JournalEntry>? JournalEntryReceived;
    
    
            public event EventHandler<NetworkMessage>? CommandReceived;
    
    
            public event EventHandler<string>? ErrorOccurred;
    
    
        }

    // Class: ClientServiceController (from 2 sources)
        public partial class ClientServiceController : IDisposable
        {
            // --- Constants & Fields ---
            private readonly ATMConfig _config;
    
            private NetworkEngine? _networkEngine;
    
            private JournalProcessor? _journalProcessor;
    
            private RemoteCommandHandler? _commandHandler;
    
            private CancellationTokenSource? _cts;
    
            private Task? _monitorTask;
    
    
            // --- Properties ---
            public bool IsConnected => _networkEngine?.IsConnected ?? false;
    
            public DateTime LastHeartbeat => _networkEngine?.LastHeartbeatReceived ?? DateTime.MinValue;
    
            public ATMConfig Config => _config;
    
    
            // --- Constructors ---
            public ClientServiceController(ATMConfig config)
            _config = config;
    
    
            // --- Events ---
            public event EventHandler<bool>? ConnectionStateChanged;
    
            public event EventHandler<string>? StatusMessage;
    
            public event EventHandler<JournalEntry>? JournalEntryReceived;
    
            public event EventHandler<NetworkMessage>? CommandReceived;
    
            public event EventHandler<string>? ErrorOccurred;
    
    
        }
}
