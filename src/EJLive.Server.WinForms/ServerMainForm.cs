using EJLive.Core;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Server.Services;
using EJLive.Shared;
using System.Collections.Concurrent;
using System.Text.Json;
using AppConstants = EJLive.Core.AppConstants;
using NetworkConfig = EJLive.Core.NetworkConfig;

namespace EJLive.Server.WinForms;

public sealed class ServerMainForm : Form
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
    private readonly ServerAuditService _serverAudit = new();
    private readonly JournalSyncAlertService _syncAlertService;
    private readonly JournalAnalyticsService _journalAnalytics;
    private readonly RemoteControlService _remoteControl;
    private DataGridView _fleetGrid = null!;
    private DataGridView _syncGrid = null!;
    private DataGridView _deliveryGrid = null!;
    private DataGridView _alertGrid = null!;
    private DataGridView _commandGrid = null!;
    private DataGridView _commandAuditGrid = null!;
    private DataGridView _opsAnalyticsGrid = null!;
    private DataGridView _telemetryTimelineGrid = null!;
    private DataGridView _telemetryAtmGrid = null!;
    private MenuStrip _mainMenu = null!;
    private PictureBox _remotePreview = null!;
    private Label _remotePreviewStatus = null!;
    private FlowLayoutPanel _networkMap = null!;
    private RichTextBox _log = null!;
    private ComboBox _commandTarget = null!;
    private Label _totalAtmsValue = null!;
    private Label _connectedAtmsValue = null!;
    private Label _syncingAtmsValue = null!;
    private Label _offlineAtmsValue = null!;
    private Label _fleetHealthValue = null!;
    private Label _syncOpenValue = null!;
    private Label _syncFailedValue = null!;
    private Label _syncCompletedValue = null!;
    private Label _syncProgressValue = null!;
    private Label _commandAuditSummary = null!;
    private Label _opsAnalyticsSummary = null!;
    private Label _telemetrySummary = null!;
    private NumericUpDown _listenPort = null!;
    private ComboBox _commandAuditAtmFilter = null!;
    private ComboBox _commandAuditScopeFilter = null!;
    private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 5000 };
    private readonly string _smartStorageRoot = Path.Combine(AppConstants.DefaultServerSharePath, "SmartStorage");
    private readonly ConcurrentDictionary<string, int> _remoteCommandRows = new(StringComparer.OrdinalIgnoreCase);
    private UnifiedServerAnalyticsSnapshot? _lastOpsAnalyticsSnapshot;
    private ClientTelemetryAnalyticsSnapshot? _lastTelemetrySnapshot;
    private DateTime _lastTelemetryUiRefreshUtc = DateTime.MinValue;

    public ServerMainForm()
    {
        DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath);
        _telemetryState = new ClientTelemetryStateService(_stateStore, _alerts);
        _journalAnalytics = new JournalAnalyticsService(_smartStorageRoot, AppConstants.DefaultArchivePath);
        _remoteControl = new RemoteControlService(_serverEngine);
        _syncAlertService = new JournalSyncAlertService(_alerts);
        _journalAnalytics.OnLog += message => AppendLog("[JournalAnalytics] " + message);
        _remoteControl.OnLog += message => AppendLog("[RemoteControl] " + message);
        _remoteControl.OnCommandResult += (atmId, commandId, success, result) =>
            RunOnUi(() => HandleTrackedCommandResult(atmId, commandId, success, result));
        Text = "EJLive Enterprise Server";
        MinimumSize = new Size(1120, 740);
        Size = new Size(1220, 820);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);
        DoubleBuffered = true;

        EnsureImageShareFolders();
        WireServerEngine();
        SeedRuntimeState();
        InitializeUi();
        Shown += (_, _) => StartServer();
        _refreshTimer.Tick += (_, _) =>
        {
            UpdateSummaryCards();
            RefreshNetworkMap();
            EvaluateStalledJournalTransfers();
        };
        _refreshTimer.Start();
    }

    private void InitializeUi()
    {
        _mainMenu = BuildMainMenu();
        MainMenuStrip = _mainMenu;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildFleetTab());
        tabs.TabPages.Add(BuildNetworkMapTab());
        tabs.TabPages.Add(BuildJournalViewerTab());
        tabs.TabPages.Add(BuildSyncDashboardTab());
        tabs.TabPages.Add(BuildDeliveryTrackerTab());
        tabs.TabPages.Add(BuildRemoteCommandsTab());
        tabs.TabPages.Add(BuildAlertsTab());
        tabs.TabPages.Add(BuildArchiveTab());
        tabs.TabPages.Add(BuildReportsTab());
        tabs.TabPages.Add(BuildOpsAnalyticsTab());
        tabs.TabPages.Add(BuildTelemetryTab());
        tabs.TabPages.Add(BuildCommandAuditTab());
        tabs.TabPages.Add(BuildSettingsTab());
        Controls.Add(tabs);
        Controls.Add(_mainMenu);
    }

    private MenuStrip BuildMainMenu()
    {
        var menu = new MenuStrip { Dock = DockStyle.Top };

        var file = new ToolStripMenuItem("File");
        file.DropDownItems.Add("Start Server", null, (_, _) => StartServer());
        file.DropDownItems.Add("Stop Server", null, (_, _) => StopServer());
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("Exit", null, (_, _) => Close());

        var operations = new ToolStripMenuItem("Operations");
        operations.DropDownItems.Add("Refresh Fleet", null, (_, _) => RefreshFleet());
        operations.DropDownItems.Add("Refresh Ops Analytics (24h)", null, (_, _) => RefreshOpsAnalytics(24));
        operations.DropDownItems.Add("Refresh Telemetry (24h)", null, (_, _) => RefreshTelemetry(24));
        operations.DropDownItems.Add("Daily Ops Report", null, (_, _) => ExportOperationalWindowReport("day", 24));

        var admin = new ToolStripMenuItem("Admin");
        admin.DropDownItems.Add("Ping Selected ATM", null, (_, _) => SendRemoteCommand(AppConstants.CMD_PING));
        admin.DropDownItems.Add("Force Sync Selected ATM", null, (_, _) => SendRemoteCommand(AppConstants.CMD_FORCE_SYNC));
        admin.DropDownItems.Add("Queue Connectivity Probe", null, (_, _) => SendConnectivityProbe());

        var logs = new ToolStripMenuItem("Logs");
        logs.DropDownItems.Add("Export Runtime Log", null, (_, _) => ExportRuntimeLogSnapshot());
        logs.DropDownItems.Add("Clear Runtime Log", null, (_, _) => { if (_log is not null) _log.Clear(); });
        logs.DropDownItems.Add(new ToolStripSeparator());
        logs.DropDownItems.Add("Open Reports Folder", null, (_, _) => OpenFolder(AppConstants.DefaultReportsPath));
        logs.DropDownItems.Add("Open Archive Folder", null, (_, _) => OpenFolder(AppConstants.DefaultArchivePath));

        menu.Items.Add(file);
        menu.Items.Add(operations);
        menu.Items.Add(admin);
        menu.Items.Add(logs);
        return menu;
    }

    private TabPage BuildFleetTab()
    {
        var tab = new TabPage("Fleet");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Refresh Fleet", RefreshFleet));
        actions.Controls.Add(Ui.Button("Open ATM Details", () => new ATMDetailForm(CurrentAtm()).Show(this)));
        actions.Controls.Add(Ui.Button("Open Detail Drawer", () => new ATMDetailDrawerForm(CurrentAtm()).Show(this)));
        actions.Controls.Add(Ui.Button("Broadcast Message", () => { _serverEngine.Broadcast("Server broadcast from EJLive."); AppendLog("Broadcast message sent."); }));
        actions.Controls.Add(Ui.Button("Start Server", StartServer));
        actions.Controls.Add(Ui.Button("Stop Server", StopServer));
        var summary = Ui.CardRow(5);
        _totalAtmsValue = Ui.AddMetricCard(summary, "Total ATMs", "0", Color.FromArgb(46, 134, 222));
        _connectedAtmsValue = Ui.AddMetricCard(summary, "Connected", "0", Color.FromArgb(16, 172, 132));
        _syncingAtmsValue = Ui.AddMetricCard(summary, "Syncing", "0", Color.FromArgb(255, 159, 67));
        _offlineAtmsValue = Ui.AddMetricCard(summary, "Offline", "0", Color.FromArgb(238, 82, 83));
        _fleetHealthValue = Ui.AddMetricCard(summary, "Avg Health", "0%", Color.FromArgb(95, 39, 205));
        _fleetGrid = Ui.Grid();
        _fleetGrid.Columns.Add("ATM_ID", "ATM Id");
        _fleetGrid.Columns.Add("Name", "Name");
        _fleetGrid.Columns.Add("Type", "Type");
        _fleetGrid.Columns.Add("Status", "Status");
        _fleetGrid.Columns.Add("Health", "Health");
        root.Controls.Add(_fleetGrid);
        root.Controls.Add(summary);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshFleet();
        return tab;
    }

    private TabPage BuildNetworkMapTab()
    {
        var tab = new TabPage("Network Map");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Refresh Map", RefreshNetworkMap));
        actions.Controls.Add(Ui.Button("Open Selected ATM", () => new ATMDetailDrawerForm(CurrentAtm()).Show(this)));
        actions.Controls.Add(Ui.Button("Broadcast Status Check", () =>
        {
            _serverEngine.Broadcast("STATUS_CHECK");
            AppendLog("Status check broadcast sent.");
        }));

        var legend = new Label
        {
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Green: active | Yellow: idle | Blue: syncing/waiting | Orange: supervisor | Red: recently offline | Gray: critical offline",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(241, 245, 249)
        };

        _networkMap = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(246, 248, 250)
        };

        root.Controls.Add(_networkMap);
        root.Controls.Add(legend);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshNetworkMap();
        return tab;
    }

    private TabPage BuildJournalViewerTab()
    {
        var tab = new TabPage("Journal Viewer");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Open Viewer", () => new JournalViewerForm().Show(this)));
        actions.Controls.Add(Ui.Button("Load Today", () => AppendLog("Today's journals loaded.")));
        actions.Controls.Add(Ui.Button("Export", () => AppendLog("Journal export generated.")));
        actions.Controls.Add(Ui.Button("Search", () => AppendLog("Journal search completed.")));
        _log = Ui.LogBox();
        root.Controls.Add(_log);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildSyncDashboardTab()
    {
        var tab = new TabPage("Sync Dashboard");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Open Sync Dashboard", () => new SyncDashboardForm(_syncTracking.Records).Show(this)));
        actions.Controls.Add(Ui.Button("Retry Failed", RetryFailedSync));
        actions.Controls.Add(Ui.Button("Verify Checksums", VerifySyncChecksums));
        var summary = Ui.CardRow(4);
        _syncOpenValue = Ui.AddMetricCard(summary, "Open Sync", "0", Color.FromArgb(46, 134, 222));
        _syncFailedValue = Ui.AddMetricCard(summary, "Failed Sync", "0", Color.FromArgb(238, 82, 83));
        _syncCompletedValue = Ui.AddMetricCard(summary, "Completed", "0", Color.FromArgb(16, 172, 132));
        _syncProgressValue = Ui.AddMetricCard(summary, "Avg Progress", "0%", Color.FromArgb(255, 159, 67));
        _syncGrid = Ui.Grid();
        _syncGrid.Columns.Add("SyncId", "Sync Id");
        _syncGrid.Columns.Add("ATM", "ATM");
        _syncGrid.Columns.Add("File", "File");
        _syncGrid.Columns.Add("State", "State");
        _syncGrid.Columns.Add("Progress", "Progress");
        root.Controls.Add(_syncGrid);
        root.Controls.Add(summary);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshSync();
        return tab;
    }

    private TabPage BuildDeliveryTrackerTab()
    {
        var tab = new TabPage("Delivery Tracker");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Refresh", () => RefreshDeliveryTracker("all")));
        actions.Controls.Add(Ui.Button("Pending Only", () => RefreshDeliveryTracker("pending")));
        actions.Controls.Add(Ui.Button("Failed Only", () => RefreshDeliveryTracker("failed")));
        actions.Controls.Add(Ui.Button("Open Smart Storage", () => OpenFolder(_smartStorageRoot)));

        _deliveryGrid = Ui.Grid();
        _deliveryGrid.Columns.Add("TransferId", "Transfer Id");
        _deliveryGrid.Columns.Add("ATM", "ATM");
        _deliveryGrid.Columns.Add("Type", "Type");
        _deliveryGrid.Columns.Add("File", "File");
        _deliveryGrid.Columns.Add("Category", "Category");
        _deliveryGrid.Columns.Add("SizeKB", "Size KB");
        _deliveryGrid.Columns.Add("Status", "Status");
        _deliveryGrid.Columns.Add("Received", "Received");
        _deliveryGrid.Columns.Add("Detail", "Detail");
        _deliveryGrid.Columns.Add("Path", "Storage Path");

        root.Controls.Add(_deliveryGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshDeliveryTracker("all");
        return tab;
    }

    private TabPage BuildRemoteCommandsTab()
    {
        var tab = new TabPage("Remote Commands");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        _commandTarget = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(4) };
        actions.Controls.Add(new Label { Text = "Target", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
        actions.Controls.Add(_commandTarget);
        actions.Controls.Add(Ui.Button("Refresh Targets", RefreshCommandTargets));
        actions.Controls.Add(Ui.Button("Ping", () => SendRemoteCommand(AppConstants.CMD_PING)));
        actions.Controls.Add(Ui.Button("Ping (Tracked)", SendPingTracked));
        actions.Controls.Add(Ui.Button("Connectivity Probe", SendConnectivityProbe));
        actions.Controls.Add(Ui.Button("Sync Time", () => SendRemoteCommand(AppConstants.CMD_SYNC_TIME)));
        actions.Controls.Add(Ui.Button("Capture Screen", () => SendRemoteCommand(AppConstants.CMD_SCREENSHOT)));
        actions.Controls.Add(Ui.Button("Start Remote Session", () => SendRemoteCommand(AppConstants.CMD_REMOTE_SESSION_START)));
        actions.Controls.Add(Ui.Button("Stop Remote Session", () => SendRemoteCommand(AppConstants.CMD_REMOTE_SESSION_STOP)));
        actions.Controls.Add(Ui.Button("RDP Start", SendWindowsRemoteStart));
        actions.Controls.Add(Ui.Button("RDP Check", SendWindowsRemoteCheck));
        actions.Controls.Add(Ui.Button("RDP Stop", SendWindowsRemoteStop));
        actions.Controls.Add(Ui.Button("Change Password", SendChangePassword));
        actions.Controls.Add(Ui.Button("Change Win Password", SendChangeWindowsPassword));
        actions.Controls.Add(Ui.Button("Request Journal", SendJournalRequest));
        actions.Controls.Add(Ui.Button("Image -> Inbox", () => SendImageToTarget(ImageDistributionMode.InboxStaging)));
        actions.Controls.Add(Ui.Button("Image -> Direct", () => SendImageToTarget(ImageDistributionMode.DirectApply)));
        actions.Controls.Add(Ui.Button("Dist Folder -> Inbox", DistributeImagesFromServerFoldersInbox));
        actions.Controls.Add(Ui.Button("Dist Folder -> Direct", DistributeImagesFromServerFoldersDirect));
        actions.Controls.Add(Ui.Button("Sync Images", SendSyncImages));
        actions.Controls.Add(Ui.Button("Force Sync", () => SendRemoteCommand(AppConstants.CMD_FORCE_SYNC)));
        actions.Controls.Add(Ui.Button("Restart", () => SendRemoteCommand(AppConstants.CMD_RESTART)));
        actions.Controls.Add(Ui.Button("Tracked Restart", SendTrackedRestart));

        _remotePreview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black
        };
        _remotePreviewStatus = new Label
        {
            Dock = DockStyle.Top,
            Height = 26,
            Text = "Remote preview idle",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(241, 245, 249)
        };
        var previewPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 220,
            Padding = new Padding(0, 6, 0, 0)
        };
        previewPanel.Controls.Add(_remotePreview);
        previewPanel.Controls.Add(_remotePreviewStatus);

        _commandGrid = Ui.Grid();
        _commandGrid.Columns.Add("Time", "Time");
        _commandGrid.Columns.Add("Target", "Target");
        _commandGrid.Columns.Add("Command", "Command");
        _commandGrid.Columns.Add("Status", "Status");
        _commandGrid.Columns.Add("CommandId", "Command Id");
        _commandGrid.Columns.Add("Result", "Result");

        root.Controls.Add(_commandGrid);
        root.Controls.Add(previewPanel);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshCommandTargets();
        return tab;
    }

    private TabPage BuildAlertsTab()
    {
        var tab = new TabPage("Alerts");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Raise Test Alert", () => { _alerts.Raise(AlertSeverity.Warning, "Test Alert", "Manual test alert", "Server"); RefreshAlerts(); }));
        actions.Controls.Add(Ui.Button("Mark Read", () => AppendLog("Alert marked read.")));
        actions.Controls.Add(Ui.Button("Export Alerts", ExportAlertsCsv));
        actions.Controls.Add(Ui.Button("Open Reports Folder", () => OpenFolder(AppConstants.DefaultReportsPath)));
        _alertGrid = Ui.Grid();
        _alertGrid.Columns.Add("Severity", "Severity");
        _alertGrid.Columns.Add("Category", "Category");
        _alertGrid.Columns.Add("Recommendation", "Recommended Action");
        _alertGrid.Columns.Add("Title", "Title");
        _alertGrid.Columns.Add("Message", "Message");
        _alertGrid.Columns.Add("Source", "Source");
        _alertGrid.Columns.Add("Created", "Created");
        root.Controls.Add(_alertGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildArchiveTab()
    {
        var tab = new TabPage("Archive");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Run Archive", () => AppendLog("Archive cycle completed.")));
        actions.Controls.Add(Ui.Button("Archive Eligible Journals", RunJournalArchive));
        actions.Controls.Add(Ui.Button("Open Archive Folder", () => OpenFolder(AppConstants.DefaultArchivePath)));
        actions.Controls.Add(Ui.Button("Cleanup Report", CreateArchiveCleanupReport));
        root.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Archive manager keeps server journal copies, monthly folders, and reports.", TextAlign = ContentAlignment.MiddleCenter });
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildReportsTab()
    {
        var tab = new TabPage("Reports");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Shift Ops Report", () => ExportOperationalWindowReport("shift", 8)));
        actions.Controls.Add(Ui.Button("Daily Ops Report", () => ExportOperationalWindowReport("day", 24)));
        actions.Controls.Add(Ui.Button("Weekly Ops Report", () => ExportOperationalWindowReport("week", 168)));
        actions.Controls.Add(Ui.Button("Fleet Health Report", ExportFleetHealthReport));
        actions.Controls.Add(Ui.Button("Combined Ops Report", ExportOperationalWindowsBundleReport));
        actions.Controls.Add(Ui.Button("Open Reports Folder", () => OpenFolder(AppConstants.DefaultReportsPath)));
        root.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "Reports combine journal sync, alerts, uptime, and transaction analysis.", TextAlign = ContentAlignment.MiddleCenter });
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        return tab;
    }

    private TabPage BuildOpsAnalyticsTab()
    {
        var tab = new TabPage("Ops Analytics");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        _opsAnalyticsSummary = new Label
        {
            AutoSize = true,
            Padding = new Padding(8, 8, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105)
        };

        actions.Controls.Add(Ui.Button("Refresh", () => RefreshOpsAnalytics(24)));
        actions.Controls.Add(Ui.Button("Last 1h", () => RefreshOpsAnalytics(1)));
        actions.Controls.Add(Ui.Button("Last 24h", () => RefreshOpsAnalytics(24)));
        actions.Controls.Add(Ui.Button("Export JSON", ExportOpsAnalyticsSnapshot));
        actions.Controls.Add(Ui.Button("Open Reports Folder", () => OpenFolder(AppConstants.DefaultReportsPath)));
        actions.Controls.Add(_opsAnalyticsSummary);

        _opsAnalyticsGrid = Ui.Grid();
        _opsAnalyticsGrid.Columns.Add("ATM", "ATM");
        _opsAnalyticsGrid.Columns.Add("Type", "Type");
        _opsAnalyticsGrid.Columns.Add("Conn", "Connection");
        _opsAnalyticsGrid.Columns.Add("Health", "Health");
        _opsAnalyticsGrid.Columns.Add("SyncOpen", "Sync Open");
        _opsAnalyticsGrid.Columns.Add("SyncFail", "Sync Failed");
        _opsAnalyticsGrid.Columns.Add("PendingDel", "Pending Delivery");
        _opsAnalyticsGrid.Columns.Add("CmdFail", "Command Failures");
        _opsAnalyticsGrid.Columns.Add("TelWarn", "Telemetry Warn");
        _opsAnalyticsGrid.Columns.Add("TelErr", "Telemetry Error");
        _opsAnalyticsGrid.Columns.Add("LastTel", "Last Telemetry");
        _opsAnalyticsGrid.Columns.Add("LastHB", "Last Heartbeat");
        _opsAnalyticsGrid.Columns.Add("HBMin", "HB Age (min)");

        root.Controls.Add(_opsAnalyticsGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshOpsAnalytics(24);
        return tab;
    }

    private TabPage BuildCommandAuditTab()
    {
        var tab = new TabPage("Command Audit");
        var root = Ui.Stack();
        var actions = Ui.Flow();

        _commandAuditAtmFilter = new ComboBox
        {
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(4)
        };
        _commandAuditScopeFilter = new ComboBox
        {
            Width = 170,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(4)
        };
        _commandAuditScopeFilter.Items.AddRange(new object[]
        {
            "All Command Events",
            "Dispatch Only",
            "Results Only",
            "Failures Only"
        });
        _commandAuditScopeFilter.SelectedIndex = 0;

        _commandAuditSummary = new Label
        {
            AutoSize = true,
            Padding = new Padding(8, 8, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105)
        };

        actions.Controls.Add(new Label { Text = "ATM", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
        actions.Controls.Add(_commandAuditAtmFilter);
        actions.Controls.Add(new Label { Text = "Scope", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
        actions.Controls.Add(_commandAuditScopeFilter);
        actions.Controls.Add(Ui.Button("Refresh", () => RefreshCommandAudit(24)));
        actions.Controls.Add(Ui.Button("Last 1h", () => RefreshCommandAudit(1)));
        actions.Controls.Add(Ui.Button("Last 24h", () => RefreshCommandAudit(24)));
        actions.Controls.Add(Ui.Button("Export CSV", ExportCommandAuditCsv));
        actions.Controls.Add(Ui.Button("Open Reports Folder", () => OpenFolder(AppConstants.DefaultReportsPath)));
        actions.Controls.Add(_commandAuditSummary);

        _commandAuditGrid = Ui.Grid();
        _commandAuditGrid.Columns.Add("Time", "Time");
        _commandAuditGrid.Columns.Add("ATM", "ATM");
        _commandAuditGrid.Columns.Add("Action", "Action");
        _commandAuditGrid.Columns.Add("By", "By");
        _commandAuditGrid.Columns.Add("Detail", "Detail");

        root.Controls.Add(_commandAuditGrid);
        root.Controls.Add(actions);
        tab.Controls.Add(root);

        PopulateCommandAuditTargets();
        RefreshCommandAudit(24);
        return tab;
    }

    private TabPage BuildTelemetryTab()
    {
        var tab = new TabPage("Telemetry");
        var root = Ui.Stack();
        var actions = Ui.Flow();
        _telemetrySummary = new Label
        {
            AutoSize = true,
            Padding = new Padding(8, 8, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105)
        };

        actions.Controls.Add(Ui.Button("Refresh", () => RefreshTelemetry(24)));
        actions.Controls.Add(Ui.Button("Last 1h", () => RefreshTelemetry(1)));
        actions.Controls.Add(Ui.Button("Last 24h", () => RefreshTelemetry(24)));
        actions.Controls.Add(Ui.Button("Export Timeline CSV", ExportTelemetryTimelineCsv));
        actions.Controls.Add(Ui.Button("Export ATM Summary CSV", ExportTelemetryAtmSummaryCsv));
        actions.Controls.Add(Ui.Button("Open Reports Folder", () => OpenFolder(AppConstants.DefaultReportsPath)));
        actions.Controls.Add(_telemetrySummary);

        _telemetryTimelineGrid = Ui.Grid();
        _telemetryTimelineGrid.Columns.Add("Time", "Time");
        _telemetryTimelineGrid.Columns.Add("ATM", "ATM");
        _telemetryTimelineGrid.Columns.Add("Severity", "Severity");
        _telemetryTimelineGrid.Columns.Add("Type", "Type");
        _telemetryTimelineGrid.Columns.Add("Detail", "Detail");

        _telemetryAtmGrid = Ui.Grid();
        _telemetryAtmGrid.Columns.Add("ATM", "ATM");
        _telemetryAtmGrid.Columns.Add("Total", "Total");
        _telemetryAtmGrid.Columns.Add("Warnings", "Warnings");
        _telemetryAtmGrid.Columns.Add("Errors", "Errors");
        _telemetryAtmGrid.Columns.Add("LastType", "Last Event Type");
        _telemetryAtmGrid.Columns.Add("LastAt", "Last Event Time");

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 280
        };
        split.Panel1.Controls.Add(_telemetryTimelineGrid);
        split.Panel2.Controls.Add(_telemetryAtmGrid);

        root.Controls.Add(split);
        root.Controls.Add(actions);
        tab.Controls.Add(root);
        RefreshTelemetry(24);
        return tab;
    }

    private TabPage BuildSettingsTab()
    {
        var tab = new TabPage("Settings");
        var panel = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(16) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _listenPort = new NumericUpDown { Minimum = 1, Maximum = 65535, Value = NetworkConfig.DEFAULT_PORT };
        AddRow(panel, "Listen Port", _listenPort);
        AddRow(panel, "Storage Path", new TextBox { Text = ATMPaths.SERVER_DEFAULT_DRIVE + @"\" + ATMPaths.SERVER_EJOURNAL_FILES, Dock = DockStyle.Fill });
        AddRow(panel, "Archive Path", new TextBox { Text = AppConstants.DefaultArchivePath, Dock = DockStyle.Fill });
        AddRow(panel, "Max Connections", new NumericUpDown { Minimum = 1, Maximum = 5000, Value = 100 });
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Save Server Settings", () => AppendLog("Server settings saved.")));
        actions.Controls.Add(Ui.Button("Initialize Database", () => { DatabaseManager.Instance.Initialize(AppConstants.DefaultDatabasePath); AppendLog("Database initialized."); }));
        var root = Ui.Stack();
        root.Controls.Add(actions);
        root.Controls.Add(panel);
        tab.Controls.Add(root);
        return tab;
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
        var accent = atm.GetCardColor();
        var card = new Panel
        {
            Width = 270,
            Height = 178,
            Margin = new Padding(8),
            Padding = new Padding(0),
            BackColor = SoftStatusColor(atm),
            Cursor = Cursors.Hand
        };

        var colorBar = new Panel { Dock = DockStyle.Top, Height = 6, BackColor = accent };
        var body = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(10, 8, 10, 8),
            BackColor = Color.Transparent
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        for (var i = 0; i < 7; i++)
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 7));

        var id = CardLabel(atm.ATM_ID ?? "UNKNOWN", 10F, FontStyle.Bold, Color.FromArgb(31, 41, 55));
        var type = CardLabel(atm.ATM_Type ?? "ATM", 8.5F, FontStyle.Regular, Color.FromArgb(100, 116, 139), ContentAlignment.MiddleRight);
        var status = CardLabel(atm.GetStatusLabel(), 8.5F, FontStyle.Bold, accent, ContentAlignment.MiddleCenter);
        var name = CardLabel(atm.ATM_Name ?? atm.ATM_ID ?? "Unnamed ATM", 8.5F, FontStyle.Regular, Color.FromArgb(31, 41, 55), ContentAlignment.MiddleCenter);
        var network = CardLabel($"Network: {atm.NetworkType}", 8F, FontStyle.Regular, Color.FromArgb(100, 116, 139), ContentAlignment.MiddleCenter);
        var heartbeat = CardLabel($"Heartbeat: {ElapsedUtc(atm.LastHeartbeatUtc)}", 8F, FontStyle.Regular, Color.FromArgb(100, 116, 139), ContentAlignment.MiddleCenter);
        var lastData = CardLabel($"Last data: {ElapsedUtc(atm.LastDataReceivedUtc)}", 8F, FontStyle.Regular, Color.FromArgb(100, 116, 139), ContentAlignment.MiddleCenter);
        var health = CardLabel($"Health: {atm.HealthScore}%", 8F, FontStyle.Bold, Color.FromArgb(31, 41, 55), ContentAlignment.MiddleCenter);
        var stats = CardLabel($"OK {atm.ApprovedTransactions} | Failed {atm.FailedTransactions} | Cards {atm.CardsCaptured}", 8F, FontStyle.Regular, Color.FromArgb(71, 85, 105), ContentAlignment.MiddleCenter);

        body.Controls.Add(id, 0, 0);
        body.Controls.Add(type, 1, 0);
        body.Controls.Add(status, 0, 1);
        body.SetColumnSpan(status, 2);
        body.Controls.Add(name, 0, 2);
        body.SetColumnSpan(name, 2);
        body.Controls.Add(network, 0, 3);
        body.SetColumnSpan(network, 2);
        body.Controls.Add(heartbeat, 0, 4);
        body.SetColumnSpan(heartbeat, 2);
        body.Controls.Add(lastData, 0, 5);
        body.SetColumnSpan(lastData, 2);
        body.Controls.Add(stats, 0, 6);
        body.Controls.Add(health, 1, 6);

        card.Controls.Add(body);
        card.Controls.Add(colorBar);
        card.DoubleClick += (_, _) => new ATMDetailDrawerForm(atm).Show(this);
        body.DoubleClick += (_, _) => new ATMDetailDrawerForm(atm).Show(this);
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

    private static void AddRow(TableLayoutPanel panel, string label, Control control)
    {
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        panel.Controls.Add(control);
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

public sealed class JournalViewerForm : Form
{
    public JournalViewerForm()
    {
        Text = "Journal Viewer";
        Size = new Size(900, 620);
        var box = Ui.LogBox();
        var actions = Ui.Flow();
        actions.Controls.Add(Ui.Button("Load", () => box.Text = "Journal viewer ready."));
        actions.Controls.Add(Ui.Button("Export", () => box.AppendText("Export requested." + Environment.NewLine)));
        Controls.Add(box);
        Controls.Add(actions);
    }
}

public sealed class SyncDashboardForm : Form
{
    public SyncDashboardForm(IEnumerable<JournalSyncRecord> records)
    {
        Text = "Sync Dashboard";
        Size = new Size(820, 520);
        var grid = Ui.Grid();
        grid.Columns.Add("ATM", "ATM");
        grid.Columns.Add("File", "File");
        grid.Columns.Add("State", "State");
        grid.Columns.Add("Progress", "Progress");
        foreach (var record in records)
            grid.Rows.Add(record.ATM_ID, record.FileName, record.State, record.ProgressPercent + "%");
        Controls.Add(grid);
    }
}

internal static class Ui
{
    public static Panel Stack() => new() { Dock = DockStyle.Fill, Padding = new Padding(8) };
    public static FlowLayoutPanel Flow() => new() { Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true };
    public static Button Button(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, Height = 32, Margin = new Padding(4) };
        button.Click += (_, _) => action();
        return button;
    }
    public static TableLayoutPanel CardRow(int columns)
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Top, Height = 92, ColumnCount = columns, Padding = new Padding(4) };
        for (var i = 0; i < columns; i++)
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
        return row;
    }

    public static Label AddMetricCard(TableLayoutPanel row, string title, string value, Color accent)
    {
        var card = new Panel { Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White };
        var accentBar = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent };
        var titleLabel = new Label { Text = title, Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F) };
        var valueLabel = new Label { Text = value, Dock = DockStyle.Fill, ForeColor = Color.FromArgb(35, 35, 35), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        card.Controls.Add(accentBar);
        row.Controls.Add(card);
        return valueLabel;
    }

    public static DataGridView Grid()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.None };
        grid.EnableDoubleBuffering();
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        grid.EnableHeadersVisualStyles = false;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        return grid;
    }
    public static RichTextBox LogBox() => new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F) };
}

internal static class ControlRenderingExtensions
{
    public static void EnableDoubleBuffering(this Control control)
    {
        var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        property?.SetValue(control, true, null);
    }
}
