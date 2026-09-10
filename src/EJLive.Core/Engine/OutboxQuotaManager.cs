namespace EJLive.Core.Engine;

/// <summary>
/// Manages journal outbox maintenance including quota enforcement,
/// cleanup of stale entries, compaction, and statistics reporting.
/// Implements Track 037 (Outbox Maintenance and Quota).
/// </summary>
public sealed class OutboxQuotaManager
{
    private readonly long _maxQueueSizeBytes;
    private readonly int _maxPendingItems;
    private readonly TimeSpan _staleThreshold;

    /// <summary>
    /// Initializes the outbox quota manager.
    /// </summary>
    /// <param name="maxQueueSizeMb">Maximum total queue size in MB. Default 500 MB.</param>
    /// <param name="maxPendingItems">Maximum pending items before throttling. Default 10000.</param>
    /// <param name="staleThreshold">Age after which failed items are considered stale. Default 7 days.</param>
    public OutboxQuotaManager(
        int maxQueueSizeMb = 500,
        int maxPendingItems = 10000,
        TimeSpan? staleThreshold = null)
    {
        _maxQueueSizeBytes = (long)maxQueueSizeMb * 1024 * 1024;
        _maxPendingItems = maxPendingItems;
        _staleThreshold = staleThreshold ?? TimeSpan.FromDays(7);
    }

    /// <summary>
    /// Evaluates whether a new item can be enqueued given current state.
    /// </summary>
    /// <param name="currentPendingCount">Current number of pending items.</param>
    /// <param name="currentQueueSizeBytes">Current total size of queued data.</param>
    /// <returns>True if the item can be enqueued.</returns>
    public bool CanEnqueue(int currentPendingCount, long currentQueueSizeBytes)
    {
        return currentPendingCount < _maxPendingItems &&
               currentQueueSizeBytes < _maxQueueSizeBytes;
    }

    /// <summary>
    /// Gets the remaining capacity in both items and bytes.
    /// </summary>
    public (int RemainingItems, long RemainingBytes) GetRemainingCapacity(
        int currentPendingCount, long currentQueueSizeBytes)
    {
        return (
            Math.Max(0, _maxPendingItems - currentPendingCount),
            Math.Max(0, _maxQueueSizeBytes - currentQueueSizeBytes)
        );
    }

    /// <summary>
    /// Determines which items should be purged from the outbox based on age and failure state.
    /// </summary>
    /// <param name="items">All outbox items with their metadata.</param>
    /// <returns>List of item IDs that should be purged.</returns>
    public List<string> IdentifyPurgeCandidates(IEnumerable<OutboxItemMetadata> items)
    {
        var now = DateTime.UtcNow;
        var candidates = new List<string>();

        foreach (var item in items)
        {
            // Purge items that have been acknowledged (fully delivered)
            if (item.State == "Acknowledged" && (now - item.AcknowledgedAtUtc) > TimeSpan.FromHours(24))
            {
                candidates.Add(item.ItemId);
                continue;
            }

            // Purge failed items past stale threshold
            if (item.State == "Failed" && (now - item.LastAttemptUtc) > _staleThreshold)
            {
                candidates.Add(item.ItemId);
                continue;
            }

            // Purge items exceeding max retry count
            if (item.RetryCount > 20 && item.State == "Failed")
            {
                candidates.Add(item.ItemId);
            }
        }

        return candidates;
    }

    /// <summary>
    /// Builds a quota health report.
    /// </summary>
    public OutboxQuotaReport BuildReport(int pendingCount, long queueSizeBytes, int failedCount, int acknowledgedCount)
    {
        var usagePercent = _maxQueueSizeBytes > 0
            ? (double)queueSizeBytes / _maxQueueSizeBytes * 100
            : 0;

        return new OutboxQuotaReport
        {
            PendingCount = pendingCount,
            FailedCount = failedCount,
            AcknowledgedCount = acknowledgedCount,
            QueueSizeBytes = queueSizeBytes,
            MaxQueueSizeBytes = _maxQueueSizeBytes,
            MaxPendingItems = _maxPendingItems,
            UsagePercent = usagePercent,
            Healthy = usagePercent < 80 && pendingCount < _maxPendingItems * 0.8,
            StaleThreshold = _staleThreshold
        };
    }
}

/// <summary>
/// Metadata for an outbox item used in maintenance decisions.
/// </summary>
public sealed class OutboxItemMetadata
{
    public string ItemId { get; set; } = string.Empty;
    public string State { get; set; } = "Pending";
    public int RetryCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastAttemptUtc { get; set; }
    public DateTime AcknowledgedAtUtc { get; set; }
    public long SizeBytes { get; set; }
}

/// <summary>
/// Report on outbox queue quota and health.
/// </summary>
public sealed class OutboxQuotaReport
{
    public int PendingCount { get; set; }
    public int FailedCount { get; set; }
    public int AcknowledgedCount { get; set; }
    public long QueueSizeBytes { get; set; }
    public long MaxQueueSizeBytes { get; set; }
    public int MaxPendingItems { get; set; }
    public double UsagePercent { get; set; }
    public bool Healthy { get; set; }
    public TimeSpan StaleThreshold { get; set; }
}
