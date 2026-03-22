using Globalization;
using System;
using WebAPI;
using WebAPI.Tables;

namespace FigSpec.SortingExpert.VersionUpdate
{
    public partial class AboutForm : BaseForm
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            this.Text = "关于".ToMultiLanguage();
            this.label1.Text = GlobalSettings.ApplyInfo.SoftwareName;
            this.labVersionTitle.Text = "版本：".ToMultiLanguage();
            lbVersion.Text = GlobalSettings.ApplyInfo.VersionInfo;

            label3.Visible = label2.Visible = false;
        }

        private void btnCheckUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                //检查更新
                string result = new SystemApi().GetCheckUpdate(GlobalSettings.ApplyInfo.PlatformCode, GlobalSettings.ApplyInfo.BuildVersion, out Versions version);
                if (string.IsNullOrEmpty(result) && version != null)
                {
                    VersionUpdate versionUpdate = new VersionUpdate();
                    versionUpdate.ShowUpgradeProgramForm(version);
                }
                else if (string.IsNullOrEmpty(result))
                {
                    FormShowHelper.ShowMessage("已是最新版本".ToMultiLanguage(),"提示".ToMultiLanguage());
                }
                else
                {
                    FormShowHelper.ShowMessage(result, "提示".ToMultiLanguage());
                }
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLog("检查新版本失败", ex);
                return;
            }
        }
    }
}