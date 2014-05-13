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

        static GameMode gameMode;
        static Renderer renderer;
        static Camera camera;

        static GlobalsConfigFolder config = new GlobalsConfigFolder();
        static GlobalsConfigFile globalConfig;

        static public LocalPlayer localPlayer;
        static public InputManager IM;

        static void Main(string[] args)
        {
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
                                            Program.WriteLine("GameMode: " + gameMode.ToString());
                                            if (gameMode == GameMode.Started || gameMode == GameMode.Paused)
                                            {
                                                Program.WriteLine(game.gameObjects.Count + " game objects.");
                                            }
                                        }
                                    }
                                }
                                break;
                            case "r":
                            case "rate":
                                new Thread(() => { rateThread(); }).Start();
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

        private static void rateThread()
        {
            if (renderer != null)
            {
                while (!cancelConsole)
                {
                    //do not use Program.WriteLine for this
                    Console.Write("\r                                             \rRate: ");
                    if (renderer.secondsPerFrame > 0)
                        Console.Write(1 / renderer.secondsPerFrame);
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
                                    Program.WriteLine("Waiting for other players to join.");
                                    break;
                                case GameMode.Started:
                                    Program.WriteLine("Game started.");
                                    break;
                                case GameMode.Stopping:
                                    Program.WriteLine("Game ended.");
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
            gameMode = GameMode.None;
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
                lock (client.ServerMessagesLock)
                {
                    while(client.ServerMessages.Count == 0)
                    {
                        bool b;
                        if (checkDisconnect(false, out b))
                            return;

                        Monitor.Wait(client.ServerMessagesLock);
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
