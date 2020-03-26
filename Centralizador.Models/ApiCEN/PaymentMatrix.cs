﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{

    public class ResultPaymentMatrix
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("auxiliary_data")]
        public AuxiliaryData AuxiliaryData { get; set; }

        [JsonProperty("created_ts")]
        public DateTime CreatedTs { get; set; }

        [JsonProperty("updated_ts")]
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
        public string PublishDate { get; set; }

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
        public int BillingWindow { get; set; }

        [JsonProperty("payment_due_type")]
        public int PaymentDueType { get; set; }
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

        public static IList<ResultPaymentMatrix> GetPaymentMatrix(DateTime date)
        {
            DateTime createdBefore = date.AddMonths(1);
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                string res = wc.DownloadString($"payment-matrices/?created_after={string.Format("{0:yyyy-MM-dd}", date)}&created_before={string.Format("{0:yyyy-MM-dd}", createdBefore)}");
                if (res != null)
                {
                    PaymentMatrix p = JsonConvert.DeserializeObject<PaymentMatrix>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    return p.Results;
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

        #region Testing Async



        //private static readonly HttpClient client = new HttpClient();
        //public static async Task<IList<ResultPaymentMatrix>> GetPaymentMatrixAsync(DateTime date)
        //{
        //    DateTime createdBefore = date.AddMonths(1);
        //    client.BaseAddress =  new Uri(Properties.Settings.Default.BaseAddress);
        //    client.DefaultRequestHeaders.Accept.Clear();          
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    try
        //    {
        //        HttpResponseMessage res = await client.GetAsync($"payment-matrices/?created_after={string.Format("{0:yyyy-MM-dd}", date)}&created_before={string.Format("{0:yyyy-MM-dd}", createdBefore)}").ConfigureAwait(false);
        //        string jSon = await res.Content.ReadAsStringAsync();

        //        if (res.IsSuccessStatusCode)
        //        {
        //            PaymentMatrix p = JsonConvert.DeserializeObject<PaymentMatrix>(jSon, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        //            return p.Results;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //    finally
        //    {

        //    }
        //    return null;

        //}
        #endregion

    }


}
