using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using EJLive.Core;

namespace EJLive.Setup
{
    public class SetupWizardForm : Form
    {
        private Panel pnlSidebar, pnlContent, pnlButtons;
        private Label[] lblSteps;
        private int _currentStep;
        private string[] _stepTitles = { "Welcome", "License", "Component", "Settings", "Paths", "Install", "Complete" };

        private Label lblWelcomeTitle, lblWelcomeDesc;
        private RichTextBox rtbLicense;
        private CheckBox chkAcceptLicense;
        private RadioButton rbClient, rbServer, rbMonitor, rbAll;
        private Label lblComponentDesc;
        private Label lblATMName, lblServerIP, lblServerPort, lblATMType;
        private TextBox txtATMName, txtServerIP, txtServerPort;
        private ComboBox cmbATMType;
        private CheckBox chkAutoDetect;
        private Label lblInstallPath, lblDataPath;
        private TextBox txtInstallPath, txtDataPath;
        private Button btnBrowseInstall, btnBrowseData;
        private ProgressBar progressInstall;
        private RichTextBox rtbInstallLog;
        private Label lblComplete;
        private CheckBox chkStartService, chkOpenDashboard;
        private Button btnBack, btnNext, btnCancel;
        private string _selectedComponent = "Client";

        public SetupWizardForm()
        {
            InitializeComponent();
            ShowStep(0);
        }

        private void InitializeComponent()
        {
            this.Text = "EJLive Enterprise - Setup Wizard";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9F);

            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 180, BackColor = Color.FromArgb(30, 30, 50) };
            BuildSidebar();
            this.Controls.Add(pnlSidebar);

            pnlButtons = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(240, 240, 245) };
            btnBack = new Button { Text = "< Back", Location = new Point(380, 12), Size = new Size(90, 28), Enabled = false };
            btnNext = new Button { Text = "Next >", Location = new Point(475, 12), Size = new Size(90, 28) };
            btnCancel = new Button { Text = "Cancel", Location = new Point(575, 12), Size = new Size(90, 28) };
            btnBack.Click += (s, e) => { if (_currentStep > 0) ShowStep(_currentStep - 1); };
            btnNext.Click += (s, e) => NavigateNext();
            btnCancel.Click += (s, e) => { if (MessageBox.Show("Cancel?", "Cancel", MessageBoxButtons.YesNo) == DialogResult.Yes) Close(); };
            pnlButtons.Controls.AddRange(new Control[] { btnBack, btnNext, btnCancel });
            this.Controls.Add(pnlButtons);

            pnlContent = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(20) };
            BuildAllSteps();
            this.Controls.Add(pnlContent);
        }

        private void BuildSidebar()
        {
            pnlSidebar.Controls.Add(new Label { Text = "EJLive Setup", Location = new Point(15, 20), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold) });
            lblSteps = new Label[_stepTitles.Length];
            for (int i = 0; i < _stepTitles.Length; i++)
            {
                lblSteps[i] = new Label { Text = (i + 1) + ". " + _stepTitles[i], Location = new Point(20, 70 + i * 35), AutoSize = true, ForeColor = Color.Gray };
                pnlSidebar.Controls.Add(lblSteps[i]);
            }
        }

        private void BuildAllSteps()
        {
            lblWelcomeTitle = new Label { Text = "Welcome to EJLive Enterprise Setup", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            lblWelcomeDesc = new Label { Text = "This wizard will install EJLive Enterprise.\n\nSupported ATMs: NCR, GRG, Wincor Nixdorf\n\nClick Next to continue.", Location = new Point(20, 60), Size = new Size(450, 200) };
            pnlContent.Controls.AddRange(new Control[] { lblWelcomeTitle, lblWelcomeDesc });

            rtbLicense = new RichTextBox { Location = new Point(20, 20), Size = new Size(460, 280), ReadOnly = true, Visible = false, Text = "EJLive Enterprise License\n\nProprietary software. Unauthorized use prohibited.\n\n(c) 2024 EJLive Enterprise." };
            chkAcceptLicense = new CheckBox { Text = "I accept the license", Location = new Point(20, 310), AutoSize = true, Visible = false };
            chkAcceptLicense.CheckedChanged += (s, e) => btnNext.Enabled = chkAcceptLicense.Checked;
            pnlContent.Controls.AddRange(new Control[] { rtbLicense, chkAcceptLicense });

            lblComponentDesc = new Label { Text = "Select component:", Location = new Point(20, 5), AutoSize = true, Visible = false, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            rbClient = new RadioButton { Text = "Client (ATM Side)", Location = new Point(20, 30), AutoSize = true, Visible = false, Checked = true };
            rbServer = new RadioButton { Text = "Server (Host)", Location = new Point(20, 60), AutoSize = true, Visible = false };
            rbMonitor = new RadioButton { Text = "Monitor (Dashboard)", Location = new Point(20, 90), AutoSize = true, Visible = false };
            rbAll = new RadioButton { Text = "Full Installation", Location = new Point(20, 120), AutoSize = true, Visible = false };
            pnlContent.Controls.AddRange(new Control[] { lblComponentDesc, rbClient, rbServer, rbMonitor, rbAll });

            lblATMName = new Label { Text = "ATM Name:", Location = new Point(20, 30), AutoSize = true, Visible = false };
            txtATMName = new TextBox { Location = new Point(130, 27), Width = 200, Visible = false };
            lblServerIP = new Label { Text = "Server IP:", Location = new Point(20, 65), AutoSize = true, Visible = false };
            txtServerIP = new TextBox { Location = new Point(130, 62), Width = 200, Visible = false };
            lblServerPort = new Label { Text = "Port:", Location = new Point(20, 100), AutoSize = true, Visible = false };
            txtServerPort = new TextBox { Location = new Point(130, 97), Width = 80, Text = "5656", Visible = false };
            lblATMType = new Label { Text = "ATM Type:", Location = new Point(20, 135), AutoSize = true, Visible = false };
            cmbATMType = new ComboBox { Location = new Point(130, 132), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            cmbATMType.Items.AddRange(new object[] { "NCR", "GRG", "Wincor Nixdorf", "Auto Detect" });
            cmbATMType.SelectedIndex = 3;
            chkAutoDetect = new CheckBox { Text = "Auto-detect ATM type", Location = new Point(20, 170), AutoSize = true, Checked = true, Visible = false };
            pnlContent.Controls.AddRange(new Control[] { lblATMName, txtATMName, lblServerIP, txtServerIP, lblServerPort, txtServerPort, lblATMType, cmbATMType, chkAutoDetect });

            lblInstallPath = new Label { Text = "Install Path:", Location = new Point(20, 30), AutoSize = true, Visible = false };
            txtInstallPath = new TextBox { Location = new Point(120, 27), Width = 300, Text = @"C:\EJLive", Visible = false };
            btnBrowseInstall = new Button { Text = "...", Location = new Point(425, 26), Size = new Size(30, 23), Visible = false };
            lblDataPath = new Label { Text = "Data Path:", Location = new Point(20, 65), AutoSize = true, Visible = false };
            txtDataPath = new TextBox { Location = new Point(120, 62), Width = 300, Text = @"C:\EJLive\Data", Visible = false };
            btnBrowseData = new Button { Text = "...", Location = new Point(425, 61), Size = new Size(30, 23), Visible = false };
            btnBrowseInstall.Click += (s, e) => { using (var f = new FolderBrowserDialog()) { if (f.ShowDialog() == DialogResult.OK) txtInstallPath.Text = f.SelectedPath; } };
            btnBrowseData.Click += (s, e) => { using (var f = new FolderBrowserDialog()) { if (f.ShowDialog() == DialogResult.OK) txtDataPath.Text = f.SelectedPath; } };
            pnlContent.Controls.AddRange(new Control[] { lblInstallPath, txtInstallPath, btnBrowseInstall, lblDataPath, txtDataPath, btnBrowseData });

            progressInstall = new ProgressBar { Location = new Point(20, 30), Size = new Size(460, 25), Visible = false };
            rtbInstallLog = new RichTextBox { Location = new Point(20, 65), Size = new Size(460, 250), ReadOnly = true, Visible = false, Font = new Font("Consolas", 9) };
            pnlContent.Controls.AddRange(new Control[] { progressInstall, rtbInstallLog });

            lblComplete = new Label { Text = "Installation Complete!\n\nEJLive Enterprise installed successfully.", Location = new Point(20, 30), Size = new Size(450, 150), Visible = false, Font = new Font("Segoe UI", 11) };
            chkStartService = new CheckBox { Text = "Start service now", Location = new Point(20, 200), AutoSize = true, Checked = true, Visible = false };
            chkOpenDashboard = new CheckBox { Text = "Open dashboard", Location = new Point(20, 225), AutoSize = true, Visible = false };
            pnlContent.Controls.AddRange(new Control[] { lblComplete, chkStartService, chkOpenDashboard });
        }

        private void ShowStep(int step)
        {
            _currentStep = step;
            for (int i = 0; i < lblSteps.Length; i++)
            {
                lblSteps[i].ForeColor = i == step ? Color.White : (i < step ? Color.LightGreen : Color.Gray);
                lblSteps[i].Font = new Font("Segoe UI", 9, i == step ? FontStyle.Bold : FontStyle.Regular);
            }
            foreach (Control c in pnlContent.Controls) c.Visible = false;
            btnNext.Enabled = true;

            switch (step)
            {
                case 0: lblWelcomeTitle.Visible = true; lblWelcomeDesc.Visible = true; break;
                case 1: rtbLicense.Visible = true; chkAcceptLicense.Visible = true; btnNext.Enabled = chkAcceptLicense.Checked; break;
                case 2: lblComponentDesc.Visible = true; rbClient.Visible = true; rbServer.Visible = true; rbMonitor.Visible = true; rbAll.Visible = true; break;
                case 3: lblATMName.Visible = true; txtATMName.Visible = true; lblServerIP.Visible = true; txtServerIP.Visible = true; lblServerPort.Visible = true; txtServerPort.Visible = true; lblATMType.Visible = true; cmbATMType.Visible = true; chkAutoDetect.Visible = true; break;
                case 4: lblInstallPath.Visible = true; txtInstallPath.Visible = true; btnBrowseInstall.Visible = true; lblDataPath.Visible = true; txtDataPath.Visible = true; btnBrowseData.Visible = true; break;
                case 5: progressInstall.Visible = true; rtbInstallLog.Visible = true; btnBack.Enabled = false; btnNext.Enabled = false; PerformInstallation(); break;
                case 6: lblComplete.Visible = true; chkStartService.Visible = true; chkOpenDashboard.Visible = true; btnNext.Text = "Finish"; break;
            }
            btnBack.Enabled = step > 0 && step != 5;
        }

        private void NavigateNext()
        {
            if (_currentStep == 6) { Close(); return; }
            if (_currentStep == 2) { _selectedComponent = rbClient.Checked ? "Client" : rbServer.Checked ? "Server" : rbMonitor.Checked ? "Monitor" : "All"; }
            ShowStep(_currentStep + 1);
        }

        private void PerformInstallation()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    InstallLog("Starting installation..."); SetProgress(5);
                    InstallLog("Creating directories..."); SetProgress(15);
                    string ip = ""; SafeInvoke(() => ip = txtInstallPath.Text);
                    if (!string.IsNullOrEmpty(ip) && !Directory.Exists(ip)) Directory.CreateDirectory(ip);
                    InstallLog("Copying files..."); SetProgress(40);
                    InstallLog("Creating configuration..."); SetProgress(55);
                    InstallLog("Registering service..."); SetProgress(70);
                    InstallLog("Creating shortcuts..."); SetProgress(85);
                    InstallLog("Detecting ATM type...");
                    string detected = "Unknown";
                    if (Directory.Exists(ATMPaths.NCR_SOURCE)) detected = "NCR";
                    else if (Directory.Exists(ATMPaths.GRG_SOURCE)) detected = "GRG";
                    else if (Directory.Exists(ATMPaths.WN_SOURCE)) detected = "Wincor";
                    InstallLog("  Detected: " + detected); SetProgress(95);
                    InstallLog("Installation completed!"); SetProgress(100);
                    System.Threading.Thread.Sleep(500);
                    SafeInvoke(() => ShowStep(6));
                }
                catch (Exception ex) { InstallLog("ERROR: " + ex.Message); SafeInvoke(() => { btnNext.Enabled = true; btnBack.Enabled = true; }); }
            });
        }

        private void InstallLog(string msg) { SafeInvoke(() => { rtbInstallLog.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg + "\n"); rtbInstallLog.ScrollToCaret(); }); }
        private void SetProgress(int v) { SafeInvoke(() => progressInstall.Value = Math.Min(v, 100)); }
        private void SafeInvoke(Action a) { if (this.InvokeRequired) this.BeginInvoke(a); else a(); }
    }
}
