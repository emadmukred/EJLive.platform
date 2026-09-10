namespace EJLive.Core.Models;

/// <summary>
/// Represents a vendor-neutral normalized event extracted from an XFS log or vendor trace.
/// </summary>
/// <param name="EventId">Unique identifier for this normalized event.</param>
/// <param name="ATM_ID">The ATM identifier where the event originated.</param>
/// <param name="Vendor">The vendor name (e.g., NCR, Diebold, Wincor).</param>
/// <param name="DeviceClass">The device class involved (e.g., CashDispenser, CardReader, Printer).</param>
/// <param name="Severity">The severity level of the event (e.g., Info, Warning, Error, Fatal).</param>
/// <param name="Code">The vendor-specific or standardized event code.</param>
/// <param name="Message">The human-readable event message.</param>
/// <param name="Timestamp">The event timestamp (UTC when possible).</param>
/// <param name="RawLine">The original unparsed log line.</param>
/// <param name="SourceFile">The path or name of the source log file.</param>
/// <param name="ConfidenceScore">Confidence (0.0 to 1.0) that this event was correctly normalized.</param>
/// <param name="ImpactedTransactionId">The correlated transaction ID, if known.</param>
/// <param name="CorrelationReason">Human-readable explanation of how this event was correlated.</param>
/// <param name="FalsePositiveRisk">Estimated risk (0.0 to 1.0) that this correlation is a false positive.</param>
/// <param name="OperatorExplanation">Suggested explanation for operators or investigators.</param>
public sealed record NormalizedVendorEvent(
    string EventId,
    string ATM_ID,
    string Vendor,
    string DeviceClass,
    string Severity,
    string Code,
    string Message,
    DateTime Timestamp,
    string RawLine,
    string SourceFile,
    double ConfidenceScore,
    string ImpactedTransactionId,
    string CorrelationReason,
    double FalsePositiveRisk,
    string OperatorExplanation
);
