using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public static class ControlExtend
    {
        /// <summary>
        /// 给ComboBoxEdit绑定Items
        /// </summary>
        /// <param name="combo"></param>
        /// <param name="items"></param>
        public static void BindComboBoxItem(this ComboBoxEdit @combo, List<ValueTextItem<int>> items)
        {
            ComboBoxItemCollection itemCollection = combo.Properties.Items;
            itemCollection.BeginUpdate();
            itemCollection.Clear();
            itemCollection.AddRange(items);
            itemCollection.EndUpdate();
            if (items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        public static void SetSelectedValue<T>(this ComboBoxEdit @combo, T value) where T : struct
        {
            ComboBoxItemCollection itemCollection = combo.Properties.Items;
            var selItem = itemCollection.Cast<ValueTextItem<T>>().FirstOrDefault(i => Equals(i.Value, value));
            if (selItem == null)
            {
                combo.SelectedIndex = -1;
            }
            else
            {
                combo.EditValue = selItem;
            }
        }

        /// <summary>
        /// 获取ComboBoxEdit选中值
        /// </summary>
        /// <param name="combo"></param>
        /// <returns></returns>
        public static T? GetSelectedValue<T>(this ComboBoxEdit @combo) where T : struct
        {
            var selItem = combo.SelectedItem as ValueTextItem<T>;
            return selItem?.Value;
        }
    }
}
