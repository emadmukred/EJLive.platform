using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using EJLive.Core.Models;

namespace EJLive.Client.WinForms.Controls
{
    /// <summary>
    /// Interactive ATM card that presents connection, sync, cash, and error state.
    /// </summary>
    public class ATMCardControl : Panel
    {
        private ATMInfo _atmInfo;
        private Label _lblId = null!;
        private Label _lblStatus = null!;
        private Label _lblLastSync = null!;
        private Label _lblNetworkType = null!;
        private Label _lblLatency = null!;
        private Label _lblCash = null!;
        private Label _lblError = null!;
        private Panel _statusIndicator = null!;

        public event EventHandler<ATMInfo>? OnCardDoubleClick;
        public event EventHandler<ATMInfo>? OnActionRequested;

        public ATMInfo ATMData => _atmInfo;

        public ATMCardControl(ATMInfo atm)
        {
            _atmInfo = atm;
            InitializeControls();
            UpdateUI();

            DoubleClick += (s, e) => OnCardDoubleClick?.Invoke(this, _atmInfo);
            Click += (s, e) => OnActionRequested?.Invoke(this, _atmInfo);
            MouseEnter += (s, e) => { BackColor = Color.FromArgb(45, 45, 45); Invalidate(); };
            MouseLeave += (s, e) => { BackColor = Color.FromArgb(37, 37, 37); Invalidate(); };

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void InitializeControls()
        {
            Size = new Size(260, 160);
            BackColor = Color.FromArgb(37, 37, 37);
            Cursor = Cursors.Hand;
            Padding = new Padding(12);
            Margin = new Padding(6);

            _statusIndicator = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = Color.FromArgb(0, 200, 83)
            };
            Controls.Add(_statusIndicator);

            _lblId = new Label
            {
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(224, 224, 224),
                AutoSize = true,
                Location = new Point(12, 16)
            };
            Controls.Add(_lblId);

            _lblNetworkType = new Label
            {
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(117, 117, 117),
                AutoSize = true,
                Location = new Point(12, 38)
            };
            Controls.Add(_lblNetworkType);

            _lblStatus = new Label
            {
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(12, 62)
            };
            Controls.Add(_lblStatus);

            _lblLastSync = new Label
            {
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(170, 170, 170),
                AutoSize = true,
                Location = new Point(12, 84)
            };
            Controls.Add(_lblLastSync);

            _lblCash = new Label
            {
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(170, 170, 170),
                AutoSize = true,
                Location = new Point(12, 104)
            };
            Controls.Add(_lblCash);

            _lblLatency = new Label
            {
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(117, 117, 117),
                AutoSize = true,
                Location = new Point(180, 38)
            };
            Controls.Add(_lblLatency);

            _lblError = new Label
            {
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(213, 0, 0),
                AutoSize = false,
                Size = new Size(236, 20),
                Location = new Point(12, 128),
                Text = string.Empty
            };
            Controls.Add(_lblError);
        }

        public void UpdateATMInfo(ATMInfo atm)
        {
            _atmInfo = atm;
            if (InvokeRequired) { Invoke(new Action(UpdateUI)); return; }
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_atmInfo == null) return;

            _lblId.Text = _atmInfo.ATMId ?? "---";
            _lblNetworkType.Text = $"{_atmInfo.ATMType} - {_atmInfo.NetworkType ?? "LAN"}";
            _lblLatency.Text = _atmInfo.Latency > 0 ? $"{_atmInfo.Latency}ms" : "---";

            (string statusText, Color statusColor, Color indicatorColor) = GetStatusInfo(_atmInfo);
            _lblStatus.Text = statusText;
            _lblStatus.ForeColor = statusColor;
            _statusIndicator.BackColor = indicatorColor;

            if (_atmInfo.LastSyncTime > DateTime.MinValue)
            {
                var ago = DateTime.UtcNow - _atmInfo.LastSyncTime;
                _lblLastSync.Text = ago.TotalMinutes < 60
                    ? $"Last sync: {(int)ago.TotalMinutes} minutes ago"
                    : $"Last sync: {(int)ago.TotalHours} hours ago";
            }
            else
            {
                _lblLastSync.Text = "No sync completed yet";
                _lblLastSync.ForeColor = Color.FromArgb(255, 109, 0);
            }

            _lblCash.Text = _atmInfo.CashDispensed > 0
                ? $"Cash dispensed: {_atmInfo.CashDispensed:N0}"
                : "Cash data unavailable";

            if (!string.IsNullOrEmpty(_atmInfo.LastError))
            {
                _lblError.Text = $"Warning: {_atmInfo.LastError}";
                _lblError.ForeColor = Color.FromArgb(213, 0, 0);
            }
            else
            {
                _lblError.Text = string.Empty;
            }

            Invalidate();
        }

        private (string, Color, Color) GetStatusInfo(ATMInfo atm)
        {
            if (atm.ConnectionStatus == ConnectionStatus.Connected)
            {
                if (atm.Status == ATMStatus.Supervisor)
                    return ("Supervisor mode", Color.FromArgb(255, 109, 0), Color.FromArgb(255, 109, 0));
                if (atm.IsSyncing)
                    return ("Syncing", Color.FromArgb(13, 110, 253), Color.FromArgb(13, 110, 253));
                return ("Connected and active", Color.FromArgb(0, 200, 83), Color.FromArgb(0, 200, 83));
            }

            var offline = DateTime.UtcNow - atm.LastConnectionTime;
            if (offline.TotalMinutes > 10)
                return ("Critical disconnect", Color.FromArgb(213, 0, 0), Color.FromArgb(97, 97, 97));
            if (offline.TotalMinutes > 5)
                return ("Disconnect warning", Color.FromArgb(255, 109, 0), Color.FromArgb(255, 109, 0));

            return ("Disconnected", Color.FromArgb(170, 170, 170), Color.FromArgb(55, 55, 55));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = GetRoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 8);
            using var borderPen = new Pen(Color.FromArgb(55, 55, 55), 1);
            g.DrawPath(borderPen, path);
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

}
