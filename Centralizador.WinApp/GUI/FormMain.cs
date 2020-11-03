using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.AppFunctions;
using Centralizador.Models.DataBase;
using Centralizador.Models.Outlook;
using Centralizador.Models.registroreclamodteservice;

using TenTec.Windows.iGridLib;

using static Centralizador.Models.ApiSII.ServiceDetalle;
using static Centralizador.Models.AppFunctions.ValidatorFlag;

namespace Centralizador.WinApp.GUI
{

    public partial class FormMain : Form
    {
        #region Global variables/prop

        //Creditor     
        private IList<Detalle> DetallesCreditor { get; set; }
        private BackgroundWorker BgwCreditor { get; set; }
        private BackgroundWorker BgwInsertNv { get; set; }
        private BackgroundWorker BgwInsertRef { get; set; }


        //Debitor
        private IList<Detalle> DetallesDebtor { get; set; }
        private BackgroundWorker BgwDebtor { get; set; }

        //General
        private string VersionApp { get; set; }
        private ResultParticipant UserParticipant { get; set; }
        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");
        public IEnumerable<ResultBilingType> BillingTypes { get; set; }
        public IList<ResultParticipant> Participants { get; set; }

        // Init
        public string TokenSii { get; set; }
        public string TokenCen { get; set; }



        private bool IsCreditor { get; set; }
        public bool IsRunning { get; set; }
        public ServiceReadMail ServiceOutlook { get; set; }
        public BackgroundWorker BgwConvertPdf { get; private set; }
        public StringBuilder StringLogging { get; set; }
        public string DataBaseName { get; set; }
        public BackgroundWorker BgwReadEmail { get; private set; }
        public BackgroundWorker BgwPay { get; private set; }

        public ServiceSendMail Mail { get; private set; }

        // Button Class
        private readonly IGButtonColumnManager Btn = new IGButtonColumnManager();

        // Watch
        public Stopwatch Watch { get; set; }

        #endregion

        #region FormMain methods

        public FormMain()
        {
            InitializeComponent();
        }
        private void FormMain_Load(object sender, EventArgs e)
        {

            // Load                   
            VersionApp = AssemblyVersion;
            Text = VersionApp;
            Watch = new Stopwatch();


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

            // Worker Debtor
            BgwDebtor = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            BgwDebtor.ProgressChanged += BgwDebtor_ProgressChanged;
            BgwDebtor.RunWorkerCompleted += BgwDebtor_RunWorkerCompleted;
            BgwDebtor.DoWork += BgwDebtor_DoWork;
            // Worker Creditor
            BgwCreditor = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            BgwCreditor.ProgressChanged += BgwCreditor_ProgressChanged;
            BgwCreditor.RunWorkerCompleted += BgwCreditor_RunWorkerCompleted;
            BgwCreditor.DoWork += BgwCreditor_DoWork;
            // Worker Insert Nv
            BgwInsertNv = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            BgwInsertNv.ProgressChanged += BgwInsertNv_ProgressChanged;
            BgwInsertNv.RunWorkerCompleted += BgwInsertNv_RunWorkerCompleted;
            BgwInsertNv.DoWork += BgwInsertNv_DoWork;
            // Worker Insert References
            BgwInsertRef = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            BgwInsertRef.ProgressChanged += BgwInsertRef_ProgressChanged;
            BgwInsertRef.RunWorkerCompleted += BgwInsertRef_RunWorkerCompleted;
            BgwInsertRef.DoWork += BgwInsertRef_DoWork;

            // Worker read email
            BgwReadEmail = new BackgroundWorker
            {
                WorkerReportsProgress = true
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
            BtnOutlook.Text = string.Format(CultureInfo, "{0:g}", ServiceReadMail.GetLastDateTime());

            // Timer Second (every minute)
            System.Timers.Timer timerMinute = new System.Timers.Timer(1000);
            timerMinute.Elapsed += TimerMinute_Elapsed;
            timerMinute.Enabled = true;
            timerMinute.AutoReset = true;

            // Timer Hour (every hour)
            System.Timers.Timer timerHour = new System.Timers.Timer(3600000);
            timerHour.Elapsed += TimerHour_Elapsed;
            timerHour.Enabled = true;
            timerHour.AutoReset = true;

        }
        private void TimerHour_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TokenSii = ServiceSoap.GETTokenFromSii(Properties.Settings.Default.SerialDigitalCert);
            if (ServiceOutlook != null)
            {
                ServiceOutlook.TokenSii = TokenSii;
            }
        }
        private void TimerMinute_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                TssLblFechaHora.Text = string.Format(CultureInfo, "{0:g}", DateTime.Now);
            }
            catch (Exception)
            {

                //throw;
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
            IGridMain.Cols.Add("fechaEnvio", "Sending", 60, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols["fechaEnvio"].AllowGrouping = true;
            IGridMain.Cols.Add("status", "Status", 50, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols["status"].AllowGrouping = true;

            // Button Reject
            iGCol colbtnRejected = IGridMain.Cols.Add("btnRejected", "Reject", 40, pattern);
            colbtnRejected.Tag = IGButtonColumnManager.BUTTON_COLUMN_TAG;
            colbtnRejected.CellStyle = new iGCellStyle();
            Btn.CellButtonClick += Bcm_CellButtonClick;
            Btn.Attach(IGridMain);
            //Btn.CellButtonVisible += Bcm_CellButtonVisible;
            //Btn.CellButtonTooltip += Bcm_CellButtonTooltip;



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
            IGridMain.Footer.Cells[0, 18].SpanCols = 3;
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
        private void CboParticipants_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex != 0)
            {
                UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;
                XmlDocument document = Properties.Settings.Default.DBSoftland;
                foreach (XmlNode item in document.ChildNodes[0])
                {
                    if (item.Attributes["id"].Value == UserParticipant.Id.ToString())
                    {
                        DataBaseName = item.FirstChild.InnerText;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(DataBaseName))
                {
                    MessageBox.Show("This company does not have an associated database in the config file (Xml)", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CboParticipants.SelectedIndex = 0;
                    TxtCtaCteParticipant.Text = "";
                    TxtRutParticipant.Text = "";
                    DataBaseName = "";
                }
                else
                {
                    // Success
                    TxtCtaCteParticipant.Text = UserParticipant.BankAccount;
                    TxtRutParticipant.Text = UserParticipant.Rut.ToString() + "-" + UserParticipant.VerificationCode;
                }
            }
            else
            {
                TxtCtaCteParticipant.Text = "";
                TxtRutParticipant.Text = "";

            }
            TssLblMensaje.Text = "";
        }
        private void BtnHiperLink_Click(object sender, EventArgs e)
        {
            if (IGridMain.CurRow == null || IsRunning)
            {
                return;
            }
            Detalle detalle = null;
            if (IsCreditor)
            {
                detalle = DetallesCreditor.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
            }
            else
            {
                detalle = DetallesDebtor.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
            }
            if (detalle != null && detalle.Instruction != null)
            {
                Process.Start($"https://ppagos-sen.coordinadorelectrico.cl/pagos/instrucciones/{detalle.Instruction.Id}/");
            }

        }
        private void BtnHiperLink_MouseHover(object sender, EventArgs e)
        {
            try
            {
                if (IGridMain.CurRow == null && IsRunning)
                {
                    return;
                }
                Detalle detalle = null;
                if (IsCreditor)
                {
                    detalle = DetallesCreditor.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                else
                {
                    detalle = DetallesDebtor.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                if (detalle != null && detalle.Instruction != null && !IsRunning)
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
        }

        #endregion

        #region Insert NV

        private async void BtnInsertNv_ClickAsync(object sender, EventArgs e)
        {
            if (IsRunning)
            {
                TssLblMensaje.Text = "Bussy!";
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
            string file = $"ce_empresas_dwnld_{now.Year}{string.Format("{0:00}", now.Month)}{string.Format("{0:00}", now.Day)}.csv";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + file;
            if (!File.Exists(path))
            {
                DialogResult resp = MessageBox.Show($"The file '{file}' NOT found, please download...", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
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

            IList<Detalle> detallesFinal = new List<Detalle>();
            foreach (Detalle item in DetallesCreditor)
            {
                if (ChkIncludeReclaimed.CheckState == CheckState.Checked)
                {
                    if (item.DataEvento != null)
                    {
                        if (item.DataEvento.ListEvenHistDoc.Count > 0)
                        {
                            if (item.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "NCA") != null) // // Recepción de NC de anulación que referencia al documento.
                            {
                                detallesFinal.Add(item);
                            }
                        }
                    }
                }
                if (item.Folio == 0 && item.MntNeto > 9) // only up then CLP $10
                {
                    detallesFinal.Add(item);
                }
            }
            if (detallesFinal.Count > 0)
            {
                DialogResult resp = MessageBox.Show($"There are {detallesFinal.Count} pending payment instructions for billing{Environment.NewLine + Environment.NewLine}Are you sure?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resp == DialogResult.Yes)
                {
                    try
                    {
                        // Sql          
                        Conexion con = new Conexion(DataBaseName, Properties.Settings.Default.DBUser, Properties.Settings.Default.DBPassword);
                        int foliosDisp = await NotaVenta.CheckFoliosAsync(con);
                        if (foliosDisp > 0 && foliosDisp < detallesFinal.Count)
                        {
                            MessageBox.Show($"F° Available: {foliosDisp}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    BgwInsertNv.RunWorkerAsync(detallesFinal);
                }
            }
            else
            {
                TssLblMensaje.Text = "All payment instructions are billed, cannot insert NV.";
            }
        }
        private void BgwInsertNv_DoWork(object sender, DoWorkEventArgs e)
        {
            IsRunning = true;
            IList<Detalle> detallesFinal = e.Argument as IList<Detalle>;
            List<AuxCsv> values = new List<AuxCsv>();
            int c = 0;
            float porcent = 0;
            int result = 0;
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            StringLogging.Clear();
            DateTime now = DateTime.Now;
            string file = $"ce_empresas_dwnld_{now.Year}{string.Format("{0:00}", now.Month)}{string.Format("{0:00}", now.Day)}.csv";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + file;

            BgwInsertNv.ReportProgress(0, "Wait please...");
            try
            {
                values = File.ReadAllLines(path).Skip(1).Select(v => AuxCsv.GetFronCsv(v)).ToList();
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error Inserting the data.", ex, MessageBoxIcon.Stop); e.Cancel = true;
            }

            int resultInsertNV;
            int lastF = 0;
            int foliosDisp = 0;
            IList<int> folios = new List<int>();
            IList<Comuna> comunas = new List<Comuna>();
            // Sql          
            Conexion con = new Conexion(DataBaseName, Properties.Settings.Default.DBUser, Properties.Settings.Default.DBPassword);
            try
            {
                foliosDisp = NotaVenta.CheckFoliosAsync(con).Result;
                // Get comunas                      
                comunas = Comuna.GetComunas(con);
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error Inserting the data.", ex, MessageBoxIcon.Stop); e.Cancel = true;
            }

            foreach (Detalle item in detallesFinal)
            {
                if (c == foliosDisp)
                {
                    continue;
                }
                resultInsertNV = 0;
                // Get F° NV if exists
                int F = NotaVenta.GetNvIfExists(item.Instruction, con);
                if (F == 0)
                {
                    try
                    {
                        AuxCsv a = values.FirstOrDefault(x => x.Rut == item.Instruction.ParticipantDebtor.Rut + "-" + item.Instruction.ParticipantDebtor.VerificationCode);
                        if (a != null)
                        {
                            // Info from CSV file
                            string name = ti.ToTitleCase(a.Name.ToLower());
                            item.Instruction.ParticipantDebtor.BusinessName = name;
                            item.Instruction.ParticipantDebtor.DteReceptionEmail = a.Email;
                            item.Instruction.ParticipantDebtor.Name = item.Instruction.ParticipantDebtor.Name.ToUpper();
                        }
                    }
                    catch (Exception)
                    {
                        StringLogging.AppendLine($"{item.Instruction.Id}\tUpdate email\t\tError in CSV file.");
                        continue;
                    }
                    try
                    {
                        Auxiliar aux = new Auxiliar
                        {
                            //RutAux = item.Instruction.ParticipantDebtor.Rut + "-" + item.Instruction.ParticipantDebtor.VerificationCode,
                            //CodAux = item.Instruction.ParticipantDebtor.Rut
                        };
                        Auxiliar auxiliar = aux.GetAuxiliar(item.Instruction, con);
                        if (auxiliar == null)
                        {
                            Comuna comunaobj = null;
                            // Insert New Auxiliar

                            // Get Comuna
                            do
                            {
                                string promptValue = ComunaInput.ShowDialog(Application.ProductName,
                                    $"'Comuna' not found, please input below:",
                                    item.Instruction.ParticipantDebtor.BusinessName,
                                    item.RutReceptor,
                                    item.Instruction.ParticipantDebtor.CommercialAddress,
                                    comunas);

                                comunaobj = comunas.FirstOrDefault(x => aux.RemoveDiacritics(x.ComDes).ToLower() == aux.RemoveDiacritics(promptValue.ToLower()));
                            } while (comunaobj == null);
                            // Sending porperty for show DirAux in Log
                            result = aux.InsertAuxiliar(item.Instruction, con, ref aux, comunaobj);
                            switch (result)
                            {
                                case 1:
                                    break;
                                case 0:
                                    break;
                                case 2:
                                    StringLogging.AppendLine($"{item.Instruction.Id}\tAuxiliar Insert:\tOk: {item.Instruction.ParticipantDebtor.Rut} / {aux.DirAux} / {aux.ComAux}");
                                    // Insert NV
                                    lastF = NotaVenta.GetLastNv(con);
                                    string prod = BillingTypes.FirstOrDefault(x => x.Id == item.Instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                                    resultInsertNV = NotaVenta.InsertNv(item.Instruction, lastF + 1, prod, con);
                                    folios.Add(lastF + 1);
                                    if (resultInsertNV == 0) { StringLogging.AppendLine($"{item.Instruction.Id}\tInsert NV:\tError Sql"); }
                                    break;
                                default:
                                    StringLogging.AppendLine($"{item.Instruction.Id}\tAuxiliar Insert:\tError: {item.Instruction.ParticipantDebtor.Rut}");
                                    break;
                            }
                        }
                        else
                        {
                            // Yes Exists : Update
                            if (auxiliar.ComAux == null || auxiliar.DirAux == null || auxiliar.GirAux == null || auxiliar.RutAux == null)
                            {
                                // Only msg
                                StringLogging.AppendLine($"{item.Instruction.Id}\tAuxiliar Update:\tError, update in Softland: {item.Instruction.ParticipantDebtor.Rut}");
                            }
                            //else
                            //{
                            // Update Aux
                            result = aux.UpdateAuxiliar(item.Instruction, con);
                            // 1= update ok 2= update ok (en algunos escenarios)
                            if (result == 0)
                            {
                                // Error
                                StringLogging.AppendLine($"{item.Instruction.Id}\tAuxiliar Update:\tError Sql: {item.Instruction.ParticipantDebtor.Rut}");
                            }
                            else
                            {
                                // 
                                lastF = NotaVenta.GetLastNv(con);
                                string prod = BillingTypes.FirstOrDefault(x => x.Id == item.Instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                                resultInsertNV = NotaVenta.InsertNv(item.Instruction, lastF + 1, prod, con);
                                if (resultInsertNV == 0)
                                {
                                    StringLogging.AppendLine($"{item.Instruction.Id}\tInsert NV:\tError Sql");
                                }
                            }
                            //}
                        }
                        // Insert NV  
                        if (resultInsertNV == 1)
                        {
                            // Success
                            StringLogging.AppendLine($"{item.Instruction.Id}\tInsert NV:\tF°: {lastF + 1}");
                            folios.Add(lastF + 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        new ErrorMsgCen("There was an error Inserting the data.", ex, MessageBoxIcon.Stop); e.Cancel = true;
                        //return;
                    }
                }
                else
                {
                    folios.Add(F);
                }

                c++;
                porcent = (float)(100 * c) / detallesFinal.Count;
                BgwInsertNv.ReportProgress((int)porcent, $"Inserting NV, wait please...   ({c}/{detallesFinal.Count})");
            }
            e.Result = folios;
        }
        private void BgwInsertNv_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IList<int> folios = e.Result as List<int>;
            if (e.Cancelled == true)
            {
                TssLblMensaje.Text = "Error!";
            }
            else
            {

                TssLblMensaje.Text = "Check the log file.";
            }
            TssLblProgBar.Value = 0;
            IsCreditor = true;
            IsRunning = false;

            if (folios != null && folios.Count > 0)
            {
                int menor = folios.Min();
                int mayor = folios.Max();
                StringLogging.AppendLine($"Resumen: From-{menor} To-{mayor}");
            }
            string nameFile = $"{UserParticipant.Name}_InsertNv_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";

            if (StringLogging.Length > 0)
            {
                string path = @"C:\Centralizador\Log\";
                new CreatePath(path);
                File.WriteAllText(path + nameFile + ".txt", StringLogging.ToString());
                ProcessStartInfo process = new ProcessStartInfo(path + nameFile + ".txt")
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                Process.Start(process);
            }
        }
        private void BgwInsertNv_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }

        #endregion

        #region Insert REF

        private void BtnInsertRef_Click(object sender, EventArgs e)
        {

            if (IsRunning)
            {
                TssLblMensaje.Text = "Bussy!";
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
            IList<Detalle> detallesFinal = new List<Detalle>();
            foreach (Detalle item in DetallesCreditor)
            {
                if (item.Folio > 0 && item.FechaRecepcion == null)
                {
                    detallesFinal.Add(item);
                }
            }
            if (detallesFinal.Count > 0)
            {
                DialogResult resp = MessageBox.Show($"CEN references will be inserted{Environment.NewLine + Environment.NewLine}Are you sure?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resp == DialogResult.Yes)
                {
                    BgwInsertRef.RunWorkerAsync(detallesFinal);
                }
            }
            else
            {
                TssLblMensaje.Text = "No invoices.";
            }
        }
        private void BgwInsertRef_DoWork(object sender, DoWorkEventArgs e)
        {
            IsRunning = true;
            int c = 0;
            IList<Detalle> detallesFinal = e.Argument as IList<Detalle>;
            StringLogging.Clear();
            // Sql
            Conexion con = new Conexion(DataBaseName, Properties.Settings.Default.DBUser, Properties.Settings.Default.DBPassword);

            foreach (Detalle item in detallesFinal)
            {
                if (item.References != null)
                {
                    // Insert References 
                    if (item.References.NroInt > 0)
                    {
                        int result = Reference.InsertReference(item.Instruction, item.References.NroInt, con);
                        if (result == 0)
                        {
                            StringLogging.AppendLine($"{item.Instruction.Id}\tREF Insert:\t\t Error Invoice: {item.References.Folio}");
                        }
                        //else if (result == -1)
                        //{
                        //    // Exists
                        //    StringLogging.AppendLine(item.Instruction.Id + "\t" + "REF Insert:" + "\t\t" + "Invoice F°: *" + item.Folio);
                        //}
                        //else if (result == 2)
                        //{
                        //    // OK
                        //}

                    }
                }
                c++;
                float porcent = (float)(100 * c) / detallesFinal.Count;
                BgwInsertRef.ReportProgress((int)porcent, $"Inserting REF, wait please...   ({c}/{detallesFinal.Count})");
            }
        }
        private void BgwInsertRef_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TssLblProgBar.Value = 0;
            IsCreditor = true;
            IsRunning = false;
            TssLblMensaje.Text = "Complete!.";
            // Only shows msg if Error
            if (StringLogging.Length > 0)
            {
                string nameFile = $"{UserParticipant.Name}_InsertRef_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";
                TssLblMensaje.Text = "Check the log file.";
                string path = @"C:\Centralizador\Log\";
                new CreatePath(path);
                File.WriteAllText(path + nameFile + ".txt", StringLogging.ToString());
                ProcessStartInfo process = new ProcessStartInfo(path + nameFile + ".txt")
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                Process.Start(process);
            }

        }
        private void BgwInsertRef_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }

        #endregion

        #region Convert PDF

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
                    IsRunning = true;
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
                TssLblMensaje.Text = "Canceled!";
            }

            TssLblProgBar.Value = 0;
            TssLblMensaje.Text = "Operation completed.";
            IsRunning = false;
            IGridMain.Focus();
        }

        #endregion

        #region Creditor Transactions

        private async void BtnCreditor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0 && !BgwCreditor.IsBusy)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            else if (IsRunning)
            {
                TssLblMensaje.Text = "Bussy!";
                return;
            }
            try
            {
                IList<ResultPaymentMatrix> matrices = await PaymentMatrix.GetPaymentMatrixAsync(new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1));
                if (matrices != null && matrices.Count > 0)
                {
                    BgwCreditor.RunWorkerAsync(matrices);
                }
                else
                {
                    TssLblMensaje.Text = $"There are no published instructions for:  {CboMonths.SelectedItem}-{CboYears.SelectedItem}.";
                }
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error loading the data.", ex, MessageBoxIcon.Warning); return;
            }


        }
        private void BgwCreditor_DoWork(object sender, DoWorkEventArgs e)
        {
            IsRunning = true;
            IList<ResultPaymentMatrix> matrices = (IList<ResultPaymentMatrix>)e.Argument;
            DetallesCreditor = new List<Detalle>();
            int c = 0;
            float porcent = 0;
            Conexion con = null;
            con = new Conexion(DataBaseName, Properties.Settings.Default.DBUser, Properties.Settings.Default.DBPassword);

            try
            {
                Watch.Restart();
                Watch.Start();
                foreach (ResultPaymentMatrix m in matrices)
                {
                    // Get Window Billing
                    m.BillingWindow = BillingWindow.GetBillingWindowByIdAsync(m).Result;
                    IList<ResultInstruction> lista = Instruction.GetInstructionCreditorAsync(m, UserParticipant).Result;
                    if (lista != null && lista.Count > 0)
                    {
                        int d = 0;
                        foreach (ResultInstruction instruction in lista)
                        {
                            // Tester
                            //if (instruction.Id != 1924576)
                            //{
                            //    continue;
                            //}
                            // Get Participant Debtor
                            instruction.ParticipantDebtor = Participant.GetParticipantByIdAsync(instruction.Debtor).Result;
                            // Root Class
                            Detalle detalle = new Detalle(instruction.ParticipantDebtor.Rut, instruction.ParticipantDebtor.VerificationCode, instruction.ParticipantDebtor.BusinessName, instruction.Amount, instruction, true);
                            // REF from Softland 
                            IList<Reference> references = Reference.GetInfoFactura(instruction, con);
                            detalle.StatusDetalle = StatusDetalle.No;
                            if (references != null)
                            {
                                Reference reference = references.OrderByDescending(x => x.Folio).First();
                                // Compare DateTime
                                int compare = DateTime.Compare(reference.FechaEmision, instruction.PaymentMatrix.PublishDate);
                                if (compare > 0)
                                {
                                    detalle.FechaEmision = reference.FechaEmision.ToString();
                                    detalle.References = reference;
                                    detalle.Folio = reference.Folio;
                                    detalle.MntNeto = reference.NetoAfecto;
                                    detalle.MntIva = reference.Iva;
                                    detalle.MntTotal = reference.Total;

                                    if (reference.FileEnviado != null)
                                    {
                                        // Facturado y enviado al Sii
                                        detalle.FechaRecepcion = reference.FechaRecepcionSii.ToString();
                                        // Attach object dte
                                        detalle.DTEDef = ServicePdf.TransformStringDTEDefTypeToObjectDTE(reference.FileBasico);
                                        // Flags         
                                        detalle.ValidatorFlag = new ValidatorFlag(detalle);
                                        // Events Sii
                                        DataEvento evento = ServiceEvento.GetStatusDteAsync("Creditor", TokenSii, "33", detalle, UserParticipant, Properties.Settings.Default.SerialDigitalCert).Result;
                                        if (evento != null)
                                        {
                                            detalle.DataEvento = evento;
                                        }
                                        // Status
                                        detalle.StatusDetalle = GetStatus(detalle);
                                        // Insert CEN, only Accepted.
                                        if (detalle.StatusDetalle == StatusDetalle.Accepted && detalle.Instruction != null && detalle.Instruction.StatusBilled == Instruction.StatusBilled.NoFacturado)
                                        {
                                            // 1 No Facturado y cuando hay más de 1 dte informado
                                            // 2 Facturado
                                            // 3 Facturado con retraso
                                            // Existe el DTE?
                                            ResultDte doc = Dte.GetDteAsync(detalle, true).Result;
                                            if (doc == null)
                                            {
                                                // Enviar el DTE
                                                ResultDte resultDte = Dte.SendDteCreditorAsync(detalle, TokenCen).Result;
                                                if (resultDte != null)
                                                {
                                                    detalle.Instruction.Dte = resultDte;
                                                }
                                            }
                                            else
                                            {
                                                detalle.Instruction.Dte = doc;
                                            }

                                        }
                                    }
                                    //else
                                    //{
                                    //    // Facturado pero no enviado a Sii o
                                    //    // Facturado pero sin Ref insertadas
                                    //}
                                    // BgwCreditor.ReportProgress((int)porcent, $"Retrieve information of invoices, wait please. ({c}/{instructions.Count})");
                                }
                                else
                                {
                                    // -1 
                                    // Existen F que están aceptadas pero emitidas a diferente RUT, caso Guacolda.
                                    // -1 La fecha de la publicación es mayor que la fecha de la ref encontrada. 
                                    // BgwCreditor.ReportProgress((int)porcent, $"Retrieve information Creditor, wait please. ({c}/{instructions.Count})");
                                }
                            }
                            else
                            {
                                // Instrucciones pendientes por facturar

                            }
                            DetallesCreditor.Add(detalle);
                            d++;
                            porcent = (float)(100 * d) / lista.Count;
                            BgwCreditor.ReportProgress((int)porcent, $"Searching 'Pay Instructions' from CEN, wait please.  ({c}/{matrices.Count}) / ({d}/{lista.Count})");
                            // Cancel Task
                            if (BgwCreditor.CancellationPending) { e.Cancel = true; return; }
                        }
                    }
                    c++;
                    porcent = (float)(100 * c) / matrices.Count;
                    BgwCreditor.ReportProgress((int)porcent, $"Searching 'Pay Instructions' from CEN, wait please. ({c}/{matrices.Count})");
                }
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error loading the data.", ex, MessageBoxIcon.Warning); e.Cancel = true;
            }
        }
        private void BgwCreditor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void BgwCreditor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CleanControls();
            IsCreditor = true;
            IGridFill(DetallesCreditor);
            TssLblProgBar.Value = 0;
            IsRunning = false;
            Watch.Stop();
            TssLblMensaje.Text += "         *[" + Watch.Elapsed.TotalSeconds + " seconds.]";
            // Buttons
            BtnPagar.Enabled = false;
            BtnInsertNv.Enabled = true;
            BtnInsertRef.Enabled = true;
            // Send email                                      
            ServiceSendMail.BatchSendMail();
            IGridMain.BackgroundImage = null;
            if (e.Cancelled == true)
            {
                TssLblMensaje.Text = "Canceled!";
            }
        }

        #endregion

        #region Debtor Transactions

        private async void BtnDebtor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0 && !BgwDebtor.IsBusy)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            else if (IsRunning)
            {
                TssLblMensaje.Text = "Bussy!";
                return;
            }
            try
            {
                // Get info from Sii.
                IList<Detalle> detallesDebtor = await GetLibroAsync("Debtor", UserParticipant, "33", $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 1)}", TokenSii);
                if (detallesDebtor != null && detallesDebtor.Count > 0)
                {
                    DetallesDebtor = detallesDebtor;
                    string nameFile = "";
                    nameFile += @"C:\Centralizador\Inbox\" + CboYears.SelectedItem + @"\" + (CboMonths.SelectedIndex + 1);
                    BgwDebtor.RunWorkerAsync(nameFile);
                }
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error loading the data.", ex, MessageBoxIcon.Warning); return;
            }
        }
        private void BgwDebtor_DoWork(object sender, DoWorkEventArgs e)
        {
            IsRunning = true;
            string nameFilePath = e.Argument.ToString();
            string nameFile = "";
            int c = 0;
            try
            {
                foreach (Detalle item in DetallesDebtor)
                {
                    //if (item.Folio != 6705)
                    //{
                    //    continue;
                    //}
                    DTEDefType xmlObjeto = null;
                    DTEDefTypeDocumento dte = null;
                    DTEDefTypeDocumentoReferencia[] references = null;
                    DTEDefTypeDocumentoReferencia reference = null;
                    ResultParticipant participant = null;

                    // Get XML file in folder
                    nameFile = nameFilePath + $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__33__{item.Folio}.xml";
                    if (File.Exists(nameFile))
                    {
                        xmlObjeto = ServicePdf.TransformXmlDTEDefTypeToObjectDTE(nameFile);
                    }
                    // Get Participant
                    participant = Participant.GetParticipantByRutAsync(item.RutReceptor.ToString()).Result;
                    if (participant != null && participant.Id > 0)
                    {
                        item.IsParticipant = true;
                    }
                    // Exists XML file
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
                                ResultBillingWindow window = BillingWindow.GetBillingWindowByNaturalKey(reference);
                                // Get Matrix
                                if (window != null && window.Id > 0)
                                {
                                    IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrixByBillingWindowIdAsync(window).Result;
                                    if (matrices != null && matrices.Count > 0)
                                    {
                                        ResultPaymentMatrix matrix = matrices.FirstOrDefault(x => x.NaturalKey.Equals(reference.RazonRef.Trim(), StringComparison.OrdinalIgnoreCase));
                                        if (matrix != null)
                                        {
                                            ResultInstruction instruction = Instruction.GetInstructionDebtorAsync(matrix, participant, UserParticipant).Result;
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
                    // Flags 
                    item.ValidatorFlag = new ValidatorFlag(item);

                    // Events Sii  
                    item.DataEvento = ServiceEvento.GetStatusDteAsync("Debtor", TokenSii, "33", item, UserParticipant, Properties.Settings.Default.SerialDigitalCert).Result;
                    // Status
                    if (item.DataEvento != null)
                    {
                        item.StatusDetalle = GetStatus(item);
                    }
                    // Insert CEN, only Accepted.
                    // Debtor DON'T to use the variable 'Instruction.StatusBilled.NoFacturado'
                    if (item.StatusDetalle == StatusDetalle.Accepted && item.Instruction != null && item.Instruction.IsPaid == false)
                    {
                        // 1 No Facturado y cuando hay más de 1 dte informado
                        // 2 Facturado
                        // 3 Facturado con retraso    
                        // Existe el DTE?
                        ResultDte doc = Dte.GetDteAsync(item, false).Result;
                        if (doc == null)
                        {
                            // Enviar el DTE
                            ResultDte resultDte = Dte.SendDteDebtorAsync(item, TokenCen).Result;
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
                    c++;
                    float porcent = (float)(100 * c) / DetallesDebtor.Count;
                    BgwDebtor.ReportProgress((int)porcent, $"Retrieve information Debtor, wait please. ({c}/{DetallesDebtor.Count})");
                    // Cancel Task
                    if (BgwDebtor.CancellationPending) { e.Cancel = true; break; }
                }
                DetallesDebtor = DetallesDebtor.OrderBy(x => x.FechaRecepcion).ToList();
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error loading the data.", ex, MessageBoxIcon.Warning); e.Cancel = true;
            }

        }
        private void BgwDebtor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void BgwDebtor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CleanControls();
            TssLblProgBar.Value = 0;
            IsCreditor = false;
            IsRunning = false;
            IGridFill(DetallesDebtor);
            // Buttons
            BtnPagar.Enabled = true;
            BtnInsertNv.Enabled = false;
            BtnInsertRef.Enabled = false;
            // Send email                                      
            ServiceSendMail.BatchSendMail();
            IGridMain.BackgroundImage = null;
            if (e.Cancelled == true)
            {
                TssLblMensaje.Text = "Canceled!";
            }

        }

        #endregion

        #region Common functions
        private void BtnCancelTak_Click(object sender, EventArgs e)
        {
            if (BgwCreditor != null && !BgwCreditor.CancellationPending && BgwCreditor.IsBusy)
            {
                BgwCreditor.CancelAsync();
            }
            else if (BgwDebtor != null && !BgwDebtor.CancellationPending && BgwDebtor.IsBusy)
            {
                BgwDebtor.CancelAsync();
            }
            else if (BgwConvertPdf != null && !BgwConvertPdf.CancellationPending && BgwConvertPdf.IsBusy)
            {
                BgwConvertPdf.CancelAsync();
            }
        }
        public string AssemblyVersion
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    Version ver = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                    return string.Format("{4} Version: {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision, Application.ProductName);
                }
                else
                {
                    Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                    return string.Format("{4} Version: {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision, Application.ProductName);
                }
            }
        }
        private void IGridFill(IList<Detalle> detalles)
        {
            try
            {
                IGridMain.BeginUpdate();
                IGridMain.Rows.Clear();
                iGRow myRow;
                int c = 0;

                int rejectedNeto = 0;
                int rejectedExento = 0;
                int rejectedIva = 0;
                int rejectedTotal = 0;
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
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
                    if (item.IsParticipant)
                    {
                        myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.RznSocRecep.ToLower());
                        myRow.Cells["rznsocial"].ImageList = fImageListSmall;
                        myRow.Cells["rznsocial"].ImageIndex = 0;
                        myRow.Cells["rznsocial"].ImageAlign = iGContentAlignment.MiddleLeft;
                    }
                    else
                    {
                        myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.RznSocRecep.ToLower());
                    }

                    myRow.Cells["neto"].Value = item.MntNeto;
                    myRow.Cells["exento"].Value = item.MntExento;
                    myRow.Cells["iva"].Value = item.MntIva;
                    myRow.Cells["total"].Value = item.MntTotal;
                    if (item.Folio > 0)
                    {
                        myRow.Cells["folio"].Value = item.Folio;
                    }
                    else
                    {
                        // Waiting for invoice
                        rejectedNeto += item.MntNeto;
                        rejectedExento += item.MntExento;
                        rejectedIva += item.MntIva;
                        rejectedTotal += item.MntTotal;

                    }
                    if (item.FechaEmision != null) { myRow.Cells["fechaEmision"].Value = string.Format(CultureInfo, "{0:d}", Convert.ToDateTime(item.FechaEmision)); }
                    if (item.FechaRecepcion != null) { myRow.Cells["fechaEnvio"].Value = string.Format(CultureInfo, "{0:d}", Convert.ToDateTime(item.FechaRecepcion)); }
                    if (item.DTEDef != null) { myRow.Cells["flagxml"].TypeFlags = iGCellTypeFlags.HasEllipsisButton; }
                    if (IsCreditor)
                    {
                        myRow.Cells["P1"].Type = iGCellType.Check;
                        myRow.Cells["P2"].Type = iGCellType.Check;
                        if (item.Instruction != null && (item.Instruction.StatusBilled == Instruction.StatusBilled.ConRetraso || item.Instruction.StatusBilled == Instruction.StatusBilled.Facturado))
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
                    myRow.Cells["flagRef"].ImageIndex = GetFlagImageIndex(item.ValidatorFlag.Flag);
                    myRow.Cells["flagRef"].BackColor = GetFlagBackColor(item.ValidatorFlag.Flag);
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
                            break;
                        case StatusDetalle.No:
                            // Col Status                         
                            if (item.ValidatorFlag != null && item.ValidatorFlag.Flag != LetterFlag.Green && IsCreditor == false)
                            {
                                myRow.Cells["btnRejected"].ImageIndex = 6;
                                myRow.Cells["btnRejected"].Enabled = iGBool.True;
                            }
                            else
                            {
                                myRow.Cells["btnRejected"].Enabled = iGBool.False;
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
                // Footer Rejected
                IGridMain.Footer.Cells[0, "neto"].Value = rejectedNeto;
                IGridMain.Footer.Cells[0, "exento"].Value = rejectedExento;
                IGridMain.Footer.Cells[0, "iva"].Value = rejectedIva;
                IGridMain.Footer.Cells[0, "total"].Value = rejectedTotal;

                TssLblMensaje.Text = $"{detalles.Count} invoices loaded for {UserParticipant.Name.ToUpper()} company.";

            }
            catch (Exception)
            {
                //throw;
            }
            finally
            {
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
            //TxtDscItem.Text = "";
            TxtTpoDocRef.Text = "";

            // Clean Colors
            TxtFolioRef.BackColor = Color.Empty;
            TxtFmaPago.BackColor = Color.Empty;
            TxtRznRef.BackColor = Color.Empty;
            TxtTpoDocRef.BackColor = Color.Empty;

        }
        private void BtnExcelConvert_Click(object sender, EventArgs e)
        {

            if (!IsRunning && DetallesCreditor != null || DetallesDebtor != null)
            {
                if (IsCreditor && DetallesCreditor.Count > 0)
                {
                    ServiceExcel serviceExcel = new ServiceExcel(UserParticipant);
                    serviceExcel.ExportToExcel(DetallesCreditor, true);
                }
                else if (!IsRunning && !IsCreditor && DetallesDebtor.Count > 0)
                {
                    ServiceExcel serviceExcel = new ServiceExcel(UserParticipant);
                    serviceExcel.ExportToExcel(DetallesDebtor, false);
                }
            }


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
            if (e.ColIndex == 0 || e.ColIndex == 7)
            {
                e.DoDefault = false;
            }
        }
        private void IGridMain_CurRowChanged(object sender, EventArgs e)
        {
            if (!IsRunning && IGridMain.CurRow.Type != iGRowType.AutoGroupRow && IGridMain.CurRow != null)
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
                if (detalle != null && detalle.References != null)
                {
                    if (detalle.References.FileEnviado == null)
                    {
                        TssLblMensaje.Text = "This Invoice has not been sent to Sii.";
                    }
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
            if (!IsRunning)
            {
                Detalle detalle;
                iGRow fCurRow = IGridMain.CurRow;
                if (IsCreditor)
                {
                    detalle = DetallesCreditor.First(x => x.Nro == Convert.ToInt32(fCurRow.Cells[1].Value));
                }
                else
                {
                    detalle = DetallesDebtor.First(x => x.Nro == Convert.ToInt32(fCurRow.Cells[1].Value));
                }
                if (detalle.DTEDef != null)
                {
                    IGridMain.DrawAsFocused = true;
                    ServicePdf.ConvertToPdf(detalle);
                    IGridMain.Focus();
                    IGridMain.DrawAsFocused = false;
                }
            }
        }
        private void IGridMain_RequestCellToolTipText(object sender, iGRequestCellToolTipTextEventArgs e)
        {
            if (!IsRunning && e.ColIndex == 19)
            {
                Detalle detalle = null;
                StringBuilder builder = new StringBuilder();
                if (IsCreditor)
                {
                    detalle = DetallesCreditor.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
                }
                else
                {
                    detalle = DetallesDebtor.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
                }
                if (detalle != null && detalle.DataEvento != null)
                {
                    if (detalle.DataEvento.ListEvenHistDoc.Count > 0)
                    {
                        builder.AppendLine("Events:");
                        foreach (ListEvenHistDoc item in detalle.DataEvento.ListEvenHistDoc)
                        {
                            builder.AppendLine($"{Convert.ToDateTime(item.FechaEvento):dd-MM-yyyy}");
                            builder.AppendLine($" - {item.CodEvento}: {item.DescEvento}");

                        }
                        e.Text = builder.ToString();
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
            if (IsCreditor)
            {
                return;
            }
            else
            {
                TssLblMensaje.Text = null;
                Detalle detalle = null;
                if (IsCreditor)
                {
                    detalle = DetallesCreditor.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                else
                {
                    detalle = DetallesDebtor.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                if (detalle.StatusDetalle == StatusDetalle.No)
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

        #region Outlook

        private void BtnOutlook_Click(object sender, EventArgs e)
        {
            if (!BgwReadEmail.IsBusy)
            {
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
            BtnOutlook.Text = string.Format(CultureInfo, "{0:g}", e.Result);
            TssLblMensaje.Text = "Complete!";
            IGridMain.Focus();


        }

        #endregion

        #region Pagar
        private void BtnPagar_Click(object sender, EventArgs e)
        {
            string monto;
            string msje = null;

            // Excluir Banco Security Rut 97.053.000-2
            if (!BgwPay.IsBusy && !IsCreditor && !IsRunning && IGridMain.Rows.Count > 0)
            {
                IList<Detalle> detallesFinal = new List<Detalle>();
                if (ChkIncludeCEN.CheckState == CheckState.Checked) // Only Participants
                {
                    foreach (Detalle item in DetallesDebtor)
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
                    foreach (Detalle item in DetallesDebtor)
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
                    foreach (Detalle item in DetallesDebtor)
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

        private void BgwPay_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TssLblProgBar.Value = 0;
            if (e.Result != null)
            {
                TssLblMensaje.Text = "Success, check the Excel file.";
                IGridFill(DetallesDebtor);
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

