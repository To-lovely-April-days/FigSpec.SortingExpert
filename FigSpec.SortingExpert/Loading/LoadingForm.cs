using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using DevExpress.Xpo.Logger;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.ButtonsPanelControl;
using Globalization;

namespace FigSpec.SortingExpert
{
    public delegate void ProgressTaskHandler(DoWorkEventArgs e);
    public partial class LoadingForm : BaseForm
    {
        #region 字段和属性
        /// <summary>
        /// 工作线程
        /// </summary>
        public BackgroundWorker MyWork = new BackgroundWorker();

        /// <summary>
        /// 开始工作的委托事件
        /// </summary>
        public event ProgressTaskHandler OnDoWork;

        /// <summary>
        /// 进度时间
        /// </summary>
        private System.Threading.Timer progressUpdateTimer;

        /// <summary>
        /// 当前进度
        /// </summary>
        private int currentProgress = 0;
        /// <summary>
        /// 其他信息
        /// </summary>
        private string otherInfo = "";
        /// <summary>
        /// 当前运行时间
        /// </summary>
        private int currentTime = 0;

        /// <summary>
        /// 是否停止计时
        /// </summary>
        private bool StopTiming = true;

        private bool isShowProgress = true;
        #endregion

        #region 构造函数
        private LoadingForm()
        {
            InitializeComponent();
            InitMyWork();

            this.ShowInTaskbar = false;
            this.TopMost = true;
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="caption">提示</param>
        /// <param name="message">消息内容</param>
        /// <param name="content">详细描述</param>
        /// <param name="isShowCancel">是否显示取消按钮</param>
        public LoadingForm(string caption = "", string message = "", string content = "", bool isShowCancel = false, bool isShowProgress = true)
            : this()
        {
            caption = string.IsNullOrWhiteSpace(caption) ? "提示".ToMultiLanguage() : caption;
            message = string.IsNullOrWhiteSpace(message) ? "正在努力加载中，请稍等......".ToMultiLanguage() : message;
            content = string.IsNullOrWhiteSpace(content) ? content : "";
            this.isShowProgress = isShowProgress;
            if (!isShowCancel)
            {
                gpcCaption.CustomHeaderButtons.Clear();
            }
            else
            {
                (gpcCaption.CustomHeaderButtons[0] as GroupBoxButton).Caption = "取消".ToMultiLanguage();
            }

            SetCaption(caption);
            SetMessage(message);
            SetContent(content);
        }

        private void LoadingForm_Load(object sender, EventArgs e)
        {
            layoutControlItem3.Visibility = isShowProgress? DevExpress.XtraLayout.Utils.LayoutVisibility.Always : DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
        }
        #endregion

        #region 方法
        private void InitMyWork()
        {
            // 允许BackgroundWorker报告进度并处理事件
            MyWork.WorkerReportsProgress = true;
            MyWork.WorkerSupportsCancellation = true; // 如果需要取消支持

            // 绑定事件处理方法
            MyWork.DoWork += BackgroundWorker_DoWork;
            MyWork.ProgressChanged += BackgroundWorker_ProgressChanged;
            MyWork.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        private void InitTimer()
        {
            // 初始化定时器
            progressUpdateTimer = new System.Threading.Timer(OnTimerTick, null, 1000, 1000);
            StopTiming = false;
        }

        private void OnTimerTick(object state)
        {
            if (StopTiming)
            {
                return;
            }
            currentTime += 1;
            // 假设this指代一个Form或Control对象
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    SetProgress(currentProgress);
                    SetContentTime(currentTime);
                }));
            }
            else
            {
                SetProgress(currentProgress);
                SetContentTime(currentTime);
            }

            if (!MyWork.IsBusy)
            {
                MyWork.RunWorkerAsync();
            }
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnDoWork?.Invoke(e);
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            currentProgress = Math.Max(currentProgress, e.ProgressPercentage);
            otherInfo = $"   {e.UserState?.ToString()}";
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!this.IsHandleCreated)
            {
                return;
            }

            StopTiming = true;
            if (this.InvokeRequired)
            {
                if (!this.IsHandleCreated)
                {
                    return;
                }
                this.Invoke(new Action(() =>
                {
                    this.Close();
                }));
            }
            else
            {
                this.Close();
            }

            return;
            //if (e.Cancelled || e.Error != null)
            //{

            //}
            //else
            //{
            //    //自动关闭加载窗体
            //    if (this.InvokeRequired)
            //    {
            //        this.Invoke(new Action(() =>
            //        {
            //            if (currentTime < 10)
            //            {
            //                SetContentTime(currentTime++);
            //                var sum = 100 - (int)progressShow.EditValue;
            //                for (int i = 0; i < sum; i++)
            //                {
            //                    if (i % 30 == 0)
            //                    {
            //                        SetContentTime(currentTime++);
            //                    }
            //                    SetProgress();
            //                    Thread.Sleep(30);
            //                }
            //            }
            //            this.Close();
            //        }));
            //    }
            //    else
            //    {
            //        if (currentTime < 10)
            //        {
            //            SetContentTime(currentTime++);
            //            var sum = 100 - (int)progressShow.EditValue;
            //            for (int i = 0; i < sum; i++)
            //            {
            //                if (i % 30 == 0)
            //                {
            //                    SetContentTime(currentTime++);
            //                }
            //                SetProgress();
            //                Thread.Sleep(30);
            //            }
            //        }
            //        this.Close();
            //    }
            //}
        }

        /// <summary>
        /// 设置提示
        /// </summary>
        /// <param name="newCaption"></param>
        public void SetCaption(string newCaption)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    gpcCaption.Text = newCaption;
                }));
            }
            else
            {
                gpcCaption.Text = newCaption;
            }
        }

        /// <summary>
        /// 设置消息
        /// </summary>
        /// <param name="newMessage"></param>
        public void SetMessage(string newMessage)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    lblMessage.Text = newMessage;
                }));
            }
            else
            {
                lblMessage.Text = newMessage;
            }
            
        }

        /// <summary>
        /// 设置描述
        /// </summary>
        /// <param name="newContent"></param>
        public void SetContent(string newContent)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    lblContent.Text = newContent;
                }));
            }
            else
            {
                lblContent.Text = newContent;
            }
            
        }

        /// <summary>
        /// 设置时间描述
        /// </summary>
        /// <param name="newContent"></param>
        public void SetContentTime(int time)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    lblContent.Text = string.Format("已运行：{0}s".ToMultiLanguage(), time) + otherInfo;
                }));
            }
            else
            {
                lblContent.Text = string.Format("已运行：{0}s".ToMultiLanguage(), time) + otherInfo;
            }
           
        }

        public void SetProcessVisible(bool isVisible)
        {
            isShowProgress = isVisible;
            currentTime = 0;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    layoutControlItem3.Visibility = isShowProgress ? DevExpress.XtraLayout.Utils.LayoutVisibility.Always : DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                }));
            }
            else
            {
                layoutControlItem3.Visibility = isShowProgress ? DevExpress.XtraLayout.Utils.LayoutVisibility.Always : DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
            }
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        public void SetProgress()
        {
            progressShow.PerformStep();
            this.Refresh();
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="ProgressPercentage"></param>
        public void SetProgress(int ProgressPercentage)
        {
            progressShow.EditValue = ProgressPercentage;
            this.Refresh();
        }
        #endregion

        #region 事件
        private void ShowProgressDialogForm_Shown(object sender, EventArgs e)
        {
            InitTimer();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (progressUpdateTimer != null)
            {
                progressUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite); // 停止定时器
                progressUpdateTimer.Dispose(); // 清理资源
                progressUpdateTimer = null; // 确保引用为空
            }

            MyWork.Dispose();

            base.OnClosing(e);
        }

        private void gpcCaption_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            if (FormShowHelper.ShowMessage("取消任务将可能导致数据错误和异常,是否确认取消?".ToMultiLanguage() , "提示".ToMultiLanguage(), buttons: new DialogResult[] { DialogResult.OK, DialogResult.Cancel }) == DialogResult.OK)
            {
                if (MyWork.IsBusy && !StopTiming)
                {
                    MyWork.CancelAsync();
                    SetMessage("取消任务中,请稍等...".ToMultiLanguage());
                }
            }
        }
        #endregion


    }
}

