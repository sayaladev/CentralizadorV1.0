using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.AppFunctions;
using static Centralizador.Models.ApiSII.ServiceDetalle;

namespace Centralizador.Models.Interfaces
{
    internal class DetalleDebtor : IDetalleD
    {
        public string DataBaseName { get; set; }
        public ResultParticipant UserParticipant { get; set; }
        public string TokenSii { get; set; }
        public string TokenCen { get; set; }
        public ProgressReportModel ProgressReport { get; set; }
        public StringBuilder StringLogging { get; set; }

        public DetalleDebtor(string dataBaseName, ResultParticipant userParticipant, string tokenSii, string tokenCen, StringBuilder stringLogging)
        {
            DataBaseName = dataBaseName;
            UserParticipant = userParticipant;
            TokenSii = tokenSii;
            TokenCen = tokenCen;
            ProgressReport = new ProgressReportModel(ProgressReportModel.TipoTask.GetDebtor);
            StringLogging = stringLogging;
        }

        public async Task<List<Detalle>> GetDetalleDebtor(List<Detalle> detalles, IProgress<ProgressReportModel> progress, CancellationToken tokenCancel, string p)
        {
            int c = 0;
            List<Detalle> detallesFinal = new List<Detalle>();
            foreach (Detalle item in detalles)
            {
                // TESTER
                //var folio = item.Folio;

                try
                {
                    DTEDefType xmlObjeto = null;
                    // GET XML FILE
                    string nameFile = p + $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__33__{item.Folio}.xml";
                    if (File.Exists(nameFile)) { xmlObjeto = ServicePdf.TransformXmlDTEDefTypeToObjectDTE(nameFile); }
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
                        // GET REFERENCE SEN.
                        DTEDefTypeDocumentoReferencia r = new GetReferenceCen(item).DocumentoReferencia;
                        if (r != null && r.RazonRef != null)
                        {
                            // Get Window
                            ResultBillingWindow window = await BillingWindow.GetBillingWindowByNaturalKeyAsync(r);
                            // Get Matrix
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
                    item.ValidatorFlag = new ValidatorFlag(item, false);
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
                    ProgressReport.PercentageComplete = (int)porcent;
                    ProgressReport.Message = $"Retrieving information from SII, wait please.  ({c}/{detalles.Count})";
                    progress.Report(ProgressReport);
                    if (tokenCancel.IsCancellationRequested) { tokenCancel.ThrowIfCancellationRequested(); }
                }
                catch (OperationCanceledException) when (tokenCancel.IsCancellationRequested)
                {
                    ProgressReport.Message = "Task canceled...  !";
                    ProgressReport.PercentageComplete = 100;
                    progress.Report(ProgressReport);
                    return detallesFinal.OrderBy(x => x.FechaRecepcion).ToList();
                }
            }
            return detallesFinal.OrderBy(x => x.FechaRecepcion).ToList();
        }
    }
}