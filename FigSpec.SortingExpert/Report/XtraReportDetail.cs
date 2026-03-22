using DevExpress.XtraReports.UI;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.ModelFiles;
using Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;

namespace FigSpec.SortingExpert
{
    public partial class XtraReportDetail : DevExpress.XtraReports.UI.XtraReport
    {
        public XtraReportDetail(Model model = null)
        {
            InitializeComponent();
            InitData(model);
        }

        public void InitData(Model model = null)
        {
            colAlName.Text = "算法名称".ToMultiLanguage();
            colAlType.Text = "算法类型".ToMultiLanguage();
            colAlParams.Text = "参数信息".ToMultiLanguage();
            colCName.Text = "名称".ToMultiLanguage();
            colCColor.Text = "颜色".ToMultiLanguage();
            colCCode.Text = "编码".ToMultiLanguage();
            colCThreshold.Text = "阈值".ToMultiLanguage();
            colCRemark.Text = "备注".ToMultiLanguage();

            if (model != null)
            {
               
                if (model.classes != null && model.selections != null)
                {
                    var lst =  model.classes.FindAll(i => model.selections.Exists(k => k.classID == i.id));
                    this.DetailReport.DataSource = lst;
                }
                if (model.Preprocessings != null)
                {
                    List<dynamic> lst = new List<dynamic>();
                    foreach (var item in model.Preprocessings)
                    {
                        string typeName = item.PreprocessType.ToMultiLanguage<PreprocessTypes>();
                        string name = string.Empty;
                        string paramText = string.Empty;
                        switch (item.PreprocessType)
                        {
                            case PreprocessTypes.BaselineCorrection:
                                name = ((EnumBaselineCorrection)item.PreprocessAlgorithm).ToMultiLanguage<EnumBaselineCorrection>();
                                break;
                            case PreprocessTypes.ScaleScalingAlgorithm:
                                name = ((EnumScaleScalingAlgorithm)item.PreprocessAlgorithm).ToMultiLanguage<EnumScaleScalingAlgorithm>();
                                break;
                            //case PreprocessTypes.ScatteringCorrection:
                            //    name = ((EnumScatteringCorrection)model.PreprocessAlgorithm).ToMultiLanguage<EnumScatteringCorrection>();
                            //    break;
                            case PreprocessTypes.SmoothingAlgorithm:
                                name = ((EnumSmoothingAlgorithm)item.PreprocessAlgorithm).ToMultiLanguage<EnumSmoothingAlgorithm>();
                                paramText = "滤波长度:".ToMultiLanguage() + item.FilterStrength.ToString();
                                if (item.PreprocessAlgorithm == (int)EnumSmoothingAlgorithm.SGSmoothing)
                                {
                                    paramText += ",滤波次数:".ToMultiLanguage() + item.FilterNumber.ToString();
                                }
                                break;
                        }
                        lst.Add(new
                        {
                            TypeName = typeName,
                            Name = name,
                            ParamText = paramText
                        });
                    }

                    this.DetailReport1.DataSource = lst;
                }
                lblTitle.Text = model.name;
                lblDatetime.Text = model.createTime.Split('-')[0];
                lblPretreatment.Text = "预处理".ToMultiLanguage();
                lblClassification.Text = "标签信息".ToMultiLanguage();
                lblOtherInfo.Text = "波长信息".ToMultiLanguage();
                if (model.HdrWaveLength != null && model.HdrWaveLength.Length > 0)
                {
                    lblFullWavelength.Text = "支持的波长范围：".ToMultiLanguage() + " " + $"{model.HdrWaveLength[0]}nm - {model.HdrWaveLength[model.HdrWaveLength.Length - 1]}nm";
                }
                else 
                {
                    lblFullWavelength.Text = "";
                }
                lblEffectiveWavelength.Text = "使用的波长范围：".ToMultiLanguage() + " " + $"{model.HdrWaveLength[model.StartBandIndex]}nm - {model.HdrWaveLength[model.EndBandIndex]}nm";
                lblSensor.Text = "设备型号：".ToMultiLanguage() + " " + model.FxModel;
                lblVersion.Text = "版本：".ToMultiLanguage() + " " + model.version;
                lblRemark.Text = "备注：".ToMultiLanguage() + " " + model.remark;
            }
            else
            {
                lblTitle.Text = "";
                lblDatetime.Text = "";
                lblPretreatment.Text = "预处理".ToMultiLanguage();
                lblClassification.Text = "标签信息".ToMultiLanguage();
                lblOtherInfo.Text = "波长信息".ToMultiLanguage();
                lblFullWavelength.Text = "";

                lblSensor.Text = "";
                lblVersion.Text = "";
                lblRemark.Text = "";

                this.DetailReport.DataSource = null;
                this.DataSource = null;
            }
        }

        private void xrTableCell6_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cellCColor.Text) && int.TryParse(cellCColor.Text, out int value))
            {
                cellCColor.BackColor = Color.FromArgb(value);
            }
            else
            {
                cellCColor.BackColor = Color.DarkGray;
            }
            cellCColor.TextChanged -= xrTableCell6_TextChanged;
            cellCColor.Text = string.Empty;
            cellCColor.TextChanged += xrTableCell6_TextChanged;
        }

    }
}
