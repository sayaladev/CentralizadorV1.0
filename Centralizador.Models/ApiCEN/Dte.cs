
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Centralizador.Models.ApiSII;

using Newtonsoft.Json;

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

        [JsonIgnore]
        public DateTime CreatedTs { get; set; }

        [JsonIgnore]
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

     
        /// <summary>
        /// Send 1 Dte to CEN API (Creditor)
        /// </summary>
        /// <param name="detalle"></param>
        /// <param name="tokenCen"></param>
        /// <returns></returns>
        public static async Task<ResultDte> SendDteCreditorAsync(Detalle detalle, string tokenCen)
        {
            string fileName = detalle.Folio + "_" + detalle.Instruction;
            string docXml = detalle.References.FileBasico;
            string idFile = SendFileAsync(tokenCen, fileName, docXml).Result;
            if (idFile == null)
            {
                // Error Exception
                return null;
            }
            ResultDte resultDte = new ResultDte();
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
                    using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                    {
                        Uri uri = new Uri(Properties.Settings.Default.BaseAddress, "api/v1/operations/dtes/create/");
                        string d = JsonConvert.SerializeObject(dte);
                        wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                        NameValueCollection postData = new NameValueCollection() { { "data", d } };
                        byte[] res = await wc.UploadValuesTaskAsync(uri, WebRequestMethods.Http.Post, postData); // POST
                        if (res != null)
                        {
                            string json = Encoding.UTF8.GetString(res);
                            InsertDTe r = JsonConvert.DeserializeObject<InsertDTe>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                            if (r != null)
                            {
                                resultDte = r.ResultDte;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Error Exception
                    return null;
                }
            }
            return resultDte;
        }

        /// <summary>
        /// Send 1 Dte to CEN API (Debtor)
        /// </summary>
        /// <param name="detalle"></param>
        /// <param name="tokenCen"></param>
        /// <returns></returns>
        public static async Task<ResultDte> SendDteDebtorAsync(Detalle detalle, string tokenCen)
        {
            ResultDte resultDte = new ResultDte();
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
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, "api/v1/operations/dtes/create/");
                    string d = JsonConvert.SerializeObject(dte);
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                    NameValueCollection postData = new NameValueCollection() { { "data", d } };

                    byte[] res = await wc.UploadValuesTaskAsync("api/v1/operations/dtes/create/", WebRequestMethods.Http.Post, postData); // POST
                    if (res != null)
                    {
                        string json = Encoding.UTF8.GetString(res);
                        InsertDTe r = JsonConvert.DeserializeObject<InsertDTe>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (r != null)
                        {
                            resultDte = r.ResultDte;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Error Exception
                return null;
            }
            return resultDte;
        }

        /// <summary>
        /// Send 1 file for Insert into Dte CEN API
        /// </summary>
        /// <param name="tokenCen"></param>
        /// <param name="fileName"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static async Task<string> SendFileAsync(string tokenCen, string fileName, string doc)
        {
            string fileId = "";
            try
            {
                using (WebClient wc = new WebClient() { Encoding = Encoding.UTF8 })
                {
                    Uri uri = new Uri(Properties.Settings.Default.BaseAddress, "api/v1/resources/auxiliary-files/");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                    wc.Headers.Add("Content-Disposition", "attachment; filename=" + fileName + ".xml");
                    string res = await wc.UploadStringTaskAsync(uri, WebRequestMethods.Http.Put, doc); // PUT
                    if (res != null)
                    {
                        Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                        fileId = dic["invoice_file_id"];
                    }
                }
            }
            catch (Exception)
            {
                // Error Exception
                return null;
            }
            return fileId;
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
