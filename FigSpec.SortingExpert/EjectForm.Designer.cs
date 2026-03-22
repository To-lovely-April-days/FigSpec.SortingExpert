
using FigSpec.SortingExpert.BaseControl;

namespace FigSpec.SortingExpert
{
    partial class EjectForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EjectForm));
            this.pnlFill = new DevExpress.XtraEditors.PanelControl();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnStartEject = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.btnEjectSetting = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.pnlBackground = new System.Windows.Forms.Panel();
            this.labFrameRate = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFill)).BeginInit();
            this.pnlFill.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.pnlBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlFill
            // 
            this.pnlFill.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlFill.Controls.Add(this.pictureBox1);
            this.pnlFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFill.Location = new System.Drawing.Point(0, 0);
            this.pnlFill.Name = "pnlFill";
            this.pnlFill.Padding = new System.Windows.Forms.Padding(3);
            this.pnlFill.Size = new System.Drawing.Size(955, 547);
            this.pnlFill.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(949, 541);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.panelControl2);
            this.panelControl1.Controls.Add(this.pnlBackground);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(967, 607);
            this.panelControl1.TabIndex = 4;
            // 
            // panelControl2
            // 
            this.panelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelControl2.Controls.Add(this.labFrameRate);
            this.panelControl2.Controls.Add(this.flowLayoutPanel1);
            this.panelControl2.Location = new System.Drawing.Point(5, 5);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(957, 42);
            this.panelControl2.TabIndex = 4;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.btnStartEject);
            this.flowLayoutPanel1.Controls.Add(this.btnEjectSetting);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(865, 32);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // btnStartEject
            // 
            this.btnStartEject.AutoSize = true;
            this.btnStartEject.Location = new System.Drawing.Point(3, 3);
            this.btnStartEject.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnStartEject.Name = "btnStartEject";
            this.btnStartEject.Size = new System.Drawing.Size(75, 23);
            this.btnStartEject.TabIndex = 6;
            this.btnStartEject.Text = "启动气吹";
            this.btnStartEject.Click += new System.EventHandler(this.btnStartEject_Click);
            // 
            // btnEjectSetting
            // 
            this.btnEjectSetting.AutoSize = true;
            this.btnEjectSetting.Location = new System.Drawing.Point(84, 3);
            this.btnEjectSetting.MinimumSize = new System.Drawing.Size(75, 23);
            this.btnEjectSetting.Name = "btnEjectSetting";
            this.btnEjectSetting.Size = new System.Drawing.Size(75, 23);
            this.btnEjectSetting.TabIndex = 5;
            this.btnEjectSetting.Text = "气吹设置";
            this.btnEjectSetting.Click += new System.EventHandler(this.btnEjectSetting_Click);
            // 
            // pnlBackground
            // 
            this.pnlBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBackground.Controls.Add(this.pnlFill);
            this.pnlBackground.Location = new System.Drawing.Point(5, 53);
            this.pnlBackground.Name = "pnlBackground";
            this.pnlBackground.Size = new System.Drawing.Size(957, 549);
            this.pnlBackground.TabIndex = 6;
            // 
            // labFrameRate
            // 
            this.labFrameRate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labFrameRate.Appearance.Font = new System.Drawing.Font("Tahoma", 15F, System.Drawing.FontStyle.Bold);
            this.labFrameRate.Appearance.Options.UseFont = true;
            this.labFrameRate.Appearance.Options.UseTextOptions = true;
            this.labFrameRate.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.labFrameRate.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labFrameRate.Location = new System.Drawing.Point(695, 8);
            this.labFrameRate.Name = "labFrameRate";
            this.labFrameRate.Size = new System.Drawing.Size(257, 26);
            this.labFrameRate.TabIndex = 8;
            this.labFrameRate.Text = "当前帧率：3000fps";
            // 
            // EjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(967, 607);
            this.Controls.Add(this.panelControl1);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Name = "EjectForm";
            this.Text = "EjectForm";
            this.Load += new System.EventHandler(this.EjectForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pnlFill)).EndInit();
            this.pnlFill.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.pnlBackground.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl pnlFill;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.PanelControl panelControl2;
        private SortingBaseButton btnStartEject;
        private SortingBaseButton btnEjectSetting;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlBackground;
        private DevExpress.XtraEditors.LabelControl labFrameRate;
    }
}