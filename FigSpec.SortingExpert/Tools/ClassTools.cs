using FigSpec.SortingExpert.Entities;
using ImgView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hyperspectral.SpectralFile;
using Hyperspectral.SpectralProc;
using System.Drawing;
using FigSpec.SortingExpert.AppCode;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Globalization;

namespace FigSpec.SortingExpert.Tools
{
    public class ClassTools
    {

        private static ClassIdAndReflectivity AddReflectivity(Selecting selecting, float[] reflectivity)
        {
            var set = GlobalSettings.ApplySetting.traingSet;
            int length = set.algorithm.EndBandIndex - set.algorithm.StartBandIndex + 1;
            float[] temp = new float[length];
            for (int s = 0; s < length; s++)
            {
                temp[s] = reflectivity[s + set.algorithm.StartBandIndex];
            }
            if (GlobalSettings.ApplySetting.traingSet.Preprocessings.Count > 0)
            {
                temp = PreprocessingHelper.GetPreprocessingReflectSingle(temp, GlobalSettings.ApplySetting.traingSet.Preprocessings);
            }
            return new ClassIdAndReflectivity() { ClassId = selecting.classID, Reflectivity = temp };
        }


        public static List<ClassIdAndWaveLengthIndex> WriteToCsvFile(string dir, SPE spe, CancellationTokenSource tokenSource)
        {
            var selectings = GlobalSettings.ApplySetting.traingSet.label.selections;
            int height = spe.Samples;
            int width = spe.Lines;
            //获取所有数据
            List<ClassIdAndReflectivity> datas = new List<ClassIdAndReflectivity>();
            foreach (var item in selectings)
            {
                if (tokenSource.IsCancellationRequested) return null;
                switch (item.labelType)
                {
                    case SelectionType.点:
                        datas.Add(AddReflectivity(item, spe[item.Points[0].X, item.Points[0].Y]));
                        break;
                    case SelectionType.魔法棒:
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                if (item.CompareResult[x][y] == 1)
                                {
                                    datas.Add(AddReflectivity(item, spe[x, y]));
                                }
                                if (tokenSource.IsCancellationRequested) return null;
                            }
                        }
                        break;
                    case SelectionType.矩形:
                        if (item.ContourEnabled)
                        {
                            foreach (var refItem in item.reflects)
                            {
                                datas.Add(AddReflectivity(item, refItem));
                                if (tokenSource.IsCancellationRequested) return null;
                            }
                        }
                        else
                        {
                            int xx = item.AbsoluteRectangle.X;
                            int yy = item.AbsoluteRectangle.Y;
                            int wid = item.AbsoluteRectangle.Width;
                            int heig = item.AbsoluteRectangle.Height;
                            for (int x = xx; x < xx + wid; x++)
                            {
                                for (int y = yy; y < yy + heig; y++)
                                {
                                    datas.Add(AddReflectivity(item, spe[x, y]));
                                    if (tokenSource.IsCancellationRequested) return null;
                                }
                            }
                        }
                        break;
                    case SelectionType.抠图:
                        if (item.ContourEnabled)
                        {
                            foreach (var refItem in item.reflects)
                            {
                                datas.Add(AddReflectivity(item, refItem));
                                if (tokenSource.IsCancellationRequested) return null;
                            }
                        }
                        else
                        {
                            foreach (var point in item.Points)
                            {
                                datas.Add(AddReflectivity(item, spe[point.X, point.Y]));
                                if (tokenSource.IsCancellationRequested) return null;
                            }
                        }
                        break;
                    case SelectionType.点选区:
                        if (item.ContourEnabled)
                        {
                            foreach (var refItem in item.reflects)
                            {
                                datas.Add(AddReflectivity(item, refItem));
                                if (tokenSource.IsCancellationRequested) return null;
                            }
                        }
                        else
                        {
                            foreach (var point in item.Points)
                            {
                                datas.Add(AddReflectivity(item, spe[point.X, point.Y]));
                                if (tokenSource.IsCancellationRequested) return null;
                            }
                        }
                        break;
                    case SelectionType.画笔:
                        foreach (var point in item.Points)
                        {
                            datas.Add(AddReflectivity(item, spe[point.X, point.Y]));
                            if (tokenSource.IsCancellationRequested) return null;
                        }
                        break;
                    case SelectionType.轮廓:
                        if (item.ContourPoints?.Length > 0)
                        {
                            if (item.ContourEnabled)
                            {
                                foreach (var refItem in item.reflects)
                                {
                                    datas.Add(AddReflectivity(item, refItem));
                                    if (tokenSource.IsCancellationRequested) return null;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < item.ContourPoints.Length; i++)
                                {
                                    foreach (var point in item.ContourPoints[i])
                                    {
                                        datas.Add(AddReflectivity(item, spe[point.X, point.Y]));
                                        if (tokenSource.IsCancellationRequested) return null;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            //创建目录
            //string dir = Path.Combine(Path.GetDirectoryName(fileName), "Temp");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FileTool.DeleteAllFiles(dir, ".csv");

            List<ClassIdAndWaveLengthIndex> indexs = new List<ClassIdAndWaveLengthIndex>();

            var set = GlobalSettings.ApplySetting.traingSet;
            //分别存储
            foreach (var labelClass in set.label.classes)
            {
                if (tokenSource.IsCancellationRequested) return null;
                if (!labelClass.CanDelete)
                    continue;
                if (!set.label.selections.Exists(i => i.classID == labelClass.id))
                    continue;

                //文件名称
                string modelFileName = Path.Combine(dir, $"{labelClass.id}.csv");

                //所用波段
                var model = set.algorithm.train_bands.Bands.Find(i => i.ClassId == labelClass.id);
                List<int> waveLengthIndexs = new List<int>();
                if (model == null || model.WaveLengthIndexs.Count == 0)
                {
                    for (int i = 0; i < set.algorithm.train_bands.WaveLength.Length; i++)
                    {
                        if (i < set.algorithm.StartBandIndex || i > set.algorithm.EndBandIndex)
                            continue;
                        waveLengthIndexs.Add(i);
                    }
                }
                else
                {
                    waveLengthIndexs.AddRange(model.WaveLengthIndexs.OrderBy(i => i));
                }
                var listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

                int startIndex = set.algorithm.StartBandIndex;
                indexs.Add(new ClassIdAndWaveLengthIndex() { ClassId = labelClass.id, WaveLengthIndexs = waveLengthIndexs });
                StringBuilder builder = new StringBuilder();
                using (StreamWriter streamWriter = new StreamWriter(modelFileName, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
                {
                    foreach (var data in datas)
                    {
                        builder.Append(data.ClassId == labelClass.id ? "1" : "0");
                        foreach (var index in waveLengthIndexs)
                        {
                            builder.Append(listSeparator);
                            builder.Append(data.Reflectivity[index - startIndex]); 
                        }
                        streamWriter.WriteLine(builder.ToString());
                        builder.Clear();
                    }
                }
            }


            return indexs;
        }

        public static void ExportData(float[,] data, int startIndex, int endIndex)
        {
            int rows = data.GetLength(0);

            StringBuilder builder = new StringBuilder();
            using (StreamWriter streamWriter = new StreamWriter(@"D:\22.csv", append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = startIndex; j <= endIndex; j++)
                    {
                        builder.Append(data[i, j]);
                        if (j < endIndex)
                            builder.Append(",");
                    }
                    streamWriter.WriteLine(builder.ToString());
                    builder.Clear();
                }
            }
        }


        //public static byte[,] SelectArea(int width, int height, List<Selecting> datas)
        //{
        //    var td = new byte[height, width];
        //    if (datas != null)
        //    {

        //        foreach (var item in datas)
        //        {
        //            if (item.labelType == SelectionType.点)
        //            {
        //                td[item.Points[0].Y, item.Points[0].X] = (byte)item.classID;
        //            }
        //            else if (item.labelType == SelectionType.魔法棒)
        //            {
        //                Parallel.For(0, height, (int y) =>
        //                {
        //                    for (int x = 0; x < width; x++)
        //                    {
        //                        if (item.CompareResult[x][y] == 1)
        //                        {
        //                            td[y, x] = (byte)item.classID;
        //                        }
        //                    }
        //                });
        //            }
        //            else if (item.labelType == SelectionType.画笔)
        //            {
        //                foreach (var point in item.Points)
        //                {
        //                    td[point.Y, point.X] = (byte)item.classID;
        //                }
        //            }
        //            else if (item.labelType == SelectionType.矩形)
        //            {
        //                int xx = item.AbsoluteRectangle.X;
        //                int yy = item.AbsoluteRectangle.Y;
        //                int wid = item.AbsoluteRectangle.Width;
        //                int heig = item.AbsoluteRectangle.Height;
        //                Parallel.For(xx, xx + wid, (int x) =>
        //                {
        //                    for (int y = yy; y < yy + heig; y++)
        //                    {
        //                        td[y, x] = (byte)item.classID;
        //                    }
        //                });
        //            }
        //            else if (item.labelType == SelectionType.抠图)
        //            {
        //                foreach (var point in item.Points)
        //                {
        //                    td[point.X, point.Y] = (byte)item.classID;
        //                }
        //            }
        //            else if (item.labelType == SelectionType.点选区)
        //            {
        //                if (item.Points?.Length > 0)
        //                {
        //                    foreach (var point in item.Points)
        //                    {
        //                        if (point.X < width && point.Y < height)
        //                        {
        //                            td[point.Y, point.X] = (byte)item.classID;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return td;
        //}

        /// <summary>
        /// 抠图类型的计算反射率均值
        /// </summary>
        public static void GetReflectBySelectMattingArea(Selecting currSelection, SPE spe, SpectraOfSPE spectraOfSPE)
        {
            if (currSelection.labelType != SelectionType.抠图 || currSelection.CompareResult == null)
                return;

            int _x = currSelection.AbsoluteRectangle.X;
            int _y = currSelection.AbsoluteRectangle.Y;
            int _width = currSelection.AbsoluteRectangle.Width;
            int _height = currSelection.AbsoluteRectangle.Height;

            List<Point> points = new List<Point>();
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (currSelection.CompareResult[x][y] == 1)
                    {
                        points.Add(new Point(x + _x, y + _y));
                    }
                }
            }
            currSelection.reflects.Clear();
            currSelection.Points = points.ToArray();
            int splitCount = GlobalSettings.ApplySetting.traingSet.algorithm.split_point_count;
            if (currSelection.ContourEnabled && points.Count > splitCount)
            {
                while (points.Count >= splitCount)
                {
                    var ps = points.Take(splitCount).ToArray();
                    currSelection.reflects.Add(spectraOfSPE.GetAverageData(spe, ps));
                    points.RemoveRange(0, splitCount);
                }
            }
            else if (points.Count > 0)
            {
                currSelection.reflects.Add(spectraOfSPE.GetAverageData(spe, points.ToArray()));
            }
            else
            {
                currSelection.reflects.Clear();
            }
        }

        /// <summary>
        /// 矩形计算反射率均值
        /// </summary>
        /// <param name="currSelection"></param>
        /// <param name="spe"></param>
        /// <param name="spectraOfSPE"></param>
        public static void GetReflectBySelectRectArea(Selecting currSelection, SPE spe, SpectraOfSPE spectraOfSPE)
        {
            if (currSelection.labelType != SelectionType.矩形)
                return;

            int _x = currSelection.AbsoluteRectangle.X;
            int _y = currSelection.AbsoluteRectangle.Y;
            int _width = currSelection.AbsoluteRectangle.Width;
            int _height = currSelection.AbsoluteRectangle.Height;
            int sum = _width * _height;
            currSelection.reflects.Clear();
            int splitCount = GlobalSettings.ApplySetting.traingSet.algorithm.split_point_count;
            if (currSelection.ContourEnabled && sum > splitCount)
            {
                List<Point> ps = new List<Point>();
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        ps.Add(new Point(x + _x, y + _y));
                        if (ps.Count == splitCount)
                        {
                            currSelection.reflects.Add(spectraOfSPE.GetAverageData(spe, ps.ToArray()));
                            ps.Clear();
                        }
                    }
                }
            }
            else
            {
                currSelection.reflects.Add(spectraOfSPE.GetAverageData(spe, currSelection.AbsoluteRectangle));
            }
        }

        /// <summary>
        /// 多点类型计算反射率均值
        /// </summary>
        /// <param name="currSelection"></param>
        /// <param name="spe"></param>
        /// <param name="spectraOfSPE"></param>
        public static void GetReflectBySelectPointsArea(Selecting currSelection, SPE spe, SpectraOfSPE spectraOfSPE)
        {
            if (currSelection.Points == null || currSelection.Points.Length == 0)
                return;
            currSelection.reflects.Clear();
            int splitCount = GlobalSettings.ApplySetting.traingSet.algorithm.split_point_count;
            if (currSelection.ContourEnabled && currSelection.Points.Length > splitCount)
            {
                int count = 0;
                while (count + splitCount < currSelection.Points.Length)
                {
                    var ps = currSelection.Points.Skip(count).Take(splitCount).ToArray();
                    currSelection.reflects.Add(spectraOfSPE.GetAverageData(spe, ps));
                    count += splitCount;
                }
            }
            else
            {
                currSelection.reflects.Add(spectraOfSPE.GetAverageData(spe, currSelection.Points));
            }
        }

        /// <summary>
        /// 轮廓计算平均反射率
        /// </summary>
        /// <param name="currSelection"></param>
        /// <param name="spe"></param>
        /// <param name="spectraOfSPE"></param>
        public static void GetReflectBySelectOutlineArea(Selecting currSelection, SPE spe, SpectraOfSPE spectraOfSPE)
        {
            if (currSelection.ContourPoints == null || currSelection.ContourPoints.Length == 0)
                return;
            currSelection.reflects.Clear();
            int splitCount = GlobalSettings.ApplySetting.traingSet.algorithm.split_point_count;
            if (currSelection.ContourEnabled)
            {
                for (int i = 0; i < currSelection.ContourPoints.Length; i++)
                {
                    if (currSelection.ContourPoints[i].Length > splitCount)
                    {
                        int count = 0;
                        while (count + splitCount < currSelection.ContourPoints[i].Length)
                        {
                            var ps = currSelection.ContourPoints[i].Skip(count).Take(splitCount).ToArray();
                            currSelection.reflects.Add(spectraOfSPE.GetAverageData(spe, ps));
                            count += splitCount;
                        }
                    }
                }
                
            }
            else
            {
                for (int i = 0; i < currSelection.ContourPoints.Length; i++)
                {
                    currSelection.reflects.Add(spectraOfSPE.GetAverageData(spe, currSelection.ContourPoints[i]));
                }
                    
            }
        }


        /// <summary>
        /// 点转换
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static PointSelection GetPointSelection(Selecting selection)
        {
            if (selection.Points.Length == 0)
                return null;
            var s = new PointSelection()
            {
                Guid = selection.uid,
                Type = SelectionType.点,
                SelectionColor = Color.FromArgb(selection.color),
                IsSelected = false,
                Visible = true,
                AbsolutePointF = selection.Points[0]
            };
            return s;
        }

        public static BrushSelection GetLineSelection(Selecting selection)
        {
            if (selection.Points.Length == 0)
                return null;
            var points = selection.Points.Select(i => new PointF(i.X, i.Y)).ToList();
            var s = new BrushSelection()
            {
                Guid = selection.uid,
                Type = SelectionType.画笔,
                SelectionColor = Color.FromArgb(selection.color),
                IsSelected = false,
                Visible = true,
                AbsolutePointFs = points
            };
            return s;
        }

        public static RectangularSelection GetRectangleSelection(Selecting selection)
        {
            var s = new RectangularSelection()
            {
                Guid = selection.uid,
                Type = SelectionType.矩形,
                SelectionColor = Color.FromArgb(selection.color),
                IsSelected = false,
                Visible = true,
                AbsoluteRectangleF = selection.AbsoluteRectangle
            };
            return s;
        }

        public static BrushSelection GetPointsTolineSelection(Selecting selection)
        {
            if (selection.Points.Length == 0)
                return null;
            var points = selection.Points.Select(i => new PointF(i.X, i.Y)).ToList();
            var s = new BrushSelection()
            {
                Guid = selection.uid,
                Type = SelectionType.点选区,
                SelectionColor = Color.FromArgb(selection.color),
                IsSelected = false,
                Visible = true,
                IsAllPoints = true,
                AbsolutePointFs = points
            };
            return s;
        }

        public static IrregularSelection GetOutlineSelection(Selecting selection)
        {
            if (selection.ContourPoints.Length == 0)
                return null;
            List<Point> points = new List<Point>();
            foreach (var item in selection.ContourPoints)
            {
                points.AddRange(item);
            }
            var s = new IrregularSelection()
            {
                Guid = selection.uid,
                Type = SelectionType.轮廓,
                SelectionColor = Color.FromArgb(selection.color),
                IsSelected = false,
                Visible = true,
                AbsolutePoints = points,
                AbsoluteRectangleF = selection.AbsoluteRectangle
            };
            return s;
        }

        public static IrregularSelection GetMattingSelection(Selecting selection)
        {
            if (selection.Points.Length == 0)
                return null;
            var points = selection.Points.Select(i => new PointF(i.X, i.Y)).ToList();
            var s = new IrregularSelection()
            {
                Guid = selection.uid,
                Type = SelectionType.抠图,
                SelectionColor = Color.FromArgb(selection.color),
                IsSelected = false,
                Visible = true,
                AbsolutePoints = selection.Points.ToList(),
                AbsoluteRectangleF = selection.AbsoluteRectangle
            };
            return s;
        }
    }
}
