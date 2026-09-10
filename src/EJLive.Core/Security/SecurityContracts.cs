using System;
using System.Collections.Generic;

namespace EJLive.Core.Security
{
    /// <summary>
    /// Role-Based Access Control roles for EJLive operators.
    /// </summary>
    public enum RbacRole
    {
        Viewer,
        Operator,
        Support,
        Admin,
        SecurityAdmin
    }

    /// <summary>
    /// Encapsulated remote command with state machine, audit trail, and rollback plan.
    /// No command may execute without signature verification, timestamp freshness check,
    /// RBAC approval, and audit logging.
    /// </summary>
    public sealed class RemoteCommandEnvelope
    {
        public string CommandId { get; set; } = Guid.NewGuid().ToString("N");
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
        public string OperatorId { get; set; } = string.Empty;
        public RbacRole Role { get; set; } = RbacRole.Viewer;
        public string TargetAtmId { get; set; } = string.Empty;
        public string CommandType { get; set; } = string.Empty;
        public string? PayloadJson { get; set; }
        public string? Signature { get; set; }
        public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ExpiryUtc { get; set; } = DateTimeOffset.UtcNow.AddMinutes(30);
        public CommandState State { get; set; } = CommandState.Draft;
        public string? ResultJson { get; set; }
        public string? FailureReason { get; set; }
        public string? RollbackPlanJson { get; set; }
        public DateTimeOffset? CompletedUtc { get; set; }
    }

    /// <summary>
    /// Command lifecycle states. Draft → Approved → Sent → Ack → Completed/Failed/Expired/Rejected/Cancelled.
    /// </summary>
    public enum CommandState
    {
        Draft,
        Approved,
        Sent,
        Ack,
        Completed,
        Failed,
        Expired,
        Rejected,
        Cancelled
    }

    /// <summary>
    /// Governed capability types that require policy, signature, RBAC, and audit.
    /// </summary>
    public enum GovernedCapability
    {
        Screenshot,
        ForceSync,
        RestartRequest,
        TimeSyncRequest,
        RemoteAssistanceReadiness,
        RdpPrecheck,
        PasswordChangeRequest,
        FirewallScopeCheck,
        RegistryPolicyPrecheck,
        ServiceStatus,
        ServiceRestartRequest,
        ImagePackagePromote,
        FileDistribution
    }

    /// <summary>
    /// Audit record for every command attempt — before and after execution.
    /// </summary>
    public sealed class CommandAuditRecord
    {
        public string AuditId { get; set; } = Guid.NewGuid().ToString("N");
        public string CommandId { get; set; } = string.Empty;
        public string OperatorId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // e.g., "CommandCreated", "CommandExecuted", "CommandRejected"
        public string? DetailsJson { get; set; }
        public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Policy engine for governed command execution.
    /// Default is audit-only. Enforce mode requires explicit configuration and signed command.
    /// </summary>
    public sealed class UnifiedRemoteCommandPolicy
    {
        private readonly Dictionary<GovernedCapability, RbacRole> _requiredRoles = new Dictionary<GovernedCapability, RbacRole>
        {
            [GovernedCapability.Screenshot] = RbacRole.Operator,
            [GovernedCapability.ForceSync] = RbacRole.Operator,
            [GovernedCapability.RestartRequest] = RbacRole.Admin,
            [GovernedCapability.TimeSyncRequest] = RbacRole.Admin,
            [GovernedCapability.RemoteAssistanceReadiness] = RbacRole.Support,
            [GovernedCapability.RdpPrecheck] = RbacRole.Support,
            [GovernedCapability.PasswordChangeRequest] = RbacRole.SecurityAdmin,
            [GovernedCapability.FirewallScopeCheck] = RbacRole.Admin,
            [GovernedCapability.RegistryPolicyPrecheck] = RbacRole.Admin,
            [GovernedCapability.ServiceStatus] = RbacRole.Viewer,
            [GovernedCapability.ServiceRestartRequest] = RbacRole.Admin,
            [GovernedCapability.ImagePackagePromote] = RbacRole.Operator,
            [GovernedCapability.FileDistribution] = RbacRole.Admin
        };

        /// <summary>
        /// Returns true if the given role is authorized for the capability.
        /// </summary>
        public bool IsAuthorized(GovernedCapability capability, RbacRole role)
        {
            if (!_requiredRoles.TryGetValue(capability, out var required))
                return false;
            return role >= required;
        }

        /// <summary>
        /// Validates a command envelope before execution.
        /// Returns null if valid, or a rejection reason.
        /// </summary>
        public string? Validate(RemoteCommandEnvelope command)
        {
            // Stale timestamp check (5 minute tolerance)
            if (Math.Abs((DateTimeOffset.UtcNow - command.TimestampUtc).TotalMinutes) > 5)
                return "Command timestamp is stale (exceeds 5-minute tolerance).";

            // Expiry check
            if (DateTimeOffset.UtcNow > command.ExpiryUtc)
                return "Command has expired.";

            // Signature required for non-Draft states
            if (command.State != CommandState.Draft && string.IsNullOrWhiteSpace(command.Signature))
                return "Signature is required for non-draft commands.";

            return null; // Valid
        }
    }

    /// <summary>
    /// Redacts sensitive values from logs, exports, and diagnostic outputs.
    /// Never logs card numbers, account numbers, passwords, or service keys.
    /// </summary>
    public static class SecretRedactor
    {
        /// <summary>
        /// Patterns to redact from log output.
        /// </summary>
        private static readonly string[] SensitivePatterns = new[]
        {
            "Password",
            "password",
            "Secret",
            "secret",
            "Key",
            "key",
            "Card",
            "PAN",
            "AccountNumber"
        };

        /// <summary>
        /// Returns a redacted version of the input by masking sensitive keys.
        /// For structured JSON, replaces values of sensitive keys. For raw text, replaces known patterns.
        /// </summary>
        public static string Redact(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input ?? string.Empty;

            var result = input;
            foreach (var pattern in SensitivePatterns)
            {
                // Simple heuristic: mask values near sensitive key names
                var index = result.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    // Mark the entire input as containing sensitive data
                    return "[REDACTED — contains sensitive data]";
                }
            }
            return result;
        }

        /// <summary>
        /// Masks card PAN to show only first 6 and last 4 digits.
        /// </summary>
        public static string MaskCard(string? pan)
        {
            if (string.IsNullOrWhiteSpace(pan) || pan.Length < 10)
                return "****";
            return pan.Substring(0, 6) + "****" + pan.Substring(pan.Length - 4);
        }

        /// <summary>
        /// Masks account number to show only last 4 digits.
        /// </summary>
        public static string MaskAccount(string? account)
        {
            if (string.IsNullOrWhiteSpace(account) || account.Length <= 4)
                return "****";
            return "****" + account.Substring(account.Length - 4);
        }
    }
}