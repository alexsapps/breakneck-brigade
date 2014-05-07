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
        static Client client; static BBLock clientLock = new BBLock();
        static ClientGame _game;
        static ClientGame game
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
                    cPlayer = new LocalPlayer();
                    cPlayer.Game = _game;
                }
                else
                {
                    cPlayer = null;
                }
            }
        }
        static BBLock gameLock = new BBLock();

        static GameMode gameMode;
        static Renderer renderer;
        static Camera camera;

        static GlobalsConfigFolder config = new GlobalsConfigFolder();
        static GlobalsConfigFile globalConfig;

        static public LocalPlayer cPlayer;
        static public InputManager IM;

        static void Main(string[] args)
        {

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
#if PROJECT_GAMECODE_TEST

            globalConfig = config.Open(BB.GlobalConfigFilename);

            

            Thread inputThread = null;
            inputThread = new Thread(new ThreadStart(readInput));
            inputThread.Start();

            //Thread appThread = null;
            //appThread = new Thread(new ThreadStart(doGameCycle));
            //appThread.Start();

            //appThread.Join();

            doGameCycle();

            CloseHandle(GetStdHandle(StdHandle.Stdin)); //terminate input thread
            inputThread.Abort();
#endif
        }

        static void readInput()
        {
            try
            {
                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == null)
                        return; //thread asked to abort
                    string[] parts = line.Split(new string[] { Environment.NewLine }, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        switch (parts[0])
                        {
                            case "exit":
                                lock (clientLock)
                                {
                                    if (client != null)
                                    {
                                        client.Disconnect();
                                    }
                                }
                                Environment.Exit(0);
                                
                                break;
                            case "status":
                                lock(clientLock)
                                {
                                    if (client == null || !client.IsConnected)
                                    {
                                        Console.WriteLine("Not connected.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Connected.");
                                        lock (gameLock)
                                        {
                                            Console.WriteLine("GameMode: " + gameMode.ToString());
                                            if (gameMode == GameMode.Started || gameMode == GameMode.Paused)
                                            {
                                                Console.WriteLine(game.gameObjects.Count + " game objects.");
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                Console.WriteLine("Command not recognized.");
                                break;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {

            }
        }

        static Thread serverMessageHandlerLoopThread;
        static void doGameCycle()
        {
            using (renderer = new Renderer())
            {
                IM = new InputManager();
                IM.EnableFPSMode();

                camera = new Camera();
                new Thread(new ThreadStart(input_loop)).Start();
                
                while (true)
                {
                    Console.WriteLine("Prompting to connect...");
                    lock (clientLock)
                    {
                        client = promptConnect();
                        if (client == null)
                            break;

                        lock (gameLock)
                        {
                            gameMode = GameMode.Init;
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
                        if (gameMode != oldMode)
                        {
                            switch (gameMode)
                            {
                                case GameMode.Init:
                                    Console.WriteLine("Waiting for other players to join.");
                                    break;
                                case GameMode.Started:
                                    Console.WriteLine("Game started.");
                                    break;
                                case GameMode.Stopping:
                                    Console.WriteLine("Game ended.");
                                    return true; //reconnect
                            }
                            oldMode = gameMode;
                        }
                        switch (gameMode)
                        {
                            case GameMode.Started:
                            case GameMode.Paused:
                                renderer.GameObjects = game.gameObjects.Values.ToList<ClientGameObject>();
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
                if (!(gameMode == GameMode.Started || gameMode == GameMode.Paused))
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
                                camera.Update(cPlayer);
                                if (cPlayer != null)
                                    cPlayer.Update(IM, game, camera);
                            }
                        }
                        if (cPlayer != null)
                        {
                            if (cPlayer.NetworkEvents.Count > 0)
                            {
                                sendEvents(cPlayer.NetworkEvents);
                                cPlayer.NetworkEvents.Clear();
                            }
                        }
                    }
                    Monitor.Wait(IM.Lock, 1);
                }
            }
        }

        static void on_disconnected()
        {
            clientLock.AssertHeld();
            gameLock.AssertHeld();

            lock (client.ServerMessages)
            {
                Monitor.PulseAll(client.ServerMessages);
            }
            serverMessageHandlerLoopThread.Join();
            _disconnecting = false;
            _game_ending = false;

            client = null;
            game = null;
            gameMode = GameMode.None;
            renderer.GameObjects = null;
            Console.WriteLine("Disconnected.");
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
            renderer.Render(camera);
        }

        static void serverMessageHandlerLoop()
        {
            List<ServerMessage> serverMessages;

            while (true)
            {
                lock (client.ServerMessages)
                {
                    while(client.ServerMessages.Count == 0)
                    {
                        bool b;
                        if (checkDisconnect(false, out b))
                            return;

                        Monitor.Wait(client.ServerMessages);
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
                            gameMode = msg.Mode;
                            switch (gameMode)
                            {
                                case GameMode.Init:
                                    break;
                                case GameMode.Started:
                                    game = new ClientGame(gameLock);
                                    break;
                                case GameMode.Stopping:
                                    break;
                                default:
                                    client.Disconnect();
                                    return;
                            }
                        }
                    }
                    else if (m is ServerGameStateUpdateMessage)
                    {
                        var msg = (ServerGameStateUpdateMessage)m;

                        Dictionary<int, ClientGameObject> gos;
                        lock (gameLock)
                        {
                            gos = new Dictionary<int,ClientGameObject>(game.gameObjects);
                        }
                        using (MemoryStream mem = new MemoryStream(msg.Binary))
                        {
                            using (BinaryReader reader = new BinaryReader(mem))
                            {
                                int len = reader.ReadInt32();
                                for (int i = 0; i < len; i++)
                                {
                                    int id = reader.ReadInt32();
                                    ClientGameObject obj = ClientGameObject.Deserialize(id, reader, game);
                                    gos.Add(obj.Id, obj);
                                }
                                len = reader.ReadInt32();
                                for (int i = 0; i < len; i++)
                                {
                                    int id = reader.ReadInt32();
                                    gos[id].StreamUpdate(reader);
                                }
                                len = reader.ReadInt32();
                                for (int i = 0; i < len; i++)
                                {
                                    int id = reader.ReadInt32();
                                    gos.Remove(id);
                                }
                            }
                        }
                        lock(gameLock)
                        {
                            game.gameObjects = gos;
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
                    else
                    {
                        throw new Exception("client does not understand message type " + m.GetType().ToString());
                    }
                }
            }
        }

        static void sendEvents(List<ClientEvent> @events)
        {
            clientLock.AssertHeld();
            lock (client.ClientEvents)
            {
                client.ClientEvents.AddRange(@events);
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

        // P/Invoke:
        private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(StdHandle std);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hdl);

    }
}
