using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Comprehensive ATM device model with component health, cash management,
    /// retained cards tracking, and computed health score.
    /// </summary>
    /// <remarks>
    /// Wave 1 (SS-04 D-08): rewrote the file. The previous version was an auto-merge dump
    /// with three to four copies of <c>ATMDevice</c>, <c>ComponentHealth</c>,
    /// <c>RetainedCardInfo</c> and <c>ComponentStatus</c> interleaved with provenance
    /// comments. <c>HealthScore</c> body was repeated as loose statements (orphan <c>if</c>
    /// expressions without the enclosing <c>get { }</c>), and <c>ComponentStatus</c> was
    /// declared twice (once as a real <c>enum</c>, once as <c>partial enum</c>, which is not
    /// a C# construct). Members kept the union set the variants agreed on. Health-score
    /// formula is the one every variant carried; vendor-aware <c>SyncStatusText</c> entries
    /// for <c>Storing</c> and <c>Paused</c> were present in two copies and are kept (SS-15:
    /// exhaustive over enum, no silent fall-through).
    /// </remarks>
    public class ATMDevice
    {
        public string DeviceId { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
        public string DeviceName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string RegionName { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public ATMType ATMType { get; set; } = ATMType.NCR;
        public ATMStatus Status { get; set; } = ATMStatus.Unknown;
        public ConnectionType ConnectionType { get; set; } = ConnectionType.LAN;
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Idle;
        public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
        public DateTime LastSyncTime { get; set; } = DateTime.MinValue;
        public double LatencyMs { get; set; } = 0;
        public string CurrentVersion { get; set; } = "3.4.0";
        public bool IsGhostMode { get; set; } = false;
        public string GhostOperator { get; set; } = string.Empty;
        public DateTime? GhostStartTime { get; set; }

        // Device Components Health
        public ComponentHealth Printer { get; set; } = new();
        public ComponentHealth CardReader { get; set; } = new();
        public ComponentHealth CashDispenser { get; set; } = new();
        public ComponentHealth ReceiptPrinter { get; set; } = new();
        public ComponentHealth PINPad { get; set; } = new();
        public ComponentHealth Camera { get; set; } = new();
        public ComponentHealth NetworkModule { get; set; } = new();
        public ComponentHealth UPS { get; set; } = new();

        // Cash Management
        public decimal TotalCashLoaded { get; set; } = 0;
        public decimal TotalCashDispensed { get; set; } = 0;
        public decimal RemainingCash { get; set; } = 0;
        public int Cassette1Count { get; set; } = 0;
        public int Cassette2Count { get; set; } = 0;
        public int Cassette3Count { get; set; } = 0;
        public int Cassette4Count { get; set; } = 0;

        // Journal Info
        public string LastJournalFile { get; set; } = string.Empty;
        public long LastJournalSize { get; set; } = 0;
        public DateTime LastJournalDate { get; set; } = DateTime.MinValue;
        public int JournalLinesCount { get; set; } = 0;

        // Retained Cards
        public int RetainedCardsCount { get; set; } = 0;
        public List<RetainedCardInfo> RetainedCards { get; set; } = new();

        /// <summary>
        /// Computed health score (0-100) based on device status, component health, latency, and cash levels.
        /// </summary>
        public int HealthScore
        {
            get
            {
                int score = 100;
                if (Status != ATMStatus.Online && Status != ATMStatus.InService) score -= 40;
                if (SyncStatus == SyncStatus.Failed) score -= 20;
                if (Printer.Status != ComponentStatus.Ok) score -= 5;
                if (CardReader.Status != ComponentStatus.Ok) score -= 10;
                if (CashDispenser.Status != ComponentStatus.Ok) score -= 10;
                if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
                if (LatencyMs > 1000) score -= 10;
                if (RemainingCash < 10000) score -= 15;
                return Math.Max(0, score);
            }
        }

        public string StatusColor => Status switch
        {
            ATMStatus.Online => "#00C853",
            ATMStatus.InService => "#2962FF",
            ATMStatus.Supervisor => "#FF6D00",
            ATMStatus.Fault => "#D50000",
            ATMStatus.Offline => "#616161",
            ATMStatus.Maintenance => "#AA00FF",
            _ => "#9E9E9E"
        };

        public string SyncStatusText => SyncStatus switch
        {
            SyncStatus.Idle => "Idle",
            SyncStatus.Storing => "Storing Journal...",
            SyncStatus.Syncing => "Preparing Sync...",
            SyncStatus.InProgress => "Syncing...",
            SyncStatus.Resyncing => "Resyncing...",
            SyncStatus.Completed => "Synced",
            SyncStatus.Failed => "Failed",
            SyncStatus.Paused => "Paused",
            _ => "Unknown"
        };
    }

    public class ComponentHealth
    {
        public ComponentStatus Status { get; set; } = ComponentStatus.Ok;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    }

    public class RetainedCardInfo
    {
        public string CardNumber { get; set; } = string.Empty;
        public DateTime RetentionTime { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonDescription { get; set; } = string.Empty;
    }

    public enum ComponentStatus
    {
        Ok = 1,
        Warning = 2,
        Error = 3,
        Offline = 4,
        Unknown = 5
    }
}
