
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;

using Newtonsoft.Json;

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
        public object AcceptanceDt { get; set; }

        [JsonProperty("acceptance_erp")]
        public object AcceptanceErp { get; set; }

        [JsonProperty("acceptance_status")]
        public object AcceptanceStatus { get; set; }

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

        public static IList<ResultDte> GetDteByParticipant(ResultInstruction instruction)
        {
            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Encoding = Encoding.UTF8;
                string res = wc.DownloadString($"api/v1/resources/dtes/?reported_by_creditor=true&instruction={instruction.Id}&creditor={instruction.Creditor}");
                if (res != null)
                {
                    Dte dte = JsonConvert.DeserializeObject<Dte>(res, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    if (dte.Results.Count > 0)
                    {
                        return dte.Results;
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

        public static ResultDte SendDte(ResultDte dte, string tokenCen, string doc)
        {
            string fileName = dte.Folio + "_" + dte.Instruction;
            string idFile = SendFile(tokenCen, fileName, doc);
            if (!string.IsNullOrEmpty(idFile))
            {
                dte.EmissionFile = idFile;

                WebClient wc = new WebClient
                {
                    BaseAddress = Properties.Settings.Default.BaseAddress
                };
                try
                {
                    string d = JsonConvert.SerializeObject(dte);   
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Encoding = Encoding.UTF8;
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
                finally
                {
                    wc.Dispose();
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        private static string SendFile(string tokenCen, string fileName, string doc)
        {

            WebClient wc = new WebClient
            {
                BaseAddress = Properties.Settings.Default.BaseAddress
            };
            try
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.Encoding = Encoding.UTF8;
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
            finally
            {
                wc.Dispose();
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
