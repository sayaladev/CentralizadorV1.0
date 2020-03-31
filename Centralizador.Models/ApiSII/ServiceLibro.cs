﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;

using Newtonsoft.Json;

namespace Centralizador.Models.ApiSII
{
    public class ServiceLibro
    {

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("metaData")]
        public MetaData MetaData { get; set; }

        public ServiceLibro(MetaData metaData, Data data)
        {
            MetaData = metaData;
            Data = data;
        }

        
        public static IList<Detalle> GetLibro(string tipoLibro, ResultParticipant participante, string tipoDoc, string periodo)
        {
            string ns = "", url = "";
            string token = TokenSeed.GETTokenFromSii();
            byte op = 0;
       
            switch (tipoLibro)
            {
                case "Debitor":
                    ns = "cl.sii.sdi.lob.diii.consemitidos.data.api.interfaces.FacadeService/getDetalleRecibidos";
                    op = 2;
                    url = "https://www4.sii.cl/consemitidosinternetui/services/data/facadeService/getDetalleRecibidos";
                    break;
                case "Creditor":
                    ns = "cl.sii.sdi.lob.diii.consemitidos.data.api.interfaces.FacadeService/getDetalle";
                    op = 1;
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
                Rut = participante.Rut.ToString(),
                Dv = participante.VerificationCode,
                Periodo = periodo,
                Operacion = op,
                DerrCodigo = tipoDoc,
                RefNCD = "0"
            };

            ServiceLibro apiDetalleLibroReq = new ServiceLibro(metaData, data);

            try
            {
                string jSon = JsonConvert.SerializeObject(apiDetalleLibroReq, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                WebClient wc = new WebClient();
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                wc.Headers[HttpRequestHeader.Cookie] = $"RUT_NS={participante.Rut}; DV_NS={participante.VerificationCode};TOKEN={token}";
                string result = wc.UploadString(url, "POST", jSon);
                if (result != null)
                {
                    DetalleLibro detalleLibro = JsonConvert.DeserializeObject<DetalleLibro>(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (detalleLibro.MetaData.Errors != null)
                    {
                        MessageBox.Show($"Message from Sii:{Environment.NewLine}{detalleLibro.MetaData.Errors[0].Descripcion}");
                        return null;
                    }

                    if (detalleLibro.RespEstado.CodRespuesta != 0)
                    {
                        MessageBox.Show($"Message from Sii:{Environment.NewLine}{detalleLibro.RespEstado.MsgeRespuesta}");
                        return null;
                    }
                    return detalleLibro.DataResp.Detalles;
                }
            }
            catch (WebException e)
            {
                //throw;//token fuera de tiempo: error 401 no autorizado.
                HttpWebResponse http = (HttpWebResponse)e.Response;
                HttpStatusCode code = http.StatusCode;
                if (code.ToString() == "Unauthorized")
                {
                                     

                }
                else
                {
                    MessageBox.Show($"Message from Http:{Environment.NewLine}{http.StatusDescription}");
                    return null;
                }
            }
            return null;
        }

    }

    public class Detalle
    {

        [JsonProperty("rutReceptor")]
        public uint RutReceptor { get; set; }

        [JsonProperty("dvReceptor")]
        public string DvReceptor { get; set; }

        [JsonProperty("rznSocRecep")]
        public string RznSocRecep { get; set; }

        [JsonProperty("folio")]
        public uint Folio { get; set; }

        [JsonProperty("fechaEmision")]
        public string FechaEmision { get; set; }

        [JsonProperty("fechaRecepcion")]
        public string FechaRecepcion { get; set; }

        [JsonProperty("mntNeto")]
        public uint MntNeto { get; set; }

        [JsonProperty("mntExento")]
        public uint MntExento { get; set; }

        [JsonProperty("mntIva")]
        public uint MntIva { get; set; }

        [JsonProperty("mntTotal")]
        public uint MntTotal { get; set; }

        [JsonProperty("tasaImptoIVA")]
        public float TasaImptoIVA { get; set; }

        [JsonProperty("dehOrdenEvento")]
        public string DehOrdenEvento { get; set; }

        [JsonProperty("dehDescripcion")]
        public string DehDescripcion { get; set; }

        [JsonProperty("totOtrosImp")]
        public object TotOtrosImp { get; set; }

        [JsonProperty("dhdrCodigo")]
        public object DhdrCodigo { get; set; }

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

        //[JsonIgnore]
        [JsonProperty("errors")]
        public List<Error> Errors { get; set; }

        [JsonProperty("page")]
        public object Page { get; set; }
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
        public int Operacion { get; set; }

        [JsonProperty("derrCodigo")]
        public string DerrCodigo { get; set; }

        [JsonProperty("refNCD")]
        public string RefNCD { get; set; }
    }

    public class DetalleLibro
    {
        [JsonProperty("data")]
        public object Data { get; set; }

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