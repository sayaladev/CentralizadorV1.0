
using Centralizador.Models.ApiSII;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;

using static Centralizador.Models.ApiSII.ServiceDetalle;

namespace Centralizador.Models.ApiCEN
{
    public class ResultDte
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("instruction")]
        public int Instruction { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("type_sii_code")]
        public int TypeSiiCode { get; set; } // Especial for POST 

        [JsonProperty("folio")]
        public int Folio { get; set; }

        [JsonProperty("gross_amount")]
        public int GrossAmount { get; set; }

        [JsonProperty("net_amount")]
        public int NetAmount { get; set; }

        [JsonProperty("reported_by_creditor")]
        public bool ReportedByCreditor { get; set; }

        [JsonProperty("emission_dt")]
        public string EmissionDt { get; set; }

        [JsonProperty("emission_file")]
        public string EmissionFile { get; set; }

        [JsonProperty("emission_erp_a")]
        public object EmissionErpA { get; set; }

        [JsonProperty("emission_erp_b")]
        public object EmissionErpB { get; set; }

        [JsonProperty("reception_dt")]
        public string ReceptionDt { get; set; }

        [JsonProperty("reception_erp")]
        public object ReceptionErp { get; set; }

        [JsonProperty("acceptance_dt")]
        public string AcceptanceDt { get; set; }

        [JsonProperty("acceptance_erp")]
        public object AcceptanceErp { get; set; }

        [JsonProperty("acceptance_status")]
        public byte AcceptanceStatus { get; set; }

        [JsonProperty("created_ts")]
        public DateTime CreatedTs { get; set; }

        [JsonProperty("updated_ts")]
        public DateTime UpdatedTs { get; set; }


    }

    public class Dte
    {

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public IList<ResultDte> Results { get; set; }

        public static ResultDte GetDteByFolio(Detalle detalle, bool isCreditor) // GET
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress,
                Encoding = Encoding.UTF8
            };
            try
            {
                ResultInstruction i = detalle.Instruction;
                // &type=1 (F 33)       
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                string res = wc.DownloadString($"api/v1/resources/dtes/?reported_by_creditor={isCreditor}&instruction={i.Id}&creditor={i.Creditor}&folio={detalle.Folio}");
                if (res != null)
                {
                    Dte dte = JsonConvert.DeserializeObject<Dte>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (dte.Results.Count == 1)
                    {
                        return dte.Results[0];
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public static ResultDte SendDteCreditor(Detalle detalle, string tokenCen) // POST
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress,
                Encoding = Encoding.UTF8
            };
            string fileName = detalle.Folio + "_" + detalle.Instruction;
            string docXml = detalle.References.FileBasico;
            string idFile = SendFile(tokenCen, fileName, docXml);
            if (!string.IsNullOrEmpty(idFile))
            {
                ResultDte dte = new ResultDte
                {
                    Folio = detalle.Folio,
                    GrossAmount = detalle.MntTotal,
                    Instruction = detalle.Instruction.Id,
                    NetAmount = detalle.MntNeto,
                    ReportedByCreditor = true,
                    TypeSiiCode = 33,
                    EmissionDt = string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(detalle.FechaEmision)),
                    ReceptionDt = string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(detalle.FechaRecepcion)),
                    EmissionFile = idFile
            };
          
                try
                {
                    string d = JsonConvert.SerializeObject(dte);             
                    //wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                    NameValueCollection postData = new NameValueCollection() { { "data", d } };

                    byte[] res = wc.UploadValues("api/v1/operations/dtes/create/", postData);
                    if (res != null)
                    {
                        string json = Encoding.UTF8.GetString(res);
                        InsertDTe r = JsonConvert.DeserializeObject<InsertDTe>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                        if (r != null)
                        {
                            return r.ResultDte;
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }
        public static ResultDte SendDteDebtor(Detalle detalle, string tokenCen) // POST
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress,
                Encoding = Encoding.UTF8
            };
            ResultDte dte = new ResultDte
            {
                Folio = detalle.Folio,
                GrossAmount = detalle.MntTotal,
                Instruction = detalle.Instruction.Id,
                NetAmount = detalle.MntNeto,
                ReportedByCreditor = false,
                TypeSiiCode = 33,
                AcceptanceDt = string.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(detalle.FechaRecepcion))
            };
            switch (detalle.StatusDetalle)
            {
                case StatusDetalle.Accepted:
                    dte.AcceptanceStatus = 1;
                    break;
                case StatusDetalle.Rejected:
                    dte.AcceptanceStatus = 2;
                    break;
                case StatusDetalle.No:
                    break;
                default:
                    break;
            }
            //switch (detalle.StatusDetalle) // Accept 1 / Reclaimed 2                       
            //{
            //    case LetterFlag.Green:
            //        dte.AcceptanceStatus = 1;
            //        break;
            //    case LetterFlag.Red:
            //        dte.AcceptanceStatus = 2;
            //        break;
            //}
            try
            {
                string d = JsonConvert.SerializeObject(dte);
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                NameValueCollection postData = new NameValueCollection() { { "data", d } };

                byte[] res = wc.UploadValues("api/v1/operations/dtes/create/", postData);
                if (res != null)
                {
                    string json = Encoding.UTF8.GetString(res);
                    InsertDTe r = JsonConvert.DeserializeObject<InsertDTe>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                    if (r != null)
                    {
                        return r.ResultDte;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        private static string SendFile(string tokenCen, string fileName, string doc) // PUT
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress,
                Encoding = Encoding.UTF8
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                wc.Headers.Add("Content-Disposition", "attachment; filename=" + fileName + ".xml");
                string res = wc.UploadString("api/v1/resources/auxiliary-files/", WebRequestMethods.Http.Put, doc);
                if (res != null)
                {
                    Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                    return dic["invoice_file_id"];
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

      
    }
    public class InsertDTe
    {

        [JsonProperty("result")]
        public ResultDte ResultDte { get; set; }

        [JsonProperty("errors")]
        public IList<object> Errors { get; set; }

        [JsonProperty("operation")]
        public int Operation { get; set; }
    }

}
