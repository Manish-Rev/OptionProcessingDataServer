using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace DataServer
{
    /// <summary>
    /// Logger class implements functionality to log strings or exceptions
    /// </summary>
    public class Logger
    {
        private string m_Target;        // logger name used to find proper logger
        private NLog.Logger m_Logger;   // logger

        /// <summary>
        /// constructor, name of logger must be specified
        /// </summary>
        /// <param name="aTarget">specifies name of logger that may not be changed</param>
        public Logger(string aTarget)
        {
            m_Target = aTarget;
            try
            {
                m_Logger = LogManager.GetLogger(m_Target);
            }
            catch
            {
            }
            if (m_Logger == null)
                m_Logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// name of logger, read only
        /// </summary>
        public string Target
        {
            get { return m_Target; }
        }

        /// <summary>
        /// writes error, generally used for critical errors
        /// </summary>
        /// <param name="msg">additional info, optional parameter</param>
        /// <param name="e">exception, optional parameter</param>
        public void WriteToLog(string msg, Exception e)
        {
            if (!String.IsNullOrEmpty(msg) && e != null)
                m_Logger.ErrorException(msg, e);
            else if (!String.IsNullOrEmpty(msg))
                m_Logger.Error(msg);
            else if (e != null)
                m_Logger.Error(e);
        }

        /// <summary>
        /// writes warning, generally used for non critical error
        /// </summary>
        /// <param name="msg">additional info, optional parameter</param>
        /// <param name="e">exception, optional parameter</param>
        public void WriteToLog_Warning(string msg, Exception e)
        {
            if (!String.IsNullOrEmpty(msg) && e != null)
                m_Logger.WarnException(msg, e);
            else if (!String.IsNullOrEmpty(msg))
                m_Logger.Warn(msg);
            else if (e != null)
                m_Logger.Warn(e);
        }

        /// <summary>
        /// writes info, generally used to log an additional info
        /// </summary>
        /// <param name="msg">additional info, optional parameter</param>
        /// <param name="e">exception, optional parameter</param>
        public void WriteToLog_Info(string msg, Exception e)
        {
            if (!String.IsNullOrEmpty(msg) && e != null)
                m_Logger.InfoException(msg, e);
            else if (!String.IsNullOrEmpty(msg))
                m_Logger.Info(msg);
            else if (e != null)
                m_Logger.Info(e);
        }
    }
}
