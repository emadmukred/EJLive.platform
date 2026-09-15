using System.Drawing;
using System.Windows.Forms;
using EJLive.Core.UI;

namespace EJLive.Server.WinForms;

/// <summary>
/// Designer surface for <see cref="JournalStudioForm"/> (SS-10). Authored to the shape
/// Visual Studio's WinForms designer emits and consumes: every control is a named field,
/// created and property-set only inside <c>InitializeComponent</c>, parented through
/// <c>Add</c>/<c>SuspendLayout</c> pairs, with <c>TabStop</c> + <c>TabIndex</c> in visual
/// order and Dock/Anchor-only layout (no absolute pixel placement).
///
/// Regeneration protocol (safe <c>.Designer.cs</c> cycle — SS-10):
///  1. Event bindings live in the companion file's <c>WireEvents()</c>, NOT here, so
///     deleting and re-emitting this partial never orphans a handler;
///  2. behaviour overrides stay in the companion file;
///  3. adding a control means: declare the field, create + configure here, wire behaviour
///     in the companion — the three-step order keeps the designer round-trip stable;
///  4. the form carries no <c>.resx</c>: every property is a literal settable by the
///     designer (icons/resources would require adding <c>components</c> + a resx, which
///     this surface deliberately avoids).
/// Control → function mapping is documented on the companion type and in
/// <c>docs/inventory/UI-SURFACES.md</c> (generated).
/// </summary>
public sealed partial class JournalStudioForm
{
    // ── toolbar row 1: commands ──────────────────────────────────────────────
    private Button _openButton = null!;
    private Button _parseButton = null!;
    private Button _exportCsvButton = null!;
    private Button _exportJsonButton = null!;
    private Button _exportExcelButton = null!;
    private Button _bulkFolderButton = null!;
    private Button _copyDataButton = null!;
    private CheckBox _redactPreviewCheck = null!;
    private ComboBox _classificationComboBox = null!;
    private TableLayoutPanel _toolbarPanel = null!;

    // ── toolbar row 2: filters ───────────────────────────────────────────────
    private TextBox _searchTextBox = null!;
    private ComboBox _terminalComboBox = null!;
    private NumericUpDown _amountMinBox = null!;
    private NumericUpDown _amountMaxBox = null!;
    private DateTimePicker _fromPicker = null!;
    private DateTimePicker _toPicker = null!;
    private TableLayoutPanel _filterPanel = null!;
    private TableLayoutPanel _amountRangePanel = null!;
    private TableLayoutPanel _dateRangePanel = null!;

    // ── centre: grid ⇄ (raw | ladder) ────────────────────────────────────────
    private SplitContainer _centreSplit = null!;
    private TableLayoutPanel _gridPanel = null!;
    private DataGridView _transactionsGrid = null!;
    private Label _summaryLabel = null!;
    private SplitContainer _rightSplit = null!;
    private TableLayoutPanel _rawPanel = null!;
    private TextBox _rawLineView = null!;
    private TableLayoutPanel _ladderPanel = null!;
    private TextBox _ladderView = null!;

    // ── bottom tabs: anomalies / analytics / correlation / bulk ─────────────
    private TabControl _bottomTabs = null!;
    private TabPage _anomalyTab = null!;
    private DataGridView _findingsGrid = null!;
    private TabPage _analyticsTab = null!;
    private TableLayoutPanel _analyticsPanel = null!;
    private OwnerDrawnHistogram _classificationHistogram = null!;
    private OwnerDrawnHistogram _anomalyPareto = null!;
    private OwnerDrawnTimeSeries _throughputChart = null!;
    private Label _reconciliationLabel = null!;
    private TabPage _correlationTab = null!;
    private TabPage _bulkTab = null!;
    private TableLayoutPanel _bulkPanel = null!;
    private Panel _bulkToolbarPanel = null!;
    private Button _bulkCancelButton = null!;
    private Button _exportBulkCsvButton = null!;
    private Button _exportBulkExcelButton = null!;
    private Label _bulkStatusLabel = null!;
    private ProgressBar _bulkProgressBar = null!;
    private DataGridView _bulkGrid = null!;

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

        // ══ form ═══════════════════════════════════════════════════════════════
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1264, 761);
        MinimumSize = new Size(960, 640);
        StartPosition = FormStartPosition.CenterParent;
        Text = "Journal Analysis Log Studio";
        Font = new Font("Segoe UI", 9F);
        DoubleBuffered = true;

        // ══ toolbar row 1 (10 equal columns; Open spans 2) ═════════════════════
        _toolbarPanel = new TableLayoutPanel
        {
            Name = "toolbarPanel",
            Dock = DockStyle.Top,
            Height = 90,
            ColumnCount = 10,
            RowCount = 1,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(245, 247, 250),
            TabStop = false
        };
        for (var i = 0; i < 10; i++)
            _toolbarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10f));

        _openButton = new Button { Name = "openButton", Text = "&Open .LOG…", Height = 32, Dock = DockStyle.Fill, TabIndex = 0 };
        _parseButton = new Button { Name = "parseButton", Text = "Re-&parse at offset", Height = 32, Dock = DockStyle.Fill, Enabled = false, TabIndex = 1 };
        _exportCsvButton = new Button { Name = "exportCsvButton", Text = "Export &CSV", Height = 32, Dock = DockStyle.Fill, Enabled = false, TabIndex = 2 };
        _exportJsonButton = new Button { Name = "exportJsonButton", Text = "Export &JSON", Height = 32, Dock = DockStyle.Fill, Enabled = false, TabIndex = 3 };
        _exportExcelButton = new Button { Name = "exportExcelButton", Text = "Export &Excel", Height = 32, Dock = DockStyle.Fill, Enabled = false, TabIndex = 4 };
        _copyDataButton = new Button { Name = "copyDataButton", Text = "&Copy data", Height = 32, Dock = DockStyle.Fill, Enabled = false, TabIndex = 5 };
        _bulkFolderButton = new Button { Name = "bulkFolderButton", Text = "Bulk analyze folder…", Height = 32, Dock = DockStyle.Fill, TabIndex = 6 };
        _redactPreviewCheck = new CheckBox
        {
            Name = "redactPreviewCheck",
            Text = "Redact PAN (Observer view)",
            Dock = DockStyle.Fill,
            Checked = true,
            CheckAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleLeft,
            AccessibleName = "Redact PAN preview toggle",
            TabIndex = 7
        };
        _classificationComboBox = new ComboBox
        {
            Name = "classificationComboBox",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            AccessibleName = "Transaction classification filter",
            TabIndex = 8
        };
        _classificationComboBox.Items.Add("All classifications");
        foreach (var kind in Enum.GetValues<TransactionClassification>())
            _classificationComboBox.Items.Add(kind.ToString());
        _classificationComboBox.SelectedIndex = 0;

        _toolbarPanel.Controls.Add(_openButton, 0, 0);
        _toolbarPanel.SetColumnSpan(_openButton, 2);
        _toolbarPanel.Controls.Add(_parseButton, 2, 0);
        _toolbarPanel.Controls.Add(_exportCsvButton, 3, 0);
        _toolbarPanel.Controls.Add(_exportJsonButton, 4, 0);
        _toolbarPanel.Controls.Add(_exportExcelButton, 5, 0);
        _toolbarPanel.Controls.Add(_copyDataButton, 6, 0);
        _toolbarPanel.Controls.Add(_bulkFolderButton, 7, 0);
        _toolbarPanel.Controls.Add(_redactPreviewCheck, 8, 0);
        _toolbarPanel.Controls.Add(_classificationComboBox, 9, 0);

        // ══ toolbar row 2: search / terminal / amount / date ═══════════════════
        _filterPanel = new TableLayoutPanel
        {
            Name = "filterPanel",
            Dock = DockStyle.Top,
            Height = 60,
            ColumnCount = 6,
            RowCount = 1,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(250, 251, 253),
            TabStop = false
        };
        for (var i = 0; i < 6; i++)
            _filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));

        _searchTextBox = new TextBox
        {
            Name = "searchTextBox",
            Dock = DockStyle.Fill,
            PlaceholderText = "Search text (Ctrl+F)…",
            Margin = new Padding(0, 4, 0, 4),
            AccessibleName = "Journal transaction search",
            TabIndex = 10
        };
        _terminalComboBox = new ComboBox
        {
            Name = "terminalComboBox",
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDown,
            AccessibleName = "Terminal filter",
            TabIndex = 11
        };
        _terminalComboBox.Items.Add("All terminals");
        _terminalComboBox.SelectedIndex = 0;

        _amountRangePanel = new TableLayoutPanel { Name = "amountRangePanel", Dock = DockStyle.Fill, ColumnCount = 2, TabStop = false };
        _amountRangePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        _amountRangePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        _amountMinBox = new NumericUpDown
        {
            Name = "amountMinBox",
            Dock = DockStyle.Fill,
            Minimum = -1_000_000,
            Maximum = 1_000_000,
            DecimalPlaces = 2,
            Value = -1_000_000,
            AccessibleName = "Amount range minimum",
            TabIndex = 12
        };
        _amountMaxBox = new NumericUpDown
        {
            Name = "amountMaxBox",
            Dock = DockStyle.Fill,
            Minimum = -1_000_000,
            Maximum = 1_000_000,
            DecimalPlaces = 2,
            Value = 1_000_000,
            AccessibleName = "Amount range maximum",
            TabIndex = 13
        };
        _amountRangePanel.Controls.Add(_amountMinBox, 0, 0);
        _amountRangePanel.Controls.Add(_amountMaxBox, 1, 0);

        _dateRangePanel = new TableLayoutPanel { Name = "dateRangePanel", Dock = DockStyle.Fill, ColumnCount = 2, TabStop = false };
        _dateRangePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        _dateRangePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        _fromPicker = new DateTimePicker { Name = "fromPicker", Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, AccessibleName = "Date range from", TabIndex = 14 };
        _toPicker = new DateTimePicker { Name = "toPicker", Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, AccessibleName = "Date range to", TabIndex = 15 };
        _dateRangePanel.Controls.Add(_fromPicker, 0, 0);
        _dateRangePanel.Controls.Add(_toPicker, 1, 0);

        _filterPanel.Controls.Add(_searchTextBox, 0, 0);
        _filterPanel.SetColumnSpan(_searchTextBox, 2);
        _filterPanel.Controls.Add(_terminalComboBox, 2, 0);
        _filterPanel.Controls.Add(new Label
        {
            Name = "amountRangeLabel",
            Text = "Amount min/max",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            TabStop = false
        }, 3, 0);
        _filterPanel.Controls.Add(_amountRangePanel, 4, 0);
        _filterPanel.Controls.Add(_dateRangePanel, 5, 0);

        // ══ centre split: grid + summary | raw + ladder ════════════════════════
        _centreSplit = new SplitContainer
        {
            Name = "centreSplit",
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 700,
            SplitterWidth = 6
        };

        _gridPanel = new TableLayoutPanel { Name = "gridPanel", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, TabStop = false };
        _gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        _gridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));

        _transactionsGrid = new DataGridView
        {
            Name = "transactionsGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            VirtualMode = true,
            AllowUserToResizeRows = false,
            BackgroundColor = Color.White,
            AccessibleName = "Parsed journal transactions",
            TabIndex = 20
        };
        _transactionsGrid.Columns.Add("Timestamp", "Timestamp");
        _transactionsGrid.Columns.Add("Terminal", "Terminal");
        _transactionsGrid.Columns.Add("TransactionId", "Transaction Id");
        _transactionsGrid.Columns.Add("Classification", "Classification");
        _transactionsGrid.Columns.Add("Amount", "Amount");
        _transactionsGrid.Columns.Add("Confidence", "Conf.");
        _transactionsGrid.Columns.Add("Card", "Card");
        _transactionsGrid.Columns.Add("MCode", "MCode");
        _transactionsGrid.Columns.Add("RCode", "RCode");

        _summaryLabel = new Label
        {
            Name = "summaryLabel",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(8),
            ForeColor = Color.FromArgb(31, 41, 55),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AccessibleName = "Load summary: rows, vendor, parser, file",
            TabStop = false
        };
        _gridPanel.Controls.Add(_transactionsGrid, 0, 0);
        _gridPanel.Controls.Add(_summaryLabel, 0, 1);

        _rightSplit = new SplitContainer
        {
            Name = "rightSplit",
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 240,
            SplitterWidth = 6
        };

        _rawPanel = new TableLayoutPanel { Name = "rawPanel", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, TabStop = false };
        _rawPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        _rawPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        _rawPanel.Controls.Add(new Label
        {
            Name = "rawHeaderLabel",
            Text = "Raw source line (offset highlighted in studio render)",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(8, 4, 0, 0),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            TabStop = false
        }, 0, 0);
        _rawLineView = new TextBox
        {
            Name = "rawLineView",
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            Font = new Font("Consolas", 9F),
            BackColor = Color.FromArgb(252, 252, 253),
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            AccessibleName = "Raw source line view",
            TabIndex = 21
        };
        _rawPanel.Controls.Add(_rawLineView, 0, 1);

        _ladderPanel = new TableLayoutPanel { Name = "ladderPanel", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, TabStop = false };
        _ladderPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        _ladderPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        _ladderPanel.Controls.Add(new Label
        {
            Name = "ladderHeaderLabel",
            Text = "Parsed receipt ladder (classification · amount · card · trace · outcome)",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(8, 4, 0, 0),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            TabStop = false
        }, 0, 0);
        _ladderView = new TextBox
        {
            Name = "ladderView",
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            Font = new Font("Consolas", 9F),
            BackColor = Color.FromArgb(252, 252, 253),
            ScrollBars = ScrollBars.Both,
            WordWrap = false,
            AccessibleName = "Parsed receipt ladder view",
            TabIndex = 22
        };
        _ladderPanel.Controls.Add(_ladderView, 0, 1);

        _rightSplit.Panel1.Controls.Add(_rawPanel);
        _rightSplit.Panel2.Controls.Add(_ladderPanel);

        _centreSplit.Panel1.Controls.Add(_gridPanel);
        _centreSplit.Panel2.Controls.Add(_rightSplit);

        // ══ bottom tabs ════════════════════════════════════════════════════════
        _bottomTabs = new TabControl { Name = "bottomTabs", Dock = DockStyle.Bottom, Height = 280, TabIndex = 30 };

        _anomalyTab = new TabPage { Name = "anomalyTab", Text = "Anomalies" };
        _findingsGrid = new DataGridView
        {
            Name = "findingsGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            AccessibleName = "Anomaly findings",
            TabIndex = 31
        };
        _findingsGrid.Columns.Add("Severity", "Severity");
        _findingsGrid.Columns.Add("Rule", "Rule");
        _findingsGrid.Columns.Add("Evidence", "Evidence");
        _findingsGrid.Columns.Add("Offset", "Offset");
        _anomalyTab.Controls.Add(_findingsGrid);

        _analyticsTab = new TabPage { Name = "analyticsTab", Text = "Analytics" };
        _analyticsPanel = new TableLayoutPanel { Name = "analyticsPanel", Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2, TabStop = false };
        _analyticsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        _analyticsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        _analyticsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.4f));
        _analyticsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 65f));
        _analyticsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
        _classificationHistogram = new OwnerDrawnHistogram { Name = "classificationHistogram", Dock = DockStyle.Fill, BackColor = Color.White, Title = "Classification histogram" };
        _anomalyPareto = new OwnerDrawnHistogram { Name = "anomalyPareto", Dock = DockStyle.Fill, BackColor = Color.White, Title = "Anomaly Pareto" };
        _throughputChart = new OwnerDrawnTimeSeries { Name = "throughputChart", Dock = DockStyle.Fill, BackColor = Color.White, Title = "Throughput per hour" };
        _reconciliationLabel = new Label
        {
            Name = "reconciliationLabel",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(8),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            AccessibleName = "Journal vs archive reconciliation",
            TabStop = false
        };
        _analyticsPanel.Controls.Add(_classificationHistogram, 0, 0);
        _analyticsPanel.Controls.Add(_anomalyPareto, 1, 0);
        _analyticsPanel.Controls.Add(_throughputChart, 2, 0);
        _analyticsPanel.SetColumnSpan(_reconciliationLabel, 3);
        _analyticsPanel.Controls.Add(_reconciliationLabel, 0, 1);
        _analyticsTab.Controls.Add(_analyticsPanel);

        _correlationTab = new TabPage { Name = "correlationTab", Text = "Correlation strip" };
        _correlationTab.Controls.Add(new Label
        {
            Name = "correlationHintLabel",
            Dock = DockStyle.Fill,
            Text = "XFS trace ⇄ journal ⇄ server archive",
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(71, 85, 105),
            TabStop = false
        });

        _bulkTab = new TabPage { Name = "bulkTab", Text = "Bulk folder analysis" };
        _bulkPanel = new TableLayoutPanel { Name = "bulkPanel", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, TabStop = false };
        _bulkPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        _bulkPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _bulkToolbarPanel = new Panel { Name = "bulkToolbarPanel", Dock = DockStyle.Fill, Padding = new Padding(6) };
        _bulkStatusLabel = new Label
        {
            Name = "bulkStatusLabel",
            Text = "No bulk run yet — use “Bulk analyze folder…”.",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Location = new Point(6, 12),
            Width = 420,
            Height = 22,
            TabStop = false
        };
        _bulkProgressBar = new ProgressBar
        {
            Name = "bulkProgressBar",
            Minimum = 0,
            Maximum = 0,
            Style = ProgressBarStyle.Blocks,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(432, 10),
            Size = new Size(180, 24)
        };
        _bulkCancelButton = new Button
        {
            Name = "bulkCancelButton",
            Text = "Cancel",
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Size = new Size(75, 26),
            Location = new Point(618, 9),
            Enabled = false,
            TabIndex = 40
        };
        _exportBulkCsvButton = new Button
        {
            Name = "exportBulkCsvButton",
            Text = "Export CSV",
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Size = new Size(85, 26),
            Location = new Point(699, 9),
            Enabled = false,
            TabIndex = 41
        };
        _exportBulkExcelButton = new Button
        {
            Name = "exportBulkExcelButton",
            Text = "Export Excel",
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Size = new Size(92, 26),
            Location = new Point(790, 9),
            Enabled = false,
            TabIndex = 42
        };
        _bulkToolbarPanel.Controls.Add(_exportBulkExcelButton);
        _bulkToolbarPanel.Controls.Add(_exportBulkCsvButton);
        _bulkToolbarPanel.Controls.Add(_bulkCancelButton);
        _bulkToolbarPanel.Controls.Add(_bulkProgressBar);
        _bulkToolbarPanel.Controls.Add(_bulkStatusLabel);

        _bulkGrid = new DataGridView
        {
            Name = "bulkGrid",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            AccessibleName = "Bulk journal analysis results (double-click a row to load it)",
            TabIndex = 43
        };
        _bulkGrid.Columns.Add("File", "File");
        _bulkGrid.Columns.Add("Vendor", "Vendor");
        _bulkGrid.Columns.Add("Parser", "Parser");
        _bulkGrid.Columns.Add("Lines", "Lines");
        _bulkGrid.Columns.Add("Transactions", "Txns");
        _bulkGrid.Columns.Add("Success", "Success");
        _bulkGrid.Columns.Add("Suspicious", "Suspicious");
        _bulkGrid.Columns.Add("TotalAmount", "Total amount");
        _bulkGrid.Columns.Add("Status", "Status");

        _bulkPanel.Controls.Add(_bulkToolbarPanel, 0, 0);
        _bulkPanel.Controls.Add(_bulkGrid, 0, 1);
        _bulkTab.Controls.Add(_bulkPanel);

        _bottomTabs.TabPages.Add(_anomalyTab);
        _bottomTabs.TabPages.Add(_analyticsTab);
        _bottomTabs.TabPages.Add(_correlationTab);
        _bottomTabs.TabPages.Add(_bulkTab);

        // ══ z-order assembly (Dock rules: Fill last, top-strip rows after) ═════
        Controls.Add(_centreSplit);
        Controls.Add(_toolbarPanel);
        Controls.Add(_filterPanel);
        Controls.Add(_bottomTabs);

        ResumeLayout(false);
    }
}
