using System.IO;
using EJLive.Client.Controls;

namespace EJLive.Client.Forms
{
    public partial class LogViewerForm : Form
    {
        private ListBox _logList;
        private TextBox _txtFilter;
        private ComboBox _cmbLevel;
        private string _logPath;
    
        public LogViewerForm(string logPath = @"C:\EJLive_Storage\Logs")
        {
            _logPath = logPath;
            InitializeComponent();
            LoadLogs();
        }
    
        private void InitializeComponent()
        {
            Text = "Log Viewer";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ThemeColors.Background;
            ForeColor = ThemeColors.Foreground;
    
            // Filter panel
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(8)
            };
    
            _cmbLevel = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 100,
                BackColor = ThemeColors.Surface,
                ForeColor = ThemeColors.Foreground
            };
            _cmbLevel.Items.AddRange(new[] { "ALL", "DEBUG", "INFO", "WARNING", "ERROR", "FATAL" });
            _cmbLevel.SelectedIndex = 0;
            _cmbLevel.SelectedIndexChanged += (s, e) => LoadLogs();
            filterPanel.Controls.Add(_cmbLevel);
    
            _txtFilter = new TextBox
            {
                Left = 110,
                Width = 300,
                BackColor = ThemeColors.Card,
                ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Filter logs..."
            };
            _txtFilter.TextChanged += (s, e) => LoadLogs();
            filterPanel.Controls.Add(_txtFilter);
    
            // Log list
            _logList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Card,
                ForeColor = ThemeColors.TextPrimary,
                Font = new Font("Consolas", 9F),
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                HorizontalScrollbar = true
            };
    
            Controls.Add(_logList);
            Controls.Add(filterPanel);
        }
    
        private void LoadLogs()
        {
            _logList.Items.Clear();
            if (!Directory.Exists(_logPath)) return;
    
            var files = Directory.GetFiles(_logPath, "*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f));
    
            var levelFilter = _cmbLevel.SelectedIndex == 0 ? "" : _cmbLevel.SelectedItem?.ToString() ?? "";
            var textFilter = _txtFilter.Text?.ToLowerInvariant() ?? "";
    
            foreach (var file in files.Take(5))
            {
                try
                {
                    var lines = File.ReadLines(file).Reverse().Take(500);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(levelFilter) && !line.Contains($"[{levelFilter}]"))
                            continue;
                        if (!string.IsNullOrEmpty(textFilter) && !line.ToLowerInvariant().Contains(textFilter))
                            continue;
    
                        _logList.Items.Add(line);
                    }
                }
                catch { /* ignore file access errors */ }
            }
        }
    }
    public partial class LogViewerForm : Form
        {
            private ListBox _logList;
    
    
            private TextBox _txtFilter;
    
    
            private ComboBox _cmbLevel;
    
    
            private string _logPath;
    
    
            public LogViewerForm(string logPath = @"C:\EJLive_Storage\Logs")
            _logPath = logPath;
    
    
            InitializeComponent();
    
    
            LoadLogs();
    
    
        }
    // Class: LogViewerForm (from 2 sources)
        public partial class LogViewerForm : Form
        {
            // --- Constants & Fields ---
            private ListBox _logList;
    
            private TextBox _txtFilter;
    
            private ComboBox _cmbLevel;
    
            private string _logPath;
    
    
            // --- Constructors ---
            public LogViewerForm(string logPath = @"C:\EJLive_Storage\Logs")
            _logPath = logPath;
    
    
            // --- Methods ---
            InitializeComponent();
    
            LoadLogs();
    
    
        }
    public partial public class LogViewerForm : Form
        {
            private ListBox _logList;
            private TextBox _txtFilter;
            private ComboBox _cmbLevel;
            private string _logPath;
            public LogViewerForm(string logPath = @"C:\EJLive_Storage\Logs")
            {
            private void InitializeComponent()
            {
            private void LoadLogs()
            {
        }
    
    }
}
