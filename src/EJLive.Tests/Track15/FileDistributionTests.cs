using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track15;

[TestClass]
public class FileDistributionTests
{
    private static FileDistributionEngine CreateEngine(
        IFileSystem? fileSystem = null,
        IFileDistributionPolicy? policy = null,
        IStagingArea? staging = null)
    {
        return new FileDistributionEngine(
            fileSystem ?? new TestFileSystem(),
            policy ?? new TestPolicy(allowed: true),
            staging ?? new TestStagingArea());
    }

    [TestMethod]
    public async Task DistributeAsync_PathTraversal_ReturnsFailure()
    {
        // Arrange
        var request = new FileDistributionRequest
        {
            RequestId = Guid.NewGuid(),
            AtmId = "ATM001",
            SourcePath = "/server/files/update.exe",
            DestinationPath = "/atm/allowed/../etc/malicious.exe",
            ExpectedSha256 = "abcd",
            AllowedDestinationFolder = "/atm/allowed"
        };

        var engine = CreateEngine();

        // Act
        var result = await engine.DistributeAsync(request, Array.Empty<byte>());

        // Assert
        Assert.AreEqual(DistributionStatus.Failed, result.Status);
        Assert.IsNotNull(result.Message);
        StringAssert.Contains(result.Message, "Path traversal");
    }

    [TestMethod]
    public async Task DistributeAsync_ChecksumMismatch_ReturnsFailure()
    {
        // Arrange
        var request = new FileDistributionRequest
        {
            RequestId = Guid.NewGuid(),
            AtmId = "ATM001",
            SourcePath = "/server/files/update.exe",
            DestinationPath = "/atm/allowed/update.exe",
            ExpectedSha256 = "0000000000000000000000000000000000000000000000000000000000000000",
            AllowedDestinationFolder = "/atm/allowed"
        };

        var engine = CreateEngine();

        // Act
        var result = await engine.DistributeAsync(request, new byte[] { 0x01, 0x02, 0x03 });

        // Assert
        Assert.AreEqual(DistributionStatus.Failed, result.Status);
        StringAssert.Contains(result.Message, "checksum mismatch");
    }

    [TestMethod]
    public async Task DistributeAsync_DestinationMissing_ReturnsFailure()
    {
        // Arrange
        var fs = new TestFileSystem(exists: false, writable: false);
        var request = new FileDistributionRequest
        {
            RequestId = Guid.NewGuid(),
            AtmId = "ATM001",
            SourcePath = "/server/files/update.exe",
            DestinationPath = "/atm/nonexistent/update.exe",
            ExpectedSha256 = ComputeSha256(new byte[] { 0x01 }),
            AllowedDestinationFolder = "/atm/nonexistent"
        };

        var engine = CreateEngine(fileSystem: fs);

        // Act
        var result = await engine.DistributeAsync(request, new byte[] { 0x01 });

        // Assert
        Assert.AreEqual(DistributionStatus.Failed, result.Status);
        StringAssert.Contains(result.Message, "Destination directory does not exist");
    }

    [TestMethod]
    public async Task DistributeAsync_InsufficientPermissions_ReturnsFailure()
    {
        // Arrange
        var fs = new TestFileSystem(exists: true, writable: false);
        var request = new FileDistributionRequest
        {
            RequestId = Guid.NewGuid(),
            AtmId = "ATM001",
            SourcePath = "/server/files/update.exe",
            DestinationPath = "/atm/allowed/update.exe",
            ExpectedSha256 = ComputeSha256(new byte[] { 0x01 }),
            AllowedDestinationFolder = "/atm/allowed"
        };

        var engine = CreateEngine(fileSystem: fs);

        // Act
        var result = await engine.DistributeAsync(request, new byte[] { 0x01 });

        // Assert
        Assert.AreEqual(DistributionStatus.Failed, result.Status);
        StringAssert.Contains(result.Message, "Insufficient permissions");
    }

    // Test helpers

    private static string ComputeSha256(byte[] data)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha.ComputeHash(data);
        var sb = new System.Text.StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    private class TestFileSystem : IFileSystem
    {
        private readonly bool _exists;
        private readonly bool _writable;
        public List<(string Path, byte[] Data)> WrittenFiles { get; } = new();

        public TestFileSystem(bool exists = true, bool writable = true)
        {
            _exists = exists;
            _writable = writable;
        }

        public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

        public bool DirectoryExists(string path) => _exists;

        public bool FileExists(string path) => false;

        public bool HasWritePermission(string path) => _writable;

        public Task WriteAllBytesAsync(string path, byte[] data, CancellationToken cancellationToken = default)
        {
            WrittenFiles.Add((path, data));
            return Task.CompletedTask;
        }

        public void DeleteFile(string path) { }
    }

    private class TestPolicy : IFileDistributionPolicy
    {
        private readonly bool _allowed;

        public TestPolicy(bool allowed)
        {
            _allowed = allowed;
        }

        public bool IsDestinationAllowed(string destinationPath) => _allowed;
    }

    private class TestStagingArea : IStagingArea
    {
        public Task<string> StageAsync(Guid requestId, byte[] content, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"/staging/{requestId}");
        }
    }
}
