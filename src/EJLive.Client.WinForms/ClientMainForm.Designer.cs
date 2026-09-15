using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Client.WinForms;

/// <summary>
/// Designer surface for <see cref="ClientMainForm"/> (SS-10 / C-28). Authored to the shape
/// Visual Studio's WinForms designer emits and consumes: every control is a named field,
/// created and property-set only inside <c>InitializeComponent</c>, parented through
/// <c>Controls.Add</c> with <c>SuspendLayout</c>/<c>ResumeLayout</c> discipline,
/// <c>TabStop</c>/<c>TabIndex</c> in visual order, Dock/Anchor-only layout (no absolute
/// pixel placement), and <c>AccessibleName</c> on data surfaces.
///
/// Regeneration protocol (safe <c>.Designer.cs</c> cycle — SS-10, proven on
/// <c>JournalStudioForm</c> in C-24):
///  1. Event bindings live in the companion file's <c>WireEvents()</c>, NOT here, so
///     deleting and re-emitting this partial never orphans a handler;
///  2. behaviour overrides stay in the companion file;
///  3. adding a control means: declare the field, create + configure here, wire behaviour
///     in the companion — the three-step order keeps the designer round-trip stable;
///  4. the form carries a <c>.resx</c> but no resources are drawn from it: every property
///     is a literal the designer can set.
///
/// Migrated-surface note: the legacy constructor set window <c>Size</c> (1120 × 760);
/// the designer equivalent is <c>ClientSize</c> (1104 × 713) with the same
/// <c>MinimumSize</c>, so the window metrics are preserved through the split.
/// <c>_refreshTimer</c> is deliberately NOT a design-time component: it is a runtime
/// artefact owned by the companion partial (created, started and stopped there), so
/// <c>Dispose(bool)</c> disposes only the visual tree.
/// Control → function mapping is documented on the companion type and in
/// <c>docs/inventory/UI-SURFACES.md</c> (generated).
/// </summary>
public sealed partial class ClientMainForm
{
    // ── header: title, snapshot source, actions ─────────────────────────────
    private Panel _headerPanel = null!;
    private Label _title = null!;
    private Label _snapshotSource = null!;
    private FlowLayoutPanel _headerActions = null!;
    private Button _languageButton = null!;
    private Button _refreshButton = null!;

    // ── status card row ──────────────────────────────────────────────────────
    private TableLayoutPanel _statusCardsPanel = null!;
    private Panel _serviceCard = null!;
    private Label _serviceCardCaption = null!;
    private Label _serviceState = null!;
    private Panel _connectionCard = null!;
    private Label _connectionCardCaption = null!;
    private Label _connectionState = null!;
    private Panel _heartbeatCard = null!;
    private Label _heartbeatCardCaption = null!;
    private Label _heartbeatState = null!;
    private Panel _syncCard = null!;
    private Label _syncCardCaption = null!;
    private Label _syncState = null!;

    // ── details: summary grid + component grid ───────────────────────────────
    private SplitContainer _detailsSplit = null!;
    private Panel _detailsCard = null!;
    private TableLayoutPanel _detailsGrid = null!;
    private Label _atmCaption = null!;
    private Label _atmValue = null!;
    private Label _sessionCaption = null!;
    private Label _sessionValue = null!;
    private Label _queueCaption = null!;
    private Label _queueValue = null!;
    private Label _trafficCaption = null!;
    private Label _trafficValue = null!;
    private Label _errorCaption = null!;
    private Label _errorValue = null!;
    private DataGridView _componentGrid = null!;

    // ── event feed ───────────────────────────────────────────────────────────
    private Panel _eventsCard = null!;
    private ListBox _events = null!;

    // ── root layout ──────────────────────────────────────────────────────────
    private TableLayoutPanel _rootPanel = null!;

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

        // ══ form ══════════════════════════════════════════════════════════════
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1104, 713);
        MinimumSize = new Size(980, 680);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EJLive Client Companion";
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(248, 250, 252);

        // ══ root: 1 column — header / cards / details / events ═════════════════
        _rootPanel = new TableLayoutPanel
        {
            Name = "rootPanel",
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(20),
            BackColor = Color.FromArgb(248, 250, 252),
            TabStop = false
        };
        _rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        _rootPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
        _rootPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 68f));
        _rootPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 32f));

        // ══ header: title + snapshot source + right action strip ══════════════
        _headerPanel = new Panel { Name = "headerPanel", Dock = DockStyle.Fill, BackColor = Color.FromArgb(248, 250, 252), TabStop = false };

        _title = new Label
        {
            Name = "title",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 19F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            TextAlign = ContentAlignment.MiddleLeft,
            AccessibleName = "Endpoint console title",
            TabStop = false
        };
        _snapshotSource = new Label
        {
            Name = "snapshotSource",
            Dock = DockStyle.Bottom,
            Height = 22,
            ForeColor = Color.FromArgb(100, 116, 139),
            TextAlign = ContentAlignment.MiddleLeft,
            TabStop = false
        };
        _headerActions = new FlowLayoutPanel
        {
            Name = "headerActions",
            Dock = DockStyle.Right,
            Width = 270,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 14, 0, 0),
            TabStop = false
        };
        _languageButton = new Button
        {
            Name = "languageButton",
            Text = "العربية",
            AutoSize = true,
            MinimumSize = new Size(112, 36),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(226, 232, 240),
            ForeColor = Color.FromArgb(30, 41, 59),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Margin = new Padding(4),
            TabIndex = 0
        };
        _refreshButton = new Button
        {
            Name = "refreshButton",
            Text = "Refresh",
            AutoSize = true,
            MinimumSize = new Size(112, 36),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(226, 232, 240),
            ForeColor = Color.FromArgb(30, 41, 59),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Margin = new Padding(4),
            TabIndex = 1
        };
        _headerActions.Controls.Add(_languageButton);
        _headerActions.Controls.Add(_refreshButton);
        _headerPanel.Controls.Add(_title);
        _headerPanel.Controls.Add(_snapshotSource);
        _headerPanel.Controls.Add(_headerActions);

        // ══ status card row: 4 equal columns ═══════════════════════════════════
        _statusCardsPanel = new TableLayoutPanel
        {
            Name = "statusCardsPanel",
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 4, 0, 12),
            TabStop = false
        };
        for (var i = 0; i < 4; i++)
            _statusCardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

        _serviceCard = new Panel { Name = "serviceCard", Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(6), Padding = new Padding(14), TabStop = false };
        _serviceCardCaption = new Label
        {
            Name = "serviceCardCaption",
            Text = "Agent Service",
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Color.FromArgb(100, 116, 139),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TabStop = false
        };
        _serviceState = new Label
        {
            Name = "serviceState",
            Text = "Waiting",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(217, 119, 6),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AccessibleName = "Agent service state",
            TabStop = false
        };
        _serviceCard.Controls.Add(_serviceState);
        _serviceCard.Controls.Add(_serviceCardCaption);

        _connectionCard = new Panel { Name = "connectionCard", Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(6), Padding = new Padding(14), TabStop = false };
        _connectionCardCaption = new Label
        {
            Name = "connectionCardCaption",
            Text = "Server Link",
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Color.FromArgb(100, 116, 139),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TabStop = false
        };
        _connectionState = new Label
        {
            Name = "connectionState",
            Text = "Waiting",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(217, 119, 6),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AccessibleName = "Server link state",
            TabStop = false
        };
        _connectionCard.Controls.Add(_connectionState);
        _connectionCard.Controls.Add(_connectionCardCaption);

        _heartbeatCard = new Panel { Name = "heartbeatCard", Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(6), Padding = new Padding(14), TabStop = false };
        _heartbeatCardCaption = new Label
        {
            Name = "heartbeatCardCaption",
            Text = "Heartbeat",
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Color.FromArgb(100, 116, 139),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TabStop = false
        };
        _heartbeatState = new Label
        {
            Name = "heartbeatState",
            Text = "Waiting",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(217, 119, 6),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AccessibleName = "Heartbeat state",
            TabStop = false
        };
        _heartbeatCard.Controls.Add(_heartbeatState);
        _heartbeatCard.Controls.Add(_heartbeatCardCaption);

        _syncCard = new Panel { Name = "syncCard", Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(6), Padding = new Padding(14), TabStop = false };
        _syncCardCaption = new Label
        {
            Name = "syncCardCaption",
            Text = "Journal Sync",
            Dock = DockStyle.Top,
            Height = 28,
            ForeColor = Color.FromArgb(100, 116, 139),
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            TabStop = false
        };
        _syncState = new Label
        {
            Name = "syncState",
            Text = "Waiting",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(217, 119, 6),
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AccessibleName = "Journal sync state",
            TabStop = false
        };
        _syncCard.Controls.Add(_syncState);
        _syncCard.Controls.Add(_syncCardCaption);

        _statusCardsPanel.Controls.Add(_serviceCard, 0, 0);
        _statusCardsPanel.Controls.Add(_connectionCard, 1, 0);
        _statusCardsPanel.Controls.Add(_heartbeatCard, 2, 0);
        _statusCardsPanel.Controls.Add(_syncCard, 3, 0);

        // ══ details: summary grid (left) + component grid (right) ══════════════
        _detailsSplit = new SplitContainer
        {
            Name = "detailsSplit",
            Dock = DockStyle.Fill,
            SplitterDistance = 410,
            SplitterWidth = 8,
            BackColor = Color.FromArgb(248, 250, 252)
        };

        _detailsCard = new Panel { Name = "detailsCard", Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(4), TabStop = false };
        _detailsGrid = new TableLayoutPanel
        {
            Name = "detailsGrid",
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(12),
            TabStop = false
        };
        _detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        _detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        for (var i = 0; i < 5; i++)
            _detailsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

        _atmCaption = new Label { Name = "atmCaption", Text = "ATM", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI", 9F, FontStyle.Bold), TabStop = false };
        _atmValue = new Label { Name = "atmValue", Text = "-", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(30, 41, 59), AutoEllipsis = true, AccessibleName = "ATM identifier", TabStop = false };
        _sessionCaption = new Label { Name = "sessionCaption", Text = "Session", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI", 9F, FontStyle.Bold), TabStop = false };
        _sessionValue = new Label { Name = "sessionValue", Text = "-", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(30, 41, 59), AutoEllipsis = true, AccessibleName = "Session identifier", TabStop = false };
        _queueCaption = new Label { Name = "queueCaption", Text = "Pending", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI", 9F, FontStyle.Bold), TabStop = false };
        _queueValue = new Label { Name = "queueValue", Text = "-", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(30, 41, 59), AutoEllipsis = true, AccessibleName = "Pending outbox items", TabStop = false };
        _trafficCaption = new Label { Name = "trafficCaption", Text = "Traffic", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI", 9F, FontStyle.Bold), TabStop = false };
        _trafficValue = new Label { Name = "trafficValue", Text = "-", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(30, 41, 59), AutoEllipsis = true, AccessibleName = "Upstream and downstream traffic", TabStop = false };
        _errorCaption = new Label { Name = "errorCaption", Text = "Last error", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(100, 116, 139), Font = new Font("Segoe UI", 9F, FontStyle.Bold), TabStop = false };
        _errorValue = new Label { Name = "errorValue", Text = "-", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(30, 41, 59), AutoEllipsis = true, AccessibleName = "Last error message", TabStop = false };

        _detailsGrid.Controls.Add(_atmCaption, 0, 0);
        _detailsGrid.Controls.Add(_atmValue, 1, 0);
        _detailsGrid.Controls.Add(_sessionCaption, 0, 1);
        _detailsGrid.Controls.Add(_sessionValue, 1, 1);
        _detailsGrid.Controls.Add(_queueCaption, 0, 2);
        _detailsGrid.Controls.Add(_queueValue, 1, 2);
        _detailsGrid.Controls.Add(_trafficCaption, 0, 3);
        _detailsGrid.Controls.Add(_trafficValue, 1, 3);
        _detailsGrid.Controls.Add(_errorCaption, 0, 4);
        _detailsGrid.Controls.Add(_errorValue, 1, 4);
        _detailsCard.Controls.Add(_detailsGrid);

        _componentGrid = new DataGridView
        {
            Name = "componentGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AccessibleName = "Agent component runtime table",
            TabIndex = 2
        };
        _componentGrid.Columns.Add("Component", "Component");
        _componentGrid.Columns.Add("Status", "Status");
        _componentGrid.Columns.Add("Detail", "Detail");
        _componentGrid.Columns[0].FillWeight = 32f;
        _componentGrid.Columns[1].FillWeight = 18f;
        _componentGrid.Columns[2].FillWeight = 50f;

        _detailsSplit.Panel1.Padding = new Padding(0, 0, 4, 0);
        _detailsSplit.Panel1.Controls.Add(_detailsCard);
        _detailsSplit.Panel2.Padding = new Padding(4, 0, 0, 0);
        _detailsSplit.Panel2.Controls.Add(_componentGrid);

        // ══ event feed ═════════════════════════════════════════════════════════
        _eventsCard = new Panel { Name = "eventsCard", Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(4), Padding = new Padding(12), TabStop = false };
        _events = new ListBox
        {
            Name = "events",
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(30, 41, 59),
            Font = new Font("Consolas", 9.5F),
            AccessibleName = "Agent event feed",
            TabIndex = 3
        };
        _eventsCard.Controls.Add(_events);

        // ══ z-order assembly (root rows 0..3) ══════════════════════════════════
        _rootPanel.Controls.Add(_headerPanel, 0, 0);
        _rootPanel.Controls.Add(_statusCardsPanel, 0, 1);
        _rootPanel.Controls.Add(_detailsSplit, 0, 2);
        _rootPanel.Controls.Add(_eventsCard, 0, 3);

        Controls.Add(_rootPanel);

        ResumeLayout(false);
    }
}
