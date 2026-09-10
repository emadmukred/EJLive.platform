using EJLive.Core.Engine;

namespace EJLive.Client.Service;

/// <summary>
/// Publishes time-sync requests and measures drift from server responses.
/// OS clock mutation remains outside this scheduler and requires an approved policy path.
/// </summary>
public sealed class TimeSyncScheduler : IDisposable
{
    private readonly NetworkEngine? _network;
    private readonly string _atmId;
    private System.Threading.Timer? _timer;

    public event Action<string>? OnLog;
    public DateTime LastRequestUtc { get; private set; }

    public TimeSyncScheduler(NetworkEngine? network, string atmId)
    {
        _network = network;
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
    }

    public void Start() => _timer ??= new System.Threading.Timer(
        _ => SendRequest(),
        null,
        TimeSpan.FromMinutes(5),
        TimeSpan.FromHours(1));

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void HandleResponse(string message)
    {
        var parts = (message ?? string.Empty).Split('|', StringSplitOptions.None);
        if (parts.Length < 2 ||
            !string.Equals(parts[0], "TIME_SYNC_RESP", StringComparison.OrdinalIgnoreCase) ||
            !DateTime.TryParse(parts[1], null, System.Globalization.DateTimeStyles.RoundtripKind, out var serverTime))
        {
            return;
        }

        var driftMilliseconds = Math.Abs((DateTime.UtcNow - serverTime.ToUniversalTime()).TotalMilliseconds);
        OnLog?.Invoke($"Time drift: {driftMilliseconds:F0} ms");
    }

    public void SendRequest()
    {
        LastRequestUtc = DateTime.UtcNow;
        if (_network?.IsConnected == true)
        {
            _network.SendMessage(CommunicationProtocol.BuildFrame(
                CommunicationProtocol.MsgType.Broadcast,
                $"TIME_SYNC_REQUEST|{_atmId}|{LastRequestUtc:O}"));
        }
        OnLog?.Invoke($"Time sync request at {LastRequestUtc:HH:mm:ss} UTC");
    }

    public void Dispose() => Stop();
}
