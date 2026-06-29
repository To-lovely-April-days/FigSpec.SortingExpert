using CHNSpec.Tools;
using FigSpec.SortingExpert.ModelFiles;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FigSpec.SortingExpert.Tools
{
    /// <summary>
    /// UDP 参数下发器(协议 v1.0)。
    /// 
    /// 角色:本程序作为发送方(协议文档中的"配置工具"角色),
    ///       每 1 秒把当前 10 个参数打成 SET 包发往分选程序(FastSorting)。
    /// 
    /// 协议要点:
    ///   - 包头 0xAA 0xBB,版本 0x01,小端字节序
    ///   - SET 命令字 0x01,SET_ACK 0x81
    ///   - 每参数 6 字节(ID 1 + Type 1 + Value 4)
    ///   - 校验和 = 除自身外所有字节 XOR
    /// 
    /// 真正落地的参数:
    ///   0x01 g_totalDelay         ← XApplySetting.EjectAirDelay
    ///   0x02 g_duration           ← XApplySetting.EjectAirTime
    ///   0x06 g_unifyEnable        ← model.UnifyColorEnabled
    ///   0x08 g_unifyForceClassId  ← model.UnifyTargetClassId
    ///   0x09 g_unifyThreshold     ← model.UnifyConfidenceThreshold / 100f
    ///   0x0A g_unifyFillBackground ← model.UnifyFillBackground
    /// 
    /// 空操作参数(发默认值,不真改任何东西):
    ///   0x03 g_valveThresholdRatio       → 0
    ///   0x04 g_frameActivateThreshold    → 0
    ///   0x05 g_centerValveInflate        → 0
    ///   0x07 g_unifyTailFrames           → 24
    /// 
    /// 失败处理:发送后等 ACK,超时或 status!=0 重试,最多 3 次。
    /// </summary>
    public class UdpParamSender : IDisposable
    {
        // ========== 协议常量 ==========
        private const byte HEADER1 = 0xAA;
        private const byte HEADER2 = 0xBB;
        private const byte VERSION = 0x01;
        private const byte CMD_SET = 0x01;
        private const byte CMD_SET_ACK = 0x81;

        // 参数 ID
        private const byte ID_TOTAL_DELAY = 0x01;
        private const byte ID_DURATION = 0x02;
        private const byte ID_VALVE_THRESHOLD_RATIO = 0x03;
        private const byte ID_FRAME_ACTIVATE_THRESHOLD = 0x04;
        private const byte ID_CENTER_VALVE_INFLATE = 0x05;
        private const byte ID_UNIFY_ENABLE = 0x06;
        private const byte ID_UNIFY_TAIL_FRAMES = 0x07;
        private const byte ID_UNIFY_FORCE_CLASS_ID = 0x08;
        private const byte ID_UNIFY_THRESHOLD = 0x09;
        private const byte ID_UNIFY_FILL_BACKGROUND = 0x0A;

        // 数据类型
        private const byte TYPE_INT32 = 0;
        private const byte TYPE_FLOAT = 1;
        private const byte TYPE_BOOL = 2;

        // 空操作参数的固定值
        private const int DEFAULT_VALVE_THRESHOLD_RATIO = 0;
        private const int DEFAULT_FRAME_ACTIVATE_THRESHOLD = 0;
        private const int DEFAULT_CENTER_VALVE_INFLATE = 0;
        private const int DEFAULT_UNIFY_TAIL_FRAMES = 24;

        // ========== 网络配置 ==========
        private const string TARGET_IP = "127.0.0.1";
        private const int TARGET_PORT = 8080;
        private const int POLL_INTERVAL_MS = 1000;
        private const int ACK_TIMEOUT_MS = 1000;
        private const int MAX_RETRY = 3;

        // ========== 单例 ==========
        private static UdpParamSender _shared;
        private static readonly object _sharedLock = new object();
        public static UdpParamSender Shared
        {
            get
            {
                if (_shared == null)
                {
                    lock (_sharedLock)
                    {
                        if (_shared == null)
                            _shared = new UdpParamSender();
                    }
                }
                return _shared;
            }
        }

        // ========== 运行时状态 ==========
        private Timer _timer;
        private UdpClient _udpClient;
        private IPEndPoint _targetEndpoint;
        private volatile bool _running = false;
        private long _sendCounter = 0;
        private long _ackOkCounter = 0;
        private long _ackFailCounter = 0;
        private readonly object _sendLock = new object();   // 防止定时器重入

        private UdpParamSender() { }

        /// <summary>
        /// 启动定时下发。重复调用安全。
        /// </summary>
        public void Start()
        {
            if (_running) return;
            try
            {
                _udpClient = new UdpClient();
                _udpClient.Client.ReceiveTimeout = ACK_TIMEOUT_MS;
                _targetEndpoint = new IPEndPoint(IPAddress.Parse(TARGET_IP), TARGET_PORT);
                _running = true;
                _timer = new Timer(OnTick, null, 0, POLL_INTERVAL_MS);
                LogHelper.WriteLog($"[UDP-SEND] 启动定时下发, 目标 {TARGET_IP}:{TARGET_PORT}, 间隔 {POLL_INTERVAL_MS}ms");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"[UDP-SEND] 启动失败: {ex.Message}");
                _running = false;
            }
        }

        /// <summary>
        /// 停止定时下发。重复调用安全。
        /// </summary>
        public void Stop()
        {
            if (!_running) return;
            _running = false;
            try { _timer?.Dispose(); } catch { }
            try { _udpClient?.Close(); } catch { }
            _timer = null;
            _udpClient = null;
            LogHelper.WriteLog($"[UDP-SEND] 已停止, 累计发送 {_sendCounter}, ACK 成功 {_ackOkCounter}, 失败 {_ackFailCounter}");
        }

        public void Dispose() => Stop();

        // ========== 定时回调 ==========

        private void OnTick(object state)
        {
            if (!_running) return;
            // 单线程串行,避免上一轮还没收完 ACK 下一轮就上来
            if (!Monitor.TryEnter(_sendLock)) return;
            try
            {
                SendOnce();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"[UDP-SEND] OnTick 异常: {ex.Message}");
            }
            finally
            {
                Monitor.Exit(_sendLock);
            }
        }

        /// <summary>
        /// 取一次当前参数快照、打包、发送、等 ACK,失败重试。
        /// </summary>
        private void SendOnce()
        {
            byte[] packet;
            try
            {
                packet = BuildSetPacket();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"[UDP-SEND] 打包失败: {ex.Message}");
                return;
            }

            for (int attempt = 1; attempt <= MAX_RETRY; attempt++)
            {
                try
                {
                    _udpClient.Send(packet, packet.Length, _targetEndpoint);
                    _sendCounter++;

                    // 等 ACK
                    var ackResult = WaitAck();
                    if (ackResult.success)
                    {
                        _ackOkCounter++;
                        // 每 60 轮(约 1 分钟)打一次心跳日志
                        if ((_ackOkCounter % 60) == 1)
                        {
                            LogHelper.WriteLog($"[UDP-SEND] OK 心跳: 累计发送 {_sendCounter}, ACK 成功 {_ackOkCounter}, 失败 {_ackFailCounter}");
                        }
                        return;
                    }

                    LogHelper.WriteLog($"[UDP-SEND] 第 {attempt}/{MAX_RETRY} 次失败: {ackResult.reason}");
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog($"[UDP-SEND] 第 {attempt}/{MAX_RETRY} 次异常: {ex.Message}");
                }
            }

            _ackFailCounter++;
        }

        /// <summary>
        /// 阻塞等 ACK,最多 ACK_TIMEOUT_MS 毫秒。
        /// 返回 (是否成功, 失败原因)。
        /// </summary>
        private (bool success, string reason) WaitAck()
        {
            try
            {
                IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpClient.Receive(ref remote);

                // 最小 ACK 包 = 9 字节 (head2 + ver1 + cmd1 + len2 + status1 + errId1 + xor1)
                if (data == null || data.Length < 9)
                    return (false, $"ACK 太短 len={data?.Length ?? 0}");
                if (data[0] != HEADER1 || data[1] != HEADER2)
                    return (false, "ACK 包头错误");
                if (data[3] != CMD_SET_ACK)
                    return (false, $"ACK cmd 错误 0x{data[3]:X2}");

                // 校验 XOR
                byte expectedXor = 0;
                for (int i = 0; i < data.Length - 1; i++) expectedXor ^= data[i];
                if (expectedXor != data[data.Length - 1])
                    return (false, "ACK 校验和错误");

                byte status = data[6];
                byte errId = data[7];
                if (status == 0) return (true, null);
                return (false, $"status={status}, errParam=0x{errId:X2}");
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                return (false, "ACK 超时");
            }
            catch (Exception ex)
            {
                return (false, $"接收异常: {ex.Message}");
            }
        }

        // ========== 打包 ==========

        /// <summary>
        /// 取当前参数快照,打成完整 SET 包。
        /// </summary>
        private byte[] BuildSetPacket()
        {
            // 取 10 个参数的当前值
            var ass = GlobalSettings.ApplySetting;
            Model model = null;
            try
            {
                model = ass?.traingSet?.models?.FindLast(m => m.UnifyColorEnabled)
                     ?? ass?.traingSet?.models?.FirstOrDefault();
            }
            catch { model = null; }

            int totalDelay = (int)(ass?.EjectAirDelay ?? 0);
            int duration = (int)(ass?.EjectAirTime ?? 0);
            bool unifyEnable = model?.UnifyColorEnabled ?? false;
            int unifyClassId = model?.UnifyTargetClassId ?? 1;
            // model 里是 0~100 的 int,协议要 0.0~1.0 的 float
            float unifyThreshold = (model?.UnifyConfidenceThreshold ?? 50) / 100f;
            bool unifyFill = model?.UnifyFillBackground ?? false;

            // 协议范围钳位(避免对端回 OUT_OF_RANGE)
            totalDelay = Clamp(totalDelay, 0, 65535);
            duration = Clamp(duration, 0, 65535);
            unifyClassId = Clamp(unifyClassId < 1 ? 1 : unifyClassId, 1, 12);
            if (unifyThreshold < 0f) unifyThreshold = 0f;
            if (unifyThreshold > 1f) unifyThreshold = 1f;

            // 构造 10 个参数(顺序固定 0x01 → 0x0A)
            const int paramCount = 10;
            const int paramSize = 6;
            int payloadLen = 1 + paramCount * paramSize;  // 1 字节 ParamCount + N×6
            byte[] pkt = new byte[6 + payloadLen + 1];

            // 包头
            pkt[0] = HEADER1;
            pkt[1] = HEADER2;
            pkt[2] = VERSION;
            pkt[3] = CMD_SET;
            pkt[4] = (byte)(payloadLen & 0xFF);
            pkt[5] = (byte)((payloadLen >> 8) & 0xFF);
            pkt[6] = paramCount;

            int idx = 7;
            // 0x01 g_totalDelay (int)
            WriteParam(pkt, ref idx, ID_TOTAL_DELAY, TYPE_INT32, IntBytes(totalDelay));
            // 0x02 g_duration (int)
            WriteParam(pkt, ref idx, ID_DURATION, TYPE_INT32, IntBytes(duration));
            // 0x03 g_valveThresholdRatio (int) 空操作 = 0
            WriteParam(pkt, ref idx, ID_VALVE_THRESHOLD_RATIO, TYPE_INT32, IntBytes(DEFAULT_VALVE_THRESHOLD_RATIO));
            // 0x04 g_frameActivateThreshold (int) 空操作 = 0
            WriteParam(pkt, ref idx, ID_FRAME_ACTIVATE_THRESHOLD, TYPE_INT32, IntBytes(DEFAULT_FRAME_ACTIVATE_THRESHOLD));
            // 0x05 g_centerValveInflate (int) 空操作 = 0
            WriteParam(pkt, ref idx, ID_CENTER_VALVE_INFLATE, TYPE_INT32, IntBytes(DEFAULT_CENTER_VALVE_INFLATE));
            // 0x06 g_unifyEnable (bool)
            WriteParam(pkt, ref idx, ID_UNIFY_ENABLE, TYPE_BOOL, BoolBytes(unifyEnable));
            // 0x07 g_unifyTailFrames (int) 空操作 = 24
            WriteParam(pkt, ref idx, ID_UNIFY_TAIL_FRAMES, TYPE_INT32, IntBytes(DEFAULT_UNIFY_TAIL_FRAMES));
            // 0x08 g_unifyForceClassId (int)
            WriteParam(pkt, ref idx, ID_UNIFY_FORCE_CLASS_ID, TYPE_INT32, IntBytes(unifyClassId));
            // 0x09 g_unifyThreshold (float)
            WriteParam(pkt, ref idx, ID_UNIFY_THRESHOLD, TYPE_FLOAT, FloatBytes(unifyThreshold));
            // 0x0A g_unifyFillBackground (bool)
            WriteParam(pkt, ref idx, ID_UNIFY_FILL_BACKGROUND, TYPE_BOOL, BoolBytes(unifyFill));

            // XOR 校验
            byte xor = 0;
            for (int i = 0; i < idx; i++) xor ^= pkt[i];
            pkt[idx] = xor;

            return pkt;
        }

        // ========== 字节工具 ==========

        private static void WriteParam(byte[] pkt, ref int idx, byte id, byte type, byte[] value4)
        {
            pkt[idx++] = id;
            pkt[idx++] = type;
            pkt[idx++] = value4[0];
            pkt[idx++] = value4[1];
            pkt[idx++] = value4[2];
            pkt[idx++] = value4[3];
        }

        private static byte[] IntBytes(int v)
        {
            // BitConverter 在 x86/x64 默认就是 little-endian,但显式写更安全
            byte[] b = new byte[4];
            b[0] = (byte)(v & 0xFF);
            b[1] = (byte)((v >> 8) & 0xFF);
            b[2] = (byte)((v >> 16) & 0xFF);
            b[3] = (byte)((v >> 24) & 0xFF);
            return b;
        }

        private static byte[] FloatBytes(float v)
        {
            // IEEE 754 little-endian
            byte[] b = BitConverter.GetBytes(v);
            if (!BitConverter.IsLittleEndian) Array.Reverse(b);
            return b;
        }

        private static byte[] BoolBytes(bool v)
        {
            // 协议:4 字节,只用第 1 字节,0/1
            byte[] b = new byte[4];
            b[0] = v ? (byte)1 : (byte)0;
            return b;
        }

        private static int Clamp(int v, int min, int max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }
    }
}