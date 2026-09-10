using System.Diagnostics;
using EJLive.Client.WinForms.Services;
using EJLive.Core;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Installer.WinForms;

public sealed class InstallerExecutionResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string Mode { get; set; } = string.Empty;
    public List<string> Lines { get; } = new();
}

/// <summary>
/// Silent automation entrypoint for install/uninstall/validate workflows.
/// Adds a headless mode on top of the existing WinForms installer without replacing UI paths.
/// </summary>
public static class InstallerAutomationRunner
{
    public static InstallerExecutionResult RunInstall()
    {
        var result = new InstallerExecutionResult { Mode = "install", Success = true, ExitCode = 0 };
        try
        {
            Append(result, "Install started.");
            ValidateEnvironment(result);

            DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath);
            var config = AppConfig.Load();
            config.ApplyDefaults();
            EnforceClientAdminDefaults(config, result);
            config.Save();
            AgentConfigurationXmlService.SaveAppConfig(config);

            var baseline = ApplyWindowsRemoteBaseline(config);
            Append(result, baseline);

            var serviceInstall = InstallClientServiceWindowsRuntime(result);
            Append(result, serviceInstall);
            if (!serviceInstall.Contains("completed successfully", StringComparison.OrdinalIgnoreCase))
            {
                result.Success = false;
                result.ExitCode = 20;
            }

            var serviceRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "ClientService");
            var servicePort = config.ScopedFirewallPort > 0
                ? config.ScopedFirewallPort
                : (config.ServerPort > 0 ? config.ServerPort : AppConstants.DefaultPort);
            var remoteAddresses = !string.IsNullOrWhiteSpace(config.ScopedFirewallRemoteAddresses)
                ? config.ScopedFirewallRemoteAddresses
                : (string.IsNullOrWhiteSpace(config.ServerIP) ? "Any" : config.ServerIP);

            var hardening = InstallerOperationalSecurityService.Apply(new InstallerOperationalSecurityOptions
            {
                ProgramRoot = serviceRoot,
                DatabasePath = AppConstants.DefaultDatabasePath,
                ServicePort = servicePort,
                RemoteAddresses = remoteAddresses,
                ConfigureDefenderExclusion = config.ConfigureDefenderExclusions,
                SystemOnlyDatabaseAcl = true,
                CleanupInstallerTempArtifacts = true
            });
            Append(result, hardening.Success ? "Security hardening: OK" : "Security hardening: WARNING");
            foreach (var note in hardening.Notes)
                Append(result, "  - " + note);
            if (!hardening.Success && result.Success)
            {
                result.Success = false;
                result.ExitCode = 30;
            }

            var readiness = WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();
            Append(result, "Windows remote readiness: " + readiness.ToSummary());
            Append(result, "Install actions completed.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ExitCode = 99;
            Append(result, "Install failed: " + ex.Message);
        }

        PersistAutomationLog(result);
        return result;
    }

    public static InstallerExecutionResult RunUninstall()
    {
        var result = new InstallerExecutionResult { Mode = "uninstall", Success = true, ExitCode = 0 };
        try
        {
            Append(result, "Uninstall started.");

            var stop = WindowsServiceRegistrationService.StopService();
            Append(result, "Service stop: " + stop.Message);

            var removeService = WindowsServiceRegistrationService.UninstallService();
            Append(result, "Service uninstall: " + removeService.Message);
            if (!removeService.Success && removeService.ExitCode != 1060)
            {
                result.Success = false;
                result.ExitCode = 40;
            }

            var unregisterTask = WindowsStartupService.UnregisterClientAutostart();
            Append(result, "Startup task cleanup: " + unregisterTask.Message);

            var unregisterCompanion = WindowsStartupService.UnregisterUserSessionCompanion();
            Append(result, "Companion startup cleanup: " + unregisterCompanion.Message);

            var serviceRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "ClientService");
            var uninstall = InstallerOperationalSecurityService.Uninstall(
                programRoot: serviceRoot,
                databasePath: AppConstants.DefaultDatabasePath,
                removeClientRuntimeFolders: true);

            Append(result, uninstall.Success ? "Security cleanup: OK" : "Security cleanup: WARNING");
            foreach (var note in uninstall.Notes)
                Append(result, "  - " + note);
            if (!uninstall.Success && result.Success)
            {
                result.Success = false;
                result.ExitCode = 50;
            }

            Append(result, "Uninstall actions completed.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ExitCode = 99;
            Append(result, "Uninstall failed: " + ex.Message);
        }

        PersistAutomationLog(result);
        return result;
    }

    public static InstallerExecutionResult RunValidate()
    {
        var result = new InstallerExecutionResult { Mode = "validate", Success = true, ExitCode = 0 };
        try
        {
            Append(result, "Validation started.");
            ValidateEnvironment(result);
            var readiness = WindowsRemoteAccessService.EvaluateRemoteDesktopReadiness();
            Append(result, "Windows remote readiness: " + readiness.ToSummary());
            Append(result, "Validation completed.");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ExitCode = 99;
            Append(result, "Validation failed: " + ex.Message);
        }

        PersistAutomationLog(result);
        return result;
    }

    private static void ValidateEnvironment(InstallerExecutionResult result)
    {
        foreach (var path in new[]
                 {
                     AppConstants.DefaultLogPath,
                     AppConstants.DefaultArchivePath,
                     AppConstants.DefaultReportsPath,
                     AppConstants.DefaultClientOutboxPath,
                     AppConstants.DefaultClientInboxPath
                 })
        {
            Directory.CreateDirectory(path);
            Append(result, "OK " + path);
        }
    }

    private static void EnforceClientAdminDefaults(AppConfig config, InstallerExecutionResult result)
    {
        var changed = false;
        if (!config.AutoEnableRemoteAccess) { config.AutoEnableRemoteAccess = true; changed = true; }
        if (!config.AllowLocalWindowsPasswordChange) { config.AllowLocalWindowsPasswordChange = true; changed = true; }
        if (!config.RequireEncryptedWindowsPasswordPayload) { config.RequireEncryptedWindowsPasswordPayload = true; changed = true; }
        if (!config.AutoPrepareWindowsRuntime) { config.AutoPrepareWindowsRuntime = true; changed = true; }
        if (!config.EnableWinRmBootstrap) { config.EnableWinRmBootstrap = true; changed = true; }
        if (!config.EnableRemoteRegistryBootstrap) { config.EnableRemoteRegistryBootstrap = true; changed = true; }
        if (!config.EnforceScopedFirewallRule) { config.EnforceScopedFirewallRule = true; changed = true; }
        if (!config.ConfigureDefenderExclusions) { config.ConfigureDefenderExclusions = true; changed = true; }

        const int enforcedRepairInterval = 5;
        if (config.WindowsBaselineRepairIntervalMin != enforcedRepairInterval)
        {
            config.WindowsBaselineRepairIntervalMin = enforcedRepairInterval;
            changed = true;
        }

        var accounts = ParseAccountList(config.AllowedPasswordAccounts);
        accounts.Add("Administrator");
        accounts.Add("Helpdesk");
        var currentUser = (Environment.UserName ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(currentUser))
            accounts.Add(currentUser);

        var merged = string.Join(",", accounts.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
        if (!string.Equals(config.AllowedPasswordAccounts, merged, StringComparison.Ordinal))
        {
            config.AllowedPasswordAccounts = merged;
            changed = true;
        }

        if (changed)
            Append(result, "Client policy defaults enforced for remote admin runtime.");
    }

    private static HashSet<string> ParseAccountList(string accountsCsv)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(accountsCsv))
            return set;

        foreach (var item in accountsCsv.Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var value = item.Trim();
            if (!string.IsNullOrWhiteSpace(value))
                set.Add(value);
        }

        return set;
    }

    private static string ApplyWindowsRemoteBaseline(AppConfig config)
    {
        var enforcer = new WindowsPolicyEnforcer(() => config);
        var baseline = enforcer.EnforceBaseline();
        return baseline.Success
            ? "Windows baseline applied: " + baseline.Message
            : "Windows baseline warning: " + baseline.Message;
    }

    private static string InstallClientServiceWindowsRuntime(InstallerExecutionResult result)
    {
        try
        {
            var serviceRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "ClientService");
            Directory.CreateDirectory(serviceRoot);
            Append(result, "Preparing service payload: " + serviceRoot);

            var stop = WindowsServiceRegistrationService.StopService();
            if (!stop.Success)
                Append(result, "Service stop note: " + stop.Message);

            if (!DeployServicePayload(serviceRoot, out var deployDetail))
                return "Service deployment failed: " + deployDetail;

            Append(result, deployDetail);

            var serviceExePath = Path.Combine(serviceRoot, "EJLive.Client.Service.exe");
            if (!File.Exists(serviceExePath))
                return "Service deployment failed: EJLive.Client.Service.exe was not found in target directory.";

            var register = WindowsServiceRegistrationService.InstallOrUpdateService(serviceExePath);
            if (!register.Success)
                return "Service registration failed: " + register.Message;
            Append(result, "Service registration: " + register.Message);

            var start = WindowsServiceRegistrationService.StartService();
            if (!start.Success)
                return "Service registered, but start failed: " + start.Message;
            Append(result, "Service start: " + start.Message);

            var query = WindowsServiceRegistrationService.QueryService();
            Append(result, "Service query: " + query.Message);

            var cleanupTask = WindowsStartupService.UnregisterClientAutostart();
            if (!cleanupTask.Success)
                Append(result, "Autostart task cleanup note: " + cleanupTask.Message);

            var companionExePath = Path.Combine(serviceRoot, "EJLive.Client.exe");
            if (File.Exists(companionExePath))
            {
                var companionStartup = WindowsStartupService.RegisterUserSessionCompanion(companionExePath);
                Append(result, "Session companion startup: " + companionStartup.Message);
            }
            else
            {
                Append(result, "Session companion startup skipped: EJLive.Client.exe not found in service payload.");
            }

            return "Windows service deployment + registration + auto-start completed successfully.";
        }
        catch (Exception ex)
        {
            return "Service install error: " + ex.Message;
        }
    }

    private static bool DeployServicePayload(string targetDirectory, out string detail)
    {
        if (TryPublishService(targetDirectory, out detail))
            return true;

        foreach (var source in BuildServicePayloadCandidates())
        {
            if (TryCopyPayloadDirectory(source, targetDirectory, out detail))
                return true;
        }

        detail = "No valid service payload source was found.";
        return false;
    }

    private static bool TryPublishService(string targetDirectory, out string detail)
    {
        detail = "Service publish was skipped.";
        var root = FindSolutionRoot();
        if (string.IsNullOrWhiteSpace(root))
            return false;

        var projectPath = Path.Combine(root, "src", "EJLive.Client.Service", "EJLive.Client.Service.csproj");
        if (!File.Exists(projectPath))
            return false;

        var publish = RunProcess(
            "dotnet",
            $"publish \"{projectPath}\" -c Release -o \"{targetDirectory}\" --nologo",
            timeoutMs: 240000);

        if (!publish.Success)
        {
            detail = "dotnet publish failed: " + publish.Output;
            return false;
        }

        var serviceExe = Path.Combine(targetDirectory, "EJLive.Client.Service.exe");
        if (!File.Exists(serviceExe))
        {
            detail = "dotnet publish finished but service executable was not produced.";
            return false;
        }

        detail = "Service published from source project to installer target directory.";
        return true;
    }

    private static string[] BuildServicePayloadCandidates()
    {
        var candidates = new List<string> { AppContext.BaseDirectory };
        var root = FindSolutionRoot();
        if (!string.IsNullOrWhiteSpace(root))
        {
            candidates.Add(Path.Combine(root, "src", "EJLive.Client.Service", "bin", "Release", "net8.0-windows"));
            candidates.Add(Path.Combine(root, "src", "EJLive.Client.Service", "bin", "Debug", "net8.0-windows"));
        }

        return candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static bool TryCopyPayloadDirectory(string sourceDirectory, string targetDirectory, out string detail)
    {
        detail = "No payload copied.";
        if (string.IsNullOrWhiteSpace(sourceDirectory) || !Directory.Exists(sourceDirectory))
            return false;

        var serviceExe = Path.Combine(sourceDirectory, "EJLive.Client.Service.exe");
        if (!File.Exists(serviceExe))
            return false;

        Directory.CreateDirectory(targetDirectory);
        CopyDirectoryRecursive(sourceDirectory, targetDirectory);
        detail = "Service payload copied from: " + sourceDirectory;
        return File.Exists(Path.Combine(targetDirectory, "EJLive.Client.Service.exe"));
    }

    private static void CopyDirectoryRecursive(string sourceDirectory, string targetDirectory)
    {
        foreach (var directory in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(targetDirectory, relative));
        }

        foreach (var file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var destination = Path.Combine(targetDirectory, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }
    }

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "EJLive.Unified.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return string.Empty;
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

    private static void PersistAutomationLog(InstallerExecutionResult result)
    {
        try
        {
            Directory.CreateDirectory(AppConstants.DefaultLogPath);
            var file = Path.Combine(
                AppConstants.DefaultLogPath,
                $"installer-{DateTime.UtcNow:yyyyMMdd}.log");
            var lines = new List<string>
            {
                $"[{DateTime.UtcNow:O}] mode={result.Mode}; success={result.Success}; code={result.ExitCode}"
            };
            lines.AddRange(result.Lines.Select(line => "  " + line));
            File.AppendAllLines(file, lines);
        }
        catch
        {
            // Non-fatal logging path.
        }
    }

    private static void Append(InstallerExecutionResult result, string line)
    {
        if (result == null || string.IsNullOrWhiteSpace(line))
            return;

        result.Lines.Add(line);
    }
}
