using System.IO;
using System.Xml.Serialization;

using Centralizador.Models.Outlook;

namespace Centralizador.Models.AppFunctions
{
    public class ServicePdf
    {

        public static DTEDefType TransformXmlToObject(string pathFile)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(EnvioDTE));

            using (StreamReader reader = new StreamReader(pathFile))
            {
                EnvioDTE document = (EnvioDTE)deserializer.Deserialize(reader);
                return document.SetDTE.DTE[0];
            }            
        }

        public static string TransformObjectToXml(DTEDefTypeDocumento doc) {
            XmlSerializer serializer = new XmlSerializer(doc.GetType());
            using (StringWriter reader = new StringWriter())
            {
                serializer.Serialize(reader, doc);               
                return reader.ToString();
            }         

        }
    }
}
