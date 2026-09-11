// JournalSyncHub.cs
// Processed by Code Intelligence Scanner — Smart Merge & Restructure

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Core.Services
{
    /// <summary>
    /// Unified JournalSync Hub — aggregates tracking, state management,
    /// monitoring, alerting, and dashboard metrics into a single facade.
    ///
    /// This hub wraps the existing individual services rather than replacing them,
    /// preserving backward compatibility while providing a simplified entry point.
    ///
    /// Existing individual classes (JournalSyncTracker, JournalSyncStateService,
    /// JournalSyncMonitorService, JournalSyncAlertService, etc.) remain available
    /// for direct use where fine-grained control is needed.
    /// </summary>
    public sealed class JournalSyncHub : IDisposable
    {
        private readonly JournalSyncTracker _tracker;
        private readonly JournalOutbox _outbox;
        private readonly ConcurrentDictionary<string, LiveSyncProgress> _activeTransfers = new(StringComparer.OrdinalIgnoreCase);
        private CancellationTokenSource? _cts;
        private Task? _syncLoop;
        private readonly object _lock = new();
        private volatile bool _disposed;

        // ──────────────── Events ────────────────
        public event EventHandler<LiveSyncProgress>? ProgressChanged;
        public event EventHandler<JournalSyncRecord>? StateChanged;
        public event EventHandler<string>? OnSyncError;
        public event EventHandler<string>? OnLog;

        // ──────────────── Properties ────────────────
        public bool IsRunning { get; private set; }
        public int ActiveTransferCount => _activeTransfers.Count;
        public long TotalBytesProcessed { get; private set; }
        public long TotalFilesProcessed { get; private set; }
        public JournalOutbox Outbox => _outbox;
        public JournalSyncTracker Tracker => _tracker;

        public JournalSyncHub()
        {
            _tracker = new JournalSyncTracker();
            _outbox = new JournalOutbox();
            _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
        }

    // ──────────────── Lifecycle ────────────────

    public void Start(TimeSpan? syncInterval = null)
    {
        lock (_lock)
        {
            if (IsRunning) return;

            IsRunning = true;
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var interval = syncInterval ?? TimeSpan.FromMilliseconds(500);
            _syncLoop = Task.Run(() => ProcessSyncLoopAsync(_cts.Token, interval), _cts.Token);
            Log("JournalSyncHub started.");
        }
}

public void Stop()
{
    lock (_lock)
    {
        if (!IsRunning) return;

        IsRunning = false;
        try { _cts?.Cancel(); } catch { }

        try { _syncLoop?.Wait(TimeSpan.FromSeconds(5)); } catch { }

        Log("JournalSyncHub stopped.");
    }
}

// ──────────────── Queue Operations ────────────────

public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum)
{
    _outbox.Enqueue(atmId, fileName, data, offset, checksum);
    _tracker.AddOrGet(atmId, fileName, data.Length, offset, checksum);
}

public void ReportProgress(LiveSyncProgress progress)
{
    if (progress is null) return;
    _activeTransfers[progress.ATMId ?? "unknown"] = progress;
    ProgressChanged?.Invoke(this, progress);
}

public IReadOnlyDictionary<string, LiveSyncProgress> GetActiveTransfers()
=> new ConcurrentDictionary<string, LiveSyncProgress>(_activeTransfers);

// ──────────────── Tracking (delegated to JournalSyncTracker) ────────────────

public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
=> _tracker.AddOrGet(atmId, fileName, fileSize, offset, checksum);

public void MarkSyncing(string atmId, string syncId, int chunkPercent)
=> _tracker.MarkSyncing(atmId, syncId, chunkPercent);

public void MarkCompleted(string atmId, string syncId, string? sha256 = null)
=> _tracker.MarkCompleted(atmId, syncId, sha256);

public void MarkFailed(string atmId, string syncId, string reason)
=> _tracker.MarkFailed(atmId, syncId, reason);

public void MarkReSyncing(string atmId, string syncId)
=> _tracker.MarkReSyncing(atmId, syncId);

public void MarkArchived(string syncId)
=> _tracker.MarkArchived(syncId);

public List<JournalSyncRecord> GetAllForATM(string atmId)
=> _tracker.GetAllForATM(atmId);

public List<JournalSyncRecord> GetPendingForATM(string atmId)
=> _tracker.GetPendingForATM(atmId);

public List<JournalSyncRecord> GetFailedForATM(string atmId)
=> _tracker.GetFailedForATM(atmId);

public int GetPendingCount(string atmId)
=> _tracker.GetPendingCount(atmId);

public (int completed, int failed, int pending) GetStats(string atmId)
=> _tracker.GetStats(atmId);

public void LoadFromDatabase(string atmId)
=> _tracker.LoadFromDatabase(atmId);

public void ResetInFlightToResync(string atmId)
=> _tracker.ResetInFlightToResync(atmId);

// ──────────────── Outbox Operations ────────────────

public int RequeueTimedOut() => _outbox.RequeueTimedOutAcknowledgements(
RetryPolicy.ForNetwork("LAN"), 50);

// ──────────────── Sync Summary ────────────────

public SyncSummary BuildSummary(string? atmId = null)
{
    var allRecords = atmId != null;
    ? _tracker.GetAllForATM(atmId)
    : _tracker.GetAllForATM("*");

    return new SyncSummary
    {
        Total = allRecords.Count,
        Pending = allRecords.Count(r => r.State == JournalSyncState.Pending),
        InProgress = allRecords.Count(r => r.State is JournalSyncState.Syncing or JournalSyncState.ReSyncing),
        Completed = allRecords.Count(r => r.State == JournalSyncState.Completed),
        Failed = allRecords.Count(r => r.State == JournalSyncState.Failed),
        AverageProgress = allRecords.Count == 0 ? 0
        : (int)Math.Round(allRecords.Average(r => r.ProgressPercent)),
        TotalBytesProcessed = TotalBytesProcessed,
        TotalFilesProcessed = TotalFilesProcessed,
        ActiveTransfers = ActiveTransferCount
    };
}

// ──────────────── Sync Loop ────────────────

private async Task ProcessSyncLoopAsync(CancellationToken cancellationToken, TimeSpan interval)
{
    const int batchSize = 5;
    const int errorDelayMs = 2000;

    while (!cancellationToken.IsCancellationRequested && IsRunning)
    {
        try
        {
            var processed = 0;
            for (var i = 0; i < batchSize && !cancellationToken.IsCancellationRequested; i++)
            {
                if (!_outbox.TryDequeue(out var item))
                break;

                processed++;
                await ProcessOutboxItemAsync(item, cancellationToken).ConfigureAwait(false);
            }

        var requeued = RequeueTimedOut();
        if (requeued > 0)
        Log($"Requeued {requeued} timed-out items.");

        if (processed == 0)
        await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        else
        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
    }
catch (OperationCanceledException) { break; }
catch (Exception ex)
{
    Log($"Sync loop error: {ex.Message}");
    OnSyncError?.Invoke(this, ex.Message);
    try { await Task.Delay(errorDelayMs, cancellationToken).ConfigureAwait(false); }
    catch { break; }
}
}
}

private async Task ProcessOutboxItemAsync(JournalOutboxItem item, CancellationToken cancellationToken)
{
    var progress = new LiveSyncProgress;
    {
        ATMId = item.ATM_ID,
        FileName = item.FileName,
        TotalBytes = item.SizeBytes,
        StateLabel = "Syncing",
        StateIcon = "⟳",
        UpdatedAt = DateTime.UtcNow
    };

try
{
    _outbox.MarkAwaitingAcknowledgement(item.ItemId, TimeSpan.FromMinutes(2));

    progress.StateLabel = "Sending";
    progress.Percent = 0;
    ReportProgress(progress);

    var chunks = Math.Max(1, (int)Math.Ceiling(item.SizeBytes / (double)AppConstants.ChunkSizeBytes));
    for (var seq = 0; seq < chunks; seq++)
    {
        if (cancellationToken.IsCancellationRequested) break;

        progress.BytesSent = Math.Min((seq + 1) * (long)AppConstants.ChunkSizeBytes, item.SizeBytes);
        progress.Percent = (int)(progress.BytesSent * 100 / Math.Max(1, item.SizeBytes));
        progress.SeqNum = seq;
        progress.TotalChunks = chunks;
        progress.UpdatedAt = DateTime.UtcNow;
        ReportProgress(progress);

        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
    }

progress.StateLabel = "Completed";
progress.StateIcon = "✓";
progress.Percent = 100;
progress.BytesSent = item.SizeBytes;
progress.UpdatedAt = DateTime.UtcNow;
_outbox.MarkCompleted(item.ItemId, "sync-ok");
_tracker.MarkCompleted(item.ATM_ID, $"hub-{item.ItemId}", null);
ReportProgress(progress);

Interlocked.Add(ref TotalBytesProcessed, item.SizeBytes);
Interlocked.Increment(ref TotalFilesProcessed);
}
catch (OperationCanceledException)
{
    _outbox.RetryItem(item.ItemId, 5000, "cancelled");
    progress.StateLabel = "Cancelled";
    progress.StateIcon = "⊘";
    ReportProgress(progress);
}
catch (Exception ex)
{
    _outbox.RetryItem(item.ItemId, 5000, ex.Message);
    _tracker.MarkFailed(item.ATM_ID, $"hub-{item.ItemId}", ex.Message);
    progress.StateLabel = "Failed";
    progress.StateIcon = "✕";
    ReportProgress(progress);
    Log($"Sync failed for {item.FileName}: {ex.Message}");
    OnSyncError?.Invoke(this, $"Failed: {item.FileName} — {ex.Message}");
}
finally
{
    _activeTransfers.TryRemove(item.ATM_ID, out _);
}
}

private void Log(string message) => OnLog?.Invoke(this, $"[JournalSyncHub] {message}");

public void Dispose()
{
    if (_disposed) return;
    _disposed = true;
    Stop();
    _cts?.Dispose();
    _syncLoop?.Dispose();
}
}
public partial class JournalSyncHub : IDisposable
{
    private readonly JournalSyncTracker _tracker;
    private readonly JournalOutbox _outbox;
    private readonly ConcurrentDictionary<string, LiveSyncProgress> _activeTransfers = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _cts;
    private Task? _syncLoop;
    private readonly object _lock = new();
    private volatile bool _disposed;
    public bool IsRunning { get; private set; }
    public int ActiveTransferCount => _activeTransfers.Count;
    public long TotalBytesProcessed { get; private set; }
    public long TotalFilesProcessed { get; private set; }
    public JournalOutbox Outbox => _outbox;
    public JournalSyncTracker Tracker => _tracker;
    public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum)
    {
        _outbox.Enqueue(atmId, fileName, data, offset, checksum);
        _tracker.AddOrGet(atmId, fileName, data.Length, offset, checksum);
    }
public JournalSyncHub()
_tracker = new JournalSyncTracker();
public JournalSyncHub()
{
    _tracker = new JournalSyncTracker();
    _outbox = new JournalOutbox();
    _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
}
_outbox = new JournalOutbox();
_tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
public void Start(TimeSpan? syncInterval = null)
{
    lock (_lock)
    {
        if (IsRunning) return;
        IsRunning = true;
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var interval = syncInterval ?? TimeSpan.FromMilliseconds(500);
        _syncLoop = Task.Run(() => ProcessSyncLoopAsync(_cts.Token, interval), _cts.Token);
        Log("JournalSyncHub started.");
    }
}
public void Stop()
{
    lock (_lock)
    {
        if (!IsRunning) return;
        IsRunning = false;
        try { _cts?.Cancel(); } catch { }
        try { _syncLoop?.Wait(TimeSpan.FromSeconds(5)); } catch { }
        Log("JournalSyncHub stopped.");
    }
}
public void ReportProgress(LiveSyncProgress progress)
{
    if (progress is null) return;
    _activeTransfers[progress.ATMId ?? "unknown"] = progress;
    ProgressChanged?.Invoke(this, progress);
}
public IReadOnlyDictionary<string, LiveSyncProgress> GetActiveTransfers()
=> new ConcurrentDictionary<string, LiveSyncProgress>(_activeTransfers);
public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
=> _tracker.AddOrGet(atmId, fileName, fileSize, offset, checksum);
public void MarkSyncing(string atmId, string syncId, int chunkPercent)
=> _tracker.MarkSyncing(atmId, syncId, chunkPercent);
public void MarkCompleted(string atmId, string syncId, string? sha256 = null)
=> _tracker.MarkCompleted(atmId, syncId, sha256);
public void MarkFailed(string atmId, string syncId, string reason)
=> _tracker.MarkFailed(atmId, syncId, reason);
public void MarkReSyncing(string atmId, string syncId)
=> _tracker.MarkReSyncing(atmId, syncId);
public void MarkArchived(string syncId)
=> _tracker.MarkArchived(syncId);
public List<JournalSyncRecord> GetAllForATM(string atmId)
=> _tracker.GetAllForATM(atmId);
public List<JournalSyncRecord> GetPendingForATM(string atmId)
=> _tracker.GetPendingForATM(atmId);
public List<JournalSyncRecord> GetFailedForATM(string atmId)
=> _tracker.GetFailedForATM(atmId);
public int GetPendingCount(string atmId)
=> _tracker.GetPendingCount(atmId);
public (int completed, int failed, int pending) GetStats(string atmId)
=> _tracker.GetStats(atmId);
public void LoadFromDatabase(string atmId)
=> _tracker.LoadFromDatabase(atmId);
public void ResetInFlightToResync(string atmId)
=> _tracker.ResetInFlightToResync(atmId);
public int RequeueTimedOut() => _outbox.RequeueTimedOutAcknowledgements(
RetryPolicy.ForNetwork("LAN"), 50);
public SyncSummary BuildSummary(string? atmId = null)
{
    var allRecords = atmId != null;
    ? _tracker.GetAllForATM(atmId)
    : _tracker.GetAllForATM("*");
    return new SyncSummary
    {
        Total = allRecords.Count,
        Pending = allRecords.Count(r => r.State == JournalSyncState.Pending),
        InProgress = allRecords.Count(r => r.State is JournalSyncState.Syncing or JournalSyncState.ReSyncing),
        Completed = allRecords.Count(r => r.State == JournalSyncState.Completed),
        Failed = allRecords.Count(r => r.State == JournalSyncState.Failed),
        AverageProgress = allRecords.Count == 0 ? 0
        : (int)Math.Round(allRecords.Average(r => r.ProgressPercent)),
        TotalBytesProcessed = TotalBytesProcessed,
        TotalFilesProcessed = TotalFilesProcessed,
        ActiveTransfers = ActiveTransferCount
    };
}
private async Task ProcessSyncLoopAsync(CancellationToken cancellationToken, TimeSpan interval)
{
    const int batchSize = 5;
    const int errorDelayMs = 2000;
    while (!cancellationToken.IsCancellationRequested && IsRunning)
    {
        try
        {
            var processed = 0;
            for (var i = 0; i < batchSize && !cancellationToken.IsCancellationRequested; i++)
            {
                if (!_outbox.TryDequeue(out var item))
                break;
                processed++;
                await ProcessOutboxItemAsync(item, cancellationToken).ConfigureAwait(false);
            }
        var requeued = RequeueTimedOut();
        if (requeued > 0)
        Log($"Requeued {requeued} timed-out items.");
        if (processed == 0)
        await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        else
        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
    }
catch (OperationCanceledException) { break; }
catch (Exception ex)
{
    Log($"Sync loop error: {ex.Message}");
    OnSyncError?.Invoke(this, ex.Message);
    try { await Task.Delay(errorDelayMs, cancellationToken).ConfigureAwait(false); }
    catch { break; }
}
}
}
private async Task ProcessOutboxItemAsync(JournalOutboxItem item, CancellationToken cancellationToken)
{
    var progress = new LiveSyncProgress;
    {
        ATMId = item.ATM_ID,
        FileName = item.FileName,
        TotalBytes = item.SizeBytes,
        StateLabel = "Syncing",
        StateIcon = "⟳",
        UpdatedAt = DateTime.UtcNow
    };
try
{
    _outbox.MarkAwaitingAcknowledgement(item.ItemId, TimeSpan.FromMinutes(2));
    progress.StateLabel = "Sending";
    progress.Percent = 0;
    ReportProgress(progress);
    var chunks = Math.Max(1, (int)Math.Ceiling(item.SizeBytes / (double)AppConstants.ChunkSizeBytes));
    for (var seq = 0; seq < chunks; seq++)
    {
        if (cancellationToken.IsCancellationRequested) break;
        progress.BytesSent = Math.Min((seq + 1) * (long)AppConstants.ChunkSizeBytes, item.SizeBytes);
        progress.Percent = (int)(progress.BytesSent * 100 / Math.Max(1, item.SizeBytes));
        progress.SeqNum = seq;
        progress.TotalChunks = chunks;
        progress.UpdatedAt = DateTime.UtcNow;
        ReportProgress(progress);
        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
    }
progress.StateLabel = "Completed";
progress.StateIcon = "✓";
progress.Percent = 100;
progress.BytesSent = item.SizeBytes;
progress.UpdatedAt = DateTime.UtcNow;
_outbox.MarkCompleted(item.ItemId, "sync-ok");
_tracker.MarkCompleted(item.ATM_ID, $"hub-{item.ItemId}", null);
ReportProgress(progress);
Interlocked.Add(ref TotalBytesProcessed, item.SizeBytes);
Interlocked.Increment(ref TotalFilesProcessed);
}
catch (OperationCanceledException)
{
    _outbox.RetryItem(item.ItemId, 5000, "cancelled");
    progress.StateLabel = "Cancelled";
    progress.StateIcon = "⊘";
    ReportProgress(progress);
}
catch (Exception ex)
{
    _outbox.RetryItem(item.ItemId, 5000, ex.Message);
    _tracker.MarkFailed(item.ATM_ID, $"hub-{item.ItemId}", ex.Message);
    progress.StateLabel = "Failed";
    progress.StateIcon = "✕";
    ReportProgress(progress);
    Log($"Sync failed for {item.FileName}: {ex.Message}");
    OnSyncError?.Invoke(this, $"Failed: {item.FileName} — {ex.Message}");
}
finally
{
    _activeTransfers.TryRemove(item.ATM_ID, out _);
}
}
private void Log(string message) => OnLog?.Invoke(this, $"[JournalSyncHub] {message}");
public void Dispose()
{
    if (_disposed) return;
    _disposed = true;
    Stop();
    _cts?.Dispose();
    _syncLoop?.Dispose();
}
public event EventHandler<LiveSyncProgress>? ProgressChanged;
public event EventHandler<JournalSyncRecord>? StateChanged;
public event EventHandler<string>? OnSyncError;
public event EventHandler<string>? OnLog;
}
public partial public public sealed class JournalSyncHub : IDisposable
{
    private readonly JournalSyncTracker _tracker;
    private readonly JournalOutbox _outbox;
    private readonly ConcurrentDictionary<string, LiveSyncProgress> _activeTransfers = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();
    private volatile bool _disposed;
    public int ActiveTransferCount => _activeTransfers.Count;
    public JournalOutbox Outbox => _outbox;
    public JournalSyncTracker Tracker => _tracker;
    const int batchSize = 5;
    const int errorDelayMs = 2000;
    public JournalSyncHub()
    {
        public bool IsRunning { get; private set; }
        public long TotalBytesProcessed { get; private set; }
        public long TotalFilesProcessed { get; private set; }
        public void Start(TimeSpan? syncInterval = null)
        {
            public void Stop()
            {
                public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum)
                {
                    public void ReportProgress(LiveSyncProgress progress)
                    {
                        public IReadOnlyDictionary<string, LiveSyncProgress> GetActiveTransfers()
                        =>
                        public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
                        =>
                        public void MarkSyncing(string atmId, string syncId, int chunkPercent)
                        =>
                        public void MarkCompleted(string atmId, string syncId, string? sha256 = null)
                        =>
                        public void MarkFailed(string atmId, string syncId, string reason)
                        =>
                        public void MarkReSyncing(string atmId, string syncId)
                        =>
                        public void MarkArchived(string syncId)
                        =>
                        public List<JournalSyncRecord> GetAllForATM(string atmId)
                        =>
                        public List<JournalSyncRecord> GetPendingForATM(string atmId)
                        =>
                        public List<JournalSyncRecord> GetFailedForATM(string atmId)
                        =>
                        public int GetPendingCount(string atmId)
                        =>
                        public void LoadFromDatabase(string atmId)
                        =>
                        public void ResetInFlightToResync(string atmId)
                        =>
                        public int RequeueTimedOut() =>
                        public SyncSummary BuildSummary(string? atmId = null)
                        {
                            private async Task ProcessSyncLoopAsync(CancellationToken cancellationToken, TimeSpan interval)
                            {
                                private async Task ProcessOutboxItemAsync(JournalOutboxItem item, CancellationToken cancellationToken)
                                {
                                    private void Log(string message) =>
                                    public void Dispose()
                                    {
                                    }

                            }
                        public partial public sealed class JournalSyncHub : IDisposable
                        {
                            private readonly JournalSyncTracker _tracker;
                            private readonly JournalOutbox _outbox;
                            private readonly ConcurrentDictionary<string, LiveSyncProgress> _activeTransfers = new(StringComparer.OrdinalIgnoreCase);
                            private readonly object _lock = new();
                            private volatile bool _disposed;
                            public int ActiveTransferCount => _activeTransfers.Count;
                            public JournalOutbox Outbox => _outbox;
                            public JournalSyncTracker Tracker => _tracker;
                            const int batchSize = 5;
                            const int errorDelayMs = 2000;
                            public JournalSyncHub()
                            {
                                public bool IsRunning { get; private set; }
                                public long TotalBytesProcessed { get; private set; }
                                public long TotalFilesProcessed { get; private set; }
                                public void Start(TimeSpan? syncInterval = null)
                                {
                                    public void Stop()
                                    {
                                        public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum)
                                        {
                                            public void ReportProgress(LiveSyncProgress progress)
                                            {
                                                public IReadOnlyDictionary<string, LiveSyncProgress> GetActiveTransfers()
                                                =>
                                                public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
                                                =>
                                                public void MarkSyncing(string atmId, string syncId, int chunkPercent)
                                                =>
                                                public void MarkCompleted(string atmId, string syncId, string? sha256 = null)
                                                =>
                                                public void MarkFailed(string atmId, string syncId, string reason)
                                                =>
                                                public void MarkReSyncing(string atmId, string syncId)
                                                =>
                                                public void MarkArchived(string syncId)
                                                =>
                                                public List<JournalSyncRecord> GetAllForATM(string atmId)
                                                =>
                                                public List<JournalSyncRecord> GetPendingForATM(string atmId)
                                                =>
                                                public List<JournalSyncRecord> GetFailedForATM(string atmId)
                                                =>
                                                public int GetPendingCount(string atmId)
                                                =>
                                                public void LoadFromDatabase(string atmId)
                                                =>
                                                public void ResetInFlightToResync(string atmId)
                                                =>
                                                public int RequeueTimedOut() =>
                                                public SyncSummary BuildSummary(string? atmId = null)
                                                {
                                                    private async Task ProcessSyncLoopAsync(CancellationToken cancellationToken, TimeSpan interval)
                                                    {
                                                        private async Task ProcessOutboxItemAsync(JournalOutboxItem item, CancellationToken cancellationToken)
                                                        {
                                                            private void Log(string message) =>
                                                            public void Dispose()
                                                            {
                                                            }

                                                    }
                                                public partial class JournalSyncHub : IDisposable
                                                {
                                                    private readonly JournalSyncTracker _tracker;


                                                    private readonly JournalOutbox _outbox;


                                                    private readonly ConcurrentDictionary<string, LiveSyncProgress> _activeTransfers = new(StringComparer.OrdinalIgnoreCase);


                                                    private CancellationTokenSource? _cts;


                                                    private Task? _syncLoop;


                                                    private readonly object _lock = new();


                                                    private volatile bool _disposed;


                                                    public bool IsRunning { get; private set; }


                                                    public int ActiveTransferCount => _activeTransfers.Count;


                                                    public long TotalBytesProcessed { get; private set; }


                                                    public long TotalFilesProcessed { get; private set; }


                                                    public JournalOutbox Outbox => _outbox;


                                                    public JournalSyncTracker Tracker => _tracker;


                                                    public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum)
                                                    {
                                                        _outbox.Enqueue(atmId, fileName, data, offset, checksum);
                                                        _tracker.AddOrGet(atmId, fileName, data.Length, offset, checksum);
                                                    }


                                                public JournalSyncHub()
                                                _tracker = new JournalSyncTracker();


                                                // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Services\JournalSyncHub.cs
                                                public JournalSyncHub()
                                                {
                                                    _tracker = new JournalSyncTracker();
                                                    _outbox = new JournalOutbox();
                                                    _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
                                                }


                                            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\JournalSyncHub.cs
                                            public JournalSyncHub()
                                            {
                                                _tracker = new JournalSyncTracker();
                                                _outbox = new JournalOutbox();
                                                _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
                                            }


                                        // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Services\JournalSyncHub.cs
                                        public JournalSyncHub()
                                        {
                                            _tracker = new JournalSyncTracker();
                                            _outbox = new JournalOutbox();
                                            _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
                                        }


                                    // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Services\JournalSyncHub.cs
                                    public JournalSyncHub()
                                    {
                                        _tracker = new JournalSyncTracker();
                                        _outbox = new JournalOutbox();
                                        _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
                                    }


                                _outbox = new JournalOutbox();


                                _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);


                                public void Start(TimeSpan? syncInterval = null)
                                {
                                    lock (_lock)
                                    {
                                        if (IsRunning) return;

                                        IsRunning = true;
                                        _cts?.Dispose();
                                        _cts = new CancellationTokenSource();
                                        var interval = syncInterval ?? TimeSpan.FromMilliseconds(500);
                                        _syncLoop = Task.Run(() => ProcessSyncLoopAsync(_cts.Token, interval), _cts.Token);
                                        Log("JournalSyncHub started.");
                                    }
                            }


                        public void Stop()
                        {
                            lock (_lock)
                            {
                                if (!IsRunning) return;

                                IsRunning = false;
                                try { _cts?.Cancel(); } catch { }

                                try { _syncLoop?.Wait(TimeSpan.FromSeconds(5)); } catch { }

                                Log("JournalSyncHub stopped.");
                            }
                    }


                public void ReportProgress(LiveSyncProgress progress)
                {
                    if (progress is null) return;
                    _activeTransfers[progress.ATMId ?? "unknown"] = progress;
                    ProgressChanged?.Invoke(this, progress);
                }


            public IReadOnlyDictionary<string, LiveSyncProgress> GetActiveTransfers()
            => new ConcurrentDictionary<string, LiveSyncProgress>(_activeTransfers);


            public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
            => _tracker.AddOrGet(atmId, fileName, fileSize, offset, checksum);


            public void MarkSyncing(string atmId, string syncId, int chunkPercent)
            => _tracker.MarkSyncing(atmId, syncId, chunkPercent);


            public void MarkCompleted(string atmId, string syncId, string? sha256 = null)
            => _tracker.MarkCompleted(atmId, syncId, sha256);


            public void MarkFailed(string atmId, string syncId, string reason)
            => _tracker.MarkFailed(atmId, syncId, reason);


            public void MarkReSyncing(string atmId, string syncId)
            => _tracker.MarkReSyncing(atmId, syncId);


            public void MarkArchived(string syncId)
            => _tracker.MarkArchived(syncId);


            public List<JournalSyncRecord> GetAllForATM(string atmId)
            => _tracker.GetAllForATM(atmId);


            public List<JournalSyncRecord> GetPendingForATM(string atmId)
            => _tracker.GetPendingForATM(atmId);


            public List<JournalSyncRecord> GetFailedForATM(string atmId)
            => _tracker.GetFailedForATM(atmId);


            public int GetPendingCount(string atmId)
            => _tracker.GetPendingCount(atmId);


            public (int completed, int failed, int pending) GetStats(string atmId)
            => _tracker.GetStats(atmId);


            public void LoadFromDatabase(string atmId)
            => _tracker.LoadFromDatabase(atmId);


            public void ResetInFlightToResync(string atmId)
            => _tracker.ResetInFlightToResync(atmId);


            public int RequeueTimedOut() => _outbox.RequeueTimedOutAcknowledgements(
            RetryPolicy.ForNetwork("LAN"), 50);


            public SyncSummary BuildSummary(string? atmId = null)
            {
                var allRecords = atmId != null;
                ? _tracker.GetAllForATM(atmId)
                : _tracker.GetAllForATM("*");

                return new SyncSummary
                {
                    Total = allRecords.Count,
                    Pending = allRecords.Count(r => r.State == JournalSyncState.Pending),
                    InProgress = allRecords.Count(r => r.State is JournalSyncState.Syncing or JournalSyncState.ReSyncing),
                    Completed = allRecords.Count(r => r.State == JournalSyncState.Completed),
                    Failed = allRecords.Count(r => r.State == JournalSyncState.Failed),
                    AverageProgress = allRecords.Count == 0 ? 0
                    : (int)Math.Round(allRecords.Average(r => r.ProgressPercent)),
                    TotalBytesProcessed = TotalBytesProcessed,
                    TotalFilesProcessed = TotalFilesProcessed,
                    ActiveTransfers = ActiveTransferCount
                };
        }


    private async Task ProcessSyncLoopAsync(CancellationToken cancellationToken, TimeSpan interval)
    {
        const int batchSize = 5;
        const int errorDelayMs = 2000;

        while (!cancellationToken.IsCancellationRequested && IsRunning)
        {
            try
            {
                var processed = 0;
                for (var i = 0; i < batchSize && !cancellationToken.IsCancellationRequested; i++)
                {
                    if (!_outbox.TryDequeue(out var item))
                    break;

                    processed++;
                    await ProcessOutboxItemAsync(item, cancellationToken).ConfigureAwait(false);
                }

            var requeued = RequeueTimedOut();
            if (requeued > 0)
            Log($"Requeued {requeued} timed-out items.");

            if (processed == 0)
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            else
            await Task.Delay(50, cancellationToken).ConfigureAwait(false);
        }
    catch (OperationCanceledException) { break; }
    catch (Exception ex)
    {
        Log($"Sync loop error: {ex.Message}");
        OnSyncError?.Invoke(this, ex.Message);
        try { await Task.Delay(errorDelayMs, cancellationToken).ConfigureAwait(false); }
        catch { break; }
    }
}
}


private async Task ProcessOutboxItemAsync(JournalOutboxItem item, CancellationToken cancellationToken)
{
    var progress = new LiveSyncProgress;
    {
        ATMId = item.ATM_ID,
        FileName = item.FileName,
        TotalBytes = item.SizeBytes,
        StateLabel = "Syncing",
        StateIcon = "⟳",
        UpdatedAt = DateTime.UtcNow
    };

try
{
    _outbox.MarkAwaitingAcknowledgement(item.ItemId, TimeSpan.FromMinutes(2));

    progress.StateLabel = "Sending";
    progress.Percent = 0;
    ReportProgress(progress);

    var chunks = Math.Max(1, (int)Math.Ceiling(item.SizeBytes / (double)AppConstants.ChunkSizeBytes));
    for (var seq = 0; seq < chunks; seq++)
    {
        if (cancellationToken.IsCancellationRequested) break;

        progress.BytesSent = Math.Min((seq + 1) * (long)AppConstants.ChunkSizeBytes, item.SizeBytes);
        progress.Percent = (int)(progress.BytesSent * 100 / Math.Max(1, item.SizeBytes));
        progress.SeqNum = seq;
        progress.TotalChunks = chunks;
        progress.UpdatedAt = DateTime.UtcNow;
        ReportProgress(progress);

        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
    }

progress.StateLabel = "Completed";
progress.StateIcon = "✓";
progress.Percent = 100;
progress.BytesSent = item.SizeBytes;
progress.UpdatedAt = DateTime.UtcNow;
_outbox.MarkCompleted(item.ItemId, "sync-ok");
_tracker.MarkCompleted(item.ATM_ID, $"hub-{item.ItemId}", null);
ReportProgress(progress);

Interlocked.Add(ref TotalBytesProcessed, item.SizeBytes);
Interlocked.Increment(ref TotalFilesProcessed);
}
catch (OperationCanceledException)
{
    _outbox.RetryItem(item.ItemId, 5000, "cancelled");
    progress.StateLabel = "Cancelled";
    progress.StateIcon = "⊘";
    ReportProgress(progress);
}
catch (Exception ex)
{
    _outbox.RetryItem(item.ItemId, 5000, ex.Message);
    _tracker.MarkFailed(item.ATM_ID, $"hub-{item.ItemId}", ex.Message);
    progress.StateLabel = "Failed";
    progress.StateIcon = "✕";
    ReportProgress(progress);
    Log($"Sync failed for {item.FileName}: {ex.Message}");
    OnSyncError?.Invoke(this, $"Failed: {item.FileName} — {ex.Message}");
}
finally
{
    _activeTransfers.TryRemove(item.ATM_ID, out _);
}
}


private void Log(string message) => OnLog?.Invoke(this, $"[JournalSyncHub] {message}");


public void Dispose()
{
    if (_disposed) return;
    _disposed = true;
    Stop();
    _cts?.Dispose();
    _syncLoop?.Dispose();
}


public event EventHandler<LiveSyncProgress>? ProgressChanged;


public event EventHandler<JournalSyncRecord>? StateChanged;


public event EventHandler<string>? OnSyncError;


public event EventHandler<string>? OnLog;

}
public partial class JournalSyncHub : IDisposable
{
    private readonly JournalSyncTracker _tracker;


    private readonly JournalOutbox _outbox;


    private readonly ConcurrentDictionary<string, LiveSyncProgress> _activeTransfers = new(StringComparer.OrdinalIgnoreCase);


    private CancellationTokenSource? _cts;


    private Task? _syncLoop;


    private readonly object _lock = new();


    private volatile bool _disposed;


    public bool IsRunning { get; private set; }


    public int ActiveTransferCount => _activeTransfers.Count;


    public long TotalBytesProcessed { get; private set; }


    public long TotalFilesProcessed { get; private set; }


    public JournalOutbox Outbox => _outbox;


    public JournalSyncTracker Tracker => _tracker;


    public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum)
    {
        _outbox.Enqueue(atmId, fileName, data, offset, checksum);
        _tracker.AddOrGet(atmId, fileName, data.Length, offset, checksum);
    }


public JournalSyncHub()
_tracker = new JournalSyncTracker();


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Services\JournalSyncHub.cs
public JournalSyncHub()
{
    _tracker = new JournalSyncTracker();
    _outbox = new JournalOutbox();
    _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\JournalSyncHub.cs
public JournalSyncHub()
{
    _tracker = new JournalSyncTracker();
    _outbox = new JournalOutbox();
    _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
}


_outbox = new JournalOutbox();


_tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);


public void Start(TimeSpan? syncInterval = null)
{
    lock (_lock)
    {
        if (IsRunning) return;

        IsRunning = true;
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var interval = syncInterval ?? TimeSpan.FromMilliseconds(500);
        _syncLoop = Task.Run(() => ProcessSyncLoopAsync(_cts.Token, interval), _cts.Token);
        Log("JournalSyncHub started.");
    }
}


public void Stop()
{
    lock (_lock)
    {
        if (!IsRunning) return;

        IsRunning = false;
        try { _cts?.Cancel(); } catch { }

        try { _syncLoop?.Wait(TimeSpan.FromSeconds(5)); } catch { }

        Log("JournalSyncHub stopped.");
    }
}


public void ReportProgress(LiveSyncProgress progress)
{
    if (progress is null) return;
    _activeTransfers[progress.ATMId ?? "unknown"] = progress;
    ProgressChanged?.Invoke(this, progress);
}


public IReadOnlyDictionary<string, LiveSyncProgress> GetActiveTransfers()
=> new ConcurrentDictionary<string, LiveSyncProgress>(_activeTransfers);


public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
=> _tracker.AddOrGet(atmId, fileName, fileSize, offset, checksum);


public void MarkSyncing(string atmId, string syncId, int chunkPercent)
=> _tracker.MarkSyncing(atmId, syncId, chunkPercent);


public void MarkCompleted(string atmId, string syncId, string? sha256 = null)
=> _tracker.MarkCompleted(atmId, syncId, sha256);


public void MarkFailed(string atmId, string syncId, string reason)
=> _tracker.MarkFailed(atmId, syncId, reason);


public void MarkReSyncing(string atmId, string syncId)
=> _tracker.MarkReSyncing(atmId, syncId);


public void MarkArchived(string syncId)
=> _tracker.MarkArchived(syncId);


public List<JournalSyncRecord> GetAllForATM(string atmId)
=> _tracker.GetAllForATM(atmId);


public List<JournalSyncRecord> GetPendingForATM(string atmId)
=> _tracker.GetPendingForATM(atmId);


public List<JournalSyncRecord> GetFailedForATM(string atmId)
=> _tracker.GetFailedForATM(atmId);


public int GetPendingCount(string atmId)
=> _tracker.GetPendingCount(atmId);


public (int completed, int failed, int pending) GetStats(string atmId)
=> _tracker.GetStats(atmId);


public void LoadFromDatabase(string atmId)
=> _tracker.LoadFromDatabase(atmId);


public void ResetInFlightToResync(string atmId)
=> _tracker.ResetInFlightToResync(atmId);


public int RequeueTimedOut() => _outbox.RequeueTimedOutAcknowledgements(
RetryPolicy.ForNetwork("LAN"), 50);


public SyncSummary BuildSummary(string? atmId = null)
{
    var allRecords = atmId != null;
    ? _tracker.GetAllForATM(atmId)
    : _tracker.GetAllForATM("*");

    return new SyncSummary
    {
        Total = allRecords.Count,
        Pending = allRecords.Count(r => r.State == JournalSyncState.Pending),
        InProgress = allRecords.Count(r => r.State is JournalSyncState.Syncing or JournalSyncState.ReSyncing),
        Completed = allRecords.Count(r => r.State == JournalSyncState.Completed),
        Failed = allRecords.Count(r => r.State == JournalSyncState.Failed),
        AverageProgress = allRecords.Count == 0 ? 0
        : (int)Math.Round(allRecords.Average(r => r.ProgressPercent)),
        TotalBytesProcessed = TotalBytesProcessed,
        TotalFilesProcessed = TotalFilesProcessed,
        ActiveTransfers = ActiveTransferCount
    };
}


private async Task ProcessSyncLoopAsync(CancellationToken cancellationToken, TimeSpan interval)
{
    const int batchSize = 5;
    const int errorDelayMs = 2000;

    while (!cancellationToken.IsCancellationRequested && IsRunning)
    {
        try
        {
            var processed = 0;
            for (var i = 0; i < batchSize && !cancellationToken.IsCancellationRequested; i++)
            {
                if (!_outbox.TryDequeue(out var item))
                break;

                processed++;
                await ProcessOutboxItemAsync(item, cancellationToken).ConfigureAwait(false);
            }

        var requeued = RequeueTimedOut();
        if (requeued > 0)
        Log($"Requeued {requeued} timed-out items.");

        if (processed == 0)
        await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        else
        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
    }
catch (OperationCanceledException) { break; }
catch (Exception ex)
{
    Log($"Sync loop error: {ex.Message}");
    OnSyncError?.Invoke(this, ex.Message);
    try { await Task.Delay(errorDelayMs, cancellationToken).ConfigureAwait(false); }
    catch { break; }
}
}
}


private async Task ProcessOutboxItemAsync(JournalOutboxItem item, CancellationToken cancellationToken)
{
    var progress = new LiveSyncProgress;
    {
        ATMId = item.ATM_ID,
        FileName = item.FileName,
        TotalBytes = item.SizeBytes,
        StateLabel = "Syncing",
        StateIcon = "⟳",
        UpdatedAt = DateTime.UtcNow
    };

try
{
    _outbox.MarkAwaitingAcknowledgement(item.ItemId, TimeSpan.FromMinutes(2));

    progress.StateLabel = "Sending";
    progress.Percent = 0;
    ReportProgress(progress);

    var chunks = Math.Max(1, (int)Math.Ceiling(item.SizeBytes / (double)AppConstants.ChunkSizeBytes));
    for (var seq = 0; seq < chunks; seq++)
    {
        if (cancellationToken.IsCancellationRequested) break;

        progress.BytesSent = Math.Min((seq + 1) * (long)AppConstants.ChunkSizeBytes, item.SizeBytes);
        progress.Percent = (int)(progress.BytesSent * 100 / Math.Max(1, item.SizeBytes));
        progress.SeqNum = seq;
        progress.TotalChunks = chunks;
        progress.UpdatedAt = DateTime.UtcNow;
        ReportProgress(progress);

        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
    }

progress.StateLabel = "Completed";
progress.StateIcon = "✓";
progress.Percent = 100;
progress.BytesSent = item.SizeBytes;
progress.UpdatedAt = DateTime.UtcNow;
_outbox.MarkCompleted(item.ItemId, "sync-ok");
_tracker.MarkCompleted(item.ATM_ID, $"hub-{item.ItemId}", null);
ReportProgress(progress);

Interlocked.Add(ref TotalBytesProcessed, item.SizeBytes);
Interlocked.Increment(ref TotalFilesProcessed);
}
catch (OperationCanceledException)
{
    _outbox.RetryItem(item.ItemId, 5000, "cancelled");
    progress.StateLabel = "Cancelled";
    progress.StateIcon = "⊘";
    ReportProgress(progress);
}
catch (Exception ex)
{
    _outbox.RetryItem(item.ItemId, 5000, ex.Message);
    _tracker.MarkFailed(item.ATM_ID, $"hub-{item.ItemId}", ex.Message);
    progress.StateLabel = "Failed";
    progress.StateIcon = "✕";
    ReportProgress(progress);
    Log($"Sync failed for {item.FileName}: {ex.Message}");
    OnSyncError?.Invoke(this, $"Failed: {item.FileName} — {ex.Message}");
}
finally
{
    _activeTransfers.TryRemove(item.ATM_ID, out _);
}
}


private void Log(string message) => OnLog?.Invoke(this, $"[JournalSyncHub] {message}");


public void Dispose()
{
    if (_disposed) return;
    _disposed = true;
    Stop();
    _cts?.Dispose();
    _syncLoop?.Dispose();
}


public event EventHandler<LiveSyncProgress>? ProgressChanged;


public event EventHandler<JournalSyncRecord>? StateChanged;


public event EventHandler<string>? OnSyncError;


public event EventHandler<string>? OnLog;

}
// Class: JournalSyncHub (from 3 sources)
public sealed partial class JournalSyncHub : IDisposable
{
    // --- Constants & Fields ---
    private readonly JournalSyncTracker _tracker;

    private readonly JournalOutbox _outbox;

    private readonly ConcurrentDictionary<string, LiveSyncProgress> _activeTransfers = new(StringComparer.OrdinalIgnoreCase);

    private CancellationTokenSource? _cts;

    private Task? _syncLoop;

    private readonly object _lock = new();

    private volatile bool _disposed;


    // --- Properties ---
    public bool IsRunning { get; private set; }

    public int ActiveTransferCount => _activeTransfers.Count;

    public long TotalBytesProcessed { get; private set; }

    public long TotalFilesProcessed { get; private set; }

    public JournalOutbox Outbox => _outbox;

    public JournalSyncTracker Tracker => _tracker;

    public void Queue(string atmId, string fileName, byte[] data, long offset, string checksum)
    {
        _outbox.Enqueue(atmId, fileName, data, offset, checksum);
        _tracker.AddOrGet(atmId, fileName, data.Length, offset, checksum);
    }


// --- Constructors ---
public JournalSyncHub()
_tracker = new JournalSyncTracker();

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\JournalSyncHub.cs
public JournalSyncHub()
{
    _tracker = new JournalSyncTracker();
    _outbox = new JournalOutbox();
    _tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);
}


// --- Methods ---
_outbox = new JournalOutbox();

_tracker.OnStateChanged += (_, record) => StateChanged?.Invoke(this, record);

public void Start(TimeSpan? syncInterval = null)
{
    lock (_lock)
    {
        if (IsRunning) return;

        IsRunning = true;
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var interval = syncInterval ?? TimeSpan.FromMilliseconds(500);
        _syncLoop = Task.Run(() => ProcessSyncLoopAsync(_cts.Token, interval), _cts.Token);
        Log("JournalSyncHub started.");
    }
}

public void Stop()
{
    lock (_lock)
    {
        if (!IsRunning) return;

        IsRunning = false;
        try { _cts?.Cancel(); } catch { }

        try { _syncLoop?.Wait(TimeSpan.FromSeconds(5)); } catch { }

        Log("JournalSyncHub stopped.");
    }
}

public void ReportProgress(LiveSyncProgress progress)
{
    if (progress is null) return;
    _activeTransfers[progress.ATMId ?? "unknown"] = progress;
    ProgressChanged?.Invoke(this, progress);
}

public IReadOnlyDictionary<string, LiveSyncProgress> GetActiveTransfers()
=> new ConcurrentDictionary<string, LiveSyncProgress>(_activeTransfers);

public JournalSyncRecord AddOrGet(string atmId, string fileName, long fileSize, long offset, string checksum)
=> _tracker.AddOrGet(atmId, fileName, fileSize, offset, checksum);

public void MarkSyncing(string atmId, string syncId, int chunkPercent)
=> _tracker.MarkSyncing(atmId, syncId, chunkPercent);

public void MarkCompleted(string atmId, string syncId, string? sha256 = null)
=> _tracker.MarkCompleted(atmId, syncId, sha256);

public void MarkFailed(string atmId, string syncId, string reason)
=> _tracker.MarkFailed(atmId, syncId, reason);

public void MarkReSyncing(string atmId, string syncId)
=> _tracker.MarkReSyncing(atmId, syncId);

public void MarkArchived(string syncId)
=> _tracker.MarkArchived(syncId);

public List<JournalSyncRecord> GetAllForATM(string atmId)
=> _tracker.GetAllForATM(atmId);

public List<JournalSyncRecord> GetPendingForATM(string atmId)
=> _tracker.GetPendingForATM(atmId);

public List<JournalSyncRecord> GetFailedForATM(string atmId)
=> _tracker.GetFailedForATM(atmId);

public int GetPendingCount(string atmId)
=> _tracker.GetPendingCount(atmId);

public (int completed, int failed, int pending) GetStats(string atmId)
=> _tracker.GetStats(atmId);

public void LoadFromDatabase(string atmId)
=> _tracker.LoadFromDatabase(atmId);

public void ResetInFlightToResync(string atmId)
=> _tracker.ResetInFlightToResync(atmId);

public int RequeueTimedOut() => _outbox.RequeueTimedOutAcknowledgements(
RetryPolicy.ForNetwork("LAN"), 50);

public SyncSummary BuildSummary(string? atmId = null)
{
    var allRecords = atmId != null;
    ? _tracker.GetAllForATM(atmId)
    : _tracker.GetAllForATM("*");

    return new SyncSummary
    {
        Total = allRecords.Count,
        Pending = allRecords.Count(r => r.State == JournalSyncState.Pending),
        InProgress = allRecords.Count(r => r.State is JournalSyncState.Syncing or JournalSyncState.ReSyncing),
        Completed = allRecords.Count(r => r.State == JournalSyncState.Completed),
        Failed = allRecords.Count(r => r.State == JournalSyncState.Failed),
        AverageProgress = allRecords.Count == 0 ? 0
        : (int)Math.Round(allRecords.Average(r => r.ProgressPercent)),
        TotalBytesProcessed = TotalBytesProcessed,
        TotalFilesProcessed = TotalFilesProcessed,
        ActiveTransfers = ActiveTransferCount
    };
}

private async Task ProcessSyncLoopAsync(CancellationToken cancellationToken, TimeSpan interval)
{
    const int batchSize = 5;
    const int errorDelayMs = 2000;

    while (!cancellationToken.IsCancellationRequested && IsRunning)
    {
        try
        {
            var processed = 0;
            for (var i = 0; i < batchSize && !cancellationToken.IsCancellationRequested; i++)
            {
                if (!_outbox.TryDequeue(out var item))
                break;

                processed++;
                await ProcessOutboxItemAsync(item, cancellationToken).ConfigureAwait(false);
            }

        var requeued = RequeueTimedOut();
        if (requeued > 0)
        Log($"Requeued {requeued} timed-out items.");

        if (processed == 0)
        await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        else
        await Task.Delay(50, cancellationToken).ConfigureAwait(false);
    }
catch (OperationCanceledException) { break; }
catch (Exception ex)
{
    Log($"Sync loop error: {ex.Message}");
    OnSyncError?.Invoke(this, ex.Message);
    try { await Task.Delay(errorDelayMs, cancellationToken).ConfigureAwait(false); }
    catch { break; }
}
}
}

private async Task ProcessOutboxItemAsync(JournalOutboxItem item, CancellationToken cancellationToken)
{
    var progress = new LiveSyncProgress;
    {
        ATMId = item.ATM_ID,
        FileName = item.FileName,
        TotalBytes = item.SizeBytes,
        StateLabel = "Syncing",
        StateIcon = "⟳",
        UpdatedAt = DateTime.UtcNow
    };

try
{
    _outbox.MarkAwaitingAcknowledgement(item.ItemId, TimeSpan.FromMinutes(2));

    progress.StateLabel = "Sending";
    progress.Percent = 0;
    ReportProgress(progress);

    var chunks = Math.Max(1, (int)Math.Ceiling(item.SizeBytes / (double)AppConstants.ChunkSizeBytes));
    for (var seq = 0; seq < chunks; seq++)
    {
        if (cancellationToken.IsCancellationRequested) break;

        progress.BytesSent = Math.Min((seq + 1) * (long)AppConstants.ChunkSizeBytes, item.SizeBytes);
        progress.Percent = (int)(progress.BytesSent * 100 / Math.Max(1, item.SizeBytes));
        progress.SeqNum = seq;
        progress.TotalChunks = chunks;
        progress.UpdatedAt = DateTime.UtcNow;
        ReportProgress(progress);

        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
    }

progress.StateLabel = "Completed";
progress.StateIcon = "✓";
progress.Percent = 100;
progress.BytesSent = item.SizeBytes;
progress.UpdatedAt = DateTime.UtcNow;
_outbox.MarkCompleted(item.ItemId, "sync-ok");
_tracker.MarkCompleted(item.ATM_ID, $"hub-{item.ItemId}", null);
ReportProgress(progress);

Interlocked.Add(ref TotalBytesProcessed, item.SizeBytes);
Interlocked.Increment(ref TotalFilesProcessed);
}
catch (OperationCanceledException)
{
    _outbox.RetryItem(item.ItemId, 5000, "cancelled");
    progress.StateLabel = "Cancelled";
    progress.StateIcon = "⊘";
    ReportProgress(progress);
}
catch (Exception ex)
{
    _outbox.RetryItem(item.ItemId, 5000, ex.Message);
    _tracker.MarkFailed(item.ATM_ID, $"hub-{item.ItemId}", ex.Message);
    progress.StateLabel = "Failed";
    progress.StateIcon = "✕";
    ReportProgress(progress);
    Log($"Sync failed for {item.FileName}: {ex.Message}");
    OnSyncError?.Invoke(this, $"Failed: {item.FileName} — {ex.Message}");
}
finally
{
    _activeTransfers.TryRemove(item.ATM_ID, out _);
}
}

private void Log(string message) => OnLog?.Invoke(this, $"[JournalSyncHub] {message}");

public void Dispose()
{
    if (_disposed) return;
    _disposed = true;
    Stop();
    _cts?.Dispose();
    _syncLoop?.Dispose();
}


// --- Events ---
public event EventHandler<LiveSyncProgress>? ProgressChanged;

public event EventHandler<JournalSyncRecord>? StateChanged;

public event EventHandler<string>? OnSyncError;

public event EventHandler<string>? OnLog;

}
}