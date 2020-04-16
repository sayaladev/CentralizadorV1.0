using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

using Centralizador.Models.AppFunctions;
using Centralizador.Models.registroreclamodteservice;

using EAGetMail;

namespace Centralizador.Models.Outlook
{
    public class ServiceOutlook
    {
        //public DateTime LastTime { get; set; }

        private string TokenSii { get; set; }

        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");

        public ServiceOutlook(string tokenSii)
        {
            //LastTime = lastTime;
            TokenSii = tokenSii;
        }

        public void GetXmlFromEmail(BackgroundWorker bgw)
        {
            bgw.DoWork += Bgw_DoWork;
            bgw.RunWorkerAsync();

        }

        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            IList<Mail> mails = new List<Mail>();
            int c = 0;
            float porcent = 0;


            string userName = Properties.Settings.Default.UserEmail;
            string password = Properties.Settings.Default.UserPassword;
            MailServer oServer = new MailServer("outlook.office365.com", userName, password, ServerProtocol.Imap4)
            {
                SSLConnection = true,
                Port = 993
            };
            try
            {
                worker.ReportProgress(0, "Connecting with the email server.");
                MailClient oClient = new MailClient("EG-C1508812802-00376-7E2448B3BDAEEB7D-338FF6UAA8257EB7");
                oClient.Connect(oServer);
                worker.ReportProgress(0, "Connecting with the email server.");
                MailInfo[] infos = oClient.GetMailInfos();
                Thread.Sleep(500);

                int len = infos.Length;
                long Uid = Properties.Settings.Default.UIdEmail;
                for (int i = len - 1; i > 0; i--)
                {
                    MailInfo info = infos[i];
                    if (Convert.ToInt64(info.UIDL) > Uid)
                    {
                        Mail oMail = new Mail("EG-C1508812802-00376-7E2448B3BDAEEB7D-338FF6UAA8257EB7");
                        oMail = oClient.GetMail(info);
                        mails.Add(oMail);
                        if (i == len - 1)
                        {
                            e.Result = oMail.ReceivedDate;
                            Properties.Settings.Default.DateTimeEmail = oMail.ReceivedDate;
                            Properties.Settings.Default.UIdEmail = Convert.ToInt64(info.UIDL);
                        }
                        c++;
                        worker.ReportProgress(0, $"Dowloading email from server... [{c}][{string.Format(CultureInfo, "{0:g}", oMail.ReceivedDate)}]");
                    }
                    else
                    {
                        e.Result = Properties.Settings.Default.DateTimeEmail;
                        break;
                    }

                }
                oClient.Close();
                if (mails.Count == 0)
                {
                    worker.ReportProgress(0, "There are no emails to download.");
                    Thread.Sleep(500);
                    return;
                }
                c = 0;
                string pathTemp = Directory.GetCurrentDirectory() + @"\temp";
                if (!Directory.Exists(pathTemp))
                {
                    Directory.CreateDirectory(pathTemp);
                }

                foreach (Mail item in mails)
                {
                    Attachment[] atts = item.Attachments;
                    foreach (Attachment att in atts)
                    {
                        if (att.Name.Contains(".xml"))
                        {
                            att.SaveAs(pathTemp + @"\" + att.Name, true);
                            try
                            {
                                XDocument xmlDocumnet = XDocument.Load(pathTemp + @"\" + att.Name);
                                // Save file                          
                                SaveFiles(xmlDocumnet, pathTemp + @"\" + att.Name);
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message == "Se excedió el tiempo de espera de la operación")
                                {
                                    MessageBox.Show("Sii: Application with Momentary Suspension", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                                continue;
                            }
                        }

                    }
                    c++;
                    porcent = (float)(100 * c) / mails.Count;
                    worker.ReportProgress((int)porcent, $"Processing Xml files... [{item.ReceivedDate}] ({c}/{mails.Count})");
                }
                Thread.Sleep(500);
                worker.ReportProgress((int)porcent, $"Processing Xml files... [finished]");
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {



            }

        }

        private void SaveFiles(XDocument xmlDocumnet, string path)
        {
            if (xmlDocumnet.Root.Name.LocalName == "EnvioDTE")
            {
                // Deserialize
                EnvioDTE xmlObjeto = ServicePdf.TransformXmlToObject(path);
                foreach (DTEDefType dte in xmlObjeto.SetDTE.DTE)
                {
                    DTEDefTypeDocumento document = (DTEDefTypeDocumento)dte.Item;
                    string[] emisor = document.Encabezado.Emisor.RUTEmisor.Split('-');
                    string response;
                    using (RegistroReclamoDteServiceEndpointService dateTimeDte = new RegistroReclamoDteServiceEndpointService(TokenSii))
                    {
                        response = dateTimeDte.consultarFechaRecepcionSii(emisor.GetValue(0).ToString(),
                        emisor.GetValue(1).ToString(),
                        Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString(),
                        document.Encabezado.IdDoc.Folio);
                    }
                    if (response.Length != 0)
                    {
                        DateTime timeResponse = DateTime.Parse(string.Format(CultureInfo, "{0:D}", response));
                        string nameFolder = timeResponse.Year + @"\" + timeResponse.Month + @"\" + document.Encabezado.Receptor.RUTRecep;
                        string nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + document.Encabezado.IdDoc.Folio;
                        if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\inbox\" + nameFolder))
                        {
                            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\inbox\" + nameFolder);
                        }
                        ;
                        File.WriteAllText(Directory.GetCurrentDirectory() + @"\inbox\" + nameFolder + @"\" + nameFile + ".xml", ServicePdf.TransformObjectToXml(dte));
                    }
                    else
                    {
                        string nameFolder = document.Encabezado.IdDoc.FchEmis.Year + @"\" + document.Encabezado.IdDoc.FchEmis.Month + @"\" + document.Encabezado.Receptor.RUTRecep;
                        string nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + document.Encabezado.IdDoc.Folio;
                        if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\inbox\" + nameFolder + @"\no_date"))
                        {
                            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\inbox\" + nameFolder + @"\no_date");
                        }
                        File.WriteAllText(Directory.GetCurrentDirectory() + @"\inbox\" + nameFolder + @"\no_date\" + nameFile + ".xml", ServicePdf.TransformObjectToXml(dte));
                    }
                }
            }
        }



    }


}


