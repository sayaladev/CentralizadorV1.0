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
        private static string VersionApp { get; set; }

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            // Variables           
            string tokenSii = ServiceSoap.GETTokenFromSii(Properties.Settings.Default.SerialDigitalCert);
            string tokenCen = Agent.GetTokenCenAsync().Result;
            // Get Participants
            IList<ResultParticipant> participants = Participant.GetParticipants();

            // Get Biling types
            IList<ResultBilingType> billingTypes = BilingType.GetBilinTypesAsync().Result;

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
                    MessageBox.Show($"Missing Sii Token. Please check the digital cert.{Environment.NewLine}Impossible to start!", Application.CompanyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (string.IsNullOrEmpty(tokenCen) || participants == null || billingTypes == null)
                {
                    MessageBox.Show($"The web service belonging to CEN is under maintenance.{Environment.NewLine}Impossible to start!", Application.CompanyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (participants.Count == 0)
                {
                    MessageBox.Show($"No participants found.{Environment.NewLine}Impossible to start!", Application.CompanyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Open Form
                Application.Run(new FormMain() { TokenCen = tokenCen, TokenSii = tokenSii, Participants = participants, BillingTypes = billingTypes });
            }
            mutex.ReleaseMutex();
        }
    }
}
