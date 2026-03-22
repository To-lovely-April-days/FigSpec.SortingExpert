using CHNSpec.Tools;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.VersionUpdate;
using Globalization;
using Hyperspectral.SpectralFile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert
{
    public partial class MainForm : BaseForm
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // 用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            this.KeyPreview = true; // 确保窗体可以接收按键事件
        }

        //当前显示在哪个界面
        private int whitchFace = 0;
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 检查是否按下了 Ctrl + Tab
            if (keyData == (Keys.Control | Keys.Tab))
            {
                // 处理 Ctrl + Tab 键组合的逻辑
                //MessageBox.Show("Ctrl + Tab pressed!");
                switch (whitchFace + 1)
                {
                    case 0:
                        labScan_Click(null, null);
                        break;
                    case 1:
                        labBrowse_Click(null, null);
                        break;
                    case 2:
                        labTran_Click(null, null);
                        break;
                    case 3:
                        labApps_Click(null, null);
                        break;
                    case 4:
                        labSorting_Click(null, null);
                        break;
                    case 5:
                        labEject_Click(null, null);
                        break;
                    default:
                        labScan_Click(null, null);
                        break;
                }
                return true; // 表示事件已被处理
            }


            // 其他键盘输入的默认处理
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            FormShowHelper.CloseLoadingForm();
            this.WindowState = FormWindowState.Maximized;
            //创建指定文件夹
            if (!Directory.Exists(GlobalSettings.ApplyInfo.ApplyParamPath))
                Directory.CreateDirectory(GlobalSettings.ApplyInfo.ApplyParamPath);
            InitMenuLanguage();
            labScan_Click(null, null);

            NotificationAction.Send2Sorting = (data) =>
            {
                labSorting_Click(null, null);
                formSorting.UpdateModel.Invoke(data);
            };

            NotificationAction.Send2OutlineSorting = (data) =>
            {
                labOutlineSorting_Click(null, null);
                formOutlineSorting.UpdateModel.Invoke(data);
            };

            NotificationAction.Send2Train = (path, isUpdateLabel) =>
            {
                if (GlobalSettings.ApplySetting.traingSet.spe == null || GlobalSettings.ApplySetting.traingSet.spe.SpePath != path)
                {
                    try
                    {
                        var spe = new SPE(path);
                        var datatype = spe.Hdr.DataType;
                        spe.SpeDispose();
                        if (datatype != 4)
                        {
                            FormShowHelper.ShowMessage("训练图片必须是反射率图片".ToMultiLanguage(), "提示".ToMultiLanguage());
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        FormShowHelper.ShowMessage("无法打开训练图片".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                }
               
                labTran_Click(null, null);
                formTrain.UpdateHypeImg.Invoke(path, isUpdateLabel);
            };

            if (Constant.HideOutlineFunc)
            {
                labOutlineSorting.Visible = false;
            }
            this.TopMost = true;
            this.TopMost = false;
        }

        private void InitMenuLanguage()
        {
            List<BarCheckItem> barItems = new List<BarCheckItem>();
            foreach (DictionaryEntry item in typeof(EnumLanguage).GetItems().ToMultiLanguage<EnumLanguage>())
            {
                barItems.Add(new BarCheckItem
                {
                    Name = "MenuLanguage" + item.Key,
                    Caption = item.Value.ToString(),
                    Tag = item.Key,
                    Checked = GlobalLanguage.Language == (EnumLanguage)item.Key,
                    CheckStyle = BarCheckStyles.Radio,
                    GroupIndex = 2111301848
                });
            }
            foreach (var item in barItems)
            {
                item.CheckedChanged += (s, e) =>
                {
                    if (FormShowHelper.ShowMessage("切换语言将重启软件，确定切换吗？".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) == DialogResult.OK)
                    {
                        Properties.Settings.Default.Language = (int)item.Tag; //修改配置文件
                        Properties.Settings.Default.Save();
                        System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    }
                };
            }

            barItemLanguage.AddItems(barItems.ToArray());
        }

        /// <summary>
        /// 设置背景颜色
        /// </summary>
        private void SetBackColor(string name)
        {
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                LabelControl label = control as LabelControl;
                if (label.Name == name)
                {
                    //label.Appearance.BackColor = ColorTranslator.FromHtml("#ffffff");//#EFF0F2
                    //label.Appearance.BackColor = label.AppearanceHovered.BackColor;//#EFF0F2
                    //label.Appearance.BorderColor = ColorTranslator.FromHtml("#EFF0F2");
                    label.ForeColor = Color.Orange;
                    this.Text = label.Text + " - SortingExpert";
                }
                else
                {
                    //label.Appearance.BackColor = this.BackColor;
                    //label.Appearance.BorderColor = ColorTranslator.FromHtml("#B4B4B4");
                    label.ForeColor = Color.White;
                }
            }
        }

        private void AddForm2Panel(Form form, PanelControl panel)
        {
            panel.Controls.Clear();
            form.TopLevel = false;
            form.ShowIcon = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            panel.Controls.Add(form);
            form.Show();

            GlobalSettings.CurrentFormName = form.Name;
        }

        private BrowseForm formBrowse = new BrowseForm();

        private void labBrowse_Click(object sender, EventArgs e)
        {
            whitchFace = 1;
            SetBackColor(labBrowse.Name);
            if (panelControl2.Controls.Count == 0 || panelControl2.Controls[0].Name != formBrowse.Name)
            {
                AddForm2Panel(formBrowse, panelControl2);
            }
        }

        private TrainForm formTrain = new TrainForm();

        private void labTran_Click(object sender, EventArgs e)
        {
            whitchFace = 2;
            SetBackColor(labTran.Name);
            if (panelControl2.Controls.Count == 0 || panelControl2.Controls[0].Name != formTrain.Name)
            {
                AddForm2Panel(formTrain, panelControl2);
            }
        }

        private AppsForm formApps = new AppsForm();

        private void labApps_Click(object sender, EventArgs e)
        {
            whitchFace = 3;
            SetBackColor(labApps.Name);
            if (panelControl2.Controls.Count == 0 || panelControl2.Controls[0].Name != formApps.Name)
            {
                if (formApps != null)
                {
                    formApps.ShowNodes();
                }
                AddForm2Panel(formApps, panelControl2);
            }
        }

        private SortingForm formSorting = new SortingForm();

        private void labSorting_Click(object sender, EventArgs e)
        {
            whitchFace = 4;
            SetBackColor(labSorting.Name);
            if (panelControl2.Controls.Count == 0 || panelControl2.Controls[0].Name != formSorting.Name || (panelControl2.Controls[0] as SortingForm).IsOutlineSorting)
            {
                AddForm2Panel(formSorting, panelControl2);
            }
        }

        private SortingForm formOutlineSorting = new SortingForm();

        private void labOutlineSorting_Click(object sender, EventArgs e)
        {
            formOutlineSorting.IsOutlineSorting = true;
            SetBackColor(labOutlineSorting.Name);
            if (panelControl2.Controls.Count == 0 || panelControl2.Controls[0].Name != formOutlineSorting.Name || !(panelControl2.Controls[0] as SortingForm).IsOutlineSorting)
            {
                AddForm2Panel(formOutlineSorting, panelControl2);
            }
        }

        private EjectForm formEject = new EjectForm();

        private void labEject_Click(object sender, EventArgs e)
        {
            whitchFace = 5;
            SetBackColor(labEject.Name);
            if (panelControl2.Controls.Count == 0 || panelControl2.Controls[0].Name != formEject.Name)
            {
                AddForm2Panel(formEject, panelControl2);
            }
        }

        private void lblMenu_Click(object sender, EventArgs e)
        {
            popupMenu1.ShowPopup(MousePosition);
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalSettings.ApplySetting.Save();
            if (ScanParaMeter.camera != null)
            {
                ScanParaMeter.camera.CloseCamera();
            }
        }
        /// <summary>
        /// 关于
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barBtnAbout_ItemClick(object sender, ItemClickEventArgs e)
        {
            AboutForm form = new AboutForm();
            FormShowHelper.ShowDialog(form);
        }

        private ScanForm formScan = new ScanForm();
        /// <summary>
        /// Scan界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void labScan_Click(object sender, EventArgs e)
        {
            whitchFace = 0;
            SetBackColor(labScan.Name);
            if (panelControl2.Controls.Count == 0 || panelControl2.Controls[0].Name != formScan.Name)
            {
                AddForm2Panel(formScan, panelControl2);
            }
        }
        /// <summary>
        /// 导入相机配置文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonImportCamFig_ItemClick(object sender, ItemClickEventArgs e)
        {
            ScanParaMeter.ImportCameraSetting();
        }
        /// <summary>
        /// 使用向导
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItemGuide_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ScanParaMeter.camera != null && ScanParaMeter.camera.IsGrab)
            {
                FormShowHelper.ShowMessage("请先停止正在运行的采集功能".ToMultiLanguage());
                return;
            }

            UserGuideForm form = new UserGuideForm();
            FormShowHelper.ShowDialog(form);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            string[] files = ScanParaMeter.GetCameraSettingFiles();
            if(files.Length == 0)
            {
                UserGuideForm form = new UserGuideForm();
                FormShowHelper.ShowDialog(form);
            }

        }
    }
}