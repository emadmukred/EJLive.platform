using System;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Static authorization and risk policy evaluator for remote commands.
    /// </summary>
    public static class RemoteCommandPolicy
    {
        /// <summary>
        /// Evaluates whether the issuer is authorized to issue the command based on role and risk.
        /// </summary>
        /// <param name="envelope">The command envelope to evaluate.</param>
        /// <param name="currentUtc">The current UTC time against which expiry is checked.</param>
        /// <param name="inMaintenanceWindow">Whether the system is currently in a maintenance window.</param>
        /// <param name="operatorConfirmed">Whether a second operator has explicitly confirmed the command.</param>
        /// <returns>True if the command passes authorization; otherwise false.</returns>
        public static bool IsAuthorized(
            RemoteCommandEnvelope envelope,
            DateTime currentUtc,
            bool inMaintenanceWindow,
            bool operatorConfirmed)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            if (string.Equals(envelope.IssuerRole, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (envelope.RiskLevel == CommandRiskLevel.Critical)
                {
                    return inMaintenanceWindow && operatorConfirmed;
                }
                return true;
            }

            if (string.Equals(envelope.IssuerRole, "Support", StringComparison.OrdinalIgnoreCase))
            {
                return envelope.RiskLevel == CommandRiskLevel.Low || envelope.RiskLevel == CommandRiskLevel.Medium;
            }

            return false;
        }

        /// <summary>
        /// Verifies the cryptographic signature of the command envelope.
        /// This is a placeholder for production integration with HSM or key-vault signing.
        /// </summary>
        /// <param name="envelope">The command envelope to verify.</param>
        /// <returns>True if the signature is valid; otherwise false.</returns>
        public static bool VerifySignature(RemoteCommandEnvelope envelope)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            if (string.IsNullOrWhiteSpace(envelope.Signature))
                return false;

            // Placeholder: production code should compute a deterministic byte representation
            // and verify against an issuer-specific public key retrieved from a key vault.
            return envelope.Signature.StartsWith("sig:", StringComparison.Ordinal);
        }

        /// <summary>
        /// Enforces the risk policy for a command, checking timestamp freshness and signature validity.
        /// </summary>
        /// <param name="envelope">The command envelope to evaluate.</param>
        /// <param name="currentUtc">The current UTC time.</param>
        /// <param name="inMaintenanceWindow">Whether the system is in a maintenance window.</param>
        /// <param name="operatorConfirmed">Whether operator confirmation was received.</param>
        /// <param name="failureReason">When the method returns false, contains a human-readable failure reason.</param>
        /// <returns>True if the command satisfies all risk policies; otherwise false.</returns>
        public static bool EnforceRiskPolicy(
            RemoteCommandEnvelope envelope,
            DateTime currentUtc,
            bool inMaintenanceWindow,
            bool operatorConfirmed,
            out string failureReason)
        {
            if (envelope == null)
                throw new ArgumentNullException(nameof(envelope));

            if (currentUtc > envelope.ExpiryUtc)
            {
                failureReason = "Command has expired.";
                return false;
            }

            if (currentUtc < envelope.TimestampUtc.AddMinutes(-5))
            {
                failureReason = "Command timestamp is too far in the future.";
                return false;
            }

            if (!VerifySignature(envelope))
            {
                failureReason = "Signature verification failed.";
                return false;
            }

            if (!IsAuthorized(envelope, currentUtc, inMaintenanceWindow, operatorConfirmed))
            {
                failureReason = "Issuer is not authorized for this command risk level.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }
    }
}
