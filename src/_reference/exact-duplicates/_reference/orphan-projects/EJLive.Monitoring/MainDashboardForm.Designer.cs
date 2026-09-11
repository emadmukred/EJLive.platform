namespace EJLive.Monitoring.WinForms
{
    partial class MainDashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
            
}namespace EJLive.Monitoring.WinForms
{
    partial class MainDashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.tabATMs = new System.Windows.Forms.TabPage();
            this.tabAlerts = new System.Windows.Forms.TabPage();
            this.tabArchive = new System.Windows.Forms.TabPage();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblLastUpdate = new System.Windows.Forms.Label();
            this.pnlStats = new System.Windows.Forms.Panel();
            this.lblTotalATMs = new System.Windows.Forms.Label();
            this.lblOnlineATMs = new System.Windows.Forms.Label();
            this.lblEJSynced = new System.Windows.Forms.Label();
            this.lblFaults = new System.Windows.Forms.Label();
            this.lblTotalLabel = new System.Windows.Forms.Label();
            this.lblOnlineLabel = new System.Windows.Forms.Label();
            this.lblSyncedLabel = new System.Windows.Forms.Label();
            this.lblFaultsLabel = new System.Windows.Forms.Label();
            this.lvATMs = new System.Windows.Forms.ListView();
            this.colATMId = new System.Windows.Forms.ColumnHeader();
            this.colBranch = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.colLastSync = new System.Windows.Forms.ColumnHeader();
            this.lvAlerts = new System.Windows.Forms.ListView();
            this.colTime = new System.Windows.Forms.ColumnHeader();
            this.colAlertATM = new System.Windows.Forms.ColumnHeader();
            this.colMessage = new System.Windows.Forms.ColumnHeader();
            this.pnlCommands = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnRestartATM = new System.Windows.Forms.Button();
            this.btnScreenshot = new System.Windows.Forms.Button();
            this.btnSyncTime = new System.Windows.Forms.Button();
            this.btnArchive = new System.Windows.Forms.Button();
            this.btnChangePassword = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl.SuspendLayout();
            this.tabDashboard.SuspendLayout();
            this.tabATMs.SuspendLayout();
            this.tabAlerts.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlStats.SuspendLayout();
            this.pnlCommands.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(30, 30, 50);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblLastUpdate);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1100, 50);
            this.pnlHeader.TabIndex = 0;
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(15, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(250, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "EJLive NOC Dashboard";
            //
            // lblLastUpdate
            //
            this.lblLastUpdate.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblLastUpdate.AutoSize = true;
            this.lblLastUpdate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLastUpdate.ForeColor = System.Drawing.Color.LightGray;
            this.lblLastUpdate.Location = new System.Drawing.Point(900, 18);
            this.lblLastUpdate.Name = "lblLastUpdate";
            this.lblLastUpdate.Size = new System.Drawing.Size(150, 15);
            this.lblLastUpdate.TabIndex = 1;
            this.lblLastUpdate.Text = "Last Update: --:--:--";
            //
            // tabControl
            //
            this.tabControl.Controls.Add(this.tabDashboard);
            this.tabControl.Controls.Add(this.tabATMs);
            this.tabControl.Controls.Add(this.tabAlerts);
            this.tabControl.Controls.Add(this.tabArchive);
            this.tabControl.Controls.Add(this.tabSettings);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabControl.Location = new System.Drawing.Point(0, 50);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1100, 580);
            this.tabControl.TabIndex = 1;
            //
            // tabDashboard
            //
            this.tabDashboard.Controls.Add(this.pnlStats);
            this.tabDashboard.Controls.Add(this.pnlCommands);
            this.tabDashboard.Text = "  Dashboard  ";
            this.tabDashboard.Padding = new System.Windows.Forms.Padding(10);
            //
            // pnlStats
            //
            this.pnlStats.Controls.Add(this.lblTotalLabel);
            this.pnlStats.Controls.Add(this.lblTotalATMs);
            this.pnlStats.Controls.Add(this.lblOnlineLabel);
            this.pnlStats.Controls.Add(this.lblOnlineATMs);
            this.pnlStats.Controls.Add(this.lblSyncedLabel);
            this.pnlStats.Controls.Add(this.lblEJSynced);
            this.pnlStats.Controls.Add(this.lblFaultsLabel);
            this.pnlStats.Controls.Add(this.lblFaults);
            this.pnlStats.Location = new System.Drawing.Point(15, 15);
            this.pnlStats.Name = "pnlStats";
            this.pnlStats.Size = new System.Drawing.Size(1050, 100);
            this.pnlStats.TabIndex = 0;
            //
            // lblTotalLabel
            //
            this.lblTotalLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTotalLabel.ForeColor = System.Drawing.Color.Gray;
            this.lblTotalLabel.Location = new System.Drawing.Point(20, 10);
            this.lblTotalLabel.Name = "lblTotalLabel";
            this.lblTotalLabel.Size = new System.Drawing.Size(100, 20);
            this.lblTotalLabel.Text = "Total ATMs";
            //
            // lblTotalATMs
            //
            this.lblTotalATMs.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.lblTotalATMs.ForeColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.lblTotalATMs.Location = new System.Drawing.Point(20, 30);
            this.lblTotalATMs.Name = "lblTotalATMs";
            this.lblTotalATMs.Size = new System.Drawing.Size(100, 50);
            this.lblTotalATMs.Text = "0";
            //
            // lblOnlineLabel
            //
            this.lblOnlineLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblOnlineLabel.ForeColor = System.Drawing.Color.Gray;
            this.lblOnlineLabel.Location = new System.Drawing.Point(200, 10);
            this.lblOnlineLabel.Name = "lblOnlineLabel";
            this.lblOnlineLabel.Size = new System.Drawing.Size(100, 20);
            this.lblOnlineLabel.Text = "Online";
            //
            // lblOnlineATMs
            //
            this.lblOnlineATMs.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.lblOnlineATMs.ForeColor = System.Drawing.Color.Green;
            this.lblOnlineATMs.Location = new System.Drawing.Point(200, 30);
            this.lblOnlineATMs.Name = "lblOnlineATMs";
            this.lblOnlineATMs.Size = new System.Drawing.Size(100, 50);
            this.lblOnlineATMs.Text = "0";
            //
            // lblSyncedLabel
            //
            this.lblSyncedLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSyncedLabel.ForeColor = System.Drawing.Color.Gray;
            this.lblSyncedLabel.Location = new System.Drawing.Point(380, 10);
            this.lblSyncedLabel.Name = "lblSyncedLabel";
            this.lblSyncedLabel.Size = new System.Drawing.Size(100, 20);
            this.lblSyncedLabel.Text = "EJ Synced";
            //
            // lblEJSynced
            //
            this.lblEJSynced.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.lblEJSynced.ForeColor = System.Drawing.Color.FromArgb(0, 150, 136);
            this.lblEJSynced.Location = new System.Drawing.Point(380, 30);
            this.lblEJSynced.Name = "lblEJSynced";
            this.lblEJSynced.Size = new System.Drawing.Size(100, 50);
            this.lblEJSynced.Text = "0";
            //
            // lblFaultsLabel
            //
            this.lblFaultsLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFaultsLabel.ForeColor = System.Drawing.Color.Gray;
            this.lblFaultsLabel.Location = new System.Drawing.Point(560, 10);
            this.lblFaultsLabel.Name = "lblFaultsLabel";
            this.lblFaultsLabel.Size = new System.Drawing.Size(100, 20);
            this.lblFaultsLabel.Text = "Faults";
            //
            // lblFaults
            //
            this.lblFaults.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.lblFaults.ForeColor = System.Drawing.Color.Red;
            this.lblFaults.Location = new System.Drawing.Point(560, 30);
            this.lblFaults.Name = "lblFaults";
            this.lblFaults.Size = new System.Drawing.Size(100, 50);
            this.lblFaults.Text = "0";
            //
            // pnlCommands
            //
            this.btnExportReport = new System.Windows.Forms.Button();
            this.btnSettings = new System.Windows.Forms.Button();
            this.pnlCommands.Controls.Add(this.btnRefresh);
            this.pnlCommands.Controls.Add(this.btnRestartATM);
            this.pnlCommands.Controls.Add(this.btnScreenshot);
            this.pnlCommands.Controls.Add(this.btnSyncTime);
            this.pnlCommands.Controls.Add(this.btnArchive);
            this.pnlCommands.Controls.Add(this.btnChangePassword);
            this.pnlCommands.Controls.Add(this.btnExportReport);
            this.pnlCommands.Controls.Add(this.btnSettings);
            this.pnlCommands.Location = new System.Drawing.Point(15, 130);
            this.pnlCommands.Name = "pnlCommands";
            this.pnlCommands.Size = new System.Drawing.Size(1050, 50);
            this.pnlCommands.TabIndex = 1;
            //
            // btnRefresh
            //
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRefresh.Location = new System.Drawing.Point(5, 8);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 32);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            // btnRestartATM
            //
            this.btnRestartATM.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnRestartATM.Location = new System.Drawing.Point(115, 8);
            this.btnRestartATM.Name = "btnRestartATM";
            this.btnRestartATM.Size = new System.Drawing.Size(120, 32);
            this.btnRestartATM.TabIndex = 1;
            this.btnRestartATM.Text = "Restart ATM";
            this.btnRestartATM.Click += new System.EventHandler(this.btnRestartATM_Click);
            //
            // btnScreenshot
            //
            this.btnScreenshot.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnScreenshot.Location = new System.Drawing.Point(245, 8);
            this.btnScreenshot.Name = "btnScreenshot";
            this.btnScreenshot.Size = new System.Drawing.Size(120, 32);
            this.btnScreenshot.TabIndex = 2;
            this.btnScreenshot.Text = "Screenshot";
            this.btnScreenshot.Click += new System.EventHandler(this.btnScreenshot_Click);
            //
            // btnSyncTime
            //
            this.btnSyncTime.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSyncTime.Location = new System.Drawing.Point(375, 8);
            this.btnSyncTime.Name = "btnSyncTime";
            this.btnSyncTime.Size = new System.Drawing.Size(120, 32);
            this.btnSyncTime.TabIndex = 3;
            this.btnSyncTime.Text = "Sync Time";
            this.btnSyncTime.Click += new System.EventHandler(this.btnSyncTime_Click);
            //
            // btnArchive
            //
            this.btnArchive.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnArchive.Location = new System.Drawing.Point(505, 8);
            this.btnArchive.Name = "btnArchive";
            this.btnArchive.Size = new System.Drawing.Size(120, 32);
            this.btnArchive.TabIndex = 4;
            this.btnArchive.Text = "Archive";
            this.btnArchive.Click += new System.EventHandler(this.btnArchive_Click);
            //
            // btnChangePassword
            //
            this.btnChangePassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnChangePassword.Location = new System.Drawing.Point(635, 8);
            this.btnChangePassword.Name = "btnChangePassword";
            this.btnChangePassword.Size = new System.Drawing.Size(140, 32);
            this.btnChangePassword.TabIndex = 5;
            this.btnChangePassword.Text = "Change Password";
            this.btnChangePassword.Click += new System.EventHandler(this.btnChangePassword_Click);
            //
            // btnExportReport
            //
            this.btnExportReport.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnExportReport.Location = new System.Drawing.Point(785, 8);
            this.btnExportReport.Name = "btnExportReport";
            this.btnExportReport.Size = new System.Drawing.Size(120, 32);
            this.btnExportReport.TabIndex = 6;
            this.btnExportReport.Text = "Export Report";
            this.btnExportReport.Click += new System.EventHandler(this.btnExportReport_Click);
            //
            // btnSettings
            //
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnSettings.Location = new System.Drawing.Point(915, 8);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(100, 32);
            this.btnSettings.TabIndex = 7;
            this.btnSettings.Text = "Settings";
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            //
            // tabATMs
            //
            this.tabATMs.Controls.Add(this.lvATMs);
            this.tabATMs.Text = "  ATM Network  ";
            this.tabATMs.Padding = new System.Windows.Forms.Padding(10);
            //
            // lvATMs
            //
            this.lvATMs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this.colATMId, this.colBranch, this.colStatus, this.colLastSync });
            this.lvATMs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvATMs.FullRowSelect = true;
            this.lvATMs.GridLines = true;
            this.lvATMs.Name = "lvATMs";
            this.lvATMs.View = System.Windows.Forms.View.Details;
            //
            // colATMId
            //
            this.colATMId.Text = "ATM ID";
            this.colATMId.Width = 150;
            //
            // colBranch
            //
            this.colBranch.Text = "Branch / Location";
            this.colBranch.Width = 250;
            //
            // colStatus
            //
            this.colStatus.Text = "Status";
            this.colStatus.Width = 180;
            //
            // colLastSync
            //
            this.colLastSync.Text = "Last EJ Sync";
            this.colLastSync.Width = 150;
            //
            // tabAlerts
            //
            this.tabAlerts.Controls.Add(this.lvAlerts);
            this.tabAlerts.Text = "  Alerts  ";
            this.tabAlerts.Padding = new System.Windows.Forms.Padding(10);
            //
            // lvAlerts
            //
            this.lvAlerts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this.colTime, this.colAlertATM, this.colMessage });
            this.lvAlerts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvAlerts.FullRowSelect = true;
            this.lvAlerts.GridLines = true;
            this.lvAlerts.Name = "lvAlerts";
            this.lvAlerts.View = System.Windows.Forms.View.Details;
            //
            // colTime
            //
            this.colTime.Text = "Time";
            this.colTime.Width = 80;
            //
            // colAlertATM
            //
            this.colAlertATM.Text = "ATM ID";
            this.colAlertATM.Width = 150;
            //
            // colMessage
            //
            this.colMessage.Text = "Alert Message";
            this.colMessage.Width = 600;
            //
            // tabArchive
            //
            this.tabArchive.Text = "  Archive  ";
            //
            // tabSettings
            //
            this.tabSettings.Text = "  Settings  ";
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.tsslStatus });
            this.statusStrip.Location = new System.Drawing.Point(0, 630);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1100, 22);
            this.statusStrip.TabIndex = 2;
            //
            // tsslStatus
            //
            this.tsslStatus.Name = "tsslStatus";
            this.tsslStatus.Text = "Ready - EJLive NOC Dashboard";
            //
            // MainDashboardForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 652);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusStrip);
            this.Name = "MainDashboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EJLive NOC Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tabControl.ResumeLayout(false);
            this.tabDashboard.ResumeLayout(false);
            this.tabATMs.ResumeLayout(false);
            this.tabAlerts.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlStats.ResumeLayout(false);
            this.pnlCommands.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDashboard;
        private System.Windows.Forms.TabPage tabATMs;
        private System.Windows.Forms.TabPage tabAlerts;
        private System.Windows.Forms.TabPage tabArchive;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblLastUpdate;
        private System.Windows.Forms.Panel pnlStats;
        private System.Windows.Forms.Label lblTotalATMs;
        private System.Windows.Forms.Label lblOnlineATMs;
        private System.Windows.Forms.Label lblEJSynced;
        private System.Windows.Forms.Label lblFaults;
        private System.Windows.Forms.Label lblTotalLabel;
        private System.Windows.Forms.Label lblOnlineLabel;
        private System.Windows.Forms.Label lblSyncedLabel;
        private System.Windows.Forms.Label lblFaultsLabel;
        private System.Windows.Forms.ListView lvATMs;
        private System.Windows.Forms.ColumnHeader colATMId;
        private System.Windows.Forms.ColumnHeader colBranch;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colLastSync;
        private System.Windows.Forms.ListView lvAlerts;
        private System.Windows.Forms.ColumnHeader colTime;
        private System.Windows.Forms.ColumnHeader colAlertATM;
        private System.Windows.Forms.ColumnHeader colMessage;
        private System.Windows.Forms.Panel pnlCommands;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnRestartATM;
        private System.Windows.Forms.Button btnScreenshot;
        private System.Windows.Forms.Button btnSyncTime;
        private System.Windows.Forms.Button btnArchive;
        private System.Windows.Forms.Button btnChangePassword;
        private System.Windows.Forms.Button btnExportReport;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblLastUpdate = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.pnlDashboardBody = new System.Windows.Forms.TableLayoutPanel();
            this.grpOverview = new System.Windows.Forms.GroupBox();
            this.tblOverview = new System.Windows.Forms.TableLayoutPanel();
            this.lblTotalCaption = new System.Windows.Forms.Label();
            this.lblOnlineCaption = new System.Windows.Forms.Label();
            this.lblSyncedCaption = new System.Windows.Forms.Label();
            this.lblFaultsCaption = new System.Windows.Forms.Label();
            this.lblWarningsCaption = new System.Windows.Forms.Label();
            this.lblCitCaption = new System.Windows.Forms.Label();
            this.lblSupervisorCaption = new System.Windows.Forms.Label();
            this.lblRemainingCashCaption = new System.Windows.Forms.Label();
            this.lblTotalATMs = new System.Windows.Forms.Label();
            this.lblOnlineATMs = new System.Windows.Forms.Label();
            this.lblEJSynced = new System.Windows.Forms.Label();
            this.lblFaults = new System.Windows.Forms.Label();
            this.lblWarnings = new System.Windows.Forms.Label();
            this.lblCitRequired = new System.Windows.Forms.Label();
            this.lblSupervisorCount = new System.Windows.Forms.Label();
            this.lblRemainingCash = new System.Windows.Forms.Label();
            this.grpCommands = new System.Windows.Forms.GroupBox();
            this.flowCommands = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnExportReport = new System.Windows.Forms.Button();
            this.grpTerminalsPreview = new System.Windows.Forms.GroupBox();
            this.dgvTerminals = new System.Windows.Forms.DataGridView();
            this.colTerminalId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBranch = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRegion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVendor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNetwork = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHealth = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHeartbeat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEjSync = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlerts = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastTransaction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpCash = new System.Windows.Forms.GroupBox();
            this.dgvCash = new System.Windows.Forms.DataGridView();
            this.colCashTerminal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCass1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCass2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCass3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCass4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRemaining = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLoaded = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeposit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDispense = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReject = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRetract = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpAlerts = new System.Windows.Forms.GroupBox();
            this.dgvAlerts = new System.Windows.Forms.DataGridView();
            this.colAlertTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlertTerminal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSeverity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMessage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabDashboard.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.pnlDashboardBody.SuspendLayout();
            this.grpOverview.SuspendLayout();
            this.tblOverview.SuspendLayout();
            this.grpCommands.SuspendLayout();
            this.flowCommands.SuspendLayout();
            this.grpTerminalsPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTerminals)).BeginInit();
            this.grpCash.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCash)).BeginInit();
            this.grpAlerts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlerts)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(24, 32, 48);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblLastUpdate);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1440, 56);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(344, 30);
            this.lblTitle.Text = "EJLive Central Monitoring Dashboard";
            // 
            // lblLastUpdate
            // 
            this.lblLastUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLastUpdate.AutoSize = true;
            this.lblLastUpdate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLastUpdate.ForeColor = System.Drawing.Color.Gainsboro;
            this.lblLastUpdate.Location = new System.Drawing.Point(1260, 22);
            this.lblLastUpdate.Name = "lblLastUpdate";
            this.lblLastUpdate.Size = new System.Drawing.Size(147, 15);
            this.lblLastUpdate.Text = "Last Update: --:--:--";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.tsslStatus });
            this.statusStrip.Location = new System.Drawing.Point(0, 818);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1440, 22);
            // 
            // tsslStatus
            // 
            this.tsslStatus.Name = "tsslStatus";
            this.tsslStatus.Text = "Ready";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabDashboard);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 56);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1440, 762);
            // 
            // tabDashboard
            // 
            this.tabDashboard.Controls.Add(this.pnlDashboardBody);
            this.tabDashboard.Location = new System.Drawing.Point(4, 22);
            this.tabDashboard.Name = "tabDashboard";
            this.tabDashboard.Padding = new System.Windows.Forms.Padding(8);
            this.tabDashboard.Size = new System.Drawing.Size(1432, 736);
            this.tabDashboard.Text = "Dashboard";
            this.tabDashboard.UseVisualStyleBackColor = true;
            // 
            // pnlDashboardBody
            // 
            this.pnlDashboardBody.ColumnCount = 2;
            this.pnlDashboardBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67F));
            this.pnlDashboardBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.pnlDashboardBody.Controls.Add(this.grpOverview, 0, 0);
            this.pnlDashboardBody.Controls.Add(this.grpCommands, 1, 0);
            this.pnlDashboardBody.Controls.Add(this.grpTerminalsPreview, 0, 1);
            this.pnlDashboardBody.Controls.Add(this.grpCash, 1, 1);
            this.pnlDashboardBody.Controls.Add(this.grpAlerts, 0, 2);
            this.pnlDashboardBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboardBody.Location = new System.Drawing.Point(8, 8);
            this.pnlDashboardBody.Name = "pnlDashboardBody";
            this.pnlDashboardBody.RowCount = 3;
            this.pnlDashboardBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.pnlDashboardBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.pnlDashboardBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.pnlDashboardBody.Size = new System.Drawing.Size(1416, 720);
            // 
            // grpOverview
            // 
            this.grpOverview.Controls.Add(this.tblOverview);
            this.grpOverview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpOverview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpOverview.Location = new System.Drawing.Point(3, 3);
            this.grpOverview.Name = "grpOverview";
            this.grpOverview.Size = new System.Drawing.Size(942, 144);
            this.grpOverview.Text = "Operational Overview";
            // 
            // tblOverview
            // 
            this.tblOverview.ColumnCount = 4;
            this.tblOverview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblOverview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblOverview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblOverview.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tblOverview.Controls.Add(this.lblTotalCaption, 0, 0);
            this.tblOverview.Controls.Add(this.lblOnlineCaption, 1, 0);
            this.tblOverview.Controls.Add(this.lblSyncedCaption, 2, 0);
            this.tblOverview.Controls.Add(this.lblFaultsCaption, 3, 0);
            this.tblOverview.Controls.Add(this.lblTotalATMs, 0, 1);
            this.tblOverview.Controls.Add(this.lblOnlineATMs, 1, 1);
            this.tblOverview.Controls.Add(this.lblEJSynced, 2, 1);
            this.tblOverview.Controls.Add(this.lblFaults, 3, 1);
            this.tblOverview.Controls.Add(this.lblWarningsCaption, 0, 2);
            this.tblOverview.Controls.Add(this.lblCitCaption, 1, 2);
            this.tblOverview.Controls.Add(this.lblSupervisorCaption, 2, 2);
            this.tblOverview.Controls.Add(this.lblRemainingCashCaption, 3, 2);
            this.tblOverview.Controls.Add(this.lblWarnings, 0, 3);
            this.tblOverview.Controls.Add(this.lblCitRequired, 1, 3);
            this.tblOverview.Controls.Add(this.lblSupervisorCount, 2, 3);
            this.tblOverview.Controls.Add(this.lblRemainingCash, 3, 3);
            this.tblOverview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblOverview.Location = new System.Drawing.Point(3, 19);
            this.tblOverview.Name = "tblOverview";
            this.tblOverview.RowCount = 4;
            this.tblOverview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tblOverview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tblOverview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tblOverview.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            // 
            // captions and values
            // 
            this.lblTotalCaption.Text = "Total Terminals";
            this.lblOnlineCaption.Text = "Online";
            this.lblSyncedCaption.Text = "EJ Synced";
            this.lblFaultsCaption.Text = "Critical / Offline";
            this.lblWarningsCaption.Text = "Warnings";
            this.lblCitCaption.Text = "Need CIT";
            this.lblSupervisorCaption.Text = "Supervisor Mode";
            this.lblRemainingCashCaption.Text = "Remaining Cash";
            SetCaptionStyle(this.lblTotalCaption);
            SetCaptionStyle(this.lblOnlineCaption);
            SetCaptionStyle(this.lblSyncedCaption);
            SetCaptionStyle(this.lblFaultsCaption);
            SetCaptionStyle(this.lblWarningsCaption);
            SetCaptionStyle(this.lblCitCaption);
            SetCaptionStyle(this.lblSupervisorCaption);
            SetCaptionStyle(this.lblRemainingCashCaption);
            SetValueStyle(this.lblTotalATMs, System.Drawing.Color.FromArgb(0, 122, 204));
            SetValueStyle(this.lblOnlineATMs, System.Drawing.Color.ForestGreen);
            SetValueStyle(this.lblEJSynced, System.Drawing.Color.Teal);
            SetValueStyle(this.lblFaults, System.Drawing.Color.Firebrick);
            SetValueStyle(this.lblWarnings, System.Drawing.Color.DarkOrange);
            SetValueStyle(this.lblCitRequired, System.Drawing.Color.SaddleBrown);
            SetValueStyle(this.lblSupervisorCount, System.Drawing.Color.MediumPurple);
            SetValueStyle(this.lblRemainingCash, System.Drawing.Color.DarkSlateBlue);
            // 
            // grpCommands
            // 
            this.grpCommands.Controls.Add(this.flowCommands);
            this.grpCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCommands.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpCommands.Location = new System.Drawing.Point(951, 3);
            this.grpCommands.Name = "grpCommands";
            this.grpCommands.Size = new System.Drawing.Size(462, 144);
            this.grpCommands.Text = "Dashboard Actions";
            // 
            // flowCommands
            // 
            this.flowCommands.Controls.Add(this.btnRefresh);
            this.flowCommands.Controls.Add(this.btnExportReport);
            this.flowCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowCommands.Location = new System.Drawing.Point(3, 19);
            this.flowCommands.Name = "flowCommands";
            this.flowCommands.Padding = new System.Windows.Forms.Padding(12);
            this.flowCommands.Size = new System.Drawing.Size(456, 122);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Text = "Refresh Dashboard";
            this.btnRefresh.Size = new System.Drawing.Size(180, 36);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnExportReport
            // 
            this.btnExportReport.Text = "Export Monitoring Report";
            this.btnExportReport.Size = new System.Drawing.Size(180, 36);
            this.btnExportReport.Click += new System.EventHandler(this.btnExportReport_Click);
            // 
            // grpTerminalsPreview
            // 
            this.grpTerminalsPreview.Controls.Add(this.dgvTerminals);
            this.grpTerminalsPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTerminalsPreview.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTerminalsPreview.Location = new System.Drawing.Point(3, 153);
            this.grpTerminalsPreview.Name = "grpTerminalsPreview";
            this.grpTerminalsPreview.Size = new System.Drawing.Size(942, 323);
            this.grpTerminalsPreview.Text = "Terminal Live Summary";
            // 
            // dgvTerminals
            // 
            this.dgvTerminals.AllowUserToAddRows = false;
            this.dgvTerminals.AllowUserToDeleteRows = false;
            this.dgvTerminals.BackgroundColor = System.Drawing.Color.White;
            this.dgvTerminals.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTerminals.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colTerminalId, this.colBranch, this.colRegion, this.colVendor, this.colNetwork,
                this.colHealth, this.colHeartbeat, this.colEjSync, this.colAlerts, this.colLastTransaction});
            this.dgvTerminals.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTerminals.ReadOnly = true;
            this.dgvTerminals.RowHeadersVisible = false;
            this.dgvTerminals.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // terminal columns
            // 
            this.colTerminalId.HeaderText = "Terminal";
            this.colBranch.HeaderText = "Branch";
            this.colRegion.HeaderText = "Region";
            this.colVendor.HeaderText = "Vendor";
            this.colNetwork.HeaderText = "Network";
            this.colHealth.HeaderText = "Health";
            this.colHeartbeat.HeaderText = "Heartbeat";
            this.colEjSync.HeaderText = "EJ Sync";
            this.colAlerts.HeaderText = "Alerts";
            this.colLastTransaction.HeaderText = "Last Transaction";
            this.colBranch.Width = 150;
            this.colLastTransaction.Width = 150;
            // 
            // grpCash
            // 
            this.grpCash.Controls.Add(this.dgvCash);
            this.grpCash.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpCash.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpCash.Location = new System.Drawing.Point(951, 153);
            this.grpCash.Name = "grpCash";
            this.grpCash.Size = new System.Drawing.Size(462, 323);
            this.grpCash.Text = "Cash / Replenishment Summary";
            // 
            // dgvCash
            // 
            this.dgvCash.AllowUserToAddRows = false;
            this.dgvCash.AllowUserToDeleteRows = false;
            this.dgvCash.BackgroundColor = System.Drawing.Color.White;
            this.dgvCash.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCash.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colCashTerminal, this.colCass1, this.colCass2, this.colCass3, this.colCass4,
                this.colRemaining, this.colLoaded, this.colDeposit, this.colDispense, this.colReject, this.colRetract});
            this.dgvCash.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCash.ReadOnly = true;
            this.dgvCash.RowHeadersVisible = false;
            this.dgvCash.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // cash columns
            // 
            this.colCashTerminal.HeaderText = "Terminal";
            this.colCass1.HeaderText = "Cass1";
            this.colCass2.HeaderText = "Cass2";
            this.colCass3.HeaderText = "Cass3";
            this.colCass4.HeaderText = "Cass4";
            this.colRemaining.HeaderText = "Remain";
            this.colLoaded.HeaderText = "Loaded";
            this.colDeposit.HeaderText = "Deposit";
            this.colDispense.HeaderText = "Dispense";
            this.colReject.HeaderText = "Reject";
            this.colRetract.HeaderText = "Retract";
            this.colCashTerminal.Width = 95;
            // 
            // grpAlerts
            // 
            this.pnlDashboardBody.SetColumnSpan(this.grpAlerts, 2);
            this.grpAlerts.Controls.Add(this.dgvAlerts);
            this.grpAlerts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpAlerts.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpAlerts.Location = new System.Drawing.Point(3, 482);
            this.grpAlerts.Name = "grpAlerts";
            this.grpAlerts.Size = new System.Drawing.Size(1410, 235);
            this.grpAlerts.Text = "Alert Center";
            // 
            // dgvAlerts
            // 
            this.dgvAlerts.AllowUserToAddRows = false;
            this.dgvAlerts.AllowUserToDeleteRows = false;
            this.dgvAlerts.BackgroundColor = System.Drawing.Color.White;
            this.dgvAlerts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAlerts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.colAlertTime, this.colAlertTerminal, this.colSeverity, this.colMessage});
            this.dgvAlerts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvAlerts.ReadOnly = true;
            this.dgvAlerts.RowHeadersVisible = false;
            this.dgvAlerts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.colAlertTime.HeaderText = "Time";
            this.colAlertTerminal.HeaderText = "Terminal";
            this.colSeverity.HeaderText = "Severity";
            this.colMessage.HeaderText = "Message";
            this.colMessage.Width = 900;
            // 
            // MainDashboardForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1440, 840);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusStrip);
            this.Name = "MainDashboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EJLive Central Monitoring Dashboard";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.tabDashboard.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.pnlDashboardBody.ResumeLayout(false);
            this.grpOverview.ResumeLayout(false);
            this.tblOverview.ResumeLayout(false);
            this.flowCommands.ResumeLayout(false);
            this.grpCommands.ResumeLayout(false);
            this.grpTerminalsPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTerminals)).EndInit();
            this.grpCash.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCash)).EndInit();
            this.grpAlerts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAlerts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private static void SetCaptionStyle(System.Windows.Forms.Label label)
        {
            label.Dock = System.Windows.Forms.DockStyle.Fill;
            label.ForeColor = System.Drawing.Color.DimGray;
            label.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        }

        private static void SetValueStyle(System.Windows.Forms.Label label, System.Drawing.Color color)
        {
            label.Dock = System.Windows.Forms.DockStyle.Fill;
            label.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            label.ForeColor = color;
            label.Text = "0";
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblLastUpdate;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDashboard;
        private System.Windows.Forms.TableLayoutPanel pnlDashboardBody;
        private System.Windows.Forms.GroupBox grpOverview;
        private System.Windows.Forms.TableLayoutPanel tblOverview;
        private System.Windows.Forms.Label lblTotalCaption;
        private System.Windows.Forms.Label lblOnlineCaption;
        private System.Windows.Forms.Label lblSyncedCaption;
        private System.Windows.Forms.Label lblFaultsCaption;
        private System.Windows.Forms.Label lblWarningsCaption;
        private System.Windows.Forms.Label lblCitCaption;
        private System.Windows.Forms.Label lblSupervisorCaption;
        private System.Windows.Forms.Label lblRemainingCashCaption;
        private System.Windows.Forms.Label lblTotalATMs;
        private System.Windows.Forms.Label lblOnlineATMs;
        private System.Windows.Forms.Label lblEJSynced;
        private System.Windows.Forms.Label lblFaults;
        private System.Windows.Forms.Label lblWarnings;
        private System.Windows.Forms.Label lblCitRequired;
        private System.Windows.Forms.Label lblSupervisorCount;
        private System.Windows.Forms.Label lblRemainingCash;
        private System.Windows.Forms.GroupBox grpCommands;
        private System.Windows.Forms.FlowLayoutPanel flowCommands;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnExportReport;
        private System.Windows.Forms.GroupBox grpTerminalsPreview;
        private System.Windows.Forms.DataGridView dgvTerminals;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTerminalId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBranch;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRegion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVendor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNetwork;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHealth;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHeartbeat;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEjSync;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlerts;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastTransaction;
        private System.Windows.Forms.GroupBox grpCash;
        private System.Windows.Forms.DataGridView dgvCash;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCashTerminal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCass1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCass2;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCass3;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCass4;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRemaining;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLoaded;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeposit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDispense;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReject;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRetract;
        private System.Windows.Forms.GroupBox grpAlerts;
        private System.Windows.Forms.DataGridView dgvAlerts;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlertTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlertTerminal;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSeverity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMessage;
    }
}

