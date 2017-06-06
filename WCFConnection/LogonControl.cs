using System;
using System.Windows.Forms;
using System.Collections.Generic;
using CommonObjects;

namespace WCFServiceHost
{
    public partial class LogonControl : UserControl, ILogonControl
    {
        public LogonControl()
        {
            InitializeComponent();
        }

        public void ValidateSettings()
        {
            if (txtAddress.Text.Trim().Length == 0)
            {
                throw new Exception("Invalid Address");
            }
        }

        public Dictionary<string, object> Settings
        {
            get
            {
                Dictionary<string, object> aDict = new Dictionary<string, object>();

                aDict["ip"] = txtAddress.Text.Trim();
                aDict["port"] = numPort.Value.ToString();
                aDict["design_time_port"] = numDTPort.Value.ToString();

                return aDict;
            }
            set
            {
                SuspendLayout();
                try
                {
                    txtAddress.Text = string.Empty;
                    numPort.Value = 1005;
                    numDTPort.Value = 0;
                    if (value.ContainsKey("ip"))
                        txtAddress.Text = value["ip"].ToString();
                    if (value.ContainsKey("port"))
                    {
                        int iVal;

                        if (Int32.TryParse(value["port"].ToString(), out iVal))
                            numPort.Value = iVal;
                    }
                    if (value.ContainsKey("design_time_port"))
                    {
                        int iVal;

                        if (Int32.TryParse(value["design_time_port"].ToString(), out iVal))
                            numDTPort.Value = iVal;
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
