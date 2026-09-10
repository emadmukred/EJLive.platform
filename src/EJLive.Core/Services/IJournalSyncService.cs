namespace EJLive.Core.Services
{
    public interface IJournalSyncService
    {
        void StartSync();
        void StopSync();
        bool IsRunning { get; }
    }
}
