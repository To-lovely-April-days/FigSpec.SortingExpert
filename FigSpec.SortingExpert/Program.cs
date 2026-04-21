using FigSpec.SortingExpert.Algorithm;
using FigSpec.SortingExpert.Tools;
using Globalization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebAPI;
using WebAPI.Tables;

namespace FigSpec.SortingExpert
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // 设置当前线程的区域为美国
            CultureInfo ci = new CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            Process process = RunningInstance();
            if (process != null)
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //设置应用程序处理异常方式：ThreadException处理
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //处理UI线程异常
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            //处理非UI线程异常
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //设置环境变量 在log4net.config中使用
            Environment.SetEnvironmentVariable("MyDocuments", string.Format(GlobalSettings.ApplyInfo.ApplyMyDocuments, "log"));


            #region 加载多语言
            if (Properties.Settings.Default.Language == -1)
                GlobalLanguage.Language = MultiLanguage.GetEnumLanguage(CultureInfo.InstalledUICulture.Name);
            else
                GlobalLanguage.Language = (EnumLanguage)Properties.Settings.Default.Language;//读取默认语言

            GlobalLanguage.LanguageLibrary = MultiLanguage.ReadLanguageXml(GlobalLanguage.Language);
            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo((int)GlobalLanguage.Language);
            }
            catch (Exception)
            {
                throw;
            }
            DevExpress.XtraEditors.Controls.Localizer.Active = new MessageBoxExtend(); //提示框按钮多语言

            FormShowHelper.ShowLoadingForm(new Form(), "启动中...".ToMultiLanguage(), second: 60 * 30, true);
     

            #endregion 加载多语言

            //try
            //{
            //    //检查更新
            //    int maxBuild = Math.Max(GlobalSettings.ApplyInfo.BuildVersion, Properties.Settings.Default.IgnoreBuild);
            //    SystemApi systemApi = new SystemApi();

            //    string result = systemApi.GetCheckUpdate(GlobalSettings.ApplyInfo.PlatformCode, maxBuild, out Versions version);
            //    if (string.IsNullOrEmpty(result) && version != null)
            //    {
            //        VersionUpdate.VersionUpdate versionUpdate = new VersionUpdate.VersionUpdate();
            //        versionUpdate.ShowUpgradeProgramForm(version);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogHelper.WriteLog("检查新版本失败", ex);
            //    return;
            //}
            Application.Run(new MainForm());
        }

        /// <summary>
        /// 查找该进程是否已经启动
        /// </summary>
        /// <returns></returns>
        public static Process RunningInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            string currentAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            Process[] Processes = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (Process process in Processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    try
                    {
                        // 尝试获取其他进程的MainModule，并与当前程序集的位置进行比较
                        if (process.MainModule.FileName.Equals(currentAssemblyLocation, StringComparison.OrdinalIgnoreCase))
                        {
                            return process; // 找到了另一个实例
                        }
                    }
                    catch (Exception)
                    {
                        // 访问MainModule可能会失败，例如由于权限问题
                    }
                }
            }
            return null;
            throw new NotImplementedException();
        }

        /// <summary>
        /// 处理UI线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            string str = GetExceptionMsg(e.Exception, e.ToString());
            LogHelper.WriteLog(str, e.Exception);
        }

        /// <summary>
        /// CurrentDomain_UnhandledException
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string str = GetExceptionMsg(e.ExceptionObject as Exception, e.ToString());
            LogHelper.WriteLog(str, (Exception)e.ExceptionObject);
        }

        /// <summary>
        /// 生成自定义异常消息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="backStr">备用异常消息：当ex为null时有效</param>
        /// <returns>异常字符串文本</returns>
        private static string GetExceptionMsg(Exception ex, string backStr)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("****************************异常文本****************************");
            sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());
            if (ex != null)
            {
                sb.AppendLine("【异常类型】：" + ex.GetType().Name);
                sb.AppendLine("【异常信息】：" + ex.Message);
                sb.AppendLine("【堆栈调用】：" + ex.StackTrace);
            }
            else
            {
                sb.AppendLine("【未处理异常】：" + backStr);
            }
            sb.AppendLine("***************************************************************");
            return sb.ToString();
        }
    }
}