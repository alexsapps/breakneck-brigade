using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeCuisine
{
    class Game : IDisposable
    {
        public BBLock Lock = new BBLock();

        public GameMode Mode { get; private set; }

        private Thread runThread;

        public Game()
        {
            Mode = GameMode.Init; //start in init mode
        }

        public void Start()
        {
            Lock.AssertHeld();

            if (Mode != GameMode.Init)
                throw new Exception("can't start from state " + Mode.ToString() + ".");

            runThread = new Thread(() => Run());
            runThread.Start();

            Mode = GameMode.Started;
        }

        public void Stop()
        {
            Lock.AssertHeld();

            if (Mode == GameMode.Stopping)
                throw new Exception("already stopping.");
            if (Mode == GameMode.Started || Mode == GameMode.Paused)
                runThread.Abort();
            
            Mode = GameMode.Stopping;
        }

        public void Run()
        {
            long start = DateTime.UtcNow.Ticks;
            long second = (new TimeSpan(0, 0, 0, 1)).Ticks;
            int seconds = 1; //TODO: read from config
            long rate = seconds * second;
            long next = start;

            while (true)
            {
                next += rate;
                lock (Lock)
                {

                }
                
                long waitTime = next - DateTime.UtcNow.Ticks;
                if (waitTime > 0)
                    Thread.Sleep(new TimeSpan(waitTime));
                //TODO:  if not more than zero, log item -- rate too fast!
            }
        }

        public void Dispose()
        {
            
        }
    }
}
