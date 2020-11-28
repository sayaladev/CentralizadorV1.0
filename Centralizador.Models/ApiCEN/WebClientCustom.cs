using System.Net;
using System.Text;

namespace Centralizador.Models.ApiCEN
{
    internal class WebClientCustom : WebClient
    {
        public WebClientCustom()
        {

            Encoding = Encoding.UTF8;

            // Headers[HttpRequestHeader.ContentType] = "application/json";

            // Prevent NET::ERR_CERT_DATE_INVALID / CEN WEB PAGE            
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public void Getalgo() { 
        
        }
    }
}
