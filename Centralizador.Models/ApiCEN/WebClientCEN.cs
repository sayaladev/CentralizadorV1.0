using System.Net;
using System.Text;

namespace Centralizador.Models.ApiCEN
{
    public class WebClientCEN
    {
        public static WebClient WebClient { get; set; }

        public WebClientCEN(string baseAdress)
        {
            WebClient = new WebClient
            {
                BaseAddress = baseAdress
            };           
            WebClient.Encoding = Encoding.UTF8;
            WebClient.Proxy = null;
        }
    }
}
