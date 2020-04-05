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
            XmlSerializer deserializer = new XmlSerializer(typeof(EnvioDTE));
            using (StreamReader reader = new StreamReader(pathFile))
            {
                EnvioDTE document = (EnvioDTE)deserializer.Deserialize(reader);
                return document;
            }            
        }

        public static string TransformObjectToXml(DTEDefTypeDocumento doc) {
            XmlSerializer serializer = new XmlSerializer(typeof(DTEDefTypeDocumento), new XmlRootAttribute("DTE"));
       
            using (UTF8StringWriter stringWriter = new UTF8StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true}))
                {
                    serializer.Serialize(xmlWriter, doc);
                }

                return stringWriter.ToString();
            }


            //using (StringWriter reader = new StringWriter())
            //{
                

            //    serializer.Serialize(reader, doc);               
            //    return reader.ToString();
            //}         

        }

        public class UTF8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get
                {
                    return Encoding.UTF8;
                }
            }
        }
    }
}
