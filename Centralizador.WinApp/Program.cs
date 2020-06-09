using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.WinApp.GUI;

namespace Centralizador.WinApp
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            string tokenSii = ServiceSoap.GETTokenFromSii(Properties.Settings.Default.SerialDigitalCert);
            string tokenCen = Agent.GetTokenCen(Properties.Settings.Default.UserCEN, Properties.Settings.Default.PasswordCEN);
            ResultAgent agent = Agent.GetAgetByEmail(Properties.Settings.Default.UserCEN);         
            if (!string.IsNullOrEmpty(tokenSii) && agent != null && !string.IsNullOrEmpty(tokenCen))
            {               
                Application.Run(new FormMain(tokenSii, agent, tokenCen));
            }
            else
            {
                if (tokenSii == null)
                {
                    MessageBox.Show("Missing Sii Token. Please check the digital cert...", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (agent == null)
                {
                    MessageBox.Show($"The web service belonging to CEN is under maintenance.{Environment.NewLine}Impossible to start!", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

           


        }
    }
}
