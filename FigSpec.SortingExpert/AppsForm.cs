using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.ModelFiles;
using FigSpec.SortingExpert.SettingForms;
using FigSpec.SortingExpert.TypeExtensions;
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

namespace FigSpec.SortingExpert
{
    public partial class AppsForm : BaseFormInside
    {
        /// <summary>
        /// 模型来源：
        /// 扫描固定文件夹（指定高光谱源文件路径，不进行拷贝）
        /// 下载云端共享文件夹（按照公司固定下载路径）
        /// </summary>
        private Model model;
        private List<ModelsDisplayCell> localModels = new List<ModelsDisplayCell>();  //本地模型
        private List<ModelsDisplayCell> cloudModels = new List<ModelsDisplayCell>();   //云端模型
        //模型的数据源
        private List<ModelsDisplayCell> modelsDisplayCell = new List<ModelsDisplayCell>();


        // 监视本地模型文件夹
        private FileSystemWatcher watcher = new FileSystemWatcher();
        //打印报表明细
        private XtraReportDetail xtraReportDetail = null;

        public AppsForm()
        {
            InitializeComponent();
            
        }

        private void AppsForm_Load(object sender, EventArgs e)
        {
            var set = GlobalSettings.ApplySetting;
            string path = set.FolderPathOfBrowse;
            if (!Directory.Exists(path))
            {
                path = set.FolderPathOfBrowse = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                GlobalSettings.ApplySetting = set;
            }
            FolderWatcher(path);
            ChangeFolder(path);


            InitReport();
            InitTreeList();
            NotificationAction.FolderPathOfBrowseChanged = ChangeFolderAction;

            if (Constant.HideOutlineFunc)
            {
                btnSent2OutlineSorting.Visible = false;
                barBtnSendToOutlineSorting.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }
        }

        public void ShowNodes()
        {
            treeList1.ExpandAll();
        }

        /// <summary>
        /// 本地文件夹监视
        /// </summary>
        /// <param name="path"></param>
        private void FolderWatcher(string path)
        {
            watcher.Path = path;

            watcher.NotifyFilter = NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;

            // Only watch text files.
            watcher.Filter = "*.fsmodel";

            // Add event handlers.
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnChanged;

            // Start monitoring.
            watcher.EnableRaisingEvents = true;
        }
        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            ChangeFolderAction();
        }


        private void ChangeFolderAction()
        {
            if (!this.IsHandleCreated)
                return;
            this.Invoke(new Action(() =>
            {
                ChangeFolder(GlobalSettings.ApplySetting.FolderPathOfBrowse);
            }));
        }


        private void ChangeFolder(string path)
        {
            path = Path.Combine(path, @"Models");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            watcher.Path = path;
            string[] needFiles = Directory.GetFiles(path, "*.fsmodel");
            localModels.Clear();
            foreach (string file in needFiles)
            {
                if (File.Exists(file))
                {
                    var filename = Path.GetFileNameWithoutExtension(file);
                    localModels.Add(new ModelsDisplayCell() {  name = filename, LocalPath = file });
                }
            }
            UpdateModelsSet();
        }

        private int cellFocusedID = 0;
        private void UpdateModelsSet()
        {
            modelsDisplayCell.Clear();
            int sortid = 0;
            modelsDisplayCell.Add(new ModelsDisplayCell()
            {
                ID = sortid++,
                RegionID = -1,
                CellType = EnumModelDisplayType.LocalModelRoot,
                name = "本地模型文件".ToMultiLanguage()
            });
            foreach (var item in localModels)
            {
                modelsDisplayCell.Add(new ModelsDisplayCell()
                {
                    ID = sortid++,
                    RegionID = 0,
                    CellType = EnumModelDisplayType.LocalModel,
                    name = item.name,
                    LocalPath = item.LocalPath
                });
            }
            if (cloudModels.Count > 0)
            {
                var cloudRoot = new ModelsDisplayCell()
                {
                    ID = sortid++,
                    RegionID = -1,
                    CellType = EnumModelDisplayType.CloudModelRoot,
                    name = "云端模型文件".ToMultiLanguage()
                };
                modelsDisplayCell.Add(cloudRoot);
                foreach (var item in cloudModels)
                {
                    modelsDisplayCell.Add(new ModelsDisplayCell()
                    {
                        ID = sortid++,
                        RegionID = cloudRoot.ID,
                        CellType = EnumModelDisplayType.CloudModel,
                        name = item.name,
                    });
                }
            }

            if (cellFocusedID >= 0 && cellFocusedID < sortid)
            {
                //选中指定行
                var selectedNode = treeList1.GetNodeList().Find(i => i["ID"].Equals(cellFocusedID));
                treeList1.FocusedNode = selectedNode;
            }
            treeList1.RefreshDataSource();
        }


        private void InitTreeList()
        {
            treeList1.KeyFieldName = "ID";
            treeList1.ParentFieldName = "RegionID";
            //Allow the treelist to create columns bound to the fields the KeyFieldName and ParentFieldName properties specify.
            treeList1.OptionsBehavior.PopulateServiceColumns = true;
            // 设置显示网格线
            treeList1.OptionsView.ShowHorzLines = false;
            treeList1.OptionsView.ShowVertLines = false;
            // 设置节点不可编辑
            treeList1.OptionsBehavior.Editable = false;
            // 更改背景颜色
            treeList1.Appearance.Empty.BackColor = System.Drawing.Color.Gray;
            treeList1.Appearance.Row.BackColor = System.Drawing.Color.Gray;
            treeList1.Appearance.Row.ForeColor = System.Drawing.Color.White;
            treeList1.OptionsSelection.EnableAppearanceFocusedCell = false;
            treeList1.OptionsView.ShowIndicator = false;
            treeList1.DataSource = modelsDisplayCell;
            ShowNodes();
        }
   
        private void InitReport()
        {
            xtraReportDetail = new XtraReportDetail();
            ShowReport();
        }
        private void ShowReport()
        {
            xtraReportDetail.InitData(model);
            xtraReportDetail.CreateDocument(true);
            documentViewer1.DocumentSource = xtraReportDetail;
        }


        private void treeList1_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            if (treeList1.Nodes == null || treeList1.Nodes.Count <= 0)
            {
                return;
            }
            ModelsDisplayCell focusModelCell = treeList1.GetFocusedRow() as ModelsDisplayCell;

            if (focusModelCell != null && focusModelCell.CellType == EnumModelDisplayType.LocalModel)
            {
                //panelControl3.Visible = true;
                panelControl4.Visible = true;
                panelControl5.Visible = true;
                var set = GlobalSettings.ApplySetting;
                var path = Path.Combine(set.FolderPathOfBrowse, @"Models\") + focusModelCell.name + ".fsmodel";
                model = new Model();
                Model.TryRead(path, ref model);
                model.ModelFileName = focusModelCell.name;
                txtModelName.Text = model.name;
                txtSensor.Text = model.FxModel;
                txtVersion.Text = model.version;
                txtRemark.Text = model.remark;
                ShowReport();
            }
            else
            {
                //panelControl3.Visible = false;
                panelControl4.Visible = false;
                panelControl5.Visible = false;
            }
        }

        private void btnSent2Sorting_Click(object sender, EventArgs e)
        {
            if (model != null)
            {
                if (GlobalSettings.ApplySetting.runingApp == "sort")
                {
                    return;
                }
                NotificationAction.Send2Sorting?.Invoke(model);
            }
        }

        private void btnSent2OutlineSorting_Click(object sender, EventArgs e)
        {
            if (model != null)
            {
                if (GlobalSettings.ApplySetting.runingApp == "sort")
                {
                    return;
                }
                NotificationAction.Send2OutlineSorting?.Invoke(model);
            }
        }

        private void barBtnDeleteModel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (treeList1.Nodes == null || treeList1.Nodes.Count <= 0)
            {
                return;
            }
            ModelsDisplayCell models = treeList1.GetFocusedRow() as ModelsDisplayCell;
            if (models != null && models.CellType == EnumModelDisplayType.LocalModel)
            {
                try
                {
                    cellFocusedID = models.ID - 1;
                    if (!string.IsNullOrEmpty(models.LocalPath) && File.Exists(models.LocalPath))
                        File.Delete(models.LocalPath);

                }
                catch (Exception)
                {

                }
            }
        }

        private void barBtnSendToSorting_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            btnSent2Sorting_Click(null, null);
        }

        private void barBtnSendToOutlineSorting_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            btnSent2OutlineSorting_Click(null, null);
        }

        private void barBtnPrint_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            xtraReportDetail.PrintDialog();
        }

        private void treeList1_MouseUp(object sender, MouseEventArgs e)
        {
            var hitInfo = treeList1.CalcHitInfo(e.Location);
            if (hitInfo.InRow && e.Button == MouseButtons.Right)
            {
                treeList1.FocusedNode = hitInfo.Node;
                ModelsDisplayCell cell = treeList1.GetFocusedRow() as ModelsDisplayCell;
                if (cell == null || !(cell.CellType == EnumModelDisplayType.LocalModel || cell.CellType == EnumModelDisplayType.CloudModel))
                    return;
                popupMenu1.ShowPopup(MousePosition);
            }
        }

        private void documentViewer1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                popupMenu2.ShowPopup(MousePosition);
            }
        }

        private void barBtnFileRename_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (treeList1.Nodes == null || treeList1.Nodes.Count <= 0)
            {
                return;
            }
            ModelsDisplayCell models = treeList1.GetFocusedRow() as ModelsDisplayCell;
            if (models != null && models.CellType == EnumModelDisplayType.LocalModel)
            {
                try
                {
                    watcher.EnableRaisingEvents = false;
                    var form = new FromModelFileRename(models.LocalPath);
                    if (FormShowHelper.ShowDialog(form) == DialogResult.OK)
                    {
                        models.name = form.FileName;
                        models.LocalPath = form.FullPath;
                        treeList1.Refresh();
                    }
                    watcher.EnableRaisingEvents = true;
                }
                catch (Exception)
                {

                }
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            if (treeList1.Nodes == null || treeList1.Nodes.Count <= 0)
            {
                FormShowHelper.ShowMessage("未选中模型!".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            ModelsDisplayCell focusModelCell = treeList1.GetFocusedRow() as ModelsDisplayCell;
            if (focusModelCell == null)
            {
                FormShowHelper.ShowMessage("未选中模型!".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            if (model == null)
            {
                FormShowHelper.ShowMessage("模型未加载!".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            watcher.EnableRaisingEvents = false;
            var set = GlobalSettings.ApplySetting;
            var path = Path.Combine(set.FolderPathOfBrowse, @"Models\");
            model.name = txtModelName.Text;
            //model.FxModel =txtSensor.Text;
            //model.version =txtVersion.Text;
            model.remark = txtRemark.Text;

            string fileName = focusModelCell.name + ".fsmodel";
            bool isSuc = model.TrySave(path, fileName);
            watcher.EnableRaisingEvents = true;
            if (!isSuc)
            {
                FormShowHelper.ShowMessage("修改失败!".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            FormShowHelper.ShowMessage("修改成功".ToMultiLanguage(), "提示".ToMultiLanguage());

        }

        private void btnSent2Train_Click(object sender, EventArgs e)
        {
            SendModelToTrain();
        }

        private void barBtnSendToTrain_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SendModelToTrain();
        }
        private void SendModelToTrain()
        {
            var set = GlobalSettings.ApplySetting;
            set.traingSet.models.Add(model);
            FormShowHelper.ShowMessage("发送成功".ToMultiLanguage(), "提示".ToMultiLanguage());
            NotificationAction.SendModel2Train?.Invoke();
        }


    }
}
