namespace EJLive.Core.Models;

/// <summary>
/// Describes the strength of correlation between a normalized vendor event
/// and an Electronic Journal transaction.
/// </summary>
public enum CorrelationMatchStrength
{
    /// <summary>
    /// High-confidence correlation based on exact identifiers such as
    /// TransactionNumber, STAN, RRN, or TransferId.
    /// </summary>
    Strong,

    /// <summary>
    /// Moderate-confidence correlation based on contextual alignment such as
    /// ATM_ID, timestamp window, device class, host message direction,
    /// cassette identifier, or SessionId.
    /// </summary>
    Medium,

    /// <summary>
    /// Low-confidence correlation based on loose proximity such as nearby
    /// timestamp, same file session, error burst, or same device class within
    /// an operational window.
    /// </summary>
    Weak
}
