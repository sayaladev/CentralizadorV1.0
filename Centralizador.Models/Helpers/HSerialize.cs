using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using Centralizador.Models.ApiSII;

namespace Centralizador.Models.Helpers
{
    public static class HSerialize
    {
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

        public static async Task<EnvioDTE> TransformStringDTEDefTypeToObjectDTEAsync(XDocument xDoc)
        {
            try
            {
                XmlDocument xmlDoc = ToXmlDocument(xDoc);
                xmlDoc.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
                XmlSerializer deserializer = new XmlSerializer(typeof(EnvioDTE));
                using (StringReader reader = new StringReader(xmlDoc.InnerXml))
                {
                    EnvioDTE document = (EnvioDTE)deserializer.Deserialize(reader);
                    return await Task.FromResult(document);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static XmlDocument ToXmlDocument(XDocument xDocument)
        {
            XmlDocument xmlDocument = new XmlDocument();
            using (XmlReader xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using (XmlNodeReader nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        public static string TransformObjectToXmlDte(DTEDefType obj) // DTE INSERT REF INTO XML AND UPDATE DB => FILED SEND TO CLIENTS
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DTEDefType), new XmlRootAttribute("DTE"));
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

        public static string TransformObjectToXmlForCen(DTEDefType obj) // POR ALGUNA RAZÓN ESTÁ ENVIANDO EL DTE AL CEN SIN <?xml version="1.0" encoding="utf-8"?>
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DTEDefType));
                //var encoding = Encoding.GetEncoding("ISO-8859-1");
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = false
                };
                using (Utf8StringWriter stringWriter = new Utf8StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings)) // OmitXmlDecla : <?xml version="1.0" encoding="utf-8"?>
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
            //IPdfDocument document;
            //string nomenclatura;
            //document = ConvertXmlToPdf(detalle);

            //TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            //nomenclatura = detalle.Folio + "_" + ti.ToTitleCase(detalle.RznSocRecep.ToLower());
            //byte[] content = document.Content();
            //try
            //{
            //    File.WriteAllBytes(Path.GetTempPath() + "\\" + nomenclatura + ".pdf", content);
            //    Process.Start(Path.GetTempPath() + "\\" + nomenclatura + ".pdf");
            //}
            //catch (Exception)
            //{
            //    throw;
            //}
        }

        //public void ConvertToPdf(BackgroundWorker bgw, string path)
        //{
        //   // bgw.DoWork += Bgw_DoWork;
        //    //bgw.RunWorkerAsync(path);
        //}

        //private void Bgw_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    string dialog = e.Argument as string;
        //    BackgroundWorker bgw = sender as BackgroundWorker;
        //    IPdfDocument document;
        //    int c = 0;
        //    string nomenclatura;
        //    foreach (Detalle item in Detalles)
        //    {
        //        if (item.DTEDef != null)
        //        {
        //            document = ConvertXmlToPdf(item);
        //            byte[] content = document.Content();
        //            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
        //            nomenclatura = item.Folio + "_" + ti.ToTitleCase(item.RznSocRecep.ToLower());
        //            File.WriteAllBytes(dialog + "\\" + nomenclatura + ".pdf", content);
        //        }
        //        // Is Cancel?
        //        if (bgw.CancellationPending)
        //        {
        //            e.Cancel = true;
        //            break;
        //        }
        //        c++;
        //        float porcent = (float)(100 * c) / Detalles.Count;
        //        bgw.ReportProgress((int)porcent, $"Converting to Pdf... [{item.Folio}] ({c}/{Detalles.Count})");
        //    }
        //    ProgressReportModel.SetIsRuning(false);
        //    Process.Start(dialog);
        //}
    }
}