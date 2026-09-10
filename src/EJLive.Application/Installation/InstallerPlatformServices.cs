using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using EJLive.Core;
using EJLive.Core.Models;
using Microsoft.Win32;

namespace EJLive.Application.Installation;

internal sealed record InstallerOperationResult(
    bool Success,
    int ExitCode,
    string Message,
    bool RequiresAdministrator = false);

internal sealed record InstallerPolicyBaselineResult(
    bool Success,
    bool RequiresAdministrator,
    string Message,
    InstallerRemoteReadiness Readiness);

internal sealed record InstallerRemoteReadiness(
    bool IsAdministrator,
    bool DomainJoinedLikely,
    bool RemoteDesktopEnabled,
    bool NlaEnabled,
    bool TermServiceRunning,
    bool Port3389Listening,
    bool RemoteRegistryRunning,
    bool WinRmRunning,
    bool Port5985Listening)
{
    public bool ReadyForSafeRemoteAccess =>
        IsAdministrator &&
        RemoteDesktopEnabled &&
        NlaEnabled &&
        TermServiceRunning &&
        Port3389Listening;

    public string ToSummary() => string.Join(
        "; ",
        $"Ready={ReadyForSafeRemoteAccess}",
        $"Admin={IsAdministrator}",
        $"DomainJoined={DomainJoinedLikely}",
        $"RDPEnabled={RemoteDesktopEnabled}",
        $"NLA={NlaEnabled}",
        $"TermService={TermServiceRunning}",
        $"Port3389={Port3389Listening}",
        $"RemoteRegistry={RemoteRegistryRunning}",
        $"WinRM={WinRmRunning}",
        $"WinRM5985={Port5985Listening}");
}

internal static class InstallerConfigurationWriter
{
    private static readonly string DefaultPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive",
        "Client",
        "AgentConf.xml");

    public static InstallerOperationResult SaveAppConfig(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var tempPath = DefaultPath + ".tmp";
        try
        {
            config.ApplyDefaults();
            var directory = Path.GetDirectoryName(DefaultPath)
                ?? throw new InvalidOperationException("Agent configuration directory is unavailable.");
            Directory.CreateDirectory(directory);

            var document = new XDocument(
                new XElement(
                    "AgentConfiguration",
                    new XElement("ATM_ID", config.ATM_ID ?? string.Empty),
                    new XElement("ATM_Name", config.ATM_Name ?? string.Empty),
                    new XElement("ATM_Type", config.ATM_Type ?? string.Empty),
                    new XElement("ServerIP", config.ServerIP ?? "127.0.0.1"),
                    new XElement("ServerPort", config.ServerPort > 0 ? config.ServerPort : AppConstants.DefaultPort),
                    new XElement("NetworkType", config.NetworkType ?? "LAN"),
                    new XElement("SourcePath", config.SourcePath ?? string.Empty),
                    new XElement("BackupPath", config.BackupPath ?? string.Empty),
                    new XElement("ImageInboxPath", config.ImageInboxPath ?? string.Empty),
                    new XElement("AutoConnect", config.AutoConnect),
                    new XElement("AutoBackup", config.AutoBackup),
                    new XElement("EnforceCommandAuthorization", config.EnforceCommandAuthorization),
                    new XElement("DefaultCommandRole", config.DefaultCommandRole ?? string.Empty),
                    new XElement("EnableSupabaseSync", config.EnableSupabaseSync),
                    new XElement("SupabaseUrl", config.SupabaseUrl ?? string.Empty),
                    new XElement(
                        "SupabaseServiceKey",
                        EJLive.Shared.SecurityHelper.ProtectDpapiStringIfNeeded(config.SupabaseServiceKey ?? string.Empty)),
                    new XElement("AutoEnableRemoteAccess", config.AutoEnableRemoteAccess),
                    new XElement("AutoPrepareWindowsRuntime", config.AutoPrepareWindowsRuntime),
                    new XElement("EnableWinRmBootstrap", config.EnableWinRmBootstrap),
                    new XElement("EnableRemoteRegistryBootstrap", config.EnableRemoteRegistryBootstrap),
                    new XElement("EnforceScopedFirewallRule", config.EnforceScopedFirewallRule),
                    new XElement("ScopedFirewallPort", config.ScopedFirewallPort),
                    new XElement("ScopedFirewallRemoteAddresses", config.ScopedFirewallRemoteAddresses ?? string.Empty),
                    new XElement("ConfigureDefenderExclusions", config.ConfigureDefenderExclusions),
                    new XElement("DefenderExclusionPaths", config.DefenderExclusionPaths ?? string.Empty),
                    new XElement("HelpdeskAdGroup", config.HelpdeskAdGroup ?? string.Empty),
                    new XElement("WindowsBaselineRepairIntervalMin", config.WindowsBaselineRepairIntervalMin),
                    new XElement("WindowsPolicyProfileMode", config.WindowsPolicyProfileMode ?? "Audit"),
                    new XElement("AllowLocalWindowsPasswordChange", config.AllowLocalWindowsPasswordChange),
                    new XElement("RequireEncryptedWindowsPasswordPayload", config.RequireEncryptedWindowsPasswordPayload),
                    new XElement("AllowedPasswordAccounts", config.AllowedPasswordAccounts ?? string.Empty),
                    new XElement("EnforceLowPriorityMode", config.EnforceLowPriorityMode),
                    new XElement("PinToLastProcessorCore", config.PinToLastProcessorCore),
                    new XElement("HeartbeatIntervalSec", config.HeartbeatIntervalSec),
                    new XElement("ReconnectIntervalSec", config.ReconnectIntervalSec)));

            document.Save(tempPath);
            File.Move(tempPath, DefaultPath, overwrite: true);
            return new InstallerOperationResult(true, 0, $"Agent XML configuration saved: {DefaultPath}");
        }
        catch (Exception ex)
        {
            TryDeleteTempFile(tempPath);
            return new InstallerOperationResult(false, -1, "Agent XML configuration failed: " + ex.Message);
        }
    }

    private static void TryDeleteTempFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // The next installer run will replace the temporary file.
        }
    }
}

internal static class InstallerServiceManager
{
    public const string DefaultServiceName = "EJLiveClientAgent";
    private const string DefaultDisplayName = "EJLive Client Agent Service";
    private const string DefaultDescription = "Runs EJLive ATM synchronization, heartbeat, journal delivery, and governed command processing.";
    private static readonly Regex ServiceNamePattern = new("^[A-Za-z0-9_.-]{1,80}$", RegexOptions.CultureInvariant);

    public static InstallerOperationResult InstallOrUpdateService(
        string serviceExePath,
        string serviceName = DefaultServiceName)
    {
        if (!TryValidateServiceName(serviceName, out var validation))
            return validation;
        if (string.IsNullOrWhiteSpace(serviceExePath) || !File.Exists(serviceExePath))
            return new InstallerOperationResult(false, -10, "Service executable path is missing.");
        if (!InstallerSecurityContext.IsAdministrator())
            return AdministratorRequired("install or update the service");

        var fullPath = Path.GetFullPath(serviceExePath);
        var query = RunSc("query", serviceName);
        if (!query.Success && !IsServiceMissing(query))
            return query;

        if (IsServiceMissing(query))
        {
            var create = RunSc(
                "create",
                serviceName,
                "binPath=",
                $"\"{fullPath}\"",
                "type=",
                "own",
                "start=",
                "auto",
                "obj=",
                "LocalSystem",
                "DisplayName=",
                DefaultDisplayName);
            if (!create.Success && !ContainsCode(create, 1073))
                return create with { Message = "Service creation failed: " + create.Message };
        }

        var configure = RunSc(
            "config",
            serviceName,
            "binPath=",
            $"\"{fullPath}\"",
            "start=",
            "auto",
            "obj=",
            "LocalSystem",
            "DisplayName=",
            DefaultDisplayName);
        if (!configure.Success)
            return configure with { Message = "Service configuration failed: " + configure.Message };

        var description = RunSc("description", serviceName, DefaultDescription);
        if (!description.Success)
            return description with { Message = "Service description failed: " + description.Message };

        var recovery = RunSc(
            "failure",
            serviceName,
            "reset=",
            "86400",
            "actions=",
            "restart/60000/restart/60000/restart/60000");
        if (!recovery.Success)
            return recovery with { Message = "Service recovery configuration failed: " + recovery.Message };

        _ = RunSc("failureflag", serviceName, "1");
        return new InstallerOperationResult(
            true,
            0,
            $"Service '{serviceName}' is installed and configured for automatic LocalSystem startup.");
    }

    public static InstallerOperationResult StartService(string serviceName = DefaultServiceName)
    {
        if (!TryValidateServiceName(serviceName, out var validation))
            return validation;
        if (!InstallerSecurityContext.IsAdministrator())
            return AdministratorRequired("start the service");

        var result = RunSc("start", serviceName);
        return result.Success || ContainsCode(result, 1056)
            ? new InstallerOperationResult(true, 0, $"Service '{serviceName}' is running.")
            : result;
    }

    public static InstallerOperationResult StopService(string serviceName = DefaultServiceName)
    {
        if (!TryValidateServiceName(serviceName, out var validation))
            return validation;
        if (!InstallerSecurityContext.IsAdministrator())
            return AdministratorRequired("stop the service");

        var result = RunSc("stop", serviceName);
        return result.Success || ContainsCode(result, 1060) || ContainsCode(result, 1062)
            ? new InstallerOperationResult(true, 0, $"Service '{serviceName}' is stopped or was already absent.")
            : result;
    }

    public static InstallerOperationResult UninstallService(string serviceName = DefaultServiceName)
    {
        if (!TryValidateServiceName(serviceName, out var validation))
            return validation;
        if (!InstallerSecurityContext.IsAdministrator())
            return AdministratorRequired("uninstall the service");

        _ = StopService(serviceName);
        var result = RunSc("delete", serviceName);
        return result.Success || ContainsCode(result, 1060)
            ? new InstallerOperationResult(true, 0, $"Service '{serviceName}' was removed or was already absent.")
            : result;
    }

    public static InstallerOperationResult QueryService(string serviceName = DefaultServiceName)
    {
        if (!TryValidateServiceName(serviceName, out var validation))
            return validation;
        return RunSc("query", serviceName);
    }

    internal static InstallerOperationResult RunSc(params string[] arguments) =>
        InstallerPlatformCommand.Run("sc.exe", arguments, TimeSpan.FromSeconds(20));

    private static bool TryValidateServiceName(string serviceName, out InstallerOperationResult result)
    {
        if (ServiceNamePattern.IsMatch(serviceName ?? string.Empty))
        {
            result = new InstallerOperationResult(true, 0, string.Empty);
            return true;
        }

        result = new InstallerOperationResult(false, -12, "Service name contains unsupported characters.");
        return false;
    }

    private static InstallerOperationResult AdministratorRequired(string action) =>
        new(false, -11, $"Administrator privileges are required to {action}.", true);

    private static bool IsServiceMissing(InstallerOperationResult result) =>
        !result.Success && ContainsCode(result, 1060);

    private static bool ContainsCode(InstallerOperationResult result, int code) =>
        result.ExitCode == code || result.Message.Contains(code.ToString(), StringComparison.Ordinal);
}

internal static class InstallerStartupManager
{
    public const string ClientTaskName = "EJLive Client AutoStart";
    public const string CompanionRunValueName = "EJLiveClientCompanion";
    private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public static InstallerOperationResult UnregisterClientAutostart()
    {
        if (!InstallerSecurityContext.IsAdministrator())
            return new InstallerOperationResult(false, -11, "Administrator privileges are required to remove the startup task.", true);

        var result = InstallerPlatformCommand.Run(
            "schtasks.exe",
            new[] { "/Delete", "/TN", ClientTaskName, "/F" },
            TimeSpan.FromSeconds(20));
        return result.Success || IsMissingTask(result)
            ? new InstallerOperationResult(true, 0, $"Startup task '{ClientTaskName}' was removed or was already absent.")
            : result;
    }

    public static InstallerOperationResult RegisterUserSessionCompanion(string clientExePath)
    {
        if (string.IsNullOrWhiteSpace(clientExePath) || !File.Exists(clientExePath))
            return new InstallerOperationResult(false, -10, "Client companion executable was not found.");
        if (!InstallerSecurityContext.IsAdministrator())
            return new InstallerOperationResult(false, -11, "Administrator privileges are required to register the session companion.", true);

        try
        {
            var fullPath = Path.GetFullPath(clientExePath);
            using var baseKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default);
            using var runKey = baseKey.CreateSubKey(RunKeyPath, writable: true);
            if (runKey is null)
                return new InstallerOperationResult(false, -20, "HKLM Run key is unavailable.");

            runKey.SetValue(
                CompanionRunValueName,
                $"\"{fullPath}\" --background --companion",
                RegistryValueKind.String);
            return new InstallerOperationResult(true, 0, "User-session companion startup was registered in HKLM Run.");
        }
        catch (Exception ex)
        {
            return new InstallerOperationResult(false, -21, "Session companion registration failed: " + ex.Message);
        }
    }

    public static InstallerOperationResult UnregisterUserSessionCompanion()
    {
        if (!InstallerSecurityContext.IsAdministrator())
            return new InstallerOperationResult(false, -11, "Administrator privileges are required to remove the session companion.", true);

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default);
            using var runKey = baseKey.OpenSubKey(RunKeyPath, writable: true);
            runKey?.DeleteValue(CompanionRunValueName, throwOnMissingValue: false);
            return new InstallerOperationResult(true, 0, "User-session companion startup was removed or was already absent.");
        }
        catch (Exception ex)
        {
            return new InstallerOperationResult(false, -22, "Session companion removal failed: " + ex.Message);
        }
    }

    private static bool IsMissingTask(InstallerOperationResult result) =>
        result.Message.Contains("cannot find", StringComparison.OrdinalIgnoreCase) ||
        result.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase);
}

internal static class InstallerWindowsPolicyService
{
    private const string TerminalServerKey = @"SYSTEM\CurrentControlSet\Control\Terminal Server";
    private const string TerminalServerPolicyKey = @"SOFTWARE\Policies\Microsoft\Windows NT\Terminal Services";
    private const string RdpTcpKey = @"SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp";
    private const string SystemPolicyKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

    public static InstallerPolicyBaselineResult ApplyBaseline(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.ApplyDefaults();

        var before = CaptureReadiness();
        var mode = ResolveMode(config.WindowsPolicyProfileMode);
        if (mode == InstallerPolicyMode.Audit)
        {
            return new InstallerPolicyBaselineResult(
                before.ReadyForSafeRemoteAccess,
                !before.IsAdministrator,
                "Windows policy profile is audit-only; no machine settings were changed. " + before.ToSummary(),
                before);
        }

        if (mode == InstallerPolicyMode.DomainPolicyRespect && before.DomainJoinedLikely)
        {
            return new InstallerPolicyBaselineResult(
                true,
                false,
                "Domain-policy-respect profile detected a domain context; local policy writes were skipped. " + before.ToSummary(),
                before);
        }

        if (!before.IsAdministrator)
        {
            return new InstallerPolicyBaselineResult(
                false,
                true,
                "Administrator privileges are required to apply the Windows policy baseline.",
                before);
        }

        var actions = new List<InstallerOperationResult>
        {
            ApplyRegistryBaseline(),
            EnsureServiceRunning("TermService", "auto"),
            EnsureServiceRunning("UmRdpService", "demand"),
            EnableRemoteDesktopFirewall()
        };

        if (config.EnableRemoteRegistryBootstrap)
            actions.Add(EnsureServiceRunning("RemoteRegistry", "auto"));
        if (config.EnableWinRmBootstrap)
            actions.Add(EnableWinRm());

        foreach (var account in ParseAccountList(config.AllowedPasswordAccounts))
            actions.Add(AddRemoteDesktopAccount(account));

        var after = CaptureReadiness();
        var commandFailures = actions.Where(item => !item.Success).ToArray();
        var success = commandFailures.Length == 0 && after.ReadyForSafeRemoteAccess;
        var messages = actions.Select(item => item.Message).Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        messages.Add("Readiness: " + after.ToSummary());

        return new InstallerPolicyBaselineResult(
            success,
            commandFailures.Any(item => item.RequiresAdministrator),
            string.Join("; ", messages),
            after);
    }

    public static InstallerRemoteReadiness CaptureReadiness() => new(
        IsAdministrator: InstallerSecurityContext.IsAdministrator(),
        DomainJoinedLikely: InstallerSecurityContext.IsDomainJoinedLikely(),
        RemoteDesktopEnabled: ReadDword(TerminalServerKey, "fDenyTSConnections") == 0,
        NlaEnabled: ReadDword(RdpTcpKey, "UserAuthentication") == 1,
        TermServiceRunning: IsServiceRunning("TermService"),
        Port3389Listening: IsPortListening(3389),
        RemoteRegistryRunning: IsServiceRunning("RemoteRegistry"),
        WinRmRunning: IsServiceRunning("WinRM"),
        Port5985Listening: IsPortListening(5985));

    private static InstallerOperationResult ApplyRegistryBaseline()
    {
        try
        {
            SetDword(TerminalServerKey, "fDenyTSConnections", 0);
            SetDword(TerminalServerKey, "AllowRemoteRPC", 1);
            SetDword(RdpTcpKey, "UserAuthentication", 1);
            SetDword(TerminalServerPolicyKey, "fDenyTSConnections", 0);
            SetDword(TerminalServerPolicyKey, "UserAuthentication", 1);
            SetDword(TerminalServerPolicyKey, "fSingleSessionPerUser", 1);
            SetDword(SystemPolicyKey, "LocalAccountTokenFilterPolicy", 1);
            return new InstallerOperationResult(true, 0, "RDP, NLA, single-session, and local-admin-token settings were applied.");
        }
        catch (Exception ex)
        {
            return new InstallerOperationResult(false, -30, "Windows policy registry update failed: " + ex.Message);
        }
    }

    private static InstallerOperationResult EnsureServiceRunning(string serviceName, string startupMode)
    {
        var configure = InstallerServiceManager.RunSc("config", serviceName, "start=", startupMode);
        if (!configure.Success)
            return configure with { Message = $"{serviceName} startup configuration failed: {configure.Message}" };

        var start = InstallerServiceManager.RunSc("start", serviceName);
        if (start.Success || start.Message.Contains("1056", StringComparison.Ordinal))
            return new InstallerOperationResult(true, 0, $"{serviceName} is configured and running.");
        return start with { Message = $"{serviceName} start failed: {start.Message}" };
    }

    private static InstallerOperationResult EnableRemoteDesktopFirewall()
    {
        var result = InstallerPlatformCommand.Run(
            "netsh.exe",
            new[] { "advfirewall", "firewall", "set", "rule", "group=remote desktop", "new", "enable=Yes" },
            TimeSpan.FromSeconds(30));
        return result.Success
            ? result with { Message = "Windows Remote Desktop firewall group was enabled." }
            : result with { Message = "Remote Desktop firewall update failed: " + result.Message };
    }

    private static InstallerOperationResult EnableWinRm()
    {
        var result = InstallerPlatformCommand.Run(
            "winrm.exe",
            new[] { "quickconfig", "-quiet" },
            TimeSpan.FromSeconds(45));
        return result.Success
            ? result with { Message = "WinRM quick configuration completed." }
            : result with { Message = "WinRM quick configuration failed: " + result.Message };
    }

    private static InstallerOperationResult AddRemoteDesktopAccount(string account)
    {
        if (!Regex.IsMatch(account, "^[\\p{L}\\p{N}_.@\\- \\\\]{1,128}$", RegexOptions.CultureInvariant))
            return new InstallerOperationResult(false, -31, $"Remote Desktop account token was rejected: {account}");

        var result = InstallerPlatformCommand.Run(
            "net.exe",
            new[] { "localgroup", "Remote Desktop Users", account, "/add" },
            TimeSpan.FromSeconds(20));
        if (result.Success || result.Message.Contains("1378", StringComparison.Ordinal))
            return new InstallerOperationResult(true, 0, $"Remote Desktop group membership is present for '{account}'.");
        return result with { Message = $"Remote Desktop group update failed for '{account}': {result.Message}" };
    }

    private static IEnumerable<string> ParseAccountList(string? accountsCsv) =>
        (accountsCsv ?? string.Empty)
            .Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private static InstallerPolicyMode ResolveMode(string? configuredMode)
    {
        var normalized = (configuredMode ?? string.Empty)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Trim();

        if (normalized.Equals("audit", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("auditonly", StringComparison.OrdinalIgnoreCase))
            return InstallerPolicyMode.Audit;
        if (normalized.Equals("domaingporespect", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("domainpolicyrespect", StringComparison.OrdinalIgnoreCase))
            return InstallerPolicyMode.DomainPolicyRespect;
        return InstallerPolicyMode.Enforce;
    }

    private static int? ReadDword(string keyPath, string valueName)
    {
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine,
                Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default);
            using var key = baseKey.OpenSubKey(keyPath, writable: false);
            var value = key?.GetValue(valueName);
            return value is null ? null : Convert.ToInt32(value);
        }
        catch
        {
            return null;
        }
    }

    private static void SetDword(string keyPath, string valueName, int value)
    {
        using var baseKey = RegistryKey.OpenBaseKey(
            RegistryHive.LocalMachine,
            Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Default);
        using var key = baseKey.CreateSubKey(keyPath, writable: true)
            ?? throw new InvalidOperationException($"Registry key is unavailable: {keyPath}");
        key.SetValue(valueName, value, RegistryValueKind.DWord);
    }

    private static bool IsServiceRunning(string serviceName)
    {
        var query = InstallerServiceManager.RunSc("query", serviceName);
        return query.Success &&
               (Regex.IsMatch(query.Message, @"STATE\s*:\s*4\b", RegexOptions.CultureInvariant) ||
                query.Message.Contains("RUNNING", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsPortListening(int port)
    {
        try
        {
            return IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpListeners()
                .Any(endpoint => endpoint.Port == port);
        }
        catch
        {
            return false;
        }
    }

    private enum InstallerPolicyMode
    {
        Audit,
        Enforce,
        DomainPolicyRespect
    }
}

internal static class InstallerSecurityContext
{
    public static bool IsAdministrator()
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

    public static bool IsDomainJoinedLikely()
    {
        try
        {
            return !string.IsNullOrWhiteSpace(Environment.UserDomainName) &&
                   !Environment.UserDomainName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}

internal static class InstallerPlatformCommand
{
    public static InstallerOperationResult Run(
        string fileName,
        IEnumerable<string> arguments,
        TimeSpan timeout)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            foreach (var argument in arguments)
                process.StartInfo.ArgumentList.Add(argument);

            if (!process.Start())
                return new InstallerOperationResult(false, -1, $"{fileName} did not start.");

            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit((int)Math.Clamp(timeout.TotalMilliseconds, 1, int.MaxValue)))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit(5000);
                }
                catch
                {
                    // Best effort termination after a bounded installer command timeout.
                }

                return new InstallerOperationResult(false, -2, $"{fileName} timed out.");
            }

            Task.WaitAll(stdout, stderr);
            var output = string.Join(
                Environment.NewLine,
                new[] { stdout.Result, stderr.Result }.Where(value => !string.IsNullOrWhiteSpace(value))).Trim();
            return new InstallerOperationResult(
                process.ExitCode == 0,
                process.ExitCode,
                string.IsNullOrWhiteSpace(output) ? $"{fileName} exit={process.ExitCode}" : output);
        }
        catch (Exception ex)
        {
            return new InstallerOperationResult(false, -3, $"{fileName} failed: {ex.Message}");
        }
    }
}
