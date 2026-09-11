using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EJLive.Core.Models;

namespace EJLive.Core.Server
{
    public class ActiveSession
        {
            public string AtmId { get; set; } = string.Empty;
            public string SessionId { get; set; } = string.Empty;
            public DateTime StartedAtUtc { get; set; }
            public DateTime LastActivityUtc { get; set; }
        }
    public partial class ActiveSession
        {
            public string AtmId { get; set; } = string.Empty;
            public string SessionId { get; set; } = string.Empty;
            public DateTime StartedAtUtc { get; set; }
            public DateTime LastActivityUtc { get; set; }
        }
    public class IngestionResult
        {
            public string AtmId { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string? StagingPath { get; set; }
            public string? ArchivePath { get; set; }
            public bool ChecksumVerified { get; set; }
            public bool Success { get; set; }
            public string? Error { get; set; }
        }
    public partial class IngestionResult
        {
            public string AtmId { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string? StagingPath { get; set; }
            public string? ArchivePath { get; set; }
            public bool ChecksumVerified { get; set; }
            public bool Success { get; set; }
            public string? Error { get; set; }
        }
    public partial public public class ServerIngestionPipeline
        {
            private readonly string _stagingRoot;
            private readonly string _archiveRoot;
            private readonly Dictionary<string, ActiveSession> _sessions = new();
            public int Count => _sessions.Count;
            public ServerIngestionPipeline(string? stagingRoot = null, string? archiveRoot = null)
            {
            public string AtmId { get; set; }
            public string FileName { get; set; }
            public string? StagingPath { get; set; }
            public string? ArchivePath { get; set; }
            public bool ChecksumVerified { get; set; }
            public bool Success { get; set; }
            public string? Error { get; set; }
            public string SessionId { get; set; }
            public DateTime StartedAtUtc { get; set; }
            public DateTime LastActivityUtc { get; set; }
            public async Task<IngestionResult> IngestAsync(string atmId, string fileName, byte[] data, string expectedChecksum)
            {
            private static string ComputeSha256(byte[] data)
            {
            public ActiveSession Register(string atmId, string sessionId)
            {
            public ActiveSession? Get(string atmId) =>
            public void Remove(string atmId) =>
            public IEnumerable<ActiveSession> GetAll() =>
        }
    
        public partial public public class IngestionResult
        {
            public string AtmId { get; set; }
            public string FileName { get; set; }
            public string? StagingPath { get; set; }
            public string? ArchivePath { get; set; }
            public bool ChecksumVerified { get; set; }
            public bool Success { get; set; }
            public string? Error { get; set; }
        }
    
        public partial public public class SessionRegistry
        {
            private readonly Dictionary<string, ActiveSession> _sessions = new();
            public int Count => _sessions.Count;
            public string AtmId { get; set; }
            public string SessionId { get; set; }
            public DateTime StartedAtUtc { get; set; }
            public DateTime LastActivityUtc { get; set; }
            public ActiveSession Register(string atmId, string sessionId)
            {
            public ActiveSession? Get(string atmId) =>
            public void Remove(string atmId) =>
            public IEnumerable<ActiveSession> GetAll() =>
        }
    
        public partial public public class ActiveSession
        {
            public string AtmId { get; set; }
            public string SessionId { get; set; }
            public DateTime StartedAtUtc { get; set; }
            public DateTime LastActivityUtc { get; set; }
        }
    
    }
    public partial public class ServerIngestionPipeline
        {
            private readonly string _stagingRoot;
            private readonly string _archiveRoot;
            public ServerIngestionPipeline(string? stagingRoot = null, string? archiveRoot = null)
            {
            public async Task<IngestionResult> IngestAsync(string atmId, string fileName, byte[] data, string expectedChecksum)
            {
            private static string ComputeSha256(byte[] data)
            {
        }
    
        public partial public class IngestionResult
        {
            public string AtmId { get; set; }
            public string FileName { get; set; }
            public string? StagingPath { get; set; }
            public string? ArchivePath { get; set; }
            public bool ChecksumVerified { get; set; }
            public bool Success { get; set; }
            public string? Error { get; set; }
        }
    
        public partial public class SessionRegistry
        {
            private readonly Dictionary<string, ActiveSession> _sessions = new();
            public int Count => _sessions.Count;
            public ActiveSession Register(string atmId, string sessionId)
            {
            public ActiveSession? Get(string atmId) =>
            public void Remove(string atmId) =>
            public IEnumerable<ActiveSession> GetAll() =>
        }
    
        public partial public class ActiveSession
        {
            public string AtmId { get; set; }
            public string SessionId { get; set; }
            public DateTime StartedAtUtc { get; set; }
            public DateTime LastActivityUtc { get; set; }
        }
    
    }

    // Class: ActiveSession (from 3 sources)
        public partial class ActiveSession
        {
            // --- Properties ---
                    public string AtmId { get; set; } = string.Empty;
    
                    public string SessionId { get; set; } = string.Empty;
    
                    public DateTime StartedAtUtc { get; set; }
    
                    public DateTime LastActivityUtc { get; set; }
    
    
        }
    // Class: IngestionResult (from 3 sources)
        public partial class IngestionResult
        {
            // --- Properties ---
                    public string AtmId { get; set; } = string.Empty;
    
                    public string FileName { get; set; } = string.Empty;
    
                    public string? StagingPath { get; set; }
    
                    public string? ArchivePath { get; set; }
    
                    public bool ChecksumVerified { get; set; }
    
                    public bool Success { get; set; }
    
                    public string? Error { get; set; }
    
    
        }
    // Class: ServerIngestionPipeline (from 1 sources)
        public partial class ServerIngestionPipeline
        {
            // --- Constants & Fields ---
                    private readonly string _stagingRoot;
    
                    private readonly string _archiveRoot;
    
    
            // --- Constructors ---
                    public ServerIngestionPipeline(string? stagingRoot = null, string? archiveRoot = null)
                    {
                        _stagingRoot = stagingRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Server", "Staging");
                        _archiveRoot = archiveRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Server", "Archive");
                        Directory.CreateDirectory(_stagingRoot);
                        Directory.CreateDirectory(_archiveRoot);
                    }
    
    
            // --- Methods ---
                    public async Task<IngestionResult> IngestAsync(string atmId, string fileName, byte[] data, string expectedChecksum)
                    {
                        var result = new IngestionResult { AtmId = atmId, FileName = fileName };
    
                        // Stage
                        var stagingPath = Path.Combine(_stagingRoot, $"{atmId}_{fileName}_{DateTime.UtcNow:yyyyMMddHHmmss}");
                        await File.WriteAllBytesAsync(stagingPath, data);
                        result.StagingPath = stagingPath;
    
                        // Verify SHA256
                        var actualChecksum = ComputeSha256(data);
                        if (!string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Success = false;
                            result.Error = $"SHA256 mismatch: expected={expectedChecksum[..Math.Min(12, expectedChecksum.Length)]}..., actual={actualChecksum[..Math.Min(12, actualChecksum.Length)]}...";
                            return result;
                        }
                        result.ChecksumVerified = true;
    
                        // Archive
                        var archiveDir = Path.Combine(_archiveRoot, DateTime.UtcNow.ToString("yyyy-MM"), atmId);
                        Directory.CreateDirectory(archiveDir);
                        var archivePath = Path.Combine(archiveDir, fileName);
                        File.Move(stagingPath, archivePath, overwrite: true);
                        result.ArchivePath = archivePath;
                        result.Success = true;
    
                        return result;
                    }
    
                    private static string ComputeSha256(byte[] data)
                    {
                        var hash = SHA256.HashData(data);
                        return Convert.ToHexString(hash).ToLowerInvariant();
                    }
    
    
        }
    /// <summary>
        /// Server ingestion pipeline: Receive → Staging → SHA256 Verify → Archive → Index.
        /// </summary>
        public class ServerIngestionPipeline
        {
            private readonly string _stagingRoot;
            private readonly string _archiveRoot;
    
            public ServerIngestionPipeline(string? stagingRoot = null, string? archiveRoot = null)
            {
                _stagingRoot = stagingRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Server", "Staging");
                _archiveRoot = archiveRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Server", "Archive");
                Directory.CreateDirectory(_stagingRoot);
                Directory.CreateDirectory(_archiveRoot);
            }
    
            public async Task<IngestionResult> IngestAsync(string atmId, string fileName, byte[] data, string expectedChecksum)
            {
                var result = new IngestionResult { AtmId = atmId, FileName = fileName };
    
                // Stage
                var stagingPath = Path.Combine(_stagingRoot, $"{atmId}_{fileName}_{DateTime.UtcNow:yyyyMMddHHmmss}");
                await File.WriteAllBytesAsync(stagingPath, data);
                result.StagingPath = stagingPath;
    
                // Verify SHA256
                var actualChecksum = ComputeSha256(data);
                if (!string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase))
                {
                    result.Success = false;
                    result.Error = $"SHA256 mismatch: expected={expectedChecksum[..Math.Min(12, expectedChecksum.Length)]}..., actual={actualChecksum[..Math.Min(12, actualChecksum.Length)]}...";
                    return result;
                }
                result.ChecksumVerified = true;
    
                // Archive
                var archiveDir = Path.Combine(_archiveRoot, DateTime.UtcNow.ToString("yyyy-MM"), atmId);
                Directory.CreateDirectory(archiveDir);
                var archivePath = Path.Combine(archiveDir, fileName);
                File.Move(stagingPath, archivePath, overwrite: true);
                result.ArchivePath = archivePath;
                result.Success = true;
    
                return result;
            }
    
            private static string ComputeSha256(byte[] data)
            {
                var hash = SHA256.HashData(data);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }
    public partial class ServerIngestionPipeline
        {
            private readonly string _stagingRoot;
            private readonly string _archiveRoot;
            public ServerIngestionPipeline(string? stagingRoot = null, string? archiveRoot = null)
            {
                _stagingRoot = stagingRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Server", "Staging");
                _archiveRoot = archiveRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EJLive", "Server", "Archive");
                Directory.CreateDirectory(_stagingRoot);
                Directory.CreateDirectory(_archiveRoot);
            }
            public async Task<IngestionResult> IngestAsync(string atmId, string fileName, byte[] data, string expectedChecksum)
            {
                var result = new IngestionResult { AtmId = atmId, FileName = fileName };
                // Stage
                var stagingPath = Path.Combine(_stagingRoot, $"{atmId}_{fileName}_{DateTime.UtcNow:yyyyMMddHHmmss}");
                await File.WriteAllBytesAsync(stagingPath, data);
                result.StagingPath = stagingPath;
                // Verify SHA256
                var actualChecksum = ComputeSha256(data);
                if (!string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase))
                {
                    result.Success = false;
                    result.Error = $"SHA256 mismatch: expected={expectedChecksum[..Math.Min(12, expectedChecksum.Length)]}..., actual={actualChecksum[..Math.Min(12, actualChecksum.Length)]}...";
                    return result;
                }
                result.ChecksumVerified = true;
                // Archive
                var archiveDir = Path.Combine(_archiveRoot, DateTime.UtcNow.ToString("yyyy-MM"), atmId);
                Directory.CreateDirectory(archiveDir);
                var archivePath = Path.Combine(archiveDir, fileName);
                File.Move(stagingPath, archivePath, overwrite: true);
                result.ArchivePath = archivePath;
                result.Success = true;
                return result;
            }
            private static string ComputeSha256(byte[] data)
            {
                var hash = SHA256.HashData(data);
                return Convert.ToHexString(hash).ToLowerInvariant();
            }
        }
    // Class: SessionRegistry (from 1 sources)
        public partial class SessionRegistry
        {
            // --- Constants & Fields ---
                    private readonly Dictionary<string, ActiveSession> _sessions = new();
    
    
            // --- Properties ---
                    public int Count => _sessions.Count;
    
    
            // --- Methods ---
                    public ActiveSession Register(string atmId, string sessionId)
                    {
                        var session = new ActiveSession
                        {
                            AtmId = atmId,
                            SessionId = sessionId,
                            StartedAtUtc = DateTime.UtcNow,
                            LastActivityUtc = DateTime.UtcNow
                        };
                        _sessions[atmId] = session;
                        return session;
                    }
    
                    public ActiveSession? Get(string atmId) =>
                        _sessions.TryGetValue(atmId, out var s) ? s : null;
    
                    public void Remove(string atmId) => _sessions.Remove(atmId);
    
                    public IEnumerable<ActiveSession> GetAll() => _sessions.Values;
    
    
        }
    /// <summary>
        /// Registry of active transfer sessions from ATM clients.
        /// </summary>
        public class SessionRegistry
        {
            private readonly Dictionary<string, ActiveSession> _sessions = new();
    
            public ActiveSession Register(string atmId, string sessionId)
            {
                var session = new ActiveSession
                {
                    AtmId = atmId,
                    SessionId = sessionId,
                    StartedAtUtc = DateTime.UtcNow,
                    LastActivityUtc = DateTime.UtcNow
                };
                _sessions[atmId] = session;
                return session;
            }
    
            public ActiveSession? Get(string atmId) =>
                _sessions.TryGetValue(atmId, out var s) ? s : null;
    
            public void Remove(string atmId) => _sessions.Remove(atmId);
            public IEnumerable<ActiveSession> GetAll() => _sessions.Values;
            public int Count => _sessions.Count;
        }
    public partial class SessionRegistry
        {
            private readonly Dictionary<string, ActiveSession> _sessions = new();
            public int Count => _sessions.Count;
            public ActiveSession Register(string atmId, string sessionId)
            {
                var session = new ActiveSession
                {
                    AtmId = atmId,
                    SessionId = sessionId,
                    StartedAtUtc = DateTime.UtcNow,
                    LastActivityUtc = DateTime.UtcNow
                };
                _sessions[atmId] = session;
                return session;
            }
            public ActiveSession? Get(string atmId) =>
                _sessions.TryGetValue(atmId, out var s) ? s : null;
            public void Remove(string atmId) => _sessions.Remove(atmId);
            public IEnumerable<ActiveSession> GetAll() => _sessions.Values;
        }
}
