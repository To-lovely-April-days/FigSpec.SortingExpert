
namespace FigSpec.SortingExpert.SettingForms
{
    partial class FormPreprocessEdit
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
            this.lblType = new DevExpress.XtraEditors.LabelControl();
            this.cboType = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblName = new DevExpress.XtraEditors.LabelControl();
            this.cboName = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblParam1 = new DevExpress.XtraEditors.LabelControl();
            this.lblParam2 = new DevExpress.XtraEditors.LabelControl();
            this.btnOK = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnClose = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.spParam1 = new DevExpress.XtraEditors.SpinEdit();
            this.spParam2 = new DevExpress.XtraEditors.SpinEdit();
            this.dxErrorProvider1 = new DevExpress.XtraEditors.DXErrorProvider.DXErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.cboType.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spParam1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spParam2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblType
            // 
            this.lblType.Location = new System.Drawing.Point(127, 66);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(48, 14);
            this.lblType.TabIndex = 15;
            this.lblType.Text = "算法类型";
            // 
            // cboType
            // 
            this.cboType.Location = new System.Drawing.Point(127, 86);
            this.cboType.Name = "cboType";
            this.cboType.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboType.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboType.Size = new System.Drawing.Size(202, 20);
            this.cboType.TabIndex = 16;
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(127, 127);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(24, 14);
            this.lblName.TabIndex = 17;
            this.lblName.Text = "算法";
            // 
            // cboName
            // 
            this.cboName.Location = new System.Drawing.Point(127, 147);
            this.cboName.Name = "cboName";
            this.cboName.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboName.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboName.Size = new System.Drawing.Size(202, 20);
            this.cboName.TabIndex = 18;
            // 
            // lblParam1
            // 
            this.lblParam1.Location = new System.Drawing.Point(127, 186);
            this.lblParam1.Name = "lblParam1";
            this.lblParam1.Size = new System.Drawing.Size(48, 14);
            this.lblParam1.TabIndex = 19;
            this.lblParam1.Text = "滤波长度";
            // 
            // lblParam2
            // 
            this.lblParam2.Location = new System.Drawing.Point(127, 247);
            this.lblParam2.Name = "lblParam2";
            this.lblParam2.Size = new System.Drawing.Size(48, 14);
            this.lblParam2.TabIndex = 21;
            this.lblParam2.Text = "滤波次数";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(228, 398);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(113, 23);
            this.btnOK.TabIndex = 23;
            this.btnOK.Text = "确定";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(347, 398);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(113, 23);
            this.btnClose.TabIndex = 24;
            this.btnClose.Text = "取消";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // spParam1
            // 
            this.spParam1.EditValue = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.spParam1.Location = new System.Drawing.Point(127, 206);
            this.spParam1.Name = "spParam1";
            this.spParam1.Properties.Appearance.Options.UseTextOptions = true;
            this.spParam1.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.spParam1.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spParam1.Properties.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.spParam1.Properties.IsFloatValue = false;
            this.spParam1.Properties.Mask.EditMask = "N00";
            this.spParam1.Properties.MaxValue = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.spParam1.Properties.MinValue = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.spParam1.Size = new System.Drawing.Size(202, 20);
            this.spParam1.TabIndex = 25;
            this.spParam1.Validating += new System.ComponentModel.CancelEventHandler(this.spParam1_Validating);
            // 
            // spParam2
            // 
            this.spParam2.EditValue = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.spParam2.Location = new System.Drawing.Point(127, 267);
            this.spParam2.Name = "spParam2";
            this.spParam2.Properties.Appearance.Options.UseTextOptions = true;
            this.spParam2.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.spParam2.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spParam2.Properties.IsFloatValue = false;
            this.spParam2.Properties.Mask.EditMask = "N00";
            this.spParam2.Properties.MaxValue = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.spParam2.Properties.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spParam2.Size = new System.Drawing.Size(202, 20);
            this.spParam2.TabIndex = 26;
            // 
            // dxErrorProvider1
            // 
            this.dxErrorProvider1.ContainerControl = this;
            // 
            // FormPreprocessEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 443);
            this.Controls.Add(this.spParam2);
            this.Controls.Add(this.spParam1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblParam2);
            this.Controls.Add(this.lblParam1);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.cboName);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.cboType);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "FormPreprocessEdit";
            this.Text = "新增/编辑预处理算法";
            this.Load += new System.EventHandler(this.FormPreprocessEdit_Load);
            ((System.ComponentModel.ISupportInitialize)(this.cboType.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spParam1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spParam2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dxErrorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl lblType;
        private DevExpress.XtraEditors.ComboBoxEdit cboType;
        private DevExpress.XtraEditors.LabelControl lblName;
        private DevExpress.XtraEditors.ComboBoxEdit cboName;
        private DevExpress.XtraEditors.LabelControl lblParam1;
        private DevExpress.XtraEditors.LabelControl lblParam2;
        private BaseControl.SortingBaseButton btnOK;
        private BaseControl.SortingBaseButton btnClose;
        private DevExpress.XtraEditors.SpinEdit spParam1;
        private DevExpress.XtraEditors.SpinEdit spParam2;
        private DevExpress.XtraEditors.DXErrorProvider.DXErrorProvider dxErrorProvider1;
    }
}