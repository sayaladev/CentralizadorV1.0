using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
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
        private IList<Detalle> Detalles { get; set; }

        public ServicePdf(IList<Detalle> detalles)
        {
            Detalles = detalles;
        }


        /// <summary>
        /// Method return a object (Xml 'EnvioDTE' to object).
        /// </summary>
        /// <param name="pathFile"></param>
        /// <returns>Sergio Ayala</returns>        
        public static EnvioDTE TransformXmlEnvioDTEToObject(string pathFile)
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
            catch (Exception)
            {
                return null;
            }

        }

        public static RespuestaDTE TransformXmlRespuestaDTEToObject(string pathFile)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(RespuestaDTE));
                using (StreamReader reader = new StreamReader(pathFile, Encoding.Default))
                {
                    RespuestaDTE document = (RespuestaDTE)deserializer.Deserialize(reader);
                    return document;
                }
            }
            catch (Exception)
            {
                return null;
            }

        }
        public static string TransformObjectToXml(RespuestaDTEResultadoResultadoDTE obj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RespuestaDTEResultadoResultadoDTE), new XmlRootAttribute("ResultadoDTE"));
                using (Utf8StringWriter stringWriter = new Utf8StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
                    {
                        serializer.Serialize(xmlWriter, obj, new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty }));
                    }
                    return stringWriter.ToString();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Method return a object (Xml 'DTE' to object).</returns>
        public static DTEDefType TransformXmlDTEDefTypeToObjectDTE(string filePath)
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
            catch (Exception)
            {
                return null;
            }

        }
        /// <summary>
        /// Method return a object (Xml 'DTE' to object).
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static DTEDefType TransformStringDTEDefTypeToObjectDTE(string file)
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
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// Method return a string (Object DTE to Xml).
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
            catch (Exception)
            {
                return null;
            }
        }
        public sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        /// <summary>
        /// Method return a string (Object TED to Xml).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
            catch (Exception)
            {
                return null;
            }
        }
        public static void ConvertToPdf(Detalle detalle)
        {
            IPdfDocument document;
            string nomenclatura;
            document = ConvertXmlToPdf(detalle);
            //nomenclatura = detalle.RutReceptor + "_" + detalle.Folio;
            nomenclatura = detalle.Folio + "_" + detalle.RutReceptor;
            byte[] content = document.Content();
            try
            {
                File.WriteAllBytes(Path.GetTempPath() + "\\" + nomenclatura + ".pdf", content);
                Process.Start(Path.GetTempPath() + "\\" + nomenclatura + ".pdf");
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void ConvertToPdf(BackgroundWorker bgw)
        {
            bgw.DoWork += Bgw_DoWork;
            if (Detalles.Count > 0)
            {
                // N doc
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Reset();
                dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    bgw.RunWorkerAsync(dialog);
                }
            }

        }
        private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            FolderBrowserDialog dialog = e.Argument as FolderBrowserDialog;
            BackgroundWorker bgw = sender as BackgroundWorker;
            IPdfDocument document;
            int c = 0;
            string nomenclatura;
            foreach (Detalle item in Detalles)
            {
                if (item.DTEDef != null)
                {
                    document = ConvertXmlToPdf(item);
                    byte[] content = document.Content();
                    //nomenclatura = item.RutReceptor + "_" + item.Folio;
                    nomenclatura = item.Folio + "_" + item.RutReceptor;
                    File.WriteAllBytes(dialog.SelectedPath + "\\" + nomenclatura + ".pdf", content);
                }
                // Is Cancel?
                if (bgw.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                c++;
                float porcent = (float)(100 * c) / Detalles.Count;
                bgw.ReportProgress((int)porcent, $"Converting to Pdf... [{item.Folio}] ({c}/{Detalles.Count})");
            }
            Process.Start(dialog.SelectedPath);
        }
        private static IPdfDocument ConvertXmlToPdf(Detalle obj)
        {
            // Timbre Pdf417            
            DTEDefTypeDocumento documento = (DTEDefTypeDocumento)obj.DTEDef.Item;
            try
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
                //encoder.WidthToHeightRatio(1.9);
                encoder.Encode(TransformObjectToXml(documento.TED).ToString());
                encoder.SaveBarcodeToPngFile(Path.GetTempPath() + "\\timbre.png");
                XsltArgumentList argumentList = new XsltArgumentList();
                argumentList.AddParam("timbre", "", Path.GetTempPath() + "\\timbre.png");
                // Xml to Html
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(TransformObjectToXml(obj.DTEDef));
                XslCompiledTransform transform = new XslCompiledTransform();
                using (XmlReader xmlReader = XmlReader.Create(new StringReader(Properties.Resources.EncoderXmlToHtml)))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(Path.GetTempPath() + "\\invoice.html"))
                    {
                        transform.Load(xmlReader);
                        transform.Transform(xmlDocument, argumentList, xmlWriter);
                    }
                }
                IPdfDocument pdfDocument = Pdf.From(File.ReadAllText(Path.GetTempPath() + "\\invoice.html")).OfSize(PaperSize.Letter);
                //pdfDocument.Comressed();
                //pdfDocument.Content();
                return pdfDocument;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
