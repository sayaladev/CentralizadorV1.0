using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{
    public class Agent : CustomHead
    {
        [JsonProperty("results")]
        public List<ResultAgent> Results { get; set; }

        public static async Task<ResultAgent> GetAgetByEmailAsync(string userCEN)
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, $"api/v1/resources/agents/?email={userCEN}");
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

        public static async Task<string> GetTokenCenAsync(string userCEN, string passwordCEN)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "username", userCEN  },
                    { "password", passwordCEN }
                };
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, "api/token-auth/");
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
    }

    public class ResultAgent
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonIgnore]
        public DateTime CreatedTs { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("participants")]
        public List<ResultParticipant> Participants { get; set; }

        [JsonProperty("phones")]
        public List<string> Phones { get; set; }

        [JsonProperty("profile")]
        public int Profile { get; set; }

        [JsonIgnore]
        public DateTime UpdatedTs { get; set; }
    }
}