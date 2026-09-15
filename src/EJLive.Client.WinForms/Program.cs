using System.Threading.Tasks;
using EJLive.Core.Data;

namespace EJLive.Client.WinForms;

internal static class Program
{
    /// <summary>
    /// Endpoint Console entry point. The console is the session companion of the headless
    /// service (SS3): it still runs the shared bootstrap so its views (offset grid, outbox,
    /// journal tail) read the same dataroot the service writes, but a bootstrap failure is a
    /// warning here — the console must open to show the operator what is wrong.
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
                "\n\nThe console stays available for diagnostics; the agent service owns capture.",
                "EJLive Endpoint Console",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        Application.Run(new ClientMainForm());
    }
}
