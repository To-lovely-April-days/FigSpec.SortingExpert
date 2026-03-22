using SerialPortLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools.Client
{
    public class JiuShunEjectDevice : IBaseEjectDevice
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
        public bool OpenByCom(string COM, int AirBaudRate )
        {
            if (SPICOM.IsConnected)
                return true;
            try
            {
                SPICOM.SetPort(COM, 921600);
                bool isSiuccess = SPICOM.Connect();
                if (!isSiuccess)
                {
                    Trace.WriteLine("初始化气吹控制失败");
                    return false;
                }
                //Set(new int[] { 0, 1, 2, 3 }, new int[] { 120 });
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
                OpenAir(ports, time);
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
        //private void OpenAir(int startindex, int endindex, ushort duration, ushort delay = 1)
        //{
        //    long start_time = DateTime.Now.Ticks;
        //    var duration_b1 = (byte)(duration >> 8);
        //    var duration_b2 = (byte)(duration & 0xff);
        //    var index_b1 = (byte)startindex;
        //    var index_b2 = (byte)endindex;
        //    var delay_b1 = (byte)(delay >> 8);
        //    var delay_b2 = (byte)(delay & 0xff);
        //    byte[] message = new byte[] { 0xff, index_b1, index_b2, delay_b1, delay_b2, duration_b1, duration_b2 };
        //    bool isSuccess = SPICOM.SendMessage(message);
        //    if (!isSuccess)
        //    {
        //        LogHelper.WriteLog("初始化气吹控制失败");
        //    }
        //}
        /// <summary>
        /// 设置延时和吹气时长
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Set(int[] devices, int[] data)
        {
            foreach (var item in devices)
            {
                Set(item, data);
            }
            return true;
        }
        private bool Set(int device, int[] data)
        {
            var item = device;
            var t1 = (byte)(data[0] >> 8);
            var t2 = (byte)(data[0] & 0xff);

            byte[] message = new byte[] { 0xA9, 0x57, (byte)item, 0x02, 0x00, 0x01, (byte)t1, (byte)t2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            ushort sum = 0;
            sum = (ushort)message.Sum(x => x);
            var b1 = (byte)(sum >> 8);
            var b2 = (byte)(sum & 0xff);
            message[12] = b1;
            message[13] = b2;
            message[14] = 0xff;
            message[15] = 0xff;
            bool isSuccess = SPICOM.SendMessage(message);
            if (!isSuccess)
            {
                LogHelper.WriteLog("初始化气吹控制失败1 " + item);
            }
            //message = new byte[] { 0xA9, 0x57, (byte)item, 0x02, 0x00, 0x02, 0x00, (byte)data[1], 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //sum = (ushort)message.Sum(x => x);
            //b1 = (byte)(sum >> 8);
            //b2 = (byte)(sum & 0xff);
            //message[12] = b1;
            //message[13] = b2;
            //message[14] = 0xff;
            //message[15] = 0xff;
            //isSuccess = SPICOM.SendMessage(message);
            //if (!isSuccess)
            //{
            //    LogHelper.WriteLog("初始化气吹控制失败2 " + item);
            //}
            return true;
        }

        private bool Tick(int device, uint highdata, uint lowdata)
        {
            byte[] message = new byte[] { 0xA9, 0x57, (byte)device, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            int n = 4;
            message[n++] = (byte)(highdata >> 24);
            message[n++] = (byte)(highdata >> 16);
            message[n++] = (byte)(highdata >> 8);
            message[n++] = (byte)(highdata );

            message[n++] = (byte)(lowdata >> 24);
            message[n++] = (byte)(lowdata >> 16);
            message[n++] = (byte)(lowdata >> 8);
            message[n++] = (byte)(lowdata);

            ushort sum = 0;
            sum = (ushort)message.Sum(x => x);
            var b1 = (byte)(sum >> 8);
            var b2 = (byte)(sum & 0xff);
            message[12] = b1;
            message[13] = b2;
            message[14] = 0xff;
            message[15] = 0xff;
            bool isSuccess = SPICOM.SendMessage(message);
            if (!isSuccess)
            {
                LogHelper.WriteLog("初始化气吹控制失败1 ");
            }
            return true;
        }
        /// <summary>
        /// 吹气
        /// </summary>
        //private void OpenAir(int[] ports)
        //{
        //    int[] ejectdata = new int[4 * 2];
        //    foreach (var item in ports)
        //    {
        //        int x = item / 32;
        //        int y = item % 32;
        //        ejectdata[x] |= 0x01 << y;
        //    }
        //    for (int i = 0; i < 4; i++)
        //    {
        //        Tick(i, (uint)ejectdata[i * 2 + 1], (uint)ejectdata[i * 2]);
        //    }
        //}
        /// <summary>
        /// 吹气
        /// </summary>
        private void OpenAir(int[] ports, ushort delay)
        {
            int[] ejectdata = new int[4 * 2];
            foreach (var item in ports)
            {
                int x = item / 32;
                int y = item % 32;
                ejectdata[x] |= 0x01 << (y);
            }
            for (int i = 0; i < 4; i++)
            {
                Tick(i+1, (uint)ejectdata[i * 2 + 1], (uint)ejectdata[i * 2]);
            }
            Thread.Sleep((int)delay);
            for (int i = 0; i < 4; i++)
            {
                Tick(i + 1, (uint)0, (uint)0);
            }
        }
    }
}
