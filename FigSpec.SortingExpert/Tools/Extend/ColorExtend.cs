using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public class ColorExtend
    {
        /// <summary>
        /// 默认的19种颜色
        /// </summary>
        public static List<string> DefaultColors = new List<string>()
        {
            "#FFA500",
            "#e6194B", 
            "#3cb44b", 
            "#ffe119", 
            "#4363d8", 
            "#f58231", 
            "#911eb4", 
            "#42d4f4", 
            "#f032e6", 
            "#bfef45", 
            "#fabed4", 
            "#469990", 
            "#dcbeff", 
            "#9A6324", 
            "#fffac8", 
            "#800000", 
            "#aaffc3", 
            "#808000", 
            "#ffd8b1", 
            "#000075"
        };


        public static int GetDefaultColorToArgb(List<int> colors)
        {
            int color;
            foreach (var item in DefaultColors)
            {
                color = Color.FromArgb(230, ColorTranslator.FromHtml(item)).ToArgb();
                if (colors == null || !colors.Contains(color))
                    return color;
            }

            int colorValue = Color.FromArgb(255, 255, 255, 255).ToArgb();
            int d = -5000000;
            color = Color.FromArgb(230, Color.FromArgb(colors.Count * d >
                colorValue ? colorValue - colors.Count * d : colors.Count * d)).ToArgb();

            return color;
        }
    }
}
