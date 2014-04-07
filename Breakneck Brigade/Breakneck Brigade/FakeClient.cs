using SousChef;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Breakneck_Brigade
{
    public partial class FakeClient : Form
    {
        public FakeClient()
        {
            InitializeComponent();
        }

        Client client;

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            client = new Client();
            string host = txtIP.Text;
            int port;

            if (String.IsNullOrEmpty(host))
            {
                throw new Exception();
            }
            if (!int.TryParse(txtPort.Text, out port))
            {
                throw new Exception();
            }

            client.Connect(host, port);
            cmdConnect.Enabled = false;
        }

        private void cmdClientEvent_Click(object sender, EventArgs e)
        {
            lock (client.ClientEvents) {
                client.ClientEvents.Add(new ClientEvent() { Type = ClientEventType.RequestTestObject });
                Monitor.PulseAll(client.ClientEvents);
            }
            
        }
    }
}
