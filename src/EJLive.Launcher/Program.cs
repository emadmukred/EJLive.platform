using System;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace UnifiedEJLiveProject
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check command line arguments to determine which mode to run
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "client":
                    case "-client":
                        RunClient();
                        return;
                    case "server":
                    case "-server":
                        RunServer();
                        return;
                    default:
                        ShowUsage();
                        return;
                }
            }
            else
            {
                // Show a dialog to choose between client and server
                ShowModeSelectionDialog();
            }
        }

        static void RunClient()
        {
            try
            {
                // Launch the client application using the ClientMainForm from EJLive.Client
                var clientForm = new EJLive.Client.WinForms.ClientMainForm();
                Application.Run(clientForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start client mode: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void RunServer()
        {
            try
            {
                // Launch the server application using the ServerMainForm from EJLive.Server
                var serverForm = new EJLive.Server.WinForms.ServerMainForm();
                Application.Run(serverForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start server mode: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void ShowModeSelectionDialog()
        {
            // Show a simple dialog to choose between client and server
            var result = MessageBox.Show(
                "What mode do you want to start?\n\n" +
                "Yes: Client Mode (ATM connection and processing)\n" +
                "No: Server Mode (Multi-ATM management and processing)\n\n" +
                "Select a mode:", 
                "EJLive Mode Selection", 
                MessageBoxButtons.YesNoCancel);

            switch (result)
            {
                case DialogResult.Yes:
                    RunClient();
                    break;
                case DialogResult.No:
                    RunServer();
                    break;
                default:
                    // Cancel or close
                    break;
            }
        }

        static void ShowUsage()
        {
            MessageBox.Show(
                "Usage: UnifiedEJLiveProject.exe [option]\n\n" +
                "Options:\n" +
                "  client      Run in client mode (ATM connection and processing)\n" +
                "  server      Run in server mode (Multi-ATM management and processing)\n\n" +
                "If no option is provided, a mode selection dialog will appear.", 
                "EJLive Usage");
        }
    }
}