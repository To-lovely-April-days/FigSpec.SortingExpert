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
    public class ServerControl
    {
        private Socket serverSocket;
        //private TcpListener _listener;
        //private IPEndPoint _remoteEndPoint;
        //private Thread _receiveThread;
        private Action<byte[]> _onReceiveDataHandler;
        // 自定义有参构造方法（IP地址，流程传输方式，TCP协议）
        public ServerControl(string port, Action<byte[]> onReceiveDataHandler)
        {
            while (true)
            {
                try
                {
                    serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    serverSocket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(port)));
                    serverSocket.Listen(10);
                    _onReceiveDataHandler = onReceiveDataHandler;
                    Console.WriteLine("服务器启动成功");
                    break; // 如果成功启动，则跳出循环
                }
                catch (Exception ex)
                {
                    Console.WriteLine("服务器启动失败：" + ex.Message);
                    Thread.Sleep(2000); // 等待2秒后再次尝试启动
                }
            }
            
            //serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //// 服务器启动
            //serverSocket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(port)));
            //serverSocket.Listen(10);
            //_onReceiveDataHandler = onReceiveDataHandler;
            //Console.WriteLine("服务器启动成功");
        }

        /// <summary>
        /// 创建启动方法（IPEndPoint用于指定地址及端口初始化，需using System.Net;）
        /// </summary>
        public void Start()
        {
            // 开启线程：目的实现服务器和客户端一对多连接
            Thread threadAccept = new Thread(Accept);
            threadAccept.IsBackground = true;
            threadAccept.Start();
        }

        /// <summary>
        /// 关闭服务端
        /// </summary>
        public void Close()
        {
            serverSocket.Close();
        }

        /// <summary>
        /// Accept方法测试：接收客户端连接
        /// </summary>
        private void Accept()
        {

            // 接收客户端方法，会挂起当前线程（.RemoteEndPoint表示远程地址）
            Socket client = serverSocket.Accept();
            IPEndPoint point = client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine(point.Address + "[" + point.Port + "] 连接成功！");

            // 开启一个新线程线程，实现消息多次接收

            ThreadPool.QueueUserWorkItem(new WaitCallback(Receive), client);


            //Thread threadReceive = new Thread(Receive);
            //threadReceive.IsBackground = true;
            //threadReceive.Start(client);

            // 尾递归
            Accept();
        }

        /// <summary>
        /// Receive方法的使用测试
        /// 接收客户端发送过来的消息，以字节为单位进行操作
        /// 该方法会阻塞当前线程，所以适合开启新的线程使用该方法
        /// Accept()中将Receive作为线程传递对象，所以要注意一点，使用线程传递对象只能是object类型的！！
        /// </summary>
        /// <param name="obj"></param>
        private void Receive(object obj)
        {
            try
            {
                // 尝试发送或接收数据的操作
                // 将object类型强行转换成socket
                Socket client = obj as Socket;
                byte[] msg = new byte[700];
                while (true)
                {
                    int msgLen = client.Receive(msg);
                    if (msgLen <= 0)
                    {
                        if (client.Poll(10, SelectMode.SelectRead))
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                            break;
                        }
                        Thread.Sleep(100);
                        // break;
                    }
                    else
                    {
                        _onReceiveDataHandler?.Invoke(msg);
                    }
                    // 将msg装换成字符串
                    Array.Clear(msg, 0, msg.Length);
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
            {
                // 远程主机强迫关闭了一个现有连接
                Console.WriteLine("连接已被远程主机关闭");
                // 可以在这里进行重新连接等处理
            }
            catch (Exception ex)
            {
                // 其他异常
                Console.WriteLine("发生异常：" + ex.Message);
            }
          
        }
    }
}
