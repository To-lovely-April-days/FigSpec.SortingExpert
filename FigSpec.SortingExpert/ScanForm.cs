using Cameras;
using DevExpress.XtraCharts;
using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.SettingForms;
using FigSpec.SortingExpert.Tools;
using FigSpec.Spectral.Extensions;
using Globalization;
using Hyperspectral.Tools;
using ImgView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace FigSpec.SortingExpert
{
    public partial class ScanForm : BaseFormInside
    {
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
        /// <summary>
        /// 进度条
        /// </summary>
        float progress;
        /// <summary>
        /// 界面上显示的图片数据
        /// </summary>
        private byte[] img = null;

        /// <summary>
        /// 选择校准前的选取方式
        /// </summary>
        private BeforeUsedEnum BeforeUsed = BeforeUsedEnum.None;


        public ScanForm()
        {
            InitializeComponent();
            NotificationAction.SendStatus2Form += (type) =>
            {
                if (type == NoticeForm.NotScan)
                    return;
                updateButton();
            };
            NotificationAction.RefreshScanFrame = updateFrameRate;
        }

        private void ScanForm_Load(object sender, EventArgs e)
        {
            updateFrameRate();
            updateButton();
            SomeEvent();

        }

        /// <summary>
        /// 刷新界面显示帧率
        /// </summary>
        private void updateFrameRate()
        {
            var ass = GlobalSettings.ApplySetting;
            if (ScanParaMeter.camera != null)
            {
                labFrameRate.Text = "当前帧率：".ToMultiLanguage() + $"{ass.ScanCameraSetting.FrameRate}fps";
            }
            else
            {
                labFrameRate.Text = "当前帧率：".ToMultiLanguage() + "--";
            }
        }



        /// <summary>
        /// 界面按钮状态
        /// </summary>
        /// <param name="status"></param>
        private void updateButton(bool status = false)
        {
            if (status)
            {
                btnConnectCamera.Enabled = false;
                btnStartScan.Text = "结束采集".ToMultiLanguage();
                btnCameraSetting.Enabled = false;
                btndisplayandcalibsetting.Enabled = false;
                btn_SeleteBlack.Enabled = false;
                btn_SelectWhite.Enabled = false;
                btnsaveimage.Enabled = false;
                btnPause.Enabled = true;
                SaveBeforeUsed();
                imageViewWithTools1.SetToolButtons(new ToolButton[] { ToolButton.Move, ToolButton.ZoomIn, ToolButton.ZoomOut });
                imageViewWithTools1.imageView.DragEnabled = true;
                //imageViewWithTools1.Enabled = false;
            }
            else
            {
                btnConnectCamera.Enabled = true;
                btnStartScan.Text = "开始采集".ToMultiLanguage();
                btnPause.Text = "暂停".ToMultiLanguage();
                btnPause.Enabled = false;
                btnCameraSetting.Enabled = true;
                btndisplayandcalibsetting.Enabled = true;
                btn_SeleteBlack.Enabled = true;
                btn_SelectWhite.Enabled = true;
                btnsaveimage.Enabled = true;
                imageViewWithTools1.SetToolButtons(new ToolButton[] { ToolButton.Move, ToolButton.ZoomIn, ToolButton.ZoomOut, ToolButton.Rectangle, ToolButton.Point });
                SetBeforeUsed();
                //imageViewWithTools1.Enabled = true;
            }
            if (ScanParaMeter.camera == null)
            {
                btnConnectCamera.Text = "连接相机".ToMultiLanguage();
            }
            else
            {
                btnConnectCamera.Text = "断开连接".ToMultiLanguage();
            }
        }

        /// <summary>
        /// 用于黑白校准的事件
        /// </summary>
        public void SomeEvent()
        {
            imageViewWithTools1.AddSelectionEvent += (SelectionType type, object value) =>
            {
                bool IsLegal = true;
                if (ScanParaMeter.collectData.SuspendFlag)
                {
                    IsLegal = false;
                }
                if (ScanParaMeter.collectData.spe == null && IsLegal)
                {
                    FormShowHelper.ShowMessage("无图像".ToMultiLanguage(), "提示".ToMultiLanguage());
                    IsLegal = false;
                }
                switch (type)
                {
                    case SelectionType.矩形:
                        var ee_rectangle = value as RectangleEventArgs;
                        if (!IsLegal || ScanParaMeter.collectData.spe.Raw == null)
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

                        if (cb_ReflectanceDisplay.Checked && ScanParaMeter.collectData.spe.DataType != 4)
                        {
                            //原始数据，需要校准
                            var n = 0;
                            ScanParaMeter.collectData.spe.ParallelFor(ee_rectangle.AbsoluteRectangle, (int x, int y, float progress, float[] values) =>
                            {
                                lock (avg)
                                {
                                    for (int i = 0; i < avg.Length; i++)
                                    {
                                        float cdata = 0;
                                        float needOri = values[i];
                                        float needRefWhiteK = ScanParaMeter.ScanCorrectionData.k_White[i];
                                        float needWhite = ScanParaMeter.ScanCorrectionData.White_ReadBytes[y + i * ScanParaMeter.collectData.spe.Samples];
                                        float needBlack = ScanParaMeter.ScanCorrectionData.Black_ReadBytes[y + i * ScanParaMeter.collectData.spe.Samples];
                                        if (needWhite <= needBlack || needOri <= needBlack)
                                        {
                                            cdata = 0;
                                        }
                                        else
                                        {
                                            cdata = (needOri - needBlack) / (needWhite - needBlack) * needRefWhiteK;
                                        }
                                        avg[i] += cdata;
                                    }
                                    n++;
                                }
                            });
                            for (int i = 0; i < avg.Length; i++)
                            {
                                avg[i] = avg[i] / n;
                            }
                        }
                        else
                        {
                            avg = ScanParaMeter.collectData.spectraOfSPE.GetAverageData(ScanParaMeter.collectData.spe, ee_rectangle.AbsoluteRectangle);
                        }
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
                        if (!IsLegal || ScanParaMeter.collectData.spe.Raw == null)
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
                        if (cb_ReflectanceDisplay.Checked && ScanParaMeter.collectData.spe.DataType != 4)
                        {
                            result2 = Spectral.ReflectivityCorrection.DotByLine(result2, (int)ScanParaMeter.parminfo.SpatialPixelNumCur, (int)ScanParaMeter.parminfo.SpectralChannelNumCur, (int)ee_point.AbsolutePoint.Y, ScanParaMeter.ScanCorrectionData.k_White, ScanParaMeter.ScanCorrectionData.White_ReadBytes, ScanParaMeter.ScanCorrectionData.Black_ReadBytes);
                        }

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

                gridControl_GuangPu.RefreshDataSource();
            };

            gridControl_GuangPu.DataSource = seriesDatas;
            gridControl_GuangPu.RefreshDataSource();
            imageViewWithTools1.SetToolButtons(new ToolButton[] { ToolButton.Move, ToolButton.ZoomIn, ToolButton.ZoomOut, ToolButton.Rectangle, ToolButton.Point });
            imageViewWithTools1.SetFreeDragXY(false);//XY线不允许拖动

        }



        #region 界面事件

        /// <summary>
        /// 连接摄像头
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnectCamera_Click(object sender, EventArgs e)
        {
            try
            {
                if (ScanParaMeter.camera == null)
                {
                    FormShowHelper.ShowLoadingForm(this, "相机连接中...".ToMultiLanguage(), second: 60, true);
                    ScanParaMeter.OpenCamera(true, (string res) =>
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
                    updateFrameRate();
                    FormShowHelper.CloseLoadingForm();
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
                    FormShowHelper.CloseLoadingForm();
                    FormShowHelper.ShowLoadingForm(this, "相机已经断开".ToMultiLanguage(), second: 60, true);
                }
                updateButton();
                FormShowHelper.CloseLoadingForm();
                NotificationAction.SendStatus2Form?.Invoke(NoticeForm.NotScan);
            }
            catch (Exception)
            {
                FormShowHelper.CloseLoadingForm();
            }

            LoadWaveLength();
        }

        /// <summary>
        /// 启动拍摄
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                //分选功能是否正在被训练模块使用
                if (GlobalSettings.ApplySetting.runingApp == "scan" && GlobalSettings.ApplySetting.stopAppFlag == false)
                {
                    GlobalSettings.ApplySetting.stopAppFlag = true;
                    if (ScanParaMeter.camera != null && ScanParaMeter.camera.IsOpen)
                    {
                        ScanParaMeter.collectData?.StopGrab();
                        NotificationAction.SendStatus2Form?.Invoke(NoticeForm.NotScan);
                    }
                    //FormShowHelper.ShowLoadingForm(this, "正在停止...".ToMultiLanguage(), second: 60 * 30, true);
                    if (ScanParaMeter.IsSavingImage)
                    {
                        FormShowHelper.ShowLoadingForm(this, "正在将采集到的图像写入磁盘".ToMultiLanguage(), 100, true);
                        while (ScanParaMeter.IsSavingImage)
                        {
                            Thread.Sleep(100);
                        }
                        FormShowHelper.CloseLoadingForm();
                    }
                    Thread thread = new Thread(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                if (string.IsNullOrEmpty(GlobalSettings.ApplySetting.runingApp))
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
                        }
                        catch (Exception)
                        {

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
                //分选功能是否正在被训练模块使用
                if (ScanParaMeter.camera == null)
                {
                    FormShowHelper.ShowLoadingForm(this, "相机连接中...".ToMultiLanguage(), second: 60, true);
                    ScanParaMeter.OpenCamera(true, (string res) =>
                    {
                        if (!string.IsNullOrEmpty(res))
                        {
                            FormShowHelper.ShowMessage(res.ToMultiLanguage(), "提示".ToMultiLanguage());
                        }
                    });
                    FormShowHelper.CloseLoadingForm();
                    LoadWaveLength();
                }
                if (cboStartWaveLength.Properties.Items.Count == 0)
                {
                    LoadWaveLength();
                }

                if (ScanParaMeter.camera != null)
                {
                    ScanParaMeter.ResetCameraInfo(true);

                    if (!GlobalSettings.ApplySetting.scanSet.SaveSawImgFlag)
                    {
                        //判断校准文件
                        if (!CalibrationInfo.LoadCalibrationFiles())
                            return;
                    }

                    if (chartControl2.Series.Count != 0)
                    {
                        btn_DeleteAll_Click(null, null);
                    }
                    ScanParaMeter.collectData.DataSaveComplated = GetImageAfterDataSave;
                    updateFrameRate();
                    FormShowHelper.ShowLoadingForm(this, "正在启动...".ToMultiLanguage(), second: 60 * 30, true);
                    updateButton(true);
                    FormShowHelper.CloseLoadingForm();
                    ResetTimer();

                    imageViewWithTools1.imageView.AutoResize();
                    ScanParaMeter.collectData.StartGrab();
                }

            }
            catch (Exception ee)
            {

            }
        }

        private void GetImageAfterDataSave()
        {
            if (!IsHandleCreated)
                return;
            this.Invoke(new Action(() =>
            {
                //timer1.Interval = 800; // 设置定时器的时间间隔为1秒
                //timer1.Tick += ImageViewTimer_Tick; // 订阅Timer的Tick事件
                //timer1.Start();
                var cameraSetting = GlobalSettings.ApplySetting.ScanCameraSetting;
                if (ScanParaMeter.collectData.spe.Hdr.DataType == 4)
                {
                    //校准后的图片
                    //每帧数据转rgb
                    float tr = cameraSetting.ImgThresholdFR;
                    float tg = cameraSetting.ImgThresholdFG;
                    float tb = cameraSetting.ImgThresholdFB;
                    img = SpeExtend.PreviewRefRGBOfLine(ScanParaMeter.collectData.spe, cameraSetting.ImgR, cameraSetting.ImgG, cameraSetting.ImgB, tr, tg, tb);
                }
                else
                {
                    //原始光谱图片
                    ScanParaMeter.collectData.spectraOfSPE.ToBGR(ScanParaMeter.collectData.spe, (cameraSetting.ImgR, cameraSetting.ImgG, cameraSetting.ImgB), (cameraSetting.ImgOrginThresholdR, cameraSetting.ImgOrginThresholdG, cameraSetting.ImgOrginThresholdB), (byte[] nd, int i, int total, bool complete) =>
                    {
                        img = nd;
                        progress = i / (float)total * 100f;
                    });
                }
                var im = img.ToBitmap(ScanParaMeter.collectData.spe.Lines, ScanParaMeter.collectData.spe.Samples);
                imageViewWithTools1.SetImage(im, GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
            }));
        }

        /// <summary>
        /// 相机设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCameraSetting_Click(object sender, EventArgs e)
        {
            if (ScanParaMeter.camera != null)
            {
                CameraSettingForm cameraSettingForm = new CameraSettingForm(true, true, null);
                FormShowHelper.ShowDialog(cameraSettingForm);
                updateFrameRate();
            }
            else
            {
                FormShowHelper.ShowMessage("请先连接相机".ToMultiLanguage(), "提示".ToMultiLanguage());
            }
        }

        /// <summary>
        /// 黑白校准设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btndisplayandcalibsetting_Click(object sender, EventArgs e)
        {
            if (ScanParaMeter.camera != null)
            {
                DisplayAndCalibrationSettingForm displayAndCalibrationSettingForm = new DisplayAndCalibrationSettingForm();
                FormShowHelper.ShowDialog(displayAndCalibrationSettingForm);
            }
            else
            {
                FormShowHelper.ShowMessage("请先连接相机".ToMultiLanguage(), "提示".ToMultiLanguage());
            }

        }

        /// <summary>
        /// 白校准
        /// </summary>
        private void btn_SelectWhite_Click(object sender, EventArgs e)
        {
            DoCalibrate(ColorType.白);
        }

        /// <summary>
        /// 黑校准
        private void btn_SeleteBlack_Click(object sender, EventArgs e)
        {
            DoCalibrate(ColorType.黑);
        }

        private void DoCalibrate(ColorType colorType)
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

            var form = new SplicingWhiteCalibrationForm(true, colorType, null, null, null);
            FormShowHelper.ShowDialog(form);
        }

        private void SaveBeforeUsed()
        {
            if (imageViewWithTools1.imageView.PointEnabled)
            {
                BeforeUsed = BeforeUsedEnum.选点;
            }
            else if (imageViewWithTools1.imageView.RectangleEnabled)
            {
                BeforeUsed = BeforeUsedEnum.选框;
            }
            else if (imageViewWithTools1.imageView.DragEnabled)
            {
                BeforeUsed = BeforeUsedEnum.拖动;
            }
            else
            {
                BeforeUsed = BeforeUsedEnum.None;
            }
        }

        private void SetBeforeUsed()
        {
            switch (BeforeUsed)
            {
                case BeforeUsedEnum.拖动:
                    imageViewWithTools1.imageView.DragEnabled = true;
                    break;
                case BeforeUsedEnum.选点:
                    imageViewWithTools1.imageView.PointEnabled = true;
                    break;
                case BeforeUsedEnum.选框:
                    imageViewWithTools1.imageView.RectangleEnabled = true;
                    break;
                default:
                    imageViewWithTools1.imageView.DragEnabled = true;
                    break;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnsaveimage_Click(object sender, EventArgs e)
        {
            if (ScanParaMeter.collectData.spe == null)
            {
                FormShowHelper.ShowMessage("请先采集图像".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            var appset = GlobalSettings.ApplySetting;
            if (appset.scanSet.SaveSawImgFlag)
            {
                if (ScanParaMeter.collectData.spe.DataType == 4)
                {
                    FormShowHelper.ShowMessage("采集图像是校准数据，无法保存成原始文件".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
            }
            else
            {
                if (!CalibrationInfo.CheckScanCalibrationData())
                {
                    return;
                }
                //if (ScanParaMeter.collectData.spe.DataType != 4)
                //{
                //    FormShowHelper.ShowMessage("采集图像是原始数据，无法保存成校准后的文件".ToMultiLanguage(), "提示".ToMultiLanguage());
                //    return;
                //}
            }

            try
            {
                var form = new SaveFileOfName();
                if (FormShowHelper.ShowDialog(form) == DialogResult.OK)
                {
                    using (var tokenSource = new CancellationTokenSource())
                    using (var loading = new LoadingForm("保存数据".ToMultiLanguage(), "正在保存高光谱数据,请稍等...".ToMultiLanguage(), isShowCancel: true, isShowProgress: false))
                    {
                        loading.OnDoWork += (eArgs) =>
                        {
                            int startX = 0;
                            int lines = ScanParaMeter.collectData.spe.Hdr.Lines;
                            string path = Path.Combine(appset.FolderPathOfBrowse, form.FileName + ".spe");
                            if (appset.scanSet.SaveDisplayArea && !appset.scanSet.SaveSawImgFlag)
                            {
                                //截取部分保存
                                var rect = imageViewWithTools1.GetCropAbsRect();
                                startX = rect.X > 0 ? rect.X : 0;
                                if (rect.Width + startX >= lines)
                                {
                                    lines = lines - startX;
                                }
                                else
                                {
                                    lines = rect.Width;
                                }
                            }

                            void OnProgress(float value, bool isEnd)
                            {
                                if (loading.MyWork.CancellationPending)
                                {
                                    eArgs.Cancel = true;
                                    tokenSource.Cancel();
                                    return;
                                }

                                loading.MyWork.ReportProgress((int)value);
                                if (isEnd)
                                {
                                    loading.MyWork.ReportProgress(100);
                                }
                            }

                            //保持高光谱数据
                            SaveRefData(path, startX, lines, OnProgress, tokenSource);

                            //保存图片
                            SaveAllImage(path, startX, lines, loading.MyWork.CancellationPending);

                        };
                        loading.ShowDialog();
                    }
                }
            }
            catch
            {
                FormShowHelper.ShowMessage("数据保存失败".ToMultiLanguage(), "提示".ToMultiLanguage());
            }
        }
 

        /// <summary>
        /// 保存文件图片
        /// </summary>
        /// <param name="startX">开始帧数</param>
        /// <param name="lines">保存帧数</param>
        /// <param name="spePath">spe文件路径</param>
        private void SaveAllImage(string spePath, int startX, int lines, bool isCancel)
        {
            if (isCancel)
                return;

            //int stepx = 1;
            //int maxLines = 100000;
            //if (lines > maxLines)
            //{
            //    //帧数过多，压缩图片
            //    stepx = (int)Math.Ceiling(lines * 1.0 / maxLines);
            //    lines = lines / stepx;
            //}

            ////自定义像素个数
            //int customSamples = GlobalSettings.ApplySetting.CustomSamples;
            //byte[] imgg = new byte[lines * customSamples * 3];
            //int stride = lines * 3;
            //int realStride = ScanParaMeter.collectData.spe.Hdr.Lines * 3;
            //int customStartSampleIndex = GlobalSettings.ApplySetting.StartSampleIndex;
            string path = Path.ChangeExtension(spePath, ".bmp");

            //for (int i = 0; i < lines; i++)
            //{
            //    for (int p = 0; p < customSamples; p++)
            //    {
            //        int currIndexB = p * stride + i * 3;
            //        int realIndexB = (p + customStartSampleIndex) * realStride + (i * stepx + startX) * 3;
            //        if (realIndexB + 3 > img.Length)
            //            continue;
            //        imgg[currIndexB] = img[realIndexB];
            //        imgg[currIndexB + 1] = img[realIndexB + 1];
            //        imgg[currIndexB + 2] = img[realIndexB + 2];

            //        if (isCancel)
            //            return;
            //    }
            //}
            //imgg.ToBitmap(lines, customSamples).Save(path);

            imageViewWithTools1.GetControlPicture().Save(path, ImageFormat.Bmp);

            NotificationAction.FolderPathOfNewImgChanged?.Invoke(); //通知更新浏览界面
        }

        /// <summary>
        /// 保存高光谱反射率图像
        /// </summary>
        /// <param name="path"></param>
        public void SaveRefData(string filename, int startX, int lines, Action<float, bool> onProgress, CancellationTokenSource cts)
        {
            if (ScanParaMeter.collectData.spe == null)
                return;
            try
            {
                if (!GlobalSettings.ApplySetting.scanSet.SaveSawImgFlag)
                {
                    //ScanParaMeter.collectData.spe.SaveWithHandleFrame(path, hdr, (int y, byte[] frame) =>
                    //{
                    //    return ScanParaMeter.collectData.spectraOfLine.ReflexCalibrationFromLine(frame, ScanParaMeter.ScanCorrectionData.Black_ReadBytes, ScanParaMeter.ScanCorrectionData.White_ReadBytes, ScanParaMeter.ScanCorrectionData.k_White,
                    //           ScanParaMeter.collectData.spe.Samples, ScanParaMeter.collectData.spe.Bands,
                    //           ScanParaMeter.parminfo.PixelFormatCur == EnumPixelFormat.Mono8 ? 1 : 12);
                    //},
                    //new Rectangle(startX, 0, lines, ScanParaMeter.collectData.spe.Samples), onProgress: onProgress, interval: 10, cts: cts);

                    //为应对 保存图片为黑色问题，做如下调整 
                    int calcCount = 25;
                    int count = lines / calcCount;
                    if (filename.EndsWith(".hdr") || filename.EndsWith(".spe") || filename.EndsWith(".gps"))
                        filename = filename.Substring(0, filename.Length - 4);
                    var hdrPath = filename + ".hdr";
                    var spePath = filename + ".spe";

                    var hdr = ScanParaMeter.collectData.spe.Hdr.Clone();
                    hdr.DataType = 4;
                    hdr.Lines = count * calcCount;

                    var nRaw = MemoryMappedFile.CreateFromFile(spePath, FileMode.Create, "tmp", hdr.Size);

                    var White = ScanParaMeter.ScanCorrectionData.White_ReadBytes;
                    var Black = ScanParaMeter.ScanCorrectionData.Black_ReadBytes;
                    var refK = ScanParaMeter.ScanCorrectionData.k_White;

                    unsafe
                    {
                        for (long i = 0; i < count; i++)
                        {
                            // 新建文件内存映射
                            var nOffset = (long)i * hdr.SizeOfLine * calcCount;
                            var nStream = nRaw.CreateViewStream(nOffset, hdr.SizeOfLine * calcCount);
                            byte* nPtr = null;
                            nStream.SafeMemoryMappedViewHandle.AcquirePointer(ref nPtr);
                            nPtr += nStream.PointerOffset;
                            // 创建指定区域一帧数据
                            var nframe = new float[hdr.Samples * hdr.Bands * calcCount];


                            //原始文件内存映射
                            long offset = (long)startX * ScanParaMeter.collectData.spe.SizeOfLine + (long)i * ScanParaMeter.collectData.spe.SizeOfLine * calcCount;
                            var stream = ScanParaMeter.collectData.spe.Raw.CreateViewStream(offset, ScanParaMeter.collectData.spe.SizeOfLine * calcCount);
                            byte* ptr = null;
                            stream.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                            ptr += stream.PointerOffset;
                            var frame = new byte[ScanParaMeter.collectData.spe.SizeOfLine * calcCount];
                            System.Runtime.InteropServices.Marshal.Copy((IntPtr)ptr, frame, 0, ScanParaMeter.collectData.spe.SizeOfLine * calcCount);


                            for (long p = 0; p < calcCount; p++)
                            {
                                for (long m = 0; m < ScanParaMeter.collectData.spe.Bands; m++)
                                {
                                    for (long n = 0; n < ScanParaMeter.collectData.spe.Samples; n++)
                                    {
                                        long refIndex = (m * ScanParaMeter.collectData.spe.Samples + n);
                                        byte byte0 = frame[ScanParaMeter.collectData.spe.SizeOfLine * p + refIndex * 2];
                                        byte byte1 = frame[ScanParaMeter.collectData.spe.SizeOfLine * p + refIndex * 2 + 1];
                                        float needOri = (ushort)(byte0 | (byte1 << 8));
                                        long trueBand = m;   //有效波长，0起始
                                        float needRefK = refK[trueBand];
                                        float needWhite = White[refIndex];
                                        float needBlack = Black[refIndex];
                                        if (needWhite <= needBlack || needOri <= needBlack)
                                        {
                                            nframe[p * hdr.Samples * hdr.Bands + n + m * hdr.Samples] = 0;
                                        }
                                        else
                                        {
                                            nframe[p * hdr.Samples * hdr.Bands + n + m * hdr.Samples] = (needOri - needBlack) / (needWhite - needBlack) * needRefK;
                                        }
                                    }
                                }
                            }
                            System.Runtime.InteropServices.Marshal.Copy(nframe, 0, (IntPtr)nPtr, nframe.Length);
                            stream.SafeMemoryMappedViewHandle.ReleasePointer();
                            stream.Dispose();
                            nStream.SafeMemoryMappedViewHandle.ReleasePointer();
                            nStream.Dispose();
                        }
                    }
                    hdr.Save(hdrPath);
                    nRaw.Dispose();
                }
                else
                {
                    //保存原始图像代码
                    ScanParaMeter.collectData.spe.SaveWithHandlePixel(filename, ScanParaMeter.collectData.spe.Hdr, (int x, int y, float[] values) =>
                    {
                        return values;
                    }, onProgress: onProgress, interval: 10, cts: cts);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }

        }




        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// 删除所有目标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_DeleteAll_Click(object sender, EventArgs e)
        {
            seriesDatas.Clear();
            chartControl2.Series.Clear();
            DrawSeriesEmpty();

            gridControl_GuangPu.RefreshDataSource();
            imageViewWithTools1.ClearPoints();//删除点
            imageViewWithTools1.ClearRectangles();//删除矩形选区
        }

        /// <summary>
        /// 删除选中的目标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 保存表格中的光谱到一个单独的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 保存表格中的所有光谱到一个单独的文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void gridView_GuangPu_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            //列名为 gridColumn2的单元列
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





        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        //System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();

        public void ResetTimer()
        {
            ScanParaMeter.ImgTimer = timer;
            timer.Interval = 20; // 设置定时器的时间间隔为1秒
            timer.Tick += Timer_Tick; // 订阅Timer的Tick事件
        }
        int tickCount = 0;
        /// <summary>
        /// 定时刷新显示图像画面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (ScanParaMeter.collectData.SuspendFlag)
                return;

            if (GlobalSettings.CurrentFormName == this.Name)
            {
                //实时刷新的图片
                imageViewWithTools1.SetImage(ScanParaMeter.collectData.GetBitmap(), GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
            }

            tickCount++;
            if (tickCount >= 50)
            {
                //每隔1秒，判断如果内存使用大于80%，则暂停
                tickCount = 0;
                if (Common.GetMemoryLoad() > 80)
                {
                    btnPause_Click(null, null);
                    if (ScanParaMeter.collectData.SuspendFlag)
                    {
                        FormShowHelper.ShowMessage("内存使用量过高，采集已暂停".ToMultiLanguage(), "提示".ToMultiLanguage());
                    }
                }
            }
        }




        #endregion





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


        private void LoadWaveLength()
        {
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

        private void cboStartWaveLength_Validating(object sender, CancelEventArgs e)
        {
            int startIndex = waveLengthStartIndex;
            int endIndex = waveLengthEndIndex;
            if (startIndex >= endIndex)
                e.Cancel = true;
        }

        bool waveLengthChangedCancel = false;
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

        bool cbReflectanceCancel = false;
        private void cb_ReflectanceDisplay_CheckedChanged(object sender, EventArgs e)
        {
            if (cbReflectanceCancel)
            {
                cbReflectanceCancel = false;
                return;
            }
            if (ScanParaMeter.camera == null || ScanParaMeter.collectData == null)
            {
                cbReflectanceCancel = true;
                cb_ReflectanceDisplay.Checked = false;
                return;
            }
            if (!CalibrationInfo.CheckScanCalibrationData())
            {
                cbReflectanceCancel = true;
                cb_ReflectanceDisplay.Checked = false;
                return;
            }
        }
    }


}
