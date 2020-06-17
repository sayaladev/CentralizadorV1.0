using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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

        [JsonIgnore]
        public DateTime CreatedTs { get; set; }

        [JsonIgnore]
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
        /// Get 1 'Agente' from CEN API.
        /// </summary>
        /// <param name="userCEN"></param>
        /// <returns></returns>
        public static async Task<ResultAgent> GetAgetByEmailAsync()
        {        
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, $"api/v1/resources/agents/?email={Properties.Settings.Default.UserCEN}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri); // GET
                    if (res != null)
                    {
                        Agent agent = JsonConvert.DeserializeObject<Agent>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (agent.Results.Count == 1)
                        {
                            return agent.Results[0];
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

        /// <summary>
        /// Get 1 Token from CEN API.
        /// </summary>
        /// <param name="userCEN"></param>
        /// <param name="passwordCEN"></param>
        /// <returns></returns>
        public static async Task<string> GetTokenCenAsync()
        {      
            Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "username", Properties.Settings.Default.UserCEN  },
                    { "password", Properties.Settings.Default.PasswordCEN }
                };
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, "api/token-auth/");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.UploadStringTaskAsync(uri, WebRequestMethods.Http.Post, JsonConvert.SerializeObject(dic, Formatting.Indented)); // POST
                    if (res != null)
                    {
                        dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                        return dic["token"];
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        /// <summary>
        /// Get UserCEN from Configuration settings
        /// </summary>
        public static string GetUserCEN => Properties.Settings.Default.UserCEN;
    }

}
