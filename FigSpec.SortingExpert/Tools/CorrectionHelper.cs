using Hyperspectral.ReflCharFile;
using Hyperspectral.SpectralFile;
using Hyperspectral.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    /// <summary>
    /// 校准文件
    /// </summary>
    public  class CorrectionHelper
    {
        /// <summary>
        /// 保存校准文件
        /// </summary>
        /// <param name="spe"></param>
        /// <param name="filepath"></param>
        /// <param name="correctionDataType">0：黑校准 1:白校准</param>
        /// <param name="lineBytes"></param>
        /// <param name="dataType"></param>
        /// <param name="bands"></param>
        /// <param name="samples"></param>
        /// <param name="waveLength"></param>
        public unsafe static void SaveFile(SPE spe,int correctionDataType, string filepath, byte[] lineBytes, int dataType, int bands, int samples, float[] waveLength, float[] reflexCoefficient)
        {
            int sizeOfBand = SizeOfBand(dataType);
            if (dataType == -1)
            {
                return;
            }

            var setting = GlobalSettings.ApplySetting.ScanCameraSetting;

            // 声明校正数据头信息
            CorrectionHeader header = new CorrectionHeader {
                VersionCode = 1,
                Lines = 1,
                Samples = spe.Samples,
                ReflexCoefficient = reflexCoefficient,
                SamplesBinning = (int)(spe.Hdr.SpaceBinning ?? 0),
                Bands = spe.Bands,
                BandsBinning = (int)(spe.Hdr.SpectrumBinning ?? 0),
                ExposureTime = setting.ExposureTime,
                Gain = setting.Gain,
                CameraSN = setting.CameraSN,
                WaveLength = waveLength,
                DataType =  (CorrectionHeader.CorrectionDataType)correctionDataType,
                SaveType = EnumInterleave.bil,
                Type = CorrectionHeader.CorrectionType.Line,
                
                
            };

            CorrectionData correctionData = new CorrectionData(header);
            fixed (byte* bytes = lineBytes)
            {
                var lineData = new float[correctionData.SizeOfLine / 4];
                for (int s = 0; s < samples; s++)
                {
                    for (int b = 0; b < bands; b++)
                    {
                        float data1 = 0;
                        if (sizeOfBand == 1)
                        {
                            byte* pData = bytes;
                            data1 = *(pData + b * samples + s);
                        }
                        else if (sizeOfBand == 2)
                        {
                            ushort* pData = (ushort*)bytes;
                            data1 = *(pData + b * samples + s);

                        }
                        else if (sizeOfBand == 4)
                        {
                            float* pData = (float*)bytes;
                            data1 = *(pData + b * samples + s);
                        }
                        lineData[s + b * samples] = data1;
                    }
                }
                correctionData.AddLineData(0, lineData);
            }
            correctionData.CreateVerificationCode();
            correctionData.Save(filepath);
        }

        /// <summary>
        /// line的 byte 转 float
        /// </summary>
        /// <param name="lineBytes"></param>
        /// <param name="dataType"></param>
        /// <param name="samples"></param>
        /// <param name="bands"></param>
        /// <returns></returns>
        public unsafe static float[] LineByte2Float(byte[] lineBytes, int dataType, int samples, int bands)
        {
            int sizeOfBand = SizeOfBand(dataType);
            if (dataType == -1)
            {
                return null;
            }
            
            var lineData = new float[samples * bands];
            fixed (byte* bytes = lineBytes)
            {
               
                for (int s = 0; s < samples; s++)
                {
                    for (int b = 0; b < bands; b++)
                    {
                        float data1 = 0;
                        if (sizeOfBand == 1)
                        {
                            byte* pData = bytes;
                            data1 = *(pData + b * samples + s);
                        }
                        else if (sizeOfBand == 2)
                        {
                            ushort* pData = (ushort*)bytes;
                            data1 = *(pData + b * samples + s);

                        }
                        else if (sizeOfBand == 4)
                        {
                            float* pData = (float*)bytes;
                            data1 = *(pData + b * samples + s);
                        }
                        lineData[s + b * samples] = data1;
                    }
                }
            }

            return lineData;
        }

        /// <summary>
        /// 保存校准文件
        /// </summary>
        /// <param name="spe"></param>
        /// <param name="correctionDataType">0：黑校准 1:白校准</param>
        /// <param name="filepath"></param>
        /// <param name="lineBytes"></param>
        /// <param name="waveLength"></param>
        /// <param name="reflexCoefficient"></param>
        public unsafe static void SaveFile(SPE spe, int correctionDataType, string filepath, float[] lineBytes, float[] waveLength, float[] reflexCoefficient)
        {
            var setting = GlobalSettings.ApplySetting.ScanCameraSetting;

            // 声明校正数据头信息
            CorrectionHeader header = new CorrectionHeader
            {
                VersionCode = 1,
                Lines = 1,
                Samples = spe.Samples,
                ReflexCoefficient = reflexCoefficient,
                SamplesBinning = (int)(spe.Hdr.SpaceBinning ?? 0),
                Bands = spe.Bands,
                BandsBinning = (int)(spe.Hdr.SpectrumBinning ?? 0),
                ExposureTime = setting.ExposureTime,
                Gain = setting.Gain,
                CameraSN = setting.CameraSN ?? "FX-?",
                WaveLength = waveLength,
                DataType = (CorrectionHeader.CorrectionDataType)correctionDataType,
                SaveType = EnumInterleave.bil,
                Type = CorrectionHeader.CorrectionType.Line,


            };

            CorrectionData correctionData = new CorrectionData(header);
            correctionData.AddLineData(0, lineBytes);
            correctionData.CreateVerificationCode();
            correctionData.Save(filepath);
        }


        public static byte[] Line2BGR(float[] lineDatas, int samples, (int r, int g, int b) channel, (float r, float g, float b) threshold)
        {
            var data = new byte[samples * 3];
            for (int y = 0; y < samples; y++)
            {
                var offsetR = y + channel.r * samples;
                var offsetG = y + channel.g * samples;
                var offsetB = y + channel.b * samples;
       
                data[y * 3] = (lineDatas[offsetB] / threshold.b * 255).ToByte();
                data[y * 3 + 1] = (lineDatas[offsetG] / threshold.g * 255).ToByte();
                data[y * 3 + 2] = (lineDatas[offsetR] / threshold.r * 255).ToByte();
            }

            return data;
        }

        /// <summary>
        /// 获取相机波长和校准数据
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static (float[] WaveLength, float[] Signal) GetCorrectionData(string filepath)
        {
            CorrectionData correctionData = new CorrectionData(filepath);
            var data = (correctionData.Header.WaveLength, correctionData.GetLineData(0));
            correctionData.Dispose();
            return data;
        }

        public static CorrectionData GetCorrection(string filepath)
        {
            CorrectionData correctionData = new CorrectionData(filepath);
            return correctionData;
        }


        /// <summary>
        /// 获取单个像素大小
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        /// <exception cref="DataTypeExpection"></exception>
        public static int SizeOfBand(int dataType)
        {
            switch (dataType)
            {
                case 0:
                case 1:
                    return 1;
                case 4:
                    return 4;
                case 12:
                case 16:
                    return 2;
                default:
                    return -1;
            }
        }
    }
}
