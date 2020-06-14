using System;
using System.Threading;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.WinApp.GUI;

namespace Centralizador.WinApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Variables
            string tokenSii = ServiceSoap.GETTokenFromSii(Properties.Settings.Default.SerialDigitalCert);
            string tokenCen = Agent.GetTokenCenAsync(Properties.Settings.Default.UserCEN, Properties.Settings.Default.PasswordCEN);
            ResultAgent agent = Agent.GetAgetByEmailAsync(Properties.Settings.Default.UserCEN);

            // Prevent to open twice the form
            Mutex mutex = new Mutex(true, "FormMain", out bool active);
            if (!active)
            {
                Application.Exit();
            }
            else
            {
                // Checking
                if (string.IsNullOrEmpty(tokenSii))
                {
                    MessageBox.Show("Missing Sii Token. Please check the digital cert...", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (agent == null || string.IsNullOrEmpty(tokenCen))
                {
                    MessageBox.Show($"The web service belonging to CEN is under maintenance.{Environment.NewLine}Impossible to start!", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Open Form
                Application.Run(new FormMain(tokenSii, agent, tokenCen));
            }
            mutex.ReleaseMutex();
        }
    }
}
