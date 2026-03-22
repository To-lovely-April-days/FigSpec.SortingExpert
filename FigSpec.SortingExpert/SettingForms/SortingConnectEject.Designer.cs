
namespace FigSpec.SortingExpert.SettingForms
{
    partial class SortingConnectEject
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
            this.pnlFile = new DevExpress.XtraEditors.PanelControl();
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.spinEditSaveFrame = new DevExpress.XtraEditors.SpinEdit();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.btnSaveFileName = new DevExpress.XtraEditors.ButtonEdit();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.cmbCommunication = new DevExpress.XtraEditors.ComboBoxEdit();
            this.btnClose = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.pnlTcp = new DevExpress.XtraEditors.PanelControl();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnGetIPAddress = new FigSpec.SortingExpert.BaseControl.SortingBaseButton();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.textIPPort = new DevExpress.XtraEditors.TextEdit();
            this.textIP = new DevExpress.XtraEditors.TextEdit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFile)).BeginInit();
            this.pnlFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinEditSaveFrame.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveFileName.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCommunication.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlTcp)).BeginInit();
            this.pnlTcp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textIPPort.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textIP.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.pnlFile);
            this.panelControl1.Controls.Add(this.labelControl3);
            this.panelControl1.Controls.Add(this.cmbCommunication);
            this.panelControl1.Controls.Add(this.btnClose);
            this.panelControl1.Controls.Add(this.pnlTcp);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(662, 386);
            this.panelControl1.TabIndex = 0;
            // 
            // pnlFile
            // 
            this.pnlFile.Controls.Add(this.labelControl4);
            this.pnlFile.Controls.Add(this.spinEditSaveFrame);
            this.pnlFile.Controls.Add(this.labelControl6);
            this.pnlFile.Controls.Add(this.btnSaveFileName);
            this.pnlFile.Location = new System.Drawing.Point(75, 116);
            this.pnlFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlFile.Name = "pnlFile";
            this.pnlFile.Size = new System.Drawing.Size(508, 145);
            this.pnlFile.TabIndex = 23;
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(33, 38);
            this.labelControl4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(108, 22);
            this.labelControl4.TabIndex = 18;
            this.labelControl4.Text = "文件存储路径";
            // 
            // spinEditSaveFrame
            // 
            this.spinEditSaveFrame.EditValue = new decimal(new int[] {
            1500,
            0,
            0,
            0});
            this.spinEditSaveFrame.Location = new System.Drawing.Point(299, 89);
            this.spinEditSaveFrame.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.spinEditSaveFrame.Name = "spinEditSaveFrame";
            this.spinEditSaveFrame.Properties.Appearance.Options.UseTextOptions = true;
            this.spinEditSaveFrame.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.spinEditSaveFrame.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.spinEditSaveFrame.Properties.IsFloatValue = false;
            this.spinEditSaveFrame.Properties.Mask.EditMask = "N00";
            this.spinEditSaveFrame.Properties.MaxValue = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.spinEditSaveFrame.Properties.MinValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.spinEditSaveFrame.Size = new System.Drawing.Size(194, 30);
            this.spinEditSaveFrame.TabIndex = 21;
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(33, 93);
            this.labelControl6.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(108, 22);
            this.labelControl6.TabIndex = 22;
            this.labelControl6.Text = "文件存储帧数";
            // 
            // btnSaveFileName
            // 
            this.btnSaveFileName.Location = new System.Drawing.Point(299, 34);
            this.btnSaveFileName.Name = "btnSaveFileName";
            this.btnSaveFileName.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton()});
            this.btnSaveFileName.Size = new System.Drawing.Size(194, 30);
            this.btnSaveFileName.TabIndex = 17;
            this.btnSaveFileName.ButtonClick += new DevExpress.XtraEditors.Controls.ButtonPressedEventHandler(this.btnSaveFileName_ButtonClick);
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(108, 58);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(72, 22);
            this.labelControl3.TabIndex = 16;
            this.labelControl3.Text = "通讯方式";
            // 
            // cmbCommunication
            // 
            this.cmbCommunication.Location = new System.Drawing.Point(374, 50);
            this.cmbCommunication.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbCommunication.Name = "cmbCommunication";
            this.cmbCommunication.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.cmbCommunication.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            this.cmbCommunication.Size = new System.Drawing.Size(194, 30);
            this.cmbCommunication.TabIndex = 15;
            this.cmbCommunication.SelectedIndexChanged += new System.EventHandler(this.cmbCommunication_SelectedIndexChanged);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(418, 307);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(150, 36);
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "确定";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // pnlTcp
            // 
            this.pnlTcp.Controls.Add(this.labelControl1);
            this.pnlTcp.Controls.Add(this.btnGetIPAddress);
            this.pnlTcp.Controls.Add(this.labelControl2);
            this.pnlTcp.Controls.Add(this.textIPPort);
            this.pnlTcp.Controls.Add(this.textIP);
            this.pnlTcp.Location = new System.Drawing.Point(75, 116);
            this.pnlTcp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlTcp.Name = "pnlTcp";
            this.pnlTcp.Size = new System.Drawing.Size(508, 145);
            this.pnlTcp.TabIndex = 11;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(33, 36);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(53, 22);
            this.labelControl1.TabIndex = 6;
            this.labelControl1.Text = "IP地址";
            // 
            // btnGetIPAddress
            // 
            this.btnGetIPAddress.Location = new System.Drawing.Point(131, 30);
            this.btnGetIPAddress.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnGetIPAddress.Name = "btnGetIPAddress";
            this.btnGetIPAddress.Size = new System.Drawing.Size(159, 36);
            this.btnGetIPAddress.TabIndex = 10;
            this.btnGetIPAddress.Text = "获取本机IP";
            this.btnGetIPAddress.Click += new System.EventHandler(this.btnGetIPAddress_Click);
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(33, 88);
            this.labelControl2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(54, 22);
            this.labelControl2.TabIndex = 7;
            this.labelControl2.Text = "端口号";
            // 
            // textIPPort
            // 
            this.textIPPort.Location = new System.Drawing.Point(299, 83);
            this.textIPPort.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textIPPort.Name = "textIPPort";
            this.textIPPort.Size = new System.Drawing.Size(194, 30);
            this.textIPPort.TabIndex = 9;
            // 
            // textIP
            // 
            this.textIP.Location = new System.Drawing.Point(299, 31);
            this.textIP.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textIP.Name = "textIP";
            this.textIP.Size = new System.Drawing.Size(194, 30);
            this.textIP.TabIndex = 8;
            // 
            // SortingConnectEject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 386);
            this.Controls.Add(this.panelControl1);
            this.LookAndFeel.SkinName = "Office 2010 Black";
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.Margin = new System.Windows.Forms.Padding(9, 13, 9, 13);
            this.Name = "SortingConnectEject";
            this.Text = "气吹设置";
            this.Load += new System.EventHandler(this.SortingConnectEject_Load);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlFile)).EndInit();
            this.pnlFile.ResumeLayout(false);
            this.pnlFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinEditSaveFrame.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveFileName.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCommunication.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlTcp)).EndInit();
            this.pnlTcp.ResumeLayout(false);
            this.pnlTcp.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textIPPort.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textIP.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private BaseControl.SortingBaseButton btnGetIPAddress;
        private DevExpress.XtraEditors.TextEdit textIPPort;
        private DevExpress.XtraEditors.TextEdit textIP;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.PanelControl pnlTcp;
        private BaseControl.SortingBaseButton btnClose;
        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.ButtonEdit btnSaveFileName;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.ComboBoxEdit cmbCommunication;
        private DevExpress.XtraEditors.PanelControl pnlFile;
        private DevExpress.XtraEditors.SpinEdit spinEditSaveFrame;
        private DevExpress.XtraEditors.LabelControl labelControl6;
    }
}