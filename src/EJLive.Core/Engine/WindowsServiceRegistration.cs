using System.Diagnostics;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Installs, configures, and removes Windows services.
/// Uses sc.exe to interact with the Service Control Manager.
/// No WinForms references; suitable for Core libraries.
/// </summary>
public class WindowsServiceRegistration
{
    private readonly string _auditLogPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsServiceRegistration"/> class.
    /// </summary>
    /// <param name="auditLogPath">Path to the install audit log file.</param>
    public WindowsServiceRegistration(string auditLogPath)
    {
        _auditLogPath = auditLogPath;
    }

    /// <summary>
    /// Installs a Windows service with the specified configuration.
    /// </summary>
    /// <param name="service">Service definition from the install manifest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if installation succeeded; otherwise <c>false</c>.</returns>
    public async Task<bool> InstallAsync(InstallService service, CancellationToken cancellationToken = default)
    {
        WriteAudit($"Installing service '{service.DisplayName}' ({service.ServiceName}) ...");

        var createArgs = $"create \"{service.ServiceName}\" "
            + $"binPath= \"{service.ExecutablePath}\" "
            + $"start= {MapStartType(service.StartType)} "
            + $"DisplayName= \"{service.DisplayName}\"";

        if (!string.IsNullOrWhiteSpace(service.Dependencies))
        {
            createArgs += $" depend= {service.Dependencies}";
        }

        var result = await RunScAsync(createArgs, cancellationToken).ConfigureAwait(false);

        if (result.ExitCode != 0)
        {
            WriteAudit($"ERROR: Failed to create service '{service.ServiceName}'. Exit code: {result.ExitCode}. Output: {result.Output}");
            return false;
        }

        WriteAudit($"Service '{service.ServiceName}' created successfully.");

        // Configure description using the display name as a short description
        var descResult = await RunScAsync(
            $"description \"{service.ServiceName}\" \"{service.DisplayName}\"",
            cancellationToken).ConfigureAwait(false);

        if (descResult.ExitCode != 0)
        {
            WriteAudit($"WARNING: Failed to set description for '{service.ServiceName}'.");
        }

        WriteAudit($"Service '{service.ServiceName}' installation completed.");
        return true;
    }

    /// <summary>
    /// Starts the specified Windows service.
    /// </summary>
    /// <param name="serviceName">Service name used by the SCM.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the service started successfully.</returns>
    public async Task<bool> StartAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        WriteAudit($"Starting service '{serviceName}' ...");

        var result = await RunScAsync($"start \"{serviceName}\"", cancellationToken).ConfigureAwait(false);

        if (result.ExitCode != 0)
        {
            WriteAudit($"ERROR: Failed to start service '{serviceName}'. Exit code: {result.ExitCode}. Output: {result.Output}");
            return false;
        }

        WriteAudit($"Service '{serviceName}' started successfully.");
        return true;
    }

    /// <summary>
    /// Stops the specified Windows service.
    /// </summary>
    /// <param name="serviceName">Service name used by the SCM.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the service stopped successfully.</returns>
    public async Task<bool> StopAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        WriteAudit($"Stopping service '{serviceName}' ...");

        var result = await RunScAsync($"stop \"{serviceName}\"", cancellationToken).ConfigureAwait(false);

        // 1062 = service not started; treat as success for idempotency
        if (result.ExitCode != 0 && result.ExitCode != 1062)
        {
            WriteAudit($"ERROR: Failed to stop service '{serviceName}'. Exit code: {result.ExitCode}. Output: {result.Output}");
            return false;
        }

        WriteAudit($"Service '{serviceName}' stopped (or was not running).");
        return true;
    }

    /// <summary>
    /// Removes the specified Windows service.
    /// </summary>
    /// <param name="serviceName">Service name used by the SCM.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the service was removed successfully.</returns>
    public async Task<bool> UninstallAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        WriteAudit($"Removing service '{serviceName}' ...");

        var result = await RunScAsync($"delete \"{serviceName}\"", cancellationToken).ConfigureAwait(false);

        if (result.ExitCode != 0)
        {
            WriteAudit($"ERROR: Failed to delete service '{serviceName}'. Exit code: {result.ExitCode}. Output: {result.Output}");
            return false;
        }

        WriteAudit($"Service '{serviceName}' removed successfully.");
        return true;
    }

    /// <summary>
    /// Checks whether the specified service exists in the SCM.
    /// </summary>
    /// <param name="serviceName">Service name to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the service exists.</returns>
    public async Task<bool> ExistsAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var result = await RunScAsync($"query \"{serviceName}\"", cancellationToken).ConfigureAwait(false);
        return result.ExitCode == 0;
    }

    private static string MapStartType(string startType)
    {
        return startType.ToUpperInvariant() switch
        {
            "AUTOMATIC" => "auto",
            "MANUAL" => "demand",
            "DISABLED" => "disabled",
            _ => "demand"
        };
    }

    private async Task<ScResult> RunScAsync(string arguments, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo("sc.exe", arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process is null)
        {
            return new ScResult(1, "Failed to start sc.exe");
        }

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        return new ScResult(process.ExitCode, output + error);
    }

    private void WriteAudit(string message)
    {
        var line = $"[{DateTime.UtcNow:O}] {message}";
        Directory.CreateDirectory(Path.GetDirectoryName(_auditLogPath)!);
        File.AppendAllText(_auditLogPath, line + Environment.NewLine);
    }

    private sealed record ScResult(int ExitCode, string Output);
}
