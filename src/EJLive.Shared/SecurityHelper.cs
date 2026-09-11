using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Text;

namespace EJLive.Shared;

public static class SecurityHelper
{
    private const int AesKeySize = 256;
    private const int AesBlockSize = 128;
    private const int Pbkdf2Iterations = 100_000;
    private const int RsaKeySize = 2048;
    private const string DpapiPrefix = "dpapi:";
    private static readonly byte[] DpapiEntropy = Encoding.UTF8.GetBytes("EJLive.Config.Secret.v1");

    public static byte[] EncryptAES(byte[] data, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(key);

        using var aes = Aes.Create();
        aes.KeySize = AesKeySize;
        aes.BlockSize = AesBlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = NormalizeKey(key, 32);
        aes.GenerateIV();

        using var enc = aes.CreateEncryptor();
        var cipherText = enc.TransformFinalBlock(data, 0, data.Length);
        var result = new byte[16 + cipherText.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, 16);
        Buffer.BlockCopy(cipherText, 0, result, 16, cipherText.Length);
        return result;
    }

    public static byte[] DecryptAES(byte[] data, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(key);
        if (data.Length < 16)
            throw new ArgumentException("Encrypted payload must contain a 16 byte IV.", nameof(data));

        var iv = new byte[16];
        var cipherText = new byte[data.Length - 16];
        Buffer.BlockCopy(data, 0, iv, 0, 16);
        Buffer.BlockCopy(data, 16, cipherText, 0, cipherText.Length);

        using var aes = Aes.Create();
        aes.KeySize = AesKeySize;
        aes.BlockSize = AesBlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = NormalizeKey(key, 32);
        aes.IV = iv;

        using var dec = aes.CreateDecryptor();
        return dec.TransformFinalBlock(cipherText, 0, cipherText.Length);
    }

    public static (string publicKeyXml, RSACryptoServiceProvider privateKey) GenerateRSAKeyPair()
    {
        var rsa = new RSACryptoServiceProvider(RsaKeySize) { PersistKeyInCsp = false };
        return (rsa.ToXmlString(false), rsa);
    }

    public static byte[] EncryptWithRSAPublicKey(byte[] data, string publicKeyXml)
    {
        using var rsa = new RSACryptoServiceProvider { PersistKeyInCsp = false };
        rsa.FromXmlString(publicKeyXml);
        return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
    }

    public static byte[] DecryptWithRSAPrivateKey(byte[] encryptedData, RSACryptoServiceProvider privateKey)
    {
        return privateKey.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
    }

    public static byte[] GenerateSessionKey()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        return key;
    }

    public static byte[] DeriveKey(string password, byte[]? salt = null, int keySize = 32)
    {
        salt ??= SecurityConfig.GetMachineSalt();
        using var kdf = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
        return kdf.GetBytes(keySize);
    }

    public static string HashPassword(string password, string? saltHex = null)
    {
        var salt = saltHex is null ? RandomBytes(16) : HexToBytes(saltHex);
        var hash = DeriveKey(password, salt);
        return $"{BytesToHex(salt)}:{BytesToHex(hash)}";
    }

    public static bool VerifyPassword(string password, string hashString)
    {
        if (string.IsNullOrWhiteSpace(hashString) || !hashString.Contains(':'))
            return false;

        var parts = hashString.Split(':');
        if (parts.Length != 2)
            return false;

        var computed = DeriveKey(password, HexToBytes(parts[0]));
        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(BytesToHex(computed)),
            Encoding.ASCII.GetBytes(parts[1]));
    }

    public static string MD5Hash(byte[] data)
    {
        if (data.Length == 0)
            return "d41d8cd98f00b204e9800998ecf8427e";

        // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
        using var md5 = MD5.Create();
        // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
        return BytesToHex(md5.ComputeHash(data));
    }

    public static string MD5Hash(string text) => MD5Hash(Encoding.UTF8.GetBytes(text ?? string.Empty));

    public static string ComputeMD5(byte[] data) => MD5Hash(data);

    public static string ComputeFileMD5(string filePath)
    {
        using var fs = OpenSequentialReadStream(filePath);
        // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
        using var md5 = MD5.Create();
        // safe: vendor journal/archive fingerprint kept for byte-compatibility; not a security boundary (SS9 integrity uses HMAC-SHA256)
        return BytesToHex(md5.ComputeHash(fs));
    }

    public static string SHA256Hash(byte[] data)
    {
        if (data.Length == 0)
            return string.Empty;

        using var sha = SHA256.Create();
        return BytesToHex(sha.ComputeHash(data));
    }

    public static string SHA256Hash(string text) => SHA256Hash(Encoding.UTF8.GetBytes(text ?? string.Empty));

    public static string ComputeSHA256(byte[] data) => SHA256Hash(data);

    public static string HMACSHA256(byte[] data, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return BytesToHex(hmac.ComputeHash(data));
    }

    public static byte[] Compress(byte[] data)
    {
        if (data.Length == 0)
            return Array.Empty<byte>();

        using var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionMode.Compress, leaveOpen: true))
            stream.Write(data, 0, data.Length);
        return output.ToArray();
    }

    public static string CompressText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return Convert.ToBase64String(Compress(Encoding.UTF8.GetBytes(text)));
    }

    public static byte[] Decompress(byte[] data)
    {
        if (data.Length == 0)
            return Array.Empty<byte>();

        using var input = new MemoryStream(data);
        using var stream = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        stream.CopyTo(output);
        return output.ToArray();
    }

    public static string DecompressText(string compressedText)
    {
        if (string.IsNullOrEmpty(compressedText))
            return string.Empty;

        return Encoding.UTF8.GetString(Decompress(Convert.FromBase64String(compressedText)));
    }

    public static byte[] Encrypt(byte[] data) => EncryptAES(data, SecurityConfig.GetTransferKey());

    public static string EncryptText(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(plainText)));
    }

    public static byte[] Decrypt(byte[] data) => DecryptAES(data, SecurityConfig.GetTransferKey());

    public static string DecryptText(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(encryptedText)));
    }

    public static byte[] MigrationEncrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = SecurityConfig.GetMigrationAesKey();
        aes.IV = SecurityConfig.GetMigrationAesIv();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public static byte[] MigrationDecrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = SecurityConfig.GetMigrationAesKey();
        aes.IV = SecurityConfig.GetMigrationAesIv();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }

    public static byte[] CompressAndEncrypt(byte[] data) => Encrypt(Compress(data));

    public static string CompressAndEncryptToBase64(byte[] data) => Convert.ToBase64String(CompressAndEncrypt(data));

    public static byte[] DecryptAndDecompress(byte[] data) => Decompress(Decrypt(data));

    public static byte[] DecryptAndDecompress(string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData))
            return Array.Empty<byte>();

        return DecryptAndDecompress(Convert.FromBase64String(encryptedData));
    }

    public static string CalculateChecksum(byte[] data) => SHA256Hash(data);

    public static bool VerifyChecksum(byte[] data, string expectedChecksum)
    {
        return string.Equals(CalculateChecksum(data), expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }

    public static byte[]? ReadFileChunk(string filePath, long offset = 0, int maxBytes = 524288)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            using var fs = OpenSequentialReadStream(filePath);
            if (offset > fs.Length)
                return null;

            fs.Seek(offset, SeekOrigin.Begin);
            var toRead = (int)Math.Min(maxBytes, fs.Length - offset);
            if (toRead <= 0)
                return null;

            if (toRead >= 2 * 1024 * 1024)
            {
                var mapped = ReadFileChunkMemoryMapped(filePath, offset, toRead);
                if (mapped != null && mapped.Length > 0)
                    return mapped;
            }

            var buffer = new byte[toRead];
            var read = fs.Read(buffer, 0, toRead);
            if (read == toRead)
                return buffer;

            Array.Resize(ref buffer, read);
            return buffer;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    public static byte[]? ReadFileSafe(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            using var fs = OpenSequentialReadStream(filePath);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            return ms.ToArray();
        }
        catch
        {
            return null;
        }
    }

    public static long GetFileLengthSafe(string filePath)
    {
        try
        {
            using var fs = OpenSequentialReadStream(filePath);
            return fs.Length;
        }
        catch
        {
            return 0;
        }
    }

    public static string ProtectDpapiStringIfNeeded(string? plainText)
    {
        var value = plainText ?? string.Empty;
        if (value.Length == 0)
            return string.Empty;

        if (value.StartsWith(DpapiPrefix, StringComparison.OrdinalIgnoreCase))
            return value;

        try
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var encrypted = ProtectedData.Protect(bytes, DpapiEntropy, DataProtectionScope.LocalMachine);
            return DpapiPrefix + Convert.ToBase64String(encrypted);
        }
        catch
        {
            return value;
        }
    }

    public static string TryUnprotectDpapiString(string? protectedValue)
    {
        var value = protectedValue ?? string.Empty;
        if (value.Length == 0)
            return string.Empty;

        if (!value.StartsWith(DpapiPrefix, StringComparison.OrdinalIgnoreCase))
            return value;

        try
        {
            var base64 = value.Substring(DpapiPrefix.Length);
            var encrypted = Convert.FromBase64String(base64);
            var plain = ProtectedData.Unprotect(encrypted, DpapiEntropy, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(plain);
        }
        catch
        {
            return protectedValue ?? string.Empty;
        }
    }

    public static string GenerateRandomHex(int length)
    {
        if (length <= 0)
            return string.Empty;

        return BytesToHex(RandomBytes((int)Math.Ceiling(length / 2.0)))[..length];
    }

    public static byte[] GenerateNonce(int sizeBytes = 12) => RandomBytes(sizeBytes);

    public static string BytesToHex(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
            builder.AppendFormat("{0:x2}", b);
        return builder.ToString();
    }

    public static byte[] HexToBytes(string hex)
    {
        if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
            return Array.Empty<byte>();

        var result = new byte[hex.Length / 2];
        for (var i = 0; i < result.Length; i++)
            result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return result;
    }

    private static byte[] NormalizeKey(byte[] key, int targetLength)
    {
        if (key.Length == targetLength)
            return key;

        var result = new byte[targetLength];
        Buffer.BlockCopy(key, 0, result, 0, Math.Min(key.Length, targetLength));
        return result;
    }

    private static byte[] RandomBytes(int length)
    {
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }

    private static FileStream OpenSequentialReadStream(string filePath)
    {
        var options = new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.ReadWrite | FileShare.Delete,
            BufferSize = 64 * 1024,
            Options = FileOptions.SequentialScan
        };
        return new FileStream(filePath, options);
    }

    private static byte[]? ReadFileChunkMemoryMapped(string filePath, long offset, int length)
    {
        if (offset < 0 || length <= 0)
            return null;

        try
        {
            using var mapping = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var view = mapping.CreateViewAccessor(offset, length, MemoryMappedFileAccess.Read);
            var buffer = new byte[length];
            view.ReadArray(0, buffer, 0, length);
            return buffer;
        }
        catch
        {
            return null;
        }
    }
}
