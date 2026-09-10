using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EJLive.Client.Service;

/// <summary>
/// Windows Service host for the EJLive client agent.
/// Uses BackgroundService and IAgentController so the production client remains
/// headless, service-safe, and free from direct WinForms dependencies.
/// </summary>
public sealed class ClientAgentWindowsService : BackgroundService
{
    private readonly ILogger<ClientAgentWindowsService> _logger;
    private readonly RuntimeAgentConfigResolver _runtimeConfigResolver;
    private readonly RuntimeAgentConfigOverrides _startupOverrides;
    private readonly object _lifecycleLock = new();

    private IAgentController? _agent;
    private AgentHealthReporter? _health;
    private DateTime _lastStatusLogUtc;
    private DateTime _lastRestartAttemptUtc;

    public ClientAgentWindowsService(
        ILogger<ClientAgentWindowsService> logger,
        IConfiguration configuration,
        RuntimeAgentConfigResolver runtimeConfigResolver)
    {
        _logger = logger;
        _runtimeConfigResolver = runtimeConfigResolver;
        _startupOverrides = ReadStartupOverrides(configuration);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EJLive Client Agent Service starting.");
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(StopAgentSafely);
        return RunSupervisionLoopAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EJLive Client Agent Service stopping.");
        StopAgentSafely();
        return base.StopAsync(cancellationToken);
    }

    private async Task RunSupervisionLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                EnsureAgentRunning();
                RestartAgentIfFailed();
                LogAgentStatusPeriodically();
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service supervision loop encountered an error. Retrying.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private void EnsureAgentRunning()
    {
        lock (_lifecycleLock)
        {
            if (_agent != null)
                return;

            _agent = CreateAgentController();
            _agent.OnLog += message => _logger.LogInformation("{Message}", message);
            _agent.OnStatusUpdate += status =>
                _logger.LogInformation(
                    "Agent status: state={State}, connected={Connected}, handshake={Handshake}, pending={Pending}",
                    status.State,
                    status.Connected,
                    status.HandshakeComplete,
                    status.PendingOutboxItems);

            _agent.StartAll();

            var healthFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Agent", "health.json");

            _health = new AgentHealthReporter(_agent, TimeSpan.FromSeconds(30), healthFile);
            _health.Publish();

            _lastStatusLogUtc = DateTime.UtcNow;
            _logger.LogInformation("Agent supervision initialized. Controller={ControllerType}", _agent.GetType().Name);
        }
    }

    private void RestartAgentIfFailed()
    {
        var agent = _agent;
        if (agent == null)
            return;

        var status = SafeGetStatus(agent);
        if (status.State != AgentControllerState.Failed)
            return;

        var now = DateTime.UtcNow;
        if (now - _lastRestartAttemptUtc < TimeSpan.FromMinutes(1))
            return;

        _lastRestartAttemptUtc = now;
        _logger.LogWarning("Agent is in Failed state. Restarting headless controller.");

        StopAgentSafely();
        EnsureAgentRunning();
    }

    private AgentStatus SafeGetStatus(IAgentController agent)
    {
        try
        {
            return agent.GetStatus();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to read agent status.");
            return new AgentStatus(
                AgentControllerState.Failed,
                false,
                false,
                0,
                0,
                0,
                null,
                null,
                null,
                ex.Message);
        }
    }

    private void StopAgentSafely()
    {
        lock (_lifecycleLock)
        {
            try
            {
                _health?.Dispose();
                _agent?.StopAll();
                _agent?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while stopping EJLive agent.");
            }
            finally
            {
                _health = null;
                _agent = null;
            }
        }
    }

    private IAgentController CreateAgentController()
    {
        var persistedConfig = AgentConfigurationXmlService.LoadAppConfig(
            EJLive.Core.Models.AppConfig.Load());

        return new AgentHeadlessController(
            config: persistedConfig,
            overrides: _startupOverrides,
            resolver: _runtimeConfigResolver,
            preferEnvironment: true);
    }

    private static RuntimeAgentConfigOverrides ReadStartupOverrides(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var serverIp = FirstNonEmpty(
            configuration["EJLive:ServerIp"],
            configuration["server-ip"]);
        var atmId = FirstNonEmpty(
            configuration["EJLive:AtmId"],
            configuration["atm-id"]);
        var rawPort = FirstNonEmpty(
            configuration["EJLive:ServerPort"],
            configuration["server-port"]);

        int? serverPort = null;
        if (!string.IsNullOrWhiteSpace(rawPort))
            serverPort = int.TryParse(rawPort, out var parsedPort) ? parsedPort : 0;

        return new RuntimeAgentConfigOverrides(serverIp, serverPort, atmId);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    private void LogAgentStatusPeriodically()
    {
        var agent = _agent;
        if (agent == null)
            return;

        var now = DateTime.UtcNow;
        if (now - _lastStatusLogUtc < TimeSpan.FromMinutes(2))
            return;

        _lastStatusLogUtc = now;

        var status = SafeGetStatus(agent);
        _logger.LogInformation(
            "Health check: state={State}, connected={Connected}, handshake={Handshake}, pending={Pending}, session={SessionId}",
            status.State,
            status.Connected,
            status.HandshakeComplete,
            status.PendingOutboxItems,
            status.SessionId);

    }
}
