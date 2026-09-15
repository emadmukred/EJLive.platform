using System;

namespace EJLive.Core.Services
{
    /// <summary>
    /// Composition-root seam for <see cref="IJournalSyncService"/>. Wave 4 (SS-15 debt):
    /// the previous implementation reached the client assembly through
    /// <c>Type.GetType("…, EJLive.Client")</c> + reflection, which SS15 bans ("reflection
    /// used to reach a type that could be a referenced contract"). The inversion is now
    /// explicit: an upper layer that owns a real sync service registers it once at startup
    /// via <see cref="RegisterJournalSyncService"/>; every other context resolves the
    /// documented no-op fallback. This is one of the two sanctioned mutable static
    /// registries (with <c>EjParserRegistry</c>/<c>XfsAdapterRegistry</c>) and it is
    /// append-only after startup to keep it race-free.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly object Gate = new object();
        private static readonly Lazy<IJournalSyncService> FallbackJournalSyncService =
            new(() => new JournalSyncServiceStub(), isThreadSafe: true);
        private static IJournalSyncService? _registered;

        /// <summary>
        /// Registers the live journal-sync implementation. Called by the owning composition
        /// root (client console / server host) before consumers resolve; a second call
        /// replaces the registration only if nothing was registered yet.
        /// </summary>
        public static void RegisterJournalSyncService(IJournalSyncService service)
        {
            ArgumentNullException.ThrowIfNull(service);
            lock (Gate)
            {
                _registered ??= service;
            }
        }

        /// <summary>Test seam: clears the registration so a fixture can assert fallback behaviour.</summary>
        public static void ClearJournalSyncServiceRegistration()
        {
            lock (Gate)
                _registered = null;
        }

        /// <summary>True when a live implementation has been registered (tests assert the seam).</summary>
        public static bool HasJournalSyncService
        {
            get
            {
                lock (Gate)
                    return _registered is not null;
            }
        }

        /// <summary>Resolves the registered service, or the fallback stub in non-client contexts.</summary>
        public static IJournalSyncService GetJournalSyncService()
        {
            lock (Gate)
            {
                return _registered ?? FallbackJournalSyncService.Value;
            }
        }
    }
}
