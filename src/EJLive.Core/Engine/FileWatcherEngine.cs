using System.Collections.Concurrent;

namespace EJLive.Core.Engine;

/// <summary>Watches journal directories with polling fallback and duplicate event suppression.</summary>
public sealed class FileWatcherEngine : IDisposable
{
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly List<string> _paths = new();
    private readonly ConcurrentDictionary<string, FileSnapshot> _lastSeen = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lifecycleLock = new();
    private System.Threading.Timer? _pollTimer;
    private bool _disposed;

    public event EventHandler<string>? FileChanged;
    public event EventHandler<string>? WatchError;
    public bool IsRunning { get; private set; }
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(10);

    public void Start(params string[] paths)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(paths);
        if (PollInterval <= TimeSpan.Zero)
            throw new InvalidOperationException("Polling interval must be positive.");

        lock (_lifecycleLock)
        {
            StopCore();
            foreach (var path in paths
                         .Where(path => !string.IsNullOrWhiteSpace(path))
                         .Select(Path.GetFullPath)
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(path))
                {
                    RaiseError($"Watch directory does not exist: {path}");
                    continue;
                }

                var watcher = new FileSystemWatcher(path)
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                    Filter = "*.*",
                    EnableRaisingEvents = true
                };
                watcher.Created += OnFileEvent;
                watcher.Changed += OnFileEvent;
                watcher.Renamed += OnFileRenamed;
                watcher.Error += OnWatcherError;
                _paths.Add(path);
                _watchers.Add(watcher);
            }

            IsRunning = _watchers.Count > 0;
            if (IsRunning)
                _pollTimer = new System.Threading.Timer(_ => PollWatchedPaths(), null, TimeSpan.FromSeconds(1), PollInterval);
        }
    }

    public void Stop()
    {
        lock (_lifecycleLock)
            StopCore();
    }

    private void StopCore()
    {
        IsRunning = false;
        _pollTimer?.Dispose();
        _pollTimer = null;
        foreach (var watcher in _watchers)
            watcher.Dispose();
        _watchers.Clear();
        _paths.Clear();
        _lastSeen.Clear();
    }

    private void OnFileEvent(object sender, FileSystemEventArgs args) => EmitIfChanged(args.FullPath);
    private void OnFileRenamed(object sender, RenamedEventArgs args) => EmitIfChanged(args.FullPath);
    private void OnWatcherError(object sender, ErrorEventArgs args) => RaiseError(args.GetException().Message);

    private void PollWatchedPaths()
    {
        if (!IsRunning || _disposed)
            return;

        foreach (var path in _paths.ToArray())
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(path))
                    EmitIfChanged(file);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
            {
                RaiseError($"Polling failed for '{path}': {exception.Message}");
            }
        }
    }

    private void EmitIfChanged(string filePath)
    {
        if (!IsRunning || string.IsNullOrWhiteSpace(filePath))
            return;

        FileSnapshot snapshot;
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(filePath);
            var info = new FileInfo(fullPath);
            if (!info.Exists)
                return;
            snapshot = new FileSnapshot(info.Length, info.LastWriteTimeUtc);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            RaiseError($"Unable to inspect '{filePath}': {exception.Message}");
            return;
        }

        if (_lastSeen.TryGetValue(fullPath, out var previous) && previous.Equals(snapshot))
            return;
        _lastSeen[fullPath] = snapshot;
        RaiseFileChanged(fullPath);
    }

    private void RaiseFileChanged(string path)
    {
        var handlers = FileChanged;
        if (handlers is null)
            return;
        foreach (EventHandler<string> handler in handlers.GetInvocationList())
        {
            try { handler(this, path); }
            catch (Exception exception) { RaiseError($"FileChanged subscriber failed: {exception.Message}"); }
        }
    }

    private void RaiseError(string detail)
    {
        var handlers = WatchError;
        if (handlers is null)
            return;
        foreach (EventHandler<string> handler in handlers.GetInvocationList())
        {
            try { handler(this, detail); }
            catch { /* Error observers must not terminate the watcher. */ }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        Stop();
        _disposed = true;
    }

    private readonly record struct FileSnapshot(long Length, DateTime LastWriteUtc);
}
