using System.ComponentModel;
using EJLive.Client.Controls;
using EJLive.Client.Services;
using EJLive.Core.Enums;
using EJLive.Core.Models;
using EJLive.Core.Utils;

namespace EJLive.Client.Forms
{
    public partial class MainForm : Form
    {
        private readonly ATMConfig _config;
        private readonly ClientServiceController _serviceController;
        private Label _statusLabel;
        private Label _connectionLabel;
        private DarkButton _btnConnect;
        private DarkButton _btnScreenshot;
        private DarkButton _btnTimeSync;
        private DarkButton _btnSendJournal;
        private DarkButton _btnSettings;
        private DarkButton _btnViewLog;
        private Panel _mainPanel;
        private ListBox _logListBox;
        private StatusCard _cardStatus;
        private StatusCard _cardJournal;
        private StatusCard _cardNetwork;
        private StatusCard _cardLastSync;
        private System.Windows.Forms.Timer _refreshTimer;
    
        public MainForm(ATMConfig config, ClientServiceController serviceController)
        {
            _config = config;
            _serviceController = serviceController;
            InitializeComponent();
            SetupEventHandlers();
        }
    
        private void InitializeComponent()
        {
            Text = $"EJLive Client v3.4.0 - {_config.DeviceName}";
            Size = new Size(960, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeColors.Background;
            ForeColor = ThemeColors.Foreground;
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(800, 500);
    
            // Header Panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(16, 8, 16, 8)
            };
    
            var titleLabel = new Label
            {
                Text = $"EJLive Client - {_config.DeviceName}",
                ForeColor = ThemeColors.TextPrimary,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 12)
            };
            headerPanel.Controls.Add(titleLabel);
    
            _connectionLabel = new Label
            {
                Text = "Disconnected",
                ForeColor = ThemeColors.StatusCritical,
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(400, 18)
            };
            headerPanel.Controls.Add(_connectionLabel);
    
            // Status bar at bottom
            var statusBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = ThemeColors.Surface
            };
            _statusLabel = new Label
            {
                Text = "Ready",
                ForeColor = ThemeColors.TextSecondary,
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 4, 0, 0)
            };
            statusBar.Controls.Add(_statusLabel);
    
            // KPI Cards Panel
            var cardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = ThemeColors.Background,
                Padding = new Padding(12),
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true
            };
    
            _cardStatus = new StatusCard
            {
                Title = "Device Status",
                Value = _config.ATMType.ToString(),
                Subtitle = _config.BranchName,
                IndicatorColor = ThemeColors.StatusWarning,
                Size = new Size(220, 100)
            };
            cardsPanel.Controls.Add(_cardStatus);
    
            _cardNetwork = new StatusCard
            {
                Title = "Network",
                Value = "Offline",
                Subtitle = $"{_config.ServerIP}:{_config.ServerPort}",
                IndicatorColor = ThemeColors.StatusCritical,
                Size = new Size(220, 100)
            };
            cardsPanel.Controls.Add(_cardNetwork);
    
            _cardJournal = new StatusCard
            {
                Title = "Journal Status",
                Value = "Idle",
                Subtitle = _config.JournalSourcePath,
                IndicatorColor = ThemeColors.TextMuted,
                Size = new Size(220, 100)
            };
            cardsPanel.Controls.Add(_cardJournal);
    
            _cardLastSync = new StatusCard
            {
                Title = "Last Sync",
                Value = "Never",
                Subtitle = "Waiting...",
                IndicatorColor = ThemeColors.TextMuted,
                Size = new Size(220, 100)
            };
            cardsPanel.Controls.Add(_cardLastSync);
    
            // Buttons Panel
            var buttonsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = ThemeColors.Background,
                Padding = new Padding(12, 8, 12, 8),
                FlowDirection = FlowDirection.LeftToRight
            };
    
            _btnConnect = new DarkButton
            {
                Text = "Connect",
                ButtonColor = ThemeColors.AccentGreen,
                Size = new Size(100, 36)
            };
            _btnConnect.Click += BtnConnect_Click;
            buttonsPanel.Controls.Add(_btnConnect);
    
            _btnScreenshot = new DarkButton
            {
                Text = "Screenshot",
                ButtonColor = ThemeColors.AccentBlue,
                Size = new Size(100, 36),
                Enabled = false
            };
            _btnScreenshot.Click += BtnScreenshot_Click;
            buttonsPanel.Controls.Add(_btnScreenshot);
    
            _btnTimeSync = new DarkButton
            {
                Text = "Time Sync",
                ButtonColor = ThemeColors.AccentBlue,
                Size = new Size(100, 36),
                Enabled = false
            };
            _btnTimeSync.Click += BtnTimeSync_Click;
            buttonsPanel.Controls.Add(_btnTimeSync);
    
            _btnSendJournal = new DarkButton
            {
                Text = "Send Journal",
                ButtonColor = ThemeColors.AccentOrange,
                Size = new Size(110, 36),
                Enabled = false
            };
            _btnSendJournal.Click += BtnSendJournal_Click;
            buttonsPanel.Controls.Add(_btnSendJournal);
    
            _btnViewLog = new DarkButton
            {
                Text = "View Logs",
                ButtonColor = ThemeColors.AccentPurple,
                Size = new Size(100, 36)
            };
            _btnViewLog.Click += BtnViewLog_Click;
            buttonsPanel.Controls.Add(_btnViewLog);
    
            _btnSettings = new DarkButton
            {
                Text = "Settings",
                ButtonColor = ThemeColors.BorderDark,
                Size = new Size(100, 36)
            };
            _btnSettings.Click += BtnSettings_Click;
            buttonsPanel.Controls.Add(_btnSettings);
    
            // Log Panel
            var logPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Surface,
                Padding = new Padding(12),
                Margin = new Padding(12)
            };
    
            var logHeader = new Label
            {
                Text = "Live Log",
                ForeColor = ThemeColors.TextPrimary,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 24
            };
            logPanel.Controls.Add(logHeader);
    
            _logListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Card,
                ForeColor = ThemeColors.TextPrimary,
                Font = new Font("Consolas", 9F),
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                HorizontalScrollbar = true
            };
            logPanel.Controls.Add(_logListBox);
    
            // Main layout
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ThemeColors.Background,
                Padding = new Padding(0)
            };
    
            Controls.Add(_mainPanel);
            _mainPanel.Controls.Add(logPanel);
            _mainPanel.Controls.Add(buttonsPanel);
            _mainPanel.Controls.Add(cardsPanel);
            Controls.Add(statusBar);
            Controls.Add(headerPanel);
    
            // Refresh timer
            _refreshTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }
    
        private void SetupEventHandlers()
        {
            _serviceController.ConnectionStateChanged += (s, connected) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateConnectionState(connected)));
                    return;
                }
                UpdateConnectionState(connected);
            };
    
            _serviceController.StatusMessage += (s, msg) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => AddLogEntry($"[INFO] {msg}")));
                    return;
                }
                AddLogEntry($"[INFO] {msg}");
            };
    
            _serviceController.ErrorOccurred += (s, msg) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => AddLogEntry($"[ERROR] {msg}")));
                    return;
                }
                AddLogEntry($"[ERROR] {msg}");
            };
    
            _serviceController.JournalEntryReceived += (s, entry) =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        _cardJournal.Value = $"Line {entry.LineNumber}";
                        _cardJournal.Subtitle = entry.EventType;
                        _cardJournal.IndicatorColor = entry.IsError ? ThemeColors.StatusCritical : ThemeColors.AccentGreen;
                    }));
                    return;
                }
            };
        }
    
        private void UpdateConnectionState(bool connected)
        {
            _connectionLabel.Text = connected ? "Connected" : "Disconnected";
            _connectionLabel.ForeColor = connected ? ThemeColors.StatusOnline : ThemeColors.StatusCritical;
            _btnConnect.Text = connected ? "Disconnect" : "Connect";
            _btnConnect.ButtonColor = connected ? ThemeColors.StatusCritical : ThemeColors.AccentGreen;
            _cardNetwork.Value = connected ? "Online" : "Offline";
            _cardNetwork.IndicatorColor = connected ? ThemeColors.StatusOnline : ThemeColors.StatusCritical;
            _btnScreenshot.Enabled = connected;
            _btnTimeSync.Enabled = connected;
            _btnSendJournal.Enabled = connected;
        }
    
        private void AddLogEntry(string entry)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logListBox.Items.Insert(0, $"[{timestamp}] {entry}");
            if (_logListBox.Items.Count > 500) _logListBox.Items.RemoveAt(500);
            _statusLabel.Text = entry.Length > 80 ? entry[..80] + "..." : entry;
        }
    
        private async void BtnConnect_Click(object? sender, EventArgs e)
        {
            if (_serviceController.IsConnected)
            {
                _serviceController.Stop();
                AddLogEntry("Disconnected from server");
            }
            else
            {
                AddLogEntry("Connecting to server...");
                _serviceController.Start();
            }
        }
    
        private async void BtnScreenshot_Click(object? sender, EventArgs e)
        {
            AddLogEntry("Requesting screenshot...");
            await _serviceController.RequestScreenshotAsync();
        }
    
        private async void BtnTimeSync_Click(object? sender, EventArgs e)
        {
            AddLogEntry("Requesting time sync...");
            await _serviceController.RequestTimeSyncAsync();
        }
    
        private void BtnSendJournal_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Journal Files|*.log;*.lob;*.trace|All Files|*.*",
                InitialDirectory = _config.JournalSourcePath
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                AddLogEntry($"Sending journal: {Path.GetFileName(dlg.FileName)}");
                _ = _serviceController.SendJournalFileAsync(dlg.FileName);
            }
        }
    
        private void BtnViewLog_Click(object? sender, EventArgs e)
        {
            var logPath = @"C:\EJLive_Storage\Logs";
            if (Directory.Exists(logPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", logPath);
            }
        }
    
        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using var settingsForm = new SettingsForm(_config);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                Program.SaveConfiguration(_config);
                AddLogEntry("Settings saved");
            }
        }
    
        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            if (_serviceController.IsConnected)
            {
                var lastHeartbeat = _serviceController.LastHeartbeat;
                if (lastHeartbeat > DateTime.MinValue)
                {
                    var elapsed = DateTime.UtcNow - lastHeartbeat;
                    _cardLastSync.Value = $"{elapsed.TotalSeconds:0}s ago";
                    _cardLastSync.IndicatorColor = elapsed.TotalMinutes > 2 ? ThemeColors.StatusWarning : ThemeColors.StatusOnline;
                }
            }
        }
    
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _refreshTimer?.Stop();
            _serviceController?.Dispose();
            base.OnFormClosing(e);
        }
    }
    public partial class MainForm : Form
        {
            private readonly ATMConfig _config;
    
    
            private readonly ClientServiceController _serviceController;
    
    
            private Label _statusLabel;
    
    
            private Label _connectionLabel;
    
    
            private DarkButton _btnConnect;
    
    
            private DarkButton _btnScreenshot;
    
    
            private DarkButton _btnTimeSync;
    
    
            private DarkButton _btnSendJournal;
    
    
            private DarkButton _btnSettings;
    
    
            private DarkButton _btnViewLog;
    
    
            private Panel _mainPanel;
    
    
            private ListBox _logListBox;
    
    
            private StatusCard _cardStatus;
    
    
            private StatusCard _cardJournal;
    
    
            private StatusCard _cardNetwork;
    
    
            private StatusCard _cardLastSync;
    
    
            private System.Windows.Forms.Timer _refreshTimer;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_38\src\EJLive.Client.WinForms\Forms\MainForm.cs
            _serviceController = serviceController;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_39\MainForm.cs
            _serviceController = serviceController;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\EJLive_40\src\EJLive.Client.WinForms\Forms\MainForm.cs
            _serviceController = serviceController;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Client.WinForms\Forms\MainForm.cs.before_unify
            _serviceController = serviceController;
    
    
            // Variant: d:\EJLIVE\EJlive_Reference_Projects\Ejliv-ATM-32\src\EJLive.Client.WinForms\Forms\MainForm.cs.v22_bak
            _serviceController = serviceController;
    
    
            public MainForm(ATMConfig config, ClientServiceController serviceController)
            _config = config;
    
    
            InitializeComponent();
    
    
            SetupEventHandlers();
    
    
        }
    public partial class MainForm : Form
        {
            private readonly ATMConfig _config;
    
    
            private readonly ClientServiceController _serviceController;
    
    
            private Label _statusLabel;
    
    
            private Label _connectionLabel;
    
    
            private DarkButton _btnConnect;
    
    
            private DarkButton _btnScreenshot;
    
    
            private DarkButton _btnTimeSync;
    
    
            private DarkButton _btnSendJournal;
    
    
            private DarkButton _btnSettings;
    
    
            private DarkButton _btnViewLog;
    
    
            private Panel _mainPanel;
    
    
            private ListBox _logListBox;
    
    
            private StatusCard _cardStatus;
    
    
            private StatusCard _cardJournal;
    
    
            private StatusCard _cardNetwork;
    
    
            private StatusCard _cardLastSync;
    
    
            private System.Windows.Forms.Timer _refreshTimer;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\EJLive_Complete_Unified\src\EJLive.Client.WinForms\Forms\MainForm.cs
            _serviceController = serviceController;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Client.WinForms\Forms\MainForm.cs.v22_bak
            _serviceController = serviceController;
    
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Client.WinForms\Forms\MainForm.cs.before_unify
            _serviceController = serviceController;
    
    
            public MainForm(ATMConfig config, ClientServiceController serviceController)
            _config = config;
    
    
            InitializeComponent();
    
    
            SetupEventHandlers();
    
    
        }
    // Class: MainForm (from 2 sources)
        public partial class MainForm : Form
        {
            // --- Constants & Fields ---
            private readonly ATMConfig _config;
    
            private readonly ClientServiceController _serviceController;
    
            private Label _statusLabel;
    
            private Label _connectionLabel;
    
            private DarkButton _btnConnect;
    
            private DarkButton _btnScreenshot;
    
            private DarkButton _btnTimeSync;
    
            private DarkButton _btnSendJournal;
    
            private DarkButton _btnSettings;
    
            private DarkButton _btnViewLog;
    
            private Panel _mainPanel;
    
            private ListBox _logListBox;
    
            private StatusCard _cardStatus;
    
            private StatusCard _cardJournal;
    
            private StatusCard _cardNetwork;
    
            private StatusCard _cardLastSync;
    
            private System.Windows.Forms.Timer _refreshTimer;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Client.WinForms\Forms\MainForm.cs.v22_bak
            _serviceController = serviceController;
    
            // Variant from: d:\EJLIVE\EJlive_Reference_Projects\CodexMarege\src\EJLive.Client.WinForms\Forms\MainForm.cs.before_unify
            _serviceController = serviceController;
    
    
            // --- Constructors ---
            public MainForm(ATMConfig config, ClientServiceController serviceController)
            _config = config;
    
    
            // --- Methods ---
            InitializeComponent();
    
            SetupEventHandlers();
    
    
        }
    public partial public class MainForm : Form
        {
            private readonly ATMConfig _config;
            private readonly ClientServiceController _serviceController;
            private Label _statusLabel;
            private Label _connectionLabel;
            private DarkButton _btnConnect;
            private DarkButton _btnScreenshot;
            private DarkButton _btnTimeSync;
            private DarkButton _btnSendJournal;
            private DarkButton _btnSettings;
            private DarkButton _btnViewLog;
            private Panel _mainPanel;
            private ListBox _logListBox;
            private StatusCard _cardStatus;
            private StatusCard _cardJournal;
            private StatusCard _cardNetwork;
            private StatusCard _cardLastSync;
            public MainForm(ATMConfig config, ClientServiceController serviceController)
            {
            private void InitializeComponent()
            {
            private void SetupEventHandlers()
            {
            private void UpdateConnectionState(bool connected)
            {
            private void AddLogEntry(string entry)
            {
            private async void BtnConnect_Click(object? sender, EventArgs e)
            {
            private async void BtnScreenshot_Click(object? sender, EventArgs e)
            {
            private async void BtnTimeSync_Click(object? sender, EventArgs e)
            {
            private void BtnSendJournal_Click(object? sender, EventArgs e)
            {
            private void BtnViewLog_Click(object? sender, EventArgs e)
            {
            private void BtnSettings_Click(object? sender, EventArgs e)
            {
            private void RefreshTimer_Tick(object? sender, EventArgs e)
            {
            protected override void OnFormClosing(FormClosingEventArgs e)
            {
        }
    
    }
    public partial class MainForm : Form
        {
            private readonly ATMConfig _config;
    
    
            private readonly ClientServiceController _serviceController;
    
    
            private Label _statusLabel;
    
    
            private Label _connectionLabel;
    
    
            private DarkButton _btnConnect;
    
    
            private DarkButton _btnScreenshot;
    
    
            private DarkButton _btnTimeSync;
    
    
            private DarkButton _btnSendJournal;
    
    
            private DarkButton _btnSettings;
    
    
            private DarkButton _btnViewLog;
    
    
            private Panel _mainPanel;
    
    
            private ListBox _logListBox;
    
    
            private StatusCard _cardStatus;
    
    
            private StatusCard _cardJournal;
    
    
            private StatusCard _cardNetwork;
    
    
            private StatusCard _cardLastSync;
    
    
            private System.Windows.Forms.Timer _refreshTimer;
    
    
            _serviceController = serviceController;
    
    
            public MainForm(ATMConfig config, ClientServiceController serviceController)
            _config = config;
    
    
            InitializeComponent();
    
    
            SetupEventHandlers();
    
    
        }
}

using var settingsForm = new SettingsForm(_config);
