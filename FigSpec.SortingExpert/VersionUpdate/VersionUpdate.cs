using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebAPI;
using WebAPI.Tables;

namespace FigSpec.SortingExpert.VersionUpdate
{
    public class VersionUpdate
    {
        /// <summary>
        /// 显示升级窗口
        /// </summary>
        /// <param name="version"></param>
        /// <param name="isManualUpdate"></param>
        public void ShowUpgradeProgramForm(Versions version)
        {
            if (version != null && version.build > GlobalSettings.ApplyInfo.BuildVersion)
            {
                NewVersionForm form = new NewVersionForm(version);
                form.ShowDialog();
            }
        }

        /// <summary>
        /// 下载新版本
        /// </summary>
        public void DownloadNewVersion(string fileName, Versions version, DownloadProgressChangedEventHandler downloadProgressChanged = null, AsyncCompletedEventHandler downloadFileCompleted = null)
        {
            //版本间隔，超过一个以上版本没有升级的  使用完整包的版本，否则使用增加量包的版本
            int interval = Math.Abs(version.build - GlobalSettings.ApplyInfo.BuildVersion);
            string url = !string.IsNullOrEmpty(version.download_url2) && interval <= 1 ? version.download_url2 : version.download_url;
            HttpTool tool = new HttpTool();
            tool.Download(url, fileName, downloadProgressChanged, downloadFileCompleted);
        }


        public bool RunAppasAdmin(string fileName, string arguments)
        {
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = fileName;
                psi.UseShellExecute = true;
                psi.CreateNoWindow = true;
                psi.Arguments = arguments;//带参数
                try
                {
                    System.Diagnostics.Process.Start(psi);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                    // MessageBox.Show(e.Message);
                }
            }
            else
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = fileName;
                psi.UseShellExecute = true;
                psi.WorkingDirectory = Environment.CurrentDirectory;
                psi.Arguments = arguments;
                psi.Verb = "runas";
                try
                {
                    System.Diagnostics.Process.Start(psi);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
