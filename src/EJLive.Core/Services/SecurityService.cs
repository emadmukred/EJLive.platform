using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EJLive.Core.Services
{
    /// <summary>
    /// Security service for handling encryption, decryption, and security operations
    /// </summary>
    public class SecurityService
    {
        /// <summary>
        /// Encrypts data using AES-256 encryption
        /// </summary>
        /// <param name="data">Data to encrypt</param>
        /// <param name="key">Encryption key</param>
        /// <returns>Encrypted data</returns>
        public static byte[] EncryptData(byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.GenerateIV();
                
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Decrypts data using AES-256 decryption
        /// </summary>
        /// <param name="data">Data to decrypt</param>
        /// <param name="key">Decryption key</param>
        /// <param name="iv">Initialization vector</param>
        /// <returns>Decrypted data</returns>
        public static byte[] DecryptData(byte[] data, string key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(data))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var result = new MemoryStream())
                {
                    cs.CopyTo(result);
                    return result.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Calculates SHA-256 checksum for data integrity verification
        /// </summary>
        /// <param name="data">Data to calculate checksum for</param>
        /// <returns>Hexadecimal representation of the checksum</returns>
        public static string CalculateChecksum(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        
        /// <summary>
        /// Generates a random encryption key
        /// </summary>
        /// <returns>Random encryption key</returns>
        public static string GenerateEncryptionKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] key = new byte[32]; // 256 bits
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }
        
        /// <summary>
        /// Derives a key from a password using PBKDF2
        /// </summary>
        /// <param name="password">Password to derive key from</param>
        /// <param name="salt">Salt for key derivation</param>
        /// <param name="iterations">Number of iterations</param>
        /// <returns>Derived key</returns>
        public static string DeriveKeyFromPassword(string password, byte[] salt, int iterations = 10000)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] key = pbkdf2.GetBytes(32); // 256 bits
                return Convert.ToBase64String(key);
            }
        }
        
        /// <summary>
        /// Generates a random salt
        /// </summary>
        /// <returns>Random salt</returns>
        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
        
        /// <summary>
        /// Encrypts a file with AES-256 encryption
        /// </summary>
        /// <param name="inputPath">Path to input file</param>
        /// <param name="outputPath">Path to output file</param>
        /// <param name="key">Encryption key</param>
        public static void EncryptFile(string inputPath, string outputPath, string key)
        {
            byte[] fileContent = File.ReadAllBytes(inputPath);
            byte[] encryptedContent = EncryptData(fileContent, key);
            File.WriteAllBytes(outputPath, encryptedContent);
        }
        
        /// <summary>
        /// Decrypts a file with AES-256 decryption
        /// </summary>
        /// <param name="inputPath">Path to input file</param>
        /// <param name="outputPath">Path to output file</param>
        /// <param name="key">Decryption key</param>
        /// <param name="iv">Initialization vector</param>
        public static void DecryptFile(string inputPath, string outputPath, string key, byte[] iv)
        {
            byte[] fileContent = File.ReadAllBytes(inputPath);
            byte[] decryptedContent = DecryptData(fileContent, key, iv);
            File.WriteAllBytes(outputPath, decryptedContent);
        }
    }
}