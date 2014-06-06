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

        public List<ServerMessage> ServerEvents { get; set; }

        public HashSet<int> HeldObjects = new HashSet<int>();


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
            this.ServerEvents = new List<ServerMessage>();
#if PROJECT_DEBUG || PROJECT_WORLD_BUILDING
            MultiJump = true;
#else
            MultiJump = false;
#endif
        }
        public void Dispose()
        {
            server.ClientEnter -= server_ClientEnter;
            server.ClientLeave -= server_ClientLeave;
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
            this.Controller.UpdateConfig();
        }

        void server_ClientEnter(object sender, ClientEventArgs e)
        {
            lock (Lock)
            {
                lock (e.Client.Lock)
                {
                    clients.Add(e.Client);

                    SendMode(e.Client);
                    SendLobbyState(e.Client);

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
            Vector3 spawnLoc = this.Controller.AssignSpawnPoint(client);
            client.Player = new ServerPlayer(server.Game, spawnLoc, client);
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

            while (Mode == GameMode.Init)
                Monitor.Wait(Lock);
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
                Mode = GameMode.Started;
                SendModeChangeUpdate();

                this.CollisionShapes = new AlignedCollisionShapeArray();

                // collision configuration contains default setup for memory, collision setup
                WorldFileParser p = new WorldFileParser(new GameObjectConfig(), this);
                p.LoadFile(1);
                _world.SetInternalTickCallback(CollisionCallback);


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

                StartTime = DateTime.Now;
                Monitor.PulseAll(Lock);
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
                            case GameMode.Results:
                                break;
                            default:
                                return;
                        }
                        /*
                         * send updates to clients
                         */
                        if(this.Mode == GameMode.Started)
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

        private bool UpdateGame(long fallBehind)
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
#if !PROJECT_DEBUG && !PROJECT_WORLD_BUILDING
                    if (this.Controller.CurrentGameState == ServerGameController.GameControllerState.Waiting && input.Event.Type != ClientEventType.ChangeOrientation)
                        break; // don't process these client events.
#endif
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
                        case ClientEventType.LeftClickEvent:
                            var thrEv = (ClientLeftClickEvent)input.Event;
                            player.Throw(thrEv.Hand, thrEv.Force); // thrEv.Orientation, thrEv.Incline, thrEv.Force);
                            break;
                        case ClientEventType.Dash:
                            player.Dash();
                            break;
                        case ClientEventType.Eject:
                            player.AttemptToEjectCooker();
                            break;
                        case ClientEventType.ChangeTeam:
                            break;
                        case ClientEventType.Cook:
                            player.AttemptToCook();
                            break;
                        case ClientEventType.Hint:
                            foreach(var objectName in client.Team.getTintList())
                            {
                                foreach(ServerGameObject sgo in GameObjects.Values)
                                {
                                    if(sgo is ServerIngredient)
                                    {
                                        if(((ServerIngredient) sgo).Type.Name == objectName)
                                        {
                                            Vector3 above = sgo.Position + new Vector3(0, 10, 0);
                                            SendParticleEffect(BBParticleEffect.ARROW, above, player);
                                        }
                                    }
                                }
                            }
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

            if(!this.Controller.Update())
            {
                SendLobbyStateToAll(); //Game.MarkLobbyStateDirty(); //TODO: what's the right way to do this?
                
                if (Winner != null)
                Program.WriteLine("Team " + Winner.Name + " Wins!");
                else
                    Program.WriteLine("Draw! No team won.");

                Mode = GameMode.Results;
                SendModeChangeUpdate();
                return false;
            }

            return true;
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
            writer.Write(ServerEvents.Count);
            foreach(var e in this.ServerEvents)
            {
                writer.Write((byte)e.Type);
                e.Write(writer);
            }
            HasAdded.Clear();
            HasChanged.Clear();
            HasRemoved.Clear();
            ServerEvents.Clear();

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

            var events = new List<ServerMessage>();
            foreach (var team in Controller.Teams.Values)
            {
                events.Add(computeTintListMessage(team));
            }
            events.Add(computeGoalsMessage());

            writer.Write(events.Count);
            foreach (var e in events)
            {
                writer.Write((byte)e.Type);
                e.Write(writer);
            }
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

        private ServerGameModeUpdateMessage getModeMessage()
        {
            return new ServerGameModeUpdateMessage() { Mode = Mode };
        }
        private void SendMode(Client client)
        {
            MessageClient(client, getModeMessage());
        }
        private void SendModeChangeUpdate()
        {
            MessageAll(getModeMessage());
        }

        public void MessageClient(Client client, ServerMessage message)
        {
            client.Lock.AssertHeld();
            client.SendMessage(message);
        }
        public void MessageClient(Client client, messageGenerator messageGen)
        {
            client.Lock.AssertHeld();
            client.SendMessage(messageGen(client));
        }
        public void MessageAll(ServerMessage message)
        {
            MessageAll((c) => message);
        }
        public void MessageAll(messageGenerator messageGen)
        {
            foreach (Client client in clients)
            {
                lock (client.Lock)
                {
                    MessageClient(client, messageGen);
                }
            }
        }
        public delegate ServerMessage messageGenerator(Client client);

        private ServerLobbyStateUpdateMessage calculateServerLobbyStateMessage(Client client)
        {
            ServerLobbyStateUpdateMessage msg = new ServerLobbyStateUpdateMessage();
            msg.Write((w) =>
            {
                w.Write(Controller.Teams.Count);
                foreach(var team in Controller.Teams.Values)
                {
                    w.Write(team.Name);
                    w.Write(team.Points);
                    var members = team.GetMembers();
                    w.Write(members.Count);
                    foreach (var member in members)
                        w.Write(member.ToString());
                }
                w.Write(client.Team.Name);
                w.Write(Controller.ScoreToWin);
                w.Write((long)Controller.MaxTime);
                w.Write(Winner != null);
                if (Winner != null)
                    w.Write(Winner.Name);
            });
            return msg;
        }
        public void SendLobbyState(Client client)
        {
            MessageClient(client, calculateServerLobbyStateMessage);
        }
        public void SendLobbyStateToAll()
        {
            MessageAll(calculateServerLobbyStateMessage);
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


        public ServerTeam Winner { get; set; }

        public void SendTintListUpdate(ServerTeam serverTeam)
        {
            ServerEvents.Add(computeTintListMessage(serverTeam));
        }
        private ServerTintListUpdateMessage computeTintListMessage(ServerTeam team)
        {
            return new ServerTintListUpdateMessage() { Team = team.Name, TintList = team.getTintList()};
        }


        public void SendGoalsUpdate()
        {
            ServerEvents.Add(computeGoalsMessage());
        }
        private ServerGoalsUpdateMessage computeGoalsMessage()
        {
            var msg = new ServerGoalsUpdateMessage();
            msg.Write((writer) =>
            {
                var count = Controller.Goals.Count;
                writer.Write(count);
                for (int i = 0; i < count; i++)
                {
                    var goal = Controller.Goals[i];
                    writer.Write(goal.EndGoal.FinalProduct.Name);
                    writer.Write(goal.Expiring);
                }
            });
            return msg;
        }

        public void SendSound(BBSound sound, Vector3 location)
        {
            var msg = new ServerSoundMessage() { Sound = sound, Location = location.ToVector4() };
            ServerEvents.Add(msg);
        }
        public void SendParticleEffect(BBParticleEffect effect, Vector3 location)
        {
            SendParticleEffect(effect, location, 0, null);
        }
        public void SendParticleEffect(BBParticleEffect effect, Vector3 location, ServerPlayer player)
        {
            SendParticleEffect(effect, location, 0, player);
        }
        public void SendParticleEffect(BBParticleEffect effect, Vector3 location, int param)
        {
            SendParticleEffect(effect, location, param, null);
        }
        public void SendParticleEffect(BBParticleEffect effect, Vector3 location, int param, ServerPlayer player)
        {
            int id = -1;
            if (player != null)
                id = player.Id;
            var msg = new ServerParticleEffectMessage() { ParticleEffect = effect, Location = location.ToVector4(), Param = param, Id = id };
            ServerEvents.Add(msg);
        }
        public void SendParticleEffect(BBParticleEffect effect, Vector3 location, int param, int followID)
        {
            var msg = new ServerParticleEffectMessage() { ParticleEffect = effect, Location = location.ToVector4(), Param = param , FollowID = followID};
            ServerEvents.Add(msg);
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

        // Command Line Player functions
        public string ClearBoard()
        {
            return CommandLinePlayer.ClearBoard(this.GameObjects);
        }

        public void RemoveObj(int id)
        {
            this.GameObjects[id].Remove();
        }

        public void MoveObj(int id, int x, int y, int z)
        {
            ServerGameObject objToMove;
            if (this.GameObjects.TryGetValue(id, out objToMove))
            {
                objToMove.Position = new Vector3(x, y, z);
                objToMove.Body.ProceedToTransform(Matrix.RotationY(objToMove.GeomInfo.Orientation) * Matrix.Translation(objToMove.Position));
                Program.WriteLine("The new Position is " + objToMove.Position.X + " " + objToMove.Position.Y + " " + objToMove.Position.Z);
        }

        }


        /// <summary>
        /// Change the values by deltas
        /// </summary>
        public void TransObj(int id, int x, int y, int z)
        {
            ServerGameObject objToMove;
            if (this.GameObjects.TryGetValue(id, out objToMove))
            {
                objToMove.Position = new Vector3(objToMove.Position.X + x, objToMove.Position.Y + y, objToMove.Position.Z + z);
                objToMove.Body.ProceedToTransform(Matrix.RotationY(objToMove.GeomInfo.Orientation) * Matrix.Translation(objToMove.Position));
                Program.WriteLine("The new Position is " + objToMove.Position.X + " " + objToMove.Position.Y + " " + objToMove.Position.Z);
            }
        }

        // Hacked way to scale on the fly, reset the Geom info before making
        // the new object. Hacked but we will never need to do this in production
        public int ScaleObj(int id, float x, float y, float z)
        {
            ServerGameObject objToMove;
            if (!this.GameObjects.TryGetValue(id, out objToMove))
                return -1;
            ServerGameObject scaled = null;
            GeometryInfo oldGeomInfo;
            GeometryInfo newGeomInfo;
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            switch (objToMove.ObjectClass)
            {
                case GameObjectClass.Cooker:
                    var cooker = (ServerCooker)objToMove;
                    oldGeomInfo = this.Config.Cookers[cooker.Type.Name].GeomInfo;
                    // * MathConstants.DEG2RAD
                    attributes.Add("orientation",radToDegreeString(oldGeomInfo.Orientation));
                    this.Config.Cookers[cooker.Type.Name].GeomInfo = BBXItemParser<CookerType>.getGeomInfo(
                        attributes, new float[3] { x, y, z }, oldGeomInfo.Mass, oldGeomInfo.Friction, oldGeomInfo.RollingFriction, oldGeomInfo.Restitution, oldGeomInfo.AngularDamping, oldGeomInfo.Orientation, cooker.Type.Name);
                    scaled = new ServerCooker(cooker.Type, cooker.Team, cooker.Game, cooker.Position, cooker.GeomInfo);
                    cooker.Remove();
                    //new ServerCooker(objToMove)
                    break;
                case GameObjectClass.Ingredient:
                    var ing = (ServerIngredient)objToMove;
                    oldGeomInfo = this.Config.Ingredients[ing.Type.Name].GeomInfo;
                    attributes.Add("orientation",radToDegreeString(oldGeomInfo.Orientation));
                    this.Config.Ingredients[ing.Type.Name].GeomInfo = BBXItemParser<CookerType>.getGeomInfo(
                        attributes, new float[3] { x, y, z }, oldGeomInfo.Mass, oldGeomInfo.Friction, oldGeomInfo.RollingFriction, oldGeomInfo.Restitution, oldGeomInfo.AngularDamping, oldGeomInfo.Orientation, ing.Type.Name);
                    scaled = new ServerIngredient(ing.Type, ing.Game, ing.Position);
                    ing.Remove();
                    break;
                case GameObjectClass.Player:
                    Program.WriteLine("Just don't try to scale the player...it will fuck shit up");
                    break;
                case GameObjectClass.StaticObject:
                    var statObj = (ServerStaticObject)objToMove;
                    oldGeomInfo = statObj.GeomInfo; // No type so it's different
                    attributes.Add("orientation",radToDegreeString(oldGeomInfo.Orientation));
                    newGeomInfo = BBXItemParser<CookerType>.getGeomInfo(
                        attributes, new float[3] { x, y, z }, oldGeomInfo.Mass, oldGeomInfo.Friction, oldGeomInfo.RollingFriction, oldGeomInfo.Restitution, oldGeomInfo.AngularDamping, oldGeomInfo.Orientation, oldGeomInfo.Model);
                    scaled = new ServerStaticObject(statObj.Game, newGeomInfo, statObj.Model, "fucker", statObj.Position, "blue");
                    statObj.Remove();
                    break;
                case GameObjectClass.Terrain:
                    var terrain = (ServerTerrain)objToMove;
                    oldGeomInfo = terrain.GeomInfo; // No type so it's different
                    attributes.Add("orientation",radToDegreeString(oldGeomInfo.Orientation));
                    newGeomInfo = BBXItemParser<CookerType>.getGeomInfo(
                        attributes, new float[3] { x, y, z }, oldGeomInfo.Mass, oldGeomInfo.Friction, oldGeomInfo.RollingFriction, oldGeomInfo.Restitution, oldGeomInfo.AngularDamping, oldGeomInfo.Orientation, terrain.Type.Name);
                    scaled = new ServerTerrain(terrain.Game, terrain.Type, terrain.Position, newGeomInfo);
                    terrain.Remove();
                    break;

            }

            if (scaled != null)
                return scaled.Id;
            return -1;
        }
        public void RotObj(int id, int deg)
        {
            ServerGameObject objToMove;
            if (this.GameObjects.TryGetValue(id, out objToMove))
            {
                objToMove.GeomInfo.Orientation = deg * MathConstants.DEG2RAD;
                objToMove.Body.ProceedToTransform(Matrix.RotationY(deg * MathConstants.DEG2RAD) * Matrix.Translation(objToMove.Position));
                Program.WriteLine("The new Position is " + objToMove.Position.X + " " + objToMove.Position.Y + " " + objToMove.Position.Z + " with rotation "+ deg);
                objToMove.MarkDirty();
            }
        }
        private string radToDegreeString(float rad)
        {
            string str = (rad * (1 / MathConstants.DEG2RAD)).ToString();
            return str;

        }
    }
}
