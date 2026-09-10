namespace EJLive.Core.Models;

/// <summary>
/// Defines the types of reports available in the NOC dashboard.
/// </summary>
public enum ReportType
{
    Daily,
    Monthly,
    AtmDetails,
    SyncFailures,
    CommandAudit
}

/// <summary>
/// A request to generate an operational or audit report.
/// </summary>
public sealed record ReportRequest
{
    /// <summary>
    /// The type of report to generate.
    /// </summary>
    public required ReportType ReportType { get; init; }

    /// <summary>
    /// JSON-encoded filter criteria (vendor, region, status, date range, etc.).
    /// </summary>
    public required string FilterJson { get; init; }

    /// <summary>
    /// Username or identifier of the operator requesting the report.
    /// </summary>
    public required string RequestedBy { get; init; }

    /// <summary>
    /// UTC timestamp when the request was submitted.
    /// </summary>
    public required DateTime RequestedUtc { get; init; }
}
