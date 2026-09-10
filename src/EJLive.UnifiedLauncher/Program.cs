using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]

namespace EJLive.UnifiedLauncher;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        var mode = ResolveMode(args);
        switch (mode)
        {
            case LauncherMode.Client:
                RunMode("client", static () => new EJLive.Client.WinForms.ClientMainForm());
                break;
            case LauncherMode.Server:
                RunMode("server", static () => new EJLive.Server.WinForms.ServerMainForm());
                break;
            case LauncherMode.Selection:
                ShowModeSelectionDialog();
                break;
            default:
                ShowUsage();
                break;
        }
    }

    private static LauncherMode ResolveMode(IEnumerable<string> args)
    {
        var first = args.FirstOrDefault(argument => !string.IsNullOrWhiteSpace(argument));
        if (first is null)
            return LauncherMode.Selection;

        return first.Trim().TrimStart('-', '/').ToLowerInvariant() switch
        {
            "client" => LauncherMode.Client,
            "server" => LauncherMode.Server,
            _ => LauncherMode.Invalid
        };
    }

    private static void RunMode(string modeName, Func<Form> formFactory)
    {
        try
        {
            using var form = formFactory();
            Application.Run(form);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to start {modeName} mode: {ex.Message}",
                "EJLive Launcher",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static void ShowModeSelectionDialog()
    {
        var result = MessageBox.Show(
            "Choose the EJLive runtime to start:\n\n" +
            "Yes: Client companion (read-only agent status)\n" +
            "No: Server operations console",
            "EJLive Mode Selection",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        switch (result)
        {
            case DialogResult.Yes:
                RunMode("client", static () => new EJLive.Client.WinForms.ClientMainForm());
                break;
            case DialogResult.No:
                RunMode("server", static () => new EJLive.Server.WinForms.ServerMainForm());
                break;
        }
    }

    private static void ShowUsage()
    {
        MessageBox.Show(
            "Usage: EJLive.UnifiedLauncher.exe [client|server]\n\n" +
            "Prefixing the option with '-', '--', or '/' is also supported.\n" +
            "If no option is supplied, the mode selection dialog is shown.",
            "EJLive Launcher",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private enum LauncherMode
    {
        Invalid,
        Selection,
        Client,
        Server
    }
}
