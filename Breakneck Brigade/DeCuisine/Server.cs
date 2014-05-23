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
            lock (Lock)
            {
                loadConfig();
            }
        }
        public Server(IPAddress ip, int port) : this()
        {
            this.IP = ip;
            this.Port = port;
        }

        public void ReloadConfig()
        {
            Lock.AssertHeld();

            loadConfig();

            if (Game != null)
            {
                lock (Game.Lock)
                {
                    Game.ReloadConfig();
                }
            }
        }

        void loadConfig()
        {
            Lock.AssertHeld();
        }

        TcpListener listener;
        Thread listener_thread;
        List<Client> clients = new List<Client>();

        public ServerGame Game { get; private set; }

        public void Start()
        {
            Lock.AssertHeld();

            if (Started)
            {
                throw new Exception("Already started.");
            }

            Game = new ServerGame(this);

            cancel_listen = false;
            listener = new TcpListener(IP, Port);
            listener_thread = new Thread(new ThreadStart(Listen));
            listener_thread.Start();
        }

        public void Stop()
        {
            Lock.AssertHeld();

            if (Started)
            {
                cancel_listen = true;
                listener.Stop(); //do this first so reader doesn't try to do things while we're stopping
                listener_thread.Abort();

                while(cancel_listen)
                {
                    Monitor.Wait(Lock, 50); //give up lock so listener can stop
                }

                listener = null;

                lock (Lock)
                {
                    foreach (var client in new List<Client>(clients))
                    {
                        lock (client.Lock)
                        {
                            client.Disconnect();
                        }
                    }
                    while (clients.Count > 0)
                    {
                        Monitor.Wait(Lock, 10);
                    }
                }

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
                ;

                lock (Lock)
                {
                    listener.Stop();

                    {
                        int retries = 0;
                        while (true)
                        {
                            try
                            {
                                listener.Start();
                                break;
                            }
                            catch
                            {
                                if (retries++ < 10)
                                {
                                    Program.WriteLine("Listen failed.  Retrying...");
                                    System.Threading.Thread.Sleep(2000);
                                }
                                else
                                    throw;
                            }
                        }
                        if (retries > 0)
                        {
                            Program.WriteLine("Finally acquired TCP port " + Port);
                        }
                    }
                    Started = true;
                }
                
                try
                {
                    while (true)
                    {
                        if (cancel_listen)
                            return;

                        TcpClient connection = listener.AcceptTcpClient();

                        lock (Lock)
                        {
                            Client client = new Client(this);
                            clients.Add(client);
                            Game.Controller.AssignTeam(client); // assign random team
                            client.Connected += client_Connected;
                            client.Disconnected += client_Disconnected;
                            Thread client_thread = new Thread(() => { client.Receive(connection); });
                            client.receiveThread = client_thread;
                            client_thread.Start();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (cancel_listen)
                        return; //server is shutting down
                    else
                    {
                        Program.WriteLine(ex.ToString());
                        System.Diagnostics.Debugger.Break();
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
            try
            {
                lock (Lock)
                {
                    clients.Remove((Client)sender);
                    ClientLeave(this, new ClientEventArgs((Client)sender));
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
                throw ex;
            }
        }

        public string PrintStatus()
        {
            StringBuilder b = new StringBuilder();
            Lock.AssertHeld();
            b.AppendLine(Started ? "Started." : "Offline.");
            if (Started)
            {
                b.AppendLine(clients.Count.ToString() + " clients connected.");
                lock(Game.Lock) {
                    Game.PrintStatus(b);
                }
            }
            return b.ToString();
        }
    }
}
