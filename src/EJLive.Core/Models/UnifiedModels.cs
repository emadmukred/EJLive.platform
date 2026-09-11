using System.Drawing;
using System.Text.Json;
using EJLive.Shared;

namespace EJLive.Core.Models;

public enum ConnectionStatus { Disconnected = 0, Connecting = 1, Connected = 2, WaitingReply = 3, Syncing = 4 }
public enum ATMStatus { Unknown = 0, Online = 1, Idle = 2, Supervisor = 3, Warning = 4, Offline = 5, Critical = 6, InService = 10, ConnectedOnly = 11, WaitingResponse = 12, OutOfService = 13, CriticalFault = 14, Fault = 15, Maintenance = 16 }
public enum ATMType { NCR, GRG, WN, DieboldNixdorf, Hyosung, Other }
public enum SyncStatus { Idle, Pending, InProgress, Syncing, Resyncing, Completed, Failed, Archived }
public enum AlertSeverity { Info = 0, Warning = 1, Critical = 2, Emergency = 3 }
public enum JournalSyncState { Pending = 0, Syncing = 1, ReSyncing = 2, Completed = 3, Failed = 4, Archived = 5 }
public enum ATMCardState { NeverConnected, ConnectedActive, ConnectedIdle, Syncing, WaitingReply, Supervisor, RecentlyDisconnected, WarningOffline, CriticalOffline }
public enum SyncStrategy { NCR_Overwrite, GRG_DailyFiles, WN_DailyFiles, GenericAppend }
public enum RemoteCommandStatus { Pending, Sent, Running, Completed, Failed, Cancelled }
public enum RemoteSessionState { Created, Starting, Active, Paused, Stopped, Failed }
public enum TransactionType { Unknown, Withdrawal, Deposit, Transfer, BalanceInquiry, Reversal, BillPayment }
public enum TransactionStatus { Unknown, Approved, Declined, Failed, Reversed, Timeout }
public enum ATMOperationalState { Unknown, InService, OutOfService, Supervisor, Maintenance, Faulted }

public sealed class ATMInfo
{
    private string? _sourcePath;
    private string? _backupPath;

    public string? ATM_ID { get; set; }
    public string? ATM_Name { get; set; }
    public string? ATM_Type { get; set; } = AppConstants.ATM_TYPE_NCR;
    public string? BranchName { get; set; }
    public string? Region { get; set; }
    public string? BranchCode { get; set; }
    public string? ATMId { get => ATM_ID; set => ATM_ID = value; }
    public string? ATMName { get => ATM_Name; set => ATM_Name = value; }
    public string? ATM_Description { get; set; }
    public string? IPAddress { get => ServerIP; set => ServerIP = value; }
    public string? Location { get => Region; set => Region = value; }

    public ATMType ATMType
    {
        get => AppConstants.NormalizeATMType(ATM_Type) switch
        {
            AppConstants.ATM_TYPE_NCR => ATMType.NCR,
            AppConstants.ATM_TYPE_GRG => ATMType.GRG,
            AppConstants.ATM_TYPE_WN => ATMType.WN,
            AppConstants.ATM_TYPE_DN => ATMType.DieboldNixdorf,
            AppConstants.ATM_TYPE_HY => ATMType.Hyosung,
            _ => ATMType.Other
        };
        set => ATM_Type = value switch
        {
            ATMType.NCR => AppConstants.ATM_TYPE_NCR,
            ATMType.GRG => AppConstants.ATM_TYPE_GRG,
            ATMType.WN => AppConstants.ATM_TYPE_WN,
            ATMType.DieboldNixdorf => AppConstants.ATM_TYPE_DN,
            ATMType.Hyosung => AppConstants.ATM_TYPE_HY,
            _ => "OTHER"
        };
    }

    public string? ServerIP { get; set; }
    public int ServerPort { get; set; } = NetworkConfig.DEFAULT_PORT;
    public string NetworkType { get; set; } = "LAN";
    public int Latency_ms { get; set; }
    public int Latency { get => Latency_ms; set => Latency_ms = value; }

    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    public ATMStatus Status { get; set; } = ATMStatus.Unknown;
    public string? SessionId { get; set; }
    public bool IsSupervisorMode { get; set; }
    public bool IsHostConnected { get; set; }
    public bool IsConnected { get; set; }
    public bool IsSendingData { get; set; }
    public bool IsCSCConnected { get; set; }

    public DateTime ConnectedAtUtc { get; set; }
    public DateTime DisconnectedAtUtc { get; set; }
    public DateTime LastHeartbeatUtc { get; set; }
    public DateTime LastSyncUtc { get; set; }
    public DateTime LastDataReceivedUtc { get; set; }
    public DateTime LastCommandSentUtc { get; set; }
    public DateTime LastConnectionTime { get => ConnectedAtUtc; set => ConnectedAtUtc = value; }
    public DateTime LastSyncTime { get => LastSyncUtc; set => LastSyncUtc = value; }
    public DateTime LastDataReceived { get => LastDataReceivedUtc; set => LastDataReceivedUtc = value; }
    public DateTime LastHeartbeat { get => LastHeartbeatUtc; set => LastHeartbeatUtc = value; }

    public long TotalSyncedBytes { get; set; }
    public long TotalTransactions { get; set; }
    public int ConsecutiveSyncFailures { get; set; }
    public double SyncSuccessRate { get; set; } = 100.0;
    public double ReceiveSpeedKBs { get; set; }
    public long JournalSizeToday { get; set; }
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskUsagePercent { get; set; }
    public int HealthScore { get; set; } = 100;
    public int PendingJournalCount { get; set; }
    public double SuccessRate { get => SyncSuccessRate; set => SyncSuccessRate = value; }
    public long TotalTransactionsSynced { get => TotalTransactions; set => TotalTransactions = value; }
    public int TransactionCount { get => (int)Math.Min(int.MaxValue, TotalTransactions); set => TotalTransactions = value; }

    public int ApprovedTransactions { get; set; }
    public int FailedTransactions { get; set; }
    public int CardsCaptured { get; set; }
    public long CashDispensed { get; set; }
    public int[,] OperationStats { get; set; } = new int[3, 4];
    public int ATMCache { get; set; }
    public int TotalDispensed { get; set; }
    public int Cassette1Remaining { get; set; }
    public int Cassette2Remaining { get; set; }
    public int Cassette3Remaining { get; set; }
    public int Cassette4Remaining { get; set; }
    public int CashLoadedTotal { get; set; }
    public int CashDepositInTotal { get; set; }
    public int CashRejectCount { get; set; }
    public int CashRetractCount { get; set; }
    public DateTime CashTelemetryUpdatedAtUtc { get; set; }
    public SyncStatus SyncState { get; set; } = SyncStatus.Idle;
    public long TotalBytesSent { get; set; }
    public int TotalFilesSynced { get; set; }
    public int TotalLinesSent { get; set; }

    public string? LastJournalFile { get; set; }
    public string? LastSyncFile { get; set; }
    public string? LastErrorCode { get; set; }
    public string? LastErrorMessage { get; set; }
    public string? LastTransaction { get; set; }
    public string? LastError { get => LastErrorMessage; set => LastErrorMessage = value; }
    public bool IsSyncing { get => ConnectionStatus == ConnectionStatus.Syncing; set { if (value) ConnectionStatus = ConnectionStatus.Syncing; } }
    public bool HasCashTelemetry =>
        CashTelemetryUpdatedAtUtc > DateTime.MinValue &&
        (Cassette1Remaining > 0 ||
         Cassette2Remaining > 0 ||
         Cassette3Remaining > 0 ||
         Cassette4Remaining > 0 ||
         ATMCache > 0 ||
         CashLoadedTotal > 0 ||
         TotalDispensed > 0 ||
         CashDepositInTotal > 0 ||
         CashRejectCount > 0 ||
         CashRetractCount > 0);
    public string? ClientVersion { get; set; }
    public string? OSVersion { get; set; }

    public string GetSourcePath() => !string.IsNullOrWhiteSpace(_sourcePath) ? _sourcePath : AppConstants.GetDefaultSourcePath(ATM_Type);
    public void SetSourcePath(string value) => _sourcePath = value;
    public string GetBackupPath() => !string.IsNullOrWhiteSpace(_backupPath) ? _backupPath : AppConstants.GetDefaultBackupPath(ATM_Type);
    public void SetBackupPath(string value) => _backupPath = value;
    public SyncStrategy GetSyncStrategy() => AppConstants.NormalizeATMType(ATM_Type) switch
    {
        AppConstants.ATM_TYPE_NCR => SyncStrategy.NCR_Overwrite,
        AppConstants.ATM_TYPE_GRG => SyncStrategy.GRG_DailyFiles,
        AppConstants.ATM_TYPE_WN => SyncStrategy.WN_DailyFiles,
        _ => SyncStrategy.GenericAppend
    };

    public ATMCardState GetCardState()
    {
        if (ConnectionStatus == ConnectionStatus.Disconnected)
        {
            if (LastHeartbeatUtc == DateTime.MinValue)
                return ATMCardState.NeverConnected;

            var minutes = (DateTime.UtcNow - LastHeartbeatUtc).TotalMinutes;
            if (minutes > AppConstants.AlertDisconnectCriticalMin)
                return ATMCardState.CriticalOffline;
            if (minutes > AppConstants.AlertDisconnectWarningMin)
                return ATMCardState.WarningOffline;
            return ATMCardState.RecentlyDisconnected;
        }

        if (ConnectionStatus == ConnectionStatus.Syncing)
            return ATMCardState.Syncing;
        if (IsSupervisorMode)
            return ATMCardState.Supervisor;
        if (ConnectionStatus == ConnectionStatus.WaitingReply)
            return ATMCardState.WaitingReply;

        var idleMinutes = (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes;
        return idleMinutes > 30 ? ATMCardState.ConnectedIdle : ATMCardState.ConnectedActive;
    }

    public Color GetCardColor() => GetCardState() switch
    {
        ATMCardState.ConnectedActive => Color.FromArgb(52, 199, 89),
        ATMCardState.ConnectedIdle => Color.FromArgb(255, 214, 10),
        ATMCardState.Syncing => Color.FromArgb(10, 132, 255),
        ATMCardState.WaitingReply => Color.FromArgb(10, 132, 255),
        ATMCardState.Supervisor => Color.FromArgb(255, 159, 10),
        ATMCardState.RecentlyDisconnected => Color.FromArgb(255, 69, 58),
        ATMCardState.WarningOffline => Color.FromArgb(255, 69, 58),
        ATMCardState.CriticalOffline => Color.FromArgb(99, 99, 102),
        _ => Color.Gray
    };

    public string GetStatusLabel() => GetCardState() switch
    {
        ATMCardState.ConnectedActive => "Connected and active",
        ATMCardState.ConnectedIdle => "Connected and idle",
        ATMCardState.Syncing => "Syncing",
        ATMCardState.WaitingReply => "Waiting for reply",
        ATMCardState.Supervisor => "Supervisor mode",
        ATMCardState.RecentlyDisconnected => "Recently disconnected",
        ATMCardState.WarningOffline => "Offline warning",
        ATMCardState.CriticalOffline => "Critical offline",
        ATMCardState.NeverConnected => "Never connected",
        _ => "Unknown"
    };

    public string GetStatusDescription() => GetStatusLabel();
    public string GetConnectionStatusDescription() => ConnectionStatus.ToString();
    public string GetStatusColor() => ColorTranslator.ToHtml(GetCardColor());
    public bool NeedsAlert() => !IsConnected || (DateTime.UtcNow - LastDataReceivedUtc).TotalMinutes >= AppConstants.AlertNoDataWarningMin;

    public void RecalculateHealthScore()
    {
        var score = 100;
        if (ConnectionStatus == ConnectionStatus.Disconnected)
            score -= 35;
        if (Latency_ms > 500)
            score -= 15;
        if (ConsecutiveSyncFailures > 0)
            score -= Math.Min(25, ConsecutiveSyncFailures * 5);
        if (CpuUsagePercent > 90)
            score -= 10;
        if (MemoryUsagePercent > 90)
            score -= 10;
        if (DiskUsagePercent > 95)
            score -= 10;
        HealthScore = Math.Clamp(score, 0, 100);
    }

    public override string ToString() => $"[{ATM_ID}] {ATM_Name} ({ATM_Type}/{NetworkType}) - {ConnectionStatus}";
}

public class ClientConfig
{
    public string ATM_ID { get; set; } = "ATM001";
    public string ATM_Name { get; set; } = "Default ATM";
    public string ATM_Type { get; set; } = AppConstants.ATM_TYPE_NCR;
    public string ServerIP { get; set; } = "127.0.0.1";
    public int ServerPort { get; set; } = NetworkConfig.DEFAULT_PORT;
    public string NetworkType { get; set; } = "LAN";
    public bool SyncTimeEnabled { get; set; } = true;
    public int MessageSizeLines { get; set; } = NetworkConfig.DEFAULT_MESSAGE_SIZE_LINES;
    public int FilePackageKB { get; set; } = NetworkConfig.DEFAULT_FILE_PACKAGE_KB;
    public string SourcePath { get; set; } = AppConstants.GetDefaultSourcePath(AppConstants.ATM_TYPE_NCR);
    public string BackupPath { get; set; } = AppConstants.GetDefaultBackupPath(AppConstants.ATM_TYPE_NCR);
    public bool AutoStart { get; set; } = true;
    public bool RunAsService { get; set; } = true;
    public bool AutoConnect { get; set; } = true;
    public bool EnableEncryption { get; set; } = true;
    public bool EnableCompression { get; set; } = true;
    public bool EnableTlsTransport { get; set; } = false;
    public bool RequireTlsTransport { get; set; } = false;
    public bool AllowUntrustedTlsCertificate { get; set; } = false;
    public bool EnableAdaptiveChunking { get; set; } = true;
    public int WeakNetworkLatencyMs { get; set; } = 500;
    public bool AutoBackup { get; set; } = true;
    public bool EnforceCommandAuthorization { get; set; } = true;
    public string DefaultCommandRole { get; set; } = "Support";
    public bool EnableSupabaseSync { get; set; } = false;
    public string SupabaseUrl { get; set; } = string.Empty;
    public string SupabaseServiceKey { get; set; } = string.Empty;
    public string ImageInboxPath { get; set; } = Path.Combine(AppConstants.DefaultImagesPath, "Inbox");
    public bool AutoEnableRemoteAccess { get; set; } = true;
    public bool AutoPrepareWindowsRuntime { get; set; } = true;
    public bool EnableWinRmBootstrap { get; set; } = true;
    public bool EnableRemoteRegistryBootstrap { get; set; } = true;
    public bool EnforceScopedFirewallRule { get; set; } = true;
    public int ScopedFirewallPort { get; set; } = 0;
    public string ScopedFirewallRemoteAddresses { get; set; } = string.Empty;
    public bool ConfigureDefenderExclusions { get; set; } = true;
    public string DefenderExclusionPaths { get; set; } = string.Empty;
    public string HelpdeskAdGroup { get; set; } = "EJLive-Helpdesk";
    public int WindowsBaselineRepairIntervalMin { get; set; } = 30;
    public string WindowsPolicyProfileMode { get; set; } = "Enforce";
    public bool AllowLocalWindowsPasswordChange { get; set; } = false;
    public bool RequireEncryptedWindowsPasswordPayload { get; set; } = true;
    public string AllowedPasswordAccounts { get; set; } = "Administrator,Helpdesk";
    public bool AllowUnsignedCommands { get; set; }
    public bool EnforceLowPriorityMode { get; set; } = true;
    public bool PinToLastProcessorCore { get; set; } = false;
    public int HeartbeatIntervalSec { get; set; } = AppConstants.HeartbeatIntervalSec;
    public int ReconnectIntervalSec { get; set; } = 20;
}

public sealed class ServerConfig
{
    public int ListenPort { get; set; } = NetworkConfig.DEFAULT_PORT;
    public string StoragePath { get; set; } = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES;
    public string ArchivePath { get; set; } = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_REPORTS;
    public bool AutoArchive { get; set; } = true;
    public int MaxConnections { get; set; } = 100;
    public bool EnableEncryption { get; set; } = true;
    public bool EnableCompression { get; set; } = true;
}

public sealed class AppConfig : ClientConfig
{
    private static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Client", "appconfig.json");

    public static AppConfig Load()
    {
        try
        {
            if (!File.Exists(ConfigPath))
                return new AppConfig();

            var loaded = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigPath)) ?? new AppConfig();
            loaded.SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(loaded.SupabaseServiceKey);
            loaded.ApplyDefaults();
            return loaded;
        }
        catch
        {
            return new AppConfig();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        var serialized = JsonSerializer.Serialize(this);
        var copy = JsonSerializer.Deserialize<AppConfig>(serialized) ?? new AppConfig();
        copy.SupabaseServiceKey = SecurityHelper.ProtectDpapiStringIfNeeded(copy.SupabaseServiceKey);

        File.WriteAllText(
            ConfigPath,
            JsonSerializer.Serialize(copy, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void ApplyDefaults()
    {
        ATM_Type = AppConstants.NormalizeATMType(ATM_Type);
        if (string.IsNullOrWhiteSpace(SourcePath))
            SourcePath = AppConstants.GetDefaultSourcePath(ATM_Type);
        if (string.IsNullOrWhiteSpace(BackupPath))
            BackupPath = AppConstants.GetDefaultBackupPath(ATM_Type);
        if (string.IsNullOrWhiteSpace(ImageInboxPath))
            ImageInboxPath = Path.Combine(AppConstants.DefaultImagesPath, "Inbox");
        if (string.IsNullOrWhiteSpace(DefaultCommandRole))
            DefaultCommandRole = "Support";
        else
            DefaultCommandRole = DefaultCommandRole.Trim();
        if (HeartbeatIntervalSec <= 0)
            HeartbeatIntervalSec = AppConstants.HeartbeatIntervalSec;
        if (ReconnectIntervalSec <= 0)
            ReconnectIntervalSec = 20;
        WeakNetworkLatencyMs = Math.Clamp(WeakNetworkLatencyMs, 120, 3000);
        if (WindowsBaselineRepairIntervalMin <= 0)
            WindowsBaselineRepairIntervalMin = 30;
        if (string.IsNullOrWhiteSpace(WindowsPolicyProfileMode))
            WindowsPolicyProfileMode = "Enforce";
        else
            WindowsPolicyProfileMode = WindowsPolicyProfileMode.Trim();
        HeartbeatIntervalSec = Math.Clamp(HeartbeatIntervalSec, 5, 300);
        ReconnectIntervalSec = Math.Clamp(ReconnectIntervalSec, 5, 300);
        WindowsBaselineRepairIntervalMin = Math.Clamp(WindowsBaselineRepairIntervalMin, 5, 720);
        SupabaseUrl = (SupabaseUrl ?? string.Empty).Trim();
        SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(SupabaseServiceKey).Trim();
        AllowedPasswordAccounts = (AllowedPasswordAccounts ?? string.Empty).Trim();
        ScopedFirewallRemoteAddresses = string.IsNullOrWhiteSpace(ScopedFirewallRemoteAddresses)
            ? (ServerIP ?? string.Empty).Trim()
            : ScopedFirewallRemoteAddresses.Trim();
        DefenderExclusionPaths = (DefenderExclusionPaths ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(HelpdeskAdGroup))
            HelpdeskAdGroup = "EJLive-Helpdesk";
        else
            HelpdeskAdGroup = HelpdeskAdGroup.Trim();
        ScopedFirewallPort = Math.Clamp(ScopedFirewallPort, 0, 65535);
        if (ServerPort <= 0)
            ServerPort = NetworkConfig.DEFAULT_PORT;
    }
}

public sealed class AgentConfigurationRecord
{
    public string ConfigPath { get; set; } = string.Empty;
    public DateTime LoadedAtUtc { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}


public sealed class JournalSyncRecord
{
    public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
    public string ATM_ID { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public long FileOffset { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public string MD5Hash { get; set; } = string.Empty;
    public string SHA256Hash { get; set; } = string.Empty;
    public JournalSyncState State { get; set; } = JournalSyncState.Pending;
    public int ProgressPercent { get; set; }
    public int RetryCount { get; set; }
    public string LocalPath { get; set; } = string.Empty;
    public string ServerPath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public string StateIcon => State switch
    {
        JournalSyncState.Pending => "PEND",
        JournalSyncState.Syncing => "SYNC",
        JournalSyncState.ReSyncing => "RSYNC",
        JournalSyncState.Completed => "OK",
        JournalSyncState.Failed => "FAIL",
        JournalSyncState.Archived => "ARCH",
        _ => "?"
    };
    public string StateLabel => State.ToString();
}

public sealed class LiveSyncProgress
{
    public string ATM_ID { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long BytesSent { get; set; }
    public long TotalBytes { get; set; }
    public int CurrentChunk { get; set; }
    public int TotalChunks { get; set; }
    public double SpeedKBs { get; set; }
    public SyncStatus Status { get; set; } = SyncStatus.Pending;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public int Percent => TotalBytes <= 0 ? 0 : (int)Math.Clamp(BytesSent * 100.0 / TotalBytes, 0, 100);
}

public sealed class RemoteCommand
{
    public string CommandId { get; set; } = Guid.NewGuid().ToString("N");
    public string ATM_ID { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public RemoteCommandStatus Status { get; set; } = RemoteCommandStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SentAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public bool RequiresConfirmation { get; set; }
    public string Result { get; set; } = string.Empty;
}

public sealed class ATMTransaction
{
    public string TransactionId { get; set; } = Guid.NewGuid().ToString("N");
    public string ATM_ID { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public string RawText { get; set; } = string.Empty;
}


public sealed class RemoteSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public string ATM_ID { get; set; } = string.Empty;
    public RemoteSessionState Status { get; set; } = RemoteSessionState.Created;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAtUtc { get; set; }
}

public sealed class RetainedCard
{
    public string ATM_ID { get; set; } = string.Empty;
    public string MaskedPan { get; set; } = string.Empty;
    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
}



public sealed class AuditLogEntry
{
    public string EntryId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty;
}

public sealed class FleetSummary
{
    public int Total { get; set; }
    public int Connected { get; set; }
    public int Syncing { get; set; }
    public int Offline { get; set; }
    public int AverageHealth { get; set; }
    public int AttentionRequired => Offline + Math.Max(0, Total - Connected - Offline);
}

public sealed class SyncSummary
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int AverageProgress { get; set; }
    public int OpenItems => Pending + InProgress + Failed;
}

public sealed class TerminalCashStatus
{
    public string Source { get; set; } = "Derived";
    public int Cassette1 { get; set; }
    public int Cassette2 { get; set; }
    public int Cassette3 { get; set; }
    public int Cassette4 { get; set; }
    public int Remaining { get; set; }
    public int Loaded { get; set; }
    public int DepositIn { get; set; }
    public int DispenseOut { get; set; }
    public int Reject { get; set; }
    public int Retract { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public int LowCashThreshold { get; set; } = 3000;

    public int TotalCassetteNotes => Math.Max(0, Cassette1) +
                                     Math.Max(0, Cassette2) +
                                     Math.Max(0, Cassette3) +
                                     Math.Max(0, Cassette4);
    public bool IsLowCash => Remaining > 0 && Remaining <= LowCashThreshold;
    public bool IsEmpty => Remaining <= 0;
}

public sealed class TerminalLiveSummary
{
    public string TerminalId { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    public ATMStatus Status { get; set; } = ATMStatus.Unknown;
    public int HealthScore { get; set; }
    public bool SupervisorMode { get; set; }
    public int ActiveAlerts { get; set; }
    public DateTime LastHeartbeatUtc { get; set; }
    public DateTime LastEjSyncUtc { get; set; }
    public string LastTransaction { get; set; } = string.Empty;
    public TerminalCashStatus Cash { get; set; } = new();
}
