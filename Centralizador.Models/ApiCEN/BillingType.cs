﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public static IList<ResultBilingType> GetBilinTypes()
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                string res = wc.DownloadString("billing-types");
                if (res != null)
                {
                    BilingType bilingType = JsonConvert.DeserializeObject<BilingType>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (bilingType.Results.Count > 0)
                    {
                        return bilingType.Results;
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