using System;
using System.Diagnostics;

namespace Centralizador.Models
{
    public class HPgModel
    {
        public int PercentageComplete { get; set; }
        public string Msg { get; set; }
        public bool IsBussy { get; set; }
        public Stopwatch StopWatch { get; set; }

        // public TipoTask TaskType { get; set; }
        public DateTime? FchOutlook { get; set; }

        //public HPgModel(TipoTask taskType)
        //{
        //    TaskType = taskType;
        //    IsBussy = true;
        //    PercentageComplete = 0;
        //    //if (TaskType == TipoTask.GetCreditor || TaskType == TipoTask.GetDebtor || taskType == TipoTask.ConvertToPdf)
        //    //{
        //    //    StopWatch = Stopwatch.StartNew();
        //    //}
        //}

        public HPgModel()
        {
            PercentageComplete = 0;
            StopWatch = new Stopwatch();
        }

        //public enum TipoTask
        //{
        //    GetDebtor,
        //    GetCreditor,
        //    InsertNV,
        //    ConvertToPdf,
        //    ReadEmail,
        //    SendEmail
        //}
    }
}