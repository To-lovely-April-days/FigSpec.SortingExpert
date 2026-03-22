using FigSpec.SortingExpert.Enums;
using Globalization;
using Hyperspectral.SpectralFile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public static class HypeImgData
    {
        public static List<HypeImg> hypeImgs { get; set; }
        public static HypeImg GetHypeImgRandom() { return new HypeImg() ; }
        public static List<HypeImg> GetHypeImgRandoms(int count)
        {
            var his = new List<HypeImg>();
            for (int i = 0; i < count; i++)
            {
                his.Add(GetHypeImgRandom());
            }
            return his;
        }
    }
    /// <summary>
    /// 用于在目录界面显示高光谱图像
    /// </summary>
    public class HypeImg
    {
        public Image image { get; set; }

        public string name { get; set; }

        public string fullPath { get; set; }

        public string remark { get; set; }

        public HDR HdrInfo { get; set; }
    }
}
