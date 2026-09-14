using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace EJLive.Core.UI;

/// <summary>
/// Owner-drawn bar histogram. WinForms-only (no external chart framework).
/// Honours light backgrounds and accessible fore colours. Used by the
/// Journal Studio analytics tab; promoted here in Wave 4 so any future
/// surface (Monitoring / Server) can reuse it without re-implementing.
/// </summary>
public sealed class OwnerDrawnHistogram : Control
{
    public string Title { get; set; } = string.Empty;
    private List<(string Label, double Value)> _data = new();

    public void SetData(IEnumerable<(string Label, double Value)> data)
        => _data = data.ToList();

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        var titleHeight = 24;
        using var titleFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        g.DrawString(Title, titleFont, Brushes.Black, 8, 4);

        var chartRect = new RectangleF(8, titleHeight, Width - 16, Height - titleHeight - 8);
        if (_data.Count == 0)
        {
            g.DrawString("(no data)", titleFont, Brushes.Gray, chartRect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }

        var max = Math.Max(1, _data.Max(d => d.Value));
        var barWidth = chartRect.Width / Math.Max(_data.Count, 1);
        using var barBrush = new SolidBrush(LightUiTheme.Accent);
        for (var i = 0; i < _data.Count; i++)
        {
            var value = _data[i].Value;
            var height = (float)(value / max * (chartRect.Height - 20));
            var x = chartRect.X + i * barWidth;
            var y = chartRect.Y + (chartRect.Height - 20 - height);
            g.FillRectangle(barBrush, x + 2, y, (float)barWidth - 4, height);
            g.DrawString(((int)value).ToString(), Font, Brushes.Black, x, y - 16);
            g.DrawString(_data[i].Label, Font, Brushes.Black, x, chartRect.Bottom - 16);
        }
    }
}

/// <summary>
/// Owner-drawn line chart for time-series (hourly throughput, etc.). Simple
/// but accessible and fully under our control — no third-party dependency.
/// </summary>
public sealed class OwnerDrawnTimeSeries : Control
{
    public string Title { get; set; } = string.Empty;
    private List<(DateTime Hour, double Value)> _points = new();

    public void SetData(IEnumerable<(DateTime Hour, double Value)> data)
        => _points = data.ToList();

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        var titleHeight = 24;
        using var titleFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        g.DrawString(Title, titleFont, Brushes.Black, 8, 4);

        var chartRect = new RectangleF(8, titleHeight, Width - 16, Height - titleHeight - 18);
        if (_points.Count < 2)
        {
            g.DrawString("(not enough points)", titleFont, Brushes.Gray, chartRect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            return;
        }

        var max = Math.Max(1, _points.Max(p => p.Value));
        var step = chartRect.Width / Math.Max(_points.Count - 1, 1);
        using var linePen = new Pen(LightUiTheme.Healthy, 2f);
        var prevX = chartRect.X;
        var prevY = (float)(chartRect.Bottom - (_points[0].Value / max) * chartRect.Height);
        for (var i = 1; i < _points.Count; i++)
        {
            var x = chartRect.X + i * step;
            var y = (float)(chartRect.Bottom - (_points[i].Value / max) * chartRect.Height);
            g.DrawLine(linePen, prevX, prevY, x, y);
            prevX = x;
            prevY = y;
        }
    }
}
