using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.DataBase;
using Centralizador.Models.Helpers;

namespace Centralizador.Models
{
    public class Cve : CveStore
    {
        public TipoTask Mode { get; set; }
        public List<int> FoliosNv { get; set; }

        // CTOR FROM PARENT.
        public Cve(ResultParticipant userParticipant, IProgress<HPgModel> progress, string dataBaseName, string tokenSii, string tokenCen)
           : base(userParticipant, progress, dataBaseName, tokenSii, tokenCen)
        {
        }

        // OVERRIDE (ACTIONS)
        public override async Task GetDocFromStore(DateTime period)
        {
            // INFO
            PgModel.StopWatch.Restart();
            PgModel.IsBussy = true;

            BilingTypes = await BilingType.GetBilinTypesAsync();
            switch (Mode)
            {
                case TipoTask.Creditor:
                    List<ResultPaymentMatrix> matrices = await PaymentMatrix.GetPaymentMatrixAsync(period);
                    if (matrices != null && matrices.Count > 0)
                    {
                        // DELETE NV.
                        DeleteNV();
                        // INSERT TRIGGER.
                        DteInfoRef.InsertTriggerRefCen(Conn);
                        List<ResultInstruction> i = await GetInstructions(matrices);
                        if (i.Count > 0)
                        {
                            try
                            {
                                DetalleList = await GetCreditor(i);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                    break;

                case TipoTask.Debtor:
                    string nameFile = @"C:\Centralizador\Inbox\" + period.Year + @"\" + period.Month;
                    string p = $"{period.Year}-{string.Format("{0:00}", period.Month)}";
                    List<Detalle> lista = await ServiceDetalle.GetLibroAsync("Debtor", UserParticipant, "33", p, TokenSii);
                    if (lista != null && lista.Count > 0)
                    {
                        try
                        {
                            DetalleList = await GetDebtor(lista, nameFile);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    break;
            }
            // INFO
            PgModel.StopWatch.Stop();
            PgModel.IsBussy = false;
        }

        public override async Task InsertNotaVenta()
        {
            // INFO
            PgModel.StopWatch.Restart();
            PgModel.IsBussy = true;

            // GET VALUES LIST FROM CSV.
            FileSii.ReadFileSii();
            FoliosNv = await InsertNv(DetalleList);
            if (FoliosNv != null && FoliosNv.Count > 0)
            {
                string nameFile = $"{UserParticipant.Name}_InsertNv_{DateTime.Now:dd-MM-yyyy-HH-mm-ss}";
                StringLogging.AppendLine("");
                StringLogging.AppendLine($"Summary: From {FoliosNv.Min()} To-{FoliosNv.Max()}");
                SaveLogging(@"C:\Centralizador\Log\", nameFile);
            }

            // INFO
            PgModel.StopWatch.Stop();
            PgModel.IsBussy = false;
        }
    }
}