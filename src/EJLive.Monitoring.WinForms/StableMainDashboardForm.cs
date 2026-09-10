using System;
using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Monitoring.WinForms
{
    public sealed class MainDashboardForm : Form
    {
        private readonly Label _fleet;
        private readonly Label _journal;
        private readonly Label _alarm;

        public MainDashboardForm()
        {
            Text = "EJLive Monitoring Dashboard";
            MinimumSize = new Size(980, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(243, 247, 248);

            _fleet = CreateMetric("Fleet", "1 Online");
            _journal = CreateMetric("Journal Sync", "Idle");
            _alarm = CreateMetric("Critical Alarms", "0");
            BuildLayout();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(18)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var title = new Label
            {
                Text = "Real-time ATM status and journal intelligence",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 52, 66)
            };
            root.Controls.Add(title, 0, 0);

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.4f));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(grid, 0, 1);

            grid.Controls.Add(Wrap(_fleet), 0, 0);
            grid.Controls.Add(Wrap(_journal), 1, 0);
            grid.Controls.Add(Wrap(_alarm), 2, 0);

            var timeline = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(250, 252, 253),
                Font = new Font("Consolas", 10f)
            };
            timeline.Items.Add(DateTime.Now.ToString("HH:mm:ss") + "  Dashboard initialized.");
            timeline.Items.Add(DateTime.Now.ToString("HH:mm:ss") + "  Waiting for live server feed.");
            grid.SetColumnSpan(timeline, 3);
            grid.Controls.Add(timeline, 0, 1);
        }

        private static Label CreateMetric(string caption, string value)
        {
            return new Label
            {
                Text = caption + Environment.NewLine + value,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 95, 87)
            };
        }

        private static Panel Wrap(Control child)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                BackColor = Color.FromArgb(250, 252, 253)
            };
            panel.Controls.Add(child);
            return panel;
        }
    }
}
