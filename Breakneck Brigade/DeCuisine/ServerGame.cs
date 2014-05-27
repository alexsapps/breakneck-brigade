﻿using SousChef;
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

        public GameMode Mode { get; set; }

        public Dictionary<int, ServerGameObject> GameObjects = new Dictionary<int, ServerGameObject>();

        public ServerGameController Controller { get; private set; }

        private Thread runThread;

        private Server server;

        public DateTime StartTime { get; set; }
        public TimeSpan GameTime { get { return DateTime.Now.Subtract(StartTime); } }

        public ConfigSalad Config { get; private set; }

        public HashSet<ServerGameObject> HasAdded = new HashSet<ServerGameObject>();  
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

        public bool MultiJump { get; set; }

        // Physics
        DynamicsWorld _world;
        public DynamicsWorld World
        {
            get { return _world; }
            set { _world = value; }
        }

        public CollisionConfiguration CollisionConf;
        public CollisionDispatcher Dispatcher;
        public BroadphaseInterface Broadphase;
        public AlignedCollisionShapeArray CollisionShapes { get; private set; }

        public ServerGame(Server server)
        {
            this.Mode = GameMode.Init; //start in init mode
            this.server = server;
            server.ClientEnter += server_ClientEnter;
            server.ClientLeave += server_ClientLeave;
            lock (Lock)
            {
                ClientInput = new List<DCClientEvent>();
                Controller = new ServerGameController(this);
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
            
            Config = new GameObjectConfig().GetConfigSalad();

            var configFolder = new GlobalsConfigFolder();
            var config = configFolder.Open("settings.xml");
            FrameRateMilliseconds = int.Parse(config.GetSetting("frame-rate", 100));
            
            Controller.UpdateConfig(Config, int.Parse(config.GetSetting("num-goals", 1)));
        }

        void server_ClientEnter(object sender, ClientEventArgs e)
        {
            lock (Lock)
            {
                lock (e.Client.Lock)
                {
                    clients.Add(e.Client);

                    SendMode(e.Client);

                    if (Mode == GameMode.Started)
                    {
                        e.Client.SendMessage(CalculateGameStateMessage(CalculateGameStateFull)); //do this before StartClient so it doesn't get sent its player object right away, since it's already going to be in HasChanged
                        StartClient(e.Client);
                    }
                }
            }
        }

        private void StartClient(Client client)
        {
            client.Lock.AssertHeld();
            client.Player = new ServerPlayer(server.Game, new Vector3(10, 100, 10), client);
            client.SendMessage(new ServerPlayerIdUpdateMessage() { PlayerId = client.Player.Id });
        }

        void server_ClientLeave(object sender, ClientEventArgs e)
        {
            lock (Lock)
            {
                if(Mode == GameMode.Started || Mode == GameMode.Paused)
                if(e.Client.Player != null)
                    if(e.Client.Player.InWorld) //could have disconnected BECAUSE the player was removed (fell off screeen), so we need to do this check.
                        e.Client.Player.Remove();
                clients.Remove(e.Client);
            }
        }

        public void Start()
        {
            Lock.AssertHeld();

            if (Mode != GameMode.Init)
                throw new Exception("can't start from state " + Mode.ToString() + ".");

            runThread = new Thread(new ThreadStart(Run));
            runThread.Start();

            Mode = GameMode.Started;
            StartTime = DateTime.Now;
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

            //we just loaded the config file, and the objects think everything has been added in a delta kind of way, but really it was just initing, so clear this.  init data is not just a big delta anymore,
            //it's it's own thing sent in StartClient which uses the CalculateGameStateFull function.  this is a workaround that's easier than an actual solution, such as game objects checking if the game is
            //in init mode before adding it to HasAdded
            HasAdded.Clear();
            HasChanged.Clear();
            HasRemoved.Clear();

            // loop over clients and make play objects for them
            var startMsg = CalculateGameStateMessage(CalculateGameStateFull); //must be computed before StartClient called for any client, because it should not include their players, which are already in HasChanged
            foreach (var client in clients)
            {
                StartClient(client);
                client.SendMessage(startMsg);
            }
            try
            {
                long next;
                long fallBehind = 0;
                while (true)
                {
                    next = DateTime.UtcNow.Ticks + FrameRateTicks; //not next+= FrameRateTicks because we don't want the server to ever wait less than the tick time
                    lock (Lock)
                    {
                        switch(this.Mode)
                        {
                            case GameMode.Started:
                                this.UpdateGame(fallBehind);
                                break;
                            case GameMode.Paused:
                                break;
                            default:
                                return;
                        }


                        /*
                         * send updates to clients
                         */
                        {
                            var msg = CalculateGameStateMessage(CalculateGameStateDifference);
                            foreach (Client client in clients)
                            {
                                lock (client.Lock)
                                {
                                    client.SendMessage(msg);
                                }
                            }
                        }
                    }

                    //monitor();
                    /*
                     * wait until end of tick
                     */
                    long waitTime = next - DateTime.UtcNow.Ticks;
                    fallBehind = 0;
                    if (waitTime > 0)
                        Thread.Sleep(new TimeSpan(waitTime));
                    else
                    {
                        fallBehind = -waitTime / millisecond_ticks;
                        Program.WriteLine("error:  tick rate too fast by " + fallBehind + "ms.");
                    }
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
                        constraint.Dispose();
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

        private void UpdateGame(long fallBehind)
        {
            /*
             * handle client input, e.g. move
             */
            List<DCClientEvent> inputs;
            lock (ClientInput)
            {
                inputs = new List<DCClientEvent>(ClientInput);
                ClientInput.Clear();
            }
            foreach (DCClientEvent input in inputs)
            {
                var client = input.Client;
                lock (client.Lock)
                {
                    if (!client.IsConnected)
                        break; //player has disconnected by the time we got around to processing this event.  may get null ptr trying to access its player, so return.
                    var player = client.Player;
                    switch (input.Event.Type)
                    {
                        case ClientEventType.Test:
                            break;
                        case ClientEventType.ChangeOrientation:
                            var orientationEv = (ClientChangeOrientationEvent)input.Event;
                            player.Orientation = orientationEv.Orientation;
                            player.Incline = orientationEv.Incline;
                            break;
                        case ClientEventType.BeginMove:
                            var moveEv = (ClientBeginMoveEvent)input.Event;
                            player.Move(moveEv.Delta.x, moveEv.Delta.y, moveEv.Delta.z);
                            break;
                        case ClientEventType.EndMove:
                            break;
                        case ClientEventType.Jump:
                            player.Jump();
                            break;
                        case ClientEventType.ThrowItem:
                            var thrEv = (ClientThrowEvent)input.Event;
                            player.Throw(thrEv.Hand, thrEv.Orientation, thrEv.Incline, thrEv.Force);
                            break;
                        case ClientEventType.Dash:
                            player.Dash();
                            break;
                        case ClientEventType.Eject:
                            player.AttemptToEjectCooker();
                            break;
                        case ClientEventType.ChangeTeam:
                        case ClientEventType.Cook:
                            player.AttemptToCook();
                            break;
                        case ClientEventType.Command:
                            throw new InvalidOperationException(); //these handled elsewhere
                        default:
                            Debugger.Break();
                            throw new Exception("server does not understand client event " + input.Event.Type.ToString());
                    }
                }
            }



            /*
             * Physics happens here.
             */
            var timeStep = (FrameRateMilliseconds - (float)fallBehind) / 1000;
            _world.StepSimulation(FrameRateMilliseconds);
            //_world.StepSimulation(timeStep, 0, timeStep);

            this.Controller.Update();
        }

        /// <summary>
        /// runs a function in a new thread that has the game's lock
        /// </summary>
        /// <param name="start"></param>
        public void Invoke(ServerInvokable start)
        {
            new Thread(() => { runInLock(this, start); }).Start();
        }
        public delegate void ServerInvokable();
        private void runInLock(ServerGame game, ServerInvokable invokable)
        {
            lock(game.Lock)
            {
                invokable();
            }
        }

        public delegate void AsyncCommandResultCallback(string result, Client client);
        public static void DoServerCommandAsync(string[] args, Client client, AsyncCommandResultCallback callback)
        {
            new Thread(() => { asyncServerCommandThread(args, client, callback); }).Start();
        }

        protected static void asyncServerCommandThread(string[] args, Client client, AsyncCommandResultCallback callback)
        {
            string result = Program.DoCommand(args);
            
            //do not do anything complicated between beginwrite and endwrite or it will cause deadlock
            Program.serverConsole.BeginWrite();
            Program.serverConsole.Write("client: ", ConsoleColor.Green);
            Program.serverConsole.Write(string.Join(" ", args) + "\n" + result + "\n", ConsoleColor.Gray);
            Program.serverConsole.EndWrite();

            try
            {
                callback(result, client);
            }
            catch(Exception ex)
            {
                Program.WriteLine("Error sending result to client:\n" + ex.Message);
            }
        }
        public static void AsyncCommandCallback(string result, Client client)
        {
            client.SendMessage(new ServerCommandResponseMessage() { Result = result ?? "{server did not issue a text response}" });
        }

        protected delegate void GameStateWriter(BinaryWriter writer);
        protected ServerGameStateUpdateMessage CalculateGameStateMessage(GameStateWriter calculator)
        {
            var msg = new ServerGameStateUpdateMessage() { Created = DateTime.Now };
            msg.Write((writer) => { calculator(writer); });
            return msg;
        }

        protected void gameObjSendOrderSort(List<ServerGameObject> objects)
        {
            objects.Sort(GameObjSendOrderComparison);
        }

        protected void CalculateGameStateDifference(BinaryWriter writer)
        {
            CalculateGameStateHeader(writer);
            var added = HasAdded.ToList();
            writer.Write(added.Count);
            gameObjSendOrderSort(added);
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
        protected void CalculateGameStateFull(BinaryWriter writer)
        {
            CalculateGameStateHeader(writer);
            writer.Write(GameObjects.Count);
            var sortedObjets = GameObjects.Values.ToList();
            gameObjSendOrderSort(sortedObjets);
            foreach (var obj in sortedObjets)
            {
                obj.Serialize(writer);
            }
            writer.Write(0); //0 "changed"
            writer.Write(0); //0 "deleted"
        }

        protected void CalculateGameStateHeader(BinaryWriter writer)
        {
            //game state information that only applies while connected, but isn't associated with game objects

            writer.Write(GameTime.Ticks);
        }

        public int GameObjSendOrderComparison(ServerGameObject o1, ServerGameObject o2)
        {
            return o1.SortOrder - o2.SortOrder;
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
                lock (client.Lock)
                {
                    SendMode(client);
                }
            }
        }

        private void SendMode(Client client)
        {
            client.Lock.AssertHeld();
            var update = new ServerGameModeUpdateMessage()
            {
                Mode = Mode
            };
            client.SendMessage(update);
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
            this.HasAdded.Remove(obj); //may actually be deleted on same tick added, e.g. if added below delete-y-threshdold.
            this.HasRemoved.Add(obj.Id);
            this.HasChanged.Remove(obj);
            GameObjects.Remove(obj.Id); // 

        }

        public ServerGameObject GeomToObj(CollisionShape geom)
        {
            Lock.AssertHeld();
            return (ServerGameObject)geom.UserObject;
        }

        public void PrintStatus(StringBuilder b)
        {
            Lock.AssertHeld();
            b.AppendLine("Game mode: " + Mode.ToString());
            b.AppendLine(GameObjects.Count + " game objects");
        }




        // TEST: Dev code for cooker adding
        public void TestCookerAdd(int cookerId, int ingredientId)
        {
            CommandLinePlayer.TestCookerAdd(this.GameObjects, cookerId, ingredientId);
        }

        // TEST: Dev Code to list current game objects
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

        // TEST
        public string ClearBoard()
        {
            return CommandLinePlayer.ClearBoard(this.GameObjects);
        }

        public void RemoveObj(int id)
        {
            this.GameObjects[id].Remove();
        }


    }
}
