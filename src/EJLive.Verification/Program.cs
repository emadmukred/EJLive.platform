using System.Data.SQLite;
using System.Windows.Forms;
using EJLive.Application;
using EJLive.Business;
using EJLive.Client.WinForms;
using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Installer.WinForms;
using EJLive.Monitoring.WinForms;
using EJLive.Server.Services;
using EJLive.Server.WinForms;
using EJLive.Shared;
using AppConstants = EJLive.Core.AppConstants;
using CoreUnifiedClientServiceSupervisor = EJLive.Core.Services.UnifiedClientServiceSupervisor;
using CoreUnifiedJournalStorageService = EJLive.Core.Services.UnifiedJournalStorageService;
using CoreUnifiedRemoteCommandOrchestrator = EJLive.Core.Services.UnifiedRemoteCommandOrchestrator;

var checks = new List<(string Name, bool Passed, string Detail)>
{
    RunDatabaseMigrationProbe(),
    RunApplicationLayerProbe(),
    await RunNetworkProbeAsync(),
    await RunRemoteCommandProbeAsync(),
    await RunClientTelemetryProbeAsync(),
    await RunPulseJsonTelemetryProbeAsync(),
    await RunJournalAckMetadataProbeAsync(),
    await RunFileWatcherProbeAsync(),
    RunUiProbe(),
    RunRuntimeCapabilityProbe(),
    RunBusinessServiceCompositionProbe(),
    RunRemoteOperationsSurfaceProbe(),
    RunOperationalFusionProbe(),
    RunUnifiedServiceOperationsProbe(),
    RunOperationalReportingProbe(),
    RunOperationalReportCatalogProbe(),
    RunClientTelemetryAnalyticsProbe(),
    await RunServerOperationalServicesProbe(),
    RunSourceTruthProbe(),
    RunFileLinkageProbe(),
    RunUnsafeTermScanProbe(),
    RunUiInServicePathProbe(),
    RunDuplicateTypeProbe()
};

foreach (var check in checks)
{
    var state = check.Passed ? "PASS" : "FAIL";
    Console.WriteLine($"{state} {check.Name}: {check.Detail}");
}

static async Task<(string Name, bool Passed, string Detail)> RunRemoteCommandProbeAsync()
{
    var port = GetAvailableTcpPort();
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    using var commandResult = new ManualResetEventSlim(false);
    var detail = string.Empty;

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.Log += (_, message) =>
        {
            if (message.Contains("Command result", StringComparison.OrdinalIgnoreCase))
            {
                detail = message;
                commandResult.Set();
            }
        };
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-COMMAND", AppConstants.ATM_TYPE_NCR);
        client.OnMessageReceived += (_, message) =>
        {
            if (message.Type == CommunicationProtocol.MsgType.Command &&
                RemoteCommandEnvelope.TryParse(message.Text, out var command))
            {
                client.SendMessage(CommunicationProtocol.BuildCommandResult(command.CommandId, true, $"{command.CommandType} acknowledged by probe."));
            }
        };

        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));
        var sent = server.SendCommand("ATM-COMMAND", new RemoteCommandEnvelope { CommandType = AppConstants.CMD_PING });
        var resultReceived = commandResult.Wait(TimeSpan.FromSeconds(5));
        client.Disconnect();
        server.Stop();

        var passed = connectReturned && accepted && sent && resultReceived;
        return ("Remote command routing", passed, $"connectReturned={connectReturned}, accepted={accepted}, sent={sent}, resultReceived={resultReceived}, detail={detail}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Remote command routing", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunFileWatcherProbeAsync()
{
    var folder = Path.Combine(Path.GetTempPath(), "ejlive-filewatcher-probe", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(folder);
    using var watcher = new FileWatcherEngine { PollInterval = TimeSpan.FromSeconds(1) };
    using var detected = new ManualResetEventSlim(false);
    var detectedFile = string.Empty;

    try
    {
        watcher.FileChanged += (_, file) =>
        {
            detectedFile = Path.GetFileName(file);
            detected.Set();
        };
        watcher.Start(folder);
        var path = Path.Combine(folder, "EJDATA.LOG");
        await File.WriteAllTextAsync(path, "probe").ConfigureAwait(false);
        var passed = detected.Wait(TimeSpan.FromSeconds(5));
        watcher.Stop();
        Directory.Delete(folder, recursive: true);
        return ("File watcher fallback", passed, $"detected={passed}, file={detectedFile}");
    }
    catch (Exception ex)
    {
        try { watcher.Stop(); } catch { }
        try { Directory.Delete(folder, recursive: true); } catch { }
        return ("File watcher fallback", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunClientTelemetryProbeAsync()
{
    var port = GetAvailableTcpPort();
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    using var telemetryReceived = new ManualResetEventSlim(false);
    ClientTelemetryPacket? captured = null;

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.ClientTelemetryReceived += (_, packet) =>
        {
            captured = packet;
            telemetryReceived.Set();
        };
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-TEL-PROBE", AppConstants.ATM_TYPE_NCR);
        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));

        var detail = "probe=telemetry";
        var payload =
            "TELEMETRY|ATM=ATM-TEL-PROBE;Type=probe_event;Severity=info;Utc=" + DateTime.UtcNow.ToString("O") +
            ";DetailB64=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(detail));
        client.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, payload));

        var received = telemetryReceived.Wait(TimeSpan.FromSeconds(5));
        client.Disconnect();
        server.Stop();

        var passed = connectReturned &&
                     accepted &&
                     received &&
                     captured is not null &&
                     captured.ATM_ID == "ATM-TEL-PROBE" &&
                     captured.EventType == "probe_event" &&
                     captured.Detail == detail;
        var detailText = captured is null
            ? "none"
            : $"{captured.ATM_ID}/{captured.Severity}/{captured.EventType}";
        return ("Client telemetry stream", passed, $"connectReturned={connectReturned}, accepted={accepted}, received={received}, packet={detailText}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Client telemetry stream", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunPulseJsonTelemetryProbeAsync()
{
    var port = GetAvailableTcpPort();
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    using var telemetryReceived = new ManualResetEventSlim(false);
    ClientTelemetryPacket? captured = null;

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.ClientTelemetryReceived += (_, packet) =>
        {
            if (packet.EventType.Equals("pulse_json", StringComparison.OrdinalIgnoreCase))
            {
                captured = packet;
                telemetryReceived.Set();
            }
        };
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-PULSEJSON", AppConstants.ATM_TYPE_NCR);
        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));

        var now = DateTime.UtcNow;
        var payload = "{\"terminalId\":\"ATM-PULSEJSON\",\"timestampUtc\":\"" + now.ToString("O") +
                      "\",\"serviceState\":\"connected\",\"handshake\":true,\"pendingOutbox\":2,\"networkType\":\"lan\"}";
        client.SendMessage(CommunicationProtocol.BuildFrame(CommunicationProtocol.MsgType.Broadcast, "PULSE_JSON|" + payload));

        var received = telemetryReceived.Wait(TimeSpan.FromSeconds(5));
        client.Disconnect();
        server.Stop();

        var passed = connectReturned &&
                     accepted &&
                     received &&
                     captured is not null &&
                     captured.ATM_ID == "ATM-PULSEJSON" &&
                     captured.EventType == "pulse_json" &&
                     captured.RawJson == payload &&
                     captured.Detail.Contains("pending=2", StringComparison.OrdinalIgnoreCase);
        var detailText = captured is null
            ? "none"
            : $"{captured.ATM_ID}/{captured.Severity}/{captured.EventType}";
        return ("Client pulse_json telemetry", passed, $"connectReturned={connectReturned}, accepted={accepted}, received={received}, packet={detailText}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Client pulse_json telemetry", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunJournalAckMetadataProbeAsync()
{
    var failures = new List<string>();

    for (var attempt = 1; attempt <= 3; attempt++)
    {
        var result = await RunJournalAckMetadataAttemptAsync(attempt).ConfigureAwait(false);
        if (result.Passed)
            return ("Journal ACK metadata", true, result.Detail);

        failures.Add(result.Detail);
        await Task.Delay(250).ConfigureAwait(false);
    }

    return ("Journal ACK metadata", false, string.Join("; ", failures));
}

static async Task<(bool Passed, string Detail)> RunJournalAckMetadataAttemptAsync(int attempt)
{
    var port = GetAvailableTcpPort();
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    using var ackReceived = new ManualResetEventSlim(false);
    var ack = string.Empty;

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-ACK-PROBE", AppConstants.ATM_TYPE_NCR);
        client.OnJournalAcknowledged += (_, text) =>
        {
            ack = text;
            ackReceived.Set();
        };

        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));

        var data = System.Text.Encoding.UTF8.GetBytes("ACK-PROBE-DATA");
        var checksum = SecurityHelper.MD5Hash(data);
        var sent = client.SendJournalFile("EJDATA.LOG", data, 0, checksum);
        var received = ackReceived.Wait(TimeSpan.FromSeconds(8));

        client.Disconnect();
        server.Stop();

        var passed = connectReturned &&
                     accepted &&
                     sent &&
                     received &&
                     ack.Contains("|OK|", StringComparison.OrdinalIgnoreCase) &&
                     ack.Contains("sha256=", StringComparison.OrdinalIgnoreCase) &&
                     ack.Contains("size=", StringComparison.OrdinalIgnoreCase) &&
                     ack.Contains("staging_time_ms=", StringComparison.OrdinalIgnoreCase);
        return (passed, $"attempt={attempt}, connectReturned={connectReturned}, accepted={accepted}, sent={sent}, received={received}, ack={ack}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return (false, $"attempt={attempt}, error={ex.Message}");
    }
}

static (string Name, bool Passed, string Detail) RunUiProbe()
{
    (string Name, bool Passed, string Detail) result = ("WinForms UI composition", false, "not run");
    var thread = new Thread(() =>
    {
        try
        {
            Environment.SetEnvironmentVariable(
                "EJLIVE_DATABASE_PATH",
                Path.Combine(Path.GetTempPath(), $"ejlive-ui-probe-{Guid.NewGuid():N}.db"));
            ApplicationConfiguration.Initialize();
            using var client = new ClientMainForm();
            using var server = new ServerMainForm();
            using var monitoring = new MainDashboardForm();
            using var installer = new InstallerForm();

            var clientTabs = GetTabs(client);
            var serverTabs = GetTabs(server);
            var monitoringTabs = GetTabs(monitoring);
            var clientButtonTexts = EnumerateControls(client)
                .OfType<Button>()
                .Select(button => button.Text.Trim())
                .Where(text => text.Length > 0)
                .ToArray();
            var clientLabels = EnumerateControls(client)
                .OfType<Label>()
                .Select(label => label.Text.Trim())
                .Where(text => text.Length > 0)
                .ToArray();
            var clientGrids = EnumerateControls(client).OfType<DataGridView>().ToArray();
            var serverButtons = CountControls<Button>(server);
            var monitoringButtons = CountControls<Button>(monitoring);
            var installerButtons = CountControls<Button>(installer);
            var forbiddenClientActions = new[] { "Connect", "Restart", "Force", "Shutdown", "Execute", "Send Command" };
            var clientIsPassiveCompanion =
                clientTabs.Length == 0 &&
                clientButtonTexts.Length == 2 &&
                clientButtonTexts.Any(text => text.Equals("Refresh", StringComparison.OrdinalIgnoreCase)) &&
                clientButtonTexts.Any(text => text is "العربية" or "English") &&
                !forbiddenClientActions.Any(action => clientButtonTexts.Any(text =>
                    text.Contains(action, StringComparison.OrdinalIgnoreCase))) &&
                ContainsAll(clientLabels, "Agent Service", "Server Link", "Heartbeat", "Journal Sync", "ATM", "Session", "Pending", "Traffic", "Last error") &&
                clientGrids.Any(grid => grid.ReadOnly && !grid.AllowUserToAddRows && !grid.AllowUserToDeleteRows);

            var passed =
                clientIsPassiveCompanion &&
                ContainsAll(serverTabs, "Fleet", "Network Map", "Journal Viewer", "Sync Dashboard", "Remote Commands", "Alerts", "Archive", "Reports", "Settings") &&
                ContainsAll(monitoringTabs, "Overview", "Operational Map", "Device State", "Realtime Sync", "XFS Events", "Vendor Logs", "Reports") &&
                serverButtons >= 25 &&
                monitoringButtons >= 8 &&
                installerButtons >= 4;

            result = ("WinForms UI composition", passed,
                $"clientPassive={clientIsPassiveCompanion}; clientButtons={string.Join(',', clientButtonTexts)}; clientGrids={clientGrids.Length}; serverTabs={string.Join(',', serverTabs)}; monitoringTabs={string.Join(',', monitoringTabs)}; buttons={serverButtons}/{monitoringButtons}/{installerButtons}");
        }
        catch (Exception ex)
        {
            result = ("WinForms UI composition", false, ex.Message);
        }
    });
    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
    thread.Join();
    return result;
}

static string[] GetTabs(Form form)
{
    return EnumerateControls(form)
        .OfType<TabControl>()
        .SelectMany(t => t.TabPages.Cast<TabPage>())
        .Select(t => t.Text)
        .ToArray();
}

static int CountControls<T>(Control root) where T : Control => EnumerateControls(root).OfType<T>().Count();

static IEnumerable<Control> EnumerateControls(Control root)
{
    foreach (Control child in root.Controls)
    {
        yield return child;
        foreach (var descendant in EnumerateControls(child))
            yield return descendant;
    }
}

static bool ContainsAll(IEnumerable<string> actual, params string[] expected)
{
    var set = new HashSet<string>(actual, StringComparer.OrdinalIgnoreCase);
    return expected.All(set.Contains);
}

static (string Name, bool Passed, string Detail) RunFileLinkageProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var srcRoot = Path.Combine(root, "src");
        var solutionProjects = ReadSolutionProjects(root);
        var missingSolutionProjects = solutionProjects
            .Where(project => !File.Exists(project))
            .Select(project => ToRelativePath(root, project))
            .ToArray();

        var reachableProjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pendingProjects = new Queue<string>(solutionProjects.Where(File.Exists));
        var missingReferences = new List<string>();
        var externalReferences = new List<string>();

        while (pendingProjects.Count > 0)
        {
            var project = Path.GetFullPath(pendingProjects.Dequeue());
            if (!reachableProjects.Add(project))
                continue;

            foreach (var reference in ReadProjectReferences(project))
            {
                if (!IsWithinRoot(root, reference))
                {
                    externalReferences.Add($"{ToRelativePath(root, project)} -> {reference}");
                    continue;
                }

                if (!File.Exists(reference))
                {
                    missingReferences.Add($"{ToRelativePath(root, project)} -> {ToRelativePath(root, reference)}");
                    continue;
                }

                pendingProjects.Enqueue(reference);
            }
        }

        var verificationProject = Path.Combine(srcRoot, "EJLive.Verification", "EJLive.Verification.csproj");
        var verificationIsReachable = reachableProjects.Contains(Path.GetFullPath(verificationProject));
        var passed = solutionProjects.Count > 0 &&
                      verificationIsReachable &&
                      missingSolutionProjects.Length == 0 &&
                      missingReferences.Count == 0 &&
                      externalReferences.Count == 0;

        return ("Project file linkage", passed,
            $"solutionProjects={solutionProjects.Count}, reachableProjects={reachableProjects.Count}, verificationReachable={verificationIsReachable}, missingProjects={string.Join(',', missingSolutionProjects)}, missingReferences={string.Join(';', missingReferences.Take(5))}, externalReferences={string.Join(';', externalReferences.Take(5))}");
    }
    catch (Exception ex)
    {
        return ("Project file linkage", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunSourceTruthProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var solutionPath = FindSolutionFile(root);
        var solutionProjects = ReadSolutionProjects(root);
        var missingProjects = solutionProjects
            .Where(project => !File.Exists(project))
            .Select(project => ToRelativePath(root, project))
            .ToArray();
        var duplicateProjects = solutionProjects
            .GroupBy(project => Path.GetFullPath(project), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => ToRelativePath(root, group.Key))
            .ToArray();
        var outsideProjects = solutionProjects
            .Where(project => !IsWithinRoot(root, project))
            .Select(project => project)
            .ToArray();
        var verificationPresent = solutionProjects.Any(project =>
            project.EndsWith(Path.Combine("EJLive.Verification", "EJLive.Verification.csproj"), StringComparison.OrdinalIgnoreCase));

        var passed = solutionProjects.Count >= 8 &&
                     verificationPresent &&
                     missingProjects.Length == 0 &&
                     duplicateProjects.Length == 0 &&
                     outsideProjects.Length == 0;
        return ("Solution source truth", passed,
            $"solution={Path.GetFileName(solutionPath)}, projects={solutionProjects.Count}, verificationPresent={verificationPresent}, missing={string.Join(',', missingProjects)}, duplicates={string.Join(',', duplicateProjects)}, outside={string.Join(',', outsideProjects)}");
    }
    catch (Exception ex)
    {
        return ("Source truth document", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunRuntimeCapabilityProbe()
{
    try
    {
        using var runtime = new UnifiedBusinessRuntime();
        var capabilities = runtime.BuildCapabilities();
        var required = new[]
        {
            "Fleet State",
            "Journal Sync",
            "Journal Evidence",
            "Journal Storage",
            "Remote Command Governance",
            "Client Service Supervision",
            "Vendor Intelligence",
            "Protocol and Security",
            "SQLite Store",
            "Workflow Coordination"
        };
        var names = capabilities.Select(capability => capability.Name).ToArray();
        var distinct = capabilities
            .Select(capability => $"{capability.Layer}:{capability.Name}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var allowedCommands = runtime.RemoteCommandPolicy.GetAllowedCommands();

        var passed = required.All(name => names.Contains(name, StringComparer.OrdinalIgnoreCase)) &&
                     distinct == capabilities.Count &&
                     allowedCommands.Contains(AppConstants.CMD_PING, StringComparer.OrdinalIgnoreCase) &&
                     allowedCommands.Contains(AppConstants.CMD_RESTART, StringComparer.OrdinalIgnoreCase);

        return ("Runtime capability surface", passed,
            $"capabilities={capabilities.Count}, distinct={distinct}, required={required.Length}, commands={allowedCommands.Count}");
    }
    catch (Exception ex)
    {
        return ("Runtime capability surface", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunBusinessServiceCompositionProbe()
{
    try
    {
        using var runtime = new UnifiedBusinessRuntime();
        var registered = runtime.ClientServiceSupervisor.Register("VERIFY-AGENT", "ATM-VERIFY", "127.0.0.1", AppConstants.DefaultPort);
        var heartbeatUpdated = runtime.ClientServiceSupervisor.UpdateHeartbeat(registered.AgentId);
        var journal = runtime.JournalStorage.Analyze(
            "ATM-VERIFY",
            "NCR EJDATA APPROVED AMOUNT 250\nREVERSAL COMPLETED\nM-18 CASH ERROR");
        var lowRisk = runtime.RemoteCommandPolicy.Validate(AppConstants.CMD_PING, "Support", operatorConfirmed: false, maintenanceWindow: false);
        var deniedCritical = runtime.RemoteCommandPolicy.Validate(AppConstants.CMD_RESTART, "Support", operatorConfirmed: true, maintenanceWindow: true);
        var allowedCritical = runtime.RemoteCommandPolicy.Validate(AppConstants.CMD_RESTART, "Admin", operatorConfirmed: true, maintenanceWindow: true);
        var unknown = runtime.RemoteCommandPolicy.Validate("UNAPPROVED_COMMAND", "Admin", operatorConfirmed: true, maintenanceWindow: true);

        var passed = heartbeatUpdated &&
                     registered.Status == "Healthy" &&
                     journal.TotalLines == 3 &&
                     journal.ApprovedCount == 1 &&
                     journal.ReversalCount == 1 &&
                     lowRisk.Allowed &&
                     !deniedCritical.Allowed &&
                     allowedCritical.Allowed &&
                     !unknown.Allowed;

        return ("Business service composition", passed,
            $"heartbeat={heartbeatUpdated}, journalLines={journal.TotalLines}, lowRisk={lowRisk.Allowed}, supportCritical={deniedCritical.Allowed}, adminCritical={allowedCritical.Allowed}, unknown={unknown.Allowed}");
    }
    catch (Exception ex)
    {
        return ("Business service composition", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunRemoteOperationsSurfaceProbe()
{
    try
    {
        var diagnostics = new ControlledDiagnosticCommandService();
        DiagnosticCommandResult rejected = diagnostics.ExecutePreset("UNAPPROVED_DIAGNOSTIC");
        var sessions = new RemoteAssistanceStream();
        var active = sessions.Start("ATM-REMOTE");
        sessions.Stop();
        var stopped = sessions.CurrentSession;

        var passed = rejected.Blocked &&
                     !rejected.Success &&
                     active.ATM_ID == "ATM-REMOTE" &&
                     stopped?.Status == RemoteSessionState.Stopped &&
                     stopped.EndedAtUtc.HasValue;

        return ("Remote operations surface", passed,
            $"diagnosticBlocked={rejected.Blocked}, session={stopped?.Status}, ended={stopped?.EndedAtUtc.HasValue}");
    }
    catch (Exception ex)
    {
        return ("Remote operations surface", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunOperationalFusionProbe()
{
    try
    {
        using var runtime = new UnifiedBusinessRuntime();
        var atm = runtime.RegisterAtm("ATM-FUSION", "Fusion Probe", AppConstants.ATM_TYPE_NCR, "127.0.0.1");
        atm.LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-1);
        atm.LastDataReceivedUtc = DateTime.UtcNow.AddMinutes(-2);
        runtime.TrackJournalSync("ATM-FUSION", "EJDATA.LOG", 8192, JournalSyncState.Completed);

        var command = new RemoteCommand
        {
            ATM_ID = "ATM-FUSION",
            CommandType = AppConstants.CMD_RESTART,
            RequiresConfirmation = true
        };
        var fusion = runtime.BuildOperationalFusion(
            "NCR APTRA EJDATA APPROVED AMOUNT 700\nCARD CAPTURED\nM-18 CASH ERROR\nHOST MESSAGE IN",
            command,
            role: "Admin",
            operatorConfirmed: true,
            maintenanceWindow: true);

        var passed = fusion.JournalLinesAnalyzed == 4 &&
                     fusion.CommandAllowed &&
                     fusion.FleetTotal == 1 &&
                     fusion.FleetOnline == 1 &&
                     fusion.SyncCompleted == 1;

        return ("Unified operational fusion", passed,
            $"journalLines={fusion.JournalLinesAnalyzed}, confidence={fusion.JournalConfidence}, fleet={fusion.FleetOnline}/{fusion.FleetTotal}, completed={fusion.SyncCompleted}, commandAllowed={fusion.CommandAllowed}");
    }
    catch (Exception ex)
    {
        return ("Unified operational fusion", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunUnifiedServiceOperationsProbe()
{
    var root = Path.Combine(Path.GetTempPath(), "ejlive-service-ops-probe", Guid.NewGuid().ToString("N"));
    try
    {
        var services = CreateCoreServiceGraph();
        var storage = Path.Combine(root, "storage");
        var archive = Path.Combine(root, "archive");
        var reports = Path.Combine(root, "reports");
        var data = System.Text.Encoding.UTF8.GetBytes("NCR EJDATA APPROVED AMOUNT 900\nCARD CAPTURED\nM-18 CASH ERROR");

        var stored = services.JournalStorage.StoreJournalData(storage, "ATM-SVC", AppConstants.ATM_TYPE_NCR, "EJDATA.LOG", data);
        var csv = services.JournalStorage.ExportCsvReport(new[] { stored }, Path.Combine(reports, "journal.csv"));
        var html = services.JournalStorage.ExportHtmlReport(new[] { stored }, Path.Combine(reports, "journal.html"));
        var zip = services.JournalStorage.ArchiveMonth(storage, archive, "ATM-SVC", DateTime.UtcNow.ToString("yyyy-MM"));
        var dispatch = services.RemoteCommands.Queue("ATM-SVC", AppConstants.CMD_RESTART, role: "Admin", operatorConfirmed: true, maintenanceWindow: true);
        services.ClientServices.Start("Journal Sync", "Probe activation");
        services.ClientServices.Start("Network Monitor", "Probe activation");
        var serviceReport = services.ClientServices.BuildReport();

        var passed = File.Exists(stored.StoragePath) &&
                     File.Exists(csv) &&
                     File.Exists(html) &&
                     File.Exists(zip) &&
                     stored.Evidence.ApprovedTransactions == 1 &&
                     stored.Evidence.CashErrorEvents == 1 &&
                     dispatch.Policy.Allowed &&
                     serviceReport.Total >= 10 &&
                     serviceReport.Running >= 2;

        return ("Unified service operations", passed,
            $"stored={File.Exists(stored.StoragePath)}, csv={File.Exists(csv)}, html={File.Exists(html)}, zip={File.Exists(zip)}, running={serviceReport.Running}/{serviceReport.Total}, commandAllowed={dispatch.Policy.Allowed}");
    }
    catch (Exception ex)
    {
        return ("Unified service operations", false, ex.Message);
    }
    finally
    {
        try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
    }
}

static (string Name, bool Passed, string Detail) RunOperationalReportingProbe()
{
    var folder = Path.Combine(Path.GetTempPath(), $"ejlive-reporting-probe-{Guid.NewGuid():N}");
    Directory.CreateDirectory(folder);
    try
    {
        var now = DateTime.UtcNow;
        var snapshot = new UnifiedServerAnalyticsSnapshot(
            Fleet: new FleetSummary { Total = 1, Connected = 1, Offline = 0, AverageHealth = 95 },
            Sync: new SyncSummary { Total = 1, Completed = 1, AverageProgress = 100 },
            ConfirmedDeliveries: 1,
            PendingDeliveries: 0,
            FailedDeliveries: 0,
            CommandDispatches: 2,
            CommandResults: 2,
            CommandFailures: 0,
            LastCommandAtUtc: now.AddMinutes(-2),
            TelemetryEvents: 2,
            TelemetryWarnings: 1,
            TelemetryErrors: 0,
            NetworkDisconnectEvents: 0,
            HandshakeMissingEvents: 0,
            FileRetryEvents: 1,
            LastTelemetryAtUtc: now.AddMinutes(-1),
            AtmRows: new[]
            {
                new UnifiedAtmOperationalAnalyticsRow(
                    "ATM-RPT",
                    AppConstants.ATM_TYPE_NCR,
                    95,
                    ConnectionStatus.Connected,
                    0,
                    0,
                    1,
                    0,
                    0,
                    1,
                    0,
                    now.AddMinutes(-1),
                    now.AddSeconds(-30),
                    1)
            });

        var reporting = new UnifiedOperationalReportingService();
        var window = reporting.ExportWindowReport(folder, "day", 24, snapshot, DateTime.Now, now);
        var bundle = reporting.ExportBundleReport(
            folder,
            new[]
            {
                new OperationalWindowSnapshot("shift", 8, snapshot),
                new OperationalWindowSnapshot("day", 24, snapshot),
                new OperationalWindowSnapshot("week", 168, snapshot)
            },
            DateTime.Now,
            now);
        var fleet = reporting.ExportFleetHealthReport(folder, snapshot, DateTime.Now);

        var passed =
            File.Exists(window.JsonPath) &&
            File.Exists(window.CsvPath) &&
            File.Exists(bundle.JsonPath) &&
            File.Exists(bundle.SummaryCsvPath) &&
            File.Exists(bundle.AtmCsvPath) &&
            File.Exists(fleet);
        return ("Operational reporting", passed, $"window={Path.GetFileName(window.JsonPath)}, bundle={Path.GetFileName(bundle.JsonPath)}, fleet={Path.GetFileName(fleet)}");
    }
    catch (Exception ex)
    {
        return ("Operational reporting", false, ex.Message);
    }
    finally
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }
}

static (string Name, bool Passed, string Detail) RunOperationalReportCatalogProbe()
{
    var folder = Path.Combine(Path.GetTempPath(), $"ejlive-report-catalog-probe-{Guid.NewGuid():N}");
    Directory.CreateDirectory(folder);
    try
    {
        var now = DateTime.UtcNow;
        var snapshot = new UnifiedServerAnalyticsSnapshot(
            Fleet: new FleetSummary { Total = 1, Connected = 1, Offline = 0, AverageHealth = 92 },
            Sync: new SyncSummary { Total = 1, Pending = 0, InProgress = 0, Completed = 1, Failed = 0, AverageProgress = 100 },
            ConfirmedDeliveries: 1,
            PendingDeliveries: 0,
            FailedDeliveries: 0,
            CommandDispatches: 1,
            CommandResults: 1,
            CommandFailures: 0,
            LastCommandAtUtc: now.AddMinutes(-1),
            TelemetryEvents: 1,
            TelemetryWarnings: 1,
            TelemetryErrors: 0,
            NetworkDisconnectEvents: 0,
            HandshakeMissingEvents: 0,
            FileRetryEvents: 0,
            LastTelemetryAtUtc: now.AddMinutes(-1),
            AtmRows: new[]
            {
                new UnifiedAtmOperationalAnalyticsRow(
                    "ATM-CAT-PROBE",
                    AppConstants.ATM_TYPE_NCR,
                    92,
                    ConnectionStatus.Connected,
                    0,
                    0,
                    1,
                    0,
                    0,
                    1,
                    0,
                    now.AddMinutes(-1),
                    now.AddSeconds(-30),
                    0)
            });

        var reporting = new UnifiedOperationalReportingService();
        reporting.ExportBundleReport(
            folder,
            new[]
            {
                new OperationalWindowSnapshot("shift", 8, snapshot),
                new OperationalWindowSnapshot("day", 24, snapshot),
                new OperationalWindowSnapshot("week", 168, snapshot)
            },
            DateTime.Now,
            now);

        var catalog = new OperationalReportCatalogService();
        var files = catalog.GetLatestReportFiles(folder, 20);
        var summary = catalog.LoadLatestBundleSummary(folder);
        var passed = files.Count >= 3 &&
                     summary.Rows.Count == 3 &&
                     summary.Rows.Any(row => row.Window.Equals("day", StringComparison.OrdinalIgnoreCase) && row.LookbackHours == 24);
        return ("Operational report catalog", passed, $"files={files.Count}, bundleRows={summary.Rows.Count}");
    }
    catch (Exception ex)
    {
        return ("Operational report catalog", false, ex.Message);
    }
    finally
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }
}

static (string Name, bool Passed, string Detail) RunClientTelemetryAnalyticsProbe()
{
    var folder = Path.Combine(Path.GetTempPath(), $"ejlive-telemetry-probe-{Guid.NewGuid():N}");
    Directory.CreateDirectory(folder);
    try
    {
        var now = DateTime.UtcNow;
        var audit = new[]
        {
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-P1", Details = "info|network_connected|ok", CreatedAtUtc = now.AddMinutes(-3) },
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-P1", Details = "warning|file_retry|retry=1", CreatedAtUtc = now.AddMinutes(-2) },
            new AuditLogEntry { Action = "ClientTelemetry", Target = "ATM-P2", Details = "error|network_disconnected|socket lost", CreatedAtUtc = now.AddMinutes(-1) }
        };

        var service = new ClientTelemetryAnalyticsService();
        var snapshot = service.BuildSnapshot(audit);
        var timeline = service.ExportTimelineCsv(folder, snapshot, DateTime.Now);
        var atmSummary = service.ExportAtmSummaryCsv(folder, snapshot, DateTime.Now);
        var passed = snapshot.TotalEvents == 3 &&
                     snapshot.WarningEvents == 1 &&
                     snapshot.ErrorEvents == 1 &&
                     snapshot.DistinctAtms == 2 &&
                     File.Exists(timeline) &&
                     File.Exists(atmSummary);
        return ("Client telemetry analytics", passed, $"events={snapshot.TotalEvents}, warn={snapshot.WarningEvents}, err={snapshot.ErrorEvents}, atms={snapshot.DistinctAtms}");
    }
    catch (Exception ex)
    {
        return ("Client telemetry analytics", false, ex.Message);
    }
    finally
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunServerOperationalServicesProbe()
{
    var root = Path.Combine(Path.GetTempPath(), "ejlive-server-services-probe", Guid.NewGuid().ToString("N"));
    var storage = Path.Combine(root, "storage");
    var archive = Path.Combine(root, "archive");
    var reports = Path.Combine(root, "reports");
    var port = GetAvailableTcpPort();

    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);

    try
    {
        server.ClientConnected += (_, _) => connected.Set();
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-SERVER", AppConstants.ATM_TYPE_NCR);
        var clientConnected = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));

        using var analytics = new JournalAnalyticsService(storage, archive);
        using var remoteControl = new RemoteControlService(server);
        Directory.CreateDirectory(reports);

        var data = System.Text.Encoding.UTF8.GetBytes("NCR EJDATA APPROVED AMOUNT 150\nCARD CAPTURED\nM-18 CASH ERROR");
        analytics.StoreJournalData("ATM-SERVER", "EJDATA.LOG", data, SecurityHelper.SHA256Hash(data));
        var month = DateTime.Now.ToString("yyyy-MM");
        var zip = analytics.ArchiveMonth("ATM-SERVER", month);
        var csv = analytics.ExportCSVReport(Path.Combine(reports, "server.csv"), "ATM-SERVER");
        var html = analytics.ExportHTMLReport(Path.Combine(reports, "server.html"), "ATM-SERVER");

        var cmdId = remoteControl.SendRestart("ATM-SERVER", 5);
        await Task.Delay(250).ConfigureAwait(false);
        var last = remoteControl.GetCommandHistory("ATM-SERVER", 1).FirstOrDefault();

        client.Disconnect();
        server.Stop();

        var passed = clientConnected &&
                     accepted &&
                     File.Exists(zip) &&
                     File.Exists(csv) &&
                     File.Exists(html) &&
                     !string.IsNullOrWhiteSpace(cmdId) &&
                     last is not null &&
                     last.Sent;

        return ("Server operational services", passed,
            $"clientConnected={clientConnected}, accepted={accepted}, zip={File.Exists(zip)}, csv={File.Exists(csv)}, html={File.Exists(html)}, cmdSent={last?.Sent}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Server operational services", false, ex.Message);
    }
    finally
    {
        try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
    }
}

static (string Name, bool Passed, string Detail) RunUnsafeTermScanProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var srcRoot = Path.Combine(root, "src");
        var unsafeTerms = new[] { "Stealth", "HiddenProcess", "DisableDefender", "DisableFirewall", "BypassGpo", "KillProcess", "ArbitraryShell", "ExecScript", "NoConsentPrompt" };
        var allowedExceptions = new[]
        {
            "RemoteAssistanceStream",
            "ControlledDiagnosticCommandService",
            "AllowNoConsentPrompt",
            "requestNoConsentPrompt",
            "Arbitrary shell commands are not permitted"
        }; // Explicitly allowed policy-gated names under review.
        var violations = new List<string>();

        foreach (var csFile in Directory.EnumerateFiles(srcRoot, "*.cs", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(root, csFile).Replace('\\', '/');
            if (IsBuildOutput(rel))
                continue;
            if (rel.StartsWith("src/EJLive.Tests/", StringComparison.OrdinalIgnoreCase))
                continue;
            // Skip reference-only files (None Include)
            var externalSourceMarker = "/reference" + "-source/";
            if (rel.Contains(externalSourceMarker, StringComparison.OrdinalIgnoreCase))
                continue;
            var content = File.ReadAllText(csFile);
            foreach (var term in unsafeTerms)
            {
                if (content.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                    !allowedExceptions.Any(ex => content.Contains(ex, StringComparison.OrdinalIgnoreCase)))
                {
                    violations.Add($"{rel}: contains '{term}'");
                }
            }
        }

        var passed = violations.Count == 0;
        return ("Unsafe term scan", passed, $"violations={violations.Count}, samples={string.Join("; ", violations.Take(3))}");
    }
    catch (Exception ex)
    {
        return ("Unsafe term scan", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunUiInServicePathProbe()
{
    try
    {
        var root = FindSolutionRoot();
        var servicePaths = new[]
        {
            Path.Combine(root, "src", "EJLive.Client.Service")
        };
        var uiIndicators = new[] { "System.Windows.Forms", "MessageBox", "Form", "Control", "Button", "TextBox", "DataGridView", "DialogResult" };
        var violations = new List<string>();

        foreach (var path in servicePaths.Where(Directory.Exists))
        {
            foreach (var csFile in Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(root, csFile).Replace('\\', '/');
                if (IsBuildOutput(rel))
                    continue;
                var content = File.ReadAllText(csFile);
                foreach (var indicator in uiIndicators)
                {
                    var pattern = indicator == "System.Windows.Forms"
                        ? System.Text.RegularExpressions.Regex.Escape(indicator)
                        : $@"(?<![A-Za-z0-9_]){System.Text.RegularExpressions.Regex.Escape(indicator)}(?![A-Za-z0-9_])";

                    if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        // Allow comments that document UI boundary
                        var lines = content.Split('\n')
                            .Where(l => System.Text.RegularExpressions.Regex.IsMatch(l, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            .ToArray();
                        var codeLines = lines.Where(l => !l.TrimStart().StartsWith("//") && !l.TrimStart().StartsWith("*")).ToArray();
                        if (codeLines.Length > 0)
                        {
                            violations.Add($"{rel}: references '{indicator}'");
                            break;
                        }
                    }
                }
            }
        }

        var passed = violations.Count == 0;
        return ("UI-in-service-path scan", passed, $"violations={violations.Count}, samples={string.Join("; ", violations.Take(3))}");
    }
    catch (Exception ex)
    {
        return ("UI-in-service-path scan", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunDuplicateTypeProbe()
{
    try
    {
        var assemblies = new[]
        {
            typeof(EJLive.Application.EJLiveApplicationHost).Assembly,
            typeof(EJLive.Business.UnifiedBusinessRuntime).Assembly,
            typeof(EJLive.Core.Constants).Assembly,
            typeof(EJLive.Shared.SecurityHelper).Assembly,
            typeof(EJLive.Client.Service.ClientAgentWindowsService).Assembly,
            typeof(EJLive.Client.WinForms.ClientMainForm).Assembly,
            typeof(EJLive.Server.WinForms.ServerMainForm).Assembly,
            typeof(EJLive.Installer.WinForms.InstallerForm).Assembly,
            typeof(EJLive.Monitoring.WinForms.MainDashboardForm).Assembly,
        };

        var typeNames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name.StartsWith("<", StringComparison.Ordinal) ||
                    type.FullName?.Contains("<PrivateImplementationDetails>", StringComparison.OrdinalIgnoreCase) == true ||
                    type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), inherit: false))
                {
                    continue;
                }

                var typeKey = type.FullName ?? type.Name;
                if (!typeNames.TryGetValue(typeKey, out var list))
                {
                    list = new List<string>();
                    typeNames[typeKey] = list;
                }
                list.Add(assembly.GetName().Name ?? "?");
            }
        }

        var duplicates = typeNames
            .Where(kvp => kvp.Value.Count > 1 && kvp.Value.Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            .Select(kvp => $"{kvp.Key}: [{string.Join(", ", kvp.Value.Distinct(StringComparer.OrdinalIgnoreCase))}]")
            .ToArray();

        var passed = duplicates.Length == 0;
        return ("Duplicate type detection", passed, $"duplicates={duplicates.Length}, samples={string.Join("; ", duplicates.Take(3))}");
    }
    catch (Exception ex)
    {
        return ("Duplicate type detection", false, ex.Message);
    }
}

static (
    CoreUnifiedJournalStorageService JournalStorage,
    CoreUnifiedRemoteCommandOrchestrator RemoteCommands,
    CoreUnifiedClientServiceSupervisor ClientServices) CreateCoreServiceGraph()
{
    var journalStorage = new CoreUnifiedJournalStorageService();
    var remoteCommands = new CoreUnifiedRemoteCommandOrchestrator();
    var clientServices = new CoreUnifiedClientServiceSupervisor();

    return (journalStorage, remoteCommands, clientServices);
}

static int GetAvailableTcpPort()
{
    var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
    listener.Start();
    try
    {
        return ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    }
    finally
    {
        listener.Stop();
    }
}

static string FindSolutionRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory != null)
    {
        var verificationProject = Path.Combine(
            directory.FullName,
            "src",
            "EJLive.Verification",
            "EJLive.Verification.csproj");
        var hasSolution =
            Directory.EnumerateFiles(directory.FullName, "*.sln", SearchOption.TopDirectoryOnly).Any() ||
            Directory.EnumerateFiles(directory.FullName, "*.slnx", SearchOption.TopDirectoryOnly).Any();

        if (hasSolution && File.Exists(verificationProject))
            return directory.FullName;
        directory = directory.Parent;
    }
    throw new DirectoryNotFoundException("Could not locate the EJLive solution root from the verification output folder.");
}

static string FindSolutionFile(string root)
{
    var preferredNames = new[]
    {
        "EJLive.Platform.slnx",
        "EJLive.Platform.sln",
        "EJLive.Unified.slnx",
        "EJLive.Unified.sln"
    };

    foreach (var name in preferredNames)
    {
        var candidate = Path.Combine(root, name);
        if (File.Exists(candidate))
            return candidate;
    }

    var discovered = Directory.EnumerateFiles(root, "*.slnx", SearchOption.TopDirectoryOnly)
        .Concat(Directory.EnumerateFiles(root, "*.sln", SearchOption.TopDirectoryOnly))
        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
        .FirstOrDefault();

    return discovered ?? throw new FileNotFoundException("No .sln or .slnx file exists at the EJLive solution root.");
}

static IReadOnlyList<string> ReadSolutionProjects(string root)
{
    var solutionPath = FindSolutionFile(root);
    var text = File.ReadAllText(solutionPath);
    return System.Text.RegularExpressions.Regex
        .Matches(
            text,
            "[\"'](?<path>[^\"']+\\.csproj)[\"']",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase |
            System.Text.RegularExpressions.RegexOptions.CultureInvariant)
        .Select(match => match.Groups["path"].Value)
        .Where(path => !string.IsNullOrWhiteSpace(path))
        .Select(path => Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.GetFullPath(Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar))))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

static IReadOnlyList<string> ReadProjectReferences(string projectPath)
{
    var projectDirectory = Path.GetDirectoryName(projectPath)
        ?? throw new InvalidOperationException($"Project directory is unavailable: {projectPath}");
    var document = System.Xml.Linq.XDocument.Load(projectPath);
    return document
        .Descendants()
        .Where(element => element.Name.LocalName == "ProjectReference")
        .Select(element => element.Attribute("Include")?.Value)
        .Where(include => !string.IsNullOrWhiteSpace(include))
        .Select(include => Path.IsPathRooted(include!)
            ? Path.GetFullPath(include!)
            : Path.GetFullPath(Path.Combine(projectDirectory, include!.Replace('/', Path.DirectorySeparatorChar))))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

static bool IsWithinRoot(string root, string path) => IsWithinDirectory(root, path);

static bool IsWithinDirectory(string directory, string path)
{
    var relative = Path.GetRelativePath(Path.GetFullPath(directory), Path.GetFullPath(path));
    return !Path.IsPathRooted(relative) &&
           relative != ".." &&
           !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
           !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal);
}

static string ToRelativePath(string root, string path)
{
    return IsWithinRoot(root, path)
        ? Path.GetRelativePath(root, path).Replace('\\', '/')
        : Path.GetFullPath(path);
}

static bool IsBuildOutput(string path)
{
    var normalized = path.Replace('\\', '/');
    return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("/.vs/", StringComparison.OrdinalIgnoreCase) ||
           normalized.StartsWith(".git/", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("/artifacts/", StringComparison.OrdinalIgnoreCase);
}

return checks.All(c => c.Passed) ? 0 : 1;

static (string Name, bool Passed, string Detail) RunDatabaseMigrationProbe()
{
    var db = Path.Combine(Path.GetTempPath(), "ejlive-verification-migration-schema.db");
    foreach (var path in new[] { db, db + "-wal", db + "-shm" })
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    try
    {
        using (var connection = new SQLiteConnection($"Data Source={db};Version=3;"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE audit_log (
                    entry_id TEXT PRIMARY KEY,
                    user_name TEXT NOT NULL,
                    action TEXT NOT NULL,
                    target TEXT NOT NULL,
                    details TEXT NOT NULL
                );
                CREATE TABLE sync_records (
                    sync_id TEXT PRIMARY KEY,
                    atm_id TEXT NOT NULL,
                    file_name TEXT NOT NULL,
                    state TEXT NOT NULL,
                    progress INTEGER NOT NULL
                );
                """;
            command.ExecuteNonQuery();
        }

        DatabaseManager.Instance.Initialize(db);

        using var upgraded = new SQLiteConnection($"Data Source={db};Version=3;");
        upgraded.Open();
        using var verify = upgraded.CreateCommand();
        verify.CommandText = "SELECT name FROM pragma_table_info('audit_log') WHERE name='created_at_utc';";
        var auditColumn = Convert.ToString(verify.ExecuteScalar());
        verify.CommandText = "SELECT name FROM sqlite_master WHERE type='index' AND name='ix_audit_log_created_at';";
        var auditIndex = Convert.ToString(verify.ExecuteScalar());
        verify.CommandText = "SELECT name FROM pragma_table_info('sync_records') WHERE name='updated_at_utc';";
        var syncColumn = Convert.ToString(verify.ExecuteScalar());

        var passed = auditColumn == "created_at_utc" &&
                     auditIndex == "ix_audit_log_created_at" &&
                     syncColumn == "updated_at_utc";
        return ("SQLite migration", passed, $"auditColumn={auditColumn}, auditIndex={auditIndex}, syncColumn={syncColumn}");
    }
    catch (Exception ex)
    {
        return ("SQLite migration", false, ex.Message);
    }
}

static (string Name, bool Passed, string Detail) RunApplicationLayerProbe()
{
    var db = Path.Combine(Path.GetTempPath(), $"ejlive-application-probe-{Guid.NewGuid():N}.db");
    try
    {
        using var host = EJLiveApplicationHost.Create(db);
        var atm = host.SeedDemoAtm("ATM-APP");
        host.Runtime.TrackJournalSync(atm.ATM_ID ?? "ATM-APP", "EJDATA.LOG", 4096, JournalSyncState.Completed);
        var readiness = host.ValidateReadiness();
        var snapshot = host.Runtime.BuildSnapshot();
        var flow = host.DescribeDataFlow();

        var passed = readiness.Passed &&
                     snapshot.Fleet.Total == 1 &&
                     snapshot.Sync.Completed == 1 &&
                     flow.Count == 6 &&
                     snapshot.Capabilities.Any(capability => capability.Name == "Remote Command Governance") &&
                     snapshot.Capabilities.Any(capability => capability.Name == "Journal Storage");
        return ("Application/business layering", passed, $"ready={readiness.Passed}, fleet={snapshot.Fleet.Total}, completed={snapshot.Sync.Completed}, flow={flow.Count}, capabilities={snapshot.Capabilities.Count}");
    }
    catch (Exception ex)
    {
        return ("Application/business layering", false, ex.Message);
    }
}

static async Task<(string Name, bool Passed, string Detail)> RunNetworkProbeAsync()
{
    var port = GetAvailableTcpPort();
    using var server = new ServerEngine();
    using var connected = new ManualResetEventSlim(false);
    var connectedAtm = string.Empty;
    var errors = new List<string>();

    try
    {
        server.ClientConnected += (_, connection) =>
        {
            connectedAtm = connection.ATM_ID;
            connected.Set();
        };
        server.Error += (_, message) => errors.Add(message);
        server.Start(port);

        using var client = new NetworkEngine("127.0.0.1", port, "ATM-SMOKE", AppConstants.ATM_TYPE_NCR);
        var connectReturned = await Task.Run(client.Connect).ConfigureAwait(false);
        var accepted = connected.Wait(TimeSpan.FromSeconds(5));
        client.Disconnect();
        server.Stop();

        var passed = connectReturned && accepted && connectedAtm == "ATM-SMOKE" && errors.Count == 0;
        return ("Client/server network", passed, $"connectReturned={connectReturned}, accepted={accepted}, atm={connectedAtm}, errors={errors.Count}");
    }
    catch (Exception ex)
    {
        try { server.Stop(); } catch { }
        return ("Client/server network", false, ex.Message);
    }
}
