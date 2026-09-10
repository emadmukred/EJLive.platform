using System;
using System.Windows.Forms;

namespace EJLive.Server.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServerMainForm());
        }
    }
}
