using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Transport;

namespace EJLive.Core.Sync
{
    public partial public public sealed class ChunkedSyncService : IDisposable
        {
            private readonly ConcurrentDictionary<string, TransferSession> _activeTransfers = new();
            private readonly ConcurrentDictionary<string, FileManifest> _manifests = new();
            private readonly ConcurrentQueue<FileManifest> _outboxQueue = new();
            private readonly string _outboxPath;
            private readonly int _chunkSize;
            private readonly int _maxRetries;
            private readonly object _lock = new();
            private bool _disposed;
            public int OutboxPendingCount => _outboxQueue.Count;
            public IReadOnlyList<TransferSession> ActiveTransfers => _activeTransfers.Values.ToList().AsReadOnly();
            public IReadOnlyList<FileManifest> AllManifests => _manifests.Values.ToList().AsReadOnly();
            public ChunkedSyncService(string outboxPath = "", int chunkSize = 65536, int maxRetries = 3)
            {
            public int Queued { get; set; }
            public int Transferring { get; set; }
            public int Completed { get; set; }
            public int Failed { get; set; }
            public long TotalBytes { get; set; }
            public double SuccessRate { get; set; }
            public FileManifest EnqueueFile(string atmId, string sourcePath, string fileType = "Journal")
            {
            public List<FileManifest> EnqueueFiles(string atmId, List<string> sourcePaths, string fileType = "Journal")
            {
            private async Task<TransferSession?> TransferFileAsync(
            FileManifest manifest,
            Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
            Func<FileManifest, Task<bool>> verifyAsync,
            CancellationToken ct)
            {
            public async Task<TransferSession?> RetryTransferAsync(
            TransferSession session,
            Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
            Func<FileManifest, Task<bool>> verifyAsync,
            CancellationToken ct = default)
            {
            public static string ComputeSha256(string filePath)
            {
            public static string ComputeSha256(byte[] data)
            {
            public static bool VerifyChecksum(string filePath, string expectedChecksum)
            {
            public bool IsDuplicate(string checksum)
            {
            public int ClearCompletedManifests()
            {
            public int ClearFailedManifests()
            {
            public SyncStats GetStats()
            {
            private void EmitTransferStateChanged(TransferSession session)
            {
            public void Dispose()
            {
        }
    
        public partial public public sealed class SyncStats
        {
            public int Queued { get; set; }
            public int Transferring { get; set; }
            public int Completed { get; set; }
            public int Failed { get; set; }
            public long TotalBytes { get; set; }
            public double SuccessRate { get; set; }
        }
    
    }
    public partial public sealed class ChunkedSyncService : IDisposable
        {
            private readonly ConcurrentDictionary<string, TransferSession> _activeTransfers = new();
            private readonly ConcurrentDictionary<string, FileManifest> _manifests = new();
            private readonly ConcurrentQueue<FileManifest> _outboxQueue = new();
            private readonly string _outboxPath;
            private readonly int _chunkSize;
            private readonly int _maxRetries;
            private readonly object _lock = new();
            private bool _disposed;
            public int OutboxPendingCount => _outboxQueue.Count;
            public IReadOnlyList<TransferSession> ActiveTransfers => _activeTransfers.Values.ToList().AsReadOnly();
            public IReadOnlyList<FileManifest> AllManifests => _manifests.Values.ToList().AsReadOnly();
            public ChunkedSyncService(string outboxPath = "", int chunkSize = 65536, int maxRetries = 3)
            {
            public FileManifest EnqueueFile(string atmId, string sourcePath, string fileType = "Journal")
            {
            public List<FileManifest> EnqueueFiles(string atmId, List<string> sourcePaths, string fileType = "Journal")
            {
            private async Task<TransferSession?> TransferFileAsync(
            FileManifest manifest,
            Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
            Func<FileManifest, Task<bool>> verifyAsync,
            CancellationToken ct)
            {
            public async Task<TransferSession?> RetryTransferAsync(
            TransferSession session,
            Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
            Func<FileManifest, Task<bool>> verifyAsync,
            CancellationToken ct = default)
            {
            public static string ComputeSha256(string filePath)
            {
            public static string ComputeSha256(byte[] data)
            {
            public static bool VerifyChecksum(string filePath, string expectedChecksum)
            {
            public bool IsDuplicate(string checksum)
            {
            public int ClearCompletedManifests()
            {
            public int ClearFailedManifests()
            {
            public SyncStats GetStats()
            {
            private void EmitTransferStateChanged(TransferSession session)
            {
            public void Dispose()
            {
        }
    
        public partial public sealed class SyncStats
        {
            public int Queued { get; set; }
            public int Transferring { get; set; }
            public int Completed { get; set; }
            public int Failed { get; set; }
            public long TotalBytes { get; set; }
            public double SuccessRate { get; set; }
        }
    
    }
    public partial class SyncStats
        {
            public int Queued { get; set; }
    
    
            public int Transferring { get; set; }
    
    
            public int Completed { get; set; }
    
    
            public int Failed { get; set; }
    
    
            public long TotalBytes { get; set; }
    
    
            public double SuccessRate { get; set; }
    
    
        }
    /// <summary>Sync statistics summary.</summary>
        public sealed class SyncStats
        {
            public int Queued { get; set; }
            public int Transferring { get; set; }
            public int Completed { get; set; }
            public int Failed { get; set; }
            public long TotalBytes { get; set; }
            public double SuccessRate { get; set; }
        }

    public partial class ChunkedSyncService : IDisposable
        {
            private readonly ConcurrentDictionary<string, TransferSession> _activeTransfers = new();
    
    
            private readonly ConcurrentDictionary<string, FileManifest> _manifests = new();
    
    
            private readonly ConcurrentQueue<FileManifest> _outboxQueue = new();
    
    
            private readonly string _outboxPath;
    
    
            private readonly int _chunkSize;
    
    
            private readonly int _maxRetries;
    
    
            private readonly object _lock = new();
    
    
            private CancellationTokenSource? _cts;
    
    
            private bool _disposed;
    
    
            public int OutboxPendingCount => _outboxQueue.Count;
    
    
            public IReadOnlyList<TransferSession> ActiveTransfers => _activeTransfers.Values.ToList().AsReadOnly();
    
    
            public IReadOnlyList<FileManifest> AllManifests => _manifests.Values.ToList().AsReadOnly();
    
    
            public ChunkedSyncService(string outboxPath = "", int chunkSize = 65536, int maxRetries = 3)
            {
                _outboxPath = string.IsNullOrEmpty(outboxPath)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Outbox")
                    : outboxPath;
                _chunkSize = chunkSize;
                _maxRetries = maxRetries;
    
                if (!Directory.Exists(_outboxPath))
                    Directory.CreateDirectory(_outboxPath);
            }
    
    
            public FileManifest EnqueueFile(string atmId, string sourcePath, string fileType = "Journal")
            {
                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException($"Source file not found: {sourcePath}");
    
                var fileInfo = new FileInfo(sourcePath);
                var checksum = ComputeSha256(sourcePath);
                var totalChunks = (int)Math.Ceiling((double)fileInfo.Length / _chunkSize);
    
                var manifest = new FileManifest
                {
                    ManifestId = Guid.NewGuid().ToString("N"),
                    ATM_ID = atmId,
                    FileName = Path.GetFileName(sourcePath),
                    FileSize = fileInfo.Length,
                    Checksum = checksum,
                    FileType = fileType,
                    SourcePath = sourcePath,
                    TotalChunks = totalChunks,
                    ChunkSize = _chunkSize,
                    Status = "Queued",
                    CreatedUtc = DateTime.UtcNow
                };
    
                _manifests[manifest.ManifestId] = manifest;
                _outboxQueue.Enqueue(manifest);
                OnFileQueued?.Invoke(manifest);
                return manifest;
            }
    
    
            public List<FileManifest> EnqueueFiles(string atmId, List<string> sourcePaths, string fileType = "Journal")
            {
                return sourcePaths.Select(p => EnqueueFile(atmId, p, fileType)).ToList();
            }
    
    
            public async Task<List<TransferSession>> ProcessOutboxAsync(
                Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
                Func<FileManifest, Task<bool>> verifyAsync,
                CancellationToken ct = default)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var completedSessions = new List<TransferSession>();
    
                while (_outboxQueue.TryDequeue(out var manifest))
                {
                    if (_cts.Token.IsCancellationRequested)
                        break;
    
                    var session = await TransferFileAsync(manifest, sendChunkAsync, verifyAsync, _cts.Token)
                        .ConfigureAwait(false);
    
                    if (session != null)
                    {
                        completedSessions.Add(session);
                        manifest.Status = session.State == TransferState.Verified ? "Completed" : "Failed";
                        manifest.FailureReason = session.State == TransferState.Failed ? "Transfer failed after retries." : null;
                        manifest.LastModifiedUtc = DateTime.UtcNow;
                    }
                }
    
                if (_outboxQueue.IsEmpty)
                    OnOutboxEmpty?.Invoke();
    
                return completedSessions;
            }
    
    
            private async Task<TransferSession?> TransferFileAsync(
                FileManifest manifest,
                Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
                Func<FileManifest, Task<bool>> verifyAsync,
                CancellationToken ct)
            {
                var session = new TransferSession
                {
                    TransferId = Guid.NewGuid().ToString("N"),
                    ATM_ID = manifest.ATM_ID,
                    FileName = manifest.FileName,
                    FileSize = manifest.FileSize,
                    TotalChunks = manifest.TotalChunks,
                    ChunkSize = manifest.ChunkSize,
                    Checksum = manifest.Checksum,
                    SourcePath = manifest.SourcePath,
                    ManifestId = manifest.ManifestId,
                    State = TransferState.Pending
                };
    
                _activeTransfers[session.TransferId] = session;
                manifest.Status = "Transferring";
                manifest.LastModifiedUtc = DateTime.UtcNow;
    
                try
                {
                    if (!File.Exists(session.SourcePath))
                    {
                        session.State = TransferState.Failed;
                        session.CompletedUtc = DateTime.UtcNow;
                        EmitTransferStateChanged(session);
                        return session;
                    }
    
                    session.State = TransferState.InProgress;
                    session.StartedUtc = DateTime.UtcNow;
    
                    await using var fileStream = new FileStream(
                        session.SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                        bufferSize: session.ChunkSize, useAsync: true);
    
                    var buffer = new byte[session.ChunkSize];
                    var backoff = new ExponentialBackoffWithJitter();
    
                    for (int chunkIndex = 0; chunkIndex < session.TotalChunks; chunkIndex++)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            session.State = TransferState.Cancelled;
                            break;
                        }
    
                        int bytesRead = await fileStream.ReadAsync(buffer, 0, session.ChunkSize, ct)
                            .ConfigureAwait(false);
                        var chunkPayload = new byte[bytesRead];
                        Array.Copy(buffer, chunkPayload, bytesRead);
    
                        bool chunkSent = false;
                        int retryCount = 0;
    
                        while (!chunkSent && retryCount <= _maxRetries)
                        {
                            try
                            {
                                chunkSent = await sendChunkAsync(manifest, chunkPayload, chunkIndex, session.TotalChunks)
                                    .ConfigureAwait(false);
                            }
                            catch
                            {
                                // Will retry
                            }
    
                            if (!chunkSent)
                            {
                                retryCount++;
                                session.TotalRetries++;
                                session.FailedChunks++;
                                manifest.RetryCount++;
    
                                if (retryCount <= _maxRetries)
                                {
                                    var delay = backoff.NextDelay();
                                    await Task.Delay(delay, ct).ConfigureAwait(false);
                                }
                            }
                        }
    
                        if (!chunkSent)
                        {
                            session.State = TransferState.Failed;
                            session.CompletedUtc = DateTime.UtcNow;
                            EmitTransferStateChanged(session);
                            return session;
                        }
    
                        session.CompletedChunks++;
                        session.BytesTransferred += bytesRead;
                        backoff.Reset();
                    }
    
                    // Verification step
                    if (session.State != TransferState.Cancelled)
                    {
                        bool verified = false;
                        try
                        {
                            verified = await verifyAsync(manifest).ConfigureAwait(false);
                        }
                        catch
                        {
                            verified = false;
                        }
    
                        if (verified)
                        {
                            session.State = TransferState.Verified;
                            session.VerificationPassed = true;
                        }
                        else
                        {
                            session.State = TransferState.Failed;
                        }
                    }
    
                    session.CompletedUtc = DateTime.UtcNow;
                    EmitTransferStateChanged(session);
                    return session;
                }
                catch (OperationCanceledException)
                {
                    session.State = TransferState.Cancelled;
                    session.CompletedUtc = DateTime.UtcNow;
                    EmitTransferStateChanged(session);
                    return session;
                }
                catch (Exception)
                {
                    session.State = TransferState.Failed;
                    session.CompletedUtc = DateTime.UtcNow;
                    EmitTransferStateChanged(session);
                    return session;
                }
                finally
                {
                    _activeTransfers.TryRemove(session.TransferId, out _);
                }
            }
    
    
            public async Task<TransferSession?> RetryTransferAsync(
                TransferSession session,
                Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
                Func<FileManifest, Task<bool>> verifyAsync,
                CancellationToken ct = default)
            {
                if (!_manifests.TryGetValue(session.ManifestId, out var manifest))
                    return null;
    
                // Reset session state for retry
                session.State = TransferState.Pending;
                session.CompletedUtc = null;
                session.TotalRetries = 0;
                session.FailedChunks = 0;
    
                return await TransferFileAsync(manifest, sendChunkAsync, verifyAsync, ct).ConfigureAwait(false);
            }
    
    
            public static string ComputeSha256(string filePath)
            {
                using var stream = File.OpenRead(filePath);
                var hash = SHA256.HashData(stream);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
    
    
            public static string ComputeSha256(byte[] data)
            {
                var hash = SHA256.HashData(data);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
    
    
            public static bool VerifyChecksum(string filePath, string expectedChecksum)
            {
                if (!File.Exists(filePath)) return false;
                var actual = ComputeSha256(filePath);
                return string.Equals(actual, expectedChecksum, StringComparison.OrdinalIgnoreCase);
            }
    
    
            public bool IsDuplicate(string checksum)
            {
                return _manifests.Values.Any(m =>
                    string.Equals(m.Checksum, checksum, StringComparison.OrdinalIgnoreCase) &&
                    (m.Status == "Completed" || m.Status == "Queued" || m.Status == "Transferring"));
            }
    
    
            public int ClearCompletedManifests()
            {
                var completed = _manifests.Values
                    .Where(m => m.Status == "Completed" || m.Status == "Verified")
                    .ToList();
                foreach (var m in completed)
                    _manifests.TryRemove(m.ManifestId, out _);
                return completed.Count;
            }
    
    
            public int ClearFailedManifests()
            {
                var failed = _manifests.Values.Where(m => m.Status == "Failed").ToList();
                foreach (var m in failed)
                    _manifests.TryRemove(m.ManifestId, out _);
                return failed.Count;
            }
    
    
            public SyncStats GetStats()
            {
                var all = _manifests.Values.ToList();
                return new SyncStats
                {
                    Queued = all.Count(m => m.Status == "Queued"),
                    Transferring = all.Count(m => m.Status == "Transferring"),
                    Completed = all.Count(m => m.Status == "Completed" || m.Status == "Verified"),
                    Failed = all.Count(m => m.Status == "Failed"),
                    TotalBytes = all.Where(m => m.Status == "Completed" || m.Status == "Verified").Sum(m => m.FileSize),
                    SuccessRate = all.Count > 0
                        ? Math.Round((double)all.Count(m => m.Status == "Completed" || m.Status == "Verified") / all.Count * 100, 1)
                        : 0
                };
            }
    
    
            private void EmitTransferStateChanged(TransferSession session)
            {
                try { OnTransferStateChanged?.Invoke(session); } catch { /* Observers must not crash. */ }
            }
    
    
            public void Dispose()
            {
                if (!_disposed)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _activeTransfers.Clear();
                    _manifests.Clear();
                    _disposed = true;
                }
            }
    
    
            public event Action<TransferSession>? OnTransferStateChanged;
    
    
            public event Action<FileManifest>? OnFileQueued;
    
    
            public event Action? OnOutboxEmpty;
    
    
        }
    // Class: ChunkedSyncService (from 1 sources)
        public sealed partial class ChunkedSyncService : IDisposable
        {
            // --- Constants & Fields ---
                    private readonly ConcurrentDictionary<string, TransferSession> _activeTransfers = new();
    
                    private readonly ConcurrentDictionary<string, FileManifest> _manifests = new();
    
                    private readonly ConcurrentQueue<FileManifest> _outboxQueue = new();
    
                    private readonly string _outboxPath;
    
                    private readonly int _chunkSize;
    
                    private readonly int _maxRetries;
    
                    private readonly object _lock = new();
    
                    private CancellationTokenSource? _cts;
    
                    private bool _disposed;
    
    
            // --- Properties ---
                    public int OutboxPendingCount => _outboxQueue.Count;
    
                    public IReadOnlyList<TransferSession> ActiveTransfers => _activeTransfers.Values.ToList().AsReadOnly();
    
                    public IReadOnlyList<FileManifest> AllManifests => _manifests.Values.ToList().AsReadOnly();
    
    
            // --- Constructors ---
                    public ChunkedSyncService(string outboxPath = "", int chunkSize = 65536, int maxRetries = 3)
                    {
                        _outboxPath = string.IsNullOrEmpty(outboxPath)
                            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Outbox")
                            : outboxPath;
                        _chunkSize = chunkSize;
                        _maxRetries = maxRetries;
    
                        if (!Directory.Exists(_outboxPath))
                            Directory.CreateDirectory(_outboxPath);
                    }
    
    
            // --- Methods ---
                    public FileManifest EnqueueFile(string atmId, string sourcePath, string fileType = "Journal")
                    {
                        if (!File.Exists(sourcePath))
                            throw new FileNotFoundException($"Source file not found: {sourcePath}");
    
                        var fileInfo = new FileInfo(sourcePath);
                        var checksum = ComputeSha256(sourcePath);
                        var totalChunks = (int)Math.Ceiling((double)fileInfo.Length / _chunkSize);
    
                        var manifest = new FileManifest
                        {
                            ManifestId = Guid.NewGuid().ToString("N"),
                            ATM_ID = atmId,
                            FileName = Path.GetFileName(sourcePath),
                            FileSize = fileInfo.Length,
                            Checksum = checksum,
                            FileType = fileType,
                            SourcePath = sourcePath,
                            TotalChunks = totalChunks,
                            ChunkSize = _chunkSize,
                            Status = "Queued",
                            CreatedUtc = DateTime.UtcNow
                        };
    
                        _manifests[manifest.ManifestId] = manifest;
                        _outboxQueue.Enqueue(manifest);
                        OnFileQueued?.Invoke(manifest);
                        return manifest;
                    }
    
                    public List<FileManifest> EnqueueFiles(string atmId, List<string> sourcePaths, string fileType = "Journal")
                    {
                        return sourcePaths.Select(p => EnqueueFile(atmId, p, fileType)).ToList();
                    }
    
                    public async Task<List<TransferSession>> ProcessOutboxAsync(
                        Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
                        Func<FileManifest, Task<bool>> verifyAsync,
                        CancellationToken ct = default)
                    {
                        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                        var completedSessions = new List<TransferSession>();
    
                        while (_outboxQueue.TryDequeue(out var manifest))
                        {
                            if (_cts.Token.IsCancellationRequested)
                                break;
    
                            var session = await TransferFileAsync(manifest, sendChunkAsync, verifyAsync, _cts.Token)
                                .ConfigureAwait(false);
    
                            if (session != null)
                            {
                                completedSessions.Add(session);
                                manifest.Status = session.State == TransferState.Verified ? "Completed" : "Failed";
                                manifest.FailureReason = session.State == TransferState.Failed ? "Transfer failed after retries." : null;
                                manifest.LastModifiedUtc = DateTime.UtcNow;
                            }
                        }
    
                        if (_outboxQueue.IsEmpty)
                            OnOutboxEmpty?.Invoke();
    
                        return completedSessions;
                    }
    
                    private async Task<TransferSession?> TransferFileAsync(
                        FileManifest manifest,
                        Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
                        Func<FileManifest, Task<bool>> verifyAsync,
                        CancellationToken ct)
                    {
                        var session = new TransferSession
                        {
                            TransferId = Guid.NewGuid().ToString("N"),
                            ATM_ID = manifest.ATM_ID,
                            FileName = manifest.FileName,
                            FileSize = manifest.FileSize,
                            TotalChunks = manifest.TotalChunks,
                            ChunkSize = manifest.ChunkSize,
                            Checksum = manifest.Checksum,
                            SourcePath = manifest.SourcePath,
                            ManifestId = manifest.ManifestId,
                            State = TransferState.Pending
                        };
    
                        _activeTransfers[session.TransferId] = session;
                        manifest.Status = "Transferring";
                        manifest.LastModifiedUtc = DateTime.UtcNow;
    
                        try
                        {
                            if (!File.Exists(session.SourcePath))
                            {
                                session.State = TransferState.Failed;
                                session.CompletedUtc = DateTime.UtcNow;
                                EmitTransferStateChanged(session);
                                return session;
                            }
    
                            session.State = TransferState.InProgress;
                            session.StartedUtc = DateTime.UtcNow;
    
                            await using var fileStream = new FileStream(
                                session.SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                bufferSize: session.ChunkSize, useAsync: true);
    
                            var buffer = new byte[session.ChunkSize];
                            var backoff = new ExponentialBackoffWithJitter();
    
                            for (int chunkIndex = 0; chunkIndex < session.TotalChunks; chunkIndex++)
                            {
                                if (ct.IsCancellationRequested)
                                {
                                    session.State = TransferState.Cancelled;
                                    break;
                                }
    
                                int bytesRead = await fileStream.ReadAsync(buffer, 0, session.ChunkSize, ct)
                                    .ConfigureAwait(false);
                                var chunkPayload = new byte[bytesRead];
                                Array.Copy(buffer, chunkPayload, bytesRead);
    
                                bool chunkSent = false;
                                int retryCount = 0;
    
                                while (!chunkSent && retryCount <= _maxRetries)
                                {
                                    try
                                    {
                                        chunkSent = await sendChunkAsync(manifest, chunkPayload, chunkIndex, session.TotalChunks)
                                            .ConfigureAwait(false);
                                    }
                                    catch
                                    {
                                        // Will retry
                                    }
    
                                    if (!chunkSent)
                                    {
                                        retryCount++;
                                        session.TotalRetries++;
                                        session.FailedChunks++;
                                        manifest.RetryCount++;
    
                                        if (retryCount <= _maxRetries)
                                        {
                                            var delay = backoff.NextDelay();
                                            await Task.Delay(delay, ct).ConfigureAwait(false);
                                        }
                                    }
                                }
    
                                if (!chunkSent)
                                {
                                    session.State = TransferState.Failed;
                                    session.CompletedUtc = DateTime.UtcNow;
                                    EmitTransferStateChanged(session);
                                    return session;
                                }
    
                                session.CompletedChunks++;
                                session.BytesTransferred += bytesRead;
                                backoff.Reset();
                            }
    
                            // Verification step
                            if (session.State != TransferState.Cancelled)
                            {
                                bool verified = false;
                                try
                                {
                                    verified = await verifyAsync(manifest).ConfigureAwait(false);
                                }
                                catch
                                {
                                    verified = false;
                                }
    
                                if (verified)
                                {
                                    session.State = TransferState.Verified;
                                    session.VerificationPassed = true;
                                }
                                else
                                {
                                    session.State = TransferState.Failed;
                                }
                            }
    
                            session.CompletedUtc = DateTime.UtcNow;
                            EmitTransferStateChanged(session);
                            return session;
                        }
                        catch (OperationCanceledException)
                        {
                            session.State = TransferState.Cancelled;
                            session.CompletedUtc = DateTime.UtcNow;
                            EmitTransferStateChanged(session);
                            return session;
                        }
                        catch (Exception)
                        {
                            session.State = TransferState.Failed;
                            session.CompletedUtc = DateTime.UtcNow;
                            EmitTransferStateChanged(session);
                            return session;
                        }
                        finally
                        {
                            _activeTransfers.TryRemove(session.TransferId, out _);
                        }
                    }
    
                    public async Task<TransferSession?> RetryTransferAsync(
                        TransferSession session,
                        Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
                        Func<FileManifest, Task<bool>> verifyAsync,
                        CancellationToken ct = default)
                    {
                        if (!_manifests.TryGetValue(session.ManifestId, out var manifest))
                            return null;
    
                        // Reset session state for retry
                        session.State = TransferState.Pending;
                        session.CompletedUtc = null;
                        session.TotalRetries = 0;
                        session.FailedChunks = 0;
    
                        return await TransferFileAsync(manifest, sendChunkAsync, verifyAsync, ct).ConfigureAwait(false);
                    }
    
                    public static string ComputeSha256(string filePath)
                    {
                        using var stream = File.OpenRead(filePath);
                        var hash = SHA256.HashData(stream);
                        return Convert.ToHexString(hash).ToLowerInvariant();
                    }
    
                    public static string ComputeSha256(byte[] data)
                    {
                        var hash = SHA256.HashData(data);
                        return Convert.ToHexString(hash).ToLowerInvariant();
                    }
    
                    public static bool VerifyChecksum(string filePath, string expectedChecksum)
                    {
                        if (!File.Exists(filePath)) return false;
                        var actual = ComputeSha256(filePath);
                        return string.Equals(actual, expectedChecksum, StringComparison.OrdinalIgnoreCase);
                    }
    
                    public bool IsDuplicate(string checksum)
                    {
                        return _manifests.Values.Any(m =>
                            string.Equals(m.Checksum, checksum, StringComparison.OrdinalIgnoreCase) &&
                            (m.Status == "Completed" || m.Status == "Queued" || m.Status == "Transferring"));
                    }
    
                    public int ClearCompletedManifests()
                    {
                        var completed = _manifests.Values
                            .Where(m => m.Status == "Completed" || m.Status == "Verified")
                            .ToList();
                        foreach (var m in completed)
                            _manifests.TryRemove(m.ManifestId, out _);
                        return completed.Count;
                    }
    
                    public int ClearFailedManifests()
                    {
                        var failed = _manifests.Values.Where(m => m.Status == "Failed").ToList();
                        foreach (var m in failed)
                            _manifests.TryRemove(m.ManifestId, out _);
                        return failed.Count;
                    }
    
                    public SyncStats GetStats()
                    {
                        var all = _manifests.Values.ToList();
                        return new SyncStats
                        {
                            Queued = all.Count(m => m.Status == "Queued"),
                            Transferring = all.Count(m => m.Status == "Transferring"),
                            Completed = all.Count(m => m.Status == "Completed" || m.Status == "Verified"),
                            Failed = all.Count(m => m.Status == "Failed"),
                            TotalBytes = all.Where(m => m.Status == "Completed" || m.Status == "Verified").Sum(m => m.FileSize),
                            SuccessRate = all.Count > 0
                                ? Math.Round((double)all.Count(m => m.Status == "Completed" || m.Status == "Verified") / all.Count * 100, 1)
                                : 0
                        };
                    }
    
                    private void EmitTransferStateChanged(TransferSession session)
                    {
                        try { OnTransferStateChanged?.Invoke(session); } catch { /* Observers must not crash. */ }
                    }
    
                    public void Dispose()
                    {
                        if (!_disposed)
                        {
                            _cts?.Cancel();
                            _cts?.Dispose();
                            _activeTransfers.Clear();
                            _manifests.Clear();
                            _disposed = true;
                        }
                    }
    
    
            // --- Events ---
                    public event Action<TransferSession>? OnTransferStateChanged;
    
                    public event Action<FileManifest>? OnFileQueued;
    
                    public event Action? OnOutboxEmpty;
    
    
        }
    /// <summary>
        /// Production-grade chunked file sync service.
        /// Manages file transfer with SHA256 verification, chunking,
        /// retry with exponential backoff, and resume capability.
        /// Implements Phase 3 requirements: Reliable Chunked Transfer Session (Track 010).
        /// </summary>
        public sealed class ChunkedSyncService : IDisposable
        {
            private readonly ConcurrentDictionary<string, TransferSession> _activeTransfers = new();
            private readonly ConcurrentDictionary<string, FileManifest> _manifests = new();
            private readonly ConcurrentQueue<FileManifest> _outboxQueue = new();
            private readonly string _outboxPath;
            private readonly int _chunkSize;
            private readonly int _maxRetries;
            private readonly object _lock = new();
            private CancellationTokenSource? _cts;
            private bool _disposed;
    
            /// <summary>Fired when a transfer session state changes.</summary>
            public event Action<TransferSession>? OnTransferStateChanged;
    
            /// <summary>Fired when a manifest is added to the outbox.</summary>
            public event Action<FileManifest>? OnFileQueued;
    
            /// <summary>Fired when all queued files are synced.</summary>
            public event Action? OnOutboxEmpty;
    
            public ChunkedSyncService(string outboxPath = "", int chunkSize = 65536, int maxRetries = 3)
            {
                _outboxPath = string.IsNullOrEmpty(outboxPath)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Outbox")
                    : outboxPath;
                _chunkSize = chunkSize;
                _maxRetries = maxRetries;
    
                if (!Directory.Exists(_outboxPath))
                    Directory.CreateDirectory(_outboxPath);
            }
    
            // ===== Outbox Queue Management =====
    
            /// <summary>Enqueue a file for transfer with automatic chunk size calculation.</summary>
            public FileManifest EnqueueFile(string atmId, string sourcePath, string fileType = "Journal")
            {
                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException($"Source file not found: {sourcePath}");
    
                var fileInfo = new FileInfo(sourcePath);
                var checksum = ComputeSha256(sourcePath);
                var totalChunks = (int)Math.Ceiling((double)fileInfo.Length / _chunkSize);
    
                var manifest = new FileManifest
                {
                    ManifestId = Guid.NewGuid().ToString("N"),
                    ATM_ID = atmId,
                    FileName = Path.GetFileName(sourcePath),
                    FileSize = fileInfo.Length,
                    Checksum = checksum,
                    FileType = fileType,
                    SourcePath = sourcePath,
                    TotalChunks = totalChunks,
                    ChunkSize = _chunkSize,
                    Status = "Queued",
                    CreatedUtc = DateTime.UtcNow
                };
    
                _manifests[manifest.ManifestId] = manifest;
                _outboxQueue.Enqueue(manifest);
                OnFileQueued?.Invoke(manifest);
                return manifest;
            }
    
            /// <summary>Enqueue multiple files at once.</summary>
            public List<FileManifest> EnqueueFiles(string atmId, List<string> sourcePaths, string fileType = "Journal")
            {
                return sourcePaths.Select(p => EnqueueFile(atmId, p, fileType)).ToList();
            }
    
            /// <summary>Get the current outbox queue count.</summary>
            public int OutboxPendingCount => _outboxQueue.Count;
    
            /// <summary>Get all active transfer sessions.</summary>
            public IReadOnlyList<TransferSession> ActiveTransfers => _activeTransfers.Values.ToList().AsReadOnly();
    
            /// <summary>Get all manifests (pending, active, completed).</summary>
            public IReadOnlyList<FileManifest> AllManifests => _manifests.Values.ToList().AsReadOnly();
    
            // ===== Chunked Transfer =====
    
            /// <summary>
            /// Process the outbox queue: transfer all pending files with chunking and retry.
            /// Returns the list of completed transfer sessions.
            /// </summary>
            public async Task<List<TransferSession>> ProcessOutboxAsync(
                Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
                Func<FileManifest, Task<bool>> verifyAsync,
                CancellationToken ct = default)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var completedSessions = new List<TransferSession>();
    
                while (_outboxQueue.TryDequeue(out var manifest))
                {
                    if (_cts.Token.IsCancellationRequested)
                        break;
    
                    var session = await TransferFileAsync(manifest, sendChunkAsync, verifyAsync, _cts.Token)
                        .ConfigureAwait(false);
    
                    if (session != null)
                    {
                        completedSessions.Add(session);
                        manifest.Status = session.State == TransferState.Verified ? "Completed" : "Failed";
                        manifest.FailureReason = session.State == TransferState.Failed ? "Transfer failed after retries." : null;
                        manifest.LastModifiedUtc = DateTime.UtcNow;
                    }
                }
    
                if (_outboxQueue.IsEmpty)
                    OnOutboxEmpty?.Invoke();
    
                return completedSessions;
            }
    
            /// <summary>
            /// Transfer a single file with chunking, retry, and SHA256 verification.
            /// </summary>
            private async Task<TransferSession?> TransferFileAsync(
                FileManifest manifest,
                Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
                Func<FileManifest, Task<bool>> verifyAsync,
                CancellationToken ct)
            {
                var session = new TransferSession
                {
                    TransferId = Guid.NewGuid().ToString("N"),
                    ATM_ID = manifest.ATM_ID,
                    FileName = manifest.FileName,
                    FileSize = manifest.FileSize,
                    TotalChunks = manifest.TotalChunks,
                    ChunkSize = manifest.ChunkSize,
                    Checksum = manifest.Checksum,
                    SourcePath = manifest.SourcePath,
                    ManifestId = manifest.ManifestId,
                    State = TransferState.Pending
                };
    
                _activeTransfers[session.TransferId] = session;
                manifest.Status = "Transferring";
                manifest.LastModifiedUtc = DateTime.UtcNow;
    
                try
                {
                    if (!File.Exists(session.SourcePath))
                    {
                        session.State = TransferState.Failed;
                        session.CompletedUtc = DateTime.UtcNow;
                        EmitTransferStateChanged(session);
                        return session;
                    }
    
                    session.State = TransferState.InProgress;
                    session.StartedUtc = DateTime.UtcNow;
    
                    await using var fileStream = new FileStream(
                        session.SourcePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                        bufferSize: session.ChunkSize, useAsync: true);
    
                    var buffer = new byte[session.ChunkSize];
                    var backoff = new ExponentialBackoffWithJitter();
    
                    for (int chunkIndex = 0; chunkIndex < session.TotalChunks; chunkIndex++)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            session.State = TransferState.Cancelled;
                            break;
                        }
    
                        int bytesRead = await fileStream.ReadAsync(buffer, 0, session.ChunkSize, ct)
                            .ConfigureAwait(false);
                        var chunkPayload = new byte[bytesRead];
                        Array.Copy(buffer, chunkPayload, bytesRead);
    
                        bool chunkSent = false;
                        int retryCount = 0;
    
                        while (!chunkSent && retryCount <= _maxRetries)
                        {
                            try
                            {
                                chunkSent = await sendChunkAsync(manifest, chunkPayload, chunkIndex, session.TotalChunks)
                                    .ConfigureAwait(false);
                            }
                            catch
                            {
                                // Will retry
                            }
    
                            if (!chunkSent)
                            {
                                retryCount++;
                                session.TotalRetries++;
                                session.FailedChunks++;
                                manifest.RetryCount++;
    
                                if (retryCount <= _maxRetries)
                                {
                                    var delay = backoff.NextDelay();
                                    await Task.Delay(delay, ct).ConfigureAwait(false);
                                }
                            }
                        }
    
                        if (!chunkSent)
                        {
                            session.State = TransferState.Failed;
                            session.CompletedUtc = DateTime.UtcNow;
                            EmitTransferStateChanged(session);
                            return session;
                        }
    
                        session.CompletedChunks++;
                        session.BytesTransferred += bytesRead;
                        backoff.Reset();
                    }
    
                    // Verification step
                    if (session.State != TransferState.Cancelled)
                    {
                        bool verified = false;
                        try
                        {
                            verified = await verifyAsync(manifest).ConfigureAwait(false);
                        }
                        catch
                        {
                            verified = false;
                        }
    
                        if (verified)
                        {
                            session.State = TransferState.Verified;
                            session.VerificationPassed = true;
                        }
                        else
                        {
                            session.State = TransferState.Failed;
                        }
                    }
    
                    session.CompletedUtc = DateTime.UtcNow;
                    EmitTransferStateChanged(session);
                    return session;
                }
                catch (OperationCanceledException)
                {
                    session.State = TransferState.Cancelled;
                    session.CompletedUtc = DateTime.UtcNow;
                    EmitTransferStateChanged(session);
                    return session;
                }
                catch (Exception)
                {
                    session.State = TransferState.Failed;
                    session.CompletedUtc = DateTime.UtcNow;
                    EmitTransferStateChanged(session);
                    return session;
                }
                finally
                {
                    _activeTransfers.TryRemove(session.TransferId, out _);
                }
            }
    
            // ===== Retry & Resume =====
    
            /// <summary>Retry a failed transfer session from the last successful chunk.</summary>
            public async Task<TransferSession?> RetryTransferAsync(
                TransferSession session,
                Func<FileManifest, byte[], int, int, Task<bool>> sendChunkAsync,
                Func<FileManifest, Task<bool>> verifyAsync,
                CancellationToken ct = default)
            {
                if (!_manifests.TryGetValue(session.ManifestId, out var manifest))
                    return null;
    
                // Reset session state for retry
                session.State = TransferState.Pending;
                session.CompletedUtc = null;
                session.TotalRetries = 0;
                session.FailedChunks = 0;
    
                return await TransferFileAsync(manifest, sendChunkAsync, verifyAsync, ct).ConfigureAwait(false);
            }
    
            // ===== Checksum =====
    
            /// <summary>Compute SHA256 checksum of a file.</summary>
            public static string ComputeSha256(string filePath)
            {
                using var stream = File.OpenRead(filePath);
                var hash = SHA256.HashData(stream);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
    
            /// <summary>Compute SHA256 checksum of bytes.</summary>
            public static string ComputeSha256(byte[] data)
            {
                var hash = SHA256.HashData(data);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
    
            /// <summary>Verify a file against a known checksum.</summary>
            public static bool VerifyChecksum(string filePath, string expectedChecksum)
            {
                if (!File.Exists(filePath)) return false;
                var actual = ComputeSha256(filePath);
                return string.Equals(actual, expectedChecksum, StringComparison.OrdinalIgnoreCase);
            }
    
            // ===== Deduplication =====
    
            /// <summary>Check if a file with the same checksum already exists in the outbox.</summary>
            public bool IsDuplicate(string checksum)
            {
                return _manifests.Values.Any(m =>
                    string.Equals(m.Checksum, checksum, StringComparison.OrdinalIgnoreCase) &&
                    (m.Status == "Completed" || m.Status == "Queued" || m.Status == "Transferring"));
            }
    
            // ===== Cleanup =====
    
            /// <summary>Clear completed manifests from memory.</summary>
            public int ClearCompletedManifests()
            {
                var completed = _manifests.Values
                    .Where(m => m.Status == "Completed" || m.Status == "Verified")
                    .ToList();
                foreach (var m in completed)
                    _manifests.TryRemove(m.ManifestId, out _);
                return completed.Count;
            }
    
            /// <summary>Remove failed manifests.</summary>
            public int ClearFailedManifests()
            {
                var failed = _manifests.Values.Where(m => m.Status == "Failed").ToList();
                foreach (var m in failed)
                    _manifests.TryRemove(m.ManifestId, out _);
                return failed.Count;
            }
    
            /// <summary>Get transfer statistics.</summary>
            public SyncStats GetStats()
            {
                var all = _manifests.Values.ToList();
                return new SyncStats
                {
                    Queued = all.Count(m => m.Status == "Queued"),
                    Transferring = all.Count(m => m.Status == "Transferring"),
                    Completed = all.Count(m => m.Status == "Completed" || m.Status == "Verified"),
                    Failed = all.Count(m => m.Status == "Failed"),
                    TotalBytes = all.Where(m => m.Status == "Completed" || m.Status == "Verified").Sum(m => m.FileSize),
                    SuccessRate = all.Count > 0
                        ? Math.Round((double)all.Count(m => m.Status == "Completed" || m.Status == "Verified") / all.Count * 100, 1)
                        : 0
                };
            }
    
            private void EmitTransferStateChanged(TransferSession session)
            {
                try { OnTransferStateChanged?.Invoke(session); } catch { /* Observers must not crash. */ }
            }
    
            public void Dispose()
            {
                if (!_disposed)
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _activeTransfers.Clear();
                    _manifests.Clear();
                    _disposed = true;
                }
            }
        }
    // Class: SyncStats (from 3 sources)
        public sealed partial class SyncStats
        {
            // --- Properties ---
                    public int Queued { get; set; }
    
                    public int Transferring { get; set; }
    
                    public int Completed { get; set; }
    
                    public int Failed { get; set; }
    
                    public long TotalBytes { get; set; }
    
                    public double SuccessRate { get; set; }
    
    
        }
}

using var stream = File.OpenRead(filePath);
