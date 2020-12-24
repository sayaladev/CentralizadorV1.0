using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.AppFunctions;
using Centralizador.Models.DataBase;

using static Centralizador.Models.ApiSII.ServiceDetalle;
using static Centralizador.Models.AppFunctions.ValidatorFlag;

namespace Centralizador.Models
{
    public class DetalleI : IDetalle
    {
        public DetalleI(string dataBaseName, ResultParticipant userParticipant, string tokenSii, string tokenCen, ProgressReportModel reportModel)
        {
            DataBaseName = dataBaseName;
            UserParticipant = userParticipant;
            TokenSii = tokenSii;
            TokenCen = tokenCen;
            ReportModel = reportModel;
        }

        public  string DataBaseName { get; set; }
        public ResultParticipant UserParticipant { get; set; }
        public string TokenSii { get; set; }
        public string TokenCen { get; set; }
        public ProgressReportModel ReportModel { get; set; }

        public async Task<List<Detalle>> GetDetalleCreditor(List<ResultPaymentMatrix> matrices, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            int c = 0;
            float porcent;
            List<Detalle> detalles = new List<Detalle>();
            Conexion con = new Conexion(DataBaseName, Properties.Settings.Default.DBUser, Properties.Settings.Default.DBPassword);
            foreach (ResultPaymentMatrix m in matrices)
            {
                // GET BILLING WINDOW.
                int d = 0;
                m.BillingWindow = await BillingWindow.GetBillingWindowByIdAsync(m);
                IList<ResultInstruction> InstructionsList = await Instruction.GetInstructionCreditorAsync(m, UserParticipant);
                if (InstructionsList != null)
                {
                    try
                    {
                        DteInfoRef infoLastF = null;
                        Dictionary<string, int> dic = GetReemplazosFile();
                        foreach (ResultInstruction instruction in InstructionsList)
                        {
                            // GET PARTICIPANT DEBTOR
                            instruction.ParticipantDebtor = await Participant.GetParticipantByIdAsync(instruction.Debtor);
                            // REEMPLAZOS
                            if (dic.ContainsKey(instruction.ParticipantDebtor.Id.ToString()))
                            {
                                instruction.ParticipantNew = await Participant.GetParticipantByIdAsync(dic[instruction.ParticipantDebtor.Id.ToString()]);
                            }
                            // ROOT CLASS.
                            Detalle detalle = new Detalle(instruction.ParticipantDebtor.Rut, instruction.ParticipantDebtor.VerificationCode, instruction.ParticipantDebtor.BusinessName, instruction.Amount, instruction, true);
                            // GET INFO OF INVOICES.
                            IList<DteInfoRef> dteInfos = await DteInfoRef.GetInfoRefAsync(instruction, con, "F");
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
                                detalle.DteInfoRefs = dteInfoRefs; // ATTACH FILES
                                foreach (DteInfoRef item in detalle.DteInfoRefs)
                                {
                                    IList<DteFiles> files = await DteFiles.GetDteFilesAsync(con, item.NroInt, item.Folio);
                                    if (files != null)
                                    {
                                        item.DteFiles = files;
                                    }
                                    else
                                    {
                                        files = await DteFiles.GetDteFilesAsync(con, item.IDXMLDoc);
                                        item.DteFiles = files;
                                    }
                                }
                                // ATTACH PRINCIPAL DOC.
                                if (detalle.DteInfoRefs.Count >= 1)
                                {
                                    infoLastF = detalle.DteInfoRefs.First(); // SHOW THE LAST DOC.                                    
                                    switch (detalle.DteInfoRefs.First().DteFiles.Count)
                                    {
                                        case 1:
                                            if (infoLastF.DteFiles[0].TipoXML == null)
                                            {
                                                detalle.DTEDef = ServicePdf.TransformStringDTEDefTypeToObjectDTE(infoLastF.DteFiles[0].Archivo);
                                            }
                                            break;
                                        default:
                                            {
                                                // Atach el XML veriones nuevas                          
                                                detalle.DTEDef = ServicePdf.TransformStringDTEDefTypeToObjectDTE(infoLastF.DteFiles.FirstOrDefault(x => x.TipoXML == "D").Archivo);
                                                break;
                                            }
                                    }
                                    // SET REF MISSING
                                    DTEDefTypeDocumento obj = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                                    DTEDefTypeDocumentoReferencia[] refr = obj.Referencia;
                                    DTEDefTypeDocumentoReferencia r = refr.FirstOrDefault(x => x.TpoDocRef.ToUpper() == "SEN");
                                    if (infoLastF.EnviadoSII == 0) // No ha sido enviado a SII
                                    {
                                        if (r == null || string.IsNullOrEmpty(infoLastF.Glosa) || string.IsNullOrEmpty(infoLastF.FolioRef))
                                        {
                                            detalle.RefMissing = true; // NO REF IN DTE
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
                                        DataEvento evento = await ServiceEvento.GetStatusDteAsync("Creditor", TokenSii, "33", detalle, UserParticipant, Properties.Settings.Default.SerialDigitalCert);
                                        if (evento != null)
                                        {
                                            detalle.DataEvento = evento;
                                            detalle.StatusDetalle = ServiceDetalle.GetStatus(detalle);
                                        }
                                    }
                                    if (detalle.StatusDetalle == ServiceDetalle.StatusDetalle.Pending && infoLastF.AceptadoCliente == 1)
                                    {
                                        detalle.FechaRecepcion = infoLastF.FechaEnvioSII.ToString("dd-MM-yyyy");
                                        detalle.StatusDetalle = ServiceDetalle.StatusDetalle.Accepted;
                                    }
                                    else if (detalle.StatusDetalle == ServiceDetalle.StatusDetalle.Accepted && infoLastF.AceptadoCliente == 0)
                                    {
                                        DteFiles.UpdateFiles(con, detalle); // UPDATE DTE_DocCab SET infoLastF.EnviadoCliente = 1
                                    }
                                }
                                // SEND DTE TO CEN.
                                if (detalle.StatusDetalle == ServiceDetalle.StatusDetalle.Accepted && detalle.Instruction != null && detalle.Instruction.StatusBilled == Instruction.StatusBilled.NoFacturado)
                                {
                                    string xmlDoc = ServicePdf.TransformObjectToXmlForCen(detalle.DTEDef);
                                    ResultDte resultDte = await Dte.SendDteCreditorAsync(detalle, TokenCen, xmlDoc);
                                    if (resultDte != null)
                                    {
                                        detalle.Instruction.Dte = resultDte;
                                        detalle.Instruction.StatusBilled = Instruction.StatusBilled.Facturado;
                                    }
                                }
                                bool validaCreditor = true;
                                detalle.ValidatorFlag = new ValidatorFlag(detalle, validaCreditor);
                            }
                            else
                            {
                                detalle.ValidatorFlag = new ValidatorFlag() { Flag = LetterFlag.Clear };
                            }
                            detalles.Add(detalle);
                            d++;
                            ReportModel.Message = $"Retrieving informartion from SOFTLAND & SII, wait please.  ({c}/{matrices.Count}) => ({d}/{InstructionsList.Count})";
                            progress.Report(ReportModel);
                            if (cancellationToken.IsCancellationRequested)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }
                        }
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        //ReportModel.Message = "Canceling Task...  !";
                        ReportModel.PercentageComplete = 100;
                        progress.Report(ReportModel);
                        break;
                    }
                }
                c++;
                porcent = (float)(100 * c) / matrices.Count;
                ReportModel.PercentageComplete = (int)porcent;
                ReportModel.Message = $"Searching 'PAY INSTRUCTIONS' from CEN, wait please.  ({c}/{matrices.Count})";
                progress.Report(ReportModel);
            }
            return detalles;
        }

        private Dictionary<string, int> GetReemplazosFile()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            try
            {
                XDocument doc = XDocument.Load(@"C:\Centralizador\Reemplazos_.xml");
                return doc.Descendants("Empresa").ToDictionary(d => (string)d.Attribute("id"), d => (int)d);
            }
            catch (Exception ex)
            {
                new ErrorMsgCen(@"The file 'C:\Centralizador\Reemplazos_.xml' has problems.", ex, MessageBoxIcon.Stop);
            }
            return null;
        }

        public async void DeleteNV()
        {
            await NotaVenta.DeleteNvAsync(new Conexion(DataBaseName, Properties.Settings.Default.DBUser, Properties.Settings.Default.DBPassword));
        }
              
        public async Task<List<Detalle>> GetDetalleDebtor(List<Detalle> detalles, IProgress<ProgressReportModel> progress, CancellationToken cancellationToken, string p)
        {
            string nameFilePath = p; // @"C:\Centralizador\Inbox\" + CboYears.SelectedItem + @"\" + (CboMonths.SelectedIndex + 1);
            string nameFile = "";
            int c = 0;
            List<Detalle> detallesFinal = new List<Detalle>();
            foreach (Detalle item in detalles)
            {
                try
                {
                    DTEDefType xmlObjeto = null;
                    DTEDefTypeDocumento dte = null;
                    DTEDefTypeDocumentoReferencia[] references = null;
                    DTEDefTypeDocumentoReferencia reference = null;
                    ResultParticipant participant = null;
                    // GET XML FILE
                    nameFile = nameFilePath + $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__33__{item.Folio}.xml";
                    if (File.Exists(nameFile)) { xmlObjeto = ServicePdf.TransformXmlDTEDefTypeToObjectDTE(nameFile); }
                    // GET PARTICPANT INFO FROM CEN
                    participant = await Participant.GetParticipantByRutAsync(item.RutReceptor.ToString());
                    if (participant != null && participant.Id > 0) { item.IsParticipant = true; }
                    if (xmlObjeto != null)
                    {
                        item.DTEDef = xmlObjeto;
                        dte = (DTEDefTypeDocumento)xmlObjeto.Item;
                        references = dte.Referencia;
                        if (references != null)
                        {
                            reference = references.FirstOrDefault(x => x.TpoDocRef.ToUpper() == "SEN");
                            if (reference != null && reference.RazonRef != null)
                            {
                                // Get Window                       
                                ResultBillingWindow window = await BillingWindow.GetBillingWindowByNaturalKeyAsync(reference);
                                // Get Matrix
                                if (window != null && window.Id > 0)
                                {
                                    IList<ResultPaymentMatrix> matrices = await PaymentMatrix.GetPaymentMatrixByBillingWindowIdAsync(window);
                                    if (matrices != null && matrices.Count > 0)
                                    {
                                        ResultPaymentMatrix matrix = matrices.FirstOrDefault(x => x.NaturalKey.Equals(reference.RazonRef.Trim(), StringComparison.OrdinalIgnoreCase));
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
                    bool validaCreditor = false;
                    item.ValidatorFlag = new ValidatorFlag(item, validaCreditor);
                    // EVENTS FROM SII  
                    item.DataEvento = await ServiceEvento.GetStatusDteAsync("Debtor", TokenSii, "33", item, UserParticipant, Properties.Settings.Default.SerialDigitalCert);
                    // STATUS DOC
                    if (item.DataEvento != null) { item.StatusDetalle = GetStatus(item); }
                    // INSERT IN CEN
                    if (item.StatusDetalle == StatusDetalle.Accepted && item.Instruction != null && item.Instruction.IsPaid == false)
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
                    ReportModel.PercentageComplete = (int)porcent;
                    ReportModel.Message = $"Retrieving information from SII, wait please.  ({c}/{detalles.Count})";
                    progress.Report(ReportModel);
                    if (cancellationToken.IsCancellationRequested) { cancellationToken.ThrowIfCancellationRequested(); }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    //ReportModel.Message = "Canceling Task...  !";
                    ReportModel.PercentageComplete = 100;
                    progress.Report(ReportModel);
                    break;
                }
            }
            return detallesFinal.OrderBy(x => x.FechaRecepcion).ToList();
        }
       

    }
}
