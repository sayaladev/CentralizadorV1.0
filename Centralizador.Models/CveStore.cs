using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;

using Centralizador.Models.DataBase;
using Centralizador.Models.Helpers;
using OpenHtmlToPdf;
using static Centralizador.Models.Helpers.HFlagValidator;

namespace Centralizador.Models
{
    public abstract class CveStore
    {
        // CTOR.
        protected CveStore(ResultParticipant userParticipant,
            IProgress<HPgModel> progress,
            string dataBaseName,
            string tokenSii,
            string tokenCen)
        {
            UserParticipant = userParticipant;
            Progress = progress;
            CancelToken = new CancellationTokenSource();
            Conn = new Conexion(dataBaseName);
            PgModel = new HPgModel();
            TokenSii = tokenSii;
            TokenCen = tokenCen;
        }

        // PROP.
        public string TokenSii { get; set; }

        public string TokenCen { get; set; }
        public ResultParticipant UserParticipant { get; set; }
        public List<ResultBilingType> BilingTypes { get; set; }
        public HPgModel PgModel { get; set; }
        public IProgress<HPgModel> Progress { get; set; }

        public CancellationTokenSource CancelToken { get; set; }
        public Conexion Conn { get; set; }
        public List<Detalle> DetalleList { get; set; }

        public StringBuilder StringLogging { get; set; } = new StringBuilder();

        // ENUM.
        public enum TipoTask
        {
            Debtor,
            Creditor
        }

        // OVERRIDABLE.
        public abstract Task GetDocFromStore(DateTime period);

        public abstract Task InsertNotaVenta();

        // METHODS.
        public Task CancelTask()
        {
            return Task.Run(async () =>
            {
                CancelToken.Cancel();
                await ReportProgress(100, "Cancelada!");
                PgModel.IsBussy = false;
            });
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

        public async void DeleteNV()
        {
            await NotaVenta.DeleteNvAsync(Conn);
        }

        public async Task<List<Detalle>> GetCreditor(List<ResultInstruction> list)
        {
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
                                            detalle.DTEDef = HSerialize.TransformStringDTEDefTypeToObjectDTE(infoLastF.DteFiles[0].Archivo);
                                            detalle.DTEFile = infoLastF.DteFiles[0].Archivo;
                                        }
                                        break;

                                    default:
                                        {
                                            detalle.DTEDef = HSerialize.TransformStringDTEDefTypeToObjectDTE(infoLastF.DteFiles.FirstOrDefault(x => x.TipoXML == "D").Archivo);
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
                                DataEvento evento = await ServiceEvento.GetStatusDteAsync("Creditor", TokenSii, "33", detalle, UserParticipant);
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

        public async Task ConvertXmlToPdf()
        {
            // INFO
            PgModel.StopWatch.Start();
            PgModel.IsBussy = true;

            if (DetalleList.Count > 0)
            {
                List<Detalle> lista = new List<Detalle>();
                int c = 0;
                foreach (Detalle item in DetalleList)
                {
                    if (item.DTEDef != null)
                    {
                        lista.Add(item);
                    }
                }
                List<Task<IPdfDocument>> tareas = new List<Task<IPdfDocument>>();
                IPdfDocument pdfDocument = null;
                string path = @"C:\Centralizador\Pdf\" + UserParticipant.BusinessName;
                new CreateFile($"{path}");
                await Task.Run(() =>
                {
                    tareas = lista.Select(async item =>
                    {
                        if (item.DTEDef != null)
                        {
                            await HFiles.EncodeTimbre417(item).ContinueWith(async x =>
                            {
                                await HFiles.HtmlToXmlTransform(item, path);
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
            // INFO
            PgModel.StopWatch.Stop();
            PgModel.IsBussy = false;
        }

        public async Task<List<Detalle>> GetDebtor(List<Detalle> detalles, string p)
        {
            int c = 0;
            List<Detalle> detallesFinal = new List<Detalle>();
            List<Task<List<Detalle>>> tareas = new List<Task<List<Detalle>>>();
            tareas = detalles.Select(async item =>
            {
                DTEDefType xmlObjeto = null;
                // GET XML FILE
                string nameFile = p + $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__33__{item.Folio}.xml";
                if (File.Exists(nameFile)) { xmlObjeto = HSerialize.TransformXmlDTEDefTypeToObjectDTE(nameFile); }
                // GET PARTICPANT INFO FROM CEN
                ResultParticipant participant = await Participant.GetParticipantByRutAsync(item.RutReceptor.ToString());
                if (participant != null && participant.Id > 0)
                {
                    item.IsParticipant = true;
                    item.ParticipantMising = participant;
                }
                if (xmlObjeto != null && item.IsParticipant)
                {
                    item.DTEDef = xmlObjeto;
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
                // FLAGS IF EXISTS XML FILE
                item.ValidatorFlag = new HFlagValidator(item, false);
                // EVENTS FROM SII
                item.DataEvento = await ServiceEvento.GetStatusDteAsync("Debtor", TokenSii, "33", item, UserParticipant);
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

        public async Task<List<int>> InsertNv(List<Detalle> detalles)
        {
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
                        StringLogging.AppendLine($"{item.Instruction.Id}\tInsert NV:\tF°: {lastF}");
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
    }
}