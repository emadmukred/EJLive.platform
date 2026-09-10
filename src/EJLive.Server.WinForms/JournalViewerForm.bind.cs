using System;
using System.Windows.Forms;

namespace EJLive.Server.WinForms
{
    public partial class JournalViewerForm
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                EJLive.Server.WinForms.Services.UIBinder.BindControlTexts(this, this.Name);
            }
            catch
            {
            }
        }
    }
}
