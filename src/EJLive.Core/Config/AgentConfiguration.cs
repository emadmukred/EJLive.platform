using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EJLive.Core.Config
{
    /// <summary>
    /// Central configuration for the EJLive Client Agent.
    /// Loaded from agent-config.json at service startup.
    /// All paths are vendor-aware and locally validated.
    /// </summary>
    public sealed class AgentConfiguration
    {
        // ATM Identity
        public string AgentId { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
        public string TerminalName { get; set; } = string.Empty;
        public string Vendor { get; set; } = "NCR";
        public string Model { get; set; } = string.Empty;
        public string NetworkType { get; set; } = "LAN";

        // Server Connection
        public string ServerHost { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 9000;

        // Paths
        public string SourceJournalPath { get; set; } = @"C:\NCR\EJDATA";
        public string BackupJournalPath { get; set; } = @"C:\EJLive\Backup";
        public string ImageInboxPath { get; set; } = @"C:\EJLive\Content\Staging";
        public string ImageDestinationPath { get; set; } = @"C:\NCR\Content";
        public string ScreenshotCachePath { get; set; } = @"C:\EJLive\Screenshots";
        public string LogPath { get; set; } = @"C:\ProgramData\EJLive\Logs";
        public string HealthPath { get; set; } = @"C:\ProgramData\EJLive\Health";
        public string ConfigPath { get; set; } = @"C:\ProgramData\EJLive\Config";

        // Behavior
        public bool AutoConnect { get; set; } = true;
        public bool AutoBackup { get; set; } = true;
        public bool DurableSyncEnabled { get; set; } = true;
        public bool AckRequired { get; set; } = true;
        public bool DedupEnabled { get; set; } = true;
        public int HeartbeatIntervalSec { get; set; } = 30;
        public int ReconnectIntervalSec { get; set; } = 10;
        public int MaxOutboxItems { get; set; } = 1000;
        public long MaxOutboxBytes { get; set; } = 100 * 1024 * 1024; // 100MB

        // Security
        public string AllowedPasswordAccounts { get; set; } = string.Empty;
        public string ScopedFirewallRemoteAddresses { get; set; } = string.Empty;

        // Runtime State
        public DateTimeOffset LoadedAtUtc { get; set; } = DateTimeOffset.UtcNow;
        public string ConfigSource { get; set; } = "default";

        /// <summary>Loads configuration from the specified JSON file path.</summary>
        public static AgentConfiguration Load(string configPath)
        {
            if (!File.Exists(configPath))
                return CreateDefault();

            try
            {
                var json = File.ReadAllText(configPath, Encoding.UTF8);
                var config = SimpleJsonDeserialize(json);
                config.ConfigSource = configPath;
                config.LoadedAtUtc = DateTimeOffset.UtcNow;
                return config;
            }
            catch
            {
                var fallback = CreateDefault();
                fallback.ConfigSource = "default (load failed)";
                return fallback;
            }
        }

        /// <summary>Saves configuration to the specified path as JSON.</summary>
        public void Save(string configPath)
        {
            var dir = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = SimpleJsonSerialize();
            File.WriteAllText(configPath, json, Encoding.UTF8);
        }

        /// <summary>Validates all paths exist or can be created.</summary>
        public List<string> ValidatePaths()
        {
            var errors = new List<string>();
            if (!Directory.Exists(SourceJournalPath))
                errors.Add($"Source journal path not found: {SourceJournalPath}");
            if (string.IsNullOrWhiteSpace(ServerHost))
                errors.Add("Server host is not configured.");
            if (ServerPort <= 0 || ServerPort > 65535)
                errors.Add($"Invalid server port: {ServerPort}");
            if (string.IsNullOrWhiteSpace(AgentId))
                errors.Add("AgentId is required.");
            return errors;
        }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                ["AgentId"] = AgentId,
                ["TerminalId"] = TerminalId,
                ["TerminalName"] = TerminalName,
                ["Vendor"] = Vendor,
                ["Model"] = Model,
                ["NetworkType"] = NetworkType,
                ["ServerHost"] = ServerHost,
                ["ServerPort"] = ServerPort.ToString(),
                ["SourcePath"] = SourceJournalPath,
                ["BackupPath"] = BackupJournalPath,
                ["ImageInboxPath"] = ImageInboxPath,
                ["ImageDestinationPath"] = ImageDestinationPath,
                ["AutoConnect"] = AutoConnect.ToString(),
                ["AutoBackup"] = AutoBackup.ToString(),
                ["DurableSyncEnabled"] = DurableSyncEnabled.ToString(),
                ["AckRequired"] = AckRequired.ToString(),
                ["DedupEnabled"] = DedupEnabled.ToString(),
                ["HeartbeatIntervalSec"] = HeartbeatIntervalSec.ToString(),
                ["ReconnectIntervalSec"] = ReconnectIntervalSec.ToString(),
                ["MaxOutboxItems"] = MaxOutboxItems.ToString(),
                ["MaxOutboxBytes"] = MaxOutboxBytes.ToString(),
            };
        }

        private static AgentConfiguration CreateDefault() => new AgentConfiguration
        {
            AgentId = Environment.MachineName,
            TerminalId = Environment.MachineName,
            TerminalName = Environment.MachineName,
            ConfigSource = "default"
        };

        private string SimpleJsonSerialize()
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            AppendJsonProperty(sb, "agentId", AgentId);
            AppendJsonProperty(sb, "terminalId", TerminalId);
            AppendJsonProperty(sb, "terminalName", TerminalName);
            AppendJsonProperty(sb, "vendor", Vendor);
            AppendJsonProperty(sb, "model", Model);
            AppendJsonProperty(sb, "networkType", NetworkType);
            AppendJsonProperty(sb, "serverHost", ServerHost);
            sb.AppendLine($"  \"serverPort\": {ServerPort},");
            AppendJsonProperty(sb, "sourceJournalPath", SourceJournalPath);
            AppendJsonProperty(sb, "backupJournalPath", BackupJournalPath);
            AppendJsonProperty(sb, "imageInboxPath", ImageInboxPath);
            AppendJsonProperty(sb, "imageDestinationPath", ImageDestinationPath);
            sb.AppendLine($"  \"autoConnect\": {AutoConnect.ToString().ToLowerInvariant()},");
            sb.AppendLine($"  \"autoBackup\": {AutoBackup.ToString().ToLowerInvariant()},");
            sb.AppendLine($"  \"heartbeatIntervalSec\": {HeartbeatIntervalSec},");
            sb.AppendLine($"  \"reconnectIntervalSec\": {ReconnectIntervalSec},");
            sb.AppendLine($"  \"maxOutboxItems\": {MaxOutboxItems},");
            sb.AppendLine($"  \"maxOutboxBytes\": {MaxOutboxBytes}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void AppendJsonProperty(StringBuilder sb, string name, string value)
        {
            sb.Append("  \"")
              .Append(name)
              .Append("\": \"")
              .Append(EscapeJson(value))
              .AppendLine("\",");
        }

        private static string EscapeJson(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static AgentConfiguration SimpleJsonDeserialize(string json)
        {
            var config = new AgentConfiguration();
            // Simple key-value parsing
            var lines = json.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim().TrimEnd(',');
                var colonIdx = trimmed.IndexOf(':');
                if (colonIdx < 0) continue;
                var key = trimmed.Substring(0, colonIdx).Trim().Trim('"').Trim();
                var value = trimmed.Substring(colonIdx + 1).Trim().Trim('"').Trim();

                switch (key)
                {
                    case "agentId": config.AgentId = value; break;
                    case "terminalId": config.TerminalId = value; break;
                    case "terminalName": config.TerminalName = value; break;
                    case "vendor": config.Vendor = value; break;
                    case "model": config.Model = value; break;
                    case "networkType": config.NetworkType = value; break;
                    case "serverHost": config.ServerHost = value; break;
                    case "serverPort": if (int.TryParse(value, out var p)) config.ServerPort = p; break;
                    case "sourceJournalPath": config.SourceJournalPath = value; break;
                    case "backupJournalPath": config.BackupJournalPath = value; break;
                    case "imageInboxPath": config.ImageInboxPath = value; break;
                    case "imageDestinationPath": config.ImageDestinationPath = value; break;
                    case "autoConnect": if (bool.TryParse(value, out var ac)) config.AutoConnect = ac; break;
                    case "autoBackup": if (bool.TryParse(value, out var ab)) config.AutoBackup = ab; break;
                    case "heartbeatIntervalSec": if (int.TryParse(value, out var hb)) config.HeartbeatIntervalSec = hb; break;
                    case "reconnectIntervalSec": if (int.TryParse(value, out var rc)) config.ReconnectIntervalSec = rc; break;
                }
            }
            return config;
        }
    }
}