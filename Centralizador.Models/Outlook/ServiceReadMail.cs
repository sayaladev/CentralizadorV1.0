using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

using Centralizador.Models.AppFunctions;
using Centralizador.Models.registroreclamodteservice;

using EAGetMail;


namespace Centralizador.Models.Outlook
{
    /// <summary>
    /// Get EMail with "EAGetMail" Tool.
    /// https://www.emailarchitect.net/eagetmail/sdk/
    /// 
    /// Config IMAP
    /// Nombre de servidor: outlook.office365.com
    /// Puerto: 993
    /// Método de cifrado: TLS
    /// 
    /// 
    /// </summary>
    public class ServiceReadMail
    {
        public string TokenSii { get; set; }
        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");

        public ServiceReadMail(string tokenSii)
        {
            TokenSii = tokenSii;
        }

        public void GetXmlFromEmail(BackgroundWorker BgwReadEmail)
        {
            BgwReadEmail.DoWork += BgwReadEmail_DoWork;
            BgwReadEmail.RunWorkerAsync();        
        }

        private void BgwReadEmail_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgw = sender as BackgroundWorker;
            int c = 0;
            string pathTemp = @"C:\Centralizador\Temp\";
            new CreateFile(pathTemp);          
            MailServer oServer = new MailServer("outlook.office365.com", Properties.Settings.Default.UserEmail, Properties.Settings.Default.UserPassword, ServerProtocol.Imap4)
            {
                SSLConnection = true,
                Port = 993
            };
            MailClient oClient = new MailClient("EG-C1508812802-00376-7E2448B3BDAEEB7D-338FF6UAA8257EB7");
            try
            {
                //bgw.ReportProgress(0, "Connecting to the mail server...");
                oClient.Connect(oServer);
                oClient.GetMailInfosParam.Reset();
                oClient.GetMailInfosParam.GetMailInfosOptions |= GetMailInfosOptionType.UIDRange;
                oClient.GetMailInfosParam.UIDRange = $"{Properties.Settings.Default.UIDRange}:*";


                Imap4Folder[] imap4Folders = oClient.GetFolders();
                //oClient.SelectFolder(imap4Folders[1]); // spam

                foreach (Imap4Folder item in imap4Folders)
                {
                    if (item.Name == "INBOX" || item.Name == "Correo no deseado")
                    {
                        oClient.SelectFolder(item);
                        MailInfo[] infos = oClient.GetMailInfos();
                        e.Result = Properties.Settings.Default.DateTimeEmail;
                        for (int i = 0; i < infos.Length; i++)
                        {
                            MailInfo info = infos[i];
                            Mail oMail = new Mail("EG-C1508812802-00376-7E2448B3BDAEEB7D-338FF6UAA8257EB7");
                            oMail = oClient.GetMail(info);
                            if (oMail.From.Address == Properties.Settings.Default.UserEmail)
                            {
                                continue;
                            }
                            Attachment[] atts = oMail.Attachments;
                            foreach (Attachment att in atts)
                            {
                                if (att.Name.Contains(".xml"))
                                {
                                    att.SaveAs(pathTemp + att.Name, true);
                                    try
                                    {
                                        XDocument xmlDocument;
                                        using (StreamReader oReader = new StreamReader(pathTemp + att.Name, Encoding.GetEncoding("ISO-8859-1")))
                                        {
                                            xmlDocument = XDocument.Load(oReader);
                                        }
                                        if (xmlDocument.Root.Name.LocalName == "EnvioDTE")
                                        {
                                            int res = SaveFiles(pathTemp + att.Name);
                                            switch (res)
                                            {
                                                case 0: // Success
                                                    continue;
                                                case 1: // Error in Sii (exit funcion)
                                                    MessageBox.Show("Sii: Application with Momentary Suspension", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                    return;
                                                case 2: // Error in serialization
                                                    continue;
                                                case 3: // Error in query in Sii: folio wrong example.
                                                    break;

                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    catch (System.Xml.XmlException)
                                    {
                                        // Nothing for error in "xmlDocument = XDocument.Load(oReader);"
                                    }
                                }
                            }
                            if (item.Name == "INBOX")
                            {
                                e.Result = oMail.ReceivedDate;
                                Properties.Settings.Default.DateTimeEmail = oMail.ReceivedDate;
                                Properties.Settings.Default.UIDRange = info.UIDL;
                                c++;
                                float porcent = (float)(100 * c) / infos.Length;
                                bgw.ReportProgress((int)porcent, $"Dowloading messages... [{string.Format(CultureInfo, "{0:g}", oMail.ReceivedDate)}] ({c}/{infos.Length})  Subject: {oMail.Subject} ");
                            }

                            // CANCEL
                            if (bgw.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }


                        }
                    }
                }
            }
            catch (Exception)
            {
                //if (ex.ErrorCode == 0)
                //{

                //}
                throw;
            }
            finally
            {
                oClient.Close();
                SaveParam();
            }
        }

        public static void SaveParam()
        {
            Properties.Settings.Default.Save();
        }
        private int SaveFiles(string path)
        {
            string response = "";
            string nameFolder;
            string nameFile;
            // Deserialize
            EnvioDTE xmlObjeto = ServicePdf.TransformXmlEnvioDTEToObject(path);
            if (xmlObjeto != null)
            {
                foreach (DTEDefType dte in xmlObjeto.SetDTE.DTE)
                {
                    DTEDefTypeDocumento document = (DTEDefTypeDocumento)dte.Item;
                    string[] emisor = document.Encabezado.Emisor.RUTEmisor.Split('-');
                    // Only process TipoDTE 33 & 34.
                    if (document.Encabezado.IdDoc.TipoDTE == global::DTEType.Item33 || document.Encabezado.IdDoc.TipoDTE == global::DTEType.Item34)
                    {
                        // Remove zeros at left 
                        document.Encabezado.IdDoc.Folio = document.Encabezado.IdDoc.Folio.TrimStart(new char[] { '0' });
                        try
                        {
                            using (RegistroReclamoDteServiceEndpointService dateTimeDte = new RegistroReclamoDteServiceEndpointService(TokenSii))
                            {
                                response = dateTimeDte.consultarFechaRecepcionSii(emisor.GetValue(0).ToString(),
                                emisor.GetValue(1).ToString(),
                                Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString(),
                                document.Encabezado.IdDoc.Folio);
                            }
                        }
                        catch (Exception)
                        {
                            // Error in Sii
                            return 1;
                        }

                        if (response.Length != 0)
                        {
                            DateTime timeResponse = DateTime.Parse(string.Format(CultureInfo, "{0:D}", response));
                            // 2020\06\76470581-5
                            nameFolder = timeResponse.Year + @"\" + timeResponse.Month + @"\" + document.Encabezado.Receptor.RUTRecep;

                            // 15357870_33_8888
                            nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString() + "__" + document.Encabezado.IdDoc.Folio;
                            Save(nameFolder, nameFile, dte);
                            return 0;
                        }
                        else
                        {
                            // Errors. // dejar carpeta folder en 2020/5
                            nameFolder = document.Encabezado.IdDoc.FchEmis.Year + @"\" + document.Encabezado.IdDoc.FchEmis.Month;
                            nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString() + "__" + document.Encabezado.IdDoc.Folio;
                            Save(nameFolder + @"\Errors\", nameFile, dte);
                            return 3;
                        }
                    }
                }
            }
            else
            {
                // Serialialization failed
                return 2;
            }
            return 0;
        }
        private void Save(string nameFolder, string nameFile, DTEDefType dte)
        {
            try
            {
                string path = @"C:\Centralizador\Inbox\" + nameFolder;
                new CreateFile(path);
                File.WriteAllText(path + @"\" + nameFile + ".xml", ServicePdf.TransformObjectToXml(dte));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DateTime GetLastDateTime()
        {
            return Properties.Settings.Default.DateTimeEmail;
        }
    }
}


