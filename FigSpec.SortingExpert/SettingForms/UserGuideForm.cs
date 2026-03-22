using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.Tools;
using Globalization;
using ImgView;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WebAPI;
using WebAPI.Tables;

namespace FigSpec.SortingExpert.VersionUpdate
{
    public partial class UserGuideForm : BaseForm
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public UserGuideForm()
        {
            InitializeComponent();
        }

        private void UserGuideForm_Load(object sender, EventArgs e)
        {
            imageViewWithTools1.HideTool();
            imageViewWithTools1.OnPointMove += ImageViewWithTools1_OnPointMove;
            imageViewWithTools1.SetIntervalPoint(500);
            cmbGain.BindComboBoxItem(typeof(EnumGain).GetValueTextItems());
            cmbGain.SetSelectedValue<int>(GlobalSettings.ApplySetting.ScanCameraSetting.Gain);
            spExposureTime.EditValue = GlobalSettings.ApplySetting.ScanCameraSetting.ExposureTime;

            string[] files = ScanParaMeter.GetCameraSettingFiles();
            if (files.Length > 0)
            {
                fnlStep2.Enabled = true;
                lblStep1Result.Text = "相机配置已导入".ToMultiLanguage();
            }
            if (ScanParaMeter.camera != null)
            {
                Step2_ConnectCamera();
            }
        }

        private void ImageViewWithTools1_OnPointMove(PointF obj)
        {
            this.Invoke(new Action(() =>
            {
                lblLastPoint.Text = string.Format("坐标：({0}, {1})".ToMultiLanguage(), (int)obj.X, (int)obj.Y);
            }));
        }

        private void btnImportFile_Click(object sender, EventArgs e)
        {
            if (ScanParaMeter.ImportCameraSetting())
            {
                lblStep1Result.Text = "相机配置导入成功".ToMultiLanguage();
                //lblStep1Result.ForeColor = Color.Lime;
                fnlStep2.Enabled = true;
            }
            else
            {
                lblStep1Result.Text = "相机配置导入失败".ToMultiLanguage();
                //lblStep1Result.ForeColor = Color.Red;
            }
        }

        private void btnConnectCamera_Click(object sender, EventArgs e)
        {
            try
            {
                if (ScanParaMeter.camera == null)
                {
                    FormShowHelper.ShowLoadingForm(this, "相机连接中...".ToMultiLanguage(), second: 60, false);
                    ScanParaMeter.OpenCamera(true, null);
                    FormShowHelper.CloseLoadingForm();
                    if (ScanParaMeter.camera == null)
                    {
                        lblStep2Result.Text = "相机连接失败".ToMultiLanguage();
                        lblLinkFile.Visible = true;
                        return;
                    }
                }
                else
                {
                    FormShowHelper.ShowLoadingForm(this, "相机断开中...".ToMultiLanguage(), second: 60, false);
                    ScanParaMeter.camera.CloseCamera();
                    ScanParaMeter.camera = null;
                    FormShowHelper.CloseLoadingForm();
                }
            }
            catch (Exception)
            {
                //FormShowHelper.CloseLoadingForm();
            }
            Step2_ConnectCamera();
        }

        private void Step2_ConnectCamera()
        {
            if (ScanParaMeter.camera == null)
            {
                btnConnectCamera.Text = "连接相机".ToMultiLanguage();
                lblStep2Result.Text = "相机已断开".ToMultiLanguage();
                lblLinkFile.Visible = true;
            }
            else
            {
                btnConnectCamera.Text = "断开连接".ToMultiLanguage();
                lblStep2Result.Text = "相机已连接".ToMultiLanguage();
                fnlStep3.Enabled = true;
                pnlContent.Enabled = true;
                PrepareStep3();
            }
        }

        private void PrepareStep3()
        {
            if (ScanParaMeter.camera != null)
            {
                var cameraSetting = GlobalSettings.ApplySetting.ScanCameraSetting;
                cameraSetting.ExposureTime = (int)spExposureTime.Value;
                cameraSetting.Gain = cmbGain.GetSelectedValue<int>() ?? 0;
                ScanParaMeter.camera.SetExposureTime(cameraSetting.ExposureTime);
                ScanParaMeter.camera.SetGain(cameraSetting.Gain);
                ScanParaMeter.parminfo = ScanParaMeter.camera.GetParmInfo();
                ScanParaMeter.camera.SetFrameRate(ScanParaMeter.parminfo.FrameRateMax);
                lblMaxFrame.Text = $"{(int)ScanParaMeter.parminfo.FrameRateMax}"; 
            }
        }

        private void btnAdjust_Click(object sender, EventArgs e)
        {
            try
            {
                if (ScanParaMeter.camera == null)
                {
                    FormShowHelper.ShowMessage("请先连接相机", "提示".ToMultiLanguage());
                    return;
                }

                //正在采集数据中
                if (ScanParaMeter.camera.IsGrab)
                {
                    FormShowHelper.ShowLoadingForm(this, "正在停止...".ToMultiLanguage(), second: 30, false);
                    if (ScanParaMeter.camera != null && ScanParaMeter.camera.IsOpen)
                    {
                        ScanParaMeter.collectData?.StopGrab();
                    }
                    FormShowHelper.CloseLoadingForm();
                    updateButton(false);
                    return;
                }
                ScanParaMeter.ResetCameraInfo(true);
                FormShowHelper.ShowLoadingForm(this, "正在启动...".ToMultiLanguage(), second: 60, false);
                PrepareStep3();
                ResetTimer();
                imageViewWithTools1.imageView.AutoResize();
                int samples = (int)ScanParaMeter.parminfo.SpatialPixelNumCur;
                int lines = (int)(imageViewWithTools1.Width * 1.0f / imageViewWithTools1.Height * samples);
                ScanParaMeter.collectData.StartGrabUser(lines);
                FormShowHelper.CloseLoadingForm();
                updateButton(true);
  
            }
            catch (Exception ee)
            {
                LogHelper.WriteLog(ee.Message);
            }
        }

        private void updateButton(bool status = false)
        {
            if (status)
            {
                btnAdjust.Text = "停止调节清晰度和信号量".ToMultiLanguage();
                
            }
            else
            {
                btnAdjust.Text = "开始调节清晰度和信号量".ToMultiLanguage();
            }
            btnConnectCamera.Enabled = !status;

        }

        public void ResetTimer()
        {
            ScanParaMeter.ImgTimer = timer;
            timer.Interval = 20; // 设置定时器的时间间隔为1秒
            timer.Tick += Timer_Tick; // 订阅Timer的Tick事件
        }

        /// <summary>
        /// 定时刷新显示图像画面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            //实时刷新的图片
            imageViewWithTools1.SetImage(ScanParaMeter.collectData.GetBitmap());
            var claritys = ScanParaMeter.collectData.GetClarity();
            if (claritys != null)
            {
                lblClarity.Text = claritys[1].ToString("0.##");
                lblSignal.Text = ((int)claritys[0]).ToString();
                lblSignal.ForeColor = claritys[0] > 16000 * 0.9375f || claritys[0] < 16000 * 0.5f ? Color.Red : Color.White;
            }
        }

        private void btnSet2Device_Click(object sender, EventArgs e)
        {
            PrepareStep3();
        }

        private void UserGuideForm_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            try
            {
                GlobalSettings.ApplySetting.Save();
                imageViewWithTools1.OnPointMove -= ImageViewWithTools1_OnPointMove;
                if (ScanParaMeter.camera != null && ScanParaMeter.camera.IsGrab && ScanParaMeter.camera.IsOpen)
                {
                    ScanParaMeter.collectData?.StopGrab();
                }
                NotificationAction.SendStatus2Form?.Invoke(NoticeForm.NotOutlineSorting);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }

        }

        private void lblLinkFile_Click(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Application.StartupPath, @"Assets\Files\POCC.pdf");
            if (File.Exists(filePath))
            {
                FileTool.OpenPdf(filePath);
            }
        }
    }
}