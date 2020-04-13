using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

using Centralizador.Models.ApiSII;
using Centralizador.Models.Outlook;

using OpenHtmlToPdf;

using Pdf417EncoderLibrary;



namespace Centralizador.Models.AppFunctions
{
    public class ServicePdf
    {
        /// <summary>
        /// Method return a object (Xml 'EnvioDTE' to object).
        /// </summary>
        /// <param name="pathFile"></param>
        /// <returns></returns>
        public static EnvioDTE TransformXmlToObject(string pathFile)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(EnvioDTE));
                using (StreamReader reader = new StreamReader(pathFile, Encoding.Default))
                {
                    EnvioDTE document = (EnvioDTE)deserializer.Deserialize(reader);
                    return document;
                }
            }
            catch (System.Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// Methos return a object (Xml 'DTE' to object).
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static DTEDefType TransformXmlToObjectDTE(string filePath)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(DTEDefType));
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    DTEDefType document = (DTEDefType)deserializer.Deserialize(reader);
                    return document;
                }
            }
            catch (System.Exception)
            {
                return null;
            }

        }
        /// <summary>
        /// Method return a object (Xml 'DTE' to object).
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static DTEDefType TransformXmlStringToObjectDTE(string file)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(file);
                xmlDoc.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
                XmlSerializer deserializer = new XmlSerializer(typeof(DTEDefType));
                using (StringReader reader = new StringReader(xmlDoc.InnerXml))
                {
                    DTEDefType document = (DTEDefType)deserializer.Deserialize(reader);
                    return document;
                }
            }
            catch (System.Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// Method return a string (Object to Xml).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string TransformObjectToXml(DTEDefType obj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DTEDefType));
                using (Utf8StringWriter stringWriter = new Utf8StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
                    {
                        serializer.Serialize(xmlWriter, obj);
                    }
                    return stringWriter.ToString();
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }
        public sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
        public static string TransformObjectToXml(DTEDefTypeDocumentoTED obj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DTEDefTypeDocumentoTED), new XmlRootAttribute("TED"));
                using (Utf8StringWriter stringWriter = new Utf8StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
                    {
                        serializer.Serialize(xmlWriter, obj, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                    }
                    return stringWriter.ToString();
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }


        public static void GetPdfDocument(Detalle obj)
        {
            // Timbre Pdf417            
            DTEDefTypeDocumento documento = (DTEDefTypeDocumento)obj.DTEDef.Item;
            try
            {
                Pdf417Encoder encoder = new Pdf417Encoder
                {
                    //EncodingControl = EncodingControl.ByteOnly,
                    //ErrorCorrection = ErrorCorrectionLevel.Level_5,
                    GlobalLabelIDCharacterSet = "ISO-8859-1",
                    //QuietZone = 14,
                    DefaultDataColumns = 14,
                    RowHeight = 6,
                    NarrowBarWidth = 2
                };
                encoder.WidthToHeightRatio(1.9);
                encoder.Encode(TransformObjectToXml(documento.TED).ToString());
                encoder.SaveBarcodeToPngFile(Path.GetTempPath() + "\\timbre.png");

                XsltArgumentList argumentList = new XsltArgumentList();
                argumentList.AddParam("timbre", "", Path.GetTempPath() + "\\timbre.png");

                // Xml to Html
                XmlDocument xmlDocument = new XmlDocument();
                // DTEDefTypeDocumento dteee = (DTEDefTypeDocumento)obj.DTEDef.Item;

                xmlDocument.LoadXml(TransformObjectToXml(obj.DTEDef));
                XslCompiledTransform transform = new XslCompiledTransform();
                //string RemoveAllNamespaces(string xmlDocument);
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(Properties.Resources.EncoderXmlToHtml)))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(Path.GetTempPath() + "\\invoice.html"))
                    {
                        transform.Load(xmlReader);
                        transform.Transform(xmlDocument, argumentList, xmlWriter);
                    }
                }

                // Html to Pdf
                //PdfDocument pdfDocument = new PdfDocument();
                //HtmlToPdf htmlToPdf = new HtmlToPdf();
                //htmlToPdf.Options.PdfPageSize = PdfPageSize.Letter;
                //pdfDocument = htmlToPdf.ConvertUrl(Path.GetTempPath() + "\\invoice.html");
                //pdfDocument.Save(Path.GetTempPath() + "\\invoice.pdf");
                //pdfDocument.Close();
                //System.Diagnostics.Process.Start(Path.GetTempPath() + "\\invoice.pdf");

                // OpenHtmlToPdf


                IPdfDocument pdfDocument = Pdf.From(File.ReadAllText(Path.GetTempPath() + "\\invoice.html")).OfSize(PaperSize.Letter);
                pdfDocument.WithGlobalSetting("margin.left", "0.5cm");
                //pdfDocument.Comressed();
                //pdfDocument.Content();
                byte[] content = pdfDocument.Content();
                File.WriteAllBytes(Path.GetTempPath() + "\\invoice.pdf", content);
                System.Diagnostics.Process.Start(Path.GetTempPath() + "\\invoice.pdf");


                //return obj;
            }


            catch (System.Exception)
            {

                throw;
            }
        }
    }
}
