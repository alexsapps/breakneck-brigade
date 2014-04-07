using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using SousChef;

namespace DeCuisine
{
    class Server
    {
        public BBLock Lock = new BBLock();

        public bool Started { get; private set; }

        public event EventHandler<ClientEventArgs> ClientEnter;
        public event EventHandler<ClientEventArgs> ClientLeave;

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
        public Server(IPAddress ip, int port) : this()
        {
            this.IP = ip;
            this.Port = port;
        }

        TcpListener listener;
        Thread listener_thread;
        List<Client> clients = new List<Client>();

        public Game Game { get; private set; }

        public void Start()
        {
            Lock.AssertHeld();

            if (Started)
            {
                throw new Exception("Already started.");
            }

            Game = new Game(this);

            cancel_listen = false;
            listener = new TcpListener(IP, Port);
            listener_thread = new Thread(new ThreadStart(Listen));
            listener_thread.Start();
            Started = true;
        }

        public void Stop()
        {
            Lock.AssertHeld();

            if (Started)
            {
                cancel_listen = true;
                listener.Stop(); //do this first, actually.
                listener_thread.Abort();

                while(cancel_listen)
                {
                    Monitor.Wait(Lock, 50); //give up lock so listener can stop
                }

                //listener.Stop();
                listener = null;

                foreach (var client in clients)
                {
                    lock (client.Lock)
                    {
                        client.Disconnect(); //TODO: synchronize.  what if we are reading data from client?
                    }
                }
                clients.Clear();

                lock (Game.Lock)
                {
                    if (Game.Mode == GameMode.Started || Game.Mode == GameMode.Paused)
                    {
                        Game.Stop();
                    }
                    Game.Dispose();
                    Game = null;
                }

                Started = false;
            }
        }

        private bool cancel_listen;
        public void Listen()
        {
            try
            {
            keepGoing:
                listener.Stop();
                listener.Start();

                try
                {
                    while (true)
                    {
                        if (cancel_listen)
                            return;

                        TcpClient connection = listener.AcceptTcpClient();

                        lock (Lock)
                        {
                            if (Game.Mode == GameMode.Init)  //only connect during init
                            {
                                Client client = new Client(this);
                                clients.Add(client);
                                client.Connected += client_Connected;
                                client.Disconnected += client_Disconnected;
                                Thread client_thread = new Thread(() => client.Receive(connection));
                                client.receiveThread = client_thread;
                                client_thread.Start();
                            }
                            else
                            {
                                connection.Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (cancel_listen)
                        return; //server is shutting down
                    else
                    {
                        //TODO: log ex
                        Console.WriteLine(ex.ToString());
                        System.Diagnostics.Debugger.Break(); //TODO:  break whenever logging error
                        goto keepGoing;
                    }
                }
                finally
                {
                    if (listener != null)
                        listener.Stop();
                }
            }
            finally
            {
                cancel_listen = false; //indicate listening cancelled so stop() can become unblocked
            }
        }

        void client_Connected(object sender, EventArgs e)
        {
            ClientEnter(this, new ClientEventArgs((Client)sender));
        }

        void client_Disconnected(object sender, EventArgs e)
        {
            clients.Remove((Client)sender);
            ClientLeave(this, new ClientEventArgs((Client)sender));
        }
    }
}
