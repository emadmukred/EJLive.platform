using System;
using System.Windows.Forms;

namespace EJLive.Client.WinForms
{
    public partial public public static class ControlExtensions
        {
            public static void InvokeIfRequired(this Control control, Action action)
            {
            public static void InvokeIfRequired<T>(this Control control, Action<T> action, T arg)
            {
            public static FlowLayoutPanel Flow() =>
            public static Button Button(string text, Action action)
            {
            public static DataGridView Grid() =>
            public static RichTextBox LogBox() =>
            public static TextBox Text(string text) =>
            public static ComboBox Combo(System.Collections.Generic.IEnumerable<string> values, string selected)
            {
        }
    
        public partial public public static class Ui
        {
            public static FlowLayoutPanel Flow() =>
            public static Button Button(string text, Action action)
            {
            public static DataGridView Grid() =>
            public static RichTextBox LogBox() =>
            public static TextBox Text(string text) =>
            public static ComboBox Combo(System.Collections.Generic.IEnumerable<string> values, string selected)
            {
        }
    
    }
    public partial public static class ControlExtensions
        {
            public static void InvokeIfRequired(this Control control, Action action)
            {
            public static void InvokeIfRequired<T>(this Control control, Action<T> action, T arg)
            {
        }
    
        public partial public static class Ui
        {
            public static FlowLayoutPanel Flow() =>
            public static Button Button(string text, Action action)
            {
            public static DataGridView Grid() =>
            public static RichTextBox LogBox() =>
            public static TextBox Text(string text) =>
            public static ComboBox Combo(System.Collections.Generic.IEnumerable<string> values, string selected)
            {
        }
    
    }
    /// <summary>
        /// Safe cross-thread UI extension methods for WinForms controls.
        /// </summary>
        public static class ControlExtensions
        {
            /// <summary>
            /// Invokes an action on the control's UI thread if required.
            /// </summary>
            public static void InvokeIfRequired(this Control control, Action action)
            {
                if (control == null) return;
                if (control.InvokeRequired)
                control.Invoke(action);
                else
                action();
            }
    
        /// <summary>
        /// Invokes an action with a parameter on the control's UI thread if required.
        /// </summary>
        public static void InvokeIfRequired<T>(this Control control, Action<T> action, T arg)
        {
            if (control == null) return;
            if (control.InvokeRequired)
            control.Invoke(action, arg);
            else
            action(arg);
        }
    }
    public partial class ControlExtensions
        {
            public static void InvokeIfRequired(this Control control, Action action)
            {
                if (control == null) return;
                if (control.InvokeRequired)
                    control.Invoke(action);
                else
                    action();
            }
    
    
            public static void InvokeIfRequired<T>(this Control control, Action<T> action, T arg)
            {
                if (control == null) return;
                if (control.InvokeRequired)
                    control.Invoke(action, arg);
                else
                    action(arg);
            }
    
    
        }
    // Class: ControlExtensions (from 1 sources)
        public static partial class ControlExtensions
        {
            // --- Methods ---
                    public static void InvokeIfRequired(this Control control, Action action)
                    {
                        if (control == null) return;
                        if (control.InvokeRequired)
                            control.Invoke(action);
                        else
                            action();
                    }
    
                    public static void InvokeIfRequired<T>(this Control control, Action<T> action, T arg)
                    {
                        if (control == null) return;
                        if (control.InvokeRequired)
                            control.Invoke(action, arg);
                        else
                            action(arg);
                    }
    
    
        }
    /// <summary>
    /// Lightweight UI builder helpers used by ClientMainForm.
    /// </summary>
    public static class Ui
    {
        public static FlowLayoutPanel Flow() => new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new System.Drawing.Size(0, 50),
            Padding = new Padding(8, 6, 8, 6),
            Margin = new Padding(0, 0, 0, 8),
            BackColor = System.Drawing.Color.FromArgb(246, 248, 250),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };
    
    public static Button Button(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new System.Drawing.Size(128, 34),
            Height = 34,
            Margin = new Padding(4),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.FromArgb(240, 244, 250),
            ForeColor = System.Drawing.Color.FromArgb(23, 37, 60)
        };
    button.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(187, 198, 214);
    button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(226, 236, 248);
    button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(213, 228, 245);
    button.Click += (_, _) => action();
    return button;
    }
    
    public static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 0, 0, 8),
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        ReadOnly = true,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        RowHeadersVisible = false,
        BackgroundColor = System.Drawing.Color.White,
        BorderStyle = BorderStyle.None
    };
    
    public static RichTextBox LogBox() => new()
    {
        Dock = DockStyle.Fill,
        Font = new System.Drawing.Font("Consolas", 9F),
        BorderStyle = BorderStyle.FixedSingle,
        ReadOnly = true
    };
    
    public static TextBox Text(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill
    };
    
    public static ComboBox Combo(System.Collections.Generic.IEnumerable<string> values, string selected)
    {
        var combo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
    combo.Items.AddRange(System.Linq.Enumerable.Cast<object>(values).ToArray());
    combo.SelectedItem = selected;
    if (combo.SelectedIndex < 0 && combo.Items.Count > 0)
    combo.SelectedIndex = 0;
    return combo;
    }
    }
    public partial class Ui
        {
            public static FlowLayoutPanel Flow() => new()
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new System.Drawing.Size(0, 50),
                Padding = new Padding(8, 6, 8, 6),
                Margin = new Padding(0, 0, 0, 8),
                BackColor = System.Drawing.Color.FromArgb(246, 248, 250),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
    
    
            public static Button Button(string text, Action action)
            {
                var button = new Button
                {
                    Text = text,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    MinimumSize = new System.Drawing.Size(128, 34),
                    Height = 34,
                    Margin = new Padding(4),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(240, 244, 250),
                    ForeColor = System.Drawing.Color.FromArgb(23, 37, 60)
                };
                button.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(187, 198, 214);
                button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(226, 236, 248);
                button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(213, 228, 245);
                button.Click += (_, _) => action();
                return button;
            }
    
    
            public static DataGridView Grid() => new()
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None
            };
    
    
            public static RichTextBox LogBox() => new()
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 9F),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };
    
    
            public static TextBox Text(string text) => new()
            {
                Text = text,
                Dock = DockStyle.Fill
            };
    
    
            public static ComboBox Combo(System.Collections.Generic.IEnumerable<string> values, string selected)
            {
                var combo = new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                combo.Items.AddRange(System.Linq.Enumerable.Cast<object>(values).ToArray());
                combo.SelectedItem = selected;
                if (combo.SelectedIndex < 0 && combo.Items.Count > 0)
                    combo.SelectedIndex = 0;
                return combo;
            }
    
    
        }
    // Class: Ui (from 1 sources)
        public static partial class Ui
        {
            // --- Methods ---
                    public static FlowLayoutPanel Flow() => new()
                    {
                        Dock = DockStyle.Top,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        MinimumSize = new System.Drawing.Size(0, 50),
                        Padding = new Padding(8, 6, 8, 6),
                        Margin = new Padding(0, 0, 0, 8),
                        BackColor = System.Drawing.Color.FromArgb(246, 248, 250),
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = true
                    };
    
                    public static Button Button(string text, Action action)
                    {
                        var button = new Button
                        {
                            Text = text,
                            AutoSize = true,
                            AutoSizeMode = AutoSizeMode.GrowAndShrink,
                            MinimumSize = new System.Drawing.Size(128, 34),
                            Height = 34,
                            Margin = new Padding(4),
                            FlatStyle = FlatStyle.Flat,
                            BackColor = System.Drawing.Color.FromArgb(240, 244, 250),
                            ForeColor = System.Drawing.Color.FromArgb(23, 37, 60)
                        };
                        button.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(187, 198, 214);
                        button.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(226, 236, 248);
                        button.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(213, 228, 245);
                        button.Click += (_, _) => action();
                        return button;
                    }
    
                    public static DataGridView Grid() => new()
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0, 0, 0, 8),
                        AllowUserToAddRows = false,
                        AllowUserToDeleteRows = false,
                        ReadOnly = true,
                        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                        RowHeadersVisible = false,
                        BackgroundColor = System.Drawing.Color.White,
                        BorderStyle = BorderStyle.None
                    };
    
                    public static RichTextBox LogBox() => new()
                    {
                        Dock = DockStyle.Fill,
                        Font = new System.Drawing.Font("Consolas", 9F),
                        BorderStyle = BorderStyle.FixedSingle,
                        ReadOnly = true
                    };
    
                    public static TextBox Text(string text) => new()
                    {
                        Text = text,
                        Dock = DockStyle.Fill
                    };
    
                    public static ComboBox Combo(System.Collections.Generic.IEnumerable<string> values, string selected)
                    {
                        var combo = new ComboBox
                        {
                            Dock = DockStyle.Fill,
                            DropDownStyle = ComboBoxStyle.DropDownList
                        };
                        combo.Items.AddRange(System.Linq.Enumerable.Cast<object>(values).ToArray());
                        combo.SelectedItem = selected;
                        if (combo.SelectedIndex < 0 && combo.Items.Count > 0)
                            combo.SelectedIndex = 0;
                        return combo;
                    }
    
    
        }
}
