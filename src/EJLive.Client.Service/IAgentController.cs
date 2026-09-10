using System;

namespace EJLive.Client.Service;

/// <summary>
/// Lifecycle states for the headless EJLive client agent.
/// Kept intentionally small so service hosting, health reporting, and tests
/// and future adapters can depend on it without taking UI dependencies.
/// </summary>
public enum AgentControllerState
{
    Stopped,
    Starting,
    Running,
    Paused,
    Failed
}

/// <summary>
/// Immutable operational snapshot for the EJLive headless agent.
/// This is the single contract consumed by Windows Service hosting,
/// health reporter, telemetry, verification, and future dashboards.
/// </summary>
public sealed record AgentStatus(
    AgentControllerState State,
    bool Connected,
    bool HandshakeComplete,
    int PendingOutboxItems,
    long TotalBytesSent,
    long TotalBytesReceived,
    DateTime? LastHeartbeatUtc,
    DateTime? LastJournalSyncUtc,
    string? SessionId,
    string? LastError);

/// <summary>
/// Headless agent controller contract.
/// Implementations must remain free from WinForms/WPF/UI framework references.
/// Production client service must only expose status/telemetry and execute
/// explicitly authorized server-owned operations.
/// </summary>
public interface IAgentController : IDisposable
{
    string AtmId { get; }

    event Action<string>? OnLog;
    event Action<AgentStatus>? OnStatusUpdate;

    void StartAll();
    void StopAll();

    AgentStatus GetStatus();

    void ForceJournalSync();
    void ForceLogBackup();
}
