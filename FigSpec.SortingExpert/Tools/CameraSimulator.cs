using Cameras;
using FigSpec.SortingExpert.ModelFiles;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace FigSpec.SortingExpert.Tools
{
    /// <summary>
    /// 相机数据模拟器 - 以指定帧率回放预录的帧数据
    /// 用于诊断实时分选链路性能(不依赖真实相机)
    /// </summary>
    public class CameraSimulator
    {
        private Thread _worker;
        private volatile bool _running;

        /// <summary>回放速率: 帧/秒</summary>
        public int TargetFps { get; set; } = 1805;

        /// <summary>物料密度: 0=全空载, 1=100%密集</summary>
        public double MaterialDensity { get; set; } = 0.3;

        /// <summary>数据源: 单帧样本(真实相机的一帧)</summary>
        private byte[] _emptyFrame;    // 无物料时的帧
        private byte[] _materialFrame; // 有物料时的帧

        private int _samples;
        private int _bands;

        /// <summary>数据发送回调 - 挂到这里来接收数据</summary>
        public event Action<GrabData> GrabedData;

        public CameraSimulator(int samples, int bands)
        {
            _samples = samples;
            _bands = bands;

            // 生成一个"空帧" - 都是暗电流+背景的低值
            int frameSize = samples * bands * 2;
            _emptyFrame = new byte[frameSize];
            Random r = new Random(42);
            for (int i = 0; i < frameSize; i++)
            {
                _emptyFrame[i] = (byte)r.Next(20, 60);  // 低反射率背景
            }

            // 生成一个"物料帧" - 反射率较高
            _materialFrame = new byte[frameSize];
            for (int i = 0; i < frameSize; i++)
            {
                // 在中间 1/3 区域制造一个高反射率物料
                int pixelIdx = i / (bands * 2);
                if (pixelIdx > samples / 3 && pixelIdx < samples * 2 / 3)
                {
                    _materialFrame[i] = (byte)r.Next(150, 220); // 高反射率
                }
                else
                {
                    _materialFrame[i] = (byte)r.Next(20, 60);
                }
            }
        }

        public void Start()
        {
            _running = true;
            _worker = new Thread(Run) { IsBackground = true, Name = "CameraSim" };
            _worker.Start();
        }

        public void Stop()
        {
            _running = false;
            _worker?.Join(1000);
        }

        private void Run()
        {
            // 用 Stopwatch 做精确计时(DateTime.Now 分辨率不够)
            var sw = Stopwatch.StartNew();
            long framesSent = 0;
            long ticksPerFrame = Stopwatch.Frequency / TargetFps;
            Random r = new Random();

            // 物料"流"模拟: 每隔一段时间集中出现一批物料(模拟颗粒的簇)
            int materialBurstRemaining = 0;

            while (_running)
            {
                long targetTicks = framesSent * ticksPerFrame;
                long waitTicks = targetTicks - sw.ElapsedTicks;

                // 精确等待下一帧时刻
                if (waitTicks > 100000)  // 10ms 以上用 Sleep
                {
                    Thread.Sleep((int)(waitTicks * 1000 / Stopwatch.Frequency - 1));
                }
                else
                {
                    while (sw.ElapsedTicks < targetTicks) { /* spin */ }
                }

                // 决定这一帧有没有物料
                bool hasMaterial;
                if (materialBurstRemaining > 0)
                {
                    hasMaterial = true;
                    materialBurstRemaining--;
                }
                else if (r.NextDouble() < MaterialDensity * 0.05)
                {
                    // 触发一个 10~30 帧的物料"簇"
                    materialBurstRemaining = r.Next(10, 30);
                    hasMaterial = true;
                }
                else
                {
                    hasMaterial = false;
                }

                // 每帧都 new 一个新的 GrabData(避免影响下游队列的引用持有)
                byte[] values = hasMaterial
                    ? (byte[])_materialFrame.Clone()
                    : (byte[])_emptyFrame.Clone();

                GrabedData?.Invoke(new GrabData { Values = values });
                framesSent++;
            }
        }
    }
}