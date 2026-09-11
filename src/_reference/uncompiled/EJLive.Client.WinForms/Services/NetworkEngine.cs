using System;
using System.IO;
using System.Linq;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Client.WinForms.Services
{
    public sealed class NetworkEngine : IDisposable
    {
        private readonly EJLive.Core.Engine.NetworkEngine _engine;

        public event Action<string>? OnLog;
        public event Action<Exception>? OnError;
        public event Action? OnConnected;
        public event Action? OnDisconnected;
        public event Action<string, string[]>? OnCommandReceived;
        public bool IsConnected => _engine.IsConnected;

        public NetworkEngine(string atmId, string atmType, string serverIP, int serverPort)
        {
            var config = AppConfig.Load();
            var transport = new NetworkTransportOptions
            {
                EnableTlsTransport = config.EnableTlsTransport,
                RequireTlsTransport = config.RequireTlsTransport,
                AllowUntrustedTlsCertificate = config.AllowUntrustedTlsCertificate,
                EnableAdaptiveChunking = config.EnableAdaptiveChunking,
                WeakNetworkLatencyMs = config.WeakNetworkLatencyMs
            };

            _engine = new EJLive.Core.Engine.NetworkEngine(
                serverIP,
                serverPort,
                string.IsNullOrWhiteSpace(atmId) ? "Unknown" : atmId.Trim(),
                string.IsNullOrWhiteSpace(atmType) ? AppConstants.ATM_TYPE_NCR : atmType.Trim(),
                transportOptions: transport);

            _engine.OnLog += (_, message) => OnLog?.Invoke("[Network] " + message);
            _engine.OnError += (_, message) => OnError?.Invoke(new InvalidOperationException(message));
            _engine.OnConnectionChanged += (_, connected) =>
            {
                if (connected) OnConnected?.Invoke();
                else OnDisconnected?.Invoke();
            };
            _engine.OnMessageReceived += (_, message) => HandleIncomingMessage(message);
        }

        public void Connect()
        {
            _engine.Connect();
        }

        public void Disconnect()
        {
            _engine.Disconnect();
        }

        public bool SendJournalData(string fileName, byte[] rawData, string checksum)
        {
            var payload = rawData ?? Array.Empty<byte>();
            var computedChecksum = string.IsNullOrWhiteSpace(checksum)
                ? SecurityHelper.SHA256Hash(payload)
                : checksum;
            return _engine.SendJournalFile(fileName ?? "journal.ej", payload, 0, computedChecksum);
        }

        public void SendTerminalProfile(string branchName, string vendor, string network, string region)
        {
            var payload = $"STATUSMETA|branch={Sanitize(branchName)}|vendor={Sanitize(vendor)}|network={Sanitize(network)}|region={Sanitize(region)}";
            _engine.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, payload));
        }

        public void SendCashStatus(int cass1, int cass2, int cass3, int cass4, int remaining, int loaded, int depositIn, int dispenseOut, int reject, int retract)
        {
            var payload = string.Join("|", new[]
            {
                "CASHSTATUS",
                "cass1=" + cass1,
                "cass2=" + cass2,
                "cass3=" + cass3,
                "cass4=" + cass4,
                "remaining=" + remaining,
                "loaded=" + loaded,
                "depositIn=" + depositIn,
                "dispenseOut=" + dispenseOut,
                "reject=" + reject,
                "retract=" + retract
            });
            _engine.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, payload));
        }

        public void SendCommandResult(string command, bool success, string message)
        {
            _engine.SendMessage(CommunicationProtocol.BuildCommandResult(
                string.IsNullOrWhiteSpace(command) ? Guid.NewGuid().ToString("N") : command,
                success,
                message ?? string.Empty));
        }

        public void SendFile(string fileName, byte[] data, string checksum)
        {
            var payload = data ?? Array.Empty<byte>();
            var computedChecksum = string.IsNullOrWhiteSpace(checksum)
                ? SecurityHelper.SHA256Hash(payload)
                : checksum;
            _engine.SendJournalFile(fileName ?? "payload.bin", payload, 0, computedChecksum);
        }

        public void SendRawData(byte[] data)
        {
            if (data == null || data.Length == 0)
                return;
            _engine.SendMessage(data);
        }

        private void HandleIncomingMessage(EJMessage message)
        {
            if (message.Type != CommunicationProtocol.MsgType.Command)
                return;

            if (RemoteCommandEnvelope.TryParse(message.Text, out var command))
            {
                var parameters = string.IsNullOrWhiteSpace(command.Payload)
                    ? Array.Empty<string>()
                    : command.Payload.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(value => value.Trim())
                        .Where(value => value.Length > 0)
                        .ToArray();
                OnCommandReceived?.Invoke(command.CommandType, parameters);
                return;
            }

            if (!AppConfig.Load().AllowUnsignedLegacyCommands)
            {
                OnLog?.Invoke("[Network] Command rejected: signature verification failed and unsigned fallback is disabled.");
                return;
            }

            var fallbackParts = (message.Text ?? string.Empty).Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (fallbackParts.Length == 0)
                return;
            OnCommandReceived?.Invoke(fallbackParts[0], fallbackParts.Skip(1).ToArray());
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "-";
            return value.Replace("|", "/").Replace("\r", " ").Replace("\n", " ").Trim();
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
