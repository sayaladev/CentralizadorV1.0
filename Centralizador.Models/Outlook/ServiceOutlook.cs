﻿using System;
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
    public class ServiceOutlook
    {
        public string TokenSii { get; set; }
        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");

        public void GetXmlFromEmail(BackgroundWorker bgw)
        {
            bgw.DoWork += Bgw_DoWork;
            bgw.RunWorkerAsync();
        }

        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgw = sender as BackgroundWorker;
            int c = 0;
            float porcent = 0;
            MailServer oServer = new MailServer("outlook.office365.com", Properties.Settings.Default.UserEmail, Properties.Settings.Default.UserPassword, ServerProtocol.Imap4)
            {
                SSLConnection = true,
                Port = 993
            };
            MailClient oClient = new MailClient("EG-C1508812802-00376-7E2448B3BDAEEB7D-338FF6UAA8257EB7");
            try
            {
                bgw.ReportProgress(0, "Trying to connect to the mail server...");
                oClient.Connect(oServer);
                oClient.GetMailInfosParam.Reset();
                //oClient.GetMailInfosParam.GetMailInfosOptions |= GetMailInfosOptionType.NewOnly;
                //oClient.GetMailInfosParam.GetMailInfosOptions |= GetMailInfosOptionType.DateRange;
                oClient.GetMailInfosParam.GetMailInfosOptions |= GetMailInfosOptionType.UIDRange;
                //oClient.GetMailInfosParam.DateRange.SINCE = Properties.Settings.Default.DateTimeEmail;
                oClient.GetMailInfosParam.UIDRange = $"{Properties.Settings.Default.UIDRange}:*";
                MailInfo[] infos = oClient.GetMailInfos();
                // MailInfo[] infoss = oClient.SearchMail($"ALL SINCE {Properties.Settings.Default.DateTimeEmail}");
                //Properties.Settings.Default.Save();
                string pathTemp = Directory.GetCurrentDirectory() + @"\temp";
                if (!Directory.Exists(pathTemp))
                {
                    Directory.CreateDirectory(pathTemp);
                }
                e.Result = Properties.Settings.Default.DateTimeEmail;
                for (int i = 0; i < infos.Length; i++)
                {
                    MailInfo info = infos[i];
                    Mail oMail = new Mail("EG-C1508812802-00376-7E2448B3BDAEEB7D-338FF6UAA8257EB7");
                    oMail = oClient.GetMail(info);
                    Attachment[] atts = oMail.Attachments;
                    foreach (Attachment att in atts)
                    {
                        if (att.Name.Contains(".xml"))
                        {
                            att.SaveAs(pathTemp + @"\" + att.Name, true);
                            try
                            {
                                XDocument xmlDocument;
                                //XDocument xmlDocument = XDocument.Load(pathTemp + @"\" + att.Name);
                                using (StreamReader oReader = new StreamReader(pathTemp + @"\" + att.Name, Encoding.GetEncoding("ISO-8859-1")))
                                {
                                    xmlDocument = XDocument.Load(oReader);
                                }
                                if (xmlDocument.Root.Name.LocalName == "EnvioDTE")
                                {
                                    if (!SaveFiles(pathTemp + @"\" + att.Name))
                                    {
                                        MessageBox.Show("Sii: Application with Momentary Suspension", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return;
                                    }
                                }
                            }
                            catch (System.Xml.XmlException)
                            {
                                //throw;
                            }
                        }
                    }
                    e.Result = oMail.ReceivedDate;
                    Properties.Settings.Default.DateTimeEmail = oMail.ReceivedDate;
                    Properties.Settings.Default.UIDRange = info.UIDL;
                    c++;
                    porcent = (float)(100 * c) / infos.Length;
                    bgw.ReportProgress((int)porcent, $"Dowloading email from server... [{string.Format(CultureInfo, "{0:g}", oMail.ReceivedDate)}] ({c}/{infos.Length})");
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                oClient.Close();
                Properties.Settings.Default.Save();
            }
        }
        private bool SaveFiles(string path)
        {
            string response = "";
            string nameFolder;
            string nameFile;
            // Deserialize
            EnvioDTE xmlObjeto = ServicePdf.TransformXmlEnvioDTEToObject(path);
            foreach (DTEDefType dte in xmlObjeto.SetDTE.DTE)
            {
                DTEDefTypeDocumento document = (DTEDefTypeDocumento)dte.Item;
                string[] emisor = document.Encabezado.Emisor.RUTEmisor.Split('-');
                // Only process TipoDTE 33 & 34.
                if (document.Encabezado.IdDoc.TipoDTE == global::DTEType.Item33 || document.Encabezado.IdDoc.TipoDTE == global::DTEType.Item34)
                {
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
                        return false;
                    }
                    if (response.Length != 0)
                    {
                        DateTime timeResponse = DateTime.Parse(string.Format(CultureInfo, "{0:D}", response));
                        nameFolder = timeResponse.Year + @"\" + timeResponse.Month + @"\" + document.Encabezado.Receptor.RUTRecep;
                        nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString() + "__" + document.Encabezado.IdDoc.Folio;
                        Save(nameFolder, nameFile, dte);
                        return true;
                    }
                    else
                    {
                        // Errors.
                        nameFolder = document.Encabezado.IdDoc.FchEmis.Year + @"\" + document.Encabezado.IdDoc.FchEmis.Month + @"\" + document.Encabezado.Receptor.RUTRecep;
                        nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString() + "__" + document.Encabezado.IdDoc.Folio;
                        Save(nameFolder + @"\Errors\", nameFile, dte);
                        return true;
                    }
                }
            }
            return true;
        }
        private void Save(string nameFolder, string nameFile, DTEDefType dte)
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\inbox\" + nameFolder))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\inbox\" + nameFolder);
            }
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\inbox\" + nameFolder + @"\" + nameFile + ".xml", ServicePdf.TransformObjectToXml(dte));
        }     
    }
}


