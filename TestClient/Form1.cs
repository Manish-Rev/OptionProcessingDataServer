using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestClient.ServerMess;

namespace TestClient
{
    public partial class Form1 : Form, IWCFServiceCallback, IDisposable
    {
        private WCFServiceClient _client;
        private InstanceContext _context;
        private readonly System.ServiceModel.Channels.Binding _binding = new NetTcpBinding("NetTcpBinding_IWCFService");
        private EndpointAddress _endpoint;
        private DataFeed[] _DataFeeds;

        public Form1()
        {
            InitializeComponent();
            cmbHPeriodicity.Items.AddRange(new string[] { "Minutely", "Hourly", "Daily", "Weekly", "Tick", "Range" });
            cmbHPeriodicity.SelectedIndex = 0;
            EnabledControls(false);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            _endpoint = new EndpointAddress(string.Format("net.tcp://{0}:{1}/DataServer_Service", txtHost.Text, numPort.Value));
            _context = new InstanceContext(this);
            _client = new WCFServiceClient(_context, _binding, _endpoint);

            try
            {
                txtLog.AppendText( "Connecting..." + Environment.NewLine);
                var loginRequest = new LoginRequest { Login = txtLogin.Text, Password = txtPassword.Text };
                _client.Login(loginRequest);
                txtLog.AppendText("Connected" + Environment.NewLine);
                _client.MessageIn(new DataFeedListRequest());
                EnabledControls(true);
                ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //_client.MessageIn(HistoryRequest
            
        }

        private void ScrollToEnd()
        {
            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.ScrollToCaret();
        }

        private void EnabledControls(bool logined)
        {
            pnlLogin.Enabled = !logined;
            gbLevel1.Enabled = gbHistory.Enabled = logined;
        }

        private void btnSubscribe_Click(object sender, EventArgs e)
        {
            _client.MessageIn(new SubscribeRequest() { Symbol = new SymbolItem() { DataFeed = cmbL1DataFeeds.Text, Symbol = txtL1Symbol.Text, Type = Instrument.Equity } });
        }

        private void btnUnsubscribe_Click(object sender, EventArgs e)
        {
            _client.MessageIn(new UnsubscribeRequest() { Symbol = new SymbolItem() { DataFeed = cmbL1DataFeeds.Text, Symbol = txtL1Symbol.Text, Type = Instrument.Equity } });
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
                case 4:
                    interval = 4;
                    period = Periodicity.Tick;
                    break;
                case 5:
                    interval = 6;
                    period = Periodicity.Range;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            _client.MessageIn(new HistoryRequest() 
            { 
                Selection = new HistoryParameters() 
                { 
                    Symbol = new SymbolItem() { DataFeed = cmbL1DataFeeds.Text, Symbol = txtL1Symbol.Text, Type = Instrument.Equity },
                    Id = Guid.NewGuid().ToString(),
                     Periodicity = period,
                     Interval = interval,
                     BarsCount = (int)numHBarCount.Value
                }});
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

        #region Dispose
        
        public void Dispose()
        {

        }

        #endregion

        #region IWCFServiceCallback

        void IWCFServiceCallback.MessageOut(ResponseMessage message)
        {
            if (message is LoginResponse)
            {

            }
            else if (message is DataFeedListResponse)
            {
                _DataFeeds = ((DataFeedListResponse)message).DataFeeds;
                InitDataFeedControl();
            }
            else if (message is NewTickResponse)
            {
                NewTickResponse tickMessage =(NewTickResponse) message;
                foreach (var tick in tickMessage.Tick)
                {
                    txtLog.AppendText(string.Format("{0}:{1} - Price:{2}  Volume:{3}", tick.Symbol.DataFeed, tick.Symbol.Symbol, tick.Price, tick.Volume) + Environment.NewLine);
                }
                ScrollToEnd();
            }
            else if (message is HistoryResponse)
            {
                HistoryResponse histMessage = (HistoryResponse)message;
                txtLog.Text += "History Message" + Environment.NewLine;
                foreach (var bar in histMessage.Bars)
                {
                    txtLog.AppendText(string.Format("{0} O:{1} H:{2} L:{3} C:{4} V:{5}", bar.Date.ToString(), bar.Open, bar.High, bar.Low, bar.Close, bar.Volume) + Environment.NewLine);
                }
                ScrollToEnd();
            }
            else if (message is HeartbeatResponse)
            {

            }
            else if (message is ErrorInfo)
            {

            }
            else
            {

            }
        }

        #endregion

    }
}
