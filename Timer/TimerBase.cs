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

        public void Start()
        {
            StartTime = DateTime.Now;
        }

        public string DurationMs()
        {
            return (DateTime.Now - StartTime).Milliseconds.ToString();
        }

        public string DurationS()
        {
            return (DateTime.Now - StartTime).Seconds.ToString();
        }
    }
}
