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
        public BBLock Lock;
        TcpClient connection;
        public bool IsConnected { get; private set; }
        
        public class PlayerIdUpdatedEventArgs : EventArgs { public int GameObjId { get; set; } }

        public Client(BBLock @lock)
        {
            Lock = @lock;
            ClientEvents = new List<ClientEvent>();
        }

        public void Connect(string host, int port)
        {
            Lock.AssertHeld();

            connection = new TcpClient();
            connection.Connect(host, port);
            receiverThread = new Thread(new ThreadStart(receive));
            receiverThread.Start();

            IsConnected = true;
        }

        public List<ServerMessage> ServerMessages = new List<ServerMessage>();

        private Thread receiverThread;
        private void receive()
        {
            try
            {
                connection.NoDelay = true;
                var streamBuffer = new BufferedStream(connection.GetStream());
                new BinaryWriter(streamBuffer).Write(BB.ClientProtocolHandshakeStr);
                
                using (BinaryReader reader = new BinaryReader(streamBuffer))
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
                        lock (Lock) //lock UI so user gets confused and looks for the message
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
                        ServerMessage msg = (ServerMessage)Activator.CreateInstance(getServerMessageType(type));
                        msg.Read(reader);

                        lock (Lock)
                        {
                            if (!IsConnected)
                                return;

                            lock (ServerMessages)
                            {
                                ServerMessages.Add(msg);
                                Monitor.PulseAll(ServerMessages);
                            }
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

        private Type getServerMessageType(ServerMessageType type)
        {
            switch(type)
            {
                case ServerMessageType.GameStateUpdate:
                    return typeof(ServerGameStateUpdateMessage);
                case ServerMessageType.GameModeUpdate:
                    return typeof(ServerGameModeUpdateMessage);
                case ServerMessageType.PlayerIdUpdate:
                    return typeof(ServerPlayerIdUpdateMessage);
                default:
                    throw new Exception("getServerMessageType not defined for " + type.ToString());
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
                    while (true)
                    {
                        List<ClientEvent> clientEvents;
                        
                        lock(ClientEvents)
                        {
                            while (ClientEvents.Count == 0)
                            {
                                if (!IsConnected)
                                    return;
                                Monitor.Wait(ClientEvents);
                            }
                        }

                        lock (Lock)
                        {
                            if (!IsConnected)
                                return;

                            lock (ClientEvents)
                            {
                                clientEvents = new List<ClientEvent>(ClientEvents);
                                ClientEvents.Clear();
                            }
                        }
                        
                        foreach (var clientEvent in clientEvents)
                        {
                            writer.Write((byte)ClientMessageType.ClientEvent);
                            writer.Write((byte)clientEvent.Type);
                            clientEvent.Write(writer);
                            writer.Flush();
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
            lock (ClientEvents)
            {
                Monitor.PulseAll(ClientEvents); //close sender thread
            }

            connection.Close(); //close receiver thread

            //Monitor.Exit(Lock);
            //{
            //    //receiver thread creates sender thread, so end receiver first
            //    if (receiverThread != Thread.CurrentThread)
            //        receiverThread.Join();

            //    //then end sender thread if it was already created
            //    if (senderThread != null)
            //        if (senderThread != Thread.CurrentThread)
            //            senderThread.Join();
            //}
            //Monitor.Enter(Lock); //let's hope we didn't re-connect in the meantime.  not sure how to fix this.  actually...since it makes a new Client on every connection, it's fine for now.  ideally, receiver thread makes a signal to disconnect instead of actually disconnecting, and another thread actually does the disconnect.
            //update: not sure why we were joining.  they should be guaranteed to terminate cleanly when they re-acquire the client lock and see that we've disconnected.

            //note: game thread constantly checks IsConnected to know when to terminate
        }
    }
}
