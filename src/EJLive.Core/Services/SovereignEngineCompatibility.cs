using EJLive.Shared;

namespace EJLive.Sovereign.Engine;

/// <summary>
/// Compatibility bridge for legacy remote diagnostic calls.
/// Routes execution to the active allowlisted diagnostic service to avoid arbitrary shell execution.
/// </summary>
public sealed class RemoteDiagnosticBridge
{
    private const string LoggerSource = "RemoteDiagnosticCompat";
    private readonly EJLive.Core.Services.RemoteDiagnosticCommandService _service = new();

    /// <summary>
    /// Legacy compatibility signature. Executes only mapped allowlisted presets.
    /// </summary>
    public void ExecuteSilentShell(string script)
    {
        _ = ExecuteSilentShellSafe(script);
    }

    /// <summary>
    /// Preferred compatibility API with explicit result.
    /// </summary>
    public EJLive.Core.Services.RemoteDiagnosticCommandResult ExecuteSilentShellSafe(string script, int timeoutMs = 12000)
    {
        var preset = ResolvePreset(script);
        var result = _service.ExecuteSilentShell(preset, timeoutMs);

        if (!result.Success)
            AppLogger.Instance.Warning($"Remote diagnostic command rejected/failed. preset={preset}; detail={result.Output}", LoggerSource);

        return result;
    }

    private static string ResolvePreset(string? script)
    {
        if (string.IsNullOrWhiteSpace(script))
            return string.Empty;

        var value = script.Trim();
        if (IsKnownPreset(value))
            return value;

        // Legacy command-text mapping to approved presets only.
        if (value.IndexOf("quser", StringComparison.OrdinalIgnoreCase) >= 0) return "QUSER";
        if (value.IndexOf("whoami", StringComparison.OrdinalIgnoreCase) >= 0) return "WHOAMI";
        if (value.IndexOf("hostname", StringComparison.OrdinalIgnoreCase) >= 0) return "HOSTNAME";
        if (value.IndexOf("w32tm", StringComparison.OrdinalIgnoreCase) >= 0) return "W32TM_STATUS";
        if (value.IndexOf("termservice", StringComparison.OrdinalIgnoreCase) >= 0) return "TERMSERVICE_STATE";
        if (value.IndexOf("advfirewall", StringComparison.OrdinalIgnoreCase) >= 0 ||
            value.IndexOf("firewall", StringComparison.OrdinalIgnoreCase) >= 0) return "FIREWALL_STATE";
        if (value.IndexOf("ping", StringComparison.OrdinalIgnoreCase) >= 0) return "PING_LOCAL";

        // Keep original token so allowlist layer can reject with deterministic reason.
        return value;
    }

    private static bool IsKnownPreset(string preset)
    {
        return preset.Equals("PING_LOCAL", StringComparison.OrdinalIgnoreCase) ||
               preset.Equals("QUSER", StringComparison.OrdinalIgnoreCase) ||
               preset.Equals("WHOAMI", StringComparison.OrdinalIgnoreCase) ||
               preset.Equals("HOSTNAME", StringComparison.OrdinalIgnoreCase) ||
               preset.Equals("W32TM_STATUS", StringComparison.OrdinalIgnoreCase) ||
               preset.Equals("TERMSERVICE_STATE", StringComparison.OrdinalIgnoreCase) ||
               preset.Equals("FIREWALL_STATE", StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Compatibility bridge for legacy JournalEngine reads.
/// Uses active non-blocking reader service with shared-access semantics.
/// </summary>
public sealed class JournalEngine
{
    /// <summary>
    /// Legacy compatibility signature.
    /// </summary>
    public void ReadJournalNonBlocking(string path)
    {
        _ = EJLive.Core.Services.JournalReadService.TryReadAllNonBlocking(path, out _);
    }

    /// <summary>
    /// Preferred API: read full content without blocking active ATM writers.
    /// </summary>
    public bool TryReadJournalNonBlocking(string path, out string content)
    {
        return EJLive.Core.Services.JournalReadService.TryReadAllNonBlocking(path, out content);
    }

    /// <summary>
    /// Preferred API: read incremental delta using shared access and stability retries.
    /// </summary>
    public bool TryReadJournalDeltaNonBlocking(
        string path,
        long previousOffset,
        out byte[] delta,
        out long nextOffset)
    {
        return EJLive.Core.Services.JournalReadService.TryReadDeltaNonBlocking(
            path,
            previousOffset,
            out delta,
            out nextOffset);
    }
}
