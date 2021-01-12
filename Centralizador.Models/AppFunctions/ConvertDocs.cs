using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using OpenHtmlToPdf;
using Pdf417EncoderLibrary;
using static Centralizador.Models.ProgressReportModel;

namespace Centralizador.Models.AppFunctions
{
    public class ConvertDocs
    {
        private static ProgressReportModel PgModel { get; set; }
        private static IProgress<ProgressReportModel> Progress { get; set; }
        private ResultParticipant UserParticipant { get; set; }
        private static CancellationTokenSource CancelToken { get; set; }

        public ConvertDocs(IProgress<ProgressReportModel> pg, ResultParticipant up)
        {
            PgModel = new ProgressReportModel(TipoTask.GetCreditor);
            Progress = pg;
            UserParticipant = up;
            CancelToken = new CancellationTokenSource();
        }

        public async Task ConvertXmlToPdf(List<Detalle> detalles)
        {
            SetIsRuning(true);
            int c = 0;
            List<Task<IPdfDocument>> tareas = new List<Task<IPdfDocument>>();
            IPdfDocument pdfDocument = null;
            string path = @"C:\Centralizador\Pdf\" + UserParticipant.BusinessName;
            new CreateFile($"{path}");

            await Task.Run(() =>
             {
                 tareas = detalles.Select(async item =>
             {
                 if (item.DTEDef != null)
                 {
                     try
                     {
                         await EncodeTimbre417(item);
                         await HtmlToXmlTransform(item, path);
                     }
                     catch (Exception)
                     {
                         throw new TaskCanceledException();
                     }
                 }
                 c++;
                 float porcent = (float)(100 * c) / detalles.Count;
                 await ReportProgress(porcent, $"Converting doc N° [{item.Folio}] to PDF.    ({c}/{detalles.Count})");
                 return pdfDocument;
             }).ToList();
             }, CancelToken.Token);
            await Task.WhenAll(tareas).ContinueWith(x =>
            {
                SetIsRuning(false);
                Process.Start(path);
            });
        }

        private Task HtmlToXmlTransform(Detalle d, string path)
        {
            IPdfDocument pdfDocument = null;
            return Task.Run(() =>
            {
                XsltArgumentList argumentList = new XsltArgumentList();
                argumentList.AddParam("timbre", "", Path.GetTempPath() + $"\\timbre{d.Folio}.png");
                // XML TO HTML.
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(ServicePdf.TransformObjectToXml(d.DTEDef));
                XslCompiledTransform transform = new XslCompiledTransform();
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(Properties.Resources.EncoderXmlToHtml)))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(Path.GetTempPath() + $"\\invoice{d.Folio}.html"))
                    {
                        transform.Load(xmlReader);
                        transform.Transform(xmlDocument, argumentList, xmlWriter);
                    }
                }
                pdfDocument = Pdf.From(File.ReadAllText(Path.GetTempPath() + $"\\invoice{d.Folio}.html")).OfSize(PaperSize.Letter);
                // SAVE
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                string nomenclatura = d.Folio + "_" + ti.ToTitleCase(d.RznSocRecep.ToLower());
                File.WriteAllBytes(path + "\\" + nomenclatura + ".pdf", pdfDocument.Content());
            });
        }

        private Task EncodeTimbre417(Detalle d)
        {
            DTEDefTypeDocumento doc = (DTEDefTypeDocumento)d.DTEDef.Item;
            return Task.Run(() =>
             {
                 Pdf417Encoder encoder = new Pdf417Encoder
                 {
                     //EncodingControl = EncodingControl.ByteOnly,
                     ErrorCorrection = ErrorCorrectionLevel.Level_5,
                     GlobalLabelIDCharacterSet = "ISO-8859-1",
                     QuietZone = 14,
                     DefaultDataColumns = 14,
                     RowHeight = 6,
                     NarrowBarWidth = 2
                 };
                 encoder.Encode(ServicePdf.TransformObjectToXml(doc.TED).ToString());
                 encoder.SaveBarcodeToPngFile(Path.GetTempPath() + $"\\timbre{d.Folio}.png");
             });
        }

        private static Task ReportProgress(float p, string msg)
        {
            return Task.Run(() =>
            {
                PgModel.PercentageComplete = (int)p;
                PgModel.SetMessage(msg);
                Progress.Report(PgModel);
            });
        }

        public static Task CancelTask()
        {
            return Task.Run(async () =>
            {
                CancelToken.Cancel();
                await ReportProgress(100, "Cancelada!");
                SetIsRuning(false);
            });
        }
    }
}