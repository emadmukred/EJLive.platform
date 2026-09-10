using System;
using System.Collections.Generic;

namespace EJLive.Core.Transport
{
    public class HandshakeRequest
    {
        public string ATM_ID { get; set; }
        public string MachineId { get; set; }
        public string AgentId { get; set; }
        public string ProtocolVersion { get; set; }
        public string ClientVersion { get; set; }
        public string Nonce { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string Capabilities { get; set; }
        public string Vendor { get; set; }
        public string Model { get; set; }
        public string PathProfile { get; set; }
    }
    public class HandshakeResponse
    {
        public string SessionId { get; set; }
        public bool Accepted { get; set; }
        public string ServerVersion { get; set; }
        public DateTime ServerTimeUtc { get; set; }
        public string RejectReason { get; set; }
    }
    public enum HandshakeState { None, Sent, Accepted, Rejected, Expired }
    public class HeartbeatPayload
    {
        public string ATM_ID { get; set; }
        public string SessionId { get; set; }
        public string ClientState { get; set; }
        public int OutboxCount { get; set; }
        public int FailedCount { get; set; }
        public string WatcherState { get; set; }
        public string ImageInboxState { get; set; }
        public long LastJournalOffset { get; set; }
        public int DiskPercent { get; set; }
        public int MemoryPercent { get; set; }
        public int CPUPercent { get; set; }
        public string LastError { get; set; }
    }
    public class HeartbeatAck
    {
        public DateTime ServerTimeUtc { get; set; }
        public int PendingCommandCount { get; set; }
    }
    public class TransferSession
    {
        public string TransferId { get; set; }
        public string ATM_ID { get; set; }
        public string FileName { get; set; }
        public long Length { get; set; }
        public int ChunkSize { get; set; }
        public int TotalChunks { get; set; }
        public string FileSHA256 { get; set; }
        public long NextExpectedOffset { get; set; }
    }
    public class ReconnectPolicy
    {
        public int BaseMs { get; set; } = 1000;
        public int MaxMs { get; set; } = 60000;
        public int CurrentAttempt { get; set; }
        public int GetNextDelayMs()
        {
            var delay = BaseMs * (int)Math.Pow(2, Math.Min(CurrentAttempt, 6));
            var jitter = new Random().Next((int)(delay * 0.1));
            CurrentAttempt++;
            return Math.Min(delay + jitter, MaxMs);
        }
        public void Reset() { CurrentAttempt = 0; }
    }
    public class SessionRegistry
    {
        private readonly Dictionary<string, TransferSession> _sessions = new Dictionary<string, TransferSession>();
        public void Register(TransferSession session) => _sessions[session.TransferId] = session;
        public TransferSession Get(string id) => _sessions.TryGetValue(id, out var s) ? s : null;
        public void Remove(string id) => _sessions.Remove(id);
    }
}