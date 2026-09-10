using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EJLive.Core.Transport
{
    /// <summary>Central JSON serializer for transport contracts.</summary>
    public static class TransportSerializer
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        public static string Serialize<T>(T value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return JsonSerializer.Serialize(value, Options);
        }

        public static T Deserialize<T>(string json)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(json);
            return JsonSerializer.Deserialize<T>(json, Options)
                ?? throw new JsonException($"Transport payload could not be deserialized as {typeof(T).Name}.");
        }
    }

    // ===== Handshake =====

    /// <summary>Client-to-server handshake request with nonce for replay protection.</summary>
    public sealed class HandshakeRequest
    {
        /// <summary>ATM unique identifier.</summary>
        public string ATM_ID { get; set; } = string.Empty;

        /// <summary>Conventional alias used by application and test contracts.</summary>
        public string AtmId
        {
            get => ATM_ID;
            set => ATM_ID = value ?? string.Empty;
        }

        /// <summary>Machine hardware ID.</summary>
        public string MachineId { get; set; } = string.Empty;

        /// <summary>Agent/terminal identifier.</summary>
        public string AgentId { get; set; } = string.Empty;

        /// <summary>Protocol version identifier.</summary>
        public string ProtocolVersion { get; set; } = "EJLv5";

        /// <summary>Client application version.</summary>
        public string ClientVersion { get; set; } = "5.0.0";

        /// <summary>Cryptographic nonce to prevent replay.</summary>
        public string Nonce { get; set; } = GenerateNonce();

        /// <summary>UTC timestamp of the handshake request.</summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Client capabilities list.</summary>
        public List<string> Capabilities { get; set; } = new();

        /// <summary>ATM vendor (NCR, GRG, Wincor, Diebold, Hyosung, Cashway).</summary>
        public string Vendor { get; set; } = string.Empty;

        /// <summary>ATM model identifier.</summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>Path profile identifier for vendor-specific paths.</summary>
        public string PathProfile { get; set; } = string.Empty;

        /// <summary>HMAC-SHA256 signature of the request.</summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>Generate a cryptographically secure nonce.</summary>
        public static string GenerateNonce()
        {
            var bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>Serialize for signing.</summary>
        public string ToSignableString()
            => $"{ATM_ID}|{MachineId}|{AgentId}|{ProtocolVersion}|{Nonce}|{TimestampUtc:O}";

        /// <summary>Sign with HMAC-SHA256.</summary>
        public void Sign(byte[] secretKey)
        {
            var data = Encoding.UTF8.GetBytes(ToSignableString());
            using var hmac = new HMACSHA256(secretKey);
            Signature = Convert.ToBase64String(hmac.ComputeHash(data));
        }

        /// <summary>Verify signature.</summary>
        public bool VerifySignature(byte[] secretKey)
        {
            if (string.IsNullOrEmpty(Signature)) return false;
            var data = Encoding.UTF8.GetBytes(ToSignableString());
            using var hmac = new HMACSHA256(secretKey);
            var computed = Convert.ToBase64String(hmac.ComputeHash(data));
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(Signature),
                Encoding.UTF8.GetBytes(computed));
        }
    }

    /// <summary>Server-to-client handshake response.</summary>
    public sealed class HandshakeResponse
    {
        /// <summary>Whether the handshake was accepted.</summary>
        public bool Accepted { get; set; }

        /// <summary>Session identifier for the established connection.</summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>Server UTC time for time synchronization.</summary>
        public string ServerTimeUtc { get; set; } = DateTime.UtcNow.ToString("O");

        /// <summary>Reason for rejection if not accepted.</summary>
        public string? RejectReason { get; set; }

        /// <summary>Features enabled for this session.</summary>
        public List<string> EnabledFeatures { get; set; } = new();

        /// <summary>Server-assigned nonce for this session.</summary>
        public string ServerNonce { get; set; } = HandshakeRequest.GenerateNonce();

        /// <summary>Message of the day or server notice.</summary>
        public string? ServerNotice { get; set; }

        /// <summary>How long the session is valid (seconds).</summary>
        public int SessionTimeoutSeconds { get; set; } = 3600;
    }

    /// <summary>Handshake lifecycle states.</summary>
    public enum HandshakeState
    {
        None,
        Initiated,
        Accepted,
        Rejected,
        Expired,
        Replaced
    }

    // ===== Heartbeat =====

    /// <summary>Client-to-server heartbeat with operational telemetry.</summary>
    public sealed class HeartbeatPayload
    {
        /// <summary>ATM unique identifier.</summary>
        public string ATM_ID { get; set; } = string.Empty;

        /// <summary>Conventional alias used by application and test contracts.</summary>
        public string AtmId
        {
            get => ATM_ID;
            set => ATM_ID = value ?? string.Empty;
        }

        /// <summary>Current session identifier.</summary>
        public string? SessionId { get; set; }

        /// <summary>Client agent state: Running, Stopped, Degraded, etc.</summary>
        public string ClientState { get; set; } = "Unknown";

        /// <summary>Alias retained for protocol consumers that call this field AgentState.</summary>
        public string AgentState
        {
            get => ClientState;
            set => ClientState = value ?? "Unknown";
        }

        /// <summary>Number of items in the outbox queue.</summary>
        public int OutboxCount { get; set; }

        /// <summary>Number of failed transfers since last heartbeat.</summary>
        public int FailedCount { get; set; }

        /// <summary>Journal file watcher state.</summary>
        public string WatcherState { get; set; } = "inactive";

        /// <summary>Image inbox watcher state.</summary>
        public string ImageInboxState { get; set; } = "inactive";

        /// <summary>Last journal file offset read.</summary>
        public long LastJournalOffset { get; set; }

        /// <summary>CPU usage percentage.</summary>
        public int CpuPercent { get; set; }

        /// <summary>Memory usage in MB.</summary>
        public int MemoryMb { get; set; }

        /// <summary>Free disk space in MB.</summary>
        public int DiskFreeMb { get; set; }

        /// <summary>Last error message if any.</summary>
        public string? LastError { get; set; }

        /// <summary>UTC timestamp of the heartbeat.</summary>
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Cryptographic nonce for replay protection.</summary>
        public string Nonce { get; set; } = HandshakeRequest.GenerateNonce();

        /// <summary>Number of successful syncs since last heartbeat.</summary>
        public int SuccessCount { get; set; }

        /// <summary>Total bytes sent since last heartbeat.</summary>
        public long BytesSent { get; set; }

        /// <summary>Total bytes received since last heartbeat.</summary>
        public long BytesReceived { get; set; }

        /// <summary>Operational mode of the ATM.</summary>
        public string OperationalMode { get; set; } = "Unknown";

        /// <summary>HMAC-SHA256 signature.</summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>Serialize for signing.</summary>
        public string ToSignableString()
            => $"{ATM_ID}|{SessionId}|{Nonce}|{TimestampUtc:O}|{ClientState}";

        /// <summary>Sign with HMAC-SHA256.</summary>
        public void Sign(byte[] secretKey)
        {
            var data = Encoding.UTF8.GetBytes(ToSignableString());
            using var hmac = new HMACSHA256(secretKey);
            Signature = Convert.ToBase64String(hmac.ComputeHash(data));
        }

        /// <summary>Verify signature.</summary>
        public bool VerifySignature(byte[] secretKey)
        {
            if (string.IsNullOrEmpty(Signature)) return false;
            var data = Encoding.UTF8.GetBytes(ToSignableString());
            using var hmac = new HMACSHA256(secretKey);
            var computed = Convert.ToBase64String(hmac.ComputeHash(data));
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(Signature),
                Encoding.UTF8.GetBytes(computed));
        }
    }

    /// <summary>Server-to-client heartbeat acknowledgment.</summary>
    public sealed class HeartbeatAck
    {
        /// <summary>Server UTC timestamp.</summary>
        public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Number of pending commands for this ATM.</summary>
        public int PendingCommandCount { get; set; }

        /// <summary>Whether the server requests an immediate sync.</summary>
        public bool RequestImmediateSync { get; set; }

        /// <summary>Server-assigned nonce for this ack.</summary>
        public string ServerNonce { get; set; } = HandshakeRequest.GenerateNonce();

        /// <summary>Whether the server considers the client healthy.</summary>
        public bool ClientHealthy { get; set; } = true;

        /// <summary>Optional server message.</summary>
        public string? ServerMessage { get; set; }
    }

    // ===== Transfer Session =====

    /// <summary>Transport message for file chunk transfer.</summary>
    public sealed class TransportMessage
    {
        /// <summary>Message type: StartFile, FileChunk, ChunkAck, ChunkNak, CompleteFile, VerifyFile.</summary>
        public string MessageType { get; set; } = string.Empty;

        /// <summary>Session identifier.</summary>
        public string? SessionId { get; set; }

        /// <summary>Transfer session identifier.</summary>
        public string? TransferId { get; set; }

        /// <summary>File name being transferred.</summary>
        public string? FileName { get; set; }

        /// <summary>Total file size in bytes.</summary>
        public long FileSize { get; set; }

        /// <summary>Current chunk index (1-based).</summary>
        public int ChunkIndex { get; set; }

        /// <summary>Total number of chunks.</summary>
        public int TotalChunks { get; set; }

        /// <summary>Chunk size in bytes.</summary>
        public int ChunkSize { get; set; }

        /// <summary>Chunk payload bytes.</summary>
        public byte[]? Payload { get; set; }

        /// <summary>SHA256 checksum of the complete file.</summary>
        public string? Checksum { get; set; }

        /// <summary>SHA256 checksum of this chunk.</summary>
        public string? ChunkChecksum { get; set; }

        /// <summary>Nonce for this message.</summary>
        public string Nonce { get; set; } = HandshakeRequest.GenerateNonce();

        /// <summary>File manifest identifier for tracking.</summary>
        public string? ManifestId { get; set; }

        /// <summary>Source path on the client.</summary>
        public string? SourcePath { get; set; }

        /// <summary>Retry count for this chunk.</summary>
        public int RetryCount { get; set; }

        /// <summary>Whether this is the last chunk of the file.</summary>
        public bool IsLastChunk { get; set; }

        /// <summary>Compute SHA256 checksum of the payload.</summary>
        public static string ComputeChecksum(byte[] data)
        {
            var hash = SHA256.HashData(data);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    /// <summary>
    /// Transfer session tracking model.
    /// Manages the lifecycle of a single file transfer with chunking and retry.
    /// </summary>
    public sealed class TransferSession
    {
        /// <summary>Unique transfer identifier.</summary>
        public string TransferId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>File name being transferred.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>Total file size in bytes.</summary>
        public long FileSize { get; set; }

        /// <summary>Total number of chunks.</summary>
        public int TotalChunks { get; set; }

        /// <summary>Chunk size in bytes.</summary>
        public int ChunkSize { get; set; } = 65536; // 64KB default

        /// <summary>SHA256 checksum of the complete file.</summary>
        public string Checksum { get; set; } = string.Empty;

        /// <summary>Source path on the client.</summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>Destination path on the server.</summary>
        public string DestinationPath { get; set; } = string.Empty;

        /// <summary>Current transfer state.</summary>
        public TransferState State { get; set; } = TransferState.Pending;

        /// <summary>Number of successfully transferred chunks.</summary>
        public int CompletedChunks { get; set; }

        /// <summary>Indexes confirmed by the receiver, used for resumable transfers.</summary>
        public HashSet<int> ReceivedChunks { get; set; } = new();

        /// <summary>Whether every expected chunk has been confirmed.</summary>
        public bool IsComplete => TotalChunks > 0 && ReceivedChunks.Count >= TotalChunks;

        /// <summary>First missing zero-based chunk offset.</summary>
        public int NextExpectedOffset
        {
            get
            {
                for (var index = 0; index < TotalChunks; index++)
                {
                    if (!ReceivedChunks.Contains(index))
                        return index;
                }

                return TotalChunks;
            }
        }

        /// <summary>Number of failed chunk transfers.</summary>
        public int FailedChunks { get; set; }

        /// <summary>Total retries across all chunks.</summary>
        public int TotalRetries { get; set; }

        /// <summary>UTC timestamp when the transfer started.</summary>
        public DateTime StartedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp when the transfer completed (or failed).</summary>
        public DateTime? CompletedUtc { get; set; }

        /// <summary>Bytes transferred so far.</summary>
        public long BytesTransferred { get; set; }

        /// <summary>Percent complete (0.0 to 100.0).</summary>
        public double ProgressPercent => FileSize > 0 ? (double)BytesTransferred / FileSize * 100.0 : 0.0;

        /// <summary>Whether verification passed.</summary>
        public bool VerificationPassed { get; set; }

        /// <summary>ATM identifier for this transfer.</summary>
        public string ATM_ID { get; set; } = string.Empty;

        /// <summary>File manifest identifier.</summary>
        public string ManifestId { get; set; } = Guid.NewGuid().ToString("N");
    }

    /// <summary>Transfer session states.</summary>
    public enum TransferState
    {
        Pending,
        InProgress,
        Completed,
        Verified,
        Failed,
        Cancelled,
        Duplicate
    }

    /// <summary>File manifest for tracking transferred files.</summary>
    public sealed class FileManifest
    {
        /// <summary>Unique manifest identifier.</summary>
        public string ManifestId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>ATM identifier.</summary>
        public string ATM_ID { get; set; } = string.Empty;

        /// <summary>File name.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>File size in bytes.</summary>
        public long FileSize { get; set; }

        /// <summary>SHA256 checksum.</summary>
        public string Checksum { get; set; } = string.Empty;

        /// <summary>File type: Journal, Log, Trace, Image, Content, Diagnostic.</summary>
        public string FileType { get; set; } = string.Empty;

        /// <summary>Source path on the client.</summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>Number of chunks expected.</summary>
        public int TotalChunks { get; set; }

        /// <summary>Chunk size used.</summary>
        public int ChunkSize { get; set; }

        /// <summary>UTC timestamp when the manifest was created.</summary>
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of the last modification.</summary>
        public DateTime LastModifiedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Transfer status.</summary>
        public string Status { get; set; } = "Pending";

        /// <summary>Failure reason if applicable.</summary>
        public string? FailureReason { get; set; }

        /// <summary>Number of retries attempted.</summary>
        public int RetryCount { get; set; }
    }

    // ===== Reconnection & Resilience =====

    /// <summary>Reconnection policy configuration.</summary>
    public sealed class ReconnectPolicy
    {
        /// <summary>Maximum number of reconnection attempts.</summary>
        public int MaxRetries { get; set; } = 10;

        /// <summary>Base delay in milliseconds.</summary>
        public int BaseDelayMs { get; set; } = 1000;

        /// <summary>Maximum delay in milliseconds.</summary>
        public int MaxDelayMs { get; set; } = 60000;

        /// <summary>Whether to use jitter.</summary>
        public bool UseJitter { get; set; } = true;

        /// <summary>Jitter factor (0.0 to 1.0).</summary>
        public double JitterFactor { get; set; } = 0.3;
    }

    /// <summary>Exponential backoff with optional jitter.</summary>
    public sealed class ExponentialBackoffWithJitter
    {
        private readonly Random _random = new();
        private int _attempt;
        private readonly double _jitterFactor;

        public ExponentialBackoffWithJitter(double jitterFactor = 0.3)
        {
            _jitterFactor = jitterFactor;
        }

        /// <summary>Get the next delay based on attempt count.</summary>
        public TimeSpan NextDelay()
        {
            _attempt++;
            var baseMs = 1000.0 * Math.Pow(2, Math.Min(_attempt - 1, 6));
            var jitter = _random.NextDouble() * baseMs * _jitterFactor;
            return TimeSpan.FromMilliseconds(Math.Min(baseMs + jitter, 60000));
        }

        /// <summary>Get the current attempt number.</summary>
        public int CurrentAttempt => _attempt;

        /// <summary>Reset the attempt counter.</summary>
        public void Reset() => _attempt = 0;
    }

    /// <summary>Circuit breaker pattern implementation.</summary>
    public sealed class CircuitBreaker
    {
        /// <summary>Circuit breaker states.</summary>
        public enum State { Closed, Open, HalfOpen }

        /// <summary>Current state of the circuit breaker.</summary>
        public State CurrentState { get; private set; } = State.Closed;

        private int _failureCount;
        private DateTime _lastFailureUtc;
        private readonly int _failureThreshold;
        private readonly TimeSpan _resetTimeout;
        private readonly object _lock = new();

        /// <summary>
        /// Create a new circuit breaker.
        /// </summary>
        /// <param name="failureThreshold">Number of consecutive failures before opening the circuit.</param>
        /// <param name="resetTimeoutSec">Seconds to wait before transitioning from Open to HalfOpen.</param>
        public CircuitBreaker(int failureThreshold = 5, int resetTimeoutSec = 30)
        {
            _failureThreshold = failureThreshold;
            _resetTimeout = TimeSpan.FromSeconds(resetTimeoutSec);
        }

        /// <summary>Check if a request is allowed through.</summary>
        public bool AllowRequest()
        {
            lock (_lock)
            {
                return CurrentState switch
                {
                    State.Closed => true,
                    State.Open when DateTime.UtcNow - _lastFailureUtc > _resetTimeout =>
                        (CurrentState = State.HalfOpen) == State.HalfOpen, // returns true
                    State.Open => false,
                    State.HalfOpen => true,
                    _ => false
                };
            }
        }

        /// <summary>Record a successful request.</summary>
        public void RecordSuccess()
        {
            lock (_lock)
            {
                _failureCount = 0;
                CurrentState = State.Closed;
            }
        }

        /// <summary>Record a failed request.</summary>
        public void RecordFailure()
        {
            lock (_lock)
            {
                _failureCount++;
                _lastFailureUtc = DateTime.UtcNow;
                if (_failureCount >= _failureThreshold)
                    CurrentState = State.Open;
            }
        }

        /// <summary>Current failure count.</summary>
        public int FailureCount { get { lock (_lock) { return _failureCount; } } }
    }

    // ===== Session Registry Entry =====

    /// <summary>Client session entry for server-side tracking.</summary>
    public sealed class ClientSessionEntry
    {
        /// <summary>ATM identifier.</summary>
        public string ATM_ID { get; set; } = string.Empty;

        /// <summary>Session identifier.</summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>IP address of the client.</summary>
        public string RemoteAddress { get; set; } = string.Empty;

        /// <summary>Client version.</summary>
        public string ClientVersion { get; set; } = string.Empty;

        /// <summary>Protocol version.</summary>
        public string ProtocolVersion { get; set; } = string.Empty;

        /// <summary>UTC timestamp when the session was established.</summary>
        public DateTime ConnectedUtc { get; set; } = DateTime.UtcNow;

        /// <summary>UTC timestamp of the last heartbeat.</summary>
        public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Number of pending commands for this client.</summary>
        public int PendingCommands { get; set; }

        /// <summary>Total bytes sent by this client.</summary>
        public long TotalBytesSent { get; set; }

        /// <summary>Total bytes received by this client.</summary>
        public long TotalBytesReceived { get; set; }

        /// <summary>Whether the client is marked as healthy.</summary>
        public bool IsHealthy { get; set; } = true;
    }
}
