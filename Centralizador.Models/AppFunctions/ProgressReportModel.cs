using System.Diagnostics;

namespace Centralizador.Models
{
    public class ProgressReportModel
    {
        public int PercentageComplete { get; set; } = 0;
        public string Message { get; set; }
        public bool IsRuning { get; set; }
        public Stopwatch StopWatch { get; set; }

        //public TipoDetalle DetalleType { get; set; }
        public TipoTask TaskType { get; set; }

        public ProgressReportModel(TipoTask taskType)
        {
            TaskType = taskType;
        }

        //public enum TipoDetalle
        //{
        //    Creditor,
        //    Debtor
        //}

        public enum TipoTask
        {
            GetDebtor,
            GetCreditor,
            InsertNV,
            ConvertToPdf
        }
    }
}