using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    public partial class ServerEngine
    {
    }

    public partial class ReportExportEngine
    {
    }

    public partial class UIBinder
    {
    }

    public partial class OperationalStateStore
    {
    }

    public partial class OperationalReportCatalogService
    {
        public string ATM_ID { get; set; } = "";
        public string Status { get; set; } = "Unknown";
        public int TransactionsToday { get; set; }
        public long CashRemaining { get; set; }
        public int Cassette2 { get; set; }
        public int Cassette3 { get; set; }
        public int Cassette4 { get; set; }
        public long TotalCash { get; set; }
        public string BranchName { get; set; } = "";
        public string Region { get; set; } = "";
        public string Vendor { get; set; } = "";
        public string Network { get; set; } = "LAN";
        public ConnectionStatus ConnectionStatus { get; set; }
        public int HealthScore { get; set; } = 100;
        public bool SupervisorMode { get; set; }
        public int ActiveAlerts { get; set; }
        public string LastTransaction { get; set; } = "";
        public DateTime LastHeartbeatUtc { get; set; }
        public DateTime LastEjSyncUtc { get; set; }
        public TerminalCashStatusCanonical Cash { get; set; } = new();
        public long Cassette1 { get; set; }
        public long Cassette2 { get; set; }
        public long Cassette3 { get; set; }
        public long Cassette4 { get; set; }
        public string Source { get; set; } = "Derived";
        public long Remaining { get; set; }
        public long Loaded { get; set; }
        public long DepositIn { get; set; }
        public long DispenseOut { get; set; }
        public long Reject { get; set; }
        public long Retract { get; set; }
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsEmpty => Remaining <= 0;
        public bool IsLowCash => Remaining > 0 && Remaining < 5000;
        public List<ReportFileEntry> GetLatestReportFiles(string path, int max) => new();
        public BundleSummary LoadLatestBundleSummary(string path) => new();
        public class ReportFileEntry { public string FileName { get; set; } = ""; public string Category { get; set; } = ""; public DateTime ModifiedAtLocal { get; set; } public long SizeBytes { get; set; } }
        public class BundleSummary { public string SourceFilePath { get; set; } = ""; public List<BundleRow> Rows { get; set; } = new(); }
        public class BundleRow { public string Window { get; set; } = ""; public int LookbackHours { get; set; } public int FleetTotal { get; set; } public int FleetConnected { get; set; } public int FleetOffline { get; set; } public int SyncOpen { get; set; } public int SyncFailed { get; set; } public int PendingDelivery { get; set; } public int CommandFailures { get; set; } public int TelemetryWarnings { get; set; } public int TelemetryErrors { get; set; } }
        public class DatabaseManager { public static DatabaseManager Instance { get; } = new(); public System.Data.DataTable GetAuditLog(string? table, DateTime from, DateTime to, int limit) => new(); }
        public class JournalSyncTrackingService { }
        public class AlertManager { }
        public class JournalSyncAlertService { }
        public class JournalSyncMonitorService { }
        public class JournalAnalyticsService { }
        public class RemoteControlService { }
        public class JournalTransferIntelligenceService { }
        public class UnifiedJournalRoutingService { }
        public class UnifiedServerAnalyticsService { }
        public class UnifiedOperationalReportingService { }
        public class ClientTelemetryAnalyticsService { }
        public class VendorRootCapabilityService { }
        public class FleetSummary { public int Total { get; set; } public int Connected { get; set; } public int Syncing { get; set; } public int Offline { get; set; } public double AverageHealth { get; set; } }
        public class VendorRootArtifact { public string ArtifactType { get; set; } = ""; public string RelativePath { get; set; } = ""; public string? Summary { get; set; } }
        public class VendorRootProfile { public string Vendor { get; set; } = ""; public string Model { get; set; } = ""; public string VendorName { get; set; } = ""; public string PlatformLineage { get; set; } = ""; public bool HasFilterIni { get; set; } public bool HasXfsMediaTemplates { get; set; } public bool HasDispenserConfigData { get; set; } public bool HasKeyboardMapData { get; set; } public bool HasKbapeConfig { get; set; } public string FilterHeaderHint { get; set; } = ""; public List<VendorRootArtifact> Artifacts { get; set; } = new(); }
        public class UnifiedServerAnalyticsSnapshot { }
        public class ClientTelemetryAnalyticsSnapshot { }
        public class UnifiedRuntimeSnapshot { }
        public class AuditLogEntry { }
        public class RemoteFramePacket { }
        public class JournalTransferProgressPacket { }
        public class ClientTelemetryPacket { }
        public class JournalFileReceivedPacket { }
        public class ReceivedPacket { }
        public class TerminalLiveSummaryCanonical
        public string TerminalId { get; set; } = "";
        public class TerminalCashStatusCanonical
        public int Cassette1 { get; set; }
    }

}
