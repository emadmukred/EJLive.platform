using System.Collections.Concurrent;

namespace EJLive.Client.WinForms.Services;

public enum FileDeliveryStatus
{
    Queued,
    Sent,
    Confirmed,
    Failed
}

public sealed record FileDeliveryReceipt(
    string DeliveryId,
    string FileName,
    string Checksum,
    long SizeBytes,
    FileDeliveryStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string Detail);

/// <summary>
/// Tracks file delivery from queue -> sent -> acknowledged states.
/// </summary>
public sealed class FileDeliveryConfirmationTracker
{
    private readonly ConcurrentDictionary<string, FileDeliveryReceipt> _receipts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _latestByFile = new(StringComparer.OrdinalIgnoreCase);

    public int PendingCount => _receipts.Values.Count(item =>
        item.Status is FileDeliveryStatus.Queued or FileDeliveryStatus.Sent);

    public FileDeliveryReceipt RegisterQueued(string fileName, string checksum, long sizeBytes)
    {
        var normalizedName = NormalizeFileName(fileName);
        var deliveryId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        var receipt = new FileDeliveryReceipt(
            deliveryId,
            normalizedName,
            checksum ?? string.Empty,
            Math.Max(0, sizeBytes),
            FileDeliveryStatus.Queued,
            now,
            now,
            "Queued");

        _receipts[deliveryId] = receipt;
        _latestByFile[normalizedName] = deliveryId;
        return receipt;
    }

    public bool TryMarkSent(string fileName, out FileDeliveryReceipt receipt)
    {
        receipt = default!;
        if (!TryResolveLatest(fileName, out var existing))
            return false;

        var updated = existing with
        {
            Status = FileDeliveryStatus.Sent,
            UpdatedAtUtc = DateTime.UtcNow,
            Detail = "Sent to server"
        };
        _receipts[existing.DeliveryId] = updated;
        receipt = updated;
        return true;
    }

    public bool TryApplyAcknowledgement(string ackText, out FileDeliveryReceipt receipt)
    {
        receipt = default!;
        if (!TryParseAcknowledgement(ackText, out var fileName, out var ok, out var detail))
            return false;

        if (!TryResolveLatest(fileName, out var existing))
        {
            var now = DateTime.UtcNow;
            var status = ok ? FileDeliveryStatus.Confirmed : FileDeliveryStatus.Failed;
            var created = new FileDeliveryReceipt(
                Guid.NewGuid().ToString("N"),
                NormalizeFileName(fileName),
                string.Empty,
                0,
                status,
                now,
                now,
                detail);
            _receipts[created.DeliveryId] = created;
            _latestByFile[created.FileName] = created.DeliveryId;
            receipt = created;
            return true;
        }

        var updated = existing with
        {
            Status = ok ? FileDeliveryStatus.Confirmed : FileDeliveryStatus.Failed,
            UpdatedAtUtc = DateTime.UtcNow,
            Detail = detail
        };
        _receipts[existing.DeliveryId] = updated;
        receipt = updated;
        return true;
    }

    public IReadOnlyList<FileDeliveryReceipt> Snapshot(int maxItems = 200)
    {
        return _receipts.Values
            .OrderByDescending(item => item.UpdatedAtUtc)
            .Take(Math.Max(1, maxItems))
            .ToArray();
    }

    private bool TryResolveLatest(string fileName, out FileDeliveryReceipt receipt)
    {
        receipt = default!;
        var normalizedName = NormalizeFileName(fileName);
        if (!_latestByFile.TryGetValue(normalizedName, out var deliveryId))
            return false;
        return _receipts.TryGetValue(deliveryId, out receipt!);
    }

    private static bool TryParseAcknowledgement(string text, out string fileName, out bool success, out string detail)
    {
        fileName = string.Empty;
        success = false;
        detail = string.Empty;

        var parts = (text ?? string.Empty).Split('|', StringSplitOptions.None);
        if (parts.Length < 2)
            return false;

        fileName = NormalizeFileName(parts[0]);
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        success = string.Equals(parts[1], "OK", StringComparison.OrdinalIgnoreCase);
        detail = parts.Length > 2 ? string.Join("|", parts.Skip(2)) : (success ? "Journal acknowledged" : "Journal rejected");
        return true;
    }

    private static string NormalizeFileName(string fileName)
    {
        return Path.GetFileName(string.IsNullOrWhiteSpace(fileName) ? "unknown.bin" : fileName.Trim());
    }
}
