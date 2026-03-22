using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools.Client.XiWang
{
    public struct PuffData
    {
        public UInt32 X;
        public long timec;
        public PuffData(ushort x, long time)
        {
            this.X = x;
            this.timec = time;
        }
    }


    public class XiWang_ChuiQi
    {
        // 声明变量
        private Socket clientSocket;

        private byte[][] pByte;

        public bool IsConnected { get; private set; }

        public XiWang_ChuiQi()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            pByte = new byte[17][];

            for (int i = 0; i < pByte.Length; i++)
            {
                pByte[i] = new byte[17]; // 初始化每个子数组为19个字节  
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    IsConnected = false;
                    //Console.WriteLine("断开服务器成功");
                }
                catch
                {
                    //Console.WriteLine("断开服务器失败");
                }
            }
        }
        // 定义委托
        public delegate void ConnectStatusChangedHandler(bool isConnected);
        public event ConnectStatusChangedHandler ConnectStatusChanged;

        /// <summary>
        /// 触发连接状态变化事件的方法
        /// </summary>
        /// <param name="isConnected"></param>
        protected virtual void OnConnectStatusChanged(bool isConnected)
        {
            ConnectStatusChanged?.Invoke(isConnected);
        }
        /// <summary>
        /// 开始连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public async Task<bool> Connect(string ip, int port)
        {
            try
            {
                int attempts = 0;
                while (attempts < 3 && !IsConnected)
                {
                    try
                    {
                        int timeoutMilliseconds = 10000; // 连接超时时间（毫秒）
                        Task connectTask = clientSocket.ConnectAsync(ip, port);
                        // 等待连接或超时
                        Task completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutMilliseconds));
                        // 检查连接是否完成
                        if (completedTask == connectTask)
                        {
                            // 连接已完成
                            if (clientSocket.Connected)
                            {
                                IsConnected = true;
                                //master = ModbusIpMaster.CreateIp(tcpClient);
                                Console.WriteLine("连接服务器成功");
                            }
                            else
                            {
                                Console.WriteLine("连接失败：无法连接到目标地址或端口。");
                            }
                        }
                        else
                        {
                            // 连接超时
                            //clientSocket.Close();
                            Console.WriteLine("连接超时：无法连接到目标地址或端口。");
                        }

                    }
                    catch
                    {
                        attempts++;
                        Thread.Sleep(200); // 等待2秒
                    }
                }
                if (!IsConnected)
                {
                    Console.WriteLine("连接服务器失败，请检查网络或服务器状态。");
                }
                OnConnectStatusChanged(IsConnected);
                if (!IsConnected)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private ConcurrentQueue<PuffData> puffQueue = new ConcurrentQueue<PuffData>();
        /// <summary>
        /// 发送吹气的服务端
        /// </summary>
        /// <summary>
        /// 喷吹处理
        /// </summary>
        /// <param name="state"></param>

        public bool Ejact(PuffData d)
        {
            byte[] tempByteArr = new byte[4];
            //tempByteArr[0] = BitConverter.GetBytes(d.X)[1];
            //tempByteArr[1] = BitConverter.GetBytes(d.X)[0];
            //tempByteArr[2] = BitConverter.GetBytes(d.Y)[1];
            //tempByteArr[3] = BitConverter.GetBytes(d.Y)[0];
            tempByteArr[2] = BitConverter.GetBytes(d.X)[3];
            tempByteArr[3] = BitConverter.GetBytes(d.X)[2];
            tempByteArr[0] = BitConverter.GetBytes(d.X)[1];
            tempByteArr[1] = BitConverter.GetBytes(d.X)[0];

            try
            {
                Send(GetWriteCommand("1", tempByteArr, 1, 16, GetCheckHead(16)));
                Thread.Sleep((int)d.timec);
                tempByteArr[0] = 0;
                tempByteArr[1] = 0;
                tempByteArr[2] = 0;
                tempByteArr[3] = 0;
                Send(GetWriteCommand("1", tempByteArr, 1, 16, GetCheckHead(16)));
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送失败");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 获取随机校验头
        /// </summary>
        /// <returns></returns>
        private byte[] GetCheckHead(int seed)
        {
            var random = new Random(DateTime.Now.Millisecond + seed);
            return new byte[] { (byte)random.Next(255), (byte)random.Next(255) };
        }
        /// <summary>
        /// 获取写入命令
        /// </summary>
        /// <param name="address">寄存器地址</param>
        /// <param name="values">批量读取的值</param>
        /// <param name="stationNumber">站号</param>
        /// <param name="functionCode">功能码</param>
        /// <returns></returns>
        private byte[] GetWriteCommand(string address, byte[] values, byte stationNumber, byte functionCode, byte[] check = null)
        {
            var writeAddress = ushort.Parse(address?.Trim());
            //if (plcAddresses) writeAddress = (ushort)(Convert.ToUInt16(address?.Trim().Substring(1)) - 1);

            byte[] buffer = new byte[13 + values.Length];
            buffer[0] = check?[0] ?? 0x19;
            buffer[1] = check?[1] ?? 0xB2;//事务处理标识符                
            buffer[4] = BitConverter.GetBytes(7 + values.Length)[1];
            buffer[5] = BitConverter.GetBytes(7 + values.Length)[0];//表示的是header handle后面还有多长的字节

            buffer[6] = stationNumber; //站号
            buffer[7] = functionCode;  //功能码:16  写多个保持寄存器 寄存器PLC地址:40001 - 49999 位操作/字操作:字操作 操作数量:多个
            buffer[8] = BitConverter.GetBytes(writeAddress)[1];
            buffer[9] = BitConverter.GetBytes(writeAddress)[0];//寄存器地址
            buffer[10] = (byte)(values.Length / 2 / 256);
            buffer[11] = (byte)(values.Length / 2 % 256);//写寄存器数量(除2是两个字节一个寄存器，寄存器16位。除以256是byte最大存储255。)              
            buffer[12] = (byte)(values.Length);          //写字节的个数
            values.CopyTo(buffer, 13);                   //把目标值附加到数组后面
            return buffer;
        }
        /// <summary>
        /// Send方法测试：即发送消息，以字节为单位
        /// </summary>
        /// <param name="bytes"></param>
        private void Send(byte[] bytes)
        {
            try
            {
                Console.WriteLine(BitConverter.ToString(bytes));
                clientSocket.Send(bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                //IsConnected = false;
                //clientSocket.Close();
                Console.WriteLine("发送失败，服务端断开连接@@@@");
            }
        }
    }
}
