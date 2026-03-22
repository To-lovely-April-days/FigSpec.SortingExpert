using System;
using NumSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml.Serialization;


namespace FigSpec.SortingExpert.ModelFiles
{
    public class PLS
    {
        /// <summary>
        /// 模型对于的标签的ID
        /// </summary>
        public int classid { get; set; }
        /// <summary>
        /// 模型的阈值
        /// </summary>
        public float threshold { get; set; }
        /// <summary>
        /// 模型掩模图中的颜色
        /// </summary>
        public int color { get; set; }
        /// <summary>
        /// 选定的波长的索引，真实相机所有支持波段中的索引
        /// </summary>
        public List<int> selectIndex { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Components { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Features { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public NDArray Intercept { get; set; }
        public string Intercept_Xml
        {
            get => Intercept.ToXmlString();
            set => Intercept = NDArrayExtend.FromXmlString(value);
        }
        public List<float> GetIntercept_list()
        {
            return Intercept.GetData<float>().ToList();
        }

        [JsonIgnore, XmlIgnore]
        public NDArray Ceof { get; set; }
        public string Ceof_Xml
        {
            get => Ceof.ToXmlString();
            set => Ceof = NDArrayExtend.FromXmlString(value);
        }
        public List<float> GetCeof_list()
        {
            return Ceof.T.GetData<float>().ToList();
        }

        [JsonIgnore, XmlIgnore]
        public NDArray StdX { get; set; }
        public string StdX_Xml
        {
            get => StdX.ToXmlString();
            set => StdX = NDArrayExtend.FromXmlString(value);
        }
        public List<float> GetStdX_list()
        {
            return StdX.GetData<float>().ToList();
        }

        [JsonIgnore, XmlIgnore]
        public NDArray MeanX { get; set; }
        public string MeanX_Xml
        {
            get => MeanX.ToXmlString();
            set => MeanX = NDArrayExtend.FromXmlString(value);
        }
        public List<float> GetMeanX_list()
        {
           return MeanX.GetData<float>().ToList();
        }

        /// <summary>
        /// 训练模型文件路径，SVM使用 
        /// </summary>
        public string TrainFileName { get; set; }

        /// <summary>
        /// 推断
        /// </summary>
        /// <param name="X"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        public NDArray Predict(NDArray X)
        {
            X = np.subtract(X, MeanX);
            X = np.divide(X, StdX);
            var YPred = X.dot(Ceof);
            return YPred + Intercept;
        }
    }
}
