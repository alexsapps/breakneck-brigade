using SousChef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public ClientGame Game { get; private set; }
        public GameMode GameMode { get; private set; }

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
                        lock (Lock)
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
                                var mode = (GameMode)reader.ReadByte();
                                lock (Lock)
                                {
                                    GameMode = mode;
                                    switch (GameMode)
                                    {
                                        case GameMode.Init:
                                            break;
                                        case GameMode.Started:
                                            Game = new ClientGame();
                                            break;
                                        case GameMode.Stopping:
                                            break;
                                        default:
                                            Disconnect();
                                            return;
                                    }
                                }

                                break;
                            case ServerMessageType.GameStateUpdate:
                                lock (Game.gameObjects)
                                {
                                    int len;
                                    len = reader.ReadInt32();
                                    for (int i = 0; i < len; i++)
                                    {
                                        int id = reader.ReadInt32();
                                        ClientGameObject obj = ClientGameObject.Deserialize(id, reader, Game);
                                        Game.gameObjects.Add(obj.Id, obj);
                                    }
                                    len = reader.ReadInt32();
                                    for (int i = 0; i < len; i++)
                                    {
                                        int id = reader.ReadInt32();
                                        Game.gameObjects[id].StreamUpdate(reader);
                                    }
                                    len = reader.ReadInt32();
                                    for (int i = 0; i < len; i++)
                                    {
                                        int id = reader.ReadInt32();
                                        Game.gameObjects.Remove(id);
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
                Console.WriteLine(ex.ToString());
                System.Diagnostics.Debugger.Break();
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
                Console.WriteLine(ex.ToString());
                lock (Lock) { Disconnect(); }
                throw;
            }
        }

        public void Disconnect()
        {
            Debug.Assert(IsConnected);

            Lock.AssertHeld();

            GameMode = SousChef.GameMode.Stopping;
            IsConnected = false;

            lock (ClientEvents)
            {
                Monitor.PulseAll(ClientEvents); //close sender thread
            }

            connection.Close(); //close receiver thread

            Monitor.Exit(Lock);
            {
                //receiver thread creates sender thread, so end receiver first
                if (receiverThread != Thread.CurrentThread)
                    receiverThread.Join();

                //then end sender thread if it was already created
                if (senderThread != null)
                    if (senderThread != Thread.CurrentThread)
                        senderThread.Join();
                
            }
            Monitor.Enter(Lock);

            //NOTE: game thread constantly checks IsConnected to know when to terminate

            //if (Game != null) //e.g. if quitting before game started (init phase)
            //{
            //    lock (Game.gameObjects)
            //    {
            //        //Game.HasUpdates = true; //the update is that the game has ended
            //        //Monitor.PulseAll(Game.gameObjects); //close renderer thread --we do this below now
            //    }
            //}


            Game = null;
        }
    }
}
