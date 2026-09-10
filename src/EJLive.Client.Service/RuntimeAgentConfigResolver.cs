using EJLive.Core.Models;

namespace EJLive.Client.Service;

/// <summary>
/// Immutable connection identity used by the headless agent after all
/// configuration sources have been resolved.
/// </summary>
public sealed class RuntimeAgentConfig
{
    public string ServerIp { get; init; } = string.Empty;
    public int ServerPort { get; init; }
    public string AtmId { get; init; } = string.Empty;

    public string ConnectionDisplay => ServerIp.Contains(':', StringComparison.Ordinal)
        ? $"[{ServerIp}]:{ServerPort}"
        : $"{ServerIp}:{ServerPort}";

    public bool IsValid(out string reason)
    {
        var normalizedServer = NormalizeHost(ServerIp);
        if (string.IsNullOrWhiteSpace(normalizedServer))
        {
            reason = "Server endpoint is required.";
            return false;
        }

        if (!IsHostTokenValid(normalizedServer))
        {
            reason = $"Server endpoint '{ServerIp}' is not a valid host or IP address.";
            return false;
        }

        if (ServerPort is < 1 or > 65535)
        {
            reason = $"Server port '{ServerPort}' is outside range 1..65535.";
            return false;
        }

        if (!IsAtmIdentifierValid(AtmId))
        {
            reason = "ATM identifier is required and may contain only letters, digits, '-', '_' or '.'.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    internal static string NormalizeHost(string? host)
    {
        var candidate = (host ?? string.Empty).Trim();
        if (candidate.StartsWith("[", StringComparison.Ordinal) &&
            candidate.EndsWith("]", StringComparison.Ordinal) &&
            candidate.Length > 2)
        {
            candidate = candidate[1..^1];
        }

        return candidate;
    }

    internal static bool IsHostTokenValid(string host)
    {
        var type = Uri.CheckHostName(host);
        return type is UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6;
    }

    internal static bool IsAtmIdentifierValid(string? atmId)
    {
        var value = (atmId ?? string.Empty).Trim();
        if (value.Length is 0 or > 128)
            return false;

        return value.All(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' or '.');
    }
}

/// <summary>
/// Explicit values supplied by the service host at startup. A missing value
/// does not replace environment or persisted configuration.
/// </summary>
public sealed record RuntimeAgentConfigOverrides(
    string? ServerIp = null,
    int? ServerPort = null,
    string? AtmId = null)
{
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(ServerIp) &&
        ServerPort is null &&
        string.IsNullOrWhiteSpace(AtmId);
}

/// <summary>
/// Resolves the headless agent endpoint using deterministic precedence:
/// startup overrides, environment variables, then persisted AppConfig.
/// </summary>
public sealed class RuntimeAgentConfigResolver
{
    public const string EnvServerIpKey = "EJLIVE_SERVER_IP";
    public const string EnvServerPortKey = "EJLIVE_SERVER_PORT";
    public const string EnvAtmIdKey = "EJLIVE_ATM_ID";

    private readonly Func<string, string?> _environmentReader;

    public RuntimeAgentConfigResolver()
        : this(Environment.GetEnvironmentVariable)
    {
    }

    public RuntimeAgentConfigResolver(Func<string, string?> environmentReader)
    {
        _environmentReader = environmentReader ?? throw new ArgumentNullException(nameof(environmentReader));
    }

    public bool TryResolve(
        AppConfig? source,
        out RuntimeAgentConfig runtimeConfig,
        out string reason,
        string? serverIpOverride = null,
        int? serverPortOverride = null,
        string? atmIdOverride = null,
        bool preferEnvironment = true)
    {
        return TryResolve(
            source,
            new RuntimeAgentConfigOverrides(serverIpOverride, serverPortOverride, atmIdOverride),
            out runtimeConfig,
            out reason,
            preferEnvironment);
    }

    public bool TryResolve(
        AppConfig? source,
        RuntimeAgentConfigOverrides overrides,
        out RuntimeAgentConfig runtimeConfig,
        out string reason,
        bool preferEnvironment = true)
    {
        ArgumentNullException.ThrowIfNull(overrides);

        runtimeConfig = new RuntimeAgentConfig();
        if (source is null)
        {
            reason = "Runtime configuration source is missing.";
            return false;
        }

        if (overrides.ServerPort is < 1 or > 65535)
        {
            reason = $"Startup server-port override '{overrides.ServerPort}' is outside range 1..65535.";
            return false;
        }

        var environmentServerIp = preferEnvironment ? _environmentReader(EnvServerIpKey) : null;
        var environmentAtmId = preferEnvironment ? _environmentReader(EnvAtmIdKey) : null;
        if (!TryResolveEnvironmentPort(preferEnvironment, out var environmentServerPort, out reason))
            return false;

        var resolvedServerIp = FirstNonEmpty(overrides.ServerIp, environmentServerIp, source.ServerIP);
        var resolvedAtmId = FirstNonEmpty(overrides.AtmId, environmentAtmId, source.ATM_ID);
        var resolvedPort = overrides.ServerPort ?? environmentServerPort ?? source.ServerPort;

        runtimeConfig = new RuntimeAgentConfig
        {
            ServerIp = RuntimeAgentConfig.NormalizeHost(resolvedServerIp),
            ServerPort = resolvedPort,
            AtmId = (resolvedAtmId ?? string.Empty).Trim()
        };

        return runtimeConfig.IsValid(out reason);
    }

    public void ApplyTo(AppConfig target, RuntimeAgentConfig runtimeConfig)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(runtimeConfig);

        if (!runtimeConfig.IsValid(out var reason))
            throw new InvalidOperationException("Cannot apply invalid runtime configuration: " + reason);

        target.ServerIP = runtimeConfig.ServerIp;
        target.ServerPort = runtimeConfig.ServerPort;
        target.ATM_ID = runtimeConfig.AtmId;
    }

    private bool TryResolveEnvironmentPort(
        bool preferEnvironment,
        out int? port,
        out string reason)
    {
        port = null;
        reason = string.Empty;

        if (!preferEnvironment)
            return true;

        var raw = _environmentReader(EnvServerPortKey);
        if (string.IsNullOrWhiteSpace(raw))
            return true;

        if (int.TryParse(raw.Trim(), out var parsed) && parsed is >= 1 and <= 65535)
        {
            port = parsed;
            return true;
        }

        reason = $"Environment variable {EnvServerPortKey} must contain a port in range 1..65535.";
        return false;
    }

    private static string FirstNonEmpty(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate))
                return candidate.Trim();
        }

        return string.Empty;
    }
}
