using DevExpress.Utils.Extensions;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Tools;
using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class ClassSettingForm : BaseFormSetting
    {
        public List<LabelClass> LabelClasses { get; set; }

        public ClassSettingForm(List<LabelClass> _labelClasses)
        {
            InitializeComponent();
            LabelClasses = _labelClasses ?? new List<LabelClass>();
        }

        private void ClassSettingForm_Load(object sender, EventArgs e)
        {
            RefreClassDataSource();
        }

        /// <summary>
        /// 刷新分类数据源
        /// </summary>
        /// <param name="listLabelClass"></param>
        private void RefreClassDataSource(LabelClass focusClass = null)
        {
            if (LabelClasses.Count > 0)
            {
                LabelClasses = LabelClasses.OrderBy(o => o.id).ToList();
            }

            gcClass.DataSource = LabelClasses;
            gcClass.RefreshDataSource();
            if (focusClass != null)
            {
                gvClass.FocusedRowHandle = LabelClasses.IndexOf(focusClass);
            }
            else if (LabelClasses.Count > 0)
            {
                gvClass.FocusedRowHandle = 0;
            }
            gvClass_Click(null, null);
        }

        /// <summary>
        /// 选中目标行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gvClass_Click(object sender, EventArgs e)
        {
            if (gvClass.FocusedRowHandle < 0)
            {
                return;
            }
            LabelClass labelClass = gvClass.GetFocusedRow() as LabelClass;
            btnDeleteRow.Enabled = labelClass.CanDelete;

            EditValueChangedEventCancel = true;

            txtName.Text = labelClass.name;
            colorEdit.EditValue = labelClass.color;
            spinCode.EditValue = labelClass.id;
            //chcEditthreshold.Checked = labelClass.ThresholdEnabled;
            //spinType.ReadOnly = !chcEditthreshold.Checked;
            spinType.EditValue = labelClass.Threshold;
            txtRemark.EditValue = labelClass.Remark;

            EditValueChangedEventCancel = false;
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteRow_Click(object sender, EventArgs e)
        {
            if (gvClass.RowCount <= 0)
            {
                return;
            }
            LabelClass labelClass = gvClass.GetFocusedRow() as LabelClass;
            if (!labelClass.CanDelete)
            {
                return;
            }
            if (labelClass != null)
            {
                LabelClasses.Remove(labelClass);
            }
            var nextClass = LabelClasses.FindLast(i => i.id < labelClass.id);
            RefreClassDataSource(nextClass);
        }

        /// <summary>
        /// 新增分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInsertRow_Click(object sender, EventArgs e)
        {
            if (gvClass.RowCount >= 20)
            {
                FormShowHelper.ShowMessage("数量已经达到最大值".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            LabelClass labelClass = new LabelClass();
            for (int i = 1; i <= 20; i++)
            {
                if (!LabelClasses.Exists(o => o.id == i))
                {
                    labelClass.id = i;
                    break;
                }
            }

            labelClass.uid = Guid.NewGuid().ToString();
            labelClass.name = "类别".ToMultiLanguage() + labelClass.id;

   
            List<int> existColors = LabelClasses.Count == 0 ? null :  LabelClasses.Select(i => i.color).ToList();
            labelClass.color = ColorExtend.GetDefaultColorToArgb(existColors);

            LabelClasses.Add(labelClass);
            RefreClassDataSource(labelClass);
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnColose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void gvClass_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            //绘制颜色块
            if (e.Column.Name == "columnColor" && LabelClasses.Count > 0)
            {
                LabelClass labelClass = gvClass.GetRow(e.RowHandle) as LabelClass;
                e.Appearance.BackColor = Color.FromArgb(labelClass.color);
            }
        }

        /// <summary>
        /// 事假取消flag
        /// </summary>
        bool EditValueChangedEventCancel = false;
        private void Control_EditValueChanged(object sender, EventArgs e)
        {
            if (EditValueChangedEventCancel)
                return;

            if (gvClass.FocusedRowHandle < 0)
            {
                return;
            }
            LabelClass labelClass = gvClass.GetFocusedRow() as LabelClass;
            labelClass.name = txtName.Text;
            labelClass.color = Color.FromArgb(230, (Color)colorEdit.EditValue).ToArgb();
            //labelClass.ThresholdEnabled = chcEditthreshold.Checked;
            //spinType.ReadOnly = !chcEditthreshold.Checked;
            labelClass.Threshold = (float)Convert.ToDouble(spinType.EditValue);
            labelClass.Remark = txtRemark.Text;

            gcClass.RefreshDataSource();

        }



    }
}
