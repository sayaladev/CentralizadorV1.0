using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;

using EAGetMail;

namespace Centralizador.Models.Outlook
{
    public class ServiceOutlook
    {
        //public string LocalInbox { get; set; }

        public DateTime LastTime { get; set; }

        public object Year { get; set; }

        public int Month { get; set; }

        public IList<DTEDefType> Attachments { get; set; }

        private MailInfo[] Infos { get; set; }

        private MailClient Client { get; set; }

        private IList<Mail> ListMail { get; set; }

        private string[] XmlFiles { get; set; }

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
                bgw.ReportProgress(0, $"Success!");
                Infos = Client.GetMailInfos();
                //Thread.Sleep(500);
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
                DateTime time = Properties.Settings.Default.DateTimeEmail;
                for (int i = Infos.Length - 1; i >= 0; i--)
                {
                    MailInfo mail = Infos[i];
                    Mail oMail = Client.GetMail(mail);
                    int res = DateTime.Compare(time, oMail.ReceivedDate); // -1 ok
                    if (res == -1)
                    {
                        ListMail.Add(oMail);
                        if (mail.Index == Infos.Length)
                        {
                            LastTime = oMail.ReceivedDate;
                            Properties.Settings.Default.DateTimeEmail = LastTime;
                            // //Puedo usarlo en otro momento!
                        }
                        c++;
                        worker.ReportProgress(0, $"Working in download Email... receiving ({c}-{oMail.ReceivedDate})");
                    }
                    else
                    {
                        break;
                    }
                }

                if (ListMail.Count == 0)
                {
                    worker.ReportProgress(0, "There are no new messages...");
                    LastTime = time;
                    Thread.Sleep(500);
                    return;
                }
                else
                {                    
                    c = 0;
                    foreach (Mail item in ListMail)
                    {
                        if (!Directory.Exists($"{ Directory.GetCurrentDirectory()}\\inbox\\{Year}\\{item.ReceivedDate.Month}"))
                            {
                                Directory.CreateDirectory($"{ Directory.GetCurrentDirectory()}\\inbox\\{Year}\\{item.ReceivedDate.Month}");
                            }
                            item.DecodeTNEF();
                            Attachment[] atts = item.Attachments;
                            foreach (Attachment attachment in atts)
                            {
                                if (attachment.Name.Contains(".xml"))
                                {                                    
                                    attachment.SaveAs($"{$"{ Directory.GetCurrentDirectory()}\\inbox\\{Year}\\{item.ReceivedDate.Month}"}\\{attachment.Name}", true);
                                }
                            }
                            c++;
                            porcent = (float)(100 * c) / ListMail.Count;
                            worker.ReportProgress((int)porcent, $"Working in save Xml files... saving ({c}/{ListMail.Count})");                       
                    }
                    Thread.Sleep(500);
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
        public void ReadXML(BackgroundWorker bgw)
        {
            bgw.DoWork += BgwReadXml_DoWork;            
            XmlFiles = Directory.GetFiles($"{ Directory.GetCurrentDirectory()}\\inbox\\{Year}\\{Month}", "*.xml", SearchOption.TopDirectoryOnly);
            bgw.RunWorkerAsync();

        }

        private void BgwReadXml_DoWork(object sender, DoWorkEventArgs e)
        {
            Attachments = new List<DTEDefType>();
            try
            {
                foreach (string item in XmlFiles)
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(EnvioDTE));
                    FileStream docSearchingRoot = File.OpenRead(item);
                    XDocument xmlDocumnet = XDocument.Load(item);
                    if (xmlDocumnet.Root.Name.LocalName == "EnvioDTE")
                    {
                        EnvioDTE xmlObjeto = (EnvioDTE)deserializer.Deserialize(docSearchingRoot);
                        foreach (DTEDefType element in xmlObjeto.SetDTE.DTE)
                        {
                            Attachments.Add(element);
                        }
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}

