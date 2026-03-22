using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Entities;
using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hyperspectral.SpectralFile;

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class FormThresholdChange : BaseFormSetting
    {
        string selectionUid;
        Selecting currSelection;
        SPE spe;

        public Action<string> SetMagicImage;


        public FormThresholdChange(string uid, SPE spe)
        {
            selectionUid = uid;
            this.spe = spe;
            InitializeComponent();
        }

        private void FormThresholdChange_Load(object sender, EventArgs e)
        {
            currSelection = GlobalSettings.ApplySetting.traingSet.label.selections.Find(i => i.uid == selectionUid);
            barThreshold.Properties.Maximum = 100;
            barThreshold.Properties.Minimum = 0;
            barThreshold.Properties.TickFrequency = 1;

            spThreshold.EditValue = currSelection.ThresholdPoint;
            barThreshold.EditValue = (int)(currSelection.ThresholdPoint * 100);

            barErodeLevel.EditValue = currSelection.ErodeLevel;
            spErodeLevel.EditValue = currSelection.ErodeLevel;

            this.spThreshold.EditValueChanged += new System.EventHandler(this.spThreshold_EditValueChanged);
            this.barThreshold.EditValueChanged += new System.EventHandler(this.barThreshold_EditValueChanged);
            this.spErodeLevel.EditValueChanged += new System.EventHandler(this.spErodeLevel_EditValueChanged);
            this.barErodeLevel.EditValueChanged += new System.EventHandler(this.barErodeLevel_EditValueChanged);
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
            barThreshold.EditValue = Convert.ToInt32(spThreshold.Value * 100);
            barThreshold.EditValueChanged += barThreshold_EditValueChanged;
            RefreshImage();
        }

        private void barThreshold_EditValueChanged(object sender, EventArgs e)
        {
            spThreshold.EditValueChanged -= spThreshold_EditValueChanged;
            spThreshold.EditValue = barThreshold.Value / 100f;
            spThreshold.EditValueChanged += spThreshold_EditValueChanged;
            RefreshImage();
        }

        private void RefreshImage()
        {
            currSelection.ThresholdPoint = Convert.ToSingle(spThreshold.Value);
            currSelection.ErodeLevel = Convert.ToInt32(spErodeLevel.Value);
            TrainFormHelper helper = new TrainFormHelper();
            currSelection.CompareResult = helper.CalaulateMagicCompareResult(currSelection.ThresholdDiffPoint, currSelection.ThresholdPoint, currSelection.ErodeLevel);
            SetMagicImage?.Invoke(selectionUid);
        }




        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}