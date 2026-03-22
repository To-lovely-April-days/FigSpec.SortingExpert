using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.Tools;
using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class SortingConnectEject : BaseFormSetting
    {
        public SortingConnectEject()
        {
            InitializeComponent();
        }

        private void SortingConnectEject_Load(object sender, EventArgs e)
        {
            var lst = new List<ValueTextItem<int>>()
            {
                new ValueTextItem<int>(0, "TCP/IP"),
                new ValueTextItem<int>(1, "COM"),
                new ValueTextItem<int>(2, "Csv文件存储".ToMultiLanguage()),
            };
            cmbCommunication.BindComboBoxItem(lst);
            var setting = GlobalSettings.ApplySetting;
            cmbCommunication.SetSelectedValue(setting.CommunicationType);

            textIP.Text = setting.ServerIp;
            textIPPort.Text = setting.ServerPort;
            btnSaveFileName.Text = setting.CommunicationFileName;
            spinEditSaveFrame.Value = setting.FrameNumberPerFile;

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            var appset = GlobalSettings.ApplySetting;
            int communicationType = cmbCommunication.GetSelectedValue<int>() ?? 0;
            appset.CommunicationType = communicationType;

            if (communicationType == 0)
            {
                appset.ServerIp = textIP.Text;
                appset.ServerPort = textIPPort.Text;
            }
            else if (communicationType == 2)
            {
                if(string.IsNullOrEmpty(btnSaveFileName.Text))
                {
                    FormShowHelper.ShowMessage("请选择保存文件".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                appset.CommunicationFileName = btnSaveFileName.Text;
                appset.FrameNumberPerFile = (int)spinEditSaveFrame.Value;
            }

            GlobalSettings.ApplySetting = appset;
            this.Close();
        }

        private void btnGetIPAddress_Click(object sender, EventArgs e)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) 
                {
                    textIP.Text = ip.ToString();
                    break;
                }
            }
        }

        private void cmbCommunication_SelectedIndexChanged(object sender, EventArgs e)
        {
            int communicationType = cmbCommunication.GetSelectedValue<int>() ?? 0;
            pnlTcp.Visible = communicationType != 2;
            pnlTcp.Enabled = communicationType == 0;
            pnlFile.Visible = communicationType == 2;
            pnlFile.Enabled = communicationType == 2;
        }

        private void btnSaveFileName_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV|*.csv";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                btnSaveFileName.Text = saveFileDialog.FileName;
            }
        }
    }
}
