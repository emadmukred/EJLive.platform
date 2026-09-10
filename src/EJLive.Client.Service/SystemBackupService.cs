using System.Buffers;
using System.Data.SQLite;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EJLive.Core;
using EJLive.Shared;
using CoreAppConstants = EJLive.Core.AppConstants;

namespace EJLive.Client.Service;

/// <summary>
/// Defines the service-owned roots used by a full system backup.
/// All paths must be absolute and the backup root must not overlap a source root.
/// </summary>
public sealed record SystemBackupOptions(
    string BackupRootPath,
    string DatabasePath,
    string ConfigurationDirectory,
    string JournalStorageDirectory,
    string IntegrityKeyPath,
    long MaximumSourceBytes = 20L * 1024 * 1024 * 1024,
    int MaximumArtifactCount = 100_000)
{
    public static SystemBackupOptions CreateDefault()
    {
        var applicationData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive");
        var configurationDirectory = Path.Combine(applicationData, "Client");

        return new SystemBackupOptions(
            Path.Combine(applicationData, "Backups"),
            CoreAppConstants.DefaultDatabasePath,
            configurationDirectory,
            Path.Combine(applicationData, "Storage"),
            Path.Combine(configurationDirectory, "system-backup.key"));
    }
}

public sealed record SystemBackupResult(
    bool Success,
    string BackupName,
    string BackupPath,
    int ArtifactCount,
    long SourceBytes,
    string Message);

public sealed record SystemBackupValidationResult(
    bool IsValid,
    string BackupName,
    int ArtifactCount,
    long SourceBytes,
    DateTimeOffset? CreatedAtUtc,
    string Message);

public sealed record SystemBackupInfo(
    string Name,
    DateTimeOffset? CreatedAtUtc,
    long StoredBytes,
    int ArtifactCount,
    bool IsValid,
    string ValidationMessage);

/// <summary>
/// A restore is intentionally unavailable without a short-lived, signed and
/// maintenance-window-approved request from the governed operation path.
/// </summary>
public sealed record SystemBackupRestoreApproval(
    bool Approved,
    bool MaintenanceWindowConfirmed,
    bool RequestSignatureVerified,
    string ApprovedBy,
    string ChangeReference,
    DateTimeOffset ExpiresAtUtc);

/// <summary>
/// Creates and restores service data without UI dependencies. Backups are
/// published by an atomic directory move and authenticated with a machine-
/// protected HMAC key. Restore only replaces files represented by the manifest;
/// unrelated journal and configuration files are never removed.
/// </summary>
public sealed class SystemBackupService
{
    private const int ManifestSchemaVersion = 1;
    private const long MaximumManifestBytes = 4 * 1024 * 1024;
    private const string ManifestFileName = "backup-manifest.json";
    private const string ManifestSignatureFileName = "backup-manifest.hmac";
    private const string JournalArchiveFileName = "journals.zip";
    private static readonly HashSet<string> ConfigurationExtensions = new(
        new[] { ".cfg", ".config", ".json", ".xml" },
        StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ReservedWindowsNames = new(
        new[]
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        },
        StringComparer.OrdinalIgnoreCase);
    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        MaxDepth = 32,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SystemBackupOptions _options;
    private readonly SemaphoreSlim _operationGate = new(1, 1);
    private readonly byte[]? _providedIntegrityKey;
    private byte[]? _loadedIntegrityKey;

    public event Action<string>? OnLog;

    public SystemBackupService(
        SystemBackupOptions? options = null,
        byte[]? manifestAuthenticationKey = null)
    {
        _options = NormalizeAndValidateOptions(options ?? SystemBackupOptions.CreateDefault());

        if (manifestAuthenticationKey is { Length: < 32 })
            throw new ArgumentException("The manifest authentication key must contain at least 32 bytes.", nameof(manifestAuthenticationKey));

        _providedIntegrityKey = manifestAuthenticationKey?.ToArray();
    }

    public async Task<SystemBackupResult> CreateBackupAsync(
        string? requestedName = null,
        CancellationToken cancellationToken = default)
    {
        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        string backupName = string.Empty;
        string stagingDirectory = string.Empty;

        try
        {
            backupName = NormalizeBackupName(requestedName);
            Directory.CreateDirectory(_options.BackupRootPath);
            EnsureNoReparsePoints(_options.BackupRootPath, _options.BackupRootPath, includeLeaf: true);

            var finalDirectory = ResolveChildPath(_options.BackupRootPath, backupName);
            if (Directory.Exists(finalDirectory) || File.Exists(finalDirectory))
            {
                return Failure(backupName, finalDirectory, $"Backup '{backupName}' already exists.");
            }

            stagingDirectory = ResolveChildPath(
                _options.BackupRootPath,
                ".creating-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stagingDirectory);
            EnsureNoReparsePoints(_options.BackupRootPath, stagingDirectory, includeLeaf: true);

            _ = GetIntegrityKey(createIfMissing: true);

            var artifacts = new List<BackupArtifact>();
            long sourceBytes = 0;

            if (File.Exists(_options.DatabasePath))
            {
                var storedPath = "database/ejlive.db";
                var destination = ResolveChildPath(stagingDirectory, storedPath);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

                await CreateSqliteSnapshotAsync(
                        _options.DatabasePath,
                        destination,
                        cancellationToken)
                    .ConfigureAwait(false);

                var fingerprint = await FingerprintFileAsync(
                        destination,
                        _options.MaximumSourceBytes - sourceBytes,
                        FileShare.Read,
                        cancellationToken)
                    .ConfigureAwait(false);
                sourceBytes = AddWithinLimit(sourceBytes, fingerprint.Length);
                artifacts.Add(new BackupArtifact(
                    BackupArtifactKind.Database,
                    storedPath,
                    string.Empty,
                    fingerprint.Length,
                    fingerprint.Sha256));
            }

            if (Directory.Exists(_options.ConfigurationDirectory))
            {
                foreach (var sourcePath in EnumerateSafeFiles(_options.ConfigurationDirectory, recursive: false)
                             .Where(path => ConfigurationExtensions.Contains(Path.GetExtension(path)))
                             .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    EnsureArtifactCapacity(artifacts.Count + 1);

                    var fileName = Path.GetFileName(sourcePath);
                    var storedPath = "configuration/" + fileName;
                    var destination = ResolveChildPath(stagingDirectory, storedPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                    var fingerprint = await CopyFileWithFingerprintAsync(
                            sourcePath,
                            destination,
                            _options.MaximumSourceBytes - sourceBytes,
                            FileShare.ReadWrite | FileShare.Delete,
                            cancellationToken)
                        .ConfigureAwait(false);

                    sourceBytes = AddWithinLimit(sourceBytes, fingerprint.Length);
                    artifacts.Add(new BackupArtifact(
                        BackupArtifactKind.Configuration,
                        storedPath,
                        fileName,
                        fingerprint.Length,
                        fingerprint.Sha256));
                }
            }

            if (Directory.Exists(_options.JournalStorageDirectory))
            {
                sourceBytes = await CreateJournalArchiveAsync(
                        stagingDirectory,
                        artifacts,
                        sourceBytes,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            EnsureArtifactCapacity(artifacts.Count);
            var manifest = new BackupManifest
            {
                SchemaVersion = ManifestSchemaVersion,
                BackupName = backupName,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                ApplicationVersion = CoreAppConstants.AppVersion,
                Artifacts = artifacts
                    .OrderBy(item => item.Kind)
                    .ThenBy(item => item.RestorePath, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };

            await WriteManifestAsync(stagingDirectory, manifest, cancellationToken).ConfigureAwait(false);
            Directory.Move(stagingDirectory, finalDirectory);
            stagingDirectory = string.Empty;

            Log($"System backup created: {backupName}; artifacts={artifacts.Count}; bytes={sourceBytes}.");
            return new SystemBackupResult(
                true,
                backupName,
                finalDirectory,
                artifacts.Count,
                sourceBytes,
                "Backup created and integrity metadata verified.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log($"System backup failed: {ex.Message}");
            return Failure(backupName, stagingDirectory, ex.Message);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(stagingDirectory) &&
                Directory.Exists(stagingDirectory) &&
                IsWithinOrEqual(stagingDirectory, _options.BackupRootPath) &&
                !PathsEqual(stagingDirectory, _options.BackupRootPath))
            {
                try
                {
                    Directory.Delete(stagingDirectory, recursive: true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Log($"Unable to remove incomplete backup staging directory: {ex.Message}");
                }
            }

            _operationGate.Release();
        }
    }

    public async Task<SystemBackupValidationResult> ValidateBackupAsync(
        string backupName,
        CancellationToken cancellationToken = default)
    {
        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var loaded = await LoadAndValidateBackupAsync(backupName, cancellationToken).ConfigureAwait(false);
            return loaded.Validation;
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async Task<IReadOnlyList<SystemBackupInfo>> ListBackupsAsync(
        CancellationToken cancellationToken = default)
    {
        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!Directory.Exists(_options.BackupRootPath))
                return Array.Empty<SystemBackupInfo>();

            EnsureNoReparsePoints(_options.BackupRootPath, _options.BackupRootPath, includeLeaf: true);
            var results = new List<SystemBackupInfo>();

            foreach (var directory in Directory.EnumerateDirectories(
                         _options.BackupRootPath,
                         "*",
                         SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = Path.GetFileName(directory);
                if (name.StartsWith(".", StringComparison.Ordinal))
                    continue;

                var loaded = await LoadAndValidateBackupAsync(name, cancellationToken).ConfigureAwait(false);
                long storedBytes;
                try
                {
                    storedBytes = EnumerateSafeFiles(directory, recursive: true)
                        .Sum(path => new FileInfo(path).Length);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
                {
                    storedBytes = 0;
                    loaded = loaded with
                    {
                        Validation = loaded.Validation with
                        {
                            IsValid = false,
                            Message = "Unable to inspect stored files: " + ex.Message
                        }
                    };
                }

                results.Add(new SystemBackupInfo(
                    name,
                    loaded.Validation.CreatedAtUtc,
                    storedBytes,
                    loaded.Validation.ArtifactCount,
                    loaded.Validation.IsValid,
                    loaded.Validation.Message));
            }

            return results
                .OrderByDescending(item => item.CreatedAtUtc)
                .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        finally
        {
            _operationGate.Release();
        }
    }

    public async Task<SystemBackupResult> RestoreBackupAsync(
        string backupName,
        SystemBackupRestoreApproval approval,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approval);
        var approvalError = ValidateRestoreApproval(approval);
        if (approvalError is not null)
            return Failure(backupName, string.Empty, approvalError);

        await _operationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        var rollbackActions = new List<RollbackAction>();

        try
        {
            var loaded = await LoadAndValidateBackupAsync(backupName, cancellationToken).ConfigureAwait(false);
            if (!loaded.Validation.IsValid || loaded.Manifest is null)
            {
                return Failure(backupName, loaded.DirectoryPath, "Backup validation failed: " + loaded.Validation.Message);
            }

            var manifest = loaded.Manifest;
            var nonJournalArtifacts = manifest.Artifacts
                .Where(item => item.Kind != BackupArtifactKind.Journal)
                .OrderBy(item => item.Kind == BackupArtifactKind.Database ? 1 : 0)
                .ThenBy(item => item.RestorePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var artifact in nonJournalArtifacts)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sourcePath = ResolveChildPath(loaded.DirectoryPath, artifact.StoredPath);
                var targetPath = artifact.Kind switch
                {
                    BackupArtifactKind.Database => _options.DatabasePath,
                    BackupArtifactKind.Configuration => ResolveChildPath(
                        _options.ConfigurationDirectory,
                        artifact.RestorePath),
                    _ => throw new InvalidDataException($"Unsupported artifact kind '{artifact.Kind}'.")
                };

                if (artifact.Kind == BackupArtifactKind.Database)
                    QuarantineSqliteCompanionFiles(rollbackActions);

                await using var source = OpenRead(sourcePath, FileShare.Read);
                await StageAndReplaceAsync(
                        source,
                        targetPath,
                        artifact.Length,
                        artifact.Sha256,
                        rollbackActions,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var journalArtifacts = manifest.Artifacts
                .Where(item => item.Kind == BackupArtifactKind.Journal)
                .OrderBy(item => item.RestorePath, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (journalArtifacts.Length > 0)
            {
                var archivePath = ResolveChildPath(loaded.DirectoryPath, JournalArchiveFileName);
                await using var archiveStream = OpenRead(archivePath, FileShare.Read);
                using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: false);
                var entries = archive.Entries.ToDictionary(
                    entry => NormalizeArchivePath(entry.FullName),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var artifact in journalArtifacts)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!entries.TryGetValue(artifact.RestorePath, out var entry))
                        throw new InvalidDataException($"Journal entry '{artifact.RestorePath}' is missing.");

                    var targetPath = ResolveChildPath(
                        _options.JournalStorageDirectory,
                        artifact.RestorePath);
                    await using var source = entry.Open();
                    await StageAndReplaceAsync(
                            source,
                            targetPath,
                            artifact.Length,
                            artifact.Sha256,
                            rollbackActions,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            var cleanupWarnings = CommitRestore(rollbackActions);
            Log(
                $"System backup restored: {backupName}; approvedBy={approval.ApprovedBy}; " +
                $"change={approval.ChangeReference}; artifacts={manifest.Artifacts.Count}.");

            var message = cleanupWarnings.Count == 0
                ? "Restore completed with integrity checks and atomic file replacement."
                : "Restore completed; rollback-file cleanup warnings: " + string.Join(" | ", cleanupWarnings);
            return new SystemBackupResult(
                true,
                backupName,
                loaded.DirectoryPath,
                manifest.Artifacts.Count,
                manifest.Artifacts.Sum(item => item.Length),
                message);
        }
        catch (OperationCanceledException)
        {
            var rollbackErrors = RollBackRestore(rollbackActions);
            if (rollbackErrors.Count > 0)
                Log("Restore cancellation rollback warnings: " + string.Join(" | ", rollbackErrors));
            throw;
        }
        catch (Exception ex)
        {
            var rollbackErrors = RollBackRestore(rollbackActions);
            var message = rollbackErrors.Count == 0
                ? ex.Message
                : ex.Message + " Rollback requires attention: " + string.Join(" | ", rollbackErrors);
            Log($"System restore failed for {backupName}: {message}");
            return Failure(backupName, string.Empty, message);
        }
        finally
        {
            _operationGate.Release();
        }
    }

    private async Task<long> CreateJournalArchiveAsync(
        string stagingDirectory,
        List<BackupArtifact> artifacts,
        long sourceBytes,
        CancellationToken cancellationToken)
    {
        var files = EnumerateSafeFiles(_options.JournalStorageDirectory, recursive: true)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (files.Length == 0)
            return sourceBytes;

        EnsureArtifactCapacity(artifacts.Count + files.Length);
        var archivePath = ResolveChildPath(stagingDirectory, JournalArchiveFileName);
        await using var archiveStream = new FileStream(
            archivePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.WriteThrough);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true);

        foreach (var sourcePath in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = NormalizeArchivePath(
                Path.GetRelativePath(_options.JournalStorageDirectory, sourcePath)
                    .Replace(Path.DirectorySeparatorChar, '/'));
            var entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            await using var source = OpenRead(sourcePath, FileShare.ReadWrite | FileShare.Delete);
            var fingerprint = await CopyStreamWithFingerprintAsync(
                    source,
                    entryStream,
                    _options.MaximumSourceBytes - sourceBytes,
                    cancellationToken)
                .ConfigureAwait(false);

            sourceBytes = AddWithinLimit(sourceBytes, fingerprint.Length);
            artifacts.Add(new BackupArtifact(
                BackupArtifactKind.Journal,
                JournalArchiveFileName,
                relativePath,
                fingerprint.Length,
                fingerprint.Sha256));
        }

        return sourceBytes;
    }

    private async Task<LoadedBackup> LoadAndValidateBackupAsync(
        string requestedName,
        CancellationToken cancellationToken)
    {
        string backupName;
        string directoryPath;

        try
        {
            backupName = NormalizeBackupName(requestedName, generateWhenEmpty: false);
            directoryPath = ResolveChildPath(_options.BackupRootPath, backupName);
            if (!Directory.Exists(directoryPath))
                return InvalidLoaded(backupName, directoryPath, "Backup directory does not exist.");

            EnsureNoReparsePoints(_options.BackupRootPath, directoryPath, includeLeaf: true);
            var manifestPath = ResolveChildPath(directoryPath, ManifestFileName);
            var signaturePath = ResolveChildPath(directoryPath, ManifestSignatureFileName);
            if (!File.Exists(manifestPath) || !File.Exists(signaturePath))
                return InvalidLoaded(backupName, directoryPath, "Manifest or manifest authentication file is missing.");

            var manifestInfo = new FileInfo(manifestPath);
            if (manifestInfo.Length is <= 0 or > MaximumManifestBytes)
                return InvalidLoaded(backupName, directoryPath, "Manifest size is outside the allowed range.");

            var manifestBytes = await File.ReadAllBytesAsync(manifestPath, cancellationToken).ConfigureAwait(false);
            var signatureText = (await File.ReadAllTextAsync(signaturePath, cancellationToken).ConfigureAwait(false)).Trim();
            if (!TryDecodeSha256(signatureText, out var expectedSignature))
                return InvalidLoaded(backupName, directoryPath, "Manifest authentication value is malformed.");

            using (var hmac = new HMACSHA256(GetIntegrityKey(createIfMissing: false)))
            {
                var actualSignature = hmac.ComputeHash(manifestBytes);
                if (!CryptographicOperations.FixedTimeEquals(actualSignature, expectedSignature))
                    return InvalidLoaded(backupName, directoryPath, "Manifest authentication failed.");
            }

            var manifest = JsonSerializer.Deserialize<BackupManifest>(manifestBytes, ManifestJsonOptions);
            if (manifest is null)
                return InvalidLoaded(backupName, directoryPath, "Manifest is empty.");

            ValidateManifestHeader(manifest, backupName);
            await ValidateArtifactsAsync(directoryPath, manifest, cancellationToken).ConfigureAwait(false);

            var sourceBytes = manifest.Artifacts.Sum(item => item.Length);
            return new LoadedBackup(
                directoryPath,
                manifest,
                new SystemBackupValidationResult(
                    true,
                    backupName,
                    manifest.Artifacts.Count,
                    sourceBytes,
                    manifest.CreatedAtUtc,
                    "Backup manifest, authentication and artifact hashes are valid."));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            backupName = string.IsNullOrWhiteSpace(requestedName) ? string.Empty : requestedName.Trim();
            directoryPath = string.IsNullOrWhiteSpace(backupName)
                ? string.Empty
                : Path.Combine(_options.BackupRootPath, backupName);
            return InvalidLoaded(backupName, directoryPath, ex.Message);
        }
    }

    private async Task ValidateArtifactsAsync(
        string directoryPath,
        BackupManifest manifest,
        CancellationToken cancellationToken)
    {
        EnsureArtifactCapacity(manifest.Artifacts.Count);
        var expectedStoredFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ManifestFileName,
            ManifestSignatureFileName
        };
        var configurationTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var journalTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var databaseCount = 0;
        long sourceBytes = 0;

        foreach (var artifact in manifest.Artifacts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ValidateArtifactMetadata(artifact);
            sourceBytes = AddWithinLimit(sourceBytes, artifact.Length);

            switch (artifact.Kind)
            {
                case BackupArtifactKind.Database:
                    databaseCount++;
                    if (databaseCount > 1 ||
                        !string.Equals(artifact.StoredPath, "database/ejlive.db", StringComparison.Ordinal) ||
                        artifact.RestorePath.Length != 0)
                    {
                        throw new InvalidDataException("Database artifact metadata is invalid.");
                    }
                    break;

                case BackupArtifactKind.Configuration:
                    var configName = Path.GetFileName(artifact.RestorePath);
                    if (!string.Equals(configName, artifact.RestorePath, StringComparison.Ordinal) ||
                        !ConfigurationExtensions.Contains(Path.GetExtension(configName)) ||
                        !string.Equals(artifact.StoredPath, "configuration/" + configName, StringComparison.Ordinal) ||
                        !configurationTargets.Add(configName))
                    {
                        throw new InvalidDataException("Configuration artifact metadata is invalid.");
                    }
                    break;

                case BackupArtifactKind.Journal:
                    var normalizedJournalPath = NormalizeArchivePath(artifact.RestorePath);
                    _ = ResolveChildPath(_options.JournalStorageDirectory, normalizedJournalPath);
                    if (!string.Equals(artifact.StoredPath, JournalArchiveFileName, StringComparison.Ordinal) ||
                        !string.Equals(normalizedJournalPath, artifact.RestorePath, StringComparison.Ordinal) ||
                        !journalTargets.Add(normalizedJournalPath))
                    {
                        throw new InvalidDataException("Journal artifact metadata is invalid.");
                    }
                    break;

                default:
                    throw new InvalidDataException($"Unknown backup artifact kind '{artifact.Kind}'.");
            }
        }

        foreach (var artifact in manifest.Artifacts.Where(item => item.Kind != BackupArtifactKind.Journal))
        {
            var path = ResolveChildPath(directoryPath, artifact.StoredPath);
            EnsureNoReparsePoints(directoryPath, path, includeLeaf: true);
            if (!File.Exists(path))
                throw new InvalidDataException($"Stored artifact '{artifact.StoredPath}' is missing.");

            var fingerprint = await FingerprintFileAsync(
                    path,
                    artifact.Length,
                    FileShare.Read,
                    cancellationToken)
                .ConfigureAwait(false);
            EnsureFingerprintMatches(artifact, fingerprint);
            expectedStoredFiles.Add(artifact.StoredPath);

            if (artifact.Kind == BackupArtifactKind.Database)
                await ValidateSqliteSnapshotAsync(path, cancellationToken).ConfigureAwait(false);
        }

        var journalArtifacts = manifest.Artifacts
            .Where(item => item.Kind == BackupArtifactKind.Journal)
            .ToDictionary(item => item.RestorePath, StringComparer.OrdinalIgnoreCase);
        if (journalArtifacts.Count > 0)
        {
            var archivePath = ResolveChildPath(directoryPath, JournalArchiveFileName);
            EnsureNoReparsePoints(directoryPath, archivePath, includeLeaf: true);
            if (!File.Exists(archivePath))
                throw new InvalidDataException("Journal archive is missing.");

            expectedStoredFiles.Add(JournalArchiveFileName);
            await using var archiveStream = OpenRead(archivePath, FileShare.Read);
            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: false);
            var seenEntries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entryPath = NormalizeArchivePath(entry.FullName);
                if (!seenEntries.Add(entryPath) || !journalArtifacts.TryGetValue(entryPath, out var artifact))
                    throw new InvalidDataException($"Unexpected or duplicate journal entry '{entryPath}'.");
                if (entry.Length != artifact.Length)
                    throw new InvalidDataException($"Journal entry length mismatch for '{entryPath}'.");

                await using var entryStream = entry.Open();
                var fingerprint = await FingerprintStreamAsync(
                        entryStream,
                        artifact.Length,
                        cancellationToken)
                    .ConfigureAwait(false);
                EnsureFingerprintMatches(artifact, fingerprint);
            }

            if (seenEntries.Count != journalArtifacts.Count)
                throw new InvalidDataException("Journal archive does not contain every manifest entry.");
        }

        var actualStoredFiles = EnumerateSafeFiles(directoryPath, recursive: true)
            .Select(path => Path.GetRelativePath(directoryPath, path).Replace(Path.DirectorySeparatorChar, '/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!actualStoredFiles.SetEquals(expectedStoredFiles))
        {
            var unexpected = actualStoredFiles.Except(expectedStoredFiles, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
            var missing = expectedStoredFiles.Except(actualStoredFiles, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
            throw new InvalidDataException(
                unexpected is not null
                    ? $"Unexpected stored file '{unexpected}'."
                    : $"Expected stored file '{missing}' is missing.");
        }
    }

    private async Task WriteManifestAsync(
        string stagingDirectory,
        BackupManifest manifest,
        CancellationToken cancellationToken)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(manifest, ManifestJsonOptions);
        if (bytes.Length > MaximumManifestBytes)
            throw new InvalidDataException("Backup manifest exceeds the allowed size.");

        var manifestPath = ResolveChildPath(stagingDirectory, ManifestFileName);
        await WriteNewFileDurablyAsync(manifestPath, bytes, cancellationToken).ConfigureAwait(false);

        byte[] signature;
        using (var hmac = new HMACSHA256(GetIntegrityKey(createIfMissing: true)))
            signature = hmac.ComputeHash(bytes);

        var signaturePath = ResolveChildPath(stagingDirectory, ManifestSignatureFileName);
        await WriteNewFileDurablyAsync(
                signaturePath,
                Encoding.ASCII.GetBytes(Convert.ToHexString(signature).ToLowerInvariant()),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private byte[] GetIntegrityKey(bool createIfMissing)
    {
        if (_providedIntegrityKey is not null)
            return _providedIntegrityKey;
        if (_loadedIntegrityKey is not null)
            return _loadedIntegrityKey;

        var environmentValue = Environment.GetEnvironmentVariable("EJLIVE_BACKUP_INTEGRITY_KEY");
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            var environmentKey = Convert.FromBase64String(environmentValue.Trim());
            if (environmentKey.Length < 32)
                throw new InvalidOperationException("EJLIVE_BACKUP_INTEGRITY_KEY must decode to at least 32 bytes.");
            return _loadedIntegrityKey = environmentKey;
        }

        if (File.Exists(_options.IntegrityKeyPath))
        {
            var protectedText = File.ReadAllText(_options.IntegrityKeyPath).Trim();
            if (!protectedText.StartsWith("dpapi:", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The backup integrity key is not machine protected.");

            var plainText = SecurityHelper.TryUnprotectDpapiString(protectedText);
            if (plainText.StartsWith("dpapi:", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The backup integrity key cannot be unprotected on this machine.");

            var key = Convert.FromBase64String(plainText);
            if (key.Length < 32)
                throw new InvalidDataException("The backup integrity key is too short.");
            return _loadedIntegrityKey = key;
        }

        if (!createIfMissing)
            throw new FileNotFoundException("The backup integrity key is unavailable.", _options.IntegrityKeyPath);

        Directory.CreateDirectory(_options.ConfigurationDirectory);
        EnsureNoReparsePoints(
            _options.ConfigurationDirectory,
            _options.ConfigurationDirectory,
            includeLeaf: true);

        var generated = RandomNumberGenerator.GetBytes(32);
        var protectedValue = SecurityHelper.ProtectDpapiStringIfNeeded(Convert.ToBase64String(generated));
        if (!protectedValue.StartsWith("dpapi:", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Unable to protect the backup integrity key with machine DPAPI.");

        var temporaryPath = _options.IntegrityKeyPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            using (var stream = new FileStream(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       4096,
                       FileOptions.WriteThrough))
            {
                var content = Encoding.UTF8.GetBytes(protectedValue);
                stream.Write(content);
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, _options.IntegrityKeyPath, overwrite: false);
        }
        catch (IOException) when (File.Exists(_options.IntegrityKeyPath))
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
            _loadedIntegrityKey = null;
            return GetIntegrityKey(createIfMissing: false);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }

        return _loadedIntegrityKey = generated;
    }

    private static async Task CreateSqliteSnapshotAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    SQLiteConnection.CreateFile(destinationPath);
                    var sourceBuilder = new SQLiteConnectionStringBuilder
                    {
                        DataSource = sourcePath,
                        ReadOnly = true,
                        FailIfMissing = true
                    };
                    var destinationBuilder = new SQLiteConnectionStringBuilder
                    {
                        DataSource = destinationPath,
                        FailIfMissing = true
                    };

                    using (var source = new SQLiteConnection(sourceBuilder.ConnectionString))
                    using (var destination = new SQLiteConnection(destinationBuilder.ConnectionString))
                    {
                        source.Open();
                        destination.Open();
                        source.BackupDatabase(destination, "main", "main", -1, null, 0);

                        using (var checkpoint = destination.CreateCommand())
                        {
                            checkpoint.CommandText = "PRAGMA wal_checkpoint(TRUNCATE)";
                            checkpoint.ExecuteNonQuery();
                        }

                        using (var journalMode = destination.CreateCommand())
                        {
                            journalMode.CommandText = "PRAGMA journal_mode=DELETE";
                            var mode = Convert.ToString(journalMode.ExecuteScalar());
                            if (!string.Equals(mode, "delete", StringComparison.OrdinalIgnoreCase))
                                throw new InvalidDataException("SQLite snapshot journal mode could not be normalized.");
                        }

                        using var check = destination.CreateCommand();
                        check.CommandText = "PRAGMA quick_check";
                        var result = Convert.ToString(check.ExecuteScalar());
                        if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
                            throw new InvalidDataException("SQLite snapshot failed its integrity check.");
                    }

                    foreach (var suffix in new[] { "-wal", "-shm" })
                    {
                        var companionPath = destinationPath + suffix;
                        if (File.Exists(companionPath))
                            File.Delete(companionPath);
                    }
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task ValidateSqliteSnapshotAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await Task.Run(
                () =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var builder = new SQLiteConnectionStringBuilder
                    {
                        DataSource = path,
                        ReadOnly = true,
                        FailIfMissing = true
                    };
                    using var connection = new SQLiteConnection(builder.ConnectionString);
                    connection.Open();
                    using var check = connection.CreateCommand();
                    check.CommandText = "PRAGMA quick_check";
                    var result = Convert.ToString(check.ExecuteScalar());
                    if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("SQLite backup is not structurally valid.");
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task StageAndReplaceAsync(
        Stream source,
        string targetPath,
        long expectedLength,
        string expectedSha256,
        List<RollbackAction> rollbackActions,
        CancellationToken cancellationToken)
    {
        var root = ResolveRestoreRoot(targetPath);
        var directory = Path.GetDirectoryName(targetPath)
            ?? throw new InvalidDataException("Restore target has no parent directory.");
        Directory.CreateDirectory(directory);
        EnsureNoReparsePoints(root, directory, includeLeaf: true);
        if (File.Exists(targetPath))
            EnsureNoReparsePoints(root, targetPath, includeLeaf: true);

        var temporaryPath = Path.Combine(
            directory,
            "." + Path.GetFileName(targetPath) + ".restore-" + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            await using (var destination = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             64 * 1024,
                             FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                var fingerprint = await CopyStreamWithFingerprintAsync(
                        source,
                        destination,
                        expectedLength,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (fingerprint.Length != expectedLength ||
                    !string.Equals(fingerprint.Sha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"Restore content verification failed for '{Path.GetFileName(targetPath)}'.");
                }

                await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
                destination.Flush(flushToDisk: true);
            }

            if (File.Exists(targetPath))
            {
                var rollbackPath = targetPath + ".rollback-" + Guid.NewGuid().ToString("N");
                File.Replace(temporaryPath, targetPath, rollbackPath, ignoreMetadataErrors: true);
                rollbackActions.Add(new RollbackAction(targetPath, rollbackPath, OriginalExisted: true));
            }
            else
            {
                File.Move(temporaryPath, targetPath);
                rollbackActions.Add(new RollbackAction(targetPath, string.Empty, OriginalExisted: false));
            }
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private void QuarantineSqliteCompanionFiles(List<RollbackAction> rollbackActions)
    {
        var databaseRoot = Path.GetDirectoryName(_options.DatabasePath)
            ?? throw new InvalidDataException("Database path has no parent directory.");

        foreach (var suffix in new[] { "-wal", "-shm" })
        {
            var path = _options.DatabasePath + suffix;
            if (!File.Exists(path))
                continue;

            EnsureNoReparsePoints(databaseRoot, path, includeLeaf: true);
            var rollbackPath = path + ".rollback-" + Guid.NewGuid().ToString("N");
            File.Move(path, rollbackPath);
            rollbackActions.Add(new RollbackAction(path, rollbackPath, OriginalExisted: true));
        }
    }

    private static List<string> CommitRestore(List<RollbackAction> actions)
    {
        var warnings = new List<string>();
        foreach (var action in actions)
        {
            if (!action.OriginalExisted || string.IsNullOrWhiteSpace(action.RollbackPath))
                continue;

            try
            {
                if (File.Exists(action.RollbackPath))
                    File.Delete(action.RollbackPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                warnings.Add($"{Path.GetFileName(action.RollbackPath)}: {ex.Message}");
            }
        }

        actions.Clear();
        return warnings;
    }

    private static List<string> RollBackRestore(List<RollbackAction> actions)
    {
        var errors = new List<string>();
        for (var index = actions.Count - 1; index >= 0; index--)
        {
            var action = actions[index];
            try
            {
                if (!action.OriginalExisted)
                {
                    if (File.Exists(action.TargetPath))
                        File.Delete(action.TargetPath);
                    continue;
                }

                if (!File.Exists(action.RollbackPath))
                    throw new IOException("Rollback copy is missing.");

                if (File.Exists(action.TargetPath))
                    File.Replace(action.RollbackPath, action.TargetPath, null, ignoreMetadataErrors: true);
                else
                    File.Move(action.RollbackPath, action.TargetPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                errors.Add($"{Path.GetFileName(action.TargetPath)}: {ex.Message}");
            }
        }

        actions.Clear();
        return errors;
    }

    private static async Task<FileFingerprint> CopyFileWithFingerprintAsync(
        string sourcePath,
        string destinationPath,
        long maximumBytes,
        FileShare sourceShare,
        CancellationToken cancellationToken)
    {
        await using var source = OpenRead(sourcePath, sourceShare);
        await using var destination = new FileStream(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.WriteThrough);
        var fingerprint = await CopyStreamWithFingerprintAsync(
                source,
                destination,
                maximumBytes,
                cancellationToken)
            .ConfigureAwait(false);
        await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
        destination.Flush(flushToDisk: true);
        return fingerprint;
    }

    private static async Task<FileFingerprint> FingerprintFileAsync(
        string path,
        long maximumBytes,
        FileShare share,
        CancellationToken cancellationToken)
    {
        await using var source = OpenRead(path, share);
        return await FingerprintStreamAsync(source, maximumBytes, cancellationToken).ConfigureAwait(false);
    }

    private static Task<FileFingerprint> FingerprintStreamAsync(
        Stream source,
        long maximumBytes,
        CancellationToken cancellationToken) =>
        CopyStreamWithFingerprintAsync(source, Stream.Null, maximumBytes, cancellationToken);

    private static async Task<FileFingerprint> CopyStreamWithFingerprintAsync(
        Stream source,
        Stream destination,
        long maximumBytes,
        CancellationToken cancellationToken)
    {
        if (maximumBytes < 0)
            throw new InvalidDataException("Backup source size exceeds the configured limit.");

        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        long length = 0;

        try
        {
            while (true)
            {
                var read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                    .ConfigureAwait(false);
                if (read == 0)
                    break;

                length = checked(length + read);
                if (length > maximumBytes)
                    throw new InvalidDataException("Backup source size exceeds the configured limit.");

                hash.AppendData(buffer, 0, read);
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            }

            return new FileFingerprint(length, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    private static async Task WriteNewFileDurablyAsync(
        string path,
        byte[] content,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            16 * 1024,
            FileOptions.Asynchronous | FileOptions.WriteThrough);
        await stream.WriteAsync(content, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        stream.Flush(flushToDisk: true);
    }

    private static FileStream OpenRead(string path, FileShare share) => new(
        path,
        FileMode.Open,
        FileAccess.Read,
        share,
        64 * 1024,
        FileOptions.Asynchronous | FileOptions.SequentialScan);

    private IEnumerable<string> EnumerateSafeFiles(string root, bool recursive)
    {
        EnsureNoReparsePoints(root, root, includeLeaf: true);
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = recursive,
            IgnoreInaccessible = false,
            ReturnSpecialDirectories = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        };

        foreach (var path in Directory.EnumerateFiles(root, "*", options))
        {
            EnsureNoReparsePoints(root, path, includeLeaf: true);
            yield return path;
        }
    }

    private static SystemBackupOptions NormalizeAndValidateOptions(SystemBackupOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.MaximumSourceBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "MaximumSourceBytes must be positive.");
        if (options.MaximumArtifactCount is <= 0 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(options), "MaximumArtifactCount is outside the allowed range.");

        var backupRoot = NormalizeAbsolutePath(options.BackupRootPath, nameof(options.BackupRootPath));
        var databasePath = NormalizeAbsolutePath(options.DatabasePath, nameof(options.DatabasePath));
        var configDirectory = NormalizeAbsolutePath(options.ConfigurationDirectory, nameof(options.ConfigurationDirectory));
        var journalDirectory = NormalizeAbsolutePath(options.JournalStorageDirectory, nameof(options.JournalStorageDirectory));
        var keyPath = NormalizeAbsolutePath(options.IntegrityKeyPath, nameof(options.IntegrityKeyPath));

        if (IsWithinOrEqual(backupRoot, configDirectory) ||
            IsWithinOrEqual(backupRoot, journalDirectory) ||
            IsWithinOrEqual(databasePath, backupRoot))
        {
            throw new ArgumentException("The backup root must not be inside a source root and must not contain the database.", nameof(options));
        }

        if (!IsWithinOrEqual(keyPath, configDirectory))
            throw new ArgumentException("IntegrityKeyPath must be inside ConfigurationDirectory.", nameof(options));
        if (IsWithinOrEqual(databasePath, configDirectory) || IsWithinOrEqual(databasePath, journalDirectory))
            throw new ArgumentException("DatabasePath must not overlap configuration or journal storage.", nameof(options));
        if (IsWithinOrEqual(configDirectory, journalDirectory) || IsWithinOrEqual(journalDirectory, configDirectory))
            throw new ArgumentException("ConfigurationDirectory and JournalStorageDirectory must not overlap.", nameof(options));

        return options with
        {
            BackupRootPath = backupRoot,
            DatabasePath = databasePath,
            ConfigurationDirectory = configDirectory,
            JournalStorageDirectory = journalDirectory,
            IntegrityKeyPath = keyPath
        };
    }

    private static string NormalizeAbsolutePath(string path, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path.Trim()))
            throw new ArgumentException("Path must be absolute.", parameterName);
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path.Trim()));
    }

    private static string NormalizeBackupName(string? requestedName, bool generateWhenEmpty = true)
    {
        if (string.IsNullOrWhiteSpace(requestedName))
        {
            if (!generateWhenEmpty)
                throw new ArgumentException("Backup name is required.", nameof(requestedName));
            return $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}_{RandomNumberGenerator.GetInt32(0x10000):x4}";
        }

        var name = requestedName.Trim();
        if (name.Length is < 1 or > 80 || !char.IsLetterOrDigit(name[0]))
            throw new ArgumentException("Backup name must start with a letter or digit and contain at most 80 characters.", nameof(requestedName));
        if (name.Any(character =>
                !(char.IsLetterOrDigit(character) || character is '-' or '_' or '.')))
        {
            throw new ArgumentException("Backup name contains unsupported characters.", nameof(requestedName));
        }

        var baseName = name.Split('.')[0];
        if (ReservedWindowsNames.Contains(baseName))
            throw new ArgumentException("Backup name is reserved by Windows.", nameof(requestedName));
        return name;
    }

    private static string NormalizeArchivePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Contains('\\') || path.StartsWith("/", StringComparison.Ordinal))
            throw new InvalidDataException("Archive entry path is invalid.");

        var segments = path.Split('/');
        if (segments.Any(segment =>
                string.IsNullOrWhiteSpace(segment) ||
                segment is "." or ".." ||
                segment.Contains(':') ||
                segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
        {
            throw new InvalidDataException("Archive entry path escapes the journal storage root.");
        }

        return string.Join('/', segments);
    }

    private static string ResolveChildPath(string root, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathFullyQualified(relativePath))
            throw new InvalidDataException("Relative path is invalid.");

        var platformRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var result = Path.GetFullPath(Path.Combine(root, platformRelativePath));
        if (!IsWithinOrEqual(result, root) || PathsEqual(result, root))
            throw new InvalidDataException("Resolved path escapes its allowed root.");
        return result;
    }

    private string ResolveRestoreRoot(string targetPath)
    {
        if (PathsEqual(targetPath, _options.DatabasePath))
            return Path.GetDirectoryName(_options.DatabasePath)!;
        if (IsWithinOrEqual(targetPath, _options.ConfigurationDirectory))
            return _options.ConfigurationDirectory;
        if (IsWithinOrEqual(targetPath, _options.JournalStorageDirectory))
            return _options.JournalStorageDirectory;
        throw new InvalidDataException("Restore target is outside the configured roots.");
    }

    private static void EnsureNoReparsePoints(string root, string candidate, bool includeLeaf)
    {
        if (!IsWithinOrEqual(candidate, root))
            throw new InvalidDataException("Path is outside its allowed root.");

        var relative = Path.GetRelativePath(root, candidate);
        var current = root;
        if (Directory.Exists(current) &&
            (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidDataException($"Reparse-point root is not allowed: {current}");
        }

        if (relative == ".")
            return;

        var segments = relative.Split(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
            StringSplitOptions.RemoveEmptyEntries);
        var length = includeLeaf ? segments.Length : Math.Max(0, segments.Length - 1);
        for (var index = 0; index < length; index++)
        {
            current = Path.Combine(current, segments[index]);
            if ((Directory.Exists(current) || File.Exists(current)) &&
                (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException($"Reparse points are not allowed in managed paths: {current}");
            }
        }
    }

    private static bool IsWithinOrEqual(string candidate, string root)
    {
        var normalizedCandidate = Path.GetFullPath(candidate);
        var normalizedRoot = Path.GetFullPath(root);
        if (PathsEqual(normalizedCandidate, normalizedRoot))
            return true;

        var rootWithSeparator = Path.EndsInDirectorySeparator(normalizedRoot)
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;
        return normalizedCandidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase);
    }

    private static bool PathsEqual(string left, string right) =>
        string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            StringComparison.OrdinalIgnoreCase);

    private static void ValidateManifestHeader(BackupManifest manifest, string requestedName)
    {
        if (manifest.SchemaVersion != ManifestSchemaVersion)
            throw new InvalidDataException($"Unsupported manifest schema version '{manifest.SchemaVersion}'.");
        if (!string.Equals(manifest.BackupName, requestedName, StringComparison.Ordinal))
            throw new InvalidDataException("Manifest backup name does not match its directory.");
        if (manifest.CreatedAtUtc == default || string.IsNullOrWhiteSpace(manifest.ApplicationVersion))
            throw new InvalidDataException("Manifest header is incomplete.");
        if (manifest.Artifacts is null)
            throw new InvalidDataException("Manifest artifacts are missing.");
    }

    private static void ValidateArtifactMetadata(BackupArtifact artifact)
    {
        if (string.IsNullOrWhiteSpace(artifact.StoredPath) || artifact.Length < 0)
            throw new InvalidDataException("Artifact metadata is incomplete.");
        if (!TryDecodeSha256(artifact.Sha256, out _))
            throw new InvalidDataException("Artifact SHA-256 value is malformed.");
    }

    private static void EnsureFingerprintMatches(BackupArtifact artifact, FileFingerprint fingerprint)
    {
        if (fingerprint.Length != artifact.Length ||
            !string.Equals(fingerprint.Sha256, artifact.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Artifact integrity check failed for '{artifact.RestorePath}'.");
        }
    }

    private void EnsureArtifactCapacity(int count)
    {
        if (count > _options.MaximumArtifactCount)
            throw new InvalidDataException("Backup artifact count exceeds the configured limit.");
    }

    private long AddWithinLimit(long current, long added)
    {
        var total = checked(current + added);
        if (total > _options.MaximumSourceBytes)
            throw new InvalidDataException("Backup source size exceeds the configured limit.");
        return total;
    }

    private static bool TryDecodeSha256(string? value, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        if (value is null || value.Length != 64)
            return false;
        try
        {
            bytes = Convert.FromHexString(value);
            return bytes.Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string? ValidateRestoreApproval(SystemBackupRestoreApproval approval)
    {
        if (!approval.Approved)
            return "Restore request is not approved.";
        if (!approval.MaintenanceWindowConfirmed)
            return "Restore requires a confirmed maintenance window.";
        if (!approval.RequestSignatureVerified)
            return "Restore request signature has not been verified.";
        if (string.IsNullOrWhiteSpace(approval.ApprovedBy) || approval.ApprovedBy.Length > 128)
            return "Restore approver identity is required.";
        if (string.IsNullOrWhiteSpace(approval.ChangeReference) || approval.ChangeReference.Length > 128)
            return "Restore change reference is required.";

        var now = DateTimeOffset.UtcNow;
        if (approval.ExpiresAtUtc <= now || approval.ExpiresAtUtc > now.AddHours(24))
            return "Restore approval is expired or exceeds the 24-hour authorization window.";
        return null;
    }

    private static LoadedBackup InvalidLoaded(string name, string path, string message) => new(
        path,
        null,
        new SystemBackupValidationResult(false, name, 0, 0, null, message));

    private static SystemBackupResult Failure(string name, string path, string message) =>
        new(false, name, path, 0, 0, message);

    private void Log(string message)
    {
        try
        {
            OnLog?.Invoke(message);
        }
        catch
        {
            // A log consumer cannot change backup transaction semantics.
        }
    }

    private enum BackupArtifactKind
    {
        Database,
        Configuration,
        Journal
    }

    private sealed class BackupManifest
    {
        public int SchemaVersion { get; init; }
        public string BackupName { get; init; } = string.Empty;
        public DateTimeOffset CreatedAtUtc { get; init; }
        public string ApplicationVersion { get; init; } = string.Empty;
        public List<BackupArtifact> Artifacts { get; init; } = new();
    }

    private sealed record BackupArtifact(
        BackupArtifactKind Kind,
        string StoredPath,
        string RestorePath,
        long Length,
        string Sha256);

    private sealed record FileFingerprint(long Length, string Sha256);
    private sealed record RollbackAction(string TargetPath, string RollbackPath, bool OriginalExisted);
    private sealed record LoadedBackup(
        string DirectoryPath,
        BackupManifest? Manifest,
        SystemBackupValidationResult Validation);
}
