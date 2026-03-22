using FigSpec.SortingExpert.Algorithm;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.ModelFiles;
using Hyperspectral.Tools;
using Hyperspectral.Tools.BaselineCorrection;
using Hyperspectral.Tools.ScatterCorrection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public class PreprocessingHelper
    {

        /// <summary>
        /// 预处理光谱反射率
        /// </summary>
        /// <param name="lreflect"></param>
        /// <returns></returns>
        public static float[][] GetPreprocessingReflect(float[][] lreflect, List<Preprocessing> preprocessings)
        {
            if (preprocessings.Count == 0 || preprocessings == null)
                return lreflect;
            foreach (var item in preprocessings)
            {
                if (!item.Enabled)
                    continue;
                switch (item.PreprocessType)
                {
                    case PreprocessTypes.BaselineCorrection:
                        switch (item.PreprocessAlgorithm)
                        {
                            case (int)EnumBaselineCorrection.FirstDerivative:
                                for (int i = 0; i < lreflect.Length; i++)
                                {
                                    lreflect[i] = ReflectPreprocessing.FirstDerivative(lreflect[i]);
                                }
                                break;
                            case (int)EnumBaselineCorrection.SecondDerivative:
                                for (int i = 0; i < lreflect.Length; i++)
                                {
                                    lreflect[i] = ReflectPreprocessing.SecondDerivative(lreflect[i]);
                                }
                                break;
                        }
                        break;
                    case PreprocessTypes.ScaleScalingAlgorithm:
                        switch (item.PreprocessAlgorithm)
                        {
                            case (int)EnumScaleScalingAlgorithm.Standardization:
                                for (int i = 0; i < lreflect.Length; i++)
                                {
                                    lreflect[i] = ReflectPreprocessing.Standardize(lreflect[i]);
                                }
                                break;
                            //case (int)EnumScaleScalingAlgorithm.Regularize:
                            //    int length = lreflect[0].Length;
                            //    float[] maxRef = new float[lreflect[0].Length];
                            //    float[] minRef = new float[lreflect[0].Length];
                            //    for (int s = 0; s < length; s++)
                            //    {
                            //        for (int i = 0; i < lreflect.Length; i++)
                            //        {
                            //            float temp = lreflect[i][s];
                            //            if (i == 0)
                            //            {
                            //                maxRef[s] = temp;
                            //                minRef[s] = temp;
                            //                continue;
                            //            }
                            //            if (temp > maxRef[s])
                            //            {
                            //                maxRef[s] = temp;
                            //            }
                            //            if (temp < minRef[s])
                            //            {
                            //                minRef[s] = temp;
                            //            }
                            //        }
                            //    }
                            //    item.PreBandMaxValue = maxRef;
                            //    item.PreBandMinValue = minRef;
                            //    for (int i = 0; i < lreflect.Length; i++)
                            //    {
                            //        lreflect[i] = ReflectPreprocessing.Regularize(lreflect[i], maxRef, minRef);
                            //    }
                            //    break;
                            case (int)EnumScaleScalingAlgorithm.MaxMinNormalization:
                                for (int i = 0; i < lreflect.Length; i++)
                                {
                                    lreflect[i] = ReflectPreprocessing.MaxMinNormailize(lreflect[i]);
                                }
                                break;
                        }
                        break;
                    //case PreprocessTypes.ScatteringCorrection:
                    //    switch (item.PreprocessAlgorithm)
                    //    {
                    //        case (int)EnumScatteringCorrection.E_SNV:
                    //            lreflect = lreflect.StandardNormalTransform();
                    //            break;
                    //        case EnumScatteringCorrection.E_MSC:
                    //            lreflect = lreflect.MultipScatterCorrection();
                    //            break;
                    //    }
                    //    break;
                    case PreprocessTypes.SmoothingAlgorithm:
                        switch (item.PreprocessAlgorithm)
                        {
                            case (int)EnumSmoothingAlgorithm.MeanFiltering:
                                for (int i = 0; i < lreflect.Length; i++)
                                {
                                    lreflect[i] = ReflectPreprocessing.SlidingSmooth(lreflect[i], item.FilterStrength);
                                }
                                break;
                            case (int)EnumSmoothingAlgorithm.SGSmoothing:
                                for (int i = 0; i < lreflect.Length; i++)
                                {
                                    lreflect[i] = ReflectPreprocessing.SavitzkyGolay(lreflect[i], item.FilterStrength, item.FilterNumber);
                                }
                                break;
                        }
                        break;
                }
            }

            return lreflect;
        }


        public static float[] GetPreprocessingReflectSingle(float[] lreflect, List<Preprocessing> preprocessings)
        {
            if (preprocessings.Count == 0 || preprocessings == null)
                return lreflect;
            foreach (var item in preprocessings)
            {
                if (!item.Enabled)
                    continue;
                switch (item.PreprocessType)
                {
                    case PreprocessTypes.BaselineCorrection:
                        switch (item.PreprocessAlgorithm)
                        {
                            case (int)EnumBaselineCorrection.FirstDerivative:
                                lreflect = ReflectPreprocessing.FirstDerivative(lreflect);
                                break;
                            case (int)EnumBaselineCorrection.SecondDerivative:
                                lreflect = ReflectPreprocessing.SecondDerivative(lreflect);
                                break;
                        }
                        break;
                    case PreprocessTypes.ScaleScalingAlgorithm:
                        switch (item.PreprocessAlgorithm)
                        {
                            case (int)EnumScaleScalingAlgorithm.Standardization:
                                lreflect = ReflectPreprocessing.Standardize(lreflect);
                                break;
                            //case (int)EnumScaleScalingAlgorithm.Regularize:
                            //    lreflect = ReflectPreprocessing.Regularize(lreflect, item.PreBandMaxValue, item.PreBandMinValue);
                            //    break;
                            case (int)EnumScaleScalingAlgorithm.MaxMinNormalization:
                                lreflect = ReflectPreprocessing.MaxMinNormailize(lreflect);
                                break;
                        }
                        break;
                    //case PreprocessTypes.ScatteringCorrection:
                    //    switch (item.PreprocessAlgorithm)
                    //    {
                    //        case (int)EnumScatteringCorrection.E_SNV:
                    //            lreflect = lreflect.StandardNormalTransform();
                    //            break;
                    //        case EnumScatteringCorrection.E_MSC:
                    //            lreflect = lreflect.MultipScatterCorrection();
                    //            break;
                    //    }
                    //    break;
                    case PreprocessTypes.SmoothingAlgorithm:
                        switch (item.PreprocessAlgorithm)
                        {
                            case (int)EnumSmoothingAlgorithm.MeanFiltering:
                                lreflect = ReflectPreprocessing.SlidingSmooth(lreflect, item.FilterStrength);
                                break;
                            case (int)EnumSmoothingAlgorithm.SGSmoothing:
                                lreflect = ReflectPreprocessing.SavitzkyGolay(lreflect, item.FilterStrength, item.FilterNumber);
                                break;
                        }
                        break;
                }
            }

            return lreflect;
        }

    }



}
