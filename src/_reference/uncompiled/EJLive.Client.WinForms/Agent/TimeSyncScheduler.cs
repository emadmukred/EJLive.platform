using System.Diagnostics;
using EJLive.Client.WinForms.Services;
using EJLive.Core.Engine;
using CoreNetworkEngine = EJLive.Core.Engine.NetworkEngine;

namespace EJLive.Client.WinForms.Agent;

/// <summary>
/// Sends periodic time-sync requests and can parse time-sync responses.
/// </summary>
public sealed class TimeSyncScheduler : IDisposable
{
    private readonly Func<bool> _isConnected;
    private readonly Action<string> _sendText;
    private readonly string _atmId;
    private readonly string _serverIp;
    private System.Threading.Timer? _timer;

    public event Action<string>? OnLog;
    public DateTime LastSync { get; private set; }

    public TimeSyncScheduler(NetworkManager? network, string atmId, string serverIp)
    {
        _isConnected = () => network?.IsConnected == true;
        _sendText = text => network?.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, text));
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        _serverIp = serverIp ?? string.Empty;
    }

    public TimeSyncScheduler(CoreNetworkEngine? network, string atmId, string serverIp)
    {
        _isConnected = () => network?.IsConnected == true;
        _sendText = text => network?.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, text));
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
        _serverIp = serverIp ?? string.Empty;
    }

    public void Start()
    {
        _timer = new System.Threading.Timer(_ => Sync(), null, TimeSpan.FromMinutes(5), TimeSpan.FromHours(1));
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public void HandleResponse(string message)
    {
        var parts = (message ?? string.Empty).Split('|', StringSplitOptions.None);
        if (parts.Length < 2 || !DateTime.TryParse(parts[1], out var serverTime))
            return;

        var driftMs = Math.Abs((DateTime.UtcNow - serverTime.ToUniversalTime()).TotalMilliseconds);
        OnLog?.Invoke($"Time drift: {driftMs:F0} ms");
    }

    private void Sync()
    {
        if (_isConnected())
            _sendText($"TIME_SYNC_REQUEST|{_atmId}|{DateTime.UtcNow:O}");

        TryW32TmSync();
        LastSync = DateTime.UtcNow;
        OnLog?.Invoke($"Time sync tick at {LastSync:HH:mm:ss} UTC");
    }

    private void TryW32TmSync()
    {
        if (string.IsNullOrWhiteSpace(_serverIp))
            return;

        try
        {
            using var process = Process.Start(new ProcessStartInfo("w32tm", $"/resync /computer:{_serverIp}")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false
            });
            process?.WaitForExit(5000);
        }
        catch
        {
            // best effort
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
