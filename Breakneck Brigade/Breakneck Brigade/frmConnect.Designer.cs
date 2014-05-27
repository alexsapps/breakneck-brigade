namespace Breakneck_Brigade
{
    partial class frmConnect
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdConnect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlConnect = new System.Windows.Forms.Panel();
            this.lstHosts = new System.Windows.Forms.ListBox();
            this.txtConnect = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.pnlConnect.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(263, 11);
            this.cmdConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(100, 158);
            this.cmdConnect.TabIndex = 1;
            this.cmdConnect.Text = "connect";
            this.cmdConnect.UseVisualStyleBackColor = true;
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 29);
            this.label1.TabIndex = 2;
            this.label1.Text = "ip:port";
            // 
            // pnlConnect
            // 
            this.pnlConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlConnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.pnlConnect.Controls.Add(this.lstHosts);
            this.pnlConnect.Controls.Add(this.label1);
            this.pnlConnect.Controls.Add(this.txtConnect);
            this.pnlConnect.Controls.Add(this.cmdConnect);
            this.pnlConnect.Location = new System.Drawing.Point(16, 292);
            this.pnlConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlConnect.Name = "pnlConnect";
            this.pnlConnect.Size = new System.Drawing.Size(379, 191);
            this.pnlConnect.TabIndex = 3;
            // 
            // lstHosts
            // 
            this.lstHosts.FormattingEnabled = true;
            this.lstHosts.ItemHeight = 16;
            this.lstHosts.Location = new System.Drawing.Point(16, 84);
            this.lstHosts.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstHosts.Name = "lstHosts";
            this.lstHosts.Size = new System.Drawing.Size(237, 84);
            this.lstHosts.TabIndex = 3;
            this.lstHosts.SelectedIndexChanged += new System.EventHandler(this.lstHosts_SelectedIndexChanged);
            this.lstHosts.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstHosts_MouseDoubleClick);
            // 
            // txtConnect
            // 
            this.txtConnect.Location = new System.Drawing.Point(16, 52);
            this.txtConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtConnect.Name = "txtConnect";
            this.txtConnect.Size = new System.Drawing.Size(237, 22);
            this.txtConnect.TabIndex = 0;
            this.txtConnect.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtConnect_KeyDown);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(418, 303);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 158);
            this.button1.TabIndex = 4;
            this.button1.Text = "Create New Room";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // frmConnect
            // 
            this.AcceptButton = this.cmdConnect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 497);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pnlConnect);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.Name = "frmConnect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Breakneck Brigade";
            this.Load += new System.EventHandler(this.frmConnect_Load);
            this.Shown += new System.EventHandler(this.frmConnect_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.keyDown);
            this.pnlConnect.ResumeLayout(false);
            this.pnlConnect.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdConnect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pnlConnect;
        private System.Windows.Forms.ListBox lstHosts;
        private System.Windows.Forms.TextBox txtConnect;
        private System.Windows.Forms.Button button1;
    }
}