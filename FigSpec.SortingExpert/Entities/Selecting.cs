using ImgView;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{

    public class Selecting
    {
        /// <summary>
        /// 标签类型
        /// </summary>
        public SelectionType labelType { get; set; }
        /// <summary>
        /// 关联工具内部区域的uid
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// 标签名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 暂时未使用
        /// </summary>
        public bool vision { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public int color { get; set; }
        /// <summary>
        /// 标签本身的uid
        /// </summary>
        public string classUid { get; set; }
        /// <summary>
        /// 标签的ID 0-255 
        /// </summary>
        public int classID { get; set; }


        /// <summary>
        /// 分选算法 规定当前算法下标签计算逻辑
        /// </summary>
        //public TypeAlgorithm TypeAlgorithm { get; set; }

        /// <summary>
        /// 启用轮廓
        /// </summary>
        public bool ContourEnabled { get; set; }
        /// <summary>
        /// 反射率
        /// </summary>
        public List<float[]> reflects { get; set; } = new List<float[]>();

        /// <summary>
        /// 选择区域点的坐标
        /// </summary>
        public Point[] Points { get; set; }

        /// <summary>
        /// 各个轮廓内部的点
        /// </summary>
        public Point[][] ContourPoints { get; set; }

        /// <summary>
        /// 矩形位置
        /// </summary>
        public Rectangle AbsoluteRectangle { get; set; }


        /// <summary>
        /// 抠图的二值化算法 0 二值化， 1 反向二值化
        /// </summary>
        public int ThresholdAlgorithm { get; set; }
        /// <summary>
        /// 抠图的二值化阈值
        /// </summary>
        public byte Threshold { get; set; }
        /// <summary>
        /// 抠图的二值化对比阈值， x,y 为所选矩形的宽高，
        /// </summary>
        public byte[][] ThresholdDiffs { get; set; }


        /// <summary>
        /// 魔法棒计算波长个数
        /// </summary>
        public int CalculateWavelengthCount { get; set; }
        /// <summary>
        /// 魔法棒选择的点对比光谱相似度
        /// </summary>
        public float[][] ThresholdDiffPoint { get; set; }
        /// <summary>
        /// 魔法棒选择的点的阈值
        /// </summary>
        public float ThresholdPoint { get; set; }
        /// <summary>
        /// 腐蚀等级 > 0
        /// </summary>
        public int ErodeLevel { get; set; }
        /// <summary>
        /// 比较结果  0 1
        /// </summary>
        public byte[][] CompareResult { get; set; }

        //勾选状态记录
        [JsonIgnore]
        public bool labelCheck { get; set; } = true;

    }




}
