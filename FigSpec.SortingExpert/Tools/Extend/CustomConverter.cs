using CHNSpec.Tools;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.SettingForms;
using Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace FigSpec.SortingExpert.Tools
{
    /// <summary>
    /// 下拉列表项
    /// </summary>
    public class ValueTextItem<T> where T : struct
    {
        public string Text { get; set; }

        public T Value { get; set; }

        public object Tag { get; set; }

        public ValueTextItem(T value, string text)
        {
            this.Text = text;
            this.Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class ValueStringItem
    {
        public string Text { get; set; }

        public string Value { get; set; }

        public object Tag { get; set; }

        public ValueStringItem(string value, string text)
        {
            this.Text = text;
            this.Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }

    public class BoolConverter : TypeConverter
    {
        private bool[] values;
        private string[] names;

        public BoolConverter()
        {
            values = new bool[2] { true, false };
            names = new string[] { "是".ToMultiLanguage(), "否".ToMultiLanguage() };
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(values);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (destType == typeof(string))
            {
                if ((bool)value == true)
                    return "是".ToMultiLanguage();
                else
                    return "否".ToMultiLanguage();
            }
            return base.ConvertTo(context, culture, value, destType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            if (srcType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, srcType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                if ((string)value == "是".ToMultiLanguage())
                    return true;
                else
                    return false;
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    public class BandsConverter : TypeConverter
    {
        private int[] values;
        private string[] names;

        public BandsConverter()
        {
            ReSet();
        }

        private void ReSet()
        {
            var waveLengths = GlobalSettings.ApplySetting.traingSet.algorithm?.train_bands?.WaveLength;
            if (waveLengths == null)
            {
                values = new int[0];
                names = new string[0];
                return;
            }
            values = new int[waveLengths.Length];
            names = new string[waveLengths.Length];
            for (int i = 0; i < waveLengths.Length; i++)
            {
                values[i] = i;
                names[i] = $"{i}: {waveLengths[i].ToString("F2")}";
            }
        }

        private void Refresh()
        {
            var waveLengths = GlobalSettings.ApplySetting.traingSet.algorithm?.train_bands?.WaveLength;
            if (values?.Length != waveLengths?.Length)
            {
                ReSet();
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            Refresh();
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(values);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (destType == typeof(string))
            {
                if (names.Length == 0)
                {
                    ReSet();
                }
                if (names.Length > 0)
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        if (values[i] == (int)value)
                        {
                            return names[i];
                        }
                    }
                    return names[0];
                }
            }
            return base.ConvertTo(context, culture, value, destType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            if (srcType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, srcType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                string valueString = (string)value;
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i] == valueString)
                        return values[i];
                }
                return values[0];
            }
            return base.ConvertFrom(context, culture, value);
        }
    }

    /// <summary>
    /// 训练波长集合弹框
    /// </summary>
    public class TrainBandsEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // 设置编辑器的编辑风格为模式对话框
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            // 编辑值时弹出自定义对话框
            var model = value as TrainBands;
            if (model == null)
                model = new TrainBands();

            SelectTrainBandsForm subForm = new SelectTrainBandsForm(model);
            subForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            subForm.MaximizeBox = true;
            subForm.MinimizeBox = false;
            subForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            subForm.ShowIcon = false;
            subForm.TopLevel = true;

            if (subForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return subForm.trainBands;
            }
            return model;
        }
    }

    public class EnumConverter<T> : TypeConverter where T : Enum
    {
        private int[] values;
        private string[] names;

        public EnumConverter()
        {
            var lst = typeof(T).GetItems().ToMultiLanguage<T>();
            values = new int[lst.Count];
            names = new string[lst.Count];

            for (int i = 0; i < lst.Count; i++)
            {
                var dic = (DictionaryEntry)lst[i];
                values[i] = (int)dic.Key;
                names[i] = dic.Value.ToString().ToMultiLanguage();
            }
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(values);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            if (destType == typeof(string))
            {
                for (int i = 0; i < names.Length; i++)
                {
                    if (values[i] == (int)value)
                    {
                        return names[i];
                    }
                }
                return names[0];
            }
            return base.ConvertTo(context, culture, value, destType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            if (srcType == typeof(string))
                return true;
            else
                return base.CanConvertFrom(context, srcType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                string valueString = (string)value;
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i] == valueString)
                        return values[i];
                }
                return values[0];
            }
            return base.ConvertFrom(context, culture, value);
        }
    }


}