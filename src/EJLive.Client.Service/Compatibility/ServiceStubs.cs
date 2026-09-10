using System;
using EJLive.Core.Models;

namespace EJLive.Client.Service.Compatibility
{
    /// <summary>
    /// Minimal stubs for types that live in Core but are not yet compiled there
    /// due to dependency chain issues (DatabaseManager, etc.).
    /// These stubs allow Client.Service to build while Core is being incrementally
    /// repaired. Replace with real Core implementations as dependencies are resolved.
    /// </summary>

    // ---- Transport ----

    public class NetworkTransportOptions
    {
        public static NetworkTransportOptions FromEnvironment() => new();
    }

    // ---- Outbox ----

    public class JournalOutbox : IDisposable
    {
        public int PendingCount => 0;
        public void Dispose() { }
    }

    // ---- Heartbeat (stubs for types not yet compiled in Core) ----

    public class HeartbeatAck
    {
        public bool RequestImmediateSync { get; set; }
        public int CommandsPendingCount { get; set; }
    }

    // ---- Communication Protocol ----

    public static class CommunicationProtocol
    {
        public enum MsgType
        {
            Unknown = 0,
            Data = 1,
            Command = 2,
            Broadcast = 3,
            Heartbeat = 4,
            HeartbeatAck = 5,
            JournalAck = 6
        }

        public static EJMessage BuildCommandResult(string commandId, bool success, string message)
        {
            return new EJMessage
            {
                Type = MsgType.Data,
                Text = $"{commandId}|{success}|{message}"
            };
        }
    }

    // ---- Messages ----

    public class EJMessage
    {
        public CommunicationProtocol.MsgType Type { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    // ---- Network Engine ----

    public class NetworkEngine : IDisposable
    {
        public NetworkEngine(
            string serverIp,
            int serverPort,
            string atmId,
            string atmType,
            string networkType,
            JournalOutbox? outbox,
            NetworkTransportOptions? transportOptions = null)
        {
            ServerIp = serverIp;
            ServerPort = serverPort;
            AtmId = atmId;
            AtmType = atmType;
            NetworkType = networkType;
        }

        public string ServerIp { get; }
        public int ServerPort { get; }
        public string AtmId { get; }
        public string AtmType { get; }
        public string NetworkType { get; }

        public bool IsConnected { get; private set; }
        public string SessionId { get; private set; } = string.Empty;

        public event EventHandler<bool>? OnConnectionChanged;
        public event EventHandler<string>? OnSessionEstablished;
        public event EventHandler<EJMessage>? OnMessageReceived;
        public event EventHandler<string>? OnJournalAcknowledged;
        public event EventHandler<HeartbeatAck>? OnHeartbeatAck;

        public void Connect()
        {
            IsConnected = true;
            OnConnectionChanged?.Invoke(this, true);
        }

        public void Disconnect()
        {
            IsConnected = false;
            OnConnectionChanged?.Invoke(this, false);
        }

        public void SendMessage(EJMessage message) { }

        public void SendHeartbeat(HeartbeatPayload payload)
        {
            OnHeartbeatAck?.Invoke(this, new HeartbeatAck
            {
                RequestImmediateSync = false,
                CommandsPendingCount = 0
            });
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
