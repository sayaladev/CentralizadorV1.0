using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.AppFunctions;
using Centralizador.Models.DataBase;
using static Centralizador.Models.ApiSII.ServiceDetalle;
using static Centralizador.Models.AppFunctions.ValidatorFlag;

namespace Centralizador.Models.Interfaces
{
    public class CveCreditor : CveStoreDoc
    {
        // CTOR
        public CveCreditor() : base()
        {
        }

        // CTOR.
        public CveCreditor(TipoTask taskType, ResultParticipant userParticipant, ProgressReportModel pgModel, IProgress<ProgressReportModel> progress, string dataBaseName) : base(taskType, userParticipant, pgModel, progress, dataBaseName)
        {
        }

        public override async Task<List<Detalle>> GetDocFromStore(List<ResultInstruction> list, string tokenSii, string tokenCen)
        {
            DteInfoRef infoLastF = null;
            List<Detalle> detalles = new List<Detalle>();
            Conexion con = new Conexion(DataBaseName);
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
                    List<DteInfoRef> dteInfos = await DteInfoRef.GetInfoRefAsync(instruction, con, "F");
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
                                            detalle.DTEDef = ServicePdf.TransformStringDTEDefTypeToObjectDTE(infoLastF.DteFiles[0].Archivo);
                                            detalle.DTEFile = infoLastF.DteFiles[0].Archivo;
                                        }
                                        break;

                                    default:
                                        {
                                            detalle.DTEDef = ServicePdf.TransformStringDTEDefTypeToObjectDTE(infoLastF.DteFiles.FirstOrDefault(x => x.TipoXML == "D").Archivo);
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
                                DataEvento evento = await ServiceEvento.GetStatusDteAsync("Creditor", tokenSii, "33", detalle, UserParticipant);
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
                                DteFiles.UpdateFiles(con, detalle); // UPDATE DTE_DocCab SET infoLastF.EnviadoCliente = 1
                            }
                        }
                        // SEND DTE TO CEN.
                        if (detalle.StatusDetalle == StatusDetalle.Accepted && detalle.Instruction != null && detalle.Instruction.StatusBilled == Instruction.StatusBilled.NoFacturado)
                        {
                            ResultDte resultDte = await Dte.SendDteCreditorAsync(detalle, tokenCen, detalle.DTEFile);
                            if (resultDte != null)
                            {
                                detalle.Instruction.Dte = resultDte;
                                detalle.Instruction.StatusBilled = Instruction.StatusBilled.Facturado;
                            }
                        }
                        detalle.ValidatorFlag = new ValidatorFlag(detalle, true);
                    }
                    else
                    {
                        detalle.ValidatorFlag = new ValidatorFlag() { Flag = LetterFlag.Clear };
                    }
                    detalles.Add(detalle);
                    c++;
                    porcent = (float)(100 * c) / list.Count;
                    //ReportModel.PercentageComplete = (int)porcent;
                    //ReportModel.SetMessage($"Processing 'Pay Instructions' {instruction.Id}, wait please.  ({c}/{list.Count})");
                    //progress.Report(ReportModel);
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

        public async Task<List<ResultInstruction>> GetInstructions(List<ResultPaymentMatrix> matrices, IProgress<ProgressReportModel> report)
        {
            // DELETE NV.
            DeleteNV();
            // INSERT TRIGGER.
            DteInfoRef.InsertTriggerRefCen(new Conexion(DataBaseName));

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
                //ReportModel.PercentageComplete = (int)porcent;
                //ReportModel.SetMessage($"Processing 'Pay Instructions Matrix' N° {m.Id}, wait please.  ({c}/{matrices.Count})");
                //report.Report(ReportModel);
                return instructions;
            }).ToList();
            await Task.WhenAll(tareas);
            return instructions;
        }
    }
}