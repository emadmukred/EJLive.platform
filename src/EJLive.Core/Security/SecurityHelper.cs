using System.Security.Cryptography;
using System.Text;

namespace EJLive.Core.Security;

/// <summary>
/// Provides foundational security utilities including hashing, encryption,
/// and token generation used across Core services.
/// </summary>
public static class SecurityHelper
{
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    /// <summary>
    /// Computes the SHA256 hash of the input string and returns it as a lowercase hex string.
    /// </summary>
    public static string ComputeSha256Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Computes the SHA256 hash of raw bytes and returns it as a lowercase hex string.
    /// </summary>
    public static string ComputeSha256Hash(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
    /// Computes an MD5 checksum for integrations that still require that format.
    /// </summary>
    public static string ComputeMd5Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
        var hash = MD5.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Generates a cryptographically secure random token as a base64 string.
    /// </summary>
    /// <param name="byteLength">Number of random bytes (default 32).</param>
    public static string GenerateToken(int byteLength = 32)
    {
        var bytes = new byte[byteLength];
        _rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Generates a random hex string suitable for session IDs or nonces.
    /// </summary>
    /// <param name="byteLength">Number of random bytes (default 16).</param>
    public static string GenerateHexToken(int byteLength = 16)
    {
        var bytes = new byte[byteLength];
        _rng.GetBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Performs a constant-time comparison of two strings to prevent timing attacks.
    /// </summary>
    public static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }

    /// <summary>
    /// Securely erases a byte array from memory by overwriting with zeros.
    /// </summary>
    public static void SecureErase(byte[] data)
    {
        if (data == null) return;
        CryptographicOperations.ZeroMemory(data);
    }

    /// <summary>
    /// Simple AES-256 encryption helper for sensitive configuration values.
    /// Uses a derived key from the provided passphrase.
    /// </summary>
    public static string EncryptString(string plainText, string passphrase)
    {
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        // Prepend IV to ciphertext
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Simple AES-256 decryption helper for sensitive configuration values.
    /// </summary>
    public static string DecryptString(string cipherText, string passphrase)
    {
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
        var fullCipher = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Key = key;
        var iv = new byte[aes.IV.Length];
        var cipherBytes = new byte[fullCipher.Length - iv.Length];
        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);
        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
