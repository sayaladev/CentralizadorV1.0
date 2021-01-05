using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Centralizador.Models.AppFunctions;
using Centralizador.Models.registroreclamodteservice;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using static Centralizador.Models.ProgressReportModel;

namespace Centralizador.Models.Outlook.MailKit
{
    public class ReadEmailFrom
    {
        private string TokenSii { get; set; }
        private IProgress<ProgressReportModel> Progress { get; set; }
        private ProgressReportModel ProgressReport { get; set; }

        public ReadEmailFrom(string tokenSii, IProgress<ProgressReportModel> progress)
        {
            TokenSii = tokenSii;
            Progress = progress;
            ProgressReport = new ProgressReportModel(TipoTask.ReadEmail);
        }

        public ReadEmailFrom()
        {
        }

        public async Task ReadMailFromServer(CancellationToken token)
        {
            //ProgressReport = new ProgressReportModel(TipoTask.ReadEmail);
            using (ImapClient client = new ImapClient())
            {
                string pathTemp = @"C:\Centralizador\Temp\";
                new CreateFile(pathTemp);
                int c = 0;
                try
                {
                    await client.ConnectAsync("outlook.office365.com", 993, true);
                    await client.AuthenticateAsync(Properties.Settings.Default.UserEmail, Properties.Settings.Default.UserPassword);

                    IList<IMailFolder> folders = await client.GetFoldersAsync(client.PersonalNamespaces[0]);
                    foreach (IMailFolder f in folders)
                    {
                        if (f == folders[1] || f == folders[8] || f == folders[13])
                        {
                            await f.OpenAsync(FolderAccess.ReadOnly);
                            UniqueIdRange range = new UniqueIdRange(new UniqueId(Convert.ToUInt32(Properties.Settings.Default.UIDRange)), UniqueId.MaxValue);
                            BinarySearchQuery query = SearchQuery.DeliveredAfter(Properties.Settings.Default.DateTimeEmail).And(SearchQuery.ToContains(Properties.Settings.Default.UserEmail));
                            IList<UniqueId> listM = await f.SearchAsync(range, query);
                            int total = listM.Count;

                            foreach (UniqueId uid in await f.SearchAsync(range, query))
                            {
                                MimeMessage message = await f.GetMessageAsync(uid);
                                if (message.Attachments != null)
                                {
                                    IEnumerable<MimeEntity> attachments = GetXmlAttachments(message.BodyParts);
                                    foreach (MimeEntity item in attachments)
                                    {
                                        using (MemoryStream memory = new MemoryStream())
                                        {
                                            if (item is MessagePart rfc822)
                                            {
                                                await rfc822.Message.WriteToAsync(memory);
                                            }
                                            else
                                            {
                                                MimePart part = (MimePart)item;
                                                await part.Content.DecodeToAsync(memory);
                                            }
                                            memory.Position = 0;
                                            XDocument xDocument = XDocument.Load(memory);
                                            if (xDocument.Root.Name.LocalName == "EnvioDTE")
                                            {
                                                int res = SaveFiles(xDocument);
                                                if (res != 0)
                                                {
                                                    MessageBox.Show("Error", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                        }
                                    }
                                }
                                if (f == folders[1])
                                {
                                    Properties.Settings.Default.DateTimeEmail = message.Date.DateTime;
                                    Properties.Settings.Default.UIDRange = uid.ToString();
                                    c++;
                                    float porcent = (float)(100 * c) / total;
                                    ProgressReport.Message = $"Dowloading messages... [{string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy HH:mm}", message.Date.DateTime)}] ({c}/{total})  Subject: {message.Subject} ";
                                    ProgressReport.PercentageComplete = (int)porcent;
                                    ProgressReport.FchOutlook = Properties.Settings.Default.DateTimeEmail;
                                    Progress.Report(ProgressReport);
                                }
                                if (token.IsCancellationRequested) { token.ThrowIfCancellationRequested(); }
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    ProgressReport.Message = "Task canceled...  !";
                    ProgressReport.PercentageComplete = 100;
                    Progress.Report(ProgressReport);
                    return;
                }
                finally
                {
                    SaveParam();
                    await client.DisconnectAsync(true);
                    SetStateReport(false);
                }
            }
        }

        private static IEnumerable<MimeEntity> GetXmlAttachments(IEnumerable<MimeEntity> bodyParts)
        {
            foreach (MimeEntity bodyPart in bodyParts)
            {
                MessagePart rfc822 = bodyPart as MessagePart;

                if (rfc822 != null)
                {
                    foreach (MimeEntity attachment in GetXmlAttachments(rfc822.Message.BodyParts))
                    {
                        yield return attachment;
                    }
                }
                else
                {
                    string fileName = bodyPart.ContentDisposition?.FileName;

                    if (fileName != null && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return bodyPart;
                    }
                }
            }
        }

        public static void SaveParam()
        {
            Properties.Settings.Default.Save();
        }

        private int SaveFiles(XDocument xDocument)
        {
            string response = "";
            string nameFolder;
            string nameFile;
            // DESERIALIZE.
            EnvioDTE xmlObjeto = ServicePdf.TransformStringDTEDefTypeToObjectDTEAsync(xDocument).Result;
            if (xmlObjeto != null)
            {
                foreach (DTEDefType dte in xmlObjeto.SetDTE.DTE)
                {
                    DTEDefTypeDocumento document = (DTEDefTypeDocumento)dte.Item;
                    string[] emisor = document.Encabezado.Emisor.RUTEmisor.Split('-');
                    if (document.Encabezado.IdDoc.TipoDTE == global::DTEType.Item33 || document.Encabezado.IdDoc.TipoDTE == global::DTEType.Item34)
                    {
                        document.Encabezado.IdDoc.Folio = document.Encabezado.IdDoc.Folio.TrimStart(new char[] { '0' }); // REMOVE ZERO LEFT.
                        try
                        {
                            using (RegistroReclamoDteServiceEndpointService dateTimeDte = new RegistroReclamoDteServiceEndpointService(TokenSii))
                            {
                                response = dateTimeDte.consultarFechaRecepcionSii(emisor.GetValue(0).ToString(),
                                emisor.GetValue(1).ToString(),
                                Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString(),
                                document.Encabezado.IdDoc.Folio);
                            }

                            if (response.Length != 0)
                            {
                                DateTime timeResponse = DateTime.Parse(string.Format(CultureInfo.InvariantCulture, "{0:D}", response));
                                // 2020\06\76470581-5
                                nameFolder = timeResponse.Year + @"\" + timeResponse.Month + @"\" + document.Encabezado.Receptor.RUTRecep;
                                // 15357870_33_8888
                                nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString() + "__" + document.Encabezado.IdDoc.Folio;
                                SaveXmlInFolder(nameFolder, nameFile, dte);
                            }
                            else
                            {
                                // Errors. // dejar carpeta folder en 2020/5
                                nameFolder = document.Encabezado.IdDoc.FchEmis.Year + @"\" + document.Encabezado.IdDoc.FchEmis.Month;
                                nameFile = document.Encabezado.Emisor.RUTEmisor + "__" + Convert.ToInt32(document.Encabezado.IdDoc.TipoDTE).ToString() + "__" + document.Encabezado.IdDoc.Folio;
                                SaveXmlInFolder(nameFolder + @"\Errors\", nameFile, dte);
                            }
                        }
                        catch (Exception)
                        {
                            // ERROR.
                            return 1;
                        }
                    }
                }
            }
            // SUCCESS.
            return 0;
        }

        private static void SaveXmlInFolder(string nameFolder, string nameFile, DTEDefType dte)
        {
            try
            {
                string path = @"C:\Centralizador\Inbox\" + nameFolder;
                new CreateFile(path);
                File.WriteAllText(path + @"\" + nameFile + ".xml", ServicePdf.TransformObjectToXml(dte));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DateTime GetLastDateTime()
        {
            return Properties.Settings.Default.DateTimeEmail;
        }
    }
}