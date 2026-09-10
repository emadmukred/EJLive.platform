using System.Xml.Linq;
using EJLive.Core.Models;
using EJLive.Shared;
using AppConfig = EJLive.Shared.AppConfig;

namespace EJLive.Core.Services;

public static class AgentConfigurationXmlService
{
    public static AppConfig LoadAppConfig(AppConfig fallback, string path = "")
    {
        fallback.ApplyDefaults();
        var record = LoadOrCreate(fallback, path);
        if (record.Values.TryGetValue("ATM_ID", out var atmId) && !string.IsNullOrWhiteSpace(atmId))
            fallback.ATM_ID = atmId;
        if (record.Values.TryGetValue("ATM_Name", out var atmName) && !string.IsNullOrWhiteSpace(atmName))
            fallback.ATM_Name = atmName;
        if (record.Values.TryGetValue("ATM_Type", out var atmType) && !string.IsNullOrWhiteSpace(atmType))
            fallback.ATM_Type = atmType;
        if (record.Values.TryGetValue("ServerIP", out var serverIp) && !string.IsNullOrWhiteSpace(serverIp))
            fallback.ServerIP = serverIp;
        if (record.Values.TryGetValue("ServerPort", out var serverPort) && int.TryParse(serverPort, out var port) && port > 0)
            fallback.ServerPort = port;
        if (record.Values.TryGetValue("NetworkType", out var networkType) && !string.IsNullOrWhiteSpace(networkType))
            fallback.NetworkType = networkType;
        if (record.Values.TryGetValue("SourcePath", out var sourcePath) && !string.IsNullOrWhiteSpace(sourcePath))
            fallback.SourcePath = sourcePath;
        if (record.Values.TryGetValue("BackupPath", out var backupPath) && !string.IsNullOrWhiteSpace(backupPath))
            fallback.BackupPath = backupPath;
        if (record.Values.TryGetValue("ImageInboxPath", out var imageInboxPath) && !string.IsNullOrWhiteSpace(imageInboxPath))
            fallback.ImageInboxPath = imageInboxPath;
        if (record.Values.TryGetValue("AutoConnect", out var autoConnect) && bool.TryParse(autoConnect, out var autoConnectValue))
            fallback.AutoConnect = autoConnectValue;
        if (record.Values.TryGetValue("AutoBackup", out var autoBackup) && bool.TryParse(autoBackup, out var autoBackupValue))
            fallback.AutoBackup = autoBackupValue;
        if (record.Values.TryGetValue("EnforceCommandAuthorization", out var enforceCommandAuthorization) &&
            bool.TryParse(enforceCommandAuthorization, out var enforceAuthorization))
        {
            fallback.EnforceCommandAuthorization = enforceAuthorization;
        }
        if (record.Values.TryGetValue("DefaultCommandRole", out var defaultCommandRole) && !string.IsNullOrWhiteSpace(defaultCommandRole))
            fallback.DefaultCommandRole = defaultCommandRole;
        if (record.Values.TryGetValue("EnableSupabaseSync", out var enableSupabaseSync) && bool.TryParse(enableSupabaseSync, out var supabaseValue))
            fallback.EnableSupabaseSync = supabaseValue;
        if (record.Values.TryGetValue("SupabaseUrl", out var supabaseUrl) && !string.IsNullOrWhiteSpace(supabaseUrl))
            fallback.SupabaseUrl = supabaseUrl;
        if (record.Values.TryGetValue("SupabaseServiceKey", out var supabaseServiceKey) && !string.IsNullOrWhiteSpace(supabaseServiceKey))
            fallback.SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(supabaseServiceKey);
        if (record.Values.TryGetValue("AutoEnableRemoteAccess", out var autoEnableRemoteAccess) &&
            bool.TryParse(autoEnableRemoteAccess, out var enableRemote))
        {
            fallback.AutoEnableRemoteAccess = enableRemote;
        }
        if (record.Values.TryGetValue("AutoPrepareWindowsRuntime", out var autoPrepareWindowsRuntime) &&
            bool.TryParse(autoPrepareWindowsRuntime, out var autoPrepare))
        {
            fallback.AutoPrepareWindowsRuntime = autoPrepare;
        }
        if (record.Values.TryGetValue("EnableWinRmBootstrap", out var enableWinRmBootstrap) &&
            bool.TryParse(enableWinRmBootstrap, out var enableWinRm))
        {
            fallback.EnableWinRmBootstrap = enableWinRm;
        }
        if (record.Values.TryGetValue("EnableRemoteRegistryBootstrap", out var enableRemoteRegistryBootstrap) &&
            bool.TryParse(enableRemoteRegistryBootstrap, out var enableRemoteRegistry))
        {
            fallback.EnableRemoteRegistryBootstrap = enableRemoteRegistry;
        }
        if (record.Values.TryGetValue("EnforceScopedFirewallRule", out var enforceScopedFirewallRule) &&
            bool.TryParse(enforceScopedFirewallRule, out var scopedFirewallRule))
        {
            fallback.EnforceScopedFirewallRule = scopedFirewallRule;
        }
        if (record.Values.TryGetValue("ScopedFirewallPort", out var scopedFirewallPort) &&
            int.TryParse(scopedFirewallPort, out var scopedPort) &&
            scopedPort >= 0)
        {
            fallback.ScopedFirewallPort = scopedPort;
        }
        if (record.Values.TryGetValue("ScopedFirewallRemoteAddresses", out var scopedFirewallRemoteAddresses) &&
            !string.IsNullOrWhiteSpace(scopedFirewallRemoteAddresses))
        {
            fallback.ScopedFirewallRemoteAddresses = scopedFirewallRemoteAddresses;
        }
        if (record.Values.TryGetValue("ConfigureDefenderExclusions", out var configureDefenderExclusions) &&
            bool.TryParse(configureDefenderExclusions, out var defenderExclusions))
        {
            fallback.ConfigureDefenderExclusions = defenderExclusions;
        }
        if (record.Values.TryGetValue("DefenderExclusionPaths", out var defenderExclusionPaths) &&
            !string.IsNullOrWhiteSpace(defenderExclusionPaths))
        {
            fallback.DefenderExclusionPaths = defenderExclusionPaths;
        }
        if (record.Values.TryGetValue("HelpdeskAdGroup", out var helpdeskAdGroup) &&
            !string.IsNullOrWhiteSpace(helpdeskAdGroup))
        {
            fallback.HelpdeskAdGroup = helpdeskAdGroup;
        }
        if (record.Values.TryGetValue("WindowsBaselineRepairIntervalMin", out var baselineRepairIntervalMin) &&
            int.TryParse(baselineRepairIntervalMin, out var baselineInterval) &&
            baselineInterval > 0)
        {
            fallback.WindowsBaselineRepairIntervalMin = baselineInterval;
        }
        if (record.Values.TryGetValue("AllowLocalWindowsPasswordChange", out var allowLocalWindowsPasswordChange) &&
            bool.TryParse(allowLocalWindowsPasswordChange, out var allowLocalPasswordChange))
        {
            fallback.AllowLocalWindowsPasswordChange = allowLocalPasswordChange;
        }
        if (record.Values.TryGetValue("RequireEncryptedWindowsPasswordPayload", out var requireEncryptedWindowsPasswordPayload) &&
            bool.TryParse(requireEncryptedWindowsPasswordPayload, out var requireEncryptedWindowsPassword))
        {
            fallback.RequireEncryptedWindowsPasswordPayload = requireEncryptedWindowsPassword;
        }
        if (record.Values.TryGetValue("AllowedPasswordAccounts", out var allowedPasswordAccounts) &&
            !string.IsNullOrWhiteSpace(allowedPasswordAccounts))
        {
            fallback.AllowedPasswordAccounts = allowedPasswordAccounts;
        }
        if (record.Values.TryGetValue("AllowUnsignedLegacyCommands", out var allowUnsignedLegacyCommands) &&
            bool.TryParse(allowUnsignedLegacyCommands, out var allowUnsignedLegacy))
        {
            fallback.AllowUnsignedLegacyCommands = allowUnsignedLegacy;
        }
        if (record.Values.TryGetValue("EnforceLowPriorityMode", out var enforceLowPriorityMode) &&
            bool.TryParse(enforceLowPriorityMode, out var enforceLowPriority))
        {
            fallback.EnforceLowPriorityMode = enforceLowPriority;
        }
        if (record.Values.TryGetValue("PinToLastProcessorCore", out var pinToLastProcessorCore) &&
            bool.TryParse(pinToLastProcessorCore, out var pinToLastCore))
        {
            fallback.PinToLastProcessorCore = pinToLastCore;
        }
        if (record.Values.TryGetValue("HeartbeatIntervalSec", out var heartbeatIntervalSec) &&
            int.TryParse(heartbeatIntervalSec, out var heartbeatSec) &&
            heartbeatSec > 0)
        {
            fallback.HeartbeatIntervalSec = heartbeatSec;
        }
        if (record.Values.TryGetValue("ReconnectIntervalSec", out var reconnectIntervalSec) &&
            int.TryParse(reconnectIntervalSec, out var reconnectSec) &&
            reconnectSec > 0)
        {
            fallback.ReconnectIntervalSec = reconnectSec;
        }
        fallback.ApplyDefaults();
        return fallback;
    }

    public static AppConfig LoadAppConfig(AppConfig fallback) => LoadAppConfig(fallback, string.Empty);

    public static AgentConfigurationRecord LoadOrCreate(AppConfig config, string path = "")
    {
        var resolvedPath = ResolveConfigPath(path);

        var folder = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(folder))
            Directory.CreateDirectory(folder);

        if (!File.Exists(resolvedPath))
            SaveAppConfig(config, resolvedPath);

        var loaded = XDocument.Load(resolvedPath);
        var record = new AgentConfigurationRecord { ConfigPath = resolvedPath };
        foreach (var element in loaded.Root?.Elements() ?? Enumerable.Empty<XElement>())
            record.Values[element.Name.LocalName] = element.Value;
        return record;
    }

    public static void SaveAppConfig(AppConfig config, string path = "")
    {
        var resolvedPath = ResolveConfigPath(path);
        var folder = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(folder))
            Directory.CreateDirectory(folder);

        var doc = BuildXml(config);
        doc.Save(resolvedPath);
    }

    private static string ResolveConfigPath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Client", "AgentConf.xml")
            : path;
    }

    private static XDocument BuildXml(AppConfig config)
    {
        return new XDocument(
            new XElement("AgentConfiguration",
                new XElement("ATM_ID", config.ATM_ID),
                new XElement("ATM_Name", config.ATM_Name),
                new XElement("ATM_Type", config.ATM_Type),
                new XElement("ServerIP", config.ServerIP),
                new XElement("ServerPort", config.ServerPort),
                new XElement("NetworkType", config.NetworkType),
                new XElement("SourcePath", config.SourcePath),
                new XElement("BackupPath", config.BackupPath),
                new XElement("ImageInboxPath", config.ImageInboxPath),
                new XElement("AutoConnect", config.AutoConnect),
                new XElement("AutoBackup", config.AutoBackup),
                new XElement("EnforceCommandAuthorization", config.EnforceCommandAuthorization),
                new XElement("DefaultCommandRole", config.DefaultCommandRole),
                new XElement("EnableSupabaseSync", config.EnableSupabaseSync),
                new XElement("SupabaseUrl", config.SupabaseUrl),
                new XElement("SupabaseServiceKey", SecurityHelper.ProtectDpapiStringIfNeeded(config.SupabaseServiceKey)),
                new XElement("AutoEnableRemoteAccess", config.AutoEnableRemoteAccess),
                new XElement("AutoPrepareWindowsRuntime", config.AutoPrepareWindowsRuntime),
                new XElement("EnableWinRmBootstrap", config.EnableWinRmBootstrap),
                new XElement("EnableRemoteRegistryBootstrap", config.EnableRemoteRegistryBootstrap),
                new XElement("EnforceScopedFirewallRule", config.EnforceScopedFirewallRule),
                new XElement("ScopedFirewallPort", config.ScopedFirewallPort),
                new XElement("ScopedFirewallRemoteAddresses", config.ScopedFirewallRemoteAddresses),
                new XElement("ConfigureDefenderExclusions", config.ConfigureDefenderExclusions),
                new XElement("DefenderExclusionPaths", config.DefenderExclusionPaths),
                new XElement("HelpdeskAdGroup", config.HelpdeskAdGroup),
                new XElement("WindowsBaselineRepairIntervalMin", config.WindowsBaselineRepairIntervalMin),
                new XElement("AllowLocalWindowsPasswordChange", config.AllowLocalWindowsPasswordChange),
                new XElement("RequireEncryptedWindowsPasswordPayload", config.RequireEncryptedWindowsPasswordPayload),
                new XElement("AllowedPasswordAccounts", config.AllowedPasswordAccounts),
                new XElement("AllowUnsignedLegacyCommands", config.AllowUnsignedLegacyCommands),
                new XElement("EnforceLowPriorityMode", config.EnforceLowPriorityMode),
                new XElement("PinToLastProcessorCore", config.PinToLastProcessorCore),
                new XElement("HeartbeatIntervalSec", config.HeartbeatIntervalSec),
                new XElement("ReconnectIntervalSec", config.ReconnectIntervalSec)));
    }
}
