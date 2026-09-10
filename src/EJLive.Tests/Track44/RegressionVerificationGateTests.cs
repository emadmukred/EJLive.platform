using Microsoft.VisualStudio.TestTools.UnitTesting;
using EJLive.Core.Engine;

namespace EJLive.Tests.Track44;

[TestClass]
public sealed class RegressionVerificationGateTests
{
    private string _testSolutionRoot = null!;

    [TestInitialize]
    public void Setup()
    {
        _testSolutionRoot = Path.Combine(Path.GetTempPath(), $"ejlive-gate-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testSolutionRoot);

        // Create minimal solution structure
        Directory.CreateDirectory(Path.Combine(_testSolutionRoot, "artifacts"));
        Directory.CreateDirectory(Path.Combine(_testSolutionRoot, "docs"));
        Directory.CreateDirectory(Path.Combine(_testSolutionRoot, "src", "EJLive.Core", "Engine"));
        Directory.CreateDirectory(Path.Combine(_testSolutionRoot, "src", "EJLive.Core", "Models"));
        Directory.CreateDirectory(Path.Combine(_testSolutionRoot, "src", "EJLive.Tests"));

        // Create critical files
        File.WriteAllText(Path.Combine(_testSolutionRoot, "artifacts", "ActiveCompileMap.csv"), "Project,File\nCore,Test.cs");
        File.WriteAllText(Path.Combine(_testSolutionRoot, "artifacts", "ProjectDependencyGraph.md"), "# Deps");
        File.WriteAllText(Path.Combine(_testSolutionRoot, "src", "EJLive.Core", "EJLive.Core.csproj"),
            "<Project><Compile Include=\"Test.cs\" /></Project>");

        // Create engine files
        var engineDir = Path.Combine(_testSolutionRoot, "src", "EJLive.Core", "Engine");
        var criticalFiles = new[]
        {
            "SecureHandshakeService.cs", "FileWatcherEngine.cs", "ChunkedTransferEngine.cs",
            "IngestionPipeline.cs", "JournalOutbox.cs", "EjParserRegistry.cs",
            "NcrEjTransactionParser.cs", "HeartbeatService.cs", "SafeRemoteCommandQueue.cs",
            "InstallerEngine.cs", "SecretProtector.cs", "CorrelationEngine.cs"
        };

        foreach (var file in criticalFiles)
        {
            File.WriteAllText(Path.Combine(engineDir, file),
                $"namespace EJLive.Core.Engine; public class {Path.GetFileNameWithoutExtension(file)} {{}}");
        }

        // Create test files
        File.WriteAllText(Path.Combine(_testSolutionRoot, "src", "EJLive.Tests", "SomeTests.cs"), "");
        File.WriteAllText(Path.Combine(_testSolutionRoot, "src", "EJLive.Tests", "MoreTests.cs"), "");
        File.WriteAllText(Path.Combine(_testSolutionRoot, "src", "EJLive.Tests", "IntegrationTests.cs"), "");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testSolutionRoot))
            Directory.Delete(_testSolutionRoot, recursive: true);
    }

    [TestMethod]
    public void RunAllChecks_WithCompleteStructure_ReturnsReport()
    {
        // Add docs with enough lines
        var statusLines = new string[55];
        statusLines[0] = "Service,Status";
        for (int i = 1; i < 55; i++)
            statusLines[i] = $"Service{i},Active";
        File.WriteAllLines(Path.Combine(_testSolutionRoot, "docs", "12-service-activation-status.csv"), statusLines);

        var gate = new RegressionVerificationGate(_testSolutionRoot);
        var report = gate.RunAllChecks();

        Assert.IsNotNull(report);
        Assert.IsTrue(report.Checks.Count >= 5);
        Assert.IsTrue(report.PassCount > 0);
    }

    [TestMethod]
    public void RunAllChecks_MissingArtifacts_FailsRelevantChecks()
    {
        // Remove artifacts
        Directory.Delete(Path.Combine(_testSolutionRoot, "artifacts"), recursive: true);

        var gate = new RegressionVerificationGate(_testSolutionRoot);
        var report = gate.RunAllChecks();

        Assert.IsFalse(report.AllPassed);
        var mapCheck = report.Checks.FirstOrDefault(c => c.Name.Contains("ActiveCompileMap"));
        Assert.IsNotNull(mapCheck);
        Assert.IsFalse(mapCheck.Passed);
    }

    [TestMethod]
    public void RunAllChecks_CriticalFilesPresent_Passes()
    {
        // Enough docs
        var statusLines = new string[55];
        statusLines[0] = "Service,Status";
        for (int i = 1; i < 55; i++)
            statusLines[i] = $"Service{i},Active";
        File.WriteAllLines(Path.Combine(_testSolutionRoot, "docs", "12-service-activation-status.csv"), statusLines);

        var gate = new RegressionVerificationGate(_testSolutionRoot);
        var report = gate.RunAllChecks();

        var engineCheck = report.Checks.FirstOrDefault(c => c.Name.Contains("Critical engine"));
        Assert.IsNotNull(engineCheck);
        Assert.IsTrue(engineCheck.Passed, $"Critical engine files check failed: {engineCheck.Detail}");
    }
}
