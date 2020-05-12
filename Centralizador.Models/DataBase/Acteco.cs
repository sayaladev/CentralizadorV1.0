using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

using Centralizador.Models.ApiCEN;

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
        public IList<Actividade> Actividades { get; set; }

        public static IList<Actividade> GetActecoCode(ResultParticipant participant)
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                string res = wc.DownloadString($"https://siichile.herokuapp.com/consulta?rut={participant.Rut}-{participant.VerificationCode}");
                if (res != null)
                {
                    Acteco herokuapp = JsonConvert.DeserializeObject<Acteco>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (herokuapp.Actividades.Count > 0)
                    {
                        return herokuapp.Actividades;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                wc.Dispose();
            }
            return null;
        }

        public static int InsertActeco(string descripcion, Conexion conexion)
        {
            try
            {     
                StringBuilder query = new StringBuilder();

                query.Append($"IF (NOT EXISTS (SELECT * FROM softland.cwtgiro WHERE GirDes = '{descripcion}')) BEGIN ");
                query.Append("INSERT INTO softland.cwtgiro  (GirCod, GirDes) values ((select MAX(GirCod) +1 from softland.cwtgiro), ");
                query.Append($"'{descripcion}') END");
                conexion.Query = query.ToString();
                return Conexion.ExecuteNonQuery(conexion);
            }
            catch (Exception)
            {
                throw;
            }

        }

    }
}



