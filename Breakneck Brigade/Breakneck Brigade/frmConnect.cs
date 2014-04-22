using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

        public Client client { get; private set; }

        private void frmConnect_Load(object sender, EventArgs e)
        {
            Text = Application.ProductName;
        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            
        }

    }
}
