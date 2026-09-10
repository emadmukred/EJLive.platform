using System;
using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Monitor
{
    public sealed class MonitoringDashboard : UserControl
    {
        public MonitoringDashboard()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(243, 247, 248);

            var label = new Label
            {
                Dock = DockStyle.Fill,
                Text = "EJLive monitor module ready" + Environment.NewLine + "Read-only operational dashboard component",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 77, 92)
            };

            Controls.Add(label);
        }
    }
}
