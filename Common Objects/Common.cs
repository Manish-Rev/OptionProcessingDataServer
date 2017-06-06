using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Diagnostics;
using Newtonsoft.Json;

namespace CommonObjects
{
    using System.Timers;

    #region Delegates
    
    public delegate void HistoryAnswerHandler(GetHistoryCtx aCtx, List<Bar> aBars);
    public delegate void NewQuoteHandler(string dataFeed, TimeZoneInfo aTZI, Tick tick);
    public delegate void NewLevel2DataHandler(string dataFeed, TimeZoneInfo aTZI, Level2Data level2);
    public delegate void ReceivedMessageHandler(RequestMessage message);
    public delegate LoginResponse LoginHandler(LoginRequest message);
    public delegate void LogoutHandler();

    #endregion

    #region IDataFeed Classes

    /// <summary>
    /// defines methods and properties common for all sessions managed by DataServer
    /// </summary>
    public interface IUserInfo
    {
        /// <summary>
        /// user name
        /// </summary>
        string Login { get; set; }
        /// <summary>
        /// session ID, must be unique
        /// </summary>
        string ID { get; set; }
        /// <summary>
        /// session object, generally session context
        /// </summary>
        object SessionObject { get; set; }

        /// <summary>
        /// send response message to client
        /// </summary>
        /// <param name="aResponse">response message</param>
        void Send(ResponseMessage aResponse);
        /// <summary>
        /// disconnect session
        /// </summary>
        void Disconnect();
        /// <summary>
        /// send heartbeat
        /// </summary>
        void Heartbeat();
        /// <summary>
        /// sends error info
        /// </summary>
        /// <param name="e">exception object</param>
        void SendError(Exception e);
    }

    /// <summary>
    /// device info
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// device token
        /// </summary>
        public string DeviceToken;

        /// <summary>
        /// device type (apple, android...)
        /// </summary>
        public string DeviceType;

        /// <summary>
        /// minimal interval between two notifications with same alert's id
        /// </summary>
        private const double MinNotificationPeriodisity = 30000;

        /// <summary>
        /// timer for implement minimal interval between two notifications with same alert's id
        /// </summary>
        private Timer notificationTimer;

        /// <summary>
        /// list of alert's ids that have been already notified in curent time interval
        /// </summary>
        public List<string> AlreadyNotifiedAlertsIds;

        /// <summary>
        /// constructor
        /// </summary>
        public DeviceInfo()
        {
            AlreadyNotifiedAlertsIds = new List<string>();
            notificationTimer = new Timer(MinNotificationPeriodisity);
            notificationTimer.AutoReset = true;
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="aDeviceToken">device token</param>
        /// <param name="aDeviceType">device type (pple, android...)</param>
        public DeviceInfo(string aDeviceToken, string aDeviceType) : this()
        {
            this.DeviceToken = aDeviceToken;
            this.DeviceType = aDeviceType;
        }

        public void StartNotificationTimer()
        {
            AlreadyNotifiedAlertsIds = new List<string>();
            notificationTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            notificationTimer.Enabled = true;
        }

        public void StopNotificationTimer()
        {
            notificationTimer.Enabled = false;
            notificationTimer.Elapsed -= OnTimedEvent;
        }

        public bool IsNotificationTimerStarted ()
        {
            return notificationTimer.Enabled;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            AlreadyNotifiedAlertsIds = new List<string>();
        }
    }

    /// <summary>
    /// exchange info
    /// </summary>
    [DataContract]
    public class ExchangeInfo
    {
        /// <summary>
        /// exchange name
        /// </summary>
        [DataMember] 
        public string Name;

        /// <summary>
        /// list of symbols
        /// </summary>
        [DataMember] 
        public List<SymbolItem> Symbols;
    }

    /// <summary>
    /// implements functionality to store and compare items
    /// </summary>
    [DataContract]
    public class SymbolItem
    {
        /// <summary>
        /// data feeder name
        /// </summary>
        [DataMember]
        public string DataFeed;
        /// <summary>
        /// exchange name
        /// </summary>
        [DataMember]
        public string Exchange;
        /// <summary>
        /// symbol name
        /// </summary>
        [DataMember] 
        public string Symbol;
        /// <summary>
        /// company name
        /// </summary>
        [DataMember]
        public string Company;

        /// <summary>
        /// instrument type
        /// </summary>
        [DataMember]
        public Instrument Type;

        /// <summary>
        /// equalizes objects
        /// </summary>
        /// <param name="obj">object to compare</param>
        /// <returns>true, if two objects are equal</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            SymbolItem o = obj as SymbolItem;

            if ((object)o == null)
                return false;
            
            return DataFeed == o.DataFeed && Symbol == o.Symbol && Type == o.Type;
        }

        /// <summary>
        /// compares two objects
        /// </summary>
        /// <param name="a">left object</param>
        /// <param name="b">right object</param>
        /// <returns>true, if equal</returns>
        public static bool operator ==(SymbolItem a, SymbolItem b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// compares two objects
        /// </summary>
        /// <param name="a">left object</param>
        /// <param name="b">right object</param>
        /// <returns>true, if non equal</returns>
        public static bool operator != (SymbolItem a, SymbolItem b)
        {
            return !(a == b);
        }

        /// <summary>
        /// calculates hash code
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return String.Format("{0}:{1}:{2}"
                , (DataFeed == null) ? String.Empty : DataFeed
                , (Symbol == null) ? String.Empty : Symbol
                , this.Type).GetHashCode();
        }

    }

    /// <summary>
    /// request for historical data
    /// </summary>
    [DataContract]
    public class HistoryParameters
    {
        /// <summary>
        /// request id
        /// </summary>
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// symbol name
        /// </summary>
        [DataMember]
        public SymbolItem Symbol { get; set; }
        /// <summary>
        /// periodicity: second, minute, hour, day, week
        /// </summary>
        [DataMember]
        public Periodicity Periodicity { get; set; }
        /// <summary>
        /// number of units specified by Periodicity
        /// </summary>
        [DataMember]
        public int Interval { get; set; }
        /// <summary>
        /// bar's count
        /// </summary>
        [DataMember]
        public int BarsCount { get; set; }
        /// <summary>
        /// min timestamp for requested bars, DateTime.MinValue is available
        /// </summary>
        [DataMember]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime From;
        /// <summary>
        /// max timestamp for requested bars, DateTime.MaxValue is available
        /// </summary>
        [DataMember]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime To;
        /// <summary>
        /// instrument type, default value is Instrument.Unknow
        /// </summary>

        /// <summary>
        /// if true draw line chart from requested bars and serialize it to JSON string 
        /// </summary>
        [DataMember]
        public Boolean GenerateChartImage { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public HistoryParameters()
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="requestId">request id</param>
        /// <param name="symbol">symbol name</param>
        /// <param name="periodicity">periodicity, see Periodicity property</param>
        /// <param name="interval">interval, see Interval property</param>
        /// <param name="bars">bar's count, see BarsCount property</param>
        public HistoryParameters(string requestId, SymbolItem symbol, Periodicity periodicity, int interval, int bars)
        {
            Id = requestId;
            Symbol = symbol;
            Periodicity = periodicity;
            Interval = interval;
            BarsCount = bars;
        }
    }

    public class UnixDateTimeConverter : Newtonsoft.Json.Converters.DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long val;
            if (value is DateTime)
            {
                var timeSpan = (((DateTime)value) - new DateTime(1970, 1, 1, 0, 0, 0));
                val = (long)timeSpan.TotalMilliseconds;
            }
            else
            {
                throw new Exception("Expected date object value.");
            }
            writer.WriteValue(val);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                        JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
                throw new Exception("Wrong Token Type");

            long ticks = (long)reader.Value;
            if (ticks > 0)
            {
                DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return date.AddMilliseconds(ticks);
            }
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// represents bar info
    /// </summary>
    [DataContract]
    public class Bar
    {
        /// <summary>
        /// timestamp
        /// </summary>
        [DataMember]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Date { get; set; }
        /// <summary>
        /// open price
        /// </summary>
        [DataMember]
        public double Open { get; set; }
        /// <summary>
        /// high price
        /// </summary>
        [DataMember]
        public double High { get; set; }
        /// <summary>
        /// low price
        /// </summary>
        [DataMember]
        public double Low { get; set; }
        /// <summary>
        /// close price
        /// </summary>
        [DataMember]
        public double Close { get; set; }
        /// <summary>
        /// volume
        /// </summary>
        [DataMember]
        public double Volume { get; set; }

        public override string ToString()
        {
            return Date.ToString();
        }
    }
      
    /// <summary>
    /// represents exchange quote info
    /// </summary>
    [DataContract]
    public class Tick
    {
        /// <summary>
        /// symbol name
        /// </summary>
        [DataMember]
        public SymbolItem Symbol { get; set; }
        /// <summary>
        /// timestamp
        /// </summary>
        [DataMember]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Date { get; set; }
        /// <summary>
        /// price
        /// </summary>
        [DataMember]
        public double Price { get; set; }
        /// <summary>
        /// volume
        /// </summary>
        [DataMember]
        public double Volume { get; set; }
        /// <summary>
        /// bid price
        /// </summary>
        [DataMember]
        public double Bid { get; set; }
        /// <summary>
        /// bid size
        /// </summary>
        [DataMember]
        public double BidSize { get; set; }
        /// <summary>
        /// ask price
        /// </summary>
        [DataMember]
        public double Ask { get; set; }
        /// <summary>
        /// ask size
        /// </summary>
        [DataMember]
        public double AskSize { get; set; }
    }

    /// <summary>
    /// represents data feeder info
    /// </summary>
    [DataContract]
    public class DataFeed
    {
        /// <summary>
        /// feeder name
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// supported exchanges
        /// </summary>
        [DataMember]
        public List<ExchangeInfo> Exchanges { get; set; }
    }

        /// <summary>
    /// data server exception
    /// </summary>
    [DataContract]
    public class DataServerException
    {
        /// <summary>
        /// error message
        /// </summary>
        [DataMember]
        public string Reason { get; set; }

        public DataServerException(string aMessage)
        {
            Reason = aMessage;
        }
    }


    [DataContract]
    public class Level2Data
    {
        [DataMember]
        public SymbolItem Symbol { get; set; }

        [DataMember]
        public IList<Level2Item> Bids { get; set; }

        [DataMember]
        public IList<Level2Item> Asks { get; set; }
    }

    [DataContract]
    public class Level2Item
    {
        [DataMember]
        public string MarketMaker { get; set; }

        [DataMember]
        public double Price { get; set; }

        [DataMember]
        public int Quantity { get; set; }
    }
       
    
    [DataContract]
    public class Alert
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public SymbolItem Symbol { get; set; }

        [DataMember]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Timestamp { get; set; }
    }

    [DataContract]
    public class BacktestInformationItem
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Text { get; set; }

        public BacktestInformationItem()
        {
            Name = string.Empty;
            Text = string.Empty;
        }

        public BacktestInformationItem(string name, string value)
        {
            Name = name;
            Text = value;
        }
    }

    [DataContract]
    public class BacktestTradeItem
    {
        [DataMember]
        public string DateString { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public double Price { get; set; }

        public BacktestTradeItem()
        {
            DateString = string.Empty;
            Type = string.Empty;
        }

        public BacktestTradeItem(string dateString, string type, double price)
        {
            DateString = dateString;
            Type = type;
            Price = price;
        }
    }

    public class GetHistoryCtx
    {
        /// <summary>
        /// original request
        /// </summary>
        public HistoryRequest Request;
        /// <summary>
        /// bar's history
        /// </summary>
        public List<Bar> Bars;
        /// <summary>
        /// begin of period, time zone is feeder's time zone
        /// </summary>
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime FromF;
        /// <summary>
        /// end of time, time zone is feeder's time zone
        /// </summary>
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime ToF;
        /// <summary>
        /// target feed
        /// </summary>
        public IDataFeed DataFeed;
        /// <summary>
        /// max bars
        /// </summary>
        public int MaxBars;
        /// <summary>
        /// original request's interval
        /// </summary>
        public int Interval;
    }

    #endregion

    #region Request Messages

    /// <summary>
    /// base class for a request message
    /// </summary>
    [DataContract]
    [KnownType(typeof(DataFeedListRequest))]
    [KnownType(typeof(HeartbeatRequest))]
    [KnownType(typeof(SubscribeRequest))]
    [KnownType(typeof(UnsubscribeRequest))]
    [KnownType(typeof(L2SubscribeRequest))]
    [KnownType(typeof(L2UnsubscribeRequest))]
    [KnownType(typeof(HistoryRequest))]
    [KnownType(typeof(AlertSubscribeRequest))]
    [KnownType(typeof(AlertUnsubscribeRequest))]
    [KnownType(typeof(BacktestGetRequest))]
    public class RequestMessage
    {
        /// <summary>
        /// identifies user/session info where the request received
        /// </summary>
        [IgnoreDataMember]
        public IUserInfo User { get; set; }

        [JsonProperty]
        public string MsgType { get; set; }

        public RequestMessage()
        {
            this.MsgType = this.GetType().Name;
        }
    }
    
    /// <summary>
    /// login request
    /// </summary>
    [DataContract]
    public class LoginRequest : RequestMessage
    {
        /// <summary>
        /// login, generally user name
        /// </summary>
        [DataMember]
        public string Login;
        /// <summary>
        /// password
        /// </summary>
        [DataMember]
        public string Password;


        public LoginRequest(string aLogin, string aPassword)
        {
            Login = aLogin;
            Password = aPassword;
        }
    }

    /// <summary>
    /// request for subscribe on remote notification
    /// </summary>
    [DataContract]
    public class RemoteNotificationRequest : RequestMessage
    {
        /// <summary>
        /// device token
        /// </summary>
        [DataMember]
        public string DeviceToken;
        /// <summary>
        /// device type (apple, android...)
        /// </summary>
        [DataMember]
        public string DeviceType;


        public RemoteNotificationRequest(string aDeviceToken, string aDeviceType)
        {
            DeviceToken = aDeviceToken;
            DeviceType = aDeviceType;
        }
    }

    /// <summary>
    /// request for supported data feeders
    /// </summary>
    [DataContract]
    public class DataFeedListRequest : RequestMessage
    {

    }

    /// <summary>
    /// heartbeat, generally used to detect sessions's state
    /// </summary>
    [DataContract]
    public class HeartbeatRequest : RequestMessage
    {
        [DataMember]
        public string Text { get; set; }

        public HeartbeatRequest(string aText)
        {
            this.Text = aText;
        }
    }

    /// <summary>
    /// subscribe symbol request
    /// </summary>
    [DataContract]
    public class SubscribeRequest : RequestMessage
    {
        /// <summary>
        /// symbol info
        /// </summary>
        [DataMember]
        public SymbolItem Symbol { get; set; }

        public SubscribeRequest(SymbolItem aSymbol)
        {
            Symbol = aSymbol;
        }
    }

    /// <summary>
    /// unsubscribe symbol request
    /// </summary>
    [DataContract]
    public class UnsubscribeRequest : RequestMessage
    {
        /// <summary>
        /// symbol info
        /// </summary>
        [DataMember]
        public SymbolItem Symbol { get; set; }

        public UnsubscribeRequest(SymbolItem aSymbol)
        {
            Symbol = aSymbol;
        }
    }

    [DataContract]
    public class L2SubscribeRequest : RequestMessage
    {
        [DataMember]
        public SymbolItem Symbol { get; set; }
    }

    [DataContract]
    public class L2UnsubscribeRequest : RequestMessage
    {
        [DataMember]
        public SymbolItem Symbol { get; set; }
    }

    /// <summary>
    /// historical bars request
    /// </summary>
    [DataContract]
    public class HistoryRequest : RequestMessage
    {
        /// <summary>
        /// request parameters
        /// </summary>
        [DataMember]
        public HistoryParameters Selection { get; set; }

        public HistoryRequest(HistoryParameters aSelection)
        {
            Selection = aSelection;
        }
    }

    [DataContract]
    public class AlertSubscribeRequest : RequestMessage
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string AlertName { get; set; }

        [DataMember]
        public string Name { get; set; } //scrip type (buy or sell)

        [DataMember]
        public string Script { get; set; }

        [DataMember]
        public SymbolItem Symbol { get; set; }

        [DataMember]
        public Periodicity Periodicity { get; set; }

        [DataMember]
        public int Interval { get; set; }

        [DataMember]
        public int BarsCount { get; set; }

        [DataMember]
        public AlertCalculateType CalculationType { get; set; }
    
    }
    
    [DataContract]
    public class AlertUnsubscribeRequest : RequestMessage
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public SymbolItem Symbol { get; set; }
    }

    [DataContract]
    public class AlertsHistoryRequest : RequestMessage
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public SymbolItem Symbol { get; set; }
    }

    [DataContract]
    public class BacktestGetRequest : RequestMessage
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string BuyScript { get; set; }

        [DataMember]
        public string SellScript { get; set; }

        [DataMember]
        public SymbolItem Symbol { get; set; }

        [DataMember]
        public Periodicity Periodicity { get; set; }

        [DataMember]
        public int Interval { get; set; }

        [DataMember]
        public int BarsCount { get; set; }
    }

    #endregion

    #region Response Messages
    
    /// <summary>
    /// base class for a response message
    /// </summary>
    [DataContract]
    [KnownType(typeof(DataFeedListResponse))]
    [KnownType(typeof(HeartbeatResponse))]
    [KnownType(typeof(HistoryResponse))]
    [KnownType(typeof(NewTickResponse))]
    [KnownType(typeof(ErrorInfo))]
    [KnownType(typeof(AlertSubscribeResponse))]
    [KnownType(typeof(BacktestGetResponse))]
    [KnownType(typeof(L2SubscribeResponse))]
    public class ResponseMessage
    {
        /// <summary>
        /// user/session info to send response
        /// </summary>
        public IUserInfo User { get; set; }
                
        [JsonProperty]
        public string MsgType { get; set; }

        public ResponseMessage()
        {
            this.MsgType = this.GetType().Name;
        }
    }
    
    /// <summary>
    /// confirms successfull logon
    /// </summary>
    [DataContract]
    public class LoginResponse : ResponseMessage
    {
        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string Error { get; set; }
    }

    /// <summary>
    /// confirms logout successfull
    /// </summary>
    [DataContract]
    public class LogoutResponse : ResponseMessage
    {
        /// <summary>
        /// login
        /// </summary>
        public string Login { get; set; }
    }

    /// <summary>
    /// represents list of supported data feeds
    /// </summary>
    [DataContract]
    public class DataFeedListResponse : ResponseMessage
    {
        /// <summary>
        /// list of data feeds
        /// </summary>
        [DataMember]
        public List<DataFeed> DataFeeds { get; set; }
    }

    /// <summary>
    /// heartbeat, generally used to detect sessions's state
    /// </summary>
    [DataContract]
    public class HeartbeatResponse : ResponseMessage
    {
        [DataMember]
        public string Text { get; set; }

        public HeartbeatResponse(string aText)
        {
            this.Text = aText;
        }
    }

    /// <summary>
    /// represents historical bar's
    /// </summary>
    [DataContract]
    public class HistoryResponse : ResponseMessage
    {
        /// <summary>
        /// ID, that was specified in historical request
        /// </summary>
        [DataMember]
        public string ID { get; set; }

        /// <summary>
        /// list of bars
        /// </summary>
        [DataMember]
        public List<Bar> Bars { get; set; }

        /// <summary>
        /// true, if the response is last in chain
        /// </summary>
        [DataMember]
        public bool Tail { get; set; }

        [DataMember]
        public String ChartImageData;
    }

    /// <summary>
    /// exchange quotes
    /// </summary>
    [DataContract]
    public class NewTickResponse : ResponseMessage
    {
        /// <summary>
        /// list of exchange quotes in order that these received from feeder
        /// </summary>
        [DataMember]
        public List<Tick> Tick { get; set; }
    }

    [DataContract]
    public class ErrorInfo : ResponseMessage
    {
        public ErrorInfo(Exception e)
        {
            this.Error = e.Message;
        }

        public ErrorInfo(string errorMessage)
        {
            this.Error = errorMessage;
        }

        [DataMember]
        public string Error { get; set; }
    }


    [DataContract]
    public class AlertSubscribeResponse : ResponseMessage
    {

        [DataMember]
        public string AlertName { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public Alert Alert { get; set; }

    }

    [DataContract]
    public class AlertsHistoryResponse : ResponseMessage
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public List<Alert> Alerts { get; set; }

    }

    [DataContract]
    public class BacktestGetResponse : ResponseMessage
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public SymbolItem Symbol { get; set; }

        [DataMember]
        public IList<BacktestInformationItem> Information { get; set; }

        [DataMember]
        public IList<BacktestTradeItem> Trades { get; set; }

        [DataMember]
        public string Error { get; set; }
    }

    [DataContract]
    public class L2SubscribeResponse : ResponseMessage
    {
        [DataMember]
        public Level2Data Level2 { get; set; }
    }

    #endregion

    #region Enums
    
    /// <summary>
    /// represents available periodicities in historical bars request
    /// </summary>
    public enum Periodicity
    {
        Second = 0,
        Minute,//1
        Hour,//2
        Day,//3
        Week,//4
        Month,//5
        Year,//6
        Tick,//7
        Range//8
    }

    public enum AlertCalculateType
    {
        OnTick = 0,
        OnBarClose
    }

    public enum Instrument
    {
        Unknown = 0,
        Equity = 1,
        Option = 2,
        Forex = 3
    }

    #endregion
}
