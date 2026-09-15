using System;
using System.IO;
using System.Text.Json;

namespace EJLive.Shared
{
    public partial class AppConfig
    {
        public string ServerIP { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 5656;
        public string ATM_ID { get; set; } = "ATM001";
        public string ATM_Name { get; set; } = "ATM 001";
        public string ATM_Type { get; set; } = "NCR";
        public string NetworkType { get; set; } = "LAN";
        public string SourcePath { get; set; } = @"C:\Journal\";
        public string BackupPath { get; set; } = @"C:\EJLive_BackupLog\";
        public string LogPath { get; set; } = string.Empty;
        public string DatabasePath { get; set; } = string.Empty;
        public bool AutoConnect { get; set; }
        public bool AutoBackup { get; set; }
        public bool AutoEnableRemoteAccess { get; set; }
        public bool AutoPrepareWindowsRuntime { get; set; }
        public bool AllowLocalWindowsPasswordChange { get; set; }
        public bool RequireEncryptedWindowsPasswordPayload { get; set; }
        public bool EnableTlsTransport { get; set; }
        public bool EnableEncryption { get; set; } = true;
        public bool EnableCompression { get; set; } = true;
        public bool RequireTlsTransport { get; set; }
        public bool AllowUntrustedTlsCertificate { get; set; }
        public bool EnableAdaptiveChunking { get; set; }
        public bool ConfigureDefenderExclusions { get; set; }
        public bool EnforceScopedFirewallRule { get; set; }
        public bool EnableRemoteRegistryBootstrap { get; set; }
        public bool EnableWinRmBootstrap { get; set; }
        public bool AllowUnsignedLegacyCommands { get; set; }
        public int HeartbeatIntervalSec { get; set; } = 10;
        public int ReconnectIntervalSec { get; set; } = 15;
        public int WeakNetworkLatencyMs { get; set; } = 500;
        public int ScopedFirewallPort { get; set; } = 8080;
        public int WindowsBaselineRepairIntervalMin { get; set; } = 60;
        public string WindowsPolicyProfileMode { get; set; } = "AuditOnly";
        public string AllowedPasswordAccounts { get; set; } = string.Empty;
        public string ScopedFirewallRemoteAddresses { get; set; } = string.Empty;
        public string DefenderExclusionPaths { get; set; } = string.Empty;
        public string HelpdeskAdGroup { get; set; } = string.Empty;
        public string ImageInboxPath { get; set; } = string.Empty;
        public string ImageDestinationPath { get; set; } = string.Empty;
        public string SupabaseUrl { get; set; } = string.Empty;
        public string SupabaseKey { get; set; } = string.Empty;
        public string SupabaseServiceKey { get; set; } = string.Empty;
        public bool EnableSupabaseSync { get; set; }
        public bool EnforceCommandAuthorization { get; set; }
        public string DefaultCommandRole { get; set; } = string.Empty;
        public bool EnforceLowPriorityMode { get; set; }
        public bool PinToLastProcessorCore { get; set; }
        public bool EnableJournalXfsAnalysis { get; set; } = true;
        public bool EnableCashTelemetry { get; set; } = true;
        public bool EnableSessionCompanionIpc { get; set; }
        public int CashTelemetryIntervalMin { get; set; } = 15;
        public int ScreenshotIntervalMin { get; set; } = 5;
        public int SupabaseFlushIntervalSec { get; set; } = 10;
        public int BootNotificationTimeoutSec { get; set; } = 60;
        public int NetworkMonitorIntervalSec { get; set; } = 15;
        public int LogBackupIntervalHours { get; set; } = 6;
        public int TimeSyncIntervalHours { get; set; } = 1;
        public int LogBackupRetentionDays { get; set; } = 30;
        public string ServerHost { get; set; } = "localhost";
        public string ArchivePath { get; set; } = string.Empty;
        public string EncryptionKey { get; set; } = "default_key";
        public int KeySize { get; set; } = 256;
        public string ClientId { get; set; } = "default_client";
        public string ServerAddress { get; set; } = "localhost";
        public int ClientPort { get; set; } = 8080;
        public bool EnableDebugLogging { get; set; } = false;
        public int MaxLogSizeMB { get; set; } = 10;
        public string WatchPath { get; set; } = string.Empty;
        public int ScanIntervalSeconds { get; set; } = 30;
        public string[] FileExtensions { get; set; } = { ".ej", ".log" };
        public string Theme { get; set; } = "Light";
        public bool EnableNotifications { get; set; } = true;
        public int RefreshIntervalSeconds { get; set; } = 5;
        // Single fixed configuration file under the central data root (SS-20 dataroot).
        private static readonly string DefaultConfigPath = DataRootPaths.ConfigurationFile;
        public static AppConfig Load()
        {
            return Load(DefaultConfigPath);
        }
        public static AppConfig Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null)
                    {
                        config.Normalize();
                        return config;
                    }
                }
            }
            catch { /* Fall back to defaults */ }
            var defaults = new AppConfig();
            defaults.Normalize();
            return defaults;
        }
        public void ApplyDefaults() => Normalize();
        public void Save()
        {
            Save(DefaultConfigPath);
        }
        public void Save(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch { }
        }
        public void EnsureRuntimeFolders()
        {
            // Central layout first so a fresh machine gets the full fixed tree before the
            // capture/backup folders (which are terminal-local) are added on top.
            DataRootPaths.EnsureDirectories();
            try { Directory.CreateDirectory(SourcePath); } catch { }
            try { Directory.CreateDirectory(BackupPath); } catch { }
        }
        private void Normalize()
        {
            // Capture paths stay machine-local to the terminal's journal layout; runtime paths
            // (logs, database, archive, watch, image inbox) hang off the single central data
            // root so every process on the fleet resolves the same fixed location.
            if (string.IsNullOrWhiteSpace(SourcePath))
                SourcePath = @"C:\Journal\";
            if (string.IsNullOrWhiteSpace(BackupPath))
                BackupPath = @"C:\EJLive_BackupLog\";
            if (string.IsNullOrWhiteSpace(LogPath))
                LogPath = DataRootPaths.LogsDirectory;
            if (string.IsNullOrWhiteSpace(DatabasePath))
                DatabasePath = DataRootPaths.DatabaseFile;
            if (string.IsNullOrWhiteSpace(ArchivePath))
                ArchivePath = DataRootPaths.ArchiveDirectory;
            if (string.IsNullOrWhiteSpace(WatchPath))
                WatchPath = DataRootPaths.WatchDirectory;
            if (string.IsNullOrWhiteSpace(ImageInboxPath))
                ImageInboxPath = Path.Combine(DataRootPaths.ImagesDirectory, "Inbox");
        }
    }
}
