
namespace FigSpec.SortingExpert.SettingForms
{
    partial class SplicingWhiteCalibrationForm
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
            this.imageViewWithTools1 = new ImgView.ImageViewWithTools();
            this.gcConfig = new DevExpress.XtraGrid.GridControl();
            this.gvConfig = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colColor = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemCheckEdit1 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.btnOkAndClose = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnDelete = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.picResult = new System.Windows.Forms.PictureBox();
            this.grpPreArea = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnStartScan = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnPause = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnCameraSetting = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnSaveAndSwitch = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.gcConfig)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvConfig)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picResult)).BeginInit();
            this.grpPreArea.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageViewWithTools1
            // 
            this.imageViewWithTools1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewWithTools1.Location = new System.Drawing.Point(3, 18);
            this.imageViewWithTools1.Name = "imageViewWithTools1";
            this.imageViewWithTools1.Size = new System.Drawing.Size(794, 471);
            this.imageViewWithTools1.TabIndex = 2;
            // 
            // gcConfig
            // 
            this.gcConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gcConfig.Location = new System.Drawing.Point(888, 97);
            this.gcConfig.MainView = this.gvConfig;
            this.gcConfig.Name = "gcConfig";
            this.gcConfig.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckEdit1});
            this.gcConfig.Size = new System.Drawing.Size(147, 448);
            this.gcConfig.TabIndex = 25;
            this.gcConfig.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvConfig});
            // 
            // gvConfig
            // 
            this.gvConfig.Appearance.Empty.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gvConfig.Appearance.Empty.Options.UseBackColor = true;
            this.gvConfig.Appearance.Row.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gvConfig.Appearance.Row.Options.UseBackColor = true;
            this.gvConfig.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colColor});
            this.gvConfig.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            this.gvConfig.GridControl = this.gcConfig;
            this.gvConfig.IndicatorWidth = 35;
            this.gvConfig.Name = "gvConfig";
            this.gvConfig.OptionsCustomization.AllowFilter = false;
            this.gvConfig.OptionsCustomization.AllowSort = false;
            this.gvConfig.OptionsMenu.EnableColumnMenu = false;
            this.gvConfig.OptionsSelection.CheckBoxSelectorColumnWidth = 60;
            this.gvConfig.OptionsSelection.MultiSelect = true;
            this.gvConfig.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CheckBoxRowSelect;
            this.gvConfig.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            this.gvConfig.OptionsView.ShowGroupPanel = false;
            this.gvConfig.OptionsView.ShowIndicator = false;
            this.gvConfig.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gvConfig_RowCellStyle);
            // 
            // colColor
            // 
            this.colColor.Caption = "  ";
            this.colColor.FieldName = "colColor";
            this.colColor.Name = "colColor";
            this.colColor.OptionsColumn.AllowEdit = false;
            this.colColor.Visible = true;
            this.colColor.VisibleIndex = 1;
            this.colColor.Width = 35;
            // 
            // repositoryItemCheckEdit1
            // 
            this.repositoryItemCheckEdit1.AutoHeight = false;
            this.repositoryItemCheckEdit1.Name = "repositoryItemCheckEdit1";
            // 
            // btnOkAndClose
            // 
            this.btnOkAndClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOkAndClose.AutoSize = true;
            this.btnOkAndClose.Location = new System.Drawing.Point(691, 3);
            this.btnOkAndClose.MinimumSize = new System.Drawing.Size(125, 23);
            this.btnOkAndClose.Name = "btnOkAndClose";
            this.btnOkAndClose.Size = new System.Drawing.Size(125, 23);
            this.btnOkAndClose.TabIndex = 26;
            this.btnOkAndClose.Text = "保存并关闭";
            this.btnOkAndClose.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.AutoSize = true;
            this.btnDelete.Location = new System.Drawing.Point(888, 68);
            this.btnDelete.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 27;
            this.btnDelete.Text = "删除选中";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // picResult
            // 
            this.picResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picResult.Location = new System.Drawing.Point(3, 18);
            this.picResult.Name = "picResult";
            this.picResult.Size = new System.Drawing.Size(63, 471);
            this.picResult.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picResult.TabIndex = 0;
            this.picResult.TabStop = false;
            // 
            // grpPreArea
            // 
            this.grpPreArea.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpPreArea.Controls.Add(this.picResult);
            this.grpPreArea.ForeColor = System.Drawing.Color.White;
            this.grpPreArea.Location = new System.Drawing.Point(10, 56);
            this.grpPreArea.Name = "grpPreArea";
            this.grpPreArea.Size = new System.Drawing.Size(69, 492);
            this.grpPreArea.TabIndex = 28;
            this.grpPreArea.TabStop = false;
            this.grpPreArea.Text = "预览区";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.btnStartScan);
            this.flowLayoutPanel1.Controls.Add(this.btnPause);
            this.flowLayoutPanel1.Controls.Add(this.btnCameraSetting);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(10, 14);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1019, 36);
            this.flowLayoutPanel1.TabIndex = 29;
            // 
            // btnStartScan
            // 
            this.btnStartScan.AutoSize = true;
            this.btnStartScan.Location = new System.Drawing.Point(1, 3);
            this.btnStartScan.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.btnStartScan.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnStartScan.Name = "btnStartScan";
            this.btnStartScan.Size = new System.Drawing.Size(75, 23);
            this.btnStartScan.TabIndex = 16;
            this.btnStartScan.Text = "开始采集";
            this.btnStartScan.Click += new System.EventHandler(this.btnStartScan_Click);
            // 
            // btnPause
            // 
            this.btnPause.AutoSize = true;
            this.btnPause.Location = new System.Drawing.Point(80, 3);
            this.btnPause.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.btnPause.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 18;
            this.btnPause.Text = "暂停";
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnCameraSetting
            // 
            this.btnCameraSetting.AutoSize = true;
            this.btnCameraSetting.Location = new System.Drawing.Point(161, 3);
            this.btnCameraSetting.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnCameraSetting.Name = "btnCameraSetting";
            this.btnCameraSetting.Size = new System.Drawing.Size(75, 23);
            this.btnCameraSetting.TabIndex = 19;
            this.btnCameraSetting.Text = "相机设置";
            this.btnCameraSetting.Click += new System.EventHandler(this.btnCameraSetting_Click);
            // 
            // btnSaveAndSwitch
            // 
            this.btnSaveAndSwitch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAndSwitch.AutoSize = true;
            this.btnSaveAndSwitch.Location = new System.Drawing.Point(560, 3);
            this.btnSaveAndSwitch.MinimumSize = new System.Drawing.Size(125, 23);
            this.btnSaveAndSwitch.Name = "btnSaveAndSwitch";
            this.btnSaveAndSwitch.Size = new System.Drawing.Size(125, 23);
            this.btnSaveAndSwitch.TabIndex = 30;
            this.btnSaveAndSwitch.Text = "保存并切换到白校准";
            this.btnSaveAndSwitch.Click += new System.EventHandler(this.btnSaveAndSwitch_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.Controls.Add(this.btnOkAndClose);
            this.flowLayoutPanel2.Controls.Add(this.btnSaveAndSwitch);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(213, 551);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(819, 31);
            this.flowLayoutPanel2.TabIndex = 31;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.imageViewWithTools1);
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(82, 56);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(800, 492);
            this.groupBox1.TabIndex = 32;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "选择校准区域，请点击图像";
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Tahoma", 25F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle.Location = new System.Drawing.Point(446, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(423, 38);
            this.lblTitle.TabIndex = 33;
            this.lblTitle.Text = "黑校准";
            // 
            // SplicingWhiteCalibrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1044, 594);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.grpPreArea);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.gcConfig);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "SplicingWhiteCalibrationForm";
            this.Text = "黑/白校准";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SplicingWhiteCalibrationForm_FormClosed);
            this.Load += new System.EventHandler(this.SplicingWhiteCalibrationForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gcConfig)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvConfig)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picResult)).EndInit();
            this.grpPreArea.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ImgView.ImageViewWithTools imageViewWithTools1;
        private DevExpress.XtraGrid.GridControl gcConfig;
        private DevExpress.XtraGrid.Views.Grid.GridView gvConfig;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit1;
        private BaseControl.SortingBaseButton btnOkAndClose;
        private BaseControl.SortingBaseButton btnDelete;
        private System.Windows.Forms.PictureBox picResult;
        private System.Windows.Forms.GroupBox grpPreArea;
        private DevExpress.XtraGrid.Columns.GridColumn colColor;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private BaseControl.SortingBaseButton btnStartScan;
        private BaseControl.SortingBaseButton btnPause;
        private BaseControl.SortingBaseButton btnSaveAndSwitch;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private BaseControl.SortingBaseButton btnCameraSetting;
        private System.Windows.Forms.GroupBox groupBox1;
        private DevExpress.XtraEditors.LabelControl lblTitle;
    }
}