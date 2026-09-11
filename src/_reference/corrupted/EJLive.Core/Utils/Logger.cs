using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using EJLive.Core.Enums;

namespace EJLive.Core.Utils
{
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
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
            private StreamWriter _writer;
            private string _currentFile;
            private readonly Thread _flushThread;
            private volatile bool _running = true;
            public Level  MinLevel       { get; set; } = Level.Info;
            public string LogDirectory   { get; private set; }
            public string Prefix         { get; set; } = "ejlive";
            public int    MaxFileSizeMB  { get; set; } = 50;
            public int    KeepDays       { get; set; } = 30;
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
            private static readonly object _initLock = new object();
            private readonly object _fileLock = new object();
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
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
            public event EventHandler<LogEntry> OnLog;
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
        }
    public partial class LogEntry
        {
            public DateTime        Timestamp { get; set; }
            public AppLogger.Level Level     { get; set; }
            public string          Source    { get; set; }
            public string          Message   { get; set; }
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
            public string Format() =>
                $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{LevelTag,-3}] [{Source,-22}] {Message}";
        }

    /// <summary>
    /// Structured logger with log level filtering and optional file output.
    /// Provides a unified logging interface across Core services.
    /// </summary>
    public class Logger
    {
        private readonly string _category;
        private readonly LogLevel _minimumLevel;
        private readonly string? _logFilePath;
        private static readonly object _fileLock = new();
    
        /// <summary>
        /// Creates a logger for the specified category.
        /// </summary>
        /// <param name="category">Category name for log entries (e.g., service name).</param>
        /// <param name="minimumLevel">Minimum log level to output.</param>
        /// <param name="logFilePath">Optional file path for persistent logging.</param>
        public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
        {
            _category = category;
            _minimumLevel = minimumLevel;
            _logFilePath = logFilePath;
        }
    
        /// <summary>Logs a debug-level message.</summary>
        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }
    
        /// <summary>Logs an info-level message.</summary>
        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }
    
        /// <summary>Logs a warning-level message.</summary>
        public void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }
    
        /// <summary>Logs an error-level message with optional exception details.</summary>
        public void Error(string message, Exception? ex = null)
        {
            var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
            Log(LogLevel.Error, fullMessage);
        }
    
        /// <summary>Logs a fatal-level message with optional exception details.</summary>
        public void Fatal(string message, Exception? ex = null)
        {
            var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
            Log(LogLevel.Fatal, fullMessage);
        }
    
        private void Log(LogLevel level, string message)
        {
            if (level < _minimumLevel) return;
    
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{level}] [{_category}] {message}";
    
            // Output to debug console
            System.Diagnostics.Debug.WriteLine(logEntry);
    
            // Optionally write to file
            if (!string.IsNullOrEmpty(_logFilePath))
            {
                lock (_fileLock)
                {
                    try
                    {
                        File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                    }
                    catch
                    {
                        // Silently fail if file logging is unavailable
                    }
                }
            }
        }
    
        // Static convenience methods for quick logging without instantiating a Logger.
        // These use default settings and are suitable for simple service-level logging.
    
        /// <summary>Static convenience: logs a debug message with category.</summary>
        public static void Debug(string message, string category)
        {
            System.Diagnostics.Debug.WriteLine($"[{LogLevel.Debug}] [{category}] {message}");
        }
    
        /// <summary>Static convenience: logs an info message with category.</summary>
        public static void Info(string message, string category)
        {
            System.Diagnostics.Debug.WriteLine($"[{LogLevel.Info}] [{category}] {message}");
        }
    
        /// <summary>Static convenience: logs a warning message with category.</summary>
        public static void Warning(string message, string category)
        {
            System.Diagnostics.Debug.WriteLine($"[{LogLevel.Warning}] [{category}] {message}");
        }
    
        /// <summary>Static convenience: logs an error message with category and optional exception.</summary>
        public static void Error(string message, string category, Exception? ex = null)
        {
            var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
            System.Diagnostics.Debug.WriteLine($"[{LogLevel.Error}] [{category}] {fullMessage}");
        }
    
        /// <summary>Static convenience: logs a fatal message with category and optional exception.</summary>
        public static void Fatal(string message, string category, Exception? ex = null)
        {
            var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
            System.Diagnostics.Debug.WriteLine($"[{LogLevel.Fatal}] [{category}] {fullMessage}");
        }
    }
    // Enum: Level (from 3 sources)
        public partial enum Level
        {
        }
    // Class: LogEntry (from 3 sources)
        public partial class LogEntry
        {
        }
    public partial public public static class Logger
        {
            private static readonly object _lock = new();
            private static string _logPath = @"C:\EJLive_Storage\Logs";
            private static LogLevel _minLevel = LogLevel.Debug;
            private readonly string _category;
            private readonly LogLevel _minimumLevel;
            private static readonly object _fileLock = new();
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
            public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
            {
            public static void Debug(string message, string source = "")
            {
            public static void Info(string message, string source = "")
            {
            public static void Warning(string message, string source = "", Exception? ex = null)
            {
            public static void Error(string message, string source = "", Exception? ex = null)
            {
            public static void Fatal(string message, string source = "", Exception? ex = null)
            {
            private static void Log(LogLevel level, string message, string source, Exception? ex = null)
            {
            public void Debug(string message)
            {
            public void Info(string message)
            {
            public void Warning(string message)
            {
            public void Error(string message, Exception? ex = null)
            {
            public void Fatal(string message, Exception? ex = null)
            {
            private void Log(LogLevel level, string message)
            {
            public static void Debug(string message, string category)
            {
            public static void Info(string message, string category)
            {
            public static void Warning(string message, string category)
            {
            public static void Error(string message, string category, Exception? ex = null)
            {
            public static void Fatal(string message, string category, Exception? ex = null)
            {
        }
    
    }
    public partial public static class Logger
        {
            private static readonly object _lock = new();
            private static string _logPath = @"C:\EJLive_Storage\Logs";
            private static LogLevel _minLevel = LogLevel.Debug;
            private readonly string _category;
            private readonly LogLevel _minimumLevel;
            private static readonly object _fileLock = new();
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
            public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
            {
            public static void Debug(string message, string source = "")
            {
            public static void Info(string message, string source = "")
            {
            public static void Warning(string message, string source = "", Exception? ex = null)
            {
            public static void Error(string message, string source = "", Exception? ex = null)
            {
            public static void Fatal(string message, string source = "", Exception? ex = null)
            {
            private static void Log(LogLevel level, string message, string source, Exception? ex = null)
            {
            public void Debug(string message)
            {
            public void Info(string message)
            {
            public void Warning(string message)
            {
            public void Error(string message, Exception? ex = null)
            {
            public void Fatal(string message, Exception? ex = null)
            {
            private void Log(LogLevel level, string message)
            {
            public static void Debug(string message, string category)
            {
            public static void Info(string message, string category)
            {
            public static void Warning(string message, string category)
            {
            public static void Error(string message, string category, Exception? ex = null)
            {
            public static void Fatal(string message, string category, Exception? ex = null)
            {
        }
    
    }
    public partial class Logger
        {
            private readonly string _category;
    
    
            private readonly LogLevel _minimumLevel;
    
    
            private readonly string? _logFilePath;
    
    
            private static readonly object _fileLock = new();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Utils\Logger.cs
            _minimumLevel = minimumLevel;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Utils\Logger.cs
            _logFilePath = logFilePath;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.v22_bak
            _minimumLevel = minimumLevel;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.v22_bak
            _logFilePath = logFilePath;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.v17_bak
            _minimumLevel = minimumLevel;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.v17_bak
            _logFilePath = logFilePath;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.before_unify
            _minimumLevel = minimumLevel;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.before_unify
            _logFilePath = logFilePath;
    
    
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Utils\Logger.cs
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
                _category = category;
                _minimumLevel = minimumLevel;
                _logFilePath = logFilePath;
            }
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
                _category = category;
                _minimumLevel = minimumLevel;
                _logFilePath = logFilePath;
            }
    
    
            public void Debug(string message)
            {
                Log(LogLevel.Debug, message);
            }
    
    
            public void Info(string message)
            {
                Log(LogLevel.Info, message);
            }
    
    
            public void Warning(string message)
            {
                Log(LogLevel.Warning, message);
            }
    
    
            public void Error(string message, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                Log(LogLevel.Error, fullMessage);
            }
    
    
            public void Fatal(string message, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                Log(LogLevel.Fatal, fullMessage);
            }
    
    
            private void Log(LogLevel level, string message)
            {
                if (level < _minimumLevel) return;
    
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] [{_category}] {message}";
    
                // Output to debug console
                System.Diagnostics.Debug.WriteLine(logEntry);
    
                // Optionally write to file
                if (!string.IsNullOrEmpty(_logFilePath))
                {
                    lock (_fileLock)
                    {
                        try
                        {
                            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                        }
                        catch
                        {
                            // Silently fail if file logging is unavailable
                        }
                    }
                }
            }
    
    
            public static void Debug(string message, string category)
            {
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Debug}] [{category}] {message}");
            }
    
    
            public static void Info(string message, string category)
            {
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Info}] [{category}] {message}");
            }
    
    
            public static void Warning(string message, string category)
            {
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Warning}] [{category}] {message}");
            }
    
    
            public static void Error(string message, string category, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Error}] [{category}] {fullMessage}");
            }
    
    
            public static void Fatal(string message, string category, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Fatal}] [{category}] {fullMessage}");
            }
    
    
        }
    // Class: Logger (from 4 sources)
        public partial class Logger
        {
            // --- Constants & Fields ---
            private readonly string _category;
    
            private readonly LogLevel _minimumLevel;
    
            private readonly string? _logFilePath;
    
            private static readonly object _fileLock = new();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.v22_bak
            _minimumLevel = minimumLevel;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.v22_bak
            _logFilePath = logFilePath;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.v17_bak
            _minimumLevel = minimumLevel;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.v17_bak
            _logFilePath = logFilePath;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.before_unify
            _minimumLevel = minimumLevel;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs.before_unify
            _logFilePath = logFilePath;
    
    
            // --- Constructors ---
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Utils\Logger.cs
                public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
                {
                    _category = category;
                    _minimumLevel = minimumLevel;
                    _logFilePath = logFilePath;
                }
    
    
            // --- Methods ---
                public void Debug(string message)
                {
                    Log(LogLevel.Debug, message);
                }
    
                public void Info(string message)
                {
                    Log(LogLevel.Info, message);
                }
    
                public void Warning(string message)
                {
                    Log(LogLevel.Warning, message);
                }
    
                public void Error(string message, Exception? ex = null)
                {
                    var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                    Log(LogLevel.Error, fullMessage);
                }
    
                public void Fatal(string message, Exception? ex = null)
                {
                    var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                    Log(LogLevel.Fatal, fullMessage);
                }
    
                private void Log(LogLevel level, string message)
                {
                    if (level < _minimumLevel) return;
    
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] [{level}] [{_category}] {message}";
    
                    // Output to debug console
                    System.Diagnostics.Debug.WriteLine(logEntry);
    
                    // Optionally write to file
                    if (!string.IsNullOrEmpty(_logFilePath))
                    {
                        lock (_fileLock)
                        {
                            try
                            {
                                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                            }
                            catch
                            {
                                // Silently fail if file logging is unavailable
                            }
                        }
                    }
                }
    
                public static void Debug(string message, string category)
                {
                    System.Diagnostics.Debug.WriteLine($"[{LogLevel.Debug}] [{category}] {message}");
                }
    
                public static void Info(string message, string category)
                {
                    System.Diagnostics.Debug.WriteLine($"[{LogLevel.Info}] [{category}] {message}");
                }
    
                public static void Warning(string message, string category)
                {
                    System.Diagnostics.Debug.WriteLine($"[{LogLevel.Warning}] [{category}] {message}");
                }
    
                public static void Error(string message, string category, Exception? ex = null)
                {
                    var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                    System.Diagnostics.Debug.WriteLine($"[{LogLevel.Error}] [{category}] {fullMessage}");
                }
    
                public static void Fatal(string message, string category, Exception? ex = null)
                {
                    var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                    System.Diagnostics.Debug.WriteLine($"[{LogLevel.Fatal}] [{category}] {fullMessage}");
                }
    
    
        }
    public static class Logger
    {
        private static readonly object _lock = new();
        private static string _logPath = @"C:\EJLive_Storage\Logs";
        private static LogLevel _minLevel = LogLevel.Debug;
    
        public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
        {
            _logPath = logPath;
            _minLevel = minLevel;
            Directory.CreateDirectory(_logPath);
        }
    
        public static void Debug(string message, string source = "")
        {
            Log(LogLevel.Debug, message, source);
        }
    
        public static void Info(string message, string source = "")
        {
            Log(LogLevel.Info, message, source);
        }
    
        public static void Warning(string message, string source = "", Exception? ex = null)
        {
            Log(LogLevel.Warning, message, source, ex);
        }
    
        public static void Error(string message, string source = "", Exception? ex = null)
        {
            Log(LogLevel.Error, message, source, ex);
        }
    
        public static void Fatal(string message, string source = "", Exception? ex = null)
        {
            Log(LogLevel.Fatal, message, source, ex);
        }
    
        private static void Log(LogLevel level, string message, string source, Exception? ex = null)
        {
            if (level < _minLevel) return;
    
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var levelStr = level.ToString().ToUpperInvariant();
            var exMessage = ex != null ? $" | Exception: {ex.Message}" : "";
            var logEntry = $"[{timestamp}] [{levelStr}] [{source}] {message}{exMessage}";
    
            lock (_lock)
            {
                Console.WriteLine(logEntry);
                var fileName = $"ejlive_{DateTime.Now:yyyyMMdd}.log";
                var filePath = Path.Combine(_logPath, fileName);
                File.AppendAllText(filePath, logEntry + Environment.NewLine);
            }
        }
    }
    public partial class Logger
        {
            private readonly string _category;
            private readonly LogLevel _minimumLevel;
            private readonly string? _logFilePath;
            private static readonly object _fileLock = new();
            _minimumLevel = minimumLevel;
            _logFilePath = logFilePath;
            private static readonly object _lock = new();
            private static string _logPath = @"C:\EJLive_Storage\Logs";
            private static LogLevel _minLevel = LogLevel.Debug;
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
                _category = category;
                _minimumLevel = minimumLevel;
                _logFilePath = logFilePath;
            }
            public void Debug(string message)
            {
                Log(LogLevel.Debug, message);
            }
            public void Info(string message)
            {
                Log(LogLevel.Info, message);
            }
            public void Warning(string message)
            {
                Log(LogLevel.Warning, message);
            }
            public void Error(string message, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                Log(LogLevel.Error, fullMessage);
            }
            public void Fatal(string message, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                Log(LogLevel.Fatal, fullMessage);
            }
            private void Log(LogLevel level, string message)
            {
                if (level < _minimumLevel) return;
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] [{_category}] {message}";
                // Output to debug console
                System.Diagnostics.Debug.WriteLine(logEntry);
                // Optionally write to file
                if (!string.IsNullOrEmpty(_logFilePath))
                {
                    lock (_fileLock)
                    {
                        try
                        {
                            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                        }
                        catch
                        {
                            // Silently fail if file logging is unavailable
                        }
                    }
                }
            }
            public static void Debug(string message, string category)
            {
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Debug}] [{category}] {message}");
            }
            public static void Info(string message, string category)
            {
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Info}] [{category}] {message}");
            }
            public static void Warning(string message, string category)
            {
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Warning}] [{category}] {message}");
            }
            public static void Error(string message, string category, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Error}] [{category}] {fullMessage}");
            }
            public static void Fatal(string message, string category, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Fatal}] [{category}] {fullMessage}");
            }
            public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
            {
                _logPath = logPath;
                _minLevel = minLevel;
                Directory.CreateDirectory(_logPath);
            }
            public static void Debug(string message, string source = "")
            {
                Log(LogLevel.Debug, message, source);
            }
            public static void Info(string message, string source = "")
            {
                Log(LogLevel.Info, message, source);
            }
            public static void Warning(string message, string source = "", Exception? ex = null)
            {
                Log(LogLevel.Warning, message, source, ex);
            }
            public static void Error(string message, string source = "", Exception? ex = null)
            {
                Log(LogLevel.Error, message, source, ex);
            }
            public static void Fatal(string message, string source = "", Exception? ex = null)
            {
                Log(LogLevel.Fatal, message, source, ex);
            }
            private static void Log(LogLevel level, string message, string source, Exception? ex = null)
            {
                if (level < _minLevel) return;
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var levelStr = level.ToString().ToUpperInvariant();
                var exMessage = ex != null ? $" | Exception: {ex.Message}" : "";
                var logEntry = $"[{timestamp}] [{levelStr}] [{source}] {message}{exMessage}";
                lock (_lock)
                {
                    Console.WriteLine(logEntry);
                    var fileName = $"ejlive_{DateTime.Now:yyyyMMdd}.log";
                    var filePath = Path.Combine(_logPath, fileName);
                    File.AppendAllText(filePath, logEntry + Environment.NewLine);
                }
            }
        }

    public partial enum Level
        {
        }
    public partial class LogEntry
        {
        }
}

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
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
    
    
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
    
    
            private StreamWriter _writer;
    
    
            private string _currentFile;
    
    
            private readonly Thread _flushThread;
    
    
            private volatile bool _running = true;
    
    
            public Level  MinLevel       { get; set; } = Level.Info;
    
    
            public string LogDirectory   { get; private set; }
    
    
            public string Prefix         { get; set; } = "ejlive";
    
    
            public int    MaxFileSizeMB  { get; set; } = 50;
    
    
            public int    KeepDays       { get; set; } = 30;
    
    
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
    
    
            private static readonly object _initLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_36\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
    
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
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_37\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\CodexMaregerestructuredReplitFinalzip\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\EJLiveEnterprisev4Unified\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\VBCode_local\CodexMarege\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\CodexMaregerestructuredReplitFinalzip\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\EJLiveEnterprisev4Unified\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\VBCode_local\CodexMarege\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_41\CodexMaregerestructuredReplitFinalzip\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-12\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-14\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-15\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-16\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-1\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-22\EJLiveEnterprisev4Unified\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-26\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-27\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-28\VBCode_local\CodexMarege\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-29\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-33\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-34\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-4\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-5\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-6\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-7\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-8\CodexMaregerestructuredReplitFinalzip\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-9\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            public event EventHandler<LogEntry> OnLog;
    
    
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
    
    
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
    
    
            private StreamWriter _writer;
    
    
            private string _currentFile;
    
    
            private readonly Thread _flushThread;
    
    
            private volatile bool _running = true;
    
    
            public Level  MinLevel       { get; set; } = Level.Info;
    
    
            public string LogDirectory   { get; private set; }
    
    
            public string Prefix         { get; set; } = "ejlive";
    
    
            public int    MaxFileSizeMB  { get; set; } = 50;
    
    
            public int    KeepDays       { get; set; } = 30;
    
    
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
    
    
            private static readonly object _initLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            public event EventHandler<LogEntry> OnLog;
    
    
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
    
    
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
    
    
            private StreamWriter _writer;
    
    
            private string _currentFile;
    
    
            private readonly Thread _flushThread;
    
    
            private volatile bool _running = true;
    
    
            public Level  MinLevel       { get; set; } = Level.Info;
    
    
            public string LogDirectory   { get; private set; }
    
    
            public string Prefix         { get; set; } = "ejlive";
    
    
            public int    MaxFileSizeMB  { get; set; } = 50;
    
    
            public int    KeepDays       { get; set; } = 30;
    
    
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
    
    
            private static readonly object _initLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\VBCode_local\CodexMarege\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local-2\VBCode_local\CodexMarege\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            public event EventHandler<LogEntry> OnLog;
    
    
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
    
    
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
    
    
            private StreamWriter _writer;
    
    
            private string _currentFile;
    
    
            private readonly Thread _flushThread;
    
    
            private volatile bool _running = true;
    
    
            public Level  MinLevel       { get; set; } = Level.Info;
    
    
            public string LogDirectory   { get; private set; }
    
    
            public string Prefix         { get; set; } = "ejlive";
    
    
            public int    MaxFileSizeMB  { get; set; } = 50;
    
    
            public int    KeepDays       { get; set; } = 30;
    
    
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
    
    
            private static readonly object _initLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLiveEnterprisev4Unified\EJLiveEnterprisev4Unified\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLiveEnterprisev4Unified\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            public event EventHandler<LogEntry> OnLog;
    
    
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
    
    
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
    
    
            private StreamWriter _writer;
    
    
            private string _currentFile;
    
    
            private readonly Thread _flushThread;
    
    
            private volatile bool _running = true;
    
    
            public Level  MinLevel       { get; set; } = Level.Info;
    
    
            public string LogDirectory   { get; private set; }
    
    
            public string Prefix         { get; set; } = "ejlive";
    
    
            public int    MaxFileSizeMB  { get; set; } = 50;
    
    
            public int    KeepDays       { get; set; } = 30;
    
    
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
    
    
            private static readonly object _initLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            public event EventHandler<LogEntry> OnLog;
    
    
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
    
    
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
    
    
            private StreamWriter _writer;
    
    
            private string _currentFile;
    
    
            private readonly Thread _flushThread;
    
    
            private volatile bool _running = true;
    
    
            public Level  MinLevel       { get; set; } = Level.Info;
    
    
            public string LogDirectory   { get; private set; }
    
    
            public string Prefix         { get; set; } = "ejlive";
    
    
            public int    MaxFileSizeMB  { get; set; } = 50;
    
    
            public int    KeepDays       { get; set; } = 30;
    
    
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
    
    
            private static readonly object _initLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            public event EventHandler<LogEntry> OnLog;
    
    
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
    
    
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
    
    
            private StreamWriter _writer;
    
    
            private string _currentFile;
    
    
            private readonly Thread _flushThread;
    
    
            private volatile bool _running = true;
    
    
            public Level  MinLevel       { get; set; } = Level.Info;
    
    
            public string LogDirectory   { get; private set; }
    
    
            public string Prefix         { get; set; } = "ejlive";
    
    
            public int    MaxFileSizeMB  { get; set; } = 50;
    
    
            public int    KeepDays       { get; set; } = 30;
    
    
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
    
    
            private static readonly object _initLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            public event EventHandler<LogEntry> OnLog;
    
    
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
    
    
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
    
    
            private StreamWriter _writer;
    
    
            private string _currentFile;
    
    
            private readonly Thread _flushThread;
    
    
            private volatile bool _running = true;
    
    
            public Level  MinLevel       { get; set; } = Level.Info;
    
    
            public string LogDirectory   { get; private set; }
    
    
            public string Prefix         { get; set; } = "ejlive";
    
    
            public int    MaxFileSizeMB  { get; set; } = 50;
    
    
            public int    KeepDays       { get; set; } = 30;
    
    
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
    
    
            private static readonly object _initLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            public event EventHandler<LogEntry> OnLog;
    
    
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    public partial class AppLogger : IDisposable
        {
            private static AppLogger _instance;
    
    
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
    
    
            private StreamWriter _writer;
    
    
            private string _currentFile;
    
    
            private readonly Thread _flushThread;
    
    
            private volatile bool _running = true;
    
    
            public Level  MinLevel       { get; set; } = Level.Info;
    
    
            public string LogDirectory   { get; private set; }
    
    
            public string Prefix         { get; set; } = "ejlive";
    
    
            public int    MaxFileSizeMB  { get; set; } = 50;
    
    
            public int    KeepDays       { get; set; } = 30;
    
    
            private AppLogger()
            {
                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                _flushThread.Start();
            }
    
    
            private static readonly object _initLock = new object();
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
    
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
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\VBCode_local\CodexMarege\CodexMarege\EJLive.Unified\src\EJLive.Shared\Logger.cs
            private readonly object _fileLock = new object();
    
    
            public event EventHandler<LogEntry> OnLog;
    
    
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    public enum Level {
    
        public partial public public class AppLogger : IDisposable
        {
            private static AppLogger _instance;
            private StreamWriter _writer;
            private string _currentFile;
            private readonly Thread _flushThread;
            private volatile bool _running = true;
            private static readonly object _initLock = new object();
            private readonly object _fileLock = new object();
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
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
            private AppLogger()
            {
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
            public Level  MinLevel       { get; set; }
            public string LogDirectory   { get; private set; }
            public string Prefix         { get; set; }
            public int    MaxFileSizeMB  { get; set; }
            public int    KeepDays       { get; set; }
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
            public DateTime        Timestamp { get; set; }
            public string          Source    { get; set; }
            public string          Message   { get; set; }
            public void Initialize(string logDirectory, string prefix = "ejlive", Level minLevel = Level.Info)
            {
            public void Debug(string msg, string src = null)    =>
            public void Info(string msg, string src = null)     =>
            public void Warning(string msg, string src = null)  =>
            public void Error(string msg, string src = null)    =>
            public void Error(Exception ex, string src = null)  =>
            public void Critical(string msg, string src = null) =>
            public void Log(Level level, string message, string source = null)
            {
            private void FlushLoop()
            {
            private void FlushBuffer()
            {
            private void OpenWriter()
            {
            private void RotateIfNeeded()
            {
            public void Dispose()
            {
            public string Format() =>
            public event EventHandler<LogEntry> OnLog;
        }
    
        public partial public public class LogEntry
        {
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
            public DateTime        Timestamp { get; set; }
            public string          Source    { get; set; }
            public string          Message   { get; set; }
            public string Format() =>
        }
    
    }
    public enum Level {
    
        public partial public sealed class AppLogger : IDisposable
        {
            private static AppLogger _instance;
            private static readonly object _initLock = new object();
            private StreamWriter _writer;
            private string _currentFile;
            private readonly object _fileLock = new object();
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
            private readonly Thread _flushThread;
            private volatile bool _running = true;
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
            private readonly string _category;
            private readonly LogLevel _minimumLevel;
            private static readonly object _lock = new();
            private static string _logPath = @"
            private static LogLevel _minLevel = LogLevel.Debug;
            private static LogLevel _minLevel = LogLevel.Debug;
            private AppLogger()
            {
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
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
            public Level  MinLevel       { get; set; }
            public string LogDirectory   { get; private set; }
            public string Prefix         { get; set; }
            public int    MaxFileSizeMB  { get; set; }
            public int    KeepDays       { get; set; }
            public DateTime        Timestamp { get; set; }
            public string          Source    { get; set; }
            public string          Message   { get; set; }
            public void Initialize(string logDirectory, string prefix = "ejlive", Level minLevel = Level.Info)
            {
            public void Debug(string msg, string src = null)    =>
            public void Info(string msg, string src = null)     =>
            public void Warning(string msg, string src = null)  =>
            public void Error(string msg, string src = null)    =>
            public void Error(Exception ex, string src = null)  =>
            public void Critical(string msg, string src = null) =>
            public void Log(Level level, string message, string source = null)
            {
            private void FlushLoop()
            {
            private void FlushBuffer()
            {
            private void OpenWriter()
            {
            private void RotateIfNeeded()
            {
            public void Dispose()
            {
            public string Format() =>
            public void Debug(string message)
            {
            public void Info(string message)
            {
            public void Warning(string message)
            {
            public void Error(string message, Exception? ex = null)
            {
            public void Fatal(string message, Exception? ex = null)
            {
            private void Log(LogLevel level, string message)
            {
            public static void Debug(string message, string category)
            {
            public static void Info(string message, string category)
            {
            public static void Warning(string message, string category)
            {
            public static void Error(string message, string category, Exception? ex = null)
            {
            public static void Fatal(string message, string category, Exception? ex = null)
            {
            public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
            {
            public static void Debug(string message, string source = "")
            {
            public static void Info(string message, string source = "")
            {
            public static void Warning(string message, string source = "", Exception? ex = null)
            {
            public static void Error(string message, string source = "", Exception? ex = null)
            {
            public static void Fatal(string message, string source = "", Exception? ex = null)
            {
            private static void Log(LogLevel level, string message, string source, Exception? ex = null)
            {
            public event EventHandler<LogEntry> OnLog;
        }
    
        public partial public class LogEntry
        {
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
            public DateTime        Timestamp { get; set; }
            public string          Source    { get; set; }
            public string          Message   { get; set; }
            public string Format() =>
        }
    
        public partial public class Logger
        {
            private readonly string _category;
            private readonly LogLevel _minimumLevel;
            private static readonly object _fileLock = new();
            private static readonly object _lock = new();
            private static string _logPath = @"
            private static LogLevel _minLevel = LogLevel.Debug;
            private static LogLevel _minLevel = LogLevel.Debug;
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            // Variant from:
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            // Variant from:
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            // Variant from:
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            public void Debug(string message)
            {
            public void Debug(string message)
            {
            public void Info(string message)
            {
            public void Warning(string message)
            {
            public void Error(string message, Exception? ex = null)
            {
            public void Fatal(string message, Exception? ex = null)
            {
            private void Log(LogLevel level, string message)
            {
            public static void Debug(string message, string category)
            {
            public static void Info(string message, string category)
            {
            public static void Warning(string message, string category)
            {
            public static void Error(string message, string category, Exception? ex = null)
            {
            public static void Fatal(string message, string category, Exception? ex = null)
            {
            public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
            {
            public static void Debug(string message, string source = "")
            {
            public static void Info(string message, string source = "")
            {
            public static void Warning(string message, string source = "", Exception? ex = null)
            {
            public static void Error(string message, string source = "", Exception? ex = null)
            {
            public static void Fatal(string message, string source = "", Exception? ex = null)
            {
            private static void Log(LogLevel level, string message, string source, Exception? ex = null)
            {
        }
    
    }
    public enum Level {
    
        public partial public class AppLogger : IDisposable
        {
            private static AppLogger _instance;
            private StreamWriter _writer;
            private string _currentFile;
            private readonly Thread _flushThread;
            private volatile bool _running = true;
            private static readonly object _initLock = new object();
            private readonly object _fileLock = new object();
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
            private AppLogger()
            {
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
            public Level  MinLevel       { get; set; }
            public string LogDirectory   { get; private set; }
            public string Prefix         { get; set; }
            public int    MaxFileSizeMB  { get; set; }
            public int    KeepDays       { get; set; }
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
            public void Initialize(string logDirectory, string prefix = "ejlive", Level minLevel = Level.Info)
            {
            public void Debug(string msg, string src = null)    =>
            public void Info(string msg, string src = null)     =>
            public void Warning(string msg, string src = null)  =>
            public void Error(string msg, string src = null)    =>
            public void Error(Exception ex, string src = null)  =>
            public void Critical(string msg, string src = null) =>
            public void Log(Level level, string message, string source = null)
            {
            private void FlushLoop()
            {
            private void FlushBuffer()
            {
            private void OpenWriter()
            {
            private void RotateIfNeeded()
            {
            public void Dispose()
            {
            public event EventHandler<LogEntry> OnLog;
        }
    
        public partial public class LogEntry
        {
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
            public DateTime        Timestamp { get; set; }
            public string          Source    { get; set; }
            public string          Message   { get; set; }
            public string Format() =>
        }
    
    }
    public enum Level {
    
        public partial public sealed class AppLogger : IDisposable
        {
            private static AppLogger _instance;
            private static readonly object _initLock = new object();
            private StreamWriter _writer;
            private string _currentFile;
            private readonly object _fileLock = new object();
            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
            private readonly Thread _flushThread;
            private volatile bool _running = true;
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
            private readonly string _category;
            private readonly LogLevel _minimumLevel;
            private static readonly object _lock = new();
            private static string _logPath = @"
            private static LogLevel _minLevel = LogLevel.Debug;
            private AppLogger()
            {
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
            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
            public Level  MinLevel       { get; set; }
            public string LogDirectory   { get; private set; }
            public string Prefix         { get; set; }
            public int    MaxFileSizeMB  { get; set; }
            public int    KeepDays       { get; set; }
            public DateTime        Timestamp { get; set; }
            public string          Source    { get; set; }
            public string          Message   { get; set; }
            public void Initialize(string logDirectory, string prefix = "ejlive", Level minLevel = Level.Info)
            {
            public void Debug(string msg, string src = null)    =>
            public void Info(string msg, string src = null)     =>
            public void Warning(string msg, string src = null)  =>
            public void Error(string msg, string src = null)    =>
            public void Error(Exception ex, string src = null)  =>
            public void Critical(string msg, string src = null) =>
            public void Log(Level level, string message, string source = null)
            {
            private void FlushLoop()
            {
            private void FlushBuffer()
            {
            private void OpenWriter()
            {
            private void RotateIfNeeded()
            {
            public void Dispose()
            {
            public string Format() =>
            public void Debug(string message)
            {
            public void Info(string message)
            {
            public void Warning(string message)
            {
            public void Error(string message, Exception? ex = null)
            {
            public void Fatal(string message, Exception? ex = null)
            {
            private void Log(LogLevel level, string message)
            {
            public static void Debug(string message, string category)
            {
            public static void Info(string message, string category)
            {
            public static void Warning(string message, string category)
            {
            public static void Error(string message, string category, Exception? ex = null)
            {
            public static void Fatal(string message, string category, Exception? ex = null)
            {
            public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
            {
            public static void Debug(string message, string source = "")
            {
            public static void Info(string message, string source = "")
            {
            public static void Warning(string message, string source = "", Exception? ex = null)
            {
            public static void Error(string message, string source = "", Exception? ex = null)
            {
            public static void Fatal(string message, string source = "", Exception? ex = null)
            {
            private static void Log(LogLevel level, string message, string source, Exception? ex = null)
            {
            public event EventHandler<LogEntry> OnLog;
        }
    
        public partial public class LogEntry
        {
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
            public DateTime        Timestamp { get; set; }
            public string          Source    { get; set; }
            public string          Message   { get; set; }
            public string Format() =>
        }
    
        public partial public class Logger
        {
            private readonly string _category;
            private readonly LogLevel _minimumLevel;
            private static readonly object _fileLock = new();
            private static readonly object _lock = new();
            private static string _logPath = @"
            private static LogLevel _minLevel = LogLevel.Debug;
            private static LogLevel _minLevel = LogLevel.Debug;
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            // Variant from:
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            // Variant from:
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            // Variant from:
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            public void Debug(string message)
            {
            public void Debug(string message)
            {
            public void Info(string message)
            {
            public void Warning(string message)
            {
            public void Error(string message, Exception? ex = null)
            {
            public void Fatal(string message, Exception? ex = null)
            {
            private void Log(LogLevel level, string message)
            {
            public static void Debug(string message, string category)
            {
            public static void Info(string message, string category)
            {
            public static void Warning(string message, string category)
            {
            public static void Error(string message, string category, Exception? ex = null)
            {
            public static void Fatal(string message, string category, Exception? ex = null)
            {
            public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
            {
            public static void Debug(string message, string source = "")
            {
            public static void Info(string message, string source = "")
            {
            public static void Warning(string message, string source = "", Exception? ex = null)
            {
            public static void Error(string message, string source = "", Exception? ex = null)
            {
            public static void Fatal(string message, string source = "", Exception? ex = null)
            {
            private static void Log(LogLevel level, string message, string source, Exception? ex = null)
            {
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
    public partial class LogEntry
        {
            public DateTime        Timestamp { get; set; }
    
    
            public AppLogger.Level Level     { get; set; }
    
    
            public string          Source    { get; set; }
    
    
            public string          Message   { get; set; }
    
    
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
    
    
            public string Format() =>
                $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{LevelTag,-3}] [{Source,-22}] {Message}";
    
    
        }

    // Class: AppLogger (from 2 sources)
        public sealed partial class AppLogger : IDisposable
        {
            // --- Constants & Fields ---
                    private static AppLogger _instance;
    
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
    
                    private StreamWriter _writer;
    
                    private string _currentFile;
    
                    private readonly Thread _flushThread;
    
                    private volatile bool _running = true;
    
    
            // --- Properties ---
                    public Level  MinLevel       { get; set; } = Level.Info;
    
                    public string LogDirectory   { get; private set; }
    
                    public string Prefix         { get; set; } = "ejlive";
    
                    public int    MaxFileSizeMB  { get; set; } = 50;
    
                    public int    KeepDays       { get; set; } = 30;
    
    
            // --- Constructors ---
                    private AppLogger()
                    {
                        _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                        _flushThread.Start();
                    }
    
    
            // --- Methods ---
                    private static readonly object _initLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
                    private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\UnifiedEJLiveProject\src\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
    
            // --- Events ---
                    public event EventHandler<LogEntry> OnLog;
    
    
            // --- Nested Enums ---
                    public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    // Class: AppLogger (from 3 sources)
        public sealed partial class AppLogger : IDisposable
        {
            // --- Constants & Fields ---
                    private static AppLogger _instance;
    
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
    
                    private StreamWriter _writer;
    
                    private string _currentFile;
    
                    private readonly Thread _flushThread;
    
                    private volatile bool _running = true;
    
    
            // --- Properties ---
                    public Level  MinLevel       { get; set; } = Level.Info;
    
                    public string LogDirectory   { get; private set; }
    
                    public string Prefix         { get; set; } = "ejlive";
    
                    public int    MaxFileSizeMB  { get; set; } = 50;
    
                    public int    KeepDays       { get; set; } = 30;
    
    
            // --- Constructors ---
                    private AppLogger()
                    {
                        _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                        _flushThread.Start();
                    }
    
    
            // --- Methods ---
                    private static readonly object _initLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified\EJLive.Unified\src\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
                    private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\Ejlive\EJLive.Unified\src\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive.Unified.Enhanced\EJLive.Unified\src\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
    
            // --- Events ---
                    public event EventHandler<LogEntry> OnLog;
    
    
            // --- Nested Enums ---
                    public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    // Class: AppLogger (from 2 sources)
        public sealed partial class AppLogger : IDisposable
        {
            // --- Constants & Fields ---
                    private static AppLogger _instance;
    
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
    
                    private StreamWriter _writer;
    
                    private string _currentFile;
    
                    private readonly Thread _flushThread;
    
                    private volatile bool _running = true;
    
    
            // --- Properties ---
                    public Level  MinLevel       { get; set; } = Level.Info;
    
                    public string LogDirectory   { get; private set; }
    
                    public string Prefix         { get; set; } = "ejlive";
    
                    public int    MaxFileSizeMB  { get; set; } = 50;
    
                    public int    KeepDays       { get; set; } = 30;
    
    
            // --- Constructors ---
                    private AppLogger()
                    {
                        _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                        _flushThread.Start();
                    }
    
    
            // --- Methods ---
                    private static readonly object _initLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_Ultimate\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
                    private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Unified_Master\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
    
            // --- Events ---
                    public event EventHandler<LogEntry> OnLog;
    
    
            // --- Nested Enums ---
                    public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    // Class: AppLogger (from 2 sources)
        public sealed partial class AppLogger : IDisposable
        {
            // --- Constants & Fields ---
                    private static AppLogger _instance;
    
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
    
                    private StreamWriter _writer;
    
                    private string _currentFile;
    
                    private readonly Thread _flushThread;
    
                    private volatile bool _running = true;
    
    
            // --- Properties ---
                    public Level  MinLevel       { get; set; } = Level.Info;
    
                    public string LogDirectory   { get; private set; }
    
                    public string Prefix         { get; set; } = "ejlive";
    
                    public int    MaxFileSizeMB  { get; set; } = 50;
    
                    public int    KeepDays       { get; set; } = 30;
    
    
            // --- Constructors ---
                    private AppLogger()
                    {
                        _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                        _flushThread.Start();
                    }
    
    
            // --- Methods ---
                    private static readonly object _initLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMaregerestructuredReplitFinalzip\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
                    private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMaregerestructured\CodexMaregerestructuredReplitFinalzip\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
    
            // --- Events ---
                    public event EventHandler<LogEntry> OnLog;
    
    
            // --- Nested Enums ---
                    public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    // Class: AppLogger (from 9 sources)
        public sealed partial class AppLogger : IDisposable
        {
            // --- Constants & Fields ---
                            private static AppLogger _instance;
    
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
    
                            private StreamWriter _writer;
    
                            private string _currentFile;
    
                            private readonly Thread _flushThread;
    
                            private volatile bool _running = true;
    
    
            // --- Properties ---
                            public Level  MinLevel       { get; set; } = Level.Info;
    
                            public string LogDirectory   { get; private set; }
    
                            public string Prefix         { get; set; } = "ejlive";
    
                            public int    MaxFileSizeMB  { get; set; } = 50;
    
                            public int    KeepDays       { get; set; } = 30;
    
    
            // --- Constructors ---
                            private AppLogger()
                            {
                                _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                                _flushThread.Start();
                            }
    
    
            // --- Methods ---
                            private static readonly object _initLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_Unified\CodexMarege\EJLive.Shared\Logger.cs
                            private readonly object _fileLock = new object();
    
                            private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Enterprise_v4_Unified\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Final\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
    
            // --- Events ---
                            public event EventHandler<LogEntry> OnLog;
    
    
            // --- Nested Enums ---
                            public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    // Class: AppLogger (from 5 sources)
        public sealed partial class AppLogger : IDisposable
        {
            // --- Constants & Fields ---
                    private static AppLogger _instance;
    
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
    
                    private StreamWriter _writer;
    
                    private string _currentFile;
    
                    private readonly Thread _flushThread;
    
                    private volatile bool _running = true;
    
    
            // --- Properties ---
                    public Level  MinLevel       { get; set; } = Level.Info;
    
                    public string LogDirectory   { get; private set; }
    
                    public string Prefix         { get; set; } = "ejlive";
    
                    public int    MaxFileSizeMB  { get; set; } = 50;
    
                    public int    KeepDays       { get; set; } = 30;
    
    
            // --- Constructors ---
                    private AppLogger()
                    {
                        _flushThread = new Thread(FlushLoop) { IsBackground = true, Name = "EJLive.Logger" };
                        _flushThread.Start();
                    }
    
    
            // --- Methods ---
                    private static readonly object _initLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_new_src\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
                    private readonly ConcurrentQueue<LogEntry> _buffer = new ConcurrentQueue<LogEntry>();
    
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
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_restructured\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778703792493\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_1778791921744\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege_extracted\CodexMarege\EJLive.Shared\Logger.cs
                    private readonly object _fileLock = new object();
    
    
            // --- Events ---
                    public event EventHandler<LogEntry> OnLog;
    
    
            // --- Nested Enums ---
                    public enum Level { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    
    
        }
    // Enum: Level (from 4 sources)
        public partial enum Level
        {
        }
    // Class: LogEntry (from 6 sources)
        public partial class LogEntry
        {
            // --- Properties ---
                    public DateTime        Timestamp { get; set; }
    
                    public AppLogger.Level Level     { get; set; }
    
                    public string          Source    { get; set; }
    
                    public string          Message   { get; set; }
    
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
    
    
            // --- Methods ---
                    public string Format() =>
                        $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{LevelTag,-3}] [{Source,-22}] {Message}";
    
    
        }
    // Class: LogEntry (from 3 sources)
        public partial class LogEntry
        {
            // --- Properties ---
                    public DateTime        Timestamp { get; set; }
    
                    public AppLogger.Level Level     { get; set; }
    
                    public string          Source    { get; set; }
    
                    public string          Message   { get; set; }
    
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
    
    
            // --- Methods ---
                    public string Format() =>
                        $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{LevelTag,-3}] [{Source,-22}] {Message}";
    
    
        }
    // Class: LogEntry (from 2 sources)
        public partial class LogEntry
        {
            // --- Properties ---
                    public DateTime        Timestamp { get; set; }
    
                    public AppLogger.Level Level     { get; set; }
    
                    public string          Source    { get; set; }
    
                    public string          Message   { get; set; }
    
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
    
    
            // --- Methods ---
                    public string Format() =>
                        $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{LevelTag,-3}] [{Source,-22}] {Message}";
    
    
        }
    // Class: LogEntry (from 9 sources)
        public partial class LogEntry
        {
            // --- Properties ---
                            public DateTime        Timestamp { get; set; }
    
                            public AppLogger.Level Level     { get; set; }
    
                            public string          Source    { get; set; }
    
                            public string          Message   { get; set; }
    
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
    
    
            // --- Methods ---
                            public string Format() =>
                                $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{LevelTag,-3}] [{Source,-22}] {Message}";
    
    
        }
    // Class: LogEntry (from 5 sources)
        public partial class LogEntry
        {
            // --- Properties ---
                    public DateTime        Timestamp { get; set; }
    
                    public AppLogger.Level Level     { get; set; }
    
                    public string          Source    { get; set; }
    
                    public string          Message   { get; set; }
    
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
    
    
            // --- Methods ---
                    public string Format() =>
                        $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{LevelTag,-3}] [{Source,-22}] {Message}";
    
    
        }
    public partial class Logger
        {
            private readonly string _category;
    
    
            private readonly LogLevel _minimumLevel;
    
    
            private readonly string? _logFilePath;
    
    
            private static readonly object _fileLock = new();
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Utils\Logger.cs
            _minimumLevel = minimumLevel;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Utils\Logger.cs
            _logFilePath = logFilePath;
    
    
            private static readonly object _lock = new();
    
    
            private static string _logPath = @"C:\EJLive_Storage\Logs";
    
    
            private static LogLevel _minLevel = LogLevel.Debug;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\Logger.cs
            _minimumLevel = minimumLevel;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\Logger.cs
            _logFilePath = logFilePath;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Utils\Logger.cs
            _minimumLevel = minimumLevel;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Utils\Logger.cs
            _logFilePath = logFilePath;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Utils\Logger.cs.before_unify
            _minimumLevel = minimumLevel;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Utils\Logger.cs.before_unify
            _logFilePath = logFilePath;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Utils\Logger.cs.v17_bak
            _minimumLevel = minimumLevel;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Utils\Logger.cs.v17_bak
            _logFilePath = logFilePath;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Utils\Logger.cs.v22_bak
            _minimumLevel = minimumLevel;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Utils\Logger.cs.v22_bak
            _logFilePath = logFilePath;
    
    
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            _category = category;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Utils\Logger.cs
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
                _category = category;
                _minimumLevel = minimumLevel;
                _logFilePath = logFilePath;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\Logger.cs
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
                _category = category;
                _minimumLevel = minimumLevel;
                _logFilePath = logFilePath;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Utils\Logger.cs
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
                _category = category;
                _minimumLevel = minimumLevel;
                _logFilePath = logFilePath;
            }
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Utils\Logger.cs
            public Logger(string category, LogLevel minimumLevel = LogLevel.Info, string? logFilePath = null)
            {
                _category = category;
                _minimumLevel = minimumLevel;
                _logFilePath = logFilePath;
            }
    
    
            public void Debug(string message)
            {
                Log(LogLevel.Debug, message);
            }
    
    
            public void Info(string message)
            {
                Log(LogLevel.Info, message);
            }
    
    
            public void Warning(string message)
            {
                Log(LogLevel.Warning, message);
            }
    
    
            public void Error(string message, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                Log(LogLevel.Error, fullMessage);
            }
    
    
            public void Fatal(string message, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                Log(LogLevel.Fatal, fullMessage);
            }
    
    
            private void Log(LogLevel level, string message)
            {
                if (level < _minimumLevel) return;
    
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] [{_category}] {message}";
    
                // Output to debug console
                System.Diagnostics.Debug.WriteLine(logEntry);
    
                // Optionally write to file
                if (!string.IsNullOrEmpty(_logFilePath))
                {
                    lock (_fileLock)
                    {
                        try
                        {
                            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                        }
                        catch
                        {
                            // Silently fail if file logging is unavailable
                        }
                    }
                }
            }
    
    
            public static void Debug(string message, string category)
            {
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Debug}] [{category}] {message}");
            }
    
    
            public static void Info(string message, string category)
            {
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Info}] [{category}] {message}");
            }
    
    
            public static void Warning(string message, string category)
            {
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Warning}] [{category}] {message}");
            }
    
    
            public static void Error(string message, string category, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Error}] [{category}] {fullMessage}");
            }
    
    
            public static void Fatal(string message, string category, Exception? ex = null)
            {
                var fullMessage = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}" : message;
                System.Diagnostics.Debug.WriteLine($"[{LogLevel.Fatal}] [{category}] {fullMessage}");
            }
    
    
            public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
            {
                _logPath = logPath;
                _minLevel = minLevel;
                Directory.CreateDirectory(_logPath);
            }
    
    
            public static void Debug(string message, string source = "")
            {
                Log(LogLevel.Debug, message, source);
            }
    
    
            public static void Info(string message, string source = "")
            {
                Log(LogLevel.Info, message, source);
            }
    
    
            public static void Warning(string message, string source = "", Exception? ex = null)
            {
                Log(LogLevel.Warning, message, source, ex);
            }
    
    
            public static void Error(string message, string source = "", Exception? ex = null)
            {
                Log(LogLevel.Error, message, source, ex);
            }
    
    
            public static void Fatal(string message, string source = "", Exception? ex = null)
            {
                Log(LogLevel.Fatal, message, source, ex);
            }
    
    
            private static void Log(LogLevel level, string message, string source, Exception? ex = null)
            {
                if (level < _minLevel) return;
    
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var levelStr = level.ToString().ToUpperInvariant();
                var exMessage = ex != null ? $" | Exception: {ex.Message}" : "";
                var logEntry = $"[{timestamp}] [{levelStr}] [{source}] {message}{exMessage}";
    
                lock (_lock)
                {
                    Console.WriteLine(logEntry);
                    var fileName = $"ejlive_{DateTime.Now:yyyyMMdd}.log";
                    var filePath = Path.Combine(_logPath, fileName);
                    File.AppendAllText(filePath, logEntry + Environment.NewLine);
                }
            }
    
    
        }
    public partial class Logger
        {
            private static readonly object _lock = new();
    
    
            private static string _logPath = @"C:\EJLive_Storage\Logs";
    
    
            private static LogLevel _minLevel = LogLevel.Debug;
    
    
            public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
            {
                _logPath = logPath;
                _minLevel = minLevel;
                Directory.CreateDirectory(_logPath);
            }
    
    
            public static void Debug(string message, string source = "")
            {
                Log(LogLevel.Debug, message, source);
            }
    
    
            public static void Info(string message, string source = "")
            {
                Log(LogLevel.Info, message, source);
            }
    
    
            public static void Warning(string message, string source = "", Exception? ex = null)
            {
                Log(LogLevel.Warning, message, source, ex);
            }
    
    
            public static void Error(string message, string source = "", Exception? ex = null)
            {
                Log(LogLevel.Error, message, source, ex);
            }
    
    
            public static void Fatal(string message, string source = "", Exception? ex = null)
            {
                Log(LogLevel.Fatal, message, source, ex);
            }
    
    
            private static void Log(LogLevel level, string message, string source, Exception? ex = null)
            {
                if (level < _minLevel) return;
    
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var levelStr = level.ToString().ToUpperInvariant();
                var exMessage = ex != null ? $" | Exception: {ex.Message}" : "";
                var logEntry = $"[{timestamp}] [{levelStr}] [{source}] {message}{exMessage}";
    
                lock (_lock)
                {
                    Console.WriteLine(logEntry);
                    var fileName = $"ejlive_{DateTime.Now:yyyyMMdd}.log";
                    var filePath = Path.Combine(_logPath, fileName);
                    File.AppendAllText(filePath, logEntry + Environment.NewLine);
                }
            }
    
    
        }
    // Class: Logger (from 3 sources)
        public static partial class Logger
        {
            // --- Constants & Fields ---
                private static readonly object _lock = new();
    
                private static string _logPath = @"C:\EJLive_Storage\Logs";
    
                private static LogLevel _minLevel = LogLevel.Debug;
    
    
            // --- Methods ---
                public static void Initialize(string logPath, LogLevel minLevel = LogLevel.Debug)
                {
                    _logPath = logPath;
                    _minLevel = minLevel;
                    Directory.CreateDirectory(_logPath);
                }
    
                public static void Debug(string message, string source = "")
                {
                    Log(LogLevel.Debug, message, source);
                }
    
                public static void Info(string message, string source = "")
                {
                    Log(LogLevel.Info, message, source);
                }
    
                public static void Warning(string message, string source = "", Exception? ex = null)
                {
                    Log(LogLevel.Warning, message, source, ex);
                }
    
                public static void Error(string message, string source = "", Exception? ex = null)
                {
                    Log(LogLevel.Error, message, source, ex);
                }
    
                public static void Fatal(string message, string source = "", Exception? ex = null)
                {
                    Log(LogLevel.Fatal, message, source, ex);
                }
    
                private static void Log(LogLevel level, string message, string source, Exception? ex = null)
                {
                    if (level < _minLevel) return;
    
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var levelStr = level.ToString().ToUpperInvariant();
                    var exMessage = ex != null ? $" | Exception: {ex.Message}" : "";
                    var logEntry = $"[{timestamp}] [{levelStr}] [{source}] {message}{exMessage}";
    
                    lock (_lock)
                    {
                        Console.WriteLine(logEntry);
                        var fileName = $"ejlive_{DateTime.Now:yyyyMMdd}.log";
                        var filePath = Path.Combine(_logPath, fileName);
                        File.AppendAllText(filePath, logEntry + Environment.NewLine);
                    }
                }
    
    
        }

    public enum Level
        { Debug = 0, Info = 1, Warning = 2, Error = 3, Critical = 4 }
    public partial enum Level
        {
        }
}
