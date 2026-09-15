namespace EJLive.Core.Services;

/// <summary>Activation state of one service file in the SS-15 sense.</summary>
public enum ServiceActivationStatusKind
{
    /// <summary>Listed in its project's compile map — the class is a live part of the assembly.</summary>
    ActiveCompiled,

    /// <summary>Archived under <c>src/_reference</c> but its nominal path routes to an active service.</summary>
    CoveredByBridge,

    /// <summary>Sits in a bridge-routed project directory yet is outside the compile map — must be promoted or retired.</summary>
    NeedsActivation
}

/// <summary>One candidate file with its classification and the active service that carries its behavior.</summary>
public sealed record ServiceActivationCandidate(
    string Path,
    ServiceActivationStatusKind Status,
    string ActiveService);

/// <summary>Aggregated activation answer for one repository root.</summary>
public sealed class ServiceActivationAuditReport
{
    public string RootPath { get; set; } = string.Empty;
    public List<ServiceActivationCandidate> Candidates { get; } = new();
    public int TotalCandidates => Candidates.Count;
    public int ActiveCompiledCandidates => Candidates.Count(c => c.Status == ServiceActivationStatusKind.ActiveCompiled);
    public int CoveredCandidates => Candidates.Count(c => c.Status == ServiceActivationStatusKind.CoveredByBridge);
    public int NeedsActivationCandidates => Candidates.Count(c => c.Status == ServiceActivationStatusKind.NeedsActivation);
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
}

/// <summary>
/// Service-activation auditor (C-27): the C# counterpart of <c>tools/inventory/service_activation.py</c>,
/// scoped to the <see cref="ServiceBridgeRoutes"/> directories. Every C# file under a bridge-routed
/// project directory must be either compiled (in the project's csproj map) or demoted into the
/// <c>src/_reference</c> tree behind the same nominal path; anything else is <see cref="ServiceActivationStatusKind.NeedsActivation"/>
/// and fails the platform rule "no source inside a project may sit outside its compile map"
/// (gate TYPE-4). The activation ledger in <c>docs/12-service-activation-status.csv</c> is the
/// audited snapshot of exactly this classification, regenerated on every push.
/// </summary>
public sealed class UnifiedServiceActivationAuditService
{
    public ServiceActivationAuditReport Analyze(string rootPath)
    {
        var report = new ServiceActivationAuditReport
        {
            RootPath = rootPath,
            GeneratedAtUtc = DateTime.UtcNow
        };
        if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
        {
            report.Summary = $"Invalid root path: {rootPath}";
            return report;
        }

        var srcRoot = Path.Combine(rootPath, "src");
        if (!Directory.Exists(srcRoot))
        {
            report.Summary = $"No src/ tree under {rootPath}";
            return report;
        }

        var compileMaps = LoadCompileMaps(srcRoot);

        foreach (var route in ServiceBridgeRoutes.Map)
        {
            // 1) live project tree under the routed prefix
            var routedDir = Path.Combine(rootPath, route.PathPrefix.TrimEnd('/').Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(routedDir))
            {
                var projectMap = MapForProject(compileMaps, route.PathPrefix);
                var projectDir = Path.Combine(srcRoot, route.PathPrefix["src/".Length..].Split('/')[0]);
                foreach (var file in Directory.EnumerateFiles(routedDir, "*.cs", SearchOption.AllDirectories))
                {
                    if (IsGenerated(file))
                        continue;
                    var relative = ServiceBridgeRoutes.NormalizePath(Path.GetRelativePath(rootPath, file));
                    var withinProject = ServiceBridgeRoutes.NormalizePath(Path.GetRelativePath(projectDir, file));
                    var status = projectMap.Contains(Key(withinProject))
                        ? ServiceActivationStatusKind.ActiveCompiled
                        : ServiceActivationStatusKind.NeedsActivation;
                    report.Candidates.Add(new ServiceActivationCandidate(relative, status, route.ActiveService));
                }
            }

            // 2) archived mirrors of the same prefix (src/_reference/<bucket>/<same tail>)
            foreach (var bucket in new[] { "uncompiled", "exact-duplicates", "corrupted" })
            {
                var tail = route.PathPrefix
                    .Replace("src/", string.Empty, StringComparison.Ordinal)
                    .TrimEnd('/');
                var archivedDir = Path.Combine(srcRoot, "_reference", bucket,
                    tail.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(archivedDir))
                    continue;

                foreach (var file in Directory.EnumerateFiles(archivedDir, "*.cs", SearchOption.AllDirectories))
                {
                    if (IsGenerated(file))
                        continue;
                    var nominal = route.PathPrefix + Path.GetRelativePath(archivedDir, file)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    report.Candidates.Add(new ServiceActivationCandidate(
                        ServiceBridgeRoutes.NormalizePath(nominal),
                        ServiceActivationStatusKind.CoveredByBridge,
                        route.ActiveService));
                }
            }
        }

        report.Summary =
            $"Activation audit: {report.TotalCandidates} candidates, {report.ActiveCompiledCandidates} active-compiled, " +
            $"{report.CoveredCandidates} covered-by-bridge, {report.NeedsActivationCandidates} needing activation.";
        return report;
    }

    private static Dictionary<string, HashSet<string>> LoadCompileMaps(string srcRoot)
    {
        var maps = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var projectDir in Directory.GetDirectories(srcRoot))
        {
            var projectName = Path.GetFileName(projectDir);
            if (projectName.Equals("_reference", StringComparison.OrdinalIgnoreCase))
                continue;
            var projectFile = Path.Combine(projectDir, projectName + ".csproj");
            if (File.Exists(projectFile))
                maps[projectName] = UnifiedProjectIntegrationAuditService.LoadCompileMap(projectFile);
        }
        return maps;
    }

    private static HashSet<string> MapForProject(Dictionary<string, HashSet<string>> maps, string prefix)
    {
        // "src/EJLive.Core/Services/" -> "EJLive.Core"
        var rest = prefix["src/".Length..];
        var slash = rest.IndexOf('/');
        var projectName = slash < 0 ? rest : rest[..slash];
        return maps.TryGetValue(projectName, out var map)
            ? map
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private static string Key(string relativePath) => relativePath.TrimStart('.').Trim('/');

    private static bool IsGenerated(string file) =>
        file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
        file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
