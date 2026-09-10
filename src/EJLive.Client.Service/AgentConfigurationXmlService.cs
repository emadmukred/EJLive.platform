using System.Text;
using System.Xml;
using System.Xml.Linq;
using EJLive.Core.Models;
using EJLive.Shared;
using AppConfig = EJLive.Core.Models.AppConfig;
using SecurityHelper = EJLive.Shared.SecurityHelper;

namespace EJLive.Client.Service;

/// <summary>
/// Reads and writes the service-owned AgentConf.xml compatibility file.
/// The implementation accepts both the historical nested schema and the
/// current flat schema, prohibits DTD processing, and writes atomically.
/// </summary>
public static class AgentConfigurationXmlService
{
    private const long MaximumDocumentBytes = 2 * 1024 * 1024;

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "EJLive",
        "Client",
        "AgentConf.xml");

    public static AppConfig LoadAppConfig(AppConfig? fallback = null, string? path = null)
    {
        var config = fallback ?? new AppConfig();
        config.ApplyDefaults();

        var record = LoadOrCreate(config, path);
        ApplyString(record, "ATM_ID", value => config.ATM_ID = value);
        ApplyString(record, "ATM_Name", value => config.ATM_Name = value);
        ApplyString(record, "ATM_Type", value => config.ATM_Type = value);
        ApplyString(record, "ServerIP", value => config.ServerIP = value);
        ApplyInt(record, "ServerPort", value => config.ServerPort = value, value => value is >= 1 and <= 65535);
        ApplyString(record, "NetworkType", value => config.NetworkType = value);
        ApplyBool(record, "SyncTimeEnabled", value => config.SyncTimeEnabled = value);
        ApplyInt(record, "MessageSizeLines", value => config.MessageSizeLines = value, value => value > 0);
        ApplyInt(record, "FilePackageKB", value => config.FilePackageKB = value, value => value > 0);
        ApplyString(record, "SourcePath", value => config.SourcePath = value);
        ApplyString(record, "BackupPath", value => config.BackupPath = value);
        ApplyBool(record, "AutoStart", value => config.AutoStart = value);
        ApplyBool(record, "RunAsService", value => config.RunAsService = value);
        ApplyBool(record, "AutoConnect", value => config.AutoConnect = value);
        ApplyBool(record, "EnableEncryption", value => config.EnableEncryption = value);
        ApplyBool(record, "EnableCompression", value => config.EnableCompression = value);
        ApplyBool(record, "EnableTlsTransport", value => config.EnableTlsTransport = value);
        ApplyBool(record, "RequireTlsTransport", value => config.RequireTlsTransport = value);
        ApplyBool(record, "AllowUntrustedTlsCertificate", value => config.AllowUntrustedTlsCertificate = value);
        ApplyBool(record, "EnableAdaptiveChunking", value => config.EnableAdaptiveChunking = value);
        ApplyInt(record, "WeakNetworkLatencyMs", value => config.WeakNetworkLatencyMs = value, value => value > 0);
        ApplyBool(record, "AutoBackup", value => config.AutoBackup = value);
        ApplyBool(record, "EnforceCommandAuthorization", value => config.EnforceCommandAuthorization = value);
        ApplyString(record, "DefaultCommandRole", value => config.DefaultCommandRole = value);
        ApplyBool(record, "EnableSupabaseSync", value => config.EnableSupabaseSync = value);
        ApplyString(record, "SupabaseUrl", value => config.SupabaseUrl = value);
        ApplyString(
            record,
            "SupabaseServiceKey",
            value => config.SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(value),
            allowEmpty: true);
        ApplyString(record, "ImageInboxPath", value => config.ImageInboxPath = value);
        ApplyBool(record, "AutoEnableRemoteAccess", value => config.AutoEnableRemoteAccess = value);
        ApplyBool(record, "AutoPrepareWindowsRuntime", value => config.AutoPrepareWindowsRuntime = value);
        ApplyBool(record, "EnableWinRmBootstrap", value => config.EnableWinRmBootstrap = value);
        ApplyBool(record, "EnableRemoteRegistryBootstrap", value => config.EnableRemoteRegistryBootstrap = value);
        ApplyBool(record, "EnforceScopedFirewallRule", value => config.EnforceScopedFirewallRule = value);
        ApplyInt(record, "ScopedFirewallPort", value => config.ScopedFirewallPort = value, value => value is >= 0 and <= 65535);
        ApplyString(record, "ScopedFirewallRemoteAddresses", value => config.ScopedFirewallRemoteAddresses = value, allowEmpty: true);
        ApplyBool(record, "ConfigureDefenderExclusions", value => config.ConfigureDefenderExclusions = value);
        ApplyString(record, "DefenderExclusionPaths", value => config.DefenderExclusionPaths = value, allowEmpty: true);
        ApplyString(record, "HelpdeskAdGroup", value => config.HelpdeskAdGroup = value);
        ApplyInt(
            record,
            "WindowsBaselineRepairIntervalMin",
            value => config.WindowsBaselineRepairIntervalMin = value,
            value => value > 0);
        ApplyString(record, "WindowsPolicyProfileMode", value => config.WindowsPolicyProfileMode = value);
        ApplyBool(record, "AllowLocalWindowsPasswordChange", value => config.AllowLocalWindowsPasswordChange = value);
        ApplyBool(
            record,
            "RequireEncryptedWindowsPasswordPayload",
            value => config.RequireEncryptedWindowsPasswordPayload = value);
        ApplyString(record, "AllowedPasswordAccounts", value => config.AllowedPasswordAccounts = value);
        ApplyBool(record, "EnforceLowPriorityMode", value => config.EnforceLowPriorityMode = value);
        ApplyBool(record, "PinToLastProcessorCore", value => config.PinToLastProcessorCore = value);
        ApplyInt(record, "HeartbeatIntervalSec", value => config.HeartbeatIntervalSec = value, value => value > 0);
        ApplyInt(record, "ReconnectIntervalSec", value => config.ReconnectIntervalSec = value, value => value > 0);

        config.ApplyDefaults();
        return config;
    }

    public static AgentConfigurationRecord LoadOrCreate(AppConfig seed, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(seed);

        var resolvedPath = ResolveConfigPath(path);
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        if (!File.Exists(resolvedPath))
            SaveAppConfig(seed, resolvedPath);

        var fileInfo = new FileInfo(resolvedPath);
        if (fileInfo.Length > MaximumDocumentBytes)
            throw new InvalidDataException($"Agent configuration exceeds {MaximumDocumentBytes} bytes.");

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = MaximumDocumentBytes,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };

        using var stream = new FileStream(
            resolvedPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using var reader = XmlReader.Create(stream, settings);
        var document = XDocument.Load(reader, LoadOptions.None);

        return ParseRecord(document, resolvedPath);
    }

    public static void SaveAppConfig(AppConfig config, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(config);

        config.ApplyDefaults();
        var resolvedPath = ResolveConfigPath(path);
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var document = BuildXml(config);
        var temporaryPath = resolvedPath + ".tmp-" + Guid.NewGuid().ToString("N");

        try
        {
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                Indent = true,
                NewLineHandling = NewLineHandling.Entitize
            };

            using (var stream = new FileStream(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 16 * 1024,
                       FileOptions.WriteThrough))
            using (var writer = XmlWriter.Create(stream, settings))
            {
                document.Save(writer);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, resolvedPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static AgentConfigurationRecord ParseRecord(XDocument document, string path)
    {
        var root = document.Root ?? throw new InvalidDataException("Agent configuration has no root element.");
        var record = new AgentConfigurationRecord
        {
            ConfigPath = path,
            LoadedAtUtc = DateTime.UtcNow
        };

        if (root.Name.LocalName.Equals("AgentConfiguration", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var element in root.Elements().Where(element => !element.HasElements))
                record.Values[element.Name.LocalName] = element.Value;

            NormalizeAliases(record);
            return record;
        }

        if (!root.Name.LocalName.Equals("AgentConf", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"Unsupported agent configuration root '{root.Name.LocalName}'.");
        }

        var terminal = root.Element("Terminal");
        var server = root.Element("Server");
        var paths = root.Element("Paths");
        var sync = root.Element("Sync");

        AddValue(record, "ATM_ID", ReadAttributeOrElement(terminal, "TerminalId", "TerminalId") ??
                                   ReadAttributeOrElement(root, "AgentId", "AgentId"));
        AddValue(record, "ATM_Name", ReadAttributeOrElement(terminal, "TerminalName", "ATM_Name"));
        AddValue(record, "ATM_Type", ReadAttributeOrElement(terminal, "Vendor", "ATM_Type"));
        AddValue(record, "NetworkType", ReadAttributeOrElement(terminal, "NetworkType", "NetworkType"));
        AddValue(record, "ServerIP", ReadAttributeOrElement(server, "Host", "ServerIP"));
        AddValue(record, "ServerPort", ReadAttributeOrElement(server, "Port", "ServerPort"));
        AddValue(record, "SourcePath", ReadAttributeOrElement(paths, "Source", "SourcePath"));
        AddValue(record, "BackupPath", ReadAttributeOrElement(paths, "Backup", "BackupPath"));
        AddValue(record, "AutoConnect", ReadAttributeOrElement(sync, "AutoConnect", "AutoConnect"));

        foreach (var parameter in root.Element("Parameters")?.Elements("Param") ?? Enumerable.Empty<XElement>())
        {
            var key = parameter.Attribute("Key")?.Value;
            if (!string.IsNullOrWhiteSpace(key))
                record.Values[key.Trim()] = parameter.Attribute("Value")?.Value ?? parameter.Value;
        }

        NormalizeAliases(record);
        return record;
    }

    private static XDocument BuildXml(AppConfig config)
    {
        var protectedSupabaseKey = ProtectSecret(config.SupabaseServiceKey);

        return new XDocument(
            new XElement(
                "AgentConfiguration",
                new XAttribute("Version", "2.0"),
                new XAttribute("GeneratedAtUtc", DateTime.UtcNow.ToString("O")),
                Element("ATM_ID", config.ATM_ID),
                Element("ATM_Name", config.ATM_Name),
                Element("ATM_Type", config.ATM_Type),
                Element("ServerIP", config.ServerIP),
                Element("ServerPort", config.ServerPort),
                Element("NetworkType", config.NetworkType),
                Element("SyncTimeEnabled", config.SyncTimeEnabled),
                Element("MessageSizeLines", config.MessageSizeLines),
                Element("FilePackageKB", config.FilePackageKB),
                Element("SourcePath", config.SourcePath),
                Element("BackupPath", config.BackupPath),
                Element("AutoStart", config.AutoStart),
                Element("RunAsService", config.RunAsService),
                Element("AutoConnect", config.AutoConnect),
                Element("EnableEncryption", config.EnableEncryption),
                Element("EnableCompression", config.EnableCompression),
                Element("EnableTlsTransport", config.EnableTlsTransport),
                Element("RequireTlsTransport", config.RequireTlsTransport),
                Element("AllowUntrustedTlsCertificate", config.AllowUntrustedTlsCertificate),
                Element("EnableAdaptiveChunking", config.EnableAdaptiveChunking),
                Element("WeakNetworkLatencyMs", config.WeakNetworkLatencyMs),
                Element("AutoBackup", config.AutoBackup),
                Element("EnforceCommandAuthorization", config.EnforceCommandAuthorization),
                Element("DefaultCommandRole", config.DefaultCommandRole),
                Element("EnableSupabaseSync", config.EnableSupabaseSync),
                Element("SupabaseUrl", config.SupabaseUrl),
                Element("SupabaseServiceKey", protectedSupabaseKey),
                Element("ImageInboxPath", config.ImageInboxPath),
                Element("AutoEnableRemoteAccess", config.AutoEnableRemoteAccess),
                Element("AutoPrepareWindowsRuntime", config.AutoPrepareWindowsRuntime),
                Element("EnableWinRmBootstrap", config.EnableWinRmBootstrap),
                Element("EnableRemoteRegistryBootstrap", config.EnableRemoteRegistryBootstrap),
                Element("EnforceScopedFirewallRule", config.EnforceScopedFirewallRule),
                Element("ScopedFirewallPort", config.ScopedFirewallPort),
                Element("ScopedFirewallRemoteAddresses", config.ScopedFirewallRemoteAddresses),
                Element("ConfigureDefenderExclusions", config.ConfigureDefenderExclusions),
                Element("DefenderExclusionPaths", config.DefenderExclusionPaths),
                Element("HelpdeskAdGroup", config.HelpdeskAdGroup),
                Element("WindowsBaselineRepairIntervalMin", config.WindowsBaselineRepairIntervalMin),
                Element("WindowsPolicyProfileMode", config.WindowsPolicyProfileMode),
                Element("AllowLocalWindowsPasswordChange", config.AllowLocalWindowsPasswordChange),
                Element("RequireEncryptedWindowsPasswordPayload", config.RequireEncryptedWindowsPasswordPayload),
                Element("AllowedPasswordAccounts", config.AllowedPasswordAccounts),
                Element("EnforceLowPriorityMode", config.EnforceLowPriorityMode),
                Element("PinToLastProcessorCore", config.PinToLastProcessorCore),
                Element("HeartbeatIntervalSec", config.HeartbeatIntervalSec),
                Element("ReconnectIntervalSec", config.ReconnectIntervalSec)));
    }

    private static XElement Element(string name, object? value) =>
        new(name, value ?? string.Empty);

    private static string ProtectSecret(string? secret)
    {
        if (string.IsNullOrEmpty(secret))
            return string.Empty;

        var protectedValue = SecurityHelper.ProtectDpapiStringIfNeeded(secret);
        if (!protectedValue.StartsWith("dpapi:", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Unable to protect the Supabase service key with machine-level DPAPI.");
        }

        return protectedValue;
    }

    private static string ResolveConfigPath(string? path) =>
        Path.GetFullPath(string.IsNullOrWhiteSpace(path) ? DefaultPath : path.Trim());

    private static void ApplyString(
        AgentConfigurationRecord record,
        string key,
        Action<string> setter,
        bool allowEmpty = false)
    {
        if (!record.Values.TryGetValue(key, out var value))
            return;

        value = value?.Trim() ?? string.Empty;
        if (allowEmpty || value.Length > 0)
            setter(value);
    }

    private static void ApplyBool(
        AgentConfigurationRecord record,
        string key,
        Action<bool> setter)
    {
        if (record.Values.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed))
            setter(parsed);
    }

    private static void ApplyInt(
        AgentConfigurationRecord record,
        string key,
        Action<int> setter,
        Func<int, bool> predicate)
    {
        if (record.Values.TryGetValue(key, out var value) &&
            int.TryParse(value, out var parsed) &&
            predicate(parsed))
        {
            setter(parsed);
        }
    }

    private static void AddValue(AgentConfigurationRecord record, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            record.Values[key] = value.Trim();
    }

    private static void NormalizeAliases(AgentConfigurationRecord record)
    {
        CopyAlias(record, "TerminalId", "ATM_ID");
        CopyAlias(record, "AgentId", "ATM_ID");
        CopyAlias(record, "TerminalName", "ATM_Name");
        CopyAlias(record, "Vendor", "ATM_Type");
        CopyAlias(record, "ServerHost", "ServerIP");
    }

    private static void CopyAlias(
        AgentConfigurationRecord record,
        string sourceKey,
        string targetKey)
    {
        if (record.Values.ContainsKey(targetKey))
            return;

        if (record.Values.TryGetValue(sourceKey, out var value) && !string.IsNullOrWhiteSpace(value))
            record.Values[targetKey] = value.Trim();
    }

    private static string? ReadAttributeOrElement(XElement? parent, string attributeName, string elementName) =>
        parent?.Attribute(attributeName)?.Value ?? parent?.Element(elementName)?.Value;
}
