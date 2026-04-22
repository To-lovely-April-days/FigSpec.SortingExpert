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
        /// 吹气目标类别集合。
        /// null 或空集 = 吹所有前景类别（默认行为）
        /// 非空 = 只吹集合内的类别
        /// </summary>
        public HashSet<byte> EjectTargetClassIds { get; set; } = null;
        /// <summary>
        /// 气吹控制串口
        /// </summary>

        public static int AirBaudRate = 115200;


        private IBaseEjectDevice AirCtrl;

        public ConcurrentQueue<(bool isContinue, byte[] data, long timestamp)> AirQueue = new ConcurrentQueue<(bool isContinue, byte[] data, long timestamp)>();
        /// <summary>
        /// AirQueue 入队唤醒事件。
        /// EjectForm.SendEjectData 里 Enqueue 之后 Set,AirProcessWork 里 WaitOne。
        /// 取代原来的 Thread.Sleep(1) 轮询,降低抖动。
        /// </summary>
        public AutoResetEvent AirQueueEvent = new AutoResetEvent(false);

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
            long overtime = 10000000 / ass.SortCameraSetting.FrameRate * 300;// 600000;//最长超时时间
            long start_time = 0;
            long timestamp = 0;
            byte[] frame = null;
            bool usefullFlag = false;//数据是否还有用
                                     // === 诊断: AirProcessWork 启动 ===
            LogHelper.WriteLog($"[AIR-PROC] AirProcessWork 线程已启动, " +
                               $"AirDelay={AirDelay}ms, AirDuration={AirDuration}ms, " +
                               $"activatePixelsX={activatePixelsX}, activatePixelsY={activatePixelsY}, " +
                               $"AirCount={AirCount}, FrameRate={ass.SortCameraSetting.FrameRate}");
            long _airProcLoopCounter = 0;  // 循环计数器
            long _airProcDequeueCounter = 0;  // 出队计数器
            long _airProcEjectCounter = 0;  // 触发吹气计数器
            long _airProcExceptionCount = 0;
            // ==================================
            // ===== 诊断探针 B 的统计字段 =====
            long _probeBSample = 0;
            long _probeBSum = 0;
            long _probeBMax = 0;
            // =================================
            // ===== 诊断探针 C 的统计字段 =====
            long _probeCSample = 0;
            long _probeCSum = 0;
            long _probeCMax = 0;
            long _probeCEjectLagSum = 0;
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
                                // 事件驱动,由 EjectForm.SendEjectData 入队时 Set 唤醒。
                                // WaitOne(2) 给 2ms 兜底,防止 Set 信号丢失时死等。
                                AirQueueEvent.WaitOne(2);
                                continue;
                            }
                            AirQueue.TryDequeue(out var removeData);
                            frame = removeData.data;
                            timestamp = removeData.timestamp;
                            _airProcDequeueCounter++;

                            // === 诊断探针 B: 从"相机扫到(timestamp)" 到 "被 AirProcessWork 取出" 的耗时 ===
                            // 这段包含: 入 AirQueue 之前的所有处理 + AirQueue 排队等待
                            // 如果这段的值 稳定偏大 → 前面链路本身慢
                            // 如果这段的值 偶尔尖峰 → AirQueue 堆积了
                            long dequeueDelayMs = (DateTime.Now.Ticks - timestamp) / 10000;
                            _probeBSample++;
                            _probeBSum += dequeueDelayMs;
                            _probeBMax = Math.Max(_probeBMax, dequeueDelayMs);
                            if (_probeBSample >= 500)
                            {
                                double avg = (double)_probeBSum / _probeBSample;
                                LogHelper.WriteLog($"[PROBE-B-DEQUEUE] 最近500帧: 平均={avg:F1}ms, 最大={_probeBMax}ms, " +
                                                   $"AirQueue剩余={AirQueue.Count}");
                                _probeBSample = 0;
                                _probeBSum = 0;
                                _probeBMax = 0;
                            }
                            if (dequeueDelayMs > 30)
                            {
                                LogHelper.WriteLog($"[PROBE-B-DEQUEUE] !!! 尖峰 {dequeueDelayMs}ms !!! AirQueue剩余={AirQueue.Count}");
                            }
                            // ==========================================================================

                            // === 诊断: 每 100 次出队打印一次(保留原日志) ===
                            if ((_airProcDequeueCounter % 100) == 0)
                            {
                                LogHelper.WriteLog($"[AIR-PROC] 已出队 {_airProcDequeueCounter} 帧, AirQueue 当前剩余={AirQueue.Count}");
                            }
                            // ==================================

                            //LogHelper.WriteLine($"TOTAL-TIME-一帧数据从采样到吹气的总时间：" + (currentTimeLong - timestamp) / 10);


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
                            // === 诊断: 偶尔打印超时丢弃 ===
                            if ((_airProcDequeueCounter % 50) == 0)
                            {
                                long overdueMs = (currentTimeLong - timestamp - delay - overtime) / 10000;
                                LogHelper.WriteLog($"[AIR-PROC] 帧超时丢弃 #{_airProcDequeueCounter}, " +
                                                   $"超时 {overdueMs}ms (delay={AirDelay}ms, overtime={overtime / 10000}ms)");
                            }
                            // ============================
                            usefullFlag = false;
                            continue;
                        }
                        usefullFlag = false;
                    }

                    //LogHelper.WriteLine($"chui-取数据时间：" + (DateTime.Now.Ticks - start_time));


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

                            int maxIdx = Math.Min(frame.Length, preSampleState.Length) - 1;
                            eIndex = eIndex < maxIdx ? eIndex : maxIdx;
                            if (sIndex > eIndex) continue;

                            int count = 0;
                            for (int y = sIndex; y <= eIndex; y++)
                            {
                                if (y >= frame.Length) break;

                                bool isTarget;
                                if (EjectTargetClassIds != null && EjectTargetClassIds.Count > 0)
                                    isTarget = EjectTargetClassIds.Contains(frame[y]);
                                else
                                    isTarget = frame[y] > 0 && frame[y] < 254;

                                count = isTarget ? (count + 1) : 0;  // 这行不能少！

                                if (count >= activatePixelsY)
                                {
                                    airsState[i] = (byte)activatePixelsX;

                                    int clearStart = pixelAirMap[i][0];
                                    int clearLen = pixelAirMap[i].Count;
                                    if (clearStart < 0) clearStart = 0;
                                    if (clearStart + clearLen > temp.Length)
                                        clearLen = temp.Length - clearStart;
                                    if (clearLen > 0)
                                        Array.Clear(temp, clearStart, clearLen);

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

                                if (index < 0 || index >= frame.Length || index >= preSampleState.Length)
                                    continue;

                                // === 修复: 只把 真实业务类 (1~253) 当前景, 排除 0 和保留值 254/255 ===
                                // 改成：
                                // 改成：
                                if (EjectTargetClassIds != null && EjectTargetClassIds.Count > 0)
                                {
                                    if (!EjectTargetClassIds.Contains(frame[index]))
                                        continue;
                                }
                                else
                                {
                                    if (frame[index] == 0 || frame[index] >= 254)
                                        continue;
                                }
                                // ============================================================

                                var maxValue = Math.Max(TryGetValue(preSampleState, index - 1),
                                                         Math.Max(preSampleState[index],
                                                                  TryGetValue(preSampleState, index + 1)));
                                if (maxValue == activatePixelsX - 1)
                                {
                                    airsState[i] = (byte)activatePixelsX;

                                    // Array.Clear 越界保护 (之前已加)
                                    int clearStart = pixelAirMap[i][0];
                                    int clearLen = pixelAirMap[i].Count;
                                    if (clearStart < 0) clearStart = 0;
                                    if (clearStart + clearLen > temp.Length)
                                        clearLen = temp.Length - clearStart;
                                    if (clearLen > 0)
                                        Array.Clear(temp, clearStart, clearLen);

                                    break;
                                }
                                else
                                {
                                    if (index < temp.Length)
                                        temp[index] = (byte)(maxValue + 1);
                                }
                            }
                        }
                    }
                    Array.Copy(temp, preSampleState, temp.Length);


                    //LogHelper.WriteLine($"chui-判断吹气孔的时间：" + (DateTime.Now.Ticks - start_time));

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
                        _airProcEjectCounter++;
                        long triggerTicks = DateTime.Now.Ticks;
                        long realDelayMs = (triggerTicks - timestamp) / 10000;

                        // === 诊断探针 C: 完整端到端延迟 + 吹气函数耗时 ===
                        // realDelayMs = 相机扫到 → 即将调用 Eject() 的总延时
                        // AirDelay 大于 0 时,这个值应该 ≈ max(链路总耗时, AirDelay)
                        _probeCSample++;
                        _probeCSum += realDelayMs;
                        _probeCMax = Math.Max(_probeCMax, realDelayMs);
                        if (_probeCSample >= 50)
                        {
                            double avg = (double)_probeCSum / _probeCSample;
                            LogHelper.WriteLog($"[PROBE-C-END2END] 最近50次触发: 平均={avg:F1}ms, 最大={_probeCMax}ms, " +
                                               $"AirDelay设置={AirDelay}ms, AirQueue剩余={AirQueue.Count}");
                            _probeCSample = 0;
                            _probeCSum = 0;
                            _probeCMax = 0;
                        }
                        // 单次异常尖峰立即打
                        if (realDelayMs > AirDelay + 20)   // 超过期望值 20ms 算尖峰
                        {
                            LogHelper.WriteLog($"[PROBE-C-END2END] !!! 尖峰 realDelay={realDelayMs}ms " +
                                               $"(期望≈{AirDelay}ms) AirQueue剩余={AirQueue.Count}");
                        }
                        // ====================================================

                        // 保留原 AIR-EJECT 日志(每 50 次)
                        if ((_airProcEjectCounter % 50) == 1)
                        {
                            LogHelper.WriteLog($"[AIR-EJECT] 触发吹气 #{_airProcEjectCounter}, " +
                                               $"实际端到端延迟={realDelayMs}ms, " +
                                               $"AirQueue剩余={AirQueue.Count}, " +
                                               $"气管={string.Join(",", arrejt)}, " +
                                               $"duration={AirDuration}ms");
                        }

                        // 调用 Eject, 同时测量 Eject 本身耗时
                        long ejectCallStart = DateTime.Now.Ticks;
                        Eject(arrejt.ToArray(), (ushort)AirDuration);
                        long ejectCallMs = (DateTime.Now.Ticks - ejectCallStart) / 10000;

                        // === 诊断探针 D: Eject 函数本身的耗时(从调用到返回) ===
                        // 这是"代码内 Eject 调用"的时间,不包含硬件响应时间
                        // 通常应该 <=3ms,如果经常 >10ms 说明串口发送变慢
                        if (ejectCallMs > 5)
                        {
                            LogHelper.WriteLog($"[PROBE-D-EJECT-CALL] !!! Eject() 调用耗时 {ejectCallMs}ms " +
                                               $"(期望<=3ms), 气管数={arrejt.Count}");
                        }
                        // ====================================================
                    }

                }
                catch (Exception e)
                {
                    _airProcExceptionCount++;
                    if (_airProcExceptionCount <= 5)
                    {
                        // 头 5 次详细打印, 包含上下文
                        LogHelper.WriteLog($"[AIR-PROC] !!! 异常 #{_airProcExceptionCount} !!! {e.Message}\n" +
                                           $"  上下文: AirCount={AirCount}, " +
                                           $"frame.Length={frame?.Length}, " +
                                           $"preSampleState.Length={preSampleState?.Length}, " +
                                           $"airsState.Length={airsState?.Length}\n" +
                                           $"  StackTrace:\n{e.StackTrace}");
                    }
                    else if ((_airProcExceptionCount % 1000) == 0)
                    {
                        // 之后每 1000 次打一次, 避免日志爆炸
                        LogHelper.WriteLog($"[AIR-PROC] 累计异常 {_airProcExceptionCount} 次, 最新: {e.Message}");
                    }
                    Trace.WriteLine($"air err " + e.Message.ToString());
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
