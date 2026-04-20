using DevExpress.XtraEditors.Controls;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Tools;
using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class EjectIpSettingForm : BaseFormSetting
    {
        Action refreshControl;
        public EjectIpSettingForm(bool connectDisable, Action action)
        {
            this.refreshControl = action;
            InitializeComponent();
            panelControl2.Enabled = !connectDisable;
        }

        private void EjectIpSettingForm_Load(object sender, EventArgs e)
        {
            var setting = GlobalSettings.ApplySetting;
            //显示本机Ip和地址
            string ip = ClientControl.Getip();
            textIP.Text = ip;
            txtServerPort.Text = setting.ServerPort;
            string[] ports = SerialPort.GetPortNames();
            chkTcpConnect.Checked = setting.EjectDeviceConnectTcp;
            txtEjectIp.Text = setting.EjectDeviceIp;
            txtEjectPort.Text = setting.EjectDevicePort.ToString();

            ComboBoxItemCollection itemCollection = cmbCOM.Properties.Items;
            itemCollection.BeginUpdate();
            itemCollection.Clear();
            itemCollection.AddRange(ports);
            itemCollection.EndUpdate();
            cmbCOM.SelectedItem = setting.EjectDeviceCom;


            spinDelayTime.EditValue = setting.EjectAirDelay;
            spinBlowTime.EditValue = setting.EjectAirTime;
            spActivatePixelsX.EditValue = setting.ActivatePixelsX;
            spActivatePixelsY.EditValue = setting.ActivatePixelsY;

            spPixelNumber.EditValue = setting.TracheaSet.PixelNumber;
            spStartPixel.EditValue = setting.TracheaSet.StartPixel;
            spEndPixel.EditValue = setting.TracheaSet.EndPixel;
            spTrachea.EditValue = setting.TracheaSet.Trachea;
           



            spTracheaPixels.EditValue = setting.TracheaSet.TracheaPixels;
            spPixelInterval.EditValue = setting.TracheaSet.PixelInterval;
            chkTracheaDesc.Checked = setting.TracheaSet.TracheaDesc;

            gcConfig.DataSource = setting.TracheaSet.Items;
            gcConfig.RefreshDataSource();

            RefreshConnectCtrl();

            chkTcpConnect_CheckedChanged(null, null);

        }

        private void RefreshConnectCtrl()
        {
            bool isConnnected = ClassifierControl.Shared.IsConnected;
            txtServerPort.ReadOnly = isConnnected;
            cmbCOM.ReadOnly = isConnnected;
            chkTcpConnect.ReadOnly = isConnnected;
            txtEjectIp.ReadOnly = isConnnected;
            txtEjectPort.ReadOnly = isConnnected;
            btnConnect.Text = isConnnected ? "断开连接".ToMultiLanguage() : "连接".ToMultiLanguage();
        }

        private void EjectIpSettingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //SaveData();
        }

        private void SaveData()
        {
            var appset = GlobalSettings.ApplySetting;
            appset.ServerIp = textIP.Text;
            appset.ServerPort = txtServerPort.Text.Trim();
            appset.EjectDeviceConnectTcp = chkTcpConnect.Checked;
            appset.EjectDeviceIp = txtEjectIp.Text.Trim();
            int.TryParse(txtEjectPort.Text.Trim(), out int ejectPort);
            appset.EjectDevicePort = ejectPort;

            if (cmbCOM.SelectedItem != null)
            {
                appset.EjectDeviceCom = (string)cmbCOM.SelectedItem;
            }
            appset.EjectAirDelay = (uint)spinDelayTime.Value;
            appset.EjectAirTime = (uint)spinBlowTime.Value;
            appset.ActivatePixelsX = (int)spActivatePixelsX.Value;
            appset.ActivatePixelsY = (int)spActivatePixelsY.Value;
            GlobalSettings.ApplySetting = appset;

            refreshControl?.Invoke();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            btnApply_Click(null, null);
            this.Close();
        }

        private void btnCreateConfig_Click(object sender, EventArgs e)
        {
            try
            {
                var pixelNumber = Convert.ToInt32(spPixelNumber.Value);
                var startPixel = Convert.ToInt32(spStartPixel.Value);
                var endPixel = Convert.ToInt32(spEndPixel.Value);

                var trachea = Convert.ToInt32(spTrachea.Value);
       
                var pixelInterval = Convert.ToSingle(spPixelInterval.Value);//偏移量


                if (pixelNumber < 1 || startPixel < 1 || endPixel < 1 || trachea < 1  )
                {
                    FormShowHelper.ShowMessage("参数配置必须大于零".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                if (startPixel > pixelNumber || endPixel > pixelNumber)
                {
                    FormShowHelper.ShowMessage("起始像素和终止像素必须在像素个数范围内".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                if (startPixel > endPixel)
                {
                    FormShowHelper.ShowMessage("起始像素不能大于终止像素".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }

                List<TracheaSetItem> configs = new List<TracheaSetItem>();
                float lInterval = (float)(endPixel - startPixel+1) / (trachea);
                float extra = 3;//
                for (int i = 0; i <= trachea; i++)
                {
                    float loc = startPixel + pixelInterval + lInterval * (i ) ;

                    configs.Add(new TracheaSetItem()
                    {
                        TracheaNumber = chkTracheaDesc.Checked ? (trachea - i) : i+1,
                        StartPixel = loc - lInterval / 2- extra,
                        EndPixel = loc + lInterval / 2+ extra
                    });
                }

                gcConfig.DataSource = configs;
                gcConfig.RefreshDataSource();

                var set = GlobalSettings.ApplySetting.TracheaSet;
                set.PixelNumber = pixelNumber;
                set.StartPixel = startPixel;
                set.EndPixel = endPixel;
                set.Trachea = trachea;

                set.PixelInterval = pixelInterval;
                set.TracheaDesc = chkTracheaDesc.Checked;
                set.Items = configs;

                refreshControl?.Invoke();
            }
            catch (Exception)
            {
                FormShowHelper.ShowMessage("参数设置异常".ToMultiLanguage(), "提示".ToMultiLanguage());
            }
        }

        private void gvConfig_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            int pixelNumber = GlobalSettings.ApplySetting.TracheaSet.PixelNumber;
            var rowData = gvConfig.GetFocusedRow() as TracheaSetItem;
            if (gvConfig.FocusedColumn == colStartPixel)
            {
                if (!int.TryParse(e.Value?.ToString(), out int startPixel))
                {
                    e.Valid = false;
                    return;
                }
                if (startPixel > pixelNumber || startPixel < 0)
                {
                    e.ErrorText = string.Format("起始像素必须在像素个数[0~{0}]范围内".ToMultiLanguage(), pixelNumber);
                    e.Valid = false;
                    return;
                }
                if (startPixel > rowData.EndPixel)
                {
                    e.ErrorText = "起始像素不能大于终止像素".ToMultiLanguage();
                    e.Valid = false;
                    return;
                }
            }
            else if (gvConfig.FocusedColumn == colEndPixel)
            {
                if (!int.TryParse(e.Value?.ToString(), out int endPixel))
                {
                    e.Valid = false;
                    return;
                }
                if (endPixel > pixelNumber || endPixel < 0)
                {
                    e.ErrorText = string.Format("终止像素必须在像素个数[0~{0}]范围内".ToMultiLanguage(), pixelNumber);
                    e.Valid = false;
                    return;
                }
                if (endPixel < rowData.StartPixel)
                {
                    e.ErrorText = "终止像素不能小于起始像素".ToMultiLanguage();
                    e.Valid = false;
                    return;
                }
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            FormShowHelper.ShowLoadingForm(this, "应用中".ToMultiLanguage() + "...");
            SaveData();
            if (ClassifierControl.Shared.IsConnected)
            {
                ClassifierControl.Shared.ApplyAirParams();
            }
            FormShowHelper.CloseLoadingForm();
        }

        private void repositoryItemButtonEdit1_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            var rowData = gvConfig.GetFocusedRow() as TracheaSetItem;
            bool isConnected = ClassifierControl.Shared.IsConnected;

            LogHelper.WriteLog($"[EJECT-BTN] 点击吹气按钮, rowData={(rowData == null ? "NULL" : "气管号=" + rowData.TracheaNumber)}, IsConnected={isConnected}, 吹气时长={spinBlowTime.Value}ms");

            if (rowData == null)
            {
                LogHelper.WriteLog("[EJECT-BTN] 已退出: rowData 为 NULL (没选中任何一行)");
                FormShowHelper.ShowMessage("请先选中一行气管配置".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            if (!isConnected)
            {
                LogHelper.WriteLog("[EJECT-BTN] 已退出: 气吹设备未连接,请先点顶部'连接'按钮");
                FormShowHelper.ShowMessage("气吹设备未连接,请先点顶部'连接'按钮".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }

            int portIndex = rowData.TracheaNumber - 1;
            ushort duration = (ushort)spinBlowTime.Value;
            LogHelper.WriteLog($"[EJECT-BTN] 调用 Shared.Eject, ports=[{portIndex}], duration={duration}ms");

            try
            {
                ClassifierControl.Shared.Eject(new int[] { portIndex }, duration);
                LogHelper.WriteLog("[EJECT-BTN] Shared.Eject 调用完成");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"[EJECT-BTN] Shared.Eject 抛异常: {ex.Message}");
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (ClassifierControl.Shared.IsConnected)
            {
                ClassifierControl.Shared.DisConnectEjectDevice();
            }
            else
            {
                if (chkTcpConnect.Checked)
                {
                    string ip = txtEjectIp.Text.Trim();
                    int.TryParse(txtEjectPort.Text.Trim(), out int port);
                    if (string.IsNullOrEmpty(ip) || port == 0)
                    {
                        FormShowHelper.ShowMessage("请先输入IP和端口".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                    if (!ClassifierControl.Shared.ConnectEjectDevice(ip, port))
                    {
                        FormShowHelper.ShowMessage("气吹设备连接失败".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                }
                else
                {
                    string com = cmbCOM.SelectedItem != null ? (string)cmbCOM.SelectedItem : GlobalSettings.ApplySetting.EjectDeviceCom;
                    if (string.IsNullOrEmpty(com))
                    {
                        FormShowHelper.ShowMessage("请先选择COM口".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                    if (!ClassifierControl.Shared.ConnectEjectDevice(com))
                    {
                        FormShowHelper.ShowMessage("气吹设备连接失败".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                }
            }
            RefreshConnectCtrl();
        }

        private void chkTcpConnect_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTcpConnect.Checked)
            {
                ShowRow(3);
                ShowRow(4);
                HideRow(5);
            }
            else
            {
                HideRow(3);
                HideRow(4);
                ShowRow(5);
            }
        }
        // 显示行
        private void ShowRow(int rowIndex)
        {
            tableLayoutPanel1.RowStyles[rowIndex].Height = 30;
            switch (rowIndex)
            {
                case 3:
                    lblEjectIp.Visible = txtEjectIp.Visible = true;
                    break;
                case 4:
                    lblEjectPort.Visible = txtEjectPort.Visible = true;
                    break;
                case 5:
                    lblEjectCom.Visible = cmbCOM.Visible = true;
                    break;
            }
        }

        // 隐藏行
        private void HideRow(int rowIndex)
        {
            tableLayoutPanel1.RowStyles[rowIndex].Height = 0;
            switch (rowIndex)
            {
                case 3:
                    lblEjectIp.Visible = txtEjectIp.Visible = false;
                    break;
                case 4:
                    lblEjectPort.Visible = txtEjectPort.Visible = false;
                    break;
                case 5:
                    lblEjectCom.Visible = cmbCOM.Visible = false;
                    break;
            }
        }

        private void txtEjectPort_Validating(object sender, CancelEventArgs e)
        {
            string port = (sender as DevExpress.XtraEditors.TextEdit).Text.Trim();
            if (!string.IsNullOrEmpty(port) && !int.TryParse(port, out _))
            {
                e.Cancel = true;
            }
        }

        private void sortingBaseButton1_Click(object sender, EventArgs e)
        {
            //var EjectAirDelay = (uint)spinDelayTime.Value;
            //var EjectAirTime = (uint)spinBlowTime.Value;
            //ClassifierControl.Shared.Set(new int[] { 0,1,2,3},new int[] {(int) EjectAirDelay ,(int)EjectAirTime });
        }
    }
}
