using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        public string DefaultHost { get; set; }
        public string DefaultPort { get; set; }
        public Client client { get; private set; }

        private void frmConnect_Load(object sender, EventArgs e)
        {
            Text = Application.ProductName;
            KeyDown += keyDown;
            txtConnect.KeyDown += keyDown;
            txtConnect.Text = DefaultHost + ":" + DefaultPort;
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            Client client = new Client();

            string host;
            int port;
            Regex regex = new Regex("^(.*):([0-9]+)$");

            try
            {
                var match = regex.Match(txtConnect.Text);
                if (match.Success)
                {
                    host = match.Groups[1].Captures[0].Value;
                    port = int.Parse(match.Groups[2].Captures[0].Value);

                    lock (client.Lock)
                    {
                        client.Connect(host, port);
                    }

                    DialogResult = DialogResult.OK;
                    Close();
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

    }
}
