namespace EJLive.Server.WinForms
{
    partial class ServerMainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

namespace EJLive.Server.WinForms
{
    partial class ServerMainForm
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
        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabServer = new System.Windows.Forms.TabPage();
            this.tabConnections = new System.Windows.Forms.TabPage();
            this.tabArchive = new System.Windows.Forms.TabPage();
            this.tabLog = new System.Windows.Forms.TabPage();
            // Server tab
            this.grpServerConfig = new System.Windows.Forms.GroupBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblStorage = new System.Windows.Forms.Label();
            this.txtStorage = new System.Windows.Forms.TextBox();
            this.btnBrowseStorage = new System.Windows.Forms.Button();
            this.lblStatusLabel = new System.Windows.Forms.Label();
            this.lblServerStatus = new System.Windows.Forms.Label();
            this.lblConnectedLabel = new System.Windows.Forms.Label();
            this.lblConnectedCount = new System.Windows.Forms.Label();
            this.btnStartStop = new System.Windows.Forms.Button();
            // Connections tab
            this.lvConnections = new System.Windows.Forms.ListView();
            this.colATMId = new System.Windows.Forms.ColumnHeader();
            this.colIP = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.colLastSync = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.grpCommands = new System.Windows.Forms.GroupBox();
            this.cmbCommand = new System.Windows.Forms.ComboBox();
            this.btnSendCommand = new System.Windows.Forms.Button();
            // Archive tab
            this.grpArchiveStats = new System.Windows.Forms.GroupBox();
            this.lblStorageUsedLabel = new System.Windows.Forms.Label();
            this.lblStorageUsed = new System.Windows.Forms.Label();
            this.lblArchivedLabel = new System.Windows.Forms.Label();
            this.lblArchivedSize = new System.Windows.Forms.Label();
            this.lblTotalFilesLabel = new System.Windows.Forms.Label();
            this.lblTotalFiles = new System.Windows.Forms.Label();
            this.btnArchiveNow = new System.Windows.Forms.Button();
            // Log tab
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            // Status strip
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();

            this.pnlHeader.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabServer.SuspendLayout();
            this.tabConnections.SuspendLayout();
            this.tabArchive.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.grpServerConfig.SuspendLayout();
            this.grpCommands.SuspendLayout();
            this.grpArchiveStats.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(20, 60, 20);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblVersion);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(850, 55);
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(15, 8);
            this.lblTitle.Text = "\u25C9 EJLive Central Server";
            //
            // lblVersion
            //
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVersion.ForeColor = System.Drawing.Color.LightGray;
            this.lblVersion.Location = new System.Drawing.Point(18, 35);
            this.lblVersion.Text = "Electronic Journal Server & Archive Manager v3.2.1 Ironclad";
            //
            // tabControl
            //
            this.tabControl.Controls.Add(this.tabServer);
            this.tabControl.Controls.Add(this.tabConnections);
            this.tabControl.Controls.Add(this.tabArchive);
            this.tabControl.Controls.Add(this.tabLog);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabControl.Location = new System.Drawing.Point(0, 55);
            this.tabControl.Name = "tabControl";
            this.tabControl.Size = new System.Drawing.Size(850, 475);
            //
            // tabServer
            //
            this.tabServer.Controls.Add(this.grpServerConfig);
            this.tabServer.Controls.Add(this.btnStartStop);
            this.tabServer.Location = new System.Drawing.Point(4, 28);
            this.tabServer.Name = "tabServer";
            this.tabServer.Padding = new System.Windows.Forms.Padding(10);
            this.tabServer.Size = new System.Drawing.Size(842, 443);
            this.tabServer.Text = "  \u25C9 Server  ";
            //
            // grpServerConfig
            //
            this.grpServerConfig.Controls.Add(this.lblPort);
            this.grpServerConfig.Controls.Add(this.txtPort);
            this.grpServerConfig.Controls.Add(this.lblStorage);
            this.grpServerConfig.Controls.Add(this.txtStorage);
            this.grpServerConfig.Controls.Add(this.btnBrowseStorage);
            this.grpServerConfig.Controls.Add(this.lblStatusLabel);
            this.grpServerConfig.Controls.Add(this.lblServerStatus);
            this.grpServerConfig.Controls.Add(this.lblConnectedLabel);
            this.grpServerConfig.Controls.Add(this.lblConnectedCount);
            this.grpServerConfig.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpServerConfig.Location = new System.Drawing.Point(20, 15);
            this.grpServerConfig.Name = "grpServerConfig";
            this.grpServerConfig.Size = new System.Drawing.Size(790, 180);
            this.grpServerConfig.Text = "Server Configuration";
            //
            // lblPort
            //
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(20, 40);
            this.lblPort.Text = "Listen Port:";
            //
            // txtPort
            //
            this.txtPort.Location = new System.Drawing.Point(160, 37);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(120, 25);
            this.txtPort.Text = "5005";
            //
            // lblStorage
            //
            this.lblStorage.AutoSize = true;
            this.lblStorage.Location = new System.Drawing.Point(20, 80);
            this.lblStorage.Text = "Storage Path:";
            //
            // txtStorage
            //
            this.txtStorage.Location = new System.Drawing.Point(160, 77);
            this.txtStorage.Name = "txtStorage";
            this.txtStorage.Size = new System.Drawing.Size(480, 25);
            this.txtStorage.Text = "C:\\EJLive_Storage";
            //
            // btnBrowseStorage
            //
            this.btnBrowseStorage.Location = new System.Drawing.Point(650, 76);
            this.btnBrowseStorage.Name = "btnBrowseStorage";
            this.btnBrowseStorage.Size = new System.Drawing.Size(90, 27);
            this.btnBrowseStorage.Text = "Browse...";
            this.btnBrowseStorage.Click += new System.EventHandler(this.btnBrowseStorage_Click);
            //
            // lblStatusLabel
            //
            this.lblStatusLabel.AutoSize = true;
            this.lblStatusLabel.Location = new System.Drawing.Point(20, 120);
            this.lblStatusLabel.Text = "Status:";
            //
            // lblServerStatus
            //
            this.lblServerStatus.AutoSize = true;
            this.lblServerStatus.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblServerStatus.ForeColor = System.Drawing.Color.Red;
            this.lblServerStatus.Location = new System.Drawing.Point(160, 119);
            this.lblServerStatus.Text = "Stopped";
            //
            // lblConnectedLabel
            //
            this.lblConnectedLabel.AutoSize = true;
            this.lblConnectedLabel.Location = new System.Drawing.Point(20, 148);
            this.lblConnectedLabel.Text = "Connected ATMs:";
            //
            // lblConnectedCount
            //
            this.lblConnectedCount.AutoSize = true;
            this.lblConnectedCount.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblConnectedCount.Location = new System.Drawing.Point(160, 147);
            this.lblConnectedCount.Text = "0";
            //
            // btnStartStop
            //
            this.btnStartStop.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartStop.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartStop.ForeColor = System.Drawing.Color.White;
            this.btnStartStop.Location = new System.Drawing.Point(20, 210);
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.Size = new System.Drawing.Size(200, 48);
            this.btnStartStop.Text = "\u25B6 Start Server";
            this.btnStartStop.UseVisualStyleBackColor = false;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            //
            // tabConnections
            //
            this.tabConnections.Controls.Add(this.lvConnections);
            this.tabConnections.Controls.Add(this.grpCommands);
            this.tabConnections.Location = new System.Drawing.Point(4, 28);
            this.tabConnections.Name = "tabConnections";
            this.tabConnections.Padding = new System.Windows.Forms.Padding(10);
            this.tabConnections.Size = new System.Drawing.Size(842, 443);
            this.tabConnections.Text = "  \u26A1 Connections  ";
            //
            // lvConnections
            //
            this.lvConnections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colATMId, this.colIP, this.colStatus, this.colLastSync, this.colType });
            this.lvConnections.FullRowSelect = true;
            this.lvConnections.GridLines = true;
            this.lvConnections.Location = new System.Drawing.Point(10, 10);
            this.lvConnections.Name = "lvConnections";
            this.lvConnections.Size = new System.Drawing.Size(810, 280);
            this.lvConnections.View = System.Windows.Forms.View.Details;
            //
            // colATMId
            //
            this.colATMId.Text = "ATM ID";
            this.colATMId.Width = 130;
            //
            // colIP
            //
            this.colIP.Text = "IP Address";
            this.colIP.Width = 140;
            //
            // colStatus
            //
            this.colStatus.Text = "Status";
            this.colStatus.Width = 130;
            //
            // colLastSync
            //
            this.colLastSync.Text = "Last Sync";
            this.colLastSync.Width = 180;
            //
            // colType
            //
            this.colType.Text = "ATM Type";
            this.colType.Width = 100;
            //
            // grpCommands
            //
            this.grpCommands.Controls.Add(this.cmbCommand);
            this.grpCommands.Controls.Add(this.btnSendCommand);
            this.grpCommands.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpCommands.Location = new System.Drawing.Point(10, 300);
            this.grpCommands.Name = "grpCommands";
            this.grpCommands.Size = new System.Drawing.Size(810, 70);
            this.grpCommands.Text = "Remote Commands";
            //
            // cmbCommand
            //
            this.cmbCommand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCommand.Items.AddRange(new object[] { "RESTART", "SCREENSHOT", "TIMESYNC", "SHUTDOWN" });
            this.cmbCommand.Location = new System.Drawing.Point(20, 30);
            this.cmbCommand.Name = "cmbCommand";
            this.cmbCommand.Size = new System.Drawing.Size(200, 25);
            //
            // btnSendCommand
            //
            this.btnSendCommand.BackColor = System.Drawing.Color.FromArgb(200, 80, 0);
            this.btnSendCommand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendCommand.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSendCommand.ForeColor = System.Drawing.Color.White;
            this.btnSendCommand.Location = new System.Drawing.Point(240, 28);
            this.btnSendCommand.Name = "btnSendCommand";
            this.btnSendCommand.Size = new System.Drawing.Size(150, 30);
            this.btnSendCommand.Text = "\u26A0 Send Command";
            this.btnSendCommand.UseVisualStyleBackColor = false;
            this.btnSendCommand.Click += new System.EventHandler(this.btnSendCommand_Click);
            //
            // tabArchive
            //
            this.tabArchive.Controls.Add(this.grpArchiveStats);
            this.tabArchive.Controls.Add(this.btnArchiveNow);
            this.tabArchive.Location = new System.Drawing.Point(4, 28);
            this.tabArchive.Name = "tabArchive";
            this.tabArchive.Padding = new System.Windows.Forms.Padding(10);
            this.tabArchive.Size = new System.Drawing.Size(842, 443);
            this.tabArchive.Text = "  \u2261 Archive  ";
            //
            // grpArchiveStats
            //
            this.grpArchiveStats.Controls.Add(this.lblStorageUsedLabel);
            this.grpArchiveStats.Controls.Add(this.lblStorageUsed);
            this.grpArchiveStats.Controls.Add(this.lblArchivedLabel);
            this.grpArchiveStats.Controls.Add(this.lblArchivedSize);
            this.grpArchiveStats.Controls.Add(this.lblTotalFilesLabel);
            this.grpArchiveStats.Controls.Add(this.lblTotalFiles);
            this.grpArchiveStats.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpArchiveStats.Location = new System.Drawing.Point(20, 15);
            this.grpArchiveStats.Name = "grpArchiveStats";
            this.grpArchiveStats.Size = new System.Drawing.Size(790, 140);
            this.grpArchiveStats.Text = "Storage Statistics";
            //
            // lblStorageUsedLabel
            //
            this.lblStorageUsedLabel.AutoSize = true;
            this.lblStorageUsedLabel.Location = new System.Drawing.Point(20, 40);
            this.lblStorageUsedLabel.Text = "Total Storage Used:";
            //
            // lblStorageUsed
            //
            this.lblStorageUsed.AutoSize = true;
            this.lblStorageUsed.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStorageUsed.Location = new System.Drawing.Point(200, 40);
            this.lblStorageUsed.Text = "0 B";
            //
            // lblArchivedLabel
            //
            this.lblArchivedLabel.AutoSize = true;
            this.lblArchivedLabel.Location = new System.Drawing.Point(20, 70);
            this.lblArchivedLabel.Text = "Archived Size:";
            //
            // lblArchivedSize
            //
            this.lblArchivedSize.AutoSize = true;
            this.lblArchivedSize.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblArchivedSize.Location = new System.Drawing.Point(200, 70);
            this.lblArchivedSize.Text = "0 B";
            //
            // lblTotalFilesLabel
            //
            this.lblTotalFilesLabel.AutoSize = true;
            this.lblTotalFilesLabel.Location = new System.Drawing.Point(20, 100);
            this.lblTotalFilesLabel.Text = "Total Files:";
            //
            // lblTotalFiles
            //
            this.lblTotalFiles.AutoSize = true;
            this.lblTotalFiles.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalFiles.Location = new System.Drawing.Point(200, 100);
            this.lblTotalFiles.Text = "0";
            //
            // btnArchiveNow
            //
            this.btnArchiveNow.BackColor = System.Drawing.Color.FromArgb(100, 50, 150);
            this.btnArchiveNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArchiveNow.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnArchiveNow.ForeColor = System.Drawing.Color.White;
            this.btnArchiveNow.Location = new System.Drawing.Point(20, 170);
            this.btnArchiveNow.Name = "btnArchiveNow";
            this.btnArchiveNow.Size = new System.Drawing.Size(180, 40);
            this.btnArchiveNow.Text = "\u2261 Archive Now";
            this.btnArchiveNow.UseVisualStyleBackColor = false;
            this.btnArchiveNow.Click += new System.EventHandler(this.btnArchiveNow_Click);
            //
            // tabLog
            //
            this.tabLog.Controls.Add(this.txtLog);
            this.tabLog.Controls.Add(this.btnClearLog);
            this.tabLog.Location = new System.Drawing.Point(4, 28);
            this.tabLog.Name = "tabLog";
            this.tabLog.Padding = new System.Windows.Forms.Padding(10);
            this.tabLog.Size = new System.Drawing.Size(842, 443);
            this.tabLog.Text = "  \u2263 Log  ";
            //
            // txtLog
            //
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.ForeColor = System.Drawing.Color.LightGreen;
            this.txtLog.Location = new System.Drawing.Point(10, 10);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(810, 370);
            //
            // btnClearLog
            //
            this.btnClearLog.Location = new System.Drawing.Point(10, 390);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(100, 30);
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.tsslStatus });
            this.statusStrip.Location = new System.Drawing.Point(0, 530);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(850, 22);
            //
            // tsslStatus
            //
            this.tsslStatus.Name = "tsslStatus";
            this.tsslStatus.Text = "Ready";
            //
            // ServerMainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 552);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(866, 591);
            this.Name = "ServerMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EJLive Central Server";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabServer.ResumeLayout(false);
            this.tabConnections.ResumeLayout(false);
            this.tabArchive.ResumeLayout(false);
            this.tabLog.ResumeLayout(false);
            this.tabLog.PerformLayout();
            this.grpServerConfig.ResumeLayout(false);
            this.grpServerConfig.PerformLayout();
            this.grpCommands.ResumeLayout(false);
            this.grpArchiveStats.ResumeLayout(false);
            this.grpArchiveStats.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabServer;
        private System.Windows.Forms.TabPage tabConnections;
        private System.Windows.Forms.TabPage tabArchive;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.GroupBox grpServerConfig;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblStorage;
        private System.Windows.Forms.TextBox txtStorage;
        private System.Windows.Forms.Button btnBrowseStorage;
        private System.Windows.Forms.Label lblStatusLabel;
        private System.Windows.Forms.Label lblServerStatus;
        private System.Windows.Forms.Label lblConnectedLabel;
        private System.Windows.Forms.Label lblConnectedCount;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.ListView lvConnections;
        private System.Windows.Forms.ColumnHeader colATMId;
        private System.Windows.Forms.ColumnHeader colIP;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colLastSync;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.GroupBox grpCommands;
        private System.Windows.Forms.ComboBox cmbCommand;
        private System.Windows.Forms.Button btnSendCommand;
        private System.Windows.Forms.GroupBox grpArchiveStats;
        private System.Windows.Forms.Label lblStorageUsedLabel;
        private System.Windows.Forms.Label lblStorageUsed;
        private System.Windows.Forms.Label lblArchivedLabel;
        private System.Windows.Forms.Label lblArchivedSize;
        private System.Windows.Forms.Label lblTotalFilesLabel;
        private System.Windows.Forms.Label lblTotalFiles;
        private System.Windows.Forms.Button btnArchiveNow;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabServer = new System.Windows.Forms.TabPage();
            this.tabConnections = new System.Windows.Forms.TabPage();
            this.tabArchive = new System.Windows.Forms.TabPage();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.grpServerConfig = new System.Windows.Forms.GroupBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblStorage = new System.Windows.Forms.Label();
            this.txtStorage = new System.Windows.Forms.TextBox();
            this.btnBrowseStorage = new System.Windows.Forms.Button();
            this.lblStatusLabel = new System.Windows.Forms.Label();
            this.lblServerStatus = new System.Windows.Forms.Label();
            this.lblConnectedLabel = new System.Windows.Forms.Label();
            this.lblConnectedCount = new System.Windows.Forms.Label();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.lvConnections = new System.Windows.Forms.ListView();
            this.colATMId = new System.Windows.Forms.ColumnHeader();
            this.colIP = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.colLastSync = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.grpCommands = new System.Windows.Forms.GroupBox();
            this.cmbCommand = new System.Windows.Forms.ComboBox();
            this.btnSendCommand = new System.Windows.Forms.Button();
            this.btnAtmDetails = new System.Windows.Forms.Button();
            this.btnOpenSyncDashboard = new System.Windows.Forms.Button();
            this.grpAtmSummary = new System.Windows.Forms.GroupBox();
            this.lblSummaryAtm = new System.Windows.Forms.Label();
            this.lblSummaryStatus = new System.Windows.Forms.Label();
            this.lblSummaryVendor = new System.Windows.Forms.Label();
            this.lblSummaryLineage = new System.Windows.Forms.Label();
            this.rtbAtmSummary = new System.Windows.Forms.RichTextBox();
            this.grpArchiveStats = new System.Windows.Forms.GroupBox();
            this.lblStorageUsedLabel = new System.Windows.Forms.Label();
            this.lblStorageUsed = new System.Windows.Forms.Label();
            this.lblArchivedLabel = new System.Windows.Forms.Label();
            this.lblArchivedSize = new System.Windows.Forms.Label();
            this.lblTotalFilesLabel = new System.Windows.Forms.Label();
            this.lblTotalFiles = new System.Windows.Forms.Label();
            this.btnArchiveNow = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlHeader.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabServer.SuspendLayout();
            this.tabConnections.SuspendLayout();
            this.tabArchive.SuspendLayout();
            this.tabLog.SuspendLayout();
            this.grpServerConfig.SuspendLayout();
            this.grpCommands.SuspendLayout();
            this.grpAtmSummary.SuspendLayout();
            this.grpArchiveStats.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(20, 60, 20);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblVersion);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1120, 55);
            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(15, 8);
            this.lblTitle.Text = "â—‰ EJLive Central Server";
            // lblVersion
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblVersion.ForeColor = System.Drawing.Color.LightGray;
            this.lblVersion.Location = new System.Drawing.Point(18, 35);
            this.lblVersion.Text = "Electronic Journal Server & Archive Manager";
            // tabControl
            this.tabControl.Controls.Add(this.tabServer);
            this.tabControl.Controls.Add(this.tabConnections);
            this.tabControl.Controls.Add(this.tabArchive);
            this.tabControl.Controls.Add(this.tabLog);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabControl.Location = new System.Drawing.Point(0, 55);
            this.tabControl.Size = new System.Drawing.Size(1120, 615);
            // tabServer
            this.tabServer.Controls.Add(this.grpServerConfig);
            this.tabServer.Controls.Add(this.btnStartStop);
            this.tabServer.Location = new System.Drawing.Point(4, 28);
            this.tabServer.Padding = new System.Windows.Forms.Padding(10);
            this.tabServer.Size = new System.Drawing.Size(1112, 583);
            this.tabServer.Text = "  â—‰ Server  ";
            // grpServerConfig
            this.grpServerConfig.Controls.Add(this.lblPort);
            this.grpServerConfig.Controls.Add(this.txtPort);
            this.grpServerConfig.Controls.Add(this.lblStorage);
            this.grpServerConfig.Controls.Add(this.txtStorage);
            this.grpServerConfig.Controls.Add(this.btnBrowseStorage);
            this.grpServerConfig.Controls.Add(this.lblStatusLabel);
            this.grpServerConfig.Controls.Add(this.lblServerStatus);
            this.grpServerConfig.Controls.Add(this.lblConnectedLabel);
            this.grpServerConfig.Controls.Add(this.lblConnectedCount);
            this.grpServerConfig.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpServerConfig.Location = new System.Drawing.Point(20, 15);
            this.grpServerConfig.Size = new System.Drawing.Size(860, 180);
            this.grpServerConfig.Text = "Server Configuration";
            // port/storage/status controls
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(20, 40);
            this.lblPort.Text = "Listen Port:";
            this.txtPort.Location = new System.Drawing.Point(160, 37);
            this.txtPort.Size = new System.Drawing.Size(120, 25);
            this.txtPort.Text = "5005";
            this.lblStorage.AutoSize = true;
            this.lblStorage.Location = new System.Drawing.Point(20, 80);
            this.lblStorage.Text = "Storage Path:";
            this.txtStorage.Location = new System.Drawing.Point(160, 77);
            this.txtStorage.Size = new System.Drawing.Size(520, 25);
            this.txtStorage.Text = "C:\\EJLive_Storage";
            this.btnBrowseStorage.Location = new System.Drawing.Point(700, 76);
            this.btnBrowseStorage.Size = new System.Drawing.Size(90, 27);
            this.btnBrowseStorage.Text = "Browse...";
            this.btnBrowseStorage.Click += new System.EventHandler(this.btnBrowseStorage_Click);
            this.lblStatusLabel.AutoSize = true;
            this.lblStatusLabel.Location = new System.Drawing.Point(20, 120);
            this.lblStatusLabel.Text = "Status:";
            this.lblServerStatus.AutoSize = true;
            this.lblServerStatus.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblServerStatus.ForeColor = System.Drawing.Color.Red;
            this.lblServerStatus.Location = new System.Drawing.Point(160, 119);
            this.lblServerStatus.Text = "Stopped";
            this.lblConnectedLabel.AutoSize = true;
            this.lblConnectedLabel.Location = new System.Drawing.Point(20, 148);
            this.lblConnectedLabel.Text = "Connected ATMs:";
            this.lblConnectedCount.AutoSize = true;
            this.lblConnectedCount.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblConnectedCount.Location = new System.Drawing.Point(160, 147);
            this.lblConnectedCount.Text = "0";
            // btnStartStop
            this.btnStartStop.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.btnStartStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartStop.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnStartStop.ForeColor = System.Drawing.Color.White;
            this.btnStartStop.Location = new System.Drawing.Point(20, 210);
            this.btnStartStop.Size = new System.Drawing.Size(200, 48);
            this.btnStartStop.Text = "â–¶ Start Server";
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // tabConnections
            this.tabConnections.Controls.Add(this.grpAtmSummary);
            this.tabConnections.Controls.Add(this.lvConnections);
            this.tabConnections.Controls.Add(this.grpCommands);
            this.tabConnections.Location = new System.Drawing.Point(4, 28);
            this.tabConnections.Padding = new System.Windows.Forms.Padding(10);
            this.tabConnections.Size = new System.Drawing.Size(1112, 583);
            this.tabConnections.Text = "  âš¡ Connections  ";
            // lvConnections
            this.lvConnections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this.colATMId, this.colIP, this.colStatus, this.colLastSync, this.colType });
            this.lvConnections.FullRowSelect = true;
            this.lvConnections.GridLines = true;
            this.lvConnections.Location = new System.Drawing.Point(10, 10);
            this.lvConnections.Size = new System.Drawing.Size(650, 300);
            this.lvConnections.View = System.Windows.Forms.View.Details;
            this.lvConnections.SelectedIndexChanged += new System.EventHandler(this.lvConnections_SelectedIndexChanged);
            this.colATMId.Text = "ATM ID";
            this.colATMId.Width = 130;
            this.colIP.Text = "IP Address";
            this.colIP.Width = 130;
            this.colStatus.Text = "Status";
            this.colStatus.Width = 100;
            this.colLastSync.Text = "Last Sync";
            this.colLastSync.Width = 180;
            this.colType.Text = "ATM Type";
            this.colType.Width = 90;
            // grpAtmSummary
            this.grpAtmSummary.Controls.Add(this.lblSummaryAtm);
            this.grpAtmSummary.Controls.Add(this.lblSummaryStatus);
            this.grpAtmSummary.Controls.Add(this.lblSummaryVendor);
            this.grpAtmSummary.Controls.Add(this.lblSummaryLineage);
            this.grpAtmSummary.Controls.Add(this.rtbAtmSummary);
            this.grpAtmSummary.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpAtmSummary.Location = new System.Drawing.Point(675, 10);
            this.grpAtmSummary.Size = new System.Drawing.Size(425, 300);
            this.grpAtmSummary.Text = "Selected ATM Summary";
            this.lblSummaryAtm.AutoSize = true;
            this.lblSummaryAtm.Location = new System.Drawing.Point(15, 28);
            this.lblSummaryAtm.Text = "ATM: --";
            this.lblSummaryStatus.AutoSize = true;
            this.lblSummaryStatus.Location = new System.Drawing.Point(15, 52);
            this.lblSummaryStatus.Text = "Status: --";
            this.lblSummaryVendor.AutoSize = true;
            this.lblSummaryVendor.Location = new System.Drawing.Point(15, 76);
            this.lblSummaryVendor.Text = "Vendor: --";
            this.lblSummaryLineage.AutoSize = true;
            this.lblSummaryLineage.Location = new System.Drawing.Point(15, 100);
            this.lblSummaryLineage.Text = "Lineage: --";
            this.rtbAtmSummary.BackColor = System.Drawing.Color.FromArgb(24, 24, 34);
            this.rtbAtmSummary.Font = new System.Drawing.Font("Consolas", 9F);
            this.rtbAtmSummary.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.rtbAtmSummary.Location = new System.Drawing.Point(18, 128);
            this.rtbAtmSummary.ReadOnly = true;
            this.rtbAtmSummary.Size = new System.Drawing.Size(390, 155);
            this.rtbAtmSummary.Text = "Select an ATM to see its Root capabilities and operational summary.";
            // grpCommands
            this.grpCommands.Controls.Add(this.btnOpenSyncDashboard);
            this.grpCommands.Controls.Add(this.btnAtmDetails);
            this.grpCommands.Controls.Add(this.cmbCommand);
            this.grpCommands.Controls.Add(this.btnSendCommand);
            this.grpCommands.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpCommands.Location = new System.Drawing.Point(10, 320);
            this.grpCommands.Size = new System.Drawing.Size(1090, 80);
            this.grpCommands.Text = "Server Actions";
            this.cmbCommand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCommand.Items.AddRange(new object[] { "RESTART", "SCREENSHOT", "TIMESYNC", "SHUTDOWN" });
            this.cmbCommand.Location = new System.Drawing.Point(20, 34);
            this.cmbCommand.Size = new System.Drawing.Size(180, 25);
            this.btnSendCommand.BackColor = System.Drawing.Color.FromArgb(200, 80, 0);
            this.btnSendCommand.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendCommand.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSendCommand.ForeColor = System.Drawing.Color.White;
            this.btnSendCommand.Location = new System.Drawing.Point(215, 32);
            this.btnSendCommand.Size = new System.Drawing.Size(145, 30);
            this.btnSendCommand.Text = "âš  Send Command";
            this.btnSendCommand.Click += new System.EventHandler(this.btnSendCommand_Click);
            this.btnAtmDetails.Location = new System.Drawing.Point(380, 32);
            this.btnAtmDetails.Size = new System.Drawing.Size(150, 30);
            this.btnAtmDetails.Text = "ATM Details";
            this.btnAtmDetails.Click += new System.EventHandler(this.btnAtmDetails_Click);
            this.btnOpenSyncDashboard.Location = new System.Drawing.Point(545, 32);
            this.btnOpenSyncDashboard.Size = new System.Drawing.Size(160, 30);
            this.btnOpenSyncDashboard.Text = "Sync Dashboard";
            this.btnOpenSyncDashboard.Click += new System.EventHandler(this.btnOpenSyncDashboard_Click);
            // tabArchive
            this.tabArchive.Controls.Add(this.grpArchiveStats);
            this.tabArchive.Controls.Add(this.btnArchiveNow);
            this.tabArchive.Location = new System.Drawing.Point(4, 28);
            this.tabArchive.Padding = new System.Windows.Forms.Padding(10);
            this.tabArchive.Size = new System.Drawing.Size(1112, 583);
            this.tabArchive.Text = "  â‰¡ Archive  ";
            this.grpArchiveStats.Controls.Add(this.lblStorageUsedLabel);
            this.grpArchiveStats.Controls.Add(this.lblStorageUsed);
            this.grpArchiveStats.Controls.Add(this.lblArchivedLabel);
            this.grpArchiveStats.Controls.Add(this.lblArchivedSize);
            this.grpArchiveStats.Controls.Add(this.lblTotalFilesLabel);
            this.grpArchiveStats.Controls.Add(this.lblTotalFiles);
            this.grpArchiveStats.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpArchiveStats.Location = new System.Drawing.Point(20, 15);
            this.grpArchiveStats.Size = new System.Drawing.Size(790, 140);
            this.grpArchiveStats.Text = "Storage Statistics";
            this.lblStorageUsedLabel.AutoSize = true;
            this.lblStorageUsedLabel.Location = new System.Drawing.Point(20, 40);
            this.lblStorageUsedLabel.Text = "Total Storage Used:";
            this.lblStorageUsed.AutoSize = true;
            this.lblStorageUsed.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStorageUsed.Location = new System.Drawing.Point(200, 40);
            this.lblStorageUsed.Text = "0 B";
            this.lblArchivedLabel.AutoSize = true;
            this.lblArchivedLabel.Location = new System.Drawing.Point(20, 70);
            this.lblArchivedLabel.Text = "Archived Size:";
            this.lblArchivedSize.AutoSize = true;
            this.lblArchivedSize.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblArchivedSize.Location = new System.Drawing.Point(200, 70);
            this.lblArchivedSize.Text = "0 B";
            this.lblTotalFilesLabel.AutoSize = true;
            this.lblTotalFilesLabel.Location = new System.Drawing.Point(20, 100);
            this.lblTotalFilesLabel.Text = "Total Files:";
            this.lblTotalFiles.AutoSize = true;
            this.lblTotalFiles.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalFiles.Location = new System.Drawing.Point(200, 100);
            this.lblTotalFiles.Text = "0";
            this.btnArchiveNow.BackColor = System.Drawing.Color.FromArgb(100, 50, 150);
            this.btnArchiveNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnArchiveNow.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnArchiveNow.ForeColor = System.Drawing.Color.White;
            this.btnArchiveNow.Location = new System.Drawing.Point(20, 170);
            this.btnArchiveNow.Size = new System.Drawing.Size(180, 40);
            this.btnArchiveNow.Text = "â‰¡ Archive Now";
            this.btnArchiveNow.Click += new System.EventHandler(this.btnArchiveNow_Click);
            // tabLog
            this.tabLog.Controls.Add(this.txtLog);
            this.tabLog.Controls.Add(this.btnClearLog);
            this.tabLog.Location = new System.Drawing.Point(4, 28);
            this.tabLog.Padding = new System.Windows.Forms.Padding(10);
            this.tabLog.Size = new System.Drawing.Size(1112, 583);
            this.tabLog.Text = "  â‰£ Log  ";
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(20, 20, 30);
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLog.ForeColor = System.Drawing.Color.LightGreen;
            this.txtLog.Location = new System.Drawing.Point(10, 10);
            this.txtLog.Multiline = true;
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(1090, 450);
            this.btnClearLog.Location = new System.Drawing.Point(10, 470);
            this.btnClearLog.Size = new System.Drawing.Size(100, 30);
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // statusStrip
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.tsslStatus });
            this.statusStrip.Location = new System.Drawing.Point(0, 670);
            this.statusStrip.Size = new System.Drawing.Size(1120, 22);
            this.tsslStatus.Text = "Ready";
            // ServerMainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1120, 692);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1136, 731);
            this.Name = "ServerMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EJLive Central Server";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabServer.ResumeLayout(false);
            this.tabConnections.ResumeLayout(false);
            this.tabArchive.ResumeLayout(false);
            this.tabLog.ResumeLayout(false);
            this.tabLog.PerformLayout();
            this.grpServerConfig.ResumeLayout(false);
            this.grpServerConfig.PerformLayout();
            this.grpCommands.ResumeLayout(false);
            this.grpAtmSummary.ResumeLayout(false);
            this.grpAtmSummary.PerformLayout();
            this.grpArchiveStats.ResumeLayout(false);
            this.grpArchiveStats.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabServer;
        private System.Windows.Forms.TabPage tabConnections;
        private System.Windows.Forms.TabPage tabArchive;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.GroupBox grpServerConfig;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblStorage;
        private System.Windows.Forms.TextBox txtStorage;
        private System.Windows.Forms.Button btnBrowseStorage;
        private System.Windows.Forms.Label lblStatusLabel;
        private System.Windows.Forms.Label lblServerStatus;
        private System.Windows.Forms.Label lblConnectedLabel;
        private System.Windows.Forms.Label lblConnectedCount;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.ListView lvConnections;
        private System.Windows.Forms.ColumnHeader colATMId;
        private System.Windows.Forms.ColumnHeader colIP;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colLastSync;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.GroupBox grpCommands;
        private System.Windows.Forms.ComboBox cmbCommand;
        private System.Windows.Forms.Button btnSendCommand;
        private System.Windows.Forms.Button btnAtmDetails;
        private System.Windows.Forms.Button btnOpenSyncDashboard;
        private System.Windows.Forms.GroupBox grpAtmSummary;
        private System.Windows.Forms.Label lblSummaryAtm;
        private System.Windows.Forms.Label lblSummaryStatus;
        private System.Windows.Forms.Label lblSummaryVendor;
        private System.Windows.Forms.Label lblSummaryLineage;
        private System.Windows.Forms.RichTextBox rtbAtmSummary;
        private System.Windows.Forms.GroupBox grpArchiveStats;
        private System.Windows.Forms.Label lblStorageUsedLabel;
        private System.Windows.Forms.Label lblStorageUsed;
        private System.Windows.Forms.Label lblArchivedLabel;
        private System.Windows.Forms.Label lblArchivedSize;
        private System.Windows.Forms.Label lblTotalFilesLabel;
        private System.Windows.Forms.Label lblTotalFiles;
        private System.Windows.Forms.Button btnArchiveNow;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
    }
}

