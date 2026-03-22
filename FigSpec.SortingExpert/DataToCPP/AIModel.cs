using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.DataToCPP
{
    public class AIModel
    {
        //相机信息
        public string Sn;
        public string FxModel;
        //相机设置
        public int ExpTime;
        public int Gain;
        public int FrameRate;
        public int StartBandIndex;
        public int EndBandIndex;
        /// <summary>
        /// ROI模式下的索引，即在parminfo.SpectralChannelWavelength的开始索引
        /// 每次在设置ROI模式后更新
        /// 应对沈工开发的bug
        /// 训练过程设置的起始位置和结束位置，与相机能够设置的不一致，这个参数用于记录这个偏差
        /// </summary>
        public int RoiBandStartIndex;
        //限制范围（原先的扣底）
        public int LimitScopeFlag;
        public float LowestValue;
        public float HighestValue;
        //预处理
        public List<int> Preprocessings = new List<int> { };
        public int FilterStrength;//滑动滤波长度
        //模型列表
        public List<PlsAI> CoreList;
    }
}
