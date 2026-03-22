using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.Sorting.Entities
{
    public class ApplyInfo
    {
        /// <summary>
        /// 获取软件名称
        /// </summary>
        public string SoftwareName { get { return "SortingExpert"; } }

        /// <summary>
        /// 获取应用名称
        /// </summary>
        public string ApplyName { get { return "SortingExpert"; } }
        /// <summary>
        /// 平台Code
        /// </summary>
        public int PlatformCode { get { return 45; } }

        /// <summary>
        /// 构建版本
        /// </summary>
        public int BuildVersion { get { return 13; } }

        /// <summary>
        /// 版本日期
        /// </summary>
        public string VersionInfo { get { return " V3.2025-08-29.1"; } } 
        /// <summary>
        /// 升级包路径
        /// </summary>
        public string PackageUpgradePath { get { return string.Format(ApplyMyDocuments, "upgrade_package"); } }
        /// <summary>
        /// 我的文档路径
        /// </summary>
        public string ApplyMyDocuments { get => $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{ApplyName}\\{{0}}\\"; }

        public string MyDocumentsFile { get => $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{ApplyName}\\{{0}}\\{{1}}"; }
        /// <summary>
        /// 应用配置路径
        /// </summary>
        public string ApplySettingPath { get => $"{string.Format(ApplyMyDocuments, "setting")}"; }
        /// <summary>
        /// 应用参数路径
        /// </summary>
        public string ApplyParamPath { get => $"{string.Format(ApplyMyDocuments, "param")}"; }
        /// <summary>
        /// 应用测试数据的路径
        /// </summary>
        public string ApplyImgPath { get => $"{string.Format(ApplyMyDocuments, "Files")}"; }
        /// <summary>
        /// 模型应用预览图片的路径
        /// </summary>
        public string PreviewImgPath { get => $"{string.Format(ApplyMyDocuments, "Preview")}"; }
        /// <summary>
        /// SCAN模块缓存数据的路径 （采集自动保存）
        /// </summary>
        //public string ApplyScanImgPath { get => $"{string.Format(ApplyMyDocuments, "Scan")}"; }
    }
}
