using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.WinApp.GUI;

namespace Centralizador.WinApp
{
    /// <summary>
    /// Internal Class Init
    /// </summary>
    internal static class Program
    {

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            // Variables 
            IList<ResultBilingType> billingTypes;
            IList<ResultParticipant> participants;
            string tokenSii;
            string tokenCen;

            try
            {
                tokenSii = ServiceSoap.GETTokenFromSii(Properties.Settings.Default.SerialDigitalCert);
                // Get Participants
                participants = Participant.GetParticipants(Properties.Settings.Default.UserCEN);
                // Get Biling types
                billingTypes = BilingType.GetBilinTypesAsync().Result;

                tokenCen = Agent.GetTokenCenAsync(Properties.Settings.Default.UserCEN, Properties.Settings.Default.PasswordCEN).Result;

            }
            catch (Exception ex)
            {
                new ErrorMsgCen("Impossible to start the Application.", ex, MessageBoxIcon.Stop);
                return;
            }

            //Tester
            //tokenSii = "RWARM2S4NX7MD";

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
                    new ErrorMsgCen("The token has not been obtained from SII.", "Impossible to start the Application.", MessageBoxIcon.Stop);
                    return;
                }
                else if (string.IsNullOrEmpty(tokenCen) || participants == null || billingTypes == null)
                {
                    new ErrorMsgCen("The token has not been obtained from CEN.", "Impossible to start the Application.", MessageBoxIcon.Stop);
                    return;
                }
                else if (participants.Count == 0)
                {
                    new ErrorMsgCen("No participants found...", "Impossible to start the Application.", MessageBoxIcon.Stop);
                    return;
                }
                // Open Form
                FormMain main = new FormMain() { TokenCen = tokenCen, TokenSii = tokenSii, Participants = participants, BillingTypes = billingTypes };
                main.WindowState = FormWindowState.Normal;
                main.BringToFront();
                //main.TopMost = true;
                main.Focus();
                Application.Run(main);
            }
            mutex.ReleaseMutex();


            // https://docs.microsoft.com/es-es/visualstudio/deployment/how-to-configure-the-clickonce-trust-prompt-behavior?view=vs-2019
            //Microsoft.Win32.RegistryKey key;
            //key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey("SOFTWARE\\MICROSOFT\\.NETFramework\\Security\\TrustManager\\PromptingLevel");
            //key.SetValue("MyComputer", "Enabled");
            //key.SetValue("LocalIntranet", "Enabled");
            //key.SetValue("Internet", "AuthenticodeRequired");
            //key.SetValue("TrustedSites", "Enabled");
            //key.SetValue("UntrustedSites", "Disabled");
            //key.Close();
        }


    }
}
