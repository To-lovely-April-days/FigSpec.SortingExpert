using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.ModelFiles;
using FigSpec.SortingExpert.Tools;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Cuda
{
    public class CudaAccelerator : IDisposable
    {
        public static CudaAccelerator Shared = new CudaAccelerator();

        private ILGPU.Context context;
        private ILGPU.Runtime.Accelerator accelerator;

        private ArrayView2D<byte, Stride2D.DenseY> scan_rawdata;
        private ArrayView2D<byte, Stride2D.DenseY> scan_imgdata;
        private ArrayView2D<float, Stride2D.DenseY> scan_caldata;

        private ArrayView1D<float, Stride1D.Dense> refK;             //反射率系数
        private ArrayView2D<int, Stride2D.DenseY> selectWaveIndexs; //所选波长下标集合  x模型id， y波长下标

        //skatyang
        private ArrayView2D<float, Stride2D.DenseY> cuda_mean;
        private ArrayView2D<float, Stride2D.DenseY> cuda_std;
        private ArrayView3D<float, Stride3D.DenseXY> cuda_ceof;
        private ArrayView2D<float, Stride2D.DenseX> whiteAndBlackRef;           //黑白校准数据  0 白校准 1 黑校准
        private ArrayView1D<SampleOption, Stride1D.Dense> cuda_opts;            //samples 等单个参数
        private ArrayView1D<PreprocessOption, Stride1D.Dense> cuda_pres;         
        private ArrayView1D<ModelOption, Stride1D.Dense> cuda_models;

        private Action<Index2D,
            ArrayView2D<byte, Stride2D.DenseY>,
            ArrayView1D<SampleOption, Stride1D.Dense>,
            ArrayView1D<float, Stride1D.Dense>,
            ArrayView2D<float, Stride2D.DenseX>,
            ArrayView2D<byte, Stride2D.DenseY>,
            ArrayView2D<float, Stride2D.DenseY>> scanDealDataKernal;

        private Action<Index2D,
            ArrayView2D<byte, Stride2D.DenseY>,           //每帧
            ArrayView1D<SampleOption, Stride1D.Dense>,
            ArrayView1D<PreprocessOption, Stride1D.Dense>,
            ArrayView1D<ModelOption, Stride1D.Dense>,
            ArrayView2D<byte, Stride2D.DenseY>,
            ArrayView1D<float, Stride1D.Dense>,                //定标数据系数
            ArrayView2D<float, Stride2D.DenseX>,       //黑白校准数据
            ArrayView3D<float, Stride3D.DenseZY>,         //输出数据  点*有效波段
            ArrayView3D<float, Stride3D.DenseZY>,         //输出数据  点*有效波段
            ArrayView2D<int, Stride2D.DenseY>,      //不同模型选择的波段索引 
            ArrayView2D<float, Stride2D.DenseY>,
            ArrayView2D<float, Stride2D.DenseY>,
            ArrayView3D<float, Stride3D.DenseXY>,
            ArrayView2D<byte, Stride2D.DenseY>> totalDealDataKernal;


        private Action<Index3D,
            ArrayView2D<byte, Stride2D.DenseY>,         //每帧
            ArrayView1D<SampleOption, Stride1D.Dense>,       //参数
            ArrayView1D<float, Stride1D.Dense>,           //定标数据系数
            ArrayView2D<float, Stride2D.DenseX>,    //黑白校准数据
            ArrayView3D<float, Stride3D.DenseZY>> calibrationOrginDataKernal;

        private Action<Index2D, ArrayView1D<SampleOption, Stride1D.Dense>, ArrayView3D<float, Stride3D.DenseZY>> backgroundCorrectionKernelV1;

        private Action<Index2D,
            ArrayView2D<float, Stride2D.DenseY>,
            ArrayView1D<SampleOption, Stride1D.Dense>,
            ArrayView3D<float, Stride3D.DenseZY>> backgroundCorrectionKernelV2;   //输出数据  点*有效波段


        private Action<Index2D,    //索引，第几个像素点
             ArrayView1D<SampleOption, Stride1D.Dense>,              //参数
             ArrayView1D<ModelOption, Stride1D.Dense>,
             ArrayView3D<float, Stride3D.DenseZY>,             //预处理后的数据 像素点*有效波长个数
             ArrayView2D<int, Stride2D.DenseY>,      //不同模型选择的波段索引 
             ArrayView2D<float, Stride2D.DenseY>,
             ArrayView2D<float, Stride2D.DenseY>,
             ArrayView3D<float, Stride3D.DenseXY>,
             ArrayView2D<byte, Stride2D.DenseY>> plssClassifyByteKernal;            //对比阈值后的classid



        private Action<Index1D,
            ArrayView1D<float, Stride1D.Dense>,
            ArrayView1D<int, Stride1D.Dense>,
            ArrayView1D<float, Stride1D.Dense>,
            ArrayView1D<float, Stride1D.Dense>,
            ArrayView1D<float, Stride1D.Dense>,
            ArrayView1D<byte, Stride1D.Dense>,
            ArrayView1D<float, Stride1D.Dense>> simClassifyKernalNoCalibration; //相似度算法，一帧对比中心点


        private Action<Index3D, ArrayView3D<float, Stride3D.DenseZY>, ArrayView3D<float, Stride3D.DenseZY>, int> slidingSmooth;
        private Action<Index2D, ArrayView3D<float, Stride3D.DenseZY>, ArrayView3D<float, Stride3D.DenseZY>, ArrayView2D<float, Stride2D.DenseY>, int, int> savitzkyGolay;
        private Action<Index2D, ArrayView3D<float, Stride3D.DenseZY>, ArrayView3D<float, Stride3D.DenseZY>> firstDerivative;
        private Action<Index2D, ArrayView3D<float, Stride3D.DenseZY>, ArrayView3D<float, Stride3D.DenseZY>> secondDerivative;
        private Action<Index2D, ArrayView3D<float, Stride3D.DenseZY>> standardize;
        private Action<Index2D, ArrayView3D<float, Stride3D.DenseZY>> maxMinNormailize;




        private CudaAccelerator()
        {
            //实例化一个加速器
            context = ILGPU.Context.Create(builder => builder.Default().EnableAlgorithms().Math(MathMode.Fast32BitOnly).Release().Optimize(OptimizationLevel.O2));

            if (Properties.Settings.Default.ComputeCore == 0)
            {
                if (context.GetCudaDevices().Count > 0)
                {
                    accelerator = context.GetPreferredDevices(false, false).First().CreateAccelerator(context);//GPU
                    //accelerator.PrintInformation();
                }
                else
                {
                    accelerator = context.GetPreferredDevices(true, false).First().CreateAccelerator(context);//CPU
                }
            }
            else if (Properties.Settings.Default.ComputeCore == 1)
            {
                accelerator = context.GetPreferredDevices(false, false).First().CreateAccelerator(context);//GPU
            }
            else if (Properties.Settings.Default.ComputeCore == 2)
            {
                accelerator = context.GetPreferredDevices(false, false).Last().CreateAccelerator(context);//GPU
            }
            else
            {
                accelerator = context.GetPreferredDevices(true, false).First().CreateAccelerator(context);//CPU
            }

            Trace.WriteLine($"device: {accelerator.Name}");
            Trace.WriteLine($"{nameof(accelerator.MaxNumThreadsPerGroup)}: {accelerator.MaxNumThreadsPerGroup}");
            Trace.WriteLine($"{nameof(accelerator.MaxNumThreadsPerMultiprocessor)}: {accelerator.MaxNumThreadsPerMultiprocessor}");

            scanDealDataKernal = accelerator.LoadAutoGroupedStreamKernel<Index2D,
            ArrayView2D<byte, Stride2D.DenseY>,
            ArrayView1D<SampleOption, Stride1D.Dense>,
            ArrayView1D<float, Stride1D.Dense>,
            ArrayView2D<float, Stride2D.DenseX>,
            ArrayView2D<byte, Stride2D.DenseY>,
            ArrayView2D<float, Stride2D.DenseY>>(Kernals.ScanDealDataKernal);

            totalDealDataKernal = accelerator.LoadAutoGroupedStreamKernel<Index2D,
            ArrayView2D<byte, Stride2D.DenseY>,           //每帧
            ArrayView1D<SampleOption, Stride1D.Dense>,
            ArrayView1D<PreprocessOption, Stride1D.Dense>,
            ArrayView1D<ModelOption, Stride1D.Dense>,
            ArrayView2D<byte, Stride2D.DenseY>,
            ArrayView1D<float, Stride1D.Dense>,                //定标数据系数
            ArrayView2D<float, Stride2D.DenseX>,       //黑白校准数据
            ArrayView3D<float, Stride3D.DenseZY>,         //输出数据  点*有效波段
            ArrayView3D<float, Stride3D.DenseZY>,         //输出数据  点*有效波段
            ArrayView2D<int, Stride2D.DenseY>,      //不同模型选择的波段索引 
            ArrayView2D<float, Stride2D.DenseY>,
            ArrayView2D<float, Stride2D.DenseY>,
            ArrayView3D<float, Stride3D.DenseXY>,
            ArrayView2D<byte, Stride2D.DenseY>>(Kernals.TotalDealDataKernal);

            //告诉gpu自定义方法参数类型和名称 
            calibrationOrginDataKernal = accelerator.LoadAutoGroupedStreamKernel<Index3D,
                ArrayView2D<byte, Stride2D.DenseY>,         //每帧
                ArrayView1D<SampleOption, Stride1D.Dense>,       //参数
                ArrayView1D<float, Stride1D.Dense>,           //定标数据系数
                ArrayView2D<float, Stride2D.DenseX>,    //黑白校准数据
                ArrayView3D<float, Stride3D.DenseZY>>(Kernals.CalibrationOrginDataKernal);

            backgroundCorrectionKernelV1 = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView1D<SampleOption, Stride1D.Dense>,
                ArrayView3D<float, Stride3D.DenseZY>>(Kernals.BackgroundCorrectionKernelV1);

            backgroundCorrectionKernelV2 = accelerator.LoadAutoGroupedStreamKernel<Index2D,
                ArrayView2D<float, Stride2D.DenseY>,
                ArrayView1D<SampleOption, Stride1D.Dense>,
                ArrayView3D<float, Stride3D.DenseZY>>(Kernals.BackgroundCorrectionKernelV2);


            plssClassifyByteKernal = accelerator.LoadAutoGroupedStreamKernel<Index2D,    //索引，第几个像素点
                 ArrayView1D<SampleOption, Stride1D.Dense>,             
                 ArrayView1D<ModelOption, Stride1D.Dense>,
                 ArrayView3D<float, Stride3D.DenseZY>,              //预处理后的数据 像素点*有效波长个数
                 ArrayView2D<int, Stride2D.DenseY>,                 //不同模型选择的波段索引 
                 ArrayView2D<float, Stride2D.DenseY>,
                 ArrayView2D<float, Stride2D.DenseY>,
                 ArrayView3D<float, Stride3D.DenseXY>,
                 ArrayView2D<byte, Stride2D.DenseY>>(Kernals.PlssClassifyByteKernal);

            simClassifyKernalNoCalibration = accelerator.LoadAutoGroupedStreamKernel<Index1D,
                ArrayView1D<float, Stride1D.Dense>,
                ArrayView1D<int, Stride1D.Dense>,
                ArrayView1D<float, Stride1D.Dense>,
                ArrayView1D<float, Stride1D.Dense>,
                ArrayView1D<float, Stride1D.Dense>,
                ArrayView1D<byte, Stride1D.Dense>,
                ArrayView1D<float, Stride1D.Dense>>(Kernals.SpectralSimilarityKernalNoCalibration);


            slidingSmooth = accelerator.LoadAutoGroupedStreamKernel<Index3D,
                ArrayView3D<float, Stride3D.DenseZY>,
                ArrayView3D<float, Stride3D.DenseZY>, int>(Kernals.SlidingSmooth);

            savitzkyGolay = accelerator.LoadAutoGroupedStreamKernel<Index2D,
                ArrayView3D<float, Stride3D.DenseZY>,
                ArrayView3D<float, Stride3D.DenseZY>,
                ArrayView2D<float, Stride2D.DenseY>, int, int>(Kernals.SavitzkyGolay);

            firstDerivative = accelerator.LoadAutoGroupedStreamKernel<Index2D,
                ArrayView3D<float, Stride3D.DenseZY>,
                ArrayView3D<float, Stride3D.DenseZY>>(Kernals.FirstDerivative);

            secondDerivative = accelerator.LoadAutoGroupedStreamKernel<Index2D,
                ArrayView3D<float, Stride3D.DenseZY>,
                ArrayView3D<float, Stride3D.DenseZY>>(Kernals.SecondDerivative);

            standardize = accelerator.LoadAutoGroupedStreamKernel<Index2D,
                ArrayView3D<float, Stride3D.DenseZY>>(Kernals.Standardize);

            maxMinNormailize = accelerator.LoadAutoGroupedStreamKernel<Index2D,
                ArrayView3D<float, Stride3D.DenseZY>>(Kernals.MaxMinNormailize);



        }

        /// <summary>
        /// 校准系数
        /// </summary>
        /// <param name="refData"></param>
        public void AllocCALRef(float[] refData)
        {
            refK = accelerator.Allocate1D(refData);
        }

        /// <summary>
        /// 黑白板反射率
        /// </summary>
        public void AllocCALRefWhiteBlack(float[] white, float[] black)
        {
            var ceofs = new float[2, white.Length];
            for (int i = 0; i < white.Length; i++)
            {
                ceofs[0, i] = white[i];
                ceofs[1, i] = black[i];
            }
            whiteAndBlackRef = accelerator.Allocate2DDenseX(ceofs);
        }

        public void AllocScanClassify(int lines, int samples, int bands)
        {
            var cameraSetting = GlobalSettings.ApplySetting.ScanCameraSetting;
            scan_rawdata = accelerator.Allocate2DDenseY<byte>(new LongIndex2D(lines, samples * bands * 2));
            scan_imgdata = accelerator.Allocate2DDenseY<byte>(new LongIndex2D(lines, samples * 3));
            scan_caldata = accelerator.Allocate2DDenseY<float>(new LongIndex2D(lines, samples * bands));
            cuda_opts = accelerator.Allocate1D(new SampleOption[]
            {
                new SampleOption()
                {
                    Lines = lines,
                    Samples = samples,
                    StartBandIndex = 0,
                    EndBandIndex = bands - 1,
                    ImageR = cameraSetting.ImgR,
                    ImageG = cameraSetting.ImgG,
                    ImageB = cameraSetting.ImgB,
                    ThresholdR = cameraSetting.ImgThresholdFR,
                    ThresholdG = cameraSetting.ImgThresholdFG,
                    ThresholdB = cameraSetting.ImgThresholdFB
                }
            });
        }

        public void AllocPLSSClassify(Model model, int lines, int samples, int datatype = 4, bool needCalibration = true, bool isOutline = false)
        {
            if (model.plss == null || model.ModelType != 0)
                return;
            //plsda 参数
            int row = model.plss.Count;
            int maxLength = model.plss.Max(i => i.selectIndex.Count);
            var sindexs = new int[row, maxLength];

            var meanXs = new float[row, maxLength];
            var stdXs = new float[row, maxLength];
            //var intercepts = new float[row, 2];
            var ceofs = new float[row, maxLength, 2];

            //var class_info = new int[2, row];
            //var class_threshold = new float[row];

            var c_model = new ModelOption[row];
            var c_pre = new PreprocessOption[model.Preprocessings.Count];
            for (int i = 0; i < model.Preprocessings.Count; i++)
            {
                var item = model.Preprocessings[i];
                c_pre[i] = new PreprocessOption();
                switch (item.PreprocessType)
                {
                    case PreprocessTypes.BaselineCorrection:
                        switch ((EnumBaselineCorrection)item.PreprocessAlgorithm)
                        {
                            case EnumBaselineCorrection.FirstDerivative:
                                c_pre[i].Type = 1;
                                break;
                            case EnumBaselineCorrection.SecondDerivative:
                                c_pre[i].Type = 2;
                                break;
                        }
                        break;
                    case PreprocessTypes.ScaleScalingAlgorithm:
                        switch ((EnumScaleScalingAlgorithm)item.PreprocessAlgorithm)
                        {
                            case EnumScaleScalingAlgorithm.Standardization:
                                c_pre[i].Type = 4;
                                break;
                            case EnumScaleScalingAlgorithm.MaxMinNormalization:
                                c_pre[i].Type = 3;
                                break;
                        }
                        break;
                    //case PreprocessTypes.ScatteringCorrection:
                    //    name = ((EnumScatteringCorrection)model.PreprocessAlgorithm).ToMultiLanguage<EnumScatteringCorrection>();
                    //    break;
                    case PreprocessTypes.SmoothingAlgorithm:
                        switch ((EnumSmoothingAlgorithm)item.PreprocessAlgorithm)
                        {
                            case EnumSmoothingAlgorithm.MeanFiltering:
                            case EnumSmoothingAlgorithm.SGSmoothing:
                                c_pre[i].Type = 0;
                                c_pre[i].WindowSize = item.FilterStrength;
                                break;
                        }
                        break;
                }

            }

            for (int i = 0; i < row; i++)
            {
                for (int m = 0; m < model.plss[i].MeanX.size; m++)
                {
                    meanXs[i, m] = model.plss[i].MeanX[m];
                }
                for (int m = 0; m < model.plss[i].StdX.size; m++)
                {
                    stdXs[i, m] = model.plss[i].StdX[m];
                }
                //for (int m = 0; m < model.plss[i].Intercept.size; m++)
                //{
                //    intercepts[i, m] = model.plss[i].Intercept[m];
                //}
                for (int m = 0; m < model.plss[i].Ceof.shape[0]; m++)
                {
                    for (int n = 0; n < model.plss[i].Ceof.shape[1]; n++)
                    {
                        ceofs[i, m, n] = model.plss[i].Ceof[m, n];
                    }
                }
                for (int m = 0; m < maxLength; m++)
                {
                    sindexs[i, m] = m < model.plss[i].selectIndex.Count ? model.plss[i].selectIndex[m] : -1;
                }
                //class_info[0, i] = model.plss[i].classid;
                //class_info[1, i] = model.plss[i].selectIndex.Count;
                //class_threshold[i] = model.plss[i].threshold;

                var color = Color.FromArgb(model.plss[i].color);
                c_model[i] = new ModelOption()
                {
                    ID = (byte)model.plss[i].classid,
                    BandsCount = model.plss[i].selectIndex.Count,
                    Threshold = model.plss[i].threshold,
                    Intercept = model.plss[i].Intercept[0],
                    ColorR = color.R,
                    ColorG = color.G,
                    ColorB = color.B
                };
            }


            cuda_mean = accelerator.Allocate2DDenseY(meanXs);
            cuda_std = accelerator.Allocate2DDenseY(stdXs);
            cuda_ceof = accelerator.Allocate3DDenseXY(ceofs);
            if (c_pre.Count() > 0)
            {
                cuda_pres = accelerator.Allocate1D(c_pre);
            }


            cuda_models = accelerator.Allocate1D(c_model);
            selectWaveIndexs = accelerator.Allocate2DDenseY(sindexs);

            var set = GlobalSettings.ApplySetting.SortCameraSetting;
            float tr = set.ImgThresholdFR;
            float tg = set.ImgThresholdFG;
            float tb = set.ImgThresholdFB;
            int ir = set.ImgR >= model.StartBandIndex && set.ImgR <= model.EndBandIndex ? set.ImgR - model.StartBandIndex : 0;
            int ig = set.ImgG >= model.StartBandIndex && set.ImgG <= model.EndBandIndex ? set.ImgG - model.StartBandIndex : 0;
            int ib = set.ImgB >= model.StartBandIndex && set.ImgB <= model.EndBandIndex ? set.ImgB - model.StartBandIndex : 0;
            cuda_opts = accelerator.Allocate1D(new SampleOption[]
            {
                new SampleOption()
                {
                    Lines = lines,
                    Samples = samples,
                    StartBandIndex = model.StartBandIndex,
                    EndBandIndex = model.EndBandIndex,
                    RoiBandStartIndex = model.RoiBandStartIndex,
                    ModelCount = row,
                    PreprocessCount = model.Preprocessings.Count,
                    BackgroundCorrection = model.BackgroundCorrection ? 1 : 0,
                    StartBackgroundThreshold = model.StartBackgroundThreshold,
                    EndBackgroundThreshold = model.EndBackgroundThreshold,
                    ImageR = ir,
                    ImageG = ig,
                    ImageB = ib,
                    ThresholdR = tr,
                    ThresholdG = tg,
                    ThresholdB = tb,
                    StartSampleIndex = GlobalSettings.ApplySetting.StartSampleIndex,
                }
            });
            //清空数据
            if (cuda_frameData != null)
            {
                cuda_frameData.Dispose();
                imgData.Dispose();
                outputData.Dispose();
                cuda_tags.Dispose();
                tempData.Dispose();
                cuda_frameData = null;
            }
        }

        private byte[,] PlssClassify(Model model, int line, int length, MemoryBuffer3D<float, Stride3D.DenseZY> outputData)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //存储计算结果的变量
            MemoryBuffer2D<byte, Stride2D.DenseY> cuda_tags = accelerator.Allocate2DDenseY<byte>(new LongIndex2D(line, length));

            if (model.Preprocessings.Count > 0)
            {
                int bandLength = model.EndBandIndex - model.StartBandIndex + 1;
                //临时数据
                MemoryBuffer3D<float, Stride3D.DenseZY> tempData = accelerator.Allocate3DDenseZY<float>(new LongIndex3D(line, length, bandLength));

                //预处理
                bool tempIsOutput = false;
                foreach (var item in model.Preprocessings)
                {
                    switch (item.PreprocessType)
                    {
                        case Enums.PreprocessTypes.SmoothingAlgorithm:
                            switch (item.PreprocessAlgorithm)
                            {
                                case (int)Enums.EnumSmoothingAlgorithm.SGSmoothing:
                                    MemoryBuffer2D<float, Stride2D.DenseY> coeTempData = accelerator.Allocate2DDenseY<float>(new LongIndex2D(item.FilterStrength, item.FilterNumber + 1));
                                    savitzkyGolay(new Index2D(line, length), outputData, tempData, coeTempData, item.FilterStrength, item.FilterNumber);
                                    coeTempData.Dispose();
                                    break;
                                case (int)Enums.EnumSmoothingAlgorithm.MeanFiltering:
                                    slidingSmooth(new Index3D(line, length, bandLength), outputData, tempData, item.FilterStrength);
                                    tempIsOutput = true;
                                    break;
                            }
                            break;
                        case Enums.PreprocessTypes.BaselineCorrection:
                            switch (item.PreprocessAlgorithm)
                            {
                                case (int)Enums.EnumBaselineCorrection.FirstDerivative:
                                    firstDerivative(new Index2D(line, length), outputData, tempData);
                                    tempIsOutput = true;
                                    break;
                                case (int)Enums.EnumBaselineCorrection.SecondDerivative:
                                    secondDerivative(new Index2D(line, length), outputData, tempData);
                                    break;
                            }
                            break;
                        case Enums.PreprocessTypes.ScaleScalingAlgorithm:
                            switch (item.PreprocessAlgorithm)
                            {
                                case (int)Enums.EnumScaleScalingAlgorithm.Standardization:
                                    standardize(new Index2D(line, length), outputData);
                                    break;
                                case (int)Enums.EnumScaleScalingAlgorithm.MaxMinNormalization:

                                    maxMinNormailize(new Index2D(line, length), outputData);
                                    break;
                            }
                            break;
                    }
                    accelerator.Synchronize();
                    if (tempIsOutput)
                    {
                        tempIsOutput = false;
                        var temp = outputData;
                        outputData = tempData;
                        tempData = temp;
                    }

                    Console.WriteLine($"预处理数据{item.PreprocessType}：" + stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency);
                }

                tempData.Dispose();
            }


            plssClassifyByteKernal(new Index2D(line, length), cuda_opts, cuda_models, outputData, selectWaveIndexs, cuda_mean, cuda_std, cuda_ceof, cuda_tags);
            accelerator.Synchronize();

            byte[,] tags = cuda_tags.GetAsArray2D();
            outputData.Dispose();
            cuda_tags.Dispose();
            stopwatch.Stop();
            Console.WriteLine("分类时间：" + stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency);
            return tags;
        }

        /// <summary>
        /// 无需校准分类
        /// </summary>
        /// <param name="model"></param>
        /// <param name="samples"></param>
        /// <param name="frameData"></param>
        /// <returns></returns>
        public byte[,] PLSSClassifyNoCorrection(Model model, int lines, int samples, float[,] frameData)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //输入数据
            MemoryBuffer2D<float, Stride2D.DenseY> cuda_frameData = accelerator.Allocate2DDenseY<float>(frameData);
            //输出数据
            int length = model.EndBandIndex - model.StartBandIndex + 1;
            MemoryBuffer3D<float, Stride3D.DenseZY> outputData = accelerator.Allocate3DDenseZY<float>(new LongIndex3D(lines, samples, length));

            Console.WriteLine("分选分配内存时间：" + stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency);


            backgroundCorrectionKernelV2(new Index2D(lines, samples), cuda_frameData, cuda_opts, outputData);
            accelerator.Synchronize();
            cuda_frameData.Dispose();
            stopwatch.Stop();
            Console.WriteLine("分选扣底时间：" + stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency);
            return PlssClassify(model, lines, samples, outputData);
        }


        public (float[,], byte[,]) ScanDealData(int lines, int samples, int bands, byte[,] frameData)
        {
            scan_rawdata.CopyFromCPU(frameData);
            scanDealDataKernal(new Index2D(lines, samples), scan_rawdata, cuda_opts, refK, whiteAndBlackRef, scan_imgdata, scan_caldata);
            accelerator.Synchronize();
            byte[,] im = new byte[lines, samples * 3];
            float[,] refFrame = new float[lines, samples * bands];
            scan_imgdata.CopyToCPU(im);
            scan_caldata.CopyToCPU(refFrame);
            return (refFrame, im);

        }


        private MemoryBuffer2D<byte, Stride2D.DenseY> cuda_frameData = null;
        private MemoryBuffer3D<float, Stride3D.DenseZY> outputData = null;
        private MemoryBuffer2D<byte, Stride2D.DenseY> imgData = null;
        //存储计算结果的变量
        MemoryBuffer2D<byte, Stride2D.DenseY> cuda_tags = null;
        MemoryBuffer3D<float, Stride3D.DenseZY> tempData = null;
        /// <summary>
        /// 需要校准
        /// </summary>
        /// <param name="model"></param>
        /// <param name="samples"></param>
        /// <param name="frameData"></param>
        /// <returns></returns>
        public (byte[,], byte[,]) PLSSClassifyByCorrection(Model model, int lines, int samples, byte[,] frameData)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            long start_time = DateTime.Now.Ticks;
            int length = model.EndBandIndex - model.StartBandIndex + 1;
            //输入数据
            if (cuda_frameData == null)
            {
                cuda_frameData = accelerator.Allocate2DDenseY<byte>(frameData);
                //输出数据
                outputData = accelerator.Allocate3DDenseZY<float>(new LongIndex3D(lines, samples, length));
                //图片数据
                imgData = accelerator.Allocate2DDenseY<byte>(new LongIndex2D(lines, samples * 3));

                //存储计算结果的变量
                cuda_tags = accelerator.Allocate2DDenseY<byte>(new LongIndex2D(lines, samples));

                //临时数据
                tempData = accelerator.Allocate3DDenseZY<float>(new LongIndex3D(lines, samples, length));
            }
            else
            {
                cuda_frameData.CopyFromCPU(frameData);
            }

            byte[,] im = new byte[lines, samples * 3];
#if DEBUG
            LogHelper.WriteLine("part1-将数据发送至GPU中：" + (DateTime.Now.Ticks - start_time));
#endif
            {
                totalDealDataKernal(new Index2D(lines, samples), cuda_frameData, cuda_opts, cuda_pres, cuda_models, imgData, refK, whiteAndBlackRef, outputData, tempData, selectWaveIndexs, cuda_mean, cuda_std, cuda_ceof, cuda_tags);
                accelerator.Synchronize();
                byte[,] tags = new byte[lines, samples];
                cuda_tags.CopyToCPU(tags);
                imgData.CopyToCPU(im);
                //imgData.Dispose();
                //cuda_frameData.Dispose();
                //outputData.Dispose();
                //cuda_tags.Dispose();
                //tempData.Dispose();
#if DEBUG
                LogHelper.WriteLine("part2-校准+预处理+PLS分类时间：" + (DateTime.Now.Ticks - start_time));
#endif
                return (tags, im);
            }


        }

        /// <summary>
        /// 分选，多个像素点
        /// </summary>
        /// <param name="length">像素个数</param>
        /// <param name="frameData"> x 像素Index, y 校准后的反射率</param>
        /// <returns></returns>
        public byte[] OutlineClassify(Model model, int length, float[,] frameData)
        {
            //输入数据
            //MemoryBuffer2D<float, Stride2D.DenseY> outputData = accelerator.Allocate2DDenseY(frameData);
            //return PlssClassify(model, length, outputData);
            return new byte[1];
        }

        public float[] CalibrationDatas(int samples, float[] frameData, float backgroundBase)
        {
            //输入数据
            MemoryBuffer1D<float, Stride1D.Dense> cuda_frameData = accelerator.Allocate1D<float>(frameData);

            //校准
            //calibrationKernalV2(samples, backgroundBase, cuda_frameData, cuda_opts, refK, whiteAndBlackRef);
            accelerator.Synchronize();
            return cuda_frameData.GetAsArray1D();
        }

        /// <summary>
        /// 分选，多个像素点
        /// </summary>
        /// <param name="length">像素个数</param>
        /// <param name="frameData"> x 像素Index, y 校准后的反射率</param>
        /// <returns></returns>
        public byte[] OutlineClassify(int length, float[,] frameData)
        {
            //输入数据
            MemoryBuffer2D<float, Stride2D.DenseY> cuda_frameData = accelerator.Allocate2DDenseY<float>(frameData);
            //存储计算结果的变量
            MemoryBuffer1D<byte, Stride1D.Dense> cuda_tags = accelerator.Allocate1D<byte>(length);

            //plssClassifyByteKernal(length, cuda_opts, cuda_frameData, cuda_class_threshold, cuda_class_info, selectWaveIndexs,
            //    cuda_mean, cuda_std, cuda_intercept, cuda_ceof, cuda_tags);
            accelerator.Synchronize();
            return cuda_tags.GetAsArray1D();
        }


        /// <summary>
        /// 相似度算法，计算一帧与中心点对比的阈值
        /// </summary>
        public float[] SimClassifyNoCalibration(int samples, int bands, float[] frameData, float[] clusterCenter, float centerS, float centerAvg, byte[] wavelengths)
        {
            var index = new Index1D(samples);
            var x = accelerator.Allocate1D(frameData);
            var acc = accelerator.Allocate1D<float>(index);
            var needOri = accelerator.Allocate1D<float>(bands);
            var cl = accelerator.Allocate1D(clusterCenter);
            var opt = accelerator.Allocate1D(new int[] { samples, bands });
            var optSim = accelerator.Allocate1D(new float[] { centerAvg, centerS });
            var wave = accelerator.Allocate1D(wavelengths);
            simClassifyKernalNoCalibration(index, x, opt, optSim, cl, needOri, wave, acc);
            accelerator.Synchronize();
            return acc.GetAsArray1D();
        }






        public void Dispose()
        {
            throw new NotImplementedException();
        }

        //芯片名称
        public string GetName()
        {
            return accelerator?.Name ?? string.Empty;
        }
    }
}
