using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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

    public class BilingType
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public IList<ResultBilingType> Results { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<IList<ResultBilingType>> GetBilinTypesAsync() 
        {
            IList<ResultBilingType> billingTypes = new List<ResultBilingType>();
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/billing-types");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri).ConfigureAwait(false); // GET
                    if (res != null)
                    {
                        BilingType bilingType = JsonConvert.DeserializeObject<BilingType>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });                     
                            billingTypes = bilingType.Results;                        
                    }
                }
            }
            catch (Exception)
            {
                // Error Exception
                return null;
            }           
            return billingTypes;
        }
    }


}
