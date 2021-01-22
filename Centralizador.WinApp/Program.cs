using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Centralizador.Models;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.Outlook.MailKit;
using Centralizador.WinApp.GUI;

namespace Centralizador.WinApp
{
    /// <summary>
    /// Internal Class Init
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static async Task Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // VARIABLES.
            List<ResultParticipant> participants;
            string tokenSii;
            string tokenCen;

            try
            {
                // TESTER
                //XDocument docc = XDocument.Load(@"C:\Centralizador\Centralizador_config.xml");
                //string SerialNumberr = docc.Root.Element("CertificadoDigital").Element("SerialNumber").Value;
                //ServiceSoap ss = new ServiceSoap(SerialNumberr);
                //tokenSii = ss.GETTokenFromSii();
                //XDocument xDocument = XDocument.Load(@"C:\Centralizador\Temp\33_76026828-3_138024_20210107_0b07bb3d-6463-443b-b7ab-40cb36c021af.xml");
                //if (xDocument.Root.Name.LocalName == "EnvioDTE")
                //{
                //    ReadEmailFrom readEmail = new ReadEmailFrom(tokenSii, new Progress<HPgModel>());
                //    int res = readEmail.SaveFiles(xDocument);
                //    if (res == 0)
                //    {
                //        // ok
                //    }
                //    else if (res == 1)
                //    {
                //        //MessageBox.Show("Error", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //        // ERROR SII.
                //    }
                //}

                // LOAD XML CONFIG
                // LOAD THE XML CONFIG
                XDocument doc = XDocument.Load(@"C:\Centralizador\Centralizador_config.xml");
                string UserCen = doc.Root.Element("CEN").Element("UserCen").Value;
                string PasswordCeN = doc.Root.Element("CEN").Element("PasswordCen").Value;
                Uri UrlCen = new Uri(doc.Root.Element("CEN").Element("UrlCen").Value);
                //CERT
                string SerialNumber = doc.Root.Element("CertificadoDigital").Element("SerialNumber").Value;
                ServiceSoap s = new ServiceSoap(SerialNumber);
                tokenSii = s.GETTokenFromSii();
                // GET PARTICIPANTS CEN.
                participants = await Participant.GetParticipants(UserCen, UrlCen);
                // GET TOKEN CEN.
                tokenCen = await Agent.GetTokenCenAsync(UserCen, PasswordCeN, UrlCen);
                // PREVENT OPEN 2 FORM.
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
                        new Models.ErrorMsgCen("The token has not been obtained from SII.", "Impossible to start the Application.", MessageBoxIcon.Stop);
                        return;
                    }
                    else if (string.IsNullOrEmpty(tokenCen) || participants == null)
                    {
                        new Models.ErrorMsgCen("The token has not been obtained from CEN.", "Impossible to start the Application.", MessageBoxIcon.Stop);
                        return;
                    }
                    else if (participants.Count == 0)
                    {
                        new Models.ErrorMsgCen("No participants found...", "Impossible to start the Application.", MessageBoxIcon.Stop);
                        return;
                    }
                    // OPEN MAIN FORM.

                    FormMain main = new FormMain(tokenCen, tokenSii, participants)
                    {
                        WindowState = FormWindowState.Normal
                    };
                    main.BringToFront();
                    //main.TopMost = true;
                    main.Focus();
                    Application.Run(main);
                }
                mutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                new Models.ErrorMsgCen("Impossible to start the Application.", ex, MessageBoxIcon.Stop);
                return;
            }
        }
    }
}