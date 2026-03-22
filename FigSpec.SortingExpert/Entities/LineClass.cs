using FigSpec.SortingExpert.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{

    public class LineClass
    {
        public int ID { get; set; }
        public float axis { get; set; }//坐标
        public ColorType type { get; set; }//类型0黑校准1白校准
    }
}
