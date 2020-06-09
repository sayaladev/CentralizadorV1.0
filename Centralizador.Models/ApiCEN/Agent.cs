﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{
    public class ResultAgent
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("phones")]
        public IList<string> Phones { get; set; }

        [JsonProperty("profile")]
        public int Profile { get; set; }

        [JsonProperty("participants")]
        public IList<ResultParticipant> Participants { get; set; }

        [JsonProperty("created_ts")]
        public DateTime CreatedTs { get; set; }

        [JsonProperty("updated_ts")]
        public DateTime UpdatedTs { get; set; }
    }

    public class Agent
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public IList<ResultAgent> Results { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ResultAgent GetAgetByEmail(string userCEN) // GET
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress,
                Encoding = Encoding.UTF8               
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = wc.DownloadString($"api/v1/resources/agents/?email={userCEN}");
                if (res != null)
                {
                    Agent agent = JsonConvert.DeserializeObject<Agent>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (agent.Results.Count == 1)
                    {
                        return agent.Results[0];
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
        /// <returns></returns>
        public static string GetTokenCen(string userCEN, string passwordCEN) // POST
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress,
                Encoding = Encoding.UTF8
            };
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "username", userCEN },
                    { "password", passwordCEN }
                };
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = wc.UploadString("api/token-auth/", WebRequestMethods.Http.Post, JsonConvert.SerializeObject(dic, Formatting.Indented));
                if (res != null)
                {
                    dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                    return dic["token"];
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
