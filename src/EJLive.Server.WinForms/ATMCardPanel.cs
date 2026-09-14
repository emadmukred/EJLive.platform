using System;
using System.Drawing;
using System.Windows.Forms;
using EJLive.Core.Models;
using EJLive.Core.UI;

namespace EJLive.Server.WinForms;

/// <summary>
/// Interactive ATM card (Wave 1 / SS-17 promotion from
/// <c>src/_reference/uncompiled/EJLive.Server.WinForms/ATMCardPanel.cs</c>,
/// Wave 4 light-palette refresh).
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
///   * Wave 4: palette moved to <see cref="LightUiTheme"/> (canonical light
///     palette in <c>EJLive.Core.UI</c>) and the per-state BackColor switch
///     uses the same accent the status badge carries, so the card body
///     harmonises with the status colour rather than fighting it.
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

        _colorBar = new Panel { Height = 5, Dock = DockStyle.Top };

        _headerPanel = new Panel { Height = 32, Dock = DockStyle.Top, Padding = new Padding(8, 6, 8, 0) };
        _lblId = new Label { AutoSize = true, ForeColor = LightUiTheme.Ink, Font = new Font("Consolas", 10F, FontStyle.Bold) };
        _lblType = new Label { AutoSize = true, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 8F), Dock = DockStyle.Right };
        _headerPanel.Controls.AddRange(new Control[] { _lblId, _lblType });

        _lblStatus = new Label
        {
            Height = 20,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Padding = new Padding(4, 0, 4, 0)
        };

        _lblName = new Label { Height = 18, Dock = DockStyle.Top, ForeColor = LightUiTheme.Ink, Font = new Font("Segoe UI", 9F), TextAlign = ContentAlignment.MiddleCenter };
        _lblNetwork = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleCenter };
        _lblHB = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleCenter };
        _lblLatency = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleCenter };
        _lblLastJournal = new Label { Height = 18, Dock = DockStyle.Top, ForeColor = LightUiTheme.Healthy, Font = new Font("Segoe UI", 7.5F), TextAlign = ContentAlignment.MiddleCenter };
        _lblLastError = new Label { Height = 16, Dock = DockStyle.Top, ForeColor = LightUiTheme.Failed, Font = new Font("Consolas", 7.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
        _lblSyncStats = new Label { Height = 16, Dock = DockStyle.Bottom, ForeColor = LightUiTheme.Muted, Font = new Font("Segoe UI", 7F), TextAlign = ContentAlignment.MiddleCenter };

        Controls.AddRange(new Control[]
        {
            _colorBar, _headerPanel, _lblStatus, _lblName, _lblNetwork,
            _lblHB, _lblLatency, _lblLastJournal, _lblLastError, _lblSyncStats
        });

        DoubleClick += (_, _) => OnDoubleClickCard?.Invoke(this, _atm);
        _lblStatus.DoubleClick += (_, _) => OnDoubleClickCard?.Invoke(this, _atm);
        AttachHoverEffect();

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
        _lblHB.Text = $"HB {UiHelpers.ElapsedUtc(atm.LastHeartbeatUtc)} | EJ {UiHelpers.ElapsedUtc(atm.LastSyncUtc)}";
        _lblLatency.Text = $"LAT {atm.Latency_ms} ms";
        _lblLastJournal.Text = !string.IsNullOrEmpty(atm.LastJournalFile) ? $"FILE {atm.LastJournalFile}" : string.Empty;
        _lblLastError.Text = !string.IsNullOrEmpty(atm.LastErrorCode) ? $"WARN {atm.LastErrorCode}" : string.Empty;
        _lblSyncStats.Text = $"OK:{atm.ApprovedTransactions} FAIL:{atm.FailedTransactions} CARD:{atm.CardsCaptured}";

        // Card body uses a soft state colour so the operator reads the
        // status at a glance (the badge carries the saturated accent).
        BackColor = state switch
        {
            ATMCardState.ConnectedActive => SoftColor(LightUiTheme.Healthy),
            ATMCardState.ConnectedIdle => SoftColor(LightUiTheme.Warning),
            ATMCardState.Syncing or ATMCardState.WaitingReply => SoftColor(LightUiTheme.Syncing),
            ATMCardState.Supervisor => SoftColor(LightUiTheme.Supervisor),
            ATMCardState.RecentlyDisconnected or ATMCardState.WarningOffline => SoftColor(LightUiTheme.Failed),
            ATMCardState.CriticalOffline => Color.FromArgb(241, 245, 249),
            _ => LightUiTheme.Surface
        };

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

    private static Color SoftColor(Color accent)
        => Color.FromArgb(
            (int)Math.Min(255, accent.R + 32),
            (int)Math.Min(255, accent.G + 32),
            (int)Math.Min(255, accent.B + 32));

    private void BlinkTick(object? sender, EventArgs e)
    {
        _blinkState = !_blinkState;
        _colorBar.BackColor = _blinkState ? LightUiTheme.Accent : LightUiTheme.Syncing;
    }

    private void AttachHoverEffect()
    {
        MouseEnter += (_, _) =>
        {
            if (IsDisposed) return;
            using var g = CreateGraphics();
            ControlPaint.DrawBorder(g, ClientRectangle, LightUiTheme.Accent, ButtonBorderStyle.Solid);
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
}
