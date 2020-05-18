using System.Net;
using System.Text;

namespace Centralizador.Models.ApiCEN
{
    public static class WebClientCEN
    {
        public static WebClient WebClient { get; set; }

        static WebClientCEN()
        {
            WebClient = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            WebClient.Headers[HttpRequestHeader.ContentType] = "application/json";
            WebClient.Encoding = Encoding.UTF8;
            WebClient.Proxy = null;
        }
    }
}
