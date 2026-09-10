using System.Text.Json;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Orchestrates install, upgrade, and uninstall operations.
/// Validates prerequisites, executes the install manifest, and supports
/// rollback to the last known good state on failure.
/// </summary>
public class InstallerEngine
{
    private readonly WindowsServiceRegistration _serviceRegistration;
    private readonly string _installRoot;
    private readonly string _auditLogPath;
    private readonly string _lastKnownGoodFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallerEngine"/> class.
    /// </summary>
    /// <param name="installRoot">Root directory for installation.</param>
    /// <param name="auditLogPath">Path to the install audit log.</param>
    public InstallerEngine(string installRoot, string auditLogPath)
    {
        _installRoot = installRoot;
        _auditLogPath = auditLogPath;
        _serviceRegistration = new WindowsServiceRegistration(auditLogPath);
        _lastKnownGoodFile = Path.Combine(installRoot, "last-known-good.json");
    }

    /// <summary>
    /// Installs or upgrades the application using the provided manifest.
    /// </summary>
    /// <param name="manifest">The install manifest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="InstallResult"/> describing the outcome.</returns>
    public async Task<InstallResult> InstallAsync(InstallManifest manifest, CancellationToken cancellationToken = default)
    {
        WriteAudit($"Starting installation for manifest {manifest.ManifestId}, version {manifest.Version}.");

        var validation = ValidatePrerequisites(manifest);
        if (!validation.IsValid)
        {
            WriteAudit($"Prerequisite validation failed: {validation.Message}");
            return InstallResult.Failed(validation.Message);
        }

        // Save current state as last-known-good before mutating anything
        await SaveLastKnownGoodAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await EnsurePathsAsync(manifest.Paths, cancellationToken).ConfigureAwait(false);
            await InstallComponentsAsync(manifest.Components, cancellationToken).ConfigureAwait(false);
            await RegisterServicesAsync(manifest.Services, cancellationToken).ConfigureAwait(false);

            WriteAudit($"Installation completed successfully for version {manifest.Version}.");
            return InstallResult.Succeeded(manifest.Version);
        }
        catch (Exception ex)
        {
            WriteAudit($"Installation failed: {ex.Message}. Initiating rollback.");
            await RollbackAsync(cancellationToken).ConfigureAwait(false);
            return InstallResult.Failed($"Installation failed and was rolled back. {ex.Message}");
        }
    }

    /// <summary>
    /// Uninstalls the application, removing files, services, and directories.
    /// </summary>
    /// <param name="manifest">The manifest describing what was installed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="InstallResult"/> describing the outcome.</returns>
    public async Task<InstallResult> UninstallAsync(InstallManifest manifest, CancellationToken cancellationToken = default)
    {
        WriteAudit($"Starting uninstall for manifest {manifest.ManifestId}, version {manifest.Version}.");

        try
        {
            // Stop and remove services in reverse order
            foreach (var service in manifest.Services.Reverse())
            {
                if (await _serviceRegistration.ExistsAsync(service.ServiceName, cancellationToken).ConfigureAwait(false))
                {
                    await _serviceRegistration.StopAsync(service.ServiceName, cancellationToken).ConfigureAwait(false);
                    await _serviceRegistration.UninstallAsync(service.ServiceName, cancellationToken).ConfigureAwait(false);
                }
            }

            // Remove components (files)
            foreach (var component in manifest.Components.Reverse())
            {
                if (File.Exists(component.Destination))
                {
                    File.Delete(component.Destination);
                    WriteAudit($"Deleted file: {component.Destination}");
                }
            }

            // Remove empty directories under install root
            foreach (var pathDef in manifest.Paths.Reverse())
            {
                if (Directory.Exists(pathDef.Path) && IsUnderInstallRoot(pathDef.Path))
                {
                    try
                    {
                        Directory.Delete(pathDef.Path, recursive: true);
                        WriteAudit($"Deleted directory: {pathDef.Path}");
                    }
                    catch (IOException ex)
                    {
                        WriteAudit($"WARNING: Could not delete directory {pathDef.Path}: {ex.Message}");
                    }
                }
            }

            WriteAudit("Uninstall completed successfully.");
            return InstallResult.Succeeded(manifest.Version);
        }
        catch (Exception ex)
        {
            WriteAudit($"Uninstall failed: {ex.Message}");
            return InstallResult.Failed($"Uninstall failed. {ex.Message}");
        }
    }

    /// <summary>
    /// Validates prerequisites defined in the manifest.
    /// Checks .NET Desktop Runtime, permissions, paths, and ports.
    /// </summary>
    private PrerequisiteValidation ValidatePrerequisites(InstallManifest manifest)
    {
        foreach (var prereq in manifest.Prerequisites)
        {
            bool satisfied = prereq.CheckType.ToUpperInvariant() switch
            {
                "FRAMEWORK" => ValidateFramework(prereq.CheckValue),
                "REGISTRY" => ValidateRegistry(prereq.CheckValue),
                "FILE" => File.Exists(prereq.CheckValue),
                "PORT" => ValidatePort(prereq.CheckValue),
                "PERMISSIONS" => ValidatePermissions(prereq.CheckValue),
                _ => false
            };

            if (!satisfied)
            {
                var message = $"Prerequisite '{prereq.Name}' ({prereq.CheckType}) not satisfied: {prereq.CheckValue}";
                if (prereq.IsMandatory)
                {
                    return PrerequisiteValidation.Failed(message);
                }

                WriteAudit($"WARNING: Optional prerequisite not satisfied: {prereq.Name}");
            }
        }

        return PrerequisiteValidation.Passed();
    }

    private static bool ValidateFramework(string version)
    {
        // Simplified check: look for common .NET Desktop Runtime registry keys
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

            if (key is null) return false;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                var displayName = subKey?.GetValue("DisplayName")?.ToString() ?? string.Empty;
                if (displayName.Contains(".NET", StringComparison.OrdinalIgnoreCase)
                    && displayName.Contains(version, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch
        {
            // Registry access may be restricted
        }

        return false;
    }

    private static bool ValidateRegistry(string path)
    {
        try
        {
            var parts = path.Split('\\', 2);
            var hive = parts[0].ToUpperInvariant() switch
            {
                "HKLM" => Microsoft.Win32.Registry.LocalMachine,
                "HKCU" => Microsoft.Win32.Registry.CurrentUser,
                _ => null
            };

            if (hive is null || parts.Length < 2) return false;

            using var key = hive.OpenSubKey(parts[1]);
            return key is not null;
        }
        catch
        {
            return false;
        }
    }

    private static bool ValidatePort(string portValue)
    {
        if (!int.TryParse(portValue, out var port))
        {
            return false;
        }

        try
        {
            var listener = System.Net.Sockets.TcpListener.Create(port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool ValidatePermissions(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var testFile = Path.Combine(path, $".write-test-{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFile, string.Empty);
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task EnsurePathsAsync(IReadOnlyList<InstallPath> paths, CancellationToken cancellationToken)
    {
        foreach (var pathDef in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(pathDef.Path) && pathDef.CreateIfMissing)
            {
                Directory.CreateDirectory(pathDef.Path);
                WriteAudit($"Created directory: {pathDef.Path}");
            }
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private async Task InstallComponentsAsync(IReadOnlyList<InstallComponent> components, CancellationToken cancellationToken)
    {
        foreach (var component in components)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Directory.CreateDirectory(Path.GetDirectoryName(component.Destination)!);

            // In production this would copy from a payload stream or network source.
            // For the engine contract we simulate with an empty placeholder.
            await File.WriteAllTextAsync(component.Destination, string.Empty, cancellationToken).ConfigureAwait(false);

            WriteAudit($"Installed component: {component.Name} -> {component.Destination}");
        }
    }

    private async Task RegisterServicesAsync(IReadOnlyList<InstallService> services, CancellationToken cancellationToken)
    {
        foreach (var service in services)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await _serviceRegistration.ExistsAsync(service.ServiceName, cancellationToken).ConfigureAwait(false))
            {
                WriteAudit($"Service '{service.ServiceName}' already exists; skipping creation.");
                continue;
            }

            var created = await _serviceRegistration.InstallAsync(service, cancellationToken).ConfigureAwait(false);
            if (!created)
            {
                throw new InvalidOperationException($"Failed to install service '{service.ServiceName}'.");
            }

            var started = await _serviceRegistration.StartAsync(service.ServiceName, cancellationToken).ConfigureAwait(false);
            if (!started)
            {
                throw new InvalidOperationException($"Failed to start service '{service.ServiceName}'.");
            }
        }
    }

    private async Task SaveLastKnownGoodAsync(CancellationToken cancellationToken)
    {
        var state = new LastKnownGoodState
        {
            TimestampUtc = DateTime.UtcNow,
            InstallRoot = _installRoot,
            Files = Directory.Exists(_installRoot)
                ? Directory.GetFiles(_installRoot, "*", SearchOption.AllDirectories).ToList()
                : new List<string>()
        };

        var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_lastKnownGoodFile, json, cancellationToken).ConfigureAwait(false);

        WriteAudit("Saved last-known-good state.");
    }

    private async Task RollbackAsync(CancellationToken cancellationToken)
    {
        WriteAudit("Rollback initiated.");

        if (!File.Exists(_lastKnownGoodFile))
        {
            WriteAudit("No last-known-good state found; rollback aborted.");
            return;
        }

        var json = await File.ReadAllTextAsync(_lastKnownGoodFile, cancellationToken).ConfigureAwait(false);
        var state = JsonSerializer.Deserialize<LastKnownGoodState>(json);

        if (state is null)
        {
            WriteAudit("Failed to deserialize last-known-good state; rollback aborted.");
            return;
        }

        // Remove any files that were not part of the last-known-good snapshot
        if (Directory.Exists(_installRoot))
        {
            var currentFiles = Directory.GetFiles(_installRoot, "*", SearchOption.AllDirectories);
            foreach (var file in currentFiles)
            {
                if (state.Files.Contains(file)) continue;

                try
                {
                    File.Delete(file);
                    WriteAudit($"Rollback: removed new file {file}");
                }
                catch (Exception ex)
                {
                    WriteAudit($"Rollback: failed to remove {file}: {ex.Message}");
                }
            }
        }

        WriteAudit("Rollback completed.");
    }

    private bool IsUnderInstallRoot(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(_installRoot);
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    private void WriteAudit(string message)
    {
        var line = $"[{DateTime.UtcNow:O}] {message}";
        Directory.CreateDirectory(Path.GetDirectoryName(_auditLogPath)!);
        File.AppendAllText(_auditLogPath, line + Environment.NewLine);
    }

    private sealed class LastKnownGoodState
    {
        public DateTime TimestampUtc { get; set; }
        public string InstallRoot { get; set; } = string.Empty;
        public List<string> Files { get; set; } = new();
    }

    private sealed record PrerequisiteValidation(bool IsValid, string Message)
    {
        public static PrerequisiteValidation Passed() => new(true, string.Empty);
        public static PrerequisiteValidation Failed(string message) => new(false, message);
    }
}

/// <summary>
/// Describes the outcome of an install or uninstall operation.
/// </summary>
public sealed record InstallResult
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The version affected by the operation.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Human-readable message describing the outcome.
    /// </summary>
    public required string Message { get; init; }

    public static InstallResult Succeeded(string version) =>
        new() { Success = true, Version = version, Message = "Operation completed successfully." };

    public static InstallResult Failed(string message) =>
        new() { Success = false, Version = string.Empty, Message = message };
}
