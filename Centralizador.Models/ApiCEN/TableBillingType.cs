using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{
    public class ResultBilingType
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("natural_key")]
        public string NaturalKey { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("system_prefix")]
        public string SystemPrefix { get; set; }

        [JsonProperty("description_prefix")]
        public string DescriptionPrefix { get; set; }

        [JsonProperty("payment_window")]
        public int PaymentWindow { get; set; }

        [JsonProperty("department")]
        public int Department { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }

    public class BilingType : CustomHead
    {
        [JsonProperty("results")]
        public List<ResultBilingType> Results { get; set; }

        public static async Task<List<ResultBilingType>> GetBilinTypesAsync()
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, $"api/v1/resources/billing-types");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri); // GET
                    if (res != null)
                    {
                        BilingType bilingType = JsonConvert.DeserializeObject<BilingType>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        return bilingType.Results;
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