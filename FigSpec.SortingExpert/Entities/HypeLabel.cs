using Globalization;
using ImgView;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FigSpec.SortingExpert.Entities
{
    public class HypeLabel : BaseInfo
    {
        [JsonIgnore, XmlIgnore]
        public List<Selecting> selections { get; set; } = new List<Selecting>();

        public List<LabelClass> classes { get; set; } = new List<LabelClass>();

        /// <summary>
        /// 矩形，抠图
        /// </summary>
        public Selecting AddSelecting(SelectionType type, RectangularSelection value)
        {
            var defaultClass = GetDefaultClassfy();
            var sel = new Selecting()
            {
                name = GetDefaultName(selections.Count + 1),
                classUid = defaultClass.uid,
                classID = defaultClass.id,
                labelType = type,
                color = defaultClass.color,
                uid = value.Guid,
                ContourEnabled = GlobalSettings.ApplySetting.traingSet.algorithm.train_contour_enabled,
                AbsoluteRectangle = value.AbsoluteRectangle,

                ErodeLevel = 1,
                ThresholdAlgorithm = type == SelectionType.抠图 ? 1 : 0
            };
            selections.Add(sel);
            return sel;
        }

        /// <summary>
        /// 多点类型
        /// </summary>
        public Selecting AddSelecting(SelectionType type, BrushSelection value, float[] lreflect, Point[] points)
        {
            var defaultClass = GetDefaultClassfy();
            var sel = new Selecting()
            {
                name = GetDefaultName(selections.Count + 1),
                classUid = defaultClass.uid,
                classID = defaultClass.id,
                labelType = type,
                color = defaultClass.color,
                uid = value.Guid,
                ContourEnabled = GlobalSettings.ApplySetting.traingSet.algorithm.train_contour_enabled,
                Points = points,
            };
            if (lreflect != null)
                sel.reflects.Add(lreflect);
            selections.Add(sel);
            return sel;
        }

        /// <summary>
        /// 单点类型
        /// </summary>
        public void AddSelecting(SelectionType type, PointSelection value, float[] lreflect, float[][] diffs, float threshold)
        {
            var defaultClass = GetDefaultClassfy();
            selections.Add(new Selecting()
            {
                name = GetDefaultName(selections.Count + 1),
                classUid = defaultClass.uid,
                classID = defaultClass.id,
                labelType = type,
                color = defaultClass.color,
                uid = value.Guid,
                ContourEnabled = GlobalSettings.ApplySetting.traingSet.algorithm.train_contour_enabled,
                reflects = new List<float[]> { lreflect },
                Points = new Point[1] { value.AbsolutePoint },
                ThresholdDiffPoint = diffs,
                ThresholdPoint = threshold,
            });
        }

        public void AddSelecting(SelectionType type, IrregularSelection value, float[] lreflect, byte[][] diffs, byte threshold)
        {
            var defaultClass =  GetDefaultClassfy();
            var sel = new Selecting()
            {
                name = GetDefaultName(selections.Count + 1),
                classUid = defaultClass.uid,
                classID = defaultClass.id,
                labelType = type,
                color = defaultClass.color,
                uid = value.Guid,
                ContourEnabled = GlobalSettings.ApplySetting.traingSet.algorithm.train_contour_enabled,
                AbsoluteRectangle = value.AbsoluteRectangle,
                ThresholdDiffs = diffs,
                Threshold = threshold,
                ErodeLevel = 1,
                ThresholdAlgorithm = type == SelectionType.抠图 ? 1 : 0
            };
            selections.Add(sel);
            if (lreflect != null)
                sel.reflects.Add(lreflect);
        }

        private LabelClass GetDefaultClassfy()
        {
            return GlobalSettings.ApplySetting.traingSet.label.classes.Find(o => o.id == 254);
        }

        private string GetDefaultName(int index)
        {
            string name = "样本".ToMultiLanguage() + index;
            if (selections.Exists(i => i.name == name))
            {
                return GetDefaultName(++index);
            }
            return name;
        }

    }
}
