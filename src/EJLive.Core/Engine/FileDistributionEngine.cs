using EJLive.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace EJLive.Core.Engine;

/// <summary>
/// Status values for file distribution.
/// </summary>
public enum DistributionStatus
{
    /// <summary>File was staged, verified, and promoted successfully.</summary>
    Success,

    /// <summary>Distribution failed; staged file was rolled back.</summary>
    Failed
}

/// <summary>
/// Result of a file distribution command.
/// </summary>
public sealed record CommandResult
{
    /// <summary>Destination path of the distributed file.</summary>
    public required string DestinationPath { get; init; }

    /// <summary>SHA-256 checksum of the file.</summary>
    public string? Checksum { get; init; }

    /// <summary>Size of the file in bytes.</summary>
    public long? Bytes { get; init; }

    /// <summary>Status of the distribution.</summary>
    public required DistributionStatus Status { get; init; }

    /// <summary>Detail message.</summary>
    public string? Message { get; init; }

    /// <summary>Creates a successful result.</summary>
    public static CommandResult Success(string destinationPath, string checksum, long bytes) => new()
    {
        DestinationPath = destinationPath,
        Status = DistributionStatus.Success,
        Checksum = checksum,
        Bytes = bytes
    };

    /// <summary>Creates a failed result.</summary>
    public static CommandResult Failure(string destinationPath, string message) => new()
    {
        DestinationPath = destinationPath,
        Status = DistributionStatus.Failed,
        Message = message
    };
}

/// <summary>
/// Abstracts file system operations for testability.
/// </summary>
public interface IFileSystem
{
    /// <summary>Gets the directory name from a path.</summary>
    string? GetDirectoryName(string path);

    /// <summary>Determines whether the directory exists.</summary>
    bool DirectoryExists(string path);

    /// <summary>Determines whether the file exists.</summary>
    bool FileExists(string path);

    /// <summary>Checks whether the process has write permission for the directory.</summary>
    bool HasWritePermission(string path);

    /// <summary>Writes all bytes to the file asynchronously.</summary>
    Task WriteAllBytesAsync(string path, byte[] data, CancellationToken cancellationToken = default);

    /// <summary>Deletes the file.</summary>
    void DeleteFile(string path);
}

/// <summary>
/// Defines allowed destination policies.
/// </summary>
public interface IFileDistributionPolicy
{
    /// <summary>Determines whether the destination is allowed.</summary>
    bool IsDestinationAllowed(string destinationPath);
}

/// <summary>
/// Manages temporary staging of incoming files.
/// </summary>
public interface IStagingArea
{
    /// <summary>Stages file content and returns the staged path.</summary>
    Task<string> StageAsync(Guid requestId, byte[] content, CancellationToken cancellationToken = default);
}

/// <summary>
/// Receives files from the server and stages them to ATM destinations
/// with integrity verification and path traversal protection.
/// </summary>
public class FileDistributionEngine
{
    private readonly IFileSystem _fileSystem;
    private readonly IFileDistributionPolicy _policy;
    private readonly IStagingArea _stagingArea;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDistributionEngine"/> class.
    /// </summary>
    public FileDistributionEngine(
        IFileSystem fileSystem,
        IFileDistributionPolicy policy,
        IStagingArea stagingArea)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _stagingArea = stagingArea ?? throw new ArgumentNullException(nameof(stagingArea));
    }

    /// <summary>
    /// Executes a file distribution request: stage, verify, promote.
    /// </summary>
    public async Task<CommandResult> DistributeAsync(
        FileDistributionRequest request,
        byte[] fileContent,
        CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (fileContent is null) throw new ArgumentNullException(nameof(fileContent));

        // Block path traversal before any normalization
        if (ContainsPathTraversal(request.DestinationPath))
        {
            return CommandResult.Failure(request.DestinationPath, "Path traversal detected in destination path.");
        }

        // Validate destination is within allowed folder
        if (!IsPathWithinAllowedFolder(request.DestinationPath, request.AllowedDestinationFolder))
        {
            return CommandResult.Failure(request.DestinationPath, "Destination path is outside the allowed folder.");
        }

        // Check destination folder exists and permissions
        string? destinationDirectory = _fileSystem.GetDirectoryName(request.DestinationPath);
        if (destinationDirectory is null || !_fileSystem.DirectoryExists(destinationDirectory))
        {
            return CommandResult.Failure(request.DestinationPath, "Destination directory does not exist.");
        }

        if (!_fileSystem.HasWritePermission(destinationDirectory))
        {
            return CommandResult.Failure(request.DestinationPath, "Insufficient permissions to write to destination.");
        }

        // Verify policy allows this vendor/type
        if (!_policy.IsDestinationAllowed(request.DestinationPath))
        {
            return CommandResult.Failure(request.DestinationPath, "Destination is not permitted by policy.");
        }

        // Stage the file
        string stagedPath = await _stagingArea.StageAsync(request.RequestId, fileContent, cancellationToken).ConfigureAwait(false);

        try
        {
            // Verify SHA-256
            string actualHash = ComputeSha256(fileContent);
            if (!string.Equals(actualHash, request.ExpectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                return CommandResult.Failure(request.DestinationPath, "SHA-256 checksum mismatch.");
            }

            // Promote to destination
            await _fileSystem.WriteAllBytesAsync(request.DestinationPath, fileContent, cancellationToken).ConfigureAwait(false);

            return CommandResult.Success(request.DestinationPath, actualHash, fileContent.Length);
        }
        catch (Exception ex)
        {
            // Rollback on promote failure: remove staged file if still present
            try
            {
                if (_fileSystem.FileExists(stagedPath))
                {
                    _fileSystem.DeleteFile(stagedPath);
                }
            }
            catch
            {
                // Best-effort cleanup; do not mask original exception.
            }

            return CommandResult.Failure(request.DestinationPath, $"Promotion failed: {ex.Message}");
        }
    }

    private static bool IsPathWithinAllowedFolder(string destinationPath, string allowedFolder)
    {
        string fullDestination = Path.GetFullPath(destinationPath);
        string fullAllowed = Path.GetFullPath(allowedFolder);

        // Ensure allowed folder ends with directory separator for prefix matching
        if (!fullAllowed.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
            !fullAllowed.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
        {
            fullAllowed += Path.DirectorySeparatorChar;
        }

        return fullDestination.StartsWith(fullAllowed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsPathTraversal(string path)
    {
        // Detect .. components that could escape intended directories
        var parts = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Any(p => p == "..");
    }

    private static string ComputeSha256(byte[] data)
    {
        byte[] hash = SHA256.HashData(data);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }
}
