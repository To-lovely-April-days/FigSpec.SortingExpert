using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using Newtonsoft.Json;
using NumSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FigSpec.SortingExpert.ModelFiles
{
    public class Model : BaseInfo
    {
        /// <summary>
        /// 模型文件的文件名，不保存
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public string ModelFileName { get; set; }
        /// <summary>
        /// 模型上一次训练HDR文件内容的GUID
        /// </summary>
        public string speHdrGuid { get; set; }
        /// <summary>
        /// 模型训练验证图片路径
        /// </summary>
        public string spe_image_path { get; set; }
        /// <summary>
        /// 高光谱图像SPE路径
        /// </summary>
        public string hdr_spe_path { get; set; }
        /// <summary>
        /// 分类标签信息
        /// </summary>
        public List<LabelClass> classes { get; set; }
        /// <summary>
        /// 分类后的信息
        /// </summary>
        public List<PLS> plss { get; set; }
        /// <summary>
        /// 选择的选区信息
        /// </summary>
        public List<Selecting> selections { get; set; }

        public bool Loaded { get => plss != null; }


        ///// <summary>
        ///// 黑校准数据
        ///// </summary>
        //[JsonIgnore, XmlIgnore]
        //public NDArray CAL_W { get; set; }
        //public string CAL_W_Xml 
        //{
        //    get => CAL_W.ToXmlString();
        //    set => CAL_W = NDArrayExtend.FromXmlString(value);
        //}
        ///// <summary>
        ///// 白校准数据
        ///// </summary>
        //[JsonIgnore, XmlIgnore]
        //public NDArray CAL_B { get; set; }
        //public string CAL_B_Xml
        //{
        //    get => CAL_B.ToXmlString();
        //    set => CAL_B = NDArrayExtend.FromXmlString(value);
        //}

        /// <summary>
        /// 曝光时间
        /// </summary>
        public int ExpTime { get; set; } = -1;
        /// <summary>
        /// sn号
        /// </summary>
        public string Sn { get; set; }
        /// <summary>
        /// 增益
        /// </summary>
        public int Gain { get; set; }
        /// <summary>
        /// 相机类型
        /// </summary>
        public  string FxModel { get; set; }
        /// <summary>
        /// 高光盘图像的波长信息 对应于 spe.Hdr.WaveLength， 是相机支持的所有波长信息
        /// </summary>
        public float[] HdrWaveLength { get; set; }
        /// <summary>
        /// 数据类型 校准后的图片类型为4
        /// </summary>
        public int DataType { get; set; }

        public int ImgR { get; set; }
        /// <summary>
        /// G的波长通道
        /// </summary>
        public int ImgG { get; set; }
        /// <summary>
        /// B的波长通道
        /// </summary>
        public int ImgB { get; set; }
        /// <summary>
        /// R的波长通道
        /// </summary>
        public float ImgFR { get; set; }
        /// <summary>
        /// G的波长通道
        /// </summary>
        public float ImgFG { get; set; }
        /// <summary>
        /// B的波长通道
        /// </summary>
        public float ImgFB { get; set; }
        /// <summary>
        /// 图像阈值R
        /// </summary>
        public int ImgThresholdR { get; set; }
        /// <summary>
        /// 图像阈值G
        /// </summary>
        public int ImgThresholdG { get; set; }
        /// <summary>
        /// 图像阈值B
        /// </summary>
        public int ImgThresholdB { get; set; }
        //这些信息需要
        /// <summary>
        /// 模型类型 0 PLS, 1 SVM
        /// </summary>
        public int ModelType { get; set; } = 0;
        /// <summary>
        /// 启用轮廓计算
        /// </summary>
        public bool ContourEnabled { get; set; }
        /// <summary>
        /// 轮廓阈值
        /// </summary>
        public float ContourThreshold { get; set; }
        /// <summary>
        /// 轮廓腐蚀度
        /// </summary>
        public int ContourErode { get; set; }

        /// <summary>
        /// 背景启用
        /// </summary>
        public bool BackgroundCorrection { get; set; }
        /// <summary>
        /// 背景平均基数
        /// </summary>
        public float StartBackgroundThreshold { get; set; }

        public float EndBackgroundThreshold { get; set; }


        /// <summary>
        /// 预处理算法，模型中的都应是已经启用的算法
        /// </summary>
        public List<Preprocessing> Preprocessings { get; set; } = new List<Preprocessing>();
        /// <summary>
        /// 开始波段，是HdrWaveLength中的索引
        /// </summary>
        public int StartBandIndex { get; set; }
        /// <summary>
        /// 结束波段，是HdrWaveLength中的索引
        /// </summary>
        public int EndBandIndex { get; set; }
        /// <summary>
        /// ROI模式下的索引，即在parminfo.SpectralChannelWavelength的开始索引
        /// 每次在设置ROI模式后更新
        /// </summary>
        public int RoiBandStartIndex { get; set; }
        /// <summary>
        /// 重置所有参数
        /// </summary>
        public void Reset()
        {
            ModelType = 0;
            //ExpTime = -1;
            //CAL_W = np.zeros(0);
            //CAL_B = np.zeros(0);
            plss = null;
        }
        /// <summary>
        /// 保存到文件 .fsmodel 含有标签，高光谱图像路径等信息的完整文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool TrySave(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            string content = JsonHelp.SerializeObject<Model>(this);
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }

            string file = $"{filePath}{name}-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.fsmodel"; 
            if (!File.Exists(file))
            {
                File.Create(file).Dispose();
            }
            File.WriteAllText(file, content);
            return true;
        }
        public bool TrySave(string filePath, string fileName)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"{filePath}{name}-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.fsmodel";
            }
            string content = JsonHelp.SerializeObject<Model>(this);
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }
            string file = filePath + fileName;
            if (!File.Exists(file))
            {
                File.Create(file).Dispose();
            }
            File.WriteAllText(file, content);
            return true;
        }
        /// <summary>
        /// 保存到文件 .fsmodel 含有标签，高光谱图像路径等信息的完整文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool TryRead(string filePath, ref Model model)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (!File.Exists(filePath))
            {
                return false;
            }
            string content = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }
         
            model  = JsonHelp.DeserializeObject<Model>(content);
            return true;
        }

        /// <summary>
        /// 加载模型文件 .pmodel  plsda算法生成的文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="pls"></param>
        /// <param name="similarity"></param>
        public bool TryLoad(string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }
            Reset();
            plss = new List<PLS>();
            var modelC = File.ReadAllLines(filename);
            int m = 0;
            var kv = modelC[m++].Split('=');
            if (kv.Length != 2) return false;
            string prop = kv[0].Trim();
            string rawValue = kv[1].Trim();
            if (prop == "model_type" && int.TryParse(rawValue, out int mt))
            {
                ModelType = mt;
            }
            else
            {
                return false;
            }
            //kv = modelC[m++].Split('=');
            //if (kv.Length != 2) return false;
            //prop = kv[0].Trim();
            //rawValue = kv[1].Trim();
            //if (prop != "sn") return false;
            //Sn = rawValue;

            //kv = modelC[m++].Split('=');
            //if (kv.Length != 2) return false;
            //prop = kv[0].Trim();
            //rawValue = kv[1].Trim();
            //if (prop == "exp" && int.TryParse(rawValue, out mt))
            //    ExpTime = mt;

            //kv = modelC[m++].Split('=');
            //if (kv.Length != 2) return false;
            //prop = kv[0].Trim();
            //rawValue = kv[1].Trim();
            //if (prop != "fx_model") return false;
            //FxModel = rawValue;

            kv = modelC[m++].Split('=');
            if (kv.Length != 2) return false;
            prop = kv[0].Trim();
            rawValue = kv[1].Trim();
            int count = 0;
            if (prop == "model_count" && int.TryParse(rawValue, out mt))
                count = mt;
            else
                return false;
            
            
            PLS lpls = null;
            for (int n = 0; n < count; n++)
            {
                if (ModelType == 0)
                {
                    lpls = new PLS();
                    
                    plss.Add(lpls);
                    for (int p = 0; p < 7; p++)
                    {
                        kv = modelC[m + n * 7 + p].Split('=');
                        if (kv.Length != 2) continue;
                        prop = kv[0].Trim();
                        rawValue = kv[1].Trim();
                        switch (prop)
                        {
                            case "classid":
                                lpls.classid = int.Parse(rawValue);
                                break;
                            case "n_components":
                                lpls.Components = int.Parse(rawValue);
                                break;
                            case "n_features_in":
                                lpls.Features = int.Parse(rawValue);
                                break;
                            //case "wavelength":
                            //    string[] datas = rawValue.Split(',');
                            //    lpls.selectWavelength = new float[datas.Length];
                            //    for (int i = 0; i < datas.Length; i++)
                            //    {
                            //        if (float.TryParse(datas[i], out float result))
                            //            lpls.selectWavelength[i] = result;
                            //        else
                            //            lpls.selectWavelength[i] = 0;
                            //    }
                            //    break;
                            case "intercept":
                                lpls.Intercept = np.array(ParseFloatArray(rawValue).items);
                                break;
                            case "coef":
                                (var shape, var items) = ParseFloatArray(rawValue);
                                var ceof = new float[shape[0], shape[1]];
                                for (int i = 0; i < shape[0]; i++)
                                {
                                    for (int j = 0; j < shape[1]; j++)
                                    {
                                        ceof[i, j] = items[i * shape[1] + j];
                                    }
                                }
                                lpls.Ceof = np.array(ceof);
                                break;
                            case "x_std":
                                lpls.StdX = np.array(ParseFloatArray(rawValue).items);
                                break;
                            case "x_mean":
                                lpls.MeanX = np.array(ParseFloatArray(rawValue).items);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            return true;
        }
        private (int[] shape, float[] items) ParseFloatArray(string value)
        {
            var sv = Regex.Split(value, @"\]\[");
            //var sv = value.Split("][");
            var _shape = sv[0].Trim('[').Split(',');
            var _items = sv[1].Trim(']').Split(',');
            var shape = new int[_shape.Length];
            for (int i = 0; i < _shape.Length; i++)
                shape[i] = int.Parse(_shape[i]);

            var items = new float[_items.Length];
            for (int i = 0; i < _items.Length; i++)
                items[i] = float.Parse(_items[i]);

            return (shape, items);
        }
    }
}
