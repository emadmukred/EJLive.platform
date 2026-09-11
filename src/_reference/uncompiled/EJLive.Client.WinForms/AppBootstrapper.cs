using System.IO;
using EJLive.Client.WinForms.Services;
using EJLive.Core.Services;

namespace EJLive.Client.WinForms
{
    public static class AppBootstrapper
    {
        public static void Init()
        {
            // try Resources\labels_en.json then root labels_en.json
            var baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var labelsPath = Path.Combine(baseDir, "Resources", "labels_en.json");
            if (!File.Exists(labelsPath))
                labelsPath = Path.Combine(baseDir, "labels_en.json");

            var labelService = new LabelMappingService(labelsPath);
            ServiceRegistry.Register(labelService);

            var journalSync = new JournalSyncService();
            journalSync.StartSync();
            ServiceRegistry.Register<IJournalSyncService>(journalSync);
            ServiceRegistry.Register(journalSync);
        }
    }
}
