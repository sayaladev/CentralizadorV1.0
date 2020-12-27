using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

using Centralizador.Models;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.AppFunctions;
using Centralizador.Models.DataBase;
using Centralizador.Models.Outlook;
using Centralizador.Models.registroreclamodteservice;

using TenTec.Windows.iGridLib;


using static Centralizador.Models.ApiSII.ServiceDetalle;
using static Centralizador.Models.AppFunctions.ValidatorFlag;
using static Centralizador.Models.ProgressReportModel;

namespace Centralizador.WinApp.GUI
{

    public partial class FormMain : Form
    {
        //General

        private ResultParticipant UserParticipant { get; set; }
        public IEnumerable<ResultBilingType> BillingTypes { get; set; }
        public List<ResultParticipant> Participants { get; set; }

        #region VARIABLES
        public CancellationTokenSource CancellationTk { get; set; }
        private Progress<ProgressReportModel> ProgressReport { get; set; }
        private List<Detalle> DetallePrincipal { get; set; }
        public ProgressReportModel ReportModel { get; set; }

        #endregion

        // Init
        public string TokenSii { get; set; }
        public string TokenCen { get; set; }

        public ServiceReadMail ServiceOutlook { get; set; }
        public BackgroundWorker BgwConvertPdf { get; private set; }
        public StringBuilder StringLogging { get; set; }
        public string DataBaseName { get; set; }
        public BackgroundWorker BgwReadEmail { get; private set; }
        public BackgroundWorker BgwPay { get; private set; }
        public ServiceSendMail Mail { get; private set; }




        #region FormMain methods

        public FormMain()
        {
            InitializeComponent();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            // VERSION            
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            if (Environment.Is64BitProcess)
            {
                Text = string.Format("{0}    Version: {1}.{2}", Application.ProductName, ver.Major, ver.Minor) + " (64 bits)";
            }
            else
            {
                Text = string.Format("{0}    Version: {1}.{2}", Application.ProductName, ver.Major, ver.Minor) + " (32 bits)";
            }
            // BUTTONS
            BtnCancelTak.Enabled = false;







            //Load ComboBox participants
            CboParticipants.DisplayMember = "Name";
            CboParticipants.DataSource = Participants;
            CboParticipants.SelectedIndex = 0;

            //Load ComboBox months
            CboMonths.DataSource = DateTimeFormatInfo.InvariantInfo.MonthNames.Take(12).ToList();
            CboMonths.SelectedIndex = DateTime.Today.Month - 1;
            //Load ComboBox years
            CboYears.DataSource = Enumerable.Range(2019, 2).ToList();
            CboYears.SelectedItem = DateTime.Today.Year;



            // User email
            TssLblUserEmail.Text = "|  " + Properties.Settings.Default.UserCEN;






            // Worker read email
            BgwReadEmail = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            BgwReadEmail.ProgressChanged += BgwReadEmail_ProgressChanged;
            BgwReadEmail.RunWorkerCompleted += BgwReadEmail_RunWorkerCompleted;

            // Worker Pay
            BgwPay = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            BgwPay.ProgressChanged += BgwPay_ProgressChanged;
            BgwPay.RunWorkerCompleted += BgwPay_RunWorkerCompleted;


            // Logging file
            StringLogging = new StringBuilder();

            // Date Time Outlook
            BtnOutlook.Text = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy HH:mm}", ServiceReadMail.GetLastDateTime());


            // Timer Hour (every hour)
            System.Timers.Timer timerHour = new System.Timers.Timer(3600000);
            timerHour.Elapsed += TimerHour_Elapsed;
            timerHour.Enabled = true;
            timerHour.AutoReset = true;

        }
        private void TimerHour_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TokenSii = ServiceSoap.GETTokenFromSii(Models.Properties.Settings.Default.SerialDigitalCert);
            if (ServiceOutlook != null)
            {
                ServiceOutlook.TokenSii = TokenSii;
            }
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            IGridMain.BeginUpdate();
            // Frozen columns.
            IGridMain.FrozenArea.ColCount = 3;
            IGridMain.FrozenArea.ColsEdge = new iGPenStyle(SystemColors.ControlDark, 2, DashStyle.Solid);
            // Set up the deafult parameters of the frozen columns.
            IGridMain.DefaultCol.AllowMoving = false;
            IGridMain.DefaultCol.IncludeInSelect = false;
            IGridMain.DefaultCol.AllowSizing = false;
            // Set up the width of the first frozen column (hot and current row indicator).
            IGridMain.DefaultCol.Width = 10;
            // Add the first frozen column.
            IGridMain.Cols.Add().CellStyle.CustomDrawFlags = iGCustomDrawFlags.Foreground;
            // Set up the width of the second frozen column (row numbers).
            IGridMain.DefaultCol.Width = 25;
            // Add the second frozen column.
            IGridMain.Cols.Add("N°").CellStyle.CustomDrawFlags = iGCustomDrawFlags.None;

            // Add data columns.

            // Pattern headers cols.
            iGColPattern pattern = new iGColPattern();
            pattern.ColHdrStyle.TextAlign = iGContentAlignment.MiddleCenter;
            pattern.ColHdrStyle.Font = new Font("Calibri", 8.5f, FontStyle.Bold);
            pattern.AllowSizing = false;
            //pattern.AllowMoving = true;
            pattern.AllowGrouping = true;

            // General options
            IGridMain.GroupBox.Visible = true;
            IGridMain.RowMode = true;
            IGridMain.SelectionMode = iGSelectionMode.One;
            IGridMain.DefaultRow.Height = 20;
            IGridMain.Font = new Font("Microsoft Sans Serif", 7.5f);
            IGridMain.ImageList = FListPics;
            IGridMain.EllipsisButtonGlyph = FpicBoxSearch.Image;
            IGridMain.UseXPStyles = false;
            IGridMain.Appearance = iGControlPaintAppearance.StyleFlat;

            // Info cols.
            iGCellStyle cellStyleCommon = new iGCellStyle
            {
                TextAlign = iGContentAlignment.MiddleCenter,
                ImageAlign = iGContentAlignment.MiddleCenter
            };
            //IGridMain.Cols.Add("folio", "F°", 60, pattern).CellStyle = new iGCellStyle() { TextAlign = iGContentAlignment.MiddleLeft };
            IGridMain.Cols.Add("folio", "F°", 60, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("fechaEmision", "Emission", 60, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("rut", "RUT", 63, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("rznsocial", "Name", 300, pattern).CellStyle = new iGCellStyle() { TextAlign = iGContentAlignment.MiddleCenter, Font = new Font("Microsoft Sans Serif", 8f) };

            //Button see xml to pdf

            IGridMain.Cols.Add("flagxml", "", 20, pattern);
            //iGCellStyle cellStyleFlag = new iGCellStyle
            //{
            //    ImageAlign = iGContentAlignment.MiddleCenter
            //};
            //iGCol colflagxml = IGridMain.Cols.Add("flagxml", "", 20, pattern);
            //colflagxml.CellStyle = cellStyleFlag;
            //colflagxml.CellStyle = new iGCellStyle();



            IGridMain.Cols.Add("inst", "Instructions", 47, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("codProd", "Code", 35, pattern).CellStyle = cellStyleCommon;

            // Colour flag
            IGridMain.Cols.Add("flagRef", "", 17, pattern);

            // Info checkboxes
            IGridMain.Cols.Add("P1", "I", 16, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("P2", "II", 16, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("P3", "III", 16, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("P4", "IV", 16, pattern).CellStyle = cellStyleCommon;

            // Money cols
            iGCellStyle cellStyleMoney = new iGCellStyle
            {
                TextAlign = iGContentAlignment.MiddleCenter,
                FormatString = "{0:#,##}"
            };
            IGridMain.Cols.Add("neto", "Net $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["neto"].AllowGrouping = true;
            IGridMain.Cols.Add("exento", "Exent $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["exento"].AllowGrouping = false;
            IGridMain.Cols.Add("iva", "Tax $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["iva"].AllowGrouping = false;
            IGridMain.Cols.Add("total", "Total $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["total"].AllowGrouping = false;

            // Sii info
            IGridMain.Cols.Add("fechaEnvio", "Sending", 80, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols["fechaEnvio"].AllowGrouping = true;
            IGridMain.Cols.Add("status", "Status", 68, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols["status"].AllowGrouping = true;

            // Button Reject
            iGCol colbtnRejected = IGridMain.Cols.Add("btnRejected", "Reject", 40, pattern);
            colbtnRejected.Tag = IGButtonColumnManager.BUTTON_COLUMN_TAG;
            colbtnRejected.CellStyle = new iGCellStyle();
            IGButtonColumnManager Btn = new IGButtonColumnManager();
            Btn.CellButtonClick += Bcm_CellButtonClick;
            Btn.Attach(IGridMain);



            // Header
            IGridMain.Header.Cells[0, "inst"].SpanCols = 3;
            //IGridMain.Header.Cells[0, "P1"].SpanCols = 4;


            // Footer freezer section
            IGridMain.Footer.Visible = true;
            IGridMain.Footer.Rows.Count = 2;
            IGridMain.Footer.Cells[0, 0].SpanCols = 3;
            IGridMain.Footer.Cells[1, 0].SpanCols = 3;
            IGridMain.Footer.Cells[0, 3].SpanCols = 11;
            IGridMain.Footer.Cells[1, 3].SpanCols = 11;
            //IGridMain.Footer.Cells[0, 18].SpanCols = 3;
            IGridMain.Footer.Cells[1, 18].SpanCols = 3;
            IGridMain.Footer.Cells[1, "neto"].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[1, "exento"].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[1, "iva"].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[1, "total"].AggregateFunction = iGAggregateFunction.Sum;
            iGFooterCellStyle style = new iGFooterCellStyle
            {
                TextAlign = iGContentAlignment.MiddleRight,
                Font = new Font("Calibri", 8.5f, FontStyle.Bold)
            };
            iGFooterCell fooUp = IGridMain.Footer.Cells[0, "fechaEmision"];
            fooUp.Style = style;
            fooUp.Value = "Invoices rejected & waiting:";
            iGFooterCell fooDown = IGridMain.Footer.Cells[1, "fechaEmision"];
            fooDown.Value = "Totals:";
            fooDown.Style = style;


            // Scroll          
            IGridMain.VScrollBar.CustomButtons.Add(iGScrollBarCustomButtonAlign.Near, iGActions.GoFirstRow, "Go to first row");
            IGridMain.VScrollBar.CustomButtons.Add(iGScrollBarCustomButtonAlign.Far, iGActions.GoLastRow, "Go to last row");
            IGridMain.VScrollBar.Visibility = iGScrollBarVisibility.Always;
            IGridMain.HScrollBar.Visibility = iGScrollBarVisibility.Hide;



            IGridMain.EndUpdate();
        }
        private void CboParticipants_SelectionChangeCommittedAsync(object sender, EventArgs e)
        {
            TxtCtaCteParticipant.Text = "";
            TxtRutParticipant.Text = "";
            TssLblMensaje.Text = "";
            if (CboParticipants.SelectedIndex != 0)
            {
                UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;
                // READ 'DATABASES' FILE.
                Dictionary<string, string> dic = new Dictionary<string, string>();
                try
                {
                    XDocument doc = XDocument.Load(@"C:\Centralizador\Softland_DB_Names.xml");
                    dic = doc.Descendants("Empresa").ToDictionary(d => (string)d.Attribute("id"), d => (string)d);
                }
                catch (Exception ex)
                {
                    new ErrorMsgCen(@"The file 'C:\Centralizador\Softland_DB_Names.xml' has problems.", ex, MessageBoxIcon.Stop);
                    CboParticipants.SelectedIndex = 0;
                    return;
                }
                if (dic.ContainsKey(UserParticipant.Id.ToString()))
                {
                    DataBaseName = dic[UserParticipant.Id.ToString()];            
                    TxtCtaCteParticipant.Text = UserParticipant.BankAccount;
                    TxtRutParticipant.Text = UserParticipant.Rut.ToString() + "-" + UserParticipant.VerificationCode;
                }
                else
                {
                    MessageBox.Show("This company does not have an associated database in the config file (Xml)", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    TssLblMensaje.Text = @"Please, edit 'C:\Centralizador\Softland_DB_Names.xml' file with ID => " + UserParticipant.Id.ToString();
                    CboParticipants.SelectedIndex = 0;
                }
            }
        }
        private void BtnHiperLink_Click(object sender, EventArgs e)
        {
            if (IGridMain.CurRow == null || ReportModel.IsRuning)
            {
                return;
            }
            Detalle detalle = null;
            if (DetallePrincipal != null)
            {
                detalle = DetallePrincipal.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
            }
            if (detalle != null && detalle.Instruction != null)
            {
                Process.Start($"https://ppagos-sen.coordinador.cl/pagos/instrucciones/{detalle.Instruction.Id}/");
            }

        }
        private void BtnHiperLink_MouseHover(object sender, EventArgs e)
        {
            try
            {
                if (IGridMain.CurRow == null && ReportModel.IsRuning)
                {
                    return;
                }
                Detalle detalle = null;
                if (DetallePrincipal != null)
                {
                    detalle = DetallePrincipal.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                if (detalle != null && detalle.Instruction != null && !ReportModel.IsRuning)
                {
                    ToolTip tip = new ToolTip();
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"Publication date: {Convert.ToDateTime(detalle.Instruction.PaymentMatrix.PublishDate):dd-MM-yyyy}");
                    builder.AppendLine($"Max billing: {Convert.ToDateTime(detalle.Instruction.PaymentMatrix.BillingDate):dd-MM-yyyy}");
                    if (detalle.Instruction.MaxPaymentDate != null)
                    {
                        builder.AppendLine($" / Max payment: {Convert.ToDateTime(detalle.Instruction.MaxPaymentDate):dd-MM-yyyy}");
                    }
                    builder.Append(Environment.NewLine);
                    builder.AppendLine($"Payment matrix: {detalle.Instruction.PaymentMatrix.NaturalKey}");
                    builder.AppendLine($"Reference code: {detalle.Instruction.PaymentMatrix.ReferenceCode}");
                    tip.ToolTipTitle = "Instruction Information";
                    tip.IsBalloon = true;
                    tip.InitialDelay = 100;
                    tip.AutoPopDelay = 20000;
                    tip.SetToolTip(sender as Button, builder.ToString());
                }
            }
            catch (Exception)
            {

            }
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {   // Send email                                      
            ServiceSendMail.BatchSendMail();
            ServiceReadMail.SaveParam();
            if (BgwReadEmail.IsBusy) { BgwReadEmail.CancelAsync(); }
            if (CancellationTk != null && !CancellationTk.IsCancellationRequested) { CancellationTk.Cancel(); }

        }

        #endregion   

        #region Convert PDF

        private void BtnPdfConvert_Click(object sender, EventArgs e)
        {
            if (!ReportModel.IsRuning && IGridMain.Rows.Count > 0)
            {
                List<Detalle> lista = new List<Detalle>();
                if (DetallePrincipal != null)
                {
                    foreach (Detalle item in DetallePrincipal)
                    {
                        if (item.DTEDef != null)
                        {
                            lista.Add(item);
                        }
                    }
                }
                DialogResult = MessageBox.Show($"You are going to convert {lista.Count} documents,{Environment.NewLine}Are you sure?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (DialogResult == DialogResult.Yes)
                {
                    BgwConvertPdf = new BackgroundWorker
                    {
                        WorkerReportsProgress = true,
                        WorkerSupportsCancellation = true
                    };
                    BgwConvertPdf.ProgressChanged += BgwConvertPdf_ProgressChanged;
                    BgwConvertPdf.RunWorkerCompleted += BgwConvertPdf_RunWorkerCompleted;
                    ServicePdf servicePdf = new ServicePdf(lista);
                    ReportModel.IsRuning = true;
                    servicePdf.ConvertToPdf(BgwConvertPdf);
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
            if (e.Cancelled == true)
            {
                TssLblMensaje.Text = $"Canceled! {DataBaseName}";
            }

            TssLblProgBar.Value = 0;
            TssLblMensaje.Text = "Operation completed.";
            ReportModel.IsRuning = false;
            IGridMain.Focus();
        }

        #endregion

        #region INSERTNV

        private async void BtnInsertNv_ClickAsync(object sender, EventArgs e)
        {
            if (ReportModel.IsRuning) { TssLblMensaje.Text = "Bussy!"; return; }
            if (ReportModel != null && ReportModel.DetalleType == TipoDetalle.Debtor || IGridMain.Rows.Count == 0) { TssLblMensaje.Text = "Plesase select Creditor!"; return; }
            if (CboParticipants.SelectedIndex == 0) { TssLblMensaje.Text = "Plesase select a Company!"; return; }    
            List<Detalle> detallesPaso = new List<Detalle>();
            List<Detalle> detallesFinal = new List<Detalle>();

            // DOWNLOAD SII FILE IF NOT EXISTS.
            DialogResult resp;
            while (!new FileSii().ExistsFile)
            {
                resp = MessageBox.Show($"The file '{FileSii.Path}' NOT found, please download...", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (resp == DialogResult.OK)
                {
                    Process.Start("https://palena.sii.cl/cvc_cgi/dte/ce_consulta_rut");
                    return;
                }
                else if (resp == DialogResult.Cancel)
                {
                    return;
                }
            }
            int foliosDisp = await NotaVenta.GetFoliosDisponiblesDTEAsync(new Conexion(DataBaseName));
            // TESTER
            foliosDisp = 100;
            int count = DetallePrincipal.Count;
            StringBuilder builder = new StringBuilder();
            foreach (Detalle item in DetallePrincipal)
            {
                if (ChkIncludeReclaimed.CheckState == CheckState.Checked)
                {
                    if (item.DataEvento != null && item.DataEvento.ListEvenHistDoc.Count > 0 && item.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "NCA") != null)
                    {
                        detallesPaso.Add(item);
                    }
                }
                else
                {
                    if (item.Folio == 0 && item.MntNeto > 9) { detallesPaso.Add(item); } // only > $10
                }
            }
            int c = 0;
            int foliosDispBefore = foliosDisp;
            while (foliosDisp > 0 && detallesPaso.Count > c)
            {
                detallesFinal.Add(detallesPaso[c]);
                foliosDisp--;
                c++;
            }

            if (detallesFinal.Count > 0)
            {
                builder.AppendLine($"There are {foliosDispBefore} F° available, so you can only insert {detallesFinal.Count} of {detallesPaso.Count} NV");
                builder.AppendLine(); builder.AppendLine();
                builder.AppendLine("WARNING: You must run the 'FPL' process NOW or these NV will be deleted from DB.");

                resp = MessageBox.Show(builder.ToString(), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    Stopwatch watch = Stopwatch.StartNew();
                    try
                    {
                        watch.Start();
                        ReportModel = new ProgressReportModel(TipoDetalle.Creditor, TipoTask.InsertNV)
                        {
                            IsRuning = true,
                            StopWatch = watch
                        };
                        ProgressReport = new Progress<ProgressReportModel>();
                        ProgressReport.ProgressChanged += ReportProgress;
                        TssLblMensaje.Text = "Reading SII file, be patient...";
                        FileSii.GetValues(); // GET VALUES LIST FROM CSV.
                        DetalleI detalleI = new DetalleI(DataBaseName, UserParticipant, TokenSii, TokenCen, ReportModel);
                        List<int> folios = await detalleI.InsertNv(detallesFinal, ProgressReport, await BilingType.GetBilinTypesAsync());
                        if (folios != null)
                        { 
                            string nameFile = $"{UserParticipant.Name}_InsertNv_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";
                            if (folios != null && folios.Count > 0)
                            {
                                int menor = folios.Min();
                                int mayor = folios.Max();
                                detalleI.StringLogging.AppendLine("");
                                detalleI.StringLogging.AppendLine($"Summary: From {menor} To-{mayor}");                                
                                new CreateFile(@"C:\Centralizador\Log\", detalleI.StringLogging, nameFile);                                
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        new ErrorMsgCen("There was an error insert into the DB.", ex, MessageBoxIcon.Warning);
                        return;
                    }
                    finally
                    {
                        ReportModel.IsRuning = false;                       
                    }

                }
                else { TssLblMensaje.Text = "Cancel."; }
            }
            else
            {
                if (foliosDisp == 0 && detallesPaso.Count > 0)
                {
                    TssLblMensaje.Text = "F° Available: 0, you need get more in SII.";
                    resp = MessageBox.Show("You will be redirected to the SII site...", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (resp == DialogResult.Yes)
                    {
                        Process.Start("https://palena.sii.cl/cvc_cgi/dte/of_solicita_folios");
                        return;
                    }
                }
                else if (true)
                {
                    TssLblMensaje.Text = "There are already NV associated with these instructions, it cannot be inserted.";
                }
            }
        }
        #endregion

        #region CREDITOR
        private async void BtnCreditor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0) { TssLblMensaje.Text = "Plesase select a Company!"; return; }
            if (ReportModel != null && ReportModel.IsRuning) { TssLblMensaje.Text = "Bussy!"; return; }
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                watch.Start();
                ReportModel = new ProgressReportModel(TipoDetalle.Creditor, TipoTask.GetCreditor)
                {
                    IsRuning = true,
                    StopWatch = watch
                };
                ProgressReport = new Progress<ProgressReportModel>();
                ProgressReport.ProgressChanged += ReportProgress;
                CancellationTk = new CancellationTokenSource();
                List<ResultPaymentMatrix> matrices = await PaymentMatrix.GetPaymentMatrixAsync(new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1));
                if (matrices != null)
                {
                    DetalleI detalleI = new DetalleI(DataBaseName, UserParticipant, TokenSii, TokenCen, ReportModel);
                    detalleI.DeleteNV(); // DELETE NV.
                    DteInfoRef.InsertTriggerRefCen(new Conexion(DataBaseName));  // INSERT TRIGGER.
                    DetallePrincipal = await detalleI.GetDetalleCreditor(matrices, ProgressReport, CancellationTk.Token);
                    if (DetallePrincipal != null) { IGridFill(await BilingType.GetBilinTypesAsync()); }
                }
                else
                {
                    TssLblMensaje.Text = $"There are no published instructions for:  {CboMonths.SelectedItem}-{CboYears.SelectedItem}.";
                }
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error loading the data.", ex, MessageBoxIcon.Warning);
                return;
            }
            finally
            {
                ReportModel.IsRuning = false;
            }
        }

        #endregion

        #region DEBTOR
        private async void BtnDebtor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0) { TssLblMensaje.Text = "Plesase select a Company!"; return; }
            if (ReportModel != null && ReportModel.IsRuning) { TssLblMensaje.Text = "Bussy!"; return; }
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                string nameFile = @"C:\Centralizador\Inbox\" + CboYears.SelectedItem + @"\" + (CboMonths.SelectedIndex + 1);
                watch.Start();
                ReportModel = new ProgressReportModel(TipoDetalle.Debtor, TipoTask.GetDebtor)
                {
                    IsRuning = true,
                    StopWatch = watch
                };
                ProgressReport = new Progress<ProgressReportModel>();
                ProgressReport.ProgressChanged += ReportProgress;
                CancellationTk = new CancellationTokenSource();
                List<Detalle> detalles = await GetLibroAsync("Debtor", UserParticipant, "33", $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 1)}", TokenSii);
                if (detalles != null)
                {
                    DetalleI detalleI = new DetalleI(DataBaseName, UserParticipant, TokenSii, TokenCen, ReportModel);
                    DetallePrincipal = await detalleI.GetDetalleDebtor(detalles, ProgressReport, CancellationTk.Token, nameFile);
                    if (DetallePrincipal != null) { IGridFill(await BilingType.GetBilinTypesAsync()); }
                }
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error loading the data.", ex, MessageBoxIcon.Warning); return;
            }
            finally
            {
                ReportModel.IsRuning = false;
            }
        }

        #endregion

        #region COMMON
        private void ReportProgress(object sender, ProgressReportModel e)
        {
            // Progress Bar           
            TssLblProgBar.Value = e.PercentageComplete;
            TssLblMensaje.Text = e.Message;
            BtnCancelTak.Enabled = true;
            if (e.PercentageComplete == 100)
            {
                e.StopWatch.Stop();
                BtnCancelTak.Enabled = false;
                TssLblProgBar.Value = 0;
                TssLblDBName.Text = "|DB: " + DataBaseName;
             


                // VER ESTO LUEGO
                //    // Send email                                      
                //    ServiceSendMail.BatchSendMail();


                // Para controlar !!!
                switch (e.TaskType)
                {
                    case TipoTask.GetDebtor:
                        BtnPagar.Enabled = true;
                        BtnInsertNv.Enabled = false;
                        TssLblMensaje.Text = $"{DetallePrincipal.Count} invoices loaded for {UserParticipant.Name.ToUpper()} company.   [DEBTOR]";                      
                        break;

                    case TipoTask.GetCreditor:
                        BtnPagar.Enabled = false;
                        BtnInsertNv.Enabled = true;
                        TssLblMensaje.Text = $"{DetallePrincipal.Count} invoices loaded for {UserParticipant.Name.ToUpper()} company.   [CREDITOR]";
                        break;

                    case TipoTask.InsertNV:
                        BtnInsertNv.Enabled = false;
                        TssLblMensaje.Text = $"Check the log file for Execute to FPL.";
                        break;

                    case TipoTask.ConvertToPdf:
                        break;

                    default:
                        break;
                }
                TssLblMensaje.Text += "         *[" + e.StopWatch.Elapsed.TotalSeconds.ToString("0.0000") + " seconds.]";
            }
        }
        private void BtnCancelTak_Click(object sender, EventArgs e)
        {
            try
            {
                CancellationTk?.Cancel();
                CleanControls();
                if (BgwReadEmail.IsBusy) { BgwReadEmail.CancelAsync(); }
                BtnCancelTak.Enabled = false;

                //else if (BgwDebtor != null && !BgwDebtor.CancellationPending && BgwDebtor.IsBusy)
                //{
                //    BgwDebtor.CancelAsync();
                //}
                //else if (BgwConvertPdf != null && !BgwConvertPdf.CancellationPending && BgwConvertPdf.IsBusy)
                //{
                //    BgwConvertPdf.CancelAsync();
                //}

                //  CancellationTk.Dispose();

            }
            catch (Exception)
            {
                throw;
            }
        }
        private void IGridFill(List<ResultBilingType> types)
        {
            try
            {
                IGridMain.BeginUpdate();
                IGridMain.Rows.Clear();
                iGRow myRow;
                int c = 0, rejectedNeto = 0, rejectedExento = 0, rejectedIva = 0, rejectedTotal = 0, rej = 0, rejNc = 0;
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                IGridMain.Footer.Cells[0, "status"].Value = $"{rej} of {rejNc}";

                foreach (Detalle item in DetallePrincipal)
                {
                    myRow = IGridMain.Rows.Add();
                    c++;
                    item.Nro = c;
                    myRow.Cells[1].Value = item.Nro;
                    if (item.Instruction != null)
                    {
                        myRow.Cells["inst"].Value = item.Instruction.Id;
                        myRow.Cells["codProd"].Value = types.FirstOrDefault(x => x.Id == item.Instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                    }
                    else
                    {
                        if (item.IsParticipant && item.DTEDef != null)
                        {
                            myRow.Cells["inst"].Value = "*";
                        }
                    }
                    myRow.Cells["rut"].Value = item.RutReceptor + "-" + item.DvReceptor;
                    myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.RznSocRecep.ToLower());
                    // Icon for Participants
                    if (item.IsParticipant && item.Instruction != null)
                    {
                        //myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.RznSocRecep.ToLower());
                        if (ReportModel.DetalleType == TipoDetalle.Creditor)
                        {
                            myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.Instruction.ParticipantDebtor.Name.ToLower());
                        }
                        else if (ReportModel.DetalleType == TipoDetalle.Debtor)
                        {
                            myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.Instruction.ParticipantCreditor.Name.ToLower());
                        }
                        if (item.Instruction.ParticipantNew != null)
                        {
                            myRow.Cells["rznsocial"].ImageList = fImageListSmall;
                            myRow.Cells["rznsocial"].ImageIndex = 3;
                            myRow.Cells["rznsocial"].ImageAlign = iGContentAlignment.MiddleLeft;
                            myRow.Cells["rznsocial"].Value +=  "   ***  BILLED TO: " + item.Instruction.ParticipantNew.Name.ToLower();
                        }
                        else
                        {
                            myRow.Cells["rznsocial"].ImageList = fImageListSmall;
                            myRow.Cells["rznsocial"].ImageIndex = 0;
                            myRow.Cells["rznsocial"].ImageAlign = iGContentAlignment.MiddleLeft;
                        }
                    
                    }
                    //else
                    //{
                    //    myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.RznSocRecep.ToLower());
                    //}

                    myRow.Cells["neto"].Value = item.MntNeto;
                    myRow.Cells["exento"].Value = item.MntExento;
                    myRow.Cells["iva"].Value = item.MntIva;
                    myRow.Cells["total"].Value = item.MntTotal;
                    if (item.Folio > 0)
                    {
                        myRow.Cells["folio"].Value = item.Folio;
                        if (item.DteInfoRefs != null && item.DteInfoRefs.Count > 1)
                        {
                            myRow.Cells["folio"].ImageList = fImageListSmall;
                            myRow.Cells["folio"].ImageIndex = 4;
                            myRow.Cells["folio"].ImageAlign = iGContentAlignment.TopRight;
                        }
                    }
                    else
                    {
                        // Waiting for invoice
                        rejectedNeto += item.MntNeto;
                        rejectedExento += item.MntExento;
                        rejectedIva += item.MntIva;
                        rejectedTotal += item.MntTotal;

                    }
                    if (item.FechaEmision != null) { myRow.Cells["fechaEmision"].Value = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy}", Convert.ToDateTime(item.FechaEmision)); }
                    if (item.FechaRecepcion != null)
                    {
                        myRow.Cells["fechaEnvio"].Value = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy}", Convert.ToDateTime(item.FechaRecepcion));
                        if (item.DteInfoRefLast != null && item.DteInfoRefLast.EnviadoCliente == 1)
                        {
                            myRow.Cells["fechaEnvio"].ImageList = FListPics;
                            myRow.Cells["fechaEnvio"].ImageIndex = 9;
                            myRow.Cells["fechaEnvio"].ImageAlign = iGContentAlignment.MiddleRight;
                        }
                    }
                    if (item.DteInfoRefLast != null && item.DteInfoRefLast.EnviadoCliente == 1)
                    {
                        myRow.Cells["fechaEnvio"].Value = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy}", Convert.ToDateTime(item.FechaRecepcion)) + "";
                    }
                    if (item.DTEDef != null) { myRow.Cells["flagxml"].TypeFlags = iGCellTypeFlags.HasEllipsisButton; }
                    if (ReportModel != null && ReportModel.DetalleType == TipoDetalle.Creditor)
                    {
                        myRow.Cells["P1"].Type = iGCellType.Check;
                        myRow.Cells["P2"].Type = iGCellType.Check;
                        if (item.Folio > 0 && item.Instruction != null && (item.Instruction.StatusBilled == Instruction.StatusBilled.ConRetraso || item.Instruction.StatusBilled == Instruction.StatusBilled.Facturado))
                        {
                            myRow.Cells["P1"].Value = 1;
                            myRow.Cells["P2"].Value = 1;
                        }
                    }
                    else
                    {
                        if (item.IsParticipant)
                        {
                            myRow.Cells["P3"].Type = iGCellType.Check;
                            myRow.Cells["P4"].Type = iGCellType.Check;
                            if (item.Instruction != null && item.Instruction.Dte != null) // Debtor dont use StatusBilled.Facturado, use dte property.
                            {
                                myRow.Cells["P3"].Value = 1;
                            }
                            if (item.Instruction != null && item.Instruction.IsPaid)
                            {
                                myRow.Cells["P4"].Value = 1;
                            }
                        }
                    }
                    // Flags 
                    if (item.ValidatorFlag != null)
                    {
                        myRow.Cells["flagRef"].ImageIndex = GetFlagImageIndex(item.ValidatorFlag.Flag);
                        myRow.Cells["flagRef"].BackColor = GetFlagBackColor(item.ValidatorFlag.Flag);
                    }

                    // Status      
                    switch (item.StatusDetalle)
                    {
                        case StatusDetalle.Accepted:
                            // Col Status
                            myRow.Cells["status"].Value = item.StatusDetalle;
                            // Col Rejected
                            myRow.Cells["btnRejected"].Enabled = iGBool.False;
                            myRow.Cells["btnRejected"].ImageIndex = 15;
                            break;
                        case StatusDetalle.Rejected:
                            // Col Status
                            myRow.Cells["status"].Value = item.StatusDetalle;
                            myRow.Cells["status"].Style = new iGCellStyle() { ForeColor = Color.Red };

                            // Col Rejected
                            myRow.Cells["btnRejected"].Enabled = iGBool.False;
                            myRow.Cells["btnRejected"].ImageIndex = 5;

                            rejectedNeto += item.MntNeto;
                            rejectedExento += item.MntExento;
                            rejectedIva += item.MntIva;
                            rejectedTotal += item.MntTotal;
                            rejNc++;
                            // Icon
                            if (item.DataEvento != null && item.DataEvento.ListEvenHistDoc.Count > 0 && item.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "NCA") != null)
                            {
                                rej++;
                                myRow.Cells["status"].ImageList = fImageListSmall;
                                myRow.Cells["status"].ImageIndex = 5;
                                myRow.Cells["status"].ImageAlign = iGContentAlignment.MiddleRight;
                            }
                            break;
                        case StatusDetalle.Pending:
                            // Col Status                         
                            if (item.ValidatorFlag != null && item.ValidatorFlag.Flag != LetterFlag.Green && ReportModel != null && ReportModel.DetalleType == TipoDetalle.Debtor && item.Folio > 0) // Debtor
                            {
                                myRow.Cells["btnRejected"].ImageIndex = 6;
                                myRow.Cells["btnRejected"].Enabled = iGBool.True;
                                myRow.Cells["status"].Value = "P";
                            }
                            else
                            {
                                myRow.Cells["btnRejected"].Enabled = iGBool.False;
                                if (item.Folio > 0 && item.FechaRecepcion != null) { myRow.Cells["status"].Value = "P"; }
                            }
                            break;
                        case StatusDetalle.Factoring:
                            myRow.Cells["status"].Value = item.StatusDetalle;
                            myRow.Cells["btnRejected"].Enabled = iGBool.False;
                            myRow.Cells["btnRejected"].ImageIndex = 5;
                            break;
                        default:
                            break;
                    }
                }
                // PICTURE
                IGridMain.BackgroundImage = null;
                // Footer Rejected
                IGridMain.Footer.Cells[0, "neto"].Value = rejectedNeto;
                IGridMain.Footer.Cells[0, "exento"].Value = rejectedExento;
                IGridMain.Footer.Cells[0, "iva"].Value = rejectedIva;
                IGridMain.Footer.Cells[0, "total"].Value = rejectedTotal;
                // Footer Status 
                if (ReportModel != null && ReportModel.DetalleType == TipoDetalle.Creditor && rejNc > 0)
                {
                    IGridMain.Footer.Cells[0, "status"].ImageList = fImageListSmall;
                    IGridMain.Footer.Cells[0, "status"].ImageIndex = 5;
                    IGridMain.Footer.Cells[0, "status"].ImageAlign = iGContentAlignment.MiddleLeft;
                    IGridMain.Footer.Cells[0, "status"].Value = $"{rej} of {rejNc}";
                }
                //TssLblMensaje.Text = $"{DetallePrincipal.Count} invoices loaded for {UserParticipant.Name.ToUpper()} company.";
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error loading the data (IGridFill).", ex, MessageBoxIcon.Warning);
            }
            finally
            {
                IGridMain.EndUpdate();
                IGridMain.Focus();
            }
        }
        private void CleanControls()
        {
            TxtNmbItem.Text = "";
            TxtFolioRef.Text = "";
            TxtRznRef.Text = "";
            TxtFmaPago.Text = "";
            //TxtDscItem.Text = "";
            TxtTpoDocRef.Text = "";


            // Clean Colors
            TxtFolioRef.BackColor = Color.Empty;
            TxtFmaPago.BackColor = Color.Empty;
            TxtRznRef.BackColor = Color.Empty;
            TxtTpoDocRef.BackColor = Color.Empty;

        }
        #endregion

        #region EXCEL CONVERT


        private void BtnExcelConvert_Click(object sender, EventArgs e)
        {

            if (!ReportModel.IsRuning && DetallePrincipal != null && DetallePrincipal.Count > 0)
            {
                ServiceExcel serviceExcel = new ServiceExcel(UserParticipant);
                if (ReportModel.DetalleType == TipoDetalle.Creditor)
                {
                    serviceExcel.ExportToExcel(DetallePrincipal, true, CboMonths.SelectedItem);
                }
                else if (ReportModel.DetalleType == TipoDetalle.Debtor)
                {
                    serviceExcel.ExportToExcel(DetallePrincipal, false, CboMonths.SelectedItem);
                }
            }
        }
        #endregion

        #region IGRID

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
            if (e.ColIndex == 0 || e.ColIndex == 7)
            {
                e.DoDefault = false;
            }
        }
        private void IGridMain_CurRowChanged(object sender, EventArgs e)
        {
            if (!ReportModel.IsRuning && IGridMain.CurRow.Type != iGRowType.AutoGroupRow && IGridMain.CurRow != null)
            {
                CleanControls();
                Detalle detalle = null;
                if (DetallePrincipal != null)
                {
                    detalle = DetallePrincipal.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                if (detalle != null && detalle.DTEDef != null)
                {
                    DTEDefTypeDocumento dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                    DTEDefTypeDocumentoDetalle[] detalles = dte.Detalle;
                    TxtFmaPago.Text = dte.Encabezado.IdDoc.FmaPago.ToString();
                    foreach (DTEDefTypeDocumentoDetalle detailProd in detalles)
                    {
                        TxtNmbItem.Text += "+ :" + detailProd.NmbItem.ToLowerInvariant() + Environment.NewLine;
                        //TxtDscItem.Text = dte.Detalle[0].DscItem;
                    }
                    if (dte.Referencia != null)
                    {
                        DTEDefTypeDocumentoReferencia referencia = dte.Referencia.FirstOrDefault(x => x.TpoDocRef == "SEN");
                        if (referencia != null)
                        {
                            TxtFolioRef.Text = referencia.FolioRef;
                            TxtRznRef.Text = referencia.RazonRef;
                            //TxtDscItem.Text = dte.Detalle[0].DscItem;
                            TxtTpoDocRef.Text = referencia.TpoDocRef;
                        }
                    }
                }
                if (detalle != null && detalle.DteInfoRefLast != null && detalle.FechaRecepcion == null)
                {
                    TssLblMensaje.Text = "This Invoice has not been sent to Sii.";
                }
                //Req: Pintar TextBox indicando el error de la bandera roja 27-10-2020
                if (detalle.ValidatorFlag != null && detalle.IsParticipant && detalle.DTEDef != null)
                {
                    if (detalle.ValidatorFlag.FmaPago && detalle.ValidatorFlag.FolioRef == false && detalle.ValidatorFlag.TpoDocRef == false && detalle.ValidatorFlag.RazonRef == false)
                    {
                        TxtFmaPago.BackColor = GetFlagBackColor(detalle.ValidatorFlag.Flag);
                        TxtFolioRef.BackColor = GetFlagBackColor(LetterFlag.Green);
                        TxtTpoDocRef.BackColor = GetFlagBackColor(LetterFlag.Green);
                        TxtRznRef.BackColor = GetFlagBackColor(LetterFlag.Green);
                    }
                    if (detalle.ValidatorFlag.FmaPago)
                    {
                        TxtFmaPago.BackColor = GetFlagBackColor(detalle.ValidatorFlag.Flag);
                    }
                    else
                    {
                        TxtFmaPago.BackColor = GetFlagBackColor(LetterFlag.Green);
                    }
                    if (detalle.ValidatorFlag.FolioRef)
                    {
                        TxtFolioRef.BackColor = GetFlagBackColor(detalle.ValidatorFlag.Flag);
                    }
                    else
                    {
                        TxtFolioRef.BackColor = GetFlagBackColor(LetterFlag.Green);
                    }
                    if (detalle.ValidatorFlag.TpoDocRef)
                    {
                        TxtTpoDocRef.BackColor = GetFlagBackColor(detalle.ValidatorFlag.Flag);
                    }
                    else
                    {
                        TxtTpoDocRef.BackColor = GetFlagBackColor(LetterFlag.Green);
                    }
                    if (detalle.ValidatorFlag.RazonRef)
                    {
                        TxtRznRef.BackColor = GetFlagBackColor(detalle.ValidatorFlag.Flag);
                    }
                    else
                    {
                        TxtRznRef.BackColor = GetFlagBackColor(LetterFlag.Green);
                    }
                    //if (detalle.ValidatorFlag.TpoDocRef)
                    //{
                    //    TxtTpoDocRef.BackColor = GetFlagBackColor(LetterFlag.Green);
                    //}

                    // TxtTpoDocRef.BackColor = GetFlagBackColor(detalle.ValidatorFlag.Flag);
                }
                //IGridMain.BeginUpdate();
                //iGRow myRow = IGridMain.CurRow;
                //myRow.Cells["flagRef"].ImageIndex = GetFlagImageIndex(detalle.ValidatorFlag.Flag);
                // myRow.Cells["flagRef"].BackColor = GetFlagBackColor(detalle.ValidatorFlag.Flag);
                //IGridMain.EndUpdate();
            }
        }
        private void IGridMain_CellEllipsisButtonClick(object sender, iGEllipsisButtonClickEventArgs e)
        {
            if (!ReportModel.IsRuning)
            {
                Detalle detalle = null;
                iGRow fCurRow = IGridMain.CurRow;
                if (DetallePrincipal != null)
                {
                    detalle = DetallePrincipal.First(x => x.Nro == Convert.ToInt32(fCurRow.Cells[1].Value));
                }
                if (detalle.DTEDef != null)
                {
                    IGridMain.DrawAsFocused = true;
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        ServicePdf.ConvertToPdf(detalle);
                        IGridMain.Focus();
                        IGridMain.DrawAsFocused = false;
                        Cursor.Current = Cursors.Default;
                    }
                    catch (Exception)
                    {
                        TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                        string nomenclatura = detalle.Folio + "_" + ti.ToTitleCase(detalle.RznSocRecep.ToLower() + ".pdf");
                        new ErrorMsgCen($"The process cannot access the file '{nomenclatura}' because it is being used by another process.", MessageBoxIcon.Warning);
                    }

                }
            }
        }
        private void IGridMain_RequestCellToolTipText(object sender, iGRequestCellToolTipTextEventArgs e)
        {
            if (!ReportModel.IsRuning && e.ColIndex == 19) // Sii Events
            {
                Detalle detalle = null;
                StringBuilder builder = new StringBuilder();
                if (DetallePrincipal != null)
                {
                    detalle = DetallePrincipal.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
                }
                if (detalle != null && detalle.DataEvento != null)
                {
                    if (detalle.DataEvento.ListEvenHistDoc.Count > 0)
                    {
                        builder.AppendLine("Events:");
                        foreach (ListEvenHistDoc item in detalle.DataEvento.ListEvenHistDoc)
                        {
                            builder.AppendLine($"{item.FechaEvento:dd-MM-yyyy}");
                            builder.AppendLine($" - {item.CodEvento}: {item.DescEvento}");

                        }
                        e.Text = builder.ToString();
                    }

                }
            }
            else if (!ReportModel.IsRuning && e.ColIndex == 18) // Email Aux send Xml
            {
                Detalle detalle = null;
                if (ReportModel.DetalleType == TipoDetalle.Creditor)
                {
                    StringBuilder builder = new StringBuilder();
                    detalle = DetallePrincipal.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
                    if (detalle.DTEDef != null)
                    {
                        DTEDefTypeDocumento doc = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                        if (detalle != null && !string.IsNullOrEmpty(doc.Encabezado.Receptor.CorreoRecep))
                        {
                            ResultParticipant aux = detalle.Instruction.ParticipantDebtor;
                            builder.AppendLine($"Email Sent: [{doc.Encabezado.Receptor.CorreoRecep}]");
                            //builder.AppendLine($"Email Today: [{aux.DteReceptionEmail}]");
                            e.Text = builder.ToString();
                        }
                    }

                }

            }
            else if (!ReportModel.IsRuning && e.ColIndex == 2) // History DTE
            {
                if (ReportModel.DetalleType == TipoDetalle.Creditor)
                {
                    StringBuilder builder = new StringBuilder();
                    Detalle detalle = null;
                    detalle = DetallePrincipal.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
                    if (detalle != null && detalle.DteInfoRefs != null && detalle.DteInfoRefs.Count > 1)
                    {
                        builder.AppendLine("History:");
                        foreach (DteInfoRef item in detalle.DteInfoRefs.OrderBy(x => x.Folio))
                        {
                            if (item.Folio != detalle.Folio && detalle.Instruction.PaymentMatrix.NaturalKey == item.Glosa && detalle.Instruction.PaymentMatrix.ReferenceCode == item.FolioRef)
                            {
                                if (item.AuxDocNum > 0)
                                {
                                    builder.AppendLine($"F° {item.Folio}-{item.Fecha:dd-MM-yyyy} / [{item.FolioRef}-{item.Glosa}] / NC: {item.AuxDocNum}-{item.AuxDocfec:dd-MM-yyyy}");
                                }
                                else
                                {
                                    builder.AppendLine($"F° {item.Folio}-{item.Fecha:dd-MM-yyyy} / [{item.FolioRef}-{item.Glosa}] / NC:");
                                }
                            }

                        }
                        if (builder.Length > 10)
                        {
                            e.Text = builder.ToString();
                        }

                    }
                }
            }
        }
        private void IGridMain_CustomDrawCellEllipsisButtonForeground(object sender, iGCustomDrawEllipsisButtonEventArgs e)
        {
            if (e.ColIndex == 6)
            {
                // Determine the colors of the background
                Color myColor1, myColor2;
                switch (e.State)
                {
                    case iGControlState.Pressed:
                        myColor1 = SystemColors.ControlDark;
                        myColor2 = SystemColors.ControlLightLight;
                        break;
                    case iGControlState.Hot:
                        myColor1 = SystemColors.ControlLightLight;
                        myColor2 = SystemColors.ControlDark;
                        break;
                    default:
                        myColor1 = SystemColors.ControlLightLight;
                        myColor2 = SystemColors.Control;
                        break;
                }
                //Draw the background
                LinearGradientBrush myBrush = new LinearGradientBrush(e.Bounds, myColor1, myColor2, 45);
                e.Graphics.FillRectangle(myBrush, e.Bounds);
                e.Graphics.DrawRectangle(SystemPens.ControlDark, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                //Notify the grid that the foreground has been drawn, and there is no need to draw it
                //e.DoDefault = false;
            }
        }
        private void Bcm_CellButtonClick(object sender, IGButtonColumnManager.IGCellButtonClickEventArgs e)
        {
            if (ReportModel != null && ReportModel.DetalleType == TipoDetalle.Creditor)
            {
                return;
            }
            else
            {
                TssLblMensaje.Text = null;
                Detalle detalle = null;
                if (DetallePrincipal != null)
                {
                    detalle = DetallePrincipal.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                if (detalle.StatusDetalle == StatusDetalle.Pending)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"Invoice F°: {detalle.Folio}");
                    builder.AppendLine($"Amount $: {detalle.MntNeto:#,##}");
                    builder.AppendLine($"Remaining time to reject: {8 - detalle.DataEvento.DiferenciaFecha} days");
                    builder.Append(Environment.NewLine);
                    builder.Append("Are you sure?");
                    DialogResult result = MessageBox.Show(builder.ToString(), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        if (Mail == null)
                        {
                            Mail = new ServiceSendMail(3, UserParticipant); // 3 Threads

                        }
                        // Reject in Sii
                        // ACD: Acepta Contenido del Documento
                        // RCD: Reclamo al Contenido del Documento
                        // ERM: Otorga Recibo de Mercaderías o Servicios
                        // RFP: Reclamo por Falta Parcial de Mercaderías
                        // RFT: Reclamo por Falta Total de Mercaderías
                        respuestaTo resp = ServiceSoap.SendActionToSii(TokenSii, detalle, "RCD");
                        // Tester
                        //respuestaTo resp = new respuestaTo();
                        resp.codResp = 0;
                        builder.Clear();
                        builder.AppendLine("Results:");
                        builder.AppendLine(Environment.NewLine);
                        if (resp != null)
                        {
                            builder.AppendLine("Rejected in SII: Yes");
                            if (detalle.IsParticipant && detalle.Instruction != null)
                            {

                                switch (resp.codResp)
                                {
                                    case 0: // Acción Completada OK. 
                                        detalle.StatusDetalle = StatusDetalle.Rejected;
                                        // Send email
                                        Mail.SendEmailToParticipant(detalle);
                                        // Reject in CEN                                
                                        ResultDte doc = Dte.SendDteDebtorAsync(detalle, TokenCen).Result;
                                        if (doc != null)
                                        {
                                            detalle.Instruction.Dte = doc;
                                            IGridMain.CurRow.Cells["P3"].Value = 1;
                                            builder.AppendLine("Rejected in CEN: Yes");
                                        }
                                        else
                                        {
                                            builder.AppendLine("Rejected in CEN: No");
                                        }
                                        builder.AppendLine("Email Send: Yes");


                                        break;
                                    case 7: // Evento registrado previamente
                                        break;
                                    case 8: // Pasados 8 días después de la recepción no es posible registrar reclamos o eventos.
                                        break;
                                    default:
                                        break;
                                }
                                //IGridFill(DetallesDebtor); 
                            }
                            else
                            {
                                builder.AppendLine("Email Send: No");
                                builder.AppendLine("Rejected in CEN: No");
                            }
                        }
                        else
                        {
                            builder.AppendLine("Rejected in SII: No");
                            builder.AppendLine("Email Send: No");
                            builder.AppendLine("Rejected in CEN: No");
                        }
                        Thread.Sleep(2000); // 2 segundos
                        MessageBox.Show(builder.ToString(), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        private void ChkIncludeCEN_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.Checked)
            {
                ChkNoIncludeCEN.Checked = false;
            }
        }
        private void ChkNoIncludeCEN_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.Checked)
            {
                ChkIncludeCEN.Checked = false;
            }
        }
        #endregion

        #region OUTLOOK

        private void BtnOutlook_Click(object sender, EventArgs e)
        {
            if (!BgwReadEmail.IsBusy)
            {
                TssLblMensaje.Text = "Connecting to the mail server... Please wait.";
                ServiceOutlook = new ServiceReadMail(TokenSii);
                ServiceOutlook.GetXmlFromEmail(BgwReadEmail);
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
            if (e.Cancelled)
            {
                TssLblMensaje.Text = "Canceled!";
                BtnOutlook.Text = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy HH:mm}", ServiceReadMail.GetLastDateTime());
            }
            else
            {
                BtnOutlook.Text = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy HH:mm}", e.Result);
                TssLblMensaje.Text = "Complete!";
            }

            IGridMain.Focus();


        }

        #endregion

        #region Pagar
        private void BtnPagar_Click(object sender, EventArgs e)
        {
            string monto;
            string msje = null;

            // Excluir Banco Security Rut 97.053.000-2
            if (!BgwPay.IsBusy && ReportModel != null && ReportModel.DetalleType == TipoDetalle.Debtor && !ReportModel.IsRuning && IGridMain.Rows.Count > 0)
            {
                List<Detalle> detallesFinal = new List<Detalle>();
                if (ChkIncludeCEN.CheckState == CheckState.Checked) // Only Participants
                {
                    foreach (Detalle item in DetallePrincipal)
                    {
                        if (item.IsParticipant && item.StatusDetalle == StatusDetalle.Accepted && item.Instruction != null && item.RutReceptor != "97053000")
                        {
                            if (item.Instruction.Dte != null && item.ValidatorFlag != null && item.ValidatorFlag.Flag == LetterFlag.Green) // If exists Dte can Pay
                            {
                                detallesFinal.Add(item);
                            }
                        }
                    }
                    monto = string.Format(CultureInfo.CurrentCulture, "{0:N0}", detallesFinal.Sum(x => x.MntTotal));
                    msje = $"There are {detallesFinal.Count} pending invoices for pay:{Environment.NewLine} ${monto} (Accepted + Green Flags)";
                }
                else if (ChkNoIncludeCEN.CheckState == CheckState.Checked) // Only NO Participants
                {
                    foreach (Detalle item in DetallePrincipal)
                    {
                        if (!item.IsParticipant && item.StatusDetalle == StatusDetalle.Accepted && item.RutReceptor != "97053000")
                        {
                            detallesFinal.Add(item);
                        }
                    }
                    monto = string.Format(CultureInfo.CurrentCulture, "{0:N0}", detallesFinal.Sum(x => x.MntTotal));
                    msje = $"There are {detallesFinal.Count} pending invoices for pay:{Environment.NewLine} ${monto} (Accepted)";
                }
                else if (ChkNoIncludeCEN.CheckState == CheckState.Unchecked && ChkIncludeCEN.CheckState == CheckState.Unchecked)  // All
                {
                    foreach (Detalle item in DetallePrincipal)
                    {
                        if (item.StatusDetalle == StatusDetalle.Accepted && item.RutReceptor != "97053000")
                        {
                            detallesFinal.Add(item);
                        }
                    }
                    monto = string.Format(CultureInfo.CurrentCulture, "{0:N0}", detallesFinal.Sum(x => x.MntTotal));
                    msje = $"There are {detallesFinal.Count} pending invoices for pay:{Environment.NewLine} ${monto} (All)";
                }
                // Total
                if (detallesFinal.Count > 0)
                {
                    monto = string.Format(CultureInfo.CurrentCulture, "{0:N0}", detallesFinal.Sum(x => x.MntTotal));
                    DialogResult resp = MessageBox.Show($"{msje} {Environment.NewLine} Are you sure?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (resp == DialogResult.Yes)
                    {
                        ServiceExcel serviceExcel = new ServiceExcel(detallesFinal, UserParticipant, TokenCen);
                        serviceExcel.CreateNomina(BgwPay);
                    }

                }
                else if (detallesFinal.Count == 0)
                {
                    TssLblMensaje.Text = "Cannot make payments.";
                }
            }
        }

        private async void BgwPay_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TssLblProgBar.Value = 0;
            if (e.Result != null)
            {
                TssLblMensaje.Text = "Success, check the Excel file.";
                IGridFill(await BilingType.GetBilinTypesAsync());
            }
            else
            {
                TssLblMensaje.Text = "No instructions for pay.";
            }

            IGridMain.Focus();
        }

        private void BgwPay_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }








        #endregion


    }
}

