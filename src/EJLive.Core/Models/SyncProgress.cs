using EJLive.Core.Enums;

namespace EJLive.Core.Models
{
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
    public partial class SyncProgress
        {
            public TimeSpan? EstimatedRemaining
            if (ProgressPercent <= 0) return null;
    
    
            var elapsedMs = Elapsed.TotalMilliseconds;
    
    
            var totalMs = elapsedMs * 100.0 / ProgressPercent;
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\SyncProgress.cs
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\SyncProgress.cs
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
    
    
            return TimeSpan.FromMilliseconds(totalMs - elapsedMs);
    
    
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
    /// <summary>
    /// Tracks the progress of a synchronization operation including
    /// computed transfer speed and estimated time remaining.
    /// </summary>
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

    // Class: SyncProgress (from 3 sources)
        public partial class SyncProgress
        {
            // --- Constants & Fields ---
            public TimeSpan? EstimatedRemaining
            if (ProgressPercent <= 0) return null;
    
            var elapsedMs = Elapsed.TotalMilliseconds;
    
            var totalMs = elapsedMs * 100.0 / ProgressPercent;
    
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\SyncProgress.cs
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
    
    
            // --- Properties ---
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
    
    
            // --- Methods ---
            return TimeSpan.FromMilliseconds(totalMs - elapsedMs);
    
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
