using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CommonObjects;
using Extensions;

namespace DataServer
{
    public class MessageProcessor : Logger
    {
        #region Variables

        private readonly Dictionary<string, IDataFeed> m_DataFeedsByName = new Dictionary<string, IDataFeed>();
        private readonly Dictionary<string, IDataServerServiceHost> m_SessionManagersByName = new Dictionary<string, IDataServerServiceHost>();
        private readonly Dictionary<SymbolItem, Level1Subscribers> m_Level1SubscribersBySymbols = new Dictionary<SymbolItem, Level1Subscribers>();
        private readonly Dictionary<SymbolItem, Level2Subscribers> m_Level2SubscribersBySymbols = new Dictionary<SymbolItem, Level2Subscribers>();
        private readonly Queue<RequestMessage> m_InMessages = new Queue<RequestMessage>();
        private readonly Dictionary<string, Queue<ResponseMessage>> m_OutMessages = new Dictionary<string, Queue<ResponseMessage>>();
        private readonly Dictionary<string, Queue<Tick>> m_Ticks = new Dictionary<string, Queue<Tick>>();
        private TimeZoneInfo m_EST;
        private int m_MaxBlockSizeInDays = 7;
        
        #endregion

        #region Public

        public MessageProcessor()
            : base("MsgProc")
        {
        }

        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                if (m_EST == null)
                {
                    foreach (TimeZoneInfo item in TimeZoneInfo.GetSystemTimeZones())
                    {
                        if (item.StandardName == "Eastern Standard Time")
                        {
                            m_EST = item;
                            break;
                        }
                    }
                }

                return m_EST;
            }
        }

        public void Start(List<DataFeedItem> aDataFeeds, List<ConnServiceHostItem> aSessionManagers, Dictionary<string, object> aAdditionalParams)
        {
            foreach (DataFeedItem item in aDataFeeds)
            {
                if (item.State == "Started")
                {
                    item.DataFeed.NewQuote += OnNewQuote_DataFeed;
                    item.DataFeed.NewLevel2 += DataFeed_NewLevel2;
                    m_DataFeedsByName.Add(item.DataFeed.Name, item.DataFeed);
                }
            }
            foreach (ConnServiceHostItem item in aSessionManagers)
            {
                if (item.State == "Started")
                    m_SessionManagersByName.Add(item.Host.Name, item.Host);
            }
            DataServer.MessageRouter.gMessageRouter.RouteRequest += OnRouteRequest;
            DataServer.MessageRouter.gMessageRouter.RemovedSession += OnRemovedSession;
        }

        public void Stop()
        {
            DataServer.MessageRouter.gMessageRouter.RouteRequest -= OnRouteRequest;
            DataServer.MessageRouter.gMessageRouter.RemovedSession -= OnRemovedSession;
            m_DataFeedsByName.Clear();
            m_SessionManagersByName.Clear();
            lock (m_InMessages)
            {
                m_InMessages.Clear();
            }
            lock (m_OutMessages)
            {
                m_OutMessages.Clear();
            }
            lock (m_Level1SubscribersBySymbols)
            {
                m_Level1SubscribersBySymbols.Clear();
            }
        }

        #endregion

        #region event handlers

        private void OnRouteRequest(object sender, MessageRouter.MessageRouter_EventArgs args)
        {
            lock (m_InMessages)
            {
                args.Request.User = args.UserInfo;
                m_InMessages.Enqueue(args.Request);
                if (m_InMessages.Count == 1)
                    ThreadPool.QueueUserWorkItem(o => ThreadFunc_RequestWorker());
            }
        }

        void OnRemovedSession(object sender, MessageRouter.MessageRouter_EventArgs e)
        {
            UnsubscribeSymbolsBySessionID(e.ID);
        }

        #endregion

        #region DataFeeds

        private void OnNewQuote_DataFeed(string aDataFeed, TimeZoneInfo aTZI, Tick aTick)
        {
            Level1Subscribers aSubscribers;

            lock (m_Level1SubscribersBySymbols)
            {
                m_Level1SubscribersBySymbols.TryGetValue(aTick.Symbol, out aSubscribers);
            }
            if (aSubscribers == null)
                return;

            CheckBarSubscribers(aTick, aSubscribers);

            //aTick.Date = TimeZoneInfo.ConvertTimeToUtc(aTick.Date, aTZI);

            List<string> aUsers;

            lock (aSubscribers)
            {
                aSubscribers.Tick = aTick;
                aUsers = new List<string>(aSubscribers.Subscribers);
            }
            foreach (string item in aUsers)
            {
                PushTick(item, aTick);
            }
        }

        void DataFeed_NewLevel2(string dataFeed, TimeZoneInfo aTZI, Level2Data level2)
        {
            Level2Subscribers aSubscribers;

            lock (m_Level2SubscribersBySymbols)
            {
                m_Level2SubscribersBySymbols.TryGetValue(level2.Symbol, out aSubscribers);
            }
            if (aSubscribers == null)
                return;

            List<string> aUsers;

            lock (aSubscribers)
            {
                aUsers = new List<string>(aSubscribers.Subscribers);
            }
            foreach (string item in aUsers)
            {
                var response = new L2SubscribeResponse
                {
                    Level2 = level2,
                    User = DataServer.MessageRouter.gMessageRouter.GetUserInfo(item),
                };
                PushResponses(item, new ResponseMessage[] { response });
            }
        }

        #endregion

        #region Manage Messages Methods

        private void ThreadFunc_RequestWorker()
        {
            bool bContinue = true;

            while (bContinue)
            {
                RequestMessage aRequest = null;

                try
                {
                    lock (m_InMessages)
                    {
                        if (m_InMessages.Count > 0)
                        {
                            aRequest = m_InMessages.Peek();
                        }
                    }
                    if (aRequest == null)
                        break;

                    try
                    {
                        if (aRequest is DataFeedListRequest)
                            GetDataFeedList((DataFeedListRequest)aRequest);
                        else if (aRequest is SubscribeRequest)
                            SubscribeRequest((SubscribeRequest)aRequest);
                        else if (aRequest is UnsubscribeRequest)
                            UnsubscribeRequest((UnsubscribeRequest)aRequest);
                        else if (aRequest is HistoryRequest)
                            GetHistoryRequest((HistoryRequest)aRequest);
                        else if (aRequest is HeartbeatRequest)
                            HeartBeatRequest((HeartbeatRequest)aRequest);
                        else if (aRequest is AlertSubscribeRequest)
                            SubscribeAlert((AlertSubscribeRequest)aRequest);
                        else if (aRequest is AlertUnsubscribeRequest)
                            UnsubscribeAlert((AlertUnsubscribeRequest)aRequest);
                        else if (aRequest is AlertsHistoryRequest)
                            this.SendAlertsHistory((AlertsHistoryRequest)aRequest);
                        else if (aRequest is BacktestGetRequest)
                            RunBacktest((BacktestGetRequest)aRequest);
                        else if (aRequest is L2SubscribeRequest)
                            SubscribeLevel2((L2SubscribeRequest)aRequest);
                        else if (aRequest is L2UnsubscribeRequest)
                            UnsubscribeLevel2((L2UnsubscribeRequest)aRequest);
                        else
                            throw new ApplicationException("Unrecognized incoming message");
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    finally
                    {
                        lock (m_InMessages)
                        {
                            if (m_InMessages.Count > 0)
                                m_InMessages.Dequeue();

                            if (m_InMessages.Count == 0)
                                bContinue = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteToLog("ThreadFunc_RequestWorker", e);
                    try
                    {
                        aRequest.User.SendError(e);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void ThreadFunc_ResponseWorker(Queue<ResponseMessage> aResponses)
        {
            ResponseMessage aResponse;
            bool bContinue = true;

            while (bContinue)
            {
                try
                {
                    aResponse = null;
                    lock (aResponses)
                    {
                        if (aResponses.Count > 0)
                            aResponse = aResponses.Peek();
                    }
                    if (aResponse == null)
                        break;
                    try
                    {
                        aResponse.User.Send(aResponse);
                    }
                    finally
                    {
                        lock (aResponses)
                        {
                            if (aResponses.Count > 0)
                                aResponses.Dequeue();

                            if (aResponses.Count == 0)
                                bContinue = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteToLog("ThreadFunc_ResponseWorker", e);
                }
            }
        }

        private void TickMessageOutWorker(string aSessionID, Queue<Tick> aTicks)
        {
            IUserInfo aUserInfo;
            const int aMaxPackSize = 20;

            aUserInfo = DataServer.MessageRouter.gMessageRouter.GetUserInfo(aSessionID);
            if (aUserInfo == null)
            {
                lock (aTicks)
                {
                    aTicks.Clear();

                    return;
                }
            }
            try
            {
                while (true)
                {
                    NewTickResponse aResponse = new NewTickResponse();
                    bool bEmptyQueue = false;

                    aResponse.Tick = new List<Tick>();
                    aResponse.User = aUserInfo;
                    lock (aTicks)
                    {
                        while (aResponse.Tick.Count < aMaxPackSize)
                        {
                            if (aTicks.Count > 1)
                                aResponse.Tick.Add(aTicks.Dequeue());
                            else if (aTicks.Count == 1)
                            {
                                bEmptyQueue = true;
                                aResponse.Tick.Add(aTicks.Peek());
                                break;
                            }
                            else
                            {
                                bEmptyQueue = true;
                                break;
                            }
                        }
                    }
                    if (aResponse.Tick.Count > 0)
                        aResponse.User.Send(aResponse);
                    if (bEmptyQueue)
                    {
                        lock (aTicks)
                        {
                            if (aTicks.Count > 0)
                                aTicks.Dequeue();
                            if (aTicks.Count == 0)
                                break;
                        }
                    }
                }
            }
            catch
            {
                lock (aTicks)
                {
                    aTicks.Clear();
                }
            }
        }

        private void PushResponses(string aID, IEnumerable<ResponseMessage> aCollection)
        {
            bool bStart;
            Queue<ResponseMessage> aResponses;

            lock (m_OutMessages)
            {
                if (!m_OutMessages.TryGetValue(aID, out aResponses))
                {
                    aResponses = new Queue<ResponseMessage>();
                    m_OutMessages.Add(aID, aResponses);
                }
            }
            lock (aResponses)
            {
                bStart = (aResponses.Count == 0);
                foreach (ResponseMessage item in aCollection)
                {
                    aResponses.Enqueue(item);
                }
            }
            if (bStart)
                ThreadPool.QueueUserWorkItem(o => ThreadFunc_ResponseWorker(aResponses));
        }

        private void PushResponse(ResponseMessage aResponse)
        {
            if (aResponse is HistoryResponse)
            {
                HistoryResponse aHistResponse = (HistoryResponse)aResponse;
                List<ResponseMessage> aList = new List<ResponseMessage>();

                if (aHistResponse.Bars.Count > 1000)
                {
                    int i = 0;

                    while (i < aHistResponse.Bars.Count)
                    {
                        HistoryResponse resp = new HistoryResponse();
                        int n = Math.Min(1000, aHistResponse.Bars.Count - i);

                        resp.User = aHistResponse.User;
                        resp.ID = aHistResponse.ID;
                        resp.Bars = new List<Bar>(aHistResponse.Bars.GetRange(i, n));
                        i += n;
                        aList.Add(resp);
                    }
                }
                else
                    aList.Add(aResponse);
                ((HistoryResponse)aList[aList.Count - 1]).Tail = true;
                PushResponses(aResponse.User.ID, aList);
            }
            else
            {
                PushResponses(aResponse.User.ID, new ResponseMessage[] { aResponse });
            }
        }

        private void PushTick(string aSessionID, Tick tick)
        {
            Queue<Tick> aMessages;

            lock (m_Ticks)
            {
                if (!m_Ticks.TryGetValue(aSessionID, out aMessages))
                {
                    aMessages = new Queue<Tick>();
                    m_Ticks.Add(aSessionID, aMessages);
                }
            }
            lock (aMessages)
            {
                aMessages.Enqueue(tick);
                if (aMessages.Count == 1)
                    ThreadPool.QueueUserWorkItem(o => TickMessageOutWorker(aSessionID, aMessages));
            }
        }

        #endregion

        #region Messages Request

        private void Validate(SubscribeRequest aRequest)
        {
            SymbolItem aSymbol = aRequest.Symbol;

            if (aSymbol.Type == Instrument.Unknown)
                throw new ApplicationException("Invalid instrument");
        }

        private void Validate(UnsubscribeRequest aRequest)
        {
            SymbolItem aSymbol = aRequest.Symbol;

            if (aSymbol.Type == Instrument.Unknown)
                throw new ApplicationException("Invalid instrument");
        }

        private void Validate(HistoryRequest aRequest)
        {
            SymbolItem aSymbol = aRequest.Selection.Symbol;

            if (aSymbol.Type == Instrument.Unknown)
                throw new ApplicationException("Invalid instrument");
        }

        private void SubscribeRequest(SubscribeRequest aRequest)
        {
            Level1Subscribers aLevel1Subscribers;
            Tick aTick;

            Validate(aRequest);
            lock (m_Level1SubscribersBySymbols)
            {
                if (!m_Level1SubscribersBySymbols.TryGetValue(aRequest.Symbol, out aLevel1Subscribers))
                {
                    aLevel1Subscribers = new Level1Subscribers();

                    aLevel1Subscribers.Subscribers = new List<string>();
                    aLevel1Subscribers.AlertSubscribers = new List<AlertSubscription>();
                    m_Level1SubscribersBySymbols.Add(aRequest.Symbol, aLevel1Subscribers);
                }
            }
            lock (aLevel1Subscribers)
            {
                if (!aLevel1Subscribers.Subscribers.Contains(aRequest.User.ID))
                    aLevel1Subscribers.Subscribers.Add(aRequest.User.ID);
                aTick = aLevel1Subscribers.Tick;
            }
            if (aTick != null)
                PushTick(aRequest.User.ID, aTick);

            IDataFeed aDataFeed = GetDataFeedByName(aRequest.Symbol.DataFeed);
            if (aDataFeed != null)
                aDataFeed.Subscribe(aRequest.Symbol);
        }

        private void UnsubscribeRequest(UnsubscribeRequest aRequest)
        {
            Validate(aRequest);
            UnsubscribeSymbolsBySessionID(aRequest.User.ID, new SymbolItem[] { aRequest.Symbol });
        }

        private IEnumerable<SymbolItem> GetSubscribedSymbols(string aSessionID)
        {
            List<SymbolItem> aList = new List<SymbolItem>();

            lock (m_Level1SubscribersBySymbols)
            {
                foreach (KeyValuePair<SymbolItem, Level1Subscribers> item_pair in m_Level1SubscribersBySymbols)
                {
                    if (item_pair.Value.Subscribers.Contains(aSessionID))
                        aList.Add(item_pair.Key);
                }
            }

            return aList;
        }

        private void UnsubscribeSymbolsBySessionID(string aSessionID)
        {
            List<SymbolItem> aList = new List<SymbolItem>();

            lock (m_Level1SubscribersBySymbols)
            {
                foreach (KeyValuePair<SymbolItem, Level1Subscribers> item_pair in m_Level1SubscribersBySymbols)
                {
                    lock (item_pair.Value.Subscribers)
                    {
                        if (item_pair.Value.Subscribers.Remove(aSessionID))
                        {
                            if (item_pair.Value.Subscribers.Count == 0 && item_pair.Value.AlertSubscribers.Count == 0)
                                aList.Add(item_pair.Key);
                        }
                    }
                }
            }
            foreach (SymbolItem item in aList)
            {
                IDataFeed aDataFeed = GetDataFeedByName(item.DataFeed);

                if (aDataFeed != null)
                    aDataFeed.UnSubscribe(item);
            }
        }

        private void UnsubscribeSymbolsBySessionID(string aSessionID, IEnumerable<SymbolItem> aCollection)
        {
            foreach (SymbolItem item in aCollection)
            {
                Level1Subscribers aLevel1Subscribers;
                bool bUnsubscribe = false;

                lock (m_Level1SubscribersBySymbols)
                {
                    if (!m_Level1SubscribersBySymbols.TryGetValue(item, out aLevel1Subscribers))
                        continue;
                }
                lock (aLevel1Subscribers)
                {
                    if (aLevel1Subscribers.Subscribers.Contains(aSessionID))
                    {
                        aLevel1Subscribers.Subscribers.Remove(aSessionID);
                        if (aLevel1Subscribers.Subscribers.Count == 0 && aLevel1Subscribers.AlertSubscribers.Count == 0)
                            bUnsubscribe = true;
                    }
                }
                if (bUnsubscribe)
                {
                    IDataFeed aDataFeed = GetDataFeedByName(item.DataFeed);

                    if (aDataFeed != null)
                        aDataFeed.UnSubscribe(item);
                }
            }
        }

        private void UnsubscribeLevel2SymbolsBySessionID(string aSessionID, IEnumerable<SymbolItem> aCollection)
        {
            foreach (SymbolItem item in aCollection)
            {
                Level2Subscribers aLevel2Subscribers;
                bool bUnsubscribe = false;

                lock (m_Level1SubscribersBySymbols)
                {
                    if (!m_Level2SubscribersBySymbols.TryGetValue(item, out aLevel2Subscribers))
                        continue;
                }
                lock (aLevel2Subscribers)
                {
                    if (aLevel2Subscribers.Subscribers.Contains(aSessionID))
                    {
                        aLevel2Subscribers.Subscribers.Remove(aSessionID);
                        if (aLevel2Subscribers.Subscribers.Count == 0)
                            bUnsubscribe = true;
                    }
                }
                if (bUnsubscribe)
                {
                    IDataFeed aDataFeed = GetDataFeedByName(item.DataFeed);

                    if (aDataFeed != null)
                        aDataFeed.UnsubscribeLevel2(item);
                }
            }
        }

        private bool IsWeekend(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    return true;
                case DayOfWeek.Sunday:
                    return true;
                default:
                    return false;
            }
        }

        private List<Bar> CalculateBars(GetHistoryCtx aCtx, List<Bar> aPrimitives)
        {
            List<Bar> aList = new List<Bar>();

            if (aCtx.Bars.Count == 0)
                return aList;
            if (aCtx.Request.Selection.Periodicity == Periodicity.Minute)
            {
                int aInterval = aCtx.Interval;

                if (aInterval > 1)
                {
                    int i = 0;

                    aCtx.Bars.Reverse();
                    while (i < aCtx.Bars.Count)
                    {
                        DateTime aStart = aCtx.Bars[i].Date;
                        int aRest = 0;
                        Bar aBar = new Bar();
                        List<Bar> aList2;
                        int i2;

                        aRest = aStart.TimeOfDay.Minutes % aInterval;
                        if (aRest != 0)
                            aStart = aStart.AddMinutes(-aRest);
                        i2 = aCtx.Bars.FindIndex(i, o => o.Date >= aStart.AddMinutes(aInterval));
                        if (i2 >= i)
                        {
                            if (i2 > i)
                                aList2 = aCtx.Bars.GetRange(i, Math.Min(i2 - i, aInterval));
                            else
                            {
                                aList2 = new List<Bar>();

                                aList2.Add(aCtx.Bars[i]);
                            }
                            aBar.Date = aStart;
                            aBar.Open = aList2[0].Open;
                            aBar.Close = aList2[aList2.Count - 1].Close;
                            aBar.High = aList2.Max(o => o.High);
                            aBar.Low = aList2.Min(o => o.Low);
                            aBar.Volume = aList2.Sum(o => o.Volume);
                            aStart = aStart.AddMinutes(aInterval);
                            aList.Add(aBar);
                            i += aList2.Count();
                        }
                        else
                            break;
                    }
                    aList.Reverse();
                }
                else
                    aList.AddRange(aPrimitives);
            }
            else if (aCtx.Request.Selection.Periodicity == Periodicity.Day)
                return aCtx.Bars;
            else
                return aCtx.Bars;

            return aList;
        }

        private void GetHistoryRequest(HistoryRequest aRequest)
        {
            Validate(aRequest);

            IDataFeed aDataFeed = GetDataFeedByName(aRequest.Selection.Symbol.DataFeed);

            if (aDataFeed != null)
            {
                GetHistoryCtx aCtx = GetHistoryCtx(aRequest.Selection, aDataFeed.TimeZoneInfo);
                aCtx.Request = aRequest;
                aCtx.DataFeed = aDataFeed;

                ThreadPool.QueueUserWorkItem(o =>
                    {
                        aDataFeed.GetHistory(aCtx, Get1MinHistoryCallback);
                    });

            }
        }

        private GetHistoryCtx GetHistoryCtx(HistoryParameters aParameters, TimeZoneInfo datafeedTimeZone)
        {
            int nBars = aParameters.BarsCount;
            DateTime aFromF;
            DateTime aToF;
            int maxBars = nBars;

            if (aParameters.Interval != 1)
                nBars = aParameters.Interval * nBars;
            if ((aParameters.From == DateTime.MinValue || aParameters.From == DateTime.MaxValue) && (aParameters.To == DateTime.MinValue || aParameters.To == DateTime.MaxValue))
            {
                TimeZoneInfo aTZI = datafeedTimeZone;
                DateTime aUTCNow = DateTime.UtcNow;

                aToF = TimeZoneInfo.ConvertTimeFromUtc(aUTCNow, aTZI);
                switch (aParameters.Periodicity)
                {
                    case Periodicity.Second:
                        aFromF = aToF.Date.AddMinutes(-nBars);
                        //not supported
                        break;
                    case Periodicity.Minute:
                        //need check
                        if (aParameters.Symbol.Type == Instrument.Forex)
                        {
                            double nDays = 3.0 * (1 + nBars / (24.0 * 60.0) * (7.0 / 5.0));

                            aFromF = aToF.Date.AddDays(-(int)nDays);
                        }
                        else
                            aFromF = aToF.Date.AddMinutes(-6 * nBars - 5000);
                        break;
                    case Periodicity.Hour:
                        //need check
                        if (aParameters.Symbol.Type == Instrument.Forex)
                        {
                            double nDays = 3.0 * (1 + nBars / (24.0) * (7.0 / 5.0));

                            aFromF = aToF.Date.AddDays(-(int)nDays);
                        }
                        else
                            aFromF = aToF.Date.AddMinutes(-6 * nBars * 24 - 5000);
                        break;
                    case Periodicity.Day:
                        aFromF = aToF.Date.AddDays(-(int)nBars * 7 / 5);
                        break;
                    case Periodicity.Week:
                        aFromF = aToF.Date.AddDays(-(int)7 * nBars * 7 / 5);
                        break;
                    case Periodicity.Month:
                        aFromF = aToF.Date.AddMonths(-nBars);
                        maxBars = nBars;
                        break;
                    case Periodicity.Year:
                        aToF = new DateTime(aToF.Year, 1, 1);
                        aFromF = aToF.Date.AddYears(-nBars);
                        nBars *= 12;
                        maxBars = nBars;
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false);
                        aFromF = aToF.Date.AddMinutes(-nBars);
                        break;
                }

                aParameters.To = TimeZoneInfo.ConvertTimeToUtc(aToF, aTZI);
                aParameters.From = TimeZoneInfo.ConvertTimeToUtc(aFromF, aTZI);
            }
            else if (aParameters.From == DateTime.MinValue || aParameters.From == DateTime.MaxValue || aParameters.To == DateTime.MinValue || aParameters.To == DateTime.MaxValue)
                throw new ApplicationException("Hitory period is specified incorrectly");
            else
            {
                TimeZoneInfo aTZI = datafeedTimeZone;
                DateTime aUTCNow = DateTime.UtcNow;

                aToF = TimeZoneInfo.ConvertTimeFromUtc(aParameters.To, aTZI);
                aFromF = TimeZoneInfo.ConvertTimeFromUtc(aParameters.From, aTZI).Date;
                aParameters.From = TimeZoneInfo.ConvertTimeToUtc(aFromF, aTZI);
            }

            GetHistoryCtx aCtx = new GetHistoryCtx();
            aCtx.MaxBars = maxBars;
            //aCtx.Request = aRequest;
            aCtx.Bars = new List<Bar>();
            aCtx.FromF = aFromF;
            aCtx.ToF = aToF;
            //aCtx.DataFeed = aDataFeed;
            aCtx.Interval = aParameters.Interval;

            return aCtx;
        }


        private void Get1MinHistoryCallback(GetHistoryCtx aCtx, List<Bar> aBars)
        {
            HistoryResponse aResponse = new HistoryResponse();

            var tZone = aCtx.DataFeed.TimeZoneInfo;
            /*foreach (var b in aBars)
            {
                b.Date = TimeZoneInfo.ConvertTimeToUtc(b.Date, tZone);
            }*/

            aResponse.Bars = aBars;
            aResponse.ID = aCtx.Request.Selection.Id;
            aResponse.User = aCtx.Request.User;
            if (aCtx.Request.Selection.GenerateChartImage)
                aResponse.ChartImageData = OHLCPainter.GenerateChartAsJsonString(aBars);
            PushResponse(aResponse);
        }

        private void GetDayHistoryCallback(GetHistoryCtx aCtx, List<Bar> aBars)
        {
            try
            {
                List<Bar> aList2 = new List<Bar>();

                aBars.ForEach(o =>
                {
                    WriteToLog_Info(String.Format("DateTime: {0}", o.Date.ToString("yy-MM-dd")), null);
                    o.Date = TimeZoneInfo.ConvertTimeToUtc(o.Date, aCtx.DataFeed.TimeZoneInfo);
                });
                aBars = aBars.OrderBy(o => o.Date).ToList();
                // response contains bars for a previous day
                if (aBars.Count > 0)
                {
                    DateTime aUTCFrom;
                    DateTime aUTCTo;

                    aUTCFrom = aBars[0].Date;
                    aUTCTo = TimeZoneInfo.ConvertTimeToUtc(aCtx.ToF);
                }
                aCtx.Bars.AddRange(aList2.OrderByDescending(o => o.Date));

                // send response
                HistoryResponse aResponse = new HistoryResponse();

                aResponse.Bars = CalculateBars(aCtx, aCtx.Bars);
                aResponse.ID = aCtx.Request.Selection.Id;
                aResponse.User = aCtx.Request.User;
                PushResponse(aResponse);
            }
            catch (Exception e)
            {
                WriteToLog("GetDayHistoryCallback", e);
                try
                {
                    aCtx.Request.User.SendError(new ApplicationException(String.Format("Historical request (ID='{0}')", aCtx.Request.Selection.Id)));
                }
                catch
                {
                }
            }
        }

        private void HeartBeatRequest(HeartbeatRequest msg)
        {

        }

        private void GetDataFeedList(DataFeedListRequest aRequest)
        {
            DataFeedListResponse aResponse = new DataFeedListResponse();

            aResponse.DataFeeds = new List<DataFeed>();
            foreach (KeyValuePair<string, IDataFeed> item_pair in m_DataFeedsByName)
            {
                aResponse.DataFeeds.Add(new DataFeed
                {
                    Name = item_pair.Value.Name,
                    Exchanges = item_pair.Value.Markets
                });
            }
            aResponse.User = aRequest.User;
            PushResponse(aResponse);
        }

        #endregion

        #region Helper Methods
        private IDataFeed GetDataFeedByName(string aName)
        {
            IDataFeed aDataFeed;

            if (string.IsNullOrEmpty(aName))
                return null;

            m_DataFeedsByName.TryGetValue(aName, out aDataFeed);

            return aDataFeed;
        }
    
        #endregion

        #region Alert methods

        private void SubscribeAlert(AlertSubscribeRequest aRequest)
        {
            var errorString = TradeScript.Validate(aRequest.Script);
            if (aRequest.Script == string.Empty)
                errorString = "Script can not be empty";
            
            if (errorString != string.Empty)
            {
                var response = new AlertSubscribeResponse
                {
                    Id = aRequest.Id,
                    Error = errorString,
                    AlertName = aRequest.AlertName,
                    Alert = new Alert
                    {
                        Name = aRequest.Name,
                        Symbol = aRequest.Symbol
                    },
                    User = aRequest.User
                };
                PushResponses(aRequest.User.ID, new ResponseMessage[] { response });
                return;
            }

            var selection = new HistoryParameters(aRequest.Id, aRequest.Symbol, aRequest.Periodicity, aRequest.Interval, aRequest.BarsCount);
            IDataFeed aDataFeed = GetDataFeedByName(aRequest.Symbol.DataFeed);

            if (aDataFeed != null)
            {
                GetHistoryCtx aCtx = GetHistoryCtx(selection, aDataFeed.TimeZoneInfo);
                aCtx.Request = new HistoryRequest(selection);
                aCtx.DataFeed = aDataFeed;
                ThreadPool.QueueUserWorkItem(o =>
                {
                    aDataFeed.GetHistory(aCtx, (ctx, bars) =>
                    {
                        Level1Subscribers subscribers = null;
                        var symbolItem = new SymbolItem()
                        {
                            DataFeed = aRequest.Symbol.DataFeed,
                            Exchange = aRequest.Symbol.Exchange,
                            Symbol = aRequest.Symbol.Symbol,
                            Type = aRequest.Symbol.Type
                        };
                        lock (m_Level1SubscribersBySymbols)
                        {
                            if (!m_Level1SubscribersBySymbols.TryGetValue(aRequest.Symbol, out subscribers))
                            {
                                subscribers = new Level1Subscribers();
                                subscribers.Subscribers = new List<string>();
                                subscribers.AlertSubscribers = new List<AlertSubscription>();
                                m_Level1SubscribersBySymbols.Add(aRequest.Symbol, subscribers);
                            }
                        }
                        AlertSubscription alert = new AlertSubscription()
                        {
                            Id = aRequest.Id,
                            AlertName = aRequest.AlertName,
                            Symbol = symbolItem,
                            Name = aRequest.Name,
                            Periodicity = aRequest.Periodicity,
                            Interval = aRequest.Interval,
                            Script = aRequest.Script,
                            UserSessionId = aRequest.User.ID,
                            Login = aRequest.User.Login,
                            CalculationType = aRequest.CalculationType
                        };
                        alert.InitAlert(bars);
                        lock (subscribers.AlertSubscribers)
                        {
                            subscribers.AlertSubscribers.Add(alert);
                        }
                        aDataFeed.Subscribe(aRequest.Symbol);
                    });
                });
            }
        }

        private void UnsubscribeAlert(AlertUnsubscribeRequest aRequest)
        {
            Level1Subscribers aLevel1Subscribers;
            bool bUnsubscribe = false;

            lock (m_Level1SubscribersBySymbols)
            {
                if (!m_Level1SubscribersBySymbols.TryGetValue(aRequest.Symbol, out aLevel1Subscribers))
                    return;
            }
            lock (aLevel1Subscribers)
            {
                for (int i = 0; i < aLevel1Subscribers.AlertSubscribers.Count; i++)
                {
                    if (aLevel1Subscribers.AlertSubscribers[i].Id == aRequest.Id)
                    {
                        aLevel1Subscribers.AlertSubscribers.RemoveAt(i);
                        bUnsubscribe = aLevel1Subscribers.Subscribers.Count == 0 && aLevel1Subscribers.AlertSubscribers.Count == 0;
                        break;
                    }
                }
            }
            if (bUnsubscribe)
            {
                IDataFeed aDataFeed = GetDataFeedByName(aRequest.Symbol.DataFeed);

                if (aDataFeed != null)
                    aDataFeed.UnSubscribe(aRequest.Symbol);
            }
        }

        private void SendAlertsHistory(AlertsHistoryRequest aRequest)
        {
            Level1Subscribers aLevel1Subscribers;

            lock (m_Level1SubscribersBySymbols)
            {
                if (!m_Level1SubscribersBySymbols.TryGetValue(aRequest.Symbol, out aLevel1Subscribers))
                    return;
            }
            lock (aLevel1Subscribers)
            {
                for (int i = 0; i < aLevel1Subscribers.AlertSubscribers.Count; i++)
                {
                    if (aLevel1Subscribers.AlertSubscribers[i].Id == aRequest.Id)
                    {
                        
                        AlertsHistoryResponse response = new AlertsHistoryResponse
                        {
                            Id = aRequest.Id,
                            Alerts = aLevel1Subscribers.AlertSubscribers[i].FiredAlertsHistory,
                            User = DataServer.MessageRouter.gMessageRouter.GetUserInfoByLogin(aLevel1Subscribers.AlertSubscribers[i].Login)
                        };
                        PushResponses(response.User.ID, new ResponseMessage[] { response });
                        break;
                    }
                }
            }
        }

        private void CheckBarSubscribers(Tick aTick, Level1Subscribers subscribers)
        {
            List<AlertSubscription> alertSubscriptions;
            lock (subscribers)
            {
                alertSubscriptions = new List<AlertSubscription>(subscribers.AlertSubscribers);
            }

            for (int i = 0; i < alertSubscriptions.Count; i++)
            {
                bool fire = alertSubscriptions[i].AppendTick(aTick);
                if (fire)
                {
                    AlertSubscribeResponse response = new AlertSubscribeResponse
                    {
                        Id = alertSubscriptions[i].Id,
                        AlertName = alertSubscriptions[i].AlertName,
                        Alert = new Alert
                        {
                            Name = alertSubscriptions[i].Name,
                            Symbol = alertSubscriptions[i].Symbol,
                            Timestamp = aTick.Date,
                        },
                        User = DataServer.MessageRouter.gMessageRouter.GetUserInfoByLogin(alertSubscriptions[i].Login)
                    };
                    alertSubscriptions[i].AddFiredAlertToHistory(response.Alert);
                    if (response.User != null)
                    {
                        PushResponses(response.User.ID, new ResponseMessage[] { response });
                    }
                    else
                    {
                        //send remote notification
                        DataServer.MessageRouter.gMessageRouter.PushRemoteNotification(alertSubscriptions[i].Login, alertSubscriptions[i].Id, alertSubscriptions[i].AlertName, response.Alert);
                    }

                }
            }
        }

        public void RunBacktest(BacktestGetRequest aRequest)
        {
            var errorString = TradeScript.ValidateBuySell(aRequest.BuyScript, aRequest.SellScript);
            if (aRequest.BuyScript == string.Empty || aRequest.SellScript == string.Empty)
                errorString = "Buy and sell script can not be empty";

            if (errorString != string.Empty)
            {
                var response = new BacktestGetResponse
                {
                    Id = aRequest.Id,
                    Error = errorString,
                    User = aRequest.User
                };
                PushResponses(aRequest.User.ID, new ResponseMessage[] { response });
                return;
            }

            var selection = new HistoryParameters(aRequest.Id, aRequest.Symbol, aRequest.Periodicity, aRequest.Interval, aRequest.BarsCount);
            IDataFeed aDataFeed = GetDataFeedByName(aRequest.Symbol.DataFeed);

            if (aDataFeed != null)
            {
                GetHistoryCtx aCtx = GetHistoryCtx(selection, aDataFeed.TimeZoneInfo);
                aCtx.Request = new HistoryRequest(selection);
                aCtx.DataFeed = aDataFeed;
                ThreadPool.QueueUserWorkItem(o =>
                {
                    aDataFeed.GetHistory(aCtx, (ctx, bars) =>
                    {
                        string backtestOutput = TradeScript.Backtest(aRequest.BuyScript, aRequest.SellScript, bars);
                        var response = new BacktestGetResponse
                        {
                            Id = aRequest.Id,
                            User = aRequest.User,
                            Symbol = aRequest.Symbol,
                            Information = TradeScript.ExtractBacktestInformation(backtestOutput),
                            Trades = TradeScript.ExtractBacktestTrades(backtestOutput)
                        };
                        PushResponses(aRequest.User.ID, new ResponseMessage[] { response });
                    });
                });
            }
        }

        private void SubscribeLevel2(L2SubscribeRequest aRequest)
        {
            Level2Subscribers aLevel2Subscribers;
            lock (m_Level2SubscribersBySymbols)
            {
                if (!m_Level2SubscribersBySymbols.TryGetValue(aRequest.Symbol, out aLevel2Subscribers))
                {
                    aLevel2Subscribers = new Level2Subscribers();
                    aLevel2Subscribers.Subscribers = new List<string>();
                    m_Level2SubscribersBySymbols.Add(aRequest.Symbol, aLevel2Subscribers);
                }
            }
            lock (aLevel2Subscribers)
            {
                if (!aLevel2Subscribers.Subscribers.Contains(aRequest.User.ID))
                    aLevel2Subscribers.Subscribers.Add(aRequest.User.ID);
            }
            IDataFeed aDataFeed = GetDataFeedByName(aRequest.Symbol.DataFeed);

            if (aDataFeed != null)
                aDataFeed.SubscribeLevel2(aRequest.Symbol);
        }

        private void UnsubscribeLevel2(L2UnsubscribeRequest aRequest)
        {
            UnsubscribeLevel2SymbolsBySessionID(aRequest.User.ID, new SymbolItem[] { aRequest.Symbol });
        }

        #endregion

       


    }

}
