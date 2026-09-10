using System;
using System.Collections.Generic;
using EJLive.Core.Engine;
using System;
using System.Collections.Generic;
using EJLive.Core.Engine;

namespace EJLive.Core.Models
{
    /// <summary>
    /// نماذج العمليات والإحصائيات المفصلة
    /// تُستخدم من: TransactionAnalysisEngine + ReportExportEngine + ServerMainForm
    /// </summary>

    // ==========================================
    // إحصائيات الصراف الشاملة
    // ==========================================

    public class ATMStatsSummary
    {
        public string   ATMId                  { get; set; }
        public string   ATMName                { get; set; }
        public string   ATMType                { get; set; }
        public DateTime PeriodFrom             { get; set; }
        public DateTime PeriodTo               { get; set; }

        // عمليات
        public int    TotalTransactions        { get; set; }
        public int    ApprovedTransactions     { get; set; }
        public int    DeclinedTransactions     { get; set; }
        public int    CardsCaptured            { get; set; }
        public int    BalanceInquiries         { get; set; }
        public long   TotalCashDispensed       { get; set; }
        public double SuccessRate              => TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100 : 0;

        // أخطاء
        public int    PowerResets              { get; set; }
        public int    SupervisorEntries        { get; set; }
        public int    CashErrors               { get; set; }
        public int    MediaErrors              { get; set; }
        public List<string> ErrorCodesFound    { get; set; } = new List<string>();

        // أعلى أوقات النشاط
        public int    PeakHour                 { get; set; }
        public int    PeakDayOfWeek            { get; set; }

        // مزامنة
        public long   TotalJournalBytesReceived { get; set; }
        public int    TotalJournalFiles        { get; set; }
        public double SyncSuccessRate          { get; set; } = 100.0;
        public double UptimePercent            { get; set; } = 100.0;

        public string CashDisplay =>
            TotalCashDispensed >= 1_000_000 ? $"{TotalCashDispensed / 1_000_000.0:F2} M" :
            TotalCashDispensed >= 1_000     ? $"{TotalCashDispensed / 1_000.0:F1} K" :
            TotalCashDispensed.ToString("N0");
    }

    // ==========================================
    // حدث عملية حية
    // ==========================================

    public class LiveTransactionEvent
    {
        public string      ATMId        { get; set; }
        public TxType      TxType       { get; set; }
        public TxResult    Result       { get; set; }
        public long        Amount       { get; set; }
        public string      ErrorCode    { get; set; }
        public bool        CardCaptured { get; set; }
        public string      RawLine      { get; set; }
        public DateTime    OccurredAt   { get; set; } = DateTime.UtcNow;

        public string Icon => Result switch
        {
            TxResult.Approved => "✅",
            TxResult.Declined => "❌",
            TxResult.Error    => "🚨",
            TxResult.Warning  => "⚠️",
            _                 => "ℹ️"
        };

        public string DisplayLabel => $"{Icon} {TxType} {(Amount > 0 ? Amount.ToString("N0") : "")} {ErrorCode ?? ""}".Trim();
    }

    // ==========================================
    // تقرير المزامنة اليومي
    // ==========================================

    public class DailySyncReport
    {
        public DateTime ReportDate         { get; set; }
        public string   ATMId              { get; set; }
        public int      FilesReceived      { get; set; }
        public int      FilesFailed        { get; set; }
        public long     BytesReceived      { get; set; }
        public double   AverageSpeedKBs    { get; set; }
        public TimeSpan TotalSyncTime      { get; set; }
        public int      ReconnectCount     { get; set; }
        public double   UptimePercent      { get; set; }
        public string   Notes              { get; set; }
    }

    // ==========================================
    // تقرير التنبيهات
    // ==========================================

    public class AlertReport
    {
        public DateTime From               { get; set; }
        public DateTime To                 { get; set; }
        public int      TotalAlerts        { get; set; }
        public int      CriticalAlerts     { get; set; }
        public int      WarningAlerts      { get; set; }
        public int      InfoAlerts         { get; set; }
        public List<AlertPayload> Alerts   { get; set; } = new List<AlertPayload>();

        public string Summary =>
            $"🚨{CriticalAlerts} ⚠️{WarningAlerts} ℹ️{InfoAlerts} — إجمالي: {TotalAlerts}";
    }

    // ==========================================
    // Audit Log Entry
    // ==========================================

    public class AuditLogEntry
    {
        public string   LogId        { get; set; } = Guid.NewGuid().ToString("N");
        public string   Action       { get; set; }
        public string   PerformedBy  { get; set; }
        public string   ATMId        { get; set; }
        public string   Details      { get; set; }
        public string   IPAddress    { get; set; }
        public DateTime PerformedAt  { get; set; } = DateTime.UtcNow;
        public bool     IsSuccessful { get; set; } = true;
    }

    // ==========================================
    // مستخدم النظام
    // ==========================================

    public class SystemUser
    {
        public string   UserId          { get; set; } = Guid.NewGuid().ToString("N");
        public string   Username        { get; set; }
        public string   PasswordHash    { get; set; }
        public string   Role            { get; set; } = AppConstants.ROLE_OBSERVER;
        public string   FullName        { get; set; }
        public string   Email           { get; set; }
        public bool     IsActive        { get; set; } = true;
        public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt    { get; set; }

        public bool CanManageUsers      => Role == AppConstants.ROLE_ADMIN;
        public bool CanSendCommands     => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
        public bool CanExportReports    => Role != AppConstants.ROLE_OBSERVER;
        public bool CanViewArchive      => true;
        public bool CanViewGhostView    => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
    }

    // ==========================================
    // إعدادات العميل
    // ==========================================

    public class AppConfig
    {
        public string ATM_ID         { get; set; } = "ATM001";
        public string ATM_Name       { get; set; } = "صراف رقم 1";
        public string ATM_Type       { get; set; } = AppConstants.ATM_TYPE_NCR;
        public string ServerIP       { get; set; } = "192.168.1.100";
        public int    ServerPort     { get; set; } = 5656;
        public string NetworkType    { get; set; } = "LAN";
        public string SourcePath     { get; set; } = AppConstants.NCR_JournalPath;
        public string BackupPath     { get; set; } = AppConstants.NCR_BackupPath;
        public bool   AutoConnect    { get; set; } = true;
        public bool   Encrypt        { get; set; } = true;
        public bool   Compress       { get; set; } = true;
        public bool   AutoBackup     { get; set; } = true;
        public string HashedPassword { get; set; }
        public int    HeartbeatSec   { get; set; } = 30;
        public int    ChunkSizeKB    { get; set; } = 64;

        private static readonly string _configPath = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
            "EJLive", "Client", "client.cfg");

        private static readonly string _legacyConfigPath = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "EJLive", "client.cfg");

        public static string ConfigPath => _configPath;

        public static AppConfig Load()
        {
            try
            {
                var cfg = new AppConfig();
                var loadPath = System.IO.File.Exists(_configPath)
                    ? _configPath
                    : (System.IO.File.Exists(_legacyConfigPath) ? _legacyConfigPath : null);

                if (string.IsNullOrWhiteSpace(loadPath))
                {
                    cfg.ApplyDefaults();
                    return cfg;
                }
                var lines = System.IO.File.ReadAllLines(loadPath, System.Text.Encoding.UTF8);
                foreach (var line in lines)
                {
                    if (!line.Contains("=")) continue;
                    var idx = line.IndexOf('=');
                    var key = line.Substring(0, idx).Trim();
                    var val = line.Substring(idx + 1).Trim();
                    switch (key)
                    {
                        case "ATM_ID":       cfg.ATM_ID       = val; break;
                        case "ATM_Name":     cfg.ATM_Name     = val; break;
                        case "ATM_Type":     cfg.ATM_Type     = val; break;
                        case "ServerIP":     cfg.ServerIP     = val; break;
                        case "ServerPort":   if (int.TryParse(val, out var p)) cfg.ServerPort = p; break;
                        case "NetworkType":  cfg.NetworkType  = val; break;
                        case "SourcePath":   cfg.SourcePath   = val; break;
                        case "BackupPath":   cfg.BackupPath   = val; break;
                        case "AutoConnect":  cfg.AutoConnect  = val == "true"; break;
                        case "Encrypt":      cfg.Encrypt      = val == "true"; break;
                        case "Compress":     cfg.Compress     = val == "true"; break;
                        case "AutoBackup":   cfg.AutoBackup   = val == "true"; break;
                        case "HashedPassword": cfg.HashedPassword = val; break;
                    }
                }
                cfg.ApplyDefaults();
                return cfg;
            }
            catch
            {
                var cfg = new AppConfig();
                cfg.ApplyDefaults();
                return cfg;
            }
        }

        public void Save()
        {
            try
            {
                ApplyDefaults();
                EnsureRuntimeFolders();
                var dir = System.IO.Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                var lines = new[]
                {
                    $"ATM_ID={ATM_ID}",     $"ATM_Name={ATM_Name}", $"ATM_Type={AppConstants.NormalizeATMType(ATM_Type)}",
                    $"ServerIP={ServerIP}", $"ServerPort={ServerPort}", $"NetworkType={NetworkType}",
                    $"SourcePath={SourcePath}", $"BackupPath={BackupPath}",
                    $"AutoConnect={AutoConnect.ToString().ToLower()}",
                    $"Encrypt={Encrypt.ToString().ToLower()}",
                    $"Compress={Compress.ToString().ToLower()}",
                    $"AutoBackup={AutoBackup.ToString().ToLower()}",
                    $"HashedPassword={HashedPassword ?? ""}",
                };
                System.IO.File.WriteAllLines(_configPath, lines, System.Text.Encoding.UTF8);
            }
            catch { }
        }

        public void ApplyDefaults()
        {
            ATM_Type = AppConstants.NormalizeATMType(ATM_Type);
            if (string.IsNullOrWhiteSpace(SourcePath) || AppConstants.IsKnownDefaultSourcePath(SourcePath))
                SourcePath = AppConstants.GetDefaultSourcePath(ATM_Type);
            if (string.IsNullOrWhiteSpace(BackupPath) || AppConstants.IsKnownDefaultBackupPath(BackupPath))
                BackupPath = AppConstants.GetDefaultBackupPath(ATM_Type);
            if (ServerPort <= 0)
                ServerPort = AppConstants.DefaultPort;
            if (string.IsNullOrWhiteSpace(NetworkType))
                NetworkType = AppConstants.NET_LAN;
        }

        public void EnsureRuntimeFolders()
        {
            foreach (var folder in AppConstants.GetClientRuntimeFolders(ATM_Type, SourcePath, BackupPath))
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(folder))
                        System.IO.Directory.CreateDirectory(folder);
                }
                catch
                {
                    // Source journal folders can be protected or created by the ATM vendor software.
                
    /// <summary>
    /// نماذج العمليات والإحصائيات المفصلة
    /// تُستخدم من: TransactionAnalysisEngine + ReportExportEngine + ServerMainForm
    /// </summary>
    public class ATMStatsSummary
    {
        public string ATMId { get; set; }
        public string ATMName { get; set; }
        public string ATMType { get; set; }
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int TotalTransactions { get; set; }
        public int ApprovedTransactions { get; set; }
        public int DeclinedTransactions { get; set; }
        public int CardsCaptured { get; set; }
        public int BalanceInquiries { get; set; }
        public long TotalCashDispensed { get; set; }
        public double SuccessRate => TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100 : 0;
        public int PowerResets { get; set; }
        public int SupervisorEntries { get; set; }
        public int CashErrors { get; set; }
        public int MediaErrors { get; set; }
        public List<string> ErrorCodesFound { get; set; } = new List<string>();
        public int PeakHour { get; set; }
        public int PeakDayOfWeek { get; set; }
        public long TotalJournalBytesReceived { get; set; }
        public int TotalJournalFiles { get; set; }
        public double SyncSuccessRate { get; set; } = 100.0;
        public double UptimePercent { get; set; } = 100.0;
        public string CashDisplay =>
            TotalCashDispensed >= 1000000 ? $"{TotalCashDispensed / 1000000.0:F2} M" :
            TotalCashDispensed >= 1000 ? $"{TotalCashDispensed / 1000.0:F1} K" :
            TotalCashDispensed.ToString("N0");
    }

    public class LiveTransactionEvent
    {
        public string ATMId { get; set; }
        public TxType TxType { get; set; }
        public TxResult Result { get; set; }
        public long Amount { get; set; }
        public string ErrorCode { get; set; }
        public bool CardCaptured { get; set; }
        public string RawLine { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string Icon => Result switch
        {
            TxResult.Approved => "✅",
            TxResult.Declined => "❌",
            TxResult.Error => "🚨",
            TxResult.Warning => "⚠️",
            _ => "ℹ️"
        };
        public string DisplayLabel => $"{Icon} {TxType} {(Amount > 0 ? Amount.ToString("N0") : "")} {ErrorCode ?? ""}".Trim();
    }

    public class DailySyncReport
    {
        public DateTime ReportDate { get; set; }
        public string ATMId { get; set; }
        public int FilesReceived { get; set; }
        public int FilesFailed { get; set; }
        public long BytesReceived { get; set; }
        public double AverageSpeedKBs { get; set; }
        public TimeSpan TotalSyncTime { get; set; }
        public int ReconnectCount { get; set; }
        public double UptimePercent { get; set; }
        public string Notes { get; set; }
    }

    public class AlertReport
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }
        public List<AlertPayload> Alerts { get; set; } = new List<AlertPayload>();
        public string Summary => $"🚨{CriticalAlerts} ⚠️{WarningAlerts} ℹ️{InfoAlerts} — إجمالي: {TotalAlerts}";
    }

    public class AuditLogEntry
    {
        public string LogId { get; set; } = Guid.NewGuid().ToString("N");
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public string ATMId { get; set; }
        public string Details { get; set; }
        public string IPAddress { get; set; }
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; set; } = true;
    }

    public class SystemUser
    {
        public string UserId { get; set; } = Guid.NewGuid().ToString("N");
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = AppConstants.ROLE_OBSERVER;
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool CanManageUsers => Role == AppConstants.ROLE_ADMIN;
        public bool CanSendCommands => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
        public bool CanExportReports => Role != AppConstants.ROLE_OBSERVER;
        public bool CanViewArchive => true;
        public bool CanViewGhostView => Role == AppConstants.ROLE_ADMIN || Role == AppConstants.ROLE_SUPPORT;
    }

    public class AppConfig
    {
        public string ATM_ID { get; set; } = "ATM001";
        public string ATM_Name { get; set; } = "صراف رقم 1";
        public string ATM_Type { get; set; } = AppConstants.ATM_TYPE_NCR;
        public string ServerIP { get; set; } = "192.168.1.100";
        public int ServerPort { get; set; } = 5656;
        public string NetworkType { get; set; } = "LAN";
        public string SourcePath { get; set; } = AppConstants.NCR_JournalPath;
        public string BackupPath { get; set; } = AppConstants.NCR_BackupPath;
        public bool AutoConnect { get; set; } = true;
        public bool Encrypt { get; set; } = true;
        public bool Compress { get; set; } = true;
        public bool AutoBackup { get; set; } = true;
        public string HashedPassword { get; set; }
        public int HeartbeatSec { get; set; } = 30;
        public int ChunkSizeKB { get; set; } = 64;

        private static readonly string _configPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "EJLive", "Client", "client.cfg");
        private static readonly string _legacyConfigPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "EJLive", "client.cfg");
        public static string ConfigPath => _configPath;

        public static AppConfig Load()
        {
            try
            {
                var cfg = new AppConfig();
                var loadPath = System.IO.File.Exists(_configPath) ? _configPath : (System.IO.File.Exists(_legacyConfigPath) ? _legacyConfigPath : null);
                if (string.IsNullOrWhiteSpace(loadPath))
                {
                    cfg.ApplyDefaults();
                    return cfg;
                }
                var lines = System.IO.File.ReadAllLines(loadPath, System.Text.Encoding.UTF8);
                foreach (var line in lines)
                {
                    if (!line.Contains("=")) continue;
                    var idx = line.IndexOf('=');
                    var key = line.Substring(0, idx).Trim();
                    var val = line.Substring(idx + 1).Trim();
                    switch (key)
                    {
                        case "ATM_ID": cfg.ATM_ID = val; break;
                        case "ATM_Name": cfg.ATM_Name = val; break;
                        case "ATM_Type": cfg.ATM_Type = val; break;
                        case "ServerIP": cfg.ServerIP = val; break;
                        case "ServerPort": if (int.TryParse(val, out var p)) cfg.ServerPort = p; break;
                        case "NetworkType": cfg.NetworkType = val; break;
                        case "SourcePath": cfg.SourcePath = val; break;
                        case "BackupPath": cfg.BackupPath = val; break;
                        case "AutoConnect": cfg.AutoConnect = val == "true"; break;
                        case "Encrypt": cfg.Encrypt = val == "true"; break;
                        case "Compress": cfg.Compress = val == "true"; break;
                        case "AutoBackup": cfg.AutoBackup = val == "true"; break;
                        case "HashedPassword": cfg.HashedPassword = val; break;
                    }
                }
                cfg.ApplyDefaults();
                return cfg;
            }
            catch
            {
                var cfg = new AppConfig();
                cfg.ApplyDefaults();
                return cfg;
            }
        }

        public void Save()
        {
            try
            {
                ApplyDefaults();
                EnsureRuntimeFolders();
                var dir = System.IO.Path.GetDirectoryName(_configPath);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);
                var lines = new[]
                {
                    $"ATM_ID={ATM_ID}", $"ATM_Name={ATM_Name}", $"ATM_Type={AppConstants.NormalizeATMType(ATM_Type)}",
                    $"ServerIP={ServerIP}", $"ServerPort={ServerPort}", $"NetworkType={NetworkType}",
                    $"SourcePath={SourcePath}", $"BackupPath={BackupPath}",
                    $"AutoConnect={AutoConnect.ToString().ToLower()}",
                    $"Encrypt={Encrypt.ToString().ToLower()}",
                    $"Compress={Compress.ToString().ToLower()}",
                    $"AutoBackup={AutoBackup.ToString().ToLower()}",
                    $"HashedPassword={HashedPassword ?? ""}",
                };
                System.IO.File.WriteAllLines(_configPath, lines, System.Text.Encoding.UTF8);
            }
            catch { }
        }

        public void ApplyDefaults()
        {
            ATM_Type = AppConstants.NormalizeATMType(ATM_Type);
            if (string.IsNullOrWhiteSpace(SourcePath) || AppConstants.IsKnownDefaultSourcePath(SourcePath))
                SourcePath = AppConstants.GetDefaultSourcePath(ATM_Type);
            if (string.IsNullOrWhiteSpace(BackupPath) || AppConstants.IsKnownDefaultBackupPath(BackupPath))
                BackupPath = AppConstants.GetDefaultBackupPath(ATM_Type);
            if (ServerPort <= 0)
                ServerPort = AppConstants.DefaultPort;
            if (string.IsNullOrWhiteSpace(NetworkType))
                NetworkType = AppConstants.NET_LAN;
        }

        public void EnsureRuntimeFolders()
        {
            foreach (var folder in AppConstants.GetClientRuntimeFolders(ATM_Type, SourcePath, BackupPath))
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(folder))
                        System.IO.Directory.CreateDirectory(folder);
                }
                catch
                {
                }
            }
        }
    }
}

