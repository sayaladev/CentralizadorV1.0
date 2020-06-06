using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;

using EASendMail;

namespace Centralizador.Models.Outlook
{
    /// <summary>
    /// Send EMail with "EASendMail" tool.
    /// https://www.emailarchitect.net/easendmail/sdk/
    /// 
    /// Config SMTP
    /// Nombre de servidor: smtp.office365.com
    /// Puerto: 587
    /// Método de cifrado: STARTTLS
    /// 
    /// </summary>
    public class ServiceSendMail
    {

        private  SmtpServer OServer { get; set; }
        private static SmtpMail[] OMails { get; set; }
        private static SmtpServer[] OServers { get; set; }
        private static SmtpClient OSmtp { get; set; }
        private static int Count { get; set; }
        private ResultParticipant UserParticipant { get; set; }
              
        public ServiceSendMail(int count, ResultParticipant participant)
        {
            UserParticipant = participant;
            OMails = new SmtpMail[count];
            OServers = new SmtpServer[count];
            OSmtp = new SmtpClient();
            OSmtp.OnBatchSendMail += OSmtp_OnBatchSendMail;


            OServer = new SmtpServer("smtp.office365.com")
            {
                // SMTP server address
                // User and password for ESMTP authentication
                User = Properties.Settings.Default.UserEmail,
                Password = Properties.Settings.Default.UserPassword,

                // Most mordern SMTP servers require SSL/TLS connection now.
                // ConnectTryTLS means if server supports SSL/TLS, SSL/TLS will be used automatically.
                ConnectType = SmtpConnectType.ConnectTryTLS

                // If your SMTP server uses 587 port
                // oServer.Port = 587;

                // If your SMTP server requires SSL/TLS connection on 25/587/465 port
                // oServer.Port = 25; // 25 or 587 or 465
                // oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;
            };
        }

        private void OSmtp_OnBatchSendMail(object sender, SmtpServer server, SmtpMail mail, Exception ep, ref bool cancel)
        {
            string nameFile;
            // you can insert the result to database in this subroutine.
            if (ep != null)
            {
                // something wrong, please refer to ep.Message
                // cancel = true; // set cancel to true can cancel the remained emails.
                nameFile = $"{UserParticipant.Name}_SendEmail_Error_{mail.Date:dd-MM-yyyy-HH-mm-ss}";
            }
            else
            {
                // delivered
                nameFile = $"{UserParticipant.Name}_SendEmail_{mail.Date:dd-MM-yyyy-HH-mm-ss}";
            }
            if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\log\"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\log\");
            }
            mail.SaveAs(Directory.GetCurrentDirectory() + @"\log\" + nameFile + ".eml", false);
        }

        /// <summary>
        /// Send email about information rejected, to only Participants.
        /// </summary>
        /// <param name="detalle"></param>
        public void SendEmailToParticipant(Detalle detalle)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder builderCEN = new StringBuilder();
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string nameCreditor = ti.ToTitleCase(detalle.RznSocRecep.ToLower());
            string nameDebtor = ti.ToTitleCase(detalle.Instruction.ParticipantDebtor.BusinessName.ToLower());
            string rutCreditor = string.Format(CultureInfo.CurrentCulture, "{0:N0}", detalle.Instruction.ParticipantCreditor.Rut).Replace(',', '.');
            string rutDebtor = string.Format(CultureInfo.CurrentCulture, "{0:N0}", detalle.Instruction.ParticipantDebtor.Rut).Replace(',', '.');
            if (detalle.DTEDef != null)
            {
                DTEDefTypeDocumentoReferencia referencia = null;
                DTEDefTypeDocumento dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                if (dte.Referencia != null)
                {
                    referencia = dte.Referencia.FirstOrDefault(x => x.TpoDocRef == "SEN");
                }

                if (dte.Encabezado.IdDoc.FmaPago != DTEDefTypeDocumentoEncabezadoIdDocFmaPago.Crédito)
                {
                    builderCEN.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;FmaPago&gt;2&lt;/FmaPago&gt;" + "<br/>");
                }
                if (Convert.ToUInt32(dte.Encabezado.Totales.MntNeto) != detalle.Instruction.Amount)
                {
                    builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;MntNeto&gt;{detalle.Instruction.Amount}&lt;/MntNeto&gt;" + "<br/>");
                }
                if (referencia != null)
                {
                    if (referencia.FolioRef != detalle.Instruction.PaymentMatrix.ReferenceCode)
                    {
                        builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;FolioRef&gt;{detalle.Instruction.PaymentMatrix.ReferenceCode}&lt;/FolioRef&gt;" + "<br/>");
                    }
                    if (string.Compare(referencia.RazonRef, detalle.Instruction.PaymentMatrix.NaturalKey, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;RazonRef&gt;{detalle.Instruction.PaymentMatrix.NaturalKey}&lt;/RazonRef&gt;" + "<br/>");
                    }
                }
                if (dte.Detalle != null && dte.Detalle.Length == 1)
                {
                    if (dte.Detalle[0].DscItem != detalle.Instruction.PaymentMatrix.NaturalKey)
                    {
                        builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;DscItem&gt;{detalle.Instruction.PaymentMatrix.ReferenceCode}&lt;/DscItem&gt;" + "<br/>");
                    }
                }
                else
                {
                    builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;DscItem&gt;{detalle.Instruction.PaymentMatrix.ReferenceCode}&lt;/DscItem&gt;" + "<br/>");
                }

                if (dte.Referencia == null || referencia == null)
                {
                    builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;TpoDocRef&gt; SEN &lt;/TpoDocRef&gt;" + "<br/>");
                    builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;FolioRef&gt;{detalle.Instruction.PaymentMatrix.ReferenceCode}&lt;/FolioRef&gt;" + "<br/>");
                    builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;FchRef&gt;{detalle.Instruction.PaymentMatrix.PublishDate}&lt;/FchRef&gt;" + "<br/>");
                    builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;RazonRef&gt;{detalle.Instruction.PaymentMatrix.NaturalKey}&lt;/RazonRef&gt;" + "<br/>");
                }
            }
            else
            {
                builderCEN.AppendLine($"&nbsp;&nbsp;&nbsp;&nbsp;-El archivo XML no ha sido recepcionado en nuestra casilla de correos.");
            }

            builder.AppendFormat("<p>Se&ntilde;ores: <span style=\"text-decoration: underline;\">{0}</span>&nbsp;</p>\r\n<p>Rut: {1}-{2}</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>La empresa \"{3}\" Rut: {4}-{5} " +
                "informa que la factura que se individualiza a continuaci&oacute;n fue rechazada en el SII por el motivo que se se&ntilde;ala:</p>\r\n<p>&nbsp;</p>\r\n" +
                "<p>Folio: {6}</p>\r\n" +
                "<p>Fecha: {7}</p>\r\n" +
                "<p>Monto neto: $ {8}</p>\r\n" +
                "<p>Motivo del rechazo: <br/> {9}</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>&nbsp;</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>Para mayor informaci&oacute;n sobre las exigencias del CEN: " +
                "<a href=\"https://shorturl.at/foHX6\">https://shorturl.at/foHX6</a>&nbsp;" +
                "</p>\r\n<hr />\r\n<p><strong><span style=\"color: #0000ff;\">" +
                "Una herramienta Centralizador.</span></strong></p>", nameCreditor, rutCreditor, detalle.DvReceptor, nameDebtor, rutDebtor, detalle.Instruction.ParticipantDebtor.VerificationCode, detalle.Folio, detalle.FechaEmision, detalle.MntNeto.ToString("#,##"), builderCEN.ToString());
            try
            {
                SmtpMail OMail = new SmtpMail("TryIt")
                {
                    // Set 
                    From = new MailAddress(detalle.Instruction.ParticipantDebtor.BusinessName, Properties.Settings.Default.UserEmail),
                    //To = "sergiokml@outlook.com",
                    Subject = "Notifica rechazo factura CEN",
                    //TextBody = "this is a test email sent from c# project, do not reply",               
                    HtmlBody = builder.ToString(),
                    Priority = MailPriority.High,
                    ReplyTo = detalle.Instruction.ParticipantDebtor.BillsContact.Email
                    //Sender = detalle.Instruction.ParticipantDebtor.BusinessName
                };
                // To
                if (detalle.Instruction.ParticipantCreditor.BillsContact.Email != null)
                {
                    OMail.To.Add(new MailAddress(detalle.Instruction.ParticipantCreditor.BillsContact.Email));
                }

                if (detalle.Instruction.ParticipantCreditor.PaymentsContact.Email != null)
                {
                    OMail.To.Add(new MailAddress(detalle.Instruction.ParticipantCreditor.PaymentsContact.Email));
                }

                OMails[Count] = OMail;
                OServers[Count] = OServer;
                Count++;

                // if you want to catch the result in OnBatchSendMail event, please add the following code 
                // oSmtp.OnBatchSendMail += new EASendMail.SmtpClient.OnBatchSendMailEventHandler(OnBatchSendMail);

                if (Count == OMails.Length)
                {
                    BatchSendMail();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void BatchSendMail()
        {
            if (Count > 0)
            {
                OSmtp.BatchSendMail(OMails.Length, OServers, OMails);
                Count = 0;
            }

        }

        public static void Reject() { 
        
        }

    }
}


