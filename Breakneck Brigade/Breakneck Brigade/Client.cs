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
        TcpClient Connection;
        public bool Connected { get; private set; }

        public void Connect(string host, int port)
        {
            Lock.AssertHeld();

            Connection = new TcpClient(host, port);
            receiverThread = new Thread(() => receive());
            receiverThread.Start();

            Connected = true;
        }

        private Thread receiverThread;
        private void receive()
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(Connection.GetStream()))
                {
                    writer.Write(BB.ClientProtocolHandshakeStr);
                }

                using (BinaryReader reader = new BinaryReader(Connection.GetStream()))
                {
                    Connection.ReceiveTimeout = 10000;
                    if (!reader.ReadString().Equals(BB.ServerProtocolHandshakeStr))
                    {
                        Disconnect();
                        return;
                    }
                    Connection.ReceiveTimeout = 0;

                    senderThread = new Thread(() => send());
                    senderThread.Start();

                    while (true)
                    {
                        ServerMessageType type = (ServerMessageType)reader.ReadByte();
                        switch (type)
                        {
                            case ServerMessageType.GameModeUpdate:
                                break;
                            case ServerMessageType.GameStateUpdate:
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch(SocketException)
            {
                //disconnecting
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                //TODO: log error message ex
                Disconnect(); //TODO: are we already disconnecting?
                throw;
            }
        }

        private Thread senderThread;
        public List<ClientEvent> ClientEvents { get; private set; }
        private void send()
        {
            using (BinaryWriter writer = new BinaryWriter(Connection.GetStream()))
            {
                lock (ClientEvents)
                {
                    while (Connected)
                    {
                        foreach(var clientEvent in ClientEvents)
                        {
                            writer.Write((byte)ClientMessageType.ClientEvent);
                            writer.Write((byte)clientEvent.Type);
                            writer.Write(clientEvent.Args.Count);
                            foreach(var pair in clientEvent.Args)
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

        public void Disconnect()
        {
            Lock.AssertHeld();

            Connected = false;

            lock (ClientEvents)
            {
                Monitor.PulseAll(ClientEvents); //close sender thread
            }

            Connection.Close(); //close receiver thread
        }

    }
}
