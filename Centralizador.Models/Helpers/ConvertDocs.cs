//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Xml;
//using System.Xml.Linq;
//using System.Xml.Xsl;
//using Centralizador.Models.ApiCEN;
//using Centralizador.Models.ApiSII;
//using Centralizador.Models.Helpers;
//using OpenHtmlToPdf;
//using ZXing;
//using ZXing.PDF417;
//using static Centralizador.Models.HPgModel;

//namespace Centralizador.Models.Helpers
//{
//    public class ConvertDocs
//    {
//        private static HPgModel PgModel { get; set; }
//        private static IProgress<HPgModel> Progress { get; set; }
//        private ResultParticipant UserParticipant { get; set; }
//        private static CancellationTokenSource CancelToken { get; set; }

//        public ConvertDocs(IProgress<HPgModel> pg, ResultParticipant up)
//        {
//            PgModel = new HPgModel(TipoTask.GetCreditor);
//            Progress = pg;
//            UserParticipant = up;
//            CancelToken = new CancellationTokenSource();
//        }

//        public async Task ConvertXmlToPdf(List<Detalle> detalles)
//        {
//            IsBussy = true;
//            int c = 0;
//            List<Task<IPdfDocument>> tareas = new List<Task<IPdfDocument>>();
//            IPdfDocument pdfDocument = null;
//            string path = @"C:\Centralizador\Pdf\" + UserParticipant.BusinessName;
//            new CreateFile($"{path}");

//            await Task.Run(() =>
//            {
//                tareas = detalles.Select(async item =>
//            {
//                if (item.DTEDef != null)
//                {
//                    await EncodeTimbre417(item).ContinueWith(async x =>
//                     {
//                         await HtmlToXmlTransform(item, path);
//                     });
//                }
//                c++;
//                float porcent = (float)(100 * c) / detalles.Count;
//                await ReportProgress(porcent, $"Converting doc N° [{item.Folio}] to PDF.    ({c}/{detalles.Count})");
//                return pdfDocument;
//            }).ToList();
//            }, CancelToken.Token);
//            await Task.WhenAll(tareas).ContinueWith(x =>
//            {
//                SetIsRuning(false);
//                Process.Start(path);
//            });
//        }

//        private Task HtmlToXmlTransform(Detalle d, string path)
//        {
//            return Task.Run(() =>
//            {
//                // XML TO HTML.
//                IPdfDocument pdfDocument = null;
//                XsltArgumentList argumentList = new XsltArgumentList();
//                argumentList.AddParam("timbre", "", Path.GetTempPath() + $"\\timbre{d.Folio}.png");
//                XmlDocument xmlDocument = new XmlDocument();
//                xmlDocument.LoadXml(HSerialize.TransformObjectToXml(d.DTEDef));
//                XslCompiledTransform transform = new XslCompiledTransform();
//                using (XmlReader xmlReader = XmlReader.Create(new StringReader(Properties.Resources.EncoderXmlToHtml)))
//                {
//                    using (XmlWriter xmlWriter = XmlWriter.Create(Path.GetTempPath() + $"\\invoice{d.Folio}.html"))
//                    {
//                        transform.Load(xmlReader);
//                        transform.Transform(xmlDocument, argumentList, xmlWriter);
//                    }
//                }
//                pdfDocument = Pdf.From(File.ReadAllText(Path.GetTempPath() + $"\\invoice{d.Folio}.html")).OfSize(PaperSize.Letter);
//                // SAVE
//                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
//                string nomenclatura = d.Folio + "_" + ti.ToTitleCase(d.RznSocRecep.ToLower());
//                File.WriteAllBytes(path + "\\" + nomenclatura + ".pdf", pdfDocument.Content());
//            });
//        }

//        private Task EncodeTimbre417(Detalle d)
//        {
//            return Task.Run(() =>
//             {
//                 string ted = d.DteInfoRefLast.FirmaDTE;
//                 BarcodeWriter timbre417 = new BarcodeWriter
//                 {
//                     Format = BarcodeFormat.PDF_417,
//                     Options = new PDF417EncodingOptions()
//                     {
//                         ErrorCorrection = ZXing.PDF417.Internal.PDF417ErrorCorrectionLevel.L5,
//                         Height = 3,
//                         Width = 9
//                     }
//                 };
//                 timbre417
//                     .Write(ted)
//                     .Save(Path.GetTempPath() + $"\\timbre{d.Folio}.png");
//             });
//        }

//        private static Task ReportProgress(float p, string msg)
//        {
//            return Task.Run(() =>
//            {
//                PgModel.PercentageComplete = (int)p;
//                PgModel.SetMessage(msg);
//                Progress.Report(PgModel);
//            });
//        }

//        public static Task CancelTask()
//        {
//            return Task.Run(async () =>
//            {
//                CancelToken.Cancel();
//                await ReportProgress(100, "Cancelada!");
//                SetIsRuning(false);
//            });
//        }

//        public async Task ConvertXmlToPdf2(List<Detalle> detalles)
//        {
//            SetIsRuning(true);
//            int c = 0;
//            List<Task<IPdfDocument>> tareas = new List<Task<IPdfDocument>>();
//            string path = @"C:\Centralizador\Pdf\" + UserParticipant.BusinessName;
//            new CreateFile($"{path}");
//            IPdfDocument pdfDocument = null;
//            await Task.Run(() =>
//            {
//                tareas = detalles.Select(async item =>
//                {
//                    await Task.Run(() =>
//                     {
//                         if (item.DTEDef != null)
//                         {
//                             string ted = item.DteInfoRefLast.FirmaDTE;
//                             BarcodeWriter timbre417 = new BarcodeWriter
//                             {
//                                 Format = BarcodeFormat.PDF_417,
//                                 Options = new PDF417EncodingOptions()
//                                 {
//                                     ErrorCorrection = ZXing.PDF417.Internal.PDF417ErrorCorrectionLevel.L5,
//                                     Height = 3,
//                                     Width = 9
//                                 }
//                             };
//                             timbre417
//                                .Write(ted)
//                                .Save(Path.GetTempPath() + $"\\timbre{item.Folio}.png");

//                             // XML TO HTML.
//                             XsltArgumentList argumentList = new XsltArgumentList();
//                             argumentList.AddParam("timbre", "", Path.GetTempPath() + $"\\timbre{item.Folio}.png");
//                             XmlDocument xmlDocument = new XmlDocument();
//                             xmlDocument.LoadXml(HSerialize.TransformObjectToXml(item.DTEDef));
//                             XslCompiledTransform transform = new XslCompiledTransform();
//                             using (XmlReader xmlReader = XmlReader.Create(new StringReader(Properties.Resources.EncoderXmlToHtml)))
//                             {
//                                 using (XmlWriter xmlWriter = XmlWriter.Create(Path.GetTempPath() + $"\\invoice{item.Folio}.html"))
//                                 {
//                                     transform.Load(xmlReader);
//                                     transform.Transform(xmlDocument, argumentList, xmlWriter);
//                                 }
//                             }
//                             pdfDocument = Pdf.From(File.ReadAllText(Path.GetTempPath() + $"\\invoice{item.Folio}.html")).OfSize(PaperSize.Letter);
//                             // SAVE
//                             TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
//                             string nomenclatura = item.Folio + "_" + ti.ToTitleCase(item.RznSocRecep.ToLower());
//                             File.WriteAllBytes(path + "\\" + nomenclatura + ".pdf", pdfDocument.Content());
//                         }
//                     });
//                    c++;
//                    float porcent = (float)(100 * c) / detalles.Count;
//                    await ReportProgress(porcent, $"Converting doc N° [{item.Folio}] to PDF.    ({c}/{detalles.Count})");
//                    return pdfDocument;
//                }).ToList();
//            }, CancelToken.Token);
//            await Task.WhenAll(tareas).ContinueWith(x =>
//            {
//                SetIsRuning(false);
//                Process.Start(path);
//            });
//        }
//    }
//}