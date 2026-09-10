namespace EJLive.Core.Models;

/// <summary>
/// Fleet-level key performance indicators for the NOC dashboard.
/// </summary>
public sealed record FleetKpi
{
    /// <summary>
    /// Number of ATMs currently online.
    /// </summary>
    public required int OnlineCount { get; init; }

    /// <summary>
    /// Number of ATMs currently offline.
    /// </summary>
    public required int OfflineCount { get; init; }

    /// <summary>
    /// Number of ATMs reporting warning conditions.
    /// </summary>
    public required int WarningCount { get; init; }

    /// <summary>
    /// Total number of ATMs in the fleet.
    /// </summary>
    public required int TotalAtms { get; init; }
}

/// <summary>
/// Synchronization-level key performance indicators for the NOC dashboard.
/// </summary>
public sealed record SyncKpi
{
    /// <summary>
    /// Percentage of successful sync operations in the last hour (0-100).
    /// </summary>
    public required double SuccessRate { get; init; }

    /// <summary>
    /// Average latency of sync operations in milliseconds.
    /// </summary>
    public required double AverageLatency { get; init; }

    /// <summary>
    /// Number of sync failures in the last hour.
    /// </summary>
    public required int FailuresLastHour { get; init; }
}

/// <summary>
/// Filter criteria available on the NOC dashboard.
/// </summary>
public sealed record DashboardFilters
{
    /// <summary>
    /// Vendor name filter (e.g., "Diebold", "NCR").
    /// </summary>
    public string? Vendor { get; init; }

    /// <summary>
    /// Geographic region filter.
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// ATM operational status filter.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Alert severity filter.
    /// </summary>
    public AlertSeverity? Severity { get; init; }
}

/// <summary>
/// Lightweight view model for the NOC dashboard. The UI reads this snapshot
/// directly and never queries the database itself.
/// </summary>
public sealed record DashboardViewModel
{
    /// <summary>
    /// Fleet-wide KPIs.
    /// </summary>
    public required FleetKpi FleetKpi { get; init; }

    /// <summary>
    /// Synchronization KPIs.
    /// </summary>
    public required SyncKpi SyncKpi { get; init; }

    /// <summary>
    /// Active alerts visible to the operator.
    /// </summary>
    public required IReadOnlyList<AlertSnapshot> Alerts { get; init; }

    /// <summary>
    /// Available filter dimensions and current selections.
    /// </summary>
    public required DashboardFilters Filters { get; init; }

    /// <summary>
    /// UTC timestamp when this snapshot was generated.
    /// </summary>
    public required DateTime SnapshotTimestampUtc { get; init; }
}
