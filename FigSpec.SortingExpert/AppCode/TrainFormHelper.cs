using DevExpress.XtraCharts;
using FigSpec.SortingExpert.Cuda;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.ModelFiles;
using Globalization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hyperspectral.SpectralFile;
using FigSpec.SortingExpert.Enums;
using Hyperspectral.Tools;
using Hyperspectral.Tools.ScatterCorrection;
using Hyperspectral.Tools.BaselineCorrection;

namespace FigSpec.SortingExpert.AppCode
{
    /// <summary>
    /// 训练功能帮助类
    /// </summary>
    public class TrainFormHelper
    {
        /// <summary>
        /// 获取默认模型名称
        /// </summary>
        /// <param name="index"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        public string GetDefaultModelName(int index, List<Model> models)
        {
            string name = "模型".ToMultiLanguage() + index;
            if (models != null && models.Exists(i => i.name == name))
            {
                return GetDefaultModelName(++index, models);
            }
            return name;
        }



        /// <summary>
        /// 创建圆角矩形路径的辅助方法
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="cornerRadius"></param>
        /// <returns></returns>
        public GraphicsPath RoundedRectangle(Rectangle rect, int cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();

            int diameter = cornerRadius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(rect.Location, size);

            // 左上角
            path.AddArc(arc, 180, 90);
            // 右上角
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            // 右下角
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            // 左下角
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();

            return path;
        }


        public void AddStrip(XYDiagram diagram, TrainBands trainBands, int startIndex, int endIndex)
        {
            double startX = trainBands.WaveLength[startIndex];
            if (startIndex > 0)
            {
                startX = (trainBands.WaveLength[startIndex - 1] + startX) / 2;
            }

            double endX = trainBands.WaveLength[endIndex];
            if (endIndex < trainBands.WaveLength.Length - 1)
            {
                endX = (trainBands.WaveLength[endIndex + 1] + endX) / 2;
            }
            Strip strip = new Strip();
            strip.Color = Color.FromArgb(100, 100, 100);
            strip.MinLimit.AxisValue = startX;
            strip.MaxLimit.AxisValue = endX;
            strip.ShowInLegend = false; // 在图例中不显示分割区域
            diagram.AxisX.Strips.Add(strip); // 将分割区域添加到 X 轴上
        }



        /// <summary>
        /// 计算整个图片所有像素与单个的阈值差
        /// </summary>
        /// <param name="spe"></param>
        /// <param name="localX"></param>
        /// <param name="localY"></param>
        /// <param name="bands">使用波长 0 未使用 1 使用</param>
        /// <returns></returns>
        public unsafe float[][] CalculateThresholdDiffs(CancellationToken cancelToken, SPE spe, int localX, int localY, byte[] bands, Action<int, float[][]> action)
        {
            float[][] diff = new float[spe.Lines][];
            for (int i = 0; i < spe.Lines; i++)
            {
                diff[i] = new float[spe.Samples];
            }

            float[] centerArray = spe[localX, localY];
            float centerAvg = 0f;
            float centerS = 0f;
            int count = 0;
            for (int i = 0; i < centerArray.Length; i++)
            {
                if (bands[i] == 0)
                    continue;
                count++;
                centerAvg += centerArray[i];
            }
            centerAvg /= count; 

            for (int i = 0; i < centerArray.Length; i++)
            {
                if (bands[i] == 0)
                    continue;
                centerS += (float)Math.Pow(centerArray[i] - centerAvg, 2);
            }
            centerS = (float)Math.Sqrt(centerS);
            Parallel.For(0, spe.Lines, delegate (int x, ParallelLoopState pls)
            {
                long offset = (long)x * (long)spe.SizeOfLine;
                var memoryMappedViewStream = spe.Raw.CreateViewStream(offset, spe.SizeOfLine);
                byte* pointer = null;
                memoryMappedViewStream.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
                pointer += memoryMappedViewStream.PointerOffset;
                var frame = new byte[spe.SizeOfLine];
                System.Runtime.InteropServices.Marshal.Copy((IntPtr)pointer, frame, 0, spe.SizeOfLine);
                var refData = frame.ToFloat(spe.DataType);


                diff[x] = CudaAccelerator.Shared.SimClassifyNoCalibration(spe.Samples, spe.Bands, refData, centerArray, centerS, centerAvg, bands);
                memoryMappedViewStream.SafeMemoryMappedViewHandle.ReleasePointer();
                memoryMappedViewStream.Dispose();

                action?.Invoke(x, diff);

                if (cancelToken.IsCancellationRequested)
                {
                    pls.Break();
                }
            });



            return diff;
        }

        /// <summary>
        /// 计算魔法棒结果
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public byte[][] CalaulateMagicCompareResult(float[][] diffs, float threshold, int erodeLevel)
        {
            int width = diffs.Length;
            int hight = diffs[0].Length;
            byte[][] nd = new byte[width][];

            for (int i = 0; i < width; i++)
            {
                nd[i] = new byte[hight];
            }

            //比较阈值
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < hight; j++)
                {
                    nd[i][j] = (byte)(diffs[i][j] > threshold ? 1 : 0);
                }
            }

            //腐蚀
            return Erode(erodeLevel, nd);

        }

        /// <summary>
        /// 二值化计算阈值
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="threshold"></param>
        /// <param name="rect"></param>
        /// <param name="inv"></param>
        /// <returns></returns>
        public byte[][] CalculateThresholdDiffs(byte[] data, int width, int height, Rectangle rect)
        {
            int stride = width * 3;
            

            int _x = rect.X;
            int _y = rect.Y;
            int _width = Math.Min(width, rect.Right) - _x;
            int _height = Math.Min(height, rect.Bottom) - _y;

            byte[][] td = new byte[_width][];
            for (int i = 0; i < _width; i++)
            {
                td[i] = new byte[_height];
            }

            Parallel.For(0, _height, delegate (int y)
            {
                for (int x = 0; x < _width; x++)
                {
                    int index = (y + _y) * stride + (x + _x) * 3;
                    byte threshold = (byte)((data[index] + data[index + 1] + data[index + 2]) / 3);
                    td[x][y] = threshold;
                }
            });
            return td;
        }

        /// <summary>
        /// 抠图计算结果
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public byte[][] CalaulateMattingCompareResult(Selecting selection)
        {
            byte[][] diffs = selection.ThresholdDiffs;
            byte threshold = selection.Threshold;
            int algorithm = selection.ThresholdAlgorithm;
            int erodeLevel = selection.ErodeLevel;

            int width = diffs.Length;
            int hight = diffs[0].Length;
            byte[][] nd = new byte[width][];

            for (int i = 0; i < width; i++)
            {
                nd[i] = new byte[hight];
            }

            //比较阈值
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < hight; j++)
                {
                    nd[i][j] = (byte)((diffs[i][j] < threshold && algorithm == 0) || (diffs[i][j] >= threshold && algorithm == 1) ? 1 : 0);
                }
            }

            //腐蚀
            return Erode(erodeLevel, nd);
        }

        /// <summary>
        /// 腐蚀
        /// </summary>
        /// <param name="kernal"></param>
        public byte[][] Erode(int size, byte[][] data)
        {
            if (size < 1)
                return data;

            size = size * 2 + 1;
            //kernal宽高必须是单数 比如 3*3  5*5
            byte[,] kernal = new byte[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernal[i, j] = 1;
                }
            }

            int kernalWidth = kernal.GetLength(0);
            int kernalHight = kernal.GetLength(1);
   

            int width = data.Length;
            int hight = data[0].Length;

            byte[][] nd = new byte[width][];
            for (int i = 0; i < width; i++)
            {
                nd[i] = new byte[hight];
            }

            int xr = (kernal.GetLength(0) - 1) / 2;
            int yr = (kernal.GetLength(1) - 1) / 2;

            Parallel.For(0, width, delegate (int x)
            {
                for (int y = 0; y < hight; y++)
                {
                    byte b = 1;
                    for (int j = 0; j < kernalHight; j++)
                    {
                        int _y = y - yr + j;
                        if (_y >= 0 && _y < hight)
                        {
                            for (int k = 0; k < kernalWidth; k++)
                            {
                                int _x = x - xr + k;
                                if (_x >= 0 && _x < width && data[_x][_y] != kernal[k, j])
                                {
                                    b = 0;
                                    break;
                                }
                            }

                            if (b == 0)
                            {
                                break;
                            }
                        }
                    }

                    nd[x][y] = b;
                }
         

                
            });

            return nd;
        }

        /// <summary>
        /// 腐蚀
        /// </summary>
        /// <param name="kernal"></param>
        public byte[][] ErodeV2(int size, byte[][] data)
        {
            if (size < 1)
                return data;

            size = size * 2 + 1;
            //kernal宽高必须是单数 比如 3*3  5*5
            byte[,] kernal = new byte[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernal[i, j] = 0;
                }
            }

            int kernalWidth = kernal.GetLength(0);
            int kernalHight = kernal.GetLength(1);

            int width = data.Length;
            int hight = data[0].Length;

            byte[][] nd = new byte[width][];
            for (int i = 0; i < width; i++)
            {
                nd[i] = new byte[hight];
            }

            int xr = (kernal.GetLength(0) - 1) / 2;
            int yr = (kernal.GetLength(1) - 1) / 2;

            Parallel.For(0, width, delegate (int x)
            {
                for (int y = 0; y < hight; y++)
                {
                    byte b = 1;
                    for (int j = 0; j < kernalHight; j++)
                    {
                        int _y = y - yr + j;
                        if (_y >= 0 && _y < hight)
                        {
                            for (int k = 0; k < kernalWidth; k++)
                            {
                                int _x = x - xr + k;
                                if (_x >= 0 && _x < width && data[_x][_y] == kernal[k, j])
                                {
                                    b = 0;
                                    break;
                                }
                            }
                            if (b == 0)
                            {
                                break;
                            }
                        }
                    }
                    if (b == 1)
                    {
                        nd[x][y] = data[x][y];
                    }
                    else
                    {
                        nd[x][y] = 0;
                    }
                }
            });
            return nd;
        }



    }
}
