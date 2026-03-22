using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public class ContourInfo
    {
        /// <summary>
        /// 轮廓数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 轮廓内所有点
        /// </summary>
        public System.Drawing.Point[][] AllPoints { get; set; }
        /// <summary>
        /// 轮廓所在矩形
        /// </summary>
        public Rectangle[] ContourRects { get; set; }
        /// <summary>
        /// 预览图片
        /// </summary>
        public Bitmap PreviewImage { get; set; }
    }

    public class OpenCV
    {

        public static ContourInfo FindContourInfo(Bitmap bitmap, float threshold, int erode, bool needPreview = false, bool needRect = false, bool needAllPoints = true)
        {
            ContourInfo info = new ContourInfo();
            Mat oriMat = BitmapConverter.ToMat(bitmap);
            // 提取轮廓
            OpenCvSharp.Point[][] contours = FindContours(oriMat, threshold, erode);
            info.Count = contours.Length;

            if (needAllPoints || needRect)
            {
                info.AllPoints = GetAllPoints(contours, out Rectangle[] rects);
                info.ContourRects = rects;
            }
            if (needPreview)
            {
                Cv2.DrawContours(oriMat, contours, -1, Scalar.Red, 2);
                info.PreviewImage = BitmapConverter.ToBitmap(oriMat);
            }
            oriMat.Release();

            return info;
        }



        /// <summary>
        /// 寻找轮廓(所有点)
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>Length 轮廓个数，Point[]轮廓内部点坐标</returns>
        public static System.Drawing.Point[][] FindContourPoints(Bitmap bitmap, float threshold, int erode)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Mat oriMat = BitmapConverter.ToMat(bitmap);
            // 提取轮廓
            OpenCvSharp.Point[][] contours = FindContours(oriMat, threshold, erode);
            stopwatch.Stop();
            Console.WriteLine("寻找轮廓时间" + stopwatch.ElapsedMilliseconds);
            stopwatch.Start();
            var allPoints = GetAllPoints(contours, out Rectangle[] rects);
            stopwatch.Stop();
            Console.WriteLine("寻找轮廓点时间" + stopwatch.ElapsedMilliseconds);
            oriMat.Dispose();
            return allPoints;
        }


        public static (OpenCvSharp.Point[][], Rect[]) FindContourRect(Bitmap bitmap, float threshold, int erode)
        {
            Mat oriMat = BitmapConverter.ToMat(bitmap);
            // 提取轮廓
            OpenCvSharp.Point[][] contours = FindContours(oriMat, threshold, erode);

            Rect[] rects = new Rect[contours.Length];
            for (int i = 0; i < contours.Length; i++)
            {
                rects[i] = Cv2.BoundingRect(contours[i]);
            }
            oriMat.Dispose();

            return (contours, rects);
        }

        // 判断点是否在多边形内  
        public static bool IsPointInPolygon(OpenCvSharp.Point[] polygon, int x, int y)
        {
            bool isInside = false;
            int j = polygon.Length - 1;

            for (int i = 0; i < polygon.Length; i++)
            {
                if (((polygon[i].Y <= y && y < polygon[j].Y) || (polygon[j].Y <= y && y < polygon[i].Y)) &&
                    (x < (polygon[j].X - polygon[i].X) * (y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }

                j = i;
            }

            return isInside;
        }

        public static bool IsPointPolygonTest(OpenCvSharp.Point[] polygon, int x, int y)
        {
            return Cv2.PointPolygonTest(polygon, new OpenCvSharp.Point(x, y) , false) >= 0;
        }

        /// <summary>
        /// 寻找轮廓（最外一圈轮廓的点）
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="threshold"></param>
        /// <param name="erode"></param>
        /// <returns></returns>
        private static OpenCvSharp.Point[][] FindContours(Mat oriMat, float threshold, int erode)
        {
            // 转换为灰度图
            Mat grayMat = new Mat();
            Cv2.CvtColor(oriMat, grayMat, ColorConversionCodes.BGR2GRAY);

            // 阈值处理
            Mat binaryMat = new Mat();
            threshold = threshold < 0 ? 0 : threshold;
            threshold = threshold > 255 ? 255 : threshold;
            Cv2.Threshold(grayMat, binaryMat, threshold, 255, ThresholdTypes.Binary);

            // 腐蚀操作
            Mat erodedMat = new Mat();
            if (erode > 0)
            {
                erode = erode * 2 + 1;
                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(erode, erode));
                Cv2.Erode(binaryMat, erodedMat, kernel);
            }
            else
            {
                erodedMat = binaryMat;
            }

            // 提取轮廓
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(erodedMat, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            //获取轮廓内所有的点
            //var isFind = erodedMat.FindNonZero().GetArray(out OpenCvSharp.Point[] points);

            //Cv2.ContourArea(contours[i])>10
            //Rect boundingRect = Cv2.BoundingRect(contours[i]);
            try
            {
                erodedMat.Release();
                binaryMat.Release();
                grayMat.Release();
            }
            catch (Exception)
            {

            }


            return contours;
        }

        private static System.Drawing.Point[][] GetAllPoints(OpenCvSharp.Point[][] contours, out Rectangle[] rectangles)
        {
            System.Drawing.Point[][] allPoints = null;
            rectangles = null;
            if (contours.Length > 0)
            {
                allPoints = new System.Drawing.Point[contours.Length][];
                List<List<System.Drawing.Point>> pointsInsideContours = new List<List<System.Drawing.Point>>();
                var rects = new Rectangle[contours.Length];
                for (int i = 0; i < contours.Length; i++)
                {
                    var pointsInsideContour = new List<System.Drawing.Point>();
                    pointsInsideContours.Add(pointsInsideContour);
                }
                // 遍历图像中的每个像素，检查其是否在轮廓内
                Parallel.For(0, contours.Length, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (int i) =>
                {
                    int minX = contours[i][0].X;
                    int maxX = contours[i][0].X;
                    int minY = contours[i][0].Y;
                    int maxY = contours[i][0].Y;

                    for (int j = 1; j < contours[i].Length; j++)
                    {
                        if (contours[i][j].X < minX)
                            minX = contours[i][j].X;
                        else if (contours[i][j].X > maxX)
                            maxX = contours[i][j].X;

                        if (contours[i][j].Y < minY)
                            minY = contours[i][j].Y;
                        else if (contours[i][j].Y > maxY)
                            maxY = contours[i][j].Y;
                    }
                    rects[i] = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                    for (int x = minX; x <= maxX; x++)
                    {
                        for (int y = minY; y <= maxY; y++)
                        {
                            var currentPoint = new OpenCvSharp.Point(x, y);
                            // 检查当前点是否在轮廓内
                            //if (IsPointInPolygon(contours[i], currentPoint))
                            if (Cv2.PointPolygonTest(contours[i], currentPoint, false) >= 0)
                            {
                                pointsInsideContours[i].Add(new System.Drawing.Point(x, y));
                            }
                        }
                    }
                });

                for (int i = 0; i < contours.Length; i++)
                {
                    allPoints[i] = pointsInsideContours[i].ToArray();
                }
                rectangles = rects;

            }
            else
            {
                allPoints = new System.Drawing.Point[0][];
            }

            return allPoints;
        }



    }
}
