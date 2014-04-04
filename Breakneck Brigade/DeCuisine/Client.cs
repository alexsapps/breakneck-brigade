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
        private TcpClient connection { get; set; }

        public event EventHandler Connected;
        public event EventHandler Disconnected;

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
                Disconnect();
            }
            catch (Exception)
            {
                Disconnect();
                throw;
            }
            
        }

        public void Disconnect()
        {
            try
            {
                connection.Close();
            }
            catch { }

            Disconnected(this, EventArgs.Empty);
        }
    }
}
