namespace EJLive.Client.WinForms
{
    partial class ClientMainForm
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
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Text = "EJLive Enterprise Client v4.0.0";
            this.BackColor = System.Drawing.Color.FromArgb(26, 28, 30);
            this.ForeColor = System.Drawing.Color.FromArgb(200, 200, 200);
        }
    }
}
