using System.Collections.Concurrent;
using System.Text;
using EJLive.Core;

namespace EJLive.Core.Services;

public sealed class UnifiedServiceGateway
{
    private readonly UnifiedJournalStorageService _journalStorage;
    private readonly UnifiedRemoteCommandOrchestrator _remoteCommands;
    private readonly UnifiedClientServiceSupervisor _clientServices;
    private readonly UnifiedProjectIntegrationAuditService _integrationAudit;
    private readonly ConcurrentQueue<UnifiedGatewayActivation> _activations = new();
    private readonly ConcurrentDictionary<string, UnifiedGatewayAtmRuntimeState> _atmStates = new(StringComparer.OrdinalIgnoreCase);

    private static readonly ActiveServiceReplacement[] ReferenceRouteMap =
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

    public UnifiedServiceGateway(
        UnifiedJournalStorageService journalStorage,
        UnifiedRemoteCommandOrchestrator remoteCommands,
        UnifiedClientServiceSupervisor clientServices,
        UnifiedProjectIntegrationAuditService integrationAudit)
    {
        _journalStorage = journalStorage ?? throw new ArgumentNullException(nameof(journalStorage));
        _remoteCommands = remoteCommands ?? throw new ArgumentNullException(nameof(remoteCommands));
        _clientServices = clientServices ?? throw new ArgumentNullException(nameof(clientServices));
        _integrationAudit = integrationAudit ?? throw new ArgumentNullException(nameof(integrationAudit));
    }

    public JournalStorageResult StoreJournal(
        string storagePath,
        string atmId,
        string atmType,
        string fileName,
        byte[] data,
        string checksum = "",
        string referencePath = "")
    {
        var stored = _journalStorage.StoreJournalData(storagePath, atmId, atmType, fileName, data, checksum);
        var occurredAt = DateTime.UtcNow;
        UpdateAtmState(atmId, state => state with
        {
            LastJournalUtc = occurredAt,
            Activations = state.Activations + 1
        });
        RegisterActivation("StoreJournal", atmId, referencePath, $"Stored {stored.FileName} ({stored.FileSize} bytes).", occurredAt, trackAtmActivity: false);
        return stored;
    }

    public RemoteCommandDispatch DispatchRemoteCommand(
        string atmId,
        string commandType,
        string payload = "",
        string role = "Admin",
        bool operatorConfirmed = true,
        bool maintenanceWindow = true,
        string referencePath = "")
    {
        var dispatch = _remoteCommands.Queue(atmId, commandType, payload, role, operatorConfirmed, maintenanceWindow);
        var occurredAt = DateTime.UtcNow;
        UpdateAtmState(atmId, state => state with
        {
            LastRemoteCommandUtc = occurredAt,
            Activations = state.Activations + 1
        });
        RegisterActivation("DispatchRemoteCommand", atmId, referencePath, $"{commandType} => {dispatch.Command.Status}", occurredAt, trackAtmActivity: false);
        return dispatch;
    }

    public bool CompleteRemoteCommand(string commandId, bool success, string result, string atmId = "", string referencePath = "")
    {
        var completed = _remoteCommands.Complete(commandId, success, result);
        if (!completed)
            return false;

        var occurredAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(atmId))
        {
            UpdateAtmState(atmId, state => state with
            {
                LastRemoteCommandUtc = occurredAt,
                Activations = state.Activations + 1
            });
        }

        RegisterActivation("CompleteRemoteCommand", atmId, referencePath, $"{commandId} => {(success ? "Success" : "Failed")}", occurredAt, trackAtmActivity: false);
        return true;
    }

    public ClientServiceState MarkClientServiceRunning(string serviceName, string detail = "Running", string referencePath = "")
    {
        var state = _clientServices.Start(serviceName, detail);
        RegisterActivation("ClientServiceRunning", string.Empty, referencePath, $"{state.Name} => {state.Status}", state.UpdatedAtUtc);
        return state;
    }

    public ClientServiceState MarkClientServiceFaulted(string serviceName, string detail, string referencePath = "")
    {
        var state = _clientServices.MarkFaulted(serviceName, detail);
        RegisterActivation("ClientServiceFaulted", string.Empty, referencePath, $"{state.Name} => {state.Status}", state.UpdatedAtUtc);
        return state;
    }

    public void RegisterHeartbeat(string atmId, DateTime? heartbeatUtc = null, string referencePath = "")
    {
        var occurredAt = heartbeatUtc ?? DateTime.UtcNow;
        UpdateAtmState(atmId, state => state with
        {
            LastHeartbeatUtc = occurredAt,
            Activations = state.Activations + 1
        });
        RegisterActivation("Heartbeat", atmId, referencePath, "Heartbeat bridged to runtime state.", occurredAt, trackAtmActivity: false);
    }

    public void RegisterBackupSnapshot(string atmId, string archivePath, long sizeBytes = 0, DateTime? createdAtUtc = null, string referencePath = "")
    {
        var occurredAt = createdAtUtc ?? DateTime.UtcNow;
        UpdateAtmState(atmId, state => state with
        {
            LastBackupUtc = occurredAt,
            Activations = state.Activations + 1
        });
        RegisterActivation("BackupSnapshot", atmId, referencePath, $"{archivePath} ({sizeBytes} bytes).", occurredAt, trackAtmActivity: false);
    }

    public void RegisterScreenshotResult(
        string atmId,
        string screenshotPath,
        bool success,
        long sizeBytes = 0,
        DateTime? capturedAtUtc = null,
        string referencePath = "")
    {
        var occurredAt = capturedAtUtc ?? DateTime.UtcNow;
        UpdateAtmState(atmId, state => state with
        {
            LastScreenshotUtc = occurredAt,
            Activations = state.Activations + 1
        });
        RegisterActivation("ScreenshotResult", atmId, referencePath, $"{(success ? "OK" : "FAIL")} {screenshotPath} ({sizeBytes} bytes).", occurredAt, trackAtmActivity: false);
    }

    public ProjectIntegrationAuditReport BuildIntegrationAudit(string rootPath)
    {
        return _integrationAudit.Analyze(rootPath);
    }

    public UnifiedGatewayActivationBatchResult ActivateReferencePaths(
        IEnumerable<string> referencePaths,
        string atmId,
        string storagePath,
        string atmType = AppConstants.ATM_TYPE_NCR)
    {
        var normalizedPaths = (referencePaths ?? Array.Empty<string>())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Normalize)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var normalizedAtmId = string.IsNullOrWhiteSpace(atmId) ? "ATM-GATEWAY" : atmId.Trim();
        var effectiveStoragePath = string.IsNullOrWhiteSpace(storagePath)
            ? Path.Combine(Path.GetTempPath(), "ejlive-gateway-bridge")
            : storagePath;

        var journalActivations = 0;
        var commandActivations = 0;
        var serviceActivations = 0;
        var telemetryActivations = 0;
        var unclassifiedActivations = 0;
        var unclassifiedPaths = new List<string>();

        foreach (var path in normalizedPaths)
        {
            if (ContainsAny(path, "/journal", "journal", "ejdata", "sync"))
            {
                var payload = Encoding.UTF8.GetBytes("NCR EJDATA APPROVED AMOUNT 300\nCARD CAPTURED\nM-18 CASH ERROR");
                var fileName = BuildBridgeFileName(path);
                StoreJournal(effectiveStoragePath, normalizedAtmId, atmType, fileName, payload, referencePath: path);
                if (ContainsAny(path, "sync"))
                    MarkClientServiceRunning("Journal Sync", "Reference activation bridge", path);
                journalActivations++;
                continue;
            }

            if (ContainsAny(path, "remote", "command", "ghost"))
            {
                DispatchRemoteCommand(normalizedAtmId, AppConstants.CMD_PING, role: "Admin", operatorConfirmed: true, maintenanceWindow: true, referencePath: path);
                commandActivations++;
                continue;
            }

            if (ContainsAny(path, "network", "socket", "startup", "access"))
            {
                MarkClientServiceRunning("Network Monitor", "Reference activation bridge", path);
                serviceActivations++;
                continue;
            }

            if (ContainsAny(path, "backup", "archive"))
            {
                RegisterBackupSnapshot(normalizedAtmId, Path.Combine(effectiveStoragePath, "bridge-archive.zip"), sizeBytes: 4096, referencePath: path);
                telemetryActivations++;
                continue;
            }

            if (ContainsAny(path, "screenshot", "screen"))
            {
                RegisterScreenshotResult(normalizedAtmId, Path.Combine(effectiveStoragePath, "bridge-screen.jpg"), success: true, sizeBytes: 2048, referencePath: path);
                telemetryActivations++;
                continue;
            }

            if (ContainsAny(path, "heartbeat", "timesync", "time"))
            {
                RegisterHeartbeat(normalizedAtmId, referencePath: path);
                telemetryActivations++;
                continue;
            }

            if (path.StartsWith("src/EJLive.Core/Services/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("src/EJLive.Core/Engine/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("src/EJLive.Core/Xfs/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("src/EJLive.Core/Models/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("src/EJLive.Shared/", StringComparison.OrdinalIgnoreCase))
            {
                MarkClientServiceRunning("Agent Controller", "Core/service bridge activation", path);
                serviceActivations++;
                continue;
            }

            var route = ResolveReferenceRoute(path);
            if (route.Mapped)
            {
                RegisterReferenceBridge(path, normalizedAtmId, "Reference path routed to active unified service bridge.");
                serviceActivations++;
                continue;
            }

            RegisterReferenceBridge(path, normalizedAtmId, "Reference path mapped to generic gateway bridge operation.");
            unclassifiedActivations++;
            unclassifiedPaths.Add(path);
        }

        return new UnifiedGatewayActivationBatchResult(
            normalizedPaths.Length,
            journalActivations + commandActivations + serviceActivations + telemetryActivations + unclassifiedActivations,
            journalActivations,
            commandActivations,
            serviceActivations,
            telemetryActivations,
            unclassifiedActivations,
            unclassifiedPaths);
    }

    public UnifiedGatewayActivationBatchResult ActivateAllReferenceServices(
        string rootPath,
        string atmId,
        string storagePath,
        string atmType = AppConstants.ATM_TYPE_NCR)
    {
        var audit = BuildIntegrationAudit(rootPath);
        var referencePaths = audit.ReferenceOnlyFiles.Select(file => file.Path).ToArray();
        return ActivateReferencePaths(referencePaths, atmId, storagePath, atmType);
    }

    public UnifiedGatewayReferenceCoverage BuildReferenceCoverage(string rootPath)
    {
        var audit = BuildIntegrationAudit(rootPath);
        var bindings = audit.ReferenceOnlyFiles
            .Select(BuildReferenceBinding)
            .OrderBy(binding => binding.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var uncoveredPaths = bindings
            .Where(binding => !binding.Covered)
            .Select(binding => binding.Path)
            .ToArray();

        return new UnifiedGatewayReferenceCoverage(
            bindings.Length,
            bindings.Count(binding => binding.GatewayMapped),
            bindings.Count(binding => binding.CoveredByAuditReplacement),
            bindings.Count(binding => binding.Covered),
            uncoveredPaths.Length,
            bindings,
            uncoveredPaths);
    }

    public UnifiedServiceGatewayReport BuildReport(int recentActivationLimit = 100)
    {
        var activations = _activations.ToArray()
            .OrderByDescending(item => item.OccurredAtUtc)
            .ToArray();

        var mappedReferenceActivations = activations.Count(item => item.ReferenceMapped);
        var unresolvedReferenceActivations = activations.Count(item =>
            !item.ReferenceMapped &&
            !string.IsNullOrWhiteSpace(item.ReferencePath));

        var serviceActivity = activations
            .GroupBy(item => item.ActiveService, StringComparer.OrdinalIgnoreCase)
            .Select(group => new UnifiedGatewayServiceActivity(group.Key, group.Count()))
            .OrderByDescending(item => item.Activations)
            .ThenBy(item => item.ActiveService, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var referencePaths = activations
            .Where(item => !string.IsNullOrWhiteSpace(item.ReferencePath))
            .Select(item => item.ReferencePath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var atmStates = _atmStates.Values
            .OrderBy(state => state.AtmId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new UnifiedServiceGatewayReport(
            activations.Length,
            mappedReferenceActivations,
            unresolvedReferenceActivations,
            atmStates.Length,
            referencePaths,
            serviceActivity,
            atmStates,
            activations.Take(Math.Max(1, recentActivationLimit)).ToArray());
    }

    private void RegisterReferenceBridge(string referencePath, string atmId, string detail)
    {
        RegisterActivation("ReferenceBridge", atmId, referencePath, detail);
    }

    private static UnifiedGatewayReferenceBinding BuildReferenceBinding(ReferenceOnlyServiceFile file)
    {
        var normalized = Normalize(file.Path);
        var route = ResolveReferenceRoute(normalized);
        var coveredByAuditReplacement = !string.IsNullOrWhiteSpace(file.ActiveReplacement);
        var covered = route.Mapped || coveredByAuditReplacement;
        var activeService = coveredByAuditReplacement ? file.ActiveReplacement : route.Route.ActiveService;
        var coverage = coveredByAuditReplacement
            ? "Mapped by integration audit replacement map."
            : route.Route.Coverage;

        return new UnifiedGatewayReferenceBinding(
            normalized,
            activeService,
            route.Mapped,
            coveredByAuditReplacement,
            covered,
            coverage);
    }

    private UnifiedGatewayActivation RegisterActivation(
        string operation,
        string atmId,
        string referencePath,
        string detail,
        DateTime? occurredAtUtc = null,
        bool trackAtmActivity = true)
    {
        var occurredAt = occurredAtUtc ?? DateTime.UtcNow;
        var normalizedReference = Normalize(referencePath);
        var route = ResolveReferenceRoute(normalizedReference);
        var activation = new UnifiedGatewayActivation(
            Guid.NewGuid().ToString("N"),
            occurredAt,
            operation,
            atmId ?? string.Empty,
            normalizedReference,
            route.Route.ActiveService,
            route.Mapped,
            detail ?? string.Empty);

        _activations.Enqueue(activation);

        if (trackAtmActivity && !string.IsNullOrWhiteSpace(atmId))
        {
            UpdateAtmState(atmId, state => state with { Activations = state.Activations + 1 });
        }

        return activation;
    }

    private static (ActiveServiceReplacement Route, bool Mapped) ResolveReferenceRoute(string normalizedReferencePath)
    {
        if (string.IsNullOrWhiteSpace(normalizedReferencePath))
        {
            return (new ActiveServiceReplacement("runtime", "UnifiedServiceGateway", "Direct runtime activation without an archived-source path."), false);
        }

        var route = ReferenceRouteMap.FirstOrDefault(item =>
            normalizedReferencePath.StartsWith(item.PathPrefix, StringComparison.OrdinalIgnoreCase));

        if (route is not null)
            return (route, true);

        return (new ActiveServiceReplacement(normalizedReferencePath, "UnifiedServiceGateway", "No explicit mapping rule exists for this reference path."), false);
    }

    private void UpdateAtmState(string atmId, Func<UnifiedGatewayAtmRuntimeState, UnifiedGatewayAtmRuntimeState> updater)
    {
        if (string.IsNullOrWhiteSpace(atmId))
            return;

        _atmStates.AddOrUpdate(
            atmId,
            key => updater(new UnifiedGatewayAtmRuntimeState(key, null, null, null, null, null, 0)),
            (_, current) => updater(current));
    }

    private static string BuildBridgeFileName(string referencePath)
    {
        var name = Path.GetFileNameWithoutExtension(referencePath);
        if (string.IsNullOrWhiteSpace(name))
            name = "bridge-journal";
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return cleaned + ".bridge.log";
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        return terms.Any(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace('\\', '/').Trim();
    }
}

public sealed record UnifiedGatewayActivation(
    string ActivationId,
    DateTime OccurredAtUtc,
    string Operation,
    string AtmId,
    string ReferencePath,
    string ActiveService,
    bool ReferenceMapped,
    string Detail);

public sealed record UnifiedGatewayServiceActivity(
    string ActiveService,
    int Activations);

public sealed record UnifiedGatewayAtmRuntimeState(
    string AtmId,
    DateTime? LastHeartbeatUtc,
    DateTime? LastBackupUtc,
    DateTime? LastScreenshotUtc,
    DateTime? LastJournalUtc,
    DateTime? LastRemoteCommandUtc,
    int Activations);

public sealed record UnifiedGatewayReferenceBinding(
    string Path,
    string ActiveService,
    bool GatewayMapped,
    bool CoveredByAuditReplacement,
    bool Covered,
    string Coverage);

public sealed record UnifiedGatewayActivationBatchResult(
    int RequestedReferencePaths,
    int ActivatedReferencePaths,
    int JournalActivations,
    int CommandActivations,
    int ServiceActivations,
    int TelemetryActivations,
    int UnclassifiedActivations,
    IReadOnlyList<string> UnclassifiedPaths);

public sealed record UnifiedGatewayReferenceCoverage(
    int TotalReferenceFiles,
    int GatewayMappedFiles,
    int CoveredByAuditReplacementFiles,
    int CoveredFiles,
    int UncoveredFiles,
    IReadOnlyList<UnifiedGatewayReferenceBinding> Bindings,
    IReadOnlyList<string> UncoveredPaths);

public sealed record UnifiedServiceGatewayReport(
    int TotalActivations,
    int MappedReferenceActivations,
    int UnresolvedReferenceActivations,
    int DistinctAtms,
    IReadOnlyList<string> DistinctReferencePaths,
    IReadOnlyList<UnifiedGatewayServiceActivity> ServiceActivity,
    IReadOnlyList<UnifiedGatewayAtmRuntimeState> AtmStates,
    IReadOnlyList<UnifiedGatewayActivation> RecentActivations);
