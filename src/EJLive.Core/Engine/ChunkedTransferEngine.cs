using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Resumable chunked file transfer with per-chunk SHA256 verification and bitmap tracking.
    /// </summary>
    public sealed class ChunkedTransferEngine : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, TransferSession> _sessions = new();
        private readonly NetworkEngine _network;
        private readonly int _defaultChunkSize;
        private readonly CancellationTokenSource _cts = new();

        public event Action<TransferSession>? OnTransferCompleted;
        public event Action<TransferSession>? OnTransferFailed;
        public event Action<string>? OnLog;

        public ChunkedTransferEngine(NetworkEngine network, int defaultChunkSize = 65536)
        {
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _defaultChunkSize = defaultChunkSize;
        }

        public TransferSession BeginTransfer(string atmId, string filePath, string? expectedSha256 = null)
        {
            var info = new FileInfo(filePath);
            if (!info.Exists)
                throw new FileNotFoundException("Source file not found.", filePath);

            var session = new TransferSession
            {
                AtmId = atmId,
                FileName = Path.GetFileName(filePath),
                Length = info.Length,
                Offset = 0,
                ChunkSize = _defaultChunkSize,
                ExpectedSha256 = expectedSha256,
                State = TransferState.InProgress
            };

            _sessions[session.TransferId] = session;
            Log($"Transfer begun: {session.TransferId} for {session.FileName} ({session.Length} bytes, {session.TotalChunks} chunks)");
            return session;
        }

        public async Task RunTransferAsync(Guid transferId, string sourceFilePath, CancellationToken token)
        {
            if (!_sessions.TryGetValue(transferId, out var session))
                throw new InvalidOperationException("Transfer session not found.");

            using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cts.Token);
            var ct = linked.Token;

            try
            {
                using var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var buffer = ArrayPool<byte>.Shared.Rent(session.ChunkSize);
                try
                {
                    for (int seq = 0; seq < session.TotalChunks && !ct.IsCancellationRequested; seq++)
                    {
                        if (session.ReceivedChunks.Contains(seq))
                            continue; // skip already-acknowledged chunks on resume

                        int read = await stream.ReadAsync(buffer.AsMemory(0, session.ChunkSize), ct);
                        if (read <= 0)
                            break;

                        var chunkData = new byte[read];
                        Buffer.BlockCopy(buffer, 0, chunkData, 0, read);
                        var chunkHash = ComputeHash(chunkData);

                        var payload = new ChunkPayload(transferId, seq, chunkData, stream.Position - read, read, chunkHash);
                        bool acked = await SendChunkWithRetryAsync(payload, ct);

                        if (!acked)
                        {
                            session.State = TransferState.Failed;
                            session.LastError = "Chunk ACK timeout after retries.";
                            OnTransferFailed?.Invoke(session);
                            return;
                        }

                        session.ReceivedChunks.Add(seq);
                        session.Offset = stream.Position;
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }

                // Finalize
                var computedSha = await ComputeFileHashAsync(sourceFilePath, ct);
                session.ComputedSha256 = computedSha;
                session.State = TransferState.Completed;
                session.CompletedUtc = DateTime.UtcNow;

                var complete = new TransferComplete(transferId, session.FileName, computedSha, session.Length, DateTime.UtcNow);
                _network.SendTransferComplete(complete);

                OnTransferCompleted?.Invoke(session);
                Log($"Transfer completed: {transferId} SHA256={computedSha}");
            }
            catch (Exception ex)
            {
                session.State = TransferState.Failed;
                session.LastError = ex.Message;
                OnTransferFailed?.Invoke(session);
                Log($"Transfer failed: {transferId} => {ex.Message}");
            }
        }

        public TransferSession? GetSession(Guid transferId)
        {
            _sessions.TryGetValue(transferId, out var session);
            return session;
        }

        private async Task<bool> SendChunkWithRetryAsync(ChunkPayload payload, CancellationToken ct, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                var tcs = new TaskCompletionSource<ChunkAck>(TaskCreationOptions.RunContinuationsAsynchronously);
                void Handler(object? s, ChunkAck ack) { if (ack.TransferId == payload.TransferId && ack.Sequence == payload.Sequence) tcs.TrySetResult(ack); }
                _network.OnChunkAck += Handler;
                try
                {
                    _network.SendChunk(payload);
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    using var reg = timeout.Token.Register(() => tcs.TrySetCanceled());
                    var ack = await tcs.Task;
                    return ack.Ok;
                }
                catch (OperationCanceledException)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), ct);
                }
                finally
                {
                    _network.OnChunkAck -= Handler;
                }
            }
            return false;
        }

        private static string ComputeHash(byte[] data)
        {
            return Convert.ToHexString(SHA256.HashData(data));
        }

        private static async Task<string> ComputeFileHashAsync(string path, CancellationToken ct)
        {
            using var sha = SHA256.Create();
            await using var stream = File.OpenRead(path);
            var hash = await sha.ComputeHashAsync(stream, ct);
            return Convert.ToHexString(hash);
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.UtcNow:O}] [ChunkedTransfer] {message}");
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
