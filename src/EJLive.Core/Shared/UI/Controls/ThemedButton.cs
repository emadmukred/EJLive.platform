using System;
using System.Drawing;
using System.Windows.Forms;

namespace EJLive.Shared.UI.Controls
{
    /// <summary>
    /// A themed button control with consistent styling across EJLive applications
    /// </summary>
    public class ThemedButton : Button
    {
        private Color _primaryColor = Color.FromArgb(25, 118, 210);
        private Color _accentColor = Color.FromArgb(33, 150, 243);
        private Color _hoverColor = Color.FromArgb(30, 144, 255);
        private Color _clickColor = Color.FromArgb(21, 101, 192);
        
        public ThemedButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = _primaryColor;
            this.ForeColor = Color.White;
            this.Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold);
            this.Size = new Size(100, 30);
        }
        
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.BackColor = _hoverColor;
        }
        
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.BackColor = _primaryColor;
        }
        
        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            this.BackColor = _clickColor;
        }
        
        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            this.BackColor = _hoverColor;
        }
        
        /// <summary>
        /// Gets or sets the primary color of the button
        /// </summary>
        public Color PrimaryColor
        {
            get { return _primaryColor; }
            set
            {
                _primaryColor = value;
                this.Invalidate();
            }
        }
        
        /// <summary>
        /// Gets or sets the accent color of the button
        /// </summary>
        public Color AccentColor
        {
            get { return _accentColor; }
            set
            {
                _accentColor = value;
                this.Invalidate();
            }
        }
        
        /// <summary>
        /// Gets or sets the hover color of the button
        /// </summary>
        public Color HoverColor
        {
            get { return _hoverColor; }
            set
            {
                _hoverColor = value;
                this.Invalidate();
            }
        }
        
        /// <summary>
        /// Gets or sets the click color of the button
        /// </summary>
        public Color ClickColor
        {
            get { return _clickColor; }
            set
            {
                _clickColor = value;
                this.Invalidate();
            }
        }
    }
}