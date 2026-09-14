using System;
using System.Drawing;
using System.Windows.Forms;
using EJLive.Core.Models;
using EJLive.Shared;

namespace EJLive.Server.WinForms;

/// <summary>
/// Interactive ATM card (Wave 1 / SS-17 promotion from
/// <c>src/_reference/uncompiled/EJLive.Server.WinForms/ATMCardPanel.cs</c>).
///
/// Renders one ATM on the Network Map with a 7-colour status palette
/// (green/yellow/blue/orange/red/gray/dark-gray) and a blink animation while
/// the ATM is in <see cref="ATMCardState.Syncing"/>. Double-click opens the
/// detail drawer; the host wires that through <see cref="OnDoubleClickCard"/>.
///
/// Differences from the archived copy:
///   * <c>atm.GetElapsed(DateTime)</c> did not exist on <see cref="ATMInfo"/>;
///     the elapsed formatter lives here as a private static helper so the
///     server-only WinForms layer owns its UI formatting (Core stays free of
///     string-display methods).
///   * Comments translated to English (operator-visible copy stays in
///     <see cref="LanguageManager"/>; source prose is English only).
/// </summary>
public class ATMCardPanel : Panel
{
    private ATMInfo _atm;
    private Label _lblId = null!;
    private Label _lblName = null!;
    private Label _lblType = null!;
    private Label _lblStatus = null!;
    private Label _lblLastJournal = null!;
    private Label _lblLastError = null!;
    private Label _lblHB = null!;
    private Label _lblLatency = null!;
    private Label _lblSyncStats = null!;
    private Label _lblNetwork = null!;
    private Panel _colorBar = null!;
    private Panel _headerPanel = null!;
    private Timer? _blinkTimer;
    private bool _blinkState;

    /// <summary>Raised when the operator double-clicks the card body or status line.</summary>
    public event EventHandler<ATMInfo>? OnDoubleClickCard;

    public ATMCardPanel(ATMInfo atm)
    {
        _atm = atm;
        InitializeCard();
        UpdateATM(atm);
    }

    private void InitializeCard()
    {
        Size = new Size(240, 170);
        BackColor = LightUiTheme.Surface;
        Cursor = Cursors.Hand;
        Margin = new Padding(6);
        BorderStyle = BorderStyle.None;

        // Top colour bar — accent for the current ATMCardState.
        _colorBar = new Panel { Height = 5, Dock = DockStyle.Top };

        // Header row: ATM id (left) + vendor type (right).
        _headerPanel = new Panel { Height = 32, Dock = DockStyle.Top, Padding = new Padding(8, 6, 8, 0) };
        _lblId = new Label { AutoSize = true, ForeColor = LightUiTheme.Text, Font = new Font("Consolas", 10F, FontStyle.Bold) };
        _lblType = new Label { AutoSize = true, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 8F), Dock = DockStyle.Right };
        _headerPanel.Controls.AddRange(new Control[] { _lblId, _lblType });

        // Status line (accent-coloured).
        _lblStatus = new Label
        {
            Height = 20,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(4, 0, 4, 0)
        };

        _lblName = new Label { Height = 18, Dock = DockStyle.Top, ForeColor = LightUiTheme.Text, Font = new Font("Segoe UI", 9F), TextAlign = ContentAlignment.MiddleCenter };
        _lblNetwork = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleCenter };
        _lblHB = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleCenter };
        _lblLatency = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleCenter };
        _lblLastJournal = new Label { Height = 18, Dock = DockStyle.Top, ForeColor = Color.FromArgb(25, 135, 84), Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleCenter };
        _lblLastError = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = Color.FromArgb(180, 35, 24), Font = new Font("Consolas", 7.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
        _lblSyncStats = new Label { Height = 16, Dock = DockStyle.Bottom, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7F), TextAlign = ContentAlignment.MiddleCenter };

        Controls.AddRange(new Control[]
        {
            _colorBar, _headerPanel, _lblStatus, _lblName, _lblNetwork,
            _lblHB, _lblLatency, _lblLastJournal, _lblLastError, _lblSyncStats
        });

        // Drill-down events.
        DoubleClick += (_, _) => OnDoubleClickCard?.Invoke(this, _atm);
        _lblStatus.DoubleClick += (_, _) => OnDoubleClickCard?.Invoke(this, _atm);
        AttachHoverEffect();

        // Blink timer for the Syncing state (started/stopped in UpdateATM).
        _blinkTimer = new Timer { Interval = 800 };
        _blinkTimer.Tick += BlinkTick;
    }

    public void UpdateATM(ATMInfo atm)
    {
        _atm = atm;
        if (IsDisposed || !IsHandleCreated) return;
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => UpdateATM(atm)));
            return;
        }

        var state = atm.GetCardState();
        var cardColor = atm.GetCardColor();

        _colorBar.BackColor = cardColor;
        _lblId.Text = atm.ATM_ID ?? "?";
        _lblType.Text = atm.ATM_Type ?? string.Empty;
        _lblName.Text = atm.ATM_Name ?? atm.ATM_ID ?? string.Empty;
        _lblStatus.Text = atm.GetStatusLabel();
        _lblStatus.ForeColor = cardColor;
        _lblNetwork.Text = $"NET {atm.NetworkType} | {atm.BranchName ?? "—"}";
        _lblHB.Text = $"HB {FormatElapsed(atm.LastHeartbeatUtc)} | EJ {FormatElapsed(atm.LastSyncUtc)}";
        _lblLatency.Text = $"LAT {atm.Latency_ms} ms";
        _lblLastJournal.Text = !string.IsNullOrEmpty(atm.LastJournalFile) ? $"FILE {atm.LastJournalFile}" : string.Empty;
        _lblLastError.Text = !string.IsNullOrEmpty(atm.LastErrorCode) ? $"WARN {atm.LastErrorCode}" : string.Empty;
        _lblSyncStats.Text = $"OK:{atm.ApprovedTransactions} FAIL:{atm.FailedTransactions} CARD:{atm.CardsCaptured}";

        // Background by state.
        BackColor = state == ATMCardState.ConnectedActive
            ? Color.FromArgb(232, 245, 233)
            : state == ATMCardState.CriticalOffline
                ? Color.FromArgb(255, 235, 238)
                : LightUiTheme.Surface;

        // Blink on Syncing only.
        if (_blinkTimer is not null)
        {
            if (state == ATMCardState.Syncing && !_blinkTimer.Enabled)
                _blinkTimer.Start();
            else if (state != ATMCardState.Syncing && _blinkTimer.Enabled)
                _blinkTimer.Stop();
        }

        _lblSyncStats.Text = $"{_lblSyncStats.Text} | H:{atm.HealthScore}%";

        Invalidate();
    }

    private void BlinkTick(object? sender, EventArgs e)
    {
        _blinkState = !_blinkState;
        _colorBar.BackColor = _blinkState ? Color.FromArgb(10, 132, 255) : Color.FromArgb(0, 80, 200);
    }

    private void AttachHoverEffect()
    {
        MouseEnter += (_, _) =>
        {
            if (IsDisposed) return;
            using var g = CreateGraphics();
            ControlPaint.DrawBorder(g, ClientRectangle, Color.FromArgb(99, 99, 102), ButtonBorderStyle.Solid);
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        using var pen = new Pen(LightUiTheme.Border, 1);
        g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _blinkTimer?.Dispose();
        base.Dispose(disposing);
    }

    private static string FormatElapsed(DateTime utc)
    {
        if (utc == DateTime.MinValue) return "-";
        var elapsed = DateTime.UtcNow - utc;
        if (elapsed.TotalSeconds < 60) return "now";
        if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes} min ago";
        if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours} hr ago";
        return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }
}
