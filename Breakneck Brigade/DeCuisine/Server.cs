using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace DeCuisine
{
    class Server
    {
        public bool Started { get; private set; }

        public IPAddress _ip;
        public IPAddress IP
        {
            get
            {
                return _ip;
            }
            set
            {
                if (Started)
                    throw new Exception("Can't change listening IP while running.");
                this._ip = value;
            }
        }

        private int _port;
        public int Port
        {
            get 
            {
                return _port;  
            }
            set
            {
                if (Started)
                    throw new Exception("Can't change listening port while running.");
                this._port = value;
            }
        }
    
        public Server()
        {

        }
        public Server(IPAddress ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

        TcpListener listener;
        Thread listener_thread;
        List<Client> clients = new List<Client>();

        Game game;

        public void Start()
        {
            if (Started)
            {
                throw new Exception("Already started.");
            }

            game = new Game();

            listener = new TcpListener(IP, Port);
            listener_thread = new Thread(new ThreadStart(Listen));
            listener_thread.Start();
            Started = true;
        }

        public void Stop()
        {
            if (Started)
            {
                listener_thread.Abort();
                listener.Stop();
                listener = null;

                game.Dispose();
                game = null;

                foreach (var client in clients)
                {
                    client.Disconnect(); //TODO: synchronize.  what if we are reading data from client?
                }
                clients.Clear();

                Started = false;
            }
        }

        public void Listen()
        {
            while (true)
            {
                TcpClient connection = listener.AcceptTcpClient();
                Client client = new Client();
                Thread client_thread = new Thread(() => client.Communicate(connection));
            }
        }
    }
}
