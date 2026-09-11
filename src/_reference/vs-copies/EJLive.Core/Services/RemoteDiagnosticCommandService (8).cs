// RemoteDiagnosticCommandService (8).cs
// Processed by Code Intelligence Scanner — Smart Merge & Restructure

using System.Diagnostics;

namespace EJLive.Core.Services
{
    public sealed record RemoteDiagnosticCommandResult(bool Success, bool Blocked, string Output)
    {
        public static RemoteDiagnosticCommandResult Successful(string output) => new(true, false, output ?? string.Empty);
        public static RemoteDiagnosticCommandResult Failed(string output) => new(false, false, output ?? string.Empty);
        public static RemoteDiagnosticCommandResult Rejected(string reason) => new(false, true, reason ?? "blocked");
    }
/// <summary>
/// Policy-safe replacement for legacy terminal helpers.
/// Executes only approved diagnostic commands with output capture.
/// </summary>
public sealed class RemoteDiagnosticCommandService
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };

public RemoteDiagnosticCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return RemoteDiagnosticCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return RemoteDiagnosticCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteApproved(command.FileName, command.Arguments, timeoutMs);
}

private static RemoteDiagnosticCommandResult ExecuteApproved(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return RemoteDiagnosticCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return RemoteDiagnosticCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? RemoteDiagnosticCommandResult.Successful(output)
: RemoteDiagnosticCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return RemoteDiagnosticCommandResult.Failed(ex.Message);
}
}
}
public partial class RemoteDiagnosticCommandService
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.v17_bak
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.before_unify
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.v17_bak
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.before_unify
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    // --- Properties ---
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };
public RemoteDiagnosticCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return RemoteDiagnosticCommandResult.Rejected("Preset command is required.");
    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return RemoteDiagnosticCommandResult.Rejected("Preset is not allowed by policy allowlist.");
    return ExecuteApproved(command.FileName, command.Arguments, timeoutMs);
}
private static RemoteDiagnosticCommandResult ExecuteApproved(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
    using var process = Process.Start(psi);
    if (process == null)
    return RemoteDiagnosticCommandResult.Failed("Process failed to start.");
    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return RemoteDiagnosticCommandResult.Failed("Process timed out.");
    }
var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];
return process.ExitCode == 0
? RemoteDiagnosticCommandResult.Successful(output)
: RemoteDiagnosticCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return RemoteDiagnosticCommandResult.Failed(ex.Message);
}
}
}
public partial public public sealed class RemoteDiagnosticCommandService
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };
public RemoteDiagnosticCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    private static RemoteDiagnosticCommandResult ExecuteApproved(string fileName, string arguments, int timeoutMs)
    {
    }

}
public partial public sealed class RemoteDiagnosticCommandService
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };
public RemoteDiagnosticCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    private static RemoteDiagnosticCommandResult ExecuteApproved(string fileName, string arguments, int timeoutMs)
    {
    }

}
public partial class RemoteDiagnosticCommandService
{
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =


    new(StringComparer.OrdinalIgnoreCase)


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.v17_bak
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.before_unify
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =

    new(StringComparer.OrdinalIgnoreCase)

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.v17_bak
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.before_unify
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    // --- Properties ---
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs
public RemoteDiagnosticCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return RemoteDiagnosticCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return RemoteDiagnosticCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteApproved(command.FileName, command.Arguments, timeoutMs);
}


// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs
public RemoteDiagnosticCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return RemoteDiagnosticCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return RemoteDiagnosticCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteApproved(command.FileName, command.Arguments, timeoutMs);
}


private static RemoteDiagnosticCommandResult ExecuteApproved(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return RemoteDiagnosticCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return RemoteDiagnosticCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? RemoteDiagnosticCommandResult.Successful(output)
: RemoteDiagnosticCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return RemoteDiagnosticCommandResult.Failed(ex.Message);
}
}

}
// Class: RemoteDiagnosticCommandService (from 3 sources)
public sealed partial class RemoteDiagnosticCommandService
{
    // --- Constants & Fields ---
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =

    new(StringComparer.OrdinalIgnoreCase)

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.v17_bak
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")

    // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs.before_unify
    ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")


    // --- Properties ---
    private static readonly Dictionary<string, (string FileName, string Arguments)> PresetCommands =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["PING_LOCAL"] = ("cmd.exe", "/c ping -n 1 127.0.0.1"),
        ["QUSER"] = ("quser.exe", string.Empty),
        ["WHOAMI"] = ("whoami.exe", "/all"),
        ["HOSTNAME"] = ("hostname.exe", string.Empty),
        ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
        ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
        ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
    };

// Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Services\RemoteDiagnosticCommandService.cs
public RemoteDiagnosticCommandResult ExecuteSilentShell(string preset, int timeoutMs = 12000)
{
    if (string.IsNullOrWhiteSpace(preset))
    return RemoteDiagnosticCommandResult.Rejected("Preset command is required.");

    if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
    return RemoteDiagnosticCommandResult.Rejected("Preset is not allowed by policy allowlist.");

    return ExecuteApproved(command.FileName, command.Arguments, timeoutMs);
}


// --- Methods ---
private static RemoteDiagnosticCommandResult ExecuteApproved(string fileName, string arguments, int timeoutMs)
{
    try
    {
        var psi = new ProcessStartInfo;
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

    using var process = Process.Start(psi);
    if (process == null)
    return RemoteDiagnosticCommandResult.Failed("Process failed to start.");

    if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
    {
        try { process.Kill(); } catch { }
        return RemoteDiagnosticCommandResult.Failed("Process timed out.");
    }

var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
if (output.Length > 2000)
output = output[..2000];

return process.ExitCode == 0
? RemoteDiagnosticCommandResult.Successful(output)
: RemoteDiagnosticCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
}
catch (Exception ex)
{
    return RemoteDiagnosticCommandResult.Failed(ex.Message);
}
}

}
}

using var process = Process.Start(psi);