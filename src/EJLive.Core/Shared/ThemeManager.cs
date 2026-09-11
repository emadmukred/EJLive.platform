using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace EJLive.Shared
{
    public static class ThemeManager
    {
        // لوحة ألوان مريحة للعين — دافئة ومتدرجة
        public static class Palette
        {
            // ألوان أساسية دافئة
            public static Color Primary    = Color.FromArgb(99, 102, 241);   // نيلي دافئ
            public static Color Success    = Color.FromArgb(16, 185, 129);   // أخضر زمردي
            public static Color Warning    = Color.FromArgb(245, 158, 11);   // كهرماني
            public static Color Danger     = Color.FromArgb(239, 68, 68);    // أحمر دافئ
            public static Color Info       = Color.FromArgb(6, 182, 212);    // سماوي
            public static Color Accent     = Color.FromArgb(139, 92, 246);   // بنفسجي فاتح

            // خلفيات متدرجة دافئة (بدلاً من الأسود الصارم)
            public static Color BgDeep     = Color.FromArgb(15, 23, 42);     // أزرق غامق عميق
            public static Color BgSurface  = Color.FromArgb(30, 41, 59);     // سطحي دافئ
            public static Color BgCard     = Color.FromArgb(40, 52, 72);     // كروت
            public static Color BgElevated = Color.FromArgb(51, 65, 85);     // مرتفع
            public static Color BgInput    = Color.FromArgb(20, 30, 48);     // حقول إدخال

            // نصوص مريحة
            public static Color TextPrimary   = Color.FromArgb(226, 232, 240);  // أبيض دافئ
            public static Color TextSecondary = Color.FromArgb(148, 163, 184);  // رمادي فاتح
            public static Color TextMuted     = Color.FromArgb(100, 116, 139);  // رمادي خافت
            public static Color TextAccent    = Color.FromArgb(165, 180, 252);  // بنفسجي فاتح

            // حدود ناعمة
            public static Color Border       = Color.FromArgb(51, 65, 85);
            public static Color BorderLight  = Color.FromArgb(71, 85, 105);
        }

        public static void Apply(Form form)
        {
            form.BackColor = Palette.BgDeep;
            form.ForeColor = Palette.TextPrimary;
            form.Font = new Font("Segoe UI", 9.5F);
            RecursiveTheme(form);
        }

        private static void RecursiveTheme(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button btn) StyleButton(btn);
                else if (c is TextBox txt) StyleInput(txt);
                else if (c is ComboBox cmb) StyleCombo(cmb);
                else if (c is DataGridView g) StyleGrid(g);
                else if (c is RichTextBox r) StyleRichText(r);
                else if (c is StatusStrip ss) ss.BackColor = Palette.BgDeep;
                if (c.HasChildren) RecursiveTheme(c);
            }
        }

        public static Button Create(string text, Color accent, Action onClick, int w = 130)
        {
            var b = new Button
            {
                Text = text, Width = w, Height = 36, FlatStyle = FlatStyle.Flat,
                BackColor = accent, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(5), Cursor = Cursors.Hand,
            };
            b.FlatAppearance.BorderSize = 0;
            b.MouseEnter += (s, e) => b.BackColor = Lighten(accent, 0.12f);
            b.MouseLeave += (s, e) => b.BackColor = accent;
            b.Click += (s, e) => onClick();
            return b;
        }

        public static Button CreateOutline(string text, Color accent, Action onClick, int w = 130)
        {
            var b = new Button
            {
                Text = text, Width = w, Height = 36, FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent, ForeColor = accent,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(5), Cursor = Cursors.Hand,
            };
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = accent;
            b.MouseEnter += (s, e) => { b.BackColor = Color.FromArgb(25, accent); b.ForeColor = Color.White; };
            b.MouseLeave += (s, e) => { b.BackColor = Color.Transparent; b.ForeColor = accent; };
            b.Click += (s, e) => onClick();
            return b;
        }

        public static void StyleButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
        }

        public static Panel Card(string title, string value, Color accent, int w = 220)
        {
            var c = new Panel { Width = w, Height = 92, Margin = new Padding(6), BackColor = Palette.BgCard };
            c.Paint += (s, e) => { using var p = new Pen(Palette.Border, 1); e.Graphics.DrawRectangle(p, 0, 0, c.Width - 1, c.Height - 1); };
            var bar = new Panel { Left = 0, Width = 4, Height = 92, BackColor = accent };
            c.Controls.Add(bar);
            c.Controls.Add(new Label { Text = title, Location = new Point(16, 14), AutoSize = true, ForeColor = Palette.TextSecondary, Font = new Font("Segoe UI", 8.5F) });
            c.Controls.Add(new Label { Text = value, Location = new Point(16, 44), AutoSize = true, ForeColor = Palette.TextPrimary, Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold) });
            return c;
        }

        public static DataGridView Grid()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false,
                BackgroundColor = Palette.BgSurface, GridColor = Palette.Border, BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false
            };
            g.DefaultCellStyle.BackColor = Palette.BgSurface;
            g.DefaultCellStyle.ForeColor = Palette.TextPrimary;
            g.DefaultCellStyle.SelectionBackColor = Palette.Primary;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.BackColor = Palette.BgElevated;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Palette.TextSecondary;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 47, 65);
            EnableDb(g);
            return g;
        }

        public static void StyleGrid(DataGridView g)
        {
            g.BackgroundColor = Palette.BgSurface; g.GridColor = Palette.Border;
            g.DefaultCellStyle.BackColor = Palette.BgSurface; g.DefaultCellStyle.ForeColor = Palette.TextPrimary;
            g.ColumnHeadersDefaultCellStyle.BackColor = Palette.BgElevated;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 47, 65);
            EnableDb(g);
        }

        public static RichTextBox LogBox()
        {
            return new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Palette.BgInput, ForeColor = Color.FromArgb(148, 238, 164), Font = new Font("Consolas", 9F), BorderStyle = BorderStyle.FixedSingle };
        }

        public static void StyleRichText(RichTextBox r) { r.BackColor = Palette.BgInput; r.ForeColor = Color.FromArgb(148, 238, 164); r.Font = new Font("Consolas", 9F); }
        public static TextBox Input(string v = "", int w = 200) => new TextBox { Text = v, Width = w, BackColor = Palette.BgInput, ForeColor = Palette.TextPrimary, BorderStyle = BorderStyle.FixedSingle };
        public static void StyleInput(TextBox t) { t.BackColor = Palette.BgInput; t.ForeColor = Palette.TextPrimary; }
        public static void StyleCombo(ComboBox c) { c.BackColor = Palette.BgInput; c.ForeColor = Palette.TextPrimary; }

        public static ComboBox Combo(string[] items, string sel, int w = 200)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = w, BackColor = Palette.BgInput, ForeColor = Palette.TextPrimary, FlatStyle = FlatStyle.Flat };
            c.Items.AddRange(items); c.SelectedItem = sel;
            if (c.SelectedIndex < 0 && c.Items.Count > 0) c.SelectedIndex = 0;
            return c;
        }

        public static FlowLayoutPanel Flow(int h = 54)
        {
            return new FlowLayoutPanel { Height = h, Padding = new Padding(12, 8, 12, 8), FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = Palette.BgSurface };
        }

        public static Panel GradientHeader(string title, Color from, Color to, int h = 64)
        {
            GradientPanel gp; var p = gp = new GradientPanel(from, to) { Dock = DockStyle.Top, Height = h };
            var l = new Label { Text = title, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.White, Font = new Font("Segoe UI", 15F, FontStyle.Bold) };
            p.Controls.Add(l);
            return p;
        }

        public static Label Label(string t, FontStyle s = FontStyle.Regular, float sz = 9.5F) => new Label { Text = t, AutoSize = true, ForeColor = Palette.TextSecondary, Font = new Font("Segoe UI", sz, s) };

        private static Color Lighten(Color c, float a) => Color.FromArgb(c.A, Math.Min(255, (int)(c.R + 255 * a)), Math.Min(255, (int)(c.G + 255 * a)), Math.Min(255, (int)(c.B + 255 * a)));
        private static void EnableDb(Control c) { var p = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic); p?.SetValue(c, true, null); }
    }

    public class GradientPanel : Panel
    {
        private readonly Color _f, _t;
        public GradientPanel(Color f, Color t) { _f = f; _t = t; DoubleBuffered = true; }
        protected override void OnPaint(PaintEventArgs e)
        {
            using var b = new LinearGradientBrush(ClientRectangle, _f, _t, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(b, ClientRectangle);
            base.OnPaint(e);
        }
    }
}