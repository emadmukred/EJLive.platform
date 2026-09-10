using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EJLive.Core.Models;

namespace EJLive.Security
{
    public sealed class SecurityCertificate
    {
        public string Id { get; set; } = string.Empty;
        public DateTime IssuedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public string PublicKey { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public sealed class EnhancedSecurityService
    {
        private readonly UnifiedSystemConfiguration _config;
        private readonly DatabaseManager _database;

        public EnhancedSecurityService(UnifiedSystemConfiguration config, DatabaseManager database)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Task<string> GenerateDigitalSignatureAsync(string data)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data ?? string.Empty));
                return Task.FromResult(Convert.ToBase64String(hash));
            }
        }

        public async Task<bool> VerifyDigitalSignatureAsync(string data, string signature)
        {
            var computedSignature = await GenerateDigitalSignatureAsync(data).ConfigureAwait(false);
            return FixedTimeEquals(computedSignature, signature ?? string.Empty);
        }

        public Task<string> EncryptDataAsync(string data)
        {
            return Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes(data ?? string.Empty)));
        }

        public Task<string> DecryptDataAsync(string encryptedData)
        {
            if (string.IsNullOrWhiteSpace(encryptedData))
            {
                return Task.FromResult(string.Empty);
            }

            try
            {
                return Task.FromResult(Encoding.UTF8.GetString(Convert.FromBase64String(encryptedData)));
            }
            catch (FormatException)
            {
                return Task.FromResult(string.Empty);
            }
        }

        public async Task<SecurityCertificate> GenerateSecurityCertificateAsync()
        {
            var issuedAt = DateTime.UtcNow;
            var payload = _config.ServerHost + ":" + _config.ServerPort + ":" + issuedAt.ToString("O");
            return new SecurityCertificate
            {
                Id = Guid.NewGuid().ToString("N"),
                IssuedAtUtc = issuedAt,
                ExpiresAtUtc = issuedAt.AddYears(1),
                PublicKey = "EJLive-" + _config.ServerHost,
                Signature = await GenerateDigitalSignatureAsync(payload).ConfigureAwait(false)
            };
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            var leftBytes = Encoding.UTF8.GetBytes(left ?? string.Empty);
            var rightBytes = Encoding.UTF8.GetBytes(right ?? string.Empty);

            if (leftBytes.Length != rightBytes.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < leftBytes.Length; i++)
            {
                diff |= leftBytes[i] ^ rightBytes[i];
            }

            return diff == 0;
        }
    }
}
