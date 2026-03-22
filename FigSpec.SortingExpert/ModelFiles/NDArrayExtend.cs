using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NumSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.ModelFiles
{
    public static class NDArrayExtend
    {
        public static string ToXmlString(this NDArray dArray)
        {
            if (dArray is null || dArray.size == 0)
                return string.Empty;
            var listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            string shape = string.Join(listSeparator, dArray.shape);
            string items = string.Join(listSeparator, dArray.flatten().AsIterator().Cast<object>());
            //string shape = string.Join(",", dArray.shape);
            //string items = string.Join(",", dArray.flatten().AsIterator().Cast<object>());
            string res = $"[{shape}][{items}]";
            return res;
        }

        public static NDArray FromXmlString(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                return np.zeros(0);
            }
            (var shape, var items) = ParseFloatArray(rawValue);
            if (shape.Length == 1)
            {
                return np.array(items);
            }
            else
            {
                var ceof = new float[shape[0], shape[1]];
                for (int i = 0; i < shape[0]; i++)
                {
                    for (int j = 0; j < shape[1]; j++)
                    {
                        ceof[i, j] = items[i * shape[1] + j];
                    }
                }
                return np.array(ceof);
            }

        }

        public static (int[] shape, float[] items) ParseFloatArray(string value)
        {
            string listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            var sv = Regex.Split(value, @"\]\[");
            //var sv = value.Split("][");
            //var _shape = sv[0].Trim('[').Split(',');
            //var _items = sv[1].Trim(']').Split(',');
            var _shape = sv[0].Trim('[').Split(listSeparator.First());
            var _items = sv[1].Trim(']').Split(listSeparator.First());
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
