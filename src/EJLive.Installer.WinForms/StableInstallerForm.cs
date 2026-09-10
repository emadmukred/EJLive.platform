using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EJLive.Installer.WinForms
{
    public sealed class InstallerForm : Form
    {
        private readonly TextBox _installPath;
        private readonly TextBox _serverHost;
        private readonly Label _result;

        public InstallerForm()
        {
            Text = "EJLive Installer";
            MinimumSize = new Size(860, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(244, 247, 250);

            _installPath = new TextBox();
            _serverHost = new TextBox();
            _result = new Label();
            BuildLayout();
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(22)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "EJLive installation readiness",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 52, 66)
            }, 0, 0);

            _installPath.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "EJLive");
            root.Controls.Add(CreateField("Install path", _installPath), 0, 1);

            _serverHost.Text = "127.0.0.1:5656";
            root.Controls.Add(CreateField("Server endpoint", _serverHost), 0, 2);

            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(250, 252, 253), Padding = new Padding(16) };
            var validate = new Button
            {
                Text = "Validate",
                Width = 130,
                Height = 38,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(222, 236, 229),
                ForeColor = Color.FromArgb(36, 83, 72),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            validate.Click += ValidateClicked;
            panel.Controls.Add(validate);

            _result.Dock = DockStyle.Fill;
            _result.Padding = new Padding(0, 58, 0, 0);
            _result.Font = new Font("Segoe UI", 11f);
            _result.ForeColor = Color.FromArgb(74, 90, 104);
            panel.Controls.Add(_result);
            root.Controls.Add(panel, 0, 3);
        }

        private void ValidateClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_installPath.Text))
            {
                _result.Text = "Install path is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_serverHost.Text) || !_serverHost.Text.Contains(":"))
            {
                _result.Text = "Server endpoint must include host and port.";
                return;
            }

            _result.Text = "Configuration is valid. Service registration is ready for an elevated installer step.";
        }

        private static Control CreateField(string caption, TextBox textBox)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 8) };
            var label = new Label
            {
                Text = caption,
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(66, 82, 96)
            };
            textBox.Dock = DockStyle.Bottom;
            textBox.Height = 28;
            textBox.Font = new Font("Segoe UI", 10f);
            panel.Controls.Add(textBox);
            panel.Controls.Add(label);
            return panel;
        }
    }
}
