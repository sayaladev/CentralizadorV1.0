using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Centralizador.Models.ApiCEN;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiSII
{
    public class ServiceEvento
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("metaData")]
        public MetaData MetaData { get; set; }

        public static string RutToken { get; set; } = GetCertFromPc().Subject;

        public ServiceEvento(Data data, MetaData metaData)
        {
            Data = data;
            MetaData = metaData;
        }

        public static async Task<DataEvento> GetStatusDteAsync(string tipoUser, string token, string tipoDoc, Detalle detalle, ResultParticipant userParticipant)
        {
            string rutToken;
            rutToken = RutToken.Substring(RutToken.IndexOf('=') + 1, 10);
            MetaData metaData = new MetaData
            {
                Namespace = "cl.sii.sdi.lob.diii.registrorechazodtej6.data.api.interfaces.FacadeService/validarAccesoReceptor",
                ConversationId = token,
                TransactionId = "0"
            };
            Data data = new Data();
            if (tipoUser == "Debtor")
            {
                data.RutEmisor = detalle.RutReceptor.ToString();
                data.DvEmisor = detalle.DvReceptor;
                data.TipoDoc = tipoDoc;
                data.Folio = detalle.Folio.ToString();
                data.RutToken = rutToken.Split('-').GetValue(0).ToString();
                data.DvToken = rutToken.Split('-').GetValue(1).ToString();
            }
            else if (tipoUser == "Creditor")
            {
                data.RutEmisor = userParticipant.Rut.ToString();
                data.DvEmisor = userParticipant.VerificationCode;
                data.TipoDoc = tipoDoc;
                data.Folio = detalle.Folio.ToString();
                data.RutToken = rutToken.Split('-').GetValue(0).ToString();
                data.DvToken = rutToken.Split('-').GetValue(1).ToString();
            }

            string url = "https://www4.sii.cl/registrorechazodtej6ui/services/data/facadeService/validarAccesoReceptor";
            ServiceEvento serviceEvento = new ServiceEvento(data, metaData);
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    string jSon = JsonConvert.SerializeObject(serviceEvento, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    wc.Encoding = Encoding.UTF8;
                    wc.Headers[HttpRequestHeader.Cookie] = $"TOKEN={token}";
                    string result = await wc.UploadStringTaskAsync(url, WebRequestMethods.Http.Post, jSon);
                    if (result != null)
                    {
                        ResultEvent detalleLibro = JsonConvert.DeserializeObject<ResultEvent>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (detalleLibro.MetaData.Errors == null)
                        {
                            return detalleLibro.DataEvento;
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

        private static X509Certificate2 GetCertFromPc()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            foreach (X509Certificate2 item in store.Certificates)
            {
                if (item.SerialNumber == Properties.Settings.Default.SerialNumber && item.NotAfter > DateTime.Now)
                {
                    return item;
                }
            }
            store.Close();
            return null;
        }
    }

    public class ListEvenHistDoc
    {
        [JsonProperty("codigoDoc")]
        public object CodigoDoc { get; set; }

        [JsonProperty("descEvento")]
        public string DescEvento { get; set; }

        [JsonProperty("codEvento")]
        public string CodEvento { get; set; }

        [JsonProperty("fechaEvento")]
        public string FechaEvento { get; set; }

        [JsonProperty("rutResponsable")]
        public int RutResponsable { get; set; }

        [JsonProperty("dvResponsable")]
        public string DvResponsable { get; set; }

        [JsonProperty("direccionIp")]
        public object DireccionIp { get; set; }

        [JsonProperty("rutEmisor")]
        public object RutEmisor { get; set; }

        [JsonProperty("dvEmisor")]
        public object DvEmisor { get; set; }

        [JsonProperty("rutReceptor")]
        public object RutReceptor { get; set; }

        [JsonProperty("dvReceptor")]
        public object DvReceptor { get; set; }
    }

    public class DataEvento
    {
        [JsonProperty("dhdrCodigo")]
        public long DhdrCodigo { get; set; }

        [JsonProperty("dhdrRutEmisor")]
        public uint DhdrRutEmisor { get; set; }

        [JsonProperty("dhdrDvEmisor")]
        public string DhdrDvEmisor { get; set; }

        [JsonProperty("rzEmisor")]
        public string RzEmisor { get; set; }

        [JsonProperty("dtdcCodigo")]
        public int DtdcCodigo { get; set; }

        [JsonProperty("descDoc")]
        public string DescDoc { get; set; }

        [JsonProperty("dhdrFolio")]
        public int DhdrFolio { get; set; }

        [JsonProperty("dhdrRutRecep")]
        public int DhdrRutRecep { get; set; }

        [JsonProperty("dhdrDvRecep")]
        public string DhdrDvRecep { get; set; }

        [JsonProperty("rzReceptor")]
        public string RzReceptor { get; set; }

        [JsonProperty("dhdrFchEmis")]
        public string DhdrFchEmis { get; set; }

        [JsonProperty("dhdrMntTotal")]
        public int DhdrMntTotal { get; set; }

        [JsonProperty("dhdrIva")]
        public int DhdrIva { get; set; }

        [JsonProperty("dtecTmstRecep")]
        public string DtecTmstRecep { get; set; }

        [JsonProperty("diferenciaFecha")]
        public int DiferenciaFecha { get; set; }

        [JsonProperty("tieneAccesoReceptor")]
        public bool TieneAccesoReceptor { get; set; }

        [JsonProperty("tieneAccesoEmisor")]
        public bool TieneAccesoEmisor { get; set; }

        [JsonProperty("tieneAccesoTenedorVig")]
        public bool TieneAccesoTenedorVig { get; set; }

        [JsonProperty("pagadoAlContado")]
        public bool PagadoAlContado { get; set; }

        [JsonProperty("tieneReclamos")]
        public bool TieneReclamos { get; set; }

        [JsonProperty("mayorOchoDias")]
        public bool MayorOchoDias { get; set; }

        [JsonProperty("tieneAcuses")]
        public bool TieneAcuses { get; set; }

        [JsonProperty("tieneReferenciaGuia")]
        public bool TieneReferenciaGuia { get; set; }

        [JsonProperty("msgDteCedible")]
        public object MsgDteCedible { get; set; }

        [JsonProperty("listEvenHistDoc")]
        public List<ListEvenHistDoc> ListEvenHistDoc { get; set; }
    }

    public class ResultEvent
    {
        [JsonProperty("data")]
        public DataEvento DataEvento { get; set; }

        [JsonProperty("metaData")]
        public MetaData MetaData { get; set; }
    }
}