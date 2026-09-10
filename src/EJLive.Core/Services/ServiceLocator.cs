using System;
using System.Reflection;

namespace EJLive.Core.Services
{
    public static class ServiceLocator
    {
        private static readonly Lazy<IJournalSyncService> FallbackJournalSyncService =
            new(() => new JournalSyncServiceStub(), isThreadSafe: true);

        public static IJournalSyncService? GetJournalSyncService()
        {
            try
            {
                // Resolve from the client registry when the WinForms runtime is active.
                var registryType = Type.GetType("EJLive.Client.WinForms.Services.ServiceRegistry, EJLive.Client", throwOnError: false);
                if (registryType != null)
                {
                    var method = registryType.GetMethod("Get", BindingFlags.Public | BindingFlags.Static);
                    if (method != null && method.IsGenericMethodDefinition)
                    {
                        var generic = method.MakeGenericMethod(typeof(IJournalSyncService));
                        var resolved = generic.Invoke(null, null) as IJournalSyncService;
                        if (resolved != null)
                            return resolved;
                    }
                }
            }
            catch
            {
                // Non-client contexts are expected to fall back safely.
            }

            // Safe singleton fallback for non-client contexts (tests, verification, tooling).
            return FallbackJournalSyncService.Value;
        }
    }
}
