using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Algorithm
{
    public class ReflectPreprocessing
    {
        #region 滤波算法
        /// <summary>
        /// 滑动滤波
        /// </summary>
        /// <param name="values">输入数据</param>
        /// <param name="windowSize"></param>
        /// <returns></returns>
        public static float[] SlidingSmooth(float[] values, int windowSize)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException();
            }
            if (windowSize < 3 || windowSize % 2 == 0)
            {
                throw new ArgumentException();
            }
            int length = values.Length;
            float[] results = new float[length];
            int start = 0;
            int end = 0;
            for (int i = 0; i < length; i++)
            {
                if (i < windowSize / 2)
                {
                    start = 0;
                }
                else
                {
                    start = i - windowSize / 2;
                }
                if (length - i - 1 < windowSize / 2)
                {
                    end = length - 1;
                }
                else
                {
                    end = i + windowSize / 2;
                }
                float sum = 0;
                for (int n = start; n <= end; n++)
                {
                    sum += values[n];
                }
                results[i] = sum / (end - start + 1);
            }
            return results;
        }

        /// <summary>
        /// SG平滑
        /// </summary>
        /// <param name = "values" > 输入数据 </ param >
        /// < param name="windowSize">窗口大小</param>
        /// <param name = "polynomialOrder" > 多项式阶数 </ param >
        /// < returns ></ returns >
        public static float[] SavitzkyGolay(float[] inputData, int windowSize = 3, int polynomialOrder = 1)
        {
            int halfWindowSize = windowSize / 2;
            int n = inputData.Length;
            float[] outputData = new float[n];

            //计算系数矩阵
            double[,] coeffMatrix = SavitzkyGolayCalculateCoefficients(windowSize, polynomialOrder);

            //滤波
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = -halfWindowSize; j <= halfWindowSize; j++)
                {
                    int index = i + j;
                    if (index < 0)
                        sum += inputData[0] * coeffMatrix[j + halfWindowSize, 0];
                    else if (index >= n)
                        sum += inputData[n - 1] * coeffMatrix[j + halfWindowSize, 0];
                    else
                        sum += inputData[index] * coeffMatrix[j + halfWindowSize, 0];
                }
                outputData[i] = (float)sum;
            }
            return outputData;
        }
        private static double[,] SavitzkyGolayCalculateCoefficients(int windowSize, int polyOrder)
        {
            if (windowSize % 2 == 0)
            {
                throw new ArgumentException("WindowSize must be an even number");
            }
            if (polyOrder <= 0)
            {
                throw new ArgumentException();
            }
            int halfWindowSize = windowSize / 2;
            double[,] coeffMatrix = new double[windowSize, polyOrder + 1];

            for (int i = -halfWindowSize; i <= halfWindowSize; i++)
            {
                for (int j = 0; j <= polyOrder; j++)
                {
                    double sum = 0;
                    for (int k = -halfWindowSize; k <= halfWindowSize; k++)
                    {
                        sum += Math.Pow(k, j) * (k == i ? 1 : 0);
                    }
                    coeffMatrix[i + halfWindowSize, j] = sum / windowSize;
                }
            }
            return coeffMatrix;
        }
 
        #endregion

        #region 尺度缩放
        /// <summary>
        /// Z-score 标准化
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float[] Standardize(float[] values)
        {
            int length = values.Length;
            float sum = 0;
            foreach (var value in values)
            {
                sum += value;
            }
            var mean = sum / length;//得到均值
            float sumOfSquares = 0;
            foreach (var value in values)
            {
                float diff = value - mean;
                sumOfSquares += diff * diff;
            }
            var stdDev = (float)Math.Sqrt(sumOfSquares / length);//得到标准差
            float[] standardizedData = new float[length];
            for (int i = 0; i < values.Length; i++)
            {
                standardizedData[i] = (values[i] - mean) / stdDev;
            }
            return standardizedData;
        }
        /// <summary>
        /// 正规化
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float[] Regularize(float[] values, float[] maxValues, float[] minValues)
        {
            int length = values.Length;
            float[] Data = new float[length];
            for (int i = 0; i < length; i++)
            {
                if (maxValues[i] == minValues[i])
                    Data[i] = 0;
                else
                    Data[i] = (values[i] - minValues[i]) / (maxValues[i] - minValues[i]);
            }
            return Data;
        }
        /// <summary>
        /// 最大最小归一化 V1
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float[] MaxMinNormailize(float[] values, float maxValues, float minValues)
        {
            int length = values.Length;
            float[] Data = new float[length];
            for (int i = 0; i < length; i++)
            {
                Data[i] = (values[i] - minValues) / (maxValues - minValues);
            }
            return Data;
        }
        /// <summary>
        /// 最大最小归一化 V2
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float[] MaxMinNormailize(float[] values)
        {
            int length = values.Length;
            float maxValues, minValues;
            maxValues = minValues = values[0];//得到最大最小值
            for (int i = 0; i < length; i++)
            {
                if (values[i] < minValues)
                {
                    minValues = values[i];
                }
                else if (values[i] > maxValues)
                {
                    maxValues = values[i];
                }
            }
            float[] Data = new float[length];
            if (maxValues != minValues)
            {
                for (int i = 0; i < length; i++)
                {
                    Data[i] = (values[i] - minValues) / (maxValues - minValues);
                }
            }
            return Data;
        }
        #endregion

        #region 基线校正
        public static float[] FirstDerivative(float[] spectra)
        {
            int n = spectra.Length;
            float[] firstDerivative = new float[n];

            for (int i = 0; i < n - 1; i++)
            {
                firstDerivative[i] = spectra[i + 1] - spectra[i];
            }

            firstDerivative[n - 1] = firstDerivative[n - 2];

            return firstDerivative;
        }
        public static float[] SecondDerivative(float[] spectra)
        {
            return FirstDerivative(FirstDerivative(spectra));
        }
        #endregion

        #region 散射校正


        #endregion 
        public static double[,] LoadCsvData(string filePath, int startRow = 0, int startColum = 0)
        {
            try
            {
                string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                string[] lines = File.ReadAllLines(filePath);
                int rowCount = lines.Length;
                int colCount = lines[0].Split(listSeparator.First()).Length;
                double[,] data = new double[rowCount - startRow, colCount - startColum];
                for (int i = startRow; i < rowCount; i++)
                {
                    string[] values = lines[i].Split(listSeparator.First());
                    for (int j = startColum; j < colCount; j++)
                    {
                        if (float.TryParse(values[j], out float value))
                        {
                            data[i - startRow, j - startColum] = value;
                        }
                        else
                        {
                            Console.WriteLine($"无法将 '{values[j]}' 转换为浮点数，在第 {i + 1} 行，第 {j + 1} 列。");
                        }
                    }
                }
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SaveToCsv(float[] data, string filePath)
        {
            try
            {
                var listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                using (StreamWriter writer = new StreamWriter(filePath, false))  // false 表示覆盖已有文件
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        writer.Write(data[i]);
                        if (i < data.Length - 1)
                        {
                            writer.Write(listSeparator);
                            //writer.Write(",");
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void SaveToCsv(double[] data, string filePath)
        {
            try
            {
                var listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                using (StreamWriter writer = new StreamWriter(filePath, false))  // false 表示覆盖已有文件
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        writer.Write(data[i]);
                        if (i < data.Length - 1)
                        {
                            writer.Write(listSeparator);
                            //writer.Write(",");
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
