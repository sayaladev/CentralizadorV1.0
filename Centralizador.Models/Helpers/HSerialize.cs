using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using Centralizador.Models.ApiSII;

namespace Centralizador.Models.Helpers
{
    public static class HSerialize
    {
        // DTEDefType = DTE

        #region XML TO OBJECT

        public static DTEDefType DTE_To_Object(string s) // DTE RECIBIDO Y EL DE SOFTLAND
        {
            try
            {
                if (s.StartsWith(@"C:\Centralizador\Inbox\"))
                {
                    // FILE ON PATH.
                    using (StreamReader reader = new StreamReader(s, Encoding.UTF8))
                    {
                        return (DTEDefType)new XmlSerializer(typeof(DTEDefType)).Deserialize(reader);
                    }
                }
                else
                {
                    // FILE ON STRING.
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(s);
                    xmlDoc.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
                    using (StringReader reader = new StringReader(xmlDoc.InnerXml))
                    {
                        return (DTEDefType)new XmlSerializer(typeof(DTEDefType)).Deserialize(reader);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static EnvioDTE ENVIODTE_To_Object(XDocument xDoc) // ARCHIVO ENVIODTE QUE LLEGA POR EMAIL
        {
            try
            {
                XmlDocument xmlDoc = ToXmlDocument(xDoc);
                xmlDoc.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
                using (StringReader reader = new StringReader(xmlDoc.InnerXml))
                {
                    return (EnvioDTE)new XmlSerializer(typeof(EnvioDTE)).Deserialize(reader);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion XML TO OBJECT

        #region HELPER

        public sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
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

        #endregion HELPER

        #region OBJECT TO XML

        public static string DTE_To_Xml(DTEDefType obj) // AL CONVERTIR A PDF & DTE RECIBIDOS
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

        public static string TED_To_Xml(DTEDefTypeDocumentoTED obj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DTEDefTypeDocumentoTED), new XmlRootAttribute("TED"));
                using (Utf8StringWriter stringWriter = new Utf8StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
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

        #endregion OBJECT TO XML
    }
}