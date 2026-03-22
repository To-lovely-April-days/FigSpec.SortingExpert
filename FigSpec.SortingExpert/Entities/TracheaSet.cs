using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    /// <summary>
    /// 气管设置
    /// </summary>
    public class TracheaSet
    {
        public int PixelNumber { get; set; } = 640;
        public int StartPixel { get; set; } = 1;
        public int EndPixel { get; set; } = 640;
        public int Trachea { get; set; } = 40;
        public Single TracheaPixels { get; set; } = 16;
        public Single PixelInterval { get; set; } = 16;//偏移量

        public bool TracheaDesc { get; set; }

        public List<TracheaSetItem> Items { get; set; }

        public Dictionary<int, List<int>> GetDictionaryItems()
        {
            Dictionary<int, List<int>> dic = new Dictionary<int, List<int>>();
            if (Items == null || Items.Count == 0)
            {
                return dic;
            }
            foreach (var item in Items)
            {
                var lst = new List<int>();
                if (item.StartPixel > 0 && item.EndPixel > 0)
                {
                    for (float i = item.StartPixel - 1; i < item.EndPixel; i++)
                    {
                        lst.Add((int)i);
                    }
                }
                dic.Add(item.TracheaNumber - 1, lst);
            }
            return dic;
        }
    }

    public class TracheaSetItem
    {
        /// <summary>
        /// 气管编号
        /// </summary>
        public int TracheaNumber { get; set; }
        /// <summary>
        /// 起始像素
        /// </summary>
        public Single StartPixel { get; set; }
        /// <summary>
        /// 结束像素
        /// </summary>
        public Single EndPixel { get; set; }
    }

}
