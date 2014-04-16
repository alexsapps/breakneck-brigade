using SousChef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    class Client
    {
        public BBLock Lock = new BBLock();
        TcpClient connection;
        public bool IsConnected { get; private set; }
        public Game Game { get; private set; }
        public GameMode GameMode {get; private set; }
        public event EventHandler Disconnected;

        public Client()
        {
            ClientEvents = new List<ClientEvent>();
            GameMode = GameMode.Init;
        }

        public void Connect(string host, int port)
        {
            Lock.AssertHeld();

            connection = new TcpClient();
            connection.Connect(host, port);
            receiverThread = new Thread(() => receive());
            receiverThread.Start();

            IsConnected = true;
        }

        private Thread receiverThread;
        private void receive()
        {
            try
            {
                new BinaryWriter(connection.GetStream()).Write(BB.ClientProtocolHandshakeStr);
                
                using (BinaryReader reader = new BinaryReader(connection.GetStream()))
                {
                    connection.ReceiveTimeout = 10000;
                    if (!reader.ReadString().Equals(BB.ServerProtocolHandshakeStr))
                    {
                        Disconnect();
                        return;
                    }
                    connection.ReceiveTimeout = 0;

                    senderThread = new Thread(() => send());
                    senderThread.Start();

                    while (true)
                    {
                        ServerMessageType type = (ServerMessageType)reader.ReadByte();
                        switch (type)
                        {
                            case ServerMessageType.GameModeUpdate:
                                GameMode = (GameMode)reader.ReadByte();
                                switch (GameMode)
                                {
                                    case GameMode.Init:
                                        break;
                                    case GameMode.Started:
                                        Game = new Game();
                                        break;
                                    case GameMode.Stopping:
                                        break;
                                    default:
                                        lock (Lock)
                                        {
                                            Disconnect();
                                        }
                                        return;
                                }

                                break;
                            case ServerMessageType.GameStateUpdate:
                                lock (Game.Lock)
                                {
                                    Game.gameObjects.Clear();
                                    int len = reader.ReadInt32();

                                    for (int i = 0; i < len; i++)
                                    {
                                        int id = reader.ReadInt32();
                                        Game.gameObjects.Add(id,
                                            new Ingredient(id, new IngredientType("cheese", 10, null))
                                            {
                                                Position = new Vector4(
                                                    reader.ReadInt32(),
                                                    reader.ReadInt32(),
                                                    reader.ReadInt32(),
                                                    reader.ReadInt32())
                                            });
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch(IOException)
            {
                //if connected, server ended session so call disconnect().  otherwise, we initiated disconnect--don't need to call again.

                lock (Lock)
                {
                    if (IsConnected) 
                        Disconnect();
                }
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
        public List<ClientEvent> ClientEvents { get; private set; }
        private void send()
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(connection.GetStream()))
                {
                    lock (ClientEvents)
                    {
                        while (IsConnected)
                        {
                            foreach (var clientEvent in ClientEvents)
                            {
                                writer.Write((byte)ClientMessageType.ClientEvent);
                                writer.Write((byte)clientEvent.Type);
                                writer.Write(clientEvent.Args.Count);
                                foreach (var pair in clientEvent.Args)
                                {
                                    writer.Write(pair.Key);
                                    writer.Write(pair.Value);
                                }
                            }
                            ClientEvents.Clear();

                            Monitor.Wait(ClientEvents);
                        }
                    }
                }
            }
            catch (IOException)
            {
                //if connected, server ended session so call disconnect().  otherwise, we initiated disconnect--don't need to call again.

                lock (Lock)
                {
                    if (IsConnected)
                        Disconnect();
                }
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

        public void Disconnect()
        {
            Lock.AssertHeld();

            IsConnected = false;

            lock (ClientEvents)
            {
                Monitor.PulseAll(ClientEvents); //close sender thread
            }

            connection.Close(); //close receiver thread
            Disconnected(this, EventArgs.Empty);
        }

    }
}
