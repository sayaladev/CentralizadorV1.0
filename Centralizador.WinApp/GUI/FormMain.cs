using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.AppFunctions;
using Centralizador.Models.DataBase;
using Centralizador.Models.Outlook;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using TenTec.Windows.iGridLib;

using static Centralizador.Models.ApiSII.ServiceDetalle;

using Timer = System.Windows.Forms.Timer;

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
        private ResultParticipant UserParticipant { get; set; }
        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");
        private IEnumerable<ResultBilingType> BillingTypes { get; set; }
        public string TokenSii { get; set; }
        public string TokenCen { get; set; }
        public ResultAgent Agent { get; set; }
        private bool IsCreditor { get; set; }
        public bool IsRunning { get; set; }
        public ServiceOutlook ServiceOutlook { get; set; }
        public int Intervalo { get; set; }
        public StringBuilder StringLogging { get; set; }
        public string DataBaseName { get; set; }
        // Button
        private readonly IGButtonColumnManager Btn = new IGButtonColumnManager();


        #endregion

        #region FormMain methods

        public FormMain(string tokenSii, ResultAgent agent, string tokenCen)
        {
            InitializeComponent();
            TokenSii = tokenSii;
            Agent = agent;
            TokenCen = tokenCen;

        }
        private void FormMain_Load(object sender, EventArgs e)
        {
            // Load
            IList<ResultParticipant> participants = new List<ResultParticipant>();
            foreach (ResultParticipant item in Agent.Participants)
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
            BillingTypes = BilingType.GetBilinTypesAsync();

            // User email
            TssLblUserEmail.Text = "|  " + Agent.Email;

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



            // Logging file
            StringLogging = new StringBuilder();

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
            if (Intervalo == 12720000)
            {
                TokenSii = ServiceSoap.GETTokenFromSii();
                if (ServiceOutlook != null)
                {
                    ServiceOutlook.TokenSii = TokenSii;
                }
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
            IGridMain.Cols.Add("status", "Status", 50, pattern).CellStyle = cellStyleCommon;

            // Button Reject      
            //IGridMain.Cols.Add("btnRejected", "Reject", 40, pattern).Tag = IGButtonColumnManager.BUTTON_COLUMN_TAG;
            //Btn.Attach(IGridMain);

            //IGridMain.Cols["btnRejected"].CellStyle.CustomDrawFlags = iGCustomDrawFlags.Foreground;

            Btn.CellButtonClick += Bcm_CellButtonClick;
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

            // Header & Footer
            IGridMain.Header.Cells[0, "inst"].SpanCols = 3;
            IGridMain.Header.Cells[0, "P1"].SpanCols = 4;
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

            IGridMain.EndUpdate();
        }



        private void CboParticipants_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex != 0)
            {
                DataBaseName = "";
                UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;

                XmlDocument document = Models.Properties.Settings.Default.DBSoftland;
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
                    MessageBox.Show("This company does not have an associated database in the config file (Xml)", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    CboParticipants.SelectedIndex = 0;
                    TxtCtaCteParticipant.Text = "";
                    TxtRutParticipant.Text = "";
                }
                else
                {
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

        #endregion

        #region Insert NV

        private void BtnInsertNv_Click(object sender, EventArgs e)
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
            string file = $"ce_empresas_dwnld_{now.Year}{string.Format("{0:00}", now.Month)}{string.Format("{0:00}", now.Day)}.csv";
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + file;
            if (!File.Exists(path))
            {
                MessageBox.Show($"The file '{file}' NOT found, please download...", "Centralizador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            if (item.DataEvento.ListEvenHistDoc.FirstOrDefault(x => x.CodEvento == "RCD") != null) // Only if NC
                            {
                                detallesFinal.Add(item);
                            }
                        }
                    }
                }
                if (item.Folio == 0)
                {
                    detallesFinal.Add(item);
                }
            }
            if (detallesFinal.Count > 0)
            {
                DialogResult resp = MessageBox.Show($"There are {detallesFinal.Count} pending payment instructions for billing{Environment.NewLine + Environment.NewLine}Are you sure?", "Centralizador", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resp == DialogResult.Yes)
                {
                    BgwInsertNv.RunWorkerAsync(detallesFinal);
                }
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
            Conexion con = new Conexion(DataBaseName);

            foreach (Detalle item in detallesFinal)
            {
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
                            // Update dte & name aux from all instructions 
                            string name = ti.ToTitleCase(a.Name.ToLower());
                            item.Instruction.ParticipantDebtor.BusinessName = name;
                            item.Instruction.ParticipantDebtor.DteReceptionEmail = a.Email;
                            item.Instruction.ParticipantDebtor.Name = item.Instruction.ParticipantDebtor.Name.ToUpper();
                            //StringLogging.Append(item.Instruction.Id + "\t" + "Update email" + "\t\t" + "Yes" + Environment.NewLine);
                        }
                    }
                    catch (Exception)
                    {
                        StringLogging.Append(item.Instruction.Id + "\t" + "Update email" + "\t\t" + "Error in CSV file." + Environment.NewLine);
                        continue;
                    }

                    // Crear auxiliares nuevos
                    Auxiliar auxiliar = Auxiliar.GetAuxiliar(item.Instruction, con);
                    if (auxiliar == null)
                    {
                        // Get comunas                      
                        IList<Comuna> comunas = Comuna.GetComunas(con);
                        if (comunas != null)
                        {
                            foreach (Comuna com in comunas)
                            {
                                if (item.Instruction.ParticipantDebtor.CommercialAddress.Contains(com.ComDes))
                                {
                                    comuna = com;
                                    break;
                                }
                            }
                        }
                        // Get acteco
                        IList<Actividade> actividades = Acteco.GetActecoCode(item.Instruction.ParticipantDebtor);
                        if (actividades != null)
                        {
                            acteco = actividades[0].Giro.Substring(0, 60);
                            // Insert acteco
                            Acteco.InsertActeco(acteco, con);
                        }
                        else
                        {
                            StringLogging.Append(item.Instruction.Id + "\t" + "Auxiliar Insert:" + "\t" + "Error API: " + item.Instruction.ParticipantDebtor.Rut + Environment.NewLine);
                        }
                        // Insert aux
                        result = Auxiliar.InsertAuxiliar(item.Instruction, acteco, comuna, con);
                        switch (result)
                        {
                            case 0:
                                break;
                            case 1:
                                StringLogging.Append(item.Instruction.Id + "\t" + "Auxiliar Insert:" + "\t" + item.Instruction.ParticipantDebtor.Rut + Environment.NewLine);
                                break;
                            case -1:
                                break;
                            case 99:
                                StringLogging.Append(item.Instruction.Id + "\t" + "Auxiliar Insert:" + "\t" + "Error" + Environment.NewLine);
                                break;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(auxiliar.ComAux))
                        {
                            StringLogging.Append(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error: ComAux" + Environment.NewLine);
                            continue;
                        }
                        if (string.IsNullOrEmpty(auxiliar.DirAux))
                        {
                            StringLogging.Append(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error: DirAux" + Environment.NewLine);
                            continue;
                        }
                        if (string.IsNullOrEmpty(auxiliar.GirAux))
                        {
                            StringLogging.Append(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error: GirAux" + Environment.NewLine);
                            continue;
                        }
                        if (string.IsNullOrEmpty(auxiliar.RutAux))
                        {
                            StringLogging.Append(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error: RutAux" + Environment.NewLine);
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
                                StringLogging.Append(item.Instruction.Id + "\t" + "Auxiliar Update:" + "\t" + "Error: " + item.Instruction.ParticipantDebtor.Rut + Environment.NewLine);
                                break;
                        }
                    }

                    // Insert NV
                    int lastF = NotaVenta.GetLastNv(con);
                    string prod = BillingTypes.FirstOrDefault(x => x.Id == item.Instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                    result = NotaVenta.InsertNv(item.Instruction, lastF + 1, prod, con);
                    switch (result)
                    {
                        case 1:
                            StringLogging.Append(item.Instruction.Id + "\t" + "NV Insert:" + "\t\t" + (lastF + 1) + Environment.NewLine);
                            break;
                        case 0:
                            break;
                        case -1:
                            // StringLogging.Append(item.Instruction.Id + "\t" + "NV Insert:" + "\t\t" + (lastF + 1) + Environment.NewLine);
                            break;
                        case 99: // Error
                            StringLogging.Append(item.Instruction.Id + "\t" + "NV Insert:" + "\t\t" + "Error" + Environment.NewLine);
                            break;

                    }
                }
                else
                {
                    StringLogging.Append(item.Instruction.Id + "\t" + "NV Insert:" + "\t\t" + "*" + (F) + Environment.NewLine);
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
                DialogResult resp = MessageBox.Show($"CEN references will be inserted{Environment.NewLine + Environment.NewLine}Are you sure?", "Centralizador", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resp == DialogResult.Yes)
                {
                    BgwInsertRef.RunWorkerAsync(detallesFinal);
                }

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
            Conexion con = new Conexion(DataBaseName);

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
                            case 1:
                                StringLogging.Append(item.Instruction.Id + "\t" + "REF Insert:" + "\t\t" + item.Folio + Environment.NewLine);
                                break;
                            case 0:
                                break;
                            case -1: // ya existe
                                StringLogging.Append(item.Instruction.Id + "\t" + "REF Insert:" + "\t\t" + "*" + item.Folio + Environment.NewLine);
                                break;
                            case 99: // Error
                                StringLogging.Append(item.Instruction.Id + "\t" + "REF Insert:" + "\t\t" + "Error" + Environment.NewLine);
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

                DialogResult = MessageBox.Show($"You are going to convert {lista.Count} documents,{Environment.NewLine}Are you sure?", "Centralizador", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                TssLblMensaje.Text = $"There are no published instructions:  {CboMonths.SelectedItem}-{CboYears.SelectedItem}";
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
                m.BillingWindow = BillingWindow.GetBillingWindowById(m); ;
                IList<ResultInstruction> lista = Instruction.GetInstructionCreditor(m, UserParticipant);
                if (lista != null)
                {
                    instructions.AddRange(lista);
                }
                c++;
                porcent = (float)(100 * c) / matrices.Count;
                BgwCreditor.ReportProgress((int)porcent, $"Retrieve information Creditor, wait please. ({c}/{matrices.Count})");
            }
            c = 0;
            foreach (ResultInstruction instruction in instructions)
            {
                // Tester
                //if (instruction.Id != 1783538)
                //{
                //    continue;
                //}
                instruction.ParticipantDebtor = Participant.GetParticipantById(instruction.Debtor);
                Detalle detalle = new Detalle(instruction.ParticipantDebtor.Rut, instruction.ParticipantDebtor.VerificationCode, instruction.ParticipantDebtor.BusinessName, instruction.Amount, instruction, true);
                // REF from Softland          
                Conexion con = new Conexion(DataBaseName);
                DTEDefType xmlObjeto = null;
                IList<Reference> references = Reference.GetInfoFactura(instruction, con);
                if (references != null)
                {
                    Reference reference = references.OrderByDescending(x => x.Folio).First();
                    if (reference.FileBasico != null)
                    {
                        xmlObjeto = ServicePdf.TransformStringDTEDefTypeToObjectDTE(reference.FileBasico);
                    }
                    if (xmlObjeto != null)
                    {
                        detalle.DTEDef = xmlObjeto;
                        DTEDefTypeDocumento dte = (DTEDefTypeDocumento)xmlObjeto.Item;
                        detalle.MntIva = Convert.ToInt32(dte.Encabezado.Totales.IVA);
                        detalle.MntTotal = Convert.ToInt32(dte.Encabezado.Totales.MntTotal);
                    }
                    detalle.Folio = reference.Folio;
                    detalle.References = reference;
                    if (reference.FechaRecepcionSii != null)
                    {
                        detalle.FechaRecepcion = reference.FechaRecepcionSii.ToString();
                        // Events                      
                        detalle.DataEvento = ServiceEvento.GetStatusDte("Creditor", TokenSii, "33", detalle, UserParticipant);
                        // Flags         
                        detalle.Flag = ValidateCen(detalle);
                        // Insert CEN
                        ResultDte doc = Dte.GetDteByFolio(detalle, true);
                        if (doc == null)
                        {
                            doc = Dte.SendDteCreditor(detalle, TokenCen);
                        }
                        detalle.Instruction.Dte = doc;
                        // Status
                        detalle.StatusDetalle = GetStatus(detalle);
                    }
                    if (reference.FechaEmision != null)
                    {
                        detalle.FechaEmision = reference.FechaEmision.ToString();
                    }
                }
                c++;
                DetallesCreditor.Add(detalle);
                porcent = (float)(100 * c) / instructions.Count;
                BgwCreditor.ReportProgress((int)porcent, $"Retrieve information Creditor, wait please. ({c}/{instructions.Count})");
            }
            // Order the list
            //DetallesCreditor = DetallesCreditor.OrderBy(x => x.Folio).ToList();
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
            //BtnRechazar.Enabled = false;
            //BtnPagar.Enabled = false;
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
            DetallesDebtor = GetLibro("Debtor", UserParticipant, "33", $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 1)}", TokenSii);
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
                DTEDefType xmlObjeto = null;
                DTEDefTypeDocumento dte = null;
                DTEDefTypeDocumentoReferencia[] references = null;
                DTEDefTypeDocumentoReferencia reference = null;
                ResultBillingWindow window = null;

                // XML file in folder
                nameFile = nameFilePath + $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__33__{item.Folio}.xml";
                if (File.Exists(nameFile))
                {
                    xmlObjeto = ServicePdf.TransformXmlDTEDefTypeToObjectDTE(nameFile);
                    item.DTEDef = xmlObjeto;
                }
                // Participant
                ResultParticipant participant = Participant.GetParticipantByRut(item.RutReceptor.ToString());
                if (participant != null)
                {
                    item.IsParticipant = true;
                    IList<ResultInstruction> instructions = Instruction.GetInstructionByParticipants(participant, UserParticipant);
                    if (instructions != null)
                    {
                        // Find Instruction by amount
                        IList<ResultInstruction> i = instructions.Where(x => x.Amount == item.MntNeto).ToList();
                        if (i.Count == 1)
                        {
                            item.Instruction = i[0];
                            item.Instruction.PaymentMatrix = PaymentMatrix.GetPaymentMatrixById(i[0]);
                            item.Instruction.PaymentMatrix.BillingWindow = BillingWindow.GetBillingWindowById(item.Instruction.PaymentMatrix);
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
                                            IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrixByBillingWindowId(window);
                                            ResultPaymentMatrix matrix = matrices.FirstOrDefault(x => x.NaturalKey == reference.RazonRef.TrimEnd());
                                            if (matrix != null)
                                            {
                                                item.Instruction = Instruction.GetInstructionDebtor(matrix, participant, UserParticipant);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                // Flags         
                item.Flag = ValidateCen(item);
                // Events
                item.DataEvento = ServiceEvento.GetStatusDte("Debtor", TokenSii, "33", item, UserParticipant);
                // Status
                if (item.DataEvento != null)
                {
                    item.StatusDetalle = GetStatus(item);
                }
                // Insert CEN
                if (item.StatusDetalle != StatusDetalle.No && item.Instruction != null)
                {
                    ResultDte doc = Dte.GetDteByFolio(item, false);
                    if (doc == null)
                    {
                        doc = Dte.SendDteDebtor(item, TokenCen);
                    }
                    item.Instruction.Dte = doc;
                }
                c++;
                float porcent = (float)(100 * c) / DetallesDebtor.Count;
                BgwDebtor.ReportProgress((int)porcent, $"Retrieve information Debtor, wait please. ({c}/{DetallesDebtor.Count})");
                //Thread.Sleep(100);
            }
            DetallesDebtor = DetallesDebtor.OrderBy(x => x.FechaRecepcion).ToList();
            //e.Result = true;
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
                    if (item.Folio > 0) { myRow.Cells["folio"].Value = item.Folio; }
                    if (item.FechaEmision != null) { myRow.Cells["fechaEmision"].Value = string.Format(CultureInfo, "{0:d}", Convert.ToDateTime(item.FechaEmision)); }
                    if (item.FechaRecepcion != null) { myRow.Cells["fechaEnvio"].Value = string.Format(CultureInfo, "{0:d}", Convert.ToDateTime(item.FechaRecepcion)); }
                    if (item.DTEDef != null) { myRow.Cells["flagxml"].TypeFlags = iGCellTypeFlags.HasEllipsisButton; }
                    if (IsCreditor)
                    {
                        myRow.Cells["P1"].Type = iGCellType.Check;
                        myRow.Cells["P2"].Type = iGCellType.Check;
                        if (item.Instruction.Dte != null)
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
                            if (item.Instruction != null && item.Instruction.Dte != null)
                            {
                                myRow.Cells["P3"].Value = 1;
                            }
                        }
                    }

                    // Flags                    
                    myRow.Cells["flagRef"].ImageIndex = GetFlagImageIndex(item.Flag);
                    myRow.Cells["flagRef"].BackColor = GetFlagBackColor(item.Flag);
                    // Status
                    myRow.Cells["status"].Value = item.StatusDetalle;
                   
                }
                // Finally
                //IGridMain.Cols["btnRejected"].ImageIndex = 6;
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
            TxtDscItem.Text = "";
            TxtTpoDocRef.Text = "";
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
                if (detalle.References != null)
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
            if (!IsRunning)
            {
                if (e.ColIndex == 19)
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
                    if (detalle.DataEvento != null)
                    {
                        if (detalle.DataEvento.ListEvenHistDoc.Count > 0)
                        {
                            builder.Append("Events:" + Environment.NewLine);
                            foreach (ListEvenHistDoc item in detalle.DataEvento.ListEvenHistDoc)
                            {
                                builder.Append($"{string.Format(CultureInfo, "{0:dd-MM-yyyy}", Convert.ToDateTime(item.FechaEvento))}");
                                builder.Append($" - {item.CodEvento}: {item.DescEvento}" + Environment.NewLine);

                            }
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
            MessageBox.Show(string.Format("Button cell ({0}, {1}) clicked!", e.RowIndex, e.ColIndex));
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

        #region Pagar
        private void BtnPagar_Click(object sender, EventArgs e)
        {

        }
        #endregion




    }
}

