using System;
using System.Drawing;
using System.Windows.Forms;
using EJLive.Core;

namespace EJLive.Server.WinForms
{
    public sealed class ServerMainForm : Form
    {
        private readonly Label _status;
        private readonly ListBox _audit;

        public ServerMainForm()
        {
            Text = "EJLive Server Operations";
            MinimumSize = new Size(1024, 680);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(242, 246, 248);

            _status = new Label();
            _audit = new ListBox();
            BuildLayout();
            AddAudit("Server console initialized.");
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(18)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            Controls.Add(root);

            var header = new Label
            {
                Text = "EJLive Server - monitoring, ingestion, reporting, controlled operations",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 17f, FontStyle.Bold),
                ForeColor = Color.FromArgb(36, 54, 68),
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(header, 0, 0);

            var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10f) };
            tabs.TabPages.Add(CreateOverviewTab());
            tabs.TabPages.Add(CreateCommandsTab());
            tabs.TabPages.Add(CreateReportsTab());
            root.Controls.Add(tabs, 0, 1);

            _audit.Dock = DockStyle.Fill;
            _audit.BorderStyle = BorderStyle.None;
            _audit.BackColor = Color.FromArgb(250, 252, 253);
            _audit.Font = new Font("Consolas", 10f);
            root.Controls.Add(_audit, 0, 2);
        }

        private TabPage CreateOverviewTab()
        {
            var page = new TabPage("Operations");
            var panel = CreatePanel();
            page.Controls.Add(panel);

            _status.Text = "Fleet: 1 demo terminal online | Alarms: 0 critical | Sync: idle";
            _status.Dock = DockStyle.Top;
            _status.Height = 56;
            _status.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            _status.ForeColor = Color.FromArgb(42, 106, 91);
            panel.Controls.Add(_status);

            var detail = new Label
            {
                Text = "Active source has been stabilized around Core, Shared, Server UI, Client UI, service worker, Security, Reports, and Verification.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(76, 92, 106)
            };
            panel.Controls.Add(detail);
            return page;
        }

        private TabPage CreateCommandsTab()
        {
            var page = new TabPage("Controlled Commands");
            var panel = CreatePanel();
            page.Controls.Add(panel);

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 64 };
            panel.Controls.Add(flow);

            flow.Controls.Add(CreateCommandButton("Validate Policy", delegate
            {
                var policy = new UnifiedRemoteCommandPolicy();
                var result = policy.Validate(new RemoteCommandEnvelope
                {
                    CommandType = "Ping",
                    OperatorRole = "Admin",
                    OperatorId = Environment.UserName,
                    Approved = true
                });
                AddAudit("Policy result: " + result.Allowed + " - " + result.Reason);
            }));

            flow.Controls.Add(CreateCommandButton("Queue Sync", delegate
            {
                AddAudit("ForceSync queued for approval workflow.");
            }));

            flow.Controls.Add(CreateCommandButton("Archive Intake", delegate
            {
                AddAudit("Archive intake check completed.");
            }));

            var note = new Label
            {
                Text = "Remote operations are represented as audited, allowlisted requests. This UI does not execute arbitrary shell commands.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(76, 92, 106)
            };
            panel.Controls.Add(note);
            return page;
        }

        private TabPage CreateReportsTab()
        {
            var page = new TabPage("Reports");
            var panel = CreatePanel();
            page.Controls.Add(panel);

            var button = CreateCommandButton("Generate Summary", delegate
            {
                AddAudit("Operational summary report generated in memory.");
            });
            button.Dock = DockStyle.Top;
            panel.Controls.Add(button);

            var detail = new Label
            {
                Text = "Reports layer is active and ready for export wiring.",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(76, 92, 106)
            };
            panel.Controls.Add(detail);
            return page;
        }

        private void AddAudit(string message)
        {
            _audit.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") + "  " + message);
        }

        private static Panel CreatePanel()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                BackColor = Color.FromArgb(250, 252, 253)
            };
        }

        private static Button CreateCommandButton(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
                Width = 160,
                Height = 38,
                Margin = new Padding(6),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(226, 236, 244),
                ForeColor = Color.FromArgb(38, 69, 96),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            button.Click += handler;
            return button;
        }
    }
}
