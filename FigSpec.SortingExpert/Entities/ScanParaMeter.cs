using Cameras;
using Cameras.Factory;
using FigSpec.LightSource.Structs;
using FigSpec.SortingExpert.ModelFiles;
using FigSpec.SortingExpert.Tools;
using FigSpec.Spectral;
using Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FigSpec.SortingExpert.Entities
{
    public static class ScanParaMeter
    {
        /// <summary>
        /// 连接到设备的相机列表
        /// </summary>
        public static List<ICameraInfo> cameraInfos;
        /// <summary>
        /// 连接到设备的相机实例
        /// </summary>
        public static Camera camera { get; set; }
        /// <summary>
        /// 连接到设备的相机参数信息
        /// </summary>
        public static ParmInfo parminfo { get; set; }
        /// <summary>
        /// 光源参数
        /// </summary>
        public static LightSource.LightSource lightSource { get; set; }
        /// <summary>
        /// 定时器  使用定时器，图像显示不流畅
        /// </summary>
        public static System.Windows.Forms.Timer ImgTimer { get; set; }
        /// <summary>
        /// 光源是否开启
        /// </summary>
        public static bool IsLight { get; set; }

        /// <summary>
        /// 全部波段，点、线的反射率文件:一个波长，一个反射率信号量
        /// </summary>
        public static (float[] WaveLength, float[] Signal) tupleWhiteRef = (new float[1], new float[1]);
        public static (float[] WaveLength, float[] Signal) tupleBlackRef = (new float[1], new float[1]);

        /// <summary>
        /// Scan的校准数据
        /// </summary>
        public static CorrectionData ScanCorrectionData { get; set; }
        /// <summary>
        /// Sort的校准数据
        /// </summary>
        public static CorrectionData SortCorrectionData { get; set; }

        /// <summary>
        /// 表示采集的图像是否正在写入磁盘
        /// </summary>
        public static bool IsSavingImage { get; set; } = false;

#if DEBUG
        /// <summary>
        /// 已经采集到的帧数
        /// </summary>
        public static long ReceivedLines { get; set; }
        public static long StartReceivedTime { get; set; }
        public static long StartReceivedLines { get; set; }
        public static long FrameFrequency { get; set; }
        public static long SpendTime { get; set; }

#endif



        /// <summary>
        /// <summary>
        /// 线的暗电流数据
        /// </summary>
        public static byte[] DarkCurrent_ReadBytes { get; set; }

        public static ClientControl client = new ClientControl();
        public static CollectData collectData;

        public static void OpenCamera(bool isScan, Action<string> action = null)
        {
            if (camera == null || !camera.IsOpen)
            {
                Camera camera;
                //打开相机 
                try
                {
                    //通过调用 GetCameras 方法，获取该电脑已连接的相机列表                   
                    cameraInfos = CameraEnumerate.GetCameras(GetCameraParam());
                    if (cameraInfos.Count == 0)
                    {
                        return;
                    }
                    else
                    {
                        camera = CameraEnumerate.InitCamera(cameraInfos[0]);
                        camera.OpenCamera();
                        ScanParaMeter.camera = camera;

                        parminfo = camera.GetParmInfo();
                        //检查相机是否与配置文件匹配
                        string res = CheckCamera(isScan, camera);
                        action?.Invoke(res);

                        ResetCameraInfo(isScan);

                        var cameraSetting = isScan ? GlobalSettings.ApplySetting.ScanCameraSetting : GlobalSettings.ApplySetting.SortCameraSetting;
                        int datatype = (parminfo.PixelFormatCur == EnumPixelFormat.Mono8) ? 1 : 12;
                        collectData?.Dispose();
                        collectData = new CollectData(client, cameraSetting.DisplayFrameNum, datatype);
                    }
                }
                catch (Exception ex)
                {
                    FormShowHelper.ShowMessage(ex.ToString(), "提示".ToMultiLanguage());
                }
            }
        }

        public static void ResetCameraInfo(bool isScan)
        {
            var cameraSetting = isScan ? GlobalSettings.ApplySetting.ScanCameraSetting : GlobalSettings.ApplySetting.SortCameraSetting;

            //设置像素格式
            if (ScanParaMeter.camera.SetPixelFormat(EnumPixelFormat.Mono14))
                Console.WriteLine("设置像素格式为Mono14");
            else 
                Console.WriteLine("设置像素格式失败");

            if (isScan)
            {
                if (ScanParaMeter.camera.SetRunTypeAll())
                    Console.WriteLine("设置ALL模式");
            }

            //设置曝光时间
            if (ScanParaMeter.camera.SetExposureTime(cameraSetting.ExposureTime))//Model.ExpTime
                Console.WriteLine("设置曝光时间成功");
            else 
                Console.WriteLine("设置曝光时间失败");



            //设置触发模式
            if (ScanParaMeter.camera.SetTriggerMode((EnumTriggerMode)cameraSetting.TriggerMode))
                Console.WriteLine("设置触发模式成功");
            else 
                Console.WriteLine("设置触发模式失败");

            RefreshFrameRate(isScan);

            //0高增益 1 中 2 低
            if (ScanParaMeter.camera.SetGain(cameraSetting.Gain))
                Console.WriteLine("设置增益成功");
            else
                Console.WriteLine("设置增益失败");

            if (ScanParaMeter.camera.SetReverseSpatialPixel(cameraSetting.FlipXFlag))
                Console.WriteLine("设置镜像采集成功");
            else
                Console.WriteLine("设置镜像采集失败");

        }


        public static float RefreshFrameRate(bool isScan)
        {
            var cameraSetting = isScan ? GlobalSettings.ApplySetting.ScanCameraSetting : GlobalSettings.ApplySetting.SortCameraSetting;

            //设置帧频
            parminfo = camera.GetParmInfo();
            float framRate = cameraSetting.FrameRate;
            if (cameraSetting.IsInitialData)
            {
                //初始连接，设置最大帧频
                framRate = parminfo.FrameRateMax;
                cameraSetting.IsInitialData = false;
            }
            else
            {
                framRate = framRate < parminfo.FrameRateMax ? framRate : parminfo.FrameRateMax;
            }
            cameraSetting.FrameRate = (int)framRate;
            if (ScanParaMeter.camera.SetFrameRate(framRate))
                Console.WriteLine("设置帧频成功");
            else
                Console.WriteLine("设置帧频失败");
            return framRate;
        }



        /// <summary>
        /// 设置ROI
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool SetRunTypeBank(Model model)
        {
            bool isSuc = SetRunTypeBank(model, model.StartBandIndex, model.EndBandIndex);
            if (isSuc)
            {
                //如果成功包含了所有波段，则更新最新的在ROI中的起始索引
                for (int i = 0; i < parminfo.SpectralChannelWavelength.Length; i++)
                {
                    if (parminfo.SpectralChannelWavelength[i] >= model.HdrWaveLength[model.StartBandIndex])
                    {
                        model.RoiBandStartIndex = i;
                        break;
                    }
                }
            }
            return isSuc;
        }

        /// <summary>
        /// 设置ROI
        /// </summary>
        /// <param name="model"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        private static bool SetRunTypeBank(Model model, int startIndex, int endIndex)
        {
            startIndex = startIndex < 0 ? 0 : startIndex;
            endIndex = endIndex >= model.HdrWaveLength.Length ? model.HdrWaveLength.Length - 1 : endIndex;
            parminfo.BanklistCur[0].EnableBank(model.HdrWaveLength[startIndex], model.HdrWaveLength[endIndex]);
            bool isSuc = camera.SetRunTypeBank(parminfo.BanklistCur);
            if (!isSuc)
                return false;
            parminfo = camera.GetParmInfo();
            if (parminfo.SpectralChannelWavelength[0] > model.HdrWaveLength[startIndex])
            {
                startIndex--;
                if (!SetRunTypeBank(model, startIndex, endIndex))
                {
                    return false;
                }
            }
            if (parminfo.SpectralChannelWavelength[parminfo.SpectralChannelWavelength.Length - 1] < model.HdrWaveLength[endIndex])
            {
                endIndex++;
                if (!SetRunTypeBank(model, startIndex, endIndex))
                {
                    return false;
                }
            }
            return true;
        }

        public static void RefreshCameraImgRGB(bool isScan)
        {
            var cameraSetting = isScan ? GlobalSettings.ApplySetting.ScanCameraSetting : GlobalSettings.ApplySetting.SortCameraSetting;
            float minRvalue = Math.Abs(cameraSetting.ImgFR - parminfo.SpectralChannelWavelength[0]);
            float minGvalue = Math.Abs(cameraSetting.ImgFG - parminfo.SpectralChannelWavelength[0]);
            float minBvalue = Math.Abs(cameraSetting.ImgFB - parminfo.SpectralChannelWavelength[0]);
            float FR = parminfo.SpectralChannelWavelength[0];
            float FG = parminfo.SpectralChannelWavelength[0];
            float FB = parminfo.SpectralChannelWavelength[0];
            cameraSetting.ImgR = 0;
            cameraSetting.ImgG = 0;
            cameraSetting.ImgB = 0;
            for (int i = 0; i < parminfo.SpectralChannelWavelength.Length; i++)
            {
                float tempR = Math.Abs(cameraSetting.ImgFR - parminfo.SpectralChannelWavelength[i]);

                if (tempR < minRvalue)
                {
                    minRvalue = tempR;
                    FR = parminfo.SpectralChannelWavelength[i];
                    cameraSetting.ImgR = i;
                }
                float tempG = Math.Abs(cameraSetting.ImgFG - parminfo.SpectralChannelWavelength[i]);
                if (tempG < minGvalue)
                {
                    minGvalue = tempG;
                    FG = parminfo.SpectralChannelWavelength[i];
                    cameraSetting.ImgG = i;
                }
                float tempB = Math.Abs(cameraSetting.ImgFB - parminfo.SpectralChannelWavelength[i]);
                if (tempB < minBvalue)
                {
                    minBvalue = tempB;
                    FB = parminfo.SpectralChannelWavelength[i];
                    cameraSetting.ImgB = i;
                }

            }
            cameraSetting.ImgFR = FR;
            cameraSetting.ImgFG = FG;
            cameraSetting.ImgFB = FB;
        }


        /// <summary>
        /// 相机修改后更新波长信息
        /// 校准文件检查
        /// </summary>
        /// <param name="camera"></param>
        private static string CheckCamera(bool isScan, Camera camera)
        {
            var cameraSetting = isScan ? GlobalSettings.ApplySetting.ScanCameraSetting : GlobalSettings.ApplySetting.SortCameraSetting;
            //相机序列号不同时，更新选择的波段
            if (cameraSetting.CameraSN != camera.Info.InstrumentSN)
            {
                cameraSetting.CameraSN = camera.Info.InstrumentSN;
                RefreshCameraImgRGB(isScan);
            }

            if (isScan)
            {
                var setting = GlobalSettings.ApplySetting;
                //校准文件
                if (!File.Exists(setting.scanSet.calibrationInfo.whiteRawFilePath) || !File.Exists(setting.scanSet.calibrationInfo.blackRawFilePath))
                {
                    return "缺少校准文件，需重新校准".ToMultiLanguage();
                }
                var correctionWData = CorrectionHelper.GetCorrection(setting.scanSet.calibrationInfo.whiteRawFilePath);
                var correctionBData = CorrectionHelper.GetCorrection(setting.scanSet.calibrationInfo.blackRawFilePath);
                correctionBData.Dispose();
                correctionWData.Dispose();

                if (correctionBData.Header.ExposureTime != cameraSetting.ExposureTime
                 || correctionWData.Header.ExposureTime != cameraSetting.ExposureTime
                 || correctionBData.Header.Gain != cameraSetting.Gain
                 || correctionWData.Header.Gain != cameraSetting.Gain)
                {
                    return "增益或曝光时间已修改，请确定是否需要重新校准".ToMultiLanguage();
                }

                if (!correctionBData.Header.CameraSN.Equals(cameraSetting.CameraSN)
                    || !correctionWData.Header.CameraSN.Equals(cameraSetting.CameraSN))
                {
                    try
                    {
                        if (File.Exists(setting.scanSet.calibrationInfo.whiteRawFilePath))
                        {
                            File.Delete(setting.scanSet.calibrationInfo.whiteRawFilePath);
                        }
                        if (File.Exists(setting.scanSet.calibrationInfo.blackRawFilePath))
                        {
                            File.Delete(setting.scanSet.calibrationInfo.blackRawFilePath);
                        }
                    }
                    catch (Exception)
                    {

                    }

                    return "相机与校准文件不匹配，需重新校准".ToMultiLanguage();
                }
            }

            return null;
        }

        /// <summary>
        /// 获取相机配置文件
        /// </summary>
        /// <returns></returns>
        public static string[] GetCameraSettingFiles()
        {
            var path = GlobalSettings.ApplyInfo.ApplyParamPath;
            string[] files = Directory.GetFiles(path, "*.figspecinf");
            return files;
        }

        /// <summary>
        /// 获取相机配置并发送给sdk
        /// </summary>
        /// <returns></returns>
        private static List<FigSpecInf> GetCameraParam()
        {
            List<FigSpecInf> infs = new List<FigSpecInf>();
            var files = GetCameraSettingFiles();
            if (files.Length == 0)
            {
                if (FormShowHelper.ShowMessage("未找到相机配置文件，是否导入？（注: 相机配置文件一般在相机配套的U盘中）".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) != DialogResult.OK)
                    return infs;
                ImportCameraSetting(false);
            }
            files = GetCameraSettingFiles();
            foreach (var file in files)
            {
                infs.Add(FigSpecInf.FromFile(file));
            }
            return infs;
        }

        /// <summary>
        /// 导入相机配置文件
        /// </summary>
        /// <param name="isShowTip"></param>
        public static bool ImportCameraSetting(bool isShowTip = true)
        {
            bool isSuc = false;
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Title = "请选择相机配置文件".ToMultiLanguage(),
                    Filter = "figspecinf files (*.figspecinf)|*.figspecinf",
                    Multiselect = true
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string FileName in dialog.FileNames)
                    {
                        string filePath = Path.Combine(GlobalSettings.ApplyInfo.ApplyParamPath, Path.GetFileName(FileName));
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        File.Copy(FileName, filePath);

                        //拷贝一份给c++分选程序
                        string filePath1 = Environment.CurrentDirectory + "\\fastsorting";
                        filePath1 = Path.Combine(filePath1, Path.GetFileName(FileName));
                        File.Copy(FileName, filePath1);
                        isSuc = true;
                    }
                    if(isShowTip)
                        FormShowHelper.ShowMessage("相机配置文件导入成功!".ToMultiLanguage(), "提示".ToMultiLanguage());
                }
            }
            catch (Exception ex)
            {

            }
            return isSuc;
        }
    }

    /// <summary>
    /// 校准数据
    /// </summary>
    public class CorrectionData
    {
        /// <summary>
        /// 1帧白数据反射率矩阵-存储标准白板每个光谱通道对应的反射率
        /// </summary>
        public float[] k_White { get; set; }
        public float[] k_Black { get; set; }
        /// <summary>
        /// 黑校准一帧数据
        /// </summary>
        public float[] Black_ReadBytes { get; set; }
        /// <summary>
        /// 线的白校准数据
        /// </summary>
        public float[] White_ReadBytes { get; set; }


        //校准时的帧频
        public int White_FrameRate { get; set; }
        /// <summary>
        /// 校准时的曝光时间
        /// </summary>
        public int White_ExposureTime { get; set; }
        //校准时的帧频
        public int Black_FrameRate { get; set; }
        /// <summary>
        /// 校准时的曝光时间
        /// </summary>
        public int Black_ExposureTime { get; set; }
    }
}
