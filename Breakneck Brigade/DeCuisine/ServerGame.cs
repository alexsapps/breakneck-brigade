using SousChef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BulletSharp;

namespace DeCuisine
{
    class ServerGame : System.IDisposable
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

        // Physics
        DynamicsWorld _world;
        public DynamicsWorld World
        {
            get { return _world; }
            set { _world = value; }
        }

        protected CollisionConfiguration CollisionConf;
        protected CollisionDispatcher Dispatcher;
        protected BroadphaseInterface Broadphase;
        protected ConstraintSolver Solver;
        public AlignedCollisionShapeArray CollisionShapes { get; private set; }

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

        public virtual RigidBody LocalCreateRigidBody(float mass, Matrix startTransform, CollisionShape shape)
        {
            //rigidbody is dynamic if and only if mass is non zero, otherwise static
            bool isDynamic = (mass != 0.0f);

            Vector3 localInertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            //using motionstate is recommended, it provides interpolation capabilities, and only synchronizes 'active' objects
            DefaultMotionState myMotionState = new DefaultMotionState(startTransform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);
            rbInfo.Dispose();

            _world.AddRigidBody(body);

            return body;
        }

        public void Run()
        {
            /* initialize physics */
            lock (Lock)
            {
                this.CollisionShapes = new AlignedCollisionShapeArray();

                // collision configuration contains default setup for memory, collision setup
                WorldFileParser p = new WorldFileParser(new GameObjectConfig(), this);
                p.LoadFile(1);
                _world.SetInternalTickCallback(CollisionCallback);
            }
            // loop over clients and make play objects for them
            foreach (var client in clients)
            {
                client.Player = new ServerPlayer(server.Game, new Vector3(10, 100, 10));
                lock (client.ServerMessages)
                {
                    client.ServerMessages.Add(new ServerPlayerIdUpdateMessage() { PlayerId = client.Player.Id });
                }
            }
            try
            {
                long next;
                while (true)
                {
                    next = DateTime.UtcNow.Ticks + FrameRateTicks; //not next+= FrameRateTicks because we don't want the server to ever wait less than the tick time
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
                                        var pos = new Vector3()
                                        {
                                            X = 10,
                                            Y = 10,
                                            Z = 10
                                        };
                                        ServerIngredient ing = new ServerIngredient(Config.Ingredients["banana"], this, pos);
                                        break;
                                    case ClientEventType.ChangeOrientation:
                                        break;
                                    case ClientEventType.BeginMove:
                                        ClientBeginMoveEvent e = (ClientBeginMoveEvent)input.Event;
                                        //SousChef.Vector4 direction = (input.Client.Player.Rotation * new SousChef.Vector4(0.0, 1.0, 0.0));
                                        var lastPos = input.Client.Player.Position;
                                        var newpos = new Vector3();
                                        newpos.X = (lastPos.X + e.Delta.x);
                                        newpos.Y = (lastPos.Y + e.Delta.y);
                                        newpos.Z = (lastPos.Z + e.Delta.z);
                                        input.Client.Player.Position = newpos;
                                        //TEST
                                        // direction.Scale(3.0f);
                                        // input.Client.Player.Position = new Vector3(lastPos.X + direction.X, lastPos.Y + direction.Y, lastPos.Z + direction.Z);
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
                        //ServerPlayer Last = clients[0].Player;
                        /*
                         * Physics happens here.
                         */
                        _world.StepSimulation(0.1f);
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
                if (_world != null)
                {
                    //remove/dispose constraints
                    int i;
                    for (i = _world.NumConstraints - 1; i >= 0; i--)
                    {
                        TypedConstraint constraint = _world.GetConstraint(i);
                        _world.RemoveConstraint(constraint);
                        constraint.Dispose(); ;
                    }

                    //remove the rigidbodies from the dynamics world and delete them
                    for (i = _world.NumCollisionObjects - 1; i >= 0; i--)
                    {
                        CollisionObject obj = _world.CollisionObjectArray[i];
                        RigidBody body = obj as RigidBody;
                        if (body != null && body.MotionState != null)
                        {
                            body.MotionState.Dispose();
                        }
                        _world.RemoveCollisionObject(obj);
                        obj.Dispose();
                    }

                    //delete collision shapes
                    foreach (CollisionShape shape in CollisionShapes)
                        shape.Dispose();
                    CollisionShapes.Clear();

                    _world.Dispose();
                }

                if (Broadphase != null)
                {
                    Broadphase.Dispose();
                }
                if (Dispatcher != null)
                {
                    Dispatcher.Dispose();
                }
                if (CollisionConf != null)
                {
                    CollisionConf.Dispose();
                }
            }
        }

        private void CollisionCallback(DynamicsWorld world, float timeStep)
        {
            int numManifolds = this.World.Dispatcher.NumManifolds;
            for (int i = 0; i < numManifolds; i++)
            {
                bool didCollide = false;
                PersistentManifold contactManifold = this.World.Dispatcher.GetManifoldByIndexInternal(i);
                CollisionObject obA = (CollisionObject)contactManifold.Body0; //btCollisionObject* obA = static_cast<btCollisionObject*>(contactManifold->getBody0());
                CollisionObject obB = (CollisionObject)contactManifold.Body1; //btCollisionObject* obB = static_cast<btCollisionObject*>(contactManifold->getBody1());
                int numContacts = contactManifold.NumContacts;
                for (int j = 0; j < numContacts; j++)
                {
                    ManifoldPoint pt = contactManifold.GetContactPoint(j);
                    if (pt.Distance < 0.0f)
                    {
                        Vector3 ptA = pt.PositionWorldOnA; //.getPositionWorldOnA();
                        Vector3 ptB = pt.PositionWorldOnB; // pt.getPositionWorldOnB();
                        Vector3 normalOnB = pt.NormalWorldOnB;
                        didCollide = true;
                    }
                }

                if (didCollide && obA.CollisionShape.UserObject != null && obB.CollisionShape.UserObject != null)
                {
                    ServerGameObject obj1 = (ServerGameObject)obA.CollisionShape.UserObject;
                    ServerGameObject obj2 = (ServerGameObject)obB.CollisionShape.UserObject;
                    obj1.OnCollide(obj2);
                    obj2.OnCollide(obj1);
                    // Get position vectors 
                    // regidbody.movementstate.worldtransform.origin
                }
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

        public ServerGameObject GeomToObj(CollisionShape geom)
        {
            Lock.AssertHeld();
            return (ServerGameObject)geom.UserObject;
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
        public string ListGameObjects()
        {
            return CommandLinePlayer.ListGameObjects(this.GameObjects);
        }
        public string ListIngredients()
        {
            return CommandLinePlayer.ListIngredients(this.GameObjects);
        }
        public string ListCookerContents(int cookerId)
        {
            return CommandLinePlayer.ListCookerContents(this.GameObjects, cookerId);
        }
    }
}
