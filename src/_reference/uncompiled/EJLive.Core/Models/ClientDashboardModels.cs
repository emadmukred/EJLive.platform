using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public partial class AgentConfigViewModel
        {
            public List<ConfigEntry> Entries { get; set; } = new();
    
    
        }
    // ===== Agent Config Tab =====
        public sealed class AgentConfigViewModel
        {
            public List<ConfigEntry> Entries { get; set; } = new();
        }
    public partial class CommandRequestEntry
        {
            public string RequestId { get; set; } = string.Empty;
    
    
            public string Command { get; set; } = string.Empty;
    
    
            public string Status { get; set; } = "Queued";
    
    
            public DateTime QueuedAt { get; set; }
    
    
            public DateTime? SentAt { get; set; }
    
    
            public DateTime? CompletedAt { get; set; }
    
    
            public string Result { get; set; } = string.Empty;
    
    
            public string? FailureReason { get; set; }
    
    
        }
    public sealed class CommandRequestEntry
        {
            public string RequestId { get; set; } = string.Empty;
            public string Command { get; set; } = string.Empty;
            public string Status { get; set; } = "Queued";
            public DateTime QueuedAt { get; set; }
            public DateTime? SentAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public string Result { get; set; } = string.Empty;
            public string? FailureReason { get; set; }
        }
    public partial class ConfigEntry
        {
            public string Key { get; set; } = string.Empty;
    
    
            public string Value { get; set; } = string.Empty;
    
    
            public string OperationalUsage { get; set; } = string.Empty;
    
    
            public string Source { get; set; } = string.Empty;
    
    
            public bool IsSensitive { get; set; }
    
    
            public string Validation { get; set; } = string.Empty;
    
    
        }
    public sealed class ConfigEntry
        {
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string OperationalUsage { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public bool IsSensitive { get; set; }
            public string Validation { get; set; } = string.Empty;
        }
    public partial class ConnectionViewModel
        {
            public string ServerHost { get; set; } = string.Empty;
    
    
            public int ServerPort { get; set; } = 8080;
    
    
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string AgentId { get; set; } = string.Empty;
    
    
            public string TerminalId { get; set; } = string.Empty;
    
    
            public string ATM_Name { get; set; } = string.Empty;
    
    
            public string Branch { get; set; } = string.Empty;
    
    
            public string Region { get; set; } = string.Empty;
    
    
            public string Vendor { get; set; } = string.Empty;
    
    
            public string Model { get; set; } = string.Empty;
    
    
            public string NetworkType { get; set; } = "LAN";
    
    
            public string SourceJournalPath { get; set; } = string.Empty;
    
    
            public string BackupPath { get; set; } = string.Empty;
    
    
            public string ConnectionState { get; set; } = "Disconnected";
    
    
            public string SessionId { get; set; } = string.Empty;
    
    
            public long BytesSent { get; set; }
    
    
            public long BytesReceived { get; set; }
    
    
            public double HealthPercent { get; set; }
    
    
            public DateTime? LastDataTime { get; set; }
    
    
            public DateTime? LastHandshake { get; set; }
    
    
            public DateTime? LastHeartbeat { get; set; }
    
    
            public string ReconnectState { get; set; } = string.Empty;
    
    
            public List<string> ConnectionLog { get; set; } = new();
    
    
            public Dictionary<string, string> HealthMatrix { get; set; } = new();
    
    
        }
    /// <summary>
        /// Client companion UI tab models.
        /// Structured data contracts for the 13 client tabs defined in section 9
        /// of the project master document. Keeps UI decoupled from runtime logic.
        /// </summary>
    
        // ===== Connection Tab =====
        public sealed class ConnectionViewModel
        {
            public string ServerHost { get; set; } = string.Empty;
            public int ServerPort { get; set; } = 8080;
            public string ATM_ID { get; set; } = string.Empty;
            public string AgentId { get; set; } = string.Empty;
            public string TerminalId { get; set; } = string.Empty;
            public string ATM_Name { get; set; } = string.Empty;
            public string Branch { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public string Vendor { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string NetworkType { get; set; } = "LAN";
            public string SourceJournalPath { get; set; } = string.Empty;
            public string BackupPath { get; set; } = string.Empty;
            public string ConnectionState { get; set; } = "Disconnected";
            public string SessionId { get; set; } = string.Empty;
            public long BytesSent { get; set; }
            public long BytesReceived { get; set; }
            public double HealthPercent { get; set; }
            public DateTime? LastDataTime { get; set; }
            public DateTime? LastHandshake { get; set; }
            public DateTime? LastHeartbeat { get; set; }
            public string ReconnectState { get; set; } = string.Empty;
            public List<string> ConnectionLog { get; set; } = new();
            public Dictionary<string, string> HealthMatrix { get; set; } = new();
        }
    public partial class ControlledRequestsViewModel
        {
            public string SelectedRequestType { get; set; } = string.Empty;
    
    
            public Dictionary<string, string> RequestParameters { get; set; } = new();
    
    
            public List<CommandRequestEntry> CommandQueue { get; set; } = new();
    
    
            public byte[]? ScreenPreview { get; set; }
    
    
            public string PolicyStatus { get; set; } = "Unknown";
    
    
            public string LastResult { get; set; } = string.Empty;
    
    
        }
    // ===== Controlled Requests Tab =====
        public sealed class ControlledRequestsViewModel
        {
            public string SelectedRequestType { get; set; } = string.Empty;
            public Dictionary<string, string> RequestParameters { get; set; } = new();
            public List<CommandRequestEntry> CommandQueue { get; set; } = new();
            public byte[]? ScreenPreview { get; set; }
            public string PolicyStatus { get; set; } = "Unknown";
            public string LastResult { get; set; } = string.Empty;
        }
    public partial class DiagnosticsViewModel
        {
            public bool ServiceExists { get; set; }
    
    
            public string ServiceAccount { get; set; } = string.Empty;
    
    
            public bool StartupRegistered { get; set; }
    
    
            public bool JournalSourceValid { get; set; }
    
    
            public bool BackupPathValid { get; set; }
    
    
            public bool ImageInboxValid { get; set; }
    
    
            public bool ImageDestinationValid { get; set; }
    
    
            public bool ServerReachable { get; set; }
    
    
            public bool PortConnectivity { get; set; }
    
    
            public bool HandshakeReady { get; set; }
    
    
            public bool OutboxReady { get; set; }
    
    
            public long DiskSpaceMb { get; set; }
    
    
            public double HealthScore { get; set; }
    
    
            public List<string> RecommendedActions { get; set; } = new();
    
    
            public List<string> DiagnosticLog { get; set; } = new();
    
    
        }
    // ===== Diagnostics Tab =====
        public sealed class DiagnosticsViewModel
        {
            public bool ServiceExists { get; set; }
            public string ServiceAccount { get; set; } = string.Empty;
            public bool StartupRegistered { get; set; }
            public bool JournalSourceValid { get; set; }
            public bool BackupPathValid { get; set; }
            public bool ImageInboxValid { get; set; }
            public bool ImageDestinationValid { get; set; }
            public bool ServerReachable { get; set; }
            public bool PortConnectivity { get; set; }
            public bool HandshakeReady { get; set; }
            public bool OutboxReady { get; set; }
            public long DiskSpaceMb { get; set; }
            public double HealthScore { get; set; }
            public List<string> RecommendedActions { get; set; } = new();
            public List<string> DiagnosticLog { get; set; } = new();
        }
    public partial class ImagePackageEntry
        {
            public string PackageName { get; set; } = string.Empty;
    
    
            public string ServerSource { get; set; } = string.Empty;
    
    
            public string ClientInboxPath { get; set; } = string.Empty;
    
    
            public string AtmDestinationPath { get; set; } = string.Empty;
    
    
            public long FileSize { get; set; }
    
    
            public string Checksum { get; set; } = string.Empty;
    
    
            public DateTime? ReceivedTime { get; set; }
    
    
            public DateTime? PromotedTime { get; set; }
    
    
            public string Status { get; set; } = "Pending";
    
    
            public string? FailureReason { get; set; }
    
    
            public bool ReceiptSent { get; set; }
    
    
        }
    public sealed class ImagePackageEntry
        {
            public string PackageName { get; set; } = string.Empty;
            public string ServerSource { get; set; } = string.Empty;
            public string ClientInboxPath { get; set; } = string.Empty;
            public string AtmDestinationPath { get; set; } = string.Empty;
            public long FileSize { get; set; }
            public string Checksum { get; set; } = string.Empty;
            public DateTime? ReceivedTime { get; set; }
            public DateTime? PromotedTime { get; set; }
            public string Status { get; set; } = "Pending";
            public string? FailureReason { get; set; }
            public bool ReceiptSent { get; set; }
        }
    public partial class ImageSyncViewModel
        {
            public string Vendor { get; set; } = string.Empty;
    
    
            public string AtmType { get; set; } = string.Empty;
    
    
            public string Model { get; set; } = string.Empty;
    
    
            public List<ImagePackageEntry> Packages { get; set; } = new();
    
    
        }
    // ===== Image Sync / Content Delivery Tab =====
        public sealed class ImageSyncViewModel
        {
            public string Vendor { get; set; } = string.Empty;
            public string AtmType { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public List<ImagePackageEntry> Packages { get; set; } = new();
        }
    public partial class InstallerViewModel
        {
            public string ServiceName { get; set; } = "EJLive Client Agent Service";
    
    
            public string ServiceAccount { get; set; } = "LocalSystem";
    
    
            public string InstallPath { get; set; } = string.Empty;
    
    
            public string ConfigPath { get; set; } = string.Empty;
    
    
            public string HealthFilePath { get; set; } = string.Empty;
    
    
            public string CurrentVersion { get; set; } = string.Empty;
    
    
            public string RollbackVersion { get; set; } = string.Empty;
    
    
            public bool PrerequisitesMet { get; set; }
    
    
            public string DotNetRuntimeStatus { get; set; } = "Unknown";
    
    
            public bool ServiceInstalled { get; set; }
    
    
            public bool ServiceRunning { get; set; }
    
    
            public bool StartupEnabled { get; set; }
    
    
            public string InstallationLog { get; set; } = string.Empty;
    
    
            public string RollbackStatus { get; set; } = string.Empty;
    
    
        }
    // ===== Installer/Activation Tab =====
        public sealed class InstallerViewModel
        {
            public string ServiceName { get; set; } = "EJLive Client Agent Service";
            public string ServiceAccount { get; set; } = "LocalSystem";
            public string InstallPath { get; set; } = string.Empty;
            public string ConfigPath { get; set; } = string.Empty;
            public string HealthFilePath { get; set; } = string.Empty;
            public string CurrentVersion { get; set; } = string.Empty;
            public string RollbackVersion { get; set; } = string.Empty;
            public bool PrerequisitesMet { get; set; }
            public string DotNetRuntimeStatus { get; set; } = "Unknown";
            public bool ServiceInstalled { get; set; }
            public bool ServiceRunning { get; set; }
            public bool StartupEnabled { get; set; }
            public string InstallationLog { get; set; } = string.Empty;
            public string RollbackStatus { get; set; } = string.Empty;
        }
    public partial class JournalPreviewViewModel
        {
            public DateTime DateFrom { get; set; } = DateTime.UtcNow.AddDays(-1);
    
    
            public DateTime DateTo { get; set; } = DateTime.UtcNow;
    
    
            public string StatusFilter { get; set; } = "All";
    
    
            public string TypeFilter { get; set; } = "All";
    
    
            public string SearchText { get; set; } = string.Empty;
    
    
            public int TransactionCount { get; set; }
    
    
            public Dictionary<string, int> StatusCounts { get; set; } = new();
    
    
            public List<string> LastLinesPreview { get; set; } = new();
    
    
            public long FileSize { get; set; }
    
    
            public DateTime? LastWriteTime { get; set; }
    
    
            public string ParserAvailability { get; set; } = "Unknown";
    
    
        }
    public partial class LocalHealthViewModel
        {
            public bool Connected { get; set; }
    
    
            public string SessionId { get; set; } = string.Empty;
    
    
            public string HandshakeState { get; set; } = "None";
    
    
            public string HeartbeatState { get; set; } = "None";
    
    
            public DateTime? LastHeartbeat { get; set; }
    
    
            public DateTime? LastHandshake { get; set; }
    
    
            public DateTime? LastSync { get; set; }
    
    
            public int OutboxCount { get; set; }
    
    
            public int FailedCount { get; set; }
    
    
            public string WatcherStates { get; set; } = "inactive";
    
    
            public string SocketState { get; set; } = "Disconnected";
    
    
            public string ServiceState { get; set; } = "Unknown";
    
    
            public double HealthScore { get; set; }
    
    
            public string LastError { get; set; } = string.Empty;
    
    
            public List<string> Recommendations { get; set; } = new();
    
    
            public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== Local Health / Service Snapshot Tab =====
        public sealed class LocalHealthViewModel
        {
            public bool Connected { get; set; }
            public string SessionId { get; set; } = string.Empty;
            public string HandshakeState { get; set; } = "None";
            public string HeartbeatState { get; set; } = "None";
            public DateTime? LastHeartbeat { get; set; }
            public DateTime? LastHandshake { get; set; }
            public DateTime? LastSync { get; set; }
            public int OutboxCount { get; set; }
            public int FailedCount { get; set; }
            public string WatcherStates { get; set; } = "inactive";
            public string SocketState { get; set; } = "Disconnected";
            public string ServiceState { get; set; } = "Unknown";
            public double HealthScore { get; set; }
            public string LastError { get; set; } = string.Empty;
            public List<string> Recommendations { get; set; } = new();
            public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class LogsViewModel
        {
            public List<string> LogEntries { get; set; } = new();
    
    
            public string SeverityFilter { get; set; } = "All";
    
    
            public string ComponentFilter { get; set; } = "All";
    
    
            public string CorrelationIdFilter { get; set; } = string.Empty;
    
    
            public string SearchText { get; set; } = string.Empty;
    
    
            public bool AutoScroll { get; set; } = true;
    
    
            public bool RedactionEnabled { get; set; } = true;
    
    
            public int TotalEntries { get; set; }
    
    
            public int FilteredEntries { get; set; }
    
    
        }
    // ===== Logs Tab =====
        public sealed class LogsViewModel
        {
            public List<string> LogEntries { get; set; } = new();
            public string SeverityFilter { get; set; } = "All";
            public string ComponentFilter { get; set; } = "All";
            public string CorrelationIdFilter { get; set; } = string.Empty;
            public string SearchText { get; set; } = string.Empty;
            public bool AutoScroll { get; set; } = true;
            public bool RedactionEnabled { get; set; } = true;
            public int TotalEntries { get; set; }
            public int FilteredEntries { get; set; }
        }
    public partial class ServiceComponentEntry
        {
            public string Component { get; set; } = string.Empty;
    
    
            public string Status { get; set; } = "Unknown";
    
    
            public DateTime? LastTransition { get; set; }
    
    
            public string LastError { get; set; } = string.Empty;
    
    
            public string Details { get; set; } = string.Empty;
    
    
            public string RecommendedAction { get; set; } = string.Empty;
    
    
        }
    public sealed class ServiceComponentEntry
        {
            public string Component { get; set; } = string.Empty;
            public string Status { get; set; } = "Unknown";
            public DateTime? LastTransition { get; set; }
            public string LastError { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
            public string RecommendedAction { get; set; } = string.Empty;
        }
    public partial class ServicesControlViewModel
        {
            public List<ServiceComponentEntry> Components { get; set; } = new();
    
    
            public bool WindowsStartup { get; set; }
    
    
            public bool ServiceInstalled { get; set; }
    
    
        }
    // ===== Services/Control Tab =====
        public sealed class ServicesControlViewModel
        {
            public List<ServiceComponentEntry> Components { get; set; } = new();
            public bool WindowsStartup { get; set; }
            public bool ServiceInstalled { get; set; }
        }
    public partial class SettingsViewModel
        {
            public string ServerHost { get; set; } = "127.0.0.1";
    
    
            public int ServerPort { get; set; } = 8080;
    
    
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string ATM_Name { get; set; } = string.Empty;
    
    
            public string Vendor { get; set; } = string.Empty;
    
    
            public string Model { get; set; } = string.Empty;
    
    
            public string JournalPath { get; set; } = string.Empty;
    
    
            public string BackupPath { get; set; } = string.Empty;
    
    
            public string ImageInboxPath { get; set; } = string.Empty;
    
    
            public string ImageDestinationPath { get; set; } = string.Empty;
    
    
            public bool AutoConnect { get; set; } = true;
    
    
            public bool AutoBackup { get; set; } = true;
    
    
            public bool DurableSync { get; set; } = true;
    
    
            public bool AckRequired { get; set; } = true;
    
    
            public bool DedupEnabled { get; set; } = true;
    
    
            public bool EncryptionEnabled { get; set; }
    
    
            public bool CompressionEnabled { get; set; }
    
    
            public int HeartbeatIntervalSec { get; set; } = 10;
    
    
            public int ReconnectIntervalSec { get; set; } = 15;
    
    
            public bool IsDirty { get; set; }
    
    
        }
    // ===== Settings Tab =====
        public sealed class SettingsViewModel
        {
            public string ServerHost { get; set; } = "127.0.0.1";
            public int ServerPort { get; set; } = 8080;
            public string ATM_ID { get; set; } = string.Empty;
            public string ATM_Name { get; set; } = string.Empty;
            public string Vendor { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string JournalPath { get; set; } = string.Empty;
            public string BackupPath { get; set; } = string.Empty;
            public string ImageInboxPath { get; set; } = string.Empty;
            public string ImageDestinationPath { get; set; } = string.Empty;
            public bool AutoConnect { get; set; } = true;
            public bool AutoBackup { get; set; } = true;
            public bool DurableSync { get; set; } = true;
            public bool AckRequired { get; set; } = true;
            public bool DedupEnabled { get; set; } = true;
            public bool EncryptionEnabled { get; set; }
            public bool CompressionEnabled { get; set; }
            public int HeartbeatIntervalSec { get; set; } = 10;
            public int ReconnectIntervalSec { get; set; } = 15;
            public bool IsDirty { get; set; }
        }
    public partial class SyncFileEntry
        {
            public string ItemId { get; set; } = string.Empty;
    
    
            public string FileName { get; set; } = string.Empty;
    
    
            public long Size { get; set; }
    
    
            public string Status { get; set; } = "Pending";
    
    
            public double Progress { get; set; }
    
    
            public int Retries { get; set; }
    
    
            public string Checksum { get; set; } = string.Empty;
    
    
            public long BytesSent { get; set; }
    
    
            public DateTime AddedTime { get; set; }
    
    
            public DateTime? LastAttempt { get; set; }
    
    
            public string? FailureReason { get; set; }
    
    
        }
    public sealed class SyncFileEntry
        {
            public string ItemId { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public long Size { get; set; }
            public string Status { get; set; } = "Pending";
            public double Progress { get; set; }
            public int Retries { get; set; }
            public string Checksum { get; set; } = string.Empty;
            public long BytesSent { get; set; }
            public DateTime AddedTime { get; set; }
            public DateTime? LastAttempt { get; set; }
            public string? FailureReason { get; set; }
        }
    public partial class SyncViewModel
        {
            public int PendingCount { get; set; }
    
    
            public int SyncingCount { get; set; }
    
    
            public int CompletedCount { get; set; }
    
    
            public int FailedCount { get; set; }
    
    
            public long TotalSize { get; set; }
    
    
            public double SuccessRate { get; set; }
    
    
            public double Progress { get; set; }
    
    
            public List<SyncFileEntry> Files { get; set; } = new();
    
    
        }
    // ===== Sync Tab =====
        public sealed class SyncViewModel
        {
            public int PendingCount { get; set; }
            public int SyncingCount { get; set; }
            public int CompletedCount { get; set; }
            public int FailedCount { get; set; }
            public long TotalSize { get; set; }
            public double SuccessRate { get; set; }
            public double Progress { get; set; }
            public List<SyncFileEntry> Files { get; set; } = new();
        }
    public partial class VendorPathEntry
        {
            public string JournalSource { get; set; } = string.Empty;
    
    
            public string Backup { get; set; } = string.Empty;
    
    
            public string TraceLog { get; set; } = string.Empty;
    
    
            public string ImageInbox { get; set; } = string.Empty;
    
    
            public string ImageDestination { get; set; } = string.Empty;
    
    
            public string ScreenshotCache { get; set; } = string.Empty;
    
    
            public string Extensions { get; set; } = "*.LOG,*.TXT";
    
    
            public string RolloverBehavior { get; set; } = "Daily";
    
    
            public bool ValidationPassed { get; set; }
    
    
            public bool WritePermission { get; set; }
    
    
            public string RestartNotes { get; set; } = string.Empty;
    
    
            public bool IsDirty { get; set; }
    
    
        }
    public sealed class VendorPathEntry
        {
            public string JournalSource { get; set; } = string.Empty;
            public string Backup { get; set; } = string.Empty;
            public string TraceLog { get; set; } = string.Empty;
            public string ImageInbox { get; set; } = string.Empty;
            public string ImageDestination { get; set; } = string.Empty;
            public string ScreenshotCache { get; set; } = string.Empty;
            public string Extensions { get; set; } = "*.LOG,*.TXT";
            public string RolloverBehavior { get; set; } = "Daily";
            public bool ValidationPassed { get; set; }
            public bool WritePermission { get; set; }
            public string RestartNotes { get; set; } = string.Empty;
            public bool IsDirty { get; set; }
        }
    public partial class VendorPathMappingViewModel
        {
            public string SelectedVendor { get; set; } = "NCR";
    
    
            public Dictionary<string, VendorPathEntry> VendorPaths { get; set; } = new();
    
    
        }
    // ===== Vendor Paths / Mapping Tab =====
        public sealed class VendorPathMappingViewModel
        {
            public string SelectedVendor { get; set; } = "NCR";
            public Dictionary<string, VendorPathEntry> VendorPaths { get; set; } = new();
        }

    // Class: AgentConfigViewModel (from 3 sources)
        public sealed partial class AgentConfigViewModel
        {
            // --- Properties ---
                    public List<ConfigEntry> Entries { get; set; } = new();
    
    
        }
    // Class: CommandRequestEntry (from 3 sources)
        public sealed partial class CommandRequestEntry
        {
            // --- Properties ---
                    public string RequestId { get; set; } = string.Empty;
    
                    public string Command { get; set; } = string.Empty;
    
                    public string Status { get; set; } = "Queued";
    
                    public DateTime QueuedAt { get; set; }
    
                    public DateTime? SentAt { get; set; }
    
                    public DateTime? CompletedAt { get; set; }
    
                    public string Result { get; set; } = string.Empty;
    
                    public string? FailureReason { get; set; }
    
    
        }
    // Class: ConfigEntry (from 3 sources)
        public sealed partial class ConfigEntry
        {
            // --- Properties ---
                    public string Key { get; set; } = string.Empty;
    
                    public string Value { get; set; } = string.Empty;
    
                    public string OperationalUsage { get; set; } = string.Empty;
    
                    public string Source { get; set; } = string.Empty;
    
                    public bool IsSensitive { get; set; }
    
                    public string Validation { get; set; } = string.Empty;
    
    
        }
    // Class: ConnectionViewModel (from 3 sources)
        public sealed partial class ConnectionViewModel
        {
            // --- Properties ---
                    public string ServerHost { get; set; } = string.Empty;
    
                    public int ServerPort { get; set; } = 8080;
    
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string AgentId { get; set; } = string.Empty;
    
                    public string TerminalId { get; set; } = string.Empty;
    
                    public string ATM_Name { get; set; } = string.Empty;
    
                    public string Branch { get; set; } = string.Empty;
    
                    public string Region { get; set; } = string.Empty;
    
                    public string Vendor { get; set; } = string.Empty;
    
                    public string Model { get; set; } = string.Empty;
    
                    public string NetworkType { get; set; } = "LAN";
    
                    public string SourceJournalPath { get; set; } = string.Empty;
    
                    public string BackupPath { get; set; } = string.Empty;
    
                    public string ConnectionState { get; set; } = "Disconnected";
    
                    public string SessionId { get; set; } = string.Empty;
    
                    public long BytesSent { get; set; }
    
                    public long BytesReceived { get; set; }
    
                    public double HealthPercent { get; set; }
    
                    public DateTime? LastDataTime { get; set; }
    
                    public DateTime? LastHandshake { get; set; }
    
                    public DateTime? LastHeartbeat { get; set; }
    
                    public string ReconnectState { get; set; } = string.Empty;
    
                    public List<string> ConnectionLog { get; set; } = new();
    
                    public Dictionary<string, string> HealthMatrix { get; set; } = new();
    
    
        }
    // Class: ControlledRequestsViewModel (from 3 sources)
        public sealed partial class ControlledRequestsViewModel
        {
            // --- Properties ---
                    public string SelectedRequestType { get; set; } = string.Empty;
    
                    public Dictionary<string, string> RequestParameters { get; set; } = new();
    
                    public List<CommandRequestEntry> CommandQueue { get; set; } = new();
    
                    public byte[]? ScreenPreview { get; set; }
    
                    public string PolicyStatus { get; set; } = "Unknown";
    
                    public string LastResult { get; set; } = string.Empty;
    
    
        }
    // Class: DiagnosticsViewModel (from 3 sources)
        public sealed partial class DiagnosticsViewModel
        {
            // --- Properties ---
                    public bool ServiceExists { get; set; }
    
                    public string ServiceAccount { get; set; } = string.Empty;
    
                    public bool StartupRegistered { get; set; }
    
                    public bool JournalSourceValid { get; set; }
    
                    public bool BackupPathValid { get; set; }
    
                    public bool ImageInboxValid { get; set; }
    
                    public bool ImageDestinationValid { get; set; }
    
                    public bool ServerReachable { get; set; }
    
                    public bool PortConnectivity { get; set; }
    
                    public bool HandshakeReady { get; set; }
    
                    public bool OutboxReady { get; set; }
    
                    public long DiskSpaceMb { get; set; }
    
                    public double HealthScore { get; set; }
    
                    public List<string> RecommendedActions { get; set; } = new();
    
                    public List<string> DiagnosticLog { get; set; } = new();
    
    
        }
    // Class: ImagePackageEntry (from 3 sources)
        public sealed partial class ImagePackageEntry
        {
            // --- Properties ---
                    public string PackageName { get; set; } = string.Empty;
    
                    public string ServerSource { get; set; } = string.Empty;
    
                    public string ClientInboxPath { get; set; } = string.Empty;
    
                    public string AtmDestinationPath { get; set; } = string.Empty;
    
                    public long FileSize { get; set; }
    
                    public string Checksum { get; set; } = string.Empty;
    
                    public DateTime? ReceivedTime { get; set; }
    
                    public DateTime? PromotedTime { get; set; }
    
                    public string Status { get; set; } = "Pending";
    
                    public string? FailureReason { get; set; }
    
                    public bool ReceiptSent { get; set; }
    
    
        }
    // Class: ImageSyncViewModel (from 3 sources)
        public sealed partial class ImageSyncViewModel
        {
            // --- Properties ---
                    public string Vendor { get; set; } = string.Empty;
    
                    public string AtmType { get; set; } = string.Empty;
    
                    public string Model { get; set; } = string.Empty;
    
                    public List<ImagePackageEntry> Packages { get; set; } = new();
    
    
        }
    // Class: InstallerViewModel (from 3 sources)
        public sealed partial class InstallerViewModel
        {
            // --- Properties ---
                    public string ServiceName { get; set; } = "EJLive Client Agent Service";
    
                    public string ServiceAccount { get; set; } = "LocalSystem";
    
                    public string InstallPath { get; set; } = string.Empty;
    
                    public string ConfigPath { get; set; } = string.Empty;
    
                    public string HealthFilePath { get; set; } = string.Empty;
    
                    public string CurrentVersion { get; set; } = string.Empty;
    
                    public string RollbackVersion { get; set; } = string.Empty;
    
                    public bool PrerequisitesMet { get; set; }
    
                    public string DotNetRuntimeStatus { get; set; } = "Unknown";
    
                    public bool ServiceInstalled { get; set; }
    
                    public bool ServiceRunning { get; set; }
    
                    public bool StartupEnabled { get; set; }
    
                    public string InstallationLog { get; set; } = string.Empty;
    
                    public string RollbackStatus { get; set; } = string.Empty;
    
    
        }
    // Class: JournalPreviewViewModel (from 3 sources)
        public sealed partial class JournalPreviewViewModel
        {
            // --- Properties ---
                    public DateTime DateFrom { get; set; } = DateTime.UtcNow.AddDays(-1);
    
                    public DateTime DateTo { get; set; } = DateTime.UtcNow;
    
                    public string StatusFilter { get; set; } = "All";
    
                    public string TypeFilter { get; set; } = "All";
    
                    public string SearchText { get; set; } = string.Empty;
    
                    public int TransactionCount { get; set; }
    
                    public Dictionary<string, int> StatusCounts { get; set; } = new();
    
                    public List<string> LastLinesPreview { get; set; } = new();
    
                    public long FileSize { get; set; }
    
                    public DateTime? LastWriteTime { get; set; }
    
                    public string ParserAvailability { get; set; } = "Unknown";
    
    
        }
    // ===== Journal Viewer Tab (Client-side lightweight preview) =====
        public sealed class JournalPreviewViewModel
        {
            public DateTime DateFrom { get; set; } = DateTime.UtcNow.AddDays(-1);
            public DateTime DateTo { get; set; } = DateTime.UtcNow;
            public string StatusFilter { get; set; } = "All";
            public string TypeFilter { get; set; } = "All";
            public string SearchText { get; set; } = string.Empty;
            public int TransactionCount { get; set; }
            public Dictionary<string, int> StatusCounts { get; set; } = new();
            public List<string> LastLinesPreview { get; set; } = new();
            public long FileSize { get; set; }
            public DateTime? LastWriteTime { get; set; }
            public string ParserAvailability { get; set; } = "Unknown";
        }
    // Class: LocalHealthViewModel (from 3 sources)
        public sealed partial class LocalHealthViewModel
        {
            // --- Properties ---
                    public bool Connected { get; set; }
    
                    public string SessionId { get; set; } = string.Empty;
    
                    public string HandshakeState { get; set; } = "None";
    
                    public string HeartbeatState { get; set; } = "None";
    
                    public DateTime? LastHeartbeat { get; set; }
    
                    public DateTime? LastHandshake { get; set; }
    
                    public DateTime? LastSync { get; set; }
    
                    public int OutboxCount { get; set; }
    
                    public int FailedCount { get; set; }
    
                    public string WatcherStates { get; set; } = "inactive";
    
                    public string SocketState { get; set; } = "Disconnected";
    
                    public string ServiceState { get; set; } = "Unknown";
    
                    public double HealthScore { get; set; }
    
                    public string LastError { get; set; } = string.Empty;
    
                    public List<string> Recommendations { get; set; } = new();
    
                    public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: LogsViewModel (from 3 sources)
        public sealed partial class LogsViewModel
        {
            // --- Properties ---
                    public List<string> LogEntries { get; set; } = new();
    
                    public string SeverityFilter { get; set; } = "All";
    
                    public string ComponentFilter { get; set; } = "All";
    
                    public string CorrelationIdFilter { get; set; } = string.Empty;
    
                    public string SearchText { get; set; } = string.Empty;
    
                    public bool AutoScroll { get; set; } = true;
    
                    public bool RedactionEnabled { get; set; } = true;
    
                    public int TotalEntries { get; set; }
    
                    public int FilteredEntries { get; set; }
    
    
        }
    // Class: ServiceComponentEntry (from 3 sources)
        public sealed partial class ServiceComponentEntry
        {
            // --- Properties ---
                    public string Component { get; set; } = string.Empty;
    
                    public string Status { get; set; } = "Unknown";
    
                    public DateTime? LastTransition { get; set; }
    
                    public string LastError { get; set; } = string.Empty;
    
                    public string Details { get; set; } = string.Empty;
    
                    public string RecommendedAction { get; set; } = string.Empty;
    
    
        }
    // Class: ServicesControlViewModel (from 3 sources)
        public sealed partial class ServicesControlViewModel
        {
            // --- Properties ---
                    public List<ServiceComponentEntry> Components { get; set; } = new();
    
                    public bool WindowsStartup { get; set; }
    
                    public bool ServiceInstalled { get; set; }
    
    
        }
    // Class: SettingsViewModel (from 3 sources)
        public sealed partial class SettingsViewModel
        {
            // --- Properties ---
                    public string ServerHost { get; set; } = "127.0.0.1";
    
                    public int ServerPort { get; set; } = 8080;
    
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string ATM_Name { get; set; } = string.Empty;
    
                    public string Vendor { get; set; } = string.Empty;
    
                    public string Model { get; set; } = string.Empty;
    
                    public string JournalPath { get; set; } = string.Empty;
    
                    public string BackupPath { get; set; } = string.Empty;
    
                    public string ImageInboxPath { get; set; } = string.Empty;
    
                    public string ImageDestinationPath { get; set; } = string.Empty;
    
                    public bool AutoConnect { get; set; } = true;
    
                    public bool AutoBackup { get; set; } = true;
    
                    public bool DurableSync { get; set; } = true;
    
                    public bool AckRequired { get; set; } = true;
    
                    public bool DedupEnabled { get; set; } = true;
    
                    public bool EncryptionEnabled { get; set; }
    
                    public bool CompressionEnabled { get; set; }
    
                    public int HeartbeatIntervalSec { get; set; } = 10;
    
                    public int ReconnectIntervalSec { get; set; } = 15;
    
                    public bool IsDirty { get; set; }
    
    
        }
    // Class: SyncFileEntry (from 3 sources)
        public sealed partial class SyncFileEntry
        {
            // --- Properties ---
                    public string ItemId { get; set; } = string.Empty;
    
                    public string FileName { get; set; } = string.Empty;
    
                    public long Size { get; set; }
    
                    public string Status { get; set; } = "Pending";
    
                    public double Progress { get; set; }
    
                    public int Retries { get; set; }
    
                    public string Checksum { get; set; } = string.Empty;
    
                    public long BytesSent { get; set; }
    
                    public DateTime AddedTime { get; set; }
    
                    public DateTime? LastAttempt { get; set; }
    
                    public string? FailureReason { get; set; }
    
    
        }
    // Class: SyncViewModel (from 3 sources)
        public sealed partial class SyncViewModel
        {
            // --- Properties ---
                    public int PendingCount { get; set; }
    
                    public int SyncingCount { get; set; }
    
                    public int CompletedCount { get; set; }
    
                    public int FailedCount { get; set; }
    
                    public long TotalSize { get; set; }
    
                    public double SuccessRate { get; set; }
    
                    public double Progress { get; set; }
    
                    public List<SyncFileEntry> Files { get; set; } = new();
    
    
        }
    // Class: VendorPathEntry (from 3 sources)
        public sealed partial class VendorPathEntry
        {
            // --- Properties ---
                    public string JournalSource { get; set; } = string.Empty;
    
                    public string Backup { get; set; } = string.Empty;
    
                    public string TraceLog { get; set; } = string.Empty;
    
                    public string ImageInbox { get; set; } = string.Empty;
    
                    public string ImageDestination { get; set; } = string.Empty;
    
                    public string ScreenshotCache { get; set; } = string.Empty;
    
                    public string Extensions { get; set; } = "*.LOG,*.TXT";
    
                    public string RolloverBehavior { get; set; } = "Daily";
    
                    public bool ValidationPassed { get; set; }
    
                    public bool WritePermission { get; set; }
    
                    public string RestartNotes { get; set; } = string.Empty;
    
                    public bool IsDirty { get; set; }
    
    
        }
    // Class: VendorPathMappingViewModel (from 3 sources)
        public sealed partial class VendorPathMappingViewModel
        {
            // --- Properties ---
                    public string SelectedVendor { get; set; } = "NCR";
    
                    public Dictionary<string, VendorPathEntry> VendorPaths { get; set; } = new();
    
    
        }
}
