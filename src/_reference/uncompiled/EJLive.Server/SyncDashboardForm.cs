using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EJLive.Core.Models;

namespace EJLive.Server.WinForms
{
    public partial class SyncDashboardForm : Form
    {
        public SyncDashboardForm()
        {
            InitializeComponent();
        }

        public void BindItems(IReadOnlyList<JournalSyncDashboardItem> items)
        {
            dgvSync.Rows.Clear();
            rtbSyncDetails.Clear();

            foreach (var item in items ?? Array.Empty<JournalSyncDashboardItem>())
            {
                int rowIndex = dgvSync.Rows.Add(
                    item.ATM_ID,
                    item.ATM_Name,
                    item.ATM_Type,
                    item.IsConnected ? "Connected" : "Disconnected",
                    item.LastHeartbeatUtc.HasValue ? item.LastHeartbeatUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "--",
                    item.LastJournalSyncUtc.HasValue ? item.LastJournalSyncUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "--",
                    item.PendingFiles,
                    item.FailedFiles,
                    item.CompletedFiles,
                    item.PendingBytes,
                    item.Alerts.Count);

                var row = dgvSync.Rows[rowIndex];
                row.Tag = item;
                if (item.Alerts.Any(a => a.Severity == JournalSyncAlertSeverity.Critical))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 225, 225);
                else if (item.Alerts.Any(a => a.Severity == JournalSyncAlertSeverity.Warning))
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220);
            }

            if (dgvSync.Rows.Count > 0)
                dgvSync.Rows[0].Selected = true;
            else
                rtbSyncDetails.Text = "No sync dashboard items available yet.";
        }

        private void dgvSync_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSync.SelectedRows.Count == 0)
                return;

            var item = dgvSync.SelectedRows[0].Tag as JournalSyncDashboardItem;
            if (item == null)
                return;

            rtbSyncDetails.Text = RenderItemDetails(item);
        }

        private string RenderItemDetails(JournalSyncDashboardItem item)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Journal Sync Dashboard Item");
            sb.AppendLine("===========================");
            sb.AppendLine("ATM ID: " + (item.ATM_ID ?? "--"));
            sb.AppendLine("ATM Name: " + (item.ATM_Name ?? "--"));
            sb.AppendLine("ATM Type: " + (item.ATM_Type ?? "--"));
            sb.AppendLine("Connected: " + item.IsConnected);
            sb.AppendLine("Last Heartbeat: " + (item.LastHeartbeatUtc.HasValue ? item.LastHeartbeatUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "--"));
            sb.AppendLine("Last Successful Sync: " + (item.LastJournalSyncUtc.HasValue ? item.LastJournalSyncUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "--"));
            sb.AppendLine("Pending Files: " + item.PendingFiles);
            sb.AppendLine("Failed Files: " + item.FailedFiles);
            sb.AppendLine("Completed Files: " + item.CompletedFiles);
            sb.AppendLine("Pending Bytes: " + item.PendingBytes);
            sb.AppendLine();
            if (item.Alerts != null && item.Alerts.Count > 0)
            {
                sb.AppendLine("Alerts:");
                foreach (var alert in item.Alerts.OrderByDescending(a => a.Severity))
                {
                    sb.AppendLine("- [" + alert.Severity + "] " + alert.Title);
                    sb.AppendLine("  " + alert.Message);
                    if (!string.IsNullOrWhiteSpace(alert.RecommendedAction))
                        sb.AppendLine("  Action: " + alert.RecommendedAction);
                }
            }
            else
            {
                sb.AppendLine("Alerts: none");
            }
            return sb.ToString();
        }
    }
}
