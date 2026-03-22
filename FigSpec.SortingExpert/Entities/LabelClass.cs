using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public class LabelClass
    {
        public int id { get; set; }

        /// <summary>
        /// 是否用户可编辑，删除 id 254 未分类,255 背景,不可删除
        /// </summary>
        public bool CanDelete { get => id < 254; }

        public string uid { get; set; }

        public string name { get; set; }

        public int color { get; set; }//使用Color类型无法正常保存到xml文件

        // <summary>
        /// 阈值是否启用
        /// </summary>
        public bool ThresholdEnabled { get; set; }
        /// <summary>
        /// 阈值
        /// </summary>
        public float Threshold { get; set; } = 0.5f;

        /// <summary>
        /// 描述
        /// </summary>
        public string Remark { get; set; }
    }
}
