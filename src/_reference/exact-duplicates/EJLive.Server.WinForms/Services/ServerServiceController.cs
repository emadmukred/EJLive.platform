using EJLive.Core.Enums;
using EJLive.Core.Models;
using EJLive.Core.Network;
using EJLive.Core.Parsers;
using EJLive.Core.Services;
using EJLive.Core.Utils;
using ELive.Core.Models;
using System.Collections.Concurrent;

namespace EJLive.Server.Services
{
    public partial class ServerServiceController : IDisposable
    {
        private readonly AppSettings _settings;
        private ServerEngine? _serverEngine;
        private ImageSyncEngine? _imageSyncEngine;
        private TransactionAnalysisEngine? _transactionEngine;
        private FileTransferManager? _fileTransferManager;
        private AlertManager _alertManager;
        private CancellationTokenSource? _cts;
        private System.Timers.Timer? _healthCheckTimer;
        public ConcurrentDictionary<string, ATMDevice> Devices { get; } = new();
        public ServerEngine? ServerEngine => _serverEngine;
        public AlertManager AlertManager => _alertManager;
        public bool IsRunning => _serverEngine?.IsRunning ?? false;
        public ServerServiceController(AppSettings settings)
        _settings = settings;
        _alertManager = new AlertManager();
        _alertManager.NewAlert += (s, e) => AlertReceived?.Invoke(this, e);
        public event EventHandler? DeviceListChanged;
        public event EventHandler<AlertPayload>? AlertReceived;
        public event EventHandler<string>? StatusMessage;
        public event EventHandler<NetworkMessage>? MessageReceived;
    }

}
