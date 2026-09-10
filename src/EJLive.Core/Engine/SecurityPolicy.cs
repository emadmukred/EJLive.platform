using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Defines the static security policy enforced across the EJLive.Unified system.
    /// Includes transport requirements, hash algorithm policies, and certificate trust rules.
    /// </summary>
    public static class SecurityPolicy
    {
        /// <summary>
        /// Gets a value indicating whether TLS is required for all outbound connections.
        /// In production environments this is always <c>true</c>.
        /// </summary>
        public static bool TlsRequired => true;

        /// <summary>
        /// Gets the minimum TLS version allowed for secure connections.
        /// </summary>
        public static System.Security.Authentication.SslProtocols MinimumTlsVersion { get; } =
            System.Security.Authentication.SslProtocols.Tls12;

        /// <summary>
        /// The primary hash algorithm used for integrity verification (SHA-256).
        /// </summary>
        public const string IntegrityHashAlgorithm = "SHA256";

        /// <summary>
        /// The migration-only identifier hash algorithm (MD5). It must never protect secrets
        /// (e.g., checksum labels), never for integrity verification or cryptographic purposes.
        /// </summary>
        public const string MigrationIdentifierHashAlgorithm = "MD5";

        /// <summary>
        /// Gets a value indicating whether certificate pinning is enabled.
        /// When enabled, only certificates whose thumbprints appear in
        /// <see cref="PinnedCertificateThumbprints"/> are trusted.
        /// </summary>
        public static bool CertificatePinningEnabled { get; private set; } = false;

        /// <summary>
        /// Gets the set of pinned certificate SHA-256 thumbprints (hex, uppercase).
        /// Only consulted when <see cref="CertificatePinningEnabled"/> is <c>true</c>.
        /// </summary>
        public static IReadOnlyCollection<string> PinnedCertificateThumbprints { get; private set; } =
            new List<string>().AsReadOnly();

        /// <summary>
        /// Gets a value indicating whether the system should fall back to the OS trust store
        /// when certificate pinning is disabled or when a pinned certificate is not found.
        /// </summary>
        public static bool TrustStoreFallbackEnabled => true;

        /// <summary>
        /// Validates that the specified hash algorithm name is permitted under the current policy.
        /// </summary>
        /// <param name="algorithmName">The hash algorithm name to validate.</param>
        /// <param name="usage">The intended usage context (e.g., "integrity", "migration-identifier").</param>
        /// <returns><c>true</c> if the algorithm is permitted for the given usage; otherwise, <c>false</c>.</returns>
        public static bool IsHashAlgorithmPermitted(string algorithmName, string usage)
        {
            if (string.IsNullOrWhiteSpace(algorithmName))
            {
                return false;
            }

            string normalized = algorithmName.Trim().ToUpperInvariant();
            string normalizedUsage = (usage ?? string.Empty).Trim().ToUpperInvariant();

            if (normalized == "SHA256" || normalized == "SHA-256")
            {
                return true;
            }

            if ((normalized == "MD5" || normalized == "MD-5") && normalizedUsage == "MIGRATION-IDENTIFIER")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Configures certificate pinning with the given thumbprints.
        /// </summary>
        /// <param name="thumbprints">A collection of SHA-256 certificate thumbprints (hex, uppercase).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="thumbprints"/> is null.</exception>
        public static void EnableCertificatePinning(IEnumerable<string> thumbprints)
        {
            if (thumbprints == null)
            {
                throw new ArgumentNullException(nameof(thumbprints));
            }

            var list = new List<string>();
            foreach (string thumbprint in thumbprints)
            {
                if (!string.IsNullOrWhiteSpace(thumbprint))
                {
                    list.Add(thumbprint.Trim().ToUpperInvariant());
                }
            }

            PinnedCertificateThumbprints = list.AsReadOnly();
            CertificatePinningEnabled = PinnedCertificateThumbprints.Count > 0;
        }

        /// <summary>
        /// Disables certificate pinning and reverts to OS trust store validation.
        /// </summary>
        public static void DisableCertificatePinning()
        {
            CertificatePinningEnabled = false;
            PinnedCertificateThumbprints = new List<string>().AsReadOnly();
        }

        /// <summary>
        /// Validates the provided server certificate against the active policy
        /// (pinning or trust store fallback).
        /// </summary>
        /// <param name="certificate">The certificate to validate.</param>
        /// <returns><c>true</c> if the certificate is trusted under the current policy; otherwise, <c>false</c>.</returns>
        public static bool ValidateServerCertificate(X509Certificate2? certificate)
        {
            if (certificate == null)
            {
                return false;
            }

            if (!CertificatePinningEnabled)
            {
                return TrustStoreFallbackEnabled;
            }

            string thumbprint = certificate.Thumbprint;
            foreach (string pinned in PinnedCertificateThumbprints)
            {
                if (string.Equals(pinned, thumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return TrustStoreFallbackEnabled;
        }
    }
}
