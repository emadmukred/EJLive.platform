using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EJLive.Server.WinForms.Services
{
    public static class UIBinder
    {
        public static void BindControlTexts(Control root, string formName = null)
        {
            if (root == null) return;
            var labelService = EJLive.Client.WinForms.Services.ServiceRegistry.Get<EJLive.Client.WinForms.Services.LabelMappingService>();
            Traverse(root, formName ?? root.Name, labelService);
        }

        private static void Traverse(Control control, string formName, EJLive.Client.WinForms.Services.LabelMappingService labelService)
        {
            if (control == null) return;

            var key = $"{formName}.{control.Name}";
            if (labelService != null)
            {
                var txt = labelService.Get(key);
                if (!string.IsNullOrEmpty(txt))
                    control.Text = txt;
                else
                {
                    txt = labelService.Get(control.Name);
                    if (!string.IsNullOrEmpty(txt))
                        control.Text = txt;
                }
            }

            if (control is Button btn)
            {
                if (btn.Name?.IndexOf("sync", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    btn.Click -= SyncButton_Click;
                    btn.Click += SyncButton_Click;
                }
            }

            foreach (Control child in control.Controls)
                Traverse(child, formName, labelService);
        }

        private static void SyncButton_Click(object sender, EventArgs e)
        {
            var svc = EJLive.Core.Services.ServiceLocator.GetJournalSyncService();
            svc?.StartSync();
        }
    }
}
