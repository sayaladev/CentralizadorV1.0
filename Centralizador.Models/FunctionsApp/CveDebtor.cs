using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.DataBase;
using Centralizador.Models.Helpers;

using OpenHtmlToPdf;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Centralizador.Models.Helpers.HEnum;
using static Centralizador.Models.Helpers.HFlagValidator;

namespace Centralizador.Models.FunctionsApp
{
    public class CveDebtor : ICveFunction
    {
        public CancellationTokenSource Cancellation { get; set; }
        public string DBName { get; set; }
        public HPgModel PgModel { get; set; }
        public IProgress<HPgModel> Progress { get; set; }
        public string Query { get; set; }
        public string TokenCen { get; set; }
        public string TokenSii { get; set; }
        public ResultParticipant UserParticipant { get; set; }
        public List<Detalle> DetalleList { get; set; }
        public Conexion Conn { get; set; }
        public TipoTask Mode { get; set; }
        public List<ResultBilingType> BilingTypes { get; set; }
        public StringBuilder StringLogging { get; set; }
        public bool IsRuning { get; set; }

        public CveDebtor(string dBName, IProgress<HPgModel> progress, string tokenCen, string tokenSii, ResultParticipant userParticipant)
        {
            DBName = dBName;
            Progress = progress;
            TokenCen = tokenCen;
            TokenSii = tokenSii;
            UserParticipant = userParticipant;
            Mode = TipoTask.Debtor;
            PgModel = new HPgModel();
            Progress = progress;
            Conn = new Conexion(dBName);
            IsRuning = true;
        }

        public void SaveLogging(string path, string nameFile)
        {
            new CreateFile(path, StringLogging, nameFile);
        }

        //public void SaveParam()
        //{
        //    Properties.Settings.Default.Save();
        //}

        public Task ReportProgress(float p, string msg)
        {
            return Task.Run(() =>
            {
                PgModel.PercentageComplete = (int)p;
                PgModel.Msg = msg;
                Progress.Report(PgModel);
            });
        }

        public Task CancelTask()
        {
            return Task.Run(() =>
            {
                Cancellation.Cancel();
            });
        }

        public async Task ConvertXmlToPdf(TipoTask task, List<Detalle> lista)
        {
            int c = 0;

            PgModel.StopWatch.Start();
            List<Task<IPdfDocument>> tareas = new List<Task<IPdfDocument>>();
            IPdfDocument pdfDocument = null;
            string path = @"C:\Centralizador\Pdf\" + UserParticipant.BusinessName;
            new CreateFile($"{path}");
            try
            {
                await Task.Run(() =>
                {
                    tareas = lista.Select(async item =>
                    {
                        if (item.DTEDef != null)
                        {
                            await HConvertToPdf.EncodeTimbre417(item, task).ContinueWith(async x =>
                            {
                                await HConvertToPdf.XmlToPdf(item, path);
                            });
                        }
                        c++;
                        float porcent = (float)(100 * c) / lista.Count;
                        await ReportProgress(porcent, $"Converting doc N° [{item.Folio}] to PDF.    ({c}/{lista.Count})");
                        return pdfDocument;
                    }).ToList();
                });
                await Task.WhenAll(tareas).ContinueWith(x =>
                {
                    Process.Start(path);
                });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // INFO
                PgModel.StopWatch.Stop();
            }
        }

        public async Task ConvertXmlToPdf(Detalle d, TipoTask task)
        {
            // INFO
            PgModel.StopWatch.Start();

            IPdfDocument pdfDocument = null;
            try
            {
                //TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                string path = @"C:\Centralizador\Pdf\" + UserParticipant.BusinessName;
                new CreateFile($"{path}");
                string nomenclatura = null;
                await Task.Run(async () =>
                {
                    if (d.DTEDef != null)
                    {
                        await HConvertToPdf.EncodeTimbre417(d, task);
                        nomenclatura = await HConvertToPdf.XmlToPdf(d, path);
                    }
                    return pdfDocument;
                }).ContinueWith(x =>
                {
                    //string nomenclatura = d.Folio + "_" + ti.ToTitleCase(d.RznSocRecep.ToLower());
                    //Process.Start(path + "\\" + nomenclatura + ".pdf");
                    Process.Start(nomenclatura);
                });
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // INFO
                PgModel.StopWatch.Stop();
            }
        }

        public async Task GetDocFromStore(DateTime period)
        {
            // INFO
            PgModel.StopWatch.Start();
            BilingTypes = await BilingType.GetBilinTypesAsync();

            string nameFile = @"C:\Centralizador\Inbox\" + period.Year + @"\" + period.Month;
            string p = $"{period.Year}-{string.Format("{0:00}", period.Month)}";
            List<Detalle> lista33 = await ServiceDetalle.GetLibroAsync("Debtor", UserParticipant, "33", p, TokenSii, Progress);
            List<Detalle> lista34 = await ServiceDetalle.GetLibroAsync("Debtor", UserParticipant, "34", p, TokenSii, Progress);
            List<Detalle> listaTotal = new List<Detalle>();

            if (lista33 != null)
            {
                foreach (var item in lista33)
                {
                    item.Tipo = "33";
                    listaTotal.Add(item);
                }
            }
            if (lista34 != null)
            {
                foreach (var item in lista34)
                {
                    item.Tipo = "34";
                    listaTotal.Add(item);
                }
            }
            if (listaTotal != null && listaTotal.Count > 0)
            {
                try
                {
                    DetalleList = await GetDebtor(listaTotal, nameFile);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void Dispose()
        {
            PgModel.StopWatch.Stop();
            IsRuning = false;
        }

        public async Task<List<Detalle>> GetDebtor(List<Detalle> detalles, string p)
        {
            int c = 0;
            ServiceEvento dataEvento = new ServiceEvento(TokenSii);
            List<Detalle> detallesFinal = new List<Detalle>();
            List<Task<List<Detalle>>> tareas = new List<Task<List<Detalle>>>();
            tareas = detalles.Select(async item =>
            {
                DTEDefType xmlObjeto = null;
                string nameFile = null;
                // GET XML FILE

                nameFile = p + $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__{item.Tipo}__{item.Folio}.xml";

                if (File.Exists(nameFile)) { xmlObjeto = HSerialize.DTE_To_Object(nameFile); }
                // GET PARTICPANT INFO FROM CEN
                ResultParticipant participant = await Participant.GetParticipantByRutAsync(item.RutReceptor.ToString());
                if (participant != null && participant.Id > 0)
                {
                    item.IsParticipant = true;
                    item.ParticipantMising = participant;
                }
                if (xmlObjeto != null)
                {
                    item.DTEDef = xmlObjeto;
                    if (item.IsParticipant)
                    {
                        // GET REFERENCE SEN.
                        DTEDefTypeDocumentoReferencia r = null;
                        GetReferenceCen doc = new GetReferenceCen(item);
                        if (doc != null) { r = doc.DocumentoReferencia; }
                        if (r != null && r.RazonRef != null)
                        {
                            // GET WINDOW.
                            ResultBillingWindow window = await BillingWindow.GetBillingWindowByNaturalKeyAsync(r);
                            // GET MATRIX.
                            if (window != null && window.Id > 0)
                            {
                                List<ResultPaymentMatrix> matrices = await PaymentMatrix.GetPaymentMatrixByBillingWindowIdAsync(window);
                                if (matrices != null && matrices.Count > 0)
                                {
                                    ResultPaymentMatrix matrix = matrices.FirstOrDefault(x => x.NaturalKey.Equals(r.RazonRef.Trim(), StringComparison.OrdinalIgnoreCase));
                                    if (matrix != null)
                                    {
                                        ResultInstruction instruction = await Instruction.GetInstructionDebtorAsync(matrix, participant, UserParticipant);
                                        if (instruction != null && instruction.Id > 0)
                                        {
                                            item.Instruction = instruction;
                                            item.Instruction.ParticipantCreditor = participant;
                                            item.Instruction.ParticipantDebtor = UserParticipant;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // FLAGS IF EXISTS XML FILE
                item.ValidatorFlag = new HFlagValidator(item, false);
                // EVENTS FROM SII
                item.DataEvento = await dataEvento.GetStatusDteAsync2("Debtor", TokenSii, "33", item, UserParticipant);
                // STATUS DOC
                if (item.DataEvento != null) { item.StatusDetalle = GetStatus(item); }
                // INSERT IN CEN
                if (item.StatusDetalle == StatusDetalle.Accepted && item.Instruction != null)
                {
                    // 1 No Facturado y cuando hay más de 1 dte informado
                    // 2 Facturado
                    // 3 Facturado con retraso
                    // Existe el DTE?
                    ResultDte doc = await Dte.GetDteAsync(item, false);
                    if (doc == null)
                    {
                        // Enviar el DTE
                        ResultDte resultDte = await Dte.SendDteDebtorAsync(item, TokenCen);
                        if (resultDte != null && resultDte.Folio > 0)
                        {
                            item.Instruction.Dte = resultDte;
                        }
                    }
                    else
                    {
                        item.Instruction.Dte = doc;
                    }
                }
                detallesFinal.Add(item);
                c++;
                float porcent = (float)(100 * c) / detalles.Count;
                await ReportProgress(porcent, $"Processing 'Pay Instructions' {item.Folio}, wait please.  ({c}/{detalles.Count})");
                return detalles;
            }).ToList();
            await Task.WhenAll(tareas);
            return detalles.OrderBy(x => x.FechaRecepcion).ToList();
        }
    }
}