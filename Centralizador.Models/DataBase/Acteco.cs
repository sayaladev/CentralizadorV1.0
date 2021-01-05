using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Centralizador.Models.DataBase
{
    public class Actividade
    {
        [JsonProperty("giro")]
        public string Giro { get; set; }

        [JsonProperty("codigo")]
        public int Codigo { get; set; }

        [JsonProperty("categoria")]
        public string Categoria { get; set; }

        [JsonProperty("afecta")]
        public bool Afecta { get; set; }
    }

    public class Acteco
    {
        [JsonProperty("rut")]
        public string Rut { get; set; }

        [JsonProperty("razon_social")]
        public string RazonSocial { get; set; }

        [JsonProperty("actividades")]
        public List<Actividade> Actividades { get; set; }

        public static async Task<int> InsertActecoAsync(string descripcion, Conexion conexion)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($"IF (NOT EXISTS (SELECT * FROM softland.cwtgiro WHERE GirDes = '{descripcion}')) BEGIN ");
                query.Append("INSERT INTO softland.cwtgiro  (GirCod, GirDes) values ((select MAX(CAST(GirCod AS int)) +1 from softland.cwtgiro), ");
                query.Append($"'{descripcion}') END");
                conexion.Query = query.ToString();
                return await Conexion.ExecuteNonQueryAsync(conexion);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}