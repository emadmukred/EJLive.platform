using System.Text.RegularExpressions;

namespace EJLive.Core.Services;

/// <summary>
/// One reference-path route: which archived directory prefix an evidence file sits under,
/// which active unified service now owns its behavior, and why the bridge is sufficient.
/// Named for the corpus vocabulary ("active service replacement") so the integration audit
/// and the service gateway consume the same record.
/// </summary>
public sealed record ActiveServiceReplacement(string PathPrefix, string ActiveService, string Coverage);

/// <summary>
/// A C# file that sits inside a project tree but is deliberately outside that project's
/// compile map — retained as source evidence rather than compiled. <see cref="ActiveReplacement"/>
/// names the active service bridging its behavior (empty when nothing replaces it, which the
/// gate forbids for project-tree files and the audit reports as uncovered).
/// </summary>
public sealed record ReferenceOnlyServiceFile(string Path, string ActiveReplacement = "");

/// <summary>A duplicate non-partial type name declared by several files inside one project.</summary>
public sealed record DuplicateTypeFinding(string TypeName, IReadOnlyList<string> Files);

/// <summary>
/// The single bridge-route table (SS-15). Both <see cref="UnifiedServiceGateway"/>'s activation
/// routing and <see cref="UnifiedProjectIntegrationAuditService"/>'s replacement mapping read this
/// table, so a route can never exist in one view and not the other (Wave-4 fix-up, C-27: the
/// gateway previously carried the map inline and the audit carried none).
/// </summary>
public static class ServiceBridgeRoutes
{
    public static readonly ActiveServiceReplacement[] Map =
    [
        new("src/EJLive.Client.WinForms/Agent/", "UnifiedClientServiceSupervisor", "Agent lifecycle and scheduler behavior is supervised by active client service operations."),
        new("src/EJLive.Client.WinForms/Services/", "UnifiedClientServiceSupervisor + UnifiedRemoteCommandOrchestrator + UnifiedJournalStorageService", "Client service variants are bridged through active command, journal, and supervision services."),
        new("src/EJLive.Server/Services/", "UnifiedJournalStorageService + UnifiedRemoteCommandOrchestrator", "Legacy server services are bridged through active storage/report and command orchestration."),
        new("src/EJLive.Server.WinForms/Services/", "UnifiedJournalStorageService + UnifiedClientServiceSupervisor", "Server WinForms service variants are bridged through active storage/supervision services."),
        new("src/EJLive.Core/Services/", "CoreServices + UnifiedOperationalFusion + UnifiedServiceOperations + UnifiedServiceGateway", "Core service variants are consolidated into compiled service modules with the unified gateway bridge."),
        new("src/EJLive.Core/Engine/", "OperationalEngines + NetworkEngine + CommunicationProtocol + JournalOutbox", "Legacy engine variants are consolidated into compiled operational engine services."),
        new("src/EJLive.Core/Xfs/", "XfsModels + UnifiedJournalEvidenceAnalyzer", "XFS variants are represented by compiled normalized models and analyzer-based evidence."),
        new("src/EJLive.Core/Models/", "UnifiedModels", "Model variants are consolidated into compiled unified models."),
        new("src/EJLive.Shared/", "AppLogger + SecurityHelper + DateTimeHelper + RetryPolicy", "Shared helper variants are represented by compiled shared utility services."),
        new("legacy/original/", "UnifiedServiceGateway", "Legacy archive roots are retained as source evidence and bridged through unified runtime services.")
    ];

    /// <summary>Longest-matching route for a normalized forward-slash repo-relative path.</summary>
    public static ActiveServiceReplacement? Resolve(string normalizedPath)
    {
        ActiveServiceReplacement? best = null;
        foreach (var route in Map)
        {
            if (normalizedPath.StartsWith(route.PathPrefix, StringComparison.OrdinalIgnoreCase)
                && (best is null || route.PathPrefix.Length > best.PathPrefix.Length))
            {
                best = route;
            }
        }
        return best;
    }

    /// <summary>Backslashes collapsed to '/', trimmed of leading './' and whitespace.</summary>
    public static string NormalizePath(string path) =>
        path.Trim().Replace('\\', '/').TrimStart('.');
}

/// <summary>
/// Result of the repository-wide integration audit (SS-15/SS-17): how much source evidence the
/// tree carries, which service files are reference-only, whether every one of them is covered
/// by an active replacement, and whether the compiled set still hides duplicate types.
/// </summary>
public sealed class ProjectIntegrationAuditReport
{
    public string RootPath { get; set; } = string.Empty;
    public int TotalProjects { get; set; }
    public int TotalCSFiles { get; set; }
    public int TotalSolutions { get; set; }
    public bool IsIntegrated { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> AnalyzedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime GeneratedAtUtc { get; set; }

    /// <summary>All C# files seen under <c>src/</c>, compiled set plus reference archive.</summary>
    public int SourceFileCount { get; set; }

    /// <summary>Project-tree C# files living outside their project's compile map.</summary>
    public List<ReferenceOnlyServiceFile> ReferenceOnlyFiles { get; set; } = new();

    public int ReferenceOnlyServiceFileCount => ReferenceOnlyFiles.Count;

    /// <summary>The bridge-route table in force (the replacement map the gateway shares).</summary>
    public IReadOnlyList<ActiveServiceReplacement> ActiveReplacements { get; set; } = ServiceBridgeRoutes.Map;

    /// <summary>Reference-only files whose route lookup failed to name an active replacement.</summary>
    public List<ReferenceOnlyServiceFile> UncoveredReferenceOnlyFiles { get; set; } = new();

    public bool AllReferenceOnlyServicesCovered => UncoveredReferenceOnlyFiles.Count == 0;

    /// <summary>Non-partial type names declared by more than one file inside the same project.</summary>
    public List<DuplicateTypeFinding> DuplicateTypeFindings { get; set; } = new();
}

/// <summary>
/// Repository integration auditor. The gateway exposes it as the single composition-level
/// "what is compiled, what is reference-only, what replaces what" answer. Implementation is
/// deliberately dependency-free (Directory enumeration + csproj text scan) so it runs inside any
/// host, the verification suite, and tests without touching a database or a vendor path.
/// </summary>
public sealed class UnifiedProjectIntegrationAuditService
{
    private static readonly Regex TypeDeclaration = new(
        @"^\s*(?:\[[^\]]*\]\s*)*(?:public|internal)\s+(?:static\s+|sealed\s+|abstract\s+|readonly\s+)*(?<kind>class|record|struct|interface|enum)\s+(?<name>[A-Za-z_]\w*)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex CompileItem = new(
        @"<Compile\s+Include=""([^""]+)""",
        RegexOptions.Compiled);

    public ProjectIntegrationAuditReport Analyze(string rootPath)
    {
        var report = new ProjectIntegrationAuditReport
        {
            RootPath = rootPath,
            GeneratedAtUtc = DateTime.UtcNow
        };
        if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
        {
            report.Errors.Add($"Invalid root path: {rootPath}");
            return report;
        }

        var srcRoot = Path.Combine(rootPath, "src");
        if (!Directory.Exists(srcRoot))
        {
            report.Errors.Add($"No src/ tree under root: {rootPath}");
            return report;
        }

        report.TotalSolutions =
            Directory.GetFiles(rootPath, "*.sln", SearchOption.TopDirectoryOnly).Length +
            Directory.GetFiles(rootPath, "*.slnx", SearchOption.TopDirectoryOnly).Length;

        foreach (var projectDir in Directory.GetDirectories(srcRoot))
        {
            var projectName = Path.GetFileName(projectDir);
            if (projectName.Equals("_reference", StringComparison.OrdinalIgnoreCase))
                continue;

            var projectFile = Path.Combine(projectDir, projectName + ".csproj");
            if (!File.Exists(projectFile))
                continue;

            report.TotalProjects++;
            var compileMap = LoadCompileMap(projectFile);
            var declarations = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories))
            {
                if (IsGenerated(file))
                    continue;

                report.TotalCSFiles++;
                report.SourceFileCount++;
                var relative = ServiceBridgeRoutes.NormalizePath(
                    Path.GetRelativePath(rootPath, file));
                report.AnalyzedFiles.Add(relative);

                var projectRelative = ServiceBridgeRoutes.NormalizePath(
                    Path.GetRelativePath(projectDir, file));
                if (!compileMap.Contains(NormalizeKey(projectRelative)))
                {
                    var replacement = ServiceBridgeRoutes.Resolve(relative)?.ActiveService ?? string.Empty;
                    var referenceOnly = new ReferenceOnlyServiceFile(relative, replacement);
                    report.ReferenceOnlyFiles.Add(referenceOnly);
                    if (string.IsNullOrWhiteSpace(replacement))
                        report.UncoveredReferenceOnlyFiles.Add(referenceOnly);
                    continue;
                }

                CollectDuplicateCandidates(file, relative, declarations);
            }

            // A non-partial type name declared by two files in one project is a CS0101 the
            // gate has not been allowed to accumulate: surface it as a finding instead.
            foreach (var (name, files) in declarations)
            {
                if (files.Count > 1)
                    report.DuplicateTypeFindings.Add(new DuplicateTypeFinding(name, files));
            }
        }

        // The archived tree counts toward the evidence volume only.
        foreach (var archived in EnumerateArchivedSources(srcRoot))
            report.SourceFileCount++;

        report.IsIntegrated = report.TotalProjects > 0 && report.Errors.Count == 0 && report.AllReferenceOnlyServicesCovered;
        report.Summary =
            $"Integration audit: {report.TotalProjects} projects, {report.TotalCSFiles} compiled-map-area C# files " +
            $"({report.SourceFileCount} sources incl. archive), {report.ReferenceOnlyServiceFileCount} reference-only, " +
            $"{report.DuplicateTypeFindings.Count} duplicate-type findings. Integrated: {report.IsIntegrated}";
        return report;
    }

    /// <summary>
    /// A project's compile map: every <c>Compile Include</c> in its csproj, normalized to a
    /// project-relative forward-slash key. Shared with the service-activation auditor.
    /// </summary>
    internal static HashSet<string> LoadCompileMap(string projectFile)
    {
        var map = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in CompileItem.Matches(File.ReadAllText(projectFile)))
            map.Add(NormalizeKey(match.Groups[1].Value.Replace('\\', '/')));
        return map;
    }

    private static string NormalizeKey(string path) => path.TrimStart('.').Trim('/');

    private static bool IsGenerated(string file) =>
        file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
        file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<string> EnumerateArchivedSources(string srcRoot)
    {
        var archive = Path.Combine(srcRoot, "_reference");
        if (!Directory.Exists(archive))
            yield break;
        foreach (var file in Directory.EnumerateFiles(archive, "*.cs", SearchOption.AllDirectories))
        {
            if (!IsGenerated(file))
                yield return file;
        }
    }

    private static void CollectDuplicateCandidates(
        string file, string relative, Dictionary<string, List<string>> declarations)
    {
        string text;
        try { text = File.ReadAllText(file); }
        catch { return; }

        foreach (Match match in TypeDeclaration.Matches(text))
        {
            var declarationStart = text.LastIndexOf('\n', match.Index) + 1;
            var line = text[declarationStart..(match.Index + match.Length)];
            if (line.Contains("partial", StringComparison.Ordinal))
                continue; // legal same-assembly partial split (Designer pattern) — never a finding
            if (!declarations.TryGetValue(match.Groups["name"].Value, out var seen))
                declarations[match.Groups["name"].Value] = seen = new List<string>();
            if (!seen.Contains(relative, StringComparer.OrdinalIgnoreCase))
                seen.Add(relative);
        }
    }
}
