using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using CommonObjects;
using System.Globalization;
using ddfplus;
using System.Net;
using System.IO;
using System.Threading;
using System.Text;
using ddfplus.Net;

namespace DDFDataFeed
{
    /// <summary>
    /// BarChart DataFeed
    /// </summary>
    public class BarChartDataFeed : DataServer.Logger, IDataFeed
    {
        #region Variables and Init

        private const string HistoryBaseUrl = "http://ds01.ddfplus.com/historical/{0}.ashx";
        private const string LoginBaseUrl = "http://www.ddfplus.com/getdataserver.htx";
        private const string DefaultServer = "qs01.ddfplus.com";
        private const string DefaultSymbol = "DM";
        private const string Level1MarketMaker = "L1";
        private string m_UserName;
        private string m_Password;
        private Client m_DDFClient;
        private readonly CultureInfo m_Culture = new CultureInfo("en-US");
        private readonly List<string> m_SubscribedSymbol = new List<string>();
        private readonly List<string> m_Level2SubscribedSymbol = new List<string>();
        private readonly List<string> m_DDFSymbols = new List<string>();
        private const string MarketName = "NASDAQ";
        private TimeZoneInfo m_TimeZoneInfo;
        private string m_Path;
        private volatile Status m_ConnectionStatus = Status.Disconnected;
        
        public event NewQuoteHandler NewQuote;
        public event NewLevel2DataHandler NewLevel2;
        private List<ExchangeInfo> _Markets = null;

        public BarChartDataFeed()
            : base("DDF")
        {

        }

        #endregion

        #region IDataFeed
        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public string Name
        {
            get { return base.Target; }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public List<ExchangeInfo> Markets
        {
            get
            {
                if (_Markets != null)
                    return _Markets;

                ExchangeInfo market = new ExchangeInfo
                    {
                        Name = MarketName,
                        Symbols = new List<SymbolItem>()
                    };

                try
                {
                    string[] lines = File.ReadAllLines(Path.Combine(m_Path, "Symbols.txt"));
                    IEnumerable<string[]> symbolsInfo =
                        lines.Select(t => t.Replace("          ", "|").Split('|')).Where(symbol => symbol.Length >= 2);

                    foreach (string[] symbol in symbolsInfo)
                    {
                        var typeInstrument = Instrument.Equity;
                        if (System.String.CompareOrdinal(symbol[2], "FOREX") == 0)
                        {
                             typeInstrument = Instrument.Forex;                            
                        }

                        market.Symbols.Add(new SymbolItem { Company = symbol[0], Symbol = symbol[1], Type = typeInstrument });
                    }
                }
                catch (Exception e)
                {
                    WriteToLog(null, e);
                }
                _Markets = new List<ExchangeInfo>() { market };

                return _Markets;
            }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public ILogonControl LogonControl
        {
            get
            {
                return new LogonControl();
            }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public Dictionary<string, object> DefaultSettings
        {
            get
            {
                Dictionary<string, object> aDict = new Dictionary<string, object>();

                aDict["user_name"] = String.Empty;
                aDict["password"] = String.Empty;

                return aDict;
            }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public void Start(Dictionary<string, object> aParams, string aPath)
        {
            if (aParams.ContainsKey("user_name"))
                m_UserName = aParams["user_name"] as string;
            if (m_UserName == null)
                m_UserName = String.Empty;
            if (aParams.ContainsKey("password"))
                m_Password = aParams["password"] as string;
            else
                m_Password = String.Empty;
            m_Path = aPath;

            string aURL = LoginBaseUrl + "?username=" + m_UserName + "&password=" + m_Password;
			string[] res = OpenURL(aURL).Split(new[] { "\r\n" }, StringSplitOptions.None);

            if (res.Count() < 3)
                throw new ApplicationException("Data feed is not available. The response is not recognized.");

            if (res[2].Split(new char[] { ' ' }).Where(o => String.Compare(o, "ok", true) == 0).FirstOrDefault() != null)
            {
                if (res[1].Split(new char[] { ' ' }).Where(o => String.Compare(o, "ok", true) == 0).FirstOrDefault() != null)
                {
                    string aServer = res[0].Replace("StreamingServer:", "").Trim();

                    Connection.Properties["streamingversion"] = "3";
                    Connection.Mode = ConnectionMode.Default;
                    Connection.Username = m_UserName;
                    Connection.Password = m_Password;
                    Connection.Server = aServer;
                    Connection.Properties["traceinfo"] = true;
                    Connection.Properties["tracedebug"] = true;
                    Connection.Properties["messagetracefilter"] = "*";
                    
                    System.Diagnostics.Trace.WriteLine("test");

                    m_DDFClient = new Client();
                    m_DDFClient.NewQuote += OnDdfClient_NewQuote;
                    m_DDFClient.Error += m_DDFClient_Error;
                    Connection.StatusChange += Connection_StatusChange;
                    Subscribe(new SymbolItem() { Symbol = DefaultSymbol, Type = Instrument.Equity });
                }
                else
                    throw new ApplicationException("Data feed is not available. The status is not OK.");
            }
            else
                throw new ApplicationException("Data feed is not available. The authentication failed.");
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public void Stop()
        {
            if (m_DDFClient != null)
            {
                m_DDFClient.NewQuote -= OnDdfClient_NewQuote;
                m_DDFClient.Error -= m_DDFClient_Error;
                Connection.StatusChange -= Connection_StatusChange;
                m_DDFClient.Symbols = String.Empty;
                m_DDFClient = null;
                m_UserName = string.Empty;
                m_Password = string.Empty;
                lock (m_SubscribedSymbol)
                {
                    m_SubscribedSymbol.Clear();
                }
            }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public void Subscribe(SymbolItem aSymbol)
        {
            string aSymbolName = BuildSymbolName(aSymbol.Symbol, aSymbol.Type);
            int maxWhileCount = 20;
            int whileCount = 0;
            while(m_ConnectionStatus == Status.Connecting || m_ConnectionStatus == Status.Disconnecting)
            {
                Thread.Sleep(100);
                whileCount++;
                if (whileCount >= maxWhileCount)
                    break;
            }
            bool isAdded = true;
            lock (m_SubscribedSymbol)
            {
                if (!m_SubscribedSymbol.Contains(aSymbol.Symbol))
                {
                    isAdded = false;
                    m_SubscribedSymbol.Add(aSymbol.Symbol);
                }
            }
            if (!isAdded)
            {
                lock (m_DDFSymbols)
                {
                    if (!m_DDFSymbols.Contains(aSymbolName))
                    {
                        m_DDFSymbols.Add(aSymbolName);
                        isAdded = false;
                    }
                    else
                    {
                        isAdded = true;
                    }
                }
            }
            if (!isAdded)
            {
                UpdateSubscriberSymbols();
            }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public void UnSubscribe(SymbolItem aSymbol)
        {
            string aSymbolName = BuildSymbolName(aSymbol.Symbol, aSymbol.Type);
            int maxWhileCount = 20;
            int whileCount = 0;
            while (m_ConnectionStatus == Status.Connecting || m_ConnectionStatus == Status.Disconnecting)
            {
                Thread.Sleep(100);
                whileCount++;
                if (whileCount >= maxWhileCount)
                    break;
            }
            lock (m_SubscribedSymbol)
            {
                if (m_SubscribedSymbol.Contains(aSymbolName))
                {
                    m_SubscribedSymbol.Remove(aSymbolName);
                }
            }
            bool isLevel2Subscribed = false;
            lock (m_Level2SubscribedSymbol)
            {
                isLevel2Subscribed = m_Level2SubscribedSymbol.Contains(aSymbolName);
            }
            if (!isLevel2Subscribed)
            {
                lock (m_DDFSymbols)
                {
                    m_DDFSymbols.Remove(aSymbolName);
                }
                UpdateSubscriberSymbols();
            }
        }

        public void SubscribeLevel2(SymbolItem aSymbol)
        {
            string aSymbolName = BuildSymbolName(aSymbol.Symbol, aSymbol.Type);
            bool isAdded = true;
            lock (m_Level2SubscribedSymbol)
            {
                if (!m_Level2SubscribedSymbol.Contains(aSymbolName))
                {
                    isAdded = false;
                    m_Level2SubscribedSymbol.Add(aSymbolName);
                }
            }
            if (!isAdded)
            {
                lock (m_DDFSymbols)
                {
                    if (!m_DDFSymbols.Contains(aSymbolName))
                    {
                        m_DDFSymbols.Add(aSymbolName);
                        isAdded = false;
                    }
                    else
                    {
                        isAdded = true;
                    }
                }
            }
            if (!isAdded)
            {
                UpdateSubscriberSymbols();
            }
        }

        public void UnsubscribeLevel2(SymbolItem aSymbol)
        {
            string aSymbolName = BuildSymbolName(aSymbol.Symbol, aSymbol.Type);
            lock (m_Level2SubscribedSymbol)
            {
                if (m_Level2SubscribedSymbol.Contains(aSymbolName))
                {
                    m_Level2SubscribedSymbol.Remove(aSymbolName);
                }
            }
            bool isLevel1Subscribed = false;
            lock (m_Level2SubscribedSymbol)
            {
                isLevel1Subscribed = m_SubscribedSymbol.Contains(aSymbolName);
            }
            if (!isLevel1Subscribed)
            {
                lock (m_DDFSymbols)
                {
                    m_DDFSymbols.Remove(aSymbolName);
                }
                UpdateSubscriberSymbols();
            }
        }

        public void GetHistory(GetHistoryCtx aCtx, HistoryAnswerHandler aCallback)
        {
            List<Bar> aBars = new List<Bar>();

            //doesn't support
            if (aCtx.Request.Selection.Periodicity == Periodicity.Tick ||
                aCtx.Request.Selection.Periodicity == Periodicity.Range)
            {
                aCallback(aCtx, aBars);
                return;
            }

            try
            {
                string aData;
                if (aCtx.FromF.Year < 1900 )
                    aCtx.FromF = new DateTime(1900, 1, 1);

                string aURL = BuildUrl2(aCtx.Request.Selection, aCtx.FromF, aCtx.ToF, aCtx.MaxBars);
                string dateFormat;
                int dateColumn;
                if (aCtx.Request.Selection.Periodicity >= Periodicity.Day)
                {
                    dateFormat = "yyyy-MM-dd";
                    dateColumn = 1;
                }
                else
                {
                    dateFormat = "yyyy-MM-dd HH:mm";
                    dateColumn = 0;
                }
                string[] aRows;

                aData = OpenURL(aURL);
                aRows = aData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string item in aRows)
                {
                    Bar aBar = new Bar();
                    string[] aCells = item.Split(',');

                    if (aCells.Length == 7)
                    {
                        aBar.Date = DateTime.ParseExact(aCells[dateColumn], dateFormat, null);
                        aBar.Date = TimeZoneInfo.ConvertTimeToUtc(aBar.Date, TimeZoneInfo);//
                        aBar.Open = Double.Parse(aCells[2], m_Culture.NumberFormat);
                        if (Math.Abs(aBar.Open) < Double.Epsilon)
                            continue;
                        aBar.High = Double.Parse(aCells[3], m_Culture.NumberFormat);
                        aBar.Low = Double.Parse(aCells[4], m_Culture.NumberFormat);
                        aBar.Close = Double.Parse(aCells[5], m_Culture.NumberFormat);
                        aBar.Volume = Int64.Parse(aCells[6], m_Culture.NumberFormat);
                    }
                    else
                        throw new ApplicationException("History response is not valid: " + aData);
                    aBars.Add(aBar);
                }
                aBars.Reverse();
            }
            catch (Exception e)
            {
                WriteToLog("GetHistory", e);
            }

            if (NeedConvertBars(aCtx))
            {
                aBars = CombineBars(aBars, aCtx.Request.Selection.Periodicity, aCtx.Request.Selection.Interval);
            }

            aCallback(aCtx, aBars);
        }

        private bool NeedConvertBars(GetHistoryCtx aCtx)
        {
            return (aCtx.Request.Selection.Periodicity == Periodicity.Hour && aCtx.Request.Selection.Interval >= 18) ||
                (aCtx.Request.Selection.Periodicity == Periodicity.Month && aCtx.Request.Selection.Interval > 1) ||
                aCtx.Request.Selection.Periodicity == Periodicity.Year;
        }

        private List<Bar> CombineBars(List<Bar> aBars, Periodicity aPeriodicity, int aInterval)
        {
            List<Bar> aCombinedBars = new List<Bar>();
            if (aBars.Count == 0)
                return aBars;

            Bar lastBar = new Bar(){ 
                Date = aBars[0].Date, 
                Open = aBars[0].Open,
                High = aBars[0].High,
                Low = aBars[0].Low,
                Close = aBars[0].Close,
                Volume = aBars[0].Volume
            };
            if (aPeriodicity == Periodicity.Hour)
            {
                lastBar.Date = new DateTime(lastBar.Date.Year, lastBar.Date.Month, lastBar.Date.Day, lastBar.Date.Hour, 0, 0);
            }

            aCombinedBars.Add(lastBar);
            switch (aPeriodicity)
            {
                case Periodicity.Hour:
                    for (int i = 1; i < aBars.Count; i++)
                    {
                        if ((aBars[i].Date - lastBar.Date).TotalHours >= aInterval)
                        {
                            lastBar = new Bar()
                            {
                                Date = aBars[i].Date,
                                Open = aBars[i].Open,
                                High = aBars[i].High,
                                Low = aBars[i].Low,
                                Close = aBars[i].Close,
                                Volume = aBars[i].Volume
                            };
                            lastBar.Date = new DateTime(lastBar.Date.Year, lastBar.Date.Month, lastBar.Date.Day, lastBar.Date.Hour, 0, 0);
                            aCombinedBars.Add(lastBar);
                        }
                        else
                        {
                            lastBar.High = Math.Max(lastBar.High, aBars[i].High);
                            lastBar.Low = Math.Min(lastBar.Low, aBars[i].Low);
                            lastBar.Close = aBars[i].Close;
                            lastBar.Volume += aBars[i].Volume;
                        }
                    }
                    break;
                case Periodicity.Month:
                    for (int i = 1; i < aBars.Count; i++)
                    {
                        if (DifferentMonthCount(lastBar.Date,aBars[i].Date) >= aInterval)
                        {
                             lastBar = new Bar(){ 
                                Date = aBars[i].Date, 
                                Open = aBars[i].Open,
                                High = aBars[i].High,
                                Low = aBars[i].Low,
                                Close = aBars[i].Close,
                                Volume = aBars[i].Volume
                            };
                            aCombinedBars.Add(lastBar);
                        }
                        else
                        {
                            lastBar.High = Math.Max(lastBar.High, aBars[i].High);
                            lastBar.Low = Math.Min(lastBar.Low, aBars[i].Low);
                            lastBar.Close = aBars[i].Close;
                            lastBar.Volume += aBars[i].Volume;
                        }
                    }
                    break;
                case Periodicity.Year:
                    for (int i = 1; i < aBars.Count; i++)
                    {
                        if (lastBar.Date.Year + aInterval <= aBars[i].Date.Year)
                        {
                            lastBar = new Bar()
                            {
                                Date = aBars[i].Date,
                                Open = aBars[i].Open,
                                High = aBars[i].High,
                                Low = aBars[i].Low,
                                Close = aBars[i].Close,
                                Volume = aBars[i].Volume
                            };
                            aCombinedBars.Add(lastBar);
                        }
                        else
                        {
                            lastBar.High = Math.Max(lastBar.High, aBars[i].High);
                            lastBar.Low = Math.Min(lastBar.Low, aBars[i].Low);
                            lastBar.Close = aBars[i].Close;
                            lastBar.Volume += aBars[i].Volume;
                        }
                    }
                    break;
                default:
                    break;
            }

            return aCombinedBars;
        }

        private int DifferentMonthCount(DateTime firstDate, DateTime lastDate)
        {
            if (firstDate.Year == lastDate.Year)
                return lastDate.Month - firstDate.Month;
            else
            {
                return (lastDate.Year - firstDate.Year - 1) * 12 + 12 - firstDate.Month + lastDate.Month;
            }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                if (m_TimeZoneInfo == null)
                {
					var test = TimeZoneInfo.GetSystemTimeZones();

                    foreach (TimeZoneInfo item in TimeZoneInfo.GetSystemTimeZones())
                    {
						if (item.StandardName == "Eastern Standard Time" || item.StandardName == "EST")
                        {
                            m_TimeZoneInfo = item;
                            break;
                        }
                    }
                }

                return m_TimeZoneInfo;
            }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public string BuildSymbolName(string aStandard, Instrument aType)
        {
            string ret = aStandard;

            switch (aType)
            {
                case Instrument.Equity:
                    break;
                case Instrument.Option:
                    break;
                case Instrument.Forex:
                    ret = "^" + aStandard.Substring(0, 3) + aStandard.Substring(4, 3);
                    break;
                default:
                    break;
            }

            return ret;
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public bool TryParse(string aSymbol, out string aStandardName, out Instrument aType)
        {
            switch (aSymbol[0])
            {
                case '^':
                    aStandardName = aSymbol.Substring(1, 3) + "/" + aSymbol.Substring(4, 3);
                    aType = Instrument.Forex;
                    break;
                default:
                    aStandardName = aSymbol;
                    aType = Instrument.Equity;
                    break;
            }

            return true;
        }
        #endregion

        #region Ddf Events

        /// <summary>
        /// occurs when data feed sends a quote
        /// </summary>
        /// 
        void OnDdfClient_NewQuote(object sender, Client.NewQuoteEventArgs e)
        {
            try
            {
                string aSymbolName;
                Instrument aType;
                Session session = e.Quote.Sessions[Sessions.Combined];
                if (!TryParse(e.Quote.Symbol, out aSymbolName, out aType))
                {
                    return;
                }

                if (NewQuote != null && m_SubscribedSymbol.Contains(aSymbolName))
                {
                    SymbolItem aSymbol = new SymbolItem() { DataFeed = Name, Exchange = MarketName, Symbol = aSymbolName, Type = aType };
                    NewQuote(Name, this.TimeZoneInfo, new Tick
                    {
                        Symbol = aSymbol,
                        //Date = e.Quote.Timestamp.ToUniversalTime(),
                        Date = TimeZoneInfo.ConvertTimeToUtc(e.Quote.Timestamp, TimeZoneInfo),
                        Price = session == null ? (e.Quote.Ask + e.Quote.Bid) / 2 : session.Last,
                        Volume = session == null ? 0 : session.LastSize,
                        Bid = e.Quote.Bid,
                        BidSize = e.Quote.BidSize,
                        Ask = e.Quote.Ask,
                        AskSize = e.Quote.AskSize
                    });
                }

                if (NewLevel2 != null && m_Level2SubscribedSymbol.Contains(aSymbolName))
                {
                    SymbolItem aSymbol = new SymbolItem() { DataFeed = Name, Exchange = MarketName, Symbol = aSymbolName, Type = aType };

                    Level2Data level2 = new Level2Data();
                    level2.Symbol = aSymbol;
                    level2.Asks = new Level2Item[] { new Level2Item{ 
                        MarketMaker = Level1MarketMaker, 
                        Price = e.Quote.Ask, 
                        Quantity = (int)e.Quote.AskSize }};
                    level2.Bids = new Level2Item[] { new Level2Item{ 
                        MarketMaker = Level1MarketMaker, 
                        Price = e.Quote.Bid, 
                        Quantity = (int)e.Quote.BidSize }};

                    NewLevel2(Name, this.TimeZoneInfo, level2);
                }
            }
            catch (Exception ex)
            {
                WriteToLog("---OnDdfClient_NewQuote Exception", ex);
            }
        }
        
        void Connection_StatusChange(object sender, ddfplus.Net.StatusChangeEventArgs e)
        {
            WriteToLog("----Connection Status " + e.NewStatus.ToString(), null);
            m_ConnectionStatus = e.NewStatus;
        }

        void m_DDFClient_Error(object sender, Client.ErrorEventArgs e)
        {
            WriteToLog("-----DDF error:" + e.Error.ToString() + " description:" + e.Description, null);
        }

        #endregion

        #region Private methods

        private void UpdateSubscriberSymbols()
        {
            //m_DDFClient.Symbols = "";
            m_DDFClient.Symbols = String.Join(",", m_DDFSymbols); //symbols;
        }

        /// <summary>
        /// sends request
        /// </summary>
        /// <param name="URL">contains request parameters</param>
        /// <returns>response</returns>
        private string OpenURL(string URL)
        {
            string ret = "";

            try
            {
                WebClient aClient = new WebClient();

                aClient.Proxy = null; 
                ret = aClient.DownloadString(URL);
            }
            catch (Exception e)
            {
                WriteToLog(null, e); 
            }

            return ret;
        }

        private string BuildUrl2(HistoryParameters aRequest, DateTime aFromF, DateTime aToF, int nBars)
        {
            string aPage;
            string aURL;
            string requestData;
            string aStart = aFromF.ToString("yyyyMMddHHmmss");
            string aEnd = aToF.ToString("yyyyMMddHHmmss");
            int aInterval = aRequest.Interval;

            switch (aRequest.Periodicity)
            {
                case Periodicity.Minute:
                    aPage = "queryminutes";
                    requestData = "minute";
                    break;
                case Periodicity.Hour:
                    aPage = "queryminutes";
                    requestData = "minute";
                    if (aInterval >= 18)
                        aInterval = 60;
                    else
                        aInterval = aInterval * 60;
                    break;
                case Periodicity.Day:
                    aPage = "queryeod";
                    requestData = "daily";
                    break;
                case Periodicity.Week:
                    aPage = "queryeod";
                    requestData = "weekly";
                    break;
                case Periodicity.Month:
                    aPage = "queryeod";
                    requestData = "monthly";
                    break;
                case Periodicity.Year:
                    aPage = "queryeod";
                    requestData = "monthly";
                    aInterval = aInterval * 12;
                    break;
                default:
                    throw new ApplicationException("{0} periodicity is not supported");
            }

            aURL = string.Format("{0}?username={1}&password={2}&symbol={3}&interval={4}&start={5}&end={6}&maxrecords={7}&data={8}&format=csv&order=desc"
                , String.Format(HistoryBaseUrl, aPage)
                , HttpUtility.UrlEncode(m_UserName), HttpUtility.UrlEncode(m_Password), BuildSymbolName(aRequest.Symbol.Symbol, aRequest.Symbol.Type),
                aInterval, aStart, aEnd, nBars, requestData);
            
            return aURL;
        }

        #endregion
    }


}
