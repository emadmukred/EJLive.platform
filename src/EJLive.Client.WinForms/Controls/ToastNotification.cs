using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace EJLive.Client.WinForms.Controls
{
    /// <summary>
    /// Professional toast notifications for operational feedback.
    /// </summary>
    public class ToastNotification : Form
    {
        private readonly string _message;
        private readonly ToastType _type;
        private readonly System.Windows.Forms.Timer _closeTimer;
        private float _opacity = 0f;
        private readonly System.Windows.Forms.Timer _fadeTimer;
        private bool _isFadingOut = false;

        private static int _stackOffset = 0;
        private static readonly object _stackLock = new object();

        private ToastNotification(string message, ToastType type, int durationMs)
        {
            _message = message;
            _type = type;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ShowInTaskbar = false;
            Size = new Size(380, 72);
            BackColor = GetBackColor();
            Opacity = 0;
            DoubleBuffered = true;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);

            PositionWindow();

            _closeTimer = new System.Windows.Forms.Timer { Interval = durationMs };
            _closeTimer.Tick += (s, e) =>
            {
                _closeTimer.Stop();
                _isFadingOut = true;
            };

            _fadeTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _fadeTimer.Tick += OnFadeTick;
            _fadeTimer.Start();
        }

        private void PositionWindow()
        {
            var screen = Screen.PrimaryScreen?.WorkingArea ?? SystemInformation.WorkingArea;
            int stackIndex;
            lock (_stackLock)
            {
                stackIndex = _stackOffset;
                _stackOffset++;
            }
            Location = new Point(
                screen.Right - Width - 16,
                screen.Bottom - Height - 16 - (stackIndex * (Height + 8)));
        }

        private void OnFadeTick(object? sender, EventArgs e)
        {
            if (!_isFadingOut)
            {
                _opacity = Math.Min(1f, _opacity + 0.08f);
                Opacity = _opacity;
                if (_opacity >= 1f)
                    _closeTimer.Start();
            }
            else
            {
                _opacity = Math.Max(0f, _opacity - 0.06f);
                Opacity = _opacity;
                if (_opacity <= 0f)
                {
                    _fadeTimer.Stop();
                    lock (_stackLock) { _stackOffset = Math.Max(0, _stackOffset - 1); }
                    Close();
                    Dispose();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using var path = GetRoundedRect(rect, 10);
            using var brush = new SolidBrush(GetBackColor());
            g.FillPath(brush, path);

            using var accentBrush = new SolidBrush(GetAccentColor());
            g.FillRectangle(accentBrush, 0, 0, 5, Height);

            string icon = GetIcon();
            using var iconFont = new Font("Segoe UI", 16F);
            using var iconBrush = new SolidBrush(GetAccentColor());
            g.DrawString(icon, iconFont, iconBrush, new PointF(14, (Height - 26) / 2f));

            string title = GetTitle();
            using var titleFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            using var titleBrush = new SolidBrush(Color.FromArgb(224, 224, 224));
            g.DrawString(title, titleFont, titleBrush, new PointF(46, 12));

            using var msgFont = new Font("Segoe UI", 9F);
            using var msgBrush = new SolidBrush(Color.FromArgb(170, 170, 170));
            var msgRect = new RectangleF(46, 34, Width - 60, 28);
            g.DrawString(_message, msgFont, msgBrush, msgRect);

            using var borderPen = new Pen(Color.FromArgb(55, 55, 55), 1);
            g.DrawPath(borderPen, path);
        }

        private Color GetBackColor() => Color.FromArgb(30, 30, 30);

        private Color GetAccentColor() => _type switch
        {
            ToastType.Success => Color.FromArgb(0, 200, 83),
            ToastType.Warning => Color.FromArgb(255, 109, 0),
            ToastType.Error => Color.FromArgb(213, 0, 0),
            ToastType.Info => Color.FromArgb(13, 110, 253),
            _ => Color.FromArgb(13, 110, 253)
        };

        private string GetIcon() => _type switch
        {
            ToastType.Success => "✔",
            ToastType.Warning => "⚠",
            ToastType.Error => "✖",
            ToastType.Info => "ℹ",
            _ => "ℹ"
        };

        private string GetTitle() => _type switch
        {
            ToastType.Success => "Success",
            ToastType.Warning => "Warning",
            ToastType.Error => "Error",
            ToastType.Info => "Information",
            _ => "Notification"
        };

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

        protected override void OnMouseClick(MouseEventArgs e)
        {
            _isFadingOut = true;
            base.OnMouseClick(e);
        }

        public static void ShowSuccess(string message, int durationMs = 4000) =>
            Show(message, ToastType.Success, durationMs);

        public static void ShowWarning(string message, int durationMs = 5000) =>
            Show(message, ToastType.Warning, durationMs);

        public static void ShowError(string message, int durationMs = 6000) =>
            Show(message, ToastType.Error, durationMs);

        public static void ShowInfo(string message, int durationMs = 3500) =>
            Show(message, ToastType.Info, durationMs);

        private static void Show(string message, ToastType type, int durationMs)
        {
            if (Application.OpenForms.Count == 0) return;
            var owner = Application.OpenForms[0];
            if (owner == null)
                return;
            if (owner.InvokeRequired)
            {
                owner.Invoke(new Action(() => Show(message, type, durationMs)));
                return;
            }
            var toast = new ToastNotification(message, type, durationMs);
            toast.Show();
        }
    }

    public enum ToastType { Success, Warning, Error, Info }
}
