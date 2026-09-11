using System;
using System.Collections.Generic;

namespace EJLive.Core.Data
{
    public partial class AtmDeviceEntity
        {
            public string AtmId { get; set; } = string.Empty;
    
    
            public string AtmName { get; set; } = string.Empty;
    
    
            public string Vendor { get; set; } = string.Empty;
    
    
            public string Model { get; set; } = string.Empty;
    
    
            public string TerminalId { get; set; } = string.Empty;
    
    
            public string LUNO { get; set; } = string.Empty;
    
    
            public string Branch { get; set; } = string.Empty;
    
    
            public string Region { get; set; } = string.Empty;
    
    
            public string Province { get; set; } = string.Empty;
    
    
            public string City { get; set; } = string.Empty;
    
    
            public string Geography { get; set; } = string.Empty;
    
    
            public string IpAddress { get; set; } = string.Empty;
    
    
            public int Port { get; set; }
    
    
            public string OperationalMode { get; set; } = "Unknown";
    
    
            public string ConnectionState { get; set; } = "Offline";
    
    
            public string JournalSourcePath { get; set; } = string.Empty;
    
    
            public string BackupPath { get; set; } = string.Empty;
    
    
            public string ImageInboxPath { get; set; } = string.Empty;
    
    
            public string ImageDestinationPath { get; set; } = string.Empty;
    
    
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    /// <summary>
        /// Complete database schema contracts for EJLive Unified v2.0.
        /// Mirrors section 15 of the project master document.
        /// All entities are production-grade with audit fields.
        /// </summary>
    
        // ===== Core ATM Entity =====
    
        /// <summary>ATM device definition and location.</summary>
        public sealed class AtmDeviceEntity
        {
            public string AtmId { get; set; } = string.Empty;
            public string AtmName { get; set; } = string.Empty;
            public string Vendor { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public string TerminalId { get; set; } = string.Empty;
            public string LUNO { get; set; } = string.Empty;
            public string Branch { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public string Province { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Geography { get; set; } = string.Empty;
            public string IpAddress { get; set; } = string.Empty;
            public int Port { get; set; }
            public string OperationalMode { get; set; } = "Unknown";
            public string ConnectionState { get; set; } = "Offline";
            public string JournalSourcePath { get; set; } = string.Empty;
            public string BackupPath { get; set; } = string.Empty;
            public string ImageInboxPath { get; set; } = string.Empty;
            public string ImageDestinationPath { get; set; } = string.Empty;
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
            public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class AuditLogEntity
        {
            public string LogId { get; set; } = string.Empty;
    
    
            public string Action { get; set; } = string.Empty;
    
    
            public string UserId { get; set; } = string.Empty;
    
    
            public string UserRole { get; set; } = string.Empty;
    
    
            public string TargetAtm { get; set; } = string.Empty;
    
    
            public string CommandId { get; set; } = string.Empty;
    
    
            public string Details { get; set; } = string.Empty;
    
    
            public string Result { get; set; } = string.Empty;
    
    
            public string RiskLevel { get; set; } = "Low";
    
    
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== Audit Log =====
    
        /// <summary>General audit log for sensitive activity.</summary>
        public sealed class AuditLogEntity
        {
            public string LogId { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string UserRole { get; set; } = string.Empty;
            public string TargetAtm { get; set; } = string.Empty;
            public string CommandId { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
            public string Result { get; set; } = string.Empty;
            public string RiskLevel { get; set; } = "Low";
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class CashDistributionEntity
        {
            public string CashRecordId { get; set; } = string.Empty;
    
    
            public string AtmId { get; set; } = string.Empty;
    
    
            public string TransactionId { get; set; } = string.Empty;
    
    
            public string CassetteId { get; set; } = string.Empty;
    
    
            public int Denomination { get; set; }
    
    
            public long DispensedCount { get; set; }
    
    
            public long RejectedCount { get; set; }
    
    
            public long RetractedCount { get; set; }
    
    
            public long RemainingCount { get; set; }
    
    
            public string Currency { get; set; } = string.Empty;
    
    
            public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== Cash Distribution =====
    
        /// <summary>Cash distribution tracking per cassette.</summary>
        public sealed class CashDistributionEntity
        {
            public string CashRecordId { get; set; } = string.Empty;
            public string AtmId { get; set; } = string.Empty;
            public string TransactionId { get; set; } = string.Empty;
            public string CassetteId { get; set; } = string.Empty;
            public int Denomination { get; set; }
            public long DispensedCount { get; set; }
            public long RejectedCount { get; set; }
            public long RetractedCount { get; set; }
            public long RemainingCount { get; set; }
            public string Currency { get; set; } = string.Empty;
            public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class ClientSessionEntity
        {
            public string SessionId { get; set; } = string.Empty;
    
    
            public string AtmId { get; set; } = string.Empty;
    
    
            public string RemoteAddress { get; set; } = string.Empty;
    
    
            public string ClientVersion { get; set; } = string.Empty;
    
    
            public string ProtocolVersion { get; set; } = string.Empty;
    
    
            public DateTime ConnectedUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? DisconnectedUtc { get; set; }
    
    
            public int PendingCommands { get; set; }
    
    
            public long TotalBytesSent { get; set; }
    
    
            public long TotalBytesReceived { get; set; }
    
    
            public bool IsHealthy { get; set; } = true;
    
    
            public string SessionState { get; set; } = "Active";
    
    
        }
    // ===== Client Session =====
    
        /// <summary>Client session tracking.</summary>
        public sealed class ClientSessionEntity
        {
            public string SessionId { get; set; } = string.Empty;
            public string AtmId { get; set; } = string.Empty;
            public string RemoteAddress { get; set; } = string.Empty;
            public string ClientVersion { get; set; } = string.Empty;
            public string ProtocolVersion { get; set; } = string.Empty;
            public DateTime ConnectedUtc { get; set; } = DateTime.UtcNow;
            public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;
            public DateTime? DisconnectedUtc { get; set; }
            public int PendingCommands { get; set; }
            public long TotalBytesSent { get; set; }
            public long TotalBytesReceived { get; set; }
            public bool IsHealthy { get; set; } = true;
            public string SessionState { get; set; } = "Active";
        }
    public partial class CommandAuditEntity
        {
            public string AuditId { get; set; } = string.Empty;
    
    
            public string CommandId { get; set; } = string.Empty;
    
    
            public string Action { get; set; } = string.Empty;
    
    
            public string UserId { get; set; } = string.Empty;
    
    
            public string UserRole { get; set; } = string.Empty;
    
    
            public string TargetAtm { get; set; } = string.Empty;
    
    
            public string Details { get; set; } = string.Empty;
    
    
            public string Result { get; set; } = string.Empty;
    
    
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== Command Audit =====
    
        /// <summary>Audit log for sensitive commands.</summary>
        public sealed class CommandAuditEntity
        {
            public string AuditId { get; set; } = string.Empty;
            public string CommandId { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
            public string UserRole { get; set; } = string.Empty;
            public string TargetAtm { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
            public string Result { get; set; } = string.Empty;
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class CommandQueueEntity
        {
            public string CommandId { get; set; } = string.Empty;
    
    
            public string CorrelationId { get; set; } = string.Empty;
    
    
            public string CommandType { get; set; } = string.Empty;
    
    
            public string TargetAtm { get; set; } = string.Empty;
    
    
            public string OperatorId { get; set; } = string.Empty;
    
    
            public string ApproverId { get; set; } = string.Empty;
    
    
            public string RequiredRole { get; set; } = "Admin";
    
    
            public string RiskLevel { get; set; } = "Low";
    
    
            public string Payload { get; set; } = string.Empty;
    
    
            public string Signature { get; set; } = string.Empty;
    
    
            public string Nonce { get; set; } = string.Empty;
    
    
            public string State { get; set; } = "Draft";
    
    
            public string? FailureReason { get; set; }
    
    
            public string? RollbackPlan { get; set; }
    
    
            public bool RollbackExecuted { get; set; }
    
    
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? ApprovedUtc { get; set; }
    
    
            public DateTime ExpiryUtc { get; set; } = DateTime.UtcNow.AddMinutes(5);
    
    
            public DateTime? CompletedUtc { get; set; }
    
    
            public int RetryCount { get; set; }
    
    
            public int MaxRetries { get; set; } = 3;
    
    
        }
    // ===== Command Queue =====
    
        /// <summary>Remote command queue entry.</summary>
        public sealed class CommandQueueEntity
        {
            public string CommandId { get; set; } = string.Empty;
            public string CorrelationId { get; set; } = string.Empty;
            public string CommandType { get; set; } = string.Empty;
            public string TargetAtm { get; set; } = string.Empty;
            public string OperatorId { get; set; } = string.Empty;
            public string ApproverId { get; set; } = string.Empty;
            public string RequiredRole { get; set; } = "Admin";
            public string RiskLevel { get; set; } = "Low";
            public string Payload { get; set; } = string.Empty;
            public string Signature { get; set; } = string.Empty;
            public string Nonce { get; set; } = string.Empty;
            public string State { get; set; } = "Draft";
            public string? FailureReason { get; set; }
            public string? RollbackPlan { get; set; }
            public bool RollbackExecuted { get; set; }
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
            public DateTime? ApprovedUtc { get; set; }
            public DateTime ExpiryUtc { get; set; } = DateTime.UtcNow.AddMinutes(5);
            public DateTime? CompletedUtc { get; set; }
            public int RetryCount { get; set; }
            public int MaxRetries { get; set; } = 3;
        }
    public partial class CorrelationResultEntity
        {
            public string CorrelationId { get; set; } = string.Empty;
    
    
            public string TransactionId { get; set; } = string.Empty;
    
    
            public string EventId { get; set; } = string.Empty;
    
    
            public string MatchLevel { get; set; } = "None";
    
    
            public string MatchBasis { get; set; } = string.Empty;
    
    
            public double Confidence { get; set; }
    
    
            public string RootCause { get; set; } = string.Empty;
    
    
            public DateTime CorrelatedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== Correlation Result =====
    
        /// <summary>EJ/XFS/TRACE correlation results.</summary>
        public sealed class CorrelationResultEntity
        {
            public string CorrelationId { get; set; } = string.Empty;
            public string TransactionId { get; set; } = string.Empty;
            public string EventId { get; set; } = string.Empty;
            public string MatchLevel { get; set; } = "None";
            public string MatchBasis { get; set; } = string.Empty;
            public double Confidence { get; set; }
            public string RootCause { get; set; } = string.Empty;
            public DateTime CorrelatedUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class FileManifestEntity
        {
            public string ManifestId { get; set; } = string.Empty;
    
    
            public string AtmId { get; set; } = string.Empty;
    
    
            public string FileName { get; set; } = string.Empty;
    
    
            public long FileSize { get; set; }
    
    
            public string Checksum { get; set; } = string.Empty;
    
    
            public string FileType { get; set; } = "Journal";
    
    
            public string SourcePath { get; set; } = string.Empty;
    
    
            public string ArchivePath { get; set; } = string.Empty;
    
    
            public int TotalChunks { get; set; }
    
    
            public int ChunkSize { get; set; }
    
    
            public string Status { get; set; } = "Pending";
    
    
            public string? FailureReason { get; set; }
    
    
            public int RetryCount { get; set; }
    
    
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime LastModifiedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== File Manifest =====
    
        /// <summary>File manifest for transferred files.</summary>
        public sealed class FileManifestEntity
        {
            public string ManifestId { get; set; } = string.Empty;
            public string AtmId { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public long FileSize { get; set; }
            public string Checksum { get; set; } = string.Empty;
            public string FileType { get; set; } = "Journal";
            public string SourcePath { get; set; } = string.Empty;
            public string ArchivePath { get; set; } = string.Empty;
            public int TotalChunks { get; set; }
            public int ChunkSize { get; set; }
            public string Status { get; set; } = "Pending";
            public string? FailureReason { get; set; }
            public int RetryCount { get; set; }
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
            public DateTime LastModifiedUtc { get; set; } = DateTime.UtcNow;
        }
    // ===== Source of Truth =====
    
        /// <summary>Source-of-truth record for file decisions.</summary>
        public sealed class SourceTruthRecordEntity
        {
            public string RecordId { get; set; } = string.Empty;
            public string FilePath { get; set; } = string.Empty;
            public string Classification { get; set; } = string.Empty;
            public string Action { get; set; } = string.Empty;
            public string? Reason { get; set; }
            public string? ReplacementPath { get; set; }
            public DateTime ReviewedUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class HealthSnapshotEntity
        {
            public string SnapshotId { get; set; } = string.Empty;
    
    
            public string AtmId { get; set; } = string.Empty;
    
    
            public string SnapshotType { get; set; } = "Operational";
    
    
            public double HealthScore { get; set; }
    
    
            public double ConfidenceScore { get; set; }
    
    
            public string ConnectionState { get; set; } = "Unknown";
    
    
            public string OperationalMode { get; set; } = "Unknown";
    
    
            public string AgentServiceState { get; set; } = "Unknown";
    
    
            public int ActiveAlarms { get; set; }
    
    
            public string Priority { get; set; } = "None";
    
    
            public string? SnapshotJson { get; set; }
    
    
            public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== Health Snapshot =====
    
        /// <summary>Periodic health snapshot for ATM/service/server.</summary>
        public sealed class HealthSnapshotEntity
        {
            public string SnapshotId { get; set; } = string.Empty;
            public string AtmId { get; set; } = string.Empty;
            public string SnapshotType { get; set; } = "Operational";
            public double HealthScore { get; set; }
            public double ConfidenceScore { get; set; }
            public string ConnectionState { get; set; } = "Unknown";
            public string OperationalMode { get; set; } = "Unknown";
            public string AgentServiceState { get; set; } = "Unknown";
            public int ActiveAlarms { get; set; }
            public string Priority { get; set; } = "None";
            public string? SnapshotJson { get; set; }
            public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class ImagePackageEntity
        {
            public string PackageId { get; set; } = string.Empty;
    
    
            public string PackageName { get; set; } = string.Empty;
    
    
            public string TargetVendor { get; set; } = string.Empty;
    
    
            public string TargetAtmGroup { get; set; } = string.Empty;
    
    
            public string SourceFile { get; set; } = string.Empty;
    
    
            public long FileSize { get; set; }
    
    
            public string Checksum { get; set; } = string.Empty;
    
    
            public string Status { get; set; } = "Pending";
    
    
            public int TotalTargets { get; set; }
    
    
            public int DeployedTargets { get; set; }
    
    
            public int FailedTargets { get; set; }
    
    
            public string? FailureReason { get; set; }
    
    
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? DeployedUtc { get; set; }
    
    
        }
    // ===== Image Package =====
    
        /// <summary>Image/content distribution package.</summary>
        public sealed class ImagePackageEntity
        {
            public string PackageId { get; set; } = string.Empty;
            public string PackageName { get; set; } = string.Empty;
            public string TargetVendor { get; set; } = string.Empty;
            public string TargetAtmGroup { get; set; } = string.Empty;
            public string SourceFile { get; set; } = string.Empty;
            public long FileSize { get; set; }
            public string Checksum { get; set; } = string.Empty;
            public string Status { get; set; } = "Pending";
            public int TotalTargets { get; set; }
            public int DeployedTargets { get; set; }
            public int FailedTargets { get; set; }
            public string? FailureReason { get; set; }
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
            public DateTime? DeployedUtc { get; set; }
        }
    public partial class JournalFileEntity
        {
            public string JournalFileId { get; set; } = string.Empty;
    
    
            public string AtmId { get; set; } = string.Empty;
    
    
            public string FileName { get; set; } = string.Empty;
    
    
            public long FileSize { get; set; }
    
    
            public string Checksum { get; set; } = string.Empty;
    
    
            public string ArchivePath { get; set; } = string.Empty;
    
    
            public int TotalLines { get; set; }
    
    
            public int ParsedLines { get; set; }
    
    
            public int TransactionCount { get; set; }
    
    
            public string ParseStatus { get; set; } = "Pending";
    
    
            public string? ParseError { get; set; }
    
    
            public string ParserUsed { get; set; } = string.Empty;
    
    
            public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? ParsedUtc { get; set; }
    
    
        }
    // ===== Journal File =====
    
        /// <summary>Archived journal file with parse status.</summary>
        public sealed class JournalFileEntity
        {
            public string JournalFileId { get; set; } = string.Empty;
            public string AtmId { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public long FileSize { get; set; }
            public string Checksum { get; set; } = string.Empty;
            public string ArchivePath { get; set; } = string.Empty;
            public int TotalLines { get; set; }
            public int ParsedLines { get; set; }
            public int TransactionCount { get; set; }
            public string ParseStatus { get; set; } = "Pending";
            public string? ParseError { get; set; }
            public string ParserUsed { get; set; } = string.Empty;
            public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
            public DateTime? ParsedUtc { get; set; }
        }
    public partial class JournalTransactionEntity
        {
            public string TransactionId { get; set; } = string.Empty;
    
    
            public string AtmId { get; set; } = string.Empty;
    
    
            public string JournalFileId { get; set; } = string.Empty;
    
    
            public int StartLine { get; set; }
    
    
            public int EndLine { get; set; }
    
    
            public string CardMasked { get; set; } = string.Empty;
    
    
            public string AccountMasked { get; set; } = string.Empty;
    
    
            public decimal? Amount { get; set; }
    
    
            public string Currency { get; set; } = string.Empty;
    
    
            public string STAN { get; set; } = string.Empty;
    
    
            public string RRN { get; set; } = string.Empty;
    
    
            public int? Cassette1 { get; set; }
    
    
            public int? Cassette2 { get; set; }
    
    
            public int? Cassette3 { get; set; }
    
    
            public int? Cassette4 { get; set; }
    
    
            public string MCode { get; set; } = string.Empty;
    
    
            public string RCode { get; set; } = string.Empty;
    
    
            public string HostResponse { get; set; } = string.Empty;
    
    
            public string Classification { get; set; } = string.Empty;
    
    
            public double Confidence { get; set; }
    
    
            public string FinalStatus { get; set; } = string.Empty;
    
    
            public DateTime? TransactionDate { get; set; }
    
    
            public DateTime ParsedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== Journal Transaction =====
    
        /// <summary>Extracted journal transaction with financial fields.</summary>
        public sealed class JournalTransactionEntity
        {
            public string TransactionId { get; set; } = string.Empty;
            public string AtmId { get; set; } = string.Empty;
            public string JournalFileId { get; set; } = string.Empty;
            public int StartLine { get; set; }
            public int EndLine { get; set; }
            public string CardMasked { get; set; } = string.Empty;
            public string AccountMasked { get; set; } = string.Empty;
            public decimal? Amount { get; set; }
            public string Currency { get; set; } = string.Empty;
            public string STAN { get; set; } = string.Empty;
            public string RRN { get; set; } = string.Empty;
            public int? Cassette1 { get; set; }
            public int? Cassette2 { get; set; }
            public int? Cassette3 { get; set; }
            public int? Cassette4 { get; set; }
            public string MCode { get; set; } = string.Empty;
            public string RCode { get; set; } = string.Empty;
            public string HostResponse { get; set; } = string.Empty;
            public string Classification { get; set; } = string.Empty;
            public double Confidence { get; set; }
            public string FinalStatus { get; set; } = string.Empty;
            public DateTime? TransactionDate { get; set; }
            public DateTime ParsedUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class ParserEvidenceEntity
        {
            public string EvidenceId { get; set; } = string.Empty;
    
    
            public string RunId { get; set; } = string.Empty;
    
    
            public string TransactionId { get; set; } = string.Empty;
    
    
            public string EvidenceType { get; set; } = string.Empty;
    
    
            public string EvidenceValue { get; set; } = string.Empty;
    
    
            public double Confidence { get; set; }
    
    
            public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== Parser Evidence =====
    
        /// <summary>Evidence for each parser execution.</summary>
        public sealed class ParserEvidenceEntity
        {
            public string EvidenceId { get; set; } = string.Empty;
            public string RunId { get; set; } = string.Empty;
            public string TransactionId { get; set; } = string.Empty;
            public string EvidenceType { get; set; } = string.Empty;
            public string EvidenceValue { get; set; } = string.Empty;
            public double Confidence { get; set; }
            public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class ParserRunEntity
        {
            public string RunId { get; set; } = string.Empty;
    
    
            public string JournalFileId { get; set; } = string.Empty;
    
    
            public string ParserName { get; set; } = string.Empty;
    
    
            public string Vendor { get; set; } = string.Empty;
    
    
            public int TotalLines { get; set; }
    
    
            public int ParsedLines { get; set; }
    
    
            public int TransactionsFound { get; set; }
    
    
            public int SuccessCount { get; set; }
    
    
            public int FailedCount { get; set; }
    
    
            public int SuspiciousCount { get; set; }
    
    
            public string Status { get; set; } = "Running";
    
    
            public string? ErrorMessage { get; set; }
    
    
            public DateTime StartedUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? CompletedUtc { get; set; }
    
    
        }
    // ===== Parser Run =====
    
        /// <summary>Parser execution run.</summary>
        public sealed class ParserRunEntity
        {
            public string RunId { get; set; } = string.Empty;
            public string JournalFileId { get; set; } = string.Empty;
            public string ParserName { get; set; } = string.Empty;
            public string Vendor { get; set; } = string.Empty;
            public int TotalLines { get; set; }
            public int ParsedLines { get; set; }
            public int TransactionsFound { get; set; }
            public int SuccessCount { get; set; }
            public int FailedCount { get; set; }
            public int SuspiciousCount { get; set; }
            public string Status { get; set; } = "Running";
            public string? ErrorMessage { get; set; }
            public DateTime StartedUtc { get; set; } = DateTime.UtcNow;
            public DateTime? CompletedUtc { get; set; }
        }
    public partial class ReportSnapshotEntity
        {
            public string ReportId { get; set; } = string.Empty;
    
    
            public string ReportType { get; set; } = string.Empty;
    
    
            public string ReportName { get; set; } = string.Empty;
    
    
            public string? ReportJson { get; set; }
    
    
            public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
    
    
            public string GeneratedBy { get; set; } = string.Empty;
    
    
            public string Status { get; set; } = "Generated";
    
    
        }
    // ===== Report Snapshot =====
    
        /// <summary>Saved report results and KPIs.</summary>
        public sealed class ReportSnapshotEntity
        {
            public string ReportId { get; set; } = string.Empty;
            public string ReportType { get; set; } = string.Empty;
            public string ReportName { get; set; } = string.Empty;
            public string? ReportJson { get; set; }
            public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
            public string GeneratedBy { get; set; } = string.Empty;
            public string Status { get; set; } = "Generated";
        }
    public partial class RoleEntity
        {
            public string RoleId { get; set; } = string.Empty;
    
    
            public string RoleName { get; set; } = string.Empty;
    
    
            public int PrivilegeLevel { get; set; }
    
    
            public string? Description { get; set; }
    
    
            public string? Permissions { get; set; }
    
    
        }
    /// <summary>RBAC role definition.</summary>
        public sealed class RoleEntity
        {
            public string RoleId { get; set; } = string.Empty;
            public string RoleName { get; set; } = string.Empty;
            public int PrivilegeLevel { get; set; }
            public string? Description { get; set; }
            public string? Permissions { get; set; }
        }
    public partial class SchemaVersionEntity
        {
            public int VersionId { get; set; }
    
    
            public string VersionName { get; set; } = string.Empty;
    
    
            public string? Description { get; set; }
    
    
            public DateTime AppliedUtc { get; set; } = DateTime.UtcNow;
    
    
            public string Status { get; set; } = "Applied";
    
    
        }
    // ===== Schema Version =====
    
        /// <summary>Database schema version tracker.</summary>
        public sealed class SchemaVersionEntity
        {
            public int VersionId { get; set; }
            public string VersionName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public DateTime AppliedUtc { get; set; } = DateTime.UtcNow;
            public string Status { get; set; } = "Applied";
        }
    public partial class SourceTruthRecordEntity
        {
            public string RecordId { get; set; } = string.Empty;
    
    
            public string FilePath { get; set; } = string.Empty;
    
    
            public string Classification { get; set; } = string.Empty;
    
    
            public string Action { get; set; } = string.Empty;
    
    
            public string? Reason { get; set; }
    
    
            public string? ReplacementPath { get; set; }
    
    
            public DateTime ReviewedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    public partial class TransactionEvidenceEntity
        {
            public string EvidenceId { get; set; } = string.Empty;
    
    
            public string TransactionId { get; set; } = string.Empty;
    
    
            public string EvidenceType { get; set; } = "Journal";
    
    
            public string SourceFile { get; set; } = string.Empty;
    
    
            public int LineNumber { get; set; }
    
    
            public string RawLine { get; set; } = string.Empty;
    
    
            public string EvidenceCategory { get; set; } = string.Empty;
    
    
            public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== Transaction Evidence =====
    
        /// <summary>Raw evidence lines and sources for each transaction judgment.</summary>
        public sealed class TransactionEvidenceEntity
        {
            public string EvidenceId { get; set; } = string.Empty;
            public string TransactionId { get; set; } = string.Empty;
            public string EvidenceType { get; set; } = "Journal";
            public string SourceFile { get; set; } = string.Empty;
            public int LineNumber { get; set; }
            public string RawLine { get; set; } = string.Empty;
            public string EvidenceCategory { get; set; } = string.Empty;
            public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
        }
    public partial class UserEntity
        {
            public string UserId { get; set; } = string.Empty;
    
    
            public string Username { get; set; } = string.Empty;
    
    
            public string DisplayName { get; set; } = string.Empty;
    
    
            public string Role { get; set; } = "Viewer";
    
    
            public bool IsActive { get; set; } = true;
    
    
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime LastLoginUtc { get; set; } = DateTime.UtcNow;
    
    
            public string? PasswordHash { get; set; }
    
    
            public string? AllowedAtmGroups { get; set; }
    
    
            public string? AllowedModules { get; set; }
    
    
        }
    // ===== User / Role / Permission =====
    
        /// <summary>RBAC user entity.</summary>
        public sealed class UserEntity
        {
            public string UserId { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Role { get; set; } = "Viewer";
            public bool IsActive { get; set; } = true;
            public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
            public DateTime LastLoginUtc { get; set; } = DateTime.UtcNow;
            public string? PasswordHash { get; set; }
            public string? AllowedAtmGroups { get; set; }
            public string? AllowedModules { get; set; }
        }
    public partial class XfsEventEntity
        {
            public string EventId { get; set; } = string.Empty;
    
    
            public string AtmId { get; set; } = string.Empty;
    
    
            public string Vendor { get; set; } = string.Empty;
    
    
            public string DeviceClass { get; set; } = string.Empty;
    
    
            public string Component { get; set; } = string.Empty;
    
    
            public string Code { get; set; } = string.Empty;
    
    
            public string Severity { get; set; } = "Info";
    
    
            public string Message { get; set; } = string.Empty;
    
    
            public string RawLine { get; set; } = string.Empty;
    
    
            public string SourceFile { get; set; } = string.Empty;
    
    
            public DateTime EventUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // ===== XFS Event =====
    
        /// <summary>Normalized XFS/vendor event.</summary>
        public sealed class XfsEventEntity
        {
            public string EventId { get; set; } = string.Empty;
            public string AtmId { get; set; } = string.Empty;
            public string Vendor { get; set; } = string.Empty;
            public string DeviceClass { get; set; } = string.Empty;
            public string Component { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public string Severity { get; set; } = "Info";
            public string Message { get; set; } = string.Empty;
            public string RawLine { get; set; } = string.Empty;
            public string SourceFile { get; set; } = string.Empty;
            public DateTime EventUtc { get; set; } = DateTime.UtcNow;
            public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
        }

    // Class: AtmDeviceEntity (from 3 sources)
        public sealed partial class AtmDeviceEntity
        {
            // --- Properties ---
                    public string AtmId { get; set; } = string.Empty;
    
                    public string AtmName { get; set; } = string.Empty;
    
                    public string Vendor { get; set; } = string.Empty;
    
                    public string Model { get; set; } = string.Empty;
    
                    public string TerminalId { get; set; } = string.Empty;
    
                    public string LUNO { get; set; } = string.Empty;
    
                    public string Branch { get; set; } = string.Empty;
    
                    public string Region { get; set; } = string.Empty;
    
                    public string Province { get; set; } = string.Empty;
    
                    public string City { get; set; } = string.Empty;
    
                    public string Geography { get; set; } = string.Empty;
    
                    public string IpAddress { get; set; } = string.Empty;
    
                    public int Port { get; set; }
    
                    public string OperationalMode { get; set; } = "Unknown";
    
                    public string ConnectionState { get; set; } = "Offline";
    
                    public string JournalSourcePath { get; set; } = string.Empty;
    
                    public string BackupPath { get; set; } = string.Empty;
    
                    public string ImageInboxPath { get; set; } = string.Empty;
    
                    public string ImageDestinationPath { get; set; } = string.Empty;
    
                    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: AuditLogEntity (from 3 sources)
        public sealed partial class AuditLogEntity
        {
            // --- Properties ---
                    public string LogId { get; set; } = string.Empty;
    
                    public string Action { get; set; } = string.Empty;
    
                    public string UserId { get; set; } = string.Empty;
    
                    public string UserRole { get; set; } = string.Empty;
    
                    public string TargetAtm { get; set; } = string.Empty;
    
                    public string CommandId { get; set; } = string.Empty;
    
                    public string Details { get; set; } = string.Empty;
    
                    public string Result { get; set; } = string.Empty;
    
                    public string RiskLevel { get; set; } = "Low";
    
                    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: CashDistributionEntity (from 3 sources)
        public sealed partial class CashDistributionEntity
        {
            // --- Properties ---
                    public string CashRecordId { get; set; } = string.Empty;
    
                    public string AtmId { get; set; } = string.Empty;
    
                    public string TransactionId { get; set; } = string.Empty;
    
                    public string CassetteId { get; set; } = string.Empty;
    
                    public int Denomination { get; set; }
    
                    public long DispensedCount { get; set; }
    
                    public long RejectedCount { get; set; }
    
                    public long RetractedCount { get; set; }
    
                    public long RemainingCount { get; set; }
    
                    public string Currency { get; set; } = string.Empty;
    
                    public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: ClientSessionEntity (from 3 sources)
        public sealed partial class ClientSessionEntity
        {
            // --- Properties ---
                    public string SessionId { get; set; } = string.Empty;
    
                    public string AtmId { get; set; } = string.Empty;
    
                    public string RemoteAddress { get; set; } = string.Empty;
    
                    public string ClientVersion { get; set; } = string.Empty;
    
                    public string ProtocolVersion { get; set; } = string.Empty;
    
                    public DateTime ConnectedUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime? DisconnectedUtc { get; set; }
    
                    public int PendingCommands { get; set; }
    
                    public long TotalBytesSent { get; set; }
    
                    public long TotalBytesReceived { get; set; }
    
                    public bool IsHealthy { get; set; } = true;
    
                    public string SessionState { get; set; } = "Active";
    
    
        }
    // Class: CommandAuditEntity (from 3 sources)
        public sealed partial class CommandAuditEntity
        {
            // --- Properties ---
                    public string AuditId { get; set; } = string.Empty;
    
                    public string CommandId { get; set; } = string.Empty;
    
                    public string Action { get; set; } = string.Empty;
    
                    public string UserId { get; set; } = string.Empty;
    
                    public string UserRole { get; set; } = string.Empty;
    
                    public string TargetAtm { get; set; } = string.Empty;
    
                    public string Details { get; set; } = string.Empty;
    
                    public string Result { get; set; } = string.Empty;
    
                    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: CommandQueueEntity (from 3 sources)
        public sealed partial class CommandQueueEntity
        {
            // --- Properties ---
                    public string CommandId { get; set; } = string.Empty;
    
                    public string CorrelationId { get; set; } = string.Empty;
    
                    public string CommandType { get; set; } = string.Empty;
    
                    public string TargetAtm { get; set; } = string.Empty;
    
                    public string OperatorId { get; set; } = string.Empty;
    
                    public string ApproverId { get; set; } = string.Empty;
    
                    public string RequiredRole { get; set; } = "Admin";
    
                    public string RiskLevel { get; set; } = "Low";
    
                    public string Payload { get; set; } = string.Empty;
    
                    public string Signature { get; set; } = string.Empty;
    
                    public string Nonce { get; set; } = string.Empty;
    
                    public string State { get; set; } = "Draft";
    
                    public string? FailureReason { get; set; }
    
                    public string? RollbackPlan { get; set; }
    
                    public bool RollbackExecuted { get; set; }
    
                    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime? ApprovedUtc { get; set; }
    
                    public DateTime ExpiryUtc { get; set; } = DateTime.UtcNow.AddMinutes(5);
    
                    public DateTime? CompletedUtc { get; set; }
    
                    public int RetryCount { get; set; }
    
                    public int MaxRetries { get; set; } = 3;
    
    
        }
    // Class: CorrelationResultEntity (from 3 sources)
        public sealed partial class CorrelationResultEntity
        {
            // --- Properties ---
                    public string CorrelationId { get; set; } = string.Empty;
    
                    public string TransactionId { get; set; } = string.Empty;
    
                    public string EventId { get; set; } = string.Empty;
    
                    public string MatchLevel { get; set; } = "None";
    
                    public string MatchBasis { get; set; } = string.Empty;
    
                    public double Confidence { get; set; }
    
                    public string RootCause { get; set; } = string.Empty;
    
                    public DateTime CorrelatedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: FileManifestEntity (from 3 sources)
        public sealed partial class FileManifestEntity
        {
            // --- Properties ---
                    public string ManifestId { get; set; } = string.Empty;
    
                    public string AtmId { get; set; } = string.Empty;
    
                    public string FileName { get; set; } = string.Empty;
    
                    public long FileSize { get; set; }
    
                    public string Checksum { get; set; } = string.Empty;
    
                    public string FileType { get; set; } = "Journal";
    
                    public string SourcePath { get; set; } = string.Empty;
    
                    public string ArchivePath { get; set; } = string.Empty;
    
                    public int TotalChunks { get; set; }
    
                    public int ChunkSize { get; set; }
    
                    public string Status { get; set; } = "Pending";
    
                    public string? FailureReason { get; set; }
    
                    public int RetryCount { get; set; }
    
                    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime LastModifiedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: HealthSnapshotEntity (from 3 sources)
        public sealed partial class HealthSnapshotEntity
        {
            // --- Properties ---
                    public string SnapshotId { get; set; } = string.Empty;
    
                    public string AtmId { get; set; } = string.Empty;
    
                    public string SnapshotType { get; set; } = "Operational";
    
                    public double HealthScore { get; set; }
    
                    public double ConfidenceScore { get; set; }
    
                    public string ConnectionState { get; set; } = "Unknown";
    
                    public string OperationalMode { get; set; } = "Unknown";
    
                    public string AgentServiceState { get; set; } = "Unknown";
    
                    public int ActiveAlarms { get; set; }
    
                    public string Priority { get; set; } = "None";
    
                    public string? SnapshotJson { get; set; }
    
                    public DateTime SnapshotUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: ImagePackageEntity (from 3 sources)
        public sealed partial class ImagePackageEntity
        {
            // --- Properties ---
                    public string PackageId { get; set; } = string.Empty;
    
                    public string PackageName { get; set; } = string.Empty;
    
                    public string TargetVendor { get; set; } = string.Empty;
    
                    public string TargetAtmGroup { get; set; } = string.Empty;
    
                    public string SourceFile { get; set; } = string.Empty;
    
                    public long FileSize { get; set; }
    
                    public string Checksum { get; set; } = string.Empty;
    
                    public string Status { get; set; } = "Pending";
    
                    public int TotalTargets { get; set; }
    
                    public int DeployedTargets { get; set; }
    
                    public int FailedTargets { get; set; }
    
                    public string? FailureReason { get; set; }
    
                    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime? DeployedUtc { get; set; }
    
    
        }
    // Class: JournalFileEntity (from 3 sources)
        public sealed partial class JournalFileEntity
        {
            // --- Properties ---
                    public string JournalFileId { get; set; } = string.Empty;
    
                    public string AtmId { get; set; } = string.Empty;
    
                    public string FileName { get; set; } = string.Empty;
    
                    public long FileSize { get; set; }
    
                    public string Checksum { get; set; } = string.Empty;
    
                    public string ArchivePath { get; set; } = string.Empty;
    
                    public int TotalLines { get; set; }
    
                    public int ParsedLines { get; set; }
    
                    public int TransactionCount { get; set; }
    
                    public string ParseStatus { get; set; } = "Pending";
    
                    public string? ParseError { get; set; }
    
                    public string ParserUsed { get; set; } = string.Empty;
    
                    public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime? ParsedUtc { get; set; }
    
    
        }
    // Class: JournalTransactionEntity (from 3 sources)
        public sealed partial class JournalTransactionEntity
        {
            // --- Properties ---
                    public string TransactionId { get; set; } = string.Empty;
    
                    public string AtmId { get; set; } = string.Empty;
    
                    public string JournalFileId { get; set; } = string.Empty;
    
                    public int StartLine { get; set; }
    
                    public int EndLine { get; set; }
    
                    public string CardMasked { get; set; } = string.Empty;
    
                    public string AccountMasked { get; set; } = string.Empty;
    
                    public decimal? Amount { get; set; }
    
                    public string Currency { get; set; } = string.Empty;
    
                    public string STAN { get; set; } = string.Empty;
    
                    public string RRN { get; set; } = string.Empty;
    
                    public int? Cassette1 { get; set; }
    
                    public int? Cassette2 { get; set; }
    
                    public int? Cassette3 { get; set; }
    
                    public int? Cassette4 { get; set; }
    
                    public string MCode { get; set; } = string.Empty;
    
                    public string RCode { get; set; } = string.Empty;
    
                    public string HostResponse { get; set; } = string.Empty;
    
                    public string Classification { get; set; } = string.Empty;
    
                    public double Confidence { get; set; }
    
                    public string FinalStatus { get; set; } = string.Empty;
    
                    public DateTime? TransactionDate { get; set; }
    
                    public DateTime ParsedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: ParserEvidenceEntity (from 3 sources)
        public sealed partial class ParserEvidenceEntity
        {
            // --- Properties ---
                    public string EvidenceId { get; set; } = string.Empty;
    
                    public string RunId { get; set; } = string.Empty;
    
                    public string TransactionId { get; set; } = string.Empty;
    
                    public string EvidenceType { get; set; } = string.Empty;
    
                    public string EvidenceValue { get; set; } = string.Empty;
    
                    public double Confidence { get; set; }
    
                    public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: ParserRunEntity (from 3 sources)
        public sealed partial class ParserRunEntity
        {
            // --- Properties ---
                    public string RunId { get; set; } = string.Empty;
    
                    public string JournalFileId { get; set; } = string.Empty;
    
                    public string ParserName { get; set; } = string.Empty;
    
                    public string Vendor { get; set; } = string.Empty;
    
                    public int TotalLines { get; set; }
    
                    public int ParsedLines { get; set; }
    
                    public int TransactionsFound { get; set; }
    
                    public int SuccessCount { get; set; }
    
                    public int FailedCount { get; set; }
    
                    public int SuspiciousCount { get; set; }
    
                    public string Status { get; set; } = "Running";
    
                    public string? ErrorMessage { get; set; }
    
                    public DateTime StartedUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime? CompletedUtc { get; set; }
    
    
        }
    // Class: ReportSnapshotEntity (from 3 sources)
        public sealed partial class ReportSnapshotEntity
        {
            // --- Properties ---
                    public string ReportId { get; set; } = string.Empty;
    
                    public string ReportType { get; set; } = string.Empty;
    
                    public string ReportName { get; set; } = string.Empty;
    
                    public string? ReportJson { get; set; }
    
                    public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
    
                    public string GeneratedBy { get; set; } = string.Empty;
    
                    public string Status { get; set; } = "Generated";
    
    
        }
    // Class: RoleEntity (from 3 sources)
        public sealed partial class RoleEntity
        {
            // --- Properties ---
                    public string RoleId { get; set; } = string.Empty;
    
                    public string RoleName { get; set; } = string.Empty;
    
                    public int PrivilegeLevel { get; set; }
    
                    public string? Description { get; set; }
    
                    public string? Permissions { get; set; }
    
    
        }
    // Class: SchemaVersionEntity (from 3 sources)
        public sealed partial class SchemaVersionEntity
        {
            // --- Properties ---
                    public int VersionId { get; set; }
    
                    public string VersionName { get; set; } = string.Empty;
    
                    public string? Description { get; set; }
    
                    public DateTime AppliedUtc { get; set; } = DateTime.UtcNow;
    
                    public string Status { get; set; } = "Applied";
    
    
        }
    // Class: SourceTruthRecordEntity (from 3 sources)
        public sealed partial class SourceTruthRecordEntity
        {
            // --- Properties ---
                    public string RecordId { get; set; } = string.Empty;
    
                    public string FilePath { get; set; } = string.Empty;
    
                    public string Classification { get; set; } = string.Empty;
    
                    public string Action { get; set; } = string.Empty;
    
                    public string? Reason { get; set; }
    
                    public string? ReplacementPath { get; set; }
    
                    public DateTime ReviewedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: TransactionEvidenceEntity (from 3 sources)
        public sealed partial class TransactionEvidenceEntity
        {
            // --- Properties ---
                    public string EvidenceId { get; set; } = string.Empty;
    
                    public string TransactionId { get; set; } = string.Empty;
    
                    public string EvidenceType { get; set; } = "Journal";
    
                    public string SourceFile { get; set; } = string.Empty;
    
                    public int LineNumber { get; set; }
    
                    public string RawLine { get; set; } = string.Empty;
    
                    public string EvidenceCategory { get; set; } = string.Empty;
    
                    public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
    // Class: UserEntity (from 3 sources)
        public sealed partial class UserEntity
        {
            // --- Properties ---
                    public string UserId { get; set; } = string.Empty;
    
                    public string Username { get; set; } = string.Empty;
    
                    public string DisplayName { get; set; } = string.Empty;
    
                    public string Role { get; set; } = "Viewer";
    
                    public bool IsActive { get; set; } = true;
    
                    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime LastLoginUtc { get; set; } = DateTime.UtcNow;
    
                    public string? PasswordHash { get; set; }
    
                    public string? AllowedAtmGroups { get; set; }
    
                    public string? AllowedModules { get; set; }
    
    
        }
    // Class: XfsEventEntity (from 3 sources)
        public sealed partial class XfsEventEntity
        {
            // --- Properties ---
                    public string EventId { get; set; } = string.Empty;
    
                    public string AtmId { get; set; } = string.Empty;
    
                    public string Vendor { get; set; } = string.Empty;
    
                    public string DeviceClass { get; set; } = string.Empty;
    
                    public string Component { get; set; } = string.Empty;
    
                    public string Code { get; set; } = string.Empty;
    
                    public string Severity { get; set; } = "Info";
    
                    public string Message { get; set; } = string.Empty;
    
                    public string RawLine { get; set; } = string.Empty;
    
                    public string SourceFile { get; set; } = string.Empty;
    
                    public DateTime EventUtc { get; set; } = DateTime.UtcNow;
    
                    public DateTime CapturedUtc { get; set; } = DateTime.UtcNow;
    
    
        }
}
