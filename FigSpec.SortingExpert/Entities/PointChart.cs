using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public enum AreaType
    {
        Point,
        Rectangle
    }

    public class SeriesData
    {
        /// <summary>
        /// 曲线颜色
        /// </summary>
        public Color SeriesColor { get; set; }
        /// <summary>
        /// 区域类型
        /// </summary>
        public AreaType AreaType { get; set; }
        /// <summary>
        /// 选择区域的guid
        /// </summary>
        public string AreaGuid { get; set; }
        /// <summary>
        /// X坐标
        /// </summary>
        public float PointX { get; set; }
        /// <summary>
        /// Y坐标
        /// </summary>
        public float PointY { get; set; }
        /// <summary>
        /// 光谱数据
        /// </summary>
        public float[] SpectralData { get; set; }


    }

}
