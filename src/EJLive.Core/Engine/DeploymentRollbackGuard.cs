using System.Security.Cryptography;
using System.Text.Json;

namespace EJLive.Core.Engine;

/// <summary>
/// Track 041 – Deployment Rollback Guard.
/// Validates deployment integrity before and after upgrades.
/// Creates deterministic rollback checkpoints and verifies service health post-deploy.
/// </summary>
public sealed class DeploymentRollbackGuard
{
    private readonly string _checkpointDirectory;
    private readonly string _auditLogPath;

    public DeploymentRollbackGuard(string checkpointDirectory, string auditLogPath)
    {
        _checkpointDirectory = checkpointDirectory;
        _auditLogPath = auditLogPath;
        Directory.CreateDirectory(_checkpointDirectory);
    }

    /// <summary>
    /// Creates a pre-deployment checkpoint capturing all file hashes and service state.
    /// </summary>
    public async Task<DeploymentCheckpoint> CreateCheckpointAsync(
        string installRoot,
        string version,
        CancellationToken cancellationToken = default)
    {
        var files = Directory.Exists(installRoot)
            ? Directory.GetFiles(installRoot, "*", SearchOption.AllDirectories)
            : Array.Empty<string>();

        var fileHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var hash = await ComputeFileHashAsync(file, cancellationToken).ConfigureAwait(false);
            var relativePath = Path.GetRelativePath(installRoot, file);
            fileHashes[relativePath] = hash;
        }

        var checkpoint = new DeploymentCheckpoint
        {
            CheckpointId = Guid.NewGuid(),
            Version = version,
            CreatedUtc = DateTime.UtcNow,
            InstallRoot = installRoot,
            FileHashes = fileHashes,
            TotalFiles = fileHashes.Count,
            TotalSizeBytes = files.Sum(f => new FileInfo(f).Length)
        };

        var checkpointPath = Path.Combine(_checkpointDirectory, $"checkpoint-{checkpoint.CheckpointId}.json");
        var json = JsonSerializer.Serialize(checkpoint, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(checkpointPath, json, cancellationToken).ConfigureAwait(false);

        WriteAudit($"Checkpoint created: {checkpoint.CheckpointId} (v{version}, {fileHashes.Count} files)");
        return checkpoint;
    }

    /// <summary>
    /// Validates post-deployment integrity against a checkpoint.
    /// Returns a detailed report of differences.
    /// </summary>
    public async Task<RollbackValidationResult> ValidateDeploymentAsync(
        DeploymentCheckpoint baseline,
        CancellationToken cancellationToken = default)
    {
        var added = new List<string>();
        var modified = new List<string>();
        var removed = new List<string>();

        // Check existing files vs baseline
        var currentFiles = Directory.Exists(baseline.InstallRoot)
            ? Directory.GetFiles(baseline.InstallRoot, "*", SearchOption.AllDirectories)
            : Array.Empty<string>();

        var currentHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in currentFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = Path.GetRelativePath(baseline.InstallRoot, file);
            var hash = await ComputeFileHashAsync(file, cancellationToken).ConfigureAwait(false);
            currentHashes[relativePath] = hash;

            if (!baseline.FileHashes.ContainsKey(relativePath))
            {
                added.Add(relativePath);
            }
            else if (baseline.FileHashes[relativePath] != hash)
            {
                modified.Add(relativePath);
            }
        }

        // Check for removed files
        foreach (var baselineFile in baseline.FileHashes.Keys)
        {
            if (!currentHashes.ContainsKey(baselineFile))
            {
                removed.Add(baselineFile);
            }
        }

        var result = new RollbackValidationResult
        {
            CheckpointId = baseline.CheckpointId,
            ValidatedUtc = DateTime.UtcNow,
            AddedFiles = added.AsReadOnly(),
            ModifiedFiles = modified.AsReadOnly(),
            RemovedFiles = removed.AsReadOnly(),
            IsIntact = modified.Count == 0 && removed.Count == 0
        };

        WriteAudit($"Validation: +{added.Count} ~{modified.Count} -{removed.Count} | Intact={result.IsIntact}");
        return result;
    }

    /// <summary>
    /// Performs a health check after deployment to verify critical services are responsive.
    /// </summary>
    public async Task<PostDeployHealthCheck> VerifyPostDeployHealthAsync(
        IReadOnlyList<string> criticalServiceNames,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ServiceHealthEntry>();

        foreach (var serviceName in criticalServiceNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = await CheckServiceHealthAsync(serviceName, timeout, cancellationToken).ConfigureAwait(false);
            results.Add(entry);
        }

        var allHealthy = results.All(r => r.IsHealthy);

        var check = new PostDeployHealthCheck
        {
            CheckedUtc = DateTime.UtcNow,
            AllHealthy = allHealthy,
            Services = results.AsReadOnly(),
            RecommendRollback = !allHealthy
        };

        WriteAudit($"Post-deploy health: {(allHealthy ? "PASS" : "FAIL")} ({results.Count(r => r.IsHealthy)}/{results.Count})");
        return check;
    }

    /// <summary>
    /// Lists all available checkpoints ordered by creation time descending.
    /// </summary>
    public async Task<IReadOnlyList<DeploymentCheckpoint>> ListCheckpointsAsync(CancellationToken cancellationToken = default)
    {
        var checkpoints = new List<DeploymentCheckpoint>();
        var files = Directory.GetFiles(_checkpointDirectory, "checkpoint-*.json");

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var json = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
            var checkpoint = JsonSerializer.Deserialize<DeploymentCheckpoint>(json);
            if (checkpoint is not null)
            {
                checkpoints.Add(checkpoint);
            }
        }

        return checkpoints.OrderByDescending(c => c.CreatedUtc).ToList().AsReadOnly();
    }

    private static async Task<ServiceHealthEntry> CheckServiceHealthAsync(
        string serviceName,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            // Check service status via sc query simulation
            var psi = new System.Diagnostics.ProcessStartInfo("sc.exe", $"query \"{serviceName}\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(psi);
            if (process is null)
            {
                return new ServiceHealthEntry(serviceName, false, sw.Elapsed, "Could not start sc.exe");
            }

            await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
            var output = await process.StandardOutput.ReadToEndAsync(cts.Token).ConfigureAwait(false);
            var running = output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase);

            return new ServiceHealthEntry(serviceName, running, sw.Elapsed, running ? null : "Service not running");
        }
        catch (OperationCanceledException)
        {
            return new ServiceHealthEntry(serviceName, false, sw.Elapsed, "Health check timed out");
        }
        catch (Exception ex)
        {
            return new ServiceHealthEntry(serviceName, false, sw.Elapsed, ex.Message);
        }
    }

    private static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
        return Convert.ToHexString(hash);
    }

    private void WriteAudit(string message)
    {
        var line = $"[{DateTime.UtcNow:O}] [RollbackGuard] {message}";
        Directory.CreateDirectory(Path.GetDirectoryName(_auditLogPath)!);
        File.AppendAllText(_auditLogPath, line + Environment.NewLine);
    }
}

/// <summary>
/// Captures the full file-level state of a deployment at a point in time.
/// </summary>
public sealed record DeploymentCheckpoint
{
    public required Guid CheckpointId { get; init; }
    public required string Version { get; init; }
    public required DateTime CreatedUtc { get; init; }
    public required string InstallRoot { get; init; }
    public required Dictionary<string, string> FileHashes { get; init; }
    public required int TotalFiles { get; init; }
    public required long TotalSizeBytes { get; init; }
}

/// <summary>
/// Result of validating the current deployment state against a checkpoint.
/// </summary>
public sealed record RollbackValidationResult
{
    public required Guid CheckpointId { get; init; }
    public required DateTime ValidatedUtc { get; init; }
    public required IReadOnlyList<string> AddedFiles { get; init; }
    public required IReadOnlyList<string> ModifiedFiles { get; init; }
    public required IReadOnlyList<string> RemovedFiles { get; init; }
    public required bool IsIntact { get; init; }
}

/// <summary>
/// Aggregated post-deployment health check results.
/// </summary>
public sealed record PostDeployHealthCheck
{
    public required DateTime CheckedUtc { get; init; }
    public required bool AllHealthy { get; init; }
    public required IReadOnlyList<ServiceHealthEntry> Services { get; init; }
    public required bool RecommendRollback { get; init; }
}

/// <summary>
/// Health status of a single service post-deployment.
/// </summary>
public sealed record ServiceHealthEntry(
    string ServiceName,
    bool IsHealthy,
    TimeSpan ResponseTime,
    string? ErrorMessage);
