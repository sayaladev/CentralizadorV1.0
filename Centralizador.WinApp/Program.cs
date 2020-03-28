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
            //ACÁ DEBO COMPROBAR VARIABLES DE INICIO...
            //COMPROBAR ESTADO DE DB SOFTLAND POR EJEMPLO

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
