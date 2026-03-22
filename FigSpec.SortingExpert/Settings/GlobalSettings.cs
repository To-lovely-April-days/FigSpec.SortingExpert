using CHNSpec.Tools;
using FigSpec.Sorting.Entities;
using FigSpec.SortingExpert.Entities;
using Globalization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert
{
    public class GlobalSettings
    {
        public static ApplyInfo ApplyInfo = new ApplyInfo();
        private static XApplySetting applySetting;

        public static XApplySetting ApplySetting
        {
            get
            {
                if (applySetting == null)
                {
                    string filePath = ApplyInfo.ApplySettingPath + "ApplySetting.xml";
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            applySetting = XmlHelper.XMLFlieToObject<XApplySetting>(filePath, Encoding.UTF8);
                        }
                        catch(Exception ex)
                        {
                            File.Delete(filePath);
                        }
                    }           
                    if (applySetting == null)
                    {
                        applySetting = applySetting ?? new XApplySetting();
                        applySetting.traingSet.label.classes = new List<LabelClass>() {
                        new LabelClass() { id = 254, uid = "uid_Unclassified", name = "未分类".ToMultiLanguage(),color = Color.FromArgb(230, Color.DarkGray).ToArgb() },
                        new LabelClass() { id = 255, uid = "uid_background", name = "背景".ToMultiLanguage(), color =  Color.FromArgb(230, Color.DarkGray).ToArgb() },
                        new LabelClass() { id = 1, uid = "uid_target", name = "目标".ToMultiLanguage(), color =  Color.FromArgb(230, Color.Orange).ToArgb() }};
                        ApplySetting = applySetting;
                    }

                }
                return applySetting;
            }

            set
            {
                value.Save();
                applySetting = value;
            }
        }

        public static string CurrentFormName { get; set; }
 
    }
}
