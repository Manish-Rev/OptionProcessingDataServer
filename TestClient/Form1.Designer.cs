namespace TestClient
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtLogin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.pnlLogin = new System.Windows.Forms.Panel();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.gbLevel1 = new System.Windows.Forms.GroupBox();
            this.btnUnsubscribe = new System.Windows.Forms.Button();
            this.btnSubscribe = new System.Windows.Forms.Button();
            this.txtL1Symbol = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.gbHistory = new System.Windows.Forms.GroupBox();
            this.cmbL1DataFeeds = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btnGetHistory = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbHPeriodicity = new System.Windows.Forms.ComboBox();
            this.numHBarCount = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtHSymbol = new System.Windows.Forms.TextBox();
            this.cmbHDataFeeds = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.pnlLogin.SuspendLayout();
            this.gbLevel1.SuspendLayout();
            this.gbHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHBarCount)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Login";
            // 
            // txtLogin
            // 
            this.txtLogin.Location = new System.Drawing.Point(61, 55);
            this.txtLogin.Name = "txtLogin";
            this.txtLogin.Size = new System.Drawing.Size(157, 20);
            this.txtLogin.TabIndex = 1;
            this.txtLogin.Text = "akaushal";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(61, 81);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(157, 20);
            this.txtPassword.TabIndex = 1;
            this.txtPassword.Text = "barchart";
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(61, 107);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(89, 23);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Host";
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(61, 3);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(157, 20);
            this.txtHost.TabIndex = 1;
            this.txtHost.Text = "127.0.0.1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Port";
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(61, 29);
            this.numPort.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(157, 20);
            this.numPort.TabIndex = 3;
            this.numPort.Value = new decimal(new int[] {
            4504,
            0,
            0,
            0});
            // 
            // pnlLogin
            // 
            this.pnlLogin.Controls.Add(this.txtHost);
            this.pnlLogin.Controls.Add(this.numPort);
            this.pnlLogin.Controls.Add(this.label1);
            this.pnlLogin.Controls.Add(this.btnLogin);
            this.pnlLogin.Controls.Add(this.txtLogin);
            this.pnlLogin.Controls.Add(this.txtPassword);
            this.pnlLogin.Controls.Add(this.label3);
            this.pnlLogin.Controls.Add(this.label4);
            this.pnlLogin.Controls.Add(this.label2);
            this.pnlLogin.Location = new System.Drawing.Point(12, 0);
            this.pnlLogin.Name = "pnlLogin";
            this.pnlLogin.Size = new System.Drawing.Size(224, 153);
            this.pnlLogin.TabIndex = 4;
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(12, 159);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(638, 281);
            this.txtLog.TabIndex = 5;
            // 
            // gbLevel1
            // 
            this.gbLevel1.Controls.Add(this.cmbL1DataFeeds);
            this.gbLevel1.Controls.Add(this.btnUnsubscribe);
            this.gbLevel1.Controls.Add(this.txtL1Symbol);
            this.gbLevel1.Controls.Add(this.label6);
            this.gbLevel1.Controls.Add(this.btnSubscribe);
            this.gbLevel1.Controls.Add(this.label5);
            this.gbLevel1.Location = new System.Drawing.Point(242, 3);
            this.gbLevel1.Name = "gbLevel1";
            this.gbLevel1.Size = new System.Drawing.Size(386, 68);
            this.gbLevel1.TabIndex = 6;
            this.gbLevel1.TabStop = false;
            this.gbLevel1.Text = "Level 1";
            // 
            // btnUnsubscribe
            // 
            this.btnUnsubscribe.Location = new System.Drawing.Point(97, 39);
            this.btnUnsubscribe.Name = "btnUnsubscribe";
            this.btnUnsubscribe.Size = new System.Drawing.Size(85, 23);
            this.btnUnsubscribe.TabIndex = 0;
            this.btnUnsubscribe.Text = "Unsubscribe";
            this.btnUnsubscribe.UseVisualStyleBackColor = true;
            this.btnUnsubscribe.Click += new System.EventHandler(this.btnUnsubscribe_Click);
            // 
            // btnSubscribe
            // 
            this.btnSubscribe.Location = new System.Drawing.Point(6, 38);
            this.btnSubscribe.Name = "btnSubscribe";
            this.btnSubscribe.Size = new System.Drawing.Size(85, 23);
            this.btnSubscribe.TabIndex = 0;
            this.btnSubscribe.Text = "Subscribe";
            this.btnSubscribe.UseVisualStyleBackColor = true;
            this.btnSubscribe.Click += new System.EventHandler(this.btnSubscribe_Click);
            // 
            // txtL1Symbol
            // 
            this.txtL1Symbol.Location = new System.Drawing.Point(207, 11);
            this.txtL1Symbol.Name = "txtL1Symbol";
            this.txtL1Symbol.Size = new System.Drawing.Size(85, 20);
            this.txtL1Symbol.TabIndex = 1;
            this.txtL1Symbol.Text = "GOOG";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(160, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Symbol";
            // 
            // gbHistory
            // 
            this.gbHistory.Controls.Add(this.cmbHDataFeeds);
            this.gbHistory.Controls.Add(this.numHBarCount);
            this.gbHistory.Controls.Add(this.cmbHPeriodicity);
            this.gbHistory.Controls.Add(this.txtHSymbol);
            this.gbHistory.Controls.Add(this.label10);
            this.gbHistory.Controls.Add(this.label8);
            this.gbHistory.Controls.Add(this.label9);
            this.gbHistory.Controls.Add(this.label7);
            this.gbHistory.Controls.Add(this.btnGetHistory);
            this.gbHistory.Location = new System.Drawing.Point(242, 77);
            this.gbHistory.Name = "gbHistory";
            this.gbHistory.Size = new System.Drawing.Size(386, 76);
            this.gbHistory.TabIndex = 6;
            this.gbHistory.TabStop = false;
            this.gbHistory.Text = "History";
            // 
            // cmbL1DataFeeds
            // 
            this.cmbL1DataFeeds.FormattingEnabled = true;
            this.cmbL1DataFeeds.Location = new System.Drawing.Point(69, 11);
            this.cmbL1DataFeeds.Name = "cmbL1DataFeeds";
            this.cmbL1DataFeeds.Size = new System.Drawing.Size(85, 21);
            this.cmbL1DataFeeds.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Data Feed";
            // 
            // btnGetHistory
            // 
            this.btnGetHistory.Location = new System.Drawing.Point(296, 16);
            this.btnGetHistory.Name = "btnGetHistory";
            this.btnGetHistory.Size = new System.Drawing.Size(85, 27);
            this.btnGetHistory.TabIndex = 0;
            this.btnGetHistory.Text = "Get History";
            this.btnGetHistory.UseVisualStyleBackColor = true;
            this.btnGetHistory.Click += new System.EventHandler(this.btnGetHistory_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(4, 50);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Periodicity";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(158, 49);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(59, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "Bars Count";
            // 
            // cmbHPeriodicity
            // 
            this.cmbHPeriodicity.FormattingEnabled = true;
            this.cmbHPeriodicity.Location = new System.Drawing.Point(67, 46);
            this.cmbHPeriodicity.Name = "cmbHPeriodicity";
            this.cmbHPeriodicity.Size = new System.Drawing.Size(85, 21);
            this.cmbHPeriodicity.TabIndex = 2;
            // 
            // numHBarCount
            // 
            this.numHBarCount.Location = new System.Drawing.Point(221, 45);
            this.numHBarCount.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numHBarCount.Name = "numHBarCount";
            this.numHBarCount.Size = new System.Drawing.Size(69, 20);
            this.numHBarCount.TabIndex = 3;
            this.numHBarCount.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(158, 23);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Symbol";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(4, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(57, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Data Feed";
            // 
            // txtHSymbol
            // 
            this.txtHSymbol.Location = new System.Drawing.Point(221, 19);
            this.txtHSymbol.Name = "txtHSymbol";
            this.txtHSymbol.Size = new System.Drawing.Size(69, 20);
            this.txtHSymbol.TabIndex = 1;
            this.txtHSymbol.Text = "GOOG";
            // 
            // cmbHDataFeeds
            // 
            this.cmbHDataFeeds.FormattingEnabled = true;
            this.cmbHDataFeeds.Location = new System.Drawing.Point(67, 19);
            this.cmbHDataFeeds.Name = "cmbHDataFeeds";
            this.cmbHDataFeeds.Size = new System.Drawing.Size(85, 21);
            this.cmbHDataFeeds.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 452);
            this.Controls.Add(this.gbHistory);
            this.Controls.Add(this.gbLevel1);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.pnlLogin);
            this.Name = "Form1";
            this.Text = "Test Client";
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.pnlLogin.ResumeLayout(false);
            this.pnlLogin.PerformLayout();
            this.gbLevel1.ResumeLayout(false);
            this.gbLevel1.PerformLayout();
            this.gbHistory.ResumeLayout(false);
            this.gbHistory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHBarCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLogin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Panel pnlLogin;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.GroupBox gbLevel1;
        private System.Windows.Forms.Button btnUnsubscribe;
        private System.Windows.Forms.Button btnSubscribe;
        private System.Windows.Forms.TextBox txtL1Symbol;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox gbHistory;
        private System.Windows.Forms.ComboBox cmbL1DataFeeds;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbHDataFeeds;
        private System.Windows.Forms.NumericUpDown numHBarCount;
        private System.Windows.Forms.ComboBox cmbHPeriodicity;
        private System.Windows.Forms.TextBox txtHSymbol;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnGetHistory;
    }
}

