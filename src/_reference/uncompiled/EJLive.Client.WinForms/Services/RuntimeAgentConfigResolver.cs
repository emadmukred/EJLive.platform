using EJLive.Core.Models;
using AppConfig = EJLive.Shared.AppConfig;

namespace EJLive.Client.WinForms.Services;

public sealed class RuntimeAgentConfig
{
    public string ServerIp { get; init; } = string.Empty;
    public int ServerPort { get; init; }
    public string AtmId { get; init; } = string.Empty;

    public string ConnectionDisplay => $"{ServerIp}:{ServerPort}";

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
            reason = $"Server endpoint '{ServerIp}' is not a valid host or IP.";
            return false;
        }

        if (ServerPort < 1 || ServerPort > 65535)
        {
            reason = $"Server port '{ServerPort}' is outside range 1..65535.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(AtmId))
        {
            reason = "ATM identifier is required.";
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
}

public sealed class RuntimeAgentConfigResolver
{
    public const string EnvServerIpKey = "EJLIVE_SERVER_IP";
    public const string EnvServerPortKey = "EJLIVE_SERVER_PORT";
    public const string EnvAtmIdKey = "EJLIVE_ATM_ID";

    private readonly Func<string, string?> _environmentReader;

    public RuntimeAgentConfigResolver(Func<string, string?>? environmentReader = null)
    {
        _environmentReader = environmentReader ?? Environment.GetEnvironmentVariable;
    }

    public bool TryResolve(
        AppConfig source,
        out RuntimeAgentConfig runtimeConfig,
        out string reason,
        string? serverIpFromUi = null,
        int? serverPortFromUi = null,
        string? atmIdFromUi = null,
        bool preferEnvironment = true)
    {
        runtimeConfig = new RuntimeAgentConfig();
        reason = string.Empty;

        if (source == null)
        {
            reason = "Runtime configuration source is missing.";
            return false;
        }

        var envServerIp = preferEnvironment ? _environmentReader(EnvServerIpKey) : null;
        var envAtmId = preferEnvironment ? _environmentReader(EnvAtmIdKey) : null;
        var envServerPort = ResolveEnvironmentPort(preferEnvironment);

        var resolvedServerIp = FirstNonEmpty(serverIpFromUi, envServerIp, source.ServerIP);
        var resolvedAtmId = FirstNonEmpty(atmIdFromUi, envAtmId, source.ATM_ID);
        var resolvedPort = ResolvePort(serverPortFromUi, envServerPort, source.ServerPort);

        runtimeConfig = new RuntimeAgentConfig
        {
            ServerIp = RuntimeAgentConfig.NormalizeHost(resolvedServerIp),
            ServerPort = resolvedPort,
            AtmId = (resolvedAtmId ?? string.Empty).Trim()
        };

        if (!runtimeConfig.IsValid(out reason))
            return false;

        return true;
    }

    public void ApplyTo(AppConfig target, RuntimeAgentConfig runtimeConfig)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));
        if (runtimeConfig == null)
            throw new ArgumentNullException(nameof(runtimeConfig));
        if (!runtimeConfig.IsValid(out var reason))
            throw new InvalidOperationException("Cannot apply invalid runtime configuration: " + reason);

        target.ServerIP = runtimeConfig.ServerIp;
        target.ServerPort = runtimeConfig.ServerPort;
        target.ATM_ID = runtimeConfig.AtmId;
    }

    private int? ResolveEnvironmentPort(bool preferEnvironment)
    {
        if (!preferEnvironment)
            return null;

        var raw = _environmentReader(EnvServerPortKey);
        if (int.TryParse(raw, out var parsed) && parsed >= 1 && parsed <= 65535)
            return parsed;
        return null;
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

    private static int ResolvePort(int? serverPortFromUi, int? serverPortFromEnvironment, int serverPortFromConfig)
    {
        if (serverPortFromUi is >= 1 and <= 65535)
            return serverPortFromUi.Value;
        if (serverPortFromEnvironment is >= 1 and <= 65535)
            return serverPortFromEnvironment.Value;
        return serverPortFromConfig;
    }
}
