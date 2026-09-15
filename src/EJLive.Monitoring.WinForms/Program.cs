using System.Threading.Tasks;
using EJLive.Core.Data;

namespace EJLive.Monitoring.WinForms;

internal static class Program
{
    /// <summary>
    /// NOC / Windows Operations Console entry point. The console reads server data through
    /// snapshots, but keeps the shared bootstrap (SS-20) for the local report/log tree under
    /// the central dataroot so exports, runtime logs and the studio launcher land in the
    /// fixed central location. Failure is a rendered warning, never a crash (SS-14).
    /// Startup-sequential wait, not a UI-thread sync-over-async (SS-12).
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var outcome = Task.Run(() => new PlatformBootstrap().RunAsync()).GetAwaiter().GetResult();
        if (!outcome.Success)
        {
            MessageBox.Show(
                "Local platform data could not be prepared:\n\n" + outcome.FailureDetail +
                "\n\nFleet monitoring continues against the server; local exports may be unavailable.",
                "EJLive Operations Console",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        Application.Run(new MainDashboardForm());
    }
}
