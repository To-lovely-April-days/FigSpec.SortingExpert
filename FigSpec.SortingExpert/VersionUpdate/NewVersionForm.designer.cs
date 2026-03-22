
namespace FigSpec.SortingExpert.VersionUpdate
{
    partial class NewVersionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewVersionForm));
            this.btnUpdateNow = new DevExpress.XtraEditors.SimpleButton();
            this.labLog = new DevExpress.XtraEditors.LabelControl();
            this.labTitle = new DevExpress.XtraEditors.LabelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnClose = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnUpdateLater = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnIgnore = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.SuspendLayout();
            // 
            // btnUpdateNow
            // 
            this.btnUpdateNow.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(173)))), ((int)(((byte)(25)))));
            this.btnUpdateNow.Appearance.Font = new System.Drawing.Font("Tahoma", 11F);
            this.btnUpdateNow.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnUpdateNow.Appearance.Options.UseBackColor = true;
            this.btnUpdateNow.Appearance.Options.UseFont = true;
            this.btnUpdateNow.Appearance.Options.UseForeColor = true;
            this.btnUpdateNow.Appearance.Options.UseTextOptions = true;
            this.btnUpdateNow.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.btnUpdateNow.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnUpdateNow.Location = new System.Drawing.Point(474, 386);
            this.btnUpdateNow.Name = "btnUpdateNow";
            this.btnUpdateNow.Size = new System.Drawing.Size(166, 40);
            this.btnUpdateNow.TabIndex = 0;
            this.btnUpdateNow.Text = "立即更新";
            this.btnUpdateNow.Click += new System.EventHandler(this.btnUpdateNow_Click);
            // 
            // labLog
            // 
            this.labLog.Appearance.ForeColor = System.Drawing.Color.White;
            this.labLog.Appearance.Options.UseForeColor = true;
            this.labLog.Appearance.Options.UseTextOptions = true;
            this.labLog.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.labLog.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labLog.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labLog.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labLog.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.labLog.Location = new System.Drawing.Point(60, 106);
            this.labLog.Name = "labLog";
            this.labLog.Size = new System.Drawing.Size(580, 266);
            this.labLog.TabIndex = 3;
            // 
            // labTitle
            // 
            this.labTitle.Appearance.Font = new System.Drawing.Font("Tahoma", 13F, System.Drawing.FontStyle.Bold);
            this.labTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.labTitle.Appearance.Options.UseFont = true;
            this.labTitle.Appearance.Options.UseForeColor = true;
            this.labTitle.Location = new System.Drawing.Point(12, 12);
            this.labTitle.Name = "labTitle";
            this.labTitle.Size = new System.Drawing.Size(57, 22);
            this.labTitle.TabIndex = 4;
            this.labTitle.Text = "新版本";
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 20F);
            this.labelControl1.Appearance.ForeColor = System.Drawing.Color.White;
            this.labelControl1.Appearance.Options.UseFont = true;
            this.labelControl1.Appearance.Options.UseForeColor = true;
            this.labelControl1.Appearance.Options.UseTextOptions = true;
            this.labelControl1.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.labelControl1.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelControl1.Location = new System.Drawing.Point(60, 57);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(580, 33);
            this.labelControl1.TabIndex = 5;
            this.labelControl1.Text = "Sorting Expert";
            // 
            // btnClose
            // 
            this.btnClose.AutoSize = true;
            this.btnClose.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnClose.ImageOptions.ImageUri.Uri = "Delete;Size32x32;Office2013";
            this.btnClose.Location = new System.Drawing.Point(662, 2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(36, 36);
            this.btnClose.TabIndex = 6;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnUpdateLater
            // 
            this.btnUpdateLater.Appearance.BorderColor = System.Drawing.Color.White;
            this.btnUpdateLater.Appearance.Font = new System.Drawing.Font("Tahoma", 11F);
            this.btnUpdateLater.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnUpdateLater.Appearance.Options.UseBorderColor = true;
            this.btnUpdateLater.Appearance.Options.UseFont = true;
            this.btnUpdateLater.Appearance.Options.UseForeColor = true;
            this.btnUpdateLater.Appearance.Options.UseTextOptions = true;
            this.btnUpdateLater.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.btnUpdateLater.Location = new System.Drawing.Point(267, 386);
            this.btnUpdateLater.Name = "btnUpdateLater";
            this.btnUpdateLater.Size = new System.Drawing.Size(166, 40);
            this.btnUpdateLater.TabIndex = 7;
            this.btnUpdateLater.Text = "以后提醒我";
            this.btnUpdateLater.Click += new System.EventHandler(this.btnUpdateLater_Click);
            // 
            // btnIgnore
            // 
            this.btnIgnore.Appearance.BorderColor = System.Drawing.Color.White;
            this.btnIgnore.Appearance.Font = new System.Drawing.Font("Tahoma", 11F);
            this.btnIgnore.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnIgnore.Appearance.Options.UseBorderColor = true;
            this.btnIgnore.Appearance.Options.UseFont = true;
            this.btnIgnore.Appearance.Options.UseForeColor = true;
            this.btnIgnore.Appearance.Options.UseTextOptions = true;
            this.btnIgnore.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.btnIgnore.Location = new System.Drawing.Point(60, 386);
            this.btnIgnore.Name = "btnIgnore";
            this.btnIgnore.Size = new System.Drawing.Size(166, 40);
            this.btnIgnore.TabIndex = 8;
            this.btnIgnore.Text = "忽略当前版本";
            this.btnIgnore.Click += new System.EventHandler(this.btnIgnore_Click);
            // 
            // NewVersionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 490);
            this.Controls.Add(this.btnIgnore);
            this.Controls.Add(this.btnUpdateLater);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.labTitle);
            this.Controls.Add(this.labLog);
            this.Controls.Add(this.btnUpdateNow);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "NewVersionForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NewVersionForm";
            this.Load += new System.EventHandler(this.NewVersionForm_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PNTop_MouseDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnUpdateNow;
        private DevExpress.XtraEditors.LabelControl labLog;
        private DevExpress.XtraEditors.LabelControl labTitle;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private BaseControl.SortingBaseButton btnClose;
        private BaseControl.SortingBaseButton btnUpdateLater;
        private BaseControl.SortingBaseButton btnIgnore;
    }
}