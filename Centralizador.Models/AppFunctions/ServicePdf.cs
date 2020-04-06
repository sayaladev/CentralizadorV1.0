using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Centralizador.Models.Outlook;

namespace Centralizador.Models.AppFunctions
{
    public class ServicePdf
    {
        /// <summary>
        /// Method return a object (Xml to object).
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
        /// Method return a string (Object to Xml).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string TransformObjectToXml(DTEDefType obj)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DTEDefType));
                using (Utf8StringWriter stringWriter = new Utf8StringWriter( ))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
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
    }
}
