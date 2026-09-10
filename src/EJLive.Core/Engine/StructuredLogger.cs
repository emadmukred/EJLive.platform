using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Unified structured logger for the EJLive system.
    /// Writes rolling JSON Lines logs locally and queues events for server telemetry.
    /// All events carry a <see cref="OperationalEvent.CorrelationId"/> and are redacted
    /// via <see cref="LogRedactionEngine"/> before persistence.
    /// </summary>
    public sealed class StructuredLogger : IDisposable
    {
        private readonly string _logDirectory;
        private readonly long _maxFileSizeBytes;
        private readonly int _maxQueueSize;
        private readonly object _fileLock = new object();
        private readonly ConcurrentQueue<OperationalEvent> _serverQueue = new ConcurrentQueue<OperationalEvent>();
        private readonly Func<OperationalEvent, bool>? _serverEnqueueCallback;

        private string _currentFilePath = string.Empty;
        private long _currentFileSize;
        private bool _disposed;
        private static readonly JsonSerializerOptions LogJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredLogger"/> class.
        /// </summary>
        /// <param name="logDirectory">The directory where log files are written.</param>
        /// <param name="maxFileSizeBytes">The maximum size of a single log file before rotation. Default is 10 MB.</param>
        /// <param name="maxQueueSize">The maximum number of events to buffer for server telemetry. Default is 10,000.</param>
        /// <param name="serverEnqueueCallback">
        /// An optional callback invoked to deliver events to a remote server.
        /// Should return <c>true</c> on successful enqueue.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="logDirectory"/> is null.</exception>
        public StructuredLogger(
            string logDirectory,
            long maxFileSizeBytes = 10 * 1024 * 1024,
            int maxQueueSize = 10000,
            Func<OperationalEvent, bool>? serverEnqueueCallback = null)
        {
            _logDirectory = logDirectory ?? throw new ArgumentNullException(nameof(logDirectory));
            _maxFileSizeBytes = maxFileSizeBytes > 0 ? maxFileSizeBytes : 10 * 1024 * 1024;
            _maxQueueSize = maxQueueSize > 0 ? maxQueueSize : 10000;
            _serverEnqueueCallback = serverEnqueueCallback;

            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            RollFile();
        }

        /// <summary>
        /// Gets the approximate number of events currently buffered for server telemetry.
        /// </summary>
        public int ServerQueueDepth => _serverQueue.Count;

        /// <summary>
        /// Logs a synchronization event.
        /// </summary>
        /// <param name="correlationId">The correlation identifier for the operation.</param>
        /// <param name="atmId">The ATM identifier.</param>
        /// <param name="message">A descriptive message.</param>
        /// <param name="severity">The event severity.</param>
        /// <param name="metadata">Optional metadata object, serialized to JSON.</param>
        public void LogSync(Guid correlationId, string atmId, string message, OperationalSeverity severity, object? metadata = null)
        {
            WriteEvent(correlationId, atmId, "Sync", severity, message, metadata);
        }

        /// <summary>
        /// Logs a command event.
        /// </summary>
        /// <param name="correlationId">The correlation identifier for the operation.</param>
        /// <param name="atmId">The ATM identifier.</param>
        /// <param name="message">A descriptive message.</param>
        /// <param name="severity">The event severity.</param>
        /// <param name="metadata">Optional metadata object, serialized to JSON.</param>
        public void LogCommand(Guid correlationId, string atmId, string message, OperationalSeverity severity, object? metadata = null)
        {
            WriteEvent(correlationId, atmId, "Command", severity, message, metadata);
        }

        /// <summary>
        /// Logs a heartbeat event.
        /// </summary>
        /// <param name="correlationId">The correlation identifier for the operation.</param>
        /// <param name="atmId">The ATM identifier.</param>
        /// <param name="message">A descriptive message.</param>
        /// <param name="severity">The event severity.</param>
        /// <param name="metadata">Optional metadata object, serialized to JSON.</param>
        public void LogHeartbeat(Guid correlationId, string atmId, string message, OperationalSeverity severity, object? metadata = null)
        {
            WriteEvent(correlationId, atmId, "Heartbeat", severity, message, metadata);
        }

        /// <summary>
        /// Logs a parser event.
        /// </summary>
        /// <param name="correlationId">The correlation identifier for the operation.</param>
        /// <param name="atmId">The ATM identifier.</param>
        /// <param name="message">A descriptive message.</param>
        /// <param name="severity">The event severity.</param>
        /// <param name="metadata">Optional metadata object, serialized to JSON.</param>
        public void LogParser(Guid correlationId, string atmId, string message, OperationalSeverity severity, object? metadata = null)
        {
            WriteEvent(correlationId, atmId, "Parser", severity, message, metadata);
        }

        /// <summary>
        /// Logs an exception at a boundary. Use this for silently-caught exceptions
        /// to ensure they are captured without propagating the error.
        /// </summary>
        /// <param name="correlationId">The correlation identifier for the operation.</param>
        /// <param name="atmId">The ATM identifier.</param>
        /// <param name="context">A description of the boundary where the exception was caught.</param>
        /// <param name="ex">The exception that was caught.</param>
        public void LogBoundaryException(Guid correlationId, string atmId, string context, Exception ex)
        {
            var metadata = new Dictionary<string, object>
            {
                ["exceptionType"] = ex.GetType().Name,
                ["exceptionMessage"] = ex.Message,
                ["stackTrace"] = ex.StackTrace ?? string.Empty,
                ["boundaryContext"] = context
            };

            WriteEvent(correlationId, atmId, "Boundary", OperationalSeverity.Error, $"Boundary exception in {context}: {ex.Message}", metadata);
        }

        /// <summary>
        /// Attempts to flush all buffered server telemetry events using the configured callback.
        /// Events that fail to enqueue remain in the buffer.
        /// </summary>
        /// <returns>The number of events successfully flushed.</returns>
        public int FlushServerQueue()
        {
            if (_serverEnqueueCallback == null)
            {
                return 0;
            }

            int flushed = 0;
            while (_serverQueue.TryDequeue(out OperationalEvent? ev))
            {
                try
                {
                    if (_serverEnqueueCallback(ev))
                    {
                        flushed++;
                    }
                    else
                    {
                        // Re-enqueue at the back if delivery failed and we have capacity
                        if (_serverQueue.Count < _maxQueueSize)
                        {
                            _serverQueue.Enqueue(ev);
                        }
                        break;
                    }
                }
                catch
                {
                    // Silent catch: re-enqueue if capacity allows
                    if (_serverQueue.Count < _maxQueueSize)
                    {
                        _serverQueue.Enqueue(ev);
                    }
                    break;
                }
            }

            return flushed;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="StructuredLogger"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        private void WriteEvent(Guid correlationId, string atmId, string eventType, OperationalSeverity severity, string message, object? metadata)
        {
            if (_disposed)
            {
                return;
            }

            string metadataJson = metadata != null
                ? JsonSerializer.Serialize(metadata)
                : "{}";

            var ev = new OperationalEvent(
                EventId: Guid.NewGuid(),
                CorrelationId: correlationId,
                AtmId: atmId ?? string.Empty,
                EventType: eventType,
                Severity: severity,
                Message: LogRedactionEngine.Redact(message) ?? string.Empty,
                TimestampUtc: DateTime.UtcNow,
                MetadataJson: metadataJson);

            AppendToLocalLog(ev);
            EnqueueForServer(ev);
        }

        private void AppendToLocalLog(OperationalEvent ev)
        {
            string jsonLine = JsonSerializer.Serialize(ev, LogJsonOptions);

            byte[] bytes = Encoding.UTF8.GetBytes(jsonLine + "\n");

            lock (_fileLock)
            {
                if (_currentFileSize + bytes.Length > _maxFileSizeBytes)
                {
                    RollFile();
                }

                File.AppendAllText(_currentFilePath, jsonLine + "\n");
                _currentFileSize += bytes.Length;
            }
        }

        private void EnqueueForServer(OperationalEvent ev)
        {
            // Logging must remain non-blocking and deterministic. Remote delivery
            // is performed only by FlushServerQueue; writes always enqueue first.
            if (_serverQueue.Count < _maxQueueSize)
            {
                _serverQueue.Enqueue(ev);
            }
        }

        private void RollFile()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            string fileName = $"ejlive-events-{timestamp}.jsonl";
            _currentFilePath = Path.Combine(_logDirectory, fileName);
            _currentFileSize = 0;

            if (!File.Exists(_currentFilePath))
            {
                File.WriteAllText(_currentFilePath, string.Empty);
            }
            else
            {
                _currentFileSize = new FileInfo(_currentFilePath).Length;
            }
        }
    }
}
