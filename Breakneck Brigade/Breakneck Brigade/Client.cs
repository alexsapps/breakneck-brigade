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
using System.Windows.Forms;

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

                    string hash = reader.ReadString();
                    string ourhash = new GameObjectConfig().GetConfigSalad().Hash; //load config file just to compute hash.  (load again when playing the game, in case it changes later, which is okay.)
                    if(!ourhash.Equals(hash))
                    {
                        lock (Program.ProgramLock) //lock UI so user gets confused and looks for the message
                        {
                            Console.WriteLine("Warning!!!  ConfigSalad hash mismatch between client and server.");
                            MessageBox.Show("ConfigSalad hash mismatch between client and server.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
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
                                lock (Game.Lock)
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
            catch (IOException) { lock (Lock) { Disconnect(); } return; }
            catch (ObjectDisposedException) { lock (Lock) { Disconnect(); } return; }
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
                lock (Lock) { Disconnect(); }
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
            Lock.AssertHeld();
            if (!IsConnected)  //if connected, we are initiating disconnect.  if not connected, something else caused disconnect so no need to disconnect again.
                return;
            IsConnected = false;
            GameMode = SousChef.GameMode.Stopping;

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
            Monitor.Enter(Lock); //let's hope we didn't re-connect in the meantime.  not sure how to fix this.  actually...since it makes a new Client on every connection, it's fine for now.  ideally, receiver thread makes a signal to disconnect instead of actually disconnecting, and another thread actually does the disconnect.

            //note: game thread constantly checks IsConnected to know when to terminate

            Game = null; //may already be null e.g. if quitting before game started (init phase)
        }
    }
}
