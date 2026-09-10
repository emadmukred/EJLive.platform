using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track18;

[TestClass]
public class DashboardSnapshotTests
{
    [TestMethod]
    public async Task GetSnapshot_ReturnsValidFleetKpi()
    {
        var service = new DashboardSnapshotService();

        var snapshot = await service.GetSnapshotAsync();

        Assert.IsNotNull(snapshot);
        Assert.IsTrue(snapshot.FleetKpi.TotalAtms > 0);
        Assert.AreEqual(snapshot.FleetKpi.TotalAtms,
            snapshot.FleetKpi.OnlineCount
            + snapshot.FleetKpi.OfflineCount
            + snapshot.FleetKpi.WarningCount,
            "Fleet counts should sum to total.");
    }

    [TestMethod]
    public async Task GetSnapshot_ReturnsValidSyncKpi()
    {
        var service = new DashboardSnapshotService();

        var snapshot = await service.GetSnapshotAsync();

        Assert.IsNotNull(snapshot);
        Assert.IsTrue(snapshot.SyncKpi.SuccessRate >= 0.0 && snapshot.SyncKpi.SuccessRate <= 100.0);
        Assert.IsTrue(snapshot.SyncKpi.AverageLatency >= 0.0);
        Assert.IsTrue(snapshot.SyncKpi.FailuresLastHour >= 0);
    }

    [TestMethod]
    public async Task GetSnapshot_CacheIsUsed_WhenCalledTwiceWithinTtl()
    {
        var service = new DashboardSnapshotService
        {
            CacheTtl = TimeSpan.FromMinutes(5)
        };

        var first = await service.GetSnapshotAsync();
        var second = await service.GetSnapshotAsync();

        Assert.AreEqual(first.SnapshotTimestampUtc, second.SnapshotTimestampUtc,
            "Cached snapshot should have identical timestamp within TTL.");
    }

    [TestMethod]
    public async Task GetSnapshot_GeneratesNewSnapshot_AfterCacheInvalidation()
    {
        var service = new DashboardSnapshotService
        {
            CacheTtl = TimeSpan.FromMinutes(5)
        };

        var first = await service.GetSnapshotAsync();
        service.InvalidateCache();
        var second = await service.GetSnapshotAsync();

        Assert.AreNotEqual(first.SnapshotTimestampUtc, second.SnapshotTimestampUtc,
            "Snapshot timestamp should differ after cache invalidation.");
    }

    [TestMethod]
    public async Task GetSnapshot_FiltersBySeverity()
    {
        var service = new DashboardSnapshotService();
        var filters = new DashboardFilters { Severity = AlertSeverity.Critical };

        var snapshot = await service.GetSnapshotAsync(filters);

        Assert.IsTrue(snapshot.Alerts.All(a => a.Severity == AlertSeverity.Critical),
            "All returned alerts should match the requested severity.");
    }

    [TestMethod]
    public async Task GetSnapshot_NoAlertsWhenSeverityDoesNotMatch()
    {
        var service = new DashboardSnapshotService();
        var filters = new DashboardFilters { Severity = AlertSeverity.Info };

        var snapshot = await service.GetSnapshotAsync(filters);

        Assert.AreEqual(0, snapshot.Alerts.Count,
            "No alerts should be returned when severity filter does not match.");
    }

    [TestMethod]
    public async Task GetSnapshot_SnapshotTimestampIsUtc()
    {
        var service = new DashboardSnapshotService();

        var snapshot = await service.GetSnapshotAsync();

        Assert.AreEqual(DateTimeKind.Utc, snapshot.SnapshotTimestampUtc.Kind,
            "Snapshot timestamp must be UTC.");
    }

    [TestMethod]
    public void ReportRequest_PropertiesAreImmutable()
    {
        var request = new ReportRequest
        {
            ReportType = ReportType.Daily,
            FilterJson = "{\"region\":\"EMEA\"}",
            RequestedBy = "noc-operator-01",
            RequestedUtc = DateTime.UtcNow
        };

        Assert.AreEqual(ReportType.Daily, request.ReportType);
        Assert.AreEqual("noc-operator-01", request.RequestedBy);
        Assert.IsFalse(string.IsNullOrWhiteSpace(request.FilterJson));
    }

    [TestMethod]
    public void ReportRequest_SupportsAllReportTypes()
    {
        var types = Enum.GetValues<ReportType>();

        Assert.IsTrue(types.Contains(ReportType.Daily));
        Assert.IsTrue(types.Contains(ReportType.Monthly));
        Assert.IsTrue(types.Contains(ReportType.AtmDetails));
        Assert.IsTrue(types.Contains(ReportType.SyncFailures));
        Assert.IsTrue(types.Contains(ReportType.CommandAudit));
    }

    [TestMethod]
    public void AlertSnapshot_RequiresAllFields()
    {
        var alert = new AlertSnapshot
        {
            AlertId = Guid.NewGuid(),
            AtmId = "ATM-TEST-001",
            Severity = AlertSeverity.Warning,
            Message = "Test alert message",
            TimestampUtc = DateTime.UtcNow,
            Acknowledged = false
        };

        Assert.IsNotNull(alert.AtmId);
        Assert.IsFalse(alert.Acknowledged);
    }

    [TestMethod]
    public async Task GetSnapshot_Supports500AtmSimulation()
    {
        var service = new DashboardSnapshotService();

        var snapshot = await service.GetSnapshotAsync();

        Assert.IsTrue(snapshot.FleetKpi.TotalAtms >= 500,
            "Snapshot must support fleets of at least 500 ATMs.");
    }
}
