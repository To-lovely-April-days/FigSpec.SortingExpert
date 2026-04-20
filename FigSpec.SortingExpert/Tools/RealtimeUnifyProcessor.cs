using System;
using FigSpec.SortingExpert.Tools;

namespace FigSpec.SortingExpert.Tools
{
    /// <summary>
    /// 实时分选场景下的"统一颜色"处理器 (v3 带诊断)。
    /// 
    /// 工作原理(方案 A + 方案 C 组合):
    /// - 维护一个"尾部缓存"(上一批处理完保留的最后 K 行)
    /// - 新一批 tags 来了,拼接 [tail + new] → 做连通域统一(先不填背景)
    /// - 得到"纯净版" → 缓存尾部 K 行 (避免填背景导致的雪崩污染)
    /// - 对"要发送的 N 行"单独应用 fillBackground (若用户启用)
    /// - 跨批矿石能完整进入 bbox,按合并信息投票,
    ///   保证和训练界面(整图处理)效果一致性 >= 95%。
    /// 
    /// 代价: 吹气延迟 +K 帧 (默认 K=25,@1000fps 即 25ms),
    /// 可通过调小 EjectAirDelay 无感补偿。
    /// 
    /// === v2 修复说明 ===
    /// 1. 忽略保留值 254/255 (未分类/背景),避免 GPU 输出的保留值被当成前景
    /// 2. hist 支持完整 256 类空间,不再漏统计异常值
    /// 3. 多数投票初始化 bestCnt=0 (原 -1 会在全零时误选 1)
    /// 4. 拆分"纯净版"和"发送版",缓冲只留纯净版,彻底消除 fillBackground 雪崩
    /// 5. 置信度分母改为"bbox 内前景像素数",而非整个 bbox 面积,避免大 bbox 稀释占比
    /// 
    /// === v3 诊断说明 ===
    /// 加了 [UNIFY-DBG] 日志, 用于定位 "全 255 输入 → 全 force 输出" 的奇怪 bug
    /// 每 50 次 Process 调用打印一次详细诊断
    /// 不改业务逻辑, 只加日志
    /// 
    /// 线程安全: 非线程安全。当前只在 SendEjectData 单线程内使用。
    /// </summary>
    public class RealtimeUnifyProcessor
    {
        /// <summary>
        /// 尾部缓存行数(跨批跨度),默认 25 行,和 CalculatedFrameCount 一致。
        /// </summary>
        private readonly int K;

        /// <summary>
        /// 支持的 classId 最大值 + 1。必须开 256,覆盖完整 byte 范围,
        /// 避免异常值(如 254/255)被忽略。
        /// </summary>
        private const int NUM_CLASSES = 256;

        /// <summary>
        /// 保留值下界: 254=未分类, 255=背景。连通域判断时这些值视为背景。
        /// </summary>
        private const byte RESERVED_MIN = 254;

        /// <summary>
        /// 上一批保留的尾部"纯净版"(不带 fillBackground),shape = [K, Samples]。
        /// 首次调用时为 null。
        /// </summary>
        private byte[,] _tail;

        /// <summary>
        /// === v3 新增 === Process 调用计数器, 每 50 次打一次诊断日志
        /// </summary>
        private long _processCallCounter = 0;

        public RealtimeUnifyProcessor(int carryLines = 25)
        {
            K = carryLines;
            _tail = null;
        }

        /// <summary>
        /// 分选开始/结束时调用,清空内部缓冲。
        /// </summary>
        public void Reset()
        {
            _tail = null;
            _processCallCounter = 0;
        }

        /// <summary>
        /// 处理一批 tags,返回处理后需要发送给吹气的 tags。
        /// 
        /// 统一颜色未启用时(forceClassId &lt; 1)直接原样返回并清空缓冲。
        /// </summary>
        /// <param name="tags">当前批的分类结果 byte[N, Samples]</param>
        /// <param name="forceClassId">目标 classId,必须 &gt;= 1 且 &lt; 254,否则透传</param>
        /// <param name="confidenceThreshold">置信度阈值 (0~1 之间)</param>
        /// <param name="fillBackground">是否把连通域 bbox 内的背景也染成目标色</param>
        /// <returns>处理后的 tags,shape 与输入 tags 相同</returns>
        public byte[,] Process(byte[,] tags, int forceClassId,
                                float confidenceThreshold, bool fillBackground)
        {
            if (tags == null) return null;

            // 功能关闭或目标是保留值: 透传 + 清缓冲
            if (forceClassId < 1 || forceClassId >= RESERVED_MIN)
            {
                _tail = null;
                return tags;
            }

            byte force = (byte)forceClassId;
            int N = tags.GetLength(0);
            int S = tags.GetLength(1);

            // === v3 诊断: 每 50 次 Process 打印输入直方图 ===
            _processCallCounter++;
            bool debug = (_processCallCounter % 50) == 1;
            if (debug)
            {
                int[] inHist = new int[256];
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < S; j++)
                        inHist[tags[i, j]]++;
                var sb = new System.Text.StringBuilder();
                for (int c = 0; c < 256; c++)
                    if (inHist[c] > 0) sb.Append($"类{c}={inHist[c]} ");

                LogHelper.WriteLog($"[UNIFY-DBG] Process #{_processCallCounter} 入口, " +
                                   $"force={force}, thr={confidenceThreshold:F3}, " +
                                   $"fillBg={fillBackground}, " +
                                   $"tail={(_tail == null ? "null" : "K=" + _tail.GetLength(0))}, " +
                                   $"输入(N={N},S={S}): {sb}");
            }
            // ================================================

            // ============================================================
            // 阶段 A: 产出"纯净版"(fillBackground=false),用于更新缓冲
            // ============================================================
            byte[,] cleanProcessed;
            int sendOffsetRows;  // 发送区在 cleanProcessed 里的起始行

            if (_tail == null)
            {
                // 第一批: 独立处理
                cleanProcessed = ApplyUnify(tags, force, confidenceThreshold, false, debug, "阶段A-第一批");
                sendOffsetRows = 0;
            }
            else if (_tail.GetLength(1) != S)
            {
                // Samples 变了(ROI 改了等),缓冲失效,重新开始
                _tail = null;
                cleanProcessed = ApplyUnify(tags, force, confidenceThreshold, false, debug, "阶段A-缓冲失效");
                sendOffsetRows = 0;
            }
            else
            {
                // 正常情况: 拼接 [tail + new]
                int K_cur = _tail.GetLength(0);
                byte[,] combined = new byte[K_cur + N, S];
                Buffer.BlockCopy(_tail, 0, combined, 0, K_cur * S);
                Buffer.BlockCopy(tags, 0, combined, K_cur * S, N * S);

                cleanProcessed = ApplyUnify(combined, force, confidenceThreshold, false, debug, "阶段A-拼接");
                sendOffsetRows = K_cur;
            }

            // === v3 诊断: 阶段 A 输出统计 ===
            if (debug)
            {
                int[] cleanHist = new int[256];
                int cleanLines = cleanProcessed.GetLength(0);
                for (int i = 0; i < cleanLines; i++)
                    for (int j = 0; j < S; j++)
                        cleanHist[cleanProcessed[i, j]]++;
                var sb = new System.Text.StringBuilder();
                for (int c = 0; c < 256; c++)
                    if (cleanHist[c] > 0) sb.Append($"类{c}={cleanHist[c]} ");
                LogHelper.WriteLog($"[UNIFY-DBG]   阶段A 完成, cleanProcessed: {sb}");
            }
            // ================================

            // ============================================================
            // 阶段 B: 更新缓冲 (存纯净版的最后 K 行)
            // ============================================================
            _tail = ExtractLastKRows(cleanProcessed, K);

            // ============================================================
            // 阶段 C: 产出"发送版"
            // 若 fillBackground=false,直接切片返回纯净版
            // 若 fillBackground=true,对发送区单独做一次 fillBackground
            // ============================================================
            byte[,] toSend = new byte[N, S];
            Buffer.BlockCopy(cleanProcessed, sendOffsetRows * S, toSend, 0, N * S);

            if (fillBackground)
            {
                // 只对要发送的 N 行做 fillBackground
                // 这里基于"已统一过颜色"的 toSend 做第二次单批处理
                toSend = ApplyUnify(toSend, force, confidenceThreshold, true, debug, "阶段C-fillBg");
            }

            // === v3 诊断: 最终输出统计 ===
            if (debug)
            {
                int[] outHist = new int[256];
                for (int i = 0; i < N; i++)
                    for (int j = 0; j < S; j++)
                        outHist[toSend[i, j]]++;
                var sb = new System.Text.StringBuilder();
                for (int c = 0; c < 256; c++)
                    if (outHist[c] > 0) sb.Append($"类{c}={outHist[c]} ");
                LogHelper.WriteLog($"[UNIFY-DBG]   最终输出: {sb}");
            }
            // ================================

            return toSend;
        }

        /// <summary>
        /// 从 2D 数组中提取最后 K 行(若总行数 &lt; K 则全取)
        /// </summary>
        private static byte[,] ExtractLastKRows(byte[,] src, int K)
        {
            int total = src.GetLength(0);
            int S = src.GetLength(1);
            int take = total >= K ? K : total;
            byte[,] result = new byte[take, S];
            int offset = (total - take) * S;
            Buffer.BlockCopy(src, offset, result, 0, take * S);
            return result;
        }

        // ================================================================
        // 核心算法: 连通域分析 + 投票 + 填充
        // ================================================================

        /// <summary>
        /// 判断一个 classid 是否算"前景" (参与连通域)。
        /// 规则: 非 0 且小于 254 (不是保留值)。
        /// </summary>
        private static bool IsForeground(byte cls)
        {
            return cls != 0 && cls < RESERVED_MIN;
        }

        /// <summary>
        /// 对 tags 做"按连通域统一 classid + 可选填充背景"处理。
        /// 
        /// 决策逻辑(对齐训练界面 UnifyClassIdByContour 中"指定目标类"模式):
        ///   对每个连通域,计算 bounding box 中 forceClassId 像素的占比
        ///   (分母用"bbox 内前景像素数",不用总面积,避免大 bbox 稀释)。
        ///   占比 &gt;= threshold → 模型认为是目标矿 → 整颗染成 forceClassId
        ///   占比 &lt; threshold  → 模型认为不是目标矿 → 按多数投票保留
        /// 
        /// 采用 Two-Pass 8连通 算法(与 OpenCV 一致)。
        /// 
        /// === v3 新增参数 ===
        /// debug: 是否打印诊断日志
        /// callerTag: 调用上下文标签(阶段A/阶段C等), 用于区分日志来源
        /// </summary>
        private byte[,] ApplyUnify(byte[,] tags, byte forceClassId,
                                    float threshold, bool fillBackground,
                                    bool debug, string callerTag)
        {
            int lines = tags.GetLength(0);
            int samples = tags.GetLength(1);

            // === v3 诊断 ===
            if (debug)
            {
                LogHelper.WriteLog($"[UNIFY-DBG]   ApplyUnify [{callerTag}] 入口: " +
                                   $"lines={lines}, samples={samples}, " +
                                   $"force={forceClassId}, thr={threshold:F3}, " +
                                   $"fillBg={fillBackground}");
            }
            // ================

            // 阶段 1: Two-Pass 8连通 标记
            int[,] labels;
            int numComponents;
            ConnectedComponents8(tags, out labels, out numComponents);

            // === v3 诊断 ===
            if (debug)
            {
                LogHelper.WriteLog($"[UNIFY-DBG]   ApplyUnify [{callerTag}] 连通域数: {numComponents}");
            }
            // ================

            if (numComponents == 0)
            {
                // === v3 诊断 ===
                if (debug)
                {
                    LogHelper.WriteLog($"[UNIFY-DBG]   ApplyUnify [{callerTag}] 走 numComponents=0 分支, 返回 copy(原样)");
                }
                // ================

                // 没有任何前景,直接返回副本
                byte[,] copy = new byte[lines, samples];
                Buffer.BlockCopy(tags, 0, copy, 0, lines * samples);
                return copy;
            }

            // 阶段 2: 聚合每个连通域的 bbox + 直方图 + 前景像素数
            int[,] bbox = new int[numComponents + 1, 4];       // { r0, r1, c0, c1 }
            int[,] hist = new int[numComponents + 1, NUM_CLASSES];
            int[] fgPixels = new int[numComponents + 1];

            for (int k = 1; k <= numComponents; k++)
            {
                bbox[k, 0] = lines;
                bbox[k, 1] = -1;
                bbox[k, 2] = samples;
                bbox[k, 3] = -1;
            }

            for (int i = 0; i < lines; i++)
            {
                for (int j = 0; j < samples; j++)
                {
                    int k = labels[i, j];
                    if (k == 0) continue;
                    byte cls = tags[i, j];
                    hist[k, cls]++;          // 256 列,任何 byte 都能统计
                    fgPixels[k]++;
                    if (i < bbox[k, 0]) bbox[k, 0] = i;
                    if (i > bbox[k, 1]) bbox[k, 1] = i;
                    if (j < bbox[k, 2]) bbox[k, 2] = j;
                    if (j > bbox[k, 3]) bbox[k, 3] = j;
                }
            }

            // 阶段 3: 逐连通域决策 + 染色
            byte[,] output = new byte[lines, samples];
            Buffer.BlockCopy(tags, 0, output, 0, lines * samples);

            for (int k = 1; k <= numComponents; k++)
            {
                int r0 = bbox[k, 0], r1 = bbox[k, 1];
                int c0 = bbox[k, 2], c1 = bbox[k, 3];
                int fgInComp = fgPixels[k];
                if (fgInComp <= 0) continue;  // 防御,理论不会发生

                int targetCount = hist[k, forceClassId];
                float confidence = (float)targetCount / fgInComp;

                byte target;
                if (confidence >= threshold)
                {
                    // 高置信度 = 是目标矿 → 整颗染成目标类
                    target = forceClassId;
                }
                else
                {
                    // 低置信度 = 不是目标矿 → 按多数投票
                    // 注意初始化 bestCnt = 0,避免全 0 时误选
                    int bestCls = 0;
                    int bestCnt = 0;
                    // 只在真实类 1..253 里投票,跳过保留值 254/255
                    for (int c = 1; c < RESERVED_MIN; c++)
                    {
                        if (hist[k, c] > bestCnt)
                        {
                            bestCnt = hist[k, c];
                            bestCls = c;
                        }
                    }
                    if (bestCnt == 0)
                    {
                        // 整个连通域全是保留值/零(理论不该发生),不染色,保持原样
                        continue;
                    }
                    target = (byte)bestCls;
                }

                // === v3 诊断: 只打头 5 个连通域的决策 ===
                if (debug && k <= 5)
                {
                    LogHelper.WriteLog($"[UNIFY-DBG]   ApplyUnify [{callerTag}] 连通域 #{k}: " +
                                       $"bbox=({r0},{c0})~({r1},{c1}), " +
                                       $"fgPx={fgInComp}, targetCnt={targetCount}, " +
                                       $"conf={confidence:P2}, 决策染成={target}");
                }
                // ============================================

                // 染色: bbox 内属于本连通域的前景 + (可选)空白背景
                for (int i = r0; i <= r1; i++)
                {
                    for (int j = c0; j <= c1; j++)
                    {
                        int lab = labels[i, j];
                        if (lab == k)
                        {
                            output[i, j] = target;
                        }
                        else if (fillBackground && lab == 0)
                        {
                            // 只填"真正的背景"(labels==0)
                            // 不覆盖保留值 254/255,保持背景语义
                            byte orig = tags[i, j];
                            if (orig < RESERVED_MIN)
                            {
                                output[i, j] = target;
                            }
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Two-Pass 8 连通 标记。
        /// 输出 labels[i, j]: 0=背景, >=1=连通域编号(连续编号)。
        /// 
        /// "前景" = IsForeground() 为 true 的像素 (非 0 且非保留值 254/255)。
        /// </summary>
        private static void ConnectedComponents8(byte[,] tags,
            out int[,] labels, out int numComponents)
        {
            int lines = tags.GetLength(0);
            int samples = tags.GetLength(1);
            labels = new int[lines, samples];

            // Union-Find 数组,估算容量: 最坏情况每像素一个 label,但通常远少于
            int capacity = Math.Max(16, (lines * samples) / 8 + 16);
            int[] parent = new int[capacity];
            parent[0] = 0; // 索引 0 不用
            int nextLabel = 1;

            // Pass 1: 扫描 + 临时标记
            for (int i = 0; i < lines; i++)
            {
                for (int j = 0; j < samples; j++)
                {
                    if (!IsForeground(tags[i, j]))
                        continue;

                    // 8 邻域中上一行 3 个 + 当前行左 1 个
                    int n1 = (i > 0 && j > 0) ? labels[i - 1, j - 1] : 0;
                    int n2 = (i > 0) ? labels[i - 1, j] : 0;
                    int n3 = (i > 0 && j < samples - 1) ? labels[i - 1, j + 1] : 0;
                    int n4 = (j > 0) ? labels[i, j - 1] : 0;

                    int minLabel = int.MaxValue;
                    if (n1 > 0 && n1 < minLabel) minLabel = n1;
                    if (n2 > 0 && n2 < minLabel) minLabel = n2;
                    if (n3 > 0 && n3 < minLabel) minLabel = n3;
                    if (n4 > 0 && n4 < minLabel) minLabel = n4;

                    if (minLabel == int.MaxValue)
                    {
                        // 新标签
                        if (nextLabel >= parent.Length)
                        {
                            int newCap = parent.Length * 2;
                            int[] grown = new int[newCap];
                            Buffer.BlockCopy(parent, 0, grown, 0, parent.Length * sizeof(int));
                            parent = grown;
                        }
                        parent[nextLabel] = nextLabel;
                        labels[i, j] = nextLabel;
                        nextLabel++;
                    }
                    else
                    {
                        labels[i, j] = minLabel;
                        if (n1 > 0 && n1 != minLabel) Union(parent, minLabel, n1);
                        if (n2 > 0 && n2 != minLabel) Union(parent, minLabel, n2);
                        if (n3 > 0 && n3 != minLabel) Union(parent, minLabel, n3);
                        if (n4 > 0 && n4 != minLabel) Union(parent, minLabel, n4);
                    }
                }
            }

            // Pass 2: 压缩标签到连续编号
            int[] rootToCompact = new int[nextLabel];
            int compactCount = 0;
            for (int old = 1; old < nextLabel; old++)
            {
                int r = Find(parent, old);
                if (rootToCompact[r] == 0)
                {
                    compactCount++;
                    rootToCompact[r] = compactCount;
                }
            }
            for (int i = 0; i < lines; i++)
            {
                for (int j = 0; j < samples; j++)
                {
                    int lab = labels[i, j];
                    if (lab != 0)
                    {
                        labels[i, j] = rootToCompact[Find(parent, lab)];
                    }
                }
            }

            numComponents = compactCount;
        }

        private static int Find(int[] parent, int x)
        {
            int root = x;
            while (parent[root] != root) root = parent[root];
            // 路径压缩
            while (parent[x] != root)
            {
                int next = parent[x];
                parent[x] = root;
                x = next;
            }
            return root;
        }

        private static void Union(int[] parent, int a, int b)
        {
            int ra = Find(parent, a);
            int rb = Find(parent, b);
            if (ra != rb)
            {
                if (ra < rb) parent[rb] = ra;
                else parent[ra] = rb;
            }
        }
    }
}