using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using Centralizador.Models.ApiSII;

using OpenHtmlToPdf;
using ZXing;
using ZXing.PDF417;

using static Centralizador.Models.Helpers.HEnum;

namespace Centralizador.Models.Helpers
{
    internal class HConvertToPdf
    {
        public static async Task<string> XmlToPdf(Detalle d, string path)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string nomenclatura = path + "\\" + d.Folio + "_" + ti.ToTitleCase(d.RznSocRecep.ToLower());
            return await Task.Run(() =>
              {
                  // XML TO HTML.
                  IPdfDocument pdfDocument = null;
                  XsltArgumentList argumentList = new XsltArgumentList();
                  argumentList.AddParam("timbre", "", Path.GetTempPath() + $"\\timbre{d.Folio}.png");
                  XmlDocument xmlDocument = new XmlDocument();
                  xmlDocument.LoadXml(HSerialize.DTE_To_Xml(d.DTEDef));
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
                  try
                  {
                      File.WriteAllBytes($"{nomenclatura}.pdf", pdfDocument.Content());
                      return $"{nomenclatura}.pdf";
                  }
                  catch (System.IO.IOException)
                  {
                      Random generator = new Random();
                      string r = generator.Next(0, 1000).ToString();
                      File.WriteAllBytes($"{nomenclatura}{r}.pdf", pdfDocument.Content());
                      return $"{nomenclatura}{r}.pdf";
                  }
              });
            // return  path + "\\" + nomenclatura + "_1.pdf";
        }

        public static Task EncodeTimbre417(Detalle d, TipoTask task)
        {
            return Task.Run(() =>
            {
                string ted = null;
                switch (task)
                {
                    case TipoTask.Creditor:
                        ted = d.DteInfoRefLast.FirmaDTE;
                        break;

                    case TipoTask.Debtor:
                        DTEDefTypeDocumento doc = (DTEDefTypeDocumento)d.DTEDef.Item;
                        ted = HSerialize.TED_To_Xml(doc.TED);
                        break;
                }

                BarcodeWriter timbre417 = new BarcodeWriter
                {
                    Format = BarcodeFormat.PDF_417,
                    Options = new PDF417EncodingOptions()
                    {
                        ErrorCorrection = ZXing.PDF417.Internal.PDF417ErrorCorrectionLevel.L5,
                        Height = 3,
                        Width = 9,
                        Compaction = ZXing.PDF417.Internal.Compaction.BYTE
                    }
                };
                timbre417
                    .Write(ted)
                    .Save(Path.GetTempPath() + $"\\timbre{d.Folio}.png");
            });
        }
    }
}