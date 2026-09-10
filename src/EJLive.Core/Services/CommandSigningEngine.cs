using System.Security.Cryptography;
using System.Text;

namespace EJLive.Core.Services;

/// <summary>
/// Signs and verifies remote command envelopes with HMAC/SHA-256.
/// Protects command integrity against tampering on untrusted networks.
/// </summary>
public static class CommandEnvelopeAuthenticator
{
    public const string SignatureVersion = "HMAC-SHA256-V1";

    public static bool AllowUnsignedCommands
    {
        get
        {
            var raw = Environment.GetEnvironmentVariable("EJLIVE_ALLOW_UNSIGNED_COMMANDS");
            return bool.TryParse(raw, out var enabled) && enabled;
        }
    }

    public static int MaxCommandAgeMinutes
    {
        get
        {
            var raw = Environment.GetEnvironmentVariable("EJLIVE_COMMAND_MAX_AGE_MIN");
            if (int.TryParse(raw, out var value))
                return Math.Clamp(value, 1, 240);
            return 30;
        }
    }

    public static string BuildSigningPayload(
        string commandId,
        string commandType,
        bool requiresConfirmation,
        string payloadBase64,
        string issuedAtUtc,
        string nonce)
    {
        return string.Join("\n", new[]
        {
            (commandId ?? string.Empty).Trim(),
            (commandType ?? string.Empty).Trim(),
            requiresConfirmation ? "true" : "false",
            payloadBase64 ?? string.Empty,
            issuedAtUtc ?? string.Empty,
            nonce ?? string.Empty
        });
    }

    public static string SignPayload(string signingPayload)
    {
        var signature = ComputeSignatureBytes(signingPayload ?? string.Empty);
        return Convert.ToHexString(signature);
    }

    public static bool VerifyPayload(string signingPayload, string? signature, out string reason)
    {
        var provided = (signature ?? string.Empty).Trim();
        if (provided.Length == 0)
        {
            reason = "missing signature";
            return false;
        }

        byte[] providedBytes;
        try
        {
            providedBytes = Convert.FromHexString(provided);
        }
        catch
        {
            reason = "invalid signature format";
            return false;
        }

        var expected = ComputeSignatureBytes(signingPayload ?? string.Empty);
        if (!CryptographicOperations.FixedTimeEquals(expected, providedBytes))
        {
            reason = "signature mismatch";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    /// <summary>
    /// Signs a raw command payload and returns Base64 HMAC-SHA256.
    /// Uses the same secure key resolver as envelope signing.
    /// </summary>
    public static string SignCommand(string command)
    {
        var signature = ComputeSignatureBytes(command ?? string.Empty);
        return Convert.ToBase64String(signature);
    }

    /// <summary>
    /// Verifies a raw command payload against Base64 or HEX signature formats.
    /// </summary>
    public static bool VerifyCommand(string command, string? signature)
    {
        var provided = (signature ?? string.Empty).Trim();
        if (provided.Length == 0)
            return false;

        if (!TryDecodeSignature(provided, out var providedBytes))
            return false;

        var expected = ComputeSignatureBytes(command ?? string.Empty);
        return CryptographicOperations.FixedTimeEquals(expected, providedBytes);
    }

    public static bool IsFresh(string? issuedAtUtc, out string reason)
    {
        if (!DateTime.TryParse(issuedAtUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out var issued))
        {
            reason = "invalid issued-at timestamp";
            return false;
        }

        var age = DateTime.UtcNow - issued.ToUniversalTime();
        if (age < TimeSpan.FromMinutes(-5))
        {
            reason = "issued-at is in the future";
            return false;
        }

        if (age > TimeSpan.FromMinutes(MaxCommandAgeMinutes))
        {
            reason = $"command expired (age={age.TotalMinutes:F1}m)";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static byte[] ResolveSecretKey()
    {
        var raw = Environment.GetEnvironmentVariable("EJLIVE_COMMAND_HMAC_KEY");
        if (string.IsNullOrWhiteSpace(raw))
            return EJLive.Shared.SecurityConfig.GetTransferKey();

        var key = raw.Trim();
        try
        {
            var base64 = Convert.FromBase64String(key);
            if (base64.Length >= 16)
                return base64;
        }
        catch
        {
            // Fall back to UTF-8 bytes.
        }

        var utf8 = Encoding.UTF8.GetBytes(key);
        if (utf8.Length >= 16)
            return utf8;

        // Avoid weak key lengths.
        return SHA256.HashData(utf8);
    }

    private static byte[] ComputeSignatureBytes(string text)
    {
        var key = ResolveSecretKey();
        var bytes = Encoding.UTF8.GetBytes(text);
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(bytes);
    }

    private static bool TryDecodeSignature(string signature, out byte[] bytes)
    {
        bytes = Array.Empty<byte>();
        try
        {
            bytes = Convert.FromHexString(signature);
            return true;
        }
        catch
        {
            // Ignore and try Base64.
        }

        try
        {
            bytes = Convert.FromBase64String(signature);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
