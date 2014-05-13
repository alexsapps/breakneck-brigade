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
        BBConsole console;

        public BBStopwatch(BBConsole console)
        {
            this.stopwatch = new Stopwatch();
            this.console = console;
        }

        public void Start()
        {
            stopwatch.Restart(); //reset elapsed time and start
        }

        public void Stop(int maxMs, string errorMsg)
        {
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > maxMs)
                console.WriteLine(String.Format(errorMsg, stopwatch.ElapsedMilliseconds));
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
