using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SuperWebSocket;

namespace WebSocketsServiceHostOld
{
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
            m_WebSocketSession.Send(JsonConvert.SerializeObject(new CommonObjects.ErrorInfo(e.Message)));
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
    

}
