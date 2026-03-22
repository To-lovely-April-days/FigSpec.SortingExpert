
namespace FigSpec.SortingExpert.SettingForms
{
    partial class DisplayAndCalibrationSettingForm
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
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.chkOnlyCacheDisplayArea = new DevExpress.XtraEditors.CheckEdit();
            this.chkSaveDisplayArea = new DevExpress.XtraEditors.CheckEdit();
            this.checkSavesawimage = new DevExpress.XtraEditors.CheckEdit();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.lblEffectiveHour = new DevExpress.XtraEditors.LabelControl();
            this.spEffectiveHour = new DevExpress.XtraEditors.SpinEdit();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.btnimportblackraw = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnimportwhiteraw = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnimportwhiteref = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnSave = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.lblSampleInterval = new DevExpress.XtraEditors.LabelControl();
            this.lblT = new DevExpress.XtraEditors.LabelControl();
            this.spEndSample = new DevExpress.XtraEditors.SpinEdit();
            this.spStartSample = new DevExpress.XtraEditors.SpinEdit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkOnlyCacheDisplayArea.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSaveDisplayArea.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkSavesawimage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spEffectiveHour.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spEndSample.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spStartSample.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.groupControl2);
            this.panelControl1.Controls.Add(this.groupControl1);
            this.panelControl1.Controls.Add(this.btnSave);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(682, 401);
            this.panelControl1.TabIndex = 0;
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.spStartSample);
            this.groupControl2.Controls.Add(this.spEndSample);
            this.groupControl2.Controls.Add(this.lblT);
            this.groupControl2.Controls.Add(this.lblSampleInterval);
            this.groupControl2.Controls.Add(this.chkOnlyCacheDisplayArea);
            this.groupControl2.Controls.Add(this.chkSaveDisplayArea);
            this.groupControl2.Controls.Add(this.checkSavesawimage);
            this.groupControl2.Location = new System.Drawing.Point(12, 182);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(658, 176);
            this.groupControl2.TabIndex = 9;
            this.groupControl2.Text = "保存设置";
            // 
            // chkOnlyCacheDisplayArea
            // 
            this.chkOnlyCacheDisplayArea.Location = new System.Drawing.Point(17, 108);
            this.chkOnlyCacheDisplayArea.Name = "chkOnlyCacheDisplayArea";
            this.chkOnlyCacheDisplayArea.Properties.Caption = "仅缓存显示区域的数据";
            this.chkOnlyCacheDisplayArea.Size = new System.Drawing.Size(619, 19);
            this.chkOnlyCacheDisplayArea.TabIndex = 13;
            // 
            // chkSaveDisplayArea
            // 
            this.chkSaveDisplayArea.Location = new System.Drawing.Point(17, 74);
            this.chkSaveDisplayArea.Name = "chkSaveDisplayArea";
            this.chkSaveDisplayArea.Properties.Caption = "仅保存当前显示区域（注：仅对水平方向进行截取）";
            this.chkSaveDisplayArea.Size = new System.Drawing.Size(619, 19);
            this.chkSaveDisplayArea.TabIndex = 12;
            // 
            // checkSavesawimage
            // 
            this.checkSavesawimage.Location = new System.Drawing.Point(17, 40);
            this.checkSavesawimage.Name = "checkSavesawimage";
            this.checkSavesawimage.Properties.Caption = "保存原始数据（注：未进行黑白校准的原始数据）";
            this.checkSavesawimage.Size = new System.Drawing.Size(619, 19);
            this.checkSavesawimage.TabIndex = 11;
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.lblEffectiveHour);
            this.groupControl1.Controls.Add(this.spEffectiveHour);
            this.groupControl1.Controls.Add(this.labelControl4);
            this.groupControl1.Controls.Add(this.labelControl1);
            this.groupControl1.Controls.Add(this.labelControl2);
            this.groupControl1.Controls.Add(this.btnimportblackraw);
            this.groupControl1.Controls.Add(this.btnimportwhiteraw);
            this.groupControl1.Controls.Add(this.btnimportwhiteref);
            this.groupControl1.Location = new System.Drawing.Point(12, 12);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(658, 164);
            this.groupControl1.TabIndex = 8;
            this.groupControl1.Text = "校准文件状态";
            // 
            // lblEffectiveHour
            // 
            this.lblEffectiveHour.Location = new System.Drawing.Point(102, 134);
            this.lblEffectiveHour.Name = "lblEffectiveHour";
            this.lblEffectiveHour.Size = new System.Drawing.Size(79, 14);
            this.lblEffectiveHour.TabIndex = 17;
            this.lblEffectiveHour.Text = "有效时间（h）";
            // 
            // spEffectiveHour
            // 
            this.spEffectiveHour.EditValue = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.spEffectiveHour.Location = new System.Drawing.Point(17, 131);
            this.spEffectiveHour.Name = "spEffectiveHour";
            this.spEffectiveHour.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spEffectiveHour.Properties.EditFormat.FormatString = "F2";
            this.spEffectiveHour.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.spEffectiveHour.Properties.MaxValue = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.spEffectiveHour.Size = new System.Drawing.Size(75, 20);
            this.spEffectiveHour.TabIndex = 16;
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(98, 102);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(24, 14);
            this.labelControl4.TabIndex = 15;
            this.labelControl4.Text = "名称";
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(98, 44);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(24, 14);
            this.labelControl1.TabIndex = 13;
            this.labelControl1.Text = "名称";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(98, 73);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(24, 14);
            this.labelControl2.TabIndex = 12;
            this.labelControl2.Text = "名称";
            // 
            // btnimportblackraw
            // 
            this.btnimportblackraw.AutoSize = true;
            this.btnimportblackraw.Location = new System.Drawing.Point(17, 98);
            this.btnimportblackraw.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnimportblackraw.Name = "btnimportblackraw";
            this.btnimportblackraw.Size = new System.Drawing.Size(75, 23);
            this.btnimportblackraw.TabIndex = 11;
            this.btnimportblackraw.Text = "导入";
            this.btnimportblackraw.Click += new System.EventHandler(this.btnimportblackraw_Click);
            // 
            // btnimportwhiteraw
            // 
            this.btnimportwhiteraw.AutoSize = true;
            this.btnimportwhiteraw.Location = new System.Drawing.Point(17, 69);
            this.btnimportwhiteraw.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnimportwhiteraw.Name = "btnimportwhiteraw";
            this.btnimportwhiteraw.Size = new System.Drawing.Size(75, 23);
            this.btnimportwhiteraw.TabIndex = 9;
            this.btnimportwhiteraw.Text = "导入";
            this.btnimportwhiteraw.Click += new System.EventHandler(this.btnimportwhiteraw_Click);
            // 
            // btnimportwhiteref
            // 
            this.btnimportwhiteref.AutoSize = true;
            this.btnimportwhiteref.Location = new System.Drawing.Point(17, 40);
            this.btnimportwhiteref.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnimportwhiteref.Name = "btnimportwhiteref";
            this.btnimportwhiteref.Size = new System.Drawing.Size(75, 23);
            this.btnimportwhiteref.TabIndex = 8;
            this.btnimportwhiteref.Text = "导入";
            this.btnimportwhiteref.Click += new System.EventHandler(this.btnimportwhiteref_Click);
            // 
            // btnSave
            // 
            this.btnSave.AutoSize = true;
            this.btnSave.Location = new System.Drawing.Point(592, 364);
            this.btnSave.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "保存";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lblSampleInterval
            // 
            this.lblSampleInterval.Location = new System.Drawing.Point(194, 145);
            this.lblSampleInterval.Name = "lblSampleInterval";
            this.lblSampleInterval.Size = new System.Drawing.Size(119, 14);
            this.lblSampleInterval.TabIndex = 18;
            this.lblSampleInterval.Text = "显示像素区间(1~640)";
            // 
            // lblT
            // 
            this.lblT.Location = new System.Drawing.Point(98, 145);
            this.lblT.Name = "lblT";
            this.lblT.Size = new System.Drawing.Size(9, 14);
            this.lblT.TabIndex = 21;
            this.lblT.Text = "~";
            // 
            // spEndSample
            // 
            this.spEndSample.EditValue = new decimal(new int[] {
            640,
            0,
            0,
            0});
            this.spEndSample.Location = new System.Drawing.Point(113, 142);
            this.spEndSample.Name = "spEndSample";
            this.spEndSample.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spEndSample.Properties.EditFormat.FormatString = "F2";
            this.spEndSample.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.spEndSample.Properties.IsFloatValue = false;
            this.spEndSample.Properties.Mask.EditMask = "N00";
            this.spEndSample.Properties.MaxValue = new decimal(new int[] {
            640,
            0,
            0,
            0});
            this.spEndSample.Properties.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spEndSample.Size = new System.Drawing.Size(75, 20);
            this.spEndSample.TabIndex = 22;
            // 
            // spStartSample
            // 
            this.spStartSample.EditValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spStartSample.Location = new System.Drawing.Point(17, 142);
            this.spStartSample.Name = "spStartSample";
            this.spStartSample.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spStartSample.Properties.EditFormat.FormatString = "F2";
            this.spStartSample.Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.spStartSample.Properties.IsFloatValue = false;
            this.spStartSample.Properties.Mask.EditMask = "N00";
            this.spStartSample.Properties.MaxValue = new decimal(new int[] {
            640,
            0,
            0,
            0});
            this.spStartSample.Properties.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spStartSample.Size = new System.Drawing.Size(75, 20);
            this.spStartSample.TabIndex = 23;
            // 
            // DisplayAndCalibrationSettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(682, 401);
            this.Controls.Add(this.panelControl1);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "DisplayAndCalibrationSettingForm";
            this.Text = "校准和保存设置";
            this.Load += new System.EventHandler(this.DisplayAndCalibrationSettingForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkOnlyCacheDisplayArea.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSaveDisplayArea.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkSavesawimage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spEffectiveHour.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spEndSample.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spStartSample.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private BaseControl.SortingBaseButton btnSave;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private BaseControl.SortingBaseButton btnimportblackraw;
        private BaseControl.SortingBaseButton btnimportwhiteraw;
        private BaseControl.SortingBaseButton btnimportwhiteref;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.CheckEdit checkSavesawimage;
        private DevExpress.XtraEditors.CheckEdit chkSaveDisplayArea;
        private DevExpress.XtraEditors.CheckEdit chkOnlyCacheDisplayArea;
        private DevExpress.XtraEditors.LabelControl lblEffectiveHour;
        private DevExpress.XtraEditors.SpinEdit spEffectiveHour;
        private DevExpress.XtraEditors.LabelControl lblSampleInterval;
        private DevExpress.XtraEditors.LabelControl lblT;
        private DevExpress.XtraEditors.SpinEdit spStartSample;
        private DevExpress.XtraEditors.SpinEdit spEndSample;
    }
}