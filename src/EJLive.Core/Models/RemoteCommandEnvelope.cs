using System;
using System.Globalization;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Represents a cryptographically signed remote command envelope with risk metadata.
    /// </summary>
    public sealed record RemoteCommandEnvelope
    {
        /// <summary>Unique identifier for this command instance.</summary>
        public string CommandId { get; init; } = Guid.NewGuid().ToString("N");

        /// <summary>Discriminator for the command type (e.g., ExecuteScript, Restart, Screenshot).</summary>
        public string CommandType { get; init; } = string.Empty;

        /// <summary>Serialized JSON payload for the command.</summary>
        public string Payload { get; init; } = string.Empty;

        /// <summary>Whether the command requires explicit operator confirmation before execution.</summary>
        public bool RequiresConfirmation { get; init; }

        /// <summary>Cryptographic signature over the canonical command bytes.</summary>
        public string Signature { get; init; } = string.Empty;

        /// <summary>UTC timestamp when the command was issued.</summary>
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

        /// <summary>UTC expiry after which the command must be rejected.</summary>
        public DateTime ExpiryUtc { get; init; } = DateTime.UtcNow.AddMinutes(5);

        /// <summary>Role of the issuing operator (e.g., Admin, Support).</summary>
        public string IssuerRole { get; init; } = "Support";

        /// <summary>Identity of the issuing operator.</summary>
        public string IssuerId { get; init; } = string.Empty;

        /// <summary>Calculated risk level for the command.</summary>
        public Engine.CommandRiskLevel RiskLevel { get; init; } = Engine.CommandRiskLevel.Low;

        /// <summary>
        /// Serializes the envelope to a wire-safe string using pipe delimiters.
        /// Format: CommandId|CommandType|Base64Payload|Signature|TimestampUtcO|ExpiryUtcO|IssuerRole|IssuerId|RiskLevel|RequiresConfirmation
        /// </summary>
        public string ToWireText()
        {
            return string.Join("|",
                CommandId,
                CommandType,
                Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Payload)),
                Signature,
                TimestampUtc.ToString("O", CultureInfo.InvariantCulture),
                ExpiryUtc.ToString("O", CultureInfo.InvariantCulture),
                IssuerRole,
                IssuerId,
                RiskLevel.ToString(),
                RequiresConfirmation ? "1" : "0");
        }

        /// <summary>
        /// Attempts to parse a wire text string into a <see cref="RemoteCommandEnvelope"/>.
        /// </summary>
        public static bool TryParse(string? text, out RemoteCommandEnvelope envelope)
        {
            envelope = default!;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var parts = text.Split('|');
            if (parts.Length >= 9)
            {
                if (!DateTime.TryParseExact(parts[4], "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var timestampUtc))
                    return false;
                if (!DateTime.TryParseExact(parts[5], "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiryUtc))
                    return false;
                if (!Enum.TryParse<Engine.CommandRiskLevel>(parts[8], out var riskLevel))
                    return false;

                envelope = new RemoteCommandEnvelope
                {
                    CommandId = parts[0],
                    CommandType = parts[1],
                    Payload = DecodePayload(parts[2]),
                    Signature = parts[3],
                    TimestampUtc = timestampUtc,
                    ExpiryUtc = expiryUtc,
                    IssuerRole = parts[6],
                    IssuerId = parts[7],
                    RiskLevel = riskLevel,
                    RequiresConfirmation = parts.Length >= 10 && ParseBoolean(parts[9])
                };
                return true;
            }

            // Legacy wire format:
            // CommandId|CommandType|RequiresConfirmation|Base64Payload|IssuedAtUtc|Nonce|Signature|SignatureVersion
            if (parts.Length >= 8)
            {
                var issuedAt = DateTime.TryParse(parts[4], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var issued)
                    ? issued
                    : DateTime.UtcNow;

                envelope = new RemoteCommandEnvelope
                {
                    CommandId = parts[0],
                    CommandType = parts[1],
                    RequiresConfirmation = ParseBoolean(parts[2]),
                    Payload = DecodePayload(parts[3]),
                    Signature = parts[6],
                    TimestampUtc = issuedAt.ToUniversalTime(),
                    ExpiryUtc = issuedAt.ToUniversalTime().AddMinutes(5),
                    IssuerRole = "Support",
                    IssuerId = string.Empty,
                    RiskLevel = Engine.CommandRiskLevel.Medium
                };
                return true;
            }

            if (parts.Length >= 3)
            {
                envelope = new RemoteCommandEnvelope
                {
                    CommandId = parts[0],
                    CommandType = parts[1],
                    RequiresConfirmation = ParseBoolean(parts[2]),
                    Payload = parts.Length >= 4 ? DecodePayload(parts[3]) : string.Empty,
                    Signature = string.Empty,
                    TimestampUtc = DateTime.UtcNow,
                    ExpiryUtc = DateTime.UtcNow.AddMinutes(5),
                    IssuerRole = "Support",
                    IssuerId = string.Empty,
                    RiskLevel = Engine.CommandRiskLevel.Medium
                };
                return true;
            }

            return false;
        }

        private static bool ParseBoolean(string? value)
        {
            if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
                return true;
            return bool.TryParse(value, out var parsed) && parsed;
        }

        private static string DecodePayload(string? base64OrText)
        {
            if (string.IsNullOrEmpty(base64OrText))
                return string.Empty;

            try
            {
                var bytes = Convert.FromBase64String(base64OrText);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                return base64OrText;
            }
        }
    }
}
