using SerialPortLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools.Client
{
    public class ChspecEjectDevice : IBaseEjectDevice
    {
        private SerialPortInput SPICOM = new SerialPortInput();

        public bool IsConnected
        {
            get
            {
                return SPICOM.IsConnected;
            }
        }

        public bool OpenByTcp(string IP, int port)
        {
            return false;
        }
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="COM"></param>
        /// <returns></returns>
        public bool OpenByCom(string COM, int AirBaudRate = 115200)
        {
            if (SPICOM.IsConnected)
                return true;
            try
            {
                SPICOM.SetPort(COM, AirBaudRate);
                bool isSiuccess = SPICOM.Connect();
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
        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            if (SPICOM.IsConnected)
            {
                try
                {
                    SPICOM.Disconnect();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return true;
        }
        /// <summary>
        /// 气吹控制
        /// </summary>
        /// <param name="ports"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool Eject(int[] ports, ushort time)
        {
            try
            {
               
                if (ports.Length == 0)
                {
                   
                    return true;
                }
                int count = ports.Length;
                ports = ports.OrderBy(x => x).ToArray();
                int perfindex = 0;
                int endindex = 0;
                int i = 0;
                while (true)
                {
                    perfindex = ports[i];
                    endindex = ports[i];
                    for (int p = i + 1; p <= count; p++)
                    {
                        if (p != count && ports[p] == endindex + 1)
                        {
                            endindex = ports[p];
                        }
                        else
                        {
                            i = p;
                            OpenAir(perfindex, endindex, time);
                            break;
                        }
                    }
                    if (i == count)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return true;
        }
        /// <summary>
        /// 打开指定气吹 
        /// </summary>
        /// <param name="index">气吹编号 起始需要和截至序号</param>
        /// <param name="duration">吹气时长(ms)</param>
        /// <param name="delay">延时(ms)</param>
        private void OpenAir(int startindex, int endindex, ushort duration, ushort delay = 1)
        {
            var duration_b1 = (byte)(duration >> 8);
            var duration_b2 = (byte)(duration & 0xff);
            var index_b1 = (byte)startindex;
            var index_b2 = (byte)endindex;
            var delay_b1 = (byte)(delay >> 8);
            var delay_b2 = (byte)(delay & 0xff);
            byte[] message = new byte[] { 0xff, index_b1, index_b2, delay_b1, delay_b2, duration_b1, duration_b2 };

            // 【新增探针 H】精确测量每次 SendMessage 耗时
            long sendStart = DateTime.Now.Ticks;
            bool isSuccess = SPICOM.SendMessage(message);
            long sendMs = (DateTime.Now.Ticks - sendStart) / 10000;

            // 只在超过 2ms 时打印(正常应该 <1ms)
            if (sendMs > 2)
            {
                LogHelper.WriteLog($"[PROBE-H-SERIAL] !!! SendMessage 耗时 {sendMs}ms " +
                                   $"气管 {startindex}~{endindex}, 连接状态={SPICOM.IsConnected}");
            }

            if (!isSuccess)
            {
                LogHelper.WriteLog($"[CHSPEC-EJECT] !!! 发送失败 !!! 气管 {startindex}~{endindex}, " +
                                   $"duration={duration}ms, SPICOM.IsConnected={SPICOM.IsConnected}");
            }
        }
        public bool Set(int[] devices,int[] data)
        {
            return true;
        }
    }
}
