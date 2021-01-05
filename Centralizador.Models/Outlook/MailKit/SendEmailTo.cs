using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace Centralizador.Models.Outlook.MailKit
{
    public class SendEmailTo
    {
        private ResultParticipant UserParticipant { get; set; }

        public SendEmailTo(ResultParticipant userParticipant)
        {
            UserParticipant = userParticipant;
        }

        public async Task SendMailToParticipantAsync(Detalle detalle, ResultParticipant participant)
        {
            var bodyBuilder = new BodyBuilder();
            StringBuilder builderCEN = new StringBuilder();
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string RutReceptorEmail = null;
            string nameReceptorEmail = null;

            if (detalle.Instruction != null)
            {
                RutReceptorEmail = string.Format(CultureInfo.CurrentCulture, "{0:N0}", detalle.Instruction.ParticipantCreditor.Rut).Replace(',', '.');
                nameReceptorEmail = ti.ToTitleCase(detalle.Instruction.ParticipantCreditor.BusinessName.ToLower());
            }
            string rutDebtor = string.Format(CultureInfo.CurrentCulture, "{0:N0}", UserParticipant.Rut).Replace(',', '.');
            string dv = UserParticipant.VerificationCode;
            string nameDebtor = ti.ToTitleCase(UserParticipant.BusinessName.ToLower());
            nameReceptorEmail = ti.ToTitleCase(detalle.RznSocRecep.ToLower());
            RutReceptorEmail = detalle.RutReceptor + "-" + detalle.DvReceptor;

            if (detalle.ValidatorFlag != null)
            {
                if (detalle.ValidatorFlag.FmaPago) // FORMA PAGO
                {
                    builderCEN.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;FmaPago&gt;" + "<br/>");
                }
                if (detalle.ValidatorFlag.Flag == AppFunctions.ValidatorFlag.LetterFlag.Blue) // MONTO
                {
                    builderCEN.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;-Monto no corresponde." + "<br/>");
                }
                if (detalle.ValidatorFlag.FolioRef) // DE01724A17C14S0015
                {
                    builderCEN.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;FolioRef&gt;" + "<br/>");
                }
                if (detalle.ValidatorFlag.RazonRef) // SEN_[RBPA][Ene18-Dic18][R][V02]
                {
                    builderCEN.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;-No se encuentra el Tag :  &lt;RazonRef&gt;" + "<br/>");
                }
            }
            if (detalle.DTEDef == null)
            {
                builderCEN.AppendLine("&nbsp;&nbsp;&nbsp;&nbsp;-El archivo XML no ha sido recepcionado en nuestra casilla de correos.");
            }

            bodyBuilder.HtmlBody += $"<p>Se&ntilde;ores: <span style=\"text-decoration: underline;\">{nameReceptorEmail}</span>&nbsp;</p>\r\n<p>Rut: {RutReceptorEmail}</p>\r\n" +
                $"<p>&nbsp;</p>\r\n<p>La empresa \"{nameDebtor}\" Rut: {rutDebtor}-{dv} " +
                "informa que la factura que se individualiza a continuaci&oacute;n fue rechazada en el SII por el motivo que se se&ntilde;ala:</p>\r\n<p>&nbsp;</p>\r\n" +
                $"<p>Folio: {detalle.Folio}</p>\r\n" +
                $"<p>Fecha: {detalle.FechaEmision}</p>\r\n" +
                $"<p>Monto neto: $ {detalle.MntNeto:#,##}</p>\r\n" +
                $"<p>Motivo del rechazo: <br/> {builderCEN}</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>&nbsp;</p>\r\n" +
                "<p>&nbsp;</p>\r\n<p>Para mayor informaci&oacute;n sobre las exigencias del CEN: " +
                "<a href=\"https://www.coordinador.cl/corta/6\">https://www.coordinador.cl</a>&nbsp;" +
                "</p>\r\n<hr />\r\n<p><strong><span style=\"color: #0000ff;\">" +
                "Una herramienta Centralizador.</span></strong></p>";
            try
            {
                MailboxAddress ffrom = new MailboxAddress(Properties.Settings.Default.UserEmail, Properties.Settings.Default.UserEmail);
                MailboxAddress tto = new MailboxAddress("sergiokml@outlook.com", "sergiokml@outlook.com");
                MailboxAddress cc = new MailboxAddress(Properties.Settings.Default.CCEmail, Properties.Settings.Default.CCEmail);

                var message = new MimeMessage();
                message.From.Add(ffrom);
                message.To.Add(tto);
                message.Cc.Add(cc);
                message.Subject = "Notifica rechazo factura CEN";
                message.Priority = MessagePriority.Urgent;

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.MessageSent += Client_MessageSent;
                    await client.ConnectAsync("smtp.office365.com", 587, false);
                    await client.AuthenticateAsync(Properties.Settings.Default.UserEmail, Properties.Settings.Default.UserPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                // To
                if (detalle.Instruction != null)
                {
                    if (detalle.Instruction.ParticipantCreditor.BillsContact.Email != null)
                    {
                        message.To.Add(new MailboxAddress(detalle.Instruction.ParticipantCreditor.BillsContact.Email, detalle.Instruction.ParticipantCreditor.BillsContact.Email));
                    }

                    if (detalle.Instruction.ParticipantCreditor.PaymentsContact.Email != null)
                    {
                        message.To.Add(new MailboxAddress(detalle.Instruction.ParticipantCreditor.PaymentsContact.Email, detalle.Instruction.ParticipantCreditor.PaymentsContact.Email));
                    }
                }
                else
                {
                    if (participant.DteReceptionEmail != null)
                    {
                        message.To.Add(new MailboxAddress(participant.DteReceptionEmail, participant.DteReceptionEmail));
                    }
                    if (participant.BillsContact.Email != null)
                    {
                        message.To.Add(new MailboxAddress(participant.BillsContact.Email, participant.BillsContact.Email));
                    }
                    if (participant.PaymentsContact.Email != null)
                    {
                        message.To.Add(new MailboxAddress(participant.PaymentsContact.Email, participant.PaymentsContact.Email));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Client_MessageSent(object sender, MessageSentEventArgs e)
        {
            MimeMessage res = e.Message;
        }
    }
}