using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Centralizador.Models.ApiSII
{
    public class Data
    {

        [JsonProperty("tipoDoc")]
        public string TipoDoc { get; set; }

        [JsonProperty("rut")]
        public string Rut { get; set; }

        [JsonProperty("dv")]
        public string Dv { get; set; }

        [JsonProperty("periodo")]
        public string Periodo { get; set; }

        [JsonProperty("operacion")]
        public string Operacion { get; set; }

        [JsonProperty("derrCodigo")]
        public string DerrCodigo { get; set; }

        [JsonProperty("refNCD")]
        public string RefNCD { get; set; }

        // ServiceEvento
        [JsonProperty("dvEmisor")]
        public string DvEmisor { get; set; }

        [JsonProperty("rutEmisor")]
        public string RutEmisor { get; set; }

        [JsonProperty("folio")]
        public string Folio { get; set; }

        [JsonProperty("rutToken")]
        public string RutToken { get; set; }

        [JsonProperty("dvToken")]
        public string DvToken { get; set; }
    }
}
