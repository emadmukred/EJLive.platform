using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EJLive.Core.Models;
using EJLive.Core.Services;

namespace EJLive.Server.WinForms
{
    public sealed class JournalSyncDashboardForm : Form
    {
        private readonly JournalSyncMonitorService _monitor;
        private readonly Timer _refreshTimer;
        private readonly Label _lblTotals;
        private readonly Label _lblAlerts;
        private readonly DataGridView _dgvAtmStatus;
        private readonly DataGridView _dgvRecentSyncs;
        private readonly DataGridView _dgvAlerts;

        public JournalSyncDashboardForm(JournalSyncMonitorService monitor)
        {
            _monitor = monitor;
            Text = "EJLive Journal Sync Dashboard";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1280, 760);
            MinimumSize = new Size(1100, 680);
            Font = new Font("Segoe UI", 9F);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            Controls.Add(root);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 55, 95) };
            var lblTitle = new Label
            {
                Text = "Journal Sync Dashboard",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(14, 13)
            };
            _lblTotals = new Label
            {
                Text = "ATMs: 0 | Pending: 0 | Failed: 0 | Completed: 0",
                ForeColor = Color.WhiteSmoke,
                AutoSize = true,
                Location = new Point(320, 18)
            };
            _lblAlerts = new Label
            {
                Text = "Active alerts: 0",
                ForeColor = Color.WhiteSmoke,
                AutoSize = true,
                Location = new Point(760, 18)
            };
            header.Controls.Add(lblTitle);
            header.Controls.Add(_lblTotals);
            header.Controls.Add(_lblAlerts);
            root.Controls.Add(header, 0, 0);

            _dgvAtmStatus = CreateGrid();
            _dgvAtmStatus.Columns.Add(CreateTextCol("ATM_ID", "ATM", 110));
            _dgvAtmStatus.Columns.Add(CreateTextCol("ATMType", "Type", 90));
            _dgvAtmStatus.Columns.Add(CreateTextCol("ConnectionStatus", "Connection", 110));
            _dgvAtmStatus.Columns.Add(CreateTextCol("LastHeartbeatUtc", "Last Heartbeat", 150));
            _dgvAtmStatus.Columns.Add(CreateTextCol("LastJournalReceivedUtc", "Last Journal", 150));
            _dgvAtmStatus.Columns.Add(CreateTextCol("LastSuccessfulSyncUtc", "Last Success", 150));
            _dgvAtmStatus.Columns.Add(CreateTextCol("PendingCount", "Pending", 70));
            _dgvAtmStatus.Columns.Add(CreateTextCol("FailedCount", "Failed", 70));
            _dgvAtmStatus.Columns.Add(CreateTextCol("CompletedCount", "Completed", 80));
            _dgvAtmStatus.Columns.Add(CreateTextCol("TotalBytesReceived", "Bytes", 90));
            _dgvAtmStatus.Columns.Add(CreateTextCol("SyncIsStale", "Stale", 60));
            root.Controls.Add(WrapGroup("ATM Sync Status", _dgvAtmStatus), 0, 1);

            _dgvRecentSyncs = CreateGrid();
            _dgvRecentSyncs.Columns.Add(CreateTextCol("ATM_ID", "ATM", 100));
            _dgvRecentSyncs.Columns.Add(CreateTextCol("FileName", "File", 230));
            _dgvRecentSyncs.Columns.Add(CreateTextCol("State", "State", 90));
            _dgvRecentSyncs.Columns.Add(CreateTextCol("FileSize", "Size", 90));
            _dgvRecentSyncs.Columns.Add(CreateTextCol("ProgressPercent", "Progress", 70));
            _dgvRecentSyncs.Columns.Add(CreateTextCol("RetryCount", "Retries", 60));
            _dgvRecentSyncs.Columns.Add(CreateTextCol("CreatedAtUtc", "Created", 150));
            _dgvRecentSyncs.Columns.Add(CreateTextCol("CompletedAtUtc", "Completed", 150));
            _dgvRecentSyncs.Columns.Add(CreateFillCol("Message", "Message"));
            root.Controls.Add(WrapGroup("Recent Sync Records", _dgvRecentSyncs), 0, 2);

            _dgvAlerts = CreateGrid();
            _dgvAlerts.Columns.Add(CreateTextCol("ATM_ID", "ATM", 100));
            _dgvAlerts.Columns.Add(CreateTextCol("Severity", "Severity", 90));
            _dgvAlerts.Columns.Add(CreateTextCol("Title", "Title", 180));
            _dgvAlerts.Columns.Add(CreateTextCol("RaisedAtUtc", "Raised", 150));
            _dgvAlerts.Columns.Add(CreateFillCol("Message", "Message"));
            root.Controls.Add(WrapGroup("Sync Alerts", _dgvAlerts), 0, 3);

            _refreshTimer = new Timer { Interval = 5000 };
            _refreshTimer.Tick += (s, e) => RefreshDashboard();
            _refreshTimer.Start();
            FormClosed += (s, e) => _refreshTimer.Stop();

            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            _dgvAtmStatus.Rows.Clear();
            _dgvRecentSyncs.Rows.Clear();
            _dgvAlerts.Rows.Clear();

            if (_monitor == null)
            {
                _lblTotals.Text = "Sync monitor unavailable";
                _lblAlerts.Text = "Active alerts: 0";
                return;
            }

            var atms = _monitor.GetAtmSummaries().ToList();
            var records = _monitor.GetRecentRecords().Take(200).ToList();
            var alerts = _monitor.GetActiveAlerts().ToList();

            foreach (var atm in atms)
            {
                int idx = _dgvAtmStatus.Rows.Add(
                    atm.ATM_ID,
                    atm.ATMType,
                    atm.ConnectionStatus,
                    FormatUtc(atm.LastHeartbeatUtc),
                    FormatUtc(atm.LastJournalReceivedUtc),
                    FormatUtc(atm.LastSuccessfulSyncUtc),
                    atm.PendingCount,
                    atm.FailedCount,
                    atm.CompletedCount,
                    atm.TotalBytesReceived,
                    atm.SyncIsStale ? "Yes" : "No");
                if (atm.SyncIsStale)
                    _dgvAtmStatus.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220);
                if (string.Equals(atm.ConnectionStatus, "Disconnected", StringComparison.OrdinalIgnoreCase))
                    _dgvAtmStatus.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
            }

            foreach (var rec in records)
            {
                int idx = _dgvRecentSyncs.Rows.Add(
                    rec.ATM_ID,
                    rec.FileName,
                    rec.State,
                    rec.FileSize,
                    rec.ProgressPercent + "%",
                    rec.RetryCount,
                    FormatUtc(rec.CreatedAtUtc),
                    FormatUtc(rec.CompletedAtUtc),
                    rec.Message ?? string.Empty);
                if (rec.State == JournalSyncState.Failed)
                    _dgvRecentSyncs.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                else if (rec.State == JournalSyncState.Syncing)
                    _dgvRecentSyncs.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);
            }

            foreach (var alert in alerts)
            {
                int idx = _dgvAlerts.Rows.Add(alert.ATM_ID, alert.Severity, alert.Title, FormatUtc(alert.RaisedAtUtc), alert.Message);
                if (alert.Severity == JournalSyncAlertSeverity.Critical)
                    _dgvAlerts.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220);
                else if (alert.Severity == JournalSyncAlertSeverity.Warning)
                    _dgvAlerts.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220);
            }

            _lblTotals.Text = string.Format(
                "ATMs: {0} | Pending: {1} | Failed: {2} | Completed: {3}",
                atms.Count,
                atms.Sum(a => a.PendingCount),
                atms.Sum(a => a.FailedCount),
                atms.Sum(a => a.CompletedCount));
            _lblAlerts.Text = "Active alerts: " + alerts.Count;
        }

        private static string FormatUtc(DateTime? value)
        {
            return value.HasValue ? value.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "--";
        }

        private static GroupBox WrapGroup(string title, Control inner)
        {
            var group = new GroupBox { Text = title, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            inner.Dock = DockStyle.Fill;
            group.Controls.Add(inner);
            return group;
        }

        private static DataGridView CreateGrid()
        {
            return new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders
            };
        }

        private static DataGridViewTextBoxColumn CreateTextCol(string name, string header, int width)
        {
            return new DataGridViewTextBoxColumn { Name = name, HeaderText = header, Width = width, ReadOnly = true };
        }

        private static DataGridViewTextBoxColumn CreateFillCol(string name, string header)
        {
            return new DataGridViewTextBoxColumn { Name = name, HeaderText = header, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true };
        }
    }
}
