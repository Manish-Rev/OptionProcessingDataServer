using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonObjects;

namespace WebSocketsServiceHost
{
    public partial class LogonControl : UserControl, ILogonControl
    {
        public LogonControl()
        {
            InitializeComponent();
        }

        public Dictionary<string, object> Settings
        {
            get
            {
                Dictionary<string, object> aDict = new Dictionary<string, object>();

                aDict["ip"] = txtAddress.Text.Trim();
                aDict["port"] = numPort.Value.ToString();

                return aDict;
            }
            set
            {
                SuspendLayout();
                try
                {
                    txtAddress.Text = string.Empty;
                    numPort.Value = 2012;
                    if (value.ContainsKey("ip"))
                        txtAddress.Text = value["ip"].ToString();
                    if (value.ContainsKey("port"))
                    {
                        int iVal;

                        if (Int32.TryParse(value["port"].ToString(), out iVal))
                            numPort.Value = iVal;
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

        public void ValidateSettings()
        {
            if (txtAddress.Text.Trim().Length == 0)
            {
                throw new Exception("Invalid Address");
            }
        }
    }
}
