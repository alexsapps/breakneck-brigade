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
            this.pnlConnect.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(197, 9);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(75, 128);
            this.cmdConnect.TabIndex = 1;
            this.cmdConnect.Text = "connect";
            this.cmdConnect.UseVisualStyleBackColor = true;
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 24);
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
            this.pnlConnect.Location = new System.Drawing.Point(12, 237);
            this.pnlConnect.Name = "pnlConnect";
            this.pnlConnect.Size = new System.Drawing.Size(284, 155);
            this.pnlConnect.TabIndex = 3;
            // 
            // lstHosts
            // 
            this.lstHosts.FormattingEnabled = true;
            this.lstHosts.Location = new System.Drawing.Point(12, 68);
            this.lstHosts.Name = "lstHosts";
            this.lstHosts.Size = new System.Drawing.Size(179, 69);
            this.lstHosts.TabIndex = 3;
            this.lstHosts.SelectedIndexChanged += new System.EventHandler(this.lstHosts_SelectedIndexChanged);
            this.lstHosts.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstHosts_MouseDoubleClick);
            // 
            // txtConnect
            // 
            this.txtConnect.Location = new System.Drawing.Point(12, 42);
            this.txtConnect.Name = "txtConnect";
            this.txtConnect.Size = new System.Drawing.Size(179, 20);
            this.txtConnect.TabIndex = 0;
            // 
            // frmConnect
            // 
            this.AcceptButton = this.cmdConnect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(794, 404);
            this.Controls.Add(this.pnlConnect);
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
    }
}