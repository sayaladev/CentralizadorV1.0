using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.AppFunctions;
using Centralizador.Models.DataBase;

using Newtonsoft.Json;

using static Centralizador.Models.ApiSII.ServiceDetalle;

namespace Centralizador.Models.ApiSII
{
    public class ServiceDetalle
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("metaData")]
        public MetaData MetaData { get; set; }

        public ServiceDetalle(MetaData metaData, Data data)
        {
            MetaData = metaData;
            Data = data;
        }
        public static async Task<List<Detalle>> GetLibroAsync(string tipoUser, ResultParticipant userParticipant, string tipoDoc, string periodo, string token)
        {
            string ns = "", url = "", op = "";
            switch (tipoUser)
            {
                case "Debtor":
                    ns = "cl.sii.sdi.lob.diii.consemitidos.data.api.interfaces.FacadeService/getDetalleRecibidos";
                    op = "2";
                    url = "https://www4.sii.cl/consemitidosinternetui/services/data/facadeService/getDetalleRecibidos";
                    break;
                case "Creditor":
                    ns = "cl.sii.sdi.lob.diii.consemitidos.data.api.interfaces.FacadeService/getDetalle";
                    op = "1";
                    url = "https://www4.sii.cl/consemitidosinternetui/services/data/facadeService/getDetalle";
                    break;
            }
            MetaData metaData = new MetaData
            {
                Namespace = ns,
                ConversationId = token,
                TransactionId = "0"
            };
            Data data = new Data
            {
                TipoDoc = tipoDoc,
                Rut = userParticipant.Rut.ToString(),
                Dv = userParticipant.VerificationCode,
                Periodo = periodo,
                Operacion = op,
                DerrCodigo = tipoDoc,
                RefNCD = "0"
            };
            ServiceDetalle apiDetalleLibroReq = new ServiceDetalle(metaData, data);
            try
            {
                string jSon = JsonConvert.SerializeObject(apiDetalleLibroReq, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    wc.Headers[HttpRequestHeader.Cookie] = $"RUT_NS={userParticipant.Rut}; DV_NS={userParticipant.VerificationCode};TOKEN={token}";
                    string result = await wc.UploadStringTaskAsync(url, WebRequestMethods.Http.Post, jSon);
                    if (result != null)
                    {
                        DetalleLibro detalleLibro = JsonConvert.DeserializeObject<DetalleLibro>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        switch (detalleLibro.RespEstado.CodRespuesta)
                        {
                            case 2:
                                MessageBox.Show($"There are no documents registered for the period {periodo}.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return null;
                            case 0:
                                return detalleLibro.DataResp.Detalles;
                            case 99:
                                MessageBox.Show($"{detalleLibro.RespEstado.MsgeRespuesta}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case 1:
                                MessageBox.Show("This option only maintains the detail of the last six months", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                break;
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

        public enum StatusDetalle
        {
            Accepted,
            Rejected,
            Pending,
            Factoring

        }

        public static StatusDetalle GetStatus(Detalle detalle)
        {
            // http://www.sii.cl/factura_electronica/Webservice_Registro_Reclamo_DTE_V1.2.pdf
            if (detalle.DataEvento.ListEvenHistDoc.Count > 0)
            {
                // Ordeno por el evento más reciente
                IList<ListEvenHistDoc> eventos = detalle.DataEvento.ListEvenHistDoc.OrderByDescending(x => x.FechaEvento).ToList();
                ListEvenHistDoc res = eventos.FirstOrDefault();

                switch (res.CodEvento)
                {
                    case "ACD": // Acepta Contenido del Documento
                        return StatusDetalle.Accepted;
                    case "RCD": // Reclamo al Contenido del Documento
                        return StatusDetalle.Rejected;
                    case "PAG": // Pago Contado
                        return StatusDetalle.Accepted;
                    case "ERM": // Acuse de  Recibo de Mercaderías y Servicios Ley 19.983
                        return StatusDetalle.Accepted;
                    case "ENC": // Recepción de NC, distinta de anulación, que referencia al documento.
                        return StatusDetalle.Accepted;
                    case "RFT": // Receclamo por falta total de mercaderías.
                        return StatusDetalle.Rejected;
                    case "RFP": // Receclamo por falta parcial de mercaderías.
                        return StatusDetalle.Rejected;
                    case "NCA": // Recepción de NC de anulación que referencia al documento.
                        return StatusDetalle.Rejected;
                    case "CED": // DTE Cedido.
                        return StatusDetalle.Factoring;
                    default:
                        return StatusDetalle.Pending;
                }
            }
            else
            {
                if (detalle.DataEvento.MayorOchoDias)
                {
                    return StatusDetalle.Accepted;
                }
                else
                {
                    return StatusDetalle.Pending;
                }
            }
        }
    }

    public class Detalle
    {
        [JsonProperty("rutReceptor")]
        public string RutReceptor { get; set; }

        [JsonProperty("dvReceptor")]
        public string DvReceptor { get; set; }

        [JsonProperty("rznSocRecep")]
        public string RznSocRecep { get; set; }

        [JsonProperty("folio")]
        public int Folio { get; set; }

        [JsonProperty("fechaEmision")]
        public string FechaEmision { get; set; }

        [JsonProperty("fechaRecepcion")]
        public string FechaRecepcion { get; set; }

        [JsonProperty("mntNeto")]
        public int MntNeto { get; set; }

        [JsonProperty("mntExento")]
        public int MntExento { get; set; }

        [JsonProperty("mntIva")]
        public int MntIva { get; set; }

        [JsonProperty("mntTotal")]
        public int MntTotal { get; set; }

        [JsonProperty("tasaImptoIVA")]
        public float TasaImptoIVA { get; set; }

        [JsonProperty("dehOrdenEvento")]
        public byte DehOrdenEvento { get; set; }

        [JsonProperty("dehDescripcion")]
        public string DehDescripcion { get; set; }

        [JsonProperty("totOtrosImp")]
        public object TotOtrosImp { get; set; }

        [JsonProperty("dhdrCodigo")]
        public object DhdrCodigo { get; set; }

        // New properties
        public int Nro { get; set; }
        public ResultInstruction Instruction { get; set; }
        public DteInfoRef DteInfoRefLast { get; set; }
        public IList<DteInfoRef> DteInfoRefs { get; set; }
        public DTEDefType DTEDef { get; set; }
        public DataEvento DataEvento { get; set; }
        public bool IsParticipant { get; set; }
        //public LetterFlag Flag { get; set; } // Exigencias CEN si están correctas.
        public StatusDetalle StatusDetalle { get; set; }
        public ValidatorFlag ValidatorFlag { get; set; } // CEN requeriment validator.
        public int NroInt { get; set; }
        public bool RefMissing { get; set; }

        public string DTEFile { get; set; }
        // public int FolioNVInsertada { get; set; }


        // Constructor
        public Detalle(string rutReceptor, string dvReceptor, string rznSocRecep, int mntNeto, ResultInstruction instruction, bool isParticipant)
        {
            RutReceptor = rutReceptor;
            DvReceptor = dvReceptor;
            RznSocRecep = rznSocRecep;
            MntNeto = mntNeto;
            Instruction = instruction;
            IsParticipant = isParticipant;
            StatusDetalle = StatusDetalle.Pending;
        }
        public Detalle()
        {

        }
    }


    public class DataResp
    {

        [JsonProperty("detalles")]
        public List<Detalle> Detalles { get; set; }

        [JsonProperty("totMntExe")]
        public int TotMntExe { get; set; }

        [JsonProperty("totMntNeto")]
        public int TotMntNeto { get; set; }

        [JsonProperty("totMntIVA")]
        public int TotMntIVA { get; set; }

        [JsonProperty("totMntTotal")]
        public int TotMntTotal { get; set; }
    }

    public class RespEstado
    {

        [JsonProperty("codRespuesta")]
        public int CodRespuesta { get; set; }

        [JsonProperty("msgeRespuesta")]
        public object MsgeRespuesta { get; set; }

        [JsonProperty("codError")]
        public object CodError { get; set; }
    }

    public class DetalleLibro
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("dataResp")]
        public DataResp DataResp { get; set; }

        [JsonProperty("dataReferencias")]
        public object DataReferencias { get; set; }

        [JsonProperty("dataReferenciados")]
        public object DataReferenciados { get; set; }

        [JsonProperty("reparos")]
        public object Reparos { get; set; }

        [JsonProperty("metaData")]
        public MetaData MetaData { get; set; }

        [JsonProperty("detalleDte")]
        public object DetalleDte { get; set; }

        [JsonProperty("impuestoAdicional")]
        public object ImpuestoAdicional { get; set; }

        [JsonProperty("respEstado")]
        public RespEstado RespEstado { get; set; }

    }

    public class Error
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("descripcion")]
        public string Descripcion { get; set; }
    }

}
