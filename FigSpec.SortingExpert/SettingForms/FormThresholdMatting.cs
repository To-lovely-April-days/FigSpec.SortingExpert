using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using FigSpec.SortingExpert.AppCode;
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
using ImgView;
using Hyperspectral.SpectralFile;
using Hyperspectral.SpectralProc;

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class FormThresholdMatting : BaseFormSetting
    {
        string selectionUid;
        Selecting currSelection;

        public Action SetMattingImage;

        public Action FirstCalculateNotice;

        public SPE spe;
        public SpectraOfSPE spectraOfSPE;

        public FormThresholdMatting(string uid)
        {
            selectionUid = uid;
            currSelection = GlobalSettings.ApplySetting.traingSet.label.selections.Find(i => i.uid == selectionUid);
            InitializeComponent();
            FirstCalculateNotice = FirstCalculateNoticeEvent;
        }

        private void FirstCalculateNoticeEvent()
        {
            if (currSelection.CompareResult != null)
            {
                bool isNull = true;
                for (int i = 0; i < currSelection.CompareResult.Length; i++)
                {
                    for (int j = 0; j < currSelection.CompareResult[0].Length; j++)
                    {
                        if (currSelection.CompareResult[i][j] == 1)
                        {
                            isNull = false;
                            break;
                        }
                    }
                    if (!isNull)
                    {
                        break;
                    }
                }
                FormShowHelper.ShowLoadingForm(this, "计算中".ToMultiLanguage() + "...");
                if (isNull)
                {
                    //第一次抠图计算阈值，如果选中区域为空，则自动反向二值化，重新计算
                    currSelection.ThresholdAlgorithm = 0;
                    currSelection.CompareResult = CommonMethods.CalaulateMattingCompareResult(currSelection.ThresholdDiffs, currSelection.Threshold, currSelection.ThresholdAlgorithm, currSelection.ErodeLevel);
                    if (this.IsHandleCreated)
                    {
                        this.Invoke(new Action(() => { cboPaiHeiDiSuanFa.SelectedIndex = currSelection.ThresholdAlgorithm; }));
                    }
                }

                GetSelectedPoints();
                FormShowHelper.CloseLoadingForm();
            }

        }

        private void FormThresholdMatting_Load(object sender, EventArgs e)
        {

            List<ValueTextItem<int>> items = new List<ValueTextItem<int>>()
            {
                new ValueTextItem<int>(0, "二值化".ToMultiLanguage()),
                new ValueTextItem<int>(1, "二值化反转".ToMultiLanguage()),
            };
            cboPaiHeiDiSuanFa.BindComboBoxItem(items);

            barThreshold.Properties.Maximum = 255;
            barThreshold.Properties.Minimum = 0;
            barThreshold.Properties.TickFrequency = 1;

            spThreshold.EditValue = barThreshold.EditValue = (int)currSelection.Threshold;
            cboPaiHeiDiSuanFa.SelectedIndex = currSelection.ThresholdAlgorithm;

            barErodeLevel.EditValue = currSelection.ErodeLevel;
            spErodeLevel.EditValue = currSelection.ErodeLevel;

            this.spThreshold.EditValueChanged += new System.EventHandler(this.spThreshold_EditValueChanged);
            this.barThreshold.EditValueChanged += new System.EventHandler(this.barThreshold_EditValueChanged);
            this.spErodeLevel.EditValueChanged += new System.EventHandler(this.spErodeLevel_EditValueChanged);
            this.barErodeLevel.EditValueChanged += new System.EventHandler(this.barErodeLevel_EditValueChanged);
            cboPaiHeiDiSuanFa.SelectedIndexChanged += cboPaiHeiDiSuanFa_SelectedIndexChanged;
        }


        private void barErodeLevel_EditValueChanged(object sender, EventArgs e)
        {
            spErodeLevel.EditValueChanged -= spErodeLevel_EditValueChanged;
            spErodeLevel.EditValue = barErodeLevel.EditValue;
            spErodeLevel.EditValueChanged += spErodeLevel_EditValueChanged;
            RefreshImage();
        }

        private void spErodeLevel_EditValueChanged(object sender, EventArgs e)
        {
            barErodeLevel.EditValueChanged -= barErodeLevel_EditValueChanged;
            barErodeLevel.EditValue = spErodeLevel.EditValue;
            barErodeLevel.EditValueChanged += barErodeLevel_EditValueChanged;
            RefreshImage();
        }

        private void spThreshold_EditValueChanged(object sender, EventArgs e)
        {
            barThreshold.EditValueChanged -= barThreshold_EditValueChanged;
            barThreshold.EditValue = spThreshold.EditValue;
            barThreshold.EditValueChanged += barThreshold_EditValueChanged;
            RefreshImage();
        }

        private void barThreshold_EditValueChanged(object sender, EventArgs e)
        {
            spThreshold.EditValueChanged -= spThreshold_EditValueChanged;
            spThreshold.EditValue = barThreshold.EditValue;
            spThreshold.EditValueChanged += spThreshold_EditValueChanged;
            RefreshImage();
        }



        private void cboPaiHeiDiSuanFa_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        private void RefreshImage()
        {
            FormShowHelper.ShowLoadingForm(this, "计算中".ToMultiLanguage() + "...");
            currSelection.Threshold = Convert.ToByte(barThreshold.EditValue);
            currSelection.ThresholdAlgorithm = cboPaiHeiDiSuanFa.SelectedIndex;
            currSelection.ErodeLevel = Convert.ToInt32(spErodeLevel.Value);
            currSelection.CompareResult = CommonMethods.CalaulateMattingCompareResult(currSelection.ThresholdDiffs, currSelection.Threshold, currSelection.ThresholdAlgorithm, currSelection.ErodeLevel);

            GetSelectedPoints();

            FormShowHelper.CloseLoadingForm();
        }

        private void GetSelectedPoints()
        {
            ClassTools.GetReflectBySelectMattingArea(currSelection, spe, spectraOfSPE);
            SetMattingImage?.Invoke();
        }




        private void FormThresholdMatting_FormClosed(object sender, FormClosedEventArgs e)
        {
            barThreshold.EditValueChanged -= barThreshold_EditValueChanged;
            cboPaiHeiDiSuanFa.SelectedIndexChanged -= cboPaiHeiDiSuanFa_SelectedIndexChanged;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}