using System.Windows.Forms;
using System.Collections.Generic;
using CommonObjects;

namespace DDFDataFeed
{
    public partial class LogonControl : UserControl, ILogonControl
    {
        public LogonControl()
        {
            InitializeComponent();
        }

        public void ValidateSettings()
        {
        }

        public Dictionary<string, object> Settings
        {
            get
            {
                Dictionary<string, object> aDict = new Dictionary<string, object>();

                aDict.Add("user_name", txtLogin.Text.Trim());
                aDict.Add("password", txtPassword.Text.Trim());

                return aDict;
            }
            set
            {
                SuspendLayout();
                try
                {
                    txtLogin.Text = string.Empty;
                    txtPassword.Text = string.Empty;
                    if (value != null)
                    {
                        if (value.ContainsKey("user_name"))
                            txtLogin.Text = value["user_name"].ToString();
                        if (value.ContainsKey("password"))
                            txtPassword.Text = value["password"].ToString();
                    }
                }
                catch
                {
                }
                finally
                {
                    ResumeLayout();
                }
            }
        }

    }
}
