using System.Reflection;

namespace EJLive.Core.Engine;

/// <summary>
/// Track 044 – Regression Verification Gate.
/// Performs automated pre-release verification checks across the solution.
/// Validates assembly contracts, dependency graphs, file integrity, and naming conventions.
/// </summary>
public sealed class RegressionVerificationGate
{
    private readonly string _solutionRoot;
    private readonly List<GateCheckResult> _results = new();

    public RegressionVerificationGate(string solutionRoot)
    {
        _solutionRoot = solutionRoot;
    }

    /// <summary>
    /// Runs all regression gate checks and returns a comprehensive report.
    /// </summary>
    public GateReport RunAllChecks()
    {
        _results.Clear();

        CheckActiveCompileMapExists();
        CheckServiceActivationStatus();
        CheckProjectDependencyGraph();
        CheckNoOrphanedModelFiles();
        CheckNamespaceConsistency();
        CheckCriticalEngineFilesPresent();
        CheckNoHardcodedSecrets();
        CheckTestCoverage();

        var passed = _results.All(r => r.Passed);

        return new GateReport
        {
            RunTimestampUtc = DateTime.UtcNow,
            SolutionRoot = _solutionRoot,
            Checks = _results.AsReadOnly(),
            AllPassed = passed,
            PassCount = _results.Count(r => r.Passed),
            FailCount = _results.Count(r => !r.Passed)
        };
    }

    private void CheckActiveCompileMapExists()
    {
        var path = Path.Combine(_solutionRoot, "artifacts", "ActiveCompileMap.csv");
        AddResult("ActiveCompileMap.csv exists", File.Exists(path),
            File.Exists(path) ? "Found at expected location" : "Missing artifacts/ActiveCompileMap.csv");
    }

    private void CheckServiceActivationStatus()
    {
        var path = Path.Combine(_solutionRoot, "docs", "12-service-activation-status.csv");
        if (!File.Exists(path))
        {
            AddResult("Service activation status", false, "Missing docs/12-service-activation-status.csv");
            return;
        }

        var lines = File.ReadAllLines(path);
        var hasSufficientEntries = lines.Length > 50;
        AddResult("Service activation status coverage", hasSufficientEntries,
            $"Found {lines.Length - 1} entries (min required: 50)");
    }

    private void CheckProjectDependencyGraph()
    {
        var path = Path.Combine(_solutionRoot, "artifacts", "ProjectDependencyGraph.md");
        AddResult("ProjectDependencyGraph.md exists", File.Exists(path),
            File.Exists(path) ? "Found at expected location" : "Missing artifacts/ProjectDependencyGraph.md");
    }

    private void CheckNoOrphanedModelFiles()
    {
        var modelsDir = Path.Combine(_solutionRoot, "src", "EJLive.Core", "Models");
        if (!Directory.Exists(modelsDir))
        {
            AddResult("Model files directory", false, "Models directory not found");
            return;
        }

        var csprojPath = Path.Combine(_solutionRoot, "src", "EJLive.Core", "EJLive.Core.csproj");
        if (!File.Exists(csprojPath))
        {
            AddResult("Model files tracked", false, "csproj not found");
            return;
        }

        var csprojContent = File.ReadAllText(csprojPath);
        var modelFiles = Directory.GetFiles(modelsDir, "*.cs");
        var orphaned = modelFiles
            .Select(Path.GetFileName)
            .Where(f => !csprojContent.Contains(f!, StringComparison.OrdinalIgnoreCase))
            .ToList();

        AddResult("No orphaned model files", orphaned.Count == 0,
            orphaned.Count == 0
                ? "All model files are tracked in csproj"
                : $"Orphaned: {string.Join(", ", orphaned)}");
    }

    private void CheckNamespaceConsistency()
    {
        var engineDir = Path.Combine(_solutionRoot, "src", "EJLive.Core", "Engine");
        if (!Directory.Exists(engineDir))
        {
            AddResult("Engine namespace consistency", false, "Engine directory not found");
            return;
        }

        var wrongNamespace = new List<string>();
        foreach (var file in Directory.GetFiles(engineDir, "*.cs"))
        {
            var content = File.ReadAllText(file);
            if (!content.Contains("namespace EJLive.Core.Engine", StringComparison.Ordinal)
                && !content.Contains("namespace EJLive.Core.Engine;", StringComparison.Ordinal))
            {
                wrongNamespace.Add(Path.GetFileName(file));
            }
        }

        AddResult("Engine namespace consistency", wrongNamespace.Count == 0,
            wrongNamespace.Count == 0
                ? "All engine files use EJLive.Core.Engine namespace"
                : $"Wrong namespace: {string.Join(", ", wrongNamespace)}");
    }

    private void CheckCriticalEngineFilesPresent()
    {
        var criticalFiles = new[]
        {
            "SecureHandshakeService.cs",
            "FileWatcherEngine.cs",
            "ChunkedTransferEngine.cs",
            "IngestionPipeline.cs",
            "JournalOutbox.cs",
            "EjParserRegistry.cs",
            "NcrEjTransactionParser.cs",
            "HeartbeatService.cs",
            "SafeRemoteCommandQueue.cs",
            "InstallerEngine.cs",
            "SecretProtector.cs",
            "CorrelationEngine.cs"
        };

        var engineDir = Path.Combine(_solutionRoot, "src", "EJLive.Core", "Engine");
        var missing = criticalFiles
            .Where(f => !File.Exists(Path.Combine(engineDir, f)))
            .ToList();

        AddResult("Critical engine files present", missing.Count == 0,
            missing.Count == 0
                ? $"All {criticalFiles.Length} critical engine files present"
                : $"Missing: {string.Join(", ", missing)}");
    }

    private void CheckNoHardcodedSecrets()
    {
        var suspiciousPatterns = new[] { "password=", "apikey=", "secret=", "connectionstring=" };
        var engineDir = Path.Combine(_solutionRoot, "src", "EJLive.Core", "Engine");
        if (!Directory.Exists(engineDir))
        {
            AddResult("No hardcoded secrets", true, "Engine directory not present (skipped)");
            return;
        }

        var violations = new List<string>();
        foreach (var file in Directory.GetFiles(engineDir, "*.cs"))
        {
            var content = File.ReadAllText(file).ToLowerInvariant();
            foreach (var pattern in suspiciousPatterns)
            {
                if (content.Contains(pattern) && !content.Contains("// safe:"))
                {
                    violations.Add($"{Path.GetFileName(file)} contains '{pattern}'");
                }
            }
        }

        AddResult("No hardcoded secrets", violations.Count == 0,
            violations.Count == 0
                ? "No suspicious secret patterns found"
                : string.Join("; ", violations.Take(5)));
    }

    private void CheckTestCoverage()
    {
        var testsDir = Path.Combine(_solutionRoot, "src", "EJLive.Tests");
        if (!Directory.Exists(testsDir))
        {
            AddResult("Test project exists", false, "EJLive.Tests directory not found");
            return;
        }

        var testFiles = Directory.GetFiles(testsDir, "*Tests.cs", SearchOption.AllDirectories);
        AddResult("Test coverage", testFiles.Length >= 3,
            $"Found {testFiles.Length} test files (minimum 3 required)");
    }

    private void AddResult(string name, bool passed, string detail)
    {
        _results.Add(new GateCheckResult(name, passed, detail));
    }
}

/// <summary>
/// Result of a single regression gate check.
/// </summary>
public sealed record GateCheckResult(string Name, bool Passed, string Detail);

/// <summary>
/// Comprehensive report from all regression gate checks.
/// </summary>
public sealed record GateReport
{
    public required DateTime RunTimestampUtc { get; init; }
    public required string SolutionRoot { get; init; }
    public required IReadOnlyList<GateCheckResult> Checks { get; init; }
    public required bool AllPassed { get; init; }
    public required int PassCount { get; init; }
    public required int FailCount { get; init; }
}
