using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using static Centralizador.Models.ApiCEN.Pay;

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
        public Instruction.StatusBilled StatusBilled { get; set; }

        [JsonProperty("status_paid")]
        public StatusPay StatusPaid { get; set; }

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

        [JsonIgnore]
        public DateTime CreatedTs { get; set; }

        [JsonIgnore]
        public DateTime UpdatedTs { get; set; }

        // NEW PROPERTIES.
        public ResultParticipant ParticipantDebtor { get; set; }

        public ResultParticipant ParticipantCreditor { get; set; }
        public ResultPaymentMatrix PaymentMatrix { get; set; }
        public ResultDte Dte { get; set; }
        public ResultParticipant ParticipantNew { get; set; }
    }

    public class Instruction : CustomHead
    {
        [JsonProperty("results")]
        public List<ResultInstruction> Results { get; set; }

        public static async Task<List<ResultInstruction>> GetInstructionCreditorAsync(ResultPaymentMatrix matrix, ResultParticipant Userparticipant)
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, $"api/v1/resources/instructions/?payment_matrix={matrix.Id}&creditor={Userparticipant.Id}&status=Publicado");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri);
                    if (res != null)
                    {
                        Instruction instruction = JsonConvert.DeserializeObject<Instruction>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (instruction.Results.Count > 0)
                        {
                            foreach (ResultInstruction item in instruction.Results)
                            {
                                item.ParticipantCreditor = Userparticipant;
                                item.PaymentMatrix = matrix;
                            }
                            return instruction.Results;
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

        public static async Task<ResultInstruction> GetInstructionDebtorAsync(ResultPaymentMatrix matrix, ResultParticipant participant, ResultParticipant userPart)
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, $"api/v1/resources/instructions/?payment_matrix={matrix.Id}&creditor={participant.Id}&debtor={userPart.Id}&status=Publicado");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri);
                    if (res != null)
                    {
                        Instruction instruction = JsonConvert.DeserializeObject<Instruction>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (instruction.Count > 0)
                        {
                            instruction.Results[0].PaymentMatrix = matrix;
                            instruction.Results[0].ParticipantCreditor = participant;
                            instruction.Results[0].ParticipantDebtor = userPart;
                            return instruction.Results[0];
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

        public enum StatusBilled
        {
            NoFacturado = 1, // + DE 1 DTE TOO.
            Facturado = 2,
            ConRetraso = 3
        }
    }
}