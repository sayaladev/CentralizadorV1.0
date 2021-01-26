using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.DataBase;
using Centralizador.Models.Helpers;

using OpenHtmlToPdf;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Centralizador.Models.Helpers.HEnum;
using static Centralizador.Models.Helpers.HFlagValidator;

namespace Centralizador.Models.FunctionsApp
{
    public class CveCreditor : ICveFunction
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
        public List<int> FoliosNv { get; set; }
        public bool IsRuning { get; set; }

        public CveCreditor(ResultParticipant userParticipant, IProgress<HPgModel> progress, string dataBaseName, string tokenSii, string tokenCen)
        {
            DBName = dataBaseName;
            TokenCen = tokenCen;
            UserParticipant = userParticipant;
            TokenSii = tokenSii;
            Mode = TipoTask.Creditor;
            PgModel = new HPgModel();
            Conn = new Conexion(dataBaseName);
            Progress = progress;
            IsRuning = true;
        }

        // CTOR FOR NV INSERT.
        public CveCreditor(string dataBaseName, IProgress<HPgModel> progress, ResultParticipant userParticipant)
        {
            Mode = TipoTask.Creditor;
            IsRuning = true;
            StringLogging = new StringBuilder();
            Conn = new Conexion(dataBaseName);
            PgModel = new HPgModel();
            Progress = progress;
            UserParticipant = userParticipant;
        }

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
            // PgModel.StopWatch.Start();

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
                // PgModel.StopWatch.Stop();
            }
        }

        public async Task GetDocFromStore(DateTime period)
        {
            BilingTypes = await BilingType.GetBilinTypesAsync();
            List<ResultPaymentMatrix> matrices = await PaymentMatrix.GetPaymentMatrixAsync(period);
            if (matrices != null && matrices.Count > 0)
            {
                // DELETE NV.
                DeleteNV();
                // INSERT TRIGGER.
                DteInfoRef.InsertTriggerRefCen(Conn);
                List<ResultInstruction> i = await GetInstructions(matrices);
                if (i.Count > 0)
                {
                    try
                    {
                        DetalleList = await GetCreditor(i);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    DetalleList = null;
                }
            }
        }

        public async Task<List<int>> InsertNv(List<Detalle> detalles)
        {
            BilingTypes = await BilingType.GetBilinTypesAsync();
            int c = 0;
            float porcent = 0;
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            int resultInsertNV;
            int lastF = 0;
            List<int> folios = new List<int>();
            foreach (Detalle item in detalles)
            {
                AuxCsv a = FileSii.GetAuxCvsFromFile(item); // INFORMATION UPDATE FROM CSV FILE.
                if (a != null)
                {
                    string name = ti.ToTitleCase(a.Name.ToLower());
                    item.Instruction.ParticipantDebtor.BusinessName = name;
                    item.Instruction.ParticipantDebtor.DteReceptionEmail = a.Email;
                    item.Instruction.ParticipantDebtor.Name = item.Instruction.ParticipantDebtor.Name.ToUpper();
                }
                else
                {
                    StringLogging.AppendLine($"{item.Instruction.Id}\tUpdate email\t\tError in CSV file.");
                    continue;
                }
                Auxiliar aux = await Auxiliar.GetAuxiliarAsync(item.Instruction, Conn);
                Comuna comunaobj;
                if (aux == null) // INSERT NEW AUX.
                {
                    comunaobj = await Comuna.GetComunaFromInput(item, Conn, true);
                    aux = await Auxiliar.InsertAuxiliarAsync(item.Instruction, Conn, comunaobj);
                    if (aux != null)
                    {
                        StringLogging.AppendLine($"{item.Instruction.Id}\tAuxiliar Insert:\tOk: {item.Instruction.ParticipantDebtor.Rut} / {aux.DirAux} / {aux.ComAux}");
                    }
                    else
                    {
                        StringLogging.AppendLine($"{item.Instruction.Id}\tAuxiliar Error:\t {item.Instruction.ParticipantDebtor.Rut}");
                    }
                }
                else // UPDATE ALL AUX FROM LIST.
                {
                    if (aux.ComAux == null)
                    {
                        comunaobj = await Comuna.GetComunaFromInput(item, Conn, false);
                    }
                    else
                    {
                        comunaobj = new Comuna { ComCod = aux.ComAux };
                    }
                    if (await aux.UpdateAuxiliarAsync(item.Instruction, Conn, comunaobj) == 0)
                    {
                        StringLogging.AppendLine($"{item.Instruction.Id}\tAuxiliar Update:\tError Sql: {item.Instruction.ParticipantDebtor.Rut}");
                        continue;
                    }
                }
                // INSERT THE NV.
                lastF = await NotaVenta.GetLastNv(Conn);
                string prod = BilingTypes.FirstOrDefault(x => x.Id == item.Instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                resultInsertNV = await NotaVenta.InsertNvAsync(item.Instruction, lastF, prod, Conn);
                if (resultInsertNV == 0)
                {
                    StringLogging.AppendLine($"{item.Instruction.Id}\tInsert NV:\tError Sql");
                }
                else if (resultInsertNV == 1) // SUCCESS INSERT.
                {
                    if (item.Instruction.ParticipantNew == null)
                    {
                        StringLogging.AppendLine($"{item.Nro}-{item.Instruction.Id}\tInsert NV:\tF°: {lastF}");
                    }
                    else
                    {
                        StringLogging.AppendLine($"{item.Instruction.Id}\tInsert NV:\tF°: {lastF}  *Change RUT {item.Instruction.ParticipantDebtor.Rut} by {item.Instruction.ParticipantNew.Rut}");
                    }
                    folios.Add(lastF);
                }
                c++;
                porcent = (float)(100 * c) / detalles.Count;
                await ReportProgress(porcent, $"Inserting NV, wait please...   ({c}/{detalles.Count})  F°: {lastF})");
            }
            return folios;
        }

        public void SaveLogging(string path, string nameFile)
        {
            new CreateFile(path, StringLogging, nameFile);
        }

        public async Task InsertNotaVenta(List<Detalle> detalles)
        {
            StringLogging.Clear();
            // GET VALUES LIST FROM CSV.
            FileSii.ReadFileSii();
            FoliosNv = await InsertNv(detalles);
            if (FoliosNv != null && FoliosNv.Count > 0)
            {
                string nameFile = $"{UserParticipant.Name}_InsertNv_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";
                StringLogging.AppendLine("");
                StringLogging.AppendLine($"Summary: From {FoliosNv.Min()} To-{FoliosNv.Max()}");
                SaveLogging(@"C:\Centralizador\Log\", nameFile);
            }
        }

        public void Dispose()
        {
            // INFO
            PgModel.StopWatch.Stop();
            IsRuning = false;
        }

        public async Task<List<Detalle>> GetCreditor(List<ResultInstruction> list)
        {
            ServiceEvento dataEvento = new ServiceEvento(TokenSii);
            DteInfoRef infoLastF = null;
            List<Detalle> detalles = new List<Detalle>();
            Dictionary<string, int> dic = Properties.Settings.Default.DicReem;
            int c = 0;
            float porcent;
            List<Task<List<Detalle>>> tareas = new List<Task<List<Detalle>>>();
            tareas = list.Select(async instruction =>
            {
                try
                {
                    // GET PARTICIPANT DEBTOR
                    instruction.ParticipantDebtor = await Participant.GetParticipantByIdAsync(instruction.Debtor);
                    //REEMPLAZOS
                    if (dic.ContainsKey(instruction.ParticipantDebtor.Id.ToString()))
                    {
                        instruction.ParticipantNew = await Participant.GetParticipantByIdAsync(dic[instruction.ParticipantDebtor.Id.ToString()]);
                    }
                    // ROOT CLASS.
                    Detalle detalle = new Detalle(instruction.ParticipantDebtor.Rut, instruction.ParticipantDebtor.VerificationCode, instruction.ParticipantDebtor.BusinessName, instruction.Amount, instruction, true);
                    // GET INFO OF INVOICES.
                    List<DteInfoRef> dteInfos = await DteInfoRef.GetInfoRefAsync(instruction, Conn, "F");
                    List<DteInfoRef> dteInfoRefs = new List<DteInfoRef>();
                    if (dteInfos != null)
                    {
                        foreach (DteInfoRef item in dteInfos)
                        {
                            if (string.Compare(item.Glosa, instruction.PaymentMatrix.NaturalKey, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                dteInfoRefs.Add(item);
                            }
                        }
                        // ATTACH FILES.
                        detalle.DteInfoRefs = dteInfoRefs;
                        // ATTACH PRINCIPAL DOC.
                        if (detalle.DteInfoRefs.Count >= 1)
                        {
                            infoLastF = detalle.DteInfoRefs.First(); // SHOW THE LAST DOC.
                            if (dteInfoRefs.First().DteFiles != null)
                            {
                                switch (detalle.DteInfoRefs.First().DteFiles.Count)
                                {
                                    case 1:
                                        if (infoLastF.DteFiles[0].TipoXML == null)
                                        {
                                            detalle.DTEDef = HSerialize.DTE_To_Object(infoLastF.DteFiles[0].Archivo);
                                            detalle.DTEFile = infoLastF.DteFiles[0].Archivo;
                                        }
                                        break;

                                    default:
                                        {
                                            detalle.DTEDef = HSerialize.DTE_To_Object(infoLastF.DteFiles.FirstOrDefault(x => x.TipoXML == "D").Archivo);
                                            detalle.DTEFile = infoLastF.DteFiles.FirstOrDefault(x => x.TipoXML == "D").Archivo;
                                            break;
                                        }
                                }
                            }
                            detalle.DteInfoRefLast = infoLastF;
                            detalle.NroInt = infoLastF.NroInt;
                            detalle.FechaEmision = infoLastF.Fecha.ToString();
                            detalle.Folio = infoLastF.Folio;
                            detalle.MntNeto = infoLastF.NetoAfecto;
                            detalle.MntIva = infoLastF.IVA;
                            detalle.MntTotal = infoLastF.Total;
                            // GET INFO FROM SII
                            if (infoLastF.EnviadoSII == 1 && infoLastF.AceptadoCliente == 0) // 1 Enviado / 0 No enviado
                            {
                                detalle.FechaRecepcion = infoLastF.FechaEnvioSII.ToString("dd-MM-yyyy");
                                // EVENTS FROM SII
                                DataEvento evento = await dataEvento.GetStatusDteAsync("Creditor", TokenSii, "33", detalle, UserParticipant);
                                if (evento != null)
                                {
                                    detalle.DataEvento = evento;
                                    detalle.StatusDetalle = GetStatus(detalle);
                                }
                            }
                            if (detalle.StatusDetalle == StatusDetalle.Pending && infoLastF.AceptadoCliente == 1)
                            {
                                detalle.FechaRecepcion = infoLastF.FechaEnvioSII.ToString("dd-MM-yyyy");
                                detalle.StatusDetalle = StatusDetalle.Accepted;
                            }
                            else if (detalle.StatusDetalle == StatusDetalle.Accepted && infoLastF.AceptadoCliente == 0)
                            {
                                DteFiles.UpdateFiles(Conn, detalle); // UPDATE DTE_DocCab SET infoLastF.EnviadoCliente = 1
                            }
                        }
                        // SEND DTE TO CEN.
                        if (detalle.StatusDetalle == StatusDetalle.Accepted && detalle.Instruction != null && detalle.Instruction.StatusBilled == Instruction.StatusBilled.NoFacturado)
                        {
                            ResultDte resultDte = await Dte.SendDteCreditorAsync(detalle, TokenCen, detalle.DTEFile);
                            if (resultDte != null)
                            {
                                detalle.Instruction.Dte = resultDte;
                                detalle.Instruction.StatusBilled = Instruction.StatusBilled.Facturado;
                            }
                        }
                        detalle.ValidatorFlag = new HFlagValidator(detalle, true);
                    }
                    else
                    {
                        detalle.ValidatorFlag = new HFlagValidator() { Flag = LetterFlag.Clear };
                    }
                    detalles.Add(detalle);
                    c++;
                    porcent = (float)(100 * c) / list.Count;
                    await ReportProgress(porcent, $"Processing 'Pay Instructions' {instruction.Id}, wait please.  ({c}/{list.Count})");
                    return detalles;
                }
                catch (Exception)
                {
                    return null;
                    throw;
                }
            }).ToList();
            await Task.WhenAll(tareas);
            return detalles.OrderBy(x => x.Instruction.Id).ToList();
        }

        public async void DeleteNV()
        {
            await NotaVenta.DeleteNvAsync(Conn);
        }

        public async Task<List<ResultInstruction>> GetInstructions(List<ResultPaymentMatrix> matrices)
        {
            List<ResultInstruction> instructions = new List<ResultInstruction>();
            List<Task<List<ResultInstruction>>> tareas = new List<Task<List<ResultInstruction>>>();
            int c = 0; float porcent;
            tareas = matrices.Select(async m =>
            {
                m.BillingWindow = await BillingWindow.GetBillingWindowByIdAsync(m);
                List<ResultInstruction> listResult = await Instruction.GetInstructionCreditorAsync(m, UserParticipant);
                if (listResult != null)
                {
                    instructions.AddRange(listResult);
                }
                c++;
                porcent = (float)(100 * c) / matrices.Count;
                await ReportProgress(porcent, $"Processing 'Pay Instructions Matrix' N° {m.Id}, wait please.  ({c}/{matrices.Count})");
                return instructions;
            }).ToList();
            await Task.WhenAll(tareas);
            return instructions;
        }
    }
}