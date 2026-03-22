using System;
using System.Collections.Generic;
using System.Linq;
using Hyperspectral.SpectralFile;
using Hyperspectral.SpectralProc;
using System.Text;
using System.Threading.Tasks;
using Hyperspectral.Tools;
using System.Drawing;
using System.Drawing.Imaging;

namespace FigSpec.SortingExpert.AppCode
{
    public static class SpeExtend
    {
        /// <summary>
        /// 显示校准后的图像
        /// </summary>
        /// <param name="rc"></param>
        /// <param name="gc"></param>
        /// <param name="bc"></param>
        /// <param name="rt"></param>
        /// <param name="gt"></param>
        /// <param name="bt"></param>
        /// <param name="whiteRef"></param>
        /// <param name="whiteRawSignal"></param>
        /// <param name="blackRawSignal"></param>
        /// <param name="onProgress"></param>
        /// <param name="interval"></param>
        public static byte[] PreviewRefRGBOfLine(SPE spe, int rc, int gc, int bc, float rt, float gt, float bt)
        {
            //单个波长的信号量大小
            int sizeOfBand = spe.SizeOfBand;

            var stride = spe.Lines * 3;
            var data = new byte[spe.Samples * stride];
            unsafe
            {
                Parallel.For(0, spe.Lines, (int x) => {
                    //样品的原始指针
                    long offset = (long)x * spe.SizeOfLine;
                    var stream = spe.Raw.CreateViewStream(offset, spe.SizeOfLine);
                    byte* ptr = null;
                    stream.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                    ptr += stream.PointerOffset;

                    for (int s = 0; s < spe.Samples; s++)
                    {
                        float* _ptr = (float*)ptr;
                        data[s * stride + x * 3] = (*(_ptr + (bc * spe.Samples + s)) / bt * 255).ToByte();
                        data[s * stride + x * 3 + 1] = (*(_ptr + (gc * spe.Samples + s)) / gt * 255).ToByte();
                        data[s * stride + x * 3 + 2] = (*(_ptr + (rc * spe.Samples + s)) / rt * 255).ToByte();
                    }
                   
                    stream.SafeMemoryMappedViewHandle.ReleasePointer();
                    stream.Dispose();
                });
            }

            return data;
        }


        /// <summary>
        /// 扣除背景转图片
        /// </summary>
        public unsafe static Bitmap BackgroundCorrectionToBitmap(SPE spe, (int r, int g, int b) channel, (float r, float g, float b) threshold, int startBandIndex, int endBandIndex, bool backgroundCorrection, float sThreshold, float eThreshold)
        {
            var bitmap = new Bitmap(spe.Lines, spe.Samples, PixelFormat.Format24bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var imagePtr = (byte*)bitmapData.Scan0;

            Parallel.For(0, spe.Lines, (x) =>
            {
                long offset = (long)x * spe.SizeOfLine;
                var stream = spe.Raw.CreateViewStream(offset, spe.SizeOfLine);
                byte* ptr = null;
                stream.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                ptr += stream.PointerOffset;

                for (int y = 0; y < spe.Samples; y++)
                {
                    byte r = 0, g = 0, b = 0;
                    var offsetR = y + channel.r * spe.Samples;
                    var offsetG = y + channel.g * spe.Samples;
                    var offsetB = y + channel.b * spe.Samples;
                    if (spe.DataType == 4)
                    {
                        float* _ptr = (float*)ptr;
                        r = (*(_ptr + offsetR) / threshold.r * 255).ToByte();
                        g = (*(_ptr + offsetG) / threshold.g * 255).ToByte();
                        b = (*(_ptr + offsetB) / threshold.b * 255).ToByte();
                        if (backgroundCorrection)
                        {
                            float sum = 0f;
                            for (int k = startBandIndex; k <= endBandIndex; k++)
                            {
                                sum += *(_ptr + y + k * spe.Samples);
                            }
                            float averageValue = sum / (endBandIndex - startBandIndex + 1);

                            if (averageValue < eThreshold && averageValue > sThreshold)
                            {
                                //扣除背景
                                r = g = b = 0;
                            }
                        }

                    }
                    else if (spe.DataType == 12 || spe.DataType == 14)
                    {
                        ushort* _ptr = (ushort*)ptr;
                        r = (*(_ptr + offsetR) / threshold.r * 255).ToByte();
                        g = (*(_ptr + offsetG) / threshold.g * 255).ToByte();
                        b = (*(_ptr + offsetB) / threshold.b * 255).ToByte();
                    }
                    else if (spe.DataType == 1 || spe.DataType == 0)
                    {
                        r = (*(ptr + offsetR) / threshold.r * 255).ToByte();
                        g = (*(ptr + offsetG) / threshold.g * 255).ToByte();
                        b = (*(ptr + offsetB) / threshold.b * 255).ToByte();
                    }

                    *(imagePtr + y * bitmapData.Stride + x * 3) = b;
                    *(imagePtr + y * bitmapData.Stride + x * 3 + 1) = g;
                    *(imagePtr + y * bitmapData.Stride + x * 3 + 2) = r;
                }

                stream.SafeMemoryMappedViewHandle.ReleasePointer();
                stream.Dispose();
            });

            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }


        public static void SpeDispose(this SPE spe)
        {
            if (spe != null && spe.Raw != null)
                spe.Dispose();
        }

    }



}


