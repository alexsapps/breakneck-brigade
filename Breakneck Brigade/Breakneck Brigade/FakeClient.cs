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
        GlobalsConfigFolder config = new GlobalsConfigFolder();
        GlobalsConfigFile globalConfig;
        private void FakeClient_Load(object sender, EventArgs e)
        {
            globalConfig = config.Open(BB.GlobalConfigFilename);
            txtPort.Text = globalConfig.GetSetting("server-port", BB.DefaultServerPort);
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            client = new Client();
            client.Disconnected += client_Disconnected;
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

            lock (client.Lock)
            {
                client.Connect(host, port);
            }

            cmdConnect.Enabled = false;
            tmrRender.Start();
        }

        void client_Disconnected(object sender, EventArgs e)
        {
            this.BeginInvoke((ThreadStart)Close);
        }

        private void cmdClientEvent_Click(object sender, EventArgs e)
        {
            lock (client.ClientEvents) {
                client.ClientEvents.Add(new ClientEvent() { Type = ClientEventType.RequestTestObject, Args = new Dictionary<string,string>() });
                Monitor.PulseAll(client.ClientEvents);
            }
            
        }

        private void FakeClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null)
            {
                lock (client.Lock)
                {
                    if (client.IsConnected) //check if connected because we don't know if we will start a disconnection by closing, or if we're closing because we got disconnected.
                    {
                        client.Disconnect();
                    }
                }
            }
        }

        private void tmrRender_Tick(object sender, EventArgs e)
        {
            StringBuilder v = new StringBuilder();

            lock(client.Lock)
            {
                if (client.GameMode == GameMode.Started)
                {
                    lock (client.Game.Lock)
                    {
                        foreach (var x in client.Game.gameObjects)
                        {
                            v.AppendLine(x.Key + " { " + x.Value.pos.ToString() + " } ");
                        }
                    }
                }
                else
                {
                    v.AppendLine("Mode: " + client.GameMode.ToString());
                }
            }
            
            txtMessages.Text = v.ToString();
        }

    }
}
