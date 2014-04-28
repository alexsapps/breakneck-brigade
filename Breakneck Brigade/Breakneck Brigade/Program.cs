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
        static Renderer renderer;

        static GlobalsConfigFolder config = new GlobalsConfigFolder();
        static GlobalsConfigFile globalConfig;

        static public ClientPlayer cPlayer = new ClientPlayer();
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
                                        lock (client.Lock)
                                        {
                                            Console.WriteLine("Connected.");
                                            Console.WriteLine("GameMode: " + client.GameMode.ToString());
                                            if (client.GameMode == GameMode.Started || client.GameMode == GameMode.Paused)
                                            {
                                                Console.WriteLine(client.Game.gameObjects.Count + " game objects.");
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

        static void doGameCycle()
        {
            using (renderer = new Renderer())
            {
                while (true)
                {
                    Console.WriteLine("Prompting to connect...");
                    client = promptConnect();
                    if (client == null)
                        break;

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

                lock (ProgramLock)
                {
                    if (client.GameMode != oldMode)
                    {
                        switch (client.GameMode)
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
                                Console.WriteLine("Game ended.");
                                on_disconnected();
                                return true; //reconnect
                        }
                        oldMode = client.GameMode;
                    }
                }
                render();
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
                        if (!(client.GameMode == GameMode.Started || client.GameMode == GameMode.Paused))
                            return true; //play again

                        lock (client.Game.Lock)
                        {
                            renderer.GameObjects = client.Game.gameObjects.Values.ToList<ClientGameObject>();

                            render();
                        }
                    }
                }
            }
        }

        static void on_disconnected()
        {
            ProgramLock.AssertHeld();
            client = null;
            Console.WriteLine("Disconnected.");
        }

        static Client promptConnect()
        {
            ConnectPrompter prompter = new ConnectPrompter();
            lock (prompter.Lock)
            {
                prompter.Host = globalConfig.GetSetting("server-host", BB.DefaultServerHost);
                prompter.Port = int.Parse(globalConfig.GetSetting("server-port", BB.DefaultServerPort));

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

                                IM = new InputManager();
                                IM.EnableFPSMode();

                                prompter.connectedCallback();
                                return client;
                            }
                        }
                        catch(Exception ex)
                        {
                            client = null;
                            prompter.errorCallback(ex.ToString());
                        }
                    }
                    Monitor.Wait(prompter.Lock);
                }

                return null;
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
            cPlayer.Update(IM);
            renderer.Render(cPlayer);
        }

        static void sendEvent(ClientEvent @event)
        {
            lock (client.Lock)
            {
                if (client.GameMode == GameMode.Started)
                {
                    lock (client.ClientEvents)
                    {
                        client.ClientEvents.Add(@event);
                        Monitor.PulseAll(client.ClientEvents);
                    }
                }
                else
                {
                    MessageBox.Show("game hasn't started yet.  first, issue 'play' command to server.");
                }
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
