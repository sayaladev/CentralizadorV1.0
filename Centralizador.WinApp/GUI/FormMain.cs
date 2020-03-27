using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.DataBase;

using TenTec.Windows.iGridLib;

namespace Centralizador.WinApp.GUI
{

    public partial class FormMain : Form
    {
        private ResultParticipant userParticipant;
        private InstructionsList instructionsList;
        private CultureInfo info = CultureInfo.GetCultureInfo("es-CL");

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
                participants.Add(Participant.GetParticipantById(item));
            }
            CboParticipants.DisplayMember = "Name";
            CboParticipants.DataSource = participants;
            CboParticipants.SelectedIndex = -1;

            //Load ComboBox certificates
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            CboCerts.DisplayMember = "FriendlyName";
            CboCerts.DataSource = store.Certificates;
            store.Close();
            //Load ComboBox months
            CboMonths.DataSource = DateTimeFormatInfo.InvariantInfo.MonthNames.Take(12).ToList();
            CboMonths.SelectedIndex = DateTime.Today.Month - 1;
            //Load ComboBox years
            CboYears.DataSource = Enumerable.Range(2019, 2).ToList();
            CboYears.SelectedItem = DateTime.Today.Year;



        }

        private void BtnCreditor_Click(object sender, EventArgs e)
        {
            GetData("Deudor");
            BtnCreditor.Enabled = false;

        }

        #region IGridMain
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
        

        private void BackgroundW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            DateTime date = new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1);
            string tipoLista = e.Argument.ToString();

            IList<ResultInstruction> instructionsList = new List<ResultInstruction>();
            try
            {
                // Get list of payments matrix              
                IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrix(date);
                foreach (ResultPaymentMatrix matrix in matrices)
                {
                    // ONLY TESTER
                    if (matrix.Id == 680)
                    {
                        int i = 0;
                        BackgroundW.ReportProgress(0, "");
                        // Get list of instructions
                        IList<ResultInstruction> instructions;
                        instructions = Instruction.GetInstructions(matrix, userParticipant);
                        if (instructions != null)
                        {
                            foreach (ResultInstruction instruction in instructions)
                            {
                                if (instruction.Status == "Publicado")
                                {
                                    // Mapping matrix from CEN
                                    instruction.PaymentMatrixM = matrix;
                                    // Mapping participant of dte from CEN 
                                    ResultParticipant participant = new ResultParticipant(instruction.Debtor);
                                    instruction.ParticipantM = Participant.GetParticipantById(participant);

                                    // Mapping dte from Softland (only creditor)
                                    Softland softland = new Softland
                                    {
                                        IdInstruction = instruction.Id,
                                        Reference = Reference.GetReferenceByGlosa(instruction).ToList()

                                    };
                                    //instruction.ResultDteMapping = DBReference.GetReferenceByGlosa(instruction);

                                    // Mapping Dte from CEN
                                    //if (instruction.StatusBilled == 2) // 2 significa que fue enviado paso 1 y 2
                                    //{
                                    //    ResultDte dte = Dte.GetDte(instruction);
                                    //    //instruction.ResultDteMapping = dte;
                                    //}

                                    // Mapping status send Sii from Softland 
                                    //if (instruction.DBSendSiiMapping != null)
                                    //{
                                    //    instruction.DBSendSiiMapping = DBSendSii.GetSendSiiByFolio(resultParticipant, instruction);
                                    //}



                                    instructionsList.Add(instruction);

                                }
                                i++;
                                BackgroundW.ReportProgress(100 * i / instructions.Count, $"Working...in: {matrix.NaturalKey}");
                            }
                        }
                    }
                }
                BackgroundW.ReportProgress(100, "Complete!");
                switch (tipoLista)
                {
                    case "Deudor":
                        this.instructionsList.InstructionDebitor = instructionsList;
                        break;
                    case "Acreedor":
                        this.instructionsList.InstructionCreditor = instructionsList;
                        break;
                }


            }

            catch (Exception)
            {

            }
            finally
            {

            }
            // Main Grid
            IGridMain.BeginUpdate();
            try
            {
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
                IGridMain.Cols.Add("Rut");
                IGridMain.Cols.Add("dv");
                IGridMain.Cols.Add("Amount");
                IGridMain.Cols.Add("Folio");
                IGridMain.Cols.Add("Fecha");
                IGridMain.Cols.Add("Paso 1");
                IGridMain.Cols.Add("Paso 2");
                IGridMain.Cols.Add("Paso 3");
                IGridMain.Cols.Add("Paso 4");
                IGridMain.Cols.Add("Enviado sii");
                IGridMain.Cols.Add("Aceptado sii");
                IGridMain.Cols.Add("Enviado cliente");
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
                foreach (ResultInstruction item in instructionsList)
                {
                    // Add rows.                    
                    myRow = IGridMain.Rows.Add();
                    myRow.Cells[2].Value = item.Id;
                    myRow.Cells[3].Value = item.ParticipantM.Rut;
                    myRow.Cells[4].Value = item.ParticipantM.VerificationCode;
                    myRow.Cells[5].Value = string.Format(info, "{0:C0}", item.Amount);


                    myRow.Cells[8].Value = item.StatusBilled;
                    myRow.Cells[9].Value = item.StatusBilled;
                    myRow.Cells[10].Value = "p3";
                    myRow.Cells[11].Value = "p4";
                    //if (item.DBSendSiiMapping != null)
                    //{
                    //    myRow.Cells[6].Value = item.ResultDteMapping.Folio;
                    //    myRow.Cells[7].Value = string.Format("{0:dd/MM/yyyy}", item.ResultDteMapping.EmissionDt);
                    //    myRow.Cells[12].Value = item.DBSendSiiMapping.EnviadoSII;
                    //    myRow.Cells[13].Value = item.DBSendSiiMapping.AceptadoSII;
                    //    myRow.Cells[14].Value = item.DBSendSiiMapping.EnviadoCliente;
                    //    myRow.Cells[15].Value = item.DBSendSiiMapping.AceptadoCliente;
                    //}



                }

                // Fit the columns' width.
                IGridMain.Cols.AutoWidth();

            }

            catch (Exception)
            {

                throw;
            }


            finally
            {

            }

        }

        private void BackgroundW_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            TssLblProgBar.Value = e.ProgressPercentage;
            TssLblMensaje.Text = e.UserState.ToString();
        }

        private void BackgroundW_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            IGridMain.EndUpdate();
            IGridMain.Focus();
            TssLblProgBar.Value = 0;
            BtnCreditor.Enabled = true;
        }

        private void CboParticipants_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            userParticipant = (ResultParticipant)comboBox.SelectedItem;
            //Get Data
            instructionsList = null;

            GetData("Deudor");
            BtnCreditor.Enabled = false;

        }


        private void GetData(string tipoLista)
        {
            switch (tipoLista)
            {
                case "Deudor":
                    BackgroundW.RunWorkerAsync("Deudor");
                    break;
                case "Acreedor":
                    BackgroundW.RunWorkerAsync("Acreedor");
                    break;
            }

        }
    }
}

