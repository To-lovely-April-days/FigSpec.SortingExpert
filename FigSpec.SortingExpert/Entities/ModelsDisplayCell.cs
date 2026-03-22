using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    /// <summary>
    /// 模型节点
    /// </summary>
    public class ModelsDisplayCell : BaseInfo
    {
        public int ID { get; set; }

        public int RegionID { get; set; }

        public EnumModelDisplayType CellType { get; set; }

        public string LocalPath { get; set; }
    }

    public enum EnumModelDisplayType
    {
        LocalModelRoot,
        LocalModel,
        CloudModelRoot,
        CloudModel
    }
}
