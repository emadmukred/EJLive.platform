using System;
using System.Reflection;
using EJLive.Client.WinForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track08;

[TestClass]
public class ClientCompanionStatusTests
{
    [TestMethod]
    public void ClientMainForm_ServiceHealthSnapshotParser_ReadsExpectedFields()
    {
        var method = typeof(ClientMainForm).GetMethod(
            "TryParseServiceHealthSnapshot",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);

        var heartbeatUtc = new DateTime(2026, 05, 26, 18, 00, 00, DateTimeKind.Utc);
        var syncUtc = new DateTime(2026, 05, 26, 18, 05, 00, DateTimeKind.Utc);
        var json =
            "{" +
            "\"atmId\":\"ATM-TEST-01\"," +
            "\"timestampUtc\":\"2026-05-26T18:06:00Z\"," +
            "\"state\":2," +
            "\"connected\":true," +
            "\"handshakeComplete\":true," +
            "\"pendingOutboxItems\":3," +
            "\"totalBytesSent\":2048," +
            "\"totalBytesReceived\":4096," +
            "\"lastHeartbeatUtc\":\"" + heartbeatUtc.ToString("O") + "\"," +
            "\"lastJournalSyncUtc\":\"" + syncUtc.ToString("O") + "\"," +
            "\"sessionId\":\"sess-01\"," +
            "\"lastError\":\"\"," +
            "\"uptimeSeconds\":123.5" +
            "}";

        var args = new object?[] { json, null };
        var parsed = (bool)(method!.Invoke(null, args) ?? false);
        Assert.IsTrue(parsed);
        Assert.IsNotNull(args[1]);

        var snapshot = args[1]!;
        Assert.AreEqual("Running", ReadProperty(snapshot, "State"));
        Assert.AreEqual(true, ReadProperty(snapshot, "Connected"));
        Assert.AreEqual(3, ReadProperty(snapshot, "PendingOutboxItems"));
        Assert.AreEqual("sess-01", ReadProperty(snapshot, "SessionId"));
    }

    [TestMethod]
    public void ClientMainForm_HeartbeatStatusClassification_CoversExpectedBuckets()
    {
        var method = typeof(ClientMainForm).GetMethod(
            "ClassifyHeartbeatServiceStatus",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);

        var runningArgs = new object?[] { DateTime.UtcNow.AddSeconds(-10), null };
        var running = (string)(method!.Invoke(null, runningArgs) ?? string.Empty);
        Assert.AreEqual("Running", running);
        Assert.IsTrue(Convert.ToString(runningArgs[1])!.Contains("age=", StringComparison.OrdinalIgnoreCase));

        var warningArgs = new object?[] { DateTime.UtcNow.AddSeconds(-(EJLive.Core.AppConstants.HeartbeatTimeoutSec - 5)), null };
        var warning = (string)(method.Invoke(null, warningArgs) ?? string.Empty);
        Assert.AreEqual("Warning", warning);

        var failedArgs = new object?[] { DateTime.UtcNow.AddSeconds(-(EJLive.Core.AppConstants.HeartbeatTimeoutSec * 4)), null };
        var failed = (string)(method.Invoke(null, failedArgs) ?? string.Empty);
        Assert.AreEqual("Failed", failed);
    }

    private static object? ReadProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(property, $"Property {propertyName} was not found.");
        return property!.GetValue(instance);
    }
}
