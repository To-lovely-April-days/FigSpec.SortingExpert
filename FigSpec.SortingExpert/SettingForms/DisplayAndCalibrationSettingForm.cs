using FigSpec.SortingExpert.Entities;
using Globalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class DisplayAndCalibrationSettingForm : BaseFormSetting
    {
        public DisplayAndCalibrationSettingForm()
        {
            InitializeComponent();
        }


        private void DisplayAndCalibrationSettingForm_Load(object sender, EventArgs e)
        {
            var scanSet = GlobalSettings.ApplySetting.scanSet;
            spEffectiveHour.EditValue = GlobalSettings.ApplySetting.CalibrationEffectiveHour;
            spStartSample.EditValue = GlobalSettings.ApplySetting.StartSample;
            spEndSample.EditValue = GlobalSettings.ApplySetting.EndSample;
            chkSaveDisplayArea.Checked = scanSet.SaveDisplayArea;
            checkSavesawimage.Checked = scanSet.SaveSawImgFlag;
            chkOnlyCacheDisplayArea.Checked = scanSet.OnlyCacheDisplayArea;

            if (File.Exists(scanSet.calibrationInfo.whiteRefFilePath))
            {
                labelControl1.Text = "白校准系数文件已存在".ToMultiLanguage();
                labelControl1.ForeColor = Color.White;
            }
            else
            {
                labelControl1.Text = "白校准系数文件不存在".ToMultiLanguage();
                labelControl1.ForeColor = Color.DarkRed;
            }

            if (File.Exists(scanSet.calibrationInfo.whiteRawFilePath))
            {
                labelControl2.Text = "白校准文件已存在".ToMultiLanguage();
                labelControl2.ForeColor = Color.White;
            }
            else
            {
                labelControl2.Text = "白校准文件不存在".ToMultiLanguage();
                labelControl2.ForeColor = Color.DarkRed;

            }

            if (File.Exists(scanSet.calibrationInfo.blackRawFilePath))
            {
                labelControl4.Text = "黑校准文件已存在".ToMultiLanguage();
                labelControl4.ForeColor = Color.White;

            }
            else
            {
                labelControl4.Text = "黑校准文件不存在".ToMultiLanguage();
                labelControl4.ForeColor = Color.DarkRed;

            }


        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (spStartSample.Value >= spEndSample.Value)
            {
                FormShowHelper.ShowMessage("像素显示区间设置异常，请重新设置".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            var set = GlobalSettings.ApplySetting;
            set.scanSet.SaveDisplayArea = chkSaveDisplayArea.Checked;
            set.scanSet.SaveSawImgFlag = checkSavesawimage.Checked;
            set.scanSet.OnlyCacheDisplayArea = chkOnlyCacheDisplayArea.Checked;
            set.CalibrationEffectiveHour = (float)spEffectiveHour.Value;
            set.StartSample = (int)spStartSample.Value;
            set.EndSample = (int)spEndSample.Value;
            GlobalSettings.ApplySetting = set;
            this.Close();
        }


        private void btnimportwhiteref_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Title = "请选择".ToMultiLanguage(),
                    Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls;"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.FileName != GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRefFilePath)
                    {
                        if (CalibrationInfo.LoadWhiteRefFile(dialog.FileName, out string result))
                        {
                            if (File.Exists(GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRefFilePath))
                            {
                                File.Move(GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRefFilePath, GlobalSettings.ApplyInfo.ApplyParamPath + "白校准系数文件".ToMultiLanguage() + Guid.NewGuid().ToString() + ".xlsx");
                            }
                            File.Copy(dialog.FileName, GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRefFilePath);
                            labelControl1.Text = "白校准系数文件已存在".ToMultiLanguage();
                            labelControl1.ForeColor = Color.White;

                        }
                        FormShowHelper.ShowMessage(result, "提示".ToMultiLanguage());
                    }
                }
            }
            catch (Exception ex)
            {
                FormShowHelper.ShowMessage(ex.ToString());

            }
        }

        private void btnimportwhiteraw_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Title = "请选择".ToMultiLanguage(),
                    Filter = "figspecwhite files (*.figspecwhite)|*.figspecwhite"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.FileName != GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRawFilePath)
                    {
                        if (CalibrationInfo.LoadWriteRawFile(dialog.FileName, out string result))
                        {
                            if (File.Exists(GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRawFilePath))
                            {
                                File.Move(GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRawFilePath, GlobalSettings.ApplySetting.FolderPathOfBrowse + "白校准文件".ToMultiLanguage() + Guid.NewGuid().ToString() + ".figspecwhite");
                            }
                            File.Copy(dialog.FileName, GlobalSettings.ApplySetting.scanSet.calibrationInfo.whiteRawFilePath);
                            labelControl2.Text = "白校准文件已存在".ToMultiLanguage();
                            labelControl2.ForeColor = Color.White;

                        }
                        FormShowHelper.ShowMessage(result, "提示".ToMultiLanguage());
                    }
                    
                }
            }
            catch (Exception ex)
            {
                FormShowHelper.ShowMessage(ex.ToString());

            }
            
        }


        private void btnimportblackraw_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    Title = "请选择".ToMultiLanguage(),
                    Filter = "figspecblack files (*.figspecblack)|*.figspecblack"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (dialog.FileName != GlobalSettings.ApplySetting.scanSet.calibrationInfo.blackRawFilePath)
                    {
                        if (CalibrationInfo.LoadBlackRawFile(dialog.FileName, out string result))
                        {

                            if (File.Exists(GlobalSettings.ApplySetting.scanSet.calibrationInfo.blackRawFilePath))
                            {
                                File.Move(GlobalSettings.ApplySetting.scanSet.calibrationInfo.blackRawFilePath, GlobalSettings.ApplySetting.FolderPathOfBrowse + "黑校准文件".ToMultiLanguage() + Guid.NewGuid().ToString() + ".figspecblack");
                            }
                            File.Copy(dialog.FileName, GlobalSettings.ApplySetting.scanSet.calibrationInfo.blackRawFilePath);
                            labelControl4.Text = "黑校准文件已存在".ToMultiLanguage();
                            labelControl4.ForeColor = Color.White;
                        }
                        FormShowHelper.ShowMessage(result, "提示".ToMultiLanguage());
                    }
                 
                }
            }
            catch (Exception ex )
            {
                FormShowHelper.ShowMessage(ex.ToString());
            }
        }
    }
}
