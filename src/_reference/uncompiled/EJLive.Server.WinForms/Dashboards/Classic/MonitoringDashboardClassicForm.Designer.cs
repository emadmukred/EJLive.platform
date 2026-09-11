#nullable disable

namespace EJLive.Server.WinForms.Dashboards.Classic
{
    partial class MonitoringDashboardClassicForm
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
            // MonitoringDashboardClassicForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1600, 950);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(1200, 700);
            this.Name = "MonitoringDashboardClassicForm";
            this.Text = "EJLive Enterprise - Monitoring Dashboard";
            this.ResumeLayout(false);
        }
    }
}

