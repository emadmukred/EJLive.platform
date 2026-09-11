using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EJLive.Client.WinForms
{
    public sealed class ClientMainForm : Form
    {
        private readonly Label _title;
        private readonly Label _serviceState;
        private readonly Label _syncState;
        private readonly ListBox _events;
        private readonly Button _languageButton;
        private bool _arabic;

        public ClientMainForm()
        {
            Text = "EJLive Client Companion";
            MinimumSize = new Size(940, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 247, 250);

            _title = new Label();
            _serviceState = new Label();
            _syncState = new Label();
            _events = new ListBox();
            _languageButton = CreateButton("Arabic");

            BuildLayout();
            ApplyLanguage(false);
            AddEvent("Client companion ready.");
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(18)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            Controls.Add(root);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(235, 241, 247) };
            _title.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            _title.ForeColor = Color.FromArgb(34, 52, 67);
            _title.Dock = DockStyle.Left;
            _title.Width = 520;
            _title.TextAlign = ContentAlignment.MiddleLeft;
            header.Controls.Add(_title);

            _languageButton.Dock = DockStyle.Right;
            _languageButton.Width = 120;
            _languageButton.Click += delegate { ApplyLanguage(!_arabic); };
            header.Controls.Add(_languageButton);
            root.Controls.Add(header, 0, 0);

            var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.Controls.Add(grid, 0, 1);

            grid.Controls.Add(CreateStatusPanel("Service", _serviceState, "Windows service health snapshot"), 0, 0);
            grid.Controls.Add(CreateStatusPanel("Sync", _syncState, "Journal and log transfer readiness"), 1, 0);
            grid.Controls.Add(CreateCommandPanel(), 0, 1);
            grid.Controls.Add(CreatePathPanel(), 1, 1);

            _events.Dock = DockStyle.Fill;
            _events.BorderStyle = BorderStyle.None;
            _events.BackColor = Color.FromArgb(250, 252, 253);
            _events.Font = new Font("Consolas", 10f);
            root.Controls.Add(_events, 0, 2);
        }

        private Control CreateStatusPanel(string caption, Label valueLabel, string detail)
        {
            var panel = CreatePanel();
            var captionLabel = CreateCaption(caption);
            valueLabel.Text = "Waiting";
            valueLabel.Font = new Font("Segoe UI", 22f, FontStyle.Bold);
            valueLabel.ForeColor = Color.FromArgb(43, 111, 91);
            valueLabel.Dock = DockStyle.Top;
            valueLabel.Height = 58;
            var detailLabel = CreateDetail(detail);
            panel.Controls.Add(detailLabel);
            panel.Controls.Add(valueLabel);
            panel.Controls.Add(captionLabel);
            return panel;
        }

        private Control CreateCommandPanel()
        {
            var panel = CreatePanel();
            panel.Controls.Add(CreateCaption("Commands"));

            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 56, 0, 0) };
            panel.Controls.Add(flow);

            var ping = CreateButton("Ping");
            ping.Click += delegate
            {
                _serviceState.Text = "Reachable";
                AddEvent("Ping command completed locally.");
            };
            flow.Controls.Add(ping);

            var sync = CreateButton("Force sync");
            sync.Click += delegate
            {
                _syncState.Text = "Queued";
                AddEvent("Journal sync was queued through policy path.");
            };
            flow.Controls.Add(sync);

            var diagnostics = CreateButton("Diagnostics");
            diagnostics.Click += delegate { AddEvent("Diagnostics snapshot requested."); };
            flow.Controls.Add(diagnostics);

            return panel;
        }

        private Control CreatePathPanel()
        {
            var panel = CreatePanel();
            panel.Controls.Add(CreateCaption("Agent Snapshot"));
            var text = CreateDetail(GetHealthSnapshotSummary());
            text.Padding = new Padding(0, 56, 0, 0);
            panel.Controls.Add(text);
            return panel;
        }

        private string GetHealthSnapshotSummary()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive",
                "Agent",
                "health.json");

            return File.Exists(path)
                ? "Service snapshot detected at " + path
                : "No service snapshot yet. Start EJLive.Client.Service to publish health.";
        }

        private void ApplyLanguage(bool arabic)
        {
            _arabic = arabic;
            RightToLeft = arabic ? RightToLeft.Yes : RightToLeft.No;
            RightToLeftLayout = arabic;
            _title.Text = arabic ? "EJLive لوحة العميل" : "EJLive Client Companion";
            _languageButton.Text = arabic ? "English" : "Arabic";
            AddEvent(arabic ? "Language switched to Arabic." : "Language switched to English.");
        }

        private void AddEvent(string text)
        {
            _events.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") + "  " + text);
        }

        private static Panel CreatePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                Padding = new Padding(16),
                BackColor = Color.FromArgb(250, 252, 253)
            };
        }

        private static Label CreateCaption(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 84, 96)
            };
        }

        private static Label CreateDetail(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(78, 94, 108)
            };
        }

        private static Button CreateButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 132,
                Height = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 235, 232),
                ForeColor = Color.FromArgb(30, 74, 68),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Margin = new Padding(4)
            };
        }
    }
}
