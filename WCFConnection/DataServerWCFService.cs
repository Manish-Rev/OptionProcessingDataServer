using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ServiceModel;
using CommonObjects;
using System.IO;
using System.ServiceModel.Web;

namespace WCFServiceHost
{
    /// <summary>
    /// implements core functionality to process incoming/outgoing messages
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DataServerWCFService : DataServer.Logger, IWCFService
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="aTarget">name of service used to get logger</param>
        public DataServerWCFService(string aTarget) : base(aTarget)
        {
        }

        /// <summary>
        /// starts service
        /// </summary>
        public void Start()
        {
            WriteToLog_Info("Service core started.", null);
        }

        /// <summary>
        /// stops service
        /// </summary>
        public void Stop()
        {
            WriteToLog_Info("Service core stopped.", null);
        }

        #region IWCFService Interface
        /// <summary>
        /// sends incoming message to message router
        /// </summary>
        /// <param name="aRequest"></param>
        public void MessageIn(RequestMessage aRequest)
        {
            DataServer.MessageRouter.gMessageRouter.ProcessRequest(GetSessionID(OperationContext.Current.SessionId), aRequest);
        }

        /// <summary>
        /// implements functionalities: authenticate, initialize user/session info, add user/session info
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public LoginResponse Login(LoginRequest aRequest)
        {
            LoginResponse aResponse = new LoginResponse();

            try
            {
                if (DataServer.MessageRouter.gMessageRouter.Authenticate(aRequest.Login, aRequest.Password))
                {
                    WCFUserInfo aUserInfo;


                    aUserInfo = new WCFUserInfo(aRequest.Login, GetSessionID(OperationContext.Current.SessionId), OperationContext.Current);
                    lock (DataServer.MessageRouter.gMessageRouter)
                    {
                        DataServer.MessageRouter.gMessageRouter.AddSession(aUserInfo.ID, aUserInfo);
                        aUserInfo.Chanel.Closed += Chanel_Closed;
                        aUserInfo.Chanel.Faulted += Chanel_Closed;
                    }
                    WriteToLog_Info(String.Format("Login succeeded: user = '{0}' id = '{1}'", aRequest.Login, aUserInfo.ID), null);
                }
                else
                {
                    WriteToLog_Warning(String.Format("Login error: user = '{0}'", aRequest.Login), null);

                    throw new ApplicationException("Logon fault.");
                }
            }
            catch (Exception ex)
            {
                throw new FaultException<DataServerException>(new DataServerException(ex.Message), new FaultReason(ex.Message));
            }

            return aResponse;
        }

        public void LogOut()
        {
            if (OperationContext.Current == null)
                return;

            string aID = OperationContext.Current.SessionId;

            try
            {
                WCFUserInfo aUserInfo;

                lock (DataServer.MessageRouter.gMessageRouter)
                {
                    aUserInfo = DataServer.MessageRouter.gMessageRouter.RemoveSession(GetSessionID(aID)) as WCFUserInfo;
                    if (aUserInfo != null)
                    {
                        aUserInfo.Chanel.Closed -= Chanel_Closed;
                        aUserInfo.Chanel.Faulted -= Chanel_Closed;
                    }
                }
                if (aUserInfo != null)
                    WriteToLog_Info(String.Format("Logout : user = '{0}' id = '{1}'", aUserInfo.Login, aUserInfo.ID), null);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        #region Helper Methods
        private string GetSessionID(string aID)
        {
            string[] arr = aID.Split(new char[] { ':', ';' });

            return arr[1] + ":" + arr[2];
        }

        /// <summary>
        /// occurs when WCF session closed or failed
        /// </summary>
        /// <param name="sender">session context</param>
        /// <param name="e">event arguments</param>
        void Chanel_Closed(object sender, EventArgs e)
        {
            try
            {
                lock (DataServer.MessageRouter.gMessageRouter)
                {
                    IUserInfo aUserInfo = DataServer.MessageRouter.gMessageRouter.GetUserInfo(sender);

                    if (aUserInfo != null)
                    {
                        DataServer.MessageRouter.gMessageRouter.RemoveSession(aUserInfo.ID);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }




}
