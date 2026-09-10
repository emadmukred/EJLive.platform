using System;

namespace EJLive.Core.Services
{
    /// <summary>
    /// Backward-compatible journal sync stub.
    /// It delegates to an operational IJournalSyncService so existing callers
    /// receive real behavior instead of no-op logging.
    /// </summary>
    public sealed class JournalSyncServiceStub : IJournalSyncService
    {
        private readonly IJournalSyncService _inner;

        public JournalSyncServiceStub()
            : this(new JournalSyncService())
        {
        }

        public JournalSyncServiceStub(IJournalSyncService inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public bool IsRunning => _inner.IsRunning;

        public void StartSync()
        {
            _inner.StartSync();
        }

        public void StopSync()
        {
            _inner.StopSync();
        }
    }
}
