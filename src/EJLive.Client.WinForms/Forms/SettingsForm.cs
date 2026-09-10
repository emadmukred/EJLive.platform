using EJLive.Client.Controls;
using EJLive.Core.Enums;
using EJLive.Core.Models;

namespace EJLive.Client.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly ATMConfig _config;
        private ComboBox _cmbATMType;
        private TextBox _txtDeviceName;
        private TextBox _txtBranch;
        private TextBox _txtServerIP;
        private NumericUpDown _numPort;
        private TextBox _txtJournalPath;
        private TextBox _txtBackupPath;
        private NumericUpDown _numHeartbeat;
        private CheckBox _chkCompression;
        private CheckBox _chkEncryption;
        private CheckBox _chkAutoSync;
    
        public SettingsForm(ATMConfig config)
        {
            _config = config;
            InitializeComponent();
            LoadSettings();
        }
    
        private void InitializeComponent()
        {
            Text = "EJLive Client Settings";
            Size = new Size(560, 520);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ThemeColors.Background;
            ForeColor = ThemeColors.Foreground;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
    
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                Padding = new Padding(16),
                BackColor = ThemeColors.Background
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
    
            int row = 0;
    
            // ATM Type
            panel.Controls.Add(CreateLabel("ATM Type:"), 0, row);
            _cmbATMType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = ThemeColors.Surface,
                ForeColor = ThemeColors.Foreground,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill
            };
            _cmbATMType.Items.AddRange(Enum.GetNames(typeof(ATMType)));
            panel.Controls.Add(_cmbATMType, 1, row++);
    
            // Device Name
            panel.Controls.Add(CreateLabel("Device Name:"), 0, row);
            _txtDeviceName = CreateTextBox();
            panel.Controls.Add(_txtDeviceName, 1, row++);
    
            // Branch
            panel.Controls.Add(CreateLabel("Branch:"), 0, row);
            _txtBranch = CreateTextBox();
            panel.Controls.Add(_txtBranch, 1, row++);
    
            // Server IP
            panel.Controls.Add(CreateLabel("Server IP:"), 0, row);
            _txtServerIP = CreateTextBox();
            panel.Controls.Add(_txtServerIP, 1, row++);
    
            // Server Port
            panel.Controls.Add(CreateLabel("Server Port:"), 0, row);
            _numPort = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 65535,
                Value = 8888,
                BackColor = ThemeColors.Surface,
                ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill
            };
            panel.Controls.Add(_numPort, 1, row++);
    
            // Journal Path
            panel.Controls.Add(CreateLabel("Journal Path:"), 0, row);
            _txtJournalPath = CreateTextBox();
            panel.Controls.Add(_txtJournalPath, 1, row++);
    
            // Backup Path
            panel.Controls.Add(CreateLabel("Backup Path:"), 0, row);
            _txtBackupPath = CreateTextBox();
            panel.Controls.Add(_txtBackupPath, 1, row++);
    
            // Heartbeat Interval
            panel.Controls.Add(CreateLabel("Heartbeat (sec):"), 0, row);
            _numHeartbeat = new NumericUpDown
            {
                Minimum = 5,
                Maximum = 300,
                Value = 30,
                BackColor = ThemeColors.Surface,
                ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill
            };
            panel.Controls.Add(_numHeartbeat, 1, row++);
    
            // Compression
            panel.Controls.Add(CreateLabel("Compression:"), 0, row);
            _chkCompression = new CheckBox
            {
                ForeColor = ThemeColors.Foreground,
                Text = "Enable",
                Checked = true
            };
            panel.Controls.Add(_chkCompression, 1, row++);
    
            // Encryption
            panel.Controls.Add(CreateLabel("Encryption:"), 0, row);
            _chkEncryption = new CheckBox
            {
                ForeColor = ThemeColors.Foreground,
                Text = "Enable",
                Checked = true
            };
            panel.Controls.Add(_chkEncryption, 1, row++);
    
            // Auto Sync
            panel.Controls.Add(CreateLabel("Auto Sync:"), 0, row);
            _chkAutoSync = new CheckBox
            {
                ForeColor = ThemeColors.Foreground,
                Text = "Enable",
                Checked = true
            };
            panel.Controls.Add(_chkAutoSync, 1, row++);
    
            // Buttons
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = ThemeColors.Background,
                Padding = new Padding(8)
            };
    
            var btnSave = new DarkButton
            {
                Text = "Save",
                ButtonColor = ThemeColors.AccentGreen,
                Size = new Size(90, 32),
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(btnSave);
    
            var btnCancel = new DarkButton
            {
                Text = "Cancel",
                ButtonColor = ThemeColors.BorderDark,
                Size = new Size(90, 32),
                DialogResult = DialogResult.Cancel
            };
            buttonPanel.Controls.Add(btnCancel);
    
            Controls.Add(panel);
            Controls.Add(buttonPanel);
        }
    
        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = ThemeColors.TextSecondary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }
    
        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                BackColor = ThemeColors.Surface,
                ForeColor = ThemeColors.Foreground,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill
            };
        }
    
        private void LoadSettings()
        {
            _cmbATMType.SelectedItem = _config.ATMType.ToString();
            _txtDeviceName.Text = _config.DeviceName;
            _txtBranch.Text = _config.BranchName;
            _txtServerIP.Text = _config.ServerIP;
            _numPort.Value = _config.ServerPort;
            _txtJournalPath.Text = _config.JournalSourcePath;
            _txtBackupPath.Text = _config.LocalBackupPath;
            _numHeartbeat.Value = _config.HeartbeatIntervalSeconds;
            _chkCompression.Checked = _config.UseCompression;
            _chkEncryption.Checked = _config.UseEncryption;
            _chkAutoSync.Checked = _config.AutoSync;
        }
    
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (Enum.TryParse<ATMType>(_cmbATMType.SelectedItem?.ToString(), out var atmType))
                _config.ATMType = atmType;
    
            _config.DeviceName = _txtDeviceName.Text;
            _config.BranchName = _txtBranch.Text;
            _config.ServerIP = _txtServerIP.Text;
            _config.ServerPort = (int)_numPort.Value;
            _config.JournalSourcePath = _txtJournalPath.Text;
            _config.LocalBackupPath = _txtBackupPath.Text;
            _config.HeartbeatIntervalSeconds = (int)_numHeartbeat.Value;
            _config.UseCompression = _chkCompression.Checked;
            _config.UseEncryption = _chkEncryption.Checked;
            _config.AutoSync = _chkAutoSync.Checked;
    
            // Apply type-specific defaults if paths are empty
            if (string.IsNullOrWhiteSpace(_config.JournalSourcePath))
            {
                var defaults = ATMConfig.GetDefaultConfig(_config.ATMType);
                _config.JournalSourcePath = defaults.JournalSourcePath;
                _config.LocalBackupPath = defaults.LocalBackupPath;
                _config.ImageTargetPath = defaults.ImageTargetPath;
            }
        }
    }
    // Class: SettingsForm (from 2 sources)
        public partial class SettingsForm : Form
        {
            // --- Constants & Fields ---
            private readonly ATMConfig _config;
    
            private ComboBox _cmbATMType;
    
            private TextBox _txtDeviceName;
    
            private TextBox _txtBranch;
    
            private TextBox _txtServerIP;
    
            private NumericUpDown _numPort;
    
            private TextBox _txtJournalPath;
    
            private TextBox _txtBackupPath;
    
            private NumericUpDown _numHeartbeat;
    
            private CheckBox _chkCompression;
    
            private CheckBox _chkEncryption;
    
            private CheckBox _chkAutoSync;
    
    
            // --- Constructors ---
            public SettingsForm(ATMConfig config)
            _config = config;
    
    
            // --- Methods ---
            InitializeComponent();
    
            LoadSettings();
    
    
        }
    public partial class SettingsForm : Form
        {
            private readonly ATMConfig _config;
    
    
            private ComboBox _cmbATMType;
    
    
            private TextBox _txtDeviceName;
    
    
            private TextBox _txtBranch;
    
    
            private TextBox _txtServerIP;
    
    
            private NumericUpDown _numPort;
    
    
            private TextBox _txtJournalPath;
    
    
            private TextBox _txtBackupPath;
    
    
            private NumericUpDown _numHeartbeat;
    
    
            private CheckBox _chkCompression;
    
    
            private CheckBox _chkEncryption;
    
    
            private CheckBox _chkAutoSync;
    
    
            public SettingsForm(ATMConfig config)
            _config = config;
    
    
            InitializeComponent();
    
    
            LoadSettings();
    
    
        }
    public partial public class SettingsForm : Form
        {
            private readonly ATMConfig _config;
            private ComboBox _cmbATMType;
            private TextBox _txtDeviceName;
            private TextBox _txtBranch;
            private TextBox _txtServerIP;
            private NumericUpDown _numPort;
            private TextBox _txtJournalPath;
            private TextBox _txtBackupPath;
            private NumericUpDown _numHeartbeat;
            private CheckBox _chkCompression;
            private CheckBox _chkEncryption;
            private CheckBox _chkAutoSync;
            public SettingsForm(ATMConfig config)
            {
            private void InitializeComponent()
            {
            private Label CreateLabel(string text)
            {
            private TextBox CreateTextBox()
            {
            private void LoadSettings()
            {
            private void BtnSave_Click(object? sender, EventArgs e)
            {
        }
    
    }
}
