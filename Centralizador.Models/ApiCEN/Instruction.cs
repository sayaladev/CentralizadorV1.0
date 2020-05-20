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
        /// Method return list of instructions with the matrix + user participant binding.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="Userparticipant"></param>
        /// <returns></returns>
        public static IList<ResultInstruction> GetInstructionCreditor(ResultPaymentMatrix matrix, ResultParticipant Userparticipant)
        {
            try
            {
                WebClientCEN.WebClient.Headers.Clear();
                WebClientCEN.WebClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = WebClientCEN.WebClient.DownloadString($"api/v1/resources/instructions/?payment_matrix={matrix.Id}&creditor={Userparticipant.Id}&status=Publicado");
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
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// Method return 1 instruction with the matrix + participants binding.
        /// </summary>
        /// <param name="idMatrix"></param>
        /// <param name="idCReditor"></param>
        /// <param name="idDebtor"></param>
        /// <returns></returns>
        public static ResultInstruction GetInstructionDebtor(ResultPaymentMatrix matrix, ResultParticipant participant, ResultParticipant userPart)
        {          
            try
            {
                WebClientCEN.WebClient.Headers.Clear();
                WebClientCEN.WebClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = WebClientCEN.WebClient.DownloadString($"api/v1/resources/instructions/?payment_matrix={matrix.Id}&creditor={participant.Id}&debtor={userPart.Id}&status=Publicado");
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
            catch (Exception)
            {
                return null;
            }          
            return null;
        }

        public static IList<ResultInstruction> GetInstructionByParticipants(ResultParticipant participant, ResultParticipant userPart)
        {       
            try
            {
                WebClientCEN.WebClient.Headers.Clear();
                WebClientCEN.WebClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = WebClientCEN.WebClient.DownloadString($"api/v1/resources/instructions/?creditor={participant.Id}&debtor={userPart.Id}&status=Publicado");
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
            catch (Exception)
            {
                return null;
            }        
            return null;
        }
    }
}




