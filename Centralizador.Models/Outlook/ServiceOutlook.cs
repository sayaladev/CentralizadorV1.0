using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using EAGetMail;

namespace Centralizador.Models.Outlook
{
    public class ServiceOutlook
    {

        public static DateTime GetXmlFromEmail()
        {
            DateTime lastTime = Properties.Settings.Default.DateTimeEmail;
            MailServer oServer = new MailServer("outlook.office365.com", "sergiokml@outlook.com", "nrphikakngnrbtpl", ServerProtocol.Imap4)
            {
                AuthType = ServerAuthType.AuthLogin,
                SSLConnection = true,
                Port = 993
            };
            MailClient oClient = new MailClient("TryIt");
            try
            {
                oClient.Connect(oServer);
                MailInfo[] infos = oClient.GetMailInfos();
                int c = 0;
                if (infos.Length != 0)
                {
                    for (int i = infos.Length - 1; i >= 0; i--)
                    {
                        MailInfo mail = infos[i];
                        Mail oMail = oClient.GetMail(mail);
                        int res = DateTime.Compare(lastTime, oMail.ReceivedDate); // -1 ok
                        if (res == -1)
                        {
                            // Create folder 
                            string localInbox = $"{ Directory.GetCurrentDirectory()}\\inbox\\{oMail.ReceivedDate.Year}\\{oMail.ReceivedDate.Month}";

                            if (!Directory.Exists(localInbox))
                            {
                                Directory.CreateDirectory(localInbox);
                            }


                            // Save attachment
                            oMail.DecodeTNEF();
                            Attachment[] atts = oMail.Attachments;
                            foreach (Attachment attachment in atts)
                            {
                                string attname = $"{localInbox}\\{attachment.Name}";
                                if (attachment.Name.Contains(".xml"))
                                {
                                    attachment.SaveAs(attname, true);
                                }
                            }
                            // Save new date only first email
                            if (c == 0)
                            {
                                Properties.Settings.Default.DateTimeEmail = oMail.ReceivedDate;
                                Properties.Settings.Default.Save();
                                c++;
                            }
                        }
                        else
                        {
                            break;
                        }

                    }
                    oClient.Close();

                }
            }
            catch (Exception)
            {

            }
            return lastTime;
        }



        public static void ReadXML()
        {

            DateTime today = DateTime.Today.Date;
            string localInbox = $"{ Directory.GetCurrentDirectory()}\\inbox\\{today.Year}\\{today.Month}";
            IList<DTEDefType> attachments = new List<DTEDefType>();
            string[] xmlFiles = Directory.GetFiles(localInbox, "*.xml", SearchOption.TopDirectoryOnly);
            foreach (string item in xmlFiles)
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(EnvioDTE));

                FileStream docSearchingRoot = File.OpenRead(item);
                XDocument xmlDocumnet = XDocument.Load(item);
                if (xmlDocumnet.Root.Name.LocalName == "EnvioDTE")
                {

                    EnvioDTE xmlObjeto = (EnvioDTE)deserializer.Deserialize(docSearchingRoot);
                    foreach (DTEDefType element in xmlObjeto.SetDTE.DTE)
                    {
                        // Search Participant

                        //AttachmentXml attachmentXml = new AttachmentXml();
                        attachments.Add(element);
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(xmlDocumnet.Root.Name.LocalName);
                }
                
                try
                {

                }
                catch (Exception)
                {

                    throw;
                }             
            }
        }


    }
}

