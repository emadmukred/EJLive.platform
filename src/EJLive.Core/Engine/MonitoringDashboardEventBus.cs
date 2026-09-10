using System.Collections.Concurrent;
using EJLive.Core.Models;

namespace EJLive.Core.Engine;

/// <summary>
/// Track 043 – Monitoring Dashboard Real-Time Event Push.
/// Manages server-sent event streams for the monitoring dashboard.
/// Distributes real-time ATM status changes, alerts, and sync events
/// to connected dashboard clients without polling.
/// </summary>
public class DashboardEventBus
{
    private readonly ConcurrentDictionary<Guid, DashboardSubscription> _subscribers = new();
    private readonly ConcurrentQueue<DashboardEvent> _eventLog = new();
    private readonly int _maxEventLogSize;

    /// <summary>
    /// Initializes a new dashboard event bus.
    /// </summary>
    /// <param name="maxEventLogSize">Maximum number of events retained in the circular log.</param>
    public DashboardEventBus(int maxEventLogSize = 10000)
    {
        _maxEventLogSize = maxEventLogSize;
    }

    /// <summary>
    /// Gets the number of currently active subscribers.
    /// </summary>
    public int SubscriberCount => _subscribers.Count;

    /// <summary>
    /// Subscribes a dashboard client to receive real-time events.
    /// </summary>
    /// <param name="clientId">Unique identifier for the dashboard client session.</param>
    /// <param name="filter">Optional filter to receive only specific event types.</param>
    /// <returns>A subscription handle.</returns>
    public DashboardSubscription Subscribe(Guid clientId, DashboardEventFilter? filter = null)
    {
        var subscription = new DashboardSubscription
        {
            SubscriptionId = Guid.NewGuid(),
            ClientId = clientId,
            Filter = filter ?? DashboardEventFilter.All,
            ConnectedUtc = DateTime.UtcNow,
            EventQueue = new ConcurrentQueue<DashboardEvent>()
        };

        _subscribers[subscription.SubscriptionId] = subscription;
        return subscription;
    }

    /// <summary>
    /// Removes a dashboard client subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription to remove.</param>
    public void Unsubscribe(Guid subscriptionId)
    {
        _subscribers.TryRemove(subscriptionId, out _);
    }

    /// <summary>
    /// Publishes an event to all matching subscribers and the event log.
    /// </summary>
    /// <param name="dashboardEvent">The event to publish.</param>
    public void Publish(DashboardEvent dashboardEvent)
    {
        // Add to circular event log
        _eventLog.Enqueue(dashboardEvent);
        while (_eventLog.Count > _maxEventLogSize)
        {
            _eventLog.TryDequeue(out _);
        }

        // Distribute to matching subscribers
        foreach (var (_, subscription) in _subscribers)
        {
            if (MatchesFilter(dashboardEvent, subscription.Filter))
            {
                subscription.EventQueue.Enqueue(dashboardEvent);
                subscription.LastEventUtc = DateTime.UtcNow;
            }
        }
    }

    /// <summary>
    /// Publishes an ATM status change event.
    /// </summary>
    public void PublishAtmStatusChange(string atmId, string previousStatus, string newStatus)
    {
        Publish(new DashboardEvent
        {
            EventId = Guid.NewGuid(),
            EventType = DashboardEventType.AtmStatusChange,
            AtmId = atmId,
            TimestampUtc = DateTime.UtcNow,
            Payload = $"Status changed from {previousStatus} to {newStatus}",
            Severity = newStatus == "Offline" ? AlertSeverity.Critical : AlertSeverity.Info
        });
    }

    /// <summary>
    /// Publishes an alert event for the dashboard.
    /// </summary>
    public void PublishAlert(string atmId, string message, AlertSeverity severity)
    {
        Publish(new DashboardEvent
        {
            EventId = Guid.NewGuid(),
            EventType = DashboardEventType.Alert,
            AtmId = atmId,
            TimestampUtc = DateTime.UtcNow,
            Payload = message,
            Severity = severity
        });
    }

    /// <summary>
    /// Publishes a sync completion event.
    /// </summary>
    public void PublishSyncComplete(string atmId, int filesTransferred, long bytesTransferred)
    {
        Publish(new DashboardEvent
        {
            EventId = Guid.NewGuid(),
            EventType = DashboardEventType.SyncComplete,
            AtmId = atmId,
            TimestampUtc = DateTime.UtcNow,
            Payload = $"Synced {filesTransferred} files ({bytesTransferred:N0} bytes)",
            Severity = AlertSeverity.Info
        });
    }

    /// <summary>
    /// Publishes a heartbeat received event.
    /// </summary>
    public void PublishHeartbeatReceived(string atmId, double healthScore)
    {
        var severity = healthScore switch
        {
            >= 80 => AlertSeverity.Info,
            >= 50 => AlertSeverity.Warning,
            _ => AlertSeverity.Critical
        };

        Publish(new DashboardEvent
        {
            EventId = Guid.NewGuid(),
            EventType = DashboardEventType.HeartbeatReceived,
            AtmId = atmId,
            TimestampUtc = DateTime.UtcNow,
            Payload = $"Health score: {healthScore:F1}",
            Severity = severity
        });
    }

    /// <summary>
    /// Drains pending events from a subscription's queue.
    /// </summary>
    /// <param name="subscriptionId">The subscription to drain.</param>
    /// <param name="maxEvents">Maximum number of events to return.</param>
    /// <returns>The pending events for this subscription.</returns>
    public IReadOnlyList<DashboardEvent> DrainEvents(Guid subscriptionId, int maxEvents = 100)
    {
        if (!_subscribers.TryGetValue(subscriptionId, out var subscription))
        {
            return Array.Empty<DashboardEvent>();
        }

        var events = new List<DashboardEvent>(Math.Min(maxEvents, subscription.EventQueue.Count));
        while (events.Count < maxEvents && subscription.EventQueue.TryDequeue(out var evt))
        {
            events.Add(evt);
        }

        return events.AsReadOnly();
    }

    /// <summary>
    /// Returns recent events from the event log for catch-up purposes.
    /// </summary>
    /// <param name="count">Number of recent events to return.</param>
    public IReadOnlyList<DashboardEvent> GetRecentEvents(int count = 50)
    {
        return _eventLog.Reverse().Take(count).Reverse().ToList().AsReadOnly();
    }

    /// <summary>
    /// Removes stale subscribers that have not drained events for the specified duration.
    /// </summary>
    /// <param name="staleDuration">How long a subscriber can be idle before cleanup.</param>
    /// <returns>Number of stale subscribers removed.</returns>
    public int CleanupStaleSubscribers(TimeSpan staleDuration)
    {
        var cutoff = DateTime.UtcNow - staleDuration;
        var stale = _subscribers
            .Where(kvp => kvp.Value.LastEventUtc < cutoff && kvp.Value.ConnectedUtc < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in stale)
        {
            _subscribers.TryRemove(id, out _);
        }

        return stale.Count;
    }

    private static bool MatchesFilter(DashboardEvent evt, DashboardEventFilter filter)
    {
        if (filter.EventTypes is not null && !filter.EventTypes.Contains(evt.EventType))
            return false;

        if (filter.MinSeverity.HasValue && evt.Severity < filter.MinSeverity.Value)
            return false;

        if (!string.IsNullOrWhiteSpace(filter.AtmIdPrefix)
            && !evt.AtmId.StartsWith(filter.AtmIdPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}

/// <summary>
/// Monitoring-dashboard specialization of the shared event bus.
/// </summary>
public sealed class MonitoringDashboardEventBus : DashboardEventBus
{
    public MonitoringDashboardEventBus(int maxEventLogSize = 10000) : base(maxEventLogSize)
    {
    }
}

/// <summary>
/// A real-time event delivered to dashboard subscribers.
/// </summary>
public sealed record DashboardEvent
{
    public required Guid EventId { get; init; }
    public required DashboardEventType EventType { get; init; }
    public required string AtmId { get; init; }
    public required DateTime TimestampUtc { get; init; }
    public required string Payload { get; init; }
    public required AlertSeverity Severity { get; init; }
}

/// <summary>
/// Types of events published to the monitoring dashboard.
/// </summary>
public enum DashboardEventType
{
    AtmStatusChange,
    Alert,
    SyncComplete,
    HeartbeatReceived,
    CommandExecuted,
    MaintenanceScheduled,
    ConnectionLost,
    ConnectionRestored
}

/// <summary>
/// Filter criteria for a dashboard event subscription.
/// </summary>
public sealed record DashboardEventFilter
{
    /// <summary>
    /// Event types to subscribe to. Null means all types.
    /// </summary>
    public IReadOnlySet<DashboardEventType>? EventTypes { get; init; }

    /// <summary>
    /// Minimum severity to receive. Null means all severities.
    /// </summary>
    public AlertSeverity? MinSeverity { get; init; }

    /// <summary>
    /// ATM ID prefix filter (e.g., "ATM-01" for a region).
    /// </summary>
    public string? AtmIdPrefix { get; init; }

    /// <summary>
    /// Returns a filter that matches all events.
    /// </summary>
    public static DashboardEventFilter All => new();
}

/// <summary>
/// Represents an active dashboard client subscription.
/// </summary>
public sealed class DashboardSubscription
{
    public required Guid SubscriptionId { get; init; }
    public required Guid ClientId { get; init; }
    public required DashboardEventFilter Filter { get; init; }
    public required DateTime ConnectedUtc { get; init; }
    public required ConcurrentQueue<DashboardEvent> EventQueue { get; init; }
    public DateTime LastEventUtc { get; set; } = DateTime.UtcNow;
}
