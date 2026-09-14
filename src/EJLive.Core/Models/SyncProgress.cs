using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Tracks the progress of a synchronization operation including
    /// computed transfer speed and estimated time remaining.
    /// </summary>
    /// <remarks>
    /// Wave 1 (SS-04 D-08): rewrote the file. The previous version had three copies of
    /// <c>SyncProgress</c> interleaved with provenance comments, plus orphan expressions
    /// (the <c>if (ProgressPercent &lt;= 0) return null;</c> statements the merge tool
    /// pulled out of the enclosing <c>EstimatedRemaining { get; }</c> block). The body
    /// is identical across copies, so the union collapses to the canonical declaration.
    /// The <c>FormatBytes</c> helper stays <c>private</c>; it is only consumed by the
    /// <c>TransferSpeed</c> getter.
    /// </remarks>
    public class SyncProgress
    {
        public string DeviceId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public SyncStatus Status { get; set; } = SyncStatus.Idle;
        public long TotalBytes { get; set; } = 0;
        public long TransferredBytes { get; set; } = 0;
        public int TotalChunks { get; set; } = 0;
        public int CurrentChunk { get; set; } = 0;
        public double ProgressPercent => TotalBytes > 0 ? (TransferredBytes * 100.0 / TotalBytes) : 0;
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public TimeSpan Elapsed => DateTime.UtcNow - StartTime;

        public TimeSpan? EstimatedRemaining
        {
            get
            {
                if (ProgressPercent <= 0) return null;
                var elapsedMs = Elapsed.TotalMilliseconds;
                var totalMs = elapsedMs * 100.0 / ProgressPercent;
                return TimeSpan.FromMilliseconds(totalMs - elapsedMs);
            }
        }

        public string TransferSpeed
        {
            get
            {
                var seconds = Elapsed.TotalSeconds;
                if (seconds <= 0 || TransferredBytes <= 0) return "0 B/s";
                var bytesPerSec = TransferredBytes / seconds;
                return FormatBytes((long)bytesPerSec) + "/s";
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.00} {sizes[order]}";
        }
    }
}
