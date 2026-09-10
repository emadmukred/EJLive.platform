using System;
using System.Text;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Shared;
using AppConstants = EJLive.Core.AppConstants;

namespace EJLive.Client.WinForms.Services
{
    /// <summary>
    /// Advanced network compatibility layer that delegates to NetworkManager.
    /// </summary>
    public sealed class AdvancedNetworkManager : IDisposable
    {
        private readonly NetworkManager _network;
        private readonly string _atmId;

        public event Action<bool>? OnConnectionStatusChanged;
        public event Action<string>? OnLogMessage;
        public event Action<byte[]>? OnDataReceived;

        public bool IsConnected => _network.IsConnected;

        public AdvancedNetworkManager(string serverIP, int serverPort, string atmId)
        {
            _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
            _network = new NetworkManager(serverIP, serverPort, _atmId, AppConstants.ATM_TYPE_NCR);
            _network.OnConnectionChanged += (_, connected) => OnConnectionStatusChanged?.Invoke(connected);
            _network.OnLog += (_, message) => OnLogMessage?.Invoke(message);
            _network.OnMessageReceived += (_, message) => OnDataReceived?.Invoke(BuildPayload(message));
        }

        public bool Connect()
        {
            var connected = _network.Connect();
            OnConnectionStatusChanged?.Invoke(connected);
            return connected;
        }

        public void Disconnect()
        {
            _network.Disconnect();
            OnConnectionStatusChanged?.Invoke(false);
        }

        public bool SendData(byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;
            var fileName = $"adv-{DateTime.UtcNow:yyyyMMddHHmmssfff}.bin";
            var checksum = SecurityHelper.SHA256Hash(data);
            var sent = _network.SendJournalFile(fileName, data, 0, checksum);
            OnLogMessage?.Invoke(sent
                ? $"Advanced data sent: {data.Length} bytes"
                : "Advanced data send failed");
            return sent;
        }

        public void Dispose()
        {
            _network.Dispose();
        }

        private byte[] BuildPayload(EJMessage message)
        {
            if (message.Payload != null && message.Payload.Length > 0)
                return message.Payload;
            return Encoding.UTF8.GetBytes(message.Text ?? string.Empty);
        }
    }
}
