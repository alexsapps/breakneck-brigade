using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class Program
    {
        static Server server;

        static void Main(string[] args)
        {
            int port = 0; //TODO: read port from config file

            server = new Server(new System.Net.IPAddress(new byte[] { 0, 0, 0, 0 }), port);

            start(); //start automatically

            string line;
            while ((line = Console.ReadLine()) != null)
            {
                String[] parts = line.Split();
                switch (parts[0])
                {
                    case "stop":
                        if (server.Started)
                        {
                            stop();
                        }
                        else
                        {
                            Console.WriteLine("Already stopped.");
                        }
                        break;
                    case "restart":
                        if (server.Started)
                        {
                            stop();
                        }
                        start();
                        break;
                    case "start":
                        if (server.Started)
                            Console.WriteLine("Already started.");
                        else
                            start();
                        break;
                    default:
                        Console.WriteLine(String.Format("Breakneck Brigade server does not understand command: '%0'", parts[0]));
                        break;
                }
            }
        }
            
        static void start() {
            server.Start();
            Console.WriteLine(String.Format("Listening on port %0", server.Port));
        }

        static void stop()
        {
            server.Stop();
            Console.WriteLine("Stopped.");
        }
    }
}
