using System;
using System.Collections.Generic;

namespace EJLive.Core.Communication
{
    /// <summary>
        /// Unified message types used across the system.
        /// Bridges legacy EJMessage references in Client.WinForms.
        /// </summary>
        public sealed class EJMessage
        {
            public string Type { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public string? SenderId { get; set; }
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
            public Dictionary<string, string> Headers { get; set; } = new();
        }
    public partial class EJMessage
        {
            public string Type { get; set; } = string.Empty;
    
    
            public string Text { get; set; } = string.Empty;
    
    
            public string? SenderId { get; set; }
    
    
            public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
    
            public Dictionary<string, string> Headers { get; set; } = new();
    
    
        }
    public partial public public sealed class EJMessage
        {
            public string Type { get; set; }
            public string Text { get; set; }
            public string? SenderId { get; set; }
            public DateTime TimestampUtc { get; set; }
            public Dictionary<string, string> Headers { get; set; }
        }
    public partial public sealed class EJMessage
        {
            public string Type { get; set; }
            public string Text { get; set; }
            public string? SenderId { get; set; }
            public DateTime TimestampUtc { get; set; }
            public Dictionary<string, string> Headers { get; set; }
        }
    /// <summary>Legacy journal outbox entry.</summary>
        public sealed class JournalOutboxItem
        {
            public string ItemId { get; set; } = Guid.NewGuid().ToString("N");
            public string ATM_ID { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string FilePath { get; set; } = string.Empty;
            public long FileSize { get; set; }
            public string Checksum { get; set; } = string.Empty;
            public string Status { get; set; } = "Pending";
            public int RetryCount { get; set; }
            public string? FailureReason { get; set; }
            public DateTime QueuedUtc { get; set; } = DateTime.UtcNow;
            public DateTime? SentUtc { get; set; }
        }
    public partial class JournalOutboxItem
        {
            public string ItemId { get; set; } = Guid.NewGuid().ToString("N");
    
    
            public string ATM_ID { get; set; } = string.Empty;
    
    
            public string FileName { get; set; } = string.Empty;
    
    
            public string FilePath { get; set; } = string.Empty;
    
    
            public long FileSize { get; set; }
    
    
            public string Checksum { get; set; } = string.Empty;
    
    
            public string Status { get; set; } = "Pending";
    
    
            public int RetryCount { get; set; }
    
    
            public string? FailureReason { get; set; }
    
    
            public DateTime QueuedUtc { get; set; } = DateTime.UtcNow;
    
    
            public DateTime? SentUtc { get; set; }
    
    
        }
    public partial public public sealed class JournalOutboxItem
        {
            public string ItemId { get; set; }
            public string ATM_ID { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public long FileSize { get; set; }
            public string Checksum { get; set; }
            public string Status { get; set; }
            public int RetryCount { get; set; }
            public string? FailureReason { get; set; }
            public DateTime QueuedUtc { get; set; }
            public DateTime? SentUtc { get; set; }
        }
    public partial public sealed class JournalOutboxItem
        {
            public string ItemId { get; set; }
            public string ATM_ID { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public long FileSize { get; set; }
            public string Checksum { get; set; }
            public string Status { get; set; }
            public int RetryCount { get; set; }
            public string? FailureReason { get; set; }
            public DateTime QueuedUtc { get; set; }
            public DateTime? SentUtc { get; set; }
        }
    public partial class JournalSyncServiceStub
        {
            public bool IsRunning { get; set; }
    
    
            public string LastError { get; set; } = string.Empty;
    
    
            public int PendingCount { get; set; }
    
    
        }
    public partial public public sealed class JournalSyncServiceStub
        {
            public bool IsRunning { get; set; }
            public string LastError { get; set; }
            public int PendingCount { get; set; }
        }
    public partial public sealed class JournalSyncServiceStub
        {
            public bool IsRunning { get; set; }
            public string LastError { get; set; }
            public int PendingCount { get; set; }
        }
    /// <summary>Legacy journal sync state service stub.</summary>
        public sealed class JournalSyncStateServiceStub
        {
            public string CurrentState { get; set; } = "Idle";
            public DateTime? LastSyncUtc { get; set; }
        }
    public partial class JournalSyncStateServiceStub
        {
            public string CurrentState { get; set; } = "Idle";
    
    
            public DateTime? LastSyncUtc { get; set; }
    
    
        }
    public partial public public sealed class JournalSyncStateServiceStub
        {
            public string CurrentState { get; set; }
            public DateTime? LastSyncUtc { get; set; }
        }
    public partial public sealed class JournalSyncStateServiceStub
        {
            public string CurrentState { get; set; }
            public DateTime? LastSyncUtc { get; set; }
        }

    // Class: EJMessage (from 3 sources)
        public sealed partial class EJMessage
        {
            // --- Properties ---
                public string Type { get; set; } = string.Empty;
    
                public string Text { get; set; } = string.Empty;
    
                public string? SenderId { get; set; }
    
                public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    
                public Dictionary<string, string> Headers { get; set; } = new();
    
    
        }
    // Class: JournalOutboxItem (from 3 sources)
        public sealed partial class JournalOutboxItem
        {
            // --- Properties ---
                public string ItemId { get; set; } = Guid.NewGuid().ToString("N");
    
                public string ATM_ID { get; set; } = string.Empty;
    
                public string FileName { get; set; } = string.Empty;
    
                public string FilePath { get; set; } = string.Empty;
    
                public long FileSize { get; set; }
    
                public string Checksum { get; set; } = string.Empty;
    
                public string Status { get; set; } = "Pending";
    
                public int RetryCount { get; set; }
    
                public string? FailureReason { get; set; }
    
                public DateTime QueuedUtc { get; set; } = DateTime.UtcNow;
    
                public DateTime? SentUtc { get; set; }
    
    
        }
    /// <summary>Legacy journal sync service stub (forward to Core.Journal).</summary>
        public sealed class JournalSyncServiceStub
        {
            public bool IsRunning { get; set; }
            public string LastError { get; set; } = string.Empty;
            public int PendingCount { get; set; }
        }
    // Class: JournalSyncServiceStub (from 3 sources)
        public sealed partial class JournalSyncServiceStub
        {
            // --- Properties ---
                public bool IsRunning { get; set; }
    
                public string LastError { get; set; } = string.Empty;
    
                public int PendingCount { get; set; }
    
    
        }
    // Class: JournalSyncStateServiceStub (from 3 sources)
        public sealed partial class JournalSyncStateServiceStub
        {
            // --- Properties ---
                public string CurrentState { get; set; } = "Idle";
    
                public DateTime? LastSyncUtc { get; set; }
    
    
        }
}
