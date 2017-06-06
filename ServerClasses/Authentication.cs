using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;

namespace DataServer
{
    /// <summary>
    /// The сlass implements functionality to autentificate user. DB used to store logon info
    /// </summary>
    public class Authentication
    {
        /// <summary>
        /// DB connection string
        /// </summary>
        private string m_ConnectionString = String.Empty;
        /// <summary>
        /// logger
        /// </summary>
        private Logger m_Logger;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="aLogger">logger</param>
        public Authentication(Logger aLogger)
        {
            m_Logger = aLogger;
        }

        /// <summary>
        /// returns connection string
        /// </summary>
        public string ConnectionStr
        {
            get
            {
                return m_ConnectionString;
            }
        }

        /// <summary>
        /// initializes object
        /// </summary>
        /// <param name="aConfigFile">configuration file name</param>
        public virtual void Init(string aConnectionStr)
        {
            using (SqlConnection aConnection = new SqlConnection(aConnectionStr))
            {
                aConnection.Open();
                aConnection.Close();
                m_ConnectionString = aConnectionStr;
            }
        }

        /// <summary>
        /// validates user's credentials
        /// </summary>
        /// <param name="login">user name</param>
        /// <param name="password">password</param>
        /// <returns>true, if user credentials validated otherwise false</returns>
        public virtual bool Login(string login, string password)
        {
            bool isLoggedIn = false;

            using (SqlConnection aConnection = new SqlConnection(m_ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("Select top(1) [Login] from [dbo].[Users] where [Login]=@login AND [Password]=@password",
                                                aConnection);
                cmd.Parameters.AddWithValue("login", login);
                cmd.Parameters.AddWithValue("password", password);
                try
                {
                    object value;

                    aConnection.Open();
                    value = cmd.ExecuteScalar();
                    isLoggedIn = value != null && value != DBNull.Value;
                }
                catch (Exception e)
                {
                    m_Logger.WriteToLog_Info("User: " + login, e);
                }
            }

            return isLoggedIn;
        }
    }
}
