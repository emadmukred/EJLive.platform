using System.Collections.Concurrent;
using System.Text;

namespace EJLive.Shared;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public sealed class LogEntryEventArgs : EventArgs
{
    public LogEntryEventArgs(DateTimeOffset timestamp, LogLevel level, string source, string message)
    {
        Timestamp = timestamp;
        Level = level;
        Source = source;
        Message = message;
    }

    public DateTimeOffset Timestamp { get; }
    public LogLevel Level { get; }
    public string Source { get; }
    public string Message { get; }
    public string FormattedForUI => $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] [{Source}] {Message}";
}

public sealed class AppLogger : IDisposable
{
    private const long MaxLogFileBytes = 10 * 1024 * 1024;
    private readonly object _sync = new();
    private readonly ConcurrentQueue<string> _pendingLines = new();
    private readonly System.Threading.Timer _flushTimer;
    private string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Logs");
    private string _component = "app";
    private bool _initialized;
    private bool _disposed;
    private int _flushInProgress;

    public static AppLogger Instance { get; } = new();

    public event EventHandler<LogEntryEventArgs>? OnLog;

    private AppLogger()
    {
        _flushTimer = new System.Threading.Timer(
            static state => ((AppLogger)state!).Flush(),
            this,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1));
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Flush();
    }

    public void Initialize(string logDirectory, string component)
    {
        lock (_sync)
        {
            _logDirectory = string.IsNullOrWhiteSpace(logDirectory) ? _logDirectory : logDirectory;
            _component = string.IsNullOrWhiteSpace(component) ? "app" : component;
            Directory.CreateDirectory(_logDirectory);
            _initialized = true;
        }
    }

    public void Debug(string message, string source = "General") => Write(LogLevel.Debug, source, message);
    public void Info(string message, string source = "General") => Write(LogLevel.Info, source, message);
    public void Warning(string message, string source = "General") => Write(LogLevel.Warning, source, message);
    public void Error(string message, string source = "General") => Write(LogLevel.Error, source, message);
    public void Error(Exception exception, string source = "General")
    {
        ArgumentNullException.ThrowIfNull(exception);
        var detail = $"{exception.GetType().Name}: {exception.Message}";
        if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            detail += Environment.NewLine + exception.StackTrace;
        Write(LogLevel.Error, source, detail);
    }
    public void Critical(string message, string source = "General") => Write(LogLevel.Critical, source, message);

    public void Write(LogLevel level, string source, string message)
    {
        if (_disposed)
            return;

        var entry = new LogEntryEventArgs(DateTimeOffset.Now, level, source, message);
        OnLog?.Invoke(this, entry);

        try
        {
            lock (_sync)
            {
                if (!_initialized)
                    Initialize(_logDirectory, _component);

            }

            _pendingLines.Enqueue(entry.FormattedForUI);
        }
        catch
        {
            // Logging must not interrupt ATM operations.
        }
    }

    public void Flush()
    {
        if (Interlocked.Exchange(ref _flushInProgress, 1) != 0)
            return;

        try
        {
            var lines = new List<string>();
            while (_pendingLines.TryDequeue(out var line))
                lines.Add(line);

            if (lines.Count == 0)
                return;

            lock (_sync)
            {
                Directory.CreateDirectory(_logDirectory);
                var filePath = Path.Combine(_logDirectory, $"{_component}-{DateTime.Today:yyyyMMdd}.log");
                RotateIfRequired(filePath, lines);

                using var writer = new StreamWriter(filePath, append: true, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                foreach (var line in lines)
                    writer.WriteLine(line);
            }
        }
        catch
        {
            // Logging must remain best-effort and must never interrupt ATM operations.
        }
        finally
        {
            Volatile.Write(ref _flushInProgress, 0);
        }
    }

    private static void RotateIfRequired(string filePath, IReadOnlyCollection<string> pendingLines)
    {
        if (!File.Exists(filePath))
            return;

        var pendingBytes = pendingLines.Sum(line => Encoding.UTF8.GetByteCount(line) + Environment.NewLine.Length);
        if (new FileInfo(filePath).Length + pendingBytes <= MaxLogFileBytes)
            return;

        var directory = Path.GetDirectoryName(filePath) ?? AppContext.BaseDirectory;
        var stem = Path.GetFileNameWithoutExtension(filePath);
        var rotatedPath = Path.Combine(directory, $"{stem}-{DateTime.UtcNow:HHmmssfff}.log");
        File.Move(filePath, rotatedPath, overwrite: false);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _flushTimer.Change(Timeout.Infinite, Timeout.Infinite);
        Flush();
        _disposed = true;
        _flushTimer.Dispose();
    }
}
