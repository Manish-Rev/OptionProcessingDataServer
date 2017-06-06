using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Collections.Generic;
using CommonObjects;
using System.Threading;
using System.IO;

namespace WCFServiceHost
{
    /// <summary>
    /// WCF connection service host
    /// </summary>
    public class DataServerWCFServiceHost : DataServer.Logger, DataServer.IDataServerServiceHost
    {
        private DataServerWCFService m_ServiceCore;
        private ServiceHost m_ServiceHost;

        public DataServerWCFServiceHost()
            : base("WCF")
        {
        }

        #region IDataServerServiceHost implementation
        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public Dictionary<string, object> DefaultSettings
        {
            get
            {
                Dictionary<string, object> aDict = new Dictionary<string, object>();

                aDict["ip"] = "127.0.0.1";
                aDict["port"] = "4504";

                return aDict;
            }
        }

        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public string Name
        {
            get { return base.Target; }
        }

        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public void Start(Dictionary<string, object> args)
        {
            //configure wcf
            string localIp = null;
            int port = 0;
            int design_time_port = 0;

            WriteToLog_Info("Service host starting...", null);
            if (args.ContainsKey("ip"))
                localIp = args["ip"] as string;
            if (localIp == null)
                throw new Exception("WCF session manager can not start because 'ip' parameter is not specified");
            if (args.ContainsKey("port"))
                Int32.TryParse(args["port"].ToString(), out port);
            if (port == 0)
                throw new Exception("WCF session manager can not start because 'port' parameter is not specified");
            if (args.ContainsKey("design_time_port"))
                Int32.TryParse(args["design_time_port"].ToString(), out design_time_port);

            TimeSpan timeOut = new TimeSpan(0, 1, 0);
            List<Uri> aURIs = new List<Uri>();

            aURIs.Add(new Uri(string.Format("net.tcp://{0}:{1}/DataServer_Service", localIp, port)));
            if (design_time_port != 0)
                aURIs.Add(new Uri(string.Format("http://{0}:{1}/DataServer_Service", localIp, design_time_port)));

            NetTcpBinding binding = new NetTcpBinding();
            binding.TransactionFlow = false;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.ReceiveTimeout = timeOut;
            binding.SendTimeout = timeOut;
            binding.OpenTimeout = timeOut;
            binding.CloseTimeout = timeOut;
            binding.ReliableSession.InactivityTimeout = timeOut;
            binding.Security.Mode = SecurityMode.None;
            binding.MaxBufferSize = 1073741823;
            binding.MaxReceivedMessageSize = 1073741823;
            // start WCF service
            m_ServiceCore = new DataServerWCFService(this.Name);
            m_ServiceCore.Start();
            m_ServiceHost = new ServiceHost(m_ServiceCore, aURIs.ToArray());
            m_ServiceHost.AddServiceEndpoint(typeof(IWCFService), binding, aURIs[0]);
            m_ServiceHost.Description.Behaviors.Add(new ServiceMetadataBehavior());
            if (design_time_port != 0)
                m_ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
            
            m_ServiceHost.Open();
            WriteToLog_Info("Service host started.", null);
        }

        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public void Stop()
        {
            WriteToLog_Info("Service host stopping...", null);
            if (m_ServiceCore != null)
            {
                m_ServiceCore.Stop();
                m_ServiceCore = null;
            }
            if (m_ServiceHost != null)
            {
                if (m_ServiceHost.State == CommunicationState.Opened)
                {
                    try
                    {
                        m_ServiceHost.Close();
                    }
                    catch (Exception e)
                    {
                        WriteToLog(null, e); 
                    }
                }
                m_ServiceHost = null;
            }
            WriteToLog_Info("Service host stopped", null);
        }

        /// <summary>
        /// see IDataServerServiceHost interface
        /// </summary>
        public ILogonControl LogonControl
        {
            get
            {
                return new LogonControl();
            }
        }
#endregion
    }
}
