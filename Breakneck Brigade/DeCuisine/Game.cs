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

        private Server server;

        public Game(Server server)
        {
            Mode = GameMode.Init; //start in init mode
            this.server = server;
            server.ClientEnter += server_ClientEnter;
            server.ClientLeave += server_ClientLeave;
            ClientInput = new List<ClientEvent>();
        }

        void server_ClientEnter(object sender, ClientEventArgs e)
        {
            lock (Lock)
            {
                clients.Remove(e.Client);

                lock (ClientInput)
                {
                    ClientInput.Add(new ClientEvent() { Client = e.Client, Type = ClientEventType.Enter }); //we can change this.
                }
            }
        }

        void server_ClientLeave(object sender, ClientEventArgs e)
        {
            lock (Lock)
            {
                clients.Add(e.Client);

                lock (ClientInput)
                {
                    ClientInput.Add(new ClientEvent() { Client = e.Client, Type = ClientEventType.Leave });
                }
            }
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

        private List<Client> clients = new List<Client>();

        // clients Lock(ClientInput) without locking the whole server, just to specify their input
        public List<ClientEvent> ClientInput { get; private set; }

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
                    /*
                     * handle client input, e.g. move
                     */
                    lock (ClientInput)
                    {
                        foreach (ClientEvent input in ClientInput)
                        {
                            switch (input.Type)
                            {
                                case ClientEventType.Move:
                                    
                                    break;
                                default:
                                    //error
                                    break;
                            }
                        }
                        ClientInput.Clear();
                    }
                    
                    /*
                     * handle an instant in time, e.g. gravity, collisions
                     */
                    {

                    }

                    /*
                     * send updates to clients
                     */
                    {
                        //TODO: make ThreadPool for sending messages to all clients.  jobs are sending tick-update to client.
                        foreach(Client client in clients)
                        {
                            
                        }
                    }
                }
                
                /*
                 * wait until end of tick
                 */
                long waitTime = next - DateTime.UtcNow.Ticks;
                if (waitTime > 0)
                    Thread.Sleep(new TimeSpan(waitTime));
                //TODO:  if not more than zero, log item -- rate too fast!
            }
        }

        //ThreadPool clientUpdaterPool;
        private void updateClient(Task clientUpdateTask)
        {

        }

        public void Dispose()
        {
            
        }
    }
}
