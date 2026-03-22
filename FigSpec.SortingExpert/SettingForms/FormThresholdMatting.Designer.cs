
namespace FigSpec.SortingExpert.SettingForms
{
    partial class FormThresholdMatting
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
            this.barThreshold = new DevExpress.XtraEditors.ZoomTrackBarControl();
            this.lblPaiHeiDiSuanFa = new DevExpress.XtraEditors.LabelControl();
            this.cboPaiHeiDiSuanFa = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblThreshold = new DevExpress.XtraEditors.LabelControl();
            this.spErodeLevel = new DevExpress.XtraEditors.SpinEdit();
            this.btnOk = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.lblErodeLevel = new DevExpress.XtraEditors.LabelControl();
            this.barErodeLevel = new DevExpress.XtraEditors.ZoomTrackBarControl();
            this.spThreshold = new DevExpress.XtraEditors.SpinEdit();
            ((System.ComponentModel.ISupportInitialize)(this.barThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barThreshold.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPaiHeiDiSuanFa.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spErodeLevel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barErodeLevel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barErodeLevel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spThreshold.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // barThreshold
            // 
            this.barThreshold.EditValue = null;
            this.barThreshold.Location = new System.Drawing.Point(26, 103);
            this.barThreshold.Name = "barThreshold";
            this.barThreshold.Size = new System.Drawing.Size(160, 17);
            this.barThreshold.TabIndex = 17;
            // 
            // lblPaiHeiDiSuanFa
            // 
            this.lblPaiHeiDiSuanFa.Location = new System.Drawing.Point(26, 16);
            this.lblPaiHeiDiSuanFa.Margin = new System.Windows.Forms.Padding(3, 7, 3, 2);
            this.lblPaiHeiDiSuanFa.Name = "lblPaiHeiDiSuanFa";
            this.lblPaiHeiDiSuanFa.Size = new System.Drawing.Size(60, 14);
            this.lblPaiHeiDiSuanFa.TabIndex = 19;
            this.lblPaiHeiDiSuanFa.Text = "扣黑底算法";
            // 
            // cboPaiHeiDiSuanFa
            // 
            this.cboPaiHeiDiSuanFa.EditValue = "";
            this.cboPaiHeiDiSuanFa.Location = new System.Drawing.Point(26, 36);
            this.cboPaiHeiDiSuanFa.Margin = new System.Windows.Forms.Padding(3, 4, 3, 2);
            this.cboPaiHeiDiSuanFa.Name = "cboPaiHeiDiSuanFa";
            this.cboPaiHeiDiSuanFa.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cboPaiHeiDiSuanFa.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cboPaiHeiDiSuanFa.Size = new System.Drawing.Size(160, 20);
            this.cboPaiHeiDiSuanFa.TabIndex = 20;
            // 
            // lblThreshold
            // 
            this.lblThreshold.Location = new System.Drawing.Point(26, 73);
            this.lblThreshold.Margin = new System.Windows.Forms.Padding(3, 7, 3, 2);
            this.lblThreshold.Name = "lblThreshold";
            this.lblThreshold.Size = new System.Drawing.Size(60, 14);
            this.lblThreshold.TabIndex = 21;
            this.lblThreshold.Text = "扣黑底阈值";
            // 
            // spErodeLevel
            // 
            this.spErodeLevel.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spErodeLevel.Location = new System.Drawing.Point(108, 131);
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
            this.spErodeLevel.TabIndex = 25;
            // 
            // btnOk
            // 
            this.btnOk.AutoSize = true;
            this.btnOk.Location = new System.Drawing.Point(108, 188);
            this.btnOk.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 24;
            this.btnOk.Text = "确定";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // lblErodeLevel
            // 
            this.lblErodeLevel.Location = new System.Drawing.Point(26, 134);
            this.lblErodeLevel.Name = "lblErodeLevel";
            this.lblErodeLevel.Size = new System.Drawing.Size(36, 14);
            this.lblErodeLevel.TabIndex = 23;
            this.lblErodeLevel.Text = "腐蚀度";
            // 
            // barErodeLevel
            // 
            this.barErodeLevel.EditValue = null;
            this.barErodeLevel.Location = new System.Drawing.Point(26, 160);
            this.barErodeLevel.Name = "barErodeLevel";
            this.barErodeLevel.Properties.Maximum = 100;
            this.barErodeLevel.Size = new System.Drawing.Size(160, 17);
            this.barErodeLevel.TabIndex = 22;
            // 
            // spThreshold
            // 
            this.spThreshold.EditValue = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.spThreshold.Location = new System.Drawing.Point(108, 70);
            this.spThreshold.Name = "spThreshold";
            this.spThreshold.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spThreshold.Properties.IsFloatValue = false;
            this.spThreshold.Properties.Mask.EditMask = "N00";
            this.spThreshold.Size = new System.Drawing.Size(78, 20);
            this.spThreshold.TabIndex = 26;
            // 
            // FormThresholdMatting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(207, 237);
            this.Controls.Add(this.spThreshold);
            this.Controls.Add(this.spErodeLevel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblErodeLevel);
            this.Controls.Add(this.barErodeLevel);
            this.Controls.Add(this.lblPaiHeiDiSuanFa);
            this.Controls.Add(this.cboPaiHeiDiSuanFa);
            this.Controls.Add(this.lblThreshold);
            this.Controls.Add(this.barThreshold);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "FormThresholdMatting";
            this.Text = "阈值修改";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormThresholdMatting_FormClosed);
            this.Load += new System.EventHandler(this.FormThresholdMatting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.barThreshold.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboPaiHeiDiSuanFa.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spErodeLevel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barErodeLevel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barErodeLevel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spThreshold.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DevExpress.XtraEditors.ZoomTrackBarControl barThreshold;
        private DevExpress.XtraEditors.LabelControl lblPaiHeiDiSuanFa;
        private DevExpress.XtraEditors.ComboBoxEdit cboPaiHeiDiSuanFa;
        private DevExpress.XtraEditors.LabelControl lblThreshold;
        private DevExpress.XtraEditors.SpinEdit spErodeLevel;
        private BaseControl.SortingBaseButton btnOk;
        private DevExpress.XtraEditors.LabelControl lblErodeLevel;
        private DevExpress.XtraEditors.ZoomTrackBarControl barErodeLevel;
        private DevExpress.XtraEditors.SpinEdit spThreshold;
    }
}