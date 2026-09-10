using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Business;

public sealed class UnifiedBusinessRuntime : IDisposable
{
    public DatabaseManager Database { get; } = DatabaseManager.Instance;
    public OperationalStateStore OperationalState { get; } = new();
    public JournalSyncTrackingService SyncTracking { get; } = new();
    public JournalSyncService JournalSync { get; } = new();
    public AlertManager Alerts { get; } = new();
    public RoleBasedAccess Access { get; } = new();
    public VendorRootCapabilityService VendorCapabilities { get; } = new();
    public XfsLogAnalysisService XfsAnalysis { get; } = new();
    public TransactionAnalysisEngine TransactionAnalysis { get; } = new();
    public FileWatcherEngine FileWatcher { get; } = new();
    public ImageSyncEngine ImageSync { get; } = new();
    public ReportExportEngine Reports { get; } = new();
    public UnifiedJournalEvidenceAnalyzer JournalEvidence { get; } = new();
    public UnifiedRemoteCommandPolicy RemoteCommandPolicy { get; } = new();
    public UnifiedFleetReadinessService FleetReadiness { get; }
    public UnifiedFileBindingService FileBindings { get; } = new();
    public UnifiedOperationalFusionService OperationalFusion { get; } = new();
    public UnifiedJournalStorageService JournalStorage { get; }
    public UnifiedRemoteCommandOrchestrator RemoteCommands { get; }
    public UnifiedClientServiceSupervisor ClientServiceSupervisor { get; } = new();

    public UnifiedBusinessRuntime()
    {
        FleetReadiness = new UnifiedFleetReadinessService(OperationalState);
        JournalStorage = new UnifiedJournalStorageService(JournalEvidence);
        RemoteCommands = new UnifiedRemoteCommandOrchestrator(RemoteCommandPolicy);
    }

    public static UnifiedBusinessRuntime CreateInitialized(string? databasePath = null)
    {
        var runtime = new UnifiedBusinessRuntime();
        runtime.Database.Initialize(string.IsNullOrWhiteSpace(databasePath) ? AppConstants.DefaultDatabasePath : databasePath);
        return runtime;
    }

    public ATMInfo RegisterAtm(string atmId, string name, string atmType, string? serverIp = null)
    {
        var atm = new ATMInfo
        {
            ATM_ID = atmId,
            ATM_Name = name,
            ATM_Type = AppConstants.NormalizeATMType(atmType),
            ServerIP = serverIp,
            ConnectionStatus = ConnectionStatus.Connected,
            Status = ATMStatus.Online,
            IsConnected = true,
            ConnectedAtUtc = DateTime.UtcNow,
            LastHeartbeatUtc = DateTime.UtcNow,
            LastDataReceivedUtc = DateTime.UtcNow
        };
        atm.RecalculateHealthScore();
        OperationalState.Upsert(atm);
        return atm;
    }

    public JournalSyncRecord TrackJournalSync(string atmId, string fileName, long fileSize, JournalSyncState state)
    {
        var record = new JournalSyncRecord
        {
            ATM_ID = atmId,
            FileName = fileName,
            FileSize = fileSize,
            State = state,
            ProgressPercent = state == JournalSyncState.Completed ? 100 : 0
        };
        SyncTracking.AddOrUpdate(record);
        return record;
    }

    public UnifiedRuntimeSnapshot BuildSnapshot()
    {
        return new UnifiedRuntimeSnapshot(
            OperationalState.BuildSummary(),
            SyncTracking.BuildSummary(),
            Alerts.Alerts.Count,
            BuildCapabilities());
    }

    public IReadOnlyList<FunctionalCapability> BuildCapabilities()
    {
        return new[]
        {
            new FunctionalCapability("Business", "Fleet State", "ATM identity, connection health, heartbeat state, alerts, and fleet readiness snapshots."),
            new FunctionalCapability("Business", "Journal Sync", "Outbox coordination, sync tracking, retry state, progress summaries, and checksum verification."),
            new FunctionalCapability("Business", "Journal Evidence", "Transaction outcome, reversal, fault, and confidence analysis over electronic-journal content."),
            new FunctionalCapability("Business", "Journal Storage", "Validated journal persistence, archive partitioning, checksums, and retrieval workflows."),
            new FunctionalCapability("Business", "Remote Command Governance", "Role checks, operator confirmation, maintenance policy, command tracking, and audit state."),
            new FunctionalCapability("Business", "Client Service Supervision", "Agent registration, heartbeat health, and service-state reporting."),
            new FunctionalCapability("Business", "Vendor Intelligence", "NCR, GRG, Wincor, Diebold, Hyosung, and Cashway paths, XFS events, and diagnostics."),
            new FunctionalCapability("Core", "Protocol and Security", "Authenticated envelopes, encryption, password hashing, checksums, and bounded file-transfer primitives."),
            new FunctionalCapability("Data", "SQLite Store", "ATM state, audit records, journal metadata, migrations, and indexed operational queries."),
            new FunctionalCapability("Application", "Workflow Coordination", "Readiness validation and orchestration over explicit business services.")
        };
    }

    public UnifiedOperationalFusionSnapshot BuildOperationalFusion(
        string journalText,
        RemoteCommand? command = null,
        string role = "Admin",
        bool operatorConfirmed = true,
        bool maintenanceWindow = true)
    {
        return OperationalFusion.Build(
            OperationalState.Snapshot,
            SyncTracking.Records,
            journalText,
            command,
            role,
            operatorConfirmed,
            maintenanceWindow);
    }

    public void Dispose()
    {
        FileWatcher.Dispose();
    }
}

public sealed record FunctionalCapability(string Layer, string Name, string Responsibility);

public sealed record UnifiedRuntimeSnapshot(
    FleetSummary Fleet,
    SyncSummary Sync,
    int ActiveAlerts,
    IReadOnlyList<FunctionalCapability> Capabilities);
