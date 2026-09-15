using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Server.WinForms.Monitoring;

/// <summary>
/// Designer surface for <see cref="MonitoringConsoleForm"/> (SS-10 / C-29, NOC / Windows
/// Operations Console). Authored to the shape Visual Studio's WinForms designer emits
/// and consumes: every control is a named field, created and property-set only inside
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
///  4. the form carries a <c>.resx</c> but no resources are drawn from it: every
///     property is a literal the designer can set.
///
/// Migrated-surface notes:
///  * the legacy constructor set window <c>Size</c> (1180 × 780); the designer
///    equivalent is <c>ClientSize</c> (1164 × 733) with the same <c>MinimumSize</c>;
///  * no grid row is seeded here: the static Device-State / Realtime-Sync demo rows and
///    every live row are added by the companion's <c>PerformInitialRefresh</c>, because
///    two of them carry <c>DateTime.Now</c>-relative values a designer cannot express;
///  * the operational-map cards are runtime content (one <c>Panel</c> per ATM, rebuilt
///    on every refresh) and are therefore never part of this tree.
/// Control → function mapping is documented on the companion type and in
/// <c>docs/inventory/UI-SURFACES.md</c> (generated).
/// </summary>
public sealed partial class MonitoringConsoleForm
{
    // ── tab container ────────────────────────────────────────────────────────
    private TabControl _tabs = null!;

    // ── Overview tab ─────────────────────────────────────────────────────────
    private TabPage _overviewTab = null!;
    private Panel _overviewRoot = null!;
    private DataGridView _overviewGrid = null!;
    private TableLayoutPanel _overviewSummaryRow = null!;
    private Panel _totalCard = null!;
    private Label _totalCardCaption = null!;
    private Label _totalValue = null!;
    private Panel _onlineCard = null!;
    private Label _onlineCardCaption = null!;
    private Label _onlineValue = null!;
    private Panel _syncingCard = null!;
    private Label _syncingCardCaption = null!;
    private Label _syncingValue = null!;
    private Panel _offlineCard = null!;
    private Label _offlineCardCaption = null!;
    private Label _offlineValue = null!;
    private Panel _healthCard = null!;
    private Label _healthCardCaption = null!;
    private Label _healthValue = null!;
    private FlowLayoutPanel _overviewActions = null!;
    private Button _overviewRefreshButton = null!;
    private Button _overviewWindowButton = null!;
    private Button _overviewReviewButton = null!;

    // ── Cash Matrix tab ──────────────────────────────────────────────────────
    private TabPage _cashMatrixTab = null!;
    private Panel _cashMatrixRoot = null!;
    private DataGridView _cashMatrixGrid = null!;
    private FlowLayoutPanel _cashMatrixActions = null!;
    private Button _cashMatrixRefreshButton = null!;
    private Button _cashMatrixWindowButton = null!;

    // ── Terminal List tab ────────────────────────────────────────────────────
    private TabPage _terminalListTab = null!;
    private Panel _terminalListRoot = null!;
    private DataGridView _terminalListGrid = null!;
    private FlowLayoutPanel _terminalListActions = null!;
    private Button _terminalListRefreshButton = null!;
    private Button _terminalListWindowButton = null!;

    // ── Operational Map tab ──────────────────────────────────────────────────
    private TabPage _mapTab = null!;
    private Panel _mapRoot = null!;
    private FlowLayoutPanel _mapPanel = null!;
    private Label _mapLegend = null!;
    private FlowLayoutPanel _mapActions = null!;
    private Button _mapRefreshButton = null!;
    private Button _mapReviewButton = null!;

    // ── Device State tab ─────────────────────────────────────────────────────
    private TabPage _deviceStateTab = null!;
    private DataGridView _deviceStateGrid = null!;

    // ── Realtime Sync tab ────────────────────────────────────────────────────
    private TabPage _syncTab = null!;
    private DataGridView _syncStatusGrid = null!;

    // ── XFS Events tab ───────────────────────────────────────────────────────
    private TabPage _xfsTab = null!;
    private Panel _xfsRoot = null!;
    private DataGridView _xfsGrid = null!;
    private FlowLayoutPanel _xfsActions = null!;
    private Button _xfsNcrButton = null!;
    private Button _xfsGrgButton = null!;
    private Button _xfsWincorButton = null!;
    private Button _xfsHyosungButton = null!;
    private Button _xfsWindowButton = null!;
    private Button _xfsClearButton = null!;

    // ── Vendor Logs tab ──────────────────────────────────────────────────────
    private TabPage _vendorLogsTab = null!;
    private Panel _vendorLogsRoot = null!;
    private RichTextBox _vendorLog = null!;
    private FlowLayoutPanel _vendorLogsActions = null!;
    private Button _vendorAnalyzeButton = null!;
    private Button _vendorExtractButton = null!;
    private Button _vendorClearButton = null!;

    // ── Reports tab ──────────────────────────────────────────────────────────
    private TabPage _reportsTab = null!;
    private Panel _reportsRoot = null!;
    private SplitContainer _reportsSplit = null!;
    private DataGridView _reportsWindowGrid = null!;
    private DataGridView _reportsFilesGrid = null!;
    private FlowLayoutPanel _reportsActions = null!;
    private Button _reportsRefreshButton = null!;
    private Button _reportsBundleButton = null!;
    private Button _reportsWindowSummaryButton = null!;
    private Button _reportsFilesIndexButton = null!;
    private Label _reportsInfo = null!;

    // ── Smart Analysis tab (SS-27) ───────────────────────────────────────────
    private TabPage _smartTab = null!;
    private Panel _smartRoot = null!;
    private FlowLayoutPanel _smartActions = null!;
    private Button _smartAnalyzeButton = null!;
    private Button _smartReSortButton = null!;
    private Button _smartGroupButton = null!;
    private Button _smartSampleButton = null!;
    private Button _smartClearButton = null!;
    private TextBox _smartVendorBox = null!;
    private SplitContainer _smartSplit = null!;
    private RichTextBox _smartUploadBox = null!;
    private TableLayoutPanel _smartResults = null!;
    private DataGridView _smartFindingsGrid = null!;
    private SplitContainer _smartBottomSplit = null!;
    private DataGridView _smartCassetteGrid = null!;
    private DataGridView _smartHourlyGrid = null!;
    private TableLayoutPanel _smartSummaryRow = null!;
    private Panel _smartCriticalCard = null!;
    private Label _smartCriticalCardCaption = null!;
    private Label _smartCritical = null!;
    private Panel _smartWarningCard = null!;
    private Label _smartWarningCardCaption = null!;
    private Label _smartWarning = null!;
    private Panel _smartInfoCard = null!;
    private Label _smartInfoCardCaption = null!;
    private Label _smartInfo = null!;
    private Panel _smartSummaryCard = null!;
    private Label _smartSummaryCardCaption = null!;
    private Label _smartSummary = null!;

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

        // ══ form ═════════════════════════════════════════════════════════════
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1164, 733);
        MinimumSize = new Size(1060, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EJLive Monitoring Console";
        Font = new Font("Segoe UI", 9F);
        DoubleBuffered = true;

        // ══ tab container ════════════════════════════════════════════════════
        _tabs = new TabControl { Name = "tabs", Dock = DockStyle.Fill, TabIndex = 0 };

        // ══ Overview tab ═════════════════════════════════════════════════════
        _overviewTab = new TabPage { Name = "overviewTab", Text = "Overview" };
        _overviewRoot = new Panel { Name = "overviewRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _overviewGrid = new DataGridView
        {
            Name = "overviewGrid",
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
            AccessibleName = "Fleet overview grid",
            TabIndex = 1
        };
        _overviewGrid.Columns.Add("ATM", "ATM");
        _overviewGrid.Columns.Add("Status", "Status");
        _overviewGrid.Columns.Add("Health", "Health");
        _overviewGrid.Columns.Add("LastHeartbeat", "Last Heartbeat");

        _overviewSummaryRow = new TableLayoutPanel { Name = "overviewSummaryRow", Dock = DockStyle.Top, Height = 92, ColumnCount = 5, Padding = new Padding(4), TabStop = false };
        for (var i = 0; i < 5; i++)
            _overviewSummaryRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));

        _totalCard = new Panel { Name = "totalCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _totalCardCaption = new Label { Name = "totalCardCaption", Text = "Total ATMs", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _totalValue = new Label { Name = "totalValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Total ATM count", TabStop = false };
        _totalCard.Controls.Add(_totalValue);
        _totalCard.Controls.Add(_totalCardCaption);
        _totalCard.Controls.Add(new Panel { Name = "totalCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(46, 134, 222), TabStop = false });

        _onlineCard = new Panel { Name = "onlineCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _onlineCardCaption = new Label { Name = "onlineCardCaption", Text = "Online", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _onlineValue = new Label { Name = "onlineValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Online ATM count", TabStop = false };
        _onlineCard.Controls.Add(_onlineValue);
        _onlineCard.Controls.Add(_onlineCardCaption);
        _onlineCard.Controls.Add(new Panel { Name = "onlineCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(16, 172, 132), TabStop = false });

        _syncingCard = new Panel { Name = "syncingCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _syncingCardCaption = new Label { Name = "syncingCardCaption", Text = "Syncing", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _syncingValue = new Label { Name = "syncingValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Syncing ATM count", TabStop = false };
        _syncingCard.Controls.Add(_syncingValue);
        _syncingCard.Controls.Add(_syncingCardCaption);
        _syncingCard.Controls.Add(new Panel { Name = "syncingCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(255, 159, 67), TabStop = false });

        _offlineCard = new Panel { Name = "offlineCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _offlineCardCaption = new Label { Name = "offlineCardCaption", Text = "Offline", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _offlineValue = new Label { Name = "offlineValue", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Offline ATM count", TabStop = false };
        _offlineCard.Controls.Add(_offlineValue);
        _offlineCard.Controls.Add(_offlineCardCaption);
        _offlineCard.Controls.Add(new Panel { Name = "offlineCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(238, 82, 83), TabStop = false });

        _healthCard = new Panel { Name = "healthCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _healthCardCaption = new Label { Name = "healthCardCaption", Text = "Avg Health", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _healthValue = new Label { Name = "healthValue", Text = "0%", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Average fleet health", TabStop = false };
        _healthCard.Controls.Add(_healthValue);
        _healthCard.Controls.Add(_healthCardCaption);
        _healthCard.Controls.Add(new Panel { Name = "healthCardAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(95, 39, 205), TabStop = false });

        _overviewSummaryRow.Controls.Add(_totalCard);
        _overviewSummaryRow.Controls.Add(_onlineCard);
        _overviewSummaryRow.Controls.Add(_syncingCard);
        _overviewSummaryRow.Controls.Add(_offlineCard);
        _overviewSummaryRow.Controls.Add(_healthCard);

        _overviewActions = new FlowLayoutPanel { Name = "overviewActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _overviewRefreshButton = new Button { Name = "overviewRefreshButton", Text = "Refresh", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 2 };
        _overviewWindowButton = new Button { Name = "overviewWindowButton", Text = "Open Overview Window", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 3 };
        _overviewReviewButton = new Button { Name = "overviewReviewButton", Text = "Raise Health Review", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 4 };
        _overviewActions.Controls.Add(_overviewRefreshButton);
        _overviewActions.Controls.Add(_overviewWindowButton);
        _overviewActions.Controls.Add(_overviewReviewButton);

        _overviewRoot.Controls.Add(_overviewGrid);
        _overviewRoot.Controls.Add(_overviewSummaryRow);
        _overviewRoot.Controls.Add(_overviewActions);
        _overviewTab.Controls.Add(_overviewRoot);

        // ══ Cash Matrix tab ══════════════════════════════════════════════════
        _cashMatrixTab = new TabPage { Name = "cashMatrixTab", Text = "Cash Matrix" };
        _cashMatrixRoot = new Panel { Name = "cashMatrixRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _cashMatrixGrid = new DataGridView
        {
            Name = "cashMatrixGrid",
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
            AccessibleName = "Per-cassette cash matrix",
            TabIndex = 5
        };
        _cashMatrixGrid.Columns.Add("ATM", "ATM");
        _cashMatrixGrid.Columns.Add("Branch", "Branch");
        _cashMatrixGrid.Columns.Add("Region", "Region");
        _cashMatrixGrid.Columns.Add("Vendor", "Vendor");
        _cashMatrixGrid.Columns.Add("Source", "Source");
        _cashMatrixGrid.Columns.Add("Updated", "Updated");
        _cashMatrixGrid.Columns.Add("Cass1", "Cass1");
        _cashMatrixGrid.Columns.Add("Cass2", "Cass2");
        _cashMatrixGrid.Columns.Add("Cass3", "Cass3");
        _cashMatrixGrid.Columns.Add("Cass4", "Cass4");
        _cashMatrixGrid.Columns.Add("Remaining", "Remaining");
        _cashMatrixGrid.Columns.Add("Loaded", "Loaded");
        _cashMatrixGrid.Columns.Add("DispenseOut", "Dispense Out");
        _cashMatrixGrid.Columns.Add("Reject", "Reject");
        _cashMatrixGrid.Columns.Add("Retract", "Retract");
        _cashMatrixGrid.Columns.Add("Band", "Cash Band");

        _cashMatrixActions = new FlowLayoutPanel { Name = "cashMatrixActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _cashMatrixRefreshButton = new Button { Name = "cashMatrixRefreshButton", Text = "Refresh Matrix", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 6 };
        _cashMatrixWindowButton = new Button { Name = "cashMatrixWindowButton", Text = "Open Matrix Window", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 7 };
        _cashMatrixActions.Controls.Add(_cashMatrixRefreshButton);
        _cashMatrixActions.Controls.Add(_cashMatrixWindowButton);

        _cashMatrixRoot.Controls.Add(_cashMatrixGrid);
        _cashMatrixRoot.Controls.Add(_cashMatrixActions);
        _cashMatrixTab.Controls.Add(_cashMatrixRoot);

        // ══ Terminal List tab ════════════════════════════════════════════════
        _terminalListTab = new TabPage { Name = "terminalListTab", Text = "Terminal List" };
        _terminalListRoot = new Panel { Name = "terminalListRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _terminalListGrid = new DataGridView
        {
            Name = "terminalListGrid",
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
            AccessibleName = "Terminal list",
            TabIndex = 8
        };
        _terminalListGrid.Columns.Add("ATM", "ATM");
        _terminalListGrid.Columns.Add("Branch", "Branch");
        _terminalListGrid.Columns.Add("Region", "Region");
        _terminalListGrid.Columns.Add("Vendor", "Vendor");
        _terminalListGrid.Columns.Add("Network", "Network");
        _terminalListGrid.Columns.Add("Status", "Status");
        _terminalListGrid.Columns.Add("Connection", "Connection");
        _terminalListGrid.Columns.Add("Health", "Health");
        _terminalListGrid.Columns.Add("Supervisor", "Supervisor");
        _terminalListGrid.Columns.Add("Alerts", "Alerts");
        _terminalListGrid.Columns.Add("LastTx", "Last Tx");
        _terminalListGrid.Columns.Add("LastHeartbeat", "Last Heartbeat");
        _terminalListGrid.Columns.Add("LastSync", "Last Sync");
        _terminalListGrid.Columns.Add("Remaining", "Remaining");

        _terminalListActions = new FlowLayoutPanel { Name = "terminalListActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _terminalListRefreshButton = new Button { Name = "terminalListRefreshButton", Text = "Refresh List", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 9 };
        _terminalListWindowButton = new Button { Name = "terminalListWindowButton", Text = "Open List Window", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 10 };
        _terminalListActions.Controls.Add(_terminalListRefreshButton);
        _terminalListActions.Controls.Add(_terminalListWindowButton);

        _terminalListRoot.Controls.Add(_terminalListGrid);
        _terminalListRoot.Controls.Add(_terminalListActions);
        _terminalListTab.Controls.Add(_terminalListRoot);

        // ══ Operational Map tab ══════════════════════════════════════════════
        _mapTab = new TabPage { Name = "mapTab", Text = "Operational Map" };
        _mapRoot = new Panel { Name = "mapRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _mapPanel = new FlowLayoutPanel
        {
            Name = "mapPanel",
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(246, 248, 250),
            AccessibleName = "Operational map card wall (cards are runtime content)",
            TabStop = false
        };
        _mapLegend = new Label
        {
            Name = "mapLegend",
            Dock = DockStyle.Top,
            Height = 34,
            Text = "Green: active | Yellow: idle | Blue: syncing | Red: offline | Gray: critical",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            ForeColor = Color.FromArgb(71, 85, 105),
            BackColor = Color.FromArgb(241, 245, 249),
            TabStop = false
        };
        _mapActions = new FlowLayoutPanel { Name = "mapActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _mapRefreshButton = new Button { Name = "mapRefreshButton", Text = "Refresh Map", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 11 };
        _mapReviewButton = new Button { Name = "mapReviewButton", Text = "Raise Health Review", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 12 };
        _mapActions.Controls.Add(_mapRefreshButton);
        _mapActions.Controls.Add(_mapReviewButton);

        _mapRoot.Controls.Add(_mapPanel);
        _mapRoot.Controls.Add(_mapLegend);
        _mapRoot.Controls.Add(_mapActions);
        _mapTab.Controls.Add(_mapRoot);

        // ══ Device State tab ═════════════════════════════════════════════════
        _deviceStateTab = new TabPage { Name = "deviceStateTab", Text = "Device State" };
        _deviceStateGrid = new DataGridView
        {
            Name = "deviceStateGrid",
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
            AccessibleName = "Device layer state",
            TabIndex = 13
        };
        _deviceStateGrid.Columns.Add("Device", "Device");
        _deviceStateGrid.Columns.Add("Layer", "Layer");
        _deviceStateGrid.Columns.Add("State", "State");
        _deviceStateTab.Controls.Add(_deviceStateGrid);

        // ══ Realtime Sync tab ════════════════════════════════════════════════
        _syncTab = new TabPage { Name = "syncTab", Text = "Realtime Sync" };
        _syncStatusGrid = new DataGridView
        {
            Name = "syncStatusGrid",
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
            AccessibleName = "Sync queue status",
            TabIndex = 14
        };
        _syncStatusGrid.Columns.Add("Queue", "Queue");
        _syncStatusGrid.Columns.Add("Pending", "Pending");
        _syncStatusGrid.Columns.Add("Retry", "Retry");
        _syncStatusGrid.Columns.Add("LastAck", "Last Ack");
        _syncTab.Controls.Add(_syncStatusGrid);

        // ══ XFS Events tab ═══════════════════════════════════════════════════
        _xfsTab = new TabPage { Name = "xfsTab", Text = "XFS Events" };
        _xfsRoot = new Panel { Name = "xfsRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _xfsGrid = new DataGridView
        {
            Name = "xfsGrid",
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
            AccessibleName = "XFS event findings",
            TabIndex = 15
        };
        _xfsGrid.Columns.Add("Vendor", "Vendor");
        _xfsGrid.Columns.Add("Component", "Component");
        _xfsGrid.Columns.Add("Severity", "Severity");
        _xfsGrid.Columns.Add("Message", "Message");

        _xfsActions = new FlowLayoutPanel { Name = "xfsActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _xfsNcrButton = new Button { Name = "xfsNcrButton", Text = "Load NCR Sample", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 16 };
        _xfsGrgButton = new Button { Name = "xfsGrgButton", Text = "Load GRG Sample", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 17 };
        _xfsWincorButton = new Button { Name = "xfsWincorButton", Text = "Load Wincor Sample", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 18 };
        _xfsHyosungButton = new Button { Name = "xfsHyosungButton", Text = "Load Hyosung Sample", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 19 };
        _xfsWindowButton = new Button { Name = "xfsWindowButton", Text = "Open XFS Window", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 20 };
        _xfsClearButton = new Button { Name = "xfsClearButton", Text = "Clear", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 21 };
        _xfsActions.Controls.Add(_xfsNcrButton);
        _xfsActions.Controls.Add(_xfsGrgButton);
        _xfsActions.Controls.Add(_xfsWincorButton);
        _xfsActions.Controls.Add(_xfsHyosungButton);
        _xfsActions.Controls.Add(_xfsWindowButton);
        _xfsActions.Controls.Add(_xfsClearButton);

        _xfsRoot.Controls.Add(_xfsGrid);
        _xfsRoot.Controls.Add(_xfsActions);
        _xfsTab.Controls.Add(_xfsRoot);

        // ══ Vendor Logs tab ══════════════════════════════════════════════════
        _vendorLogsTab = new TabPage { Name = "vendorLogsTab", Text = "Vendor Logs" };
        _vendorLogsRoot = new Panel { Name = "vendorLogsRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _vendorLog = new RichTextBox
        {
            Name = "vendorLog",
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BackColor = Color.FromArgb(252, 252, 253),
            Text = "Paste NCR, GRG, Diebold, or Wincor log text here.",
            AccessibleName = "Vendor log input and probable-cause output",
            TabIndex = 22
        };
        _vendorLogsActions = new FlowLayoutPanel { Name = "vendorLogsActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _vendorAnalyzeButton = new Button { Name = "vendorAnalyzeButton", Text = "Analyze Log", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 23 };
        _vendorExtractButton = new Button { Name = "vendorExtractButton", Text = "Extract Probable Cause", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 24 };
        _vendorClearButton = new Button { Name = "vendorClearButton", Text = "Clear", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 25 };
        _vendorLogsActions.Controls.Add(_vendorAnalyzeButton);
        _vendorLogsActions.Controls.Add(_vendorExtractButton);
        _vendorLogsActions.Controls.Add(_vendorClearButton);

        _vendorLogsRoot.Controls.Add(_vendorLog);
        _vendorLogsRoot.Controls.Add(_vendorLogsActions);
        _vendorLogsTab.Controls.Add(_vendorLogsRoot);

        // ══ Reports tab ══════════════════════════════════════════════════════
        _reportsTab = new TabPage { Name = "reportsTab", Text = "Reports" };
        _reportsRoot = new Panel { Name = "reportsRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _reportsSplit = new SplitContainer
        {
            Name = "reportsSplit",
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 210
        };
        _reportsWindowGrid = new DataGridView
        {
            Name = "reportsWindowGrid",
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
            AccessibleName = "Operational window summary (ops bundle)",
            TabIndex = 26
        };
        _reportsWindowGrid.Columns.Add("Window", "Window");
        _reportsWindowGrid.Columns.Add("Hours", "Hours");
        _reportsWindowGrid.Columns.Add("Fleet", "Fleet");
        _reportsWindowGrid.Columns.Add("Connected", "Connected");
        _reportsWindowGrid.Columns.Add("Offline", "Offline");
        _reportsWindowGrid.Columns.Add("SyncOpen", "Sync Open");
        _reportsWindowGrid.Columns.Add("SyncFailed", "Sync Failed");
        _reportsWindowGrid.Columns.Add("PendingDel", "Pending Delivery");
        _reportsWindowGrid.Columns.Add("CmdFail", "Command Failures");
        _reportsWindowGrid.Columns.Add("TelWarn", "Telemetry Warnings");
        _reportsWindowGrid.Columns.Add("TelErr", "Telemetry Errors");
        _reportsFilesGrid = new DataGridView
        {
            Name = "reportsFilesGrid",
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
            AccessibleName = "Report file index",
            TabIndex = 27
        };
        _reportsFilesGrid.Columns.Add("File", "File");
        _reportsFilesGrid.Columns.Add("Category", "Category");
        _reportsFilesGrid.Columns.Add("Modified", "Modified");
        _reportsFilesGrid.Columns.Add("SizeKB", "Size KB");

        _reportsSplit.Panel1.Controls.Add(_reportsWindowGrid);
        _reportsSplit.Panel2.Controls.Add(_reportsFilesGrid);

        _reportsActions = new FlowLayoutPanel { Name = "reportsActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _reportsRefreshButton = new Button { Name = "reportsRefreshButton", Text = "Refresh Reports", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 28 };
        _reportsBundleButton = new Button { Name = "reportsBundleButton", Text = "Load Latest Ops Bundle", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 29 };
        _reportsWindowSummaryButton = new Button { Name = "reportsWindowSummaryButton", Text = "Open Windows Summary", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 30 };
        _reportsFilesIndexButton = new Button { Name = "reportsFilesIndexButton", Text = "Open Files Index", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 31 };
        _reportsInfo = new Label { Name = "reportsInfo", AutoSize = true, Padding = new Padding(8, 8, 0, 0), ForeColor = Color.FromArgb(71, 85, 105), TabStop = false };
        _reportsActions.Controls.Add(_reportsRefreshButton);
        _reportsActions.Controls.Add(_reportsBundleButton);
        _reportsActions.Controls.Add(_reportsWindowSummaryButton);
        _reportsActions.Controls.Add(_reportsFilesIndexButton);
        _reportsActions.Controls.Add(_reportsInfo);

        _reportsRoot.Controls.Add(_reportsSplit);
        _reportsRoot.Controls.Add(_reportsActions);
        _reportsTab.Controls.Add(_reportsRoot);

        // ══ Smart Analysis tab (SS-27) ═══════════════════════════════════════
        _smartTab = new TabPage { Name = "smartTab", Text = "Smart Analysis" };
        _smartRoot = new Panel { Name = "smartRoot", Dock = DockStyle.Fill, Padding = new Padding(8), TabStop = false };

        _smartActions = new FlowLayoutPanel { Name = "smartActions", Dock = DockStyle.Top, Height = 58, Padding = new Padding(8), WrapContents = true, TabStop = false };
        _smartAnalyzeButton = new Button { Name = "smartAnalyzeButton", Text = "Analyze Text", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 32 };
        _smartReSortButton = new Button { Name = "smartReSortButton", Text = "Re-sort by Severity", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 33 };
        _smartGroupButton = new Button { Name = "smartGroupButton", Text = "Group by Category", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 34 };
        _smartSampleButton = new Button { Name = "smartSampleButton", Text = "Load Sample Log", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 35 };
        _smartClearButton = new Button { Name = "smartClearButton", Text = "Clear", AutoSize = true, Height = 32, Margin = new Padding(4), TabIndex = 36 };
        _smartVendorBox = new TextBox { Name = "smartVendorBox", Width = 110, PlaceholderText = "Vendor (NCR/GRG/Wincor)", Margin = new Padding(4, 8, 4, 4), AccessibleName = "Vendor hint for smart analysis", TabIndex = 37 };
        _smartActions.Controls.Add(_smartAnalyzeButton);
        _smartActions.Controls.Add(_smartReSortButton);
        _smartActions.Controls.Add(_smartGroupButton);
        _smartActions.Controls.Add(_smartSampleButton);
        _smartActions.Controls.Add(_smartClearButton);
        _smartActions.Controls.Add(_smartVendorBox);

        _smartSplit = new SplitContainer
        {
            Name = "smartSplit",
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 160
        };
        _smartUploadBox = new RichTextBox
        {
            Name = "smartUploadBox",
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            BackColor = Color.FromArgb(252, 252, 253),
            AccessibleName = "Smart analysis upload payload",
            TabIndex = 38
        };
        _smartUploadBox.Text = string.Join(Environment.NewLine, new[]
        {
            "[2025-09-14 12:01:33] NCR SDC LINK ERROR: M-146 timeout on dispenser handler.",
            "[2025-09-14 12:01:35] GRG CASSETTE STATUS: CAS1=1800 CAS2=0 CAS3=900 CAS4=600",
            "[2025-09-14 12:01:40] NCR PRINTER JAM at receipt path, customer reports stuck paper.",
            "[2025-09-14 12:02:05] WINCOR WOSA/XFS SP ERROR: CDM CASH UNIT EMPTY",
            "[2025-09-14 12:02:30] WITHDRAWAL AMOUNT=200 EUR processed.",
            "[2025-09-14 12:02:42] CARD CAPTURED — dispute opened on rejected dispense."
        });
        _smartSplit.Panel1.Controls.Add(_smartUploadBox);

        _smartResults = new TableLayoutPanel { Name = "smartResults", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, TabStop = false };
        _smartResults.RowStyles.Add(new RowStyle(SizeType.Percent, 60f));
        _smartResults.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        _smartResults.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

        _smartFindingsGrid = new DataGridView
        {
            Name = "smartFindingsGrid",
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
            AccessibleName = "Smart analysis findings",
            TabIndex = 39
        };
        _smartFindingsGrid.Columns.Add("Severity", "Severity");
        _smartFindingsGrid.Columns.Add("Category", "Category");
        _smartFindingsGrid.Columns.Add("Code", "Code");
        _smartFindingsGrid.Columns.Add("Vendor", "Vendor");
        _smartFindingsGrid.Columns.Add("Action", "Recommended Action");
        _smartFindingsGrid.Columns.Add("Source", "Source");

        _smartBottomSplit = new SplitContainer
        {
            Name = "smartBottomSplit",
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 480
        };
        _smartCassetteGrid = new DataGridView
        {
            Name = "smartCassetteGrid",
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
            AccessibleName = "Cassette movement summary",
            TabIndex = 40
        };
        _smartCassetteGrid.Columns.Add("Slot", "Slot");
        _smartCassetteGrid.Columns.Add("Notes", "Notes");
        _smartHourlyGrid = new DataGridView
        {
            Name = "smartHourlyGrid",
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
            AccessibleName = "Hourly log density",
            TabIndex = 41
        };
        _smartHourlyGrid.Columns.Add("Hour", "Hour");
        _smartHourlyGrid.Columns.Add("Lines", "Lines");
        _smartBottomSplit.Panel1.Controls.Add(_smartCassetteGrid);
        _smartBottomSplit.Panel2.Controls.Add(_smartHourlyGrid);

        _smartSummaryRow = new TableLayoutPanel { Name = "smartSummaryRow", Dock = DockStyle.Top, Height = 92, ColumnCount = 4, Padding = new Padding(4), TabStop = false };
        for (var i = 0; i < 4; i++)
            _smartSummaryRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

        _smartCriticalCard = new Panel { Name = "smartCriticalCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _smartCriticalCardCaption = new Label { Name = "smartCriticalCardCaption", Text = "Critical Findings", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _smartCritical = new Label { Name = "smartCritical", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Critical finding count", TabStop = false };
        _smartCriticalCard.Controls.Add(_smartCritical);
        _smartCriticalCard.Controls.Add(_smartCriticalCardCaption);
        _smartCriticalCard.Controls.Add(new Panel { Name = "smartCriticalAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(238, 82, 83), TabStop = false });

        _smartWarningCard = new Panel { Name = "smartWarningCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _smartWarningCardCaption = new Label { Name = "smartWarningCardCaption", Text = "Warnings", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _smartWarning = new Label { Name = "smartWarning", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Warning count", TabStop = false };
        _smartWarningCard.Controls.Add(_smartWarning);
        _smartWarningCard.Controls.Add(_smartWarningCardCaption);
        _smartWarningCard.Controls.Add(new Panel { Name = "smartWarningAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(255, 159, 67), TabStop = false });

        _smartInfoCard = new Panel { Name = "smartInfoCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _smartInfoCardCaption = new Label { Name = "smartInfoCardCaption", Text = "Info", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _smartInfo = new Label { Name = "smartInfo", Text = "0", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Info count", TabStop = false };
        _smartInfoCard.Controls.Add(_smartInfo);
        _smartInfoCard.Controls.Add(_smartInfoCardCaption);
        _smartInfoCard.Controls.Add(new Panel { Name = "smartInfoAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(46, 134, 222), TabStop = false });

        _smartSummaryCard = new Panel { Name = "smartSummaryCard", Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(10), BackColor = Color.White, TabStop = false };
        _smartSummaryCardCaption = new Label { Name = "smartSummaryCardCaption", Text = "Last Trace", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(90, 90, 90), Font = new Font("Segoe UI", 8F), TabStop = false };
        _smartSummary = new Label { Name = "smartSummary", Text = "—", Dock = DockStyle.Fill, ForeColor = Color.FromArgb(31, 41, 55), Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, AccessibleName = "Last smart analysis trace", TabStop = false };
        _smartSummaryCard.Controls.Add(_smartSummary);
        _smartSummaryCard.Controls.Add(_smartSummaryCardCaption);
        _smartSummaryCard.Controls.Add(new Panel { Name = "smartSummaryAccent", Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(95, 39, 205), TabStop = false });

        _smartSummaryRow.Controls.Add(_smartCriticalCard);
        _smartSummaryRow.Controls.Add(_smartWarningCard);
        _smartSummaryRow.Controls.Add(_smartInfoCard);
        _smartSummaryRow.Controls.Add(_smartSummaryCard);

        _smartResults.Controls.Add(_smartFindingsGrid, 0, 0);
        _smartResults.Controls.Add(_smartBottomSplit, 0, 1);
        _smartResults.Controls.Add(_smartSummaryRow, 0, 2);
        _smartSplit.Panel2.Controls.Add(_smartResults);

        _smartRoot.Controls.Add(_smartActions);
        _smartRoot.Controls.Add(_smartSplit);
        _smartTab.Controls.Add(_smartRoot);

        // ══ tab assembly ══════════════════════════════════════════════════════
        _tabs.TabPages.Add(_overviewTab);
        _tabs.TabPages.Add(_cashMatrixTab);
        _tabs.TabPages.Add(_terminalListTab);
        _tabs.TabPages.Add(_mapTab);
        _tabs.TabPages.Add(_deviceStateTab);
        _tabs.TabPages.Add(_syncTab);
        _tabs.TabPages.Add(_xfsTab);
        _tabs.TabPages.Add(_vendorLogsTab);
        _tabs.TabPages.Add(_reportsTab);
        _tabs.TabPages.Add(_smartTab);

        Controls.Add(_tabs);

        ResumeLayout(false);
    }
}
