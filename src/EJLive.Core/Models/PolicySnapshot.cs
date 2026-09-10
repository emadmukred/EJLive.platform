using System;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Captures the state of a policy before and after a change, including a rollback plan.
    /// </summary>
    public sealed record PolicySnapshot
    {
        /// <summary>Unique identifier for this snapshot.</summary>
        public required string SnapshotId { get; init; }

        /// <summary>The type of policy being changed (e.g., Windows, RDP, Firewall, WinRM).</summary>
        public required string PolicyType { get; init; }

        /// <summary>Serialized JSON representing the policy state before the change.</summary>
        public required string BeforeJson { get; init; }

        /// <summary>Serialized JSON representing the policy state after the change.</summary>
        public required string AfterJson { get; init; }

        /// <summary>Serialized JSON representing the rollback plan to restore the previous state.</summary>
        public required string RollbackJson { get; init; }

        /// <summary>UTC timestamp when the snapshot was captured.</summary>
        public required DateTime TimestampUtc { get; init; }

        /// <summary>Identity of the operator who initiated the policy change.</summary>
        public required string OperatorId { get; init; }

        /// <summary>Human-readable reason for the policy change.</summary>
        public required string Reason { get; init; }
    }
}
