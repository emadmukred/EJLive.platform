using System;
using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Client.Controls;

public class DarkButton : Button
{
    private bool _isHovered = false;
    private bool _isPressed = false;

    public Color ButtonColor { get; set; } = ThemeColors.AccentBlue;
    public Color HoverColor { get; set; } = Color.FromArgb(11, 88, 208);
    public Color PressedColor { get; set; } = Color.FromArgb(9, 72, 168);
    public int BorderRadius { get; set; } = 4;

    public DarkButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = ButtonColor;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        Cursor = Cursors.Hand;
        Size = new Size(120, 36);

        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var color = _isPressed ? PressedColor : (_isHovered ? HoverColor : ButtonColor);
        using var brush = new SolidBrush(color);
        using var path = GetRoundedRect(ClientRectangle, BorderRadius);
        g.FillPath(brush, path);

        // Text
        var textColor = Enabled ? ForeColor : Color.Gray;
        using var textBrush = new SolidBrush(textColor);
        var textSize = g.MeasureString(Text, Font);
        var textX = (Width - textSize.Width) / 2;
        var textY = (Height - textSize.Height) / 2;
        g.DrawString(Text, Font, textBrush, textX, textY);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        _isPressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _isPressed = true;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isPressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    private System.Drawing.Drawing2D.GraphicsPath GetRoundedRect(Rectangle rect, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
