namespace EJLive.Server.WinForms
{
    partial class SyncDashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvSync = new System.Windows.Forms.DataGridView();
            this.colAtmId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAtmName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAtmType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colConnected = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastHeartbeat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastSync = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPending = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFailed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCompleted = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPendingBytes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAlertCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rtbSyncDetails = new System.Windows.Forms.RichTextBox();
            this.lblDetails = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSync)).BeginInit();
            this.SuspendLayout();
            this.dgvSync.AllowUserToAddRows = false;
            this.dgvSync.AllowUserToDeleteRows = false;
            this.dgvSync.BackgroundColor = System.Drawing.Color.White;
            this.dgvSync.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSync.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.colAtmId, this.colAtmName, this.colAtmType, this.colConnected, this.colLastHeartbeat, this.colLastSync, this.colPending, this.colFailed, this.colCompleted, this.colPendingBytes, this.colAlertCount });
            this.dgvSync.Location = new System.Drawing.Point(12, 12);
            this.dgvSync.MultiSelect = false;
            this.dgvSync.Name = "dgvSync";
            this.dgvSync.ReadOnly = true;
            this.dgvSync.RowHeadersVisible = false;
            this.dgvSync.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSync.Size = new System.Drawing.Size(1160, 300);
            this.dgvSync.SelectionChanged += new System.EventHandler(this.dgvSync_SelectionChanged);
            this.colAtmId.HeaderText = "ATM ID";
            this.colAtmId.ReadOnly = true;
            this.colAtmName.HeaderText = "ATM Name";
            this.colAtmName.ReadOnly = true;
            this.colAtmName.Width = 130;
            this.colAtmType.HeaderText = "Type";
            this.colAtmType.ReadOnly = true;
            this.colConnected.HeaderText = "Connected";
            this.colConnected.ReadOnly = true;
            this.colLastHeartbeat.HeaderText = "Last Heartbeat";
            this.colLastHeartbeat.ReadOnly = true;
            this.colLastHeartbeat.Width = 150;
            this.colLastSync.HeaderText = "Last Sync";
            this.colLastSync.ReadOnly = true;
            this.colLastSync.Width = 150;
            this.colPending.HeaderText = "Pending";
            this.colPending.ReadOnly = true;
            this.colFailed.HeaderText = "Failed";
            this.colFailed.ReadOnly = true;
            this.colCompleted.HeaderText = "Completed";
            this.colCompleted.ReadOnly = true;
            this.colPendingBytes.HeaderText = "Pending Bytes";
            this.colPendingBytes.ReadOnly = true;
            this.colPendingBytes.Width = 110;
            this.colAlertCount.HeaderText = "Alerts";
            this.colAlertCount.ReadOnly = true;
            this.lblDetails.AutoSize = true;
            this.lblDetails.Location = new System.Drawing.Point(12, 323);
            this.lblDetails.Text = "ATM details:";
            this.rtbSyncDetails.BackColor = System.Drawing.Color.FromArgb(24, 24, 34);
            this.rtbSyncDetails.Font = new System.Drawing.Font("Consolas", 9F);
            this.rtbSyncDetails.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.rtbSyncDetails.Location = new System.Drawing.Point(12, 342);
            this.rtbSyncDetails.ReadOnly = true;
            this.rtbSyncDetails.Size = new System.Drawing.Size(1160, 176);
            this.rtbSyncDetails.Text = "Select an ATM row to inspect its sync state and alerts.";
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 531);
            this.Controls.Add(this.lblDetails);
            this.Controls.Add(this.rtbSyncDetails);
            this.Controls.Add(this.dgvSync);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "SyncDashboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Journal Sync Dashboard";
            ((System.ComponentModel.ISupportInitialize)(this.dgvSync)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvSync;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAtmId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAtmName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAtmType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colConnected;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastHeartbeat;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastSync;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPending;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFailed;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCompleted;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPendingBytes;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAlertCount;
        private System.Windows.Forms.RichTextBox rtbSyncDetails;
        private System.Windows.Forms.Label lblDetails;
    }
}
