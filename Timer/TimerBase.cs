using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocationAnalyzer.Timer
{
    public class TimerBase
    {
        public TimerBase()
        {

        }

        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }

        public void Start()
        {
            StartTime = DateTime.Now;
        }

        public void Finish()
        {
            FinishTime = DateTime.Now;
        }

        public string DurationMs()
        {
            return (FinishTime - StartTime).Milliseconds.ToString();
        }

        public string DurationS()
        {
            return (FinishTime - StartTime).Seconds.ToString();
        }
    }
}
