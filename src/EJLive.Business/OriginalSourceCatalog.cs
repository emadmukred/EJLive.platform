namespace EJLive.Business;

public static class OriginalSourceCatalog
{
    private static readonly OriginalSourceProject[] SourceProjects =
    [
        ProjectRow("_coder01_source_study", SourceMergeRole.StudyArchive, 136, 71, 7, 1, 14, 28, 57, 72, 18, 53, "Coder01 study extraction with build instructions, architecture notes, and source evidence."),
        ProjectRow("_codex_zip_study_20260515_053548", SourceMergeRole.StudyArchive, 874, 591, 35, 5, 65, 318, 243, 189, 375, 216, "Large comparative ZIP study containing ActiveTestPackage, FixedGpt, LatestWorkspace, Replit, and final snapshots."),
        ProjectRow("ChatGPT_ActiveTestPackage", SourceMergeRole.TestEvidence, 166, 95, 7, 1, 7, 54, 63, 18, 54, 41, "Active test package with source, sample ATM logs, Excel requirements, vendor research notes, and UI evidence."),
        ProjectRow("ChatGPT_LatestWorkspace", SourceMergeRole.ReferenceWorkspace, 152, 94, 7, 1, 7, 54, 50, 16, 55, 39, "Clean workspace containing JournalSync services, XFS adapters, vendor parsers, and active WinForms surfaces."),
        ProjectRow("Coder01", SourceMergeRole.ActiveBaseline, 1398, 148, 14, 2, 29, 60, 1234, 154, 43, 105, "Primary enterprise baseline with client, server, monitor, setup, journal, archive, network, and remote-control code."),
        ProjectRow("Coder01 (2)", SourceMergeRole.DistArchive, 702, 77, 7, 1, 15, 32, 617, 78, 25, 52, "Second Coder01 distribution archive retained for duplicate and backup comparison."),
        ProjectRow("Coder01-orginal", SourceMergeRole.DistArchive, 746, 77, 7, 1, 15, 32, 661, 79, 25, 52, "Original Coder01 distribution archive with binaries, backup files, design notes, and service variants."),
        ProjectRow("CodexMarege", SourceMergeRole.ReferenceWorkspace, 672, 134, 7, 1, 17, 70, 530, 56, 84, 50, "Working merge snapshot close to FixedGpt with diagnostic manifests and refactor status reports."),
        ProjectRow("CodexMarege_restructured_Replit", SourceMergeRole.ReferenceWorkspace, 176, 134, 7, 1, 17, 70, 34, 59, 95, 39, "Restructured Replit snapshot used for layout and staged refactor comparison."),
        ProjectRow("CodexMarege-refactor_Gethub", SourceMergeRole.ReferenceWorkspace, 106, 79, 4, 1, 13, 34, 22, 33, 53, 26, "Compact refactor branch used to compare simplified project structure and source organization."),
        ProjectRow("EJLive_APPs", SourceMergeRole.DistArchive, 9, 0, 0, 0, 0, 0, 9, 0, 0, 0, "Application package artifacts preserved as non-source reference material."),
        ProjectRow("EJLive_Client_v5_Enhanced", SourceMergeRole.ReferenceWorkspace, 186, 149, 9, 1, 17, 72, 27, 34, 141, 8, "Enhanced v5 client package; near-active source with eight compiled C# differences staged for focused promotion."),
        ProjectRow("EJLive_Enterprise_20260510_build_candidate", SourceMergeRole.ReferenceWorkspace, 21, 18, 2, 1, 0, 6, 0, 5, 7, 11, "Build-candidate subset used for targeted service and project comparison."),
        ProjectRow("EJLive_Enterprise_20260510_latest_workspace", SourceMergeRole.ReferenceWorkspace, 152, 94, 7, 1, 7, 54, 50, 16, 55, 39, "Enterprise latest-workspace mirror for cross-checking ChatGPT_LatestWorkspace."),
        ProjectRow("EJLive_Enterprise_active_test_package_2026-05-10", SourceMergeRole.TestEvidence, 166, 95, 7, 1, 7, 54, 63, 18, 54, 41, "Enterprise active test package mirror with log and requirement evidence."),
        ProjectRow("EJLive_Enterprise_v3.2.1_Enhanced", SourceMergeRole.DistArchive, 48, 36, 6, 1, 7, 10, 5, 18, 0, 36, "Enhanced v3.2.1 branch retained for historical UI and service behavior comparison."),
        ProjectRow("EJLive_Enterprise_v3.4.0", SourceMergeRole.DistArchive, 426, 71, 7, 1, 14, 28, 347, 66, 16, 55, "Older enterprise v3.4.0 release with backup files, build notes, and historical journal/server behavior."),
        ProjectRow("EJLive_Enterprise_v3.4.0_Enhanced", SourceMergeRole.DistArchive, 56, 42, 6, 1, 7, 18, 7, 24, 0, 42, "Enhanced v3.4.0 branch used for additional UI and operational-service comparison."),
        ProjectRow("EJLive_Enterprise_v3.4.0_latest", SourceMergeRole.ReferenceWorkspace, 146, 96, 6, 1, 7, 55, 43, 17, 41, 55, "Latest v3.4.0 branch with newer service and XFS/vendor behavior candidates."),
        ProjectRow("EJLive_Menus_ai", SourceMergeRole.ExternalDesign, 60, 45, 6, 1, 7, 20, 8, 25, 0, 45, "Menu-oriented AI branch retained for UI/menu consolidation ideas."),
        ProjectRow("EJLive_replik", SourceMergeRole.ReferenceWorkspace, 56, 42, 6, 1, 7, 18, 7, 24, 0, 42, "Replit branch retained for source layout and service comparison."),
        ProjectRow("EJLive-CodexMarege-fixedGpt", SourceMergeRole.ReferenceWorkspace, 203, 134, 7, 1, 17, 70, 61, 38, 83, 51, "FixedGpt archive mirror with FleetPrediction, Windows startup, remote access, and remote-control references."),
        ProjectRow("EJLive.Unified", SourceMergeRole.ReferenceWorkspace, 180, 141, 9, 1, 17, 72, 29, 34, 134, 7, "Prior unified source snapshot used to compare the current active project with near-identical source."),
        ProjectRow("EJLiveWorkCoder", SourceMergeRole.ReferenceWorkspace, 60, 45, 6, 1, 7, 20, 8, 25, 0, 45, "WorkCoder branch retained for enhanced UI/service comparison."),
        ProjectRow("FixedGpt", SourceMergeRole.ReferenceWorkspace, 203, 134, 7, 1, 17, 70, 61, 38, 83, 51, "Expanded fixed workspace with FleetPrediction, Windows startup, remote access, remote control, and build helpers."),
        ProjectRow("Kimi_Agent", SourceMergeRole.ExternalDesign, 44, 36, 4, 1, 7, 11, 3, 0, 0, 36, "Independent English implementation with Settings, LogViewer, Dashboard, alert, file transfer, and service-controller concepts."),
        ProjectRow("Kimi_Agent_تحليل نظام EJLive", SourceMergeRole.ExternalDesign, 45, 36, 4, 1, 7, 11, 4, 0, 0, 36, "Kimi Agent analysis package; source content is English and preserved for design comparison."),
        ProjectRow("src", SourceMergeRole.ReferenceWorkspace, 55, 36, 8, 1, 7, 8, 10, 0, 32, 4, "Original src bundle with near-active English source and a small set of compiled C# differences."),
        ProjectRow("System-Analyzer-Unified_Replit", SourceMergeRole.StudyArchive, 1549, 63, 7, 2, 11, 24, 1477, 67, 10, 53, "System Analyzer/Replit archive with analyzer state, frontend artifacts, and the Ultimate enterprise snapshot."),
        ProjectRow("Testing_UI", SourceMergeRole.TestEvidence, 1137, 362, 27, 4, 35, 194, 744, 127, 169, 193, "UI testing bundle containing Coder01, build-candidate, latest-workspace, and active-test package copies.")
    ];

    private static readonly OriginalSourceFeature[] SourceFeatures =
    [
        Feature("_coder01_source_study", "Baseline study", "Filtered Coder01 source, architecture notes, and source evidence.", FeatureMergeState.ActiveWithReference, "Used to rebuild the canonical UI/runtime map while preserving the source-study files."),
        Feature("_codex_zip_study_20260515_053548", "Comparative study", "ActiveTestPackage, FixedGpt, LatestWorkspace, Replit, and final snapshots.", FeatureMergeState.ActiveWithReference, "Feeds the source-inventory evidence and the staged promotion backlog."),
        Feature("ChatGPT_ActiveTestPackage", "Test evidence", "ATM logs, Excel requirements, parser research, and active UI evidence.", FeatureMergeState.TestEvidence, "Kept as parser and verification evidence for phase-two tests."),
        Feature("ChatGPT_LatestWorkspace", "Sync and vendor services", "JournalSync services, XFS adapters, vendor parsers, and WinForms shells.", FeatureMergeState.ActiveWithReference, "Mapped into Core services, XFS boundaries, and runtime capabilities."),
        Feature("Coder01", "Primary enterprise baseline", "Client, server, monitor, setup, journal, archive, network, and remote-control code.", FeatureMergeState.ActiveWithReference, "Baseline UI tabs and command/file-watcher behavior are active; raw variants remain linked as reference."),
        Feature("Coder01 (2)", "Distribution comparison", "Duplicate distribution archive for backup and drift comparison.", FeatureMergeState.StagedReference, "Preserved to prove duplicate coverage without compiling conflicting copies."),
        Feature("Coder01-orginal", "Original distribution", "Binaries, backup files, design notes, and service variants.", FeatureMergeState.StagedReference, "Tracked for historical behavior and fleet-service comparison."),
        Feature("CodexMarege", "Working merge snapshot", "Diagnostic manifests, refactor status reports, and FixedGpt-near service code.", FeatureMergeState.ActiveWithReference, "Operational diagnostics and refactor cues are represented in the unified runtime map."),
        Feature("CodexMarege_restructured_Replit", "Restructured layout", "Replit source organization and staged refactor layout.", FeatureMergeState.StagedReference, "Used to validate the current structure without replacing it."),
        Feature("CodexMarege-refactor_Gethub", "Compact refactor", "Simplified project graph and source organization.", FeatureMergeState.StagedReference, "Retained as a structural comparison point."),
        Feature("EJLive_APPs", "Application artifacts", "Packaged app artifacts and non-source deliverables.", FeatureMergeState.StagedReference, "Linked as release evidence in the historical catalog."),
        Feature("EJLive_Client_v5_Enhanced", "Enhanced v5 client", "Enhanced v5 client: Agent bootstrap, startup elevation, background mode, controls, services, and v5 UI/runtime deltas.", FeatureMergeState.ActiveWithReference, "Startup planning is active and tested; remaining v5 source is preserved for focused promotion."),
        Feature("EJLive_Enterprise_20260510_build_candidate", "Build candidate", "Targeted service and project subset.", FeatureMergeState.StagedReference, "Used for focused build and service comparison."),
        Feature("EJLive_Enterprise_20260510_latest_workspace", "Latest workspace mirror", "Mirror of latest ChatGPT workspace with sync/vendor behavior.", FeatureMergeState.ActiveWithReference, "Cross-checks the active sync and vendor capability map."),
        Feature("EJLive_Enterprise_active_test_package_2026-05-10", "Enterprise test package", "Log and requirement evidence mirroring the active test package.", FeatureMergeState.TestEvidence, "Used as test evidence for parser and workflow promotion."),
        Feature("EJLive_Enterprise_v3.2.1_Enhanced", "Historical enhanced release", "Historical UI and service behavior.", FeatureMergeState.StagedReference, "Retained for behavior and UI comparison."),
        Feature("EJLive_Enterprise_v3.4.0", "Historical v3.4.0", "Older enterprise release with backup files and journal/server behavior.", FeatureMergeState.StagedReference, "Preserved as historical compatibility evidence."),
        Feature("EJLive_Enterprise_v3.4.0_Enhanced", "Enhanced v3.4.0", "Additional UI and operational-service comparison.", FeatureMergeState.StagedReference, "Queued for UI/service promotion after focused tests."),
        Feature("EJLive_Enterprise_v3.4.0_latest", "Latest v3.4.0", "Newer service and XFS/vendor behavior candidates.", FeatureMergeState.ActiveWithReference, "Contributes to vendor/XFS capability coverage."),
        Feature("EJLive_Menus_ai", "Menu design branch", "Menu-oriented UI interaction ideas.", FeatureMergeState.ExternalDesignReference, "Preserved for phase-two English menu consolidation."),
        Feature("EJLive_replik", "Replit service branch", "Source layout and service comparison.", FeatureMergeState.StagedReference, "Retained for behavior comparison."),
        Feature("EJLive-CodexMarege-fixedGpt", "FixedGpt mirror", "FleetPrediction, Windows startup, remote access, and remote-control references.", FeatureMergeState.ActiveWithReference, "Fleet prediction and Windows startup/access concepts are represented in active services."),
        Feature("EJLive.Unified", "Prior unified snapshot", "Near-active source snapshot for regression comparison.", FeatureMergeState.ActiveWithReference, "Used to prevent drift from the previous unified baseline."),
        Feature("EJLiveWorkCoder", "WorkCoder branch", "Enhanced UI and service comparison.", FeatureMergeState.ExternalDesignReference, "Preserved for UI/menu and service refinement."),
        Feature("FixedGpt", "Fixed workspace", "FleetPrediction, Windows startup, remote access, remote control, and build helpers.", FeatureMergeState.ActiveWithReference, "Startup/access helpers and fleet prediction are active or staged behind safe policy."),
        Feature("Kimi_Agent", "Independent English implementation", "Settings, log viewer, dashboard, alert, file transfer, and service-controller concepts.", FeatureMergeState.ExternalDesignReference, "Clean English UX patterns are retained for phase-two UI normalization."),
        Feature("Kimi_Agent_تحليل نظام EJLive", "Kimi analysis package", "English source content and design comparison material.", FeatureMergeState.ExternalDesignReference, "Used as external design validation without changing the active structure."),
        Feature("src", "Original src bundle", "Near-active English source with a small set of differences.", FeatureMergeState.ActiveWithReference, "Cross-checks the current source tree and installer/runtime deltas."),
        Feature("System-Analyzer-Unified_Replit", "Analyzer archive", "Analyzer state, frontend artifacts, and Ultimate enterprise snapshot.", FeatureMergeState.StudyArchive, "Retained as system-study evidence."),
        Feature("Testing_UI", "UI testing bundle", "Coder01, build-candidate, latest-workspace, and active-test copies.", FeatureMergeState.TestEvidence, "Feeds UI composition and comparison verification.")
    ];

    public static IReadOnlyList<OriginalSourceProject> Projects => SourceProjects;

    public static IReadOnlyList<OriginalSourceFeature> Features => SourceFeatures;

    public static int TotalFiles => SourceProjects.Sum(project => project.Files);

    public static int TotalCSharpFiles => SourceProjects.Sum(project => project.CSharpFiles);

    public static int TotalDifferentCSharpFromActive => SourceProjects.Sum(project => project.DifferentCSharpFromActive);

    public static IReadOnlyList<OriginalSourceProject> ProjectsWithUniqueCSharp =>
        SourceProjects.Where(project => project.DifferentCSharpFromActive > 0).ToArray();

    public static IReadOnlyList<OriginalSourceProject> ProjectsWithEnglishOnlyText =>
        SourceProjects.Where(project => project.ArabicTextFiles == 0).ToArray();

    public static IReadOnlyList<OriginalSourceFeature> FeaturesFor(string sourceName)
    {
        return SourceFeatures
            .Where(feature => string.Equals(feature.SourceName, sourceName, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public static IReadOnlyList<string> ProjectsWithoutFeatureCoverage()
    {
        var covered = SourceFeatures
            .Select(feature => feature.SourceName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return SourceProjects
            .Where(project => !covered.Contains(project.Name))
            .Select(project => project.Name)
            .ToArray();
    }

    public static IReadOnlyList<OriginalSourceFeature> ActiveOrReferencedFeatures =>
        SourceFeatures
            .Where(feature => feature.State is FeatureMergeState.ActiveWithReference or FeatureMergeState.ActiveRuntime)
            .ToArray();

    public static OriginalSourceProject FindByName(string name)
    {
        return SourceProjects.First(project => string.Equals(project.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public static OriginalSourceFeature[] BuildFeatureCoverageReport()
    {
        return SourceProjects
            .SelectMany(project => FeaturesFor(project.Name))
            .OrderBy(feature => feature.SourceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(feature => feature.Area, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static OriginalSourceProject ProjectRow(
        string name,
        SourceMergeRole role,
        int files,
        int cSharpFiles,
        int projectFiles,
        int solutionFiles,
        int formFiles,
        int serviceEngineManagerFiles,
        int configDataReferenceFiles,
        int arabicTextFiles,
        int identicalCSharpToActive,
        int differentCSharpFromActive,
        string functionalRole)
    {
        return new OriginalSourceProject(
            name,
            role,
            files,
            cSharpFiles,
            projectFiles,
            solutionFiles,
            formFiles,
            serviceEngineManagerFiles,
            configDataReferenceFiles,
            arabicTextFiles,
            identicalCSharpToActive,
            differentCSharpFromActive,
            functionalRole);
    }

    private static OriginalSourceFeature Feature(
        string sourceName,
        string area,
        string feature,
        FeatureMergeState state,
        string unifiedImplementation)
    {
        return new OriginalSourceFeature(sourceName, area, feature, state, unifiedImplementation);
    }
}

public sealed record OriginalSourceProject(
    string Name,
    SourceMergeRole Role,
    int Files,
    int CSharpFiles,
    int ProjectFiles,
    int SolutionFiles,
    int FormFiles,
    int ServiceEngineManagerFiles,
    int ConfigDataReferenceFiles,
    int ArabicTextFiles,
    int IdenticalCSharpToActive,
    int DifferentCSharpFromActive,
    string FunctionalRole);

public enum SourceMergeRole
{
    ActiveBaseline,
    ReferenceWorkspace,
    TestEvidence,
    DistArchive,
    ExternalDesign,
    StudyArchive
}

public sealed record OriginalSourceFeature(
    string SourceName,
    string Area,
    string Feature,
    FeatureMergeState State,
    string UnifiedImplementation);

public enum FeatureMergeState
{
    ActiveRuntime,
    ActiveWithReference,
    StagedReference,
    TestEvidence,
    ExternalDesignReference,
    StudyArchive
}
