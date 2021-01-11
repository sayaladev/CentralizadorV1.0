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
    public class DetalleDebtor : IDetalleD
    {
        public string DataBaseName { get; set; }
        public ResultParticipant UserParticipant { get; set; }
        public string TokenSii { get; set; }
        public string TokenCen { get; set; }
        public ProgressReportModel ProgressReport { get; set; }
        public StringBuilder StringLogging { get; set; }

        public DetalleDebtor(string dataBaseName, ResultParticipant userParticipant, string tokenSii, string tokenCen)
        {
            DataBaseName = dataBaseName;
            UserParticipant = userParticipant;
            TokenSii = tokenSii;
            TokenCen = tokenCen;
            ProgressReport = new ProgressReportModel(ProgressReportModel.TipoTask.GetDebtor);
            StringLogging = new StringBuilder();
        }

        public async Task<List<Detalle>> GetDetalleDebtor(List<Detalle> detalles, IProgress<ProgressReportModel> progress, string p)
        {
            int c = 0;
            List<Detalle> detallesFinal = new List<Detalle>();
            var tareas = new List<Task<List<Detalle>>>();
            tareas = detalles.Select(async item =>
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
                if (xmlObjeto != null && item.IsParticipant)
                {
                    item.DTEDef = xmlObjeto;
                    // GET REFERENCE SEN.
                    DTEDefTypeDocumentoReferencia r = null;
                    var doc = new GetReferenceCen(item);
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
                ProgressReport.SetMessage($"Retrieving information from SII, wait please.  ({c}/{detalles.Count})");
                progress.Report(ProgressReport);
                return detalles;
            }).ToList();
            await Task.WhenAll(tareas);
            return detalles.OrderBy(x => x.FechaRecepcion).ToList();
        }
    }
}