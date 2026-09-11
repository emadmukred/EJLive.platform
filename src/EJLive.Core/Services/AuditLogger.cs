using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Core.Services
{
    /// <summary>
    /// سجل التدقيق غير القابل للتعديل
    /// يطبق: S-03 (Immutable Audit Log) — كل أمر حساس مُسجَّل بتوقيت UTC
    /// </summary>
    public sealed class AuditLogger
    {
        private static readonly object _fileLock = new object();
        private static string _logDirectory = AppConstants.DefaultLogPath;
        private static string _currentLogFile = Path.Combine(AppConstants.DefaultLogPath, "audit_init.log");
        private static readonly List<AuditEntry> _memoryBuffer = new List<AuditEntry>();
        private const int MAX_BUFFER = 500;

        public void Record(AuditLogEntry entry)
        {
            if (entry == null)
                return;

            DatabaseManager.Instance.ExecuteNonQuery(
                "INSERT OR REPLACE INTO audit_log(entry_id,user_name,action,target,created_at_utc,details) VALUES(@id,@user,@action,@target,@created,@details)",
                new SQLiteParameter("@id", entry.EntryId),
                new SQLiteParameter("@user", entry.UserName),
                new SQLiteParameter("@action", entry.Action),
                new SQLiteParameter("@target", entry.Target),
                new SQLiteParameter("@created", entry.CreatedAtUtc.ToString("O")),
                new SQLiteParameter("@details", entry.Details));
        }

        public static void Initialize(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(logDirectory);
            RotateLogFile();
        }

        private static void RotateLogFile()
        {
            _currentLogFile = Path.Combine(
                _logDirectory,
                $"audit_{DateTime.UtcNow:yyyyMMdd}.log");
        }

        // ==========================================
        // الدوال الرئيسية
        // ==========================================

        public static void Log(AuditAction action, string userId, string? targetAtmId,
            string details, bool success = true, string? ipAddress = null)
        {
            var entry = new AuditEntry
            {
                EntryId = Guid.NewGuid().ToString("N").Substring(0, 16),
                TimestampUtc = DateTime.UtcNow,
                Action = action,
                UserId = userId ?? "SYSTEM",
                TargetAtmId = targetAtmId ?? "ALL",
                Details = details,
                Success = success,
                IpAddress = ipAddress ?? "127.0.0.1"
            };

            WriteEntry(entry);
        }

        public static void LogCommand(string userId, string atmId, string command, string parameters, bool success)
            => Log(AuditAction.RemoteCommand, userId, atmId, $"CMD:{command} | Params:{parameters}", success);

        public static void LogLogin(string userId, string? ipAddress, bool success)
            => Log(AuditAction.Login, userId, null, $"Login attempt from {ipAddress}", success, ipAddress);

        public static void LogPasswordChange(string userId, string atmId, bool success)
            => Log(AuditAction.PasswordChange, userId, atmId, "ATM password changed", success);

        public static void LogRestart(string userId, string atmId, bool success)
            => Log(AuditAction.Restart, userId, atmId, "ATM restart command issued", success);

        public static void LogRemoteSessionAccess(string userId, string atmId, bool started)
            => Log(AuditAction.RemoteSessionAccess, userId, atmId, started ? "Remote session STARTED" : "Remote session STOPPED", true);

        public static void LogArchiveAccess(string userId, string atmId, string query)
            => Log(AuditAction.ArchiveAccess, userId, atmId, $"Archive query: {query}", true);

        public static void LogConfigChange(string userId, string setting, string oldVal, string newVal)
            => Log(AuditAction.ConfigChange, userId, null, $"Setting '{setting}': '{oldVal}' → '{newVal}'", true);

        // ==========================================
        // الكتابة والحفظ
        // ==========================================

        private static void WriteEntry(AuditEntry entry)
        {
            string line = FormatEntry(entry);

            lock (_fileLock)
            {
                // تدوير الملف يوميًا
                if (!Path.GetFileName(_currentLogFile).Contains(DateTime.UtcNow.ToString("yyyyMMdd")))
                    RotateLogFile();

                // كتابة فورية للملف
                File.AppendAllText(_currentLogFile, line + Environment.NewLine, Encoding.UTF8);

                // الحفاظ على Buffer في الذاكرة
                _memoryBuffer.Add(entry);
                if (_memoryBuffer.Count > MAX_BUFFER)
                    _memoryBuffer.RemoveAt(0);
            }
        }

        private static string FormatEntry(AuditEntry e) =>
            $"[{e.TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}Z] [{e.EntryId}] " +
            $"[{(e.Success ? "OK" : "FAIL")}] [{e.Action}] " +
            $"User:{e.UserId} | ATM:{e.TargetAtmId} | IP:{e.IpAddress} | {e.Details}";

        // ==========================================
        // القراءة والتصفية
        // ==========================================

        public static List<AuditEntry> GetRecentEntries(int count = 100)
        {
            lock (_fileLock)
            {
                int start = Math.Max(0, _memoryBuffer.Count - count);
                return _memoryBuffer.GetRange(start, Math.Min(count, _memoryBuffer.Count - start));
            }
        }

        public static List<AuditEntry> GetEntriesByAtm(string atmId, int count = 50)
        {
            lock (_fileLock)
            {
                var results = new List<AuditEntry>();
                for (int i = _memoryBuffer.Count - 1; i >= 0 && results.Count < count; i--)
                {
                    if (_memoryBuffer[i].TargetAtmId == atmId)
                        results.Add(_memoryBuffer[i]);
                }
                return results;
            }
        }

        public static string ExportToCsv(string outputPath, DateTime? from = null, DateTime? to = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("EntryId,TimestampUtc,Action,UserId,TargetAtmId,Success,IpAddress,Details");

            lock (_fileLock)
            {
                foreach (var e in _memoryBuffer)
                {
                    if (from.HasValue && e.TimestampUtc < from.Value) continue;
                    if (to.HasValue && e.TimestampUtc > to.Value) continue;
                    sb.AppendLine($"\"{e.EntryId}\",\"{e.TimestampUtc:O}\",\"{e.Action}\"," +
                        $"\"{e.UserId}\",\"{e.TargetAtmId}\",\"{e.Success}\"," +
                        $"\"{e.IpAddress}\",\"{e.Details?.Replace("\"", "\"\"")}\"");
                }
            }

            Directory.CreateDirectory(outputPath);
            string filePath = Path.Combine(outputPath, $"AuditLog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            return filePath;
        }
    }

    public class AuditEntry
    {
        public string EntryId { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; }
        public AuditAction Action { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TargetAtmId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }

