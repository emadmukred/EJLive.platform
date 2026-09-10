using System.Security.Principal;
using EJLive.Client.WinForms.Services;

namespace EJLive.Client.WinForms.Agent;

/// <summary>
/// Handles elevation checks and startup registration for background agent mode.
/// </summary>
public static class AutoAdminSetup
{
    public static bool IsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    public static bool RequiresElevationForBackground(IEnumerable<string>? args)
    {
        return ClientStartupPlanner.ContainsAgentArgument(args) && !IsAdministrator();
    }

    public static StartupRegistrationResult EnsureStartupTask(string? processPath)
    {
        if (string.IsNullOrWhiteSpace(processPath) || !File.Exists(processPath))
        {
            return new StartupRegistrationResult
            {
                Success = false,
                Message = "Executable path is missing."
            };
        }

        return WindowsStartupService.RegisterClientAutostart(processPath);
    }
}
