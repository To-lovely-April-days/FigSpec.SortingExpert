using CHNSpec.Tools;
using FigSpec.SortingExpert.Entities;
using FigSpec.SortingExpert.ModelFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FigSpec.SortingExpert
{
    /// <summary>
    /// 应用的配置文件信息
    /// </summary>
    public class XApplySetting
    {
        public void Save()
        {
            string filePath = GlobalSettings.ApplyInfo.ApplySettingPath;

            //创建指定文件夹
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            filePath += "ApplySetting.xml";

            XmlHelper.ObjectToXMLFile(this, filePath, Encoding.UTF8);
        }
        /// <summary>
        /// 浏览界面的路径保存
        /// </summary>
        public string FolderPathOfBrowse { get; set; } = GlobalSettings.ApplyInfo.ApplyImgPath;
        /// <summary>
        /// 临时文件夹路径
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public string FolderPathOfBrowseTemp 
        {
            get 
            {
                string path = Path.Combine(FolderPathOfBrowse, "Temp");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        } 
        /// <summary>
        /// 文件夹历史记录
        /// </summary>
        public List<string> FolderPathOfBrowseRecords { get; set; }

        /// <summary>
        /// 训练配置
        /// </summary>
        public TraingSet traingSet { get; set; } = new TraingSet();
        /// <summary>
        /// 相机设置 scan
        /// </summary>
        public CameraSetting ScanCameraSetting { get; set; } = new CameraSetting();
        /// <summary>
        /// 相机设置 sorting
        /// </summary>
        public CameraSetting SortCameraSetting { get; set; } = new CameraSetting();
        /// <summary>
        /// scan模块设置
        /// </summary>
        public ScanSet scanSet { get; set; } = new ScanSet();

        /// <summary>
        /// 同一时间点只能有一个模块使用相机或GPU
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public string runingApp { get; set; } = string.Empty;
        /// <summary>
        /// 停止运行标志
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public bool stopAppFlag { get; set; } = false;


        public int StartSample { get; set; } = 1;

        public int EndSample { get; set; } = 640;


        public int StartSampleIndex => StartSample - 1;
        public int CustomSamples => EndSample - StartSample + 1;

        /// <summary>
        /// Scan的白校准文件过期时间
        /// </summary>
        public DateTime? CalibrationTimeWhite { get; set; }
        /// <summary>
        /// Scan的黑校准文件过期时间
        /// </summary>
        public DateTime? CalibrationTimeBlack { get; set; }

        /// <summary>
        /// 校准文件的有效时间间隔，小时
        /// </summary>
        public float CalibrationEffectiveHour { get; set; } = 4f;
        
        
        
        
        /// <summary>
        /// 启用TCP连接吹气模块
        /// </summary>
        [Obsolete("废弃")]
        public bool TcpConnectEnable { get; set; } = true;
        /// <summary>
        /// 需要连接的服务器的地址
        /// </summary>
        public string ServerIp { get; set; }
        /// <summary>
        /// 服务器端口
        /// </summary>
        public string ServerPort { get; set; }
        /// <summary>
        /// 通讯方式 0:TCP 1:COM, 2:csv 
        /// 后续添加的， 所以之前默认是使用 TcpConnectEnable
        /// </summary>
        public int CommunicationType { get; set; }

        public string CommunicationFileName { get; set; }

        public int FrameNumberPerFile { get; set; } = 1500;



        /// <summary>
        /// TCP连接吹气设备
        /// </summary>
        public bool EjectDeviceConnectTcp { get; set; }
        public string EjectDeviceIp { get; set; }
        public int EjectDevicePort { get; set; }
        /// <summary>
        /// 吹气COM口
        /// </summary>
        public string EjectDeviceCom { get; set; }
        /// <summary>
        /// 吹气延迟
        /// </summary>
        public uint EjectAirDelay { get; set; }
        /// <summary>
        /// 吹气时长
        /// </summary>
        public uint EjectAirTime { get; set; } = 50;
        /// <summary>
        /// 激活像素个数X
        /// </summary>
        public int ActivatePixelsX { get; set; } = 6;
        /// <summary>
        /// 激活像素个数Y
        /// </summary>
        public int ActivatePixelsY { get; set; } = 6;
        /// <summary>
        /// 气管配置
        /// </summary>
        public TracheaSet TracheaSet { get; set; } = new TracheaSet();
        /// <summary>
        /// 分选时计算的帧数
        /// </summary>
        private int _CalculatedFrameCount = 25;
        public int CalculatedFrameCount 
        {
            get => _CalculatedFrameCount;
            set 
            {
                if (value > 0 && value < 1000)
                {
                    _CalculatedFrameCount = value;
                }
            }
        }
        /// <summary>
        /// 删除超时数据
        /// </summary>
        //public bool DeleteOverTimeData = true;
        public XApplySetting()
        {
            ServerIp = "127.0.0.1";
            ServerPort = "5050";
            EjectDeviceCom = "COM1";
            EjectDeviceIp = "192.168.21.21";
            EjectDevicePort = 502;
        }
    }
}
