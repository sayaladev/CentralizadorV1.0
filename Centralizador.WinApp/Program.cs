using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;
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


            string tokenSii = Models.ApiSII.TokenSeed.GETTokenFromSii();
            ResultAgent agent = Agent.GetAgetByEmail();         
            if (tokenSii != null && agent != null)
            {               
                Application.Run(new FormMain(tokenSii, agent));
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
