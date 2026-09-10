using System;
using System.Collections.Generic;

namespace EJLive.Core.Client
{
    /// <summary>Sync status snapshot for the Companion UI Sync tab.</summary>
    public sealed class ClientSyncSnapshot
    {
        public int PendingCount { get; set; }
        public int SyncingCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public long TotalBytesPending { get; set; }
        public double SuccessRate { get; set; }
        public List<SyncFileEntry> Files { get; set; } = new List<SyncFileEntry>();
    }

    public sealed class SyncFileEntry
    {
        public string ItemId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string Status { get; set; } = "Pending";
        public int ProgressPercent { get; set; }
        public int RetryCount { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public long BytesSent { get; set; }
        public DateTimeOffset AddedTime { get; set; }
        public DateTimeOffset? LastAttempt { get; set; }
        public string FailureReason { get; set; } = string.Empty;
    }

    /// <summary>Path configuration snapshot for the Vendor Paths tab.</summary>
    public sealed class ClientPathSnapshot
    {
        public string Vendor { get; set; } = string.Empty;
        public List<string> JournalSourcePaths { get; set; } = new List<string>();
        public List<string> JournalBackupPaths { get; set; } = new List<string>();
        public List<string> TraceLogPaths { get; set; } = new List<string>();
        public string ImageInboxPath { get; set; } = string.Empty;
        public List<string> ImageDestinationPaths { get; set; } = new List<string>();
        public string ScreenshotCachePath { get; set; } = string.Empty;
        public List<string> SupportedExtensions { get; set; } = new List<string>();
        public string RolloverBehavior { get; set; } = string.Empty;
        public bool RequiresRestartForContentDeploy { get; set; }
        public List<string> WritePermissionNotes { get; set; } = new List<string>();
    }

    /// <summary>Command/request status snapshot for the Controlled Requests tab.</summary>
    public sealed class ClientCommandSnapshot
    {
        public string CommandId { get; set; } = string.Empty;
        public string CommandType { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft";
        public DateTimeOffset QueuedAt { get; set; }
        public DateTimeOffset? SentAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public string Result { get; set; } = string.Empty;
        public string FailureReason { get; set; } = string.Empty;
    }

    /// <summary>Image sync status snapshot for the Image Sync tab.</summary>
    public sealed class ClientImageSyncSnapshot
    {
        public string Vendor { get; set; } = string.Empty;
        public string AtmType { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string ServerSource { get; set; } = string.Empty;
        public string ClientInboxPath { get; set; } = string.Empty;
        public string AtmDestinationPath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public DateTimeOffset? ReceivedTime { get; set; }
        public DateTimeOffset? PromotedTime { get; set; }
        public string Status { get; set; } = "Idle";
        public string FailureReason { get; set; } = string.Empty;
        public string ReceiptStatus { get; set; } = string.Empty;
    }

    /// <summary>Health score and recommendations for the Services / Control tab.</summary>
    public sealed class ClientHealthScore
    {
        public string OverallHealth { get; set; } = "Unknown";
        public int ScorePercent { get; set; }
        public List<string> ActiveChecks { get; set; } = new List<string>();
        public List<string> FailedChecks { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    /// <summary>Complete client companion view model aggregating all tab data.</summary>
    public sealed class ClientCompanionViewModel
    {
        public ClientRuntimeSnapshot Runtime { get; set; } = new ClientRuntimeSnapshot();
        public ClientSyncSnapshot Sync { get; set; } = new ClientSyncSnapshot();
        public ClientPathSnapshot Paths { get; set; } = new ClientPathSnapshot();
        public ClientHealthScore Health { get; set; } = new ClientHealthScore();
        public List<ClientCommandSnapshot> Commands { get; set; } = new List<ClientCommandSnapshot>();
        public List<ClientImageSyncSnapshot> ImageSync { get; set; } = new List<ClientImageSyncSnapshot>();
        public List<ClientComponentSnapshot> ServiceComponents { get; set; } = new List<ClientComponentSnapshot>();
        public Dictionary<string, string> AgentConfig { get; set; } = new Dictionary<string, string>();
        public List<string> LogEntries { get; set; } = new List<string>();
        public DateTimeOffset LastRefreshUtc { get; set; } = DateTimeOffset.UtcNow;
    }
}