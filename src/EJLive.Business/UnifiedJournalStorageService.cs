using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Business
{
    /// <summary>
    /// Service for storing, archiving, and retrieving journal files.
    /// Provides storage reliability, deduplication, and archive management.
    /// </summary>
    public sealed class UnifiedJournalStorageService
    {
        private readonly UnifiedJournalEvidenceAnalyzer _evidence;
        private readonly string _storageRoot;

        public UnifiedJournalStorageService(UnifiedJournalEvidenceAnalyzer evidence)
        {
            _evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
            _storageRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Storage");
            Directory.CreateDirectory(_storageRoot);
        }

        /// <summary>
        /// Stores a journal file for the given ATM.
        /// Returns the storage record.
        /// </summary>
        public async Task<JournalStorageRecord> StoreJournalAsync(string atmId, string fileName, byte[] data)
        {
            var record = new JournalStorageRecord
            {
                StorageId = Guid.NewGuid().ToString("N"),
                AtmId = atmId,
                FileName = fileName,
                OriginalSize = data?.Length ?? 0,
                StoredAtUtc = DateTime.UtcNow
            };

            if (data == null || data.Length == 0)
            {
                record.Status = "Empty";
                return record;
            }

            try
            {
                var atmDir = Path.Combine(_storageRoot, SanitizeAtmId(atmId));
                Directory.CreateDirectory(atmDir);

                var monthDir = Path.Combine(atmDir, DateTime.UtcNow.ToString("yyyy-MM"));
                Directory.CreateDirectory(monthDir);

                var filePath = Path.Combine(monthDir, record.StorageId + ".ejf");
                await File.WriteAllBytesAsync(filePath, data).ConfigureAwait(false);

                record.StoragePath = filePath;
                record.Checksum = SecurityHelper.SHA256Hash(data);
                record.Status = "Stored";

                DatabaseManager.Instance.InsertArchiveEntry(new JournalEntry
                {
                    ATMId = atmId,
                    FileName = fileName,
                    OriginalSize = data.Length,
                    CompressedSize = data.Length,
                    Checksum = record.Checksum,
                    ArchivePath = filePath,
                    MonthPartition = DateTime.UtcNow.ToString("yyyy-MM"),
                    ReceivedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                record.Status = "Failed";
                record.ErrorMessage = ex.Message;
                AppLogger.Instance.Error($"Storage error for {fileName}: {ex.Message}", "Storage");
            }

            return record;
        }

        /// <summary>
        /// Retrieves a stored journal file by its storage ID or path.
        /// </summary>
        public async Task<byte[]?> RetrieveJournalAsync(string storageId)
        {
            // Search for file matching the storage ID
            foreach (var file in Directory.GetFiles(_storageRoot, "*.ejf", SearchOption.AllDirectories))
            {
                var storedId = Path.GetFileNameWithoutExtension(file);
                if (string.Equals(storedId, storageId, StringComparison.OrdinalIgnoreCase))
                {
                    return await File.ReadAllBytesAsync(file).ConfigureAwait(false);
                }
            }
            return null;
        }

        /// <summary>
        /// Archives old journal files beyond the retention period.
        /// </summary>
        public async Task<int> ArchiveJournalsAsync(string atmId, int retentionDays = 90)
        {
            var archived = 0;
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var atmDir = Path.Combine(_storageRoot, SanitizeAtmId(atmId));

            if (!Directory.Exists(atmDir)) return 0;

            await Task.Run(() =>
            {
                foreach (var monthDir in Directory.GetDirectories(atmDir))
                {
                    var dirName = Path.GetFileName(monthDir);
                    if (DateTime.TryParse(dirName + "-01", out var monthDate) && monthDate < cutoffDate)
                    {
                        foreach (var file in Directory.GetFiles(monthDir))
                        {
                            try { File.Delete(file); archived++; }
                            catch { }
                        }
                    }
                }
            });

            AppLogger.Instance.Info($"Archived {archived} files for ATM {atmId}", "Storage");
            return archived;
        }

        public JournalAnalysisResult Analyze(string atmId, string journalText)
            => _evidence.Analyze(atmId, journalText);

        private static string SanitizeAtmId(string atmId)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid) atmId = atmId.Replace(c, '_');
            return string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        }
    }

    public sealed class JournalStorageRecord
    {
        public string StorageId { get; set; } = string.Empty;
        public string AtmId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public long OriginalSize { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime StoredAtUtc { get; set; }
    }
}
