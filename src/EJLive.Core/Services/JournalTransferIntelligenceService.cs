using System.Collections.Concurrent;
using EJLive.Core.Engine;
using EJLive.Core.Models;

namespace EJLive.Core.Services;

/// <summary>
/// Consolidates journal transfer progress events into durable sync records and
/// exposes simple health queries (stalled transfers) for dashboard alerting.
/// </summary>
public sealed class JournalTransferIntelligenceService
{
    private readonly ConcurrentDictionary<string, JournalSyncRecord> _records = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<JournalSyncRecord> Snapshot => _records.Values
        .OrderByDescending(item => item.UpdatedAtUtc)
        .ToArray();

    public JournalSyncRecord Upsert(JournalTransferProgressPacket progress)
    {
        ArgumentNullException.ThrowIfNull(progress);

        var transferId = string.IsNullOrWhiteSpace(progress.TransferId)
            ? BuildFallbackTransferId(progress.ATM_ID, progress.FileName)
            : progress.TransferId.Trim();
        var now = DateTime.UtcNow;
        var fileName = string.IsNullOrWhiteSpace(progress.FileName) ? "journal.bin" : progress.FileName.Trim();
        var state = progress.State;
        var progressPercent = Math.Max(0, Math.Min(100, progress.ProgressPercent));
        var expectedBytes = Math.Max(progress.ExpectedBytes, progress.ReceivedBytes);

        var record = _records.AddOrUpdate(
            transferId,
            _ => BuildNewRecord(progress, transferId, fileName, expectedBytes, progressPercent, state, now),
            (_, existing) =>
            {
                existing.ATM_ID = string.IsNullOrWhiteSpace(progress.ATM_ID) ? existing.ATM_ID : progress.ATM_ID.Trim();
                existing.FileName = fileName;
                existing.FileSize = expectedBytes > 0 ? expectedBytes : existing.FileSize;
                existing.FileOffset = Math.Max(existing.FileOffset, progress.ReceivedBytes);
                existing.ProgressPercent = Math.Max(existing.ProgressPercent, progressPercent);
                existing.State = state;
                existing.Message = string.IsNullOrWhiteSpace(progress.Message) ? existing.Message : progress.Message;
                existing.Checksum = string.IsNullOrWhiteSpace(progress.Checksum) ? existing.Checksum : progress.Checksum;
                existing.SHA256Hash = string.IsNullOrWhiteSpace(progress.Sha256) ? existing.SHA256Hash : progress.Sha256;
                existing.UpdatedAtUtc = now;
                if (state == JournalSyncState.Completed || state == JournalSyncState.Failed)
                    existing.CompletedAtUtc = now;
                return existing;
            });

        return record;
    }

    public IReadOnlyList<JournalSyncRecord> DetectStalledTransfers(TimeSpan staleFor)
    {
        var threshold = DateTime.UtcNow - staleFor;
        return Snapshot
            .Where(record =>
                record.State is JournalSyncState.Pending or JournalSyncState.Syncing or JournalSyncState.ReSyncing &&
                record.UpdatedAtUtc < threshold)
            .ToArray();
    }

    private static JournalSyncRecord BuildNewRecord(
        JournalTransferProgressPacket progress,
        string transferId,
        string fileName,
        long expectedBytes,
        int progressPercent,
        JournalSyncState state,
        DateTime now)
    {
        return new JournalSyncRecord
        {
            SyncId = transferId,
            ATM_ID = string.IsNullOrWhiteSpace(progress.ATM_ID) ? "UNKNOWN" : progress.ATM_ID.Trim(),
            FileName = fileName,
            FileSize = expectedBytes,
            FileOffset = Math.Max(0, progress.ReceivedBytes),
            Checksum = progress.Checksum ?? string.Empty,
            SHA256Hash = progress.Sha256 ?? string.Empty,
            State = state,
            ProgressPercent = progressPercent,
            Message = progress.Message ?? string.Empty,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CompletedAtUtc = state is JournalSyncState.Completed or JournalSyncState.Failed ? now : null
        };
    }

    private static string BuildFallbackTransferId(string atmId, string fileName)
    {
        var safeAtm = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        var safeFile = string.IsNullOrWhiteSpace(fileName) ? "journal.bin" : fileName.Trim();
        return $"{safeAtm}:{safeFile}";
    }
}
