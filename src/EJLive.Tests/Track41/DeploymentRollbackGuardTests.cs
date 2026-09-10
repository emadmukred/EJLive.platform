using Microsoft.VisualStudio.TestTools.UnitTesting;
using EJLive.Core.Engine;

namespace EJLive.Tests.Track41;

[TestClass]
public sealed class DeploymentRollbackGuardTests
{
    private string _testDir = null!;
    private string _checkpointDir = null!;
    private string _auditLog = null!;

    [TestInitialize]
    public void Setup()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"ejlive-test-{Guid.NewGuid():N}");
        _checkpointDir = Path.Combine(_testDir, "checkpoints");
        _auditLog = Path.Combine(_testDir, "audit.log");
        Directory.CreateDirectory(_testDir);

        // Create sample install files
        var installRoot = Path.Combine(_testDir, "install");
        Directory.CreateDirectory(installRoot);
        File.WriteAllText(Path.Combine(installRoot, "app.dll"), "binary-content");
        File.WriteAllText(Path.Combine(installRoot, "config.json"), "{}");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    [TestMethod]
    public async Task CreateCheckpoint_CapturesAllFiles()
    {
        var guard = new DeploymentRollbackGuard(_checkpointDir, _auditLog);
        var installRoot = Path.Combine(_testDir, "install");

        var checkpoint = await guard.CreateCheckpointAsync(installRoot, "1.0.0");

        Assert.IsNotNull(checkpoint);
        Assert.AreEqual("1.0.0", checkpoint.Version);
        Assert.AreEqual(2, checkpoint.TotalFiles);
        Assert.IsTrue(checkpoint.FileHashes.ContainsKey("app.dll"));
        Assert.IsTrue(checkpoint.FileHashes.ContainsKey("config.json"));
    }

    [TestMethod]
    public async Task ValidateDeployment_NoChanges_IsIntact()
    {
        var guard = new DeploymentRollbackGuard(_checkpointDir, _auditLog);
        var installRoot = Path.Combine(_testDir, "install");

        var checkpoint = await guard.CreateCheckpointAsync(installRoot, "1.0.0");
        var result = await guard.ValidateDeploymentAsync(checkpoint);

        Assert.IsTrue(result.IsIntact);
        Assert.AreEqual(0, result.ModifiedFiles.Count);
        Assert.AreEqual(0, result.RemovedFiles.Count);
        Assert.AreEqual(0, result.AddedFiles.Count);
    }

    [TestMethod]
    public async Task ValidateDeployment_FileModified_DetectsChange()
    {
        var guard = new DeploymentRollbackGuard(_checkpointDir, _auditLog);
        var installRoot = Path.Combine(_testDir, "install");

        var checkpoint = await guard.CreateCheckpointAsync(installRoot, "1.0.0");

        // Modify a file
        File.WriteAllText(Path.Combine(installRoot, "config.json"), "{\"updated\": true}");

        var result = await guard.ValidateDeploymentAsync(checkpoint);

        Assert.IsFalse(result.IsIntact);
        Assert.AreEqual(1, result.ModifiedFiles.Count);
        Assert.IsTrue(result.ModifiedFiles.Contains("config.json"));
    }

    [TestMethod]
    public async Task ValidateDeployment_FileAdded_DetectsAddition()
    {
        var guard = new DeploymentRollbackGuard(_checkpointDir, _auditLog);
        var installRoot = Path.Combine(_testDir, "install");

        var checkpoint = await guard.CreateCheckpointAsync(installRoot, "1.0.0");

        // Add a new file
        File.WriteAllText(Path.Combine(installRoot, "new-module.dll"), "new-binary");

        var result = await guard.ValidateDeploymentAsync(checkpoint);

        Assert.AreEqual(1, result.AddedFiles.Count);
        Assert.IsTrue(result.AddedFiles.Contains("new-module.dll"));
    }

    [TestMethod]
    public async Task ListCheckpoints_ReturnsOrderedByDate()
    {
        var guard = new DeploymentRollbackGuard(_checkpointDir, _auditLog);
        var installRoot = Path.Combine(_testDir, "install");

        await guard.CreateCheckpointAsync(installRoot, "1.0.0");
        await Task.Delay(10);
        await guard.CreateCheckpointAsync(installRoot, "2.0.0");

        var checkpoints = await guard.ListCheckpointsAsync();

        Assert.AreEqual(2, checkpoints.Count);
        Assert.AreEqual("2.0.0", checkpoints[0].Version); // Most recent first
        Assert.AreEqual("1.0.0", checkpoints[1].Version);
    }
}
