using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert.Tools
{
    public class Common
    {
        public static bool FloatArrayEqual(float[] value1, float[] value2)
        {
            if (value1 is null || value2 is null)
                return false;
            if (object.ReferenceEquals(value1, value2))
                return true;
            if (value1.Length != value2.Length)
                return false;
            for (int i = 0; i < value1.Length; i++)
            {
                if (value1[i] != value2[i])
                    return false;
            }
            return true;
        }

        public static string GenerateGuidFromString(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static string GenerateGuidFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;
            try
            {

                // 获取文件内容
                byte[] fileContent = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);

                using (var md5 = MD5.Create())
                {
                    // 创建一个 MemoryStream 用于存放文件名和文件内容
                    using (var ms = new MemoryStream())
                    {
                        // 写入文件名
                        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                        ms.Write(fileNameBytes, 0, fileNameBytes.Length);

                        // 写入文件内容
                        ms.Write(fileContent, 0, fileContent.Length);

                        // 计算 MD5 哈希
                        byte[] hashBytes = md5.ComputeHash(ms.ToArray());
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

        }



        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GlobalMemoryStatusEx(ref MEMORYINFO mi);

        //Define the information structure of memory
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYINFO
        {
            public uint dwLength; //当前结构尺寸
            public uint dwMemoryLoad; //当前内存利用率
            public ulong ullTotalPhys; //总物理内存大小
            public ulong ullAvailPhys; //可用物理内存大小
            public ulong ullTotalPageFile; //Exchange文件总大小
            public ulong ullAvailPageFile; //Exchange文件总大小
            public ulong ullTotalVirtual; //虚拟内存总大小
            public ulong ullAvailVirtual; //可用虚拟内存大小
            public ulong ullAvailExtendedVirtual;//保持此值始终为零
        }

        /// <summary>
        /// Get the current memory usage
        /// </summary>
        /// <returns></returns>
        public static MEMORYINFO GetMemoryStatus()
        {
            MEMORYINFO memoryInfo = new MEMORYINFO();
            memoryInfo.dwLength = (uint)Marshal.SizeOf(memoryInfo);
            GlobalMemoryStatusEx(ref memoryInfo);
            return memoryInfo;
        }

        /// <summary>
        /// 内存利用率
        /// </summary>
        /// <returns></returns>
        public static uint GetMemoryLoad()
        {
            return GetMemoryStatus().dwMemoryLoad;
        }

    }
}
