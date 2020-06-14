using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
        public Instruction.StatusBilled StatusBilled { get; set; }

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

        [JsonIgnore]
        public DateTime CreatedTs { get; set; }

        [JsonIgnore]
        public DateTime UpdatedTs { get; set; }

        //Mapping (new properties)    
        public ResultParticipant ParticipantDebtor { get; set; }
        public ResultParticipant ParticipantCreditor { get; set; }
        public ResultPaymentMatrix PaymentMatrix { get; set; }
        public ResultDte Dte { get; set; }


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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="Userparticipant"></param>
        /// <returns></returns>
        public static async Task<IList<ResultInstruction>> GetInstructionCreditorAsync(ResultPaymentMatrix matrix, ResultParticipant Userparticipant)
        {
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/instructions/?payment_matrix={matrix.Id}&creditor={Userparticipant.Id}&status=Publicado");
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
                        }
                        return instruction.Results;
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
        /// <param name="idMatrix"></param>
        /// <param name="idCReditor"></param>
        /// <param name="idDebtor"></param>
        /// <returns></returns>
        public static async Task<ResultInstruction> GetInstructionDebtorAsync(ResultPaymentMatrix matrix, ResultParticipant participant, ResultParticipant userPart)
        {       
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/instructions/?payment_matrix={matrix.Id}&creditor={participant.Id}&debtor={userPart.Id}&status=Publicado");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri);
                    if (res != null)
                    {
                        Instruction instruction = JsonConvert.DeserializeObject<Instruction>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (instruction.Results.Count == 1)
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
                return null;
            }
            return null;
        }

        /// <summary>
        /// Get 1 'Instrucción de pago' from CEN API
        /// </summary>
        /// <param name="participant"></param>
        /// <param name="userPart"></param>
        /// <returns></returns>
        public static async Task<IList<ResultInstruction>> GetInstructionByParticipantsAsync(ResultParticipant participant, ResultParticipant userPart)
        {         
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/instructions/?creditor={participant.Id}&debtor={userPart.Id}&status=Publicado");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri);
                    if (res != null)
                    {
                        Instruction instruction = JsonConvert.DeserializeObject<Instruction>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (instruction.Results.Count > 0)
                        {
                            instruction.Results[0].ParticipantCreditor = participant;
                            instruction.Results[0].ParticipantDebtor = userPart;
                            return instruction.Results;
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
        public enum StatusBilled
        {
            // 1 No Facturado y cuando hay más de 1 dte informado
            // 2 Facturado
            // 3 Facturado con retraso
            NoFacturado = 1,
            Facturado = 2,
            ConRetraso = 3
        }
    }
}




