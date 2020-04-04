using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;

using Centralizador.Models.AppFunctions;
using Centralizador.Models.registroreclamodteservice;

using EAGetMail;

using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Limilabs.Mail.MIME;

namespace Centralizador.Models.Outlook
{

    internal class LastRun
    {
        public long LargestUID { get; set; }
    }

    public class ServiceOutlook
    {

        public DateTime LastTime { get; set; }

        private string TokenSii { get; set; }

        private MailInfo[] Infos { get; set; }

        private MailClient Client { get; set; }

        private IList<Mail> ListMail { get; set; }

        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");

        public ServiceOutlook(DateTime lastTime, string tokenSii)
        {
            LastTime = lastTime;
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
            float porcent = 0;
            int c = 0;

            using (Imap imap = new Imap())
            {
                string userName = Properties.Settings.Default.UserEmail;
                string password = Properties.Settings.Default.UserPassword;
                string pathTemp = Directory.GetCurrentDirectory() + @"\temp";

                imap.Connect("outlook.office365.com"); // or ConnectSSL for SSL
                imap.UseBestLogin(userName, password);

                long Uid = Properties.Settings.Default.UIdEmail;
                imap.SelectInbox(); // Default inbox
                List<long> uids = imap.Search().Where(Expression.UID(Range.From(Uid)));

                if (!Directory.Exists(pathTemp))
                {
                    Directory.CreateDirectory(pathTemp);
                }
                foreach (long uid in uids)
                {
                    byte[] eml = imap.GetMessageByUID(uid);
                    IMail email = new MailBuilder().CreateFromEml(eml);
                    foreach (MimeData attachment in email.Attachments)
                    {
                        if (attachment.ContentType.MimeSubtype.Name == "text/xml")
                        {
                            attachment.Save(pathTemp + @"\" + attachment.SafeFileName);
                            XDocument xmlDocumnet = XDocument.Load(pathTemp + @"\" + attachment.SafeFileName);
                            if (xmlDocumnet.Root.Name.LocalName == "EnvioDTE")
                            {
                                XmlSerializer deserializer = new XmlSerializer(typeof(EnvioDTE));
                                EnvioDTE xmlObjeto;
                                using (StreamReader reader = new StreamReader(pathTemp + @"\" + attachment.SafeFileName))
                                {
                                    xmlObjeto = (EnvioDTE)deserializer.Deserialize(reader);
                                }
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
                                    string response;
                                    using (RegistroReclamoDteServiceEndpointService dateTimeDte = new RegistroReclamoDteServiceEndpointService(TokenSii))
                                    {
                                        response = dateTimeDte.consultarFechaRecepcionSii(emisor.GetValue(0).ToString(),
                                        emisor.GetValue(1).ToString(),
                                        tipoDte,
                                        document.Encabezado.IdDoc.Folio);
                                    }
                                    if (response.Length != 0)
                                    {
                                        DateTime timeResponse = DateTime.Parse(string.Format(CultureInfo, "{0:D}", response));
                                        string nameFolder = $"{timeResponse.Year}\\{timeResponse.Month}\\{document.Encabezado.Receptor.RUTRecep}";
                                        string nameFile = $"{document.Encabezado.Emisor.RUTEmisor}__{document.Encabezado.IdDoc.Folio}";
                                        if (!Directory.Exists($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}"))
                                        {
                                            Directory.CreateDirectory($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}");
                                        }
                                        //

                                        attachment.SaveAs($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}\\{nameFile}.xml", true);

                                    }
                                    else
                                    {


                                    }
                                }




                            }

                            //
                        }

                        LastTime = (DateTime)email.Date;
                    }
                    imap.Close();
                }






                ListMail = new List<Mail>();
                try
                {



                    {
                        c = 0;

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
                                                        tipoDte,
                                                        document.Encabezado.IdDoc.Folio);
                                                    // Serialize 1 dte
                                                    string dte = ServicePdf.TransformObjectToXml(document);
                                                    if (response.Length != 0)
                                                    {
                                                        DateTime timeResponse = DateTime.Parse(string.Format(CultureInfo, "{0:D}", response));
                                                        // Save in month folder                                                   
                                                        string nameFolder = $"{timeResponse.Year}\\{timeResponse.Month}\\{document.Encabezado.Receptor.RUTRecep}";
                                                        string nameFile = $"{document.Encabezado.Emisor.RUTEmisor}__{document.Encabezado.IdDoc.Folio}";
                                                        if (!Directory.Exists($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}"))
                                                        {
                                                            Directory.CreateDirectory($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}");
                                                        }
                                                        //

                                                        attachment.SaveAs($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}\\{nameFile}.xml", true);
                                                    }
                                                    else
                                                    {
                                                        // Save errors
                                                        string nameFolder = $"{document.Encabezado.IdDoc.FchEmis.Year}\\{document.Encabezado.IdDoc.FchEmis.Month}\\{document.Encabezado.Receptor.RUTRecep}";
                                                        string nameFile = $"{document.Encabezado.Emisor.RUTEmisor}__{document.Encabezado.IdDoc.Folio}";
                                                        if (!Directory.Exists($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}\\no_date"))
                                                        {
                                                            Directory.CreateDirectory($"{ Directory.GetCurrentDirectory()}\\inbox\\{nameFolder}\\no_date");
                                                        }
                                                        // Save only 1 doc, no attachment
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


        }
    }

