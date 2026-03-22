using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Cuda
{
    public struct SampleOption
    {
        public int Lines { get; set; }

        public int Samples { get; set; }

        public int StartBandIndex { get; set; }

        public int EndBandIndex { get; set; }

        public int RoiBandStartIndex { get; set; }
        
        public int ModelCount { get; set; }

        public int PreprocessCount { get; set; }

        public int BackgroundCorrection { get; set; }

        public float StartBackgroundThreshold { get; set; }

        public float EndBackgroundThreshold { get; set; }

        public int ImageR { get; set; }
        public int ImageG { get; set; }
        public int ImageB { get; set; }

        public float ThresholdR { get; set; }
        public float ThresholdG { get; set; }
        public float ThresholdB { get; set; }
        /// <summary>
        /// 像素开始下标
        /// </summary>
        public int StartSampleIndex { get; set; }
    }

    public struct PreprocessOption
    {
        public int Type { get; set; }

        public int WindowSize { get; set; }


    }

    public struct ModelOption
    {
        /// <summary>
        /// 分类ID
        /// </summary>
        public byte ID { get; set; }
        /// <summary>
        /// 波段个数
        /// </summary>
        public int BandsCount { get; set; }
        /// <summary>
        /// 分类阈值
        /// </summary>
        public float Threshold { get; set; }

        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }

        public float Intercept { get; set; }
    }


    public static class Kernals
    {
        public static void ScanDealDataKernal(Index2D index, 
            ArrayView2D<byte, Stride2D.DenseY> orginData, 
            ArrayView1D<SampleOption, Stride1D.Dense> opt,
            ArrayView1D<float, Stride1D.Dense> refK,              
            ArrayView2D<float, Stride2D.DenseX> whiteOrBlack,
            ArrayView2D<byte, Stride2D.DenseY> imageData,
            ArrayView2D<float, Stride2D.DenseY> outputData)
        {
            int line = index.X;
            int sample = index.Y;
            var paraOpt = opt[0];
            int samples = paraOpt.Samples;//像素数
            int startBandIndex = paraOpt.StartBandIndex;//起始波段
            int endBandIndex = paraOpt.EndBandIndex;
            int length = endBandIndex - startBandIndex + 1;
            for (int i = 0; i < length; i++)
            {

                int trueBand = i + startBandIndex;   //有效波长，0起始
                int refIndex = sample + trueBand * samples;

                byte byte0 = orginData[line, refIndex * 2];
                byte byte1 = orginData[line, refIndex * 2 + 1];
                // 将字节合并为 ushort
                float needOri = (ushort)(byte0 | (byte1 << 8));
                float needRefK = refK[trueBand];
                float needWhite = whiteOrBlack[0, refIndex];
                float needBlack = whiteOrBlack[1, refIndex];
                if (needWhite <= needBlack || needOri <= needBlack)
                {
                    needOri = 0;
                }
                else
                {
                    needOri = (needOri - needBlack) / (needWhite - needBlack) * needRefK;
                }
                outputData[line, sample + i * samples] = needOri;

            }
            //计算图片
            int imgIndex = sample * 3;
            imageData[line, imgIndex] = (byte)(outputData[line, sample + paraOpt.ImageB * samples] / paraOpt.ThresholdB * 255);
            imageData[line, imgIndex + 1] = (byte)(outputData[line, sample + paraOpt.ImageG * samples] / paraOpt.ThresholdG * 255);
            imageData[line, imgIndex + 2] = (byte)(outputData[line, sample + paraOpt.ImageR * samples] / paraOpt.ThresholdR * 255);
        }





        public static void TotalDealDataKernal(
            Index2D index,
            ArrayView2D<byte, Stride2D.DenseY> orginData,           //每帧
            ArrayView1D<SampleOption, Stride1D.Dense> opt,                   //参数
            ArrayView1D<PreprocessOption, Stride1D.Dense> preprocess,    //预处理参数
            ArrayView1D<ModelOption, Stride1D.Dense> modeloption,       //模型参数
            ArrayView2D<byte, Stride2D.DenseY> imageData,          //校准后的图片数据

            ArrayView1D<float, Stride1D.Dense> refK,                //定标数据系数
            ArrayView2D<float, Stride2D.DenseX> whiteOrBlack,       //黑白校准数据

            ArrayView3D<float, Stride3D.DenseZY> outputData,         //输出数据  点*有效波段
            ArrayView3D<float, Stride3D.DenseZY> tempData,         //输出数据  点*有效波段

            ArrayView2D<int, Stride2D.DenseY> bandIndexs,      //不同模型选择的波段索引 
            ArrayView2D<float, Stride2D.DenseY> mean,
            ArrayView2D<float, Stride2D.DenseY> std,
            ArrayView3D<float, Stride3D.DenseXY> ceof,
            ArrayView2D<byte, Stride2D.DenseY> tags
            )
        {
            int line = index.X;
            int sample = index.Y;
            var paraOpt = opt[0];
            //int lines = paraOpt.Lines;//帧数
            int samples = paraOpt.Samples;//像素数
            int startBandIndex = paraOpt.StartBandIndex;//起始波段
            int endBandIndex = paraOpt.EndBandIndex;
            int length = endBandIndex - startBandIndex + 1;
            //------------------------------
            //校准
            float KDsum = 0f;//扣底求和
            {
                for (int i = 0; i < length; i++)
                {
                    //int trueBand = i + startBandIndex;
                    int trueBand = i + paraOpt.RoiBandStartIndex;   //有效波长，0起始
                    int refIndex = sample + trueBand * samples;
                    //int refIndex = sample + paraOpt.StartSampleIndex + trueBand * 640;
                    //int tempIndex = sample + (i + paraOpt.RoiBandStartIndex) * samples;
                    byte byte0 = orginData[line, refIndex * 2];
                    byte byte1 = orginData[line, refIndex * 2 + 1];
                    // 将字节合并为 ushort
                    float needOri = (ushort)(byte0 | (byte1 << 8));
                    float needRefK = refK[trueBand];
                    float needWhite = whiteOrBlack[0, refIndex];
                    float needBlack = whiteOrBlack[1, refIndex];
                    if (needWhite <= needBlack || needOri <= needBlack)
                    {
                        needOri = 0;
                    }
                    else
                    {
                        needOri = (needOri - needBlack) / (needWhite - needBlack) * needRefK;
                    }
                    outputData[line, sample, i] = needOri;
                    KDsum += needOri;
                }
            }
            //计算图片
            int imgIndex = sample * 3;
            imageData[line, imgIndex] = (byte)(outputData[line, sample, paraOpt.ImageB] / paraOpt.ThresholdB * 255);
            imageData[line, imgIndex + 1] = (byte)(outputData[line, sample, paraOpt.ImageG] / paraOpt.ThresholdG * 255);
            imageData[line, imgIndex + 2] = (byte)(outputData[line, sample, paraOpt.ImageR] / paraOpt.ThresholdR * 255);

            //------------------------------
            //扣底
            if(paraOpt.BackgroundCorrection == 1)
            {
                //float sum = 0f;
                //for (int i = 0; i < length; i++)
                //{
                //    sum += outputData[index.X, index.Y, i];
                //}
                float backgroudBase1 = paraOpt.StartBackgroundThreshold * length;
                float backgroudBase2 = paraOpt.EndBackgroundThreshold * length;
                if (KDsum <= backgroudBase2 && KDsum >= backgroudBase1)//是否扣底判断
                {
                    tags[index.X, index.Y] = 255;
                    return;
                }
            }

            //--- 预处理
            for (int k = 0; k < paraOpt.PreprocessCount; k++)
            {
                if (preprocess[k].Type == 0)
                {
                    //滑动平滑
                    int windowSize = preprocess[k].WindowSize;
                    for (int y = 0; y < length; y++)
                    {
                        int halfSize = windowSize / 2;
                        int minY = y - halfSize;
                        int maxY = y + halfSize;
                        minY = minY < 0 ? 0 : minY;
                        maxY = maxY < length ? maxY : length - 1;

                        float sum = 0.0f;
                        int count = maxY - minY + 1;

                        // 使用单一循环计算总和
                        for (int i = minY; i <= maxY; i++)
                        {
                            sum += outputData[index.X, index.Y, i];
                        }
                        // 计算平均值并存储到 tempData 中
                        tempData[index.X, index.Y, y] = sum / count;
                    }
                    for (int y = 0; y < length; y++)
                    {
                        outputData[index.X, index.Y, y] = tempData[index.X, index.Y, y];
                    }
                }
                else if (preprocess[k].Type == 1)
                {
                    //一阶求导
                    for (int i = 0; i < length - 1; i++)
                    {
                        outputData[index.X, index.Y, i] = outputData[index.X, index.Y, i + 1] - outputData[index.X, index.Y, i];
                    }
                    outputData[index.X, index.Y, length - 1] = outputData[index.X, index.Y, length - 2];
                }
                else if (preprocess[k].Type == 2)
                {
                    //二阶求导
                    for (int i = 0; i < length - 1; i++)
                    {
                        outputData[index.X, index.Y, i] = outputData[index.X, index.Y, i + 1] - outputData[index.X, index.Y, i];
                    }
                    outputData[index.X, index.Y, length - 1] = outputData[index.X, index.Y, length - 2];
                    for (int i = 0; i < length - 1; i++)
                    {
                        outputData[index.X, index.Y, i] = outputData[index.X, index.Y, i + 1] - outputData[index.X, index.Y, i];
                    }
                    outputData[index.X, index.Y, length - 1] = outputData[index.X, index.Y, length - 2];
                }
                else if (preprocess[k].Type == 3)
                {
                    //最大最小归一化
                    float maxValue = float.MinValue;
                    float minValue = float.MaxValue;

                    for (int i = 0; i < length; i++)
                    {
                        float value = outputData[index.X, index.Y, i];
                        if (value < minValue)
                        {
                            minValue = value;
                        }
                        if (value > maxValue)
                        {
                            maxValue = value;
                        }
                    }

                    if (maxValue != minValue)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            outputData[index.X, index.Y, i] = (outputData[index.X, index.Y, i] - minValue) / (maxValue - minValue);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < length; i++)
                        {
                            outputData[index.X, index.Y, i] = 0.0f;
                        }
                    }
                }
                else if (preprocess[k].Type == 4)
                {
                    //标准化
        
                    float sum = 0f;
                    for (int i = 0; i < length; i++)
                    {
                        sum += outputData[index.X, index.Y, i];
                    }
                    float avergae = sum / length;
                    float sumOfSquares = 0;
                    for (int i = 0; i < length; i++)
                    {
                        float diff = outputData[index.X, index.Y, i] - avergae;
                        sumOfSquares += diff * diff;
                    }
                    float stdDev = (float)Math.Sqrt(sumOfSquares / length); //得到标准差
                                                                           
                    for (int i = 0; i < length; i++)
                    {
                        outputData[index.X, index.Y, i] = (outputData[index.X, index.Y, i] - avergae) / stdDev;
                    }

                }
            }


            //------------------------------
            //分选
            {
                int labelCount = paraOpt.ModelCount;

                int loc = 0;
                float record = 0.0f;

                for (int m = 0; m < labelCount; m++)
                {
                    var sum = 0f;
                    var ttm = modeloption[m];
                    for (int j = 0; j < ttm.BandsCount; j++) //选择的波段数
                    {
                        float ref1 = outputData[index.X, index.Y, bandIndexs[m, j] - startBandIndex];
                        sum += (ref1 - mean[m, j]) / std[m, j] * ceof[m, j, 0]; //plsda 预测
                    }
                    var _acc = sum + ttm.Intercept;
                    if (_acc > modeloption[loc].Threshold && _acc > record)
                    {
                        loc = m;
                        record = _acc;
                    }
                }

                var mp = modeloption[loc];
                //单个的标签Classid
                if (record > mp.Threshold)
                {
                    tags[index] = mp.ID;
                    imageData[line, imgIndex] = mp.ColorB;
                    imageData[line, imgIndex + 1] = mp.ColorG;
                    imageData[line, imgIndex + 2] = mp.ColorR;
                }
                else
                {
                    tags[index] = 0;
                }
                //tags[index] = record > modeloption[loc].Threshold ? modeloption[loc].ID : (byte)0;
            }
        }
        /// <summary>
        /// 校准
        /// </summary>
        /// <param name="index"></param>
        /// <param name="orginData"></param>
        /// <param name="opt"></param>
        /// <param name="refK"></param>
        /// <param name="whiteOrBlack"></param>
        /// <param name="outputData"></param>
        public static void CalibrationOrginDataKernal(Index3D index,
            ArrayView2D<byte, Stride2D.DenseY> orginData,         //每帧
            ArrayView1D<SampleOption, Stride1D.Dense> opt,       //参数
            ArrayView1D<float, Stride1D.Dense> refK,           //定标数据系数
            ArrayView2D<float, Stride2D.DenseX> whiteOrBlack,    //黑白校准数据
            ArrayView3D<float, Stride3D.DenseZY> outputData     //输出数据  点*有效波段
        )
        {
            //int lines = opt[0].Lines;
            int samples = opt[0].Samples;
            int startBandIndex = opt[0].StartBandIndex;

            int line = index.X;
            int sample = index.Y;  //像素
            int band = index.Z;   //有效波长，0起始 ROI设置后的波段索引
            int trueBand = index.Z + startBandIndex; //真实的全局波段索引

            int refIndex = sample + band * samples;
            int refIndexFull = sample + trueBand * samples;
            byte byte0 = orginData[line, refIndex * 2];
            byte byte1 = orginData[line, refIndex * 2 + 1];
            // 将字节合并为 ushort
            float needOri = (ushort)(byte0 | (byte1 << 8));

            float needRefK = refK[band];
            float needWhite = whiteOrBlack[0, refIndexFull];
            float needBlack = whiteOrBlack[1, refIndexFull];
            if (needWhite <= needBlack || needOri <= needBlack)
            {
                needOri = 0;
            }
            else
            {
                needOri = (needOri - needBlack) / (needWhite - needBlack) * needRefK;
            }
            outputData[line, sample, index.Z] = needOri;
        }

        /// <summary>
        /// 扣底
        /// </summary>
        /// <param name="index"></param>
        /// <param name="backgroudBase"></param>
        /// <param name="outputData"></param>
        public static void BackgroundCorrectionKernelV1(Index2D index, ArrayView1D<SampleOption, Stride1D.Dense> opt, ArrayView3D<float, Stride3D.DenseZY> outputData)
        {
            float sum = 0f;
            int length = outputData.IntExtent.Z;
            for (int i = 0; i < outputData.IntExtent.Z; i++)
            {
                sum += outputData[index.X, index.Y, i];
            }

            float backgroudBase1 = opt[0].StartBackgroundThreshold * length;
            float backgroudBase2 = opt[0].EndBackgroundThreshold * length;
            if (sum < backgroudBase2 && sum > backgroudBase1)//是否扣底判断
            {
                for (int i = 0; i < length; i++)
                {
                    outputData[index.X, index.Y, i] = 0;
                }
            }

        }

        /// <summary>
        /// 校准 , 反射率扣底
        /// </summary>
        /// <param name="index"></param>
        /// <param name="orginData"></param>
        /// <param name="opt"></param>
        /// <param name="refK"></param>
        /// <param name="whiteOrBlack"></param>
        public static void BackgroundCorrectionKernelV2(Index2D index,
            ArrayView2D<float, Stride2D.DenseY> orginData,         //每帧
            ArrayView1D<SampleOption, Stride1D.Dense> opt,       //参数
            ArrayView3D<float, Stride3D.DenseZY> outputData     //输出数据  点*有效波段
        )
        {
            //int lines = opt[0].Lines;
            int samples = opt[0].Samples;
            int startBandIndex = opt[0].StartBandIndex;
            int endBandIndex = opt[0].EndBandIndex;
            float sum = 0f;
            for (int i = startBandIndex; i <= endBandIndex; i++)
            {
                sum += orginData[index.X, index.Y + i * samples];
                outputData[index.X, index.Y, i - startBandIndex] = orginData[index.X, index.Y + i * samples];
            }
            if (opt[0].BackgroundCorrection == 1)
            {
                int length = (endBandIndex - startBandIndex + 1);
                float backgroudBase1 = opt[0].StartBackgroundThreshold * length;
                float backgroudBase2 = opt[0].EndBackgroundThreshold * length;
                if (sum <= backgroudBase2 && sum >= backgroudBase1)
                {
                    for (int i = 0; i < length; i++)
                    {
                        outputData[index.X, index.Y, i] = 0;
                    }
                }
            }

        }


        /*

        /// <summary>
        /// 设置每帧的校准后的数据
        /// </summary>
        /// <param name="index">索引 像素*波段数</param>
        /// <param name="frame">第几帧</param>
        /// <param name="orginData">每帧数据</param>
        /// <param name="opt">参数</param>
        /// <param name="refK"></param>
        /// <param name="whiteOrBlack"></param>
        /// <param name="outputData">输出数据  帧数 * 像素 * 有效波段</param>
        public static void MultiCalibrationOrginDataKernal(Index2D index, int frame,
            ArrayView1D<byte, Stride1D.Dense> orginData,
            ArrayView1D<int, Stride1D.Dense> opt,
            ArrayView1D<float, Stride1D.Dense> refK,
            ArrayView2D<float, Stride2D.DenseX> whiteOrBlack,
            ArrayView3D<float, Stride3D.DenseZY> outputData
)
        {
            int lines = opt[0];
            int samples = opt[1];
            int startBandIndex = opt[2];

            int sample = index.X;  //像素
            int band = index.Y + startBandIndex;   //有效波长，0起始

            int refIndex = sample + band * samples;
            byte byte0 = orginData[refIndex * 2];
            byte byte1 = orginData[refIndex * 2 + 1];
            // 将字节合并为 ushort
            float needOri = (ushort)(byte0 | (byte1 << 8));

            float needRefK = refK[band];
            float needWhite = whiteOrBlack[0, refIndex];
            float needBlack = whiteOrBlack[1, refIndex];
            if (needWhite == needBlack)
            {
                needOri = 0;
            }
            else
            {
                needOri = (needOri - needBlack) / (needWhite - needBlack) * needRefK;
            }
            outputData[frame, sample, index.Y] = needOri;
        }

        /// <summary>
        /// 扣除背景
        /// </summary>
        /// <param name="index">帧数*像素数</param>
        /// <param name="backgroudBase">背景阈值</param>
        /// <param name="orginData">反射率 帧数*像素数*波段数</param>
        /// <param name="outputData">帧数*像素数</param>
        public static void MultiBackgroundCorrectionKernel(Index2D index,
            float backgroudBase,
            ArrayView3D<float, Stride3D.DenseZY> orginData,
            ArrayView2D<byte, Stride2D.DenseY> outputData)
        {
            int frame = index.X;
            int sample = index.Y;
            float sum = 0f;
            for (int i = 0; i < orginData.IntExtent.Z; i++)
            {
                sum += orginData[frame, sample, i];
            }
            float average = sum / orginData.IntExtent.Z;
            if (average < backgroudBase)
            {
                for (int i = 0; i < orginData.IntExtent.Z; i++)
                {
                    orginData[frame, sample, i] = 0;
                }
                outputData[frame, sample] = 0;
            }
            else
            {
                outputData[frame, sample] = 255;
            }
        }

        public static void MultiContourCorrection(Index1D index,
            ArrayView2D<byte, Stride2D.DenseY> imgData,
            ArrayView2D<int, Stride2D.DenseY> rects,
            ArrayView2D<int, Stride2D.DenseY> contourData)
        {
            int startX = rects[index.X, 0];
            int startY = rects[index.X, 1];
            int width = rects[index.X, 2];
            int heigth = rects[index.X, 3];
            int id = index.X + 1;
            for (int x = startX; x < width; x++)
            {
                for (int y = startY; y < heigth; y++)
                {
                    contourData[x, y] = imgData[x, y] & id;
                }
            }
        }


        /// 计算轮廓平均值
        /// </summary>
        /// <param name="index">轮廓数 * 波段数</param>
        /// <param name="imgData">帧 * 像素</param>
        /// <param name="rects">轮廓数 * 矩形参数</param>
        /// <param name="orginData">帧 * 像素 * 波段数</param>
        /// <param name="outputData"></param>
        public static void MultiCalculateAverageData(Index2D index,
            ArrayView2D<byte, Stride2D.DenseY> imgData,
            ArrayView2D<int, Stride2D.DenseY> rects,
            ArrayView3D<float, Stride3D.DenseZY> orginData,
            ArrayView2D<int, Stride2D.DenseY> contourData,
            ArrayView2D<float, Stride2D.DenseY> outputData)
        {
            int startX = rects[index.X, 0];
            int startY = rects[index.X, 1];
            int width = rects[index.X, 2];
            int heigth = rects[index.X, 3];
            float sum = 0f;
            int count = 0;
            int id = index.X + 1;
            for (int x = startX; x < width; x++)
            {
                for (int y = startY; y < heigth; y++)
                {
                    if (imgData[x, y] == id)
                    {
                        sum += orginData[x, y, index.Y];
                        count++;
                    }
                }
            }
            outputData[index] = sum / count;
        }


        /// <summary>
        /// 校准 扣底
        /// </summary>
        /// <param name="index"></param>
        /// <param name="orginData"></param>
        /// <param name="opt"></param>
        /// <param name="refK"></param>
        /// <param name="whiteOrBlack"></param>
        public static void CalibrationKernalV2(Index1D index, float backgroudBase,
            ArrayView1D<float, Stride1D.Dense> orginData,         //每帧
            ArrayView1D<int, Stride1D.Dense> opt,       //参数
            ArrayView1D<float, Stride1D.Dense> refK,           //定标数据系数
            ArrayView2D<float, Stride2D.DenseX> whiteOrBlack    //黑白校准数据
        )
        {
            int lines = opt[0];
            int samples = opt[1];
            int startBandIndex = opt[2];
            int endBandIndex = opt[3];
            //int labelCount = opt[4];
            int needCalibration = opt[5];
            float sum = 0f;
            for (int i = startBandIndex; i <= endBandIndex; i++)
            {
                float refl = 0;
                float needOri = orginData[index + i * samples];
                if (needCalibration == 1)
                {
                    float needRefK = refK[i];
                    float needWhite = whiteOrBlack[0, index + i * samples];
                    float needBlack = whiteOrBlack[1, index + i * samples];
                    if (needOri - needBlack == 0 || needWhite - needBlack == 0 || needRefK == 0)
                    {
                        refl = 0;
                    }
                    else
                    {
                        refl = (needOri - needBlack) / (needWhite - needBlack) * needRefK;
                    }
                    orginData[index + i * samples] = refl;
                    sum += refl;
                }
            }
            if (sum / (endBandIndex - startBandIndex + 1) < backgroudBase)
            {
                for (int i = startBandIndex; i <= endBandIndex; i++)
                {
                    orginData[index + i * samples] = 0;
                }
            }
        }

        /// <summary>
        /// 校准 原始数据 校准 + 扣底
        /// </summary>
        /// <param name="index"></param>
        /// <param name="orginData"></param>
        /// <param name="opt"></param>
        /// <param name="refK"></param>
        /// <param name="whiteOrBlack"></param>
        public static void CalibrationKernalV3(Index1D index, float backgroudBase,
            ArrayView1D<byte, Stride1D.Dense> orginData,         //每帧
            ArrayView1D<float, Stride1D.Dense> outputData,
            ArrayView1D<SampleOption, Stride1D.Dense> opt,       //参数
            ArrayView1D<float, Stride1D.Dense> refK,           //定标数据系数
            ArrayView2D<float, Stride2D.DenseX> whiteOrBlack    //黑白校准数据
        )
        {
            int lines = opt[0].Lines;
            int samples = opt[0].Samples;
            int startBandIndex = opt[0].StartBandIndex;
            int endBandIndex = opt[0].EndBandIndex;
            //int labelCount = opt[4];
            int needCalibration = opt[0].Lines;
            int dataType = opt[0].Lines;

            float sum = 0f;
            for (int i = startBandIndex; i <= endBandIndex; i++)
            {
                float refl = 0;
                float needOri = 0f;
                int refIndex = index + i * samples;
                switch (dataType)
                {
                    case 0:
                    case 1:
                        needOri = orginData[refIndex];
                        break;
                    case 4:
                        needOri = 0f;
                        break;
                    case 12:
                    case 16:
                        int byteOffset = refIndex * 2;
                        byte byte0 = orginData[byteOffset];
                        byte byte1 = orginData[byteOffset + 1];
                        // 将字节合并为 ushort
                        needOri = (ushort)(byte0 | (byte1 << 8));
                        break;
                }
                if (needCalibration == 1)
                {
                    float needRefK = refK[i];
                    float needWhite = whiteOrBlack[0, index + i * samples];
                    float needBlack = whiteOrBlack[1, index + i * samples];
                    if (needOri - needBlack == 0 || needWhite - needBlack == 0 || needRefK == 0)
                    {
                        refl = 0;
                    }
                    else
                    {
                        refl = (needOri - needBlack) / (needWhite - needBlack) * needRefK;
                    }
                    outputData[refIndex] = refl;
                    sum += refl;
                }
                else
                {
                    outputData[refIndex] = needOri;
                }
            }
            if (sum / (endBandIndex - startBandIndex + 1) < backgroudBase && backgroudBase > 0)
            {
                for (int i = startBandIndex; i <= endBandIndex; i++)
                {
                    outputData[index + i * samples] = 0;
                }
            }
        }

        */

        /// <summary>
        /// plsda分类算法
        /// </summary>
        public static void PlssClassifyByteKernal(Index2D index,    //索引，第几个像素点
             ArrayView1D<SampleOption, Stride1D.Dense> opt,              //参数
             ArrayView1D<ModelOption, Stride1D.Dense> modeloption,       //模型参数
             ArrayView3D<float, Stride3D.DenseZY> inputData,             //预处理后的数据 像素点*有效波长个数

             ArrayView2D<int, Stride2D.DenseY> bandIndexs,      //不同模型选择的波段索引 
             ArrayView2D<float, Stride2D.DenseY> mean,
             ArrayView2D<float, Stride2D.DenseY> std,
             ArrayView3D<float, Stride3D.DenseXY> ceof,
             ArrayView2D<byte, Stride2D.DenseY> tags)            //对比阈值后的classid
        {

            float backSum = 0f;
            for (int i = 0; i < inputData.IntExtent.Z; i++)
            {
                backSum += inputData[index.X, index.Y, i];
            }
            if (backSum == 0)
            {
                tags[index.X, index.Y] = 0;
                return;
            }
            var paraOpt = opt[0];
            //int lines = paraOpt.Lines;
            //int samples = opt[1];
            int startBandIndex = paraOpt.StartBandIndex;
            //int endBandIndex = opt[3];
            int labelCount = paraOpt.ModelCount;

            float refl = 0;
            int loc = 0;
            float record = 0.0f;

            for (int m = 0; m < labelCount; m++)
            {
                var sum = 0f;
                var ttm = modeloption[m];
                for (int j = 0; j < ttm.BandsCount; j++) //选择的波段数
                {
                    float ref1 = inputData[index.X, index.Y, bandIndexs[m, j] - startBandIndex];
                    sum += (ref1 - mean[m, j]) / std[m, j] * ceof[m, j, 0]; //plsda 预测
                }
                var _acc = sum + ttm.Intercept;
                if (_acc > modeloption[loc].Threshold && _acc > record)
                {
                    loc = m;
                    record = _acc;
                }
            }

            var mp = modeloption[loc];
            //单个的标签Classid
            if (record > mp.Threshold)
            {
                tags[index] = mp.ID;
            }
            else
            {
                tags[index] = 0;
            }
        }



        public static void SpectralSimilarityKernalNoCalibration(Index1D index,
            ArrayView1D<float, Stride1D.Dense> X,                   //一帧的反射率
            ArrayView1D<int, Stride1D.Dense> opt,                   // 一帧的像素数， 通道数
            ArrayView1D<float, Stride1D.Dense> optSim,              // 平均值，平均差
            ArrayView1D<float, Stride1D.Dense> clusterCenter,       //中心店反射率
            ArrayView1D<float, Stride1D.Dense> needRef,
            ArrayView1D<byte, Stride1D.Dense> wave,                 //需要计算的波长
            ArrayView1D<float, Stride1D.Dense> acc)                  //结果阈值
        {
            var sample = index.X;
            var centerAvg = optSim[0];
            var centerS = optSim[1];
            int lines = opt[0];
            var samples = opt[1];
            var bands = opt[2];


            var compareSum = 0f;
            var compareAvg = 0f;
            int count = 0;
            for (int i = 0; i < bands; i++)
            {
                if (wave[i] == 0)
                    continue;
                needRef[i] = X[sample + i * samples];
                compareSum += needRef[i];
                count++;
            }
            compareAvg = compareSum / count;

            float compareS = 0f;
            float s = 0f;
            for (int k = 0; k < bands; k++)
            {
                if (wave[k] == 0)
                    continue;
                s += (needRef[k] - compareAvg) * (clusterCenter[k] - centerAvg);
                compareS += (float)Math.Pow(needRef[k] - compareAvg, 2);
            }
            compareS = (float)Math.Sqrt(compareS);

            float res = s / (centerS * compareS);

            acc[sample] = float.IsNaN(res) ? 0f : Math.Abs(res);
        }





        #region 预处理算法

        #region 平滑算法

        public static void SlidingSmooth(Index3D index, ArrayView3D<float, Stride3D.DenseZY> inputData, ArrayView3D<float, Stride3D.DenseZY> tempData, int windowSize)
        {
            int length = inputData.IntExtent.Z;
            int y = index.Z;
            int halfSize = windowSize / 2;
            int minY = y - halfSize;
            int maxY = y + halfSize;
            minY = minY < 0 ? 0 : minY;
            maxY = maxY < length ? maxY : length - 1;

            float sum = 0.0f;
            int count = maxY - minY + 1;

            // 使用单一循环计算总和
            for (int i = minY; i <= maxY; i++)
            {
                sum += inputData[index.X, index.Y, i];
            }

            // 计算平均值并存储到 tempData 中
            tempData[index] = sum / count;
        }


        /// <summary>
        /// SG平滑
        /// </summary>
        /// <param name="inputData">输入数据</param>
        /// <param name="windowSize">窗口大小</param>
        /// <param name="polynomialOrder">多项式阶数</param>
        /// <returns></returns>
        public static void SavitzkyGolay(Index2D index, ArrayView3D<float, Stride3D.DenseZY> inputData, ArrayView3D<float, Stride3D.DenseZY> tempData, ArrayView2D<float, Stride2D.DenseY> tempCoefData, int windowSize, int polyOrder)
        {
            long length = inputData.Extent.Z;
            int halfWindowSize = windowSize / 2;
            //double[,] coeffMatrix = new double[windowSize, polyOrder + 1];
            for (int i = -halfWindowSize; i <= halfWindowSize; i++)
            {
                for (int j = 0; j <= polyOrder; j++)
                {
                    float sum = 0f;
                    for (int k = -halfWindowSize; k <= halfWindowSize; k++)
                    {
                        sum += (float)Math.Pow(k, j) * (k == i ? 1 : 0);
                    }
                    tempCoefData[i + halfWindowSize, j] = sum / windowSize;
                }
            }
            // 滤波
            for (int i = 0; i < length; i++)
            {
                float sum = 0f;
                for (int j = -halfWindowSize; j <= halfWindowSize; j++)
                {
                    int curI = i + j;
                    if (curI < 0)
                        sum += inputData[index.X, index.Y, 0] * tempCoefData[j + halfWindowSize, 0];
                    else if (curI >= length)
                        sum += inputData[index.X, index.Y, length - 1] * tempCoefData[j + halfWindowSize, 0];
                    else
                        sum += inputData[index.X, index.Y, curI] * tempCoefData[j + halfWindowSize, 0];
                }
                tempData[index.X, index.Y, i] = (float)sum;
            }

            for (int i = 0; i < length; i++)
            {
                inputData[index.X, index.Y, i] = tempData[index.X, index.Y, i];
            }
        }

        #endregion

        #region 基线校正
        /// <summary>
        /// 一阶导数
        /// </summary>
        /// <param name="index"></param>
        /// <param name="inputData"></param>
        /// <param name="tempData"></param>
        /// <param name="length"></param>
        public static void FirstDerivative(Index2D index, ArrayView3D<float, Stride3D.DenseZY> inputData, ArrayView3D<float, Stride3D.DenseZY> tempData)
        {
            long length = inputData.Extent.Z;
            for (int i = 0; i < length - 1; i++)
            {
                tempData[index.X, index.Y, i] = inputData[index.X, index.Y, i + 1] - inputData[index.X, index.Y, i];
            }
            tempData[index.X, index.Y, length - 1] = tempData[index.X, index.Y, length - 2];
        }

        /// <summary>
        /// 二阶导数
        /// </summary>
        /// <param name="spectra"></param>
        /// <returns></returns>
        public static void SecondDerivative(Index2D index, ArrayView3D<float, Stride3D.DenseZY> inputData, ArrayView3D<float, Stride3D.DenseZY> tempData)
        {
            long length = inputData.Extent.Z;
            for (int i = 0; i < length - 1; i++)
            {
                tempData[index.X, index.Y, i] = inputData[index.X, index.Y, i + 1] - inputData[index.X, index.Y, i];
            }
            tempData[index.X, index.Y, length - 1] = tempData[index.X, index.Y, length - 2];

            for (int i = 0; i < length - 1; i++)
            {
                inputData[index.X, index.Y, i] = tempData[index.X, index.Y, i + 1] - tempData[index.X, index.Y, i];
            }
            inputData[index.X, index.Y, length - 1] = inputData[index.X, index.Y, length - 2];
        }



        #endregion

        #region 尺度缩放
        /// <summary>
        /// 标准化
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static void Standardize(Index2D index, ArrayView3D<float, Stride3D.DenseZY> inputData)
        {
            long length = inputData.Extent.Z;
            float sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += inputData[index.X, index.Y, i];
            }
            float mean = sum / length;
            float sumOfSquares = 0;
            for (int i = 0; i < length; i++)
            {
                float diff = inputData[index.X, index.Y, i] - mean;
                sumOfSquares += diff * diff;
            }
            float stdDev = (float)Math.Sqrt(sumOfSquares / length);//得到标准差
            //float[] standardizedData = new float[length];
            for (int i = 0; i < length; i++)
            {
                inputData[index.X, index.Y, i] = (inputData[index.X, index.Y, i] - mean) / stdDev;
            }

        }

        /// <summary>
        /// 正规化
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static void Regularize(Index1D index, ArrayView2D<float, Stride2D.DenseY> inputData, ArrayView1D<float, Stride1D.Dense> maxValues, ArrayView1D<float, Stride1D.Dense> minValues)
        {
            long length = inputData.Extent.Y;
            for (int i = 0; i < length; i++)
            {
                if (maxValues[i] != minValues[i])
                    inputData[index, i] = (inputData[index, i] - minValues[i]) / (maxValues[i] - minValues[i]);
                else
                    inputData[index, i] = 0;
            }
        }

        /// <summary>
        /// 最大最小归一化
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static void MaxMinNormailize(Index2D index, ArrayView3D<float, Stride3D.DenseZY> inputData)
        {
            long length = inputData.Extent.Z;
            float maxValue = float.MinValue;
            float minValue = float.MaxValue;

            for (int i = 0; i < length; i++)
            {
                float value = inputData[index.X, index.Y, i];
                if (value < minValue)
                {
                    minValue = value;
                }
                if (value > maxValue)
                {
                    maxValue = value;
                }
            }

            if (maxValue != minValue)
            {
                for (int i = 0; i < length; i++)
                {
                    inputData[index.X, index.Y, i] = (inputData[index.X, index.Y, i] - minValue) / (maxValue - minValue);
                }
            }
            else
            {
                for (int i = 0; i < length; i++)
                {
                    inputData[index.X, index.Y, i] = 0.0f;
                }
            }
        }



        #endregion

        #endregion






    }
}
