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
        static Client client;

        static object ProgramLock = new object();

        static GlobalsConfigFolder config = new GlobalsConfigFolder();
        static GlobalsConfigFile globalConfig;

        static void Main(string[] args)
        {

#if PROJECT_GRAPHICS_TEST
            Renderer renderer = new Renderer();
            while (!renderer.ShouldExit())
            {
                renderer.Render();
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
            while (true)
            {
                Console.WriteLine("Prompting to connect...");
                client = promptConnect();
                if (client == null)
                    break;

                lock (ProgramLock)
                {
                    while (!disconnecting)
                    {
                        if (client.GameMode == GameMode.Init)
                        {
                            Console.WriteLine("Waiting for other players to join.");
                        }
                        else if (client.GameMode == GameMode.Started)
                        {
                            Console.WriteLine("Game started.");
                            //Thread playThread = new Thread(new ThreadStart(play));
                            //playThread.Start();
                            play();
                            break; //game ended
                        }
                        else if (client.GameMode == GameMode.Stopping)
                        {
                            Console.WriteLine("Game ended.");
                            break; //reconnect
            }

                        Monitor.Wait(ProgramLock);
                    }
                    Console.WriteLine("Disconnected.");
                }
            }
        }


        static void client_GameModeChanged(object sender, EventArgs e)
        {
            lock (ProgramLock)
            {
                Monitor.PulseAll(ProgramLock);
            }
        }

        static void client_Disconnected(object sender, EventArgs e)
        {
            lock (ProgramLock)
            {
                disconnecting = true; //close play thread and UI thread
                client = null;
                Monitor.PulseAll(ProgramLock); //close UI / render thread
        }
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
                                disconnecting = false;
                                client.Disconnected += client_Disconnected;
                                client.GameModeChanged += client_GameModeChanged;
                                client.Connect(prompter.Host, prompter.Port);
                                prompter.connectedCallback();
                                return client;
                            }
                        }
                        catch(Exception ex)
                        {
                            client.Disconnected -= client_Disconnected;
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

        static bool disconnecting;

        static BBLock renderLock = new BBLock();

        static void play()
        {
            ClientGame game;
            ClientPlayer cPlayer;
            InputManager IM;

            lock (client.Lock)
            {
                game = client.Game;
                cPlayer = new ClientPlayer();
                IM = new InputManager();
            }

            Debug.Assert(client.Game != null);

            //game will eventually become null, but this will be after GameMode set to stopping while lock held on gameObjects

            Renderer renderer;
            lock(client.Lock) {
                if (!disconnecting)
                    renderer = new Renderer(client.Game);
                else
                    return;
            }

            using (renderer)
            {
                while (true)
                {
                    if (renderer.ShouldExit())
                    {
                        onClosed();
                        break;
                    }

                    lock (client.Lock)
                    {
                        if (disconnecting)
                            break;

                        if (client.GameMode == GameMode.Stopping)
                            break;

                        lock (client.Game.Lock)
                        {
                            cPlayer.Update(IM);
                            renderer.Render(cPlayer);
                        }
                    }

                    //Monitor.Wait(ProgramLock); //we *must* check if(disconnecting) after this returns
                }   
            }
            
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
