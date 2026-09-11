using EJLive.Client.WinForms.Services;
using EJLive.Core.Engine;
using CoreNetworkEngine = EJLive.Core.Engine.NetworkEngine;

namespace EJLive.Client.WinForms.Agent;

/// <summary>
/// Detects startup/boot and sends one boot notification when the network is available.
/// </summary>
public sealed class BootNotifier
{
    private readonly Func<bool> _isConnected;
    private readonly Action<string> _sendText;
    private readonly string _atmId;
    private volatile bool _sent;

    public BootNotifier(NetworkManager? network, string atmId)
    {
        _isConnected = () => network?.IsConnected == true;
        _sendText = text => network?.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, text));
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
    }

    public BootNotifier(CoreNetworkEngine? network, string atmId)
    {
        _isConnected = () => network?.IsConnected == true;
        _sendText = text => network?.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, text));
        _atmId = string.IsNullOrWhiteSpace(atmId) ? "UNKNOWN" : atmId.Trim();
    }

    public void SendBootNotification()
    {
        if (_sent)
            return;

        _ = Task.Run(async () =>
        {
            var deadline = DateTime.UtcNow.AddSeconds(60);
            while (DateTime.UtcNow < deadline && !_sent)
            {
                if (_isConnected())
                {
                    var eventType = IsWindowsRestart() ? "windows_restart" : "agent_start";
                    _sendText($"BOOT_NOTIFY|{_atmId}|{eventType}|{DateTime.UtcNow:O}|{Environment.MachineName}");
                    _sent = true;
                    return;
                }
                await Task.Delay(2000).ConfigureAwait(false);
            }
        });
    }

    public void ResendIfPending()
    {
        if (!_sent)
            SendBootNotification();
    }

    private static bool IsWindowsRestart()
    {
        return Environment.TickCount64 < 300_000;
    }
}
