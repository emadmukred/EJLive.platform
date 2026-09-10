using EJLive.Core.Engine;
using EJLive.Core.Models;

namespace EJLive.Core.Services;

public sealed class JournalSyncService : IJournalSyncService
{
    public JournalOutbox Outbox { get; } = new();
    public bool IsRunning { get; private set; }
    public event EventHandler<LiveSyncProgress>? ProgressChanged;

    public JournalSyncService()
    {
    }

    public JournalSyncService(string storagePath)
    {
        _ = storagePath;
    }

    public void StartSync() => IsRunning = true;
    public void StopSync() => IsRunning = false;

    public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum)
        => Outbox.Enqueue(atmId, fileName, data, offset, checksum);

    public void ReportProgress(LiveSyncProgress progress) => ProgressChanged?.Invoke(this, progress);
}
