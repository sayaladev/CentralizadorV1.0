using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

namespace Centralizador.Models.ApiCEN
{
    public class ResultBillingWindow
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("natural_key")]
        public string NaturalKey { get; set; }

        [JsonProperty("billing_type")]
        public int BillingType { get; set; }

        [JsonProperty("periods")]
        public List<string> Periods { get; set; }

        [JsonIgnore]
        public DateTime CreatedTs { get; set; }

        [JsonIgnore]
        public DateTime UpdatedTs { get; set; }
    }

    public class BillingWindow : CustomHead
    {
        [JsonProperty("results")]
        public List<ResultBillingWindow> Results { get; set; }

        public static async Task<ResultBillingWindow> GetBillingWindowByIdAsync(ResultPaymentMatrix matrix)
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, $"api/v1/resources/billing-windows/?id={matrix.BillingWindowId}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri); // GET
                    if (res != null)
                    {
                        BillingWindow b = JsonConvert.DeserializeObject<BillingWindow>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        return b.Results[0];
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        public static async Task<ResultBillingWindow> GetBillingWindowByNaturalKeyAsync(DTEDefTypeDocumentoReferencia referencia)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string r1 = referencia.RazonRef.Substring(0, referencia.RazonRef.IndexOf(']') + 1).TrimStart();
            string r2 = referencia.RazonRef.Substring(0, referencia.RazonRef.IndexOf(']', referencia.RazonRef.IndexOf(']') + 1) + 1);
            r2 = r2.Substring(r2.IndexOf(']') + 1);

            // Controlling lower & upper
            string rznRef = ti.ToTitleCase(r2.ToLower());

            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, $"api/v1/resources/billing-windows/?natural_key={r1 + rznRef}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri);
                    if (res != null)
                    {
                        BillingWindow b = JsonConvert.DeserializeObject<BillingWindow>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (b.Count > 0)
                        {
                            return b.Results[0];
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }
    }
}