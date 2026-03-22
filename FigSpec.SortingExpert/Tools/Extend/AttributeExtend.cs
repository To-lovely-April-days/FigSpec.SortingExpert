using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{

    public class AttributeExtendMethod
    {

        public static void SetPropertyVisibility(object obj, string propertyName, bool visible)
        {
            Type type = typeof(BrowsableAttribute);
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(obj);
            AttributeCollection attrs = props[propertyName].Attributes;
            FieldInfo fld = type.GetField("browsable", BindingFlags.Instance | BindingFlags.NonPublic);
            fld.SetValue(attrs[type], visible);
        }

        public static void SetPropertyReadOnly(object obj, string propertyName, bool readOnly)
        {
            Type type = typeof(System.ComponentModel.ReadOnlyAttribute);
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(obj);
            AttributeCollection attrs = props[propertyName].Attributes;
            FieldInfo fld = type.GetField("isReadOnly", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance);
            fld.SetValue(attrs[type], readOnly);
        }


    }

    public class CategoryExtendAttribute : CategoryAttribute
    {
        public CategoryExtendAttribute(string category): base(category.ToMultiLanguage())
        {
            
        }
    }

    /// <summary>
    /// 扩展多语言
    /// </summary>
    public class DisplayNameExtendAttribute : DisplayNameAttribute
    {
        public DisplayNameExtendAttribute(string displayName) : base(displayName.ToMultiLanguage())
        {

        }
    }
}
