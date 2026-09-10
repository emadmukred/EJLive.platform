using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public enum JournalSyncState
        {
    
        public enum JournalSyncAlertSeverity
        {
    
        [Serializable]
        public enum JournalSyncStatus
        {
    
        [Serializable]
        public enum JournalTransferStatus
        {
    
        public partial public class JournalEntry
        {
            public double CompressionRatio =>
            OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
            public string FileSizeDisplay =>
            OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
            OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
            $"{OriginalSize} B";
            public string   EntryId          { get; set; }
            public string   ATMId            { get; set; }
            public string   FileName         { get; set; }
            public string   FilePath         { get; set; }
            public long     OriginalSize     { get; set; }
            public long     CompressedSize   { get; set; }
            public long     EncryptedSize    { get; set; }
            public bool     IsEncrypted      { get; set; }
            public bool     IsCompressed     { get; set; }
            public string   Checksum         { get; set; }
            public string   MD5Hash          { get; set; }
            public string   SHA256Hash       { get; set; }
            public int      TransactionCount { get; set; }
            public string   Status           { get; set; }
            public long     FileOffset       { get; set; }
            public string   ArchivePath      { get; set; }
            public DateTime ReceivedAt       { get; set; }
            public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
            public DateTime VerifiedAt       { get; set; }
            public string   MonthPartition   { get; set; }
        }
    
        public partial public class SyncStatusInfo
        {
            public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
            public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
            public string ETADisplay        => EstimatedTimeRemaining < 60
            ? $"{EstimatedTimeRemaining}ث"
            : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
            public string SyncedSizeDisplay =>
            SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
            SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
            $"{SyncedSize} B";
            public string   SyncId                  { get; set; }
            public string   ATMId                   { get; set; }
            public string   Status                  { get; set; }
            public int      TotalFiles              { get; set; }
            public int      SyncedFiles             { get; set; }
            public int      FailedFiles             { get; set; }
            public long     TotalSize               { get; set; }
            public long     SyncedSize              { get; set; }
            public int      ProgressPercentage      { get; set; }
            public double   SyncSpeed               { get; set; }
            public int      EstimatedTimeRemaining  { get; set; }
            public int      RetryCount              { get; set; }
            public string   FailureReason           { get; set; }
            public DateTime StartedAt               { get; set; }
            public DateTime LastUpdated             { get; set; }
            public DateTime? CompletedAt            { get; set; }
            public string FileName { get; set; }
            public JournalSyncState State { get; set; }
            public int ProgressPercent { get; set; }
            public string Message { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
        }
    
        public partial public class RemoteCommand
        {
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
            public string StatusIcon => Status switch
            {
            "Sent"      => "SENT",
            "Received"  => "RCVD",
            "Executed"  => "OK",
            "Failed"    => "FAIL",
            "Timeout"   => "TIME",
            _           => "?"
            };
            public string DisplayLabel =>
            $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
            public double SuccessRate =>
            TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
            public string BytesSentDisplay => BytesSent > 1048576
            ? $"{BytesSent / 1048576.0:F1} MB"
            : $"{BytesSent / 1024.0:F1} KB";
            public string TotalBytesDisplay => TotalBytes > 1048576
            ? $"{TotalBytes / 1048576.0:F1} MB"
            : $"{TotalBytes / 1024.0:F1} KB";
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            public const string Restart = "Restart";
            public const string Screenshot = "Screenshot";
            public const string SyncTime = "SyncTime";
            public const string StartSync = "StartSync";
            public const string StopSync = "StopSync";
            public const string GetStatus = "GetStatus";
            public const string Reboot = "Reboot";
            public const string Shutdown = "Shutdown";
            public RemoteCommand()
            {
            public string   CommandId     { get; set; }
            public string   CommandType   { get; set; }
            public string   TargetATMId   { get; set; }
            public CommandParameters Parameters { get; set; }
            public string   SentBy        { get; set; }
            public bool     RequireConfirm { get; set; }
            public DateTime SentAtUtc     { get; set; }
            public DateTime ExecutedAt    { get; set; }
            public string   Status        { get; set; }
            public string   Result        { get; set; }
            public DateTime? AckedAtUtc   { get; set; }
            public int      TimeoutSec    { get; set; }
            public string   Parameters    { get; set; }
            public string ATMId { get; set; }
            public RemoteCommandType CommandType { get; set; }
            public Dictionary<string, string> Parameters { get; private set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ExecutedAt { get; set; }
            public string Raw { get; set; }
            public DateTime Date                 { get; set; }
            public int      TotalTransactions    { get; set; }
            public int      ApprovedTransactions { get; set; }
            public int      FailedTransactions   { get; set; }
            public int      CardsCaptured        { get; set; }
            public long     CashDispensed        { get; set; }
            public long     JournalBytesReceived { get; set; }
            public double   UptimePercent        { get; set; }
            public double   SyncSuccessPercent   { get; set; }
            public string FileName        { get; set; }
            public string StateLabel      { get; set; }
            public string StateIcon       { get; set; }
            public int    Percent         { get; set; }
            public long   BytesSent       { get; set; }
            public long   TotalBytes      { get; set; }
            public double SpeedKBs        { get; set; }
            public int    SeqNum          { get; set; }
            public int    TotalChunks     { get; set; }
            public DateTime UpdatedAt     { get; set; }
            public enum JournalSyncState
            {
            Pending,
            Syncing,
            Completed,
            Failed,
            ReSyncing,
            Archived
            }
            public enum JournalSyncAlertSeverity
            {
            Info,
            Warning,
            Critical
            }
            public string SyncId { get; set; }
            public string ATM_ID { get; set; }
            public string LocalPath { get; set; }
            public string ArchivePath { get; set; }
            public string Checksum { get; set; }
            public string SHA256Hash { get; set; }
            public long FileSize { get; set; }
            public long FileOffset { get; set; }
            public int RetryCount { get; set; }
            public int ProgressPercent { get; set; }
            public JournalSyncState State { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public DateTime? StartedAtUtc { get; set; }
            public DateTime? CompletedAtUtc { get; set; }
            public DateTime? LastAttemptAtUtc { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int SyncingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public string LastError { get; set; }
            public string AlertId { get; set; }
            public JournalSyncAlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string RecommendedAction { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public VendorRootProfileSummary RootProfile { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public string ServerPath { get; set; }
            public string EntryId { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string EjPattern { get; set; }
            public string LogPattern { get; set; }
            public string GetCommandDescription() =>
            public override string ToString() =>
        }
    
        public partial public static class RemoteCommandType
        {
            public const string Restart = "Restart";
            public const string Screenshot = "Screenshot";
            public const string SyncTime = "SyncTime";
            public const string StartSync = "StartSync";
            public const string StopSync = "StopSync";
            public const string GetStatus = "GetStatus";
            public const string Reboot = "Reboot";
            public const string Shutdown = "Shutdown";
        }
    
        public partial public class CommandParameters : Dictionary<string, string>
        {
            public string Raw { get; set; }
            public override string ToString() =>
        }
    
        public partial public class JournalDailyStats
        {
            public double SuccessRate =>
            TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
            public string   ATMId                { get; set; }
            public DateTime Date                 { get; set; }
            public int      TotalTransactions    { get; set; }
            public int      ApprovedTransactions { get; set; }
            public int      FailedTransactions   { get; set; }
            public int      CardsCaptured        { get; set; }
            public long     CashDispensed        { get; set; }
            public long     JournalBytesReceived { get; set; }
            public double   UptimePercent        { get; set; }
            public double   SyncSuccessPercent   { get; set; }
        }
    
        public partial public class LiveSyncProgress
        {
            public string BytesSentDisplay => BytesSent > 1048576
            ? $"{BytesSent / 1048576.0:F1} MB"
            : $"{BytesSent / 1024.0:F1} KB";
            public string TotalBytesDisplay => TotalBytes > 1048576
            ? $"{TotalBytes / 1048576.0:F1} MB"
            : $"{TotalBytes / 1024.0:F1} KB";
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            public string ATMId           { get; set; }
            public string FileName        { get; set; }
            public string StateLabel      { get; set; }
            public string StateIcon       { get; set; }
            public int    Percent         { get; set; }
            public long   BytesSent       { get; set; }
            public long   TotalBytes      { get; set; }
            public double SpeedKBs        { get; set; }
            public int    SeqNum          { get; set; }
            public int    TotalChunks     { get; set; }
            public DateTime UpdatedAt     { get; set; }
            public enum JournalSyncState
            {
            Pending,
            Syncing,
            Completed,
            Failed,
            ReSyncing,
            Archived
            }
            public enum JournalSyncAlertSeverity
            {
            Info,
            Warning,
            Critical
            }
            public string SyncId { get; set; }
            public string ATM_ID { get; set; }
            public string LocalPath { get; set; }
            public string ArchivePath { get; set; }
            public string Checksum { get; set; }
            public string SHA256Hash { get; set; }
            public long FileSize { get; set; }
            public long FileOffset { get; set; }
            public int RetryCount { get; set; }
            public int ProgressPercent { get; set; }
            public JournalSyncState State { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public DateTime? StartedAtUtc { get; set; }
            public DateTime? CompletedAtUtc { get; set; }
            public DateTime? LastAttemptAtUtc { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int SyncingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public string LastError { get; set; }
            public string AlertId { get; set; }
            public JournalSyncAlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string RecommendedAction { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public VendorRootProfileSummary RootProfile { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
        }
    
        public partial public sealed class JournalSyncRecord
        {
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            public JournalSyncRecord()
            {
            public string SyncId { get; set; }
            public string ATM_ID { get; set; }
            public string FileName { get; set; }
            public string LocalPath { get; set; }
            public string ArchivePath { get; set; }
            public string Checksum { get; set; }
            public string SHA256Hash { get; set; }
            public long FileSize { get; set; }
            public long FileOffset { get; set; }
            public int RetryCount { get; set; }
            public int ProgressPercent { get; set; }
            public JournalSyncState State { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public DateTime? StartedAtUtc { get; set; }
            public DateTime? CompletedAtUtc { get; set; }
            public DateTime? LastAttemptAtUtc { get; set; }
            public string ServerPath { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int SyncingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public string LastError { get; set; }
            public string AlertId { get; set; }
            public JournalSyncAlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string RecommendedAction { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public VendorRootProfileSummary RootProfile { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public string EntryId { get; set; }
            public string ATMId { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string EjPattern { get; set; }
            public string LogPattern { get; set; }
        }
    
        public partial public sealed class JournalSyncStatusSnapshot
        {
            public string ATM_ID { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int SyncingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public string LastError { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
        }
    
        public partial public sealed class JournalSyncAlert
        {
            public string AlertId { get; set; }
            public string ATM_ID { get; set; }
            public JournalSyncAlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string RecommendedAction { get; set; }
            public DateTime CreatedAtUtc { get; set; }
        }
    
        public partial public sealed class JournalSyncDashboardItem
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public VendorRootProfileSummary RootProfile { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
        }
    
        public partial public public class JournalSyncModels
        {
        }
    
        public partial public public class JournalSyncEntry
        {
            public JournalSyncEntry()
            {
            public string FileName { get; set; }
            public string EntryId { get; set; }
            public string ATMId { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public string Checksum { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string EjPattern { get; set; }
            public string LogPattern { get; set; }
        }
    
        public partial public public class ATMJournalSyncState
        {
            public ATMJournalSyncState()
            {
            public string ATMId { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string EjPattern { get; set; }
            public string LogPattern { get; set; }
        }
    
        public partial public public class JournalSyncStateEnvelope
        {
            public JournalSyncStateEnvelope()
            {
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string EjPattern { get; set; }
            public string LogPattern { get; set; }
        }
    
        public partial public public class JournalSyncDashboardSnapshot
        {
            public JournalSyncDashboardSnapshot()
            {
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string EjPattern { get; set; }
            public string LogPattern { get; set; }
        }
    
        public partial public public sealed class VendorRootProfileSummary
        {
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string EjPattern { get; set; }
            public string LogPattern { get; set; }
        }
    
    }
    public enum JournalSyncState
        {
    
        public enum JournalSyncAlertSeverity
        {
    
        public partial public public class JournalEntry
        {
            public double CompressionRatio =>
            OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
            public string FileSizeDisplay =>
            OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
            OriginalSize > 1024 ? $"{OriginalSize / 1024.0:F1} KB" :
            $"{OriginalSize} B";
            public string EntryId
            {
            get;
            set;
            }
            public string MonthPartition
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public string ATMId
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public string FilePath
            {
            get;
            set;
            }
            public long OriginalSize
            {
            get;
            set;
            }
            public long CompressedSize
            {
            get;
            set;
            }
            public long EncryptedSize
            {
            get;
            set;
            }
            public bool IsEncrypted
            {
            get;
            set;
            }
            public bool IsCompressed
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string MD5Hash
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public int TransactionCount
            {
            get;
            set;
            }
            public string Status
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public DateTime ReceivedAt
            {
            get;
            set;
            }
            public DateTime CreatedAt
            {
            get => ReceivedAt;
            set => ReceivedAt = value;
            }
            public DateTime VerifiedAt
            {
            get;
            set;
            }
        }
    
        public partial public public class SyncStatusInfo
        {
            public string ProgressDisplay => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
            public string SpeedDisplay => $"{SyncSpeed:F1} KB/s";
            public string ETADisplay => EstimatedTimeRemaining < 60 ?
            $"{EstimatedTimeRemaining}ث" :
            $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
            public string SyncedSizeDisplay =>
            SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
            SyncedSize > 1024 ? $"{SyncedSize / 1024.0:F1} KB" :
            $"{SyncedSize} B";
            public string SyncId
            {
            get;
            set;
            }
            public DateTime? CompletedAt
            {
            get;
            set;
            }
            public string ATMId
            {
            get;
            set;
            }
            public string Status
            {
            get;
            set;
            }
            public int TotalFiles
            {
            get;
            set;
            }
            public int SyncedFiles
            {
            get;
            set;
            }
            public int FailedFiles
            {
            get;
            set;
            }
            public long TotalSize
            {
            get;
            set;
            }
            public long SyncedSize
            {
            get;
            set;
            }
            public int ProgressPercentage
            {
            get;
            set;
            }
            public double SyncSpeed
            {
            get;
            set;
            }
            public int EstimatedTimeRemaining
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public string FailureReason
            {
            get;
            set;
            }
            public DateTime StartedAt
            {
            get;
            set;
            }
            public DateTime LastUpdated
            {
            get;
            set;
            }
        }
    
        public partial public public class RemoteCommand
        {
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
            public string StatusIcon => Status
            switch
            {
            "Sent" => "SENT",
            "Received" => "RCVD",
            "Executed" => "OK",
            "Failed" => "FAIL",
            "Timeout" => "TIME",
            _ => "?"
            };
            public string DisplayLabel =>
            $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
            public string CommandId
            {
            get;
            set;
            }
            public int TimeoutSec
            {
            get;
            set;
            }
            public string TargetATMId
            {
            get;
            set;
            }
            public string CommandType
            {
            get;
            set;
            }
            public CommandParameters Parameters
            {
            get;
            set;
            }
            public string SentBy
            {
            get;
            set;
            }
            public bool RequireConfirm
            {
            get;
            set;
            }
            public DateTime SentAtUtc
            {
            get;
            set;
            }
            public DateTime ExecutedAt
            {
            get;
            set;
            }
            public string Status
            {
            get;
            set;
            }
            public string Result
            {
            get;
            set;
            }
            public DateTime? AckedAtUtc
            {
            get;
            set;
            }
            public string GetCommandDescription() =>
        }
    
        public partial public public class RemoteCommandType
        {
            public
            const string Restart = "Restart";
            public
            const string Screenshot = "Screenshot";
            public
            const string SyncTime = "SyncTime";
            public
            const string StartSync = "StartSync";
            public
            const string StopSync = "StopSync";
            public
            const string GetStatus = "GetStatus";
            public
            const string Reboot = "Reboot";
            public
            const string Shutdown = "Shutdown";
        }
    
        public partial public public class CommandParameters : Dictionary<string, string>
        {
            public string Raw
            {
            get;
            set;
            }
            public override string ToString() =>
        }
    
        public partial public public class JournalDailyStats
        {
            public double SuccessRate =>
            TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
            public string ATMId
            {
            get;
            set;
            }
            public double SyncSuccessPercent
            {
            get;
            set;
            }
            public DateTime Date
            {
            get;
            set;
            }
            public int TotalTransactions
            {
            get;
            set;
            }
            public int ApprovedTransactions
            {
            get;
            set;
            }
            public int FailedTransactions
            {
            get;
            set;
            }
            public int CardsCaptured
            {
            get;
            set;
            }
            public long CashDispensed
            {
            get;
            set;
            }
            public long JournalBytesReceived
            {
            get;
            set;
            }
            public double UptimePercent
            {
            get;
            set;
            }
        }
    
        public partial public public class LiveSyncProgress
        {
            public string BytesSentDisplay => BytesSent > 1048576 ?
            $"{BytesSent / 1048576.0:F1} MB" :
            $"{BytesSent / 1024.0:F1} KB";
            public string TotalBytesDisplay => TotalBytes > 1048576 ?
            $"{TotalBytes / 1048576.0:F1} MB" :
            $"{TotalBytes / 1024.0:F1} KB";
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            public string ATMId
            {
            get;
            set;
            }
            public DateTime UpdatedAt
            {
            get;
            set;
            }
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? StartedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public DateTime? LastAttemptAtUtc
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public DateTime? LastHeartbeatUtc
            {
            get;
            set;
            }
            public DateTime? LastJournalSyncUtc
            {
            get;
            set;
            }
            public int PendingFiles
            {
            get;
            set;
            }
            public int SyncingFiles
            {
            get;
            set;
            }
            public int FailedFiles
            {
            get;
            set;
            }
            public int CompletedFiles
            {
            get;
            set;
            }
            public long PendingBytes
            {
            get;
            set;
            }
            public string LastError
            {
            get;
            set;
            }
            public string AlertId
            {
            get;
            set;
            }
            public JournalSyncAlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string RecommendedAction
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public VendorRootProfileSummary RootProfile
            {
            get;
            set;
            }
            public List<JournalSyncAlert> Alerts
            {
            get;
            set;
            }
            public enum JournalSyncState
            {
            Pending,
            Syncing,
            Completed,
            Failed,
            ReSyncing,
            Archived
            }
            public enum JournalSyncAlertSeverity
            {
            Info,
            Warning,
            Critical
            }
            public string StateLabel
            {
            get;
            set;
            }
            public string StateIcon
            {
            get;
            set;
            }
            public int Percent
            {
            get;
            set;
            }
            public long BytesSent
            {
            get;
            set;
            }
            public long TotalBytes
            {
            get;
            set;
            }
            public double SpeedKBs
            {
            get;
            set;
            }
            public int SeqNum
            {
            get;
            set;
            }
            public int TotalChunks
            {
            get;
            set;
            }
        }
    
        public partial public public sealed class JournalSyncRecord
        {
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? StartedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public DateTime? LastAttemptAtUtc
            {
            get;
            set;
            }
        }
    
        public partial public public sealed class JournalSyncStatusSnapshot
        {
            public string ATM_ID
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public DateTime? LastHeartbeatUtc
            {
            get;
            set;
            }
            public DateTime? LastJournalSyncUtc
            {
            get;
            set;
            }
            public int PendingFiles
            {
            get;
            set;
            }
            public int SyncingFiles
            {
            get;
            set;
            }
            public int FailedFiles
            {
            get;
            set;
            }
            public int CompletedFiles
            {
            get;
            set;
            }
            public long PendingBytes
            {
            get;
            set;
            }
            public string LastError
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
        }
    
        public partial public public sealed class JournalSyncAlert
        {
            public string AlertId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public JournalSyncAlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public string RecommendedAction
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
        }
    
        public partial public public sealed class JournalSyncDashboardItem
        {
            public string ATM_ID
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public DateTime? LastHeartbeatUtc
            {
            get;
            set;
            }
            public DateTime? LastJournalSyncUtc
            {
            get;
            set;
            }
            public int PendingFiles
            {
            get;
            set;
            }
            public int FailedFiles
            {
            get;
            set;
            }
            public int CompletedFiles
            {
            get;
            set;
            }
            public long PendingBytes
            {
            get;
            set;
            }
            public VendorRootProfileSummary RootProfile
            {
            get;
            set;
            }
            public List<JournalSyncAlert> Alerts
            {
            get;
            set;
            }
        }
    
    }
    public enum JournalSyncState
        {
    
        public enum JournalSyncAlertSeverity
        {
    
        public partial public class JournalEntry
        {
            public double CompressionRatio =>
            OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
            public string FileSizeDisplay =>
            OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
            OriginalSize > 1024 ? $"{OriginalSize / 1024.0:F1} KB" :
            $"{OriginalSize} B";
            public string EntryId
            {
            get;
            set;
            }
            public string MonthPartition
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public string ATMId
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public string FilePath
            {
            get;
            set;
            }
            public long OriginalSize
            {
            get;
            set;
            }
            public long CompressedSize
            {
            get;
            set;
            }
            public long EncryptedSize
            {
            get;
            set;
            }
            public bool IsEncrypted
            {
            get;
            set;
            }
            public bool IsCompressed
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string MD5Hash
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public int TransactionCount
            {
            get;
            set;
            }
            public string Status
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public DateTime ReceivedAt
            {
            get;
            set;
            }
            public DateTime CreatedAt
            {
            get => ReceivedAt;
            set => ReceivedAt = value;
            }
            public DateTime VerifiedAt
            {
            get;
            set;
            }
        }
    
        public partial public class SyncStatusInfo
        {
            public string ProgressDisplay => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
            public string SpeedDisplay => $"{SyncSpeed:F1} KB/s";
            public string ETADisplay => EstimatedTimeRemaining < 60 ?
            $"{EstimatedTimeRemaining}ث" :
            $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
            public string SyncedSizeDisplay =>
            SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
            SyncedSize > 1024 ? $"{SyncedSize / 1024.0:F1} KB" :
            $"{SyncedSize} B";
            public string SyncId
            {
            get;
            set;
            }
            public DateTime? CompletedAt
            {
            get;
            set;
            }
            public string ATMId
            {
            get;
            set;
            }
            public string Status
            {
            get;
            set;
            }
            public int TotalFiles
            {
            get;
            set;
            }
            public int SyncedFiles
            {
            get;
            set;
            }
            public int FailedFiles
            {
            get;
            set;
            }
            public long TotalSize
            {
            get;
            set;
            }
            public long SyncedSize
            {
            get;
            set;
            }
            public int ProgressPercentage
            {
            get;
            set;
            }
            public double SyncSpeed
            {
            get;
            set;
            }
            public int EstimatedTimeRemaining
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public string FailureReason
            {
            get;
            set;
            }
            public DateTime StartedAt
            {
            get;
            set;
            }
            public DateTime LastUpdated
            {
            get;
            set;
            }
        }
    
        public partial public class RemoteCommand
        {
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
            public string StatusIcon => Status
            switch
            {
            "Sent" => "SENT",
            "Received" => "RCVD",
            "Executed" => "OK",
            "Failed" => "FAIL",
            "Timeout" => "TIME",
            _ => "?"
            };
            public string DisplayLabel =>
            $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
            public string CommandId
            {
            get;
            set;
            }
            public int TimeoutSec
            {
            get;
            set;
            }
            public string TargetATMId
            {
            get;
            set;
            }
            public string CommandType
            {
            get;
            set;
            }
            public CommandParameters Parameters
            {
            get;
            set;
            }
            public string SentBy
            {
            get;
            set;
            }
            public bool RequireConfirm
            {
            get;
            set;
            }
            public DateTime SentAtUtc
            {
            get;
            set;
            }
            public DateTime ExecutedAt
            {
            get;
            set;
            }
            public string Status
            {
            get;
            set;
            }
            public string Result
            {
            get;
            set;
            }
            public DateTime? AckedAtUtc
            {
            get;
            set;
            }
            public string GetCommandDescription() =>
        }
    
        public partial public class RemoteCommandType
        {
            public
            const string Restart = "Restart";
            public
            const string Screenshot = "Screenshot";
            public
            const string SyncTime = "SyncTime";
            public
            const string StartSync = "StartSync";
            public
            const string StopSync = "StopSync";
            public
            const string GetStatus = "GetStatus";
            public
            const string Reboot = "Reboot";
            public
            const string Shutdown = "Shutdown";
        }
    
        public partial public class CommandParameters : Dictionary<string, string>
        {
            public string Raw
            {
            get;
            set;
            }
            public override string ToString() =>
        }
    
        public partial public class JournalDailyStats
        {
            public double SuccessRate =>
            TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
            public string ATMId
            {
            get;
            set;
            }
            public double SyncSuccessPercent
            {
            get;
            set;
            }
            public DateTime Date
            {
            get;
            set;
            }
            public int TotalTransactions
            {
            get;
            set;
            }
            public int ApprovedTransactions
            {
            get;
            set;
            }
            public int FailedTransactions
            {
            get;
            set;
            }
            public int CardsCaptured
            {
            get;
            set;
            }
            public long CashDispensed
            {
            get;
            set;
            }
            public long JournalBytesReceived
            {
            get;
            set;
            }
            public double UptimePercent
            {
            get;
            set;
            }
        }
    
        public partial public class LiveSyncProgress
        {
            public string BytesSentDisplay => BytesSent > 1048576 ?
            $"{BytesSent / 1048576.0:F1} MB" :
            $"{BytesSent / 1024.0:F1} KB";
            public string TotalBytesDisplay => TotalBytes > 1048576 ?
            $"{TotalBytes / 1048576.0:F1} MB" :
            $"{TotalBytes / 1024.0:F1} KB";
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            public string ATMId
            {
            get;
            set;
            }
            public DateTime UpdatedAt
            {
            get;
            set;
            }
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? StartedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public DateTime? LastAttemptAtUtc
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public DateTime? LastHeartbeatUtc
            {
            get;
            set;
            }
            public DateTime? LastJournalSyncUtc
            {
            get;
            set;
            }
            public int PendingFiles
            {
            get;
            set;
            }
            public int SyncingFiles
            {
            get;
            set;
            }
            public int FailedFiles
            {
            get;
            set;
            }
            public int CompletedFiles
            {
            get;
            set;
            }
            public long PendingBytes
            {
            get;
            set;
            }
            public string LastError
            {
            get;
            set;
            }
            public string AlertId
            {
            get;
            set;
            }
            public JournalSyncAlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string RecommendedAction
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public VendorRootProfileSummary RootProfile
            {
            get;
            set;
            }
            public List<JournalSyncAlert> Alerts
            {
            get;
            set;
            }
            public enum JournalSyncState
            {
            Pending,
            Syncing,
            Completed,
            Failed,
            ReSyncing,
            Archived
            }
            public enum JournalSyncAlertSeverity
            {
            Info,
            Warning,
            Critical
            }
            public string StateLabel
            {
            get;
            set;
            }
            public string StateIcon
            {
            get;
            set;
            }
            public int Percent
            {
            get;
            set;
            }
            public long BytesSent
            {
            get;
            set;
            }
            public long TotalBytes
            {
            get;
            set;
            }
            public double SpeedKBs
            {
            get;
            set;
            }
            public int SeqNum
            {
            get;
            set;
            }
            public int TotalChunks
            {
            get;
            set;
            }
        }
    
        public partial public sealed class JournalSyncRecord
        {
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            public string SyncId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public string FileName
            {
            get;
            set;
            }
            public string LocalPath
            {
            get;
            set;
            }
            public string ArchivePath
            {
            get;
            set;
            }
            public string Checksum
            {
            get;
            set;
            }
            public string SHA256Hash
            {
            get;
            set;
            }
            public long FileSize
            {
            get;
            set;
            }
            public long FileOffset
            {
            get;
            set;
            }
            public int RetryCount
            {
            get;
            set;
            }
            public int ProgressPercent
            {
            get;
            set;
            }
            public JournalSyncState State
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
            public DateTime? StartedAtUtc
            {
            get;
            set;
            }
            public DateTime? CompletedAtUtc
            {
            get;
            set;
            }
            public DateTime? LastAttemptAtUtc
            {
            get;
            set;
            }
        }
    
        public partial public sealed class JournalSyncStatusSnapshot
        {
            public string ATM_ID
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public DateTime? LastHeartbeatUtc
            {
            get;
            set;
            }
            public DateTime? LastJournalSyncUtc
            {
            get;
            set;
            }
            public int PendingFiles
            {
            get;
            set;
            }
            public int SyncingFiles
            {
            get;
            set;
            }
            public int FailedFiles
            {
            get;
            set;
            }
            public int CompletedFiles
            {
            get;
            set;
            }
            public long PendingBytes
            {
            get;
            set;
            }
            public string LastError
            {
            get;
            set;
            }
            public DateTime UpdatedAtUtc
            {
            get;
            set;
            }
        }
    
        public partial public sealed class JournalSyncAlert
        {
            public string AlertId
            {
            get;
            set;
            }
            public string ATM_ID
            {
            get;
            set;
            }
            public JournalSyncAlertSeverity Severity
            {
            get;
            set;
            }
            public string Title
            {
            get;
            set;
            }
            public string Message
            {
            get;
            set;
            }
            public string RecommendedAction
            {
            get;
            set;
            }
            public DateTime CreatedAtUtc
            {
            get;
            set;
            }
        }
    
        public partial public sealed class JournalSyncDashboardItem
        {
            public string ATM_ID
            {
            get;
            set;
            }
            public string ATM_Name
            {
            get;
            set;
            }
            public string ATM_Type
            {
            get;
            set;
            }
            public bool IsConnected
            {
            get;
            set;
            }
            public DateTime? LastHeartbeatUtc
            {
            get;
            set;
            }
            public DateTime? LastJournalSyncUtc
            {
            get;
            set;
            }
            public int PendingFiles
            {
            get;
            set;
            }
            public int FailedFiles
            {
            get;
            set;
            }
            public int CompletedFiles
            {
            get;
            set;
            }
            public long PendingBytes
            {
            get;
            set;
            }
            public VendorRootProfileSummary RootProfile
            {
            get;
            set;
            }
            public List<JournalSyncAlert> Alerts
            {
            get;
            set;
            }
        }
    
    }
    public enum JournalSyncState
        {
    
        public enum JournalSyncAlertSeverity
        {
    
        [Serializable]
        public enum JournalSyncStatus
        {
    
        [Serializable]
        public enum JournalTransferStatus
        {
    
        public partial public class JournalEntry
        {
            public double CompressionRatio =>
            OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
            public string FileSizeDisplay =>
            OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
            OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
            $"{OriginalSize} B";
            public string   EntryId          { get; set; }
            public string   ATMId            { get; set; }
            public string   FileName         { get; set; }
            public string   FilePath         { get; set; }
            public long     OriginalSize     { get; set; }
            public long     CompressedSize   { get; set; }
            public long     EncryptedSize    { get; set; }
            public bool     IsEncrypted      { get; set; }
            public bool     IsCompressed     { get; set; }
            public string   Checksum         { get; set; }
            public string   MD5Hash          { get; set; }
            public string   SHA256Hash       { get; set; }
            public int      TransactionCount { get; set; }
            public string   Status           { get; set; }
            public long     FileOffset       { get; set; }
            public string   ArchivePath      { get; set; }
            public DateTime ReceivedAt       { get; set; }
            public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
            public DateTime VerifiedAt       { get; set; }
            public string   MonthPartition   { get; set; }
        }
    
        public partial public class SyncStatusInfo
        {
            public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
            public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
            public string ETADisplay        => EstimatedTimeRemaining < 60
            ? $"{EstimatedTimeRemaining}ث"
            : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
            public string SyncedSizeDisplay =>
            SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
            SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
            $"{SyncedSize} B";
            public string   SyncId                  { get; set; }
            public string   ATMId                   { get; set; }
            public string   Status                  { get; set; }
            public int      TotalFiles              { get; set; }
            public int      SyncedFiles             { get; set; }
            public int      FailedFiles             { get; set; }
            public long     TotalSize               { get; set; }
            public long     SyncedSize              { get; set; }
            public int      ProgressPercentage      { get; set; }
            public double   SyncSpeed               { get; set; }
            public int      EstimatedTimeRemaining  { get; set; }
            public int      RetryCount              { get; set; }
            public string   FailureReason           { get; set; }
            public DateTime StartedAt               { get; set; }
            public DateTime LastUpdated             { get; set; }
            public DateTime? CompletedAt            { get; set; }
            public string FileName { get; set; }
            public JournalSyncState State { get; set; }
            public int ProgressPercent { get; set; }
            public string Message { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
        }
    
        public partial public class RemoteCommand
        {
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
            public string StatusIcon => Status switch
            {
            "Sent"      => "SENT",
            "Received"  => "RCVD",
            "Executed"  => "OK",
            "Failed"    => "FAIL",
            "Timeout"   => "TIME",
            _           => "?"
            };
            public string DisplayLabel =>
            $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
            public RemoteCommand()
            {
            public string   CommandId     { get; set; }
            public string   CommandType   { get; set; }
            public string   TargetATMId   { get; set; }
            public CommandParameters Parameters { get; set; }
            public string   SentBy        { get; set; }
            public bool     RequireConfirm { get; set; }
            public DateTime SentAtUtc     { get; set; }
            public DateTime ExecutedAt    { get; set; }
            public string   Status        { get; set; }
            public string   Result        { get; set; }
            public DateTime? AckedAtUtc   { get; set; }
            public int      TimeoutSec    { get; set; }
            public string   Parameters    { get; set; }
            public string ATMId { get; set; }
            public RemoteCommandType CommandType { get; set; }
            public Dictionary<string, string> Parameters { get; private set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ExecutedAt { get; set; }
            public string GetCommandDescription() =>
        }
    
        public partial public static class RemoteCommandType
        {
            public const string Restart = "Restart";
            public const string Screenshot = "Screenshot";
            public const string SyncTime = "SyncTime";
            public const string StartSync = "StartSync";
            public const string StopSync = "StopSync";
            public const string GetStatus = "GetStatus";
            public const string Reboot = "Reboot";
            public const string Shutdown = "Shutdown";
        }
    
        public partial public class CommandParameters : Dictionary<string, string>
        {
            public string Raw { get; set; }
            public override string ToString() =>
        }
    
        public partial public class JournalDailyStats
        {
            public double SuccessRate =>
            TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
            public string   ATMId                { get; set; }
            public DateTime Date                 { get; set; }
            public int      TotalTransactions    { get; set; }
            public int      ApprovedTransactions { get; set; }
            public int      FailedTransactions   { get; set; }
            public int      CardsCaptured        { get; set; }
            public long     CashDispensed        { get; set; }
            public long     JournalBytesReceived { get; set; }
            public double   UptimePercent        { get; set; }
            public double   SyncSuccessPercent   { get; set; }
        }
    
        public partial public class LiveSyncProgress
        {
            public string BytesSentDisplay => BytesSent > 1048576
            ? $"{BytesSent / 1048576.0:F1} MB"
            : $"{BytesSent / 1024.0:F1} KB";
            public string TotalBytesDisplay => TotalBytes > 1048576
            ? $"{TotalBytes / 1048576.0:F1} MB"
            : $"{TotalBytes / 1024.0:F1} KB";
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            public string ATMId           { get; set; }
            public string FileName        { get; set; }
            public string StateLabel      { get; set; }
            public string StateIcon       { get; set; }
            public int    Percent         { get; set; }
            public long   BytesSent       { get; set; }
            public long   TotalBytes      { get; set; }
            public double SpeedKBs        { get; set; }
            public int    SeqNum          { get; set; }
            public int    TotalChunks     { get; set; }
            public DateTime UpdatedAt     { get; set; }
            public enum JournalSyncState
            {
            Pending,
            Syncing,
            Completed,
            Failed,
            ReSyncing,
            Archived
            }
            public enum JournalSyncAlertSeverity
            {
            Info,
            Warning,
            Critical
            }
            public string SyncId { get; set; }
            public string ATM_ID { get; set; }
            public string LocalPath { get; set; }
            public string ArchivePath { get; set; }
            public string Checksum { get; set; }
            public string SHA256Hash { get; set; }
            public long FileSize { get; set; }
            public long FileOffset { get; set; }
            public int RetryCount { get; set; }
            public int ProgressPercent { get; set; }
            public JournalSyncState State { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public DateTime? StartedAtUtc { get; set; }
            public DateTime? CompletedAtUtc { get; set; }
            public DateTime? LastAttemptAtUtc { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int SyncingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public string LastError { get; set; }
            public string AlertId { get; set; }
            public JournalSyncAlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string RecommendedAction { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public VendorRootProfileSummary RootProfile { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
        }
    
        public partial public sealed class JournalSyncRecord
        {
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            public JournalSyncRecord()
            {
            public string SyncId { get; set; }
            public string ATM_ID { get; set; }
            public string FileName { get; set; }
            public string LocalPath { get; set; }
            public string ArchivePath { get; set; }
            public string Checksum { get; set; }
            public string SHA256Hash { get; set; }
            public long FileSize { get; set; }
            public long FileOffset { get; set; }
            public int RetryCount { get; set; }
            public int ProgressPercent { get; set; }
            public JournalSyncState State { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
            public DateTime? StartedAtUtc { get; set; }
            public DateTime? CompletedAtUtc { get; set; }
            public DateTime? LastAttemptAtUtc { get; set; }
            public string ServerPath { get; set; }
        }
    
        public partial public sealed class JournalSyncStatusSnapshot
        {
            public string ATM_ID { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int SyncingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public string LastError { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
        }
    
        public partial public sealed class JournalSyncAlert
        {
            public string AlertId { get; set; }
            public string ATM_ID { get; set; }
            public JournalSyncAlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string RecommendedAction { get; set; }
            public DateTime CreatedAtUtc { get; set; }
        }
    
        public partial public sealed class JournalSyncDashboardItem
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public VendorRootProfileSummary RootProfile { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
        }
    
        public partial public class JournalSyncModels
        {
        }
    
        public partial public class JournalSyncEntry
        {
            public JournalSyncEntry()
            {
            public string FileName { get; set; }
            public string EntryId { get; set; }
            public string ATMId { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public string Checksum { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
        }
    
        public partial public class ATMJournalSyncState
        {
            public ATMJournalSyncState()
            {
            public string ATMId { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
        }
    
        public partial public class JournalSyncStateEnvelope
        {
            public JournalSyncStateEnvelope()
            {
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
        }
    
        public partial public class JournalSyncDashboardSnapshot
        {
            public JournalSyncDashboardSnapshot()
            {
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
        public partial public sealed class VendorRootProfileSummary
        {
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string EjPattern { get; set; }
            public string LogPattern { get; set; }
        }
    
    }
    public partial class RemoteCommandType
        {
            public
            const string Restart = "Restart";
    
    
            public
            const string Screenshot = "Screenshot";
    
    
            public
            const string SyncTime = "SyncTime";
    
    
            public
            const string StartSync = "StartSync";
    
    
            public
            const string StopSync = "StopSync";
    
    
            public
            const string GetStatus = "GetStatus";
    
    
            public
            const string Reboot = "Reboot";
    
    
            public
            const string Shutdown = "Shutdown";
    
    
        }
    // Class: RemoteCommandType (from 1 sources)
        public static partial class RemoteCommandType
        {
            // --- Constants & Fields ---
                    public const string Restart = "Restart";
    
                    public const string Screenshot = "Screenshot";
    
                    public const string SyncTime = "SyncTime";
    
                    public const string StartSync = "StartSync";
    
                    public const string StopSync = "StopSync";
    
                    public const string GetStatus = "GetStatus";
    
                    public const string Reboot = "Reboot";
    
                    public const string Shutdown = "Shutdown";
    
    
        }
    // Class: RemoteCommandType (from 3 sources)
        public static partial class RemoteCommandType
        {
            // --- Constants & Fields ---
                    public const string Restart = "Restart";
    
                    public const string Screenshot = "Screenshot";
    
                    public const string SyncTime = "SyncTime";
    
                    public const string StartSync = "StartSync";
    
                    public const string StopSync = "StopSync";
    
                    public const string GetStatus = "GetStatus";
    
                    public const string Reboot = "Reboot";
    
                    public const string Shutdown = "Shutdown";
    
    
        }
    // Class: RemoteCommandType (from 2 sources)
        public static partial class RemoteCommandType
        {
            // --- Constants & Fields ---
                    public const string Restart    = "Restart";
    
                    public const string Screenshot = "Screenshot";
    
                    public const string SyncTime   = "SyncTime";
    
                    public const string StartSync  = "StartSync";
    
                    public const string StopSync   = "StopSync";
    
                    public const string GetStatus  = "GetStatus";
    
                    public const string Reboot     = "Reboot";
    
                    public const string Shutdown   = "Shutdown";
    
    
        }
    // Class: RemoteCommandType (from 9 sources)
        public static partial class RemoteCommandType
        {
            // --- Constants & Fields ---
                            public const string Restart = "Restart";
    
                            public const string Screenshot = "Screenshot";
    
                            public const string SyncTime = "SyncTime";
    
                            public const string StartSync = "StartSync";
    
                            public const string StopSync = "StopSync";
    
                            public const string GetStatus = "GetStatus";
    
                            public const string Reboot = "Reboot";
    
                            public const string Shutdown = "Shutdown";
    
    
        }
    // Class: RemoteCommandType (from 5 sources)
        public static partial class RemoteCommandType
        {
            // --- Constants & Fields ---
                    public const string Restart = "Restart";
    
                    public const string Screenshot = "Screenshot";
    
                    public const string SyncTime = "SyncTime";
    
                    public const string StartSync = "StartSync";
    
                    public const string StopSync = "StopSync";
    
                    public const string GetStatus = "GetStatus";
    
                    public const string Reboot = "Reboot";
    
                    public const string Shutdown = "Shutdown";
    
    
        }
    public static class RemoteCommandType
        {
            public const string Restart = "Restart";
            public const string Screenshot = "Screenshot";
            public const string SyncTime = "SyncTime";
            public const string StartSync = "StartSync";
            public const string StopSync = "StopSync";
            public const string GetStatus = "GetStatus";
            public const string Reboot = "Reboot";
            public const string Shutdown = "Shutdown";
        }

    public partial class ATMJournalSyncState
        {
            public string ATMId { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public DateTime LastHeartbeatUtc { get; set; }
    
    
            public DateTime LastJournalReceivedUtc { get; set; }
    
    
            public DateTime LastSuccessfulSyncUtc { get; set; }
    
    
            public long TotalJournalBytesReceived { get; set; }
    
    
            public int TotalJournalFilesReceived { get; set; }
    
    
            public int FailedSyncCount { get; set; }
    
    
            public int PendingArchiveCount { get; set; }
    
    
            public string LastStoredFile { get; set; }
    
    
            public string LastChecksum { get; set; }
    
    
            public string LastError { get; set; }
    
    
            public JournalSyncStatus CurrentStatus { get; set; }
    
    
            public ATMJournalSyncState()
            {
                ATMId = "Unknown";
                CurrentStatus = JournalSyncStatus.Unknown;
            }
    
    
        }
    [Serializable]
        public class ATMJournalSyncState
        {
            public string ATMId { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
    
            public ATMJournalSyncState()
            {
                ATMId = "Unknown";
                CurrentStatus = JournalSyncStatus.Unknown;
            }
        }
    public partial class CommandParameters : Dictionary<string, string>
        {
            public string Raw
            {
                get;
                set;
            } = string.Empty;
    
    
            public string Raw { get; set; } = string.Empty;
    
    
            public static implicit operator CommandParameters(string value)
            {
                return new CommandParameters
                {
                    Raw = value ?? string.Empty
                };
            }
    
    
            public override string ToString() => Raw;
    
    
        }
    public partial class CommandParameters : Dictionary<string, string>
        {
            public string Raw { get; set; } = string.Empty;
    
    
            public static implicit operator CommandParameters(string value)
            {
                return new CommandParameters { Raw = value ?? string.Empty };
            }
    
    
            public override string ToString() => Raw;
    
    
        }
    public class CommandParameters : Dictionary<string, string>
        {
            public string Raw { get; set; } = string.Empty;
    
            public static implicit operator CommandParameters(string value)
            {
                return new CommandParameters { Raw = value ?? string.Empty };
            }
    
            public override string ToString() => Raw;
        }
    public partial class JournalDailyStats
        {
            public string ATMId
            {
                get;
                set;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public double SyncSuccessPercent
            {
                get;
                set;
            } = 100.0;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public double SyncSuccessPercent
            {
                get;
                set;
            } = 100.0;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public double SyncSuccessPercent
            {
                get;
                set;
            } = 100.0;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public double SyncSuccessPercent
            {
                get;
                set;
            } = 100.0;
    
    
            public string   ATMId                { get; set; }
    
    
            public DateTime Date                 { get; set; }
    
    
            public int      TotalTransactions    { get; set; }
    
    
            public int      ApprovedTransactions { get; set; }
    
    
            public int      FailedTransactions   { get; set; }
    
    
            public int      CardsCaptured        { get; set; }
    
    
            public long     CashDispensed        { get; set; }
    
    
            public long     JournalBytesReceived { get; set; }
    
    
            public double   UptimePercent        { get; set; } = 100.0;
    
    
            public double   SyncSuccessPercent   { get; set; } = 100.0;
    
    
            public double SuccessRate =>
                TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
    
    
        }
    public partial class JournalDailyStats
        {
            public string   ATMId                { get; set; }
    
    
            public DateTime Date                 { get; set; }
    
    
            public int      TotalTransactions    { get; set; }
    
    
            public int      ApprovedTransactions { get; set; }
    
    
            public int      FailedTransactions   { get; set; }
    
    
            public int      CardsCaptured        { get; set; }
    
    
            public long     CashDispensed        { get; set; }
    
    
            public long     JournalBytesReceived { get; set; }
    
    
            public double   UptimePercent        { get; set; } = 100.0;
    
    
            public double   SyncSuccessPercent   { get; set; } = 100.0;
    
    
            public double SuccessRate =>
                TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
    
    
        }
    public partial class JournalDailyStats
        {
            public string ATMId
            {
                get;
                set;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public double SyncSuccessPercent
            {
                get;
                set;
            } = 100.0;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public double SyncSuccessPercent
            {
                get;
                set;
            } = 100.0;
    
    
            public double SuccessRate =>
                TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
    
    
        }
    // ==========================================
        // إحصائيات الجورنال اليومية
        // ==========================================
    
        public class JournalDailyStats
        {
            public string   ATMId                { get; set; }
            public DateTime Date                 { get; set; }
            public int      TotalTransactions    { get; set; }
            public int      ApprovedTransactions { get; set; }
            public int      FailedTransactions   { get; set; }
            public int      CardsCaptured        { get; set; }
            public long     CashDispensed        { get; set; }
            public long     JournalBytesReceived { get; set; }
            public double   UptimePercent        { get; set; } = 100.0;
            public double   SyncSuccessPercent   { get; set; } = 100.0;
    
            public double SuccessRate =>
                TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
        }
    public partial class JournalDailyStats
        {
            public string ATMId
            {
                get;
                set;
            }
            public double SyncSuccessPercent
            {
                get;
                set;
            } = 100.0;
            public string   ATMId                { get; set; }
            public DateTime Date                 { get; set; }
            public int      TotalTransactions    { get; set; }
            public int      ApprovedTransactions { get; set; }
            public int      FailedTransactions   { get; set; }
            public int      CardsCaptured        { get; set; }
            public long     CashDispensed        { get; set; }
            public long     JournalBytesReceived { get; set; }
            public double   UptimePercent        { get; set; } = 100.0;
            public double   SyncSuccessPercent   { get; set; } = 100.0;
            public double SuccessRate =>
                TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
        }
    public partial class JournalEntry
        {
            public string EntryId
            {
                get;
                set;
            } = Guid.NewGuid().ToString("N");
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string MonthPartition
            {
                get;
                set;
            } // YYYY-MM
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public string MonthPartition
            {
                get;
                set;
            } // YYYY-MM
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string MonthPartition
            {
                get;
                set;
            } // YYYY-MM
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string MonthPartition
            {
                get;
                set;
            } // YYYY-MM
    
    
            public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string   ATMId            { get; set; }
    
    
            public string   FileName         { get; set; }
    
    
            public string   FilePath         { get; set; }
    
    
            public long     OriginalSize     { get; set; }
    
    
            public long     CompressedSize   { get; set; }
    
    
            public long     EncryptedSize    { get; set; }
    
    
            public bool     IsEncrypted      { get; set; } = true;
    
    
            public bool     IsCompressed     { get; set; } = true;
    
    
            public string   Checksum         { get; set; }    // MD5
    
    
            public string   MD5Hash          { get; set; }
    
    
            public string   SHA256Hash       { get; set; }
    
    
            public int      TransactionCount { get; set; }
    
    
            public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
    
            public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
    
            public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
    
            public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
    
            public DateTime VerifiedAt       { get; set; }
    
    
            public string   MonthPartition   { get; set; }    // YYYY-MM
    
    
            public double CompressionRatio =>
                OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
    
            public string FileSizeDisplay =>
                OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                $"{OriginalSize} B";
    
    
            public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime CreatedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime CreatedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public DateTime CreatedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime CreatedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime CreatedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime CreatedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime CreatedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime CreatedAt { get; set; }
    
    
        }
    public partial class JournalEntry
        {
            public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string   ATMId            { get; set; }
    
    
            public string   FileName         { get; set; }
    
    
            public string   FilePath         { get; set; }
    
    
            public long     OriginalSize     { get; set; }
    
    
            public long     CompressedSize   { get; set; }
    
    
            public long     EncryptedSize    { get; set; }
    
    
            public bool     IsEncrypted      { get; set; } = true;
    
    
            public bool     IsCompressed     { get; set; } = true;
    
    
            public string   Checksum         { get; set; }    // MD5
    
    
            public string   MD5Hash          { get; set; }
    
    
            public string   SHA256Hash       { get; set; }
    
    
            public int      TransactionCount { get; set; }
    
    
            public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
    
            public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
    
            public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
    
            public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
    
            public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
    
    
            public DateTime VerifiedAt       { get; set; }
    
    
            public string   MonthPartition   { get; set; }    // YYYY-MM
    
    
            public double CompressionRatio =>
                OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
    
            public string FileSizeDisplay =>
                OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                $"{OriginalSize} B";
    
    
        }
    public partial class JournalEntry
        {
            public string ATMId { get; set; }
    
    
            public string FileName { get; set; }
    
    
            public long OriginalSize { get; set; }
    
    
            public long CompressedSize { get; set; }
    
    
            public long EncryptedSize { get; set; }
    
    
            public bool IsEncrypted { get; set; }
    
    
            public bool IsCompressed { get; set; }
    
    
            public string Checksum { get; set; }
    
    
            public string MD5Hash { get; set; }
    
    
            public DateTime CreatedAt { get; set; }
    
    
            public DateTime ReceivedAt { get; set; }
    
    
            public string Status { get; set; }
    
    
            public int TransactionCount { get; set; }
    
    
        }
    public partial class JournalEntry
        {
            public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string   ATMId            { get; set; }
    
    
            public string   FileName         { get; set; }
    
    
            public string   FilePath         { get; set; }
    
    
            public long     OriginalSize     { get; set; }
    
    
            public long     CompressedSize   { get; set; }
    
    
            public long     EncryptedSize    { get; set; }
    
    
            public bool     IsEncrypted      { get; set; } = true;
    
    
            public bool     IsCompressed     { get; set; } = true;
    
    
            public string   Checksum         { get; set; }    // MD5
    
    
            public string   MD5Hash          { get; set; }
    
    
            public string   SHA256Hash       { get; set; }
    
    
            public int      TransactionCount { get; set; }
    
    
            public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
    
            public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
    
            public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
    
            public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
    
            public DateTime VerifiedAt       { get; set; }
    
    
            public string   MonthPartition   { get; set; }    // YYYY-MM
    
    
            public double CompressionRatio =>
                OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
    
            public string FileSizeDisplay =>
                OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                $"{OriginalSize} B";
    
    
        }
    public partial class JournalEntry
        {
            public string EntryId
            {
                get;
                set;
            } = Guid.NewGuid().ToString("N");
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string MonthPartition
            {
                get;
                set;
            } // YYYY-MM
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string MonthPartition
            {
                get;
                set;
            } // YYYY-MM
    
    
            public long FileOffset
            {
                get;
                set;
            } // آخر موضع (مهم لـ NCR)
    
    
            public double CompressionRatio =>
                OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
    
            public string FileSizeDisplay =>
                OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
            OriginalSize > 1024 ? $"{OriginalSize / 1024.0:F1} KB" :
            $"{OriginalSize} B";
    
    
        }
    /// <summary>
        /// نماذج الجورنال الكاملة: JournalEntry, SyncStatusInfo, RemoteCommand, SyncProgress
        /// </summary>
    
        // ==========================================
        // الجورنال الأرشيفي
        // ==========================================
    
        public class JournalEntry
        {
            public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
            public string   ATMId            { get; set; }
            public string   FileName         { get; set; }
            public string   FilePath         { get; set; }
            public long     OriginalSize     { get; set; }
            public long     CompressedSize   { get; set; }
            public long     EncryptedSize    { get; set; }
            public bool     IsEncrypted      { get; set; } = true;
            public bool     IsCompressed     { get; set; } = true;
            public string   Checksum         { get; set; }    // MD5
            public string   MD5Hash          { get; set; }
            public string   SHA256Hash       { get; set; }
            public int      TransactionCount { get; set; }
            public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
            public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
            public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
            public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
            public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
            public DateTime VerifiedAt       { get; set; }
            public string   MonthPartition   { get; set; }    // YYYY-MM
    
            public double CompressionRatio =>
                OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
            public string FileSizeDisplay =>
                OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                $"{OriginalSize} B";
        }
    /// <summary>
        /// نماذج الجورنال الكاملة: JournalEntry, SyncStatusInfo, RemoteCommand, SyncProgress
        /// </summary>
    
        // ==========================================
        // الجورنال الأرشيفي
        // ==========================================
    
        public class JournalEntry
        {
            public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
            public string   ATMId            { get; set; }
            public string   FileName         { get; set; }
            public string   FilePath         { get; set; }
            public long     OriginalSize     { get; set; }
            public long     CompressedSize   { get; set; }
            public long     EncryptedSize    { get; set; }
            public bool     IsEncrypted      { get; set; } = true;
            public bool     IsCompressed     { get; set; } = true;
            public string   Checksum         { get; set; }    // MD5
            public string   MD5Hash          { get; set; }
            public string   SHA256Hash       { get; set; }
            public int      TransactionCount { get; set; }
            public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
            public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
            public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
            public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
            public DateTime VerifiedAt       { get; set; }
            public string   MonthPartition   { get; set; }    // YYYY-MM
    
            public double CompressionRatio =>
                OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
            public string FileSizeDisplay =>
                OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                $"{OriginalSize} B";
        }
    public class JournalEntry
        {
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public long OriginalSize { get; set; }
            public long CompressedSize { get; set; }
            public long EncryptedSize { get; set; }
            public bool IsEncrypted { get; set; }
            public bool IsCompressed { get; set; }
            public string Checksum { get; set; }
            public string MD5Hash { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime ReceivedAt { get; set; }
            public string Status { get; set; }
            public int TransactionCount { get; set; }
        }
    public partial class JournalEntry
        {
            public string EntryId
            {
                get;
                set;
            } = Guid.NewGuid().ToString("N");
            public string MonthPartition
            {
                get;
                set;
            } // YYYY-MM
            public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
            public string   ATMId            { get; set; }
            public string   FileName         { get; set; }
            public string   FilePath         { get; set; }
            public long     OriginalSize     { get; set; }
            public long     CompressedSize   { get; set; }
            public long     EncryptedSize    { get; set; }
            public bool     IsEncrypted      { get; set; } = true;
            public bool     IsCompressed     { get; set; } = true;
            public string   Checksum         { get; set; }    // MD5
            public string   MD5Hash          { get; set; }
            public string   SHA256Hash       { get; set; }
            public int      TransactionCount { get; set; }
            public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
            public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
            public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
            public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
            public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
            public DateTime VerifiedAt       { get; set; }
            public string   MonthPartition   { get; set; }    // YYYY-MM
            public double CompressionRatio =>
                OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
            public string FileSizeDisplay =>
                OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                $"{OriginalSize} B";
            public DateTime CreatedAt { get; set; }
        }
    public partial class JournalSyncAlert
        {
            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string ATM_ID { get; set; }
    
    
            public JournalSyncAlertSeverity Severity { get; set; }
    
    
            public string Title { get; set; }
    
    
            public string Message { get; set; }
    
    
            public string RecommendedAction { get; set; }
    
    
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public string ATMId { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            public string Code { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-26\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-28\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v23_bak
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs.v17_bak
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs.v21_bak
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs.v23_bak
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            public JournalSyncAlert()
            {
                AlertId = Guid.NewGuid().ToString("N");
                CreatedAtUtc = DateTime.UtcNow;
                Severity = "Info";
                Code = "sync-info";
            }
    
    
        }
    public partial class JournalSyncAlert
        {
            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string ATM_ID { get; set; }
    
    
            public JournalSyncAlertSeverity Severity { get; set; }
    
    
            public string Title { get; set; }
    
    
            public string Message { get; set; }
    
    
            public string RecommendedAction { get; set; }
    
    
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public string ATMId { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string Severity { get; set; }
    
    
            public string Code { get; set; }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v23_bak
            public string Severity { get; set; }
    
    
            public JournalSyncAlert()
            {
                AlertId = Guid.NewGuid().ToString("N");
                CreatedAtUtc = DateTime.UtcNow;
                Severity = "Info";
                Code = "sync-info";
            }
    
    
        }
    public partial class JournalSyncAlert
        {
            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string ATM_ID { get; set; }
    
    
            public JournalSyncAlertSeverity Severity { get; set; }
    
    
            public string Title { get; set; }
    
    
            public string Message { get; set; }
    
    
            public string RecommendedAction { get; set; }
    
    
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    public sealed class JournalSyncAlert
        {
            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
            public string ATM_ID { get; set; }
            public JournalSyncAlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string RecommendedAction { get; set; }
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        }
    // ==========================================
        // تنبيه مزامنة الجورنال
        // ==========================================
    
        public sealed class JournalSyncAlert
        {
            public string                 AlertId           { get; set; } = Guid.NewGuid().ToString("N");
            public string                 ATM_ID            { get; set; }
            public JournalSyncAlertSeverity Severity        { get; set; }
            public string                 Title             { get; set; }
            public string                 Message           { get; set; }
            public string                 RecommendedAction { get; set; }
            public DateTime               CreatedAtUtc      { get; set; } = DateTime.UtcNow;
        }
    public partial class JournalSyncAlert
        {
            public string                 AlertId           { get; set; } = Guid.NewGuid().ToString("N");
            public string                 ATM_ID            { get; set; }
            public JournalSyncAlertSeverity Severity        { get; set; }
            public string                 Title             { get; set; }
            public string                 Message           { get; set; }
            public string                 RecommendedAction { get; set; }
            public DateTime               CreatedAtUtc      { get; set; } = DateTime.UtcNow;
            public string ATMId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public JournalSyncAlert()
            {
                AlertId = Guid.NewGuid().ToString("N");
                CreatedAtUtc = DateTime.UtcNow;
                Severity = "Info";
                Code = "sync-info";
            }
        }
    public partial class JournalSyncDashboardItem
        {
            public string ATM_ID { get; set; }
    
    
            public string ATM_Name { get; set; }
    
    
            public string ATM_Type { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public DateTime? LastHeartbeatUtc { get; set; }
    
    
            public DateTime? LastJournalSyncUtc { get; set; }
    
    
            public int PendingFiles { get; set; }
    
    
            public int FailedFiles { get; set; }
    
    
            public int CompletedFiles { get; set; }
    
    
            public long PendingBytes { get; set; }
    
    
            public VendorRootProfileSummary RootProfile { get; set; }
    
    
            public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
    
    
        }
    public sealed class JournalSyncDashboardItem
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public VendorRootProfileSummary RootProfile { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
        }
    // ==========================================
        // عنصر لوحة التحكم
        // ==========================================
    
        public sealed class JournalSyncDashboardItem
        {
            public string                  ATM_ID            { get; set; }
            public string                  ATM_Name          { get; set; }
            public string                  ATM_Type          { get; set; }
            public bool                    IsConnected       { get; set; }
            public DateTime?               LastHeartbeatUtc  { get; set; }
            public DateTime?               LastJournalSyncUtc { get; set; }
            public int                     PendingFiles      { get; set; }
            public int                     FailedFiles       { get; set; }
            public int                     CompletedFiles    { get; set; }
            public long                    PendingBytes      { get; set; }
            public VendorRootProfileSummary RootProfile      { get; set; }
            public List<JournalSyncAlert>  Alerts            { get; set; } = new List<JournalSyncAlert>();
        }
    public partial class JournalSyncDashboardSnapshot
        {
            public int TotalAtms { get; set; }
    
    
            public int ConnectedAtms { get; set; }
    
    
            public int WarningAtms { get; set; }
    
    
            public int CriticalAtms { get; set; }
    
    
            public int TotalTransfers { get; set; }
    
    
            public int FailedTransfers { get; set; }
    
    
            public List<ATMJournalSyncState> AtmStates { get; set; }
    
    
            public List<JournalSyncEntry> RecentTransfers { get; set; }
    
    
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
    
            public JournalSyncDashboardSnapshot()
            {
                AtmStates = new List<ATMJournalSyncState>();
                RecentTransfers = new List<JournalSyncEntry>();
                ActiveAlerts = new List<JournalSyncAlert>();
            }
    
    
        }
    public class JournalSyncDashboardSnapshot
        {
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
            public JournalSyncDashboardSnapshot()
            {
                AtmStates = new List<ATMJournalSyncState>();
                RecentTransfers = new List<JournalSyncEntry>();
                ActiveAlerts = new List<JournalSyncAlert>();
            }
        }
    public partial class JournalSyncEntry
        {
            public string EntryId { get; set; }
    
    
            public string ATMId { get; set; }
    
    
            public string FileName { get; set; }
    
    
            public string RelativeStoragePath { get; set; }
    
    
            public long FileSizeBytes { get; set; }
    
    
            public string Checksum { get; set; }
    
    
            public DateTime ReceivedAtUtc { get; set; }
    
    
            public JournalTransferStatus Status { get; set; }
    
    
            public string ErrorMessage { get; set; }
    
    
            public JournalSyncEntry()
            {
                EntryId = Guid.NewGuid().ToString("N");
                ReceivedAtUtc = DateTime.UtcNow;
                Status = JournalTransferStatus.Received;
            }
    
    
        }
    [Serializable]
        public class JournalSyncEntry
        {
            public string EntryId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public string Checksum { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
    
            public JournalSyncEntry()
            {
                EntryId = Guid.NewGuid().ToString("N");
                ReceivedAtUtc = DateTime.UtcNow;
                Status = JournalTransferStatus.Received;
            }
        }
    public class JournalSyncEntry { public string FileName { get; set; } public long Size { get; set; } public DateTime SyncedAt { get; set; } }
    public partial class JournalSyncRecord
        {
            public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string ATM_ID { get; set; }
    
    
            public string FileName { get; set; }
    
    
            public string LocalPath { get; set; }
    
    
            public string ArchivePath { get; set; }
    
    
            public string Checksum { get; set; }
    
    
            public string SHA256Hash { get; set; }
    
    
            public long FileSize { get; set; }
    
    
            public long FileOffset { get; set; }
    
    
            public int RetryCount { get; set; }
    
    
            public int ProgressPercent { get; set; }
    
    
            public JournalSyncState State { get; set; } = JournalSyncState.Pending;
    
    
            public string Message { get; set; }
    
    
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? StartedAtUtc { get; set; }
    
    
            public DateTime? CompletedAtUtc { get; set; }
    
    
            public DateTime? LastAttemptAtUtc { get; set; }
    
    
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
    
    
            public string ServerPath { get; set; }
    
    
            public JournalSyncRecord()
            {
                SyncId = Guid.NewGuid().ToString("N");
                CreatedAtUtc = DateTime.UtcNow;
                UpdatedAtUtc = CreatedAtUtc;
                State = JournalSyncState.Pending;
            }
    
    
        }
    public partial class JournalSyncRecord
        {
            public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string ATM_ID { get; set; }
    
    
            public string FileName { get; set; }
    
    
            public string LocalPath { get; set; }
    
    
            public string ArchivePath { get; set; }
    
    
            public string Checksum { get; set; }
    
    
            public string SHA256Hash { get; set; }
    
    
            public long FileSize { get; set; }
    
    
            public long FileOffset { get; set; }
    
    
            public int RetryCount { get; set; }
    
    
            public int ProgressPercent { get; set; }
    
    
            public JournalSyncState State { get; set; } = JournalSyncState.Pending;
    
    
            public string Message { get; set; }
    
    
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? StartedAtUtc { get; set; }
    
    
            public DateTime? CompletedAtUtc { get; set; }
    
    
            public DateTime? LastAttemptAtUtc { get; set; }
    
    
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
    
    
        }
    public partial class JournalSyncRecord
        {
            public string SyncId { get; set; }
    
    
            public string ATM_ID { get; set; }
    
    
            public string FileName { get; set; }
    
    
            public long FileSize { get; set; }
    
    
            public string Checksum { get; set; }
    
    
            public JournalSyncState State { get; set; }
    
    
            public int ProgressPercent { get; set; }
    
    
            public int RetryCount { get; set; }
    
    
            public string LocalPath { get; set; }
    
    
            public string ServerPath { get; set; }
    
    
            public string Message { get; set; }
    
    
            public DateTime CreatedAtUtc { get; set; }
    
    
            public DateTime UpdatedAtUtc { get; set; }
    
    
            public JournalSyncRecord()
            {
                SyncId = Guid.NewGuid().ToString("N");
                CreatedAtUtc = DateTime.UtcNow;
                UpdatedAtUtc = CreatedAtUtc;
                State = JournalSyncState.Pending;
            }
    
    
        }
    public sealed class JournalSyncRecord
        {
            public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
            public string ATM_ID { get; set; }
            public string FileName { get; set; }
            public string LocalPath { get; set; }
            public string ArchivePath { get; set; }
            public string Checksum { get; set; }
            public string SHA256Hash { get; set; }
            public long FileSize { get; set; }
            public long FileOffset { get; set; }
            public int RetryCount { get; set; }
            public int ProgressPercent { get; set; }
            public JournalSyncState State { get; set; } = JournalSyncState.Pending;
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
            public DateTime? StartedAtUtc { get; set; }
            public DateTime? CompletedAtUtc { get; set; }
            public DateTime? LastAttemptAtUtc { get; set; }
    
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
        }
    // ==========================================
        // سجل المزامنة المفصّل
        // ==========================================
    
        public sealed class JournalSyncRecord
        {
            public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
            public string           ATM_ID          { get; set; }
            public string           FileName        { get; set; }
            public string           LocalPath       { get; set; }
            public string           ArchivePath     { get; set; }
            public string           Checksum        { get; set; }
            public string           SHA256Hash      { get; set; }
            public long             FileSize        { get; set; }
            public long             FileOffset      { get; set; }
            public int              RetryCount      { get; set; }
            public int              ProgressPercent { get; set; }
            public JournalSyncState State           { get; set; } = JournalSyncState.Pending;
            public string           Message         { get; set; }
            public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
            public DateTime?        StartedAtUtc    { get; set; }
            public DateTime?        CompletedAtUtc  { get; set; }
            public DateTime?        LastAttemptAtUtc { get; set; }
    
            public bool IsFinalState =>
                State == JournalSyncState.Completed || State == JournalSyncState.Archived;
        }
    public class JournalSyncRecord
        {
            public string SyncId { get; set; }
            public string ATM_ID { get; set; }
            public string FileName { get; set; }
            public long FileSize { get; set; }
            public string Checksum { get; set; }
            public JournalSyncState State { get; set; }
            public int ProgressPercent { get; set; }
            public int RetryCount { get; set; }
            public string LocalPath { get; set; }
            public string ServerPath { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
    
            public JournalSyncRecord()
            {
                SyncId = Guid.NewGuid().ToString("N");
                CreatedAtUtc = DateTime.UtcNow;
                UpdatedAtUtc = CreatedAtUtc;
                State = JournalSyncState.Pending;
            }
        }
    public partial class JournalSyncStateEnvelope
        {
            public DateTime LastUpdatedUtc { get; set; }
    
    
            public List<ATMJournalSyncState> AtmStates { get; set; }
    
    
            public List<JournalSyncEntry> Transfers { get; set; }
    
    
            public List<JournalSyncAlert> Alerts { get; set; }
    
    
            public JournalSyncStateEnvelope()
            {
                LastUpdatedUtc = DateTime.UtcNow;
                AtmStates = new List<ATMJournalSyncState>();
                Transfers = new List<JournalSyncEntry>();
                Alerts = new List<JournalSyncAlert>();
            }
    
    
        }
    [Serializable]
        public class JournalSyncStateEnvelope
        {
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
    
            public JournalSyncStateEnvelope()
            {
                LastUpdatedUtc = DateTime.UtcNow;
                AtmStates = new List<ATMJournalSyncState>();
                Transfers = new List<JournalSyncEntry>();
                Alerts = new List<JournalSyncAlert>();
            }
        }
    public partial class JournalSyncStatusSnapshot
        {
            public string ATM_ID { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public DateTime? LastHeartbeatUtc { get; set; }
    
    
            public DateTime? LastJournalSyncUtc { get; set; }
    
    
            public int PendingFiles { get; set; }
    
    
            public int SyncingFiles { get; set; }
    
    
            public int FailedFiles { get; set; }
    
    
            public int CompletedFiles { get; set; }
    
    
            public long PendingBytes { get; set; }
    
    
            public string LastError { get; set; }
    
    
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    public sealed class JournalSyncStatusSnapshot
        {
            public string ATM_ID { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int SyncingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public string LastError { get; set; }
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        }
    // ==========================================
        // لقطة حالة مزامنة الصراف
        // ==========================================
    
        public sealed class JournalSyncStatusSnapshot
        {
            public string    ATM_ID             { get; set; }
            public bool      IsConnected        { get; set; }
            public DateTime? LastHeartbeatUtc   { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int       PendingFiles       { get; set; }
            public int       SyncingFiles       { get; set; }
            public int       FailedFiles        { get; set; }
            public int       CompletedFiles     { get; set; }
            public long      PendingBytes       { get; set; }
            public string    LastError          { get; set; }
            public DateTime  UpdatedAtUtc       { get; set; } = DateTime.UtcNow;
        }
    public partial class LiveSyncProgress
        {
            public string ATMId
            {
                get;
                set;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime UpdatedAt
            {
                get;
                set;
            } = DateTime.UtcNow;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public DateTime UpdatedAt
            {
                get;
                set;
            } = DateTime.UtcNow;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime UpdatedAt
            {
                get;
                set;
            } = DateTime.UtcNow;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime UpdatedAt
            {
                get;
                set;
            } = DateTime.UtcNow;
    
    
            public string ATMId           { get; set; }
    
    
            public string FileName        { get; set; }
    
    
            public string StateLabel      { get; set; }
    
    
            public string StateIcon       { get; set; }
    
    
            public int    Percent         { get; set; }
    
    
            public long   BytesSent       { get; set; }
    
    
            public long   TotalBytes      { get; set; }
    
    
            public double SpeedKBs        { get; set; }
    
    
            public int    SeqNum          { get; set; }
    
    
            public int    TotalChunks     { get; set; }
    
    
            public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
    
            public string BytesSentDisplay => BytesSent > 1048576
                ? $"{BytesSent / 1048576.0:F1} MB"
                : $"{BytesSent / 1024.0:F1} KB";
    
    
            public string TotalBytesDisplay => TotalBytes > 1048576
                ? $"{TotalBytes / 1048576.0:F1} MB"
                : $"{TotalBytes / 1024.0:F1} KB";
    
    
            public sealed class JournalSyncRecord
            {
                public string SyncId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string FileName
                {
                    get;
                    set;
                }
                public string LocalPath
                {
                    get;
                    set;
                }
                public string ArchivePath
                {
                    get;
                    set;
                }
                public string Checksum
                {
                    get;
                    set;
                }
                public string SHA256Hash
                {
                    get;
                    set;
                }
                public long FileSize
                {
                    get;
                    set;
                }
                public long FileOffset
                {
                    get;
                    set;
                }
                public int RetryCount
                {
                    get;
                    set;
                }
                public int ProgressPercent
                {
                    get;
                    set;
                }
                public JournalSyncState State
                {
                    get;
                    set;
                } = JournalSyncState.Pending;
                public string Message
                {
                    get;
                    set;
                }
                public DateTime CreatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime UpdatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime? StartedAtUtc
                {
                    get;
                    set;
                }
                public DateTime? CompletedAtUtc
                {
                    get;
                    set;
                }
                public DateTime? LastAttemptAtUtc
                {
                    get;
                    set;
                }
    
                public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            }
    
    
            public sealed class JournalSyncStatusSnapshot
            {
                public string ATM_ID
                {
                    get;
                    set;
                }
                public bool IsConnected
                {
                    get;
                    set;
                }
                public DateTime? LastHeartbeatUtc
                {
                    get;
                    set;
                }
                public DateTime? LastJournalSyncUtc
                {
                    get;
                    set;
                }
                public int PendingFiles
                {
                    get;
                    set;
                }
                public int SyncingFiles
                {
                    get;
                    set;
                }
                public int FailedFiles
                {
                    get;
                    set;
                }
                public int CompletedFiles
                {
                    get;
                    set;
                }
                public long PendingBytes
                {
                    get;
                    set;
                }
                public string LastError
                {
                    get;
                    set;
                }
                public DateTime UpdatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
            }
    
    
            public sealed class JournalSyncAlert
            {
                public string AlertId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public string ATM_ID
                {
                    get;
                    set;
                }
                public JournalSyncAlertSeverity Severity
                {
                    get;
                    set;
                }
                public string Title
                {
                    get;
                    set;
                }
                public string Message
                {
                    get;
                    set;
                }
                public string RecommendedAction
                {
                    get;
                    set;
                }
                public DateTime CreatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
            }
    
    
            public sealed class JournalSyncDashboardItem
            {
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string ATM_Name
                {
                    get;
                    set;
                }
                public string ATM_Type
                {
                    get;
                    set;
                }
                public bool IsConnected
                {
                    get;
                    set;
                }
                public DateTime? LastHeartbeatUtc
                {
                    get;
                    set;
                }
                public DateTime? LastJournalSyncUtc
                {
                    get;
                    set;
                }
                public int PendingFiles
                {
                    get;
                    set;
                }
                public int FailedFiles
                {
                    get;
                    set;
                }
                public int CompletedFiles
                {
                    get;
                    set;
                }
                public long PendingBytes
                {
                    get;
                    set;
                }
                public VendorRootProfileSummary RootProfile
                {
                    get;
                    set;
                }
                public List<JournalSyncAlert> Alerts
                {
                    get;
                    set;
                } = new List<JournalSyncAlert>();
            }
    
    
            public enum JournalSyncState
                {
                    Pending,
                    Syncing,
                    Completed,
                    Failed,
                    ReSyncing,
                    Archived
                }
    
    
            public enum JournalSyncAlertSeverity
            {
                Info,
                Warning,
                Critical
            }
    
    
        }
    public partial class LiveSyncProgress
        {
            public string ATMId           { get; set; }
    
    
            public string FileName        { get; set; }
    
    
            public string StateLabel      { get; set; }
    
    
            public string StateIcon       { get; set; }
    
    
            public int    Percent         { get; set; }
    
    
            public long   BytesSent       { get; set; }
    
    
            public long   TotalBytes      { get; set; }
    
    
            public double SpeedKBs        { get; set; }
    
    
            public int    SeqNum          { get; set; }
    
    
            public int    TotalChunks     { get; set; }
    
    
            public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
    
            public string BytesSentDisplay => BytesSent > 1048576
                ? $"{BytesSent / 1048576.0:F1} MB"
                : $"{BytesSent / 1024.0:F1} KB";
    
    
            public string TotalBytesDisplay => TotalBytes > 1048576
                ? $"{TotalBytes / 1048576.0:F1} MB"
                : $"{TotalBytes / 1024.0:F1} KB";
    
    
        }
    public partial class LiveSyncProgress
        {
            public string ATMId           { get; set; }
    
    
            public string FileName        { get; set; }
    
    
            public string StateLabel      { get; set; }
    
    
            public string StateIcon       { get; set; }
    
    
            public int    Percent         { get; set; }
    
    
            public long   BytesSent       { get; set; }
    
    
            public long   TotalBytes      { get; set; }
    
    
            public double SpeedKBs        { get; set; }
    
    
            public int    SeqNum          { get; set; }
    
    
            public int    TotalChunks     { get; set; }
    
    
            public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
    
            public string BytesSentDisplay => BytesSent > 1048576
                ? $"{BytesSent / 1048576.0:F1} MB"
                : $"{BytesSent / 1024.0:F1} KB";
    
    
            public string TotalBytesDisplay => TotalBytes > 1048576
                ? $"{TotalBytes / 1048576.0:F1} MB"
                : $"{TotalBytes / 1024.0:F1} KB";
    
    
            public sealed class JournalSyncRecord
            {
                public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
                public string ATM_ID { get; set; }
                public string FileName { get; set; }
                public string LocalPath { get; set; }
                public string ArchivePath { get; set; }
                public string Checksum { get; set; }
                public string SHA256Hash { get; set; }
                public long FileSize { get; set; }
                public long FileOffset { get; set; }
                public int RetryCount { get; set; }
                public int ProgressPercent { get; set; }
                public JournalSyncState State { get; set; } = JournalSyncState.Pending;
                public string Message { get; set; }
                public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
                public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
                public DateTime? StartedAtUtc { get; set; }
                public DateTime? CompletedAtUtc { get; set; }
                public DateTime? LastAttemptAtUtc { get; set; }
    
                public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            }
    
    
            public sealed class JournalSyncStatusSnapshot
            {
                public string ATM_ID { get; set; }
                public bool IsConnected { get; set; }
                public DateTime? LastHeartbeatUtc { get; set; }
                public DateTime? LastJournalSyncUtc { get; set; }
                public int PendingFiles { get; set; }
                public int SyncingFiles { get; set; }
                public int FailedFiles { get; set; }
                public int CompletedFiles { get; set; }
                public long PendingBytes { get; set; }
                public string LastError { get; set; }
                public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
            }
    
    
            public sealed class JournalSyncAlert
            {
                public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
                public string ATM_ID { get; set; }
                public JournalSyncAlertSeverity Severity { get; set; }
                public string Title { get; set; }
                public string Message { get; set; }
                public string RecommendedAction { get; set; }
                public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
            }
    
    
            public sealed class JournalSyncDashboardItem
            {
                public string ATM_ID { get; set; }
                public string ATM_Name { get; set; }
                public string ATM_Type { get; set; }
                public bool IsConnected { get; set; }
                public DateTime? LastHeartbeatUtc { get; set; }
                public DateTime? LastJournalSyncUtc { get; set; }
                public int PendingFiles { get; set; }
                public int FailedFiles { get; set; }
                public int CompletedFiles { get; set; }
                public long PendingBytes { get; set; }
                public VendorRootProfileSummary RootProfile { get; set; }
                public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
            }
    
    
            public enum JournalSyncState
            {
                Pending,
                Syncing,
                Completed,
                Failed,
                ReSyncing,
                Archived
            }
    
    
            public enum JournalSyncAlertSeverity
            {
                Info,
                Warning,
                Critical
            }
    
    
        }
    public partial class LiveSyncProgress
        {
            public string ATMId
            {
                get;
                set;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime UpdatedAt
            {
                get;
                set;
            } = DateTime.UtcNow;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime UpdatedAt
            {
                get;
                set;
            } = DateTime.UtcNow;
    
    
            public string BytesSentDisplay => BytesSent > 1048576 ?
            $"{BytesSent / 1048576.0:F1} MB" :
            $"{BytesSent / 1024.0:F1} KB";
    
    
            public string TotalBytesDisplay => TotalBytes > 1048576 ?
                $"{TotalBytes / 1048576.0:F1} MB" :
                $"{TotalBytes / 1024.0:F1} KB";
    
    
            public sealed class JournalSyncRecord
            {
                public string SyncId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string FileName
                {
                    get;
                    set;
                }
                public string LocalPath
                {
                    get;
                    set;
                }
                public string ArchivePath
                {
                    get;
                    set;
                }
                public string Checksum
                {
                    get;
                    set;
                }
                public string SHA256Hash
                {
                    get;
                    set;
                }
                public long FileSize
                {
                    get;
                    set;
                }
                public long FileOffset
                {
                    get;
                    set;
                }
                public int RetryCount
                {
                    get;
                    set;
                }
                public int ProgressPercent
                {
                    get;
                    set;
                }
                public JournalSyncState State
                {
                    get;
                    set;
                } = JournalSyncState.Pending;
                public string Message
                {
                    get;
                    set;
                }
                public DateTime CreatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime UpdatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
                public DateTime? StartedAtUtc
                {
                    get;
                    set;
                }
                public DateTime? CompletedAtUtc
                {
                    get;
                    set;
                }
                public DateTime? LastAttemptAtUtc
                {
                    get;
                    set;
                }
    
                public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            }
    
    
            public sealed class JournalSyncStatusSnapshot
            {
                public string ATM_ID
                {
                    get;
                    set;
                }
                public bool IsConnected
                {
                    get;
                    set;
                }
                public DateTime? LastHeartbeatUtc
                {
                    get;
                    set;
                }
                public DateTime? LastJournalSyncUtc
                {
                    get;
                    set;
                }
                public int PendingFiles
                {
                    get;
                    set;
                }
                public int SyncingFiles
                {
                    get;
                    set;
                }
                public int FailedFiles
                {
                    get;
                    set;
                }
                public int CompletedFiles
                {
                    get;
                    set;
                }
                public long PendingBytes
                {
                    get;
                    set;
                }
                public string LastError
                {
                    get;
                    set;
                }
                public DateTime UpdatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
            }
    
    
            public sealed class JournalSyncAlert
            {
                public string AlertId
                {
                    get;
                    set;
                } = Guid.NewGuid().ToString("N");
                public string ATM_ID
                {
                    get;
                    set;
                }
                public JournalSyncAlertSeverity Severity
                {
                    get;
                    set;
                }
                public string Title
                {
                    get;
                    set;
                }
                public string Message
                {
                    get;
                    set;
                }
                public string RecommendedAction
                {
                    get;
                    set;
                }
                public DateTime CreatedAtUtc
                {
                    get;
                    set;
                } = DateTime.UtcNow;
            }
    
    
            public sealed class JournalSyncDashboardItem
            {
                public string ATM_ID
                {
                    get;
                    set;
                }
                public string ATM_Name
                {
                    get;
                    set;
                }
                public string ATM_Type
                {
                    get;
                    set;
                }
                public bool IsConnected
                {
                    get;
                    set;
                }
                public DateTime? LastHeartbeatUtc
                {
                    get;
                    set;
                }
                public DateTime? LastJournalSyncUtc
                {
                    get;
                    set;
                }
                public int PendingFiles
                {
                    get;
                    set;
                }
                public int FailedFiles
                {
                    get;
                    set;
                }
                public int CompletedFiles
                {
                    get;
                    set;
                }
                public long PendingBytes
                {
                    get;
                    set;
                }
                public VendorRootProfileSummary RootProfile
                {
                    get;
                    set;
                }
                public List<JournalSyncAlert> Alerts
                {
                    get;
                    set;
                } = new List<JournalSyncAlert>();
            }
    
    
            public enum JournalSyncState
                {
                    Pending,
                    Syncing,
                    Completed,
                    Failed,
                    ReSyncing,
                    Archived
                }
    
    
            public enum JournalSyncAlertSeverity
            {
                Info,
                Warning,
                Critical
            }
    
    
        }
    // ==========================================
        // نموذج تقدم الإرسال الحي
        // ==========================================
    
        public class LiveSyncProgress
        {
            public string ATMId           { get; set; }
            public string FileName        { get; set; }
            public string StateLabel      { get; set; }
            public string StateIcon       { get; set; }
            public int    Percent         { get; set; }
            public long   BytesSent       { get; set; }
            public long   TotalBytes      { get; set; }
            public double SpeedKBs        { get; set; }
            public int    SeqNum          { get; set; }
            public int    TotalChunks     { get; set; }
            public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
            public string BytesSentDisplay => BytesSent > 1048576
                ? $"{BytesSent / 1048576.0:F1} MB"
                : $"{BytesSent / 1024.0:F1} KB";
    
            public string TotalBytesDisplay => TotalBytes > 1048576
                ? $"{TotalBytes / 1048576.0:F1} MB"
                : $"{TotalBytes / 1024.0:F1} KB";
    
        public enum JournalSyncState
        {
            Pending,
            Syncing,
            Completed,
            Failed,
            ReSyncing,
            Archived
        }
    
        public enum JournalSyncAlertSeverity
        {
            Info,
            Warning,
            Critical
        }
    
        public sealed class JournalSyncRecord
        {
            public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
            public string ATM_ID { get; set; }
            public string FileName { get; set; }
            public string LocalPath { get; set; }
            public string ArchivePath { get; set; }
            public string Checksum { get; set; }
            public string SHA256Hash { get; set; }
            public long FileSize { get; set; }
            public long FileOffset { get; set; }
            public int RetryCount { get; set; }
            public int ProgressPercent { get; set; }
            public JournalSyncState State { get; set; } = JournalSyncState.Pending;
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
            public DateTime? StartedAtUtc { get; set; }
            public DateTime? CompletedAtUtc { get; set; }
            public DateTime? LastAttemptAtUtc { get; set; }
    
            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
        }
    
        public sealed class JournalSyncStatusSnapshot
        {
            public string ATM_ID { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int SyncingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public string LastError { get; set; }
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        }
    
        public sealed class JournalSyncAlert
        {
            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
            public string ATM_ID { get; set; }
            public JournalSyncAlertSeverity Severity { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public string RecommendedAction { get; set; }
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        }
    
        public sealed class JournalSyncDashboardItem
        {
            public string ATM_ID { get; set; }
            public string ATM_Name { get; set; }
            public string ATM_Type { get; set; }
            public bool IsConnected { get; set; }
            public DateTime? LastHeartbeatUtc { get; set; }
            public DateTime? LastJournalSyncUtc { get; set; }
            public int PendingFiles { get; set; }
            public int FailedFiles { get; set; }
            public int CompletedFiles { get; set; }
            public long PendingBytes { get; set; }
            public VendorRootProfileSummary RootProfile { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
        }
    }
    // ==========================================
        // نموذج تقدم الإرسال الحي
        // ==========================================
    
        public class LiveSyncProgress
        {
            public string ATMId           { get; set; }
            public string FileName        { get; set; }
            public string StateLabel      { get; set; }
            public string StateIcon       { get; set; }
            public int    Percent         { get; set; }
            public long   BytesSent       { get; set; }
            public long   TotalBytes      { get; set; }
            public double SpeedKBs        { get; set; }
            public int    SeqNum          { get; set; }
            public int    TotalChunks     { get; set; }
            public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
            public string BytesSentDisplay => BytesSent > 1048576
                ? $"{BytesSent / 1048576.0:F1} MB"
                : $"{BytesSent / 1024.0:F1} KB";
    
            public string TotalBytesDisplay => TotalBytes > 1048576
                ? $"{TotalBytes / 1048576.0:F1} MB"
                : $"{TotalBytes / 1024.0:F1} KB";
        }
    public partial class LiveSyncProgress
        {
            public string ATMId
            {
                get;
                set;
            }
            public DateTime UpdatedAt
            {
                get;
                set;
            } = DateTime.UtcNow;
            public string   ATMId       { get; set; }
            public string   FileName    { get; set; }
            public string   StateLabel  { get; set; }
            public string   StateIcon   { get; set; }
            public int      Percent     { get; set; }
            public long     BytesSent   { get; set; }
            public long     TotalBytes  { get; set; }
            public double   SpeedKBs    { get; set; }
            public int      SeqNum      { get; set; }
            public int      TotalChunks { get; set; }
            public DateTime UpdatedAt   { get; set; } = DateTime.UtcNow;
            public string BytesSentDisplay => BytesSent > 1048576
                ? $"{BytesSent / 1048576.0:F1} MB"
                : $"{BytesSent / 1024.0:F1} KB";
            public string TotalBytesDisplay => TotalBytes > 1048576
                ? $"{TotalBytes / 1048576.0:F1} MB"
                : $"{TotalBytes / 1024.0:F1} KB";
            public sealed class JournalSyncRecord
            {
                public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
                public string ATM_ID { get; set; }
                public string FileName { get; set; }
                public string LocalPath { get; set; }
                public string ArchivePath { get; set; }
                public string Checksum { get; set; }
                public string SHA256Hash { get; set; }
                public long FileSize { get; set; }
                public long FileOffset { get; set; }
                public int RetryCount { get; set; }
                public int ProgressPercent { get; set; }
                public JournalSyncState State { get; set; } = JournalSyncState.Pending;
                public string Message { get; set; }
                public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
                public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
                public DateTime? StartedAtUtc { get; set; }
                public DateTime? CompletedAtUtc { get; set; }
                public DateTime? LastAttemptAtUtc { get; set; }
                public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
            }
            public sealed class JournalSyncStatusSnapshot
            {
                public string ATM_ID { get; set; }
                public bool IsConnected { get; set; }
                public DateTime? LastHeartbeatUtc { get; set; }
                public DateTime? LastJournalSyncUtc { get; set; }
                public int PendingFiles { get; set; }
                public int SyncingFiles { get; set; }
                public int FailedFiles { get; set; }
                public int CompletedFiles { get; set; }
                public long PendingBytes { get; set; }
                public string LastError { get; set; }
                public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
            }
            public sealed class JournalSyncAlert
            {
                public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
                public string ATM_ID { get; set; }
                public JournalSyncAlertSeverity Severity { get; set; }
                public string Title { get; set; }
                public string Message { get; set; }
                public string RecommendedAction { get; set; }
                public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
            }
            public sealed class JournalSyncDashboardItem
            {
                public string ATM_ID { get; set; }
                public string ATM_Name { get; set; }
                public string ATM_Type { get; set; }
                public bool IsConnected { get; set; }
                public DateTime? LastHeartbeatUtc { get; set; }
                public DateTime? LastJournalSyncUtc { get; set; }
                public int PendingFiles { get; set; }
                public int FailedFiles { get; set; }
                public int CompletedFiles { get; set; }
                public long PendingBytes { get; set; }
                public VendorRootProfileSummary RootProfile { get; set; }
                public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
            }
            public enum JournalSyncState
            {
                Pending,
                Syncing,
                Completed,
                Failed,
                ReSyncing,
                Archived
            }
            public enum JournalSyncAlertSeverity
            {
                Info,
                Warning,
                Critical
            }
        }
    public partial class RemoteCommand
        {
            public string CommandId
            {
                get;
                set;
            } = Guid.NewGuid().ToString("N");
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public int TimeoutSec
            {
                get;
                set;
            } = 30;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public int TimeoutSec
            {
                get;
                set;
            } = 30;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public int TimeoutSec
            {
                get;
                set;
            } = 30;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public int TimeoutSec
            {
                get;
                set;
            } = 30;
    
    
            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string   CommandType   { get; set; }
    
    
            public string   TargetATMId   { get; set; }
    
    
            public string   Parameters    { get; set; }
    
    
            public string   SentBy        { get; set; }
    
    
            public bool     RequireConfirm { get; set; }
    
    
            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
    
            public string   Status        { get; set; } = "Sent";
    
    
            public string   Result        { get; set; }
    
    
            public DateTime? AckedAtUtc   { get; set; }
    
    
            public int      TimeoutSec    { get; set; } = 30;
    
    
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
    
            public string StatusIcon => Status switch
            {
                "Sent"      => "📤",
                "Received"  => "📩",
                "Executed"  => "✅",
                "Failed"    => "❌",
                "Timeout"   => "⏱️",
                _           => "?"
            };
    
    
            public string DisplayLabel =>
                $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status
            switch
            {
                "Sent" => "SENT",
                "Received" => "RCVD",
                "Executed" => "OK",
                "Failed" => "FAIL",
                "Timeout" => "TIME",
                _ => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            public DateTime ExecutedAt    { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            public string ATMId { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public RemoteCommandType CommandType { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public Dictionary<string, string> Parameters { get; private set; }
    
    
            public DateTime CreatedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? ExecutedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public RemoteCommandType CommandType { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public Dictionary<string, string> Parameters { get; private set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? ExecutedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\JournalSyncModels.cs
            public string DisplayLabel =>
                $"[{CommandId.Substring(0, Math.Min(6, CommandId.Length))}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public Dictionary<string, string> Parameters { get; private set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public string DisplayLabel =>
                $"[{CommandId.Substring(0, Math.Min(6, CommandId.Length))}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public RemoteCommandType CommandType { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public DateTime? ExecutedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status
            switch
            {
                "Sent" => "SENT",
                "Received" => "RCVD",
                "Executed" => "OK",
                "Failed" => "FAIL",
                "Timeout" => "TIME",
                _ => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public RemoteCommandType CommandType { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public Dictionary<string, string> Parameters { get; private set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? ExecutedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public RemoteCommandType CommandType { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public Dictionary<string, string> Parameters { get; private set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? ExecutedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\JournalSyncModels.cs
            public string DisplayLabel =>
                $"[{CommandId.Substring(0, Math.Min(6, CommandId.Length))}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public RemoteCommandType CommandType { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public Dictionary<string, string> Parameters { get; private set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? ExecutedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public RemoteCommandType CommandType { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public Dictionary<string, string> Parameters { get; private set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? ExecutedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public RemoteCommandType CommandType { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public Dictionary<string, string> Parameters { get; private set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? ExecutedAt { get; set; }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status
            switch
            {
                "Sent" => "SENT",
                "Received" => "RCVD",
                "Executed" => "OK",
                "Failed" => "FAIL",
                "Timeout" => "TIME",
                _ => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs
            public string DisplayLabel =>
                $"[{CommandId.Substring(0, Math.Min(6, CommandId.Length))}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            public RemoteCommand()
            {
                CommandId = Guid.NewGuid().ToString("N");
                Parameters = new Dictionary<string, string>();
                CreatedAt = DateTime.UtcNow;
                Status = "Pending";
            }
    
    
            public string GetCommandDescription() => DisplayLabel;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
    
    
        }
    public partial class RemoteCommand
        {
            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string   CommandType   { get; set; }
    
    
            public string   TargetATMId   { get; set; }
    
    
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            public string   SentBy        { get; set; }
    
    
            public bool     RequireConfirm { get; set; }
    
    
            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
    
            public DateTime ExecutedAt    { get; set; }
    
    
            public string   Status        { get; set; } = "Sent";
    
    
            public string   Result        { get; set; }
    
    
            public DateTime? AckedAtUtc   { get; set; }
    
    
            public int      TimeoutSec    { get; set; } = 30;
    
    
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
    
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            public string DisplayLabel =>
                $"[{CommandId.Substring(0, Math.Min(6, CommandId.Length))}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            public string GetCommandDescription() => DisplayLabel;
    
    
        }
    public partial class RemoteCommand
        {
            public string CommandId { get; set; }
    
    
            public string ATMId { get; set; }
    
    
            public RemoteCommandType CommandType { get; set; }
    
    
            public Dictionary<string, string> Parameters { get; private set; }
    
    
            public string Status { get; set; }
    
    
            public string Result { get; set; }
    
    
            public DateTime CreatedAt { get; set; }
    
    
            public DateTime? ExecutedAt { get; set; }
    
    
            public RemoteCommand()
            {
                CommandId = Guid.NewGuid().ToString("N");
                Parameters = new Dictionary<string, string>();
                CreatedAt = DateTime.UtcNow;
                Status = "Pending";
            }
    
    
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
    
    
        }
    public partial class RemoteCommand
        {
            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string   CommandType   { get; set; }
    
    
            public string   TargetATMId   { get; set; }
    
    
            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
    
            public string   SentBy        { get; set; }
    
    
            public bool     RequireConfirm { get; set; }
    
    
            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
    
            public DateTime ExecutedAt    { get; set; }
    
    
            public string   Status        { get; set; } = "Sent";
    
    
            public string   Result        { get; set; }
    
    
            public DateTime? AckedAtUtc   { get; set; }
    
    
            public int      TimeoutSec    { get; set; } = 30;
    
    
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
    
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
    
            public string DisplayLabel =>
                $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            public string GetCommandDescription() => DisplayLabel;
    
    
        }
    public partial class RemoteCommand
        {
            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string   CommandType   { get; set; }
    
    
            public string   TargetATMId   { get; set; }
    
    
            public string   Parameters    { get; set; }
    
    
            public string   SentBy        { get; set; }
    
    
            public bool     RequireConfirm { get; set; }
    
    
            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
    
            public string   Status        { get; set; } = "Sent";
    
    
            public string   Result        { get; set; }
    
    
            public DateTime? AckedAtUtc   { get; set; }
    
    
            public int      TimeoutSec    { get; set; } = 30;
    
    
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
    
            public string StatusIcon => Status switch
            {
                "Sent"      => "📤",
                "Received"  => "📩",
                "Executed"  => "✅",
                "Failed"    => "❌",
                "Timeout"   => "⏱️",
                _           => "?"
            };
    
    
            public string DisplayLabel =>
                $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
        }
    public partial class RemoteCommand
        {
            public string CommandId
            {
                get;
                set;
            } = Guid.NewGuid().ToString("N");
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public int TimeoutSec
            {
                get;
                set;
            } = 30;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public int TimeoutSec
            {
                get;
                set;
            } = 30;
    
    
            public string TargetATMId
            {
                get;
                set;
            }
    
    
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
    
            public string StatusIcon => Status
            switch
            {
                "Sent" => "SENT",
                "Received" => "RCVD",
                "Executed" => "OK",
                "Failed" => "FAIL",
                "Timeout" => "TIME",
                _ => "?"
            };
    
    
            public string DisplayLabel =>
            $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            public string GetCommandDescription() => DisplayLabel;
    
    
        }
    // ==========================================
        // الأوامر البعيدة
        // ==========================================
    
        public class RemoteCommand
        {
            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
            public string   CommandType   { get; set; }
            public string   TargetATMId   { get; set; }
            public CommandParameters Parameters { get; set; } = new CommandParameters();
            public string   SentBy        { get; set; }
            public bool     RequireConfirm { get; set; }
            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
            public DateTime ExecutedAt    { get; set; }
            public string   Status        { get; set; } = "Sent";
            public string   Result        { get; set; }
            public DateTime? AckedAtUtc   { get; set; }
            public int      TimeoutSec    { get; set; } = 30;
    
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
            public string DisplayLabel =>
                $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
            public string GetCommandDescription() => DisplayLabel;
        }
    // ==========================================
        // الأوامر البعيدة
        // ==========================================
    
        public class RemoteCommand
        {
            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
            public string   CommandType   { get; set; }
            public string   TargetATMId   { get; set; }
            public CommandParameters Parameters { get; set; } = new CommandParameters();
            public string   SentBy        { get; set; }
            public bool     RequireConfirm { get; set; }
            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
            public DateTime ExecutedAt    { get; set; }
            public string   Status        { get; set; } = "Sent";
            public string   Result        { get; set; }
            public DateTime? AckedAtUtc   { get; set; }
            public int      TimeoutSec    { get; set; } = 30;
    
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
    
            public string DisplayLabel =>
                $"[{CommandId.Substring(0, Math.Min(6, CommandId.Length))}] {CommandType} → {TargetATMId} [{Status}]";
    
            public string GetCommandDescription() => DisplayLabel;
        }
    // ==========================================
        // الأوامر البعيدة
        // ==========================================
    
        public class RemoteCommand
        {
            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
            public string   CommandType   { get; set; }
            public string   TargetATMId   { get; set; }
            public string   Parameters    { get; set; }
            public string   SentBy        { get; set; }
            public bool     RequireConfirm { get; set; }
            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
            public string   Status        { get; set; } = "Sent";
            public string   Result        { get; set; }
            public DateTime? AckedAtUtc   { get; set; }
            public int      TimeoutSec    { get; set; } = 30;
    
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
            public string StatusIcon => Status switch
            {
                "Sent"      => "📤",
                "Received"  => "📩",
                "Executed"  => "✅",
                "Failed"    => "❌",
                "Timeout"   => "⏱️",
                _           => "?"
            };
    
            public string DisplayLabel =>
                $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
        }
    public class RemoteCommand
        {
            public string CommandId { get; set; }
            public string ATMId { get; set; }
            public RemoteCommandType CommandType { get; set; }
            public Dictionary<string, string> Parameters { get; private set; }
            public string Status { get; set; }
            public string Result { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ExecutedAt { get; set; }
    
            public RemoteCommand()
            {
                CommandId = Guid.NewGuid().ToString("N");
                Parameters = new Dictionary<string, string>();
                CreatedAt = DateTime.UtcNow;
                Status = "Pending";
            }
    
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
        }
    public partial class RemoteCommand
        {
            public string CommandId
            {
                get;
                set;
            } = Guid.NewGuid().ToString("N");
            public int TimeoutSec
            {
                get;
                set;
            } = 30;
            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
            public string   CommandType   { get; set; }
            public string   TargetATMId   { get; set; }
            public CommandParameters Parameters { get; set; } = new CommandParameters();
            public string   SentBy        { get; set; }
            public bool     RequireConfirm { get; set; }
            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
            public DateTime ExecutedAt    { get; set; }
            public string   Status        { get; set; } = "Sent";
            public string   Result        { get; set; }
            public DateTime? AckedAtUtc   { get; set; }
            public int      TimeoutSec    { get; set; } = 30;
            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
            public string StatusIcon => Status switch
            {
                "Sent"      => "SENT",
                "Received"  => "RCVD",
                "Executed"  => "OK",
                "Failed"    => "FAIL",
                "Timeout"   => "TIME",
                _           => "?"
            };
            public string DisplayLabel =>
                $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
            public string ATMId { get; set; }
            public RemoteCommandType CommandType { get; set; }
            public Dictionary<string, string> Parameters { get; private set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ExecutedAt { get; set; }
            public string StatusIcon => Status switch
            {
                "Sent"      => "📤",
                "Received"  => "📩",
                "Executed"  => "✅",
                "Failed"    => "❌",
                "Timeout"   => "⏱️",
                _           => "?"
            };
            public string DisplayLabel =>
                $"[{CommandId.Substring(0, Math.Min(6, CommandId.Length))}] {CommandType} → {TargetATMId} [{Status}]";
            public RemoteCommand()
            {
                CommandId = Guid.NewGuid().ToString("N");
                Parameters = new Dictionary<string, string>();
                CreatedAt = DateTime.UtcNow;
                Status = "Pending";
            }
            public string GetCommandDescription() => DisplayLabel;
            public string GetCommandDescription()
            {
                return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
            }
        }
    public partial class SyncStatusInfo
        {
            public string SyncId
            {
                get;
                set;
            } = Guid.NewGuid().ToString("N");
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? CompletedAt
            {
                get;
                set;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            public DateTime? CompletedAt
            {
                get;
                set;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? CompletedAt
            {
                get;
                set;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? CompletedAt
            {
                get;
                set;
            }
    
    
            public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string   ATMId                   { get; set; }
    
    
            public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
    
    
            public int      TotalFiles              { get; set; }
    
    
            public int      SyncedFiles             { get; set; }
    
    
            public int      FailedFiles             { get; set; }
    
    
            public long     TotalSize               { get; set; }
    
    
            public long     SyncedSize              { get; set; }
    
    
            public int      ProgressPercentage      { get; set; }
    
    
            public double   SyncSpeed               { get; set; }    // KB/s
    
    
            public int      EstimatedTimeRemaining  { get; set; }    // ثانية
    
    
            public int      RetryCount              { get; set; }
    
    
            public string   FailureReason           { get; set; }
    
    
            public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
    
    
            public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? CompletedAt            { get; set; }
    
    
            public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
    
    
            public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
    
    
            public string ETADisplay        => EstimatedTimeRemaining < 60
                ? $"{EstimatedTimeRemaining}ث"
                : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
    
            public string SyncedSizeDisplay =>
                SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                $"{SyncedSize} B";
    
    
            public string FileName { get; set; }
    
    
            public JournalSyncState State { get; set; }
    
    
            public int ProgressPercent { get; set; }
    
    
            public string Message { get; set; }
    
    
            public DateTime UpdatedAtUtc { get; set; }
    
    
        }
    public partial class SyncStatusInfo
        {
            public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string   ATMId                   { get; set; }
    
    
            public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
    
    
            public int      TotalFiles              { get; set; }
    
    
            public int      SyncedFiles             { get; set; }
    
    
            public int      FailedFiles             { get; set; }
    
    
            public long     TotalSize               { get; set; }
    
    
            public long     SyncedSize              { get; set; }
    
    
            public int      ProgressPercentage      { get; set; }
    
    
            public double   SyncSpeed               { get; set; }    // KB/s
    
    
            public int      EstimatedTimeRemaining  { get; set; }    // ثانية
    
    
            public int      RetryCount              { get; set; }
    
    
            public string   FailureReason           { get; set; }
    
    
            public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
    
    
            public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? CompletedAt            { get; set; }
    
    
            public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
    
    
            public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
    
    
            public string ETADisplay        => EstimatedTimeRemaining < 60
                ? $"{EstimatedTimeRemaining}ث"
                : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
    
            public string SyncedSizeDisplay =>
                SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                $"{SyncedSize} B";
    
    
        }
    public partial class SyncStatusInfo
        {
            public string ATMId { get; set; }
    
    
            public string FileName { get; set; }
    
    
            public JournalSyncState State { get; set; }
    
    
            public int ProgressPercent { get; set; }
    
    
            public string Message { get; set; }
    
    
            public DateTime UpdatedAtUtc { get; set; }
    
    
        }
    public partial class SyncStatusInfo
        {
            public string SyncId
            {
                get;
                set;
            } = Guid.NewGuid().ToString("N");
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? CompletedAt
            {
                get;
                set;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            public DateTime? CompletedAt
            {
                get;
                set;
            }
    
    
            public string ProgressDisplay => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
    
    
            public string SpeedDisplay => $"{SyncSpeed:F1} KB/s";
    
    
            public string ETADisplay => EstimatedTimeRemaining < 60 ?
                $"{EstimatedTimeRemaining}ث" :
                $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
    
            public string SyncedSizeDisplay =>
                SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                SyncedSize > 1024 ? $"{SyncedSize / 1024.0:F1} KB" :
                $"{SyncedSize} B";
    
    
        }
    // ==========================================
        // حالة المزامنة
        // ==========================================
    
        public class SyncStatusInfo
        {
            public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
            public string   ATMId                   { get; set; }
            public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
            public int      TotalFiles              { get; set; }
            public int      SyncedFiles             { get; set; }
            public int      FailedFiles             { get; set; }
            public long     TotalSize               { get; set; }
            public long     SyncedSize              { get; set; }
            public int      ProgressPercentage      { get; set; }
            public double   SyncSpeed               { get; set; }    // KB/s
            public int      EstimatedTimeRemaining  { get; set; }    // ثانية
            public int      RetryCount              { get; set; }
            public string   FailureReason           { get; set; }
            public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
            public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
            public DateTime? CompletedAt            { get; set; }
    
            public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
            public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
            public string ETADisplay        => EstimatedTimeRemaining < 60
                ? $"{EstimatedTimeRemaining}ث"
                : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
            public string SyncedSizeDisplay =>
                SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                $"{SyncedSize} B";
        }
    public class SyncStatusInfo
        {
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public JournalSyncState State { get; set; }
            public int ProgressPercent { get; set; }
            public string Message { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
        }
    public partial class SyncStatusInfo
        {
            public string SyncId
            {
                get;
                set;
            } = Guid.NewGuid().ToString("N");
            public DateTime? CompletedAt
            {
                get;
                set;
            }
            public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
            public string   ATMId                   { get; set; }
            public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
            public int      TotalFiles              { get; set; }
            public int      SyncedFiles             { get; set; }
            public int      FailedFiles             { get; set; }
            public long     TotalSize               { get; set; }
            public long     SyncedSize              { get; set; }
            public int      ProgressPercentage      { get; set; }
            public double   SyncSpeed               { get; set; }    // KB/s
            public int      EstimatedTimeRemaining  { get; set; }    // ثانية
            public int      RetryCount              { get; set; }
            public string   FailureReason           { get; set; }
            public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
            public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
            public DateTime? CompletedAt            { get; set; }
            public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
            public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
            public string ETADisplay        => EstimatedTimeRemaining < 60
                ? $"{EstimatedTimeRemaining}ث"
                : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
            public string SyncedSizeDisplay =>
                SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                $"{SyncedSize} B";
            public string FileName { get; set; }
            public JournalSyncState State { get; set; }
            public int ProgressPercent { get; set; }
            public string Message { get; set; }
            public DateTime UpdatedAtUtc { get; set; }
        }
    public partial class VendorRootProfileSummary
        {
            public string Vendor { get; set; }
    
    
            public string Model { get; set; }
    
    
            public string EjPattern { get; set; }
    
    
            public string LogPattern { get; set; }
    
    
        }
    // Vendor root profile summary for dashboard display.
        public sealed class VendorRootProfileSummary
        {
            public string Vendor { get; set; }
            public string Model { get; set; }
            public string EjPattern { get; set; }
            public string LogPattern { get; set; }
        }

    // Class: ATMDetailedStatus (from 2 sources)
        public sealed partial class ATMDetailedStatus
        {
        }
    // Class: ATMError (from 2 sources)
        public sealed partial class ATMError
        {
        }
    // Class: ATMJournalSyncState (from 2 sources)
        public partial class ATMJournalSyncState
        {
            // --- Properties ---
                    public string ATMId { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public DateTime LastHeartbeatUtc { get; set; }
    
                    public DateTime LastJournalReceivedUtc { get; set; }
    
                    public DateTime LastSuccessfulSyncUtc { get; set; }
    
                    public long TotalJournalBytesReceived { get; set; }
    
                    public int TotalJournalFilesReceived { get; set; }
    
                    public int FailedSyncCount { get; set; }
    
                    public int PendingArchiveCount { get; set; }
    
                    public string LastStoredFile { get; set; }
    
                    public string LastChecksum { get; set; }
    
                    public string LastError { get; set; }
    
                    public JournalSyncStatus CurrentStatus { get; set; }
    
    
            // --- Constructors ---
                    public ATMJournalSyncState()
                    {
                        ATMId = "Unknown";
                        CurrentStatus = JournalSyncStatus.Unknown;
                    }
    
    
        }
    // Enum: ATMOperationalState (from 2 sources)
        public partial enum ATMOperationalState
        {
        }
    // Class: ATMTransaction (from 2 sources)
        public sealed partial class ATMTransaction
        {
        }
    // Class: CashStatus (from 2 sources)
        public sealed partial class CashStatus
        {
        }
    // Class: CommandParameters (from 1 sources)
        public partial class CommandParameters : Dictionary<string, string>
        {
            // --- Properties ---
                    public string Raw { get; set; } = string.Empty;
    
    
            // --- Methods ---
                    public static implicit operator CommandParameters(string value)
                    {
                        return new CommandParameters { Raw = value ?? string.Empty };
                    }
    
                    public override string ToString() => Raw;
    
    
        }
    // Class: CommandParameters (from 3 sources)
        public partial class CommandParameters : Dictionary<string, string>
        {
            // --- Properties ---
                    public string Raw { get; set; } = string.Empty;
    
    
            // --- Methods ---
                    public static implicit operator CommandParameters(string value)
                    {
                        return new CommandParameters { Raw = value ?? string.Empty };
                    }
    
                    public override string ToString() => Raw;
    
    
        }
    // Class: CommandParameters (from 2 sources)
        public partial class CommandParameters : Dictionary<string, string>
        {
            // --- Properties ---
                    public string Raw { get; set; } = string.Empty;
    
    
            // --- Methods ---
                    public static implicit operator CommandParameters(string value)
                    {
                        return new CommandParameters { Raw = value ?? string.Empty };
                    }
    
                    public override string ToString() => Raw;
    
    
        }
    // Class: CommandParameters (from 9 sources)
        public partial class CommandParameters : Dictionary<string, string>
        {
            // --- Properties ---
                            public string Raw { get; set; } = string.Empty;
    
    
            // --- Methods ---
                            public static implicit operator CommandParameters(string value)
                            {
                                return new CommandParameters { Raw = value ?? string.Empty };
                            }
    
                            public override string ToString() => Raw;
    
    
        }
    // Class: CommandParameters (from 5 sources)
        public partial class CommandParameters : Dictionary<string, string>
        {
            // --- Properties ---
                    public string Raw { get; set; } = string.Empty;
    
    
            // --- Methods ---
                    public static implicit operator CommandParameters(string value)
                    {
                        return new CommandParameters { Raw = value ?? string.Empty };
                    }
    
                    public override string ToString() => Raw;
    
    
        }
    // Class: GhostSession (from 2 sources)
        public sealed partial class GhostSession
        {
        }
    // Class: ImageSyncItem (from 2 sources)
        public sealed partial class ImageSyncItem
        {
        }
    // Class: JournalDailyStats (from 1 sources)
        public partial class JournalDailyStats
        {
            // --- Properties ---
                    public string   ATMId                { get; set; }
    
                    public DateTime Date                 { get; set; }
    
                    public int      TotalTransactions    { get; set; }
    
                    public int      ApprovedTransactions { get; set; }
    
                    public int      FailedTransactions   { get; set; }
    
                    public int      CardsCaptured        { get; set; }
    
                    public long     CashDispensed        { get; set; }
    
                    public long     JournalBytesReceived { get; set; }
    
                    public double   UptimePercent        { get; set; } = 100.0;
    
                    public double   SyncSuccessPercent   { get; set; } = 100.0;
    
                    public double SuccessRate =>
                        TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
    
    
        }
    // Class: JournalDailyStats (from 3 sources)
        public partial class JournalDailyStats
        {
            // --- Properties ---
                    public string   ATMId                { get; set; }
    
                    public DateTime Date                 { get; set; }
    
                    public int      TotalTransactions    { get; set; }
    
                    public int      ApprovedTransactions { get; set; }
    
                    public int      FailedTransactions   { get; set; }
    
                    public int      CardsCaptured        { get; set; }
    
                    public long     CashDispensed        { get; set; }
    
                    public long     JournalBytesReceived { get; set; }
    
                    public double   UptimePercent        { get; set; } = 100.0;
    
                    public double   SyncSuccessPercent   { get; set; } = 100.0;
    
                    public double SuccessRate =>
                        TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
    
    
        }
    // Class: JournalDailyStats (from 2 sources)
        public partial class JournalDailyStats
        {
            // --- Properties ---
                            public string   ATMId                { get; set; }
    
                            public DateTime Date                 { get; set; }
    
                            public int      TotalTransactions    { get; set; }
    
                            public int      ApprovedTransactions { get; set; }
    
                            public int      FailedTransactions   { get; set; }
    
                            public int      CardsCaptured        { get; set; }
    
                            public long     CashDispensed        { get; set; }
    
                            public long     JournalBytesReceived { get; set; }
    
                            public double   UptimePercent        { get; set; } = 100.0;
    
                            public double   SyncSuccessPercent   { get; set; } = 100.0;
    
                            public double SuccessRate =>
                                TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
    
    
        }
    // Class: JournalDailyStats (from 9 sources)
        public partial class JournalDailyStats
        {
            // --- Properties ---
                            public string   ATMId                { get; set; }
    
                            public DateTime Date                 { get; set; }
    
                            public int      TotalTransactions    { get; set; }
    
                            public int      ApprovedTransactions { get; set; }
    
                            public int      FailedTransactions   { get; set; }
    
                            public int      CardsCaptured        { get; set; }
    
                            public long     CashDispensed        { get; set; }
    
                            public long     JournalBytesReceived { get; set; }
    
                            public double   UptimePercent        { get; set; } = 100.0;
    
                            public double   SyncSuccessPercent   { get; set; } = 100.0;
    
                            public double SuccessRate =>
                                TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
    
    
        }
    // Class: JournalDailyStats (from 5 sources)
        public partial class JournalDailyStats
        {
            // --- Properties ---
                    public string   ATMId                { get; set; }
    
                    public DateTime Date                 { get; set; }
    
                    public int      TotalTransactions    { get; set; }
    
                    public int      ApprovedTransactions { get; set; }
    
                    public int      FailedTransactions   { get; set; }
    
                    public int      CardsCaptured        { get; set; }
    
                    public long     CashDispensed        { get; set; }
    
                    public long     JournalBytesReceived { get; set; }
    
                    public double   UptimePercent        { get; set; } = 100.0;
    
                    public double   SyncSuccessPercent   { get; set; } = 100.0;
    
                    public double SuccessRate =>
                        TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
    
    
        }
    // ═══ Class: JournalDailyStats (from 1 sources) ═══
        public partial class JournalDailyStats
        {
            // --- Properties ---
                    public string   ATMId                { get; set; }
    
                    public DateTime Date                 { get; set; }
    
                    public int      TotalTransactions    { get; set; }
    
                    public int      ApprovedTransactions { get; set; }
    
                    public int      FailedTransactions   { get; set; }
    
                    public int      CardsCaptured        { get; set; }
    
                    public long     CashDispensed        { get; set; }
    
                    public long     JournalBytesReceived { get; set; }
    
                    public double   UptimePercent        { get; set; } = 100.0;
    
                    public double   SyncSuccessPercent   { get; set; } = 100.0;
    
                    public double SuccessRate =>
                        TotalTransactions > 0 ? (double)ApprovedTransactions / TotalTransactions * 100.0 : 0;
    
    
        }
    // Class: JournalEntry (from 1 sources)
        public partial class JournalEntry
        {
            // --- Properties ---
                    public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   ATMId            { get; set; }
    
                    public string   FileName         { get; set; }
    
                    public string   FilePath         { get; set; }
    
                    public long     OriginalSize     { get; set; }
    
                    public long     CompressedSize   { get; set; }
    
                    public long     EncryptedSize    { get; set; }
    
                    public bool     IsEncrypted      { get; set; } = true;
    
                    public bool     IsCompressed     { get; set; } = true;
    
                    public string   Checksum         { get; set; }    // MD5
    
                    public string   MD5Hash          { get; set; }
    
                    public string   SHA256Hash       { get; set; }
    
                    public int      TransactionCount { get; set; }
    
                    public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
                    public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
                    public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
                    public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
                    public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
    
                    public DateTime VerifiedAt       { get; set; }
    
                    public string   MonthPartition   { get; set; }    // YYYY-MM
    
                    public double CompressionRatio =>
                        OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
                    public string FileSizeDisplay =>
                        OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                        OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                        $"{OriginalSize} B";
    
    
        }
    // Class: JournalEntry (from 2 sources)
        public partial class JournalEntry
        {
            // --- Properties ---
                    public string ATMId { get; set; }
    
                    public string FileName { get; set; }
    
                    public long OriginalSize { get; set; }
    
                    public long CompressedSize { get; set; }
    
                    public long EncryptedSize { get; set; }
    
                    public bool IsEncrypted { get; set; }
    
                    public bool IsCompressed { get; set; }
    
                    public string Checksum { get; set; }
    
                    public string MD5Hash { get; set; }
    
                    public DateTime CreatedAt { get; set; }
    
                    public DateTime ReceivedAt { get; set; }
    
                    public string Status { get; set; }
    
                    public int TransactionCount { get; set; }
    
    
        }
    // Class: JournalEntry (from 3 sources)
        public partial class JournalEntry
        {
            // --- Properties ---
                    public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   ATMId            { get; set; }
    
                    public string   FileName         { get; set; }
    
                    public string   FilePath         { get; set; }
    
                    public long     OriginalSize     { get; set; }
    
                    public long     CompressedSize   { get; set; }
    
                    public long     EncryptedSize    { get; set; }
    
                    public bool     IsEncrypted      { get; set; } = true;
    
                    public bool     IsCompressed     { get; set; } = true;
    
                    public string   Checksum         { get; set; }    // MD5
    
                    public string   MD5Hash          { get; set; }
    
                    public string   SHA256Hash       { get; set; }
    
                    public int      TransactionCount { get; set; }
    
                    public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
                    public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
                    public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
                    public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
                    public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
    
                    public DateTime VerifiedAt       { get; set; }
    
                    public string   MonthPartition   { get; set; }    // YYYY-MM
    
                    public double CompressionRatio =>
                        OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
                    public string FileSizeDisplay =>
                        OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                        OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                        $"{OriginalSize} B";
    
    
        }
    // Class: JournalEntry (from 2 sources)
        public partial class JournalEntry
        {
            // --- Properties ---
                            public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
                            public string   ATMId            { get; set; }
    
                            public string   FileName         { get; set; }
    
                            public string   FilePath         { get; set; }
    
                            public long     OriginalSize     { get; set; }
    
                            public long     CompressedSize   { get; set; }
    
                            public long     EncryptedSize    { get; set; }
    
                            public bool     IsEncrypted      { get; set; } = true;
    
                            public bool     IsCompressed     { get; set; } = true;
    
                            public string   Checksum         { get; set; }    // MD5
    
                            public string   MD5Hash          { get; set; }
    
                            public string   SHA256Hash       { get; set; }
    
                            public int      TransactionCount { get; set; }
    
                            public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
                            public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
                            public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
                            public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
                            public DateTime VerifiedAt       { get; set; }
    
                            public string   MonthPartition   { get; set; }    // YYYY-MM
    
                            public double CompressionRatio =>
                                OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
                            public string FileSizeDisplay =>
                                OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                                OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                                $"{OriginalSize} B";
    
    
        }
    // Class: JournalEntry (from 2 sources)
        public partial class JournalEntry
        {
            // --- Properties ---
                    public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   ATMId            { get; set; }
    
                    public string   FileName         { get; set; }
    
                    public string   FilePath         { get; set; }
    
                    public long     OriginalSize     { get; set; }
    
                    public long     CompressedSize   { get; set; }
    
                    public long     EncryptedSize    { get; set; }
    
                    public bool     IsEncrypted      { get; set; } = true;
    
                    public bool     IsCompressed     { get; set; } = true;
    
                    public string   Checksum         { get; set; }    // MD5
    
                    public string   MD5Hash          { get; set; }
    
                    public string   SHA256Hash       { get; set; }
    
                    public int      TransactionCount { get; set; }
    
                    public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
                    public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
                    public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
                    public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
                    public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
    
                    public DateTime VerifiedAt       { get; set; }
    
                    public string   MonthPartition   { get; set; }    // YYYY-MM
    
                    public double CompressionRatio =>
                        OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
                    public string FileSizeDisplay =>
                        OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                        OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                        $"{OriginalSize} B";
    
    
        }
    // Class: JournalEntry (from 9 sources)
        public partial class JournalEntry
        {
            // --- Properties ---
                            public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
                            public string   ATMId            { get; set; }
    
                            public string   FileName         { get; set; }
    
                            public string   FilePath         { get; set; }
    
                            public long     OriginalSize     { get; set; }
    
                            public long     CompressedSize   { get; set; }
    
                            public long     EncryptedSize    { get; set; }
    
                            public bool     IsEncrypted      { get; set; } = true;
    
                            public bool     IsCompressed     { get; set; } = true;
    
                            public string   Checksum         { get; set; }    // MD5
    
                            public string   MD5Hash          { get; set; }
    
                            public string   SHA256Hash       { get; set; }
    
                            public int      TransactionCount { get; set; }
    
                            public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
                            public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
                            public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
                            public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
                            public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
    
                            public DateTime VerifiedAt       { get; set; }
    
                            public string   MonthPartition   { get; set; }    // YYYY-MM
    
                            public double CompressionRatio =>
                                OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
                            public string FileSizeDisplay =>
                                OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                                OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                                $"{OriginalSize} B";
    
    
        }
    // Class: JournalEntry (from 5 sources)
        public partial class JournalEntry
        {
            // --- Properties ---
                    public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   ATMId            { get; set; }
    
                    public string   FileName         { get; set; }
    
                    public string   FilePath         { get; set; }
    
                    public long     OriginalSize     { get; set; }
    
                    public long     CompressedSize   { get; set; }
    
                    public long     EncryptedSize    { get; set; }
    
                    public bool     IsEncrypted      { get; set; } = true;
    
                    public bool     IsCompressed     { get; set; } = true;
    
                    public string   Checksum         { get; set; }    // MD5
    
                    public string   MD5Hash          { get; set; }
    
                    public string   SHA256Hash       { get; set; }
    
                    public int      TransactionCount { get; set; }
    
                    public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
                    public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
                    public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
                    public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
                    public DateTime CreatedAt        { get => ReceivedAt; set => ReceivedAt = value; }
    
                    public DateTime VerifiedAt       { get; set; }
    
                    public string   MonthPartition   { get; set; }    // YYYY-MM
    
                    public double CompressionRatio =>
                        OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
                    public string FileSizeDisplay =>
                        OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                        OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                        $"{OriginalSize} B";
    
    
        }
    // ═══ Class: JournalEntry (from 1 sources) ═══
        public partial class JournalEntry
        {
            // --- Properties ---
                    public string   EntryId          { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   ATMId            { get; set; }
    
                    public string   FileName         { get; set; }
    
                    public string   FilePath         { get; set; }
    
                    public long     OriginalSize     { get; set; }
    
                    public long     CompressedSize   { get; set; }
    
                    public long     EncryptedSize    { get; set; }
    
                    public bool     IsEncrypted      { get; set; } = true;
    
                    public bool     IsCompressed     { get; set; } = true;
    
                    public string   Checksum         { get; set; }    // MD5
    
                    public string   MD5Hash          { get; set; }
    
                    public string   SHA256Hash       { get; set; }
    
                    public int      TransactionCount { get; set; }
    
                    public string   Status           { get; set; }    // Pending/Syncing/Synced/Failed/Resyncing
    
                    public long     FileOffset       { get; set; }    // آخر موضع (مهم لـ NCR)
    
                    public string   ArchivePath      { get; set; }    // مسار الملف في الأرشيف
    
                    public DateTime ReceivedAt       { get; set; } = DateTime.UtcNow;
    
                    public DateTime VerifiedAt       { get; set; }
    
                    public string   MonthPartition   { get; set; }    // YYYY-MM
    
                    public double CompressionRatio =>
                        OriginalSize > 0 ? (1.0 - (double)CompressedSize / OriginalSize) * 100.0 : 0;
    
                    public string FileSizeDisplay =>
                        OriginalSize > 1048576 ? $"{OriginalSize / 1048576.0:F1} MB" :
                        OriginalSize > 1024    ? $"{OriginalSize / 1024.0:F1} KB"    :
                        $"{OriginalSize} B";
    
    
        }
    // Class: JournalSyncAlert (from 7 sources)
        public sealed partial class JournalSyncAlert
        {
            // --- Properties ---
                    public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string ATM_ID { get; set; }
    
                    public JournalSyncAlertSeverity Severity { get; set; }
    
                    public string Title { get; set; }
    
                    public string Message { get; set; }
    
                    public string RecommendedAction { get; set; }
    
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
                    public string ATMId { get; set; }
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v23_bak
                    public string Severity { get; set; }
    
                    public string Code { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncAlert()
                    {
                        AlertId = Guid.NewGuid().ToString("N");
                        CreatedAtUtc = DateTime.UtcNow;
                        Severity = "Info";
                        Code = "sync-info";
                    }
    
    
        }
    // Class: JournalSyncAlert (from 2 sources)
        public sealed partial class JournalSyncAlert
        {
            // --- Properties ---
                            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
    
                            public string ATM_ID { get; set; }
    
                            public JournalSyncAlertSeverity Severity { get; set; }
    
                            public string Title { get; set; }
    
                            public string Message { get; set; }
    
                            public string RecommendedAction { get; set; }
    
                            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: JournalSyncAlert (from 1 sources)
        public sealed partial class JournalSyncAlert
        {
            // --- Properties ---
                    public string                 AlertId           { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string                 ATM_ID            { get; set; }
    
                    public JournalSyncAlertSeverity Severity        { get; set; }
    
                    public string                 Title             { get; set; }
    
                    public string                 Message           { get; set; }
    
                    public string                 RecommendedAction { get; set; }
    
                    public DateTime               CreatedAtUtc      { get; set; } = DateTime.UtcNow;
    
    
        }
    // ═══ Class: JournalSyncAlert (from 1 sources) ═══
        public sealed partial class JournalSyncAlert
        {
            // --- Properties ---
                    public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string ATM_ID { get; set; }
    
                    public JournalSyncAlertSeverity Severity { get; set; }
    
                    public string Title { get; set; }
    
                    public string Message { get; set; }
    
                    public string RecommendedAction { get; set; }
    
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Enum: JournalSyncAlertSeverity (from 6 sources)
        public partial enum JournalSyncAlertSeverity
        {
            // --- Constants & Fields ---
                    Info,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v20_bak
                    Critical
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v17_bak
                    Critical
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v16_bak
                    Critical
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v15_bak
                    Critical
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs
                    Critical
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v22_bak
                    Critical
    
    
        }
    // Enum: JournalSyncAlertSeverity (from 2 sources)
        public partial enum JournalSyncAlertSeverity
        {
            // --- Constants & Fields ---
                            Info,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Models\JournalSyncModels.cs
                            Critical
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\JournalSyncModels.cs
                    Critical
    
    
        }
    // Enum: JournalSyncAlertSeverity (from 2 sources)
        public partial enum JournalSyncAlertSeverity
        {
            // --- Constants & Fields ---
                    Info,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
                    Critical
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
                    Critical
    
    
        }
    // Enum: JournalSyncAlertSeverity (from 2 sources)
        public partial enum JournalSyncAlertSeverity
        {
            // --- Constants & Fields ---
                            Info,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
                            Critical
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
                    Critical
    
    
        }
    // Enum: JournalSyncAlertSeverity (from 1 sources)
        public partial enum JournalSyncAlertSeverity
        {
            // --- Constants & Fields ---
                    Info,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
                    Critical
    
    
        }
    // ═══ Enum: JournalSyncAlertSeverity (from 1 sources) ═══
        public partial enum JournalSyncAlertSeverity
        {
            // --- Constants & Fields ---
                    Info,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\JournalSyncModels.cs
                    Critical
    
    
        }
    // Class: JournalSyncDashboardItem (from 6 sources)
        public sealed partial class JournalSyncDashboardItem
        {
            // --- Properties ---
                    public string ATM_ID { get; set; }
    
                    public string ATM_Name { get; set; }
    
                    public string ATM_Type { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public DateTime? LastHeartbeatUtc { get; set; }
    
                    public DateTime? LastJournalSyncUtc { get; set; }
    
                    public int PendingFiles { get; set; }
    
                    public int FailedFiles { get; set; }
    
                    public int CompletedFiles { get; set; }
    
                    public long PendingBytes { get; set; }
    
                    public VendorRootProfileSummary RootProfile { get; set; }
    
                    public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
    
    
        }
    // Class: JournalSyncDashboardItem (from 2 sources)
        public sealed partial class JournalSyncDashboardItem
        {
            // --- Properties ---
                            public string ATM_ID { get; set; }
    
                            public string ATM_Name { get; set; }
    
                            public string ATM_Type { get; set; }
    
                            public bool IsConnected { get; set; }
    
                            public DateTime? LastHeartbeatUtc { get; set; }
    
                            public DateTime? LastJournalSyncUtc { get; set; }
    
                            public int PendingFiles { get; set; }
    
                            public int FailedFiles { get; set; }
    
                            public int CompletedFiles { get; set; }
    
                            public long PendingBytes { get; set; }
    
                            public VendorRootProfileSummary RootProfile { get; set; }
    
                            public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
    
    
        }
    // Class: JournalSyncDashboardItem (from 1 sources)
        public sealed partial class JournalSyncDashboardItem
        {
            // --- Properties ---
                    public string                  ATM_ID            { get; set; }
    
                    public string                  ATM_Name          { get; set; }
    
                    public string                  ATM_Type          { get; set; }
    
                    public bool                    IsConnected       { get; set; }
    
                    public DateTime?               LastHeartbeatUtc  { get; set; }
    
                    public DateTime?               LastJournalSyncUtc { get; set; }
    
                    public int                     PendingFiles      { get; set; }
    
                    public int                     FailedFiles       { get; set; }
    
                    public int                     CompletedFiles    { get; set; }
    
                    public long                    PendingBytes      { get; set; }
    
                    public VendorRootProfileSummary RootProfile      { get; set; }
    
                    public List<JournalSyncAlert>  Alerts            { get; set; } = new List<JournalSyncAlert>();
    
    
        }
    // ═══ Class: JournalSyncDashboardItem (from 1 sources) ═══
        public sealed partial class JournalSyncDashboardItem
        {
            // --- Properties ---
                    public string ATM_ID { get; set; }
    
                    public string ATM_Name { get; set; }
    
                    public string ATM_Type { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public DateTime? LastHeartbeatUtc { get; set; }
    
                    public DateTime? LastJournalSyncUtc { get; set; }
    
                    public int PendingFiles { get; set; }
    
                    public int FailedFiles { get; set; }
    
                    public int CompletedFiles { get; set; }
    
                    public long PendingBytes { get; set; }
    
                    public VendorRootProfileSummary RootProfile { get; set; }
    
                    public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
    
    
        }
    // Class: JournalSyncDashboardSnapshot (from 2 sources)
        public partial class JournalSyncDashboardSnapshot
        {
            // --- Properties ---
                    public int TotalAtms { get; set; }
    
                    public int ConnectedAtms { get; set; }
    
                    public int WarningAtms { get; set; }
    
                    public int CriticalAtms { get; set; }
    
                    public int TotalTransfers { get; set; }
    
                    public int FailedTransfers { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> RecentTransfers { get; set; }
    
                    public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncDashboardSnapshot()
                    {
                        AtmStates = new List<ATMJournalSyncState>();
                        RecentTransfers = new List<JournalSyncEntry>();
                        ActiveAlerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Class: JournalSyncEntry (from 3 sources)
        public partial class JournalSyncEntry
        {
            // --- Properties ---
                    public string EntryId { get; set; }
    
                    public string ATMId { get; set; }
    
                    public string FileName { get; set; }
    
                    public string RelativeStoragePath { get; set; }
    
                    public long FileSizeBytes { get; set; }
    
                    public string Checksum { get; set; }
    
                    public DateTime ReceivedAtUtc { get; set; }
    
                    public JournalTransferStatus Status { get; set; }
    
                    public string ErrorMessage { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncEntry()
                    {
                        EntryId = Guid.NewGuid().ToString("N");
                        ReceivedAtUtc = DateTime.UtcNow;
                        Status = JournalTransferStatus.Received;
                    }
    
    
        }
    // Class: JournalSyncModels (from 1 sources)
        public partial class JournalSyncModels
        {
        }
    // Class: JournalSyncRecord (from 6 sources)
        public sealed partial class JournalSyncRecord
        {
            // --- Properties ---
                    public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string ATM_ID { get; set; }
    
                    public string FileName { get; set; }
    
                    public string LocalPath { get; set; }
    
                    public string ArchivePath { get; set; }
    
                    public string Checksum { get; set; }
    
                    public string SHA256Hash { get; set; }
    
                    public long FileSize { get; set; }
    
                    public long FileOffset { get; set; }
    
                    public int RetryCount { get; set; }
    
                    public int ProgressPercent { get; set; }
    
                    public JournalSyncState State { get; set; } = JournalSyncState.Pending;
    
                    public string Message { get; set; }
    
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime? StartedAtUtc { get; set; }
    
                    public DateTime? CompletedAtUtc { get; set; }
    
                    public DateTime? LastAttemptAtUtc { get; set; }
    
                    public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
    
    
        }
    // Class: JournalSyncRecord (from 2 sources)
        public partial class JournalSyncRecord
        {
            // --- Properties ---
                    public string SyncId { get; set; }
    
                    public string ATM_ID { get; set; }
    
                    public string FileName { get; set; }
    
                    public long FileSize { get; set; }
    
                    public string Checksum { get; set; }
    
                    public JournalSyncState State { get; set; }
    
                    public int ProgressPercent { get; set; }
    
                    public int RetryCount { get; set; }
    
                    public string LocalPath { get; set; }
    
                    public string ServerPath { get; set; }
    
                    public string Message { get; set; }
    
                    public DateTime CreatedAtUtc { get; set; }
    
                    public DateTime UpdatedAtUtc { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncRecord()
                    {
                        SyncId = Guid.NewGuid().ToString("N");
                        CreatedAtUtc = DateTime.UtcNow;
                        UpdatedAtUtc = CreatedAtUtc;
                        State = JournalSyncState.Pending;
                    }
    
    
        }
    // Class: JournalSyncRecord (from 2 sources)
        public sealed partial class JournalSyncRecord
        {
            // --- Properties ---
                            public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
    
                            public string ATM_ID { get; set; }
    
                            public string FileName { get; set; }
    
                            public string LocalPath { get; set; }
    
                            public string ArchivePath { get; set; }
    
                            public string Checksum { get; set; }
    
                            public string SHA256Hash { get; set; }
    
                            public long FileSize { get; set; }
    
                            public long FileOffset { get; set; }
    
                            public int RetryCount { get; set; }
    
                            public int ProgressPercent { get; set; }
    
                            public JournalSyncState State { get; set; } = JournalSyncState.Pending;
    
                            public string Message { get; set; }
    
                            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
                            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
                            public DateTime? StartedAtUtc { get; set; }
    
                            public DateTime? CompletedAtUtc { get; set; }
    
                            public DateTime? LastAttemptAtUtc { get; set; }
    
                            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
    
    
        }
    // Class: JournalSyncRecord (from 1 sources)
        public sealed partial class JournalSyncRecord
        {
            // --- Properties ---
                    public string           SyncId          { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string           ATM_ID          { get; set; }
    
                    public string           FileName        { get; set; }
    
                    public string           LocalPath       { get; set; }
    
                    public string           ArchivePath     { get; set; }
    
                    public string           Checksum        { get; set; }
    
                    public string           SHA256Hash      { get; set; }
    
                    public long             FileSize        { get; set; }
    
                    public long             FileOffset      { get; set; }
    
                    public int              RetryCount      { get; set; }
    
                    public int              ProgressPercent { get; set; }
    
                    public JournalSyncState State           { get; set; } = JournalSyncState.Pending;
    
                    public string           Message         { get; set; }
    
                    public DateTime         CreatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                    public DateTime         UpdatedAtUtc    { get; set; } = DateTime.UtcNow;
    
                    public DateTime?        StartedAtUtc    { get; set; }
    
                    public DateTime?        CompletedAtUtc  { get; set; }
    
                    public DateTime?        LastAttemptAtUtc { get; set; }
    
                    public bool IsFinalState =>
                        State == JournalSyncState.Completed || State == JournalSyncState.Archived;
    
    
        }
    // ═══ Class: JournalSyncRecord (from 1 sources) ═══
        public sealed partial class JournalSyncRecord
        {
            // --- Properties ---
                    public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string ATM_ID { get; set; }
    
                    public string FileName { get; set; }
    
                    public string LocalPath { get; set; }
    
                    public string ArchivePath { get; set; }
    
                    public string Checksum { get; set; }
    
                    public string SHA256Hash { get; set; }
    
                    public long FileSize { get; set; }
    
                    public long FileOffset { get; set; }
    
                    public int RetryCount { get; set; }
    
                    public int ProgressPercent { get; set; }
    
                    public JournalSyncState State { get; set; } = JournalSyncState.Pending;
    
                    public string Message { get; set; }
    
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime? StartedAtUtc { get; set; }
    
                    public DateTime? CompletedAtUtc { get; set; }
    
                    public DateTime? LastAttemptAtUtc { get; set; }
    
                    public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
    
    
        }
    // Enum: JournalSyncState (from 6 sources)
        public partial enum JournalSyncState
        {
            // --- Constants & Fields ---
                    Pending,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v20_bak
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v17_bak
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v16_bak
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v15_bak
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v22_bak
                    Archived
    
    
        }
    // Enum: JournalSyncState (from 2 sources)
        public partial enum JournalSyncState
        {
            // --- Constants & Fields ---
                    LocalSaving = 0,
    
                    Pending = 1,
    
                    Syncing = 2,
    
                    ReSyncing = 3,
    
                    Completed = 4,
    
                    Failed = 5,
    
                    StoredOnServer = 6,
    
                    Acknowledged = 7
    
    
        }
    // Enum: JournalSyncState (from 2 sources)
        public partial enum JournalSyncState
        {
            // --- Constants & Fields ---
                            Pending,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Models\JournalSyncModels.cs
                            Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\JournalSyncModels.cs
                    Archived
    
    
        }
    // Enum: JournalSyncState (from 2 sources)
        public partial enum JournalSyncState
        {
            // --- Constants & Fields ---
                    Pending   = 0,
    
                    Syncing   = 1,
    
                    ReSyncing = 2,
    
                    Completed = 3,
    
                    Failed    = 4,
    
                    Archived  = 5
    
    
        }
    // Enum: JournalSyncState (from 1 sources)
        public partial enum JournalSyncState
        {
            // --- Constants & Fields ---
                    Pending   = 0,
    
                    Syncing   = 1,
    
                    ReSyncing = 2,
    
                    Completed = 3,
    
                    Failed    = 4,
    
                    Archived  = 5
    
    
        }
    // ═══ Enum: JournalSyncState (from 1 sources) ═══
        public partial enum JournalSyncState
        {
            // --- Constants & Fields ---
                    Pending,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\JournalSyncModels.cs
                    Archived
    
    
        }
    // ==========================================
        // حالة مزامنة الجورنال (Enum)
        // ملاحظة: هذا التعريف مرجعي لـ JournalSyncModels.
        // التعريف الموثوق الأصلي بالأرقام موجود في ATMInfo.cs (EJLive.Core.Models).
        // ==========================================
    
        public enum JournalSyncState
        {
            Pending   = 0,
            Syncing   = 1,
            ReSyncing = 2,
            Completed = 3,
            Failed    = 4,
            Archived  = 5
        }
    // Class: JournalSyncStateEnvelope (from 2 sources)
        public partial class JournalSyncStateEnvelope
        {
            // --- Properties ---
                    public DateTime LastUpdatedUtc { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> Transfers { get; set; }
    
                    public List<JournalSyncAlert> Alerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncStateEnvelope()
                    {
                        LastUpdatedUtc = DateTime.UtcNow;
                        AtmStates = new List<ATMJournalSyncState>();
                        Transfers = new List<JournalSyncEntry>();
                        Alerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Enum: JournalSyncStatus (from 2 sources)
        public partial enum JournalSyncStatus
        {
            // --- Constants & Fields ---
                    Unknown,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v23_bak
                    Disconnected
    
    
        }
    // Class: JournalSyncStatusSnapshot (from 6 sources)
        public sealed partial class JournalSyncStatusSnapshot
        {
            // --- Properties ---
                    public string ATM_ID { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public DateTime? LastHeartbeatUtc { get; set; }
    
                    public DateTime? LastJournalSyncUtc { get; set; }
    
                    public int PendingFiles { get; set; }
    
                    public int SyncingFiles { get; set; }
    
                    public int FailedFiles { get; set; }
    
                    public int CompletedFiles { get; set; }
    
                    public long PendingBytes { get; set; }
    
                    public string LastError { get; set; }
    
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: JournalSyncStatusSnapshot (from 2 sources)
        public sealed partial class JournalSyncStatusSnapshot
        {
            // --- Properties ---
                            public string ATM_ID { get; set; }
    
                            public bool IsConnected { get; set; }
    
                            public DateTime? LastHeartbeatUtc { get; set; }
    
                            public DateTime? LastJournalSyncUtc { get; set; }
    
                            public int PendingFiles { get; set; }
    
                            public int SyncingFiles { get; set; }
    
                            public int FailedFiles { get; set; }
    
                            public int CompletedFiles { get; set; }
    
                            public long PendingBytes { get; set; }
    
                            public string LastError { get; set; }
    
                            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: JournalSyncStatusSnapshot (from 1 sources)
        public sealed partial class JournalSyncStatusSnapshot
        {
            // --- Properties ---
                    public string    ATM_ID             { get; set; }
    
                    public bool      IsConnected        { get; set; }
    
                    public DateTime? LastHeartbeatUtc   { get; set; }
    
                    public DateTime? LastJournalSyncUtc { get; set; }
    
                    public int       PendingFiles       { get; set; }
    
                    public int       SyncingFiles       { get; set; }
    
                    public int       FailedFiles        { get; set; }
    
                    public int       CompletedFiles     { get; set; }
    
                    public long      PendingBytes       { get; set; }
    
                    public string    LastError          { get; set; }
    
                    public DateTime  UpdatedAtUtc       { get; set; } = DateTime.UtcNow;
    
    
        }
    // ═══ Class: JournalSyncStatusSnapshot (from 1 sources) ═══
        public sealed partial class JournalSyncStatusSnapshot
        {
            // --- Properties ---
                    public string ATM_ID { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public DateTime? LastHeartbeatUtc { get; set; }
    
                    public DateTime? LastJournalSyncUtc { get; set; }
    
                    public int PendingFiles { get; set; }
    
                    public int SyncingFiles { get; set; }
    
                    public int FailedFiles { get; set; }
    
                    public int CompletedFiles { get; set; }
    
                    public long PendingBytes { get; set; }
    
                    public string LastError { get; set; }
    
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Enum: JournalTransferStatus (from 2 sources)
        public partial enum JournalTransferStatus
        {
            // --- Constants & Fields ---
                    Received,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v23_bak
                    Archived
    
    
        }
    // Class: LiveSyncProgress (from 1 sources)
        public partial class LiveSyncProgress
        {
            // --- Properties ---
                    public string ATMId           { get; set; }
    
                    public string FileName        { get; set; }
    
                    public string StateLabel      { get; set; }
    
                    public string StateIcon       { get; set; }
    
                    public int    Percent         { get; set; }
    
                    public long   BytesSent       { get; set; }
    
                    public long   TotalBytes      { get; set; }
    
                    public double SpeedKBs        { get; set; }
    
                    public int    SeqNum          { get; set; }
    
                    public int    TotalChunks     { get; set; }
    
                    public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
                    public string BytesSentDisplay => BytesSent > 1048576
                        ? $"{BytesSent / 1048576.0:F1} MB"
                        : $"{BytesSent / 1024.0:F1} KB";
    
                    public string TotalBytesDisplay => TotalBytes > 1048576
                        ? $"{TotalBytes / 1048576.0:F1} MB"
                        : $"{TotalBytes / 1024.0:F1} KB";
    
    
        }
    // Class: LiveSyncProgress (from 3 sources)
        public partial class LiveSyncProgress
        {
            // --- Properties ---
                    public string ATMId           { get; set; }
    
                    public string FileName        { get; set; }
    
                    public string StateLabel      { get; set; }
    
                    public string StateIcon       { get; set; }
    
                    public int    Percent         { get; set; }
    
                    public long   BytesSent       { get; set; }
    
                    public long   TotalBytes      { get; set; }
    
                    public double SpeedKBs        { get; set; }
    
                    public int    SeqNum          { get; set; }
    
                    public int    TotalChunks     { get; set; }
    
                    public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
                    public string BytesSentDisplay => BytesSent > 1048576
                        ? $"{BytesSent / 1048576.0:F1} MB"
                        : $"{BytesSent / 1024.0:F1} KB";
    
                    public string TotalBytesDisplay => TotalBytes > 1048576
                        ? $"{TotalBytes / 1048576.0:F1} MB"
                        : $"{TotalBytes / 1024.0:F1} KB";
    
    
            // --- Nested Classes ---
                public sealed class JournalSyncRecord
                {
                    public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
                    public string ATM_ID { get; set; }
                    public string FileName { get; set; }
                    public string LocalPath { get; set; }
                    public string ArchivePath { get; set; }
                    public string Checksum { get; set; }
                    public string SHA256Hash { get; set; }
                    public long FileSize { get; set; }
                    public long FileOffset { get; set; }
                    public int RetryCount { get; set; }
                    public int ProgressPercent { get; set; }
                    public JournalSyncState State { get; set; } = JournalSyncState.Pending;
                    public string Message { get; set; }
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
                    public DateTime? StartedAtUtc { get; set; }
                    public DateTime? CompletedAtUtc { get; set; }
                    public DateTime? LastAttemptAtUtc { get; set; }
    
                    public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
                }
    
                public sealed class JournalSyncStatusSnapshot
                {
                    public string ATM_ID { get; set; }
                    public bool IsConnected { get; set; }
                    public DateTime? LastHeartbeatUtc { get; set; }
                    public DateTime? LastJournalSyncUtc { get; set; }
                    public int PendingFiles { get; set; }
                    public int SyncingFiles { get; set; }
                    public int FailedFiles { get; set; }
                    public int CompletedFiles { get; set; }
                    public long PendingBytes { get; set; }
                    public string LastError { get; set; }
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
                }
    
                public sealed class JournalSyncAlert
                {
                    public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
                    public string ATM_ID { get; set; }
                    public JournalSyncAlertSeverity Severity { get; set; }
                    public string Title { get; set; }
                    public string Message { get; set; }
                    public string RecommendedAction { get; set; }
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
                }
    
                public sealed class JournalSyncDashboardItem
                {
                    public string ATM_ID { get; set; }
                    public string ATM_Name { get; set; }
                    public string ATM_Type { get; set; }
                    public bool IsConnected { get; set; }
                    public DateTime? LastHeartbeatUtc { get; set; }
                    public DateTime? LastJournalSyncUtc { get; set; }
                    public int PendingFiles { get; set; }
                    public int FailedFiles { get; set; }
                    public int CompletedFiles { get; set; }
                    public long PendingBytes { get; set; }
                    public VendorRootProfileSummary RootProfile { get; set; }
                    public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
                }
    
    
            // --- Nested Enums ---
                public enum JournalSyncState
                {
                    Pending,
                    Syncing,
                    Completed,
                    Failed,
                    ReSyncing,
                    Archived
                }
    
                public enum JournalSyncAlertSeverity
                {
                    Info,
                    Warning,
                    Critical
                }
    
    
        }
    // Class: LiveSyncProgress (from 2 sources)
        public partial class LiveSyncProgress
        {
            // --- Properties ---
                            public string ATMId           { get; set; }
    
                            public string FileName        { get; set; }
    
                            public string StateLabel      { get; set; }
    
                            public string StateIcon       { get; set; }
    
                            public int    Percent         { get; set; }
    
                            public long   BytesSent       { get; set; }
    
                            public long   TotalBytes      { get; set; }
    
                            public double SpeedKBs        { get; set; }
    
                            public int    SeqNum          { get; set; }
    
                            public int    TotalChunks     { get; set; }
    
                            public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
                            public string BytesSentDisplay => BytesSent > 1048576
                                ? $"{BytesSent / 1048576.0:F1} MB"
                                : $"{BytesSent / 1024.0:F1} KB";
    
                            public string TotalBytesDisplay => TotalBytes > 1048576
                                ? $"{TotalBytes / 1048576.0:F1} MB"
                                : $"{TotalBytes / 1024.0:F1} KB";
    
    
        }
    // Class: LiveSyncProgress (from 9 sources)
        public partial class LiveSyncProgress
        {
            // --- Properties ---
                            public string ATMId           { get; set; }
    
                            public string FileName        { get; set; }
    
                            public string StateLabel      { get; set; }
    
                            public string StateIcon       { get; set; }
    
                            public int    Percent         { get; set; }
    
                            public long   BytesSent       { get; set; }
    
                            public long   TotalBytes      { get; set; }
    
                            public double SpeedKBs        { get; set; }
    
                            public int    SeqNum          { get; set; }
    
                            public int    TotalChunks     { get; set; }
    
                            public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
                            public string BytesSentDisplay => BytesSent > 1048576
                                ? $"{BytesSent / 1048576.0:F1} MB"
                                : $"{BytesSent / 1024.0:F1} KB";
    
                            public string TotalBytesDisplay => TotalBytes > 1048576
                                ? $"{TotalBytes / 1048576.0:F1} MB"
                                : $"{TotalBytes / 1024.0:F1} KB";
    
    
            // --- Nested Classes ---
                        public sealed class JournalSyncRecord
                        {
                            public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
                            public string ATM_ID { get; set; }
                            public string FileName { get; set; }
                            public string LocalPath { get; set; }
                            public string ArchivePath { get; set; }
                            public string Checksum { get; set; }
                            public string SHA256Hash { get; set; }
                            public long FileSize { get; set; }
                            public long FileOffset { get; set; }
                            public int RetryCount { get; set; }
                            public int ProgressPercent { get; set; }
                            public JournalSyncState State { get; set; } = JournalSyncState.Pending;
                            public string Message { get; set; }
                            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
                            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
                            public DateTime? StartedAtUtc { get; set; }
                            public DateTime? CompletedAtUtc { get; set; }
                            public DateTime? LastAttemptAtUtc { get; set; }
    
                            public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
                        }
    
                        public sealed class JournalSyncStatusSnapshot
                        {
                            public string ATM_ID { get; set; }
                            public bool IsConnected { get; set; }
                            public DateTime? LastHeartbeatUtc { get; set; }
                            public DateTime? LastJournalSyncUtc { get; set; }
                            public int PendingFiles { get; set; }
                            public int SyncingFiles { get; set; }
                            public int FailedFiles { get; set; }
                            public int CompletedFiles { get; set; }
                            public long PendingBytes { get; set; }
                            public string LastError { get; set; }
                            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
                        }
    
                        public sealed class JournalSyncAlert
                        {
                            public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
                            public string ATM_ID { get; set; }
                            public JournalSyncAlertSeverity Severity { get; set; }
                            public string Title { get; set; }
                            public string Message { get; set; }
                            public string RecommendedAction { get; set; }
                            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
                        }
    
                        public sealed class JournalSyncDashboardItem
                        {
                            public string ATM_ID { get; set; }
                            public string ATM_Name { get; set; }
                            public string ATM_Type { get; set; }
                            public bool IsConnected { get; set; }
                            public DateTime? LastHeartbeatUtc { get; set; }
                            public DateTime? LastJournalSyncUtc { get; set; }
                            public int PendingFiles { get; set; }
                            public int FailedFiles { get; set; }
                            public int CompletedFiles { get; set; }
                            public long PendingBytes { get; set; }
                            public VendorRootProfileSummary RootProfile { get; set; }
                            public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
                        }
    
    
            // --- Nested Enums ---
                        public enum JournalSyncState
                        {
                            Pending,
                            Syncing,
                            Completed,
                            Failed,
                            ReSyncing,
                            Archived
                        }
    
                        public enum JournalSyncAlertSeverity
                        {
                            Info,
                            Warning,
                            Critical
                        }
    
    
        }
    // Class: LiveSyncProgress (from 5 sources)
        public partial class LiveSyncProgress
        {
            // --- Properties ---
                    public string ATMId           { get; set; }
    
                    public string FileName        { get; set; }
    
                    public string StateLabel      { get; set; }
    
                    public string StateIcon       { get; set; }
    
                    public int    Percent         { get; set; }
    
                    public long   BytesSent       { get; set; }
    
                    public long   TotalBytes      { get; set; }
    
                    public double SpeedKBs        { get; set; }
    
                    public int    SeqNum          { get; set; }
    
                    public int    TotalChunks     { get; set; }
    
                    public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
                    public string BytesSentDisplay => BytesSent > 1048576
                        ? $"{BytesSent / 1048576.0:F1} MB"
                        : $"{BytesSent / 1024.0:F1} KB";
    
                    public string TotalBytesDisplay => TotalBytes > 1048576
                        ? $"{TotalBytes / 1048576.0:F1} MB"
                        : $"{TotalBytes / 1024.0:F1} KB";
    
    
            // --- Nested Classes ---
                public sealed class JournalSyncRecord
                {
                    public string SyncId { get; set; } = Guid.NewGuid().ToString("N");
                    public string ATM_ID { get; set; }
                    public string FileName { get; set; }
                    public string LocalPath { get; set; }
                    public string ArchivePath { get; set; }
                    public string Checksum { get; set; }
                    public string SHA256Hash { get; set; }
                    public long FileSize { get; set; }
                    public long FileOffset { get; set; }
                    public int RetryCount { get; set; }
                    public int ProgressPercent { get; set; }
                    public JournalSyncState State { get; set; } = JournalSyncState.Pending;
                    public string Message { get; set; }
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
                    public DateTime? StartedAtUtc { get; set; }
                    public DateTime? CompletedAtUtc { get; set; }
                    public DateTime? LastAttemptAtUtc { get; set; }
    
                    public bool IsFinalState => State == JournalSyncState.Completed || State == JournalSyncState.Archived;
                }
    
                public sealed class JournalSyncStatusSnapshot
                {
                    public string ATM_ID { get; set; }
                    public bool IsConnected { get; set; }
                    public DateTime? LastHeartbeatUtc { get; set; }
                    public DateTime? LastJournalSyncUtc { get; set; }
                    public int PendingFiles { get; set; }
                    public int SyncingFiles { get; set; }
                    public int FailedFiles { get; set; }
                    public int CompletedFiles { get; set; }
                    public long PendingBytes { get; set; }
                    public string LastError { get; set; }
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
                }
    
                public sealed class JournalSyncAlert
                {
                    public string AlertId { get; set; } = Guid.NewGuid().ToString("N");
                    public string ATM_ID { get; set; }
                    public JournalSyncAlertSeverity Severity { get; set; }
                    public string Title { get; set; }
                    public string Message { get; set; }
                    public string RecommendedAction { get; set; }
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
                }
    
                public sealed class JournalSyncDashboardItem
                {
                    public string ATM_ID { get; set; }
                    public string ATM_Name { get; set; }
                    public string ATM_Type { get; set; }
                    public bool IsConnected { get; set; }
                    public DateTime? LastHeartbeatUtc { get; set; }
                    public DateTime? LastJournalSyncUtc { get; set; }
                    public int PendingFiles { get; set; }
                    public int FailedFiles { get; set; }
                    public int CompletedFiles { get; set; }
                    public long PendingBytes { get; set; }
                    public VendorRootProfileSummary RootProfile { get; set; }
                    public List<JournalSyncAlert> Alerts { get; set; } = new List<JournalSyncAlert>();
                }
    
    
            // --- Nested Enums ---
                public enum JournalSyncState
                {
                    Pending,
                    Syncing,
                    Completed,
                    Failed,
                    ReSyncing,
                    Archived
                }
    
                public enum JournalSyncAlertSeverity
                {
                    Info,
                    Warning,
                    Critical
                }
    
    
        }
    // ═══ Class: LiveSyncProgress (from 1 sources) ═══
        public partial class LiveSyncProgress
        {
            // --- Properties ---
                    public string ATMId           { get; set; }
    
                    public string FileName        { get; set; }
    
                    public string StateLabel      { get; set; }
    
                    public string StateIcon       { get; set; }
    
                    public int    Percent         { get; set; }
    
                    public long   BytesSent       { get; set; }
    
                    public long   TotalBytes      { get; set; }
    
                    public double SpeedKBs        { get; set; }
    
                    public int    SeqNum          { get; set; }
    
                    public int    TotalChunks     { get; set; }
    
                    public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    
                    public string BytesSentDisplay => BytesSent > 1048576
                        ? $"{BytesSent / 1048576.0:F1} MB"
                        : $"{BytesSent / 1024.0:F1} KB";
    
                    public string TotalBytesDisplay => TotalBytes > 1048576
                        ? $"{TotalBytes / 1048576.0:F1} MB"
                        : $"{TotalBytes / 1024.0:F1} KB";
    
    
        }
    // Class: RemoteCommand (from 1 sources)
        public partial class RemoteCommand
        {
            // --- Properties ---
                    public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   CommandType   { get; set; }
    
                    public string   TargetATMId   { get; set; }
    
                    public CommandParameters Parameters { get; set; } = new CommandParameters();
    
                    public string   SentBy        { get; set; }
    
                    public bool     RequireConfirm { get; set; }
    
                    public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
                    public DateTime ExecutedAt    { get; set; }
    
                    public string   Status        { get; set; } = "Sent";
    
                    public string   Result        { get; set; }
    
                    public DateTime? AckedAtUtc   { get; set; }
    
                    public int      TimeoutSec    { get; set; } = 30;
    
                    public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
                    public string StatusIcon => Status switch
                    {
                        "Sent"      => "SENT",
                        "Received"  => "RCVD",
                        "Executed"  => "OK",
                        "Failed"    => "FAIL",
                        "Timeout"   => "TIME",
                        _           => "?"
                    };
    
                    public string DisplayLabel =>
                        $"[{CommandId.Substring(0, Math.Min(6, CommandId.Length))}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // --- Methods ---
                    public string GetCommandDescription() => DisplayLabel;
    
    
        }
    // Class: RemoteCommand (from 2 sources)
        public partial class RemoteCommand
        {
            // --- Properties ---
                    public string CommandId { get; set; }
    
                    public string ATMId { get; set; }
    
                    public RemoteCommandType CommandType { get; set; }
    
                    public Dictionary<string, string> Parameters { get; private set; }
    
                    public string Status { get; set; }
    
                    public string Result { get; set; }
    
                    public DateTime CreatedAt { get; set; }
    
                    public DateTime? ExecutedAt { get; set; }
    
    
            // --- Constructors ---
                    public RemoteCommand()
                    {
                        CommandId = Guid.NewGuid().ToString("N");
                        Parameters = new Dictionary<string, string>();
                        CreatedAt = DateTime.UtcNow;
                        Status = "Pending";
                    }
    
    
            // --- Methods ---
                    public string GetCommandDescription()
                    {
                        return CommandType + " for " + (string.IsNullOrEmpty(ATMId) ? "selected ATM" : ATMId);
                    }
    
    
        }
    // Class: RemoteCommand (from 3 sources)
        public partial class RemoteCommand
        {
            // --- Properties ---
                    public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   CommandType   { get; set; }
    
                    public string   TargetATMId   { get; set; }
    
                    public CommandParameters Parameters { get; set; } = new CommandParameters();
    
                    public string   SentBy        { get; set; }
    
                    public bool     RequireConfirm { get; set; }
    
                    public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
                    public DateTime ExecutedAt    { get; set; }
    
                    public string   Status        { get; set; } = "Sent";
    
                    public string   Result        { get; set; }
    
                    public DateTime? AckedAtUtc   { get; set; }
    
                    public int      TimeoutSec    { get; set; } = 30;
    
                    public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
                    public string StatusIcon => Status switch
                    {
                        "Sent"      => "SENT",
                        "Received"  => "RCVD",
                        "Executed"  => "OK",
                        "Failed"    => "FAIL",
                        "Timeout"   => "TIME",
                        _           => "?"
                    };
    
                    public string DisplayLabel =>
                        $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // --- Methods ---
                    public string GetCommandDescription() => DisplayLabel;
    
    
        }
    // Class: RemoteCommand (from 2 sources)
        public partial class RemoteCommand
        {
            // --- Properties ---
                            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
                            public string   CommandType   { get; set; }
    
                            public string   TargetATMId   { get; set; }
    
                            public string   Parameters    { get; set; }
    
                            public string   SentBy        { get; set; }
    
                            public bool     RequireConfirm { get; set; }
    
                            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
                            public string   Status        { get; set; } = "Sent";
    
                            public string   Result        { get; set; }
    
                            public DateTime? AckedAtUtc   { get; set; }
    
                            public int      TimeoutSec    { get; set; } = 30;
    
                            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
                            public string StatusIcon => Status switch
                            {
                                "Sent"      => "📤",
                                "Received"  => "📩",
                                "Executed"  => "✅",
                                "Failed"    => "❌",
                                "Timeout"   => "⏱️",
                                _           => "?"
                            };
    
                            public string DisplayLabel =>
                                $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
        }
    // Class: RemoteCommand (from 2 sources)
        public partial class RemoteCommand
        {
            // --- Properties ---
                    public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   CommandType   { get; set; }
    
                    public string   TargetATMId   { get; set; }
    
                    public CommandParameters Parameters { get; set; } = new CommandParameters();
    
                    public string   SentBy        { get; set; }
    
                    public bool     RequireConfirm { get; set; }
    
                    public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
                    public DateTime ExecutedAt    { get; set; }
    
                    public string   Status        { get; set; } = "Sent";
    
                    public string   Result        { get; set; }
    
                    public DateTime? AckedAtUtc   { get; set; }
    
                    public int      TimeoutSec    { get; set; } = 30;
    
                    public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
                    public string StatusIcon => Status switch
                    {
                        "Sent"      => "SENT",
                        "Received"  => "RCVD",
                        "Executed"  => "OK",
                        "Failed"    => "FAIL",
                        "Timeout"   => "TIME",
                        _           => "?"
                    };
    
                    public string DisplayLabel =>
                        $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // --- Methods ---
                    public string GetCommandDescription() => DisplayLabel;
    
    
        }
    // Class: RemoteCommand (from 9 sources)
        public partial class RemoteCommand
        {
            // --- Properties ---
                            public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
                            public string   CommandType   { get; set; }
    
                            public string   TargetATMId   { get; set; }
    
                            public CommandParameters Parameters { get; set; } = new CommandParameters();
    
                            public string   SentBy        { get; set; }
    
                            public bool     RequireConfirm { get; set; }
    
                            public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
                            public DateTime ExecutedAt    { get; set; }
    
                            public string   Status        { get; set; } = "Sent";
    
                            public string   Result        { get; set; }
    
                            public DateTime? AckedAtUtc   { get; set; }
    
                            public int      TimeoutSec    { get; set; } = 30;
    
                            public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
                            public string StatusIcon => Status switch
                            {
                                "Sent"      => "SENT",
                                "Received"  => "RCVD",
                                "Executed"  => "OK",
                                "Failed"    => "FAIL",
                                "Timeout"   => "TIME",
                                _           => "?"
                            };
    
                            public string DisplayLabel =>
                                $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // --- Methods ---
                            public string GetCommandDescription() => DisplayLabel;
    
    
        }
    // Class: RemoteCommand (from 5 sources)
        public partial class RemoteCommand
        {
            // --- Properties ---
                    public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   CommandType   { get; set; }
    
                    public string   TargetATMId   { get; set; }
    
                    public CommandParameters Parameters { get; set; } = new CommandParameters();
    
                    public string   SentBy        { get; set; }
    
                    public bool     RequireConfirm { get; set; }
    
                    public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
                    public DateTime ExecutedAt    { get; set; }
    
                    public string   Status        { get; set; } = "Sent";
    
                    public string   Result        { get; set; }
    
                    public DateTime? AckedAtUtc   { get; set; }
    
                    public int      TimeoutSec    { get; set; } = 30;
    
                    public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
                    public string StatusIcon => Status switch
                    {
                        "Sent"      => "SENT",
                        "Received"  => "RCVD",
                        "Executed"  => "OK",
                        "Failed"    => "FAIL",
                        "Timeout"   => "TIME",
                        _           => "?"
                    };
    
                    public string DisplayLabel =>
                        $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
            // --- Methods ---
                    public string GetCommandDescription() => DisplayLabel;
    
    
        }
    // ═══ Class: RemoteCommand (from 1 sources) ═══
        public partial class RemoteCommand
        {
            // --- Properties ---
                    public string   CommandId     { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   CommandType   { get; set; }
    
                    public string   TargetATMId   { get; set; }
    
                    public string   Parameters    { get; set; }
    
                    public string   SentBy        { get; set; }
    
                    public bool     RequireConfirm { get; set; }
    
                    public DateTime SentAtUtc     { get; set; } = DateTime.UtcNow;
    
                    public string   Status        { get; set; } = "Sent";
    
                    public string   Result        { get; set; }
    
                    public DateTime? AckedAtUtc   { get; set; }
    
                    public int      TimeoutSec    { get; set; } = 30;
    
                    public bool IsExpired => (DateTime.UtcNow - SentAtUtc).TotalSeconds > TimeoutSec && AckedAtUtc == null;
    
                    public string StatusIcon => Status switch
                    {
                        "Sent"      => "📤",
                        "Received"  => "📩",
                        "Executed"  => "✅",
                        "Failed"    => "❌",
                        "Timeout"   => "⏱️",
                        _           => "?"
                    };
    
                    public string DisplayLabel =>
                        $"[{CommandId.Substring(0,6)}] {CommandType} → {TargetATMId} [{Status}]";
    
    
        }
    // Class: RetainedCard (from 2 sources)
        public sealed partial class RetainedCard
        {
        }
    // Class: SyncStatusInfo (from 1 sources)
        public partial class SyncStatusInfo
        {
            // --- Properties ---
                    public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   ATMId                   { get; set; }
    
                    public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
    
                    public int      TotalFiles              { get; set; }
    
                    public int      SyncedFiles             { get; set; }
    
                    public int      FailedFiles             { get; set; }
    
                    public long     TotalSize               { get; set; }
    
                    public long     SyncedSize              { get; set; }
    
                    public int      ProgressPercentage      { get; set; }
    
                    public double   SyncSpeed               { get; set; }    // KB/s
    
                    public int      EstimatedTimeRemaining  { get; set; }    // ثانية
    
                    public int      RetryCount              { get; set; }
    
                    public string   FailureReason           { get; set; }
    
                    public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
    
                    public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
    
                    public DateTime? CompletedAt            { get; set; }
    
                    public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
    
                    public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
    
                    public string ETADisplay        => EstimatedTimeRemaining < 60
                        ? $"{EstimatedTimeRemaining}ث"
                        : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
                    public string SyncedSizeDisplay =>
                        SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                        SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                        $"{SyncedSize} B";
    
    
        }
    // Class: SyncStatusInfo (from 2 sources)
        public partial class SyncStatusInfo
        {
            // --- Properties ---
                    public string ATMId { get; set; }
    
                    public string FileName { get; set; }
    
                    public JournalSyncState State { get; set; }
    
                    public int ProgressPercent { get; set; }
    
                    public string Message { get; set; }
    
                    public DateTime UpdatedAtUtc { get; set; }
    
    
        }
    // Class: SyncStatusInfo (from 3 sources)
        public partial class SyncStatusInfo
        {
            // --- Properties ---
                    public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   ATMId                   { get; set; }
    
                    public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
    
                    public int      TotalFiles              { get; set; }
    
                    public int      SyncedFiles             { get; set; }
    
                    public int      FailedFiles             { get; set; }
    
                    public long     TotalSize               { get; set; }
    
                    public long     SyncedSize              { get; set; }
    
                    public int      ProgressPercentage      { get; set; }
    
                    public double   SyncSpeed               { get; set; }    // KB/s
    
                    public int      EstimatedTimeRemaining  { get; set; }    // ثانية
    
                    public int      RetryCount              { get; set; }
    
                    public string   FailureReason           { get; set; }
    
                    public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
    
                    public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
    
                    public DateTime? CompletedAt            { get; set; }
    
                    public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
    
                    public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
    
                    public string ETADisplay        => EstimatedTimeRemaining < 60
                        ? $"{EstimatedTimeRemaining}ث"
                        : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
                    public string SyncedSizeDisplay =>
                        SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                        SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                        $"{SyncedSize} B";
    
    
        }
    // Class: SyncStatusInfo (from 2 sources)
        public partial class SyncStatusInfo
        {
            // --- Properties ---
                            public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
    
                            public string   ATMId                   { get; set; }
    
                            public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
    
                            public int      TotalFiles              { get; set; }
    
                            public int      SyncedFiles             { get; set; }
    
                            public int      FailedFiles             { get; set; }
    
                            public long     TotalSize               { get; set; }
    
                            public long     SyncedSize              { get; set; }
    
                            public int      ProgressPercentage      { get; set; }
    
                            public double   SyncSpeed               { get; set; }    // KB/s
    
                            public int      EstimatedTimeRemaining  { get; set; }    // ثانية
    
                            public int      RetryCount              { get; set; }
    
                            public string   FailureReason           { get; set; }
    
                            public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
    
                            public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
    
                            public DateTime? CompletedAt            { get; set; }
    
                            public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
    
                            public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
    
                            public string ETADisplay        => EstimatedTimeRemaining < 60
                                ? $"{EstimatedTimeRemaining}ث"
                                : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
                            public string SyncedSizeDisplay =>
                                SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                                SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                                $"{SyncedSize} B";
    
    
        }
    // Class: SyncStatusInfo (from 9 sources)
        public partial class SyncStatusInfo
        {
            // --- Properties ---
                            public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
    
                            public string   ATMId                   { get; set; }
    
                            public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
    
                            public int      TotalFiles              { get; set; }
    
                            public int      SyncedFiles             { get; set; }
    
                            public int      FailedFiles             { get; set; }
    
                            public long     TotalSize               { get; set; }
    
                            public long     SyncedSize              { get; set; }
    
                            public int      ProgressPercentage      { get; set; }
    
                            public double   SyncSpeed               { get; set; }    // KB/s
    
                            public int      EstimatedTimeRemaining  { get; set; }    // ثانية
    
                            public int      RetryCount              { get; set; }
    
                            public string   FailureReason           { get; set; }
    
                            public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
    
                            public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
    
                            public DateTime? CompletedAt            { get; set; }
    
                            public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
    
                            public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
    
                            public string ETADisplay        => EstimatedTimeRemaining < 60
                                ? $"{EstimatedTimeRemaining}ث"
                                : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
                            public string SyncedSizeDisplay =>
                                SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                                SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                                $"{SyncedSize} B";
    
    
        }
    // Class: SyncStatusInfo (from 5 sources)
        public partial class SyncStatusInfo
        {
            // --- Properties ---
                    public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   ATMId                   { get; set; }
    
                    public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
    
                    public int      TotalFiles              { get; set; }
    
                    public int      SyncedFiles             { get; set; }
    
                    public int      FailedFiles             { get; set; }
    
                    public long     TotalSize               { get; set; }
    
                    public long     SyncedSize              { get; set; }
    
                    public int      ProgressPercentage      { get; set; }
    
                    public double   SyncSpeed               { get; set; }    // KB/s
    
                    public int      EstimatedTimeRemaining  { get; set; }    // ثانية
    
                    public int      RetryCount              { get; set; }
    
                    public string   FailureReason           { get; set; }
    
                    public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
    
                    public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
    
                    public DateTime? CompletedAt            { get; set; }
    
                    public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
    
                    public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
    
                    public string ETADisplay        => EstimatedTimeRemaining < 60
                        ? $"{EstimatedTimeRemaining}ث"
                        : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
                    public string SyncedSizeDisplay =>
                        SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                        SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                        $"{SyncedSize} B";
    
    
        }
    // ═══ Class: SyncStatusInfo (from 1 sources) ═══
        public partial class SyncStatusInfo
        {
            // --- Properties ---
                    public string   SyncId                  { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string   ATMId                   { get; set; }
    
                    public string   Status                  { get; set; }    // Pending/Syncing/Completed/Failed
    
                    public int      TotalFiles              { get; set; }
    
                    public int      SyncedFiles             { get; set; }
    
                    public int      FailedFiles             { get; set; }
    
                    public long     TotalSize               { get; set; }
    
                    public long     SyncedSize              { get; set; }
    
                    public int      ProgressPercentage      { get; set; }
    
                    public double   SyncSpeed               { get; set; }    // KB/s
    
                    public int      EstimatedTimeRemaining  { get; set; }    // ثانية
    
                    public int      RetryCount              { get; set; }
    
                    public string   FailureReason           { get; set; }
    
                    public DateTime StartedAt               { get; set; } = DateTime.UtcNow;
    
                    public DateTime LastUpdated             { get; set; } = DateTime.UtcNow;
    
                    public DateTime? CompletedAt            { get; set; }
    
                    public string ProgressDisplay   => $"{SyncedFiles}/{TotalFiles} ملف ({ProgressPercentage}%)";
    
                    public string SpeedDisplay      => $"{SyncSpeed:F1} KB/s";
    
                    public string ETADisplay        => EstimatedTimeRemaining < 60
                        ? $"{EstimatedTimeRemaining}ث"
                        : $"{EstimatedTimeRemaining / 60}د {EstimatedTimeRemaining % 60}ث";
    
                    public string SyncedSizeDisplay =>
                        SyncedSize > 1048576 ? $"{SyncedSize / 1048576.0:F1} MB" :
                        SyncedSize > 1024    ? $"{SyncedSize / 1024.0:F1} KB"    :
                        $"{SyncedSize} B";
    
    
        }
    // Class: TransactionAnalysisReport (from 2 sources)
        public sealed partial class TransactionAnalysisReport
        {
        }
    // Class: VendorRootProfileSummary (from 1 sources)
        public sealed partial class VendorRootProfileSummary
        {
            // --- Properties ---
                    public string Vendor { get; set; }
    
                    public string Model { get; set; }
    
                    public string EjPattern { get; set; }
    
                    public string LogPattern { get; set; }
    
    
        }

    public partial class ATMDetailedStatus
        {
        }
    public partial class ATMError
        {
        }
    public partial enum ATMOperationalState
        {
        }
    public partial class ATMTransaction
        {
        }
    public partial class CashStatus
        {
        }
    public partial class GhostSession
        {
        }
    public partial class ImageSyncItem
        {
        }
    public partial enum JournalSyncAlertSeverity
        {
            Info,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v15_bak
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v16_bak
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v17_bak
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v20_bak
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v22_bak
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
        }
    public partial enum JournalSyncAlertSeverity
        {
            Info,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v20_bak
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v17_bak
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v16_bak
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v15_bak
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v22_bak
            Critical
    
    
        }
    public partial enum JournalSyncAlertSeverity
        {
            Info,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_operational\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
        }
    public partial enum JournalSyncAlertSeverity
        {
            Info,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Updated_20260512\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
        }
    public partial enum JournalSyncAlertSeverity
        {
            Info,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
        }
    public partial enum JournalSyncAlertSeverity
        {
            Info,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
        }
    public partial enum JournalSyncAlertSeverity
        {
            Info,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Models\JournalSyncModels.cs
            Critical
    
    
        }
    public enum JournalSyncAlertSeverity
        {
            Info,
            Warning,
            Critical
        }
    public partial enum JournalSyncAlertSeverity
        {
            Info,
            Critical
        }
    public partial class JournalSyncEntry
        {
        }
    public partial class JournalSyncModels
        {
        }
    public class JournalSyncModels { }
    public partial enum JournalSyncState
        {
            Pending,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            Pending   = 0,
    
    
            Syncing   = 1,
    
    
            ReSyncing = 2,
    
    
            Completed = 3,
    
    
            Failed    = 4,
    
    
            Archived  = 5
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            LocalSaving = 0,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Pending = 1,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Syncing = 2,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            ReSyncing = 3,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Completed = 4,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Failed = 5,
    
    
            StoredOnServer = 6,
    
    
            Acknowledged = 7
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Pending = 1,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Syncing = 2,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            ReSyncing = 3,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Completed = 4,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Failed = 5,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            Pending = 1,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            Syncing = 2,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            ReSyncing = 3,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            Completed = 4,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            Failed = 5,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Pending = 1,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Syncing = 2,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            ReSyncing = 3,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Completed = 4,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Failed = 5,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Pending = 1,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Syncing = 2,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            ReSyncing = 3,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Completed = 4,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Failed = 5,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Pending = 1,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Syncing = 2,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            ReSyncing = 3,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Completed = 4,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-17\EJLive_Enterprise_v3.4.0_Enhanced_Work\EJLive.Core\Models\JournalSyncModels.cs
            Failed = 5,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Pending = 1,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Syncing = 2,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            ReSyncing = 3,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Completed = 4,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-23\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Failed = 5,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Pending = 1,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Syncing = 2,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            ReSyncing = 3,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Completed = 4,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-24\EJLiveWorkCoder\EJLive.Core\Models\JournalSyncModels.cs
            Failed = 5,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v15_bak
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v16_bak
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v17_bak
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v20_bak
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v22_bak
            Archived
    
    
        }
    public partial enum JournalSyncState
        {
            Pending,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v20_bak
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v17_bak
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v16_bak
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v15_bak
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v22_bak
            Archived
    
    
        }
    public partial enum JournalSyncState
        {
            LocalSaving = 0,
    
    
            Pending = 1,
    
    
            Syncing = 2,
    
    
            ReSyncing = 3,
    
    
            Completed = 4,
    
    
            Failed = 5,
    
    
            StoredOnServer = 6,
    
    
            Acknowledged = 7
    
    
        }
    public partial enum JournalSyncState
        {
            Pending,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_operational\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalSyncState
        {
            Pending,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Updated_20260512\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalSyncState
        {
            Pending,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalSyncState
        {
            Pending   = 0,
    
    
            Syncing   = 1,
    
    
            ReSyncing = 2,
    
    
            Completed = 3,
    
    
            Failed    = 4,
    
    
            Archived  = 5
    
    
        }
    // These enums are used throughout the sync models.
        public enum JournalSyncState
        {
            Pending,
            Syncing,
            Completed,
            Failed,
            ReSyncing,
            Archived
        }
    public enum JournalSyncState
        {
            Pending,
            Syncing,
            Completed,
            Failed,
            ReSyncing,
            Archived
        }
    public enum JournalSyncState
        {
            LocalSaving = 0,
            Pending = 1,
            Syncing = 2,
            ReSyncing = 3,
            Completed = 4,
            Failed = 5,
            StoredOnServer = 6,
            Acknowledged = 7
        }
    public partial enum JournalSyncState
        {
            Pending   = 0,
            Syncing   = 1,
            ReSyncing = 2,
            Completed = 3,
            Failed    = 4,
            Archived  = 5
            Pending,
            Archived
            LocalSaving = 0,
            Pending = 1,
            Syncing = 2,
            ReSyncing = 3,
            Completed = 4,
            Failed = 5,
            StoredOnServer = 6,
            Acknowledged = 7
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-26\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-28\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v23_bak
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs.v17_bak
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs.v21_bak
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs.v23_bak
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v23_bak
            Disconnected
    
    
        }
    // ==========================================
        // Stream-oriented models from EJLive.Core.Sync
        // ==========================================
    
        [Serializable]
        public enum JournalSyncStatus
        {
            Unknown,
            ConnectedIdle,
            Syncing,
            SyncHealthy,
            SyncWarning,
            SyncCritical,
            Disconnected
        }
    // end LiveSyncProgress
    
    
        // ============================================================================
        // Stream-oriented models from EJLive.Core.Sync — merged for unified access
        // ============================================================================
    
                [Serializable]
                public enum JournalSyncStatus
                {
                        Unknown,
                        ConnectedIdle,
                        Syncing,
                        SyncHealthy,
                        SyncWarning,
                        SyncCritical,
                        Disconnected
                }
    public partial enum JournalSyncStatus
        {
            Unknown,
            Disconnected
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-10\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-11\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-13\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-26\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-28\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\JournalSyncModels.cs.v23_bak
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs.v17_bak
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs.v21_bak
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Sync\JournalSyncModels.cs.v23_bak
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\JournalSyncModels.cs.v23_bak
            Archived
    
    
        }
    [Serializable]
        public enum JournalTransferStatus
        {
            Received,
            Failed,
            Archived
        }
    public partial enum JournalTransferStatus
        {
            Received,
            Archived
        }
    public partial class RetainedCard
        {
        }
    public partial class TransactionAnalysisReport
        {
        }
}

namespace EJLive.Core.Sync
{
    public partial class ATMJournalSyncState
        {
            public string ATMId { get; set; }
    
    
            public bool IsConnected { get; set; }
    
    
            public DateTime LastHeartbeatUtc { get; set; }
    
    
            public DateTime LastJournalReceivedUtc { get; set; }
    
    
            public DateTime LastSuccessfulSyncUtc { get; set; }
    
    
            public long TotalJournalBytesReceived { get; set; }
    
    
            public int TotalJournalFilesReceived { get; set; }
    
    
            public int FailedSyncCount { get; set; }
    
    
            public int PendingArchiveCount { get; set; }
    
    
            public string LastStoredFile { get; set; }
    
    
            public string LastChecksum { get; set; }
    
    
            public string LastError { get; set; }
    
    
            public JournalSyncStatus CurrentStatus { get; set; }
    
    
            public ATMJournalSyncState()
            {
                ATMId = "Unknown";
                CurrentStatus = JournalSyncStatus.Unknown;
            }
    
    
        }
    [Serializable]
        public class ATMJournalSyncState
        {
            public string ATMId { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
    
            public ATMJournalSyncState()
            {
                ATMId = "Unknown";
                CurrentStatus = JournalSyncStatus.Unknown;
            }
        }
    public partial class JournalSyncAlert
        {
            public string AlertId { get; set; }
    
    
            public string ATMId { get; set; }
    
    
            public string Severity { get; set; }
    
    
            public string Code { get; set; }
    
    
            public string Message { get; set; }
    
    
            public DateTime CreatedAtUtc { get; set; }
    
    
            public JournalSyncAlert()
            {
                AlertId = Guid.NewGuid().ToString("N");
                CreatedAtUtc = DateTime.UtcNow;
                Severity = "Info";
                Code = "sync-info";
            }
    
    
        }
    [Serializable]
        public class JournalSyncAlert
        {
            public string AlertId { get; set; }
            public string ATMId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
    
            public JournalSyncAlert()
            {
                AlertId = Guid.NewGuid().ToString("N");
                CreatedAtUtc = DateTime.UtcNow;
                Severity = "Info";
                Code = "sync-info";
            }
        }
    public partial class JournalSyncDashboardSnapshot
        {
            public int TotalAtms { get; set; }
    
    
            public int ConnectedAtms { get; set; }
    
    
            public int WarningAtms { get; set; }
    
    
            public int CriticalAtms { get; set; }
    
    
            public int TotalTransfers { get; set; }
    
    
            public int FailedTransfers { get; set; }
    
    
            public List<ATMJournalSyncState> AtmStates { get; set; }
    
    
            public List<JournalSyncEntry> RecentTransfers { get; set; }
    
    
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
    
            public JournalSyncDashboardSnapshot()
            {
                AtmStates = new List<ATMJournalSyncState>();
                RecentTransfers = new List<JournalSyncEntry>();
                ActiveAlerts = new List<JournalSyncAlert>();
            }
    
    
        }
    public class JournalSyncDashboardSnapshot
        {
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
            public JournalSyncDashboardSnapshot()
            {
                AtmStates = new List<ATMJournalSyncState>();
                RecentTransfers = new List<JournalSyncEntry>();
                ActiveAlerts = new List<JournalSyncAlert>();
            }
        }
    public partial class JournalSyncEntry
        {
            public string EntryId { get; set; }
    
    
            public string ATMId { get; set; }
    
    
            public string FileName { get; set; }
    
    
            public string RelativeStoragePath { get; set; }
    
    
            public long FileSizeBytes { get; set; }
    
    
            public string Checksum { get; set; }
    
    
            public DateTime ReceivedAtUtc { get; set; }
    
    
            public JournalTransferStatus Status { get; set; }
    
    
            public string ErrorMessage { get; set; }
    
    
            public JournalSyncEntry()
            {
                EntryId = Guid.NewGuid().ToString("N");
                ReceivedAtUtc = DateTime.UtcNow;
                Status = JournalTransferStatus.Received;
            }
    
    
        }
    [Serializable]
        public class JournalSyncEntry
        {
            public string EntryId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public string Checksum { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
    
            public JournalSyncEntry()
            {
                EntryId = Guid.NewGuid().ToString("N");
                ReceivedAtUtc = DateTime.UtcNow;
                Status = JournalTransferStatus.Received;
            }
        }
    public partial class JournalSyncStateEnvelope
        {
            public DateTime LastUpdatedUtc { get; set; }
    
    
            public List<ATMJournalSyncState> AtmStates { get; set; }
    
    
            public List<JournalSyncEntry> Transfers { get; set; }
    
    
            public List<JournalSyncAlert> Alerts { get; set; }
    
    
            public JournalSyncStateEnvelope()
            {
                LastUpdatedUtc = DateTime.UtcNow;
                AtmStates = new List<ATMJournalSyncState>();
                Transfers = new List<JournalSyncEntry>();
                Alerts = new List<JournalSyncAlert>();
            }
    
    
        }
    [Serializable]
        public class JournalSyncStateEnvelope
        {
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
    
            public JournalSyncStateEnvelope()
            {
                LastUpdatedUtc = DateTime.UtcNow;
                AtmStates = new List<ATMJournalSyncState>();
                Transfers = new List<JournalSyncEntry>();
                Alerts = new List<JournalSyncAlert>();
            }
        }
    public partial enum JournalSyncStatus
        {
    
        public partial enum JournalTransferStatus
        {
    
        public partial public public class JournalSyncEntry
        {
            public JournalSyncEntry()
            {
            public string EntryId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public string Checksum { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
            public string AlertId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
        public partial public public class ATMJournalSyncState
        {
            public ATMJournalSyncState()
            {
            public string ATMId { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
            public string AlertId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
        public partial public public class JournalSyncAlert
        {
            public JournalSyncAlert()
            {
            public string AlertId { get; set; }
            public string ATMId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
        public partial public public class JournalSyncStateEnvelope
        {
            public JournalSyncStateEnvelope()
            {
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
        public partial public public class JournalSyncDashboardSnapshot
        {
            public JournalSyncDashboardSnapshot()
            {
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
    }
    [Serializable]
        public enum JournalSyncStatus
        {
    
        [Serializable]
        public enum JournalTransferStatus
        {
    
        public partial public class JournalSyncEntry
        {
            public JournalSyncEntry()
            {
            public string EntryId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public string Checksum { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
            public string AlertId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
        public partial public class ATMJournalSyncState
        {
            public ATMJournalSyncState()
            {
            public string ATMId { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
            public string AlertId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
        public partial public class JournalSyncAlert
        {
            public JournalSyncAlert()
            {
            public string AlertId { get; set; }
            public string ATMId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
        public partial public class JournalSyncStateEnvelope
        {
            public JournalSyncStateEnvelope()
            {
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
        public partial public class JournalSyncDashboardSnapshot
        {
            public JournalSyncDashboardSnapshot()
            {
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
    }
    public partial enum JournalSyncStatus
        {
    
        public partial enum JournalTransferStatus
        {
    
        public partial public class JournalSyncEntry
        {
            public JournalSyncEntry()
            {
            public string EntryId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public string Checksum { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
        }
    
        public partial public class ATMJournalSyncState
        {
            public ATMJournalSyncState()
            {
            public string ATMId { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
        }
    
        public partial public class JournalSyncAlert
        {
            public JournalSyncAlert()
            {
            public string AlertId { get; set; }
            public string ATMId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
        }
    
        public partial public class JournalSyncStateEnvelope
        {
            public JournalSyncStateEnvelope()
            {
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
        }
    
        public partial public class JournalSyncDashboardSnapshot
        {
            public JournalSyncDashboardSnapshot()
            {
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
    }
    [Serializable]
        public enum JournalSyncStatus
        {
    
        [Serializable]
        public enum JournalTransferStatus
        {
    
        public partial public class JournalSyncEntry
        {
            public JournalSyncEntry()
            {
            public string EntryId { get; set; }
            public string ATMId { get; set; }
            public string FileName { get; set; }
            public string RelativeStoragePath { get; set; }
            public long FileSizeBytes { get; set; }
            public string Checksum { get; set; }
            public DateTime ReceivedAtUtc { get; set; }
            public JournalTransferStatus Status { get; set; }
            public string ErrorMessage { get; set; }
        }
    
        public partial public class ATMJournalSyncState
        {
            public ATMJournalSyncState()
            {
            public string ATMId { get; set; }
            public bool IsConnected { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastJournalReceivedUtc { get; set; }
            public DateTime LastSuccessfulSyncUtc { get; set; }
            public long TotalJournalBytesReceived { get; set; }
            public int TotalJournalFilesReceived { get; set; }
            public int FailedSyncCount { get; set; }
            public int PendingArchiveCount { get; set; }
            public string LastStoredFile { get; set; }
            public string LastChecksum { get; set; }
            public string LastError { get; set; }
            public JournalSyncStatus CurrentStatus { get; set; }
        }
    
        public partial public class JournalSyncAlert
        {
            public JournalSyncAlert()
            {
            public string AlertId { get; set; }
            public string ATMId { get; set; }
            public string Severity { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAtUtc { get; set; }
        }
    
        public partial public class JournalSyncStateEnvelope
        {
            public JournalSyncStateEnvelope()
            {
            public DateTime LastUpdatedUtc { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> Transfers { get; set; }
            public List<JournalSyncAlert> Alerts { get; set; }
        }
    
        public partial public class JournalSyncDashboardSnapshot
        {
            public JournalSyncDashboardSnapshot()
            {
            public int TotalAtms { get; set; }
            public int ConnectedAtms { get; set; }
            public int WarningAtms { get; set; }
            public int CriticalAtms { get; set; }
            public int TotalTransfers { get; set; }
            public int FailedTransfers { get; set; }
            public List<ATMJournalSyncState> AtmStates { get; set; }
            public List<JournalSyncEntry> RecentTransfers { get; set; }
            public List<JournalSyncAlert> ActiveAlerts { get; set; }
        }
    
    }

    // Class: ATMJournalSyncState (from 6 sources)
        public partial class ATMJournalSyncState
        {
            // --- Properties ---
                    public string ATMId { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public DateTime LastHeartbeatUtc { get; set; }
    
                    public DateTime LastJournalReceivedUtc { get; set; }
    
                    public DateTime LastSuccessfulSyncUtc { get; set; }
    
                    public long TotalJournalBytesReceived { get; set; }
    
                    public int TotalJournalFilesReceived { get; set; }
    
                    public int FailedSyncCount { get; set; }
    
                    public int PendingArchiveCount { get; set; }
    
                    public string LastStoredFile { get; set; }
    
                    public string LastChecksum { get; set; }
    
                    public string LastError { get; set; }
    
                    public JournalSyncStatus CurrentStatus { get; set; }
    
    
            // --- Constructors ---
                    public ATMJournalSyncState()
                    {
                        ATMId = "Unknown";
                        CurrentStatus = JournalSyncStatus.Unknown;
                    }
    
    
        }
    // Class: ATMJournalSyncState (from 3 sources)
        public partial class ATMJournalSyncState
        {
            // --- Properties ---
                    public string ATMId { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public DateTime LastHeartbeatUtc { get; set; }
    
                    public DateTime LastJournalReceivedUtc { get; set; }
    
                    public DateTime LastSuccessfulSyncUtc { get; set; }
    
                    public long TotalJournalBytesReceived { get; set; }
    
                    public int TotalJournalFilesReceived { get; set; }
    
                    public int FailedSyncCount { get; set; }
    
                    public int PendingArchiveCount { get; set; }
    
                    public string LastStoredFile { get; set; }
    
                    public string LastChecksum { get; set; }
    
                    public string LastError { get; set; }
    
                    public JournalSyncStatus CurrentStatus { get; set; }
    
    
            // --- Constructors ---
                    public ATMJournalSyncState()
                    {
                        ATMId = "Unknown";
                        CurrentStatus = JournalSyncStatus.Unknown;
                    }
    
    
        }
    // Class: ATMJournalSyncState (from 2 sources)
        public partial class ATMJournalSyncState
        {
            // --- Properties ---
                    public string ATMId { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public DateTime LastHeartbeatUtc { get; set; }
    
                    public DateTime LastJournalReceivedUtc { get; set; }
    
                    public DateTime LastSuccessfulSyncUtc { get; set; }
    
                    public long TotalJournalBytesReceived { get; set; }
    
                    public int TotalJournalFilesReceived { get; set; }
    
                    public int FailedSyncCount { get; set; }
    
                    public int PendingArchiveCount { get; set; }
    
                    public string LastStoredFile { get; set; }
    
                    public string LastChecksum { get; set; }
    
                    public string LastError { get; set; }
    
                    public JournalSyncStatus CurrentStatus { get; set; }
    
    
            // --- Constructors ---
                    public ATMJournalSyncState()
                    {
                        ATMId = "Unknown";
                        CurrentStatus = JournalSyncStatus.Unknown;
                    }
    
    
        }
    // Class: ATMJournalSyncState (from 9 sources)
        public partial class ATMJournalSyncState
        {
            // --- Properties ---
                            public string ATMId { get; set; }
    
                            public bool IsConnected { get; set; }
    
                            public DateTime LastHeartbeatUtc { get; set; }
    
                            public DateTime LastJournalReceivedUtc { get; set; }
    
                            public DateTime LastSuccessfulSyncUtc { get; set; }
    
                            public long TotalJournalBytesReceived { get; set; }
    
                            public int TotalJournalFilesReceived { get; set; }
    
                            public int FailedSyncCount { get; set; }
    
                            public int PendingArchiveCount { get; set; }
    
                            public string LastStoredFile { get; set; }
    
                            public string LastChecksum { get; set; }
    
                            public string LastError { get; set; }
    
                            public JournalSyncStatus CurrentStatus { get; set; }
    
    
            // --- Constructors ---
                            public ATMJournalSyncState()
                            {
                                ATMId = "Unknown";
                                CurrentStatus = JournalSyncStatus.Unknown;
                            }
    
    
        }
    // Class: ATMJournalSyncState (from 5 sources)
        public partial class ATMJournalSyncState
        {
            // --- Properties ---
                    public string ATMId { get; set; }
    
                    public bool IsConnected { get; set; }
    
                    public DateTime LastHeartbeatUtc { get; set; }
    
                    public DateTime LastJournalReceivedUtc { get; set; }
    
                    public DateTime LastSuccessfulSyncUtc { get; set; }
    
                    public long TotalJournalBytesReceived { get; set; }
    
                    public int TotalJournalFilesReceived { get; set; }
    
                    public int FailedSyncCount { get; set; }
    
                    public int PendingArchiveCount { get; set; }
    
                    public string LastStoredFile { get; set; }
    
                    public string LastChecksum { get; set; }
    
                    public string LastError { get; set; }
    
                    public JournalSyncStatus CurrentStatus { get; set; }
    
    
            // --- Constructors ---
                    public ATMJournalSyncState()
                    {
                        ATMId = "Unknown";
                        CurrentStatus = JournalSyncStatus.Unknown;
                    }
    
    
        }
    // Class: JournalSyncAlert (from 6 sources)
        public partial class JournalSyncAlert
        {
            // --- Properties ---
                    public string AlertId { get; set; }
    
                    public string ATMId { get; set; }
    
                    public string Severity { get; set; }
    
                    public string Code { get; set; }
    
                    public string Message { get; set; }
    
                    public DateTime CreatedAtUtc { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncAlert()
                    {
                        AlertId = Guid.NewGuid().ToString("N");
                        CreatedAtUtc = DateTime.UtcNow;
                        Severity = "Info";
                        Code = "sync-info";
                    }
    
    
        }
    // Class: JournalSyncAlert (from 3 sources)
        public partial class JournalSyncAlert
        {
            // --- Properties ---
                    public string AlertId { get; set; }
    
                    public string ATMId { get; set; }
    
                    public string Severity { get; set; }
    
                    public string Code { get; set; }
    
                    public string Message { get; set; }
    
                    public DateTime CreatedAtUtc { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncAlert()
                    {
                        AlertId = Guid.NewGuid().ToString("N");
                        CreatedAtUtc = DateTime.UtcNow;
                        Severity = "Info";
                        Code = "sync-info";
                    }
    
    
        }
    // Class: JournalSyncAlert (from 2 sources)
        public partial class JournalSyncAlert
        {
            // --- Properties ---
                    public string AlertId { get; set; }
    
                    public string ATMId { get; set; }
    
                    public string Severity { get; set; }
    
                    public string Code { get; set; }
    
                    public string Message { get; set; }
    
                    public DateTime CreatedAtUtc { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncAlert()
                    {
                        AlertId = Guid.NewGuid().ToString("N");
                        CreatedAtUtc = DateTime.UtcNow;
                        Severity = "Info";
                        Code = "sync-info";
                    }
    
    
        }
    // Class: JournalSyncAlert (from 9 sources)
        public partial class JournalSyncAlert
        {
            // --- Properties ---
                            public string AlertId { get; set; }
    
                            public string ATMId { get; set; }
    
                            public string Severity { get; set; }
    
                            public string Code { get; set; }
    
                            public string Message { get; set; }
    
                            public DateTime CreatedAtUtc { get; set; }
    
    
            // --- Constructors ---
                            public JournalSyncAlert()
                            {
                                AlertId = Guid.NewGuid().ToString("N");
                                CreatedAtUtc = DateTime.UtcNow;
                                Severity = "Info";
                                Code = "sync-info";
                            }
    
    
        }
    // Class: JournalSyncAlert (from 5 sources)
        public partial class JournalSyncAlert
        {
            // --- Properties ---
                    public string AlertId { get; set; }
    
                    public string ATMId { get; set; }
    
                    public string Severity { get; set; }
    
                    public string Code { get; set; }
    
                    public string Message { get; set; }
    
                    public DateTime CreatedAtUtc { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncAlert()
                    {
                        AlertId = Guid.NewGuid().ToString("N");
                        CreatedAtUtc = DateTime.UtcNow;
                        Severity = "Info";
                        Code = "sync-info";
                    }
    
    
        }
    // Class: JournalSyncDashboardSnapshot (from 6 sources)
        public partial class JournalSyncDashboardSnapshot
        {
            // --- Properties ---
                    public int TotalAtms { get; set; }
    
                    public int ConnectedAtms { get; set; }
    
                    public int WarningAtms { get; set; }
    
                    public int CriticalAtms { get; set; }
    
                    public int TotalTransfers { get; set; }
    
                    public int FailedTransfers { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> RecentTransfers { get; set; }
    
                    public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncDashboardSnapshot()
                    {
                        AtmStates = new List<ATMJournalSyncState>();
                        RecentTransfers = new List<JournalSyncEntry>();
                        ActiveAlerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Class: JournalSyncDashboardSnapshot (from 3 sources)
        public partial class JournalSyncDashboardSnapshot
        {
            // --- Properties ---
                    public int TotalAtms { get; set; }
    
                    public int ConnectedAtms { get; set; }
    
                    public int WarningAtms { get; set; }
    
                    public int CriticalAtms { get; set; }
    
                    public int TotalTransfers { get; set; }
    
                    public int FailedTransfers { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> RecentTransfers { get; set; }
    
                    public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncDashboardSnapshot()
                    {
                        AtmStates = new List<ATMJournalSyncState>();
                        RecentTransfers = new List<JournalSyncEntry>();
                        ActiveAlerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Class: JournalSyncDashboardSnapshot (from 2 sources)
        public partial class JournalSyncDashboardSnapshot
        {
            // --- Properties ---
                    public int TotalAtms { get; set; }
    
                    public int ConnectedAtms { get; set; }
    
                    public int WarningAtms { get; set; }
    
                    public int CriticalAtms { get; set; }
    
                    public int TotalTransfers { get; set; }
    
                    public int FailedTransfers { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> RecentTransfers { get; set; }
    
                    public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncDashboardSnapshot()
                    {
                        AtmStates = new List<ATMJournalSyncState>();
                        RecentTransfers = new List<JournalSyncEntry>();
                        ActiveAlerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Class: JournalSyncDashboardSnapshot (from 9 sources)
        public partial class JournalSyncDashboardSnapshot
        {
            // --- Properties ---
                            public int TotalAtms { get; set; }
    
                            public int ConnectedAtms { get; set; }
    
                            public int WarningAtms { get; set; }
    
                            public int CriticalAtms { get; set; }
    
                            public int TotalTransfers { get; set; }
    
                            public int FailedTransfers { get; set; }
    
                            public List<ATMJournalSyncState> AtmStates { get; set; }
    
                            public List<JournalSyncEntry> RecentTransfers { get; set; }
    
                            public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
    
            // --- Constructors ---
                            public JournalSyncDashboardSnapshot()
                            {
                                AtmStates = new List<ATMJournalSyncState>();
                                RecentTransfers = new List<JournalSyncEntry>();
                                ActiveAlerts = new List<JournalSyncAlert>();
                            }
    
    
        }
    // Class: JournalSyncDashboardSnapshot (from 5 sources)
        public partial class JournalSyncDashboardSnapshot
        {
            // --- Properties ---
                    public int TotalAtms { get; set; }
    
                    public int ConnectedAtms { get; set; }
    
                    public int WarningAtms { get; set; }
    
                    public int CriticalAtms { get; set; }
    
                    public int TotalTransfers { get; set; }
    
                    public int FailedTransfers { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> RecentTransfers { get; set; }
    
                    public List<JournalSyncAlert> ActiveAlerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncDashboardSnapshot()
                    {
                        AtmStates = new List<ATMJournalSyncState>();
                        RecentTransfers = new List<JournalSyncEntry>();
                        ActiveAlerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Class: JournalSyncEntry (from 5 sources)
        public partial class JournalSyncEntry
        {
            // --- Properties ---
                    public string EntryId { get; set; }
    
                    public string ATMId { get; set; }
    
                    public string FileName { get; set; }
    
                    public string RelativeStoragePath { get; set; }
    
                    public long FileSizeBytes { get; set; }
    
                    public string Checksum { get; set; }
    
                    public DateTime ReceivedAtUtc { get; set; }
    
                    public JournalTransferStatus Status { get; set; }
    
                    public string ErrorMessage { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncEntry()
                    {
                        EntryId = Guid.NewGuid().ToString("N");
                        ReceivedAtUtc = DateTime.UtcNow;
                        Status = JournalTransferStatus.Received;
                    }
    
    
        }
    // Class: JournalSyncEntry (from 3 sources)
        public partial class JournalSyncEntry
        {
            // --- Properties ---
                    public string EntryId { get; set; }
    
                    public string ATMId { get; set; }
    
                    public string FileName { get; set; }
    
                    public string RelativeStoragePath { get; set; }
    
                    public long FileSizeBytes { get; set; }
    
                    public string Checksum { get; set; }
    
                    public DateTime ReceivedAtUtc { get; set; }
    
                    public JournalTransferStatus Status { get; set; }
    
                    public string ErrorMessage { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncEntry()
                    {
                        EntryId = Guid.NewGuid().ToString("N");
                        ReceivedAtUtc = DateTime.UtcNow;
                        Status = JournalTransferStatus.Received;
                    }
    
    
        }
    // Class: JournalSyncEntry (from 2 sources)
        public partial class JournalSyncEntry
        {
            // --- Properties ---
                    public string EntryId { get; set; }
    
                    public string ATMId { get; set; }
    
                    public string FileName { get; set; }
    
                    public string RelativeStoragePath { get; set; }
    
                    public long FileSizeBytes { get; set; }
    
                    public string Checksum { get; set; }
    
                    public DateTime ReceivedAtUtc { get; set; }
    
                    public JournalTransferStatus Status { get; set; }
    
                    public string ErrorMessage { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncEntry()
                    {
                        EntryId = Guid.NewGuid().ToString("N");
                        ReceivedAtUtc = DateTime.UtcNow;
                        Status = JournalTransferStatus.Received;
                    }
    
    
        }
    // Class: JournalSyncEntry (from 9 sources)
        public partial class JournalSyncEntry
        {
            // --- Properties ---
                            public string EntryId { get; set; }
    
                            public string ATMId { get; set; }
    
                            public string FileName { get; set; }
    
                            public string RelativeStoragePath { get; set; }
    
                            public long FileSizeBytes { get; set; }
    
                            public string Checksum { get; set; }
    
                            public DateTime ReceivedAtUtc { get; set; }
    
                            public JournalTransferStatus Status { get; set; }
    
                            public string ErrorMessage { get; set; }
    
    
            // --- Constructors ---
                            public JournalSyncEntry()
                            {
                                EntryId = Guid.NewGuid().ToString("N");
                                ReceivedAtUtc = DateTime.UtcNow;
                                Status = JournalTransferStatus.Received;
                            }
    
    
        }
    // Class: JournalSyncStateEnvelope (from 6 sources)
        public partial class JournalSyncStateEnvelope
        {
            // --- Properties ---
                    public DateTime LastUpdatedUtc { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> Transfers { get; set; }
    
                    public List<JournalSyncAlert> Alerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncStateEnvelope()
                    {
                        LastUpdatedUtc = DateTime.UtcNow;
                        AtmStates = new List<ATMJournalSyncState>();
                        Transfers = new List<JournalSyncEntry>();
                        Alerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Class: JournalSyncStateEnvelope (from 3 sources)
        public partial class JournalSyncStateEnvelope
        {
            // --- Properties ---
                    public DateTime LastUpdatedUtc { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> Transfers { get; set; }
    
                    public List<JournalSyncAlert> Alerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncStateEnvelope()
                    {
                        LastUpdatedUtc = DateTime.UtcNow;
                        AtmStates = new List<ATMJournalSyncState>();
                        Transfers = new List<JournalSyncEntry>();
                        Alerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Class: JournalSyncStateEnvelope (from 2 sources)
        public partial class JournalSyncStateEnvelope
        {
            // --- Properties ---
                    public DateTime LastUpdatedUtc { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> Transfers { get; set; }
    
                    public List<JournalSyncAlert> Alerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncStateEnvelope()
                    {
                        LastUpdatedUtc = DateTime.UtcNow;
                        AtmStates = new List<ATMJournalSyncState>();
                        Transfers = new List<JournalSyncEntry>();
                        Alerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Class: JournalSyncStateEnvelope (from 9 sources)
        public partial class JournalSyncStateEnvelope
        {
            // --- Properties ---
                            public DateTime LastUpdatedUtc { get; set; }
    
                            public List<ATMJournalSyncState> AtmStates { get; set; }
    
                            public List<JournalSyncEntry> Transfers { get; set; }
    
                            public List<JournalSyncAlert> Alerts { get; set; }
    
    
            // --- Constructors ---
                            public JournalSyncStateEnvelope()
                            {
                                LastUpdatedUtc = DateTime.UtcNow;
                                AtmStates = new List<ATMJournalSyncState>();
                                Transfers = new List<JournalSyncEntry>();
                                Alerts = new List<JournalSyncAlert>();
                            }
    
    
        }
    // Class: JournalSyncStateEnvelope (from 5 sources)
        public partial class JournalSyncStateEnvelope
        {
            // --- Properties ---
                    public DateTime LastUpdatedUtc { get; set; }
    
                    public List<ATMJournalSyncState> AtmStates { get; set; }
    
                    public List<JournalSyncEntry> Transfers { get; set; }
    
                    public List<JournalSyncAlert> Alerts { get; set; }
    
    
            // --- Constructors ---
                    public JournalSyncStateEnvelope()
                    {
                        LastUpdatedUtc = DateTime.UtcNow;
                        AtmStates = new List<ATMJournalSyncState>();
                        Transfers = new List<JournalSyncEntry>();
                        Alerts = new List<JournalSyncAlert>();
                    }
    
    
        }
    // Enum: JournalSyncStatus (from 6 sources)
        public partial enum JournalSyncStatus
        {
            // --- Constants & Fields ---
                    Unknown,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v21_bak
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v17_bak
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v23_bak
                    Disconnected
    
    
        }
    // Enum: JournalSyncStatus (from 3 sources)
        public partial enum JournalSyncStatus
        {
            // --- Constants & Fields ---
                    Unknown,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
    
        }
    // Enum: JournalSyncStatus (from 2 sources)
        public partial enum JournalSyncStatus
        {
            // --- Constants & Fields ---
                    Unknown,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
    
        }
    // Enum: JournalSyncStatus (from 2 sources)
        public partial enum JournalSyncStatus
        {
            // --- Constants & Fields ---
                    Unknown,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
    
        }
    // Enum: JournalSyncStatus (from 9 sources)
        public partial enum JournalSyncStatus
        {
            // --- Constants & Fields ---
                            Unknown,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                            Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
    
        }
    // Enum: JournalSyncStatus (from 5 sources)
        public partial enum JournalSyncStatus
        {
            // --- Constants & Fields ---
                    Unknown,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Disconnected
    
    
        }
    // Enum: JournalTransferStatus (from 5 sources)
        public partial enum JournalTransferStatus
        {
            // --- Constants & Fields ---
                    Received,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v21_bak
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v17_bak
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v23_bak
                    Archived
    
    
        }
    // Enum: JournalTransferStatus (from 3 sources)
        public partial enum JournalTransferStatus
        {
            // --- Constants & Fields ---
                    Received,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
    
        }
    // Enum: JournalTransferStatus (from 2 sources)
        public partial enum JournalTransferStatus
        {
            // --- Constants & Fields ---
                    Received,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
    
        }
    // Enum: JournalTransferStatus (from 2 sources)
        public partial enum JournalTransferStatus
        {
            // --- Constants & Fields ---
                    Received,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
    
        }
    // Enum: JournalTransferStatus (from 9 sources)
        public partial enum JournalTransferStatus
        {
            // --- Constants & Fields ---
                            Received,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                            Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
    
        }
    // Enum: JournalTransferStatus (from 5 sources)
        public partial enum JournalTransferStatus
        {
            // --- Constants & Fields ---
                    Received,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
                    Archived
    
    
        }

    public enum JournalSyncAlertSeverity
        {
                Info,
                Warning,
                Critical
            }
    public enum JournalSyncState
        {
                Pending,
                Syncing,
                Completed,
                Failed,
                ReSyncing,
                Archived
            }
    public enum JournalSyncStatus
        {
                Unknown,
                ConnectedIdle,
                Syncing,
                SyncHealthy,
                SyncWarning,
                SyncCritical,
                Disconnected
            }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v21_bak
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v17_bak
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v23_bak
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local-2\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_operational\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Updated_20260512\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveEnterprisev4Unified\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    public partial enum JournalSyncStatus
        {
            Unknown,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Disconnected
    
    
        }
    [Serializable]
        public enum JournalSyncStatus
        {
            Unknown,
            ConnectedIdle,
            Syncing,
            SyncHealthy,
            SyncWarning,
            SyncCritical,
            Disconnected
        }
    public enum JournalTransferStatus
        {
                Received,
                Failed,
                Archived
            }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v21_bak
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v17_bak
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Sync\JournalSyncModels.cs.v23_bak
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local-2\VBCode_local\CodexMarege\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_operational\EJLive_Enterprise_operational_fixes_2026-05-12\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Updated_20260512\EJLive_Enterprise_Updated_20260512_build1\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveEnterprisev4Unified\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLiveEnterprisev4Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Current_2026-06-10\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    public partial enum JournalTransferStatus
        {
            Received,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Core\Sync\JournalSyncModels.cs
            Archived
    
    
        }
    [Serializable]
        public enum JournalTransferStatus
        {
            Received,
            Failed,
            Archived
        }
}
