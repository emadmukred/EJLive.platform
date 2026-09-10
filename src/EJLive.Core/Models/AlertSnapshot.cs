namespace EJLive.Core.Models;

/// <summary>
/// A lightweight snapshot of an active alert for dashboard display.
/// </summary>
public sealed record AlertSnapshot
{
    /// <summary>
    /// Unique identifier of the alert.
    /// </summary>
    public required Guid AlertId { get; init; }

    /// <summary>
    /// Identifier of the ATM that raised the alert.
    /// </summary>
    public required string AtmId { get; init; }

    /// <summary>
    /// Severity level of the alert.
    /// </summary>
    public required AlertSeverity Severity { get; init; }

    /// <summary>
    /// Human-readable alert message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// UTC timestamp when the alert was raised.
    /// </summary>
    public required DateTime TimestampUtc { get; init; }

    /// <summary>
    /// Indicates whether the alert has been acknowledged by an operator.
    /// </summary>
    public required bool Acknowledged { get; init; }
}

// AlertSeverity is defined in UnifiedModels.cs (Info, Warning, Critical, Emergency)
