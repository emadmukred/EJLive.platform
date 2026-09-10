using EJLive.Core.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track36;

[TestClass]
public sealed class LocalAtmHealthSnapshotTests
{
    [TestMethod]
    public void Capture_AllHealthy_ReturnsMaximumScore()
    {
        var report = LocalAtmHealthSnapshot.Capture(
            "ATM-001", "NCR", connected: true, handshakeComplete: true,
            pendingOutbox: 5, lastSyncUtc: DateTime.UtcNow, journalPath: null);

        Assert.AreEqual(100, report.HealthScore);
        Assert.AreEqual("ATM-001", report.AtmId);
        Assert.IsTrue(report.Connected);
    }

    [TestMethod]
    public void Capture_Disconnected_ReducesScore()
    {
        var report = LocalAtmHealthSnapshot.Capture(
            "ATM-002", "GRG", connected: false, handshakeComplete: true,
            pendingOutbox: 0, lastSyncUtc: DateTime.UtcNow, journalPath: null);

        Assert.IsTrue(report.HealthScore < 100);
        Assert.IsFalse(report.Connected);
    }

    [TestMethod]
    public void Capture_MultipleFaults_ProducesLowBoundedScore()
    {
        var report = LocalAtmHealthSnapshot.Capture(
            "ATM-003", "Wincor", connected: false, handshakeComplete: false,
            pendingOutbox: 200, lastSyncUtc: null, journalPath: null);

        Assert.IsTrue(report.HealthScore is >= 0 and <= 20, $"Unexpected score: {report.HealthScore}");
    }

    [TestMethod]
    public void ToJson_UsesPortableCamelCaseContract()
    {
        var report = LocalAtmHealthSnapshot.Capture(
            "ATM-004", "Cashway", connected: true, handshakeComplete: true,
            pendingOutbox: 0, lastSyncUtc: DateTime.UtcNow, journalPath: null);

        var json = LocalAtmHealthSnapshot.ToJson(report);

        StringAssert.Contains(json, "\"atmId\"");
        StringAssert.Contains(json, "\"healthScore\"");
    }
}
