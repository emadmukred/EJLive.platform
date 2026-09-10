using System;
using System.Windows.Forms;

namespace EJLive.Client.WinForms
{
    public partial class ClientMainForm
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                EJLive.Client.WinForms.Services.UIBinder.BindControlTexts(this, this.Name);
            }
            catch
            {
                // swallow to avoid breaking UI
            }
        }
    }
}
