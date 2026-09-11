using System;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using AppConfig = EJLive.Shared.AppConfig;

namespace EJLive.Client.WinForms.Services
{
    /// <summary>
    /// Unified client network facade built over the compiled core NetworkEngine.
    /// </summary>
    public sealed class NetworkManager : IDisposable
    {
        private readonly EJLive.Core.Engine.NetworkEngine _engine;

        public event EventHandler<bool>? OnConnectionChanged;
        public event EventHandler<string>? OnSessionEstablished;
        public event EventHandler<EJMessage>? OnMessageReceived;
        public event EventHandler<string>? OnJournalAcknowledged;
        public event EventHandler<string>? OnLog;
        public event Action<string>? OnLogMessage;

        public bool IsConnected => _engine.IsConnected;
        public string SessionId => _engine.SessionId;
        public long TotalBytesSent => _engine.TotalBytesSent;
        public long TotalBytesReceived => _engine.TotalBytesReceived;
        public double SpeedKBs => _engine.SpeedKBs;

        public NetworkManager(
            string serverIp,
            int serverPort,
            string atmId,
            string atmType,
            string networkType = "LAN",
            JournalOutbox? outbox = null,
            AppConfig? runtimeConfig = null)
        {
            var transport = runtimeConfig == null
                ? NetworkTransportOptions.FromEnvironment()
                : new NetworkTransportOptions
                {
                    EnableTlsTransport = runtimeConfig.EnableTlsTransport,
                    RequireTlsTransport = runtimeConfig.RequireTlsTransport,
                    AllowUntrustedTlsCertificate = runtimeConfig.AllowUntrustedTlsCertificate,
                    EnableAdaptiveChunking = runtimeConfig.EnableAdaptiveChunking,
                    WeakNetworkLatencyMs = runtimeConfig.WeakNetworkLatencyMs
                };

            _engine = new EJLive.Core.Engine.NetworkEngine(
                serverIp,
                serverPort,
                string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim(),
                string.IsNullOrWhiteSpace(atmType) ? AppConstants.ATM_TYPE_NCR : atmType.Trim(),
                networkType,
                outbox,
                transport);

            _engine.OnConnectionChanged += (_, connected) => OnConnectionChanged?.Invoke(this, connected);
            _engine.OnSessionEstablished += (_, session) => OnSessionEstablished?.Invoke(this, session);
            _engine.OnMessageReceived += (_, message) => OnMessageReceived?.Invoke(this, message);
            _engine.OnJournalAcknowledged += (_, ack) => OnJournalAcknowledged?.Invoke(this, ack);
            _engine.OnLog += (_, message) =>
            {
                OnLog?.Invoke(this, message);
                OnLogMessage?.Invoke(message);
            };
            _engine.OnError += (_, error) =>
            {
                OnLog?.Invoke(this, "Network error: " + error);
                OnLogMessage?.Invoke("Network error: " + error);
            };
        }

        public NetworkManager(string serverIp, int serverPort, string atmId)
            : this(serverIp, serverPort, atmId, AppConstants.ATM_TYPE_NCR, "LAN", null, null)
        {
        }

        public bool Connect() => _engine.Connect();

        public void Disconnect() => _engine.Disconnect();

        public bool SendJournalFile(string fileName, byte[] data, long offset, string checksum)
        {
            var payload = data ?? Array.Empty<byte>();
            var effectiveChecksum = string.IsNullOrWhiteSpace(checksum)
                ? EJLive.Shared.SecurityHelper.SHA256Hash(payload)
                : checksum;
            return _engine.SendJournalFile(fileName ?? "journal.ej", payload, offset, effectiveChecksum);
        }

        public bool SendData(byte[] rawData)
        {
            var payload = rawData ?? Array.Empty<byte>();
            if (payload.Length == 0)
                return false;

            var fileName = $"raw-{DateTime.UtcNow:yyyyMMddHHmmssfff}.bin";
            return SendJournalFile(fileName, payload, 0, EJLive.Shared.SecurityHelper.SHA256Hash(payload));
        }

        public void SendMessage(byte[] frame)
        {
            if (frame == null || frame.Length == 0)
                return;
            _engine.SendMessage(frame);
        }

        public void SendCommandResult(string commandId, bool success, string message)
        {
            SendMessage(CommunicationProtocol.BuildCommandResult(
                string.IsNullOrWhiteSpace(commandId) ? Guid.NewGuid().ToString("N") : commandId,
                success,
                message ?? string.Empty));
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
