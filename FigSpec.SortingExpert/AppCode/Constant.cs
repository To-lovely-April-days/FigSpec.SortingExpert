using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.AppCode
{
    /// <summary>
    /// 常量
    /// </summary>
    public class Constant
    {
        /// <summary>
        /// 魔法棒初始阈值
        /// </summary>
        public static float MagicWandThreshold { get => 0.75f; }
        /// <summary>
        /// 抠图初始阈值
        /// </summary>
        public static byte MattingThreshold { get => 50; }
        /// <summary>
        /// 是否隐藏轮廓功能
        /// </summary>
        public static bool HideOutlineFunc { get => true; }
        /// <summary>
        /// 每个轮廓显示线的最大条数
        /// </summary>
        public static int OutlineShowLineNumber { get => 100; }



    }
}
