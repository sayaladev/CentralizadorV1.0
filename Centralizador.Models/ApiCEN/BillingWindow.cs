using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

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
        public IList<string> Periods { get; set; }

        [JsonIgnore]
        public DateTime CreatedTs { get; set; }

        [JsonIgnore]
        public DateTime UpdatedTs { get; set; }
    }

    public class BillingWindow
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public IList<ResultBillingWindow> Results { get; set; }

        /// <summary>
        /// Get 1 'Ventana de Facturación' from CEN API
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static async Task<ResultBillingWindow> GetBillingWindowByIdAsync(ResultPaymentMatrix matrix)
        {
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/billing-windows/?id={matrix.BillingWindowId}");
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


        /// <summary>
        /// Get 1 'Ventana de Facturación' from CEN API
        /// </summary>
        /// <param name="referencia"></param>
        /// <returns></returns>
        public static ResultBillingWindow GetBillingWindowByNaturalKey(DTEDefTypeDocumentoReferencia referencia)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string r1 = referencia.RazonRef.Substring(0, referencia.RazonRef.IndexOf(']') + 1).TrimStart();
            string r2 = referencia.RazonRef.Substring(0, referencia.RazonRef.IndexOf(']', referencia.RazonRef.IndexOf(']') + 1) + 1);
            r2 = r2.Substring(r2.IndexOf(']') + 1);

            // Controlling lower & upper
            string rznRef = ti.ToTitleCase(r2.ToLower());

            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/billing-windows/?natural_key={r1 + rznRef}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = wc.DownloadString(uri);
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
