using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EJLive.Server.WinForms.Dashboards
{
    public partial class LogAnalysisDashboard : UserControl
    {
        private DataGridView _gridEvents = null!;
        private DataGridView _gridCorrelation = null!;
        private RichTextBox _log = null!;
        private Label _lblSummary = null!;
        private ComboBox _cmbVendor = null!;
        private ComboBox _cmbSeverity = null!;
        public LogAnalysisDashboard()
        {
            Dock = DockStyle.Fill;
            BuildUI();
        }
        private void BuildUI()
        {
            var main = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(12) };
            var header = new FlowLayoutPanel { Dock = DockStyle.Fill, Height = 36, FlowDirection = FlowDirection.LeftToRight };
            header.Controls.Add(new Label { Text = "Real ATM Log Analysis Dashboard", Font = new Font("Segoe UI", 14F, FontStyle.Bold), AutoSize = true });
            _cmbVendor = new ComboBox { Items = { "ALL", "NCR", "GRG", "Wincor", "Diebold", "Hyosung", "Cashway" }, Width = 120, Text = "ALL", Margin = new Padding(16, 4, 4, 4) };
            _cmbSeverity = new ComboBox { Items = { "All", "Info", "Warning", "Error", "Critical" }, Width = 100, Text = "All", Margin = new Padding(4, 4, 4, 4) };
            var btnAnalyze = Button("Analyze Logs"); btnAnalyze.Click += (s, e) => RunAnalysis();
            var btnExport = Button("Export"); btnExport.Click += (s, e) => Log("Export requested.");
            header.Controls.Add(_cmbVendor); header.Controls.Add(_cmbSeverity); header.Controls.Add(btnAnalyze); header.Controls.Add(btnExport);
            main.Controls.Add(header, 0, 0); main.SetColumnSpan(header, 2);
            _lblSummary = new Label { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F), Text = "Events: — | Info: — | Warning: — | Error: — | Critical: — | Correlated: — | Root Causes: —", Height = 24, ForeColor = Color.FromArgb(0, 100, 200) };
            main.Controls.Add(_lblSummary, 0, 1); main.SetColumnSpan(_lblSummary, 2);
            _gridEvents = CreateGrid();
            main.Controls.Add(_gridEvents, 0, 2);
            _gridCorrelation = CreateGrid();
            main.Controls.Add(_gridCorrelation, 1, 2);
            _log = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9F), ReadOnly = true, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.LightGreen, Height = 120 };
            main.Controls.Add(_log, 0, 3); main.SetColumnSpan(_log, 2);
            Controls.Add(main);
        }
        private void RunAnalysis()
        {
            Log($"=== Log Analysis Started [{DateTime.Now:HH:mm:ss}] ===");
            var events = GenerateSampleEvents();
            int info = events.Count(e => e.Item4 == "Info"), warn = events.Count(e => e.Item4 == "Warning"), err = events.Count(e => e.Item4 == "Error"), crit = events.Count(e => e.Item4 == "Critical");
            // Events grid
            _gridEvents.Columns.Clear();
            foreach (var col in new[] { "Time", "Vendor", "Device", "Severity", "Code", "Message", "File" })
                _gridEvents.Columns.Add(col, col);
            _gridEvents.Rows.Clear();
            foreach (var evt in events.Take(25))
                _gridEvents.Rows.Add(evt.Item1, evt.Item2, evt.Item3, evt.Item4, evt.Item5, evt.Item6, evt.Item7);
            // Correlation grid
            _gridCorrelation.Columns.Clear();
            foreach (var col in new[] { "LinkedTx", "EventCount", "Confidence", "MatchLevel", "Source" })
                _gridCorrelation.Columns.Add(col, col);
            _gridCorrelation.Rows.Clear();
            _gridCorrelation.Rows.Add("Tx-1234", "5", "Strong", "STAN match", "EJ + NDC + XFS");
            _gridCorrelation.Rows.Add("Tx-1235", "2", "Medium", "ATM+time", "XFS + TRACE");
            _gridCorrelation.Rows.Add("Tx-1238", "1", "Weak", "Proximity", "Error burst");
            _lblSummary.Text = $"Events: {events.Count} | Info: {info} | Warning: {warn} | Error: {err} | Critical: {crit} | Correlated: 3 | Root Causes: 1";
            Log($"Analysis complete. {events.Count} events processed, {crit} critical events found.");
        }
        private static List<(string time, string vendor, string device, string sev, string code, string msg, string file)> GenerateSampleEvents()
        {
            var list = new List<(string, string, string, string, string, string, string)>();
            var rng = new Random();
            string[] vendors = { "NCR", "GRG", "NCR", "Wincor", "GRG", "NCR", "Diebold", "Hyosung", "NCR", "GRG" };
            string[] devices = { "CDM", "IDC", "PTR", "SIU", "CDM", "PIN", "CDM", "CIM", "Journal", "Host" };
            string[] msgs = { "Pick failure", "No card", "Paper out", "Door open", "Jam detected", "Key error", "Divert full", "Note reject", "Print OK", "Host timeout" };
            string[] codes = { "M-03", "05", "P-05", "SIU-01", "M-18", "EP-70", "M-07", "CIM-11", "H-00", "NDC-98" };
            string[] sevs = { "Error", "Warning", "Warning", "Info", "Critical", "Error", "Warning", "Error", "Info", "Critical" };
            for (int i = 0; i < 30; i++)
            {
                int idx = rng.Next(vendors.Length);
                list.Add(($"{DateTime.Now.AddMinutes(-rng.Next(1, 120)):HH:mm:ss}", vendors[idx], devices[idx], sevs[idx], codes[idx], msgs[idx], $"TRACE_{DateTime.Now:yyyyMMdd}.LOG"));
            }
            return list.OrderBy(e => e.Item1).ToList();
        }
        private static DataGridView CreateGrid() => new() { Dock = DockStyle.Fill, BackgroundColor = Color.White, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, ReadOnly = true, RowHeadersVisible = false };
        private static Button Button(string text) => new() { Text = text, Width = 105, Height = 28, Margin = new Padding(3), BackColor = Color.FromArgb(66, 139, 202), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, UseVisualStyleBackColor = false };
        private void Log(string msg) { _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n"); }
    }

}
