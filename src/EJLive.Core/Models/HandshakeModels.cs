using System;

namespace EJLive.Core.Models
{
    public enum HandshakeState
    {
        None,
        Sent,
        Accepted,
        Rejected,
        Expired
    }

    public sealed record HandshakeRequest(
        string AtmId,
        string MachineId,
        string ProtocolVersion,
        string ClientVersion,
        string SessionId,
        DateTime TimestampUtc);

    public sealed record HandshakeResponse(
        bool Accepted,
        string? ServerSessionId,
        string? RejectionReason,
        DateTime ServerTimeUtc,
        int CommandPendingCount,
        TimeSpan HeartbeatInterval);

    public sealed record HeartbeatPayload(
        string AtmId,
        string? SessionId,
        DateTime TimestampUtc,
        int OutboxCount,
        bool FileWatcherHealthy,
        double CpuPercent,
        double MemoryMb,
        double DiskFreeMb,
        long LastJournalOffset,
        string? LastError);

    public sealed record HeartbeatAck(
        DateTime ServerTimeUtc,
        int CommandsPendingCount,
        bool RequestImmediateSync,
        string? ServerMessage);
}
