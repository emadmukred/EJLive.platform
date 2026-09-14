using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Core.UI;

/// <summary>
/// Canonical colour palette + typography for every EJLive Windows surface
/// (Endpoint Console, Enterprise Server, NOC, Installer).
///
/// Wave 4 unification: replaces the three diverging palettes that previously
/// lived inline in each form (the inline RGBs in ServerMainForm, the
/// ATMCardPanel callouts in Server.WinForms that read
/// <c>EJLive.Shared.LightUiTheme</c> as a dark theme by accident, and the
/// private constants in ClientMainForm). The values below are the *light*
/// palette the surfaces have converged on; dark mode is out of scope for
/// this wave.
///
/// Tokens:
///   Window       — top-level Form back colour
///   Surface      — card / panel background
///   SurfaceAlt   — zebra / alternating-row tint
///   Ink          — primary text
///   Muted        — secondary text / captions
///   Border       — 1px hairlines on cards
///   Accent       — interactive highlights
///   Healthy / Warning / Failed / Syncing — semantic status colours
///     matching the 7-colour ATM card palette (ATMCardState → colour).
///
/// <see cref="Apply"/> wires the defaults onto a <see cref="Form"/>; the
/// caller adds controls afterwards. <see cref="ApplyToGrid"/> does the
/// same for a <see cref="DataGridView"/> (header, alternating rows, border).
/// </summary>
public static class LightUiTheme
{
    public static readonly Color Window = Color.FromArgb(248, 250, 252);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.FromArgb(250, 251, 253);
    public static readonly Color Ink = Color.FromArgb(31, 41, 55);
    public static readonly Color Muted = Color.FromArgb(100, 116, 139);
    public static readonly Color Border = Color.FromArgb(226, 232, 240);
    public static readonly Color Accent = Color.FromArgb(46, 134, 222);

    public static readonly Color Healthy = Color.FromArgb(16, 172, 132);
    public static readonly Color Warning = Color.FromArgb(255, 159, 67);
    public static readonly Color Failed = Color.FromArgb(238, 82, 83);
    public static readonly Color Syncing = Color.FromArgb(46, 134, 222);
    public static readonly Color Idle = Color.FromArgb(100, 116, 139);
    public static readonly Color Supervisor = Color.FromArgb(217, 119, 6);

    public static readonly Font HeadingFont = new("Segoe UI Semibold", 14F, FontStyle.Bold);
    public static readonly Font TitleFont = new("Segoe UI", 19F, FontStyle.Bold);
    public static readonly Font BodyFont = new("Segoe UI", 9F);
    public static readonly Font MonoFont = new("Consolas", 9F);

    public static void Apply(Form form)
    {
        form.BackColor = Window;
        form.Font = BodyFont;
    }

    public static void ApplyToGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Ink;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
        grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceAlt;
        grid.RowHeadersVisible = false;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        grid.DefaultCellStyle.SelectionForeColor = Ink;
    }
}
