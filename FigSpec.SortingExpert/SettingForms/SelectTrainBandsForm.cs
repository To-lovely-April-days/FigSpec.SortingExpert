using DevExpress.XtraCharts;
using DevExpress.XtraEditors.Controls;
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

namespace FigSpec.SortingExpert.SettingForms
{
    public partial class SelectTrainBandsForm : BaseFormSetting
    {
        public TrainBands trainBands { get; set; }

        int oldClassId = -1;
        public SelectTrainBandsForm(TrainBands trainBands)
        {
            this.trainBands = trainBands.DeepClone();
            InitializeComponent();
        }

        private void SelectTrainBandsForm_Load(object sender, EventArgs e)
        {
            chkShowToTrainForm.Checked = trainBands.ShowToTrainForm;
            chkSameSelect.Checked = trainBands.SelectSameBand;
            var set = GlobalSettings.ApplySetting.traingSet;
            List<ValueTextItem<int>> classIds = new List<ValueTextItem<int>>();
            foreach (var labelClass in set.label.classes)
            {
                if (!labelClass.CanDelete)
                    continue;
                if (!set.label.selections.Exists(i => i.classID == labelClass.id))
                    continue;
                classIds.Add(new ValueTextItem<int>(labelClass.id, labelClass.name));
            }
            if (classIds.Count > 0)
            {
                cboClassIds.BindComboBoxItem(classIds);
                cboClassIds.SelectedIndex = 0;
                oldClassId = cboClassIds.GetSelectedValue<int>() ?? 0;
            }
            if (trainBands.WaveLength == null)
                return;

            var bands = trainBands.Bands.Find(i => i.ClassId == oldClassId);
            checkedListBox.BeginUpdate();
            //bands 为null，默认是全选
            int end = set.algorithm.EndBandIndex+ 1;
            for (int i = set.algorithm.StartBandIndex; i < end; i++)
            {
                var state = bands == null || bands.WaveLengthIndexs.Contains(i) ? CheckState.Checked : CheckState.Unchecked;
                checkedListBox.Items.Add(i, $"{i}: {trainBands.WaveLength[i].ToString("F2")}", state, true);
            }
            checkedListBox.EndUpdate();
            RefToChart();
            cboClassIds.SelectedIndexChanged += cboClassIds_SelectedIndexChanged;

        }


        public void RefToChart()
        {
            var set = GlobalSettings.ApplySetting.traingSet;
            if (set.label.selections.Count == 0)
                return;

            var obj = cboClassIds.GetSelectedValue<int>();
            int classId = (int)(obj ?? 0);
            chartControl1.Series.Clear();
            int length = set.algorithm.EndBandIndex - set.algorithm.StartBandIndex + 1;
            List<Series> addSeries = new List<Series>();

            foreach (var item in set.label.selections)
            {
                for (int i = 0; i < item.reflects.Count; i++)
                {
                    if (item.reflects.Count > Constant.OutlineShowLineNumber)
                    {
                        if (i % (item.reflects.Count / Constant.OutlineShowLineNumber) != 0)
                            continue;
                    }
                    float[] reflect = new float[length];
                    for (int s = 0; s < length; s++)
                    {
                        reflect[s] = item.reflects[i][s + set.algorithm.StartBandIndex];
                    }
  
                    LineSeriesView lineSeriesView1 = new LineSeriesView();
                    lineSeriesView1.LineStyle.Thickness = classId == item.classID ? 5 : 2;
                    Color color = Color.FromArgb(item.color);
                    lineSeriesView1.Color = Color.FromArgb(classId == item.classID ? 255 : 160, color.R, color.G, color.B);
                    var series = new Series("", ViewType.Line) { View = lineSeriesView1 };
                    series.CrosshairEnabled = addSeries.Count == 0 ? DevExpress.Utils.DefaultBoolean.True : DevExpress.Utils.DefaultBoolean.False;
                    series.CrosshairLabelPattern = "{A}";
                    
                    var ref1 = PreprocessingHelper.GetPreprocessingReflectSingle(reflect, GlobalSettings.ApplySetting.traingSet.Preprocessings);
                    for (int j = 0; j < ref1.Length; j++)
                    {
                        series.Points.Add(new SeriesPoint(Math.Round(trainBands.WaveLength[j + set.algorithm.StartBandIndex], 2), Math.Round(ref1[j], 4)));
                    }
                    if (classId == item.classID)
                    {
                        //显示最上层
                        addSeries.Insert(0, series);
                    }
                    else
                    {
                        addSeries.Add(series);
                    }
                }
            }
            if (addSeries.Count > 0)
            {
                addSeries.Reverse();
                chartControl1.Series.AddRange(addSeries.ToArray());
            }
            else
            {
                chartControl1.Series.Add(new Series("", ViewType.Line));
            }
            if (chartControl1.Diagram != null)
            {
                var diagram = chartControl1.Diagram as XYDiagram;
                diagram.EnableAxisXScrolling = true;
                diagram.EnableAxisXZooming = true;
                diagram.EnableAxisYScrolling = true;
                diagram.EnableAxisYZooming = true;
                diagram.DefaultPane.BackColor = Color.FromArgb(80, 80, 80);
            }
            checkedListBox_ItemCheck(null, null);
        }

        private void SaveCurrentClassIdData(int classId = -1, bool saveToOther = false)
        {
            if (classId == -1)
            {
                var obj = cboClassIds.GetSelectedValue<int>();
                if (obj == null)
                    return;
                classId = (int)obj;
            }

            var band = trainBands.Bands.Find(i => i.ClassId == classId);
            if (band == null)
            {
                band = new TrainBand() { ClassId = classId };
                trainBands.Bands.Add(band);
            }
            band.WaveLengthIndexs.Clear();
            foreach (CheckedListBoxItem item in checkedListBox.Items)
            {
                if (item.CheckState == CheckState.Checked)
                {
                    band.WaveLengthIndexs.Add((int)item.Value);
                }
            }
            if (saveToOther && chkSameSelect.Checked)
            {
                //同步到其他标签
                foreach (var item in cboClassIds.Properties.Items)
                {
                    if (item is ValueTextItem<int> vt)
                    {
                        if (vt.Value == classId)
                            continue;
                        SaveCurrentClassIdData(vt.Value);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            trainBands.ShowToTrainForm = chkShowToTrainForm.Checked;
            trainBands.SelectSameBand = chkSameSelect.Checked;
            SaveCurrentClassIdData(saveToOther: true);
            DialogResult = DialogResult.OK;
            Close();
        }

 
        private void btnSelectAll_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetItemCheck(true);
        }

        private void btnReverseSelect_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (checkedListBox.ItemCount == 0)
                return;
            itemCheckCancel = true;
            checkedListBox.BeginUpdate();
            foreach (CheckedListBoxItem item in checkedListBox.Items)
            {
                if (item.CheckState == CheckState.Checked)
                {
                    item.CheckState = CheckState.Unchecked;
                }
                else
                {
                    item.CheckState = CheckState.Checked;
                }
            }
            checkedListBox.EndUpdate();
            itemCheckCancel = false;
            checkedListBox_ItemCheck(null, null);
        }

        private void btnClearSelection_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SetItemCheck(false);
        }

        private void btnSetSelection_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            itemCheckCancel = true;
            checkedListBox.CheckSelectedItems();
            itemCheckCancel = false;
            checkedListBox_ItemCheck(null, null);
        }

        /// <summary>
        /// 设置所有
        /// </summary>
        /// <param name="isCheck"></param>
        private void SetItemCheck(bool isCheck)
        {
            if (checkedListBox.ItemCount == 0)
                return;
            itemCheckCancel = true;
            for (int i = 0; i < checkedListBox.ItemCount; i++)
            {
                checkedListBox.SetItemChecked(i, isCheck);
            }
            itemCheckCancel = false;
            checkedListBox_ItemCheck(null, null);
        }

        private void checkedListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                btnSetSelection.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
                popupMenu1.ShowPopup(MousePosition);
            }
        }

        private bool isDragging;
        private Point dragStartPoint;
        private Rectangle selectionRect;

        private void chartControl1_MouseDown(object sender, MouseEventArgs e)
        {
            // 检查是否按下了 Control 键
            if (e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.Control)
            {
                // 记录鼠标点击的位置
                dragStartPoint = e.Location;
                selectionRect = new Rectangle(e.Location.X, e.Y, 1, 1);
                isDragging = true;
            }
            if (e.Button == MouseButtons.Right)
            {
                btnSetSelection.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
                popupMenu1.ShowPopup(MousePosition);
            }
        }

        private void chartControl1_MouseMove(object sender, MouseEventArgs e)
        {
            // 如果正在拖动并且按住了 Control 键
            if (isDragging)
            {
                // 计算选择框的位置和大小
                int x = Math.Min(dragStartPoint.X, e.X);
                int y = Math.Min(dragStartPoint.Y, e.Y);
                int width = Math.Abs(dragStartPoint.X - e.X);
                int height = Math.Abs(dragStartPoint.Y - e.Y);
                selectionRect = new Rectangle(x, y, width, height);
            }
        }

        private void chartControl1_Paint(object sender, PaintEventArgs e)
        {
            // 绘制选择框
            if (isDragging)
            {
                using (Pen pen = new Pen(Color.Orange, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }
            }
        }

        private void chartControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                SetSelectedAreaCheck();
            }
        }

        /// <summary>
        /// 设置选中区域X轴对应波长是否选中
        /// </summary>
        /// <param name="isCheck"></param>
        private void SetSelectedAreaCheck()
        {
            if (chartControl1.Diagram != null)
            {
                var diagram = chartControl1.Diagram as XYDiagram;
                // 获取选择区域在 X 轴上的数值范围
                double startX = Math.Floor(diagram.PointToDiagram(selectionRect.Location).NumericalArgument);
                double endX = Math.Ceiling(diagram.PointToDiagram(new Point(selectionRect.Right, selectionRect.Bottom)).NumericalArgument);
                itemCheckCancel = true;
                checkedListBox.BeginUpdate();

                
                int startIndex = GlobalSettings.ApplySetting.traingSet.algorithm.StartBandIndex;
                var changeBands = trainBands.WaveLength.Select((v, x) => new { Value = v, Index = x }).Where(i => i.Value >= startX && i.Value <= endX).ToList();
                if (changeBands.Count > 0)
                {
                    int lastIndex = 0;
                    foreach (var band in changeBands)
                    {
                        int i = band.Index - startIndex;
                        if (i < 0 || i >= checkedListBox.Items.Count)
                            continue;
                        var item = checkedListBox.Items[i];
                        item.CheckState = item.CheckState == CheckState.Checked ? CheckState.Unchecked : CheckState.Checked;
                        lastIndex = i;
                    }
                    checkedListBox.MakeItemVisible(lastIndex);
                }
                checkedListBox.EndUpdate();
                itemCheckCancel = false;
                checkedListBox_ItemCheck(null, null);
  
            }

            isDragging = false;
            selectionRect = Rectangle.Empty;
        }

        private void AddStripLines()
        {
            if (chartControl1.Diagram == null)
                return;

            var diagram = chartControl1.Diagram as XYDiagram;
            diagram.AxisX.Strips.Clear();

            TrainFormHelper helper = new TrainFormHelper();
            int startIndex = -1;
            int endIndex = -1;
            for (int i = 0; i < checkedListBox.ItemCount; i++)
            {
                if (startIndex == -1 && checkedListBox.Items[i].CheckState == CheckState.Checked)
                {
                    startIndex = (int)checkedListBox.Items[i].Value;
                }
                else if (startIndex != -1 && endIndex == -1 && checkedListBox.Items[i].CheckState != CheckState.Checked)
                {
                    endIndex = (int)checkedListBox.Items[i].Value;
                    helper.AddStrip(diagram, trainBands, startIndex, endIndex);
                    startIndex = -1;
                    endIndex = -1;
                }
            }
            if (startIndex != -1 && endIndex == -1)
            {
                endIndex = (int)checkedListBox.Items[checkedListBox.ItemCount - 1].Value;
                helper.AddStrip(diagram, trainBands, startIndex, endIndex);
            }
        }

        bool itemCheckCancel = false;
        private void checkedListBox_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
        {
            if (itemCheckCancel)
                return;
            AddStripLines();
            chartControl1.RefreshData();
        }

        private void cboClassIds_SelectedIndexChanged(object sender, EventArgs e)
        {
            var obj = cboClassIds.GetSelectedValue<int>();
            if (obj == null || trainBands.WaveLength == null)
                return;
            SaveCurrentClassIdData(oldClassId);
            oldClassId = (int)obj;

            itemCheckCancel = true;
            var bands = trainBands.Bands.Find(i => i.ClassId == oldClassId);
            var set = GlobalSettings.ApplySetting.traingSet;
            checkedListBox.BeginUpdate();
            //bands 为null，默认是全选
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                int index = (int)checkedListBox.Items[i].Value;
                var state = bands == null || bands.WaveLengthIndexs.Contains(index) ? CheckState.Checked : CheckState.Unchecked;
                checkedListBox.Items[i].CheckState = state;
            }

            checkedListBox.EndUpdate();
            itemCheckCancel = false;

            RefToChart();
        }

        private void chkSameSelect_CheckedChanged(object sender, EventArgs e)
        {
            cboClassIds.Enabled = !chkSameSelect.Checked;
        }


    }
}
