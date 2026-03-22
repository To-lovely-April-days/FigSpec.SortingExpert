using Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public static class EnumExtend
    {
        /// <summary> 
        /// 获得枚举类型数据项（不包括空项）
        /// </summary> 
        /// <param name="enumType">枚举类型</param> 
        /// <returns></returns> 
        public static List<ValueTextItem<int>> GetValueTextItems(this Type enumType)
        {
            if (!enumType.IsEnum)
                throw new InvalidOperationException();

            var list = new List<ValueTextItem<int>>();

            // 获取Description特性 
            Type typeDescription = typeof(DescriptionAttribute);
            // 获取枚举字段
            FieldInfo[] fields = enumType.GetFields();

            
            var formItem = GlobalLanguage.LanguageLibrary.Enumerate.Find(i => i.Key == enumType.Name);
            foreach (FieldInfo field in fields)
            {
                if (!field.FieldType.IsEnum)
                    continue;
                // 获取枚举值
                int value = (int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null);

                //有描述取描述，没有取名称
                object[] array = field.GetCustomAttributes(typeDescription, false);
                if (array.Length > 0)
                {
                    var attr = (DescriptionAttribute)array[0];
                    string text = formItem?.Content.Find(i => i.Key == attr.Description)?.Value ?? field.Name.ToMultiLanguage();
                    if (attr is DescriptionSortAttribute attrSort && attrSort.HasSort && attrSort.SortIndex < list.Count)
                    {
                        list.Insert((int)attrSort.SortIndex, new ValueTextItem<int>(value, text));
                    }
                    else
                    {
                        list.Add(new ValueTextItem<int>(value, text));
                    }
                }
                else
                {
                    list.Add(new ValueTextItem<int>(value, field.Name.ToMultiLanguage()));
                }

            }
            return list;
        }
    }


    public class DescriptionSortAttribute : DescriptionAttribute
    {

        private string description;

        private uint sortIndex;

        private bool hasSort;

        public override string Description => description;

        /// <summary>
        /// 排序
        /// </summary>
        public virtual uint SortIndex => sortIndex;

        public virtual bool HasSort => hasSort;


        public DescriptionSortAttribute() : this(string.Empty, 0, false)
        {

        }

        public DescriptionSortAttribute(string description) : this(description, 0, false)
        {

        }

        public DescriptionSortAttribute(string description, uint sortIndex, bool hasSort = true)
        {
            this.description = description;
            this.sortIndex = sortIndex;
            this.hasSort = hasSort;
        }
    }
}
