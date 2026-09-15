using System.Threading.Tasks;
using EJLive.Core.Data;

namespace EJLive.Server.WinForms;

internal static class Program
{
    /// <summary>
    /// Enterprise Server entry point. SS-20: the platform bootstrap (dataroot → directories
    /// → configuration → database → schema book) runs before any engine starts, once, in
    /// the one machine shared with the service and the NOC console. A failed step is rendered
    /// as its actionable line (SS-14) — the host then continues read-only so an operator can
    /// see why. The wait here is startup-sequential (no message loop yet), not a UI-thread
    /// sync-over-async (SS-12).
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var outcome = Task.Run(() => new PlatformBootstrap().RunAsync()).GetAwaiter().GetResult();
        if (!outcome.Success)
        {
            MessageBox.Show(
                "Platform bootstrap failed:\n\n" + outcome.FailureDetail +
                "\n\nThe server starts in degraded mode. Fix the reported step and restart.",
                "EJLive Enterprise Server",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        // Wave 6 / C-34 — one host, one monitoring surface: `--noc` lands the operator
        // directly on the unified NOC Monitoring tab. This replaces the retired
        // EJLive.Monitoring.exe entry point (the packaged noc.cmd calls it).
        var console = new ServerMainForm();
        if (args.Any(StartOnNocConsole))
            console.FocusNocConsole();

        Application.Run(console);
    }

    /// <summary>
    /// Accepts <c>--noc</c>, <c>-noc</c>, <c>/noc</c>, <c>--monitoring</c>: the two names
    /// operators used for the standalone console both open the same unified tab.
    /// </summary>
    private static bool StartOnNocConsole(string argument)
    {
        return argument.Trim().TrimStart('-', '/').ToLowerInvariant() is "noc" or "monitoring";
    }
}
