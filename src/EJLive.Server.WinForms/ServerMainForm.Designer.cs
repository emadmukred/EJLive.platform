using System.Drawing;
using System.Windows.Forms;
using EJLive.Core;

namespace EJLive.Server.WinForms;

/// <summary>
/// Designer surface for <see cref="ServerMainForm"/> (SS-10 / C-30, Enterprise Server
/// console). Authored to the shape Visual Studio's WinForms designer emits and consumes:
/// every control is a named field, created and property-set only inside
/// <c>InitializeComponent</c>, parented through <c>Controls.Add</c> with
/// <c>SuspendLayout</c>/<c>ResumeLayout</c> discipline, <c>TabStop</c>/<c>TabIndex</c>
/// in visual order, Dock/Anchor-only layout, and <c>AccessibleName</c> on data surfaces.
///
/// Regeneration protocol (safe <c>.Designer.cs</c> cycle — SS-10, proven on
/// <c>JournalStudioForm</c> in C-24):
///  1. Event bindings live in the companion file's <c>WireEvents()</c>, NOT here, so
///     deleting and re-emitting this partial never orphans a handler;
///  2. behaviour overrides stay in the companion file;
///  3. adding a control means: declare the field, create + configure here, wire behaviour
///     in the companion — the three-step order keeps the designer round-trip stable;
///  4. the form carries a <c>.resx</c> but no resources are drawn from it: every property
///     is a literal (or a <c>const</c>) the designer can set.
///
/// Migrated-surface notes:
///  * the legacy constructor set window <c>Size</c> (1220 × 820); the designer equivalent
///    is <c>ClientSize</c> (1204 × 773) with the same <c>MinimumSize</c>;
///  * <c>_refreshTimer</c> is deliberately NOT a design-time component: it is a runtime
///    artefact owned by the companion partial (created, started and stopped there);
///  * network-map ATM cards are runtime content (one <c>ATMCardPanel</c> per connection,
///    rebuilt on every refresh) and are never part of this tree;
///  * <c>_listenPort</c> is seeded from the const <see cref="NetworkConfig.DEFAULT_PORT"/>
///    and the Settings paths from <see cref="ATMPaths"/> / <see cref="AppConstants"/>,
///    exactly as the legacy builder did.
/// Control → function mapping is documented on the companion type and in
/// <c>docs/inventory/UI-SURFACES.md</c> (generated).
/// </summary>
public sealed partial class ServerMainForm
{
    // ── menu ─────────────────────────────────────────────────────────────────
    private MenuStrip _mainMenu = null!;
    private ToolStripMenuItem _fileMenu = null!;
    private ToolStripMenuItem _startServerMenuItem = null!;
    private ToolStripMenuItem _stopServerMenuItem = null!;
    private ToolStripMenuItem _exitMenuItem = null!;
    private ToolStripMenuItem _operationsMenu = null!;
    private ToolStripMenuItem _refreshFleetMenuItem = null!;
    private ToolStripMenuItem _refreshOpsAnalyticsMenuItem = null!;
    private ToolStripMenuItem _refreshTelemetryMenuItem = null!;
    private ToolStripMenuItem _dailyReportMenuItem = null!;
    private ToolStripMenuItem _nocConsoleMenuItem = null!;
    private ToolStripMenuItem _adminMenu = null!;
    private ToolStripMenuItem _pingMenuItem = null!;
    private ToolStripMenuItem _forceSyncMenuItem = null!;
    private ToolStripMenuItem _probeMenuItem = null!;
    private ToolStripMenuItem _logsMenu = null!;
    private ToolStripMenuItem _exportLogMenuItem = null!;
    private ToolStripMenuItem _clearLogMenuItem = null!;
    private ToolStripMenuItem _openReportsMenuItem = null!;
    private ToolStripMenuItem _openArchiveMenuItem = null!;

    // ── tab container ────────────────────────────────────────────────────────
    private TabControl _tabs = null!;

    // ── Fleet tab ────────────────────────────────────────────────────────────
    private TabPage _fleetTab = null!;
    private Panel _fleetRoot = null!;
    private DataGridView _fleetGrid = null!;
    private TableLayoutPanel _fleetSummaryRow = null!;
    private Panel _totalAtmsCard = null!;
    private Label _totalAtmsCardCaption = null!;
    private Label _totalAtmsValue = null!;
    private Panel _connectedAtmsCard = null!;
    private Label _connectedAtmsCardCaption = null!;
    private Label _connectedAtmsValue = null!;
    private Panel _syncingAtmsCard = null!;
    private Label _syncingAtmsCardCaption = null!;
    private Label _syncingAtmsValue = null!;
    private Panel _offlineAtmsCard = null!;
    private Label _offlineAtmsCardCaption = null!;
    private Label _offlineAtmsValue = null!;
    private Panel _fleetHealthCard = null!;
    private Label _fleetHealthCardCaption = null!;
    private Label _fleetHealthValue = null!;
    private FlowLayoutPanel _fleetActions = null!;
    private Button _fleetRefreshButton = null!;
    private Button _fleetDetailsButton = null!;
    private Button _fleetDrawerButton = null!;
    private Button _fleetBroadcastButton = null!;
    private Button _fleetStartButton = null!;
    private Button _fleetStopButton = null!;

    // ── Network Map tab ──────────────────────────────────────────────────────
    private TabPage _mapTab = null!;
    private Panel _mapRoot = null!;
    private FlowLayoutPanel _networkMap = null!;
    private Label _mapLegend = null!;
    private FlowLayoutPanel _mapActions = null!;
    private Button _mapRefreshButton = null!;
    private Button _mapOpenButton = null!;
    private Button _mapBroadcastButton = null!;

    // ── NOC Monitoring tab (Wave 6 / C-34 — unified from EJLive.Monitoring.WinForms) ──
    // The tab is a host, not a copy: _nocHost is the empty Panel the companion
    // partial parents the single MonitoringConsoleForm instance into (runtime
    // content, same rule as the network-map ATM cards — a designer partial never
    // instantiates a sibling surface). _nocActions carries the two host-level
    // commands; every monitoring control lives in MonitoringConsoleForm.Designer.cs.
    private TabPage _nocTab = null!;
    private Panel _nocRoot = null!;
    private FlowLayoutPanel _nocActions = null!;
    private Button _nocRefreshButton = null!;
    private Button _nocDetachButton = null!;
    private Panel _nocHost = null!;

    // ── Journal Viewer tab ───────────────────────────────────────────────────
    private TabPage _journalTab = null!;
    private Panel _journalRoot = null!;
    private RichTextBox _log = null!;
    private FlowLayoutPanel _journalActions = null!;
    private Button _journalStudioButton = null!;
    private Button _journalTodayButton = null!;
    private Button _journalArchiveButton = null!;
    private Button _journalOpenArchiveButton = null!;
    private Button _journalOpenSmartButton = null!;

    // ── Sync Dashboard tab ───────────────────────────────────────────────────
    private TabPage _syncTab = null!;
    private Panel _syncRoot = null!;
    private DataGridView _syncGrid = null!;
    private TableLayoutPanel _syncSummaryRow = null!;
    private Panel _syncOpenCard = null!;
    private Label _syncOpenCardCaption = null!;
    private Label _syncOpenValue = null!;
    private Panel _syncFailedCard = null!;
    private Label _syncFailedCardCaption = null!;
    private Label _syncFailedValue = null!;
    private Panel _syncCompletedCard = null!;
    private Label _syncCompletedCardCaption = null!;
    private Label _syncCompletedValue = null!;
    private Panel _syncProgressCard = null!;
    private Label _syncProgressCardCaption = null!;
    private Label _syncProgressValue = null!;
    private FlowLayoutPanel _syncActions = null!;
    private Button _syncOpenButton = null!;
    private Button _syncRetryButton = null!;
    private Button _syncVerifyButton = null!;

    // ── Delivery Tracker tab ─────────────────────────────────────────────────
    private TabPage _deliveryTab = null!;
    private Panel _deliveryRoot = null!;
    private DataGridView _deliveryGrid = null!;
    private FlowLayoutPanel _deliveryActions = null!;
    private Button _deliveryRefreshButton = null!;
    private Button _deliveryPendingButton = null!;
    private Button _deliveryFailedButton = null!;
    private Button _deliveryOpenSmartButton = null!;

    // ── Remote Commands tab ──────────────────────────────────────────────────
    private TabPage _remoteTab = null!;
    private Panel _remoteRoot = null!;
    private DataGridView _commandGrid = null!;
    private Panel _remotePreviewPanel = null!;
    private PictureBox _remotePreview = null!;
    private Label _remotePreviewStatus = null!;
    private FlowLayoutPanel _remoteActions = null!;
    private Label _commandTargetLabel = null!;
    private ComboBox _commandTarget = null!;
    private Button _remoteRefreshTargetsButton = null!;
    private Button _remotePingButton = null!;
    private Button _remotePingTrackedButton = null!;
    private Button _remoteProbeButton = null!;
    private Button _remoteSyncTimeButton = null!;
    private Button _remoteScreenshotButton = null!;
    private Button _remoteSessionStartButton = null!;
    private Button _remoteSessionStopButton = null!;
    private Button _rdpStartButton = null!;
    private Button _rdpCheckButton = null!;
    private Button _rdpStopButton = null!;
    private Button _changePasswordButton = null!;
    private Button _changeWinPasswordButton = null!;
    private Button _requestJournalButton = null!;
    private Button _imageInboxButton = null!;
    private Button _imageDirectButton = null!;
    private Button _distInboxButton = null!;
    private Button _distDirectButton = null!;
    private Button _syncImagesButton = null!;
    private Button _forceSyncButton = null!;
    private Button _restartButton = null!;
    private Button _trackedRestartButton = null!;

    // ── Alerts tab ───────────────────────────────────────────────────────────
    private TabPage _alertsTab = null!;
    private Panel _alertsRoot = null!;
    private DataGridView _alertGrid = null!;
    private FlowLayoutPanel _alertsActions = null!;
    private Button _alertsTestButton = null!;
    private Button _alertsMarkReadButton = null!;
    private Button _alertsExportButton = null!;
    private Button _alertsOpenReportsButton = null!;

    // ── Archive tab ──────────────────────────────────────────────────────────
    private TabPage _archiveTab = null!;
    private Panel _archiveRoot = null!;
    private Label _archiveHint = null!;
    private FlowLayoutPanel _archiveActions = null!;
    private Button _archiveRunButton = null!;
    private Button _archiveEligibleButton = null!;
    private Button _archiveOpenButton = null!;
    private Button _archiveCleanupButton = null!;

    // ── Reports tab ──────────────────────────────────────────────────────────
    private TabPage _reportsTab = null!;
    private Panel _reportsRoot = null!;
    private Label _reportsHint = null!;
    private FlowLayoutPanel _reportsActions = null!;
    private Button _reportsShiftButton = null!;
    private Button _reportsDailyButton = null!;
    private Button _reportsWeeklyButton = null!;
    private Button _reportsFleetHealthButton = null!;
    private Button _reportsCombinedButton = null!;
    private Button _reportsOpenFolderButton = null!;

    // ── Ops Analytics tab ────────────────────────────────────────────────────
    private TabPage _opsAnalyticsTab = null!;
    private Panel _opsAnalyticsRoot = null!;
    private DataGridView _opsAnalyticsGrid = null!;
    private FlowLayoutPanel _opsAnalyticsActions = null!;
    private Button _opsAnalyticsRefreshButton = null!;
    private Button _opsAnalytics1hButton = null!;
    private Button _opsAnalytics24hButton = null!;
    private Button _opsAnalyticsExportButton = null!;
    private Button _opsAnalyticsOpenButton = null!;
    private Label _opsAnalyticsSummary = null!;

    // ── Command Audit tab ────────────────────────────────────────────────────
    private TabPage _commandAuditTab = null!;
    private Panel _commandAuditRoot = null!;
    private DataGridView _commandAuditGrid = null!;
    private FlowLayoutPanel _commandAuditActions = null!;
    private Label _commandAuditAtmLabel = null!;
    private ComboBox _commandAuditAtmFilter = null!;
    private Label _commandAuditScopeLabel = null!;
    private ComboBox _commandAuditScopeFilter = null!;
    private Button _commandAuditRefreshButton = null!;
    private Button _commandAudit1hButton = null!;
    private Button _commandAudit24hButton = null!;
    private Button _commandAuditExportButton = null!;
    private Button _commandAuditOpenButton = null!;
    private Label _commandAuditSummary = null!;

    // ── Telemetry tab ────────────────────────────────────────────────────────
    private TabPage _telemetryTab = null!;
    private Panel _telemetryRoot = null!;
    private SplitContainer _telemetrySplit = null!;
    private DataGridView _telemetryTimelineGrid = null!;
    private DataGridView _telemetryAtmGrid = null!;
    private FlowLayoutPanel _telemetryActions = null!;
    private Button _telemetryRefreshButton = null!;
    private Button _telemetry1hButton = null!;
    private Button _telemetry24hButton = null!;
    private Button _telemetryTimelineCsvButton = null!;
    private Button _telemetryAtmCsvButton = null!;
    private Button _telemetryOpenButton = null!;
    private Label _telemetrySummary = null!;

    // ── Settings tab ─────────────────────────────────────────────────────────
    private TabPage _settingsTab = null!;
    private Panel _settingsRoot = null!;
    private TableLayoutPanel _settingsPanel = null!;
    private Label _listenPortLabel = null!;
    private NumericUpDown _listenPort = null!;
    private Label _storagePathLabel = null!;
    private TextBox _storagePathText = null!;
    private Label _archivePathLabel = null!;
    private TextBox _archivePathText = null!;
    private Label _maxConnectionsLabel = null!;
    private NumericUpDown _maxConnectionsBox = null!;
    private FlowLayoutPanel _settingsActions = null!;
    private Button _settingsSaveButton = null!;
    private Button _settingsInitDbButton = null!;

    /// <summary>Required by the Windows Forms designer implementation.</summary>
    private System.ComponentModel.IContainer? components;

    /// <summary>
    /// Clean up any resources being used. Emitted in designer form so a future
    /// resx-backed round-trip needs no structural change.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>The method the designer emits: creation, property sets, parent links only.</summary>
    private void InitializeComponent()
    {
        SuspendLayout();

        // ══ form ════════════════════════════════════════════════════════════
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1204, 773);
        MinimumSize = new Size(1120, 740);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EJLive Enterprise Server";
        Font = new Font("Segoe UI", 9F);
        DoubleBuffered = true;

        // ══ menu ═════════════════════════════════════════════════════════════
        _mainMenu = new MenuStrip { Name = "mainMenu", Dock = DockStyle.Top, TabStop = false };

        _fileMenu = new ToolStripMenuItem { Name = "fileMenu", Text = "File" };
        _startServerMenuItem = new ToolStripMenuItem { Name = "startServerMenuItem", Text = "Start Server" };
        _stopServerMenuItem = new ToolStripMenuItem { Name = "stopServerMenuItem", Text = "Stop Server" };
        _exitMenuItem = new ToolStripMenuItem { Name = "exitMenuItem", Text = "Exit" };
        _fileMenu.DropDownItems.Add(_startServerMenuItem);
        _fileMenu.DropDownItems.Add(_stopServerMenuItem);
        _fileMenu.DropDownItems.Add(new ToolStripSeparator());
        _fileMenu.DropDownItems.Add(_exitMenuItem);

        _operationsMenu = new ToolStripMenuItem { Name = "operationsMenu", Text = "Operations" };
        _refreshFleetMenuItem = new ToolStripMenuItem { Name = "refreshFleetMenuItem", Text = "Refresh Fleet" };
        _refreshOpsAnalyticsMenuItem = new ToolStripMenuItem { Name = "refreshOpsAnalyticsMenuItem", Text = "Refresh Ops Analytics (24h)" };
        _refreshTelemetryMenuItem = new ToolStripMenuItem { Name = "refreshTelemetryMenuItem", Text = "Refresh Telemetry (24h)" };
        _dailyReportMenuItem = new ToolStripMenuItem { Name = "dailyReportMenuItem", Text = "Daily Ops Report" };
        _nocConsoleMenuItem = new ToolStripMenuItem { Name = "nocConsoleMenuItem", Text = "NOC Monitoring Console" };
        _operationsMenu.DropDownItems.Add(_refreshFleetMenuItem);
        _operationsMenu.DropDownItems.Add(_refreshOpsAnalyticsMenuItem);
        _operationsMenu.DropDownItems.Add(_refreshTelemetryMenuItem);
        _operationsMenu.DropDownItems.Add(_dailyReportMenuItem);
        _operationsMenu.DropDownItems.Add(_nocConsoleMenuItem);

        _adminMenu = new ToolStripMenuItem { Name = "adminMenu", Text = "Admin" };
        _pingMenuItem = new ToolStripMenuItem { Name = "pingMenuItem", Text = "Ping Selected ATM" };
        _forceSyncMenuItem = new ToolStripMenuItem { Name = "forceSyncMenuItem", Text = "Force Sync Selected ATM" };
        _probeMenuItem = new ToolStripMenuItem { Name = "probeMenuItem", Text = "Queue Connectivity Probe" };
        _adminMenu.DropDownItems.Add(_pingMenuItem);
        _adminMenu.DropDownItems.Add(_forceSyncMenuItem);
        _adminMenu.DropDownItems.Add(_probeMenuItem);

        _logsMenu = new ToolStripMenuItem { Name = "logsMenu", Text = "Logs" };
        _exportLogMenuItem = new ToolStripMenuItem { Name = "exportLogMenuItem", Text = "Export Runtime Log" };
        _clearLogMenuItem = new ToolStripMenuItem { Name = "clearLogMenuItem", Text = "Clear Runtime Log" };
        _openReportsMenuItem = new ToolStripMenuItem { Name = "openReportsMenuItem", Text = "Open Reports Folder" };
        _openArchiveMenuItem = new ToolStripMenuItem { Name = "openArchiveMenuItem", Text = "Open Archive Folder" };
        _logsMenu.DropDownItems.Add(_exportLogMenuItem);
        _logsMenu.DropDownItems.Add(_clearLogMenuItem);
        _logsMenu.DropDownItems.Add(new ToolStripSeparator());
        _logsMenu.DropDownItems.Add(_openReportsMenuItem);
        _logsMenu.DropDownItems.Add(_openArchiveMenuItem);

        _mainMenu.Items.Add(_fileMenu);
        _mainMenu.Items.Add(_operationsMenu);
        _mainMenu.Items.Add(_adminMenu);
        _mainMenu.Items.Add(_logsMenu);
        MainMenuStrip = _mainMenu;

        // ══ tab container ════════════════════════════════════════════════════
        _tabs = new TabControl { Name = "tabs", Dock = DockStyle.Fill, TabIndex = 0 };

        // ══ Fleet tab ════════════════════════════════════════════════════════
        _fleetTab = new TabPage { Name = "fleetTab", Text = "Fleet" };
        _fleetRoot = new Panel { Name = "fleetRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _fleetGrid = new DataGridView
        {
            Name = "fleetGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 247, 250), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(6, 4, 6, 4) },
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(250, 251, 253) },
            DefaultCellStyle = { SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) },
            AccessibleName = "Fleet grid",
            TabIndex = 1
        };
        _fleetGrid.Columns.Add("ATM_ID", "ATM Id");
        _fleetGrid.Columns.Add("Name", "Name");
        _fleetGrid.Columns.Add("Type", "Type");
        _fleetGrid.Columns.Add("Status", "Status");
        _fleetGrid.Columns.Add("Health", "Health");

        _fleetSummaryRow = new TableLayoutPanel { Name = "fleetSummaryRow", Dock = DockStyle.Top, Height = 92, ColumnCount = 5, Padding = new Padding(4), TabStop = false };
        for (var i = 0; i < 5; i++)
            _fleetSummaryRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));

        _totalAtmsCard = new Panel { Name = "totalAtmsCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _totalAtmsCardCaption = new Label { Name = "totalAtmsCardCaption", Text = "Total ATMs", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _totalAtmsValue = new Label { Name = "totalAtmsValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Total ATM count", TabStop = false };
        _totalAtmsCard.Controls.Add(_totalAtmsValue);
        _totalAtmsCard.Controls.Add(_totalAtmsCardCaption);
        _totalAtmsCard.Controls.Add(new Panel { Name = "totalAtmsCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(46, 134, 222), TabStop = false });

        _connectedAtmsCard = new Panel { Name = "connectedAtmsCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _connectedAtmsCardCaption = new Label { Name = "connectedAtmsCardCaption", Text = "Connected", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _connectedAtmsValue = new Label { Name = "connectedAtmsValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Connected ATM count", TabStop = false };
        _connectedAtmsCard.Controls.Add(_connectedAtmsValue);
        _connectedAtmsCard.Controls.Add(_connectedAtmsCardCaption);
        _connectedAtmsCard.Controls.Add(new Panel { Name = "connectedAtmsCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(16, 172, 132), TabStop = false });

        _syncingAtmsCard = new Panel { Name = "syncingAtmsCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _syncingAtmsCardCaption = new Label { Name = "syncingAtmsCardCaption", Text = "Syncing", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _syncingAtmsValue = new Label { Name = "syncingAtmsValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Syncing ATM count", TabStop = false };
        _syncingAtmsCard.Controls.Add(_syncingAtmsValue);
        _syncingAtmsCard.Controls.Add(_syncingAtmsCardCaption);
        _syncingAtmsCard.Controls.Add(new Panel { Name = "syncingAtmsCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(255, 159, 67), TabStop = false });

        _offlineAtmsCard = new Panel { Name = "offlineAtmsCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _offlineAtmsCardCaption = new Label { Name = "offlineAtmsCardCaption", Text = "Offline", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _offlineAtmsValue = new Label { Name = "offlineAtmsValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Offline ATM count", TabStop = false };
        _offlineAtmsCard.Controls.Add(_offlineAtmsValue);
        _offlineAtmsCard.Controls.Add(_offlineAtmsCardCaption);
        _offlineAtmsCard.Controls.Add(new Panel { Name = "offlineAtmsCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(238, 82, 83), TabStop = false });

        _fleetHealthCard = new Panel { Name = "fleetHealthCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _fleetHealthCardCaption = new Label { Name = "fleetHealthCardCaption", Text = "Avg Health", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _fleetHealthValue = new Label { Name = "fleetHealthValue", Text = "0%", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Average fleet health", TabStop = false };
        _fleetHealthCard.Controls.Add(_fleetHealthValue);
        _fleetHealthCard.Controls.Add(_fleetHealthCardCaption);
        _fleetHealthCard.Controls.Add(new Panel { Name = "fleetHealthCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(95, 39, 205), TabStop = false });

        _fleetSummaryRow.Controls.Add(_totalAtmsCard);
        _fleetSummaryRow.Controls.Add(_connectedAtmsCard);
        _fleetSummaryRow.Controls.Add(_syncingAtmsCard);
        _fleetSummaryRow.Controls.Add(_offlineAtmsCard);
        _fleetSummaryRow.Controls.Add(_fleetHealthCard);

        _fleetActions = new FlowLayoutPanel { Name = "fleetActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _fleetRefreshButton = new Button { Name = "fleetRefreshButton", Text = "Refresh Fleet", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 2 };
        _fleetDetailsButton = new Button { Name = "fleetDetailsButton", Text = "Open ATM Details", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 3 };
        _fleetDrawerButton = new Button { Name = "fleetDrawerButton", Text = "Open Detail Drawer", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 4 };
        _fleetBroadcastButton = new Button { Name = "fleetBroadcastButton", Text = "Broadcast Message", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 5 };
        _fleetStartButton = new Button { Name = "fleetStartButton", Text = "Start Server", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 6 };
        _fleetStopButton = new Button { Name = "fleetStopButton", Text = "Stop Server", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 7 };
        _fleetActions.Controls.Add(_fleetRefreshButton);
        _fleetActions.Controls.Add(_fleetDetailsButton);
        _fleetActions.Controls.Add(_fleetDrawerButton);
        _fleetActions.Controls.Add(_fleetBroadcastButton);
        _fleetActions.Controls.Add(_fleetStartButton);
        _fleetActions.Controls.Add(_fleetStopButton);

        _fleetRoot.Controls.Add(_fleetGrid);
        _fleetRoot.Controls.Add(_fleetSummaryRow);
        _fleetRoot.Controls.Add(_fleetActions);
        _fleetTab.Controls.Add(_fleetRoot);

        // ══ Network Map tab ══════════════════════════════════════════════════
        _mapTab = new TabPage { Name = "mapTab", Text = "Network Map" };
        _mapRoot = new Panel { Name = "mapRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _networkMap = new FlowLayoutPanel
        {
            Name = "networkMap",
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(246, 248, 250),
            AccessibleName = "Network map card wall (cards are runtime content)",
            TabStop = false
        };
        _mapLegend = new Label
        {
            Name = "mapLegend",
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Green: active | Yellow: idle | Blue: syncing/waiting | Orange: supervisor | Red: recently offline | Gray: critical offline",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(241, 245, 249),
            TabStop = false
        };
        _mapActions = new FlowLayoutPanel { Name = "mapActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _mapRefreshButton = new Button { Name = "mapRefreshButton", Text = "Refresh Map", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 8 };
        _mapOpenButton = new Button { Name = "mapOpenButton", Text = "Open Selected ATM", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 9 };
        _mapBroadcastButton = new Button { Name = "mapBroadcastButton", Text = "Broadcast Status Check", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 10 };
        _mapActions.Controls.Add(_mapRefreshButton);
        _mapActions.Controls.Add(_mapOpenButton);
        _mapActions.Controls.Add(_mapBroadcastButton);

        _mapRoot.Controls.Add(_networkMap);
        _mapRoot.Controls.Add(_mapLegend);
        _mapRoot.Controls.Add(_mapActions);
        _mapTab.Controls.Add(_mapRoot);

        // ══ NOC Monitoring tab (Wave 6 / C-34) ════════════════════════════════
        // Unified host for the surface that used to be EJLive.Monitoring.exe: the
        // console itself is created and parented by the companion partial
        // (EnsureNocConsole), so this block stays creation/property/parent only.
        _nocTab = new TabPage { Name = "nocTab", Text = "NOC Monitoring" };
        _nocRoot = new Panel { Name = "nocRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };
        _nocActions = new FlowLayoutPanel { Name = "nocActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _nocRefreshButton = new Button { Name = "nocRefreshButton", Text = "Refresh Console", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 10 };
        _nocDetachButton = new Button { Name = "nocDetachButton", Text = "Open Detached Window", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 11 };
        _nocHost = new Panel
        {
            Name = "nocHost",
            Dock = DockStyle.Fill,
            TabStop = false,
            AccessibleName = "NOC monitoring console host",
            BackColor = Color.FromArgb(245, 249, 252)
        };
        _nocActions.Controls.Add(_nocRefreshButton);
        _nocActions.Controls.Add(_nocDetachButton);

        _nocRoot.Controls.Add(_nocHost);
        _nocRoot.Controls.Add(_nocActions);
        _nocTab.Controls.Add(_nocRoot);

        // ══ Journal Viewer tab ═══════════════════════════════════════════════
        _journalTab = new TabPage { Name = "journalTab", Text = "Journal Studio (SS-10.5)" };
        _journalRoot = new Panel { Name = "journalRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _log = new RichTextBox
        {
            Name = "log",
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BackColor = Color.FromArgb(252, 252, 253),
            AccessibleName = "Server runtime log",
            TabIndex = 11
        };
        _journalActions = new FlowLayoutPanel { Name = "journalActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _journalStudioButton = new Button { Name = "journalStudioButton", Text = "Open Journal Studio", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 12 };
        _journalTodayButton = new Button { Name = "journalTodayButton", Text = "Load Today", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 13 };
        _journalArchiveButton = new Button { Name = "journalArchiveButton", Text = "Archive Eligible Journals", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 14 };
        _journalOpenArchiveButton = new Button { Name = "journalOpenArchiveButton", Text = "Open Archive Folder", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 15 };
        _journalOpenSmartButton = new Button { Name = "journalOpenSmartButton", Text = "Open Smart Storage", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 16 };
        _journalActions.Controls.Add(_journalStudioButton);
        _journalActions.Controls.Add(_journalTodayButton);
        _journalActions.Controls.Add(_journalArchiveButton);
        _journalActions.Controls.Add(_journalOpenArchiveButton);
        _journalActions.Controls.Add(_journalOpenSmartButton);

        _journalRoot.Controls.Add(_log);
        _journalRoot.Controls.Add(_journalActions);
        _journalTab.Controls.Add(_journalRoot);

        // ══ Sync Dashboard tab ═══════════════════════════════════════════════
        _syncTab = new TabPage { Name = "syncTab", Text = "Sync Dashboard" };
        _syncRoot = new Panel { Name = "syncRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _syncGrid = new DataGridView
        {
            Name = "syncGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 247, 250), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(6, 4, 6, 4) },
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(250, 251, 253) },
            DefaultCellStyle = { SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) },
            AccessibleName = "Journal sync grid",
            TabIndex = 17
        };
        _syncGrid.Columns.Add("SyncId", "Sync Id");
        _syncGrid.Columns.Add("ATM", "ATM");
        _syncGrid.Columns.Add("File", "File");
        _syncGrid.Columns.Add("State", "State");
        _syncGrid.Columns.Add("Progress", "Progress");

        _syncSummaryRow = new TableLayoutPanel { Name = "syncSummaryRow", Dock = DockStyle.Top, Height = 92, ColumnCount = 4, Padding = new Padding(4), TabStop = false };
        for (var i = 0; i < 4; i++)
            _syncSummaryRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

        _syncOpenCard = new Panel { Name = "syncOpenCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _syncOpenCardCaption = new Label { Name = "syncOpenCardCaption", Text = "Open Sync", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _syncOpenValue = new Label { Name = "syncOpenValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Open sync count", TabStop = false };
        _syncOpenCard.Controls.Add(_syncOpenValue);
        _syncOpenCard.Controls.Add(_syncOpenCardCaption);
        _syncOpenCard.Controls.Add(new Panel { Name = "syncOpenCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(46, 134, 222), TabStop = false });

        _syncFailedCard = new Panel { Name = "syncFailedCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _syncFailedCardCaption = new Label { Name = "syncFailedCardCaption", Text = "Failed Sync", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _syncFailedValue = new Label { Name = "syncFailedValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Failed sync count", TabStop = false };
        _syncFailedCard.Controls.Add(_syncFailedValue);
        _syncFailedCard.Controls.Add(_syncFailedCardCaption);
        _syncFailedCard.Controls.Add(new Panel { Name = "syncFailedCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(238, 82, 83), TabStop = false });

        _syncCompletedCard = new Panel { Name = "syncCompletedCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _syncCompletedCardCaption = new Label { Name = "syncCompletedCardCaption", Text = "Completed", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _syncCompletedValue = new Label { Name = "syncCompletedValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Completed sync count", TabStop = false };
        _syncCompletedCard.Controls.Add(_syncCompletedValue);
        _syncCompletedCard.Controls.Add(_syncCompletedCardCaption);
        _syncCompletedCard.Controls.Add(new Panel { Name = "syncCompletedCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(16, 172, 132), TabStop = false });

        _syncProgressCard = new Panel { Name = "syncProgressCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _syncProgressCardCaption = new Label { Name = "syncProgressCardCaption", Text = "Avg Progress", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _syncProgressValue = new Label { Name = "syncProgressValue", Text = "0%", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Average sync progress", TabStop = false };
        _syncProgressCard.Controls.Add(_syncProgressValue);
        _syncProgressCard.Controls.Add(_syncProgressCardCaption);
        _syncProgressCard.Controls.Add(new Panel { Name = "syncProgressCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(255, 159, 67), TabStop = false });

        _syncSummaryRow.Controls.Add(_syncOpenCard);
        _syncSummaryRow.Controls.Add(_syncFailedCard);
        _syncSummaryRow.Controls.Add(_syncCompletedCard);
        _syncSummaryRow.Controls.Add(_syncProgressCard);

        _syncActions = new FlowLayoutPanel { Name = "syncActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _syncOpenButton = new Button { Name = "syncOpenButton", Text = "Open Sync Dashboard", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 18 };
        _syncRetryButton = new Button { Name = "syncRetryButton", Text = "Retry Failed", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 19 };
        _syncVerifyButton = new Button { Name = "syncVerifyButton", Text = "Verify Checksums", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 20 };
        _syncActions.Controls.Add(_syncOpenButton);
        _syncActions.Controls.Add(_syncRetryButton);
        _syncActions.Controls.Add(_syncVerifyButton);

        _syncRoot.Controls.Add(_syncGrid);
        _syncRoot.Controls.Add(_syncSummaryRow);
        _syncRoot.Controls.Add(_syncActions);
        _syncTab.Controls.Add(_syncRoot);

        // ══ Delivery Tracker tab ═════════════════════════════════════════════
        _deliveryTab = new TabPage { Name = "deliveryTab", Text = "Delivery Tracker" };
        _deliveryRoot = new Panel { Name = "deliveryRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _deliveryGrid = new DataGridView
        {
            Name = "deliveryGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 247, 250), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(6, 4, 6, 4) },
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(250, 251, 253) },
            DefaultCellStyle = { SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) },
            AccessibleName = "File delivery tracker",
            TabIndex = 21
        };
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

        _deliveryActions = new FlowLayoutPanel { Name = "deliveryActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _deliveryRefreshButton = new Button { Name = "deliveryRefreshButton", Text = "Refresh", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 22 };
        _deliveryPendingButton = new Button { Name = "deliveryPendingButton", Text = "Pending Only", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 23 };
        _deliveryFailedButton = new Button { Name = "deliveryFailedButton", Text = "Failed Only", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 24 };
        _deliveryOpenSmartButton = new Button { Name = "deliveryOpenSmartButton", Text = "Open Smart Storage", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 25 };
        _deliveryActions.Controls.Add(_deliveryRefreshButton);
        _deliveryActions.Controls.Add(_deliveryPendingButton);
        _deliveryActions.Controls.Add(_deliveryFailedButton);
        _deliveryActions.Controls.Add(_deliveryOpenSmartButton);

        _deliveryRoot.Controls.Add(_deliveryGrid);
        _deliveryRoot.Controls.Add(_deliveryActions);
        _deliveryTab.Controls.Add(_deliveryRoot);

        // ══ Remote Commands tab ══════════════════════════════════════════════
        _remoteTab = new TabPage { Name = "remoteTab", Text = "Remote Commands" };
        _remoteRoot = new Panel { Name = "remoteRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _commandGrid = new DataGridView
        {
            Name = "commandGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 247, 250), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(6, 4, 6, 4) },
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(250, 251, 253) },
            DefaultCellStyle = { SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) },
            AccessibleName = "Remote command log",
            TabIndex = 26
        };
        _commandGrid.Columns.Add("Time", "Time");
        _commandGrid.Columns.Add("Target", "Target");
        _commandGrid.Columns.Add("Command", "Command");
        _commandGrid.Columns.Add("Status", "Status");
        _commandGrid.Columns.Add("CommandId", "Command Id");
        _commandGrid.Columns.Add("Result", "Result");

        _remotePreviewPanel = new Panel { Name = "remotePreviewPanel", Dock = DockStyle.Bottom, Height = 220, Padding = new Padding(0, 6, 0, 0), TabStop = false };
        _remotePreview = new PictureBox
        {
            Name = "remotePreview",
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Black,
            AccessibleName = "Remote screen preview"
        };
        _remotePreviewStatus = new Label
        {
            Name = "remotePreviewStatus",
            Dock = DockStyle.Top,
            Height = 26,
            Text = "Remote preview idle",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(241, 245, 249),
            TabStop = false
        };
        _remotePreviewPanel.Controls.Add(_remotePreview);
        _remotePreviewPanel.Controls.Add(_remotePreviewStatus);

        _remoteActions = new FlowLayoutPanel { Name = "remoteActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _commandTargetLabel = new Label { Name = "commandTargetLabel", Text = "Target", AutoSize = true, Padding = new Padding(4, 8, 0, 0), TabStop = false };
        _commandTarget = new ComboBox
        {
            Name = "commandTarget",
            Width = 220,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(4),
            AccessibleName = "Remote command target",
            TabIndex = 27
        };
        _remoteRefreshTargetsButton = new Button { Name = "remoteRefreshTargetsButton", Text = "Refresh Targets", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 28 };
        _remotePingButton = new Button { Name = "remotePingButton", Text = "Ping", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 29 };
        _remotePingTrackedButton = new Button { Name = "remotePingTrackedButton", Text = "Ping (Tracked)", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 30 };
        _remoteProbeButton = new Button { Name = "remoteProbeButton", Text = "Connectivity Probe", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 31 };
        _remoteSyncTimeButton = new Button { Name = "remoteSyncTimeButton", Text = "Sync Time", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 32 };
        _remoteScreenshotButton = new Button { Name = "remoteScreenshotButton", Text = "Capture Screen", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 33 };
        _remoteSessionStartButton = new Button { Name = "remoteSessionStartButton", Text = "Start Remote Session", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 34 };
        _remoteSessionStopButton = new Button { Name = "remoteSessionStopButton", Text = "Stop Remote Session", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 35 };
        _rdpStartButton = new Button { Name = "rdpStartButton", Text = "RDP Start", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 36 };
        _rdpCheckButton = new Button { Name = "rdpCheckButton", Text = "RDP Check", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 37 };
        _rdpStopButton = new Button { Name = "rdpStopButton", Text = "RDP Stop", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 38 };
        _changePasswordButton = new Button { Name = "changePasswordButton", Text = "Change Password", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 39 };
        _changeWinPasswordButton = new Button { Name = "changeWinPasswordButton", Text = "Change Win Password", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 40 };
        _requestJournalButton = new Button { Name = "requestJournalButton", Text = "Request Journal", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 41 };
        _imageInboxButton = new Button { Name = "imageInboxButton", Text = "Image -> Inbox", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 42 };
        _imageDirectButton = new Button { Name = "imageDirectButton", Text = "Image -> Direct", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 43 };
        _distInboxButton = new Button { Name = "distInboxButton", Text = "Dist Folder -> Inbox", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 44 };
        _distDirectButton = new Button { Name = "distDirectButton", Text = "Dist Folder -> Direct", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 45 };
        _syncImagesButton = new Button { Name = "syncImagesButton", Text = "Sync Images", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 46 };
        _forceSyncButton = new Button { Name = "forceSyncButton", Text = "Force Sync", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 47 };
        _restartButton = new Button { Name = "restartButton", Text = "Restart", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 48 };
        _trackedRestartButton = new Button { Name = "trackedRestartButton", Text = "Tracked Restart", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 49 };
        _remoteActions.Controls.Add(_commandTargetLabel);
        _remoteActions.Controls.Add(_commandTarget);
        _remoteActions.Controls.Add(_remoteRefreshTargetsButton);
        _remoteActions.Controls.Add(_remotePingButton);
        _remoteActions.Controls.Add(_remotePingTrackedButton);
        _remoteActions.Controls.Add(_remoteProbeButton);
        _remoteActions.Controls.Add(_remoteSyncTimeButton);
        _remoteActions.Controls.Add(_remoteScreenshotButton);
        _remoteActions.Controls.Add(_remoteSessionStartButton);
        _remoteActions.Controls.Add(_remoteSessionStopButton);
        _remoteActions.Controls.Add(_rdpStartButton);
        _remoteActions.Controls.Add(_rdpCheckButton);
        _remoteActions.Controls.Add(_rdpStopButton);
        _remoteActions.Controls.Add(_changePasswordButton);
        _remoteActions.Controls.Add(_changeWinPasswordButton);
        _remoteActions.Controls.Add(_requestJournalButton);
        _remoteActions.Controls.Add(_imageInboxButton);
        _remoteActions.Controls.Add(_imageDirectButton);
        _remoteActions.Controls.Add(_distInboxButton);
        _remoteActions.Controls.Add(_distDirectButton);
        _remoteActions.Controls.Add(_syncImagesButton);
        _remoteActions.Controls.Add(_forceSyncButton);
        _remoteActions.Controls.Add(_restartButton);
        _remoteActions.Controls.Add(_trackedRestartButton);

        _remoteRoot.Controls.Add(_commandGrid);
        _remoteRoot.Controls.Add(_remotePreviewPanel);
        _remoteRoot.Controls.Add(_remoteActions);
        _remoteTab.Controls.Add(_remoteRoot);

        // ══ Alerts tab ═══════════════════════════════════════════════════════
        _alertsTab = new TabPage { Name = "alertsTab", Text = "Alerts" };
        _alertsRoot = new Panel { Name = "alertsRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _alertGrid = new DataGridView
        {
            Name = "alertGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 247, 250), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(6, 4, 6, 4) },
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(250, 251, 253) },
            DefaultCellStyle = { SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) },
            AccessibleName = "Alert list",
            TabIndex = 50
        };
        _alertGrid.Columns.Add("Severity", "Severity");
        _alertGrid.Columns.Add("Category", "Category");
        _alertGrid.Columns.Add("Recommendation", "Recommended Action");
        _alertGrid.Columns.Add("Title", "Title");
        _alertGrid.Columns.Add("Message", "Message");
        _alertGrid.Columns.Add("Source", "Source");
        _alertGrid.Columns.Add("Created", "Created");

        _alertsActions = new FlowLayoutPanel { Name = "alertsActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _alertsTestButton = new Button { Name = "alertsTestButton", Text = "Raise Test Alert", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 51 };
        _alertsMarkReadButton = new Button { Name = "alertsMarkReadButton", Text = "Mark Read", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 52 };
        _alertsExportButton = new Button { Name = "alertsExportButton", Text = "Export Alerts", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 53 };
        _alertsOpenReportsButton = new Button { Name = "alertsOpenReportsButton", Text = "Open Reports Folder", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 54 };
        _alertsActions.Controls.Add(_alertsTestButton);
        _alertsActions.Controls.Add(_alertsMarkReadButton);
        _alertsActions.Controls.Add(_alertsExportButton);
        _alertsActions.Controls.Add(_alertsOpenReportsButton);

        _alertsRoot.Controls.Add(_alertGrid);
        _alertsRoot.Controls.Add(_alertsActions);
        _alertsTab.Controls.Add(_alertsRoot);

        // ══ Archive tab ══════════════════════════════════════════════════════
        _archiveTab = new TabPage { Name = "archiveTab", Text = "Archive" };
        _archiveRoot = new Panel { Name = "archiveRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _archiveHint = new Label
        {
            Name = "archiveHint",
            Dock = DockStyle.Fill,
            Text = "Archive manager keeps server journal copies, monthly folders, and reports.",
            TextAlign = ContentAlignment.MiddleCenter,
            TabStop = false
        };
        _archiveActions = new FlowLayoutPanel { Name = "archiveActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _archiveRunButton = new Button { Name = "archiveRunButton", Text = "Run Archive", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 55 };
        _archiveEligibleButton = new Button { Name = "archiveEligibleButton", Text = "Archive Eligible Journals", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 56 };
        _archiveOpenButton = new Button { Name = "archiveOpenButton", Text = "Open Archive Folder", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 57 };
        _archiveCleanupButton = new Button { Name = "archiveCleanupButton", Text = "Cleanup Report", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 58 };
        _archiveActions.Controls.Add(_archiveRunButton);
        _archiveActions.Controls.Add(_archiveEligibleButton);
        _archiveActions.Controls.Add(_archiveOpenButton);
        _archiveActions.Controls.Add(_archiveCleanupButton);

        _archiveRoot.Controls.Add(_archiveHint);
        _archiveRoot.Controls.Add(_archiveActions);
        _archiveTab.Controls.Add(_archiveRoot);

        // ══ Reports tab ══════════════════════════════════════════════════════
        _reportsTab = new TabPage { Name = "reportsTab", Text = "Reports" };
        _reportsRoot = new Panel { Name = "reportsRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _reportsHint = new Label
        {
            Name = "reportsHint",
            Dock = DockStyle.Fill,
            Text = "Reports combine journal sync, alerts, uptime, and transaction analysis.",
            TextAlign = ContentAlignment.MiddleCenter,
            TabStop = false
        };
        _reportsActions = new FlowLayoutPanel { Name = "reportsActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _reportsShiftButton = new Button { Name = "reportsShiftButton", Text = "Shift Ops Report", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 59 };
        _reportsDailyButton = new Button { Name = "reportsDailyButton", Text = "Daily Ops Report", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 60 };
        _reportsWeeklyButton = new Button { Name = "reportsWeeklyButton", Text = "Weekly Ops Report", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 61 };
        _reportsFleetHealthButton = new Button { Name = "reportsFleetHealthButton", Text = "Fleet Health Report", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 62 };
        _reportsCombinedButton = new Button { Name = "reportsCombinedButton", Text = "Combined Ops Report", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 63 };
        _reportsOpenFolderButton = new Button { Name = "reportsOpenFolderButton", Text = "Open Reports Folder", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 64 };
        _reportsActions.Controls.Add(_reportsShiftButton);
        _reportsActions.Controls.Add(_reportsDailyButton);
        _reportsActions.Controls.Add(_reportsWeeklyButton);
        _reportsActions.Controls.Add(_reportsFleetHealthButton);
        _reportsActions.Controls.Add(_reportsCombinedButton);
        _reportsActions.Controls.Add(_reportsOpenFolderButton);

        _reportsRoot.Controls.Add(_reportsHint);
        _reportsRoot.Controls.Add(_reportsActions);
        _reportsTab.Controls.Add(_reportsRoot);

        // ══ Ops Analytics tab ════════════════════════════════════════════════
        _opsAnalyticsTab = new TabPage { Name = "opsAnalyticsTab", Text = "Ops Analytics" };
        _opsAnalyticsRoot = new Panel { Name = "opsAnalyticsRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _opsAnalyticsGrid = new DataGridView
        {
            Name = "opsAnalyticsGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 247, 250), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(6, 4, 6, 4) },
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(250, 251, 253) },
            DefaultCellStyle = { SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) },
            AccessibleName = "Per-ATM operational analytics",
            TabIndex = 65
        };
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

        _opsAnalyticsActions = new FlowLayoutPanel { Name = "opsAnalyticsActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _opsAnalyticsRefreshButton = new Button { Name = "opsAnalyticsRefreshButton", Text = "Refresh", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 66 };
        _opsAnalytics1hButton = new Button { Name = "opsAnalytics1hButton", Text = "Last 1h", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 67 };
        _opsAnalytics24hButton = new Button { Name = "opsAnalytics24hButton", Text = "Last 24h", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 68 };
        _opsAnalyticsExportButton = new Button { Name = "opsAnalyticsExportButton", Text = "Export JSON", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 69 };
        _opsAnalyticsOpenButton = new Button { Name = "opsAnalyticsOpenButton", Text = "Open Reports Folder", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 70 };
        _opsAnalyticsSummary = new Label { Name = "opsAnalyticsSummary", AutoSize = true, Padding = new Padding(8, 8, 0, 0), ForeColor = Color.FromArgb(71, 85, 105), TabStop = false };
        _opsAnalyticsActions.Controls.Add(_opsAnalyticsRefreshButton);
        _opsAnalyticsActions.Controls.Add(_opsAnalytics1hButton);
        _opsAnalyticsActions.Controls.Add(_opsAnalytics24hButton);
        _opsAnalyticsActions.Controls.Add(_opsAnalyticsExportButton);
        _opsAnalyticsActions.Controls.Add(_opsAnalyticsOpenButton);
        _opsAnalyticsActions.Controls.Add(_opsAnalyticsSummary);

        _opsAnalyticsRoot.Controls.Add(_opsAnalyticsGrid);
        _opsAnalyticsRoot.Controls.Add(_opsAnalyticsActions);
        _opsAnalyticsTab.Controls.Add(_opsAnalyticsRoot);

        // ══ Command Audit tab ════════════════════════════════════════════════
        _commandAuditTab = new TabPage { Name = "commandAuditTab", Text = "Command Audit" };
        _commandAuditRoot = new Panel { Name = "commandAuditRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _commandAuditGrid = new DataGridView
        {
            Name = "commandAuditGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 247, 250), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(6, 4, 6, 4) },
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(250, 251, 253) },
            DefaultCellStyle = { SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) },
            AccessibleName = "SS9 command audit trail",
            TabIndex = 71
        };
        _commandAuditGrid.Columns.Add("Time", "Time");
        _commandAuditGrid.Columns.Add("ATM", "ATM");
        _commandAuditGrid.Columns.Add("Action", "Action");
        _commandAuditGrid.Columns.Add("By", "By");
        _commandAuditGrid.Columns.Add("Detail", "Detail");

        _commandAuditActions = new FlowLayoutPanel { Name = "commandAuditActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _commandAuditAtmLabel = new Label { Name = "commandAuditAtmLabel", Text = "ATM", AutoSize = true, Padding = new Padding(4, 8, 0, 0), TabStop = false };
        _commandAuditAtmFilter = new ComboBox
        {
            Name = "commandAuditAtmFilter",
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(4),
            AccessibleName = "Command audit ATM filter",
            TabIndex = 72
        };
        _commandAuditScopeLabel = new Label { Name = "commandAuditScopeLabel", Text = "Scope", AutoSize = true, Padding = new Padding(4, 8, 0, 0), TabStop = false };
        _commandAuditScopeFilter = new ComboBox
        {
            Name = "commandAuditScopeFilter",
            Width = 170,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(4),
            AccessibleName = "Command audit scope filter",
            TabIndex = 73
        };
        _commandAuditScopeFilter.Items.AddRange(new object[]
        {
            "All Command Events",
            "Dispatch Only",
            "Results Only",
            "Failures Only"
        });
        _commandAuditScopeFilter.SelectedIndex = 0;
        _commandAuditRefreshButton = new Button { Name = "commandAuditRefreshButton", Text = "Refresh", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 74 };
        _commandAudit1hButton = new Button { Name = "commandAudit1hButton", Text = "Last 1h", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 75 };
        _commandAudit24hButton = new Button { Name = "commandAudit24hButton", Text = "Last 24h", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 76 };
        _commandAuditExportButton = new Button { Name = "commandAuditExportButton", Text = "Export CSV", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 77 };
        _commandAuditOpenButton = new Button { Name = "commandAuditOpenButton", Text = "Open Reports Folder", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 78 };
        _commandAuditSummary = new Label { Name = "commandAuditSummary", AutoSize = true, Padding = new Padding(8, 8, 0, 0), ForeColor = Color.FromArgb(71, 85, 105), TabStop = false };
        _commandAuditActions.Controls.Add(_commandAuditAtmLabel);
        _commandAuditActions.Controls.Add(_commandAuditAtmFilter);
        _commandAuditActions.Controls.Add(_commandAuditScopeLabel);
        _commandAuditActions.Controls.Add(_commandAuditScopeFilter);
        _commandAuditActions.Controls.Add(_commandAuditRefreshButton);
        _commandAuditActions.Controls.Add(_commandAudit1hButton);
        _commandAuditActions.Controls.Add(_commandAudit24hButton);
        _commandAuditActions.Controls.Add(_commandAuditExportButton);
        _commandAuditActions.Controls.Add(_commandAuditOpenButton);
        _commandAuditActions.Controls.Add(_commandAuditSummary);

        _commandAuditRoot.Controls.Add(_commandAuditGrid);
        _commandAuditRoot.Controls.Add(_commandAuditActions);
        _commandAuditTab.Controls.Add(_commandAuditRoot);

        // ══ Telemetry tab ════════════════════════════════════════════════════
        _telemetryTab = new TabPage { Name = "telemetryTab", Text = "Telemetry" };
        _telemetryRoot = new Panel { Name = "telemetryRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _telemetrySplit = new SplitContainer
        {
            Name = "telemetrySplit",
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 280
        };
        _telemetryTimelineGrid = new DataGridView
        {
            Name = "telemetryTimelineGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 247, 250), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(6, 4, 6, 4) },
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(250, 251, 253) },
            DefaultCellStyle = { SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) },
            AccessibleName = "Client telemetry timeline",
            TabIndex = 79
        };
        _telemetryTimelineGrid.Columns.Add("Time", "Time");
        _telemetryTimelineGrid.Columns.Add("ATM", "ATM");
        _telemetryTimelineGrid.Columns.Add("Severity", "Severity");
        _telemetryTimelineGrid.Columns.Add("Type", "Type");
        _telemetryTimelineGrid.Columns.Add("Detail", "Detail");
        _telemetryAtmGrid = new DataGridView
        {
            Name = "telemetryAtmGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            EnableHeadersVisualStyles = false,
            RowHeadersVisible = false,
            ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 247, 250), Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(31, 41, 55), Padding = new Padding(6, 4, 6, 4) },
            AlternatingRowsDefaultCellStyle = { BackColor = Color.FromArgb(250, 251, 253) },
            DefaultCellStyle = { SelectionBackColor = Color.FromArgb(219, 234, 254), SelectionForeColor = Color.FromArgb(31, 41, 55) },
            AccessibleName = "Per-ATM telemetry summary",
            TabIndex = 80
        };
        _telemetryAtmGrid.Columns.Add("ATM", "ATM");
        _telemetryAtmGrid.Columns.Add("Total", "Total");
        _telemetryAtmGrid.Columns.Add("Warnings", "Warnings");
        _telemetryAtmGrid.Columns.Add("Errors", "Errors");
        _telemetryAtmGrid.Columns.Add("LastType", "Last Event Type");
        _telemetryAtmGrid.Columns.Add("LastAt", "Last Event Time");
        _telemetrySplit.Panel1.Controls.Add(_telemetryTimelineGrid);
        _telemetrySplit.Panel2.Controls.Add(_telemetryAtmGrid);

        _telemetryActions = new FlowLayoutPanel { Name = "telemetryActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _telemetryRefreshButton = new Button { Name = "telemetryRefreshButton", Text = "Refresh", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 81 };
        _telemetry1hButton = new Button { Name = "telemetry1hButton", Text = "Last 1h", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 82 };
        _telemetry24hButton = new Button { Name = "telemetry24hButton", Text = "Last 24h", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 83 };
        _telemetryTimelineCsvButton = new Button { Name = "telemetryTimelineCsvButton", Text = "Export Timeline CSV", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 84 };
        _telemetryAtmCsvButton = new Button { Name = "telemetryAtmCsvButton", Text = "Export ATM Summary CSV", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 85 };
        _telemetryOpenButton = new Button { Name = "telemetryOpenButton", Text = "Open Reports Folder", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 86 };
        _telemetrySummary = new Label { Name = "telemetrySummary", AutoSize = true, Padding = new Padding(8, 8, 0, 0), ForeColor = Color.FromArgb(71, 85, 105), TabStop = false };
        _telemetryActions.Controls.Add(_telemetryRefreshButton);
        _telemetryActions.Controls.Add(_telemetry1hButton);
        _telemetryActions.Controls.Add(_telemetry24hButton);
        _telemetryActions.Controls.Add(_telemetryTimelineCsvButton);
        _telemetryActions.Controls.Add(_telemetryAtmCsvButton);
        _telemetryActions.Controls.Add(_telemetryOpenButton);
        _telemetryActions.Controls.Add(_telemetrySummary);

        _telemetryRoot.Controls.Add(_telemetrySplit);
        _telemetryRoot.Controls.Add(_telemetryActions);
        _telemetryTab.Controls.Add(_telemetryRoot);

        // ══ Settings tab ═════════════════════════════════════════════════════
        _settingsTab = new TabPage { Name = "settingsTab", Text = "Settings" };
        _settingsRoot = new Panel { Name = "settingsRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _settingsPanel = new TableLayoutPanel
        {
            Name = "settingsPanel",
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(16),
            TabStop = false
        };
        _settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        _settingsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        for (var i = 0; i < 4; i++)
            _settingsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _listenPortLabel = new Label { Name = "listenPortLabel", Text = "Listen Port", AutoSize = true, Padding = new Padding(0, 6, 0, 0), TabStop = false };
        _listenPort = new NumericUpDown { Name = "listenPort", Minimum = 1, Maximum = 65535, Value = NetworkConfig.DEFAULT_PORT, AccessibleName = "TCP listen port", TabIndex = 87 };
        _storagePathLabel = new Label { Name = "storagePathLabel", Text = "Storage Path", AutoSize = true, Padding = new Padding(0, 6, 0, 0), TabStop = false };
        _storagePathText = new TextBox { Name = "storagePathText", Text = ATMPaths.SERVER_DEFAULT_DRIVE + @"\EJOURNAL_FILES", Dock = DockStyle.Fill, TabIndex = 88 };
        _archivePathLabel = new Label { Name = "archivePathLabel", Text = "Archive Path", AutoSize = true, Padding = new Padding(0, 6, 0, 0), TabStop = false };
        _archivePathText = new TextBox { Name = "archivePathText", Text = AppConstants.DefaultArchivePath, Dock = DockStyle.Fill, TabIndex = 89 };
        _maxConnectionsLabel = new Label { Name = "maxConnectionsLabel", Text = "Max Connections", AutoSize = true, Padding = new Padding(0, 6, 0, 0), TabStop = false };
        _maxConnectionsBox = new NumericUpDown { Name = "maxConnectionsBox", Minimum = 1, Maximum = 5000, Value = 100, AccessibleName = "Maximum concurrent client connections", TabIndex = 90 };

        _settingsPanel.Controls.Add(_listenPortLabel, 0, 0);
        _settingsPanel.Controls.Add(_listenPort, 1, 0);
        _settingsPanel.Controls.Add(_storagePathLabel, 0, 1);
        _settingsPanel.Controls.Add(_storagePathText, 1, 1);
        _settingsPanel.Controls.Add(_archivePathLabel, 0, 2);
        _settingsPanel.Controls.Add(_archivePathText, 1, 2);
        _settingsPanel.Controls.Add(_maxConnectionsLabel, 0, 3);
        _settingsPanel.Controls.Add(_maxConnectionsBox, 1, 3);

        _settingsActions = new FlowLayoutPanel { Name = "settingsActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _settingsSaveButton = new Button { Name = "settingsSaveButton", Text = "Save Server Settings", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 91 };
        _settingsInitDbButton = new Button { Name = "settingsInitDbButton", Text = "Initialize Database", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 92 };
        _settingsActions.Controls.Add(_settingsSaveButton);
        _settingsActions.Controls.Add(_settingsInitDbButton);

        _settingsRoot.Controls.Add(_settingsActions);
        _settingsRoot.Controls.Add(_settingsPanel);
        _settingsTab.Controls.Add(_settingsRoot);

        // ══ tab assembly ══════════════════════════════════════════════════════
        _tabs.TabPages.Add(_fleetTab);
        _tabs.TabPages.Add(_mapTab);
        _tabs.TabPages.Add(_nocTab);
        _tabs.TabPages.Add(_journalTab);
        _tabs.TabPages.Add(_syncTab);
        _tabs.TabPages.Add(_deliveryTab);
        _tabs.TabPages.Add(_remoteTab);
        _tabs.TabPages.Add(_alertsTab);
        _tabs.TabPages.Add(_archiveTab);
        _tabs.TabPages.Add(_reportsTab);
        _tabs.TabPages.Add(_opsAnalyticsTab);
        _tabs.TabPages.Add(_telemetryTab);
        _tabs.TabPages.Add(_commandAuditTab);
        _tabs.TabPages.Add(_settingsTab);

        // ══ z-order assembly (Dock rules: Fill first, top strip after) ═══════
        Controls.Add(_tabs);
        Controls.Add(_mainMenu);

        ResumeLayout(false);
    }
}
