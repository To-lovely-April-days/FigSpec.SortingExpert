using FigSpec.SortingExpert.ModelFiles;
using Hyperspectral.SpectralFile;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FigSpec.SortingExpert.Entities
{
    public class TraingSet
    {
        public string uid { get; set; } = Guid.NewGuid().ToString();
        /// <summary>
        /// 图像通道设置R
        /// </summary>
        public int ImgR { get; set; } = 62;

        /// <summary>
        /// 图像通道设置G
        /// </summary>
        public int ImgG { get; set; } = 57;
        /// <summary>
        /// 图像通道设置B
        /// </summary>
        ///  private int _imgG;
        ///  
        public int ImgB { get; set; } = 15;
        /// <summary>
        /// 图像通道设置R
        /// </summary>
        public float ImgFR { get; set; }
        /// <summary>
        /// 图像通道设置G
        /// </summary>
        public float ImgFG { get; set; }
        /// <summary>
        /// 图像通道设置B
        /// </summary>
        public float ImgFB { get; set; }
        /// <summary>
        /// 图像阈值R
        /// </summary>
        public float ImgThresholdR { get; set; } = 100;
        /// <summary>
        /// 图像阈值G
        /// </summary>
        public float ImgThresholdG { get; set; } = 100;
        /// <summary>
        /// 图像阈值B
        /// </summary>
        public float ImgThresholdB { get; set; } = 100;


        [JsonIgnore, XmlIgnore]
        public SPE spe { get; set; }
        /// <summary>
        /// 标签信息
        /// </summary>
        public HypeLabel label { get; set; } = new HypeLabel();
        /// <summary>
        /// 算法信息
        /// </summary>
        public Algorithm algorithm { get; set; } = new Algorithm();
        /// <summary>
        /// 预处理算法
        /// </summary>
        public List<Preprocessing> Preprocessings { get; set; } = new List<Preprocessing>();
        /// <summary>
        /// 模型信息
        /// </summary>
        public List<Model> models { get; set; } = new List<Model>();
    }
}
