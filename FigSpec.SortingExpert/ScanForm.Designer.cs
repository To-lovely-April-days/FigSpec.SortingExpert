
namespace FigSpec.SortingExpert
{
    partial class ScanForm
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
            DevExpress.XtraCharts.XYDiagram xyDiagram1 = new DevExpress.XtraCharts.XYDiagram();
            DevExpress.XtraCharts.Series series1 = new DevExpress.XtraCharts.Series();
            DevExpress.XtraCharts.StackedLineSeriesView stackedLineSeriesView1 = new DevExpress.XtraCharts.StackedLineSeriesView();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.panelControl3 = new DevExpress.XtraEditors.PanelControl();
            this.imageViewWithTools1 = new ImgView.ImageViewWithTools();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.labFrameRate = new DevExpress.XtraEditors.LabelControl();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnConnectCamera = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnStartScan = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnPause = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnCameraSetting = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btndisplayandcalibsetting = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btn_SeleteBlack = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btn_SelectWhite = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnsaveimage = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this.hideContainerBottom = new DevExpress.XtraBars.Docking.AutoHideContainer();
            this.dockPanel1 = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gridControl_GuangPu = new DevExpress.XtraGrid.GridControl();
            this.gridView_GuangPu = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.colGUANGPUBIANHAO = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colYANSE = new DevExpress.XtraGrid.Columns.GridColumn();
            this.chartControl2 = new DevExpress.XtraCharts.ChartControl();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlStart = new System.Windows.Forms.Panel();
            this.lblStartWaveLength = new DevExpress.XtraEditors.LabelControl();
            this.cboStartWaveLength = new DevExpress.XtraEditors.ComboBoxEdit();
            this.pnlEnd = new System.Windows.Forms.Panel();
            this.lblEndWaveLength = new DevExpress.XtraEditors.LabelControl();
            this.cboEndWaveLength = new DevExpress.XtraEditors.ComboBoxEdit();
            this.cb_ReflectanceDisplay = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.btn_DeleteCheck = new DevExpress.XtraEditors.SimpleButton();
            this.btn_DeleteAll = new DevExpress.XtraEditors.SimpleButton();
            this.btn_SaveAllToFile = new DevExpress.XtraEditors.SimpleButton();
            this.btn_SaveToFile = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).BeginInit();
            this.panelControl3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.hideContainerBottom.SuspendLayout();
            this.dockPanel1.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_GuangPu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_GuangPu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(stackedLineSeriesView1)).BeginInit();
            this.flowLayoutPanel2.SuspendLayout();
            this.pnlStart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboStartWaveLength.Properties)).BeginInit();
            this.pnlEnd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboEndWaveLength.Properties)).BeginInit();
            this.flowLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.panelControl3);
            this.panelControl1.Controls.Add(this.panelControl2);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(1286, 753);
            this.panelControl1.TabIndex = 2;
            // 
            // panelControl3
            // 
            this.panelControl3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl3.Controls.Add(this.imageViewWithTools1);
            this.panelControl3.Location = new System.Drawing.Point(5, 53);
            this.panelControl3.Name = "panelControl3";
            this.panelControl3.Size = new System.Drawing.Size(1276, 695);
            this.panelControl3.TabIndex = 2;
            // 
            // imageViewWithTools1
            // 
            this.imageViewWithTools1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageViewWithTools1.Location = new System.Drawing.Point(2, 2);
            this.imageViewWithTools1.Name = "imageViewWithTools1";
            this.imageViewWithTools1.Size = new System.Drawing.Size(1272, 691);
            this.imageViewWithTools1.TabIndex = 1;
            // 
            // panelControl2
            // 
            this.panelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl2.Controls.Add(this.labFrameRate);
            this.panelControl2.Controls.Add(this.flowLayoutPanel1);
            this.panelControl2.Location = new System.Drawing.Point(5, 5);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(1276, 42);
            this.panelControl2.TabIndex = 1;
            // 
            // labFrameRate
            // 
            this.labFrameRate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labFrameRate.Appearance.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold);
            this.labFrameRate.Appearance.Options.UseFont = true;
            this.labFrameRate.Appearance.Options.UseTextOptions = true;
            this.labFrameRate.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.labFrameRate.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labFrameRate.Location = new System.Drawing.Point(950, 6);
            this.labFrameRate.Name = "labFrameRate";
            this.labFrameRate.Size = new System.Drawing.Size(319, 26);
            this.labFrameRate.TabIndex = 4;
            this.labFrameRate.Text = "当前帧率：3000fps";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.btnConnectCamera);
            this.flowLayoutPanel1.Controls.Add(this.btnStartScan);
            this.flowLayoutPanel1.Controls.Add(this.btnPause);
            this.flowLayoutPanel1.Controls.Add(this.btnCameraSetting);
            this.flowLayoutPanel1.Controls.Add(this.btndisplayandcalibsetting);
            this.flowLayoutPanel1.Controls.Add(this.btn_SeleteBlack);
            this.flowLayoutPanel1.Controls.Add(this.btn_SelectWhite);
            this.flowLayoutPanel1.Controls.Add(this.btnsaveimage);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1264, 32);
            this.flowLayoutPanel1.TabIndex = 9;
            // 
            // btnConnectCamera
            // 
            this.btnConnectCamera.AutoSize = true;
            this.btnConnectCamera.Location = new System.Drawing.Point(3, 3);
            this.btnConnectCamera.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnConnectCamera.Name = "btnConnectCamera";
            this.btnConnectCamera.Size = new System.Drawing.Size(75, 23);
            this.btnConnectCamera.TabIndex = 8;
            this.btnConnectCamera.Text = "连接相机";
            this.btnConnectCamera.Click += new System.EventHandler(this.btnConnectCamera_Click);
            // 
            // btnStartScan
            // 
            this.btnStartScan.AutoSize = true;
            this.btnStartScan.Location = new System.Drawing.Point(82, 3);
            this.btnStartScan.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.btnStartScan.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnStartScan.Name = "btnStartScan";
            this.btnStartScan.Size = new System.Drawing.Size(75, 23);
            this.btnStartScan.TabIndex = 0;
            this.btnStartScan.Text = "开始采集";
            this.btnStartScan.Click += new System.EventHandler(this.btnStartScan_Click);
            // 
            // btnPause
            // 
            this.btnPause.AutoSize = true;
            this.btnPause.Location = new System.Drawing.Point(161, 3);
            this.btnPause.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.btnPause.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 15;
            this.btnPause.Text = "暂停";
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnCameraSetting
            // 
            this.btnCameraSetting.AutoSize = true;
            this.btnCameraSetting.Location = new System.Drawing.Point(247, 3);
            this.btnCameraSetting.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.btnCameraSetting.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnCameraSetting.Name = "btnCameraSetting";
            this.btnCameraSetting.Size = new System.Drawing.Size(75, 23);
            this.btnCameraSetting.TabIndex = 5;
            this.btnCameraSetting.Text = "相机设置";
            this.btnCameraSetting.Click += new System.EventHandler(this.btnCameraSetting_Click);
            // 
            // btndisplayandcalibsetting
            // 
            this.btndisplayandcalibsetting.AutoSize = true;
            this.btndisplayandcalibsetting.Location = new System.Drawing.Point(326, 3);
            this.btndisplayandcalibsetting.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.btndisplayandcalibsetting.MinimumSize = new System.Drawing.Size(75, 23);
            this.btndisplayandcalibsetting.Name = "btndisplayandcalibsetting";
            this.btndisplayandcalibsetting.Size = new System.Drawing.Size(93, 23);
            this.btndisplayandcalibsetting.TabIndex = 14;
            this.btndisplayandcalibsetting.Text = "校准和保存设置";
            this.btndisplayandcalibsetting.Click += new System.EventHandler(this.btndisplayandcalibsetting_Click);
            // 
            // btn_SeleteBlack
            // 
            this.btn_SeleteBlack.AutoSize = true;
            this.btn_SeleteBlack.Location = new System.Drawing.Point(430, 3);
            this.btn_SeleteBlack.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.btn_SeleteBlack.MinimumSize = new System.Drawing.Size(75, 23);
            this.btn_SeleteBlack.Name = "btn_SeleteBlack";
            this.btn_SeleteBlack.Size = new System.Drawing.Size(75, 23);
            this.btn_SeleteBlack.TabIndex = 10;
            this.btn_SeleteBlack.Text = "黑校准";
            this.btn_SeleteBlack.Click += new System.EventHandler(this.btn_SeleteBlack_Click);
            // 
            // btn_SelectWhite
            // 
            this.btn_SelectWhite.AutoSize = true;
            this.btn_SelectWhite.Location = new System.Drawing.Point(509, 3);
            this.btn_SelectWhite.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.btn_SelectWhite.MinimumSize = new System.Drawing.Size(75, 23);
            this.btn_SelectWhite.Name = "btn_SelectWhite";
            this.btn_SelectWhite.Size = new System.Drawing.Size(75, 23);
            this.btn_SelectWhite.TabIndex = 12;
            this.btn_SelectWhite.Text = "白校准";
            this.btn_SelectWhite.Click += new System.EventHandler(this.btn_SelectWhite_Click);
            // 
            // btnsaveimage
            // 
            this.btnsaveimage.AutoSize = true;
            this.btnsaveimage.Location = new System.Drawing.Point(595, 3);
            this.btnsaveimage.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.btnsaveimage.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnsaveimage.Name = "btnsaveimage";
            this.btnsaveimage.Size = new System.Drawing.Size(103, 23);
            this.btnsaveimage.TabIndex = 9;
            this.btnsaveimage.Text = "保存至“查看图像“";
            this.btnsaveimage.Click += new System.EventHandler(this.btnsaveimage_Click);
            // 
            // dockManager1
            // 
            this.dockManager1.AutoHideContainers.AddRange(new DevExpress.XtraBars.Docking.AutoHideContainer[] {
            this.hideContainerBottom});
            this.dockManager1.Form = this;
            this.dockManager1.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "DevExpress.XtraBars.StandaloneBarDockControl",
            "System.Windows.Forms.StatusBar",
            "System.Windows.Forms.MenuStrip",
            "System.Windows.Forms.StatusStrip",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl",
            "DevExpress.XtraBars.Navigation.OfficeNavigationBar",
            "DevExpress.XtraBars.Navigation.TileNavPane",
            "DevExpress.XtraBars.TabFormControl",
            "DevExpress.XtraBars.FluentDesignSystem.FluentDesignFormControl"});
            // 
            // hideContainerBottom
            // 
            this.hideContainerBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(131)))), ((int)(((byte)(131)))), ((int)(((byte)(131)))));
            this.hideContainerBottom.Controls.Add(this.dockPanel1);
            this.hideContainerBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hideContainerBottom.Location = new System.Drawing.Point(0, 753);
            this.hideContainerBottom.Name = "hideContainerBottom";
            this.hideContainerBottom.Size = new System.Drawing.Size(1286, 24);
            // 
            // dockPanel1
            // 
            this.dockPanel1.Controls.Add(this.dockPanel1_Container);
            this.dockPanel1.Dock = DevExpress.XtraBars.Docking.DockingStyle.Bottom;
            this.dockPanel1.ID = new System.Guid("25e9c18e-1b57-46ef-83a9-36d48238eae9");
            this.dockPanel1.Location = new System.Drawing.Point(0, 553);
            this.dockPanel1.Name = "dockPanel1";
            this.dockPanel1.Options.ShowCloseButton = false;
            this.dockPanel1.OriginalSize = new System.Drawing.Size(200, 200);
            this.dockPanel1.SavedDock = DevExpress.XtraBars.Docking.DockingStyle.Bottom;
            this.dockPanel1.SavedIndex = 0;
            this.dockPanel1.Size = new System.Drawing.Size(1286, 200);
            this.dockPanel1.Text = "光谱";
            this.dockPanel1.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this.tableLayoutPanel1);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 26);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(1278, 170);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Controls.Add(this.gridControl_GuangPu, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.chartControl2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 3, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1278, 170);
            this.tableLayoutPanel1.TabIndex = 33;
            // 
            // gridControl_GuangPu
            // 
            this.gridControl_GuangPu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl_GuangPu.EmbeddedNavigator.AllowHtmlTextInToolTip = DevExpress.Utils.DefaultBoolean.True;
            this.gridControl_GuangPu.EmbeddedNavigator.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.gridControl_GuangPu.EmbeddedNavigator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.gridControl_GuangPu.EmbeddedNavigator.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.gridControl_GuangPu.EmbeddedNavigator.TextLocation = DevExpress.XtraEditors.NavigatorButtonsTextLocation.None;
            this.gridControl_GuangPu.EmbeddedNavigator.ToolTipIconType = DevExpress.Utils.ToolTipIconType.Application;
            this.gridControl_GuangPu.Location = new System.Drawing.Point(1024, 3);
            this.gridControl_GuangPu.MainView = this.gridView_GuangPu;
            this.gridControl_GuangPu.Name = "gridControl_GuangPu";
            this.gridControl_GuangPu.Size = new System.Drawing.Size(121, 164);
            this.gridControl_GuangPu.TabIndex = 29;
            this.gridControl_GuangPu.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView_GuangPu});
            this.gridControl_GuangPu.Click += new System.EventHandler(this.gridControl_GuangPu_Click);
            // 
            // gridView_GuangPu
            // 
            this.gridView_GuangPu.Appearance.HeaderPanel.Options.UseTextOptions = true;
            this.gridView_GuangPu.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridView_GuangPu.Appearance.Row.Options.UseTextOptions = true;
            this.gridView_GuangPu.Appearance.Row.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gridView_GuangPu.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colGUANGPUBIANHAO,
            this.colYANSE});
            this.gridView_GuangPu.GridControl = this.gridControl_GuangPu;
            this.gridView_GuangPu.Name = "gridView_GuangPu";
            this.gridView_GuangPu.OptionsCustomization.AllowSort = false;
            this.gridView_GuangPu.OptionsMenu.EnableColumnMenu = false;
            this.gridView_GuangPu.OptionsView.ShowGroupPanel = false;
            this.gridView_GuangPu.OptionsView.ShowIndicator = false;
            this.gridView_GuangPu.RowCellStyle += new DevExpress.XtraGrid.Views.Grid.RowCellStyleEventHandler(this.gridView_GuangPu_RowCellStyle);
            this.gridView_GuangPu.CustomColumnDisplayText += new DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventHandler(this.gridView_GuangPu_CustomColumnDisplayText);
            // 
            // colGUANGPUBIANHAO
            // 
            this.colGUANGPUBIANHAO.Caption = "光谱编号";
            this.colGUANGPUBIANHAO.FieldName = "colGUANGPUBIANHAO";
            this.colGUANGPUBIANHAO.MinWidth = 47;
            this.colGUANGPUBIANHAO.Name = "colGUANGPUBIANHAO";
            this.colGUANGPUBIANHAO.OptionsColumn.AllowEdit = false;
            this.colGUANGPUBIANHAO.OptionsColumn.AllowFocus = false;
            this.colGUANGPUBIANHAO.OptionsColumn.AllowIncrementalSearch = false;
            this.colGUANGPUBIANHAO.OptionsColumn.AllowMove = false;
            this.colGUANGPUBIANHAO.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.colGUANGPUBIANHAO.OptionsFilter.AllowAutoFilter = false;
            this.colGUANGPUBIANHAO.OptionsFilter.AllowFilter = false;
            this.colGUANGPUBIANHAO.Visible = true;
            this.colGUANGPUBIANHAO.VisibleIndex = 0;
            this.colGUANGPUBIANHAO.Width = 87;
            // 
            // colYANSE
            // 
            this.colYANSE.AppearanceCell.Font = new System.Drawing.Font("仿宋", 1.5F);
            this.colYANSE.AppearanceCell.Options.UseFont = true;
            this.colYANSE.Caption = "颜色";
            this.colYANSE.FieldName = "SeriesColor";
            this.colYANSE.MinWidth = 24;
            this.colYANSE.Name = "colYANSE";
            this.colYANSE.OptionsColumn.AllowEdit = false;
            this.colYANSE.OptionsColumn.AllowFocus = false;
            this.colYANSE.OptionsColumn.AllowIncrementalSearch = false;
            this.colYANSE.OptionsColumn.AllowMove = false;
            this.colYANSE.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            this.colYANSE.OptionsFilter.AllowAutoFilter = false;
            this.colYANSE.OptionsFilter.AllowFilter = false;
            this.colYANSE.Visible = true;
            this.colYANSE.VisibleIndex = 1;
            // 
            // chartControl2
            // 
            xyDiagram1.AxisX.VisibleInPanesSerializable = "-1";
            xyDiagram1.AxisY.VisibleInPanesSerializable = "-1";
            xyDiagram1.EnableAxisXZooming = true;
            xyDiagram1.EnableAxisYZooming = true;
            this.chartControl2.Diagram = xyDiagram1;
            this.chartControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartControl2.Legend.Name = "Default Legend";
            this.chartControl2.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            this.chartControl2.Location = new System.Drawing.Point(3, 3);
            this.chartControl2.Name = "chartControl2";
            series1.Name = "Series 1";
            series1.View = stackedLineSeriesView1;
            this.chartControl2.SeriesSerializable = new DevExpress.XtraCharts.Series[] {
        series1};
            this.chartControl2.Size = new System.Drawing.Size(888, 164);
            this.chartControl2.TabIndex = 32;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.pnlStart);
            this.flowLayoutPanel2.Controls.Add(this.pnlEnd);
            this.flowLayoutPanel2.Controls.Add(this.cb_ReflectanceDisplay);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(897, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(121, 164);
            this.flowLayoutPanel2.TabIndex = 33;
            // 
            // pnlStart
            // 
            this.pnlStart.Controls.Add(this.lblStartWaveLength);
            this.pnlStart.Controls.Add(this.cboStartWaveLength);
            this.pnlStart.Location = new System.Drawing.Point(0, 0);
            this.pnlStart.Margin = new System.Windows.Forms.Padding(0);
            this.pnlStart.Name = "pnlStart";
            this.pnlStart.Size = new System.Drawing.Size(121, 53);
            this.pnlStart.TabIndex = 32;
            // 
            // lblStartWaveLength
            // 
            this.lblStartWaveLength.Location = new System.Drawing.Point(12, 6);
            this.lblStartWaveLength.Name = "lblStartWaveLength";
            this.lblStartWaveLength.Size = new System.Drawing.Size(48, 14);
            this.lblStartWaveLength.TabIndex = 1;
            this.lblStartWaveLength.Text = "开始波长";
            // 
            // cboStartWaveLength
            // 
            this.cboStartWaveLength.Location = new System.Drawing.Point(9, 26);
            this.cboStartWaveLength.Name = "cboStartWaveLength";
            this.cboStartWaveLength.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboStartWaveLength.Properties.PopupFormMinSize = new System.Drawing.Size(20, 0);
            this.cboStartWaveLength.Properties.PopupSizeable = true;
            this.cboStartWaveLength.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboStartWaveLength.Size = new System.Drawing.Size(109, 20);
            this.cboStartWaveLength.TabIndex = 23;
            this.cboStartWaveLength.SelectedIndexChanged += new System.EventHandler(this.cboStartWaveLength_SelectedIndexChanged);
            this.cboStartWaveLength.Validating += new System.ComponentModel.CancelEventHandler(this.cboStartWaveLength_Validating);
            // 
            // pnlEnd
            // 
            this.pnlEnd.Controls.Add(this.lblEndWaveLength);
            this.pnlEnd.Controls.Add(this.cboEndWaveLength);
            this.pnlEnd.Location = new System.Drawing.Point(0, 53);
            this.pnlEnd.Margin = new System.Windows.Forms.Padding(0);
            this.pnlEnd.Name = "pnlEnd";
            this.pnlEnd.Size = new System.Drawing.Size(121, 53);
            this.pnlEnd.TabIndex = 33;
            // 
            // lblEndWaveLength
            // 
            this.lblEndWaveLength.Location = new System.Drawing.Point(12, 6);
            this.lblEndWaveLength.Name = "lblEndWaveLength";
            this.lblEndWaveLength.Size = new System.Drawing.Size(48, 14);
            this.lblEndWaveLength.TabIndex = 1;
            this.lblEndWaveLength.Text = "结束波长";
            // 
            // cboEndWaveLength
            // 
            this.cboEndWaveLength.Location = new System.Drawing.Point(9, 26);
            this.cboEndWaveLength.Name = "cboEndWaveLength";
            this.cboEndWaveLength.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboEndWaveLength.Properties.PopupFormMinSize = new System.Drawing.Size(20, 0);
            this.cboEndWaveLength.Properties.PopupSizeable = true;
            this.cboEndWaveLength.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboEndWaveLength.Size = new System.Drawing.Size(109, 20);
            this.cboEndWaveLength.TabIndex = 23;
            this.cboEndWaveLength.SelectedIndexChanged += new System.EventHandler(this.cboStartWaveLength_SelectedIndexChanged);
            this.cboEndWaveLength.Validating += new System.ComponentModel.CancelEventHandler(this.cboStartWaveLength_Validating);
            // 
            // cb_ReflectanceDisplay
            // 
            this.cb_ReflectanceDisplay.AutoSize = true;
            this.cb_ReflectanceDisplay.Location = new System.Drawing.Point(3, 109);
            this.cb_ReflectanceDisplay.Name = "cb_ReflectanceDisplay";
            this.cb_ReflectanceDisplay.Size = new System.Drawing.Size(86, 18);
            this.cb_ReflectanceDisplay.TabIndex = 31;
            this.cb_ReflectanceDisplay.Text = "反射率显示";
            this.cb_ReflectanceDisplay.UseVisualStyleBackColor = true;
            this.cb_ReflectanceDisplay.CheckedChanged += new System.EventHandler(this.cb_ReflectanceDisplay_CheckedChanged);
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.btn_DeleteCheck);
            this.flowLayoutPanel3.Controls.Add(this.btn_DeleteAll);
            this.flowLayoutPanel3.Controls.Add(this.btn_SaveAllToFile);
            this.flowLayoutPanel3.Controls.Add(this.btn_SaveToFile);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(1151, 3);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(124, 164);
            this.flowLayoutPanel3.TabIndex = 34;
            // 
            // btn_DeleteCheck
            // 
            this.btn_DeleteCheck.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btn_DeleteCheck.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.btn_DeleteCheck.Appearance.BorderColor = System.Drawing.Color.Black;
            this.btn_DeleteCheck.Appearance.ForeColor = System.Drawing.Color.Black;
            this.btn_DeleteCheck.Appearance.Options.UseBackColor = true;
            this.btn_DeleteCheck.Appearance.Options.UseBorderColor = true;
            this.btn_DeleteCheck.Appearance.Options.UseForeColor = true;
            this.btn_DeleteCheck.ImageOptions.ImageIndex = 0;
            this.btn_DeleteCheck.Location = new System.Drawing.Point(3, 3);
            this.btn_DeleteCheck.Name = "btn_DeleteCheck";
            this.btn_DeleteCheck.Size = new System.Drawing.Size(118, 35);
            this.btn_DeleteCheck.TabIndex = 24;
            this.btn_DeleteCheck.Text = "删除选中目标";
            this.btn_DeleteCheck.Click += new System.EventHandler(this.btn_DeleteCheck_Click);
            // 
            // btn_DeleteAll
            // 
            this.btn_DeleteAll.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btn_DeleteAll.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.btn_DeleteAll.Appearance.BorderColor = System.Drawing.Color.Black;
            this.btn_DeleteAll.Appearance.ForeColor = System.Drawing.Color.Black;
            this.btn_DeleteAll.Appearance.Options.UseBackColor = true;
            this.btn_DeleteAll.Appearance.Options.UseBorderColor = true;
            this.btn_DeleteAll.Appearance.Options.UseForeColor = true;
            this.btn_DeleteAll.ImageOptions.ImageIndex = 0;
            this.btn_DeleteAll.Location = new System.Drawing.Point(3, 44);
            this.btn_DeleteAll.Name = "btn_DeleteAll";
            this.btn_DeleteAll.Size = new System.Drawing.Size(118, 35);
            this.btn_DeleteAll.TabIndex = 25;
            this.btn_DeleteAll.Text = "删除所有目标";
            this.btn_DeleteAll.Click += new System.EventHandler(this.btn_DeleteAll_Click);
            // 
            // btn_SaveAllToFile
            // 
            this.btn_SaveAllToFile.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btn_SaveAllToFile.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.btn_SaveAllToFile.Appearance.BorderColor = System.Drawing.Color.Black;
            this.btn_SaveAllToFile.Appearance.ForeColor = System.Drawing.Color.Black;
            this.btn_SaveAllToFile.Appearance.Options.UseBackColor = true;
            this.btn_SaveAllToFile.Appearance.Options.UseBorderColor = true;
            this.btn_SaveAllToFile.Appearance.Options.UseForeColor = true;
            this.btn_SaveAllToFile.ImageOptions.ImageIndex = 0;
            this.btn_SaveAllToFile.Location = new System.Drawing.Point(3, 85);
            this.btn_SaveAllToFile.Name = "btn_SaveAllToFile";
            this.btn_SaveAllToFile.Size = new System.Drawing.Size(118, 35);
            this.btn_SaveAllToFile.TabIndex = 22;
            this.btn_SaveAllToFile.Text = "保存所有光谱";
            this.btn_SaveAllToFile.Click += new System.EventHandler(this.btn_SaveAllToFile_Click);
            // 
            // btn_SaveToFile
            // 
            this.btn_SaveToFile.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btn_SaveToFile.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.btn_SaveToFile.Appearance.BorderColor = System.Drawing.Color.Black;
            this.btn_SaveToFile.Appearance.ForeColor = System.Drawing.Color.Black;
            this.btn_SaveToFile.Appearance.Options.UseBackColor = true;
            this.btn_SaveToFile.Appearance.Options.UseBorderColor = true;
            this.btn_SaveToFile.Appearance.Options.UseForeColor = true;
            this.btn_SaveToFile.ImageOptions.ImageIndex = 0;
            this.btn_SaveToFile.Location = new System.Drawing.Point(3, 126);
            this.btn_SaveToFile.Name = "btn_SaveToFile";
            this.btn_SaveToFile.Size = new System.Drawing.Size(118, 35);
            this.btn_SaveToFile.TabIndex = 23;
            this.btn_SaveToFile.Text = "保存选中光谱";
            this.btn_SaveToFile.Click += new System.EventHandler(this.btn_SaveToFile_Click);
            // 
            // ScanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1286, 777);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.hideContainerBottom);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "ScanForm";
            this.Text = "ViewForm";
            this.Load += new System.EventHandler(this.ScanForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).EndInit();
            this.panelControl3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).EndInit();
            this.hideContainerBottom.ResumeLayout(false);
            this.dockPanel1.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl_GuangPu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView_GuangPu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(xyDiagram1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(stackedLineSeriesView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(series1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartControl2)).EndInit();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.pnlStart.ResumeLayout(false);
            this.pnlStart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboStartWaveLength.Properties)).EndInit();
            this.pnlEnd.ResumeLayout(false);
            this.pnlEnd.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboEndWaveLength.Properties)).EndInit();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl3;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraEditors.LabelControl labFrameRate;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private BaseControl.SortingBaseButton btnConnectCamera;
        private BaseControl.SortingBaseButton btnStartScan;
        private BaseControl.SortingBaseButton btnCameraSetting;
        private ImgView.ImageViewWithTools imageViewWithTools1;
        private BaseControl.SortingBaseButton btnsaveimage;
        private BaseControl.SortingBaseButton btn_SeleteBlack;
        private BaseControl.SortingBaseButton btn_SelectWhite;
        private BaseControl.SortingBaseButton btndisplayandcalibsetting;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private DevExpress.XtraBars.Docking.DockPanel dockPanel1;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraEditors.SimpleButton btn_SaveAllToFile;
        private DevExpress.XtraEditors.SimpleButton btn_DeleteCheck;
        private DevExpress.XtraGrid.GridControl gridControl_GuangPu;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView_GuangPu;
        private DevExpress.XtraGrid.Columns.GridColumn colGUANGPUBIANHAO;
        private DevExpress.XtraGrid.Columns.GridColumn colYANSE;
        private DevExpress.XtraEditors.SimpleButton btn_DeleteAll;
        private DevExpress.XtraEditors.SimpleButton btn_SaveToFile;
        private System.Windows.Forms.CheckBox cb_ReflectanceDisplay;
        private DevExpress.XtraCharts.ChartControl chartControl2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private BaseControl.SortingBaseButton btnPause;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Panel pnlStart;
        private DevExpress.XtraEditors.LabelControl lblStartWaveLength;
        private DevExpress.XtraEditors.ComboBoxEdit cboStartWaveLength;
        private System.Windows.Forms.Panel pnlEnd;
        private DevExpress.XtraEditors.LabelControl lblEndWaveLength;
        private DevExpress.XtraEditors.ComboBoxEdit cboEndWaveLength;
        private DevExpress.XtraBars.Docking.AutoHideContainer hideContainerBottom;
    }
}