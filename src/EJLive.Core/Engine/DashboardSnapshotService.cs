using System.Collections.Concurrent;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Generates and caches a server-side <see cref="DashboardViewModel"/> snapshot.
/// The UI reads the cached snapshot and never queries the database directly.
/// Designed to support 500 simulated ATMs and 100,000 telemetry events.
/// </summary>
public class DashboardSnapshotService
{
    private readonly object _snapshotLock = new();
    private DashboardViewModel? _cachedSnapshot;
    private DateTime _lastGeneratedUtc = DateTime.MinValue;

    /// <summary>
    /// Maximum age of the cached snapshot before regeneration is required.
    /// </summary>
    public TimeSpan CacheTtl { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the current dashboard snapshot, using the cache if it is still fresh.
    /// </summary>
    /// <param name="filters">Optional filter criteria to apply to the snapshot.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="DashboardViewModel"/> representing the current state.</returns>
    public Task<DashboardViewModel> GetSnapshotAsync(
        DashboardFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        lock (_snapshotLock)
        {
            if (_cachedSnapshot is not null
                && DateTime.UtcNow - _lastGeneratedUtc < CacheTtl)
            {
                return Task.FromResult(ApplyFilters(_cachedSnapshot, filters));
            }
        }

        return BuildAndCacheSnapshotAsync(filters, cancellationToken);
    }

    /// <summary>
    /// Invalidates the cached snapshot, forcing regeneration on the next request.
    /// </summary>
    public void InvalidateCache()
    {
        lock (_snapshotLock)
        {
            _cachedSnapshot = null;
            _lastGeneratedUtc = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Builds a new snapshot from the database, caches it, and returns the filtered view.
    /// </summary>
    private async Task<DashboardViewModel> BuildAndCacheSnapshotAsync(
        DashboardFilters? filters,
        CancellationToken cancellationToken)
    {
        // Simulate async DB read. In production this queries the telemetry and ATM tables.
        var snapshot = await ReadDatabaseAsync(cancellationToken).ConfigureAwait(false);

        lock (_snapshotLock)
        {
            _cachedSnapshot = snapshot;
            _lastGeneratedUtc = DateTime.UtcNow;
        }

        return ApplyFilters(snapshot, filters);
    }

    /// <summary>
    /// Simulates a database read optimized for 500 ATMs and 100k telemetry events.
    /// In production this would execute aggregated SQL queries.
    /// </summary>
    private static Task<DashboardViewModel> ReadDatabaseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Simulated aggregated data representing a fleet of 500 ATMs
        var fleetKpi = new FleetKpi
        {
            OnlineCount = 482,
            OfflineCount = 12,
            WarningCount = 6,
            TotalAtms = 500
        };

        var syncKpi = new SyncKpi
        {
            SuccessRate = 99.82,
            AverageLatency = 142.5,
            FailuresLastHour = 3
        };

        var alerts = new List<AlertSnapshot>
        {
            new()
            {
                AlertId = Guid.NewGuid(),
                AtmId = "ATM-001",
                Severity = AlertSeverity.Critical,
                Message = "Cash dispenser module fault",
                TimestampUtc = DateTime.UtcNow.AddMinutes(-12),
                Acknowledged = false
            },
            new()
            {
                AlertId = Guid.NewGuid(),
                AtmId = "ATM-089",
                Severity = AlertSeverity.Warning,
                Message = "Low paper warning",
                TimestampUtc = DateTime.UtcNow.AddMinutes(-45),
                Acknowledged = false
            }
        };

        var viewModel = new DashboardViewModel
        {
            FleetKpi = fleetKpi,
            SyncKpi = syncKpi,
            Alerts = alerts.AsReadOnly(),
            Filters = new DashboardFilters(),
            SnapshotTimestampUtc = DateTime.UtcNow
        };

        return Task.FromResult(viewModel);
    }

    /// <summary>
    /// Applies client-supplied filters to a snapshot without mutating the cache.
    /// </summary>
    private static DashboardViewModel ApplyFilters(
        DashboardViewModel source,
        DashboardFilters? filters)
    {
        if (filters is null)
        {
            return source;
        }

        var filteredAlerts = source.Alerts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filters.Vendor))
        {
            // Vendor is not stored on AlertSnapshot; in production this would join ATM metadata.
            // For now we simulate by treating the filter as a no-op on alerts.
        }

        if (!string.IsNullOrWhiteSpace(filters.Region))
        {
            // Region is not stored on AlertSnapshot; same pattern as vendor.
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            // Status filter applies to ATM state, not individual alerts.
        }

        if (filters.Severity.HasValue)
        {
            filteredAlerts = filteredAlerts.Where(a => a.Severity == filters.Severity.Value);
        }

        return source with
        {
            Alerts = filteredAlerts.ToList().AsReadOnly(),
            Filters = filters
        };
    }
}
