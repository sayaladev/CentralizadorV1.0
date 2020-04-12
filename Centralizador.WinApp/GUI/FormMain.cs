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

namespace Centralizador.WinApp.GUI
{

    public partial class FormMain : Form
    {
        #region Global variables/prop



        //Creditor     
        private IList<Detalle> DetallesCreditor { get; set; }
        private BackgroundWorker BgwCreditor { get; set; }

        //Debitor
        private IList<Detalle> DetallesDebtor { get; set; }

        private BackgroundWorker BgwDebtor { get; set; }

        //General
        private ResultParticipant UserParticipant { get; set; }

        private readonly CultureInfo CultureInfo = CultureInfo.GetCultureInfo("es-CL");
        private IEnumerable<ResultBilingType> BillingTypes { get; set; }

        public string TokenSii { get; set; }

        private bool IsCreditor { get; set; }


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
            BgwDebtor = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            BgwDebtor.ProgressChanged += BgwDebtor_ProgressChanged;
            BgwDebtor.RunWorkerCompleted += BgwDebtor_RunWorkerCompleted;
            BgwDebtor.DoWork += BgwDebtor_DoWork;
            BgwCreditor = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            BgwCreditor.ProgressChanged += BgwCreditor_ProgressChanged;
            BgwCreditor.RunWorkerCompleted += BgwCreditor_RunWorkerCompleted;
            BgwCreditor.DoWork += BgwCreditor_DoWork;

            // Token
            TssLblTokenSii.Text = TokenSii;

            // Date Time Outlook
            TxtDateTimeEmail.Text = string.Format(CultureInfo, "{0:g}", Models.Properties.Settings.Default.DateTimeEmail);



        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            // frozen column will display row numbers.
            IGridMain.FrozenArea.ColCount = 3;
            IGridMain.FrozenArea.RowCount = 0;
            IGridMain.FrozenArea.ColsEdge = new iGPenStyle(SystemColors.ControlDark, 2, DashStyle.Solid);
            IGridMain.FrozenArea.RowsEdge = new iGPenStyle(SystemColors.ControlDark, 2, DashStyle.Solid);

            // Set up the deafult parameters of the frozen columns.
            IGridMain.DefaultCol.AllowMoving = false;
            IGridMain.DefaultCol.IncludeInSelect = false;
            //IGridMain.DefaultCol.AllowSizing = false;

            // Set up the width of the first frozen column (hot and current row indicator).
            IGridMain.DefaultCol.Width = 10;
            // Add the first frozen column.
            IGridMain.Cols.Add().CellStyle.CustomDrawFlags = iGCustomDrawFlags.Foreground;
            // Set up the width of the second frozen column (row numbers).
            IGridMain.DefaultCol.Width = 30;
            // Add the second frozen column.
            IGridMain.Cols.Add().CellStyle.CustomDrawFlags = iGCustomDrawFlags.Foreground;

            // Add data columns.
            iGColPattern pattern = new iGColPattern();
            pattern.ColHdrStyle.TextAlign = iGContentAlignment.MiddleCenter;
            pattern.ColHdrStyle.Font = new Font("Verdana", 6.5f, FontStyle.Bold);

            iGCellStyle colCurrency = new iGCellStyle
            {
                TextAlign = iGContentAlignment.MiddleRight,
                FormatString = "{0:#,##}"
            };


            IGridMain.Cols.Add("Instruction", 55, pattern);
            IGridMain.Cols.Add("Code", 40, pattern);
            IGridMain.Cols.Add("P1", 25, pattern);
            IGridMain.Cols.Add("P2", 25, pattern);
            IGridMain.Cols.Add("P3", 25, pattern);
            IGridMain.Cols.Add("P4", 25, pattern);
            IGridMain.Cols.Add("Rut", 60, pattern);
            IGridMain.Cols.Add("Dv", 15, pattern);
            IGridMain.Cols.Add("Amount", 65, pattern);
            IGridMain.Cols[10].CellStyle = colCurrency;
            IGridMain.Cols.Add("Exent", 65, pattern);
            IGridMain.Cols[11].CellStyle = colCurrency;
            IGridMain.Cols.Add("Iva", 65, pattern);
            IGridMain.Cols[12].CellStyle = colCurrency;
            IGridMain.Cols.Add("Total", 65, pattern);
            IGridMain.Cols[13].CellStyle = colCurrency;
            IGridMain.Cols.Add("Folio", 70, pattern);
            IGridMain.Cols[14].CellStyle = new iGCellStyle { TextAlign = iGContentAlignment.MiddleCenter };
            IGridMain.Cols.Add("Emission", 65, pattern);
            //IGridMain.Cols[15].CellStyle = new iGCellStyle { FormatString = "{0:d}" };
            IGridMain.Cols.Add("Cbte.", 70, pattern);
            IGridMain.Cols.Add("F. de pago", 65, pattern);
            IGridMain.Cols.Add("pic", 25, pattern);
            IGridMain.Cols.Add("F. de envío", 65, pattern);
            IGridMain.Cols.Add("flag", 25, pattern);
            IGridMain.Cols.Add("status", 70, pattern);
            IGridMain.Cols.Add("F. de envío", 65, pattern);
            IGridMain.Cols.Add("M", 25, pattern);
            IGridMain.Cols.Add("Aceptado cliente", 70, pattern);

            // General options
            IGridMain.GroupBox.Visible = true;
            IGridMain.RowMode = true;
            IGridMain.ReadOnly = true;
            // Set up the default parameters of the data columns.
            IGridMain.DefaultCol.AllowMoving = true;
            //fGrid.DefaultCol.IncludeInSelect = true;
            IGridMain.DefaultCol.AllowSizing = true;

            // Footer
            IGridMain.Footer.Visible = true;
            //IGridMain.Footer.Rows.Count = 1;
            //IGridMain.Footer.AutoHeightEvents = iGAutoHeightEvents.None;
            //IGridMain.Footer.Rows[0].Height = 30;
            //IGridMain.Footer.Cells[0, 3].SpanCols = 10;
            //IGridMain.Footer.Cells[0, 3].TextAlign = iGContentAlignment.MiddleLeft;
            //IGridMain.Footer.Cells[0, 3].Font = new Font("Calibri", 8f, FontStyle.Italic);
            //IGridMain.Footer.Cells[0, 13].SpanCols = 12;
            //IGridMain.Footer.Cells[0, 13].TextAlign = iGContentAlignment.MiddleLeft;
            //IGridMain.Footer.Cells[0, 13].Font = new Font("Calibri", 8f, FontStyle.Italic);

            // Footer freezer section
            IGridMain.Footer.Cells[0, 0].SpanCols = 3;
            IGridMain.Footer.Cells[0, 3].SpanCols = 7;
            IGridMain.Footer.Cells[0, 14].SpanCols = 11;

            // Footer totals
            IGridMain.Footer.Cells[0, 10].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[0, 11].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[0, 12].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[0, 13].AggregateFunction = iGAggregateFunction.Sum;
            IGridMain.Footer.Cells[0, 10].TextAlign = iGContentAlignment.MiddleRight;
            IGridMain.Footer.Cells[0, 11].TextAlign = iGContentAlignment.MiddleRight;
            IGridMain.Footer.Cells[0, 12].TextAlign = iGContentAlignment.MiddleRight;
            IGridMain.Footer.Cells[0, 13].TextAlign = iGContentAlignment.MiddleRight;



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
            else if (BgwCreditor.IsBusy == true)
            {
                TssLblMensaje.Text = "Is busy!";
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
            IList<ResultPaymentMatrix> matrices = (IList<ResultPaymentMatrix>)e.Argument;
            DetallesCreditor = new List<Detalle>();
            List<ResultInstruction> instructions = new List<ResultInstruction>();
            int c = 0;
            float porcent = 0;
            foreach (ResultPaymentMatrix m in matrices)
            {
                //Tester
                //if (m.Id != 623)
                //{
                //    continue;
                //}
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
                if (instruction.Id == 1758038)
                {
                    MessageBox.Show("Test");
                }

                Detalle detalle = new Detalle
                {
                    Instruction = instruction,
                    MntNeto = instruction.Amount
                };
                detalle.Instruction.ParticipantDebtor = Participant.GetParticipantById(instruction.Debtor);
                detalle.RutReceptor = instruction.ParticipantDebtor.Rut;
                detalle.DvReceptor = instruction.ParticipantDebtor.VerificationCode;
                detalle.RznSocRecep = instruction.ParticipantDebtor.BusinessName;

                // Mapping references Softland
                IList<Reference> references = Reference.GetInfoFactura(instruction);
                if (references != null)
                {
                    // Search reference mostly recent for number folio
                    Reference reference = references.OrderByDescending(x => x.Folio).First();

                    detalle.DTEDef = ServicePdf.TransformXmlStringToObjectDTE(reference.FileBasico);
                    detalle.Folio = reference.Folio;

                    if (reference.FechaRecepcionSii != null)
                    {
                        detalle.FechaRecepcion = reference.FechaRecepcionSii.ToString();
                    }
                    if (reference.FechaEmision != null)
                    {
                        detalle.FechaEmision = reference.FechaEmision.ToString();
                    }
                    // Mapping $
                    detalle.MntIva = Convert.ToUInt32(detalle.Instruction.Amount * 0.19);
                    detalle.MntTotal = Convert.ToUInt32(detalle.Instruction.Amount * 1.19);
                    detalle.References = reference;
                }

                DetallesCreditor.Add(detalle);
                c++;
                porcent = (float)(100 * c) / instructions.Count;
                BgwCreditor.ReportProgress((int)porcent, $"Getting info from Softland...   ({c}/{instructions.Count})");
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
            IGridFill(DetallesCreditor);
            TssLblProgBar.Value = 0;
            IsCreditor = true;
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
            else if (BgwDebtor.IsBusy == true)
            {
                TssLblMensaje.Text = "Is busy!";
                return;
            }
            DetallesDebtor = new List<Detalle>();
            DetallesDebtor = ServiceLibro.GetLibro("Debtor", UserParticipant, "33", $"{CboYears.SelectedItem}-{string.Format("{0:00}", CboMonths.SelectedIndex + 1)}", TokenSii);
            string nameFile = "";
            nameFile += $"{Directory.GetCurrentDirectory()}\\inbox\\{CboYears.SelectedItem}\\{CboMonths.SelectedIndex + 1}";
            if (DetallesDebtor != null)
            {

                BgwDebtor.RunWorkerAsync(nameFile);
            }
        }

        private void BgwDebtor_DoWork(object sender, DoWorkEventArgs e)
        {
            string nameFilePath = e.Argument.ToString();
            string nameFile = "";
            int c = 0;
            float porcent = 0;
            foreach (Detalle item in DetallesDebtor)
            {
                nameFile = nameFilePath + $"\\{UserParticipant.Rut}-{UserParticipant.VerificationCode}\\{item.RutReceptor}-{item.DvReceptor}__{item.Folio}.xml";
                if (File.Exists(nameFile))
                {   // Deserialize  
                    DTEDefType xmlObjeto = ServicePdf.TransformXmlToObjectDTE(nameFile);
                    item.DTEDef = xmlObjeto;
                    DTEDefTypeDocumento dte = (DTEDefTypeDocumento)xmlObjeto.Item;
                    DTEDefTypeDocumentoReferencia[] referencias = dte.Referencia;
                    if (referencias != null)
                    {
                        foreach (DTEDefTypeDocumentoReferencia r in referencias)
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
                                            ResultParticipant participant = Participant.GetParticipantByRut(dte.Encabezado.Emisor.RUTEmisor.Split('-').GetValue(0).ToString());
                                            item.Instruction = Instruction.GetInstructionDebtor(m, participant, UserParticipant);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                c++;
                porcent = (float)(100 * c) / DetallesDebtor.Count;
                BgwDebtor.ReportProgress((int)porcent, $"Getting info from Softland...   ({c}/{DetallesDebtor.Count})");
                Thread.Sleep(100);
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
            IGridFill(DetallesDebtor);
            TssLblProgBar.Value = 0;
            IsCreditor = false;
        }

        #endregion

        private void IGridFill(IList<Detalle> detalles)
        {
            try
            {
                IGridMain.BeginUpdate();
                // Fill up the cells with data.
                IGridMain.Rows.Clear();
                // Set up the height of the first row (frozen).

                iGRow myRow;
                DateTime? fechaRecepcion = null;
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

                    if (item.FechaEmision != null)
                    {
                        DateTime FechaEmision = Convert.ToDateTime(item.FechaEmision, CultureInfo);
                        myRow.Cells[15].Value = string.Format(CultureInfo, "{0:d}", FechaEmision);
                    }




                    if (item.FechaRecepcion != null)
                    {
                        fechaRecepcion = Convert.ToDateTime(item.FechaRecepcion, CultureInfo);
                        myRow.Cells[19].Value = string.Format(CultureInfo, "{0:d}", fechaRecepcion);
                    }

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
                            myRow.Cells[21].Value = TypeEvent.AcuseRecibo;
                            break;
                        case 5:
                            TimeSpan len = DateTime.Today.Subtract((DateTime)fechaRecepcion);
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
            TxtRznSocial.Text = "";
            TxtFmaPago.Text = "";
        }

        #region IGridMain methods



        private void IGridMain_CustomDrawCellForeground(object sender, iGCustomDrawCellEventArgs e)
        {
            iGCell fCurCell = IGridMain.CurCell;

            if (e.ColIndex == 1)
            {
                //Draw the row numbers.
                e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), Font, SystemBrushes.ControlText, new Rectangle(e.Bounds.X + 2, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height));
            }
            else if (e.ColIndex == 0)
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
                //else if (e.RowIndex == -1)
                //{
                //    myBrush = Brushes.Red;
                //}

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
            if (e.ColIndex == 0 || e.ColIndex == 1)
            {
                e.DoDefault = false;
            }
        }
        private void IGridMain_CurRowChanged(object sender, EventArgs e)
        {
            if (BgwDebtor.IsBusy == true || BgwCreditor.IsBusy == true)
            {
                return;
            }
            iGRow gRow = IGridMain.CurRow;
            uint rut = Convert.ToUInt32(gRow.Cells[8].Value);
            uint folio = Convert.ToUInt32(gRow.Cells[14].Value);
            uint instruction = Convert.ToUInt32(gRow.Cells[2].Value);
            CleanControls();
            Detalle detalle;
            if (folio > 0)
            {
                if (IsCreditor)
                {
                    detalle = DetallesCreditor.First(x => x.Folio == folio && x.RutReceptor == rut);
                }
                else
                {
                    detalle = DetallesDebtor.First(x => x.Folio == folio && x.RutReceptor == rut);
                }
                try
                {
                    TxtRznSocial.Text = detalle.RznSocRecep;

                    if (detalle.DTEDef != null)
                    {
                        DTEDefTypeDocumento dte = (DTEDefTypeDocumento)detalle.DTEDef.Item;
                        DTEDefTypeDocumentoDetalle[] detalles = dte.Detalle;
                        TxtFmaPago.Text = dte.Encabezado.IdDoc.FmaPago.ToString();
                        foreach (DTEDefTypeDocumentoDetalle detailProd in detalles)
                        {
                            TxtNmbItem.Text += "+" + detailProd.NmbItem.ToLowerInvariant() + Environment.NewLine;
                        }
                        if (dte.Referencia != null)
                        {
                            DTEDefTypeDocumentoReferencia referencia = dte.Referencia.FirstOrDefault(x => x.TpoDocRef == "SEN");
                            if (referencia != null)
                            {
                                TxtFolioRef.Text = referencia.FolioRef;
                                TxtRznRef.Text = referencia.RazonRef;
                            }
                            else
                            {
                                TssLblMensaje.Text = $"This ivoice ({folio}) does not have 'references CEN'.";
                            }

                        }
                        else
                        {
                            TssLblMensaje.Text = $"This ivoice ({folio}) does not have 'references CEN'.";
                        }
                    }
                    else
                    {
                        if (IsCreditor)
                        {
                            TssLblMensaje.Text = $"This invoice ({folio}) has not been sent to Sii.";
                        }
                        else
                        {
                            TssLblMensaje.Text = $"This invoice ({folio}) does not have your XMl file received.";
                        }

                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                TssLblMensaje.Text = $"This instruction ({instruction}) has not been billed yet.";

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
                DialogResult resp = MessageBox.Show($"Se van a insertar las siguientes NV:{Environment.NewLine} Folio 5478  a 5498 {Environment.NewLine} ¿Quiere continuar?", "Insert NV to Softland", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (resp == DialogResult.OK)
                {
                    //UserParticipant = (ResultParticipant)CboParticipants.SelectedItem;

                    DialogResult b = MessageBox.Show("Are you sure?", "Insert NV to Softland", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                }


                // Check aux (comuna, giro, dte, )



                // Insert NV
                // Check if exists?






                // Insert Ref

            }
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

        #region Outlook


        /// <summary>
        /// Method download & read email from account of CVE company.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOutlook_Click(object sender, EventArgs e)
        {
            ServiceOutlook outlook = new ServiceOutlook(TokenSii);
            BackgroundWorker bgwReadEmail = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            bgwReadEmail.ProgressChanged += BgwReadEmail_ProgressChanged;
            bgwReadEmail.RunWorkerCompleted += BgwReadEmail_RunWorkerCompleted;
            if (bgwReadEmail.IsBusy == false)
            {
                outlook.GetXmlFromEmail(bgwReadEmail);
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
            IGridMain.Focus();

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

        private void IGridMain_CellDoubleClick(object sender, iGCellDoubleClickEventArgs e)
        {
            if (BgwDebtor.IsBusy == true || BgwCreditor.IsBusy == true)
            {
                return;
            }
            iGRow gRow = IGridMain.CurRow;
            uint rut = Convert.ToUInt32(gRow.Cells[8].Value);
            uint folio = Convert.ToUInt32(gRow.Cells[14].Value);
            Detalle detalle;

            if (IsCreditor)
            {
                detalle = DetallesCreditor.First(x => x.Folio == folio && x.RutReceptor == rut);
            }
            else
            {
                detalle = DetallesDebtor.First(x => x.Folio == folio && x.RutReceptor == rut);
            }

            detalle = ServicePdf.GetPdfDocument(detalle);


            //System.Diagnostics.Process.Start(detalle.PdfDocument);

        }
    }
}

