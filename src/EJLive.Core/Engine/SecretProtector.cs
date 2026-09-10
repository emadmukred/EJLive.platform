using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Provides Windows DPAPI-based protection for sensitive secrets.
    /// Uses <see cref="ProtectedData"/> with <see cref="DataProtectionScope.CurrentUser"/>.
    /// </summary>
    /// <remarks>
    /// On non-Windows platforms, operations throw <see cref="PlatformNotSupportedException"/>.
    /// </remarks>
    public static class SecretProtector
    {
        /// <summary>
        /// Encrypts the specified plain text using DPAPI for the current user.
        /// </summary>
        /// <param name="plainText">The secret to protect. Must not be null or empty.</param>
        /// <returns>The encrypted payload as a byte array.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="plainText"/> is null or empty.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown when the operating system is not Windows.</exception>
        /// <exception cref="CryptographicException">Thrown when DPAPI protection fails.</exception>
        public static byte[] Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentException("Plain text cannot be null or empty.", nameof(plainText));
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException(
                    "SecretProtector relies on Windows DPAPI and is only supported on Windows.");
            }

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            return ProtectedData.Protect(plainBytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
        }

        /// <summary>
        /// Decrypts the specified DPAPI-encrypted payload.
        /// </summary>
        /// <param name="encrypted">The encrypted payload. Must not be null or empty.</param>
        /// <returns>The original plain text string.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="encrypted"/> is null or empty.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown when the operating system is not Windows.</exception>
        /// <exception cref="CryptographicException">Thrown when DPAPI unprotection fails (e.g., corrupted data or wrong user).</exception>
        public static string Unprotect(byte[] encrypted)
        {
            if (encrypted == null || encrypted.Length == 0)
            {
                throw new ArgumentException("Encrypted data cannot be null or empty.", nameof(encrypted));
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException(
                    "SecretProtector relies on Windows DPAPI and is only supported on Windows.");
            }

            byte[] plainBytes = ProtectedData.Unprotect(encrypted, optionalEntropy: null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
