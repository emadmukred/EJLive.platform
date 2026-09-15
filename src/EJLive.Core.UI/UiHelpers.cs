using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Core.UI;

/// <summary>
/// Shared WinForms control factory. Wave 4 unification: every surface
/// (ClientMainForm, ServerMainForm, MonitoringConsoleForm, InstallerForm,
/// JournalStudioForm) had its own private <c>Ui</c> static class with
/// overlapping responsibilities. This module is the single source of
/// truth; the four forms migrate to <c>using EJLive.Core.UI;</c> in
/// follow-up commits.
///
/// All methods are <c>static</c> and return ready-to-use controls with
/// the canonical <see cref="LightUiTheme"/> applied. Buttons use
/// <see cref="Action"/>-based handlers — the caller is responsible for
/// making the action thread-safe (the caller is on the UI thread when
/// invoking these factories).
/// </summary>
public static class UiHelpers
{
    public const int DefaultFlowHeight = 58;
    public const int DefaultCardHeight = 92;
    public const int DefaultPadding = 8;

    /// <summary>A vertical stack that fills its parent with consistent padding.</summary>
    public static Panel Stack() => new()
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(DefaultPadding)
    };

    /// <summary>A horizontal toolbar flow (wraps when narrow).</summary>
    public static FlowLayoutPanel Flow(int height = DefaultFlowHeight) => new()
    {
        Dock = DockStyle.Top,
        Height = height,
        Padding = new Padding(DefaultPadding),
        WrapContents = true
    };

    /// <summary>A primary call-to-action button.</summary>
    public static Button Button(string text, Action onClick, int height = 32)
    {
        var b = new Button
        {
            Text = text,
            AutoSize = true,
            Height = height,
            Margin = new Padding(4)
        };
        if (onClick is not null)
            b.Click += (_, _) => onClick();
        return b;
    }

    /// <summary>A grid wrapper that applies the canonical theme on construction.</summary>
    public static DataGridView Grid(bool readOnly = true)
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = readOnly,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = LightUiTheme.Surface
        };
        LightUiTheme.ApplyToGrid(grid);
        grid.EnableDoubleBuffering();
        return grid;
    }

    /// <summary>A read-only log/output box (Consolas, fills parent).</summary>
    public static RichTextBox LogBox() => new()
    {
        Dock = DockStyle.Fill,
        Font = LightUiTheme.MonoFont,
        ReadOnly = true,
        BackColor = Color.FromArgb(252, 252, 253)
    };

    /// <summary>A row of N metric cards; caller adds cards via <see cref="AddMetricCard"/>.</summary>
    public static TableLayoutPanel CardRow(int columns, int height = DefaultCardHeight)
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = height,
            ColumnCount = columns,
            Padding = new Padding(4)
        };
        for (var i = 0; i < columns; i++)
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
        return row;
    }

    /// <summary>A single metric card (label + value with an accent stripe).</summary>
    public static Label AddMetricCard(TableLayoutPanel row, string title, string value, Color accent)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(4),
            Padding = new Padding(10),
            BackColor = LightUiTheme.Surface
        };
        var accentBar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 4,
            BackColor = accent
        };
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 22,
            ForeColor = Color.FromArgb(90, 90, 90),
            Font = new Font("Segoe UI", 8F)
        };
        var valueLabel = new Label
        {
            Text = value,
            Dock = DockStyle.Fill,
            ForeColor = LightUiTheme.Ink,
            Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        card.Controls.Add(valueLabel);
        card.Controls.Add(titleLabel);
        card.Controls.Add(accentBar);
        row.Controls.Add(card);
        return valueLabel;
    }

    /// <summary>Format a <see cref="DateTime"/> as a compact elapsed string ("now", "5 min ago", …).</summary>
    public static string ElapsedUtc(DateTime utc)
    {
        if (utc == DateTime.MinValue) return "-";
        var elapsed = DateTime.UtcNow - utc;
        if (elapsed.TotalSeconds < 60) return "now";
        if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes} min ago";
        if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours} hr ago";
        return utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }

    /// <summary>Mask a value like a PAN: keep first 2 + last 2, asterisks in between.</summary>
    public static string Mask(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= 4
            ? new string('*', value.Length)
            : value[..2] + new string('*', value.Length - 4) + value[^2..];
    }

    /// <summary>Format bytes compactly (B / KB / MB / GB).</summary>
    public static string FormatBytes(long value)
    {
        var safe = Math.Max(0, value);
        if (safe >= 1_073_741_824) return $"{safe / 1_073_741_824d:F1} GB";
        if (safe >= 1_048_576) return $"{safe / 1_048_576d:F1} MB";
        if (safe >= 1_024) return $"{safe / 1_024d:F1} KB";
        return $"{safe} B";
    }
}
