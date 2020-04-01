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
using Centralizador.Models.DataBase;
using Centralizador.Models.Outlook;

using TenTec.Windows.iGridLib;

namespace Centralizador.WinApp.GUI
{

    public partial class FormMain : Form
    {
        #region Global variables/prop



        //Creditor
        private IList<ResultInstruction> InstructionsCreditor { get; set; }
        private IList<ResultPaymentMatrix> MatricesCreditor { get; set; }

        //Debitor
        private IList<ResultInstruction> InstructionsDebitor { get; set; }
        private IList<ResultPaymentMatrix> MatricesDebitor { get; set; }
        private ServiceLibro LibroCompraDebtor { get; set; }

        public int MyProperty { get; set; }

        //General
        private ResultParticipant UserParticipant { get; set; }
        private DateTime DateTimeCbo { get; set; }
        private CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");
        private IEnumerable<ResultBilingType> BillingTypes { get; set; }
        private string Periodo1 { get; set; }
        private string Periodo2 { get; set; }


        #endregion

        #region FormMain methods

        public FormMain()
        {
            InitializeComponent();
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

            // Time
            TssLblFechaHora.Text = string.Format(CultureInfo, "{0:D}", DateTime.Today);

            // Controls
            BtnCreditor.Enabled = false;
            BtnDebitor.Enabled = false;
            BtnPdfConvert.Enabled = false;
            BtnFacturar.Enabled = false;

        }

        private void BtnCreditor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            else
            {
                DateTimeCbo = new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1);
                UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;
                Periodo1 = $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 1)}";
                Periodo2 = $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 2)}";




                BtnCreditor.Enabled = false;

                BackgroundW.RunWorkerAsync("Creditor");
            }


        }

        private void BtnDebitor_Click(object sender, EventArgs e)
        {

            if (CboParticipants.SelectedIndex == 0)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            else
            {
                DateTimeCbo = new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1);
                UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;

                // Get libro compra
                IList<Detalle> detalles = new List<Detalle>();
                Periodo1 = $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 1)}";
                detalles = ServiceLibro.GetLibro("Debitor", UserParticipant, "33", Periodo1);

                // Debo llegar desde outlook a matrices por el natural_key
                if (detalles != null)
                {
                    foreach (Detalle item in detalles)
                    {

                    }
                }
                MessageBox.Show(outlook.Attachments.Count.ToString());


                // Get list of payments matrix            
                // IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrix(DateTimeCbo);

            }


        }

        #endregion

        #region IGridMain methods
        private void IGridMain_CustomDrawCellForeground(object sender, iGCustomDrawCellEventArgs e)
        {
            if (e.ColIndex == 1)
            {
                // Draw the row numbers.
                e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
                e.Graphics.DrawString(
                    (e.RowIndex + 1).ToString(),
                    Font,
                    SystemBrushes.ControlText,
                    new Rectangle(e.Bounds.X + 2, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height));
            }
        }

        private void IGridMain_ColHdrMouseDown(object sender, iGColHdrMouseDownEventArgs e)
        {
            // Prohibit sorting by the hot and current row indicator columns and by the row number column.
            if (e.ColIndex == 0 || e.ColIndex == 1)
            {
                e.DoDefault = false;
            }
        }

        private void IGridDesign()
        {
            try
            {
                IGridMain.BeginUpdate();
                // Delete previous
                IGridMain.Cols.Clear();
                IGridMain.Rows.Clear();
                // Set up the frozen area parameters.
                // First frozen column will display row numbers.
                IGridMain.FrozenArea.ColCount = 2;
                IGridMain.FrozenArea.ColsEdge = new iGPenStyle(SystemColors.ControlDark, 2, DashStyle.Solid);
                //IGridMain.DefaultCol.IncludeInSelect = false;
                IGridMain.DefaultCol.AllowSizing = false;

                // Set up the width of the first frozen column (hot and current row indicator).
                IGridMain.DefaultCol.Width = 10;
                // Add the first frozen column.
                IGridMain.Cols.Add().CellStyle.CustomDrawFlags = iGCustomDrawFlags.Foreground;
                // Set up the width of the second frozen column (row numbers).
                IGridMain.DefaultCol.Width = 30;
                // Add the second frozen column.
                IGridMain.Cols.Add().CellStyle.CustomDrawFlags = iGCustomDrawFlags.Foreground;

                // Set up the default parameters of the data columns.                
                IGridMain.DefaultCol.AllowSizing = true;

                // Add data columns.                
                IGridMain.Cols.Add("Instruction");
                IGridMain.Cols.Add("Cod Prod");
                IGridMain.Cols.Add("Paso1");
                IGridMain.Cols.Add("Paso2");
                IGridMain.Cols.Add("Paso3");
                IGridMain.Cols.Add("Paso4");
                IGridMain.Cols.Add("Rut");
                IGridMain.Cols.Add("Dv");
                IGridMain.Cols.Add("Amount");
                IGridMain.Cols.Add("Exent amount");
                IGridMain.Cols.Add("Iva");
                IGridMain.Cols.Add("Total");
                IGridMain.Cols.Add("Folio");
                IGridMain.Cols.Add("F. emisión");
                IGridMain.Cols.Add("Cbte.");
                IGridMain.Cols.Add("F. de pago");
                IGridMain.Cols.Add("pic");
                IGridMain.Cols.Add("F. de envío");
                IGridMain.Cols.Add("flag");
                IGridMain.Cols.Add("status");
                IGridMain.Cols.Add("F. de envío");
                IGridMain.Cols.Add("M");
                IGridMain.Cols.Add("Aceptado cliente");

                // General config Grid
                IGridMain.SelectionMode = iGSelectionMode.MultiExtended;
                IGridMain.RowMode = true;
                IGridMain.SelRowsBackColor = Color.Empty;
                IGridMain.SelRowsForeColor = Color.Empty;

                // Rows selected
                Color myHighlightColor = SystemColors.Highlight;
                IGridMain.SelCellsBackColor = Color.FromArgb(100, myHighlightColor.R, myHighlightColor.G, myHighlightColor.B);


                // Fill up the cells with data.                
                iGRow myRow;
                if (InstructionsCreditor != null)
                {

                    foreach (ResultInstruction instruction in InstructionsCreditor)
                    {
                        // Add rows.                    
                        myRow = IGridMain.Rows.Add();
                        myRow.Cells[2].Value = instruction.Id;
                        myRow.Cells[3].Value = BillingTypes.FirstOrDefault(x => x.Id == instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                        myRow.Cells[4].Value = "p1";
                        myRow.Cells[5].Value = "p2";
                        myRow.Cells[6].Value = "p3";
                        myRow.Cells[7].Value = "p4";
                        myRow.Cells[8].Value = instruction.Participant.Rut;
                        myRow.Cells[9].Value = instruction.Participant.VerificationCode;
                        myRow.Cells[10].Value = string.Format(CultureInfo, "{0:C0}", instruction.Amount);
                        //exent
                        //iva
                        //total
                        uint folio = 0;

                        if (instruction.Softland.References != null)
                        {
                            folio = instruction.Softland.References.Max(x => x.Folio); //Folio mayor de la lista
                            myRow.Cells[14].Value = folio;
                            myRow.Cells[15].Value = instruction.Softland.References.Max(x => x.FechaEmision);
                        }

                        if (instruction.Softland.InfoSiis != null)
                        {

                            myRow.Cells[18].Value = instruction.Softland.InfoSiis.FirstOrDefault(x => x.Folio == folio).EnviadoSII;
                            myRow.Cells[19].Value = instruction.Softland.InfoSiis.FirstOrDefault(x => x.Folio == folio).FechaEnvioSII;
                            myRow.Cells[20].Value = "flag";
                            myRow.Cells[21].Value = instruction.Softland.InfoSiis.FirstOrDefault(x => x.Folio == folio).EnviadoSII;
                        }

                    }
                    // Fit the columns' width.
                    IGridMain.Cols.AutoWidth();
                }
            }
            catch (Exception)
            {

                throw;
            }
            IGridMain.EndUpdate();
            IGridMain.Focus();

        }

        #endregion

        private void BackgroundW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string UserType = e.Argument.ToString();

            switch (UserType)
            {
                case "Creditor":
                    // Get list of payments matrix            
                    MatricesCreditor = PaymentMatrix.GetPaymentMatrix(DateTimeCbo);
                    if (MatricesCreditor != null)
                    {
                        //Get libro
                        IList<Detalle> detalles1 = new List<Detalle>();
                        IList<Detalle> detalles2 = new List<Detalle>();
                        detalles1 = ServiceLibro.GetLibro("Creditor", UserParticipant, "33", Periodo1);
                        detalles2 = ServiceLibro.GetLibro("Creditor", UserParticipant, "33", Periodo2);

                        detalles1 = detalles1.Concat(detalles2).ToList();

                        if (detalles1 == null)
                        {
                            break;
                        }

                        InstructionsCreditor = new List<ResultInstruction>();
                        int m = 1;
                        foreach (ResultPaymentMatrix matrix in MatricesCreditor)
                        {
                            // if (matrix.Id == 680)
                            //{
                            IList<ResultInstruction> instructions = Instruction.GetInstructions(matrix, UserParticipant, "Creditor");
                            if (instructions != null)
                            {
                                BackgroundW.ReportProgress(0, "");
                                int i = 0;
                                foreach (ResultInstruction instruction in instructions)
                                {
                                    instruction.PaymentMatrix = matrix;
                                    Softland softland = new Softland(detalles1);
                                    softland.GetSoftlandData(instruction);
                                    instruction.Softland = softland;

                                    //guardar libro de venta (folio y status) en instruction, no enlazarlo completo FIN!






                                    //Add
                                    InstructionsCreditor.Add(instruction);
                                    i++;
                                    Thread.Sleep(1000);
                                    BackgroundW.ReportProgress(100 * i / instructions.Count, $"Working...in: {matrix.NaturalKey} ({m} de {MatricesCreditor.Count})");
                                }
                            }
                            m++;
                            //}
                        }
                    }
                    BackgroundW.ReportProgress(100, "Complete!...");
                    break;





                case "Debtor":

                    break;
            }



        }

        private void BackgroundW_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }

        private void BackgroundW_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {

            IGridDesign();
            TssLblProgBar.Value = 0;
            BtnCreditor.Enabled = true;
        }

        private void BtnFacturar_Click(object sender, EventArgs e)
        {
            // Update Dte from Sii


            //**************obligar al suario a descargar el archivo desde sii y dejarlo en el escritorio.



            if (CboParticipants.SelectedIndex == 0)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            else
            {
                DialogResult resp = MessageBox.Show("Are you sure?", "Insert NV to Softland", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (resp == DialogResult.OK)
                {
                    UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;
                    ServiceEmail.GetFile(UserParticipant);


                }


                // Check aux (comuna, giro, dte, )



                // Insert NV
                // Check if exists?






                // Insert Ref

            }
        }

        private void CboParticipants_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox senderComboBox = sender as ComboBox;
            //BackgroundWorker worker = sender as BackgroundWorker;
            if (senderComboBox.SelectedIndex != 0)
            {
                TssLblMensaje.Text = "";
            }
        }

        #region Outlook


        private ServiceOutlook outlook;
        private void BtnOutlook_Click(object sender, EventArgs e)
        {
            // Date & folder
            outlook = new ServiceOutlook
            {
                Year = CboYears.SelectedItem,
                Month = CboMonths.SelectedIndex + 1
            };

            // Read & save email
            BackgroundWorker bgwReadEmail = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            bgwReadEmail.ProgressChanged += BgwReadEmail_ProgressChanged;
            bgwReadEmail.RunWorkerCompleted += BgwReadEmailRunWorkerCompleted;
            //outlook.GetXmlFromEmail(bgwReadEmail);

            // Read Xml
            BackgroundWorker bgwReadXml = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            bgwReadXml.ProgressChanged += BgwReadXmlProgressChanged;
            bgwReadXml.RunWorkerCompleted += BgwReadXmlRunWorkerCompleted;
            outlook.ReadXML(bgwReadXml);

            BtnCreditor.Enabled = true;
            BtnDebitor.Enabled = true;



        }


        private void BgwReadEmail_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void BgwReadEmailRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TxtDateTimeEmail.Text = string.Format("{0:g}", outlook.LastTime);
            TssLblProgBar.Value = 0;
            TssLblMensaje.Text = "";
        }
        private void BgwReadXmlProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void BgwReadXmlRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TxtDateTimeEmail.Text = string.Format("{0:g}", outlook.LastTime);
            TssLblProgBar.Value = 0;
            TssLblMensaje.Text = "";
   
        }



        #endregion

        private void BtnPdfConvert_Click(object sender, EventArgs e)
        {

        }
    }
}

