namespace EJLive.Core.Models;

/// <summary>
/// Types of remote sessions supported.
/// </summary>
public enum RemoteSessionType
{
    /// <summary>Shadow session without interaction.</summary>
    Shadow,

    /// <summary>Full RDP session.</summary>
    Rdp,

    /// <summary>Remote assistance session.</summary>
    RemoteAssistance
}

/// <summary>
/// Represents a request for remote access to an ATM session.
/// </summary>
public sealed record RemoteSessionRequest
{
    /// <summary>Unique identifier for the request.</summary>
    public required Guid RequestId { get; init; }

    /// <summary>Identifier of the operator requesting access.</summary>
    public required string OperatorId { get; init; }

    /// <summary>Identifier of the target ATM.</summary>
    public required string AtmId { get; init; }

    /// <summary>Business justification for the remote session.</summary>
    public required string Reason { get; init; }

    /// <summary>Requested duration in minutes.</summary>
    public required int RequestedDurationMinutes { get; init; }

    /// <summary>UTC timestamp when the request was created.</summary>
    public required DateTime RequestedUtc { get; init; }

    /// <summary>UTC timestamp when the request was approved, if applicable.</summary>
    public DateTime? ApprovedUtc { get; init; }

    /// <summary>Identifier of the approver, if applicable.</summary>
    public string? ApprovedBy { get; init; }

    /// <summary>Type of remote session requested.</summary>
    public required RemoteSessionType SessionType { get; init; }
}
