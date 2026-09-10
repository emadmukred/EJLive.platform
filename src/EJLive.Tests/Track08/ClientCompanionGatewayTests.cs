using EJLive.Client.WinForms.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track08;

[TestClass]
public class ClientCompanionGatewayTests
{
    [TestMethod]
    public async Task InProcessGateway_UsesServiceHealthFileWhenAvailable()
    {
        var healthPath = CreateHealthSnapshot();

        try
        {
            var gateway = new InProcessClientServiceGateway(
                () => new ClientGatewayContext
                {
                    AtmId = "ATM-FALLBACK",
                    AgentState = "Stopped",
                    Connected = false,
                    PendingOutboxItems = 99
                },
                healthPath);

            var snapshot = await gateway.GetRuntimeSnapshotAsync().ConfigureAwait(false);

            Assert.AreEqual("ServiceHealthFile", snapshot.SnapshotSource);
            Assert.AreEqual("ATM-SVC-01", snapshot.AtmId);
            Assert.IsTrue(snapshot.Connected);
            Assert.IsTrue(snapshot.HandshakeComplete);
            Assert.AreEqual(2, snapshot.PendingOutboxItems);
            Assert.AreEqual("sess-svc", snapshot.SessionId);
        }
        finally
        {
            DeleteHealthSnapshot(healthPath);
        }
    }

    [TestMethod]
    public async Task InProcessGateway_QueryLocalService_IsReadOnly()
    {
        var healthPath = CreateHealthSnapshot();
        var directory = Path.GetDirectoryName(healthPath)!;

        try
        {
            var before = Directory.GetFiles(directory)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var gateway = new InProcessClientServiceGateway(
                () => new ClientGatewayContext(),
                healthPath);

            var result = await gateway.QueryLocalServiceAsync().ConfigureAwait(false);
            var after = Directory.GetFiles(directory)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Assert.IsTrue(result.Available);
            Assert.AreEqual("Running", result.State);
            Assert.IsNotNull(result.CapturedAtUtc);
            CollectionAssert.AreEqual(before, after, "A status query must not create probe files.");
        }
        finally
        {
            DeleteHealthSnapshot(healthPath);
        }
    }

    [TestMethod]
    public async Task InProcessGateway_QueryLocalService_ReportsMissingSnapshot()
    {
        var missingPath = Path.Combine(
            Path.GetTempPath(),
            $"ejlive-missing-health-{Guid.NewGuid():N}.json");
        var gateway = new InProcessClientServiceGateway(
            () => new ClientGatewayContext(),
            missingPath);

        var result = await gateway.QueryLocalServiceAsync().ConfigureAwait(false);

        Assert.IsFalse(result.Available);
        Assert.AreEqual("Unknown", result.State);
        Assert.IsNull(result.CapturedAtUtc);
    }

    [TestMethod]
    public async Task InProcessGateway_InvalidSnapshot_FallsBackWithoutThrowing()
    {
        var healthPath = Path.Combine(
            Path.GetTempPath(),
            $"ejlive-invalid-health-{Guid.NewGuid():N}.json");
        File.WriteAllText(healthPath, "{ incomplete");

        try
        {
            var gateway = new InProcessClientServiceGateway(
                () => new ClientGatewayContext
                {
                    AtmId = "ATM-FALLBACK",
                    AgentState = "Running",
                    PendingOutboxItems = -4,
                    Components = new[]
                    {
                        new ClientComponentSnapshot("Agent Controller", "Running", "fallback")
                    }
                },
                healthPath);

            var snapshot = await gateway.GetRuntimeSnapshotAsync().ConfigureAwait(false);

            Assert.AreEqual("InProcessFallback", snapshot.SnapshotSource);
            Assert.AreEqual("ATM-FALLBACK", snapshot.AtmId);
            Assert.AreEqual(0, snapshot.PendingOutboxItems);
            Assert.AreEqual(1, snapshot.Components.Count);
        }
        finally
        {
            File.Delete(healthPath);
        }
    }

    [TestMethod]
    public void ClientServiceGateway_ExposesOnlyReadOperations()
    {
        var methodNames = typeof(IClientServiceGateway)
            .GetMethods()
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(
            new[]
            {
                nameof(IClientServiceGateway.GetRuntimeSnapshotAsync),
                nameof(IClientServiceGateway.QueryLocalServiceAsync)
            },
            methodNames);
    }

    private static string CreateHealthSnapshot()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            $"ejlive-health-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var healthPath = Path.Combine(directory, "health.json");
        File.WriteAllText(
            healthPath,
            "{" +
            "\"atmId\":\"ATM-SVC-01\"," +
            "\"timestampUtc\":\"2026-05-26T19:00:00Z\"," +
            "\"state\":\"Running\"," +
            "\"connected\":true," +
            "\"handshakeComplete\":true," +
            "\"pendingOutboxItems\":2," +
            "\"totalBytesSent\":1000," +
            "\"totalBytesReceived\":2000," +
            "\"lastHeartbeatUtc\":\"2026-05-26T18:59:30Z\"," +
            "\"lastJournalSyncUtc\":\"2026-05-26T18:59:50Z\"," +
            "\"sessionId\":\"sess-svc\"," +
            "\"lastError\":\"\"," +
            "\"uptimeSeconds\":300" +
            "}");
        return healthPath;
    }

    private static void DeleteHealthSnapshot(string healthPath)
    {
        File.Delete(healthPath);
        var directory = Path.GetDirectoryName(healthPath);
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            Directory.Delete(directory);
    }
}
