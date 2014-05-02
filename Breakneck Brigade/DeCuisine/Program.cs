using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            globalConfig = config.Open("global-config.xml");
            int port = int.Parse(globalConfig.GetSetting("server-port", BB.DefaultServerPort));

            server = new Server(IPAddress.Any, port);
            lock (server.Lock)
            {
                start(); //start automatically
            }

            string line;
            prompt();
            while ((line = Console.ReadLine()) != null)
            {
                try
                {
                    String[] parts = line.Split();
                    switch (parts[0])
                    {
                        case "stop":
                            lock (server.Lock)
                            {
                                if (server.Started)
                                {
                                    stop();
                                }
                                else
                                {
                                    Console.WriteLine("Already stopped.");
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
                                    Console.WriteLine("Already started.");
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
                                            Console.WriteLine("Game already in " + server.Game.Mode.ToString() + " mode.");
                                    }
                                }
                                else
                                    Console.WriteLine("Server not started.");
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
                                Console.WriteLine("set expects at least two arguments.");
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
                                                    server.Game.FrameRate = val;
                                                else
                                                    Console.WriteLine("ticks must be more than 0.");
                                                break;
                                            default:
                                                {
                                                    Console.WriteLine("set doesn't understand " + parts[1]);
                                                    break;
                                                }
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("game must be started to set this variable.");
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
                            return;


                        // Everything under here is dev code. 
                        case "add":
                            // "takes" two arguments, the first is the cooker id of where you want to 
                            // put the ingredient, the second is the ingredient id of what you want to add
                            if (parts.Length < 3)
                            {
                                Console.WriteLine("add expects at least two arguments.");
                                break;
                            }
                            lock (server.Lock)
                            {
                                server.Game.TestCookerAdd(Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]));
                            }
                            break;
                        case "listworld":
                            // list all objects ids in the game as well as there class 
                            lock (server.Lock)
                            {
                                server.Game.ListGameObjects();
                            }
                            break;

                        case "listcooker":
                            // takes one argument, the cooker you want to list it's contents
                            if (parts.Length < 2)
                            {
                                Console.WriteLine("list expects at least one argument.");
                                break;
                            }
                            lock (server.Lock)
                            {
                                server.Game.ListCookerContents(Convert.ToInt32(parts[1]));
                            }
                            break;
                        case "listing":
                            // lists all the ingredients by name in the game world
                            lock (server.Lock)
                            {
                                server.Game.ListIngredients();
                            }
                            break;
                        default:
                            Console.WriteLine(String.Format("Breakneck Brigade server does not understand command: {0}", parts[0]));
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Console.Write("Error:  ");
                    Console.WriteLine(ex.ToString());
                }
                prompt();
            }
        }
            
        static void start() {
            server.Lock.AssertHeld();

            server.Start();
            Console.WriteLine(String.Format("Listening on port {0}", server.Port));
        }

        static void stop()
        {
            server.Lock.AssertHeld();

            server.Stop();
            Console.WriteLine("Stopped.");
        }

        static void prompt()
        {
            Console.Write("> ");
        }
    }
}
