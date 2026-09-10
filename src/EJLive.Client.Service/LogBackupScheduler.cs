using System.IO.Compression;
using EJLive.Core.Engine;

namespace EJLive.Client.Service;

/// <summary>
/// Creates periodic journal archives inside the headless service process.
/// </summary>
public sealed class LogBackupScheduler : IDisposable
{
    public sealed record BackupArtifact(string ArchivePath, long Bytes, DateTime CreatedAtUtc);

    private readonly NetworkEngine? _network;
    private readonly string _atmId;
    private readonly string _journalPath;
    private readonly string _backupPath;
    private readonly string _atmType;
    private readonly string _archivePrefix;
    private System.Threading.Timer? _timer;
    private int _backupRunning;

    public event Action<string>? OnLog;
    public event Action<BackupArtifact>? OnBackupCreated;
    public int BackupCount { get; private set; }
    public DateTime LastBackupUtc { get; private set; }

    public LogBackupScheduler(NetworkEngine? network, string atmId, string journalPath, string backupPath, string atmType)
    {
        _network = network;
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        _journalPath = journalPath ?? string.Empty;
        _backupPath = backupPath ?? string.Empty;
        _atmType = atmType ?? string.Empty;
        _archivePrefix = $"{SanitizeFileNamePart(_atmId)}_{SanitizeFileNamePart(_atmType)}";
    }

    public void Start() => _timer ??= new System.Threading.Timer(
        _ => RunNow(),
        null,
        TimeSpan.FromMinutes(10),
        TimeSpan.FromHours(6));

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void RunNow() => _ = Task.Run(() =>
    {
        if (Interlocked.Exchange(ref _backupRunning, 1) == 1)
            return;

        try
        {
            CreateArchive();
        }
        finally
        {
            Interlocked.Exchange(ref _backupRunning, 0);
        }
    });

    private void CreateArchive()
    {
        try
        {
            if (!Directory.Exists(_journalPath))
                return;

            var files = ResolveJournalFiles();
            if (files.Length == 0)
                return;

            Directory.CreateDirectory(_backupPath);
            var archivePath = Path.Combine(_backupPath, $"{_archivePrefix}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.zip");
            using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                    archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Fastest);
            }

            BackupCount++;
            LastBackupUtc = DateTime.UtcNow;
            var bytes = new FileInfo(archivePath).Length;
            OnLog?.Invoke($"Backup #{BackupCount}: {Path.GetFileName(archivePath)} ({files.Length} files)");
            OnBackupCreated?.Invoke(new BackupArtifact(archivePath, bytes, LastBackupUtc));

            if (_network?.IsConnected == true)
            {
                _network.SendMessage(CommunicationProtocol.BuildFrame(
                    CommunicationProtocol.MsgType.Broadcast,
                    $"BACKUP_DONE|{_atmId}|{Path.GetFileName(archivePath)}|{bytes}"));
            }

            PruneExpiredArchives();
        }
        catch (Exception ex)
        {
            OnLog?.Invoke("Backup error: " + ex.Message);
        }
    }

    private string[] ResolveJournalFiles()
    {
        if (string.Equals(_atmType.Trim(), "GRG", StringComparison.OrdinalIgnoreCase))
        {
            return Directory.GetFiles(_journalPath, "EJ_*.dat", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(_journalPath, "TRACE*", SearchOption.TopDirectoryOnly))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        return Directory.GetFiles(_journalPath, "*.LOG", SearchOption.TopDirectoryOnly);
    }

    private void PruneExpiredArchives()
    {
        foreach (var archivePath in Directory.GetFiles(
                     _backupPath,
                     _archivePrefix + "_*.zip",
                     SearchOption.TopDirectoryOnly))
        {
            try
            {
                if (File.GetCreationTimeUtc(archivePath) < DateTime.UtcNow.AddDays(-30))
                    File.Delete(archivePath);
            }
            catch (IOException ex)
            {
                OnLog?.Invoke($"Backup retention warning for {Path.GetFileName(archivePath)}: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                OnLog?.Invoke($"Backup retention denied for {Path.GetFileName(archivePath)}: {ex.Message}");
            }
        }
    }

    private static string SanitizeFileNamePart(string? value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string((value ?? string.Empty)
            .Trim()
            .Where(character => !invalid.Contains(character))
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized) ? "UNKNOWN" : sanitized;
    }

    public void Dispose() => Stop();
}
