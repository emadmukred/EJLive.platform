using System;
using System.Drawing;
using System.Windows.Forms;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Server.WinForms
{
    /// <summary>
    /// بطاقة الصراف التفاعلية — تعرض حالة الصراف بنظام الألوان الكامل
    /// 7 ألوان: أخضر/أصفر/أزرق/برتقالي/أحمر/رمادي/رمادي داكن
    /// عند الضغط مرتين: فتح واجهة قراءة الجورنال التفصيلية
    /// </summary>
    public class ATMCardPanel : Panel
    {
        private ATMInfo _atm;
        private Label   _lblId, _lblName, _lblType, _lblStatus;
        private Label   _lblLastJournal, _lblLastError, _lblHB, _lblLatency;
        private Label   _lblSyncStats, _lblNetwork;
        private Panel   _colorBar;
        private Panel   _headerPanel;
        private Timer   _blinkTimer;
        private bool    _blinkState;

        public event EventHandler<ATMInfo> OnDoubleClickCard;

        public ATMCardPanel(ATMInfo atm)
        {
            _atm = atm;
            InitializeCard();
            UpdateATM(atm);
        }

        private void InitializeCard()
        {
            Size        = new Size(240, 170);
            BackColor   = LightUiTheme.Surface;
            Cursor      = Cursors.Hand;
            Margin      = new Padding(6);
            BorderStyle = BorderStyle.None;

            // شريط اللون العلوي
            _colorBar = new Panel { Height = 5, Dock = DockStyle.Top };

            // Header (ID + Type)
            _headerPanel = new Panel { Height = 32, Dock = DockStyle.Top, Padding = new Padding(8, 6, 8, 0) };
            _lblId   = new Label { AutoSize = true, ForeColor = LightUiTheme.Text, Font = new Font("Consolas", 10f, FontStyle.Bold) };
            _lblType = new Label { AutoSize = true, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 8f), Dock = DockStyle.Right };
            _headerPanel.Controls.AddRange(new Control[] { _lblId, _lblType });

            // حالة
            _lblStatus = new Label
            {
                Height    = 20,
                Dock      = DockStyle.Top,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding   = new Padding(4, 0, 4, 0)
            };

            // اسم الصراف
            _lblName = new Label { Height = 18, Dock = DockStyle.Top, ForeColor = LightUiTheme.Text, Font = new Font("Segoe UI", 9f), TextAlign = ContentAlignment.MiddleCenter };

            // شبكة
            _lblNetwork = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7.5f), TextAlign = ContentAlignment.MiddleCenter };

            // آخر Heartbeat + Latency
            _lblHB      = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7.5f), TextAlign = ContentAlignment.MiddleCenter };
            _lblLatency = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7.5f), TextAlign = ContentAlignment.MiddleCenter };

            // آخر جورنال
            _lblLastJournal = new Label { Height = 18, Dock = DockStyle.Top, ForeColor = Color.FromArgb(25, 135, 84), Font = new Font("Segoe UI", 7.5f), TextAlign = ContentAlignment.MiddleCenter };

            // آخر خطأ (أحمر)
            _lblLastError = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = Color.FromArgb(180, 35, 24), Font = new Font("Consolas", 7.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

            // إحصائيات
            _lblSyncStats = new Label { Height = 16, Dock = DockStyle.Bottom, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7f), TextAlign = ContentAlignment.MiddleCenter };

            Controls.AddRange(new Control[] {
                _colorBar, _headerPanel, _lblStatus, _lblName, _lblNetwork,
                _lblHB, _lblLatency, _lblLastJournal, _lblLastError, _lblSyncStats
            });

            // أحداث
            DoubleClick             += (s, e) => OnDoubleClickCard?.Invoke(this, _atm);
            _lblStatus.DoubleClick  += (s, e) => OnDoubleClickCard?.Invoke(this, _atm);
            _pbMouseEnter();

            // Blink Timer للحالة النشطة
            _blinkTimer = new Timer { Interval = 800 };
            _blinkTimer.Tick += BlinkTick;
        }

        public void UpdateATM(ATMInfo atm)
        {
            _atm = atm;
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired) { Invoke(new Action(() => UpdateATM(atm))); return; }

            var state     = atm.GetCardState();
            var cardColor = atm.GetCardColor();

            _colorBar.BackColor   = cardColor;
            _lblId.Text           = atm.ATM_ID ?? "?";
            _lblType.Text         = atm.ATM_Type ?? "";
            _lblName.Text         = atm.ATM_Name ?? atm.ATM_ID;
            _lblStatus.Text       = atm.GetStatusLabel();
            _lblStatus.ForeColor  = cardColor;
            _lblNetwork.Text      = $"NET {atm.NetworkType} | {atm.BranchName ?? "—"}";
            _lblHB.Text           = $"HB {atm.GetElapsed(atm.LastHeartbeatUtc)} | EJ {atm.GetElapsed(atm.LastSyncUtc)}";
            _lblLatency.Text      = $"LAT {atm.Latency_ms} ms";
            _lblLastJournal.Text  = !string.IsNullOrEmpty(atm.LastJournalFile) ? $"FILE {atm.LastJournalFile}" : "";
            _lblLastError.Text    = !string.IsNullOrEmpty(atm.LastErrorCode)   ? $"WARN {atm.LastErrorCode}" : "";
            _lblSyncStats.Text    = $"OK:{atm.ApprovedTransactions} FAIL:{atm.FailedTransactions} CARD:{atm.CardsCaptured}";

            // تأثير الخلفية حسب الحالة
            _lblSyncStats.Text = $"{_lblSyncStats.Text} | H:{atm.HealthScore}%";

            BackColor = state == ATMCardState.ConnectedActive
                ? Color.FromArgb(232, 245, 233)
                : state == ATMCardState.CriticalOffline
                ? Color.FromArgb(255, 235, 238)
                : LightUiTheme.Surface;

            // Blink عند المزامنة
            if (state == ATMCardState.Syncing && !_blinkTimer.Enabled)
                _blinkTimer.Start();
            else if (state != ATMCardState.Syncing && _blinkTimer.Enabled)
                _blinkTimer.Stop();

            Invalidate();
        }

        private void BlinkTick(object s, EventArgs e)
        {
            _blinkState    = !_blinkState;
            _colorBar.BackColor = _blinkState ? Color.FromArgb(10, 132, 255) : Color.FromArgb(0, 80, 200);
        }

        // Hover effect
        private void _pbMouseEnter()
        {
            MouseEnter += (s, e) =>
            {
                using var g = CreateGraphics();
                ControlPaint.DrawBorder(g, ClientRectangle, Color.FromArgb(99, 99, 102), ButtonBorderStyle.Solid);
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            // رسم حدود دقيقة
            using var pen = new System.Drawing.Pen(LightUiTheme.Border, 1);
            g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _blinkTimer?.Dispose();
            base.Dispose(disposing);
        }
    }
}
