﻿using System;
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
            doConnect();
        }
    }
}
