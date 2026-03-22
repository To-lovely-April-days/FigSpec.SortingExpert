using Cameras;
using DevExpress.XtraEditors.Repository;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.ModelFiles;
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
    public partial class CameraSettingForm : BaseFormSetting
    {
        bool isScan = true;
        bool isOrginal = true;

        CameraSetting cameraSetting => isScan ? GlobalSettings.ApplySetting.ScanCameraSetting : GlobalSettings.ApplySetting.SortCameraSetting;

        public CameraSettingForm(bool isScan, bool isOrginal, Model model)
        {
            this.isScan = isScan;
            this.isOrginal = isOrginal;
            if (isScan)
            {
                ScanParaMeter.camera.SetRunTypeAll();
            }
            else
            {
                ScanParaMeter.SetRunTypeBank(model);
            }
            ScanParaMeter.RefreshFrameRate(isScan);

            InitializeComponent();
        }

        private void rangeTBC1_EditValueChanged(object sender, EventArgs e)
        {
            rangeTBC1.Value = new TrackBarRange() { Minimum = 0, Maximum = rangeTBC1.Value.Maximum };
            spinImgRange1.EditValueChanged -= spinImgRange1_EditValueChanged;
            spinImgRange1.EditValue = rangeTBC1.Value.Maximum;
            spinImgRange1.EditValueChanged += spinImgRange1_EditValueChanged;
        }

        private void rangeTBC2_EditValueChanged(object sender, EventArgs e)
        {
            rangeTBC2.Value = new TrackBarRange() { Minimum = 0, Maximum = rangeTBC2.Value.Maximum };
            spinImgRange2.EditValueChanged -= spinImgRange2_EditValueChanged;
            spinImgRange2.EditValue = rangeTBC2.Value.Maximum;
            spinImgRange2.EditValueChanged += spinImgRange2_EditValueChanged;
        }

        private void rangeTBC3_EditValueChanged(object sender, EventArgs e)
        {
            rangeTBC3.Value = new TrackBarRange() { Minimum = 0, Maximum = rangeTBC3.Value.Maximum };
            spinImgRange3.EditValueChanged -= spinImgRange3_EditValueChanged;
            spinImgRange3.EditValue = rangeTBC3.Value.Maximum;
            spinImgRange3.EditValueChanged += spinImgRange3_EditValueChanged;
        }
        private void spinImgRange1_EditValueChanged(object sender, EventArgs e)
        {
            rangeTBC1.EditValueChanged -= rangeTBC1_EditValueChanged;
            rangeTBC1.Value = new TrackBarRange() { Minimum = 0, Maximum = Convert.ToInt32(spinImgRange1.EditValue) };
            rangeTBC1.EditValueChanged += rangeTBC1_EditValueChanged;
        }

        private void spinImgRange2_EditValueChanged(object sender, EventArgs e)
        {
            rangeTBC2.EditValueChanged -= rangeTBC2_EditValueChanged;
            rangeTBC2.Value = new TrackBarRange() { Minimum = 0, Maximum = Convert.ToInt32(spinImgRange2.EditValue) };
            rangeTBC2.EditValueChanged += rangeTBC2_EditValueChanged;
        }

        private void spinImgRange3_EditValueChanged(object sender, EventArgs e)
        {
            rangeTBC3.EditValueChanged -= rangeTBC3_EditValueChanged;
            rangeTBC3.Value = new TrackBarRange() { Minimum = 0, Maximum = Convert.ToInt32(spinImgRange3.EditValue) };
            rangeTBC3.EditValueChanged += rangeTBC3_EditValueChanged;
        }

        private void hlLabelResetRange_Click(object sender, EventArgs e)
        {
            int tr = isOrginal ? 10000 : 100;
            int tg = isOrginal ? 10000 : 100;
            int tb = isOrginal ? 10000 : 100;

            if (isOrginal)
            {
                cameraSetting.ImgOrginThresholdR = tr;
                cameraSetting.ImgOrginThresholdG = tg;
                cameraSetting.ImgOrginThresholdB = tb;
            }
            else
            {
                cameraSetting.ImgThresholdR = tr;
                cameraSetting.ImgThresholdG = tg;
                cameraSetting.ImgThresholdB = tb;
            }
           
            GlobalSettings.ApplySetting.Save();

            spinImgRange1.EditValue = tr;
            spinImgRange2.EditValue = tg;
            spinImgRange3.EditValue = tb;

        }

        private void hlLabelResetRGB_Click(object sender, EventArgs e)
        {
            //更新显示波段和阈值
            cameraSetting.ImgFR = 1252.98f;
            cameraSetting.ImgFG = 1404.30f;
            cameraSetting.ImgFB = 1603.92f;
            GlobalSettings.ApplySetting.Save();

            InitDisplayImgRGB();
        }


        private void InitDisplayImgRGB()
        {
            ScanParaMeter.RefreshCameraImgRGB(isScan);

            List<BaseDictionary> baseDictionaries1 = new List<BaseDictionary>();
            for (int i = 0; i < ScanParaMeter.parminfo.SpectralChannelWavelength.Length; i++)
            {
                baseDictionaries1.Add(new BaseDictionary()
                {
                    Index = i,
                    Code = i.ToString(),
                    Value = ScanParaMeter.parminfo.SpectralChannelWavelength[i].ToString(),
                    Desc = i + ": " + ScanParaMeter.parminfo.SpectralChannelWavelength[i].ToString("F2"),
                });
            }
            cmbRed.Properties.DataSource = baseDictionaries1;
            cmbRed.EditValue = cameraSetting.ImgFR;
            cmbRed.ItemIndex = cameraSetting.ImgR;
            cmbGreen.Properties.DataSource = baseDictionaries1;
            cmbGreen.EditValue = cameraSetting.ImgFG;
            cmbGreen.ItemIndex = cameraSetting.ImgG;
            cmbBlue.Properties.DataSource = baseDictionaries1;
            cmbBlue.EditValue = cameraSetting.ImgFB;
            cmbBlue.ItemIndex = cameraSetting.ImgB;
        }

        private void InitDisplayModel()
        {
            InitDisplayImgRGB();

            int momo = isOrginal ? 16384 : 100;
            int momoTickFrequency = 1;

            rangeTBC1.Properties.Maximum = momo;
            rangeTBC2.Properties.Maximum = momo;
            rangeTBC3.Properties.Maximum = momo;


            rangeTBC1.Properties.Minimum = 1;
            rangeTBC2.Properties.Minimum = 1;
            rangeTBC3.Properties.Minimum = 1;

            rangeTBC1.Properties.TickFrequency = momoTickFrequency;
            rangeTBC2.Properties.TickFrequency = momoTickFrequency;
            rangeTBC3.Properties.TickFrequency = momoTickFrequency;

            spinImgRange1.Properties.MaxValue = momo;
            spinImgRange2.Properties.MaxValue = momo;
            spinImgRange3.Properties.MaxValue = momo;
            if (isOrginal)
            {
                spinImgRange1.EditValue = cameraSetting.ImgOrginThresholdR > momo ? momo : (int)cameraSetting.ImgOrginThresholdR;
                spinImgRange2.EditValue = cameraSetting.ImgOrginThresholdG > momo ? momo : (int)cameraSetting.ImgOrginThresholdG;
                spinImgRange3.EditValue = cameraSetting.ImgOrginThresholdB > momo ? momo : (int)cameraSetting.ImgOrginThresholdB;
            }
            else
            {
                spinImgRange1.EditValue = cameraSetting.ImgThresholdR > momo ? momo : cameraSetting.ImgThresholdR;
                spinImgRange2.EditValue = cameraSetting.ImgThresholdG > momo ? momo : cameraSetting.ImgThresholdG;
                spinImgRange3.EditValue = cameraSetting.ImgThresholdB > momo ? momo : cameraSetting.ImgThresholdB;
            }


        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            GlobalClass globalClass = new GlobalClass();
            long RemainingMemory = globalClass.GetRemainingMemory();
            if (RemainingMemory != 0)
            {
                //RemainingMemory -= 100 * 1024 * 1024;//预留100M的内存防止采集期间别的程序占用内存
                uint wavelength = ScanParaMeter.parminfo.SpectralChannelNumCur;
                uint samples = ScanParaMeter.parminfo.SpatialPixelNumCur;
                int datasize = ScanParaMeter.parminfo.PixelFormatCur == EnumPixelFormat.Mono8 ? 1 : 2;
                long Framesize = wavelength * samples * datasize;//一帧数据大小
                var AllowFrameNums = RemainingMemory / Framesize;//可以采的帧数
                if ((int)spinEditDisplayFrame.Value > AllowFrameNums)
                {
                    FormShowHelper.ShowMessage(string.Format("当前设备可用内存为{0}MB,一次最多可采集{1}帧".ToMultiLanguage(), RemainingMemory / 1024 / 1024, AllowFrameNums), "提示".ToMultiLanguage());
                    return;
                }
            }
            else
            {
                if ((int)spinEditDisplayFrame.Value > 100000)
                {
                    FormShowHelper.ShowMessage("无法获取设备可用内存，采集过程中会不断增加占用内存，请视情况停止采集".ToMultiLanguage(), "提示".ToMultiLanguage());
                }
            }


            cameraSetting.ExposureTime = (int)spinEdit1.Value;
            cameraSetting.FrameRate = (int)spinEdit2.Value;

            cameraSetting.Gain = (cmbGain.GetSelectedValue<int>() ?? 0);
            cameraSetting.FlipXFlag = checkFlipX.Checked;
            cameraSetting.DisplayFrameNum = (int)spinEditDisplayFrame.Value;
            cameraSetting.TriggerMode = (cmbTriggerMode.GetSelectedValue<int>() ?? 0);

            cameraSetting.ImgR = cmbRed.ItemIndex;
            cameraSetting.ImgG = cmbGreen.ItemIndex;
            cameraSetting.ImgB = cmbBlue.ItemIndex;
            cameraSetting.ImgFR = ScanParaMeter.parminfo.SpectralChannelWavelength[cmbRed.ItemIndex];
            cameraSetting.ImgFG = ScanParaMeter.parminfo.SpectralChannelWavelength[cmbGreen.ItemIndex];
            cameraSetting.ImgFB = ScanParaMeter.parminfo.SpectralChannelWavelength[cmbBlue.ItemIndex];

            int tr = (int)spinImgRange1.Value;
            int tg = (int)spinImgRange2.Value;
            int tb = (int)spinImgRange3.Value;
            if (isOrginal)
            {
                cameraSetting.ImgOrginThresholdR = tr;
                cameraSetting.ImgOrginThresholdG = tg;
                cameraSetting.ImgOrginThresholdB = tb;
            }
            else
            {
                cameraSetting.ImgThresholdR = tr;
                cameraSetting.ImgThresholdG = tg;
                cameraSetting.ImgThresholdB = tb;
            }

            GlobalSettings.ApplySetting.Save();

            if (ScanParaMeter.camera != null)
            {
                //设置曝光时间
                if (ScanParaMeter.camera.SetExposureTime(cameraSetting.ExposureTime))
                    Console.WriteLine("设置曝光时间成功");
                else
                    Console.WriteLine("设置曝光时间失败");

                if (ScanParaMeter.camera.SetTriggerMode((EnumTriggerMode)cameraSetting.TriggerMode))
                    Console.WriteLine("设置触发模式成功");
                else
                    Console.WriteLine("设置触发模式失败");

                ScanParaMeter.parminfo = ScanParaMeter.camera.GetParmInfo();
                if (ScanParaMeter.camera.SetFrameRate((int)spinEdit2.Value))
                    Console.WriteLine("设置帧频成功");
                else
                    Console.WriteLine("设置帧频失败");
                //0高增益 1 中 2 低
                if (ScanParaMeter.camera.SetGain(cameraSetting.Gain))
                    Console.WriteLine("设置增益成功");
                else
                    Console.WriteLine("设置增益失败");

                //if (ScanParaMeter.camera.SetReverseSpatialPixel(cameraSetting.FlipXFlag))
                //    Console.WriteLine("设置镜像采集成功");
                //else
                //    Console.WriteLine("设置镜像采集失败");
            }
            this.Close();
        }

        private void btnreadmaxFrameRate_Click(object sender, EventArgs e)
        {
            if (ScanParaMeter.camera == null)
            {
                FormShowHelper.ShowMessage("请先连接相机".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }

            cameraSetting.ExposureTime = (int)spinEdit1.Value;

            //设置曝光时间
            if (ScanParaMeter.camera.SetExposureTime(cameraSetting.ExposureTime))
                Console.WriteLine("设置曝光时间成功");
            else 
                Console.WriteLine("设置曝光时间失败");

            ScanParaMeter.parminfo = ScanParaMeter.camera.GetParmInfo();
            spinEdit2.Value = (int)ScanParaMeter.parminfo.FrameRateMax;
        }

        private void CameraSettingForm_Load(object sender, EventArgs e)
        {
            cmbGain.BindComboBoxItem(typeof(EnumGain).GetValueTextItems());
            cmbGain.SetSelectedValue(cameraSetting.Gain);

            cmbTriggerMode.BindComboBoxItem(typeof(EnumTriggerMode).GetValueTextItems());
            cmbTriggerMode.SetSelectedValue(cameraSetting.TriggerMode);

            spinEdit1.Value = cameraSetting.ExposureTime;
            spinEdit2.Value = cameraSetting.FrameRate;
            InitDisplayModel();
            spinEditDisplayFrame.Value = cameraSetting.DisplayFrameNum;
            checkFlipX.Checked = cameraSetting.FlipXFlag;
        }

        private void sortingBaseButton1_Click(object sender, EventArgs e)
        {
            GlobalClass globalClass = new GlobalClass();
            long RemainingMemory = globalClass.GetRemainingMemory();
            if (RemainingMemory != 0)
            {
                //RemainingMemory -= 100 * 1024 * 1024;//预留100M的内存防止采集期间别的程序占用内存
                uint wavelength = ScanParaMeter.parminfo.SpectralChannelNumCur;
                uint samples = ScanParaMeter.parminfo.SpatialPixelNumCur;
                int datasize = ScanParaMeter.parminfo.PixelFormatCur == EnumPixelFormat.Mono8 ? 1 : 2;
                long Framesize = wavelength * samples * datasize;//一帧数据大小
                var AllowFrameNums = RemainingMemory / Framesize;//可以采的帧数
                spinEditDisplayFrame.Value = AllowFrameNums;

            }
            else
            {
                FormShowHelper.ShowMessage("无法获取设备可用内存，采集过程中会不断增加占用内存，请视情况停止采集".ToMultiLanguage(), "提示".ToMultiLanguage());
            }
        }
    }
}
