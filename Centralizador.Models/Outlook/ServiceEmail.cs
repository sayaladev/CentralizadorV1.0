using System.Collections.Generic;

using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Limilabs.Mail.MIME;

namespace Centralizador.Models.Outlook
{
   
    public class ServiceEmail
    {

        private const string _server = "outlook.office365.com";
        private const string _user = "facturacionchile@capvertenergie.com";
        private const string _password = "Che@2019!";


        public static void Get()
        {
            LastRun last = LoadPreviousRun();

            using (Imap imap = new Imap())
            {
                imap.Connect(_server); // or ConnectSSL for SSL
                imap.UseBestLogin(_user, _password);

                FolderStatus status = imap.SelectInbox();

                List<long> uids;

               

                uids = imap.Search().Where(Expression.UID(Range.From(last.LargestUID)));            
             


                foreach (long uid in uids)
                {
                    IMail email = new MailBuilder().CreateFromEml(imap.GetMessageByUID(uid));
                    foreach (MimeData attachment in email.Attachments)
                    {
                   
                        //attachment.Save(@"c:\" + attachment.SafeFileName);
                    }


                    LastRun current = new LastRun
                    {
                        
                        LargestUID = uid
                    };
                    SaveThisRun(current);
                }
                imap.Close();
            }
        }


        private static void SaveThisRun(LastRun run)
        {
            // Your code that saves run data.

        }

        private static LastRun LoadPreviousRun()
        {
            return new LastRun { LargestUID = 65000 }; // 32374 es que no viene, el último.
            // Your code that loads last run data (null on the first run).
        }

    }

}

