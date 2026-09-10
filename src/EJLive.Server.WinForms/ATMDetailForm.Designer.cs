namespace EJLive.Server.WinForms
{
    partial class ATMDetailForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblAtmId = new System.Windows.Forms.Label();
            this.txtAtmId = new System.Windows.Forms.TextBox();
            this.lblAtmName = new System.Windows.Forms.Label();
            this.txtAtmName = new System.Windows.Forms.TextBox();
            this.lblAtmType = new System.Windows.Forms.Label();
            this.txtAtmType = new System.Windows.Forms.TextBox();
            this.lblLineage = new System.Windows.Forms.Label();
            this.txtLineage = new System.Windows.Forms.TextBox();
            this.rtbCapabilities = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            this.lblAtmId.AutoSize = true;
            this.lblAtmId.Location = new System.Drawing.Point(12, 15);
            this.lblAtmId.Text = "ATM ID:";
            this.txtAtmId.Location = new System.Drawing.Point(90, 12);
            this.txtAtmId.ReadOnly = true;
            this.txtAtmId.Size = new System.Drawing.Size(220, 23);
            this.lblAtmName.AutoSize = true;
            this.lblAtmName.Location = new System.Drawing.Point(330, 15);
            this.lblAtmName.Text = "ATM Name:";
            this.txtAtmName.Location = new System.Drawing.Point(410, 12);
            this.txtAtmName.ReadOnly = true;
            this.txtAtmName.Size = new System.Drawing.Size(220, 23);
            this.lblAtmType.AutoSize = true;
            this.lblAtmType.Location = new System.Drawing.Point(12, 49);
            this.lblAtmType.Text = "ATM Type:";
            this.txtAtmType.Location = new System.Drawing.Point(90, 46);
            this.txtAtmType.ReadOnly = true;
            this.txtAtmType.Size = new System.Drawing.Size(220, 23);
            this.lblLineage.AutoSize = true;
            this.lblLineage.Location = new System.Drawing.Point(330, 49);
            this.lblLineage.Text = "Platform Lineage:";
            this.txtLineage.Location = new System.Drawing.Point(430, 46);
            this.txtLineage.ReadOnly = true;
            this.txtLineage.Size = new System.Drawing.Size(200, 23);
            this.rtbCapabilities.BackColor = System.Drawing.Color.FromArgb(24, 24, 34);
            this.rtbCapabilities.Font = new System.Drawing.Font("Consolas", 9F);
            this.rtbCapabilities.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.rtbCapabilities.Location = new System.Drawing.Point(15, 86);
            this.rtbCapabilities.ReadOnly = true;
            this.rtbCapabilities.Size = new System.Drawing.Size(615, 280);
            this.rtbCapabilities.Text = "Capability details will appear here.";
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 382);
            this.Controls.Add(this.rtbCapabilities);
            this.Controls.Add(this.txtLineage);
            this.Controls.Add(this.lblLineage);
            this.Controls.Add(this.txtAtmType);
            this.Controls.Add(this.lblAtmType);
            this.Controls.Add(this.txtAtmName);
            this.Controls.Add(this.lblAtmName);
            this.Controls.Add(this.txtAtmId);
            this.Controls.Add(this.lblAtmId);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "ATMDetailForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ATM Details";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblAtmId;
        private System.Windows.Forms.TextBox txtAtmId;
        private System.Windows.Forms.Label lblAtmName;
        private System.Windows.Forms.TextBox txtAtmName;
        private System.Windows.Forms.Label lblAtmType;
        private System.Windows.Forms.TextBox txtAtmType;
        private System.Windows.Forms.Label lblLineage;
        private System.Windows.Forms.TextBox txtLineage;
        private System.Windows.Forms.RichTextBox rtbCapabilities;
    }
}
