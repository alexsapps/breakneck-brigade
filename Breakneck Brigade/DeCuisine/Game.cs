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

        private Dictionary<int, GameObject> GameObjects = new Dictionary<int, GameObject>(); //TODO: this should be string (id) to GameObject

        private Thread runThread;

        private Server server;

        private Random random = new Random();

        public Game(Server server)
        {
            Mode = GameMode.Init; //start in init mode
            this.server = server;
            server.ClientEnter += server_ClientEnter;
            server.ClientLeave += server_ClientLeave;
            ClientInput = new List<DCClientEvent>();
        }

        void server_ClientEnter(object sender, ClientEventArgs e)
        {
            lock (Lock)
            {
                clients.Add(e.Client);

                lock (ClientInput)
                {
                    ClientInput.Add(new DCClientEvent() { Client = e.Client, Event = new ClientEvent() { Type = ClientEventType.Enter } }); //we can change this.
                }
            }
        }

        void server_ClientLeave(object sender, ClientEventArgs e)
        {
            lock (Lock)
            {
                clients.Remove(e.Client);

                lock (ClientInput)
                {
                    ClientInput.Add(new DCClientEvent() { Client = e.Client, Event = new ClientEvent() { Type = ClientEventType.Leave } });
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
            SendModeChangeUpdate();
        }

        public void Stop()
        {
            Lock.AssertHeld();

            if (Mode == GameMode.Stopping)
                throw new Exception("already stopping.");

            var prevMode = Mode;
            Mode = GameMode.Stopping;

            if (prevMode == GameMode.Started || prevMode == GameMode.Paused)
            {
                runThread.Join();
                runThread = null;
            }
        }

        private List<Client> clients = new List<Client>();

        // clients Lock(ClientInput) without locking the whole server, just to specify their input
        public List<DCClientEvent> ClientInput { get; private set; }

        public void Run()
        {
            long start = DateTime.UtcNow.Ticks;
            long second = (new TimeSpan(0, 0, 0, 1)).Ticks;
            int seconds = 1; //TODO: read from config
            long rate = seconds * second;
            long next = start;

            while (Mode == GameMode.Started)
            {
                next += rate;
                lock (Lock)
                {
                    /*
                     * handle client input, e.g. move
                     */
                    lock (ClientInput)
                    {
                        foreach (DCClientEvent input in ClientInput)
                        {
                            switch (input.Event.Type)
                            {
                                case ClientEventType.Enter:
                                    break;
                                case ClientEventType.Leave:
                                    break;
                                case ClientEventType.Move:
                                    break;
                                case ClientEventType.RequestTestObject:
                                    int id = getId();
                                    GameObjects.Add(id, new Ingredient(id, null) { pos = new Vector4(random.Next(100), random.Next(100), random.Next(100), random.Next(100)) });
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
                    foreach (var obj in GameObjects)
                    {
                        
                    }

                    /*
                     * send updates to clients
                     */
                    {
                        foreach (Client client in clients)
                        {
                            lock (client.ServerMessages)
                            {
                                client.ServerMessages.Add(new ServerGameStateUpdateMessage() { 
                                    Type = ServerMessageType.GameStateUpdate, 
                                    GameObjects = GameObjects });
                                Monitor.PulseAll(client.ServerMessages);
                            }
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


        private void SendModeChangeUpdate()
        {
            foreach (Client client in clients)
            {
                lock (client.ServerMessages)
                {
                    client.ServerMessages.Add(new ServerGameModeUpdateMessage()
                    {
                        Type = ServerMessageType.GameModeUpdate,
                        Mode = Mode
                    });
                    Monitor.PulseAll(client.ServerMessages);
                }
            }
        }

        int _lastId;

        public int getId() {
            return _lastId++;
        }

        public void Dispose()
        {
            
        }
    }
}
