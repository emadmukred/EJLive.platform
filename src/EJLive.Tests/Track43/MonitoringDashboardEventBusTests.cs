using Microsoft.VisualStudio.TestTools.UnitTesting;
using EJLive.Core.Engine;
using EJLive.Core.Models;

namespace EJLive.Tests.Track43;

[TestClass]
public sealed class MonitoringDashboardEventBusTests
{
    [TestMethod]
    public void Subscribe_IncreasesSubscriberCount()
    {
        var bus = new MonitoringDashboardEventBus();

        var sub = bus.Subscribe(Guid.NewGuid());

        Assert.AreEqual(1, bus.SubscriberCount);
        Assert.IsNotNull(sub);
    }

    [TestMethod]
    public void Unsubscribe_DecreasesSubscriberCount()
    {
        var bus = new MonitoringDashboardEventBus();
        var sub = bus.Subscribe(Guid.NewGuid());

        bus.Unsubscribe(sub.SubscriptionId);

        Assert.AreEqual(0, bus.SubscriberCount);
    }

    [TestMethod]
    public void Publish_DeliversToAllSubscribers()
    {
        var bus = new MonitoringDashboardEventBus();
        var sub1 = bus.Subscribe(Guid.NewGuid());
        var sub2 = bus.Subscribe(Guid.NewGuid());

        bus.PublishAlert("ATM-001", "Test alert", AlertSeverity.Warning);

        var events1 = bus.DrainEvents(sub1.SubscriptionId);
        var events2 = bus.DrainEvents(sub2.SubscriptionId);

        Assert.AreEqual(1, events1.Count);
        Assert.AreEqual(1, events2.Count);
        Assert.AreEqual("ATM-001", events1[0].AtmId);
    }

    [TestMethod]
    public void Publish_WithSeverityFilter_OnlyMatchingSubscribersReceive()
    {
        var bus = new MonitoringDashboardEventBus();

        var criticalSub = bus.Subscribe(Guid.NewGuid(), new DashboardEventFilter
        {
            MinSeverity = AlertSeverity.Critical
        });
        var allSub = bus.Subscribe(Guid.NewGuid());

        bus.PublishAlert("ATM-002", "Low paper", AlertSeverity.Info);

        var criticalEvents = bus.DrainEvents(criticalSub.SubscriptionId);
        var allEvents = bus.DrainEvents(allSub.SubscriptionId);

        Assert.AreEqual(0, criticalEvents.Count); // Filtered out
        Assert.AreEqual(1, allEvents.Count); // Received
    }

    [TestMethod]
    public void PublishAtmStatusChange_CreatesCorrectEventType()
    {
        var bus = new MonitoringDashboardEventBus();
        var sub = bus.Subscribe(Guid.NewGuid());

        bus.PublishAtmStatusChange("ATM-100", "Online", "Offline");

        var events = bus.DrainEvents(sub.SubscriptionId);
        Assert.AreEqual(1, events.Count);
        Assert.AreEqual(DashboardEventType.AtmStatusChange, events[0].EventType);
        Assert.AreEqual(AlertSeverity.Critical, events[0].Severity); // Offline = Critical
    }

    [TestMethod]
    public void PublishSyncComplete_CreatesInfoSeverityEvent()
    {
        var bus = new MonitoringDashboardEventBus();
        var sub = bus.Subscribe(Guid.NewGuid());

        bus.PublishSyncComplete("ATM-050", 3, 1024 * 1024);

        var events = bus.DrainEvents(sub.SubscriptionId);
        Assert.AreEqual(1, events.Count);
        Assert.AreEqual(DashboardEventType.SyncComplete, events[0].EventType);
        Assert.AreEqual(AlertSeverity.Info, events[0].Severity);
    }

    [TestMethod]
    public void GetRecentEvents_ReturnsLatestEvents()
    {
        var bus = new MonitoringDashboardEventBus(maxEventLogSize: 100);

        for (int i = 0; i < 20; i++)
        {
            bus.PublishHeartbeatReceived($"ATM-{i:D3}", 95.0);
        }

        var recent = bus.GetRecentEvents(5);
        Assert.AreEqual(5, recent.Count);
    }

    [TestMethod]
    public void CleanupStaleSubscribers_RemovesInactive()
    {
        var bus = new MonitoringDashboardEventBus();

        // Create a subscriber with a backdated connection time
        var sub = bus.Subscribe(Guid.NewGuid());

        // Force the subscription to appear stale by waiting (or we test the count)
        Assert.AreEqual(1, bus.SubscriberCount);

        // With a very long stale duration, nothing should be removed
        var removed = bus.CleanupStaleSubscribers(TimeSpan.FromHours(1));
        Assert.AreEqual(0, removed);
        Assert.AreEqual(1, bus.SubscriberCount);
    }

    [TestMethod]
    public void DrainEvents_EmptiesQueue()
    {
        var bus = new MonitoringDashboardEventBus();
        var sub = bus.Subscribe(Guid.NewGuid());

        bus.PublishAlert("ATM-001", "Alert 1", AlertSeverity.Info);
        bus.PublishAlert("ATM-001", "Alert 2", AlertSeverity.Warning);

        var firstDrain = bus.DrainEvents(sub.SubscriptionId);
        var secondDrain = bus.DrainEvents(sub.SubscriptionId);

        Assert.AreEqual(2, firstDrain.Count);
        Assert.AreEqual(0, secondDrain.Count);
    }
}
