using System.Net.NetworkInformation;
using EJLive.Client.WinForms.Services;
using EJLive.Core.Engine;
using CoreNetworkEngine = EJLive.Core.Engine.NetworkEngine;

namespace EJLive.Client.WinForms.Agent;

/// <summary>
/// Monitors reachability of the configured server host.
/// </summary>
public sealed class NetworkMonitor : IDisposable
{
    private readonly Func<bool> _isConnected;
    private readonly string _serverIp;
    private readonly string _atmId;
    private readonly Action<string, string>? _eventSink;
    private System.Threading.Timer? _timer;
    private bool _wasReachable = true;
    private DateTime _disconnectedAt;

    public event Action<string>? OnLog;
    public int DisconnectCount { get; private set; }
    public bool IsReachable { get; private set; } = true;
    public bool IsConnected => _isConnected();

    public NetworkMonitor(NetworkManager? network, string serverIp, string atmId, Action<string, string>? eventSink = null)
    {
        _isConnected = () => network?.IsConnected == true;
        _serverIp = serverIp ?? string.Empty;
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        _eventSink = eventSink;
    }

    public NetworkMonitor(CoreNetworkEngine? network, string serverIp, string atmId, Action<string, string>? eventSink = null)
    {
        _isConnected = () => network?.IsConnected == true;
        _serverIp = serverIp ?? string.Empty;
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        _eventSink = eventSink;
    }

    public void Start()
    {
        _timer = new System.Threading.Timer(_ => Check(), null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private void Check()
    {
        var reachable = PingHost(_serverIp);
        IsReachable = reachable;

        if (_wasReachable && !reachable)
        {
            _wasReachable = false;
            _disconnectedAt = DateTime.UtcNow;
            DisconnectCount++;
            var detail = $"Disconnect #{DisconnectCount} from {_serverIp} ({_atmId})";
            OnLog?.Invoke(detail);
            _eventSink?.Invoke("network_disconnect", detail);
            return;
        }

        if (!_wasReachable && reachable)
        {
            _wasReachable = true;
            var duration = DateTime.UtcNow - _disconnectedAt;
            var detail = $"Reconnect after {duration.TotalSeconds:F0}s outage";
            OnLog?.Invoke(detail);
            _eventSink?.Invoke("network_reconnect", detail);
        }
    }

    private static bool PingHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        try
        {
            using var ping = new Ping();
            return ping.Send(host, 3000)?.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
