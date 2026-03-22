
namespace FigSpec.SortingExpert.SettingForms
{
    partial class ClassSettingForm
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
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.btnColose = new BaseControl.SortingBaseButton();
            this.panelControl3 = new DevExpress.XtraEditors.PanelControl();
            this.btnSaveName = new BaseControl.SortingBaseButton();
            this.txtRemark = new DevExpress.XtraEditors.TextEdit();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.chcEditthreshold = new DevExpress.XtraEditors.CheckEdit();
            this.spinType = new DevExpress.XtraEditors.SpinEdit();
            this.spinCode = new DevExpress.XtraEditors.SpinEdit();
            this.colorEdit = new DevExpress.XtraEditors.ColorEdit();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.txtName = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.labelControl7 = new DevExpress.XtraEditors.LabelControl();
            this.btnInsertRow = new BaseControl.SortingBaseButton();
            this.btnDeleteRow = new BaseControl.SortingBaseButton();
            this.gcClass = new DevExpress.XtraGrid.GridControl();
            this.gvClass = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumn3 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.columnColor = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumn2 = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).BeginInit();
            this.panelControl3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chcEditthreshold.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinCode.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.colorEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcClass)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvClass)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.btnColose);
            this.panelControl1.Controls.Add(this.panelControl3);
            this.panelControl1.Controls.Add(this.panelControl2);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(661, 471);
            this.panelControl1.TabIndex = 0;
            // 
            // btnColose
            // 
            this.btnColose.Location = new System.Drawing.Point(609, 442);
            this.btnColose.Name = "btnColose";
            this.btnColose.Size = new System.Drawing.Size(47, 23);
            this.btnColose.TabIndex = 3;
            this.btnColose.Text = "关闭";
            this.btnColose.Click += new System.EventHandler(this.btnColose_Click);
            // 
            // panelControl3
            // 
            this.panelControl3.Controls.Add(this.btnSaveName);
            this.panelControl3.Controls.Add(this.txtRemark);
            this.panelControl3.Controls.Add(this.labelControl6);
            this.panelControl3.Controls.Add(this.chcEditthreshold);
            this.panelControl3.Controls.Add(this.spinType);
            this.panelControl3.Controls.Add(this.spinCode);
            this.panelControl3.Controls.Add(this.colorEdit);
            this.panelControl3.Controls.Add(this.labelControl5);
            this.panelControl3.Controls.Add(this.labelControl4);
            this.panelControl3.Controls.Add(this.labelControl3);
            this.panelControl3.Controls.Add(this.labelControl2);
            this.panelControl3.Controls.Add(this.txtName);
            this.panelControl3.Controls.Add(this.labelControl1);
            this.panelControl3.Location = new System.Drawing.Point(245, 5);
            this.panelControl3.Name = "panelControl3";
            this.panelControl3.Size = new System.Drawing.Size(411, 431);
            this.panelControl3.TabIndex = 2;
            // 
            // btnSaveName
            // 
            this.btnSaveName.Location = new System.Drawing.Point(200, 47);
            this.btnSaveName.Name = "btnSaveName";
            this.btnSaveName.Size = new System.Drawing.Size(47, 23);
            this.btnSaveName.TabIndex = 4;
            this.btnSaveName.Text = "保存";
            this.btnSaveName.Click += new System.EventHandler(this.Control_EditValueChanged);
            // 
            // txtRemark
            // 
            this.txtRemark.Location = new System.Drawing.Point(8, 208);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Properties.Appearance.Options.UseTextOptions = true;
            this.txtRemark.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.txtRemark.Properties.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.txtRemark.Properties.AutoHeight = false;
            this.txtRemark.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.txtRemark.Size = new System.Drawing.Size(396, 216);
            this.txtRemark.TabIndex = 11;
            this.txtRemark.EditValueChanged += new System.EventHandler(this.Control_EditValueChanged);
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(8, 188);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(24, 14);
            this.labelControl6.TabIndex = 10;
            this.labelControl6.Text = "备注";
            // 
            // chcEditthreshold
            // 
            this.chcEditthreshold.Location = new System.Drawing.Point(172, 155);
            this.chcEditthreshold.Name = "chcEditthreshold";
            this.chcEditthreshold.Properties.Caption = "启用阈值";
            this.chcEditthreshold.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chcEditthreshold.Size = new System.Drawing.Size(75, 19);
            this.chcEditthreshold.TabIndex = 9;
            this.chcEditthreshold.Visible = false;
            this.chcEditthreshold.CheckedChanged += new System.EventHandler(this.Control_EditValueChanged);
            // 
            // spinType
            // 
            this.spinType.EditValue = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.spinType.Location = new System.Drawing.Point(253, 154);
            this.spinType.Name = "spinType";
            this.spinType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinType.Size = new System.Drawing.Size(151, 20);
            this.spinType.TabIndex = 8;
            this.spinType.EditValueChanged += new System.EventHandler(this.Control_EditValueChanged);
            // 
            // spinCode
            // 
            this.spinCode.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spinCode.Location = new System.Drawing.Point(253, 118);
            this.spinCode.Name = "spinCode";
            this.spinCode.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinCode.Properties.ReadOnly = true;
            this.spinCode.Size = new System.Drawing.Size(151, 20);
            this.spinCode.TabIndex = 7;
            // 
            // colorEdit
            // 
            this.colorEdit.EditValue = System.Drawing.Color.Empty;
            this.colorEdit.Location = new System.Drawing.Point(253, 82);
            this.colorEdit.Name = "colorEdit";
            this.colorEdit.Properties.Appearance.BackColor = System.Drawing.Color.White;
            this.colorEdit.Properties.Appearance.Options.UseBackColor = true;
            this.colorEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.colorEdit.Size = new System.Drawing.Size(151, 20);
            this.colorEdit.TabIndex = 6;
            this.colorEdit.EditValueChanged += new System.EventHandler(this.Control_EditValueChanged);
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(8, 157);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(48, 14);
            this.labelControl5.TabIndex = 5;
            this.labelControl5.Text = "分类阈值";
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(8, 121);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(24, 14);
            this.labelControl4.TabIndex = 4;
            this.labelControl4.Text = "编码";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(8, 85);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(24, 14);
            this.labelControl3.TabIndex = 3;
            this.labelControl3.Text = "颜色";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(8, 49);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(24, 14);
            this.labelControl2.TabIndex = 2;
            this.labelControl2.Text = "名称";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(253, 48);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(151, 20);
            this.txtName.TabIndex = 1;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(5, 7);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(36, 14);
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Text = "类信息";
            // 
            // panelControl2
            // 
            this.panelControl2.Controls.Add(this.labelControl7);
            this.panelControl2.Controls.Add(this.btnInsertRow);
            this.panelControl2.Controls.Add(this.btnDeleteRow);
            this.panelControl2.Controls.Add(this.gcClass);
            this.panelControl2.Location = new System.Drawing.Point(5, 5);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(234, 431);
            this.panelControl2.TabIndex = 1;
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(7, 11);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(48, 14);
            this.labelControl7.TabIndex = 12;
            this.labelControl7.Text = "已有分类";
            // 
            // btnInsertRow
            // 
            this.btnInsertRow.Location = new System.Drawing.Point(200, 7);
            this.btnInsertRow.Name = "btnInsertRow";
            this.btnInsertRow.Size = new System.Drawing.Size(26, 23);
            this.btnInsertRow.TabIndex = 2;
            this.btnInsertRow.Text = "+";
            this.btnInsertRow.Click += new System.EventHandler(this.btnInsertRow_Click);
            // 
            // btnDeleteRow
            // 
            this.btnDeleteRow.Location = new System.Drawing.Point(168, 7);
            this.btnDeleteRow.Name = "btnDeleteRow";
            this.btnDeleteRow.Size = new System.Drawing.Size(26, 23);
            this.btnDeleteRow.TabIndex = 1;
            this.btnDeleteRow.Text = "-";
            this.btnDeleteRow.Click += new System.EventHandler(this.btnDeleteRow_Click);
            // 
            // gcClass
            // 
            this.gcClass.Location = new System.Drawing.Point(7, 35);
            this.gcClass.MainView = this.gvClass;
            this.gcClass.Name = "gcClass";
            this.gcClass.Size = new System.Drawing.Size(219, 391);
            this.gcClass.TabIndex = 0;
            this.gcClass.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvClass});
            // 
            // gvClass
            // 
            this.gvClass.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumn3,
            this.columnColor,
            this.gridColumn2});
            this.gvClass.GridControl = this.gcClass;
            this.gvClass.Name = "gvClass";
            this.gvClass.OptionsBehavior.Editable = false;
            this.gvClass.OptionsView.ShowColumnHeaders = false;
            this.gvClass.OptionsView.ShowGroupPanel = false;
            this.gvClass.OptionsView.ShowIndicator = false;
            this.gvClass.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.gvClass_CustomDrawCell);
            this.gvClass.Click += new System.EventHandler(this.gvClass_Click);
            // 
            // gridColumn3
            // 
            this.gridColumn3.Caption = "Id";
            this.gridColumn3.FieldName = "Id";
            this.gridColumn3.Name = "gridColumn3";
            // 
            // columnColor
            // 
            this.columnColor.Caption = "颜色";
            this.columnColor.Name = "columnColor";
            this.columnColor.Visible = true;
            this.columnColor.VisibleIndex = 0;
            this.columnColor.Width = 171;
            // 
            // gridColumn2
            // 
            this.gridColumn2.Caption = "名称";
            this.gridColumn2.FieldName = "name";
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.Visible = true;
            this.gridColumn2.VisibleIndex = 1;
            this.gridColumn2.Width = 659;
            // 
            // ClassSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 471);
            this.Controls.Add(this.panelControl1);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "ClassSettingForm";
            this.Text = "分类管理";
            this.Load += new System.EventHandler(this.ClassSettingForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl3)).EndInit();
            this.panelControl3.ResumeLayout(false);
            this.panelControl3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chcEditthreshold.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinCode.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.colorEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.panelControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gcClass)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvClass)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl3;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private DevExpress.XtraGrid.GridControl gcClass;
        private DevExpress.XtraGrid.Views.Grid.GridView gvClass;
        private DevExpress.XtraEditors.SpinEdit spinType;
        private DevExpress.XtraEditors.SpinEdit spinCode;
        private DevExpress.XtraEditors.ColorEdit colorEdit;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private BaseControl.SortingBaseButton btnInsertRow;
        private BaseControl.SortingBaseButton btnDeleteRow;
        private DevExpress.XtraEditors.TextEdit txtRemark;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.CheckEdit chcEditthreshold;
        private BaseControl.SortingBaseButton btnColose;
        private DevExpress.XtraEditors.LabelControl labelControl7;
        private DevExpress.XtraGrid.Columns.GridColumn columnColor;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumn3;
        private BaseControl.SortingBaseButton btnSaveName;
    }
}