using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.AppFunctions;
using Centralizador.Models.DataBase;
using Centralizador.Models.Outlook;

using TenTec.Windows.iGridLib;

using Timer = System.Windows.Forms.Timer;

namespace Centralizador.WinApp.GUI
{

    public partial class FormMain : Form
    {
        #region Global variables/prop

        //Creditor     
        private IList<Detalle> DetallesCreditor { get; set; }
        private BackgroundWorker BgwCreditor { get; set; }
        private BackgroundWorker Bgwinvoicing { get; set; }

        //Debitor
        private IList<Detalle> DetallesDebtor { get; set; }
        private BackgroundWorker BgwDebtor { get; set; }

        //General
        private ResultParticipant UserParticipant { get; set; }
        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");
        private IEnumerable<ResultBilingType> BillingTypes { get; set; }
        public string TokenSii { get; set; }
        private bool IsCreditor { get; set; }
        public bool IsRunning { get; set; }
        public ServiceOutlook ServiceOutlook { get; set; }
        public int Intervalo { get; set; }

        #endregion

        #region FormMain methods

        public FormMain(string value)
        {
            InitializeComponent();
            TokenSii = value;
        }
        private void FormMain_Load(object sender, EventArgs e)
        {

            //Load ComboBox with CVE companies 
            ResultAgent agent = Agent.GetAgetByEmail();
            IList<ResultParticipant> participants = new List<ResultParticipant>();
            foreach (ResultParticipant item in agent.Participants)
            {
                participants.Add(Participant.GetParticipantById(item.ParticipantId));
            }
            participants.Insert(0, new ResultParticipant { Name = "Please select a Company" });
            CboParticipants.DisplayMember = "Name";
            CboParticipants.DataSource = participants;
            CboParticipants.SelectedIndex = 0;

            //Load ComboBox months
            CboMonths.DataSource = DateTimeFormatInfo.InvariantInfo.MonthNames.Take(12).ToList();
            CboMonths.SelectedIndex = DateTime.Today.Month - 1;
            //Load ComboBox years
            CboYears.DataSource = Enumerable.Range(2019, 2).ToList();
            CboYears.SelectedItem = DateTime.Today.Year;

            //Biling types
            BillingTypes = BilingType.GetBilinTypes();

            // User email
            TssLblUserEmail.Text = "|  " + agent.Email;

            // Worker Debtor
            BgwDebtor = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            BgwDebtor.ProgressChanged += BgwDebtor_ProgressChanged;
            BgwDebtor.RunWorkerCompleted += BgwDebtor_RunWorkerCompleted;
            BgwDebtor.DoWork += BgwDebtor_DoWork;
            // Worker Creditor
            BgwCreditor = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            BgwCreditor.ProgressChanged += BgwCreditor_ProgressChanged;
            BgwCreditor.RunWorkerCompleted += BgwCreditor_RunWorkerCompleted;
            BgwCreditor.DoWork += BgwCreditor_DoWork;

            Bgwinvoicing = new BackgroundWorker();
            Bgwinvoicing.WorkerReportsProgress = true;
            Bgwinvoicing.ProgressChanged += Bgwinvoicing_ProgressChanged;
            Bgwinvoicing.RunWorkerCompleted += Bgwinvoicing_RunWorkerCompleted;
            Bgwinvoicing.DoWork += Bgwinvoicing_DoWork;


            // Date Time Outlook
            TxtDateTimeEmail.Text = string.Format(CultureInfo, "{0:g}", Models.Properties.Settings.Default.DateTimeEmail);

            // Timer 1 hour = 3600 seconds (1000 = 1 second)            
            Timer timer = new Timer
            {
                Enabled = true,
                Interval = 60000
            };
            timer.Tick += Timer_Tick;
            Intervalo = timer.Interval;
            TssLblFechaHora.Text = string.Format(CultureInfo, "{0:g}", DateTime.Now);

        }

        

        private void Timer_Tick(object sender, EventArgs e)
        {
            TssLblFechaHora.Text = string.Format(CultureInfo, "{0:g}", DateTime.Now);
            Intervalo += 60000;
            if (Intervalo == 3600000)
            {
                TokenSii = TokenSeed.GETTokenFromSii();
                if (ServiceOutlook != null)
                {
                    ServiceOutlook.TokenSii = TokenSii;
                }
            }

        }
        private void FormMain_Shown(object sender, EventArgs e)
        {
            // frozen column will display row numbers.
            IGridMain.FrozenArea.ColCount = 3;
            //IGridMain.FrozenArea.RowCount = 0;
            IGridMain.FrozenArea.ColsEdge = new iGPenStyle(SystemColors.ControlDark, 2, DashStyle.Solid);
            //IGridMain.FrozenArea.RowsEdge = new iGPenStyle(SystemColors.ControlDark, 2, DashStyle.Solid);

            // Set up the deafult parameters of the frozen columns.
            IGridMain.DefaultCol.AllowMoving = false;
            IGridMain.DefaultCol.IncludeInSelect = false;
            IGridMain.DefaultCol.AllowSizing = false;
            // Set up the width of the first frozen column (hot and current row indicator).
            IGridMain.DefaultCol.Width = 10;
            // Add the first frozen column.
            IGridMain.Cols.Add().CellStyle.CustomDrawFlags = iGCustomDrawFlags.Foreground;
            // Set up the width of the second frozen column (row numbers).
            IGridMain.DefaultCol.Width = 30;
            // Add the second frozen column.
            IGridMain.Cols.Add().CellStyle.CustomDrawFlags = iGCustomDrawFlags.None;

            // Add data columns.
            // Pattern headers cols.
            iGColPattern pattern = new iGColPattern();
            pattern.ColHdrStyle.TextAlign = iGContentAlignment.MiddleCenter;
            pattern.ColHdrStyle.Font = new Font("Calibri", 8.5f, FontStyle.Bold);
            pattern.AllowSizing = false;
            //pattern.AllowMoving = false;
            pattern.AllowGrouping = true;


            // Info cols.
            iGCellStyle cellStyleCommon = new iGCellStyle
            {
                TextAlign = iGContentAlignment.MiddleCenter

            };
            IGridMain.Cols.Add("folio", "F°", 60, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("fechaEmision", "Emission", 60, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("rut", "RUT", 63, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("rznsocial", "Name", 300, pattern).CellStyle = new iGCellStyle() { TextAlign = iGContentAlignment.MiddleCenter, Font = new Font("Microsoft Sans Serif", 8f) };
            IGridMain.Cols.Add("flagxml", "", 20, pattern);
            IGridMain.Cols.Add("inst", "Instructions", 47, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("codProd", "Code", 35, pattern).CellStyle = cellStyleCommon;

            // Info checkboxes
            iGCellStyle cellStyleChk = new iGCellStyle
            {
                ImageAlign = iGContentAlignment.MiddleCenter
            };
            IGridMain.Cols.Add("flagRef", "", 17, pattern);
            IGridMain.Cols.Add("P1", "", 16, pattern).CellStyle = cellStyleChk;
            IGridMain.Cols.Add("P2", "", 16, pattern).CellStyle = cellStyleChk;
            IGridMain.Cols.Add("P3", "", 16, pattern).CellStyle = cellStyleChk;
            IGridMain.Cols.Add("P4", "", 16, pattern).CellStyle = cellStyleChk;

            // Money cols
            iGCellStyle cellStyleMoney = new iGCellStyle
            {
                TextAlign = iGContentAlignment.MiddleCenter,
                FormatString = "{0:#,##}",
                SingleClickEdit = iGBool.True
            };
            IGridMain.Cols.Add("neto", "Net $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["neto"].AllowGrouping = false;
            IGridMain.Cols.Add("exento", "Exent $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["exento"].AllowGrouping = false;
            IGridMain.Cols.Add("iva", "Tax $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["iva"].AllowGrouping = false;
            IGridMain.Cols.Add("total", "Total $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["total"].AllowGrouping = false;

            // Sii info.
            IGridMain.Cols.Add("fechaEnvio", "Sending", 60, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("status", "Status", 56, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("flagstatus", "", 70, pattern); // flag         

            // General options
            IGridMain.GroupBox.Visible = true;
            IGridMain.RowMode = true;
            IGridMain.SelectionMode = iGSelectionMode.One;
            IGridMain.DefaultRow.Height = 20;
            IGridMain.Font = new Font("Microsoft Sans Serif", 7.5f);
            IGridMain.Header.Cells[0, "inst"].SpanCols = 3;
            IGridMain.Header.Cells[0, "P1"].SpanCols = 4;
            IGridMain.ImageList = FListPics;


            // Footer
            IGridMain.Footer.Visible = true;

            // Footer freezer section
            IGridMain.Footer.Cells[0, 0].SpanCols = 3;
            IGridMain.Footer.Cells[0, 3].SpanCols = 7;
            IGridMain.Footer.Cells[0, "neto"].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[0, "exento"].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[0, "iva"].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[0, "total"].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[0, "neto"].TextAlign = iGContentAlignment.MiddleRight;
            IGridMain.Footer.Cells[0, "exento"].TextAlign = iGContentAlignment.MiddleRight;
            IGridMain.Footer.Cells[0, "iva"].TextAlign = iGContentAlignment.MiddleRight;
            IGridMain.Footer.Cells[0, "total"].TextAlign = iGContentAlignment.MiddleRight;

            // Scroll
            IGridMain.VScrollBar.CustomButtons.Add(iGScrollBarCustomButtonAlign.Near, iGActions.GoFirstRow, "Go to first row");
            IGridMain.VScrollBar.CustomButtons.Add(iGScrollBarCustomButtonAlign.Far, iGActions.GoLastRow, "Go to last row");
            IGridMain.VScrollBar.Visibility = iGScrollBarVisibility.OnDemand;
            IGridMain.HScrollBar.Visibility = iGScrollBarVisibility.OnDemand;


        }
   
        private void CboParticipants_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex != 0)
            {
                UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;
                TxtCtaCteParticipant.Text = UserParticipant.BankAccount;
                TxtRutParticipant.Text = UserParticipant.Rut.ToString() + "-" + UserParticipant.VerificationCode; ;
            }
            else
            {
                TxtCtaCteParticipant.Text = "";
                TxtRutParticipant.Text = "";
            }
            TssLblMensaje.Text = "";
        }

        #endregion

        #region Billing Creditor
        private void BtnFacturar_Click(object sender, EventArgs e)
        {
            if (IsRunning)
            {
                return;
            }
            if (!IsCreditor || IGridMain.Rows.Count == 0)
            {
                TssLblMensaje.Text = "Plesase select Creditor!";
                return;
            }
            if (CboParticipants.SelectedIndex == 0)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            // Exist file?
            DateTime now = DateTime.Now;
            string file = $"\\ce_empresas_dwnld_{now.Year}{string.Format("{0:00}", now.Month)}{string.Format("{0:00}", now.Day)}.csv";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + file;
            if (!File.Exists(path))
            {
                MessageBox.Show($"The file 'ce_empresas_dwnld_{now.Year}{now.Month}{now.Day}.csv' NOT found, please download...", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Diagnostics.Process.Start("https://palena.sii.cl/cvc_cgi/dte/ce_consulta_rut");
                return;
            }
            Bgwinvoicing.RunWorkerAsync(path);
        }
        private void Bgwinvoicing_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = e.Argument.ToString();
            int c = 0;
            float porcent = 0;                   
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            porcent = (float)(100 * c) / DetallesCreditor.Count;
            Bgwinvoicing.ReportProgress((int)porcent, "Updating DTE email from Sii file, please wait...");
            Cursor = Cursors.WaitCursor;
            List<AuxCsv> values = File.ReadAllLines(path).Skip(1).Select(v => AuxCsv.GetFronCsv(v)).ToList();
            foreach (Detalle item in DetallesCreditor)
            {
                try
                {                    
                    AuxCsv a = values.FirstOrDefault(x => x.Rut == item.Instruction.ParticipantDebtor.Rut + "-" + item.Instruction.ParticipantDebtor.VerificationCode);
                    if (a != null)
                    {
                        // Update dte & name aux from all instructions 
                        string name = ti.ToTitleCase(a.Name.ToLower());
                        item.Instruction.ParticipantDebtor.BusinessName = name;
                        item.Instruction.ParticipantDebtor.DteReceptionEmail = a.Email;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                c++;
                porcent = (float)(100 * c) / DetallesCreditor.Count;
                // Insert or Update in Softland
                InsertUpdateAuxSoftland(item);
                Bgwinvoicing.ReportProgress((int)porcent, $"Updating in Softland DB, wait please...   ({c}/{DetallesCreditor.Count})");




            }
        }

        private void Bgwinvoicing_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {          
            TssLblProgBar.Value = 0;
            IsCreditor = true;
            IsRunning = false;
        }

        private void Bgwinvoicing_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void InsertUpdateAuxSoftland(Detalle detalle)
        {
            // Comuna

            // Giro

            int resp = Auxiliar.InsertAuxiliar(detalle.Instruction);
            if (resp == -1)
            {
                // escribir log txt si error.
            }
           

           
        }

        #endregion


        #region Convert Pdf

        private void BtnPdfConvert_Click(object sender, EventArgs e)
        {
            if (!IsRunning && IGridMain.Rows.Count > 0)
            {
                IList<Detalle> lista = new List<Detalle>();
                if (IsCreditor)
                {
                    foreach (Detalle item in DetallesCreditor)
                    {
                        if (item.DTEDef != null)
                        {
                            lista.Add(item);
                        }
                    }
                }
                else
                {
                    foreach (Detalle item in DetallesDebtor)
                    {
                        if (item.DTEDef != null)
                        {
                            lista.Add(item);
                        }
                    }
                }

                DialogResult = MessageBox.Show($"Are you sure you convert {lista.Count} documents?", "Centralizador", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (DialogResult == DialogResult.Yes)
                {
                    BackgroundWorker bgwConvertPdf = new BackgroundWorker
                    {
                        WorkerReportsProgress = true
                    };
                    bgwConvertPdf.ProgressChanged += BgwConvertPdf_ProgressChanged;
                    bgwConvertPdf.RunWorkerCompleted += BgwConvertPdf_RunWorkerCompleted;
                    ServicePdf servicePdf = new ServicePdf(lista);
                    servicePdf.ConvertToPdf(bgwConvertPdf);
                }
            }
        }
        private void BgwConvertPdf_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void BgwConvertPdf_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TssLblProgBar.Value = 0;
            TssLblMensaje.Text = "Operation completed.";
            IsRunning = false;
            IGridMain.Focus();
        }

        #endregion

        #region Creditor Transactions

        private void BtnCreditor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            else if (IsRunning)
            {
                return;
            }
            IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrix(new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1));
            if (matrices != null)
            {
                BgwCreditor.RunWorkerAsync(matrices);
            }
            else
            {
                TssLblMensaje.Text = "There are no published instructions.";
            }
        }
        private void BgwCreditor_DoWork(object sender, DoWorkEventArgs e)
        {
            IsRunning = true;
            IList<ResultPaymentMatrix> matrices = (IList<ResultPaymentMatrix>)e.Argument;
            DetallesCreditor = new List<Detalle>();
            List<ResultInstruction> instructions = new List<ResultInstruction>();
            int c = 0;
            float porcent = 0;
            foreach (ResultPaymentMatrix m in matrices)
            {
                ResultBillingWindow window = BillingWindow.GetBillingWindowById(m);
                m.BillingWindow = window;
                // Get instructions with matrix binding.
                IList<ResultInstruction> lista = Instruction.GetInstructionCreditor(m, UserParticipant);
                if (lista != null)
                {
                    instructions.AddRange(lista);
                }
                c++;
                porcent = (float)(100 * c) / matrices.Count;
                BgwCreditor.ReportProgress((int)porcent, $"Getting info from CEN... ({c}/{matrices.Count})");
            }
            c = 0;
            foreach (ResultInstruction instruction in instructions)
            {
                // Tester
                //if (instruction.Id != 1669491) 
                //{
                //    continue;
                //}
                Detalle detalle = new Detalle();
                instruction.ParticipantDebtor = Participant.GetParticipantById(instruction.Debtor);
                //instruction.ParticipantCreditor = UserParticipant;
                detalle.Instruction = instruction;
                detalle.MntNeto = instruction.Amount;
                detalle.RutReceptor = instruction.ParticipantDebtor.Rut;
                detalle.DvReceptor = instruction.ParticipantDebtor.VerificationCode;
                detalle.RznSocRecep = instruction.ParticipantDebtor.BusinessName;

                // Mapping references Softland
                IList<Reference> references = Reference.GetInfoFactura(instruction);
                if (references != null)
                {
                    // Search reference mostly recent for number folio
                    Reference reference = references.OrderByDescending(x => x.Folio).First();
                    // Deserialize 
                    DTEDefType xmlObjeto = null;
                    if (reference.FileBasico != null)
                    {
                        xmlObjeto = ServicePdf.TransformStringDTEDefTypeToObjectDTE(reference.FileBasico);
                    }
                    detalle.Folio = reference.Folio;
                    if (xmlObjeto != null)
                    {
                        detalle.DTEDef = xmlObjeto;
                        DTEDefTypeDocumento dte = (DTEDefTypeDocumento)xmlObjeto.Item;
                        detalle.MntIva = Convert.ToUInt32(dte.Encabezado.Totales.IVA);
                        detalle.MntTotal = Convert.ToUInt32(dte.Encabezado.Totales.MntTotal);

                        // Sii
                        DataEvento evento = ServiceEvento.GetStatusDte("Creditor", TokenSii, "33", detalle, UserParticipant);
                        if (evento != null)
                        {
                            detalle.DataEvento = evento;
                        }

                    }
                    detalle.References = reference;
                    if (reference.FechaRecepcionSii != null)
                    {
                        detalle.FechaRecepcion = reference.FechaRecepcionSii.ToString();
                    }
                    if (reference.FechaEmision != null)
                    {
                        detalle.FechaEmision = reference.FechaEmision.ToString();
                    }
                }
                c++;
                //detalle.Nro = c;
                DetallesCreditor.Add(detalle);
                porcent = (float)(100 * c) / instructions.Count;
                BgwCreditor.ReportProgress((int)porcent, $"Getting info from Softland...   ({c}/{instructions.Count})");
            }
            // Order the list
            DetallesCreditor = DetallesCreditor.OrderBy(x => x.Folio).ToList();
        }
        private void BgwCreditor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void BgwCreditor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CleanControls();
            IGridFill(DetallesCreditor);
            TssLblProgBar.Value = 0;
            IsCreditor = true;
            IsRunning = false;
        }

        #endregion

        #region Debtor Transactions

        private void BtnDebtor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            else if (IsRunning)
            {
                return;
            }
            DetallesDebtor = new List<Detalle>();
            DetallesDebtor = ServiceDetalle.GetLibro("Debtor", UserParticipant, "33", $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 1)}", TokenSii);
            string nameFile = "";
            nameFile += $"{Directory.GetCurrentDirectory()}\\inbox\\{CboYears.SelectedItem}\\{CboMonths.SelectedIndex + 1}";
            if (DetallesDebtor != null)
            {
                BgwDebtor.RunWorkerAsync(nameFile);
            }
        }
        private void BgwDebtor_DoWork(object sender, DoWorkEventArgs e)
        {
            IsRunning = true;
            string nameFilePath = e.Argument.ToString();
            string nameFile = "";
            int c = 0;
            foreach (Detalle item in DetallesDebtor)
            {
                nameFile = nameFilePath + $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__33__{item.Folio}.xml";
                ResultParticipant participant = Participant.GetParticipantByRut(item.RutReceptor.ToString());
                if (participant != null)
                {
                    item.IsParticipant = true;
                    IList<ResultInstruction> instructions = Instruction.GetInstructionByParticipants(participant, UserParticipant);
                    if (instructions != null)
                    {
                        IList<ResultInstruction> i = instructions.Where(x => x.Amount == item.MntNeto).ToList();
                        if (i.Count == 1)
                        {
                            item.IsParticipant = true;
                            item.Instruction = i[0];
                            item.Instruction.PaymentMatrix = PaymentMatrix.GetPaymentMatrixById(i[0]);
                            item.Instruction.PaymentMatrix.BillingWindow = BillingWindow.GetBillingWindowById(item.Instruction.PaymentMatrix);
                        }
                    }
                }
                DTEDefTypeDocumento dte = null;
                DTEDefTypeDocumentoReferencia[] references = null;
                if (File.Exists(nameFile))
                {   // Deserialize  
                    DTEDefType xmlObjeto = ServicePdf.TransformXmlDTEDefTypeToObjectDTE(nameFile);
                    if (xmlObjeto == null || item.Instruction != null)
                    {
                        continue;
                    }
                    item.DTEDef = xmlObjeto;
                    dte = (DTEDefTypeDocumento)xmlObjeto.Item;
                    if (dte != null)
                    {
                        if (dte.Referencia != null)
                        {
                            references = dte.Referencia;
                        }
                    }
                    if (references != null)
                    {
                        DTEDefTypeDocumentoReferencia r = references.FirstOrDefault(x => x.TpoDocRef == "SEN");
                        if (r != null)
                        {
                            string rznRef = "";
                            ResultBillingWindow window = null;
                            if (r.RazonRef != null)
                            {
                                // Controlling lower & upper
                                string r1 = r.RazonRef.Substring(0, r.RazonRef.IndexOf(']') + 1).TrimStart();
                                string r2 = r.RazonRef.Substring(0, r.RazonRef.IndexOf(']', r.RazonRef.IndexOf(']') + 1) + 1);
                                r2 = r2.Substring(r2.IndexOf(']') + 1);
                                string r3 = r.RazonRef.Substring(r1.Length + r2.Length).TrimEnd();
                                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                                rznRef = ti.ToTitleCase(r2.ToLower());
                                window = BillingWindow.GetBillingWindowByNaturalKey(r1 + rznRef);
                                if (window != null)
                                {
                                    // Get the asociated matrix  
                                    IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrixByBillingWindowId(window);
                                    ResultPaymentMatrix matrix = matrices.FirstOrDefault(x => x.NaturalKey == r1 + rznRef + r3);
                                    if (matrix != null)
                                    {
                                        // Get the instruction
                                        ResultInstruction instruction = Instruction.GetInstructionDebtor(matrix, participant, UserParticipant);
                                        if (instruction != null)
                                        {
                                            item.Instruction = instruction;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // Sii
                DataEvento evento = ServiceEvento.GetStatusDte("Debtor", TokenSii, "33", item, UserParticipant);
                if (evento != null)
                {
                    item.DataEvento = evento;
                }
                c++;
                //item.Nro = c;
                float porcent = (float)(100 * c) / DetallesDebtor.Count;
                BgwDebtor.ReportProgress((int)porcent, $"Getting info from Softland...   ({c}/{DetallesDebtor.Count})");
                Thread.Sleep(100);
            }
            // Order the list
            DetallesDebtor = DetallesDebtor.OrderBy(x => x.FechaRecepcion).ToList();
        }
        private void BgwDebtor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void BgwDebtor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CleanControls();
            IGridFill(DetallesDebtor);
            TssLblProgBar.Value = 0;
            IsCreditor = false;
            IsRunning = false;
        }

        #endregion

        #region Common functions

        private void IGridFill(IList<Detalle> detalles)
        {
            try
            {
                IGridMain.BeginUpdate();
                IGridMain.Rows.Clear();
                iGRow myRow;
                int c = 0;
                foreach (Detalle item in detalles)
                {
                    myRow = IGridMain.Rows.Add();
                    c++;
                    item.Nro = c;
                    myRow.Cells[1].Value = item.Nro;
                    if (item.Instruction != null)
                    {
                        myRow.Cells["inst"].Value = item.Instruction.Id;
                        myRow.Cells["codProd"].Value = BillingTypes.FirstOrDefault(x => x.Id == item.Instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                    }
                    myRow.Cells["rut"].Value = item.RutReceptor + "-" + item.DvReceptor;
                    TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                    myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.RznSocRecep.ToLower());
                    myRow.Cells["neto"].Value = item.MntNeto;
                    myRow.Cells["exento"].Value = item.MntExento;
                    myRow.Cells["iva"].Value = item.MntIva;
                    myRow.Cells["total"].Value = item.MntTotal;
                    myRow.Cells["folio"].Value = item.Folio;

                    if (item.FechaEmision != null)
                    {
                        myRow.Cells["fechaEmision"].Value = string.Format(CultureInfo, "{0:d}", Convert.ToDateTime(item.FechaEmision));
                    }

                    if (item.FechaRecepcion != null)
                    {
                        myRow.Cells["fechaEnvio"].Value = string.Format(CultureInfo, "{0:d}", Convert.ToDateTime(item.FechaRecepcion));
                    }

                    if (item.DataEvento != null)
                    {
                        if (item.DataEvento.MayorOchoDias)
                        {
                            myRow.Cells["status"].Value = "Accepted";
                        }
                        if (item.DataEvento.ListEvenHistDoc.Count > 0)
                        {

                            if (item.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "ACD") != null)
                            {
                                myRow.Cells["status"].Value = "Accepted";
                            }
                            else if (item.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "RCD") != null)
                            {
                                myRow.Cells["status"].Value = "Reclaimed";
                            }
                            else if (item.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "PAG") != null)
                            {
                                myRow.Cells["status"].Value = "Accepted";
                            }
                        }
                    }
                    if (item.DTEDef != null)
                    {
                        myRow.Cells["flagxml"].TypeFlags = iGCellTypeFlags.HasEllipsisButton;
                    }
                    if (IsCreditor)
                    {
                        myRow.Cells["P1"].Type = iGCellType.Check;
                        myRow.Cells["P2"].Type = iGCellType.Check;
                    }
                    else
                    {
                        myRow.Cells["P3"].Type = iGCellType.Check;
                        myRow.Cells["P4"].Type = iGCellType.Check;
                    }
                    // Flags
                    LetterFlag myFlag = ValidateCen(item);
                    myRow.Cells["flagRef"].ImageIndex = GetFlagImageIndex(myFlag);
                    myRow.Cells["flagRef"].BackColor = GetFlagBackColor(myFlag);

                }
                IGridMain.EllipsisButtonGlyph = FpicBoxSearch.Image;
                TssLblMensaje.Text = $"{detalles.Count} invoices loaded for {UserParticipant.Name.ToUpper()} company.";
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                //IGridMain.ReadOnly = true;               
                IGridMain.EndUpdate();
                IGridMain.Focus();
            }
        }
        private void CleanControls()
        {
            TssLblMensaje.Text = "";
            TxtNmbItem.Text = "";
            TxtFolioRef.Text = "";
            TxtRznRef.Text = "";
            TxtFmaPago.Text = "";
            TxtDscItem.Text = "";
            TxtTpoDocRef.Text = "";
        }
        public enum LetterFlag
        {
            Red,
            Blue,
            Yellow,
            Green,
            Complete,
            Clear
        }
        private int GetFlagImageIndex(LetterFlag flag)
        {
            switch (flag)
            {
                case LetterFlag.Red:
                    return 11;
                case LetterFlag.Blue:
                    return 12;
                case LetterFlag.Yellow:
                    return 13;
                case LetterFlag.Green:
                    return 14;
                case LetterFlag.Complete:
                    return 15;
                default:
                    return 16;
            }
        }
        private Color GetFlagBackColor(LetterFlag flag)
        {
            switch (flag)
            {
                case LetterFlag.Red:
                    return Color.FromArgb(207, 93, 96);
                case LetterFlag.Blue:
                    return Color.FromArgb(92, 131, 180);
                case LetterFlag.Yellow:
                    return Color.FromArgb(255, 193, 96);
                case LetterFlag.Green:
                    return Color.FromArgb(139, 180, 103);
                case LetterFlag.Complete:
                    return Color.White;
                default:
                    return Color.Empty;
            }
        }
        private LetterFlag ValidateCen(Detalle detalle)
        {
            if (detalle.IsParticipant)
            {
                if (detalle.DTEDef != null && detalle.Instruction != null)
                {
                    DTEDefTypeDocumento dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                    if (dte.Referencia != null)
                    {
                        DTEDefTypeDocumentoReferencia referencia = dte.Referencia.FirstOrDefault(x => x.TpoDocRef == "SEN");
                        if (referencia != null)
                        {
                            return LetterFlag.Red;
                        }
                        if (Convert.ToUInt32(dte.Encabezado.Totales.MntNeto) != detalle.Instruction.Amount)
                        {
                            return LetterFlag.Red;
                        }
                        else if (referencia == null)
                        {
                            return LetterFlag.Red;
                        }
                        else if (referencia.FolioRef != detalle.Instruction.PaymentMatrix.ReferenceCode)
                        {
                            return LetterFlag.Red;
                        }
                        else if (referencia.RazonRef != detalle.Instruction.PaymentMatrix.NaturalKey)
                        {
                            return LetterFlag.Red;
                        }
                        else if (dte.Encabezado.IdDoc.FmaPago != DTEDefTypeDocumentoEncabezadoIdDocFmaPago.Crédito)
                        {
                            return LetterFlag.Red;
                        }
                        return LetterFlag.Green;
                    }
                }
                return LetterFlag.Red;
            }
            return LetterFlag.Clear;
        }
        #endregion

        #region IGridMain methods

        private void IGridMain_CustomDrawCellForeground(object sender, iGCustomDrawCellEventArgs e)
        {
            iGCell fCurCell = IGridMain.CurCell;
            if (e.ColIndex == 0)
            {
                // Draw the hot and current row indicators.
                e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
                int myY = e.Bounds.Y + ((e.Bounds.Height - 7) / 2);
                int myX = e.Bounds.X + ((e.Bounds.Width - 4) / 2);
                Brush myBrush = null;
                if (fCurCell != null && e.RowIndex == fCurCell.RowIndex)
                {
                    myBrush = Brushes.Green;
                }
                if (myBrush != null)
                {
                    e.Graphics.FillRectangle(myBrush, myX, myY, 1, 7);
                    e.Graphics.FillRectangle(myBrush, myX + 1, myY + 1, 1, 5);
                    e.Graphics.FillRectangle(myBrush, myX + 2, myY + 2, 1, 3);
                    e.Graphics.FillRectangle(myBrush, myX + 3, myY + 3, 1, 1);
                }
            }

        }
        private void IGridMain_ColHdrMouseDown(object sender, iGColHdrMouseDownEventArgs e)
        {
            // Prohibit sorting by the hot and current row indicator columns and by the row number column.
            if (e.ColIndex == 0 || e.ColIndex == 6 || e.ColIndex == 7 || e.ColIndex == 10)
            {
                e.DoDefault = false;
            }
        }
        private void IGridMain_CurRowChanged(object sender, EventArgs e)
        {
            //IGridMain.CurRow.Cells["flagRef"].Col.IncludeInSelect = false;  INTENTAR NO PINTAR LA CELDA CON PICS.
            if (IGridMain.CurRow == null)
            {
                return;
            }
            if (!IsRunning && IGridMain.CurRow.Type != iGRowType.AutoGroupRow)
            {
                CleanControls();
                Detalle detalle = null;
                if (IsCreditor)
                {
                    detalle = DetallesCreditor.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                else
                {
                    detalle = DetallesDebtor.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                if (detalle.DTEDef != null)
                {
                    DTEDefTypeDocumento dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                    DTEDefTypeDocumentoDetalle[] detalles = dte.Detalle;
                    TxtFmaPago.Text = dte.Encabezado.IdDoc.FmaPago.ToString();
                    foreach (DTEDefTypeDocumentoDetalle detailProd in detalles)
                    {
                        TxtNmbItem.Text += "+ :" + detailProd.NmbItem.ToLowerInvariant() + Environment.NewLine;
                    }
                    if (dte.Referencia != null)
                    {
                        DTEDefTypeDocumentoReferencia referencia = dte.Referencia.FirstOrDefault(x => x.TpoDocRef == "SEN");
                        if (referencia != null)
                        {
                            TxtFolioRef.Text = referencia.FolioRef;
                            TxtRznRef.Text = referencia.RazonRef;
                            TxtDscItem.Text = dte.Detalle[0].DscItem;
                            TxtTpoDocRef.Text = referencia.TpoDocRef;
                        }
                    }
                }
            }
        }
        private void IGridMain_ColDividerDoubleClick(object sender, iGColDividerDoubleClickEventArgs e)
        {
            TssLblMensaje.Text = IGridMain.Cols[e.ColIndex].Width.ToString();
        }
        private void IGridMain_CellEllipsisButtonClick(object sender, iGEllipsisButtonClickEventArgs e)
        {
            if (!IsRunning)
            {
                Detalle detalle;
                if (IsCreditor)
                {
                    detalle = DetallesCreditor.First(x => x.Nro == e.RowIndex + 1);
                }
                else
                {
                    detalle = DetallesDebtor.First(x => x.Nro == e.RowIndex + 1);
                }
                if (detalle.DTEDef != null)
                {
                    ServicePdf.ConvertToPdf(detalle);
                }
            }
        }
        private void IGridMain_RequestCellToolTipText(object sender, iGRequestCellToolTipTextEventArgs e)
        {
            if (!IsRunning)
            {
                if (e.ColIndex == 19)
                {
                    Detalle detalle = null;
                    if (IsCreditor)
                    {
                        detalle = DetallesCreditor.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
                    }
                    else
                    {
                        detalle = DetallesDebtor.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
                    }
                    if (detalle.DataEvento != null)
                    {
                        if (detalle.DataEvento.ListEvenHistDoc.Count > 0)
                        {
                            foreach (ListEvenHistDoc item in detalle.DataEvento.ListEvenHistDoc)
                            {
                                //DateTime fechaRecepcion = Convert.ToDateTime(item.FechaEvento, CultureInfo);
                                //myRow.Cells["fechaEnvio"].Value = string.Format(CultureInfo, "{0:d}", fechaRecepcion);

                                e.Text = $"Events:{Environment.NewLine}";
                                e.Text += $"{string.Format(CultureInfo, "{0:dd-MM-yyyy}", Convert.ToDateTime(item.FechaEvento))} - {item.CodEvento}: {item.DescEvento}{Environment.NewLine}";
                            }


                        }
                    }
                }
            }
        }
        #endregion

        #region Outlook

        private void BtnOutlook_Click(object sender, EventArgs e)
        {
            ServiceOutlook = new ServiceOutlook
            {
                TokenSii = TokenSii
            };
            BackgroundWorker bgwReadEmail = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            bgwReadEmail.ProgressChanged += BgwReadEmail_ProgressChanged;
            bgwReadEmail.RunWorkerCompleted += BgwReadEmail_RunWorkerCompleted;
            if (!IsRunning)
            {
                IsRunning = true;
                ServiceOutlook.GetXmlFromEmail(bgwReadEmail);
            }
        }
        private void BgwReadEmail_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void BgwReadEmail_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TssLblProgBar.Value = 0;
            BtnOutlook.Enabled = true;
            TxtDateTimeEmail.Text = string.Format(CultureInfo, "{0:g}", e.Result);
            TssLblMensaje.Text = "Complete!";
            IsRunning = false;
            IGridMain.Focus();
        }

        #endregion



    }
}

