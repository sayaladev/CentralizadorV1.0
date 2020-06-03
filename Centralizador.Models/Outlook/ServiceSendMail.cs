using System.ComponentModel;
using System.Globalization;
using System.Text;

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
        public static void SendEmailToParticipant(BackgroundWorker BgwSendEmail, Detalle detalle) {
            BgwSendEmail.DoWork += BgwSendEmail_DoWork;
            BgwSendEmail.RunWorkerAsync(detalle);
        }

        private static void BgwSendEmail_DoWork(object sender, DoWorkEventArgs e)
        {
            Detalle detalle = e.Argument as Detalle;
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string name = ti.ToTitleCase(detalle.RznSocRecep.ToLower());
            StringBuilder builder = new StringBuilder();
            string rutCreditor = string.Format(CultureInfo.CurrentCulture, "{0:N0}", detalle.Instruction.ParticipantCreditor.Rut).Replace(',', '.');
            string rutDebtor = string.Format(CultureInfo.CurrentCulture, "{0:N0}", detalle.Instruction.ParticipantDebtor.Rut).Replace(',', '.');

            builder.AppendFormat("<p>Se&ntilde;ores: <span style=\"text-decoration: underline;\">{0}</span>&nbsp;</p>\r\n<p>Rut: {1}-{2}</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>La empresa \"{3}\" Rut: {4}-{5} " +
                "informa que la factura que se individualiza a continuaci&oacute;n fue rechazada por el motivo que se se&ntilde;ala:</p>\r\n<p>&nbsp;</p>\r\n" +
                "<p>Folio: {6}</p>\r\n" +
                "<p>Fecha: {7}</p>\r\n" +
                "<p>Monto neto: {8}</p>\r\n" +
                "<p>Motivo del rechazo:</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>&nbsp;</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>Para mayor informaci&oacute;n sobre las exigencias del CEN:" +
                "<a href=\"https://drive.google.com/drive/folders/10CYJU7VaG1JRvPiKvqPiNpOEoLDBkz1z\">https://drive.google.com/drive/folders/10CYJU7VaG1JRvPiKvqPiNpOEoLDBkz1z</a>&nbsp;" +
                "</p>\r\n<hr />\r\n<p><strong><span style=\"color: #0000ff;\">" +
                "Una herramienta Centralizador.</span></strong></p>", name, rutCreditor, detalle.DvReceptor, detalle.Instruction.ParticipantDebtor.BusinessName, rutDebtor, detalle.Instruction.ParticipantDebtor.VerificationCode, detalle.Folio, detalle.FechaEmision, detalle.MntNeto.ToString("#,##"));

            try
            {
                SmtpMail oMail = new SmtpMail("TryIt")
                {
                    // Set 
                    From = Properties.Settings.Default.UserEmail,
                    //To = "sergiokml@outlook.com",
                    To = "sergiokml@outlook.com",
                    Subject = "Notifica rechazo factura CEN",
                    //TextBody = "this is a test email sent from c# project, do not reply",               
                    HtmlBody = builder.ToString(),
                    Priority = MailPriority.High,
                    ReplyTo = detalle.Instruction.ParticipantDebtor.BillsContact.Email,
                    Sender = detalle.Instruction.ParticipantDebtor.BusinessName
                };

                // SMTP server address
                SmtpServer oServer = new SmtpServer("smtp.office365.com")
                {
                    // User and password for ESMTP authentication
                    User = Properties.Settings.Default.UserEmail,
                    Password = Properties.Settings.Default.UserPassword,

                    // Most mordern SMTP servers require SSL/TLS connection now.
                    // ConnectTryTLS means if server supports SSL/TLS, SSL/TLS will be used automatically.
                    ConnectType = SmtpConnectType.ConnectTryTLS
                };

                // If your SMTP server uses 587 port
                // oServer.Port = 587;

                // If your SMTP server requires SSL/TLS connection on 25/587/465 port
                // oServer.Port = 25; // 25 or 587 or 465
                // oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;

                //Console.WriteLine("start to send email ...");

                // SYNC
                SmtpClient oSmtp = new SmtpClient();
                oSmtp.SendMail(oServer, oMail);



                //Console.WriteLine("email was sent successfully!");
       
            }
            catch (System.Exception)
            {
                throw;
            }
        }

       
    }
}


