using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Server.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests;

[TestClass]
public sealed class RuntimeServiceBehaviorTests
{
    [TestMethod]
    public void DefaultDatabasePath_HonoursEnvironmentVariableOverride()
    {
        var previous = Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH");
        var sentinel = Path.Combine(Path.GetTempPath(), $"ejlive-defaultdb-{Guid.NewGuid():N}.db");
        try
        {
            Environment.SetEnvironmentVariable("EJLIVE_DATABASE_PATH", sentinel);
            Assert.AreEqual(sentinel, AppConstants.DefaultDatabasePath);
        }
        finally
        {
            Environment.SetEnvironmentVariable("EJLIVE_DATABASE_PATH", previous);
        }
    }

    [TestMethod]
    public void DefaultDatabasePath_FallsBackToApplicationData()
    {
        var previous = Environment.GetEnvironmentVariable("EJLIVE_DATABASE_PATH");
        try
        {
            Environment.SetEnvironmentVariable("EJLIVE_DATABASE_PATH", null);
            Assert.IsTrue(AppConstants.DefaultDatabasePath.EndsWith("ejlive.db", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Environment.SetEnvironmentVariable("EJLIVE_DATABASE_PATH", previous);
        }
    }

    [TestMethod]
    public void JournalAnalyticsService_ImplementsDisposableLifecycle()
    {
        Assert.IsTrue(typeof(IDisposable).IsAssignableFrom(typeof(JournalAnalyticsService)));
        Assert.IsTrue(typeof(JournalAnalyticsService).IsSealed);

        var root = Path.Combine(Path.GetTempPath(), $"ejlive-analytics-{Guid.NewGuid():N}");
        try
        {
            using (new JournalAnalyticsService(Path.Combine(root, "storage"), Path.Combine(root, "archive")))
            {
                Assert.IsTrue(Directory.Exists(Path.Combine(root, "storage")));
                Assert.IsTrue(Directory.Exists(Path.Combine(root, "archive")));
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void JournalAnalyticsService_DtosHaveNonNullDefaults()
    {
        var stats = new ATMJournalStats();
        var record = new JournalRecord();

        Assert.IsNotNull(stats.ATM_ID);
        Assert.IsNotNull(stats.LastFileName);
        Assert.IsNotNull(record.ATM_ID);
        Assert.IsNotNull(record.FileName);
        Assert.IsNotNull(record.Checksum);
        Assert.IsNotNull(record.StoragePath);
    }

    [TestMethod]
    public void JournalAnalyticsService_StoresAndAggregatesJournal()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ejlive-analytics-store-{Guid.NewGuid():N}");
        try
        {
            using var service = new JournalAnalyticsService(Path.Combine(root, "storage"), Path.Combine(root, "archive"));
            var data = System.Text.Encoding.UTF8.GetBytes(
                "NCR EJDATA APPROVED AMOUNT 200\nWITHDRAWAL 200\nDISPENSE 200\nERROR JAM\nCARD CAPTURED");
            service.StoreJournalData("ATM-TEST", "EJDATA.LOG", data, checksum: "deadbeef");

            var snapshot = service.GetATMStats("ATM-TEST");
            Assert.AreEqual(1, snapshot.TotalFiles);
            Assert.IsTrue(snapshot.TotalWithdrawals >= 1);
            Assert.IsTrue(snapshot.TotalErrors >= 1);
            Assert.AreEqual("deadbeef", service.GetRecentRecords(10, "ATM-TEST").Single().Checksum);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void RemoteControlService_ImplementsDisposableLifecycle()
    {
        Assert.IsTrue(typeof(IDisposable).IsAssignableFrom(typeof(RemoteControlService)));
        Assert.IsTrue(typeof(RemoteControlService).IsSealed);
        using var server = new ServerEngine();
        using (new RemoteControlService(server)) { }
        using (new RemoteControlService(server)) { }
    }

    [TestMethod]
    public void RemoteControlService_RecordsOfflineCommandResult()
    {
        using var server = new ServerEngine();
        using var service = new RemoteControlService(server);

        var commandId = service.SendScreenshot("ATM-NOT-CONNECTED");
        var record = service.GetCommandHistory("ATM-NOT-CONNECTED", 1).Single();

        Assert.AreEqual(commandId, record.CommandId);
        Assert.IsFalse(record.Sent);
        Assert.IsFalse(record.Completed);
        Assert.AreEqual("No active connection", record.Result);
    }

    [TestMethod]
    public void RemoteControlService_BroadcastReturnsZeroWithoutConnections()
    {
        using var server = new ServerEngine();
        using var service = new RemoteControlService(server);

        Assert.AreEqual(0, service.BroadcastRestart(delaySeconds: 5));
        Assert.AreEqual(0, service.BroadcastTimeSync());
        Assert.AreEqual(0, service.BroadcastScreenshot());
    }
}
