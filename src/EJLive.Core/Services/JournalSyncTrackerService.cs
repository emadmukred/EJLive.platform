namespace EJLive.Core.Services;

public sealed class JournalSyncTrackerService : JournalSyncTrackingService
{
    public JournalSyncTrackerService()
    {
    }

    public JournalSyncTrackerService(string storageRoot)
        : base(storageRoot)
    {
    }

    public void EnsureInitialized()
    {
    }
}
