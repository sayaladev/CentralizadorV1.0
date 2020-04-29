using System;
using System.Collections.Generic;
using System.Globalization;
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

    public class Herokuapp
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
                    Herokuapp herokuapp = JsonConvert.DeserializeObject<Herokuapp>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
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

        public static int InsertActeco(ResultInstruction instruction, string descripcion)
        {
            try
            {
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                string DataBaseName = "";
                string ServerName = Properties.Settings.Default.ServerName;
                string id = Properties.Settings.Default.DBUser;
                string password = Properties.Settings.Default.DBPassword;

                foreach (XmlNode item in document.ChildNodes[0])
                {
                    if (item.Attributes["id"].Value == instruction.Creditor.ToString())
                    {
                        DataBaseName = item.FirstChild.InnerText;
                        break;
                    }
                }
                if (DataBaseName == null || ServerName == null || id == null || password == null)
                {
                    return -1;
                }
                Conexion con = new Conexion
                {
                    Cnn = $"Data Source={ServerName};Initial Catalog={DataBaseName};Persist Security Info=True;User ID={id};Password={password}"
                };
                StringBuilder query = new StringBuilder();
                query.Append($"IF (NOT EXISTS (SELECT * FROM softland.cwtgiro WHERE GirDes = '{descripcion}')) BEGIN ");
                query.Append("INSERT INTO softland.cwtgiro  (GirCod, GirDes) values ((select MAX(GirCod) +1 from softland.cwtgiro), ");
                query.Append($"'{descripcion}') END;SELECT SCOPE_IDENTITY();"); 
                con.Query = query.ToString();
                return Conexion.ExecuteNonQuery(con);
            }
            catch (Exception)
            {
                throw;
            }

        }

    }
}



