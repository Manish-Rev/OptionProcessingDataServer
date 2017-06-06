using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonObjects;
using PushSharp;
using PushSharp.Apple;
using PushSharp.Core;

namespace DataServer
{
    using System.Linq.Expressions;

    public class RemoteNotification
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, DeviceInfo> m_DeviceInfoByLogin;
        /// <summary>
        /// broker for sending push notifications 
        /// </summary>
        private readonly PushBroker pushBrokerIOS;
        /// <summary>
        /// broker for sending push notifications to OSX 
        /// </summary>
        private readonly PushBroker pushBrokerOSX;
        /// <summary>
        /// constructor
        /// </summary>
        public RemoteNotification(ApnSettings apnSettings)
        {
            m_DeviceInfoByLogin = new Dictionary<string, DeviceInfo>();
            ///Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "M4_PUSH_Production.p12")
            pushBrokerIOS = new PushBroker();
            var appleCertIOS = File.ReadAllBytes(apnSettings.CertPathIOS);
            pushBrokerIOS.RegisterAppleService(new ApplePushChannelSettings(apnSettings.IsProductionIOS, appleCertIOS, apnSettings.CertPasswordIOS)); // Extension method
            pushBrokerOSX = new PushBroker();
            var appleCertOSX = File.ReadAllBytes(apnSettings.CertPathOSX);

            pushBrokerOSX.RegisterAppleService(new ApplePushChannelSettings(apnSettings.IsProductionOSX, appleCertOSX, apnSettings.CertPasswordOSX, true)); // Extension method
        }

        public virtual void SetDeviceTokenAndTypeForUser(string aLogin, string aDeviceToken, string aDeviceType)
        {
            DeviceInfo newDevice = new DeviceInfo(aDeviceToken, aDeviceType);
            lock (m_DeviceInfoByLogin)
            {
                if (m_DeviceInfoByLogin.ContainsKey(aLogin))
                {
                    m_DeviceInfoByLogin[aLogin] = newDevice;
                }
                else
                {
                    m_DeviceInfoByLogin.Add(aLogin,newDevice);
                }
            }
        }

        public DeviceInfo GetDeviceInfoByLogin(string aLogin)
        {
            if (m_DeviceInfoByLogin.ContainsKey(aLogin)) return m_DeviceInfoByLogin[aLogin];
            else return null;
        }
        public virtual void Notify(string aLogin, string aId, string aAlertName, Alert aAlert)
        {
            DeviceInfo destinationDevice;
            bool deviceInfoExist;
            lock (m_DeviceInfoByLogin)
            {
                deviceInfoExist = m_DeviceInfoByLogin.TryGetValue(aLogin, out destinationDevice);
            }
            if (deviceInfoExist && !destinationDevice.AlreadyNotifiedAlertsIds.Contains(aId))
            {
                if (!destinationDevice.IsNotificationTimerStarted())
                {
                    destinationDevice.StartNotificationTimer();
                }
                string alertInfo = "\nSymbol: " + aAlert.Symbol.Symbol + "\nDate: " + String.Format("{0:MM/dd/yyyy HH:mm:ss}", aAlert.Timestamp);
                if (Equals(destinationDevice.DeviceType, "apple"))
                {
                    pushBrokerIOS.QueueNotification(new AppleNotification()
                        .ForDeviceToken(destinationDevice.DeviceToken)
                        .WithAlert("Alert: \"" + aAlertName + "\"" + alertInfo)
                        .WithSound("default")
                        .WithCustomItem("id", aId));
                }
                else if (Equals(destinationDevice.DeviceType, "appleOSX"))
                {
                    pushBrokerOSX.QueueNotification(new AppleNotification()
                       .ForDeviceToken(destinationDevice.DeviceToken)
                       .WithAlert("Alert: \"" + aAlertName + "\"" + alertInfo)
                       .WithSound("default")
                       .WithCustomItem("id", aId));
                }
                else
                {
                    // for other devices
                }
                destinationDevice.AlreadyNotifiedAlertsIds.Add(aId);
            }
        }
    }
}
