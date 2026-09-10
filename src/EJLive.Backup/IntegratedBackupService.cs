using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Backup
{
    /// <summary>
    /// خدمة النسخ الاحتياطي المتكاملة - Integrated Backup Service
    /// توفر وظائف متقدمة للنسخ الاحتياطي والاستعادة
    /// </summary>
    public class IntegratedBackupService
    {
        private readonly UnifiedSystemConfiguration _config;
        private readonly DatabaseManager _database;
        private readonly string _backupPath;

        public IntegratedBackupService(UnifiedSystemConfiguration config, DatabaseManager database)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _backupPath = Path.Combine(_config.BasePath, "Backups");
        }

        /// <summary>
        /// Creates a full backup with an archive-friendly name.
        /// </summary>
        public async Task<bool> CreateBackupAsync(string backupName)
        {
            try
            {
                var safeName = string.IsNullOrWhiteSpace(backupName)
                    ? $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}"
                    : SanitizeFileName(backupName);

                var backupDir = Path.Combine(_backupPath, safeName);
                Directory.CreateDirectory(backupDir);

                await BackupDatabaseAsync(backupDir);
                await BackupConfigurationAsync(backupDir);
                await BackupJournalsAsync(backupDir);

                WriteBackupMetaFile(backupDir, safeName);

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error($"Backup creation failed: {ex.Message}", "Backup");
                return false;
            }
        }

        /// <summary>
        /// Restores a previously created backup by name.
        /// </summary>
        public async Task<bool> RestoreBackupAsync(string backupName)
        {
            try
            {
                var backupDir = Path.Combine(_backupPath, backupName);
                if (!Directory.Exists(backupDir))
                {
                    AppLogger.Instance.Warning($"Backup directory not found: {backupDir}", "Backup");
                    return false;
                }

                await RestoreDatabaseAsync(backupName);
                await RestoreConfigurationAsync(backupName);
                await RestoreJournalsAsync(backupName);

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error($"Backup restore failed: {ex.Message}", "Backup");
                return false;
            }
        }

        /// <summary>
        /// Lists all available backup directories.
        /// </summary>
        public List<BackupInfo> ListBackups()
        {
            var result = new List<BackupInfo>();
            try
            {
                if (!Directory.Exists(_backupPath))
                    return result;

                foreach (var dir in Directory.GetDirectories(_backupPath))
                {
                    var info = new DirectoryInfo(dir);
                    result.Add(new BackupInfo
                    {
                        Name = info.Name,
                        CreatedAt = info.CreationTime,
                        Size = GetDirectorySize(info),
                        Description = $"Backup: {info.Name}"
                    });
                }

                result.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            }
            catch { }
            return result;
        }

        // ── Private helpers ──────────────────────────────────────────

        private async Task BackupDatabaseAsync(string backupDir)
        {
            var dbPath = AppConstants.DefaultDatabasePath;
            if (!File.Exists(dbPath))
            {
                AppLogger.Instance.Warning($"Database file not found: {dbPath}", "Backup");
                return;
            }

            var destPath = Path.Combine(backupDir, "ejlive.db");
            await Task.Run(() => File.Copy(dbPath, destPath, overwrite: true));
            AppLogger.Instance.Info($"Database backed up: {destPath}", "Backup");
        }

        private async Task BackupConfigurationAsync(string backupDir)
        {
            var configDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Client");

            if (!Directory.Exists(configDir))
                return;

            var destDir = Path.Combine(backupDir, "Configuration");
            Directory.CreateDirectory(destDir);

            await Task.Run(() =>
            {
                foreach (var file in Directory.GetFiles(configDir, "*.cfg", SearchOption.TopDirectoryOnly))
                {
                    var dest = Path.Combine(destDir, Path.GetFileName(file));
                    File.Copy(file, dest, overwrite: true);
                }
            });
        }

        private async Task BackupJournalsAsync(string backupDir)
        {
            var journalRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Storage");

            if (!Directory.Exists(journalRoot))
                return;

            var destDir = Path.Combine(backupDir, "Journals");
            Directory.CreateDirectory(destDir);

            await Task.Run(() => CompressDirectory(journalRoot, Path.Combine(backupDir, "journals.zip")));
        }

        private async Task RestoreDatabaseAsync(string backupName)
        {
            var srcPath = Path.Combine(_backupPath, backupName, "ejlive.db");
            if (!File.Exists(srcPath))
            {
                AppLogger.Instance.Warning($"Backup database not found: {srcPath}", "Backup");
                return;
            }

            var destPath = AppConstants.DefaultDatabasePath;
            var destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDir))
                Directory.CreateDirectory(destDir);

            await Task.Run(() => File.Copy(srcPath, destPath, overwrite: true));
            AppLogger.Instance.Info($"Database restored: {destPath}", "Backup");
        }

        private async Task RestoreConfigurationAsync(string backupName)
        {
            var srcDir = Path.Combine(_backupPath, backupName, "Configuration");
            if (!Directory.Exists(srcDir))
                return;

            var destDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Client");

            Directory.CreateDirectory(destDir);

            await Task.Run(() =>
            {
                foreach (var file in Directory.GetFiles(srcDir, "*.cfg"))
                {
                    var dest = Path.Combine(destDir, Path.GetFileName(file));
                    File.Copy(file, dest, overwrite: true);
                }
            });
        }

        private async Task RestoreJournalsAsync(string backupName)
        {
            var zipPath = Path.Combine(_backupPath, backupName, "journals.zip");
            if (!File.Exists(zipPath))
                return;

            var destDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Storage");

            await Task.Run(() =>
            {
                if (Directory.Exists(destDir))
                {
                    // Clear before restore
                    var existing = Directory.GetFiles(destDir, "*", SearchOption.AllDirectories);
                    foreach (var f in existing) { try { File.Delete(f); } catch { } }
                }
                ZipFile.ExtractToDirectory(zipPath, destDir);
            });
        }

        private static void CompressDirectory(string sourceDir, string destZip)
        {
            try
            {
                if (File.Exists(destZip))
                    File.Delete(destZip);
                ZipFile.CreateFromDirectory(sourceDir, destZip, CompressionLevel.Optimal, includeBaseDirectory: false);
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error($"Compression failed: {ex.Message}", "Backup");
            }
        }

        private static void WriteBackupMetaFile(string backupDir, string name)
        {
            try
            {
                var metaPath = Path.Combine(backupDir, "backup-meta.json");
                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    name,
                    createdUtc = DateTime.UtcNow.ToString("o"),
                    appVersion = AppConstants.AppVersion
                }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(metaPath, json);
            }
            catch { }
        }

        private static long GetDirectorySize(DirectoryInfo dir)
        {
            long size = 0;
            try
            {
                foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
                {
                    try { size += file.Length; } catch { }
                }
            }
            catch { }
            return size;
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
                name = name.Replace(c, '_');
            return string.IsNullOrWhiteSpace(name) ? "backup" : name.Trim();
        }

        // ── Nested types ─────────────────────────────────────────────

        public class BackupInfo
        {
            public string Name { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public long Size { get; set; }
            public string Description { get; set; } = string.Empty;
        }
    }
}
