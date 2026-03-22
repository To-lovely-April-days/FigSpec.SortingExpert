using FigSpec.SortingExpert.Enums;
using FigSpec.SortingExpert.ModelFiles;
using Hyperspectral.SpectralFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.AppCode
{
    public class NotificationAction
    {
        /// <summary>
        /// 浏览界面遍历刷新图片显示通知
        /// </summary>
        public static Action FolderPathOfNewImgChanged;
        /// <summary>
        /// 浏览图片路径变更通知
        /// </summary>
        public static Action FolderPathOfBrowseChanged;

        /// <summary>
        /// 发送到实时分选界面
        /// </summary>
        public static Action<Model> Send2Sorting;
        /// <summary>
        /// 发送到轮廓分选界面
        /// </summary>
        public static Action<Model> Send2OutlineSorting;
        /// <summary>
        /// 发送到训练界面
        /// </summary>
        public static Action<string, bool> Send2Train;

        /// <summary>
        /// 将模型从app页面发送到训练页面
        /// </summary>
        public static Action SendModel2Train;
        /// <summary>
        /// 发送相机的状态到界面 
        /// </summary>
        public static Action<NoticeForm> SendStatus2Form;


        public static Action<int, int, Dictionary<int, int>> SendEjectSetting;

        public static Action<byte[], long> SendEjectData;

        /// <summary>
        /// 刷新Scan界面帧频
        /// </summary>
        public static Action RefreshScanFrame;
    }


}
