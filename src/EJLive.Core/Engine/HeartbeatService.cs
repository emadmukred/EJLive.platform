using System.Collections.Concurrent;
using System.Text.Json;

namespace EJLive.Core.Engine;

/// <summary>
/// Manages heartbeat/pulse reliability for ATM client agents.
/// Tracks last-seen times, supports exponential backoff on reconnect,
/// and produces structured pulse JSON for telemetry.
/// </summary>
public sealed class HeartbeatService : IDisposable
{
    private readonly ConcurrentDictionary<string, AtmHeartbeatState> _states = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _interval;
    private readonly TimeSpan _warningThreshold;
    private readonly TimeSpan _criticalThreshold;
    private System.Threading.Timer? _checkTimer;
    private bool _disposed;

    /// <summary>Raised when an ATM transitions to warning (no heartbeat for warningThreshold).</summary>
    public event Action<string, AtmHeartbeatState>? OnWarning;

    /// <summary>Raised when an ATM transitions to critical (no heartbeat for criticalThreshold).</summary>
    public event Action<string, AtmHeartbeatState>? OnCritical;

    /// <summary>Raised when an ATM comes back online after being offline.</summary>
    public event Action<string, AtmHeartbeatState>? OnRecovered;

    /// <summary>
    /// Initializes the heartbeat service with configurable thresholds.
    /// </summary>
    /// <param name="interval">Heartbeat check interval. Default 30 seconds.</param>
    /// <param name="warningThreshold">Time without heartbeat to raise warning. Default 2 minutes.</param>
    /// <param name="criticalThreshold">Time without heartbeat to raise critical. Default 5 minutes.</param>
    public HeartbeatService(
        TimeSpan? interval = null,
        TimeSpan? warningThreshold = null,
        TimeSpan? criticalThreshold = null)
    {
        _interval = interval ?? TimeSpan.FromSeconds(30);
        _warningThreshold = warningThreshold ?? TimeSpan.FromMinutes(2);
        _criticalThreshold = criticalThreshold ?? TimeSpan.FromMinutes(5);

        if (_interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval), "Check interval must be positive.");
        if (_warningThreshold <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(warningThreshold), "Warning threshold must be positive.");
        if (_criticalThreshold <= _warningThreshold)
            throw new ArgumentOutOfRangeException(nameof(criticalThreshold), "Critical threshold must exceed the warning threshold.");
    }

    /// <summary>
    /// Starts the periodic heartbeat check timer.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _checkTimer?.Dispose();
        _checkTimer = new System.Threading.Timer(_ => CheckHeartbeats(), null, _interval, _interval);
    }

    /// <summary>
    /// Records a heartbeat received from an ATM client.
    /// </summary>
    /// <param name="atmId">The ATM identifier.</param>
    /// <param name="serviceState">Current service state reported by client.</param>
    /// <param name="handshakeComplete">Whether handshake is complete.</param>
    /// <param name="pendingOutbox">Number of pending outbox items.</param>
    /// <param name="networkType">Network type (LAN, WAN, VPN).</param>
    public void RecordHeartbeat(
        string atmId,
        string serviceState = "connected",
        bool handshakeComplete = true,
        int pendingOutbox = 0,
        string networkType = "LAN")
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        if (pendingOutbox < 0)
            throw new ArgumentOutOfRangeException(nameof(pendingOutbox));

        var now = DateTime.UtcNow;
        var key = atmId.Trim();
        var state = _states.GetOrAdd(key, id => new AtmHeartbeatState(id));

        bool recovered;
        AtmHeartbeatState snapshot;
        lock (state.SyncRoot)
        {
            recovered = state.Level is HeartbeatLevel.Critical or HeartbeatLevel.Warning;
            state.LastHeartbeatUtc = now;
            state.ServiceState = string.IsNullOrWhiteSpace(serviceState) ? "unknown" : serviceState.Trim();
            state.HandshakeComplete = handshakeComplete;
            state.PendingOutbox = pendingOutbox;
            state.NetworkType = string.IsNullOrWhiteSpace(networkType) ? "unknown" : networkType.Trim();
            state.ConsecutiveMisses = 0;
            state.Level = HeartbeatLevel.Healthy;
            snapshot = state.Copy();
        }

        if (recovered)
            OnRecovered?.Invoke(key, snapshot);
    }

    /// <summary>
    /// Builds a pulse JSON payload for sending to the server.
    /// </summary>
    public string BuildPulseJson(
        string terminalId,
        string serviceState,
        bool handshake,
        int pendingOutbox,
        string networkType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(terminalId);
        if (pendingOutbox < 0)
            throw new ArgumentOutOfRangeException(nameof(pendingOutbox));

        var payload = new
        {
            terminalId,
            timestampUtc = DateTime.UtcNow.ToString("O"),
            serviceState,
            handshake,
            pendingOutbox,
            networkType
        };
        return JsonSerializer.Serialize(payload);
    }

    /// <summary>
    /// Gets the current heartbeat state for all tracked ATMs.
    /// </summary>
    public IReadOnlyDictionary<string, AtmHeartbeatState> GetAllStates() =>
        _states.ToDictionary(pair => pair.Key, pair => Snapshot(pair.Value), StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the state for a specific ATM.
    /// </summary>
    public AtmHeartbeatState? GetState(string atmId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        return _states.TryGetValue(atmId.Trim(), out var state) ? Snapshot(state) : null;
    }

    /// <summary>
    /// Computes exponential backoff delay for reconnection attempts.
    /// </summary>
    /// <param name="consecutiveFailures">Number of consecutive connection failures.</param>
    /// <param name="baseDelayMs">Base delay in milliseconds. Default 1000ms.</param>
    /// <param name="maxDelayMs">Maximum delay in milliseconds. Default 60000ms.</param>
    /// <returns>The computed delay.</returns>
    public static TimeSpan ComputeBackoff(int consecutiveFailures, int baseDelayMs = 1000, int maxDelayMs = 60000)
    {
        if (baseDelayMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(baseDelayMs));
        if (maxDelayMs < baseDelayMs)
            throw new ArgumentOutOfRangeException(nameof(maxDelayMs), "Maximum delay must not be less than base delay.");

        if (consecutiveFailures <= 0)
            return TimeSpan.FromMilliseconds(baseDelayMs);

        var delay = baseDelayMs * Math.Pow(2, Math.Min(consecutiveFailures, 10));
        var jitter = Random.Shared.Next(0, baseDelayMs / 2);
        return TimeSpan.FromMilliseconds(Math.Min(delay + jitter, maxDelayMs));
    }

    private void CheckHeartbeats()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _states)
        {
            var state = kvp.Value;
            HeartbeatLevel? transition = null;
            AtmHeartbeatState? snapshot = null;
            lock (state.SyncRoot)
            {
                var elapsed = now - state.LastHeartbeatUtc;
                if (elapsed >= _criticalThreshold && state.Level != HeartbeatLevel.Critical)
                {
                    state.Level = HeartbeatLevel.Critical;
                    state.ConsecutiveMisses++;
                    transition = HeartbeatLevel.Critical;
                }
                else if (elapsed >= _warningThreshold && state.Level == HeartbeatLevel.Healthy)
                {
                    state.Level = HeartbeatLevel.Warning;
                    state.ConsecutiveMisses++;
                    transition = HeartbeatLevel.Warning;
                }

                if (transition.HasValue)
                    snapshot = state.Copy();
            }

            if (transition == HeartbeatLevel.Critical)
                OnCritical?.Invoke(kvp.Key, snapshot!);
            else if (transition == HeartbeatLevel.Warning)
                OnWarning?.Invoke(kvp.Key, snapshot!);
        }
    }

    private static AtmHeartbeatState Snapshot(AtmHeartbeatState state)
    {
        lock (state.SyncRoot)
            return state.Copy();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _checkTimer?.Dispose();
    }
}

/// <summary>
/// Represents the heartbeat state of a single ATM.
/// </summary>
public sealed class AtmHeartbeatState
{
    public AtmHeartbeatState(string atmId) => AtmId = atmId;

    public string AtmId { get; }
    public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;
    public string ServiceState { get; set; } = "unknown";
    public bool HandshakeComplete { get; set; }
    public int PendingOutbox { get; set; }
    public string NetworkType { get; set; } = "LAN";
    public HeartbeatLevel Level { get; set; } = HeartbeatLevel.Healthy;
    public int ConsecutiveMisses { get; set; }

    internal object SyncRoot { get; } = new();

    internal AtmHeartbeatState Copy() => new(AtmId)
    {
        LastHeartbeatUtc = LastHeartbeatUtc,
        ServiceState = ServiceState,
        HandshakeComplete = HandshakeComplete,
        PendingOutbox = PendingOutbox,
        NetworkType = NetworkType,
        Level = Level,
        ConsecutiveMisses = ConsecutiveMisses
    };
}

/// <summary>
/// Heartbeat health level for an ATM.
/// </summary>
public enum HeartbeatLevel
{
    /// <summary>Heartbeat received within expected interval.</summary>
    Healthy,
    /// <summary>Heartbeat delayed beyond warning threshold.</summary>
    Warning,
    /// <summary>Heartbeat missing beyond critical threshold.</summary>
    Critical
}
