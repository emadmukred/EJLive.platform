using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Cryptographic signing engine for command payloads using HMAC-SHA256.
    /// Supports key rotation through versioned keys.
    /// </summary>
    public sealed class CommandSigningEngine
    {
        private readonly Dictionary<int, byte[]> _versionedKeys;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandSigningEngine"/> class
        /// with the provided versioned keys.
        /// </summary>
        /// <param name="versionedKeys">
        /// A dictionary mapping key version integers to their raw key material.
        /// Must contain at least one entry.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="versionedKeys"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="versionedKeys"/> is empty.</exception>
        public CommandSigningEngine(Dictionary<int, byte[]> versionedKeys)
        {
            if (versionedKeys == null)
            {
                throw new ArgumentNullException(nameof(versionedKeys));
            }

            if (versionedKeys.Count == 0)
            {
                throw new ArgumentException("At least one versioned key must be provided.", nameof(versionedKeys));
            }

            _versionedKeys = new Dictionary<int, byte[]>();
            foreach (var kvp in versionedKeys)
            {
                if (kvp.Value == null || kvp.Value.Length == 0)
                {
                    throw new ArgumentException($"Key material for version {kvp.Key} cannot be null or empty.");
                }

                _versionedKeys[kvp.Key] = (byte[])kvp.Value.Clone();
            }
        }

        /// <summary>
        /// Gets the latest key version available in the engine.
        /// </summary>
        public int LatestKeyVersion
        {
            get
            {
                lock (_lock)
                {
                    int latest = int.MinValue;
                    foreach (int version in _versionedKeys.Keys)
                    {
                        if (version > latest)
                        {
                            latest = version;
                        }
                    }

                    return latest;
                }
            }
        }

        /// <summary>
        /// Computes a Base64-encoded HMAC-SHA256 signature for the given payload.
        /// </summary>
        /// <param name="payload">The payload to sign. Must not be null.</param>
        /// <param name="keyVersion">The key version to use for signing.</param>
        /// <returns>A Base64-encoded signature string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the specified <paramref name="keyVersion"/> is not available.</exception>
        public string Sign(byte[] payload, int keyVersion)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            byte[] key = ResolveKey(keyVersion);
            using var hmac = new HMACSHA256(key);
            byte[] signature = hmac.ComputeHash(payload);
            return Convert.ToBase64String(signature);
        }

        /// <summary>
        /// Computes a Base64-encoded HMAC-SHA256 signature for the given UTF-8 payload.
        /// </summary>
        /// <param name="payload">The payload to sign. Must not be null.</param>
        /// <param name="keyVersion">The key version to use for signing.</param>
        /// <returns>A Base64-encoded signature string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the specified <paramref name="keyVersion"/> is not available.</exception>
        public string Sign(string payload, int keyVersion)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            return Sign(Encoding.UTF8.GetBytes(payload), keyVersion);
        }

        /// <summary>
        /// Verifies that the provided signature matches the computed HMAC-SHA256 signature
        /// for the given payload and key version.
        /// </summary>
        /// <param name="payload">The payload that was signed. Must not be null.</param>
        /// <param name="signature">The Base64-encoded signature to verify.</param>
        /// <param name="keyVersion">The key version used when signing.</param>
        /// <returns><c>true</c> if the signature is valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the specified <paramref name="keyVersion"/> is not available.</exception>
        public bool Verify(byte[] payload, string signature, int keyVersion)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (string.IsNullOrEmpty(signature))
            {
                return false;
            }

            byte[] key = ResolveKey(keyVersion);
            using var hmac = new HMACSHA256(key);
            byte[] computed = hmac.ComputeHash(payload);
            byte[] provided;

            try
            {
                provided = Convert.FromBase64String(signature);
            }
            catch (FormatException)
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(computed, provided);
        }

        /// <summary>
        /// Verifies that the provided signature matches the computed HMAC-SHA256 signature
        /// for the given UTF-8 payload and key version.
        /// </summary>
        /// <param name="payload">The payload that was signed. Must not be null.</param>
        /// <param name="signature">The Base64-encoded signature to verify.</param>
        /// <param name="keyVersion">The key version used when signing.</param>
        /// <returns><c>true</c> if the signature is valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the specified <paramref name="keyVersion"/> is not available.</exception>
        public bool Verify(string payload, string signature, int keyVersion)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            return Verify(Encoding.UTF8.GetBytes(payload), signature, keyVersion);
        }

        /// <summary>
        /// Rotates the signing keys by adding a new version while optionally
        /// retiring an old version.
        /// </summary>
        /// <param name="newVersion">The version identifier for the new key.</param>
        /// <param name="newKey">The raw key material for the new version.</param>
        /// <param name="retireVersion">The version to remove, or null to keep all existing versions.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="newKey"/> is null or empty, or when <paramref name="newVersion"/> already exists.</exception>
        public void RotateKey(int newVersion, byte[] newKey, int? retireVersion = null)
        {
            if (newKey == null || newKey.Length == 0)
            {
                throw new ArgumentException("New key material cannot be null or empty.", nameof(newKey));
            }

            lock (_lock)
            {
                if (_versionedKeys.ContainsKey(newVersion))
                {
                    throw new ArgumentException($"Key version {newVersion} already exists.", nameof(newVersion));
                }

                _versionedKeys[newVersion] = (byte[])newKey.Clone();

                if (retireVersion.HasValue)
                {
                    _versionedKeys.Remove(retireVersion.Value);
                }
            }
        }

        /// <summary>
        /// Returns a copy of the currently active key versions.
        /// </summary>
        /// <returns>A read-only view of active version numbers.</returns>
        public IReadOnlyCollection<int> GetActiveVersions()
        {
            lock (_lock)
            {
                return new List<int>(_versionedKeys.Keys).AsReadOnly();
            }
        }

        private byte[] ResolveKey(int keyVersion)
        {
            lock (_lock)
            {
                if (!_versionedKeys.TryGetValue(keyVersion, out byte[]? key))
                {
                    throw new ArgumentException($"Key version {keyVersion} is not available.", nameof(keyVersion));
                }

                return key;
            }
        }
    }
}
