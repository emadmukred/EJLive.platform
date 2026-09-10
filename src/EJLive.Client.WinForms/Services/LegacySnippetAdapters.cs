using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using EJLive.Client.WinForms.Agent;
using EJLive.Client.WinForms.Services;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using AppConfig = EJLive.Shared.AppConfig;

namespace EJLive.Client.Engine
{

/// <summary>
/// Compatibility adapter for legacy snippet references.
/// Delegates to the active hardened policy enforcer without replacing current architecture.
/// </summary>
public sealed class WindowsPolicyEnforcer
{
    private readonly EJLive.Client.WinForms.Services.WindowsPolicyEnforcer _inner;

    public WindowsPolicyEnforcer()
        : this(null)
    {
    }

    public WindowsPolicyEnforcer(Func<AppConfig>? configAccessor)
    {
        _inner = new EJLive.Client.WinForms.Services.WindowsPolicyEnforcer(configAccessor);
    }

    public void ApplySovereignty() => _ = _inner.EnforceBaseline();
    public void ApplyForcedConfiguration() => _ = _inner.ApplyForcedConfiguration();
    public bool IsRunningAsSystem() => _inner.IsSystemElevated();
    public bool IsSystemElevated() => _inner.IsSystemElevated();
}

/// <summary>
/// Compatibility adapter for legacy remote assistance operations.
/// Uses active safe implementations (policy-checked shadow command + local password API).
/// </summary>
public sealed class RemoteAssistanceEngine
{
    private readonly EJLive.Core.Services.RemoteDiagnosticCommandService _remoteDiagnostics = new();
    private readonly object _terminalSync = new();
    private bool _terminalInitialized;
    private string _lastTerminalOutput = string.Empty;

    public byte[] CaptureHiddenSession()
    {
        try
        {
            var bounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1024, 768);
            using var bmp = new Bitmap(bounds.Width, bounds.Height);
            using (var g = Graphics.FromImage(bmp))
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }

    public bool ChangeAdminPassword(string username, string newPassword)
    {
        var result = WindowsRemoteAccessService.SetLocalUserPassword(username, newPassword);
        return result.Success;
    }

    public string GetRDPShadowCommand(int sessionId, string ip)
    {
        return WindowsRemoteAccessService.GenerateShadowCommandString(
            ip,
            sessionId,
            control: true,
            requestNoConsentPrompt: true,
            promptForCredentials: true,
            enforceNoConsentPolicy: true);
    }

    public void InitializeHiddenTerminal()
    {
        lock (_terminalSync)
        {
            _terminalInitialized = true;
            _lastTerminalOutput = "TERMINAL_READY";
        }
    }

    public void ExecuteScript(string command)
    {
        EnsureInitialized();
        var result = _remoteDiagnostics.ExecuteSilentShell(command, 12000);
        lock (_terminalSync)
        {
            _lastTerminalOutput = result.Output ?? string.Empty;
        }
    }

    public void ExecuteHiddenScript(string scriptContent, bool isPowerShell = true)
    {
        // Keep signature compatibility; route to the same allowlisted execution path.
        _ = isPowerShell; // Intentional: policy controls command presets, not shell type.
        ExecuteScript(scriptContent);
    }

    /// <summary>
    /// Compatibility helper: returns the latest allowlisted execution output captured by ExecuteScript.
    /// </summary>
    public string GetLastTerminalOutput()
    {
        lock (_terminalSync)
            return _lastTerminalOutput;
    }

    private void EnsureInitialized()
    {
        if (_terminalInitialized)
            return;
        InitializeHiddenTerminal();
    }
}

/// <summary>
/// Compatibility wrapper that exposes maintenance-loop style API over the active core network engine.
/// </summary>
public sealed class NetworkEngine : IDisposable
{
    private readonly EJLive.Core.Engine.NetworkEngine _inner;
    private readonly string _atmId;

    public NetworkEngine(string ip, int port, string atmId)
    {
        _atmId = string.IsNullOrWhiteSpace(atmId) ? Environment.MachineName : atmId.Trim();
        _inner = new EJLive.Core.Engine.NetworkEngine(
            ip,
            port,
            _atmId,
            AppConstants.ATM_TYPE_NCR,
            "LAN",
            null,
            NetworkTransportOptions.FromEnvironment());
    }

    public async Task StartMaintenanceLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!_inner.IsConnected)
                {
                    if (!_inner.Connect())
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), token).ConfigureAwait(false);
                        continue;
                    }

                    await PerformHandshake().ConfigureAwait(false);
                }

                var pulse = $"PULSE|{_atmId}|STATUS:OK|UTC:{DateTime.UtcNow:O}";
                _inner.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, pulse));
                await Task.Delay(TimeSpan.FromSeconds(10), token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token).ConfigureAwait(false);
            }
        }
    }

    public void Disconnect() => _inner.Disconnect();

    public void Dispose() => _inner.Dispose();

    private Task PerformHandshake()
    {
        var payload = $"HANDSHAKE|{_atmId}|SOVEREIGN:TRUE";
        _inner.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, payload));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Legacy-compatible engine controller signature.
/// Bridges constructor/start pattern to the active core controller implementation.
/// </summary>
public sealed class AgentController : IDisposable
{
    private readonly EJLive.Client.Core.AgentController _inner = new();
    private readonly JournalWatcherEngine _watcher = new();
    private readonly string _ip;
    private readonly int _port;
    private readonly string _atmId;
    private bool _started;

    public AgentController(string ip, int port, string atmId)
    {
        _ip = ip ?? string.Empty;
        _port = port;
        _atmId = atmId ?? string.Empty;
    }

    public void Start()
    {
        if (_started)
            return;

        StartWatcherBestEffort();

        _inner.Start(new EJLive.Client.Core.RuntimeAgentConfig
        {
            ServerIp = _ip,
            ServerPort = _port,
            AtmId = _atmId
        });

        _started = true;
    }

    public void Stop()
    {
        try { _watcher.Stop(); } catch { }
        _inner.Stop();
        _started = false;
    }

    public void Dispose()
    {
        Stop();
        _watcher.Dispose();
        _inner.Dispose();
    }

    private void StartWatcherBestEffort()
    {
        try
        {
            var config = AppConfig.Load();
            config.ApplyDefaults();
            var roots = new List<string>();
            if (!string.IsNullOrWhiteSpace(config.SourcePath))
                roots.Add(config.SourcePath.Trim());
            var fallback = AppConstants.GetDefaultSourcePath(config.ATM_Type);
            if (!string.IsNullOrWhiteSpace(fallback))
                roots.Add(fallback.Trim());
            _watcher.Start(roots.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
        }
        catch
        {
            // Best-effort compatibility watcher startup.
        }
    }
}

/// <summary>
/// Legacy-compatible journal watcher API.
/// Bridges to the compiled core FileWatcherEngine without changing active watcher behavior.
/// </summary>
public sealed class JournalWatcherEngine : IDisposable
{
    private readonly EJLive.Core.Engine.FileWatcherEngine _inner = new();
    public event EventHandler<string>? JournalChanged;
    public bool IsRunning => _inner.IsRunning;

    public JournalWatcherEngine()
    {
        _inner.FileChanged += (_, path) => JournalChanged?.Invoke(this, path);
    }

    public void Start(params string[] roots)
    {
        _inner.Start(roots ?? Array.Empty<string>());
    }

    public void Stop()
    {
        _inner.Stop();
    }

    public void Dispose()
    {
        _inner.Dispose();
    }
}

/// <summary>
/// Legacy-compatible shadow command helper.
/// Generates policy-aware command strings through active readiness checks.
/// </summary>
public static class RemoteSessionManager
{
    public static string GenerateShadowCommand(string targetIp, int sessionId = 1)
    {
        return WindowsRemoteAccessService.GenerateShadowCommandString(
            targetIp,
            sessionId,
            control: true,
            requestNoConsentPrompt: true,
            promptForCredentials: true,
            enforceNoConsentPolicy: true);
    }

    public static void LaunchShadowSession(string targetIp, int sessionId = 1)
    {
        var command = GenerateShadowCommand(targetIp, sessionId);
        if (command.StartsWith("ShadowCommandBlocked:", StringComparison.OrdinalIgnoreCase))
            return;

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c " + command,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            });
        }
        catch
        {
            // Best effort compatibility launch.
        }
    }
}

}

namespace EJLive.Client.Installer
{

/// <summary>
/// Legacy installer compatibility layer.
/// Delegates to active installer-security and service-registration flows.
/// </summary>
public sealed class AuthorizedInstaller
{
    public void Install()
    {
        var config = AppConfig.Load();
        config.ApplyDefaults();
        config.AutoEnableRemoteAccess = true;
        config.AutoPrepareWindowsRuntime = true;
        config.EnforceScopedFirewallRule = true;
        if (config.ScopedFirewallPort <= 0)
            config.ScopedFirewallPort = config.ServerPort > 0 ? config.ServerPort : AppConstants.DefaultPort;
        config.Save();
        EJLive.Core.Services.AgentConfigurationXmlService.SaveAppConfig(config);

        var enforcer = new EJLive.Client.WinForms.Services.WindowsPolicyEnforcer(() => config);
        _ = enforcer.EnforceBaseline();

        var serviceRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive",
            "ClientService");

        _ = WindowsServiceRegistrationService.InstallOrUpdateService(
            Path.Combine(serviceRoot, "EJLive.Client.Service.exe"));
        _ = WindowsServiceRegistrationService.StartService();
        _ = WindowsStartupService.RegisterClientAutostart(Environment.ProcessPath ?? string.Empty);
        _ = WindowsStartupService.RegisterUserSessionCompanion(Environment.ProcessPath ?? string.Empty);
        CleanupInstallerTempFiles();
    }

    public void Uninstall()
    {
        _ = WindowsServiceRegistrationService.StopService();
        _ = WindowsServiceRegistrationService.UninstallService();
        _ = WindowsStartupService.UnregisterClientAutostart();
        _ = WindowsStartupService.UnregisterUserSessionCompanion();
        RemoveLegacyRunValue();
        RemoveLegacyFirewallRules();
        CleanupInstallerTempFiles();
    }

    private static void RemoveLegacyRunValue()
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default);
            using var run = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: true);
            run?.DeleteValue("EJLiveAgent", false);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    private static void RemoveLegacyFirewallRules()
    {
        RunHidden("netsh.exe", "advfirewall firewall delete rule name=\"EJLive-Sovereign\"");
        RunHidden("netsh.exe", "advfirewall firewall delete rule name=\"EJLive_Service\"");
        RunHidden("netsh.exe", "advfirewall firewall delete rule name=\"EJLive Client Socket Inbound\"");
        RunHidden("netsh.exe", "advfirewall firewall delete rule name=\"EJLive Client Socket Outbound\"");
        RunHidden("netsh.exe", "advfirewall firewall delete rule name=\"EJLive Client Socket Inbound 5656\"");
        RunHidden("netsh.exe", "advfirewall firewall delete rule name=\"EJLive Client Socket Outbound 5656\"");
    }

    private static void CleanupInstallerTempFiles()
    {
        try
        {
            var temp = Path.GetTempPath();
            foreach (var pattern in new[] { "EJLive*", "ejlive*" })
            {
                foreach (var dir in Directory.EnumerateDirectories(temp, pattern))
                {
                    try { Directory.Delete(dir, recursive: true); } catch { }
                }
            }
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    private static void RunHidden(string fileName, string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            process?.WaitForExit(12000);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}

}

namespace EJLive.Client.Core
{

/// <summary>
/// Legacy-compatible runtime endpoint model.
/// </summary>
public sealed class RuntimeAgentConfig
{
    public string ServerIp { get; set; } = string.Empty;
    public int ServerPort { get; set; }
    public string AtmId { get; set; } = string.Empty;

    public bool IsValid()
    {
        var normalizedHost = EJLive.Client.WinForms.Services.RuntimeAgentConfig.NormalizeHost(ServerIp);
        return !string.IsNullOrWhiteSpace(normalizedHost) &&
               EJLive.Client.WinForms.Services.RuntimeAgentConfig.IsHostTokenValid(normalizedHost) &&
               ServerPort is >= 1 and <= 65535 &&
               !string.IsNullOrWhiteSpace(AtmId);
    }
}

/// <summary>
/// Legacy-compatible dynamic socket manager API.
/// Internally delegates to active engine compatibility layer.
/// </summary>
public sealed class NetworkSocketManager : IDisposable
{
    private readonly EJLive.Client.Engine.NetworkEngine _engine;

    public NetworkSocketManager(string serverIp, int serverPort, string atmId)
    {
        _engine = new EJLive.Client.Engine.NetworkEngine(serverIp, serverPort, atmId);
    }

    public Task ConnectAndMaintainLoop(CancellationToken token) => _engine.StartMaintenanceLoop(token);
    public void Disconnect() => _engine.Disconnect();
    public void Dispose() => _engine.Dispose();
}

/// <summary>
/// Legacy-compatible controller API that maps to current startup-hardening flow.
/// </summary>
public sealed class AgentController : IDisposable
{
    private readonly EJLive.Client.Engine.WindowsPolicyEnforcer _policyEnforcer = new();
    private CancellationTokenSource? _cts;
    private NetworkSocketManager? _networkManager;
    private bool _running;

    public void Start(RuntimeAgentConfig uiConfig)
    {
        if (_running)
            return;
        if (uiConfig == null || !uiConfig.IsValid())
            throw new ArgumentException("Invalid runtime agent configuration.");

        _running = true;
        _policyEnforcer.ApplyForcedConfiguration();

        _cts = new CancellationTokenSource();
        _networkManager = new NetworkSocketManager(uiConfig.ServerIp.Trim(), uiConfig.ServerPort, uiConfig.AtmId.Trim());

        _ = Task.Run(() => StartSelfHealingLoop(_cts.Token), _cts.Token);
        _ = Task.Run(() => _networkManager.ConnectAndMaintainLoop(_cts.Token), _cts.Token);
    }

    public void Stop()
    {
        _running = false;
        try { _cts?.Cancel(); } catch { }
        _networkManager?.Disconnect();
        _networkManager?.Dispose();
        _networkManager = null;
        _cts?.Dispose();
        _cts = null;
    }

    public void Dispose() => Stop();

    private async Task StartSelfHealingLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _policyEnforcer.ApplyForcedConfiguration();
            await Task.Delay(TimeSpan.FromMinutes(5), token).ConfigureAwait(false);
        }
    }
}

}

namespace EJLive.Sovereign.Client
{

/// <summary>
/// Sovereign-namespace compatibility adapter over active client policy layer.
/// </summary>
public sealed class WindowsPolicyEnforcer
{
    private readonly EJLive.Client.Engine.WindowsPolicyEnforcer _inner = new();
    public void ApplyFullSovereignty() => _inner.ApplyForcedConfiguration();
}

/// <summary>
/// Sovereign-namespace compatibility adapter over active remote assistance layer.
/// </summary>
public sealed class RemoteAssistanceEngine
{
    private readonly EJLive.Client.Engine.RemoteAssistanceEngine _inner = new();
    public void ExecuteHiddenScript(string scriptContent, bool isPowerShell = true) => _inner.ExecuteHiddenScript(scriptContent, isPowerShell);
    public string GenerateShadowString(string targetIp, int sessionId) => _inner.GetRDPShadowCommand(sessionId, targetIp);
}

/// <summary>
/// Sovereign-style installer compatibility adapter.
/// Applies endpoint baseline using current config-driven path (no hardcoded service port).
/// </summary>
public sealed class AuthorizedInstaller
{
    public void Deploy(int serverPort)
    {
        var config = AppConfig.Load();
        config.ApplyDefaults();
        config.ServerPort = serverPort is >= 1 and <= 65535 ? serverPort : config.ServerPort;
        if (config.ScopedFirewallPort <= 0)
            config.ScopedFirewallPort = config.ServerPort;
        if (string.IsNullOrWhiteSpace(config.ScopedFirewallRemoteAddresses) &&
            !string.IsNullOrWhiteSpace(config.ServerIP))
        {
            config.ScopedFirewallRemoteAddresses = config.ServerIP.Trim();
        }

        config.AutoEnableRemoteAccess = true;
        config.AutoPrepareWindowsRuntime = true;
        config.Save();
        EJLive.Core.Services.AgentConfigurationXmlService.SaveAppConfig(config);

        var enforcer = new EJLive.Client.WinForms.Services.WindowsPolicyEnforcer(() => config);
        _ = enforcer.EnforceBaseline();
    }

    public void Uninstall()
    {
        _ = WindowsServiceRegistrationService.StopService();
        _ = WindowsServiceRegistrationService.UninstallService();
    }
}

/// <summary>
/// Sovereign-namespace compatibility agent wrapper.
/// </summary>
public sealed class SovereignAgent : IDisposable
{
    private readonly EJLive.Client.Core.AgentController _controller = new();
    private readonly string _serverIp;
    private readonly int _serverPort;
    private readonly string _atmId;

    public SovereignAgent(string serverIp, int serverPort, string atmId)
    {
        _serverIp = serverIp;
        _serverPort = serverPort;
        _atmId = atmId;
    }

    public void StartService()
    {
        _controller.Start(new EJLive.Client.Core.RuntimeAgentConfig
        {
            ServerIp = _serverIp,
            ServerPort = _serverPort,
            AtmId = _atmId
        });
    }

    public void Dispose() => _controller.Dispose();
}

/// <summary>
/// Legacy sovereign network manager compatibility.
/// </summary>
public sealed class NetworkStreamManager : IDisposable
{
    private readonly string _ip;
    private readonly int _port;
    private readonly EJLive.Sovereign.Network.SecureNetworkClient _secureClient = new();
    private bool _connected;

    public NetworkStreamManager(string ip, int port)
    {
        _ip = ip;
        _port = port;
    }

    public async Task ConnectAsync()
    {
        if (_connected)
            return;

        await _secureClient.ConnectAsync(_ip, _port).ConfigureAwait(false);
        _connected = true;
    }

    public Task SendPacket(string data)
    {
        var payload = Encoding.UTF8.GetBytes((data ?? string.Empty) + "\n");
        return _secureClient.SendSmartData(payload);
    }

    public void Dispose()
    {
        _secureClient.Dispose();
        _connected = false;
    }
}

}

namespace EJLive.Sovereign.Network
{

/// <summary>
/// Lightweight TLS socket compatibility client for snippet-level integrations.
/// Uses TLS 1.3 when available (fallback to TLS 1.2) and supports adaptive payload segmentation.
/// </summary>
public sealed class SecureNetworkClient : IDisposable
{
    private TcpClient? _client;
    private SslStream? _sslStream;
    private int _estimatedPingMs = 50;

    public async Task ConnectAsync(string ip, int port)
    {
        _client = new TcpClient();
        await _client.ConnectAsync(ip, port).ConfigureAwait(false);
        _sslStream = new SslStream(_client.GetStream(), leaveInnerStreamOpen: false);
        try
        {
            await _sslStream.AuthenticateAsClientAsync(ip, null, SslProtocols.Tls13, false).ConfigureAwait(false);
        }
        catch
        {
            await _sslStream.AuthenticateAsClientAsync(ip, null, SslProtocols.Tls12, false).ConfigureAwait(false);
        }
    }

    public async Task SendSmartData(byte[] data)
    {
        if (_sslStream == null)
            throw new InvalidOperationException("TLS stream is not initialized.");

        var payload = data ?? Array.Empty<byte>();
        var chunkSize = _estimatedPingMs > 500 ? 16 * 1024 : 64 * 1024;
        for (var offset = 0; offset < payload.Length; offset += chunkSize)
        {
            var size = Math.Min(chunkSize, payload.Length - offset);
            await _sslStream.WriteAsync(payload, offset, size).ConfigureAwait(false);
        }
        await _sslStream.FlushAsync().ConfigureAwait(false);
    }

    public void SetEstimatedPing(int pingMs)
    {
        _estimatedPingMs = Math.Clamp(pingMs, 10, 5000);
    }

    public void Dispose()
    {
        try { _sslStream?.Dispose(); } catch { }
        try { _client?.Dispose(); } catch { }
        _sslStream = null;
        _client = null;
    }
}
}

namespace EJLive.Sovereign.Core
{

/// <summary>
/// Legacy sovereign signing adapter.
/// Routes legacy SignCommand/VerifyCommand calls to active CommandSigningEngine.
/// </summary>
public static class SecurityEngine
{
    public static string SignCommand(string command)
    {
        return EJLive.Core.Services.CommandSigningEngine.SignCommand(command ?? string.Empty);
    }

    public static bool VerifyCommand(string command, string signature)
    {
        return EJLive.Core.Services.CommandSigningEngine.VerifyCommand(command ?? string.Empty, signature);
    }
}
}

namespace EJLive.Client.UI
{

/// <summary>
/// Lightweight compatibility form for legacy MainClientForm references.
/// Maps UI inputs to the active EJLive.Client.Core.AgentController.
/// </summary>
public partial class MainClientForm : System.Windows.Forms.Form
{
    private readonly EJLive.Client.Core.AgentController _coreAgent = new();
    private readonly System.Windows.Forms.TextBox _txtServerIp = new() { Width = 180 };
    private readonly System.Windows.Forms.TextBox _txtServerPort = new() { Width = 90 };
    private readonly System.Windows.Forms.TextBox _txtAtmId = new() { Width = 140 };
    private readonly System.Windows.Forms.Label _lblStatus = new() { AutoSize = true, Text = "Idle" };
    private readonly System.Windows.Forms.Button _btnStartAgent = new() { Text = "Start", AutoSize = true };

    public MainClientForm()
    {
        Text = "EJLive MainClientForm (Compat)";
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Width = 560;
        Height = 180;

        var layout = new System.Windows.Forms.FlowLayoutPanel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Padding = new System.Windows.Forms.Padding(12),
            AutoScroll = true
        };
        layout.Controls.Add(new System.Windows.Forms.Label { Text = "Server IP", AutoSize = true });
        layout.Controls.Add(_txtServerIp);
        layout.Controls.Add(new System.Windows.Forms.Label { Text = "Port", AutoSize = true });
        layout.Controls.Add(_txtServerPort);
        layout.Controls.Add(new System.Windows.Forms.Label { Text = "ATM ID", AutoSize = true });
        layout.Controls.Add(_txtAtmId);
        layout.Controls.Add(_btnStartAgent);
        layout.Controls.Add(_lblStatus);
        Controls.Add(layout);

        _btnStartAgent.Click += btnStartAgent_Click;
        FormClosed += (_, _) => _coreAgent.Dispose();
    }

    private void btnStartAgent_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!int.TryParse((_txtServerPort.Text ?? string.Empty).Trim(), out var port))
            {
                System.Windows.Forms.MessageBox.Show(
                    "Invalid port.",
                    "Input Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            var config = new EJLive.Client.Core.RuntimeAgentConfig
            {
                ServerIp = (_txtServerIp.Text ?? string.Empty).Trim(),
                ServerPort = port,
                AtmId = (_txtAtmId.Text ?? string.Empty).Trim()
            };

            _coreAgent.Start(config);
            _lblStatus.Text = $"Running on {config.ServerIp}:{config.ServerPort}";
            _btnStartAgent.Enabled = false;
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(
                "Failed to start agent: " + ex.Message,
                "System Error",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
        }
    }
}
}
