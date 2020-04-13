
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{
    public class ResultDte
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("instruction")]
        public int Instruction { get; set; }

        [JsonProperty("type")]
        public byte Type { get; set; }

        [JsonProperty("folio")]
        public uint Folio { get; set; }

        [JsonProperty("gross_amount")]
        public uint GrossAmount { get; set; }

        [JsonProperty("net_amount")]
        public uint NetAmount { get; set; }

        [JsonProperty("reported_by_creditor")]
        public bool ReportedByCreditor { get; set; }

        [JsonProperty("emission_dt")]
        public DateTime EmissionDt { get; set; }

        [JsonProperty("emission_file")]
        public string EmissionFile { get; set; }

        [JsonProperty("emission_erp_a")]
        public object EmissionErpA { get; set; }

        [JsonProperty("emission_erp_b")]
        public object EmissionErpB { get; set; }

        [JsonProperty("reception_dt")]
        public DateTime ReceptionDt { get; set; }

        [JsonProperty("reception_erp")]
        public object ReceptionErp { get; set; }

        [JsonProperty("acceptance_dt")]
        public object AcceptanceDt { get; set; }

        [JsonProperty("acceptance_erp")]
        public object AcceptanceErp { get; set; }

        [JsonProperty("acceptance_status")]
        public object AcceptanceStatus { get; set; }

        [JsonProperty("created_ts")]
        public DateTime CreatedTs { get; set; }

        [JsonProperty("updated_ts")]
        public DateTime UpdatedTs { get; set; }

        //Mapping (new properties) 
        [JsonIgnore]
        public uint NroInt { get; set; }

        [JsonIgnore]
        public bool IsPrincipal { get; set; }


    }

    public class Dte
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public IList<ResultDte> Results { get; set; }

        public static ResultDte GetDte(ResultInstruction instruction)
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                string res = wc.DownloadString($"dtes/?debtor={instruction.Debtor}&reported_by_creditor={true}&instruction={instruction.Id}&creditor={instruction.Creditor}");
                if (res != null)
                {
                    Dte dte = JsonConvert.DeserializeObject<Dte>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (dte.Results.Count == 1)
                    {
                        return dte.Results[0];
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
