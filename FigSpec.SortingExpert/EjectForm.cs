using FigSpec.SortingExpert.AppCode;
using FigSpec.SortingExpert.SettingForms;
using FigSpec.SortingExpert.Tools;
using FigSpec.Spectral.Extensions;
using Globalization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FigSpec.SortingExpert
{
    public partial class EjectForm : BaseFormInside
    {
        #region Fields
        /// <summary>
        /// 预览控件的定时器对象
        /// </summary>
        private Timer imageViewTimer;
        /// <summary>
        /// 预览控件的宽
        /// </summary>
        private int controlWidth = 15;
        /// <summary>
        /// 预览控件的高
        /// </summary>
        private int controlHeight = 15;
        private ConcurrentQueue<Byte[]> saveQueue;
        private int needLine = 1500;//实时显示帧数
        private int realSample = 10;//像素个数
        private int realFrameRate = 0; //帧频
        private Dictionary<int, int> LabelsInfo;
        /// <summary>
        /// tcp服务端对象
        /// </summary>
        private ServerControl server;
        //是否已经启动吹气
        private bool startFlag = false;

        #endregion
        public EjectForm()
        {
            InitializeComponent();
            InitCommunicationByTCPIP();
            NotificationAction.SendEjectSetting = SendEjectSetting;
            NotificationAction.SendEjectData = SendEjectData;
            updateFrameRate(false);
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void InitCommunicationByTCPIP()
        {
            //初始话TCP/IP
            var appset = GlobalSettings.ApplySetting;
            server = new ServerControl(appset.ServerPort, data =>
            {
                if (data[0] != 0xee || data[1] != 0x01 || data[2] != 0x01)
                {
                    Console.WriteLine("接收的帧数据解析错误");
                    return;
                }
                if (data[3] == 0x00)//设置信息
                {
                    LabelsInfo = new Dictionary<int, int>();
                    realFrameRate = BitConverter.ToInt32(data, 4);
                    realSample = BitConverter.ToInt32(data, 8);
                    needLine = realSample * controlWidth / controlHeight;
                    int count = BitConverter.ToInt32(data, 12);
                    for (int i = 0; i < count; i++)
                    {
                        int id = BitConverter.ToInt32(data, 16 + i * 2 * 4);
                        int color = BitConverter.ToInt32(data, 16 + (i * 2 + 1) * 4);
                        LabelsInfo.Add(id, color);
                    }
                    updateFrameRate(true);
                }
                else if (data[3] == 0x01)//吹气信息
                {
                    if (saveQueue != null)
                    {
                        var sample = BitConverter.ToInt32(data, 4);
                        byte[] frameData = new byte[realSample];
                        long time1 = BitConverter.ToInt64(data, 8);
                        Array.Copy(data, 16, frameData, 0, realSample);//帧数据

                        SendEjectData(frameData, time1);
                    }
                }
            });
            server.Start();
        }
        bool isContinue = true;
        private void updateFrameRate(bool isShow = true)
        {
            if (isShow)
            {
                labFrameRate.Text = "当前帧率：".ToMultiLanguage() + $"{realFrameRate}fps";
            }
            else
            {
                labFrameRate.Text = "当前帧率：".ToMultiLanguage() + "--";
            }
        }

        private void SendEjectSetting(int frameRate, int samples, Dictionary<int, int> labelinfo)
        {
            LabelsInfo = labelinfo;
            realSample = samples;
            realFrameRate = frameRate;
            needLine = realSample * controlWidth / controlHeight;
            updateFrameRate(true);
        }

        private void SendEjectData(byte[] frameData, long time1)
        {
            if (saveQueue != null)
            {
                if (saveQueue.Count >= needLine)
                {
                    if (saveQueue.TryDequeue(out var deleData))
                    {
                        saveQueue.Enqueue(frameData);
                    }
                }
                else
                {
                    saveQueue.Enqueue(frameData);
                }

                // 修改后的判断
                bool hasTarget = false;
                int targetCount = 0;
                for (int i = 0; i < frameData.Length; i++)
                {
                    if (frameData[i] > 0 && frameData[i] < 254)
                    {
                        hasTarget = true;
                        targetCount++;
                    }
                }

                // === 新增: 偶尔打印 hasTarget 状态 ===
                if (targetCount > 0)  // 只在真的有目标时打印,避免日志爆炸
                {
                    LogHelper.WriteLog($"[EJECT-FRAME] hasTarget=true, targetCount={targetCount}, 入队 AirQueue");
                }
                // =====================================
                if (hasTarget)
                {
                    ClassifierControl.Shared.AirQueue.Enqueue((isContinue, frameData, time1));
                    isContinue = true;
                }
                else
                {
                    isContinue = false;
                }
            }
        }

        /// <summary>
        /// 定时刷新显示图像画面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (GlobalSettings.CurrentFormName == this.Name)
                {
                    if (saveQueue.Count > 0 && realSample > 0)
                    {
                        pictureBox1.Image = frameDataHandle();
                    }
                }
            }
            catch (Exception ex)
            {
                FormShowHelper.ShowMessage(ex.ToString(), "提示".ToMultiLanguage());
            }
        }
        /// <summary>
        /// 将帧数据转成图像
        /// </summary>
        /// <returns></returns>
        private Bitmap frameDataHandle()
        {
            var imgdata = saveQueue.ToList();
            List<byte[]> realData = new List<byte[]>();
            for (int i = 0; i < imgdata.Count; i++)
            {
                byte[] element = imgdata[i];
                byte[] newData = new byte[element.Length * 3];

                for (int j = 0; j < element.Length; j++)
                {
                    if (LabelsInfo != null && LabelsInfo.ContainsKey(element[j]))
                    {
                        newData[3 * j] = Color.FromArgb(LabelsInfo[element[j]]).B;
                        newData[3 * j + 1] = Color.FromArgb(LabelsInfo[element[j]]).G;
                        newData[3 * j + 2] = Color.FromArgb(LabelsInfo[element[j]]).R;
                    }
                    else
                    {
                        if (LabelsInfo != null && LabelsInfo.ContainsKey(255) && element[j] == 0)
                        {
                            newData[3 * j] = Color.FromArgb(LabelsInfo[255]).B;
                            newData[3 * j + 1] = Color.FromArgb(LabelsInfo[255]).G;
                            newData[3 * j + 2] = Color.FromArgb(LabelsInfo[255]).R;
                        }
                        else
                        {
                            newData[3 * j] = 255;
                            newData[3 * j + 1] = 255;
                            newData[3 * j + 2] = 255;
                        }
                    }
                }
                realData.Add(newData);
            }
            return realData.ToArray().FramesToBitmap(needLine, realSample);

        }

        private void btnStartEject_Click(object sender, EventArgs e)
        {
            LogHelper.WriteLog($"[EJECT-BTN] 点击 启动气吹 按钮, 当前状态 startFlag={startFlag}");

            if (!startFlag)
            {
                var setting = GlobalSettings.ApplySetting;
                LogHelper.WriteLog($"[EJECT-BTN] 准备连接气吹设备: TCP={setting.EjectDeviceConnectTcp}, IP={setting.EjectDeviceIp}, Port={setting.EjectDevicePort}, COM={setting.EjectDeviceCom}");

                if (setting.EjectDeviceConnectTcp)
                {
                    if (!ClassifierControl.Shared.ConnectEjectDevice(setting.EjectDeviceIp, setting.EjectDevicePort))
                    {
                        LogHelper.WriteLog($"[EJECT-BTN] 气吹设备连接失败! TCP={setting.EjectDeviceIp}:{setting.EjectDevicePort}");
                        FormShowHelper.ShowMessage("气吹设备连接失败".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                }
                else
                {
                    //初始化气吹
                    if (!ClassifierControl.Shared.ConnectEjectDevice(setting.EjectDeviceCom))
                    {
                        LogHelper.WriteLog($"[EJECT-BTN] 气吹设备连接失败! COM={setting.EjectDeviceCom}");
                        FormShowHelper.ShowMessage("气吹设备连接失败".ToMultiLanguage(), "提示".ToMultiLanguage());
                        return;
                    }
                }

                LogHelper.WriteLog($"[EJECT-BTN] 气吹设备连接成功, 准备 StartAirProcess");
                ClassifierControl.Shared.StartAirProcess();
                LogHelper.WriteLog($"[EJECT-BTN] StartAirProcess 已调用");

                imageViewTimer.Start();

                //得到宽度、高度
                controlWidth = pictureBox1.Width;
                controlHeight = pictureBox1.Height;
                needLine = realSample * controlWidth / controlHeight;
                saveQueue = new ConcurrentQueue<byte[]>();
                startFlag = true;
                LogHelper.WriteLog($"[EJECT-BTN] startFlag 设为 true, 启动完成");
            }
            else
            {
                LogHelper.WriteLog($"[EJECT-BTN] 准备 StopAirProcess");
                ClassifierControl.Shared.StopAirProcess();
                imageViewTimer.Stop();
                saveQueue = null;
                startFlag = false;
                ClassifierControl.Shared.DisConnectEjectDevice();
                LogHelper.WriteLog($"[EJECT-BTN] 已停止气吹");
            }
            UpdateStatus();
        }

        private void UpdateBlowPortCount()
        {
            var setting = GlobalSettings.ApplySetting.TracheaSet;
        }


        private void UpdateStatus()
        {
            if (startFlag)
            {
                btnStartEject.Text = "停止气吹".ToMultiLanguage();
                //btnEjectSetting.Enabled = false;
            }
            else
            {
                btnStartEject.Text = "启动气吹".ToMultiLanguage();
                //btnEjectSetting.Enabled = true;

            }
        }
        private void btnEjectSetting_Click(object sender, EventArgs e)
        {
            controlWidth = pictureBox1.Width;
            controlHeight = pictureBox1.Height;
            EjectIpSettingForm ejectIpSettingForm = new EjectIpSettingForm(startFlag, () =>
            {
                pictureBox1.Invalidate();
            });
            FormShowHelper.ShowDialog(ejectIpSettingForm);

            UpdateBlowPortCount();
        }

        private void EjectForm_Load(object sender, EventArgs e)
        {
            // 订阅Timer的Tick事件
            imageViewTimer = new Timer()
            {
                Interval = 50,
                Enabled = false
            };
            imageViewTimer.Tick += Timer_Tick;
            UpdateBlowPortCount();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var set = GlobalSettings.ApplySetting.TracheaSet;
            float pw = pictureBox1.Height * 1.0f / set.PixelNumber;

            float sy = (set.StartPixel - 0.5f) * pw;
            float ey = (set.EndPixel - 0.5f) * pw;

            float ex = pictureBox1.Width - 1;
            using (Pen pen = new Pen(Color.Red, 1))
            {
                e.Graphics.DrawLine(pen, 1, sy, ex, sy);
                e.Graphics.DrawLine(pen, 1, ey, ex, ey);
            }

            float effectHeight = ey - sy;
            if (set.Trachea > 0)
            {
                float tw = effectHeight * 1.0f / set.Trachea;

                float x = 100f;
                float y = tw / 2 + sy;
                using (Pen pen = new Pen(Color.Orange, 2))
                {
                    for (int i = 0; i < set.Trachea; i++)
                    {
                        e.Graphics.DrawEllipse(pen, x, y, 2, 2);
                        y += tw;
                    }
                }
            }

        }
    }
}
