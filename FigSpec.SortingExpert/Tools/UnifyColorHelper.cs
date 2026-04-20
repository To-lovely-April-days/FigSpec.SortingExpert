using FigSpec.SortingExpert.ModelFiles;
using OpenCvSharp;
using System;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    /// <summary>
    /// "统一颜色"核心算法。训练界面预览和实时分选链路共用同一份实现，
    /// 保证两边行为完全一致。
    ///
    /// 数据源：一张逐像素 classid 图（tags），0 表示背景，>0 表示各个类别。
    /// 做法：
    ///   1. 以 tags > 0 作为前景做 8 邻接连通域（一颗矿石 = 一个连通域）。
    ///   2. 如果开启"填充背景",先用 floodFill 填掉矿石内部孔洞,把整颗矿石(含空洞)
    ///      纳入同一个连通域。
    ///   3. 对每个连通域按 model.UnifyTargetClassId / UnifyConfidenceThreshold /
    ///      UnifyFillBackground 做决策,统一该连通域的 classid。
    ///
    /// 约定：
    ///   - model.UnifyColorEnabled == false → 方法直接返回,不做任何事。
    ///   - model.UnifyTargetClassId == -1 → 自动模式(多数投票)。
    ///   - model.UnifyTargetClassId >= 0 → 强制指定类别,配合置信度阈值使用。
    ///   - 所有方法都可以在多线程中被调用,因为只对入参 tags 做原地修改,不持有共享状态。
    /// </summary>
    public static class UnifyColorHelper
    {
        /// <summary>
        /// 对一批逐像素 classid 图应用 unify。直接在 tags 上原地修改。
        ///
        /// tags 的约定:tags[row, col] 是某个像素的 classid,0 表示背景。
        /// 在训练侧,行=spe.Lines,列=spe.Samples;
        /// 在实时分选侧,行=dealCount(一批帧数),列=Samples。
        /// 两侧语义一致。
        /// </summary>
        /// <param name="tags">二维 classid 图,会被原地修改</param>
        /// <param name="model">模型,读取其中的 4 个 Unify* 字段</param>
        public static void Apply(byte[,] tags, Model model)
        {
            if (model == null) return;
            if (!model.UnifyColorEnabled) return;
            if (tags == null) return;

            int rows = tags.GetLength(0);
            int cols = tags.GetLength(1);
            if (rows == 0 || cols == 0) return;

            bool fillBackground = model.UnifyFillBackground;
            int targetClassId = model.UnifyTargetClassId;         // -1 = 自动
            float confThreshold = model.UnifyConfidenceThreshold / 100f;

            try
            {
                ApplyCore(tags, rows, cols, targetClassId, confThreshold, fillBackground);
            }
            catch (Exception ex)
            {
                // unify 失败不应该拖垮分选主流程,catch 住并记录
                Console.WriteLine("[UnifyColorHelper] Apply 异常: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// 便捷重载:用于训练预览,接受 jagged 数组 pridictionImage。
        /// 内部转成矩形 byte[,] 跑 Apply,再拷回。
        ///
        /// 约定(TrainForm 里的惯例):
        ///   - pridictionImage.Length == spe.Lines (外层索引 = 行号/line)
        ///   - pridictionImage[x].Length == spe.Samples (内层索引 = 样本号/sample)
        ///   - 所以 tags[r, c] 直接对应 pridictionImage[r][c]
        /// </summary>
        public static void ApplyToJagged(byte[][] pridictionImage, int lines, int samples, Model model)
        {
            if (model == null || !model.UnifyColorEnabled) return;
            if (pridictionImage == null || pridictionImage.Length == 0) return;
            if (lines <= 0 || samples <= 0) return;

            int rows = lines;
            int cols = samples;
            byte[,] tags = new byte[rows, cols];

            int rmax = Math.Min(pridictionImage.Length, rows);
            for (int r = 0; r < rmax; r++)
            {
                var row = pridictionImage[r];
                if (row == null) continue;
                int cmax = Math.Min(row.Length, cols);
                for (int c = 0; c < cmax; c++)
                {
                    tags[r, c] = row[c];
                }
            }

            Apply(tags, model);

            // 写回
            for (int r = 0; r < rmax; r++)
            {
                var row = pridictionImage[r];
                if (row == null) continue;
                int cmax = Math.Min(row.Length, cols);
                for (int c = 0; c < cmax; c++)
                {
                    row[c] = tags[r, c];
                }
            }
        }

        // ===================== 内部实现 =====================

        private static void ApplyCore(byte[,] tags, int rows, int cols,
                                      int targetClassId, float confThreshold,
                                      bool fillBackground)
        {
            // --- Step 1: 构建二值图,前景=tags>0 ---
            // 直接复用 tags 内存做掩码生成,避免 OpenCvSharp 的 Mat.Get/Set 逐像素调用开销
            using (Mat binary = BuildBinary(tags, rows, cols))
            {
                if (Cv2.CountNonZero(binary) == 0) return;

                // --- Step 2: 如果要填背景,先把内部孔洞填上 ---
                Mat forCC = binary;
                Mat filled = null;
                if (fillBackground)
                {
                    filled = FillInternalHoles(binary);
                    forCC = filled;
                }

                try
                {
                    // --- Step 3: 连通域标记 ---
                    using (var labelsMat = new Mat())
                    using (var statsMat = new Mat())
                    using (var centroidsMat = new Mat())
                    {
                        int numLabels = Cv2.ConnectedComponentsWithStats(
                            forCC, labelsMat, statsMat, centroidsMat,
                            PixelConnectivity.Connectivity8, MatType.CV_32S);

                        if (numLabels <= 1) return; // 只有背景 label

                        // 把 labels 转成 int[rows,cols] 方便 C# 侧快速访问
                        int[,] labels = new int[rows, cols];
                        MatToInt2D(labelsMat, labels, rows, cols);

                        // 把 stats 读到 int[]
                        // stats 每行 5 列: [LEFT, TOP, WIDTH, HEIGHT, AREA]
                        // 用硬编码索引,避免不同版本 OpenCvSharp 枚举命名差异
                        const int STAT_LEFT = 0;
                        const int STAT_TOP = 1;
                        const int STAT_WIDTH = 2;
                        const int STAT_HEIGHT = 3;
                        const int STAT_AREA = 4;

                        int[] areas = new int[numLabels];
                        int[] lefts = new int[numLabels];
                        int[] tops = new int[numLabels];
                        int[] widths = new int[numLabels];
                        int[] heights = new int[numLabels];
                        for (int i = 0; i < numLabels; i++)
                        {
                            lefts[i] = statsMat.At<int>(i, STAT_LEFT);
                            tops[i] = statsMat.At<int>(i, STAT_TOP);
                            widths[i] = statsMat.At<int>(i, STAT_WIDTH);
                            heights[i] = statsMat.At<int>(i, STAT_HEIGHT);
                            areas[i] = statsMat.At<int>(i, STAT_AREA);
                        }

                        // --- Step 4: 对每个连通域做决策和改写 ---
                        bool forceMode = targetClassId >= 0;
                        byte forceCid = forceMode ? (byte)targetClassId : (byte)0;

                        Parallel.For(1, numLabels, (int lbl) =>
                        {
                            ProcessLabel(tags, labels, rows, cols,
                                         lbl, areas[lbl],
                                         lefts[lbl], tops[lbl], widths[lbl], heights[lbl],
                                         forceMode, forceCid, confThreshold, fillBackground);
                        });
                    }
                }
                finally
                {
                    if (filled != null) filled.Dispose();
                }
            }
        }

        /// <summary>
        /// 处理单个连通域:统计投票、决策、改写 tags。
        /// 只在该连通域的外接矩形内扫描,避免遍历整图。
        /// </summary>
        private static void ProcessLabel(
            byte[,] tags, int[,] labels, int rows, int cols,
            int lbl, int area,
            int left, int top, int width, int height,
            bool forceMode, byte forceCid, float confThreshold, bool fillBackground)
        {
            if (area <= 0 || width <= 0 || height <= 0) return;

            int rEnd = Math.Min(rows, top + height);
            int cEnd = Math.Min(cols, left + width);
            int rStart = Math.Max(0, top);
            int cStart = Math.Max(0, left);

            // 投票:直方图(classid 是 byte,范围 0~255)
            int[] histogram = new int[256];
            int totalInsideLabel = 0;

            for (int r = rStart; r < rEnd; r++)
            {
                for (int c = cStart; c < cEnd; c++)
                {
                    if (labels[r, c] != lbl) continue;
                    totalInsideLabel++;
                    byte cid = tags[r, c];
                    if (cid == 0) continue;
                    histogram[cid]++;
                }
            }

            if (totalInsideLabel == 0) return;

            // 找 top 类和类别数量
            int topCid = 0;
            int topVotes = 0;
            int numClasses = 0;
            for (int i = 1; i < 256; i++) // 跳过 0(背景)
            {
                if (histogram[i] == 0) continue;
                numClasses++;
                if (histogram[i] > topVotes)
                {
                    topVotes = histogram[i];
                    topCid = i;
                }
            }

            if (numClasses == 0) return; // 该连通域里全是 0(只会出现在 fillBackground 填出的纯孔洞区域)

            // 置信度 = top 类像素 / 连通域像素(严格为 area,即 CC 给出的面积)
            // 注:fillBackground 时 area 含填出来的孔洞,分母更大,置信度偏低 → 更容易触发"不确信"
            // 这和训练界面的原逻辑语义一致
            float confidence = (float)topVotes / area;

            byte winnerClassId;
            bool shouldOverride;

            if (forceMode)
            {
                if (confidence >= confThreshold)
                {
                    // 模型确信 → 保留原判
                    if (numClasses >= 2)
                    {
                        winnerClassId = (byte)topCid;
                        shouldOverride = false;
                    }
                    else if (numClasses == 1 && fillBackground)
                    {
                        winnerClassId = (byte)topCid;
                        shouldOverride = false;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    // 模型不确信 → 按用户指定染
                    winnerClassId = forceCid;
                    shouldOverride = true;
                }
            }
            else
            {
                // 自动模式
                if (numClasses >= 2)
                {
                    winnerClassId = (byte)topCid;
                    shouldOverride = false;
                }
                else if (numClasses == 1 && fillBackground)
                {
                    winnerClassId = (byte)topCid;
                    shouldOverride = false;
                }
                else
                {
                    return;
                }
            }

            bool needFullFill = shouldOverride || fillBackground;

            // --- 改写像素 ---
            if (needFullFill)
            {
                // 整个连通域(含 fill 填出的孔洞)都染
                for (int r = rStart; r < rEnd; r++)
                {
                    for (int c = cStart; c < cEnd; c++)
                    {
                        if (labels[r, c] != lbl) continue;
                        if (tags[r, c] != winnerClassId)
                        {
                            tags[r, c] = winnerClassId;
                        }
                    }
                }
            }
            else
            {
                // 只改已有非 0 像素,保留背景
                for (int r = rStart; r < rEnd; r++)
                {
                    for (int c = cStart; c < cEnd; c++)
                    {
                        if (labels[r, c] != lbl) continue;
                        if (tags[r, c] == 0) continue;
                        if (tags[r, c] != winnerClassId)
                        {
                            tags[r, c] = winnerClassId;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 把 byte[,] 掩码(0/非 0)转成 Mat(CV_8UC1,0/255)。
        /// 用 unsafe 或 Marshal.Copy 可以更快,这里用 Mat 构造 + SetTo 保守写法。
        /// </summary>
        private static Mat BuildBinary(byte[,] tags, int rows, int cols)
        {
            // 先构造一个和 tags 同形的 Mat,内容先拷过去
            Mat src = new Mat(rows, cols, MatType.CV_8UC1);
            // OpenCvSharp 允许从托管数组创建 Mat
            // 但 byte[,] 不能直接给,需要先打平
            byte[] flat = new byte[rows * cols];
            Buffer.BlockCopy(tags, 0, flat, 0, flat.Length);
            System.Runtime.InteropServices.Marshal.Copy(flat, 0, src.Data, flat.Length);

            // threshold > 0 即为前景
            Mat binary = new Mat();
            Cv2.Threshold(src, binary, 0, 255, ThresholdTypes.Binary);
            src.Dispose();
            return binary;
        }

        /// <summary>
        /// 填补二值图里被前景包围的内部孔洞,保留原前景。
        /// 手法:反色 → 从 (0,0) floodFill 把外部背景灌成 0 → 反色结果和原图 OR。
        /// </summary>
        private static Mat FillInternalHoles(Mat binary)
        {
            // 反色:原来的前景(255) → 0,原来的背景(0) → 255
            Mat inv = new Mat();
            Cv2.BitwiseNot(binary, inv);

            // 从 (0,0) 灌水把外部背景灌成 0
            // floodFill 需要一个 mask,尺寸比 src 大 2
            Mat mask = new Mat(binary.Rows + 2, binary.Cols + 2, MatType.CV_8UC1, Scalar.All(0));
            Cv2.FloodFill(inv, mask, new OpenCvSharp.Point(0, 0), new Scalar(0));
            mask.Dispose();

            // 现在 inv 里只剩"原来被前景包围的孔洞"是 255
            Mat filled = new Mat();
            Cv2.BitwiseOr(binary, inv, filled);
            inv.Dispose();
            return filled;
        }

        /// <summary>
        /// 把 Mat(CV_32S)拷进 int[rows,cols]。
        /// 用 unsafe 可以更快,但这里保守用 At&lt;int&gt; 循环 + 一次性 Marshal.Copy 混合。
        /// </summary>
        private static void MatToInt2D(Mat labelsMat, int[,] labels, int rows, int cols)
        {
            // labelsMat 是连续的 CV_32S。一次性 Marshal.Copy 到 int[rows*cols],再 Buffer.BlockCopy
            if (!labelsMat.IsContinuous())
            {
                // 不连续就用 clone 一份连续的
                using (var cont = labelsMat.Clone())
                {
                    MatToInt2D(cont, labels, rows, cols);
                    return;
                }
            }

            int total = rows * cols;
            int[] flat = new int[total];
            System.Runtime.InteropServices.Marshal.Copy(labelsMat.Data, flat, 0, total);
            Buffer.BlockCopy(flat, 0, labels, 0, total * sizeof(int));
        }
    }
}