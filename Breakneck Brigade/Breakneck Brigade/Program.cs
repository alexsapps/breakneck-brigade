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
        public static BBLock ProgramLock = new BBLock();

        static Client client;
        static ClientGame _game;
        static ClientGame game
        {
            get
            {
                ProgramLock.AssertHeld();
                return _game;
            }
            set
            {
                ProgramLock.AssertHeld();
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
        static GameMode gameMode;
        static Renderer renderer;

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
                                lock (ProgramLock)
                                {
                                    if (client != null)
                                    {
                                        lock (client.Lock)
                                        {
                                            client.Disconnect();
                                        }
                                    }
                                    Environment.Exit(0);
                                }
                                break;
                            case "status":
                                lock(ProgramLock)
                                {
                                    if (client == null)
                                    {
                                        Console.WriteLine("Not connected.");
                                    }
                                    else
                                    {   
                                        Console.WriteLine("Connected.");
                                        Console.WriteLine("GameMode: " + gameMode.ToString());
                                        if (gameMode == GameMode.Started || gameMode == GameMode.Paused)
                                        {
                                            Console.WriteLine(game.gameObjects.Count + " game objects.");
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

        static void doGameCycle()
        {
            using (renderer = new Renderer())
            {
                IM = new InputManager();
                IM.EnableFPSMode();
                
                while (true)
                {
                    Console.WriteLine("Prompting to connect...");
                    client = promptConnect();
                    if (client == null)
                        break;
                    
                    lock (ProgramLock)
                    {
                        gameMode = GameMode.Init;
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
                if (renderer.ShouldExit())
                {
                    onClosed();
                    return false; //quit
                }
                lock (client.Lock)
                {
                    if (!client.IsConnected)
                        return true; //play again    
                }

                lock (ProgramLock)
                {
                    if (gameMode != oldMode)
                    {
                        switch (gameMode)
                        {
                            case GameMode.Init:
                                Console.WriteLine("Waiting for other players to join.");
                                break;
                            case GameMode.Started:
                                Console.WriteLine("Game started.");
                                bool playAgain;
                                Monitor.Exit(ProgramLock);
                                playAgain = play();
                                Monitor.Enter(ProgramLock);
                                renderer.GameObjects = null;
                                return playAgain;
                            case GameMode.Stopping:
                                gameMode = GameMode.None;
                                game = null;
                                Console.WriteLine("Game ended.");
                                on_disconnected();
                                return true; //reconnect
                        }
                        oldMode = gameMode;
                    }
                    handleServerMessages();
                    render();
                }
            }
        }
        static bool play()
        {
            //game will eventually become null, but this will be after GameMode set to stopping while lock held on gameObjects
            while (true)
            {
                lock (ProgramLock)
                {
                    if (renderer.ShouldExit())
                    {
                        onClosed();
                        return false; //quit
                    }
                    lock (client.Lock)
                    {
                        if (!client.IsConnected)
                            return true; //play again
                    }

                    if (!(gameMode == GameMode.Started || gameMode == GameMode.Paused))
                        return true; //play again

                    lock (game.Lock)
                    {
                        renderer.GameObjects = game.gameObjects.Values.ToList<ClientGameObject>();

                        cPlayer.Update(IM, game.gameObjects, renderer.getCamera());
                        if (cPlayer.NetworkEvents.Count > 0)
                        {
                            sendEvents(cPlayer.NetworkEvents);
                            cPlayer.NetworkEvents.Clear();
                        }
                    }

                    handleServerMessages();
                    render();
                }
            }
        }

        static void on_disconnected()
        {
            ProgramLock.AssertHeld();
            client = null;
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
                            client = new Client();
                            lock (client.Lock)
                            {
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
            renderer.Render(cPlayer);
        }

        static void handleServerMessages()
        {
            ProgramLock.AssertHeld();

            List<ServerMessage> serverMessages;
            lock (client.serverMessages)
            {
                serverMessages = new List<ServerMessage>(client.serverMessages);
                client.serverMessages.Clear();
            }
            foreach(var m in serverMessages)
            {
                if(m is ServerGameModeUpdateMessage)
                {
                    var msg = (ServerGameModeUpdateMessage)m;
                    
                    gameMode = msg.Mode;
                    switch (gameMode)
                    {
                        case GameMode.Init:
                            break;
                        case GameMode.Started:
                            game = new ClientGame();
                            break;
                        case GameMode.Stopping:
                            break;
                        default:
                            client.Disconnect();
                            return;
                    }
                }
                else if (m is ServerGameStateUpdateMessage)
                {
                    var msg = (ServerGameStateUpdateMessage)m;

                    lock (game.Lock)
                    {
                        using (MemoryStream mem = new MemoryStream(msg.Binary))
                        {
                            using (BinaryReader reader = new BinaryReader(mem))
                            {
                                int len = reader.ReadInt32();
                                for (int i = 0; i < len; i++)
                                {
                                    int id = reader.ReadInt32();
                                    ClientGameObject obj = ClientGameObject.Deserialize(id, reader, game);
                                    game.gameObjects.Add(obj.Id, obj);
                                }
                                len = reader.ReadInt32();
                                for (int i = 0; i < len; i++)
                                {
                                    int id = reader.ReadInt32();
                                    game.gameObjects[id].StreamUpdate(reader);
                                }
                                len = reader.ReadInt32();
                                for (int i = 0; i < len; i++)
                                {
                                    int id = reader.ReadInt32();
                                    game.gameObjects.Remove(id);
                                }
                            }
                        }
                    }
                }
                else if(m is ServerPlayerIdUpdateMessage)
                {
                    var msg = (ServerPlayerIdUpdateMessage)m;    
                    lock (game.Lock)
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

        static void sendEvents(List<ClientEvent> @events)
        {
            client.Lock.AssertHeld();
            if (gameMode == GameMode.Started)
            {
                lock (client.ClientEvents)
                {
                    client.ClientEvents.AddRange(@events);
                    Monitor.PulseAll(client.ClientEvents);
                }
            }
            else
            {
                MessageBox.Show("game hasn't started yet.  first, issue 'play' command to server.");
            }
        }

        static void onClosed()
        {
            if (client != null)
            {
                lock (client.Lock)
                {
                    if (client.IsConnected) //check if connected because we don't know if we will start a disconnection by closing, or if we're closing because we got disconnected.
                    {
                        client.Disconnect();
                    }
                }
            }
        }

        // P/Invoke:
        private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(StdHandle std);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hdl);

    }
}
