using System;
  using System.Windows.Forms;
  using EJLive.Core.Services;

  namespace EJLive.Core.UI;

  /// <summary>
  /// Binds control text labels from <see cref="LabelMappingService"/> onto a WinForms control tree.
  /// Auto-wires any Button whose name contains "sync" to the active <see cref="IJournalSyncService"/>.
  /// Works for both Client and Server WinForms projects — import this one class from EJLive.Core.
  /// </summary>
  public static class UIBinder
  {
      /// <summary>
      /// Traverses all controls in <paramref name="root"/> and applies label mappings.
      /// </summary>
      /// <param name="root">The root control (usually a Form).</param>
      /// <param name="formName">
      ///   Logical form name used as the first segment of the label key (e.g., <c>formName.controlName</c>).
      ///   Defaults to <c>root.Name</c> when omitted.
      /// </param>
      public static void BindControlTexts(Control root, string? formName = null)
      {
          if (root is null) return;
          var labelService = ServiceRegistry.Get<LabelMappingService>();
          Traverse(root, string.IsNullOrWhiteSpace(formName) ? root.Name : formName, labelService);
      }

      // ── Private helpers ───────────────────────────────────────────────────────

      private static void Traverse(Control control, string formName, LabelMappingService? labelService)
      {
          if (control is null) return;

          if (labelService is not null)
          {
              var key = $"{formName}.{control.Name}";
              var txt = labelService.Get(key);
              if (string.IsNullOrEmpty(txt))
                  txt = labelService.Get(control.Name);   // fallback: bare control name
              if (!string.IsNullOrEmpty(txt))
                  control.Text = txt;
          }

          // Auto-wire any button whose name contains "sync" to the journal-sync service.
          if (control is Button btn &&
              btn.Name?.IndexOf("sync", StringComparison.OrdinalIgnoreCase) >= 0)
          {
              btn.Click -= SyncButton_Click;
              btn.Click += SyncButton_Click;
          }

          foreach (Control child in control.Controls)
              Traverse(child, formName, labelService);
      }

      private static void SyncButton_Click(object? sender, EventArgs e)
      {
          // Prefer the registered service; fall back to the global locator for legacy callers.
          var svc = ServiceRegistry.Get<IJournalSyncService>()
                    ?? ServiceLocator.GetJournalSyncService();
          svc?.StartSync();
      }
  }
  