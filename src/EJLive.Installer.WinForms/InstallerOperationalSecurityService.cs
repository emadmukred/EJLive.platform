using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using EJLive.Client.WinForms.Services;
using EJLive.Core;
using Microsoft.Win32;

namespace EJLive.Installer.WinForms;

public sealed class InstallerOperationalSecurityOptions
{
    public string ProgramRoot { get; set; } = string.Empty;
    public string DatabasePath { get; set; } = AppConstants.DefaultDatabasePath;
    public int ServicePort { get; set; } = AppConstants.DefaultPort;
    public string RemoteAddresses { get; set; } = "Any";
    public bool ConfigureDefenderExclusion { get; set; } = true;
    public bool SystemOnlyDatabaseAcl { get; set; }
    public bool CleanupInstallerTempArtifacts { get; set; } = true;
}

public sealed class InstallerOperationalSecurityResult
{
    public bool Success { get; set; } = true;
    public bool RequiresAdministrator { get; set; }
    public List<string> Notes { get; } = new();
}

/// <summary>
/// Applies and removes installer-time Windows security integrations in an auditable way.
/// </summary>
public static class InstallerOperationalSecurityService
{
    private const string FirewallInboundRuleName = "EJLive Client Socket Inbound";
    private const string FirewallOutboundRuleName = "EJLive Client Socket Outbound";
    private const string LegacyFirewallInboundRuleName = "EJLive Client Socket Inbound 5656";
    private const string LegacyFirewallOutboundRuleName = "EJLive Client Socket Outbound 5656";
    private const string LocalMachineRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string CurrentUserRunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string EjliveRegistryKey = @"SOFTWARE\EJLive";

    public static InstallerOperationalSecurityResult Apply(InstallerOperationalSecurityOptions options)
    {
        var result = new InstallerOperationalSecurityResult();
        if (!IsAdministrator())
        {
            result.Success = false;
            result.RequiresAdministrator = true;
            result.Notes.Add("Administrator privileges are required for installer security provisioning.");
            return result;
        }

        var normalizedProgramRoot = NormalizePath(options.ProgramRoot);
        var normalizedDatabasePath = NormalizePath(options.DatabasePath);
        var port = ResolvePort(options.ServicePort);
        var remoteAddresses = NormalizeRemoteAddresses(options.RemoteAddresses);

        if (options.ConfigureDefenderExclusion && !string.IsNullOrWhiteSpace(normalizedProgramRoot))
            RunDefenderExclusionAdd(normalizedProgramRoot, result);
        else
            result.Notes.Add("Defender exclusion skipped by configuration.");

        RunFirewallRuleApply(port, remoteAddresses, result);
        ApplySqliteAcl(normalizedDatabasePath, options.SystemOnlyDatabaseAcl, result);
        WriteInstallerRegistryMarker(normalizedProgramRoot, normalizedDatabasePath, port, remoteAddresses, result);
        if (options.CleanupInstallerTempArtifacts)
            CleanupInstallerTempArtifacts(result);

        return result;
    }

    public static InstallerOperationalSecurityResult Uninstall(
        string programRoot,
        string databasePath,
        bool removeClientRuntimeFolders)
    {
        var result = new InstallerOperationalSecurityResult();
        if (!IsAdministrator())
        {
            result.Success = false;
            result.RequiresAdministrator = true;
            result.Notes.Add("Administrator privileges are required for uninstall security cleanup.");
            return result;
        }

        var normalizedProgramRoot = NormalizePath(programRoot);
        var normalizedDatabasePath = NormalizePath(databasePath);

        if (!string.IsNullOrWhiteSpace(normalizedProgramRoot))
            RunDefenderExclusionRemove(normalizedProgramRoot, result);

        RunFirewallRuleRemove(result);
        RemoveRegistryArtifacts(result);

        if (removeClientRuntimeFolders)
            RemoveRuntimeFolders(normalizedProgramRoot, normalizedDatabasePath, result);
        else
            result.Notes.Add("Runtime folder cleanup skipped by configuration.");

        CleanupInstallerTempArtifacts(result);

        return result;
    }

    private static void RunDefenderExclusionAdd(string programRoot, InstallerOperationalSecurityResult result)
    {
        var escapedPath = programRoot.Replace("'", "''", StringComparison.Ordinal);
        var command = "$ErrorActionPreference='Stop'; " +
                      "if (Get-Command Add-MpPreference -ErrorAction SilentlyContinue) { " +
                      $"Add-MpPreference -ExclusionPath '{escapedPath}'; Write-Output 'DefenderExclusionApplied'; " +
                      "} else { Write-Output 'Add-MpPreferenceUnavailable'; }";
        var run = RunProcess(
            "powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command \"" + command.Replace("\"", "`\"", StringComparison.Ordinal) + "\"",
            25000);
        RecordRun(result, run, "Defender exclusion apply");
    }

    private static void RunDefenderExclusionRemove(string programRoot, InstallerOperationalSecurityResult result)
    {
        var escapedPath = programRoot.Replace("'", "''", StringComparison.Ordinal);
        var command = "$ErrorActionPreference='Continue'; " +
                      "if (Get-Command Remove-MpPreference -ErrorAction SilentlyContinue) { " +
                      $"Remove-MpPreference -ExclusionPath '{escapedPath}' -ErrorAction SilentlyContinue; Write-Output 'DefenderExclusionRemoved'; " +
                      "} else { Write-Output 'Remove-MpPreferenceUnavailable'; }";
        var run = RunProcess(
            "powershell.exe",
            "-NoProfile -ExecutionPolicy Bypass -Command \"" + command.Replace("\"", "`\"", StringComparison.Ordinal) + "\"",
            25000);
        RecordRun(result, run, "Defender exclusion remove");
    }

    private static void RunFirewallRuleApply(int port, string remoteAddresses, InstallerOperationalSecurityResult result)
    {
        DeleteFirewallRule(FirewallInboundRuleName, result, "Firewall inbound pre-clean");
        DeleteFirewallRule(FirewallOutboundRuleName, result, "Firewall outbound pre-clean");
        DeleteFirewallRule(LegacyFirewallInboundRuleName, result, "Firewall inbound legacy pre-clean");
        DeleteFirewallRule(LegacyFirewallOutboundRuleName, result, "Firewall outbound legacy pre-clean");

        var addIn = RunProcess(
            "netsh.exe",
            $"advfirewall firewall add rule name=\"{FirewallInboundRuleName}\" dir=in action=allow protocol=TCP localport={port} remoteip={remoteAddresses} profile=domain,private,public",
            15000);
        var addOut = RunProcess(
            "netsh.exe",
            $"advfirewall firewall add rule name=\"{FirewallOutboundRuleName}\" dir=out action=allow protocol=TCP remoteport={port} remoteip={remoteAddresses} profile=domain,private,public",
            15000);
        RecordRun(result, addIn, $"Firewall inbound allow TCP/{port}");
        RecordRun(result, addOut, $"Firewall outbound allow TCP/{port}");
    }

    private static void RunFirewallRuleRemove(InstallerOperationalSecurityResult result)
    {
        DeleteFirewallRule(FirewallInboundRuleName, result, "Firewall inbound remove");
        DeleteFirewallRule(FirewallOutboundRuleName, result, "Firewall outbound remove");
        DeleteFirewallRule(LegacyFirewallInboundRuleName, result, "Firewall inbound legacy remove");
        DeleteFirewallRule(LegacyFirewallOutboundRuleName, result, "Firewall outbound legacy remove");
    }

    private static void ApplySqliteAcl(string databasePath, bool systemOnly, InstallerOperationalSecurityResult result)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            result.Success = false;
            result.Notes.Add("SQLite ACL skipped: database path is empty.");
            return;
        }

        try
        {
            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(databasePath))
            {
                using var _ = File.Create(databasePath);
            }

            var systemSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            var adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            var inheritFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

            var directorySecurity = new DirectorySecurity();
            directorySecurity.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            directorySecurity.AddAccessRule(new FileSystemAccessRule(systemSid, FileSystemRights.FullControl, inheritFlags, PropagationFlags.None, AccessControlType.Allow));
            if (!systemOnly)
                directorySecurity.AddAccessRule(new FileSystemAccessRule(adminSid, FileSystemRights.FullControl, inheritFlags, PropagationFlags.None, AccessControlType.Allow));

            if (!string.IsNullOrWhiteSpace(directory))
                new DirectoryInfo(directory).SetAccessControl(directorySecurity);

            var fileSecurity = new FileSecurity();
            fileSecurity.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            fileSecurity.AddAccessRule(new FileSystemAccessRule(systemSid, FileSystemRights.FullControl, AccessControlType.Allow));
            if (!systemOnly)
                fileSecurity.AddAccessRule(new FileSystemAccessRule(adminSid, FileSystemRights.FullControl, AccessControlType.Allow));
            new FileInfo(databasePath).SetAccessControl(fileSecurity);

            result.Notes.Add(systemOnly
                ? "SQLite ACL applied: LocalSystem only."
                : "SQLite ACL applied: LocalSystem + Administrators.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Notes.Add("SQLite ACL apply failed: " + ex.Message);
        }
    }

    private static void WriteInstallerRegistryMarker(
        string programRoot,
        string databasePath,
        int port,
        string remoteAddresses,
        InstallerOperationalSecurityResult result)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var key = baseKey.CreateSubKey(EjliveRegistryKey, writable: true);
            if (key == null)
            {
                result.Success = false;
                result.Notes.Add("Registry marker write failed: key handle not available.");
                return;
            }

            key.SetValue("InstallRoot", programRoot ?? string.Empty, RegistryValueKind.String);
            key.SetValue("DatabasePath", databasePath ?? string.Empty, RegistryValueKind.String);
            key.SetValue("ServicePort", port, RegistryValueKind.DWord);
            key.SetValue("RemoteAddresses", remoteAddresses ?? string.Empty, RegistryValueKind.String);
            key.SetValue("UpdatedAtUtc", DateTime.UtcNow.ToString("O"), RegistryValueKind.String);
            result.Notes.Add("Registry marker updated under HKLM\\SOFTWARE\\EJLive.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Notes.Add("Registry marker write failed: " + ex.Message);
        }
    }

    private static void RemoveRegistryArtifacts(InstallerOperationalSecurityResult result)
    {
        RemoveRegistryTree(RegistryHive.LocalMachine, RegistryView.Registry64, EjliveRegistryKey, result, "HKLM\\SOFTWARE\\EJLive");
        RemoveRegistryTree(RegistryHive.LocalMachine, RegistryView.Registry32, EjliveRegistryKey, result, "HKLM\\SOFTWARE\\WOW6432Node\\EJLive");
        RemoveRunValue(RegistryHive.LocalMachine, RegistryView.Registry64, LocalMachineRunKey, WindowsStartupService.CompanionRunValueName, result, "HKLM Run companion value");
        RemoveRunValue(RegistryHive.CurrentUser, RegistryView.Default, CurrentUserRunKey, "EJLive.Client", result, "HKCU Run client value");
    }

    private static void RemoveRuntimeFolders(string programRoot, string databasePath, InstallerOperationalSecurityResult result)
    {
        TryDeleteFile(databasePath, result, "SQLite database");
        var clientDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "EJLive",
            "Client");
        TryDeleteDirectory(clientDataRoot, result, "Client data root");
        TryDeleteDirectory(programRoot, result, "Client service payload");

        var databaseDirectory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(databaseDirectory))
            TryDeleteDirectoryIfEmpty(databaseDirectory, result, "Database directory");
    }

    private static void TryDeleteFile(string path, InstallerOperationalSecurityResult result, string label)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            if (!File.Exists(path))
            {
                result.Notes.Add(label + ": already absent.");
                return;
            }

            File.Delete(path);
            result.Notes.Add(label + ": deleted.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Notes.Add(label + " delete failed: " + ex.Message);
        }
    }

    private static void TryDeleteDirectory(string path, InstallerOperationalSecurityResult result, string label)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            if (!Directory.Exists(path))
            {
                result.Notes.Add(label + ": already absent.");
                return;
            }

            Directory.Delete(path, recursive: true);
            result.Notes.Add(label + ": deleted.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Notes.Add(label + " delete failed: " + ex.Message);
        }
    }

    private static void TryDeleteDirectoryIfEmpty(string path, InstallerOperationalSecurityResult result, string label)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return;

        try
        {
            if (Directory.EnumerateFileSystemEntries(path).Any())
            {
                result.Notes.Add(label + ": not empty, preserved.");
                return;
            }

            Directory.Delete(path, recursive: false);
            result.Notes.Add(label + ": deleted (empty).");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Notes.Add(label + " cleanup failed: " + ex.Message);
        }
    }

    private static void RemoveRegistryTree(
        RegistryHive hive,
        RegistryView view,
        string subKeyPath,
        InstallerOperationalSecurityResult result,
        string label)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            baseKey.DeleteSubKeyTree(subKeyPath, throwOnMissingSubKey: false);
            result.Notes.Add(label + ": removed.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Notes.Add(label + " remove failed: " + ex.Message);
        }
    }

    private static void RemoveRunValue(
        RegistryHive hive,
        RegistryView view,
        string keyPath,
        string valueName,
        InstallerOperationalSecurityResult result,
        string label)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(keyPath, writable: true);
            if (key == null)
            {
                result.Notes.Add(label + ": key not found.");
                return;
            }

            key.DeleteValue(valueName, throwOnMissingValue: false);
            result.Notes.Add(label + ": removed.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Notes.Add(label + " remove failed: " + ex.Message);
        }
    }

    private static int ResolvePort(int port)
    {
        return port > 0 && port <= 65535 ? port : AppConstants.DefaultPort;
    }

    private static string NormalizeRemoteAddresses(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? "Any" : trimmed;
    }

    private static string NormalizePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        try
        {
            return Path.GetFullPath(value.Trim());
        }
        catch
        {
            return value.Trim();
        }
    }

    private static bool IsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static void RecordRun(
        InstallerOperationalSecurityResult result,
        (bool Success, int ExitCode, string Output) run,
        string label)
    {
        result.Notes.Add($"{label}: exit={run.ExitCode}; output={Trim(run.Output, 240)}");
        if (!run.Success)
            result.Success = false;
    }

    private static void DeleteFirewallRule(string ruleName, InstallerOperationalSecurityResult result, string label)
    {
        var run = RunProcess("netsh.exe", $"advfirewall firewall delete rule name=\"{ruleName}\"", 12000);
        RecordRun(result, run, label);
    }

    private static void CleanupInstallerTempArtifacts(InstallerOperationalSecurityResult result)
    {
        var tempRoot = Path.GetFullPath(Path.GetTempPath());
        var candidateFolders = new[]
        {
            Path.Combine(tempRoot, "EJLiveInstaller"),
            Path.Combine(tempRoot, "EJLive", "Installer"),
            Path.Combine(tempRoot, "EJLive", "Publish")
        };

        foreach (var folder in candidateFolders)
        {
            try
            {
                var full = Path.GetFullPath(folder);
                if (!full.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!Directory.Exists(full))
                {
                    result.Notes.Add("Temp cleanup: " + full + " already absent.");
                    continue;
                }

                Directory.Delete(full, recursive: true);
                result.Notes.Add("Temp cleanup: removed " + full);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Notes.Add("Temp cleanup failed: " + folder + " => " + ex.Message);
            }
        }
    }

    private static string Trim(string value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        return value.Length <= max ? value : value.Substring(0, max);
    }

    private static (bool Success, int ExitCode, string Output) RunProcess(string fileName, string arguments, int timeoutMs)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process == null)
                return (false, -1, fileName + " did not start.");

            if (!process.WaitForExit(timeoutMs))
            {
                try { process.Kill(); } catch { }
                return (false, -2, fileName + " timed out.");
            }

            var output = (process.StandardOutput.ReadToEnd() + " " + process.StandardError.ReadToEnd()).Trim();
            return (process.ExitCode == 0, process.ExitCode, output);
        }
        catch (Exception ex)
        {
            return (false, -3, ex.Message);
        }
    }
}
