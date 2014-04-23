using SousChef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tao.Ode;

namespace DeCuisine
{
    class ServerGame : IDisposable
    {
        public BBLock Lock = new BBLock();

        public GameMode Mode { get; private set; }

        private Dictionary<int, ServerGameObject> GameObjects = new Dictionary<int, ServerGameObject>(); //TODO: this should be string (id) to GameObject

        private Thread runThread;

        private Server server;

        public ConfigSalad Config { get; private set; }

        public List<ServerGameObject> HasAdded = new List<ServerGameObject>();
        public List<ServerGameObject> HasChanged = new List<ServerGameObject>();
        public List<int> HasRemoved = new List<int>();

        private Random random = new Random();

        int frameRate; // Tick time in milliseconds

        public IntPtr World { get; protected set; }
        public IntPtr Space { get; protected set; }
        public IntPtr ContactGroup { get; protected set; }

        public ServerGame(Server server)
        {
            Mode = GameMode.Init; //start in init mode
            this.server = server;
            server.ClientEnter += server_ClientEnter;
            server.ClientLeave += server_ClientLeave;
            lock (Lock)
            {
                ClientInput = new List<DCClientEvent>();
                loadConfig();
            }
        }

        public void ReloadConfig()
        {
            loadConfig();
        }

        void loadConfig()
        {
            Lock.AssertHeld();
            var configFolder = new GlobalsConfigFolder();
            var config = configFolder.Open("settings.xml");
            frameRate = int.Parse(config.GetSetting("frame-rate", 1000));

            Config = new GameObjectConfig().GetConfigSalad();
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
            long millisecond_ticks = (new TimeSpan(0, 0, 0, 0, 1)).Ticks;
            int milliseconds = frameRate;
            long rate = milliseconds * millisecond_ticks; //rate to wait in ticks
            long next = start;

            /* initialize physics */
            Ode.dInitODE();
            World = Ode.dWorldCreate(); // Create dynamic world
            Space = Ode.dHashSpaceCreate(IntPtr.Zero); // Create dynamic space
            ContactGroup = Ode.dJointGroupCreate(0);
            Ode.dWorldSetGravity(World, 0f, 0f, -0.5f); //TODO: read gravity from config file
            Ode.dCreatePlane(Space, 0, 0, 1, 0); // Create a ground //TODO:  remove this line of test code (read from config file instead)
            try
            {
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
                                        GameObjects.Add(id, new ServerIngredient(id, new IngredientType("cheese", 10, null), this));
                                        break;
                                    default:
                                        //error
                                        break;
                                }
                            }
                            ClientInput.Clear();
                        }

                        /*
                         * Physics happens here.
                         */
                        {
                            Ode.dSpaceCollide(Space, IntPtr.Zero, dNearCallback);
                            Ode.dWorldStep(World, 0.001f * (float)frameRate);
                            Ode.dJointGroupEmpty(ContactGroup);
                        }

                        /*
                         * handle an instant in time, e.g. gravity, collisions
                         */
                        foreach (var obj in GameObjects)
                        {
                            obj.Value.Update();
                        }

                        /*
                         * send updates to clients
                         */
                        {
                            byte[] bin;
                            int binlen;

                            using (MemoryStream membin = new MemoryStream())
                            {
                                using (BinaryWriter writer = new BinaryWriter(membin))
                                {
                                    writer.Write(HasAdded.Count);
                                    foreach (var obj in HasAdded)
                                        obj.Serialize(writer);
                                    writer.Write(HasChanged.Count);
                                    foreach (var obj in HasChanged)
                                        obj.UpdateStream(writer);
                                    writer.Write(HasRemoved.Count);
                                    foreach (var obj in HasRemoved)
                                        writer.Write(obj);
                                }
                                bin = membin.ToArray();
                                binlen = bin.Length;
                            }

                            foreach (Client client in clients)
                            {
                                lock (client.ServerMessages)
                                {
                                    client.ServerMessages.Add(new ServerGameStateUpdateMessage()
                                    {
                                        Type = ServerMessageType.GameStateUpdate,
                                        Binary = bin,
                                        Length = binlen
                                    });
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
            finally
            {
                Ode.dJointGroupDestroy(ContactGroup);
                Ode.dSpaceDestroy(Space);
                Ode.dWorldDestroy(World);
                Ode.dCloseODE();

                World = IntPtr.Zero;
                Space = IntPtr.Zero;
                ContactGroup = IntPtr.Zero;
            }
        }



        private void dNearCallback(IntPtr data, IntPtr o1, IntPtr o2)
        {
            // is o1 a charcter? check list of players

            var obj1 = GeomToObj(o1);
            var obj2 = GeomToObj(o2);

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

        /*
         * this should be called only by ServerGameObject constructor
         */
        public void ObjectAdded(ServerGameObject obj)
        {
            Lock.AssertHeld();
            this.HasAdded.Add(obj);
            GameObjects.Add(obj.Id, obj);
        }

        public void ObjectChanged(ServerGameObject obj)
        {
            Lock.AssertHeld();
            this.HasChanged.Add(obj);
        }

        public void ObjectRemoved(ServerGameObject obj)
        {
            Lock.AssertHeld();
            this.HasRemoved.Add(obj.Id);
            GameObjects.Remove(obj.Id);
        }

        public ServerGameObject GeomToObj(IntPtr geom)
        {
            Lock.AssertHeld();
            return GameObjects[Ode.dGeomGetData(geom).ToInt32()];
        }
    }
}
