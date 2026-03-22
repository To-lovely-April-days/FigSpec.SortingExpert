using Globalization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    public class TrainSetDisplayCell : BaseInfo
    {

        public int ID { get; set; }
        /// <summary>
        /// 父节点ID
        /// </summary>
        public int RegionID { get; set; }

        public EnumTrainSetCellType CellType { get; set; }

        public string SelectingUid { get; set; }

        public Color color { get; set; }

        public string classUid { get; set; }

        /// <summary>
        /// 画图获取标签的的类型
        /// </summary>
        public ImgView.SelectionType labelType { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string labelTypeDes { get { return labelType.ToString().ToMultiLanguage(); } }
        public bool  labelCheck { get; set; }=false ;
    }


    public enum EnumTrainSetCellType
    {
        LabelRoot,
        Label,
        ModelRoot,
        Model,
    }
}
