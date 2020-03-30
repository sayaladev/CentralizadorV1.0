using System;
using System.Globalization;
using System.IO;

using EAGetMail;

namespace Centralizador.Models.Outlook
{
    public class ServiceOutlook
    {
        private static string _generateFileName(int sequence)
        {
            DateTime currentDateTime = DateTime.Now;
            return string.Format("{0}-{1:000}-{2:000}.eml",
                currentDateTime.ToString("yyyyMMddHHmmss", new CultureInfo("es-CL")),
                currentDateTime.Millisecond,
                sequence);
        }

        public static void Test()
        {
          
                // Create a folder named "inbox" under current directory
                // to save the email retrieved.
                string localInbox = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
                // If the folder is not existed, create it.
                if (!Directory.Exists(localInbox))
                {
                    Directory.CreateDirectory(localInbox);
                }

                MailServer oServer = new MailServer("outlook.office365.com", "sergiokml@outlook.com", "nrphikakngnrbtpl", ServerProtocol.Imap4)
                {

                    // Enable SSL/TLS connection, most modern email server require SSL/TLS by default
                    AuthType = ServerAuthType.AuthLogin,
                    SSLConnection = true,
                    Port = 993
                };

                // if your server doesn't support SSL/TLS, please use the following codes
                // oServer.SSLConnection = false;
                // oServer.Port = 143;

                MailClient oClient = new MailClient("TryIt");
            try
            {
                oClient.Connect(oServer);
              
           
                MailInfo[] infos = oClient.GetMailInfos();
                Console.WriteLine("Total {0} email(s)\r\n", infos.Length);
                for (int i = 0; i < infos.Length; i++)
                {
                    MailInfo info = infos[i];
                    Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                        info.Index, info.Size, info.UIDL);

                    // Receive email from IMAP4 server
                    Mail oMail = oClient.GetMail(info);

                    System.Windows.Forms.MessageBox.Show($"From: {oMail.From.ToString()}- Subject: {oMail.Subject}");
                    ParseAttachment(oMail);

                    // Generate an unqiue email file name based on date time.
                    string fileName = _generateFileName(i + 1);
                    string fullPath = string.Format("{0}\\{1}", localInbox, fileName);

                    // Save email to local disk
                    oMail.SaveAs(fullPath, true);

                    // Mark email as deleted from IMAP4 server.
                    //oClient.Delete(info);
                }

                // Quit and expunge emails marked as deleted from IMAP4 server.
                oClient.Quit();
                System.Windows.Forms.MessageBox.Show("Completed!");
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show(oClient.Conversation);
            }
        }

        public static void ParseAttachment(Mail  mail)
        {
            //Mail oMail = new Mail("TryIt");
            //oMail.Load("c:\\test.eml", false);

            // Decode winmail.dat (TNEF) automatically
            mail.DecodeTNEF();

            Attachment[] atts = mail.Attachments;
            int count = atts.Length;
            string tempFolder = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }

            for (int i = 0; i < count; i++)
            {
                Attachment att = atts[i];
                string attname = String.Format("{0}\\{1}", tempFolder, att.Name);
                att.SaveAs(attname, true);
            }
        }
    }
}
