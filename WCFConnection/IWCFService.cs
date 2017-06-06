using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using CommonObjects;

namespace WCFServiceHost
{
    public interface IWCFServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void MessageOut(ResponseMessage message);
    }

    [ServiceContract(CallbackContract = typeof(IWCFServiceCallback), Name = "IWCFService", SessionMode = SessionMode.Required)]
    public interface IWCFService
    {
        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(DataServerException))]
        LoginResponse Login(LoginRequest message);

        [OperationContract(IsTerminating = true, IsInitiating = false, IsOneWay = true)]
        void LogOut();

        [OperationContract(IsInitiating = false, IsOneWay = true)]
        void MessageIn(RequestMessage message);
    }

    /// <summary>
    /// Special user info for WCF connection
    /// </summary>
    public class WCFUserInfo : IUserInfo
    {
        public IWCFServiceCallback CallBack;
        public OperationContext Context;
        public IContextChannel Chanel;

        public WCFUserInfo(string aLogin, string aSessionId, OperationContext aCtx)
        {
            this.Login = aLogin;
            this.ID = aSessionId;
            this.Context = aCtx;
            this.CallBack = aCtx.GetCallbackChannel<IWCFServiceCallback>();
            this.Chanel = aCtx.Channel;
            this.SessionObject = aCtx.Channel;
        }


        public void Send(ResponseMessage aResponse)
        {
            this.CallBack.MessageOut(aResponse);
        }

        public void SendError(Exception e)
        {
            this.CallBack.MessageOut(new ErrorInfo(e));
        }

        public void Disconnect()
        {
            this.Send(new HeartbeatResponse("close session"));
            this.Chanel.Close();
        }

        public string Login { get; set; }

        public string ID { get; set; }

        public object SessionObject { get; set; }

        public void Heartbeat()
        {
            this.Send(new HeartbeatResponse(String.Empty));
        }
    }
}
