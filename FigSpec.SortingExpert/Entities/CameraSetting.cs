using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public class CameraSetting
    {
        /// <summary>
        /// 是否是初始数据，是的话，第一次连接相机，设置最大帧频
        /// </summary>
        public bool IsInitialData { get; set; } = true;

        /// <summary>
        /// 序列号
        /// </summary>
        public string CameraSN { get; set; }
        /// <summary>
        /// 曝光时间
        /// </summary>
        public int ExposureTime { get; set; } = 80;
        /// <summary>
        /// 帧率
        /// </summary>
        public int FrameRate { get; set; } = 1000;
        /// <summary>
        /// 触发模式
        /// </summary>
        public int TriggerMode { get; set; } = 0;
        /// <summary>
        /// 增益 默认高增益
        /// </summary>
        public int Gain { get; set; } = 0;

        /// <summary>
        /// 图像通道设置R
        /// </summary>
        public int ImgR { get; set; }
        /// <summary>
        /// 图像通道设置G
        /// </summary>
        public int ImgG { get; set; }
        /// <summary>
        /// 图像通道设置B
        /// </summary>
        public int ImgB { get; set; }
        /// <summary>
        /// 图像通道设置R
        /// </summary>
        public float ImgFR { get; set; } = 1252.98f;
        /// <summary>
        /// 图像通道设置G
        /// </summary>
        public float ImgFG { get; set; } = 1404.30f;
        /// <summary>
        /// 图像通道设置B
        /// </summary>
        public float ImgFB { get; set; } = 1603.93f;

        /// <summary>
        /// 反射率图像阈值R ( 0~100)
        /// </summary>
        public int ImgThresholdR { get; set; } = 100;
        /// <summary>
        /// 反射率图像阈值R ( 0~1)
        /// </summary>
        public float ImgThresholdFR => ImgThresholdR / 100f;
        /// <summary>
        /// 反射率图像阈值G
        /// </summary>
        public int ImgThresholdG { get; set; } = 100;
        public float ImgThresholdFG => ImgThresholdR / 100f;
        /// <summary>
        /// 反射率图像阈值B
        /// </summary>
        public int ImgThresholdB { get; set; } = 100;
        public float ImgThresholdFB => ImgThresholdR / 100f;

        /// <summary>
        /// 原始图像的阈值 (0~16384)
        /// </summary>
        public int ImgOrginThresholdR { get; set; } = 10000;

        public int ImgOrginThresholdG { get; set; } = 10000;

        public int ImgOrginThresholdB { get; set; } = 10000;

        /// <summary>
        /// 显示多少帧数据
        /// </summary>
        public int DisplayFrameNum { get; set; } = 1500;
        /// <summary>
        /// 图像是否镜像翻转
        /// </summary>
        public bool FlipXFlag { get; set; }

    }
}
