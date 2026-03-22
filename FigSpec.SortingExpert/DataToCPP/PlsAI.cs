using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.DataToCPP
{
    public class PlsAI
    {
        public string TargetName;//材料名称
        public List<int> selectIndex;//设置完ROI之后对应波段的序号
        /// 模型类型 0 PLS, 1 SVM
        public int ModelType;
        public int classid;
        public float threshold;
        public int Components;
        public int Features;
        public List<float> Intercept;
        public List<float> Coef;
        public List<float> StdX;
        public List<float> MeanX;
    }
}
