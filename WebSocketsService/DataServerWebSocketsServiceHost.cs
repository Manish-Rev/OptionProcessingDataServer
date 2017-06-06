using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperWebSocket;
using Newtonsoft.Json;
using CommonObjects;

namespace WebSocketsServiceHost
{
    /// <summary>
    /// starts connection service that uses WebSocket transport
    /// </summary>
    public class DataServerWebSocketsServiceHost : DataServer.Logger, DataServer.IDataServerServiceHost
    {
        // core functionality that implements WebSockets
        WebSocketServer m_ServerCore;

        public DataServerWebSocketsServiceHost()
            : base("SuperWebSockets")
        {
        }

        #region ISetrviceHost implementation
        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public void Start(Dictionary<string, object> args)
        {
            string localip = null;
            int port = 0;

            WriteToLog_Info("Starting..", null);
            if (args.ContainsKey("ip"))
                localip = args["ip"] as string;
            if (localip == null)
                throw new Exception("WebSockets service can not start because 'ip' parameter is not specified");
            if (args.ContainsKey("port"))
                Int32.TryParse(args["port"].ToString(), out port);
            if (port == 0)
                throw new Exception("WebSockets service can not start because 'port' parameter is not specified");

            m_ServerCore = new WebSocketServer();
            m_ServerCore.Setup(localip, port);
            m_ServerCore.NewDataReceived += OnNewDataReceived;
            m_ServerCore.NewMessageReceived += OnNewMessageReceived;
            m_ServerCore.NewSessionConnected += OnNewSessionConnected;
            m_ServerCore.SessionClosed += OnSessionClosed;
            m_ServerCore.Start();
            WriteToLog_Info("Started.", null);
        }

        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public void Stop()
        {
            WriteToLog_Info("Stopping...", null);
            m_ServerCore.Stop();
            WriteToLog_Info("Stopped", null);
        }

        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public Dictionary<string, object> DefaultSettings
        {
            get
            {
                Dictionary<string, object> aDict = new Dictionary<string, object>();

                aDict["ip"] = "127.0.0.1";
                aDict["port"] = "2012";

                return aDict;
            }
        }

        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public string Name
        {
            get { return "SuperWebSockets"; }
        }

        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public CommonObjects.ILogonControl LogonControl
        {
            get
            {
                return new LogonControl();
            }
        }
        #endregion

        /// <summary>
        /// indicates WebSocket session closed
        /// </summary>
        /// <param name="session">WebSocket session object</param>
        /// <param name="value">reason</param>
        void OnSessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
        {
            try
            {
                lock (DataServer.MessageRouter.gMessageRouter)
                {
                    DataServer.MessageRouter.gMessageRouter.RemoveSession(session.SessionID);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// indicates WebSocket session established
        /// </summary>
        /// <param name="aSession">WebSocket session info</param>
        void OnNewSessionConnected(WebSocketSession aSession)
        {
        }

        /// <summary>
        /// indicates a string message received
        /// </summary>
        /// <param name="aSession">WebSocket session info</param>
        /// <param name="aValue">message serialized to string</param>
        void OnNewMessageReceived(WebSocketSession aSession, string aValue)
        {
            try
            {
                // 'test code' is usefull to test client's request.
                #region test code
                /*if (aValue == "L" || aValue == "l")
                {
                    LoginRequest aRequest = new LoginRequest("DemoUser", "demo");

                    aValue = JsonConvert.SerializeObject(aRequest);
                }
                else if (aValue == "O" || aValue == "o")
                {
                    LogoutRequest aRequest = new LogoutRequest();

                    aValue = JsonConvert.SerializeObject(aRequest);
                }
                else if (aValue == "S" || aValue == "s")
                {
                    SubscribeRequest aRequest = new SubscribeRequest(new SymbolItem() { Symbol = "MSFT", DataFeed = "Simulation DataFeed", Exchange = String.Empty, Type = 1 });

                    aValue = JsonConvert.SerializeObject(aRequest);
                }
                else if (aValue == "U" || aValue == "u")
                {
                    UnsubscribeRequest aRequest = new UnsubscribeRequest(new SymbolItem() { Symbol = "MSFT", DataFeed = "Simulation DataFeed", Exchange = String.Empty, Type = 1 });

                    aValue = JsonConvert.SerializeObject(aRequest);
                }
                else if (aValue == "HD" || aValue == "hd")
                {
                    HistoryRequest aRequest = new HistoryRequest()
                    {
                        Selection = new HistoryParameters()
                        {
                            Symbol = new SymbolItem() { Symbol = "MSFT", DataFeed = "DDF", Exchange = String.Empty, Type = 1 },
                            Periodicity = CommonObjects.Periodicity.Day,
                            Interval = 1,
                            Id = Guid.NewGuid().ToString("N"),
                            BarsCount = 100,
                            From = new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc),
                            To = new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc)
                        }
                    };
                    aValue = JsonConvert.SerializeObject(aRequest);
                }
                //else if (aValue == "HM" || aValue == "hm")
                //{
                //    HistoryRequest aRequest = new HistoryRequest()
                //    {
                //        Selection = new HistoryParameters()
                //        {
                //            Symbol = new SymbolItem() { Symbol = "MSFT", DataFeed = "DDF", Exchange = String.Empty, Type = 1 },
                //            Periodicity = CommonObjects.Periodicity.Minute,
                //            Interval = 1,
                //            Id = Guid.NewGuid().ToString("N"),
                //            BarsCount = 1000,
                //            From = new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc),
                //            To = new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc)
                //        }
                //    };
                //    aValue = JsonConvert.SerializeObject(aRequest);
                //}
                else if (aValue == "HM" || aValue == "hm")
                {
                    HistoryRequest aRequest = new HistoryRequest()
                    {
                        Selection = new HistoryParameters()
                        {
                            Symbol = new SymbolItem() { Symbol = "MSFT", DataFeed = "DDF", Exchange = String.Empty, Type = 1 },
                            Periodicity = CommonObjects.Periodicity.Minute,
                            Interval = 1,
                            Id = Guid.NewGuid().ToString("N"),
                            BarsCount = 100,
                            From = DateTime.UtcNow.Date.AddDays(-1),
                            To = DateTime.UtcNow
                        }
                    };
                    aValue = JsonConvert.SerializeObject(aRequest);
                }*/
                #endregion

                RequestMessage aBaseTypeReq = JsonConvert.DeserializeObject<RequestMessage>(aValue);
                RequestMessage requestMessage = null;

                if (aBaseTypeReq.MsgType == typeof(LoginRequest).Name)
                {
                    LoginRequest aLoginRequest = JsonConvert.DeserializeObject<LoginRequest>(aValue);

                    if (DataServer.MessageRouter.gMessageRouter.Authenticate(aLoginRequest.Login, aLoginRequest.Password))
                    {

                        WebSocketUserInfo aUserInfo;

                        aUserInfo = new WebSocketUserInfo(aLoginRequest.Login, aSession);
                        lock (DataServer.MessageRouter.gMessageRouter)
                        {
                            DataServer.MessageRouter.gMessageRouter.AddSession(aUserInfo.ID, aUserInfo);
                        }
                        WriteToLog_Info(String.Format("Login succeeded: user = '{0}' id = '{1}'", aLoginRequest.Login, aUserInfo.ID), null);
                        //aSession.Send(JsonConvert.SerializeObject(new LoginResponse(aUserInfo.Login)));
                        aSession.Send(JsonConvert.SerializeObject(new LoginResponse() 
                        { 
                            Login = aLoginRequest.Login 
                        }));
                    }
                    else
                    {
                        WriteToLog_Warning(String.Format("Login error: user = '{0}'", aLoginRequest.Login), null);
                        //aSession.Send(JsonConvert.SerializeObject(new LoginResponse() { Login = aLoginRequest.Login }));
                        aSession.Send(JsonConvert.SerializeObject(new LoginResponse()
                        {
                            Login = aLoginRequest.Login,
                            Error = String.Format("Logon error: {0} account is not validated", aLoginRequest.Login)
                        }));
                        //aSession.Send(JsonConvert.SerializeObject(new ErrorInfo(String.Format("Logon error: {0} account is not validated", aLoginRequest.Login))));
                        aSession.Close();

                        return;
                    }
                }
                else if (aBaseTypeReq.MsgType == typeof(LogoutRequest).Name)
                {
                    var aLogoutRequest = JsonConvert.DeserializeObject<LogoutRequest>(aValue);

                    WriteToLog_Info(String.Format("Logout : id = '{0}'", aSession.SessionID), null);
                    aSession.Close();
                }
                else if (aBaseTypeReq.MsgType == typeof(RemoteNotificationRequest).Name)
                {

                    if (DataServer.MessageRouter.gMessageRouter.RemoteNotificator != null)
                    {
                        var aRemoteNotificationRequest =
                            JsonConvert.DeserializeObject<RemoteNotificationRequest>(aValue);
                        lock (DataServer.MessageRouter.gMessageRouter)
                        {
                            string aCurrentUserName =
                                DataServer.MessageRouter.gMessageRouter.GetUserInfo(aSession.SessionID).Login;
                            try
                            {
                                DataServer.MessageRouter.gMessageRouter.RemoteNotificator.SetDeviceTokenAndTypeForUser(
                                    aCurrentUserName, aRemoteNotificationRequest.DeviceToken,
                                    aRemoteNotificationRequest.DeviceType);
                            }
                            catch
                            {
                                
                            }
                        }
                    }
                }
                else if (aBaseTypeReq.MsgType == typeof(DataFeedListRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<DataFeedListRequest>(aValue);
                }
                else if (aBaseTypeReq.MsgType == typeof(SubscribeRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<SubscribeRequest>(aValue);
                }
                else if (aBaseTypeReq.MsgType == typeof(UnsubscribeRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<UnsubscribeRequest>(aValue);
                }
                else if (aBaseTypeReq.MsgType == typeof(HistoryRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<HistoryRequest>(aValue);
                    /*aHistoryRequest.Selection.From = aHistoryRequest.Selection.From.ToUniversalTime();
                    aHistoryRequest.Selection.To = aHistoryRequest.Selection.To.ToUniversalTime();*/
                }
                else if (aBaseTypeReq.MsgType == typeof(AlertSubscribeRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<AlertSubscribeRequest>(aValue);
                }
                else if (aBaseTypeReq.MsgType == typeof(AlertUnsubscribeRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<AlertUnsubscribeRequest>(aValue);
                }
                else if (aBaseTypeReq.MsgType == typeof(AlertsHistoryRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<AlertsHistoryRequest>(aValue);
                }
                else if (aBaseTypeReq.MsgType == typeof(BacktestGetRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<BacktestGetRequest>(aValue);
                }
                else if (aBaseTypeReq.MsgType == typeof(L2SubscribeRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<L2SubscribeRequest>(aValue);
                }
                else if (aBaseTypeReq.MsgType == typeof(L2UnsubscribeRequest).Name)
                {
                    requestMessage = JsonConvert.DeserializeObject<L2UnsubscribeRequest>(aValue);
                }
                else
                {
                    ///need implement 
                    //throw new ApplicationException(String.Format("{0} message is unknown.", aBaseTypeReq.MsgType));
                }

                if (requestMessage != null)
                {
                    lock (DataServer.MessageRouter.gMessageRouter)
                    {
                        WebSocketUserInfo aUserInfo = DataServer.MessageRouter.gMessageRouter.GetUserInfo(aSession.SessionID) as WebSocketUserInfo;
                        if (aUserInfo != null)
                            DataServer.MessageRouter.gMessageRouter.ProcessRequest(aUserInfo.ID, requestMessage);
                    }
                }

            }
            catch (Exception e)
            {
                WriteToLog(aValue, e);
                aSession.Send(JsonConvert.SerializeObject(new ErrorInfo("DataServer Error: unexpected message received")));
                aSession.Close();
            }
        }

        /// <summary>
        /// indicates that a bytes array received
        /// </summary>
        /// <param name="aSession">WebSocket session</param>
        /// <param name="aValue">bytes array</param>
        void OnNewDataReceived(WebSocketSession aSession, byte[] aValue)
        {
            return;
        }
    }



    class WebSocketUserInfo : CommonObjects.IUserInfo
    {
        private WebSocketSession m_WebSocketSession;

        public WebSocketUserInfo(string aLogin, WebSocketSession aWebSocketSession)
        {
            m_WebSocketSession = aWebSocketSession;
            this.Login = aLogin;
        }

        #region IUserInfo implementation
        public string Login { get; set; }

        public string ID
        {
            get
            {
                return m_WebSocketSession.SessionID;
            }

            set
            {
                throw new ApplicationException("Session ID can not be initialized explicitly");
            }
        }

        public object SessionObject
        {
            get
            {
                return m_WebSocketSession;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void Send(CommonObjects.ResponseMessage aResponse)
        {
            string s = JsonConvert.SerializeObject(aResponse);
            m_WebSocketSession.Send(s);
        }

        public void SendError(Exception e)
        {
            m_WebSocketSession.Send(JsonConvert.SerializeObject(new ErrorInfo(e.Message)));
        }

        public void Heartbeat()
        {
        }

        public void Disconnect()
        {
            m_WebSocketSession.Close();
        }
        #endregion
    }


    [JsonObject]
    public class LogoutRequest : RequestMessage
    {
        public LogoutRequest()
        {
        }
    }
}
