using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public class BaseDictionary
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }
        
        /// <summary>
        /// 唯一编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }
    }
}
