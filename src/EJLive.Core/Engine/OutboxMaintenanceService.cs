using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Periodic maintenance for the journal outbox: enforces quota, cleans orphans, and dead-letters exhausted items.
    /// </summary>
    public sealed class OutboxMaintenanceService : IDisposable
    {
        private readonly JournalOutbox _outbox;
        private readonly string _outboxPath;
        private readonly long _maxBytes;
        private readonly int _maxItems;
        private readonly TimeSpan _gracePeriod;
        private readonly int _maxRetries;
        private readonly System.Timers.Timer _timer;
        private readonly CancellationTokenSource _cts = new();

        public event Action<OutboxTelemetry>? OnTelemetry;
        public event Action<string>? OnLog;

        public OutboxMaintenanceService(
            JournalOutbox outbox,
            string outboxPath,
            long maxBytes = 500 * 1024 * 1024, // 500 MB
            int maxItems = 10000,
            TimeSpan? gracePeriod = null,
            int maxRetries = 5,
            double intervalMinutes = 10)
        {
            _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
            _outboxPath = outboxPath ?? throw new ArgumentNullException(nameof(outboxPath));
            _maxBytes = maxBytes;
            _maxItems = maxItems;
            _gracePeriod = gracePeriod ?? TimeSpan.FromHours(24);
            _maxRetries = maxRetries;
            _timer = new System.Timers.Timer(TimeSpan.FromMinutes(intervalMinutes).TotalMilliseconds) { AutoReset = true };
            _timer.Elapsed += (_, _) => _ = Task.Run(RunMaintenanceAsync);
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        public async Task RunMaintenanceAsync()
        {
            try
            {
                Log("Outbox maintenance starting.");
                var telemetry = new OutboxTelemetry();

                // 1. Evaluate size and item count
                var dir = new DirectoryInfo(_outboxPath);
                if (dir.Exists)
                {
                    var files = dir.GetFiles("*", SearchOption.AllDirectories);
                    long totalSize = files.Sum(f => f.Length);
                    int totalCount = files.Length;

                    telemetry.TotalBytes = totalSize;
                    telemetry.TotalItems = totalCount;

                    if (totalSize > _maxBytes || totalCount > _maxItems)
                    {
                        Log($"Quota exceeded: {totalSize} bytes, {totalCount} items. Purging old items.");
                        PurgeOldestFiles(files, totalSize - _maxBytes, totalCount - _maxItems);
                    }
                }

                // 2. Dead-letter exhausted retries
                int deadLettered = PromoteExhaustedToDeadLetter();
                telemetry.DeadLettered = deadLettered;

                // 3. Orphan payload cleanup
                int orphansRemoved = RemoveOrphanPayloads();
                telemetry.OrphansRemoved = orphansRemoved;

                OnTelemetry?.Invoke(telemetry);
                Log("Outbox maintenance completed.");
            }
            catch (Exception ex)
            {
                Log($"Maintenance error: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        private void PurgeOldestFiles(FileInfo[] files, long bytesToFree, int itemsToFree)
        {
            var ordered = files.OrderBy(f => f.LastWriteTimeUtc).ToList();
            long freed = 0;
            int removed = 0;
            foreach (var f in ordered)
            {
                if (freed >= bytesToFree && removed >= itemsToFree)
                    break;
                try
                {
                    freed += f.Length;
                    f.Delete();
                    removed++;
                }
                catch { /* best effort */ }
            }
            Log($"Purged {removed} files ({freed} bytes).");
        }

        private int PromoteExhaustedToDeadLetter()
        {
            // Integration point: JournalOutbox should expose retry metadata.
            // This is a placeholder for the promotion logic.
            return 0;
        }

        private int RemoveOrphanPayloads()
        {
            // Integration point: compare filesystem payloads against DB outbox rows.
            return 0;
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.UtcNow:O}] [OutboxMaint] {message}");
        }

        public void Dispose()
        {
            Stop();
            _timer?.Dispose();
            _cts?.Dispose();
        }
    }

    public sealed record OutboxTelemetry
    {
        public long TotalBytes { get; set; }
        public int TotalItems { get; set; }
        public int Pending { get; set; }
        public int Failed { get; set; }
        public int AwaitingAck { get; set; }
        public int DeadLettered { get; set; }
        public int OrphansRemoved { get; set; }
    }
}
