using FigSpec.Spectral;
using FigSpec.Spectral.Extensions;
using Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public class ScanSet
    {
        /// <summary>
        /// 校准文件信息
        /// </summary>
        public CalibrationInfo calibrationInfo { get; set; } = new CalibrationInfo();
        /// <summary>
        /// 保存原始图像
        /// </summary>
        public bool SaveSawImgFlag { get; set; } = false;
        /// <summary>
        /// 仅保存显示区域
        /// </summary>
        public bool SaveDisplayArea { get; set; }
        /// <summary>
        /// 仅缓存显示区域
        /// </summary>
        public bool OnlyCacheDisplayArea { get; set; }

    }
}
