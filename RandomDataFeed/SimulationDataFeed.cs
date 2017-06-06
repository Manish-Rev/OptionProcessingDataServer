using System;
using System.Collections.Generic;
using System.Linq;
using CommonObjects;
using System.IO;
using System.Threading;

namespace RandomDataFeed
{
    /// <summary>
    /// Implements non-real data feeder used to test an trade client application
    /// </summary>
    public class SimulationDataFeed : DataServer.Logger, IDataFeed
    {
        private const string MarketName = "Simulation";
        private readonly HashSet<SymbolItem> m_SubscribeSymbol = new HashSet<SymbolItem>();
        private readonly HashSet<SymbolItem> m_Level2SubscribeSymbol = new HashSet<SymbolItem>();
        private readonly Dictionary<SymbolItem, double> m_LastPrice = new Dictionary<SymbolItem, double>();
        private readonly Dictionary<SymbolItem, double> m_StartPrice = new Dictionary<SymbolItem, double>();
        private readonly Dictionary<SymbolItem, int> m_RealtimeStep = new Dictionary<SymbolItem, int>();

        private int m_MaxRealtimeShift = 100;
        
        public event NewQuoteHandler NewQuote;
        public event NewLevel2DataHandler NewLevel2;
        private Thread m_RealTimeGenerator;
        private Random m_Random;
        private string m_Path;
        private const int MarketMakerCount = 10;
        private List<string> m_MarketMakers = new List<string>();
        private List<ExchangeInfo> _Markets = null;

        public SimulationDataFeed()
            : base("Simulation DataFeed")
        {
            for (int i = 1; i <= MarketMakerCount; i++)
            {
                m_MarketMakers.Add("MM" + i.ToString());
            }
        }

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
            get { return null; }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public Dictionary<string, object> DefaultSettings
        {
            get { return new Dictionary<string,object>(); }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public void Start(Dictionary<string, object> aParams, string aPath)
        {
            WriteToLog_Info("Starting...", null);

            m_Path = aPath;
            m_Random = new Random();
            m_RealTimeGenerator = new Thread(ThreadFunc_RealTimeGenerator);
            m_RealTimeGenerator.IsBackground = true;
            m_RealTimeGenerator.Start();
            WriteToLog_Info("Started.", null);
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public void Stop()
        {
            WriteToLog_Info("Stopping...", null);
            try
            {
                m_RealTimeGenerator.Abort();
            }
            catch (Exception)
            {
            }
            m_RealTimeGenerator = null;
            m_Random = null;
            lock (m_SubscribeSymbol)
            {
                m_SubscribeSymbol.Clear();
            }
            lock (m_LastPrice)
            {
                m_LastPrice.Clear();
                m_StartPrice.Clear();
            }
            WriteToLog_Info("Stopped.", null);
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public void Subscribe(SymbolItem aSymbol)
        {
            lock (m_SubscribeSymbol)
            {
                if (!m_SubscribeSymbol.Contains(aSymbol))
                    m_SubscribeSymbol.Add(aSymbol);
            }
        }

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public void UnSubscribe(SymbolItem aSymbol)
        {
            lock (m_SubscribeSymbol)
            {
                 m_SubscribeSymbol.Remove(aSymbol);
            }
        }

        public void SubscribeLevel2(SymbolItem aSymbol)
        {
            lock (m_Level2SubscribeSymbol)
            {
                if (!m_Level2SubscribeSymbol.Contains(aSymbol))
                    m_Level2SubscribeSymbol.Add(aSymbol);
            }
        }

        public void UnsubscribeLevel2(SymbolItem aSymbol)
        {
            lock (m_Level2SubscribeSymbol)
            {
                m_Level2SubscribeSymbol.Remove(aSymbol);
            }
        }

        public void GetHistory(GetHistoryCtx aCtx, HistoryAnswerHandler aCallback)
        {
            try
            {
                double lastPrice = GetLastPrice(aCtx.Request.Selection.Symbol);

                List<Bar> bars;
                switch (aCtx.Request.Selection.Periodicity)
                {
                    case Periodicity.Tick:
                        bars = GenerateTickHistory(aCtx.Request.Selection.Interval, aCtx.MaxBars, lastPrice);
                        break;
                    case Periodicity.Range:
                        bars = GenerateRangeHistory(aCtx.Request.Selection.Interval, aCtx.MaxBars, lastPrice);
                        break;
                    default:
                        bars = GenerateHistory(aCtx.Request.Selection, aCtx.MaxBars, lastPrice);
                        break;
                }

                bars.Reverse();
                aCallback(aCtx, bars);
            }
            catch (Exception e)
            {
                WriteToLog("GetHistory", e);
            }
        }

        private List<Bar> GenerateHistory(HistoryParameters chartSelection, int maxBars, double lastPrice)
        {
            List<Bar> bars = new List<Bar>();
            DateTime barTime = DateTime.UtcNow;
            barTime = new DateTime(barTime.Year, barTime.Month, barTime.Day, barTime.Hour, barTime.Minute, 0);

            for (int i = 0; i < maxBars; i++)
            {
                Bar aBar = new Bar
                {
                    Date = barTime,
                    Close = lastPrice,
                    High = lastPrice + (double)m_Random.Next(1000) / 100,
                    Low = lastPrice - (double)m_Random.Next(1000) / 100,
                    Volume = m_Random.Next(1000000)
                };
                aBar.Open = lastPrice + (aBar.High - lastPrice > lastPrice - aBar.Low ? -1 : 1) * (double)m_Random.Next(1000) / 100;
                aBar.High = Math.Max(aBar.High, aBar.Open);
                aBar.Low = Math.Min(aBar.Low, aBar.Open);

                bars.Add(aBar);

                barTime = PrevDate(barTime, chartSelection.Periodicity, chartSelection.Interval);
                lastPrice = aBar.Open;
            }

            return bars;
        }

        private List<Bar> GenerateTickHistory(int interval, int maxBars, double lastPrice)
        {
            List<Bar> bars = new List<Bar>();
            DateTime barTime = DateTime.UtcNow;
            barTime = new DateTime(barTime.Year, barTime.Month, barTime.Day, barTime.Hour, barTime.Minute, 0);
            var tick = new HistoryTickItem(barTime, lastPrice, 0);
            for (int i = 0; i < maxBars; i++)
            {
                tick = GenerateHistoryTick(tick);
                Bar aBar = new Bar
                {
                    Date =tick.Time,
                    Open = tick.Price,
                    Close = tick.Price,
                    High = tick.Price,
                    Low = tick.Price,
                    Volume = tick.Volume
                };
                for (int t = 1; t < interval; t++)
                {
                    tick = GenerateHistoryTick(tick);

                    aBar.Open = tick.Price;
                    aBar.High = Math.Max(aBar.High, aBar.Open);
                    aBar.Low = Math.Min(aBar.Low, aBar.Open);
                    aBar.Volume += tick.Volume;
                }
                bars.Add(aBar);
            }

            return bars;
        }
        
        private double _tickSize = 0.01;
        private List<Bar> GenerateRangeHistory(int interval, int maxBars, double lastPrice)
        {
            int maxTickGenerateCount = 50;

            double range = _tickSize * interval;

            List<Bar> bars = new List<Bar>();
            DateTime barTime = DateTime.UtcNow;
            barTime = new DateTime(barTime.Year, barTime.Month, barTime.Day, barTime.Hour, barTime.Minute, 0);
            var tick = new HistoryTickItem(barTime, lastPrice, 0);
            for (int i = 0; i < maxBars; i++)
            {
                tick = GenerateHistoryTick(tick);
                Bar aBar = new Bar
                {
                    Date = tick.Time,
                    Open = tick.Price,
                    Close = tick.Price,
                    High = tick.Price,
                    Low = tick.Price,
                    Volume = tick.Volume
                };
                bool created = false;
                for (int t = 0; t < maxTickGenerateCount; t++)
                {
                    tick = GenerateHistoryTick(tick);

                    aBar.Close = tick.Price;
                    aBar.High = Math.Max(aBar.High, aBar.Close);
                    aBar.Low = Math.Min(aBar.Low, aBar.Close);
                    aBar.Volume += tick.Volume;
                    if (aBar.High - aBar.Low > range)
                    {
                        created = true;
                        break;
                    }
                }
                if (!created)
                {
                    if (aBar.Close - aBar.Low < aBar.High - aBar.Close)
                    {
                        aBar.Close = aBar.Low = aBar.High - range - (double)m_Random.Next(1000) / 1000;
                    }
                    else
                    {
                        aBar.Close = aBar.High = aBar.Low + range + (double)m_Random.Next(1000) / 1000;
                    }
                }

                bars.Add(aBar);
            }

            return bars;
        }

        private HistoryTickItem GenerateHistoryTick(HistoryTickItem tick)
        {
            DateTime date = tick.Time.AddSeconds(-m_Random.NextDouble());
            double priceShift = (m_Random.NextDouble() > 0.5 ? 1 : -1) * (double)m_Random.Next(1000) / 100;
            int volume = m_Random.Next(1000000);
            return new HistoryTickItem(date, tick.Price + priceShift, volume);
        }
        

        /// <summary>
        /// see IDataFeeder interface
        /// </summary>
        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                return TimeZoneInfo.Local;
            }
        }

        public string BuildSymbolName(string aStandard, Instrument aType)
        {
            return aStandard;
        }

        public bool TryParse(string aSymbol, out string aStandardName, out Instrument aType)
        {
            aStandardName = aSymbol;
            aType = Instrument.Unknown;

            return true;
        }
        #endregion

        #region RealTime Simulation
        /// <summary>
        /// Generate level 1 data
        /// </summary>
        private void ThreadFunc_RealTimeGenerator()
        {
            while (true)
            {
                List<SymbolItem> aList;

                lock (m_SubscribeSymbol)
                {
                    aList = new List<SymbolItem>(m_SubscribeSymbol);
                }
                foreach (SymbolItem item in aList)
                {
                    Tick tick = new Tick
                        {
                            Symbol = new SymbolItem() { DataFeed = Name, Exchange = MarketName, Symbol = item.Symbol, Type = item.Type },
                            Date = DateTime.UtcNow
                        };

                    double lastPrice = GetLastPrice(item);
                    
                
                    tick.Price = lastPrice;
                    tick.Bid = lastPrice - (double)m_Random.Next(10) / 1000;
                    tick.BidSize = m_Random.Next(10000);
                    tick.Ask = lastPrice + (double)m_Random.Next(10) / 1000;
                    tick.AskSize = m_Random.Next(10000);
                    tick.Volume = m_Random.Next(1000000);

                    int realtimeStep;
                    lock (m_RealtimeStep)
                    {
                        if (!m_RealtimeStep.TryGetValue(item, out realtimeStep))
                        {
                            realtimeStep = 0;
                        }
                    }

                    if (realtimeStep != 0)
                    {
                        lastPrice = lastPrice + (realtimeStep >= 0 ? 1 : -1) * (double)m_Random.Next(50) / 100;
                    }
                    else
                    {

                    }
                    lock (m_LastPrice)
                    {
                        m_LastPrice[item] = lastPrice;
                    }
                    
                    if (realtimeStep == 0)
                    {
                        //
                        double askMultiply = GetRealtimeTrend(item);
                        //
                        realtimeStep = ((tick.Ask - tick.Price)*askMultiply >= (tick.Price - tick.Bid)/askMultiply ? 1 : -1) * m_Random.Next(10, 100) / 10;
                    }
                    else
                    {
                        if (realtimeStep < 0)
                            realtimeStep += 1;
                        else
                            realtimeStep -= 1;
                    }
                    lock (m_RealtimeStep)
                    {
                        m_RealtimeStep[item] = realtimeStep;
                    }

                    if (NewQuote != null)
                    {
                        NewQuote(Name, this.TimeZoneInfo, tick);
                    }
                }

                lock (m_Level2SubscribeSymbol)
                {
                    aList = new List<SymbolItem>(m_Level2SubscribeSymbol);
                }
                foreach (SymbolItem item in aList)
                {
                    double lastPrice = GetLastPrice(item);
                    
                    Level2Data level2 = new Level2Data
                    {
                        Symbol =  new SymbolItem() { DataFeed = Name, Exchange = MarketName, Symbol = item.Symbol, Type = item.Type },
                        Asks = new List<Level2Item>(),
                        Bids = new List<Level2Item>()
                    };
                    foreach (var mm in m_MarketMakers)
	                {
                        level2.Asks.Add(new Level2Item{
                            MarketMaker = mm,
                            Price = lastPrice + (double)m_Random.Next(10) / 1000,
                          Quantity = m_Random.Next(10000)
                        });
                        level2.Bids.Add(new Level2Item{
                            MarketMaker = mm,
                            Price = lastPrice - (double)m_Random.Next(10) / 1000,
                          Quantity = m_Random.Next(10000)
                        });
                    }
                    if (NewLevel2!= null)
                    {
                        NewLevel2(Name, this.TimeZoneInfo, level2);
                    }
                }

                Thread.Sleep(300);
            }
        }

        /// <summary>
        /// Get date to previouse bar
        /// </summary>
        /// <param name="date">curent bar date</param>
        /// <param name="period">historical period</param>
        /// <param name="interval">interval</param>
        /// <returns></returns>
        private DateTime PrevDate(DateTime date, Periodicity period, int interval)
        {
            DateTime newDate;
            switch (period)
            {
                case Periodicity.Second:
                    newDate = date.AddSeconds(-interval);
                    break;
                case Periodicity.Minute:
                    newDate = date.AddMinutes(-interval);
                    break;
                case Periodicity.Hour:
                    newDate = date.AddHours(-interval);
                    break;
                case Periodicity.Day:
                    newDate = date.AddDays(-interval);
                    break;
                case Periodicity.Week:
                    newDate = date.AddDays(-interval*7);
                    break;
                case Periodicity.Month:
                    newDate = date.AddMonths(-interval);
                    break;
                case Periodicity.Year:
                    newDate = date.AddYears(-interval);
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    newDate = date;
                    break;
            }

            return newDate;
        }

        private double GetLastPrice(SymbolItem smbItem)
        {
            double lastPrice;
            lock (m_LastPrice)
            {
                if (!m_LastPrice.TryGetValue(smbItem, out lastPrice))
                {
                    lastPrice = (double)m_Random.Next(100000) / 100;
                    m_LastPrice[smbItem] = lastPrice;
                    m_StartPrice[smbItem] = lastPrice;
                }
            }
            return lastPrice;
        }

        private double GetRealtimeTrend(SymbolItem smbItem)
        {
            double lastPrice = 0;
            double startPrice = 0;
            lock (m_LastPrice)
            {
                m_LastPrice.TryGetValue(smbItem, out lastPrice);
                m_StartPrice.TryGetValue(smbItem, out startPrice);
            }

            if (lastPrice < startPrice * 0.75 || lastPrice < 10)
            {
                return 1.5;
            }
            else if (lastPrice > startPrice * 1.25)
            {
                return 0.5;
            }
            else
            {
                return 1;
            }

            //more then 1 - need trend to up
            //less then 1 - need trend to down
            //1 - simulation

        }

        #endregion
    }

    public class HistoryTickItem
    {
        public DateTime Time;
        public double Price;
        public int Volume;

        public HistoryTickItem(DateTime aTime, double aPrice, int aVolume)
        {
            Time = aTime;
            Price = aPrice;
            Volume = aVolume;
        }
    }
        
}
