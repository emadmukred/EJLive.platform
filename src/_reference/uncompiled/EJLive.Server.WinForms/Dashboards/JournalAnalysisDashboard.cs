using EJLive.Core.Engine;
using EJLive.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EJLive.Server.WinForms.Dashboards
{
    public partial class JournalAnalysisDashboard : UserControl
    {
        private DataGridView _gridTransactions = null!;
        private DataGridView _gridAnomalies = null!;
        private RichTextBox _log = null!;
        private Label _lblSummary = null!;
        private ComboBox _cmbVendor = null!;
        private ComboBox _cmbFilter = null!;
        public JournalAnalysisDashboard()
        {
            Dock = DockStyle.Fill;
            BuildUI();
        }
        private void BuildUI()
        {
            var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(12) };
            // Header & vendor selector
            var header = new FlowLayoutPanel { Dock = DockStyle.Fill, Height = 36, FlowDirection = FlowDirection.LeftToRight };
            header.Controls.Add(new Label { Text = "Journal Intelligence Dashboard", Font = new Font("Segoe UI", 14F, FontStyle.Bold), AutoSize = true });
            _cmbVendor = new ComboBox { Items = { "ALL", "NCR", "GRG", "Wincor", "Diebold", "Hyosung", "Cashway" }, Width = 120, Text = "ALL", Margin = new Padding(16, 4, 4, 4) };
            _cmbFilter = new ComboBox { Items = { "All", "Success", "Failed", "Suspicious", "Reversal", "PartialDispense", "ApprovedNoDispense", "CashJam", "Retract", "HostDeclined", "MissingSequence", "DuplicateSequence" }, Width = 150, Text = "All", Margin = new Padding(4, 4, 4, 4) };
            var btnRun = Button("Run Analysis"); btnRun.Click += (s, e) => RunAnalysis();
            var btnExport = Button("Export"); btnExport.Click += (s, e) => Log("Export requested.");
            header.Controls.Add(_cmbVendor); header.Controls.Add(_cmbFilter); header.Controls.Add(btnRun); header.Controls.Add(btnExport);
            main.Controls.Add(header, 0, 0); main.SetColumnSpan(header, 2);
            // Summary KPI cards
            _lblSummary = new Label { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F), Text = "Total: — | Success: — | Failed: — | Suspicious: — | Reversal: — | PD: — | AND: — | Jam: — | Missing: — | Duplicate: —", Height = 24, ForeColor = Color.FromArgb(0, 100, 200) };
            main.Controls.Add(_lblSummary, 0, 1); main.SetColumnSpan(_lblSummary, 2);
            // Transactions grid
            _gridTransactions = CreateGrid();
            main.Controls.Add(_gridTransactions, 0, 2);
            // Anomalies grid
            _gridAnomalies = CreateGrid();
            main.Controls.Add(_gridAnomalies, 1, 2);
            // Log
            _log = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F), ReadOnly = true, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.LightGreen, Height = 120 };
            main.Controls.Add(_log, 0, 3); main.SetColumnSpan(_log, 2);
            Controls.Add(main);
        }
        private void RunAnalysis()
        {
            Log($"=== Journal Analysis Started [{DateTime.Now:HH:mm:ss}] ===");
            Log($"Vendor: {_cmbVendor.Text} | Filter: {_cmbFilter.Text}");
            // Simulated analysis results
            var svc = new SequenceValidationService();
            var sampleTxs = GenerateSampleTransactions();
            var result = svc.Validate(sampleTxs);
            // Populate transactions grid
            _gridTransactions.Columns.Clear();
            foreach (var col in new[] { "Tx#", "Date", "Amount", "STAN", "RRN", "Card", "Status", "Confidence", "Cassettes" })
                _gridTransactions.Columns.Add(col, col);
            _gridTransactions.Rows.Clear();
            foreach (var tx in sampleTxs.Take(20))
                _gridTransactions.Rows.Add(tx.TransactionId, DateTime.Now.ToString("HH:mm"), $"{tx.Amount ?? 0:N0}", tx.STAN, tx.RRN, MaskCard(tx.CardNumber), tx.Classification.ToString(), $"{tx.Confidence:P0}", $"C1:{tx.Cassette1} C2:{tx.Cassette2} C3:{tx.Cassette3} C4:{tx.Cassette4}");
            // Populate anomalies grid
            _gridAnomalies.Columns.Clear();
            _gridAnomalies.Columns.Add("Type", "Type"); _gridAnomalies.Columns.Add("Serial", "Serial #"); _gridAnomalies.Columns.Add("Detail", "Detail");
            _gridAnomalies.Rows.Clear();
            foreach (var m in result.MissingSequences)
                _gridAnomalies.Rows.Add("MISSING", m.SerialNumber.ToString(), m.Detail);
            foreach (var d in result.DuplicateSequences)
                _gridAnomalies.Rows.Add("DUPLICATE", d.SerialNumber.ToString(), d.Detail);
            // Risk profiles
            int reversals = 0, partials = 0, jams = 0, ands = 0, missing = result.MissingSequences.Count, dup = result.DuplicateSequences.Count;
            foreach (var tx in sampleTxs)
            {
                var risk = svc.ClassifyRisk(tx);
                if (risk.IsReversal) reversals++;
                if (risk.IsPartialDispense) partials++;
                if (risk.IsCashJam) jams++;
                if (risk.IsApprovedNoDispense) ands++;
            }
            int success = sampleTxs.Count(t => t.Classification == TransactionClassification.Success);
            int failed = sampleTxs.Count(t => t.Classification == TransactionClassification.Failed || t.Classification == TransactionClassification.HostDeclined || t.Classification == TransactionClassification.HardwareFault);
            int suspicious = sampleTxs.Count(t => t.Classification == TransactionClassification.Suspicious);
            _lblSummary.Text = $"Total: {sampleTxs.Count} | Success: {success} | Failed: {failed} | Suspicious: {suspicious} | Reversal: {reversals} | PD: {partials} | AND: {ands} | Jam: {jams} | Missing: {missing} | Duplicate: {dup}";
            Log($"Analysis complete. Anomalies found: {result.MissingSequences.Count} missing, {result.DuplicateSequences.Count} duplicate.");
        }
        private static List<EjTransaction> GenerateSampleTransactions()
        {
            var list = new List<EjTransaction>();
            var rng = new Random();
            for (int i = 1; i <= 30; i++)
            {
                var c = i switch
                {
                    _ when i == 5 => TransactionClassification.Reversal,
                    _ when i == 11 => TransactionClassification.PartialDispense,
                    _ when i == 17 => TransactionClassification.ApprovedNoDispense,
                    _ when i == 23 => TransactionClassification.CashJam,
                    _ when i == 25 => TransactionClassification.Suspicious,
                    _ when i == 28 => TransactionClassification.HostDeclined,
                    _ => TransactionClassification.Success
                };
                list.Add(new EjTransaction(
                    TransactionId: i.ToString(),
                    StartLine: i * 10,
                    EndLine: i * 10 + 5,
                    ATM_ID: "ATM-001",
                    CardNumber: $"4532******{rng.Next(1000, 9999)}",
                    AccountNumber: $"****{rng.Next(1000, 9999)}",
                    Amount: rng.Next(100, 5000),
                    Currency: "SAR",
                    STAN: $"{100000 + i}",
                    RRN: $"{200000 + i}",
                    Cassette1: rng.Next(0, 20),
                    Cassette2: rng.Next(0, 15),
                    Cassette3: rng.Next(0, 10),
                    Cassette4: 0,
                    MCode: i % 5 == 0 ? "M-03" : "M-00",
                    RCode: i % 5 == 0 ? "R-01" : "R-00",
                    RawLines: new List<string> { $"Line {i * 10}", $"Line {i * 10 + 1}" },
                    Classification: c,
                    Confidence: c == TransactionClassification.Success ? 0.95 : 0.45,
                    Timestamp: DateTime.UtcNow
                ));
            }
            return list;
        }
        private static DataGridView CreateGrid() => new() { Dock = DockStyle.Fill, BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true, RowHeadersVisible = false };
        private static Button Button(string text) => new() { Text = text, Width = 105, Height = 28, Margin = new Padding(3), BackColor = Color.FromArgb(66, 139, 202), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false };
        private static string MaskCard(string? card) => card?.Length > 8 ? card[..6] + "******" + card[^4..] : "****";
        private void Log(string msg) { _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n"); }
    }

}
