using System;
using System.Collections.Generic;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Configuration for the <see cref="SafeWindowsPolicyEnforcer"/>.
    /// </summary>
    public sealed record SafeWindowsPolicyConfig
    {
        /// <summary>Global enforcement mode. Defaults to <see cref="PolicyEnforcementMode.Audit"/>.</summary>
        public PolicyEnforcementMode Mode { get; init; } = PolicyEnforcementMode.Audit;

        /// <summary>When true, <see cref="PolicyEnforcementMode.Enforce"/> is permitted.</summary>
        public bool ExplicitEnforceEnabled { get; init; } = false;

        /// <summary>When true, local policy writes are skipped if a domain GPO override is detected.</summary>
        public bool DomainGpoRespect { get; init; } = true;

        /// <summary>Accounts permitted for password change operations.</summary>
        public HashSet<string> AllowedPasswordAccounts { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>The server IP used to scope firewall rules.</summary>
        public string? ServerIp { get; init; }
    }

    /// <summary>
    /// Enforces Windows, RDP, Firewall, and WinRM policies safely with snapshots, rollback plans,
    /// and GPO awareness. Defaults to Audit mode; Enforce requires explicit enablement.
    /// </summary>
    public class SafeWindowsPolicyEnforcer
    {
        private readonly SafeWindowsPolicyConfig _config;
        private readonly List<PolicySnapshot> _snapshots = new();

        // Testability hooks
        private readonly Func<bool> _isElevated;
        private readonly Func<bool> _isDomainJoined;
        private readonly Func<string, bool> _hasGpoOverride;
        private readonly Func<PolicySnapshot, bool>? _policyWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeWindowsPolicyEnforcer"/> class.
        /// </summary>
        public SafeWindowsPolicyEnforcer(
            SafeWindowsPolicyConfig config,
            Func<bool>? isElevated = null,
            Func<bool>? isDomainJoined = null,
            Func<string, bool>? hasGpoOverride = null,
            Func<PolicySnapshot, bool>? policyWriter = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _isElevated = isElevated ?? (() => true); // default optimistic for unit tests
            _isDomainJoined = isDomainJoined ?? (() => false);
            _hasGpoOverride = hasGpoOverride ?? (_ => false);
            _policyWriter = policyWriter;
        }

        /// <summary>
        /// Gets a read-only view of captured policy snapshots.
        /// </summary>
        public IReadOnlyList<PolicySnapshot> Snapshots => _snapshots.AsReadOnly();

        /// <summary>
        /// Attempts to apply an RDP policy change.
        /// </summary>
        /// <param name="policyType">The policy category (e.g., RDP).</param>
        /// <param name="beforeJson">Current state JSON.</param>
        /// <param name="afterJson">Desired state JSON.</param>
        /// <param name="rollbackJson">Rollback plan JSON.</param>
        /// <param name="operatorId">Operator requesting the change.</param>
        /// <param name="reason">Reason for the change.</param>
        /// <param name="error">Error description if the operation fails.</param>
        /// <returns>True if the policy was applied or audited successfully.</returns>
        public bool TryApplyRdpPolicy(
            string policyType,
            string beforeJson,
            string afterJson,
            string rollbackJson,
            string operatorId,
            string reason,
            out string error)
        {
            if (!ValidatePrerequisites(policyType, operatorId, out error))
                return false;

            if (_config.DomainGpoRespect && _isDomainJoined() && _hasGpoOverride(policyType))
            {
                error = "Domain GPO override detected; local RDP policy will not be written.";
                return false;
            }

            return TryApplyCore(policyType, beforeJson, afterJson, rollbackJson, operatorId, reason, out error);
        }

        /// <summary>
        /// Attempts to apply a Firewall policy change.
        /// </summary>
        public bool TryApplyFirewallPolicy(
            string beforeJson,
            string afterJson,
            string rollbackJson,
            string operatorId,
            string reason,
            out string error)
        {
            const string policyType = "Firewall";

            if (!ValidatePrerequisites(policyType, operatorId, out error))
                return false;

            if (string.IsNullOrWhiteSpace(_config.ServerIp))
            {
                error = "Firewall scope is missing; ServerIp must be configured.";
                return false;
            }

            // NEVER disable Defender or Firewall
            if (afterJson.Contains("\"Enabled\": false", StringComparison.OrdinalIgnoreCase)
                || afterJson.Contains("Disable", StringComparison.OrdinalIgnoreCase))
            {
                error = "Disabling Defender or Firewall is not permitted.";
                return false;
            }

            if (_config.DomainGpoRespect && _isDomainJoined() && _hasGpoOverride(policyType))
            {
                error = "Domain GPO override detected; local Firewall policy will not be written.";
                return false;
            }

            return TryApplyCore(policyType, beforeJson, afterJson, rollbackJson, operatorId, reason, out error);
        }

        /// <summary>
        /// Attempts to apply a WinRM policy change.
        /// </summary>
        public bool TryApplyWinRmPolicy(
            string beforeJson,
            string afterJson,
            string rollbackJson,
            string operatorId,
            string reason,
            out string error)
        {
            const string policyType = "WinRM";

            if (!ValidatePrerequisites(policyType, operatorId, out error))
                return false;

            if (afterJson.Contains("\"Enabled\": false", StringComparison.OrdinalIgnoreCase))
            {
                error = "Disabling WinRM via policy enforcer is not permitted; use explicit server administration instead.";
                return false;
            }

            if (_config.DomainGpoRespect && _isDomainJoined() && _hasGpoOverride(policyType))
            {
                error = "Domain GPO override detected; local WinRM policy will not be written.";
                return false;
            }

            return TryApplyCore(policyType, beforeJson, afterJson, rollbackJson, operatorId, reason, out error);
        }

        /// <summary>
        /// Attempts to apply a Windows password policy change for a specific account.
        /// </summary>
        /// <param name="accountName">The target account name.</param>
        /// <param name="encryptedPayload">Encrypted password payload.</param>
        /// <param name="beforeJson">Current state JSON.</param>
        /// <param name="afterJson">Desired state JSON.</param>
        /// <param name="rollbackJson">Rollback plan JSON.</param>
        /// <param name="operatorId">Operator requesting the change.</param>
        /// <param name="reason">Reason for the change.</param>
        /// <param name="error">Error description if the operation fails.</param>
        /// <returns>True if the policy was applied or audited successfully.</returns>
        public bool TryApplyPasswordPolicy(
            string accountName,
            string encryptedPayload,
            string beforeJson,
            string afterJson,
            string rollbackJson,
            string operatorId,
            string reason,
            out string error)
        {
            const string policyType = "Password";

            if (!ValidatePrerequisites(policyType, operatorId, out error))
                return false;

            if (string.IsNullOrWhiteSpace(accountName))
            {
                error = "Account name is required for password policy changes.";
                return false;
            }

            if (!_config.AllowedPasswordAccounts.Contains(accountName))
            {
                error = $"Account '{accountName}' is not in the allowed password accounts list.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(encryptedPayload))
            {
                error = "Password payload must be encrypted and non-empty.";
                return false;
            }

            return TryApplyCore(policyType, beforeJson, afterJson, rollbackJson, operatorId, reason, out error);
        }

        /// <summary>
        /// Validates common prerequisites: elevation, enforcement approval, and operator identity.
        /// </summary>
        private bool ValidatePrerequisites(string policyType, string operatorId, out string error)
        {
            if (string.IsNullOrWhiteSpace(operatorId))
            {
                error = "OperatorId is required.";
                return false;
            }

            if (!_isElevated())
            {
                error = "Administrator elevation is required to apply policy changes.";
                return false;
            }

            if (_config.Mode == PolicyEnforcementMode.Enforce && !_config.ExplicitEnforceEnabled)
            {
                error = "Enforce mode is not explicitly enabled in configuration.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        /// <summary>
        /// Core application logic: capture snapshot, then audit or enforce.
        /// </summary>
        private bool TryApplyCore(
            string policyType,
            string beforeJson,
            string afterJson,
            string rollbackJson,
            string operatorId,
            string reason,
            out string error)
        {
            var snapshot = new PolicySnapshot
            {
                SnapshotId = Guid.NewGuid().ToString("N"),
                PolicyType = policyType,
                BeforeJson = beforeJson,
                AfterJson = afterJson,
                RollbackJson = rollbackJson,
                TimestampUtc = DateTime.UtcNow,
                OperatorId = operatorId,
                Reason = reason
            };

            _snapshots.Add(snapshot);

            if (_config.Mode == PolicyEnforcementMode.Audit)
            {
                error = $"Policy '{policyType}' audited only; no system changes were made.";
                return true;
            }

            if (_policyWriter is null)
            {
                error = "Enforce mode requires an explicitly configured policy writer.";
                return false;
            }

            if (!_policyWriter(snapshot))
            {
                error = $"Policy writer rejected '{policyType}'; no system changes were committed.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}
