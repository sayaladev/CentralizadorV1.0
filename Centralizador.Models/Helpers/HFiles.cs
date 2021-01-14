using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using Centralizador.Models.ApiSII;

using OpenHtmlToPdf;
using ZXing;
using ZXing.PDF417;

namespace Centralizador.Models.Helpers
{
    internal class HFiles
    {
        public static Task HtmlToXmlTransform(Detalle d, string path)
        {
            return Task.Run(() =>
            {
                // XML TO HTML.
                IPdfDocument pdfDocument = null;
                XsltArgumentList argumentList = new XsltArgumentList();
                argumentList.AddParam("timbre", "", Path.GetTempPath() + $"\\timbre{d.Folio}.png");
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(HSerialize.TransformObjectToXml(d.DTEDef));
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

        public static Task EncodeTimbre417(Detalle d)
        {
            return Task.Run(() =>
            {
                string ted = d.DteInfoRefLast.FirmaDTE;
                BarcodeWriter timbre417 = new BarcodeWriter
                {
                    Format = BarcodeFormat.PDF_417,
                    Options = new PDF417EncodingOptions()
                    {
                        ErrorCorrection = ZXing.PDF417.Internal.PDF417ErrorCorrectionLevel.L5,
                        Height = 3,
                        Width = 9
                    }
                };
                timbre417
                    .Write(ted)
                    .Save(Path.GetTempPath() + $"\\timbre{d.Folio}.png");
            });
        }
    }
}