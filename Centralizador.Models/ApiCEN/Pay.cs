using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{
    public class ResultPay
    {
        [JsonProperty("actual_collector")]
        public string ActualCollector { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("creditor")]
        public int Creditor { get; set; }

        [JsonProperty("debtor")]
        public int Debtor { get; set; }

        [JsonProperty("instruction_amount_tuples")]
        public List<List<int>> InstructionAmountTuples { get; set; }

        [JsonProperty("erp_a")]
        public string ErpA { get; set; }

        [JsonProperty("erp_b")]
        public string ErpB { get; set; }

        [JsonProperty("payment_dt")]
        public string PaymentDt { get; set; }

        [JsonProperty("transaction_type")]
        public int TransactionType { get; set; }

        [JsonProperty("dtes")]
        public List<long> Dtes { get; set; }
    }

    public class Pay : CustomHead
    {
        [JsonProperty("result")]
        public ResultPay Result { get; set; }

        public static async Task<ResultPay> SendPayAsync(ResultPay pay, string tokenCen)
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/operations/payments/create/");
                    string d = JsonConvert.SerializeObject(pay);
                    wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                    NameValueCollection postData = new NameValueCollection() { { "data", d } };
                    byte[] res = await wc.UploadValuesTaskAsync(uri, WebRequestMethods.Http.Post, postData); // POST
                    if (res != null)
                    {
                        string json = Encoding.UTF8.GetString(res);
                        Pay p = JsonConvert.DeserializeObject<Pay>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                        if (p != null)
                        {
                            return p.Result;
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

        public enum StatusPay
        {
            NoPagado = 1,
            Pagado = 2,
            PagadoAtraso = 3
        }
    }
}