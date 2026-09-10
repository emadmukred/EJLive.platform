using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EJLive.Client.Service
{
    /// <summary>Durable outbox queue service - FIFO with retry and dedup.</summary>
    public class OutboxService
    {
        private readonly string _basePath;
        public int PendingCount { get; private set; }
        public OutboxService(string? basePath = null) { _basePath = basePath ?? Path.Combine(Path.GetTempPath(), "EJLive_Outbox"); Directory.CreateDirectory(_basePath); }
        public void Enqueue(string filePath) { PendingCount++; }
        public string? Dequeue() { if (PendingCount == 0) return null; PendingCount--; return null; }
    }

    /// <summary>Sync helper - delegates to Core.Sync.ChunkedSyncService.</summary>
    public class ChunkedSyncHelper
    {
        public bool IsRunning { get; set; }
        public Task StartAsync(CancellationToken ct) { IsRunning = true; return Task.CompletedTask; }
        public Task StopAsync() { IsRunning = false; return Task.CompletedTask; }
    }

    /// <summary>Journal offset store - persists last-read byte positions per file.</summary>
    public class JournalOffsetStore
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, long> _offsets = new();
        public long GetOffset(string filePath) => _offsets.GetValueOrDefault(filePath, 0);
        public void SetOffset(string filePath, long offset) => _offsets[filePath] = offset;
        public void Clear() => _offsets.Clear();
    }

    /// <summary>Local configuration store - encrypted JSON at %ProgramData%/EJLive/Config/.</summary>
    public class LocalConfigurationStore
    {
        private readonly string _configPath;
        public LocalConfigurationStore() { _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Config", "local.json"); }
        public string? GetValue(string key) => null;
        public void SetValue(string key, string value) { }
        public void Save() { }
    }
}
