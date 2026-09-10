using System.Collections.Concurrent;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Core.Engine;

public sealed class JournalOutboxItem
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString("N");
    public string ATM_ID { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public long Offset { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public long SizeBytes { get; set; }
    public string PayloadPath { get; set; } = string.Empty;
    public bool AwaitingAcknowledgement { get; set; }
    public DateTime? LastSentAtUtc { get; set; }
    public DateTime? AckDeadlineUtc { get; set; }
    public string LastAckDetail { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime NextAttemptUtc { get; set; } = DateTime.UtcNow;
    public SyncStatus Status { get; set; } = SyncStatus.Pending;

    public bool IsReadyToSend =>
        !AwaitingAcknowledgement &&
        (Status == SyncStatus.Pending || Status == SyncStatus.Failed || Status == SyncStatus.Resyncing) &&
        DateTime.UtcNow >= NextAttemptUtc;
}

public sealed class JournalOutbox
{
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly ConcurrentDictionary<string, JournalOutboxItem> _items = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _payloadRoot;
    private readonly object _hydrateLock = new();
    private bool _hydrated;

    public int Count => _items.Count;
    public IReadOnlyCollection<JournalOutboxItem> Snapshot => _items.Values.OrderBy(i => i.CreatedAtUtc).ToArray();

    public JournalOutbox()
    {
        _payloadRoot = Path.Combine(AppConstants.DefaultClientOutboxPath, "payload");
        Directory.CreateDirectory(_payloadRoot);
        HydrateFromDatabase();
    }

    public JournalOutboxItem Enqueue(string atmId, string fileName, byte[] data, long offset, string checksum)
    {
        EnsureHydrated();
        var payload = data ?? Array.Empty<byte>();
        var item = new JournalOutboxItem
        {
            ATM_ID = atmId,
            FileName = fileName,
            Data = payload,
            Offset = offset,
            Checksum = checksum ?? string.Empty,
            SizeBytes = payload.LongLength,
            Status = SyncStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            NextAttemptUtc = DateTime.UtcNow
        };
        Enqueue(item);
        return item;
    }

    public void Enqueue(JournalOutboxItem item)
    {
        EnsureHydrated();
        if (item == null)
            return;

        if (string.IsNullOrWhiteSpace(item.ItemId))
            item.ItemId = Guid.NewGuid().ToString("N");

        item.ATM_ID = string.IsNullOrWhiteSpace(item.ATM_ID) ? "UNKNOWN" : item.ATM_ID.Trim();
        item.FileName = NormalizeFileName(item.FileName);
        item.Checksum = item.Checksum ?? string.Empty;
        item.CreatedAtUtc = item.CreatedAtUtc == DateTime.MinValue ? DateTime.UtcNow : item.CreatedAtUtc.ToUniversalTime();
        item.NextAttemptUtc = item.NextAttemptUtc == DateTime.MinValue ? DateTime.UtcNow : item.NextAttemptUtc.ToUniversalTime();
        item.UpdatedAtUtc = DateTime.UtcNow;
        item.SizeBytes = item.SizeBytes > 0 ? item.SizeBytes : (item.Data?.LongLength ?? 0);

        if (string.IsNullOrWhiteSpace(item.PayloadPath))
            item.PayloadPath = SavePayloadBytes(item.ItemId, item.Data ?? Array.Empty<byte>());
        else
            item.PayloadPath = Path.GetFullPath(item.PayloadPath);

        Persist(item);
        _items[item.ItemId] = item;
        if (!item.AwaitingAcknowledgement)
            _queue.Enqueue(item.ItemId);
    }

    public bool TryDequeue(out JournalOutboxItem item)
    {
        EnsureHydrated();
        while (_queue.TryDequeue(out var itemId))
        {
            if (!_items.TryGetValue(itemId, out var candidate))
                continue;
            if (candidate.AwaitingAcknowledgement)
                continue;

            if (!candidate.IsReadyToSend)
            {
                _queue.Enqueue(candidate.ItemId);
                continue;
            }

            if ((candidate.Data == null || candidate.Data.Length == 0) &&
                !string.IsNullOrWhiteSpace(candidate.PayloadPath))
            {
                candidate.Data = SecurityHelper.ReadFileSafe(candidate.PayloadPath) ?? Array.Empty<byte>();
            }

            if (candidate.Data == null || candidate.Data.Length == 0)
            {
                RetryItem(candidate.ItemId, 2000, "payload-missing");
                continue;
            }

            candidate.SizeBytes = candidate.Data.LongLength;
            candidate.Status = SyncStatus.InProgress;
            candidate.UpdatedAtUtc = DateTime.UtcNow;
            Persist(candidate);
            item = candidate;
            return true;
        }

        item = new JournalOutboxItem();
        return false;
    }

    public void MarkAwaitingAcknowledgement(string itemId, TimeSpan ackTimeout)
    {
        EnsureHydrated();
        if (!_items.TryGetValue(itemId, out var item))
            return;

        var now = DateTime.UtcNow;
        item.AwaitingAcknowledgement = true;
        item.Status = SyncStatus.Syncing;
        item.LastSentAtUtc = now;
        item.AckDeadlineUtc = now.Add(ackTimeout <= TimeSpan.Zero ? TimeSpan.FromMinutes(2) : ackTimeout);
        item.UpdatedAtUtc = now;
        Persist(item);
    }

    public int RequeueTimedOutAcknowledgements(RetryPolicy retryPolicy, int maxItems = 50)
    {
        EnsureHydrated();
        var now = DateTime.UtcNow;
        var requeued = 0;
        foreach (var timedOut in _items.Values
                     .Where(v => v.AwaitingAcknowledgement && v.AckDeadlineUtc.HasValue && v.AckDeadlineUtc.Value <= now)
                     .OrderBy(v => v.AckDeadlineUtc)
                     .Take(Math.Max(1, maxItems))
                     .ToArray())
        {
            RetryItem(timedOut.ItemId, retryPolicy.ComputeDelay(timedOut.RetryCount + 1), "ack-timeout");
            requeued++;
        }

        return requeued;
    }

    public bool TryApplyAcknowledgement(string ackText, RetryPolicy retryPolicy, out JournalOutboxItem item, out bool success, out string detail)
    {
        EnsureHydrated();
        item = new JournalOutboxItem();
        success = false;
        detail = string.Empty;

        if (!TryParseAcknowledgement(ackText, out var fileName, out success, out detail))
            return false;

        var candidate = _items.Values
            .Where(v => string.Equals(v.FileName, fileName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(v => v.AwaitingAcknowledgement)
            .ThenByDescending(v => v.LastSentAtUtc ?? v.CreatedAtUtc)
            .FirstOrDefault();
        if (candidate == null)
            return false;

        item = candidate;
        candidate.LastAckDetail = detail;
        candidate.UpdatedAtUtc = DateTime.UtcNow;
        if (success)
        {
            MarkCompleted(candidate.ItemId, detail);
            return true;
        }

        RetryItem(candidate.ItemId, retryPolicy.ComputeDelay(candidate.RetryCount + 1), detail);
        return true;
    }

    public void MarkCompleted(string itemId) => MarkCompleted(itemId, "ack-ok");

    public void MarkCompleted(string itemId, string detail)
    {
        EnsureHydrated();
        if (_items.TryRemove(itemId, out var item))
        {
            item.Status = SyncStatus.Completed;
            item.AwaitingAcknowledgement = false;
            item.LastAckDetail = detail ?? string.Empty;
            item.UpdatedAtUtc = DateTime.UtcNow;
            SafeDeletePayload(item.PayloadPath);
            DatabaseManager.Instance.DeleteClientOutboxItem(itemId);
        }
    }

    public void RetryItem(string itemId, int delayMs) => RetryItem(itemId, delayMs, "retry");

    public void RetryItem(string itemId, int delayMs, string detail)
    {
        EnsureHydrated();
        if (!_items.TryGetValue(itemId, out var item))
            return;

        item.RetryCount++;
        item.Status = SyncStatus.Failed;
        item.AwaitingAcknowledgement = false;
        item.LastAckDetail = detail ?? string.Empty;
        item.NextAttemptUtc = DateTime.UtcNow.AddMilliseconds(Math.Max(0, delayMs));
        item.AckDeadlineUtc = null;
        item.UpdatedAtUtc = DateTime.UtcNow;
        Persist(item);
        _queue.Enqueue(item.ItemId);
    }

    public void ClearFailed()
    {
        EnsureHydrated();
        foreach (var item in _items.Values.Where(i => i.Status == SyncStatus.Failed).ToArray())
        {
            if (_items.TryRemove(item.ItemId, out var removed))
            {
                SafeDeletePayload(removed.PayloadPath);
                DatabaseManager.Instance.DeleteClientOutboxItem(removed.ItemId);
            }
        }
    }

    private void EnsureHydrated()
    {
        if (_hydrated)
            return;
        HydrateFromDatabase();
    }

    private void HydrateFromDatabase()
    {
        lock (_hydrateLock)
        {
            if (_hydrated)
                return;

            try
            {
                var rows = DatabaseManager.Instance.GetClientOutboxItems(maxRows: AppConstants.OutboxMaxItems);
                foreach (var row in rows)
                {
                    var status = NormalizeStatus(row.Status);
                    var awaitingAck = string.Equals(row.Status, "awaiting_ack", StringComparison.OrdinalIgnoreCase);
                    var item = new JournalOutboxItem
                    {
                        ItemId = row.ItemId,
                        ATM_ID = string.IsNullOrWhiteSpace(row.ATM_ID) ? "UNKNOWN" : row.ATM_ID.Trim(),
                        FileName = NormalizeFileName(row.FileName),
                        PayloadPath = row.PayloadPath,
                        SizeBytes = Math.Max(0, row.PayloadSize),
                        Offset = Math.Max(0, row.FileOffset),
                        Checksum = row.Checksum ?? string.Empty,
                        RetryCount = Math.Max(0, row.RetryCount),
                        Status = status,
                        AwaitingAcknowledgement = awaitingAck,
                        NextAttemptUtc = row.NextAttemptUtc,
                        LastSentAtUtc = row.LastSentUtc,
                        AckDeadlineUtc = row.AckDeadlineUtc,
                        LastAckDetail = row.LastAckDetail ?? string.Empty,
                        CreatedAtUtc = row.CreatedAtUtc,
                        UpdatedAtUtc = row.UpdatedAtUtc
                    };

                    if (item.AwaitingAcknowledgement && item.AckDeadlineUtc.HasValue && item.AckDeadlineUtc.Value <= DateTime.UtcNow)
                    {
                        item.AwaitingAcknowledgement = false;
                        item.Status = SyncStatus.Failed;
                        item.LastAckDetail = string.IsNullOrWhiteSpace(item.LastAckDetail) ? "ack-timeout-recovered" : item.LastAckDetail;
                        item.NextAttemptUtc = DateTime.UtcNow;
                        item.AckDeadlineUtc = null;
                    }

                    _items[item.ItemId] = item;
                    if (!item.AwaitingAcknowledgement)
                        _queue.Enqueue(item.ItemId);

                    Persist(item);
                }
            }
            catch
            {
                // Durability hydration must never break runtime startup.
            }

            _hydrated = true;
        }
    }

    private void Persist(JournalOutboxItem item)
    {
        DatabaseManager.Instance.UpsertClientOutboxItem(
            itemId: item.ItemId,
            atmId: item.ATM_ID,
            fileName: item.FileName,
            payloadPath: item.PayloadPath,
            payloadSize: item.SizeBytes,
            fileOffset: item.Offset,
            checksum: item.Checksum,
            retryCount: item.RetryCount,
            status: item.AwaitingAcknowledgement ? "awaiting_ack" : item.Status.ToString().ToLowerInvariant(),
            nextAttemptUtc: item.NextAttemptUtc,
            createdAtUtc: item.CreatedAtUtc,
            updatedAtUtc: item.UpdatedAtUtc,
            lastSentUtc: item.LastSentAtUtc,
            ackDeadlineUtc: item.AckDeadlineUtc,
            lastAckDetail: item.LastAckDetail);
    }

    private string SavePayloadBytes(string itemId, byte[] payload)
    {
        Directory.CreateDirectory(_payloadRoot);
        var path = Path.Combine(_payloadRoot, $"{itemId}.bin");
        File.WriteAllBytes(path, payload ?? Array.Empty<byte>());
        return path;
    }

    private static void SafeDeletePayload(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
        }
    }

    private static SyncStatus NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return SyncStatus.Pending;

        return status.Trim().ToLowerInvariant() switch
        {
            "failed" => SyncStatus.Failed,
            "syncing" => SyncStatus.Syncing,
            "inprogress" => SyncStatus.InProgress,
            "completed" => SyncStatus.Completed,
            "resyncing" => SyncStatus.Resyncing,
            _ => SyncStatus.Pending
        };
    }

    private static string NormalizeFileName(string fileName)
    {
        var safe = Path.GetFileName(string.IsNullOrWhiteSpace(fileName) ? "unknown.bin" : fileName.Trim());
        return string.IsNullOrWhiteSpace(safe) ? "unknown.bin" : safe;
    }

    private static bool TryParseAcknowledgement(string text, out string fileName, out bool ok, out string detail)
    {
        fileName = string.Empty;
        ok = false;
        detail = string.Empty;

        var parts = (text ?? string.Empty).Split('|', StringSplitOptions.None);
        if (parts.Length < 2)
            return false;

        fileName = NormalizeFileName(parts[0]);
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        ok = string.Equals(parts[1], "OK", StringComparison.OrdinalIgnoreCase);
        detail = parts.Length > 2
            ? string.Join("|", parts.Skip(2))
            : (ok ? "ack-ok" : "ack-fail");
        return true;
    }
}
