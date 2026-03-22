using OpenCvSharp;
using OpenCvSharp.ML;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Algorithm
{
    public class OpenCVSvm
    {
        SVM svm;

        public OpenCVSvm(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            if (Path.GetExtension(filePath).ToLower() == ".xml")
            {
                svm = LoadFromXml(filePath);
            }
            else
            {
                var (data, tags) = TrainFromCsvFile(filePath);
                if (data == null || tags == null)
                    svm = null;
                else
                    svm = SVMTrain(data, tags);
            }
        }


        private (float[][], int[]) TrainFromCsvFile(string filePath)
        {
            try
            {
                string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                string[] lines = File.ReadAllLines(filePath);
                int rowCount = lines.Length;
                int colCount = lines[0].Split(listSeparator.First()).Length;

                float[][] data = new float[rowCount][];
                int[] tags = new int[rowCount];

                for (int i = 0; i < rowCount; i++)
                {
                    string[] values = lines[i].Split(listSeparator.First());
                    data[i] = new float[colCount - 1];
                    for (int j = 0; j < colCount; j++)
                    {
                        if (j == 0)
                        {
                            if (int.TryParse(values[j], out int value))
                            {
                                tags[i] = value;
                            }
                            else
                            {
                                Console.WriteLine($"无法将 '{values[j]}' 转换为整形，在第 {i + 1} 行，第 {j + 1} 列。");
                            }
                        }
                        else
                        {
                            if (float.TryParse(values[j], out float value))
                            {
                                data[i][j - 1] = value;
                            }
                            else
                            {
                                Console.WriteLine($"无法将 '{values[j]}' 转换为浮点数，在第 {i + 1} 行，第 {j + 1} 列。");
                            }
                        }
                    }
                }

                return (data, tags);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }

        private Mat IntArrayToMat(int[] results)
        {
            Mat mat = new Mat(results.Length, 1, MatType.CV_32SC1);
            mat.SetArray(results);
            return mat;
        }

        private Mat FloatArrayToMat(float[][] array)
        {
            // 获取数据维度
            int rows = array.Length;
            int cols = array[0].Length;

            Mat mat = new Mat(rows, cols, MatType.CV_32FC1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    mat.At<float>(i, j) = (float)array[i][j];
                }
            }

            return mat;
        }

        private Mat FloatArrayToMat(float[,] array)
        {
            // 获取数据维度
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);

            Mat mat = new Mat(rows, cols, MatType.CV_32FC1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    mat.At<float>(i, j) = (float)array[i,j];
                }
            }

            return mat;
        }

        public SVM SVMTrain(float[][] data, int[] tags)
        {
            // 创建SVM实例
            var svm1 = SVM.Create();
            svm1.KernelType = SVM.KernelTypes.Linear;
            svm1.Type = SVM.Types.CSvc;

            Mat trainData = FloatArrayToMat(data);

            Mat labelsMat = IntArrayToMat(tags);

            // 训练SVM
            svm1.Train(trainData, SampleTypes.RowSample, labelsMat);

            return svm1;
        }

        public float[] Predict(float[][] data)
        {
            Mat trainData = FloatArrayToMat(data);
            return Predict(trainData);
        }

        public float[] Predict(float[,] data)
        {
            Mat trainData = FloatArrayToMat(data);
            return Predict(trainData);
        }

        public float[] Predict(Mat trainData)
        {
            Mat outputArray = new Mat();
            svm.Predict(trainData, outputArray);
            float[] result = new float[outputArray.Rows];
            for (int i = 0; i < outputArray.Rows; i++)
            {
                result[i] = outputArray.At<float>(i);
            }
            return result;
        }

        public SVM LoadFromXml(string filePath)
        {
            return SVM.Load(filePath);
        }

        public bool SaveToXml(string filePath)
        {
            if (svm == null || Path.GetExtension(filePath).ToLower() != ".xml")
                return false;
            svm.Save(filePath);
            return true;
        }

    }
}
