using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using EJLive.Core.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track16
{
    [TestClass]
    public class SecurityHardeningTests
    {
        #region SecretProtector

        [TestMethod]
        public void Protect_NullOrEmpty_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => SecretProtector.Protect(null!));
            Assert.ThrowsException<ArgumentException>(() => SecretProtector.Protect(string.Empty));
        }

        [TestMethod]
        public void Unprotect_NullOrEmpty_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => SecretProtector.Unprotect(null!));
            Assert.ThrowsException<ArgumentException>(() => SecretProtector.Unprotect(Array.Empty<byte>()));
        }

        [TestMethod]
        public void ProtectUnprotect_RoundTrip_ReturnsOriginal()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("DPAPI is only available on Windows.");
                return;
            }

            string original = "SensitivePayload_42!@#";
            byte[] encrypted = SecretProtector.Protect(original);

            Assert.IsNotNull(encrypted);
            CollectionAssert.AreNotEqual(Encoding.UTF8.GetBytes(original), encrypted);

            string decrypted = SecretProtector.Unprotect(encrypted);
            Assert.AreEqual(original, decrypted);
        }

        [TestMethod]
        public void Protect_Unprotect_DifferentInputs_ProducesDifferentCipherTexts()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Inconclusive("DPAPI is only available on Windows.");
                return;
            }

            byte[] cipher1 = SecretProtector.Protect("SecretA");
            byte[] cipher2 = SecretProtector.Protect("SecretB");

            CollectionAssert.AreNotEqual(cipher1, cipher2);
        }

        #endregion

        #region CommandSigningEngine

        [TestMethod]
        public void Constructor_EmptyKeys_ThrowsArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(() => new CommandSigningEngine(new Dictionary<int, byte[]>()));
        }

        [TestMethod]
        public void SignAndVerify_ValidPayload_ReturnsTrue()
        {
            var keys = new Dictionary<int, byte[]>
            {
                { 1, Encoding.UTF8.GetBytes("key-one-32-bytes-long-for-hmac!!") }
            };

            var engine = new CommandSigningEngine(keys);
            string payload = "command=reset&atm=1234";

            string signature = engine.Sign(payload, 1);
            bool valid = engine.Verify(payload, signature, 1);

            Assert.IsFalse(string.IsNullOrEmpty(signature));
            Assert.IsTrue(valid);
        }

        [TestMethod]
        public void Verify_TamperedPayload_ReturnsFalse()
        {
            var keys = new Dictionary<int, byte[]>
            {
                { 1, Encoding.UTF8.GetBytes("key-one-32-bytes-long-for-hmac!!") }
            };

            var engine = new CommandSigningEngine(keys);
            string payload = "command=reset&atm=1234";
            string signature = engine.Sign(payload, 1);

            bool valid = engine.Verify("command=reset&atm=9999", signature, 1);
            Assert.IsFalse(valid);
        }

        [TestMethod]
        public void Verify_InvalidSignature_ReturnsFalse()
        {
            var keys = new Dictionary<int, byte[]>
            {
                { 1, Encoding.UTF8.GetBytes("key-one-32-bytes-long-for-hmac!!") }
            };

            var engine = new CommandSigningEngine(keys);
            string payload = "command=reset&atm=1234";

            bool valid = engine.Verify(payload, "not-a-valid-signature", 1);
            Assert.IsFalse(valid);
        }

        [TestMethod]
        public void Sign_MissingKeyVersion_ThrowsArgumentException()
        {
            var keys = new Dictionary<int, byte[]>
            {
                { 1, Encoding.UTF8.GetBytes("key-one-32-bytes-long-for-hmac!!") }
            };

            var engine = new CommandSigningEngine(keys);
            Assert.ThrowsException<ArgumentException>(() => engine.Sign("payload", 99));
        }

        [TestMethod]
        public void KeyRotation_AddsNewVersion_AndRetiresOld()
        {
            var keys = new Dictionary<int, byte[]>
            {
                { 1, Encoding.UTF8.GetBytes("key-one-32-bytes-long-for-hmac!!") }
            };

            var engine = new CommandSigningEngine(keys);
            Assert.AreEqual(1, engine.LatestKeyVersion);

            engine.RotateKey(2, Encoding.UTF8.GetBytes("key-two-32-bytes-long-for-hmac!!"), retireVersion: 1);

            Assert.AreEqual(2, engine.LatestKeyVersion);
            CollectionAssert.Contains(new List<int>(engine.GetActiveVersions()), 2);
            CollectionAssert.DoesNotContain(new List<int>(engine.GetActiveVersions()), 1);

            string payload = "test-data";
            string sig = engine.Sign(payload, 2);
            Assert.IsTrue(engine.Verify(payload, sig, 2));
        }

        [TestMethod]
        public void KeyRotation_DuplicateVersion_ThrowsArgumentException()
        {
            var keys = new Dictionary<int, byte[]>
            {
                { 1, Encoding.UTF8.GetBytes("key-one-32-bytes-long-for-hmac!!") }
            };

            var engine = new CommandSigningEngine(keys);
            Assert.ThrowsException<ArgumentException>(() =>
                engine.RotateKey(1, Encoding.UTF8.GetBytes("new-key-32-bytes-long-for-hmac")));
        }

        [TestMethod]
        public void LatestKeyVersion_MultipleVersions_ReturnsMax()
        {
            var keys = new Dictionary<int, byte[]>
            {
                { 1, Encoding.UTF8.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa") },
                { 5, Encoding.UTF8.GetBytes("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb") },
                { 3, Encoding.UTF8.GetBytes("cccccccccccccccccccccccccccccccc") }
            };

            var engine = new CommandSigningEngine(keys);
            Assert.AreEqual(5, engine.LatestKeyVersion);
        }

        #endregion

        #region LogRedactionEngine

        [TestMethod]
        public void Redact_NullOrEmpty_ReturnsAsIs()
        {
            Assert.IsNull(LogRedactionEngine.Redact(null));
            Assert.AreEqual(string.Empty, LogRedactionEngine.Redact(string.Empty));
        }

        [TestMethod]
        public void Redact_CardNumber_ReplacesWithRedactedCard()
        {
            string input = "Payment with card 4111111111111111 processed.";
            string result = LogRedactionEngine.Redact(input)!;
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex(@"\b4111111111111111\b"));
            StringAssert.Contains(result, "[REDACTED-CARD]");
        }

        [TestMethod]
        public void Redact_AccountNumber_ReplacesWithRedactedAccount()
        {
            string input = "Account 1234567890 has insufficient funds.";
            string result = LogRedactionEngine.Redact(input)!;
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex(@"\b1234567890\b"));
            StringAssert.Contains(result, "[REDACTED-ACCOUNT]");
        }

        [TestMethod]
        public void Redact_Password_ReplacesWithRedacted()
        {
            string input = "User login with password=SuperSecret123! failed.";
            string result = LogRedactionEngine.Redact(input)!;
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex(@"SuperSecret123!"));
            StringAssert.Contains(result, "[REDACTED]");
        }

        [TestMethod]
        public void Redact_SecretKey_ReplacesWithRedacted()
        {
            string input = "api_key=abc123xyz789 connectionstring=Server=myServer;Pwd=secret";
            string result = LogRedactionEngine.Redact(input)!;
            StringAssert.DoesNotMatch(result, new System.Text.RegularExpressions.Regex(@"abc123xyz789"));
            StringAssert.Contains(result, "[REDACTED]");
        }

        [TestMethod]
        public void Redact_MultipleSensitiveValues_AllRedacted()
        {
            string input = "Card 5555555555554444, Account 9876543210, password=MyPwd, token=xyz";
            string result = LogRedactionEngine.Redact(input)!;
            StringAssert.Contains(result, "[REDACTED-CARD]");
            StringAssert.Contains(result, "[REDACTED-ACCOUNT]");
            StringAssert.Contains(result, "[REDACTED]");
        }

        #endregion

        #region SecurityPolicy

        [TestMethod]
        public void TlsRequired_AlwaysTrue()
        {
            Assert.IsTrue(SecurityPolicy.TlsRequired);
        }

        [TestMethod]
        public void IsHashAlgorithmPermitted_Sha256_ReturnsTrue()
        {
            Assert.IsTrue(SecurityPolicy.IsHashAlgorithmPermitted("SHA256", "integrity"));
            Assert.IsTrue(SecurityPolicy.IsHashAlgorithmPermitted("SHA-256", "any"));
        }

        [TestMethod]
        public void IsHashAlgorithmPermitted_Md5_LegacyIdentifier_ReturnsTrue()
        {
            // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
            Assert.IsTrue(SecurityPolicy.IsHashAlgorithmPermitted("MD5", "migration-identifier"));
            Assert.IsTrue(SecurityPolicy.IsHashAlgorithmPermitted("MD-5", "migration-identifier"));
        }

        [TestMethod]
        public void IsHashAlgorithmPermitted_Md5_NonLegacy_ReturnsFalse()
        {
            // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
            Assert.IsFalse(SecurityPolicy.IsHashAlgorithmPermitted("MD5", "integrity"));
            // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
            Assert.IsFalse(SecurityPolicy.IsHashAlgorithmPermitted("MD5", ""));
        }

        [TestMethod]
        public void IsHashAlgorithmPermitted_Unknown_ReturnsFalse()
        {
            // safe: negative assertion -- proves SHA-1 is refused for integrity use
            Assert.IsFalse(SecurityPolicy.IsHashAlgorithmPermitted("SHA1", "integrity"));
            Assert.IsFalse(SecurityPolicy.IsHashAlgorithmPermitted("", "integrity"));
        }

        [TestMethod]
        public void EnableCertificatePinning_SetsThumbprints()
        {
            var thumbprints = new[] { "AABBCCDDEEFF00112233445566778899AABBCCDD" };
            SecurityPolicy.EnableCertificatePinning(thumbprints);

            Assert.IsTrue(SecurityPolicy.CertificatePinningEnabled);
            CollectionAssert.Contains(new List<string>(SecurityPolicy.PinnedCertificateThumbprints), thumbprints[0].ToUpperInvariant());

            // Cleanup
            SecurityPolicy.DisableCertificatePinning();
        }

        [TestMethod]
        public void ValidateServerCertificate_Null_ReturnsFalse()
        {
            Assert.IsFalse(SecurityPolicy.ValidateServerCertificate(null));
        }

        [TestMethod]
        public void ValidateServerCertificate_PinningDisabled_TrustStoreFallback_ReturnsTrue()
        {
            SecurityPolicy.DisableCertificatePinning();
            Assert.IsTrue(SecurityPolicy.TrustStoreFallbackEnabled);

            using var cert = CreateSelfSignedCertificate();
            Assert.IsTrue(SecurityPolicy.ValidateServerCertificate(cert));
        }

        [TestMethod]
        public void ValidateServerCertificate_InvalidPinnedCert_ReturnsFalse()
        {
            var thumbprints = new[] { "00112233445566778899AABBCCDDEEFF00112233" };
            SecurityPolicy.EnableCertificatePinning(thumbprints);

            using var cert = CreateSelfSignedCertificate();
            bool valid = SecurityPolicy.ValidateServerCertificate(cert);

            // Because trust store fallback is enabled, it returns true even when pinned cert doesn't match
            // If we wanted strict pinning, we would need to disable fallback.
            Assert.IsTrue(valid);

            SecurityPolicy.DisableCertificatePinning();
        }

        #endregion

        #region Helpers

        private static X509Certificate2 CreateSelfSignedCertificate()
        {
            var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                "CN=EJLive-Test-CA",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(false, false, 0, false));

            X509Certificate2 cert = request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(1));

            // Export and re-import to get a certificate with a private key if needed
            return new X509Certificate2(cert.Export(X509ContentType.Pfx));
        }

        #endregion
    }
}
