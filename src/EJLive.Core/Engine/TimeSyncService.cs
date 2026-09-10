namespace EJLive.Core.Engine;

/// <summary>
/// Provides time synchronization between ATM clients and the central server.
/// Uses NTP-like offset calculation to align timestamps across the fleet
/// without requiring direct NTP access from the ATM.
/// </summary>
public sealed class TimeSyncService
{
    private readonly object _lock = new();
    private TimeSpan _offset = TimeSpan.Zero;
    private DateTime _lastSyncUtc = DateTime.MinValue;
    private int _syncCount;

    /// <summary>
    /// Gets the computed offset between local and server time.
    /// Positive means the server clock is ahead of the local clock.
    /// </summary>
    public TimeSpan Offset
    {
        get { lock (_lock) return _offset; }
    }

    /// <summary>
    /// Gets the UTC timestamp of the last successful sync.
    /// </summary>
    public DateTime LastSyncUtc
    {
        get { lock (_lock) return _lastSyncUtc; }
    }

    /// <summary>
    /// Gets the number of completed syncs.
    /// </summary>
    public int SyncCount
    {
        get { lock (_lock) return _syncCount; }
    }

    /// <summary>
    /// Records a time sync exchange with the server.
    /// Uses a simplified NTP algorithm: offset = ((T2 - T1) + (T3 - T4)) / 2.
    /// </summary>
    /// <param name="clientSendUtc">T1: Client send timestamp.</param>
    /// <param name="serverReceiveUtc">T2: Server receive timestamp.</param>
    /// <param name="serverSendUtc">T3: Server send timestamp.</param>
    /// <param name="clientReceiveUtc">T4: Client receive timestamp.</param>
    public void RecordExchange(
        DateTime clientSendUtc,
        DateTime serverReceiveUtc,
        DateTime serverSendUtc,
        DateTime clientReceiveUtc)
    {
        clientSendUtc = NormalizeUtc(clientSendUtc, nameof(clientSendUtc));
        serverReceiveUtc = NormalizeUtc(serverReceiveUtc, nameof(serverReceiveUtc));
        serverSendUtc = NormalizeUtc(serverSendUtc, nameof(serverSendUtc));
        clientReceiveUtc = NormalizeUtc(clientReceiveUtc, nameof(clientReceiveUtc));

        if (clientReceiveUtc < clientSendUtc)
            throw new ArgumentException("Client receive timestamp precedes client send timestamp.", nameof(clientReceiveUtc));
        if (serverSendUtc < serverReceiveUtc)
            throw new ArgumentException("Server send timestamp precedes server receive timestamp.", nameof(serverSendUtc));

        var offsetA = serverReceiveUtc - clientSendUtc;
        var offsetB = serverSendUtc - clientReceiveUtc;
        var computedOffset = TimeSpan.FromTicks((offsetA.Ticks / 2) + (offsetB.Ticks / 2));
        if (computedOffset.Duration() > TimeSpan.FromHours(24))
            throw new ArgumentOutOfRangeException(nameof(serverReceiveUtc), "Computed clock skew exceeds the 24-hour safety limit.");

        lock (_lock)
        {
            // Exponential moving average for stability
            if (_syncCount == 0)
            {
                _offset = computedOffset;
            }
            else
            {
                const double alpha = 0.3;
                _offset = TimeSpan.FromTicks(
                    (long)(_offset.Ticks * (1 - alpha) + computedOffset.Ticks * alpha));
            }

            _lastSyncUtc = DateTime.UtcNow;
            _syncCount++;
        }
    }

    /// <summary>
    /// Records a simple sync using a server timestamp and the current local time.
    /// </summary>
    /// <param name="serverUtcNow">The server's current UTC time.</param>
    public void RecordSimpleSync(DateTime serverUtcNow)
    {
        var localNow = DateTime.UtcNow;
        RecordExchange(localNow, serverUtcNow, serverUtcNow, localNow);
    }

    /// <summary>
    /// Converts a local UTC timestamp to estimated server time using the computed offset.
    /// </summary>
    public DateTime ToServerTime(DateTime localUtc)
    {
        localUtc = NormalizeUtc(localUtc, nameof(localUtc));
        lock (_lock)
            return localUtc + _offset;
    }

    /// <summary>
    /// Converts a server UTC timestamp to estimated local time using the computed offset.
    /// </summary>
    public DateTime ToLocalTime(DateTime serverUtc)
    {
        serverUtc = NormalizeUtc(serverUtc, nameof(serverUtc));
        lock (_lock)
            return serverUtc - _offset;
    }

    /// <summary>
    /// Gets the current time adjusted to server reference.
    /// </summary>
    public DateTime NowServerAligned
    {
        get
        {
            lock (_lock)
                return DateTime.UtcNow + _offset;
        }
    }

    /// <summary>
    /// Builds a TIME_SYNC protocol message for sending to the server.
    /// </summary>
    public static string BuildTimeSyncRequest(string atmId)
    {
        return $"TIME_SYNC_REQ|ATM={NormalizeAtmId(atmId)};T1={DateTime.UtcNow:O}";
    }

    /// <summary>
    /// Builds a TIME_SYNC response from the server side.
    /// </summary>
    public static string BuildTimeSyncResponse(string atmId, DateTime t1)
    {
        t1 = NormalizeUtc(t1, nameof(t1));
        var t2 = DateTime.UtcNow;
        return $"TIME_SYNC_RSP|ATM={NormalizeAtmId(atmId)};T1={t1:O};T2={t2:O};T3={t2:O}";
    }

    private static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
            throw new ArgumentException("Timestamp is required.", parameterName);

        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static string NormalizeAtmId(string atmId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(atmId);
        var value = atmId.Trim();
        if (value.IndexOfAny(['|', ';', '\r', '\n']) >= 0)
            throw new ArgumentException("ATM identifier contains protocol delimiters.", nameof(atmId));
        return value;
    }
}
