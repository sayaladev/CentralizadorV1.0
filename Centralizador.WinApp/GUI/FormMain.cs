using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Centralizador.Models;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;

using Centralizador.Models.DataBase;
using Centralizador.Models.FunctionsApp;
using Centralizador.Models.Helpers;
using Centralizador.Models.Outlook.MailKit;
using Centralizador.Models.registroreclamodteservice;

using TenTec.Windows.iGridLib;

using static Centralizador.Models.Helpers.HEnum;
using static Centralizador.Models.Helpers.HFlagValidator;

namespace Centralizador.WinApp.GUI
{
    public partial class FormMain : Form
    {
        public FormMain(string tokenCen, string tokenSii, List<ResultParticipant> participants)
        {
            TokenCen = tokenCen;
            TokenSii = tokenSii;
            Participants = participants;
            InitializeComponent();
        }

        private ICveFunction cveFunction;

        private ResultParticipant UserParticipant;
        private string DataBase;
        public string TokenCen { get; set; }
        public string TokenSii { get; set; }
        private List<ResultParticipant> Participants { get; set; }
        private Progress<HPgModel> Progress { get; set; }

        public BackgroundWorker BgwPay { get; private set; }

        #region METHODS FORM

        public FormMain()
        {
            InitializeComponent();
        }

        private void BtnHiperLink_MouseHover(object sender, EventArgs e)
        {
            try
            {
                if (IGridMain.CurRow == null || cveFunction.IsRuning)
                {
                    return;
                }
                Detalle detalle = null;
                if (cveFunction.DetalleList != null)
                {
                    detalle = cveFunction.DetalleList.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
                }
                if (detalle != null && detalle.Instruction != null && !cveFunction.IsRuning)
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

        private async void CboParticipants_SelectionChangeCommitted(object sender, EventArgs e)
        {
            TxtCtaCteParticipant.Text = "";
            TxtRutParticipant.Text = "";
            TssLblMensaje.Text = "";
            if (CboParticipants.SelectedIndex != 0)
            {
                ResultParticipant up = (ResultParticipant)CboParticipants.SelectedItem;
                UserParticipant = up;
                Dictionary<string, string> dic = Models.Properties.Settings.Default.DicCompanies;
                if (!dic.ContainsKey(up.Id.ToString()))
                {
                    MessageBox.Show("This company does not have an associated database in the config file (Xml)", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    TssLblMensaje.Text = @"Please, edit 'C:\Centralizador\Softland_DB_Names.xml' file with ID => " + up.Id.ToString();
                    CboParticipants.SelectedIndex = 0;
                    return;
                }
                DataBase = dic[up.Id.ToString()];
                // CHECK THE DB IN SQL SERVER.
                if (await DteInfoRef.GetNameDB(new Conexion("master"), DataBase) == "FALSE")
                {
                    MessageBox.Show("This company does not exists on Sql server.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    TssLblMensaje.Text = "";
                    CboParticipants.SelectedIndex = 0;
                    return;
                }
                // NEW OBJECT.
                // cveFunction = new Cve(up, Progress, db, TokenSii, TokenCen);
                TxtCtaCteParticipant.Text = up.BankAccount;
                TxtRutParticipant.Text = up.Rut.ToString() + "-" + up.VerificationCode;
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // VARIABLES INIT.
            Progress = new Progress<HPgModel>();
            Progress.ProgressChanged += Progress_ProgressChanged;

            //cveOutook = new CveOutlook(TokenSii, Progress);
            //cveDebtor = new CveDebtor();
            // cveCreditor = new CveCreditor();

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

            //LOAD COMBOBOX.
            CboParticipants.DisplayMember = "Name";
            CboParticipants.DataSource = Participants;
            CboParticipants.SelectedIndex = 0;
            CboMonths.DataSource = DateTimeFormatInfo.InvariantInfo.MonthNames.Take(12).ToList();
            CboMonths.SelectedIndex = DateTime.Today.Month - 1;
            CboYears.DataSource = Enumerable.Range(2019, DateTime.Now.Year - 2018).ToList();
            CboYears.SelectedItem = DateTime.Now.Year;

            // User email
            TssLblUserEmail.Text = "|  " + Models.Properties.Settings.Default.UserCen;

            // Worker Pay
            BgwPay = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            BgwPay.ProgressChanged += BgwPay_ProgressChanged;
            BgwPay.RunWorkerCompleted += BgwPay_RunWorkerCompleted;

            // Logging file
            // StringLogging = new StringBuilder();

            // Date Time Outlook
            BtnOutlook.Text = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy HH:mm}", new CveOutlook().GetLastDateTime());

            // Timer Hour (every hour)
            System.Timers.Timer timerHour = new System.Timers.Timer(3600000);
            timerHour.Elapsed += TimerHour_Elapsed;
            timerHour.Enabled = true;
            timerHour.AutoReset = true;
        }

        private void Progress_ProgressChanged(object sender, HPgModel e)
        {
            // PROGRESS IFORMATION
            TssLblMensaje.Text = e.Msg;
            TssLblProgBar.Value = e.PercentageComplete;
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
            //pattern.AllowGrouping = true;

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

            //PDF BUTTON.
            IGridMain.Cols.Add("flagxml", "", 20, pattern);

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
            IGridMain.Cols["exento"].AllowGrouping = true;
            IGridMain.Cols.Add("iva", "Tax $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["iva"].AllowGrouping = true;
            IGridMain.Cols.Add("total", "Total $", 64, pattern).CellStyle = cellStyleMoney;
            IGridMain.Cols["total"].AllowGrouping = true;

            // Sii info
            //iGCellStyle cellStyNew = new iGCellStyle
            //{
            //    TextAlign = iGContentAlignment.MiddleCenter
            //    // date !!! FormatString = "{0:#,##}"
            //};
            IGridMain.Cols.Add("fechaEnvio", "Sending", 80, pattern).CellStyle = new iGCellStyle() { TextAlign = iGContentAlignment.MiddleCenter };
            IGridMain.Cols["fechaEnvio"].AllowGrouping = true;

            iGCol iGColStatus = IGridMain.Cols.Add("status", "Status", 68, pattern);
            iGColStatus.AllowGrouping = true;
            iGColStatus.CellStyle = new iGCellStyle() { TextAlign = iGContentAlignment.MiddleCenter };
            // IGridMain.Cols.Add("status", "Status", 68, pattern).CellStyle = new iGCellStyle();
            //IGridMain.Cols["status"].AllowGrouping = true;

            // Button Reject
            iGCol colbtnRejected = IGridMain.Cols.Add("btnRejected", "Reject", 40, pattern);
            colbtnRejected.Tag = IGButtonColumnManager.BUTTON_COLUMN_TAG;
            colbtnRejected.CellStyle = new iGCellStyle();
            IGButtonColumnManager Btn = new IGButtonColumnManager();
            Btn.CellButtonClick += Bcm_CellButtonClickAsync;
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
            fooUp.Value = "Pending:";
            iGFooterCell fooDown = IGridMain.Footer.Cells[1, "fechaEmision"];
            fooDown.Value = "Totals:";
            fooDown.Style = style;

            // Scroll
            IGridMain.VScrollBar.CustomButtons.Add(iGScrollBarCustomButtonAlign.Near, iGActions.GoFirstRow, "Go to first row");
            IGridMain.VScrollBar.CustomButtons.Add(iGScrollBarCustomButtonAlign.Far, iGActions.GoLastRow, "Go to last row");
            IGridMain.VScrollBar.Visibility = iGScrollBarVisibility.Always;

            IGridMain.HScrollBar.Visibility = iGScrollBarVisibility.OnDemand;

            IGridMain.EndUpdate();
        }

        private void TimerHour_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ServiceSoap s = new ServiceSoap(Models.Properties.Settings.Default.SerialNumber);
            TokenSii = s.GETTokenFromSii();
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

        public void IGridFill(List<Detalle> detalles)
        {
            try
            {
                IGridMain.BeginUpdate();
                IGridMain.Rows.Clear();
                CleanControls();
                iGRow myRow;
                int c = 0, rejectedNeto = 0, rejectedExento = 0, rejectedIva = 0, rejectedTotal = 0, rej = 0, rejNc = 0;
                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                IGridMain.Footer.Cells[0, "status"].Value = $"{rej} of {rejNc}";

                foreach (Detalle item in detalles)
                {
                    myRow = IGridMain.Rows.Add();
                    c++;
                    item.Nro = c;
                    myRow.Cells[1].Value = item.Nro;
                    if (item.Instruction != null)
                    {
                        myRow.Cells["inst"].Value = item.Instruction.Id;
                        myRow.Cells["codProd"].Value = cveFunction.BilingTypes.FirstOrDefault(x => x.Id == item.Instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                    }
                    myRow.Cells["rut"].Value = item.RutReceptor + "-" + item.DvReceptor;
                    if (item.IsParticipant) // ICON FOR PARTICIPANTS.
                    {
                        myRow.Cells["rznsocial"].ImageList = fImageListType;
                        myRow.Cells["rznsocial"].ImageIndex = 0;
                        myRow.Cells["rznsocial"].ImageAlign = iGContentAlignment.MiddleLeft;
                    }
                    // NAME
                    if (item.Instruction != null)
                    {
                        if (cveFunction.Mode == TipoTask.Creditor)
                        {
                            myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.Instruction.ParticipantDebtor.Name.ToLower());
                        }
                        else if (cveFunction.Mode == TipoTask.Debtor)
                        {
                            myRow.Cells["rznsocial"].Value = ti.ToTitleCase(item.Instruction.ParticipantCreditor.Name.ToLower());
                        }
                        if (item.Instruction.ParticipantNew != null)
                        {
                            myRow.Cells["rznsocial"].ImageList = fImageListType;
                            myRow.Cells["rznsocial"].ImageIndex = 1;
                            myRow.Cells["rznsocial"].ImageAlign = iGContentAlignment.MiddleLeft;
                            myRow.Cells["rznsocial"].Value += " / " + item.Instruction.ParticipantNew.Name.ToLower();
                        }
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
                    if (cveFunction.Mode == HEnum.TipoTask.Creditor)
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
                        myRow.Cells["flagRef"].ImageIndex = HFlagValidator.GetFlagImageIndex(item.ValidatorFlag.Flag);
                        myRow.Cells["flagRef"].BackColor = HFlagValidator.GetFlagBackColor(item.ValidatorFlag.Flag);
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
                            // ICON NC.
                            if (item.DataEvento != null && item.DataEvento.ListEvenHistDoc.Count > 0 && item.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "NCA") != null)
                            {
                                rej++;
                                myRow.Cells["status"].ImageList = fImageListSmall;
                                myRow.Cells["status"].ImageIndex = 5;
                                myRow.Cells["status"].ImageAlign = iGContentAlignment.MiddleRight;
                            }
                            break;

                        case StatusDetalle.Pending:
                            // STATUS
                            if (item.ValidatorFlag != null && item.ValidatorFlag.Flag != LetterFlag.Green && cveFunction.Mode == HEnum.TipoTask.Debtor && item.Folio > 0) // Debtor
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
                if (cveFunction.Mode == TipoTask.Creditor && rejNc > 0)
                {
                    IGridMain.Footer.Cells[0, "status"].ImageList = fImageListSmall;
                    IGridMain.Footer.Cells[0, "status"].ImageIndex = 5;
                    IGridMain.Footer.Cells[0, "status"].ImageAlign = iGContentAlignment.MiddleLeft;
                    IGridMain.Footer.Cells[0, "status"].Value = $"{rej} of {rejNc}";
                }
                //TssLblMensaje.Text = $"{cveFunction.DetalleList.Count} invoices loaded for {UserParticipant.Name.ToUpper()} company.";
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

        #endregion METHODS FORM

        #region IGRID

        private async void Bcm_CellButtonClickAsync(object sender, IGButtonColumnManager.IGCellButtonClickEventArgs e)
        {
            if (cveFunction.IsRuning)
            {
                return;
            }
            else
            {
                TssLblMensaje.Text = null;
                Detalle detalle = null;
                if (cveFunction.DetalleList != null)
                {
                    detalle = cveFunction.DetalleList.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
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
                        // GET INFO FROM CEN
                        ResultParticipant participant = detalle.ParticipantMising;
                        // REJECT IN SII.
                        respuestaTo resp = ServiceSoap.SendActionToSii(TokenSii, detalle, RejectsSii.RCD);
                        DTEDefTypeDocumento dte = null;
                        string EmailInDte = null;
                        if (detalle.DTEDef != null)
                        {
                            if ((DTEDefTypeDocumento)detalle.DTEDef.Item != null)
                            {
                                dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                                if (dte != null && dte.Encabezado != null && dte.Encabezado.Emisor.CorreoEmisor != null)
                                {
                                    EmailInDte = dte.Encabezado.Emisor.CorreoEmisor;
                                }
                            }
                        }
                        builder.Clear();
                        builder.AppendLine("Results:");
                        builder.AppendLine(Environment.NewLine);
                        if (resp != null)
                        {
                            if (resp.codResp == 0)
                            {
                                builder.AppendLine("Rejected in SII: Yes");
                                detalle.StatusDetalle = StatusDetalle.Rejected;
                            }
                            if (participant != null || EmailInDte != null)
                            {
                                // SEND EMAIL.
                                //ReportModel.TaskType = TipoTask.SendEmail;
                                //ReportModel.PercentageComplete++;
                                //ReportModel.Message = "Sending EMAIL...";
                                SendEmailTo sendMailTo = new SendEmailTo(cveFunction.UserParticipant, Progress);
                                await sendMailTo.SendMailToParticipantAsync(detalle, participant, EmailInDte);
                            }
                            else
                            {
                                builder.AppendLine("Email Send: No [No Email present]");
                            }
                            if (detalle.Instruction != null)
                            {
                                // REJECT IN CEN.
                                ResultDte doc = await Dte.SendDteDebtorAsync(detalle, TokenCen);
                                if (doc != null)
                                {
                                    detalle.Instruction.Dte = doc;
                                    IGridMain.CurRow.Cells["P3"].Value = 1;
                                }
                                else
                                {
                                    builder.AppendLine("Rejected in CEN: No");
                                }
                            }
                        }
                        else
                        {
                            builder.AppendLine("Rejected in SII: No");
                            builder.AppendLine("Email Send: No");
                            builder.AppendLine("Rejected in CEN: No");
                        }
                    }
                    //new ErrorMsgCen(builder.ToString(), MessageBoxIcon.Information);
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
            if (!cveFunction.IsRuning && IGridMain.CurRow.Type != iGRowType.AutoGroupRow && IGridMain.CurRow != null)
            {
                CleanControls();
                Detalle detalle = null;
                if (cveFunction.DetalleList != null)
                {
                    detalle = cveFunction.DetalleList.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
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
                    // GET REFERENCE SEN.
                    DTEDefTypeDocumentoReferencia r = new GetReferenceCen(detalle).DocumentoReferencia;
                    if (r != null)
                    {
                        TxtFolioRef.Text = r.FolioRef;
                        TxtRznRef.Text = r.RazonRef;
                        TxtTpoDocRef.Text = r.TpoDocRef;
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

        private void IGridMain_RequestCellToolTipText(object sender, iGRequestCellToolTipTextEventArgs e)
        {
            if (!cveFunction.IsRuning && e.ColIndex == 19) // Sii Events
            {
                Detalle detalle = null;
                StringBuilder builder = new StringBuilder();
                if (cveFunction.DetalleList != null)
                {
                    detalle = cveFunction.DetalleList.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
                }
                if (detalle != null && detalle.DataEvento != null && detalle.DataEvento.ListEvenHistDoc != null)
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
            else if (!cveFunction.IsRuning && e.ColIndex == 18) // Email Aux send Xml
            {
                Detalle detalle = null;
                if (cveFunction.Mode == TipoTask.Creditor && cveFunction.DetalleList != null)
                {
                    StringBuilder builder = new StringBuilder();
                    detalle = cveFunction.DetalleList.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
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
            else if (!cveFunction.IsRuning && e.ColIndex == 2) // History DTE
            {
                if (cveFunction.Mode == TipoTask.Creditor)
                {
                    StringBuilder builder = new StringBuilder();
                    Detalle detalle = null;
                    detalle = cveFunction.DetalleList.First(x => x.Nro == Convert.ToUInt32(IGridMain.Cells[e.RowIndex, 1].Value));
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

        #endregion IGRID

        #region PRINCIPAL BUTTONS

        private async void BtnCreditor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0) { TssLblMensaje.Text = "Plesase select a Company!"; return; }
            if (cveFunction != null && cveFunction.IsRuning) { TssLblMensaje.Text = "Bussy!"; return; }
            BtnInsertNv.Enabled = false;
            try
            {
                DateTime period = new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1);
                using (cveFunction = new CveCreditor(UserParticipant, Progress, DataBase, TokenSii, TokenCen))
                {
                    await cveFunction.GetDocFromStore(period);
                    if (cveFunction.DetalleList != null && cveFunction.DetalleList.Count > 0)
                    {
                        IGridFill(cveFunction.DetalleList);
                        TssLblMensaje.Text = $"{cveFunction.DetalleList.Count} invoices loaded for {cveFunction.UserParticipant.Name.ToUpper()} company.   [CREDITOR]";
                        TssLblMensaje.Text += $"         *[{ cveFunction.PgModel.StopWatch.Elapsed.TotalSeconds:0.00} seconds.]";
                        TssLblDBName.Text = "|DB: " + cveFunction.Conn.DBName;
                        BtnInsertNv.Enabled = true;
                    }
                    else
                    {
                        string p = $"{period.Year}-{string.Format("{0:00}", period.Month)}";
                        TssLblMensaje.Text = $"There are no instructions for the selected month '{p}'";
                        IGridMain.Rows.Clear();
                    }
                }
            }
            catch (Exception)
            {
                // new ErrorMsgCen("There was an error loading the data.", ex, MessageBoxIcon.Warning);
                TssLblMensaje.Text = "There was an error loading the data.";
                return;
            }
            finally
            {
                BtnPagar.Enabled = false;
                TssLblProgBar.Value = 0;
                BtnExcelConvert.Enabled = true;
                BtnPdfConvert.Enabled = true;
            }
        }

        private async void BtnInsertNv_ClickAsync(object sender, EventArgs e)
        {
            if (cveFunction.DetalleList == null)
            {
                return;
            }
            List<Detalle> detallesPaso = new List<Detalle>();
            List<Detalle> detallesFinal = new List<Detalle>();

            // DOWNLOAD SII FILE IF NOT EXISTS.
            DialogResult resp;
            while (!new FileSii().ExistsFile)
            {
                resp = MessageBox.Show($"The file '{@"C:\Centralizador\"}{FileSii.Path}' NOT found, please download...", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
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
            int foliosDisp = await NotaVenta.GetFoliosDisponiblesDTEAsync(cveFunction.Conn);
            //foliosDisp = 100;
            int count = cveFunction.DetalleList.Count;
            StringBuilder builder = new StringBuilder();
            foreach (Detalle item in cveFunction.DetalleList)
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
                builder.Clear();
                builder.AppendLine($"There are {foliosDispBefore} F° available, so you can only insert {detallesFinal.Count} of {detallesPaso.Count} NV");
                builder.AppendLine(); builder.AppendLine();
                builder.AppendLine("WARNING: You must run the 'FPL' process NOW or these NV will be deleted from DB.");

                resp = MessageBox.Show(builder.ToString(), Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    TssLblMensaje.Text = "Reading SII file, be patient...";
                    try
                    {
                        using (CveCreditor creditor = new CveCreditor(DataBase, Progress, UserParticipant))
                        {
                            BtnInsertNv.Enabled = false;
                            await creditor.InsertNotaVenta(detallesFinal);
                            TssLblMensaje.Text = $"Check the log file for Execute to FPL. =>Summary: From {creditor.FoliosNv.Min()} To-{creditor.FoliosNv.Max()}";
                        }
                    }
                    catch (Exception ex)
                    {
                        new ErrorMsgCen("There was an error insert into the DB.", ex, MessageBoxIcon.Warning);
                        TssLblMensaje.Text = "There was an error loading the data.";
                        return;
                    }
                    finally
                    {
                        TssLblProgBar.Value = 0;
                    }
                }
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

        private async void BtnDebtor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0) { TssLblMensaje.Text = "Plesase select a Company!"; return; }
            if (cveFunction != null && cveFunction.IsRuning) { TssLblMensaje.Text = "Bussy!"; return; }
            try
            {
                DateTime period = new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1);
                using (cveFunction = new CveDebtor(DataBase, Progress, TokenCen, TokenSii, UserParticipant))
                {
                    await cveFunction.GetDocFromStore(period);
                    if (cveFunction.DetalleList != null && cveFunction.DetalleList.Count > 0)
                    {
                        IGridFill(cveFunction.DetalleList);
                        TssLblMensaje.Text = $"{cveFunction.DetalleList.Count} invoices loaded for {cveFunction.UserParticipant.Name.ToUpper()} company.   [DEBTOR]";
                        TssLblMensaje.Text += $"         *[{ cveFunction.PgModel.StopWatch.Elapsed.TotalSeconds:0.00} seconds.]";
                        TssLblDBName.Text = "|DB: " + cveFunction.Conn.DBName;
                    }
                    else
                    {
                        IGridMain.Rows.Clear();
                    }
                }
            }
            catch (Exception)
            {
                //new ErrorMsgCen("There was an error loading the data.", ex, MessageBoxIcon.Warning);
                TssLblMensaje.Text = "There was an error loading the data.";
                return;
            }
            finally
            {
                BtnPagar.Enabled = true;
                BtnInsertNv.Enabled = false;
                TssLblProgBar.Value = 0;
                BtnExcelConvert.Enabled = true;
                BtnPdfConvert.Enabled = true;
            }
        }

        private async void BtnOutlook_Click(object sender, EventArgs e)
        {
            if (cveFunction != null && cveFunction.IsRuning) { TssLblMensaje.Text = "Bussy!"; return; }
            BtnCreditor.Enabled = false;
            BtnDebtor.Enabled = false;
            BtnCancelTask.Enabled = true;
            try
            {
                TssLblMensaje.Text = "Connecting to the mail server... Please wait.";
                using (cveFunction = new CveOutlook(TokenSii, Progress))
                {
                    await cveFunction.GetDocFromStore(new DateTime());
                }
                TssLblMensaje.Text = "Complete => Read email server.";
                IGridMain.Focus();
            }
            catch (TaskCanceledException)
            {
                TssLblMensaje.Text = "Cancel task => Read email server.";
                BtnOutlook.Text = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy HH:mm}", new CveOutlook().GetLastDateTime());
            }
            catch (Exception ex)
            {
                new ErrorMsgCen("There was an error loading the data.", ex, MessageBoxIcon.Warning);
                TssLblMensaje.Text = "There was an error loading the data.";
                throw;
            }
            finally
            {
                TssLblProgBar.Value = 0;
                BtnCreditor.Enabled = true;
                BtnDebtor.Enabled = true;
                BtnCancelTask.Enabled = false;
            }
        }

        private void BtnExcelConvert_Click(object sender, EventArgs e)
        {
            if (cveFunction.IsRuning) { TssLblMensaje.Text = "Bussy!"; return; }
            if (cveFunction.DetalleList != null && cveFunction.DetalleList.Count > 0)
            {
                ServiceExcel serviceExcel = new ServiceExcel(cveFunction.UserParticipant);
                if (cveFunction.Mode == TipoTask.Creditor)
                {
                    serviceExcel.ExportToExcel(cveFunction.DetalleList, true, CboMonths.SelectedItem);
                }
                else if (cveFunction.Mode == TipoTask.Debtor)
                {
                    serviceExcel.ExportToExcel(cveFunction.DetalleList, false, CboMonths.SelectedItem);
                }
            }
        }

        private async void BtnPdfConvert_Click(object sender, EventArgs e)
        {
            if (cveFunction.IsRuning) { TssLblMensaje.Text = "Bussy!"; return; }
            try
            {
                if (cveFunction.DetalleList != null && cveFunction.DetalleList.Count > 0)
                {
                    List<Detalle> lista = new List<Detalle>();
                    foreach (Detalle item in cveFunction.DetalleList)
                    {
                        if (item.DTEDef != null)
                        {
                            lista.Add(item);
                        }
                    }
                    if (lista.Count > 0)
                    {
                        TssLblMensaje.Text = "Converting docs to PDF, wait please.";
                        await cveFunction.ConvertXmlToPdf(cveFunction.Mode, lista);
                        TssLblMensaje.Text = "Converting docs to PDF, Complete... Please check the folder.";
                        TssLblMensaje.Text += $"         *[{cveFunction.PgModel.StopWatch.Elapsed.TotalSeconds:0.00} seconds.]";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                TssLblProgBar.Value = 0;
            }
        }

        private void BtnHiperLink_Click(object sender, EventArgs e)
        {
            if (IGridMain.CurRow == null || cveFunction.IsRuning)
            {
                return;
            }
            Detalle detalle = null;
            if (cveFunction.DetalleList != null)
            {
                detalle = cveFunction.DetalleList.First(x => x.Nro == Convert.ToUInt32(IGridMain.CurRow.Cells[1].Value));
            }
            if (detalle != null && detalle.Instruction != null)
            {
                Process.Start($"https://ppagos-sen.coordinador.cl/pagos/instrucciones/{detalle.Instruction.Id}/");
            }
        }

        private async void BtnCancelTask_Click(object sender, EventArgs e)
        {
            await cveFunction.CancelTask();
            BtnOutlook.Text = string.Format(CultureInfo.InvariantCulture, "{0:d-MM-yyyy HH:mm}", new CveOutlook().GetLastDateTime());
        }

        private async void IGridMain_CellEllipsisButtonClick(object sender, iGEllipsisButtonClickEventArgs e)
        {
            if (!cveFunction.IsRuning)
            {
                Cursor.Current = Cursors.WaitCursor;
                Detalle detalle = null;
                iGRow fCurRow = IGridMain.CurRow;
                if (cveFunction.DetalleList != null)
                {
                    detalle = cveFunction.DetalleList.First(x => x.Nro == Convert.ToInt32(fCurRow.Cells[1].Value));
                    IGridMain.DrawAsFocused = true;
                    try
                    {
                        await cveFunction.ConvertXmlToPdf(detalle, cveFunction.Mode);
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
                    finally
                    {
                        TssLblProgBar.Value = 0;
                        IGridMain.BackgroundImage = null;
                    }
                }
            }
        }

        #endregion PRINCIPAL BUTTONS

        #region Pagar

        private void BgwPay_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }

        private void BgwPay_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TssLblProgBar.Value = 0;
            if (e.Result != null)
            {
                TssLblMensaje.Text = "Success, check the Excel file.";
                //IGridFill(await BilingType.GetBilinTypesAsync());
            }
            else
            {
                TssLblMensaje.Text = "No instructions for pay.";
            }

            IGridMain.Focus();
        }

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            string monto;
            string msje = null;

            // Excluir Banco Security Rut 97.053.000-2
            if (!BgwPay.IsBusy && cveFunction.Mode == TipoTask.Debtor && !cveFunction.IsRuning && IGridMain.Rows.Count > 0)
            {
                List<Detalle> detallesFinal = new List<Detalle>();
                if (ChkIncludeCEN.CheckState == CheckState.Checked) // Only Participants
                {
                    foreach (Detalle item in cveFunction.DetalleList)
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
                    foreach (Detalle item in cveFunction.DetalleList)
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
                    foreach (Detalle item in cveFunction.DetalleList)
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
                        ServiceExcel serviceExcel = new ServiceExcel(detallesFinal, cveFunction.UserParticipant, TokenCen);
                        serviceExcel.CreateNomina(BgwPay);
                    }
                }
                else if (detallesFinal.Count == 0)
                {
                    TssLblMensaje.Text = "Cannot make payments.";
                }
            }
        }

        private async void BtnRevertPay_ClickAsync(object sender, EventArgs e)
        {
            if (cveFunction.IsRuning)
            {
                TssLblMensaje.Text = "Bussy!";
                return;
            }
            else if (cveFunction.Mode == TipoTask.Debtor)
            {
                DialogResult resp = MessageBox.Show($"You will delete all payments {Environment.NewLine} Are you sure?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resp == DialogResult.Yes)
                {
                    foreach (Detalle item in cveFunction.DetalleList)
                    {
                        if (item.Instruction != null && (item.Instruction.StatusPaid == Pay.StatusPay.Pagado || item.Instruction.StatusPaid == Pay.StatusPay.PagadoAtraso))
                        {
                            // GET PAYMENT
                            ResultPaymentExecution result = await PaymentExecution.GetPayId(item.Instruction);
                            if (result != null) { await PaymentExecution.DeletePayAsync(result, TokenCen); } // DELETE PAYMENT.
                        }
                    }
                    foreach (iGRow item in IGridMain.Rows)
                    {
                        if (item.Cells["P4"].Type == iGCellType.Check)
                        {
                            item.Cells["P4"].Value = 0;
                        }
                    }
                }
            }
        }

        #endregion Pagar
    }
}