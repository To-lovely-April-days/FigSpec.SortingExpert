using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.Tools;
using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public class Preprocessing 
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// 算法类型
        /// </summary>
        public PreprocessTypes PreprocessType { get; set; }
        /// <summary>
        /// 预处理算法
        /// </summary>
        public int PreprocessAlgorithm { get; set; }
        /// <summary>
        /// 滤波长度
        /// </summary>
        public int FilterStrength { get; set; }
        /// <summary>
        /// 滤波次数
        /// </summary>
        public int FilterNumber { get; set; }


    }
}