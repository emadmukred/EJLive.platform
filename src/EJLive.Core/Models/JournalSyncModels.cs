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
