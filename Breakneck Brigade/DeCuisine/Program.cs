using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeCuisine
{
    class Program
    {
        static Server server;
        static GlobalsConfigFolder config = new GlobalsConfigFolder();
        static GlobalsConfigFile globalConfig;

        static void Main(string[] args)
        {
            try { Console.SetWindowSize(120, 40); } catch { } //see also client's Main()
            Mutex mutex = new Mutex(true, "ccf299f3-1ea2-48e1-84bd-72d1de57fbeb");
            if (!mutex.WaitOne(TimeSpan.Zero, true)) //http://sanity-free.org/143/csharp_dotnet_single_instance_application.html
            {
                Program.WriteLine("Already started.");
                System.Threading.Thread.Sleep(600);
                return;
            }

            globalConfig = config.Open("global-config.xml");
            int port = int.Parse(globalConfig.GetSetting("server-port", BB.DefaultServerPort));

            server = new Server(IPAddress.Any, port);
            lock (server.Lock)
            {
                start(); //start automatically
                lock (server.Game.Lock)
                {
                    server.Game.Start(); //play automatically to make debugging easier.
                    Program.WriteLine("Game started automatically.");
                }
            }

            Console.CancelKeyPress += Console_CancelKeyPress;

            string line;
            
            while ((line = Console.ReadLine()) != null)
            {
                try
                {
                    String[] parts = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    switch (parts[0])
                    {
                        case "cancel":
                            {
                                cancelConsole = true;
                                break;
                            }
                        case "stop":
                            lock (server.Lock)
                            {
                                if (server.Started)
                                {
                                    stop();
                                }
                                else
                                {
                                    Program.WriteLine("Already stopped.");
                                }
                            }
                            break;
                        case "restart":
                            lock (server.Lock)
                            {
                                if (server.Started)
                                {
                                    stop();
                                }
                                start();
                            }
                            break;
                        case "start":
                            lock (server.Lock)
                            {
                                if (server.Started)
                                    Program.WriteLine("Already started.");
                                else
                                    start();
                            }
                            break;
                        case "play":
                            lock (server.Lock)
                            {
                                if (server.Started)
                                {
                                    lock (server.Game.Lock)
                                    {
                                        if (server.Game.Mode == GameMode.Init)
                                            server.Game.Start();
                                        else
                                            Program.WriteLine("Game already in " + server.Game.Mode.ToString() + " mode.");
                                    }
                                }
                                else
                                    Program.WriteLine("Server not started.");
                            }
                            break;
                        case "reconfig":
                        case "reload":
                            lock (server.Lock)
                            {
                                server.ReloadConfig();
                            }
                            break;
                        case "set":
                            if (parts.Length < 3)
                            {
                                Program.WriteLine("set expects at least two arguments.");
                                break;
                            }
                            lock (server.Lock)
                            {
                                if (server.Game != null)
                                {
                                    lock (server.Game.Lock)
                                    {
                                        switch (parts[1])
                                        {
                                            case "tick":
                                                int val = int.Parse(parts[2]);
                                                if (val > 0)
                                                    server.Game.FrameRateMilliseconds = val;
                                                else
                                                    Program.WriteLine("ticks must be more than 0.");
                                                break;
                                            default:
                                                {
                                                    Program.WriteLine("set doesn't understand " + parts[1]);
                                                    break;
                                                }
                                        }
                                    }
                                }
                                else
                                {
                                    Program.WriteLine("game must be started to set this variable.");
                                }
                            }
                            break;
                        case "status":
                            lock (server.Lock)
                            {
                                server.PrintStatus();
                            }
                            break;
                        case "exit":
                            lock (server.Lock)
                            {
                                if (server.Started)
                                {
                                    stop();
                                }
                            }
                            cancelConsole = true;
                            return;


                        default:
                            if (!CommandLinePlayer.ReadArgs(parts, server)) 
                                Program.WriteLine(String.Format("Breakneck Brigade server does not understand command: {0}", parts[0]));
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Program.WriteLine("Error:  " + ex.ToString());
                }
            }
        }

        public static bool cancelConsole;
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            cancelConsole = true;
        }
            
        static void start() {
            server.Lock.AssertHeld();

            Program.WriteLine("Starting on port " + server.Port);
            server.Start();
        }

        static void stop()
        {
            server.Lock.AssertHeld();

            server.Stop();
            Program.WriteLine("Stopped.");
        }

        static BBConsole serverConsole = new BBConsole("server", ConsoleColor.Yellow);
        public static void ClearLine()
        {
            serverConsole.ClearLine();
        }
        public static void WriteLine(string line)
        {
            serverConsole.WriteLine(line);
        }
        public static void Prompt()
        {
            serverConsole.Prompt();
        }
    }
}
