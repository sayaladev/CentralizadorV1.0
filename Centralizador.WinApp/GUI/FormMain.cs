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
using System.Text.RegularExpressions;
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
        public string UserCEN { get; set; }


        private bool IsCreditor { get; set; }
        public bool IsRunning { get; set; }
        public ServiceReadMail ServiceOutlook { get; set; }
        public StringBuilder StringLogging { get; set; }
        public string DataBaseName { get; set; }
        public BackgroundWorker BgwReadEmail { get; private set; }
        public BackgroundWorker BgwPay { get; private set; }

        public ServiceSendMail Mail { get; private set; }

        // Button Class
        private readonly IGButtonColumnManager Btn = new IGButtonColumnManager();


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
            TssLblUserEmail.Text = "|  " + UserCEN;

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
            // Worker Insert Nv
            BgwInsertNv = new BackgroundWorker
            {
                WorkerReportsProgress = true
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
            TssLblFechaHora.Text = string.Format(CultureInfo, "{0:g}", DateTime.Now);
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
            //Button see xml to pdf
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
                FormatString = "{0:#,##}"
            };
            IGridMain.Cols.Add("neto", "Net $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["neto"].AllowGrouping = false;
            IGridMain.Cols.Add("exento", "Exent $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["exento"].AllowGrouping = false;
            IGridMain.Cols.Add("iva", "Tax $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["iva"].AllowGrouping = false;
            IGridMain.Cols.Add("total", "Total $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["total"].AllowGrouping = false;

            // Sii info
            IGridMain.Cols.Add("fechaEnvio", "Sending", 60, pattern).CellStyle = cellStyleCommon;
            IGridMain.Cols.Add("status", "Status", 50, pattern).CellStyle = cellStyleCommon;

            // Button Reject
            iGCol col = IGridMain.Cols.Add("btnRejected", "Reject", 40, pattern);
            col.Tag = IGButtonColumnManager.BUTTON_COLUMN_TAG;
            col.CellStyle = new iGCellStyle();
            Btn.CellButtonClick += Bcm_CellButtonClick;
            Btn.Attach(IGridMain);
            //Btn.CellButtonVisible += Bcm_CellButtonVisible;
            //Btn.CellButtonTooltip += Bcm_CellButtonTooltip;

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

            // Header
            IGridMain.Header.Cells[0, "inst"].SpanCols = 3;
            IGridMain.Header.Cells[0, "P1"].SpanCols = 4;


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
                    MessageBox.Show("This company does not have an associated database in the config file (Xml)", Application.CompanyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (IGridMain.CurRow == null)
            {
                return;
            }
            Detalle detalle = null;
            if (IsCreditor && !IsRunning)
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
                if (IGridMain.CurRow == null)
                {
                    return;
                }
                Detalle detalle = null;
                if (IsCreditor && !IsRunning)
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
                throw;
            }
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {   // Send email                                      
            ServiceSendMail.BatchSendMail();
            ServiceReadMail.SaveParam();
        }

        #endregion

        #region Insert NV

        private void BtnInsertNv_Click(object sender, EventArgs e)
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
                MessageBox.Show($"The file '{file}' NOT found, please download...", Application.CompanyName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Process.Start("https://palena.sii.cl/cvc_cgi/dte/ce_consulta_rut");
                return;
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
                DialogResult resp = MessageBox.Show($"There are {detallesFinal.Count} pending payment instructions for billing{Environment.NewLine + Environment.NewLine}Are you sure?", Application.CompanyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resp == DialogResult.Yes)
                {
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
            int c = 0;
            float porcent = 0;
            int result = 0;
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            StringLogging.Clear();
            DateTime now = DateTime.Now;
            string file = $"ce_empresas_dwnld_{now.Year}{string.Format("{0:00}", now.Month)}{string.Format("{0:00}", now.Day)}.csv";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + file;

            BgwInsertNv.ReportProgress(0, "Wait please...");
            List<AuxCsv> values = File.ReadAllLines(path).Skip(1).Select(v => AuxCsv.GetFronCsv(v)).ToList();

            // Sql          
            Conexion con = new Conexion(DataBaseName, Properties.Settings.Default.DBUser, Properties.Settings.Default.DBPassword);
            int foliosDisp = NotaVenta.CheckFolios(con);
            foreach (Detalle item in detallesFinal)
            {
                if (c == foliosDisp)
                {
                    continue;
                }
                // Tester 
                //if (item.Instruction.Id != 1825458)
                //{
                //    continue;
                //}
                Comuna comuna = null;
                string acteco = null;
                // Get F° NV if exists
                int F = NotaVenta.GetNv(item.Instruction, con);
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
                            //StringLogging.Append(item.Instruction.Id + "\t" + "Update email" + "\t\t" + "Yes" + Environment.NewLine);
                        }
                    }
                    catch (Exception)
                    {
                        StringLogging.AppendLine(item.Instruction.Id + "\t" + "Update email" + "\t\t" + "Error in CSV file.");
                        continue;
                    }

                    // Not exists Auxiliar : Create
                    Auxiliar auxiliar = Auxiliar.GetAuxiliar(item.Instruction, con);
                    if (auxiliar == null)
                    {
                        // Get comunas                      
                        IList<Comuna> comunas = Comuna.GetComunas(con);
                        if (comunas != null)
                        {
                            Regex regex = new Regex(@"\b[\s,\.-:;]*");
                            IEnumerable<string> words = regex.Split(item.Instruction.ParticipantDebtor.CommercialAddress).Where(x => !string.IsNullOrEmpty(x));
                            IList<Comuna> coms = new List<Comuna>();
                            foreach (string w in words)
                            {
                                // Comuna según info de CEN
                                Comuna r = comunas.FirstOrDefault(x => x.ComDes.Contains(w));
                                if (r != null)
                                {
                                    coms.Add(r);
                                }
                            }
                            if (coms.Count == 1)
                            {
                                comuna = coms[0];
                            }
                            else
                            {
                                comuna = coms[1];
                            }
                        }
                        // Get acteco from CEN
                        if (item.Instruction.ParticipantDebtor.CommercialBusiness != null)
                        {
                            if (item.Instruction.ParticipantDebtor.CommercialBusiness.Length > 60)
                            {
                                acteco = item.Instruction.ParticipantDebtor.CommercialBusiness.Substring(0, 60);
                                Acteco.InsertActeco(acteco, con);
                            }
                            else
                            {
                                acteco = item.Instruction.ParticipantDebtor.CommercialBusiness;
                                Acteco.InsertActeco(acteco, con);
                            }


                        }
                        else
                        {
                            StringLogging.AppendLine(item.Instruction.Id + "\t" + "Auxiliar Insert:" + "\t" + "Error API acteco: " + item.Instruction.ParticipantDebtor.Rut);
                        }
                        // Insert aux
                        result = Auxiliar.InsertAuxiliar(item.Instruction, acteco, comuna, con);
                        switch (result)
                        {
                            case 0:
                                break;
                            case 1:
                                break;
                            case 2:
                                StringLogging.AppendLine(item.Instruction.Id + "\t" + "Auxiliar Insert:" + "\t" + item.Instruction.ParticipantDebtor.Rut);
                                break;
                            case -1:
                                break;
                            case 99:
                                StringLogging.AppendLine(item.Instruction.Id + "\t" + "Auxiliar Insert:" + "\t" + "Error: " + item.Instruction.ParticipantDebtor.Rut);
                                break;
                        }
                    }
                    // Yes exists Aux : Update
                    else
                    {
                        if (string.IsNullOrEmpty(auxiliar.ComAux))
                        {
                            StringLogging.AppendLine(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error Softland: ComAux" + item.Instruction.ParticipantDebtor.Rut);
                            continue;
                        }
                        if (string.IsNullOrEmpty(auxiliar.DirAux))
                        {
                            StringLogging.AppendLine(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error Softland: DirAux" + item.Instruction.ParticipantDebtor.Rut);
                            continue;
                        }
                        if (string.IsNullOrEmpty(auxiliar.GirAux))
                        {
                            StringLogging.AppendLine(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error Softland: GirAux" + item.Instruction.ParticipantDebtor.Rut);
                            continue;
                        }
                        if (string.IsNullOrEmpty(auxiliar.RutAux))
                        {
                            StringLogging.AppendLine(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error Softland: RutAux" + item.Instruction.ParticipantDebtor.Rut);
                            continue;
                        }
                        // Update Aux
                        result = Auxiliar.UpdateAuxiliar(item.Instruction, con);
                        switch (result)
                        {
                            case 0:
                                break;
                            case 1:
                                // StringLogging.Append(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Yes: " + item.Instruction.ParticipantDebtor.Rut  + Environment.NewLine);
                                break;
                            case -1:
                                break;
                            case 99:
                                StringLogging.AppendLine(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error Sql: " + item.Instruction.ParticipantDebtor.Rut);
                                break;
                        }
                    }

                    // Insert NV
                    //if (acteco != null && comuna != null)
                    //{
                    int lastF = NotaVenta.GetLastNv(con);
                    string prod = BillingTypes.FirstOrDefault(x => x.Id == item.Instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                    result = NotaVenta.InsertNv(item.Instruction, lastF + 1, prod, con);
                    switch (result)
                    {
                        case 1:
                            StringLogging.AppendLine(item.Instruction.Id + "\t" + "NV Insert:" + "\t\t" + (lastF + 1));
                            break;
                        case 0:
                            break;
                        case -1:
                            // StringLogging.Append(item.Instruction.Id + "\t" + "NV Insert:" + "\t\t" + (lastF + 1) + Environment.NewLine);
                            break;
                        case 99: // Error
                            StringLogging.AppendLine(item.Instruction.Id + "\t" + "NV Insert:" + "\t\t" + "Error" + item.Instruction.ParticipantDebtor.Rut);
                            break;

                    }
                    // }

                }
                else
                {
                    StringLogging.AppendLine(item.Instruction.Id + "\t" + "NV Insert:" + "\t\t" + "*" + F);
                }
                c++;
                porcent = (float)(100 * c) / detallesFinal.Count;
                BgwInsertNv.ReportProgress((int)porcent, $"Inserting NV, wait please...   ({c}/{detallesFinal.Count})");
            }

        }
        private void BgwInsertNv_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TssLblProgBar.Value = 0;
            IsCreditor = true;
            IsRunning = false;
            TssLblMensaje.Text = "Check the log file.";
            string nameFile = $"{UserParticipant.Name}_InsertNv_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";

            if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\log\"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\log\");
            }
            File.WriteAllText(Directory.GetCurrentDirectory() + @"\log\" + nameFile + ".txt", StringLogging.ToString());
            ProcessStartInfo process = new ProcessStartInfo(Directory.GetCurrentDirectory() + @"\log\" + nameFile + ".txt")
            {
                WindowStyle = ProcessWindowStyle.Minimized
            };
            Process.Start(process);
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
                DialogResult resp = MessageBox.Show($"CEN references will be inserted{Environment.NewLine + Environment.NewLine}Are you sure?", Application.CompanyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
            float porcent = 0;
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
                        switch (result)
                        {
                            case 2: // Success
                                StringLogging.AppendLine(item.Instruction.Id + "\t" + "REF Insert:" + "\t\t" + "Invoice F°: " + item.Folio);
                                break;
                            case 0:
                                break;
                            case -1: // ya existe
                                StringLogging.AppendLine(item.Instruction.Id + "\t" + "REF Insert:" + "\t\t" + "Invoice F°: *" + item.Folio);
                                break;
                            case 99: // Error
                                StringLogging.AppendLine(item.Instruction.Id + "\t" + "REF Insert:" + "\t\t" + "Error");
                                break;
                        }
                    }
                }
                c++;
                porcent = (float)(100 * c) / detallesFinal.Count;
                BgwInsertRef.ReportProgress((int)porcent, $"Inserting REF, wait please...   ({c}/{detallesFinal.Count})");
            }


        }
        private void BgwInsertRef_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TssLblProgBar.Value = 0;
            IsCreditor = true;
            IsRunning = false;
            TssLblMensaje.Text = "Check the log file.";
            string nameFile = $"{UserParticipant.Name}_InsertRef_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";
            if (StringLogging.Length > 0)
            {
                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\log\"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\log\");
                }
                // Order builder

                File.WriteAllText(Directory.GetCurrentDirectory() + @"\log\" + nameFile + ".txt", StringLogging.ToString());
                ProcessStartInfo process = new ProcessStartInfo(Directory.GetCurrentDirectory() + @"\log\" + nameFile + ".txt")
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

                DialogResult = MessageBox.Show($"You are going to convert {lista.Count} documents,{Environment.NewLine}Are you sure?", Application.CompanyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (DialogResult == DialogResult.Yes)
                {
                    BackgroundWorker bgwConvertPdf = new BackgroundWorker
                    {
                        WorkerReportsProgress = true
                    };
                    bgwConvertPdf.ProgressChanged += BgwConvertPdf_ProgressChanged;
                    bgwConvertPdf.RunWorkerCompleted += BgwConvertPdf_RunWorkerCompleted;
                    ServicePdf servicePdf = new ServicePdf(lista);
                    IsRunning = true;
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
            IList<ResultPaymentMatrix> matrices = await PaymentMatrix.GetPaymentMatrixAsync(new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1));
            if (matrices != null)
            {
                BgwCreditor.RunWorkerAsync(matrices);
            }
            else
            {
                TssLblMensaje.Text = $"There are no published instructions for:  {CboMonths.SelectedItem}-{CboYears.SelectedItem}.";
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
            foreach (ResultPaymentMatrix m in matrices)
            {

                m.BillingWindow = BillingWindow.GetBillingWindowByIdAsync(m).Result; ;
                IList<ResultInstruction> lista = Instruction.GetInstructionCreditorAsync(m, UserParticipant).Result;
                if (lista == null)
                {
                    // Error Exception
                    MessageBox.Show("There was an error connecting to the CEN API.", Application.CompanyName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    if (lista.Count > 0)
                    {
                        int d = 0;
                        foreach (ResultInstruction instruction in lista)
                        {
                            //if (d == 3)
                            //{
                            //    continue;
                            //}
                            instruction.ParticipantDebtor = Participant.GetParticipantByIdAsync(instruction.Debtor).Result;
                            Detalle detalle = new Detalle(instruction.ParticipantDebtor.Rut, instruction.ParticipantDebtor.VerificationCode, instruction.ParticipantDebtor.BusinessName, instruction.Amount, instruction, true);
                            // REF from Softland 
                            IList<Reference> references = Reference.GetInfoFactura(instruction, con);
                            detalle.StatusDetalle = StatusDetalle.No;
                            if (references == null)
                            {
                                // Error Exception
                                MessageBox.Show("There was an error connecting to the server Softland.", Application.CompanyName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else
                            {
                                if (references.Count > 0)
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
                                            detalle.Flag = ValidateCen(detalle);
                                            // Events Sii                       
                                            detalle.DataEvento = ServiceEvento.GetStatusDteAsync("Creditor", TokenSii, "33", detalle, UserParticipant, Properties.Settings.Default.SerialDigitalCert).Result;
                                            // Status
                                            detalle.StatusDetalle = GetStatus(detalle);
                                            // Insert CEN, only Accepted.
                                            if (detalle.StatusDetalle == StatusDetalle.Accepted && detalle.Instruction != null && detalle.Instruction.StatusBilled == Instruction.StatusBilled.NoFacturado)
                                            {
                                                // 1 No Facturado y cuando hay más de 1 dte informado
                                                // 2 Facturado
                                                // 3 Facturado con retraso
                                                detalle.Instruction.Dte = Dte.SendDteCreditorAsync(detalle, TokenCen).Result;
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
                            }
                            DetallesCreditor.Add(detalle);
                            d++;
                            porcent = (float)(100 * d) / lista.Count;
                            BgwCreditor.ReportProgress((int)porcent, $"Searching 'Pay Instructions' from CEN, wait please.  ({c}/{matrices.Count}) / ({d}/{lista.Count})");
                        }
                    }
                }
                c++;
                porcent = (float)(100 * c) / matrices.Count;
                BgwCreditor.ReportProgress((int)porcent, $"Searching 'Pay Instructions' from CEN, wait please. ({c}/{matrices.Count})");
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
            // Buttons
            BtnPagar.Enabled = false;
            BtnInsertNv.Enabled = true;
            BtnInsertRef.Enabled = true;
            // Send email                                      
            ServiceSendMail.BatchSendMail();
            IGridMain.BackgroundImage = null;
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
            DetallesDebtor = new List<Detalle>();
            DetallesDebtor = await GetLibroAsync("Debtor", UserParticipant, "33", $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 1)}", TokenSii);
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
                //   Tester
                //if (item.Folio != 1559)
                //{
                //    continue;
                //}
                DTEDefType xmlObjeto = null;
                DTEDefTypeDocumento dte = null;
                DTEDefTypeDocumentoReferencia[] references = null;
                DTEDefTypeDocumentoReferencia reference = null;
                ResultBillingWindow window = null;

                // Get XML file in folder
                nameFile = nameFilePath + $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__33__{item.Folio}.xml";
                if (File.Exists(nameFile))
                {
                    xmlObjeto = ServicePdf.TransformXmlDTEDefTypeToObjectDTE(nameFile);
                    item.DTEDef = xmlObjeto;
                }
                // Get Participant
                ResultParticipant participant = Participant.GetParticipantByRutAsync(item.RutReceptor.ToString()).Result;
                if (participant != null && participant.Rut != "96800570") // Exclude to: Enel Distribución Chile S.A.
                {
                    item.IsParticipant = true;
                    IList<ResultInstruction> instructions = Instruction.GetInstructionByParticipantsAsync(participant, UserParticipant).Result;
                    if (instructions != null)
                    {
                        // Find Instruction by amount
                        IList<ResultInstruction> i = instructions.Where(x => x.Amount == item.MntNeto).ToList();
                        if (i.Count == 1)
                        {
                            item.Instruction = i[0];
                            item.Instruction.PaymentMatrix = PaymentMatrix.GetPaymentMatrixByIdAsync(i[0]).Result;
                            item.Instruction.PaymentMatrix.BillingWindow = BillingWindow.GetBillingWindowByIdAsync(item.Instruction.PaymentMatrix).Result;
                        }
                        else
                        {
                            if (xmlObjeto != null)
                            {
                                dte = (DTEDefTypeDocumento)xmlObjeto.Item;
                                references = dte.Referencia;
                                if (references != null)
                                {
                                    reference = references.FirstOrDefault(x => x.TpoDocRef == "SEN");
                                    if (reference != null)
                                    {
                                        window = BillingWindow.GetBillingWindowByNaturalKey(reference);
                                        if (window != null)
                                        {
                                            IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrixByBillingWindowIdAsync(window).Result;
                                            ResultPaymentMatrix matrix = matrices.FirstOrDefault(x => x.NaturalKey == reference.RazonRef.TrimEnd());
                                            if (matrix != null)
                                            {
                                                item.Instruction = Instruction.GetInstructionDebtorAsync(matrix, participant, UserParticipant).Result;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (item.Instruction != null)
                        {
                            item.Instruction.ParticipantCreditor = participant;
                            item.Instruction.ParticipantDebtor = UserParticipant;
                        }

                    }
                }

                // Flags       
                item.Flag = ValidateCen(item);
                // Events Sii
                item.DataEvento = ServiceEvento.GetStatusDteAsync("Debtor", TokenSii, "33", item, UserParticipant, Properties.Settings.Default.SerialDigitalCert).Result;
                // Status
                if (item.DataEvento != null)
                {
                    item.StatusDetalle = GetStatus(item);
                }
                // Insert CEN
                if (item.StatusDetalle != StatusDetalle.No && item.Instruction != null)
                {
                    ResultDte doc = Dte.GetDteByFolioAsync(item, false).Result;
                    if (doc == null)
                    {
                        doc = Dte.SendDteDebtorAsync(item, TokenCen).Result;
                    }
                    item.Instruction.Dte = doc;
                }
                c++;
                float porcent = (float)(100 * c) / DetallesDebtor.Count;
                BgwDebtor.ReportProgress((int)porcent, $"Retrieve information Debtor, wait please. ({c}/{DetallesDebtor.Count})");
            }
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
        }

        #endregion

        #region Common functions
        public string AssemblyVersion
        {
            get
            {
                if (ApplicationDeployment.IsNetworkDeployed)
                {
                    Version ver = ApplicationDeployment.CurrentDeployment.CurrentVersion;
                    return string.Format("{4} Version: {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision, Assembly.GetEntryAssembly().GetName().Name);
                }
                else
                {
                    Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                    return string.Format("{4} Version: {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision, Assembly.GetEntryAssembly().GetName().Name);
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
                    myRow.Cells["rut"].Value = item.RutReceptor + "-" + item.DvReceptor;
                    myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.RznSocRecep.ToLower());
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
                            if (item.Instruction != null)
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
                    myRow.Cells["flagRef"].ImageIndex = GetFlagImageIndex(item.Flag);
                    myRow.Cells["flagRef"].BackColor = GetFlagBackColor(item.Flag);
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
                            if (item.Flag != LetterFlag.Green && IsCreditor == false)
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
                throw;
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
            if (e.ColIndex == 0 || e.ColIndex == 6 || e.ColIndex == 7 || e.ColIndex == 10)
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

        #region Rejected Button   
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


                    DialogResult result = MessageBox.Show(builder.ToString(), Application.CompanyName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        if (Mail == null)
                        {
                            Mail = new ServiceSendMail(3, UserParticipant); // Parameter!!! *********************************

                        }
                        // Reject in Sii
                        // ACD: Acepta Contenido del Documento
                        // RCD: Reclamo al Contenido del Documento
                        // ERM: Otorga Recibo de Mercaderías o Servicios
                        // RFP: Reclamo por Falta Parcial de Mercaderías
                        // RFT: Reclamo por Falta Total de Mercaderías
                        respuestaTo resp = ServiceSoap.SendActionToSii(TokenSii, detalle, "RCD");
                        // Tester
                        //respuestaTo resp = new respuestaTo
                        //{
                        //    codResp = 0
                        //};
                        detalle.StatusDetalle = StatusDetalle.Rejected;
                        if (detalle.IsParticipant && detalle.Instruction != null)
                        {
                            switch (resp.codResp)
                            {
                                case 0: // Acción Completada OK. 

                                    // Send email
                                    Mail.SendEmailToParticipant(detalle);
                                    // Reject in CEN                                
                                    ResultDte doc = Dte.SendDteDebtorAsync(detalle, TokenCen).Result;
                                    detalle.Instruction.Dte = doc;
                                    IGridMain.CurRow.Cells["P3"].Value = 1;

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
                            TssLblMensaje.Text = "Impossible to send Email.";
                        }
                    }
                    // MessageBox.Show(string.Format("Button cell ({0}, {1}) clicked!", e.RowIndex, e.ColIndex));
                }
            }
        }


        #endregion

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
            if (!BgwPay.IsBusy && !IsCreditor && !IsRunning)
            {
                IList<Detalle> detallesFinal = new List<Detalle>();
                if (ChkIncludeCEN.CheckState == CheckState.Checked)
                {
                    foreach (Detalle item in DetallesDebtor) // Only Participants
                    {
                        if (item.Instruction != null && item.IsParticipant && item.StatusDetalle == StatusDetalle.Accepted)
                        {
                            detallesFinal.Add(item);
                        }
                    }
                    if (detallesFinal.Count > 0)
                    {
                        ServiceExcel serviceExcel = new ServiceExcel(detallesFinal, UserParticipant, TokenCen);
                        serviceExcel.CreateNomina(BgwPay);
                    }
                }
                else
                {
                    foreach (Detalle item in DetallesDebtor)
                    {
                        if (item.StatusDetalle == StatusDetalle.Accepted)
                        {
                            detallesFinal.Add(item);
                        }
                    }
                    if (detallesFinal.Count > 0)
                    {
                        ServiceExcel serviceExcel = new ServiceExcel(detallesFinal, UserParticipant, TokenCen);
                        serviceExcel.CreateNomina(BgwPay);
                    }
                }
                if (detallesFinal.Count == 0)
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

