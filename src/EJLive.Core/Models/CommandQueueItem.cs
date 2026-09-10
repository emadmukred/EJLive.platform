using System;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Lifecycle states for a queued remote command.
    /// </summary>
    public enum CommandQueueStatus
    {
        /// <summary>Command is being prepared and has not yet been reviewed.</summary>
        Draft,

        /// <summary>Command has been approved by an authorized operator.</summary>
        Approved,

        /// <summary>Command has been transmitted to the target endpoint.</summary>
        Sent,

        /// <summary>Acknowledgement received from the target endpoint.</summary>
        Ack,

        /// <summary>Command completed successfully.</summary>
        Completed,

        /// <summary>Command failed during execution.</summary>
        Failed,

        /// <summary>Command expired before it could be executed.</summary>
        Expired
    }

    /// <summary>
    /// Represents a tracked remote command entry in the command queue.
    /// </summary>
    public sealed record CommandQueueItem
    {
        /// <summary>Primary key for the queue item.</summary>
        public required string Id { get; init; }

        /// <summary>Reference to the originating command envelope.</summary>
        public required string CommandId { get; init; }

        /// <summary>Current lifecycle status of the command.</summary>
        public required CommandQueueStatus Status { get; init; }

        /// <summary>UTC timestamp when the queue item was created.</summary>
        public required DateTime CreatedUtc { get; init; }

        /// <summary>UTC timestamp when the command was sent to the endpoint.</summary>
        public DateTime? SentUtc { get; init; }

        /// <summary>UTC timestamp when the command finished (success or failure).</summary>
        public DateTime? CompletedUtc { get; init; }

        /// <summary>Serialized JSON result returned by the endpoint.</summary>
        public string? ResultJson { get; init; }

        /// <summary>Error message or stack trace if the command failed.</summary>
        public string? Error { get; init; }
    }
}
