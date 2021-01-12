using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Centralizador.Models.ApiCEN;
using Centralizador.Models.ApiSII;
using Centralizador.Models.DataBase;

namespace Centralizador.Models.Interfaces
{
    public abstract class CveStoreDoc
    {
        public TipoTask TaskType { get; set; }
        public ResultParticipant UserParticipant { get; set; }
        public ProgressReportModel PgModel { get; set; }
        public IProgress<ProgressReportModel> Progress { get; set; }
        public string DataBaseName { get; set; }
        public static CancellationTokenSource CancelToken { get; set; } = new CancellationTokenSource();

        public enum TipoTask
        {
            Debtor,
            Creditor
        }

        protected CveStoreDoc(TipoTask taskType, ResultParticipant userParticipant, ProgressReportModel pgModel, IProgress<ProgressReportModel> progress, string dataBaseName)
        {
            TaskType = taskType;
            UserParticipant = userParticipant;
            PgModel = pgModel;
            Progress = progress;
            DataBaseName = dataBaseName;
        }

        //protected CveStoreDoc(ResultParticipant userParticipant, ProgressReportModel pgModel, IProgress<ProgressReportModel> progress, string dataBaseName)
        //{
        //    UserParticipant = userParticipant;
        //    PgModel = pgModel;
        //    Progress = progress;
        //    DataBaseName = dataBaseName;
        //}

        protected CveStoreDoc()
        {
        }

        public void CancelTask()
        {
            if (CancelToken.IsCancellationRequested)
            {
                CancelToken.Cancel();
            }
        }

        public Task ReportProgress(float p, string msg)
        {
            return Task.Run(() =>
            {
                PgModel.PercentageComplete = (int)p;
                PgModel.SetMessage(msg);
                Progress.Report(PgModel);
            });
        }

        public abstract Task<List<Detalle>> GetDocFromStore(List<ResultInstruction> list, string tokenSii, string tokenCen);

        public async void DeleteNV()
        {
            await NotaVenta.DeleteNvAsync(new Conexion(DataBaseName));
        }
    }
}