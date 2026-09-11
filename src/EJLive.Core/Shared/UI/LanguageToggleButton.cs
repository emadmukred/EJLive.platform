// ============================================================================
//  LanguageToggleButton.cs - Bilingual Language Switching Button
//  Provides a simple toggle button in the status strip for language switching.
// ============================================================================

using System;
using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Shared.UI
{
    /// <summary>
    /// A tool strip button that allows users to switch between Arabic and English.
    /// Shows the opposing language label and includes a globe icon indicator.
    /// </summary>
    public class LanguageToggleButton : ToolStripButton
    {
        private bool _isUpdating;

        public LanguageToggleButton()
        {
            this.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.Alignment = ToolStripItemAlignment.Right;
            this.Margin = new Padding(8, 1, 8, 1);
            UpdateDisplay();
            this.Click += OnToggleClick;

            // Subscribe to language changes to update display
            LanguageManager.LanguageChanged += OnLanguageChanged;
        }

        private void OnToggleClick(object sender, EventArgs e)
        {
            LanguageManager.ToggleLanguage();
        }

        private void OnLanguageChanged(bool isArabic)
        {
            if (_isUpdating) return;
            _isUpdating = true;
            try
            {
                UpdateDisplay();
                // Apply direction and texts to parent form
                if (this.GetCurrentParent() is ToolStrip strip && strip.Parent is Form form)
                {
                    LanguageManager.ApplyDirection(form, isArabic);
                    LanguageManager.ApplyLocalizedTexts(form);
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateDisplay()
        {
            // Show the OTHER language (what user can switch to)
            if (LanguageManager.IsArabic)
            {
                this.Text = "🌐 English";
                this.ToolTipText = "Switch to English / التبديل إلى الإنجليزية";
            }
            else
            {
                this.Text = "🌐 العربية";
                this.ToolTipText = "Switch to Arabic / التبديل إلى العربية";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LanguageManager.LanguageChanged -= OnLanguageChanged;
            }
            base.Dispose(disposing);
        }
    }
}