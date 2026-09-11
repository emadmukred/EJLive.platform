using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public partial class AlertEntry
        {
            public string AlertId { get; set; } = string.Empty;
    
    
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string AlertType { get; set; } = string.Empty;
    
    
            public string Severity { get; set; } = "Info";
    
    
            public string Message { get; set; } = string.Empty;
    
    
            public DateTime RaisedUtc { get; set; }
    
    
            public string Status { get; set; } = "Open";
    
    
            public string? AcknowledgedBy { get; set; }
    
    
            public string? AssignedTo { get; set; }
    
    
        }
    public sealed class AlertEntry
        {
            public string AlertId { get; set; } = string.Empty;
            public string ATM_ID { get; set; } = string.Empty;
            public string AlertType { get; set; } = string.Empty;
            public string Severity { get; set; } = "Info";
            public string Message { get; set; } = string.Empty;
            public DateTime RaisedUtc { get; set; }
            public string Status { get; set; } = "Open";
            public string? AcknowledgedBy { get; set; }
            public string? AssignedTo { get; set; }
        }
    public partial class AlertsRiskViewModel
        {
            public List<AlertEntry> Alerts { get; set; } = new();
    
    
        }
    // ===== Alerts & Risk Center =====
        public sealed class AlertsRiskViewModel
        {
            public List<AlertEntry> Alerts { get; set; } = new();
        }
    public partial class AtmDetailViewModel
        {
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string TerminalId { get; set; } = string.Empty;
    
    
            public string ATM_Name { get; set; } = string.Empty;
    
    
            public string Branch { get; set; } = string.Empty;
    
    
            public string Region { get; set; } = string.Empty;
    
    
            public string Vendor { get; set; } = string.Empty;
    
    
            public string Model { get; set; } = string.Empty;
    
    
            public string ConnectionState { get; set; } = "Unknown";
    
    
            public DateTime? LastHeartbeat { get; set; }
    
    
            public DateTime? LastSync { get; set; }
    
    
            public string LastJournalFile { get; set; } = string.Empty;
    
    
            public double HealthScore { get; set; }
    
    
            public int ActiveCommands { get; set; }
    
    
            public int RecentAlerts { get; set; }
    
    
            public int RecentFiles { get; set; }
    
    
            public string RecentTransactionsSummary { get; set; } = string.Empty;
    
    
        }
    // ===== ATM Details =====
        public sealed class AtmDetailViewModel
        {
            public string ATM_ID { get; set; } = string.Empty;
            public string TerminalId { get; set; } = string.Empty;
            public string ATM_Name { get; set; } = string.Empty;
            public string Branch { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public string Vendor { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string ConnectionState { get; set; } = "Unknown";
            public DateTime? LastHeartbeat { get; set; }
            public DateTime? LastSync { get; set; }
            public string LastJournalFile { get; set; } = string.Empty;
            public double HealthScore { get; set; }
            public int ActiveCommands { get; set; }
            public int RecentAlerts { get; set; }
            public int RecentFiles { get; set; }
            public string RecentTransactionsSummary { get; set; } = string.Empty;
        }
    public partial class AuditLogGridEntry
        {
            public DateTime Time { get; set; }
    
    
            public string User { get; set; } = string.Empty;
    
    
            public string Action { get; set; } = string.Empty;
    
    
            public string TargetATM { get; set; } = string.Empty;
    
    
            public string CommandId { get; set; } = string.Empty;
    
    
            public string Result { get; set; } = string.Empty;
    
    
            public string RiskLevel { get; set; } = "Low";
    
    
        }
    public sealed class AuditLogGridEntry
        {
            public DateTime Time { get; set; }
            public string User { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string TargetATM { get; set; } = string.Empty;
            public string CommandId { get; set; } = string.Empty;
            public string Result { get; set; } = string.Empty;
            public string RiskLevel { get; set; } = "Low";
        }
    public partial class AuditLogsViewModel
        {
            public List<AuditLogGridEntry> Entries { get; set; } = new();
    
    
        }
    // ===== Audit Logs =====
        public sealed class AuditLogsViewModel
        {
            public List<AuditLogGridEntry> Entries { get; set; } = new();
        }
    public partial class CommandQueueViewModel
        {
            public List<ServerCommandEntry> Commands { get; set; } = new();
    
    
        }
    // ===== Command Queue / Remote Operations =====
        public sealed class CommandQueueViewModel
        {
            public List<ServerCommandEntry> Commands { get; set; } = new();
        }
    public partial class DistributionPackageEntry
        {
            public string PackageId { get; set; } = string.Empty;
    
    
            public string PackageName { get; set; } = string.Empty;
    
    
            public string TargetVendor { get; set; } = string.Empty;
    
    
            public string TargetAtmGroup { get; set; } = string.Empty;
    
    
            public string SourceFile { get; set; } = string.Empty;
    
    
            public string SHA256 { get; set; } = string.Empty;
    
    
            public string DeploymentStatus { get; set; } = "Pending";
    
    
            public int DeployedTargets { get; set; }
    
    
            public int FailedTargets { get; set; }
    
    
        }
    public sealed class DistributionPackageEntry
        {
            public string PackageId { get; set; } = string.Empty;
            public string PackageName { get; set; } = string.Empty;
            public string TargetVendor { get; set; } = string.Empty;
            public string TargetAtmGroup { get; set; } = string.Empty;
            public string SourceFile { get; set; } = string.Empty;
            public string SHA256 { get; set; } = string.Empty;
            public string DeploymentStatus { get; set; } = "Pending";
            public int DeployedTargets { get; set; }
            public int FailedTargets { get; set; }
        }
    public partial class EvidenceTimelineEntry
        {
            public DateTime Timestamp { get; set; }
    
    
            public string Source { get; set; } = string.Empty;
    
    
            public string Event { get; set; } = string.Empty;
    
    
            public double Confidence { get; set; }
    
    
        }
    public sealed class EvidenceTimelineEntry
        {
            public DateTime Timestamp { get; set; }
            public string Source { get; set; } = string.Empty;
            public string Event { get; set; } = string.Empty;
            public double Confidence { get; set; }
        }
    public partial class ImageDistributionViewModel
        {
            public List<DistributionPackageEntry> Packages { get; set; } = new();
    
    
        }
    // ===== Image / Content Distribution =====
        public sealed class ImageDistributionViewModel
        {
            public List<DistributionPackageEntry> Packages { get; set; } = new();
        }
    public partial class IngestionArchiveViewModel
        {
            public int StagingCount { get; set; }
    
    
            public int VerifiedCount { get; set; }
    
    
            public int ArchivedCount { get; set; }
    
    
            public int ChecksumFailures { get; set; }
    
    
            public int Duplicates { get; set; }
    
    
            public int PendingParseJobs { get; set; }
    
    
            public string ArchiveRootPath { get; set; } = string.Empty;
    
    
            public List<IngestionFileEntry> Files { get; set; } = new();
    
    
        }
    // ===== Ingestion & Archive =====
        public sealed class IngestionArchiveViewModel
        {
            public int StagingCount { get; set; }
            public int VerifiedCount { get; set; }
            public int ArchivedCount { get; set; }
            public int ChecksumFailures { get; set; }
            public int Duplicates { get; set; }
            public int PendingParseJobs { get; set; }
            public string ArchiveRootPath { get; set; } = string.Empty;
            public List<IngestionFileEntry> Files { get; set; } = new();
        }
    public partial class IngestionFileEntry
        {
            public string FileName { get; set; } = string.Empty;
    
    
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public long FileSize { get; set; }
    
    
            public string Status { get; set; } = "Pending";
    
    
            public string Checksum { get; set; } = string.Empty;
    
    
            public DateTime ReceivedUtc { get; set; }
    
    
            public string? FailureReason { get; set; }
    
    
        }
    public sealed class IngestionFileEntry
        {
            public string FileName { get; set; } = string.Empty;
            public string ATM_ID { get; set; } = string.Empty;
            public long FileSize { get; set; }
            public string Status { get; set; } = "Pending";
            public string Checksum { get; set; } = string.Empty;
            public DateTime ReceivedUtc { get; set; }
            public string? FailureReason { get; set; }
        }
    public partial class JournalAnalysisViewModel
        {
            public string ParserRunId { get; set; } = string.Empty;
    
    
            public string VendorParser { get; set; } = string.Empty;
    
    
            public string FileName { get; set; } = string.Empty;
    
    
            public int TransactionCount { get; set; }
    
    
            public int SuccessCount { get; set; }
    
    
            public int FailedCount { get; set; }
    
    
            public int SuspiciousCount { get; set; }
    
    
            public int ReversalCount { get; set; }
    
    
            public int PartialDispenseCount { get; set; }
    
    
            public int ApprovedNoDispenseCount { get; set; }
    
    
            public int MissingSequenceCount { get; set; }
    
    
            public int DuplicateSequenceCount { get; set; }
    
    
            public string EvidenceQuality { get; set; } = "Unknown";
    
    
        }
    // ===== Journal Analysis / Parser Results =====
        public sealed class JournalAnalysisViewModel
        {
            public string ParserRunId { get; set; } = string.Empty;
            public string VendorParser { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public int TransactionCount { get; set; }
            public int SuccessCount { get; set; }
            public int FailedCount { get; set; }
            public int SuspiciousCount { get; set; }
            public int ReversalCount { get; set; }
            public int PartialDispenseCount { get; set; }
            public int ApprovedNoDispenseCount { get; set; }
            public int MissingSequenceCount { get; set; }
            public int DuplicateSequenceCount { get; set; }
            public string EvidenceQuality { get; set; } = "Unknown";
        }
    public partial class LiveSessionsViewModel
        {
            public List<SessionGridEntry> Sessions { get; set; } = new();
    
    
        }
    // ===== Live Sessions =====
        public sealed class LiveSessionsViewModel
        {
            public List<SessionGridEntry> Sessions { get; set; } = new();
        }
    public partial class NocOverviewViewModel
        {
            public int TotalATMs { get; set; }
    
    
            public int OnlineATMs { get; set; }
    
    
            public int OfflineATMs { get; set; }
    
    
            public int WarningATMs { get; set; }
    
    
            public int CriticalATMs { get; set; }
    
    
            public double SyncSuccessRate { get; set; }
    
    
            public int OpenAlerts { get; set; }
    
    
            public int FailedTransfers { get; set; }
    
    
            public DateTime? LastIngestionTime { get; set; }
    
    
            public Dictionary<string, int> VendorDistribution { get; set; } = new();
    
    
            public Dictionary<string, int> RegionDistribution { get; set; } = new();
    
    
            public string SelectedFilter { get; set; } = "All";
    
    
        }
    /// <summary>
        /// Server-side dashboard view models.
        /// Structured data contracts for the 16 server screens defined in section 10
        /// of the project master document. Keeps server UI decoupled from ingestion logic.
        /// </summary>
    
        // ===== NOC Overview =====
        public sealed class NocOverviewViewModel
        {
            public int TotalATMs { get; set; }
            public int OnlineATMs { get; set; }
            public int OfflineATMs { get; set; }
            public int WarningATMs { get; set; }
            public int CriticalATMs { get; set; }
            public double SyncSuccessRate { get; set; }
            public int OpenAlerts { get; set; }
            public int FailedTransfers { get; set; }
            public DateTime? LastIngestionTime { get; set; }
            public Dictionary<string, int> VendorDistribution { get; set; } = new();
            public Dictionary<string, int> RegionDistribution { get; set; } = new();
            public string SelectedFilter { get; set; } = "All";
        }
    public partial class ReportsCenterViewModel
        {
            public List<ReportTemplate> AvailableReports { get; set; } = new();
    
    
            public string SelectedReport { get; set; } = string.Empty;
    
    
            public string ExportFormat { get; set; } = "Excel";
    
    
        }
    // ===== Reports Center =====
        public sealed class ReportsCenterViewModel
        {
            public List<ReportTemplate> AvailableReports { get; set; } = new();
            public string SelectedReport { get; set; } = string.Empty;
            public string ExportFormat { get; set; } = "Excel";
        }
    public partial class ReportTemplate
        {
            public string ReportName { get; set; } = string.Empty;
    
    
            public string Description { get; set; } = string.Empty;
    
    
            public DateTime? LastGenerated { get; set; }
    
    
            public bool IsScheduled { get; set; }
    
    
        }
    public sealed class ReportTemplate
        {
            public string ReportName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime? LastGenerated { get; set; }
            public bool IsScheduled { get; set; }
        }
    public partial class ServerCommandEntry
        {
            public string CommandId { get; set; } = string.Empty;
    
    
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string CommandType { get; set; } = string.Empty;
    
    
            public string State { get; set; } = "Draft";
    
    
            public string RequestedBy { get; set; } = string.Empty;
    
    
            public string? ApprovedBy { get; set; }
    
    
            public string PolicyResult { get; set; } = string.Empty;
    
    
            public string? AuditBefore { get; set; }
    
    
            public string? AuditAfter { get; set; }
    
    
            public string? RollbackPlan { get; set; }
    
    
            public string Result { get; set; } = string.Empty;
    
    
        }
    public sealed class ServerCommandEntry
        {
            public string CommandId { get; set; } = string.Empty;
            public string ATM_ID { get; set; } = string.Empty;
            public string CommandType { get; set; } = string.Empty;
            public string State { get; set; } = "Draft";
            public string RequestedBy { get; set; } = string.Empty;
            public string? ApprovedBy { get; set; }
            public string PolicyResult { get; set; } = string.Empty;
            public string? AuditBefore { get; set; }
            public string? AuditAfter { get; set; }
            public string? RollbackPlan { get; set; }
            public string Result { get; set; } = string.Empty;
        }
    public partial class ServerSettingsViewModel
        {
            public int ListeningPort { get; set; } = 8080;
    
    
            public string ArchiveRoot { get; set; } = string.Empty;
    
    
            public string StagingRoot { get; set; } = string.Empty;
    
    
            public string DatabasePath { get; set; } = string.Empty;
    
    
            public int RetentionDays { get; set; } = 90;
    
    
            public string ParserSettings { get; set; } = string.Empty;
    
    
            public string CommandPolicy { get; set; } = string.Empty;
    
    
            public bool TlsEnabled { get; set; }
    
    
            public string CertificatePath { get; set; } = string.Empty;
    
    
            public string ReportOutputPath { get; set; } = string.Empty;
    
    
            public bool IsDirty { get; set; }
    
    
        }
    // ===== Server Settings =====
        public sealed class ServerSettingsViewModel
        {
            public int ListeningPort { get; set; } = 8080;
            public string ArchiveRoot { get; set; } = string.Empty;
            public string StagingRoot { get; set; } = string.Empty;
            public string DatabasePath { get; set; } = string.Empty;
            public int RetentionDays { get; set; } = 90;
            public string ParserSettings { get; set; } = string.Empty;
            public string CommandPolicy { get; set; } = string.Empty;
            public bool TlsEnabled { get; set; }
            public string CertificatePath { get; set; } = string.Empty;
            public string ReportOutputPath { get; set; } = string.Empty;
            public bool IsDirty { get; set; }
        }
    public partial class SessionGridEntry
        {
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string SessionId { get; set; } = string.Empty;
    
    
            public string ProtocolVersion { get; set; } = string.Empty;
    
    
            public string ClientVersion { get; set; } = string.Empty;
    
    
            public DateTime LastHeartbeat { get; set; }
    
    
            public int PendingCommands { get; set; }
    
    
            public long BytesIn { get; set; }
    
    
            public long BytesOut { get; set; }
    
    
            public string State { get; set; } = "Online";
    
    
        }
    public sealed class SessionGridEntry
        {
            public string ATM_ID { get; set; } = string.Empty;
            public string SessionId { get; set; } = string.Empty;
            public string ProtocolVersion { get; set; } = string.Empty;
            public string ClientVersion { get; set; } = string.Empty;
            public DateTime LastHeartbeat { get; set; }
            public int PendingCommands { get; set; }
            public long BytesIn { get; set; }
            public long BytesOut { get; set; }
            public string State { get; set; } = "Online";
        }
    public partial class TransactionForensicsViewModel
        {
            public string TransactionNumber { get; set; } = string.Empty;
    
    
            public DateTime? DateTime { get; set; }
    
    
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string CardMasked { get; set; } = string.Empty;
    
    
            public string AccountMasked { get; set; } = string.Empty;
    
    
            public decimal? Amount { get; set; }
    
    
            public string Currency { get; set; } = string.Empty;
    
    
            public string STAN { get; set; } = string.Empty;
    
    
            public string RRN { get; set; } = string.Empty;
    
    
            public int? Cass1 { get; set; }
    
    
            public int? Cass2 { get; set; }
    
    
            public int? Cass3 { get; set; }
    
    
            public int? Cass4 { get; set; }
    
    
            public string MCodes { get; set; } = string.Empty;
    
    
            public string RCodes { get; set; } = string.Empty;
    
    
            public string HostResponse { get; set; } = string.Empty;
    
    
            public List<string> RawLineRange { get; set; } = new();
    
    
            public List<EvidenceTimelineEntry> EvidenceTimeline { get; set; } = new();
    
    
            public double Confidence { get; set; }
    
    
        }
    // ===== Transaction Forensics =====
        public sealed class TransactionForensicsViewModel
        {
            public string TransactionNumber { get; set; } = string.Empty;
            public DateTime? DateTime { get; set; }
            public string ATM_ID { get; set; } = string.Empty;
            public string CardMasked { get; set; } = string.Empty;
            public string AccountMasked { get; set; } = string.Empty;
            public decimal? Amount { get; set; }
            public string Currency { get; set; } = string.Empty;
            public string STAN { get; set; } = string.Empty;
            public string RRN { get; set; } = string.Empty;
            public int? Cass1 { get; set; }
            public int? Cass2 { get; set; }
            public int? Cass3 { get; set; }
            public int? Cass4 { get; set; }
            public string MCodes { get; set; } = string.Empty;
            public string RCodes { get; set; } = string.Empty;
            public string HostResponse { get; set; } = string.Empty;
            public List<string> RawLineRange { get; set; } = new();
            public List<EvidenceTimelineEntry> EvidenceTimeline { get; set; } = new();
            public double Confidence { get; set; }
        }
    public partial class UserGridEntry
        {
            public string UserId { get; set; } = string.Empty;
    
    
            public string Username { get; set; } = string.Empty;
    
    
            public string DisplayName { get; set; } = string.Empty;
    
    
            public string Role { get; set; } = "Viewer";
    
    
            public bool IsActive { get; set; }
    
    
            public DateTime LastLogin { get; set; }
    
    
        }
    public sealed class UserGridEntry
        {
            public string UserId { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Role { get; set; } = "Viewer";
            public bool IsActive { get; set; }
            public DateTime LastLogin { get; set; }
        }
    public partial class UsersRbacViewModel
        {
            public List<UserGridEntry> Users { get; set; } = new();
    
    
            public List<string> Roles { get; set; } = new();
    
    
            public List<string> Modules { get; set; } = new();
    
    
        }
    // ===== Users / RBAC / Security =====
        public sealed class UsersRbacViewModel
        {
            public List<UserGridEntry> Users { get; set; } = new();
            public List<string> Roles { get; set; } = new();
            public List<string> Modules { get; set; } = new();
        }
    public partial class VendorProfile
        {
            public string Vendor { get; set; } = string.Empty;
    
    
            public string Model { get; set; } = string.Empty;
    
    
            public string JournalSource { get; set; } = string.Empty;
    
    
            public string BackupPath { get; set; } = string.Empty;
    
    
            public string TraceLogPath { get; set; } = string.Empty;
    
    
            public string ImageDestination { get; set; } = string.Empty;
    
    
            public string RolloverRule { get; set; } = "Daily";
    
    
            public string Extensions { get; set; } = "*.LOG,*.TXT";
    
    
            public string ValidationRule { get; set; } = string.Empty;
    
    
        }
    public sealed class VendorProfile
        {
            public string Vendor { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string JournalSource { get; set; } = string.Empty;
            public string BackupPath { get; set; } = string.Empty;
            public string TraceLogPath { get; set; } = string.Empty;
            public string ImageDestination { get; set; } = string.Empty;
            public string RolloverRule { get; set; } = "Daily";
            public string Extensions { get; set; } = "*.LOG,*.TXT";
            public string ValidationRule { get; set; } = string.Empty;
        }
    public partial class VendorProfilesViewModel
        {
            public List<VendorProfile> Profiles { get; set; } = new();
    
    
        }
    // ===== Vendor Profiles =====
        public sealed class VendorProfilesViewModel
        {
            public List<VendorProfile> Profiles { get; set; } = new();
        }
    public partial class VerificationViewModel
        {
            public bool BuildSuccess { get; set; }
    
    
            public bool TestsPassing { get; set; }
    
    
            public bool VerificationPassing { get; set; }
    
    
            public string ActiveCompileMapStatus { get; set; } = "Unknown";
    
    
            public string ServiceBoundaryStatus { get; set; } = "Unknown";
    
    
            public string ParserRegistryStatus { get; set; } = "Unknown";
    
    
            public string XfsRegistryStatus { get; set; } = "Unknown";
    
    
            public string SecurityProbeStatus { get; set; } = "Unknown";
    
    
            public DateTime? LastVerificationRun { get; set; }
    
    
            public string VerificationLog { get; set; } = string.Empty;
    
    
        }
    // ===== Verification / System Health =====
        public sealed class VerificationViewModel
        {
            public bool BuildSuccess { get; set; }
            public bool TestsPassing { get; set; }
            public bool VerificationPassing { get; set; }
            public string ActiveCompileMapStatus { get; set; } = "Unknown";
            public string ServiceBoundaryStatus { get; set; } = "Unknown";
            public string ParserRegistryStatus { get; set; } = "Unknown";
            public string XfsRegistryStatus { get; set; } = "Unknown";
            public string SecurityProbeStatus { get; set; } = "Unknown";
            public DateTime? LastVerificationRun { get; set; }
            public string VerificationLog { get; set; } = string.Empty;
        }
    public partial class XfsCorrelationViewModel
        {
            public List<XfsEventGridEntry> Events { get; set; } = new();
    
    
        }
    // ===== XFS / TRACE Correlation =====
        public sealed class XfsCorrelationViewModel
        {
            public List<XfsEventGridEntry> Events { get; set; } = new();
        }
    public partial class XfsEventGridEntry
        {
            public string EventId { get; set; } = string.Empty;
    
    
            public string Vendor { get; set; } = string.Empty;
    
    
            public string DeviceClass { get; set; } = string.Empty;
    
    
            public string Severity { get; set; } = "Info";
    
    
            public string Code { get; set; } = string.Empty;
    
    
            public string Message { get; set; } = string.Empty;
    
    
            public DateTime Timestamp { get; set; }
    
    
            public string SourceFile { get; set; } = string.Empty;
    
    
            public string RawLine { get; set; } = string.Empty;
    
    
            public string LinkedTransaction { get; set; } = string.Empty;
    
    
            public double CorrelationConfidence { get; set; }
    
    
        }
    public sealed class XfsEventGridEntry
        {
            public string EventId { get; set; } = string.Empty;
            public string Vendor { get; set; } = string.Empty;
            public string DeviceClass { get; set; } = string.Empty;
            public string Severity { get; set; } = "Info";
            public string Code { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public string SourceFile { get; set; } = string.Empty;
            public string RawLine { get; set; } = string.Empty;
            public string LinkedTransaction { get; set; } = string.Empty;
            public double CorrelationConfidence { get; set; }
        }

    // Class: AlertEntry (from 3 sources)
        public sealed partial class AlertEntry
        {
            // --- Properties ---
                    public string AlertId { get; set; } = string.Empty;
    
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string AlertType { get; set; } = string.Empty;
    
                    public string Severity { get; set; } = "Info";
    
                    public string Message { get; set; } = string.Empty;
    
                    public DateTime RaisedUtc { get; set; }
    
                    public string Status { get; set; } = "Open";
    
                    public string? AcknowledgedBy { get; set; }
    
                    public string? AssignedTo { get; set; }
    
    
        }
    // Class: AlertsRiskViewModel (from 3 sources)
        public sealed partial class AlertsRiskViewModel
        {
            // --- Properties ---
                    public List<AlertEntry> Alerts { get; set; } = new();
    
    
        }
    // Class: AtmDetailViewModel (from 3 sources)
        public sealed partial class AtmDetailViewModel
        {
            // --- Properties ---
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string TerminalId { get; set; } = string.Empty;
    
                    public string ATM_Name { get; set; } = string.Empty;
    
                    public string Branch { get; set; } = string.Empty;
    
                    public string Region { get; set; } = string.Empty;
    
                    public string Vendor { get; set; } = string.Empty;
    
                    public string Model { get; set; } = string.Empty;
    
                    public string ConnectionState { get; set; } = "Unknown";
    
                    public DateTime? LastHeartbeat { get; set; }
    
                    public DateTime? LastSync { get; set; }
    
                    public string LastJournalFile { get; set; } = string.Empty;
    
                    public double HealthScore { get; set; }
    
                    public int ActiveCommands { get; set; }
    
                    public int RecentAlerts { get; set; }
    
                    public int RecentFiles { get; set; }
    
                    public string RecentTransactionsSummary { get; set; } = string.Empty;
    
    
        }
    // Class: AuditLogGridEntry (from 3 sources)
        public sealed partial class AuditLogGridEntry
        {
            // --- Properties ---
                    public DateTime Time { get; set; }
    
                    public string User { get; set; } = string.Empty;
    
                    public string Action { get; set; } = string.Empty;
    
                    public string TargetATM { get; set; } = string.Empty;
    
                    public string CommandId { get; set; } = string.Empty;
    
                    public string Result { get; set; } = string.Empty;
    
                    public string RiskLevel { get; set; } = "Low";
    
    
        }
    // Class: AuditLogsViewModel (from 3 sources)
        public sealed partial class AuditLogsViewModel
        {
            // --- Properties ---
                    public List<AuditLogGridEntry> Entries { get; set; } = new();
    
    
        }
    // Class: CommandQueueViewModel (from 3 sources)
        public sealed partial class CommandQueueViewModel
        {
            // --- Properties ---
                    public List<ServerCommandEntry> Commands { get; set; } = new();
    
    
        }
    // Class: DistributionPackageEntry (from 3 sources)
        public sealed partial class DistributionPackageEntry
        {
            // --- Properties ---
                    public string PackageId { get; set; } = string.Empty;
    
                    public string PackageName { get; set; } = string.Empty;
    
                    public string TargetVendor { get; set; } = string.Empty;
    
                    public string TargetAtmGroup { get; set; } = string.Empty;
    
                    public string SourceFile { get; set; } = string.Empty;
    
                    public string SHA256 { get; set; } = string.Empty;
    
                    public string DeploymentStatus { get; set; } = "Pending";
    
                    public int DeployedTargets { get; set; }
    
                    public int FailedTargets { get; set; }
    
    
        }
    // Class: EvidenceTimelineEntry (from 3 sources)
        public sealed partial class EvidenceTimelineEntry
        {
            // --- Properties ---
                    public DateTime Timestamp { get; set; }
    
                    public string Source { get; set; } = string.Empty;
    
                    public string Event { get; set; } = string.Empty;
    
                    public double Confidence { get; set; }
    
    
        }
    // Class: ImageDistributionViewModel (from 3 sources)
        public sealed partial class ImageDistributionViewModel
        {
            // --- Properties ---
                    public List<DistributionPackageEntry> Packages { get; set; } = new();
    
    
        }
    // Class: IngestionArchiveViewModel (from 3 sources)
        public sealed partial class IngestionArchiveViewModel
        {
            // --- Properties ---
                    public int StagingCount { get; set; }
    
                    public int VerifiedCount { get; set; }
    
                    public int ArchivedCount { get; set; }
    
                    public int ChecksumFailures { get; set; }
    
                    public int Duplicates { get; set; }
    
                    public int PendingParseJobs { get; set; }
    
                    public string ArchiveRootPath { get; set; } = string.Empty;
    
                    public List<IngestionFileEntry> Files { get; set; } = new();
    
    
        }
    // Class: IngestionFileEntry (from 3 sources)
        public sealed partial class IngestionFileEntry
        {
            // --- Properties ---
                    public string FileName { get; set; } = string.Empty;
    
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public long FileSize { get; set; }
    
                    public string Status { get; set; } = "Pending";
    
                    public string Checksum { get; set; } = string.Empty;
    
                    public DateTime ReceivedUtc { get; set; }
    
                    public string? FailureReason { get; set; }
    
    
        }
    // Class: JournalAnalysisViewModel (from 3 sources)
        public sealed partial class JournalAnalysisViewModel
        {
            // --- Properties ---
                    public string ParserRunId { get; set; } = string.Empty;
    
                    public string VendorParser { get; set; } = string.Empty;
    
                    public string FileName { get; set; } = string.Empty;
    
                    public int TransactionCount { get; set; }
    
                    public int SuccessCount { get; set; }
    
                    public int FailedCount { get; set; }
    
                    public int SuspiciousCount { get; set; }
    
                    public int ReversalCount { get; set; }
    
                    public int PartialDispenseCount { get; set; }
    
                    public int ApprovedNoDispenseCount { get; set; }
    
                    public int MissingSequenceCount { get; set; }
    
                    public int DuplicateSequenceCount { get; set; }
    
                    public string EvidenceQuality { get; set; } = "Unknown";
    
    
        }
    // Class: LiveSessionsViewModel (from 3 sources)
        public sealed partial class LiveSessionsViewModel
        {
            // --- Properties ---
                    public List<SessionGridEntry> Sessions { get; set; } = new();
    
    
        }
    // Class: NocOverviewViewModel (from 3 sources)
        public sealed partial class NocOverviewViewModel
        {
            // --- Properties ---
                    public int TotalATMs { get; set; }
    
                    public int OnlineATMs { get; set; }
    
                    public int OfflineATMs { get; set; }
    
                    public int WarningATMs { get; set; }
    
                    public int CriticalATMs { get; set; }
    
                    public double SyncSuccessRate { get; set; }
    
                    public int OpenAlerts { get; set; }
    
                    public int FailedTransfers { get; set; }
    
                    public DateTime? LastIngestionTime { get; set; }
    
                    public Dictionary<string, int> VendorDistribution { get; set; } = new();
    
                    public Dictionary<string, int> RegionDistribution { get; set; } = new();
    
                    public string SelectedFilter { get; set; } = "All";
    
    
        }
    // Class: ReportsCenterViewModel (from 3 sources)
        public sealed partial class ReportsCenterViewModel
        {
            // --- Properties ---
                    public List<ReportTemplate> AvailableReports { get; set; } = new();
    
                    public string SelectedReport { get; set; } = string.Empty;
    
                    public string ExportFormat { get; set; } = "Excel";
    
    
        }
    // Class: ReportTemplate (from 3 sources)
        public sealed partial class ReportTemplate
        {
            // --- Properties ---
                    public string ReportName { get; set; } = string.Empty;
    
                    public string Description { get; set; } = string.Empty;
    
                    public DateTime? LastGenerated { get; set; }
    
                    public bool IsScheduled { get; set; }
    
    
        }
    // Class: ServerCommandEntry (from 3 sources)
        public sealed partial class ServerCommandEntry
        {
            // --- Properties ---
                    public string CommandId { get; set; } = string.Empty;
    
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string CommandType { get; set; } = string.Empty;
    
                    public string State { get; set; } = "Draft";
    
                    public string RequestedBy { get; set; } = string.Empty;
    
                    public string? ApprovedBy { get; set; }
    
                    public string PolicyResult { get; set; } = string.Empty;
    
                    public string? AuditBefore { get; set; }
    
                    public string? AuditAfter { get; set; }
    
                    public string? RollbackPlan { get; set; }
    
                    public string Result { get; set; } = string.Empty;
    
    
        }
    // Class: ServerSettingsViewModel (from 3 sources)
        public sealed partial class ServerSettingsViewModel
        {
            // --- Properties ---
                    public int ListeningPort { get; set; } = 8080;
    
                    public string ArchiveRoot { get; set; } = string.Empty;
    
                    public string StagingRoot { get; set; } = string.Empty;
    
                    public string DatabasePath { get; set; } = string.Empty;
    
                    public int RetentionDays { get; set; } = 90;
    
                    public string ParserSettings { get; set; } = string.Empty;
    
                    public string CommandPolicy { get; set; } = string.Empty;
    
                    public bool TlsEnabled { get; set; }
    
                    public string CertificatePath { get; set; } = string.Empty;
    
                    public string ReportOutputPath { get; set; } = string.Empty;
    
                    public bool IsDirty { get; set; }
    
    
        }
    // Class: SessionGridEntry (from 3 sources)
        public sealed partial class SessionGridEntry
        {
            // --- Properties ---
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string SessionId { get; set; } = string.Empty;
    
                    public string ProtocolVersion { get; set; } = string.Empty;
    
                    public string ClientVersion { get; set; } = string.Empty;
    
                    public DateTime LastHeartbeat { get; set; }
    
                    public int PendingCommands { get; set; }
    
                    public long BytesIn { get; set; }
    
                    public long BytesOut { get; set; }
    
                    public string State { get; set; } = "Online";
    
    
        }
    // Class: TransactionForensicsViewModel (from 3 sources)
        public sealed partial class TransactionForensicsViewModel
        {
            // --- Properties ---
                    public string TransactionNumber { get; set; } = string.Empty;
    
                    public DateTime? DateTime { get; set; }
    
                    public string ATM_ID { get; set; } = string.Empty;
    
                    public string CardMasked { get; set; } = string.Empty;
    
                    public string AccountMasked { get; set; } = string.Empty;
    
                    public decimal? Amount { get; set; }
    
                    public string Currency { get; set; } = string.Empty;
    
                    public string STAN { get; set; } = string.Empty;
    
                    public string RRN { get; set; } = string.Empty;
    
                    public int? Cass1 { get; set; }
    
                    public int? Cass2 { get; set; }
    
                    public int? Cass3 { get; set; }
    
                    public int? Cass4 { get; set; }
    
                    public string MCodes { get; set; } = string.Empty;
    
                    public string RCodes { get; set; } = string.Empty;
    
                    public string HostResponse { get; set; } = string.Empty;
    
                    public List<string> RawLineRange { get; set; } = new();
    
                    public List<EvidenceTimelineEntry> EvidenceTimeline { get; set; } = new();
    
                    public double Confidence { get; set; }
    
    
        }
    // Class: UserGridEntry (from 3 sources)
        public sealed partial class UserGridEntry
        {
            // --- Properties ---
                    public string UserId { get; set; } = string.Empty;
    
                    public string Username { get; set; } = string.Empty;
    
                    public string DisplayName { get; set; } = string.Empty;
    
                    public string Role { get; set; } = "Viewer";
    
                    public bool IsActive { get; set; }
    
                    public DateTime LastLogin { get; set; }
    
    
        }
    // Class: UsersRbacViewModel (from 3 sources)
        public sealed partial class UsersRbacViewModel
        {
            // --- Properties ---
                    public List<UserGridEntry> Users { get; set; } = new();
    
                    public List<string> Roles { get; set; } = new();
    
                    public List<string> Modules { get; set; } = new();
    
    
        }
    // Class: VendorProfile (from 3 sources)
        public sealed partial class VendorProfile
        {
            // --- Properties ---
                    public string Vendor { get; set; } = string.Empty;
    
                    public string Model { get; set; } = string.Empty;
    
                    public string JournalSource { get; set; } = string.Empty;
    
                    public string BackupPath { get; set; } = string.Empty;
    
                    public string TraceLogPath { get; set; } = string.Empty;
    
                    public string ImageDestination { get; set; } = string.Empty;
    
                    public string RolloverRule { get; set; } = "Daily";
    
                    public string Extensions { get; set; } = "*.LOG,*.TXT";
    
                    public string ValidationRule { get; set; } = string.Empty;
    
    
        }
    // Class: VendorProfilesViewModel (from 3 sources)
        public sealed partial class VendorProfilesViewModel
        {
            // --- Properties ---
                    public List<VendorProfile> Profiles { get; set; } = new();
    
    
        }
    // Class: VerificationViewModel (from 3 sources)
        public sealed partial class VerificationViewModel
        {
            // --- Properties ---
                    public bool BuildSuccess { get; set; }
    
                    public bool TestsPassing { get; set; }
    
                    public bool VerificationPassing { get; set; }
    
                    public string ActiveCompileMapStatus { get; set; } = "Unknown";
    
                    public string ServiceBoundaryStatus { get; set; } = "Unknown";
    
                    public string ParserRegistryStatus { get; set; } = "Unknown";
    
                    public string XfsRegistryStatus { get; set; } = "Unknown";
    
                    public string SecurityProbeStatus { get; set; } = "Unknown";
    
                    public DateTime? LastVerificationRun { get; set; }
    
                    public string VerificationLog { get; set; } = string.Empty;
    
    
        }
    // Class: XfsCorrelationViewModel (from 3 sources)
        public sealed partial class XfsCorrelationViewModel
        {
            // --- Properties ---
                    public List<XfsEventGridEntry> Events { get; set; } = new();
    
    
        }
    // Class: XfsEventGridEntry (from 3 sources)
        public sealed partial class XfsEventGridEntry
        {
            // --- Properties ---
                    public string EventId { get; set; } = string.Empty;
    
                    public string Vendor { get; set; } = string.Empty;
    
                    public string DeviceClass { get; set; } = string.Empty;
    
                    public string Severity { get; set; } = "Info";
    
                    public string Code { get; set; } = string.Empty;
    
                    public string Message { get; set; } = string.Empty;
    
                    public DateTime Timestamp { get; set; }
    
                    public string SourceFile { get; set; } = string.Empty;
    
                    public string RawLine { get; set; } = string.Empty;
    
                    public string LinkedTransaction { get; set; } = string.Empty;
    
                    public double CorrelationConfidence { get; set; }
    
    
        }
}
