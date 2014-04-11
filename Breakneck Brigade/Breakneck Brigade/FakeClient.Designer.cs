namespace Breakneck_Brigade
{
    partial class FakeClient
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
            this.components = new System.ComponentModel.Container();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.txtMessages = new System.Windows.Forms.Label();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.cmdClientEvent = new System.Windows.Forms.Button();
            this.tmrRender = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(184, 12);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(75, 23);
            this.cmdConnect.TabIndex = 0;
            this.cmdConnect.Text = "connect";
            this.cmdConnect.UseVisualStyleBackColor = true;
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // txtMessages
            // 
            this.txtMessages.Location = new System.Drawing.Point(9, 115);
            this.txtMessages.Name = "txtMessages";
            this.txtMessages.Size = new System.Drawing.Size(263, 123);
            this.txtMessages.TabIndex = 1;
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(12, 12);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(107, 20);
            this.txtIP.TabIndex = 2;
            this.txtIP.Text = "127.0.0.1";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(126, 12);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(52, 20);
            this.txtPort.TabIndex = 3;
            // 
            // cmdClientEvent
            // 
            this.cmdClientEvent.Location = new System.Drawing.Point(59, 74);
            this.cmdClientEvent.Name = "cmdClientEvent";
            this.cmdClientEvent.Size = new System.Drawing.Size(156, 23);
            this.cmdClientEvent.TabIndex = 4;
            this.cmdClientEvent.Text = "spawn something";
            this.cmdClientEvent.UseVisualStyleBackColor = true;
            this.cmdClientEvent.Click += new System.EventHandler(this.cmdClientEvent_Click);
            // 
            // tmrRender
            // 
            this.tmrRender.Interval = 10;
            this.tmrRender.Tick += new System.EventHandler(this.tmrRender_Tick);
            // 
            // FakeClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.cmdClientEvent);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.txtMessages);
            this.Controls.Add(this.cmdConnect);
            this.Name = "FakeClient";
            this.Text = "FakeClient";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FakeClient_FormClosing);
            this.Load += new System.EventHandler(this.FakeClient_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdConnect;
        private System.Windows.Forms.Label txtMessages;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button cmdClientEvent;
        private System.Windows.Forms.Timer tmrRender;
    }
}