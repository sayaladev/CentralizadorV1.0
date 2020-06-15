using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{

    public class ResultPaymentMatrix
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("auxiliary_data")]
        public AuxiliaryData AuxiliaryData { get; set; }

        [JsonIgnore]
        public DateTime CreatedTs { get; set; }

        [JsonIgnore]
        public DateTime UpdatedTs { get; set; }

        [JsonProperty("payment_type")]
        public string PaymentType { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("payment_file")]
        public string PaymentFile { get; set; }

        [JsonProperty("letter_code")]
        public string LetterCode { get; set; }

        [JsonProperty("letter_year")]
        public int LetterYear { get; set; }

        [JsonProperty("letter_file")]
        public string LetterFile { get; set; }

        [JsonProperty("matrix_file")]
        public string MatrixFile { get; set; }

        [JsonProperty("publish_date")]
        public DateTime PublishDate { get; set; }

        [JsonProperty("payment_days")]
        public int PaymentDays { get; set; }

        [JsonProperty("payment_date")]
        public object PaymentDate { get; set; }

        [JsonProperty("billing_date")]
        public string BillingDate { get; set; }

        [JsonProperty("payment_window")]
        public int PaymentWindow { get; set; }

        [JsonProperty("natural_key")]
        public string NaturalKey { get; set; }

        [JsonProperty("reference_code")]
        public string ReferenceCode { get; set; }

        [JsonProperty("billing_window")]
        public int BillingWindowId { get; set; }

        [JsonProperty("payment_due_type")]
        public int PaymentDueType { get; set; }

        //New 
        public ResultBillingWindow BillingWindow { get; set; }

    }

    public class PaymentMatrix
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public IList<ResultPaymentMatrix> Results { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static async Task<IList<ResultPaymentMatrix>> GetPaymentMatrixAsync(DateTime date)
        {
            DateTime createdBefore = date.AddMonths(1);
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/payment-matrices/?created_after={string.Format("{0:yyyy-MM-dd}", date)}&created_before={string.Format("{0:yyyy-MM-dd}", createdBefore)}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri).ConfigureAwait(false); // GET
                    if (res != null)
                    {
                        PaymentMatrix p = JsonConvert.DeserializeObject<PaymentMatrix>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (p.Results.Count > 0)
                        {
                            return p.Results;
                        }
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
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static async Task<IList<ResultPaymentMatrix>> GetPaymentMatrixByBillingWindowIdAsync(ResultBillingWindow window)
        {
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/payment-matrices/?billing_window={window.Id}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri); // GET
                    if (res != null)
                    {
                        PaymentMatrix p = JsonConvert.DeserializeObject<PaymentMatrix>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (p.Results.Count > 0)
                        {
                            foreach (ResultPaymentMatrix item in p.Results)
                            {
                                item.BillingWindow = window;
                            }
                            return p.Results;
                        }
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
        /// 
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public static async Task<ResultPaymentMatrix> GetPaymentMatrixByIdAsync(ResultInstruction instruction)
        {  
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/payment-matrices/?id={instruction.PaymentMatrixId}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri); // GET
                    if (res != null)
                    {
                        PaymentMatrix p = JsonConvert.DeserializeObject<PaymentMatrix>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (p.Results.Count == 1)
                        {
                            return p.Results[0];
                        }
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
