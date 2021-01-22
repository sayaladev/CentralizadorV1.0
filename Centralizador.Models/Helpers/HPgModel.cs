using System;
using System.Diagnostics;

namespace Centralizador.Models
{
    public class HPgModel
    {
        public int PercentageComplete { get; set; }
        public string Msg { get; set; }
        public Stopwatch StopWatch { get; set; }
        public DateTime? FchOutlook { get; set; }

        public HPgModel()
        {
            PercentageComplete = 0;
            StopWatch = new Stopwatch();
            StopWatch.Start();
        }
    }
}