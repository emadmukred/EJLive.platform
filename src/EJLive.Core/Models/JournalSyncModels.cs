using System;
using System.Collections.Generic;


namespace EJLive.Core.Models
{
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

    public class CommandParameters : Dictionary<string, string>
    {
        public string Raw { get; set; } = string.Empty;

        public static implicit operator CommandParameters(string value)
        {
            return new CommandParameters { Raw = value ?? string.Empty };
        }

        public override string ToString() => Raw;
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

    } // end LiveSyncProgress


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

      [Serializable]
      public enum JournalTransferStatus
      {
          Received,
          Failed,
          Archived
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
}
// namespace EJLive.Core.Models
