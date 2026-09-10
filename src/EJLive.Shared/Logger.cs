using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace EJLive.Shared
{
    /// <summary>
    /// مسجّل الأحداث المتكامل — Thread-Safe, File+Event Logging
    /// يدعم: Debug / Info / Warning / Error / Critical
    /// يكتب إلى ملف يومي + يُطلق حدثًا للواجهة في نفس الوقت
    /// </summary>
    public sealed class AppLogger : IDisposable
    {
        #region Singleton
        private static AppLogger _instance;
        private static readonly object _initLock = new object();
        public static AppLogger Instance
        {
            get
            {
                if (_instance == null)
                    lock (_initLock)
                        if (_instance == null)
                            _instance = new AppLogger();
                return _instance;
            }
        }
        #endregion

        public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }

        public Level  MinLevel       { get; set; } = Level.Info;
        public string LogDirectory   { get; private set; }
        public string Prefix         { get; set; } = "ejlive";
        public int    MaxFileSizeMB  { get; set; } = 50;
        public int    KeepDays       { get; set; } = 30;

        private StreamWriter _writer;
        private string _currentFile;
        private readonly object _fileLock = new object();
        private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
        private readonly Thread _flushThread;
        private volatile bool _running = true;

        public event EventHandler<LogEntry> OnLog;

        private AppLogger()
        {
            _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
            _flushThread.Start();
        }

        public void Initialize(string logDirectory, string prefix = "ejlive", Level minLevel = Level.Info)
        {
            LogDirectory = logDirectory;
            Prefix       = prefix;
            MinLevel     = minLevel;
            if (!Directory.Exists(logDirectory)) Directory.CreateDirectory(logDirectory);
            OpenWriter();
        }

        public void Debug(string msg, string src = null)    => Log(Level.Debug,    msg, src);
        public void Info(string msg, string src = null)     => Log(Level.Info,     msg, src);
        public void Warning(string msg, string src = null)  => Log(Level.Warning,  msg, src);
        public void Error(string msg, string src = null)    => Log(Level.Error,    msg, src);
        public void Error(Exception ex, string src = null)  => Log(Level.Error,    $"{ex.Message} | {ex.StackTrace}", src);
        public void Critical(string msg, string src = null) => Log(Level.Critical, msg, src);

        public void Log(Level level, string message, string source = null)
        {
            if (level < MinLevel) return;
            var entry = new LogEntry { Timestamp = DateTime.UtcNow, Level = level, Source = source ?? "System", Message = message };
            _buffer.Enqueue(entry);
            OnLog?.Invoke(this, entry);
        }

        private void FlushLoop()
        {
            while (_running)
            {
                try { Thread.Sleep(300); FlushBuffer(); RotateIfNeeded(); } catch { }
            }
        }

        private void FlushBuffer()
        {
            if (_writer == null || _buffer.IsEmpty) return;
            lock (_fileLock)
            {
                while (_buffer.TryDequeue(out var entry))
                    _writer.WriteLine(entry.Format());
                _writer.Flush();
            }
        }

        private void OpenWriter()
        {
            lock (_fileLock)
            {
                _writer?.Dispose();
                _currentFile = Path.Combine(LogDirectory, $"{Prefix}_{DateTime.UtcNow:yyyyMMdd}.log");
                _writer = new StreamWriter(new FileStream(_currentFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), Encoding.UTF8);
            }
        }

        private void RotateIfNeeded()
        {
            if (string.IsNullOrEmpty(_currentFile)) return;
            var expected = Path.Combine(LogDirectory, $"{Prefix}_{DateTime.UtcNow:yyyyMMdd}.log");
            bool rotate  = _currentFile != expected;
            if (!rotate && File.Exists(_currentFile) && new FileInfo(_currentFile).Length > MaxFileSizeMB * 1024L * 1024L) rotate = true;
            if (rotate) OpenWriter();
        }

        public void Dispose()
        {
            _running = false;
            Thread.Sleep(400);
            FlushBuffer();
            lock (_fileLock) _writer?.Dispose();
        }
    }

    public class LogEntry
    {
        public DateTime        Timestamp { get; set; }
        public AppLogger.Level Level     { get; set; }
        public string          Source    { get; set; }
        public string          Message   { get; set; }

        public string Format() =>
            $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{LevelTag,-3}] [{Source,-22}] {Message}";

        public string FormattedForUI =>
            $"[{Timestamp.ToLocalTime():HH:mm:ss}] [{LevelTag}] {Message}";

        private string LevelTag => Level switch
        {
            AppLogger.Level.Debug    => "DBG",
            AppLogger.Level.Info     => "INF",
            AppLogger.Level.Warning  => "WRN",
            AppLogger.Level.Error    => "ERR",
            AppLogger.Level.Critical => "CRT",
            _                        => "---"
        };
    }
}
