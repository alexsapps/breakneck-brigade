using SousChef;
using System;
using System.Collections.Generic;
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

        private Thread _communicateThread;
        public Thread CommunicateThread
        {
            private get { return _communicateThread; }
            set
            {
                if (_communicateThread == null)
                    _communicateThread = value;
                else
                    throw new Exception("thread already set.");
            }
        }

        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public Client()
        {
            
        }

        public void Communicate(TcpClient c)
        {
            this.connection = c;

            try
            {
                using (BinaryReader r = new BinaryReader(c.GetStream()))
                {
                    if (!r.ReadString().Equals("Breakneck Brigade v1")) //TODO make shared, auto-incrementing version generator function in SousChef
                    {
                        Disconnect();
                        return;
                    }
                    Connected(this, EventArgs.Empty);

                    while (true)
                    {
                        int cmd = r.ReadByte();
                        switch (cmd)
                        {
                            case 1: //TODO: make commands enum in SousChef
                                break;
                            default:
                                Disconnect();
                                break;
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //disconnecting.
                //do not call disconnect() already disconnecting.
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                //TODO: log error message ex
                Disconnect();
                throw;
            }
            
        }

        public void Disconnect()
        {
            Lock.AssertHeld();

            try
            {
                CommunicateThread.Abort();
                connection.Close();
            }
            catch { }

            Disconnected(this, EventArgs.Empty);
        }
    }
}
