using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Centralizador.Models.DataBase;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{


    public class ResultInstruction
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("payment_matrix")]
        public int PaymentMatrixId { get; set; }

        [JsonProperty("creditor")]
        public int Creditor { get; set; }

        [JsonProperty("debtor")]
        public int Debtor { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("amount_gross")]
        public int AmountGross { get; set; }

        [JsonProperty("closed")]
        public bool Closed { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_billed")]
        public int StatusBilled { get; set; }

        [JsonProperty("status_paid")]
        public int StatusPaid { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("max_payment_date")]
        public string MaxPaymentDate { get; set; }

        [JsonProperty("informed_paid_amount")]
        public int InformedPaidAmount { get; set; }

        [JsonProperty("is_paid")]
        public bool IsPaid { get; set; }

        [JsonProperty("auxiliary_data")]
        public AuxiliaryData AuxiliaryData { get; set; }

        [JsonProperty("created_ts")]
        public DateTime CreatedTs { get; set; }

        [JsonProperty("updated_ts")]
        public DateTime UpdatedTs { get; set; }

        //Mapping (new properties)    

        public ResultParticipant Participant { get; set; }

        public ResultPaymentMatrix PaymentMatrix { get; set; }

        public ResultDte Dte { get; set; }

        public Softland Softland { get; set; }



    }

    public class Instruction
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public IList<ResultInstruction> Results { get; set; }

        public static IList<ResultInstruction> GetInstructions(ResultPaymentMatrix matrix, ResultParticipant participant)
        {

            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                string res = wc.DownloadString($"instructions/?payment_matrix={matrix.Id}&creditor={participant.Id}");
                if (res != null)
                {
                    Instruction instruction = JsonConvert.DeserializeObject<Instruction>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (instruction.Results.Count > 0)
                    {
                        return instruction.Results;
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




