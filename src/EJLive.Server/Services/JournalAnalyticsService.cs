using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using EJLive.Core;
using EJLive.Core.Models;

namespace EJLive.Server.Services
{
    public sealed class JournalAnalyticsService : IDisposable
    {
        private string _storagePath;
        private string _archivePath;
        private Dictionary<string, ATMJournalStats> _atmStats;
        private List<JournalRecord> _recentRecords;
        private readonly object _lock = new object();
        private System.Threading.Timer _autoArchiveTimer;

        public event Action<string>? OnLog;
        public event Action<string, string>? OnArchiveCompleted;

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

        public ATMJournalStats GetATMStats(string atmId)
        {
            lock (_lock) { return _atmStats.ContainsKey(atmId) ? _atmStats[atmId] : new ATMJournalStats { ATM_ID = atmId }; }
        }

        public Dictionary<string, ATMJournalStats> GetAllStats()
        {
            lock (_lock) { return new Dictionary<string, ATMJournalStats>(_atmStats); }
        }

        public List<JournalRecord> GetRecentRecords(int count = 50, string? atmId = null)
        {
            lock (_lock) { return _recentRecords.Where(r => atmId == null || r.ATM_ID == atmId).TakeLast(count).ToList(); }
        }

        public long GetStorageSize()
        {
            if (!Directory.Exists(_storagePath)) return 0;
            return new DirectoryInfo(_storagePath).EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length);
        }

        public void Dispose()
        {
            _autoArchiveTimer?.Dispose();
        }
    }

    public class ATMJournalStats
    {
        public string ATM_ID { get; set; } = string.Empty;
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
        public string LastFileName { get; set; } = string.Empty;
    }

    public class JournalRecord
    {
        public string ATM_ID { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
    }
}
