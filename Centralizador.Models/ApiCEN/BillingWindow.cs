using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

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

        [JsonProperty("created_ts")]
        public DateTime CreatedTs { get; set; }

        [JsonProperty("updated_ts")]
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
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static ResultBillingWindow GetBillingWindowById(ResultPaymentMatrix matrix) // GET
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress,
                Encoding = Encoding.UTF8
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = wc.DownloadString($"api/v1/resources/billing-windows/?id={matrix.BillingWindowId}");
                if (res != null)
                {
                    BillingWindow b = JsonConvert.DeserializeObject<BillingWindow>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (b.Results.Count == 1)
                    {
                        return b.Results[0];

                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// Method return 1 Billing Window.
        /// </summary>
        /// <param name="naturalKey"></param>
        /// <returns></returns>
        public static ResultBillingWindow GetBillingWindowByNaturalKey(DTEDefTypeDocumentoReferencia referencia)
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress,
                Encoding = Encoding.UTF8
            };
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            string r1 = referencia.RazonRef.Substring(0, referencia.RazonRef.IndexOf(']') + 1).TrimStart();
            string r2 = referencia.RazonRef.Substring(0, referencia.RazonRef.IndexOf(']', referencia.RazonRef.IndexOf(']') + 1) + 1);
            r2 = r2.Substring(r2.IndexOf(']') + 1);

            // Controlling lower & upper
            string rznRef = ti.ToTitleCase(r2.ToLower());
            try
            {           
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = wc.DownloadString($"api/v1/resources/billing-windows/?natural_key={r1 + rznRef}");
                if (res != null)
                {
                    BillingWindow b = JsonConvert.DeserializeObject<BillingWindow>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (b.Results.Count == 1)
                    {
                        return b.Results[0];
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
    }
}
