using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class JournalSyncDashboardService
{
    public JournalSyncDashboardService()
    {
    }

    public JournalSyncDashboardService(JournalSyncTracker tracker, JournalSyncAlertService alertService, VendorRootProfileCatalogService rootProfileCatalog)
    {
        _ = tracker;
        _ = alertService;
        _ = rootProfileCatalog;
    }

    public object BuildSummary(IEnumerable<JournalSyncRecord> records)
    {
        var list = (records ?? Array.Empty<JournalSyncRecord>()).ToList();
        return new
        {
            Total = list.Count,
            Completed = list.Count(r => r.State == JournalSyncState.Completed),
            Failed = list.Count(r => r.State == JournalSyncState.Failed),
            Pending = list.Count(r => r.State == JournalSyncState.Pending)
        };
    }
}
