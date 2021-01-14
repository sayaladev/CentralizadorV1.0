using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Centralizador.Models.ApiSII;

using Newtonsoft.Json;

using static Centralizador.Models.ApiSII.ServiceDetalle;
using static Centralizador.Models.Helpers.HFlagValidator;

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
        public int TypeSiiCode { get; set; } // FOR POST.

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

    public class Dte : CustomHead
    {
        [JsonProperty("results")]
        public List<ResultDte> Results { get; set; }

        public static async Task<ResultDte> SendDteCreditorAsync(Detalle detalle, string tokenCen, string doc)
        {
            string fileName = detalle.Folio + "_" + detalle.Instruction.Id;
            string idFile = await SendFileAsync(tokenCen, fileName, doc);
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
                    using (CustomWebClient wc = new CustomWebClient())
                    {
                        Uri uri = new Uri(Properties.Settings.Default.UrlCen, "api/v1/operations/dtes/create/");
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
                                return r.ResultDte;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return null;
        }

        public static async Task<ResultDte> SendDteDebtorAsync(Detalle detalle, string tokenCen)
        {
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

                case StatusDetalle.Pending:
                    break;

                default:
                    break;
            }
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, "api/v1/operations/dtes/create/");
                    string d = JsonConvert.SerializeObject(dte);
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                    NameValueCollection postData = new NameValueCollection() { { "data", d } };

                    byte[] res = await wc.UploadValuesTaskAsync(uri, WebRequestMethods.Http.Post, postData); // POST
                    if (res != null)
                    {
                        string json = Encoding.UTF8.GetString(res);
                        InsertDTe r = JsonConvert.DeserializeObject<InsertDTe>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (r != null && r.Errors.Count == 0)
                        {
                            return r.ResultDte;
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

        private static async Task<string> SendFileAsync(string tokenCen, string fileName, string doc)
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, "api/v1/resources/auxiliary-files/");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Authorization] = $"Token {tokenCen}";
                    wc.Headers.Add("Content-Disposition", "attachment; filename=" + fileName + ".xml");
                    string res = await wc.UploadStringTaskAsync(uri, WebRequestMethods.Http.Put, doc); // PUT
                    if (res != null)
                    {
                        Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(res);
                        return dic["invoice_file_id"];
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

        public static async Task<ResultDte> GetDteAsync(Detalle detalle, bool isCreditor)
        {
            try
            {
                using (CustomWebClient wc = new CustomWebClient())
                {
                    Uri uri = new Uri(Properties.Settings.Default.UrlCen, $"api/v1/resources/dtes/?reported_by_creditor={isCreditor}&folio={detalle.Folio}&instruccion={detalle.Instruction.Id}&creditor={detalle.Instruction.Creditor}");
                    wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                    string res = await wc.DownloadStringTaskAsync(uri);  // GET
                    if (res != null)
                    {
                        Dte p = JsonConvert.DeserializeObject<Dte>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        if (p.Count == 1)
                        {
                            return p.Results[0];
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

    public class InsertDTe
    {
        [JsonProperty("result")]
        public ResultDte ResultDte { get; set; }

        [JsonProperty("errors")]
        public List<object> Errors { get; set; }

        [JsonProperty("operation")]
        public int Operation { get; set; }
    }
}