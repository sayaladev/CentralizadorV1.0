using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{
    public class PaymentsContact
    {

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("phones")]
        public IList<string> Phones { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class BillsContact
    {

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("phones")]
        public IList<string> Phones { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class ResultParticipant
    {

        [JsonProperty("is_coordinator")]
        public bool IsCoordinator { get; set; } //Agent

        [JsonProperty("participant")]
        public int ParticipantId { get; set; } //Agent

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rut")]
        public string Rut { get; set; }

        [JsonProperty("verification_code")]
        public string VerificationCode { get; set; }

        [JsonProperty("business_name")]
        public string BusinessName { get; set; }

        [JsonProperty("commercial_business")]
        public string CommercialBusiness { get; set; }

        [JsonProperty("dte_reception_email")]
        public string DteReceptionEmail { get; set; }

        [JsonProperty("bank_account")]
        public string BankAccount { get; set; }

        [JsonProperty("bank")]
        public int Bank { get; set; }

        [JsonProperty("commercial_address")]
        public string CommercialAddress { get; set; }

        [JsonProperty("postal_address")]
        public string PostalAddress { get; set; }

        [JsonProperty("manager")]
        public string Manager { get; set; }

        [JsonProperty("payments_contact")]
        public PaymentsContact PaymentsContact { get; set; }

        [JsonProperty("bills_contact")]
        public BillsContact BillsContact { get; set; }

        [JsonIgnore]
        public DateTime CreatedTs { get; set; }

        [JsonIgnore]
        public DateTime UpdatedTs { get; set; }


    }

    public class Participant
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public IList<ResultParticipant> Results { get; set; }

        /// <summary>
        /// Get 1 'Participante' from CEN API
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<ResultParticipant> GetParticipantByIdAsync(int id)
        {
            ResultParticipant participant = new ResultParticipant();
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/participants/?id={id}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri);  // GET
                    if (res != null)
                    {
                        Participant p = JsonConvert.DeserializeObject<Participant>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        participant = p.Results[0];
                    }
                }
            }
            catch (Exception)
            {
                // Error Exception
                return null;
            }
            return participant;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rut"></param>
        /// <returns></returns>
        public static async Task<ResultParticipant> GetParticipantByRutAsync(string rut)
        {
            ResultParticipant participant = new ResultParticipant();
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/participants/?rut={rut}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri); // GET
                    if (res != null)
                    {
                        Participant p = JsonConvert.DeserializeObject<Participant>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        participant = p.Results[0];
                    }
                }
            }
            catch (Exception)
            {
                // Error Exception
                return null;
            }
            return participant;
        }


        public static IList<ResultParticipant> GetParticipants()
        {
            IList<ResultParticipant> participants = new List<ResultParticipant>();           
            ResultAgent agent = Agent.GetAgetByEmailAsync().Result;
            if (agent != null)
            {
                foreach (ResultParticipant item in agent.Participants)
                {
                    ResultParticipant participant = GetParticipantByIdAsync(item.ParticipantId).Result;
                    if (participant != null)
                    {
                        participants.Add(participant);
                    }
                    else
                    {
                        // Error Exception
                        return null;
                    }
                }
                // Add Cve 76.532.358-4  
                participants.Insert(0, new ResultParticipant { Name = "Please select a Company" });
                participants.Insert(1, new ResultParticipant { Name = "CVE Renovable", Rut = "76532358", VerificationCode = "4", Id = 999, IsCoordinator = false });
            }
            else
            {
                // Error Exception
                return null;
            }
            return participants;
        }
    }


}
