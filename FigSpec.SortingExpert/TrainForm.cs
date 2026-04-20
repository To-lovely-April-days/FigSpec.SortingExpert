using DevExpress.XtraBars;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.Cuda;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.ModelFiles;
using FigSpec.SortingExpert.SettingForms;
using FigSpec.SortingExpert.Tools;
using Globalization;
using ImgView;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hyperspectral.SpectralFile;
using Hyperspectral.SpectralProc;
using Hyperspectral.Images;
using Hyperspectral.Tools;
using CHNSpec.Tools;
using FigSpec.SortingExpert.Algorithm;
using DevExpress.XtraTreeList.Nodes;

namespace FigSpec.SortingExpert
{
    public partial class TrainForm : BaseFormInside
    {/// <summary>
     /// 下拉框数据源项
     /// </summary>
        private class UnifyTargetItem
        {
            public int ClassId { get; set; }  // -1 = 自动多数投票
            public string Display { get; set; }
            public override string ToString() => Display;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // 用双缓冲绘制窗口的所有子控件
                return cp;
            }
        }

        #region 全局变量

        /// <summary>
        /// spe相关
        /// </summary>
        private SPE spe
        {
            get => GlobalSettings.ApplySetting.traingSet.spe;
            set => GlobalSettings.ApplySetting.traingSet.spe = value;
        }
        private SpectraOfSPE spectraOfSPE;
        private string speHdrGuid;

        /// <summary>
        /// 图像的字节数组
        /// </summary>
        private byte[] img;
        /// <summary>
        /// 图片处理进度
        /// </summary>
        private float progress;
        /// <summary>
        /// 验证的预览图片数据
        /// </summary>
        private byte[][] pridictionImage;
        /// <summary>
        /// pridictionImage 的原始备份(统一颜色功能用,每次聚合都从这个备份开始)
        /// </summary>
        private byte[][] pridictionImageOriginal;
        /// <summary>
        /// 标记"统一颜色"控件是否已被激活(每次训练/应用模型重置)
        /// </summary>
        private bool unifyControlsActivated = false;
        /// <summary>
        /// 标记:当前正在从模型读取设置到控件(此时不要触发写回和刷新)
        /// </summary>
        private bool isLoadingUnifySettings = false;
        /// <summary>
        /// 验证显示预览图片定时器
        /// </summary>
        private System.Windows.Forms.Timer pridictionImageViewTimer;
        /// <summary>
        /// 显示原始高光谱图像用的定时器
        /// </summary>
        private System.Windows.Forms.Timer imageViewTimer;
        /// <summary>
        /// 显示当前运算核心定时器
        /// </summary>
        private System.Windows.Forms.Timer gpuUsedTimer;
        /// <summary>
        /// 当前模型
        /// </summary>
        private Model model;
        /// <summary>
        /// 帮助类
        /// </summary>
        private TrainFormHelper helper = new TrainFormHelper();
        /// <summary>
        /// 报表数据
        /// </summary>
        private XtraReportDetail xtraReportDetail = null;
        /// <summary>
        /// 标签和模型树结构数据源
        /// </summary>
        private List<TrainSetDisplayCell> trainSetDisplays = new List<TrainSetDisplayCell>();
        /// <summary>
        /// 当前的所有轮廓点
        /// </summary>
        private ContourInfo contourInfo = null;


        /// <summary>
        /// 更新初始图片
        /// </summary>
        public Action<string, bool> UpdateHypeImg;

        #endregion

        #region 构造函数和初始化等

        public TrainForm()
        {
            InitializeComponent();
            //清空标签
            GlobalSettings.ApplySetting.traingSet.label.selections.Clear();
            spectraOfSPE = SpectraFactory.SpectraOfSPE(EnumInterleave.bil);

            //加载初始图片
            UpdateHypeImg = UpdateHypeImgPerform;

            NotificationAction.SendModel2Train = () =>
            {
                UpdateTrainSet();
            };

            InitTreeList();
            InitPropertyGridControl();
            InitReport();
            InitImageViewTool();

            //定时刷新初始图片
            imageViewTimer = new System.Windows.Forms.Timer()
            {
                Interval = 200,
                Enabled = false
            };
            imageViewTimer.Tick += ImageViewTimer_Tick;

            //定时显示验证图片
            pridictionImageViewTimer = new System.Windows.Forms.Timer()
            {
                Interval = 200,
                Enabled = false
            };
            pridictionImageViewTimer.Tick += PridictionImageViewTimer_Tick;

            //GPU名称
            gpuUsedTimer = new System.Windows.Forms.Timer()
            {
                Interval = 2 * 1000,
                Enabled = true
            };
            gpuUsedTimer.Tick += GpuUsedTimer_Tick;
            gpuUsedTimer.Start();

            //默认显示空白部分
            navigationFrame1.SelectedPageIndex = 3;

            //隐藏轮廓功能
            if (Constant.HideOutlineFunc)
            {
                btnExportCsv.Visible = false;
                barBtnSend2OutlineSorting.Visibility = BarItemVisibility.Never;
            }

            // ===== 新增:默认禁用"统一颜色"相关控件 =====
            SetUnifyControlsEnabled(false);
        }

        private void TrainForm_Load(object sender, EventArgs e)
        {
            List<ValueTextItem<int>> lst = typeof(ImgView.SelectionType).GetValueTextItems();
            var barItemList = new List<BarButtonItem>();

            //右键按钮
            BarButtonItem barButtonItem = new BarButtonItem();
            barButtonItem.Caption = "清空标签".ToMultiLanguage();
            barButtonItem.Tag = "";
            barButtonItem.ItemClick += btnClearClass_ItemClick;
            barButtonItem.Visibility = BarItemVisibility.Always;
            barItemList.Add(barButtonItem);
            foreach (var item in lst)
            {
                if (item.Value == (int)SelectionType.None || item.Value == (int)SelectionType.魔法棒 || item.Value == (int)SelectionType.轮廓)
                    continue;
                barButtonItem = new BarButtonItem();
                barButtonItem.Caption = "删除".ToMultiLanguage() + " " + item.Text;
                barButtonItem.Tag = item.Value;
                barButtonItem.ItemClick += btnLabelType_ItemClick;
                barButtonItem.Visibility = BarItemVisibility.Always;
                barItemList.Add(barButtonItem);
            }
            popupMenuLabelRoot.AddItems(barItemList.ToArray());
        }

        private void InitReport()
        {
            xtraReportDetail = new XtraReportDetail();
            ShowReport();


        }

        private void InitTreeList()
        {
            var set = GlobalSettings.ApplySetting.traingSet;
            repositoryItemLookUpEdit1.DataSource = set.label.classes.OrderBy(o => o.id);

            treeList1.KeyFieldName = "ID";
            treeList1.ParentFieldName = "RegionID";
            treeList1.OptionsBehavior.PopulateServiceColumns = true;

            // 设置显示网格线
            treeList1.OptionsView.ShowHorzLines = false;
            treeList1.OptionsView.ShowVertLines = false;
            // 设置节点不可编辑
            treeList1.OptionsBehavior.Editable = true;
            // 更改背景颜色
            treeList1.Appearance.Empty.BackColor = Color.Gray;
            treeList1.Appearance.Row.BackColor = Color.Gray;
            treeList1.Appearance.Row.ForeColor = Color.White;
            treeList1.OptionsSelection.EnableAppearanceFocusedCell = false;
            treeList1.OptionsView.ShowIndicator = false;
            // 默认展开
            //treeList1.ExpandAll();
            treeList1.DataSource = trainSetDisplays;
        }

        private void InitPropertyGridControl()
        {
            xtraTabControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            tabPageAlgorithm.Appearance.Header.BackColor = Color.Gray;
            tabPageAlgorithm.Appearance.Header.ForeColor = Color.White;
            tabPagePreprocessing.Appearance.Header.BackColor = Color.Gray;
            tabPagePreprocessing.Appearance.Header.ForeColor = Color.White;

            var lst = typeof(TypeAlgorithm).GetValueTextItems();
            cboAlgorithmType.BindComboBoxItem(lst);
            //隐藏轮廓功能
            if (Constant.HideOutlineFunc)
            {
                GlobalSettings.ApplySetting.traingSet.algorithm.train_contour_enabled = false;
                AttributeExtendMethod.SetPropertyVisibility(GlobalSettings.ApplySetting.traingSet.algorithm, "train_contour_enabled", false);
                AttributeExtendMethod.SetPropertyVisibility(GlobalSettings.ApplySetting.traingSet.algorithm, "split_point_count", false);
                AttributeExtendMethod.SetPropertyVisibility(GlobalSettings.ApplySetting.traingSet.algorithm, "train_contour_erode", false);
                AttributeExtendMethod.SetPropertyVisibility(GlobalSettings.ApplySetting.traingSet.algorithm, "train_contour_threshold", false);
            }


            XApplySetting applySetting = GlobalSettings.ApplySetting;
            cboAlgorithmType.SetSelectedValue((int)applySetting.traingSet.algorithm.TypeAlgorithm);
            propertyAlgorithm.SelectedObject = applySetting.traingSet.algorithm;
            RefreshContourCtrl();


            gcConfig.DataSource = applySetting.traingSet.Preprocessings;
            gcConfig.RefreshDataSource();

            applySetting.traingSet.algorithm.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "StartBandIndex":
                    case "EndBandIndex":
                        var algorithm = applySetting.traingSet.algorithm;
                        if (algorithm.train_bands?.Bands.Count > 0)
                        {
                            algorithm.train_bands.Bands.ForEach(k => k.WaveLengthIndexs.RemoveAll(n => n < algorithm.StartBandIndex || n > algorithm.EndBandIndex));
                        }
                        RefToChart();
                        break;
                    case "train_contour_enabled":
                        RefreshContourCtrl();
                        RefreshSelection();
                        break;
                    case "split_point_count":
                        RefreshSelection();
                        break;
                    case "background_base":
                    case "train_contour_erode":
                    case "train_contour_threshold":
                        FindCountour(true);
                        break;

                }

            };

        }

        private void InitImageViewTool()
        {
            imageViewWithTools1.SetToolButtons(new ToolButton[]
            {
                ToolButton.Move,
                ToolButton.Point,
                ToolButton.Line,
                ToolButton.Rectangle,
                ToolButton.AutoResize,
                ToolButton.Matting,
                ToolButton.PointToLine,
                //ToolButton.Outline,
            });
            var tips = new Dictionary<ToolButton, string>();
            tips.Add(ToolButton.Move, "移动".ToMultiLanguage());
            tips.Add(ToolButton.Point, "选点".ToMultiLanguage());
            tips.Add(ToolButton.Line, "画线".ToMultiLanguage());
            tips.Add(ToolButton.Rectangle, "画矩形".ToMultiLanguage());
            tips.Add(ToolButton.AutoResize, "自适应".ToMultiLanguage());
            tips.Add(ToolButton.Matting, "抠图".ToMultiLanguage());
            tips.Add(ToolButton.PointToLine, "点选区域".ToMultiLanguage());
            //tips.Add(ToolButton.Outline, "轮廓区域".ToMultiLanguage());
            imageViewWithTools1.SetButtonToolTips(tips);
            imageViewWithTools1.SetToolSizeType(ToolSizeType.Percent, 16);
            imageViewWithTools1.AddSelectionEndEvent = (SelectionType type, object value) =>
            {
                string uid = string.Empty;
                var label = GlobalSettings.ApplySetting.traingSet.label;
                switch (type)
                {
                    case SelectionType.矩形:
                        var rectangular = value as RectangularSelection;
                        uid = rectangular.Guid;
                        var selection = label.AddSelecting(type, rectangular);
                        ClassTools.GetReflectBySelectRectArea(selection, spe, spectraOfSPE);
                        break;
                    case SelectionType.点选区:
                        var brush = value as BrushSelection;
                        uid = brush.Guid;
                        //点选区域的所有点
                        var points = CommonMethods.PointInsideShape(brush.AbsolutePointFs.ToArray());
                        var selection2 = label.AddSelecting(type, brush, null, points);
                        ClassTools.GetReflectBySelectPointsArea(selection2, spe, spectraOfSPE);
                        break;
                    case SelectionType.画笔:
                        var brushe = value as BrushSelection;
                        uid = brushe.Guid;
                        float[] ref3 = spectraOfSPE.GetAverageData(spe, brushe.AbsolutePoints);
                        label.AddSelecting(type, brushe, ref3, brushe.AbsolutePoints.ToArray());
                        break;
                    case SelectionType.点:
                    case SelectionType.魔法棒:
                        var point = value as PointSelection;
                        uid = point.Guid;
                        label.AddSelecting(type, point, spe[point.AbsolutePoint], null, Constant.MagicWandThreshold);
                        break;
                    case SelectionType.抠图:
                    case SelectionType.轮廓:
                        var irregular = value as IrregularSelection;
                        uid = irregular.Guid;
                        label.AddSelecting(type, irregular, null, null, Constant.MattingThreshold);
                        break;
                }
                UpdateTrainSet(uid);

                if (type == SelectionType.抠图)
                {
                    SetMattingImageForm(uid);
                }
                else if (type == SelectionType.轮廓)
                {
                    SetOutlineArea(uid);
                }
            };
            imageViewWithTools1.AddSelectionEvent += (SelectionType type, object value) =>
            {
                var e = value as SelectionInfo;
                e.SelectionColor = Color.FromArgb(230, Color.FromArgb(200, 200, 200));
            };

        }

        /// <summary>
        /// 选择轮廓刷新
        /// </summary>
        private void RefreshContourCtrl()
        {
            bool contourEnabled = GlobalSettings.ApplySetting.traingSet.algorithm.train_contour_enabled;
            btnContourImage.Visible = contourEnabled;
            lcCtl.Visible = contourEnabled;
        }

        #endregion

        #region 界面事件

        /// <summary>
        /// 标签删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLabelType_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string value = e.Item?.Tag.ToString();

            if (value != "")
            {
                ImgView.SelectionType selectionType = (ImgView.SelectionType)Convert.ToInt32(value);
                List<TrainSetDisplayCell> srainSetDisplayCells = trainSetDisplays.FindAll(o => o.CellType == EnumTrainSetCellType.Label && o.labelType == selectionType);
                if (srainSetDisplayCells == null || srainSetDisplayCells.Count <= 0)
                {
                    return;
                }
                if (FormShowHelper.ShowMessage("确定删除样本吗？".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) != DialogResult.OK)
                    return;

                var set = GlobalSettings.ApplySetting.traingSet;
                foreach (var item in srainSetDisplayCells)
                {
                    trainSetDisplays.Remove(item);

                    imageViewWithTools1.RemoveSelection(item.SelectingUid);
                    set.label.selections.RemoveAll(x => x.uid == item.uid);
                }

                int nodeID = trainSetDisplays.Find(i => i.CellType == EnumTrainSetCellType.LabelRoot).ID;
                if (treeList1.FocusedNode.PrevNode != null)
                {
                    nodeID = Convert.ToInt32(treeList1.FocusedNode.PrevNode["ID"]);
                }
                UpdateTrainSet(selectNodeID: nodeID);

                treeList1.RefreshDataSource();
            }
        }

        private void TrainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && treeList1.Focused)
            {
                TrainSetDisplayCell displayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
                if (displayCell == null)
                    return;
                if (displayCell.CellType == EnumTrainSetCellType.Label)
                {
                    btnDeleteClass_ItemClick(null, null);
                }
                else if (displayCell.CellType == EnumTrainSetCellType.LabelRoot)
                {
                    btnClearClass_ItemClick(null, null);
                }
                else if (displayCell.CellType == EnumTrainSetCellType.Model)
                {
                    btnDeleteModel_ItemClick(null, null);
                }
                else if (displayCell.CellType == EnumTrainSetCellType.ModelRoot)
                {
                    btnClearModel_ItemClick(null, null);
                }
            }
        }

        private void Train()
        {
            GlobalSettings.ApplySetting.Save();
            var set = GlobalSettings.ApplySetting.traingSet;
            if (spe == null)
            {
                FormShowHelper.ShowMessage("请先选择训练图片".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            if (!string.IsNullOrEmpty(GlobalSettings.ApplySetting.runingApp))
            {
                FormShowHelper.ShowMessage("请先停止正在运行的采集功能".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            //判断选区信息
            if (set.label.selections.Count <= 1)
            {
                FormShowHelper.ShowMessage("选区数量少于两个，数量不够".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            else if (set.label.selections.Count(x => x.classID < 254 && x.classID > 0) < 1)
            {
                FormShowHelper.ShowMessage("缺少目标选区".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            else if (set.label.selections.Exists(x => x.classUid == ""))
            {
                FormShowHelper.ShowMessage("存在未标记类型的选区，请核实".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }

            tokenSource = new CancellationTokenSource();
            loadingForm = new LoadingForm("模型训练".ToMultiLanguage(), "正在训练模型,请稍等...".ToMultiLanguage(), isShowCancel: true, isShowProgress: false);
            loadingForm.OnDoWork += (eArgs) =>
            {
                try
                {
                    System.Threading.Timer timer = new System.Threading.Timer(OnloadTimerEvent, null, 0, 1000);
                    Task.Run(async () => { await PythonTrain(); }).Wait();
                    timer.Dispose();
                    if (model == null) return;

                    loadingForm.SetCaption("模型验证".ToMultiLanguage());
                    loadingForm.SetMessage("模型验证中...".ToMultiLanguage());
                    loadingForm.SetProcessVisible(true);
                    ShowLabel(false);
                    LoadModel();
                    while (!GlobalSettings.ApplySetting.stopAppFlag)
                    {
                        Thread.Sleep(2000); //进度条等待计算线程结束
                    }
                }
                catch (Exception ex)
                {
                    FormShowHelper.ShowMessage("训练失败".ToMultiLanguage(), "提示".ToMultiLanguage());
                    GlobalSettings.ApplySetting.runingApp = string.Empty;
                    GlobalSettings.ApplySetting.stopAppFlag = true;
                    LogHelper.WriteLog(ex.Message);
                }
            };
            loadingForm.ShowDialog();
            loadingForm.Dispose();
            loadingForm = null;

            if (model != null)
            {
                //刷新模型
                UpdateTrainSet();
                GlobalSettings.ApplySetting.Save();
                // 刷新"统一颜色"下拉框的类别选项
                RefreshUnifyTargetCombo();
            }

        }

        private async Task PythonTrain()
        {
            model = null;
            string oriPath = spe.SpePath;
            var set = GlobalSettings.ApplySetting.traingSet;
            string trainTempPath = Path.Combine(Path.GetDirectoryName(oriPath), "Temp");
            var alType = (int)GlobalSettings.ApplySetting.traingSet.algorithm.TypeAlgorithm;
            //创建label文件
            //Console.WriteLine("------------ 加载Csv数据");
            var waveIndexs = ClassTools.WriteToCsvFile(trainTempPath, spe, tokenSource);
            if (waveIndexs == null) return;

            List<PLS> trainPls = new List<PLS>();
            if (alType == 0)
            {
                float trainSize = set.algorithm.train_size;
                int nComponents = set.algorithm.n_components;
                var args = new Dictionary<string, string>();
                args.Add("train-size", trainSize.ToString());  //训练集百分比
                args.Add("n-components", nComponents.ToString()); //主成分
                args.Add("algorithm_type", alType.ToString()); //0 plsda
                args.Add("temp_path", trainTempPath);
                Console.WriteLine(args.ToArgsStr());
                var str = await PythonHelper.Command("", args, tokenSource.Token);
                if (str == null) return;
                if (str.Length == 0 || !str[0].Contains("OK"))
                {
                    FormShowHelper.ShowMessage("算法运行错误，未生成模型".ToMultiLanguage(), "提示".ToMultiLanguage());
                    return;
                }
            }
            else if (alType == 1)
            {
                string path = Path.Combine(trainTempPath, "1.csv");
                Svm.chsvm = new Svm(path);
            }
            else if (alType == 2)
            {
                string saveDir = Path.Combine(Path.GetDirectoryName(oriPath), "Models\\Xml");
                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);

                foreach (var labelClass in set.label.classes)
                {
                    string fileName = Path.Combine(trainTempPath, $"{labelClass.id}.csv");
                    if (!File.Exists(fileName))
                        continue;
                    var svm = new OpenCVSvm(fileName);
                    string saveName = Guid.NewGuid().ToString() + ".xml";
                    if (svm.SaveToXml(Path.Combine(saveDir, saveName)))
                    {
                        trainPls.Add(new PLS()
                        {
                            classid = labelClass.id,
                            threshold = labelClass.Threshold,
                            color = labelClass.color,
                            selectIndex = waveIndexs.First(x => x.ClassId == labelClass.id).WaveLengthIndexs,
                            TrainFileName = saveName,
                        });
                    }
                }
            }
            model = new Model() { name = helper.GetDefaultModelName(set.models.Count + 1, set.models) };
            model.ModelType = alType;

            if (alType == 0)
            {
                model.TryLoad(Path.Combine(trainTempPath, "model.models"));
                if (model.plss != null && spe != null)
                {
                    foreach (var pls in model.plss)
                    {
                        var c = set.label.classes.First(x => x.id == pls.classid);
                        pls.threshold = c.Threshold;
                        pls.color = c.color;
                        pls.selectIndex = waveIndexs.First(x => x.ClassId == pls.classid).WaveLengthIndexs;
                    }
                }
            }
            else
            {
                model.plss = trainPls;
            }
            model.Sn = spe.Hdr.SN;
            model.ExpTime = (int)spe.Hdr.ExposureTime;
            model.FxModel = spe.Hdr.FXModel;
            model.Gain = (int)spe.Hdr.Gain;

            //保存建模时的条件
            model.hdr_spe_path = spe.SpePath;
            model.HdrWaveLength = spe.Hdr.WaveLength;
            model.ImgR = set.ImgR;
            model.ImgG = set.ImgG;
            model.ImgB = set.ImgB;
            model.ImgFR = set.ImgFR;
            model.ImgFG = set.ImgFG;
            model.ImgFB = set.ImgFB;
            model.ImgThresholdR = (int)set.ImgThresholdR;
            model.ImgThresholdG = (int)set.ImgThresholdG;
            model.ImgThresholdB = (int)set.ImgThresholdB;
            model.classes = set.label.classes;

            model.speHdrGuid = speHdrGuid;
            model.StartBandIndex = set.algorithm.StartBandIndex;
            model.EndBandIndex = set.algorithm.EndBandIndex;
            model.selections = set.label.selections.ListDeepClone();
            if (set.Preprocessings.Count > 0)
            {
                foreach (var item in set.Preprocessings)
                {
                    if (!item.Enabled)
                        continue;
                    model.Preprocessings.Add(item.DeepClone());
                }
            }
            model.ContourEnabled = set.algorithm.train_contour_enabled;
            model.ContourThreshold = set.algorithm.train_contour_threshold;
            model.ContourErode = set.algorithm.train_contour_erode;
            model.BackgroundCorrection = set.algorithm.BackgroundCorrection;
            model.StartBackgroundThreshold = set.algorithm.StartBackgroundThreshold;
            model.EndBackgroundThreshold = set.algorithm.EndBackgroundThreshold;
            set.models.Add(model);

        }

        /// <summary>
        /// 开始训练
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTrain_Click(object sender, EventArgs e)
        {
            Train();
        }

        /// <summary>
        /// 编辑分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClass_Click(object sender, EventArgs e)
        {
            XApplySetting applySetting = GlobalSettings.ApplySetting;
            ClassSettingForm classSettingForm = new ClassSettingForm(applySetting.traingSet.label.classes);
            FormShowHelper.ShowDialog(classSettingForm);
            applySetting.traingSet.label.classes = classSettingForm.LabelClasses;
            GlobalSettings.ApplySetting = applySetting;


            //编辑后刷新数据
            repositoryItemLookUpEdit1.DataSource = applySetting.traingSet.label.classes;
            treeList1.RefreshDataSource();


            foreach (var item in trainSetDisplays)
            {
                if (item.CellType == EnumTrainSetCellType.Label)
                {
                    var tempClass = applySetting.traingSet.label.classes.FirstOrDefault(x => x.uid == item.classUid);
                    if (tempClass == null)
                    {
                        item.classUid = string.Empty;
                        item.color = Color.FromArgb(230, Color.WhiteSmoke);
                    }
                    else
                    {
                        item.color = Color.FromArgb(tempClass.color);
                    }
                    var selectionItem = applySetting.traingSet.label.selections.FirstOrDefault(o => o.uid == item.SelectingUid);
                    if (selectionItem.color == item.color.ToArgb())
                        continue;
                    selectionItem.color = item.color.ToArgb();
                    imageViewWithTools1.SetSelectionColor(item.SelectingUid, item.color);
                }
            }
            //SetMagicImage();
            //SetMattingImage();
            RefToChart();
        }

        #region 标签模型相关

        private void treeList1_CustomDrawNodeCell(object sender, DevExpress.XtraTreeList.CustomDrawNodeCellEventArgs e)
        {
            // 检查要更改的列和节点
            if (e.Column == treeListColumn3 && e.Node != null)
            {
                // 获取当前节点的绑定数据
                var rowData = treeList1.GetRow(e.Node.Id) as TrainSetDisplayCell;
                if (rowData != null && (rowData.CellType == EnumTrainSetCellType.Label))
                {
                    // 在单元格中绘制圆角矩形
                    Rectangle rect = e.Bounds;
                    rect.Inflate(-1, -1); // 调整矩形的大小
                    // 圆角半径
                    int cornerRadius = 1;

                    // 创建路径
                    GraphicsPath path = helper.RoundedRectangle(rect, cornerRadius);
                    Color color = rowData.color;
                    // 设置矩形的颜色
                    using (SolidBrush brush = new SolidBrush(color))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; // 设置抗锯齿模式
                        e.Graphics.FillPath(brush, path); // 使用红色填充圆角矩形
                    }

                    e.Handled = true; // 告诉控件已经处理了绘制事件
                }
            }
            // 检查要更改的列和节点
            if (e.Column == treeListDrawType && e.Node != null)
            {
                // 获取当前节点的绑定数据
                var rowData = treeList1.GetRow(e.Node.Id) as TrainSetDisplayCell;
                if (rowData != null && (rowData.CellType != EnumTrainSetCellType.Label))
                {
                    e.CellText = "";
                }
            }
        }

        private void treeList1_CustomNodeCellEdit(object sender, DevExpress.XtraTreeList.GetCustomNodeCellEditEventArgs e)
        {
            if (e.Column == treeListcCLassificationColumn && e.Node != null)
            {
                // 获取当前节点的绑定数据
                var rowData = treeList1.GetRow(e.Node.Id) as TrainSetDisplayCell;
                if (rowData != null && (rowData.CellType != EnumTrainSetCellType.Label))
                {
                    // 创建一个空的编辑器，这样单元格中就不会显示控件
                    e.RepositoryItem = new RepositoryItem();
                }
            }
        }

        /// <summary>
        /// 更改标签分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void repositoryItemLookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            string classUid = (sender as LookUpEdit).EditValue.ToString();
            var set = GlobalSettings.ApplySetting.traingSet;

            TrainSetDisplayCell trainSetDisplayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            XApplySetting ApplySetting = GlobalSettings.ApplySetting;
            var selectedItem = ApplySetting.traingSet.label.selections.Find(o => o.uid == trainSetDisplayCell.SelectingUid);
            if (!string.IsNullOrEmpty(trainSetDisplayCell.SelectingUid) && selectedItem != null)
            {
                var selectedClass = set.label.classes.Find(o => o.uid == classUid);
                trainSetDisplayCell.color = Color.FromArgb(selectedClass.color);
                selectedItem.classUid = selectedClass.uid;
                selectedItem.classID = selectedClass.id;
                selectedItem.color = selectedClass.color;
                imageViewWithTools1.SetSelectionColor(trainSetDisplayCell.SelectingUid, trainSetDisplayCell.color);
                RefToChart();
            }
            treeList1.Focus();
        }

        /// <summary>
        /// 选中节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            if (treeList1.Nodes.Count <= 0)
            {
                return;
            }

            TrainSetDisplayCell trainSetDisplayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (trainSetDisplayCell == null)
                return;
            switch (trainSetDisplayCell.CellType)
            {
                case EnumTrainSetCellType.LabelRoot:
                    //标签Root
                    PreviewWithRGB();
                    ShowLabel(true);
                    navigationFrame1.SelectedPageIndex = 2;
                    break;
                case EnumTrainSetCellType.Label:
                    //标签
                    PreviewWithRGB();
                    ShowLabel(true);
                    var selection = GlobalSettings.ApplySetting.traingSet.label.selections.Find(i => i.uid == trainSetDisplayCell.SelectingUid);
                    imageViewWithTools1.GetSelectionByGuid(trainSetDisplayCell.SelectingUid);
                    RefToChart();
                    navigationFrame1.SelectedPageIndex = 0;
                    break;
                case EnumTrainSetCellType.ModelRoot:
                    PreviewWithRGB();
                    ShowLabel(false);
                    //模型Root
                    navigationFrame1.SelectedPageIndex = 3;
                    break;
                case EnumTrainSetCellType.Model:
                    //模型
                    ShowLabel(false);
                    if (!SetTrainResultImage(trainSetDisplayCell))
                    {
                        PreviewWithRGB();
                    }
                    navigationFrame1.SelectedPageIndex = 1;
                    ShowReport(trainSetDisplayCell);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 模型未更改，显示上一次计算的数据
        /// </summary>
        private bool SetTrainResultImage(TrainSetDisplayCell trainSetDisplayCell = null)
        {
            Model model = null;
            if (trainSetDisplayCell != null)
            {
                model = GlobalSettings.ApplySetting.traingSet.models.Find(o => o.uid == trainSetDisplayCell.uid);
            }
            if (model == null || string.IsNullOrEmpty(model.spe_image_path) || string.IsNullOrEmpty(model.speHdrGuid))
                return false;
            if (!File.Exists(model.spe_image_path) || model.speHdrGuid != speHdrGuid)
                return false;
            imageViewWithTools1.SetImage((Bitmap)Image.FromFile(model.spe_image_path), GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
            return true;
        }
        /// <summary>
        /// 右键显示属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeList1_MouseUp(object sender, MouseEventArgs e)
        {
            var hitInfo = treeList1.CalcHitInfo(e.Location);
            if (hitInfo.InRow && e.Button == MouseButtons.Right)
            {
                treeList1.FocusedNode = hitInfo.Node;
                TrainSetDisplayCell cell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
                if (cell == null)
                    return;
                switch (cell.CellType)
                {
                    case EnumTrainSetCellType.LabelRoot:
                        popupMenuLabelRoot.ShowPopup(MousePosition);
                        break;
                    case EnumTrainSetCellType.Label:
                        var selection = GlobalSettings.ApplySetting.traingSet.label.selections.Find(i => i.uid == cell.SelectingUid);
                        barBtnThresholdEdit.Visibility = (selection.labelType == SelectionType.魔法棒 || selection.labelType == SelectionType.抠图) ? BarItemVisibility.Always : BarItemVisibility.Never;
                        popupMenuLabel.ShowPopup(MousePosition);
                        break;
                    case EnumTrainSetCellType.ModelRoot:
                        popupMenuModelRoot.ShowPopup(MousePosition);
                        break;
                    case EnumTrainSetCellType.Model:
                        popupMenuModel.ShowPopup(MousePosition);
                        break;
                }

            }
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteClass_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TrainSetDisplayCell trainSetDisplayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (trainSetDisplayCell != null && trainSetDisplayCell.CellType == EnumTrainSetCellType.Label)
            {
                if (FormShowHelper.ShowMessage("确定删除该样本吗？".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) != DialogResult.OK)
                    return;
                var set = GlobalSettings.ApplySetting.traingSet;
                imageViewWithTools1.RemoveSelection(trainSetDisplayCell.SelectingUid);
                set.label.selections.RemoveAll(x => x.uid == trainSetDisplayCell.uid);


                int nodeID = trainSetDisplays.Find(i => i.CellType == EnumTrainSetCellType.LabelRoot).ID;
                if (treeList1.FocusedNode.PrevNode != null)
                {
                    nodeID = Convert.ToInt32(treeList1.FocusedNode.PrevNode["ID"]);
                }
                UpdateTrainSet(selectNodeID: nodeID);
            }
        }

        /// <summary>
        /// 清空标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearClass_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            var set = GlobalSettings.ApplySetting.traingSet;
            if (set.label.selections.Count > 0)
            {
                if (FormShowHelper.ShowMessage("确定清空所有样本吗？".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) != DialogResult.OK)
                    return;
            }
            set.label.selections.Clear();
            imageViewWithTools1.ClearSelections();
            if (spe != null && img != null)
            {
                var im = img.ToBitmap(spe.Lines, spe.Samples);
                imageViewWithTools1.SetImage(im, GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
            }
            UpdateTrainSet();
        }

        /// <summary>
        /// 保存至查看模型
        /// </summary>
        private void btnSaveToModelView_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {//Retrieves the currently focused tree list row
            //An object that is the currently focused tree list row.
            TrainSetDisplayCell trainSetDisplayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (trainSetDisplayCell != null && trainSetDisplayCell.CellType == EnumTrainSetCellType.Model)
            {
                var set = GlobalSettings.ApplySetting.traingSet;
                string path = GlobalSettings.ApplySetting.FolderPathOfBrowse + @"\Models\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                bool isSuc = set.models.First(x => x.uid == trainSetDisplayCell.uid).TrySave(path);
                FormShowHelper.ShowMessage((isSuc ? "保存成功".ToMultiLanguage() : "保存失败".ToMultiLanguage()), "提示".ToMultiLanguage());
            }
        }

        /// <summary>
        /// 删除模型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteModel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TrainSetDisplayCell trainSetDisplayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (trainSetDisplayCell != null && trainSetDisplayCell.CellType == EnumTrainSetCellType.Model)
            {
                if (FormShowHelper.ShowMessage("确定删除模型吗？".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) != DialogResult.OK)
                    return;

                var set = GlobalSettings.ApplySetting.traingSet;
                set.models.RemoveAll(x => x.uid == trainSetDisplayCell.uid);

                int nodeID = trainSetDisplays.Find(i => i.CellType == EnumTrainSetCellType.ModelRoot).ID;
                if (treeList1.FocusedNode.PrevNode != null)
                {
                    nodeID = Convert.ToInt32(treeList1.FocusedNode.PrevNode["ID"]);
                }
                UpdateTrainSet(selectNodeID: nodeID);
            }
        }

        /// <summary>
        /// 清空模型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClearModel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {//Prevides data for the itemclick and baritemclick events
            var set = GlobalSettings.ApplySetting.traingSet;
            if (set.models.Count > 0)
            {
                if (FormShowHelper.ShowMessage("确定清空所有模型吗？".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) != DialogResult.OK)
                    return;
                set.models.Clear();
                UpdateTrainSet();
            }

        }

        /// <summary>
        /// 模型重命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barBtnModelRename_ItemClick(object sender, ItemClickEventArgs e)
        {
            TrainSetDisplayCell displayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (displayCell == null || displayCell.CellType != EnumTrainSetCellType.Model)
                return;
            FromModelRename form = new FromModelRename(displayCell.uid, displayCell.name);
            if (FormShowHelper.ShowDialog(form) == DialogResult.OK)
            {
                displayCell.name = form.DisplayName;
                treeList1.RefreshDataSource();
                RefToChart();
            }

        }

        /// <summary>
        /// 启用模型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barBtnUseModel_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (spe == null)
            {
                FormShowHelper.ShowMessage("请先选择光谱图像".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            if (!string.IsNullOrEmpty(GlobalSettings.ApplySetting.runingApp))
            {
                FormShowHelper.ShowMessage("分选模块正在被使用".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            TrainSetDisplayCell displayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (displayCell == null || displayCell.CellType != EnumTrainSetCellType.Model)
                return;
            var set = GlobalSettings.ApplySetting.traingSet;
            model = set.models.Find(i => i.uid == displayCell.uid);
            if (spe.Hdr.WaveLength.Length != model.HdrWaveLength.Length)
            {
                FormShowHelper.ShowMessage("该模型的波长通道与训练图片不匹配".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }

            tokenSource = new CancellationTokenSource();
            loadingForm = new LoadingForm("模型验证".ToMultiLanguage(), "模型验证中...".ToMultiLanguage(), isShowCancel: true, isShowProgress: true);
            loadingForm.OnDoWork += (eArgs) =>
            {
                LoadModel();
                while (!GlobalSettings.ApplySetting.stopAppFlag)
                {
                    //进度条等待计算线程结束
                    Thread.Sleep(2000);
                }
            };
            loadingForm.ShowDialog();
            loadingForm.Dispose();
            loadingForm = null;
        }

        LoadingForm loadingForm = null;
        CancellationTokenSource tokenSource = null;
        private void OnloadTimerEvent(object obj)
        {
            if (loadingForm.MyWork.CancellationPending)
            {
                tokenSource.Cancel();
                //Console.WriteLine("已取消");
                return;
            }
        }
        private void OnProgress(float value, bool isEnd)
        {
            if (loadingForm == null)
                return;
            if (loadingForm.MyWork.CancellationPending)
            {
                tokenSource.Cancel();
                return;
            }

            loadingForm.MyWork.ReportProgress((int)value);
            if (isEnd)
            {
                loadingForm.MyWork.ReportProgress(100);
            }
        }

        /// <summary>
        /// 应用标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void barBtnUseModelLabel_ItemClick(object sender, ItemClickEventArgs e)
        {
            SetUnifyControlsEnabled(false);
            unifyControlsActivated = false;
            if (!string.IsNullOrEmpty(GlobalSettings.ApplySetting.runingApp))
            {
                FormShowHelper.ShowMessage("分选模块正在被使用".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }

            TrainSetDisplayCell displayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (displayCell == null || displayCell.CellType != EnumTrainSetCellType.Model)
                return;
            var set = GlobalSettings.ApplySetting.traingSet;
            model = set.models.Find(i => i.uid == displayCell.uid);
            imageViewWithTools1.ClearSelections();
            set.label.selections.Clear();
            if (model.selections == null || model.selections.Count == 0)
                return;
            if (model.speHdrGuid != speHdrGuid && model.hdr_spe_path != spe?.SpePath)
            {
                if (!File.Exists(model.hdr_spe_path))
                {
                    return;
                }
                if (spe != null && FormShowHelper.ShowMessage("已加载图片与该模型的标签不匹配，是否重新加载图片？".ToMultiLanguage(), "提示".ToMultiLanguage(), new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) != DialogResult.OK)
                    return;
                UpdateHypeImgPerform(model.hdr_spe_path, true);
                while (progress < 100)
                {
                    await Task.Delay(500);
                }
            }
            try
            {
                var defaultC = set.label.classes.Find(i => i.id == 254); //未分类
                foreach (var item in model.selections)
                {
                    var newSelection = item.DeepClone();

                    var s = set.label.classes.Find(i => i.id == item.classID);
                    if (s == null)
                    {
                        s = defaultC;
                    }
                    newSelection.classUid = s.uid;
                    newSelection.color = s.color;
                    set.label.selections.Add(newSelection);
                    switch (newSelection.labelType)
                    {
                        case SelectionType.点:
                            imageViewWithTools1.AddPoint(ClassTools.GetPointSelection(newSelection));
                            break;
                        case SelectionType.画笔:
                            imageViewWithTools1.AddLinePoint(ClassTools.GetLineSelection(newSelection));
                            break;
                        case SelectionType.矩形:
                            imageViewWithTools1.AddRectangleSelection(ClassTools.GetRectangleSelection(newSelection));
                            break;
                        case SelectionType.点选区:
                            imageViewWithTools1.AddPointToLine(ClassTools.GetPointsTolineSelection(newSelection));
                            break;
                        case SelectionType.轮廓:
                            imageViewWithTools1.AddOutlineSelection(ClassTools.GetOutlineSelection(newSelection));
                            break;
                        case SelectionType.抠图:
                            imageViewWithTools1.AddMattingSelection(ClassTools.GetMattingSelection(newSelection));
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                set.label.selections.Clear();
                imageViewWithTools1.ClearSelections();
            }
            finally
            {
                if (spe != null && img != null)
                {
                    var im = img.ToBitmap(spe.Lines, spe.Samples);
                    imageViewWithTools1.SetImage(im, GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
                }
                UpdateTrainSet();
            }
        }

        #endregion

        /// <summary>
        /// 发送到分选模块
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barBtnSend2Sorting_ItemClick(object sender, ItemClickEventArgs e)
        {
            TrainSetDisplayCell trainSetDisplayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (trainSetDisplayCell != null && trainSetDisplayCell.CellType == EnumTrainSetCellType.Model)
            {
                if (GlobalSettings.ApplySetting.runingApp == "sort")
                {
                    return;
                }
                var model = GlobalSettings.ApplySetting.traingSet.models.First(x => x.uid == trainSetDisplayCell.uid);
                model.ModelFileName = null;
                NotificationAction.Send2Sorting?.Invoke(model);
            }
        }

        private void barBtnSend2OutlineSorting_ItemClick(object sender, ItemClickEventArgs e)
        {
            TrainSetDisplayCell trainSetDisplayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (trainSetDisplayCell != null && trainSetDisplayCell.CellType == EnumTrainSetCellType.Model)
            {
                if (GlobalSettings.ApplySetting.runingApp == "sort")
                {
                    return;
                }
                var model = GlobalSettings.ApplySetting.traingSet.models.First(x => x.uid == trainSetDisplayCell.uid);
                NotificationAction.Send2OutlineSorting?.Invoke(model);
            }
        }

        /// <summary>
        /// 修改阈值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barBtnThresholdEdit_ItemClick(object sender, ItemClickEventArgs e)
        {
            TrainSetDisplayCell cell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (cell == null)
                return;
            var selection = GlobalSettings.ApplySetting.traingSet.label.selections.Find(i => i.uid == cell.SelectingUid);
            if (img == null)
                return;
            if (selection.labelType == SelectionType.抠图)
            {
                SetMattingImageForm(cell.SelectingUid);
            }
        }

        #region 定时器事件

        private void ImageViewTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (GlobalSettings.CurrentFormName != this.Name)
                    return;

                if (progress == 100)
                {
                    imageViewTimer.Stop();
                    updateDisplayModel();
                }
                if (spe != null && img != null)
                {
                    var im = img.ToBitmap(spe.Lines, spe.Samples);
                    imageViewWithTools1.SetImage(im, GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
                    if (progress == 100)
                    {
                        //没有预览图片，保存图片，并刷新界面
                        var imgPath = Path.ChangeExtension(spe.SpePath, ".bmp");
                        if (!File.Exists(imgPath))
                        {
                            im.Save(imgPath);
                            NotificationAction.FolderPathOfNewImgChanged?.Invoke();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("高光谱图像刷新异常");
            }
        }
        /// <summary>
        /// 从 model 读取"统一颜色"设置,应用到界面控件
        /// </summary>
        private void LoadUnifySettingsFromModel()
        {
            if (model == null) return;
            // === 新增诊断日志 ===
            LogHelper.WriteLog($"[UNIFY-LOAD] 从 model 加载, uid={model.uid}, name={model.name}");
            LogHelper.WriteLog($"[UNIFY-LOAD] model 中的值: enabled={model.UnifyColorEnabled}, target={model.UnifyTargetClassId}, threshold={model.UnifyConfidenceThreshold}, fill={model.UnifyFillBackground}");
            // === 原代码继续 ===
            try
            {
                // 暂时屏蔽事件,避免读取时触发写回/刷新
                isLoadingUnifySettings = true;

                // 阈值滑块
                if (trackUnifyThreshold != null)
                {
                    int threshold = model.UnifyConfidenceThreshold;
                    if (threshold < 0) threshold = 60;
                    if (threshold > 100) threshold = 100;
                    trackUnifyThreshold.Value = threshold;
                    if (lblUnifyThreshold != null)
                    {
                        lblUnifyThreshold.Text = $"置信度阈值: {threshold}%";
                    }
                }

                // 下拉框先填充类别选项,再设置选中项
                RefreshUnifyTargetCombo();
                if (cboUnifyTargetClass != null)
                {
                    int targetIdx = 0; // 默认选"自动"
                    for (int i = 0; i < cboUnifyTargetClass.Properties.Items.Count; i++)
                    {
                        var item = cboUnifyTargetClass.Properties.Items[i] as UnifyTargetItem;
                        if (item != null && item.ClassId == model.UnifyTargetClassId)
                        {
                            targetIdx = i;
                            break;
                        }
                    }
                    cboUnifyTargetClass.SelectedIndex = targetIdx;
                }

                // "填充背景"
                if (chkFillBackground != null)
                {
                    chkFillBackground.Checked = model.UnifyFillBackground;
                }

                // 最后才设置"统一颜色"复选框,触发一次刷新
                if (chkUnifyColor != null)
                {
                    chkUnifyColor.Checked = model.UnifyColorEnabled;
                }

                Console.WriteLine($"[统一颜色] 已从模型读取设置: 勾选={model.UnifyColorEnabled}, 目标={model.UnifyTargetClassId}, 阈值={model.UnifyConfidenceThreshold}%, 填充={model.UnifyFillBackground}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[统一颜色] LoadUnifySettingsFromModel 异常: " + ex.Message);
            }
            finally
            {
                isLoadingUnifySettings = false;
            }
        }

        /// <summary>
        /// 把当前界面的"统一颜色"设置写回 model,并保存
        /// </summary>
        private void SaveUnifySettingsToModel()
        {
            if (model == null) { LogHelper.WriteLog("[UNIFY-SAVE] 跳过: model 为 null"); return; }
            if (isLoadingUnifySettings) { LogHelper.WriteLog("[UNIFY-SAVE] 跳过: 正在加载中"); return; }

            try
            {
                bool enabled = chkUnifyColor != null && chkUnifyColor.Checked;
                int classId = -1;
                var selected = cboUnifyTargetClass?.SelectedItem as UnifyTargetItem;
                if (selected != null) classId = selected.ClassId;
                int thr = trackUnifyThreshold != null ? trackUnifyThreshold.Value : 60;
                bool fill = chkFillBackground != null && chkFillBackground.Checked;

                // === 关键日志: 写入 model 之前 ===
                LogHelper.WriteLog($"[UNIFY-SAVE] 即将写入 model, uid={model.uid}, name={model.name}");
                LogHelper.WriteLog($"[UNIFY-SAVE] 值: enabled={enabled}, target={classId}, threshold={thr}, fill={fill}");

                model.UnifyColorEnabled = enabled;
                model.UnifyTargetClassId = classId;
                model.UnifyConfidenceThreshold = thr;
                model.UnifyFillBackground = fill;

                // 验证: 写入后从 model 读回
                LogHelper.WriteLog($"[UNIFY-SAVE] 写入后 model 中实际值: enabled={model.UnifyColorEnabled}, target={model.UnifyTargetClassId}");

                // 验证: 检查 traingSet.models 列表里的对应 model 是否也更新了(应该引用同一对象)
                var listedModel = GlobalSettings.ApplySetting.traingSet.models.Find(m => m.uid == model.uid);
                if (listedModel == null)
                {
                    LogHelper.WriteLog($"[UNIFY-SAVE] ⚠️ 警告: 列表里找不到 uid={model.uid} 的模型!");
                }
                else if (!ReferenceEquals(listedModel, model))
                {
                    LogHelper.WriteLog($"[UNIFY-SAVE] ⚠️ 警告: 列表里的模型和当前 model 不是同一个实例!");
                    LogHelper.WriteLog($"[UNIFY-SAVE]    列表里的 enabled={listedModel.UnifyColorEnabled}");
                }
                else
                {
                    LogHelper.WriteLog($"[UNIFY-SAVE] ✓ 列表里的 model 是同一个实例, enabled={listedModel.UnifyColorEnabled}");
                }

                // 保存到全局配置
                GlobalSettings.ApplySetting.Save();

                // 验证磁盘文件是否真的包含新值
                string xmlPath = GlobalSettings.ApplyInfo.ApplySettingPath + "ApplySetting.xml";
                if (System.IO.File.Exists(xmlPath))
                {
                    string content = System.IO.File.ReadAllText(xmlPath);
                    bool hasField = content.Contains("UnifyColorEnabled");
                    bool hasTrue = content.Contains("<UnifyColorEnabled>true</UnifyColorEnabled>");
                    LogHelper.WriteLog($"[UNIFY-SAVE] 磁盘文件: 路径={xmlPath}, 含字段={hasField}, 含true={hasTrue}");
                    LogHelper.WriteLog($"[UNIFY-SAVE] 磁盘文件大小={new System.IO.FileInfo(xmlPath).Length} 字节");
                }
                else
                {
                    LogHelper.WriteLog($"[UNIFY-SAVE] ⚠️ 警告: 磁盘文件不存在! {xmlPath}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"[UNIFY-SAVE] 异常: {ex.Message}");
                LogHelper.WriteLog($"[UNIFY-SAVE] 异常堆栈: {ex.StackTrace}");
            }
        }
        /// <summary>
        /// 统一管理"统一颜色"相关控件的启用/禁用状态
        /// </summary>
        /// <param name="enabled">true=启用(预测图已渲染),false=禁用</param>
        private void SetUnifyControlsEnabled(bool enabled)
        {
            try
            {
                if (chkUnifyColor != null) chkUnifyColor.Enabled = enabled;

                // 下拉框、阈值、填充背景这些,只有"统一颜色"被勾选了才启用
                // 所以这里要么统一禁用,要么根据 chkUnifyColor.Checked 状态联动
                bool subEnabled = enabled && chkUnifyColor != null && chkUnifyColor.Checked;
                if (cboUnifyTargetClass != null) cboUnifyTargetClass.Enabled = subEnabled;
                if (trackUnifyThreshold != null) trackUnifyThreshold.Enabled = subEnabled;
                if (lblUnifyThreshold != null) lblUnifyThreshold.Enabled = subEnabled;
                if (chkFillBackground != null) chkFillBackground.Enabled = subEnabled;

                // 禁用时,强制取消所有勾选状态并清理备份,避免下次训练时残留状态
                if (!enabled)
                {
                    if (chkUnifyColor != null) chkUnifyColor.Checked = false;
                    if (chkFillBackground != null) chkFillBackground.Checked = false;
                    pridictionImageOriginal = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[统一颜色] SetUnifyControlsEnabled 异常: " + ex.Message);
            }
        }
        /// <summary>
        /// "统一颜色"复选框状态变化时触发,立即重新刷新显示
        /// </summary>
        private void chkUnifyColor_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"===== chkUnifyColor_CheckedChanged 触发,Checked={chkUnifyColor.Checked} =====");
            // 加这一行
            if (!isLoadingUnifySettings) SaveUnifySettingsToModel();
            // 让子控件跟着"统一颜色"勾选状态联动
            bool subEnabled = chkUnifyColor.Checked && unifyControlsActivated;
            if (cboUnifyTargetClass != null) cboUnifyTargetClass.Enabled = subEnabled;
            if (trackUnifyThreshold != null) trackUnifyThreshold.Enabled = subEnabled;
            if (lblUnifyThreshold != null) lblUnifyThreshold.Enabled = subEnabled;
            if (chkFillBackground != null)
            {
                chkFillBackground.Enabled = subEnabled;
                if (!chkUnifyColor.Checked)
                {
                    chkFillBackground.Checked = false;
                }
            }

            if (model == null || pridictionImage == null || img == null || spe == null)
            {
                Console.WriteLine("[统一颜色] 条件不满足,跳过。");
                return;
            }

            if (chkUnifyColor.Checked)
            {
                if (pridictionImageOriginal == null)
                {
                    BackupPridictionImage();
                }
                RefreshUnifyTargetCombo();
            }

            RefreshPredictionImage();
        }

        /// <summary>
        /// "填充背景"复选框变化:重新渲染
        /// </summary>
        private void chkFillBackground_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine($"[填充背景] CheckedChanged: Checked={chkFillBackground.Checked}");
            // 加这一行
            if (!isLoadingUnifySettings) SaveUnifySettingsToModel();
            if (chkUnifyColor == null || !chkUnifyColor.Checked) return;
            if (model == null || pridictionImage == null || img == null || spe == null) return;

            RefreshPredictionImage();
        }
        /// <summary>
        /// 根据当前 model 填充"统一目标类别"下拉框
        /// </summary>
        private void RefreshUnifyTargetCombo()
        {
            try
            {
                cboUnifyTargetClass.Properties.Items.Clear();

                // 第一项:自动(多数投票)
                cboUnifyTargetClass.Properties.Items.Add(new UnifyTargetItem
                {
                    ClassId = -1,
                    Display = "自动(多数投票)"
                });

                // 把 model.plss 里的每个类别加进来
                if (model != null && model.plss != null)
                {
                    var classes = GlobalSettings.ApplySetting.traingSet.label.classes;
                    foreach (var pls in model.plss)
                    {
                        // 找这个 classid 对应的类别名称
                        var cls = classes.Find(c => c.id == pls.classid);
                        string name = cls != null ? cls.name : ("类别 " + pls.classid);

                        cboUnifyTargetClass.Properties.Items.Add(new UnifyTargetItem
                        {
                            ClassId = pls.classid,
                            Display = name
                        });
                    }
                }

                // 默认选第一个(自动)
                if (cboUnifyTargetClass.Properties.Items.Count > 0 &&
                    cboUnifyTargetClass.SelectedIndex < 0)
                {
                    cboUnifyTargetClass.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[统一颜色] RefreshUnifyTargetCombo 异常: " + ex.Message);
            }
        }
        /// <summary>
        /// 兜底阈值滑块变化
        /// </summary>
        private void trackUnifyThreshold_EditValueChanged(object sender, EventArgs e)
        {
            // 实时更新标签显示
            if (lblUnifyThreshold != null)
            {
                lblUnifyThreshold.Text = $"置信度阈值: {trackUnifyThreshold.Value}%";
            }
            // 加这一行
            if (!isLoadingUnifySettings) SaveUnifySettingsToModel();
            // 只有勾选了统一颜色、且选了具体类别时才重新刷新
            if (chkUnifyColor == null || !chkUnifyColor.Checked) return;
            if (model == null || pridictionImage == null || img == null || spe == null) return;

            // 如果下拉框是"自动",阈值不起作用,不用刷新
            var selected = cboUnifyTargetClass.SelectedItem as UnifyTargetItem;
            if (selected == null || selected.ClassId < 0) return;

            RefreshPredictionImage();
        }
        /// <summary>
        /// 下拉框切换:重新刷新一下画面
        /// </summary>
        private void cboUnifyTargetClass_SelectedIndexChanged(object sender, EventArgs e)
        {  // 加这一行
            if (!isLoadingUnifySettings) SaveUnifySettingsToModel();
            // 只有勾选了"统一颜色"且有预测结果,切换才刷新
            if (chkUnifyColor == null || !chkUnifyColor.Checked) return;
            if (model == null || pridictionImage == null || img == null || spe == null) return;

            RefreshPredictionImage();
        }
        /// <summary>
        /// 手动刷新预测图(不依赖定时器和焦点判断)
        /// </summary>
        private unsafe void RefreshPredictionImage()
        {
            try
            {// 无论是否勾选,都先从备份还原
                RestorePridictionImage();

                // 勾选了才做聚合
                if (chkUnifyColor != null && chkUnifyColor.Checked)
                {
                    byte? forceClassId = null;
                    var selected = cboUnifyTargetClass.SelectedItem as UnifyTargetItem;
                    if (selected != null && selected.ClassId >= 0)
                    {
                        forceClassId = (byte)selected.ClassId;
                    }

                    // 读取兜底阈值(0~100 → 0.0~1.0)
                    float fallbackThreshold = (trackUnifyThreshold != null)
                        ? trackUnifyThreshold.Value / 100f
                        : 0.2f;

                    bool fillBg = chkFillBackground != null && chkFillBackground.Checked;
                    UnifyClassIdByContour(forceClassId, fallbackThreshold, fillBg);
                }
                Bitmap bitmap = img.ToBitmap(spe.Lines, spe.Samples);
                var bData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var ptr = (byte*)bData.Scan0;

                Parallel.For(0, spe.Samples, (int y) =>
                {
                    for (int x = 0; x < pridictionImage.Length; x++)
                    {
                        if (pridictionImage[x] is null) continue;
                        if (pridictionImage[x][y] == 0) continue;
                        if (model.plss == null || model.plss.Count == 0) continue;

                        foreach (var item in model.plss)
                        {
                            if (pridictionImage[x][y] == item.classid)
                            {
                                *(ptr + y * bData.Stride + x * 3) = Color.FromArgb(item.color).B;
                                *(ptr + y * bData.Stride + x * 3 + 1) = Color.FromArgb(item.color).G;
                                *(ptr + y * bData.Stride + x * 3 + 2) = Color.FromArgb(item.color).R;
                            }
                        }
                    }
                });
                bitmap.UnlockBits(bData);
                imageViewWithTools1.SetImage(bitmap, GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
                Console.WriteLine("[统一颜色] 图片已刷新到界面");

                // ===== 方案B: 把当前效果图覆盖保存到 spe_image_path =====
                // 这样切换到其他模型再切回来时,SetTrainResultImage 读取的就是带统一颜色效果的图
                SaveCurrentBitmapToModelPath(bitmap);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[统一颜色] RefreshPredictionImage 异常: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
     
        }
        /// <summary>
        /// 把当前渲染的 bitmap 保存到 model.spe_image_path,覆盖训练时的原始预测图
        /// 这样切换模型后再切回来,能直接从磁盘读到带"统一颜色"效果的图
        /// </summary>
        private void SaveCurrentBitmapToModelPath(Bitmap bitmap)
        {
            try
            {
                if (bitmap == null || model == null) return;
                if (string.IsNullOrEmpty(model.spe_image_path)) return;

                string dir = Path.GetDirectoryName(model.spe_image_path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // 克隆一份再保存,避免 bitmap 被界面占用时写文件冲突
                using (var clone = (Bitmap)bitmap.Clone())
                {
                    // 若文件被占用,用临时文件再替换的方式保证不抛异常
                    string tempFile = model.spe_image_path + ".tmp";
                    clone.Save(tempFile, ImageFormat.Bmp);

                    if (File.Exists(model.spe_image_path))
                    {
                        File.Delete(model.spe_image_path);
                    }
                    File.Move(tempFile, model.spe_image_path);
                }

                Console.WriteLine($"[统一颜色] 已保存效果图到: {model.spe_image_path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[统一颜色] SaveCurrentBitmapToModelPath 异常: " + ex.Message);
            }
        }
        /// <summary>
        /// 备份当前的 pridictionImage 到 pridictionImageOriginal
        /// </summary>
        private void BackupPridictionImage()
        {
            if (pridictionImage == null)
            {
                pridictionImageOriginal = null;
                return;
            }

            pridictionImageOriginal = new byte[pridictionImage.Length][];
            for (int i = 0; i < pridictionImage.Length; i++)
            {
                if (pridictionImage[i] == null)
                {
                    pridictionImageOriginal[i] = null;
                }
                else
                {
                    pridictionImageOriginal[i] = new byte[pridictionImage[i].Length];
                    Buffer.BlockCopy(pridictionImage[i], 0, pridictionImageOriginal[i], 0, pridictionImage[i].Length);
                }
            }
            Console.WriteLine("[统一颜色] 已备份原始预测结果");
        }

        /// <summary>
        /// 从备份还原 pridictionImage
        /// </summary>
        private void RestorePridictionImage()
        {
            if (pridictionImageOriginal == null || pridictionImage == null) return;
            if (pridictionImageOriginal.Length != pridictionImage.Length) return;

            for (int i = 0; i < pridictionImage.Length; i++)
            {
                if (pridictionImageOriginal[i] == null)
                {
                    pridictionImage[i] = null;
                }
                else
                {
                    if (pridictionImage[i] == null || pridictionImage[i].Length != pridictionImageOriginal[i].Length)
                    {
                        pridictionImage[i] = new byte[pridictionImageOriginal[i].Length];
                    }
                    Buffer.BlockCopy(pridictionImageOriginal[i], 0, pridictionImage[i], 0, pridictionImageOriginal[i].Length);
                }
            }
        }
        /// <summary>
        /// 按连通域统一 classid
        /// </summary>
        /// <param name="forceClassId">
        /// 用户指定的目标 classid。传 null 走多数投票(仅处理花色矿石)。
        /// </param>
        /// <param name="confidenceThreshold">
        /// 置信度阈值(0.0~1.0)。仅在 forceClassId 有值时生效。
        /// 置信度 = 最多类别像素数 / 矩形总面积。
        /// 低于阈值 = 模型不确信 → 染成 forceClassId
        /// 高于阈值 = 模型确信 → 保留原判
        /// </param>
        private void UnifyClassIdByContour(byte? forceClassId = null, float confidenceThreshold = 0.6f, bool fillBackground = false)
        {
            try
            {
                if (pridictionImage == null || pridictionImage.Length == 0 || model == null)
                    return;

                var set = GlobalSettings.ApplySetting.traingSet;
                var algorithm = set.algorithm;
                var channel = (set.ImgR, set.ImgG, set.ImgB);

                float tr, tg, tb;
                if (spe.Hdr.DataType == 4)
                {
                    tr = set.ImgThresholdR / 100f;
                    tg = set.ImgThresholdG / 100f;
                    tb = set.ImgThresholdB / 100f;
                }
                else
                {
                    tr = set.ImgThresholdR;
                    tg = set.ImgThresholdG;
                    tb = set.ImgThresholdB;
                }

                Bitmap rgbBitmap = SpeExtend.BackgroundCorrectionToBitmap(
                    spe, channel, (tr, tg, tb),
                    algorithm.StartBandIndex, algorithm.EndBandIndex,
                    algorithm.BackgroundCorrection,
                    algorithm.StartBackgroundThreshold, algorithm.EndBackgroundThreshold);

                var contourInfo = OpenCV.FindContourInfo(
                    rgbBitmap, 30f, 0,
                    needPreview: false, needRect: true, needAllPoints: true);
                rgbBitmap.Dispose();

                Console.WriteLine($"[统一颜色] 矿石数={contourInfo?.Count ?? 0}, " +
                                  $"模式={(forceClassId.HasValue ? "强制=" + forceClassId.Value : "自动")}, " +
                                  $"置信度阈值={confidenceThreshold:F2}");

                if (contourInfo == null || contourInfo.Count == 0) return;

                int changedCount = 0;
                int confidentStones = 0;  // 被判定为"模型确信",保留原判的数量
                int uncertainStones = 0;  // 被判定为"模型不确信",按用户指定覆盖的数量
                const int EXPAND = 2;

                Parallel.For(0, contourInfo.Count, (int i) =>
                {
                    var rect = contourInfo.ContourRects[i];

                    int x0 = Math.Max(0, rect.X - EXPAND);
                    int y0 = Math.Max(0, rect.Y - EXPAND);
                    int x1 = Math.Min(pridictionImage.Length - 1, rect.X + rect.Width + EXPAND);
                    int y1 = Math.Min(spe.Samples - 1, rect.Y + rect.Height + EXPAND);

                    // 统计票数和总像素
                    Dictionary<byte, int> voteCount = new Dictionary<byte, int>();
                    int totalPixelCount = 0;

                    for (int x = x0; x <= x1; x++)
                    {
                        if (pridictionImage[x] == null) continue;
                        for (int y = y0; y <= y1; y++)
                        {
                            totalPixelCount++;
                            byte cid = pridictionImage[x][y];
                            if (cid == 0) continue;
                            if (voteCount.ContainsKey(cid)) voteCount[cid]++;
                            else voteCount[cid] = 1;
                        }
                    }

                    if (totalPixelCount == 0) return;

                    // 找最多票数的类别
                    byte topClassId = 0;
                    int topVotes = 0;
                    foreach (var kv in voteCount)
                    {
                        if (kv.Value > topVotes)
                        {
                            topVotes = kv.Value;
                            topClassId = kv.Key;
                        }
                    }

                    // 置信度 = 最多类别像素数 / 矩形总面积
                    float confidence = (float)topVotes / totalPixelCount;

                    // ===== 决策分支 =====
                    byte winnerClassId;
                    bool shouldOverride;  // 是否要强制覆盖(覆盖时整颗矿石都染色,含原本是背景的像素)

                    if (forceClassId.HasValue)
                    {
                        // 用户指定了类别
                        if (confidence >= confidenceThreshold)
                        {
                            // 模型确信 → 保留原判,但用矿石自己的主要类别
                            System.Threading.Interlocked.Increment(ref confidentStones);

                            if (voteCount.Count >= 2)
                            {
                                winnerClassId = topClassId;
                                shouldOverride = false;
                            }
                            else if (voteCount.Count == 1 && fillBackground)
                            {
                                // 单一类别,但要填充背景
                                winnerClassId = topClassId;
                                shouldOverride = false;
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            // 模型不确信 → 按用户指定的染
                            winnerClassId = forceClassId.Value;
                            shouldOverride = true;
                            System.Threading.Interlocked.Increment(ref uncertainStones);
                        }
                    }
                    else
                    {
                        // 自动模式
                        if (voteCount.Count >= 2)
                        {
                            // 花色矿石:多数投票
                            winnerClassId = topClassId;
                            shouldOverride = false;
                        }
                        else if (voteCount.Count == 1 && fillBackground)
                        {
                            // 单一类别,但勾了"填充背景",也要处理
                            winnerClassId = topClassId;
                            shouldOverride = false;
                        }
                        else
                        {
                            // 单一类别且未勾"填充背景",不处理
                            return;
                        }
                    }

                    // ===== 改写 =====
                    // ===== 改写 =====
                    int localChanged = 0;

                    // fillBackground=true 或 shouldOverride=true 都需要走"整颗矿石填充"路径
                    bool needFullFill = shouldOverride || fillBackground;

                    if (needFullFill)
                    {
                        // 整颗矿石(轮廓内所有点)都染成 winnerClassId
                        var points = contourInfo.AllPoints[i];
                        if (points == null) return;
                        foreach (var pt in points)
                        {
                            if (pt.X < 0 || pt.X >= pridictionImage.Length) continue;
                            if (pridictionImage[pt.X] == null) continue;
                            if (pt.Y < 0 || pt.Y >= pridictionImage[pt.X].Length) continue;

                            if (pridictionImage[pt.X][pt.Y] != winnerClassId)
                            {
                                pridictionImage[pt.X][pt.Y] = winnerClassId;
                                localChanged++;
                            }
                        }
                    }
                    else
                    {
                        // 普通统一:只改已有非 0 像素,保留背景
                        for (int x = x0; x <= x1; x++)
                        {
                            if (pridictionImage[x] == null) continue;
                            for (int y = y0; y <= y1; y++)
                            {
                                if (pridictionImage[x][y] != 0 &&
                                    pridictionImage[x][y] != winnerClassId)
                                {
                                    pridictionImage[x][y] = winnerClassId;
                                    localChanged++;
                                }
                            }
                        }
                    }

                    if (localChanged > 0)
                    {
                        System.Threading.Interlocked.Add(ref changedCount, localChanged);
                    }
                });

                Console.WriteLine($"[统一颜色] 确信保留={confidentStones}, 不确信覆盖={uncertainStones}, 改写像素={changedCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[统一颜色] UnifyClassIdByContour 异常: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        private unsafe void PridictionImageViewTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                bool isSaveImage = sender == null ? true : false;
                if (GlobalSettings.CurrentFormName != this.Name || model == null)
                    return;
                if (GlobalSettings.ApplySetting.stopAppFlag && pridictionImageViewTimer.Enabled)
                {
                    pridictionImageViewTimer.Stop();
                    isSaveImage = true;
                }

                ////var set = GlobalSettings.ApplySetting.traingSet;
                //// ========== 统一颜色: 如果复选框勾选,先按矿石聚合 classid ==========
                //if (chkUnifyColor != null && chkUnifyColor.Checked)
                //{
                //    UnifyClassIdByContour();
                //}
                //// ===============================================================

                Bitmap bitmap = img.ToBitmap(spe.Lines, spe.Samples);
                var bData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var ptr = (byte*)bData.Scan0;
                var set = GlobalSettings.ApplySetting.traingSet;
                //var firstColor = set.label.classes.First(x => x.id == 1).color;
                Parallel.For(0, spe.Samples, (int y) =>
                {
                    for (int x = 0; x < pridictionImage.Length; x++)
                    {
                        if (pridictionImage[x] is null)
                            continue;

                        if (pridictionImage[x][y] != 0)
                        {
                            if (model.plss.Count == 0)
                            {
                                //if (pridictionImage[x][y] == 1)
                                //{
                                //    *(ptr + y * bData.Stride + x * 3) = Color.FromArgb(firstColor).B;
                                //    *(ptr + y * bData.Stride + x * 3 + 1) = Color.FromArgb(firstColor).G;
                                //    *(ptr + y * bData.Stride + x * 3 + 2) = Color.FromArgb(firstColor).R;
                                //}
                            }
                            else
                            {
                                foreach (var item in model.plss)
                                {
                                    if (pridictionImage[x][y] == item.classid)
                                    {
                                        *(ptr + y * bData.Stride + x * 3) = Color.FromArgb(item.color).B;
                                        *(ptr + y * bData.Stride + x * 3 + 1) = Color.FromArgb(item.color).G;
                                        *(ptr + y * bData.Stride + x * 3 + 2) = Color.FromArgb(item.color).R;
                                    }
                                }
                            }

                        }
                    }
                });
                bitmap.UnlockBits(bData);
                imageViewWithTools1.SetImage(bitmap, GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
                // ===== 首次渲染完成后启用"统一颜色"控件 =====
                if (!unifyControlsActivated && pridictionImage != null && pridictionImage.Length > 0)
                {
                    bool hasData = false;
                    for (int x = 0; x < pridictionImage.Length; x++)
                    {
                        if (pridictionImage[x] != null)
                        {
                            hasData = true;
                            break;
                        }
                    }
                    if (hasData)
                    {
                        unifyControlsActivated = true;
                        SetUnifyControlsEnabled(true);
                        Console.WriteLine("[统一颜色] 预测图已渲染,启用相关控件");

                        // ===== 新增:从 model 读取之前保存的设置 =====
                        LoadUnifySettingsFromModel();
                    }
                }
                if (isSaveImage)
                {
                    string dir = GlobalSettings.ApplyInfo.PreviewImgPath;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    string fileName = Path.Combine(dir, Guid.NewGuid().ToString() + ".bmp");
                    bitmap.Save(fileName);
                    //已经存在的预览图片删除
                    if (File.Exists(model.spe_image_path))
                        File.Delete(model.spe_image_path);

                    model.speHdrGuid = speHdrGuid;
                    model.spe_image_path = fileName;
                }
            }
            catch
            {
                Console.WriteLine("高光谱图像刷新异常");
            }
        }

        private void GpuUsedTimer_Tick(object sender, EventArgs e)
        {
            if (CudaAccelerator.Shared != null)
            {
                string name = CudaAccelerator.Shared.GetName();
                if (!string.IsNullOrEmpty(name))
                {
                    gpuUsedTimer.Stop();
                    if (lblKernelInfo.InvokeRequired)
                    {
                        lblKernelInfo.Invoke(new Action(() =>
                        {
                            lblKernelInfo.Text = name;
                        }));
                    }
                    else
                    {
                        lblKernelInfo.Text = name;
                    }
                }
            }
        }


        //private System.Timers.Timer SetMagicImageTimer;

        //private void SetMagicImageTimer_Elapsed(object sender, EventArgs e)
        //{
        //    SetMagicImage();
        //}

        #endregion

        #region 刷新显示默认

        private void updateDisplayModel()
        {
            cmbRed.EditValue = GlobalSettings.ApplySetting.traingSet.ImgR;
            cmbGreen.EditValue = GlobalSettings.ApplySetting.traingSet.ImgG;
            cmbBlue.EditValue = GlobalSettings.ApplySetting.traingSet.ImgB;
            spinImgRange1.EditValue = (int)GlobalSettings.ApplySetting.traingSet.ImgThresholdR;
            spinImgRange2.EditValue = (int)GlobalSettings.ApplySetting.traingSet.ImgThresholdG;
            spinImgRange3.EditValue = (int)GlobalSettings.ApplySetting.traingSet.ImgThresholdB;
        }

        private void InitDisplayModel()
        {
            List<BaseDictionary> baseDictionaries1 = new List<BaseDictionary>();
            for (int i = 0; i < spe.Hdr.WaveLength.Length; i++)
            {
                baseDictionaries1.Add(new BaseDictionary()
                {
                    Index = i,
                    Code = i.ToString(),
                    Value = spe.Hdr.WaveLength[i].ToString(),
                    Desc = i + ": " + spe.Hdr.WaveLength[i].ToString("F2"),
                });
            }
            cmbRed.Properties.DataSource = baseDictionaries1;
            cmbRed.EditValue = GlobalSettings.ApplySetting.traingSet.ImgR;
            cmbGreen.Properties.DataSource = baseDictionaries1;
            cmbGreen.EditValue = GlobalSettings.ApplySetting.traingSet.ImgG;
            cmbBlue.Properties.DataSource = baseDictionaries1;
            cmbBlue.EditValue = GlobalSettings.ApplySetting.traingSet.ImgB;

            int momo = 0;
            int momoTickFrequency = 0;
            List<int> Threshold = new List<int>();
            int mono4 = 100;//需要/ 1
            int mono8 = 255;
            int mono12 = 4096;
            int mono14 = 16384;
            switch (spe.Hdr.DataType)
            {
                case 0:
                case 1:
                    momoTickFrequency = 50;
                    momo = mono8;
                    break;
                case 4://反射率图像
                    momoTickFrequency = 1;
                    momo = mono4;
                    break;
                case 12:
                    momoTickFrequency = 500;
                    momo = mono12;
                    break;
                case 14:
                    momoTickFrequency = 1000;
                    momo = mono14;
                    break;
                default:
                    momoTickFrequency = 50;
                    momo = mono8;
                    break;
            }
            rangeTBC1.Properties.Maximum = momo;
            rangeTBC2.Properties.Maximum = momo;
            rangeTBC3.Properties.Maximum = momo;

            rangeTBC1.Properties.TickFrequency = momoTickFrequency;
            rangeTBC2.Properties.TickFrequency = momoTickFrequency;
            rangeTBC3.Properties.TickFrequency = momoTickFrequency;

            spinImgRange1.Properties.MaxValue = momo;
            spinImgRange2.Properties.MaxValue = momo;
            spinImgRange3.Properties.MaxValue = momo;
            spinImgRange1.EditValue = (int)GlobalSettings.ApplySetting.traingSet.ImgThresholdR > momo ? momo : (int)GlobalSettings.ApplySetting.traingSet.ImgThresholdR;
            spinImgRange2.EditValue = (int)GlobalSettings.ApplySetting.traingSet.ImgThresholdG > momo ? momo : (int)GlobalSettings.ApplySetting.traingSet.ImgThresholdG;
            spinImgRange3.EditValue = (int)GlobalSettings.ApplySetting.traingSet.ImgThresholdB > momo ? momo : (int)GlobalSettings.ApplySetting.traingSet.ImgThresholdB;
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
            if (spe == null)
            {
                return;
            }
            var set = GlobalSettings.ApplySetting;
            // 自动计算阈值
            var rc = set.traingSet.ImgR;
            var gc = set.traingSet.ImgG;
            var bc = set.traingSet.ImgB;

            var thresholds = spectraOfSPE.GetThreshold(spe, new int[3] { rc, gc, bc });
            var rt = thresholds[0];
            var gt = thresholds[1];
            var bt = thresholds[2];
            if (spe.Hdr.DataType == 4)//反射率图像
            {
                set.traingSet.ImgThresholdR = rt * 100;
                set.traingSet.ImgThresholdG = gt * 100;
                set.traingSet.ImgThresholdB = bt * 100;
            }
            else
            {
                set.traingSet.ImgThresholdR = rt;
                set.traingSet.ImgThresholdG = gt;
                set.traingSet.ImgThresholdB = bt;
            }
            GlobalSettings.ApplySetting = set;
            updateDisplayModel();
        }

        private void hlLabelResetRGB_Click(object sender, EventArgs e)
        {
            if (spe == null)
            {
                return;
            }
            //更新显示波段和阈值
            var set = GlobalSettings.ApplySetting;
            set.traingSet.ImgR = (int)spe.Hdr.DefaultBands[0];
            set.traingSet.ImgG = (int)spe.Hdr.DefaultBands[1];
            set.traingSet.ImgB = (int)spe.Hdr.DefaultBands[2];
            set.traingSet.ImgFR = spe.Hdr.WaveLength[set.traingSet.ImgR];
            set.traingSet.ImgFG = spe.Hdr.WaveLength[set.traingSet.ImgG];
            set.traingSet.ImgFB = spe.Hdr.WaveLength[set.traingSet.ImgB];
            GlobalSettings.ApplySetting = set;
            updateDisplayModel();
        }

        /// <summary>
        /// 刷新显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFreshDisplay_Click(object sender, EventArgs e)
        {
            //保存设置
            var set = GlobalSettings.ApplySetting;
            set.traingSet.ImgR = cmbRed.ItemIndex;
            set.traingSet.ImgG = cmbGreen.ItemIndex;
            set.traingSet.ImgB = cmbBlue.ItemIndex;
            set.traingSet.ImgFR = spe.Hdr.WaveLength[set.traingSet.ImgR];
            set.traingSet.ImgFG = spe.Hdr.WaveLength[set.traingSet.ImgG];
            set.traingSet.ImgFB = spe.Hdr.WaveLength[set.traingSet.ImgB];

            set.traingSet.ImgThresholdR = (float)spinImgRange1.Value;
            set.traingSet.ImgThresholdG = (float)spinImgRange2.Value;
            set.traingSet.ImgThresholdB = (float)spinImgRange3.Value;
            GlobalSettings.ApplySetting = set;
            //刷新显示
            PreviewWithRGB();
        }

        #endregion 刷新显示默认

        #endregion

        #region methods

        /// <summary>
        /// 加载预览
        /// </summary>
        private void LoadPreview()
        {
            Task.Run(() =>
            {
                // 自动计算阈值
                var rc = (int)spe.Hdr.DefaultBands[0];
                var gc = (int)spe.Hdr.DefaultBands[1];
                var bc = (int)spe.Hdr.DefaultBands[2];

                float rt, gt, bt;

                //更新显示波段和阈值
                var set = GlobalSettings.ApplySetting;
                set.traingSet.ImgR = (int)spe.Hdr.DefaultBands[0];
                set.traingSet.ImgG = (int)spe.Hdr.DefaultBands[1];
                set.traingSet.ImgB = (int)spe.Hdr.DefaultBands[2];
                set.traingSet.ImgFR = spe.Hdr.WaveLength[set.traingSet.ImgR];
                set.traingSet.ImgFG = spe.Hdr.WaveLength[set.traingSet.ImgG];
                set.traingSet.ImgFB = spe.Hdr.WaveLength[set.traingSet.ImgB];
                if (spe.Hdr.DataType == 4)//反射率图像
                {
                    set.traingSet.ImgThresholdR = 100;
                    set.traingSet.ImgThresholdG = 100;
                    set.traingSet.ImgThresholdB = 100;
                    rt = 1;
                    gt = 1;
                    bt = 1;
                }
                else
                {
                    var thresholds = spectraOfSPE.GetThreshold(spe, new int[3] { rc, gc, bc });

                    rt = thresholds[0];
                    gt = thresholds[1];
                    bt = thresholds[2];

                    set.traingSet.ImgThresholdR = rt;
                    set.traingSet.ImgThresholdG = gt;
                    set.traingSet.ImgThresholdB = bt;
                }
                GlobalSettings.ApplySetting = set;

                // 生成预览图
                spectraOfSPE.ToBGR(spe, (rc, gc, bc), (rt, gt, bt), (data, i, total, complete) =>
                {
                    img = data;
                    progress = i / (float)total * 100f;
                });

            });
        }


        /// <summary>
        /// 根据RGB刷新图像
        /// </summary>
        public void PreviewWithRGB()
        {
            if (spe == null)
                return;
            imageViewTimer.Start();
            progress = -1;
            float tr, tg, tb;//阈值
            if (spe.Hdr.DataType == 4)//反射率图像
            {
                tr = GlobalSettings.ApplySetting.traingSet.ImgThresholdR / 100;
                tg = GlobalSettings.ApplySetting.traingSet.ImgThresholdG / 100;
                tb = GlobalSettings.ApplySetting.traingSet.ImgThresholdB / 100;
            }
            else
            {
                tr = GlobalSettings.ApplySetting.traingSet.ImgThresholdR;
                tg = GlobalSettings.ApplySetting.traingSet.ImgThresholdG;
                tb = GlobalSettings.ApplySetting.traingSet.ImgThresholdB;
            }

            spectraOfSPE.ToBGR(spe, (GlobalSettings.ApplySetting.traingSet.ImgR, GlobalSettings.ApplySetting.traingSet.ImgG, GlobalSettings.ApplySetting.traingSet.ImgB),
                (tr, tg, tb), (data, i, total, complete) =>
                {
                    img = data;
                    progress = i / (float)total * 100f;
                });
        }



        private void SetMattingImageForm(string selectionUid)
        {
            var form = new FormThresholdMatting(selectionUid);
            form.spe = spe;
            form.spectraOfSPE = spectraOfSPE;
            var currSelection = GlobalSettings.ApplySetting.traingSet.label.selections.Find(i => i.uid == selectionUid);
            if (currSelection.ThresholdDiffs == null)
            {
                Task task1 = new Task(new Action(() =>
                {
                    currSelection.ThresholdDiffs = helper.CalculateThresholdDiffs(img, spe.Lines, spe.Samples, currSelection.AbsoluteRectangle);
                    currSelection.CompareResult = CommonMethods.CalaulateMattingCompareResult(currSelection.ThresholdDiffs, currSelection.Threshold, currSelection.ThresholdAlgorithm, currSelection.ErodeLevel);
                    //第一次计算完成，通知更新界面， 为了进度条，将界面更新放到子窗体 
                    form.FirstCalculateNotice?.Invoke();
                }));
                task1.Start();
            }

            form.SetMattingImage = () =>
            {
                this.Invoke(new Action(() =>
                {
                    imageViewWithTools1.UpdateMattingSelection(selectionUid, currSelection.Points.ToList());
                    if (!string.IsNullOrEmpty(selectionUid))
                    {
                        RefToChart(selectionUid);
                    }
                }));
            };

            FormShowHelper.ShowDialog(form);
        }

        /// <summary>
        /// 计算轮廓选区
        /// </summary>
        /// <param name="selectionUid"></param>
        private void SetOutlineArea(string selectionUid)
        {
            FormShowHelper.ShowLoadingForm(this, "计算中".ToMultiLanguage() + "...");
            if (contourInfo == null || contourInfo.Count == 0)
            {
                FindCountour(false);
            }
            if (contourInfo?.Count > 0)
            {
                var currSelection = GlobalSettings.ApplySetting.traingSet.label.selections.Find(i => i.uid == selectionUid);
                List<int> insideIndex = new List<int>();

                int _x = currSelection.AbsoluteRectangle.X;
                int _y = currSelection.AbsoluteRectangle.Y;
                int _right = currSelection.AbsoluteRectangle.Right;
                int _bottom = currSelection.AbsoluteRectangle.Bottom;

                for (int i = 0; i < contourInfo.ContourRects.Length; i++)
                {
                    var rect = contourInfo.ContourRects[i];
                    // 计算重叠区域的坐标
                    int overlap_x1 = Math.Max(_x, rect.X);
                    int overlap_y1 = Math.Max(_y, rect.Y);
                    int overlap_x2 = Math.Min(_right, rect.Right);
                    int overlap_y2 = Math.Min(_bottom, rect.Bottom);

                    // 计算重叠区域的宽度和高度
                    int overlap_width = Math.Max(0, overlap_x2 - overlap_x1);
                    int overlap_height = Math.Max(0, overlap_y2 - overlap_y1);
                    if (overlap_width > 0 && overlap_height > 0)
                    {
                        //完全包含轮廓
                        if (overlap_x1 == rect.X && overlap_y1 == rect.Y && overlap_width == rect.Width && overlap_height == rect.Height)
                        {
                            insideIndex.Add(i);
                            continue;
                        }
                        bool inside = false;
                        //重叠部分
                        for (int x = overlap_x1; x <= overlap_x1 + overlap_width; x++)
                        {
                            for (int y = overlap_y1; y <= overlap_y1 + overlap_height; y++)
                            {
                                if (contourInfo.AllPoints[i].Contains(new Point(x, y)))
                                {
                                    inside = true;
                                    insideIndex.Add(i);
                                    break;
                                }
                            }
                            if (inside)
                                break;
                        }
                    }
                }

                List<Point> points = new List<Point>();

                if (insideIndex.Count > 0)
                {
                    Point[][] cPoints = new Point[insideIndex.Count][];
                    for (int i = 0; i < insideIndex.Count; i++)
                    {
                        cPoints[i] = contourInfo.AllPoints[insideIndex[i]].ToArray();
                        points.AddRange(contourInfo.AllPoints[insideIndex[i]]);
                    }
                    currSelection.ContourPoints = cPoints;

                }
                else
                {
                    Point[][] cPoints = new Point[1][];
                    for (int x = _x; x <= _right; x++)
                    {
                        for (int y = _y; y <= _bottom; y++)
                        {
                            points.Add(new Point(x, y));
                        }
                    }
                    cPoints[0] = points.ToArray();
                    currSelection.ContourPoints = cPoints;
                }
                ClassTools.GetReflectBySelectOutlineArea(currSelection, spe, spectraOfSPE);

                this.Invoke(new Action(() =>
                {
                    imageViewWithTools1.UpdateOutlineSelection(selectionUid, points);
                    if (!string.IsNullOrEmpty(selectionUid))
                    {
                        RefToChart(selectionUid);
                    }
                }));
            }
            FormShowHelper.CloseLoadingForm();
        }

        private void UpdateHypeImgPerform(string path, bool isUpdateLabel)
        {
            try
            {
                var set = GlobalSettings.ApplySetting.traingSet;
                spe.SpeDispose();
                spe = null;
                spe = new SPE(path);
                speHdrGuid = Common.GenerateGuidFromFile(spe.HdrPath);

                img = null;
                progress = -1;
                LoadPreview();
                imageViewTimer.Start();
                //切换到图像显示设置部分
                navigationFrame1.SelectedPageIndex = 2;
                InitDisplayModel();
                imageViewWithTools1.SetImage(null);
                imageViewWithTools1.imageView.AutoResize();
                if (!isUpdateLabel)
                    return;
                //更换图片后，清空选区
                set.label.selections.Clear();
                imageViewWithTools1.ClearSelections();

                //选择用于训练的波长 赋初值 
                bool isok = true;
                if (set.algorithm.train_bands == null || set.algorithm.train_bands.WaveLength == null)
                {
                    isok = false;
                }
                else
                {
                    if (set.algorithm.train_bands.WaveLength.Length != spe.Hdr.WaveLength.Length)
                    {
                        isok = false;
                    }
                    else
                    {
                        for (int i = 0; i < spe.Hdr.WaveLength.Length; i++)
                        {
                            if (spe.Hdr.WaveLength[i] - set.algorithm.train_bands.WaveLength[i] > 0.0001f)
                            {
                                isok = false;
                                break;
                            }
                        }
                    }
                    if (set.algorithm.StartBandIndex == set.algorithm.EndBandIndex)
                    {
                        isok = false;
                    }
                }
                if (!isok)
                {
                    set.algorithm.train_bands = new TrainBands() { WaveLength = spe.Hdr.WaveLength };
                    set.algorithm.StartBandIndex = 0;
                    set.algorithm.EndBandIndex = spe.Hdr.WaveLength.Length - 1;
                    propertyAlgorithm.Refresh();
                    set.models.Clear();
                }
                UpdateTrainSet();
            }
            catch (Exception ex)
            {
                Console.WriteLine("高光谱图像加载异常");
            }
        }

        private void ShowReport(TrainSetDisplayCell trainSetDisplayCell = null)
        {
            Model model = null;
            if (trainSetDisplayCell != null)
            {
                model = GlobalSettings.ApplySetting.traingSet.models.Find(o => o.uid == trainSetDisplayCell.uid);
            }
            xtraReportDetail.InitData(model);
            xtraReportDetail.CreateDocument(true);
            documentViewer1.DocumentSource = xtraReportDetail;
        }

        /// <summary>
        /// 刷新树节点
        /// </summary>
        /// <param name="selectionUid"></param>
        private void UpdateTrainSet(string selectionUid = null, int? selectNodeID = null)
        {
            trainSetDisplays.Clear();
            treeList1.Focus();

            var set = GlobalSettings.ApplySetting.traingSet;

            int sortid = 0;
            trainSetDisplays.Add(new TrainSetDisplayCell()
            {
                ID = sortid++,
                RegionID = -1,
                CellType = EnumTrainSetCellType.LabelRoot,
                name = "标签".ToMultiLanguage(),
                labelCheck = set.label.selections.Count == 0 || set.label.selections.Exists(x => x.labelCheck == false) ? false : true
            }); ;
            foreach (var item in set.label.selections)
            {
                trainSetDisplays.Add(new TrainSetDisplayCell()
                {
                    ID = sortid++,
                    RegionID = 0,
                    CellType = EnumTrainSetCellType.Label,
                    SelectingUid = item.uid,
                    name = item.name,
                    color = Color.FromArgb(item.color),
                    classUid = string.IsNullOrEmpty(item.classUid) ? "" : item.classUid,
                    uid = item.uid,
                    labelType = item.labelType,
                    labelCheck = item.labelCheck,
                });
            }
            sortid = 0;
            trainSetDisplays.Add(new TrainSetDisplayCell()
            {
                ID = 100 + sortid++,
                RegionID = -1,
                CellType = EnumTrainSetCellType.ModelRoot,
                name = "模型".ToMultiLanguage(),
            });
            foreach (var item in set.models)
            {
                trainSetDisplays.Add(new TrainSetDisplayCell()
                {
                    ID = 100 + sortid++,
                    RegionID = 100,
                    CellType = EnumTrainSetCellType.Model,
                    name = item.name,
                    uid = item.uid,
                });
            }
            treeList1.FocusedNodeChanged -= treeList1_FocusedNodeChanged;
            treeList1.BeginUpdate();
            treeList1.RefreshDataSource();
            treeList1.ExpandAll();
            treeList1.EndUpdate();
            treeList1.FocusedNodeChanged += treeList1_FocusedNodeChanged;

            if (!string.IsNullOrEmpty(selectionUid))
            {
                //选中指定节点
                var selectedCell = trainSetDisplays.Find(i => i.uid == selectionUid);
                selectNodeID = selectedCell?.ID ?? 0;
            }
            if (selectNodeID != null)
            {
                //选中指定节点
                var selectedNode = treeList1.GetNodeList().Find(i => i["ID"].Equals((int)selectNodeID));
                treeList1.FocusedNode = selectedNode;
            }
        }

        /// <summary>
        /// 隐藏/展示标签图形
        /// </summary>
        /// <param name="state"></param>
        private void ShowLabel(bool state)
        {
            this.Invoke(new Action(() =>
            {
                var ass = GlobalSettings.ApplySetting;
                foreach (var temp in ass.traingSet.label.selections)
                {
                    imageViewWithTools1.SetSelectionVisible(temp.uid, state);
                }
            }));
        }

        /// <summary>
        /// 刷新启动和停止训练 状态
        /// </summary>
        /// <param name="run"></param>
        private void refreshTrainStatus(bool run)
        {
            if (run)
            {
                btnTrain.Text = "停止训练".ToMultiLanguage();
            }
            else
            {
                btnTrain.Text = "开始训练".ToMultiLanguage();
            }

        }

        /// <summary>
        /// 加载模型到图片
        /// </summary>
        private void LoadModel()
        {
            if (model == null)
                return;
            //对图像进行plsda预测，并更新颜色
            AllocCudaData();

            GlobalSettings.ApplySetting.stopAppFlag = false;
            GlobalSettings.ApplySetting.runingApp = "train";
            pridictionImage = new byte[spe.Lines][];
            pridictionImageOriginal = null; // 清空备份,下次勾选时会重新备份
            unifyControlsActivated = false;  // 重置激活标记
            this.Invoke(new Action(() => SetUnifyControlsEnabled(false)));
            if (model.ContourEnabled)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessQueueOutline));
            }
            else
            {
                this.Invoke(new Action(() => { pridictionImageViewTimer.Start(); }));
                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessQueue));
            }
        }

        /// <summary>
        /// 分配GPU运算资源
        /// </summary>
        private void AllocCudaData()
        {
            CudaAccelerator.Shared.AllocCALRefWhiteBlack(new float[1], new float[1]);
            CudaAccelerator.Shared.AllocCALRef(new float[1]);
        }

        private unsafe void GetTag(int count, int length)
        {
            var aim = new float[count, spe.SizeOfLine / 4];
            for (int j = 0; j < count; j++)
            {
                long offset = (long)((length * 100) + j) * spe.SizeOfLine;
                var stream = spe.Raw.CreateViewStream(offset, spe.SizeOfLine);
                byte* ptr = null;
                stream.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                ptr += stream.PointerOffset;
                var frame = new byte[spe.SizeOfLine];
                System.Runtime.InteropServices.Marshal.Copy((IntPtr)ptr, frame, 0, spe.SizeOfLine);
                var im = frame.ToFloat(spe.DataType);

                var size = sizeof(float);
                Buffer.BlockCopy(im, 0, aim, j * im.Length * size, im.Length * size);

                stream.SafeMemoryMappedViewHandle.ReleasePointer();
                stream.Dispose();
            }
            var tags = CudaAccelerator.Shared.PLSSClassifyNoCorrection(model, count, spe.Samples, aim);

            for (int n = 0; n < count; n++)
            {
                byte[] re = new byte[tags.GetLength(1)];
                Buffer.BlockCopy(tags, n * re.Length, re, 0, re.Length);
                pridictionImage[(length * 100) + n] = re;
            }
        }

        private unsafe void ProcessQueue(object dynamic)
        {
            try
            {
                CudaAccelerator.Shared.AllocPLSSClassify(model, 1, spe.Samples, needCalibration: false);//公共参数赋值

                if (model.ModelType == 0)
                {
                    int count = spe.Lines / 100;
                    int mod = spe.Lines % 100;

                    for (int i = 0; i < count; i++)
                    {
                        if (tokenSource.IsCancellationRequested)
                            return;
                        OnProgress(i * 100f / spe.Lines * 100, false);

                        GetTag(100, i);
                    }
                    if (mod > 0)
                    {
                        GetTag(mod, count);
                    }
                    OnProgress(100, true);
                }
                else
                {
                    //Parallel.For(0, spe.Lines, (int _x, ParallelLoopState pls) =>
                    for (int _x = 0; _x < spe.Lines; _x++)
                    {
                        long offset = (long)_x * spe.SizeOfLine;
                        var stream = spe.Raw.CreateViewStream(offset, spe.SizeOfLine);
                        byte* ptr = null;
                        stream.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                        ptr += stream.PointerOffset;
                        var frame = new byte[spe.SizeOfLine];
                        System.Runtime.InteropServices.Marshal.Copy((IntPtr)ptr, frame, 0, spe.SizeOfLine);

                        var im = frame.ToFloat(spe.DataType);

                        if (model.ModelType == 0)
                        {
                            float[,] aim = new float[1, im.Length];
                            for (int i = 0; i < im.Length; i++)
                            {
                                aim[0, i] = im[i];
                            }
                            var tags = CudaAccelerator.Shared.PLSSClassifyNoCorrection(model, 1, spe.Samples, aim);
                            byte[] re = new byte[tags.GetLength(1)];
                            for (int i = 0; i < tags.GetLength(1); i++)
                            {
                                re[i] = tags[0, i];
                            }
                            pridictionImage[_x] = re;

                        }
                        else
                        {
                            int length = model.EndBandIndex - model.StartBandIndex + 1;
                            float[,] averageRef = new float[spe.Samples, model.EndBandIndex - model.StartBandIndex + 1];
                            bool[] isBackground = new bool[spe.Samples];
                            Parallel.For(0, spe.Samples, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (int i) =>
                            {
                                int startIndex = model.StartBandIndex;
                                int endIndex = model.EndBandIndex;
                                var wave = new float[length];
                                for (int j = startIndex; j <= endIndex; j++)
                                {
                                    wave[j - startIndex] = im[i + j * spe.Samples];
                                }
                                if (wave.Sum() / wave.Length < 0.2f)
                                {
                                    isBackground[i] = true;
                                    for (int k = 0; k < length; k++)
                                    {
                                        averageRef[i, k] = 0;
                                    }
                                }
                                else
                                {
                                    if (model.Preprocessings.Count > 0)
                                    {
                                        wave = PreprocessingHelper.GetPreprocessingReflectSingle(wave, model.Preprocessings);
                                    }
                                    for (int k = 0; k < length; k++)
                                    {
                                        averageRef[i, k] = wave[k];
                                    }
                                }

                            });
                            if (model.ModelType == 1)
                            {
                                List<int> selectIndexs = GlobalSettings.ApplySetting.traingSet.algorithm.train_bands.Bands.Find(k => k.ClassId == 1).WaveLengthIndexs;
                                double[,] tempData = new double[spe.Samples, selectIndexs.Count];
                                for (int i = 0; i < spe.Samples; i++)
                                {
                                    for (int k = 0; k < selectIndexs.Count; k++)
                                    {
                                        if (selectIndexs[k] > -1)
                                        {
                                            tempData[i, k] = averageRef[i, selectIndexs[k] - model.StartBandIndex];
                                        }
                                    }
                                }
                                var tags = Svm.chsvm.Decide(tempData);
                                for (int i = 0; i < length; i++)
                                {
                                    if (isBackground[i])
                                        tags[i] = false;
                                }
                                pridictionImage[_x] = tags.Select(i => i ? (byte)1 : (byte)0).ToArray();
                            }
                            else if (model.ModelType == 2)
                            {
                                string saveDir = Path.Combine(Path.GetDirectoryName(model.hdr_spe_path), "Models\\Xml");
                                if (!Directory.Exists(saveDir))
                                    Directory.CreateDirectory(saveDir);

                                byte[] tags = new byte[spe.Samples];
                                foreach (var item in model.plss)
                                {
                                    if (string.IsNullOrEmpty(item.TrainFileName))
                                        continue;
                                    string file = Path.Combine(saveDir, item.TrainFileName);
                                    if (!File.Exists(file))
                                        continue;
                                    var svm = new OpenCVSvm(file);
                                    float[,] tempData = new float[spe.Samples, item.selectIndex.Count];
                                    for (int i = 0; i < spe.Samples; i++)
                                    {
                                        for (int k = 0; k < item.selectIndex.Count; k++)
                                        {
                                            if (item.selectIndex[k] > -1)
                                            {
                                                tempData[i, k] = averageRef[i, item.selectIndex[k] - model.StartBandIndex];
                                            }
                                        }
                                    }
                                    var res = svm.Predict(tempData);
                                    for (int i = 0; i < res.Length; i++)
                                    {
                                        if (res[i] > 0 && !isBackground[i])
                                        {
                                            tags[i] = (byte)item.classid;
                                        }
                                    }
                                }
                                pridictionImage[_x] = tags;
                            }

                        }

                        stream.SafeMemoryMappedViewHandle.ReleasePointer();
                        stream.Dispose();


                        if (GlobalSettings.ApplySetting.stopAppFlag)//结束分选
                        {
                            break;
                        }
                    }

                }

            }
            catch (Exception E)
            {
                Console.WriteLine("--------------------LoadModel" + E.Message);
            }
            finally
            {
                GlobalSettings.ApplySetting.stopAppFlag = true;
                GlobalSettings.ApplySetting.runingApp = string.Empty;
            }
        }

        private void ShowLoading(bool isShow)
        {
            this.Invoke(new Action(() =>
            {
                if (isShow)
                {
                    FormShowHelper.ShowLoadingForm(this, "应用模型中...".ToMultiLanguage(), second: 60 * 30, true);
                }
                else
                {
                    FormShowHelper.CloseLoadingForm();
                }
            }));
        }

        private unsafe void ProcessQueueOutline(object obj)
        {
            try
            {
                ShowLoading(true);
                GlobalSettings.ApplySetting.stopAppFlag = false;//停止分选

                GlobalSettings.ApplySetting.runingApp = "train";

                var channel = (model.ImgR, model.ImgG, model.ImgB);
                var threshold = (model.ImgThresholdR, model.ImgThresholdG, model.ImgThresholdB);
                Bitmap bitmap = SpeExtend.BackgroundCorrectionToBitmap(spe, channel, threshold, model.StartBandIndex, model.EndBandIndex, model.BackgroundCorrection, model.StartBackgroundThreshold, model.EndBackgroundThreshold);
                var contours = OpenCV.FindContourPoints(bitmap, model.ContourThreshold, model.ContourErode);
                if (contours?.Length > 0)
                {
                    float[,] averageRef = new float[contours.Length, model.EndBandIndex - model.StartBandIndex + 1];  //轮廓数 * 波段数
                    Parallel.For(0, contours.Length, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (int i) =>
                    {
                        var points = contours[i];
                        var ref1 = spectraOfSPE.GetAverageData(spe, contours[i]);
                        if (model.Preprocessings.Count > 0)
                        {
                            int startIndex = model.StartBandIndex;
                            int endIndex = model.EndBandIndex;
                            int length = endIndex - startIndex + 1;
                            var wave = new float[length];
                            //预处理
                            for (int p = startIndex; p <= endIndex; p++)
                            {
                                wave[p - startIndex] = ref1[p];
                            }
                            wave = PreprocessingHelper.GetPreprocessingReflectSingle(wave, model.Preprocessings);
                            for (int p = startIndex; p <= endIndex; p++)
                            {
                                ref1[p] = wave[p - startIndex];
                            }
                        }

                        for (int k = model.StartBandIndex; k <= model.EndBandIndex; k++)
                        {
                            averageRef[i, k - model.StartBandIndex] = ref1[k];
                        }
                    });

                    for (int i = 0; i < spe.Lines; i++)
                    {
                        pridictionImage[i] = new byte[spe.Samples];
                    }
                    if (model.ModelType == 0)
                    {
                        var tags = CudaAccelerator.Shared.OutlineClassify(contours.Length, averageRef);
                        for (int i = 0; i < contours.Length; i++)
                        {
                            if (tags[i] > 0)
                            {
                                foreach (var point in contours[i])
                                {
                                    pridictionImage[point.X][point.Y] = tags[i];
                                }
                            }
                        }
                    }
                    else if (model.ModelType == 1)
                    {
                        List<int> selectIndexs = GlobalSettings.ApplySetting.traingSet.algorithm.train_bands.Bands.Find(k => k.ClassId == 1).WaveLengthIndexs;
                        double[,] tempData = new double[contours.Length, selectIndexs.Count];
                        for (int i = 0; i < contours.Length; i++)
                        {
                            for (int k = 0; k < selectIndexs.Count; k++)
                            {
                                if (selectIndexs[k] > -1)
                                {
                                    tempData[i, k] = averageRef[i, selectIndexs[k] - model.StartBandIndex];
                                }
                            }
                        }
                        var tags = Svm.chsvm.Decide(tempData);
                        for (int i = 0; i < contours.Length; i++)
                        {
                            if (tags[i])
                            {
                                foreach (var point in contours[i])
                                {
                                    pridictionImage[point.X][point.Y] = 1;
                                }
                            }
                        }
                    }
                    else if (model.ModelType == 2)
                    {
                        string saveDir = Path.Combine(Path.GetDirectoryName(model.hdr_spe_path), "Models\\Xml");
                        if (!Directory.Exists(saveDir))
                            Directory.CreateDirectory(saveDir);

                        byte[] tags = new byte[contours.Length];
                        foreach (var item in model.plss)
                        {
                            if (string.IsNullOrEmpty(item.TrainFileName))
                                continue;
                            string file = Path.Combine(saveDir, item.TrainFileName);
                            if (!File.Exists(file))
                                continue;
                            var svm = new OpenCVSvm(file);
                            float[,] tempData = new float[contours.Length, item.selectIndex.Count];
                            for (int i = 0; i < contours.Length; i++)
                            {
                                for (int k = 0; k < item.selectIndex.Count; k++)
                                {
                                    if (item.selectIndex[k] > -1)
                                    {
                                        tempData[i, k] = averageRef[i, item.selectIndex[k] - model.StartBandIndex];
                                    }
                                }
                            }
                            var res = svm.Predict(tempData);
                            for (int i = 0; i < res.Length; i++)
                            {
                                if (res[i] > 0)
                                    tags[i] = (byte)item.classid;
                            }
                        }

                        for (int i = 0; i < contours.Length; i++)
                        {
                            if (tags[i] > 0)
                            {
                                foreach (var point in contours[i])
                                {
                                    pridictionImage[point.X][point.Y] = tags[i];
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {

                GlobalSettings.ApplySetting.stopAppFlag = true;
                GlobalSettings.ApplySetting.runingApp = string.Empty;

                this.Invoke(new Action(() =>
                {
                    PridictionImageViewTimer_Tick(null, null);
                    refreshTrainStatus(false);
                }));
                ShowLoading(false);

            }
        }


        #region 分类、模型，treeList控件事件

        private string highLightSelectUid = null;
        /// <summary>
        /// 显示折线图
        /// </summary>
        /// <param name="avg"></param>
        /// <param name="WaveLength"></param>
        /// <param name="color"></param>
        private void RefToChart(string selectuid = null)
        {
            if (!string.IsNullOrEmpty(selectuid))
            {
                highLightSelectUid = selectuid;
            }

            chartControl1.Titles[0].Text = "原始光谱".ToMultiLanguage();
            chartControl2.Titles[0].Text = "预处理后的光谱".ToMultiLanguage();
            var lst = GlobalSettings.ApplySetting.traingSet.Preprocessings.FindAll(i => i.Enabled);
            RefreshPreprocessChart(lst, highLightSelectUid);

            chartControl1.Series.Clear();

            TrainSetDisplayCell displayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (displayCell == null || displayCell.CellType != EnumTrainSetCellType.Label)
                return;
            //获取勾选的标签
            var disNodes = treeList1.GetAllCheckedNodes().FindAll(x => (EnumTrainSetCellType)x["CellType"] == EnumTrainSetCellType.Label);
            if (disNodes.Count() == 0)
            {
                return;
            }

            var set = GlobalSettings.ApplySetting.traingSet;
            List<Series> addSeries1 = new List<Series>();
            bool hasPreprocess = GlobalSettings.ApplySetting.traingSet.Preprocessings.Count > 0;
            int length = set.algorithm.EndBandIndex - set.algorithm.StartBandIndex + 1;
            foreach (var item in set.label.selections)
            {
                if (item.reflects.Count == 0)
                    continue;
                //判断是否勾选
                if (!disNodes.Exists(ppt => ppt["SelectingUid"].ToString() == item.uid))
                {
                    continue;
                }
                for (int i = 0; i < item.reflects.Count; i++)
                {

                    if (item.reflects.Count > Constant.OutlineShowLineNumber)
                    {
                        if (i % (item.reflects.Count / Constant.OutlineShowLineNumber) != 0)
                            continue;

                    }
                    float[] reflect1 = new float[length];
                    for (int s = 0; s < length; s++)
                    {
                        reflect1[s] = item.reflects[i][s + set.algorithm.StartBandIndex];
                    }

                    LineSeriesView lineSeriesView1 = new LineSeriesView();
                    lineSeriesView1.LineStyle.Thickness = highLightSelectUid == item.uid ? 4 : 2;
                    Color color = Color.FromArgb(item.color);
                    lineSeriesView1.Color = Color.FromArgb(highLightSelectUid == item.uid ? 255 : 160, color.R, color.G, color.B);

                    var series1 = new Series("", ViewType.Line) { View = lineSeriesView1 };
                    series1.CrosshairEnabled = addSeries1.Count == 0 ? DevExpress.Utils.DefaultBoolean.True : DevExpress.Utils.DefaultBoolean.False;
                    series1.CrosshairLabelPattern = "{A}";
                    for (int j = 0; j < reflect1.Length; j++)
                    {
                        series1.Points.Add(new SeriesPoint(Math.Round(spe.Hdr.WaveLength[j + set.algorithm.StartBandIndex], 2), Math.Round(reflect1[j], 4)));
                    }
                    if (highLightSelectUid == item.uid)
                    {
                        addSeries1.Insert(0, series1);//显示最上层
                    }
                    else
                    {
                        addSeries1.Add(series1);
                    }
                }
            }

            if (addSeries1.Count > 0)
            {
                addSeries1.Reverse();
                chartControl1.Series.AddRange(addSeries1.ToArray());
            }
            else
            {
                chartControl1.Series.Add(new Series("", ViewType.Line));
            }

            if (chartControl1.Diagram != null)
            {
                //放大缩小
                var diagram = chartControl1.Diagram as XYDiagram;
                diagram.EnableAxisXScrolling = true;
                diagram.EnableAxisXZooming = true;
                diagram.EnableAxisYScrolling = true;
                diagram.EnableAxisYZooming = true;
                diagram.DefaultPane.BackColor = Color.FromArgb(80, 80, 80);
            }
            chartControl1.Refresh();

        }

        /// <summary>
        /// 显示预处理折线图
        /// </summary>
        /// <param name="preprocessings"></param>
        /// <param name="selectuid"></param>
        private void RefreshPreprocessChart(List<Preprocessing> preprocessings, string selectuid)
        {
            if (!string.IsNullOrEmpty(selectuid))
            {
                highLightSelectUid = selectuid;
            }
           

            chartControl2.Series.Clear();
          
            TrainSetDisplayCell displayCell = treeList1.GetFocusedRow() as TrainSetDisplayCell;
            if (displayCell == null || displayCell.CellType != EnumTrainSetCellType.Label)
                return;

            //获取勾选的标签
            var disNodes = treeList1.GetAllCheckedNodes().FindAll(x => (EnumTrainSetCellType)x["CellType"] == EnumTrainSetCellType.Label);
            if (disNodes.Count() == 0)
            {
                return;
            }

            var set = GlobalSettings.ApplySetting.traingSet;
            List<Series> addSeries2 = new List<Series>();
            int length = set.algorithm.EndBandIndex - set.algorithm.StartBandIndex + 1;


            if (preprocessings?.Count > 0)
            {
                foreach (var item in set.label.selections)
                {
                    if (item.reflects.Count == 0)
                        continue;
                    //判断是否勾选
                    if (!disNodes.Exists(ppt => ppt["SelectingUid"].ToString() == item.uid))
                    {
                        continue;
                    }
                    float[][] tempReflect = new float[item.reflects.Count][];
                    for (int i = 0; i < item.reflects.Count; i++)
                    {
                        tempReflect[i] = new float[length];
                        for (int j = set.algorithm.StartBandIndex; j <= set.algorithm.EndBandIndex; j++)
                        {
                            tempReflect[i][j - set.algorithm.StartBandIndex] = item.reflects[i][j];
                        }
                    }
                    tempReflect = PreprocessingHelper.GetPreprocessingReflect(tempReflect, preprocessings);
                    for (int i = 0; i < tempReflect.Length; i++)
                    {
                        if (tempReflect.Length > Constant.OutlineShowLineNumber)
                        {
                            if (i % (tempReflect.Length / Constant.OutlineShowLineNumber) != 0)
                                continue;
                        }

                        Color color = Color.FromArgb(item.color);
                        LineSeriesView lineSeriesView2 = new LineSeriesView();
                        lineSeriesView2.LineStyle.Thickness = highLightSelectUid == item.uid ? 4 : 2;
                        lineSeriesView2.Color = Color.FromArgb(highLightSelectUid == item.uid ? 255 : 160, color.R, color.G, color.B);

                        var series2 = new Series("", ViewType.Line) { View = lineSeriesView2 };
                        series2.CrosshairEnabled = addSeries2.Count == 0 ? DevExpress.Utils.DefaultBoolean.True : DevExpress.Utils.DefaultBoolean.False;
                        series2.CrosshairLabelPattern = "{A}";
                        var reflect2 = tempReflect[i];
                        for (int j = 0; j < reflect2.Length; j++)
                        {
                            series2.Points.Add(new SeriesPoint(Math.Round(spe.Hdr.WaveLength[j + set.algorithm.StartBandIndex], 2), Math.Round(reflect2[j], 4)));
                        }
                        if (highLightSelectUid == item.uid)
                        {
                            //显示最上层
                            addSeries2.Insert(0, series2);
                        }
                        else
                        {
                            addSeries2.Add(series2);
                        }
                    }
                }

            }

            if (addSeries2.Count > 0)
            {
                addSeries2.Reverse();
                chartControl2.Series.AddRange(addSeries2.ToArray());
            }
            else
            {
                chartControl2.Series.Add(new Series("", ViewType.Line));
            }

            if (chartControl2.Diagram != null)
            {
                //放大缩小
                var diagram = chartControl2.Diagram as XYDiagram;
                diagram.EnableAxisXScrolling = true;
                diagram.EnableAxisXZooming = true;
                diagram.EnableAxisYScrolling = true;
                diagram.EnableAxisYZooming = true;
                diagram.DefaultPane.BackColor = Color.FromArgb(80, 80, 80);
            }

            chartControl2.Refresh();
        }


        #endregion

        #endregion

        /// <summary>
        /// 预览轮廓
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnContourImage_Click(object sender, EventArgs e)
        {
            FindCountour();
        }

        /// <summary>
        /// 寻找轮廓信息
        /// </summary>
        /// <param name="isShow"></param>
        private void FindCountour(bool isShow = true, bool isDrawImg = false)
        {
            if (isShow)
            {
                FormShowHelper.ShowLoadingForm(this, "计算轮廓中".ToMultiLanguage() + "...");
            }
            var set = GlobalSettings.ApplySetting.traingSet;
            var algorithm = set.algorithm;
            var channel = (set.ImgR, set.ImgG, set.ImgB);
            var threshold = (set.ImgThresholdR / 100, set.ImgThresholdG / 100, set.ImgThresholdB / 100);
            Bitmap bitmap = SpeExtend.BackgroundCorrectionToBitmap(spe, channel, threshold, algorithm.StartBandIndex, algorithm.EndBandIndex, algorithm.BackgroundCorrection, algorithm.StartBackgroundThreshold, algorithm.EndBackgroundThreshold);
            contourInfo = OpenCV.FindContourInfo(bitmap, algorithm.train_contour_threshold, algorithm.train_contour_erode, isShow, needRect: true, needAllPoints: true);
            if (isShow && isDrawImg)
            {
                ShowLabel(false);
                imageViewWithTools1.SetImage(contourInfo.PreviewImage, GlobalSettings.ApplySetting.StartSample, GlobalSettings.ApplySetting.EndSample);
                lcCtl.Text = "轮廓数：".ToMultiLanguage() + contourInfo.Count;
            }
            FormShowHelper.CloseLoadingForm();

        }

        /// <summary>
        /// 分选算法切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboAlgorithmType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var algorithm = (TypeAlgorithm)(cboAlgorithmType.GetSelectedValue<int>() ?? 0);
            GlobalSettings.ApplySetting.traingSet.algorithm.TypeAlgorithm = algorithm;
        }

        private void RefreshSelection()
        {
            var selections = GlobalSettings.ApplySetting.traingSet.label.selections;
            if (selections.Count == 0)
                return;

            bool isUpdate = false;
            FormShowHelper.ShowLoadingForm(this, "计算反射率中".ToMultiLanguage() + "...");
            bool contourEnabled = GlobalSettings.ApplySetting.traingSet.algorithm.train_contour_enabled;
            foreach (var item in selections)
            {
                item.ContourEnabled = contourEnabled;
                switch (item.labelType)
                {
                    case SelectionType.抠图:
                        isUpdate = true;
                        ClassTools.GetReflectBySelectMattingArea(item, spe, spectraOfSPE);
                        break;
                    case SelectionType.矩形:
                        isUpdate = true;
                        ClassTools.GetReflectBySelectRectArea(item, spe, spectraOfSPE);
                        break;
                    case SelectionType.点选区:
                        isUpdate = true;
                        ClassTools.GetReflectBySelectPointsArea(item, spe, spectraOfSPE);
                        break;
                    case SelectionType.轮廓:
                        isUpdate = true;
                        ClassTools.GetReflectBySelectOutlineArea(item, spe, spectraOfSPE);
                        break;
                }
            }
            if (isUpdate)
            {
                RefToChart();
            }

            FormShowHelper.CloseLoadingForm();
        }


        /// <summary>
        /// 导出标签csv
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (spe == null)
            {
                FormShowHelper.ShowMessage("请先选择光谱图像".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            var set = GlobalSettings.ApplySetting.traingSet;
            //判断选区信息
            if (set.label.selections.Count <= 1)
            {
                FormShowHelper.ShowMessage("选区数量少于两个，数量不够".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            else if (set.label.selections.Count(x => x.classID == 255 || x.classID == 254) < 1)
            {
                FormShowHelper.ShowMessage("缺少背景选区或未分类选区".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            else if (set.label.selections.Count(x => x.classID < 254 && x.classID > 0) < 1)
            {
                FormShowHelper.ShowMessage("缺少目标选区".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            else if (set.label.selections.Exists(x => x.classUid == ""))
            {
                FormShowHelper.ShowMessage("存在未标记类型的选区，请核实".ToMultiLanguage(), "提示".ToMultiLanguage());
                return;
            }
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择文件夹".ToMultiLanguage();
            // 设置是否显示“新建文件夹”按钮
            folderBrowserDialog.ShowNewFolderButton = true;
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                FormShowHelper.ShowLoadingForm(this, "导出中".ToMultiLanguage() + "...");
                ClassTools.WriteToCsvFile(folderBrowserDialog.SelectedPath, spe, new CancellationTokenSource());
                FormShowHelper.ShowMessage("导出完成".ToMultiLanguage(), "提示".ToMultiLanguage());
            }
        }

        //上下显示图表
        private void chkUpDownLoad_CheckedChanged(object sender, EventArgs e)
        {
            splitContainerControl1.Horizontal = !chkUpDownLoad.Checked;
        }

        //显示图表
        private void rdoBoth_CheckedChanged(object sender, EventArgs e)
        {
            splitContainerControl1.PanelVisibility = rdoBoth.Checked ? SplitPanelVisibility.Both : rdoPanel1.Checked ? SplitPanelVisibility.Panel1 : SplitPanelVisibility.Panel2;
        }


        #region 预处理相关

        private void gvConfig_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (e.Info.IsRowIndicator && e.RowHandle > -1)
            {
                e.Info.DisplayText = (e.RowHandle + 1).ToString();
            }
        }

        private void gvConfig_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column == colPreprocessType && e.Value != null)
            {
                e.DisplayText = ((PreprocessTypes)(int)e.Value).ToMultiLanguage<PreprocessTypes>();
            }
        }

        private void gvConfig_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            var data = e.Row as Preprocessing;
            if (data == null)
                return;
            if (e.Column == colPreprocessAlgorithm)
            {
                switch (data.PreprocessType)
                {
                    case PreprocessTypes.BaselineCorrection:
                        e.Value = ((EnumBaselineCorrection)data.PreprocessAlgorithm).ToMultiLanguage<EnumBaselineCorrection>();
                        break;
                    case PreprocessTypes.ScaleScalingAlgorithm:
                        e.Value = ((EnumScaleScalingAlgorithm)data.PreprocessAlgorithm).ToMultiLanguage<EnumScaleScalingAlgorithm>();
                        break;
                    case PreprocessTypes.SmoothingAlgorithm:
                        e.Value = ((EnumSmoothingAlgorithm)data.PreprocessAlgorithm).ToMultiLanguage<EnumSmoothingAlgorithm>();
                        break;
                }
            }
            else if (e.Column == colParams)
            {
                if (data.PreprocessType == PreprocessTypes.SmoothingAlgorithm)
                {
                    string msg = "滤波长度:".ToMultiLanguage() + data.FilterStrength.ToString();
                    if (data.PreprocessAlgorithm == (int)EnumSmoothingAlgorithm.SGSmoothing)
                    {
                        msg += ",滤波次数:".ToMultiLanguage() + data.FilterNumber.ToString();
                    }
                    e.Value = msg;
                }
            }
        }


        private void btnPreClearAll_ItemClick(object sender, ItemClickEventArgs e)
        {
            var lst = gcConfig.DataSource as List<Preprocessing>;
            if (lst.Count > 0)
            {
                lst.Clear();
                gcConfig.RefreshDataSource();
                RefreshPreprocessChart(null, highLightSelectUid);
            }
        }

        private void btnPreDelete_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (gvConfig.FocusedRowHandle < 0)
                return;
            var data = gvConfig.GetFocusedRow() as Preprocessing;
            var lst = gcConfig.DataSource as List<Preprocessing>;
            lst.Remove(data);
            gcConfig.RefreshDataSource();
            if (data.Enabled)
            {
                RefreshEnabledModeChart();
            }
        }

        private void btnPreEdit_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (gvConfig.FocusedRowHandle < 0)
                return;
            var data = gvConfig.GetFocusedRow() as Preprocessing;
            var form = new FormPreprocessEdit(data);
            if (FormShowHelper.ShowDialog(form) == DialogResult.OK)
            {
                gcConfig.RefreshDataSource();
                if (data.Enabled)
                {
                    RefreshEnabledModeChart();
                }
            }
        }

        private void RefreshEnabledModeChart()
        {
            var lst = gcConfig.DataSource as List<Preprocessing>;
            var usdLst = lst.FindAll(i => i.Enabled);
            RefreshPreprocessChart(usdLst, highLightSelectUid);
        }

        private void AddPreprocessModel()
        {
            var form = new FormPreprocessEdit(null);
            if (FormShowHelper.ShowDialog(form) == DialogResult.OK)
            {
                var lst = gcConfig.DataSource as List<Preprocessing>;
                lst.Add(form.Model);
                gcConfig.RefreshDataSource();
                var usdLst = lst.FindAll(i => i.Enabled);
                RefreshPreprocessChart(usdLst, highLightSelectUid);
            }
        }

        private void btnPreAdd_ItemClick(object sender, ItemClickEventArgs e)
        {
            AddPreprocessModel();
        }

        private void btnAddPreprocess_Click(object sender, EventArgs e)
        {
            AddPreprocessModel();
        }

        private void gvConfig_MouseUp(object sender, MouseEventArgs e)
        {
            var hitInfo = gvConfig.CalcHitInfo(e.Location);
            if (e.Button == MouseButtons.Right)
            {
                popupPreprocess.ShowPopup(MousePosition);
                btnPreEdit.Visibility = btnPreDelete.Visibility = hitInfo.InRow ? BarItemVisibility.Always : BarItemVisibility.Never;
            }
        }

        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (gvConfig.FocusedRowHandle < 0)
                return;
            var data = gvConfig.GetFocusedRow() as Preprocessing;
            data.Enabled = (sender as CheckEdit).Checked;
            RefreshEnabledModeChart();
        }



        #endregion


        private void treeList1_AfterCheckNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            TreeListNode node = e.Node;
            var type = (EnumTrainSetCellType)node["CellType"];
            if (type == EnumTrainSetCellType.LabelRoot)
            {
                if (!node.Checked)
                {
                    node.UncheckAll();
                    var set = GlobalSettings.ApplySetting.traingSet;
                    set.label.selections.ForEach(x => x.labelCheck =false);
                }
                else
                {
                    node.CheckAll();
                    var set = GlobalSettings.ApplySetting.traingSet;
                    set.label.selections.ForEach(x => x.labelCheck = true);
                }
            }
            else if (type == EnumTrainSetCellType.Label)
            {
                var set = GlobalSettings.ApplySetting.traingSet;
                var whitch = set.label.selections.First(x => x.uid == (string)node["SelectingUid"]);
                whitch.labelCheck = (bool)node["labelCheck"];
            }
            RefToChart();
        }

        private void treeList1_CustomDrawNodeCheckBox(object sender, DevExpress.XtraTreeList.CustomDrawNodeCheckBoxEventArgs e)
        {
            //Gets or sets a value as data source for the treelist control.
            //Gets or sets the object used as the data source for current treelist control.
            var lst = treeList1.DataSource as List<TrainSetDisplayCell>;
            if (lst == null || lst.Count == 0)
                return;
            //Represent the node of the treelist control.
            //Represents a node of the Tree List control.
            TreeListNode node = e.Node;

            var type = (EnumTrainSetCellType)node["CellType"];
            if (type == EnumTrainSetCellType.Model || type == EnumTrainSetCellType.ModelRoot)
            {
                e.Handled = true;
            }
            //Gets or Sets a value specify whether an event is handled and that default painting event is therefore handled.
            //Gets or sets a value specifing whether an event was handled and that the defaule element painting is therefore not required.
        }
    }
}
