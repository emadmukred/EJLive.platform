using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace EJLive.Server.WinForms.Services
{
    public partial class ArchiveManager
    {
        private string _storagePath;
        private readonly string _archiveRoot;
        private readonly byte[] _atRestKey;
        private string _archivePath;
        private string _baseStoragePath;
        public long TotalArchivedFiles { get; private set; }
        public long TotalArchivedBytes { get; private set; }
        public ArchiveManager(string storagePath)
        {
            _storagePath = storagePath;
        }
        public ArchiveManager(string? archiveRoot = null, byte[]? atRestKey = null)
        {
            _archiveRoot = archiveRoot ?? AppConstants.DefaultArchivePath;
            _atRestKey   = atRestKey   ?? SecurityHelper.DeriveKey("EJLive.AtRest.2026");
            if (!Directory.Exists(_archiveRoot))
                Directory.CreateDirectory(_archiveRoot);
        }
        public ArchiveManager(string archiveRoot = null, byte[] atRestKey = null)
        {
            _archiveRoot = archiveRoot ?? AppConstants.DefaultArchivePath;
            _atRestKey   = atRestKey   ?? SecurityHelper.DeriveKey("EJLive.AtRest.2026");
            if (!Directory.Exists(_archiveRoot))
                Directory.CreateDirectory(_archiveRoot);
        }
        public ArchiveManager(string storage, string archive)
        {
            _storagePath = storage;
            _archivePath = archive;
            if (!Directory.Exists(_archivePath)) Directory.CreateDirectory(_archivePath);
        }
        public ArchiveManager(string storagePath)
        {
            _baseStoragePath = storagePath;
            if (!Directory.Exists(_baseStoragePath))
                Directory.CreateDirectory(_baseStoragePath);
        }
        public void ArchiveOldData(int daysOld = 30)
        {
            Log("Starting archive process (files older than " + daysOld + " days)...");
            if (!Directory.Exists(_storagePath))
            {
                Log("Storage path does not exist: " + _storagePath);
                return;
            }
            string[] atmDirs = Directory.GetDirectories(_storagePath);
            int totalArchived = 0;
            foreach (string atmDir in atmDirs)
            {
                string atmId = Path.GetFileName(atmDir);
                string[] monthDirs = Directory.GetDirectories(atmDir);
                foreach (string monthDir in monthDirs)
                {
                    string monthName = Path.GetFileName(monthDir);
                    string archivePath = Path.Combine(atmDir, "Archives");
                    if (!Directory.Exists(archivePath))
                        Directory.CreateDirectory(archivePath);
                    string zipFile = Path.Combine(archivePath, atmId + "_" + monthName + ".zip");
                    // Check if directory has old files
                    DirectoryInfo di = new DirectoryInfo(monthDir);
                    if (di.CreationTime < DateTime.Now.AddDays(-daysOld))
                    {
                        try
                        {
                            if (File.Exists(zipFile))
                                File.Delete(zipFile);
                            ZipFile.CreateFromDirectory(monthDir, zipFile);
                            Directory.Delete(monthDir, true);
                            totalArchived++;
                            Log("Archived: " + atmId + "/" + monthName + " -> " + Path.GetFileName(zipFile));
                        }
                        catch (Exception ex)
                        {
                            Log("Archive error for " + atmId + "/" + monthName + ": " + ex.Message);
                        }
                    }
                }
            }
            Log("Archive complete. Total directories archived: " + totalArchived);
        }
        public StorageStats GetStorageStats()
        {
            var stats = new StorageStats();
            if (!Directory.Exists(_storagePath)) return stats;
            string[] files = Directory.GetFiles(_storagePath, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                stats.TotalSize += fi.Length;
                stats.TotalFiles++;
                if (fi.Extension == ".zip")
                    stats.ArchivedSize += fi.Length;
            }
            stats.ATMCount = Directory.GetDirectories(_storagePath).Length;
            return stats;
        }
        private void Log(string message)
        {
            OnLogMessage?.Invoke("[Archive] " + message);
        }
        public string? Archive(string atmId, string fileName, byte[] data, string checksum, string? sha256 = null)
        {
            if (string.IsNullOrEmpty(atmId) || data == null || data.Length == 0)
                throw new ArgumentNullException("atmId/data");
            try
            {
                var actualMd5 = SecurityHelper.MD5Hash(data);
                if (!string.IsNullOrEmpty(checksum) &&
                    !string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase))
                {
                    OnError?.Invoke(this, $"Checksum mismatch for {fileName} — refusing to archive corrupted data");
                    return null;
                }
                var month      = DateTime.UtcNow.ToString("yyyy-MM");
                var atmFolder  = Path.Combine(_archiveRoot, atmId, month);
                Directory.CreateDirectory(atmFolder);
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
                var safeFile  = SanitizeFileName(fileName);
                var archName  = $"{atmId}_{safeFile}_{timestamp}.ejl";
                var archPath  = Path.Combine(atmFolder, archName);
                var encPath   = archPath + ".enc";
                var compressed  = SecurityHelper.Compress(data);
                var encrypted   = SecurityHelper.EncryptAES(compressed, _atRestKey);
                WriteArchiveFile(encPath, data.Length, compressed.Length, encrypted, actualMd5, sha256 ?? SecurityHelper.SHA256Hash(data));
                TotalArchivedFiles++;
                TotalArchivedBytes += data.Length;
                var entry = new JournalEntry
                {
                    EntryId          = Guid.NewGuid().ToString("N"),
                    ATMId            = atmId,
                    FileName         = fileName,
                    OriginalSize     = data.Length,
                    CompressedSize   = compressed.Length,
                    EncryptedSize    = encrypted.Length,
                    IsEncrypted      = true,
                    IsCompressed     = true,
                    Checksum         = actualMd5,
                    MD5Hash          = actualMd5,
                    SHA256Hash       = sha256 ?? SecurityHelper.SHA256Hash(data),
                    ArchivePath      = encPath,
                    MonthPartition   = month,
                    ReceivedAt       = DateTime.UtcNow,
                    VerifiedAt       = DateTime.UtcNow
                };
                DatabaseManager.Instance.InsertArchiveEntry(entry);
                OnArchived?.Invoke(this, encPath);
                AppLogger.Instance.Info($"Archived: {atmId}/{month}/{archName} [{data.Length / 1024.0:F1} KB -> {compressed.Length / 1024.0:F1} KB]", "Archive");
                return encPath;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Archive error: {ex.Message}");
                AppLogger.Instance.Error(ex, "ArchiveManager");
                return null;
            }
        }
        public byte[]? Retrieve(string archivePath)
        {
            if (!File.Exists(archivePath)) return null;
            try
            {
                return ReadArchiveFile(archivePath);
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error(ex, "ArchiveManager.Retrieve");
                return null;
            }
        }
        public string? RetrieveAsText(string archivePath)
        {
            var data = Retrieve(archivePath);
            return data != null ? System.Text.Encoding.UTF8.GetString(data) : null;
        }
        public (long fileCount, long totalBytes) GetATMArchiveStats(string atmId)
        {
            var atmFolder = Path.Combine(_archiveRoot, atmId);
            if (!Directory.Exists(atmFolder)) return (0, 0);
            long count = 0, bytes = 0;
            foreach (var file in Directory.GetFiles(atmFolder, "*.ejl.enc", SearchOption.AllDirectories))
            {
                count++;
                bytes += new FileInfo(file).Length;
            }
            return (count, bytes);
        }
        public double GetFreeSpaceGB()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(_archiveRoot)!);
                return drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            }
            catch { return 0; }
        }
        public double GetTotalSpaceGB()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(_archiveRoot)!);
                return drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
            }
            catch { return 0; }
        }
        private void WriteArchiveFile(string path, long origSize, long compSize, byte[] encData, string md5, string sha256)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            var w = new BinaryWriter(fs);
            w.Write(new byte[] { 0x45, 0x4A, 0x4C, 0x32 }); // "EJL2"
            w.Write((byte)1);                                  // Version
            w.Write(origSize);
            w.Write(compSize);
            var md5Raw = System.Text.Encoding.ASCII.GetBytes(md5.PadRight(32).Substring(0, 32));
            var shaRaw = System.Text.Encoding.ASCII.GetBytes(sha256.PadRight(64).Substring(0, 64));
            fs.Write(md5Raw, 0, 32);
            fs.Write(shaRaw, 0, 64);
            fs.Write(encData, 0, encData.Length);
        }
        private byte[] ReadArchiveFile(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var r = new BinaryReader(fs);
            var magic = r.ReadBytes(4);
            if (magic[0] != 0x45 || magic[1] != 0x4A || magic[2] != 0x4C || magic[3] != 0x32)
                throw new InvalidDataException("Invalid EJL archive file");
            r.ReadByte();
            var origSize  = r.ReadInt64();
            var compSize  = r.ReadInt64();
            var md5Bytes  = r.ReadBytes(32);
            var sha256B   = r.ReadBytes(64);
            var encData   = r.ReadBytes((int)(fs.Length - fs.Position));
            var decrypted   = SecurityHelper.DecryptAES(encData, _atRestKey);
            var decompressed = SecurityHelper.Decompress(decrypted);
            var actualMd5 = SecurityHelper.MD5Hash(decompressed);
            var storedMd5 = System.Text.Encoding.ASCII.GetString(md5Bytes).Trim();
            if (!string.Equals(actualMd5, storedMd5, StringComparison.OrdinalIgnoreCase))
                AppLogger.Instance.Warning($"Archive MD5 mismatch on retrieve: {path}", "Archive");
            return decompressed;
        }
        private static string SanitizeFileName(string name) =>
            string.IsNullOrEmpty(name) ? "journal"
            : string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
        public string Archive(string atmId, string fileName, byte[] data, string checksum, string sha256 = null)
        {
            if (string.IsNullOrEmpty(atmId) || data == null || data.Length == 0)
                throw new ArgumentNullException("atmId/data");
            try
            {
                // التحقق من سلامة البيانات (L-10)
                var actualMd5 = SecurityHelper.MD5Hash(data);
                if (!string.IsNullOrEmpty(checksum) &&
                    !string.Equals(actualMd5, checksum, StringComparison.OrdinalIgnoreCase))
                {
                    OnError?.Invoke(this, $"Checksum mismatch for {fileName} — refusing to archive corrupted data");
                    return null;
                }
                // بناء مسار الأرشيف (A-04: Monthly Partition)
                var month      = DateTime.UtcNow.ToString("yyyy-MM");
                var atmFolder  = Path.Combine(_archiveRoot, atmId, month);
                Directory.CreateDirectory(atmFolder);
                // اسم الملف الأرشيفي
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
                var safeFile  = SanitizeFileName(fileName);
                var archName  = $"{atmId}_{safeFile}_{timestamp}.ejl";
                var archPath  = Path.Combine(atmFolder, archName);
                var encPath   = archPath + ".enc";
                // ضغط + تشفير At-Rest (S-04)
                var compressed  = SecurityHelper.Compress(data);
                var encrypted   = SecurityHelper.EncryptAES(compressed, _atRestKey);
                // كتابة الملف مع Header
                WriteArchiveFile(encPath, data.Length, compressed.Length, encrypted, actualMd5, sha256 ?? SecurityHelper.SHA256Hash(data));
                TotalArchivedFiles++;
                TotalArchivedBytes += data.Length;
                // تسجيل في قاعدة البيانات (A-04)
                var entry = new JournalEntry
                {
                    EntryId          = Guid.NewGuid().ToString("N"),
                    ATMId            = atmId,
                    FileName         = fileName,
                    OriginalSize     = data.Length,
                    CompressedSize   = compressed.Length,
                    EncryptedSize    = encrypted.Length,
                    IsEncrypted      = true,
                    IsCompressed     = true,
                    Checksum         = actualMd5,
                    MD5Hash          = actualMd5,
                    SHA256Hash       = sha256 ?? SecurityHelper.SHA256Hash(data),
                    ArchivePath      = encPath,
                    MonthPartition   = month,
                    ReceivedAt       = DateTime.UtcNow,
                    VerifiedAt       = DateTime.UtcNow
                };
                DatabaseManager.Instance.InsertArchiveEntry(entry);
                OnArchived?.Invoke(this, encPath);
                AppLogger.Instance.Info($"✓ Archived: {atmId}/{month}/{archName} [{data.Length / 1024.0:F1} KB → {compressed.Length / 1024.0:F1} KB]", "Archive");
                return encPath;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"Archive error: {ex.Message}");
                AppLogger.Instance.Error(ex, "ArchiveManager");
                return null;
            }
        }
        public byte[] Retrieve(string archivePath)
        {
            if (!File.Exists(archivePath)) return null;
            try
            {
                return ReadArchiveFile(archivePath);
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error(ex, "ArchiveManager.Retrieve");
                return null;
            }
        }
        public string RetrieveAsText(string archivePath)
        {
            var data = Retrieve(archivePath);
            return data != null ? System.Text.Encoding.UTF8.GetString(data) : null;
        }
        private void WriteArchiveFile(string path, long origSize, long compSize, byte[] encData, string md5, string sha256)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            var w = new BinaryWriter(fs);
            w.Write(new byte[] { 0x45, 0x4A, 0x4C, 0x32 }); // "EJL2"
            w.Write((byte)1);                                  // Version
            w.Write(origSize);
            w.Write(compSize);
            var md5Bytes  = new byte[32]; var md5Raw = System.Text.Encoding.ASCII.GetBytes(md5.PadRight(32).Substring(0, 32));
            var sha256Bytes = new byte[64]; var shaRaw = System.Text.Encoding.ASCII.GetBytes(sha256.PadRight(64).Substring(0, 64));
            fs.Write(md5Raw, 0, 32);
            fs.Write(shaRaw, 0, 64);
            fs.Write(encData, 0, encData.Length);
        }
        public double GetFreeSpaceGB()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(_archiveRoot));
                return drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            }
            catch { return 0; }
        }
        public double GetTotalSpaceGB()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(_archiveRoot));
                return drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
            }
            catch { return 0; }
        }
        private string SanitizeFileName(string name) =>
            string.IsNullOrEmpty(name) ? "journal"
            : string.Concat(name.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
        public void StartAutoArchive()
        {
            new Thread(() =>
            {
                while (true)
                {
                    // Check if it's the first day of the month
                    if (DateTime.Now.Day == 1)
                    {
                        PerformMonthlyArchive();
                    }
                    Thread.Sleep(24 * 60 * 60 * 1000); // Check daily
                }
            }).Start();
        }
        public void PerformMonthlyArchive()
        {
            string lastMonth = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
            string zipFile = Path.Combine(_archivePath, $"Archive_{lastMonth}.zip");
            if (!File.Exists(zipFile))
            {
                try
                {
                    ZipFile.CreateFromDirectory(_storagePath, zipFile);
                    // Optionally clear storage after archiving
                }
                catch { }
            }
        }
        public void RunMonthlyArchive()
        {
            DateTime lastMonth = DateTime.Now.AddMonths(-1);
            string monthPattern = lastMonth.ToString("yyyyMM");
            foreach (string atmDir in Directory.GetDirectories(_baseStoragePath))
            {
                string atmID = Path.GetFileName(atmDir);
                string archiveName = Path.Combine(atmDir, $"Archive_{atmID}_{monthPattern}.zip");
                if (File.Exists(archiveName)) continue;
                var filesToArchive = Directory.GetFiles(atmDir, "*.bak")
                    .Where(f => File.GetCreationTime(f).ToString("yyyyMM") == monthPattern)
                    .ToList();
                if (filesToArchive.Any())
                {
                    using (ZipArchive archive = ZipFile.Open(archiveName, ZipArchiveMode.Create))
                    {
                        foreach (string file in filesToArchive)
                        {
                            archive.CreateEntryFromFile(file, Path.GetFileName(file));
                            File.Delete(file); // Remove original after archiving
                        }
                    }
                }
            }
        }
        public event Action<string> OnLogMessage;
        public event EventHandler<string>? OnArchived;
        public event EventHandler<string>? OnError;
        public event EventHandler<string> OnArchived;
        public event EventHandler<string> OnError;
    }

    public partial class StorageStats
    {
        public string TotalSizeFormatted
        {
            get { return FormatSize(TotalSize); }
        }
        public string ArchivedSizeFormatted
        {
            get { return FormatSize(ArchivedSize); }
        }
        public long TotalSize { get; set; }
        public long ArchivedSize { get; set; }
        public int TotalFiles { get; set; }
        public int ATMCount { get; set; }
        public string TotalSizeFormatted => FormatSize(TotalSize);
        public string ArchivedSizeFormatted => FormatSize(ArchivedSize);
        private string FormatSize(long bytes)
        {
            if (bytes >= 1073741824) return (bytes / 1073741824.0).ToString("F2") + " GB";
            if (bytes >= 1048576) return (bytes / 1048576.0).ToString("F2") + " MB";
            if (bytes >= 1024) return (bytes / 1024.0).ToString("F2") + " KB";
            return bytes + " B";
        }
        private static string FormatSize(long bytes)
        {
            if (bytes >= 1073741824) return (bytes / 1073741824.0).ToString("F2") + " GB";
            if (bytes >= 1048576) return (bytes / 1048576.0).ToString("F2") + " MB";
            if (bytes >= 1024) return (bytes / 1024.0).ToString("F2") + " KB";
            return bytes + " B";
        }
    }

}
