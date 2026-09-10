using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace EJLive.Client.Controls;

public class StatusCard : Panel
{
    private Label _titleLabel;
    private Label _valueLabel;
    private Label _subtitleLabel;
    private Panel _indicator;

    public string Title
    {
        get => _titleLabel.Text;
        set => _titleLabel.Text = value;
    }

    public string Value
    {
        get => _valueLabel.Text;
        set => _valueLabel.Text = value;
    }

    public string Subtitle
    {
        get => _subtitleLabel.Text;
        set => _subtitleLabel.Text = value;
    }

    public Color IndicatorColor
    {
        get => _indicator.BackColor;
        set => _indicator.BackColor = value;
    }

    public StatusCard()
    {
        Size = new Size(200, 100);
        BackColor = ThemeColors.Card;
        Padding = new Padding(12);
        Margin = new Padding(8);

        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);

        // Indicator strip on left
        _indicator = new Panel
        {
            Size = new Size(4, 100),
            Dock = DockStyle.Left,
            BackColor = ThemeColors.AccentBlue
        };
        Controls.Add(_indicator);

        // Title
        _titleLabel = new Label
        {
            Text = "Title",
            ForeColor = ThemeColors.TextSecondary,
            Font = new Font("Segoe UI", 9F),
            Dock = DockStyle.Top,
            Height = 22,
            Padding = new Padding(8, 4, 0, 0)
        };
        Controls.Add(_titleLabel);

        // Value
        _valueLabel = new Label
        {
            Text = "0",
            ForeColor = ThemeColors.TextPrimary,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(8, 2, 0, 0)
        };
        Controls.Add(_valueLabel);

        // Subtitle
        _subtitleLabel = new Label
        {
            Text = "Subtitle",
            ForeColor = ThemeColors.TextMuted,
            Font = new Font("Segoe UI", 8F),
            Dock = DockStyle.Top,
            Height = 20,
            Padding = new Padding(8, 2, 0, 0)
        };
        Controls.Add(_subtitleLabel);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var path = GetRoundedRect(ClientRectangle, 8);
        using var brush = new SolidBrush(BackColor);
        g.FillPath(brush, path);

        // Border
        using var pen = new Pen(ThemeColors.Border, 1);
        g.DrawPath(pen, path);
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
