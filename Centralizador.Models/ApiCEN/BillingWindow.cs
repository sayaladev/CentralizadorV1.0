using System;
using System.Collections.Generic;
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

        public static ResultBillingWindow GetBillingWindowById(ResultPaymentMatrix matrix)
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                string res = wc.DownloadString($"billing-windows/?id={matrix.BillingWindowId}");
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
            finally
            {
                wc.Dispose();
            }
            return null;

        }

        /// <summary>
        /// Method return 1 Billing Window.
        /// </summary>
        /// <param name="naturalKey"></param>
        /// <returns></returns>
        public static ResultBillingWindow GetBillingWindowByNaturalKey(string naturalKey)
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                string res = wc.DownloadString($"billing-windows/?natural_key={naturalKey}");
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
            finally
            {
                wc.Dispose();
            }
            return null;
        }
    }
}
