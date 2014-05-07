using SousChef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Dictionary<int, ServerGameObject> GameObjects = new Dictionary<int, ServerGameObject>();

        private Thread runThread;

        private Server server;

        public ConfigSalad Config { get; private set; }

        public List<ServerGameObject> HasAdded = new List<ServerGameObject>();  // TODO: Change this to a HashSet safetly
        public HashSet<ServerGameObject> HasChanged = new HashSet<ServerGameObject>();
        public List<int> HasRemoved = new List<int>();


        long millisecond_ticks = (new TimeSpan(0, 0, 0, 0, 1)).Ticks;
        private int _frameRate;
        public int FrameRateMilliseconds // Tick time in milliseconds
        {
            get { return _frameRate; }
            set 
            { 
                _frameRate = value;
                _frameRateTicks = FrameRateMilliseconds * millisecond_ticks; //rate to wait in ticks
                _frameRateSeconds = FrameRateMilliseconds * 0.001f;
            }

        }
        private long _frameRateTicks;
        long FrameRateTicks { get { return _frameRateTicks; } }
        private float _frameRateSeconds;
        float FrameRateSeconds { get { return _frameRateSeconds; } }

        int MAX_CONTACTS = 8;

        public IntPtr World { get; set; }
        public IntPtr Space { get; set; }
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
            FrameRateMilliseconds = int.Parse(config.GetSetting("frame-rate", 1000));
            Config = new GameObjectConfig().GetConfigSalad();
        }

        void server_ClientEnter(object sender, ClientEventArgs e)
        {
            lock (Lock)
            {
                clients.Add(e.Client);
                lock (ClientInput)
                {
                    ClientInput.Add(new DCClientEvent() { Client = e.Client, Event = new ClientEnterEvent() }); //we can change this.
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
                    ClientInput.Add(new DCClientEvent() { Client = e.Client, Event = new ClientLeaveEvent() });
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
                Monitor.Exit(Lock);
                runThread.Join();
                Monitor.Enter(Lock); //let's hope nothing else changed in the meantime.  not sure how to fix this.
                runThread = null;
            }
        }

        private List<Client> clients = new List<Client>();

        // clients Lock(ClientInput) without locking the whole server, just to specify their input
        public List<DCClientEvent> ClientInput { get; private set; }

        void monitor()
        {
            lock (Lock)
            {
                Console.Clear();
                Console.WriteLine("MONITOR:");
                foreach (ServerGameObject sgo in GameObjects.Values)
                {
                    Console.WriteLine("Position: " + sgo.Position.X + ", " + sgo.Position.Y + ", " + sgo.Position.Z);
                }
            }
        }

        
        public void Run()
        {
            /* initialize physics */
            lock (Lock)
            {
                Ode.dInitODE();
                ContactGroup = Ode.dJointGroupCreate(0);

                WorldFileParser p = new WorldFileParser(new GameObjectConfig(), this);
                p.LoadFile(1);

                // loop over clients and make play objects for them
                foreach (var client in clients)
                {
                    client.Player = new ServerPlayer(server.Game, new Ode.dVector3(DC.random.Next(-100, 100), DC.random.Next(-100, 100), 10));
                    lock (client.ServerMessages)
                    {
                        client.ServerMessages.Add(new ServerPlayerIdUpdateMessage() { PlayerId = client.Player.Id });
                    }
                }
            }
            try
            {
                long next = DateTime.UtcNow.Ticks;
                while (true)
                {
                    next += FrameRateTicks;
                    lock (Lock)
                    {
                        if (Mode != GameMode.Started)
                            return;

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
                                    case ClientEventType.Test:
                                        var ppos = input.Client.Player.Position;
                                        var pos = new Ode.dVector3()
                                        {
                                            X = ppos.X,
                                            Y = ppos.Y,
                                            Z = ppos.Z + 100
                                        };
                                        ServerIngredient ing = new ServerIngredient(Config.Ingredients["banana"], this, pos);
                                        break;
                                    case ClientEventType.ChangeOrientation:
                                        break;
                                    case ClientEventType.BeginMove:
                                        ClientBeginMoveEvent e = (ClientBeginMoveEvent)input.Event;
                                        Vector4 direction = (input.Client.Player.Rotation * new Vector4(0.0, 1.0, 0.0));
                                        var lastPos = input.Client.Player.Position;
                                        var newpos = new Ode.dVector3();
                                        newpos.X = lastPos.X + e.Delta.x;
                                        newpos.Y = lastPos.Y + e.Delta.y;
                                        newpos.Z = lastPos.Z + e.Delta.z;
                                        input.Client.Player.Position = newpos;
                                        //TEST
                                        //direction.Scale(3.0f);
                                        //input.Client.Player.Position = new Ode.dVector3(lastPos.X + direction.X, lastPos.Y + direction.Y, lastPos.Z + direction.Z);
                                        break;
                                    case ClientEventType.EndMove:
                                        break;
                                    default:
                                        Debugger.Break();
                                        throw new Exception("server does not understand client event " + input.Event.Type.ToString());
                                }
                            }
                            ClientInput.Clear();
                        }

                        /*
                         * Physics happens here.
                         */
                        Ode.dSpaceCollide(Space, IntPtr.Zero, dNearCallback);
                        Ode.dWorldQuickStep(World, FrameRateSeconds);
                        Ode.dJointGroupEmpty(ContactGroup);

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
                                    {
                                        obj.Serialize(writer);
                                        HasChanged.Remove(obj);
                                    }
                                    writer.Write(HasChanged.Count);
                                    foreach (var obj in HasChanged)
                                        obj.UpdateStream(writer);
                                    writer.Write(HasRemoved.Count);
                                    foreach (var obj in HasRemoved)
                                        writer.Write(obj);
                                    HasAdded.Clear();
                                    HasChanged.Clear();
                                    HasRemoved.Clear();
                                }
                                bin = membin.ToArray();
                                binlen = bin.Length;
                            }
                            var msg = new ServerGameStateUpdateMessage()
                            {
                                Binary = bin,
                                Created = DateTime.Now
                            };

                            foreach (Client client in clients)
                            {
                                lock (client.ServerMessages)
                                {
                                    client.ServerMessages.Add(msg);
                                    Monitor.PulseAll(client.ServerMessages);
                                }
                            }
                        }
                    }

                    //monitor();
                    /*
                     * wait until end of tick
                     */
                    long waitTime = next - DateTime.UtcNow.Ticks;
                    if (waitTime > 0)
                        Thread.Sleep(new TimeSpan(waitTime));
                    else
                        Console.WriteLine("error:  tick rate too fast by " + (-waitTime / millisecond_ticks) + "ms.");
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
            // exit without doing anything if the two bodies are connected by a joint
            IntPtr b1 = Ode.dGeomGetBody(o1);
            IntPtr b2 = Ode.dGeomGetBody(o2);
            if (b1 != IntPtr.Zero && b2 != IntPtr.Zero && Ode.dAreConnectedExcluding(b1, b2, (int)Ode.dJointTypes.dJointTypeContact) > 0) return;

            Ode.dContact[] contact = new Ode.dContact[MAX_CONTACTS];   // up to MAX_CONTACTS contacts per box-box
            Ode.dContactGeom[] contactGeoms = new Ode.dContactGeom[MAX_CONTACTS];
            for (int i = 0; i < MAX_CONTACTS; i++)
            {
                contactGeoms[i] = new Ode.dContactGeom();
                contact[i] = new Ode.dContact();
            }
            int numc;
            unsafe
            {
                numc =  Ode.dCollide(o1, o2, MAX_CONTACTS, contactGeoms, sizeof(Ode.dContactGeom));
            }
            if (numc > 0)
            {
                for (int i = 0; i < numc; i++)
                {
                    // Collision physics parameters
                    contact[i].surface.mode = (int)Ode.dContactFlags.dContactBounce | (int)Ode.dContactFlags.dContactSoftCFM;
                    contact[i].surface.mu = 1;
                    contact[i].surface.mu2 = 0;
                    contact[i].surface.bounce = 0.1;
                    contact[i].surface.bounce_vel = 0.1;
                    contact[i].surface.soft_cfm = 0.01;
                    contact[i].geom = contactGeoms[i];

                    //IntPtr c = Ode.dJointCreateContact(this.World, this.ContactGroup, ref contact[i]);

                    IntPtr c = Ode.dJointCreateFixed(this.World, this.ContactGroup);
                    Ode.dJointAttach(c, b1, b2);
                    Ode.dJointSetFixed(c);
                }
            }

            // Call the objects onCollision() method
            IntPtr gameObjectId1 = Ode.dGeomGetData(o1);
            IntPtr gameObjectId2 = Ode.dGeomGetData(o2);
            ServerGameObject gameObject1 = GameObjects[gameObjectId1.ToInt32()];
            ServerGameObject gameObject2 = GameObjects[gameObjectId2.ToInt32()];
            gameObject1.OnCollide(gameObject2);
        }

        private void SendModeChangeUpdate()
        {
            foreach (Client client in clients)
            {
                lock (client.ServerMessages)
                {
                    client.ServerMessages.Add(new ServerGameModeUpdateMessage()
                    {
                        Mode = Mode
                    });
                    Monitor.PulseAll(client.ServerMessages);
                }
            }
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
            this.HasChanged.Remove(obj);
            GameObjects.Remove(obj.Id);
            
        }

        public ServerGameObject GeomToObj(IntPtr geom)
        {
            Lock.AssertHeld();
            return GameObjects[Ode.dGeomGetData(geom).ToInt32()];
        }

        internal void PrintStatus()
        {
            Lock.AssertHeld();
            Console.WriteLine("Game mode: " + Mode.ToString());
            Console.WriteLine(GameObjects.Count + " game objects");
        }




        // TODO: Dev code for cooker adding
        public void TestCookerAdd(int cookerId, int ingredientId)
        {
            CommandLinePlayer.TestCookerAdd(this.GameObjects, cookerId, ingredientId);
        }

        // TODO: Dev Code to list current game objects
        public void ListGameObjects()
        {
            CommandLinePlayer.ListGameObjects(this.GameObjects);
        }
        public void ListIngredients()
        {
            CommandLinePlayer.ListIngredients(this.GameObjects);
        }
        public void ListCookerContents(int cookerId)
        {
            CommandLinePlayer.ListCookerContents(this.GameObjects, cookerId);
        }
    }
}
