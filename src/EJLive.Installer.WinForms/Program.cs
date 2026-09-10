using EJLive.Application.Installation;

namespace EJLive.Installer.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        if (args is { Length: > 0 })
        {
            var exitCode = RunSilentMode(args);
            if (exitCode >= 0)
            {
                Environment.ExitCode = exitCode;
                return;
            }
        }

        ApplicationConfiguration.Initialize();
        System.Windows.Forms.Application.Run(new InstallerForm());
    }

    private static int RunSilentMode(string[] args)
    {
        var normalized = args
            .Where(arg => !string.IsNullOrWhiteSpace(arg))
            .Select(arg => arg.Trim().ToLowerInvariant())
            .ToArray();

        if (normalized.Contains("--silent-install"))
        {
            var result = InstallerAutomationRunner.RunInstall();
            return result.Success ? 0 : (result.ExitCode == 0 ? 1 : result.ExitCode);
        }

        if (normalized.Contains("--silent-uninstall"))
        {
            var result = InstallerAutomationRunner.RunUninstall();
            return result.Success ? 0 : (result.ExitCode == 0 ? 1 : result.ExitCode);
        }

        if (normalized.Contains("--silent-validate"))
        {
            var result = InstallerAutomationRunner.RunValidate();
            return result.Success ? 0 : (result.ExitCode == 0 ? 1 : result.ExitCode);
        }

        return -1;
    }
}
