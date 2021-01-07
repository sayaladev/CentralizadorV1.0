using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{
    public class ResultPaymentExecution
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("instruction")]
        public int Instruction { get; set; }

        [JsonProperty("execution")]
        public int Execution { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("is_net_amount")]
        public bool IsNetAmount { get; set; }

        [JsonProperty("dtes")]
        public IList<int> Dtes { get; set; }

        [JsonProperty("created_ts")]
        public DateTime CreatedTs { get; set; }

        [JsonProperty("updated_ts")]
        public DateTime UpdatedTs { get; set; }
    }

    public class PaymentExecution : CustomHead
    {
        [JsonProperty("results")]
        public IList<ResultPaymentExecution> Results { get; set; }

        public static async Task<ResultPaymentExecution> GetPayId(ResultInstruction instruction)
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, $"/api/v1/resources/payment-execution-payment-instructions/?instruction={instruction.Id}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri);  // GET
                    if (res != null)
                    {
                        PaymentExecution p = JsonConvert.DeserializeObject<PaymentExecution>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (p.Count == 1)
                        {
                            return p.Results[0];
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

        public static async Task<string> DeletePayAsync(ResultPaymentExecution paymentExecution, string tokenCen)
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, $"api/v1/operations/payments/{paymentExecution.Execution}/delete/");
                    string d = "";
                    wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                    NameValueCollection postData = new NameValueCollection() { { "data", d } };

                    byte[] res = await wc.UploadValuesTaskAsync(uri, WebRequestMethods.Http.Post, postData); // POST
                    if (res != null)
                    {
                        string json = Encoding.UTF8.GetString(res);
                        DeletePaymentResult p = JsonConvert.DeserializeObject<DeletePaymentResult>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                        if (p != null)
                        {
                            return p.Result.DeletedPaymentId;
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

        private class DeletedObjectsDetail
        {
            [JsonProperty("payments.PaymentExecutionPaymentInstructionDTE")]
            public int PaymentsPaymentExecutionPaymentInstructionDTE { get; set; }

            [JsonProperty("payments.PaymentExecutionPaymentInstruction")]
            public int PaymentsPaymentExecutionPaymentInstruction { get; set; }

            [JsonProperty("payments.PaymentExecution")]
            public int PaymentsPaymentExecution { get; set; }
        }

        private class Result
        {
            [JsonProperty("deleted_objects_count")]
            public int DeletedObjectsCount { get; set; }

            [JsonProperty("deleted_objects_detail")]
            public DeletedObjectsDetail DeletedObjectsDetail { get; set; }

            [JsonProperty("deleted_payment_id")]
            public string DeletedPaymentId { get; set; }
        }

        private class DeletePaymentResult
        {
            [JsonProperty("result")]
            public Result Result { get; set; }

            [JsonProperty("errors")]
            public IList<object> Errors { get; set; }

            [JsonProperty("operation")]
            public int Operation { get; set; }
        }
    }
}