using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Data.Repositories;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Core.UI;
using EJLive.Server.Services;
using EJLive.Shared;
using System.Collections.Concurrent;
using System.Text.Json;
using AppConstants = EJLive.Core.AppConstants;
using NetworkConfig = EJLive.Core.NetworkConfig;

namespace EJLive.Server.WinForms;

    // Wave 5 / C-30 — Designer partial split (SS-10 process, proven on
    // JournalStudioForm in C-24): this file carries behaviour only; the control
    // tree (menu, 13 tabs, grids, cards, remote preview) lives in
    // ServerMainForm.Designer.cs, and every event binding lives in
    // WireEvents(), so a designer regeneration of the sibling partial can
    // never orphan a handler.
    //
    // Control → function map (designer fields, driven by this partial):
    //   _fleetGrid + _totalAtmsValue/_connectedAtmsValue/_syncingAtmsValue/
    //       _offlineAtmsValue/_fleetHealthValue — fleet overview + summary cards
    //   _networkMap — card wall (one ATMCardPanel per connection, runtime content)
    //   _log — runtime log (AppendLog); _journal* actions open the Studio
    //   _syncGrid + sync cards — journal sync state (Retry/Verify actions)
    //   _deliveryGrid — file delivery tracker
    //   _commandGrid / _commandTarget / _remotePreview — remote command console
    //   _alertGrid — alert list + export
    //   _opsAnalyticsGrid / _telemetryTimelineGrid / _telemetryAtmGrid — analytics
    //   _commandAuditGrid + filters — SS9 command audit trail
    //   _listenPort — read by StartServer (Settings tab)
    //   _mainMenu — File/Operations/Admin/Logs
public sealed partial class ServerMainForm : Form
{
    private readonly ServerEngine _serverEngine = new();
    private readonly OperationalStateStore _stateStore = new();
    private readonly AlertManager _alerts = new();
    private readonly JournalSyncTrackingService _syncTracking = new();
    private readonly JournalTransferIntelligenceService _journalTransferIntelligence = new();
    private readonly UnifiedJournalRoutingService _journalRouting = new();
    private readonly UnifiedServerAnalyticsService _opsAnalytics = new();
    private readonly UnifiedOperationalReportingService _operationalReporting = new();
    private readonly ClientTelemetryAnalyticsService _telemetryAnalytics = new();
    private readonly ClientTelemetryStateService _telemetryState;
    private readonly ServerAuditService _serverAudit;
    private readonly JournalSyncAlertService _syncAlertService;
    private readonly JournalAnalyticsService _journalAnalytics;
    private readonly RemoteControlService _remoteControl;
    private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 5000 };
    private readonly string _smartStorageRoot = Path.Combine(AppConstants.DefaultServerSharePath, "SmartStorage");
    private readonly ConcurrentDictionary<string, int> _remoteCommandRows = new(StringComparer.OrdinalIgnoreCase);
    private UnifiedServerAnalyticsSnapshot? _lastOpsAnalyticsSnapshot;
    private ClientTelemetryAnalyticsSnapshot? _lastTelemetrySnapshot;
    private DateTime _lastTelemetryUiRefreshUtc = DateTime.MinValue;

    public ServerMainForm()
    {
        DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath);

        // SS-20: the server host owns the SQLite repositories so the telemetry lane and the
        // SS9 command ledger are written by the services that consume them, not by the UI.
        var healthSnapshots = new ClientHealthSnapshotRepository(DatabaseManager.Instance);
        var atmRegistry = new ATMRegistryRepository(DatabaseManager.Instance);
        var commandAudit = new CommandAuditRepository(DatabaseManager.Instance);

        _serverAudit = new ServerAuditService(commandAudit: commandAudit);
        _telemetryState = new ClientTelemetryStateService(_stateStore, _alerts, healthSnapshots, atmRegistry);
        _journalAnalytics = new JournalAnalyticsService(_smartStorageRoot, AppConstants.DefaultArchivePath);
        _remoteControl = new RemoteControlService(_serverEngine);
        _syncAlertService = new JournalSyncAlertService(_alerts);
        _journalAnalytics.OnLog += message => AppendLog("[JournalAnalytics] " + message);
        _remoteControl.OnLog += message => AppendLog("[RemoteControl] " + message);
        _remoteControl.OnCommandResult += (atmId, commandId, success, result) =>
            RunOnUi(() => HandleTrackedCommandResult(atmId, commandId, success, result));
        EnsureImageShareFolders();
        WireServerEngine();
        SeedRuntimeState();
        InitializeComponent();
        WireEvents();
        PrepareGrids();
        PerformInitialRefresh();
        Shown += (_, _) => StartServer();
        _refreshTimer.Tick += (_, _) =>
        {
            UpdateSummaryCards();
            RefreshNetworkMap();
            EvaluateStalledJournalTransfers();
        };
        _refreshTimer.Start();
    }

    /// <summary>
    /// Every event binding for this surface, in one place (SS-10). The designer
    /// partial deliberately contains no bindings; a regenerated
    /// <c>ServerMainForm.Designer.cs</c> therefore cannot drop a handler.
    /// </summary>
    private void WireEvents()
    {
        _startServerMenuItem.Click += StartServer;
        _stopServerMenuItem.Click += StopServer;
        _exitMenuItem.Click += Close;

        _refreshFleetMenuItem.Click += RefreshFleet;
        _refreshOpsAnalyticsMenuItem.Click += () => RefreshOpsAnalytics(24);
        _refreshTelemetryMenuItem.Click += () => RefreshTelemetry(24);
        _dailyReportMenuItem.Click += () => ExportOperationalWindowReport("day", 24);

        _pingMenuItem.Click += () => SendRemoteCommand(AppConstants.CMD_PING);
        _forceSyncMenuItem.Click += () => SendRemoteCommand(AppConstants.CMD_FORCE_SYNC);
        _probeMenuItem.Click += SendConnectivityProbe;

        _exportLogMenuItem.Click += ExportRuntimeLogSnapshot;
        _clearLogMenuItem.Click += () => { if (_log is not null) _log.Clear(); };
        _openReportsMenuItem.Click += () => OpenFolder(AppConstants.DefaultReportsPath);
        _openArchiveMenuItem.Click += () => OpenFolder(AppConstants.DefaultArchivePath);

        _fleetRefreshButton.Click += RefreshFleet;
        _fleetDetailsButton.Click += () => new ATMDetailForm(CurrentAtm()).Show(this);
        _fleetDrawerButton.Click += () => new ATMDetailDrawerForm(CurrentAtm()).Show(this);
        _fleetBroadcastButton.Click += () => { _serverEngine.Broadcast("Server broadcast from EJLive."); AppendLog("Broadcast message sent."); };
        _fleetStartButton.Click += StartServer;
        _fleetStopButton.Click += StopServer;

        _mapRefreshButton.Click += RefreshNetworkMap;
        _mapOpenButton.Click += () => new ATMDetailDrawerForm(CurrentAtm()).Show(this);
        _mapBroadcastButton.Click += () =>
        {
            _serverEngine.Broadcast("STATUS_CHECK");
            AppendLog("Status check broadcast sent.");
        };

        _journalStudioButton.Click += () => new JournalStudioForm().Show(this);
        _journalTodayButton.Click += () => AppendLog("Today's journals loaded.");
        _journalArchiveButton.Click += RunJournalArchive;
        _journalOpenArchiveButton.Click += () => OpenFolder(AppConstants.DefaultArchivePath);
        _journalOpenSmartButton.Click += () => OpenFolder(_smartStorageRoot);

        _syncOpenButton.Click += () => new SyncDashboardForm(_syncTracking.Records).Show(this);
        _syncRetryButton.Click += RetryFailedSync;
        _syncVerifyButton.Click += VerifySyncChecksums;

        _deliveryRefreshButton.Click += () => RefreshDeliveryTracker("all");
        _deliveryPendingButton.Click += () => RefreshDeliveryTracker("pending");
        _deliveryFailedButton.Click += () => RefreshDeliveryTracker("failed");
        _deliveryOpenSmartButton.Click += () => OpenFolder(_smartStorageRoot);

        _remoteRefreshTargetsButton.Click += RefreshCommandTargets;
        _remotePingButton.Click += () => SendRemoteCommand(AppConstants.CMD_PING);
        _remotePingTrackedButton.Click += SendPingTracked;
        _remoteProbeButton.Click += SendConnectivityProbe;
        _remoteSyncTimeButton.Click += () => SendRemoteCommand(AppConstants.CMD_SYNC_TIME);
        _remoteScreenshotButton.Click += () => SendRemoteCommand(AppConstants.CMD_SCREENSHOT);
        _remoteSessionStartButton.Click += () => SendRemoteCommand(AppConstants.CMD_REMOTE_SESSION_START);
        _remoteSessionStopButton.Click += () => SendRemoteCommand(AppConstants.CMD_REMOTE_SESSION_STOP);
        _rdpStartButton.Click += SendWindowsRemoteStart;
        _rdpCheckButton.Click += SendWindowsRemoteCheck;
        _rdpStopButton.Click += SendWindowsRemoteStop;
        _changePasswordButton.Click += SendChangePassword;
        _changeWinPasswordButton.Click += SendChangeWindowsPassword;
        _requestJournalButton.Click += SendJournalRequest;
        _imageInboxButton.Click += () => SendImageToTarget(ImageDistributionMode.InboxStaging);
        _imageDirectButton.Click += () => SendImageToTarget(ImageDistributionMode.DirectApply);
        _distInboxButton.Click += DistributeImagesFromServerFoldersInbox;
        _distDirectButton.Click += DistributeImagesFromServerFoldersDirect;
        _syncImagesButton.Click += SendSyncImages;
        _forceSyncButton.Click += () => SendRemoteCommand(AppConstants.CMD_FORCE_SYNC);
        _restartButton.Click += () => SendRemoteCommand(AppConstants.CMD_RESTART);
        _trackedRestartButton.Click += SendTrackedRestart;

        _alertsTestButton.Click += () => { _alerts.Raise(AlertSeverity.Warning, "Test Alert", "Manual test alert", "Server"); RefreshAlerts(); };
        _alertsMarkReadButton.Click += () => AppendLog("Alert marked read.");
        _alertsExportButton.Click += ExportAlertsCsv;
        _alertsOpenReportsButton.Click += () => OpenFolder(AppConstants.DefaultReportsPath);

        _archiveRunButton.Click += () => AppendLog("Archive cycle completed.");
        _archiveEligibleButton.Click += RunJournalArchive;
        _archiveOpenButton.Click += () => OpenFolder(AppConstants.DefaultArchivePath);
        _archiveCleanupButton.Click += CreateArchiveCleanupReport;

        _reportsShiftButton.Click += () => ExportOperationalWindowReport("shift", 8);
        _reportsDailyButton.Click += () => ExportOperationalWindowReport("day", 24);
        _reportsWeeklyButton.Click += () => ExportOperationalWindowReport("week", 168);
        _reportsFleetHealthButton.Click += ExportFleetHealthReport;
        _reportsCombinedButton.Click += ExportOperationalWindowsBundleReport;
        _reportsOpenFolderButton.Click += () => OpenFolder(AppConstants.DefaultReportsPath);

        _opsAnalyticsRefreshButton.Click += () => RefreshOpsAnalytics(24);
        _opsAnalytics1hButton.Click += () => RefreshOpsAnalytics(1);
        _opsAnalytics24hButton.Click += () => RefreshOpsAnalytics(24);
        _opsAnalyticsExportButton.Click += ExportOpsAnalyticsSnapshot;
        _opsAnalyticsOpenButton.Click += () => OpenFolder(AppConstants.DefaultReportsPath);

        _commandAuditRefreshButton.Click += () => RefreshCommandAudit(24);
        _commandAudit1hButton.Click += () => RefreshCommandAudit(1);
        _commandAudit24hButton.Click += () => RefreshCommandAudit(24);
        _commandAuditExportButton.Click += ExportCommandAuditCsv;
        _commandAuditOpenButton.Click += () => OpenFolder(AppConstants.DefaultReportsPath);

        _telemetryRefreshButton.Click += () => RefreshTelemetry(24);
        _telemetry1hButton.Click += () => RefreshTelemetry(1);
        _telemetry24hButton.Click += () => RefreshTelemetry(24);
        _telemetryTimelineCsvButton.Click += ExportTelemetryTimelineCsv;
        _telemetryAtmCsvButton.Click += ExportTelemetryAtmSummaryCsv;
        _telemetryOpenButton.Click += () => OpenFolder(AppConstants.DefaultReportsPath);

        _settingsSaveButton.Click += () => AppendLog("Server settings saved.");
        _settingsInitDbButton.Click += () => { DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath); AppendLog("Database initialized."); };

        _refreshTimer.Tick += (_, _) =>
        {
            UpdateSummaryCards();
            RefreshNetworkMap();
            EvaluateStalledJournalTransfers();
        };
    }

    /// <summary>
    /// Runtime performance pass over the designer-built grids:
    /// <see cref="ControlRenderingExtensions.EnableDoubleBuffering"/> is a reflection
    /// tweak the designer cannot express, so it is applied here (the visual theme it
    /// complements is set in the designer partial).
    /// </summary>
    private void PrepareGrids()
    {
        _fleetGrid.EnableDoubleBuffering();
        _syncGrid.EnableDoubleBuffering();
        _deliveryGrid.EnableDoubleBuffering();
        _alertGrid.EnableDoubleBuffering();
        _commandGrid.EnableDoubleBuffering();
        _commandAuditGrid.EnableDoubleBuffering();
        _opsAnalyticsGrid.EnableDoubleBuffering();
        _telemetryTimelineGrid.EnableDoubleBuffering();
        _telemetryAtmGrid.EnableDoubleBuffering();
    }

    /// <summary>
    /// First paint, in the exact order the legacy tab builders applied it (each
    /// <c>Build*Tab</c> finished with its own refresh call). Runs after
    /// <see cref="SeedRuntimeState"/> so the initial grids show the seeded fleet.
    /// </summary>
    private void PerformInitialRefresh()
    {
        RefreshFleet();
        RefreshNetworkMap();
        RefreshSync();
        RefreshDeliveryTracker("all");
        RefreshCommandTargets();
        RefreshOpsAnalytics(24);
        PopulateCommandAuditTargets();
        RefreshCommandAudit(24);
        RefreshTelemetry(24);
    }

    private void SeedRuntimeState()
    {
        for (var i = 1; i <= 5; i++)
        {
            var atm = new ATMInfo
            {
                ATM_ID = $"ATM{i:000}",
                ATM_Name = $"Branch Terminal {i}",
                ATM_Type = i % 2 == 0 ? AppConstants.ATM_TYPE_GRG : AppConstants.ATM_TYPE_NCR,
                ConnectionStatus = i == 4 ? ConnectionStatus.Disconnected : ConnectionStatus.Connected,
                Status = i == 4 ? ATMStatus.Offline : ATMStatus.Online,
                LastHeartbeatUtc = DateTime.UtcNow.AddMinutes(-i),
                LastDataReceivedUtc = DateTime.UtcNow.AddMinutes(-i * 3),
                HealthScore = 100 - i * 7
            };
            _stateStore.Upsert(atm);
            _syncTracking.AddOrUpdate(new JournalSyncRecord { ATM_ID = atm.ATM_ID ?? string.Empty, FileName = $"EJDATA-{i}.LOG", State = i == 4 ? JournalSyncState.Failed : JournalSyncState.Completed, ProgressPercent = i == 4 ? 30 : 100 });
        }
    }

    private void WireServerEngine()
    {
        _serverEngine.Log += (_, message) => RunOnUi(() =>
        {
            TryApplyCommandResultFromLog(message);
            AppendLog(message);
        });
        _serverEngine.Error += (_, message) => RunOnUi(() =>
        {
            _alerts.Raise(AlertSeverity.Warning, "Server runtime error", message, "ServerEngine");
            AppendLog(message);
            RefreshAlerts();
        });
        _serverEngine.ClientConnected += (_, connection) => RunOnUi(() =>
        {
            var atm = new ATMInfo
            {
                ATM_ID = connection.ATM_ID,
                ATM_Name = connection.ATM_ID,
                ATM_Type = string.IsNullOrWhiteSpace(connection.ATM_Type) ? AppConstants.ATM_TYPE_NCR : connection.ATM_Type,
                ConnectionStatus = ConnectionStatus.Connected,
                Status = ATMStatus.Online,
                IsConnected = true,
                ConnectedAtUtc = connection.ConnectedAtUtc,
                LastHeartbeatUtc = connection.LastHeartbeatUtc,
                LastDataReceivedUtc = DateTime.UtcNow,
                ServerIP = connection.RemoteEndPoint,
                HealthScore = 100
            };
            _stateStore.Upsert(atm);
            AppendLog($"ATM connected: {connection.ATM_ID} ({connection.RemoteEndPoint}).");
            RefreshFleet();
        });
        _serverEngine.ClientDisconnected += (_, connection) => RunOnUi(() =>
        {
            if (_stateStore.TryGet(connection.ATM_ID, out var atm) && atm is not null)
            {
                atm.ConnectionStatus = ConnectionStatus.Disconnected;
                atm.Status = ATMStatus.Offline;
                atm.IsConnected = false;
                atm.DisconnectedAtUtc = DateTime.UtcNow;
                _stateStore.Upsert(atm);
            }
            AppendLog($"ATM disconnected: {connection.ATM_ID}.");
            RefreshFleet();
        });
        _serverEngine.MessageReceived += (_, message) => RunOnUi(() =>
        {
            if (message.Type is CommunicationProtocol.MsgType.StartFile or CommunicationProtocol.MsgType.Complete)
                RefreshSync();
        });
        _serverEngine.ClientTelemetryReceived += (_, packet) => RunOnUi(() => HandleClientTelemetry(packet));
        _serverEngine.JournalTransferProgress += (_, packet) => RunOnUi(() => HandleJournalTransferProgress(packet));
        _serverEngine.JournalFileReceived += (_, packet) => RunOnUi(() => HandleJournalFileReceived(packet));
        _serverEngine.RemoteFrameReceived += (_, packet) => RunOnUi(() => UpdateRemotePreview(packet));
    }

    private void StartServer()
    {
        try
        {
            var port = _listenPort is null ? NetworkConfig.DEFAULT_PORT : (int)_listenPort.Value;
            _serverEngine.Start(port);
            AppendLog($"Server is listening on port {port}.");
        }
        catch (Exception ex)
        {
            _alerts.Raise(AlertSeverity.Critical, "Server start failed", ex.Message, "ServerEngine");
            AppendLog($"Server start failed: {ex.Message}");
            RefreshAlerts();
        }
    }

    private void StopServer()
    {
        _serverEngine.Stop();
        AppendLog("Server stopped.");
        RefreshFleet();
    }

    private void RefreshCommandTargets()
    {
        if (_commandTarget is null)
            return;

        var selected = Convert.ToString(_commandTarget.SelectedItem);
        _commandTarget.Items.Clear();
        _commandTarget.Items.Add("All Connected");
        foreach (var atm in _stateStore.Snapshot)
            _commandTarget.Items.Add(atm.ATM_ID ?? atm.ATMId ?? "UNKNOWN");

        _commandTarget.SelectedItem = !string.IsNullOrWhiteSpace(selected) && _commandTarget.Items.Contains(selected)
            ? selected
            : "All Connected";
    }

    private void SendRemoteCommand(string commandType)
    {
        var target = Convert.ToString(_commandTarget?.SelectedItem);
        if (string.IsNullOrWhiteSpace(target))
            target = CurrentAtm().ATM_ID ?? "All Connected";

        var command = new RemoteCommandEnvelope
        {
            CommandType = commandType,
            RequiresConfirmation = AppConstants.CommandsRequireConfirmation.Contains(commandType),
            Payload = $"Role=Admin;IssuedBy=Server;IssuedAt={DateTime.UtcNow:O}"
        };

        var sent = string.Equals(target, "All Connected", StringComparison.OrdinalIgnoreCase)
            ? _serverEngine.BroadcastCommand(command) > 0
            : _serverEngine.SendCommand(target, command);

        RecordCommand(target, command, sent ? "Sent" : "No active connection");
        AppendLog($"{commandType} to {target}: {(sent ? "sent" : "no active connection")}.");
    }

    private void SendTrackedRestart()
    {
        var target = Convert.ToString(_commandTarget?.SelectedItem);
        if (string.IsNullOrWhiteSpace(target) || string.Equals(target, "All Connected", StringComparison.OrdinalIgnoreCase))
        {
            var count = _remoteControl.BroadcastRestart(10);
            AppendLog(count > 0
                ? $"Tracked restart sent to {count} ATM(s)."
                : "Tracked restart skipped: no active connection.");
            return;
        }

        var commandId = _remoteControl.SendRestart(target, 10);
        var record = _remoteControl.GetCommandHistory(target, 1).FirstOrDefault();
        var status = record?.Sent == true ? "sent" : "no active connection";
        AppendLog($"Tracked restart {status} for {target}. CommandId={commandId}");
    }

    private void SendWindowsRemoteStart()
    {
        ExecuteTrackedCommand(
            target => _remoteControl.SendWindowsRemoteStart(target),
            "Windows remote start");
    }

    private void SendWindowsRemoteCheck()
    {
        ExecuteTrackedCommand(
            target => _remoteControl.SendWindowsRemoteCheck(target),
            "Windows remote readiness check");
    }

    private void SendWindowsRemoteStop()
    {
        ExecuteTrackedCommand(
            target => _remoteControl.SendWindowsRemoteStop(target),
            "Windows remote stop");
    }

    private void SendPingTracked()
    {
        ExecuteTrackedCommand(
            target => _remoteControl.SendPing(target),
            "Connection test (ping)");
    }

    private void SendConnectivityProbe()
    {
        var target = Convert.ToString(_commandTarget?.SelectedItem);
        var targets = ResolveTargets(target);
        if (targets.Count == 0)
        {
            AppendLog("Connectivity probe: no active target.");
            return;
        }

        var sent = 0;
        var index = 0;
        foreach (var atmId in targets)
        {
            index++;
            var pingId = $"probe-{DateTime.UtcNow:yyyyMMddHHmmss}-{index:D3}";
            var commandId = _remoteControl.SendPing(atmId, pingId);
            var record = _remoteControl.GetCommandHistory(atmId, 1).FirstOrDefault();
            var delivered = record?.Sent == true;
            if (delivered)
                sent++;

            RecordTrackedCommand(atmId, "ConnectivityProbe", commandId, delivered);
            WriteServerCommandAudit(
                action: "ConnectivityProbeDispatch",
                atmId: atmId,
                detail: $"{commandId}|PingId={pingId}|sent={delivered}");
        }

        AppendLog($"Connectivity probe dispatched to {sent}/{targets.Count} target(s).");
    }

    private void SendChangePassword()
    {
        var password = PromptTextDialog.ShowDialog(
            owner: this,
            title: "Change ATM Password",
            label: "New password",
            defaultValue: string.Empty,
            isPassword: true);
        if (string.IsNullOrWhiteSpace(password))
            return;

        ExecuteTrackedCommand(
            target => _remoteControl.SendChangePassword(target, password),
            "Change password");
    }

    private void SendChangeWindowsPassword()
    {
        var user = PromptTextDialog.ShowDialog(
            owner: this,
            title: "Change Windows Password",
            label: "Windows account",
            defaultValue: "Administrator");
        if (string.IsNullOrWhiteSpace(user))
            return;

        var password = PromptTextDialog.ShowDialog(
            owner: this,
            title: "Change Windows Password",
            label: $"New password for {user}",
            defaultValue: string.Empty,
            isPassword: true);
        if (string.IsNullOrWhiteSpace(password))
            return;

        ExecuteTrackedCommand(
            target => _remoteControl.SendChangeWindowsPassword(target, user, password),
            $"Change Windows password ({user})");
    }

    private void SendJournalRequest()
    {
        var requestedFile = PromptTextDialog.ShowDialog(
            owner: this,
            title: "Request Journal",
            label: "File path or name",
            defaultValue: "EJDATA.LOG");
        if (string.IsNullOrWhiteSpace(requestedFile))
            return;

        ExecuteTrackedCommand(
            target => _remoteControl.SendRequestJournalFile(target, requestedFile),
            "Request journal");
    }

    private void SendImageToTarget()
    {
        SendImageToTarget(ImageDistributionMode.InboxStaging);
    }

    private void SendImageToTarget(ImageDistributionMode mode)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp|All Files|*.*",
            Title = "Select image to send"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(dialog.FileName);
        }
        catch (Exception ex)
        {
            AppendLog("Image read failed: " + ex.Message);
            return;
        }

        var fileName = Path.GetFileName(dialog.FileName);
        if (mode == ImageDistributionMode.DirectApply)
        {
            ExecuteTrackedCommand(
                target => _remoteControl.SendImageDirectByAtmType(
                    target,
                    fileName,
                    bytes,
                    ResolveAtmType(target)),
                "Distribute image (direct apply)");
            return;
        }

        var targetPathHint = PromptTextDialog.ShowDialog(
            owner: this,
            title: "Image Target Path",
            label: "Target folder hint on client (optional)",
            defaultValue: "Inbox");
        ExecuteTrackedCommand(
            target => _remoteControl.SendImageToInbox(target, fileName, bytes, targetPathHint),
            "Distribute image (inbox staging)");
    }

    private void SendSyncImages()
    {
        var pathHint = PromptTextDialog.ShowDialog(
            owner: this,
            title: "Sync Images",
            label: "Images path hint (optional)",
            defaultValue: string.Empty);

        ExecuteTrackedCommand(
            target => _remoteControl.SendSyncImages(target, pathHint),
            "Sync images");
    }

    private void DistributeImagesFromServerFoldersInbox()
    {
        ExecuteServerFolderDistribution(ImageDistributionMode.InboxStaging);
    }

    private void DistributeImagesFromServerFoldersDirect()
    {
        ExecuteServerFolderDistribution(ImageDistributionMode.DirectApply);
    }

    private void ExecuteServerFolderDistribution(ImageDistributionMode mode)
    {
        var target = Convert.ToString(_commandTarget?.SelectedItem);
        var targets = ResolveTargets(target);
        if (targets.Count == 0)
        {
            AppendLog("Image distribution: no active target.");
            return;
        }

        var totalFiles = 0;
        var totalSent = 0;

        foreach (var atmId in targets)
        {
            var atmType = ResolveAtmType(atmId);
            var files = mode == ImageDistributionMode.DirectApply
                ? EnumerateDirectDistributionFiles(atmType)
                : EnumerateInboxDistributionFiles(atmType);

            foreach (var file in files)
            {
                byte[] bytes;
                try
                {
                    bytes = File.ReadAllBytes(file);
                }
                catch (Exception ex)
                {
                    AppendLog($"Image distribution read failed ({atmId}, {file}): {ex.Message}");
                    continue;
                }

                totalFiles++;
                var fileName = Path.GetFileName(file);
                var commandId = mode == ImageDistributionMode.DirectApply
                    ? _remoteControl.SendImageDirectByAtmType(atmId, fileName, bytes, atmType)
                    : _remoteControl.SendImageToInbox(atmId, fileName, bytes, Path.Combine("Inbox", "Staging", AppConstants.NormalizeATMType(atmType)));

                var record = _remoteControl.GetCommandHistory(atmId, 1).FirstOrDefault();
                var sent = record?.Sent == true;
                if (sent)
                    totalSent++;

                var commandLabel = mode == ImageDistributionMode.DirectApply
                    ? "FolderDirectImage"
                    : "FolderInboxImage";
                RecordTrackedCommand(atmId, commandLabel, commandId, sent);
            }
        }

        var modeLabel = mode == ImageDistributionMode.DirectApply ? "direct apply" : "inbox staging";
        AppendLog($"Server-folder image distribution ({modeLabel}) finished: sent {totalSent}/{totalFiles} file command(s).");
    }

    private void ExecuteTrackedCommand(Func<string, string> sendCommand, string operationLabel)
    {
        var target = Convert.ToString(_commandTarget?.SelectedItem);
        var targets = ResolveTargets(target);
        if (targets.Count == 0)
        {
            AppendLog($"{operationLabel}: no active target.");
            return;
        }

        var sent = 0;
        foreach (var atmId in targets)
        {
            var commandId = sendCommand(atmId);
            var record = _remoteControl.GetCommandHistory(atmId, 1).FirstOrDefault();
            if (record?.Sent == true)
                sent++;
            RecordTrackedCommand(atmId, operationLabel, commandId, record?.Sent == true);
        }

        AppendLog($"{operationLabel}: sent to {sent}/{targets.Count} target(s).");
    }

    private List<string> ResolveTargets(string? selectedTarget)
    {
        if (string.IsNullOrWhiteSpace(selectedTarget) ||
            string.Equals(selectedTarget, "All Connected", StringComparison.OrdinalIgnoreCase))
        {
            return _serverEngine.Connections
                .Select(connection => connection.ATM_ID)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return new List<string> { selectedTarget };
    }

    private void RecordTrackedCommand(string target, string commandType, string commandId, bool sent)
    {
        var status = sent ? "Sent" : "No active connection";
        if (_commandGrid is not null)
        {
            var index = _commandGrid.Rows.Add(
                DateTime.Now.ToString("HH:mm:ss"),
                target,
                commandType,
                status,
                commandId,
                sent ? "Pending result..." : "No active session.");
            _commandGrid.Rows[index].DefaultCellStyle.BackColor = sent
                ? Color.FromArgb(239, 252, 246)
                : Color.FromArgb(255, 239, 239);
            if (!string.IsNullOrWhiteSpace(commandId))
                _remoteCommandRows[commandId] = index;
        }

        WriteServerCommandAudit(
            action: sent ? "RemoteCommandDispatch" : "RemoteCommandDispatchFailed",
            atmId: target,
            detail: $"{commandType}|{commandId}|status={status}");
    }

    private void HandleTrackedCommandResult(string atmId, string commandId, bool success, string result)
    {
        var status = success ? "Completed" : "Failed";
        if (_commandGrid is not null &&
            !string.IsNullOrWhiteSpace(commandId) &&
            _remoteCommandRows.TryGetValue(commandId, out var rowIndex) &&
            rowIndex >= 0 &&
            rowIndex < _commandGrid.Rows.Count)
        {
            var row = _commandGrid.Rows[rowIndex];
            row.Cells["Status"].Value = status;
            row.Cells["Result"].Value = result;
            row.DefaultCellStyle.BackColor = success
                ? Color.FromArgb(226, 250, 236)
                : Color.FromArgb(255, 232, 232);
        }

        WriteServerCommandAudit(
            action: success ? "RemoteCommandResult" : "RemoteCommandResultFailed",
            atmId: atmId,
            detail: $"{commandId}|{result}");
        AppendLog($"Remote result [{atmId}] {commandId}: {status} - {result}");
    }

    private void RunJournalArchive()
    {
        var archived = _journalAnalytics.ArchiveAll(1);
        AppendLog(archived == 0
            ? "Journal archive found no eligible month folders."
            : $"Journal archive completed for {archived} month folder(s).");
    }

    private void RecordCommand(string target, RemoteCommandEnvelope command, string status)
    {
        if (_commandGrid is not null)
        {
            var index = _commandGrid.Rows.Add(
                DateTime.Now.ToString("HH:mm:ss"),
                target,
                command.CommandType,
                status,
                command.CommandId,
                status == "Sent" ? "Pending result..." : "No active session.");
            _commandGrid.Rows[index].DefaultCellStyle.BackColor = status == "Sent"
                ? Color.FromArgb(239, 252, 246)
                : Color.FromArgb(255, 239, 239);

            if (status == "Sent" && !string.IsNullOrWhiteSpace(command.CommandId))
                _remoteCommandRows[command.CommandId] = index;
        }

        WriteServerCommandAudit(
            action: status == "Sent" ? "CommandDispatch" : "CommandDispatchFailed",
            atmId: target,
            detail: $"{command.CommandType}|{command.CommandId}|status={status}");
    }

    private void TryApplyCommandResultFromLog(string message)
    {
        if (!TryParseCommandResultLog(message, out var atmId, out var commandId, out var success, out var detail))
            return;
        if (_commandGrid is null || string.IsNullOrWhiteSpace(commandId))
            return;

        var status = success ? "Completed" : "Failed";
        if (_remoteCommandRows.TryGetValue(commandId, out var rowIndex) &&
            rowIndex >= 0 &&
            rowIndex < _commandGrid.Rows.Count)
        {
            var row = _commandGrid.Rows[rowIndex];
            row.Cells["Status"].Value = status;
            row.Cells["Result"].Value = detail;
            row.DefaultCellStyle.BackColor = success
                ? Color.FromArgb(226, 250, 236)
                : Color.FromArgb(255, 232, 232);
            return;
        }

        var created = _commandGrid.Rows.Add(
            DateTime.Now.ToString("HH:mm:ss"),
            atmId,
            "CommandResult",
            status,
            commandId,
            detail);
        _commandGrid.Rows[created].DefaultCellStyle.BackColor = success
            ? Color.FromArgb(226, 250, 236)
            : Color.FromArgb(255, 232, 232);
        _remoteCommandRows[commandId] = created;
    }

    private static bool TryParseCommandResultLog(
        string message,
        out string atmId,
        out string commandId,
        out bool success,
        out string detail)
    {
        return RemoteControlService.TryParseCommandResultLog(
            message,
            out atmId,
            out commandId,
            out success,
            out detail);
    }

    private void WriteServerCommandAudit(string action, string atmId, string detail)
    {
        try
        {
            _serverAudit.WriteCommandAudit(action, atmId, detail);
        }
        catch (Exception ex)
        {
            AppendLog("Server audit write warning: " + ex.Message);
        }
    }

    private void UpdateRemotePreview(RemoteFramePacket packet)
    {
        if (_remotePreview is null || packet.Payload.Length == 0)
            return;

        try
        {
            using var stream = new MemoryStream(packet.Payload);
            using var decoded = Image.FromStream(stream, useEmbeddedColorManagement: false, validateImageData: false);
            var frame = new Bitmap(decoded);
            var previous = _remotePreview.Image;
            _remotePreview.Image = frame;
            previous?.Dispose();

            if (_remotePreviewStatus is not null)
            {
                _remotePreviewStatus.Text =
                    $"Remote preview: {packet.ATM_ID} at {packet.ReceivedAtUtc.ToLocalTime():HH:mm:ss} ({packet.Payload.Length / 1024.0:N1} KB)";
            }
        }
        catch (Exception ex)
        {
            if (_remotePreviewStatus is not null)
                _remotePreviewStatus.Text = $"Remote preview error: {ex.Message}";
            AppendLog($"Remote preview decode failed from {packet.ATM_ID}: {ex.Message}");
        }
    }

    private void HandleJournalTransferProgress(JournalTransferProgressPacket packet)
    {
        try
        {
            var atmType = ResolveAtmType(packet.ATM_ID);
            _journalRouting.RegisterPending(
                packet.TransferId,
                packet.ATM_ID,
                atmType,
                packet.FileName,
                packet.ExpectedBytes,
                packet.Checksum,
                routeHint: "journal");

            var record = _journalTransferIntelligence.Upsert(packet);
            _syncTracking.AddOrUpdate(record);

            if (record.State == JournalSyncState.Failed)
            {
                _journalRouting.RegisterFailed(packet.TransferId, string.IsNullOrWhiteSpace(packet.Message) ? "Transfer failed." : packet.Message);
                _syncAlertService.Evaluate(record);
                RefreshAlerts();
            }

            RefreshSync();
            RefreshDeliveryTracker("all");
        }
        catch (Exception ex)
        {
            AppendLog($"Journal transfer progress update failed for {packet.ATM_ID}/{packet.FileName}: {ex.Message}");
        }
    }

    private void HandleClientTelemetry(ClientTelemetryPacket packet)
    {
        var update = _telemetryState.Apply(packet);
        if (update.AlertRaised)
            RefreshAlerts();

        AppendLog($"[Telemetry] {update.AtmId} {update.Severity}/{update.EventType}: {packet.Detail}");

        var now = DateTime.UtcNow;
        if ((now - _lastTelemetryUiRefreshUtc) > TimeSpan.FromSeconds(2))
        {
            _lastTelemetryUiRefreshUtc = now;
            RefreshFleet();
            RefreshOpsAnalytics(24);
            RefreshTelemetry(24);
        }
        else
        {
            UpdateSummaryCards();
        }
    }

    private static void ApplyCashTelemetryFromPacket(ATMInfo atm, string eventType, string detail, DateTime reportedAtUtc)
    {
        ClientTelemetryStateService.ApplyCashTelemetry(atm, eventType, detail, reportedAtUtc);
    }

    private void HandleJournalFileReceived(JournalFileReceivedPacket packet)
    {
        try
        {
            var atmType = ResolveAtmType(packet.ATM_ID);
            var routed = _journalRouting.StoreInbound(
                _smartStorageRoot,
                packet.ATM_ID,
                atmType,
                packet.FileName,
                packet.Payload,
                packet.Checksum,
                packet.TransferId,
                routeHint: "journal");

            _journalAnalytics.StoreJournalData(packet.ATM_ID, packet.FileName, packet.Payload, packet.Checksum);
            var record = _journalTransferIntelligence.Upsert(new JournalTransferProgressPacket
            {
                TransferId = packet.TransferId,
                ATM_ID = packet.ATM_ID,
                FileName = packet.FileName,
                ExpectedBytes = packet.Payload.LongLength,
                ReceivedBytes = packet.Payload.LongLength,
                ProgressPercent = 100,
                State = JournalSyncState.Completed,
                Checksum = packet.Checksum,
                Sha256 = packet.Sha256,
                Message = "Received and analyzed on server."
            });
            _syncTracking.AddOrUpdate(record);

            AppendLog($"Server analytics ingested {packet.FileName} from {packet.ATM_ID} ({packet.Payload.Length} bytes). Smart path: {routed.StoragePath}");
            RefreshSync();
            RefreshDeliveryTracker("all");
        }
        catch (Exception ex)
        {
            _alerts.Raise(AlertSeverity.Warning, "Journal ingest failed", ex.Message, "ServerMainForm");
            AppendLog($"Journal ingest failed for {packet.ATM_ID}/{packet.FileName}: {ex.Message}");
            RefreshAlerts();
        }
    }

    private void EvaluateStalledJournalTransfers()
    {
        var stalled = _journalTransferIntelligence.DetectStalledTransfers(TimeSpan.FromMinutes(3));
        foreach (var record in stalled)
        {
            _alerts.Raise(
                AlertSeverity.Warning,
                "Journal transfer delayed",
                $"No progress for {record.FileName} (ATM {record.ATM_ID}).",
                "JournalTransfer",
                $"stalled:{record.SyncId}");
            _syncAlertService.Evaluate(record);
        }

        if (stalled.Count > 0)
            RefreshAlerts();
    }

    private void RetryFailedSync()
    {
        var failed = _syncTracking.Records.Where(r => r.State == JournalSyncState.Failed).ToArray();
        foreach (var record in failed)
        {
            record.State = JournalSyncState.Pending;
            record.ProgressPercent = 0;
            record.Message = "Retry queued from server dashboard.";
            _syncTracking.AddOrUpdate(record);
        }

        RefreshSync();
        AppendLog(failed.Length == 0
            ? "No failed sync items were found."
            : $"Queued retry for {failed.Length} failed sync item(s).");
    }

    private void VerifySyncChecksums()
    {
        var records = _syncTracking.Records;
        var ready = records.Count(r => !string.IsNullOrWhiteSpace(r.LocalPath) && File.Exists(r.LocalPath) && !string.IsNullOrWhiteSpace(r.Checksum));
        var passed = 0;
        var failed = 0;

        foreach (var record in records.Where(r => !string.IsNullOrWhiteSpace(r.LocalPath) && File.Exists(r.LocalPath) && !string.IsNullOrWhiteSpace(r.Checksum)))
        {
            var bytes = File.ReadAllBytes(record.LocalPath);
            if (SecurityHelper.VerifyChecksum(bytes, record.Checksum))
                passed++;
            else
                failed++;
        }

        AppendLog(ready == 0
            ? "No sync records have a local file and checksum to verify."
            : $"Checksum verification completed. Passed={passed}, Failed={failed}.");
    }

    private void CreateArchiveCleanupReport()
    {
        Directory.CreateDirectory(AppConstants.DefaultArchivePath);
        Directory.CreateDirectory(AppConstants.DefaultReportsPath);
        var cutoff = DateTime.UtcNow.AddDays(-180);
        var candidates = Directory.EnumerateFiles(AppConstants.DefaultArchivePath, "*", SearchOption.AllDirectories)
            .Select(path => new FileInfo(path))
            .Where(file => file.Exists && file.LastWriteTimeUtc < cutoff)
            .OrderBy(file => file.LastWriteTimeUtc)
            .ToArray();

        var reportPath = Path.Combine(AppConstants.DefaultReportsPath, $"archive-cleanup-{DateTime.Now:yyyyMMddHHmmss}.csv");
        using var writer = new StreamWriter(reportPath);
        writer.WriteLine("\"File\",\"SizeBytes\",\"LastWriteUtc\"");
        foreach (var file in candidates)
            writer.WriteLine($"\"{file.FullName.Replace("\"", "\"\"")}\",\"{file.Length}\",\"{file.LastWriteTimeUtc:O}\"");

        AppendLog($"Archive cleanup report created with {candidates.Length} candidate file(s): {reportPath}");
    }

    private void RefreshNetworkMap()
    {
        if (_networkMap is null)
            return;

        _networkMap.SuspendLayout();
        try
        {
            _networkMap.Controls.Clear();
            foreach (var atm in _stateStore.Snapshot)
                _networkMap.Controls.Add(CreateAtmCard(atm));
        }
        finally
        {
            _networkMap.ResumeLayout();
        }
    }

    private Control CreateAtmCard(ATMInfo atm)
    {
        // Wave 1 / SS-17 promotion: ATMCardPanel replaces the inline TableLayoutPanel
        // card builder; it carries the 7-colour palette, blink animation for the
        // Syncing state, hover border and a typed double-click event that we wire
        // straight into the ATM detail drawer.
        var card = new ATMCardPanel(atm)
        {
            Margin = new Padding(8)
        };
        card.OnDoubleClickCard += (_, clicked) => new ATMDetailDrawerForm(clicked).Show(this);
        return card;
    }

    private void RefreshFleet()
    {
        _fleetGrid.SuspendLayout();
        _fleetGrid.Rows.Clear();
        foreach (var atm in _stateStore.Snapshot)
        {
            var index = _fleetGrid.Rows.Add(atm.ATM_ID, atm.ATM_Name, atm.ATM_Type, atm.ConnectionStatus, atm.HealthScore);
            _fleetGrid.Rows[index].DefaultCellStyle.BackColor = GetStatusBackColor(atm);
        }
        _fleetGrid.ResumeLayout();
        RefreshNetworkMap();
        RefreshCommandTargets();
        PopulateCommandAuditTargets();
        UpdateSummaryCards();
    }

    private void RefreshSync()
    {
        _syncGrid.SuspendLayout();
        _syncGrid.Rows.Clear();
        foreach (var record in _syncTracking.Records)
        {
            var index = _syncGrid.Rows.Add(record.SyncId, record.ATM_ID, record.FileName, record.State, record.ProgressPercent + "%");
            if (record.State == JournalSyncState.Failed)
                _syncGrid.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(255, 239, 239);
            else if (record.State == JournalSyncState.Completed)
                _syncGrid.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(239, 252, 246);
        }
        _syncGrid.ResumeLayout();
        UpdateSummaryCards();
    }

    private void RefreshDeliveryTracker(string filter)
    {
        if (_deliveryGrid is null)
            return;

        var mode = string.IsNullOrWhiteSpace(filter) ? "all" : filter.Trim().ToLowerInvariant();
        var records = _journalRouting.Receipts.AsEnumerable();
        if (mode == "pending")
            records = records.Where(record => !record.Confirmed);
        else if (mode == "failed")
            records = records.Where(record => !record.Confirmed && record.Detail.Contains("fail", StringComparison.OrdinalIgnoreCase));

        _deliveryGrid.SuspendLayout();
        _deliveryGrid.Rows.Clear();
        foreach (var record in records)
        {
            var shortTransfer = record.TransferId.Length > 12 ? record.TransferId[..12] : record.TransferId;
            var status = record.Confirmed ? "Confirmed" : "Pending/Failed";
            var row = _deliveryGrid.Rows.Add(
                shortTransfer,
                record.ATM_ID,
                record.ATM_Type,
                record.FileName,
                record.Category,
                Math.Max(1, record.FileSize / 1024),
                status,
                record.ReceivedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                record.Detail,
                record.StoragePath);

            _deliveryGrid.Rows[row].DefaultCellStyle.BackColor = record.Confirmed
                ? Color.FromArgb(239, 252, 246)
                : Color.FromArgb(255, 239, 239);
        }
        _deliveryGrid.ResumeLayout();
    }

    private void RefreshOpsAnalytics(int lookbackHours)
    {
        if (_opsAnalyticsGrid is null)
            return;

        var snapshot = BuildOpsAnalyticsSnapshot(lookbackHours);
        _lastOpsAnalyticsSnapshot = snapshot;

        _opsAnalyticsGrid.SuspendLayout();
        _opsAnalyticsGrid.Rows.Clear();
        foreach (var row in snapshot.AtmRows)
        {
            var heartbeatText = row.LastHeartbeatUtc.HasValue && row.LastHeartbeatUtc.Value > DateTime.MinValue
                ? row.LastHeartbeatUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                : "--";
            var heartbeatAgeText = row.MinutesSinceHeartbeat == int.MaxValue
                ? "--"
                : row.MinutesSinceHeartbeat.ToString();

            var index = _opsAnalyticsGrid.Rows.Add(
                row.ATM_ID,
                row.ATM_Type,
                row.ConnectionStatus.ToString(),
                row.HealthScore,
                row.SyncOpen,
                row.SyncFailed,
                row.PendingDeliveries,
                row.CommandFailures,
                row.TelemetryWarnings,
                row.TelemetryErrors,
                row.LastTelemetryAtUtc.HasValue && row.LastTelemetryAtUtc.Value > DateTime.MinValue
                    ? row.LastTelemetryAtUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                    : "--",
                heartbeatText,
                heartbeatAgeText);

            var rowIsAtRisk =
                row.ConnectionStatus == ConnectionStatus.Disconnected ||
                row.SyncFailed > 0 ||
                row.PendingDeliveries > 0 ||
                row.CommandFailures > 0 ||
                row.TelemetryErrors > 0 ||
                row.HealthScore < 60;
            _opsAnalyticsGrid.Rows[index].DefaultCellStyle.BackColor = rowIsAtRisk
                ? Color.FromArgb(255, 239, 239)
                : Color.FromArgb(239, 252, 246);
        }
        _opsAnalyticsGrid.ResumeLayout();

        if (_opsAnalyticsSummary is not null)
        {
            _opsAnalyticsSummary.Text =
                $"Fleet {snapshot.Fleet.Connected}/{snapshot.Fleet.Total} | " +
                $"Sync Open {snapshot.Sync.OpenItems}, Failed {snapshot.Sync.Failed} | " +
                $"Delivery Pending {snapshot.PendingDeliveries}, Failed {snapshot.FailedDeliveries} | " +
                $"Command Failures {snapshot.CommandFailures} | " +
                $"Telemetry Warn {snapshot.TelemetryWarnings}, Error {snapshot.TelemetryErrors}";
        }
    }

    private UnifiedServerAnalyticsSnapshot BuildOpsAnalyticsSnapshot(int lookbackHours)
    {
        var entries = LoadAuditEntriesForAnalytics(lookbackHours);
        return _opsAnalytics.BuildSnapshot(
            _stateStore.Snapshot,
            _syncTracking.Records,
            _journalRouting.Receipts,
            entries,
            DateTime.UtcNow);
    }

    private void ExportOpsAnalyticsSnapshot()
    {
        try
        {
            var snapshot = _lastOpsAnalyticsSnapshot;
            if (snapshot is null)
            {
                RefreshOpsAnalytics(24);
                snapshot = _lastOpsAnalyticsSnapshot;
            }

            if (snapshot is null)
            {
                AppendLog("Ops analytics export skipped: no snapshot data.");
                return;
            }

            Directory.CreateDirectory(AppConstants.DefaultReportsPath);
            var path = Path.Combine(AppConstants.DefaultReportsPath, $"ops-analytics-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
            AppendLog("Ops analytics JSON exported: " + path);
        }
        catch (Exception ex)
        {
            AppendLog("Ops analytics export failed: " + ex.Message);
        }
    }

    private void ExportOperationalWindowReport(string windowName, int lookbackHours)
    {
        try
        {
            var snapshot = BuildOpsAnalyticsSnapshot(Math.Max(1, lookbackHours));
            _lastOpsAnalyticsSnapshot = snapshot;
            var result = _operationalReporting.ExportWindowReport(
                AppConstants.DefaultReportsPath,
                windowName,
                lookbackHours,
                snapshot,
                DateTime.Now,
                DateTime.UtcNow);
            AppendLog($"Ops {result.WindowName} report exported: {result.JsonPath} | {result.CsvPath}");
        }
        catch (Exception ex)
        {
            AppendLog($"Ops window report export failed ({windowName}): {ex.Message}");
        }
    }

    private void ExportOperationalWindowsBundleReport()
    {
        try
        {
            var windows = new[]
            {
                new OperationalWindowSnapshot("shift", 8, BuildOpsAnalyticsSnapshot(8)),
                new OperationalWindowSnapshot("day", 24, BuildOpsAnalyticsSnapshot(24)),
                new OperationalWindowSnapshot("week", 168, BuildOpsAnalyticsSnapshot(168))
            };
            var bundle = _operationalReporting.ExportBundleReport(
                AppConstants.DefaultReportsPath,
                windows,
                DateTime.Now,
                DateTime.UtcNow);
            _lastOpsAnalyticsSnapshot = windows.First(item => item.LookbackHours == 24).Snapshot;
            AppendLog($"Ops bundle report exported: {bundle.JsonPath} | {bundle.SummaryCsvPath} | {bundle.AtmCsvPath}");
        }
        catch (Exception ex)
        {
            AppendLog("Ops bundle export failed: " + ex.Message);
        }
    }

    private void ExportFleetHealthReport()
    {
        try
        {
            var snapshot = BuildOpsAnalyticsSnapshot(24);
            var path = _operationalReporting.ExportFleetHealthReport(AppConstants.DefaultReportsPath, snapshot, DateTime.Now);
            AppendLog("Fleet health report exported: " + path);
        }
        catch (Exception ex)
        {
            AppendLog("Fleet health report failed: " + ex.Message);
        }
    }

    private IReadOnlyList<AuditLogEntry> LoadAuditEntriesForAnalytics(int lookbackHours)
    {
        try
        {
            return _serverAudit.LoadAuditEntries(lookbackHours);
        }
        catch (Exception ex)
        {
            AppendLog("Ops analytics audit fallback: " + ex.Message);
            return Array.Empty<AuditLogEntry>();
        }
    }

    private void RefreshTelemetry(int lookbackHours)
    {
        if (_telemetryTimelineGrid is null || _telemetryAtmGrid is null)
            return;

        var entries = LoadAuditEntriesForAnalytics(lookbackHours);
        var snapshot = _telemetryAnalytics.BuildSnapshot(entries, maxTimelineRows: 2000, maxAtmRows: 500);
        _lastTelemetrySnapshot = snapshot;

        _telemetryTimelineGrid.SuspendLayout();
        _telemetryTimelineGrid.Rows.Clear();
        foreach (var row in snapshot.TimelineRows)
        {
            var index = _telemetryTimelineGrid.Rows.Add(
                row.CreatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                row.ATM_ID,
                row.Severity,
                row.EventType,
                row.Detail);

            _telemetryTimelineGrid.Rows[index].DefaultCellStyle.BackColor = row.Severity switch
            {
                "error" or "critical" or "fatal" => Color.FromArgb(255, 232, 232),
                "warning" or "warn" => Color.FromArgb(255, 248, 230),
                _ => Color.FromArgb(239, 252, 246)
            };
        }
        _telemetryTimelineGrid.ResumeLayout();

        _telemetryAtmGrid.SuspendLayout();
        _telemetryAtmGrid.Rows.Clear();
        foreach (var row in snapshot.AtmSummaryRows)
        {
            var index = _telemetryAtmGrid.Rows.Add(
                row.ATM_ID,
                row.TotalEvents,
                row.WarningEvents,
                row.ErrorEvents,
                row.LastEventType,
                row.LastEventUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));

            _telemetryAtmGrid.Rows[index].DefaultCellStyle.BackColor = row.ErrorEvents > 0
                ? Color.FromArgb(255, 239, 239)
                : row.WarningEvents > 0
                    ? Color.FromArgb(255, 248, 230)
                    : Color.FromArgb(239, 252, 246);
        }
        _telemetryAtmGrid.ResumeLayout();

        if (_telemetrySummary is not null)
        {
            var topEvent = snapshot.TopEventTypes.FirstOrDefault();
            var topEventText = topEvent is null ? "-" : $"{topEvent.EventType} ({topEvent.Count})";
            _telemetrySummary.Text =
                $"Events {snapshot.TotalEvents} | Warn {snapshot.WarningEvents} | Error {snapshot.ErrorEvents} | " +
                $"ATMs {snapshot.DistinctAtms} | Top {topEventText}";
        }
    }

    private void ExportTelemetryTimelineCsv()
    {
        try
        {
            var snapshot = _lastTelemetrySnapshot;
            if (snapshot is null)
            {
                RefreshTelemetry(24);
                snapshot = _lastTelemetrySnapshot;
            }

            if (snapshot is null)
            {
                AppendLog("Telemetry timeline export skipped: no telemetry snapshot.");
                return;
            }

            var path = _telemetryAnalytics.ExportTimelineCsv(AppConstants.DefaultReportsPath, snapshot, DateTime.Now);
            AppendLog("Telemetry timeline CSV exported: " + path);
        }
        catch (Exception ex)
        {
            AppendLog("Telemetry timeline export failed: " + ex.Message);
        }
    }

    private void ExportTelemetryAtmSummaryCsv()
    {
        try
        {
            var snapshot = _lastTelemetrySnapshot;
            if (snapshot is null)
            {
                RefreshTelemetry(24);
                snapshot = _lastTelemetrySnapshot;
            }

            if (snapshot is null)
            {
                AppendLog("Telemetry ATM summary export skipped: no telemetry snapshot.");
                return;
            }

            var path = _telemetryAnalytics.ExportAtmSummaryCsv(AppConstants.DefaultReportsPath, snapshot, DateTime.Now);
            AppendLog("Telemetry ATM summary CSV exported: " + path);
        }
        catch (Exception ex)
        {
            AppendLog("Telemetry ATM summary export failed: " + ex.Message);
        }
    }

    private void RefreshCommandAudit(int lookbackHours)
    {
        if (_commandAuditGrid is null)
            return;

        var selectedAtm = _commandAuditAtmFilter is null ? string.Empty : Convert.ToString(_commandAuditAtmFilter.SelectedItem) ?? string.Empty;
        var atmFilter = string.IsNullOrWhiteSpace(selectedAtm) || string.Equals(selectedAtm, "All ATMs", StringComparison.OrdinalIgnoreCase)
            ? null
            : selectedAtm;
        var selectedScope = _commandAuditScopeFilter is null
            ? "All Command Events"
            : Convert.ToString(_commandAuditScopeFilter.SelectedItem) ?? "All Command Events";
        var scope = selectedScope switch
        {
            "Dispatch Only" => CommandAuditScope.Dispatch,
            "Results Only" => CommandAuditScope.Results,
            "Failures Only" => CommandAuditScope.Failures,
            _ => CommandAuditScope.All
        };

        CommandAuditQueryResult query;
        try
        {
            query = _serverAudit.QueryCommandAudit(atmFilter, lookbackHours, scope);
        }
        catch (Exception ex)
        {
            _commandAuditGrid.Rows.Clear();
            if (_commandAuditSummary is not null)
                _commandAuditSummary.Text = "Rows: 0 | Failures: 0 | Audit log schema unavailable";
            AppendLog("Command audit load warning: " + ex.Message);
            return;
        }

        _commandAuditGrid.SuspendLayout();
        _commandAuditGrid.Rows.Clear();
        foreach (var row in query.Rows)
        {
            var index = _commandAuditGrid.Rows.Add(
                row.PerformedAt,
                row.AtmId,
                row.Action,
                row.PerformedBy,
                row.Details);

            _commandAuditGrid.Rows[index].DefaultCellStyle.BackColor = row.IsFailure
                ? Color.FromArgb(255, 239, 239)
                : Color.FromArgb(239, 252, 246);
        }
        _commandAuditGrid.ResumeLayout();

        if (_commandAuditSummary is not null)
        {
            _commandAuditSummary.Text =
                $"Rows: {query.Rows.Count} | Failures: {query.FailureCount} | Window: last {query.LookbackHours}h";
        }
    }

    private void PopulateCommandAuditTargets()
    {
        if (_commandAuditAtmFilter is null)
            return;

        var selected = Convert.ToString(_commandAuditAtmFilter.SelectedItem);
        var atms = _stateStore.Snapshot
            .Select(atm => atm.ATM_ID ?? atm.ATMId ?? string.Empty)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        _commandAuditAtmFilter.Items.Clear();
        _commandAuditAtmFilter.Items.Add("All ATMs");
        foreach (var atm in atms)
            _commandAuditAtmFilter.Items.Add(atm);

        _commandAuditAtmFilter.SelectedItem = !string.IsNullOrWhiteSpace(selected) && _commandAuditAtmFilter.Items.Contains(selected)
            ? selected
            : "All ATMs";
    }

    private void ExportCommandAuditCsv()
    {
        try
        {
            Directory.CreateDirectory(AppConstants.DefaultReportsPath);
            var path = Path.Combine(AppConstants.DefaultReportsPath, $"command-audit-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
            using var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8);
            writer.WriteLine("performed_at,atm_id,action,performed_by,details");

            foreach (DataGridViewRow row in _commandAuditGrid.Rows)
            {
                if (row.IsNewRow)
                    continue;
                var values = new[]
                {
                    Convert.ToString(row.Cells["Time"].Value) ?? string.Empty,
                    Convert.ToString(row.Cells["ATM"].Value) ?? string.Empty,
                    Convert.ToString(row.Cells["Action"].Value) ?? string.Empty,
                    Convert.ToString(row.Cells["By"].Value) ?? string.Empty,
                    Convert.ToString(row.Cells["Detail"].Value) ?? string.Empty
                };
                writer.WriteLine(string.Join(",", values.Select(value => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"")));
            }

            AppendLog("Command audit CSV exported: " + path);
        }
        catch (Exception ex)
        {
            AppendLog("Command audit export failed: " + ex.Message);
        }
    }

    private void ExportRuntimeLogSnapshot()
    {
        try
        {
            Directory.CreateDirectory(AppConstants.DefaultReportsPath);
            var path = Path.Combine(AppConstants.DefaultReportsPath, $"server-runtime-log-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
            var text = _log is null ? string.Empty : _log.Text;
            File.WriteAllText(path, text);
            AppendLog("Runtime log exported: " + path);
        }
        catch (Exception ex)
        {
            AppendLog("Runtime log export failed: " + ex.Message);
        }
    }

    private static bool IsCommandAuditAction(string? action)
    {
        return ServerAuditService.IsCommandAuditAction(action);
    }

    private void ExportAlertsCsv()
    {
        try
        {
            Directory.CreateDirectory(AppConstants.DefaultReportsPath);
            var path = Path.Combine(AppConstants.DefaultReportsPath, $"alerts-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
            using var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8);
            writer.WriteLine("severity,category,recommended_action,title,message,source,created_at_local");

            foreach (var alert in _alerts.Alerts)
            {
                var category = ClassifyAlert(alert);
                var recommendation = RecommendAlertAction(category);
                var values = new[]
                {
                    alert.Severity.ToString(),
                    category,
                    recommendation,
                    alert.Title,
                    alert.Message,
                    alert.Source,
                    alert.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
                };
                writer.WriteLine(string.Join(",", values.Select(Csv)));
            }

            AppendLog("Alerts CSV exported: " + path);
        }
        catch (Exception ex)
        {
            AppendLog("Alerts export failed: " + ex.Message);
        }
    }

    private static string ClassifyAlert(AlertPayload alert)
    {
        var text = string.Join(" ",
            alert.Title ?? string.Empty,
            alert.Message ?? string.Empty,
            alert.Source ?? string.Empty).ToLowerInvariant();

        if (text.Contains("cash") || text.Contains("cass") || text.Contains("retract") || text.Contains("reject"))
            return "Cash";
        if (text.Contains("network") || text.Contains("disconnect") || text.Contains("handshake") || text.Contains("timeout"))
            return "Connectivity";
        if (text.Contains("journal") || text.Contains("sync") || text.Contains("checksum") || text.Contains("delivery"))
            return "Sync";
        if (text.Contains("tamper") || text.Contains("door") || text.Contains("sensor") || text.Contains("security"))
            return "Security";
        if (text.Contains("command") || text.Contains("probe"))
            return "Command";
        return "System";
    }

    private static string RecommendAlertAction(string category)
    {
        return category switch
        {
            "Cash" => "Reconcile per-cassette inventory and reject/retract counters, then trigger CIT if low or empty.",
            "Connectivity" => "Run host/network probe, verify route/VPN, and confirm sustained heartbeat recovery.",
            "Sync" => "Retry failed sync, verify checksum integrity, and clear blocked transfer records.",
            "Security" => "Escalate to branch security SOP and validate tamper/door/sensor evidence.",
            "Command" => "Review command audit trail and resend only after pre-check validation.",
            _ => "Review runtime logs and server health, then correlate with recent telemetry events."
        };
    }

    private static string Csv(string? value)
    {
        var text = value ?? string.Empty;
        return "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private void RefreshAlerts()
    {
        _alertGrid.SuspendLayout();
        _alertGrid.Rows.Clear();
        foreach (var alert in _alerts.Alerts)
        {
            var category = ClassifyAlert(alert);
            var index = _alertGrid.Rows.Add(
                alert.Severity,
                category,
                RecommendAlertAction(category),
                alert.Title,
                alert.Message,
                alert.Source,
                alert.CreatedAt.ToLocalTime());

            if (alert.Severity >= AlertSeverity.Critical)
                _alertGrid.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(255, 239, 239);
            else if (alert.Severity == AlertSeverity.Warning)
                _alertGrid.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(255, 247, 233);
        }
        _alertGrid.ResumeLayout();
    }

    private void UpdateSummaryCards()
    {
        if (_totalAtmsValue is null)
            return;

        var fleet = _stateStore.BuildSummary();
        var sync = _syncTracking.BuildSummary();
        _totalAtmsValue.Text = fleet.Total.ToString();
        _connectedAtmsValue.Text = fleet.Connected.ToString();
        _syncingAtmsValue.Text = fleet.Syncing.ToString();
        _offlineAtmsValue.Text = fleet.Offline.ToString();
        _fleetHealthValue.Text = $"{fleet.AverageHealth}%";

        if (_syncOpenValue is not null)
        {
            _syncOpenValue.Text = sync.OpenItems.ToString();
            _syncFailedValue.Text = sync.Failed.ToString();
            _syncCompletedValue.Text = sync.Completed.ToString();
            _syncProgressValue.Text = $"{sync.AverageProgress}%";
        }
    }

    private static Color GetStatusBackColor(ATMInfo atm)
    {
        return atm.ConnectionStatus switch
        {
            ConnectionStatus.Connected => Color.FromArgb(239, 252, 246),
            ConnectionStatus.Syncing => Color.FromArgb(239, 247, 255),
            ConnectionStatus.WaitingReply => Color.FromArgb(255, 249, 230),
            ConnectionStatus.Disconnected => Color.FromArgb(255, 239, 239),
            _ => Color.White
        };
    }

    private static Color SoftStatusColor(ATMInfo atm)
    {
        return atm.GetCardState() switch
        {
            ATMCardState.ConnectedActive => Color.FromArgb(232, 245, 233),
            ATMCardState.ConnectedIdle => Color.FromArgb(255, 249, 230),
            ATMCardState.Syncing or ATMCardState.WaitingReply => Color.FromArgb(239, 247, 255),
            ATMCardState.Supervisor => Color.FromArgb(255, 244, 230),
            ATMCardState.RecentlyDisconnected or ATMCardState.WarningOffline => Color.FromArgb(255, 239, 239),
            ATMCardState.CriticalOffline => Color.FromArgb(241, 245, 249),
            _ => Color.White
        };
    }

    private static Label CardLabel(string text, float size, FontStyle style, Color color, ContentAlignment align = ContentAlignment.MiddleLeft)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color,
            TextAlign = align
        };
    }

    private static string ElapsedUtc(DateTime utc)
    {
        if (utc == DateTime.MinValue)
            return "-";

        var elapsed = DateTime.UtcNow - utc;
        if (elapsed.TotalSeconds < 60)
            return "now";
        if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes} min ago";
        if (elapsed.TotalHours < 24)
            return $"{(int)elapsed.TotalHours} hr ago";
        return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }

    private string ResolveAtmType(string atmId)
    {
        if (!string.IsNullOrWhiteSpace(atmId) &&
            _stateStore.TryGet(atmId, out var atm) &&
            atm is not null &&
            !string.IsNullOrWhiteSpace(atm.ATM_Type))
        {
            return atm.ATM_Type;
        }

        return AppConstants.ATM_TYPE_NCR;
    }

    private static IEnumerable<string> EnumerateDirectDistributionFiles(string atmType)
    {
        var normalizedType = AppConstants.NormalizeATMType(atmType);
        var all = EnumerateImageFiles(AppConstants.ShareImagesAllPath);
        var typed = EnumerateImageFiles(Path.Combine(AppConstants.ShareImagesByTypePath, normalizedType));
        return all.Concat(typed).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> EnumerateInboxDistributionFiles(string atmType)
    {
        var normalizedType = AppConstants.NormalizeATMType(atmType);
        var all = EnumerateImageFiles(AppConstants.ShareImagesAllPath);
        var staging = EnumerateImageFiles(AppConstants.ShareImagesStagingPath);
        var typedStaging = EnumerateImageFiles(Path.Combine(AppConstants.ShareImagesStagingPath, normalizedType));
        return all.Concat(staging).Concat(typedStaging).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> EnumerateImageFiles(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            return Array.Empty<string>();

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".bmp",
            ".gif",
            ".webp"
        };

        return Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly)
            .Where(path => allowed.Contains(Path.GetExtension(path)));
    }

    private static void EnsureImageShareFolders()
    {
        foreach (var folder in AppConstants.GetServerImageShareFolders())
            Directory.CreateDirectory(folder);
    }

    private enum ImageDistributionMode
    {
        InboxStaging,
        DirectApply
    }

    private ATMInfo CurrentAtm()
    {
        if (_fleetGrid is not null && _fleetGrid.CurrentRow?.Cells["ATM_ID"].Value is not null)
        {
            var selected = Convert.ToString(_fleetGrid.CurrentRow.Cells["ATM_ID"].Value);
            if (!string.IsNullOrWhiteSpace(selected) && _stateStore.TryGet(selected, out var atm) && atm is not null)
                return atm;
        }

        return _stateStore.Snapshot.FirstOrDefault() ?? new ATMInfo { ATM_ID = "ATM000", ATM_Name = "Unknown ATM" };
    }

    private void AppendLog(string message)
    {
        if (_log is null)
            return;
        if (_log.InvokeRequired)
        {
            _log.BeginInvoke(() => AppendLog(message));
            return;
        }
        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed)
            return;
        if (InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private static void OpenFolder(string path)
    {
        Directory.CreateDirectory(path);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", path) { UseShellExecute = true });
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _refreshTimer.Stop();
        _refreshTimer.Dispose();
        if (_remotePreview is not null)
        {
            var preview = _remotePreview.Image;
            _remotePreview.Image = null;
            preview?.Dispose();
        }
        _journalAnalytics.Dispose();
        _remoteControl.Dispose();
        _serverEngine.Dispose();
        base.OnFormClosed(e);
    }
}

public sealed class ATMDetailForm : Form
{
    public ATMDetailForm(ATMInfo atm)
    {
        Text = $"ATM Detail - {atm.ATM_ID}";
        Size = new Size(640, 420);
        Controls.Add(new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = atm });
    }
}

public sealed class ATMDetailDrawerForm : Form
{
    public ATMDetailDrawerForm(ATMInfo atm)
    {
        Text = $"ATM Drawer - {atm.ATM_ID}";
        Size = new Size(420, 640);
        StartPosition = FormStartPosition.CenterParent;
        Controls.Add(new Label { Dock = DockStyle.Top, Height = 44, Text = atm.GetStatusDescription(), TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 12F, FontStyle.Bold) });
        Controls.Add(new PropertyGrid { Dock = DockStyle.Fill, SelectedObject = atm });
    }
}

public sealed class JournalViewerForm_StubRemovedByWave3SS105 : Form
{
    public JournalViewerForm_StubRemovedByWave3SS105()
    {
        // SS-10.5 replaces the stub with JournalStudioForm; this class is kept
        // only as a placeholder so any external reference still compiles.
        Text = "Journal Studio (deprecated stub)";
        Size = new Size(900, 620);
    }
}

public sealed class SyncDashboardForm : Form
{
    public SyncDashboardForm(IEnumerable<JournalSyncRecord> records)
    {
        Text = "Sync Dashboard";
        Size = new Size(820, 520);
        var grid = UiHelpers.Grid();
        grid.Columns.Add("ATM", "ATM");
        grid.Columns.Add("File", "File");
        grid.Columns.Add("State", "State");
        grid.Columns.Add("Progress", "Progress");
        foreach (var record in records)
            grid.Rows.Add(record.ATM_ID, record.FileName, record.State, record.ProgressPercent + "%");
        Controls.Add(grid);
    }
}

