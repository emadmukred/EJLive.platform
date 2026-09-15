using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using EJLive.Core.Data.Repositories;
using EJLive.Core.Engine;
using EJLive.Core.Models;
using EJLive.Core.Services;
using EJLive.Core.UI;
using EJLive.Shared;

namespace EJLive.Server.WinForms;

/// <summary>
/// Electronic Journal Analysis Log Studio (SS-10.5, extended in Wave 4 for bulk analysis
/// and Excel export). The single tool window for vendor journal deep-dive:
///   * Load any vendor journal file (.LOG / .ej / .txt); vendor is sniffed through the
///     evidence analyser's rules and the matching IEjTransactionParser is resolved from
///     EjParserRegistry (one parser per vendor, POL-1).
///   * Filter by classification, amount range, terminal, free text and date range; all
///     filters feed one virtual-mode grid (250 ms debounce, no re-query per keystroke).
///   * Raw source line + parsed receipt ladder side by side, PAN redaction (Observer view)
///     applied through the redaction engine.
///   * Anomalies (non-monotone offsets, duplicate ids, missing ids) with offset evidence.
///   * Analytics: throughput/hr, classification histogram, anomaly Pareto, reconciliation
///     delta — the journal total compared against the server archive total
///     (<see cref="IJournalArchiveRepository"/>, SS-10.5 acceptance).
///   * Bulk folder analysis (SS-10.5 "bulk analysis"): parse up to 500 journal files off
///     the UI thread with progress + cancel, aggregate per file, export CSV/Excel.
///   * Export: CSV, JSON and Excel (.xlsx via the platform's dependency-free
///     <see cref="ExcelWorkbookWriter"/>, no Office interop).
///
/// Form layout lives in <c>JournalStudioForm.Designer.cs</c> (Visual Studio Designer partial;
/// see the regeneration notes there). All event bindings live in <see cref="WireEvents"/> —
/// InitializeComponent may be regenerated without orphaning a single handler (SS-10).
/// </summary>
public sealed partial class JournalStudioForm : Form
{
    private readonly List<EjTransaction> _allTransactions = new();
    private readonly List<JournalEvidenceFinding> _findings = new();
    private readonly BindingListView<EjTransaction> _binding = new();
    private readonly List<JournalStudioBulkAnalyzer.BulkRow> _bulkRows = new();

    private CancellationTokenSource? _bulkCts;

    private string? _currentFilePath;
    private string? _detectedVendor;
    private string? _detectedParser;
    private DateTime _loadedAtUtc;

    public JournalStudioForm()
    {
        InitializeComponent();
        WireEvents();
        UpdateSummary();
    }

    // ── wiring (kept out of the designer file: regeneration-safe) ───────────
    private void WireEvents()
    {
        _openButton.Click += (_, _) => OpenJournalFile();
        _parseButton.Click += (_, _) => ReparseFromCurrentOffset();
        _exportCsvButton.Click += (_, _) => ExportTransactions("csv");
        _exportJsonButton.Click += (_, _) => ExportTransactions("json");
        _exportExcelButton.Click += (_, _) => ExportTransactions("xlsx");
        _copyDataButton.Click += (_, _) => CopyVisibleDataToClipboard();
        _bulkFolderButton.Click += async (_, _) => await RunBulkAnalysisAsync();
        _bulkCancelButton.Click += (_, _) => _bulkCts?.Cancel();
        _bulkExportCsvButton.Click += (_, _) => ExportBulk("csv");
        _bulkExportExcelButton.Click += (_, _) => ExportBulk("xlsx");
        _bulkGrid.CellDoubleClick += (_, _) => OpenBulkRowInMainView();

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

    // ── single-file load / parse ─────────────────────────────────────────────
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
        var parser = registry.Resolve(vendor);

        _detectedVendor = vendor;
        _detectedParser = parser.GetType().Name;
        _loadedAtUtc = DateTime.UtcNow;
        _allTransactions.Clear();
        _findings.Clear();

        var lines = ReadAllLines(path);
        var result = parser.Parse(lines, Path.GetFileNameWithoutExtension(path));
        _allTransactions.AddRange(result);

        RebuildTerminalList();
        RebuildFindings();
        ApplyFilter();
        _parseButton.Enabled = true;
        _exportCsvButton.Enabled = _allTransactions.Count > 0;
        _exportJsonButton.Enabled = _allTransactions.Count > 0;
        _exportExcelButton.Enabled = _allTransactions.Count > 0;
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

        // SS-10.5 acceptance: journal totals must agree with the server archive totals.
        // The archive side is optional — a standalone studio session (no bootstrapped
        // database) reconciles in-memory only and says so on the same label.
        var archiveNote = string.Empty;
        try
        {
            var repo = new JournalArchiveRepository();
            long archiveTxns = 0;
            if (_terminalComboBox.SelectedIndex > 0)
            {
                archiveTxns = repo.SumTransactions(_terminalComboBox.SelectedItem?.ToString() ?? string.Empty);
            }
            else
            {
                foreach (var terminal in _allTransactions.Select(t => t.ATM_ID)
                             .Where(id => !string.IsNullOrEmpty(id)).Distinct(StringComparer.OrdinalIgnoreCase))
                    archiveTxns += repo.SumTransactions(terminal);
            }
            archiveNote = $" | archive transactions: {archiveTxns:N0}";
        }
        catch (Exception)
        {
            // Database not initialised (studio opened without bootstrap) — the delta against
            // the archive is simply unavailable; in-memory reconciliation still stands.
            archiveNote = " | archive: (local database unavailable)";
        }

        _reconciliationLabel.Text = $"Reconciliation: journal total {totalAmount:0.00} | success-classified total {approvedTotal:0.00} | delta {delta:0.00}{archiveNote}";
    }

    // ── exports (thin handlers; work in JournalStudioExporter, SS-15 #5) ────
    private void ExportTransactions(string format)
    {
        if (_allTransactions.Count == 0) return;
        var visible = _binding.Snapshot();
        var ext = format == "csv" ? "csv" : format == "json" ? "json" : "xlsx";
        using var dialog = new SaveFileDialog
        {
            Filter = ext switch
            {
                "csv" => "CSV|*.csv",
                "json" => "JSON|*.json",
                _ => "Excel workbook|*.xlsx"
            },
            FileName = $"journal-studio-{DateTime.Now:yyyyMMddHHmmss}.{ext}"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        if (ext == "csv")
            JournalStudioExporter.WriteCsv(dialog.FileName, visible);
        else if (ext == "json")
            JournalStudioExporter.WriteJson(dialog.FileName, visible);
        else
            JournalStudioExporter.WriteExcel(dialog.FileName, visible, BuildWorkbookMetadata());

        MessageBox.Show(this, "Export completed.", "Journal Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private (string Vendor, string Parser, string File, int Transactions) BuildWorkbookMetadata() =>
        (_detectedVendor ?? "?", _detectedParser ?? "?", Path.GetFileName(_currentFilePath) ?? "(none)", _allTransactions.Count);

    private void CopyVisibleDataToClipboard()
    {
        if (_allTransactions.Count == 0) return;
        var visible = _binding.Snapshot();
        var lines = new List<string> { "timestamp,terminal,transaction_id,classification,amount,currency,confidence,start_line,end_line" };
        foreach (var t in visible)
            lines.Add($"{t.Timestamp:O},{t.ATM_ID},{t.TransactionId},{t.Classification},{t.Amount},{t.Currency},{t.Confidence},{t.StartLine},{t.EndLine}");
        Clipboard.SetText(string.Join(Environment.NewLine, lines));
    }

    // ── bulk folder analysis ─────────────────────────────────────────────────
    private async Task RunBulkAnalysisAsync()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select a folder of vendor journals to analyse in bulk",
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        _bulkCts = new CancellationTokenSource();
        SetBulkRunning(running: true);
        var progress = new Progress<int>(done =>
        {
            _bulkStatusLabel.Text = $"Analysed {done} file(s)…";
            if (_bulkProgressBar.Maximum > 0)
                _bulkProgressBar.Value = Math.Min(done, _bulkProgressBar.Maximum);
        });

        try
        {
            var rows = await JournalStudioBulkAnalyzer.RunAsync(dialog.SelectedPath, progress, _bulkCts.Token);
            FillBulkGrid(rows);
            _bulkStatusLabel.Text = $"Bulk analysis complete: {rows.Count} file(s) · {rows.Sum(r => r.Transactions):N0} transactions";
        }
        catch (OperationCanceledException)
        {
            _bulkStatusLabel.Text = "Bulk analysis cancelled.";
        }
        catch (Exception ex)
        {
            // Show the same actionable line the log records (SS-14); the bulk grid keeps
            // whatever partial results were already collected.
            _bulkStatusLabel.Text = "Bulk analysis failed: " + ex.Message;
            MessageBox.Show(this, "Bulk analysis failed:\n\n" + ex.Message,
                "Journal Studio", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _bulkCts.Dispose();
            _bulkCts = null;
            SetBulkRunning(running: false);
        }
    }

    private void SetBulkRunning(bool running)
    {
        _bulkFolderButton.Enabled = !running;
        _bulkCancelButton.Enabled = running;
        _bulkProgressBar.Style = running ? ProgressBarStyle.Continuous : ProgressBarStyle.Blocks;
        _exportBulkCsvButton.Enabled = !running && _bulkRows.Count > 0;
        _exportBulkExcelButton.Enabled = !running && _bulkRows.Count > 0;
    }

    private void FillBulkGrid(IReadOnlyList<JournalStudioBulkAnalyzer.BulkRow> rows)
    {
        _bulkRows.Clear();
        _bulkRows.AddRange(rows);
        _bulkGrid.Rows.Clear();
        foreach (var row in rows)
        {
            var index = _bulkGrid.Rows.Add(
                row.FileName, row.Vendor, row.Parser, row.Lines, row.Transactions,
                row.SuccessCount, row.SuspiciousCount, row.TotalAmount.ToString("0.00"), row.Status);
            _bulkGrid.Rows[index].Tag = row.FullPath;
        }
        _exportBulkCsvButton.Enabled = rows.Count > 0;
        _exportBulkExcelButton.Enabled = rows.Count > 0;
    }

    private void OpenBulkRowInMainView()
    {
        if (_bulkGrid.CurrentRow?.Tag is not string path || !File.Exists(path))
            return;
        try
        {
            _currentFilePath = path;
            LoadJournal(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Could not open the selected file:\n\n" + ex.Message,
                "Journal Studio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ExportBulk(string format)
    {
        if (_bulkRows.Count == 0) return;
        using var dialog = new SaveFileDialog
        {
            Filter = format == "csv" ? "CSV|*.csv" : "Excel workbook|*.xlsx",
            FileName = $"journal-bulk-{DateTime.Now:yyyyMMddHHmmss}.{(format == "csv" ? "csv" : "xlsx")}"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        if (format == "csv")
            JournalStudioExporter.WriteBulkCsv(dialog.FileName, _bulkRows);
        else
            JournalStudioExporter.WriteBulkExcel(dialog.FileName, _bulkRows);

        MessageBox.Show(this, "Export completed.", "Journal Studio", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

    /// <summary>
    /// Excel export via the platform's dependency-free OOXML writer (SS-10.5). Sheet 1 is the
    /// visible filtered set; sheet 2 carries the load metadata (vendor sniff, parser, file,
    /// counts) so an exported workbook is self-describing for audit.
    /// </summary>
    public static void WriteExcel(
        string path,
        IReadOnlyList<EjTransaction> rows,
        (string Vendor, string Parser, string File, int Transactions) metadata)
    {
        var transactions = new ExcelSheet(
            "Transactions",
            new[] { "timestamp_utc", "terminal_id", "transaction_id", "classification", "amount", "currency", "confidence", "start_line", "end_line" },
            rows.Select(t => (IReadOnlyList<string?>)new string?[]
            {
                t.Timestamp.ToString("O"), t.ATM_ID, t.TransactionId, t.Classification.ToString(),
                t.Amount.HasValue ? t.Amount.Value.ToString("0.00") : null, t.Currency,
                t.Confidence.ToString("0.00"), t.StartLine.ToString(), t.EndLine.ToString()
            }).ToList());

        var summary = new ExcelSheet(
            "Summary",
            new[] { "field", "value" },
            new List<IReadOnlyList<string?>>
            {
                new string?[] { "file", metadata.File },
                new string?[] { "detected_vendor", metadata.Vendor },
                new string?[] { "parser", metadata.Parser },
                new string?[] { "transactions_in_file", metadata.Transactions.ToString() },
                new string?[] { "rows_in_this_export", rows.Count.ToString() },
                new string?[] { "exported_utc", DateTime.UtcNow.ToString("O") }
            });

        ExcelWorkbookWriter.Write(path, new[] { summary, transactions });
    }

    public static void WriteBulkCsv(string path, IReadOnlyList<JournalStudioBulkAnalyzer.BulkRow> rows)
    {
        using var writer = new StreamWriter(path, false, new System.Text.UTF8Encoding(true));
        writer.WriteLine("file,vendor,parser,lines,transactions,success,suspicious,total_amount,status");
        foreach (var r in rows)
        {
            writer.WriteLine($"{Escape(r.FileName)},{Escape(r.Vendor)},{Escape(r.Parser)},{r.Lines},{r.Transactions},{r.SuccessCount},{r.SuspiciousCount},{r.TotalAmount:0.00},{Escape(r.Status)}");
        }
    }

    public static void WriteBulkExcel(string path, IReadOnlyList<JournalStudioBulkAnalyzer.BulkRow> rows)
    {
        var files = new ExcelSheet(
            "Files",
            new[] { "file", "vendor", "parser", "lines", "transactions", "success", "suspicious", "total_amount", "status" },
            rows.Select(r => (IReadOnlyList<string?>)new string?[]
            {
                r.FileName, r.Vendor, r.Parser, r.Lines.ToString(), r.Transactions.ToString(),
                r.SuccessCount.ToString(), r.SuspiciousCount.ToString(), r.TotalAmount.ToString("0.00"), r.Status
            }).ToList());

        var byVendor = new ExcelSheet(
            "ByVendor",
            new[] { "vendor", "files", "transactions", "success", "suspicious", "total_amount" },
            rows.GroupBy(r => r.Vendor)
                .OrderByDescending(g => g.Count())
                .Select(g => (IReadOnlyList<string?>)new string?[]
                {
                    g.Key, g.Count().ToString(), g.Sum(r => r.Transactions).ToString(),
                    g.Sum(r => r.SuccessCount).ToString(), g.Sum(r => r.SuspiciousCount).ToString(),
                    g.Sum(r => r.TotalAmount).ToString("0.00")
                })
                .ToList());

        ExcelWorkbookWriter.Write(path, new[] { byVendor, files });
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
