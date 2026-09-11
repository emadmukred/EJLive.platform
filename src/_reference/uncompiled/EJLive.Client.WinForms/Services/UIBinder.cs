using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EJLive.Core.Services;

namespace EJLive.Client.WinForms.Services
{
    public static class UIBinder
    {
        public static void BindControlTexts(Control root, string? formName = null)
        {
            if (root == null) return;
            var labelService = ServiceRegistry.Get<LabelMappingService>();
            Traverse(root, string.IsNullOrWhiteSpace(formName) ? root.Name : formName, labelService);
        }

        private static void Traverse(Control control, string formName, LabelMappingService? labelService)
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
                    // fallback: try control.Name directly
                    txt = labelService.Get(control.Name);
                    if (!string.IsNullOrEmpty(txt))
                        control.Text = txt;
                }
            }

            // auto bind sync buttons: if name contains 'sync' (case-insensitive), attach handler
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

        private static void SyncButton_Click(object? sender, EventArgs e)
        {
            var service = ServiceRegistry.Get<IJournalSyncService>() ?? ServiceLocator.GetJournalSyncService();
            service?.StartSync();
        }
    }
}
