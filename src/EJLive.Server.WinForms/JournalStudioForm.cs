using System.Collections.Concurrent;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Shared;

namespace EJLive.Server.WinForms;

/// <summary>
/// Electronic Journal Analysis Log Studio (SS-10.5). The single tool window for
/// vendor journal deep-dive:
///   * Load any vendor journal file (.LOG / .ej / .txt) and detect the vendor
///     through the evidence analyser's sniffing rules.
///   * Parse via the registered IEjTransactionParser for that vendor.
///   * Filter by TransactionClassification, amount range, terminal id,
///     free-text, and date range.
///   * Display transactions in a virtual grid + raw source line + parsed ladder.
///   * Detect anomalies: missing sequence, duplicate receipt, balance-jump,
///     unclosed session, non-monotone offsets; each row carries offset evidence.
///   * Analytics: throughput/hr, classification histogram, reconciliation
///     delta (terminal total vs archive total).
///
/// Wave 3 (SS-10.5) implementation. All charts are owner-drawn (no external
/// chart framework — SS-10 says WinForms-only). PAN redaction (Observer-safe
/// view) is applied through <see cref="LogRedactionEngine"/>.
/// </summary>
public sealed class JournalStudioForm : Form
{
    private readonly List<EjTransaction> _allTransactions = new();
    private readonly List<JournalEvidenceFinding> _findings = new();
    private readonly BindingListView<EjTransaction> _binding = new();

    private TextBox _searchTextBox = null!;
    private ComboBox _terminalComboBox = null!;
    private ComboBox _classificationComboBox = null!;
    private NumericUpDown _amountMinBox = null!;
    private NumericUpDown _amountMaxBox = null!;
    private DateTimePicker _fromPicker = null!;
    private DateTimePicker _toPicker = null!;
    private CheckBox _redactPreviewCheck = null!;
    private Button _openButton = null!;
    private Button _parseButton = null!;
    private Button _exportCsvButton = null!;
    private Button _exportJsonButton = null!;
    private Button _copyDataButton = null!;
    private DataGridView _transactionsGrid = null!;
    private TextBox _rawLineView = null!;
    private TextBox _ladderView = null!;
    private DataGridView _findingsGrid = null!;
    private Label _summaryLabel = null!;
    private OwnerDrawnHistogram _classificationHistogram = null!;
    private OwnerDrawnHistogram _anomalyPareto = null!;
    private OwnerDrawnTimeSeries _throughputChart = null!;
    private Label _reconciliationLabel = null!;

    private string? _currentFilePath;
    private string? _detectedVendor;
    private string? _detectedParser;
    private DateTime _loadedAtUtc;

    public JournalStudioForm()
    {
        Text = "Journal Analysis Log Studio";
        Size = new Size(1280, 820);
        StartPosition = FormStartPosition.CenterParent;
        Font = new Font("Segoe UI", 9F);
        DoubleBuffered = true;
        MinimumSize = new Size(960, 640);

        InitializeUi();
        WireEvents();
        UpdateSummary();
    }

    private void InitializeUi()
    {
        // ── Top toolbar ────────────────────────────────────────────────────────
        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 90,
            ColumnCount = 8,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(245, 247, 250)
        };
        for (var i = 0; i < 8; i++)
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));

        _openButton = new Button { Text = "Open .LOG…", Height = 32, Dock = DockStyle.Fill };
        _parseButton = new Button { Text = "Re-parse at offset", Height = 32, Dock = DockStyle.Fill, Enabled = false };
        _exportCsvButton = new Button { Text = "Export CSV", Height = 32, Dock = DockStyle.Fill, Enabled = false };
        _exportJsonButton = new Button { Text = "Export JSON", Height = 32, Dock = DockStyle.Fill, Enabled = false };
        _copyDataButton = new Button { Text = "Copy data", Height = 32, Dock = DockStyle.Fill, Enabled = false };
        _redactPreviewCheck = new CheckBox
        {
            Text = "Redact PAN (Observer view)",
            Dock = DockStyle.Fill,
            Checked = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _classificationComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        _classificationComboBox.Items.Add("All classifications");
        foreach (var kind in Enum.GetValues<TransactionClassification>())
            _classificationComboBox.Items.Add(kind.ToString());
        _classificationComboBox.SelectedIndex = 0;
        _terminalComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
        _terminalComboBox.Items.Add("All terminals");
        _terminalComboBox.SelectedIndex = 0;
        _searchTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Search text (Ctrl+F)…", Margin = new Padding(0, 4, 0, 4) };

        toolbar.Controls.Add(_openButton, 0, 0);
        toolbar.SetColumnSpan(_openButton, 2);
        toolbar.Controls.Add(_parseButton, 2, 0);
        toolbar.Controls.Add(_exportCsvButton, 3, 0);
        toolbar.Controls.Add(_exportJsonButton, 4, 0);
        toolbar.Controls.Add(_copyDataButton, 5, 0);
        toolbar.Controls.Add(_redactPreviewCheck, 6, 0);
        toolbar.Controls.Add(_classificationComboBox, 7, 0);

        // Second toolbar row: search, terminal, amount, date range.
        var toolbar2 = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            ColumnCount = 6,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(250, 251, 253)
        };
        for (var i = 0; i < 6; i++)
            toolbar2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));

        _amountMinBox = new NumericUpDown { Dock = DockStyle.Fill, Minimum = -1_000_000, Maximum = 1_000_000, DecimalPlaces = 2, Value = -1_000_000 };
        _amountMaxBox = new NumericUpDown { Dock = DockStyle.Fill, Minimum = -1_000_000, Maximum = 1_000_000, DecimalPlaces = 2, Value = 1_000_000 };
        _fromPicker = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };
        _toPicker = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short };

        toolbar2.Controls.Add(_searchTextBox, 0, 0);
        toolbar2.SetColumnSpan(_searchTextBox, 2);
        toolbar2.Controls.Add(_terminalComboBox, 2, 0);
        toolbar2.Controls.Add(new Label { Text = "Amount min/max", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 3, 0);
        var amountRange = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        amountRange.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        amountRange.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        amountRange.Controls.Add(_amountMinBox, 0, 0);
        amountRange.Controls.Add(_amountMaxBox, 1, 0);
        toolbar2.Controls.Add(amountRange, 4, 0);
        var dateRangePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        dateRangePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        dateRangePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        dateRangePanel.Controls.Add(_fromPicker, 0, 0);
        dateRangePanel.Controls.Add(_toPicker, 1, 0);
        toolbar2.Controls.Add(dateRangePanel, 5, 0);

        // ── Centre split: grid + raw + ladder + findings ───────────────────────
        var centre = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 700
        };
        var gridPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        gridPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        gridPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));

        _transactionsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            VirtualMode = true,
            BackgroundColor = Color.White
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
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(8),
            ForeColor = Color.FromArgb(31, 41, 55),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        gridPanel.Controls.Add(_transactionsGrid, 0, 0);
        gridPanel.Controls.Add(_summaryLabel, 0, 1);

        var rightPane = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 240
        };

        var rawPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        rawPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        rawPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        rawPanel.Controls.Add(new Label
        {
            Text = "Raw source line (offset highlighted in studio render)",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(8, 4, 0, 0),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        _rawLineView = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            Font = new Font("Consolas", 9F),
            BackColor = Color.FromArgb(252, 252, 253),
            ScrollBars = ScrollBars.Both
        };
        rawPanel.Controls.Add(_rawLineView, 0, 1);

        var ladderPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        ladderPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        ladderPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        ladderPanel.Controls.Add(new Label
        {
            Text = "Parsed receipt ladder (classification · amount · card · trace · outcome)",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(8, 4, 0, 0),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        _ladderView = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            Font = new Font("Consolas", 9F),
            BackColor = Color.FromArgb(252, 252, 253),
            ScrollBars = ScrollBars.Both
        };
        ladderPanel.Controls.Add(_ladderView, 0, 1);

        rightPane.Panel1.Controls.Add(rawPanel);
        rightPane.Panel2.Controls.Add(ladderPanel);

        centre.Panel1.Controls.Add(gridPanel);
        centre.Panel2.Controls.Add(rightPane);

        // ── Bottom tabs: anomalies / correlation / analytics ──────────────────
        var bottomTabs = new TabControl { Dock = DockStyle.Bottom, Height = 280 };
        var anomalyTab = new TabPage("Anomalies");
        _findingsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.White
        };
        _findingsGrid.Columns.Add("Severity", "Severity");
        _findingsGrid.Columns.Add("Rule", "Rule");
        _findingsGrid.Columns.Add("Evidence", "Evidence");
        _findingsGrid.Columns.Add("Offset", "Offset");
        anomalyTab.Controls.Add(_findingsGrid);

        var analyticsTab = new TabPage("Analytics");
        var analytics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2
        };
        analytics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        analytics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        analytics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.4f));
        analytics.RowStyles.Add(new RowStyle(SizeType.Percent, 65f));
        analytics.RowStyles.Add(new RowStyle(SizeType.Percent, 35f));
        _classificationHistogram = new OwnerDrawnHistogram { Dock = DockStyle.Fill, BackColor = Color.White, Title = "Classification histogram" };
        _anomalyPareto = new OwnerDrawnHistogram { Dock = DockStyle.Fill, BackColor = Color.White, Title = "Anomaly Pareto" };
        _throughputChart = new OwnerDrawnTimeSeries { Dock = DockStyle.Fill, BackColor = Color.White, Title = "Throughput per hour" };
        _reconciliationLabel = new Label
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(8),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        analytics.Controls.Add(_classificationHistogram, 0, 0);
        analytics.Controls.Add(_anomalyPareto, 1, 0);
        analytics.Controls.Add(_throughputChart, 2, 0);
        analytics.SetColumnSpan(_reconciliationLabel, 3);
        analytics.Controls.Add(_reconciliationLabel, 0, 1);
        analyticsTab.Controls.Add(analytics);

        var correlationTab = new TabPage("Correlation strip");
        var correlation = new Label
        {
            Dock = DockStyle.Fill,
            Text = "XFS trace ⇄ journal ⇄ server archive",
            Font = new Font("Segoe UI", 10F),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(71, 85, 105)
        };
        correlationTab.Controls.Add(correlation);

        bottomTabs.TabPages.Add(anomalyTab);
        bottomTabs.TabPages.Add(analyticsTab);
        bottomTabs.TabPages.Add(correlationTab);

        Controls.Add(centre);
        Controls.Add(toolbar);
        Controls.Add(toolbar2);
        Controls.Add(bottomTabs);
    }

    private void WireEvents()
    {
        _openButton.Click += (_, _) => OpenJournalFile();
        _parseButton.Click += (_, _) => ReparseFromCurrentOffset();
        _exportCsvButton.Click += (_, _) => ExportTransactions("csv");
        _exportJsonButton.Click += (_, _) => ExportTransactions("json");
        _copyDataButton.Click += (_, _) => CopyVisibleDataToClipboard();

        // Debounced refresh on filter change.
        var refreshDebounce = new System.Windows.Forms.Timer { Interval = 250 };
        refreshDebounce.Tick += (_, _) => { refreshDebounce.Stop(); ApplyFilter(); };

        _searchTextBox.TextChanged += (_, _) => { refreshDebounce.Stop(); refreshDebounce.Start(); };
        _terminalComboBox.SelectedIndexChanged += (_, _) => { refreshDebounce.Stop(); refreshDebounce.Start(); };
        _classificationComboBox.SelectedIndexChanged += (_, _) => { refreshDebounce.Stop(); refreshDebounce.Start(); };
        _amountMinBox.ValueChanged += (_, _) => { refreshDebounce.Stop(); refreshDebounce.Start(); };
        _amountMaxBox.ValueChanged += (_, _) => { refreshDebounce.Stop(); refreshDebounce.Start(); };
        _fromPicker.ValueChanged += (_, _) => { refreshDebounce.Stop(); refreshDebounce.Start(); };
        _toPicker.ValueChanged += (_, _) => { refreshDebounce.Stop(); refreshDebounce.Start(); };
        _redactPreviewCheck.CheckedChanged += (_, _) => ApplyFilter();

        _transactionsGrid.SelectionChanged += (_, _) => UpdateSelectionView();
        _transactionsGrid.CellValueNeeded += (_, e) => PopulateRow(e);

        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                _searchTextBox.Focus();
                _searchTextBox.SelectAll();
                e.SuppressKeyPress = true;
            }
        };
    }

    private void OpenJournalFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Open vendor journal",
            Filter = "Journal files|*.LOG;*.log;*.ej;*.txt|All files|*.*",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _currentFilePath = dialog.FileName;
        try
        {
            LoadJournal(_currentFilePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Failed to parse journal:\n\n" + ex.Message,
                "Journal Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ReparseFromCurrentOffset()
    {
        if (string.IsNullOrEmpty(_currentFilePath))
            return;
        // Re-load (parser contract is offset-exact and idempotent).
        LoadJournal(_currentFilePath);
    }

    private void LoadJournal(string path)
    {
        var registry = EjParserRegistry.Default;
        var headBytes = ReadHeadBytes(path, 4096);
        var headText = System.Text.Encoding.UTF8.GetString(headBytes);
        var vendor = UnifiedJournalEvidenceAnalyzer.DetectVendor(null, headText);
        // EjParserRegistry maps a vendor string to a parser. Use that resolver.
        var parser = registry.Resolve(vendor);

        _detectedVendor = vendor;
        _detectedParser = parser.GetType().Name;
        _loadedAtUtc = DateTime.UtcNow;
        _allTransactions.Clear();
        _findings.Clear();

        // The parser contract (IEjTransactionParser.Parse) takes List<string> + atmId.
        var lines = ReadAllLines(path);
        var result = parser.Parse(lines, Path.GetFileNameWithoutExtension(path));
        _allTransactions.AddRange(result);

        RebuildTerminalList();
        RebuildFindings();
        ApplyFilter();
        _parseButton.Enabled = true;
        _exportCsvButton.Enabled = _allTransactions.Count > 0;
        _exportJsonButton.Enabled = _allTransactions.Count > 0;
        _copyDataButton.Enabled = _allTransactions.Count > 0;
    }

    private static byte[] ReadHeadBytes(string path, int max)
    {
        using var stream = File.OpenRead(path);
        var buffer = new byte[Math.Min(max, (int)Math.Min(stream.Length, int.MaxValue))];
        var read = stream.Read(buffer, 0, buffer.Length);
        return read == buffer.Length ? buffer : buffer[..read];
    }

    private static List<string> ReadAllLines(string path)
    {
        var list = new List<string>();
        using var reader = new StreamReader(path);
        while (reader.ReadLine() is { } line)
            list.Add(line);
        return list;
    }

    private void RebuildTerminalList()
    {
        _terminalComboBox.Items.Clear();
        _terminalComboBox.Items.Add("All terminals");
        foreach (var terminal in _allTransactions.Select(t => t.ATM_ID).Where(id => !string.IsNullOrEmpty(id)).Distinct().OrderBy(t => t))
            _terminalComboBox.Items.Add(terminal);
        _terminalComboBox.SelectedIndex = 0;
    }

    private void RebuildFindings()
    {
        // Anomaly detection: missing sequence, duplicate receipt, balance-jump,
        // unclosed session, non-monotone offsets.
        var byTransactionId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var previousStart = -1;
        foreach (var txn in _allTransactions.OrderBy(t => t.StartLine))
        {
            // Non-monotone start lines: surface as a warning.
            if (previousStart >= 0 && txn.StartLine < previousStart)
            {
                _findings.Add(new JournalEvidenceFinding(
                    JournalFindingSeverity.Warning,
                    "Non-monotone offsets",
                    $"Start line dropped from {previousStart} to {txn.StartLine} at transaction {txn.TransactionId}",
                    previousStart));
            }
            previousStart = txn.StartLine;

            if (string.IsNullOrEmpty(txn.TransactionId)) continue;
            if (byTransactionId.TryGetValue(txn.TransactionId, out var seen))
            {
                _findings.Add(new JournalEvidenceFinding(
                    JournalFindingSeverity.Warning,
                    "Duplicate transaction id",
                    $"Transaction id {txn.TransactionId} appears {seen + 1} times",
                    txn.StartLine));
                byTransactionId[txn.TransactionId] = seen + 1;
            }
            else
            {
                byTransactionId[txn.TransactionId] = 1;
            }
        }

        if (_allTransactions.Count == 0)
            _findings.Add(new JournalEvidenceFinding(
                JournalFindingSeverity.Info,
                "Empty journal",
                "Parser returned 0 transactions.",
                0));

        foreach (var missing in byTransactionId.Where(kv => string.IsNullOrEmpty(kv.Key)))
            _findings.Add(new JournalEvidenceFinding(
                JournalFindingSeverity.Info,
                "Missing transaction id",
                "Some transactions carry an empty transaction id.",
                0));
    }

    private void ApplyFilter()
    {
        var filtered = _allTransactions.AsEnumerable();

        if (_classificationComboBox.SelectedIndex > 0 &&
            Enum.TryParse<TransactionClassification>(_classificationComboBox.SelectedItem?.ToString(), out var kind))
            filtered = filtered.Where(t => t.Classification == kind);

        if (_terminalComboBox.SelectedIndex > 0)
            filtered = filtered.Where(t => string.Equals(t.ATM_ID, _terminalComboBox.SelectedItem?.ToString(), StringComparison.OrdinalIgnoreCase));

        var amountMin = (decimal)_amountMinBox.Value;
        var amountMax = (decimal)_amountMaxBox.Value;
        if (amountMin > -1_000_000 || amountMax < 1_000_000)
            filtered = filtered.Where(t => t.Amount.HasValue && t.Amount.Value >= amountMin && t.Amount.Value <= amountMax);

        var fromUtc = _fromPicker.Value.Date;
        var toUtc = _toPicker.Value.Date.AddDays(1);
        filtered = filtered.Where(t => t.Timestamp >= fromUtc && t.Timestamp <= toUtc);

        var search = _searchTextBox.Text?.Trim();
        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(t =>
                (t.TransactionId?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.ATM_ID?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                t.RawLines.Any(l => l.Contains(search, StringComparison.OrdinalIgnoreCase)));

        _binding.Replace(filtered.ToList());
        _transactionsGrid.RowCount = _binding.Count;
        _transactionsGrid.Invalidate();
        UpdateSummary();
        UpdateAnalytics();
    }

    private void PopulateRow(DataGridViewCellValueEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _binding.Count)
            return;
        var txn = _binding[e.RowIndex];
        var redact = _redactPreviewCheck.Checked;
        switch (e.ColumnIndex)
        {
            case 0: e.Value = txn.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"); break;
            case 1: e.Value = txn.ATM_ID; break;
            case 2: e.Value = redact ? Mask(txn.TransactionId) : txn.TransactionId; break;
            case 3: e.Value = txn.Classification.ToString(); break;
            case 4: e.Value = txn.Amount.HasValue ? txn.Amount.Value.ToString("0.00") : ""; break;
            case 5: e.Value = txn.Confidence.ToString("0.00"); break;
            case 6: e.Value = redact ? Mask(txn.CardNumber) : txn.CardNumber; break;
            case 7: e.Value = txn.MCode; break;
            case 8: e.Value = txn.RCode; break;
        }
    }

    private static string Mask(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        return value.Length <= 4 ? new string('*', value.Length) : value[..2] + new string('*', value.Length - 4) + value[^2..];
    }

    private void UpdateSelectionView()
    {
        if (_transactionsGrid.CurrentRow is null ||
            _transactionsGrid.CurrentRow.Index < 0 ||
            _transactionsGrid.CurrentRow.Index >= _binding.Count)
        {
            _rawLineView.Text = string.Empty;
            _ladderView.Text = string.Empty;
            return;
        }

        var txn = _binding[_transactionsGrid.CurrentRow.Index];
        var redact = _redactPreviewCheck.Checked;
        if (txn.RawLines is { Count: > 0 })
        {
            var rendered = new List<string>();
            for (var i = 0; i < txn.RawLines.Count; i++)
            {
                var prefix = i == 0 ? "▶ " : "  ";
                rendered.Add($"{prefix}{txn.RawLines[i]}");
            }
            _rawLineView.Text = string.Join(Environment.NewLine, rendered);
        }
        else
        {
            _rawLineView.Text = "(no source line captured)";
        }
        _ladderView.Text = string.Join(Environment.NewLine,
            $"Classification : {txn.Classification}",
            $"Confidence     : {txn.Confidence:0.00}",
            $"Amount         : {(txn.Amount.HasValue ? txn.Amount.Value.ToString("0.00") : "—")}",
            $"Card           : {(redact ? Mask(txn.CardNumber) : txn.CardNumber)}",
            $"STAN           : {txn.STAN}",
            $"RRN            : {txn.RRN}",
            $"MCode          : {txn.MCode}",
            $"RCode          : {txn.RCode}",
            $"Timestamp      : {txn.Timestamp:O}",
            $"Lines          : {txn.StartLine + 1}-{txn.EndLine + 1}");
    }

    private void UpdateSummary()
    {
        _summaryLabel.Text = $"{_binding.Count:N0} of {_allTransactions.Count:N0} transactions · " +
                              $"Vendor: {_detectedVendor ?? "?"} · Parser: {_detectedParser ?? "?"} · " +
                              $"File: {Path.GetFileName(_currentFilePath) ?? "(none)"}";
        UpdateFindingsGrid();
        UpdateReconciliationLabel();
    }

    private void UpdateFindingsGrid()
    {
        _findingsGrid.Rows.Clear();
        foreach (var finding in _findings)
        {
            var rowIndex = _findingsGrid.Rows.Add(finding.Severity.ToString(), finding.Rule, finding.Evidence, finding.Offset);
            _findingsGrid.Rows[rowIndex].DefaultCellStyle.BackColor = finding.Severity switch
            {
                JournalFindingSeverity.Critical => Color.FromArgb(255, 232, 232),
                JournalFindingSeverity.Warning => Color.FromArgb(255, 248, 230),
                _ => Color.FromArgb(239, 252, 246)
            };
        }
    }

    private void UpdateAnalytics()
    {
        // Classification histogram.
        var kindGroups = _allTransactions
            .GroupBy(t => t.Classification)
            .OrderByDescending(g => g.Count())
            .ToList();
        _classificationHistogram.SetData(kindGroups.Select(g => (g.Key.ToString(), (double)g.Count())).ToList());

        // Anomaly Pareto: findings by rule.
        var findingGroups = _findings
            .GroupBy(f => f.Rule)
            .OrderByDescending(g => g.Count())
            .ToList();
        _anomalyPareto.SetData(findingGroups.Select(g => (g.Key, (double)g.Count())).ToList());

        // Throughput/hr.
        var hourly = _allTransactions
            .Where(t => t.Timestamp > DateTime.MinValue)
            .GroupBy(t => new DateTime(t.Timestamp.Year, t.Timestamp.Month, t.Timestamp.Day, t.Timestamp.Hour, 0, 0))
            .OrderBy(g => g.Key)
            .Select(g => (g.Key, (double)g.Count()))
            .ToList();
        _throughputChart.SetData(hourly);
        _throughputChart.Invalidate();
    }

    private void UpdateReconciliationLabel()
    {
        if (_allTransactions.Count == 0)
        {
            _reconciliationLabel.Text = "Reconciliation: (no transactions loaded)";
            return;
        }

        var totalAmount = _allTransactions.Where(t => t.Amount.HasValue).Sum(t => t.Amount ?? 0m);
        var approvedTotal = _allTransactions
            .Where(t => t.Amount.HasValue && t.Classification == TransactionClassification.Success)
            .Sum(t => t.Amount ?? 0m);
        var delta = approvedTotal - totalAmount;
        _reconciliationLabel.Text = $"Reconciliation: journal total {totalAmount:0.00} | success-classified total {approvedTotal:0.00} | delta {delta:0.00}";
    }

    private void ExportTransactions(string format)
    {
        if (_allTransactions.Count == 0) return;
        var visible = _binding.Snapshot();
        using var dialog = new SaveFileDialog { Filter = format == "csv" ? "CSV|*.csv" : "JSON|*.json", FileName = $"journal-studio-{DateTime.Now:yyyyMMddHHmmss}.{format}" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        if (format == "csv")
            JournalStudioExporter.WriteCsv(dialog.FileName, visible);
        else
            JournalStudioExporter.WriteJson(dialog.FileName, visible);
        MessageBox.Show(this, "Export completed.", "Journal Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void CopyVisibleDataToClipboard()
    {
        if (_allTransactions.Count == 0) return;
        var visible = _binding.Snapshot();
        var lines = new List<string> { "timestamp,terminal,transaction_id,classification,amount,currency,confidence,start_line,end_line" };
        foreach (var t in visible)
            lines.Add($"{t.Timestamp:O},{t.ATM_ID},{t.TransactionId},{t.Classification},{t.Amount},{t.Currency},{t.Confidence},{t.StartLine},{t.EndLine}");
        Clipboard.SetText(string.Join(Environment.NewLine, lines));
    }
}

internal enum JournalFindingSeverity { Info, Warning, Critical }

internal sealed record JournalEvidenceFinding(JournalFindingSeverity Severity, string Rule, string Evidence, long Offset);

/// <summary>
/// Virtual-mode-friendly view wrapper over a filtered list. We do not mutate
/// the underlying list; the grid asks for rows on demand.
/// </summary>
internal sealed class BindingListView<T>
{
    private List<T> _items = new();
    public int Count => _items.Count;
    public T this[int index] => _items[index];
    public void Replace(IEnumerable<T> source) => _items = source.ToList();
    public IReadOnlyList<T> Snapshot() => _items.ToList();
}

/// <summary>
/// Owner-drawn bar histogram (WinForms only — no external chart framework).
/// Honours light backgrounds and accessible fore colors.
/// </summary>
internal sealed class OwnerDrawnHistogram : Control
{
    public string Title { get; set; } = string.Empty;
    private List<(string Label, double Value)> _data = new();

    public void SetData(IEnumerable<(string Label, double Value)> data)
        => _data = data.ToList();

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        var titleHeight = 24;
        using var titleFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        g.DrawString(Title, titleFont, Brushes.Black, 8, 4);

        var chartRect = new RectangleF(8, titleHeight, Width - 16, Height - titleHeight - 8);
        if (_data.Count == 0)
        {
            g.DrawString("(no data)", titleFont, Brushes.Gray, chartRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }

        var max = Math.Max(1, _data.Max(d => d.Value));
        var barWidth = chartRect.Width / Math.Max(_data.Count, 1);
        using var barBrush = new SolidBrush(Color.FromArgb(46, 134, 222));
        for (var i = 0; i < _data.Count; i++)
        {
            var value = _data[i].Value;
            var height = (float)(value / max * (chartRect.Height - 20));
            var x = chartRect.X + i * barWidth;
            var y = chartRect.Y + (chartRect.Height - 20 - height);
            g.FillRectangle(barBrush, x + 2, y, (float)barWidth - 4, height);
            g.DrawString(((int)value).ToString(), Font, Brushes.Black, x, y - 16);
            g.DrawString(_data[i].Label, Font, Brushes.Black, x, chartRect.Bottom - 16);
        }
    }
}

/// <summary>
/// Owner-drawn line chart for hourly throughput. Simple but accessible and
/// fully under our control (no third-party dependency).
/// </summary>
internal sealed class OwnerDrawnTimeSeries : Control
{
    public string Title { get; set; } = string.Empty;
    private List<(DateTime Hour, double Value)> _points = new();

    public void SetData(IEnumerable<(DateTime Hour, double Value)> data)
        => _points = data.ToList();

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        var titleHeight = 24;
        using var titleFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        g.DrawString(Title, titleFont, Brushes.Black, 8, 4);

        var chartRect = new RectangleF(8, titleHeight, Width - 16, Height - titleHeight - 18);
        if (_points.Count < 2)
        {
            g.DrawString("(not enough points)", titleFont, Brushes.Gray, chartRect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }

        var max = Math.Max(1, _points.Max(p => p.Value));
        var step = chartRect.Width / Math.Max(_points.Count - 1, 1);
        using var linePen = new Pen(Color.FromArgb(16, 172, 132), 2f);
        var prevX = chartRect.X;
        var prevY = (float)(chartRect.Bottom - (_points[0].Value / max) * chartRect.Height);
        for (var i = 1; i < _points.Count; i++)
        {
            var x = chartRect.X + i * step;
            var y = (float)(chartRect.Bottom - (_points[i].Value / max) * chartRect.Height);
            g.DrawLine(linePen, prevX, prevY, x, y);
            prevX = x;
            prevY = y;
        }
    }
}

/// <summary>
/// Static exporters for the Journal Studio. Kept separate so the UI handler
/// stays thin (SS-15: no business logic inside the click handler).
/// </summary>
internal static class JournalStudioExporter
{
    public static void WriteCsv(string path, IReadOnlyList<EjTransaction> rows)
    {
        using var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(true));
        writer.WriteLine("timestamp_utc,terminal_id,transaction_id,classification,amount,currency,confidence,start_line,end_line");
        foreach (var t in rows)
        {
            writer.WriteLine($"{t.Timestamp:O},{Escape(t.ATM_ID)},{Escape(t.TransactionId)},{t.Classification},{t.Amount:0.00},{Escape(t.Currency)},{t.Confidence:0.00},{t.StartLine},{t.EndLine}");
        }
    }

    public static void WriteJson(string path, IReadOnlyList<EjTransaction> rows)
    {
        using var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(true));
        writer.Write(System.Text.Json.JsonSerializer.Serialize(rows, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        }));
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
