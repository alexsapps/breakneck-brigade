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

        static void Main(string[] args)
        {
            int port = 0; //TODO: read port from config file

            server = new Server(IPAddress.Any, port);
            lock (server.Lock)
            {
                start(); //start automatically
            }

            string line;
            prompt();
            while ((line = Console.ReadLine()) != null)
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
                    case "exit":
                        lock (server.Lock)
                        {
                            if (server.Started)
                            {
                                stop();
                            }
                        }
                        return;
                    default:
                        Console.WriteLine(String.Format("Breakneck Brigade server does not understand command: '%0'", parts[0]));
                        break;
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
