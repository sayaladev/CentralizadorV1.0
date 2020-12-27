using System.Collections.Generic;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiSII
{
    public class MetaData
    {
        [JsonProperty("conversationId")]
        public object ConversationId { get; set; }

        [JsonProperty("transactionId")]
        public object TransactionId { get; set; }

        [JsonProperty("namespace")]
        public object Namespace { get; set; }

        [JsonIgnore]
        [JsonProperty("info")]
        public object Info { get; set; }

        [JsonProperty("errors")]
        public List<Error> Errors { get; set; }

        [JsonProperty("page")]
        public object Page { get; set; }
    }
}