using System.IO.Compression;
using EJLive.Client.WinForms.Services;
using EJLive.Core.Engine;
using CoreNetworkEngine = EJLive.Core.Engine.NetworkEngine;

namespace EJLive.Client.WinForms.Agent;

/// <summary>
/// Creates periodic local ZIP backups for journal files and emits backup notifications.
/// </summary>
public sealed class LogBackupScheduler : IDisposable
{
    public sealed record BackupArtifact(string ArchivePath, long Bytes, DateTime CreatedAtUtc);

    private readonly Func<bool> _isConnected;
    private readonly Action<string> _sendText;
    private readonly string _atmId;
    private readonly string _journalPath;
    private readonly string _backupPath;
    private readonly string _atmType;
    private System.Threading.Timer? _timer;

    public event Action<string>? OnLog;
    public event Action<BackupArtifact>? OnBackupCreated;
    public int BackupCount { get; private set; }
    public DateTime LastBackup { get; private set; }

    public LogBackupScheduler(NetworkManager? network, string atmId, string journalPath, string backupPath, string atmType)
    {
        _isConnected = () => network?.IsConnected == true;
        _sendText = text => network?.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, text));
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        _journalPath = journalPath ?? string.Empty;
        _backupPath = backupPath ?? string.Empty;
        _atmType = atmType ?? string.Empty;
    }

    public LogBackupScheduler(CoreNetworkEngine? network, string atmId, string journalPath, string backupPath, string atmType)
    {
        _isConnected = () => network?.IsConnected == true;
        _sendText = text => network?.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, text));
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        _journalPath = journalPath ?? string.Empty;
        _backupPath = backupPath ?? string.Empty;
        _atmType = atmType ?? string.Empty;
    }

    public void Start()
    {
        _timer = new System.Threading.Timer(_ => RunNow(), null, TimeSpan.FromMinutes(10), TimeSpan.FromHours(6));
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void RunNow()
    {
        _ = Task.Run(DoBackup);
    }

    private void DoBackup()
    {
        try
        {
            if (!Directory.Exists(_journalPath))
                return;

            var files = ResolveJournalFiles();
            if (files.Length == 0)
                return;

            Directory.CreateDirectory(_backupPath);
            var archivePath = Path.Combine(_backupPath, $"{_atmId}_{_atmType}_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
            using (var zip = ZipFile.Open(archivePath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    try
                    {
                        zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Fastest);
                    }
                    catch
                    {
                        // ignore file-level errors to keep batch resilient
                    }
                }
            }

            BackupCount++;
            LastBackup = DateTime.UtcNow;
            var artifactBytes = new FileInfo(archivePath).Length;
            OnLog?.Invoke($"Backup #{BackupCount}: {Path.GetFileName(archivePath)} ({files.Length} files)");
            OnBackupCreated?.Invoke(new BackupArtifact(archivePath, artifactBytes, LastBackup));

            if (_isConnected())
            {
                _sendText($"BACKUP_DONE|{_atmId}|{Path.GetFileName(archivePath)}|{artifactBytes}");
            }

            PruneBackups();
        }
        catch (Exception ex)
        {
            OnLog?.Invoke("Backup error: " + ex.Message);
        }
    }

    private string[] ResolveJournalFiles()
    {
        var type = _atmType.Trim().ToUpperInvariant();
        if (type == "GRG")
        {
            return Directory.GetFiles(_journalPath, "EJ_*.dat", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(_journalPath, "TRACE*", SearchOption.TopDirectoryOnly))
                .ToArray();
        }

        return Directory.GetFiles(_journalPath, "*.LOG", SearchOption.TopDirectoryOnly);
    }

    private void PruneBackups()
    {
        foreach (var file in Directory.GetFiles(_backupPath, "*.zip", SearchOption.TopDirectoryOnly))
        {
            try
            {
                if (File.GetCreationTimeUtc(file) < DateTime.UtcNow.AddDays(-30))
                    File.Delete(file);
            }
            catch
            {
                // best-effort retention cleanup
            }
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
