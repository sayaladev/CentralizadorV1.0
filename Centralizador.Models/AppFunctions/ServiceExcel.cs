using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;

using Spire.Xls;

using static Centralizador.Models.ApiSII.ServiceDetalle;

namespace Centralizador.Models.AppFunctions
{
    public class ServiceExcel
    {

        private IList<Detalle> Detalles { get; set; }
        private ResultParticipant UserParticipant { get; set; }

        public ServiceExcel(IList<Detalle> detalles, ResultParticipant userParticipant)
        {
            Detalles = detalles;
            UserParticipant = userParticipant;
        }

        public void CreateNomina(BackgroundWorker BgwPay)
        {
            BgwPay.DoWork += BgwPay_DoWork;
            BgwPay.RunWorkerAsync();

        }

        private void BgwPay_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker BgwPay = sender as BackgroundWorker;
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            int c = 0;
            // https://ww4.indexa.cl/foindexaabonos/fotefbm/AyudaExcel.asp

            DataTable table = new DataTable();
            table.Columns.Add("1"); //2
            table.Columns.Add("2"); //rut sin guión 
            table.Columns.Add("3"); //razón social
            table.Columns.Add("4"); //nro cta
            table.Columns.Add("5"); //monto
            table.Columns.Add("6"); //1:cta cte - 2:vista - 3:ahorro
            table.Columns.Add("7"); //código banco
            table.Columns.Add("8");  //email

            foreach (Detalle item in Detalles)
            {
                try
                {                   
                        // Security Bank Txt
                        DataRow row = table.NewRow();
                        row["1"] = "2";
                        row["2"] = item.RutReceptor; // Rut sin guión
                        row["3"] = ti.ToTitleCase(item.RznSocRecep.ToLower());  // Rzn Social                     
                        if (item.Instruction != null && item.Instruction.ParticipantCreditor != null)
                        {
                            row["4"] = item.Instruction.ParticipantCreditor.BankAccount.TrimStart(new char[] { '0' }).Replace("-", ""); // Nro 
                        }

                        row["5"] = item.MntTotal;
                        row["6"] = "1";
                        if (item.Instruction != null && item.Instruction.ParticipantCreditor != null)
                        {
                            switch (item.Instruction.ParticipantCreditor.Bank)
                            {
                                case 1: // Chile
                                    row["7"] = "1";
                                    break;
                                case 2: // International
                                    row["7"] = "9";
                                    break;
                                case 3: // Scotiabank
                                    row["7"] = "14";
                                    break;
                                case 4: // Bci
                                    row["7"] = "16";
                                    break;
                                case 5: // Bice
                                    row["7"] = "28";
                                    break;
                                case 6: // Hsbc
                                    row["7"] = "31";
                                    break;
                                case 7: // Santander
                                    row["7"] = "37";
                                    break;
                                case 8: // Itau/CorpBanca
                                    row["7"] = "39"; // Itau
                                    break;
                                case 9: // Security
                                    row["7"] = "49";
                                    break;
                                case 10: // Falabella
                                    row["7"] = "51";
                                    break;
                                case 11: // Ripley
                                    row["7"] = "53";
                                    break;
                                case 12: // Rabobank
                                    row["7"] = "54";
                                    break;
                                case 13: // Concorcio
                                    row["7"] = "55";
                                    break;
                                case 16: // Bbva
                                    row["7"] = "504";
                                    break;
                                case 18: // Estado
                                    row["7"] = "12";
                                    break;

                                default:
                                    row["7"] = "";
                                    break;
                            }

                            if (item.Instruction != null && item.Instruction.ParticipantCreditor.BillsContact != null && item.Instruction.ParticipantCreditor.BillsContact.Email != null)
                            {
                                row["8"] = item.Instruction.ParticipantCreditor.BillsContact.Email;
                            }
                            else
                            {
                                if (item.Instruction != null && item.Instruction.ParticipantCreditor.BillsContact != null && item.Instruction.ParticipantCreditor.PaymentsContact.Email != null)
                                {
                                    row["8"] = item.Instruction.ParticipantCreditor.PaymentsContact.Email;
                                }
                            }
                        }                        
                        table.Rows.Add(row);
                        // Insert CEN
                        //item.Instruction.StatusPaid = 2;
                        item.Instruction.IsPaid = true;

                        // Insert Softland
                    
                }
                catch (Exception)
                {
                    throw;
                }  
                c++;
                float porcent = (float)(100 * c) / Detalles.Count;
                BgwPay.ReportProgress((int)porcent, $"Generating file, please wait. ({c}/{Detalles.Count})");               
            }

            // Save Excel
            if (table.Rows.Count > 0)
            {
                Workbook workbook = new Workbook();
                Worksheet worksheet = workbook.Worksheets[0];
                worksheet.InsertDataTable(table, false, 1, 1);
                string nameFile = $"{UserParticipant.Name}_NominaBank_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}" + ".xlsx";

                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\log\"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\log\");
                }
                workbook.SaveToFile(Directory.GetCurrentDirectory() + @"\log\" + nameFile, FileFormat.Version2016);
                ProcessStartInfo process = new ProcessStartInfo(Directory.GetCurrentDirectory() + @"\log\" + nameFile)
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                Process.Start(process);
                e.Result = table;
            }

        }
    }
}
