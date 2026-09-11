using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace EJLive.Server.Services
{
    public partial class JournalAnalyticsSummary
    {
        public int TotalEntries { get; set; }
        public long TotalBytes { get; set; }
        public int TotalTransactions { get; set; }
        public int FailedCount { get; set; }
        public int SyncedCount { get; set; }
        public DateTime GeneratedAtUtc { get; set; }
        public double SuccessRate => TotalEntries > 0 ? (double)SyncedCount / TotalEntries * 100.0 : 0;
        public string TotalBytesDisplay => TotalBytes > 1048576 ? $"{TotalBytes / 1048576.0:F1} MB" : $"{TotalBytes / 1024.0:F1} KB";
    }

    public partial class JournalAnalyticsService : IDisposable
    {
        private readonly string _archiveStoragePath;
        private readonly string _archivePath;
        private readonly List<JournalEntry> _recentEntries = new();
        private string _storagePath;
        private string _archivePath;
        private Dictionary<string, ATMJournalStats> _atmStats;
        private List<JournalRecord> _recentRecords;
        private System.Threading.Timer _autoArchiveTimer;
        public JournalAnalyticsService(string archiveStoragePath, string archivePath)
        {
            _archiveStoragePath = archiveStoragePath ?? AppConstants.DefaultServerSharePath;
            _archivePath = archivePath ?? AppConstants.DefaultArchivePath;
            EnsureDirectories();
        }
        public JournalAnalyticsService(string storagePath, string archivePath)
        {
            _storagePath = storagePath;
            _archivePath = archivePath;
            _atmStats = new Dictionary<string, ATMJournalStats>();
            _recentRecords = new List<JournalRecord>();
            if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);
            if (!Directory.Exists(_archivePath)) Directory.CreateDirectory(_archivePath);
            _autoArchiveTimer = new System.Threading.Timer(AutoArchiveCheck, null, TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));
        }
        public JournalAnalyticsSummary GetSummary(string? atmId = null)
        {
            var summary = new JournalAnalyticsSummary
            {
                GeneratedAtUtc = DateTime.UtcNow,
                TotalEntries = _recentEntries.Count,
                TotalBytes = _recentEntries.Sum(e => e.OriginalSize),
                TotalTransactions = _recentEntries.Sum(e => e.TransactionCount),
                FailedCount = _recentEntries.Count(e => e.Status == "Failed"),
                SyncedCount = _recentEntries.Count(e => e.Status == "Synced")
            };
            if (!string.IsNullOrWhiteSpace(atmId))
            {
                var atmEntries = _recentEntries.Where(e => string.Equals(e.ATMId, atmId, StringComparison.OrdinalIgnoreCase)).ToList();
                summary.TotalEntries = atmEntries.Count;
                summary.TotalBytes = atmEntries.Sum(e => e.OriginalSize);
                summary.TotalTransactions = atmEntries.Sum(e => e.TransactionCount);
                summary.FailedCount = atmEntries.Count(e => e.Status == "Failed");
                summary.SyncedCount = atmEntries.Count(e => e.Status == "Synced");
            }
            return summary;
        }
        public void RegisterEntry(JournalEntry entry)
        {
            if (entry == null) return;
            _recentEntries.Add(entry);
            Log($"Journal entry registered: {entry.FileName} ({entry.ATMId})");
        }
        public IReadOnlyList<JournalEntry> GetRecentEntries(string? atmId = null, int maxCount = 100)
        {
            var query = _recentEntries.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(atmId))
                query = query.Where(e => string.Equals(e.ATMId, atmId, StringComparison.OrdinalIgnoreCase));
            return query.OrderByDescending(e => e.ReceivedAt).Take(maxCount).ToList();
        }
        public JournalDailyStats GetDailyStats(DateTime date, string? atmId = null)
        {
            var entries = _recentEntries
                .Where(e => e.ReceivedAt.Date == date.Date)
                .Where(e => string.IsNullOrWhiteSpace(atmId) || string.Equals(e.ATMId, atmId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return new JournalDailyStats
            {
                ATMId = atmId ?? "ALL",
                Date = date,
                TotalTransactions = entries.Sum(e => e.TransactionCount),
                ApprovedTransactions = entries.Count(e => e.Status == "Synced"),
                FailedTransactions = entries.Count(e => e.Status == "Failed"),
                JournalBytesReceived = entries.Sum(e => e.OriginalSize),
                SyncSuccessPercent = entries.Count > 0
                    ? (double)entries.Count(e => e.Status == "Synced") / entries.Count * 100.0
                    : 100.0
            };
        }
        public int ArchiveEntries(int olderThanDays = 30)
        {
            var cutoff = DateTime.UtcNow.AddDays(-olderThanDays);
            var toArchive = _recentEntries
                .Where(e => e.ReceivedAt < cutoff && e.Status == "Synced")
                .ToList();
            foreach (var entry in toArchive)
            {
                _recentEntries.Remove(entry);
            }
            Log($"Archived {toArchive.Count} entries older than {olderThanDays} days.");
            return toArchive.Count;
        }
        private void EnsureDirectories()
        {
            try
            {
                if (!Directory.Exists(_archiveStoragePath))
                    Directory.CreateDirectory(_archiveStoragePath);
                if (!Directory.Exists(_archivePath))
                    Directory.CreateDirectory(_archivePath);
            }
            catch (Exception ex)
            {
                Log($"Directory creation error: {ex.Message}");
            }
        }
        private void Log(string message) => OnLog?.Invoke($"[JournalAnalytics] {message}");
        private readonly object _lock = new object();
        public void StoreJournalData(string atmId, string fileName, byte[] data, string checksum)
        {
            string atmFolder = Path.Combine(_storagePath, atmId);
            string dateFolder = Path.Combine(atmFolder, DateTime.Now.ToString("yyyy-MM"));
            if (!Directory.Exists(dateFolder)) Directory.CreateDirectory(dateFolder);
            string filePath = Path.Combine(dateFolder, fileName);
            File.WriteAllBytes(filePath, data);
            lock (_lock)
            {
                if (!_atmStats.ContainsKey(atmId))
                    _atmStats[atmId] = new ATMJournalStats { ATM_ID = atmId };
                _atmStats[atmId].TotalFiles++;
                _atmStats[atmId].TotalBytes += data.Length;
                _atmStats[atmId].LastReceived = DateTime.Now;
                _atmStats[atmId].LastFileName = fileName;
                _recentRecords.Add(new JournalRecord { ATM_ID = atmId, FileName = fileName, FileSize = data.Length, ReceivedAt = DateTime.Now, Checksum = checksum, StoragePath = filePath });
                if (_recentRecords.Count > 1000) _recentRecords.RemoveRange(0, _recentRecords.Count - 1000);
            }
            AnalyzeContent(atmId, data);
            OnLog?.Invoke("[Storage] " + atmId + " -> " + fileName + " (" + data.Length + " bytes)");
        }
        public void StoreFile(string atmId, string fileName, byte[] data)
        {
            string folder = Path.Combine(_storagePath, atmId, "files");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + fileName), data);
        }
        private void AnalyzeContent(string atmId, byte[] data)
        {
            try
            {
                string content = Encoding.UTF8.GetString(data);
                string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                lock (_lock)
                {
                    var stats = _atmStats[atmId];
                    foreach (string line in lines)
                    {
                        string u = line.ToUpperInvariant();
                        if (u.Contains("WITHDRAWAL") || u.Contains("DISPENSE")) stats.TotalWithdrawals++;
                        if (u.Contains("DEPOSIT") || u.Contains("CASH IN")) stats.TotalDeposits++;
                        if (u.Contains("ERROR") || u.Contains("FAULT") || u.Contains("JAM")) stats.TotalErrors++;
                        if (u.Contains("CARD RETAINED") || u.Contains("CARD CAPTURED")) stats.TotalCardRetained++;
                        if (u.Contains("PAPER LOW") || u.Contains("PAPER OUT")) stats.PaperWarnings++;
                        if (u.Contains("CASH LOW") || u.Contains("CASSETTE EMPTY")) stats.CashWarnings++;
                    }
                    stats.TotalLinesProcessed += lines.Length;
                }
            }
            catch { }
        }
        public string ArchiveMonth(string atmId, string yearMonth)
        {
            string src = Path.Combine(_storagePath, atmId, yearMonth);
            if (!Directory.Exists(src)) return null;
            string zip = Path.Combine(_archivePath, atmId + "_" + yearMonth.Replace("-", "") + ".zip");
            if (File.Exists(zip)) File.Delete(zip);
            System.IO.Compression.ZipFile.CreateFromDirectory(src, zip);
            OnLog?.Invoke("[Archive] " + zip);
            OnArchiveCompleted?.Invoke(atmId, zip);
            return zip;
        }
        private void AutoArchiveCheck(object state)
        {
            string prev = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
            try
            {
                if (!Directory.Exists(_storagePath)) return;
                foreach (var d in Directory.GetDirectories(_storagePath))
                {
                    string id = Path.GetFileName(d);
                    if (Directory.Exists(Path.Combine(d, prev))) ArchiveMonth(id, prev);
                }
            }
            catch (Exception ex) { OnLog?.Invoke("[AutoArchive] Error: " + ex.Message); }
        }
        public int ArchiveAll(int monthsOld = 1)
        {
            int count = 0;
            DateTime cutoff = DateTime.Now.AddMonths(-monthsOld);
            if (!Directory.Exists(_storagePath)) return 0;
            foreach (var d in Directory.GetDirectories(_storagePath))
            {
                string id = Path.GetFileName(d);
                foreach (var m in Directory.GetDirectories(d))
                {
                    string mn = Path.GetFileName(m);
                    if (DateTime.TryParse(mn + "-01", out DateTime dt) && dt < cutoff) { ArchiveMonth(id, mn); count++; }
                }
            }
            return count;
        }
        public string ExportCSVReport(string path, string atmId = null, DateTime? from = null, DateTime? to = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ATM_ID,FileName,Size,ReceivedAt,Checksum");
            lock (_lock)
            {
                foreach (var r in _recentRecords.Where(r => (atmId == null || r.ATM_ID == atmId) && (from == null || r.ReceivedAt >= from) && (to == null || r.ReceivedAt <= to)))
                    sb.AppendLine(r.ATM_ID + "," + r.FileName + "," + r.FileSize + "," + r.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss") + "," + r.Checksum);
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            OnLog?.Invoke("[CSV] " + path);
            return path;
        }
        public string ExportHTMLReport(string path, string atmId = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'><title>EJLive Report</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI;margin:20px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ddd;padding:8px}th{background:#2196F3;color:white}.card{display:inline-block;padding:15px;margin:10px;background:#f5f5f5;border-left:4px solid #2196F3;min-width:150px}</style></head><body>");
            sb.AppendLine("<h1>EJLive Journal Report</h1><p>" + DateTime.Now + "</p><div>");
            lock (_lock)
            {
                var stats = atmId != null && _atmStats.ContainsKey(atmId) ? new[] { _atmStats[atmId] } : _atmStats.Values.ToArray();
                sb.AppendLine("<div class='card'><b>Files</b><br>" + stats.Sum(s => s.TotalFiles) + "</div>");
                sb.AppendLine("<div class='card'><b>Size</b><br>" + (stats.Sum(s => s.TotalBytes) / 1024) + " KB</div>");
                sb.AppendLine("<div class='card'><b>Withdrawals</b><br>" + stats.Sum(s => s.TotalWithdrawals) + "</div>");
                sb.AppendLine("<div class='card'><b>Errors</b><br>" + stats.Sum(s => s.TotalErrors) + "</div>");
            }
            sb.AppendLine("</div><h2>Records</h2><table><tr><th>ATM</th><th>File</th><th>Size</th><th>Date</th></tr>");
            lock (_lock)
            {
                foreach (var r in _recentRecords.Where(r => atmId == null || r.ATM_ID == atmId).TakeLast(50))
                    sb.AppendLine("<tr><td>" + r.ATM_ID + "</td><td>" + r.FileName + "</td><td>" + r.FileSize + "</td><td>" + r.ReceivedAt + "</td></tr>");
            }
            sb.AppendLine("</table></body></html>");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            OnLog?.Invoke("[HTML] " + path);
            return path;
        }
        public ATMJournalStats GetATMStats(string atmId)
        {
            lock (_lock) { return _atmStats.ContainsKey(atmId) ? _atmStats[atmId] : new ATMJournalStats { ATM_ID = atmId }; }
        }
        public Dictionary<string, ATMJournalStats> GetAllStats()
        {
            lock (_lock) { return new Dictionary<string, ATMJournalStats>(_atmStats); }
        }
        public List<JournalRecord> GetRecentRecords(int count = 50, string atmId = null)
        {
            lock (_lock) { return _recentRecords.Where(r => atmId == null || r.ATM_ID == atmId).TakeLast(count).ToList(); }
        }
        public long GetStorageSize()
        {
            if (!Directory.Exists(_storagePath)) return 0;
            return new DirectoryInfo(_storagePath).EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
        }
        public void StoreJournalData(string atmId, string fileName, byte[] data, string checksum)
        {
            var safeAtmId = NormalizeAtmIdentifier(atmId);
            var safeFileName = NormalizeFileName(fileName, "journal");
            var payload = data ?? Array.Empty<byte>();
            string atmFolder = Path.Combine(_storagePath, BuildSafePathSegment(safeAtmId, "ATM_UNKNOWN"));
            string dateFolder = Path.Combine(atmFolder, DateTime.Now.ToString("yyyy-MM"));
            if (!Directory.Exists(dateFolder)) Directory.CreateDirectory(dateFolder);
            string filePath = Path.Combine(dateFolder, safeFileName);
            File.WriteAllBytes(filePath, payload);
            lock (_lock)
            {
                if (!_atmStats.ContainsKey(safeAtmId))
                    _atmStats[safeAtmId] = new ATMJournalStats { ATM_ID = safeAtmId };
                _atmStats[safeAtmId].TotalFiles++;
                _atmStats[safeAtmId].TotalBytes += payload.Length;
                _atmStats[safeAtmId].LastReceived = DateTime.Now;
                _atmStats[safeAtmId].LastFileName = safeFileName;
                _recentRecords.Add(new JournalRecord
                {
                    ATM_ID = safeAtmId,
                    FileName = safeFileName,
                    FileSize = payload.Length,
                    ReceivedAt = DateTime.Now,
                    Checksum = checksum ?? string.Empty,
                    StoragePath = filePath
                });
                if (_recentRecords.Count > 1000) _recentRecords.RemoveRange(0, _recentRecords.Count - 1000);
            }
            AnalyzeContent(safeAtmId, payload);
            if (!string.Equals(fileName, safeFileName, StringComparison.Ordinal) || !string.Equals(atmId, safeAtmId, StringComparison.Ordinal))
                OnLog?.Invoke("[Storage] Sanitized journal path tokens for safe storage.");
            OnLog?.Invoke("[Storage] " + safeAtmId + " -> " + safeFileName + " (" + payload.Length + " bytes)");
        }
        public void StoreFile(string atmId, string fileName, byte[] data)
        {
            var safeAtmId = NormalizeAtmIdentifier(atmId);
            var safeFileName = NormalizeFileName(fileName, "file");
            var payload = data ?? Array.Empty<byte>();
            string folder = Path.Combine(_storagePath, BuildSafePathSegment(safeAtmId, "ATM_UNKNOWN"), "files");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            File.WriteAllBytes(Path.Combine(folder, DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture) + "_" + safeFileName), payload);
        }
        private void AnalyzeContent(string atmId, byte[] data)
        {
            try
            {
                string content = Encoding.UTF8.GetString(data);
                string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                lock (_lock)
                {
                    var stats = _atmStats[atmId];
                    foreach (string line in lines)
                    {
                        string u = line.ToUpperInvariant();
                        if (u.Contains("WITHDRAWAL") || u.Contains("DISPENSE")) stats.TotalWithdrawals++;
                        if (u.Contains("DEPOSIT") || u.Contains("CASH IN")) stats.TotalDeposits++;
                        if (u.Contains("ERROR") || u.Contains("FAULT") || u.Contains("JAM")) stats.TotalErrors++;
                        if (u.Contains("CARD RETAINED") || u.Contains("CARD CAPTURED")) stats.TotalCardRetained++;
                        if (u.Contains("PAPER LOW") || u.Contains("PAPER OUT")) stats.PaperWarnings++;
                        if (u.Contains("CASH LOW") || u.Contains("CASSETTE EMPTY")) stats.CashWarnings++;
                    }
                    stats.TotalLinesProcessed += lines.Length;
                }
            }
            catch (Exception ex)
            {
                OnLog?.Invoke("[Analytics] Content analysis failed: " + ex.Message);
            }
        }
        public string? ArchiveMonth(string atmId, string yearMonth)
        {
            string src = Path.Combine(_storagePath, atmId, yearMonth);
            if (!Directory.Exists(src)) return null;
            string zip = Path.Combine(_archivePath, atmId + "_" + yearMonth.Replace("-", "") + ".zip");
            if (File.Exists(zip)) File.Delete(zip);
            System.IO.Compression.ZipFile.CreateFromDirectory(src, zip);
            OnLog?.Invoke("[Archive] " + zip);
            OnArchiveCompleted?.Invoke(atmId, zip);
            return zip;
        }
        private void AutoArchiveCheck(object? state)
        {
            string prev = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
            try
            {
                if (!Directory.Exists(_storagePath)) return;
                foreach (var d in Directory.GetDirectories(_storagePath))
                {
                    string id = Path.GetFileName(d);
                    if (Directory.Exists(Path.Combine(d, prev))) ArchiveMonth(id, prev);
                }
            }
            catch (Exception ex) { OnLog?.Invoke("[AutoArchive] Error: " + ex.Message); }
        }
        public string ExportCSVReport(string path, string? atmId = null, DateTime? from = null, DateTime? to = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ATM_ID,FileName,Size,ReceivedAt,Checksum");
            lock (_lock)
            {
                foreach (var r in _recentRecords.Where(r => (atmId == null || r.ATM_ID == atmId) && (from == null || r.ReceivedAt >= from) && (to == null || r.ReceivedAt <= to)))
                {
                    sb.AppendLine(string.Join(",",
                        EscapeCsv(r.ATM_ID),
                        EscapeCsv(r.FileName),
                        r.FileSize.ToString(CultureInfo.InvariantCulture),
                        EscapeCsv(r.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                        EscapeCsv(r.Checksum)));
                }
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            OnLog?.Invoke("[CSV] " + path);
            return path;
        }
        public string ExportHTMLReport(string path, string? atmId = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'><title>EJLive Report</title>");
            sb.AppendLine("<style>body{font-family:Segoe UI;margin:20px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ddd;padding:8px}th{background:#2196F3;color:white}.card{display:inline-block;padding:15px;margin:10px;background:#f5f5f5;border-left:4px solid #2196F3;min-width:150px}</style></head><body>");
            sb.AppendLine("<h1>EJLive Journal Report</h1><p>" + DateTime.Now + "</p><div>");
            lock (_lock)
            {
                var stats = atmId != null && _atmStats.ContainsKey(atmId) ? new[] { _atmStats[atmId] } : _atmStats.Values.ToArray();
                sb.AppendLine("<div class='card'><b>Files</b><br>" + stats.Sum(s => s.TotalFiles) + "</div>");
                sb.AppendLine("<div class='card'><b>Size</b><br>" + (stats.Sum(s => s.TotalBytes) / 1024) + " KB</div>");
                sb.AppendLine("<div class='card'><b>Withdrawals</b><br>" + stats.Sum(s => s.TotalWithdrawals) + "</div>");
                sb.AppendLine("<div class='card'><b>Errors</b><br>" + stats.Sum(s => s.TotalErrors) + "</div>");
            }
            sb.AppendLine("</div><h2>Records</h2><table><tr><th>ATM</th><th>File</th><th>Size</th><th>Date</th></tr>");
            lock (_lock)
            {
                foreach (var r in _recentRecords.Where(r => atmId == null || r.ATM_ID == atmId).TakeLast(50))
                    sb.AppendLine("<tr><td>" + r.ATM_ID + "</td><td>" + r.FileName + "</td><td>" + r.FileSize + "</td><td>" + r.ReceivedAt + "</td></tr>");
            }
            sb.AppendLine("</table></body></html>");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            OnLog?.Invoke("[HTML] " + path);
            return path;
        }
        public List<JournalRecord> GetRecentRecords(int count = 50, string? atmId = null)
        {
            lock (_lock) { return _recentRecords.Where(r => atmId == null || r.ATM_ID == atmId).TakeLast(count).ToList(); }
        }
        public void Dispose()
        {
            _autoArchiveTimer?.Dispose();
        }
        private static string NormalizeAtmIdentifier(string? atmId)
        {
            var candidate = (atmId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(candidate))
                return "ATM_UNKNOWN";
            return candidate;
        }
        private static string NormalizeFileName(string? fileName, string prefix)
        {
            var baseName = Path.GetFileName((fileName ?? string.Empty).Trim());
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = $"{prefix}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.dat";
            return BuildSafePathSegment(baseName, $"{prefix}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}.dat");
        }
        private static string BuildSafePathSegment(string? value, string fallback)
        {
            var candidate = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(candidate))
                return fallback;
            var invalidChars = Path.GetInvalidFileNameChars();
            var normalized = new StringBuilder(candidate.Length);
            foreach (var character in candidate)
            {
                if (character == Path.DirectorySeparatorChar || character == Path.AltDirectorySeparatorChar || invalidChars.Contains(character))
                    normalized.Append('_');
                else
                    normalized.Append(character);
            }
            var sanitized = normalized.ToString().Trim();
            if (string.IsNullOrWhiteSpace(sanitized) || string.Equals(sanitized, ".", StringComparison.Ordinal) || string.Equals(sanitized, "..", StringComparison.Ordinal))
                return fallback;
            return sanitized;
        }
        private static string EscapeCsv(string? value)
        {
            var text = value ?? string.Empty;
            if (text.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            return text;
        }
        public string ExportCSVReport(string path, string? atmId = null, DateTime? from = null, DateTime? to = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ATM_ID,FileName,Size,ReceivedAt,Checksum");
            lock (_lock)
            {
                foreach (var r in _recentRecords.Where(r => (atmId == null || r.ATM_ID == atmId) && (from == null || r.ReceivedAt >= from) && (to == null || r.ReceivedAt <= to)))
                    sb.AppendLine(r.ATM_ID + "," + r.FileName + "," + r.FileSize + "," + r.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss") + "," + r.Checksum);
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            OnLog?.Invoke("[CSV] " + path);
            return path;
        }
        public event Action<string>? OnLog;
        public event Action<string> OnLog;
        public event Action<string, string> OnArchiveCompleted;
        public event Action<string, string>? OnArchiveCompleted;
    }

    public partial class ATMJournalStats
    {
        public string ATM_ID { get; set; }
        public int TotalFiles { get; set; }
        public long TotalBytes { get; set; }
        public int TotalLinesProcessed { get; set; }
        public int TotalWithdrawals { get; set; }
        public int TotalDeposits { get; set; }
        public int TotalErrors { get; set; }
        public int TotalCardRetained { get; set; }
        public int PaperWarnings { get; set; }
        public int CashWarnings { get; set; }
        public DateTime LastReceived { get; set; }
        public string LastFileName { get; set; }
    }

    public partial class JournalRecord
    {
        public string ATM_ID { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string Checksum { get; set; }
        public string StoragePath { get; set; }
    }

}
