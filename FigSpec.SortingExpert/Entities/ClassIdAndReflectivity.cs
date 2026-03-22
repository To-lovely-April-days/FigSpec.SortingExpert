using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Entities
{
    /// <summary>
    /// 用于建模，存储到Csv
    /// </summary>
    public class ClassIdAndReflectivity
    {
        public int ClassId { get; set; }

        public float[] Reflectivity { get; set; }
    }

    public class ClassIdAndWaveLengthIndex
    {
        public int ClassId { get; set; }

        public List<int> WaveLengthIndexs { get; set; }
    }
}
