using DevExpress.XtraCharts;
using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Cuda;
using FigSpec.SortingExpert.DataToCPP;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.ModelFiles;
using FigSpec.SortingExpert.SettingForms;
using FigSpec.SortingExpert.Tools;
using Globalization;
using ImgView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace FigSpec.SortingExpert
{
    public partial class SortingForm : BaseFormInside
    {
        public Action<Model> UpdateModel;

        private Model model;

        //private List<ICameraInfo> cameraInfos = new List<ICameraInfo>();//相机参数列表

        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        /// <summary>
        /// 是否轮廓分选
        /// </summary>
        public bool IsOutlineSorting { get; set; } = false;


        public SortingForm()
        {
            InitializeComponent();

            NotificationAction.SendStatus2Form += (type) =>
            {
                if (type == NoticeForm.NotSorting && !IsOutlineSorting)
                    return;
                if (type == NoticeForm.NotOutlineSorting && IsOutlineSorting)
                    return;
                updateButton();
            };
            var appset = GlobalSettings.ApplySetting;
            UpdateModel = (data) =>
            {
                model = data;
                if (data == null)
                {
                    labModelInfo.Text = "";
                }
                else
                {
                    appset.SortCameraSetting.CameraSN = data.Sn;
                    appset.SortCameraSetting.ExposureTime = data.ExpTime;
                    appset.SortCameraSetting.Gain = data.Gain;
                    appset.SortCameraSetting.ImgR = data.ImgR;
                    appset.SortCameraSetting.ImgG = data.ImgG;
                    appset.SortCameraSetting.ImgB = data.ImgB;
                    appset.SortCameraSetting.ImgFR = data.ImgFR;
                    appset.SortCameraSetting.ImgFG = data.ImgFG;
                    appset.SortCameraSetting.ImgFB = data.ImgFB;
                    appset.SortCameraSetting.ImgThresholdR = data.ImgThresholdR;
                    appset.SortCameraSetting.ImgThresholdG = data.ImgThresholdG;
                    appset.SortCameraSetting.ImgThresholdB = data.ImgThresholdB;
                    labModelInfo.Text = "模型信息：".ToMultiLanguage() + data.name;


                }
                updateFrameRate(false);
                GlobalSettings.ApplySetting = appset;
            };
            if (model != null)
            {
                labModelInfo.Text = "模型信息：".ToMultiLanguage() + model.name;
            }

        }
        public void ResetTimer()
        {
            ScanParaMeter.ImgTimer = timer;
            timer.Interval = 50; // 设置定时器的时间间隔为1秒
            timer.Tick += Timer_Tick; // 订阅Timer的Tick事件
        }
        /// <summary>
        /// 定时刷新显示图像画面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            if (GlobalSettings.CurrentFormName == this.Name)
            {
                imageViewWithTools1.SetImage(ScanParaMeter.collectData.GetBitmap(), GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
            }
            //stopwatch.Stop();
            //Console.Write($"-----刷新一次图像的时间 {stopwatch.ElapsedMilliseconds}毫秒\r\n");
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            FormShowHelper.ShowDialog(new SortingConnectEject());
        }

        /// <summary>
        /// 分配GPU运算资源
        /// </summary>
        private void AllocCudaData()
        {
            try
            {
                var appset = GlobalSettings.ApplySetting;

                int Samples = (int)ScanParaMeter.parminfo.SpatialPixelNumCur;
                int bands = (int)ScanParaMeter.parminfo.SpectralChannelNumCur;

                // 读取黑白黑校准数据,

                CudaAccelerator.Shared.AllocCALRefWhiteBlack(ScanParaMeter.SortCorrectionData.White_ReadBytes, ScanParaMeter.SortCorrectionData.Black_ReadBytes);

                CudaAccelerator.Shared.AllocCALRef(ScanParaMeter.SortCorrectionData.k_White);
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// 启动分选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private async void btnStartSorting_Click(object sender, EventArgs e)
        {
            try
            {
                var appset = GlobalSettings.ApplySetting;
                //分选功能是否正在被训练模块使用
                if (GlobalSettings.ApplySetting.runingApp == "sort" && GlobalSettings.ApplySetting.stopAppFlag == false)
                {
                    GlobalSettings.ApplySetting.stopAppFlag = true;
                    if (ScanParaMeter.camera != null && ScanParaMeter.camera.IsOpen)
                    {
                        ScanParaMeter.collectData?.StopGrab();
                    }
                    //ScanParaMeter.client.Close();

                    FormShowHelper.ShowLoadingForm(this, "正在停止分选...".ToMultiLanguage(), second: 60 * 30, true);

                    btn_DeleteAll_Click(null, null);

                    if (GlobalSettings.ApplySetting.CommunicationType == 0 && ScanParaMeter.client.isConnect)
                    {
                        ScanParaMeter.client.Close();
                    }

                    Thread thread = new Thread(() =>
                    {
                        int count = 0;
                        while (true)
                        {
                            count++;
                            if (string.IsNullOrEmpty(GlobalSettings.ApplySetting.runingApp) || count == 20)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    FormShowHelper.CloseLoadingForm();
                                    updateButton();
                                }));
                                break;
                            }
                            Thread.Sleep(500);
                        }
                    });
                    // 启动线程
                    thread.Start();
                    return;
                }
                //分选功能是否正在被分选模块使用
                else if (!string.IsNullOrEmpty(GlobalSettings.ApplySetting.runingApp))
                {
                    FormShowHelper.ShowMessage("相机或GPU正在被使用".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }

                if (model == null)
                {
                    FormShowHelper.ShowMessage("缺少模型文件".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                if (!string.IsNullOrEmpty(GlobalSettings.ApplySetting.runingApp))
                {
                    FormShowHelper.ShowMessage("分选模块正在被使用".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                if (ScanParaMeter.camera == null)
                {
                    FormShowHelper.ShowLoadingForm(this, "相机连接中...".ToMultiLanguage(), second: 60, true);
                    ScanParaMeter.OpenCamera(false, (string res) =>
                    {
                        if (!string.IsNullOrEmpty(res))
                        {
                            FormShowHelper.ShowMessage(res.ToMultiLanguage(), "提示".ToMultiLanguage());
                        }
                    });
                    FormShowHelper.CloseLoadingForm();
                }
                if (ScanParaMeter.camera != null)
                {
                    if (model.Sn != ScanParaMeter.camera.Info.InstrumentSN)
                    {
                        FormShowHelper.ShowMessage("模型的序列号与相机的不匹配".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                    ScanParaMeter.ResetCameraInfo(false);
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

                    //string pathname = Path.GetFileNameWithoutExtension(model.hdr_spe_path);
                    if (!CalibrationInfo.LoadCalibrationFiles(model: model))
                    {
                        return;
                    }

                    if (ScanParaMeter.SortCorrectionData.k_White.Length != ScanParaMeter.parminfo.SpectralChannelNumCur
                        || ScanParaMeter.SortCorrectionData.Black_ReadBytes.Length != ScanParaMeter.parminfo.SpectralChannelNumCur * ScanParaMeter.parminfo.SpatialPixelNumCur
                        || ScanParaMeter.SortCorrectionData.White_ReadBytes.Length != ScanParaMeter.SortCorrectionData.Black_ReadBytes.Length)
                    {
                        FormShowHelper.ShowMessage("相机ROI已修改，请重新进行校准".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }

                    if (ScanParaMeter.SortCorrectionData.White_FrameRate != appset.SortCameraSetting.FrameRate
                        || ScanParaMeter.SortCorrectionData.Black_FrameRate != appset.SortCameraSetting.FrameRate
                        || ScanParaMeter.SortCorrectionData.White_ExposureTime != appset.SortCameraSetting.ExposureTime
                        || ScanParaMeter.SortCorrectionData.Black_ExposureTime != appset.SortCameraSetting.ExposureTime)
                    {
                        FormShowHelper.ShowMessage("帧频或曝光时间已修改，请重新进行校准".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }


                    updateFrameRate();
                    Dictionary<int, int> labels = new Dictionary<int, int>();
                    foreach (var item in model.classes)
                        labels.Add(item.id, item.color);
                    if (GlobalSettings.ApplySetting.CommunicationType == 0)
                    {
                        if (ScanParaMeter.client.isConnect == false)
                        {
                            if (!await ScanParaMeter.client.Connect(appset.ServerIp, int.Parse(appset.ServerPort)))
                            {
                                FormShowHelper.ShowMessage("吹气设备连接失败".ToMultiLanguage(), "提示".ToMultiLanguage());
                            }
                        }
                        ScanParaMeter.client.SendSetting(appset.SortCameraSetting.FrameRate, (int)ScanParaMeter.parminfo.SpatialPixelNumCur, labels);
                    }
                    else
                    {
                        NotificationAction.SendEjectSetting?.Invoke(appset.SortCameraSetting.FrameRate, (int)ScanParaMeter.parminfo.SpatialPixelNumCur, labels);
                    }

                    FormShowHelper.ShowLoadingForm(this, "正在启动分选...".ToMultiLanguage(), second: 60 * 30, true);
                    AllocCudaData();
                    updateButton(true);
                    NotificationAction.SendStatus2Form?.Invoke(IsOutlineSorting ? NoticeForm.NotOutlineSorting : NoticeForm.NotSorting);
                    FormShowHelper.CloseLoadingForm();
                    ResetTimer();
                    ScanParaMeter.collectData.StartGrab(model, IsOutlineSorting);

                }
            }
            catch (Exception ee)
            {

            }
        }

        private void updateButton(bool status = false)
        {
            if (!this.IsHandleCreated)
                return;
            if (status)
            {
                btnConnectCamera.Enabled = false;
                btnStartSorting.Text = "结束分选".ToMultiLanguage();
                btnEjectSetting.Enabled = false;
                btnCameraSetting.Enabled = false;
                btnWhiteCalibration.Enabled = btnBlackCalibration.Enabled = false;
            }
            else
            {
                btnConnectCamera.Enabled = true;
                btnStartSorting.Text = "开始分选".ToMultiLanguage();
                btnEjectSetting.Enabled = true;
                btnCameraSetting.Enabled = true;
                btnWhiteCalibration.Enabled = btnBlackCalibration.Enabled = true;

            }

            if (ScanParaMeter.camera == null)
            {
                btnConnectCamera.Text = "连接相机".ToMultiLanguage();
                btnrunfastsorting.Enabled = true;
            }
            else
            {
                btnrunfastsorting.Enabled = false;
                btnConnectCamera.Text = "断开连接".ToMultiLanguage();
            }

        }

        private void updateFrameRate(bool isShow = true)
        {
            var ass = GlobalSettings.ApplySetting;
            if (ScanParaMeter.camera != null && isShow)
            {
                labFrameRate.Text = "当前帧率：".ToMultiLanguage() + $"{ass.SortCameraSetting.FrameRate}fps";
            }
            else
            {
                labFrameRate.Text = "当前帧率：".ToMultiLanguage() + "--";
            }
        }
        private void btnCamera_Click(object sender, EventArgs e)
        {
            if (ScanParaMeter.camera != null)
            {
                if (model == null)
                {
                    FormShowHelper.ShowMessage("缺少模型文件".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
                CameraSettingForm cameraSettingForm = new CameraSettingForm(false, false, model);
                FormShowHelper.ShowDialog(cameraSettingForm);
                updateFrameRate();
            }
            else
            {
                FormShowHelper.ShowMessage("请先连接相机".ToMultiLanguage(), "提示".ToMultiLanguage());
            }
        }


        private void SortingForm_Load(object sender, EventArgs e)
        {
            LoadEvent();
            updateFrameRate();
            updateButton();
        }

        /// <summary>
        /// 校准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCalibration_Click(object sender, EventArgs e)
        {
            CalibrationClick(ColorType.黑);
        }

        private void btnWhiteCalibration_Click(object sender, EventArgs e)
        {
            CalibrationClick(ColorType.白);
        }

        private void CalibrationClick(ColorType colorType)
        {
            if (ScanParaMeter.camera == null)
            {
                FormShowHelper.ShowMessage("请先连接相机".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            if (ScanParaMeter.camera.IsGrab)
            {
                FormShowHelper.ShowMessage("请先停止正在运行的采集功能".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            if (model == null)
            {
                FormShowHelper.ShowMessage("缺少模型文件".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            string whiteRawFilePath = CalibrationInfo.GetWhiteRawFilePath(model);
            string blackRawFilePath = CalibrationInfo.GetBlackRawFilePath(model);
            var form = new SplicingWhiteCalibrationForm(false, colorType, whiteRawFilePath, blackRawFilePath, model);
            FormShowHelper.ShowDialog(form);
        }

        private void btnConnectCamera_Click(object sender, EventArgs e)
        {
            try
            {
                var appset = GlobalSettings.ApplySetting;

                if (ScanParaMeter.camera == null)
                {
                    FormShowHelper.ShowLoadingForm(this, "相机连接中...".ToMultiLanguage(), second: 60, true);
                    ScanParaMeter.OpenCamera(false, (string res) =>
                    {
                        if (!string.IsNullOrEmpty(res))
                        {
                            FormShowHelper.ShowMessage(res.ToMultiLanguage(), "提示".ToMultiLanguage());
                        }
                    });
                    if (ScanParaMeter.camera == null)
                    {
                        FormShowHelper.ShowMessage("相机连接失败".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }

                    if (model != null && model.Sn != ScanParaMeter.camera.Info.InstrumentSN)
                    {
                        UpdateModel(null);
                    }

                    updateFrameRate();
                    FormShowHelper.ShowLoadingForm(this, "相机连接成功".ToMultiLanguage(), second: 2, true);
                }
                else
                {
                    if (ScanParaMeter.camera.IsGrab)
                    {
                        FormShowHelper.ShowMessage("相机或GPU正在被使用".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                    FormShowHelper.ShowLoadingForm(this, "相机断开中...".ToMultiLanguage(), second: 60, true);
                    ScanParaMeter.camera.CloseCamera();
                    ScanParaMeter.camera = null;
                    updateFrameRate();
                    FormShowHelper.ShowLoadingForm(this, "相机已经断开".ToMultiLanguage(), second: 60, true);

                }
                updateButton();
                NotificationAction.SendStatus2Form?.Invoke(IsOutlineSorting ? NoticeForm.NotOutlineSorting : NoticeForm.NotSorting);
                FormShowHelper.CloseLoadingForm();
            }
            catch (Exception)
            {
                FormShowHelper.CloseLoadingForm();
            }
        }

        #region 光谱图相关方法

        /// <summary>
        /// 曲线数据
        /// </summary>
        List<SeriesData> seriesDatas = new List<SeriesData>();
        /// <summary>
        /// 当前波长
        /// </summary>
        float[] currentWaveLengths = null;
        /// <summary>
        /// 起始波长
        /// </summary>
        private int waveLengthStartIndex => cboStartWaveLength.GetSelectedValue<int>() ?? -1;
        /// <summary>
        /// 结束波长
        /// </summary>
        private int waveLengthEndIndex => cboEndWaveLength.GetSelectedValue<int>() ?? -1;

        bool waveLengthChangedCancel = false;

        private void LoadWaveLength()
        {
            if (dockPanel1.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Hidden)
                return;
            if (ScanParaMeter.camera != null)
            {
                if (!Common.FloatArrayEqual(currentWaveLengths, ScanParaMeter.parminfo.SpectralChannelWavelength))
                {
                    currentWaveLengths = (float[])ScanParaMeter.parminfo.SpectralChannelWavelength.Clone();
                    var items = currentWaveLengths.Select((x, index) => new ValueTextItem<int>(index, $"{index}: {x.ToString("F2")}")).ToList();
                    waveLengthChangedCancel = true;
                    cboStartWaveLength.BindComboBoxItem(items);
                    cboEndWaveLength.BindComboBoxItem(items);
                    cboEndWaveLength.SelectedIndex = currentWaveLengths.Length - 1;
                    waveLengthChangedCancel = false;
                }
            }
        }

        private void LoadEvent()
        {
            //LoadWaveLength();

            imageViewWithTools1.imageView.AutoResize();

            gridControl_GuangPu.DataSource = seriesDatas;
            gridControl_GuangPu.RefreshDataSource();


            if (IsOutlineSorting)
            {
                //dockPanel1.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
                imageViewWithTools1.SetToolButtons(new ToolButton[] { ToolButton.Move, ToolButton.ZoomIn, ToolButton.ZoomOut });//ToolButton.Move, ToolButton.ZoomIn, ToolButton.ZoomOut
            }
            else
            {
                imageViewWithTools1.SetToolButtons(new ToolButton[] { ToolButton.Move, ToolButton.ZoomIn, ToolButton.ZoomOut });//ToolButton.Move, ToolButton.ZoomIn, ToolButton.ZoomOut, ToolButton.Rectangle, ToolButton.Point
                imageViewWithTools1.SetFreeDragXY(false);
            }

            imageViewWithTools1.AddSelectionEvent += (SelectionType type, object value) =>
            {
                bool IsLegal = true;
                if (ScanParaMeter.collectData.spe == null && IsLegal)
                {
                    FormShowHelper.ShowMessage("无图像".ToMultiLanguage(), "提示".ToMultiLanguage());
                    IsLegal = false;
                }
                switch (type)
                {
                    case SelectionType.矩形:
                        var ee_rectangle = value as RectangleEventArgs;
                        if (!IsLegal)
                        {
                            ee_rectangle.Cancel = true;
                            return;
                        }
                        if (ee_rectangle.AbsoluteRectangle.Width <= 1 || ee_rectangle.AbsoluteRectangle.Height <= 1)
                        {
                            FormShowHelper.ShowMessage("所选区域需要完全覆盖一个像素点".ToMultiLanguage(), "提示".ToMultiLanguage());
                            ee_rectangle.Cancel = true;
                            return;
                        }
                        float[] avg = new float[ScanParaMeter.collectData.spe.Bands];
                        avg = ScanParaMeter.collectData.spectraOfSPE.GetAverageData(ScanParaMeter.collectData.spe, ee_rectangle.AbsoluteRectangle);
                        //if (!cb_ReflectanceDisplay.Checked)
                        //{

                        //}
                        //else
                        //{
                        //    var n = 0;
                        //    //计算选择区域校准后平均反射率 
                        //    ScanParaMeter.collectData.spe.ParallelFor(ee_rectangle.AbsoluteRectangle, (int x, int y, float progress, float[] values) => {
                        //        lock (avg)
                        //        {
                        //            for (int i = 0; i < avg.Length; i++)
                        //            {
                        //                float cdata = 0;
                        //                float needOri = values[i];
                        //                float needRefWhiteK = ScanParaMeter.k_White[i];
                        //                float needWhite = ScanParaMeter.White_ReadBytes[y + i * ScanParaMeter.collectData.spe.Samples];
                        //                float needRefBlackK = ScanParaMeter.k_Black[i];
                        //                float needBlack = ScanParaMeter.Black_ReadBytes[y + i * ScanParaMeter.collectData.spe.Samples];
                        //                if (needOri - needBlack == 0 || needWhite - needBlack == 0 || needRefWhiteK == 0)
                        //                {
                        //                    cdata = 0;
                        //                }
                        //                else
                        //                {
                        //                    cdata = (needOri - needBlack) / (needWhite - needBlack) * (needRefWhiteK - needRefBlackK);
                        //                }
                        //                avg[i] += cdata;
                        //            }
                        //            n++;
                        //        }
                        //    });
                        //    for (int i = 0; i < avg.Length; i++)
                        //    {
                        //        avg[i] /= n;
                        //        avg[i] = avg[i] > 2 ? 2 : avg[i];
                        //    }
                        //}

                        var model = new SeriesData()
                        {
                            SeriesColor = ee_rectangle.SelectionColor,
                            AreaType = AreaType.Rectangle,
                            AreaGuid = ee_rectangle.Guid,
                            PointX = ee_rectangle.AbsoluteRectangle.X,
                            PointY = ee_rectangle.AbsoluteRectangle.Y,
                            SpectralData = avg,
                        };
                        seriesDatas.Add(model);
                        PointToLineChart(model);
                        break;
                    case SelectionType.点:
                        var ee_point = value as PointEventArgs;
                        if (!IsLegal)
                        {
                            ee_point.Cancel = true;
                            return;
                        }
                        if (ee_point.AbsolutePoint.X > ScanParaMeter.collectData.spe.Lines || ee_point.AbsolutePoint.X < 0 || ee_point.AbsolutePoint.Y > ScanParaMeter.collectData.spe.Samples || ee_point.AbsolutePoint.Y < 0)
                        {
                            FormShowHelper.ShowMessage("超出当前数据范围".ToMultiLanguage(), "提示".ToMultiLanguage());
                            ee_point.Cancel = true;
                            return;
                        }
                        if (seriesDatas.Exists(s => s.PointX == ee_point.AbsolutePoint.X && s.PointY == ee_point.AbsolutePoint.Y && s.AreaType == AreaType.Point))
                        {
                            FormShowHelper.ShowMessage("当前点的光谱数据已存在".ToMultiLanguage(), "提示".ToMultiLanguage());
                            ee_point.Cancel = true;
                            return;
                        }
                        float[] result2 = ScanParaMeter.collectData.spe[ee_point.AbsolutePoint];
                        //if (cb_ReflectanceDisplay.Checked)
                        //{
                        //    int samples = (int)ScanParaMeter.parminfo.SpatialPixelNumCur;
                        //    for (int i = 0; i < result2.Length; i++)
                        //    {
                        //        int trueBand = i + this.model.StartBandIndex;   //有效波长，0起始
                        //        int sample = (int)ee_point.AbsolutePoint.Y;

                        //        int refIndex = sample + trueBand * samples;
                        //        int tempIndex = sample + i * samples;
                        //        float needOri = result2[i];
                        //        float needRefK = ScanParaMeter.k_White[trueBand];
                        //        float needWhite = ScanParaMeter.White_ReadBytes[refIndex];
                        //        float needBlack = ScanParaMeter.Black_ReadBytes[refIndex];
                        //        if (needWhite <= needBlack || needOri <= needBlack)
                        //        {
                        //            needOri = 0;
                        //        }
                        //        else
                        //        {
                        //            needOri = (needOri - needBlack) / (needWhite - needBlack) * needRefK;
                        //        }
                        //        result2[i] = needOri;
                        //    }
                        //}
                        //result2 = Spectral.ReflectivityCorrection.DotByLine(result2, (int)ScanParaMeter.parminfo.SpatialPixelNumCur, (int)ScanParaMeter.parminfo.SpectralChannelNumCur, (int)ee_point.AbsolutePoint.Y, ScanParaMeter.k_White, ScanParaMeter.White_ReadBytes, ScanParaMeter.Black_ReadBytes);

                        var model2 = new SeriesData()
                        {
                            SeriesColor = ee_point.SelectionColor,
                            AreaType = AreaType.Point,
                            AreaGuid = ee_point.Guid,
                            PointX = ee_point.AbsolutePoint.X,
                            PointY = ee_point.AbsolutePoint.Y,
                            SpectralData = result2,
                        };
                        seriesDatas.Add(model2);
                        PointToLineChart(model2);
                        break;
                }
            };
        }

        /// <summary>
        /// 保持有一个Series
        /// </summary>
        private void DrawSeriesEmpty()
        {
            if (chartControl2.Series.Count == 0)
            {
                Series temp = new Series();
                chartControl2.Series.Add(temp);
            }
        }

        public void PointToLineChart(SeriesData data)
        {
            if (currentWaveLengths == null)
            {
                chartControl2.Series.Clear();
                DrawSeriesEmpty();
                return;
            }

            //选择的波长
            int startIndex = waveLengthStartIndex;
            int endIndex = waveLengthEndIndex;
            int length = endIndex - startIndex + 1;

            var series = new Series("", ViewType.Line);
            series.View.Color = data.SeriesColor;

            //将数据添加到折线图
            for (int i = 0; i < length; i++)
            {
                int index = i + startIndex;
                series.Points.Add(new SeriesPoint(currentWaveLengths[index].ToString("F2"), data.SpectralData[index].ToString("F4")));
            }
            chartControl2.Series.Add(series);

            if (chartControl2.Series[0].Points.Count == 0)
                chartControl2.Series.RemoveAt(0);

            //刷新控件
            chartControl2.Refresh();
        }


        private void gridView_GuangPu_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            if (e.Column == colYANSE)
            {
                e.Appearance.BackColor = (Color)e.CellValue;
                e.Appearance.ForeColor = (Color)e.CellValue;
            }
        }

        private void gridView_GuangPu_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column == colGUANGPUBIANHAO)
            {
                e.DisplayText = (e.ListSourceRowIndex + 1).ToString();
            }
        }

        private void btn_DeleteCheck_Click(object sender, EventArgs e)
        {
            int[] selectArray = gridView_GuangPu.GetSelectedRows();
            if (selectArray.Count() <= 0)
            {
                return;
            }
            var seriesData = gridView_GuangPu.GetRow(selectArray[0]) as SeriesData;
            if (seriesData.AreaType == AreaType.Point)
            {
                imageViewWithTools1.RemovePoint(seriesData.AreaGuid);
            }
            else if (seriesData.AreaType == AreaType.Rectangle)
            {
                imageViewWithTools1.RemoveRectangle(seriesData.AreaGuid);
            }

            int index = seriesDatas.IndexOf(seriesData);
            if (index > -1)
            {
                chartControl2.Series.RemoveAt(index);
                seriesDatas.RemoveAt(index);
            }
            DrawSeriesEmpty();
            chartControl2.Refresh();
            gridControl_GuangPu.RefreshDataSource();
        }

        private void btn_DeleteAll_Click(object sender, EventArgs e)
        {
            seriesDatas.Clear();
            chartControl2.Series.Clear();
            DrawSeriesEmpty();

            gridControl_GuangPu.RefreshDataSource();
            imageViewWithTools1.ClearPoints();//删除点
            imageViewWithTools1.ClearRectangles();//删除矩形选区
        }

        private void btn_SaveToFile_Click(object sender, EventArgs e)
        {
            if (seriesDatas.Count <= 0)
            {
                FormShowHelper.ShowMessage("无数据".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            int selectindex;
            int[] selectArray = gridView_GuangPu.GetSelectedRows();
            if (selectArray != null)
            {
                selectindex = selectArray[0];
            }
            else
            {
                FormShowHelper.ShowMessage("未选中任何数据".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            SaveGuangPuToCSVFile(selectindex);
        }

        private void btn_SaveAllToFile_Click(object sender, EventArgs e)
        {
            if (seriesDatas.Count <= 0)
            {
                FormShowHelper.ShowMessage("无数据".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            SaveGuangPuToCSVFile();
        }


        /// <summary>
        /// 将光谱数据保存到CSV文件，传入选中数据的下标时则只生成选中数据
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool SaveGuangPuToCSVFile(int index = -1)
        {
            if (seriesDatas.Count <= 0)
            {
                FormShowHelper.ShowMessage("无数据".ToMultiLanguage(), "提示".ToMultiLanguage());
                return false;
            }
            var listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

            //选择的波长
            int startIndex = waveLengthStartIndex;
            int endIndex = waveLengthEndIndex;
            int length = endIndex - startIndex + 1;


            SaveFileDialog saveDlg = new SaveFileDialog();
            saveDlg.Title = "导出".ToMultiLanguage();
            saveDlg.Filter = "csv" + "文件".ToMultiLanguage() + "(*.csv)|*.csv";
            if (saveDlg.ShowDialog() != DialogResult.OK)
                return false;
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(saveDlg.FileName, FileMode.Create);
                sw = new StreamWriter(fs, Encoding.UTF8);
                string str = string.Empty;
                for (int i = 0; i < length; i++)
                {
                    if (i > 0)
                        str += listSeparator;
                    //str += ",";
                    str += $"{currentWaveLengths[startIndex + i].ToString("F2")}nm";

                }
                sw.WriteLine(str);
                for (int n = 0; n < chartControl2.Series.Count; n++)
                {
                    if (n == 0 && chartControl2.Series[n].Points.Count == 0)
                    {
                        continue;
                    }
                    if (index > -1 && n != index)
                    {
                        continue;
                    }
                    str = string.Empty;
                    for (int i = 0; i < length; i++)
                    {
                        if (i > 0)
                            str += listSeparator;
                        //str += ",";
                        str += $"{chartControl2.Series[n].Points[i].Values[0]}";
                    }
                    sw.WriteLine(str);
                }
                FormShowHelper.ShowMessage("保存成功！".ToMultiLanguage(), "提示".ToMultiLanguage());
            }
            catch (Exception ex)
            {
                FormShowHelper.ShowMessage(ex.Message, "提示".ToMultiLanguage());
            }
            finally
            {
                sw?.Close();
                fs?.Close();
            }
            return true;
        }

        /// <summary>
        /// 单击控件时获取选中曲线然后突出显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridControl_GuangPu_Click(object sender, EventArgs e)
        {
            if (seriesDatas.Count <= 0)
            {
                return;
            }
            int selectindex;
            int[] selectArray = gridView_GuangPu.GetSelectedRows();
            if (selectArray != null)
            {
                selectindex = selectArray[0];
            }
            else
            {
                FormShowHelper.ShowMessage("未选中任何数据".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            SeriesThick(selectindex);
        }

        /// <summary>
        /// 将光谱曲线加粗
        /// </summary>
        /// <param name="rowIndex"></param>
        private void SeriesThick(int rowIndex)
        {
            for (int i = 0; i < chartControl2.Series.Count; i++)
            {
                var series = chartControl2.Series[i];
                var lineSeriesView = series.View as LineSeriesView;

                if (lineSeriesView != null)
                {
                    if (i == rowIndex)
                    {
                        lineSeriesView.LineStyle.Thickness = 6;
                    }
                    else
                    {
                        lineSeriesView.LineStyle.Thickness = 2;
                    }
                }
            }
        }

        private void cboStartWaveLength_Validating(object sender, CancelEventArgs e)
        {
            int startIndex = waveLengthStartIndex;
            int endIndex = waveLengthEndIndex;
            if (startIndex >= endIndex)
                e.Cancel = true;
        }

        private void cboStartWaveLength_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (waveLengthChangedCancel)
                return;
            PointToLineChartAll();
        }

        private void PointToLineChartAll()
        {
            if (seriesDatas.Count == 0)
                return;
            chartControl2.Series.Clear();
            foreach (var item in seriesDatas)
            {
                PointToLineChart(item);
            }
        }


        #endregion

        private void sortingBaseBtbSendToCppProgram_Click(object sender, EventArgs e)
        {
            if (ScanParaMeter.camera == null || model == null || model.plss.Count == 0)
            {
                FormShowHelper.ShowMessage("相机未连接或模型不存在".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            var appset = GlobalSettings.ApplySetting;
            if (model.Sn != ScanParaMeter.camera.Info.InstrumentSN)
            {
                FormShowHelper.ShowMessage("模型的序列号与相机的不匹配".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
          

            //string pathname = Path.GetFileNameWithoutExtension(model.hdr_spe_path);
            if (!CalibrationInfo.LoadCalibrationFiles(model: model))
            {
                return;
            }

            if (ScanParaMeter.SortCorrectionData.k_White.Length != ScanParaMeter.parminfo.SpectralChannelNumCur
                || ScanParaMeter.SortCorrectionData.Black_ReadBytes.Length != ScanParaMeter.parminfo.SpectralChannelNumCur * ScanParaMeter.parminfo.SpatialPixelNumCur
                || ScanParaMeter.SortCorrectionData.White_ReadBytes.Length != ScanParaMeter.SortCorrectionData.Black_ReadBytes.Length)
            {
                FormShowHelper.ShowMessage("相机ROI已修改，请重新进行校准".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }

            if (ScanParaMeter.SortCorrectionData.White_FrameRate != appset.SortCameraSetting.FrameRate
                || ScanParaMeter.SortCorrectionData.Black_FrameRate != appset.SortCameraSetting.FrameRate
                || ScanParaMeter.SortCorrectionData.White_ExposureTime != appset.SortCameraSetting.ExposureTime
                || ScanParaMeter.SortCorrectionData.Black_ExposureTime != appset.SortCameraSetting.ExposureTime)
            {
                FormShowHelper.ShowMessage("帧频或曝光时间已修改，请重新进行校准".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }

            SaveCalibrationData();
            AIModel aiModel = new AIModel();
            aiModel.Sn = ScanParaMeter.camera.Info.InstrumentSN;
            aiModel.FxModel = ScanParaMeter.camera.Info.DisplayModel;
            aiModel.ExpTime = GlobalSettings.ApplySetting.SortCameraSetting.ExposureTime;
            aiModel.FrameRate = GlobalSettings.ApplySetting.SortCameraSetting.FrameRate;
            aiModel.Gain = GlobalSettings.ApplySetting.SortCameraSetting.Gain;
            aiModel.StartBandIndex = model.StartBandIndex;
            aiModel.EndBandIndex = model.EndBandIndex;
            aiModel.RoiBandStartIndex = model.RoiBandStartIndex;
            aiModel.LimitScopeFlag = model.BackgroundCorrection ? 1 : 0;
            aiModel.LowestValue = model.StartBackgroundThreshold;
            aiModel.HighestValue = model.EndBackgroundThreshold;
            aiModel.FilterStrength = 1;
            foreach (var item in model.Preprocessings)
            {
                if (item.Enabled)
                {
                    switch (item.PreprocessType)
                    {
                        //[DescriptionSort("平滑处理", 0)]
                        case PreprocessTypes.SmoothingAlgorithm:
                            aiModel.Preprocessings.Add(0);
                            aiModel.FilterStrength = item.FilterStrength;
                            break;
                        //[DescriptionSort("基线校正", 1)]
                        case PreprocessTypes.BaselineCorrection:
                            if (item.PreprocessAlgorithm == 1)
                            {
                                aiModel.Preprocessings.Add(1);
                            }
                            else if (item.PreprocessAlgorithm == 2)
                            {
                                aiModel.Preprocessings.Add(2);
                            }
                            break;
                        //[DescriptionSort("尺度缩放", 2)]
                        case PreprocessTypes.ScaleScalingAlgorithm:
                            aiModel.Preprocessings.Add(3);
                            break;
                    }
                }
            }
            aiModel.CoreList = new List<PlsAI>();
            foreach (var item in model.plss)
            {
                PlsAI plsAI = new PlsAI();
                aiModel.CoreList.Add(plsAI);
                plsAI.TargetName = "xx";
                plsAI.ModelType = 0;
                plsAI.selectIndex = item.selectIndex;
                plsAI.classid = item.classid;
                plsAI.threshold = item.threshold;
                plsAI.Components = item.Components;
                plsAI.Features = item.Features;
                plsAI.Intercept = item.GetIntercept_list();
                plsAI.Coef = item.GetCeof_list();
                plsAI.StdX = item.GetStdX_list();
                plsAI.MeanX = item.GetMeanX_list();
            }

            string content = JsonHelp.SerializeObject(aiModel);
            string path = GlobalSettings.ApplyInfo.ApplyParamPath + "modeCpp.models";
            // 使用 StreamWriter 将字符串写入文件
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine(content); // 写入字符串
            }
            FormShowHelper.ShowMessage("模型数据发送成功".ToMultiLanguage(), "提示".ToMultiLanguage());
        }
        private void SaveCalibrationData()
        {
            var White = ScanParaMeter.SortCorrectionData.White_ReadBytes;
            var Black = ScanParaMeter.SortCorrectionData.Black_ReadBytes;
            var refK = ScanParaMeter.SortCorrectionData.k_White;
            string str = "";
            string path = GlobalSettings.ApplyInfo.ApplyParamPath + "modelCpp.calibrationInfo";
            // 使用 StreamWriter 写入文件
            using (StreamWriter writer = new StreamWriter(path))
            {
                str = "k_White=[";
                str += string.Join(",", refK) + "]";
                writer.WriteLine(str); // 将字符串写入文件
                str = "White_ReadBytes=[";
                str += string.Join(",", White) + "]";
                writer.WriteLine(str); // 将字符串写入文件
                str = "Black_ReadBytes=[";
                str += string.Join(",", Black) + "]";
                writer.WriteLine(str); // 将字符串写入文件
            }
        }

        private void btnrunfastsorting_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK != FormShowHelper.ShowMessage("是否启动快速分选程序？".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }))
            {
                return;
            }
            Process process = new Process();
            string ImagePath = @"\fastsorting\FastSorting.exe";
            string path = Environment.CurrentDirectory + ImagePath;
            process.StartInfo.FileName = path;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;
            process.Start();
        }
    }
}
