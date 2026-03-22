using Cameras;
using CHNSpec.Tools;
using FigSpec.SortingExpert.Algorithm;
using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Cuda;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.ModelFiles;
using FigSpec.Spectral.Extensions;
using Hyperspectral.SpectralFile;
using Hyperspectral.SpectralProc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{


    public class CollectData
    {
        #region Fields


        /// <summary>
        /// 接收数据（帧数据）的队列
        /// </summary>
        private ConcurrentQueue<GrabData> byteQueue;

        /// <summary>
        /// 清晰度, 用户向导使用
        /// </summary>
        private ConcurrentQueue<float[]> clarityQueue;

        /// <summary>
        /// 准备数据，将接收数据转换成二维数据存储
        /// </summary>
        //private ConcurrentQueue<byte[,]> prepareDataQueue;

        /// <summary>
        /// 数据采集结束时间
        /// </summary>
        private ConcurrentQueue<long> timeDataQueue;

        /// <summary>
        /// 吹气数据，分选结果标签ID
        /// </summary>
        private ConcurrentQueue<byte[,]> ejectDataQueue;

        /// <summary>
        /// 分选结果显示数据，校准后的RGB 二维数组
        /// </summary>
        private ConcurrentQueue<byte[,]> displayDataQueue;

        /// <summary>
        /// 分选结果显示数据，RGB 
        /// </summary>
        private ConcurrentQueue<byte[]> displayRgbQueue;
        /// <summary>
        /// 校准后的反射率数据
        /// </summary>
        private ConcurrentQueue<float[,]> reflectDataQueue;
        /// <summary>
        /// 保存到SPE文件中的数据, 原始数据或者反射率
        /// </summary>
        private ConcurrentQueue<byte[]> saveToSpeFileQueue;


        /// <summary>
        /// 开始采集标志位
        /// </summary>
        private bool StartFlag;
        /// <summary>
        /// Scan 采集状态保存标识
        /// </summary>
        private bool SaveFlag = false;
        /// <summary>
        /// 暂停 true
        /// </summary>
        public bool SuspendFlag { get; set; } = false;

        /// <summary>
        /// 数据类型（解析数据时使用)
        /// </summary>
        private int datatype;
        /// <summary>
        /// 预览时显示的帧数
        /// </summary>
        private int needLines;
        /// <summary>
        /// 空间像素个数
        /// </summary>
        private int Samples;
        /// <summary>
        /// 波段个数
        /// </summary>
        private int bands;
        /// <summary>
        /// 发送吹气的服务端
        /// </summary>
        private ClientControl client;

        private FileStorageHelper _fileStorageHelper;

        /// <summary>
        /// 采集的数据写入到磁盘文件完成后的通知
        /// </summary>
        public Action DataSaveComplated;

        public SPE spe;
        public SpectraOfSPE spectraOfSPE;
        public SpectraOfLine spectraOfLine;
        /// <summary>
        /// 校准功能的SPE
        /// </summary>
        public SPE CalibrateSPE;

        // 创建一个AutoResetEvent，初始状态为非信号状态 (false)
        public AutoResetEvent GrabedDataEvent = new AutoResetEvent(false);
        public AutoResetEvent SortingResultEvent = new AutoResetEvent(false);
        public AutoResetEvent SortingResultDisplayEvent = new AutoResetEvent(false);
        #endregion

        #region Constructor

        public CollectData()
        {

        }

        public CollectData(ClientControl clientControl, int lines, int _datatype, string path = null)
        {
            datatype = _datatype;

            ScanParaMeter.camera.GrabedData += ConnCameraInfo_GrabedData;
            //inputPath = path;
            client = clientControl;
            spectraOfSPE = SpectraFactory.SpectraOfSPE(EnumInterleave.bil);
            spectraOfLine = SpectraFactory.SpectraOfLine(EnumInterleave.bil);
            _fileStorageHelper = new FileStorageHelper();
        }

        #endregion

        #region Handler


        /// <summary>
        /// 队列接收数据事件
        /// </summary>
        /// <param name="grabData"></param>
        private void ConnCameraInfo_GrabedData(GrabData grabData)
        {
            if (grabData.Values?.Length > 0)
            {
#if DEBUG
                ScanParaMeter.ReceivedLines++;
                if (DateTime.Now.Ticks >= ScanParaMeter.StartReceivedTime + 10000000)
                {
                    ScanParaMeter.SpendTime = DateTime.Now.Ticks - ScanParaMeter.StartReceivedTime;
                    ScanParaMeter.FrameFrequency = ScanParaMeter.ReceivedLines - ScanParaMeter.StartReceivedLines;
                    ScanParaMeter.StartReceivedTime = DateTime.Now.Ticks;
                    ScanParaMeter.StartReceivedLines = ScanParaMeter.ReceivedLines;
                }
#endif
                //接收数据队列，添加数据
                byteQueue.Enqueue(grabData);
                timeDataQueue.Enqueue(DateTime.Now.Ticks);
                GrabedDataEvent.Set();

            }
            else
            {
                Console.WriteLine($"%%%%%%%%%%%%%%%数据采集异常&&&&&&&&&&&&&&&");
            }
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public void StopGrab()
        {
            StartFlag = false;
            SaveFlag = false;
            Console.WriteLine($"%%%%%%%%%%%%%%%准备断开" + ScanParaMeter.camera.IsGrab.ToString());
            //相机停止
            Task.Run(() =>
            {
                if (ScanParaMeter.camera.IsGrab)
                    ScanParaMeter.camera.StopGrab();
                ScanParaMeter.ImgTimer.Stop();
            });
        }

        private void InitQueue()
        {
            byteQueue = new ConcurrentQueue<GrabData>();
            clarityQueue = new ConcurrentQueue<float[]>();
            //prepareDataQueue = new ConcurrentQueue<byte[,]>();
            timeDataQueue = new ConcurrentQueue<long>();
            ejectDataQueue = new ConcurrentQueue<byte[,]>();
            displayDataQueue = new ConcurrentQueue<byte[,]>();
            displayRgbQueue = new ConcurrentQueue<byte[]>();
            saveToSpeFileQueue = new ConcurrentQueue<byte[]>();
            reflectDataQueue = new ConcurrentQueue<float[,]>();
        }


        #region 用户向导，不保存数据
        public void StartGrabUser(int lines)
        {
            InitQueue();

            needLines = lines;
            StartFlag = true;
            SaveFlag = false;
            SuspendFlag = false;
            ScanParaMeter.ImgTimer.Start();
            ScanParaMeter.camera.StartGrab();//开启数据处理线程

            Samples = (int)ScanParaMeter.parminfo.SpatialPixelNumCur;
            bands = (int)ScanParaMeter.parminfo.SpectralChannelNumCur;
            datatype = (ScanParaMeter.parminfo.PixelFormatCur == EnumPixelFormat.Mono8) ? 1 : 12;

            ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessQueueUser), null);
        }

        private void ProcessQueueUser(object obj)
        {
            try
            {
                int needNum = 1;
                //存储数据集合
                List<GrabData> data = new List<GrabData>();
                float[][] needData = new float[needNum][];
                int sampleNumber = 200;
                //中间200个像素，
                int startSample = (Samples - sampleNumber) / 2;

                float[] signal = new float[sampleNumber];

                var cameraSetting = GlobalSettings.ApplySetting.ScanCameraSetting;

                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    if (byteQueue.Count >= needNum)
                    {
                        //Console.WriteLine($"使用时间：{ScanParaMeter.SpendTime}，采集帧数：{ScanParaMeter.FrameFrequency}");
                        int i = 0;
                        while (i < needNum)
                        {
                            if (byteQueue.TryDequeue(out var d))
                            {
                                data.Add(d);
                                i++;
                            }
                        }

                        for (int f = 0; f < needNum; f++)
                        {
                            byte[] dataRGB = Algorithms.GetRGBData(data[f].Values, Samples, datatype, cameraSetting.ImgR, cameraSetting.ImgG, cameraSetting.ImgB, cameraSetting.ImgOrginThresholdR, cameraSetting.ImgOrginThresholdG, cameraSetting.ImgOrginThresholdB);
                            int bIndex = 0;
                            int maxValue = 0;
                            for (int b = 0; b < bands; b++)
                            {
                                var t = BitConverter.ToInt16(data[f].Values, (startSample + b * Samples) * 2);
                                if (t > maxValue)
                                {
                                    maxValue = t;
                                    bIndex = b;
                                }
                            }

                            for (int s = 0; s < sampleNumber; s++)
                            {
                                signal[s] = BitConverter.ToInt16(data[f].Values, (startSample + s + bIndex * Samples) * 2);
                            }
                            float maxSignal = signal.Max();
                            float clarity = ReflectPreprocessing.FirstDerivative(signal).Max(n => Math.Abs(n));
                            float[] tempf = new float[2] { maxSignal, clarity };
                            if (clarityQueue.Count > 0)
                            {
                                if (clarityQueue.TryDequeue(out var deleData))
                                {
                                    clarityQueue.Enqueue(tempf);
                                }
                            }
                            else
                            {
                                clarityQueue.Enqueue(tempf);
                            }
                            SaveQueueRgbData(dataRGB);
                        }
                        data.Clear();
                    }
                }

            }
            catch (Exception ee)
            {
                Console.WriteLine("采集异常：" + ee.Message);
            }
            finally
            {
                GlobalSettings.ApplySetting.stopAppFlag = false;
                GlobalSettings.ApplySetting.runingApp = string.Empty;
            }
        }

        public float[] GetClarity()
        {
            if (clarityQueue.Count > 0 && clarityQueue.TryDequeue(out var deleData))
            {
                return deleData;
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Scan采集数据

        /// <summary>
        /// 暂停Scan
        /// </summary>
        public void SuspendScan()
        {
            //相机停止
            ScanParaMeter.camera.StopGrab();
            SuspendFlag = true;
        }

        public void ResumeScan()
        {
            //相机停止
            ScanParaMeter.camera.StartGrab();
            SuspendFlag = false;
        }

        /// SCAN - 开始采集
        public void StartGrab(bool isCalibrate = false, bool isScan = true)
        {

#if DEBUG
            ScanParaMeter.ReceivedLines = 0;
            ScanParaMeter.StartReceivedTime = 0;
            ScanParaMeter.StartReceivedLines = 0;
#endif
            InitQueue();

            SaveFlag = true;
            StartFlag = true;
            SuspendFlag = false;
            needLines = GlobalSettings.ApplySetting.ScanCameraSetting.DisplayFrameNum;

            ScanParaMeter.ImgTimer.Start();//相机打开
            ScanParaMeter.camera.StartGrab();//开启数据处理线程

            Samples = (int)ScanParaMeter.parminfo.SpatialPixelNumCur;
            bands = (int)ScanParaMeter.parminfo.SpectralChannelNumCur;
            datatype = (ScanParaMeter.parminfo.PixelFormatCur == EnumPixelFormat.Mono8) ? 1 : 12;

            GlobalSettings.ApplySetting.runingApp = "";
            GlobalSettings.ApplySetting.stopAppFlag = false;
            //设置启动状态
            if (!isCalibrate)
            {
                GlobalSettings.ApplySetting.runingApp = "scan";
                NotificationAction.SendStatus2Form?.Invoke(0);
                spe.SpeDispose();
            }
            else
            {
                CalibrateSPE.SpeDispose();
            }

            dynamic dynamic2 = new
            {
                IsCalibrate = isCalibrate,
                IsScan = isScan
            };
            ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessQueueScanRaw), dynamic2);
            ThreadPool.QueueUserWorkItem(new WaitCallback(SaveQueue), dynamic2);

            //if (GlobalSettings.ApplySetting.scanSet.SaveSawImgFlag || isCalibrate)
            //{
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessQueueScanRaw), dynamic2);
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(SaveQueue), dynamic2);
            //}
            //else
            //{
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(Prepare2ArrayData), null);
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessQueueScan), null);
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessQueueDisplay), null);
            //    ThreadPool.QueueUserWorkItem(new WaitCallback(SaveQueue), dynamic2);
            //}

        }

        /// <summary>
        /// Scan 的数据处理
        /// </summary>
        /// <param name="dynamic"></param>
        private void ProcessQueueScanRaw(dynamic obj)
        {
            try
            {
                int needNum = 5;
                //存储数据集合
                List<GrabData> data = new List<GrabData>();
                float[][] needData = new float[needNum][];

                bool isPartSave = GlobalSettings.ApplySetting.scanSet.OnlyCacheDisplayArea;
                bool isCalibrate = (bool)obj.IsCalibrate;
                bool isScan = (bool)obj.IsScan;
                var cameraSetting = isScan ? GlobalSettings.ApplySetting.ScanCameraSetting : GlobalSettings.ApplySetting.SortCameraSetting;
                float tr = cameraSetting.ImgOrginThresholdR;
                float tg = cameraSetting.ImgOrginThresholdG;
                float tb = cameraSetting.ImgOrginThresholdB;
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    if (SuspendFlag)
                    {
                        Thread.Sleep(500);
                        if (byteQueue.Count > 0)
                        {
                            byteQueue = new ConcurrentQueue<GrabData>();
                        }
                        continue;
                    }
                    if (byteQueue.Count >= needNum)
                    {
                        int i = 0;
                        while (i < needNum)
                        {
                            if (byteQueue.TryDequeue(out var d))
                            {
                                data.Add(d);
                                i++;
                            }
                        }
                        for (int f = 0; f < needNum; f++)
                        {
                            byte[] dataRGB = Algorithms.GetRGBData(data[f].Values, Samples, datatype, cameraSetting.ImgR, cameraSetting.ImgG, cameraSetting.ImgB, tr, tg, tb);
                            SaveQueueRgbData(dataRGB);
                            SaveQueueSpeData(isPartSave, data[f].Values);
                        }
                        data.Clear();
                    }
                }


            }
            catch (Exception ee)
            {
                Console.WriteLine("采集异常：" + ee.Message);
            }
            finally
            {
                GlobalSettings.ApplySetting.stopAppFlag = false;
                GlobalSettings.ApplySetting.runingApp = string.Empty;
                GC.Collect();
            }
        }

        //private void ProcessQueueScan(object obj)
        //{
        //    try
        //    {
        //        int needNum = GlobalSettings.ApplySetting.CalculatedFrameCount;

        //        CudaAccelerator.Shared.AllocScanClassify(needNum, Samples, bands);
        //        CudaAccelerator.Shared.AllocCALRef(ScanParaMeter.ScanCorrectionData.k_White);
        //        CudaAccelerator.Shared.AllocCALRefWhiteBlack(ScanParaMeter.ScanCorrectionData.White_ReadBytes, ScanParaMeter.ScanCorrectionData.Black_ReadBytes);

        //        while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
        //        {
        //            if (SuspendFlag)
        //            {
        //                Thread.Sleep(500);
        //                if (prepareDataQueue.Count > 0)
        //                {
        //                    prepareDataQueue = new ConcurrentQueue<byte[,]>();
        //                }
        //                continue;
        //            }
        //            if (prepareDataQueue.Count > 0)
        //            {
        //                if (prepareDataQueue.TryDequeue(out var d))
        //                {
        //                    var (frameData, img) = CudaAccelerator.Shared.ScanDealData(needNum, Samples, bands, d);
        //                    reflectDataQueue.Enqueue(frameData);
        //                    displayDataQueue.Enqueue(img);
        //                }
        //            }
        //            else
        //            {
        //                Thread.Sleep(1);
        //            }
        //            //Console.WriteLine($"%%%%%%%%%%%%%%%数据采集异常&&&&&&&&&&&&&&&");
        //        }
        //    }
        //    catch (Exception ee)
        //    {
        //        Console.WriteLine("采集异常：" + ee.Message);
        //    }
        //    finally
        //    {
        //        GlobalSettings.ApplySetting.stopAppFlag = false;
        //        GlobalSettings.ApplySetting.runingApp = string.Empty;
        //    }
        //}

        private void ProcessQueueDisplay(object obj)
        {
            try
            {
                bool isPartSave = GlobalSettings.ApplySetting.scanSet.OnlyCacheDisplayArea;
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    if (SuspendFlag)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    if (displayDataQueue.Count > 0)
                    {
                        if (displayDataQueue.TryDequeue(out var d))
                        {
                            SaveQueueRgbData(d);
                        }
                    }
                    if (reflectDataQueue.Count > 0)
                    {
                        if (reflectDataQueue.TryDequeue(out var d))
                        {
                            SaveQueueSpeData(isPartSave, d);
                        }
                    }
                    //Console.WriteLine($"%%%%%%%%%%%%%%%数据采集异常&&&&&&&&&&&&&&&");
                    Thread.Sleep(1);
                }

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {

            }
        }
        /// <summary>
        /// Scan的生成一个采集的原始图像
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmap()
        {
            return displayRgbQueue?.ToArray().FramesToBitmap(needLines, Samples);
        }


        /// <summary>
        /// Scan的数据保存
        /// </summary>
        /// <param name="path"></param>
        private void SaveQueue(dynamic obj)
        {
            bool isPort = GlobalSettings.ApplySetting.scanSet.OnlyCacheDisplayArea;
            bool isCalibrate = (bool)obj.IsCalibrate;


            string path = GlobalSettings.ApplySetting.FolderPathOfBrowseTemp;

            //校准文件 或者 Scan的原始文件
            string hdr_path = Path.Combine(path, isCalibrate ? "calibratefigspec.hdr" : "figspec.hdr");
            string spe_path = Path.Combine(path, isCalibrate ? "calibratefigspec.spe" : "figspec.spe");

            //开启线程处理整个需要保存的数据
            try
            {
                if (isPort)
                {
                    //实时分选的保存，只保存最后的数据
                    while (SaveFlag)
                    {
                        Thread.Sleep(1000);
                    }
                }
                int linesCount = 0;
                using (FileStream fsWrite = new FileStream(spe_path, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1024000, useAsync: true))
                {
                    ScanParaMeter.IsSavingImage = true;
                    while (SaveFlag || saveToSpeFileQueue.Count > 0)
                    {
                        if (saveToSpeFileQueue.TryDequeue(out byte[] bytes))
                        {
                            fsWrite.Write(bytes, 0, bytes.Length);
                            linesCount++;
                        }
                        else
                        {
                            Thread.Sleep(5);
                        }
                        //Console.WriteLine($"%%%%%%%%%%%%%%%数据采集异常&&&&&&&&&&&&&&&");
                    }
                    ScanParaMeter.IsSavingImage = false;
                }

                var cameraSetting = GlobalSettings.ApplySetting.ScanCameraSetting;
                HDR hdr = new HDR()
                {
                    Samples = (int)ScanParaMeter.parminfo.SpatialPixelNumCur,
                    Bands = (int)ScanParaMeter.parminfo.SpectralChannelNumCur,
                    Lines = linesCount,
                    HeaderOffset = 0,
                    FileType = "ENVI Standard",
                    DataType = ScanParaMeter.parminfo.PixelFormatCur == EnumPixelFormat.Mono8 ? 1 : 12,
                    Interleave = EnumInterleave.bil,
                    SensorType = "Unknown",
                    ByteOrder = 0,
                    XStart = 0,
                    YStart = 0,
                    DefaultBands = new uint[] { uint.Parse(cameraSetting.ImgR.ToString()), uint.Parse(cameraSetting.ImgG.ToString()), uint.Parse(cameraSetting.ImgB.ToString()) },
                    WaveLength = ScanParaMeter.parminfo.SpectralChannelWavelength,
                    WavelengthUnits = EnumWavelengthUnits.Nanometers,
                    //DefaultThresholds = new uint[] { 10000, 10000, 10000 },
                    //RefAdjust = false,
                    //RefAdjustMode = 0,
                    //LensAdjust = true,
                    //LensAdjustMode = 1,
                    FlightHeight = 0,
                    SpaceBinning = 1,
                    SpectrumBinning = 1,
                    FocalLength = 25,
                    GrabMethod = 0,
                    PixelSize = 5.86f,
                    Gain = ScanParaMeter.parminfo.GainCur,
                    FXModel = ScanParaMeter.camera.Info.DisplayModel,
                    SN = ScanParaMeter.camera.Info.InstrumentSN,
                    ExposureTime = ScanParaMeter.parminfo.ExposureTimeCur,
                    CPGpsMatrix = null,
                    //DeductingDarkCurrent = false,
                };
                hdr.Save(hdr_path);

                if (isCalibrate)
                {
                    CalibrateSPE = new SPE(spe_path);
                }
                else
                {
                    spe = new SPE(spe_path);
                }

                //用于采集完成后刷新一下整幅图像
                DataSaveComplated?.Invoke();

                Console.WriteLine($"图像保存完成,采集了{linesCount}帧");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "," + ex.InnerException);
            }
            finally
            {
                InitQueue();
                GC.Collect();
            }

        }

        #endregion

        #region 实时分选
        /// <summary>
        /// 实时分选-开始采集
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="port"></param>
        public void StartGrab(Model model, bool isOutlineSorting)
        {
#if DEBUG
            ScanParaMeter.ReceivedLines = 0;
            ScanParaMeter.StartReceivedTime = 0;
            ScanParaMeter.StartReceivedLines = 0;
#endif

            InitQueue();
            _fileStorageHelper.ClearData();
            SaveFlag = true;
            StartFlag = true;//开始采集标志
            SuspendFlag = false;
            needLines = GlobalSettings.ApplySetting.SortCameraSetting.DisplayFrameNum;


            ScanParaMeter.ImgTimer.Start(); //相机打开
            ScanParaMeter.camera.StartGrab();//开启数据处理线程

            Samples = (int)ScanParaMeter.parminfo.SpatialPixelNumCur;
            //Samples = GlobalSettings.ApplySetting.CustomSamples;  //空间像素个数
            bands = (int)ScanParaMeter.parminfo.SpectralChannelNumCur;
            datatype = (ScanParaMeter.parminfo.PixelFormatCur == EnumPixelFormat.Mono8) ? 1 : 12;

            //设置启动状态
            GlobalSettings.ApplySetting.stopAppFlag = false;
            GlobalSettings.ApplySetting.runingApp = "sort";
            NotificationAction.SendStatus2Form?.Invoke(isOutlineSorting ? NoticeForm.NotOutlineSorting : NoticeForm.NotSorting);

            CudaAccelerator.Shared.AllocPLSSClassify(model, GlobalSettings.ApplySetting.CalculatedFrameCount, Samples, datatype: datatype, isOutline: isOutlineSorting);//公共参数赋值
            if (isOutlineSorting)
            {
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessContourSortingV1_1), model);
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessContourSortingV1_2), model);
            }
            else
            {
                //ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPointSortingV1), model);

                //ThreadPool.QueueUserWorkItem(new WaitCallback(Prepare2ArrayData), model);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessPointSortingV1_2), model);
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendEjectData), model);
                ThreadPool.QueueUserWorkItem(new WaitCallback(SaveDisplayData), model);
            }
        }

        /*
        public unsafe Bitmap GetBitmap(Model model)
        {
            System.Drawing.Bitmap bitmap = img.ToBitmap(needLines, Samples);
            try
            {
                pridictionImage = saveQueuePLSDAResult.ToList();
                //var set = GlobalSettings.ApplySetting;
                var bData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var ptr = (byte*)bData.Scan0;
                Parallel.For(0, Samples, (int y) =>
                {
                    for (int x = 0; x < pridictionImage.Count; x++)
                    {
                        if (pridictionImage[x][y] == (byte)0)
                        {
                            var item = model.classes.First(pp => pp.id == 255);
                            *(ptr + y * bData.Stride + x * 3) = Color.FromArgb(item.color).B;
                            *(ptr + y * bData.Stride + x * 3 + 1) = Color.FromArgb(item.color).G;
                            *(ptr + y * bData.Stride + x * 3 + 2) = Color.FromArgb(item.color).R;
                        }
                        else
                        {
                            foreach (var item in model.classes)
                            {
                                if (pridictionImage[x][y] == item.id)
                                {
                                    *(ptr + y * bData.Stride + x * 3) = Color.FromArgb(item.color).B;
                                    *(ptr + y * bData.Stride + x * 3 + 1) = Color.FromArgb(item.color).G;
                                    *(ptr + y * bData.Stride + x * 3 + 2) = Color.FromArgb(item.color).R;
                                }
                            }
                        }
                    }
                });
                bitmap.UnlockBits(bData);

            }
            catch (Exception ex)
            {
                Console.WriteLine("高光谱图像刷新异常");
            }
            return bitmap;
        }
        */

        //public unsafe Bitmap GetBitmapFromRGB(Model model)
        //{
        //    System.Drawing.Bitmap bitmap = img.ToBitmap(needLines, Samples);
        //    try
        //    {
        //        pridictionImage = displayRgbQueue.ToList();

        //        var bData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
        //        var ptr = (byte*)bData.Scan0;
        //        Parallel.For(0, Samples, (int y) =>
        //        {
        //            for (int x = 0; x < pridictionImage.Count; x++)
        //            {
        //                *(ptr + y * bData.Stride + x * 3) = pridictionImage[x][y * 3];
        //                *(ptr + y * bData.Stride + x * 3 + 1) = pridictionImage[x][y * 3 + 1];
        //                *(ptr + y * bData.Stride + x * 3 + 2) = pridictionImage[x][y * 3 + 2];
        //            }
        //        });
        //        bitmap.UnlockBits(bData);

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("高光谱图像刷新异常");
        //    }
        //    return bitmap;
        //}

        /*
        private void ProcessPointSortingV1(object obj)
        {
            try
            {
                int dealCount = GlobalSettings.ApplySetting.CalculatedFrameCount;
                byte[,] grabDatas = new byte[dealCount, Samples * bands * 2];
                var model = obj as Model;
                GrabData[] lbyteQueue = new GrabData[dealCount];
                long start_time = 0;
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    int currentCount = byteQueue.Count;
                    if (currentCount >= dealCount)
                    {
#if DEBUG
                        LogHelper.WriteLine($"###### 开始,累积的帧数：{byteQueue.Count},时间：{(DateTime.Now.Ticks - start_time)}");
                        LogHelper.WriteLine($"累积的条数达到25条的时间：" + (DateTime.Now.Ticks - start_time));
                        LogHelper.WriteLine($"使用时间：{ScanParaMeter.SpendTime}，采集帧数：{ScanParaMeter.FrameFrequency} ，时间：{(DateTime.Now.Ticks - start_time)}");
                        start_time = DateTime.Now.Ticks;
#endif
                        int i = 0;
                        while (i < dealCount)
                        {
                            if (byteQueue.TryDequeue(out var data))
                            {
                                lbyteQueue[i] = data;
                                i++;
                            }
                            else
                            {
                                Console.WriteLine($"%%%%%%%%%%%%%%%数据采集异常&&&&&&&&&&&&&&&");
                            }
                        }
#if DEBUG
                        LogHelper.WriteLine($"从队列中取{dealCount}条数据的时间：" + (DateTime.Now.Ticks - start_time));
#endif
                        i = 0;
                        int destinationLength = lbyteQueue[i].Values.Length;// * i;
                        while (i < dealCount)
                        {
                            Buffer.BlockCopy(lbyteQueue[i].Values, 0, grabDatas, destinationLength * i, destinationLength);
                            i++;
                        }
#if DEBUG
                        LogHelper.WriteLine($"把{dealCount}条数据拷贝到矩阵中的时间：" + (DateTime.Now.Ticks - start_time));
#endif
                        var (tags, im) = CudaAccelerator.Shared.PLSSClassifyByCorrection(model, dealCount, Samples, grabDatas);
#if DEBUG
                        LogHelper.WriteLine($"执行GPU相关操作的时间：" + (DateTime.Now.Ticks - start_time));
#endif
                        //if (GlobalSettings.ApplySetting.TcpConnectEnable)
                        //{
                        //    SendClient(tags);
                        //}
                        //else
                        //{
                        //    SendToEject(tags);
                        //}
                        SaveQueuePLSDAResult(im);
#if DEBUG
                        LogHelper.WriteLine($"保存图像的时间：" + (DateTime.Now.Ticks - start_time));
#endif
#if DEBUG
                        LogHelper.WriteLine($"###### 总时间：");
                        LogHelper.WriteLine((DateTime.Now.Ticks - start_time));
                        LogHelper.WriteLine($"%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
#endif
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                LogHelper.WriteLine("实时分选每帧数据异常：" + ex.Message);
#endif
            }
            finally
            {
#if DEBUG
                LogHelper.AppendToFile();
#endif
                GC.Collect();
                GlobalSettings.ApplySetting.stopAppFlag = false;
                GlobalSettings.ApplySetting.runingApp = string.Empty;
            }
        }
        */

        //数据准备
        private void Prepare2ArrayData(object obj)
        {
            try
            {
                int dealCount = GlobalSettings.ApplySetting.CalculatedFrameCount;
                long start_time = 0;
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    if (SuspendFlag)
                    {
                        Thread.Sleep(500);
                        if (byteQueue.Count > 0)
                        {
                            byteQueue = new ConcurrentQueue<GrabData>();
                        }
                        continue;
                    }
                    int currentCount = byteQueue.Count;
                    if (currentCount >= dealCount)
                    {
#if DEBUG
                        LogHelper.WriteLine($"Prepare2ArrayData--###### 开始,累积的待分选的数量：{currentCount},时间：{(DateTime.Now.Ticks - start_time)}");
                        LogHelper.WriteLine($"Prepare2ArrayData--使用时间：{ScanParaMeter.SpendTime}，采集帧数：{ScanParaMeter.FrameFrequency} ，时间：{(DateTime.Now.Ticks - start_time)}");
#endif
                        start_time = DateTime.Now.Ticks;
                        int i = 0;
                        int destinationLength = Samples * bands * 2;// * i;
                        byte[,] grabDatas = new byte[dealCount, Samples * bands * 2];
                        while (i < dealCount)
                        {
                            if (byteQueue.TryDequeue(out var data))
                            {
                                Buffer.BlockCopy(data.Values, 0, grabDatas, destinationLength * i, destinationLength);
                                i++;
                            }
                            else
                            {
                                Console.WriteLine($"%%%%%%%%%%%%%%%数据采集异常&&&&&&&&&&&&&&&");
                            }
                        }
                        //prepareDataQueue.Enqueue(grabDatas);
#if DEBUG
                        LogHelper.WriteLine($"Prepare2ArrayData--将数据拷贝到队里的时间：" + (DateTime.Now.Ticks - start_time));
#endif
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                LogHelper.WriteLine("实时分选每帧数据异常：" + ex.Message);
#endif
            }
            finally
            {
#if DEBUG
                LogHelper.AppendToFile();
#endif
            }
        }


        private void ProcessPointSortingV1_2(object obj)
        {
            try
            {
                //bool deleteOverTimeFlag = GlobalSettings.ApplySetting.DeleteOverTimeData;
                int dealCount = GlobalSettings.ApplySetting.CalculatedFrameCount;
                var model = obj as Model;
                long start_time = 0;

                int destinationLength = Samples * bands * 2;// 
                byte[,] grabDatas = new byte[dealCount, Samples * bands * 2];
                int copyLoc = 0;//记录拷贝到了第一个帧数据
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    GrabedDataEvent.WaitOne();
                    int currentCount = byteQueue.Count;
                    if (currentCount > 0)
                    {
#if DEBUG
                        LogHelper.WriteLine($"ProcessPointSortingV1_2--###### 开始,累积的待分选的数量：{currentCount},时间：{(DateTime.Now.Ticks - start_time)}");
                        LogHelper.WriteLine($"ProcessPointSortingV1_2--使用时间：{ScanParaMeter.SpendTime}，采集帧数：{ScanParaMeter.FrameFrequency} ，时间：{(DateTime.Now.Ticks - start_time)}");
#endif
                        start_time = DateTime.Now.Ticks;
                        for (int pp = 0; pp < currentCount; pp++)
                        {
                            if (copyLoc == dealCount)
                            {
                                break;
                            }
                            if (byteQueue.TryDequeue(out var data))
                            {
                                Buffer.BlockCopy(data.Values, 0, grabDatas, destinationLength * copyLoc, destinationLength);
                                copyLoc++;
                            }
                            else
                            {
                                Console.WriteLine($"%%%%%%%%%%%%%%%数据采集异常&&&&&&&&&&&&&&&");
                            }
                        }
                        if (copyLoc < dealCount)
                        {
                            continue;
                        }
                        copyLoc = 0;
#if DEBUG
                        LogHelper.WriteLine($"ProcessPointSortingV1_2--从队列中取{dealCount}条数据的时间：" + (DateTime.Now.Ticks - start_time));
#endif
                        var (tags, im) = CudaAccelerator.Shared.PLSSClassifyByCorrection(model, dealCount, Samples, grabDatas);
#if DEBUG
                        LogHelper.WriteLine($"ProcessPointSortingV1_2--执行GPU相关操作的时间：" + (DateTime.Now.Ticks - start_time));
#endif
                        ejectDataQueue.Enqueue(tags);
                        displayDataQueue.Enqueue(im);
                        SortingResultEvent.Set();
                        SortingResultDisplayEvent.Set();
#if DEBUG
                        LogHelper.WriteLine($"ProcessPointSortingV1_2--保存图像的时间：" + (DateTime.Now.Ticks - start_time));
#endif
#if DEBUG
                        LogHelper.WriteLine($"ProcessPointSortingV1_2--###### 总时间：" + (DateTime.Now.Ticks - start_time));
                        //LogHelper.WriteLine((DateTime.Now.Ticks - start_time));
                        //LogHelper.WriteLine($"%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
#endif
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                LogHelper.WriteLine("实时分选每帧数据异常：" + ex.Message);
#endif
            }
            finally
            {
#if DEBUG
                LogHelper.AppendToFile();
#endif
                GlobalSettings.ApplySetting.stopAppFlag = false;
                GlobalSettings.ApplySetting.runingApp = string.Empty;
                GC.Collect();
            }
        }

        //发送吹气指令
        private void SendEjectData(object obj)
        {
            try
            {
                var model = obj as Model;
                long start_time = 0;
                int allSamples = (int)ScanParaMeter.parminfo.SpatialPixelNumCur;
                int startIndex = GlobalSettings.ApplySetting.StartSampleIndex;
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    SortingResultEvent.WaitOne();
                    if (ejectDataQueue.Count > 0)
                    {
#if DEBUG
                        LogHelper.WriteLine($"SendEjectData--###### 有数据需要发送到吹气：{ejectDataQueue.Count},时间：{(DateTime.Now.Ticks - start_time)}");
#endif
                        start_time = DateTime.Now.Ticks;
                        if (!ejectDataQueue.TryDequeue(out byte[,] tags))
                        {
                            continue;
                        }
#if DEBUG
                        LogHelper.WriteLine($"SendEjectData-取数据时间：" + (DateTime.Now.Ticks - start_time));
#endif
                        #region 加入腐蚀功能
                        if (model.BackgroundCorrection)
                        {
                            int line = tags.GetLength(0);
                            int pixlength = tags.GetLength(1);
                            int numMove = 3;//腐蚀几个像素
                            int[] record = new int[numMove];
                            for (int i = 0; i < line; i++) // GetLength(0) 获取第一维的长度
                            {
                                for (int j = 0; j < pixlength; j++) // GetLength(1) 获取第二维的长度
                                {
                                    if (j < numMove)
                                    {

                                    }
                                    else 
                                    {
                                        //只需要清除有误判的情况  -++ 或 ++-  >255且<510
                                        for (int p = 0; p < numMove; p++)
                                        {
                                            record[p] = tags[i, j - (3 - p)];
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        if (GlobalSettings.ApplySetting.CommunicationType == 0)
                        {
                            SendClient(tags, startIndex, GlobalSettings.ApplySetting.CustomSamples);
                        }
                        else if (GlobalSettings.ApplySetting.CommunicationType == 1)
                        {
                            SendToEject(tags, startIndex, GlobalSettings.ApplySetting.CustomSamples);
                        }
                        else if (GlobalSettings.ApplySetting.CommunicationType == 2)
                        {
                            try
                            {
                                SendCsvFile(tags, startIndex, GlobalSettings.ApplySetting.CustomSamples);

                            }
                            catch (Exception ex)
                            {
                                LogHelper.WriteLog("保存显示图像" + ex.Message);
                            }
                        }
#if DEBUG
                        LogHelper.WriteLine($"SendEjectData-发送时间：" + (DateTime.Now.Ticks - start_time));
#endif
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                LogHelper.WriteLine("实时分选每帧数据异常：" + ex.Message);
#endif
            }
            finally
            {
#if DEBUG
                LogHelper.AppendToFile();
#endif
                GlobalSettings.ApplySetting.stopAppFlag = false;
                GlobalSettings.ApplySetting.runingApp = string.Empty;
                GC.Collect();
            }
        }
        /// <summary>
        /// 保存显示图像
        /// </summary>
        /// <param name="obj"></param>
        private void SaveDisplayData(object obj)
        {
            try
            {
                long start_time = 0;
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    SortingResultDisplayEvent.WaitOne();
                    if (displayDataQueue.Count > 0)
                    {
#if DEBUG
                        LogHelper.WriteLine($"SaveDisplayData--###### 有数据需要发送到图片显示队列：{displayDataQueue.Count},时间：{(DateTime.Now.Ticks - start_time)}");
                        start_time = DateTime.Now.Ticks;
#endif
                        if (!displayDataQueue.TryDequeue(out byte[,] im))
                        {
                            continue;
                        }
#if DEBUG
                        LogHelper.WriteLine($"SaveDisplayData--###### 取数据时间：{displayDataQueue.Count},时间：{(DateTime.Now.Ticks - start_time)}");
#endif
                        SaveQueueRgbData(im);
#if DEBUG
                        LogHelper.WriteLine($"SaveDisplayData--###### 数据转化时间：{displayDataQueue.Count},时间：{(DateTime.Now.Ticks - start_time)}");
#endif
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                LogHelper.WriteLine("实时分选每帧数据异常：" + ex.Message);
#endif
            }
            finally
            {
#if DEBUG
                LogHelper.AppendToFile();
#endif
                GC.Collect();
            }
        }

        /*
        private void ProcessPointSortingV2_1(object obj)
        {
            try
            {
                int needNum = 100;
                //存储数据集合
                List<GrabData> data = new List<GrabData>();
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    if (byteQueue.Count >= needNum)
                    {
                        data.Clear();
                        do
                        {
                            if (byteQueue.TryDequeue(out var d))
                            {
                                data.Add(d);
                            }
                        }
                        while (data.Count < needNum);


                        //float[][] needData = new float[needNum][];  //反射率
                        byte[][] orgData = new byte[needNum][]; //原始数据
            
                        Parallel.For(0, needNum, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (int i) =>
                        {
                            orgData[i] = data[i].Values;
                        });

                        processedDataQueue.Enqueue(new ProcessedData() { Length = needNum, ImageBytes = null, RefDatas = null, OrgDatas = orgData });
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("实时分选每帧数据，第一步处理异常：" + ex.Message);
            }
            finally
            {
                GC.Collect();
                GlobalSettings.ApplySetting.stopAppFlag = false;
                GlobalSettings.ApplySetting.runingApp = string.Empty;
            }
        }

        /// <summary>
        /// 点分选
        /// </summary>
        /// <param name="obj"></param>
        private void ProcessPointSortingV2_2(object obj)
        {
            try
            {
                var model = obj as Model;
                int mergeNum = Properties.Settings.Default.merge;//合并个数
                int erode = Properties.Settings.Default.Erode;//腐蚀

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    if (processedDataQueue.Count > 0)
                    {
                        Console.WriteLine("---当前帧数1：" + byteQueue.Count + "---当前帧数2：" + processedDataQueue.Count);
                        stopwatch.Restart();
                        ProcessedData data = null;
                        while (data == null)
                        {
                            if (processedDataQueue.TryDequeue(out data))
                            {
                                //for (int i = 0; i < data.OrgDatas.Length; i++)
                                //{
                                //    TrySavePartialOrgData(data.OrgDatas[i]);
                                //}
                                break;
                            }
                        }
                        Console.WriteLine("保存原始数据时间：" + stopwatch.ElapsedMilliseconds);
                        int needNum = data.Length;
                        var needdata = data.OrgDatas;
                        var cTags = new byte[needNum][];
                        for (int i = 0; i < needNum; i++)
                        {
                            var tags = CudaAccelerator.Shared.PLSSClassifyByCorrection(model, Samples, needdata[i]);
                            cTags[i] = tags;
                        }
                        //Parallel.For(0, needNum, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (int i) =>
                        //{
                        //    var tags = CudaAccelerator.Shared.PLSSClassifyByCorrection(model, Samples, needdata[i]);
                        //    cTags[i] = tags;
                        //});

                        Console.WriteLine("100帧总的分选数据时间：" + stopwatch.ElapsedMilliseconds);
                        cTags = new TrainFormHelper().ErodeV2(erode, cTags);
                        for (int f = 0; f < needNum; f++)
                        {
                            SendClient(cTags[f]);
                            SaveQueuePLSDAResult(cTags[f]);
                        }
                        Console.WriteLine("分选总的：" + stopwatch.ElapsedMilliseconds);
                    }

                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("实时分选每帧数据，第二步处理异常：" + ex.Message);
            }
            finally
            {
                GC.Collect();
                GlobalSettings.ApplySetting.stopAppFlag = false;
                GlobalSettings.ApplySetting.runingApp = string.Empty;
            }
        }

        */

        #endregion

        #region 按轮廓进行分选
        /*
        /// <summary>
        /// 预处理数据（转反射率或RGB）
        /// </summary>
        /// <param name="obj"></param>
        private void ProcessContourSortingV1_1(object obj)
        {
            try
            {
                int needNum = 350;
                var model = obj as Model;
                Stopwatch stopwatch = new Stopwatch();
                //存储数据集合
                List<GrabData> data = new List<GrabData>();
                while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
                {
                    if (byteQueue.Count >= needNum)
                    {
                        stopwatch.Restart();
                        data.Clear();

                        do
                        {
                            if (byteQueue.TryDequeue(out var d))
                            {
                                data.Add(d);
                            }
                        }
                        while (data.Count < needNum);

                        float[][] needData = new float[needNum][];  //反射率

                        Parallel.For(0, needNum, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (int i) =>
                        {
                            needData[i] = data[i].Values.ToFloat(datatype);
                        });
                        processedDataQueue.Enqueue(new ProcessedData() { Length = needNum, ImageBytes = null, RefDatas = needData, OrgDatas = null });
                        stopwatch.Stop();
                        Console.WriteLine("300帧数据中处理时间：" + stopwatch.ElapsedMilliseconds);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("轮廓分选每帧数据V1，第一步处理异常：" + ex.Message);
            }
            finally
            {
                GC.Collect();
                GlobalSettings.ApplySetting.stopAppFlag = false;
                GlobalSettings.ApplySetting.runingApp = string.Empty;
            }
        }

        private void ProcessContourSortingV1_2(object obj)
        {
            var model = obj as Model;
            var rgbChannel = (model.ImgR, model.ImgG, model.ImgB);
            var rgbThreshold = (model.ImgThresholdR, model.ImgThresholdG, model.ImgThresholdB);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //int nameId = 1;
            while (StartFlag && !GlobalSettings.ApplySetting.stopAppFlag)
            {
                if (processedDataQueue.Count > 0)
                {
                    stopwatch.Restart();
                    if (processedDataQueue.TryDequeue(out var data))
                    {
                        Console.WriteLine("------------------当前未处理帧数：" + byteQueue.Count + "---------------当前将处理帧数：" + processedDataQueue.Count);

                        int needNum = data.Length;
                        byte[][] imageByte = data.ImageBytes;
                        int stride = needNum * 3;
                        Parallel.For(0, needNum, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (int i) =>
                        {
                            data.RefDatas[i] = CudaAccelerator.Shared.CalibrationDatas(Samples, data.RefDatas[i], 0);
                            imageByte[i] = new byte[Samples * needNum * 3];
                            for (int y = 0; y < Samples; y++)
                            {
                                var offsetR = y + rgbChannel.ImgR * Samples;
                                var offsetG = y + rgbChannel.ImgG * Samples;
                                var offsetB = y + rgbChannel.ImgB * Samples;
                                imageByte[i][y * 3] = (data.RefDatas[i][offsetB] / rgbThreshold.ImgThresholdB * 255).ToByte();
                                imageByte[i][y * 3 + 1] = (data.RefDatas[i][offsetG] / rgbThreshold.ImgThresholdG * 255).ToByte();
                                imageByte[i][y * 3 + 2] = (data.RefDatas[i][offsetR] / rgbThreshold.ImgThresholdR * 255).ToByte();
                            }
                        });

                        byte[][] cTags = new byte[needNum][];
                        for (int i = 0; i < needNum; i++)
                        {
                            cTags[i] = new byte[Samples];
                        }

                        //寻找轮廓
                        var bitmap = imageByte.ToBitmap(needNum, Samples);
                        //nameId = nameId > 10 ? 1 : nameId;
                        //bitmap.Save($@"D:\{nameId++}.bmp");
                        Console.WriteLine("RGB转bitmap时间：" + stopwatch.ElapsedMilliseconds);
                        var contours = OpenCV.FindContourPoints(bitmap, model.ContourThreshold, model.ContourErode);
                        Console.WriteLine("寻找轮廓时间：" + stopwatch.ElapsedMilliseconds);
                        if (contours.Length > 0)
                        {
                            float[,] contourData = new float[contours.Length, model.EndBandIndex - model.StartBandIndex + 1];

                            Parallel.For(0, contours.Length, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (int i) =>
                            {
                                for (int j = model.StartBandIndex; j <= model.EndBandIndex; j++)
                                {
                                    float sum = 0f;
                                    foreach (var point in contours[i])
                                    {
                                        sum += data.RefDatas[point.X][point.Y + j * Samples];
                                    }
                                    contourData[i, j - model.StartBandIndex] = sum / contours[i].Length;
                                }
                            });

                            Console.WriteLine("计算平均数据时间：" + stopwatch.ElapsedMilliseconds);
                            var tags = CudaAccelerator.Shared.OutlineClassify(model, contours.Length, contourData);
                            Console.WriteLine("分选时间：" + stopwatch.ElapsedMilliseconds);
                            for (int i = 0; i < contours.Length; i++)
                            {
                                if (tags[i] > 0)
                                {
                                    foreach (var point in contours[i])
                                    {
                                        cTags[point.X][point.Y] = tags[i];
                                    }
                                }
                            }
                        }

                        for (int f = 0; f < needNum; f++)
                        {

                            //发送给气吹程序
                            SendClient(cTags[f]);
                            //保存结果
                            SaveQueuePLSDAResult(cTags[f]);

                        }
                        stopwatch.Stop();
                        Console.WriteLine("总分选计算时间：" + stopwatch.ElapsedMilliseconds);
                    }
                }
            }
        }

        */
        #endregion

        #region 发送到吹气或保存结果

        private void SendToEject(byte[,] tags, int startIndex, int length)
        {
            int length1 = tags.GetLength(0);
            int length2 = tags.GetLength(1);
            for (int i = 0; i < length1; i++)
            {
                try
                {
                    long xx = DateTime.Now.Ticks;
                    timeDataQueue.TryDequeue(out var timedata1);
                    byte[] buf = new byte[length2];
                    int destinationIndex = length2 * i;
                    Buffer.BlockCopy(tags, destinationIndex + startIndex, buf, startIndex, length);
                    NotificationAction.SendEjectData(buf, timedata1);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }

        /// <summary>
        /// 发送给气吹程序
        /// </summary>
        /// <param name="tags"></param>
        private void SendClient(byte[,] tags, int startIndex, int length)
        {
            if (client != null && client.isConnect)
            {
                int length1 = tags.GetLength(0);
                int length2 = tags.GetLength(1);
                for (int i = 0; i < length1; i++)
                {
                    try
                    {
                        //传输数据给服务器
                        //数据内容
                        byte[] buf = new byte[700];
                        buf[0] = 0xee;//表头
                        buf[1] = 0x01;//指令
                        buf[2] = 0x01;//指令1
                        buf[3] = 0x01;//指令2
                        BitConverter.GetBytes(length2).CopyTo(buf, 4);//数据的总长度
                        timeDataQueue.TryDequeue(out var timedata1);
                        BitConverter.GetBytes(timedata1).CopyTo(buf, 8);//时间1
                        int destinationIndex = length2 * i;
                        Buffer.BlockCopy(tags, destinationIndex + startIndex, buf, 16 + startIndex, length);
                        client.Send(buf);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        private void SendCsvFile(byte[,] tags, int startIndex, int length)
        {
            int length1 = tags.GetLength(0);
            int length2 = tags.GetLength(1);
            for (int i = 0; i < length1; i++)
            {
                byte[] buf = new byte[length2];
                Buffer.BlockCopy(tags, length2 * i + startIndex, buf, startIndex, length);
                _fileStorageHelper.AddData(buf);
            }
        }

        /// <summary>
        /// 保存RGB队列数据
        /// </summary>
        /// <param name="dataRGB"></param>
        private void SaveQueueRgbData(byte[] dataRGB)
        {
            if (displayRgbQueue.Count >= needLines)
            {
                byte[] deleData;
                if (displayRgbQueue.TryDequeue(out deleData))
                {
                    displayRgbQueue.Enqueue(dataRGB);
                }
            }
            else
            {
                displayRgbQueue.Enqueue(dataRGB);
            }
        }

        /// <summary>
        /// 保存RGB队列数据
        /// </summary>
        /// <param name="dataRGB"></param>
        private void SaveQueueRgbData(byte[,] dataRGB)
        {
            int count = dataRGB.GetLength(0);
            int length = dataRGB.GetLength(1);
            for (int i = 0; i < count; i++)
            {
                byte[] buf = new byte[length];
                Buffer.BlockCopy(dataRGB, i * length, buf, 0, length);
                if (displayRgbQueue.Count >= needLines)
                {
                    if (displayRgbQueue.TryDequeue(out var deleData))
                    {
                        displayRgbQueue.Enqueue(buf);
                    }
                }
                else
                {
                    displayRgbQueue.Enqueue(buf);
                }
            }
        }

        /// <summary>
        /// 保存SPE文件的数据
        /// </summary>
        /// <param name="isPartSave">是否只保存最后一部分</param>
        /// <param name="data"></param>
        private void SaveQueueSpeData(bool isPartSave, byte[] data)
        {
            if (isPartSave)
            {
                if (saveToSpeFileQueue.Count >= needLines)
                {
                    if (saveToSpeFileQueue.TryDequeue(out byte[] deleData))
                    {
                        saveToSpeFileQueue.Enqueue(data);
                    }
                }
                else
                {
                    saveToSpeFileQueue.Enqueue(data);
                }
            }
            else
            {
                saveToSpeFileQueue.Enqueue(data);
            }
        }

        private void SaveQueueSpeData(bool isPartSave, float[,] data)
        {
            int count = data.GetLength(0);
            int length = data.GetLength(1) * 4;

            for (int i = 0; i < count; i++)
            {
                byte[] buf = new byte[length];
                Buffer.BlockCopy(data, i * length, buf, 0, length);
                if (isPartSave)
                {
                    if (saveToSpeFileQueue.Count >= needLines)
                    {
                        if (saveToSpeFileQueue.TryDequeue(out byte[] deleData))
                        {
                            saveToSpeFileQueue.Enqueue(buf);
                        }
                    }
                    else
                    {
                        saveToSpeFileQueue.Enqueue(buf);
                    }
                }
                else
                {
                    saveToSpeFileQueue.Enqueue(buf);
                }
            }

        }

        #endregion



        #endregion


        public void Dispose()
        {
            spe.SpeDispose();
            CalibrateSPE.SpeDispose();
        }
    }



}
