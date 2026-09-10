using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public enum TransferState
    {
        Pending,
        InProgress,
        Paused,
        Completed,
        Failed,
        DeadLetter
    }

    public sealed class TransferSession
    {
        public Guid TransferId { get; init; } = Guid.NewGuid();
        public string AtmId { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public long Length { get; init; }
        public long Offset { get; set; }
        public int ChunkSize { get; init; } = 65536;
        public int TotalChunks => (int)Math.Ceiling((double)Length / ChunkSize);
        public HashSet<int> ReceivedChunks { get; init; } = new();
        public string? ExpectedSha256 { get; init; }
        public string? ComputedSha256 { get; set; }
        public TransferState State { get; set; } = TransferState.Pending;
        public int RetryCount { get; set; }
        public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;
        public DateTime? CompletedUtc { get; set; }
        public string? LastError { get; set; }

        public bool IsComplete => ReceivedChunks.Count >= TotalChunks && TotalChunks > 0;
    }

    public sealed record ChunkPayload(
        Guid TransferId,
        int Sequence,
        byte[] Data,
        long Offset,
        int Length,
        string ChunkHash);

    public sealed record ChunkAck(
        Guid TransferId,
        int Sequence,
        bool Ok,
        long NextExpectedOffset,
        string? ServerHash);

    public sealed record TransferComplete(
        Guid TransferId,
        string FileName,
        string Sha256,
        long Length,
        DateTime TimestampUtc);
}
