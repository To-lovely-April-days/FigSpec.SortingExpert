
namespace FigSpec.SortingExpert.SettingForms
{
    partial class FormThresholdChange
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
            this.spThreshold = new DevExpress.XtraEditors.SpinEdit();
            this.lblThreshold = new DevExpress.XtraEditors.LabelControl();
            this.barThreshold = new DevExpress.XtraEditors.ZoomTrackBarControl();
            this.barErodeLevel = new DevExpress.XtraEditors.ZoomTrackBarControl();
            this.lblErodeLevel = new DevExpress.XtraEditors.LabelControl();
            this.btnOk = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.spErodeLevel = new DevExpress.XtraEditors.SpinEdit();
            ((System.ComponentModel.ISupportInitialize)(this.spThreshold.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barThreshold.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barErodeLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barErodeLevel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spErodeLevel.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // spThreshold
            // 
            this.spThreshold.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spThreshold.Location = new System.Drawing.Point(108, 16);
            this.spThreshold.Name = "spThreshold";
            this.spThreshold.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spThreshold.Properties.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.spThreshold.Properties.MaxValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spThreshold.Size = new System.Drawing.Size(78, 20);
            this.spThreshold.TabIndex = 13;
            // 
            // lblThreshold
            // 
            this.lblThreshold.Location = new System.Drawing.Point(26, 22);
            this.lblThreshold.Name = "lblThreshold";
            this.lblThreshold.Size = new System.Drawing.Size(24, 14);
            this.lblThreshold.TabIndex = 15;
            this.lblThreshold.Text = "阈值";
            // 
            // barThreshold
            // 
            this.barThreshold.EditValue = null;
            this.barThreshold.Location = new System.Drawing.Point(26, 54);
            this.barThreshold.Name = "barThreshold";
            this.barThreshold.Size = new System.Drawing.Size(160, 17);
            this.barThreshold.TabIndex = 17;
            // 
            // barErodeLevel
            // 
            this.barErodeLevel.EditValue = null;
            this.barErodeLevel.Location = new System.Drawing.Point(26, 113);
            this.barErodeLevel.Name = "barErodeLevel";
            this.barErodeLevel.Properties.Maximum = 100;
            this.barErodeLevel.Size = new System.Drawing.Size(160, 17);
            this.barErodeLevel.TabIndex = 18;
            // 
            // lblErodeLevel
            // 
            this.lblErodeLevel.Location = new System.Drawing.Point(26, 85);
            this.lblErodeLevel.Name = "lblErodeLevel";
            this.lblErodeLevel.Size = new System.Drawing.Size(36, 14);
            this.lblErodeLevel.TabIndex = 19;
            this.lblErodeLevel.Text = "腐蚀度";
            // 
            // btnOk
            // 
            this.btnOk.AutoSize = true;
            this.btnOk.Location = new System.Drawing.Point(108, 144);
            this.btnOk.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 20;
            this.btnOk.Text = "确定";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // spErodeLevel
            // 
            this.spErodeLevel.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spErodeLevel.Location = new System.Drawing.Point(108, 87);
            this.spErodeLevel.Name = "spErodeLevel";
            this.spErodeLevel.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spErodeLevel.Properties.IsFloatValue = false;
            this.spErodeLevel.Properties.Mask.EditMask = "N00";
            this.spErodeLevel.Properties.MaxValue = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.spErodeLevel.Size = new System.Drawing.Size(78, 20);
            this.spErodeLevel.TabIndex = 21;
            // 
            // FormThresholdChange
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(208, 179);
            this.Controls.Add(this.spErodeLevel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblErodeLevel);
            this.Controls.Add(this.barErodeLevel);
            this.Controls.Add(this.barThreshold);
            this.Controls.Add(this.lblThreshold);
            this.Controls.Add(this.spThreshold);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "FormThresholdChange";
            this.Text = "阈值修改";
            this.Load += new System.EventHandler(this.FormThresholdChange_Load);
            ((System.ComponentModel.ISupportInitialize)(this.spThreshold.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barThreshold.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barErodeLevel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barErodeLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spErodeLevel.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraEditors.SpinEdit spThreshold;
        private DevExpress.XtraEditors.LabelControl lblThreshold;
        private DevExpress.XtraEditors.ZoomTrackBarControl barThreshold;
        private DevExpress.XtraEditors.ZoomTrackBarControl barErodeLevel;
        private DevExpress.XtraEditors.LabelControl lblErodeLevel;
        private BaseControl.SortingBaseButton btnOk;
        private DevExpress.XtraEditors.SpinEdit spErodeLevel;
    }
}