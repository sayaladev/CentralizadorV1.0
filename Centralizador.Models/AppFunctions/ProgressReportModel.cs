using System;
using System.Diagnostics;

namespace Centralizador.Models
{
    public class ProgressReportModel
    {
        public int PercentageComplete { get; set; }

        private string message;

        public string GetMessage()
        {
            return message;
        }

        public void SetMessage(string value)
        {
            message = value;
        }

        private static bool isRuning;

        public static bool GetIsRuning()
        {
            return isRuning;
        }

        public static void SetIsRuning(bool value)
        {
            isRuning = value;
        }

        public Stopwatch StopWatch { get; set; }
        private static TipoTask TaskType { get; set; }
        public DateTime? FchOutlook { get; set; }

        public ProgressReportModel(TipoTask taskType)
        {
            TaskType = taskType;
            SetIsRuning(true);
            PercentageComplete = 0;
            if (TaskType == TipoTask.GetCreditor || TaskType == TipoTask.GetDebtor || taskType == TipoTask.ConvertToPdf)
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

        public static TipoTask GetTypeReport
        {
            get
            {
                return TaskType;
            }
        }
    }
}