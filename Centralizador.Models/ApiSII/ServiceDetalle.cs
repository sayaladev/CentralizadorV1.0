using Centralizador.Models.ApiCEN;
using Centralizador.Models.DataBase;
using Centralizador.Models.Helpers;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Centralizador.Models.Helpers.HFlagValidator;

namespace Centralizador.Models.ApiSII
{
    public class ServiceDetalle
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("metaData")]
        public MetaData MetaData { get; set; }

        [JsonIgnore]
        public static HPgModel PgModel { get; set; } = new HPgModel();

        [JsonIgnore]
        public static IProgress<HPgModel> Progress { get; set; }

        public ServiceDetalle(MetaData metaData, Data data)
        {
            MetaData = metaData;
            Data = data;
        }

        public static Task ReportProgress(float p, string msg)
        {
            return Task.Run(() =>
            {
                PgModel.PercentageComplete = (int)p;
                PgModel.Msg = msg;
                Progress.Report(PgModel);
            });
        }

        public static async Task<List<Detalle>> GetLibroAsync(string tipoUser, ResultParticipant userParticipant, string tipoDoc, string periodo, string token, IProgress<HPgModel> progress)
        {
            Progress = progress;
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
                                await ReportProgress(0, $"There are no documents registered for the period {periodo}.");
                                //MessageBox.Show($"There are no documents registered for the period {periodo}.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return null;

                            case 0:
                                return detalleLibro.DataResp.Detalles;

                            case 99:
                                MessageBox.Show($"{detalleLibro.RespEstado.MsgeRespuesta}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;

                            case 1:
                                // MessageBox.Show("This option only maintains the detail of the last six months", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                await ReportProgress(0, "This option only maintains the detail of the last six months.");
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

        // PROPERTIES.
        public int Nro { get; set; }

        public string Tipo { get; set; }
        public ResultInstruction Instruction { get; set; }
        public DteInfoRef DteInfoRefLast { get; set; }
        public List<DteInfoRef> DteInfoRefs { get; set; }
        public DTEDefType DTEDef { get; set; }
        public DataEvento DataEvento { get; set; }
        public bool IsParticipant { get; set; }
        public StatusDetalle StatusDetalle { get; set; }
        public HFlagValidator ValidatorFlag { get; set; }
        public int NroInt { get; set; }
        public ResultParticipant ParticipantMising { get; set; }
        public string DTEFile { get; set; }

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