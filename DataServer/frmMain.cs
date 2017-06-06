using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CommonObjects;
using System.Configuration;

namespace DataServer
{
    public partial class frmMain : Form
    {
        public static readonly String APN_SETTINGS_FILE = "ApnSettings.conf";

        #region Variables
        //the folder contains datafeed dlls
        private readonly string m_DataFeedFolder = Path.Combine(Application.StartupPath, "DataFeeds");
        //the folder contains connection services dlls
        private readonly string m_ConnectionFolder = Path.Combine(Application.StartupPath, "Connections");
        //list of loaded datafeeds
        private readonly List<DataFeedItem> m_DataFeeds = new List<DataFeedItem>();
        //list of loaded connection services
        private readonly List<ConnServiceHostItem> m_ConnServiceHosts = new List<ConnServiceHostItem>();
        //list of all active sessions
        private readonly SortedList<string, string> m_ActiveSessions = new SortedList<string, string>();
        private MessageProcessor m_MessageProcessor;
        private Authentication m_Authentication;
        private RemoteNotification m_RemoteNotification;
        #endregion

        public frmMain()
        {
            InitializeComponent();
        }

		/// <summary>
        /// adds a new active session specified by ID
        /// </summary>
        /// <param name="aSessionID">unique session's id</param>
        /// <param name="aLogon">logon name</param>
        private void AddSession(string aSessionID, string aLogon)
        {
            m_ActiveSessions[aSessionID] = aLogon;
            this.dgvUsers.RowCount = m_ActiveSessions.Count;
            this.dgvUsers.Refresh();
            DataServer.Program.gLogger.WriteToLog_Info(String.Format("Added session: login = '{0}' ID = '{1}'", aLogon, aSessionID), null);
        }

        /// <summary>
        /// removes an active session by ID
        /// </summary>
        /// <param name="aSessionID"></param>
        private void RemoveSession(string aSessionID)
        {
            if (m_ActiveSessions.Remove(aSessionID))
            {
                this.dgvUsers.RowCount = m_ActiveSessions.Count;
                this.dgvUsers.Refresh();
                DataServer.Program.gLogger.WriteToLog_Info(String.Format("Removed session: ID='{0}'", aSessionID), null);
            }
        }

        /// <summary>
        /// Enumerates all assemblies in the DataFeed folder and loads them
        /// </summary>
        private void LoadDataFeeds()
        {
            string[] aDLLs = null;

            try
            {
                aDLLs = Directory.GetFiles(m_DataFeedFolder, "*.dll");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            if (aDLLs.Length == 0)
                return;

            foreach (string item in aDLLs)
            {
                Assembly aDLL = Assembly.UnsafeLoadFrom(item);
                Type[] types = aDLL.GetTypes();

                foreach (Type type in types)
                {
                    try
                    {
                        //data feeder must implement IDataFeed interface
                        if (type.GetInterface("IDataFeed") != null)
                        {
                            object o = Activator.CreateInstance(type);

                            if (o is IDataFeed)
                            {
                                m_DataFeeds.Add(new DataFeedItem
                                    {
                                        FileName = item,
                                        Enabled = true,
                                        Name = ((IDataFeed)o).Name,
                                        Type = type,
                                        DataFeed = (IDataFeed)o,
                                    });
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            foreach (DataFeedItem item in m_DataFeeds)
            {
                item.Load(item.DataFeed.DefaultSettings);
            }
        }

        /// <summary>
        /// Enumerates all assemblies in the Connections folder and loads them
        /// </summary>
        private void LoadConnectionServiceHosts()
        {
            string[] aDLLs = null;

            try
            {
                aDLLs = Directory.GetFiles(m_ConnectionFolder, "*.dll");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (aDLLs.Length == 0)
                return;

            foreach (string item in aDLLs)
            {
                Assembly aDLL = Assembly.UnsafeLoadFrom(item);
                Type[] types = aDLL.GetTypes();

                foreach (Type type in types)
                {
                    try
                    {
                        //connection service must support IDataServerServiceHost interface
                        if (type.GetInterface("DataServer.IDataServerServiceHost") != null)
                        {
                            object o = Activator.CreateInstance(type);

                            if (o is IDataServerServiceHost)
                            {

                                m_ConnServiceHosts.Add(new ConnServiceHostItem
                                    {
                                        FileName = item,
                                        Enabled = true,
                                        Name = ((IDataServerServiceHost)o).Name,
                                        Type = type,
                                        Host = (IDataServerServiceHost)o,
                                        Parameters = new Dictionary<string, object>()
                                    });
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            foreach (ConnServiceHostItem item in m_ConnServiceHosts)
            {
                item.Load(item.Host.DefaultSettings);
            }
        }

        /// <summary>
        /// refresh list of data feeders in UI
        /// </summary>
        private void RefreshDataFeedsView()
        {
            dgvDataFeeds.Rows.Clear();
            dgvDataFeeds.RowCount = m_DataFeeds.Count;
            dgvDataFeeds.Refresh();
        }

        /// <summary>
        /// refresh list of connection services in UI
        /// </summary>
        private void RefreshConnectionsView()
        {
            dgvConnServiceHosts.Rows.Clear();
            dgvConnServiceHosts.RowCount = m_ConnServiceHosts.Count;
            dgvConnServiceHosts.Refresh();
        }

        /// <summary>
        /// stores settings of data feeders and connection services to file
        /// </summary>
        private void SaveChanges()
        {
            foreach (DataFeedItem datafeed in m_DataFeeds)
            {
                datafeed.Save();
            }
            foreach (ConnServiceHostItem connection in m_ConnServiceHosts)
            {
                connection.Save();
            }
        }

        /// <summary>
        /// updates enable property of UI controls
        /// </summary>
        /// <param name="enable">true enables, false disables</param>
        private void EnableControls(bool enable)
        {
            btnStart.Enabled = enable;
            dgvDataFeeds.ReadOnly = !enable;
            dgvConnServiceHosts.ReadOnly = !enable;
        }

        #region event handlers
        /// <summary>
        /// occurs when message router is adding a new session
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event parameters</param>
        void OnRemovedSession_MsgRouter(object sender, MessageRouter.MessageRouter_EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(o =>
                {
                    RemoveSession(o);
                }), e.ID);

            }
            else
                RemoveSession(e.ID);
        }

        /// <summary>
        /// occurs when message router is removing a session
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event parameters</param>
        void OnAddedSession_MsgRouter(object sender, MessageRouter.MessageRouter_EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string, string>((o1, o2) =>
                {
                    AddSession(o1, o2);
                }), e.ID, e.UserInfo.Login);

            }
            else
                AddSession(e.ID, e.UserInfo.Login);
        }

        private void OnLoad_MainForm(object sender, EventArgs e)
        {
            //load plugins found in DataFeeds, Connections folders
            try
            {
                LoadDataFeeds();
                LoadConnectionServiceHosts();
            }
            catch (Exception ex)
            {
                DataServer.Program.gLogger.WriteToLog(null, ex);

                throw ex;
            }

            //update data feeders and connection services view
            RefreshDataFeedsView();
            RefreshConnectionsView();
        }

        private void OnStart_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> aAdditionalParameters = new Dictionary<string, object>();

            if (m_MessageProcessor != null)
            {
                MessageBox.Show("Server already started", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }
            DataServer.Program.gLogger.WriteToLog_Info("Starting...", null);
            if (m_Authentication == null)
            {
                string aConnectionString = ConfigurationManager.ConnectionStrings["DataServer"].ConnectionString;

                m_Authentication = new Authentication(DataServer.Program.gLogger);
                m_Authentication.Init(aConnectionString);
            }
            aAdditionalParameters["sql connection string"] = m_Authentication.ConnectionStr;

            if (m_RemoteNotification == null)
            {
                // Read APN settings

                String apnConfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, APN_SETTINGS_FILE);

                if (File.Exists(apnConfFilePath))
                {
                    String[] confLines = null;
                    try
                    {
                        confLines = File.ReadAllLines(apnConfFilePath);
                    }
                    catch (Exception ex)
                    {
                        DataServer.Program.gLogger.WriteToLog("APN conf file reading.", ex);
                    }
                    
                    // 1-st line must be absolute path to iOS cert file.
                    // 2-nd lines must be iOS cert password.
                    // 3-rd line must be boolean value if this is production environment for iOS.
                    // 4-th line must be absolute path to OS X cert file.
                    // 5-th lines must be OS X cert password.
                    // 5-th line must be boolean value if this is production environment for OS X.
                    if (confLines != null && confLines.Length <= 6)
                    {
                        String certPathIOS = confLines[0];
                        String certPasswordIOS = confLines[1];
                        Boolean isProductionEnvironmentIOS = false;
                        Boolean.TryParse(confLines[2], out isProductionEnvironmentIOS);

                        String certPathOSX = confLines[3];
                        String certPasswordOSX = confLines[4];
                        Boolean isProductionEnvironmentOSX = false;
                        Boolean.TryParse(confLines[5], out isProductionEnvironmentOSX);

                        if (File.Exists(certPathIOS) && File.Exists(certPathOSX)) 
                        {
                            var apnSettings = new ApnSettings()
                            {
                                CertPathIOS = certPathIOS,
                                CertPasswordIOS = certPasswordIOS,
                                IsProductionIOS = isProductionEnvironmentIOS,
                                CertPathOSX = certPathOSX,
                                CertPasswordOSX = certPasswordOSX,
                                IsProductionOSX = isProductionEnvironmentOSX
                            };
                            m_RemoteNotification = new RemoteNotification(apnSettings);
                        }
                        else
                            DataServer.Program.gLogger.WriteToLog_Warning("APN cert file doesn't exists.", null);
                    }
                    else
                        DataServer.Program.gLogger.WriteToLog_Warning("Invalid APN conf file format.", null);
                }
                else
                {
                    DataServer.Program.gLogger.WriteToLog_Info("APN conf file path: " + apnConfFilePath, null);
                    DataServer.Program.gLogger.WriteToLog_Warning("No APN config file was found.", null);
                }
            }

            List<DataFeedItem> aInitializedFeeders = new List<DataFeedItem>();
            List<ConnServiceHostItem> aInitializedConnectionServices = new List<ConnServiceHostItem>();

            lblState.Text = "Starting";
            lblState.Refresh();
            EnableControls(false);
            SaveChanges();
            foreach (DataFeedItem item in m_DataFeeds)
            {
                item.Error = string.Empty;
                if (item.Enabled)
                {
                    if (item.Parameters == null && item.DataFeed != null)
                        item.Parameters = item.DataFeed.DefaultSettings;
                    aInitializedFeeders.Add(item);
                }
                else
                    item.State = "Disabled";
            }
            foreach (ConnServiceHostItem item in m_ConnServiceHosts)
            {
                item.Error = string.Empty;
                if (item.Enabled)
                {
                    if (item.Parameters == null && item.Host != null)
                        item.Parameters = item.Host.DefaultSettings;
                    aInitializedConnectionServices.Add(item);
                }
                else
                    item.State = "Disabled";
            }
            dgvDataFeeds.Refresh();
            dgvConnServiceHosts.Refresh();
            // initialize message router
            if (DataServer.MessageRouter.gMessageRouter == null)
            {
                DataServer.MessageRouter.gMessageRouter = new MessageRouter();

                DataServer.MessageRouter.gMessageRouter.Init(m_Authentication, m_RemoteNotification);
                DataServer.MessageRouter.gMessageRouter.AddedSession += OnAddedSession_MsgRouter;
                DataServer.MessageRouter.gMessageRouter.RemovedSession += OnRemovedSession_MsgRouter;
            }
            lock (DataServer.MessageRouter.gMessageRouter)
            {
                foreach (DataFeedItem item in aInitializedFeeders)
                {
                    try
                    {
                        item.State = "Starting...";
                        item.DataFeed.Start(item.Parameters, m_DataFeedFolder);
                        item.State = "Started";
                    }
                    catch (Exception ex)
                    {
                        item.State = "Failed";
                        item.Error = ex.Message;
                        DataServer.Program.gLogger.WriteToLog_Warning(String.Format("{0} feeder failed to start.", item.Name), ex);
                    }
                }
                foreach (ConnServiceHostItem item in aInitializedConnectionServices)
                {
                    try
                    {
                        item.State = "Starting...";
                        item.Host.Start(item.Parameters);
                        item.State = "Started";
                    }
                    catch (Exception ex)
                    {
                        item.State = "Failed";
                        item.Error = ex.Message;
                        DataServer.Program.gLogger.WriteToLog_Warning(String.Format("{0} connection service failed to start.", item.Name), ex);
                    }
                }
            }

            m_MessageProcessor = new MessageProcessor();
            m_MessageProcessor.Start(aInitializedFeeders, aInitializedConnectionServices, aAdditionalParameters);

            dgvDataFeeds.Refresh();
            dgvConnServiceHosts.Refresh();

            lblState.Text = "Started";
            btnStop.Enabled = true;
            this.timer1.Start();
            DataServer.Program.gLogger.WriteToLog_Info("Started.", null);
        }

        private void OnStop_Click(object sender, EventArgs e)
        {
            if (m_MessageProcessor == null)
            {
                MessageBox.Show("Server already stopped", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }
            this.timer1.Stop();
            lblState.Text = "Stopping";
            lblState.Refresh();
            try
            {
                m_MessageProcessor.Stop();
            }
            catch
            {
            }
            m_MessageProcessor = null;
            foreach (DataFeedItem item in m_DataFeeds)
            {
                try
                {
                    if (item.State == "Started")
                    {
                        item.Error = string.Empty;
                        item.State = "Stopping...";
                        item.DataFeed.Stop();
                        item.State = "Stoped";
                    }
                }
                catch (Exception ex)
                {
                    item.Error = ex.Message;
                }
            }
            this.dgvDataFeeds.Refresh();
            foreach (ConnServiceHostItem item in m_ConnServiceHosts)
            {
                try
                {
                    if (item.State == "Started")
                    {
                        item.Error = string.Empty;
                        item.State = "Stopping...";
                        item.Host.Stop();
                        item.State = "Stopped";
                    }
                }
                catch (Exception ex)
                {
                    item.Error = ex.Message;
                }
            }
            this.dgvConnServiceHosts.Refresh();
            lblState.Text = "Stopped";
            btnStop.Enabled = false;
            EnableControls(true);
        }

        /// <summary>
        /// occurs on redrawing active session
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event arguments</param>
        private void OnCellValueNeeded_Users(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= m_ActiveSessions.Count) return;

            string aSessionID = m_ActiveSessions.Keys[e.RowIndex];
            string aLogon = m_ActiveSessions[aSessionID];

            if (e.ColumnIndex == this.clmLogon.Index)
                e.Value = aLogon;
            else if (e.ColumnIndex == this.clmSessionID.Index)
                e.Value = aSessionID;
        }

        private void OnClosing_MainForm(object sender, FormClosingEventArgs e)
        {
            if (m_MessageProcessor != null)
            {
                MessageBox.Show("The server is running. Please stop server to close the application.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
            SaveChanges();
        }

        /// <summary>
        /// occurs on redrawing data feeds
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event arguments</param>
        private void OnCellValueNeeded_Datafeeds(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= m_DataFeeds.Count)
                return;

            if (e.ColumnIndex == colEnabled.Index)
                e.Value = m_DataFeeds[e.RowIndex].Enabled;
            else if (e.ColumnIndex == colName.Index)
                e.Value = m_DataFeeds[e.RowIndex].Name;
            else if (e.ColumnIndex == colState.Index)
                e.Value = m_DataFeeds[e.RowIndex].State;
            else if (e.ColumnIndex == colDFError.Index)
                e.Value = m_DataFeeds[e.RowIndex].Error;
            else if (e.ColumnIndex == colSettings.Index)
                e.Value = "Edit Settings";
            else
                System.Diagnostics.Debug.Assert(false);
        }

        private void OnCellContentClick_Datafeeds(object sender, DataGridViewCellEventArgs e)
        {
            if (m_MessageProcessor == null && e.ColumnIndex == colSettings.Index && e.RowIndex >= 0 && e.RowIndex < m_DataFeeds.Count)
            {
                ILogonControl aILogonControl = m_DataFeeds[e.RowIndex].DataFeed.LogonControl;

                if (aILogonControl is UserControl)
                {
                    DlgLogonSettings dlg = new DlgLogonSettings();

                    dlg.Init(aILogonControl);
                    aILogonControl.Settings = m_DataFeeds[e.RowIndex].Parameters;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        m_DataFeeds[e.RowIndex].Parameters = aILogonControl.Settings;
                        m_DataFeeds[e.RowIndex].Save();
                    }
                }
                else
                    MessageBox.Show("The data feeder does not support functionality to update settings", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnCellValidating_Datafeeds(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == colEnabled.Index && e.RowIndex >= 0 && e.RowIndex < m_DataFeeds.Count)
                m_DataFeeds[e.RowIndex].Enabled = (bool)e.FormattedValue;
        }

        /// <summary>
        /// occurs on redrawing active sessions
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">event arguments</param>
        private void OnCellValueNeeded_ConnServices(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= m_ConnServiceHosts.Count)
                return;

            if (e.ColumnIndex == colCEnabled.Index)
                e.Value = m_ConnServiceHosts[e.RowIndex].Enabled;
            else if (e.ColumnIndex == colCName.Index)
                e.Value = m_ConnServiceHosts[e.RowIndex].Name;
            else if (e.ColumnIndex == colCState.Index)
                e.Value = m_ConnServiceHosts[e.RowIndex].State;
            else if (e.ColumnIndex == colCError.Index)
                e.Value = m_ConnServiceHosts[e.RowIndex].Error;
            else if (e.ColumnIndex == colCSettings.Index)
                e.Value = "Edit Settings";
            else
                System.Diagnostics.Debug.Assert(false);
        }

        private void OnCellContentClick_ConnServices(object sender, DataGridViewCellEventArgs e)
        {
            if (m_MessageProcessor == null && e.ColumnIndex == colCSettings.Index && e.RowIndex >= 0 && e.RowIndex < m_ConnServiceHosts.Count)
            {
                ILogonControl aILogonControl = m_ConnServiceHosts[e.RowIndex].Host.LogonControl;

                if (aILogonControl is UserControl)
                {
                    DlgLogonSettings dlg = new DlgLogonSettings();

                    dlg.Init(aILogonControl);
                    aILogonControl.Settings = m_ConnServiceHosts[e.RowIndex].Parameters;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        m_ConnServiceHosts[e.RowIndex].Parameters = aILogonControl.Settings;
                    }
                    m_ConnServiceHosts[e.RowIndex].Save();
                }
                else
                    MessageBox.Show("The connection service does not support functionality to update settings", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnCellValidating_Connections(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == colCEnabled.Index && e.RowIndex >= 0 && e.RowIndex < m_ConnServiceHosts.Count)
                m_ConnServiceHosts[e.RowIndex].Enabled = (bool)e.FormattedValue;
        }

        private void OnClick_Disconnect(object sender, EventArgs e)
        {
            List<string> aSessions = new List<string>();

            lock (m_ActiveSessions)
            {
                foreach (DataGridViewRow item in this.dgvUsers.SelectedRows)
                {
                    aSessions.Add(m_ActiveSessions.Keys[item.Index]);
                }
            }
            foreach (string item in aSessions)
            {
                lock (DataServer.MessageRouter.gMessageRouter)
                {
                    IUserInfo aIUserInfo = DataServer.MessageRouter.gMessageRouter.GetUserInfo(item);

                    if (aIUserInfo != null)
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            aIUserInfo.Disconnect();
                        }
                        );
                    }
                }
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            List<string> aSessions = new List<string>();

            lock (m_ActiveSessions)
            {
                aSessions.AddRange(m_ActiveSessions.Keys);
            }
            foreach (string item in aSessions)
            {
                lock (DataServer.MessageRouter.gMessageRouter)
                {
                    IUserInfo aIUserInfo = DataServer.MessageRouter.gMessageRouter.GetUserInfo(item);

                    if (aIUserInfo != null)
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                aIUserInfo.Heartbeat();
                            }
                            catch
                            {
                            }
                        }
                        );
                    }
                }
            }
        }
        #endregion
    }
}
