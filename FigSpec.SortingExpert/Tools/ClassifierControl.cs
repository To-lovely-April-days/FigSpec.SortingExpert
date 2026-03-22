using CHNSpec.Tools;
using FigSpec.SortingExpert.Tools.Client;
using Globalization;
using SerialPortLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public class ClassifierControl : IDisposable
    {
        private ClassifierControl()
        {
#if XIWANG
            AirCtrl = new XiWangEjectDevice();
#elif JIUSHUN
            AirCtrl = new JiuShunEjectDevice();
#else
            AirCtrl = new ChspecEjectDevice();
#endif
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        //吹气控制仿真图
        public Action<byte[], byte[]> SimulationBlow;
        /// <summary>
        /// 延时打击的时长(ms)
        /// </summary>
        public uint AirDelay { get; set; }
        /// <summary>
        /// 打击持续时长(ms)
        /// </summary>
        public uint AirDuration { get; set; }
        /// <summary>
        /// 气管个数
        /// </summary>
        public int AirCount { get; set; }
        /// <summary>
        /// 气吹控制串口
        /// </summary>

        public static int AirBaudRate = 115200;


        private IBaseEjectDevice AirCtrl;

        public ConcurrentQueue<(bool isContinue, byte[] data, long timestamp)> AirQueue = new ConcurrentQueue<(bool isContinue, byte[] data, long timestamp)>();

        /// <summary>
        /// 气吹状态
        /// </summary>
        private byte[] airsState;
        /// <summary>
        /// 像素状态
        /// </summary>
        private byte[] preSampleState;
        /// <summary>
        /// 连续多少帧像素相邻
        /// </summary>
        private int activatePixelsX = 20;
        /// <summary>
        /// 每帧多少像素相邻
        /// </summary>
        private int activatePixelsY = 10;


        private bool AirStopToken = true;
        /// <summary>
        /// 像素，气吹口对应关系
        /// </summary>
        public Dictionary<int, List<int>> pixelAirMap;

        private static ClassifierControl shared;
        public static ClassifierControl Shared
        {
            get
            {
                if (shared == null)
                    shared = new ClassifierControl();
                return shared;
            }
        }
        public void StartAirProcess()
        {
            SetAirParams();

            AirStopToken = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(AirProcessWork), null);
        }

        public void SetAirParams()
        {
            var setting = GlobalSettings.ApplySetting;
            AirDelay = setting.EjectAirDelay;
            AirDuration = setting.EjectAirTime;
            activatePixelsX = setting.ActivatePixelsX;
            activatePixelsY = setting.ActivatePixelsY;
            airsState = new byte[setting.TracheaSet.Trachea];
            preSampleState = new byte[setting.TracheaSet.PixelNumber];
            AirCount = setting.TracheaSet.Trachea;
            pixelAirMap = setting.TracheaSet.GetDictionaryItems();
        }

        bool pauseFlag = false; //是否暂停
        bool pauseing = false; //是否暂停中
        public void ApplyAirParams()
        {
            pauseFlag = true;
            //线程进入暂停中
            if (!AirStopToken)
            {
                while (!pauseing)
                {
                    //等在线程进入暂停中
                    Thread.Sleep(1);
                }
            }
            SetAirParams();
            pauseFlag = false;
        }

        public void StopAirProcess()
        {
            AirStopToken = true;
        }

        public void AirProcessWork(object obj)
        {
            var ass = GlobalSettings.ApplySetting;
            long overtime = 10000000 / ass.SortCameraSetting.FrameRate * 100;// 600000;//最长超时时间
            long start_time = 0;
            long timestamp = 0;
            byte[] frame = null;
            bool usefullFlag = false;//数据是否还有用
            while (!AirStopToken)
            {
                while (pauseFlag)
                {
                    //暂停中
                    pauseing = true;
                    Thread.Sleep(1);
                }
                pauseing = false;
                try
                {
                    start_time = DateTime.Now.Ticks;
                    long delay = AirDelay * 10000;//需要延迟的时间
                    long currentTimeLong = DateTime.Now.Ticks;
                    {
                        if (!usefullFlag)
                        {
                            if (AirQueue.Count == 0)
                            {
                                Thread.Sleep(1);
                                continue;
                            }
                            //(frame, timestamp) = AirQueue.First();
                            AirQueue.TryDequeue(out var removeData);
                            frame = removeData.data;
                            timestamp = removeData.timestamp;
#if DEBUG
                            LogHelper.WriteLine($"TOTAL-TIME-一帧数据从采样到吹气的总时间：" + (currentTimeLong - timestamp) / 10);
#endif

                            if (!removeData.isContinue)
                            {
                                Array.Clear(airsState, 0, airsState.Length);
                                Array.Clear(preSampleState, 0, preSampleState.Length);
                            }
                        }
                        if (currentTimeLong < timestamp + delay) //时间未到，就等待
                        {
                            usefullFlag = true;
                            continue;
                        }
                        else if (currentTimeLong > timestamp + delay + overtime)//超时，直接丢弃
                        {
                            usefullFlag = false;
                            continue;
                        }
                        usefullFlag = false;
                    }
#if DEBUG
                    LogHelper.WriteLine($"chui-取数据时间：" + (DateTime.Now.Ticks - start_time));
#endif

                    byte[] temp = new byte[preSampleState.Length];
                    for (int i = 0; i < AirCount; i++)
                    {
                        if (airsState[i] > 0)
                        {
                            //吹过气
                            airsState[i] -= 1;
                            continue;
                        }
                        if (pixelAirMap[i].Count == 0)
                            continue;

                        {   //判断单帧方向上像素是否连续
                            int sIndex = pixelAirMap[i][0] - activatePixelsY + 1;
                            int eIndex = pixelAirMap[i][pixelAirMap[i].Count - 1] + activatePixelsY - 1;
                            sIndex = sIndex < 0 ? 0 : sIndex;
                            eIndex = eIndex < preSampleState.Length ? eIndex : preSampleState.Length - 1;
                            int count = 0;
                            for (int y = sIndex; y <= eIndex; y++)
                            {
                                count = frame[y] > 0 ? (count + 1) : 0;
                                if (count >= activatePixelsY)
                                {
                                    airsState[i] = (byte)activatePixelsX;
                                    Array.Clear(temp, pixelAirMap[i][0], pixelAirMap[i].Count);
                                    break;
                                }
                            }
                        }
                        if (airsState[i] > 0)
                        {
                            continue;
                        }

                        {   //判断帧数方向上是否连续
                            for (int j = 0; j < pixelAirMap[i].Count; j++)
                            {
                                var index = pixelAirMap[i][j];
                                if (frame[index] == 0)
                                    continue;
                                var maxValue = Math.Max(TryGetValue(preSampleState, index - 1), Math.Max(preSampleState[index], TryGetValue(preSampleState, index + 1)));
                                if (maxValue == activatePixelsX - 1)
                                {
                                    airsState[i] = (byte)activatePixelsX;
                                    Array.Clear(temp, pixelAirMap[i][0], pixelAirMap[i].Count);
                                    break;
                                }
                                else
                                {
                                    temp[index] = (byte)(maxValue + 1);
                                }
                            }
                        }
                    }
                    Array.Copy(temp, preSampleState, temp.Length);

#if DEBUG
                    LogHelper.WriteLine($"chui-判断吹气孔的时间：" + (DateTime.Now.Ticks - start_time));
#endif
                    List<int> arrejt = new List<int>();
                    for (int p = 0; p < AirCount; p++)
                    {
                        if (airsState[p] == activatePixelsX)
                        {
                            arrejt.Add(p);
                        }
                    }
                    if (arrejt.Count > 0)
                    {
                        Eject(arrejt.ToArray(), (ushort)AirDuration);
                    }
#if DEBUG
                    Console.Write($"chui-一帧的吹气状态：" + string.Join(",", airsState) + "\r\n");
                    Console.Write($"chui-一帧的吹气时间： {(DateTime.Now.Ticks - start_time)}" + "\r\n");
#endif
                }
                catch(Exception e)
                {
                    Trace.WriteLine($"air err "+ e.Message.ToString());
                }
            }
        }


        private byte TryGetValue(byte[] array, int index)
        {
            if (index >= 0 && index < array.Length)
                return array[index];
            else
                return 0;
        }


        public bool IsConnected => AirCtrl.IsConnected;

        public bool ConnectEjectDevice(string com)
        {
            if (AirCtrl.IsConnected)
                return true;
            try
            {
                bool isSiuccess = AirCtrl.OpenByCom(com, AirBaudRate);
                if (!isSiuccess)
                {
                    Trace.WriteLine("初始化气吹控制失败");
                    return false;
                }
                return true;
            }
            catch
            {
                Trace.WriteLine("初始化气吹控制失败");
                return false;
            }
        }

        public bool ConnectEjectDevice(string ip, int port)
        {
            if (AirCtrl.IsConnected)
                return true;
            try
            {
                bool isSiuccess = AirCtrl.OpenByTcp(ip, port);
                if (!isSiuccess)
                {
                    Trace.WriteLine("初始化气吹控制失败");
                    return false;
                }
                return true;
            }
            catch
            {
                Trace.WriteLine("初始化气吹控制失败");
                return false;
            }
        }

        public void DisConnectEjectDevice()
        {
            if (AirCtrl.IsConnected)
            {
                try
                {
                    AirCtrl.Close();
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// 打开指定气吹
        /// </summary>
        /// <param name="index">气吹编号</param>
        /// <param name="duration">吹气时长(ms)</param>
        /// <param name="delay">延时(ms)</param>
        public void Eject(int[] ports, ushort time)
        {
            bool isSuccess = AirCtrl.Eject(ports, time);
            if (!isSuccess)
            {
                LogHelper.WriteLog("初始化气吹控制失败".ToMultiLanguage());
            }
        }
        public void Set(int[] devices, int[] data)
        {
            bool isSuccess = AirCtrl.Set(devices, data);
            if (!isSuccess)
            {
                LogHelper.WriteLog("初始化气吹控制失败".ToMultiLanguage());
            }
        }
    }
}
