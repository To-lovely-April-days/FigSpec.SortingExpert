using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.IO;
using Accord.MachineLearning;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using Accord.Statistics;
using Accord.Statistics.Kernels;

namespace FigSpec.SortingExpert.Algorithm
{
    public class Svm
    {
        #region 私有属性和方法
        private LinearDualCoordinateDescent teacher;
        private SupportVectorMachine svm;
        private void newobj(double[][] inputs, int[] results)
        {
            try
            {
                teacher = new LinearDualCoordinateDescent()
                {
                    Loss = Loss.L2,
                    Complexity = 1000,
                    Tolerance = 1e-5
                };
                if (inputs == null || results == null)
                {
                    svm = null;
                }
                else
                {
                    svm = teacher.Learn(inputs, results);
                    // Save the model to a file
                    //Serializer.Save(svm, "svm_model.bin");
                    //var loadedMachine = Serializer.Load<SupportVectorMachine<Linear>>("svm_model.bin");
                    //var tags = loadedMachine.Decide(inputs);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        public static Svm chsvm = null;
        public Svm()
        {
            teacher = new LinearDualCoordinateDescent()
            {
                Loss = Loss.L2,
                Complexity = 1000,
                Tolerance = 1e-5
            };
            svm = null;
        }
        public Svm(double[][] inputs, int[] results)
        {
            newobj(inputs, results);
        }
        public Svm(double[,] csvData, int[] results)
        {
            try
            {
                double[][] inputs = new double[csvData.GetLength(0)][];
                for (int i = 0; i < csvData.GetLength(0); i++)
                {
                    inputs[i] = new double[csvData.GetLength(1)];
                    for (int m = 0; m < csvData.GetLength(1); m++)
                    {
                        inputs[i][m] = csvData[i, m];
                    }
                }
                newobj(inputs, results);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public Svm(double[,] csvData)
        {
            try
            {
                double[][] inputs = new double[csvData.GetLength(0)][];
                int[] results = new int[csvData.GetLength(0)];
                for (int i = 0; i < csvData.GetLength(0); i++)
                {
                    results[i] = (int)csvData[i, 0];
                    inputs[i] = new double[csvData.GetLength(1) - 1];
                    for (int m = 1; m < csvData.GetLength(1); m++)
                    {
                        inputs[i][m - 1] = csvData[i, m];
                    }
                }
                newobj(inputs, results);
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        public Svm(string filePath, int startRow = 0, int startColum = 0)
        {
            try
            {
                double[,] csvData = LoadCsvData(filePath, startRow, startColum);
                double[][] inputs = new double[csvData.GetLength(0)][];
                int[] results = new int[csvData.GetLength(0)];
                for (int i = 0; i < csvData.GetLength(0); i++)
                {
                    results[i] = (int)csvData[i, 0];
                    inputs[i] = new double[csvData.GetLength(1) - 1];
                    for (int m = 1; m < csvData.GetLength(1); m++)
                    {
                        inputs[i][m - 1] = csvData[i, m];
                    }
                }
                newobj(inputs, results);
            }
            catch (Exception)
            {

                throw;
            }

        }
        ~Svm()
        {
            // 在这里编写清理代码
            Console.WriteLine("析构函数被调用");
        }
        public bool[] Decide(double[][] inputs)
        {
            if (svm != null)
            {
                bool[] answers = svm.Decide(inputs);
                return answers;
            }
            return null;
        }
        public bool[] Decide(double[,] csvData)
        {
            try
            {
                if (svm != null)
                {
                    double[][] inputs = new double[csvData.GetLength(0)][];
                    for (int i = 0; i < csvData.GetLength(0); i++)
                    {
                        inputs[i] = new double[csvData.GetLength(1)];
                        for (int m = 0; m < csvData.GetLength(1); m++)
                        {
                            inputs[i][m] = csvData[i, m];
                        }
                    }
                    bool[] answers = svm.Decide(inputs);
                    return answers;
                }
                return null;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public bool[] Decide(string filePath, int startRow = 0, int startColum = 0)
        {
            try
            {
                if (svm != null)
                {
                    double[,] csvData = LoadCsvData(filePath, startRow, startColum);
                    double[][] inputs = new double[csvData.GetLength(0)][];
                    for (int i = 0; i < csvData.GetLength(0); i++)
                    {
                        inputs[i] = new double[csvData.GetLength(1)];
                        for (int m = 0; m < csvData.GetLength(1); m++)
                        {
                            inputs[i][m] = csvData[i, m];
                        }
                    }
                    bool[] answers = svm.Decide(inputs);
                    return answers;
                }
                return null;
            }
            catch (Exception)
            {

                throw;
            }
          
        }




        public double[,] LoadCsvData(string filePath, int startRow = 0, int startColum = 0)
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


    }
}
