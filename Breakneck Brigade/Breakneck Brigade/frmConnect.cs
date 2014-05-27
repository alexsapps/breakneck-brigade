using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Breakneck_Brigade
{
    partial class frmConnect : Form
    {
        public frmConnect()
        {
            InitializeComponent();
        }

        public Client ConnectionClient { get; private set; }
        public event EventHandler ConnectClicked;

        public string Host { get; set; }
        public int Port { get; set; }

        private void frmConnect_Load(object sender, EventArgs e)
        {
            Text = Application.ProductName;
            foreach(Control ctl in pnlConnect.Controls)
                ctl.KeyDown += keyDown;
            txtConnect.Text = Host + ":" + Port.ToString();

            string[] hosts = File.ReadAllLines(Program.GetHostsFile());
            foreach(string host in hosts)
                lstHosts.Items.Add(host);
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            doConnect();
        }

        private void doConnect()
        {
            Regex regex = new Regex("^(.*):([0-9]+)$");

            try
            {
                var match = regex.Match(txtConnect.Text);
                if (match.Success)
                {
                    Host = match.Groups[1].Captures[0].Value;
                    Port = int.Parse(match.Groups[2].Captures[0].Value);
                    Enabled = false;
                    ConnectClicked(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void frmConnect_Shown(object sender, EventArgs e)
        {
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.BackgroundImage = global::Breakneck_Brigade.Properties.Resources.background;
        }

        private void lstHosts_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtConnect.Text = lstHosts.SelectedItem as string;
        }

        private void lstHosts_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            doConnect();
        }

        private void txtConnect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if(lstHosts.Items.Count > 0)
                    lstHosts.SelectedIndex = 0;
                lstHosts.Focus();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Create a new server.
            Process newServer = new Process();
            newServer.StartInfo.FileName = "DeCuisine.exe";
            newServer.Start();

            this.doConnect();
        }
    }
}
