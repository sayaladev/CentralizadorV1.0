using System;
using System.Diagnostics;

namespace Centralizador.Models
{
    public class ProgressReportModel
    {
        public int PercentageComplete { get; set; } = 0;
        public string Message { get; set; }
        private static bool IsRuning { get; set; }
        public Stopwatch StopWatch { get; set; }
        private static TipoTask TaskType { get; set; }
        public DateTime? FchOutlook { get; set; }

        public ProgressReportModel(TipoTask taskType)
        {
            TaskType = taskType;
            IsRuning = true;
        }

        public enum TipoTask
        {
            GetDebtor,
            GetCreditor,
            InsertNV,
            ConvertToPdf,
            ReadEmail,
            SendEmail
        }

        public static bool GetStateReport
        {
            get
            {
                return IsRuning;
            }
        }

        public static void SetStateReport(bool sett)
        {
            IsRuning = sett;
        }

        public static TipoTask GetTypeReport
        {
            get
            {
                return TaskType;
            }
        }
    }
}