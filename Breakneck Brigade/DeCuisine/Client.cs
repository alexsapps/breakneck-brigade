using SousChef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeCuisine
{
    class Client
    {
        public BBLock Lock = new BBLock();

        private TcpClient connection { get; set; }
        
        public bool IsConnected { get; private set; }

        public ServerPlayer Player { get; set; }

        public Server Server { get; set; }
        public ServerGame Game { get { return Server.Game; } }

        private Thread _receiveThread;
        public Thread receiveThread
        {
            private get { return _receiveThread; }
            set
            {
                if (_receiveThread == null)
                    _receiveThread = value;
                else
                    throw new Exception("thread already set.");
            }
        }

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public Client(Server server)
        {
            this.Server = server;
            ServerMessages = new List<ServerMessage>();
        }

        public void Receive(TcpClient c)
        {
            this.connection = c;
            c.NoDelay = true; //this is why we use a buffered stream, so every int doesn't get written at once.
            try
            {
                var w = new BinaryWriter(c.GetStream());
                w.Write(BB.ServerProtocolHandshakeStr);
                string confighash;
                lock (Server.Lock) {
                    lock(Server.Game.Lock){
                        confighash = Server.Game.Config.Hash;
                    }
                }
                w.Write(confighash);
                w = null;

                using (BinaryReader reader = new BinaryReader(c.GetStream()))
                {
                    connection.ReceiveTimeout = 10000;
                    if (!reader.ReadString().Equals(BB.ClientProtocolHandshakeStr))
                    {
                        Disconnect();
                        return;
                    }
                    connection.ReceiveTimeout = 0;

                    IsConnected = true;
                    new Thread(() => { Connected(this, EventArgs.Empty); }).Start();

                    senderThread = new Thread(() => send());
                    senderThread.Start();

                    while (true)
                    {
                        ClientMessageType type = (ClientMessageType)reader.ReadByte();
                        switch (type)
                        {
                            case ClientMessageType.ClientEvent:
                                ClientEventType eventType = (ClientEventType)reader.ReadByte();
                                ClientEvent evt = (ClientEvent)Activator.CreateInstance(getClientEventType(eventType), reader);
                                DCClientEvent clientEvent = new DCClientEvent() { Client = this, Event = evt };

                                lock (Game.ClientInput)
                                {
                                    Game.ClientInput.Add(clientEvent);
                                }

                                break;
                            default:
                                Disconnect();
                                break;
                        }
                    }
                }
            }
            catch (ObjectDisposedException) { lock (Lock) { Disconnect(); } }
            catch (IOException) { lock (Lock) { Disconnect(); } }
            catch (Exception ex)
            {
                Program.WriteLine(ex.ToString());
                System.Diagnostics.Debugger.Break();
                lock (Lock) { Disconnect(); }
            }
        }

        private Type getClientEventType(ClientEventType t)
        {
            switch(t)
            {
                case ClientEventType.BeginMove: return typeof(ClientBeginMoveEvent);
                case ClientEventType.ChangeOrientation: return typeof(ClientChangeOrientationEvent);
                case ClientEventType.Test: return typeof(ClientTestEvent);
                case ClientEventType.Jump: return typeof(ClientJumpEvent);
                default: throw new Exception("getClientEventType not defiend for " + t.ToString());
            }
        }

        private Thread senderThread;
        
        private List<ServerMessage> ServerMessages { get; set; }
        public void SendMessage(ServerMessage message)
        {
            Lock.AssertHeld();
            lock (ServerMessages)
            {
                ServerMessages.Add(message);
                Monitor.PulseAll(ServerMessages);
            }
        }

        private void send()
        {
            BBStopwatch w1 = new BBStopwatch(), w2 = new BBStopwatch(), w3 = new BBStopwatch();
            try
            {
                var network = connection.GetStream();
                using (MemoryStream buffer = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(buffer))
                    {
                        w3.Start();
                        while (true)
                        {
                            List<ServerMessage> svrMsgs = null;

                            while (true)
                            {
                                lock (Lock)
                                {
                                    if (!IsConnected)
                                        return;
                                }

                                lock (ServerMessages)
                                {
                                    if (ServerMessages.Count > 0)
                                    {
                                        svrMsgs = new List<ServerMessage>(ServerMessages);
                                        ServerMessages.Clear();
                                        break;
                                    }

                                    Monitor.Wait(ServerMessages);
                                }
                            }
                            w3.Stop(Game.FrameRateMilliseconds + 5, "Client: slow waiting for game state from run thread. {0}");
                            w3.Start();

                            w2.Start();
                            foreach (var message in svrMsgs)
                            {
                                writer.Write((byte)message.Type);
                                message.Write(writer);

                                buffer.WriteTo(network);
                                buffer.SetLength(0);

                                if (message.Created.Subtract(DateTime.Now).TotalMilliseconds > 2)
                                    Program.WriteLine("slow message");

                            }
                            w2.Stop(2, "Client: slow write loop. {0}");
                        }
                    }
                }
            }
            catch (ObjectDisposedException) { lock (Lock) { Disconnect(); } }
            catch (IOException) { lock (Lock) { Disconnect(); } }
            catch (Exception ex)
            {
                Program.WriteLine(ex.ToString());
                System.Diagnostics.Debugger.Break();
                lock (Lock) { Disconnect(); }
                throw;
            }
        }

        public void Disconnect()
        {
            Lock.AssertHeld();

            if (!IsConnected) //if connected, client ended session so call disconnect().  otherwise, we initiated disconnect--don't need to call again.
                return;

            IsConnected = false;

            lock (ServerMessages)
            {
                Monitor.PulseAll(ServerMessages); //close sender thread
            }

            try
            {
                connection.Close(); //close receiver thread
            }
            catch { }

            new Thread(() => { Disconnected(this, EventArgs.Empty); }).Start();
        }
    }
}
