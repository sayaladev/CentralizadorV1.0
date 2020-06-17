using System;
using System.Collections.Generic;
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

            IList<ResultParticipant> participants;
            IList<ResultBilingType> billingTypes;

            string tokenSii;
            string tokenCen;
            // Variables           
            try
            {
                tokenSii = ServiceSoap.GETTokenFromSii(Properties.Settings.Default.SerialDigitalCert);
                tokenCen = Agent.GetTokenCenAsync(Properties.Settings.Default.UserCEN, Properties.Settings.Default.PasswordCEN).Result;
                // Get Participants
                participants = Participant.GetParticipants(Properties.Settings.Default.UserCEN);
                // Get Biling types
                billingTypes = BilingType.GetBilinTypesAsync().Result;
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("Impossible to start the Application.", ex, MessageBoxIcon.Stop);
                return;
            }


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
                    new ErrorMsgCen("The token has not been obtained from SII", "Impossible to start the Application.", MessageBoxIcon.Stop);
                    return;
                }
                else if (string.IsNullOrEmpty(tokenCen) || participants == null || billingTypes == null)
                {
                    new ErrorMsgCen("The token has not been obtained from CEN", "Impossible to start the Application.", MessageBoxIcon.Stop);
                    return;
                }
                else if (participants.Count == 0)
                {
                    new ErrorMsgCen("No participants found", "Impossible to start the Application.", MessageBoxIcon.Stop);
                    return;
                }
                // Open Form
                Application.Run(new FormMain() { TokenCen = tokenCen, TokenSii = tokenSii, Participants = participants, BillingTypes = billingTypes });
            }
            mutex.ReleaseMutex();
        }
    }
}
