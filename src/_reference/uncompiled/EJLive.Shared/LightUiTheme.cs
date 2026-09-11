using System;
using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Shared
{
    public static class LightUiTheme
    {
        public static readonly Color Window = Color.FromArgb(246, 248, 250);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceAlt = Color.FromArgb(241, 245, 249);
        public static readonly Color Header = Color.FromArgb(232, 238, 245);
        public static readonly Color Border = Color.FromArgb(203, 213, 225);
        public static readonly Color Text = Color.FromArgb(31, 41, 55);
        public static readonly Color Muted = Color.FromArgb(100, 116, 139);
        public static readonly Color Selection = Color.FromArgb(0, 120, 215);
        public static readonly Color Primary = Color.FromArgb(0, 102, 204);
        public static readonly Color Success = Color.FromArgb(25, 135, 84);
        public static readonly Color Warning = Color.FromArgb(180, 100, 0);
        public static readonly Color Danger = Color.FromArgb(180, 35, 24);

        public static readonly Font BaseFont = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        public static readonly Font SmallFont = new Font("Segoe UI", 8.5f, FontStyle.Regular);
        public static readonly Font StrongFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        public static void Apply(Control root)
        {
            if (root == null) return;
            ApplyTo(root);
            foreach (Control child in root.Controls)
                Apply(child);
        }

        public static void ApplyDataGrid(DataGridView dgv)
        {
            if (dgv == null) return;
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = Surface;
            dgv.GridColor = Border;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.ForeColor = Text;
            dgv.Font = SmallFont;
            dgv.ColumnHeadersHeight = Math.Max(30, dgv.ColumnHeadersHeight);
            dgv.RowTemplate.Height = Math.Max(28, dgv.RowTemplate.Height);
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Header,
                ForeColor = Text,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                SelectionBackColor = Header,
                SelectionForeColor = Text,
                Padding = new Padding(4, 3, 4, 3)
            };
            dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Surface,
                ForeColor = Text,
                SelectionBackColor = Selection,
                SelectionForeColor = Color.White,
                Padding = new Padding(4, 2, 4, 2)
            };
            dgv.RowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Surface, ForeColor = Text };
            dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(248, 250, 252), ForeColor = Text };
        }

        public static void StyleButton(Button btn, Color color)
        {
            if (btn == null) return;
            btn.BackColor = color;
            btn.ForeColor = ShouldUseWhiteText(color) ? Color.White : Text;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.UseVisualStyleBackColor = false;
            btn.Font = SmallFont;
            btn.Height = Math.Max(34, btn.Height);
            if (btn.MinimumSize.Width == 0 || btn.MinimumSize.Height < 34)
                btn.MinimumSize = new Size(Math.Max(44, btn.MinimumSize.Width), 34);
            btn.Margin = new Padding(4, 3, 4, 3);
            btn.Cursor = Cursors.Hand;
            btn.AutoEllipsis = true;
            btn.TextAlign = ContentAlignment.MiddleCenter;
        }

        private static void ApplyTo(Control control)
        {
            control.Font = NormalizeFont(control.Font);

            switch (control)
            {
                case Form form:
                    form.BackColor = Window;
                    form.ForeColor = Text;
                    form.Font = BaseFont;
                    break;
                case TabPage tab:
                    tab.BackColor = Window;
                    tab.ForeColor = Text;
                    break;
                case FlowLayoutPanel flow:
                    flow.BackColor = NormalizeSurface(flow.BackColor, Surface);
                    flow.ForeColor = Text;
                    flow.Padding = NormalizePadding(flow.Padding);
                    break;
                case TableLayoutPanel table:
                    table.BackColor = NormalizeSurface(table.BackColor, Surface);
                    table.ForeColor = Text;
                    table.Padding = NormalizePadding(table.Padding);
                    break;
                case SplitterPanel splitter:
                    splitter.BackColor = NormalizeSurface(splitter.BackColor, Window);
                    splitter.ForeColor = Text;
                    break;
                case Panel panel:
                    panel.BackColor = NormalizeSurface(panel.BackColor, Surface);
                    panel.ForeColor = Text;
                    break;
                case GroupBox group:
                    group.BackColor = Window;
                    group.ForeColor = Text;
                    group.Font = StrongFont;
                    group.Padding = NormalizePadding(group.Padding);
                    break;
                case Label label:
                    label.ForeColor = NormalizeText(label.ForeColor);
                    if (IsVeryDark(label.BackColor)) label.BackColor = Color.Transparent;
                    label.AutoEllipsis = true;
                    break;
                case TextBox text:
                    text.BackColor = Surface;
                    text.ForeColor = Text;
                    text.BorderStyle = BorderStyle.FixedSingle;
                    text.Margin = NormalizeMargin(text.Margin);
                    break;
                case ComboBox combo:
                    combo.BackColor = Surface;
                    combo.ForeColor = Text;
                    combo.FlatStyle = FlatStyle.System;
                    combo.Margin = NormalizeMargin(combo.Margin);
                    break;
                case CheckBox check:
                    check.BackColor = Color.Transparent;
                    check.ForeColor = Text;
                    check.Margin = NormalizeMargin(check.Margin);
                    break;
                case Button btn:
                    StyleButton(btn, btn.BackColor == default ? Primary : btn.BackColor);
                    break;
                case RichTextBox rich:
                    rich.BackColor = Surface;
                    rich.ForeColor = Text;
                    rich.BorderStyle = BorderStyle.FixedSingle;
                    rich.Font = new Font("Segoe UI", 9f);
                    break;
                case ListBox list:
                    list.BackColor = Surface;
                    list.ForeColor = Text;
                    list.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case ListView listView:
                    listView.BackColor = Surface;
                    listView.ForeColor = Text;
                    listView.BorderStyle = BorderStyle.FixedSingle;
                    listView.GridLines = true;
                    listView.FullRowSelect = true;
                    break;
                case DataGridView dgv:
                    ApplyDataGrid(dgv);
                    break;
                case MenuStrip menu:
                    menu.BackColor = Surface;
                    menu.ForeColor = Text;
                    break;
                case StatusStrip status:
                    status.BackColor = Surface;
                    status.ForeColor = Text;
                    status.SizingGrip = false;
                    break;
                case ToolStrip strip:
                    strip.BackColor = Surface;
                    strip.ForeColor = Text;
                    break;
                case TabControl tabs:
                    tabs.BackColor = Window;
                    tabs.ForeColor = Text;
                    tabs.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    tabs.DrawMode = TabDrawMode.Normal;
                    break;
            }
        }

        private static Font NormalizeFont(Font font)
        {
            if (font == null) return BaseFont;
            var size = Math.Max(8f, Math.Min(18f, font.Size));
            return new Font("Segoe UI", size, font.Style);
        }

        private static Padding NormalizePadding(Padding padding)
        {
            return padding;
        }

        private static Padding NormalizeMargin(Padding margin)
        {
            if (margin.All == 0) return new Padding(4);
            return margin;
        }

        private static Color NormalizeSurface(Color color, Color fallback)
        {
            if (color == Color.Transparent || color.IsEmpty) return fallback;
            if (IsVeryDark(color)) return fallback;
            return color;
        }

        private static Color NormalizeText(Color color)
        {
            if (color == Color.Transparent || color.IsEmpty) return Text;
            if (IsVeryDark(color)) return Text;
            if (IsLowContrast(color)) return Muted;
            return color;
        }

        private static bool IsVeryDark(Color color) =>
            color.R < 80 && color.G < 80 && color.B < 80;

        private static bool IsLowContrast(Color color)
        {
            if (color.R > 185 && color.G > 185 && color.B > 185) return true;
            if (color.R >= 90 && color.R <= 160 && color.G >= 90 && color.G <= 160 && color.B >= 90 && color.B <= 160) return true;
            return false;
        }

        private static bool ShouldUseWhiteText(Color color)
        {
            var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            return luminance < 0.62;
        }
    }
}
