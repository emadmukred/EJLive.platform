using System.Text;
using EJLive.Client.Service;
using EJLive.Client.WinForms.Services;
using EJLive.Core.Engine;
using EJLive.Core.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests;

/// <summary>
/// Cross-layer integration tests for the active platform surface. Feature-level
/// behavior remains in the numbered track suites; this class verifies that the
/// selected Core, Service, and passive UI boundaries work together.
/// </summary>
[TestClass]
public sealed class PlatformRuntimeIntegrationTests
{
    [TestMethod]
    public void SecurityPayload_CompressEncryptDecrypt_RoundTrips()
    {
        const string source = "EJLive platform payload";
        var encrypted = EJLive.Shared.SecurityHelper.CompressAndEncrypt(Encoding.UTF8.GetBytes(source));
        var plain = EJLive.Shared.SecurityHelper.DecryptAndDecompress(encrypted);

        Assert.AreEqual(source, Encoding.UTF8.GetString(plain));
    }

    [TestMethod]
    public void SignedCommandEnvelope_RoundTripsAndVerifies()
    {
        var command = new EJLive.Core.Engine.RemoteCommandEnvelope
        {
            CommandId = "cmd-001",
            CommandType = "PING",
            Payload = "ATM-001",
            RequiresConfirmation = false
        };

        var wireText = command.ToWireText();
        var parsed = EJLive.Core.Engine.RemoteCommandEnvelope.TryParse(wireText, out var restored);

        Assert.IsTrue(parsed, restored.SignatureFailureReason);
        Assert.IsTrue(restored.SignatureVerified);
        Assert.AreEqual(command.CommandId, restored.CommandId);
        Assert.AreEqual(command.Payload, restored.Payload);
    }

    [TestMethod]
    public void SignedCommandEnvelope_RejectsTamperedPayload()
    {
        var command = new EJLive.Core.Engine.RemoteCommandEnvelope
        {
            CommandId = "cmd-002",
            CommandType = "PING",
            Payload = "ATM-001"
        };

        var parts = command.ToWireText().Split('|');
        parts[3] = Convert.ToBase64String(Encoding.UTF8.GetBytes("ATM-OTHER"));
        var parsed = EJLive.Core.Engine.RemoteCommandEnvelope.TryParse(string.Join('|', parts), out var rejected);

        Assert.IsFalse(parsed);
        StringAssert.Contains(rejected.SignatureFailureReason, "signature");
    }

    [TestMethod]
    public async Task IngestionPipeline_ArchivesAndParsesVendorJournal()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-ingestion-{Guid.NewGuid():N}");
        var stagingPath = Path.Combine(root, "staging", "journal.log");
        var archivePath = Path.Combine(root, "archive");
        Directory.CreateDirectory(Path.GetDirectoryName(stagingPath)!);
        await File.WriteAllLinesAsync(stagingPath, new[]
        {
            "*TRANSACTION START*",
            "CARD INSERTED",
            "APPROVED",
            "NOTES PRESENTED",
            "NOTES TAKEN",
            "TRANSACTION END"
        });

        try
        {
            var pipeline = new DefaultIngestionPipeline(archivePath, new EjParserRegistry());
            var record = new IngestionRecord
            {
                AtmId = "ATM-INGEST-01",
                Vendor = "NCR",
                FileName = "journal.log",
                FileSize = new FileInfo(stagingPath).Length,
                ReceivedUtc = new DateTimeOffset(2026, 7, 12, 10, 0, 0, TimeSpan.Zero)
            };

            var result = await pipeline.IngestAsync(record, stagingPath, CancellationToken.None);

            Assert.AreEqual(IngestionStage.SnapshotReady, result.Stage, result.FailureReason);
            Assert.AreEqual(1, result.TransactionCount);
            Assert.IsTrue(File.Exists(result.ArchivePath));
            Assert.AreEqual(1, pipeline.GetSummary().TotalParsed);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ReportingExport_SaveCsv_ConfinesAndAuditsOutput()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-report-{Guid.NewGuid():N}");
        string? publishedPath = null;

        try
        {
            var exporter = new ReportingExportEngine(root);
            exporter.OnReportExported += (_, path) => publishedPath = path;

            var savedPath = exporter.SaveCsv("../fleet-status", "atmId,status\nATM-01,Running");

            Assert.AreEqual(Path.GetFullPath(root), Path.GetDirectoryName(Path.GetFullPath(savedPath)));
            Assert.IsTrue(File.Exists(savedPath));
            Assert.AreEqual(savedPath, publishedPath);
            CollectionAssert.Contains(exporter.GetExportHistory().ToList(), savedPath);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void WindowsPolicy_EnforceModeWithoutWriter_FailsClosed()
    {
        var enforcer = new SafeWindowsPolicyEnforcer(
            new SafeWindowsPolicyConfig
            {
                Mode = PolicyEnforcementMode.Enforce,
                ExplicitEnforceEnabled = true
            },
            isElevated: () => true,
            isDomainJoined: () => false,
            hasGpoOverride: _ => false);

        var applied = enforcer.TryApplyRdpPolicy(
            "RDP", "{}", "{\"enabled\":true}", "{}", "operator-1", "approved test", out var error);

        Assert.IsFalse(applied);
        Assert.AreEqual(1, enforcer.Snapshots.Count);
        StringAssert.Contains(error, "policy writer");
    }

    [TestMethod]
    public async Task ServiceHealthSnapshot_FlowsToPassiveClientGateway()
    {
        var healthPath = Path.Combine(Path.GetTempPath(), $"ejlive-health-{Guid.NewGuid():N}.json");
        using var controller = new AgentHeadlessController();
        using var reporter = new AgentHealthReporter(controller, TimeSpan.FromHours(1), healthPath);

        try
        {
            reporter.Emit();
            var gateway = new InProcessClientServiceGateway(
                () => new ClientGatewayContext { AtmId = "fallback" },
                healthPath);

            var snapshot = await gateway.GetRuntimeSnapshotAsync();
            var query = await gateway.QueryLocalServiceAsync();

            Assert.AreEqual("ServiceHealthFile", snapshot.SnapshotSource);
            Assert.AreEqual(controller.AtmId, snapshot.AtmId);
            Assert.AreEqual(AgentControllerState.Stopped.ToString(), snapshot.AgentState);
            Assert.IsTrue(query.Available);
        }
        finally
        {
            if (File.Exists(healthPath))
                File.Delete(healthPath);
        }
    }

    [TestMethod]
    public void NetworkEngine_OfflineJournal_IsQueuedWithoutNetworkMutation()
    {
        var fileName = $"offline-{Guid.NewGuid():N}.log";
        using var engine = new NetworkEngine("127.0.0.1", 1, "ATM-OFFLINE", "NCR");

        var sent = engine.SendJournalFile(fileName, new byte[] { 1, 2, 3 }, 0, "checksum");
        var queued = engine.Outbox.Snapshot.Single(item => item.FileName == fileName);

        Assert.IsFalse(sent);
        Assert.IsFalse(engine.IsConnected);
        Assert.AreEqual(0L, engine.TotalBytesSent);
        engine.Outbox.MarkCompleted(queued.ItemId, "test-cleanup");
    }
}
