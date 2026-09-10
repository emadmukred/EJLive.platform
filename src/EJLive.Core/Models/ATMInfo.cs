using System;
using System.Drawing;
using EJLive.Core;
using System;

namespace EJLive.Core.Models
{
    public class ATMInfo
    {

    // ==========================================
    // Enumerations
    // ==========================================

    public enum ConnectionStatus  { Disconnected=0, Connecting=1, Connected=2, WaitingReply=3, Syncing=4 }
    public enum ATMStatus
    {
        Unknown = 0,
        Online = 1,
        Idle = 2,
        Supervisor = 3,
        Warning = 4,
        Offline = 5,
        Critical = 6,
        InService = 10,
        ConnectedOnly = 11,
        WaitingResponse = 12,
        OutOfService = 13,
        CriticalFault = 14,
        Fault = 15,
        Maintenance = 16
    }
    public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
    public enum SyncStatus { Pending, InProgress, Syncing, Resyncing, Completed, Failed }
    public enum AlertSeverity     { Info=0, Warning=1, Critical=2, Emergency=3 }
    public enum JournalSyncState  { Pending=0, Syncing=1, ReSyncing=2, Completed=3, Failed=4, Archived=5 }
    public enum ATMCardState      { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }

    // ==========================================
    // ATMInfo — نموذج الصراف الشامل
    // ==========================================

    public class ATMInfo
    {
        // هوية
        public string ATM_ID        { get; set; }
        public string ATM_Name      { get; set; }
        public string ATM_Type      { get; set; }    // NCR / GRG / WN / DIEBOLD / HYOSUNG
        public string BranchName    { get; set; }
        public string Region        { get; set; }
        public string ATMId { get => ATM_ID; set => ATM_ID = value; }
        public string ATMName { get => ATM_Name; set => ATM_Name = value; }
        public string IPAddress { get => ServerIP; set => ServerIP = value; }
        public ATMType ATMType
        {
            get
            {
                switch (AppConstants.NormalizeATMType(ATM_Type))
                {
                    case AppConstants.ATM_TYPE_NCR: return ATMType.NCR;
                    case AppConstants.ATM_TYPE_GRG: return ATMType.GRG;
                    case AppConstants.ATM_TYPE_WN:  return ATMType.WN;
                    case AppConstants.ATM_TYPE_DN:  return ATMType.DieboldNixdorf;
                    case AppConstants.ATM_TYPE_HY:  return ATMType.Hyosung;
                    default:                        return ATMType.Other;
                }
            }
            set
            {
                ATM_Type = value switch
                {
                    ATMType.NCR            => AppConstants.ATM_TYPE_NCR,
                    ATMType.GRG            => AppConstants.ATM_TYPE_GRG,
                    ATMType.WN             => AppConstants.ATM_TYPE_WN,
                    ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
                    ATMType.Hyosung        => AppConstants.ATM_TYPE_HY,
                    _                      => "OTHER"
                };
            }
        }
        public string Location { get => Region; set => Region = value; }
        public string BranchCode { get; set; }

        // شبكة
        public string ServerIP      { get; set; }
        public int    ServerPort    { get; set; } = 5656;
        public string NetworkType   { get; set; } = "LAN";
        public int    Latency_ms    { get; set; }
        public int Latency { get => Latency_ms; set => Latency_ms = value; }

        // حالة الاتصال
        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
        public ATMStatus        Status           { get; set; } = ATMStatus.Unknown;
        public string           SessionId        { get; set; }
        public bool             IsSupervisorMode { get; set; }
        public bool             IsHostConnected  { get; set; }

        // طوابع زمنية UTC (T-11)
        public DateTime ConnectedAtUtc       { get; set; }
        public DateTime DisconnectedAtUtc    { get; set; }
        public DateTime LastHeartbeatUtc     { get; set; }
        public DateTime LastSyncUtc          { get; set; }
        public DateTime LastDataReceivedUtc  { get; set; }
        public DateTime LastCommandSentUtc   { get; set; }
        public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
        public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }

        // إحصاءات المزامنة
        public long   TotalSyncedBytes         { get; set; }
        public long   TotalTransactions        { get; set; }
        public int    ConsecutiveSyncFailures  { get; set; }
        public double SyncSuccessRate          { get; set; } = 100.0;
        public double ReceiveSpeedKBs          { get; set; }
        public long   JournalSizeToday         { get; set; }
        public double CpuUsagePercent          { get; set; }
        public double MemoryUsagePercent       { get; set; }
        public double DiskUsagePercent         { get; set; }
        public int    HealthScore              { get; set; } = 100;
        public int PendingJournalCount { get; set; }
        public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
        public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }

        // إحصاءات العمليات
        public int  ApprovedTransactions  { get; set; }
        public int  FailedTransactions    { get; set; }
        public int  CardsCaptured         { get; set; }
        public long CashDispensed         { get; set; }

        // آخر جورنال / خطأ
        public string LastJournalFile   { get; set; }
        public string LastErrorCode     { get; set; }
        public string LastErrorMessage  { get; set; }
        public string LastTransaction   { get; set; }
        public string LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
        public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }
        public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; } }

        // مسارات
        private string _sourcePath, _backupPath;
        public string ClientVersion { get; set; }
        public string OSVersion     { get; set; }

        public string GetSourcePath() => !string.IsNullOrEmpty(_sourcePath)
            ? _sourcePath
            : AppConstants.GetDefaultSourcePath(ATM_Type);

        public void SetSourcePath(string v) => _sourcePath = v;

        public string GetBackupPath() => !string.IsNullOrEmpty(_backupPath) ? _backupPath
            : System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "LocalBackup", ATM_ID ?? "DEFAULT");

        public void SetBackupPath(string v) => _backupPath = v;

        // ==========================================
        // حالة البطاقة ولونها
        // ==========================================

        public ATMCardState GetCardState()
        {
            if (ConnectionStatus == ConnectionStatus.Disconnected)
            {
                if (LastHeartbeatUtc == DateTime.MinValue) return ATMCardState.NeverConnected;
                var mins = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
                if (mins > 10) return ATMCardState.CriticalOffline;
                if (mins > 5)  return ATMCardState.WarningOffline;
                return ATMCardState.RecentlyDisconnected;
            }
            if (ConnectionStatus == ConnectionStatus.Syncing)     return ATMCardState.Syncing;
            if (IsSupervisorMode)                                 return ATMCardState.Supervisor;
            if (ConnectionStatus == ConnectionStatus.WaitingReply) return ATMCardState.WaitingReply;
            var noData = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
            return noData > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
        }

        public Color GetCardColor()
        {
            return GetCardState() switch
            {
                ATMCardState.ConnectedActive      => Color.FromArgb(52,  199, 89),   // أخضر
                ATMCardState.ConnectedIdle        => Color.FromArgb(255, 214, 10),   // أصفر
                ATMCardState.Syncing              => Color.FromArgb(10,  132, 255),  // أزرق
                ATMCardState.WaitingReply         => Color.FromArgb(10,  132, 255),  // أزرق
                ATMCardState.Supervisor           => Color.FromArgb(255, 159, 10),   // برتقالي
                ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69,  58),   // أحمر
                ATMCardState.WarningOffline       => Color.FromArgb(255, 69,  58),   // أحمر
                ATMCardState.CriticalOffline      => Color.FromArgb(99,  99,  102),  // رمادي
                ATMCardState.NeverConnected       => Color.FromArgb(72,  72,  74),   // رمادي داكن
                _ => Color.Gray
            };
        }

        public string GetStatusLabel()
        {
            return GetCardState() switch
            {
                ATMCardState.ConnectedActive      => "● متصل ونشط",
                ATMCardState.ConnectedIdle        => "● متصل خامل",
                ATMCardState.Syncing              => "⟳ يزامن",
                ATMCardState.WaitingReply         => "◎ ينتظر رد",
                ATMCardState.Supervisor           => "★ Supervisor",
                ATMCardState.RecentlyDisconnected => "✕ انقطع للتو",
                ATMCardState.WarningOffline       => "✕ انقطاع >5د",
                ATMCardState.CriticalOffline      => "✕ انقطاع حرج",
                ATMCardState.NeverConnected       => "○ لم يتصل",
                _ => "?"
            };
        }

        public string GetStatusDescription() => GetStatusLabel();

        public string GetConnectionStatusDescription() => ConnectionStatus.ToString();

        public string GetElapsed(DateTime utcRef)
        {
            if (utcRef == DateTime.MinValue) return "---";
            var s = (DateTime.UtcNow - utcRef).TotalSeconds;
            if (s < 60)   return $"{(int)s}ث";
            if (s < 3600) return $"{(int)(s/60)}د";
            return $"{(int)(s/3600)}س {(int)((s%3600)/60)}د";
        }

        public void RecalculateHealthScore()
        {
            var score = 100;
            if (ConnectionStatus == ConnectionStatus.Disconnected) score -= 35;
            if (Latency_ms > 500) score -= 15;
            if (ConsecutiveSyncFailures > 0) score -= Math.Min(25, ConsecutiveSyncFailures * 5);
            if (CpuUsagePercent > 90) score -= 10;
            if (MemoryUsagePercent > 90) score -= 10;
            if (DiskUsagePercent > 95) score -= 10;
            HealthScore = Math.Max(0, Math.Min(100, score));
        }

        public override string ToString() =>
            $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) — {ConnectionStatus}";
    }

    // ==========================================
    // التنبيهات
    // ==========================================

    public class AlertPayload
    {
        public string        AlertId   { get; set; } = Guid.NewGuid().ToString("N");
        public AlertSeverity Severity  { get; set; }
        public string        Title     { get; set; }
        public string        Message   { get; set; }
        public string        Source    { get; set; }
        public string        DedupeKey { get; set; }
        public DateTime      CreatedAt { get; set; } = DateTime.UtcNow;
        public bool          IsRead    { get; set; }

        public string Icon => Severity switch
        {
            AlertSeverity.Emergency => "CRIT",
            AlertSeverity.Critical  => "FAIL",
            AlertSeverity.Warning   => "WARN",
            _                       => "INFO"
        };
        public string SeverityIcon => Icon;
        public Color Color => Severity switch
        {
            AlertSeverity.Emergency => Color.FromArgb(255, 59,  48),
            AlertSeverity.Critical  => Color.FromArgb(255, 59,  48),
            AlertSeverity.Warning   => Color.FromArgb(255, 149, 0),
            _                       => Color.FromArgb(0,   122, 255)
        };
    }

    // ==========================================
    // سجل المزامنة
    // ==========================================

    public class JournalSyncRecord
    {
        public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
        public string           ATM_ID          { get; set; }
        public string           FileName        { get; set; }
        public long             FileSize        { get; set; }
        public long             FileOffset      { get; set; }
        public string           Checksum        { get; set; }
        public string           MD5Hash         { get; set; }
        public string           SHA256Hash      { get; set; }
        public JournalSyncState State           { get; set; }
        public int              ProgressPercent { get; set; }
        public int              RetryCount      { get; set; }
        public string           LocalPath       { get; set; }
        public string           ServerPath      { get; set; }
        public string           Message         { get; set; }
        public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
        public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
        public DateTime?        CompletedAtUtc  { get; set; }

        public string StateIcon => State switch
        {
            JournalSyncState.Pending   => "PEND",
            JournalSyncState.Syncing   => "SYNC",
            JournalSyncState.ReSyncing => "RSYNC",
            JournalSyncState.Completed => "OK",
            JournalSyncState.Failed    => "FAIL",
            JournalSyncState.Archived  => "ARCH",
            _ => "?"
        };

        public string StateLabel => State switch
        {
            JournalSyncState.Pending   => "في الطابور",
            JournalSyncState.Syncing   => "قيد المزامنة",
            JournalSyncState.ReSyncing => "إعادة مزامنة",
            JournalSyncState.Completed => "محمّل",
            JournalSyncState.Failed    => "فشل",
            JournalSyncState.Archived  => "مؤرشف",
            _ => "؟"
        };
    
        public string ATM_ID { get; set; }
        public string ATM_Name { get; set; }
        public string ATM_Description { get; set; }
        public string ATM_Type { get; set; }
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public ATMStatus Status { get; set; }
        public bool IsConnected { get; set; }
        public bool IsSendingData { get; set; }
        public bool IsCSCConnected { get; set; }
        public DateTime LastConnectionTime { get; set; }
        public DateTime LastDataReceived { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public int[,] OperationStats { get; set; }
        public int ATMCache { get; set; }
        public int TotalDispensed { get; set; }
        public SyncStatus SyncState { get; set; }
        public long TotalBytesSent { get; set; }
        public int TotalFilesSynced { get; set; }
        public int TotalLinesSent { get; set; }
        public DateTime LastSyncTime { get; set; }
        public string LastSyncFile { get; set; }
        public string OSVersion { get; set; }
        public string ClientVersion { get; set; }

        public ATMInfo()
        {
            OperationStats = new int[3, 4];
            Status = ATMStatus.Unknown;
            SyncState = SyncStatus.Idle;
            ServerPort = NetworkConfig.DEFAULT_PORT;
        }

        public string GetStatusColor()
        {
            if (Status == ATMStatus.Supervisor) return ATMStatusColors.COLOR_SUPERVISOR;
            if (!IsConnected)
            {
                var elapsed = DateTime.Now - LastConnectionTime;
                if (elapsed.TotalMinutes >= ATMStatusColors.OFFLINE_THRESHOLD_MINUTES)
                    return ATMStatusColors.COLOR_OFFLINE;
                if (elapsed.TotalMinutes >= ATMStatusColors.WARNING_THRESHOLD_MINUTES)
                    return ATMStatusColors.COLOR_WARNING;
                return ATMStatusColors.COLOR_OFFLINE;
            }
            if (IsSendingData) return ATMStatusColors.COLOR_ACTIVE;
            var idleElapsed = DateTime.Now - LastDataReceived;
            if (idleElapsed.TotalSeconds >= ATMStatusColors.IDLE_THRESHOLD_SECONDS)
                return ATMStatusColors.COLOR_IDLE;
            return ATMStatusColors.COLOR_ACTIVE;
        }

        public string GetSourcePath()
        {
            switch (ATM_Type)
            {
                case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_SOURCE;
                case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_SOURCE;
                case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_SOURCE;
                default: return string.Empty;
            }
        }

        public string GetBackupPath()
        {
            switch (ATM_Type)
            {
                case AppConstants.ATM_TYPE_NCR: return ATMPaths.NCR_BACKUP;
                case AppConstants.ATM_TYPE_GRG: return ATMPaths.GRG_BACKUP;
                case AppConstants.ATM_TYPE_WN: return ATMPaths.WN_BACKUP;
                default: return string.Empty;
            }
        }

        public SyncStrategy GetSyncStrategy()
        {
            switch (ATM_Type)
            {
                case AppConstants.ATM_TYPE_NCR: return SyncStrategy.NCR_Overwrite;
                case AppConstants.ATM_TYPE_GRG: return SyncStrategy.GRG_DailyFiles;
                case AppConstants.ATM_TYPE_WN: return SyncStrategy.WN_DailyFiles;
                default: return SyncStrategy.NCR_Overwrite;
            }
        }

        public bool NeedsAlert()
        {
            if (!IsConnected) return true;
            return (DateTime.Now - LastDataReceived).TotalMinutes >= 60;
        }
    }

    public class ClientConfig
    {
        public string ATM_ID { get; set; }
        public string ATM_Name { get; set; }
        public string ATM_Type { get; set; }
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public bool SyncTimeEnabled { get; set; }
        public int MessageSizeLines { get; set; }
        public int FilePackageKB { get; set; }
        public string SourcePath { get; set; }
        public string BackupPath { get; set; }
        public bool AutoStart { get; set; }
        public bool RunAsService { get; set; }

        public ClientConfig()
        {
            ServerPort = NetworkConfig.DEFAULT_PORT;
            MessageSizeLines = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
            FilePackageKB = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
            AutoStart = true;
            RunAsService = true;
        }
    }

    public class ServerConfig
    {
        public int ListenPort { get; set; }
        public string StoragePath { get; set; }
        public string ArchivePath { get; set; }
        public bool AutoArchive { get; set; }
        public int MaxConnections { get; set; }
        public bool EnableEncryption { get; set; }
        public bool EnableCompression { get; set; }

        public ServerConfig()
        {
            ListenPort = NetworkConfig.DEFAULT_PORT;
            StoragePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
            ArchivePath = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
            AutoArchive = true;
            MaxConnections = 100;
            EnableEncryption = true;
            EnableCompression = true;
        }
    }
}

