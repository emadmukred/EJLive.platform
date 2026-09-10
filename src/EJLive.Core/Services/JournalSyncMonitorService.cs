using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class JournalSyncMonitorService
{
    public IEnumerable<LiveSyncProgress> GetLiveProgress(IEnumerable<JournalSyncRecord> records)
    {
        return (records ?? Array.Empty<JournalSyncRecord>())
            .Select(r => new LiveSyncProgress
            {
                ATM_ID = r.ATM_ID,
                FileName = r.FileName,
                BytesSent = r.FileOffset,
                TotalBytes = r.FileSize,
                Status = SyncStatus.InProgress
            });
    }
}
