using Newtonsoft.Json;

namespace Centralizador.Models.ApiCEN
{
    public class AuxiliaryData
    {
        [JsonProperty("payment_matrix_natural_key")]
        public string PaymentMatrixNaturalKey { get; set; }

        [JsonProperty("payment_matrix_concept")]
        public string PaymentMatrixConcept { get; set; }
    }
}
