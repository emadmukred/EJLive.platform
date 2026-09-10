namespace EJLive.Core.Services;

public sealed class JournalSyncStateStore
{
    public JournalSyncTrackingService Tracking { get; } = new();

    public JournalSyncStateStore()
    {
    }

    public JournalSyncStateStore(string storagePath)
    {
        _ = storagePath;
    }
}
