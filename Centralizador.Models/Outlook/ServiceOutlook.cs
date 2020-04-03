using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;

using Centralizador.Models.registroreclamodteservice;

using EAGetMail;

namespace Centralizador.Models.Outlook
{
    public class ServiceOutlook
    {

        public DateTime LastTime { get; set; }

        public string TokenSii { get; set; }

        private MailInfo[] Infos { get; set; }

        private MailClient Client { get; set; }

        private IList<Mail> ListMail { get; set; }

        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");



        public void GetXmlFromEmail(BackgroundWorker bgw)
        {   // The mail must be delete from 'inbox' folder, because this loop is for all folder.
            // In folder there are all types of mails. (EvioDTE / EnvioRecibo / etc.)
            bgw.DoWork += Bgw_DoWork;
            Client = new MailClient("TryIt");
            try
            {
                MailServer oServer = new MailServer("outlook.office365.com", "facturacionchile@capvertenergie.com", "Che@2019!", ServerProtocol.Imap4)
                //MailServer oServer = new MailServer("outlook.office365.com", "sergiokml@outlook.com", "edkdbigryqfudzlv", ServerProtocol.Imap4)
                {
                    AuthType = ServerAuthType.AuthLogin,
                    SSLConnection = true,
                    Port = 993
                };
                bgw.ReportProgress(0, $"Working in connect to Outlook remote...");
                Client.Connect(oServer);
                bgw.ReportProgress(0, $"Working in connect to Outlook remote... Success!");
                Infos = Client.GetMailInfos();
                if (Infos.Length != 0)
                {
                    bgw.RunWorkerAsync();
                }
                else
                {
                    bgw.ReportProgress(0, $"No new messages!");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            float porcent = 0;
            int c = 0;
            ListMail = new List<Mail>();
            try
            {
                DateTime time = DateTime.Parse(string.Format("{0:g}", Properties.Settings.Default.DateTimeEmail, CultureInfo));
                for (int i = Infos.Length - 1; i >= 0; i--)
                {
                    MailInfo mail = Infos[i];
                    Mail oMail = Client.GetMail(mail);
                    int res = DateTime.Compare(time, DateTime.Parse(string.Format("{0:g}", oMail.ReceivedDate), CultureInfo)); // -1 ok
                    if (res == 0)
                    {
                        LastTime = DateTime.Parse(string.Format("{0:g}", oMail.ReceivedDate), CultureInfo);
                        break;
                    }
                    if (res == -1)
                    {
                        ListMail.Add(oMail);
                        if (mail.Index == Infos.Length)
                        {
                            LastTime = oMail.ReceivedDate;
                            Properties.Settings.Default.DateTimeEmail = DateTime.Parse(string.Format("{0:g}", LastTime), CultureInfo);
                        }
                        c++;
                        worker.ReportProgress(0, $"Download email from the server...    ({c}-{oMail.ReceivedDate})");
                    }
                    if (res == 0 || res == 1)
                    {
                        //LastTime = DateTime.Parse(string.Format("{0:g}", oMail.ReceivedDate), CultureInfo);
                        break;
                    }
                }

                if (ListMail.Count == 0)
                {
                    worker.ReportProgress(0, "There are no new messages...");
                    Thread.Sleep(500);
                    return;
                }
                else
                {
                    c = 0;
                    if (!Directory.Exists($"{ Directory.GetCurrentDirectory()}\\temp"))
                    {
                        Directory.CreateDirectory($"{ Directory.GetCurrentDirectory()}\\temp");
                    }
                    using (RegistroReclamoDteServiceEndpointService dateTimeDte = new RegistroReclamoDteServiceEndpointService(TokenSii))
                    {
                        foreach (Mail item in ListMail)
                        {
                            item.DecodeTNEF();
                            Attachment[] atts = item.Attachments;
                            foreach (Attachment attachment in atts)
                            {
                                if (attachment.Name.Contains(".xml"))
                                {
                                    attachment.SaveAs($"{ Directory.GetCurrentDirectory()}\\temp\\{attachment.Name}", true);
                                    using (StreamReader reader = new StreamReader($"{ Directory.GetCurrentDirectory()}\\temp\\{attachment.Name}"))
                                    {
                                        XmlSerializer deserializer = new XmlSerializer(typeof(EnvioDTE));
                                        XDocument xmlDocumnet = XDocument.Load($"{ Directory.GetCurrentDirectory()}\\temp\\{attachment.Name}");
                                        if (xmlDocumnet.Root.Name.LocalName == "EnvioDTE")
                                        {
                                            EnvioDTE xmlObjeto = (EnvioDTE)deserializer.Deserialize(reader);
                                            foreach (DTEDefType type in xmlObjeto.SetDTE.DTE)
                                            {
                                                DTEDefTypeDocumento document = (DTEDefTypeDocumento)type.Item;
                                                string tipoDte = null;
                                                switch (document.Encabezado.IdDoc.TipoDTE)
                                                {
                                                    case DTEType.Item33:
                                                         tipoDte = "33";
                                                        break;
                                                    case DTEType.Item34:
                                                        tipoDte = "34";
                                                        break;
                                                    case DTEType.Item46:
                                                        tipoDte = "46";
                                                        break;
                                                    case DTEType.Item52:
                                                        tipoDte = "52";
                                                        break;
                                                    case DTEType.Item56:
                                                        tipoDte = "56";
                                                        break;
                                                    case DTEType.Item61:
                                                        tipoDte = "61";
                                                        break;
                                                    default:
                                                        break;
                                                }
                                                string[] emisor = document.Encabezado.Emisor.RUTEmisor.Split('-');
                                                string response = dateTimeDte.consultarFechaRecepcionSii(emisor.GetValue(0).ToString(),
                                                    emisor.GetValue(1).ToString(),
                                                    tipoDte, // Hay un Enum
                                                    document.Encabezado.IdDoc.Folio);
                                                if (response.Length !=0)
                                                {
                                                    DateTime timeResponse = DateTime.Parse(string.Format(CultureInfo, "{0:D}", response));
                                                    // Save in month folder
                                                    // Name of folder
                                                    string nameFolder = $"{timeResponse.Year}\\{timeResponse.Month}\\{document.Encabezado.Receptor.RUTRecep}";
                                                    string nameFile = $"{document.Encabezado.Emisor.RUTEmisor}__{document.Encabezado.IdDoc.Folio}";
                                                    if (!Directory.Exists($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}"))
                                                    {
                                                        Directory.CreateDirectory($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}");
                                                    }
                                                    attachment.SaveAs($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}\\{nameFile}.xml", true);
                                                }
                                                else
                                                {
                                                    string nameFolder = $"{document.Encabezado.IdDoc.FchEmis.Year}\\{document.Encabezado.IdDoc.FchEmis.Month}\\{document.Encabezado.Receptor.RUTRecep}";
                                                    string nameFile = $"{document.Encabezado.Emisor.RUTEmisor}__{document.Encabezado.IdDoc.Folio}";
                                                    if (!Directory.Exists($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}\\no_date"))
                                                    {
                                                        Directory.CreateDirectory($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}\\no_date");
                                                    }
                                                    attachment.SaveAs($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}\\no_date\\{nameFile}.xml", true);
                                                }

                                              

                                               

                                            }
                                        }
                                    }
                                }
                            }
                            c++;
                            porcent = (float)(100 * c) / ListMail.Count;
                            worker.ReportProgress((int)porcent, $"Saving the Xml files...   ({c}/{ListMail.Count})");
                            Thread.Sleep(500);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Client.Close();
                Properties.Settings.Default.Save();
            }
        }

        enum TipoDte
        {
            item33=33,
            item34=34
            
        }
    }
}

