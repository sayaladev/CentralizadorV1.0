using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{
    public class CustomHead
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }
    }
}