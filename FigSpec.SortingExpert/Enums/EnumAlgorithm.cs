using FigSpec.SortingExpert.Tools;
using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Enums
{

    public enum PreprocessTypes
    {
        [DescriptionSort("平滑处理", 0)]
        SmoothingAlgorithm,
        [DescriptionSort("基线校正", 1)]
        BaselineCorrection,
        [DescriptionSort("尺度缩放", 2)]
        ScaleScalingAlgorithm,
        //[DescriptionSort("散射校正", 3)]
        //ScatteringCorrection,
    }

    public enum EnumSmoothingAlgorithm
    {
        [DescriptionSort("SG平滑", 1)]
        SGSmoothing = 1,

        [DescriptionSort("滑动平滑", 0)]
        MeanFiltering = 2,

        //[DescriptionSort("中值滤波", 3)]
        //MedianFiltering,

        //[DescriptionSort("小波变换", 4)]
        //WaveletTransform,
    }

    public enum EnumScaleScalingAlgorithm
    {

        [DescriptionSort("标准化", 1)]
        Standardization = 1,

        //[DescriptionSort("正规化", 2)]
        //Regularize,

        [DescriptionSort("最大最小归一化", 0)]
        MaxMinNormalization = 3,

        //[DescriptionSort("Pareto尺度化", 4)]
        //ParetoScaling,
    }

    /// <summary>
    /// 散射校正
    /// </summary>
    public enum EnumScatteringCorrection
    {

        [DescriptionSort("SNV", 1)]
        E_SNV = 1,

        [DescriptionSort("MSC", 2)]
        E_MSC,
    }

    /// <summary>
    /// 基线校正
    /// </summary>
    public enum EnumBaselineCorrection
    {
        //[DescriptionSort("不使用", 0)]
        //None = 0,

        [DescriptionSort("一阶导数", 1)]
        FirstDerivative = 1,

        [DescriptionSort("二阶导数", 2)]
        SecondDerivative,

        //[DescriptionSort("CWT", 3)]
        //E_CWT,
    }
}