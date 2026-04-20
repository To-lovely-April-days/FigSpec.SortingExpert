using CHNSpec.Tools;
using FigSpec.SortingExpert.ModelFiles;
using System;
using System.Drawing;

namespace FigSpec.SortingExpert.Tools
{
    /// <summary>
    /// 实时分选界面显示用 helper。
    /// 输入: GPU 返回的 tags (byte[lines, samples]) + 模型
    /// 输出: 可直接喂给 displayRgbQueue / FramesToBitmap 的 BGR 字节数组
    ///       格式: byte[lines, samples * 3], 内存顺序 B G R B G R ...
    /// 
    /// 处理流程:
    ///   1. 按 model.UnifyColorEnabled 决定是否统一颜色
    ///      (使用独立的 UnifyProcessor,缓冲和吹气链路分开,互不干扰)
    ///   2. 查 model.classes 的 color 字段,把 classid 映射成 BGR
    ///   3. 未知 classid 或 0 → 黑色背景
    /// 
    /// 线程安全: 非线程安全,当前只在 ProcessPointSortingV1_2 单线程内使用。
    /// </summary>
    public class SortingDisplayHelper
    {
        private readonly RealtimeUnifyProcessor _unifyProcessor;
        private long _logCounter = 0;
        private const int LOG_EVERY_N_BATCHES = 40;

        // classid → Color.ToArgb() 的缓存,避免每帧查 model.classes 列表
        // key=classid, value=(B, G, R)
        private byte[] _colorB = new byte[256];
        private byte[] _colorG = new byte[256];
        private byte[] _colorR = new byte[256];
        private bool _colorTableBuilt = false;
        private Model _lastModel = null;

        public SortingDisplayHelper(int carryLines = 25)
        {
            _unifyProcessor = new RealtimeUnifyProcessor(carryLines);
        }

        /// <summary>
        /// 分选开始/结束时清缓冲。
        /// </summary>
        public void Reset()
        {
            _unifyProcessor.Reset();
            _logCounter = 0;
            _colorTableBuilt = false;
            _lastModel = null;
        }

        /// <summary>
        /// 把 tags 渲染成 BGR 字节数组,可直接塞给 displayDataQueue。
        /// </summary>
        /// <param name="tags">shape = [lines, samples]</param>
        /// <param name="model">模型,含 classes 颜色表和统一颜色配置</param>
        /// <returns>shape = [lines, samples * 3], BGR 排列</returns>
        public byte[,] Render(byte[,] tags, Model model)
        {
            if (tags == null || model == null) return null;

            int lines = tags.GetLength(0);
            int samples = tags.GetLength(1);

            // 构建颜色查表(每次模型变了都要重建)
            if (!_colorTableBuilt || !ReferenceEquals(_lastModel, model))
            {
                BuildColorTable(model);
                _lastModel = model;
                _colorTableBuilt = true;
                LogHelper.WriteLog("[UNIFY-DISP] 颜色表已构建");
            }

            // 第 1 步: 统一颜色(若启用)
            byte[,] processedTags;
            bool shouldLog = (_logCounter % LOG_EVERY_N_BATCHES) == 0;

            if (model.UnifyColorEnabled &&
                model.UnifyTargetClassId >= 1 &&
                model.UnifyTargetClassId < 254)
            {
                float threshold = model.UnifyConfidenceThreshold / 100f;
                if (threshold < 0f) threshold = 0f;
                if (threshold > 1f) threshold = 1f;

                long unifyStart = DateTime.Now.Ticks;
                processedTags = _unifyProcessor.Process(
                    tags,
                    model.UnifyTargetClassId,
                    threshold,
                    model.UnifyFillBackground);
                long unifyCost = (DateTime.Now.Ticks - unifyStart) / 10000;

                if (shouldLog)
                {
                    LogHelper.WriteLog(
                        $"[UNIFY-DISP] 批#{_logCounter} 统一颜色 目标={model.UnifyTargetClassId} " +
                        $"阈值={model.UnifyConfidenceThreshold}% " +
                        $"填充={model.UnifyFillBackground} 耗时={unifyCost}ms");
                }
            }
            else
            {
                processedTags = tags;
                if (shouldLog)
                {
                    LogHelper.WriteLog($"[UNIFY-DISP] 批#{_logCounter} 统一颜色未启用,直接渲染");
                }
            }

            // 第 2 步: tags → BGR
            byte[,] bgr = new byte[lines, samples * 3];
            int histC0 = 0, histOthers = 0;
            // 只在 shouldLog 时才统计,避免额外开销
            if (shouldLog)
            {
                for (int i = 0; i < lines; i++)
                {
                    for (int j = 0; j < samples; j++)
                    {
                        byte cls = processedTags[i, j];
                        int offset = j * 3;
                        bgr[i, offset] = _colorB[cls];
                        bgr[i, offset + 1] = _colorG[cls];
                        bgr[i, offset + 2] = _colorR[cls];

                        if (cls == 0) histC0++;
                        else histOthers++;
                    }
                }
                LogHelper.WriteLog(
                    $"[UNIFY-DISP] 批#{_logCounter} 渲染完成 背景={histC0} 前景={histOthers}");
            }
            else
            {
                // 快速路径,不统计
                for (int i = 0; i < lines; i++)
                {
                    for (int j = 0; j < samples; j++)
                    {
                        byte cls = processedTags[i, j];
                        int offset = j * 3;
                        bgr[i, offset] = _colorB[cls];
                        bgr[i, offset + 1] = _colorG[cls];
                        bgr[i, offset + 2] = _colorR[cls];
                    }
                }
            }

            _logCounter++;
            return bgr;
        }

        /// <summary>
        /// 根据 model.classes 构建 classid → BGR 的查表。
        /// 没在列表里的 classid 统一用黑色(背景)。
        /// </summary>
        private void BuildColorTable(Model model)
        {
            // 默认全黑
            for (int i = 0; i < 256; i++)
            {
                _colorB[i] = 0;
                _colorG[i] = 0;
                _colorR[i] = 0;
            }

            if (model.classes == null) return;

            foreach (var cls in model.classes)
            {
                if (cls.id < 0 || cls.id > 255) continue;
                Color c = Color.FromArgb(cls.color);
                _colorB[cls.id] = c.B;
                _colorG[cls.id] = c.G;
                _colorR[cls.id] = c.R;
            }
        }
    }
}