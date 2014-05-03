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

            try
            {
                var w = new BinaryWriter(connection.GetStream());
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
                    Connected(this, EventArgs.Empty);

                    senderThread = new Thread(() => send());
                    senderThread.Start();

                    while (true)
                    {
                        ClientMessageType type = (ClientMessageType)reader.ReadByte();
                        switch (type)
                        {
                            case ClientMessageType.ClientEvent:
                                ClientEventType eventType = (ClientEventType)reader.ReadByte();
                                int length = reader.ReadInt32();
                                var args = new Dictionary<string, string>();
                                for (int i = 0; i < length; i++)
                                {
                                    args.Add(reader.ReadString(), reader.ReadString());
                                }
                                DCClientEvent clientEvent = new DCClientEvent() { Client = this, Event = new ClientEvent() { Type = eventType, Args = args } };

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
                Console.WriteLine(ex.ToString());
                System.Diagnostics.Debugger.Break();
                lock (Lock) { Disconnect(); }
            }
        }

        private Thread senderThread;
        public List<ServerMessage> ServerMessages { get; private set; }
        private void send()
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(connection.GetStream()))
                {
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
                        
                        foreach (var message in svrMsgs)
                        {
                            writer.Write((byte)message.Type);
                            switch (message.Type)
                            {
                                case ServerMessageType.GameModeUpdate:
                                    writer.Write((byte)((ServerGameModeUpdateMessage)message).Mode);
                                    break;
                                case ServerMessageType.GameStateUpdate:
                                    var msg = (ServerGameStateUpdateMessage)message;
                                    writer.Write(msg.Binary, 0, msg.Length);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (ObjectDisposedException) { lock (Lock) { Disconnect(); } }
            catch (IOException) { lock (Lock) { Disconnect(); } }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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

            Disconnected(this, EventArgs.Empty);
        }
    }
}
