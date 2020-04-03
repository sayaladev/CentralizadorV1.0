﻿using System;
using System.IO;
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

            // Chek file dte Sii
            string file = $"ce_empresas_dwnld_{string.Format("{0:yyyyMMdd}", DateTime.Today)}.csv";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + file;
            if (!File.Exists(path))
            {
                MessageBox.Show($"Missing Sii file '{file}', Please download for continue...", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
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
}
