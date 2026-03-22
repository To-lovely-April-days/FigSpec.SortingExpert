using DevExpress.XtraEditors;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
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
    public partial class FormPreprocessEdit : BaseFormSetting
    {
        bool isAdd = false;
        public Preprocessing Model { get; set; }

        public FormPreprocessEdit(Preprocessing model)
        {
            Model = model;
            isAdd = model == null;
            InitializeComponent();
        }

        private void FormPreprocessEdit_Load(object sender, EventArgs e)
        {
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.cboType_SelectedIndexChanged);
            this.cboName.SelectedIndexChanged += new System.EventHandler(this.cboName_SelectedIndexChanged);

            cboType.BindComboBoxItem(typeof(PreprocessTypes).GetValueTextItems());

            if (!isAdd)
            {
                cboType.SetSelectedValue((int)Model.PreprocessType);
                cboName.SetSelectedValue(Model.PreprocessAlgorithm);
                spParam1.EditValue = Model.FilterStrength;
                spParam2.EditValue = Model.FilterNumber;
            }
            this.Text = isAdd ? "新增预处理算法".ToMultiLanguage() : "修改预处理算法".ToMultiLanguage();
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            PreprocessTypes type = (PreprocessTypes)cboType.GetSelectedValue<int>();
            switch (type)
            {
                case PreprocessTypes.BaselineCorrection:
                    cboName.BindComboBoxItem(typeof(EnumBaselineCorrection).GetValueTextItems());
                    break;
                case PreprocessTypes.ScaleScalingAlgorithm:
                    cboName.BindComboBoxItem(typeof(EnumScaleScalingAlgorithm).GetValueTextItems());
                    break;
                //case PreprocessTypes.ScatteringCorrection:
                //    cboName.BindComboBoxItem(typeof(EnumScatteringCorrection).GetValueTextItems());
                //    break;
                case PreprocessTypes.SmoothingAlgorithm:
                    cboName.BindComboBoxItem(typeof(EnumSmoothingAlgorithm).GetValueTextItems());
                    break;
            }
        }

        private void cboName_SelectedIndexChanged(object sender, EventArgs e)
        {
            PreprocessTypes type = (PreprocessTypes)cboType.GetSelectedValue<int>();
            var algorithm = (int)cboName.GetSelectedValue<int>();
            bool param1Visible = false;
            bool param2Visible = false;
            switch (type)
            {
                case PreprocessTypes.BaselineCorrection:
                    break;
                case PreprocessTypes.ScaleScalingAlgorithm:
                    break;
                //case PreprocessTypes.ScatteringCorrection:
                //    break;
                case PreprocessTypes.SmoothingAlgorithm:
                    var smooth = (EnumSmoothingAlgorithm)algorithm;
                    param1Visible = true;
                    param2Visible = smooth == EnumSmoothingAlgorithm.SGSmoothing;
                    break;
            }
            lblParam1.Visible = spParam1.Visible = param1Visible;
            lblParam2.Visible = spParam2.Visible = param2Visible;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            PreprocessTypes type = (PreprocessTypes)cboType.GetSelectedValue<int>();
            var algorithm = (int)cboName.GetSelectedValue<int>();
            int value1 = Convert.ToInt32(spParam1.Value);
            int value2 = Convert.ToInt32(spParam2.Value);

            if (isAdd)
            {
                Model = new Preprocessing()
                {
                    PreprocessType = type,
                    PreprocessAlgorithm = algorithm,
                    FilterStrength = value1,
                    FilterNumber = value2,
                };
            }
            else
            {
                Model.PreprocessType = type;
                Model.PreprocessAlgorithm = algorithm;
                Model.FilterStrength = value1;
                Model.FilterNumber = value2;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void spParam1_Validating(object sender, CancelEventArgs e)
        {
            int value1 = Convert.ToInt32(spParam1.Value);
            dxErrorProvider1.SetError(spParam1, string.Empty);
            if (value1 % 2 == 0)
            {
                dxErrorProvider1.SetError(spParam1, "必须为奇数".ToMultiLanguage());
                e.Cancel = true;
            }
        }
    }
}