using System;
using System.IO;

namespace EJLive.Shared
{
    /// <summary>
    /// ثوابت التطبيق المركزية — تُستخدم عبر جميع مكونات EJLive
    /// </summary>
    public static class AppConstants
    {
        // ────────────────────────────────────
        // إصدارات ومعرفات
        // ────────────────────────────────────
        public const string AppVersion = "4.0.0";
        public const string ProtocolVersion = "EJ-4.0";
        public const int DefaultPort = 5656;
        public const int HeartbeatIntervalSec = 30;
        public const int HeartbeatTimeoutSec = 90;

        // ────────────────────────────────────
        // أنواع أجهزة الصراف الآلي
        // ────────────────────────────────────
        public const string ATM_TYPE_NCR = "NCR";
        public const string ATM_TYPE_DIEBOLD = "Diebold";
        public const string ATM_TYPE_WINCOR = "Wincor";
        public const string ATM_TYPE_HYOSUNG = "Hyosung";

        // ────────────────────────────────────
        // مسارات افتراضية
        // ────────────────────────────────────
        public static string DefaultLogPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive", "Logs");

        public static string DefaultDatabasePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive", "ejlive.db");

        public static string DefaultClientOutboxPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive", "Client", "Outbox");

        public static string DefaultClientInboxPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive", "Client", "Inbox");

        public static string DefaultReportsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive", "Reports");

        // ────────────────────────────────────
        // مسارات NCR الافتراضية
        // ────────────────────────────────────
        public const string NCR_JournalPath = @"C:\EJ\Journals";
        public const string NCR_BackupPath = @"C:\EJ\Backup";

        // ────────────────────────────────────
        // أوامر التحكم البعيد
        // ────────────────────────────────────
        public const string CMD_PING = "PING";
        public const string CMD_SYNC_TIME = "SYNC_TIME";
        public const string CMD_SCREENSHOT = "SCREENSHOT";
        public const string CMD_GHOST_START = "GHOST_START";
        public const string CMD_GHOST_STOP = "GHOST_STOP";
        public const string CMD_FORCE_SYNC = "FORCE_SYNC";
public const string CMD_GET_STATS = "GET_STATS";
public const string CMD_GET_SYSINFO = "GET_SYSINFO";
        public const string CMD_RESTART = "RESTART";
        public const string CMD_SHUTDOWN = "SHUTDOWN";
        public const string CMD_CHANGE_PASSWORD = "CHANGE_PASSWORD";
        public const string CMD_WINDOWS_REMOTE_START = "WINDOWS_REMOTE_START";
        public const string CMD_WINDOWS_REMOTE_STOP = "WINDOWS_REMOTE_STOP";
        public const string CMD_WINDOWS_REMOTE_CHECK = "CMD_WINDOWS_REMOTE_CHECK";

        // ────────────────────────────────────
        // إعدادات الشبكة
        // ────────────────────────────────────
        public static readonly string[] CommandsRequireConfirmation = {
            CMD_RESTART, CMD_SHUTDOWN, CMD_CHANGE_PASSWORD
        };

        // ────────────────────────────────────
        // دوال مساعدة
        // ────────────────────────────────────
        public static string[] GetSupportedATMTypes()
        {
            return new[] { ATM_TYPE_NCR, ATM_TYPE_DIEBOLD, ATM_TYPE_WINCOR, ATM_TYPE_HYOSUNG };
        }

        public static string NormalizeATMType(string? type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return ATM_TYPE_NCR;

            var normalized = type.Trim().ToUpperInvariant();
            return normalized switch
            {
                "NCR" => ATM_TYPE_NCR,
                "DIEBOLD" => ATM_TYPE_DIEBOLD,
                "WINCOR" => ATM_TYPE_WINCOR,
                "HYOSUNG" => ATM_TYPE_HYOSUNG,
                _ => ATM_TYPE_NCR
            };
        }

        public static string GetDefaultSourcePath(string atmType)
        {
            return atmType switch
            {
                ATM_TYPE_NCR => NCR_JournalPath,
                ATM_TYPE_DIEBOLD => @"D:\Diebold\Logs",
                ATM_TYPE_WINCOR => @"E:\Wincor\Journals",
                _ => @"C:\ATM\Journals"
            };
        }

        public static string GetDefaultBackupPath(string atmType)
        {
            return atmType switch
            {
                ATM_TYPE_NCR => NCR_BackupPath,
                ATM_TYPE_DIEBOLD => @"D:\Diebold\Backup",
                ATM_TYPE_WINCOR => @"E:\Wincor\Backup",
                _ => @"C:\ATM\Backup"
            };
        }

        public static string[] GetClientRuntimeFolders(string atmType, string sourcePath, string backupPath)
        {
            return new[]
            {
                DefaultClientOutboxPath,
                DefaultClientInboxPath,
                DefaultLogPath,
                DefaultReportsPath,
                sourcePath,
                backupPath,
                Path.Combine(backupPath, "Screenshots"),
                Path.Combine(backupPath, "MonthlyArchive")
            };
        }
    }

    /// <summary>
    /// إعدادات الشبكة
    /// </summary>
    public static class NetworkConfig
    {
        public const int PING_TIMEOUT_MS = 2000;
        public const int CONNECT_TIMEOUT_MS = 5000;
        public const int SEND_TIMEOUT_MS = 10000;
        public const int RECEIVE_TIMEOUT_MS = 10000;
        public const int MAX_RETRY_ATTEMPTS = 3;
        public const int RETRY_BASE_DELAY_MS = 200;
        public const int RETRY_MAX_DELAY_MS = 3000;
        public const int BUFFER_SIZE = 65536;
    }
}