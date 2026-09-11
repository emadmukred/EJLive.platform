using System.Diagnostics;
using System.Security.Principal;

namespace EJLive.Client.WinForms.Services;

public sealed class WindowsServiceCommandResult
{
    public bool Success { get; init; }
    public bool RequiresAdministrator { get; init; }
    public int ExitCode { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Registers and manages the EJLive background agent as a standard Windows service.
/// The service remains visible/manageable by administrators through normal Windows tools.
/// </summary>
public static class WindowsServiceRegistrationService
{
    public const string DefaultServiceName = "EJLiveClientAgent";
    public const string DefaultDisplayName = "EJLive Client Agent Service";
    public const string DefaultDescription = "Runs EJLive ATM client synchronization, heartbeat, journal delivery, and remote command processing.";

    public static WindowsServiceCommandResult InstallOrUpdateService(
        string serviceExePath,
        string serviceName = DefaultServiceName,
        string displayName = DefaultDisplayName,
        string description = DefaultDescription)
    {
        if (string.IsNullOrWhiteSpace(serviceExePath) || !File.Exists(serviceExePath))
            return new WindowsServiceCommandResult { Success = false, ExitCode = -10, Message = "Service executable path is missing." };

        if (!IsAdministrator())
            return new WindowsServiceCommandResult { Success = false, RequiresAdministrator = true, ExitCode = -11, Message = "Administrator privileges are required to install/update the service." };

        var quotedPath = $"\"{serviceExePath}\"";
        var createArgs = $"create \"{serviceName}\" binPath= {quotedPath} type= own start= auto obj= LocalSystem DisplayName= \"{displayName}\"";
        var create = RunSc(createArgs);
        if (!create.Success && create.ExitCode != 1073) // 1073: already exists
            return create;

        var config = RunSc($"config \"{serviceName}\" start= auto obj= LocalSystem DisplayName= \"{displayName}\"");
        if (!config.Success)
            return config;

        var desc = RunSc($"description \"{serviceName}\" \"{description}\"");
        if (!desc.Success)
            return desc;

        // Restart on first three failures with 60s delay.
        var failure = RunSc($"failure \"{serviceName}\" reset= 86400 actions= restart/60000/restart/60000/restart/60000");
        if (!failure.Success)
            return failure;

        var serviceQueryConfig = RunSc($"qc \"{serviceName}\"");
        var account = serviceQueryConfig.Success
            ? ExtractServiceStartAccount(serviceQueryConfig.Message)
            : "unknown";

        return new WindowsServiceCommandResult
        {
            Success = true,
            ExitCode = 0,
            Message = $"Service '{serviceName}' is installed/updated and configured for automatic startup (account={account})."
        };
    }

    public static WindowsServiceCommandResult UninstallService(string serviceName = DefaultServiceName)
    {
        if (!IsAdministrator())
            return new WindowsServiceCommandResult { Success = false, RequiresAdministrator = true, ExitCode = -11, Message = "Administrator privileges are required to uninstall the service." };

        _ = RunSc($"stop \"{serviceName}\"");
        return RunSc($"delete \"{serviceName}\"");
    }

    public static WindowsServiceCommandResult StartService(string serviceName = DefaultServiceName)
    {
        if (!IsAdministrator())
            return new WindowsServiceCommandResult { Success = false, RequiresAdministrator = true, ExitCode = -11, Message = "Administrator privileges are required to start the service." };

        return RunSc($"start \"{serviceName}\"");
    }

    public static WindowsServiceCommandResult StopService(string serviceName = DefaultServiceName)
    {
        if (!IsAdministrator())
            return new WindowsServiceCommandResult { Success = false, RequiresAdministrator = true, ExitCode = -11, Message = "Administrator privileges are required to stop the service." };

        return RunSc($"stop \"{serviceName}\"");
    }

    public static WindowsServiceCommandResult QueryService(string serviceName = DefaultServiceName)
    {
        return RunSc($"query \"{serviceName}\"");
    }

    private static WindowsServiceCommandResult RunSc(string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process == null)
                return new WindowsServiceCommandResult { Success = false, ExitCode = -1, Message = "sc.exe did not start." };

            if (!process.WaitForExit(15000))
            {
                try { process.Kill(); } catch { }
                return new WindowsServiceCommandResult { Success = false, ExitCode = -2, Message = "sc.exe timed out." };
            }

            var output = (process.StandardOutput.ReadToEnd() + " " + process.StandardError.ReadToEnd()).Trim();
            return new WindowsServiceCommandResult
            {
                Success = process.ExitCode == 0,
                ExitCode = process.ExitCode,
                Message = string.IsNullOrWhiteSpace(output) ? $"sc.exe exit={process.ExitCode}" : output
            };
        }
        catch (Exception ex)
        {
            return new WindowsServiceCommandResult { Success = false, ExitCode = -3, Message = ex.Message };
        }
    }

    private static bool IsAdministrator()
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

    private static string ExtractServiceStartAccount(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return "unknown";

        foreach (var rawLine in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            if (!line.Contains("SERVICE_START_NAME", StringComparison.OrdinalIgnoreCase))
                continue;

            var idx = line.IndexOf(':');
            if (idx >= 0 && idx + 1 < line.Length)
            {
                var value = line[(idx + 1)..].Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        return "unknown";
    }
}
