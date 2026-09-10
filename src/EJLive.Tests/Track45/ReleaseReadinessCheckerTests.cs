using Microsoft.VisualStudio.TestTools.UnitTesting;
using EJLive.Core.Engine;

namespace EJLive.Tests.Track45;

[TestClass]
public sealed class ReleaseReadinessCheckerTests
{
    private string _testSolutionRoot = null!;

    [TestInitialize]
    public void Setup()
    {
        _testSolutionRoot = Path.Combine(Path.GetTempPath(), $"ejlive-release-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testSolutionRoot);

        // Create solution file
        File.WriteAllText(Path.Combine(_testSolutionRoot, "EJLive.Unified.sln"), "");

        // Create Core project structure
        var coreDir = Path.Combine(_testSolutionRoot, "src", "EJLive.Core");
        var engineDir = Path.Combine(coreDir, "Engine");
        var xfsDir = Path.Combine(coreDir, "Xfs", "Adapters");
        Directory.CreateDirectory(engineDir);
        Directory.CreateDirectory(xfsDir);
        Directory.CreateDirectory(Path.Combine(_testSolutionRoot, "src", "EJLive.Tests"));
        Directory.CreateDirectory(Path.Combine(_testSolutionRoot, "artifacts"));
        Directory.CreateDirectory(Path.Combine(_testSolutionRoot, "docs"));

        // csproj with enough compile entries
        var csprojLines = "<Project>\n" + string.Join("\n",
            Enumerable.Range(1, 35).Select(i => $"<Compile Include=\"File{i}.cs\" />")) + "\n</Project>";
        File.WriteAllText(Path.Combine(coreDir, "EJLive.Core.csproj"), csprojLines);

        // Parsers
        var parsers = new[] { "NcrEjTransactionParser.cs", "GrgEjTransactionParser.cs",
            "WincorEjTransactionParser.cs", "DieboldEjTransactionParser.cs",
            "HyosungEjTransactionParser.cs", "CashDistributionParser.cs", "EjParserRegistry.cs" };
        foreach (var p in parsers) File.WriteAllText(Path.Combine(engineDir, p), "");

        // Security
        var security = new[] { "SecretProtector.cs", "SecureHandshakeService.cs",
            "LogRedactionEngine.cs", "SafeRemoteCommandQueue.cs", "SafeRemoteCommandExecutor.cs" };
        foreach (var s in security) File.WriteAllText(Path.Combine(engineDir, s), "");

        // Network
        var network = new[] { "ChunkedTransferEngine.cs", "IngestionPipeline.cs",
            "NetworkEngine.cs", "HeartbeatService.cs", "TimeSyncService.cs" };
        foreach (var n in network) File.WriteAllText(Path.Combine(engineDir, n), "");

        // Installer
        File.WriteAllText(Path.Combine(engineDir, "InstallerEngine.cs"), "");
        File.WriteAllText(Path.Combine(engineDir, "WindowsServiceRegistration.cs"), "");
        File.WriteAllText(Path.Combine(engineDir, "DeploymentRollbackGuard.cs"), "");

        // XFS Adapters
        File.WriteAllText(Path.Combine(xfsDir, "NcrXfsAdapter.cs"), "");
        File.WriteAllText(Path.Combine(xfsDir, "GrgXfsAdapter.cs"), "");
        File.WriteAllText(Path.Combine(xfsDir, "WincorXfsAdapter.cs"), "");

        // Tests
        File.WriteAllText(Path.Combine(_testSolutionRoot, "src", "EJLive.Tests", "Test1Tests.cs"), "");
        File.WriteAllText(Path.Combine(_testSolutionRoot, "src", "EJLive.Tests", "Test2Tests.cs"), "");
        File.WriteAllText(Path.Combine(_testSolutionRoot, "src", "EJLive.Tests", "Test3Tests.cs"), "");

        // Artifacts
        File.WriteAllText(Path.Combine(_testSolutionRoot, "artifacts", "build.log"), "");

        // Docs
        for (int i = 1; i <= 6; i++)
            File.WriteAllText(Path.Combine(_testSolutionRoot, "docs", $"doc{i}.md"), "");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testSolutionRoot))
            Directory.Delete(_testSolutionRoot, recursive: true);
    }

    [TestMethod]
    public void Assess_FullyReady_ReturnsHighScore()
    {
        var checker = new ReleaseReadinessChecker(_testSolutionRoot);
        var report = checker.Assess();

        Assert.IsNotNull(report);
        Assert.IsTrue(report.ReadinessScore >= 80, $"Expected score >= 80, got {report.ReadinessScore}");
        Assert.IsTrue(report.OverallReady);
    }

    [TestMethod]
    public void Assess_MissingSolution_IsBlocked()
    {
        File.Delete(Path.Combine(_testSolutionRoot, "EJLive.Unified.sln"));

        var checker = new ReleaseReadinessChecker(_testSolutionRoot);
        var report = checker.Assess();

        var slnItem = report.Items.FirstOrDefault(i => i.Name == "Solution file");
        Assert.IsNotNull(slnItem);
        Assert.AreEqual(ReadinessStatus.Blocked, slnItem.Status);
        Assert.IsFalse(report.OverallReady);
    }

    [TestMethod]
    public void Assess_AllParsers_ReportsReady()
    {
        var checker = new ReleaseReadinessChecker(_testSolutionRoot);
        var report = checker.Assess();

        var parserItem = report.Items.FirstOrDefault(i => i.Name == "Vendor parsers");
        Assert.IsNotNull(parserItem);
        Assert.AreEqual(ReadinessStatus.Ready, parserItem.Status);
    }

    [TestMethod]
    public void Assess_AllSecurity_ReportsReady()
    {
        var checker = new ReleaseReadinessChecker(_testSolutionRoot);
        var report = checker.Assess();

        var secItem = report.Items.FirstOrDefault(i => i.Name == "Security infrastructure");
        Assert.IsNotNull(secItem);
        Assert.AreEqual(ReadinessStatus.Ready, secItem.Status);
    }

    [TestMethod]
    public void Assess_InstallerAndRollback_ReportsReady()
    {
        var checker = new ReleaseReadinessChecker(_testSolutionRoot);
        var report = checker.Assess();

        var installItem = report.Items.FirstOrDefault(i => i.Name.Contains("Installer"));
        Assert.IsNotNull(installItem);
        Assert.AreEqual(ReadinessStatus.Ready, installItem.Status);
    }
}
