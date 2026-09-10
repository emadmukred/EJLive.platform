using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EJLive.Shared;

namespace EJLive.Core.Models
{
    public partial class AppConfig : ClientConfig
        {
            private static string ConfigPath => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Client", "appconfig.json");
    
    
            public bool AutoConnect { get; set; } = true;
    
    
            public bool EnableEncryption { get; set; } = true;
    
    
            public bool EnableCompression { get; set; } = true;
    
    
            public bool EnableTlsTransport { get; set; }
    
    
            public bool RequireTlsTransport { get; set; }
    
    
            public bool AllowUntrustedTlsCertificate { get; set; }
    
    
            public bool EnableAdaptiveChunking { get; set; } = true;
    
    
            public int WeakNetworkLatencyMs { get; set; } = 500;
    
    
            public bool AutoBackup { get; set; } = true;
    
    
            public bool EnforceCommandAuthorization { get; set; } = true;
    
    
            public string DefaultCommandRole { get; set; } = "Support";
    
    
            public bool EnableSupabaseSync { get; set; }
    
    
            public string SupabaseUrl { get; set; } = string.Empty;
    
    
            public string SupabaseServiceKey { get; set; } = string.Empty;
    
    
            public string ImageInboxPath { get; set; } = Path.Combine(AppConstants.DefaultImagesPath, "Inbox");
    
    
            public bool AutoEnableRemoteAccess { get; set; } = true;
    
    
            public bool AutoPrepareWindowsRuntime { get; set; } = true;
    
    
            public bool EnableWinRmBootstrap { get; set; } = true;
    
    
            public bool EnableRemoteRegistryBootstrap { get; set; } = true;
    
    
            public bool EnforceScopedFirewallRule { get; set; } = true;
    
    
            public int ScopedFirewallPort { get; set; }
    
    
            public string ScopedFirewallRemoteAddresses { get; set; } = string.Empty;
    
    
            public bool ConfigureDefenderExclusions { get; set; } = true;
    
    
            public string DefenderExclusionPaths { get; set; } = string.Empty;
    
    
            public string HelpdeskAdGroup { get; set; } = "EJLive-Helpdesk";
    
    
            public int WindowsBaselineRepairIntervalMin { get; set; } = 30;
    
    
            public string WindowsPolicyProfileMode { get; set; } = "Enforce";
    
    
            public bool AllowLocalWindowsPasswordChange { get; set; }
    
    
            public bool RequireEncryptedWindowsPasswordPayload { get; set; } = true;
    
    
            public string AllowedPasswordAccounts { get; set; } = "Administrator,Helpdesk";
    
    
            public bool AllowUnsignedLegacyCommands { get; set; }
    
    
            public bool EnforceLowPriorityMode { get; set; } = true;
    
    
            public bool PinToLastProcessorCore { get; set; }
    
    
            public int HeartbeatIntervalSec { get; set; } = AppConstants.HeartbeatIntervalSec;
    
    
            public int ReconnectIntervalSec { get; set; } = 20;
    
    
            public static AppConfig Load()
            {
                try
                {
                    if (!File.Exists(ConfigPath))
                        return new AppConfig();
    
                    var loaded = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigPath)) ?? new AppConfig();
                    loaded.SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(loaded.SupabaseServiceKey);
                    loaded.ApplyDefaults();
                    return loaded;
                }
                catch
                {
                    return new AppConfig();
                }
            }
    
    
            public void Save()
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
                var serialized = JsonSerializer.Serialize(this);
                var copy = JsonSerializer.Deserialize<AppConfig>(serialized) ?? new AppConfig();
                copy.SupabaseServiceKey = SecurityHelper.ProtectDpapiStringIfNeeded(copy.SupabaseServiceKey);
    
                File.WriteAllText(
                    ConfigPath,
                    JsonSerializer.Serialize(copy, new JsonSerializerOptions { WriteIndented = true }));
            }
    
    
            public void ApplyDefaults()
            {
                ATM_Type = AppConstants.NormalizeATMType(ATM_Type);
                if (string.IsNullOrWhiteSpace(SourcePath))
                    SourcePath = AppConstants.GetDefaultSourcePath(ATM_Type);
                if (string.IsNullOrWhiteSpace(BackupPath))
                    BackupPath = AppConstants.GetDefaultBackupPath(ATM_Type);
                if (string.IsNullOrWhiteSpace(ImageInboxPath))
                    ImageInboxPath = Path.Combine(AppConstants.DefaultImagesPath, "Inbox");
                if (string.IsNullOrWhiteSpace(DefaultCommandRole))
                    DefaultCommandRole = "Support";
                else
                    DefaultCommandRole = DefaultCommandRole.Trim();
                if (HeartbeatIntervalSec <= 0)
                    HeartbeatIntervalSec = AppConstants.HeartbeatIntervalSec;
                if (ReconnectIntervalSec <= 0)
                    ReconnectIntervalSec = 20;
                WeakNetworkLatencyMs = Math.Clamp(WeakNetworkLatencyMs, 120, 3000);
                if (WindowsBaselineRepairIntervalMin <= 0)
                    WindowsBaselineRepairIntervalMin = 30;
                if (string.IsNullOrWhiteSpace(WindowsPolicyProfileMode))
                    WindowsPolicyProfileMode = "Enforce";
                else
                    WindowsPolicyProfileMode = WindowsPolicyProfileMode.Trim();
                HeartbeatIntervalSec = Math.Clamp(HeartbeatIntervalSec, 5, 300);
                ReconnectIntervalSec = Math.Clamp(ReconnectIntervalSec, 5, 300);
                WindowsBaselineRepairIntervalMin = Math.Clamp(WindowsBaselineRepairIntervalMin, 5, 720);
                SupabaseUrl = (SupabaseUrl ?? string.Empty).Trim();
                SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(SupabaseServiceKey).Trim();
                AllowedPasswordAccounts = (AllowedPasswordAccounts ?? string.Empty).Trim();
                ScopedFirewallRemoteAddresses = string.IsNullOrWhiteSpace(ScopedFirewallRemoteAddresses)
                    ? (ServerIP ?? string.Empty).Trim()
                    : ScopedFirewallRemoteAddresses.Trim();
                DefenderExclusionPaths = (DefenderExclusionPaths ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(HelpdeskAdGroup))
                    HelpdeskAdGroup = "EJLive-Helpdesk";
                else
                    HelpdeskAdGroup = HelpdeskAdGroup.Trim();
                ScopedFirewallPort = Math.Clamp(ScopedFirewallPort, 0, 65535);
                if (ServerPort <= 0)
                    ServerPort = NetworkConfig.DEFAULT_PORT;
            }
    
    
        }
    /// <summary>
        /// Application-level configuration persisted to JSON with DPAPI-protected secrets.
        /// Extends the base ClientConfig with advanced policy, transport, and Windows baseline settings.
        /// </summary>
        public sealed class AppConfig : ClientConfig
        {
            private static string ConfigPath => Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive", "Client", "appconfig.json");
    
            public bool AutoConnect { get; set; } = true;
            public bool EnableEncryption { get; set; } = true;
            public bool EnableCompression { get; set; } = true;
            public bool EnableTlsTransport { get; set; }
            public bool RequireTlsTransport { get; set; }
            public bool AllowUntrustedTlsCertificate { get; set; }
            public bool EnableAdaptiveChunking { get; set; } = true;
            public int WeakNetworkLatencyMs { get; set; } = 500;
            public bool AutoBackup { get; set; } = true;
            public bool EnforceCommandAuthorization { get; set; } = true;
            public string DefaultCommandRole { get; set; } = "Support";
            public bool EnableSupabaseSync { get; set; }
            public string SupabaseUrl { get; set; } = string.Empty;
            public string SupabaseServiceKey { get; set; } = string.Empty;
            public string ImageInboxPath { get; set; } = Path.Combine(AppConstants.DefaultImagesPath, "Inbox");
            public bool AutoEnableRemoteAccess { get; set; } = true;
            public bool AutoPrepareWindowsRuntime { get; set; } = true;
            public bool EnableWinRmBootstrap { get; set; } = true;
            public bool EnableRemoteRegistryBootstrap { get; set; } = true;
            public bool EnforceScopedFirewallRule { get; set; } = true;
            public int ScopedFirewallPort { get; set; }
            public string ScopedFirewallRemoteAddresses { get; set; } = string.Empty;
            public bool ConfigureDefenderExclusions { get; set; } = true;
            public string DefenderExclusionPaths { get; set; } = string.Empty;
            public string HelpdeskAdGroup { get; set; } = "EJLive-Helpdesk";
            public int WindowsBaselineRepairIntervalMin { get; set; } = 30;
            public string WindowsPolicyProfileMode { get; set; } = "Enforce";
            public bool AllowLocalWindowsPasswordChange { get; set; }
            public bool RequireEncryptedWindowsPasswordPayload { get; set; } = true;
            public string AllowedPasswordAccounts { get; set; } = "Administrator,Helpdesk";
            public bool AllowUnsignedLegacyCommands { get; set; }
            public bool EnforceLowPriorityMode { get; set; } = true;
            public bool PinToLastProcessorCore { get; set; }
            public int HeartbeatIntervalSec { get; set; } = AppConstants.HeartbeatIntervalSec;
            public int ReconnectIntervalSec { get; set; } = 20;
    
            public static AppConfig Load()
            {
                try
                {
                    if (!File.Exists(ConfigPath))
                        return new AppConfig();
    
                    var loaded = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigPath)) ?? new AppConfig();
                    loaded.SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(loaded.SupabaseServiceKey);
                    loaded.ApplyDefaults();
                    return loaded;
                }
                catch
                {
                    return new AppConfig();
                }
            }
    
            public void Save()
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
                var serialized = JsonSerializer.Serialize(this);
                var copy = JsonSerializer.Deserialize<AppConfig>(serialized) ?? new AppConfig();
                copy.SupabaseServiceKey = SecurityHelper.ProtectDpapiStringIfNeeded(copy.SupabaseServiceKey);
    
                File.WriteAllText(
                    ConfigPath,
                    JsonSerializer.Serialize(copy, new JsonSerializerOptions { WriteIndented = true }));
            }
    
            public void ApplyDefaults()
            {
                ATM_Type = AppConstants.NormalizeATMType(ATM_Type);
                if (string.IsNullOrWhiteSpace(SourcePath))
                    SourcePath = AppConstants.GetDefaultSourcePath(ATM_Type);
                if (string.IsNullOrWhiteSpace(BackupPath))
                    BackupPath = AppConstants.GetDefaultBackupPath(ATM_Type);
                if (string.IsNullOrWhiteSpace(ImageInboxPath))
                    ImageInboxPath = Path.Combine(AppConstants.DefaultImagesPath, "Inbox");
                if (string.IsNullOrWhiteSpace(DefaultCommandRole))
                    DefaultCommandRole = "Support";
                else
                    DefaultCommandRole = DefaultCommandRole.Trim();
                if (HeartbeatIntervalSec <= 0)
                    HeartbeatIntervalSec = AppConstants.HeartbeatIntervalSec;
                if (ReconnectIntervalSec <= 0)
                    ReconnectIntervalSec = 20;
                WeakNetworkLatencyMs = Math.Clamp(WeakNetworkLatencyMs, 120, 3000);
                if (WindowsBaselineRepairIntervalMin <= 0)
                    WindowsBaselineRepairIntervalMin = 30;
                if (string.IsNullOrWhiteSpace(WindowsPolicyProfileMode))
                    WindowsPolicyProfileMode = "Enforce";
                else
                    WindowsPolicyProfileMode = WindowsPolicyProfileMode.Trim();
                HeartbeatIntervalSec = Math.Clamp(HeartbeatIntervalSec, 5, 300);
                ReconnectIntervalSec = Math.Clamp(ReconnectIntervalSec, 5, 300);
                WindowsBaselineRepairIntervalMin = Math.Clamp(WindowsBaselineRepairIntervalMin, 5, 720);
                SupabaseUrl = (SupabaseUrl ?? string.Empty).Trim();
                SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(SupabaseServiceKey).Trim();
                AllowedPasswordAccounts = (AllowedPasswordAccounts ?? string.Empty).Trim();
                ScopedFirewallRemoteAddresses = string.IsNullOrWhiteSpace(ScopedFirewallRemoteAddresses)
                    ? (ServerIP ?? string.Empty).Trim()
                    : ScopedFirewallRemoteAddresses.Trim();
                DefenderExclusionPaths = (DefenderExclusionPaths ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(HelpdeskAdGroup))
                    HelpdeskAdGroup = "EJLive-Helpdesk";
                else
                    HelpdeskAdGroup = HelpdeskAdGroup.Trim();
                ScopedFirewallPort = Math.Clamp(ScopedFirewallPort, 0, 65535);
                if (ServerPort <= 0)
                    ServerPort = NetworkConfig.DEFAULT_PORT;
            }
        }
    public partial class AuditLogEntry
        {
            public string EntryId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string UserName { get; set; } = string.Empty;
    
    
            public string Action { get; set; } = string.Empty;
    
    
            public string Target { get; set; } = string.Empty;
    
    
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public string Details { get; set; } = string.Empty;
    
    
        }
    /// <summary>
        /// Immutable audit log entry for command and configuration changes.
        /// </summary>
        public sealed class AuditLogEntry
        {
            public string EntryId { get; set; } = Guid.NewGuid().ToString("N");
            public string UserName { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string Target { get; set; } = string.Empty;
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
            public string Details { get; set; } = string.Empty;
        }
    public partial class LiveSyncProgress
        {
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string FileName { get; set; } = string.Empty;
    
    
            public long BytesSent { get; set; }
    
    
            public long TotalBytes { get; set; }
    
    
            public int CurrentChunk { get; set; }
    
    
            public int TotalChunks { get; set; }
    
    
            public double SpeedKBs { get; set; }
    
    
            public SyncStatus Status { get; set; } = SyncStatus.Pending;
    
    
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public int Percent => TotalBytes <= 0 ? 0 : (int)Math.Clamp(BytesSent * 100.0 / TotalBytes, 0, 100);
    
    
        }
    /// <summary>
        /// Lightweight live progress for an in-flight journal sync operation.
        /// </summary>
        public sealed class LiveSyncProgress
        {
            public string ATM_ID { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public long BytesSent { get; set; }
            public long TotalBytes { get; set; }
            public int CurrentChunk { get; set; }
            public int TotalChunks { get; set; }
            public double SpeedKBs { get; set; }
            public SyncStatus Status { get; set; } = SyncStatus.Pending;
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
            public int Percent => TotalBytes <= 0 ? 0 : (int)Math.Clamp(BytesSent * 100.0 / TotalBytes, 0, 100);
        }
    public partial class RemoteCommand
        {
            public string CommandId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string CommandType { get; set; } = string.Empty;
    
    
            public string Payload { get; set; } = string.Empty;
    
    
            public RemoteCommandStatus Status { get; set; } = RemoteCommandStatus.Pending;
    
    
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? SentAtUtc { get; set; }
    
    
            public DateTime? CompletedAtUtc { get; set; }
    
    
            public bool RequiresConfirmation { get; set; }
    
    
            public string Result { get; set; } = string.Empty;
    
    
        }
    /// <summary>
        /// A remotely-dispatched command bound to a specific ATM terminal.
        /// </summary>
        public sealed class RemoteCommand
        {
            public string CommandId { get; set; } = Guid.NewGuid().ToString("N");
            public string ATM_ID { get; set; } = string.Empty;
            public string CommandType { get; set; } = string.Empty;
            public string Payload { get; set; } = string.Empty;
            public RemoteCommandStatus Status { get; set; } = RemoteCommandStatus.Pending;
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
            public DateTime? SentAtUtc { get; set; }
            public DateTime? CompletedAtUtc { get; set; }
            public bool RequiresConfirmation { get; set; }
            public string Result { get; set; } = string.Empty;
        }
    public partial class SyncSummary
        {
            public int Total { get; set; }
    
    
            public int Pending { get; set; }
    
    
            public int InProgress { get; set; }
    
    
            public int Completed { get; set; }
    
    
            public int Failed { get; set; }
    
    
            public int AverageProgress { get; set; }
    
    
            public int OpenItems => Pending + InProgress + Failed;
    
    
        }
    /// <summary>
        /// High-level sync health summary across all terminals.
        /// </summary>
        public sealed class SyncSummary
        {
            public int Total { get; set; }
            public int Pending { get; set; }
            public int InProgress { get; set; }
            public int Completed { get; set; }
            public int Failed { get; set; }
            public int AverageProgress { get; set; }
            public int OpenItems => Pending + InProgress + Failed;
        }
    public partial class TerminalCashStatusCanonical
        {
            public string Source { get; set; } = "Derived";
    
    
            public int Cassette1 { get; set; }
    
    
            public int Cassette2 { get; set; }
    
    
            public int Cassette3 { get; set; }
    
    
            public int Cassette4 { get; set; }
    
    
            public int Remaining { get; set; }
    
    
            public int Loaded { get; set; }
    
    
            public int DepositIn { get; set; }
    
    
            public int DispenseOut { get; set; }
    
    
            public int Reject { get; set; }
    
    
            public int Retract { get; set; }
    
    
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
    
            public int LowCashThreshold { get; set; } = 3000;
    
    
            public int TotalCassetteNotes => Math.Max(0, Cassette1) +
                                                Math.Max(0, Cassette2) +
                                                Math.Max(0, Cassette3) +
                                                Math.Max(0, Cassette4);
    
    
            public bool IsLowCash => Remaining > 0 && Remaining <= LowCashThreshold;
    
    
            public bool IsEmpty => Remaining <= 0;
    
    
        }
    /// <summary>
        /// Canonical cash telemetry snapshot for a single terminal.
        /// </summary>
        public sealed class TerminalCashStatusCanonical
        {
            public string Source { get; set; } = "Derived";
            public int Cassette1 { get; set; }
            public int Cassette2 { get; set; }
            public int Cassette3 { get; set; }
            public int Cassette4 { get; set; }
            public int Remaining { get; set; }
            public int Loaded { get; set; }
            public int DepositIn { get; set; }
            public int DispenseOut { get; set; }
            public int Reject { get; set; }
            public int Retract { get; set; }
            public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
            public int LowCashThreshold { get; set; } = 3000;
    
            public int TotalCassetteNotes => Math.Max(0, Cassette1) +
                                                Math.Max(0, Cassette2) +
                                                Math.Max(0, Cassette3) +
                                                Math.Max(0, Cassette4);
            public bool IsLowCash => Remaining > 0 && Remaining <= LowCashThreshold;
            public bool IsEmpty => Remaining <= 0;
        }
    public partial class TerminalLiveSummaryCanonical
        {
            public string TerminalId { get; set; } = string.Empty;
    
    
            public string BranchName { get; set; } = string.Empty;
    
    
            public string Region { get; set; } = string.Empty;
    
    
            public string Vendor { get; set; } = string.Empty;
    
    
            public string Network { get; set; } = string.Empty;
    
    
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
    
            public ATMStatus Status { get; set; } = ATMStatus.Unknown;
    
    
            public int HealthScore { get; set; }
    
    
            public bool SupervisorMode { get; set; }
    
    
            public int ActiveAlerts { get; set; }
    
    
            public DateTime LastHeartbeatUtc { get; set; }
    
    
            public DateTime LastEjSyncUtc { get; set; }
    
    
            public string LastTransaction { get; set; } = string.Empty;
    
    
            public TerminalCashStatusCanonical Cash { get; set; } = new();
    
    
        }
    /// <summary>
        /// Canonical live summary view for a terminal, combining connection, health, and cash data.
        /// </summary>
        public sealed class TerminalLiveSummaryCanonical
        {
            public string TerminalId { get; set; } = string.Empty;
            public string BranchName { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public string Vendor { get; set; } = string.Empty;
            public string Network { get; set; } = string.Empty;
            public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
            public ATMStatus Status { get; set; } = ATMStatus.Unknown;
            public int HealthScore { get; set; }
            public bool SupervisorMode { get; set; }
            public int ActiveAlerts { get; set; }
            public DateTime LastHeartbeatUtc { get; set; }
            public DateTime LastEjSyncUtc { get; set; }
            public string LastTransaction { get; set; } = string.Empty;
            public TerminalCashStatusCanonical Cash { get; set; } = new();
        }

    // Class: AppConfig (from 3 sources)
        public sealed partial class AppConfig : ClientConfig
        {
            // --- Properties ---
                    private static string ConfigPath => Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "EJLive", "Client", "appconfig.json");
    
                    public bool AutoConnect { get; set; } = true;
    
                    public bool EnableEncryption { get; set; } = true;
    
                    public bool EnableCompression { get; set; } = true;
    
                    public bool EnableTlsTransport { get; set; }
    
                    public bool RequireTlsTransport { get; set; }
    
                    public bool AllowUntrustedTlsCertificate { get; set; }
    
                    public bool EnableAdaptiveChunking { get; set; } = true;
    
                    public int WeakNetworkLatencyMs { get; set; } = 500;
    
                    public bool AutoBackup { get; set; } = true;
    
                    public bool EnforceCommandAuthorization { get; set; } = true;
    
                    public string DefaultCommandRole { get; set; } = "Support";
    
                    public bool EnableSupabaseSync { get; set; }
    
                    public string SupabaseUrl { get; set; } = string.Empty;
    
                    public string SupabaseServiceKey { get; set; } = string.Empty;
    
                    public string ImageInboxPath { get; set; } = Path.Combine(AppConstants.DefaultImagesPath, "Inbox");
    
                    public bool AutoEnableRemoteAccess { get; set; } = true;
    
                    public bool AutoPrepareWindowsRuntime { get; set; } = true;
    
                    public bool EnableWinRmBootstrap { get; set; } = true;
    
                    public bool EnableRemoteRegistryBootstrap { get; set; } = true;
    
                    public bool EnforceScopedFirewallRule { get; set; } = true;
    
                    public int ScopedFirewallPort { get; set; }
    
                    public string ScopedFirewallRemoteAddresses { get; set; } = string.Empty;
    
                    public bool ConfigureDefenderExclusions { get; set; } = true;
    
                    public string DefenderExclusionPaths { get; set; } = string.Empty;
    
                    public string HelpdeskAdGroup { get; set; } = "EJLive-Helpdesk";
    
                    public int WindowsBaselineRepairIntervalMin { get; set; } = 30;
    
                    public string WindowsPolicyProfileMode { get; set; } = "Enforce";
    
                    public bool AllowLocalWindowsPasswordChange { get; set; }
    
                    public bool RequireEncryptedWindowsPasswordPayload { get; set; } = true;
    
                    public string AllowedPasswordAccounts { get; set; } = "Administrator,Helpdesk";
    
                    public bool AllowUnsignedLegacyCommands { get; set; }
    
                    public bool EnforceLowPriorityMode { get; set; } = true;
    
                    public bool PinToLastProcessorCore { get; set; }
    
                    public int HeartbeatIntervalSec { get; set; } = AppConstants.HeartbeatIntervalSec;
    
                    public int ReconnectIntervalSec { get; set; } = 20;
    
    
            // --- Methods ---
                    public static AppConfig Load()
                    {
                        try
                        {
                            if (!File.Exists(ConfigPath))
                                return new AppConfig();
    
                            var loaded = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigPath)) ?? new AppConfig();
                            loaded.SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(loaded.SupabaseServiceKey);
                            loaded.ApplyDefaults();
                            return loaded;
                        }
                        catch
                        {
                            return new AppConfig();
                        }
                    }
    
                    public void Save()
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
                        var serialized = JsonSerializer.Serialize(this);
                        var copy = JsonSerializer.Deserialize<AppConfig>(serialized) ?? new AppConfig();
                        copy.SupabaseServiceKey = SecurityHelper.ProtectDpapiStringIfNeeded(copy.SupabaseServiceKey);
    
                        File.WriteAllText(
                            ConfigPath,
                            JsonSerializer.Serialize(copy, new JsonSerializerOptions { WriteIndented = true }));
                    }
    
                    public void ApplyDefaults()
                    {
                        ATM_Type = AppConstants.NormalizeATMType(ATM_Type);
                        if (string.IsNullOrWhiteSpace(SourcePath))
                            SourcePath = AppConstants.GetDefaultSourcePath(ATM_Type);
                        if (string.IsNullOrWhiteSpace(BackupPath))
                            BackupPath = AppConstants.GetDefaultBackupPath(ATM_Type);
                        if (string.IsNullOrWhiteSpace(ImageInboxPath))
                            ImageInboxPath = Path.Combine(AppConstants.DefaultImagesPath, "Inbox");
                        if (string.IsNullOrWhiteSpace(DefaultCommandRole))
                            DefaultCommandRole = "Support";
                        else
                            DefaultCommandRole = DefaultCommandRole.Trim();
                        if (HeartbeatIntervalSec <= 0)
                            HeartbeatIntervalSec = AppConstants.HeartbeatIntervalSec;
                        if (ReconnectIntervalSec <= 0)
                            ReconnectIntervalSec = 20;
                        WeakNetworkLatencyMs = Math.Clamp(WeakNetworkLatencyMs, 120, 3000);
                        if (WindowsBaselineRepairIntervalMin <= 0)
                            WindowsBaselineRepairIntervalMin = 30;
                        if (string.IsNullOrWhiteSpace(WindowsPolicyProfileMode))
                            WindowsPolicyProfileMode = "Enforce";
                        else
                            WindowsPolicyProfileMode = WindowsPolicyProfileMode.Trim();
                        HeartbeatIntervalSec = Math.Clamp(HeartbeatIntervalSec, 5, 300);
                        ReconnectIntervalSec = Math.Clamp(ReconnectIntervalSec, 5, 300);
                        WindowsBaselineRepairIntervalMin = Math.Clamp(WindowsBaselineRepairIntervalMin, 5, 720);
                        SupabaseUrl = (SupabaseUrl ?? string.Empty).Trim();
                        SupabaseServiceKey = SecurityHelper.TryUnprotectDpapiString(SupabaseServiceKey).Trim();
                        AllowedPasswordAccounts = (AllowedPasswordAccounts ?? string.Empty).Trim();
                        ScopedFirewallRemoteAddresses = string.IsNullOrWhiteSpace(ScopedFirewallRemoteAddresses)
                            ? (ServerIP ?? string.Empty).Trim()
                            : ScopedFirewallRemoteAddresses.Trim();
                        DefenderExclusionPaths = (DefenderExclusionPaths ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(HelpdeskAdGroup))
                            HelpdeskAdGroup = "EJLive-Helpdesk";
                        else
                            HelpdeskAdGroup = HelpdeskAdGroup.Trim();
                        ScopedFirewallPort = Math.Clamp(ScopedFirewallPort, 0, 65535);
                        if (ServerPort <= 0)
                            ServerPort = NetworkConfig.DEFAULT_PORT;
                    }
    
    
        }
    // Class: AuditLogEntry (from 3 sources)
        public sealed partial class AuditLogEntry
        {
            // --- Properties ---
                    public string EntryId { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string UserName { get; set; } = string.Empty;
    
                    public string Action { get; set; } = string.Empty;
    
                    public string Target { get; set; } = string.Empty;
    
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
                    public string Details { get; set; } = string.Empty;
    
    
        }
    // Class: LiveSyncProgress (from 3 sources)
        public sealed partial class LiveSyncProgress
        {
            // --- Properties ---
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string FileName { get; set; } = string.Empty;
    
                    public long BytesSent { get; set; }
    
                    public long TotalBytes { get; set; }
    
                    public int CurrentChunk { get; set; }
    
                    public int TotalChunks { get; set; }
    
                    public double SpeedKBs { get; set; }
    
                    public SyncStatus Status { get; set; } = SyncStatus.Pending;
    
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
                    public int Percent => TotalBytes <= 0 ? 0 : (int)Math.Clamp(BytesSent * 100.0 / TotalBytes, 0, 100);
    
    
        }
    // Class: RemoteCommand (from 1 sources)
        public sealed partial class RemoteCommand
        {
            // --- Properties ---
                    public string CommandId { get; set; } = Guid.NewGuid().ToString("N");
    
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string CommandType { get; set; } = string.Empty;
    
                    public string Payload { get; set; } = string.Empty;
    
                    public RemoteCommandStatus Status { get; set; } = RemoteCommandStatus.Pending;
    
                    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime? SentAtUtc { get; set; }
    
                    public DateTime? CompletedAtUtc { get; set; }
    
                    public bool RequiresConfirmation { get; set; }
    
                    public string Result { get; set; } = string.Empty;
    
    
        }
    // Enum: RemoteCommandStatus (from 3 sources)
        public partial enum RemoteCommandStatus
        {
            // --- Constants & Fields ---
                    Pending,
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\CanonicalModels.cs
                    Cancelled
    
    
        }
    // Class: SyncSummary (from 3 sources)
        public sealed partial class SyncSummary
        {
            // --- Properties ---
                    public int Total { get; set; }
    
                    public int Pending { get; set; }
    
                    public int InProgress { get; set; }
    
                    public int Completed { get; set; }
    
                    public int Failed { get; set; }
    
                    public int AverageProgress { get; set; }
    
                    public int OpenItems => Pending + InProgress + Failed;
    
    
        }
    // Class: TerminalCashStatusCanonical (from 3 sources)
        public sealed partial class TerminalCashStatusCanonical
        {
            // --- Properties ---
                    public string Source { get; set; } = "Derived";
    
                    public int Cassette1 { get; set; }
    
                    public int Cassette2 { get; set; }
    
                    public int Cassette3 { get; set; }
    
                    public int Cassette4 { get; set; }
    
                    public int Remaining { get; set; }
    
                    public int Loaded { get; set; }
    
                    public int DepositIn { get; set; }
    
                    public int DispenseOut { get; set; }
    
                    public int Reject { get; set; }
    
                    public int Retract { get; set; }
    
                    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    
                    public int LowCashThreshold { get; set; } = 3000;
    
                    public int TotalCassetteNotes => Math.Max(0, Cassette1) +
                                                        Math.Max(0, Cassette2) +
                                                        Math.Max(0, Cassette3) +
                                                        Math.Max(0, Cassette4);
    
                    public bool IsLowCash => Remaining > 0 && Remaining <= LowCashThreshold;
    
                    public bool IsEmpty => Remaining <= 0;
    
    
        }
    // Class: TerminalLiveSummaryCanonical (from 3 sources)
        public sealed partial class TerminalLiveSummaryCanonical
        {
            // --- Properties ---
                    public string TerminalId { get; set; } = string.Empty;
    
                    public string BranchName { get; set; } = string.Empty;
    
                    public string Region { get; set; } = string.Empty;
    
                    public string Vendor { get; set; } = string.Empty;
    
                    public string Network { get; set; } = string.Empty;
    
                    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Disconnected;
    
                    public ATMStatus Status { get; set; } = ATMStatus.Unknown;
    
                    public int HealthScore { get; set; }
    
                    public bool SupervisorMode { get; set; }
    
                    public int ActiveAlerts { get; set; }
    
                    public DateTime LastHeartbeatUtc { get; set; }
    
                    public DateTime LastEjSyncUtc { get; set; }
    
                    public string LastTransaction { get; set; } = string.Empty;
    
                    public TerminalCashStatusCanonical Cash { get; set; } = new();
    
    
        }

    public partial enum RemoteCommandStatus
        {
            Pending,
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Core\Models\CanonicalModels.cs
            Cancelled
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Core\Models\CanonicalModels.cs
            Cancelled
    
    
        }
    /// <summary>
        /// Remote command execution status states.
        /// </summary>
        public enum RemoteCommandStatus
        {
            Pending,
            Sent,
            Running,
            Completed,
            Failed,
            Cancelled
        }
    public partial enum RemoteCommandStatus
        {
            Pending,
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Core\Models\CanonicalModels.cs
            Cancelled
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\CanonicalModels.cs
            Cancelled
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Core\Models\CanonicalModels.cs
            Cancelled
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Core\Models\CanonicalModels.cs
            Cancelled
    
    
        }
}
