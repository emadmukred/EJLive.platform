using System;
using System.Collections.Concurrent;
using System.Linq;
using EJLive.Core.Models;

namespace EJLive.Core.Services;

public class JournalSyncTrackingService
{
    private readonly ConcurrentDictionary<string, JournalSyncRecord> _records = new(StringComparer.OrdinalIgnoreCase);

    public JournalSyncTrackingService()
    {
    }

    public JournalSyncTrackingService(string storagePath)
    {
        _ = storagePath;
    }

    public IReadOnlyList<JournalSyncRecord> Records => _records.Values.OrderByDescending(r => r.UpdatedAtUtc).ToArray();

    public void AddOrUpdate(JournalSyncRecord record)
    {
        if (record is null)
            return;

        record.UpdatedAtUtc = DateTime.UtcNow;
        _records.AddOrUpdate(record.SyncId, record, (_, _) => record);
    }

    public IReadOnlyList<JournalSyncRecord> GetByAtm(string atmId)
    {
        return _records.Values
            .Where(r => string.Equals(r.ATM_ID, atmId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.UpdatedAtUtc)
            .ToArray();
    }

    public SyncSummary BuildSummary()
    {
        var snapshot = Records;
        return new SyncSummary
        {
            Total = snapshot.Count,
            Pending = snapshot.Count(r => r.State == JournalSyncState.Pending),
            InProgress = snapshot.Count(r => r.State is JournalSyncState.Syncing or JournalSyncState.ReSyncing),
            Completed = snapshot.Count(r => r.State == JournalSyncState.Completed),
            Failed = snapshot.Count(r => r.State == JournalSyncState.Failed),
            AverageProgress = snapshot.Count == 0 ? 0 : (int)Math.Round(snapshot.Average(r => r.ProgressPercent))
        };
    }
}
