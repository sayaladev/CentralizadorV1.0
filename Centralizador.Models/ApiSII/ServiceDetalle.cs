using Centralizador.Models.ApiCEN;
using Centralizador.Models.DataBase;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
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
        public static IList<Detalle> GetLibro(string tipoUser, ResultParticipant userParticipant, string tipoDoc, string periodo, string token)
        {
            string ns = "", url = "";
            string op = "";
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
            WebClient wc = new WebClient();
            try
            {
                string jSon = JsonConvert.SerializeObject(apiDetalleLibroReq, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                wc.Headers[HttpRequestHeader.Cookie] = $"RUT_NS={userParticipant.Rut}; DV_NS={userParticipant.VerificationCode};TOKEN={token}";
                string result = wc.UploadString(url, "POST", jSon);
                if (result != null)
                {
                    DetalleLibro detalleLibro = JsonConvert.DeserializeObject<DetalleLibro>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    switch (detalleLibro.RespEstado.CodRespuesta)
                    {
                        case 2:
                            MessageBox.Show($"There are no documents registered for the period {periodo}.", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return null;
                        case 0:
                            return detalleLibro.DataResp.Detalles;
                        case 99:
                            MessageBox.Show($"{detalleLibro.RespEstado.MsgeRespuesta}", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case 1:
                            MessageBox.Show("This option only maintains the detail of the last six months", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            break;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message, "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                wc.Dispose();
            }
            return null;
        }
        public static int GetFlagImageIndex(LetterFlag flag)
        {
            switch (flag)
            {
                case LetterFlag.Red:
                    return 11;
                case LetterFlag.Blue:
                    return 12;
                case LetterFlag.Yellow:
                    return 13;
                case LetterFlag.Green:
                    return 14;
                case LetterFlag.Complete:
                    return 15;
                default:
                    return 16;
            }
        }
        public static Color GetFlagBackColor(LetterFlag flag)
        {
            switch (flag)
            {
                case LetterFlag.Red:
                    return Color.FromArgb(207, 93, 96);
                case LetterFlag.Blue:
                    return Color.FromArgb(92, 131, 180);
                case LetterFlag.Yellow:
                    return Color.FromArgb(255, 193, 96);
                case LetterFlag.Green:
                    return Color.FromArgb(139, 180, 103);
                case LetterFlag.Complete:
                    return Color.White;
                default:
                    return Color.Empty;
            }
        }
        public static LetterFlag ValidateCen(Detalle detalle)
        {
            if (detalle.IsParticipant)
            {
                if (detalle.DTEDef != null && detalle.Instruction != null)
                {
                    DTEDefTypeDocumento dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                    if (dte.Referencia != null)
                    {
                        DTEDefTypeDocumentoReferencia referencia = dte.Referencia.FirstOrDefault(x => x.TpoDocRef == "SEN");
                        if (Convert.ToUInt32(dte.Encabezado.Totales.MntNeto) != detalle.Instruction.Amount)
                        {
                            return LetterFlag.Red;
                        }
                        else if (referencia == null)
                        {
                            return LetterFlag.Red;
                        }
                        else if (referencia.FolioRef != detalle.Instruction.PaymentMatrix.ReferenceCode)
                        {
                            return LetterFlag.Red;
                        }
                        else if (referencia.RazonRef != detalle.Instruction.PaymentMatrix.NaturalKey)
                        {
                            return LetterFlag.Red;
                        }
                        else if (dte.Encabezado.IdDoc.FmaPago != DTEDefTypeDocumentoEncabezadoIdDocFmaPago.Crédito)
                        {
                            return LetterFlag.Red;
                        }
                        else if (dte.Detalle != null && dte.Detalle.Length == 1)
                        {
                            if (dte.Detalle[0].DscItem != detalle.Instruction.PaymentMatrix.NaturalKey)
                            {
                                return LetterFlag.Red;
                            }
                        }
                        return LetterFlag.Green;
                    }
                }
                return LetterFlag.Red;
            }
            return LetterFlag.Clear;
        }
        public enum LetterFlag
        {
            Red,
            Blue,
            Yellow,
            Green,
            Complete,
            Clear
        }
        public enum StatusDetalle
        {
            Accepted,
            Rejected,
            No

        }

        public static StatusDetalle GetStatus(Detalle detalle)
        {
            // http://www.sii.cl/factura_electronica/Webservice_Registro_Reclamo_DTE_V1.2.pdf
            if (detalle.DataEvento!= null && detalle.DataEvento.ListEvenHistDoc.Count > 0)
            {
                if (detalle.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "ACD") != null) // Acepta Contenido del Documento
                {
                    return StatusDetalle.Accepted;
                }
                else if (detalle.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "RCD") != null) // Reclamo al Contenido del Documento
                {
                    return StatusDetalle.Rejected;
                }
                else if (detalle.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "PAG") != null)
                {
                    return StatusDetalle.Accepted;
                }
                else if (detalle.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "ERM") != null) // Acuse de  Recibo de Mercaderías y Servicios Ley 19.983
                {
                    return StatusDetalle.Accepted;
                }
                else if (detalle.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "ENC") != null) // Recepción de NC, distinta de anulación, que referencia al documento.
                {
                    return StatusDetalle.Accepted;
                }
                else if (detalle.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "RFT") != null) // Receclamo por falta total de mercaderías
                {
                    return StatusDetalle.Rejected;
                }
                else if (detalle.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "NCA") != null) // Recepción de NC de anulación que referencia al documento.
                {
                    return StatusDetalle.Rejected;
                }
            }
            else
            {
                if (detalle.DataEvento!= null && detalle.DataEvento.MayorOchoDias)
                {
                    return StatusDetalle.Accepted;
                }
            }
            return StatusDetalle.No;
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
        public Reference References { get; set; }
        public DTEDefType DTEDef { get; set; }
        public DataEvento DataEvento { get; set; }
        public bool IsParticipant { get; set; }
        public LetterFlag Flag { get; set; } // Exigencias CEN si están correctas.
        public StatusDetalle StatusDetalle { get; set; }

        // Constructor
        public Detalle(string rutReceptor, string dvReceptor, string rznSocRecep, int mntNeto, ResultInstruction instruction, bool isParticipant)
        {
            RutReceptor = rutReceptor;
            DvReceptor = dvReceptor;
            RznSocRecep = rznSocRecep;
            MntNeto = mntNeto;
            Instruction = instruction;
            IsParticipant = isParticipant;
        }
        public Detalle()
        {

        }
    }


    public class DataResp
    {

        [JsonProperty("detalles")]
        public IList<Detalle> Detalles { get; set; }

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
