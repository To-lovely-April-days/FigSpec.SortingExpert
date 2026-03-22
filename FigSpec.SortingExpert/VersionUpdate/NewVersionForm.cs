using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net;
using FigSpec.SortingExpert;
using Globalization;
using WebAPI.Tables;

namespace FigSpec.SortingExpert.VersionUpdate
{
    public partial class NewVersionForm : BaseFormInside
    {
        /// <summary>
        /// 升级文件所在地址
        /// </summary>
        private string FileName = string.Empty;
        private Versions version;
        public NewVersionForm(Versions _version)
        {
            InitializeComponent();

            version = _version;
        }

        private void NewVersionForm_Load(object sender, EventArgs e)
        {
            labelControl1.Text = GlobalSettings.ApplyInfo.SoftwareName;

            FileName = GlobalSettings.ApplyInfo.PackageUpgradePath + "UpgradePackage_" + version.build + ".exe";
            if (!File.Exists(FileName))
            {
                btnUpdateNow.Text = "下载并更新".ToMultiLanguage();
            }
            else
            {
                btnUpdateNow.Text = "立即更新".ToMultiLanguage();
            }

            labLog.Text = string.Empty;
            //更新说明日记
            if (version.log != null)
            {
                foreach (var item in version.log.Split('$'))
                {
                    labLog.Text = labLog.Text + item + Environment.NewLine;
                }
            }

        }

        private void btnUpdateNow_Click(object sender, EventArgs e)
        {
            VersionUpdate update = new VersionUpdate();

            if (!File.Exists(FileName))
            {
                btnUpdateNow.Text = string.Format("已下载({0})".ToMultiLanguage(), "0%");
                update.DownloadNewVersion(FileName, version, Web_DownloadProgressChanged, Web_DownloadFileCompleted);
            }
            else
            {
                if (update.RunAppasAdmin(FileName, null))
                {
                    Environment.Exit(0);
                }
                else
                {
                    FormShowHelper.ShowMessage("请允许来自未知发布者的应用对你的设备进行更改".ToMultiLanguage() ,"提示".ToMultiLanguage());
                    btnUpdateNow.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Web_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            VersionUpdate update = new VersionUpdate();
            // update.OpenUpgradeProgram(FileName, version);
            if (update.RunAppasAdmin(FileName, null))
            {
                Environment.Exit(0);
            }
            else
            {
                FormShowHelper.ShowMessage("请允许来自未知发布者的应用对你的设备进行更改".ToMultiLanguage(), "提示".ToMultiLanguage());
                btnUpdateNow.Enabled = true;
            }

        }

        /// <summary>
        /// 更新进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Web_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                btnUpdateNow.Text = string.Format("已下载({0})".ToMultiLanguage(), e.ProgressPercentage.ToString("0.##") + "%");
            }));
        }



        private void btnUpdateLater_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnIgnore_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.IgnoreBuild = version.build; //修改配置文件
            Properties.Settings.Default.Save();
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        #region 让窗体变成可移动
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("User32.dll")]
        private static extern IntPtr WindowFromPoint(Point p);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_MOVE = 0xF010;
        public const int HTCAPTION = 0x0002;
        private IntPtr moveObject = IntPtr.Zero;    //拖动窗体的句柄

        private void PNTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (moveObject == IntPtr.Zero)
            {
                if (this.Parent != null)
                {
                    moveObject = this.Parent.Handle;
                }
                else
                {
                    moveObject = this.Handle;
                }
            }
            ReleaseCapture();
            SendMessage(moveObject, WM_SYSCOMMAND, SC_MOVE + HTCAPTION, 0);
        }


        #endregion

    }
}
