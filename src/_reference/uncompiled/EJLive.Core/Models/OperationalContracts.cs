using System;
using System.Collections.Generic;

namespace EJLive.Core.Models
{
    public enum OperationalRecordState
    {
        Draft = 0,
        Active = 1,
        Pending = 2,
        InProgress = 3,
        Completed = 4,
        Failed = 5,
        Cancelled = 6
    }

    public enum UploadHealthState
    {
        Unknown = 0,
        Queued = 1,
        Uploading = 2,
        Uploaded = 3,
        Acked = 4,
        Offline = 10,
        NotUpload = 11,
        WrongPath = 12,
        Duplicate = 13,
        Incomplete = 14,
        IntegrityFailure = 15,
        Failed = 16
    }

    public enum FaultEventSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2,
        Emergency = 3
    }

    public enum TicketState
    {
        Open = 0,
        Pending = 1,
        Escalated = 2,
        Resolved = 3,
        Closed = 4,
        Rejected = 5
    }

    public enum RemoteTaskState
    {
        Created = 0,
        Assigned = 1,
        Sent = 2,
        Received = 3,
        Executing = 4,
        Executed = 5,
        Failed = 6,
        Timeout = 7
    }

    public sealed class TerminalBindingRecord
    {
        public string TerminalId { get; set; }
        public string TerminalName { get; set; }
        public string OrganizationPath { get; set; }
        public string Region { get; set; }
        public string BranchCode { get; set; }
        public string Vendor { get; set; }
        public string Model { get; set; }
        public string IpAddress { get; set; }
        public string DomainName { get; set; }
        public string AgentId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastValidationUtc { get; set; }
        public OperationalRecordState State { get; set; } = OperationalRecordState.Draft;
        public string ValidationMessage { get; set; }
    }

    public sealed class AgentConfigurationRecord
    {
        public string AgentId { get; set; }
        public string TerminalId { get; set; }
        public string ConfigPath { get; set; }
        public string ServerHost { get; set; }
        public int ServerPort { get; set; }
        public string SourcePath { get; set; }
        public string BackupPath { get; set; }
        public bool DurableSyncEnabled { get; set; } = true;
        public bool AckRequired { get; set; } = true;
        public bool DedupEnabled { get; set; } = true;
        public DateTime LoadedAtUtc { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class VendorModelMappingRecord
    {
        public string Vendor { get; set; }
        public string Model { get; set; }
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public string XfsServiceName { get; set; }
        public string MappingPath { get; set; }
        public string StatusDecoder { get; set; }
        public string Notes { get; set; }
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class UploadLogRecord
    {
        public string UploadId { get; set; } = Guid.NewGuid().ToString("N");
        public string TerminalId { get; set; }
        public string FileName { get; set; }
        public string FileKind { get; set; }
        public string SourcePath { get; set; }
        public string ServerPath { get; set; }
        public long BytesExpected { get; set; }
        public long BytesReceived { get; set; }
        public string Checksum { get; set; }
        public string AckId { get; set; }
        public UploadHealthState State { get; set; } = UploadHealthState.Queued;
        public string FailureReason { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class FaultEventRecord
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString("N");
        public string TerminalId { get; set; }
        public string Vendor { get; set; }
        public string DeviceCode { get; set; }
        public string EventCode { get; set; }
        public string EventName { get; set; }
        public FaultEventSeverity Severity { get; set; }
        public string RawLine { get; set; }
        public string RecommendedAction { get; set; }
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
        public string CorrelationId { get; set; }
    }

    public sealed class RemoteTaskRecord
    {
        public string TaskId { get; set; } = Guid.NewGuid().ToString("N");
        public string TerminalId { get; set; }
        public string CommandType { get; set; }
        public string Parameters { get; set; }
        public string AssignedBy { get; set; }
        public RemoteTaskState State { get; set; } = RemoteTaskState.Created;
        public string Result { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class TicketRecord
    {
        public string TicketId { get; set; } = Guid.NewGuid().ToString("N");
        public string TerminalId { get; set; }
        public string EventId { get; set; }
        public string Title { get; set; }
        public string AssignedTo { get; set; }
        public TicketState State { get; set; } = TicketState.Open;
        public FaultEventSeverity Severity { get; set; } = FaultEventSeverity.Warning;
        public string EscalationLevel { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class ArchiveIndexRecord
    {
        public string ArchiveId { get; set; } = Guid.NewGuid().ToString("N");
        public string TerminalId { get; set; }
        public string MonthKey { get; set; }
        public string ArchivePath { get; set; }
        public long FileCount { get; set; }
        public long TotalBytes { get; set; }
        public string IntegrityHash { get; set; }
        public DateTime IndexedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public sealed class CashReplenishmentSnapshot
    {
        public string TerminalId { get; set; }
        public int Cassette1Remaining { get; set; }
        public int Cassette2Remaining { get; set; }
        public int Cassette3Remaining { get; set; }
        public int Cassette4Remaining { get; set; }
        public long LoadedAmount { get; set; }
        public long DispenseOutAmount { get; set; }
        public long RejectAmount { get; set; }
        public long RetractAmount { get; set; }
        public int ThresholdPercent { get; set; }
        public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
