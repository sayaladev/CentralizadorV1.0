using System;
using System.ComponentModel;
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

        public static SmtpServer OServer { get; private set; }
        public static SmtpMail OMail { get; private set; }

        public SmtpMail[] OMails { get; private set; }
        public SmtpServer[] OServers { get; private set; }
        public SmtpClient OSmtp { get; private set; }
        public int Count { get; private set; }
        public StringBuilder Builder { get; private set; }
        public ResultParticipant UserParticipant { get; set; }

        public ServiceSendMail(int count)
        {
            OMails = new SmtpMail[count];
            OServers = new SmtpServer[count];
            OSmtp = new SmtpClient();
            OSmtp.OnBatchSendMail += OSmtp_OnBatchSendMail;
            Builder = new StringBuilder();

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
            // you can insert the result to database in this subroutine.
            if (ep != null)
            {
                // something wrong, please refer to ep.Message
                // cancel = true; // set cancel to true can cancel the remained emails.
            }
            else
            {
                // delivered if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\log\"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\log\");
                }
                string nameFile = $"{UserParticipant.Name}_SendEmail_{mail.Date:dd-MM-yyyy-HH-mm-ss}";
                mail.SaveAs(Directory.GetCurrentDirectory() + @"\log\" + nameFile + ".eml", false);
            }
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
            string name = ti.ToTitleCase(detalle.RznSocRecep.ToLower());
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
                    builderCEN.AppendLine("-No se encuentra el Tag :  &lt;FmaPago&gt;2 &lt;/FmaPago&gt;" + "<br/>");
                }
                if (Convert.ToUInt32(dte.Encabezado.Totales.MntNeto) != detalle.Instruction.Amount)
                {
                    builderCEN.AppendLine($"-No se encuentra el Tag :  &lt;MntNeto&gt; {detalle.Instruction.Amount} &lt;/MntNeto&gt;" + "<br/>");
                }
                if (referencia != null)
                {
                    if (referencia.FolioRef != detalle.Instruction.PaymentMatrix.ReferenceCode)
                    {
                        builderCEN.AppendLine($"-No se encuentra el Tag :  &lt;FolioRef&gt; {detalle.Instruction.PaymentMatrix.ReferenceCode} &lt;/FolioRef&gt;" + "<br/>") ;
                    }
                     if (string.Compare(referencia.RazonRef,detalle.Instruction.PaymentMatrix.NaturalKey, StringComparison.OrdinalIgnoreCase)!=0)
                    {
                        builderCEN.AppendLine($"-No se encuentra el Tag :  &lt;RazonRef&gt; {detalle.Instruction.PaymentMatrix.NaturalKey} &lt;/RazonRef&gt;" + "<br/>");
                    }                  
                }
                if (dte.Detalle != null && dte.Detalle.Length == 1)
                {
                    if (dte.Detalle[0].DscItem != detalle.Instruction.PaymentMatrix.NaturalKey)
                    {
                        builderCEN.AppendLine($"-No se encuentra el Tag :  &lt;DscItem&gt; {detalle.Instruction.PaymentMatrix.ReferenceCode} &lt;/DscItem&gt;" + "<br/>");
                    }
                }
                else
                {
                    builderCEN.AppendLine($"-No se encuentra el Tag :  &lt;DscItem&gt; {detalle.Instruction.PaymentMatrix.ReferenceCode} &lt;/DscItem&gt;" + "<br/>");
                }

                if (dte.Referencia == null || referencia == null)
                {
                    builderCEN.AppendLine($"-No se encuentra el Tag :  &lt;TpoDocRef&gt; SEN &lt;/TpoDocRef&gt;" + "<br/>");
                    builderCEN.AppendLine($"-No se encuentra el Tag :  &lt;FolioRef&gt; {detalle.Instruction.PaymentMatrix.ReferenceCode} &lt;/FolioRef&gt;" + "<br/>");
                    builderCEN.AppendLine($"-No se encuentra el Tag :  &lt;FchRef&gt; {detalle.Instruction.PaymentMatrix.PublishDate} &lt;/FchRef&gt;" + "<br/>");
                    builderCEN.AppendLine($"-No se encuentra el Tag :  &lt;RazonRef&gt; {detalle.Instruction.PaymentMatrix.NaturalKey} &lt;/RazonRef&gt;" + "<br/>");
                }
            }
            else
            {
                builderCEN.AppendLine($"-El archivo XML no ha sido recepcionado en nuestra casilla de correos.");
            }

            builder.AppendFormat("<p>Se&ntilde;ores: <span style=\"text-decoration: underline;\">{0}</span>&nbsp;</p>\r\n<p>Rut: {1}-{2}</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>La empresa \"{3}\" Rut: {4}-{5} " +
                "informa que la factura que se individualiza a continuaci&oacute;n fue rechazada por el motivo que se se&ntilde;ala:</p>\r\n<p>&nbsp;</p>\r\n" +
                "<p>Folio: {6}</p>\r\n" +
                "<p>Fecha: {7}</p>\r\n" +
                "<p>Monto neto: $ {8}</p>\r\n" +
                "<p>Motivo del rechazo: <br/> {9}</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>&nbsp;</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>Para mayor informaci&oacute;n sobre las exigencias del CEN: " +
                "<a href=\"https://shorturl.at/foHX6\">https://shorturl.at/foHX6</a>&nbsp;" +
                "</p>\r\n<hr />\r\n<p><strong><span style=\"color: #0000ff;\">" +
                "Una herramienta Centralizador.</span></strong></p>", name, rutCreditor, detalle.DvReceptor, detalle.Instruction.ParticipantDebtor.BusinessName, rutDebtor, detalle.Instruction.ParticipantDebtor.VerificationCode, detalle.Folio, detalle.FechaEmision, detalle.MntNeto.ToString("#,##"), builderCEN.ToString());
            try
            {
                OMail = new SmtpMail("TryIt")
                {
                    // Set 
                    From = Properties.Settings.Default.UserEmail,
                    To = "sergiokml@outlook.com",
                    Subject = "Notifica rechazo factura CEN",
                    //TextBody = "this is a test email sent from c# project, do not reply",               
                    HtmlBody = builder.ToString(),
                    Priority = MailPriority.High,
                    ReplyTo = detalle.Instruction.ParticipantDebtor.BillsContact.Email,
                    Sender = detalle.Instruction.ParticipantDebtor.BusinessName
                };

                // Tester Batch               
                OMails[Count] = OMail;
                OServers[Count] = OServer;
                Count++;

                // oSmtp.LogFileName = "c:\\smtp.log";
                // if the log wasn't able to be generated, 
                // please create a smtp.log file on C: and assign everyone read-write permission to this
                // file, then try it again.

                // if you want to catch the result in OnBatchSendMail event, please add the following code 
                // oSmtp.OnBatchSendMail += new EASendMail.SmtpClient.OnBatchSendMailEventHandler(OnBatchSendMail);

                if (Count == OMails.Length)
                {
                    OSmtp.BatchSendMail(OMails.Length, OServers, OMails);
                    Count = 0;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Builder.Clear();

            }
            //BgwSendEmail.DoWork += BgwSendEmail_DoWork;
            //SmtpClient oSmtp = new SmtpClient();
            //if (!BgwSendEmail.IsBusy)
            //{
            // BgwSendEmail.RunWorkerAsync();
            //BgwSendEmail.RunWorkerAsync(oSmtp.BeginSendMail(OServer, OMail, null, null));
            //}        
        }


        private static void BgwSendEmail_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                // SYNC ok!
                SmtpClient oSmtp = new SmtpClient();
                oSmtp.SendMail(OServer, OMail);


                // ASYNC
                //SmtpClientAsyncResult result = e.Argument as SmtpClientAsyncResult;
                //result.AsyncWaitHandle.WaitOne();
                //e.Result = result;

                //SmtpClient oSmtp = new SmtpClient();
                //SmtpClientAsyncResult oResult = oSmtp.BeginSendMail(OServer, OMail, null, null);
                //e.Result = true;
                //// Wait for the email sending...
                //while (!oResult.IsCompleted)
                //{
                //    //Console.WriteLine("waiting..., you can do other thing!");
                //    oResult.AsyncWaitHandle.WaitOne(50, false);
                //}
                //oSmtp.EndSendMail(oResult);



                // BATCH


            }
            catch (System.Exception)
            {
                throw;
            }
        }


    }
}


