
namespace FigSpec.SortingExpert.SettingForms
{
    partial class SelectTrainBandsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.chkShowToTrainForm = new DevExpress.XtraEditors.CheckEdit();
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.btnSetSelection = new DevExpress.XtraBars.BarButtonItem();
            this.btnSelectAll = new DevExpress.XtraBars.BarButtonItem();
            this.btnClearSelection = new DevExpress.XtraBars.BarButtonItem();
            this.btnReverseSelect = new DevExpress.XtraBars.BarButtonItem();
            this.btnSave = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.panelControl3 = new DevExpress.XtraEditors.PanelControl();
            this.chartControl1 = new DevExpress.XtraCharts.ChartControl();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.chkSameSelect = new DevExpress.XtraEditors.CheckEdit();
            this.lblClass = new DevExpress.XtraEditors.LabelControl();
            this.cboClassIds = new DevExpress.XtraEditors.ComboBoxEdit();
            this.checkedListBox = new DevExpress.XtraEditors.CheckedListBoxControl();
            this.popupMenu1 = new DevExpress.XtraBars.PopupMenu(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowToTrainForm.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).BeginInit();
            this.panelControl3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkSameSelect.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboClassIds.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkedListBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.chkShowToTrainForm);
            this.panelControl1.Controls.Add(this.btnSave);
            this.panelControl1.Controls.Add(this.panelControl3);
            this.panelControl1.Controls.Add(this.panelControl2);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1032, 619);
            this.panelControl1.TabIndex = 0;
            // 
            // chkShowToTrainForm
            // 
            this.chkShowToTrainForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShowToTrainForm.Location = new System.Drawing.Point(12, 586);
            this.chkShowToTrainForm.MenuManager = this.barManager1;
            this.chkShowToTrainForm.Name = "chkShowToTrainForm";
            this.chkShowToTrainForm.Properties.AutoWidth = true;
            this.chkShowToTrainForm.Properties.Caption = "选中波长显示到主界面图表";
            this.chkShowToTrainForm.Size = new System.Drawing.Size(166, 19);
            this.chkShowToTrainForm.TabIndex = 4;
            this.chkShowToTrainForm.Visible = false;
            // 
            // barManager1
            // 
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.btnSetSelection,
            this.btnSelectAll,
            this.btnClearSelection,
            this.btnReverseSelect});
            this.barManager1.MaxItemId = 6;
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1032, 0);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 619);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1032, 0);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 0);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 619);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1032, 0);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 619);
            // 
            // btnSetSelection
            // 
            this.btnSetSelection.Caption = "选择选中";
            this.btnSetSelection.Id = 0;
            this.btnSetSelection.Name = "btnSetSelection";
            this.btnSetSelection.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSetSelection_ItemClick);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Caption = "全选";
            this.btnSelectAll.Id = 1;
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSelectAll_ItemClick);
            // 
            // btnClearSelection
            // 
            this.btnClearSelection.Caption = "清空";
            this.btnClearSelection.Id = 2;
            this.btnClearSelection.Name = "btnClearSelection";
            this.btnClearSelection.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnClearSelection_ItemClick);
            // 
            // btnReverseSelect
            // 
            this.btnReverseSelect.Caption = "反选";
            this.btnReverseSelect.Id = 3;
            this.btnReverseSelect.Name = "btnReverseSelect";
            this.btnReverseSelect.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnReverseSelect_ItemClick);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnSave.Appearance.Options.UseForeColor = true;
            this.btnSave.Location = new System.Drawing.Point(908, 584);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(102, 23);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "确认";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // panelControl3
            // 
            this.panelControl3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl3.Controls.Add(this.chartControl1);
            this.panelControl3.Location = new System.Drawing.Point(265, 5);
            this.panelControl3.Name = "panelControl3";
            this.panelControl3.Size = new System.Drawing.Size(762, 566);
            this.panelControl3.TabIndex = 1;
            // 
            // chartControl1
            // 
            this.chartControl1.AppearanceNameSerializable = "Dark Flat";
            this.chartControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this.chartControl1.BorderOptions.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.chartControl1.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;
            this.chartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartControl1.Legend.Name = "Default Legend";
            this.chartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            this.chartControl1.Location = new System.Drawing.Point(2, 2);
            this.chartControl1.Name = "chartControl1";
            this.chartControl1.PaletteBaseColorNumber = 2;
            this.chartControl1.SeriesSerializable = new DevExpress.XtraCharts.Series[0];
            this.chartControl1.Size = new System.Drawing.Size(758, 562);
            this.chartControl1.TabIndex = 1;
            this.chartControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.chartControl1_Paint);
            this.chartControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chartControl1_MouseDown);
            this.chartControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.chartControl1_MouseMove);
            this.chartControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.chartControl1_MouseUp);
            // 
            // panelControl2
            // 
            this.panelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelControl2.Controls.Add(this.chkSameSelect);
            this.panelControl2.Controls.Add(this.lblClass);
            this.panelControl2.Controls.Add(this.cboClassIds);
            this.panelControl2.Controls.Add(this.checkedListBox);
            this.panelControl2.Location = new System.Drawing.Point(5, 5);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(254, 566);
            this.panelControl2.TabIndex = 0;
            // 
            // chkSameSelect
            // 
            this.chkSameSelect.Location = new System.Drawing.Point(13, 12);
            this.chkSameSelect.MenuManager = this.barManager1;
            this.chkSameSelect.Name = "chkSameSelect";
            this.chkSameSelect.Properties.AutoWidth = true;
            this.chkSameSelect.Properties.Caption = "所有标签使用相同选择的波段";
            this.chkSameSelect.Size = new System.Drawing.Size(178, 19);
            this.chkSameSelect.TabIndex = 5;
            this.chkSameSelect.CheckedChanged += new System.EventHandler(this.chkSameSelect_CheckedChanged);
            // 
            // lblClass
            // 
            this.lblClass.Location = new System.Drawing.Point(13, 42);
            this.lblClass.Name = "lblClass";
            this.lblClass.Size = new System.Drawing.Size(24, 14);
            this.lblClass.TabIndex = 2;
            this.lblClass.Text = "标签";
            // 
            // cboClassIds
            // 
            this.cboClassIds.Location = new System.Drawing.Point(54, 39);
            this.cboClassIds.MenuManager = this.barManager1;
            this.cboClassIds.Name = "cboClassIds";
            this.cboClassIds.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboClassIds.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboClassIds.Size = new System.Drawing.Size(167, 20);
            this.cboClassIds.TabIndex = 1;
            // 
            // checkedListBox
            // 
            this.checkedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBox.Appearance.BackColor = System.Drawing.Color.Gray;
            this.checkedListBox.Appearance.ForeColor = System.Drawing.Color.White;
            this.checkedListBox.Appearance.Options.UseBackColor = true;
            this.checkedListBox.Appearance.Options.UseForeColor = true;
            this.checkedListBox.Location = new System.Drawing.Point(2, 69);
            this.checkedListBox.Name = "checkedListBox";
            this.checkedListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.checkedListBox.Size = new System.Drawing.Size(250, 495);
            this.checkedListBox.TabIndex = 0;
            this.checkedListBox.ItemCheck += new DevExpress.XtraEditors.Controls.ItemCheckEventHandler(this.checkedListBox_ItemCheck);
            this.checkedListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.checkedListBox_MouseDown);
            // 
            // popupMenu1
            // 
            this.popupMenu1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.btnSetSelection),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnSelectAll),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnReverseSelect),
            new DevExpress.XtraBars.LinkPersistInfo(this.btnClearSelection)});
            this.popupMenu1.Manager = this.barManager1;
            this.popupMenu1.Name = "popupMenu1";
            // 
            // SelectTrainBandsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1032, 619);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.MaximizeBox = true;
            this.Name = "SelectTrainBandsForm";
            this.Text = "选择训练波段";
            this.Load += new System.EventHandler(this.SelectTrainBandsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowToTrainForm.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).EndInit();
            this.panelControl3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.panelControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkSameSelect.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboClassIds.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkedListBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.popupMenu1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl3;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraEditors.CheckedListBoxControl checkedListBox;
        private DevExpress.XtraCharts.ChartControl chartControl1;
        private BaseControl.SortingBaseButton btnSave;
        private DevExpress.XtraBars.PopupMenu popupMenu1;
        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem btnSetSelection;
        private DevExpress.XtraBars.BarButtonItem btnSelectAll;
        private DevExpress.XtraBars.BarButtonItem btnClearSelection;
        private DevExpress.XtraBars.BarButtonItem btnReverseSelect;
        private DevExpress.XtraEditors.CheckEdit chkShowToTrainForm;
        private DevExpress.XtraEditors.ComboBoxEdit cboClassIds;
        private DevExpress.XtraEditors.LabelControl lblClass;
        private DevExpress.XtraEditors.CheckEdit chkSameSelect;
    }
}