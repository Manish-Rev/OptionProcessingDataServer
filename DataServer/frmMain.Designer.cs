namespace DataServer
{
    partial class frmMain
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
            this.btnStart = new System.Windows.Forms.Button();
            this.tbSettings = new System.Windows.Forms.TabControl();
            this.tpDataFeeds = new System.Windows.Forms.TabPage();
            this.dgvDataFeeds = new System.Windows.Forms.DataGridView();
            this.colEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDFError = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSettings = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tpConnection = new System.Windows.Forms.TabPage();
            this.dgvConnServiceHosts = new System.Windows.Forms.DataGridView();
            this.colCEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colCName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCError = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCSettings = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tpActiveUsers = new System.Windows.Forms.TabPage();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.dgvUsers = new System.Windows.Forms.DataGridView();
            this.clmSessionID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmLogon = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblState = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tbSettings.SuspendLayout();
            this.tpDataFeeds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataFeeds)).BeginInit();
            this.tpConnection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvConnServiceHosts)).BeginInit();
            this.tpActiveUsers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(3, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.OnStart_Click);
            // 
            // tbSettings
            // 
            this.tbSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSettings.Controls.Add(this.tpDataFeeds);
            this.tbSettings.Controls.Add(this.tpConnection);
            this.tbSettings.Controls.Add(this.tpActiveUsers);
            this.tbSettings.Location = new System.Drawing.Point(3, 41);
            this.tbSettings.Name = "tbSettings";
            this.tbSettings.SelectedIndex = 0;
            this.tbSettings.Size = new System.Drawing.Size(641, 461);
            this.tbSettings.TabIndex = 1;
            // 
            // tpDataFeeds
            // 
            this.tpDataFeeds.Controls.Add(this.dgvDataFeeds);
            this.tpDataFeeds.Location = new System.Drawing.Point(4, 22);
            this.tpDataFeeds.Name = "tpDataFeeds";
            this.tpDataFeeds.Padding = new System.Windows.Forms.Padding(3);
            this.tpDataFeeds.Size = new System.Drawing.Size(633, 435);
            this.tpDataFeeds.TabIndex = 0;
            this.tpDataFeeds.Text = "Data Feeds";
            this.tpDataFeeds.UseVisualStyleBackColor = true;
            // 
            // dgvDataFeeds
            // 
            this.dgvDataFeeds.AllowUserToAddRows = false;
            this.dgvDataFeeds.AllowUserToDeleteRows = false;
            this.dgvDataFeeds.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDataFeeds.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colEnabled,
            this.colName,
            this.colState,
            this.colDFError,
            this.colSettings});
            this.dgvDataFeeds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDataFeeds.Location = new System.Drawing.Point(3, 3);
            this.dgvDataFeeds.Name = "dgvDataFeeds";
            this.dgvDataFeeds.RowHeadersVisible = false;
            this.dgvDataFeeds.Size = new System.Drawing.Size(627, 429);
            this.dgvDataFeeds.TabIndex = 0;
            this.dgvDataFeeds.VirtualMode = true;
            this.dgvDataFeeds.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnCellContentClick_Datafeeds);
            this.dgvDataFeeds.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.OnCellValidating_Datafeeds);
            this.dgvDataFeeds.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.OnCellValueNeeded_Datafeeds);
            // 
            // colEnabled
            // 
            this.colEnabled.HeaderText = "Enabled";
            this.colEnabled.Name = "colEnabled";
            this.colEnabled.Width = 50;
            // 
            // colName
            // 
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colState
            // 
            this.colState.HeaderText = "State";
            this.colState.Name = "colState";
            this.colState.ReadOnly = true;
            // 
            // colDFError
            // 
            this.colDFError.HeaderText = "Error";
            this.colDFError.Name = "colDFError";
            // 
            // colSettings
            // 
            this.colSettings.HeaderText = "Edit Settings";
            this.colSettings.Name = "colSettings";
            // 
            // tpConnection
            // 
            this.tpConnection.Controls.Add(this.dgvConnServiceHosts);
            this.tpConnection.Location = new System.Drawing.Point(4, 22);
            this.tpConnection.Name = "tpConnection";
            this.tpConnection.Padding = new System.Windows.Forms.Padding(3);
            this.tpConnection.Size = new System.Drawing.Size(633, 435);
            this.tpConnection.TabIndex = 1;
            this.tpConnection.Text = "Connection Services";
            this.tpConnection.UseVisualStyleBackColor = true;
            // 
            // dgvConnServiceHosts
            // 
            this.dgvConnServiceHosts.AllowUserToAddRows = false;
            this.dgvConnServiceHosts.AllowUserToDeleteRows = false;
            this.dgvConnServiceHosts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvConnServiceHosts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCEnabled,
            this.colCName,
            this.colCState,
            this.colCError,
            this.colCSettings});
            this.dgvConnServiceHosts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvConnServiceHosts.Location = new System.Drawing.Point(3, 3);
            this.dgvConnServiceHosts.Name = "dgvConnServiceHosts";
            this.dgvConnServiceHosts.RowHeadersVisible = false;
            this.dgvConnServiceHosts.Size = new System.Drawing.Size(627, 429);
            this.dgvConnServiceHosts.TabIndex = 2;
            this.dgvConnServiceHosts.VirtualMode = true;
            this.dgvConnServiceHosts.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnCellContentClick_ConnServices);
            this.dgvConnServiceHosts.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.OnCellValidating_Connections);
            this.dgvConnServiceHosts.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.OnCellValueNeeded_ConnServices);
            // 
            // colCEnabled
            // 
            this.colCEnabled.HeaderText = "Enabled";
            this.colCEnabled.Name = "colCEnabled";
            this.colCEnabled.Width = 50;
            // 
            // colCName
            // 
            this.colCName.HeaderText = "Name";
            this.colCName.Name = "colCName";
            this.colCName.ReadOnly = true;
            // 
            // colCState
            // 
            this.colCState.HeaderText = "State";
            this.colCState.Name = "colCState";
            this.colCState.ReadOnly = true;
            // 
            // colCError
            // 
            this.colCError.HeaderText = "Error";
            this.colCError.Name = "colCError";
            // 
            // colCSettings
            // 
            this.colCSettings.HeaderText = "Edit Settings";
            this.colCSettings.Name = "colCSettings";
            // 
            // tpActiveUsers
            // 
            this.tpActiveUsers.Controls.Add(this.btnDisconnect);
            this.tpActiveUsers.Controls.Add(this.dgvUsers);
            this.tpActiveUsers.Location = new System.Drawing.Point(4, 22);
            this.tpActiveUsers.Name = "tpActiveUsers";
            this.tpActiveUsers.Size = new System.Drawing.Size(633, 435);
            this.tpActiveUsers.TabIndex = 2;
            this.tpActiveUsers.Text = "Active Users";
            this.tpActiveUsers.UseVisualStyleBackColor = true;
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDisconnect.Location = new System.Drawing.Point(3, 392);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect.TabIndex = 3;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.OnClick_Disconnect);
            // 
            // dgvUsers
            // 
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AllowUserToDeleteRows = false;
            this.dgvUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvUsers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clmSessionID,
            this.clmLogon});
            this.dgvUsers.Location = new System.Drawing.Point(0, 0);
            this.dgvUsers.Name = "dgvUsers";
            this.dgvUsers.ReadOnly = true;
            this.dgvUsers.RowHeadersVisible = false;
            this.dgvUsers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsers.Size = new System.Drawing.Size(616, 386);
            this.dgvUsers.TabIndex = 0;
            this.dgvUsers.VirtualMode = true;
            this.dgvUsers.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.OnCellValueNeeded_Users);
            // 
            // clmSessionID
            // 
            this.clmSessionID.HeaderText = "Session";
            this.clmSessionID.MinimumWidth = 300;
            this.clmSessionID.Name = "clmSessionID";
            this.clmSessionID.ReadOnly = true;
            this.clmSessionID.Width = 300;
            // 
            // clmLogon
            // 
            this.clmLogon.HeaderText = "User Name";
            this.clmLogon.MinimumWidth = 100;
            this.clmLogon.Name = "clmLogon";
            this.clmLogon.ReadOnly = true;
            this.clmLogon.Width = 397;
            // 
            // lblState
            // 
            this.lblState.AutoSize = true;
            this.lblState.Location = new System.Drawing.Point(174, 17);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(47, 13);
            this.lblState.TabIndex = 2;
            this.lblState.Text = "Stopped";
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(84, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 0;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.OnStop_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 5000;
            this.timer1.Tick += new System.EventHandler(this.OnTimerTick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 505);
            this.Controls.Add(this.lblState);
            this.Controls.Add(this.tbSettings);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Name = "frmMain";
            this.Text = "Data Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing_MainForm);
            this.Load += new System.EventHandler(this.OnLoad_MainForm);
            this.tbSettings.ResumeLayout(false);
            this.tpDataFeeds.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataFeeds)).EndInit();
            this.tpConnection.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvConnServiceHosts)).EndInit();
            this.tpActiveUsers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TabControl tbSettings;
        private System.Windows.Forms.TabPage tpDataFeeds;
        private System.Windows.Forms.DataGridView dgvDataFeeds;
        private System.Windows.Forms.TabPage tpConnection;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.DataGridView dgvConnServiceHosts;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDFError;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCState;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCError;
        private System.Windows.Forms.DataGridViewButtonColumn colCSettings;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colState;
        private System.Windows.Forms.DataGridViewButtonColumn colSettings;
        private System.Windows.Forms.TabPage tpActiveUsers;
        private System.Windows.Forms.DataGridView dgvUsers;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmSessionID;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmLogon;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Timer timer1;
    }
}

