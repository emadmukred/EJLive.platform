namespace EJLive.Core.Services;

public sealed class JournalSyncTracker : JournalSyncTrackingService
{
    public JournalSyncTracker()
    {
    }

    public JournalSyncTracker(string persistFilePath)
        : base(persistFilePath)
    {
    }
}
