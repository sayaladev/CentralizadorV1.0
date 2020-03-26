using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

using Centralizador.Models.ApiCEN;

using TenTec.Windows.iGridLib;

namespace Centralizador.WinApp.GUI
{

    public partial class FormMain : Form
    {
        private ResultParticipant ResultParticipant = new ResultParticipant();

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




        [STAThread]
        private void BtnCreditor_Click(object sender, EventArgs e)
        {
            DateTime date = new DateTime((int)CboYears.SelectedItem, CboMonths.SelectedIndex + 1, 1);


            BackgroundW.RunWorkerAsync(date);

        }

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


        private void BackgroundW_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            IList<ResultInstruction> instructionsL = new List<ResultInstruction>();
            try
            {
                DateTime date = (DateTime)e.Argument;

                // Get list of payments matrix              
                IList<ResultPaymentMatrix> matrices = PaymentMatrix.GetPaymentMatrix(date);
                foreach (ResultPaymentMatrix matrix in matrices)
                {
                    int i = 0;
                    BackgroundW.ReportProgress(0, "");
                    // Get list of instructions
                    IList<ResultInstruction> instructions;
                    instructions = Instruction.GetInstructions(matrix, ResultParticipant);
                    if (instructions != null)
                    {
                        foreach (ResultInstruction instruction in instructions)
                        {
                            if (instruction.Status == "Publicado")
                            {
                                // Mapping matrix
                                instruction.PaymentMatrixMapping = matrix;
                                //Mapping participant
                                ResultParticipant participant = new ResultParticipant
                                {
                                    ParticipantId = instruction.Debtor
                                };
                                instruction.ResultParticipantMapping = Participant.GetParticipantById(participant);
                                instructionsL.Add(instruction);
                            }
                            i++;
                            BackgroundW.ReportProgress(100 * i / instructions.Count, $"Working...in: {matrix.NaturalKey}");
                        }
                    }
                }
                BackgroundW.ReportProgress(100, "Complete!");
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
                foreach (ResultInstruction item in instructionsL)
                {
                    // Add rows.                    
                    myRow = IGridMain.Rows.Add();
                    myRow.Cells[2].Value = item.Id;
                    myRow.Cells[3].Value = item.ResultParticipantMapping.Rut;
                    myRow.Cells[4].Value = item.ResultParticipantMapping.VerificationCode;
                    myRow.Cells[5].Value = item.Amount;



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

        private void CboParticipants_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ResultParticipant = (ResultParticipant)comboBox.SelectedItem;
        }

        private void BackgroundW_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            IGridMain.EndUpdate();
            IGridMain.Focus();
            TssLblProgBar.Value = 0;
        }
    }
}

