namespace EJLive.Monitor
{
    partial class MonitoringDashboard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();
            // 
            // MonitoringDashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 950);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(1200, 700);
            this.Name = "MonitoringDashboard";
            this.Text = "EJLive Enterprise - Monitoring Dashboard";
            this.ResumeLayout(false);
        }
    }
}
