namespace EJLive.Core.Models;

/// <summary>
/// Possible outcomes of a remote session.
/// </summary>
public enum RemoteSessionOutcome
{
    /// <summary>Session completed successfully.</summary>
    Success,

    /// <summary>Session was stopped by operator or system.</summary>
    Stopped,

    /// <summary>Session exceeded requested duration and timed out.</summary>
    TimedOut,

    /// <summary>Session failed to start or was rejected.</summary>
    Failed,

    /// <summary>Session was denied by policy or approval workflow.</summary>
    Denied
}

/// <summary>
/// Represents an audit record for a completed or terminated remote session.
/// </summary>
public sealed record RemoteSessionAudit
{
    /// <summary>Reference to the original request.</summary>
    public required Guid RequestId { get; init; }

    /// <summary>Unique identifier for the session execution.</summary>
    public required Guid SessionId { get; init; }

    /// <summary>UTC timestamp when the session started.</summary>
    public required DateTime StartUtc { get; init; }

    /// <summary>UTC timestamp when the session ended, if applicable.</summary>
    public DateTime? EndUtc { get; init; }

    /// <summary>Final outcome of the session.</summary>
    public required RemoteSessionOutcome Outcome { get; init; }

    /// <summary>Reason for session termination, if applicable.</summary>
    public string? StopReason { get; init; }
}
