using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

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
        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");
        private IEnumerable<ResultBilingType> BillingTypes { get; set; }
        //private string Periodo1 { get; set; }
        private string Periodo2 { get; set; }

        public string TokenSii { get; set; }


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

            // Time
            TssLblFechaHora.Text = string.Format(CultureInfo, "{0:D}", DateTime.Today);

            // Controls
            //BtnCreditor.Enabled = false;
            //BtnDebitor.Enabled = false;
            //BtnPdfConvert.Enabled = false;
            //BtnFacturar.Enabled = false;

            // Token
            TssLblTokenSii.Text = TokenSii;

            // Date Time Outlook
            TxtDateTimeEmail.Text = string.Format(CultureInfo, "{0:g}", Models.Properties.Settings.Default.DateTimeEmail);

        }

        private void BtnCreditor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            BackgroundW.RunWorkerAsync();


        }

        private void BtnDebitor_Click(object sender, EventArgs e)
        {
            if (CboParticipants.SelectedIndex == 0)
            {
                TssLblMensaje.Text = "Plesase select a Company!";
                return;
            }
            string nameFile = "";
            XmlSerializer deserializer = new XmlSerializer(typeof(EnvioDTE));
            UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;
            // Get libro compra from Sii
            IList<Detalle> detalles = new List<Detalle>();
            detalles = ServiceLibro.GetLibro("Debitor", UserParticipant, "33", $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 1)}", TokenSii);
            if (detalles != null)
            {
                foreach (Detalle item in detalles)
                {
                    nameFile += $"{Directory.GetCurrentDirectory()}\\inbox\\{CboYears.SelectedItem}\\{CboMonths.SelectedIndex + 1}";
                    nameFile += $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__{item.Folio}.xml";
                    if (File.Exists(nameFile))
                    {
                        using (StreamReader reader = new StreamReader(nameFile))
                        {
                            EnvioDTE document = (EnvioDTE)deserializer.Deserialize(reader);
                            foreach (DTEDefType type in document.SetDTE.DTE)
                            {
                                DTEDefTypeDocumento doc = (DTEDefTypeDocumento)type.Item;
                                if (doc.Referencia != null)
                                {
                                    foreach (DTEDefTypeDocumentoReferencia r in doc.Referencia)
                                    {
                                        if (r.TpoDocRef == "SEN")
                                        {
                                            string rznRef = "";
                                            ResultBillingWindow window = null;
                                            if (r.RazonRef != null)
                                            {
                                                rznRef = r.RazonRef.Substring(0, r.RazonRef.IndexOf(']', r.RazonRef.IndexOf(']') + 1) + 1);
                                                window = BillingWindow.GetBillingWindowByNaturalKey(rznRef);
                                            }
                                            if (window != null)
                                            {
                                                // Get the asociated matrix  
                                                IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrixByBillingWindowId(window);
                                                foreach (ResultPaymentMatrix m in matrices)
                                                {
                                                    if (m.NaturalKey == r.RazonRef)
                                                    {
                                                        // Get the instruction                                        
                                                        ResultParticipant participant = Participant.GetParticipantByRut(doc.Encabezado.Emisor.RUTEmisor.Split('-').GetValue(0).ToString());
                                                        ResultInstruction instruction = Instruction.GetInstructionDebtor(m, participant, UserParticipant);
                                                        // Asignament
                                                        item.Instruction = instruction;

                                                    }
                                                }

                                            }

                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                IGridDesign(detalles);
            }

        }
        private void IGridDesign(IList<Detalle> detalles)
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

                foreach (Detalle item in detalles)
                {
                    myRow = IGridMain.Rows.Add();
                    if (item.Instruction != null)
                    {
                        myRow.Cells[2].Value = item.Instruction.Id;
                        myRow.Cells[3].Value = BillingTypes.FirstOrDefault(x => x.Id == item.Instruction.PaymentMatrix.BillingWindow.BillingType).DescriptionPrefix;
                    }
                    myRow.Cells[8].Value = item.RutReceptor;
                    myRow.Cells[9].Value = item.DvReceptor;
                    myRow.Cells[10].Value = item.MntNeto;
                    myRow.Cells[11].Value = item.MntExento;
                    myRow.Cells[12].Value = item.MntIva;
                    myRow.Cells[13].Value = item.MntTotal;
                    myRow.Cells[14].Value = item.Folio;
                    myRow.Cells[15].Value = item.FechaEmision;
                    myRow.Cells[19].Value = item.FechaRecepcion;
                    switch (item.DehOrdenEvento)
                    {
                        case 1:
                            myRow.Cells[21].Value = TypeEvent.Reclamado;
                            break;
                        case 2:
                            myRow.Cells[21].Value = TypeEvent.AcuseRecibo;
                            break;
                        case 3:
                            myRow.Cells[21].Value = TypeEvent.Contado;
                            break;
                        case 4:
                            //myRow.Cells[21].Value = TypeEvent.AcuseRecibo;
                            break;
                        case 5:
                            TimeSpan len = DateTime.Today.Subtract(DateTime.Parse(item.FechaRecepcion));
                            if (len.Days > 8)
                            {
                                myRow.Cells[21].Value = TypeEvent.Aceptado;
                            }
                            else
                            {
                                myRow.Cells[21].Value = $"{8 - len.Days} d.";
                            }

                            break;
                        case 6:
                            myRow.Cells[21].Value = TypeEvent.Emitido;
                            break;


                    }

                }
                // Fit the columns' width.
                IGridMain.Cols.AutoWidth();
                TssLblMensaje.Text = $"{detalles.Count} invoices loaded for {UserParticipant.Name.ToUpper()} company.";
            }
            catch (Exception)
            {

                throw;
            }
            IGridMain.EndUpdate();
            IGridMain.Focus();

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


        #endregion



    

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
            outlook = new ServiceOutlook(Models.Properties.Settings.Default.DateTimeEmail, TokenSii);

            // Read & save email
            BackgroundWorker bgwReadEmail = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            bgwReadEmail.ProgressChanged += BgwReadEmail_ProgressChanged;
            bgwReadEmail.RunWorkerCompleted += BgwReadEmailRunWorkerCompleted;
            outlook.GetXmlFromEmail(bgwReadEmail);


            //BtnCreditor.Enabled = true;
            //BtnDebitor.Enabled = true;



        }


        private void BgwReadEmail_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }
        private void BgwReadEmailRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TssLblMensaje.Text = "Finished...";
            TssLblProgBar.Value = 0;
            TssLblMensaje.Text = "";
            TxtDateTimeEmail.Text = string.Format(CultureInfo, "{0:g}", outlook.LastTime);

        }

        #endregion

        private void BtnPdfConvert_Click(object sender, EventArgs e)
        {

        }

        private enum TypeEvent
        {
            AcuseRecibo = 2,
            Contado = 3,
            Reclamado = 1,
            Aceptado = 5,
            Emitido = 6

        }

        private void BackgroundW_DoWork(object sender, DoWorkEventArgs e)
        {
            UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;
            IList<Detalle> detalles = new List<Detalle>();

            int c = 0;
            float porcent = 0;
            IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrix(new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1));
            if (matrices != null)
            {

                foreach (ResultPaymentMatrix m in matrices)
                {

                    // Get instructions with matrix binding.
                    IList<ResultInstruction> instructions = Instruction.GetInstructionCreditor(m, UserParticipant);
                    if (instructions != null)
                    {

                        foreach (ResultInstruction instruction in instructions)
                        {
                            ResultBillingWindow window = BillingWindow.GetBillingWindowById(m);
                            m.BillingWindow = window;

                            instruction.ParticipantDebtor = Participant.GetParticipantById(instruction.Debtor);
                            instruction.References = Reference.GetInfoFactura(instruction);

                            // BillingWindow.GetBillingWindowByNaturalKey(rznRef);
                            Detalle detalle = new Detalle
                            {
                                Instruction = instruction
                            };
                            detalle.MntNeto = instruction.Amount;
                            detalle.RutReceptor = instruction.ParticipantDebtor.Rut;
                            detalle.DvReceptor = instruction.ParticipantDebtor.VerificationCode;
                            detalle.RznSocRecep = instruction.ParticipantDebtor.BusinessName;
                            if (instruction.References != null)
                            {


                                if (instruction.References[0].Folio != 0)
                                {
                                    detalle.Folio = instruction.References[0].Folio;
                                }
                                if (instruction.References[0].FechaEmision != null)
                                {
                                    detalle.FechaEmision = string.Format(CultureInfo, "{0:d}", instruction.References[0].FechaEmision);
                                }
                                if (instruction.References[0].FileEnviado != null)
                                {
                                    detalle.FechaRecepcion = instruction.References[0].FileEnviado;
                                }
                            }
                            detalles.Add(detalle);
                            c++;
                            porcent = (float)(100 * c) / matrices.Count;
                            BackgroundW.ReportProgress((int)porcent, $"Getting info from data base...   ({c}/{matrices.Count})");
                            //Thread.Sleep(500);
                        }
                    }

                }
                IGridDesign(detalles);
            }
            else
            {
                MessageBox.Show($"There are no published instructions for {UserParticipant.Name.ToUpper()} company", "Cenralizador", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }

        }

        private void BackgroundW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }

        private void BackgroundW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //IGridDesign();
            TssLblProgBar.Value = 0;
            BtnCreditor.Enabled = true;
        }
    }
}

