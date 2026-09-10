using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
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
    
        // Health Score (0-100)
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
    public partial class ATMDevice
        {
            public int HealthScore
            int score = 100;
    
    
            if (Status != ATMStatus.Online && Status != ATMStatus.InService) score -= 40;
    
    
            if (SyncStatus == SyncStatus.Failed) score -= 20;
    
    
            if (Printer.Status != ComponentStatus.Ok) score -= 5;
    
    
            if (CardReader.Status != ComponentStatus.Ok) score -= 10;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMDevice.cs
            if (LatencyMs > 1000) score -= 10;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\ATMDevice.cs
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
    
            if (RemainingCash < 10000) score -= 15;
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMDevice.cs.v22_bak
            if (LatencyMs > 1000) score -= 10;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMDevice.cs.v22_bak
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMDevice.cs.before_unify
            if (LatencyMs > 1000) score -= 10;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMDevice.cs.before_unify
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
    
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
    
    
            public ComponentHealth Printer { get; set; } = new();
    
    
            public ComponentHealth CardReader { get; set; } = new();
    
    
            public ComponentHealth CashDispenser { get; set; } = new();
    
    
            public ComponentHealth ReceiptPrinter { get; set; } = new();
    
    
            public ComponentHealth PINPad { get; set; } = new();
    
    
            public ComponentHealth Camera { get; set; } = new();
    
    
            public ComponentHealth NetworkModule { get; set; } = new();
    
    
            public ComponentHealth UPS { get; set; } = new();
    
    
            public decimal TotalCashLoaded { get; set; } = 0;
    
    
            public decimal TotalCashDispensed { get; set; } = 0;
    
    
            public decimal RemainingCash { get; set; } = 0;
    
    
            public int Cassette1Count { get; set; } = 0;
    
    
            public int Cassette2Count { get; set; } = 0;
    
    
            public int Cassette3Count { get; set; } = 0;
    
    
            public int Cassette4Count { get; set; } = 0;
    
    
            public string LastJournalFile { get; set; } = string.Empty;
    
    
            public long LastJournalSize { get; set; } = 0;
    
    
            public DateTime LastJournalDate { get; set; } = DateTime.MinValue;
    
    
            public int JournalLinesCount { get; set; } = 0;
    
    
            public int RetainedCardsCount { get; set; } = 0;
    
    
            public List<RetainedCardInfo> RetainedCards { get; set; } = new();
    
    
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
                SyncStatus.Syncing => "Preparing Sync...",
                SyncStatus.InProgress => "Syncing...",
                SyncStatus.Resyncing => "Resyncing...",
                SyncStatus.Completed => "Synced",
                SyncStatus.Failed => "Failed",
                _ => "Unknown"
            };
    
    
            return Math.Max(0, score);
    
    
        }
    /// <summary>
    /// Comprehensive ATM device model with component health, cash management,
    /// retained cards tracking, and computed health score.
    /// </summary>
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
            SyncStatus.Syncing => "Preparing Sync...",
            SyncStatus.InProgress => "Syncing...",
            SyncStatus.Resyncing => "Resyncing...",
            SyncStatus.Completed => "Synced",
            SyncStatus.Failed => "Failed",
            _ => "Unknown"
        };
    }
    public partial class ATMDevice
        {
            public int HealthScore
            int score = 100;
    
    
            if (Status != ATMStatus.Online && Status != ATMStatus.InService) score -= 40;
    
    
            if (SyncStatus == SyncStatus.Failed) score -= 20;
    
    
            if (Printer.Status != ComponentStatus.Ok) score -= 5;
    
    
            if (CardReader.Status != ComponentStatus.Ok) score -= 10;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMDevice.cs
            if (LatencyMs > 1000) score -= 10;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\ATMDevice.cs
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
    
            if (RemainingCash < 10000) score -= 15;
    
    
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
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMDevice.cs
            if (LatencyMs > 1000) score -= 10;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\ATMDevice.cs
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMDevice.cs
            if (LatencyMs > 1000) score -= 10;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\ATMDevice.cs
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMDevice.cs.before_unify
            if (LatencyMs > 1000) score -= 10;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMDevice.cs.before_unify
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMDevice.cs.v22_bak
            if (LatencyMs > 1000) score -= 10;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\ATMDevice.cs.v22_bak
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
    
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
    
    
            public ComponentHealth Printer { get; set; } = new();
    
    
            public ComponentHealth CardReader { get; set; } = new();
    
    
            public ComponentHealth CashDispenser { get; set; } = new();
    
    
            public ComponentHealth ReceiptPrinter { get; set; } = new();
    
    
            public ComponentHealth PINPad { get; set; } = new();
    
    
            public ComponentHealth Camera { get; set; } = new();
    
    
            public ComponentHealth NetworkModule { get; set; } = new();
    
    
            public ComponentHealth UPS { get; set; } = new();
    
    
            public decimal TotalCashLoaded { get; set; } = 0;
    
    
            public decimal TotalCashDispensed { get; set; } = 0;
    
    
            public decimal RemainingCash { get; set; } = 0;
    
    
            public int Cassette1Count { get; set; } = 0;
    
    
            public int Cassette2Count { get; set; } = 0;
    
    
            public int Cassette3Count { get; set; } = 0;
    
    
            public int Cassette4Count { get; set; } = 0;
    
    
            public string LastJournalFile { get; set; } = string.Empty;
    
    
            public long LastJournalSize { get; set; } = 0;
    
    
            public DateTime LastJournalDate { get; set; } = DateTime.MinValue;
    
    
            public int JournalLinesCount { get; set; } = 0;
    
    
            public int RetainedCardsCount { get; set; } = 0;
    
    
            public List<RetainedCardInfo> RetainedCards { get; set; } = new();
    
    
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
                SyncStatus.Syncing => "Preparing Sync...",
                SyncStatus.InProgress => "Syncing...",
                SyncStatus.Resyncing => "Resyncing...",
                SyncStatus.Completed => "Synced",
                SyncStatus.Failed => "Failed",
                _ => "Unknown"
            };
    
    
            return Math.Max(0, score);
    
    
        }
    public class ComponentHealth
    {
        public ComponentStatus Status { get; set; } = ComponentStatus.Ok;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorDescription { get; set; } = string.Empty;
        public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    }
    public partial class ComponentHealth
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
    public partial class RetainedCardInfo
        {
            public string CardNumber { get; set; } = string.Empty;
    
    
            public DateTime RetentionTime { get; set; }
    
    
            public string ReasonCode { get; set; } = string.Empty;
    
    
            public string ReasonDescription { get; set; } = string.Empty;
    
    
        }

    // Class: ATMDevice (from 3 sources)
        public partial class ATMDevice
        {
            // --- Constants & Fields ---
            public int HealthScore
            int score = 100;
    
            if (Status != ATMStatus.Online && Status != ATMStatus.InService) score -= 40;
    
            if (SyncStatus == SyncStatus.Failed) score -= 20;
    
            if (Printer.Status != ComponentStatus.Ok) score -= 5;
    
            if (CardReader.Status != ComponentStatus.Ok) score -= 10;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMDevice.cs.v22_bak
            if (LatencyMs > 1000) score -= 10;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMDevice.cs.v22_bak
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
            if (RemainingCash < 10000) score -= 15;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMDevice.cs.before_unify
            if (LatencyMs > 1000) score -= 10;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\ATMDevice.cs.before_unify
            if (ReceiptPrinter.Status != ComponentStatus.Ok) score -= 5;
    
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
    
    
            // --- Properties ---
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
    
            public ComponentHealth Printer { get; set; } = new();
    
            public ComponentHealth CardReader { get; set; } = new();
    
            public ComponentHealth CashDispenser { get; set; } = new();
    
            public ComponentHealth ReceiptPrinter { get; set; } = new();
    
            public ComponentHealth PINPad { get; set; } = new();
    
            public ComponentHealth Camera { get; set; } = new();
    
            public ComponentHealth NetworkModule { get; set; } = new();
    
            public ComponentHealth UPS { get; set; } = new();
    
            public decimal TotalCashLoaded { get; set; } = 0;
    
            public decimal TotalCashDispensed { get; set; } = 0;
    
            public decimal RemainingCash { get; set; } = 0;
    
            public int Cassette1Count { get; set; } = 0;
    
            public int Cassette2Count { get; set; } = 0;
    
            public int Cassette3Count { get; set; } = 0;
    
            public int Cassette4Count { get; set; } = 0;
    
            public string LastJournalFile { get; set; } = string.Empty;
    
            public long LastJournalSize { get; set; } = 0;
    
            public DateTime LastJournalDate { get; set; } = DateTime.MinValue;
    
            public int JournalLinesCount { get; set; } = 0;
    
            public int RetainedCardsCount { get; set; } = 0;
    
            public List<RetainedCardInfo> RetainedCards { get; set; } = new();
    
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
                    SyncStatus.Syncing => "Preparing Sync...",
                    SyncStatus.InProgress => "Syncing...",
                    SyncStatus.Resyncing => "Resyncing...",
                    SyncStatus.Completed => "Synced",
                    SyncStatus.Failed => "Failed",
                    _ => "Unknown"
                };
    
    
            // --- Methods ---
            return Math.Max(0, score);
    
    
        }
    // Class: ComponentHealth (from 3 sources)
        public partial class ComponentHealth
        {
            // --- Properties ---
                public ComponentStatus Status { get; set; } = ComponentStatus.Ok;
    
                public string ErrorCode { get; set; } = string.Empty;
    
                public string ErrorDescription { get; set; } = string.Empty;
    
                public DateTime LastCheck { get; set; } = DateTime.UtcNow;
    
    
        }
    // Enum: ComponentStatus (from 1 sources)
        public partial enum ComponentStatus
        {
            // --- Constants & Fields ---
                Ok = 1,
    
                Warning = 2,
    
                Error = 3,
    
                Offline = 4,
    
                Unknown = 5
    
    
        }
    // Class: RetainedCardInfo (from 3 sources)
        public partial class RetainedCardInfo
        {
            // --- Properties ---
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
    public partial enum ComponentStatus
        {
            Ok = 1,
    
    
            Warning = 2,
    
    
            Error = 3,
    
    
            Offline = 4,
    
    
            Unknown = 5
    
    
        }
}
