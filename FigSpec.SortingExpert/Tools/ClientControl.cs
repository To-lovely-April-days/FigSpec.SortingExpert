using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public class ClientControl
    {
        // 声明变量
        private Socket clientSocket;
        public bool isConnect;
        // 定义委托
        public delegate void ConnectStatusChangedHandler(bool isConnected);
        public event ConnectStatusChangedHandler ConnectStatusChanged;
        // 自定义有参构造方法（（IP地址，流程传输方式，TCP协议））
        public ClientControl()
        {
            
        }
        /// <summary>
        /// 获得本机ip地址
        /// </summary>
        /// <returns></returns>
        public static string Getip()
        {
            IPAddress local_ip = null;
            try
            {
                IPAddress[] iPs;
                iPs = Dns.GetHostAddresses(Dns.GetHostName());
                local_ip = iPs.First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                return local_ip.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void Close()
        {
            if (isConnect)
            {
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    //clientSocket.Disconnect(true);
                    clientSocket.Close();
                    //clientSocket.Dispose();
                    isConnect = false;
                    Console.WriteLine("断开服务器成功");
                }
                catch
                {
                    Console.WriteLine("断开服务器失败");
                }
            }
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
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                int attempts = 0;
                while (attempts < 3 && !isConnect)
                {
                    try
                    {
                        int timeoutMilliseconds = 1000; // 连接超时时间（毫秒）
                        Task connectTask = clientSocket.ConnectAsync(ip, port);
                        // 等待连接或超时
                        Task completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutMilliseconds));
                        // 检查连接是否完成
                        if (completedTask == connectTask)
                        {
                            // 连接已完成
                            if (clientSocket.Connected)
                            {
                                isConnect = true;
                                Console.WriteLine("连接服务器成功");
                                // 客户端接收服务器消息的线程
                                Thread threadReceive = new Thread(Receive);
                                threadReceive.IsBackground = true;
                                threadReceive.Start();
                            }
                            else
                            {
                                Console.WriteLine("连接失败：无法连接到目标地址或端口。");
                            }
                        }
                        else
                        {
                            // 连接超时
                            Console.WriteLine("连接超时：无法连接到目标地址或端口。");
                        }

                    }
                    catch(Exception ex)
                    {
                        attempts++;
                        Thread.Sleep(200); // 等待2秒
                    }
                }
                if (!isConnect)
                {
                    Console.WriteLine("连接服务器失败，请检查网络或服务器状态。");
                    OnConnectStatusChanged(isConnect);
                }
                if (!isConnect)
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
        public bool ChangeIP(string ip, int port)
        {
            try
            {
                try
                {
                    if (clientSocket != null)
                    {
                        // 创建一个新的IPAddress对象
                        IPAddress ipAddress = IPAddress.Parse(ip);

                        // 创建一个新的EndPoint对象
                        IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

                        try
                        {
                            // 先关闭当前连接
                            //clientSocket.Close();
                            // 绑定新的本地IP地址
                            clientSocket.Bind(endPoint);
                            Console.WriteLine("已切换至新的IP地址：" + ip);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("切换IP地址时出现异常：" + ex.Message);
                        }
                    }
                    //int timeoutMilliseconds = 1000; // 连接超时时间（毫秒）
                    //Task connectTask = clientSocket.ConnectAsync(ip, port);
                    //// 等待连接或超时
                    //Task completedTask = await Task.WhenAny(connectTask, Task.Delay(timeoutMilliseconds));
                    //// 检查连接是否完成
                    //if (completedTask == connectTask)
                    //{
                    //    // 连接已完成
                    //    if (clientSocket.Connected)
                    //    {
                    //        isConnect = true;
                    //        Console.WriteLine("连接服务器成功");
                    //        // 客户端接收服务器消息的线程
                    //        Thread threadReceive = new Thread(Receive);
                    //        threadReceive.IsBackground = true;
                    //        threadReceive.Start();
                    //    }
                    //    else
                    //    {
                    //        Console.WriteLine("连接失败：无法连接到目标地址或端口。");
                    //    }
                    //}
                    //else
                    //{
                    //    // 连接超时
                    //    clientSocket.Close();
                    //    Console.WriteLine("连接超时：无法连接到目标地址或端口。");
                    //}

                }
                catch
                {

                }
                //if (!isConnect)
                //{
                //    Console.WriteLine("连接服务器失败，请检查网络或服务器状态。");
                //    OnConnectStatusChanged(isConnect);
                //}
                //if (!isConnect)
                //{
                //    return false;
                //}
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }


        /// <summary>
        /// 发送设置信息给吹气程序
        /// </summary>
        public void SendSetting(int frameRate, int Samples, Dictionary<int, int> labelinfo)
        {
            //传输数据给服务器
            //数据内容
            byte[] buf = new byte[300];
            buf[0] = 0xee;//表头
            buf[1] = 0x01;//指令
            buf[2] = 0x01;//指令1
            buf[3] = 0x00;//指令2
            BitConverter.GetBytes(frameRate).CopyTo(buf, 4);//数据的总长度
            BitConverter.GetBytes(Samples).CopyTo(buf, 8);//数据的总长度
            BitConverter.GetBytes(labelinfo.Count).CopyTo(buf, 12);//数据的总长度
            int i = 0;
            foreach (var item in labelinfo)
            {
                BitConverter.GetBytes(item.Key).CopyTo(buf, 16 + i++ * 4);//数据的总长度
                BitConverter.GetBytes(item.Value).CopyTo(buf, 16 + i++ * 4);//数据的总长度
            }
            try
            {
                Send(buf);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 用于测试服务器向客户端返回一条消息
        /// </summary>
        private void Receive()
        {
            while (true)
            {
                try
                {
                    // 用于接收服务器的回复信息
                    byte[] msg = new byte[1024];
                    int msgLen = clientSocket.Receive(msg);
                    Console.WriteLine("服务器：" + Encoding.UTF8.GetString(msg, 0, msgLen));
                }
                // 异常处理方法
                catch
                {
                    Console.WriteLine("服务器积极拒绝！！");
                    // 退出while循环
                    break;
                }
            }
        }

        /// <summary>
        /// Send方法测试：即发送消息，以字节为单位
        /// </summary>
        /// <param name="bytes"></param>
        public void Send(byte[] bytes)
        {
            // 将字符创传化为字节数组
            try
            {
                clientSocket.Send(bytes);
            }
            catch (Exception)
            {
                Close();
                Console.WriteLine("发送失败，服务端断开连接");
                // 触发连接状态变化事件
                OnConnectStatusChanged(isConnect);
            }
        }

        /// <summary>
        /// 触发连接状态变化事件的方法
        /// </summary>
        /// <param name="isConnected"></param>
        protected virtual void OnConnectStatusChanged(bool isConnected)
        {
            ConnectStatusChanged?.Invoke(isConnected);
        }
    }
}
