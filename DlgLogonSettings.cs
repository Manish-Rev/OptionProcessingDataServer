using System;
using System.Drawing;
using System.Windows.Forms;
using CommonObjects;

namespace DataServer
{
    /// <summary>
    /// Control to configure datafeeds and connections
    /// </summary>
    public partial class DlgLogonSettings : Form
    {
        private ILogonControl m_Settings;

        public DlgLogonSettings()
        {
            InitializeComponent();
        }

        public void Init(ILogonControl aSettings)
        {
            m_Settings = aSettings;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                m_Settings.ValidateSettings();
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DlgLogonSettings_Load(object sender, EventArgs e)
        {
            UserControl aUICtrl = (UserControl)m_Settings;

            this.Controls.Add(aUICtrl);
            aUICtrl.Location = new Point(0, 0);
            this.Size = new Size(aUICtrl.Width + 20, aUICtrl.Height + 80);
            aUICtrl.Anchor = AnchorStyles.Top & AnchorStyles.Left & AnchorStyles.Bottom & AnchorStyles.Left;
        }
    }
}
