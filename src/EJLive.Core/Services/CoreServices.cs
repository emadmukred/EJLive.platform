using System.Data;
using System.Data.SQLite;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Xml.Linq;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Core.Services;



public sealed class AlertManager
{
    private readonly List<AlertPayload> _alerts = new();
    public event EventHandler<AlertPayload>? AlertRaised;
    public IReadOnlyList<AlertPayload> Alerts => _alerts;

    public AlertPayload Raise(AlertSeverity severity, string title, string message, string source, string dedupeKey = "")
    {
        var existing = !string.IsNullOrWhiteSpace(dedupeKey)
            ? _alerts.FirstOrDefault(a => a.DedupeKey == dedupeKey && !a.IsRead)
            : null;
        if (existing is not null)
            return existing;

        var alert = new AlertPayload { Severity = severity, Title = title, Message = message, Source = source, DedupeKey = dedupeKey };
        _alerts.Add(alert);
        AlertRaised?.Invoke(this, alert);
        return alert;
    }
}




public sealed class JournalSyncTracker : JournalSyncTrackingService
{
}

public sealed class JournalSyncTrackerService : JournalSyncTrackingService
{
}

public sealed class JournalSyncStateStore
{
    public JournalSyncTrackingService Tracking { get; } = new();
}



public sealed class JournalSyncService : IJournalSyncService
{
    public JournalOutbox Outbox { get; } = new();
    public bool IsRunning { get; private set; }
    public event EventHandler<LiveSyncProgress>? ProgressChanged;
    public void StartSync() => IsRunning = true;
    public void StopSync() => IsRunning = false;
    public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum) => Outbox.Enqueue(atmId, fileName, data, offset, checksum);
    public void ReportProgress(LiveSyncProgress progress) => ProgressChanged?.Invoke(this, progress);
}

public sealed class JournalSyncServiceStub : IJournalSyncService
{
    public bool IsRunning { get; private set; }
    public void StartSync() => IsRunning = true;
    public void StopSync() => IsRunning = false;
}


public sealed class JournalSyncDashboardService
{
    public object BuildSummary(IEnumerable<JournalSyncRecord> records)
    {
        var list = records.ToList();
        return new
        {
            Total = list.Count,
            Completed = list.Count(r => r.State == JournalSyncState.Completed),
            Failed = list.Count(r => r.State == JournalSyncState.Failed),
            Pending = list.Count(r => r.State == JournalSyncState.Pending)
        };
    }
}

public sealed class JournalSyncAlertService
{
    private readonly AlertManager _alerts;
    public JournalSyncAlertService(AlertManager alerts) => _alerts = alerts;
    public void Evaluate(JournalSyncRecord record)
    {
        if (record.State == JournalSyncState.Failed)
            _alerts.Raise(AlertSeverity.Warning, "Journal sync failed", record.Message, record.ATM_ID, record.SyncId);
    }
}




public sealed class VendorRootProfileCatalogService : VendorRootCapabilityService
{
}




