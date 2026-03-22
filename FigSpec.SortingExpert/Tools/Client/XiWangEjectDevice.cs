using FigSpec.SortingExpert.Tools.Client.XiWang;
using Globalization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools.Client
{
    public class XiWangEjectDevice : IBaseEjectDevice
    {
        XiWang_ChuiQi xiWang_ChuiQi = new XiWang_ChuiQi();

        public bool IsConnected => xiWang_ChuiQi.IsConnected;

        public bool Close()
        {
            if (xiWang_ChuiQi.IsConnected)
            {
                try
                {
                    xiWang_ChuiQi.Disconnect();
                }
                catch (Exception)
                {

                }
            }
            return true;
        }

        public bool Eject(int[] ports, ushort time)
        {
            int dada = 0;
            foreach (var item in ports)
            {
                dada |= 0x01 << item;
            }
            PuffData puffData = new PuffData()
            {
                X = (uint)dada,
                timec = time
            };
            bool isSuccess = xiWang_ChuiQi.Ejact(puffData);

            if (!isSuccess)
            {
                LogHelper.WriteLog("初始化气吹控制失败".ToMultiLanguage());
            }
            return isSuccess;
        }

        public bool OpenByCom(string COM, int AirBaudRate = 115200)
        {
            return false;
        }

        public bool OpenByTcp(string IP, int port)
        {
            if (xiWang_ChuiQi.IsConnected)
                return true;
            try
            {
                bool isSiuccess = true;
                //"192.168.21.21", 502
                var task = xiWang_ChuiQi.Connect(IP, port);
                if (!task.Result)
                {
                    isSiuccess = false;
                    FormShowHelper.ShowMessage("吹气设备连接失败".ToMultiLanguage(), "提示".ToMultiLanguage());
                }
                //bool isSiuccess = xIWANG_CHUIQI.Connect();
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
            }
            return false;
        }
        public bool Set(int[] devices,int[] data)
        {
            return true;
        }


    }
}
