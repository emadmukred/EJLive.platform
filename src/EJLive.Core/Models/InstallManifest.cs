namespace EJLive.Core.Models;

/// <summary>
/// Describes a single component to be installed (e.g., DLL, config file, resource).
/// </summary>
public sealed record InstallComponent
{
    /// <summary>
    /// Logical name of the component.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Source path or URI where the component is retrieved from.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Destination path on the target machine.
    /// </summary>
    public required string Destination { get; init; }

    /// <summary>
    /// SHA-256 hash for integrity verification.
    /// </summary>
    public required string Hash { get; init; }
}

/// <summary>
/// Describes a Windows service to be registered during installation.
/// </summary>
public sealed record InstallService
{
    /// <summary>
    /// Display name of the service.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Service name used by the Service Control Manager.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// Path to the service executable.
    /// </summary>
    public required string ExecutablePath { get; init; }

    /// <summary>
    /// Start type (e.g., Automatic, Manual, Disabled).
    /// </summary>
    public required string StartType { get; init; }

    /// <summary>
    /// Comma-separated list of service dependencies.
    /// </summary>
    public string? Dependencies { get; init; }
}

/// <summary>
/// Describes a directory path required by the installation.
/// </summary>
public sealed record InstallPath
{
    /// <summary>
    /// Absolute or expandable path.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Whether the path must be created if it does not exist.
    /// </summary>
    public required bool CreateIfMissing { get; init; }

    /// <summary>
    /// Required permissions string (e.g., "RW" for read-write).
    /// </summary>
    public string? Permissions { get; init; }
}

/// <summary>
/// Describes a prerequisite that must be satisfied before installation proceeds.
/// </summary>
public sealed record InstallPrerequisite
{
    /// <summary>
    /// Human-readable name of the prerequisite.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Type of check (e.g., "Registry", "File", "Port", "Framework").
    /// </summary>
    public required string CheckType { get; init; }

    /// <summary>
    /// The value or expression to evaluate.
    /// </summary>
    public required string CheckValue { get; init; }

    /// <summary>
    /// Whether installation must abort if this prerequisite is missing.
    /// </summary>
    public required bool IsMandatory { get; init; }
}

/// <summary>
/// A complete installation manifest used by <see cref="InstallerEngine"/> to
/// orchestrate install, upgrade, and uninstall operations.
/// </summary>
public sealed record InstallManifest
{
    /// <summary>
    /// Unique identifier for this manifest.
    /// </summary>
    public required Guid ManifestId { get; init; }

    /// <summary>
    /// Product version represented by this manifest.
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Components to install.
    /// </summary>
    public required IReadOnlyList<InstallComponent> Components { get; init; }

    /// <summary>
    /// Services to register.
    /// </summary>
    public required IReadOnlyList<InstallService> Services { get; init; }

    /// <summary>
    /// Paths to ensure exist.
    /// </summary>
    public required IReadOnlyList<InstallPath> Paths { get; init; }

    /// <summary>
    /// Prerequisites to validate before installation.
    /// </summary>
    public required IReadOnlyList<InstallPrerequisite> Prerequisites { get; init; }
}
