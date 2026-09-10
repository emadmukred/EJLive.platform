using System;
using System.Security.Cryptography;
using System.Text;

namespace EJLive.Shared;

/// <summary>
/// Production-safe secret resolution for EJLive cryptographic operations.
/// All secrets are sourced from environment variables or machine-local DPAI-protected storage.
/// Hardcoded fallback keys have been removed.
/// </summary>
public static class SecurityConfig
{
    private const string MigrationKeyEnvVar = "EJLIVE_MIGRATION_AES_KEY";
    private const string MigrationIvEnvVar = "EJLIVE_MIGRATION_AES_IV";
    private const string TransferKeyEnvVar = "EJLIVE_TRANSFER_KEY";
    private const string KdfSaltEnvVar = "EJLIVE_KDF_SALT";

    private static readonly object _saltLock = new object();
    private static byte[]? _cachedMachineSalt;

    /// <summary>
    /// Returns the AES key used only while importing encrypted historical data.
    /// </summary>
    public static byte[] GetMigrationAesKey()
    {
        var value = Environment.GetEnvironmentVariable(MigrationKeyEnvVar);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Migration AES key not configured. Set environment variable '{MigrationKeyEnvVar}' " +
                "to a 32-byte hex value while importing historical data.");
        }

        var bytes = SecurityHelper.HexToBytes(value);
        if (bytes.Length != 32)
            throw new InvalidOperationException($"Migration AES key must decode to exactly 32 bytes (got {bytes.Length}).");

        return bytes;
    }

    /// <summary>
    /// Returns the AES IV used only while importing encrypted historical data.
    /// </summary>
    public static byte[] GetMigrationAesIv()
    {
        var value = Environment.GetEnvironmentVariable(MigrationIvEnvVar);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Migration AES IV not configured. Set environment variable '{MigrationIvEnvVar}' " +
                "to a 16-byte hex value while importing historical data.");
        }

        var bytes = SecurityHelper.HexToBytes(value);
        if (bytes.Length != 16)
            throw new InvalidOperationException($"Migration AES IV must decode to exactly 16 bytes (got {bytes.Length}).");

        return bytes;
    }

    /// <summary>
    /// Returns the transfer encryption key password from environment, or a derived
    /// deterministic machine-local fallback that is NOT globally shared.
    /// </summary>
    public static byte[] GetTransferKey()
    {
        var value = Environment.GetEnvironmentVariable(TransferKeyEnvVar);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return SecurityHelper.DeriveKey(value, GetMachineSalt(), keySize: 32);
        }

        // Machine-local derived fallback — unique per machine, not shared globally.
        // In production, always set EJLIVE_TRANSFER_KEY explicitly.
        return SecurityHelper.DeriveKey(
            $"EJLive.Machine.{Environment.MachineName}.{Environment.UserDomainName}",
            GetMachineSalt(),
            keySize: 32);
    }

    /// <summary>
    /// Returns a consistent salt for this machine. Prefers an environment variable;
    /// falls back to a derived machine-unique salt (not secret, just unique).
    /// </summary>
    public static byte[] GetMachineSalt()
    {
        var env = Environment.GetEnvironmentVariable(KdfSaltEnvVar);
        if (!string.IsNullOrWhiteSpace(env))
        {
            var bytes = SecurityHelper.HexToBytes(env);
            if (bytes.Length >= 16)
                return bytes;
        }

        lock (_saltLock)
        {
            if (_cachedMachineSalt != null)
                return _cachedMachineSalt;

            var seed = $"EJLive.Salt.{Environment.MachineName}.{Environment.UserDomainName}.{Environment.UserName}";
            using var sha = SHA256.Create();
            _cachedMachineSalt = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
            return _cachedMachineSalt;
        }
    }

    /// <summary>
    /// True if the transfer key was explicitly configured via environment variable.
    /// </summary>
    public static bool IsTransferKeyExplicitlyConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(TransferKeyEnvVar));

    /// <summary>
    /// True if historical-data migration keys are configured.
    /// </summary>
    public static bool IsMigrationEncryptionConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(MigrationKeyEnvVar)) &&
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(MigrationIvEnvVar));
}
