using System;
using System.Diagnostics;

namespace Centralizador.Models
{
    public class ProgressReportModel
    {
        public int PercentageComplete { get; set; }
        public string Message { get; set; }
        private static bool IsRuning { get; set; }
        public Stopwatch StopWatch { get; set; }
        private static TipoTask TaskType { get; set; }
        public DateTime? FchOutlook { get; set; }

        public ProgressReportModel(TipoTask taskType)
        {
            TaskType = taskType;
            IsRuning = true;
            PercentageComplete = 0;
            if (TaskType == TipoTask.GetCreditor || TaskType == TipoTask.GetCreditor)
            {
                StopWatch = Stopwatch.StartNew();
            }
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
                return IsRuning;  // SERA MEJOR LLAMAR DIRECTAMENTE A LOS CAMPOS STATICOS PUBLICOS?
            }
        }

        public static void SetStateReport(bool sett)
        {
            IsRuning = sett;
        }

        public void SetPorcent(int c)
        {
            PercentageComplete += c;
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