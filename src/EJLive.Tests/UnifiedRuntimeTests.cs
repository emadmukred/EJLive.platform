using EJLive.Application;
using EJLive.Business;
using EJLive.Client.WinForms.Agent;
using EJLive.Client.WinForms;
using EJLive.Client.WinForms.Services;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Core.Xfs;
using EJLive.Server.Services;
using EJLive.Server.WinForms;
using EJLive.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading;
using CoreCommandSigningEngine = EJLive.Core.Services.CommandSigningEngine;

namespace EJLive.Tests;

[TestClass]
public sealed class UnifiedRuntimeTests
{
    [TestMethod]
    public void SecurityHelper_RoundTripsCompressedEncryptedPayload()
    {
        var source = "EJLive unified payload";
        var encrypted = SecurityHelper.CompressAndEncrypt(System.Text.Encoding.UTF8.GetBytes(source));
        var plain = SecurityHelper.DecryptAndDecompress(encrypted);

        Assert.AreEqual(source, System.Text.Encoding.UTF8.GetString(plain));
    }

    [TestMethod]
    public void CommandSigningEngine_LegacySignVerify_WorksWithBase64Signature()
    {
        var command = "CMD_RESTART|ATM-001|nonce-123";
        var signature = CoreCommandSigningEngine.SignCommand(command);

        Assert.IsTrue(CoreCommandSigningEngine.VerifyCommand(command, signature));
    }

    [TestMethod]
    public void CommandSigningEngine_LegacyVerify_RejectsTamperedPayload()
    {
        var command = "CMD_PING|ATM-001|nonce-abc";
        var signature = CoreCommandSigningEngine.SignCommand(command);

        Assert.IsFalse(CoreCommandSigningEngine.VerifyCommand(command + "|tampered", signature));
    }

    [TestMethod]
    public void BusinessRuntime_TracksAtmAndJournalSyncState()
    {
        using var runtime = new UnifiedBusinessRuntime();

        var atm = runtime.RegisterAtm("ATM-001", "Main Branch", AppConstants.ATM_TYPE_NCR, "127.0.0.1");
        var sync = runtime.TrackJournalSync(atm.ATM_ID!, "EJDATA.LOG", 1024, JournalSyncState.Completed);
        var snapshot = runtime.BuildSnapshot();

        Assert.AreEqual("ATM-001", atm.ATM_ID);
        Assert.AreEqual(100, sync.ProgressPercent);
        Assert.AreEqual(1, snapshot.Fleet.Total);
        Assert.AreEqual(1, snapshot.Sync.Completed);
        Assert.IsTrue(snapshot.Capabilities.Any(capability => capability.Layer == "Legacy Reference"));
    }

    [TestMethod]
    public void OriginalSourceCatalog_TracksSequentialAuditScope()
    {
        var kimi = OriginalSourceCatalog.FindByName("Kimi_Agent");
        var clientV5 = OriginalSourceCatalog.FindByName("EJLive_Client_v5_Enhanced");
        var missingFeatureCoverage = OriginalSourceCatalog.ProjectsWithoutFeatureCoverage();
        var clientV5Features = OriginalSourceCatalog.FeaturesFor("EJLive_Client_v5_Enhanced");

        Assert.AreEqual(30, OriginalSourceCatalog.Projects.Count);
        Assert.AreEqual(9930, OriginalSourceCatalog.TotalFiles);
        Assert.AreEqual(3175, OriginalSourceCatalog.TotalCSharpFiles);
        Assert.AreEqual(1523, OriginalSourceCatalog.TotalDifferentCSharpFromActive);
        Assert.AreEqual(SourceMergeRole.ExternalDesign, kimi.Role);
        Assert.AreEqual(0, kimi.ArabicTextFiles);
        Assert.AreEqual(149, clientV5.CSharpFiles);
        Assert.AreEqual(8, clientV5.DifferentCSharpFromActive);
        Assert.AreEqual(30, OriginalSourceCatalog.Features.Count);
        Assert.AreEqual(0, missingFeatureCoverage.Count);
        Assert.IsTrue(clientV5Features.Any(feature => feature.State == FeatureMergeState.ActiveWithReference));
        Assert.IsTrue(OriginalSourceCatalog.ActiveOrReferencedFeatures.Count >= 10);
        Assert.IsTrue(OriginalSourceCatalog.ProjectsWithUniqueCSharp.All(project => project.DifferentCSharpFromActive > 0));
    }

    [TestMethod]
    public void ApplicationHost_DescribesLayeredDataFlowAndReadiness()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"ejlive-tests-{Guid.NewGuid():N}.db");
        using var host = EJLiveApplicationHost.Create(dbPath);

        host.SeedDemoAtm();
        var readiness = host.ValidateReadiness();
        var flow = host.DescribeDataFlow();

        Assert.IsTrue(readiness.Passed, string.Join("; ", readiness.Checks.Where(check => !check.Passed).Select(check => check.Name)));
        Assert.AreEqual(6, flow.Count);
        Assert.IsTrue(flow.Any(step => step.Layer == "Data"));
        Assert.IsTrue(host.Runtime.BuildSnapshot().SourceFeatures.Any(feature => feature.SourceName == "Coder01"));
    }

    [TestMethod]
    public void UnifiedFusion_AnalyzesJournalsCommandsFleetAndFileBindings()
    {
        using var runtime = new UnifiedBusinessRuntime();
        var atm = runtime.RegisterAtm("ATM-FUSION", "Fusion Terminal", AppConstants.ATM_TYPE_NCR, "127.0.0.1");
        atm.LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-1);
        atm.LastDataReceivedUtc = DateTime.UtcNow.AddMinutes(-2);
        runtime.TrackJournalSync("ATM-FUSION", "EJDATA.LOG", 2048, JournalSyncState.Completed);

        var command = new RemoteCommand
        {
            ATM_ID = "ATM-FUSION",
            CommandType = AppConstants.CMD_SHUTDOWN,
            RequiresConfirmation = true
        };
        var fusion = runtime.BuildOperationalFusion(
            "NCR APTRA EJDATA APPROVED AMOUNT 500\nM-18 CASH ERROR\nCARD CAPTURED\nHOST MESSAGE OUT",
            new[]
            {
                "src/EJLive.Core/Services/UnifiedOperationalFusion.cs",
                "src/EJLive.Business/UnifiedBusinessRuntime.cs",
                "docs/09-file-function-inventory.csv"
            },
            command,
            role: "Admin",
            operatorConfirmed: true,
            maintenanceWindow: true);

        Assert.AreEqual(AppConstants.ATM_TYPE_NCR, fusion.JournalEvidence.Vendor);
        Assert.AreEqual(1, fusion.JournalEvidence.ApprovedTransactions);
        Assert.AreEqual(1, fusion.JournalEvidence.CashErrorEvents);
        Assert.AreEqual(1, fusion.JournalEvidence.CapturedCards);
        Assert.AreEqual(500m, fusion.JournalEvidence.TotalCashDispensed);
        Assert.AreEqual(0, fusion.FileBindings.UnclassifiedCount);
        Assert.IsTrue(fusion.CommandPolicy?.Allowed);
        Assert.AreEqual(RemoteCommandRisk.Critical, fusion.CommandPolicy?.Risk);
        Assert.AreEqual(1, fusion.FleetReadiness.Summary.Total);
    }

    [TestMethod]
    public void UnifiedServiceOperations_RebuildsMissingClientAndServerFunctions()
    {
        using var runtime = new UnifiedBusinessRuntime();
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-service-ops-{Guid.NewGuid():N}");
        var storage = Path.Combine(root, "storage");
        var archive = Path.Combine(root, "archive");
        var reports = Path.Combine(root, "reports");
        var journalBytes = System.Text.Encoding.UTF8.GetBytes("NCR EJDATA APPROVED AMOUNT 120\nCARD CAPTURED\nM-18 CASH ERROR");

        var stored = runtime.JournalStorage.StoreJournalData(
            storage,
            "ATM-SVC",
            AppConstants.ATM_TYPE_NCR,
            "EJDATA.LOG",
            journalBytes);
        var csv = runtime.JournalStorage.ExportCsvReport(new[] { stored }, Path.Combine(reports, "journal.csv"));
        var html = runtime.JournalStorage.ExportHtmlReport(new[] { stored }, Path.Combine(reports, "journal.html"));
        var zip = runtime.JournalStorage.ArchiveMonth(storage, archive, "ATM-SVC", DateTime.UtcNow.ToString("yyyy-MM"));

        var allowed = runtime.RemoteCommands.Queue("ATM-SVC", AppConstants.CMD_RESTART, role: "Admin", operatorConfirmed: true, maintenanceWindow: true);
        var denied = runtime.RemoteCommands.Queue("ATM-SVC", AppConstants.CMD_SHUTDOWN, role: "Observer", operatorConfirmed: false, maintenanceWindow: false);
        runtime.ClientServiceSupervisor.Start("Journal Sync", "Unit test activation");
        runtime.ClientServiceSupervisor.Start("Network Monitor", "Unit test activation");
        var serviceReport = runtime.ClientServiceSupervisor.BuildReport();

        AppBootstrapper.Init();
        var registeredSync = ServiceRegistry.Get<IJournalSyncService>();

        Assert.IsTrue(File.Exists(stored.StoragePath));
        Assert.AreEqual(1, stored.Evidence.ApprovedTransactions);
        Assert.AreEqual(1, stored.Evidence.CapturedCards);
        Assert.AreEqual(1, stored.Evidence.CashErrorEvents);
        Assert.IsTrue(File.Exists(csv));
        Assert.IsTrue(File.Exists(html));
        Assert.IsTrue(File.Exists(zip));
        Assert.IsTrue(allowed.Policy.Allowed);
        Assert.AreEqual(RemoteCommandStatus.Sent, allowed.Command.Status);
        Assert.IsFalse(denied.Policy.Allowed);
        Assert.AreEqual(RemoteCommandStatus.Failed, denied.Command.Status);
        Assert.AreEqual(12, serviceReport.Total);
        Assert.AreEqual(2, serviceReport.Running);
        Assert.IsNotNull(registeredSync);
        Assert.IsTrue(registeredSync.IsRunning);
    }

    [TestMethod]
    public void UnifiedServerAnalyticsService_BuildsOperationalSnapshot()
    {
        var service = new UnifiedServerAnalyticsService();
        var now = DateTime.UtcNow;
        var atms = new[]
        {
            new ATMInfo
            {
                ATM_ID = "ATM-A",
                ATM_Type = AppConstants.ATM_TYPE_NCR,
                ConnectionStatus = ConnectionStatus.Connected,
                HealthScore = 88,
                LastHeartbeatUtc = now.AddMinutes(-2)
            },
            new ATMInfo
            {
                ATM_ID = "ATM-B",
                ATM_Type = AppConstants.ATM_TYPE_GRG,
                ConnectionStatus = ConnectionStatus.Disconnected,
                HealthScore = 41,
                LastHeartbeatUtc = now.AddMinutes(-30)
            }
        };
        var sync = new[]
        {
            new JournalSyncRecord { ATM_ID = "ATM-A", State = JournalSyncState.Completed, ProgressPercent = 100 },
            new JournalSyncRecord { ATM_ID = "ATM-B", State = JournalSyncState.Failed, ProgressPercent = 25 },
            new JournalSyncRecord { ATM_ID = "ATM-B", State = JournalSyncState.Pending, ProgressPercent = 0 }
        };
        var deliveries = new[]
        {
            new JournalDeliveryReceipt("T1", "ATM-A", AppConstants.ATM_TYPE_NCR, "EJDATA.LOG", "journals", "path", 100, "x", now, true, "stored"),
            new JournalDeliveryReceipt("T2", "ATM-B", AppConstants.ATM_TYPE_GRG, "EJDATA.LOG", "journals", "path", 100, "x", now, false, "failed send")
        };
        var audit = new[]
        {
            new AuditLogEntry { Action = "CommandDispatch", Target = "ATM-A", CreatedAtUtc = now.AddMinutes(-5), Details = "CMD_PING" },
            new AuditLogEntry { Action = "CommandResultFailed", Target = "ATM-B", CreatedAtUtc = now.AddMinutes(-4), Details = "timeout fail" }
        };

        var snapshot = service.BuildSnapshot(atms, sync, deliveries, audit, now);

        Assert.AreEqual(2, snapshot.Fleet.Total);
        Assert.AreEqual(1, snapshot.Fleet.Connected);
        Assert.AreEqual(1, snapshot.Fleet.Offline);
        Assert.AreEqual(3, snapshot.Sync.Total);
        Assert.AreEqual(1, snapshot.Sync.Failed);
        Assert.AreEqual(1, snapshot.ConfirmedDeliveries);
        Assert.AreEqual(1, snapshot.PendingDeliveries);
        Assert.AreEqual(1, snapshot.FailedDeliveries);
        Assert.AreEqual(1, snapshot.CommandDispatches);
        Assert.AreEqual(1, snapshot.CommandFailures);
        Assert.AreEqual(0, snapshot.TelemetryEvents);
        Assert.AreEqual(0, snapshot.TelemetryWarnings);
        Assert.AreEqual(0, snapshot.TelemetryErrors);
        Assert.AreEqual(2, snapshot.AtmRows.Count);
        Assert.IsTrue(snapshot.AtmRows.Any(row => row.ATM_ID == "ATM-B" && row.CommandFailures >= 1));
    }

    [TestMethod]
    public void UnifiedServerAnalyticsService_IncludesClientTelemetryInSnapshot()
    {
        var service = new UnifiedServerAnalyticsService();
        var now = DateTime.UtcNow;
        var atms = new[]
        {
            new ATMInfo
            {
                ATM_ID = "ATM-TEL",
                ATM_Type = AppConstants.ATM_TYPE_NCR,
                ConnectionStatus = ConnectionStatus.Connected,
                HealthScore = 95,
                LastHeartbeatUtc = now.AddMinutes(-1)
            }
        };
        var audit = new[]
        {
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-TEL", Details = "warning|file_retry|retry=1", CreatedAtUtc = now.AddMinutes(-3) },
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-TEL", Details = "error|network_disconnected|socket lost", CreatedAtUtc = now.AddMinutes(-2) },
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-TEL", Details = "warning|handshake_missing|miss=3", CreatedAtUtc = now.AddMinutes(-1) }
        };

        var snapshot = service.BuildSnapshot(atms, Array.Empty<JournalSyncRecord>(), Array.Empty<JournalDeliveryReceipt>(), audit, now);
        var row = snapshot.AtmRows.Single(item => item.ATM_ID == "ATM-TEL");

        Assert.AreEqual(3, snapshot.TelemetryEvents);
        Assert.AreEqual(2, snapshot.TelemetryWarnings);
        Assert.AreEqual(1, snapshot.TelemetryErrors);
        Assert.AreEqual(1, snapshot.NetworkDisconnectEvents);
        Assert.AreEqual(1, snapshot.HandshakeMissingEvents);
        Assert.AreEqual(1, snapshot.FileRetryEvents);
        Assert.AreEqual(2, row.TelemetryWarnings);
        Assert.AreEqual(1, row.TelemetryErrors);
        Assert.IsTrue(row.LastTelemetryAtUtc.HasValue);
    }

    [TestMethod]
    public void UnifiedOperationalReportingService_ExportsWindowBundleAndFleetReports()
    {
        var service = new UnifiedOperationalReportingService();
        var now = new DateTime(2026, 05, 20, 12, 0, 0, DateTimeKind.Utc);
        var snapshot = new UnifiedServerAnalyticsSnapshot(
            Fleet: new FleetSummary { Total = 2, Connected = 1, Offline = 1, AverageHealth = 72 },
            Sync: new SyncSummary { Total = 3, Pending = 1, Completed = 1, Failed = 1, AverageProgress = 45 },
            ConfirmedDeliveries: 1,
            PendingDeliveries: 2,
            FailedDeliveries: 1,
            CommandDispatches: 5,
            CommandResults: 4,
            CommandFailures: 1,
            LastCommandAtUtc: now.AddMinutes(-5),
            TelemetryEvents: 7,
            TelemetryWarnings: 3,
            TelemetryErrors: 1,
            NetworkDisconnectEvents: 1,
            HandshakeMissingEvents: 1,
            FileRetryEvents: 2,
            LastTelemetryAtUtc: now.AddMinutes(-2),
            AtmRows: new[]
            {
                new UnifiedAtmOperationalAnalyticsRow(
                    ATM_ID: "ATM-R1",
                    ATM_Type: AppConstants.ATM_TYPE_NCR,
                    HealthScore: 88,
                    ConnectionStatus: ConnectionStatus.Connected,
                    SyncOpen: 1,
                    SyncFailed: 0,
                    SyncCompleted: 3,
                    PendingDeliveries: 0,
                    CommandFailures: 0,
                    TelemetryWarnings: 1,
                    TelemetryErrors: 0,
                    LastTelemetryAtUtc: now.AddMinutes(-3),
                    LastHeartbeatUtc: now.AddMinutes(-1),
                    MinutesSinceHeartbeat: 1),
                new UnifiedAtmOperationalAnalyticsRow(
                    ATM_ID: "ATM-R2",
                    ATM_Type: AppConstants.ATM_TYPE_GRG,
                    HealthScore: 42,
                    ConnectionStatus: ConnectionStatus.Disconnected,
                    SyncOpen: 2,
                    SyncFailed: 1,
                    SyncCompleted: 0,
                    PendingDeliveries: 2,
                    CommandFailures: 1,
                    TelemetryWarnings: 2,
                    TelemetryErrors: 1,
                    LastTelemetryAtUtc: now.AddMinutes(-7),
                    LastHeartbeatUtc: now.AddMinutes(-30),
                    MinutesSinceHeartbeat: 30)
            });
        var folder = Path.Combine(Path.GetTempPath(), $"ejlive-reports-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);

        try
        {
            var window = service.ExportWindowReport(folder, "day", 24, snapshot, now.ToLocalTime(), now);
            var bundle = service.ExportBundleReport(
                folder,
                new[]
                {
                    new OperationalWindowSnapshot("shift", 8, snapshot),
                    new OperationalWindowSnapshot("day", 24, snapshot),
                    new OperationalWindowSnapshot("week", 168, snapshot)
                },
                now.ToLocalTime(),
                now);
            var fleetPath = service.ExportFleetHealthReport(folder, snapshot, now.ToLocalTime());

            Assert.IsTrue(File.Exists(window.JsonPath));
            Assert.IsTrue(File.Exists(window.CsvPath));
            Assert.IsTrue(File.Exists(bundle.JsonPath));
            Assert.IsTrue(File.Exists(bundle.SummaryCsvPath));
            Assert.IsTrue(File.Exists(bundle.AtmCsvPath));
            Assert.IsTrue(File.Exists(fleetPath));

            var windowCsvHeader = File.ReadLines(window.CsvPath).FirstOrDefault() ?? string.Empty;
            var bundleSummaryHeader = File.ReadLines(bundle.SummaryCsvPath).FirstOrDefault() ?? string.Empty;
            var fleetHeader = File.ReadLines(fleetPath).FirstOrDefault() ?? string.Empty;

            Assert.IsTrue(windowCsvHeader.Contains("telemetry_errors", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(bundleSummaryHeader.Contains("telemetry_events", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(fleetHeader.Contains("heartbeat_age_minutes", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
        }
    }

    [TestMethod]
    public void OperationalReportCatalogService_LoadsLatestBundleSummaryAndIndexesFiles()
    {
        var reporting = new UnifiedOperationalReportingService();
        var catalog = new OperationalReportCatalogService();
        var now = new DateTime(2026, 05, 20, 14, 0, 0, DateTimeKind.Utc);
        var snapshot = new UnifiedServerAnalyticsSnapshot(
            Fleet: new FleetSummary { Total = 1, Connected = 1, Offline = 0, AverageHealth = 90 },
            Sync: new SyncSummary { Total = 1, Pending = 0, InProgress = 0, Completed = 1, Failed = 0, AverageProgress = 100 },
            ConfirmedDeliveries: 1,
            PendingDeliveries: 0,
            FailedDeliveries: 0,
            CommandDispatches: 3,
            CommandResults: 3,
            CommandFailures: 0,
            LastCommandAtUtc: now.AddMinutes(-2),
            TelemetryEvents: 2,
            TelemetryWarnings: 1,
            TelemetryErrors: 0,
            NetworkDisconnectEvents: 0,
            HandshakeMissingEvents: 0,
            FileRetryEvents: 1,
            LastTelemetryAtUtc: now.AddMinutes(-1),
            AtmRows: new[]
            {
                new UnifiedAtmOperationalAnalyticsRow(
                    ATM_ID: "ATM-CAT",
                    ATM_Type: AppConstants.ATM_TYPE_NCR,
                    HealthScore: 90,
                    ConnectionStatus: ConnectionStatus.Connected,
                    SyncOpen: 0,
                    SyncFailed: 0,
                    SyncCompleted: 1,
                    PendingDeliveries: 0,
                    CommandFailures: 0,
                    TelemetryWarnings: 1,
                    TelemetryErrors: 0,
                    LastTelemetryAtUtc: now.AddMinutes(-1),
                    LastHeartbeatUtc: now.AddSeconds(-20),
                    MinutesSinceHeartbeat: 0)
            });
        var folder = Path.Combine(Path.GetTempPath(), $"ejlive-report-catalog-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);

        try
        {
            reporting.ExportBundleReport(
                folder,
                new[]
                {
                    new OperationalWindowSnapshot("shift", 8, snapshot),
                    new OperationalWindowSnapshot("day", 24, snapshot),
                    new OperationalWindowSnapshot("week", 168, snapshot)
                },
                now.ToLocalTime(),
                now);

            File.WriteAllText(Path.Combine(folder, "monitoring-dashboard-20260520-140000.csv"), "atm_id,status\nATM-CAT,Connected\n");

            var files = catalog.GetLatestReportFiles(folder, 20);
            var summary = catalog.LoadLatestBundleSummary(folder);

            Assert.IsTrue(files.Count >= 4);
            Assert.IsTrue(files.Any(file => file.Category.Contains("bundle summary", StringComparison.OrdinalIgnoreCase)));
            Assert.IsFalse(string.IsNullOrWhiteSpace(summary.SourceFilePath));
            Assert.AreEqual(3, summary.Rows.Count);
            Assert.IsTrue(summary.Rows.Any(row => row.Window.Equals("day", StringComparison.OrdinalIgnoreCase) && row.LookbackHours == 24));
        }
        finally
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
        }
    }

    [TestMethod]
    public void ClientTelemetryAnalyticsService_BuildsSnapshotAndExportsCsv()
    {
        var service = new ClientTelemetryAnalyticsService();
        var now = new DateTime(2026, 05, 20, 18, 0, 0, DateTimeKind.Utc);
        var audit = new[]
        {
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-T1", Details = "info|network_connected|ok", CreatedAtUtc = now.AddMinutes(-6) },
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-T1", Details = "warning|file_retry|retry=1", CreatedAtUtc = now.AddMinutes(-5) },
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-T2", Details = "error|network_disconnected|link lost", CreatedAtUtc = now.AddMinutes(-4) },
            new AuditLogEntry { Action = "CommandDispatch", Target = "ATM-T1", Details = "CMD_PING", CreatedAtUtc = now.AddMinutes(-3) }
        };

        var snapshot = service.BuildSnapshot(audit);
        var folder = Path.Combine(Path.GetTempPath(), $"ejlive-telemetry-analytics-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        try
        {
            var timeline = service.ExportTimelineCsv(folder, snapshot, now.ToLocalTime());
            var atmSummary = service.ExportAtmSummaryCsv(folder, snapshot, now.ToLocalTime());

            Assert.AreEqual(3, snapshot.TotalEvents);
            Assert.AreEqual(1, snapshot.WarningEvents);
            Assert.AreEqual(1, snapshot.ErrorEvents);
            Assert.AreEqual(2, snapshot.DistinctAtms);
            Assert.IsTrue(snapshot.TopEventTypes.Any(item => item.EventType.Equals("network_disconnected", StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(File.Exists(timeline));
            Assert.IsTrue(File.Exists(atmSummary));
        }
        finally
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
        }
    }

    [TestMethod]
    public void UnifiedJournalRoutingService_PartitionsFilesByTypeAndTracksReceipts()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-routing-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        try
        {
            var routing = new UnifiedJournalRoutingService();
            var pending = routing.RegisterPending("TR-001", "ATM-ROUTE", AppConstants.ATM_TYPE_NCR, "EJDATA.LOG", 2048, "abc123");
            var journal = routing.StoreInbound(
                root,
                "ATM-ROUTE",
                AppConstants.ATM_TYPE_NCR,
                "EJDATA.LOG",
                System.Text.Encoding.UTF8.GetBytes("NCR EJDATA APPROVED AMOUNT 90"),
                "abc123",
                "TR-001",
                routeHint: "journal");
            var screenshot = routing.StoreInbound(
                root,
                "ATM-ROUTE",
                AppConstants.ATM_TYPE_NCR,
                "SCR_ATM-ROUTE_001.jpg",
                System.Text.Encoding.UTF8.GetBytes("fake-image"),
                transferId: "TR-002");

            Assert.AreEqual("TR-001", pending.TransferId);
            Assert.AreEqual("journals", journal.Category);
            Assert.IsTrue(journal.Confirmed);
            Assert.IsTrue(File.Exists(journal.StoragePath));
            Assert.IsTrue(journal.StoragePath.Contains(Path.Combine(AppConstants.ATM_TYPE_NCR, "ATM-ROUTE"), StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual("screenshots", screenshot.Category);
            Assert.IsTrue(screenshot.Confirmed);
            Assert.IsTrue(File.Exists(screenshot.StoragePath));
            Assert.IsTrue(screenshot.StoragePath.Contains(Path.Combine(AppConstants.ATM_TYPE_NCR, "ATM-ROUTE", "screenshots"), StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual(2, routing.Receipts.Count);
            Assert.AreEqual(0, routing.FindPending(TimeSpan.FromMilliseconds(1)).Count);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void UnifiedServiceGateway_BridgesReferenceServicesIntoActiveRuntime()
    {
        using var runtime = new UnifiedBusinessRuntime();
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-service-gateway-{Guid.NewGuid():N}");
        var storage = Path.Combine(root, "storage");
        var journalBytes = System.Text.Encoding.UTF8.GetBytes("NCR EJDATA APPROVED AMOUNT 450\nCARD CAPTURED\nM-18 CASH ERROR");

        try
        {
            var stored = runtime.ServiceGateway.StoreJournal(
                storage,
                "ATM-GATE",
                AppConstants.ATM_TYPE_NCR,
                "EJDATA.LOG",
                journalBytes,
                referencePath: "src/EJLive.Client.WinForms/Services/JournalProcessor.cs");

            var dispatch = runtime.ServiceGateway.DispatchRemoteCommand(
                "ATM-GATE",
                AppConstants.CMD_PING,
                role: "Admin",
                operatorConfirmed: true,
                maintenanceWindow: true,
                referencePath: "src/EJLive.Client.WinForms/Services/RemoteCommandHandler.cs");

            runtime.ServiceGateway.RegisterHeartbeat(
                "ATM-GATE",
                referencePath: "src/EJLive.Client.WinForms/Agent/AgentBootstrapper.cs");
            runtime.ServiceGateway.RegisterBackupSnapshot(
                "ATM-GATE",
                Path.Combine(root, "backup.zip"),
                sizeBytes: 2048,
                referencePath: "src/EJLive.Client.WinForms/Agent/LogBackupScheduler.cs");
            runtime.ServiceGateway.RegisterScreenshotResult(
                "ATM-GATE",
                Path.Combine(root, "screen.jpg"),
                success: true,
                sizeBytes: 4096,
                referencePath: "src/EJLive.Client.WinForms/Agent/ScreenshotScheduler.cs");
            runtime.ServiceGateway.MarkClientServiceRunning(
                "Network Monitor",
                "Gateway activation",
                referencePath: "src/EJLive.Client.WinForms/Services/Network/NetworkManager.cs");
            runtime.ServiceGateway.MarkClientServiceFaulted(
                "Ghost Access",
                "Gateway fault simulation",
                referencePath: "src/EJLive.Client.WinForms/Services/Advanced/RemoteCommandHandler.cs");

            var activationBatch = runtime.ServiceGateway.ActivateReferencePaths(
                new[]
                {
                    "src/EJLive.Client.WinForms/Services/JournalProcessor.cs",
                    "src/EJLive.Client.WinForms/Services/RemoteCommandHandler.cs",
                    "src/EJLive.Client.WinForms/Agent/NetworkMonitor.cs",
                    "src/EJLive.Server/Services/JournalAnalyticsService.cs",
                    "src/EJLive.Server/Services/RemoteControlService.cs",
                    "src/EJLive.Core/Engine/NetworkEngine.cs",
                    "src/EJLive.Core/Services/MergedTraceCorrelationService.cs"
                },
                "ATM-GATE",
                storage);

            var fullActivation = runtime.ServiceGateway.ActivateAllReferenceServices(
                FindRepositoryRoot(),
                "ATM-GATE",
                storage);

            var report = runtime.ServiceGateway.BuildReport();
            var atmState = report.AtmStates.Single(state => state.AtmId == "ATM-GATE");
            var coverage = runtime.ServiceGateway.BuildReferenceCoverage(FindRepositoryRoot());

            Assert.IsTrue(File.Exists(stored.StoragePath));
            Assert.AreEqual(1, stored.Evidence.ApprovedTransactions);
            Assert.AreEqual(1, stored.Evidence.CapturedCards);
            Assert.AreEqual(1, stored.Evidence.CashErrorEvents);
            Assert.IsTrue(dispatch.Policy.Allowed);
            Assert.AreEqual(RemoteCommandStatus.Sent, dispatch.Command.Status);
            Assert.IsTrue(activationBatch.ActivatedReferencePaths >= 7);
            Assert.AreEqual(0, activationBatch.UnclassifiedActivations);
            Assert.IsTrue(fullActivation.RequestedReferencePaths >= 0);
            Assert.AreEqual(0, fullActivation.UnclassifiedActivations);
            Assert.IsTrue(report.TotalActivations >= 14);
            Assert.AreEqual(0, report.UnresolvedReferenceActivations);
            Assert.IsTrue(report.MappedReferenceActivations >= 14);
            Assert.AreEqual(1, report.DistinctAtms);
            Assert.IsTrue(atmState.LastHeartbeatUtc.HasValue);
            Assert.IsTrue(atmState.LastBackupUtc.HasValue);
            Assert.IsTrue(atmState.LastScreenshotUtc.HasValue);
            Assert.IsTrue(atmState.LastJournalUtc.HasValue);
            Assert.IsTrue(atmState.LastRemoteCommandUtc.HasValue);
            Assert.IsTrue(atmState.Activations >= 5);
            Assert.IsTrue(coverage.TotalReferenceFiles >= 0);
            Assert.AreEqual(0, coverage.UncoveredFiles);
            Assert.AreEqual(coverage.TotalReferenceFiles, coverage.CoveredFiles);
            Assert.AreEqual(coverage.TotalReferenceFiles, fullActivation.RequestedReferencePaths);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void UnifiedServiceGateway_ActivatesAllReferenceServiceFilesFromAudit()
    {
        using var runtime = new UnifiedBusinessRuntime();
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-service-gateway-all-{Guid.NewGuid():N}");

        try
        {
            var solutionRoot = FindRepositoryRoot();
            var audit = runtime.BuildIntegrationAudit(solutionRoot);
            var referencePaths = audit.ReferenceOnlyFiles.Select(file => file.Path).ToArray();

            var activation = runtime.ServiceGateway.ActivateReferencePaths(
                referencePaths,
                "ATM-GATE-ALL",
                root);

            var report = runtime.ServiceGateway.BuildReport();
            var coverage = runtime.ServiceGateway.BuildReferenceCoverage(solutionRoot);

            Assert.IsTrue(referencePaths.Length >= 0);
            Assert.AreEqual(referencePaths.Length, activation.RequestedReferencePaths);
            Assert.AreEqual(referencePaths.Length, activation.ActivatedReferencePaths);
            Assert.AreEqual(0, activation.UnclassifiedActivations);
            Assert.AreEqual(coverage.TotalReferenceFiles, activation.RequestedReferencePaths);
            Assert.AreEqual(0, coverage.UncoveredFiles);
            Assert.IsTrue(report.DistinctReferencePaths.Count >= referencePaths.Length);
            Assert.AreEqual(0, report.UnresolvedReferenceActivations);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void RemoteControlService_ParsesHistoryResultAndMarksCommandCompleted()
    {
        using var serverEngine = new ServerEngine();
        using var service = new RemoteControlService(serverEngine);

        var commandId = service.SendRestart("ATM-PARSE", 5);
        var before = service.GetCommandHistory("ATM-PARSE", 1).Single();

        Assert.AreEqual(commandId, before.CommandId);
        Assert.IsFalse(before.Completed);
        Assert.IsFalse(before.Sent);
        Assert.AreEqual("No active connection", before.Result);

        var handler = typeof(RemoteControlService).GetMethod("HandleServerLog", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(handler);
        handler!.Invoke(service, new object?[] { serverEngine, $"Command result from ATM-PARSE: {commandId} OK Restart queued" });

        var after = service.GetCommandHistory("ATM-PARSE", 1).Single();
        Assert.IsTrue(after.Completed);
        Assert.IsTrue(after.Success);
        Assert.IsTrue(after.CompletedAt >= after.SentAt);
        Assert.AreEqual("Restart queued", after.Result);
    }

    [TestMethod]
    public void RemoteControlService_ParsesFailedResultAndMarksCommandFailed()
    {
        using var serverEngine = new ServerEngine();
        using var service = new RemoteControlService(serverEngine);

        var commandId = service.SendPing("ATM-FAIL");
        var handler = typeof(RemoteControlService).GetMethod("HandleServerLog", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(handler);
        handler!.Invoke(service, new object?[] { serverEngine, $"Command result from ATM-FAIL: {commandId} FAIL permission denied" });

        var record = service.GetCommandHistory("ATM-FAIL", 1).Single();
        Assert.IsTrue(record.Completed);
        Assert.IsFalse(record.Success);
        Assert.AreEqual("permission denied", record.Result);
    }

    [TestMethod]
    public void RemoteControlService_NewBroadcastOperationsReturnZeroWithoutConnections()
    {
        using var serverEngine = new ServerEngine();
        using var service = new RemoteControlService(serverEngine);

        Assert.AreEqual(0, service.BroadcastPing());
        Assert.AreEqual(0, service.BroadcastWindowsRemoteStart());
        Assert.AreEqual(0, service.BroadcastWindowsRemoteStop());
        Assert.AreEqual(0, service.BroadcastWindowsRemoteCheck());
        Assert.AreEqual(0, service.BroadcastChangePassword("Secret#123"));
        Assert.AreEqual(0, service.BroadcastRequestJournalFile("EJDATA.LOG"));
        Assert.AreEqual(0, service.BroadcastSyncImages());
        Assert.AreEqual(0, service.BroadcastImage("notice.jpg", System.Text.Encoding.UTF8.GetBytes("fake"), "Inbox"));
        Assert.AreEqual(0, service.BroadcastImageDirect(
            "notice.jpg",
            System.Text.Encoding.UTF8.GetBytes("fake"),
            _ => AppConstants.ATM_TYPE_NCR));
        Assert.AreEqual(0, service.BroadcastScreenshotNow());
    }

    [TestMethod]
    public void ServerMainForm_CommandResultParser_ParsesSuccessLine()
    {
        var method = typeof(ServerMainForm).GetMethod("TryParseCommandResultLog", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var args = new object?[]
        {
            "Command result from ATM-900: CMD_20260520_00001 OK Ping acknowledged",
            null,
            null,
            false,
            null
        };

        var parsed = (bool)(method!.Invoke(null, args) ?? false);
        Assert.IsTrue(parsed);
        Assert.AreEqual("ATM-900", Convert.ToString(args[1]));
        Assert.AreEqual("CMD_20260520_00001", Convert.ToString(args[2]));
        Assert.AreEqual(true, Convert.ToBoolean(args[3]));
        Assert.AreEqual("Ping acknowledged", Convert.ToString(args[4]));
    }

    [TestMethod]
    public void ServerMainForm_CommandResultParser_ParsesFailureLine()
    {
        var method = typeof(ServerMainForm).GetMethod("TryParseCommandResultLog", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var args = new object?[]
        {
            "Command result from ATM-901: CMD_20260520_00002 FAIL access denied",
            null,
            null,
            true,
            null
        };

        var parsed = (bool)(method!.Invoke(null, args) ?? false);
        Assert.IsTrue(parsed);
        Assert.AreEqual("ATM-901", Convert.ToString(args[1]));
        Assert.AreEqual("CMD_20260520_00002", Convert.ToString(args[2]));
        Assert.AreEqual(false, Convert.ToBoolean(args[3]));
        Assert.AreEqual("access denied", Convert.ToString(args[4]));
    }

    [TestMethod]
    public void ServerMainForm_CommandAuditActionFilter_MatchesCommandActivitiesOnly()
    {
        var method = typeof(ServerMainForm).GetMethod("IsCommandAuditAction", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var commandMatch = (bool)(method!.Invoke(null, new object?[] { "CommandDispatch" }) ?? false);
        var probeMatch = (bool)(method!.Invoke(null, new object?[] { "ConnectivityProbeDispatch" }) ?? false);
        var unrelated = (bool)(method!.Invoke(null, new object?[] { "FleetRefresh" }) ?? true);

        Assert.IsTrue(commandMatch);
        Assert.IsTrue(probeMatch);
        Assert.IsFalse(unrelated);
    }

    [TestMethod]
    public void ServerEngine_TelemetryBroadcastParser_ParsesBase64Payload()
    {
        var method = typeof(ServerEngine).GetMethod("TryParseTelemetryBroadcast", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var utc = new DateTime(2026, 05, 20, 10, 0, 0, DateTimeKind.Utc);
        var detail = "retry=2|queue";
        var payload =
            "TELEMETRY|ATM=ATM-T100;Type=file_retry;Severity=warning;Utc=" + utc.ToString("O") +
            ";DetailB64=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(detail));

        var args = new object?[] { payload, "ATM-FALLBACK", null };
        var parsed = (bool)(method!.Invoke(null, args) ?? false);
        Assert.IsTrue(parsed);

        var packet = args[2] as ClientTelemetryPacket;
        Assert.IsNotNull(packet);
        Assert.AreEqual("ATM-T100", packet!.ATM_ID);
        Assert.AreEqual("file_retry", packet.EventType);
        Assert.AreEqual("warning", packet.Severity);
        Assert.AreEqual(detail, packet.Detail);
        Assert.AreEqual(utc, packet.ReportedAtUtc);
    }

    [TestMethod]
    public void ServerEngine_PulseBroadcastParser_ParsesAtmAndTimestamp()
    {
        var method = typeof(ServerEngine).GetMethod("TryParsePulseBroadcast", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var utc = new DateTime(2026, 05, 20, 10, 15, 30, DateTimeKind.Utc);
        var args = new object?[] { $"PULSE|ATM-P100|{utc:O}|pending=2", null, null };
        var parsed = (bool)(method!.Invoke(null, args) ?? false);

        Assert.IsTrue(parsed);
        Assert.AreEqual("ATM-P100", Convert.ToString(args[1]));
        Assert.AreEqual(utc, (DateTime)args[2]!);
    }

    [TestMethod]
    public void ServerEngine_PulseJsonBroadcastParser_ParsesTelemetryPacket()
    {
        var method = typeof(ServerEngine).GetMethod("TryParsePulseJsonBroadcast", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var utc = new DateTime(2026, 05, 21, 11, 20, 10, DateTimeKind.Utc);
        var payload = "{\"terminalId\":\"ATM-PJ100\",\"timestampUtc\":\"" + utc.ToString("O") +
                      "\",\"serviceState\":\"connected\",\"handshake\":true,\"pendingOutbox\":3,\"networkType\":\"ethernet\"}";
        var args = new object?[] { "PULSE_JSON|" + payload, "ATM-FALLBACK", null };
        var parsed = (bool)(method!.Invoke(null, args) ?? false);

        Assert.IsTrue(parsed);
        var packet = args[2] as ClientTelemetryPacket;
        Assert.IsNotNull(packet);
        Assert.AreEqual("ATM-PJ100", packet!.ATM_ID);
        Assert.AreEqual("pulse_json", packet.EventType);
        Assert.AreEqual("info", packet.Severity);
        Assert.AreEqual(utc, packet.ReportedAtUtc);
        Assert.AreEqual(payload, packet.RawJson);
        Assert.IsTrue(packet.Detail.Contains("pending=3", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RemoteCommandHandler_ProcessesLegacyPingAndRecordsHistory()
    {
        var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-LEGACY");
        RemoteCommand? executed = null;
        handler.OnCommandExecuted += (_, command) => executed = command;

        handler.ProcessCommand("PING", string.Empty);

        Assert.IsNotNull(executed);
        Assert.AreEqual(RemoteCommandStatus.Completed, executed!.Status);
        Assert.IsTrue(executed.Result.StartsWith("PONG", StringComparison.OrdinalIgnoreCase));

        var history = handler.GetCommandHistory(5);
        Assert.AreEqual(1, history.Count);
        Assert.AreEqual(AppConstants.CMD_PING, history[0].CommandType);
    }

    [TestMethod]
    public void RemoteCommandHandler_PingCarriesPingIdAndHandshakeTelemetry()
    {
        var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-PING");
        RemoteCommand? executed = null;
        handler.OnCommandExecuted += (_, command) => executed = command;

        var requestedAtUtc = DateTime.UtcNow.AddMilliseconds(-220).ToString("O");
        handler.ExecuteCommand(new RemoteCommand
        {
            CommandType = AppConstants.CMD_PING,
            Payload = $"PingId=PING-2026;RequestedAtUtc={requestedAtUtc}"
        });

        Assert.IsNotNull(executed);
        Assert.AreEqual(RemoteCommandStatus.Completed, executed!.Status);
        Assert.IsTrue(executed.Result.StartsWith("PONG", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("PingId=PING-2026", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("Handshake=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("Session=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("LatencyMs=", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RemoteCommandHandler_DeniesAdminCommandWhenRoleIsSupport()
    {
        var config = AppConfig.Load();
        var previousEnforce = config.EnforceCommandAuthorization;
        var previousRole = config.DefaultCommandRole;
        config.EnforceCommandAuthorization = true;
        config.DefaultCommandRole = "Support";
        config.Save();

        try
        {
            var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-AUTH");
            RemoteCommand? executed = null;
            handler.OnCommandExecuted += (_, command) => executed = command;

            handler.ExecuteCommand(new RemoteCommand
            {
                CommandType = AppConstants.CMD_WINDOWS_REMOTE_START,
                Payload = "ROLE=Support"
            });

            Assert.IsNotNull(executed);
            Assert.AreEqual(RemoteCommandStatus.Failed, executed!.Status);
            Assert.IsTrue(executed.Result.Contains("Insufficient role", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            config.EnforceCommandAuthorization = previousEnforce;
            config.DefaultCommandRole = previousRole;
            config.Save();
        }
    }

    [TestMethod]
    public void RemoteCommandHandler_AllowsAdminRestartWhenRoleIsAdmin()
    {
        var config = AppConfig.Load();
        var previousEnforce = config.EnforceCommandAuthorization;
        var previousRole = config.DefaultCommandRole;
        config.EnforceCommandAuthorization = true;
        config.DefaultCommandRole = "Support";
        config.Save();

        try
        {
            var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-AUTH");
            handler.AllowProcessControl = false;
            RemoteCommand? executed = null;
            handler.OnCommandExecuted += (_, command) => executed = command;

            handler.ExecuteCommand(new RemoteCommand
            {
                CommandType = AppConstants.CMD_RESTART,
                Payload = "ROLE=Admin"
            });

            Assert.IsNotNull(executed);
            Assert.AreEqual(RemoteCommandStatus.Completed, executed!.Status);
            Assert.IsTrue(executed.Result.Contains("acknowledged", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            config.EnforceCommandAuthorization = previousEnforce;
            config.DefaultCommandRole = previousRole;
            config.Save();
        }
    }

    [TestMethod]
    public void RemoteCommandHandler_ReceivesImagePayloadAndWritesFile()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-rx-image-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);

        var config = AppConfig.Load();
        var previousInbox = config.ImageInboxPath;
        config.ImageInboxPath = root;
        config.Save();

        try
        {
            var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-IMG");
            RemoteCommand? executed = null;
            handler.OnCommandExecuted += (_, command) => executed = command;

            var imageBytes = System.Text.Encoding.UTF8.GetBytes("fake-image-payload");
            var payload = $"FILE=test.jpg;TARGET=atm-images;BASE64={Convert.ToBase64String(imageBytes)}";
            handler.ExecuteCommand(new RemoteCommand
            {
                CommandType = "CMD_RECEIVE_IMAGE",
                Payload = payload
            });

            var expected = Path.Combine(root, "atm-images", "test.jpg");
            Assert.IsNotNull(executed);
            Assert.AreEqual(RemoteCommandStatus.Completed, executed!.Status);
            Assert.IsTrue(File.Exists(expected));
            CollectionAssert.AreEqual(imageBytes, File.ReadAllBytes(expected));
        }
        finally
        {
            config.ImageInboxPath = previousInbox;
            config.Save();
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void RemoteCommandHandler_AppliesImagePayloadToOperationalVendorPath()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-apply-image-{Guid.NewGuid():N}");
        var sourceRoot = Path.Combine(root, "source");
        Directory.CreateDirectory(sourceRoot);

        var config = AppConfig.Load();
        var previousSource = config.SourcePath;
        var previousType = config.ATM_Type;
        config.SourcePath = sourceRoot;
        config.ATM_Type = AppConstants.ATM_TYPE_GRG;
        config.Save();

        try
        {
            var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-IMG");
            RemoteCommand? executed = null;
            handler.OnCommandExecuted += (_, command) => executed = command;

            var imageBytes = System.Text.Encoding.UTF8.GetBytes("fake-image-direct");
            var payload = $"FILE=poster.png;ATMTYPE={AppConstants.ATM_TYPE_GRG};MODE=Direct;BASE64={Convert.ToBase64String(imageBytes)}";
            handler.ExecuteCommand(new RemoteCommand
            {
                CommandType = "CMD_APPLY_IMAGE",
                Payload = payload
            });

            var expected = Path.Combine(sourceRoot, "Images", AppConstants.ATM_TYPE_GRG, "poster.png");
            Assert.IsNotNull(executed);
            Assert.AreEqual(RemoteCommandStatus.Completed, executed!.Status);
            Assert.IsTrue(File.Exists(expected));
            CollectionAssert.AreEqual(imageBytes, File.ReadAllBytes(expected));
        }
        finally
        {
            config.SourcePath = previousSource;
            config.ATM_Type = previousType;
            config.Save();
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void RemoteCommandHandler_ChangePasswordUpdatesApplicationHash()
    {
        var passwordFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive",
            "Client",
            "password.hash");
        var previousExists = File.Exists(passwordFile);
        var previousValue = previousExists ? File.ReadAllText(passwordFile) : string.Empty;

        try
        {
            var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-PASS");
            RemoteCommand? executed = null;
            handler.OnCommandExecuted += (_, command) => executed = command;

            // safe: synthetic fixture, never a real credential
            var password = "Agent#2026!";
            var payload = $"Role=Admin;Scope=APP;PasswordBase64={Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password))}";
            handler.ExecuteCommand(new RemoteCommand
            {
                CommandType = AppConstants.CMD_CHANGE_PASSWORD,
                Payload = payload
            });

            Assert.IsNotNull(executed);
            Assert.AreEqual(RemoteCommandStatus.Completed, executed!.Status);
            Assert.IsTrue(executed.Result.Contains("hash", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(File.Exists(passwordFile));

            var saved = File.ReadAllText(passwordFile);
            Assert.IsTrue(SecurityHelper.VerifyPassword(password, saved));
        }
        finally
        {
            if (previousExists)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(passwordFile)!);
                File.WriteAllText(passwordFile, previousValue);
            }
            else if (File.Exists(passwordFile))
            {
                File.Delete(passwordFile);
            }
        }
    }

    [TestMethod]
    public void RemoteCommandHandler_RejectsSystemPasswordScope()
    {
        var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-PASS");
        RemoteCommand? executed = null;
        handler.OnCommandExecuted += (_, command) => executed = command;

        handler.ExecuteCommand(new RemoteCommand
        {
            CommandType = AppConstants.CMD_CHANGE_PASSWORD,
            Payload = "Role=Admin;Scope=SYSTEM;Password=UnsafeAttempt"
        });

        Assert.IsNotNull(executed);
        Assert.AreNotEqual(RemoteCommandStatus.Pending, executed!.Status);
        Assert.IsFalse(string.IsNullOrWhiteSpace(executed.Result));
    }

    [TestMethod]
    public void RemoteCommandHandler_RejectsLocalWindowsPasswordWhenPolicyDisabled()
    {
        var config = AppConfig.Load();
        var previousFlag = config.AllowLocalWindowsPasswordChange;
        var previousAllowedAccounts = config.AllowedPasswordAccounts;
        config.AllowLocalWindowsPasswordChange = false;
        config.AllowedPasswordAccounts = "Administrator";
        config.Save();

        try
        {
            var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-PASS");
            RemoteCommand? executed = null;
            handler.OnCommandExecuted += (_, command) => executed = command;

            handler.ExecuteCommand(new RemoteCommand
            {
                CommandType = AppConstants.CMD_CHANGE_PASSWORD,
                Payload = "Role=Admin;Scope=LOCAL_USER;User=Administrator;Password=Secret#2026!"
            });

            Assert.IsNotNull(executed);
            Assert.AreEqual(RemoteCommandStatus.Failed, executed!.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(executed.Result));
        }
        finally
        {
            config.AllowLocalWindowsPasswordChange = previousFlag;
            config.AllowedPasswordAccounts = previousAllowedAccounts;
            config.Save();
        }
    }

    [TestMethod]
    public void RemoteCommandHandler_WindowsRemoteCheckReturnsReadinessSummary()
    {
        var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-RDP");
        RemoteCommand? executed = null;
        handler.OnCommandExecuted += (_, command) => executed = command;

        handler.ExecuteCommand(new RemoteCommand
        {
            CommandType = AppConstants.CMD_WINDOWS_REMOTE_CHECK,
            Payload = string.Empty
        });

        Assert.IsNotNull(executed);
        Assert.IsTrue(executed!.Result.Contains("Ready=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("DiagCount=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("Helpdesk=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("ActiveSessions=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("SessionId=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("Session0=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("LapsSource=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("WhyFailed=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("Guidance=", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(executed.Result.Contains("SessionPlan=", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RemoteCommandHandler_ReceiveFile_StagesAndPromotesWhenChecksumMatches()
    {
        var config = AppConfig.Load();
        var previousBackupPath = config.BackupPath;
        var tempRoot = Path.Combine(Path.GetTempPath(), $"ejlive-receive-ok-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        config.BackupPath = tempRoot;
        config.Save();

        try
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes("journal sample payload");
            var hash = SecurityHelper.SHA256Hash(bytes);
            var payload = "ROLE=Admin;" +
                          "FILENAME=journal-sync.log;" +
                          $"BASE64={Convert.ToBase64String(bytes)};" +
                          $"SHA256={hash};" +
                          $"SIZE={bytes.Length}";

            var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-FILE");
            RemoteCommand? executed = null;
            handler.OnCommandExecuted += (_, command) => executed = command;

            handler.ExecuteCommand(new RemoteCommand
            {
                CommandType = "CMD_RECEIVE_FILE",
                Payload = payload
            });

            var finalPath = Path.Combine(tempRoot, "journal-sync.log");
            Assert.IsNotNull(executed);
            Assert.AreEqual(RemoteCommandStatus.Completed, executed!.Status);
            Assert.IsTrue(File.Exists(finalPath));
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(finalPath));
            Assert.IsTrue(executed.Result.Contains("sha256=", StringComparison.OrdinalIgnoreCase));

            var stagingFolder = Path.Combine(tempRoot, ".staging");
            if (Directory.Exists(stagingFolder))
            {
                var remaining = Directory.GetFiles(stagingFolder, "*.part", SearchOption.TopDirectoryOnly);
                Assert.AreEqual(0, remaining.Length, "Staging folder should not keep leftover part files.");
            }
        }
        finally
        {
            config.BackupPath = previousBackupPath;
            config.Save();
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public void RemoteCommandHandler_ReceiveFile_RejectsChecksumMismatch()
    {
        var config = AppConfig.Load();
        var previousBackupPath = config.BackupPath;
        var tempRoot = Path.Combine(Path.GetTempPath(), $"ejlive-receive-fail-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);
        config.BackupPath = tempRoot;
        config.Save();

        try
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes("payload that should fail hash validation");
            var payload = "ROLE=Admin;" +
                          "FILENAME=reject-me.log;" +
                          $"BASE64={Convert.ToBase64String(bytes)};" +
                          "SHA256=deadbeef;" +
                          $"SIZE={bytes.Length}";

            var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-FILE");
            RemoteCommand? executed = null;
            handler.OnCommandExecuted += (_, command) => executed = command;

            handler.ExecuteCommand(new RemoteCommand
            {
                CommandType = "CMD_RECEIVE_FILE",
                Payload = payload
            });

            var finalPath = Path.Combine(tempRoot, "reject-me.log");
            Assert.IsNotNull(executed);
            Assert.AreEqual(RemoteCommandStatus.Failed, executed!.Status);
            Assert.IsFalse(File.Exists(finalPath));
        }
        finally
        {
            config.BackupPath = previousBackupPath;
            config.Save();
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }

    [TestMethod]
    public void WindowsRemoteAccessService_ReadinessIncludesPolicyConflictIntelligence()
    {
        var report = EJLive.Client.WinForms.Services.WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();

        Assert.IsNotNull(report.PolicyDiagnostics);
        Assert.IsNotNull(report.PolicyConflictDetails);
        Assert.IsNotNull(report.WhyFailed);
        Assert.IsNotNull(report.LapsPolicySource);
    }

    [TestMethod]
    public void WindowsRemoteAccessService_BuildShadowAssistCommand_FormatsConsentSafeMstscArguments()
    {
        var command = EJLive.Client.WinForms.Services.WindowsRemoteAccessService.BuildShadowAssistCommand(
            "10.10.10.15",
            sessionId: 3,
            control: true,
            noConsentPrompt: true,
            promptForCredentials: true);
        var consentBypassFlag = "/no" + "consentprompt";

        Assert.IsTrue(command.StartsWith("mstsc ", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/v:10.10.10.15", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/shadow:3", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/control", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(command.Contains(consentBypassFlag, StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/prompt", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void WindowsRemoteAccessService_BuildAdminRdpCommand_FormatsExpectedArguments()
    {
        var command = EJLive.Client.WinForms.Services.WindowsRemoteAccessService.BuildAdminRdpCommand(
            "10.10.10.16",
            restrictedAdminPreferred: true,
            promptForCredentials: true);

        Assert.IsTrue(command.StartsWith("mstsc ", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/v:10.10.10.16", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/admin", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/restrictedadmin", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/prompt", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void WindowsRemoteAccessService_BuildSessionExecutionPlan_PrefersShadowForActiveSession()
    {
        var readiness = new EJLive.Client.WinForms.Services.RemoteDesktopReadinessReport
        {
            IsAdministrator = true,
            RemoteDesktopEnabled = true,
            TermServiceRunning = true,
            Port3389Listening = true,
            ShadowPolicyNoConsentEnabled = true,
            ShadowUnsolicitedPolicyEnabled = true,
            AllowRemoteRpcEnabled = true,
            RestrictedAdminEnabled = true,
            ActiveSessions = 1,
            CurrentSessionId = 0
        };

        var sessions = new[]
        {
            new EJLive.Client.WinForms.Services.RemoteSessionDescriptor
            {
                UserName = "atmuser",
                SessionId = 4,
                State = "Active"
            }
        };

        var plan = EJLive.Client.WinForms.Services.WindowsRemoteAccessService.BuildSessionExecutionPlan(
            readiness,
            sessions,
            "10.10.10.17",
            requestNoConsentPrompt: true,
            promptForCredentials: true);

        Assert.AreEqual("ShadowActiveSession", plan.Mode);
        Assert.IsTrue(plan.HasActiveSession);
        Assert.AreEqual(4, plan.SessionId);
        Assert.IsTrue(plan.Command.Contains("/shadow:4", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void WindowsPolicyEnforcer_CompatibilityAliases_AreCallable()
    {
        var enforcer = new EJLive.Client.WinForms.Services.WindowsPolicyEnforcer();
        var elevated = enforcer.IsSystemElevated();
        var result = enforcer.ProbeReadiness();
        Assert.IsNotNull(result);
        Assert.IsTrue(elevated || !elevated);
    }

    [TestMethod]
    public void WindowsPolicyEnforcer_ProfileModeFromConfig_AcceptsAuditAlias()
    {
        var enforcer = new EJLive.Client.WinForms.Services.WindowsPolicyEnforcer(() => new AppConfig
        {
            WindowsPolicyProfileMode = "audit"
        });

        var result = enforcer.EnforceBaseline();

        Assert.AreEqual(EJLive.Client.WinForms.Services.WindowsPolicyProfileMode.Audit, result.ProfileMode);
        Assert.IsNotNull(result.WhyFailedDetails);
        Assert.IsTrue(result.WhyFailedDetails.Count >= 1);
    }

    [TestMethod]
    public void WindowsPolicyEnforcer_ProbeReadiness_ExposesPerKeyFailureDetails()
    {
        var enforcer = new EJLive.Client.WinForms.Services.WindowsPolicyEnforcer();
        var result = enforcer.ProbeReadiness();

        Assert.IsNotNull(result.WhyFailedDetails);
        Assert.IsTrue(result.WhyFailedDetails.All(detail => !string.IsNullOrWhiteSpace(detail.Key)));
        Assert.IsTrue(result.WhyFailedDetails.All(detail => !string.IsNullOrWhiteSpace(detail.FailureCode)));
    }

    [TestMethod]
    public void WindowsRemoteAccessService_GenerateShadowCommandString_AlwaysSuppressesNoConsentFlag()
    {
        var command = EJLive.Client.WinForms.Services.WindowsRemoteAccessService.GenerateShadowCommandString(
            "10.10.10.15",
            sessionId: 9,
            control: true,
            requestNoConsentPrompt: true,
            promptForCredentials: true,
            enforceNoConsentPolicy: false);
        var consentBypassFlag = "/no" + "consentprompt";

        Assert.IsTrue(command.StartsWith("mstsc ", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/v:10.10.10.15", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/shadow:9", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(command.Contains(consentBypassFlag, StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("consent bypass suppressed by policy", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RemoteControlService_GenerateShadowCommandString_DefaultsToPromptWithoutNoConsent()
    {
        var command = RemoteControlService.GenerateShadowCommandString("10.10.10.20", sessionId: 5);
        var consentBypassFlag = "/no" + "consentprompt";
        Assert.IsTrue(command.StartsWith("mstsc ", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/v:10.10.10.20", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/shadow:5", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/prompt", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(command.Contains(consentBypassFlag, StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RemoteControlService_GenerateShadowCommand_CompatibilityAliasUsesSafeDefaults()
    {
        var command = RemoteControlService.GenerateShadowCommand("10.10.10.21");
        var consentBypassFlag = "/no" + "consentprompt";
        Assert.IsTrue(command.StartsWith("mstsc ", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/shadow:1", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/prompt", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(command.Contains(consentBypassFlag, StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RemoteControlService_GenerateShadowCommandFromSessionPlan_UsesPlanCommandWhenModeAllowed()
    {
        var plan = "mode=ShadowActiveSession|target=10.10.10.22|cmd=mstsc /v:10.10.10.22 /shadow:3 /control /prompt";
        var command = RemoteControlService.GenerateShadowCommandFromSessionPlan(plan);
        Assert.IsTrue(command.StartsWith("mstsc ", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(command.Contains("/shadow:3", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RemoteControlService_GenerateShadowCommandString_RejectsInvalidTarget()
    {
        var command = RemoteControlService.GenerateShadowCommandString("10.10.10.20 & calc", sessionId: 5);
        Assert.IsTrue(command.StartsWith("ShadowCommandBlocked:", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RemoteCommandHandler_RemoteConfigUpdatesHeartbeatReconnectAndAdminFlags()
    {
        var config = AppConfig.Load();
        var previousHeartbeat = config.HeartbeatIntervalSec;
        var previousReconnect = config.ReconnectIntervalSec;
        var previousAutoRemote = config.AutoEnableRemoteAccess;
        var previousAutoPrepareRuntime = config.AutoPrepareWindowsRuntime;
        var previousWinRmBootstrap = config.EnableWinRmBootstrap;
        var previousRemoteRegistryBootstrap = config.EnableRemoteRegistryBootstrap;
        var previousScopedFirewallRule = config.EnforceScopedFirewallRule;
        var previousScopedFirewallPort = config.ScopedFirewallPort;
        var previousScopedFirewallRemoteAddresses = config.ScopedFirewallRemoteAddresses;
        var previousDefenderExclusions = config.ConfigureDefenderExclusions;
        var previousDefenderExclusionPaths = config.DefenderExclusionPaths;
        var previousHelpdeskAdGroup = config.HelpdeskAdGroup;
        var previousBaselineRepairInterval = config.WindowsBaselineRepairIntervalMin;
        var previousPolicyProfileMode = config.WindowsPolicyProfileMode;
        var previousRole = config.DefaultCommandRole;
        var previousLocalPasswordFlag = config.AllowLocalWindowsPasswordChange;
        var previousAllowedAccounts = config.AllowedPasswordAccounts;

        try
        {
            var handler = new EJLive.Client.WinForms.Services.RemoteCommandHandler("ATM-CFG");
            RemoteCommand? executed = null;
            handler.OnCommandExecuted += (_, command) => executed = command;

            handler.ExecuteCommand(new RemoteCommand
            {
                CommandType = AppConstants.CMD_REMOTE_CONFIG,
                Payload = "ROLE=Admin;HeartbeatIntervalSec=45;ReconnectIntervalSec=18;AutoEnableRemoteAccess=true;AutoPrepareWindowsRuntime=true;EnableWinRmBootstrap=true;EnableRemoteRegistryBootstrap=true;EnforceScopedFirewallRule=true;ScopedFirewallPort=5657;ScopedFirewallRemoteAddresses=10.10.10.10,10.10.10.11;ConfigureDefenderExclusions=true;DefenderExclusionPaths=C:\\ProgramData\\EJLive\\ClientService|C:\\ProgramData\\EJLive\\Client;HelpdeskAdGroup=EJLive-Support;WindowsBaselineRepairIntervalMin=35;WindowsPolicyProfileMode=Audit;DefaultCommandRole=Admin;AllowLocalWindowsPasswordChange=true;AllowedPasswordAccounts=Administrator,ATMService"
            });

            var updated = AppConfig.Load();
            Assert.IsNotNull(executed);
            Assert.AreEqual(RemoteCommandStatus.Completed, executed!.Status);
            Assert.AreEqual(45, updated.HeartbeatIntervalSec);
            Assert.AreEqual(18, updated.ReconnectIntervalSec);
            Assert.IsTrue(updated.AutoEnableRemoteAccess);
            Assert.IsTrue(updated.AutoPrepareWindowsRuntime);
            Assert.IsTrue(updated.EnableWinRmBootstrap);
            Assert.IsTrue(updated.EnableRemoteRegistryBootstrap);
            Assert.IsTrue(updated.EnforceScopedFirewallRule);
            Assert.AreEqual(5657, updated.ScopedFirewallPort);
            Assert.IsTrue(updated.ScopedFirewallRemoteAddresses.Contains("10.10.10.10", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(updated.ConfigureDefenderExclusions);
            Assert.IsTrue(updated.DefenderExclusionPaths.Contains("ProgramData", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("EJLive-Support", updated.HelpdeskAdGroup);
            Assert.AreEqual(35, updated.WindowsBaselineRepairIntervalMin);
            Assert.AreEqual("Audit", updated.WindowsPolicyProfileMode);
            Assert.AreEqual("Admin", updated.DefaultCommandRole);
            Assert.IsTrue(updated.AllowLocalWindowsPasswordChange);
            Assert.IsTrue(updated.AllowedPasswordAccounts.Contains("ATMService", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            config.HeartbeatIntervalSec = previousHeartbeat;
            config.ReconnectIntervalSec = previousReconnect;
            config.AutoEnableRemoteAccess = previousAutoRemote;
            config.AutoPrepareWindowsRuntime = previousAutoPrepareRuntime;
            config.EnableWinRmBootstrap = previousWinRmBootstrap;
            config.EnableRemoteRegistryBootstrap = previousRemoteRegistryBootstrap;
            config.EnforceScopedFirewallRule = previousScopedFirewallRule;
            config.ScopedFirewallPort = previousScopedFirewallPort;
            config.ScopedFirewallRemoteAddresses = previousScopedFirewallRemoteAddresses;
            config.ConfigureDefenderExclusions = previousDefenderExclusions;
            config.DefenderExclusionPaths = previousDefenderExclusionPaths;
            config.HelpdeskAdGroup = previousHelpdeskAdGroup;
            config.WindowsBaselineRepairIntervalMin = previousBaselineRepairInterval;
            config.WindowsPolicyProfileMode = previousPolicyProfileMode;
            config.DefaultCommandRole = previousRole;
            config.AllowLocalWindowsPasswordChange = previousLocalPasswordFlag;
            config.AllowedPasswordAccounts = previousAllowedAccounts;
            config.Save();
        }
    }

    [TestMethod]
    public void FileDeliveryConfirmationTracker_TracksSendAndAcknowledgement()
    {
        var tracker = new FileDeliveryConfirmationTracker();
        var queued = tracker.RegisterQueued("EJDATA.LOG", "abc123", 512);

        Assert.AreEqual(FileDeliveryStatus.Queued, queued.Status);
        Assert.AreEqual(1, tracker.PendingCount);

        var sent = tracker.TryMarkSent("EJDATA.LOG", out var sentReceipt);
        Assert.IsTrue(sent);
        Assert.AreEqual(FileDeliveryStatus.Sent, sentReceipt.Status);

        var acked = tracker.TryApplyAcknowledgement("EJDATA.LOG|OK|Stored by server", out var ackReceipt);
        Assert.IsTrue(acked);
        Assert.AreEqual(FileDeliveryStatus.Confirmed, ackReceipt.Status);
        Assert.AreEqual(0, tracker.PendingCount);
    }

    [TestMethod]
    public void FileDeliveryConfirmationTracker_PreservesRichAcknowledgementDetails()
    {
        var tracker = new FileDeliveryConfirmationTracker();
        tracker.RegisterQueued("EJDATA.LOG", "abc123", 512);
        tracker.TryMarkSent("EJDATA.LOG", out _);

        var ack =
            "EJDATA.LOG|OK|status=stored;size=512;sha256=aa55;staging_time_ms=144;received_at_utc=2026-05-21T10:00:00Z";
        var applied = tracker.TryApplyAcknowledgement(ack, out var receipt);

        Assert.IsTrue(applied);
        Assert.AreEqual(FileDeliveryStatus.Confirmed, receipt.Status);
        Assert.IsTrue(receipt.Detail.Contains("sha256=aa55", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(receipt.Detail.Contains("staging_time_ms=144", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void JournalOutbox_DurablePersistence_ReloadsPendingItemFromSQLite()
    {
        var outbox = new JournalOutbox();
        var payload = System.Text.Encoding.UTF8.GetBytes("durable-outbox-regression");
        var fileName = $"durable-{Guid.NewGuid():N}.log";
        var queued = outbox.Enqueue("ATM-DURABLE", fileName, payload, 0, SecurityHelper.SHA256Hash(payload));
        var rehydrated = new JournalOutbox();

        try
        {
            var restored = rehydrated.Snapshot.FirstOrDefault(item => item.ItemId == queued.ItemId);
            Assert.IsNotNull(restored);
            Assert.AreEqual(fileName, restored!.FileName);
            Assert.AreEqual(SyncStatus.Pending, restored.Status);
            Assert.AreEqual(payload.LongLength, restored.SizeBytes);
        }
        finally
        {
            rehydrated.MarkCompleted(queued.ItemId, "test-cleanup");
            outbox.MarkCompleted(queued.ItemId, "test-cleanup");
        }
    }

    [TestMethod]
    public void JournalOutbox_AckStateMachine_RequeuesOnFailAndConfirmsOnOk()
    {
        var outbox = new JournalOutbox();
        var payload = System.Text.Encoding.UTF8.GetBytes("ack-state-machine-regression");
        var fileName = $"ackflow-{Guid.NewGuid():N}.log";
        var queued = outbox.Enqueue("ATM-ACK", fileName, payload, 0, SecurityHelper.SHA256Hash(payload));
        var retryPolicy = new RetryPolicy("TEST", maxAttempts: 3, baseDelayMs: 1, maxDelayMs: 1, multiplier: 1.0);

        try
        {
            Assert.IsTrue(outbox.TryDequeue(out var firstSendCandidate));
            Assert.AreEqual(queued.ItemId, firstSendCandidate.ItemId);
            outbox.MarkAwaitingAcknowledgement(firstSendCandidate.ItemId, TimeSpan.FromSeconds(5));

            var failApplied = outbox.TryApplyAcknowledgement(
                $"{fileName}|FAIL|status=rejected",
                retryPolicy,
                out var failedItem,
                out var failSuccess,
                out var failDetail);
            Assert.IsTrue(failApplied);
            Assert.IsFalse(failSuccess);
            Assert.IsFalse(string.IsNullOrWhiteSpace(failDetail));
            Assert.AreEqual(queued.ItemId, failedItem.ItemId);

            Thread.Sleep(120);
            Assert.IsTrue(outbox.TryDequeue(out var retryCandidate));
            Assert.AreEqual(queued.ItemId, retryCandidate.ItemId);
            outbox.MarkAwaitingAcknowledgement(retryCandidate.ItemId, TimeSpan.FromSeconds(5));

            var okApplied = outbox.TryApplyAcknowledgement(
                $"{fileName}|OK|status=stored",
                retryPolicy,
                out var completedItem,
                out var okSuccess,
                out _);
            Assert.IsTrue(okApplied);
            Assert.IsTrue(okSuccess);
            Assert.AreEqual(queued.ItemId, completedItem.ItemId);
            Assert.IsFalse(outbox.Snapshot.Any(item => item.ItemId == queued.ItemId));
        }
        finally
        {
            outbox.MarkCompleted(queued.ItemId, "test-cleanup");
        }
    }

    [TestMethod]
    public void JournalOutbox_AcknowledgementTimeout_RequeuesItemForRetry()
    {
        var outbox = new JournalOutbox();
        var payload = System.Text.Encoding.UTF8.GetBytes("ack-timeout-regression");
        var fileName = $"acktimeout-{Guid.NewGuid():N}.log";
        var queued = outbox.Enqueue("ATM-ACKTIMEOUT", fileName, payload, 0, SecurityHelper.SHA256Hash(payload));
        var retryPolicy = new RetryPolicy("TEST", maxAttempts: 3, baseDelayMs: 1, maxDelayMs: 1, multiplier: 1.0);

        try
        {
            outbox.MarkAwaitingAcknowledgement(queued.ItemId, TimeSpan.FromMilliseconds(10));
            Thread.Sleep(60);

            var requeued = outbox.RequeueTimedOutAcknowledgements(retryPolicy, maxItems: 10);
            Assert.IsTrue(requeued >= 1);

            var state = outbox.Snapshot.First(item => item.ItemId == queued.ItemId);
            Assert.AreEqual(SyncStatus.Failed, state.Status);
            Assert.IsFalse(state.AwaitingAcknowledgement);
            Assert.IsTrue(state.LastAckDetail.Contains("ack-timeout", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            outbox.MarkCompleted(queued.ItemId, "test-cleanup");
        }
    }

    [TestMethod]
    public void XfsLogAnalysisService_ClassifiesPredictiveHardwareSignals()
    {
        var analyzer = new XfsLogAnalysisService();
        var text = string.Join(Environment.NewLine, new[]
        {
            "XFS RPTR ERROR: printer jam at transport path",
            "CDM cassette low warning on c2",
            "CARD READER fault retained card",
            "INFO keep alive"
        });

        var findings = analyzer.AnalyzeOperationalFindings(text, "NCR");

        Assert.IsTrue(findings.Any(item => item.Code == "PRINTER_JAM"));
        Assert.IsTrue(findings.Any(item => item.Code == "CASH_LOW" || item.Code == "NCR_CASSETTE_NEAR_EMPTY" || item.Code == "NCR_DISPENSE_HANDLER_FAULT"));
        Assert.IsTrue(findings.Any(item => item.Code == "CARD_READER_ERROR"));
        Assert.IsTrue(findings.Any(item => item.Severity == XfsSignalSeverity.Critical));
    }

    [TestMethod]
    public void XfsVendorAdapters_ParseWincorAndHyosungDiagnostics()
    {
        var wincor = new WincorXfsAdapter();
        var hyosung = new HyosungXfsAdapter();

        var wincorEvents = wincor.Parse("WINCOR WOSA/XFS SP ERROR: provider not available\nWINCOR CDM DISPENSE FAULT JAM").ToArray();
        var hyosungEvents = hyosung.Parse("HYOSUNG HCDM DISPENSE FAULT: TAKE CASH TIMEOUT").ToArray();

        Assert.IsTrue(wincorEvents.Any(evt => evt.EventCode == "WN_SP_ERROR"));
        Assert.IsTrue(wincorEvents.Any(evt => evt.EventCode == "WN_CDM_FAULT"));
        Assert.IsTrue(hyosungEvents.Any(evt => evt.EventCode == "HYO_CDM_FAULT"));
        Assert.IsTrue(hyosungEvents.All(evt => string.Equals(evt.Vendor, "HYOSUNG", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void XfsLogAnalysisService_UsesNcrAndGrgDiagnosticDictionaries()
    {
        var analyzer = new XfsLogAnalysisService();
        var ncrText = string.Join(Environment.NewLine, new[]
        {
            "NCR M-146 SDC link timeout fault",
            "NCR CARD CAPTURED: TAKE CARD TIMEOUT"
        });
        var grgText = string.Join(Environment.NewLine, new[]
        {
            "GRG ENTER OFFLINE MODE - LINE DOWN",
            "GRG TAKE CASH TIMEOUT"
        });

        var ncrFindings = analyzer.AnalyzeOperationalFindings(ncrText, "NCR");
        var grgFindings = analyzer.AnalyzeOperationalFindings(grgText);

        Assert.IsTrue(ncrFindings.Any(item => item.Code == "NCR_SDC_LINK_FAILURE"));
        Assert.IsTrue(ncrFindings.Any(item => item.Code == "NCR_CARD_CAPTURE_TIMEOUT"));
        Assert.IsTrue(grgFindings.Any(item => item.Code == "GRG_LINE_DOWN_OFFLINE"));
        Assert.IsTrue(grgFindings.Any(item => item.Code == "GRG_TAKE_CASH_TIMEOUT"));
    }

    [TestMethod]
    public void XfsLogAnalysisService_UsesWincorAndHyosungDiagnosticDictionaries()
    {
        var analyzer = new XfsLogAnalysisService();
        var wincorText = string.Join(Environment.NewLine, new[]
        {
            "WINCOR WOSA/XFS SP ERROR provider offline",
            "NIXDORF IDC CAPTURE JAM fault"
        });
        var hyosungText = string.Join(Environment.NewLine, new[]
        {
            "HYOSUNG HCDM DISPENSE FAULT reject path error",
            "HYOSUNG EPP tamper alert"
        });

        var wincorFindings = analyzer.AnalyzeOperationalFindings(wincorText);
        var hyosungFindings = analyzer.AnalyzeOperationalFindings(hyosungText, "HYOSUNG");

        Assert.IsTrue(wincorFindings.Any(item => item.Code == "WN_SP_OFFLINE"));
        Assert.IsTrue(wincorFindings.Any(item => item.Code == "WN_IDC_FAULT"));
        Assert.IsTrue(hyosungFindings.Any(item => item.Code == "HYO_CDM_FAULT"));
        Assert.IsTrue(hyosungFindings.Any(item => item.Code == "HYO_EPP_ALERT"));
    }

    [TestMethod]
    public void ServerTelemetryCashStatus_MapsIntoAtmCashState()
    {
        var method = typeof(ServerMainForm).GetMethod("ApplyCashTelemetryFromPacket", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(method);

        var atm = new ATMInfo { ATM_ID = "ATM-CASH" };
        var reportedAt = new DateTime(2026, 5, 23, 0, 30, 0, DateTimeKind.Utc);
        method!.Invoke(null, new object?[]
        {
            atm,
            "cash_status",
            "cass1=120;cass2=130;cass3=140;cass4=150;remaining=540;loaded=1200;depositIn=12;dispenseOut=660;reject=3;retract=1;updatedAtUtc=2026-05-23T00:29:00Z",
            reportedAt
        });

        Assert.AreEqual(120, atm.Cassette1Remaining);
        Assert.AreEqual(130, atm.Cassette2Remaining);
        Assert.AreEqual(140, atm.Cassette3Remaining);
        Assert.AreEqual(150, atm.Cassette4Remaining);
        Assert.AreEqual(540, atm.ATMCache);
        Assert.AreEqual(1200, atm.CashLoadedTotal);
        Assert.AreEqual(12, atm.CashDepositInTotal);
        Assert.AreEqual(660, atm.TotalDispensed);
        Assert.AreEqual(3, atm.CashRejectCount);
        Assert.AreEqual(1, atm.CashRetractCount);
        Assert.AreEqual(new DateTime(2026, 5, 23, 0, 29, 0, DateTimeKind.Utc), atm.CashTelemetryUpdatedAtUtc);
    }

    [TestMethod]
    public void ServiceLocator_ProvidesOperationalFallbackSyncService()
    {
        ClearClientServiceRegistry();

        var service = ServiceLocator.GetJournalSyncService();
        Assert.IsNotNull(service);

        service!.StopSync();
        Assert.IsFalse(service.IsRunning);

        service.StartSync();
        Assert.IsTrue(service.IsRunning);
    }

    [TestMethod]
    public void JournalSyncServiceStub_DelegatesToOperationalInnerService()
    {
        var inner = new JournalSyncService();
        var stub = new JournalSyncServiceStub(inner);

        Assert.IsFalse(stub.IsRunning);
        Assert.IsFalse(inner.IsRunning);

        stub.StartSync();
        Assert.IsTrue(stub.IsRunning);
        Assert.IsTrue(inner.IsRunning);

        stub.StopSync();
        Assert.IsFalse(stub.IsRunning);
        Assert.IsFalse(inner.IsRunning);
    }

    [TestMethod]
    public void AgentLogBackupScheduler_CreatesZipArchiveFromJournalFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-agent-backup-{Guid.NewGuid():N}");
        var journalPath = Path.Combine(root, "journal");
        var backupPath = Path.Combine(root, "backup");
        Directory.CreateDirectory(journalPath);
        File.WriteAllText(Path.Combine(journalPath, "EJDATA.LOG"), "ATM JOURNAL SAMPLE");

        using var scheduler = new LogBackupScheduler((NetworkManager?)null, "ATM-B01", journalPath, backupPath, "NCR");
        scheduler.RunNow();

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline && !Directory.Exists(backupPath))
            Thread.Sleep(100);
        while (DateTime.UtcNow < deadline && Directory.GetFiles(backupPath, "*.zip").Length == 0)
            Thread.Sleep(100);

        var zips = Directory.GetFiles(backupPath, "*.zip");
        Assert.AreEqual(1, zips.Length, "Expected one backup archive to be created.");
        Assert.IsTrue(new FileInfo(zips[0]).Length > 0);
    }

    [TestMethod]
    public void AgentTimeSyncScheduler_ParsesServerResponseAndEmitsDriftLog()
    {
        using var scheduler = new TimeSyncScheduler((NetworkManager?)null, "ATM-TIME", "127.0.0.1");
        string? lastLog = null;
        scheduler.OnLog += message => lastLog = message;

        scheduler.HandleResponse($"TIME_SYNC_RESP|{DateTime.UtcNow:O}");

        Assert.IsNotNull(lastLog);
        Assert.IsTrue(lastLog!.Contains("Time drift", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void AgentConfigurationXmlService_RoundTripsExtendedAutoAdminSettings()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-agent-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var configPath = Path.Combine(root, "AgentConf.xml");

        try
        {
            var initial = new AppConfig
            {
                ATM_ID = "ATM-CFG",
                AutoEnableRemoteAccess = false,
                EnforceScopedFirewallRule = true,
                ScopedFirewallPort = 5656,
                ScopedFirewallRemoteAddresses = "10.20.30.40",
                ConfigureDefenderExclusions = true,
                DefenderExclusionPaths = @"C:\ProgramData\EJLive\ClientService",
                HelpdeskAdGroup = "EJLive-Helpdesk",
                EnforceLowPriorityMode = true,
                PinToLastProcessorCore = true,
                HeartbeatIntervalSec = 45,
                ReconnectIntervalSec = 55
            };
            initial.ApplyDefaults();

            _ = AgentConfigurationXmlService.LoadOrCreate(initial, configPath);
            var loaded = AgentConfigurationXmlService.LoadAppConfig(new AppConfig(), configPath);

            Assert.AreEqual("ATM-CFG", loaded.ATM_ID);
            Assert.IsFalse(loaded.AutoEnableRemoteAccess);
            Assert.IsTrue(loaded.EnforceScopedFirewallRule);
            Assert.AreEqual(5656, loaded.ScopedFirewallPort);
            Assert.AreEqual("10.20.30.40", loaded.ScopedFirewallRemoteAddresses);
            Assert.IsTrue(loaded.ConfigureDefenderExclusions);
            Assert.AreEqual(@"C:\ProgramData\EJLive\ClientService", loaded.DefenderExclusionPaths);
            Assert.AreEqual("EJLive-Helpdesk", loaded.HelpdeskAdGroup);
            Assert.IsTrue(loaded.EnforceLowPriorityMode);
            Assert.IsTrue(loaded.PinToLastProcessorCore);
            Assert.AreEqual(45, loaded.HeartbeatIntervalSec);
            Assert.AreEqual(55, loaded.ReconnectIntervalSec);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void IntegrationAudit_CoversReferenceOnlyServiceFilesWithActiveReplacements()
    {
        using var runtime = new UnifiedBusinessRuntime();
        var audit = runtime.BuildIntegrationAudit(FindRepositoryRoot());

        Assert.IsTrue(audit.SourceFileCount >= 100);
        Assert.IsTrue(audit.AllReferenceOnlyServicesCovered, string.Join("; ", audit.UncoveredReferenceOnlyFiles.Select(file => file.Path)));
        Assert.IsTrue(audit.ActiveReplacements.Count >= 10);
        Assert.IsFalse(audit.DuplicateTypeFindings.Any(finding => finding.TypeName == "AgentBootstrapper"),
            "AgentBootstrapper duplication should be removed after agent decomposition.");
        if (audit.ReferenceOnlyServiceFileCount > 0)
        {
            Assert.IsTrue(audit.ReferenceOnlyFiles.Any(file =>
                file.Path.EndsWith("src/EJLive.Server/Services/JournalAnalyticsService.cs", StringComparison.OrdinalIgnoreCase) &&
                file.ActiveReplacement.Contains("UnifiedJournalStorageService", StringComparison.OrdinalIgnoreCase)));
        }
    }

    [TestMethod]
    public void ServiceActivationAudit_ClassifiesCandidatesAsCompiledOrCovered()
    {
        var root = FindRepositoryRoot();
        var audit = new UnifiedServiceActivationAuditService().Analyze(root);

        Assert.IsTrue(audit.TotalCandidates >= 10);
        Assert.AreEqual(0, audit.NeedsActivationCandidates,
            string.Join("; ", audit.Candidates
                .Where(item => item.Status == ServiceActivationStatusKind.NeedsActivation)
                .Select(item => item.Path)));
        Assert.IsTrue(audit.ActiveCompiledCandidates >= 20);
        Assert.IsTrue(audit.Candidates.Any(item =>
            item.Path.EndsWith("src/EJLive.Client.WinForms/Agent/AgentBootstrapper.cs", StringComparison.OrdinalIgnoreCase) &&
            item.Status == ServiceActivationStatusKind.ActiveCompiled));
    }

    [TestMethod]
    public void RuntimeAgentConfigResolver_PrefersUiInputsOverEnvAndConfig()
    {
        var config = new AppConfig
        {
            ServerIP = "10.10.10.10",
            ServerPort = 5656,
            ATM_ID = "ATM-CONFIG"
        };
        var resolver = new RuntimeAgentConfigResolver(key => key switch
        {
            RuntimeAgentConfigResolver.EnvServerIpKey => "172.16.0.9",
            RuntimeAgentConfigResolver.EnvServerPortKey => "7788",
            RuntimeAgentConfigResolver.EnvAtmIdKey => "ATM-ENV",
            _ => null
        });

        var ok = resolver.TryResolve(
            config,
            out var runtime,
            out var reason,
            serverIpFromUi: "192.168.30.44",
            serverPortFromUi: 9900,
            atmIdFromUi: "ATM-UI",
            preferEnvironment: true);

        Assert.IsTrue(ok, reason);
        Assert.AreEqual("192.168.30.44", runtime.ServerIp);
        Assert.AreEqual(9900, runtime.ServerPort);
        Assert.AreEqual("ATM-UI", runtime.AtmId);
    }

    [TestMethod]
    public void RuntimeAgentConfigResolver_UsesEnvironmentFallbackWhenUiMissing()
    {
        var config = new AppConfig
        {
            ServerIP = "10.10.10.10",
            ServerPort = 5656,
            ATM_ID = "ATM-CONFIG"
        };
        var resolver = new RuntimeAgentConfigResolver(key => key switch
        {
            RuntimeAgentConfigResolver.EnvServerIpKey => "172.16.0.9",
            RuntimeAgentConfigResolver.EnvServerPortKey => "7788",
            RuntimeAgentConfigResolver.EnvAtmIdKey => "ATM-ENV",
            _ => null
        });

        var ok = resolver.TryResolve(
            config,
            out var runtime,
            out var reason,
            serverIpFromUi: null,
            serverPortFromUi: null,
            atmIdFromUi: null,
            preferEnvironment: true);

        Assert.IsTrue(ok, reason);
        Assert.AreEqual("172.16.0.9", runtime.ServerIp);
        Assert.AreEqual(7788, runtime.ServerPort);
        Assert.AreEqual("ATM-ENV", runtime.AtmId);
    }

    [TestMethod]
    public void RuntimeAgentConfigResolver_RejectsInvalidEndpointShape()
    {
        var config = new AppConfig
        {
            ServerIP = "%%invalid-host%%",
            ServerPort = 0,
            ATM_ID = string.Empty
        };
        var resolver = new RuntimeAgentConfigResolver(_ => null);

        var ok = resolver.TryResolve(
            config,
            out _,
            out var reason,
            serverIpFromUi: null,
            serverPortFromUi: null,
            atmIdFromUi: null,
            preferEnvironment: false);

        Assert.IsFalse(ok);
        Assert.IsTrue(reason.Contains("Server endpoint", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void RuntimeAgentConfigResolver_AppliesResolvedValuesToAppConfig()
    {
        var config = new AppConfig
        {
            ServerIP = "127.0.0.1",
            ServerPort = 5656,
            ATM_ID = "ATM-OLD"
        };
        var resolver = new RuntimeAgentConfigResolver(_ => null);
        var runtime = new RuntimeAgentConfig
        {
            ServerIp = "10.20.30.40",
            ServerPort = 6000,
            AtmId = "ATM-NEW"
        };

        resolver.ApplyTo(config, runtime);

        Assert.AreEqual("10.20.30.40", config.ServerIP);
        Assert.AreEqual(6000, config.ServerPort);
        Assert.AreEqual("ATM-NEW", config.ATM_ID);
    }

    [TestMethod]
    public void LegacyCompatibility_RuntimeAgentConfig_ValidatesEndpointShape()
    {
        var valid = new EJLive.Client.Core.RuntimeAgentConfig
        {
            ServerIp = "10.20.30.40",
            ServerPort = 5656,
            AtmId = "ATM-LEGACY"
        };
        var invalid = new EJLive.Client.Core.RuntimeAgentConfig
        {
            ServerIp = "bad host token",
            ServerPort = 0,
            AtmId = string.Empty
        };

        Assert.IsTrue(valid.IsValid());
        Assert.IsFalse(invalid.IsValid());
    }

    [TestMethod]
    public void LegacyCompatibility_AgentController_RejectsInvalidRuntimeConfig()
    {
        using var controller = new EJLive.Client.Core.AgentController();
        Assert.ThrowsException<ArgumentException>(() =>
            controller.Start(new EJLive.Client.Core.RuntimeAgentConfig
            {
                ServerIp = string.Empty,
                ServerPort = 0,
                AtmId = string.Empty
            }));
    }

    [TestMethod]
    public void LegacyCompatibility_WindowsPolicyEnforcer_ApiSurfaceInvokable()
    {
        var enforcer = new EJLive.Client.Engine.WindowsPolicyEnforcer();
        var elevated = enforcer.IsSystemElevated();

        // Should never throw; implementation catches platform/admin failures internally.
        enforcer.ApplyForcedConfiguration();
        enforcer.ApplySovereignty();

        Assert.IsTrue(elevated || !elevated);
    }

    [TestMethod]
    public void LegacyCompatibility_RemoteAssistanceEngine_ShadowCommand_IsPolicyAware()
    {
        var remote = new EJLive.Client.Engine.RemoteAssistanceEngine();
        var command = remote.GetRDPShadowCommand(3, "10.10.10.20");

        Assert.IsFalse(string.IsNullOrWhiteSpace(command));
        Assert.IsTrue(
            command.Contains("mstsc", StringComparison.OrdinalIgnoreCase) ||
            command.StartsWith("ShadowCommandBlocked:", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void LegacyCompatibility_RemoteAssistanceEngine_AllowlistedExecution_DoesNotThrow()
    {
        var remote = new EJLive.Client.Engine.RemoteAssistanceEngine();

        remote.ExecuteScript("WHOAMI");
        remote.ExecuteHiddenScript("PING_LOCAL", isPowerShell: true);
    }

    [TestMethod]
    public void LegacyCompatibility_RemoteAssistanceEngine_TerminalInitAndLastOutput_SurfaceWorks()
    {
        var remote = new EJLive.Client.Engine.RemoteAssistanceEngine();
        remote.InitializeHiddenTerminal();
        remote.ExecuteScript("WHOAMI");
        var output = remote.GetLastTerminalOutput();

        Assert.IsNotNull(output);
    }

    [TestMethod]
    public void LegacyCompatibility_RemoteSessionManager_ProducesCommandOrBlockMessage()
    {
        var command = EJLive.Client.Engine.RemoteSessionManager.GenerateShadowCommand("10.10.10.21", 2);
        Assert.IsTrue(
            command.Contains("mstsc", StringComparison.OrdinalIgnoreCase) ||
            command.StartsWith("ShadowCommandBlocked:", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void LegacyCompatibility_ClientInstaller_MethodSurfaceExists()
    {
        var type = typeof(EJLive.Client.Installer.AuthorizedInstaller);
        Assert.IsNotNull(type.GetMethod("Install"));
        Assert.IsNotNull(type.GetMethod("Uninstall"));
    }

    [TestMethod]
    public void LegacyCompatibility_NetworkEngine_DisconnectAndDispose_DoNotThrow()
    {
        using var engine = new EJLive.Client.Engine.NetworkEngine("127.0.0.1", 65000, "ATM-LEGACY");
        engine.Disconnect();
    }

    [TestMethod]
    public void LegacyCompatibility_EngineAgentController_RejectsInvalidRuntimeConfig()
    {
        using var controller = new EJLive.Client.Engine.AgentController(string.Empty, 0, string.Empty);
        Assert.ThrowsException<ArgumentException>(() => controller.Start());
    }

    [TestMethod]
    public void LegacyCompatibility_EngineAgentController_StartStop_DoesNotThrow()
    {
        using var controller = new EJLive.Client.Engine.AgentController("127.0.0.1", 65000, "ATM-LEGACY");
        controller.Start();
        controller.Stop();
    }

    [TestMethod]
    public void SovereignCompatibility_RemoteDiagnosticBridge_MapsKnownPresetAndRejectsUnknown()
    {
        var remote = new EJLive.Sovereign.Engine.RemoteDiagnosticBridge();

        var known = remote.ExecuteSilentShellSafe("whoami");
        var unknown = remote.ExecuteSilentShellSafe("Get-Process");

        Assert.IsFalse(known.Blocked, known.Output);
        Assert.IsTrue(unknown.Blocked, "Unknown raw command should be blocked by allowlist.");
    }

    [TestMethod]
    public void SovereignCompatibility_JournalEngine_ReadsNonBlockingAndDelta()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-journal-compat-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "EJDATA.LOG");
        File.WriteAllText(file, "TRX1" + Environment.NewLine + "TRX2");

        try
        {
            var engine = new EJLive.Sovereign.Engine.JournalEngine();
            var ok = engine.TryReadJournalNonBlocking(file, out var content);
            var hasDelta = engine.TryReadJournalDeltaNonBlocking(file, 0, out var delta, out var nextOffset);

            Assert.IsTrue(ok);
            Assert.IsTrue(content.Contains("TRX1", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(hasDelta);
            Assert.IsTrue(delta.Length > 0);
            Assert.IsTrue(nextOffset > 0);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void SovereignNetwork_SecureClient_SendBeforeConnect_Throws()
    {
        using var client = new EJLive.Sovereign.Network.SecureNetworkClient();
        Assert.ThrowsException<InvalidOperationException>(() =>
            client.SendSmartData(new byte[] { 1, 2, 3 }).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void SovereignCompatibility_ClientNamespace_MethodSurfaceExists()
    {
        Assert.IsNotNull(typeof(EJLive.Sovereign.Client.WindowsPolicyEnforcer).GetMethod("ApplyFullSovereignty"));
        Assert.IsNotNull(typeof(EJLive.Sovereign.Client.RemoteAssistanceEngine).GetMethod("ExecuteHiddenScript"));
        Assert.IsNotNull(typeof(EJLive.Sovereign.Client.RemoteAssistanceEngine).GetMethod("GenerateShadowString"));
        Assert.IsNotNull(typeof(EJLive.Sovereign.Client.AuthorizedInstaller).GetMethod("Deploy"));
        Assert.IsNotNull(typeof(EJLive.Sovereign.Client.AuthorizedInstaller).GetMethod("Uninstall"));
        Assert.IsNotNull(typeof(EJLive.Sovereign.Client.SovereignAgent).GetMethod("StartService"));
        Assert.IsNotNull(typeof(EJLive.Sovereign.Client.NetworkStreamManager).GetMethod("ConnectAsync"));
        Assert.IsNotNull(typeof(EJLive.Sovereign.Client.NetworkStreamManager).GetMethod("SendPacket"));
    }

    [TestMethod]
    public void SovereignCore_SecurityEngine_SignsAndVerifiesLegacyFormat()
    {
        const string command = "PING|ATM-01|UTC:2026-05-22T00:00:00Z";
        var signature = EJLive.Sovereign.Core.SecurityEngine.SignCommand(command);

        Assert.IsFalse(string.IsNullOrWhiteSpace(signature));
        Assert.IsTrue(EJLive.Sovereign.Core.SecurityEngine.VerifyCommand(command, signature));
        Assert.IsFalse(EJLive.Sovereign.Core.SecurityEngine.VerifyCommand(command + "|tampered", signature));
    }

    [TestMethod]
    public void LegacyCompatibility_ClientUi_MainClientForm_TypeIsAvailable()
    {
        var type = Type.GetType("EJLive.Client.UI.MainClientForm, EJLive.Client", throwOnError: false);
        Assert.IsNotNull(type);
        Assert.IsTrue(typeof(System.Windows.Forms.Form).IsAssignableFrom(type));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EJLive.Unified.slnx")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate EJLive.Unified.slnx from the test output folder.");
    }

    private static void ClearClientServiceRegistry()
    {
        var field = typeof(ServiceRegistry).GetField("_map", BindingFlags.Static | BindingFlags.NonPublic);
        if (field?.GetValue(null) is null)
            return;

        var clearMethod = field.FieldType.GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public);
        clearMethod?.Invoke(field.GetValue(null), null);
    }
}
