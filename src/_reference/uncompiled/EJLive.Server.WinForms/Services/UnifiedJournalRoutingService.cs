using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EJLive.Server.Services
{
    public partial class JournalRoutingEntry
    {
        public string EntryId { get; set; } = Guid.NewGuid().ToString("N");
        public string ATMId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string StoredPath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string Status { get; set; } = "Pending";
        public string Checksum { get; set; } = string.Empty;
        public DateTime StoredAtUtc { get; set; } = DateTime.UtcNow;
    }

    public partial class JournalRoutingSummary
    {
        public int TotalRoutes { get; set; }
        public long TotalBytesStored { get; set; }
        public int FailedRoutes { get; set; }
        public string StorageRoot { get; set; } = string.Empty;
        public DateTime GeneratedAtUtc { get; set; }
        public string TotalBytesDisplay =>
            TotalBytesStored > 1073741824 ? $"{TotalBytesStored / 1073741824.0:F1} GB" :
            TotalBytesStored > 1048576 ? $"{TotalBytesStored / 1048576.0:F1} MB" :
            $"{TotalBytesStored / 1024.0:F1} KB";
    }

    public partial class UnifiedJournalRoutingService
    {
        private readonly string _smartStorageRoot;
        private readonly Dictionary<string, JournalRoutingEntry> _routingTable = new(StringComparer.OrdinalIgnoreCase);
        public UnifiedJournalRoutingService()
        {
            _smartStorageRoot = Path.Combine(AppConstants.DefaultServerSharePath, "SmartStorage");
            EnsureDirectories();
        }
        public UnifiedJournalRoutingService(string storageRoot)
        {
            _smartStorageRoot = storageRoot ?? Path.Combine(AppConstants.DefaultServerSharePath, "SmartStorage");
            EnsureDirectories();
        }
        public string RouteJournalFile(string atmId, string fileName, byte[] data)
        {
            var now = DateTime.UtcNow;
            var targetPath = BuildStoragePath(atmId, now, fileName);
            try
            {
                var dir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(targetPath, data ?? Array.Empty<byte>());
                var entry = new JournalRoutingEntry
                {
                    EntryId = Guid.NewGuid().ToString("N"),
                    ATMId = atmId,
                    FileName = fileName,
                    StoredPath = targetPath,
                    FileSizeBytes = data?.Length ?? 0,
                    Status = "Stored",
                    StoredAtUtc = now
                };
                _routingTable[entry.EntryId] = entry;
                Log($"Journal routed: {fileName} → {targetPath}");
                return targetPath;
            }
            catch (Exception ex)
            {
                Log($"Routing error for {fileName}: {ex.Message}");
                throw;
            }
        }
        public JournalRoutingEntry? GetRoutingStatus(string entryId)
        {
            _routingTable.TryGetValue(entryId, out var entry);
            return entry;
        }
        public IReadOnlyList<JournalRoutingEntry> GetAtmRoutes(string atmId, int maxCount = 100)
        {
            return _routingTable.Values
                .Where(e => string.Equals(e.ATMId, atmId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.StoredAtUtc)
                .Take(maxCount)
                .ToList();
        }
        public JournalRoutingSummary GetSummary()
        {
            var entries = _routingTable.Values;
            return new JournalRoutingSummary
            {
                TotalRoutes = entries.Count,
                TotalBytesStored = entries.Sum(e => e.FileSizeBytes),
                FailedRoutes = entries.Count(e => e.Status == "Failed"),
                StorageRoot = _smartStorageRoot,
                GeneratedAtUtc = DateTime.UtcNow
            };
        }
        private string BuildStoragePath(string atmId, DateTime date, string fileName)
        {
            return Path.Combine(
                _smartStorageRoot,
                SanitizePathSegment(atmId),
                date.Year.ToString(),
                date.Month.ToString("D2"),
                SanitizeFileName(fileName));
        }
        private static string SanitizePathSegment(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "UNKNOWN";
            var invalid = Path.GetInvalidPathChars();
            var sanitized = new string(input.Where(c => !invalid.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? "UNKNOWN" : sanitized.Trim();
        }
        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "journal.bin";
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = new string(input.Where(c => !invalid.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(sanitized) ? "journal.bin" : sanitized.Trim();
        }
        private void EnsureDirectories()
        {
            try
            {
                if (!Directory.Exists(_smartStorageRoot))
                    Directory.CreateDirectory(_smartStorageRoot);
            }
            catch (Exception ex)
            {
                Log($"Storage directory creation error: {ex.Message}");
            }
        }
        private void Log(string message) => OnLog?.Invoke($"[JournalRouting] {message}");
        public event Action<string>? OnLog;
    }

}
