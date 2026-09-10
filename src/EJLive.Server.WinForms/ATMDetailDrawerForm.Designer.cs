namespace EJLive.Server.WinForms
{
    partial class ATMDetailDrawerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ClientSize = new System.Drawing.Size(500, 600);
            this.Text = "تفاصيل الصراف";
            this.BackColor = System.Drawing.Color.FromArgb(26, 28, 30);
            this.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        }
    }
}
