using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CommonObjects;

namespace DataServer
{
    /// <summary>
    /// The class implements functionality to route incoming and outgoing messages from connection service to feeder and vice-versa
    /// </summary>
    public class MessageRouter
    {
        /// <summary>
        /// global object that requires synchronized access
        /// </summary>
        public static MessageRouter gMessageRouter;

        /// <summary>
        /// message router event argument
        /// </summary>
        public class MessageRouter_EventArgs : EventArgs
        {
            /// <summary>
            /// unique ID, generally, session ID
            /// </summary>
            public readonly string ID;
            /// <summary>
            /// object that represents session/user info specified by ID
            /// </summary>
            public readonly IUserInfo UserInfo;
            /// <summary>
            /// original incoming request message
            /// </summary>
            public readonly RequestMessage Request;
            /// <summary>
            /// true, if the request must be ignored
            /// </summary>
            public bool Cancel = false;
            /// <summary>
            /// reason to ignore the request
            /// </summary>
            public string Reason = String.Empty;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="aID">unique ID, generally it's session ID</param>
            /// <param name="aUserInfo">session/user info object that implements IUserInfo interface</param>
            public MessageRouter_EventArgs(string aID, IUserInfo aUserInfo)
            {
                this.ID = aID;
                this.UserInfo = aUserInfo;
                this.Request = null;
            }

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="aID">unique ID, generally it's session ID</param>
            /// <param name="aUserInfo">session/user info object that implements IUserInfo interface</param>
            /// <param name="aRequest">request message</param>
            public MessageRouter_EventArgs(string aID, IUserInfo aUserInfo, RequestMessage aRequest)
            {
                this.ID = aID;
                this.UserInfo = aUserInfo;
                this.Request = aRequest;
            }
        }

        /// <summary>
        /// fired, when a new session added
        /// </summary>
        public event EventHandler<MessageRouter_EventArgs> AddedSession;
        /// <summary>
        /// fired, when a session removed
        /// </summary>
        public event EventHandler<MessageRouter_EventArgs> RemovedSession;
        /// <summary>
        /// fired, when a client request received
        /// </summary>
        public event EventHandler<MessageRouter_EventArgs> RouteRequest;

        /// <summary>
        /// current active sessions
        /// </summary>
        protected Dictionary<string, IUserInfo> m_ActiveSessions;
        /// <summary>
        /// the object implements user authentication functionality
        /// </summary>
        private Authentication m_Authenticator;
        /// <summary>
        /// the object implements remote notifications functionality
        /// </summary>
        private RemoteNotification m_RemoteNitificator;

        public MessageRouter()
        {
        }

        /// <summary>
        /// returns object that implements user authentication functionality
        /// </summary>
        public Authentication Authenticator
        {
            get { return m_Authenticator; }
        }

        /// <summary>
        /// returns object that implements remote notification functionality
        /// </summary>
        public RemoteNotification RemoteNotificator
        {
            get { return m_RemoteNitificator; }
        }

        /// <summary>
        /// initializes message router
        /// </summary>
        /// <param name="aAuthenticator">authenticator</param>
        /// <param name="aRemoteNotificator">remote notificator</param>
        public void Init(Authentication aAuthenticator, RemoteNotification aRemoteNotificator)
        {
            m_ActiveSessions = new Dictionary<string, IUserInfo>();
            m_Authenticator = aAuthenticator;
            m_RemoteNitificator = aRemoteNotificator;
        }

        /// <summary>
        /// get session/user info by ID
        /// </summary>
        /// <param name="aID">unique ID to identify session</param>
        /// <returns>session/user info</returns>
        public virtual IUserInfo GetUserInfo(string aID)
        {
            IUserInfo aUserInfo;

            lock (m_ActiveSessions)
            {
                if (m_ActiveSessions.TryGetValue(aID, out aUserInfo))
                    return aUserInfo;
                else
                    return null;
            }
        }

        /// <summary>
        /// get user info by an object, generally, the object is session context
        /// </summary>
        /// <param name="obj">session's object to identify session</param>
        /// <returns></returns>
        public virtual IUserInfo GetUserInfo(object obj)
        {
            lock (m_ActiveSessions)
            {
                foreach (IUserInfo item in m_ActiveSessions.Values)
                {
                    if (item.SessionObject != null && object.ReferenceEquals(item.SessionObject, obj))
                        return item;
                }
            }

            return null;
        }

        /// <summary>
        /// get user info by an object, generally, the object is session context
        /// </summary>
        /// <param name="aLogin">user name</param>
        /// <returns></returns>
        public virtual IUserInfo GetUserInfoByLogin(string aLogin)
        {
            lock (m_ActiveSessions)
            {
                foreach (IUserInfo item in m_ActiveSessions.Values)
                {
                    if (Equals(item.Login, aLogin))
                        return item;
                }
            }

            return null;
        }

        /// <summary>
        /// returns unique ID if logon is complete
        /// </summary>
        /// <param name="aLogin">login, generally user name or email</param>
        /// <param name="aPassword">password</param>
        /// <returns>true, if credentials are valid</returns>
        public bool Authenticate(string aLogin, string aPassword)
        {
            if (this.Authenticator != null)
                return this.Authenticator.Login(aLogin, aPassword);
            else
                return false;
        }

        /// <summary>
        /// sends remote notification
        /// </summary>
        /// <param name="aLogin">login, generally user name or email</param>
        /// <param name="aAlertMessage">alert body message</param>
        public void PushRemoteNotification(string aLogin, string aId, string aAlertName, Alert aAlert)
        {
            if (this.RemoteNotificator != null)
                this.RemoteNotificator.Notify(aLogin, aId, aAlertName, aAlert);
        }

        /// <summary>
        /// adds a session identified by uniques ID
        /// </summary>
        /// <param name="aID">unique ID</param>
        /// <param name="aUserInfo">session/user info</param>
        public virtual void AddSession(string aID, IUserInfo aUserInfo)
        {
            if (this.AddedSession != null)
            {
                lock (m_ActiveSessions)
                {
                    MessageRouter_EventArgs args = new MessageRouter_EventArgs(aID, aUserInfo);

                    this.AddedSession(this, new MessageRouter_EventArgs(aID, aUserInfo));
                    if (!args.Cancel)
                    {
                        m_ActiveSessions.Add(aID, aUserInfo);

                        //stoping notification timer when user logined; 
                        if (m_RemoteNitificator != null)
                        {
                            DeviceInfo aDeviceInfo = m_RemoteNitificator.GetDeviceInfoByLogin(aUserInfo.Login);
                            if (aDeviceInfo != null) aDeviceInfo.StopNotificationTimer();
                        }
                    }
                    else
                        throw new ApplicationException("The session is not enabled. Reason: " + args.Reason);
                }
            }
        }

        /// <summary>
        /// removes session by ID
        /// </summary>
        /// <param name="aID">session ID</param>
        /// <returns>removed session/user info</returns>
        public virtual IUserInfo RemoveSession(string aID)
        {
            IUserInfo aUserInfo;

            lock (m_ActiveSessions)
            {
                if (m_ActiveSessions.TryGetValue(aID, out aUserInfo))
                {
                    m_ActiveSessions.Remove(aID);
                    if (this.RemovedSession != null)
                        this.RemovedSession(this, new MessageRouter_EventArgs(aID, aUserInfo));

                    return aUserInfo;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// routes incomming request
        /// </summary>
        /// <param name="aID">session ID where the request received</param>
        /// <param name="aRequest">incoming request</param>
        public virtual void ProcessRequest(string aID, RequestMessage aRequest)
        {
            IUserInfo aUserInfo = GetUserInfo(aID);

            if (aUserInfo != null)
            {
                if (this.RouteRequest != null)
                    this.RouteRequest(this, new MessageRouter_EventArgs(aID, aUserInfo, aRequest));
            }
        }

        /// <summary>
        /// dispose object
        /// </summary>
        public void Dispose()
        {
            lock (m_ActiveSessions)
            {
                m_ActiveSessions.Clear();
                m_ActiveSessions = null;
            }
            m_Authenticator = null;
        }
    }
}
