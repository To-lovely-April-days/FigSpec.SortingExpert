using Cameras;
using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.ModelFiles;
using FigSpec.SortingExpert.Tools;
using FigSpec.Spectral.Extensions;
using Globalization;
using ImgView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class SplicingWhiteCalibrationForm : BaseForm
    {
        List<LineAreaInfo> lineAreaInfos = new List<LineAreaInfo>();
        ColorType calibrateType;    //校准类型
        Bitmap orginBitmap;     //原始图片
        int datatype;           //数据类型
        int samples;            //像素数
        int bands;              //波长
        int lineCount = 100;  //校准的帧数
        float[] averageData;  //平均数据


        public bool BlackDone { get; private set; }

        public bool WhiteDone { get; private set; }
        string whitePath;
        string blackPath;
        bool isScan;
        Model model;

        public SplicingWhiteCalibrationForm(bool isScan, ColorType calibrateType, string whitePath, string blackPath, Model model)
        {
            this.calibrateType = calibrateType;
            this.whitePath = whitePath;
            this.blackPath = blackPath;
            this.isScan = isScan;
            this.model = model;
            InitializeComponent();
        }

        private void SplicingWhiteCalibrationForm_Load(object sender, EventArgs e)
        {
            RefreshTitle();
            ScanParaMeter.ResetCameraInfo(isScan);
            if (!isScan)
            {
                if (!ScanParaMeter.SetRunTypeBank(model))
                {
                    FormShowHelper.ShowMessage("相机设置工作模式失败".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                else
                {
                    ScanParaMeter.RefreshFrameRate(false);
                    ScanParaMeter.RefreshCameraImgRGB(false);
                }
            }

            datatype = ScanParaMeter.parminfo.PixelFormatCur == EnumPixelFormat.Mono8 ? 1 : 12;
            bands = (int)ScanParaMeter.parminfo.SpectralChannelNumCur;
            samples = (int)ScanParaMeter.parminfo.SpatialPixelNumCur;
            averageData = new float[samples * bands];

            gcConfig.DataSource = lineAreaInfos;
            gcConfig.RefreshDataSource();
            imageViewWithTools1.HideTool();
            imageViewWithTools1.SetFreeDragXY(false);
            imageViewWithTools1.imageView.PointYEnabled = true;
            imageViewWithTools1.OnAddLineY += (XYLineEventArgs ee) => {
                ee.Cancel = true;
                if (orginBitmap == null || ScanParaMeter.collectData.CalibrateSPE == null)
                    return;
                int framenum = ScanParaMeter.collectData.CalibrateSPE.Hdr.Lines;
                int frameindex = (int)ee.AbsolutePoint.X;
  
                if (frameindex < 0 || frameindex > framenum)
                {
                    FormShowHelper.ShowMessage("超出图像范围".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                if (frameindex < lineCount / 2 || frameindex + lineCount / 2 > ScanParaMeter.collectData.CalibrateSPE.Lines)
                {
                    FormShowHelper.ShowMessage("校准线超出图像范围".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                if (lineAreaInfos.Exists(i => i.Frame == frameindex))
                {
                    return;
                }
                try
                {
                    var info = new LineAreaInfo()
                    { 
                        Frame = frameindex,
                        Color = ImgView.Helpers.RandColor(lineAreaInfos.Count + 1)
                    };
                    int startindex = frameindex - 50;
                    int endIndex = frameindex + 50;
                    int count = endIndex - startindex;

                    byte[] allBytes = new byte[samples * bands * lineCount * CorrectionHelper.SizeOfBand(datatype)];
                    int dstOffset = 0;
                    ScanParaMeter.collectData.CalibrateSPE.ForFrame(startindex, count, (int progress, byte[] nd) => {
                        Buffer.BlockCopy(nd, 0, allBytes, dstOffset, nd.Length);
                        dstOffset += nd.Length;
                    });

                    byte[] averageData = Calibration.GetFarmeData(allBytes, datatype, bands, samples, count);
                    info.FrameData = CorrectionHelper.LineByte2Float(averageData, datatype, samples, bands);
                    lineAreaInfos.Add(info);

                    RefreshControl();
                }
                catch (Exception)
                {
       
                }

            };
        }

        private void RefreshTitle()
        {
            if (calibrateType == ColorType.白)
            {
                this.Text = lblTitle.Text = "白校准".ToMultiLanguage();
                btnSaveAndSwitch.Text = "保存并切换至黑校准".ToMultiLanguage();
            }
            else
            {
                this.Text = lblTitle.Text = "黑校准".ToMultiLanguage();
                btnSaveAndSwitch.Text = "保存并切换至白校准".ToMultiLanguage();
            }
        }

        /// <summary>
        /// 绘制选择的区域
        /// </summary>
        private unsafe void RefreshImgView()
        {
            Bitmap bitmap = (Bitmap)orginBitmap.Clone();
            var nData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var nPtr = (byte*)nData.Scan0;

            if (lineAreaInfos.Count > 0)
            {
                foreach (var item in lineAreaInfos)
                {
                    int startIndex = item.Frame - lineCount / 2;
                    int endIndex = item.Frame + lineCount / 2;
                    Parallel.For(0, samples, y => {
                        for (int x = startIndex; x < endIndex; x++)
                        {
                            *(nPtr + y * nData.Stride + x * 3) = item.Color.B;
                            *(nPtr + y * nData.Stride + x * 3 + 1) = item.Color.G;
                            *(nPtr + y * nData.Stride + x * 3 + 2) = item.Color.R;
                        }

                    });

                }
            }
            bitmap.UnlockBits(nData);
            imageViewWithTools1.SetImage(bitmap, GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
        }

        private unsafe void RefreshImgResult()
        {
            if (lineAreaInfos.Count > 1)
            {
                int band = bands / 2;
                for (int y = 0; y < samples; y++)
                {
                    float compareFloat = calibrateType == ColorType.白 ? float.MinValue : float.MaxValue;
                    int index = 0;
                    //白校准中间一个波段，原始信号最大的那一帧的像素点的原始信号，黑校准则取最小的
                    for (int i = 0; i < lineAreaInfos.Count; i++)
                    {
                        var temp = lineAreaInfos[i].FrameData[band * samples + y];
                        if (calibrateType == ColorType.白)
                        {
                            if (temp > compareFloat)
                            {
                                compareFloat = temp;
                                index = i;
                            }
                        }
                        else
                        {
                            if (temp < compareFloat)
                            {
                                compareFloat = temp;
                                index = i;
                            }
                        }
                        
                    }

                    for (int b = 0; b < bands; b++)
                    {
                        averageData[b * samples + y] = lineAreaInfos[index].FrameData[b * samples + y];
                    }
                }
            }
            else if (lineAreaInfos.Count == 1)
            {
                Array.Copy(lineAreaInfos[0].FrameData, 0, averageData, 0, averageData.Length);
            }
            else
            {
                Array.Clear(averageData, 0, averageData.Length);
                picResult.Image = null;
                return;
            }
            var set = isScan ? GlobalSettings.ApplySetting.ScanCameraSetting : GlobalSettings.ApplySetting.SortCameraSetting;
            float tr = GlobalSettings.ApplySetting.ScanCameraSetting.ImgOrginThresholdR;
            float tg = GlobalSettings.ApplySetting.ScanCameraSetting.ImgOrginThresholdG;
            float tb = GlobalSettings.ApplySetting.ScanCameraSetting.ImgOrginThresholdB;
            var imgrgb = CorrectionHelper.Line2BGR(averageData, samples, (set.ImgR, set.ImgG, set.ImgB), (tr, tg, tb));


            int customSamples = GlobalSettings.ApplySetting.CustomSamples;
            int width = (int)(1.0f * picResult.Width / picResult.Height * customSamples);
            Bitmap bitmap = new Bitmap(width, customSamples, PixelFormat.Format24bppRgb);
            var nData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var nPtr = (byte*)nData.Scan0;
            int ssIndex = GlobalSettings.ApplySetting.StartSampleIndex;
            Parallel.For(0, customSamples, y => {
                for (int x = 0; x < width; x++)
                {
                    *(nPtr + y * nData.Stride + x * 3) = imgrgb[(y + ssIndex) * 3];
                    *(nPtr + y * nData.Stride + x * 3 + 1) = imgrgb[(y + ssIndex) * 3 + 1];
                    *(nPtr + y * nData.Stride + x * 3 + 2) = imgrgb[(y + ssIndex) * 3 + 2];
                }
            });

            bitmap.UnlockBits(nData);
            picResult.Image = bitmap;
        }

        private void RefreshControl()
        {
            RefreshImgView();
            RefreshImgResult();
            gcConfig.RefreshDataSource();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var rows = gvConfig.GetSelectedRows();
            if (rows.Length == 0)
            {
                FormShowHelper.ShowMessage("请选择行".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            gvConfig.DeleteSelectedRows();
            RefreshControl();
            
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (lineAreaInfos.Count == 0)
            {
                FormShowHelper.ShowMessage("请先选择校准区域".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            SaveCalibrateData();
            this.DialogResult = DialogResult.OK;
        }

        private void SaveCalibrateData()
        {
            FormShowHelper.ShowLoadingForm(this, "保存中".ToMultiLanguage() + "...");

            if (calibrateType == ColorType.白)
            {
                string filepath = string.IsNullOrEmpty(whitePath) ? GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRawFilePath : whitePath;
                var k_White = WaveLength.DesignatedWavelengthSignal(ScanParaMeter.tupleWhiteRef, ScanParaMeter.parminfo.SpectralChannelWavelength);
                CorrectionHelper.SaveFile(ScanParaMeter.collectData.CalibrateSPE, 1, filepath, averageData, ScanParaMeter.parminfo.SpectralChannelWavelength, k_White);
                Thread.Sleep(200);

                if (isScan)
                {
                    if (ScanParaMeter.ScanCorrectionData == null)
                        ScanParaMeter.ScanCorrectionData = new CorrectionData();
                    ScanParaMeter.ScanCorrectionData.k_White = k_White;
                    ScanParaMeter.ScanCorrectionData.White_ReadBytes = CorrectionHelper.GetCorrectionData(filepath).Signal;
                    ScanParaMeter.ScanCorrectionData.White_FrameRate = GlobalSettings.ApplySetting.ScanCameraSetting.FrameRate;
                    ScanParaMeter.ScanCorrectionData.White_ExposureTime = GlobalSettings.ApplySetting.ScanCameraSetting.ExposureTime;
                }
                else
                {
                    if (ScanParaMeter.SortCorrectionData == null)
                        ScanParaMeter.SortCorrectionData = new CorrectionData();
                    ScanParaMeter.SortCorrectionData.k_White = k_White;
                    ScanParaMeter.SortCorrectionData.White_ReadBytes = CorrectionHelper.GetCorrectionData(filepath).Signal;
                    ScanParaMeter.SortCorrectionData.White_FrameRate = GlobalSettings.ApplySetting.SortCameraSetting.FrameRate;
                    ScanParaMeter.SortCorrectionData.White_ExposureTime = GlobalSettings.ApplySetting.SortCameraSetting.ExposureTime;
                }

                WhiteDone = true;
            }
            else
            {
                string filepath = string.IsNullOrEmpty(blackPath) ? GlobalSettings.ApplySetting.scanSet.calibrationInfo.blackRawFilePath : blackPath;
                var k_Black = WaveLength.DesignatedWavelengthSignal(ScanParaMeter.tupleBlackRef, ScanParaMeter.parminfo.SpectralChannelWavelength);
                CorrectionHelper.SaveFile(ScanParaMeter.collectData.CalibrateSPE, 0, filepath, averageData, ScanParaMeter.parminfo.SpectralChannelWavelength, k_Black);
                Thread.Sleep(200);

                if (isScan)
                {
                    if (ScanParaMeter.ScanCorrectionData == null)
                        ScanParaMeter.ScanCorrectionData = new CorrectionData();
                    ScanParaMeter.ScanCorrectionData.k_Black = k_Black;
                    ScanParaMeter.ScanCorrectionData.Black_ReadBytes = CorrectionHelper.GetCorrectionData(filepath).Signal;
                    ScanParaMeter.ScanCorrectionData.Black_FrameRate = GlobalSettings.ApplySetting.ScanCameraSetting.FrameRate;
                    ScanParaMeter.ScanCorrectionData.Black_ExposureTime = GlobalSettings.ApplySetting.ScanCameraSetting.ExposureTime;
                }
                else
                {
                    if (ScanParaMeter.SortCorrectionData == null)
                        ScanParaMeter.SortCorrectionData = new CorrectionData();
                    ScanParaMeter.SortCorrectionData.k_Black = k_Black;
                    ScanParaMeter.SortCorrectionData.Black_ReadBytes = CorrectionHelper.GetCorrectionData(filepath).Signal;
                    ScanParaMeter.SortCorrectionData.Black_FrameRate = GlobalSettings.ApplySetting.SortCameraSetting.FrameRate;
                    ScanParaMeter.SortCorrectionData.Black_ExposureTime = GlobalSettings.ApplySetting.SortCameraSetting.ExposureTime;
                }
                BlackDone = true;
            }

            FormShowHelper.CloseLoadingForm();
        }

        private void btnSaveAndSwitch_Click(object sender, EventArgs e)
        {
            if (lineAreaInfos.Count == 0)
            {
                FormShowHelper.ShowMessage("请先选择校准区域".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            SaveCalibrateData();
            lineAreaInfos.Clear();
            calibrateType = calibrateType == ColorType.白 ? ColorType.黑 : ColorType.白;
            RefreshControl();
            RefreshTitle();
        }

        private void gvConfig_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            if (e.Column == colColor)
            {
                e.Appearance.BackColor = (gvConfig.GetRow(e.RowHandle) as LineAreaInfo).Color;
            }
        }

        private void updateButton(bool status = false)
        {
            if (status)
            {
                btnStartScan.Text = "结束采集".ToMultiLanguage();
            }
            else
            {
                btnStartScan.Text = "开始采集".ToMultiLanguage();
                btnPause.Text = "暂停".ToMultiLanguage();
            }
            btnPause.Enabled = status;
            imageViewWithTools1.Enabled = !status;
            btnDelete.Enabled = !status;
            btnSaveAndSwitch.Enabled = !status;
            btnOkAndClose.Enabled = !status;
        }

        private void btnStartScan_Click(object sender, EventArgs e)
        {
            try
            {
                var appset = GlobalSettings.ApplySetting;

                //暂停时点击停止，先启动
                if (ScanParaMeter.collectData.SuspendFlag)
                {
                    btnPause_Click(null, null);
                }


                //正在采集数据中
                if (ScanParaMeter.camera.IsGrab)
                {
                    if (ScanParaMeter.camera != null && ScanParaMeter.camera.IsOpen)
                    {
                        ScanParaMeter.collectData?.StopGrab();
                    }
                    FormShowHelper.ShowLoadingForm(this, "正在停止...".ToMultiLanguage(), second: 60 * 30, true);
                    if (ScanParaMeter.IsSavingImage)
                    {
                        FormShowHelper.ShowLoadingForm(this, "正在将采集到的图像写入磁盘".ToMultiLanguage(), 100, true);
                        while (ScanParaMeter.IsSavingImage)
                        {
                            Thread.Sleep(200);
                        }
                        FormShowHelper.CloseLoadingForm();
                    }
                    Thread thread = new Thread(() => {
                        try
                        {
                            while (true)
                            {
                                if (string.IsNullOrEmpty(GlobalSettings.ApplySetting.runingApp))
                                {
                                    this.Invoke(new Action(() => {
                                        FormShowHelper.CloseLoadingForm();
                                        updateButton();
                                    }));
                                    break;
                                }
                                Thread.Sleep(500);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    });
                    // 启动线程
                    thread.Start();
                    return;
                }
               
                //分选功能是否正在被训练模块使用
                if (ScanParaMeter.camera == null)
                {
                    FormShowHelper.ShowMessage("请先连接相机", "提示".ToMultiLanguage());
                    return;
                }

                if (!CalibrationInfo.TryLoadWhiteRefFile(GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRefFilePath, out string result))
                {
                    FormShowHelper.ShowMessage(result, "提示".ToMultiLanguage());
                    return;
                }

                if (ScanParaMeter.camera != null)
                {
                   
                    if (lineAreaInfos.Count > 0)
                    {
                        lineAreaInfos.Clear();
                        RefreshControl();
                    }
                    ScanParaMeter.collectData.DataSaveComplated = SaveLastImage;

                    FormShowHelper.ShowLoadingForm(this, "正在启动...".ToMultiLanguage(), second: 60 * 30, true);
                    updateButton(true);
                    FormShowHelper.CloseLoadingForm();
                    ResetTimer();
                    imageViewWithTools1.imageView.AutoResize();
                    ScanParaMeter.collectData.StartGrab(true, isScan);
                }

            }
            catch (Exception ee)
            {

            }
        }

        
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

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
            imageViewWithTools1.SetImage(ScanParaMeter.collectData.GetBitmap(), GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
        }

        private void SaveLastImage()
        {
            if (!IsHandleCreated)
                return;
            this.Invoke(new Action(() => {

                var cameraSetting = isScan ? GlobalSettings.ApplySetting.ScanCameraSetting : GlobalSettings.ApplySetting.SortCameraSetting;
                float tr = GlobalSettings.ApplySetting.ScanCameraSetting.ImgOrginThresholdR;
                float tg = GlobalSettings.ApplySetting.ScanCameraSetting.ImgOrginThresholdG;
                float tb = GlobalSettings.ApplySetting.ScanCameraSetting.ImgOrginThresholdB;
                ScanParaMeter.collectData.spectraOfSPE.ToBGR(ScanParaMeter.collectData.CalibrateSPE, (cameraSetting.ImgR, cameraSetting.ImgG, cameraSetting.ImgB), (tr, tg, tb), (byte[] nd, int i, int total, bool complete) => {
                    //img = nd;
                    var progress = i / (float)total * 100f;
                    if (progress >= 100)
                    {
                        orginBitmap = nd.ToBitmap(ScanParaMeter.collectData.CalibrateSPE.Lines, ScanParaMeter.collectData.CalibrateSPE.Samples);
                    }
                });
                imageViewWithTools1.SetImage((Bitmap)orginBitmap.Clone(), GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
            }));
        }


        private void btnPause_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ScanParaMeter.collectData.SuspendFlag)
                {
                    ScanParaMeter.collectData.SuspendScan();
                    updatePauseButton();
                }
                else
                {
                    ScanParaMeter.collectData.ResumeScan();
                    updatePauseButton(true);
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void updatePauseButton(bool pause = false)
        {
            if (pause)
            {
                btnPause.Text = "暂停".ToMultiLanguage();
            }
            else
            {
                btnPause.Text = "继续".ToMultiLanguage();
            }
        }
        private void SplicingWhiteCalibrationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (ScanParaMeter.camera != null && ScanParaMeter.camera.IsGrab && ScanParaMeter.camera.IsOpen)
            {
                ScanParaMeter.collectData?.StopGrab();
            }
            ScanParaMeter.collectData.CalibrateSPE.SpeDispose();
        }

        private void btnCameraSetting_Click(object sender, EventArgs e)
        {
            if (ScanParaMeter.camera != null)
            {
                CameraSettingForm cameraSettingForm = new CameraSettingForm(isScan, true, model);
                FormShowHelper.ShowDialog(cameraSettingForm);
                NotificationAction.RefreshScanFrame?.Invoke();
            }
            else
            {
                FormShowHelper.ShowMessage("请先连接相机".ToMultiLanguage(), "提示".ToMultiLanguage());
            }
        }
    }


    public class LineAreaInfo
    {
        public int Frame { get; set; }

        public Color Color { get; set; }

        public float[] FrameData { get; set; }

    }

}