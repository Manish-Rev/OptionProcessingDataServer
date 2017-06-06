using CommonObjects;
using Newtonsoft.Json;
using System;
using System.Windows.Forms;

namespace TestWebSocketClient
{
    public partial class MainForm : Form, IDisposable
    {
        #region Private Fields
        private DataFeed[] _DataFeeds;
        private WebSocket4Net.WebSocket _socket;
        #endregion

        #region Constructors
        public MainForm()
        {
            InitializeComponent();
            cmbHPeriodicity.Items.AddRange(new string[] { "Minutely", "Hourly", "Daily", "Weekly" });
            cmbHPeriodicity.SelectedIndex = 0;
            EnabledControls(false);
        }
        #endregion

        #region Event Handlers
        private void btnLogin_Click(object sender, EventArgs e)
        {
            InitWebSocket();
        }

        private void _socket_MessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {
            var msgTypeName = GetMessageTypeName(e.Message);

            // deserialize
            switch (msgTypeName)
            {
                case "LoginResponse":
                    // send DataFeedListRequest
                    Send(new DataFeedListRequest());
                    break;

                case "DataFeedListResponse":
                    ProcessDataFeedListResponse(DeserializeResponse<DataFeedListResponse>(e.Message));
                    break;

                case "NewTickResponse":
                    ProcessNewTickResponse(DeserializeResponse<NewTickResponse>(e.Message));
                    break;

                case "HistoryResponse":
                    ProcessHistoryResponse(DeserializeResponse<HistoryResponse>(e.Message));
                    break;
            }
        }

        private void _socket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            AppendToLog("Error: " + e.Exception.Message);
        }

        private void _socket_Closed(object sender, EventArgs e)
        {
            AppendToLog("Socket has been Closed!");
        }

        private void _socket_Opened(object sender, EventArgs e)
        {
            // we successfully conected
            // send LoginRequest to server
            Send(new LoginRequest(txtLogin.Text, txtPassword.Text));

            AppendToLog("Connected");
        }

        private void btnSubscribe_Click(object sender, EventArgs e)
        {
            // send SubscribeRequest
            Send(new SubscribeRequest(new SymbolItem() { DataFeed = cmbL1DataFeeds.Text, Symbol = txtL1Symbol.Text, Type = Instrument.Equity }));
        }

        private void btnUnsubscribe_Click(object sender, EventArgs e)
        {
            // send UnsubscribeRequest
            Send(new UnsubscribeRequest(new SymbolItem() { DataFeed = cmbL1DataFeeds.Text, Symbol = txtL1Symbol.Text, Type = Instrument.Equity }));
        }

        private void btnGetHistory_Click(object sender, EventArgs e)
        {
            int interval = 1;
            Periodicity period = Periodicity.Minute;
            switch (cmbHPeriodicity.SelectedIndex)
            {
                case 0://minute
                    //use default
                    break;
                case 1:
                    interval = 60;
                    break;
                case 2:
                    period = Periodicity.Day;
                    break;
                case 3:
                    interval = 7;
                    period = Periodicity.Day;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            //send HistoryRequest
            Send(new HistoryRequest(new HistoryParameters()
            {
                Symbol = new SymbolItem() { DataFeed = cmbL1DataFeeds.Text, Symbol = txtL1Symbol.Text, Type = Instrument.Equity },
                Id = Guid.NewGuid().ToString(),
                Periodicity = period,
                Interval = interval,
                BarsCount = (int)numHBarCount.Value
            }));
        }
        #endregion

        #region Private members
        private void InitWebSocket()
        {
            try
            {
                var uri = string.Format("ws://{0}:{1}/DataServer_Service", txtHost.Text, numPort.Value);
                _socket = new WebSocket4Net.WebSocket(uri);
                _socket.Opened += _socket_Opened;
                _socket.Closed += _socket_Closed;
                _socket.Error += _socket_Error;
                _socket.MessageReceived += _socket_MessageReceived;

                AppendToLog("Connecting...");
                _socket.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ProcessHistoryResponse(HistoryResponse histMessage)
        {
            AppendToLog("History Message");
            foreach (var bar in histMessage.Bars)
            {
                AppendToLog(string.Format("{0} O:{1} H:{2} L:{3} C:{4} V:{5}", bar.Date.ToString(), bar.Open, bar.High, bar.Low, bar.Close, bar.Volume));
            }
        }

        private void ProcessNewTickResponse(NewTickResponse tickMessage)
        {
            foreach (var tick in tickMessage.Tick)
            {
                AppendToLog(string.Format("{0}:{1} - Price:{2}  Volume:{3}", tick.Symbol.DataFeed, tick.Symbol.Symbol, tick.Price, tick.Volume));
            }
        }

        private void ProcessDataFeedListResponse(DataFeedListResponse response)
        {
            _DataFeeds = response.DataFeeds.ToArray();

            InvokeIfRequired(cmbL1DataFeeds, InitDataFeedControl);

            EnabledControls(true);
        }

        /// <summary>
        /// Send provided Request to Server through socket
        /// </summary>
        /// <param name="request"></param>
        private void Send(RequestMessage request)
        {
            var serializedMsg = SerializeRequest(request);
            _socket.Send(serializedMsg);
        }

        private void AppendToLog(string msg)
        {
            var action = new Action(() =>
            {
                txtLog.AppendText(msg + Environment.NewLine);
                ScrollToEnd();
            });

            InvokeIfRequired(txtLog, action);
        }

        private void ScrollToEnd()
        {
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        private void EnabledControls(bool logined)
        {
            var action = new Action(() =>
            {
                pnlLogin.Enabled = !logined;
                gbLevel1.Enabled = gbHistory.Enabled = logined;
            });

            InvokeIfRequired(pnlLogin, action);
        }

        private void InvokeIfRequired(Control ctl, Action action)
        {
            if (ctl.InvokeRequired)
            {
                ctl.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private void InitDataFeedControl()
        {
            cmbL1DataFeeds.Items.Clear();
            cmbHDataFeeds.Items.Clear();
            foreach (var feed in _DataFeeds)
            {
                cmbL1DataFeeds.Items.Add(feed.Name);
                cmbHDataFeeds.Items.Add(feed.Name);
            }
            if (_DataFeeds.Length > 0)
            {
                cmbL1DataFeeds.SelectedIndex = 0;
                cmbHDataFeeds.SelectedIndex = 0;
            }
        } 
        #endregion

        #region Dispose

        public void Dispose()
        {

        }

        #endregion

        #region Serialization 
        private string SerializeRequest(RequestMessage request)
        {
            return JsonConvert.SerializeObject(request);
        }

        private T DeserializeResponse<T>(string rsp) where T : class 
        {
            return JsonConvert.DeserializeObject<T>(rsp);
        }

        private string GetMessageTypeName(string message)
        {
            var msgObj = JsonConvert.DeserializeObject<ResponseMessage>(message);

            return msgObj.MsgType;
        }
        #endregion
    }
}
