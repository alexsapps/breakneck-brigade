using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class BBStopwatch
    {
        Stopwatch stopwatch;

        public BBStopwatch()
        {
            this.stopwatch = new Stopwatch();
        }

        public void Start()
        {
            stopwatch.Restart(); //reset elapsed time and start
        }

        public void Stop(int maxMs, string errorMsg)
        {
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > maxMs)
                Console.WriteLine(String.Format(errorMsg, stopwatch.ElapsedMilliseconds));
        }

        public void Pause()
        {
            stopwatch.Stop();
        }

        public void Resume()
        {
            stopwatch.Start(); //does not reset elapsed time
        }
    }
}
