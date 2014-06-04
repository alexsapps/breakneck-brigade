using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.Glfw;
using Tao.OpenGl;
using System.IO;
using Breakneck_Brigade.Graphics;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Breakneck_Brigade
{
    class Program
    {
        public static Client client; 
        static BBLock clientLock = new BBLock();
        static ClientGame _game;
        public static ClientGame game
        {
            get
            {
                gameLock.AssertHeld();
                return _game;
            }
            set
            {
                gameLock.AssertHeld();
                _game = value;
                if (_game != null)
                {
                    localPlayer = new LocalPlayer();
                    localPlayer.Game = _game;
                }
                else
                {
                    localPlayer = null;
                }
            }
        }
        static BBLock gameLock = new BBLock();

        public static ClientLobbyState lobbyState = new ClientLobbyState();
        static Renderer renderer;
        static Camera camera;

        static GlobalsConfigFolder config = new GlobalsConfigFolder();
        static GlobalsConfigFile globalConfig;

        private static List<AParticleSpawner> PSToRemove { get; set; }

        static public LocalPlayer localPlayer;
        static public InputManager IM;

        static void Main(string[] args)
        {
            
            //SoundThing.Play("scratch");

            //Thread.Sleep(5000);

            try { Console.SetWindowSize(120, 40); } catch { } //see also server's Main()

#if PROJECT_GRAPHICS_TEST
            Renderer renderer = new Renderer();
            IM = new InputManager();
            IM.EnableFPSMode();
            while (!renderer.ShouldExit())
            {
                cPlayer.Update(IM);
                renderer.Render(cPlayer);
            }
            Environment.Exit(0);
#endif


            globalConfig = config.Open(BB.GlobalConfigFilename);

            Thread inputThread = null;
            inputThread = new Thread(new ThreadStart(readInput));
            inputThread.Start();

            //Thread appThread = null;
            //appThread = new Thread(new ThreadStart(doGameCycle));
            //appThread.Start();

            //appThread.Join();

            PSToRemove = new List<AParticleSpawner>();
            
            doGameCycle();

            CloseHandle(GetStdHandle(StdHandle.Stdin)); //terminate input thread
            inputThread.Abort();
        }

        static void readInput()
        {
            try
            {
                Console.CancelKeyPress += Console_CancelKeyPress;

                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == null)
                        return; //thread asked to abort

                processLine:
                    string[] parts = line.Split(new string[] { " " }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        switch (parts[0])
                        {
                            case "q":
                            case "cancel":
                                cancelConsole = true;
                                break;
                            case "exit":
                                lock (clientLock)
                                {
                                    if (client != null)
                                    {
                                        client.Disconnect();
                                    }
                                }
                                cancelConsole = true;
                                Environment.Exit(0);

                                break;
                            case "t":
                            case "status":
                                lock(clientLock)
                                {
                                    if (client == null || !client.IsConnected)
                                    {
                                        Program.WriteLine("Not connected.");
                                    }
                                    else
                                    {
                                        Program.WriteLine("Connected.");
                                        lock (gameLock)
                                        {
                                            Program.WriteLine("GameMode: " + lobbyState.Mode.ToString());
                                            if (lobbyState.Mode == GameMode.Started || lobbyState.Mode == GameMode.Paused)
                                            {
                                                Program.WriteLine("game time: " + game.GameTime.ToString("g"));
                                                Program.WriteLine("max time: " + lobbyState.MaxTime.ToString("g"));
                                                Program.WriteLine(game.LiveGameObjects.Count + " game objects.");
                                            }
                                        }
                                    }
                                }
                                break;
                            case "r":
                            case "rate":
                                new Thread(() => { rateThread(); }).Start();
                                break;
                            case "modeltime":
                                lock (gameLock)
                                {
                                    if (renderer != null)
                                    {
                                        if (parts.Length == 2 && parts[1] == "reset")
                                        {
                                            renderer.ResetModelStatus();
                                            Program.WriteLine("ok");
                                        }
                                        else
                                            Program.WriteLine(renderer.GetModelTimerStatus());
                                    }
                                    else
                                        Program.WriteLine("renderer not loaded yet.");
                                }
                                break;
                            case "s":
                            case "server":
                                if (parts.Length > 1)
                                {
                                    lock (clientLock)
                                    {
                                        if (client != null)
                                        {
                                            string[] serverparts = new string[parts.Length - 1];
                                            Array.Copy(parts, 1, serverparts, 0, parts.Length - 1);
                                            client.SendConsoleCommand(serverparts);
                                        }
                                        else
                                        {
                                            Program.WriteLine("not connected to server.");
                                        }
                                    }
                                }
                                else
                                {
                                    Program.WriteLine("you didn't specify a command to send to the server.");
                                }
                                break;
                            case "goals":
                                {
                                    consoleGameOp(() =>
                                        {
                                            foreach (var goal in game.Goals)
                                                Program.WriteLine(goal.Name);
                                        });
                                    break;
                                }
                            case "team":
                                {
                                    if(parts.Length >= 2)
                                    {
                                        switch (parts[1])
                                        {
                                            case "list":
                                                lock (clientLock)
                                                {
                                                    if (client.IsConnected)
                                                    {
                                                        lock (gameLock)
                                                        {
                                                            foreach(var team in lobbyState.Teams.Values)
                                                            {
                                                                Program.WriteLine(team.Name + ":" + team.Score);
                                                                foreach(var member in team.Clients)
                                                                {
                                                                    Program.WriteLine("\t" + member);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Program.WriteLine("not connected.");
                                                    }
                                                }
                                                break;
                                            case "join":
                                                if (parts.Length == 3)
                                                {
                                                    sendEvent(new ClientChangeTeamEvent() { TeamName = parts[2] });
                                                    System.Threading.Thread.Sleep(300); //wait for this to go through
                                                    line = "team list";
                                                    goto processLine;
                                                }
                                                else
                                                    Program.WriteLine("wrong # of args to team join");
                                                break;
                                            default:
                                                Program.WriteLine("team doesn't understand " + parts[1]);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        Program.WriteLine("team expects more args");
                                    }
                                    break;
                                }
                            default:
                                Program.WriteLine("Command not recognized.");
                                break;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {

            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            cancelConsole = true;
        }
        public static bool cancelConsole = false;

        protected delegate void consoleNetworkOpFunc();
        protected static void consoleNetworkOp(consoleNetworkOpFunc func)
        {
            lock (clientLock)
            {
                if (client != null && client.IsConnected)
                {
                    func();
                }
            }
        }
        protected delegate void consoleGameOpFunc();
        protected static void consoleGameOp(consoleGameOpFunc func)
        {
            lock (clientLock)
            {
                if (client != null && client.IsConnected)
                {
                    lock (gameLock)
                    {
                        func();
                    }
                }
                else
                {
                    Program.WriteLine("not connected.");
                }
            }
        }

        private static void rateThread()
        {
            if (renderer != null)
            {
                while (!cancelConsole)
                {
                    //do not use Program.WriteLine for this
                    Console.Write("\r                                             \rRate: ");
                    if (renderer.secondsPerFrame > 0)
                        Console.Write((1 / renderer.secondsPerFrame).ToString("f2").PadLeft(12));
                    if (renderer.secondsPerFrame2 > 0)
                        Console.Write((1 / renderer.secondsPerFrame2).ToString("f2").PadLeft(12));
                    System.Threading.Thread.Sleep(200);
                }
            }
            else
            {
                Program.WriteLine("renderer not loaded yet.");
            }
            cancelConsole = false;
            Prompt();
        }

        static Thread serverMessageHandlerLoopThread;
        static void doGameCycle()
        {
            using (renderer = new Renderer())
            {
                IM = new InputManager();
                IM.FpsOk = true;

                camera = new Camera();
                new Thread(new ThreadStart(input_loop)).Start();
                
                while (true)
                {
                    Program.WriteLine("Prompting to connect...");
                    lock (clientLock)
                    {
                        client = promptConnect();
                        if (client == null)
                        {
                            onClosed();
                            break;
                        }

                        lock (gameLock)
                        {
                            lobbyState.Mode = GameMode.Init;
                            serverMessageHandlerLoopThread = new Thread(new ThreadStart(serverMessageHandlerLoop));
                            serverMessageHandlerLoopThread.Start();
                        }
                    }

                    bool playAgain = doGame();
                    
                    if (!playAgain)
                        return; //user closed window.   quit.
                    //else, game ended or disconnected.  prompt to connect again.
                }
            }
        }
        static bool doGame()
        {
            GameMode oldMode = GameMode.None;
            while (true)
            {
                lock (clientLock)
                {
                    lock (gameLock)
                    {
                        doDisconnectCheck(oldMode == GameMode.Started || oldMode == GameMode.Paused);
                        bool playAgain1;
                        if (checkDisconnect(false, out playAgain1))
                        {
                            on_disconnected();
                            return playAgain1;
                        }
                        if (lobbyState.Mode != oldMode)
                        {
                            switch (lobbyState.Mode)
                            {
                                case GameMode.Init:
                                    Program.WriteLine("Waiting for other players to join.");
                                    break;
                                case GameMode.Started:
                                    Program.WriteLine("Game started.");
                                    break;
                                case GameMode.Results:
                                    Program.WriteLine("Game finished!");
                                    Program.WriteLine("Winner: " + (lobbyState.WinningTeam == null ? "{draw}" : lobbyState.WinningTeam.Name));
                                    break;
                                case GameMode.Stopping:
                                    Program.WriteLine("Game ended.");
                                    return true; //reconnect
                            }
                            oldMode = lobbyState.Mode;
                        }
                        switch (lobbyState.Mode)
                        {
                            case GameMode.Started:
                            case GameMode.Paused:
                                renderer.GameObjects        = game.GameObjectsCache.Values.ToList<ClientGameObject>();
                                //Update particles
                                foreach(AParticleSpawner ps in game.ParticleSpawners)
                                {
                                    ps.Update();
                                    if (ps.RemoveMe)
                                        PSToRemove.Add(ps);
                                }
                                foreach(AParticleSpawner ps in PSToRemove)
                                    game.ParticleSpawners.Remove(ps);
                                if(PSToRemove.Count > 0)
                                    PSToRemove.Clear();

                                /*DEBUG
                                if (game.ParticleSpawners.Count == 0)
                                {
                                    AParticleSpawner testSpawner = new PSArrow(new Vector4(10, 10, 10));
                                    testSpawner.StartSpawning();
                                    game.ParticleSpawners.Add(testSpawner);
                                }*/
                                renderer.ParticleSpawners   = new List<AParticleSpawner>(game.ParticleSpawners);
                                break;
                        }
                    }
                }
                render();
            }
        }

        static bool _disconnecting;
        static bool _game_ending;
        static bool _play_again;
        static void doDisconnectCheck(bool playing)
        {
            clientLock.AssertHeld();
            gameLock.AssertHeld();

            if (renderer.ShouldExit())
            {
                onClosed();
                _game_ending = true;
                _disconnecting = true;
                _play_again = false;
                return;
            }

            lock (clientLock)
            {
                if (!client.IsConnected)
                {
                    _game_ending = true;
                    _disconnecting = true;
                    _play_again = true;
                    return;
                }
            }

            if (playing)
            {
                if (!(lobbyState.Mode == GameMode.Started || lobbyState.Mode == GameMode.Paused))
                {
                    _play_again = true; //play again
                    _game_ending = true;
                    return;
                }
            }
        }
        static bool checkDisconnect(bool playing, out bool playAgain)
        {
            playAgain = _play_again;
            return playing ? _game_ending : _disconnecting;
        }

        static void input_loop()
        {
            lock (IM.Lock)
            {
                while (true)
                {
                    lock (clientLock)
                    {
                        if (closing)
                            return;

                        lock (gameLock)
                        {
                            lock (camera.Lock)
                            {
                                camera.Update(localPlayer);
                                if (localPlayer != null)
                                    localPlayer.Update(IM, game, camera);
                            }
                        }
                        if (localPlayer != null)
                        {
                            if (localPlayer.NetworkEvents.Count > 0)
                            {
                                sendEvents(localPlayer.NetworkEvents);
                                localPlayer.NetworkEvents.Clear();
                            }
                        }
                    }
                    Monitor.Wait(IM.Lock, 3);
                }
            }
        }

        static void on_disconnected()
        {
            clientLock.AssertHeld();
            gameLock.AssertHeld();

            lock (client.ServerMessagesLock)
            {
                Monitor.PulseAll(client.ServerMessagesLock);
            }
            serverMessageHandlerLoopThread.Join();
            _disconnecting = false;
            _game_ending = false;

            client = null;
            game = null;
            lobbyState.Mode = GameMode.None;
            renderer.GameObjects = null;
            Program.WriteLine("Disconnected.");
        }

        static string lastHost;
        static int? lastPort;
        static Client promptConnect()
        {
            ConnectPrompter prompter = new ConnectPrompter();
            lock (prompter.Lock)
            {
                prompter.Host = lastHost ?? globalConfig.GetSetting("server-host", BB.DefaultServerHost);
                prompter.Port = lastPort ?? int.Parse(globalConfig.GetSetting("server-port", BB.DefaultServerPort));

                lock (clientLock)
                {
                    client = new Client(clientLock);

                    try
                    {
                        //try to autoconnect
                        client.Connect(prompter.Host, prompter.Port);
                        return client;
                    }
                    catch
                    {
                        //autoconnect failed.  continue to prompt user.
                    }
                }

                prompter.BeginPrompt();

                while (prompter.cont)
                {
                    if (prompter.TryToConnect)
                    {
                        try
                        {
                            lock (clientLock)
                            {
                                client = new Client(clientLock);
                                client.Connect(prompter.Host, prompter.Port);
                                
                                lastHost = prompter.Host; //remember, if succeeded
                                lastPort = prompter.Port;
                                addHost(prompter.Host, prompter.Port);

                                prompter.connectedCallback();
                                return client;
                            }
                        }
                        catch(Exception ex)
                        {
                            client = null;
                            if (ex.Message.StartsWith("No connection could be made because"))
                                prompter.errorCallback(ex.Message);
                            else
                                prompter.errorCallback(ex.ToString());
                        }
                    }
                    Monitor.Wait(prompter.Lock);
                }

                return null;
            }
        }

        public static string GetHostsFile() { return Path.Combine(BBXml.GetLocalConfigFile("hosts")); }
        private static void addHost(string p1, int p2)
        {
            string file = GetHostsFile();
            string host = p1 + ":" + p2;
            string[] f;
            if (File.Exists(file))
                f = File.ReadAllLines(file);
            else 
                f = new string[] {};

            if (!f.Contains(host))
            {
                List<string> hosts = new List<string>(f);
                hosts.Add(host);
                File.WriteAllLines(file, hosts.ToArray());
            }
        }

        class ConnectPrompter
        {
            public BBLock Lock = new BBLock();

            public bool cont = true;
            
            public bool TryToConnect;
            public string Host;
            public int Port;
            frmConnect form;

            Thread prompterThread;

            public void BeginPrompt()
            {
                Lock.AssertHeld();
                prompterThread = new Thread(new ThreadStart(promptThread));
                prompterThread.Start();
            }

            void promptThread()
            {
                using (form = new frmConnect())
                {
                    form.Host = Host;
                    form.Port = Port;
                    form.ConnectClicked += prompt_ConnectClicked;
                    form.ShowDialog();

                    lock (Lock)
                    {
                        cont = false;
                        Monitor.PulseAll(Lock);
                    }
                }
            }

            void prompt_ConnectClicked(object sender, EventArgs e)
            {
                lock (Lock)
                {
                    Host = form.Host;
                    Port = form.Port;
                    TryToConnect = true;
                    Monitor.PulseAll(Lock);
                }
            }

            public void connectedCallback()
            {
                Lock.AssertHeld();
                form.Invoke(new ThreadStart(form.Close));
            }

            public void errorCallback(string message)
            {
                form.Invoke(new ThreadStart(() => { MessageBox.Show(message); form.Enabled = true; }));
            }
        }

        static void render()
        {
            renderer.Render(localPlayer);
        }

        static void serverMessageHandlerLoop()
        {
            List<ServerMessage> serverMessages;

            while (true)
            {
                var mylock = client.ServerMessagesLock;
                lock (mylock)
                {
                    while(client.ServerMessages.Count == 0)
                    {
                        bool b;
                        if (checkDisconnect(false, out b))
                            return;

                        Debug.Assert(mylock == client.ServerMessagesLock);
                        Monitor.Wait(mylock);
                    }
                    serverMessages = new List<ServerMessage>(client.ServerMessages);
                    client.ServerMessages.Clear();
                }

                foreach (var m in serverMessages)
                {
                    if (m is ServerGameModeUpdateMessage)
                    {
                        var msg = (ServerGameModeUpdateMessage)m;

                        lock (gameLock)
                        {
                            lobbyState.Mode = msg.Mode;
                            switch (lobbyState.Mode)
                            {
                                case GameMode.Init:
                                    break;
                                case GameMode.Started:
                                    game = new ClientGame(gameLock);
                                    break;
                                case GameMode.Results:
                                    MessageBox.Show(lobbyState.WinningTeam == null ? "Draw!  Neither team wins." : lobbyState.WinningTeam.Name + " wins!");
                                    if(lobbyState.WinningTeam != null && lobbyState.WinningTeam.Clients.Contains("hi"))
                                    {
                                        int volume = (int)Math.Log(4, 2.0);
                                        SoundThing.Play(BBSound.explosionfar, volume);
                                    }
                                    else
                                    {
                                        int volume = (int)Math.Log(4, 2.0);
                                        SoundThing.Play(BBSound.sadtrombone, volume);
                                    }

                                    break;
                                case GameMode.Stopping:
                                    break;
                                default:
                                    throw new Exception("client doesn't understand game mode: " + lobbyState.Mode.ToString());
                                    //client.Disconnect();
                                    //return;
                            }
                        }
                    }
                    else if (m is ServerGameStateUpdateMessage)
                    {
                        var msg = (ServerGameStateUpdateMessage)m;

                        lock (gameLock)
                        {
                            msg.Read((reader) =>
                            {
                                game.GameTime = new TimeSpan((long)reader.ReadInt64());

                                int len = reader.ReadInt32();
                                for (int i = 0; i < len; i++)
                                {
                                    int id = reader.ReadInt32();
                                    ClientGameObject obj = ClientGameObject.Deserialize(id, reader, game);
                                    game.LiveGameObjects.Add(obj.Id, obj);
                                }
                                len = reader.ReadInt32();
                                for (int i = 0; i < len; i++)
                                {
                                    int id = reader.ReadInt32();
                                    game.LiveGameObjects[id].StreamUpdate(reader);
                                }
                                len = reader.ReadInt32();
                                for (int i = 0; i < len; i++)
                                {
                                    int id = reader.ReadInt32();
                                    game.LiveGameObjects.Remove(id);
                                }
                                // event reading
                                len = reader.ReadInt32();
                                for(int i = 0; i < len; i++)
                                {
                                    switch ((ServerMessageType)reader.ReadByte())
                                    {
                                        case ServerMessageType.GoalsUpdate:
                                        {
                                            var e = new ServerGoalsUpdateMessage();
                                            e.Read(reader);
                                            game.Goals.Clear();
                                            foreach (var g in e.Goals)
                                                game.Goals.Add(game.Config.Ingredients[g]);
                                            game.Goals.Sort();
                                            break;
                                        }
                                        case ServerMessageType.TintListUpdate:
                                        {
                                            var e = new ServerTintListUpdateMessage();
                                            e.Read(reader);
                                            var lst = game.TintedObjects[e.Team];
                                            foreach (var tintIng in e.TintList)
                                                lst.Add(tintIng);
                                            break;
                                        }
                                        case ServerMessageType.Sound:
                                        {
                                            var e = new ServerSoundMessage();
                                            e.Read(reader);
                                            var playerPos = localPlayer.GetPosition();
                                            var soundPos = e.Location;
                                            double distance = getDistance(playerPos, soundPos);
                                            int volume = (int)Math.Log(distance, 2.0);
                                            SoundThing.Play(e.Sound, volume);
                                            break;
                                        }
                                        case ServerMessageType.ParticleEffect:
                                        {
                                            var e = new ServerParticleEffectMessage();
                                            e.Read(reader);
                                            AParticleSpawner spawner = null;
                                            switch(e.ParticleEffect)
                                            {
                                                case BBParticleEffect.CONFETTI:
                                                    spawner = new PSConfetti(e.Location);
                                                    break;
                                                case BBParticleEffect.SMOKE:
                                                    spawner = new PSSmoke(e.Location, (SmokeType) e.Param);
                                                    break;
                                                case BBParticleEffect.SPARKS:
                                                    spawner = new PSSparks(e.Location);
                                                    break;
                                                case BBParticleEffect.SPLASH:
                                                    break;
                                                case BBParticleEffect.STARS:
                                                    break;
                                                case BBParticleEffect.ARROW:
                                                    if(e.Id == game.PlayerObjId)
                                                        spawner = new PSArrow(e.Location);
                                                    break;
                                            }
                                            if(spawner != null)
                                            {
                                                game.ParticleSpawners.Add(spawner);
                                                spawner.StartSpawning();
                                            }
                                            break;
                                        }
                                        default:
                                            throw new Exception("No event like that.");
                                    }
                                    
                                }

                            });
                            
                            game.GameObjectsCache = new Dictionary<int,ClientGameObject>(game.LiveGameObjects);
                        }
                    }
                    else if (m is ServerLobbyStateUpdateMessage)
                    {
                        var msg = (ServerLobbyStateUpdateMessage)m;
                        lock(gameLock)
                        {
                            msg.Read((r) =>
                            {
                                int teamCount = r.ReadInt32();

                                var oldTeams = lobbyState.Teams;
                                lobbyState.Teams = new Dictionary<string, ClientTeam>();
                                for(int i = 0; i < teamCount; i++)
                                {
                                    var name = r.ReadString();
                                    ClientTeam team;
                                    if (!oldTeams.TryGetValue(name, out team))
                                        team = new ClientTeam() { Name = name };
                                    team.Score = r.ReadInt32();
                                    
                                    int memberCount = r.ReadInt32();
                                    team.Clients.Clear();
                                    for (int j = 0; j < memberCount; j++)
                                        team.Clients.Add(r.ReadString());
                                    lobbyState.Teams.Add(name, team);
                                }
#if PROJECT_DEBUG
                                foreach (var team in lobbyState.Teams.Values)
                                    Program.WriteLine(team.Name + " Has " + team.Score); 
#endif
                                string myTeam = r.ReadString();
                                lobbyState.MyTeam = lobbyState.Teams[myTeam];
                                lobbyState.MaxScore = r.ReadInt32();
                                lobbyState.MaxTime = new TimeSpan(r.ReadInt64());
                                if (r.ReadBoolean())
                                    lobbyState.WinningTeam = lobbyState.Teams[r.ReadString()];
                                else
                                    lobbyState.WinningTeam = null;
                            });
                        }
                    }
                    else if (m is ServerPlayerIdUpdateMessage)
                    {
                        var msg = (ServerPlayerIdUpdateMessage)m;
                        lock (gameLock)
                        {
                            game.PlayerObjId = msg.PlayerId;
                        }
                    }
                    else if (m is ServerCommandResponseMessage)
                    {
                        var msg = (ServerCommandResponseMessage)m;
                        clientConsole.BeginWrite();
                        clientConsole.Write("server: ", ConsoleColor.Yellow);
                        clientConsole.Write(msg.Result + "\n");
                        clientConsole.EndWrite();
                    }
                    else
                    {
                        throw new Exception("client does not understand message type " + m.GetType().ToString());
                    }
                }
            }
        }

        public static double getDistance(Vector4 p1, Vector4 p2)
        {
            return
                Math.Pow(
                Math.Pow(p1.X - p2.X, 2.0) +
                Math.Pow(p1.Y - p2.Y, 2.0) +
                Math.Pow(p1.Z - p2.Z, 2.0),
                0.5);
        }

        static void sendEvents(List<ClientEvent> events)
        {
            clientLock.AssertHeld();
            lock (client.ClientEvents)
            {
                client.ClientEvents.AddRange(events);
                Monitor.PulseAll(client.ClientEvents);
            }
        }
        static void sendEvent(ClientEvent @event)
        {
            clientLock.AssertHeld();
            lock (client.ClientEvents)
            {
                client.ClientEvents.Add(@event);
                Monitor.PulseAll(client.ClientEvents);
            }
        }

        static bool closing;
        static void onClosed()
        {
            clientLock.AssertHeld();
            if (client != null)
            {
                if (client.IsConnected) //check if connected because we don't know if we will start a disconnection by closing, or if we're closing because we got disconnected.
                {
                    client.Disconnect();
                }
            }
            closing = true;
        }

        public static BBConsole clientConsole = new BBConsole("client", ConsoleColor.Green);
        public static void ClearLine()
        {
            clientConsole.ClearLine();
        }
        public static void WriteLine(string line)
        {
            clientConsole.WriteLine(line);
        }
        public static void Prompt()
        {
            clientConsole.Prompt();
        }

        // P/Invoke:
        private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(StdHandle std);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hdl);

    }
}
