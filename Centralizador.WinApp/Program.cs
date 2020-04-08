using System;
using System.Windows.Forms;

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
            if (tokenSii != null)
            {
                Application.Run(new FormMain(tokenSii));
            }
            else
            {
                MessageBox.Show("Missing Sii Token. Please check the digital cert...", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }
    }
}
