﻿using System;
using System.Collections.Generic;
using System.Net;

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

        [JsonProperty("created_ts")]
        public DateTime CreatedTs { get; set; }

        [JsonProperty("updated_ts")]
        public DateTime UpdatedTs { get; set; }

        // New properties
        public string Comuna { get; set; }
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
        /// Method return 1 participant.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ResultParticipant GetParticipantById(int id) // GET
        {
            try
            {
                WebClientCEN.WebClient.Headers.Clear();
                WebClientCEN.WebClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = WebClientCEN.WebClient.DownloadString($"api/v1/resources/participants/?id={id}");
                if (res != null)
                {
                    Participant p = JsonConvert.DeserializeObject<Participant>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (p.Results.Count == 1)
                    {
                        return p.Results[0];
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
        /// Method return 1 participant.
        /// </summary>
        /// <param name="rut"></param>
        /// <returns></returns>
        public static ResultParticipant GetParticipantByRut(string rut)
        {
            try
            {
                WebClientCEN.WebClient.Headers.Clear();
                WebClientCEN.WebClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = WebClientCEN.WebClient.DownloadString($"api/v1/resources/participants/?rut={rut}");
                if (res != null)
                {
                    Participant p = JsonConvert.DeserializeObject<Participant>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (p.Results.Count == 1)
                    {
                        return p.Results[0];
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
