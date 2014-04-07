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
        public Game Game { get { return Server.Game; } }

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
                new BinaryWriter(connection.GetStream()).Write(BB.ServerProtocolHandshakeStr);

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
                                int length = reader.ReadByte();
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
            catch (IOException)
            {
                //if connected, client ended session so call disconnect().  otherwise, we initiated disconnect--don't need to call again.
                if (IsConnected)
                    lock (Lock) { Disconnect(); }
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                //TODO: log error message ex
                lock (Lock) { Disconnect(); }
                throw;
            }
        }

        private Thread senderThread;
        public List<ServerMessage> ServerMessages { get; private set; }
        private void send()
        {
            using (BinaryWriter writer = new BinaryWriter(connection.GetStream()))
            {
                lock (ServerMessages)
                {
                    while (IsConnected)
                    {
                        foreach (var message in ServerMessages)
                        {
                            writer.Write((byte)message.Type);
                            switch (message.Type)
                            {
                                case ServerMessageType.GameModeUpdate:
                                    writer.Write((byte)((ServerGameModeUpdateMessage)message).Mode);
                                    break;
                                case ServerMessageType.GameStateUpdate:
                                    var msg = (ServerGameStateUpdateMessage)message;
                                    writer.Write(msg.GameObjects.Count);
                                    foreach (var obj in msg.GameObjects)
                                    {
                                        writer.Write(obj.Key);
                                        writer.Write(obj.Value);
                                    }
                                    break;
                            }
                        }
                        ServerMessages.Clear();
                        Monitor.Wait(ServerMessages);
                    }
                }
            }
        }

        public void Disconnect()
        {
            Lock.AssertHeld();

            Debug.Assert(IsConnected); //do not disconnect twice
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
