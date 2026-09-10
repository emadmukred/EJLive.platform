using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class JournalSyncAlertService
{
    private readonly AlertManager _alerts;

    public JournalSyncAlertService(AlertManager alerts)
    {
        _alerts = alerts;
    }

    public JournalSyncAlertService(JournalSyncTracker tracker, TimeSpan? warningNoSyncThreshold = null, TimeSpan? criticalNoSyncThreshold = null)
    {
        _ = tracker;
        _ = warningNoSyncThreshold;
        _ = criticalNoSyncThreshold;
        _alerts = AlertManager.Instance;
    }

    public void Evaluate(JournalSyncRecord record)
    {
        if (record.State == JournalSyncState.Failed)
        {
            _alerts.Raise(
                AlertSeverity.Warning,
                "Journal sync failed",
                record.Message,
                record.ATM_ID,
                record.SyncId);
        }
    }
}
