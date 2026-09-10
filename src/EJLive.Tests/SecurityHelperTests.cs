using Xunit;
using EJLive.Shared;
using EJLive.Core.Engine;

namespace EJLive.Tests
{
    /// <summary>
    /// Tests for SecurityHelper cryptography and file I/O.
    /// </summary>
    public class SecurityHelperTests
    {
        [Fact]
        public void EncryptAES_DecryptAES_Roundtrip_Success()
        {
            var key = SecurityHelper.GenerateSessionKey();
            var plaintext = System.Text.Encoding.UTF8.GetBytes("EJLive Test Data 2026");
            var ciphertext = SecurityHelper.EncryptAES(plaintext, key);
            var decrypted = SecurityHelper.DecryptAES(ciphertext, key);
            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void SHA256Hash_Produces_Deterministic_Output()
        {
            var data = System.Text.Encoding.UTF8.GetBytes("test");
            var hash1 = SecurityHelper.SHA256Hash(data);
            var hash2 = SecurityHelper.SHA256Hash(data);
            Assert.Equal(hash1, hash2);
            Assert.Equal(64, hash1.Length); // SHA256 produces 64 hex chars
        }

        [Fact]
        public void Compress_Decompress_Roundtrip_Success()
        {
            var data = new byte[10000];
            new System.Random(42).NextBytes(data);
            var compressed = SecurityHelper.Compress(data);
            var decompressed = SecurityHelper.Decompress(compressed);
            Assert.Equal(data, decompressed);
        }

        [Fact]
        public void BytesToHex_HexToBytes_Roundtrip_Success()
        {
            var original = new byte[] { 0x00, 0xFF, 0xAB, 0x12, 0x34 };
            var hex = SecurityHelper.BytesToHex(original);
            var restored = SecurityHelper.HexToBytes(hex);
            Assert.Equal(original, restored);
        }

        [Fact]
        public void GenerateNonce_Returns_CorrectSize()
        {
            var nonce = SecurityHelper.GenerateNonce(16);
            Assert.Equal(16, nonce.Length);
            var nonce2 = SecurityHelper.GenerateNonce(16);
            Assert.NotEqual(nonce, nonce2); // Should be random
        }
    }
}