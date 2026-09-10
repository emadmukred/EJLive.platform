namespace EJLive.Installer.WinForms
{
    partial class InstallerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

namespace EJLive.Installer.WinForms
{
    partial class InstallerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
   
        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.grpRole = new System.Windows.Forms.GroupBox();
            this.btnInstallClient = new System.Windows.Forms.Button();
            this.btnInstallServer = new System.Windows.Forms.Button();
            this.lblClientDesc = new System.Windows.Forms.Label();
            this.lblServerDesc = new System.Windows.Forms.Label();
            this.lblSelectedRole = new System.Windows.Forms.Label();
            this.grpClientSettings = new System.Windows.Forms.GroupBox();
            this.lblATMType = new System.Windows.Forms.Label();
            this.cmbATMType = new System.Windows.Forms.ComboBox();
            this.chkAutoStartWithWindows = new System.Windows.Forms.CheckBox();
            this.lblJournalPath = new System.Windows.Forms.Label();
            this.lblJournalPathValue = new System.Windows.Forms.Label();
            this.lblBackupPath = new System.Windows.Forms.Label();
            this.lblBackupPathValue = new System.Windows.Forms.Label();
            this.grpPath = new System.Windows.Forms.GroupBox();
            this.lblInstallPath = new System.Windows.Forms.Label();
            this.txtInstallPath = new System.Windows.Forms.TextBox();
            this.btnBrowsePath = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.grpRole.SuspendLayout();
            this.grpClientSettings.SuspendLayout();
            this.grpPath.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(25, 25, 60);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Size = new System.Drawing.Size(550, 70);
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 10);
            this.lblTitle.Text = "EJLive Setup Wizard";
            //
            // lblSubtitle
            //
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.LightGray;
            this.lblSubtitle.Location = new System.Drawing.Point(22, 42);
            this.lblSubtitle.Text = "Electronic Journal Live Monitoring System v4.0.0";
            //
            // grpRole
            //
            this.grpRole.Controls.Add(this.btnInstallClient);
            this.grpRole.Controls.Add(this.btnInstallServer);
            this.grpRole.Controls.Add(this.lblClientDesc);
            this.grpRole.Controls.Add(this.lblServerDesc);
            this.grpRole.Controls.Add(this.lblSelectedRole);
            this.grpRole.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpRole.Location = new System.Drawing.Point(20, 85);
            this.grpRole.Size = new System.Drawing.Size(510, 170);
            this.grpRole.Text = "Select Installation Type";
            //
            // btnInstallClient
            //
            this.btnInstallClient.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.btnInstallClient.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstallClient.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnInstallClient.ForeColor = System.Drawing.Color.White;
            this.btnInstallClient.Location = new System.Drawing.Point(20, 35);
            this.btnInstallClient.Size = new System.Drawing.Size(220, 45);
            this.btnInstallClient.Text = "ATM Client";
            this.btnInstallClient.Click += new System.EventHandler(this.btnInstallClient_Click);
            //
            // btnInstallServer
            //
            this.btnInstallServer.BackColor = System.Drawing.Color.FromArgb(0, 150, 50);
            this.btnInstallServer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstallServer.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnInstallServer.ForeColor = System.Drawing.Color.White;
            this.btnInstallServer.Location = new System.Drawing.Point(270, 35);
            this.btnInstallServer.Size = new System.Drawing.Size(220, 45);
            this.btnInstallServer.Text = "Central Server";
            this.btnInstallServer.Click += new System.EventHandler(this.btnInstallServer_Click);
            //
            // lblClientDesc
            //
            this.lblClientDesc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblClientDesc.ForeColor = System.Drawing.Color.Gray;
            this.lblClientDesc.Location = new System.Drawing.Point(20, 85);
            this.lblClientDesc.Size = new System.Drawing.Size(220, 40);
            this.lblClientDesc.Text = "Install on ATM machines.\nSupports NCR, GRG, WN, Diebold, and Hyosung.";
            //
            // lblServerDesc
            //
            this.lblServerDesc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblServerDesc.ForeColor = System.Drawing.Color.Gray;
            this.lblServerDesc.Location = new System.Drawing.Point(270, 85);
            this.lblServerDesc.Size = new System.Drawing.Size(220, 40);
            this.lblServerDesc.Text = "Install on central server.\nMonitoring, archive, and control.";
            //
            // lblSelectedRole
            //
            this.lblSelectedRole.AutoSize = true;
            this.lblSelectedRole.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblSelectedRole.Location = new System.Drawing.Point(20, 135);
            this.lblSelectedRole.Text = "";
            //
            // grpClientSettings
            //
            this.grpClientSettings.Controls.Add(this.lblATMType);
            this.grpClientSettings.Controls.Add(this.cmbATMType);
            this.grpClientSettings.Controls.Add(this.chkAutoStartWithWindows);
            this.grpClientSettings.Controls.Add(this.lblJournalPath);
            this.grpClientSettings.Controls.Add(this.lblJournalPathValue);
            this.grpClientSettings.Controls.Add(this.lblBackupPath);
            this.grpClientSettings.Controls.Add(this.lblBackupPathValue);
            this.grpClientSettings.Enabled = false;
            this.grpClientSettings.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpClientSettings.Location = new System.Drawing.Point(20, 265);
            this.grpClientSettings.Size = new System.Drawing.Size(510, 110);
            this.grpClientSettings.Text = "ATM Client Settings";
            //
            // lblATMType
            //
            this.lblATMType.AutoSize = true;
            this.lblATMType.Location = new System.Drawing.Point(15, 30);
            this.lblATMType.Text = "ATM Type:";
            //
            // cmbATMType
            //
            this.cmbATMType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbATMType.Location = new System.Drawing.Point(105, 27);
            this.cmbATMType.Size = new System.Drawing.Size(140, 25);
            this.cmbATMType.SelectedIndexChanged += new System.EventHandler(this.cmbATMType_SelectedIndexChanged);
            //
            // chkAutoStartWithWindows
            //
            this.chkAutoStartWithWindows.AutoSize = true;
            this.chkAutoStartWithWindows.Checked = true;
            this.chkAutoStartWithWindows.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoStartWithWindows.Location = new System.Drawing.Point(270, 29);
            this.chkAutoStartWithWindows.Size = new System.Drawing.Size(205, 23);
            this.chkAutoStartWithWindows.Text = "Start with Windows as Admin";
            this.chkAutoStartWithWindows.UseVisualStyleBackColor = true;
            //
            // lblJournalPath
            //
            this.lblJournalPath.AutoSize = true;
            this.lblJournalPath.Location = new System.Drawing.Point(15, 62);
            this.lblJournalPath.Text = "Journal:";
            //
            // lblJournalPathValue
            //
            this.lblJournalPathValue.AutoEllipsis = true;
            this.lblJournalPathValue.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70);
            this.lblJournalPathValue.Location = new System.Drawing.Point(105, 62);
            this.lblJournalPathValue.Size = new System.Drawing.Size(385, 20);
            //
            // lblBackupPath
            //
            this.lblBackupPath.AutoSize = true;
            this.lblBackupPath.Location = new System.Drawing.Point(15, 84);
            this.lblBackupPath.Text = "Backup:";
            //
            // lblBackupPathValue
            //
            this.lblBackupPathValue.AutoEllipsis = true;
            this.lblBackupPathValue.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70);
            this.lblBackupPathValue.Location = new System.Drawing.Point(105, 84);
            this.lblBackupPathValue.Size = new System.Drawing.Size(385, 20);
            //
            // grpPath
            //
            this.grpPath.Controls.Add(this.lblInstallPath);
            this.grpPath.Controls.Add(this.txtInstallPath);
            this.grpPath.Controls.Add(this.btnBrowsePath);
            this.grpPath.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpPath.Location = new System.Drawing.Point(20, 385);
            this.grpPath.Size = new System.Drawing.Size(510, 70);
            this.grpPath.Text = "Installation Path";
            //
            // lblInstallPath
            //
            this.lblInstallPath.AutoSize = true;
            this.lblInstallPath.Location = new System.Drawing.Point(15, 32);
            this.lblInstallPath.Text = "Path:";
            //
            // txtInstallPath
            //
            this.txtInstallPath.Location = new System.Drawing.Point(70, 29);
            this.txtInstallPath.Size = new System.Drawing.Size(340, 25);
            this.txtInstallPath.Text = "C:\\ProgramData\\EJLive";
            //
            // btnBrowsePath
            //
            this.btnBrowsePath.Location = new System.Drawing.Point(420, 28);
            this.btnBrowsePath.Size = new System.Drawing.Size(75, 27);
            this.btnBrowsePath.Text = "Browse...";
            this.btnBrowsePath.Click += new System.EventHandler(this.btnBrowsePath_Click);
            //
            // progressBar
            //
            this.progressBar.Location = new System.Drawing.Point(20, 470);
            this.progressBar.Size = new System.Drawing.Size(510, 22);
            this.progressBar.Visible = false;
            //
            // lblProgress
            //
            this.lblProgress.AutoSize = true;
            this.lblProgress.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProgress.Location = new System.Drawing.Point(20, 497);
            this.lblProgress.Text = "";
            this.lblProgress.Visible = false;
            //
            // btnInstall
            //
            this.btnInstall.BackColor = System.Drawing.Color.FromArgb(50, 150, 50);
            this.btnInstall.Enabled = false;
            this.btnInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstall.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnInstall.ForeColor = System.Drawing.Color.White;
            this.btnInstall.Location = new System.Drawing.Point(310, 525);
            this.btnInstall.Size = new System.Drawing.Size(120, 38);
            this.btnInstall.Text = "Install";
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            //
            // btnCancel
            //
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCancel.Location = new System.Drawing.Point(440, 525);
            this.btnCancel.Size = new System.Drawing.Size(90, 38);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // InstallerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 585);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.grpRole);
            this.Controls.Add(this.grpClientSettings);
            this.Controls.Add(this.grpPath);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "InstallerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EJLive Setup Wizard";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.grpRole.ResumeLayout(false);
            this.grpRole.PerformLayout();
            this.grpClientSettings.ResumeLayout(false);
            this.grpClientSettings.PerformLayout();
            this.grpPath.ResumeLayout(false);
            this.grpPath.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.GroupBox grpRole;
        private System.Windows.Forms.Button btnInstallClient;
        private System.Windows.Forms.Button btnInstallServer;
        private System.Windows.Forms.Label lblClientDesc;
        private System.Windows.Forms.Label lblServerDesc;
        private System.Windows.Forms.Label lblSelectedRole;
        private System.Windows.Forms.GroupBox grpClientSettings;
        private System.Windows.Forms.Label lblATMType;
        private System.Windows.Forms.ComboBox cmbATMType;
        private System.Windows.Forms.CheckBox chkAutoStartWithWindows;
        private System.Windows.Forms.Label lblJournalPath;
        private System.Windows.Forms.Label lblJournalPathValue;
        private System.Windows.Forms.Label lblBackupPath;
        private System.Windows.Forms.Label lblBackupPathValue;
        private System.Windows.Forms.GroupBox grpPath;
        private System.Windows.Forms.Label lblInstallPath;
        private System.Windows.Forms.TextBox txtInstallPath;
        private System.Windows.Forms.Button btnBrowsePath;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnCancel;

            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.grpRole = new System.Windows.Forms.GroupBox();
            this.btnInstallClient = new System.Windows.Forms.Button();
            this.btnInstallServer = new System.Windows.Forms.Button();
            this.lblClientDesc = new System.Windows.Forms.Label();
            this.lblServerDesc = new System.Windows.Forms.Label();
            this.lblSelectedRole = new System.Windows.Forms.Label();
            this.grpPath = new System.Windows.Forms.GroupBox();
            this.lblInstallPath = new System.Windows.Forms.Label();
            this.txtInstallPath = new System.Windows.Forms.TextBox();
            this.btnBrowsePath = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.btnInstall = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.grpRole.SuspendLayout();
            this.grpPath.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlHeader
            //
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(25, 25, 60);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblSubtitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Size = new System.Drawing.Size(550, 70);
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 10);
            this.lblTitle.Text = "EJLive Setup Wizard";
            //
            // lblSubtitle
            //
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.LightGray;
            this.lblSubtitle.Location = new System.Drawing.Point(22, 42);
            this.lblSubtitle.Text = "Electronic Journal Live Monitoring System v3.2.1";
            //
            // grpRole
            //
            this.grpRole.Controls.Add(this.btnInstallClient);
            this.grpRole.Controls.Add(this.btnInstallServer);
            this.grpRole.Controls.Add(this.lblClientDesc);
            this.grpRole.Controls.Add(this.lblServerDesc);
            this.grpRole.Controls.Add(this.lblSelectedRole);
            this.grpRole.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpRole.Location = new System.Drawing.Point(20, 85);
            this.grpRole.Size = new System.Drawing.Size(510, 170);
            this.grpRole.Text = "Select Installation Type";
            //
            // btnInstallClient
            //
            this.btnInstallClient.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            this.btnInstallClient.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstallClient.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnInstallClient.ForeColor = System.Drawing.Color.White;
            this.btnInstallClient.Location = new System.Drawing.Point(20, 35);
            this.btnInstallClient.Size = new System.Drawing.Size(220, 45);
            this.btnInstallClient.Text = "ATM Client";
            this.btnInstallClient.Click += new System.EventHandler(this.btnInstallClient_Click);
            //
            // btnInstallServer
            //
            this.btnInstallServer.BackColor = System.Drawing.Color.FromArgb(0, 150, 50);
            this.btnInstallServer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstallServer.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnInstallServer.ForeColor = System.Drawing.Color.White;
            this.btnInstallServer.Location = new System.Drawing.Point(270, 35);
            this.btnInstallServer.Size = new System.Drawing.Size(220, 45);
            this.btnInstallServer.Text = "Central Server";
            this.btnInstallServer.Click += new System.EventHandler(this.btnInstallServer_Click);
            //
            // lblClientDesc
            //
            this.lblClientDesc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblClientDesc.ForeColor = System.Drawing.Color.Gray;
            this.lblClientDesc.Location = new System.Drawing.Point(20, 85);
            this.lblClientDesc.Size = new System.Drawing.Size(220, 40);
            this.lblClientDesc.Text = "Install on ATM machines.\nSupports NCR, GRG, and WN.";
            //
            // lblServerDesc
            //
            this.lblServerDesc.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblServerDesc.ForeColor = System.Drawing.Color.Gray;
            this.lblServerDesc.Location = new System.Drawing.Point(270, 85);
            this.lblServerDesc.Size = new System.Drawing.Size(220, 40);
            this.lblServerDesc.Text = "Install on central server.\nMonitoring, archive, and control.";
            //
            // lblSelectedRole
            //
            this.lblSelectedRole.AutoSize = true;
            this.lblSelectedRole.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblSelectedRole.Location = new System.Drawing.Point(20, 135);
            this.lblSelectedRole.Text = "";
            //
            // grpPath
            //
            this.grpPath.Controls.Add(this.lblInstallPath);
            this.grpPath.Controls.Add(this.txtInstallPath);
            this.grpPath.Controls.Add(this.btnBrowsePath);
            this.grpPath.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.grpPath.Location = new System.Drawing.Point(20, 265);
            this.grpPath.Size = new System.Drawing.Size(510, 70);
            this.grpPath.Text = "Installation Path";
            //
            // lblInstallPath
            //
            this.lblInstallPath.AutoSize = true;
            this.lblInstallPath.Location = new System.Drawing.Point(15, 32);
            this.lblInstallPath.Text = "Path:";
            //
            // txtInstallPath
            //
            this.txtInstallPath.Location = new System.Drawing.Point(70, 29);
            this.txtInstallPath.Size = new System.Drawing.Size(340, 25);
            this.txtInstallPath.Text = "C:\\Program Files\\EJLive";
            //
            // btnBrowsePath
            //
            this.btnBrowsePath.Location = new System.Drawing.Point(420, 28);
            this.btnBrowsePath.Size = new System.Drawing.Size(75, 27);
            this.btnBrowsePath.Text = "Browse...";
            this.btnBrowsePath.Click += new System.EventHandler(this.btnBrowsePath_Click);
            //
            // progressBar
            //
            this.progressBar.Location = new System.Drawing.Point(20, 350);
            this.progressBar.Size = new System.Drawing.Size(510, 22);
            this.progressBar.Visible = false;
            //
            // lblProgress
            //
            this.lblProgress.AutoSize = true;
            this.lblProgress.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProgress.Location = new System.Drawing.Point(20, 377);
            this.lblProgress.Text = "";
            this.lblProgress.Visible = false;
            //
            // btnInstall
            //
            this.btnInstall.BackColor = System.Drawing.Color.FromArgb(50, 150, 50);
            this.btnInstall.Enabled = false;
            this.btnInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstall.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnInstall.ForeColor = System.Drawing.Color.White;
            this.btnInstall.Location = new System.Drawing.Point(310, 405);
            this.btnInstall.Size = new System.Drawing.Size(120, 38);
            this.btnInstall.Text = "Install";
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            //
            // btnCancel
            //
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCancel.Location = new System.Drawing.Point(440, 405);
            this.btnCancel.Size = new System.Drawing.Size(90, 38);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            //
            // InstallerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 460);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.grpRole);
            this.Controls.Add(this.grpPath);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.btnInstall);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "InstallerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EJLive Setup Wizard";
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.grpRole.ResumeLayout(false);
            this.grpRole.PerformLayout();
            this.grpPath.ResumeLayout(false);
            this.grpPath.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.GroupBox grpRole;
        private System.Windows.Forms.Button btnInstallClient;
        private System.Windows.Forms.Button btnInstallServer;
        private System.Windows.Forms.Label lblClientDesc;
        private System.Windows.Forms.Label lblServerDesc;
        private System.Windows.Forms.Label lblSelectedRole;
        private System.Windows.Forms.GroupBox grpPath;
        private System.Windows.Forms.Label lblInstallPath;
        private System.Windows.Forms.TextBox txtInstallPath;
        private System.Windows.Forms.Button btnBrowsePath;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnCancel;
    }
}
