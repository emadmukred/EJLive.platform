using System.Diagnostics;

namespace EJLive.Core.Services;

/// <summary>
/// Executes a fixed allowlist of read-only local diagnostic commands.
/// Arbitrary executable names and arguments are never accepted from callers.
/// </summary>
public sealed class ControlledDiagnosticCommandService
{
    private static readonly IReadOnlyDictionary<string, (string FileName, string Arguments)> PresetCommands =
        new Dictionary<string, (string FileName, string Arguments)>(StringComparer.OrdinalIgnoreCase)
        {
            ["PING_LOCAL"] = ("ping.exe", "-n 1 127.0.0.1"),
            ["QUSER"] = ("quser.exe", string.Empty),
            ["WHOAMI"] = ("whoami.exe", "/all"),
            ["HOSTNAME"] = ("hostname.exe", string.Empty),
            ["W32TM_STATUS"] = ("w32tm.exe", "/query /status"),
            ["TERMSERVICE_STATE"] = ("sc.exe", "query TermService"),
            ["FIREWALL_STATE"] = ("netsh.exe", "advfirewall show allprofiles state")
        };

    public DiagnosticCommandResult ExecutePreset(string preset, int timeoutMs = 12000)
    {
        if (string.IsNullOrWhiteSpace(preset))
            return DiagnosticCommandResult.Rejected("Preset command is required.");

        if (!PresetCommands.TryGetValue(preset.Trim(), out var command))
            return DiagnosticCommandResult.Rejected("Preset is not allowed by policy.");

        return ExecuteProcess(command.FileName, command.Arguments, timeoutMs);
    }

    private static DiagnosticCommandResult ExecuteProcess(string fileName, string arguments, int timeoutMs)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process is null)
                return DiagnosticCommandResult.Failed("Process failed to start.");

            if (!process.WaitForExit(Math.Clamp(timeoutMs, 1000, 60000)))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                }

                return DiagnosticCommandResult.Failed("Process timed out.");
            }

            var output = (process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd()).Trim();
            if (output.Length > 2000)
                output = output[..2000];

            return process.ExitCode == 0
                ? DiagnosticCommandResult.Successful(output)
                : DiagnosticCommandResult.Failed($"ExitCode={process.ExitCode}; Output={output}");
        }
        catch (Exception ex)
        {
            return DiagnosticCommandResult.Failed(ex.Message);
        }
    }
}

public sealed record DiagnosticCommandResult(bool Success, bool Blocked, string Output)
{
    public static DiagnosticCommandResult Successful(string output) => new(true, false, output ?? string.Empty);
    public static DiagnosticCommandResult Failed(string output) => new(false, false, output ?? string.Empty);
    public static DiagnosticCommandResult Rejected(string reason) => new(false, true, reason ?? "blocked");
}

