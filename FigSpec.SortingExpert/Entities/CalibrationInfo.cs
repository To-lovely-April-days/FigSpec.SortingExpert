using FigSpec.SortingExpert.ModelFiles;
using FigSpec.SortingExpert.SettingForms;
using FigSpec.SortingExpert.Tools;
using FigSpec.Spectral;
using FigSpec.Spectral.Extensions;
using Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Hyperspectral.ReflCharFile.CorrectionHeader;

namespace FigSpec.SortingExpert.Entities
{
    public class CalibrationInfo
    {
        /// <summary>
        /// 白校准系数文件路径
        /// </summary>
        public string whiteRefFilePath 
        {
            get
            {
                return GlobalSettings.ApplyInfo.ApplyParamPath  + "WhiteBoardSpectralData.xlsx";
            }
        }
        /// <summary>
        /// 白校准原始数据文件路径
        /// </summary>
        public string whiteRawFilePath
        {
            get
            {
                return GlobalSettings.ApplySetting.FolderPathOfBrowse + "\\" + "白校准文件".ToMultiLanguage() + ".figspecwhite";
            }
        }
        /// <summary>
        /// 黑校准原始数据文件路径
        /// </summary>
        public string blackRawFilePath
        {
            get
            {
                return GlobalSettings.ApplySetting.FolderPathOfBrowse + "\\" + "黑校准文件".ToMultiLanguage() + ".figspecblack";
            }
        }



        /// <summary>
        /// 加载白校准系数文件，如果已经加载过就不再加载
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool TryLoadWhiteRefFile(string filepath, out string content)
        {
            content = null;
            if (ScanParaMeter.tupleWhiteRef.WaveLength.Length > 1)
            {
                return true;
            }
            return LoadWhiteRefFile(filepath, out content);
        }

        /// <summary>
        /// 加载白校准系数文件，每次加载
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool LoadWhiteRefFile(string filepath, out string content)
        {
            if (filepath == "")
            {
                content = "请选择白校准系数文件".ToMultiLanguage();
                return false;
            }
            if (!File.Exists(filepath))
            {
                content = "白校准系数文件不存在".ToMultiLanguage();
                return false;
            }
            try
            {
                ScanParaMeter.tupleWhiteRef = ExcelHelper.ReadExcelColumns(filepath);

                //同时设置黑校准
                ScanParaMeter.tupleBlackRef = ((float[])ScanParaMeter.tupleWhiteRef.WaveLength.Clone(), new float[ScanParaMeter.tupleWhiteRef.Signal.Length]);
                content = "白校准系数文件加载成功".ToMultiLanguage();
                return true;
            }
            catch(Exception ex)
            {
                content = "解析白校准系数文件失败，请检查".ToMultiLanguage();
                return false;
            }
        }

        /// <summary>
        /// 加载线文件
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool LoadWriteRawFile(string filepath, out string content)
        {
            if (filepath == "")
            {
                content = "请选择白校准文件".ToMultiLanguage();
                return false;
            }
            if (!File.Exists(filepath))
            {
                content = "白校准文件不存在".ToMultiLanguage();
                return false;
            }
            try
            {
                var correctionData = CorrectionHelper.GetCorrection(filepath);
                float[] WhiteFileWavelength = correctionData.Header.WaveLength;
                float[] WhiteFileSignal = correctionData.GetLineData(0);
                correctionData.Dispose();

                if (correctionData.Header.Type != CorrectionType.Line)
                {
                    content = "白校准文件类型不是线".ToMultiLanguage();
                    return false;
                }

                if (WhiteFileWavelength.Count() < ScanParaMeter.parminfo.SpectralChannelNumCur)
                {
                    content = "白校准文件的光谱通道不符合，请重新选择".ToMultiLanguage();
                    return false;
                }
                else if (WhiteFileWavelength.Count() > ScanParaMeter.parminfo.SpectralChannelNumCur)
                {
                    //这里需要改，挑出用户需要的光谱通道并应用，当前不知道怎么挑
                    content = "白校准文件的光谱通道不符合，请重新选择".ToMultiLanguage();
                    return false;
                }
                uint size = ScanParaMeter.parminfo.SpectralChannelNumCur * ScanParaMeter.parminfo.SpatialPixelNumCur;
                if (WhiteFileSignal.Count() != size)
                {
                    content = "白校准文件的空间通道不符合，请重新选择".ToMultiLanguage();
                    return false;
                }

                content = "白校准文件加载成功".ToMultiLanguage();
                return true;
            }
            catch
            {
                content = "解析白校准文件失败，请检查".ToMultiLanguage();
                return false;
            }
        }

        /// <summary>
        /// 加载黑校准文件
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool LoadBlackRawFile(string filepath, out string content)
        {
            if (filepath == "")
            {
                content = "请选择黑校准文件".ToMultiLanguage();
                return false;
            }
            if (!File.Exists(filepath))
            {
                content = "黑校准文件不存在".ToMultiLanguage();
                return false;
            }
            try
            {
                var correctionData = CorrectionHelper.GetCorrection(filepath);
                float[] BlackFileWavelength = correctionData.Header.WaveLength;
                float[] BlackFileSignal = correctionData.GetLineData(0);
                correctionData.Dispose();

                if (correctionData.Header.Type != CorrectionType.Line)
                {
                    content = "黑校准文件类型不是线".ToMultiLanguage();
                    return false;
                }

                if (BlackFileWavelength.Count() < ScanParaMeter.parminfo.SpectralChannelNumCur)
                {
                    content = "黑校准文件的光谱通道不符合，请重新选择".ToMultiLanguage();
                    return false;
                }
                else if (BlackFileWavelength.Count() > ScanParaMeter.parminfo.SpectralChannelNumCur)
                {
                    //这里需要改，挑出用户需要的光谱通道并应用，当前不知道怎么挑
                    content = "黑校准文件的光谱通道不符合，请重新选择".ToMultiLanguage();
                    return false;
                }
                uint size = ScanParaMeter.parminfo.SpectralChannelNumCur * ScanParaMeter.parminfo.SpatialPixelNumCur;
                if (BlackFileSignal.Count() != size)
                {
                    content = "黑校准文件的空间通道不符合，请重新选择".ToMultiLanguage();
                    return false;
                }
                content = "黑校准文件加载成功".ToMultiLanguage();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                content = "解析黑校准文件失败，请检查".ToMultiLanguage();
                return false;
            }
        }

        public static bool CheckScanCalibrationData()
        {
            if (ScanParaMeter.ScanCorrectionData == null)
            {
                FormShowHelper.ShowMessage("请先进行校准".ToMultiLanguage(), "提示".ToMultiLanguage());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取白校准文件路径
        /// model为空时，使用全局设置，是Scan模块的校准文件
        /// 实时分选模块默认使用Temp .figspecwhite校准文件，
        /// 如果存在与Model名称相同的校准文件，则优先使用
        /// ROI，曝光时间，帧率修改了需要重新校准
        /// 校准时间过期了，可以选择性校准
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetWhiteRawFilePath(Model model)
        {
            string filePath;
            if (model == null)
            {
                filePath = GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRawFilePath;
            }
            else
            {
                string defaultFilePath = Path.Combine(GlobalSettings.ApplySetting.FolderPathOfBrowse, @"Models\Temp.figspecwhite");
                if (string.IsNullOrEmpty(model.ModelFileName))
                {
                    filePath = defaultFilePath;
                }
                else
                {
                    filePath = Path.Combine(GlobalSettings.ApplySetting.FolderPathOfBrowse, $@"Models\{model.ModelFileName}.figspecwhite");
                    if (!File.Exists(filePath))
                    {
                        filePath = defaultFilePath;
                    }
                }
            }

            return filePath;
        }

        public static string GetBlackRawFilePath(Model model)
        {
            string filePath;
            if (model == null)
            {
                filePath = GlobalSettings.ApplySetting.scanSet.calibrationInfo.blackRawFilePath;
            }
            else
            {
                string defaultFilePath = Path.Combine(GlobalSettings.ApplySetting.FolderPathOfBrowse, @"Models\Temp.figspecblack");
                if (string.IsNullOrEmpty(model.ModelFileName))
                {
                    filePath = defaultFilePath;
                }
                else
                {
                    filePath = Path.Combine(GlobalSettings.ApplySetting.FolderPathOfBrowse, $@"Models\{model.ModelFileName}.figspecblack");
                    if (!File.Exists(filePath))
                    {
                        filePath = defaultFilePath;
                    }
                }
            }
            return filePath;
        }


        /// <summary>
        /// 加载校准文件，
        /// sorting界面，加载每个model对应的校准文件
        /// scan界面，加载对应校准文件
        /// 校准文件修改时间过期，提示是否重新校准文件，不强制
        /// </summary>
        /// <param name="pathname">指定文件名的校准文件</para>
        /// <returns></returns>
        public static bool LoadCalibrationFiles(Model model = null)
        {
            string whiteRawFilePath = GetWhiteRawFilePath(model);
            string blackRawFilePath = GetBlackRawFilePath(model);
            bool isScan = model == null;
            if (isScan)
            {
                if (!TryLoadWhiteRefFile(GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRefFilePath, out string result))
                {
                    FormShowHelper.ShowMessage(result, "提示".ToMultiLanguage());
                    return false;
                }
            }

            if (!File.Exists(whiteRawFilePath) || !File.Exists(blackRawFilePath))
            {
                FormShowHelper.ShowMessage("未找到校准文件，请先进行校准".ToMultiLanguage(), "提示".ToMultiLanguage());
                return false;
            }
           
            if (isScan)
            {
                ScanParaMeter.ScanCorrectionData = new CorrectionData();
            }
            else
            {
                ScanParaMeter.SortCorrectionData = new CorrectionData();
            }
            bool whiteDone = false; //是否重新校准过
            bool blackDone = false;
            var whiteInfo = new FileInfo(whiteRawFilePath);
            var blackInfo = new FileInfo(blackRawFilePath);
            float hour = GlobalSettings.ApplySetting.CalibrationEffectiveHour;
            bool isWhiteExpire = whiteInfo.LastWriteTime.AddHours(hour) < DateTime.Now;
            bool isBlackExpire = blackInfo.LastWriteTime.AddHours(hour) < DateTime.Now;
            string tip = string.Empty;
            Enums.ColorType colorType = Enums.ColorType.None;
            if (isWhiteExpire && isBlackExpire)
            {
                tip = "校准文件已过期，是否重新校准？".ToMultiLanguage();
                colorType = Enums.ColorType.黑;
            }
            else if (isWhiteExpire)
            {
                tip = "白校准文件已过期，是否重新校准？".ToMultiLanguage();
                colorType = Enums.ColorType.白;
            }
            else if (isBlackExpire)
            {
                tip = "黑校准文件已过期，是否重新校准？".ToMultiLanguage();
                colorType = Enums.ColorType.黑;
            }
            if (!string.IsNullOrEmpty(tip))
            {
                if (FormShowHelper.ShowMessage(tip, "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) == DialogResult.OK)
                {
                    var form = new SplicingWhiteCalibrationForm(isScan, colorType, whiteRawFilePath, blackRawFilePath, model);
                    FormShowHelper.ShowDialog(form);
                    whiteDone = form.WhiteDone;
                    blackDone = form.BlackDone;
                }
            }
            try
            {
                if (!whiteDone)
                {
                    var correctionData = CorrectionHelper.GetCorrection(whiteRawFilePath);
                    
                    if (isScan)
                    {
                        ScanParaMeter.ScanCorrectionData.k_White = correctionData.Header.ReflexCoefficient;
                        ScanParaMeter.ScanCorrectionData.White_ReadBytes = correctionData.GetLineData(0);
                        ScanParaMeter.ScanCorrectionData.White_FrameRate = GlobalSettings.ApplySetting.ScanCameraSetting.FrameRate;
                        ScanParaMeter.ScanCorrectionData.White_ExposureTime = GlobalSettings.ApplySetting.ScanCameraSetting.ExposureTime;
                    }
                    else
                    {
                        ScanParaMeter.SortCorrectionData.k_White = correctionData.Header.ReflexCoefficient;
                        ScanParaMeter.SortCorrectionData.White_ReadBytes = correctionData.GetLineData(0);
                        ScanParaMeter.SortCorrectionData.White_FrameRate = GlobalSettings.ApplySetting.SortCameraSetting.FrameRate;
                        ScanParaMeter.SortCorrectionData.White_ExposureTime = GlobalSettings.ApplySetting.SortCameraSetting.ExposureTime;
                    }

                    correctionData.Dispose();
                }
            }
            catch
            {
                FormShowHelper.ShowMessage("白校准文件异常".ToMultiLanguage(), "提示".ToMultiLanguage());
                return false;
            }

            try
            {
                if (!blackDone)
                {
                    var correctionData = CorrectionHelper.GetCorrection(blackRawFilePath);
                    if (isScan)
                    {
                        ScanParaMeter.ScanCorrectionData.k_Black = correctionData.Header.ReflexCoefficient;
                        ScanParaMeter.ScanCorrectionData.Black_ReadBytes = correctionData.GetLineData(0);
                        ScanParaMeter.ScanCorrectionData.Black_FrameRate = GlobalSettings.ApplySetting.ScanCameraSetting.FrameRate;
                        ScanParaMeter.ScanCorrectionData.Black_ExposureTime = GlobalSettings.ApplySetting.ScanCameraSetting.ExposureTime;
                    }
                    else
                    {
                        ScanParaMeter.SortCorrectionData.k_Black = correctionData.Header.ReflexCoefficient;
                        ScanParaMeter.SortCorrectionData.Black_ReadBytes = correctionData.GetLineData(0);
                        ScanParaMeter.SortCorrectionData.Black_FrameRate = GlobalSettings.ApplySetting.SortCameraSetting.FrameRate;
                        ScanParaMeter.SortCorrectionData.Black_ExposureTime = GlobalSettings.ApplySetting.SortCameraSetting.ExposureTime;
                    }
                    correctionData.Dispose();
                }
            }
            catch
            {
                FormShowHelper.ShowMessage("黑校准文件异常".ToMultiLanguage(), "提示".ToMultiLanguage());
                return false;
            }

            return true;
        }
    }
}
