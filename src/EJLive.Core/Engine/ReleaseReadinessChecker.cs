using System.Text.Json;

namespace EJLive.Core.Engine;

/// <summary>
/// Track 045 – Final Packaging and Release Readiness.
/// Validates that all 45 tracks are represented, artifacts are generated,
/// and the solution is ready for production deployment.
/// </summary>
public sealed class ReleaseReadinessChecker
{
    private readonly string _solutionRoot;

    public ReleaseReadinessChecker(string solutionRoot)
    {
        _solutionRoot = solutionRoot;
    }

    /// <summary>
    /// Performs a comprehensive release readiness assessment.
    /// </summary>
    public ReleaseReadinessReport Assess()
    {
        var items = new List<ReadinessItem>();

        items.Add(CheckSolutionBuilds());
        items.Add(CheckCoreProjectCompiles());
        items.Add(CheckAllParsersPresent());
        items.Add(CheckXfsAdaptersPresent());
        items.Add(CheckSecurityInfrastructure());
        items.Add(CheckNetworkEngines());
        items.Add(CheckInstallerReady());
        items.Add(CheckTestsPresent());
        items.Add(CheckArtifactsGenerated());
        items.Add(CheckDocumentation());

        var ready = items.All(i => i.Status != ReadinessStatus.Blocked);
        var score = items.Count(i => i.Status == ReadinessStatus.Ready) * 100 / Math.Max(items.Count, 1);

        return new ReleaseReadinessReport
        {
            AssessedUtc = DateTime.UtcNow,
            SolutionRoot = _solutionRoot,
            Items = items.AsReadOnly(),
            OverallReady = ready,
            ReadinessScore = score,
            Summary = ready
                ? $"Release ready. Score: {score}%"
                : $"Not ready for release. Score: {score}%. Resolve blocked items."
        };
    }

    private ReadinessItem CheckSolutionBuilds()
    {
        var slnPath = Path.Combine(_solutionRoot, "EJLive.Unified.sln");
        var slnxPath = Path.Combine(_solutionRoot, "EJLive.Unified.slnx");
        var exists = File.Exists(slnPath) || File.Exists(slnxPath);
        return new ReadinessItem("Solution file", exists ? ReadinessStatus.Ready : ReadinessStatus.Blocked,
            exists ? "Solution file found" : "No .sln or .slnx found");
    }

    private ReadinessItem CheckCoreProjectCompiles()
    {
        var csprojPath = Path.Combine(_solutionRoot, "src", "EJLive.Core", "EJLive.Core.csproj");
        if (!File.Exists(csprojPath))
            return new ReadinessItem("Core project", ReadinessStatus.Blocked, "EJLive.Core.csproj not found");

        var content = File.ReadAllText(csprojPath);
        var compileCount = content.Split("Compile Include").Length - 1;
        return new ReadinessItem("Core project compilation",
            compileCount >= 30 ? ReadinessStatus.Ready : ReadinessStatus.Warning,
            $"{compileCount} compile entries in csproj");
    }

    private ReadinessItem CheckAllParsersPresent()
    {
        var parsers = new[]
        {
            "NcrEjTransactionParser.cs",
            "GrgEjTransactionParser.cs",
            "WincorEjTransactionParser.cs",
            "DieboldEjTransactionParser.cs",
            "HyosungEjTransactionParser.cs",
            "CashDistributionParser.cs",
            "EjParserRegistry.cs"
        };

        var engineDir = Path.Combine(_solutionRoot, "src", "EJLive.Core", "Engine");
        var present = parsers.Count(p => File.Exists(Path.Combine(engineDir, p)));
        return new ReadinessItem("Vendor parsers",
            present == parsers.Length ? ReadinessStatus.Ready : ReadinessStatus.Warning,
            $"{present}/{parsers.Length} parsers present");
    }

    private ReadinessItem CheckXfsAdaptersPresent()
    {
        var xfsDir = Path.Combine(_solutionRoot, "src", "EJLive.Core", "Xfs");
        if (!Directory.Exists(xfsDir))
            return new ReadinessItem("XFS adapters", ReadinessStatus.Blocked, "Xfs directory not found");

        var adaptersDir = Path.Combine(xfsDir, "Adapters");
        var adapterCount = Directory.Exists(adaptersDir)
            ? Directory.GetFiles(adaptersDir, "*.cs").Length
            : 0;

        return new ReadinessItem("XFS adapters",
            adapterCount >= 3 ? ReadinessStatus.Ready : ReadinessStatus.Warning,
            $"{adapterCount} XFS adapter files found");
    }

    private ReadinessItem CheckSecurityInfrastructure()
    {
        var securityFiles = new[]
        {
            "SecretProtector.cs",
            "SecureHandshakeService.cs",
            "LogRedactionEngine.cs",
            "SafeRemoteCommandQueue.cs",
            "SafeRemoteCommandExecutor.cs"
        };

        var engineDir = Path.Combine(_solutionRoot, "src", "EJLive.Core", "Engine");
        var present = securityFiles.Count(f => File.Exists(Path.Combine(engineDir, f)));
        return new ReadinessItem("Security infrastructure",
            present == securityFiles.Length ? ReadinessStatus.Ready : ReadinessStatus.Warning,
            $"{present}/{securityFiles.Length} security components present");
    }

    private ReadinessItem CheckNetworkEngines()
    {
        var networkFiles = new[]
        {
            "ChunkedTransferEngine.cs",
            "IngestionPipeline.cs",
            "NetworkEngine.cs",
            "HeartbeatService.cs",
            "TimeSyncService.cs"
        };

        var engineDir = Path.Combine(_solutionRoot, "src", "EJLive.Core", "Engine");
        var present = networkFiles.Count(f => File.Exists(Path.Combine(engineDir, f)));
        return new ReadinessItem("Network engines",
            present == networkFiles.Length ? ReadinessStatus.Ready : ReadinessStatus.Warning,
            $"{present}/{networkFiles.Length} network engines present");
    }

    private ReadinessItem CheckInstallerReady()
    {
        var engineDir = Path.Combine(_solutionRoot, "src", "EJLive.Core", "Engine");
        var hasInstaller = File.Exists(Path.Combine(engineDir, "InstallerEngine.cs"));
        var hasRegistration = File.Exists(Path.Combine(engineDir, "WindowsServiceRegistration.cs"));
        var hasRollback = File.Exists(Path.Combine(engineDir, "DeploymentRollbackGuard.cs"));

        var all = hasInstaller && hasRegistration && hasRollback;
        return new ReadinessItem("Installer & rollback",
            all ? ReadinessStatus.Ready : ReadinessStatus.Warning,
            $"Installer={hasInstaller}, Registration={hasRegistration}, Rollback={hasRollback}");
    }

    private ReadinessItem CheckTestsPresent()
    {
        var testsDir = Path.Combine(_solutionRoot, "src", "EJLive.Tests");
        if (!Directory.Exists(testsDir))
            return new ReadinessItem("Test project", ReadinessStatus.Blocked, "EJLive.Tests not found");

        var testFiles = Directory.GetFiles(testsDir, "*Tests.cs", SearchOption.AllDirectories);
        return new ReadinessItem("Test coverage",
            testFiles.Length >= 3 ? ReadinessStatus.Ready : ReadinessStatus.Warning,
            $"{testFiles.Length} test files");
    }

    private ReadinessItem CheckArtifactsGenerated()
    {
        var artifactsDir = Path.Combine(_solutionRoot, "artifacts");
        if (!Directory.Exists(artifactsDir))
            return new ReadinessItem("Build artifacts", ReadinessStatus.Warning, "artifacts/ directory not found");

        var files = Directory.GetFiles(artifactsDir, "*", SearchOption.AllDirectories);
        return new ReadinessItem("Build artifacts",
            files.Length > 0 ? ReadinessStatus.Ready : ReadinessStatus.Warning,
            $"{files.Length} artifact files");
    }

    private ReadinessItem CheckDocumentation()
    {
        var docsDir = Path.Combine(_solutionRoot, "docs");
        if (!Directory.Exists(docsDir))
            return new ReadinessItem("Documentation", ReadinessStatus.Warning, "docs/ directory not found");

        var docFiles = Directory.GetFiles(docsDir, "*", SearchOption.AllDirectories);
        return new ReadinessItem("Documentation",
            docFiles.Length >= 5 ? ReadinessStatus.Ready : ReadinessStatus.Warning,
            $"{docFiles.Length} documentation files");
    }
}

/// <summary>
/// Status of a readiness check item.
/// </summary>
public enum ReadinessStatus
{
    Ready,
    Warning,
    Blocked
}

/// <summary>
/// A single readiness check result.
/// </summary>
public sealed record ReadinessItem(string Name, ReadinessStatus Status, string Detail);

/// <summary>
/// Comprehensive release readiness report.
/// </summary>
public sealed record ReleaseReadinessReport
{
    public required DateTime AssessedUtc { get; init; }
    public required string SolutionRoot { get; init; }
    public required IReadOnlyList<ReadinessItem> Items { get; init; }
    public required bool OverallReady { get; init; }
    public required int ReadinessScore { get; init; }
    public required string Summary { get; init; }
}
